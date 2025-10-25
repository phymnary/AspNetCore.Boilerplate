using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AspNetCore.Boilerplate.EntityFrameworkCore.Interceptors.Trackers;

public interface IEntityPropertyChangeTracker
{
    ValueTask TrackAsync(EntityEntry entry, DateTimeOffset modifiedAt, CancellationToken ct);
}
