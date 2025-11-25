using Python.Runtime;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Tuat.Interfaces;
using Tuat.Models;
using static Tuat.Interfaces.IScriptEngine;

namespace Tuat.ScriptEnginePython;

[DisplayName("Python")]
[Editor("Tuat.ScriptEnginePython.Editor", typeof(Editor))]
public class PythonScriptEngine : IScriptEngine
{
    private PyModule? _engine;
    private IClientService? _clientService;
    private IDataService? _dataService;
    private IVariableService? _variableService;
    private IAutomationHandler? _automationHandler;

    static PythonScriptEngine()
    {
        if (!InitializeForLinux())
        {
            InitializeForWindows();
        }
    }

    public void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript, List<AutomationInputVariable>? inputValues = null, int? topAutomationId = null)
    {
        _clientService = clientService;
        _dataService = dataService;
        _variableService = variableService;
        _automationHandler = automationHandler;
        using (Py.GIL())
        {
            _engine = Py.CreateScope();
            var systemMethods = new SystemMethods(_engine, _clientService, _dataService, _variableService, _automationHandler, topAutomationId);
            var systemScript = new StringBuilder();
            systemScript.AppendLine(GetSystemScript(_clientService, instanceId, subAutomationParameters: automationHandler.Automation.SubAutomationParameters, inputValues: inputValues));
            if (!string.IsNullOrWhiteSpace(additionalScript))
            {
                systemScript.AppendLine(additionalScript);
            }
            _engine.Set("_systemMethods", systemMethods);
            _engine.Exec(systemScript.ToString());
        }
    }

    public string GetReturnTrueStatement()
    {
        return "return true";
    }

    public void HandleSubAutomationOutputVariables(List<AutomationOutputVariable> outputVariables)
    {
        foreach (var variable in outputVariables)
        {
            string valueText = "";
            if (variable.Value == null)
            {
                valueText = "null";
            }
            else if (variable.Value is string)
            {
                valueText = $"\"{variable.Value}\"";
            }
            else
            {
                valueText = variable.Value.ToString()!.Replace(",", ".");
            }
            Execute($"{variable.Name} = {valueText}");
        }
    }

    public List<IScriptEngine.ScriptVariable> GetScriptVariables()
    {
        return [];
    }

    public void CallVoidFunction(string functionName, List<IScriptEngine.FunctionParameter>? functionParameters = null)
    {
        using (Py.GIL())
        {
            if (functionParameters?.Any() != true)
            {
                _engine!.Exec($"{functionName}()");
            }
            else
            {
                //_engine!.Call(_engine.Globals[functionName], functionParameters.Select(x => x.Value).ToArray());
            }
        }
    }

    public T CallFunction<T>(string functionName, List<FunctionParameter>? functionParameters = null)
    {
        return default;
        //if (functionParameters?.Any() != true)
        //{
        //    var result = _engine!.Call(_engine.Globals[functionName]);
        //    return (T)result.ToObject();
        //}
        //else
        //{
        //    var result = _engine!.Call(_engine.Globals[functionName], functionParameters.Select(x => x.Value).ToArray());
        //    return (T)result.ToObject();
        //}
    }

    public void Execute(string script)
    {
        throw new NotSupportedException();
    }

    public object? Evaluate(string script)
    {
        throw new NotSupportedException();
    }

    public void Dispose()
    {
        _engine?.Dispose();
        _engine = null;
    }

    public string GetDeclareFunction(string functionName, IScriptEngine.FunctionReturnValue? returnValue = null, List<IScriptEngine.FunctionParameter>? functionParameters = null, string? body = null)
    {
        var result = new StringBuilder();
        result.Append($"def {functionName}(");
        if (functionParameters?.Any() == true)
        {
            string.Join(", ", functionParameters.Select(p => p.Name));
        }
        result.AppendLine("):");
        if (body != null)
        {
            //result.AppendLine(body);
        }
        return result.ToString();
    }

    public string GetSystemScript(IClientService clientService, Guid? instanceId = null, string? additionalScript = null, List<SubAutomationParameter>? subAutomationParameters = null, List<AutomationInputVariable>? inputValues = null)
    {
        var script = new StringBuilder();
        script.AppendLine($"import clr");
        script.AppendLine($"instanceId = \"{(instanceId ?? Guid.Empty).ToString()}\"");
        script.AppendLine();

        script.AppendLine();
        script.AppendLine(SystemMethods.SystemScript());
        script.AppendLine();


        return script.ToString();
    }

    private static bool InitializeForLinux()
    {
        const string pythonLibName = "python3";
        var searchPaths = System.Environment
                .GetEnvironmentVariable("PATH")?
                .Split(";", StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.Contains("Python3", StringComparison.OrdinalIgnoreCase))
                .ToList() ?? [];
        string libPythonPath = "";
        foreach (var path in searchPaths.Where(x => Directory.Exists(x)))
        {
            var foundLib = Directory.GetFiles(path, $"{pythonLibName}*.dll")
                                        .OrderByDescending(f => f) // Get latest version
                                        .FirstOrDefault();

            if (foundLib != null)
            {
                libPythonPath = foundLib;
                break;
            }
        }

        if (string.IsNullOrEmpty(libPythonPath))
        {
            return false;
        }

        Console.WriteLine($"Initializing Python.NET with library: {libPythonPath}");
        Runtime.PythonDLL = libPythonPath;
        PythonEngine.Initialize();
        PythonEngine.BeginAllowThreads();
        using (Py.GIL())
        {
            dynamic sys = Py.Import("sys");
            Console.WriteLine($"Python Version: {sys.version}");
        }
        return true;
    }

    private static bool InitializeForWindows()
    {
        const string pythonLibName = "libpython3";
        var searchPaths = new[] { "/usr/lib/x86_64-linux-gnu", "/usr/lib", "/usr/local/lib" };
        string libPythonPath = "";
        foreach (var path in searchPaths.Where(x => Directory.Exists(x)))
        {
            var foundLib = Directory.GetFiles(path, $"{pythonLibName}.*.so*")
                                    .OrderByDescending(f => f) // Get latest version
                                    .FirstOrDefault();

            if (foundLib != null)
            {
                libPythonPath = foundLib;
                break;
            }
        }

        if (string.IsNullOrEmpty(libPythonPath))
        {
            return false;
        }

        Console.WriteLine($"Initializing Python.NET with library: {libPythonPath}");
        Runtime.PythonDLL = libPythonPath;
        PythonEngine.Initialize();
        PythonEngine.BeginAllowThreads();
        using (Py.GIL())
        {
            dynamic sys = Py.Import("sys");
            Console.WriteLine($"Python Version: {sys.version}");
        }
        return true;
    }
}
