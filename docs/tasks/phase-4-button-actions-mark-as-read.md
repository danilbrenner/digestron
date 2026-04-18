# Phase 4: Button Actions & Mark as Read

- [ ] **p4-mark-read-interface** — Add MarkAsReadAsync to IEmailProvider  
  Extend `IEmailProvider` in `Digestron.Domain` with `MarkAsReadAsync(IReadOnlyList<string> emailIds)`. Update `GraphEmailProvider` to `PATCH /me/messages/{id}` with `isRead=true` for each ID.

- [ ] **p4-inline-keyboard** — Add inline keyboard buttons to /digest reply  
  After sending the digest text, send a follow-up message with `InlineKeyboardMarkup` offering a "Mark suggested as read" button. Encode `SuggestedReadIds` in `callback_data` (e.g. comma-separated or JSON).

- [ ] **p4-callback-handler** — Handle callback queries for mark-as-read  
  In `UpdateHandler`, handle `CallbackQuery` updates. Parse email IDs from `callback_data` and call `IEmailProvider.MarkAsReadAsync`. Answer the callback query and confirm to the user.

- [ ] **p4-error-handling** — Add error handling and user-facing error messages  
  Wrap all command handlers in try/catch. Log exceptions via Serilog. Reply with a friendly error message. Handle Telegram API limits (message > 4096 chars — split or truncate).
