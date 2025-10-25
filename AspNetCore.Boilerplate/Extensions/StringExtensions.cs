using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace AspNetCore.Boilerplate.Extensions;

public static partial class StringExtensions
{
    public static string PascalToKebabCase(this string value)
    {
        return PascalToKebabMyRegex().Replace(value, "-$1").Trim().ToLower();
    }

    public static string[] SplitBySemicolon(this string value)
    {
        return value
            .Split(";", StringSplitOptions.RemoveEmptyEntries)
            .Select(it => it.Trim())
            .ToArray();
    }

    public static bool IsBlank([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static bool IsNotBlank(
        [NotNullWhen(true)] this string? value,
        [NotNullWhen(true)] out string? variable
    )
    {
        variable = value;
        return !string.IsNullOrWhiteSpace(value);
    }

    public static string? NullIfBlank(this string? text)
    {
        return text.IsBlank() ? null : text;
    }

    public static string StripPostfix(this string str, string postFix)
    {
        return str.EndsWith(postFix) ? str[..^postFix.Length] : str;
    }

    public static string StripPostfix(this string str, char postFix)
    {
        return str.EndsWith(postFix) ? str[..^1] : str;
    }

    public static string StripPrefix(this string str, string value)
    {
        if (str.StartsWith(value))
            str = str.Remove(0, value.Length);

        return str;
    }

    public static string StripPrefix(this string str, char value)
    {
        if (str.StartsWith(value))
            str = str.Remove(0, 1);

        return str;
    }

    public static bool IsTruthy(this string? text)
    {
        if (text.IsBlank())
            return false;

        text = text.ToUpperInvariant().Trim();

        return text is "TRUE" or "1" or "T";
    }

    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", RegexOptions.Compiled)]
    private static partial Regex PascalToKebabMyRegex();
}
