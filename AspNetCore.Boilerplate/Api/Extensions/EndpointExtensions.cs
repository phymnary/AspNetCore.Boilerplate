using AspNetCore.Boilerplate.Extensions;

namespace AspNetCore.Boilerplate.Api.Extensions;

public static class EndpointExtensions
{
    private static readonly string[] HttpMethodPostfixes =
    [
        ".POST",
        ".GET",
        ".DELETE",
        ".PATCH",
        ".PUT",
        ".HEAD",
        ".OPTIONS",
        ".TRACE",
        ".CONNECT",
    ];

    public static string GetRoutePatternBasedOnNamespace<TEndpoint>(
        this TEndpoint _,
        string root,
        string prefix = ""
    )
        where TEndpoint : IEndpoint
    {
        var ns = typeof(TEndpoint).Namespace!.StripPrefix(root);

        foreach (var postfix in HttpMethodPostfixes)
        {
            if (ns.TryStripPostfix(postfix, out ns))
                break;
        }

        var routeName = string.Join(
            '/',
            ns.Split(".", StringSplitOptions.RemoveEmptyEntries)
                .Select(static item =>
                {
                    var path = item;
                    var isDynamic = false;

                    if (item.StartsWith('_') && item.EndsWith('_'))
                    {
                        isDynamic = true;
                        path = path[1..^1];
                    }

                    path = path.PascalToKebabCase().ToLower();
                    return isDynamic ? "{" + path + "}" : path;
                })
        );

        return $"{prefix}/{routeName}";
    }

    private static bool TryStripPostfix(this string str, string postFix, out string result)
    {
        if (str.EndsWith(postFix))
        {
            result = str[..^postFix.Length];
            return true;
        }

        result = str;
        return false;
    }
}
