using Acornima.Ast;
using Jint;
using Radzen.Blazor.Markdown;
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

    public void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid instanceId, string? additionalScript)
    {
        _clientService = clientService;
        _dataService = dataService;
        _variableService = variableService;
        _automationHandler = automationHandler;
        _engine = new();
        var systemMethods = new SystemMethods(_clientService, _dataService, _variableService, _automationHandler);
        _engine.SetValue("system", systemMethods);
        _engine.Execute(GetSystemScript(_clientService));
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

    public string GetDeclareFunction(string functionName, bool hasReturnValue, Type? returnValueType = null, List<IScriptEngine.FunctionParameter>? functionParameters = null, string? body = null)
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
