using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.ComponentModel;
using System.Text;
using Tuat.Interfaces;

namespace Tuat.ScriptEngineCSharp;

[DisplayName("CSharp")]
[Editor("Tuat.ScriptEngineCSharp.Editor", typeof(Editor))]
public class CSharpEngine : IScriptEngine
{
    private IClientService? _clientService;
    private IDataService? _dataService;
    private IVariableService? _variableService;
    private IAutomationHandler? _automationHandler;

    private Type? _engineType;
    private object? _engine;
    private CollectibleAssemblyLoadContext? _context;
    private WeakReference? _weakRef;
    private bool _isInitialized = false;

    public void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript)
    {
        _isInitialized = true;

        _clientService = clientService;
        _dataService = dataService;
        _variableService = variableService;
        _automationHandler = automationHandler;

        TryRemoveAssemblies();

        var settingsPath = GetScriptAssemblyDirectory();
        var systemMethods = new SystemMethods(_clientService, _dataService, _variableService, _automationHandler);
        var systemScript = GetSystemScript(_clientService, instanceId, additionalScript);
        var scriptUniqueNamespace = $"TuatScript{Guid.NewGuid().ToString("N")}";
        var scriptUniqueTypeName = $"ScriptContainer{Guid.NewGuid().ToString("N")}";
        systemScript = systemScript.Replace("ScriptContainer", scriptUniqueTypeName).Replace("TuatScript", scriptUniqueNamespace);

        var syntaxTree = CSharpSyntaxTree.ParseText(systemScript);
        var assemblyName = $"{ScriptAssemblyNamePrefix}{instanceId.ToString("N")}.dll";
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath!, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(typeof(IScriptEngine).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(SystemMethods).Assembly.Location),
        };
        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)); // Create a DLL
        var assemblyFilePath = Path.Combine(settingsPath!, assemblyName);
        var result = compilation.Emit(assemblyFilePath);
        if (!result.Success)
        {
            TryRemoveAssemblies();

            var errorText = new StringBuilder();
            result.Diagnostics.Where(x => x.WarningLevel == (int)DiagnosticSeverity.Error).ToList().ForEach(diagnostic =>
            {
                errorText.AppendLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
            });
            throw new Exception($"Script compilation failed: {errorText}");
        }

        _context = new CollectibleAssemblyLoadContext();
        _context.Unloading += _ => Console.WriteLine($"[Script] ALC unloading for {assemblyName}");
        using var memoryStream = new MemoryStream(System.IO.File.ReadAllBytes(assemblyFilePath));
        var dynamicAssembly = _context.LoadFromStream(memoryStream);
        _weakRef = new WeakReference(_context, true);
        _engineType = dynamicAssembly.GetType($"{scriptUniqueNamespace}.{scriptUniqueTypeName}")!;
        _engine = Activator.CreateInstance(_engineType, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new object[] { systemMethods, instanceId.ToString() }, null);
        TryRemoveAssemblies();
    }

    private static string GetScriptAssemblyDirectory()
    {
        var result = Path.GetDirectoryName(typeof(CSharpEngine).Assembly.Location);
        while (result != null)
        {
            if (Directory.Exists(Path.Combine(result, "Settings")))
            {
                break;
            }
            result = Directory.GetParent(result)?.FullName;
        }
        result = Path.Combine(result!, "Settings");
        return result;
    }

    private string ScriptAssemblyNamePrefix => $"CSharp_{_automationHandler?.Automation.Id}_";

    private void TryRemoveAssemblies()
    {
        var settingsPath = GetScriptAssemblyDirectory();
        var files = Directory.GetFiles(settingsPath, $"{ScriptAssemblyNamePrefix}*.dll");
        foreach (var file in files)
        {
            try
            {
                System.IO.File.Delete(file);
            }
            catch
            {
                //nothing
            }
        }
    }

    public void CallVoidFunction(string functionName, List<IScriptEngine.FunctionParameter>? functionParameters = null)
    {
        var method = _engineType!.GetMethod(functionName);
        method.Invoke(_engine, functionParameters?.Select(p => p.Value).ToArray()); //todo
    }

    public void Execute(string script)
    {
        //_engine?.Execute(script);
    }

    public object? Evaluate(string script)
    {
        return null;
        //return _engine?.Evaluate(script)?.ToObject();
    }

    public void Dispose()
    {
        if (!_isInitialized)
        {
            return;
        }

        _isInitialized = false;

        try
        {
            // Call script-side cleanup if it exposes a method (pattern-based)
            var disposable = _engine as IDisposable;
            disposable?.Dispose();
        }
        catch { /* swallow script errors during cleanup */ }

        _engine = null;
        _engineType = null;

        var localContext = _context;
        _context = null;

        localContext?.Unload();

#pragma warning disable S1215 // "GC.Collect" should not be called
        GC.Collect();
        for (int i = 0; _weakRef?.IsAlive == true && (i < 10); i++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }
#pragma warning restore S1215 // "GC.Collect" should not be called
        _weakRef = null;
        TryRemoveAssemblies();
    }

    public string GetDeclareFunction(string functionName, bool hasReturnValue, Type? returnValueType = null, List<IScriptEngine.FunctionParameter>? functionParameters = null, string? body = null)
    {
        var result = new StringBuilder();
        result.Append($"public void {functionName}("); //todo return type
        //todo
        result.AppendLine(")");
        result.AppendLine("{");
        if (body != null)
        {
            result.AppendLine(body);
        }
        result.AppendLine("}");
        return result.ToString();
    }

    public string GetSystemScript(IClientService clientService, Guid? instanceId = null, string? additionalScript = null)
    {
        var script = new StringBuilder();
        script.AppendLine($"using System;");
        script.AppendLine($"namespace TuatScript;");
        script.AppendLine("public class ScriptContainer(Tuat.ScriptEngineCSharp.SystemMethods _systemMethods, string instanceId): IDisposable");
        script.AppendLine("{");
        script.AppendLine();
        script.AppendLine(""""
            public void Dispose()
            {
                _systemMethods = null;
            }
            """");

        script.AppendLine();
        script.AppendLine(SystemMethods.SystemScript());
        script.AppendLine();
        if (!string.IsNullOrEmpty(additionalScript))
        {
            script.AppendLine(additionalScript);
            script.AppendLine();
        }
        script.AppendLine("}");

        return script.ToString();
    }
}
