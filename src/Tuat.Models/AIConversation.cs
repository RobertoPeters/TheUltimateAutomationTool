namespace Tuat.Models;

public class AIConversation : ModelBase
{
    public string Name { get; set; } = null!;
    public int? DefaultAIProviderId { get; set; }
    public Dictionary<string, int?> AIAgentSettingAIProviderId { get; set; } = [];
}
