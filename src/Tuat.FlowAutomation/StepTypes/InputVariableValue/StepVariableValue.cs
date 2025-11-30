using System.Collections.Generic;
using System.ComponentModel;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.InputVariableValue;

[DisplayName("Input variable")]
[Editor("Tuat.FlowAutomation.StepTypes.InputVariableValue.StepSettings", typeof(StepSettings))]
public class StepVariableValue : Step
{
    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public const string VariableIdKey = "VariableId";

    public override string[] GetStepParameters()
    {
        return [VariableIdKey];
    }

    public Guid? VariableId
    {
        get
        {
            var s = this[VariableIdKey]?.ToString();
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }
            return Guid.Parse(s);
        }
        set => this[VariableIdKey] = value;
    }

    private bool payloadSet = false;

    public override Task<string?> SetupAsync(IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        return Task.FromResult((string?)null);
    }

    public override Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        if (!payloadSet)
        {
            payloadSet = true;

            if (VariableId != null)
            {
                var inputParameter = automation.SubAutomationParameters.Where(x => x.Id == VariableId).FirstOrDefault();
                if (inputParameter != null && inputParameter.IsInput)
                {
                    var scriptVariable = scriptEngine.GetScriptVariables().FirstOrDefault(x => x.Name == inputParameter.ScriptVariableName);
                    if (scriptVariable != null)
                    {
                        if (scriptVariable.Value != null)
                        {
                            Payloads[0].Data = scriptVariable.Value;
                            return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([Blazor.Diagrams.Core.Models.PortAlignment.Right]);
                        }
                    }
                }
            }
        }
        return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([]);
    }
}
