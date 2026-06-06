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

## Infra Apply: 4-Parameter OpenAI Migration (2026-06-06)
- Applied all publish-mode (azd) infra edits for the 4-parameter OpenAI migration across all 11 scenarios with pre-generated infra (01–10, 12, 14). Scenario 11-GitHubModels skipped (no pre-generated infra).
- **openai.module.bicep** (all 12 scenarios): Added `output endpoint string = openai.properties.endpoint`.
- **main.bicep** (all 12 scenarios): Added `output OPENAI_ENDPOINT string = openai.outputs.endpoint`. Existing `OPENAI_CONNECTIONSTRING` output retained for backward compat.
- **products.tmpl.yaml** (all 11 Container App scenarios): Removed `connectionstrings--openai` secret + `ConnectionStrings__openai` env; added `AzureOpenAIEndpoint: '{{ .Env.OPENAI_ENDPOINT }}'`; renamed `AI_ChatDeploymentName` → `AzureOpenAIDeploymentName` and `AI_embeddingsDeploymentName` → `AzureOpenAIEmbeddingsDeploymentName`. No `AzureOpenAIApiKey` added (Managed Identity, `disableLocalAuth: true`).
- **03-RealtimeAudio/realtimestore.tmpl.yaml**: Removed openai secret/env; added `AzureOpenAIEndpoint` + `AzureOpenAIRealtimeDeploymentName`; removed chat/embeddings vars (realtimestore only uses realtime deployment).
- **06-mcp/eshopmcpserver.tmpl.yaml**: Removed openai secret; added `AzureOpenAIEndpoint` + `AzureOpenAIDeploymentName`; removed embeddings var.
- **06-mcp/store.tmpl.yaml**: Removed openai secret; added `AzureOpenAIEndpoint` + `AzureOpenAIDeploymentName`.
- **09-AzureAppService** (bicep appSettings pattern): Renamed param `openai_outputs_connectionstring` → `openai_outputs_endpoint` in `products.module.bicep` + `products.tmpl.bicepparam`; updated appSettings to use `AzureOpenAIEndpoint`, `AzureOpenAIDeploymentName`, `AzureOpenAIEmbeddingsDeploymentName`.
- **14-MAFDevUI/store.tmpl.yaml**: Added OpenAI env vars (was absent before; AppHost now sends them to store).
- Full per-scenario edit list written to `.squad/decisions/inbox/apone-infra-apply.md`.
- `infra-apply` todo marked `done`.


- Scenario independence and reliable orchestration are top infra priorities for this repo.
- **CI Workflow: .NET 10 Migration (2026-06-06)**
  - Only one workflow file sets up .NET SDK: `copilot-setup-steps.yml`
  - Updated from `9.0.x` to `10.0.x` to align with project upgrade to .NET 10
  - No `global.json` exists in the repo; workflows are primary source of SDK version truth
  - Other workflows (CodeQL, squad triage, label sync, etc.) do not require .NET setup
  - Aspire workload (`dotnet workload install aspire`) remains unchanged in setup
