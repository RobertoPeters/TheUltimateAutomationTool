using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
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

    public void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript)
    {
        _clientService = clientService;
        _dataService = dataService;
        _variableService = variableService;
        _automationHandler = automationHandler;
        var systemMethods = new SystemMethods(_clientService, _dataService, _variableService, _automationHandler);
        var systemScrypt = GetSystemScript(_clientService, instanceId);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(systemScrypt);
        string assemblyName = $"CSharp{instanceId.ToString("N")}.dll";
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll"))
        };
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)); // Create a DLL
        string outputPath = Path.Combine("Settings", assemblyName);
        EmitResult result = compilation.Emit(outputPath);

        //_engine.SetValue("system", systemMethods);
        //_engine.Execute(GetSystemScript(_clientService));
    }

    public void CallVoidFunction(string functionName, List<IScriptEngine.FunctionParameter>? functionParameters = null)
    {
        var result = new StringBuilder();
        result.Append($"{functionName}(");
        if (functionParameters?.Any() == true)
        {
            string.Join(", ", functionParameters.Select(p => p.Name));
        }
        result.AppendLine(")");
        //_engine?.Execute(result.ToString());
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
        //_engine?.Dispose();
        //_engine = null;
    }

    public string GetDeclareFunction(string functionName, bool hasReturnValue, Type? returnValueType = null, List<IScriptEngine.FunctionParameter>? functionParameters = null, string? body = null)
    {
        var result = new StringBuilder();
        result.AppendLine("internal partial class ScriptContainer {");
        result.Append($"public void {functionName}("); //todo return type
        //todo
        result.AppendLine("){");
        if (body != null)
        {
            result.AppendLine(body);
        }
        result.AppendLine("}");
        return result.ToString();
    }

    public string GetSystemScript(IClientService clientService, Guid? instanceId = null)
    {
        var script = new StringBuilder();
        script.AppendLine("namespace Tuat.ScriptEngineCSharp;");
        script.AppendLine("internal partial class ScriptContainer {");
        script.AppendLine($"""var instanceId = Guid.Parse("{(instanceId ?? Guid.Empty).ToString()}");""");
        script.AppendLine();

        script.AppendLine();
        script.AppendLine(SystemMethods.SystemScript());
        script.AppendLine();
        script.AppendLine("}");

        return script.ToString();
    }
}
