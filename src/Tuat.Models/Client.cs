namespace Tuat.Models;

public class Client: ModelBase
{
    public string Name { get; set; } = null!;
    public bool Enabled { get; set; }
    public string ClientType { get; set; } = null!;
    public string Data { get; set; } = null!;
}
