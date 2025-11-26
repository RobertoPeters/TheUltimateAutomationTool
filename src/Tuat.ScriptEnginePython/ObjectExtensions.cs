namespace Tuat.ScriptEnginePython;

public static class ObjectExtensions
{
    public static object? FromPyObject(this object? obj)
    {
        if (obj is Python.Runtime.PyObject pyObject)
        {
            return pyObject.ToObject();
        }
        return obj;
    }
}
