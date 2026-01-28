using Tuat.Interfaces;

namespace Tuat.ScriptEngineJavaScript;

public class ScriptEngineSystemCodeAgent : IAgentSetting
{
    public string Id => "Tuat.ScriptEngineJavaScript.ScriptEngineSystemCodeAgent";

    public string Name => "JavaScript fixed application specific code";

    public string Description => "JavaScript fixed application specific code. These are application javascript functions.";

    public Type? ClientType => null;

    public Type? ScriptEngineType => typeof(JavaScriptEngine);

    public Type? AutomationType => null;

    public string? GetInstructions(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler? automationHandler, Guid? instanceId = null, string? systemCode = null, string? userCode = null, List<AutomationInputVariable>? inputValues = null, int? topAutomationId = null)
    {
        if (string.IsNullOrWhiteSpace(systemCode))
        {
            return null;
        }

        return $""""
            You are an expert in application specific standard JavaScript functions. 
            The scripting language is javascript.
            You do not run in a browser.
            The javascript parser is done by the nuget package Jint.
            Only answer questions about the following fixed system code
            Use the following system code as a reference for your answers:

            {systemCode}
            """";
    }
}

public class ScriptEngineUserCodeAgent : IAgentSetting
{
    public string Id => "Tuat.ScriptEngineJavaScript.ScriptEngineUserCodeAgent";

    public string Name => "JavaScript user code";

    public string Description => "JavaScript changable user code. This code is created by the user.";

    public Type? ClientType => null;

    public Type? ScriptEngineType => typeof(JavaScriptEngine);

    public Type? AutomationType => null;

    public string? GetInstructions(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler? automationHandler, Guid? instanceId = null, string? systemCode = null, string? userCode = null, List<AutomationInputVariable>? inputValues = null, int? topAutomationId = null)
    {
        if (string.IsNullOrWhiteSpace(userCode))
        {
            return null;
        }

        return $""""
            You are an expert in application specific user created JavaScript code. 
            The scripting language is javascript.
            You do not run in a browser.
            The javascript parser is done by the nuget package Jint.
            Only answer questions about the following user code
            The following code is user code and is changable:
            
            
            {userCode}
            """";
    }
}
