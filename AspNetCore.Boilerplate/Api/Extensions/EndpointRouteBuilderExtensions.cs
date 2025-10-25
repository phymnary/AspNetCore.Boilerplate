using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Boilerplate.Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : class, IEndpoint, new()
    {
        var endpoint = new TEndpoint();
        var builder = endpoint.ConfigureRouteBuilder(app);

        if (endpoint is IExtendRouteBuilder extendRouteBuilder)
            extendRouteBuilder.Extend(builder);

        return app;
    }

    public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(
        this IEndpointRouteBuilder app,
        IServiceProvider serviceProvider
    )
        where TEndpoint : class, IEndpoint
    {
        var endpoint = serviceProvider.GetRequiredService<TEndpoint>();
        var builder = endpoint.ConfigureRouteBuilder(app);

        if (endpoint is IExtendRouteBuilder extendRouteBuilder)
            extendRouteBuilder.Extend(builder);

        return app;
    }
}
