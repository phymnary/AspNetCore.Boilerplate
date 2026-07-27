# Phymnary.SugarPot.AspNetCore.Domain

Shared domain contracts and primitives for SugarPot ASP.NET Core stacks.

This project contains contracts only (interfaces, attributes, DTO-like primitives, and exception types). It does not provide persistence, transport, or host-specific runtime implementations.

## Package scope

The root namespace is:

`Phymnary.SugarPot.AspNetCore`

Main groups:

- Domain/runtime context contracts
- Entity and validation primitives
- Repository and advanced query contracts
- Auditing contracts and metadata helpers
- Multi-tenancy and security context contracts
- Domain/business exception abstractions

## Contracts by area

### Runtime and scope contracts

- `IAbortedToken`
  - `CancellationToken Get(CancellationToken cancellationToken)`
- `IRunAt`
  - `DateTimeOffset Value { get; }`
- `IScopeBuilder`
  - `AsyncServiceScope Initialize(ScopeContext context)`
- `ScopeContext`
  - `CurrentUserId`, `CurrentTenantId`, `RequestAborted`
- `IDbFunctionProvider`
  - `BeginTransactionAsync(...)`
  - `UseResilientStrategyAsync(...)`
  - `UseResilientStrategyWithTransactionAsync(...)`

### Entity contracts

- `IEntity`
  - exposes `EntityDomainStatus DomainStatus`
- `Entity<TKey>`
  - base class with `[Key] TKey Id { get; protected init; }`
- `EntityDomainStatus`
  - `IsAdded`, `IsSoftDeleted`, `OnAttached()`, `SoftDelete()`
- `ISoftDelete`
  - `DeletedById`, `DeletedAt`, and default `Delete()` implementation that flags domain status

### Validation contracts

- `IEntityValidator<TEntity>`
  - `ValueTask<EntityValidationResult> ValidateAsync(...)`
- `EntityValidationResult`
  - `IsValid`, `Errors`, plus static `Valid`
- `EntityValidationFailureDetail`
  - `Property`, `Message`, optional `Code`

### Repository contracts

- `IRepository<TEntity>`
  - write methods: `InsertAsync`, `UpsertAsync`, `UpdateAsync`, `Delete`
  - read methods: `FindAsync`, `QueryAsync`, `AnyAsync`, `CountAsync`
  - advanced query entry point: `AdvanceQuery(...)`
- `IRepository<TEntity, TKey>`
  - adds `GetAsync(TKey id, ...)`
- `IQueryTransaction`
  - transaction lifecycle and savepoint-related API

### Advanced query contracts

- `IAdvanceOrderBuilding<T>`
- `IAdvancePageBuilding<T>`
- `IAdvanceSelectableBuilding<T>`
- `IAdvanceQueryBuilder<T>`
- `PaginateResult<TEntity>`

The flow is designed as a staged builder:

1. Order (`OrderBy` / `OrderByDescending`)
2. Page (`Pick`)
3. Optional projection (`Select`)
4. Execute (`PaginateAsync` or `Build`)

### Auditing contracts

- `IAuditable`
  - audit identity (`GetAuditKey`) and created/updated fields
- `IPropertyChangeAudit`
  - immutable shape for property change records
- `AuditingAttribute`
  - class-level include list of auditable properties
- `DisabledAuditingAttribute`
  - class/property-level opt-out
- `EntityPropertyAuditingMetadata`
  - computes if a property can be audited via `CanAudit(...)`
- `AuditingEntityMapper<TConcrete, TImplement>`
  - mapping hook via `Func<TConcrete, TImplement>`
- `TrackBy`
  - `Domain` or `Database`

### Multi-tenancy and security contracts

- `IMultiTenant`
  - `Guid TenantId { get; set; }`
- `ICurrentTenant`
  - `Guid? Id { get; }`
- `ICurrentUser`
  - `Guid? Id { get; }`

### Exception contracts and types

- `IBusinessException`
  - `HttpStatusCode StatusCode`, optional `ErrorCode`
- `IDomainException : IBusinessException`

Provided domain exception classes:

- `DomainNotImplementedException` (`422 UnprocessableContent`)
- `EntityNotFoundException` (`404 NotFound`)
- `EntityValidationException` (`400 BadRequest`, includes `Failures`)
- `EntityPersistenceException` (`409 Conflict`)
- `TenantMissingInContextException` (`403 Forbidden`)

Error code defaults are configurable globally through `DomainErrorCodeRegistry`.

## Extension helpers

- `EntityExtensions.Attach(...)`
  - Adds an entity to an `ICollection<T>` and marks `DomainStatus.IsAdded`
- `ServiceProviderExtensions.InheritAsyncServiceScope(...)`
  - Builds `ScopeContext` from current user/tenant/aborted token services and initializes a new async scope

Note: `ServiceProviderExtensions` is declared in namespace `Phymnary.SugarPot.AspNetCore.Api.Extensions`.

## Installation

NuGet:

```bash
dotnet add package Phymnary.SugarPot.AspNetCore.Domain
```

## Usage examples

### Define an entity

```csharp
using Phymnary.SugarPot.AspNetCore.Entities;

public sealed class User : Entity<Guid>, ISoftDelete
{
    public User(Guid id) : base(id) { }

    public string Name { get; set; } = string.Empty;

    public Guid? DeletedById { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
}
```

### Attach child entity and mark as added

```csharp
using Phymnary.SugarPot.AspNetCore.Entities;
using Phymnary.SugarPot.AspNetCore.Extensions;

var addresses = new List<Address>();
var address = addresses.Attach(new Address(Guid.NewGuid()));

// address.DomainStatus.IsAdded == true
```

### Implement entity validation

```csharp
using Phymnary.SugarPot.AspNetCore.Entities;

public sealed class UserValidator : IEntityValidator<User>
{
    public ValueTask<EntityValidationResult> ValidateAsync(
        User entity,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            return ValueTask.FromResult(new EntityValidationResult
            {
                IsValid = false,
                Errors =
                [
                    new EntityValidationFailureDetail
                    {
                        Property = nameof(User.Name),
                        Message = "Name is required",
                        Code = "USR_NAME_REQUIRED"
                    }
                ]
            });
        }

        return ValueTask.FromResult(EntityValidationResult.Valid);
    }
}
```

### Throw standardized domain exceptions

```csharp
using Phymnary.SugarPot.AspNetCore.Exceptions;

throw new EntityNotFoundException("User not found")
    .WithErrorCode("USR_NOT_FOUND");
```

## Design intent

- Keep this package implementation-agnostic.
- Place EF Core, database, messaging, and host-specific logic in other packages.
- Use these contracts to keep domain and application layers stable and testable.

## Build metadata

Version is provided via `$(AspPackedVersion)` from the parent build configuration.

## License

See the repository root for license details.
