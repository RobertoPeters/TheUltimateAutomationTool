namespace Tuat.FlowAutomation;

public class Payload
{
    public Guid StepId { get; set; }
    public Blazor.Diagrams.Core.Models.PortAlignment Port { set; get; }
    public object? Data 
    {
        get { return field; } 
        set
        {
            if (field != value)
            {
                field = value;
                ChangedAt = DateTime.UtcNow;
            }
        }
    }
    public DateTime ChangedAt { get; private set; } = DateTime.UtcNow;
}
