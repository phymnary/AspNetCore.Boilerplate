using AspNetCore.Boilerplate.Api;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Boilerplate.Tests.Api;

[Endpoint]
public partial class PostIncremental
{
    private static string RoutePattern => "/incremental";

    private static Task HandleAsync([FromBody] int added)
    {
        Result.GetCurrentResult.Value += added;
        return Task.CompletedTask;
    }
}
