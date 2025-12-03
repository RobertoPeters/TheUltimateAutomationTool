using System.ComponentModel;
using Tuat.Extensions;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.ExecuteOnClient;

[DisplayName("Execute on client")]
[Editor("Tuat.FlowAutomation.StepTypes.ExecuteOnClient.StepSettings", typeof(StepSettings))]
public class StepExecuteOnClient: Step
{
    public override List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Left];

    public const string VariableNameKey = "VariableName";
    public const string ClientNameKey = "ClientName";
    public const string IsGlobalVariableKey = "IsGlobalVariable";
    public const string CommandKey = "Command";
    public const string Parameter1Key = "Parameter1";
    public const string Parameter2Key = "Parameter2";
    public const string Parameter3Key = "Parameter3";

    public override string[] GetStepParameters()
    {
        return [VariableNameKey, ClientNameKey, IsGlobalVariableKey, CommandKey, Parameter1Key, Parameter2Key, Parameter3Key];
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

    public string? Command
    {
        get => this[CommandKey]?.ToString();
        set => this[CommandKey] = value;
    }

    public string? Parameter1
    {
        get => this[Parameter1Key]?.ToString();
        set => this[Parameter1Key] = value;
    }

    public string? Parameter2
    {
        get => this[Parameter2Key]?.ToString();
        set => this[Parameter2Key] = value;
    }

    public string? Parameter3
    {
        get => this[Parameter3Key]?.ToString();
        set => this[Parameter3Key] = value;
    }

    private int? variableId = null;
    private int? clientId = null;
    private bool activeHasBeenHandled = false;

    public override Task<string?> SetupAsync(FlowHandler instance, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        clientId = dataService.GetClients().Where(x => string.Compare(x.Name, ClientName, true) == 0).FirstOrDefault()?.Id;
        if (!string.IsNullOrWhiteSpace(VariableName) && clientId != null)
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

            if (inputActive && !activeHasBeenHandled)
            {
                activeHasBeenHandled = true;
                if (variableId == null && !string.IsNullOrWhiteSpace(VariableName))
                {
                    variableId = variableService.GetVariables().Where(x => x.Variable.Name == VariableName
                            && x.Variable.ClientId == clientId
                            && ((!IsGlobalVariable && x.Variable.AutomationId == (instance.TopAutomationId ?? automation.Id))
                                || (IsGlobalVariable && x.Variable.AutomationId == null))
                            ).FirstOrDefault()?.Variable.Id;
                }

                await clientService.ExecuteAsync(clientId.Value, variableId, Command ?? "", Parameter1?.AutoConvert(), Parameter2?.AutoConvert(), Parameter3?.AutoConvert());
            }
        }
        return [];
    }
}
