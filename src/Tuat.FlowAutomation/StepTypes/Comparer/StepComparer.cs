using System.ComponentModel;
using System.Data;
using Tuat.Extensions;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.Comparer;

[DisplayName("Comparer")]
[Editor("Tuat.FlowAutomation.StepTypes.Comparer.StepSettings", typeof(StepSettings))]
public class StepComparer : Step
{
    public const string ComparerIdKey = "ComparerId";
    public const string ReferenceValueKey = "ReferenceValue";

    public override string[] GetStepParameters()
    {
        return [ComparerIdKey, ReferenceValueKey];
    }

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Left, Blazor.Diagrams.Core.Models.PortAlignment.Top];

    public override Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, string> PortText { get; set; } = new Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, string>
    {
        {Blazor.Diagrams.Core.Models.PortAlignment.Top, "Reference"}
    };

    private CompareOperator? comparer = null;

    public override async Task<string?> SetupAsync(FlowHandler instance, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        comparer = CompareOperator.Operators.FirstOrDefault(x => x.Id == ComparerId);
        return null;
    }

    public override async Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(FlowHandler instance, Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        var inputActive = false;
        bool? outputValue = null;
        if (comparer != null)
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
                inputActive = true;
            }

            if (inputActive)
            {
                object? inputValue = payloads!.First().Data;
                object? referenceValue = null;

                if (inputPayloads.TryGetValue(Blazor.Diagrams.Core.Models.PortAlignment.Top, out var refOutpayloads) && refOutpayloads?.Any() == true)
                {
                    referenceValue = refOutpayloads![0].Data;
                }
                else
                {
                    referenceValue = ReferenceValue.AutoConvert();
                }

                outputValue = comparer.Evaluate(inputValue, referenceValue);
            }
            else
            {
                outputValue = null;
            }
        }

        if (Payloads[0].UpdateData(outputValue == true ? true : null))
        {
            return [Blazor.Diagrams.Core.Models.PortAlignment.Right];
        }
        return [];
    }

    public int? ComparerId
    {
        get => (int?)this[ComparerIdKey];
        set => this[ComparerIdKey] = value;
    }

    public string? ReferenceValue
    {
        get => this[ReferenceValueKey]?.ToString();
        set => this[ReferenceValueKey] = value;
    }
}
