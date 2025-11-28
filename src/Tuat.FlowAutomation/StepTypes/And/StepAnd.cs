using System.ComponentModel;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.And;

[DisplayName("And")]
[Editor("Tuat.FlowAutomation.StepTypes.And.StepSettings", typeof(StepSettings))]
public class StepAnd: Step
{
    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Left];

    public override Task SetupAsync(Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        return Task.CompletedTask;
    }

    public override Task ProcessAsync(Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        if (!inputPayloads.TryGetValue(Blazor.Diagrams.Core.Models.PortAlignment.Left, out var payloads))
        {
            Payloads[0].Data = null;
        }
        if (payloads?.Any() != true)
        {
            Payloads[0].Data = null;
        }
        var result = payloads!.Any(x => x.Data == null
                || x.Data?.ToString()?.ToLower() == "off" 
                || x.Data?.ToString()?.ToLower() == "false" 
                || x.Data?.ToString()?.ToLower() == "0");

        Payloads[0].Data = result ? null : true;
        return Task.CompletedTask;
    }

}
