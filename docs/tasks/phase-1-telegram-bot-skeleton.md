# Phase 1: Telegram Bot Skeleton

- [ ] **p1-nuget-packages** — Add NuGet packages for Telegram bot  
  Add `Telegram.Bot` to `Directory.Packages.props` and reference it from `Digestron.Hosting`. Also add Serilog packages: `Serilog`, `Serilog.Extensions.Hosting`, `Serilog.Sinks.Console`, `Serilog.Sinks.ApplicationInsights`.

- [ ] **p1-bot-config** — Configure Telegram bot settings  
  Add `TelegramBotOptions` class with `BotToken` property. Wire up configuration in `appsettings.json` (placeholder) and `Program.cs`. Token must be read from environment/Azure App Settings, never hardcoded.

- [ ] **p1-bot-client** — Register Telegram bot client  
  Register `ITelegramBotClient` as a singleton in DI using `TelegramBotOptions.BotToken`. Register a hosted service (`BotPollingService`) that starts long-polling for updates using the Worker pattern.

- [ ] **p1-command-dispatcher** — Implement command dispatcher  
  Create `UpdateHandler` that receives Telegram `Update` objects and dispatches to command handlers based on `Message.Text`. Should handle unknown commands gracefully.

- [ ] **p1-commands-basic** — Implement /start, /help, /digest, /unread commands  
  `/start`: welcome message. `/help`: lists available commands. `/digest`: placeholder "Fetching digest..." reply. `/unread`: placeholder "Fetching unread count..." reply.

- [ ] **p1-serilog** — Configure Serilog  
  Set up Serilog in `Program.cs` with console sink and structured logging. Wire up with `ILogger<T>` via `Microsoft.Extensions.Logging`. Leave Application Insights sink stub for Phase 5 NFRs.
