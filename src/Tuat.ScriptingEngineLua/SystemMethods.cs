using MoonSharp.Interpreter;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Text;
using Tuat.Extensions;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.ScriptingEngineLua;

[MoonSharpUserData]
internal class SystemMethods
{
    private readonly IVariableService _variableService;
    private readonly IAutomationHandler _automationHandler;
    private readonly IClientService _clientService;
    private readonly IDataService _dataService;
    private readonly ConcurrentDictionary<int, Client> _clients;
    private readonly int? _topAutomationId;

    [MoonSharpUserData]
    public record DateTimeInfo(int year, int month, int day, int hour, int minute, int second, int dayOfWeek);

    public SystemMethods(Script engine, IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler automationHandler, int? topAutomationId)
    {
        _variableService = variableService;
        _automationHandler = automationHandler;
        _clientService = clientService;
        _dataService = dataService;
        _clients = new ConcurrentDictionary<int, Client>(dataService.GetClients().ToDictionary(c => c.Id));
        _topAutomationId = topAutomationId;
    }

    public void log(string instanceId, string message)
    {
        _automationHandler.AddLogAsync(instanceId, message);
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
                stringMockingOptions.Add(mockingOption.ToString()!);
            }
        }
        return _variableService.CreateVariableAsync(name, clientId, isAutomationVariable ? (_topAutomationId ?? _automationHandler.Automation.Id) : null, persistant, data?.ToString(), stringMockingOptions).Result ?? -1;
    }

    public int? getVariableIdByName(string name, int clientId, bool isStateMachineVariable)
    {
        var variable = _variableService
                .GetVariables()
                .FirstOrDefault(v => v.Variable.Name == name
                                    && v.Variable.ClientId == clientId
                                    && (isStateMachineVariable && v.Variable.AutomationId == (_topAutomationId ?? _automationHandler.Automation.Id) || !isStateMachineVariable && v.Variable.AutomationId == null));
        return variable?.Variable.Id;
    }

    public bool setVariableValue(int variableId, string? value)
    {
        return _variableService.SetVariableValuesAsync([(variableId, value)]).Result;
    }

    public object? getVariableValue(int variableId)
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

    public int getAutomationId(string name)
    {
        var automation = _dataService.GetAutomations().FirstOrDefault(c => c.Name == name);
        return automation?.Id ?? -1;
    }

    public bool clientExecute(int clientId, int? variableId, string command, object? parameter1, object? parameter2, object? parameter3)
    {
        return _clientService.ExecuteAsync(clientId, variableId, command, parameter1, parameter2, parameter3).Result;
    }

    public void setAutomationFinished(object? scriptOutputValues)
    {
        List<AutomationOutputVariable> outputValues = [];
        if (scriptOutputValues != null)
        {
            var array = ((MoonSharp.Interpreter.Table)scriptOutputValues).Pairs;
            foreach (var outputValue in array)
            {
                var name = outputValue.Key.String;
                var value = outputValue.Value.ToObject();
                outputValues.Add(new AutomationOutputVariable { Name = name, Value = value });
            }
        }
        _automationHandler.SetAutomationFinished(outputValues);
    }
    public void startSubAutomation(int automationId, object? scriptInputValues)
    {
        List<AutomationInputVariable> inputValues = [];
        if (scriptInputValues != null)
        {
            var array = ((MoonSharp.Interpreter.Table)scriptInputValues).Pairs;
            foreach (var inputValue in array)
            {
                var name = inputValue.Key.String;
                var value = inputValue.Value.ToObject();
                inputValues.Add(new AutomationInputVariable { Name = name, Value = value });
            }
        }
        _automationHandler.StartSubAutomation(automationId, inputValues);
    }

    public bool isSubAutomationRunning()
    {
        return _automationHandler.IsSubAutomationRunning();
    }

    public void stopSubAutomation()
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
   
    --====================================================================================
    -- SYSTEM METHODS
    --====================================================================================
    
    function log(message)
       system.log(instanceId, message)
    end

    
    --returns the current (local) date and time as an object with year, month, day, hour, minute, second and dayOfWeek properties
    function getCurrentDateTime()
        return system.getCurrentDateTime()
    end

    function currentTimeBetween(startTime, endTime, includeBoundary)
        --e.g. currentTimeBetween('8:00', '18:00', true)
        -- currentTimeBetween('22:00', '4:00', true)
        -- currentTimeBetween('22:00', '0:30', true)
        --it used the 24h clock format without leading zeros
        --returns true if the current time is between startTime and endTime (inclusive or exclusive depending on includeBoundary)
        return system.currentTimeBetween(startTime, endTime, includeBoundary)
    end
    
    -- returns the client id or -1 if not found
    function getClientId(name)
        return system.getClientId(name)
    end

    -- returns the automation id or -1 if not found
    function getAutomationId(name) 
        return system.getAutomationId(name)
    end

    --execute specific client commands (-1 if it fails)
    --e.g. executeOnClient(clientIdOfHomeAssistant, null, "callservice", "light", "turn_on", { "entity_id": "light.my_light", "brightness_pct": 20})
    function executeOnClient(clientId, variableId, command, parameter1, parameter2, parameter3)
        return system.clientExecute(clientId, variableId, command, parameter1, parameter2, parameter3)
    end

    -- creates a variable and returns the variable id (-1 if it fails)
    -- e.g. createVariable('test', clientId, true, true, "initialValue", {"option1", "option2"})
    function createVariableOnClient(name, clientId, isAutomationVariable, persistant, data, mockingOptions)
        return system.createVariable(name, clientId, isAutomationVariable, persistant, data, mockingOptions)
    end

    -- returns the variable value
    function getVariableValue(variableId)
        return system.getVariableValue(variableId)
    end

    -- sets the variable value (returns true if successful, false otherwise)
    function setVariableValue(variableId, variableValue)
        return system.setVariableValue(variableId, variableValue)
    end

    --get the variable Id by name
    function getVariableIdByName(name, clientId, isAutomationVariable)
        return system.getVariableIdByName(name, clientId, isAutomationVariable)
    end

    function isMockingVariableActive(variableId) 
        return system.isMockingVariableActive(variableId)
    end
    
    -- start a sub automation and provide input values (Table of { "name": value })
    function startSubAutomation(automationId, scriptInputValues)
        system.startSubAutomation(automationId, scriptInputValues)
    end

    -- check if the started sub-automation is still running
    function isSubAutomationRunning()
        return system.isSubAutomationRunning()
    end

    -- set the automation as finished and provide output values (Table of { "name": value })
    function setAutomationFinished(outputValues)
        return system.setAutomationFinished(outputValues)
    end
    
    -- stop the running sub-automation
    function stopSubAutomation()
        return system.stopSubAutomation()
    end
    
    """";
}

