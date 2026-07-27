# Phymnary.SugarPot.AspNetCore.EntityFrameworkCore

Entity Framework Core infrastructure for SugarPot applications.

This package provides:

- Generic EF repositories
- SaveChanges interceptors for auditing, soft delete, and tenant assignment
- ModelBuilder helpers for table naming and global query filters
- Transaction and resilient execution helpers

## What Is Included

### Repositories

- `EfRepository<TDbContext, TEntity>`
- `EfRepository<TDbContext, TEntity, TKey>`
- Query/update customization via `IRepositoryOptions<TEntity>`:
  - `EntityQueryOptions<TEntity>`
  - `EntityUpdateOptions<TEntity>`

### Interceptors

- `OnAttachedInterceptor` (always registered by `AddEfCoreServices`)
- `SoftDeleteInterceptor` (opt-in)
- `SetTenantOnSavingInterceptor` (opt-in)
- `AuditOnSavingInterceptor` (enabled through property-change audit registration)

### Helpers

- `ModelBuilderHelper` and `BuildEntity(...)`
- `IDbFunctionProvider` implementation (`DbFunctionProvider<TDbContext>`)
- `IQueryTransaction` wrapper (`WrappedDbContextTransaction`)

## Target Frameworks And EF Core Versioning

This project targets:

- net8.0
- net9.0
- net10.0

EF Core package version behavior in this project:

- For net10.0-compatible targets: `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.Relational` use `[10.0.0,)`
- Otherwise: the same packages use `[8.0.0,)`

## Installation

```bash
dotnet add package Phymnary.SugarPot.AspNetCore.EntityFrameworkCore
```

## Quick Start

### 1. Register SugarPot EF services

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Phymnary.SugarPot.AspNetCore.Extensions;

services.AddEfCoreServices<AppDbContext>(cfg =>
{
    cfg.AddSoftDelete();
    cfg.AddMultiTenancy();

    // Enables AuditOnSavingInterceptor and property-change tracking
    cfg.AddPropertyChangeAudit<AppDbContext, PropertyChangeAudit>(audit => new PropertyChangeAudit
    {
        EntityName = audit.EntityName,
        PropertyName = audit.PropertyName,
        TypeName = audit.TypeName,
        EntityId = audit.EntityId,
        OldValue = audit.OldValue,
        NewValue = audit.NewValue,
        ModifiedById = audit.ModifiedById,
        ModifiedAt = audit.ModifiedAt,
        IsDeleted = audit.IsDeleted,
    });
});

services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);

    // Add all registered EF interceptors
    options.AddInterceptors(sp.GetServices<IInterceptor>());
});
```

### 2. Inherit the repository base

```csharp
using Phymnary.SugarPot.AspNetCore.Entities;
using Phymnary.SugarPot.AspNetCore.Repositories;

public sealed class UserRepository(
    AppDbContext dbContext,
    IRepositoryOptions<User> options,
    EfRepositoryAddons addons
) : EfRepository<AppDbContext, User, Guid>(dbContext, options, addons)
{
}
```

Common methods:

- `InsertAsync`
- `UpsertAsync`
- `UpdateAsync`
- `FindAsync`
- `QueryAsync`
- `AnyAsync`
- `CountAsync`
- `AdvanceQuery(...)`
- `Delete(...)`
- `GetAsync(id)` for keyed repositories

## Repository Options

You can centralize entity behavior with `IRepositoryOptions<TEntity>`.

```csharp
using Phymnary.SugarPot.AspNetCore.Repositories;

public sealed class UserRepositoryOptions : EfRepositoryOptions<User>
{
    public UserRepositoryOptions()
    {
        QueryOptions = new EntityQueryOptions<User>
        {
            DefaultIncludeQuery = q => q,
            IncludeDetailsQuery = q => q
                .IncludeIn(u => u.Profile)
        };

        UpdateOptions = new EntityUpdateOptions<User>
        {
            Update = (input, existing) =>
            {
                existing.Name = input.Name;
                existing.Email = input.Email;
            },
        };

        // Optional domain validator
        Validator = null;
    }
}
```

Notes:

- `UpsertAsync` requires `UpdateOptions.Update`; otherwise it throws.
- `Delete(...)` first executes `UpdateOptions.OnDelete` when provided.
- If `OnDelete` returns `true`, default delete logic is skipped.

## Advanced Query API

`AdvanceQuery(...)` supports ordering, paging, projection, and pagination metadata.

```csharp
var page = await repository
    .AdvanceQuery(q => q.Where(x => x.IsActive))
    .OrderByDescending(x => x.CreatedAt)
    .Pick(perPage: 20, pageIndex: 1)
    .PaginateAsync(ct);
```

`PaginateAsync` returns:

- `Count`: total item count for the base filtered query
- `Items`: paged `IAsyncEnumerable<T>`

## ModelBuilder Helper

Use `ModelBuilderHelper` to keep entity mapping consistent.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    var helper = new ModelBuilderHelper(modelBuilder)
    {
        TenantIdAccessor = () => _currentTenant.Id!.Value,
    };

    helper
        .BuildEntity<User>(schema: "app")
        .BuildEntity<Order>(schema: "app");
}
```

Behavior:

- Table name defaults to CLR type name
- Applies soft-delete filter for entities implementing `ISoftDelete`
- Applies tenant filter for entities implementing `IMultiTenant`
  - On net10.0+, tenant accessor is required for multi-tenant entities

## Runtime Dependencies

When enabling features, make sure these services are available in DI from your application/domain layer:

- `ICurrentUser`
- `IRunAt`
- `IAbortedToken`
- `ICurrentTenant` (required when multi-tenancy is enabled)

## Notes

- `OnAttachedInterceptor` is always added by `AddEfCoreServices`.
- `AuditOnSavingInterceptor` is registered when `AddPropertyChangeAudit(...)` is configured.
- `ConfigureAuditing(...)` currently stores internal metadata used by this package.

## License

See the repository root for license details.
