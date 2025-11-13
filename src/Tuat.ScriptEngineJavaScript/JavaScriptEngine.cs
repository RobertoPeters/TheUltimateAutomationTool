using System.ComponentModel;
using System.Text;
using Tuat.Interfaces;

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

    public void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript)
    {
        _clientService = clientService;
        _dataService = dataService;
        _variableService = variableService;
        _automationHandler = automationHandler;
        _engine = new();
        startOfCustomVariableIndex = _engine.Global.GetOwnProperties().Count() + 2;
        var systemMethods = new SystemMethods(_clientService, _dataService, _variableService, _automationHandler);
        _engine.SetValue("system", systemMethods);
        _engine.Execute(GetSystemScript(_clientService, instanceId));
        if (!string.IsNullOrWhiteSpace(additionalScript))
        {
            _engine.Execute(additionalScript);
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
        var result = new StringBuilder();
        result.Append($"{functionName}(");
        if (functionParameters?.Any() == true)
        {
            string.Join(", ", functionParameters.Select(p => p.Name));
        }
        result.AppendLine(")");
        _engine?.Execute(result.ToString());
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
            string.Join(", ", functionParameters.Select(p => p.Name));
        }
        result.AppendLine("){");
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
        script.AppendLine("var global = this");
        script.AppendLine($"var instanceId = '{(instanceId ?? Guid.Empty).ToString()}'");
        script.AppendLine();

        script.AppendLine();
        script.AppendLine(SystemMethods.SystemScript());
        script.AppendLine();

        return script.ToString();
    }
}
