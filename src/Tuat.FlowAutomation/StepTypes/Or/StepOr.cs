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

    public override Task<string?> SetupAsync(FlowHandler instance, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        return Task.FromResult((string?)null);
    }

    public override Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(FlowHandler instance, Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        bool? result = null;
        if (!inputPayloads.TryGetValue(Blazor.Diagrams.Core.Models.PortAlignment.Left, out var payloads))
        {
            result = null;
        }
        else if (payloads?.Any() != true)
        {
            result = null;
        }
        else
        {
            result = payloads!.Any(x => x.IsTrue() == true);
        }

        if (Payloads[0].UpdateData(result == true ? true : null))
        {
            return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([Blazor.Diagrams.Core.Models.PortAlignment.Right]);
        }
        return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([]);
    }

}
