using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Boilerplate.EntityFrameworkCore.Extensions;

public static class RepositoryExtensions
{
    public static async Task<TEntity> GetAsync<TEntity, TId>(
        this IRepository<TEntity> repository,
        TId id,
        bool canIncludeDetails = false,
        CancellationToken cancellationToken = default
    )
        where TEntity : Entity<TId>
        where TId : IComparable, IComparable<TId>, IEquatable<TId>
    {
        return await repository
                .Queryable(canIncludeDetails)
                .FirstOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken)
            ?? throw new EntityNotFoundException(
                $"Could not find entity {typeof(TEntity).Name} with id {id}"
            );
    }
}
