using Tuat.Models;

namespace Tuat.Interfaces;

public interface IAutomationEditor
{
    Automation Automation { get; set; }

    string? Height { get; set; }

    Type? ScriptEditorType { get; set; }
    Task ReloadAutomationAsync(Automation automation);
    Task<string> GetAutomationDataAsync();
}
