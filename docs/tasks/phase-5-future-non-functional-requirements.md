# Phase 5: Future / Non-Functional Requirements

- [ ] **p5-app-insights** — Add Application Insights sink to Serilog  
  Add `Serilog.Sinks.ApplicationInsights`. Configure `InstrumentationKey` from settings. Enable only when running in Azure.

- [ ] **p5-gmail-provider** — Implement GmailEmailProvider (future)  
  Create `GmailEmailProvider` in `Digestron.Infra` implementing `IEmailProvider` using Gmail API. Should be a drop-in replacement for `GraphEmailProvider` via a DI configuration flag.
