using Tuat.Models;

namespace Tuat.Interfaces;

public class VariableInfo
{
    public Variable Variable { get; set; } = null!;
    public VariableValue VariableValue { get; set; } = null!;
    public bool IsMocking { get; set; } = false;
    public string? MockingValue { get; set; }
    public string? Value => IsMocking ? MockingValue : VariableValue.Value;
}

