namespace AspNetCore.Boilerplate.Extensions;

public static class EnumerableExtensions
{
    public static string JoinAsString<T>(this IEnumerable<T> source, string separator)
    {
        return string.Join(separator, source);
    }

    public static string JoinAsString<T>(this IEnumerable<T> source, char separator)
    {
        return string.Join(separator, source);
    }

    public static int IndexOf<T>(this T[] source, T value)
    {
        return Array.IndexOf(source, value);
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
    {
        return source.Where(it => it is not null)!;
    }
}
