using AspNetCore.Boilerplate.Api;
using AspNetCore.Boilerplate.Api.Extensions;
using Microsoft.AspNetCore.Builder;

namespace AspNetCore.Boilerplate.Tests.Api;

public class ApiGroupConfigurator
{
    [RoutePattern]
    public static string GetRouteName<TEndpoint>(TEndpoint endpoint)
        where TEndpoint : class, IEndpoint
    {
        return endpoint.GetRoutePatternBasedOnNamespace("AspNetCore.Boilerplate.Tests.Api");
    }

    [RouteBuilder]
    public static RouteHandlerBuilder GroupBuildRoute(RouteHandlerBuilder builder)
    {
        return builder.WithGroupName("testing");
    }
}
