namespace Tuat.Models;

public class AIProvider : ModelBase
{
    public string Name { get; set; } = null!;
    public string ProviderType { get; set; } = null!;
    public string Settings { get; set; } = null!;
}
