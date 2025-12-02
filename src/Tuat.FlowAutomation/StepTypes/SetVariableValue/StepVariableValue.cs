using System.ComponentModel;
using Tuat.Extensions;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.SetVariableValue;

[DisplayName("Set Variable Value")]
[Editor("Tuat.FlowAutomation.StepTypes.SetVariableValue.StepSettings", typeof(StepSettings))]
public class StepVariableValue: Step
{
    public override List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Left, Blazor.Diagrams.Core.Models.PortAlignment.Top];

    public const string VariableNameKey = "VariableName";
    public const string ClientNameKey = "ClientName";
    public const string IsGlobalVariableKey = "IsGlobalVariable";
    public const string DefaultValueKey = "DefaultValueStart";

    public override string[] GetStepParameters()
    {
        return [VariableNameKey, ClientNameKey, IsGlobalVariableKey, DefaultValueKey];
    }

    public override Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, string> PortText { get; set; } = new Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, string>
    {
        {Blazor.Diagrams.Core.Models.PortAlignment.Top, "value"}
    };


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

    public string? DefaultValue
    {
        get => this[DefaultValueKey]?.ToString();
        set => this[DefaultValueKey] = value;
    }

    private int? variableId = null;
    private int? clientId = null;
    private object? currentValue = null;

    public override Task<string?> SetupAsync(FlowHandler instance, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        currentValue = DefaultValue.AutoConvert();
        clientId = dataService.GetClients().Where(x => string.Compare(x.Name, ClientName, true) == 0).FirstOrDefault()?.Id;
        if (VariableName != null && clientId != null)
        {
            variableId = variableService.GetVariables().Where(x => x.Variable.Name == VariableName 
                    && x.Variable.ClientId == clientId
                    && ((!IsGlobalVariable && x.Variable.AutomationId == (instance.TopAutomationId ?? automation.Id))
                        || (IsGlobalVariable && x.Variable.AutomationId == null))
                    ).FirstOrDefault()?.Variable.Id;
        }
        return Task.FromResult((string?)null);
    }

    public override async Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(FlowHandler instance, Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        var inputActive = false;

        if (clientId != null)
        {
            if (!inputPayloads.TryGetValue(Blazor.Diagrams.Core.Models.PortAlignment.Left, out var payloads))
            {
                inputActive = false;
            }
            else if (payloads?.Any() != true)
            {
                inputActive = false;
            }
            else
            {
                inputActive = payloads!.Any(x => x.IsTrue() == true);
            }

            if (inputActive)
            {
                if (variableId == null && VariableName != null)
                {
                    variableId = variableService.GetVariables().Where(x => x.Variable.Name == VariableName
                            && x.Variable.ClientId == clientId
                            && ((!IsGlobalVariable && x.Variable.AutomationId == (instance.TopAutomationId ?? automation.Id))
                                || (IsGlobalVariable && x.Variable.AutomationId == null))
                            ).FirstOrDefault()?.Variable.Id;
                }

                if (variableId != null)
                {
                    if (inputPayloads.TryGetValue(Blazor.Diagrams.Core.Models.PortAlignment.Top, out var valuePayloads))
                    {
                        if (valuePayloads?.Any() == true)
                        {
                            var newPayloadValue = valuePayloads[0].Data;
                            if (!object.Equals(currentValue, newPayloadValue))
                            {
                                currentValue = newPayloadValue;
                            }
                        }
                    }

                    var variable = variableService.GetVariable(variableId.Value);
                    if (variable != null && !object.Equals(currentValue, variable.VariableValue.Value))
                    {
                        await variableService.SetVariableValuesAsync([(variableId.Value, currentValue?.ToString())]);
                    }
                }
            }
        }
        return [];
    }
}
