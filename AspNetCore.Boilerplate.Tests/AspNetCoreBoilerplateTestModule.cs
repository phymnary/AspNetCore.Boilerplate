using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.Extensions;
using AspNetCore.Boilerplate.Tests.Auditing;
using AspNetCore.Boilerplate.Tests.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Boilerplate.Tests;

[Auto]
public partial class AspNetCoreBoilerplateTestModule : IAspModule
{
    public AspNetCoreBoilerplateTestModule()
    {
        DomainFeatureFlags.IsAuditingEnable = true;
        DomainFeatureFlags.IsMultiTenantEnable = true;
    }

    public void ConfigureServices(IServiceCollection services, IConfigurationManager configuration)
    {
        try
        {
            services.GetConfiguration();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        services
            .AddBoilerplateServices()
            .AddEfCoreServices<BookStoreDbContext>(configurator =>
                configurator.AddPropertyChangeAudit<BookStoreDbContext, AppPropertyChangeAudit>(
                    data => new AppPropertyChangeAudit
                    {
                        EntityName = data.EntityName,
                        PropertyName = data.PropertyName,
                        TypeName = data.TypeName,
                        EntityId = data.EntityId,
                        OldValue = data.OldValue,
                        NewValue = data.NewValue,
                        ModifiedById = data.ModifiedById,
                        ModifiedAt = data.ModifiedAt,
                        IsDeleted = data.IsDeleted,
                    }
                )
            );
    }
}
