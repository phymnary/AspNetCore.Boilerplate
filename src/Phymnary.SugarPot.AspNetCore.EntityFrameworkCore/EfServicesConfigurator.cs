using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Phymnary.SugarPot.AspNetCore.Auditings;
using Phymnary.SugarPot.AspNetCore.Entities;
using Phymnary.SugarPot.AspNetCore.Interceptors;
using Phymnary.SugarPot.AspNetCore.Interceptors.Trackers;

namespace Phymnary.SugarPot.AspNetCore;

public class EfServicesConfigurator<TDbContext>
    where TDbContext : DbContext
{
    private readonly IServiceCollection _services;

    private bool _canAuditPropertyChange;

    private readonly EfAuditingStructure _auditingMetadata = new();

    private bool _canAudit;

    internal EfServicesConfigurator(IServiceCollection services)
    {
        _services = services;
        // Order so that OnAttachedInterceptor is always the first interceptor to be executed, ensuring that entities are properly attached before any other interceptor runs.
        _services.AddScoped<IInterceptor, OnAttachedInterceptor>();
    }

    private void RegisterAuditing()
    {
        if (_canAudit)
            return;

        _canAudit = true;
        _services.AddScoped<IInterceptor, AuditOnSavingInterceptor>();
    }

    public EfServicesConfigurator<TDbContext> ConfigureAuditing(
        Action<EfAuditingStructure> configure
    )
    {
        configure(_auditingMetadata);
        return this;
    }

    public EfServicesConfigurator<TDbContext> AddSoftDelete()
    {
        _services.AddScoped<IInterceptor, SoftDeleteInterceptor>();
        return this;
    }

    public EfServicesConfigurator<TDbContext> AddMultiTenancy()
    {
        _services.AddScoped<IInterceptor, SetTenantOnSavingInterceptor>();
        return this;
    }

    public EfServicesConfigurator<TDbContext> AddPropertyChangeAudit<TAuditDbContext, TAudit>(
        Func<IPropertyChangeAudit, TAudit> mapper
    )
        where TAuditDbContext : DbContext
        where TAudit : class, IPropertyChangeAudit, IEntity
    {
        RegisterAuditing();
        _canAuditPropertyChange = true;

        _services
            .AddScoped<
                IEntityPropertyChangeTracker,
                EntityPropertyChangeTracker<TAuditDbContext, TAudit>
            >()
            .AddSingleton(new AuditingEntityMapper<IPropertyChangeAudit, TAudit> { Map = mapper });

        _auditingMetadata.HasDifferentDbContextForAuditChanges =
            typeof(TAuditDbContext) != typeof(TDbContext);

        return this;
    }

    public IServiceCollection Build()
    {
        if (!_canAuditPropertyChange)
            _services.AddSingleton<IEntityPropertyChangeTracker>(
                new EmptyEntityPropertyChangeTracker()
            );

        _services.AddSingleton(_auditingMetadata);

        return _services;
    }
}
