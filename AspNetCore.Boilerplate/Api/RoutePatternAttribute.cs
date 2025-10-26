namespace AspNetCore.Boilerplate.Api;

/// <summary>
/// Mark a static method as route pattern builder for a group of endpoints. <br/>
/// Override on each endpoint with a member string property named RoutePattern.
/// <example>
///     <code lang='cs'>
///     // Static method to get route pattern for every endpoint in group
///     [RoutePattern]
///     public static string GetRouteName&lt;TEndpoint&gt;(TEndpoint endpoint)
///         where TEndpoint : class, global::AspNetCore.Boilerplate.Api.IEndpoint
///     {
///         return typeof(TEndpoint).Name;
///     }
///
///     // Member router pattern string property
///     private static string RoutePattern => "/api/foo"
///     </code>
/// </example>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class RoutePatternAttribute : Attribute;
