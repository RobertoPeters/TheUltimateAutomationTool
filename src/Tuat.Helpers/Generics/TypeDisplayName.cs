namespace Tuat.Helpers.Generics;

public class TypeDisplayName
{
    public string TypeName { get; set; } = null!;
    public Type Type { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? SettingsEditorType { get; set; } = null!;
    public Type? SettingsEditorComponentType { get; set; } = null!;
    public string? EditorType { get; set; } = null!;
    public Type? EditorComponentType { get; set; } = null!;
}
