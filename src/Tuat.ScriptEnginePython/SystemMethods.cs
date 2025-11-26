using Python.Runtime;
using System.Collections.Concurrent;
using System.Text;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.ScriptEnginePython;

public class SystemMethods
{
    private readonly IVariableService _variableService;
    private readonly IAutomationHandler _automationHandler;
    private readonly IClientService _clientService;
    private readonly IDataService _dataService;
    private readonly ConcurrentDictionary<int, Client> _clients;
    private readonly int? _topAutomationId;

    public record DateTimeInfo(int year, int month, int day, int hour, int minute, int second, int dayOfWeek);

    public SystemMethods(PyModule engine, IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, int? topAutomationId)
    {

        _variableService = variableService;
        _automationHandler = automationHandler;
        _clientService = clientService;
        _dataService = dataService;
        _clients = new ConcurrentDictionary<int, Client>(dataService.GetClients().ToDictionary(c => c.Id));
        _topAutomationId = topAutomationId;
    }

    public void Log(string instanceId, object? message)
    {
        _automationHandler.AddLogAsync(instanceId, message.FromPyObject());
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
                stringMockingOptions.Add(mockingOption?.FromPyObject()?.ToString() ?? "");
            }
        }

        return _variableService.CreateVariableAsync(name, clientId, isAutomationVariable ? (_topAutomationId ?? _automationHandler.Automation.Id) : null, persistant, data?.FromPyObject()?.ToString(), stringMockingOptions).Result ?? -1;
    }

    public int? GetVariableIdByName(string name, int clientId, bool isStateMachineVariable)
    {
        var variable = _variableService
                .GetVariables()
                .FirstOrDefault(v => v.Variable.Name == name
                                    && v.Variable.ClientId == clientId
                                    && (isStateMachineVariable && v.Variable.AutomationId == (_topAutomationId ?? _automationHandler.Automation.Id) || !isStateMachineVariable && v.Variable.AutomationId == null));
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

    public int GetAutomationId(string name)
    {
        var automation = _dataService.GetAutomations().FirstOrDefault(c => c.Name == name);
        return automation?.Id ?? -1;
    }

    public bool ClientExecute(int clientId, int? variableId, string command, object? parameter1, object? parameter2, object? parameter3)
    {
        return _clientService.ExecuteAsync(clientId, variableId, command, parameter1.FromPyObject(), parameter2.FromPyObject(), parameter3.FromPyObject()).Result;
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

    public void StopSubAutomation()
    {
        _automationHandler.StopSubAutomation();
    }

    public static string SystemScript()
    {
        var script = new StringBuilder();
        script.AppendLine(SystemScriptGeneric);
        return script.ToString();
    }

    private readonly static string SystemScriptGeneric = $$""""

    #====================================================================================
    # SYSTEM METHODS
    #====================================================================================
    
    def log(message):
       _systemMethods.Log(instanceId, message)
    
    def currentTimeBetween(startTime, endTime, includeBoundary=True):
        #e.g. currentTimeBetween("8:00", "18:00", true)
        # currentTimeBetween("22:00", "4:00", True)
        # currentTimeBetween("22:00", "0:30", True)
        #it used the 24h clock format without leading zeros
        #returns true if the current time is between startTime and endTime (inclusive or exclusive depending on includeBoundary)
        return _systemMethods.CurrentTimeBetween(startTime, endTime, includeBoundary)

    def getClientId(name):
        return _systemMethods.GetClientId(name)
    
    # returns the automation id or -1 if not found
    def getAutomationId(name):
        return _systemMethods.GetAutomationId(name)
    
    #execute specific client commands (false if it fails)
    #e.g. executeOnClient(clientIdOfHomeAssistant, null, "callservice", "light", "turn_on", { "entity_id": "light.my_light", "brightness_pct": 20})
    def executeOnClient(clientId, variableId, command, parameter1=None, parameter2=None, parameter3=None):
        return _systemMethods.ClientExecute(clientId, variableId, command, parameter1, parameter2, parameter3)
    
    # creates a variable and returns the variable id (-1 if it fails)
    # e.g. createVariable('test', clientId, True, True, 'initialValue', ["option1", "option2"])
    def createVariableOnClient(name, clientId, isAutomationVariable=True, persistant=False, data=None, mockingOptions=None):
        return _systemMethods.CreateVariable(name, clientId, isAutomationVariable, persistant, data, mockingOptions)
    
    # returns the variable value
    def getVariableValue(variableId):
        return _systemMethods.GetVariableValue(variableId)
    
    # sets the variable value (returns true if successful, false otherwise)
    def setVariableValue(variableId, variableValue):
        return _systemMethods.SetVariableValue(variableId, variableValue)
    
    # get the variable Id by name
    def getVariableIdByName(name, clientId, isAutomationVariable=True):
        return _systemMethods.GetVariableIdByName(name, clientId, isAutomationVariable)
    
    def isMockingVariableActive(variableId):
        return _systemMethods.IsMockingVariableActive(variableId)
    
    def setAutomationFinished(outputValues=None):
        _systemMethods.SetAutomationFinished(outputValues)
    
    def startSubAutomation(automationId, inputValues=None):
        _systemMethods.StartSubAutomation(automationId, inputValues)
    
    def isSubAutomationRunning():
        return _systemMethods.IsSubAutomationRunning()
    
    # stop the running sub-automation
    def stopSubAutomation():
        _systemMethods.StopSubAutomation()
 
    
    """";
}

