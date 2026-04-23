# Digestron

**Your personal inbox droid.**

Digestron is a smart Telegram bot that connects to your Outlook (Microsoft 365 / Outlook.com) inbox, generates concise AI-powered digests of unread emails, and helps you clean up the noise by marking low-priority messages as read.

## Tech Stack

- **Backend**: C# / .NET 8+ (ASP.NET Core Web App)
- **Telegram Client**: Telegram.Bot
- **Email Access**: Microsoft Graph SDK (Outlook)
- **AI Layer**: OpenAI (GPT-4o-mini recommended for cost & speed)
- **Hosting**: Docker
- **Storage (optional)**: Environment variables or secrets file for tokens and settings

## Setup

### Prerequisites
- .NET 10.0 SDK
- A Telegram account and bot token (from [@BotFather](https://t.me/BotFather))
- A Microsoft account (personal outlook.com or work Microsoft 365)
- Docker (for production deployment)

### 1. Register Telegram Bot
1. Message [@BotFather](https://t.me/BotFather) on Telegram
2. Create a new bot → save the **bot token**

### 2. Register Azure Entra ID App
1. Go to [portal.azure.com](https://portal.azure.com) → **Microsoft Entra ID** → **App registrations** → **New registration**
2. Name: `Digestron`
3. Supported account types: **"Accounts in any organizational directory and personal Microsoft accounts"**
4. Register → copy the **Application (client) ID**
5. **Authentication** → enable **"Allow public client flows"** → Save
6. **API permissions** → **Add permission** → **Microsoft Graph** → **Delegated** → `Mail.Read` → **Grant admin consent**

### 3. Local Development Setup
```bash
cd src/Digestron.Hosting

# Store Telegram bot token
dotnet user-secrets set "TelegramBot:BotToken" "<your-bot-token>"

# Store Microsoft Graph client ID
dotnet user-secrets set "Graph:ClientId" "<your-client-id>"

# Run the bot
dotnet run
```

The first time you send a command to the bot, you'll see a sign-in prompt:
```
🔐 Sign in at: https://microsoft.com/devicelogin and enter code: ABC123XYZ
```

Enter the code in your browser → bot authenticates and fetches your unread emails.

### 4. Production Deployment (Docker)
1. Build the Docker image:
   ```bash
   docker build -t digestron:latest .
   ```
2. Run the container, passing secrets as environment variables:
   ```bash
   docker run -d \
     -e TelegramBot__BotToken=<your-bot-token> \
     -e Graph__ClientId=<your-client-id> \
     digestron:latest
   ```
