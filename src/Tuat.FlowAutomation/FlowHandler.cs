using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel;
using System.Linq;
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
        RequestTriggerProcess();
    }

    protected override void RunStart(IScriptEngine scriptEngine, Guid instanceId, List<AutomationInputVariable>? InputValues = null, int? topAutomationId = null)
    {
        _steps.Clear();

        scriptEngine.Initialize(_clientService, _dataService, _variableService, this, instanceId, _automationProperties.PreStartAction, InputValues, topAutomationId);

        foreach (var step in _automationProperties.Steps)
        {
            var convertedStep = Step.GetStep(step);
            _steps.Add(convertedStep);
        }
        foreach(var step in _steps)
        {
            step.SetupAsync(Automation, _clientService, _dataService, _variableService, _messageBusService).Wait();
        }
        PublishPayloadsIfNeeded();
    }

    protected override void RunProcess(IScriptEngine scriptEngine)
    {
        foreach (var step in _steps)
        {
            HashSet<Guid> handledSteps = [];
            CheckStep(step, handledSteps);
        }
        PublishPayloadsIfNeeded();
    }

    private void CheckStep(Step step, HashSet<Guid> handledSteps)
    {
        if (handledSteps.Contains(step.Id))
        {
            return;
        }

        Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads = [];
        foreach (var inputPort in step.InputPorts)
        {
            var payloads = from t in _automationProperties.Transitions
                           where t.ToStepId == step.Id && t.ToStepPort == inputPort
                           join s in _steps on t.FromStepId equals s.Id
                           from p in s.Payloads
                           where p.Port == t.FromStepPort
                           select p;

            inputPayloads.Add(inputPort, payloads.ToList());
        }
        var changedPorts = step.ProcessAsync(inputPayloads, Automation, _clientService, _dataService, _variableService, _messageBusService).Result;

        if (changedPorts.Any())
        {
            return;
        }

        var nextSteps = from t in _automationProperties.Transitions
                        where t.FromStepId == step.Id && changedPorts.Contains(t.FromStepPort!.Value)
                        join s in _steps on t.ToStepId equals s.Id
                        select s;

        foreach(var nextStep in nextSteps.ToList())
        {
            CheckStep(nextStep, handledSteps);
        }
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
                RequestTriggerProcess();
            }
        }
        _lastPublishTime = now;
    }
}
