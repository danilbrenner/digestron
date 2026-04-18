using Digestron.Service.Abstractions;
using Digestron.Service.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Digestron.Service;

public static class ServicesSetup
{
    public static IServiceCollection AddServices(this IServiceCollection services)
        => services.AddTransient<IEmailService, EmailService>();
}