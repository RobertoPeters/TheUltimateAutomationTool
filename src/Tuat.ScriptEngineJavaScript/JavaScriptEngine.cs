using Jint.Native;
using System.ComponentModel;
using System.Text;
using Tuat.Interfaces;
using Tuat.Models;
using static Tuat.Interfaces.IScriptEngine;

namespace Tuat.ScriptEngineJavaScript;

[DisplayName("JavaScript")]
[Editor("Tuat.ScriptEngineJavaScript.Editor", typeof(Editor))]
public class JavaScriptEngine : IScriptEngine
{
    private Jint.Engine? _engine;
    private IClientService? _clientService;
    private IDataService? _dataService;
    private IVariableService? _variableService;
    private IAutomationHandler? _automationHandler;
    private int startOfCustomVariableIndex;

    public void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript, List<AutomationInputVariable>? inputValues = null, int? topAutomationId = null)
    {
        _clientService = clientService;
        _dataService = dataService;
        _variableService = variableService;
        _automationHandler = automationHandler;
        _engine = new();
        startOfCustomVariableIndex = _engine.Global.GetOwnProperties().Count() + 2;
        var systemMethods = new SystemMethods(_clientService, _dataService, _variableService, _automationHandler, topAutomationId);
        _engine.SetValue("system", systemMethods);
        _engine.Execute(GetSystemScript(_clientService, instanceId, subAutomationParameters: automationHandler.Automation.SubAutomationParameters, inputValues: inputValues));
        if (!string.IsNullOrWhiteSpace(additionalScript))
        {
            _engine.Execute(additionalScript);
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
            var scriptVariable = GetScriptVariables().FirstOrDefault(x => x.Name == variable.Name);
            string valueText = "";
            if (variable.Value == null)
            {
                valueText = "null";
            }
            else if (variable.Value is string)
            {
                valueText = $"'{variable.Value}'";
            }
            else if (variable.Value is bool b)
            {
                valueText = $"{(b ? "true" : "false")}";
            }
            else
            {
                valueText = variable.Value.ToString()!.Replace(",",".");
            }
            if (scriptVariable != null)
            {
                Execute($"{variable.Name} = {valueText}");
            }
            else
            {
                Execute($"var {variable.Name} = {valueText}");
            }
        }
    }

    public List<IScriptEngine.ScriptVariable> GetScriptVariables()
    {
        return _engine?.Global.GetOwnProperties().Skip(startOfCustomVariableIndex)
            .Where(x => x.Value.Value.ToObject()?.ToString()?.StartsWith("System.Func") != true)
            .Select(x => new IScriptEngine.ScriptVariable( x.Key.ToString(), x.Value.Value.ToObject()))
            .ToList() ?? [];
    }

    public void CallVoidFunction(string functionName, List<IScriptEngine.FunctionParameter>? functionParameters = null)
    {
        var func = _engine!.GetValue(functionName);
        _engine.Invoke(func, functionParameters?.Select(p => JsValue.FromObject(_engine, p.Value)).ToArray() ?? []);
    }

    public T CallFunction<T>(string functionName, List<FunctionParameter>? functionParameters = null)
    {
        var func = _engine!.GetValue(functionName);
        var jsResult = _engine.Invoke(func, functionParameters?.Select(p => JsValue.FromObject(_engine, p.Value)).ToArray() ?? []);
        return (T?)jsResult.ToObject()!;
    }

    public void Execute(string script)
    {
        _engine?.Execute(script);
    }

    public object? Evaluate(string script)
    {
        return _engine?.Evaluate(script)?.ToObject();
    }

    public void Dispose()
    {
        _engine?.Dispose();
        _engine = null;
    }

    public string GetDeclareFunction(string functionName, IScriptEngine.FunctionReturnValue? returnValue = null, List<IScriptEngine.FunctionParameter>? functionParameters = null, string? body = null)
    {
        var result = new StringBuilder();
        result.Append($"function {functionName}(");
        if (functionParameters?.Any() == true)
        {
            result.Append(string.Join(", ", functionParameters.Select(p => p.Name)));
        }
        result.AppendLine("){");
        if (body != null)
        {
            result.AppendLine(body);
        }
        result.AppendLine("}");
        return result.ToString();
    }

    public string GetSystemScript(IClientService clientService, Guid? instanceId = null, string? additionalScript = null, List<SubAutomationParameter>? subAutomationParameters = null, List<AutomationInputVariable>? inputValues = null)
    {
        var script = new StringBuilder();
        script.AppendLine("var global = this");
        script.AppendLine($"var instanceId = '{(instanceId ?? Guid.Empty).ToString()}'");
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
                script.AppendLine("//===================================================");
                script.AppendLine($"// client helper methods for {client.Client.Name}");
                script.AppendLine("//===================================================");

                foreach (var method in createVariableMethods)
                {
                    script.AppendLine($"""//{method.description}""");
                    script.AppendLine($"""//{method.example}""");
                    script.AppendLine($"""{method.methodName} = function(name, data, mockingOptions, clientId)""");
                    script.AppendLine("{");
                    script.AppendLine($"  if (clientId == null) {{ clientId = {client.Client.Id} }}");
                    script.AppendLine($"   return createVariableOnClient(name, clientId, {(method.isAutomationVariable ? "true" : "false")},  {(method.persistant ? "true" : "false")}, data, mockingOptions)");
                    script.AppendLine("}");
                }

                foreach (var method in createExecuteMethods)
                {
                    script.AppendLine($"""//{method.description}""");
                    script.AppendLine($"""//{method.example}""");
                    script.AppendLine($"""{method.methodName} = function(variableId, parameter1, parameter2, parameter3, clientId)""");
                    script.AppendLine("{");
                    script.AppendLine($"  if (clientId == null) {{ clientId = {client.Client.Id} }}");
                    script.AppendLine($"   return executeOnClient(clientId, variableId, '{method.command}', parameter1, parameter2, parameter3)");
                    script.AppendLine("}");
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
                    script.AppendLine($"var {p.ScriptVariableName} = null");
                }
                else if (inputVar.Value is string v)
                {
                    script.AppendLine($"var {p.ScriptVariableName} = '{inputVar.Value}'");
                }
                else
                {
                    script.AppendLine($"var {p.ScriptVariableName} = {inputVar.Value!.ToString()?.Replace(",", ".")}");
                }
            }
            else
            {
                script.AppendLine($"var {p.ScriptVariableName} = {p.DefaultValue}");
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
}
