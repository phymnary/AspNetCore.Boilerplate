using AspNetCore.Boilerplate.Roslyn.Components.Endpoint;
using AspNetCore.Boilerplate.Roslyn.Components.Service;
using Microsoft.CodeAnalysis;

namespace AspNetCore.Boilerplate.Roslyn.Diagnostics;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor InvalidClassDecorationForDependency = new(
        "ASPB0001",
        "Invalid Class Decoration for [Dependency] to be registered",
        "The Dependency {0} could not be registered as a dependency because it's a abstract or generic class",
        nameof(ServiceGenerator),
        DiagnosticSeverity.Error,
        true,
        "Class annotated with [Dependency] must not be abstract or generic.",
        ""
    );

    public static readonly DiagnosticDescriptor MissingHandleMethodForEndpoint = new(
        "ASPB0002",
        "Missing \"HandleAsync\" method for [Endpoint]",
        "The Endpoint {0} is missing \"HandleAsync\" method",
        nameof(EndpointGenerator),
        DiagnosticSeverity.Error,
        true,
        "Class annotated with [Endpoint] must have a method named \"HandleAsync\".",
        ""
    );
}
