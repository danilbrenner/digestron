# Digestron

**Your personal inbox droid.**

Digestron is a smart Telegram bot that connects to your Outlook (Microsoft 365 / Outlook.com) inbox, generates concise AI-powered digests of unread emails, and helps you clean up the noise by marking low-priority messages as read.

## Tech Stack

- **Backend**: C# / .NET 8+ (ASP.NET Core Web App)
- **Telegram Client**: Telegram.Bot
- **Email Access**: Microsoft Graph SDK (Outlook)
- **AI Layer**: Azure OpenAI (GPT-4o-mini recommended for cost & speed)
- **Hosting**: Azure App Service
- **Storage (optional)**: Azure Table/Blob Storage or Key Vault for tokens and settings
