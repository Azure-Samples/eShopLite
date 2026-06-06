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

## Script: Set-AzureOpenAISecrets.ps1 (2026-06-06)
- Created `scripts\Set-AzureOpenAISecrets.ps1` to interactively set all 4 Azure OpenAI Aspire parameter secrets across every scenario AppHost.
- Uses `aspire secret set Parameters:<Name> <value> --apphost <path> --non-interactive` (Aspire CLI secret mechanism).
- Parameters set: `AzureOpenAIEndpoint`, `AzureOpenAIApiKey` (SecureString, never echoed), `AzureOpenAIDeploymentName`, `AzureOpenAIEmbeddingsDeploymentName`.
- Discovers 13 AppHost projects dynamically via `Get-ChildItem -Recurse -Filter eShopAppHost.csproj`; handles 14-MAFDevUI (no `src` subfolder).
- Supports `-DryRun` (prints masked commands, no execution) and `-ScenariosRoot` override.
- Prereq check: errors with install instructions if `aspire` CLI not on PATH.
- Prints per-scenario OK/FAILED summary table; exits non-zero on any failure.
- Parse check: clean (`ParseFile` 0 errors). Dry-run: prereq OK, 13 projects discovered, prompts display correctly.
- `t2-script` todo marked `done`.

## Team Update: .NET 10 Modernization Shipped
**Date:** 2026-06-06 **Status:** ✅ Complete

Fleet completion: All 13 scenarios (.NET 10/Aspire 13) + 1 reference pattern.
- Bishop: 12 scenario modernizations + reference pattern
- Apone: CI bump + infra audit/apply (11 scenarios)
- Hicks: Docs/READMEs/PRDs/speaker-scripts updated
- Vasquez: Verification complete (13/13 Release builds green, 63/63 tests pass)

Approved exceptions documented. Ready for production deployment.

## Learnings
- 2026-06-06T14:03:53.471-04:00: Session docs for the .NET Agentic Modernization plan live under `docs/26 06 16 NET Agentic Modernization/`, with deployment kept slide-only and Hosted Agents treated as an evaluation path.
- 2026-06-06T14:03:53.471-04:00: The main infra anchors for the session are `scenarios/01-SemanticSearch/src/eShopAppHost/Program.cs`, `scenarios/06-mcp/src/eShopAppHost/Program.cs`, `scenarios/09-AzureAppService/README.md`, and `scenarios/10-A2ANet/src/eShopAppHost/Program.cs`.
- 2026-06-06T14:03:53.471-04:00: Scenario baselines should stay centered on 01-SemanticSearch unless a scenario needs MCP, A2A, or deployment-specific infrastructure.

- 2026-06-06T14:03:53.471-04:00: Scaffolded scenario README stubs for 13-17 to match the agentic modernization plan, with the session docs keeping deployment slide-only and Hosted Agents as an evaluation path.

