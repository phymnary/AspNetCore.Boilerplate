namespace AspNetCore.Boilerplate.Api;

/// <summary>
/// Mark a static method is a route builder for endpoint group or an instant method for that endpoint instant
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RouteBuilderAttribute : Attribute;
