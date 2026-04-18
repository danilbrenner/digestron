# Phase 2: Microsoft Graph Integration

- [x] **p2-graph-packages** — Add Microsoft Graph NuGet packages  
  Add `Microsoft.Graph` and `Azure.Identity` to `Directory.Packages.props`. Reference them from `Digestron.Infra`.

- [x] **p2-email-interface** — Define IEmailProvider interface in Domain  
  In `Digestron.Domain`, define `IEmailProvider` with `GetUnreadEmailsAsync(int max)` returning `IReadOnlyList<EmailMessage>`. Define `EmailMessage` record with: `Id`, `Subject`, `Sender`, `ReceivedAt`, `BodyPreview` (max 300 chars).

- [x] **p2-graph-config** — Configure Microsoft Graph credentials  
  Add `GraphOptions` class with `TenantId`, `ClientId`, `ClientSecret`, `UserEmail` properties. Read from configuration (Azure App Settings / environment). Never hardcode.

- [x] **p2-graph-provider** — Implement GraphEmailProvider  
  In `Digestron.Infra`, implement `IEmailProvider` using Microsoft Graph SDK. Use `ClientSecretCredential` for auth. Query `/me/mailFolders/inbox/messages` with `$filter=isRead eq false`, `$select=id,subject,from,receivedDateTime,bodyPreview`, `$top=50`. Truncate `bodyPreview` to 300 chars.

- [x] **p2-di-wiring** — Wire Graph provider into DI  
  Register `GraphOptions` and `GraphEmailProvider` in DI from `Program.cs`. Bind `IEmailProvider` → `GraphEmailProvider`.

- [x] **p2-unread-command** — Implement /unread command with real data  
  Update `/unread` handler to call `IEmailProvider.GetUnreadEmailsAsync` and reply with the count of unread emails.

- [x] **p2-device-code-auth** — Switch to Device Code flow (DeviceCodeCredential)  
  Replace `ClientSecretCredential` with `DeviceCodeCredential` in `GraphEmailProvider`. Remove `ClientSecret` and `TenantId` from `GraphOptions`. Use `TenantId = "common"` in credential options for multi-tenant support (personal + work accounts).

- [x] **p2-device-code-logging** — Log device code sign-in prompt  
  When Device Code flow starts, log the device code URL so user can sign in. Update UpdateHandler to detect first auth and notify user: *"🔐 First time? Sign in here: https://microsoft.com/devicelogin"*.

- [x] **p2-graph-minimal-config** — Simplify GraphOptions  
  Update `GraphOptions` to only require `ClientId`. Document in comments that no secrets are stored locally.
