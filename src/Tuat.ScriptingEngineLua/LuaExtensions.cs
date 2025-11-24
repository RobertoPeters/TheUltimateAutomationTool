using Lua;

namespace Tuat.ScriptingEngineLua;

public static class LuaExtensions
{
    public static object? ToObject(this LuaValue luaValue)
    {
        return luaValue.Type switch
        {
            LuaValueType.String => luaValue.Read<string>(),
            LuaValueType.Nil => null,
            LuaValueType.Number => luaValue.ToNumber(),
            _ => null
        };
    }

    private static object? ToNumber(this LuaValue luaValue)
    {
        if (luaValue.TryRead<int>(out var intResult))
        {
            return intResult;
        }
        if (luaValue.TryRead<double>(out var doubleResult))
        {
            return doubleResult;
        }
        if (luaValue.TryRead<long>(out var longResult))
        {
            return longResult;
        }
        if (luaValue.TryRead<float>(out var floatResult))
        {
            return floatResult;
        }
        return null;
    }
}
