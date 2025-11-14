using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.FreeScriptAutomation;

public class ScriptEngineInfo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Automation Automation { get; set; } = null!;
    public IScriptEngine Engine { get; set; } = null!;
}
