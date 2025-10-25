using System.Linq.Expressions;
using AspNetCore.Boilerplate.Domain.Pagination;
using AspNetCore.Boilerplate.Domain.ReadonlyQuery;
using AspNetCore.Boilerplate.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Boilerplate.Domain;

public interface IRepository<TEntity>
    where TEntity : class, IEntity
{
    internal DbSet<TEntity> DbSet { get; }

    internal IQueryable<TEntity> Queryable(bool canIncludeDetails = false);

    /// <summary>
    /// Insert entity to database context
    /// </summary>
    /// <param name="entity">Entity to be inserted.</param>
    /// <param name="autoSave">Call SaveChanges or not.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">Database exceptions.</exception>
    /// <exception cref="EntityValidationException">Throw by validator, run before entity got added to DbContext so failed entity won't populate EF tracker.</exception>
    Task<TEntity> InsertAsync(
        TEntity entity,
        bool autoSave = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Insert entity to database context
    /// </summary>
    /// <param name="entity">Entity to be inserted or updated if existed.</param>
    /// <param name="on">Expression to check if entity is existed or not</param>
    /// <param name="autoSave">Call SaveChanges or not.</param>
    /// <param name="canIncludeDetails">Call IncludeDetails.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">Database exceptions.</exception>
    /// <exception cref="EntityValidationException">Throw by validator.</exception>
    Task<TEntity> UpsertAsync(
        TEntity entity,
        Expression<Func<TEntity, bool>> on,
        bool autoSave = true,
        bool canIncludeDetails = false,
        CancellationToken cancellationToken = default
    );

    ///<summary>
    /// Validate entity then SaveChanges
    /// </summary>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">Database exceptions.</exception>
    /// <exception cref="EntityValidationException">Throw by validator.</exception>
    Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool canIncludeDetails = false,
        CancellationToken cancellationToken = default
    );

    Task<List<TEntity>> QueryAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool canIncludeDetails = false,
        CancellationToken cancellationToken = default
    );

    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    IPaginateOrderBuilding<TEntity> Paginate(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? filter = null,
        bool? canIncludeDetails = null,
        CancellationToken cancellationToken = default
    );

    ReadonlyQuery<TEntity> ReadonlyQuery(
        Expression<Func<TEntity, bool>> predicate,
        bool? canIncludeDetails = null
    );
}
