using Digestron.Hosting;
using Digestron.Hosting.Options;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

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

    builder.Services.AddSingleton<UpdateHandler>();
    builder.Services.AddHostedService<BotPollingService>();

    var host = builder.Build();
    host.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException && ex.GetType().Name != "StopTheHostException")
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}