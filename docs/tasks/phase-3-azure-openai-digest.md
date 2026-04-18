# Phase 3: Azure OpenAI Digest

- [ ] **p3-openai-packages** — Add Azure OpenAI NuGet package  
  Add `Azure.AI.OpenAI` to `Directory.Packages.props`. Reference from `Digestron.Infra`.

- [ ] **p3-digest-interface** — Define IDigestService interface in Domain  
  In `Digestron.Domain`, define `IDigestService` with `GenerateDigestAsync(IReadOnlyList<EmailMessage> emails)` returning `DigestResult`. `DigestResult` record: `MarkdownText`, `SuggestedReadIds` (list of low-priority email IDs).

- [ ] **p3-openai-config** — Configure Azure OpenAI settings  
  Add `AzureOpenAiOptions` with `Endpoint`, `ApiKey`, `DeploymentName` (default `gpt-4o-mini`). Read from configuration/environment. Never hardcode.

- [ ] **p3-openai-service** — Implement AzureOpenAiDigestService  
  In `Digestron.Infra`, implement `IDigestService` using `Azure.AI.OpenAI`. Send only `subject`, `sender`, `receivedAt`, and `bodyPreview` (≤300 chars) per email. Prompt should return a digest grouped by priority/action items/newsletters, plus a JSON list of low-priority email IDs.

- [ ] **p3-di-wiring-openai** — Wire OpenAI service into DI  
  Register `AzureOpenAiOptions` and `AzureOpenAiDigestService` (`IDigestService`) in DI from `Program.cs`.

- [ ] **p3-digest-command** — Implement /digest command with real AI digest  
  Update `/digest` handler: call `IEmailProvider.GetUnreadEmailsAsync`, then `IDigestService.GenerateDigestAsync`. Send formatted `MarkdownText` back via Telegram (`parse_mode: Markdown`).
