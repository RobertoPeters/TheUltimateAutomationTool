namespace Tuat.Interfaces;

public interface IClipboardService
{
    void Copy(string source, Dictionary<Type, object?> content);
    Dictionary<Type, object?> Paste();
    bool CanPaste(string source);
}
