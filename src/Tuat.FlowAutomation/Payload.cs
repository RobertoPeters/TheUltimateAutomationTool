using JasperFx.CodeGeneration;

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
            if (!object.Equals(field, value))
            {
                field = value;
                ChangedAt = DateTime.UtcNow;
            }
        }
    }
    public DateTime ChangedAt { get; private set; } = DateTime.UtcNow;
    public bool UpdateData(object? value)
    {
        if (!object.Equals(Data, value))
        {
            Data = value;
            return true;
        }
        return false;
    }

    public bool IsEmpty()
    {
        return Data switch
        {
            null => true,
            string str => string.IsNullOrEmpty(str),
            _ => false
        };
    }

    public bool? IsFalse()
    {
        return Data switch
        {
            null => null,
            string str => string.IsNullOrEmpty(str) ? null : (str == "0" || string.Compare(str, "false", true) == 0 || string.Compare(str, "off", true) == 0),
            int i => i == 0,
            bool b => b == false,
            long l => l == 0,
            float f => f == 0,
            double d => d == 0,
            _ => false
        };
    }

    public bool? IsTrue()
    {
        var isFalse = IsFalse();
        return isFalse == null ? null : !isFalse;
    }
}
