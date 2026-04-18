# Phase 2: Microsoft Graph Integration

- [ ] **p2-graph-packages** — Add Microsoft Graph NuGet packages  
  Add `Microsoft.Graph` and `Azure.Identity` to `Directory.Packages.props`. Reference them from `Digestron.Infra`.

- [ ] **p2-email-interface** — Define IEmailProvider interface in Domain  
  In `Digestron.Domain`, define `IEmailProvider` with `GetUnreadEmailsAsync(int max)` returning `IReadOnlyList<EmailMessage>`. Define `EmailMessage` record with: `Id`, `Subject`, `Sender`, `ReceivedAt`, `BodyPreview` (max 300 chars).

- [ ] **p2-graph-config** — Configure Microsoft Graph credentials  
  Add `GraphOptions` class with `TenantId`, `ClientId`, `ClientSecret`, `UserEmail` properties. Read from configuration (Azure App Settings / environment). Never hardcode.

- [ ] **p2-graph-provider** — Implement GraphEmailProvider  
  In `Digestron.Infra`, implement `IEmailProvider` using Microsoft Graph SDK. Use `ClientSecretCredential` for auth. Query `/me/mailFolders/inbox/messages` with `$filter=isRead eq false`, `$select=id,subject,from,receivedDateTime,bodyPreview`, `$top=50`. Truncate `bodyPreview` to 300 chars.

- [ ] **p2-di-wiring** — Wire Graph provider into DI  
  Register `GraphOptions` and `GraphEmailProvider` in DI from `Program.cs`. Bind `IEmailProvider` → `GraphEmailProvider`.

- [ ] **p2-unread-command** — Implement /unread command with real data  
  Update `/unread` handler to call `IEmailProvider.GetUnreadEmailsAsync` and reply with the count of unread emails.
