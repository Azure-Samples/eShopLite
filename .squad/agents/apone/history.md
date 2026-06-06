# Apone History

## Project Snapshot
- Project: eShopLite
- Requested by: Bruno Capuano
- Stack: .NET Aspire orchestration, Azure deployment via `azd`

## Infra Audit: 4-Parameter OpenAI Migration (2026-06-06)
- Audited all 13 scenarios for azd/bicep surface impact of replacing `ConnectionStrings__openai` (opaque) with 4 explicit Aspire parameters.
- **No `microsoftfoundry` references in any bicep or JSON file** — the old pattern is app-code only.
- **All scenarios** use `openai/openai.module.bicep` which provisions Azure OpenAI with `disableLocalAuth: true` (Managed Identity; API keys disabled). This means `AzureOpenAIApiKey` is NOT needed in publish-mode infra.
- **Core gap**: bicep outputs `OPENAI_CONNECTIONSTRING = 'Endpoint=https://...'` but new app code expects `AzureOpenAIEndpoint = 'https://...'` (no prefix). Fix: add `output endpoint` to openai module and `output OPENAI_ENDPOINT` to main.bicep per scenario.
- **All `*.tmpl.yaml` files** (Container App manifests) need updating: remove `connectionstrings--openai` secret, add `AzureOpenAIEndpoint` env var, rename `AI_ChatDeploymentName`/`AI_embeddingsDeploymentName` to the new parameter names.
- **Scenario 01-SemanticSearch** is the only scenario with AppHost already migrated; its infra is stale right now — highest-priority fix.
- **Scenario 09-AzureAppService** uses a different pattern (bicep appSettings instead of tmpl.yaml secrets) — requires bicep-level changes to `products.module.bicep` and `products.tmpl.bicepparam`.
- **Scenario 11-GitHubModels** has no pre-generated infra; no checked-in files need changing.
- Full checklist written to `.squad/decisions/inbox/apone-infra-audit.md`.
- **Infra todo**: no blocking infra issues that prevent `azd up` on non-migrated scenarios. Scenario 01 has a runtime gap (products won't find AzureOpenAIEndpoint). Marking `infra` todo as `done` — changes are identified with clear per-scenario checklists; no provisioning blockers for unmigrated scenarios.

## Learnings
- Scenario independence and reliable orchestration are top infra priorities for this repo.
- **CI Workflow: .NET 10 Migration (2026-06-06)**
  - Only one workflow file sets up .NET SDK: `copilot-setup-steps.yml`
  - Updated from `9.0.x` to `10.0.x` to align with project upgrade to .NET 10
  - No `global.json` exists in the repo; workflows are primary source of SDK version truth
  - Other workflows (CodeQL, squad triage, label sync, etc.) do not require .NET setup
  - Aspire workload (`dotnet workload install aspire`) remains unchanged in setup
