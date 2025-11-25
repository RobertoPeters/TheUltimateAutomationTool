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
        _automationHandler.AddLogAsync(instanceId, message);
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
    

 
    
    """";
}

