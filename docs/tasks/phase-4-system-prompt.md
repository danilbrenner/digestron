# Phase 4: System Prompt Management

- [x] **p4-system-prompt-file** — Move system prompt to an embedded resource file  
  Extract the system prompt constant from `OpenAiDigestService` into `Digestron.Infra/Digest/system-prompt.md` as an embedded resource. Add optional `SystemPromptPath` to `OpenAiOptions`. At service instantiation, resolve the prompt once (filesystem path if set and exists, otherwise embedded resource) and cache it — `/digest` never reads the file directly.

- [x] **p4-token-usage** — Surface token usage in the digest message  
  Add `int TotalTokens` to `DigestResult`. Populate it from `ChatCompletion.Usage.TotalTokenCount` (no extra API call). `MessageResponder.SendDigestAsync` appends a footer `_🔢 Tokens used: {n}_` to the Telegram message.

- [x] **p4-reload-prompt** — Add /reloadprompt command  
  Add `ReloadPrompt()` to `IDigestService`. `OpenAiDigestService` immediately re-reads the prompt from the configured source (filesystem or embedded resource) and updates the cache. `/digest` always uses the cached value — never reads the file. Add `SendPromptReloadedMessageAsync` to `IMessageResponder`. Route `/reload-prompt` in `UpdateHandler` → `digestService.ReloadPrompt()` + `messageResponder.SendPromptReloadedMessageAsync()`. Add `/reload-prompt` to the `/help` text. Add tests: command routes correctly; `ReloadPrompt` is called once.
