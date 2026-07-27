# Phymnary.SugarPot.AspNetCore.Application

Application-layer runtime helpers and HTTP-aware exception types for SugarPot ASP.NET Core solutions.

This package adds a small runtime service registration (`IRunAt`) and a set of application exception types that expose HTTP status semantics through `IBusinessException`.

## Package scope

Root namespace:

`Phymnary.SugarPot.AspNetCore`

Primary areas:

- DI extension for application services
- Scoped runtime timestamp provider
- Application exception types for common HTTP outcomes

## Public API

### Service registration

- `ServiceCollectionExtensions.AddApplicationServices(IServiceCollection)`
  - Registers `IRunAt` as scoped.

### Runtime service

- `IRunAt` (from Domain package)
  - `DateTimeOffset Value { get; }`
  - Implementation (`RunAt`) is internal and returns `DateTimeOffset.UtcNow` at resolution time.

Because `IRunAt` is scoped, the same `Value` is reused within the same DI scope.

### Exception types

All exception types below:

- Derive from `Exception`
- Implement `IBusinessException` (through an internal `IApplicationException` marker)
- Expose `HttpStatusCode StatusCode`
- Support optional `ErrorCode` via `WithErrorCode(string code)`

Mappings:

- `AspBadRequestException` -> `400 BadRequest`
- `AspUnauthorizedException` -> `401 Unauthorized`
- `AspForbiddenEndpointException` -> `403 Forbidden`
- `AspInvalidOperationException` -> `422 UnprocessableEntity`
- `InternalServiceUnavailableException` -> `503 ServiceUnavailable`

## Installation

NuGet:

```bash
dotnet add package Phymnary.SugarPot.AspNetCore.Application
```

## Usage

### Register application services

```csharp
using Phymnary.SugarPot.AspNetCore.Extensions;

builder.Services.AddApplicationServices();
```

### Consume `IRunAt`

```csharp
using Phymnary.SugarPot.AspNetCore;

public sealed class RequestAuditService(IRunAt runAt)
{
    public DateTimeOffset RequestedAt => runAt.Value;
}
```

### Throw standardized application exceptions

```csharp
using Phymnary.SugarPot.AspNetCore.Exceptions;

if (string.IsNullOrWhiteSpace(name))
{
    throw new AspBadRequestException("Name is required")
        .WithErrorCode("APP_VALIDATION_NAME_REQUIRED");
}

if (!hasPermission)
{
    throw new AspForbiddenEndpointException("Permission denied")
        .WithErrorCode("APP_AUTH_FORBIDDEN");
}
```

## Error handling guidance

These exception types are intended to be translated by API middleware or exception handlers into HTTP responses. A common response payload includes:

- `status` from `StatusCode`
- `message` from `Exception.Message`
- optional `errorCode` from `ErrorCode`

## Target frameworks

Frameworks are inherited from the repository build configuration:

- `net8.0`
- `net9.0`
- `net10.0`

## Dependencies

- Project reference: `Phymnary.SugarPot.AspNetCore.Domain`
- Framework reference: `Microsoft.AspNetCore.App`

## Build metadata

Package version is sourced from `$(AspPackedVersion)`.

## License

See repository root for license details.
