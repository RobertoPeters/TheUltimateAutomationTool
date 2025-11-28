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
    public bool _publishAllPayloads = false;
    private DateTime _lastPublishTime = DateTime.MinValue;

    public FlowHandler(Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
     : base(automation, clientService, dataService, variableService, messageBusService)
    {
    }

    public void RequestAllPayloads()
    {
        _publishAllPayloads = true;
        TriggerProcess();
    }

    protected override void RunStart(IScriptEngine scriptEngine, Guid instanceId, List<AutomationInputVariable>? InputValues = null, int? topAutomationId = null)
    {
        PublishPayloadsIfNeeded();
    }

    protected override void RunProcess(IScriptEngine scriptEngine)
    {
        _steps.Clear();
        foreach (var step in _automationProperties.Steps)
        {
            var convertedStep = Step.GetStep(step);
            _steps.Add(step);
        }
        PublishPayloadsIfNeeded();
    }

    private void PublishPayloadsIfNeeded()
    {
        var publishAll = _publishAllPayloads;
        _publishAllPayloads = false;
        var now = DateTime.UtcNow;
        if (_steps.Any())
        {
            var payloadInfo = new PayloadInfo();
            payloadInfo.AutomationId = Automation.Id;
            foreach (var step in _steps)
            {
                foreach(var payload in step.Payloads)
                {
                    if (publishAll || payload.ChangedAt > _lastPublishTime)
                    {
                        payloadInfo.Payloads.Add(payload);
                    }
                }
            }
            if (payloadInfo.Payloads.Any())
            {
                _messageBusService.PublishAsync(payloadInfo);
            }
        }
        _lastPublishTime = now;
    }
}
