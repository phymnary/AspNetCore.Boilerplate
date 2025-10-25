using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AspNetCore.Boilerplate.EntityFrameworkCore.Interceptors;

public class SetTenantOnSavingInterceptor(ICurrentTenant currentTenant) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default
    )
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, ct);

        var tenantId =
            currentTenant.Id ?? throw new AspUnauthorizedException("Missing tenant id in scope");

        foreach (var entry in eventData.Context.ChangeTracker.Entries<IMultiTenant>())
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(e => e.TenantId).CurrentValue = tenantId;
                    break;
                case EntityState.Modified:
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                default:
                    break;
            }

        return base.SavingChangesAsync(eventData, result, ct);
    }
}
