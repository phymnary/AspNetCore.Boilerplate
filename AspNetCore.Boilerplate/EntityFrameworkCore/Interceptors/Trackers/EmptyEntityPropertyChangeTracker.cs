using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AspNetCore.Boilerplate.EntityFrameworkCore.Interceptors.Trackers;

public class EmptyEntityPropertyChangeTracker : IEntityPropertyChangeTracker
{
    public ValueTask TrackAsync(EntityEntry entry, DateTimeOffset modifiedAt, CancellationToken ct)
    {
        return default;
    }
}
