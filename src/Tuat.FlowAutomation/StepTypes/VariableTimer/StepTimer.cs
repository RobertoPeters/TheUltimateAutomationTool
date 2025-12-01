using System.ComponentModel;
using System.Data;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.VariableTimer;

[DisplayName("Timer (auto cancel and runtime timeout)")]
[Editor("Tuat.FlowAutomation.StepTypes.VariableTimer.StepSettings", typeof(StepSettings))]
public class StepTimer : Step
{
    public const string VariableNameKey = "VariableName";
    public const string TimeoutSecondsKey = "TimeoutSeconds";

    public override string[] GetStepParameters()
    {
        return [VariableNameKey, TimeoutSecondsKey];
    }

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Left, Blazor.Diagrams.Core.Models.PortAlignment.Top];

    public override Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, string> PortText { get; set; } = new Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, string>
    {
        {Blazor.Diagrams.Core.Models.PortAlignment.Top, "Timeout"}
    };

    private int? variableId = null;
    private int? clientId = null;
    private bool timerIsRunning = false;
    private int currentTimeout = 0;

    public override async Task<string?> SetupAsync(IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        clientId = dataService.GetClients().Where(x => string.Compare(x.Name, "Timer", true) == 0).FirstOrDefault()?.Id;
        if (VariableName != null && clientId != null)
        {
            variableId = await variableService.CreateVariableAsync(VariableName, clientId.Value, automation.Id, false, TimeoutSeconds.ToString(), ["0", "10"]);
            currentTimeout = TimeoutSeconds!.Value;
        }
        return null;
    }

    public override async Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
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
                    //check if timer value has changes
                    if (inputPayloads.TryGetValue(Blazor.Diagrams.Core.Models.PortAlignment.Top, out var timeOutpayloads))
                    {
                        if (timeOutpayloads?.Any() == true)
                        {
                            var timeoutPayload = timeOutpayloads!.FirstOrDefault(x => x.Data != null)?.Data;
                            if (timeoutPayload != null)
                            {
                                if (int.TryParse(timeoutPayload.ToString(), out var newTimeout))
                                {
                                    if (newTimeout != currentTimeout)
                                    {
                                        currentTimeout = newTimeout;
                                    }
                                }
                            }
                        }
                    }

                    await clientService.ExecuteAsync(clientId!.Value, variableId, "start", currentTimeout, null, null);
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
                await clientService.ExecuteAsync(clientId!.Value, variableId, "stop", null, null, null);
                timerIsRunning = false;
            }
        }

        if (Payloads[0].UpdateData(result ? true : null))
        {
            return [Blazor.Diagrams.Core.Models.PortAlignment.Right];
        }
        return [];
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
