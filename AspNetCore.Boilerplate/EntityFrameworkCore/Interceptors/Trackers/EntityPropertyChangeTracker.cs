using System.Text.Json;
using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.Domain.Auditing;
using AspNetCore.Boilerplate.EntityFrameworkCore.Extensions;
using AspNetCore.Boilerplate.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AspNetCore.Boilerplate.EntityFrameworkCore.Interceptors.Trackers;

public class EntityPropertyChangeTracker<TAuditDbContext, TAudit>(
    TAuditDbContext auditDbContext,
    ICurrentUser currentUser,
    AuditingMetadata auditingMetadata,
    AuditingEntityMapper<PropertyChangeAudit, TAudit> mapper
) : IEntityPropertyChangeTracker
    where TAuditDbContext : DbContext
    where TAudit : PropertyChangeAudit
{
    private class PropertyChangeAuditData : PropertyChangeAudit;

    private class Context
    {
        public required string EntityId { get; init; }

        public required string EntityName { get; init; }

        public required DateTimeOffset ModifiedAt { get; init; }

        public required PropertyAuditing PropertyAuditing { get; init; }
    }

    public async ValueTask TrackAsync(
        EntityEntry entry,
        DateTimeOffset modifiedAt,
        CancellationToken ct
    )
    {
        var entity = (IEntity)entry.Entity;

        var metadata = auditingMetadata.GetPropertyAuditing(entity.GetType());
        if (!metadata.IsAuditEnabled)
            return;

        var changes = TrackModifyProperties(
            new Context
            {
                EntityId = JsonSerializer.Serialize(entity.Key()),
                EntityName = auditingMetadata.TrackBy switch
                {
                    TrackBy.Database => entry.Metadata.GetTableName()!,
                    TrackBy.Domain => entity.GetType().Name,
                    _ => throw new NotSupportedException("Not support this TrackBy value"),
                },
                ModifiedAt = modifiedAt,
                PropertyAuditing = metadata,
            },
            entry
        );

        auditDbContext.Set<TAudit>().AddRange(changes);

        if (auditingMetadata.HasDifferentDbContextForAuditChanges)
        {
            await auditDbContext.SaveChangesAsync(ct);
        }
    }

    private static bool NotEquals(object? val1, object? val2)
    {
        return !Equals(val1, val2);
    }

    private TAudit Map(
        Context context,
        Microsoft.EntityFrameworkCore.Metadata.IProperty propertyMetadata,
        string ownedFrom,
        object? originalValue,
        object? currentValue
    )
    {
        return mapper.Map(
            new PropertyChangeAuditData
            {
                EntityId = context.EntityId,
                EntityName = context.EntityName,
                PropertyName = auditingMetadata.TrackBy switch
                {
                    TrackBy.Database => propertyMetadata.GetColumnName(),
                    TrackBy.Domain => ownedFrom + propertyMetadata.Name,
                    _ => throw new NotSupportedException("Not support this TrackBy value"),
                },
                TypeName = propertyMetadata.ClrType.ShortDisplayName(),
                OldValue = JsonSerializer.Serialize(originalValue),
                NewValue = JsonSerializer.Serialize(currentValue),
                ModifiedById = currentUser.Id.NullIfEmpty(),
                ModifiedAt = context.ModifiedAt,
            }
        );
    }

#pragma warning disable EF1001

    private IEnumerable<TAudit> TrackModifyProperties(
        Context context,
        EntityEntry? entry,
        InternalEntityEntry? infrastructure = null,
        string ownedBy = "",
        bool isModify = false
    )
    {
        if (!ownedBy.IsBlank() && isModify && infrastructure is not null)
        {
            EntityEntry? prev = null;
            if (infrastructure.SharedIdentityEntry is { } shared)
            {
                prev = infrastructure
                    .Context.ChangeTracker.Entries()
                    .FirstOrDefault(it => it.Entity == shared.Entity);
            }

            var properties = prev?.Properties ?? entry?.Properties;
            if (properties is not null)
                foreach (
                    var removedOwnedEntityAudit in properties
                        .Select(it => new
                        {
                            it.Metadata,
                            prev?.Properties.First(p => p.Metadata == it.Metadata).OriginalValue,
                            entry?.Properties.First(p => p.Metadata == it.Metadata).CurrentValue,
                        })
                        .Where(property =>
                            context.PropertyAuditing.IsValid(ownedBy + property.Metadata.Name)
                            && NotEquals(property.OriginalValue, property.CurrentValue)
                        )
                        .Select(property =>
                            Map(
                                context,
                                property.Metadata,
                                ownedBy,
                                property.OriginalValue,
                                property.CurrentValue
                            )
                        )
                )
                    yield return removedOwnedEntityAudit;
        }

        if (entry is null)
            yield break;

        foreach (
            var propertyAudit in entry
                .Properties.Where(property =>
                    property.IsModified
                    && context.PropertyAuditing.IsValid(property.Metadata.Name)
                    && NotEquals(property.OriginalValue, property.CurrentValue)
                )
                .Select(property =>
                    Map(
                        context,
                        property.Metadata,
                        ownedBy,
                        property.OriginalValue,
                        property.CurrentValue
                    )
                )
        )
            yield return propertyAudit;

        foreach (
            var ownedEntityAudit in entry
                .References.Where(refEntry => refEntry.TargetEntry?.Metadata.IsOwned() ?? false)
                .SelectMany(refEntry =>
                    TrackModifyProperties(
                        context,
                        refEntry.TargetEntry,
                        refEntry.IsModified ? refEntry.TargetEntry?.GetInfrastructure() : null,
                        ownedBy + refEntry.Metadata.Name + ".",
                        refEntry.IsModified
                    )
                )
        )
            yield return ownedEntityAudit;
    }
}
#pragma warning restore EF1001
