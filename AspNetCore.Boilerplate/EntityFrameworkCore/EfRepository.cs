using System.Linq.Expressions;
using AspNetCore.Boilerplate.Application.Extensions;
using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.Domain.Pagination;
using AspNetCore.Boilerplate.Domain.ReadonlyQuery;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Boilerplate.EntityFrameworkCore;

public abstract class EfRepository<TDbContext, TEntity>(
    TDbContext dbContext,
    EfRepositoryAddons addons,
    IRepositoryOptions<TEntity> options
) : IRepository<TEntity>
    where TEntity : class, IEntity
    where TDbContext : DbContext
{
    public DbSet<TEntity> DbSet { get; } = dbContext.Set<TEntity>();

    private static EntityQueryOptions<TEntity>? _fallbackQueryOptions;
    private readonly EntityQueryOptions<TEntity> _queryOptions =
        options.QueryOptions ?? (_fallbackQueryOptions ??= new EntityQueryOptions<TEntity>());

    private static EntityUpdateOptions<TEntity>? _fallbackUpdateOptions;
    private readonly EntityUpdateOptions<TEntity> _updateOptions =
        options.UpdateOptions ?? (_fallbackUpdateOptions ??= new EntityUpdateOptions<TEntity>());

    protected CancellationToken GetRequestAborted(CancellationToken cancellationToken)
    {
        return addons.CancellationTokenProvider.GetRequestAborted(cancellationToken);
    }

    private Task ValidateAsync(TEntity entity, CancellationToken ct)
    {
        return options.Validator?.ValidateAndThrowOnErrorsAsync(
                entity,
                $"Fail when validate {typeof(TEntity).Name}",
                ct
            ) ?? Task.CompletedTask;
    }

    public async Task<TEntity> InsertAsync(
        TEntity entity,
        bool autoSave = true,
        CancellationToken cancellationToken = default
    )
    {
        var ct = GetRequestAborted(cancellationToken);

        await ValidateAsync(entity, ct);
        var inserted = DbSet.Add(entity).Entity;

        if (autoSave)
            await dbContext.SaveChangesAsync(ct);

        return inserted;
    }

    public async Task<TEntity> UpsertAsync(
        TEntity entity,
        Expression<Func<TEntity, bool>> on,
        bool autoSave = true,
        bool canIncludeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        var ct = GetRequestAborted(cancellationToken);
        var upsert = await Queryable(canIncludeDetails).FirstOrDefaultAsync(on, ct);

        if (upsert is not null)
        {
            _updateOptions.Run(entity, upsert);
            await ValidateAsync(upsert, ct);
        }
        else
        {
            await ValidateAsync(entity, ct);
            upsert = await InsertAsync(entity, false, ct);
        }

        if (autoSave)
            await dbContext.SaveChangesAsync(ct);

        return upsert;
    }

    public async Task<int> UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        var ct = GetRequestAborted(cancellationToken);
        await ValidateAsync(entity, ct);

        return await dbContext.SaveChangesAsync(ct);
    }

    public async Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool canIncludeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        var ct = GetRequestAborted(cancellationToken);
        return await Queryable(canIncludeDetails).FirstOrDefaultAsync(predicate, ct);
    }

    public async Task<List<TEntity>> QueryAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool canIncludeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        var ct = GetRequestAborted(cancellationToken);
        if (predicate == null)
            return await Queryable(canIncludeDetails).ToListAsync(ct);

        return await Queryable(canIncludeDetails).Where(predicate).ToListAsync(ct);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        var ct = GetRequestAborted(cancellationToken);
        return await DbSet.AnyAsync(predicate, ct);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        var ct = GetRequestAborted(cancellationToken);
        return await DbSet.CountAsync(predicate, ct);
    }

    public IPaginateOrderBuilding<TEntity> Paginate(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? filter = null,
        bool? canIncludeDetails = null,
        CancellationToken cancellationToken = default
    )
    {
        var ct = GetRequestAborted(cancellationToken);
        var dbSet = canIncludeDetails is { } include ? Queryable(include) : DbSet;
        var queryable = filter is not null ? filter(dbSet) : dbSet;
        return new PaginateQueryBuilder<TEntity>(() => queryable.CountAsync(ct), queryable);
    }

    public IQueryable<TEntity> Queryable(bool canIncludeDetails = false)
    {
        var queryable = _queryOptions.DefaultIncludeQuery.Invoke(DbSet);
        if (canIncludeDetails)
            queryable = _queryOptions.IncludeDetailsQuery.Invoke(queryable);

        return queryable;
    }

    public ReadonlyQuery<TEntity> ReadonlyQuery(
        Expression<Func<TEntity, bool>> predicate,
        bool? canIncludeDetails = null
    )
    {
        var queryable = canIncludeDetails is { } useIncludeOptions
            ? Queryable(useIncludeOptions)
            : DbSet;

        return new ReadonlyQuery<TEntity>(
            queryable.Where(predicate).AsNoTracking(),
            addons.CancellationTokenProvider
        );
    }
}
