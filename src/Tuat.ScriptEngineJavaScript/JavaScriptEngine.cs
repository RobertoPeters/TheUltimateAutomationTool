using System.ComponentModel;
using Tuat.Interfaces;

namespace Tuat.ScriptEngineJavaScript;

[DisplayName("JavaScript")]
[Editor("Tuat.ScriptEngineJavaScript.Editor", typeof(Editor))]
public class JavaScriptEngine : IScriptEngine
{
    public void Initialize(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler)
    {
    }

    public void CallVoidFunction(string functionName, List<IScriptEngine.FunctionParameter>? functionParameters = null)
    {
    }

    public void Execute(string script)
    {
    }

    public object? Evaluate(string script)
    {
        return null;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public string GetDeclareFunction(string functionName, bool hasReturnValue, Type? returnValueType = null, List<IScriptEngine.FunctionParameter>? functionParameters = null)
    {
        throw new NotImplementedException();
    }
}
