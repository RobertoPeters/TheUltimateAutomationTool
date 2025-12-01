using System.ComponentModel;
using System.Text;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation.StepTypes.Script;

[DisplayName("Script")]
[Editor("Tuat.FlowAutomation.StepTypes.Script.StepSettings", typeof(StepSettings))]
public class StepScript : Step
{
    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public override List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Left];

    public const string PreStartScriptKey = "PreStartScript";
    public const string ScriptKey = "Script";

    public override Task<string?> SetupAsync(IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        var setupScript = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(PreStartScript))
        {
            setupScript.AppendLine(PreStartScript);
            setupScript.AppendLine();
        }
        if (!string.IsNullOrWhiteSpace(Script))
        {
            var stepScript = scriptEngine.GetDeclareFunction(
                StepMethodName, 
                new IScriptEngine.FunctionReturnValue() { Nullable=true, Type = typeof(object) },
                new List<IScriptEngine.FunctionParameter>() 
                {
                     new IScriptEngine.FunctionParameter() { Name = "inputPayloads", Nullable = true, Type = typeof(List<object?>) }
                }, 
                Script);
            setupScript.AppendLine(stepScript);
            setupScript.AppendLine();
        }
        return Task.FromResult((string?)setupScript.ToString());
    }

    public override Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(FlowHandler instance, Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        if (!string.IsNullOrWhiteSpace(Script))
        {
            var inputPayloadsList = inputPayloads.SelectMany(x => x.Value.Select(y => y.Data)).ToList();
            var outputPayload = scriptEngine.CallFunction<object?>(StepMethodName, [new IScriptEngine.FunctionParameter() { Name = "inputPayloads", Nullable= true, Type = typeof(List<object?>), Value = inputPayloadsList }]);
            Payloads[0].Data = outputPayload;
        }

        return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([]);
    }

    public string? PreStartScript
    {
        get => this[PreStartScriptKey]?.ToString();
        set => this[PreStartScriptKey] = value;
    }

    public string? Script
    {
        get => this[ScriptKey]?.ToString();
        set => this[ScriptKey] = value;
    }

    private string StepMethodName => $"step_method_{Id.ToString("N")}";

}
