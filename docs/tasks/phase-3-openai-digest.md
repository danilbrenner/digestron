# Phase 3: OpenAI Digest

- [ ] **p3-openai-packages** — Add OpenAI NuGet package  
  Add `OpenAI` to `Directory.Packages.props`. Reference from `Digestron.Infra`.

- [ ] **p3-digest-interface** — Define IDigestService interface in Domain  
  In `Digestron.Services.Abstractions`, define `IDigestService` with `GenerateDigestAsync(IReadOnlyList<EmailMessage> emails)` returning `DigestResult`. `DigestResult` record: `MarkdownText`, `SuggestedReadIds` (list of low-priority email IDs).

- [ ] **p3-openai-config** — Configure OpenAI settings  
  Add `OpenAiOptions` with `ApiKey`, `Model` (default `gpt-4o-mini`). Read from configuration/environment. Never hardcode.

- [ ] **p3-openai-service** — Implement OpenAiDigestService  
  In `Digestron.Infra`, implement `IDigestService` using the `OpenAI` NuGet package. Send only `subject`, `sender`, `receivedAt`, and `bodyPreview` (≤300 chars) per email. Prompt should return a digest grouped by priority/action items/newsletters, plus a JSON list of low-priority email IDs.

- [ ] **p3-di-wiring-openai** — Wire OpenAI service into DI  
  Register `OpenAiOptions` and `OpenAiDigestService` (`IDigestService`) in DI with `AddInfra`.

- [ ] **p3-digest-command** — Implement /digest command with real AI digest  
  Update `/digest` handler: call `IEmailProvider.GetUnreadEmailsAsync`, then `IDigestService.GenerateDigestAsync`. Send formatted `MarkdownText` back via Telegram (`parse_mode: Markdown`).
