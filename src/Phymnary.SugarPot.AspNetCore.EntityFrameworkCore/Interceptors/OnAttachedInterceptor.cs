using Microsoft.EntityFrameworkCore.Diagnostics;
using Phymnary.SugarPot.AspNetCore.Entities;

namespace Phymnary.SugarPot.AspNetCore.Interceptors;

public class OnAttachedInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not { } dbContext)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (
            var entry in dbContext
                .ChangeTracker.Entries<IEntity>()
                .Where(entry => entry.Entity.DomainStatus.IsAdded)
        )
        {
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Added;
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
