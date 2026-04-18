# Digestron - Project Specification

## Goal
Build a Telegram bot that fetches unread Outlook emails, creates AI-powered digests using Azure OpenAI, and allows marking emails as read.

## Core User Flows
- User sends /digest → bot shows a clean summary of unread emails (grouped by priority/action items/newsletters).
- Bot offers buttons to mark suggested low-priority emails as read.
- Commands: /start, /help, /digest, /unread.

## Phases (Incremental)
Phase 1: Telegram bot skeleton with basic commands
Phase 2: Microsoft Graph integration to load unread emails
Phase 3: Azure OpenAI integration for generating digests
Phase 4: Button actions + marking as read
Phase 5: Future – Gmail support via IEmailProvider interface

## Non-Functional Requirements
- Run on Azure App Service
- Send only minimal data to Azure OpenAI: subject, sender, received date, and short bodyPreview (max 300 characters per email)
- All secrets (Telegram token, Azure OpenAI key) must be stored in Azure Key Vault or Azure App Settings (never in code)
- Keep costs low (GPT-4o-mini)
- Easy to extend for other email providers
- Use Pull Telegram updates (no webhooks) for simplicity
- Use Serilog for logging (console + Azure App Insights), with structured logs

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