# Digestron - Project Specification

## Goal
Build a Telegram bot that fetches unread Outlook emails, creates AI-powered digests using OpenAI, and allows marking emails as read.

## Core User Flows
- User sends /digest → bot shows a clean summary of unread emails (grouped by priority/action items/newsletters).
- Bot offers buttons to mark suggested low-priority emails as read.
- Commands: /start, /help, /digest, /unread, /reload-prompt.

## Phases (Incremental)
Phase 1: Telegram bot skeleton with basic commands
Phase 2: Microsoft Graph integration to load unread emails
Phase 3: OpenAI integration for generating digests
Phase 4: System prompt management (file, caching, hot-reload)
Phase 5: Button actions + marking as read
Phase 6: Edit messages in place instead of sending new ones
Phase 7: Scheduled digest delivery (twice-daily push to all registered chats)
Phase 8: Future – Gmail support via IEmailProvider interface

## Non-Functional Requirements
- Run on Azure Web App (Container) — deployed as a Docker image published to a container registry
- Send only minimal data to OpenAI: subject, sender, received date, and short bodyPreview (max 300 characters per email)
- All secrets (Telegram token, OpenAI API key) must be stored in Azure Key Vault or Azure App Settings (never in code)
- Keep costs low (GPT-4o-mini)
- Easy to extend for other email providers
- Use Pull Telegram updates (no webhooks) for simplicity
- Use Serilog for logging (console), with structured logs
- Expose health check endpoint at `/health` using ASP.NET Core minimal APIs for container orchestration (Kubernetes, Docker Swarm)
- The OpenAI system prompt must live in a dedicated `system-prompt.md` file (embedded resource in `Digestron.Infra`), not hardcoded in source; cached after first load; can be overridden at runtime via `OpenAi__SystemPromptPath` (Docker volume mount) without rebuilding the image
- Log total OpenAI token usage (`TotalTokenCount`) after each digest call at `Information` level for cost monitoring; also surface it to the user as a footer in the Telegram digest message

## Authentication Strategy
- **Microsoft Graph**: Device Code flow (`DeviceCodeCredential`) for user sign-in
  - Works with both personal outlook.com and work/school Microsoft 365 accounts
  - User signs in via browser at device code URL, bot receives token automatically
  - Tokens are in-memory only; bot requests fresh token on restart
  - Only requires storing `ClientId` (no client secret)
  - App registration: `TenantId = "common"`, delegated `Mail.Read` permission, public client flows enabled

## Success Criteria for MVP
- I can talk to @DigestronBot and get a readable digest of my unread emails
- I can mark emails as read from the bot

## Scheduled Digest Delivery

### Overview
Twice a day, a background job automatically pushes a digest to every chat that has an active Microsoft Graph connection (i.e., has completed Device Code authentication). No new commands or explicit registration are required.

### What "Registered" Means
A chat is considered registered if and only if it has a live, authenticated `GraphServiceClient` in the in-memory authentication cache. Chats that have not signed in, or whose bot session has been restarted and not re-authenticated, are silently skipped.

### Schedule
- Two fixed delivery times per day, configurable via `Schedule:DeliveryTimesUtc` (default: `["08:00", "18:00"]`).
- The scheduler operates in UTC.
- Times are validated at startup; an invalid value logs an error and falls back to the defaults.

### Delivery Flow
1. `ScheduledDigestService` (IHostedService) wakes at each configured delivery time.
2. It queries `IEmailProvider` for all chat IDs that currently hold an authenticated session.
3. For each chat, it runs the same digest pipeline used by the `/digest` command (`IEmailService.HandleDigestAsync()`).
4. Errors for individual chats are logged and skipped — they do not stop delivery to other chats.
5. Delivery runs sequentially per chat to avoid overwhelming the OpenAI and Graph APIs.

### Non-Functional Constraints
- No new bot commands are introduced by this feature.
- Token usage per scheduled digest is logged at `Information` level, identical to on-demand digests.
- Because registration is in-memory only, authenticated chats must re-authenticate after a bot restart before they receive scheduled digests again.

## Technical Architecture

### Web API & Health Checks
- Application uses ASP.NET Core 10 with **minimal APIs** to expose a health check endpoint
- Health endpoint: `GET /health` → returns `{ "status": "healthy", "timestamp": "<ISO-8601>" }`
- Minimal APIs provide lightweight HTTP routing without controllers, ideal for simple monitoring endpoints
- Background service (`BotPollingService`) runs alongside the web API to continuously poll Telegram updates

### Tech Stack
- **Framework**: ASP.NET Core 10 (Web SDK)
- **Telegram**: Telegram.Bot library with polling model
- **Email**: Microsoft Graph SDK with Device Code authentication
- **Logging**: Serilog with console sink
- **AI**: OpenAI (GPT-4o-mini)