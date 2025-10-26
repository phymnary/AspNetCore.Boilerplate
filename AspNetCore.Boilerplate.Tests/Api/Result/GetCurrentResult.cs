using AspNetCore.Boilerplate.Api;

namespace AspNetCore.Boilerplate.Tests.Api.Result;

[Endpoint]
[Service(Lifetime.Transient, IsSelf = true)]
internal partial class GetCurrentResult(SampleSingleton sampleSingleton)
{
    internal static int Value { get; set; }

    private static Task<int> HandleAsync()
    {
        return Task.FromResult(Value);
    }
}
