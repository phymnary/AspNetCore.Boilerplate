namespace AspNetCore.Boilerplate.Api;

/// <summary>
/// Mark a static method as route builder for a group of endpoints.<br/>
/// Override on each endpoint with a member method named BuildRoute.
/// <example>
///     <code lang='cs'>
///     // Static method for group builder
///     [RouteBuilder]
///     public static RouteHandlerBuilder GroupBuildRoute(RouteHandlerBuilder builder)
///     {
///         return builder.RequireAuthorization();
///     }
///
///     // Member builder method on each endpoint
///     public RouteHandlerBuilder BuildRoute(RouteHandlerBuilder routeBuilder)
///     {
///         return routeBuilder;
///     }
///     </code>
/// </example>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RouteBuilderAttribute : Attribute;
