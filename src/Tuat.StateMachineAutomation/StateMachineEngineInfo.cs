using Tuat.Interfaces;
using Tuat.Models;

namespace Tuat.StateMachineAutomation;

public class StateMachineEngineInfo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Automation Automation { get; set; } = null!;
    public IScriptEngine Engine { get; set; } = null!;
}
