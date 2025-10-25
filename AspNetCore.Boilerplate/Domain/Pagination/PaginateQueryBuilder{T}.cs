using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Boilerplate.Domain.Pagination;

internal class PaginateQueryBuilder<T>(Func<Task<int>> countFunc, IQueryable<T> queryable)
    : IPaginateOrderBuilding<T>,
        IPaginatePageBuilding<T>,
        IPaginateResult<T>
{
    private IQueryable<T> _queryable = queryable;

    public IPaginatePageBuilding<T> OrderBy<TProperty>(
        Expression<Func<T, TProperty>> propertyAccessor
    )
    {
        _queryable = _queryable.OrderBy(propertyAccessor);
        return this;
    }

    public IPaginatePageBuilding<T> OrderByDescending<TProperty>(
        Expression<Func<T, TProperty>> propertyAccessor
    )
    {
        _queryable = _queryable.OrderByDescending(propertyAccessor);
        return this;
    }

    public IPaginateResult<T> Page(int page, int perPage)
    {
        if (page > 0)
            _queryable = _queryable.Skip((page - 1) * perPage);

        if (perPage != int.MaxValue)
            _queryable = _queryable.Take(perPage);

        return this;
    }

    public IPaginateResult<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> selector)
    {
        return new PaginateQueryBuilder<TTarget>(countFunc, _queryable.Select(selector));
    }

    public async Task<PaginateResult<T>> BuildAsync()
    {
        return new PaginateResult<T>
        {
            Count = await countFunc(),
            Items = _queryable.AsAsyncEnumerable(),
        };
    }

    public IAsyncEnumerable<T> Build()
    {
        return _queryable.AsAsyncEnumerable();
    }
}

public interface IPaginateOrderBuilding<T>
{
    IPaginatePageBuilding<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> propertyAccessor);

    IPaginatePageBuilding<T> OrderByDescending<TProperty>(
        Expression<Func<T, TProperty>> propertyAccessor
    );
}

public interface IPaginatePageBuilding<T>
{
    IPaginateResult<T> Page(int page, int perPage);
}

public interface IPaginateResult<T>
{
    IPaginateResult<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> selector);

    Task<PaginateResult<T>> BuildAsync();

    IAsyncEnumerable<T> Build();
}
