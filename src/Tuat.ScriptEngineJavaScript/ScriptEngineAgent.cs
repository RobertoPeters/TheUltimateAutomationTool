using Tuat.Interfaces;

namespace Tuat.ScriptEngineJavaScript;

public class ScriptEngineAgent : IAgentSetting
{
    public string Id => "Tuat.ScriptEngineJavaScript.ScriptEngineAgent";

    public string Name => "JavaScript";

    public string Description => "JavaScript TUAT related functions";

    public Type? ClientType => null;

    public Type? ScriptEngineType => typeof(JavaScriptEngine);

    public Type? AutomationType => null;

    public string GetInstructions(IScriptEngine scriptEngine, IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, Guid? instanceId = null, string? additionalScript = null, List<AutomationInputVariable>? inputValues = null, int? topAutomationId = null)
    {
        return "todo";
    }
}
