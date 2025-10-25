using AspNetCore.Boilerplate.Extensions;

namespace AspNetCore.Boilerplate.Api.Extensions;

public static class EndpointExtensions
{
    public static string GetRoutePatternBasedOnNamespace<TEndpoint>(
        this TEndpoint _,
        string root,
        string prefix = ""
    )
        where TEndpoint : IEndpoint
    {
        var ns = typeof(TEndpoint).Namespace!.StripPrefix(root);

        ns = ns.StripPostfix(".POST")
            .StripPostfix(".GET")
            .StripPostfix(".DELETE")
            .StripPostfix(".PATCH")
            .StripPostfix(".PUT")
            .StripPostfix(".HEAD")
            .StripPostfix(".OPTIONS")
            .StripPostfix(".TRACE")
            .StripPostfix(".CONNECT");

        var routeName = string.Join(
            "/",
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
}
