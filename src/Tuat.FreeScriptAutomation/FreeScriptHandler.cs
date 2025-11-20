using System.ComponentModel;
using Tuat.AutomationHelper;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FreeScriptAutomation;

[DisplayName("Free Script")]
[Editor("Tuat.FreeScriptAutomation.AutomationSettings", typeof(AutomationSettings))]
[Editor("Tuat.FreeScriptAutomation.Editor", typeof(Editor))]
public class FreeScriptHandler : BaseAutomationHandler<AutomationProperties>, IAutomationHandler
{
    public FreeScriptHandler(Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
        : base(automation, clientService, dataService, variableService, messageBusService)
    {
    }

    protected override void RunProcess(IScriptEngine scriptEngine)
    {
        scriptEngine.CallVoidFunction("schedule", null);
    }

    protected override void RunStart(IScriptEngine scriptEngine, Guid instanceId, List<AutomationInputVariable>? InputValues = null)
    {
        var scheduleFunctionDeclaration = scriptEngine.GetDeclareFunction("schedule", null);
        if (string.IsNullOrWhiteSpace(_automationProperties.Script))
        {
            _automationProperties.Script = scheduleFunctionDeclaration;
        }

        scriptEngine.Initialize(_clientService, _dataService, _variableService, this, instanceId, _automationProperties.Script, InputValues);
    }
}
