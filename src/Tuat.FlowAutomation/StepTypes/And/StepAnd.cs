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

    public override Task<string?> SetupAsync(IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        return Task.FromResult((string?)null);
    }

    public override Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        if (!inputPayloads.TryGetValue(Blazor.Diagrams.Core.Models.PortAlignment.Left, out var payloads))
        {
            Payloads[0].Data = null;
        }
        if (payloads?.Any() != true)
        {
            Payloads[0].Data = null;
        }
        var result = payloads!.Any(x => x.IsTrue() != true);

        if (Payloads[0].UpdateData(result ? null : true))
        {
            return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([Blazor.Diagrams.Core.Models.PortAlignment.Right]);
        }
        return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([]);
    }

}
