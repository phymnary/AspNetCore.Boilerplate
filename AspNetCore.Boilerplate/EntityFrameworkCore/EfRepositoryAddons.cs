using AspNetCore.Boilerplate.Domain;

namespace AspNetCore.Boilerplate.EntityFrameworkCore;

public class EfRepositoryAddons(ICancellationTokenProvider cancellationTokenProvider)
{
    public ICancellationTokenProvider CancellationTokenProvider => cancellationTokenProvider;
}
