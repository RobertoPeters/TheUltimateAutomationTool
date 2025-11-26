using Python.Runtime;
namespace Tuat.ScriptEnginePython;

public static class PyObjectExtensions
{
    public static object? ToObject(this PyObject? obj)
    {
        object? result = obj;
        if (obj != null)
        {
            try
            {
                if (obj == PyObject.None)
                {
                    result = null;
                }
                else 
                {
                    using var pyType = obj.GetPythonType();
                    if (pyType.Name == "str")
                    {
                        result = obj.As<string>();
                    }
                    else if (pyType.Name == "int")
                    {
                        result = obj.As<long>();
                    }
                    else if (pyType.Name == "float")
                    {
                        result = obj.As<double>();
                    }
                    else if (pyType.Name == "bool")
                    {
                        result = obj.As<bool>();
                    }
                    else if (pyType.Name == "dict")
                    {
                        var structString = obj.ToString()!.Replace("'", "\"");
                        result = System.Text.Json.JsonSerializer.Deserialize<dynamic>(structString!);
                    }
                }
            }
            finally
            {
                obj?.Dispose();
            }
        }
        return result;
    }
}
