using AspNetCore.Boilerplate.Api;

namespace AspNetCore.Boilerplate.Tests.Api;

[Endpoint]
internal partial class GetCurrentResult(SampleSingleton sampleSingleton)
{
    private static string RoutePattern => "/result";

    private static Task<int> HandleAsync()
    {
        return Task.FromResult(100);
    }
}
