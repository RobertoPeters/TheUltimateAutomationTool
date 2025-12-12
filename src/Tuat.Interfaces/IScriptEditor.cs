namespace Tuat.Interfaces;

public interface IScriptEditor
{
    Task<string?> GetScriptAsync();
    Task SetScriptAsync(string? text, string? systemScript);
}
