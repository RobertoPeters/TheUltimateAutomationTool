using System.ComponentModel;
using Tuat.Extensions;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.Constant;

[DisplayName("Constant")]
[Editor("Tuat.FlowAutomation.StepTypes.Constant.StepSettings", typeof(StepSettings))]
public class StepConstant: Step
{
    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Left];

    public const string ConstantValueKey = "ConstantValue";
    public const string PayloadOnStartKey = "PayloadOnStart";

    public override string[] GetStepParameters()
    {
        return [ConstantValueKey, PayloadOnStartKey];
    }

    public string? ConstantValue
    {
        get => this[ConstantValueKey]?.ToString();
        set=> this[ConstantValueKey] = value;
    }

    public bool PayloadOnStart
    {
        get => (bool?)this[PayloadOnStartKey] ?? false;
        set => this[PayloadOnStartKey] = value;
    }

    private object? constantPayload = null;

    public override Task<string?> SetupAsync(FlowHandler instance, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        constantPayload = ConstantValue.AutoConvert();
        if (PayloadOnStart)
        {
            Payloads[0].Data = constantPayload;
        }
        return Task.FromResult((string?)null);
    }

    public override async Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(FlowHandler instance, Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        bool? result = null;
        if (PayloadOnStart)
        {
            result = true;
        }
        else if (!inputPayloads.TryGetValue(Blazor.Diagrams.Core.Models.PortAlignment.Left, out var payloads))
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

        if (Payloads[0].UpdateData(result == true ? constantPayload : null))
        {
            return [Blazor.Diagrams.Core.Models.PortAlignment.Right];
        }
        return [];
    }
}
