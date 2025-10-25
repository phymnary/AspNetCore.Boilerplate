using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.Domain.Auditing;
using AspNetCore.Boilerplate.EntityFrameworkCore.Interceptors;
using AspNetCore.Boilerplate.EntityFrameworkCore.Interceptors.Trackers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Boilerplate.EntityFrameworkCore;

public class EfServicesConfigurator<TDbContext>
    where TDbContext : DbContext
{
    private readonly IServiceCollection _services;

    private bool _canAuditPropertyChange;

    private readonly AuditingMetadata _auditingMetadata = new();

    internal EfServicesConfigurator(IServiceCollection services)
    {
        if (DomainFeatureFlags.IsMultiTenantEnable)
            services.AddScoped<IInterceptor, SetTenantOnSavingInterceptor>();

        if (DomainFeatureFlags.IsAuditingEnable)
            services.AddScoped<IInterceptor, AuditOnSavingInterceptor>();

        _services = services;
    }

    public EfServicesConfigurator<TDbContext> ConfigureAuditing(Action<AuditingMetadata> configure)
    {
        configure(_auditingMetadata);
        return this;
    }

    public EfServicesConfigurator<TDbContext> AddPropertyChangeAudit<TAuditDbContext, TAudit>(
        Func<PropertyChangeAudit, TAudit> mapper
    )
        where TAuditDbContext : DbContext
        where TAudit : PropertyChangeAudit, IEntity
    {
        _canAuditPropertyChange = true;
        _services
            .AddScoped<
                IEntityPropertyChangeTracker,
                EntityPropertyChangeTracker<TAuditDbContext, TAudit>
            >()
            .AddSingleton(new AuditingEntityMapper<PropertyChangeAudit, TAudit> { Map = mapper });

        _auditingMetadata.HasDifferentDbContextForAuditChanges =
            typeof(TAuditDbContext) != typeof(TDbContext);

        return this;
    }

    internal IServiceCollection Build()
    {
        if (!_canAuditPropertyChange)
            _services.AddSingleton<IEntityPropertyChangeTracker>(
                new EmptyEntityPropertyChangeTracker()
            );

        _services.AddSingleton(_auditingMetadata);

        return _services;
    }
}
