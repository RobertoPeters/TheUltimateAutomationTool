using System.ComponentModel;
using System.Data;
using Tuat.Extensions;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.FinishFlow;

[DisplayName("Finish flow")]
[Editor("Tuat.FlowAutomation.StepTypes.FinishFlow.StepSettings", typeof(StepSettings))]
public class StepFinishFlow : Step
{
    public const string SubFlowParametersKey = "SubFlowParameters";

    public override string[] GetStepParameters()
    {
        return [SubFlowParametersKey];
    }

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Left];

    public override Task<string?> SetupAsync(FlowHandler instance, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        Automation = automation;
        return Task.FromResult((string?)null);
    }

    private bool isFinshed = false;

    public override Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(FlowHandler instance, Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        var inputActive = false;

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

        if (inputActive && !isFinshed)
        {
            FinishFlow(instance, scriptEngine);
            isFinshed = true;
        }

        return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([]);
    }

    private void FinishFlow(FlowHandler instance, IScriptEngine scriptEngine)
    {
        List<AutomationOutputVariable> outputVariables = [];
        var subFlowParameters = SubFlowParameters;
        if (subFlowParameters?.Any() == true)
        {
            var scriptVariables = scriptEngine.GetScriptVariables();
            var subAutomationParameters = instance.Automation.SubAutomationParameters
                    .Where(x => x.IsOutput)
                    .ToList();
            foreach (var subStateVariable in subFlowParameters)
            {
                var subAutomationParameter = subAutomationParameters.FirstOrDefault(x => x.Id == subStateVariable.Id);
                if (subAutomationParameter != null)
                {
                    var scriptVariable = subStateVariable.IsScriptVariable ? scriptVariables.FirstOrDefault(x => x.Name == subStateVariable.ScriptVariable)?.Value : subStateVariable.ScriptVariable.AutoConvert();
                    outputVariables.Add(new AutomationOutputVariable() { Name = subAutomationParameter.Name, Value = scriptVariable });
                }
            }
        }
        instance.SetAutomationFinished(outputVariables);
    }

    public Automation? Automation { get; set; }

    public List<SubFlowParameter>? SubFlowParameters
    {
        get => string.IsNullOrWhiteSpace(this[SubFlowParametersKey]?.ToString()) ? [] : System.Text.Json.JsonSerializer.Deserialize<List<SubFlowParameter>>(this[SubFlowParametersKey].ToString()!);
        set => this[SubFlowParametersKey] = value;
    }
}
