using Microsoft.EntityFrameworkCore.Storage;

namespace AspNetCore.Boilerplate.EntityFrameworkCore;

public interface IDbFunctionProvider
{
    Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default
    );

    void ClearTracking();
}
