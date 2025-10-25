using System.Collections.Immutable;
using System.Reflection;
using AspNetCore.Boilerplate.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Boilerplate.Domain.Auditing;

public class AuditingMetadata
{
    private readonly Dictionary<Type, PropertyAuditing> _cachePropertyAuditing = new();

    internal bool HasDifferentDbContextForAuditChanges { get; set; }

    public TrackBy TrackBy { get; set; }

    private static IEnumerable<string> DisabledAuditPropertyNames(
        IEnumerable<PropertyInfo> propertyInfos,
        string? ownedBy = null
    )
    {
        var owned = ownedBy.IsNotBlank(out var parentName) ? parentName + "." : string.Empty;

        foreach (var propertyInfo in propertyInfos)
        {
            var type = propertyInfo.PropertyType;
            if (type.IsClass)
            {
                if (type.HasAttribute<OwnedAttribute>())
                {
                    foreach (
                        var disabled in DisabledAuditPropertyNames(
                            type.GetProperties(),
                            owned + propertyInfo.Name
                        )
                    )
                    {
                        yield return disabled;
                    }
                }
                else if (propertyInfo.HasAttribute<DisableAuditingAttribute>())
                {
                    if (type.IsAssignableTo(typeof(IEntity)))
                    {
                        yield return owned + propertyInfo.Name + "Id";
                    }
                    if (type == typeof(string))
                        yield return owned + propertyInfo.Name;
                }
                continue;
            }

            if (propertyInfo.HasAttribute<DisableAuditingAttribute>())
                yield return owned + propertyInfo.Name;
        }
    }

    internal PropertyAuditing GetPropertyAuditing(Type entityType)
    {
        if (_cachePropertyAuditing.TryGetValue(entityType, out var value))
            return value;

        value = new PropertyAuditing
        {
            IsAuditEnabled = !entityType.HasAttribute<DisableAuditingAttribute>(),
            ValidAuditProperties =
                entityType.GetCustomAttribute<AuditedAttribute>()?.Properties.ToImmutableArray()
                ?? [],
            IgnoreAuditProperties =
            [
                "CreatedAt",
                "CreatedById",
                "UpdatedAt",
                "UpdatedById",
                "TenantId",
                .. DisabledAuditPropertyNames(entityType.GetProperties()),
            ],
        };

        _cachePropertyAuditing[entityType] = value;

        return value;
    }
}
