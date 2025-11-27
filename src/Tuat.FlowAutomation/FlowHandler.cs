using System.ComponentModel;
using Tuat.AutomationHelper;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation;

[DisplayName("Flow (under construction)")]
[Editor("Tuat.FlowAutomation.AutomationSettings", typeof(AutomationSettings))]
[Editor("Tuat.FlowAutomation.Editor", typeof(Editor))]
public class FlowHandler : BaseAutomationHandler<AutomationProperties>, IAutomationHandler
{
    private List<Step> _steps = [];

    public Step? GetStep(Guid id)
    {
        return _steps.FirstOrDefault(s => s.Id == id);
    }

    public FlowHandler(Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
     : base(automation, clientService, dataService, variableService, messageBusService)
    {
    }

    protected override void RunStart(IScriptEngine scriptEngine, Guid instanceId, List<AutomationInputVariable>? InputValues = null, int? topAutomationId = null)
    {

    }

    protected override void RunProcess(IScriptEngine scriptEngine)
    {
        _steps.Clear();
        foreach (var step in _automationProperties.Steps)
        {
            var convertedStep = Step.GetStep(step);
            _steps.Add(step);
        }

    }
}
