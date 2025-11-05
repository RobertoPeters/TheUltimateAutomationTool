namespace Tuat.Variables;

public static class ObjectExtensions
{
    public static TResult? CopyObjectToOtherType<T, TResult>(this T target) where TResult : new()
    {
        var result = default(TResult);
        if (target != null)
        {
            var s = System.Text.Json.JsonSerializer.Serialize(target);
            if (s != null)
            {
                result = (TResult?)System.Text.Json.JsonSerializer.Deserialize(s, typeof(TResult));
            }
        }
        return result;
    }
}

