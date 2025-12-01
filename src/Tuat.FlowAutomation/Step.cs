using System.Text.Json.Serialization;
using Tuat.Extensions;
using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FlowAutomation;

public class Step
{
    public Guid Id { get; set; }
    public string StepTypeName { get; set; } = null!;
    public string? Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? UIData { get; set; }
    public Dictionary<string, object?> StepParameters { get; set; } = [];

    [field: JsonIgnore]
    [JsonIgnore]
    public Type Type
    {
        get
        {
            if (field == null)
            {
                field = Type.GetType(StepTypeName)!;
            }
            return field!;
        }
        set
        {
            field = value;
            StepTypeName = value.FullName!;
        }
    }

    [JsonIgnore]
    public virtual List<Blazor.Diagrams.Core.Models.PortAlignment> InputPorts { get; set; } = [];
    [JsonIgnore]
    public virtual List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [];
    [JsonIgnore]
    public virtual Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, string> PortText { get; set; } = [];

    [JsonIgnore]
    public List<Payload> Payloads { get; set; } = [];

    [JsonIgnore]
    private bool _isInitialized = false;

    public Step()
    {
    }

    public virtual void Initialize()
    {
        if (_isInitialized) return;
        var stepParameters = GetStepParameters();
        foreach (var parameter in stepParameters.Where(x => !StepParameters.ContainsKey(x)).ToList())
        {
            StepParameters[parameter] = null;
        }
        foreach(var outputPort in OutputPorts)
        {
            Payloads.Add(new Payload() { StepId = Id, Port = outputPort, Data = null });
        }
        _isInitialized = true;
    }

    public static Step GetStep(Step step)
    {
        var type = Type.GetType(step.StepTypeName)!;
        var convertedStep = (Step)step.CopyObjectToOtherType(type)!;
        convertedStep.Initialize();
        return convertedStep;
    }

    public object? this[string key]
    {
        get
        {
            if (StepParameters.TryGetValue(key, out var value))
            {
                if (value is System.Text.Json.JsonElement jsonElement)
                {
                    switch (jsonElement.ValueKind)
                    {
                        case System.Text.Json.JsonValueKind.String:
                            return jsonElement.GetString();
                        case System.Text.Json.JsonValueKind.Number:
                            if (jsonElement.TryGetInt32(out var intValue))
                            {
                                return intValue;
                            }
                            if (jsonElement.TryGetDouble(out var doubleValue))
                            {
                                return doubleValue;
                            }
                            break;
                        case System.Text.Json.JsonValueKind.True:
                        case System.Text.Json.JsonValueKind.False:
                            return jsonElement.GetBoolean();
                        case System.Text.Json.JsonValueKind.Null:
                            return null;
                        default:
                            return jsonElement.GetRawText();
                    }
                }
                return value;
            }
            return null;
        }
        set
        {
            StepParameters[key] = value;
        }
    }

    public string Title => !string.IsNullOrWhiteSpace(Name) ? Name : Description ?? "";

    public virtual string[] GetStepParameters()
    {
        return [];
    }

    public virtual Task<string?> SetupAsync(IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        return Task.FromResult((string?)null);
    }

    public virtual Task<List<Blazor.Diagrams.Core.Models.PortAlignment>> ProcessAsync(Dictionary<Blazor.Diagrams.Core.Models.PortAlignment, List<Payload>> inputPayloads, IScriptEngine scriptEngine, Automation automation, IClientService clientService, IDataService dataService, IVariableService variableService, IMessageBusService messageBusService)
    {
        return Task.FromResult<List<Blazor.Diagrams.Core.Models.PortAlignment>>([]);
    }
}
