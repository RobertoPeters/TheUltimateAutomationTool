using Tuat.Extensions;
using Tuat.Interfaces;

namespace Tuat;

public class ClipboardService : IClipboardService
{
    private string _source = string.Empty;
    private readonly Dictionary<Type, object?> _contents = [];

    public bool CanPaste(string source)
    {
        return _source == source;
    }

    public void Copy(string source, Dictionary<Type, object?> content)
    {
        _source = source;
        _contents.Clear();
        foreach (var item in content)
        {
            _contents.Add(item.Key, item.Value.CopyObject(item.Key));
        }
    }

    public Dictionary<Type, object?> Paste()
    {
        Dictionary<Type, object?> result = [];
        foreach (var item in _contents)
        {
            result.Add(item.Key, item.Value.CopyObject(item.Key));
        }
        return result;
    }
}
