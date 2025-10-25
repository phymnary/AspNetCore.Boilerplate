namespace AspNetCore.Boilerplate.Domain.Pagination;

public class PaginateResult<TEntity>
{
    public required int Count { get; init; }

    public required IAsyncEnumerable<TEntity> Items { get; init; }
}
