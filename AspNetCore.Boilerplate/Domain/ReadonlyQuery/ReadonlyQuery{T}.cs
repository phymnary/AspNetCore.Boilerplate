using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Boilerplate.Domain.ReadonlyQuery;

public class ReadonlyQuery<T>
{
    private readonly IQueryable<T> _queryable;
    private readonly ICancellationTokenProvider? _cancellationTokenProvider;

    internal ReadonlyQuery(
        IQueryable<T> queryable,
        ICancellationTokenProvider? cancellationTokenProvider
    )
    {
        _queryable = queryable;
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    private CancellationToken GetRequestAborted(CancellationToken cancellationToken)
    {
        return _cancellationTokenProvider?.GetRequestAborted(cancellationToken)
            ?? cancellationToken;
    }

    public ReadonlyQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> selector)
    {
        var query = _queryable.Select(selector);
        return new ReadonlyQuery<TTarget>(query, _cancellationTokenProvider);
    }

    public ReadonlyQuery<TTarget> Select<TTarget>(
        Expression<Func<T, TTarget>> selector,
        bool splitQuery
    )
        where TTarget : class
    {
        var query = _queryable.Select(selector);
        if (splitQuery)
            query = query.AsSplitQuery();
        return new ReadonlyQuery<TTarget>(query, _cancellationTokenProvider);
    }

    public IAsyncEnumerable<T> AsAsyncEnumerable()
    {
        return _queryable.AsAsyncEnumerable();
    }

    public Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        return _queryable.ToListAsync(GetRequestAborted(cancellationToken));
    }

    public Task<T[]> ToArrayAsync(CancellationToken cancellationToken = default)
    {
        return _queryable.ToArrayAsync(GetRequestAborted(cancellationToken));
    }

    public Task<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return _queryable.FirstOrDefaultAsync(GetRequestAborted(cancellationToken));
    }
}
