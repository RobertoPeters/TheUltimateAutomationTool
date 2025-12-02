using System.ComponentModel;
using System.Data;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.Timer;

[DisplayName("Timer (auto cancel)")]
[Editor("Tuat.FlowAutomation.StepTypes.Timer.StepSettings", typeof(StepSettings))]
public class StepTimer : Step
{
    public const string VariableNameKey = "VariableName";
    public const string TimeoutSecondsKey = "TimeoutSeconds";

    public override string[] GetStepParameters()
    {
        return [VariableNameKey, TimeoutSecondsKey];
    }

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Left];

    private int? variableId = null;
    private int? clientId = null;
    private bool timerIsRunning = false;

    public override async Task<string?> SetupAsync(FlowHandler instance, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        clientId = dataService.GetClients().Where(x => string.Compare(x.Name, "Timer", true) == 0).FirstOrDefault()?.Id;
        if (VariableName != null && clientId != null)
        {
            variableId = await variableService.CreateVariableAsync(VariableName, clientId.Value, instance.TopAutomationId ?? automation.Id, false, TimeoutSeconds.ToString(), ["0", "10"]);
        }
        return null;
    }

    public override Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(FlowHandler instance, Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        var inputActive = false;
        var result = false;
        if (variableId != null)
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
                if (!timerIsRunning)
                {
                    clientService.ExecuteAsync(clientId!.Value, variableId, "start", null, null, null);
                    timerIsRunning = true;
                }
                else
                {
                    var timerValue = variableService.GetVariable(variableId.Value)!.Value;
                    if (timerValue == "0")
                    {
                        result = true;
                    }
                }
            }
            else if (timerIsRunning)
            {               
                 clientService.ExecuteAsync(clientId!.Value, variableId, "stop", null, null, null);
                timerIsRunning = false;
            }
        }

        if (Payloads[0].UpdateData(result ? true : null))
        {
            return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([Blazor.Diagrams.Core.Models.PortAlignment.Right]);
        }
        return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([]);
    }

    public string? VariableName
    {
        get => this[VariableNameKey]?.ToString();
        set => this[VariableNameKey] = value;
    }

    public int? TimeoutSeconds
    {
        get => (int?)this[TimeoutSecondsKey] ?? 5;
        set => this[TimeoutSecondsKey] = value;
    }
}
