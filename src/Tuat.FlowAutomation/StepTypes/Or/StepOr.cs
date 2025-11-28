using System.ComponentModel;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.Or;

[DisplayName("Or")]
[Editor("Tuat.FlowAutomation.StepTypes.Or.StepSettings", typeof(StepSettings))]
public class StepOr: Step
{
    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Left];

    public override Task SetupAsync(Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        return Task.CompletedTask;
    }

    public override Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        if (!inputPayloads.TryGetValue(Blazor.Diagrams.Core.Models.PortAlignment.Left, out var payloads))
        {
            Payloads[0].Data = null;
        }
        if (payloads?.Any() != true)
        {
            Payloads[0].Data = null;
        }
        var result = payloads!.Count(x => x.Data == null
                || x.Data.ToString()?.ToLower() == "off" 
                || x.Data.ToString()?.ToLower() == "false" 
                || x.Data.ToString()?.ToLower() == "0") == payloads!.Count;

        if (Payloads[0].UpdateData(result ? null : true))
        {
            return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([Blazor.Diagrams.Core.Models.PortAlignment.Right]);
        }
        return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([]);
    }

}
