using Tuat.Models;

namespace Tuat.Interfaces;

public class AutomationStateInfo
{
    public int AutomationId { get; set; }
    public AutomationRunningState AutomationRunningState { get; set; }
}
