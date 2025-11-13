namespace Tuat.ScriptEngineCSharp;

public class DynamicScriptApi
{
    public Dictionary<string, object> Methods { get; } = new();
    public SystemMethods _systemMethods { get; set; } = null!;
}
