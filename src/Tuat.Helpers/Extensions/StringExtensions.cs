namespace Tuat.Extensions;

public static class StringExtensions
{
    public static object? AutoConvert(this string? value)
    {
        if (value == null)
        {
            return null;
        }
        if (bool.TryParse(value, out var boolResult))
        {
            return boolResult;
        }
        if (int.TryParse(value, out var intResult))
        {
            return intResult;
        }
        if (double.TryParse(value, out var doubleResult))
        {
            return doubleResult;
        }
        return value;
    }
}
