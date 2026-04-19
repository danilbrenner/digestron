using Digestron.Hosting;
using Digestron.Hosting.Options;
using Digestron.Domain;
using Digestron.Hosting.Handler;
using Digestron.Infra;
using Digestron.Service;
using Digestron.Service.Abstractions;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, loggerConfig) => loggerConfig
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.Configure<TelegramBotOptions>(
        builder.Configuration.GetSection(TelegramBotOptions.SectionName));

    builder.Services.AddSingleton<ITelegramBotClient>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
        return new TelegramBotClient(options.BotToken);
    });

    builder.Services
        .AddInfra(builder.Configuration)
        .AddServices()
        .AddSingleton<IMessageResponder, MessageResponder>()
        .AddSingleton<UpdateHandler>()
        .AddHostedService<BotPollingService>()
        .AddHealthChecks();

    var app = builder.Build();

    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException && ex.GetType().Name != "StopTheHostException")
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}