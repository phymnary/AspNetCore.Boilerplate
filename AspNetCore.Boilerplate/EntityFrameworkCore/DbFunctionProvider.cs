using AspNetCore.Boilerplate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AspNetCore.Boilerplate.EntityFrameworkCore;

public class DbFunctionProvider<TDbContext>(
    TDbContext dbContext,
    ICancellationTokenProvider ctProvider
) : IDbFunctionProvider
    where TDbContext : DbContext
{
    public Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default
    )
    {
        return dbContext.Database.BeginTransactionAsync(
            ctProvider.GetRequestAborted(cancellationToken)
        );
    }

    public void ClearTracking()
    {
        dbContext.ChangeTracker.Clear();
    }
}
