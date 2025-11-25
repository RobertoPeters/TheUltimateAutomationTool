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
    Dictionary<string, (IScriptEngine.FunctionReturnValue? returnValue, List<IScriptEngine.FunctionParameter>? functionParameters)> scriptMethodProtoTypes = new();
    Dictionary<string, PyObject> _pythonFunc = new();

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
            foreach (var scriptMethodProtoType in scriptMethodProtoTypes)
            {
                var pyFunc = _engine.Get(scriptMethodProtoType.Key);
                _pythonFunc.Add(scriptMethodProtoType.Key, pyFunc);
            }
        }
    }

    public string GetReturnTrueStatement()
    {
        return "return True";
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
        //using (Py.GIL()) 
        //{ 
        //     var result = _engine!.Variables()
        //        .Keys()
        //        .Where(k => k.As<string>() != "_systemMethods")
        //        .Select(k => 
        //        { 
        //            var name = k.As<string>(); 
        //            using var pyVal = _engine.Get(name); 
        //            object? managed = pyVal.IsNone() ? null : pyVal.As<object>(); 
        //            return new IScriptEngine.ScriptVariable(name, managed); 
        //        }).ToList();

        //    return result; //result.Where(x => x.Name != null).ToList();
        //} 
    }

    public void CallVoidFunction(string functionName, List<IScriptEngine.FunctionParameter>? functionParameters = null)
    {
        using (Py.GIL())
        {
            if (functionParameters?.Any() != true)
            {
                using (var pyresult = _pythonFunc[functionName].Invoke());
            }
            else
            {
                using (var pyresult = _pythonFunc[functionName].Invoke(functionParameters.Select(x => x.Value == null ? PyObject.None : PyObject.FromManagedObject(x.Value)).ToArray())) ;
            }
        }
    }

    public T CallFunction<T>(string functionName, List<FunctionParameter>? functionParameters = null)
    {
        using (Py.GIL())
        {
            T result = default;
            if (functionParameters?.Any() != true)
            {
                using (var pyresult = _pythonFunc[functionName].Invoke())
                {
                     result = pyresult.As<T>();
                }
            }
            else
            {
                using (var pyresult = _pythonFunc[functionName].Invoke(functionParameters.Select(x => x.Value == null ? PyObject.None : PyObject.FromManagedObject(x.Value)).ToArray()))
                {
                      result = pyresult.As<T>();
                }
            }
            return result;
        }
    }

    public void Execute(string script)
    {
        using (Py.GIL())
        {
            _engine!.Exec(script);
        }
    }

    public object? Evaluate(string script)
    {
        using (Py.GIL())
        {
            return _engine!.Exec(script)?.As<object>();
        }
    }

    public void Dispose()
    {
        using (Py.GIL())
        {
            foreach (var pyObj in _pythonFunc.Values)
            {
                pyObj.Dispose();
            }
            _engine?.Dispose();
        }
        _pythonFunc.Clear();
        _engine = null;
    }

    public string GetDeclareFunction(string functionName, IScriptEngine.FunctionReturnValue? returnValue = null, List<IScriptEngine.FunctionParameter>? functionParameters = null, string? body = null)
    {
        scriptMethodProtoTypes.Add(functionName, (returnValue, functionParameters));

        var result = new StringBuilder();
        result.Append($"def {functionName}(");
        if (functionParameters?.Any() == true)
        {
            string.Join(", ", functionParameters.Select(p => p.Name));
        }
        result.AppendLine("):");
        if (!string.IsNullOrWhiteSpace(body))
        {
            var lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string indent = "";
            if (!lines[0].StartsWith(" "))
            {
                indent = "    ";
            }
            foreach (var line in lines)
            {
                result.AppendLine($"{indent}{line}");
            }
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

        var clients = clientService.GetClients();
        List<string> clientTypesHandled = [];
        foreach (var client in clients)
        {
            if (clientTypesHandled.Contains(client.Client.ClientType))
            {
                continue;
            }

            var createVariableMethods = client.CreateVariableOnClientMethods();
            var createExecuteMethods = client.CreateExecuteOnClientMethods();
            if (createVariableMethods.Any() || createExecuteMethods.Any())
            {
                script.AppendLine("#===================================================");
                script.AppendLine($"# client helper methods for {client.Client.Name}");
                script.AppendLine("#===================================================");

                foreach (var method in createVariableMethods)
                {
                    script.AppendLine($"""#{method.description}""");
                    script.AppendLine($"""#{method.example}""");
                    script.AppendLine($"""def {method.methodName}(name, data=None, mockingOptions=None, clientId=None):""");
                    script.AppendLine($"  if clientId == None:");
                    script.AppendLine($"     clientId = {client.Client.Id}");
                    script.AppendLine($"  return createVariableOnClient(name, clientId, {(method.isAutomationVariable ? "True" : "False")},  {(method.persistant ? "True" : "False")}, data, mockingOptions)");
                    script.AppendLine();
                }

                foreach (var method in createExecuteMethods)
                {
                    script.AppendLine($"""#{method.description}""");
                    script.AppendLine($"""#{method.example}""");
                    script.AppendLine($"""def {method.methodName}(variableId=None, parameter1=None, parameter2=None, parameter3=None, clientId=None):""");
                    script.AppendLine($"  if clientId == None:");
                    script.AppendLine($"     clientId = {client.Client.Id}");
                    script.AppendLine($"  return executeOnClient(clientId, variableId, '{method.command}', parameter1, parameter2, parameter3)");
                    script.AppendLine();
                }
            }
        }

        script.AppendLine();
        subAutomationParameters?.ForEach(p =>
        {
            var inputVar = inputValues?.FirstOrDefault(x => string.Compare(x.Name, p.Name, true) == 0);
            if (inputVar != null)
            {
                if (inputVar.Value == null)
                {
                    script.AppendLine($"{p.ScriptVariableName} = null");
                }
                else if (inputVar.Value is string v)
                {
                    script.AppendLine($"{p.ScriptVariableName} = \"{inputVar.Value}\"");
                }
                else
                {
                    script.AppendLine($"{p.ScriptVariableName} = {inputVar.Value!.ToString()?.Replace(",", ".")}");
                }
            }
            else
            {
                script.AppendLine($"{p.ScriptVariableName} = {p.DefaultValue}");
            }
        });

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
