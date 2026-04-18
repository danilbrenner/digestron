# Phase 5: Future / Non-Functional Requirements

- [ ] **p5-app-insights** — Add Application Insights sink to Serilog  
  Add `Serilog.Sinks.ApplicationInsights`. Configure `InstrumentationKey` from settings. Enable only when running in Azure.

- [ ] **p5-azure-deploy** — Azure App Service deployment setup  
  Add Dockerfile or Azure App Service publish profile. Document all required App Settings (secrets). Ensure config is read from Azure App Settings / Key Vault references at runtime.

- [ ] **p5-gmail-provider** — Implement GmailEmailProvider (future)  
  Create `GmailEmailProvider` in `Digestron.Infra` implementing `IEmailProvider` using Gmail API. Should be a drop-in replacement for `GraphEmailProvider` via a DI configuration flag.
