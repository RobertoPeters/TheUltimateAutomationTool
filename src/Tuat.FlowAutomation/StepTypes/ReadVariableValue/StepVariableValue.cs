using System.ComponentModel;
using Tuat.Extensions;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.ReadVariableValue;

[DisplayName("Variable (read)")]
[Editor("Tuat.FlowAutomation.StepTypes.ReadVariableValue.StepSettings", typeof(StepSettings))]
public class StepVariableValue: Step
{
    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public const string VariableNameKey = "VariableName";
    public const string ClientNameKey = "ClientName";
    public const string IsGlobalVariableKey = "IsGlobalVariable";
    public const string PayloadOnStartKey = "PayloadOnStart";

    public override string[] GetStepParameters()
    {
        return [VariableNameKey, ClientNameKey, IsGlobalVariableKey, PayloadOnStartKey];
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

    private int? variableId = null;
    private object? currentPayload = null;

    public override Task<string?> SetupAsync(FlowHandler instance, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        var clientId = dataService.GetClients().Where(x => string.Compare(x.Name, ClientName, true) == 0).FirstOrDefault();
        if (VariableName != null && clientId != null)
        {
            variableId = variableService.GetVariables().Where(x => x.Variable.Name == VariableName 
                    && x.Variable.ClientId == clientId.Id 
                    && ((!IsGlobalVariable && x.Variable.AutomationId == (instance.TopAutomationId ?? automation.Id))
                        || (IsGlobalVariable && x.Variable.AutomationId == null))
                    ).FirstOrDefault()?.Variable.Id;

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
        return Task.FromResult((string?)null);
    }

    public override async Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(FlowHandler instance, Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
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
