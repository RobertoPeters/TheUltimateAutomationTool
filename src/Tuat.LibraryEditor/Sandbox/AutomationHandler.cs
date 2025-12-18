using System.Text;
using Tuat.AutomationHelper;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.LibraryEditor.Sandbox;

internal class AutomationHandler : BaseAutomationHandler<AutomationProperties>, IAutomationHandler
{
    public AutomationHandler(Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
        : base(automation, clientService, dataService, variableService, messageBusService)
    {
    }

    protected override void RunStart(IScriptEngine scriptEngine, Guid instanceId, List<AutomationInputVariable>? InputValues = null, int? topAutomationId = null)
    {
        var additionalScript = new StringBuilder();

        if (Automation.IncludeScriptId != null)
        {
            additionalScript.AppendLine();
            additionalScript.AppendLine(Tuat.Helpers.LibraryScriptGenerator.GenerateScriptCode(_dataService, Automation.IncludeScriptId, null));
            additionalScript.AppendLine();
        }

        scriptEngine.Initialize(_clientService, _dataService, _variableService, this, instanceId, additionalScript.ToString(), InputValues, topAutomationId);
    }
}
