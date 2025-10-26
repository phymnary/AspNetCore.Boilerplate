using AspNetCore.Boilerplate.Api.Extensions;
using AspNetCore.Boilerplate.Tests.Api.Result;
using Xunit.Abstractions;

namespace AspNetCore.Boilerplate.Tests.Api;

public class ApiSchemaTests(ITestOutputHelper output)
{
    [Fact]
    public void route_pattern_name()
    {
        var endpoint = new GetCurrentResult(new SampleSingleton());
        output.WriteLine(endpoint.GetRoutePatternBasedOnNamespace("AspNetCore.Boilerplate.Tests"));
    }
}
