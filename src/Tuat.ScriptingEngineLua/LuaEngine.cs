using MoonSharp.Interpreter;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Tuat.Interfaces;
using Tuat.Models;
using static Tuat.Interfaces.IScriptEngine;

namespace Tuat.ScriptingEngineLua;

[DisplayName("Lua")]
[Editor("Tuat.ScriptingEngineLua.Editor", typeof(Editor))]
public class LuaEngine : IScriptEngine
{
    private Script? _engine;
    private IClientService? _clientService;
    private IDataService? _dataService;
    private IVariableService? _variableService;
    private IAutomationHandler? _automationHandler;
    private List<string> _systemMethods = [];
 
    static LuaEngine()
    {
        UserData.RegisterAssembly(Assembly.GetAssembly(typeof(LuaEngine)));
    }

    public void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript, List<AutomationInputVariable>? inputValues = null, int? topAutomationId = null)
    {
        _clientService = clientService;
        _dataService = dataService;
        _variableService = variableService;
        _automationHandler = automationHandler;
        _engine = new Script();
        var systemMethods = new SystemMethods(_engine, _clientService, _dataService, _variableService, _automationHandler, topAutomationId);
        _engine.Options.DebugPrint = s => { systemMethods.log(instanceId.ToString(), s); };
        _engine.Globals["system"] = systemMethods;
        var systemScript = new StringBuilder();
        systemScript.AppendLine(GetSystemScript(_clientService, instanceId, subAutomationParameters: automationHandler.Automation.SubAutomationParameters, inputValues: inputValues));
        if (!string.IsNullOrWhiteSpace(additionalScript))
        {
            systemScript.AppendLine(additionalScript);
        }
         _engine.DoString(systemScript.ToString());
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
                valueText = "nil";
            }
            else if (variable.Value is string)
            {
                valueText = $"\"{variable.Value}\"";
            }
            else if (variable.Value is bool b)
            {
                valueText = $"{(b ? "true" : "false")}";
            }
            else
            {
                valueText = variable.Value.ToString()!.Replace(",",".");
            }
            Execute($"{variable.Name} = {valueText}");
        }
    }

    public List<IScriptEngine.ScriptVariable> GetScriptVariables()
    {
        return _engine?.Globals.Pairs
          .Where(x => x.Value.Type != DataType.ClrFunction 
            && x.Value.Type != DataType.Function 
            && x.Value.Type != DataType.UserData 
            && x.Value.Type != DataType.Table 
            && x.Key.ToString() != "\"_VERSION\"")
          .Select(x => new IScriptEngine.ScriptVariable(x.Key.String, x.Value.ToObject()))
          .ToList() ?? [];
    }

    public void CallVoidFunction(string functionName, List<IScriptEngine.FunctionParameter>? functionParameters = null)
    {
        if (functionParameters?.Any()!=true)
        {
            _engine!.Call(_engine.Globals[functionName]);
        }
        else
        {
            _engine!.Call(_engine.Globals[functionName], functionParameters.Select(x => x.Value).ToArray());
        }
    }

    public T CallFunction<T>(string functionName, List<FunctionParameter>? functionParameters = null)
    {
        if (functionParameters?.Any() != true)
        {
            var result = _engine!.Call(_engine.Globals[functionName]);
            return (T)result.ToObject();
        }
        else
        {
            var result = _engine!.Call(_engine.Globals[functionName], functionParameters.Select(x => x.Value).ToArray());
            return (T)result.ToObject();
        }
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
        _engine = null;
    }

    public string GetDeclareFunction(string functionName, IScriptEngine.FunctionReturnValue? returnValue = null, List<IScriptEngine.FunctionParameter>? functionParameters = null, string? body = null)
    {
        _systemMethods.Add(functionName);
        var result = new StringBuilder();
        result.Append($"function {functionName}(");
        if (functionParameters?.Any() == true)
        {
            string.Join(", ", functionParameters.Select(p => p.Name));
        }
        result.AppendLine(")");
        if (body != null)
        {
            result.AppendLine(body);
        }
        result.AppendLine("end");
        return result.ToString();
    }

    public string GetSystemScript(IClientService clientService, Guid? instanceId = null, string? additionalScript = null, List<SubAutomationParameter>? subAutomationParameters = null, List<AutomationInputVariable>? inputValues = null)
    {
        var script = new StringBuilder();
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
                script.AppendLine("--===================================================");
                script.AppendLine($"-- client helper methods for {client.Client.Name}");
                script.AppendLine("--===================================================");

                foreach (var method in createVariableMethods)
                {
                    script.AppendLine($"""--{method.description}""");
                    script.AppendLine($"""--{method.example}""");
                    script.AppendLine($"""function {method.methodName}(name, data, mockingOptions, clientId)""");
                    script.AppendLine($"  if (clientId == nil) then");
                    script.AppendLine($"    clientId = {client.Client.Id}");
                    script.AppendLine($"  end");
                    script.AppendLine($"   return createVariableOnClient(name, clientId, {(method.isAutomationVariable ? "true" : "false")},  {(method.persistant ? "true" : "false")}, data, mockingOptions)");
                    script.AppendLine("end");
                }

                foreach (var method in createExecuteMethods)
                {
                    script.AppendLine($"""--{method.description}""");
                    script.AppendLine($"""--{method.example}""");
                    script.AppendLine($"""function {method.methodName}(variableId, parameter1, parameter2, parameter3, clientId)""");
                    script.AppendLine($"  if (clientId == nil) then");
                    script.AppendLine($"    clientId = {client.Client.Id}");
                    script.AppendLine($"  end");
                    script.AppendLine($"   return executeOnClient(clientId, variableId, \"{method.command}\", parameter1, parameter2, parameter3)");
                    script.AppendLine("end");
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
                    script.AppendLine($"{p.ScriptVariableName} = nil");
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
}
