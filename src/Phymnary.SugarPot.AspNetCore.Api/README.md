# Phymnary.SugarPot.AspNetCore.Api

ASP.NET Core API primitives and runtime helpers for SugarPot.

This package provides:

- Endpoint abstractions for minimal API mapping.
- Attribute contracts used by the companion Roslyn generator.
- Request-context bindings for current user, current tenant, and aborted token.
- A JSON exception handler compatible with ASP.NET Core exception handling middleware.

## Installation

```bash
dotnet add package Phymnary.SugarPot.AspNetCore.Api
```

## What Is Included

- `IEndpoint`: endpoint abstraction that returns a `RouteHandlerBuilder`.
- `MapEndpoint<TEndpoint>()`: maps endpoint classes to an `IEndpointRouteBuilder`.
- `[Endpoint]`, `[RoutePattern]`, `[RouteBuilder]`, `[ApiSchema]`: attributes consumed by the companion generator/analyzers.
- `AddApiServices()`: registers default API-scoped runtime providers.
- `UseBoilerplateServices()`: binds user, tenant, and aborted token data from `HttpContext`.
- `AddBoilerplateExceptionHandler()`: registers `AspExceptionHandler` and `ProblemDetails` services.

## Quick Start

### 1) Register services

```csharp
using Phymnary.SugarPot.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices()
    .AddBoilerplateExceptionHandler();
```

### 2) Build app and enable middleware

```csharp
var app = builder.Build();

app.UseExceptionHandler();
app.UseBoilerplateServices();
```

### 3) Map endpoint

```csharp
using Phymnary.SugarPot.AspNetCore.Api.Extensions;

app.MapEndpoint<GetHealth>();

app.Run();
```

## Endpoint Pattern

Use `[Endpoint]` on a partial class and provide members expected by the generator (for example `HandleAsync`, optional `RoutePattern`, optional `BuildRoute`).

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Phymnary.SugarPot.AspNetCore.Api;

[Endpoint(Method.Get)]
public partial class GetHealth
{
    private static string RoutePattern => "/health";

    private static IResult HandleAsync()
    {
        return Results.Ok(new { ok = true });
    }

    private static RouteHandlerBuilder BuildRoute(RouteHandlerBuilder builder)
    {
        return builder.WithTags("Health");
    }
}
```

Notes:

- `MapEndpoint<TEndpoint>()` supports:
  - `where TEndpoint : class, IEndpoint, new()` (activates via `new()`).
  - `where TEndpoint : class, IEndpoint` with a provided `IServiceProvider` (resolves from DI).
- If an endpoint also implements `IExtendRouteBuilder`, its `Extend(...)` hook runs after route mapping.

## Group Route Configuration Attributes

Use shared static methods to decorate groups of endpoints.

### Shared route pattern

```csharp
using Phymnary.SugarPot.AspNetCore.Api;

public static class UserRouteConfig
{
    [RoutePattern]
    public static string GetRoutePattern<TEndpoint>(TEndpoint endpoint)
        where TEndpoint : class, IEndpoint
    {
        return "/api/users";
    }
}
```

### Shared route builder

```csharp
using Microsoft.AspNetCore.Builder;
using Phymnary.SugarPot.AspNetCore.Api;

public static class UserRouteBuilderConfig
{
    [RouteBuilder]
    public static RouteHandlerBuilder Build(RouteHandlerBuilder builder)
    {
        return builder.RequireAuthorization();
    }
}
```

Per-endpoint `RoutePattern` and `BuildRoute` members override group-level behavior.

## Runtime Request Context Binding

`UseBoilerplateServices()` populates scoped services from each request:

- `IAbortedToken`: set from `HttpContext.RequestAborted`.
- `ICurrentUser.Id`: parsed from user claim named `sub` by default.
- `ICurrentTenant.Id`: parsed from user claim named `tid` by default.

Customize claim names through static properties:

```csharp
using Phymnary.SugarPot.AspNetCore.Extensions;

WebApplicationBuilderExtensions.SubClaimName = "sub";
WebApplicationBuilderExtensions.TenantClaimName = "tenant";
```

## Exception Handling

`AddBoilerplateExceptionHandler()` configures:

- `AddProblemDetails()`.
- `AddExceptionHandler<AspExceptionHandler>()`.

`AspExceptionHandler` behavior:

- Maps `IBusinessException` to its `StatusCode` and `ErrorCode`.
- Handles `EntityValidationException` and includes validation failures.
- Falls back to HTTP 500 for unexpected exceptions.
- Writes JSON payload:

```json
{
  "error": {
    "message": "...",
    "code": "...",
    "detail": "...",
    "invalidParameters": []
  }
}
```

If `IAspErrorMessageProvider` is registered, it is used to resolve localized/user-friendly messages by error code.

## Utility Helpers

`GetRoutePatternBasedOnNamespace<TEndpoint>(root, prefix)` converts endpoint namespace segments into kebab-case route segments and supports dynamic segments wrapped by underscores.

Example segment conversion:

- Namespace segment `Orders` -> `orders`
- Namespace segment `_Id_` -> `{id}`

## Source Generator Packaging

In release packaging, the companion Roslyn assembly is packed into `analyzers/dotnet/cs`, so consumers receive analyzer/source-generator behavior automatically through the NuGet package.

## Target Frameworks

Build outputs in this project currently include:

- `net8.0`
- `net9.0`
- `net10.0`

Final target framework values are defined by project/solution build properties.

## License

See the repository root for license information.
