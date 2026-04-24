# Digestron - Architecture

## Overview

Digestron is organized into **four layered projects** following a clean architecture pattern:

- **Digestron.Domain** - Core business entities and domain logic
- **Digestron.Service** - Application services and abstractions
- **Digestron.Infra** - External service integrations (Microsoft Graph, OpenAI, Data)
- **Digestron.Hosting** - ASP.NET Core entry point and Telegram orchestration

## Project Structure

```
src/
├── Digestron.Domain/               # Business entities and domain logic (no dependencies)
├── Digestron.Service/              # Business logic & abstractions
│   ├── Abstractions/               # Service interfaces
│   ├── Services/
│   └── ServicesSetup.cs            # DI registration
├── Digestron.Infra/                # Infrastructure & integrations
│   ├── Email/                      # GraphEmailProvider (Microsoft Graph)
│   ├── Options/
│   └── InfraSetup.cs               # DI registration
└── Digestron.Hosting/              # ASP.NET Core host & Telegram bot
    ├── Handler/                    # UpdateHandler, MessageResponder
    ├── Options/
    └── Program.cs                  # Entry point, DI composition
tests/
└── Digestron.Tests/                # Unit tests
    ├── Domain/                     # Value semantics of domain records
    ├── Service/                    # EmailService business logic
    └── Hosting/                    # UpdateHandler routing and ParseUpdate parsing
```

## Dependency Flow

```
Domain (no dependencies)
    ↑
Service (depends on Domain)
    ↑
Infra (depends on Domain, Service)
    ↑
Hosting (depends on all layers)
```

## Component Interactions

### 1. Application Startup (Program.cs)

1. **Bootstrap Logging** - Initialize Serilog before the host starts
2. **Build WebApplication** - Create ASP.NET Core builder
3. **Register Services**:
   - Serilog logging (configured from appsettings)
   - Telegram bot token from configuration
   - Microsoft Graph provider setup
   - Business logic services
   - `IMessageResponder` implementation
   - `UpdateHandler` for processing updates
   - `BotPollingService` as a hosted service
   - Health check endpoint
4. **Start Host** - Run the web app with background polling service

### 2. Message Polling & Routing (BotPollingService)

```
┌─────────────────────────────────────────────────────────┐
│         BotPollingService (IHostedService)              │
│  Runs in background alongside ASP.NET Core              │
└─────────────────────────────────────────────────────────┘
              ↓
      StartAsync() called
              ↓
   botClient.StartReceiving()
         (long-polling)
              ↓
   Telegram updates → UpdateHandler.HandleUpdateAsync()
              ↓
   HandlePollingErrorAsync() for errors
```

### 3. Update Processing (UpdateHandler)

```
Telegram Update
    ↓
UpdateHandler.HandleUpdateAsync()
    ↓
ParseUpdate() - Extract command, text, MessageContext
    ↓
Switch on command:
    ├─ "/start"   → messageResponder.SendStartMessageAsync()
    ├─ "/help"    → messageResponder.SendHelpMessageAsync()
    ├─ "/digest"  → messageResponder.SendDigestLoadingMessageAsync()
    ├─ "/unread"  → emailService.HandleGetUnreadEmailCountAsync()
    └─ other      → messageResponder.SendUnknownCommandMessageAsync()
```

### 4. Email Service Flow (IEmailService)

```
EmailService
    ↓
GetUnreadEmailsAsync()
    ↓
GraphEmailProvider.GetUnreadEmailsAsync()
    ↓
Microsoft Graph API
    ├─ First call: Device Code authentication
    ├─ User signs in at provided URL
    └─ Subsequent calls: Use cached GraphServiceClient
    ↓
Extract email data (Subject, Sender, ReceivedAt, BodyPreview)
    ↓
Return EmailMessage[] records
```

### 5. Authentication (Device Code Flow)

Microsoft Graph uses **Device Code flow** for user sign-in:

```
First time user sends /unread:
    ↓
    Device code request to Microsoft
    ↓
    sendDeviceAuthenticationRequest()
        Displays URL and code to user
    ↓
    User opens URL, enters code, signs in
    ↓
    Microsoft returns access token
    ↓
    Token cached in ConcurrentDictionary<ChatId, GraphServiceClient>
    ↓
    Subsequent API calls use cached token
```

- **Client ID only** (no client secret needed - public client)
- **Tokens are in-memory** (cleared on bot restart)
- **TenantId = "common"** (personal + work accounts)
- **Delegated permission: Mail.Read**

## Key Abstractions & Extensibility

- **IEmailProvider** - Abstract email source (enables Gmail, IMAP later)
- **IMessageResponder** - Abstract Telegram communication
- **IEmailService** - High-level email operations

This design allows adding new email providers or messaging channels without modifying service logic.

## Configuration

### appsettings.json (Production)
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      }
    ]
  }
}
```
- **JSON logging** for structured logs (easier for monitoring tools)

### appsettings.Development.json (Local)
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console"
        }
      }
    ]
  }
}
```
- **Colored console output** for easier local debugging

### Secrets (User Secrets in Development)
```bash
dotnet user-secrets set "TelegramBot:BotToken" "<token>"
dotnet user-secrets set "Graph:ClientId" "<client-id>"
dotnet user-secrets set "OpenAi:ApiKey" "<openai-api-key>"
```

### Azure App Settings (Production)
- `TelegramBot__BotToken` - Telegram bot API token
- `Graph__ClientId` - Microsoft Entra ID app client ID
- `OpenAi__ApiKey` - OpenAI API key
- `OpenAi__Model` - OpenAI model (default: `gpt-4o-mini`)

## Logging Strategy

- **Framework**: Serilog
- **Sinks**: Console (local)
- **Context**: Structured logs with chat ID, user ID, message content
- **Levels**: Information (default), Error, Fatal
- **Format**: Plain text (dev) or JSON (production)

## Health Check

- **Endpoint**: `GET /health`
- **Framework**: ASP.NET Core minimal APIs
- **Purpose**: Container orchestration (Kubernetes, Docker Swarm)
- **Response**: JSON with status and timestamp

## Request Lifecycle

```
1. User sends message to bot (Telegram)
   ↓
2. BotPollingService receives update
   ↓
3. UpdateHandler routes to command handler
   ↓
4. Command handler (e.g., EmailService) executes business logic
   ↓
5. GraphEmailProvider calls Microsoft Graph API
   ↓
6. Data returned as EmailMessage records
   ↓
7. MessageResponder sends Telegram response to user
   ↓
8. Serilog logs the entire flow with structured context
```

## Design Patterns Used

| Pattern | Location | Purpose |
|---------|----------|---------|
| **Dependency Injection** | Program.cs | Loose coupling, testability |
| **Hosted Service** | BotPollingService | Long-running background work |
| **Provider Pattern** | IEmailProvider | Abstraction for extensibility |
| **Repository Pattern** | IEmailProvider | Data access abstraction |
| **Decorator/Extension** | UpdateHandlerExtensions | Add parsing behavior |
| **Options Pattern** | TelegramBotOptions, GraphOptions | Configuration management |
| **Record Types** | EmailMessage, MessageContext | Immutable data structures |

## Error Handling

- **Telegram Errors**: `UpdateHandler.HandlePollingErrorAsync()` logs and continues
- **Graph Errors**: Device code flow re-prompts on auth failure
- **Host Errors**: Caught at top-level, logged as fatal, graceful shutdown
- **No retries**: Polling is continuous; errors don't stop the service

## Testing

### Stack

| Library | Role |
|---------|------|
| **xUnit** | Test runner and assertion framework |
| **AutoFixture** | Generates randomized, realistic test data to eliminate manual fixture setup |
| **Moq** | Mocks interfaces and verifies interactions |

### Conventions

- **Test class naming**: `<SubjectUnderTest>Tests` (e.g. `EmailServiceTests`)
- **Method naming**: `<Method>_<Scenario>_<ExpectedOutcome>`
- AutoFixture generates `MessageContext`, `EmailMessage`, and other data objects; no hand-crafted fixtures
- Moq mocks all interfaces (`IEmailProvider`, `IMessageResponder`, `IEmailService`); arrange–act–assert structure throughout
- Tests do not cover language features such as record equality, `with` expressions, or inheritance — these are compiler guarantees, not application logic.

### Layer-by-Layer Guidance

#### Domain

Domain records have no dependencies.

#### Service

Services are tested in isolation with dependencies mocked. AutoFixture creates input data. Scenarios focus on verifying the correct responder method is called with the correct arguments.

#### Hosting

`UpdateHandler.HandleUpdateAsync` is tested by constructing `Telegram.Bot.Types.Update` objects and verifying the correct service or responder method is called via Moq. `UpdateHandlerExtensions.ParseUpdate` is tested as a pure function. Key scenarios: each known command routes to the right dependency; unrecognised commands fall through to `SendUnknownCommandMessageAsync`; updates without `Message` or `Text` are silently dropped.

### What Is Not Unit-Tested

- **`GraphEmailProvider`** — tightly coupled to the Microsoft Graph SDK; covered by integration tests.
- **`BotPollingService`** — wraps Telegram SDK lifecycle; covered by end-to-end or smoke tests.
- **`Program.cs`** — DI composition root; validated by the production build.

## Future Extensibility

### Adding Gmail Support
1. Create `GmailEmailProvider : IEmailProvider`
2. Register in `InfraSetup.cs`
3. No changes needed in `UpdateHandler` or other layers

### Adding Button Actions
1. Create handler for `UpdateType.CallbackQuery`
2. Register in `BotPollingService.ReceiverOptions`
3. Route in `UpdateHandler`

### Adding Digest Generation (AI)
1. Create `IDigestService` abstraction
2. Implement with OpenAI
3. Register and inject into email commands
