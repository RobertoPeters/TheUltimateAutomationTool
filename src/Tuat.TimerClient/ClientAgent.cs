using Tuat.Interfaces;

namespace Tuat.TimerClient;

public class ClientAgent : IAgentSetting
{
    public string Id => "Tuat.TimerClient.ClientAgent";

    public string Name => "Script code for timers";

    public string Description => "Script code for timers";

    public Type? ClientType => typeof(TimerClientHandler);

    public Type? ScriptEngineType => null;

    public Type? AutomationType => null;

    public string? GetInstructions(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler? automationHandler, Guid? instanceId = null, string? systemCode = null, string? userCode = null, List<AutomationInputVariable>? inputValues = null, int? topAutomationId = null)
    {
        if (string.IsNullOrWhiteSpace(systemCode))
        {
            return null;
        }

        return $""""
            You are an expert in timer functions. 
            You do not run in a browser.
            Only answer questions about timers and how to use them
            Use the following system code as a reference for your answers:

            The function 'timerId = createTimer(nameOfTimer, defaultTimeoutInSeconds)' is used to create a timer. This function only creates the timer, but does not start it. It returns a 'timerId' that is used to reference the timer in other functions.
            The function 'startTimer(timerId)' is used to start a timer that was created with the 'createTimer' function.
            You can also start a timer with a different timeout value using the function 'startTimer(timerId, timeoutInSeconds)'. This will start the timer with the specified timeout value instead of the default timeout value.
            The function 'stopTimer(timerId)' is used to stop a running timer.

            To check if a timer is expired, you have to check the variable value. Use 'getVariableValue(timerId)' to tget the timer value. If this is '0', the timer is expired.
            """";
    }
}
