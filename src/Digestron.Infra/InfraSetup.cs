using Digestron.Infra.Digest;
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
            .Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName))
            .AddSingleton<IEmailProvider, GraphEmailProvider>()
            .AddSingleton<IDigestService, OpenAiDigestService>();
}

