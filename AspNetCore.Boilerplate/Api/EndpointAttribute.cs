namespace AspNetCore.Boilerplate.Api;

/// <summary>
/// <para>Generate a partial class implement <see cref="AspNetCore.Boilerplate.Api.IEndpoint"/> that needs a HandleAsync method from base class</para>
/// <para>Route pattern is from string property name "RoutePattern" or group method that have <see cref="AspNetCore.Boilerplate.Api.RoutePatternAttribute">[RoutePatternAttribute]</see></para>
/// <para>Route builder can be decorated by group method that have <see cref="AspNetCore.Boilerplate.Api.RouteBuilderAttribute">[RouteBuilderAttribute]</see> or "BuildRoute" method.
/// The method must have one argument and return type of <see cref="Microsoft.AspNetCore.Builder.RouteHandlerBuilder"/>
/// </para>
/// <example>
/// <code lang="csharp">
/// [Endpoint]
/// public partial class GetBook
/// {
///     private static string RoutePattern => "/api/books";
///     private static async Task HandleAsync() {...}
///     private static RouteHandlerBuilder BuildRoute(RouteHandlerBuilder builder) {...}
/// }
/// </code>
/// </example>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class EndpointAttribute : Attribute
{
    public EndpointAttribute() { }

    /// <param name="method">Explicit tell generator what API method of RouteBuilder</param>
    public EndpointAttribute(Method method) { }
}
