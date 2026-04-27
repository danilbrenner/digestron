# Phase 5: Edit Messages in Place

- [ ] **p5-rename-message-context** — Rename `MessageContext` to `CommandContext` across the solution  
  Rename the record `MessageContext` in `Digestron.Domain/` to `CommandContext`. Update all references across `Digestron.Domain`, `Digestron.Service`, `Digestron.Infra`, and `Digestron.Hosting` — including interface signatures, service implementations, `UpdateHandler`, `MessageResponder`, and all test files. No behavioral changes; this is a pure rename.

- [ ] **p5-context-message-id** — Add `LastMessageId` to `MessageContext`  
  Add `int? ResponseMessageId { get; set; }` to `MessageContext` in `Digestron.Domain/MessageContext.cs`. This mutable property holds the Telegram message ID of the single bot response message for the command, enabling it to be edited in place after the initial send. No other changes in this task.

- [ ] **p5-loading-stores-id** — `SendDigestLoadingMessageAsync` stores the sent message ID in context  
  Update `MessageResponder.SendDigestLoadingMessageAsync` to capture the `Message.MessageId` returned by `botClient.SendMessage(...)` and assign it to `context.LastMessageId`. The `IMessageResponder` interface signature remains `Task` (no return type change). Update existing tests that mock `SendDigestLoadingMessageAsync` to set `context.LastMessageId` via a callback so downstream assertions on context remain valid.

- [ ] **p5-edit-digest-message** — Replace `SendDigestAsync` with `EditDigestMessageAsync` in the digest flow  
  Add `Task EditDigestMessageAsync(MessageContext context, string markdownText, int totalTokens, CancellationToken ct)` to `IMessageResponder`. Implement it in `MessageResponder` using `botClient.EditMessageAsync(context.ChatId, context.LastMessageId!.Value, markdownText, parseMode: ParseMode.Markdown)` with the token-usage footer `_🔢 Tokens used: {n}_` appended (logic moved from `SendDigestAsync`). In `EmailService.HandleDigestAsync`, replace both `SendDigestAsync` call sites (empty-inbox and normal result) with `EditDigestMessageAsync`. Do not remove `SendDigestAsync` from `IMessageResponder` — it remains for other potential uses. Add or update unit tests: verify `EditDigestMessageAsync` is called with the message ID stored in `context.LastMessageId`; verify `SendDigestAsync` is never called in the digest flow.
