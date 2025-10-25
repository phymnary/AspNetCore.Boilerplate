using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.EntityFrameworkCore.Extensions;
using AspNetCore.Boilerplate.EntityFrameworkCore.Interceptors.Trackers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AspNetCore.Boilerplate.EntityFrameworkCore.Interceptors;

public class AuditOnSavingInterceptor(
    ICurrentUser currentUser,
    IEntityPropertyChangeTracker propertyChangeTracker
) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not { } dbContext)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var entry in dbContext.ChangeTracker.Entries<IAuditable>())
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(e => e.CreatedAt).CurrentValue = now;
                    entry.Property(e => e.CreatedById).CurrentValue = currentUser.Id.NullIfEmpty();
                    break;
                case EntityState.Modified:
                    await AuditChangesAsync(entry, now, cancellationToken);
                    break;
                case EntityState.Unchanged:
                    if (IsModified(entry))
                        await AuditChangesAsync(entry, now, cancellationToken);
                    break;
                case EntityState.Detached:
                case EntityState.Deleted:
                default:
                    break;
            }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task AuditChangesAsync(
        EntityEntry<IAuditable> entry,
        DateTime now,
        CancellationToken ct
    )
    {
        await propertyChangeTracker.TrackAsync(entry, now, ct);

        entry.Property(e => e.UpdatedAt).CurrentValue = now;
        entry.Property(e => e.UpdatedById).CurrentValue = currentUser.Id.NullIfEmpty();
    }

    private static bool IsModified(EntityEntry entry)
    {
        return entry.State == EntityState.Modified
            || entry.References.Any(refEntry =>
                refEntry.TargetEntry != null
                && refEntry.TargetEntry.Metadata.IsOwned()
                && (refEntry.IsModified || IsModified(refEntry.TargetEntry))
            );
    }
}
