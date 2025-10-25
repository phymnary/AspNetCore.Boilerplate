namespace AspNetCore.Boilerplate.Api;

/// <summary>
/// Mark a static method (one <see cref="IEndpoint"/> parameter) is a route pattern for endpoint group or a property for that endpoint instant
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class RoutePatternAttribute : Attribute;
