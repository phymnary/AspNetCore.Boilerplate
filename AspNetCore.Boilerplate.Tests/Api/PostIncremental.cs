using AspNetCore.Boilerplate.Api;

namespace AspNetCore.Boilerplate.Tests.Api;

[Endpoint]
public partial class PostIncremental
{
    private static string RoutePattern => "/incremental";

    private static Task<int> HandleAsync()
    {
        return Task.FromResult(1);
    }
}
