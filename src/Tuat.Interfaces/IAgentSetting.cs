namespace Tuat.Interfaces;

public interface IAgentSetting
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    Type? ClientType { get; }
    Type? ScriptEngineType { get; }
    Type? AutomationType { get; }

    string? GetInstructions(IClientService clientService, IDataService dataService, IVariableService variableService, IAutomationHandler? automationHandler, Guid? instanceId = null, string? systemCode = null, string? userCode = null, List<AutomationInputVariable>? inputValues = null, int? topAutomationId = null);
}
