using System.ComponentModel;
using Tuat.Extensions;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.VariableValue;

[DisplayName("Variable (create)")]
[Editor("Tuat.FlowAutomation.StepTypes.VariableValue.StepSettings", typeof(StepSettings))]
public class StepVariableValue: Step
{
    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public const string VariableNameKey = "VariableName";
    public const string ClientNameKey = "ClientName";
    public const string IsGlobalVariableKey = "IsGlobalVariable";
    public const string IsPersistantKey = "IsPersistant";
    public const string DataKey = "Data";
    public const string MockingValuesKey = "MockingValues";
    public const string PayloadOnStartKey = "PayloadOnStart";

    public override string[] GetStepParameters()
    {
        return [VariableNameKey, ClientNameKey, IsGlobalVariableKey, IsPersistantKey, DataKey, MockingValuesKey, PayloadOnStartKey];
    }

    public string? VariableName
    {
        get => this[VariableNameKey]?.ToString();
        set
        {
            this[VariableNameKey] = value;
            Description = value;
        }
    }

    public string? ClientName
    {
        get => this[ClientNameKey]?.ToString();
        set => this[ClientNameKey] = value;
    }

    public bool IsGlobalVariable
    {
        get => (bool?)this[IsGlobalVariableKey] ?? false;
        set => this[IsGlobalVariableKey] = value;
    }

    public bool PayloadOnStart
    {
        get => (bool?)this[PayloadOnStartKey] ?? false;
        set => this[PayloadOnStartKey] = value;
    }

    public bool IsPersistant
    {
        get => (bool?)this[IsPersistantKey] ?? false;
        set => this[IsPersistantKey] = value;
    }

    public string? Data
    {
        get => this[DataKey]?.ToString();
        set => this[DataKey] = value;
    }

    public string? MockingValues
    {
        get => this[MockingValuesKey]?.ToString();
        set => this[MockingValuesKey] = value;
    }

    private int? variableId = null;
    private object? currentPayload = null;

    public override async Task<string?> SetupAsync(IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        var clientId = dataService.GetClients().Where(x => string.Compare(x.Name, ClientName, true) == 0).FirstOrDefault();
        if (VariableName != null && clientId != null)
        {
            List<string>? mochingValues = null;
            if (!string.IsNullOrWhiteSpace(MockingValues))
            {
                mochingValues = System.Text.Json.JsonSerializer.Deserialize<List<string>>(MockingValues);
            }
            variableId = await variableService.CreateVariableAsync(VariableName, clientId.Id, automation.Id, IsPersistant, Data, mochingValues);
            if (variableId != null)
            {
                var variableInfo = variableService.GetVariable(variableId.Value);
                currentPayload = variableInfo?.Value.AutoConvert();

                if (PayloadOnStart)
                {
                    Payloads[0].Data = currentPayload;
                }
            }
        }
        return null;
    }

    public override async Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        if (variableId != null)
        {
            var variableInfo = variableService.GetVariable(variableId.Value);
            var newPayload = variableInfo?.Value.AutoConvert();

            if (Payloads[0].UpdateData(newPayload))
            {
                currentPayload = newPayload;
                return [Blazor.Diagrams.Core.Models.PortAlignment.Right];
            }
        }
        return [];
    }
}
