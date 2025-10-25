using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Boilerplate;

public interface IAspModule
{
    public void ConfigureServices(IServiceCollection services, IConfigurationManager configuration);
}
