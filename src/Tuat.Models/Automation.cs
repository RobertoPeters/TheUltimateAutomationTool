namespace Tuat.Models;

public class Automation : ModelBase
{
    public string Name { get; set; } = null!;
    public bool Enabled { get; set; }
    public bool IsSubAutomation { get; set; }
    public string AutomationType { get; set; } = null!;
    public int? IncludeScriptId { get; set; } = null!;
    public string ScriptType { get; set; } = null!;
    public string? Settings { get; set; } = null!;
    public string Data { get; set; } = null!;
    public List<SubAutomationParameter> SubAutomationParameters { get; set; } = [];
}

