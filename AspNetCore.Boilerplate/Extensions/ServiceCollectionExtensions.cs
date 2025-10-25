using AspNetCore.Boilerplate.Api;
using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.Domain.Auditing;
using AspNetCore.Boilerplate.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspNetCore.Boilerplate.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureSection<TConfig>(
        this IServiceCollection services,
        Action<TConfig>? setupWithConfig = null
    )
        where TConfig : class, IAppSettingsSection
    {
        var configurations = services.GetConfiguration();
        var configureSection = configurations.GetSection(TConfig.Section);
        services = services.Configure<TConfig>(configureSection);
        setupWithConfig?.Invoke(configureSection.Get<TConfig>()!);

        return services;
    }

    /// <summary>
    /// Default Implementation for <see cref="ICurrentUser"/>, <see cref="ICurrentTenant"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddBoilerplateServices(this IServiceCollection services)
    {
        return services
            .AddScoped<ICurrentUser, HttpContextCurrentUser>()
            .AddScoped<ICurrentTenant, HttpContextCurrentTenant>()
            .AddScoped<EfRepositoryAddons>()
            .AddScoped<ICancellationTokenProvider, HttpContextCancellationTokenProvider>()
            .AddProblemDetails()
            .AddExceptionHandler<AspExceptionHandler>();
    }

    public static IServiceCollection AddEfCoreServices<TDbContext>(
        this IServiceCollection services,
        Action<EfServicesConfigurator<TDbContext>> configure
    )
        where TDbContext : DbContext
    {
        var configurator = new EfServicesConfigurator<TDbContext>(services);
        configure(configurator);

        return UnderlayAddEfServices(configurator);
    }

    public static IServiceCollection AddEfCoreServices<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        var configurator = new EfServicesConfigurator<TDbContext>(services);

        return UnderlayAddEfServices<TDbContext>(configurator);
    }

    private static IServiceCollection UnderlayAddEfServices<TDbContext>(
        EfServicesConfigurator<TDbContext> configurator
    )
        where TDbContext : DbContext
    {
        return configurator
            .Build()
            .AddScoped<IDbFunctionProvider, DbFunctionProvider<TDbContext>>();
    }

    internal static IConfiguration GetConfiguration(this IServiceCollection services)
    {
        return services.GetConfigurationOrNull()
            ?? throw new Exception(
                $"Could not find an implementation of {typeof(IConfiguration).AssemblyQualifiedName} in the service collection."
            );
    }

    private static IConfiguration? GetConfigurationOrNull(this IServiceCollection services)
    {
        var hostBuilderContext = services.GetSingletonInstanceOrNull<HostBuilderContext>();
        return hostBuilderContext?.Configuration != null
            ? hostBuilderContext.Configuration
            : services.GetSingletonInstanceOrNull<IConfiguration>();
    }

    private static T? GetSingletonInstanceOrNull<T>(this IServiceCollection services)
    {
        return (T?)
            services
                .FirstOrDefault(d => d.ServiceType == typeof(T))
                ?.NormalizedImplementationInstance();
    }
}
