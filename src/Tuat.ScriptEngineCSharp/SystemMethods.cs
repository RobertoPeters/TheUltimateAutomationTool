using System.Collections.Concurrent;
using System.Text;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.ScriptEngineCSharp;

public class SystemMethods
{
    private readonly IVariableService _variableService;
    private readonly IAutomationHandler _automationHandler;
    private readonly IClientService _clientService;
    private readonly ConcurrentDictionary<int, Client> _clients;

    public record DateTimeInfo(int year, int month, int day, int hour, int minute, int second, int dayOfWeek);

    public SystemMethods(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler)
    {

        _variableService = variableService;
        _automationHandler = automationHandler;
        _clientService = clientService;
        _clients = new ConcurrentDictionary<int, Client>(dataService.GetClients().ToDictionary(c => c.Id));
    }

    public void Log(string instanceId, object? message)
    {
        _automationHandler.AddLogAsync(instanceId, message).Wait();
    }

    public DateTimeInfo GetCurrentDateTime()
    {
        var now = DateTime.Now;
        return new DateTimeInfo(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, (int)now.DayOfWeek);
    }

    public bool CurrentTimeBetween(string startTime, string endTime, bool includeBoundary)
    {
        var currentTime = DateTime.Now.TimeOfDay;
        return currentTime.TimeOfDayBetween(startTime, endTime, includeBoundary);
    }

    public int CreateVariable(string name, int clientId, bool isAutomationVariable, bool persistant, object? data, object[]? mockingOptions)
    {
        List<string>? stringMockingOptions = null;
        if (mockingOptions?.Any() == true)
        {
            stringMockingOptions = [];
            foreach (var mockingOption in mockingOptions)
            {
                stringMockingOptions.Add(mockingOption?.ToString() ?? "");
            }
        }
        return _variableService.CreateVariableAsync(name, clientId, isAutomationVariable ? _automationHandler.Automation.Id : null, persistant, data?.ToString(), stringMockingOptions).Result ?? -1;
    }

    public int? GetVariableIdByName(string name, int clientId, bool isStateMachineVariable)
    {
        var variable = _variableService
                .GetVariables()
                .FirstOrDefault(v => v.Variable.Name == name
                                    && v.Variable.ClientId == clientId
                                    && (isStateMachineVariable && v.Variable.AutomationId == _automationHandler.Automation.Id || !isStateMachineVariable && v.Variable.AutomationId == null));
        return variable?.Variable.Id;
    }

    public bool SetVariableValue(int variableId, string? value)
    {
        return _variableService.SetVariableValuesAsync([(variableId, value)]).Result;
    }

    public string? GetVariableValue(int variableId)
    {
        return _variableService.GetVariable(variableId)?.Value;
    }

    public bool IsMockingVariableActive(int variableId)
    {
        return _variableService.GetVariable(variableId)?.IsMocking ?? false;
    }

    public int GetClientId(string name)
    {
        var client = _clients.Values.FirstOrDefault(c => c.Name == name);
        return client?.Id ?? -1;
    }

    public bool ClientExecute(int clientId, int? variableId, string command, object? parameter1, object? parameter2, object? parameter3)
    {
        return _clientService.ExecuteAsync(clientId, variableId, command, parameter1, parameter2, parameter3).Result;
    }

    public void SetAutomationFinished(List<AutomationOutputVariable> outputValues)
    {
         _automationHandler.SetAutomationFinished(outputValues);
    }
    public void StartSubAutomation(int automationId, List<AutomationInputVariable> inputValues)
    {
        _automationHandler.StartSubAutomation(automationId, inputValues);
    }

    public bool IsSubAutomationRunning()
    {
        return _automationHandler.IsSubAutomationRunning();
    }
    public static string SystemScript()
    {
        var script = new StringBuilder();
        script.AppendLine(SystemScriptGeneric);
        return script.ToString();
    }

    private readonly static string SystemScriptGeneric = $$""""

    //====================================================================================
    // SYSTEM METHODS
    //====================================================================================
    
    void Log (object? message) 
    {
        _systemMethods.Log(instanceId, message);
    }

    //returns the current (local) date and time as an object with year, month, day, hour, minute, second and dayOfWeek properties
    Tuat.ScriptEngineCSharp.SystemMethods.DateTimeInfo GetCurrentDateTime()
    {
        return _systemMethods.GetCurrentDateTime();
    }

    bool CurrentTimeBetween(string startTime, string endTime, bool includeBoundary) 
    {
        //e.g. currentTimeBetween('8:00', '18:00', true)
        // currentTimeBetween('22:00', '4:00', true)
        // currentTimeBetween('22:00', '0:30', true)
        //it used the 24h clock format without leading zeros
        //returns true if the current time is between startTime and endTime (inclusive or exclusive depending on includeBoundary)
        return _systemMethods.CurrentTimeBetween(startTime, endTime, includeBoundary);
    }
    
    // returns the client id or -1 if not found
    int GetClientId(string name) 
    {
        return _systemMethods.GetClientId(name);
    }

    //execute specific client commands (false if it fails)
    //e.g. executeOnClient(clientIdOfHomeAssistant, null, 'callservice', 'light', 'turn_on', { "entity_id": "light.my_light", "brightness_pct": 20})
    bool ExecuteOnClient(int clientId, int? variableId, string command, object? parameter1, object? parameter2, object? parameter3) 
    {
        return _systemMethods.ClientExecute(clientId, variableId, command, parameter1, parameter2, parameter3);
    }

    // creates a variable and returns the variable id (-1 if it fails)
    // e.g. createVariable('test', clientId, true, true, 'initialValue', ['option1', 'option2'])
    int CreateVariableOnClient(string name, int clientId, bool isAutomationVariable, bool persistant, object? data, object[]? mockingOptions) 
    {
        return _systemMethods.CreateVariable(name, clientId, isAutomationVariable, persistant, data, mockingOptions);
    }

    // returns the variable value
    string? GetVariableValue(int variableId) 
    {
        return _systemMethods.GetVariableValue(variableId);
    }

    // sets the variable value (returns true if successful, false otherwise)
    bool SetVariableValue(int variableId, string? variableValue) 
    {
        return _systemMethods.SetVariableValue(variableId, variableValue);
    }

    // get the variable Id by name
    int? GetVariableIdByName(string name, int clientId, bool isAutomationVariable) 
    {
        return _systemMethods.GetVariableIdByName(name, clientId, isAutomationVariable);
    }

    bool IsMockingVariableActive(int variableId) 
    {
        return _systemMethods.IsMockingVariableActive(variableId);
    }
    
    void SetAutomationFinished(List<AutomationOutputVariable> outputValues)
    {
        _systemMethods.SetAutomationFinished(outputValues);
    }

    void StartSubAutomation(int automationId, List<AutomationInputVariable> inputValues)
    {
        _systemMethods.StartSubAutomation(automationId, inputValues);
    }

    bool IsSubAutomationRunning()
    {
        return _systemMethods.IsSubAutomationRunning();
    }

    """";
}

