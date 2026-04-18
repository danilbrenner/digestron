using Digestron.Domain;
using Digestron.Infra.Email;
using Digestron.Infra.Options;
using Digestron.Service.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Digestron.Infra;

public static class InfraSetup
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
        => services
            .Configure<GraphOptions>(configuration.GetSection(GraphOptions.SectionName))
            .AddSingleton<IEmailProvider, GraphEmailProvider>();
}

