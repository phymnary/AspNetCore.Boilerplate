using Microsoft.EntityFrameworkCore;
using Phymnary.SugarPot.AspNetCore.Exceptions;
using Phymnary.SugarPot.AspNetCore.Repositories;

namespace Phymnary.SugarPot.AspNetCore;

public class EfDbStateManager
{
    internal bool IsExecutionStrategyInTransaction { get; private set; }

    internal async Task ExecuteStrategyInTransaction(
        Func<CancellationToken, Task> execution,
        CancellationToken cancellationToken
    )
    {
        IsExecutionStrategyInTransaction = true;
        try
        {
            await execution(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new EntityPersistenceException(
                "Db throw exception when was executing strategy",
                ex
            );
        }
        finally
        {
            IsExecutionStrategyInTransaction = false;
        }
    }
}

internal class DbFunctionProvider<TDbContext>(
    TDbContext dbContext,
    IAbortedToken ctProvider,
    EfDbStateManager dbStateManager
) : IDbFunctionProvider
    where TDbContext : DbContext
{
    public async Task<IQueryTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default
    )
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(
            ctProvider.Get(cancellationToken)
        );
        return new WrappedDbContextTransaction(transaction, ctProvider);
    }

    public async Task UseResilientStrategyAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default
    )
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(
            async ct =>
            {
                await operation(ct);
            },
            ctProvider.Get(cancellationToken)
        );
    }

    public async Task UseResilientStrategyWithTransactionAsync(
        Func<CancellationToken, Task> operation,
        Func<CancellationToken, Task<bool>> verifySucceeded,
        CancellationToken cancellationToken = default
    )
    {
        await dbStateManager.ExecuteStrategyInTransaction(
            async (ct) =>
            {
                var strategy = dbContext.Database.CreateExecutionStrategy();
                await strategy.ExecuteInTransactionAsync(
                    async aborted =>
                    {
                        await operation(aborted);
                    },
                    verifySucceeded,
                    ct
                );
                dbContext.ChangeTracker.AcceptAllChanges();
            },
            ctProvider.Get(cancellationToken)
        );
    }
}
