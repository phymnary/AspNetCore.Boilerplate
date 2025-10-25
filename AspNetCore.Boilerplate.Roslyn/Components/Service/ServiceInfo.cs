namespace AspNetCore.Boilerplate.Roslyn.Components.Service;

internal sealed record ServiceInfo(
    string ServiceTypeName,
    string ImplementationTypeName,
    string Lifetime
);
