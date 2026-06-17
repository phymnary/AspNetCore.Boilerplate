using Phymnary.SugarPot.AspNetCore.Entities;

namespace Phymnary.SugarPot.AspNetCore.Extensions;

public static class EntityExtensions
{
    public static T Attach<T>(this ICollection<T> collection, T entity)
        where T : IEntity
    {
        collection.Add(entity);
        entity.DomainStatus.OnAttached();
        return entity;
    }
}
