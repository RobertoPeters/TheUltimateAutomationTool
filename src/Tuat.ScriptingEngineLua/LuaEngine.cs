using Lua;
using Lua.Standard;
using System.ComponentModel;
using System.Text;
using Tuat.Interfaces;
using Tuat.Models;
using static Tuat.Interfaces.IScriptEngine;

namespace Tuat.ScriptingEngineLua;

[DisplayName("Lua")]
[Editor("Tuat.ScriptingEngineLua.Editor", typeof(Editor))]
public class LuaEngine : IScriptEngine
{
    private Lua.LuaState? _engine;
    private IClientService? _clientService;
    private IDataService? _dataService;
    private IVariableService? _variableService;
    private IAutomationHandler? _automationHandler;
    private List<string> _systemMethods = [];
    private LuaValue[]? _engineState;
 
    public void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript, List<AutomationInputVariable>? inputValues = null, int? topAutomationId = null)
    {
        _clientService = clientService;
        _dataService = dataService;
        _variableService = variableService;
        _automationHandler = automationHandler;
        _engine = LuaState.Create();
        _engine.OpenStandardLibraries();
        var systemMethods = new SystemMethods(_engine, _clientService, _dataService, _variableService, _automationHandler, topAutomationId);
        var systemScript = new StringBuilder();
        systemScript.AppendLine(GetSystemScript(_clientService, instanceId, subAutomationParameters: automationHandler.Automation.SubAutomationParameters, inputValues: inputValues));
        if (!string.IsNullOrWhiteSpace(additionalScript))
        {
            systemScript.AppendLine(additionalScript);
        }
        systemScript.Append("return ");
        systemScript.AppendLine(string.Join(",", _systemMethods));
        _engineState = _engine.DoStringAsync(systemScript.ToString()).Result;
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
            else
            {
                valueText = variable.Value.ToString()!.Replace(",",".");
            }
            Execute($"{variable.Name} = {valueText}");
        }
    }

    public List<IScriptEngine.ScriptVariable> GetScriptVariables()
    {
         return _engine?.Environment.Skip(35)
            .Where(x => x.Value.GetType() != typeof(LuaFunction) && !x.Value.ToString().StartsWith("function: "))
            .Select(x => new IScriptEngine.ScriptVariable(x.Key.ToString(), x.Value.ToObject()))
            .ToList() ?? [];
    }

    public void CallVoidFunction(string functionName, List<IScriptEngine.FunctionParameter>? functionParameters = null)
    {
        var index = _systemMethods.IndexOf(functionName);
        var func = _engineState![index].Read<LuaFunction>();
        //func.InvokeAsync(_engineState, []);
        _engine!.CallAsync(func, []);
    }

    public T CallFunction<T>(string functionName, List<FunctionParameter>? functionParameters = null)
    {
        var index = _systemMethods.IndexOf(functionName);
        var func = _engineState![index].Read<LuaFunction>();
        //func.InvokeAsync(_engineState, []);
        var luaValue = _engine!.CallAsync(func, []).Result[0];
        return luaValue.Read<T>();
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
        _systemMethods.Add(functionName);
        var result = new StringBuilder();
        result.Append($"local function {functionName}(");
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

        return script.ToString();
    }
}
