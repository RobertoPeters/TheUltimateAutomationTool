namespace Tuat.Models;

public class Library : ModelBase
{
    public string Name { get; set; } = null!;
    public string ScriptType { get; set; } = null!;
    public List<int> IncludeScriptIds { get; set; } = [];
    public string? Script { get; set; }
}
