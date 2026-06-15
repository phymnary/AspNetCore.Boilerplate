using Microsoft.Extensions.DependencyInjection;
using Phymnary.SugarPot.AspNetCore.MultiTenancy;
using Phymnary.SugarPot.AspNetCore.Security;

namespace Phymnary.SugarPot.AspNetCore.Api.Extensions;

public static class ServiceProviderExtensions
{
    public static AsyncServiceScope InheritAsyncServiceScope(this IServiceProvider provider)
    {
        var context = new ScopeContext
        {
            CurrentUserId = provider.GetService<ICurrentUser>() is { } currentUser
                ? currentUser.Id
                : null,
            CurrentTenantId = provider.GetService<ICurrentTenant>() is { } currentTenant
                ? currentTenant.Id
                : null,
            RequestAborted = provider.GetService<IAbortedToken>() is { } abortedToken
                ? abortedToken.Get(default)
                : null,
        };

        return provider.GetRequiredService<IScopeBuilder>().Initialize(context);
    }
}
