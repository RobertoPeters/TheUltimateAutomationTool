using System.Collections.Concurrent;
using System.Text;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.ScriptEngineCSharp;

internal class SystemMethods
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

    public void log(string instanceId, object? message)
    {
        _automationHandler.AddLogAsync(instanceId, message).Wait();
    }

    public DateTimeInfo getCurrentDateTime()
    {
        var now = DateTime.Now;
        return new DateTimeInfo(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, (int)now.DayOfWeek);
    }

    public bool currentTimeBetween(string startTime, string endTime, bool includeBoundary)
    {
        var currentTime = DateTime.Now.TimeOfDay;
        return currentTime.TimeOfDayBetween(startTime, endTime, includeBoundary);
    }

    public int createVariable(string name, int clientId, bool isAutomationVariable, bool persistant, object? data, object[]? mockingOptions)
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

    public int? getVariableIdByName(string name, int clientId, bool isStateMachineVariable)
    {
        var variable = _variableService
                .GetVariables()
                .FirstOrDefault(v => v.Variable.Name == name
                                    && v.Variable.ClientId == clientId
                                    && (isStateMachineVariable && v.Variable.AutomationId == _automationHandler.Automation.Id || !isStateMachineVariable && v.Variable.AutomationId == null));
        return variable?.Variable.Id;
    }

    public bool setVariableValue(int variableId, string? value)
    {
        return _variableService.SetVariableValuesAsync([(variableId, value)]).Result;
    }

    public string? getVariableValue(int variableId)
    {
        return _variableService.GetVariable(variableId)?.Value;
    }

    public bool isMockingVariableActive(int variableId)
    {
        return _variableService.GetVariable(variableId)?.IsMocking ?? false;
    }

    public int getClientId(string name)
    {
        var client = _clients.Values.FirstOrDefault(c => c.Name == name);
        return client?.Id ?? -1;
    }

    public bool clientExecute(int clientId, int? variableId, string command, object? parameter1, object? parameter2, object? parameter3)
    {
        return _clientService.ExecuteAsync(clientId, variableId, command, parameter1, parameter2, parameter3).Result;
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
    
    void log (object? message) 
    {
        system.log(instanceId, message);
    }

    //returns the current (local) date and time as an object with year, month, day, hour, minute, second and dayOfWeek properties
    SystemMethods.DateTimeInfo getCurrentDateTime
    {
        return system.getCurrentDateTime();
    }

    bool currentTimeBetween(string startTime, string endTime, bool includeBoundary) 
    {
        //e.g. currentTimeBetween('8:00', '18:00', true)
        // currentTimeBetween('22:00', '4:00', true)
        // currentTimeBetween('22:00', '0:30', true)
        //it used the 24h clock format without leading zeros
        //returns true if the current time is between startTime and endTime (inclusive or exclusive depending on includeBoundary)
        return system.currentTimeBetween(startTime, endTime, includeBoundary);
    }
    
    // returns the client id or -1 if not found
    int getClientId(string name) 
    {
        return system.getClientId(name)
    }

    //execute specific client commands (false if it fails)
    //e.g. executeOnClient(clientIdOfHomeAssistant, null, 'callservice', 'light', 'turn_on', { "entity_id": "light.my_light", "brightness_pct": 20})
    bool executeOnClient(int clientId, int? variableId, string command, object? parameter1, object? parameter2, object? parameter3) {
        return system.clientExecute(clientId, variableId, command, parameter1, parameter2, parameter3)
    }

    // creates a variable and returns the variable id (-1 if it fails)
    // e.g. createVariable('test', clientId, true, true, 'initialValue', ['option1', 'option2'])
    int createVariableOnClient(string name, int clientId, bool isAutomationVariable, bool persistant, object? data, object[]? mockingOptions) 
    {
        return system.createVariable(name, clientId, isAutomationVariable, persistant, data, mockingOptions)
    }

    // returns the variable value
    string? getVariableValue(int variableId) 
    {
        return system.getVariableValue(variableId)
    }

    // sets the variable value (returns true if successful, false otherwise)
    bool setVariableValue(int variableId, string? variableValue) 
    {
        return system.setVariableValue(variableId, variableValue)
    }

    //get the variable Id by name
    int getVariableIdByName(string name, int clientId, bool,isAutomationVariable) 
    {
        return system.getVariableIdByName(name, clientId, isAutomationVariable)
    }

    bool isMockingVariableActive(int variableId) 
    {
        return system.isMockingVariableActive(variableId)
    }    
    """";
}

