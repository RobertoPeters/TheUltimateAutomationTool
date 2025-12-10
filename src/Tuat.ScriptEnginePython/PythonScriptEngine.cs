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
    private static bool PythonInitialzed = false;
    private static string PythonNotSupportedMessage = "Python environment could not be initialized. Make sure Python is installed on the system.";

    private PyModule? _engine;
    private IClientService? _clientService;
    private IDataService? _dataService;
    private IVariableService? _variableService;
    private IAutomationHandler? _automationHandler;
    Dictionary<string, (IScriptEngine.FunctionReturnValue? returnValue, List<IScriptEngine.FunctionParameter>? functionParameters)> scriptMethodProtoTypes = new();
    Dictionary<string, PyObject> _pythonFunc = new();

    static PythonScriptEngine()
    {
        PythonInitialzed = InitializeForLinux();
        if (!PythonInitialzed)
        {
            PythonInitialzed = InitializeForWindows();
        }
    }

    public void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript, List<AutomationInputVariable>? inputValues = null, int? topAutomationId = null)
    {
        if (!PythonInitialzed) throw new NotSupportedException(PythonNotSupportedMessage);
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
        if (!PythonInitialzed) throw new NotSupportedException(PythonNotSupportedMessage);
        using (Py.GIL())
        {
            foreach (var variable in outputVariables)
            {
                _engine!.Set(variable.Name, variable.Value);
            }
        }
    }

    public List<IScriptEngine.ScriptVariable> GetScriptVariables() 
    {
        if (!PythonInitialzed) throw new NotSupportedException(PythonNotSupportedMessage);
        List<IScriptEngine.ScriptVariable> result = [];
        using (Py.GIL())
        {
            using var variables = _engine!.Variables();
            foreach (var variable in variables)
            {
                var name = variable.ToString() ?? "";
                using var val = _engine.Get(name);
                using var pyType = val.GetPythonType();
                if (pyType.Name == "str")
                {
                    result.Add(new ScriptVariable(name, val.As<string>()));
                }
                else if (pyType.Name == "int")
                {
                    result.Add(new ScriptVariable(name, val.As<long>()));
                }
                else if (pyType.Name == "float")
                {
                    result.Add(new ScriptVariable(name, val.As<double>()));
                }
                else if (pyType.Name == "bool")
                {
                    result.Add(new ScriptVariable(name, val.As<bool>()));
                }
            }
        }
        return result;
    }

    public void CallVoidFunction(string functionName, List<IScriptEngine.FunctionParameter>? functionParameters = null)
    {
        if (!PythonInitialzed) throw new NotSupportedException(PythonNotSupportedMessage);
        using (Py.GIL())
        {
            if (functionParameters?.Any() != true)
            {
                using (_pythonFunc[functionName].Invoke()) { }
            }
            else
            {
                using (_pythonFunc[functionName].Invoke(functionParameters.Select(x => x.Value == null ? PyObject.None : PyObject.FromManagedObject(x.Value)).ToArray())) { }
            }
        }
    }

    public T CallFunction<T>(string functionName, List<FunctionParameter>? functionParameters = null)
    {
        if (!PythonInitialzed) throw new NotSupportedException(PythonNotSupportedMessage);
        using (Py.GIL())
        {
            T result;
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
        if (!PythonInitialzed) throw new NotSupportedException(PythonNotSupportedMessage);
        using (Py.GIL())
        {
            _engine!.Exec(script);
        }
    }

    public object? Evaluate(string script)
    {
        if (!PythonInitialzed) throw new NotSupportedException(PythonNotSupportedMessage);
        using (Py.GIL())
        {
            try
            {
                var result = _engine!.Eval<object>(script);
                return result.FromPyObject();
            }
            catch
            {
                Execute(script);
                return "";
            }
        }
    }

    public void Dispose()
    {
        if (PythonInitialzed)
        {
            try
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
            catch
            {
                //ignore
            }
        }
    }

    public string GetDeclareFunction(string functionName, IScriptEngine.FunctionReturnValue? returnValue = null, List<IScriptEngine.FunctionParameter>? functionParameters = null, string? body = null)
    {
        if (!PythonInitialzed) throw new NotSupportedException(PythonNotSupportedMessage);
        scriptMethodProtoTypes.Add(functionName, (returnValue, functionParameters));

        var result = new StringBuilder();
        result.Append($"def {functionName}(");
        if (functionParameters?.Any() == true)
        {
            result.Append(string.Join(", ", functionParameters.Select(p => p.Name)));
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

        if (!string.IsNullOrWhiteSpace(additionalScript))
        {
            script.AppendLine();
            script.AppendLine(additionalScript);
            script.AppendLine();

        }

        return script.ToString();
    }

    private static bool InitializeForWindows()
    {
        const string pythonLibName = "python3";
        var searchPaths = System.Environment
                .GetEnvironmentVariable("PATH")?
                .Split(";", StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.Contains("Python", StringComparison.OrdinalIgnoreCase))
                .ToList() ?? [];
        searchPaths = searchPaths.Select(x =>
        {
            if (x.EndsWith(@"Python\Launcher\"))
            {
                return x.Replace(@"Python\Launcher\", @"Python\");
            }
            else
            {
                return x;
            }
        }).ToList();
        string libPythonPath = "";
        foreach (var path in searchPaths.Where(x => Directory.Exists(x)))
        {
            var foundLib = SearchFolder(path);

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
        try
        {
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                Console.WriteLine($"Python Version: {sys.version}");
            }
        }
        catch
        {
            return false;
        }
        return true;

        string? SearchFolder(string folder)
        {
            var foundLib = Directory.GetFiles(folder, $"{pythonLibName}*.dll")
                             .OrderByDescending(f => f) // Get latest version
                             .FirstOrDefault();

            if (foundLib != null)
            {
                return foundLib;
            }

            var subFolders = Directory.GetDirectories(folder);
            foreach (var subFolder in subFolders)
            {
                foundLib = SearchFolder(subFolder);
                if (foundLib != null)
                {
                    return foundLib;
                }
            }

            return null;
        }
    }

    private static bool InitializeForLinux()
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
        try
        {
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                Console.WriteLine($"Python Version: {sys.version}");
            }
        }
        catch
        {
            return false;
        }
        return true;
    }
}
