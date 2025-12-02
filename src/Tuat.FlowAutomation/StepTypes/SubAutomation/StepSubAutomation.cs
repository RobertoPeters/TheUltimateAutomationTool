using System.ComponentModel;
using System.Data;
using Tuat.Extensions;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.SubAutomation;

[DisplayName("Sub Automation")]
[Editor("Tuat.FlowAutomation.StepTypes.SubAutomation.StepSettings", typeof(StepSettings))]
public class StepSubAutomation : Step
{
    public const string AutomationIdKey = "AutomationId";
    public const string SubFlowParametersKey = "SubFlowParameters";

    public override string[] GetStepParameters()
    {
        return [AutomationIdKey, SubFlowParametersKey];
    }

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Left];

    public override Task<string?> SetupAsync(IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
         return Task.FromResult((string?)null);
    }

    private bool subAutomationStarted = false;
    private bool subAutomationRunning = false;

    public override Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(FlowHandler instance, Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        var inputActive = false;
        var result = false;

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
            if (!subAutomationStarted)
            {
                StartSubAutomation(instance, scriptEngine, dataService);
                subAutomationStarted = true;
                subAutomationRunning = true;
            }
            else
            {
                subAutomationRunning = instance.IsSubAutomationRunning();
                if (!subAutomationRunning)
                {
                    result = true;
                }
            }
        }
        else if (subAutomationRunning)
        {
            instance.StopSubAutomation();
            subAutomationStarted = false;
            subAutomationRunning = false;
        }

        if (Payloads[0].UpdateData(result ? true : null))
        {
            return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([Blazor.Diagrams.Core.Models.PortAlignment.Right]);
        }
        return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([]);
    }

    private void StartSubAutomation(FlowHandler instance, IScriptEngine scriptEngine, IDataService dataService)
    {
        List<AutomationInputVariable> inputVariables = [];
        var subFlowParameters = SubFlowParameters;
        if (subFlowParameters?.Any() == true)
        {
            var scriptVariables = scriptEngine.GetScriptVariables();
            var subAutomationParameters = dataService.GetAutomations()
                    .First(x => x.Id == AutomationId).SubAutomationParameters
                    .Where(x => x.IsInput)
                    .ToList();
            foreach (var subStateVariable in subFlowParameters)
            {
                var subAutomationParameter = subAutomationParameters.FirstOrDefault(x => x.Id == subStateVariable.Id);
                if (subAutomationParameter != null)
                {
                    var scriptVariable = subStateVariable.IsScriptVariable ? scriptVariables.FirstOrDefault(x => x.Name == subStateVariable.ScriptVariable)?.Value : subStateVariable.ScriptVariable.AutoConvert();
                    inputVariables.Add(new AutomationInputVariable() { Name = subAutomationParameter.Name, Value = scriptVariable });
                }
            }
        }
        instance.StartSubAutomation(AutomationId!.Value, inputVariables);
    }

    public int? AutomationId
    {
        get => (int?)this[AutomationIdKey];
        set => this[AutomationIdKey] = value;
    }
    public List<SubFlowParameter>? SubFlowParameters
    {
        get => string.IsNullOrWhiteSpace(this[SubFlowParametersKey]?.ToString()) ? [] : System.Text.Json.JsonSerializer.Deserialize<List<SubFlowParameter>>(this[SubFlowParametersKey].ToString()!);
        set => this[SubFlowParametersKey] = value;
    }
}
