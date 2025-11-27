using System.ComponentModel;

namespace Tuat.FlowAutomation.StepTypes.VariableValue;

[DisplayName("Variable")]
[Editor("Tuat.FlowAutomation.StepTypes.VariableValue.StepSettings", typeof(StepSettings))]
public class StepVariableValue: Step
{
    public override List<Blazor.Diagrams.Core.Models.PortAlignment> OutputPorts { get; set; } = [Blazor.Diagrams.Core.Models.PortAlignment.Right];

    public const string VariableNameKey = "VariableName";
    public const string ClientNameKey = "ClientName";
    public const string IsGlobalVariableKey = "IsGlobalVariable";
    public const string IsPersistantKey = "IsPersistant";
    public const string DataKey = "Data";
    public const string MockingValuesKey = "MockingValues";
    public const string PayloadOnStartKey = "PayloadOnStart";

    public override string[] GetStepParameters()
    {
        return [VariableNameKey, ClientNameKey, IsGlobalVariableKey, IsPersistantKey, DataKey, MockingValuesKey];
    }

    public string? VariableName
    {
        get => this[VariableNameKey]?.ToString();
        set
        {
            this[VariableNameKey] = value;
            Description = value;
        }
    }

    public string? ClientName
    {
        get => this[ClientNameKey]?.ToString();
        set => this[ClientNameKey] = value;
    }

    public bool IsGlobalVariable
    {
        get => (bool?)this[IsGlobalVariableKey] ?? false;
        set => this[IsGlobalVariableKey] = value;
    }

    public bool PayloadOnStart
    {
        get => (bool?)this[PayloadOnStartKey] ?? false;
        set => this[PayloadOnStartKey] = value;
    }

    public bool IsPersistant
    {
        get => (bool?)this[IsPersistantKey] ?? false;
        set => this[IsPersistantKey] = value;
    }

    public string? Data
    {
        get => this[DataKey]?.ToString();
        set => this[DataKey] = value;
    }

    public string? MockingValues
    {
        get => this[MockingValuesKey]?.ToString();
        set => this[MockingValuesKey] = value;
    }

}
