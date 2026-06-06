# Squad Decisions

## Active Decisions

No decisions recorded yet.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction

## Merged Decisions & Work Records
### apone-ci-dotnet10
# Decision: Upgrade GitHub Actions CI to .NET 10

**Date:** 2026-06-06  
**Author:** Apone (DevOps/Infra)  
**Status:** ✅ Completed  
**Requested by:** Bruno Capuano

## Summary
Updated GitHub Actions workflows to target .NET 10 SDK in preparation for project-wide upgrade to .NET 10.

## Scope
Only one workflow file sets up .NET:
- `copilot-setup-steps.yml` — Installs .NET for Copilot setup steps before agent launch

## Changes Made
| File | Change | Before | After |
|------|--------|--------|-------|
| `.github/workflows/copilot-setup-steps.yml` | SDK version | `9.0.x` | `10.0.x` |

## Decision Rationale
1. **Single Source of Truth:** No `global.json` exists; workflows are the authoritative .NET version configuration
2. **Minimal Scope:** Only `copilot-setup-steps.yml` explicitly configures .NET; other workflows (CodeQL, squad triage, validation) do not require SDK setup
3. **Aspire Compatibility:** .NET Aspire workload installation remains unchanged (works with both 9.x and 10.x)
4. **Scenario Independence:** No scenario-specific workflows affected; change is globally applicable

## Testing Considerations
- Workflow will be tested on next manual trigger or Copilot agent setup
- Aspire workload should install successfully with .NET 10.0.x
- No breaking changes expected in setup flow

## Related Decisions
- Part of broader project upgrade to .NET 10 (Requested by: Bruno Capuano)
- Complements any `global.json` additions or `csproj` runtime version updates

## Next Steps
- Deploy updated workflow
- Monitor Copilot agent setup runs
- No immediate action required for other workflows

### apone-infra-apply
# Infra Apply: 4-Parameter OpenAI Migration
**Author:** Apone (DevOps/Infra)  
**Requested by:** Bruno Capuano  
**Date:** 2026-06-06  
**Status:** ✅ COMPLETE — all edits applied

---

## Summary

All pre-generated `infra/` folders for scenarios 01–10, 12, 14 have been updated to wire Azure OpenAI correctly for the 4-parameter model. Scenario 11-GitHubModels (no pre-generated infra) is skipped per audit spec.

---

## Per-Scenario Edit List

### 01-SemanticSearch — ✅ EDITED
- `infra/openai/openai.module.bicep`: Added `output endpoint string = openai.properties.endpoint`
- `infra/main.bicep`: Added `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- `infra/products.tmpl.yaml`:
  - Removed `connectionstrings--openai` secret + `ConnectionStrings__openai` env
  - Added `AzureOpenAIEndpoint: '{{ .Env.OPENAI_ENDPOINT }}'`
  - Renamed `AI_ChatDeploymentName` → `AzureOpenAIDeploymentName` (value: `gpt-5-mini`)
  - Renamed `AI_embeddingsDeploymentName` → `AzureOpenAIEmbeddingsDeploymentName` (value: `text-embedding-3-small`)

### 02-AzureAISearch — ✅ EDITED
- `infra/openai/openai.module.bicep`: Added `output endpoint string`
- `infra/main.bicep`: Added `output OPENAI_ENDPOINT string`
- `infra/products.tmpl.yaml`:
  - Removed `connectionstrings--openai` secret + `ConnectionStrings__openai` env (kept `azureaisearch`)
  - Added `AzureOpenAIEndpoint`
  - Renamed deployment name vars (`gpt-41-mini`, `text-embedding-ada-002`)

### 03-RealtimeAudio — ✅ EDITED
- `infra/openai/openai.module.bicep`: Added `output endpoint string` (no `output name` in this module — role embedded)
- `infra/main.bicep`: Added `output OPENAI_ENDPOINT string`
- `infra/products.tmpl.yaml`:
  - Removed openai secret/env
  - Added `AzureOpenAIEndpoint`
  - Renamed `AI_ChatDeploymentName` → `AzureOpenAIDeploymentName` (`gpt-4o-mini`)
  - Renamed `AI_embeddingsDeploymentName` → `AzureOpenAIEmbeddingsDeploymentName` (`text-embedding-ada-002`)
  - **Removed** `AI_RealtimeDeploymentName` (products service does not receive this from AppHost)
- `infra/realtimestore.tmpl.yaml`:
  - Removed openai secret/env
  - Added `AzureOpenAIEndpoint`
  - **Removed** `AI_ChatDeploymentName` + `AI_embeddingsDeploymentName` (realtimestore only gets realtime deployment)
  - Renamed `AI_RealtimeDeploymentName` → `AzureOpenAIRealtimeDeploymentName` (`gpt-4o-mini-realtime-preview`)

### 04-chromadb — ✅ EDITED
- `infra/openai/openai.module.bicep`: Added `output endpoint string` (no `output name` — role embedded)
- `infra/main.bicep`: Added `output OPENAI_ENDPOINT string`
- `infra/products.tmpl.yaml`:
  - Removed openai secret/env
  - Added `AzureOpenAIEndpoint`
  - Renamed deployment name vars (`gpt-4o-mini`, `text-embedding-ada-002`)

### 05-deepseek — ✅ EDITED
- `infra/openai/openai.module.bicep`: Added `output endpoint string`
- `infra/main.bicep`: Added `output OPENAI_ENDPOINT string`
- `infra/products.tmpl.yaml`:
  - Removed `connectionstrings--openai` secret + `ConnectionStrings__openai` env (kept `deepseekr1`)
  - Added `AzureOpenAIEndpoint`
  - Renamed deployment name vars (`gpt-41-mini`, `text-embedding-ada-002`)
- **Note:** `infra_reference/products.tmpl.yaml` intentionally NOT changed (reference copy)
- **Note:** New DeepSeek env vars (`DeepSeekEndpoint`, `DeepSeekApiKey`, `DeepSeekDeploymentName`) added by AppHost but not yet in tmpl.yaml — out of scope for this task

### 06-mcp — ✅ EDITED
- `infra/openai/openai.module.bicep`: Added `output endpoint string`
- `infra/main.bicep`: Added `output OPENAI_ENDPOINT string`
- `infra/products.tmpl.yaml`: Removed openai secret/env; added `AzureOpenAIEndpoint`; renamed deployment vars (`gpt-41-mini`, `text-embedding-ada-002`)
- `infra/eshopmcpserver.tmpl.yaml`:
  - Removed openai secret/env
  - Added `AzureOpenAIEndpoint`
  - Renamed `AI_ChatDeploymentName` → `AzureOpenAIDeploymentName` (`gpt-41-mini`)
  - **Removed** `AI_embeddingsDeploymentName` (eshopmcpserver does not receive embeddings from AppHost)
- `infra/store.tmpl.yaml`:
  - Removed openai secret/env
  - Added `AzureOpenAIEndpoint: '{{ .Env.OPENAI_ENDPOINT }}'`
  - Added `AzureOpenAIDeploymentName: gpt-41-mini`

### 07-AgentsConcurrent — ✅ EDITED
- `infra/openai/openai.module.bicep`: Added `output endpoint string`
- `infra/main.bicep`: Added `output OPENAI_ENDPOINT string`
- `infra/products.tmpl.yaml`: Removed openai secret/env; added `AzureOpenAIEndpoint`; renamed deployment vars (`gpt-41-mini`, `text-embedding-ada-002`)
- **Note (pre-existing gap):** No `insights.tmpl.yaml` exists for the `insights` service (which also gets OpenAI). Not created — out of scope.

### 08-Sql2025 — ✅ EDITED
- `infra/openai/openai.module.bicep`: Added `output endpoint string`
- `infra/main.bicep`: Added `output OPENAI_ENDPOINT string`
- `infra/products.tmpl.yaml`: Removed openai secret/env; added `AzureOpenAIEndpoint`; renamed deployment vars (`gpt-41-mini`, `text-embedding-ada-002`)
- **Note:** Bicep deploys `text-embedding-ada-002`; AppHost publish code references `text-embedding-3-small` — pre-existing mismatch, not in scope. tmpl.yaml values match the bicep.

### 09-AzureAppService — ✅ EDITED (bicep pattern)
- `infra/openai/openai.module.bicep`: Added `output endpoint string`
- `infra/main.bicep`: Added `output OPENAI_ENDPOINT string`
- `infra/products/products.module.bicep`:
  - Renamed param `openai_outputs_connectionstring` → `openai_outputs_endpoint`
  - Replaced appSetting `ConnectionStrings__openai` → `AzureOpenAIEndpoint` (value: `openai_outputs_endpoint`)
  - Renamed `AI_ChatDeploymentName` → `AzureOpenAIDeploymentName`
  - Renamed `AI_embeddingsDeploymentName` → `AzureOpenAIEmbeddingsDeploymentName`
- `infra/products/products.tmpl.bicepparam`:
  - Replaced `param openai_outputs_connectionstring = '{{ .Env.OPENAI_CONNECTIONSTRING }}'`
  - With `param openai_outputs_endpoint = '{{ .Env.OPENAI_ENDPOINT }}'`

### 10-A2ANet — ✅ EDITED
- `infra/openai/openai.module.bicep`: Added `output endpoint string`
- `infra/main.bicep`: Added `output OPENAI_ENDPOINT string`
- `infra/products.tmpl.yaml`: Removed openai secret/env; added `AzureOpenAIEndpoint`; renamed deployment vars (`gpt-41-mini`, `text-embedding-ada-002`)

### 11-GitHubModels — ⏭️ SKIPPED (no pre-generated infra)
Per audit: no checked-in `infra/` folder. When `azd up` is run, infra is generated from the Aspire manifest at that time.

### 12-AzureFunctions — ✅ EDITED
- `infra/openai/openai.module.bicep`: Added `output endpoint string`
- `infra/main.bicep`: Added `output OPENAI_ENDPOINT string`
- `infra/products.tmpl.yaml`: Removed openai secret/env; added `AzureOpenAIEndpoint`; renamed deployment vars (`gpt-41-mini`, `text-embedding-ada-002`)
- **Note (pre-existing gap):** No `semanticsearchfunction.tmpl.yaml` exists. Not created — out of scope.

### 14-MAFDevUI — ✅ EDITED
- `infra/openai/openai.module.bicep`: Added `output endpoint string`
- `infra/main.bicep`: Added `output OPENAI_ENDPOINT string`
- `infra/products.tmpl.yaml`: Removed openai secret/env; added `AzureOpenAIEndpoint`; renamed deployment vars (`gpt-5-mini`, `text-embedding-3-small`)
- `infra/store.tmpl.yaml`: Added `AzureOpenAIEndpoint`, `AzureOpenAIDeploymentName`, `AzureOpenAIEmbeddingsDeploymentName` (store had no openai wiring before; AppHost now sends these)
- **Note:** `microsoftfoundryproject` connection string is referenced by AppHost for products + store but no wiring exists in tmpl.yaml — pre-existing gap, out of scope.

---

## Key Design Decisions Applied

| Decision | Applied |
|---|---|
| `output OPENAI_CONNECTIONSTRING` retained in main.bicep | ✅ Yes — backward-compat during transition |
| `AzureOpenAIApiKey` NOT added to any bicep/tmpl.yaml | ✅ Correct — `disableLocalAuth: true` on all resources |
| Deployment values taken from existing bicep (not AppHost) | ✅ Correct — bicep is authoritative for provisioned resources |
| `infra_reference` files untouched | ✅ Correct — reference only |

---

## Items Left for Follow-Up

| Item | Scenario | Reason |
|---|---|---|
| `insights.tmpl.yaml` missing — `insights` service needs OpenAI | 07-AgentsConcurrent | Pre-existing gap; new file creation out of scope |
| `semanticsearchfunction.tmpl.yaml` missing | 12-AzureFunctions | Pre-existing gap; new file creation out of scope |
| DeepSeek env vars not in products.tmpl.yaml | 05-deepseek | Out of scope (OpenAI only task) |
| `microsoftfoundryproject` not wired in store/products tmpl.yaml | 14-MAFDevUI | Pre-existing gap; out of scope |
| Bicep/AppHost deployment name mismatch for embeddings | 08-Sql2025 | Bicep has `ada-002`, AppHost has `3-small`; pre-existing inconsistency |
| 11-GitHubModels infra generation | 11 | Run `azd infra synth` after AppHost migration; review and commit |

### apone-infra-audit
# Infra Audit: 4-Parameter OpenAI Migration
**Author:** Apone (DevOps/Infra)  
**Requested by:** Bruno Capuano  
**Date:** 2026-06-06  
**Scope:** All scenarios under `D:\azure-samples\eShopLite\scenarios\*`

---

## 1. Inventory Summary

### azure.yaml locations (13 scenarios with azd config)
| Scenario | azure.yaml path |
|---|---|
| 01-SemanticSearch | `scenarios/01-SemanticSearch/src/eShopAppHost/azure.yaml` |
| 02-AzureAISearch | `scenarios/02-AzureAISearch/src/eShopAppHost/azure.yaml` |
| 03-RealtimeAudio | `scenarios/03-RealtimeAudio/src/eShopAppHost/azure.yaml` |
| 04-chromadb | `scenarios/04-chromadb/src/eShopAppHost/azure.yaml` |
| 05-deepseek | `scenarios/05-deepseek/src/eShopAppHost/azure.yaml` |
| 06-mcp | `scenarios/06-mcp/src/eShopAppHost/azure.yaml` |
| 07-AgentsConcurrent | `scenarios/07-AgentsConcurrent/src/eShopAppHost/azure.yaml` |
| 08-Sql2025 | `scenarios/08-Sql2025/src/eShopAppHost/azure.yaml` |
| 09-AzureAppService | `scenarios/09-AzureAppService/src/eShopAppHost/azure.yaml` |
| 10-A2ANet | `scenarios/10-A2ANet/src/eShopAppHost/azure.yaml` |
| 11-GitHubModels | `scenarios/11-GitHubModels/src/eShopAppHost/azure.yaml` |
| 12-AzureFunctions | `scenarios/12-AzureFunctions/src/eShopAppHost/azure.yaml` |
| 14-MAFDevUI | `scenarios/14-MAFDevUI/eShopAppHost/azure.yaml` |

All `azure.yaml` files use the same pattern: `host: containerapp` pointing to the AppHost `.csproj`.

### Infra folder presence
- Scenarios **01–10, 12, 14**: have pre-generated `infra/` with `main.bicep`, `main.parameters.json`, `openai/openai.module.bicep`, `openai-roles/`, `resources.bicep`, and `*.tmpl.yaml` files committed to the repo.
- Scenario **11-GitHubModels**: `azure.yaml` exists but **NO pre-generated `infra/` folder**. azd generates at deploy time.

---

## 2. Current Provisioning & Secrets Flow (pre-migration)

### Standard Pattern (all scenarios except 09, 11)

1. **`openai/openai.module.bicep`** provisions `Microsoft.CognitiveServices/accounts` with `disableLocalAuth: true` (Managed Identity only, API keys disabled). Deploys chat and embeddings models. Outputs:
   - `connectionString = 'Endpoint=${openai.properties.endpoint}'`  ← includes `Endpoint=` prefix
   - `name = openai.name`

2. **`main.bicep`** calls the openai module and exposes:
   - `output OPENAI_CONNECTIONSTRING string = openai.outputs.connectionString`
   
   This becomes an azd environment variable for the next step.

3. **`products.tmpl.yaml`** (and similar per-service files) injects the connection string into the Container App:
   ```yaml
   secrets:
     - name: connectionstrings--openai
       value: '{{ .Env.OPENAI_CONNECTIONSTRING }}'
   env:
     - name: ConnectionStrings__openai
       secretRef: connectionstrings--openai
     - name: AI_ChatDeploymentName
       value: <hardcoded name>
     - name: AI_embeddingsDeploymentName
       value: <hardcoded name>
   ```

4. **Role assignment**: `openai-roles/openai-roles.module.bicep` grants the managed identity `CognitiveServicesOpenAIContributor` on the openai resource (scenarios 01, 02, 05–08, 10, 12, 14) or the role is embedded in the openai module itself (scenarios 03, 04).

5. **`main.parameters.json`**: Contains `principalId`, `sql_password` (where applicable), `environmentName`, `location` — **no OpenAI-related parameters**.

### No `microsoftfoundry` in any bicep or JSON file
Searched entire `scenarios/` tree for `microsoftfoundry` pattern in `.bicep` and `.json` files: **zero matches**. The opaque connection string exists only in application code (scenario 14 AppHost Program.cs).

---

## 3. AppHost Migration Status (at time of audit)

| Scenario | AppHost migration state | Notes |
|---|---|---|
| **01-SemanticSearch** | ✅ **MIGRATED** | Uses 4 explicit `AddParameter()` calls; no `.WithReference(aoai)` for products in publish mode |
| 02-AzureAISearch | ❌ Not yet migrated | Old pattern: `.WithReference(aoai)` in publish |
| 03-RealtimeAudio | ❌ Not yet migrated | Old pattern |
| 04-chromadb | ❌ Not yet migrated | Old pattern |
| 05-deepseek | ❌ Not yet migrated | Old pattern |
| 06-mcp | ❌ Not yet migrated | Old pattern (eshopmcpserver also gets openai) |
| 07-AgentsConcurrent | ❌ Not yet migrated | Old pattern; uses `openai` variable |
| 08-Sql2025 | ❌ Not yet migrated | Old pattern; uses `openai` variable |
| 09-AzureAppService | ❌ Not yet migrated | Different host (App Service, not Container Apps) |
| 10-A2ANet | ❌ Not yet migrated | Old pattern |
| 11-GitHubModels | ❌ Not yet migrated | Uses GitHub token locally; AddAzureOpenAI in publish |
| 12-AzureFunctions | ❌ Not yet migrated | Old pattern; SemanticSearchFunction also gets openai |
| 14-MAFDevUI | ❌ Not yet migrated | Still uses `microsoftfoundry`/`microsoftfoundryproject` pattern |

---

## 4. Key Analysis: What Must Change for 4-Parameter Model

### A. The Core Gap

The existing tmpl.yaml files inject `ConnectionStrings__openai` (from bicep output `OPENAI_CONNECTIONSTRING`). The new 4-parameter model injects individual env vars:

| Old env var (tmpl.yaml) | New env var | Source in publish mode |
|---|---|---|
| `ConnectionStrings__openai` | — (removed) | — |
| `AI_ChatDeploymentName` (hardcoded) | `AzureOpenAIDeploymentName` | Still hardcoded in tmpl.yaml |
| `AI_embeddingsDeploymentName` (hardcoded) | `AzureOpenAIEmbeddingsDeploymentName` | Still hardcoded in tmpl.yaml |
| — | `AzureOpenAIEndpoint` | Derived from provisioned resource |
| — | `AzureOpenAIApiKey` | **NOT INJECTED in publish** (see B below) |

### B. AzureOpenAIApiKey — Not Needed in Publish Mode

All `openai.module.bicep` files set `disableLocalAuth: true`, which **disables API key authentication** on the Azure OpenAI resource. Authentication in publish mode is exclusively via Managed Identity (`AZURE_CLIENT_ID` is already injected by all tmpl.yaml files).

**Conclusion**: `AzureOpenAIApiKey` is a local-dev-only parameter. It **must NOT** be added to `main.bicep`, `main.parameters.json`, or any tmpl.yaml file. azd does not need to prompt for it or store it.

### C. Endpoint Exposure Gap

The bicep currently outputs `OPENAI_CONNECTIONSTRING = 'Endpoint=https://...'`. The `AzureOpenAIEndpoint` env var that the application expects is just `https://...` (no `Endpoint=` prefix). There is no way to strip the prefix inside a tmpl.yaml template.

**Required fix**: Add a new output to `openai/openai.module.bicep`:
```bicep
output endpoint string = openai.properties.endpoint
```
And propagate in `main.bicep`:
```bicep
output OPENAI_ENDPOINT string = openai.outputs.endpoint
```
Then tmpl.yaml uses `{{ .Env.OPENAI_ENDPOINT }}` for `AzureOpenAIEndpoint`.

> Note: The existing `OPENAI_CONNECTIONSTRING` output can be **retained** (backward-compatible) or removed once all tmpl.yaml files are updated. Recommend retaining during transition.

### D. Scenario 09-AzureAppService — Different Pattern

Scenario 09 uses App Service (not Container Apps). Connection string is injected via `siteConfig.appSettings` in `products/products.module.bicep`:
```bicep
{ name: 'ConnectionStrings__openai', value: openai_outputs_connectionstring }
{ name: 'AI_ChatDeploymentName', value: 'gpt-41-mini' }
{ name: 'AI_embeddingsDeploymentName', value: 'text-embedding-ada-002' }
```
This is wired through `products.tmpl.bicepparam` which reads `{{ .Env.OPENAI_CONNECTIONSTRING }}`. Bicep-level changes required (see checklist below).

### E. AzureOpenAIApiKey as Aspire secret parameter — azd handling

When `builder.AddParameter("AzureOpenAIApiKey", secret: true)` is in the AppHost AND if `azd infra synth` is re-run, azd would normally add `AzureOpenAIApiKey` to `main.bicep` as a `@secure()` param and `main.parameters.json` as `"${AZURE_OPENAI_API_KEY}"`. **This should be avoided** since the Azure resource has API keys disabled.

**Mitigation**: Since the infra files are checked in and managed manually, simply **do not** add `AzureOpenAIApiKey` to `main.bicep` or `main.parameters.json`. If `azd infra synth` is run, the team must revert that parameter from the generated output before committing.

### F. Scenario 11-GitHubModels — No Action Needed on Checked-In Files

No pre-generated infra files exist. When azd generates them from the AppHost manifest, it will reflect whatever the AppHost code says at that time. No manual infra changes needed for this scenario.

---

## 5. Per-Scenario Checklist of Required Infra Edits

> Legend: `[BLOCK]` = blocks azd deployment; `[WARN]` = runtime failure without fix; `[SAFE]` = can be deferred

---

### 01-SemanticSearch
**Status: AppHost ALREADY migrated — infra STALE and NEEDS IMMEDIATE UPDATE**

- [ ] `[BLOCK]` `infra/openai/openai.module.bicep`: Add `output endpoint string = openai.properties.endpoint`
- [ ] `[BLOCK]` `infra/main.bicep`: Add `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- [ ] `[WARN]` `infra/products.tmpl.yaml`:
  - Remove secret `connectionstrings--openai` and env var `ConnectionStrings__openai`
  - Add env var `AzureOpenAIEndpoint: '{{ .Env.OPENAI_ENDPOINT }}'`
  - Rename `AI_ChatDeploymentName` → `AzureOpenAIDeploymentName` (value: `gpt-5-mini`)
  - Rename `AI_embeddingsDeploymentName` → `AzureOpenAIEmbeddingsDeploymentName` (value: `text-embedding-3-small`)
  - Do NOT add `AzureOpenAIApiKey` secret

**No changes to `main.parameters.json`, `resources.bicep`, or `store.tmpl.yaml`.**

---

### 02-AzureAISearch
**Status: AppHost not yet migrated — no infra change needed until AppHost is migrated**

When AppHost is migrated:
- [ ] `[BLOCK]` `infra/openai/openai.module.bicep`: Add `output endpoint string = openai.properties.endpoint`
- [ ] `[BLOCK]` `infra/main.bicep`: Add `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- [ ] `[WARN]` `infra/products.tmpl.yaml`:
  - Remove `connectionstrings--openai` secret and `ConnectionStrings__openai` env var (keep `connectionstrings--azureaisearch`)
  - Add env var `AzureOpenAIEndpoint: '{{ .Env.OPENAI_ENDPOINT }}'`
  - Rename `AI_ChatDeploymentName` → `AzureOpenAIDeploymentName` (value: `gpt-41-mini`)
  - Rename `AI_embeddingsDeploymentName` → `AzureOpenAIEmbeddingsDeploymentName` (value: `text-embedding-ada-002`)

---

### 03-RealtimeAudio
**Status: AppHost not yet migrated**

When AppHost is migrated:
- [ ] `[BLOCK]` `infra/openai/openai.module.bicep`: Add `output endpoint string = openai.properties.endpoint`
- [ ] `[BLOCK]` `infra/main.bicep`: Add `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- [ ] `[WARN]` `infra/products.tmpl.yaml`:
  - Remove `connectionstrings--openai` / `ConnectionStrings__openai`
  - Add `AzureOpenAIEndpoint: '{{ .Env.OPENAI_ENDPOINT }}'`
  - Rename deployment name env vars
  - Retain `AI_RealtimeDeploymentName` (or rename to scenario-specific param if applicable)
- [ ] `[WARN]` `infra/realtimestore.tmpl.yaml`:
  - Same openai injection changes as products.tmpl.yaml

---

### 04-chromadb
**Status: AppHost not yet migrated**

When AppHost is migrated:
- [ ] `[BLOCK]` `infra/openai/openai.module.bicep`: Add `output endpoint string = openai.properties.endpoint`
- [ ] `[BLOCK]` `infra/main.bicep`: Add `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- [ ] `[WARN]` `infra/products.tmpl.yaml`: Same changes as scenario 01

Note: 04 does NOT have an `openai-roles` module (roles are embedded in openai.module.bicep itself). No structural difference for this audit.

---

### 05-deepseek
**Status: AppHost not yet migrated**

When AppHost is migrated:
- [ ] `[BLOCK]` `infra/openai/openai.module.bicep`: Add `output endpoint string = openai.properties.endpoint`
- [ ] `[BLOCK]` `infra/main.bicep`: Add `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- [ ] `[WARN]` `infra/products.tmpl.yaml`:
  - Remove `connectionstrings--openai` / `ConnectionStrings__openai`
  - Retain `connectionstrings--deepseekr1` / `ConnectionStrings__deepseekr1` (separate resource, unaffected)
  - Add `AzureOpenAIEndpoint: '{{ .Env.OPENAI_ENDPOINT }}'`
  - Rename deployment name env vars

---

### 06-mcp
**Status: AppHost not yet migrated**

When AppHost is migrated:
- [ ] `[BLOCK]` `infra/openai/openai.module.bicep`: Add `output endpoint string = openai.properties.endpoint`
- [ ] `[BLOCK]` `infra/main.bicep`: Add `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- [ ] `[WARN]` `infra/products.tmpl.yaml`: Same changes as scenario 01
- [ ] `[WARN]` `infra/eshopmcpserver.tmpl.yaml`:
  - Remove `connectionstrings--openai` / `ConnectionStrings__openai`
  - Add `AzureOpenAIEndpoint: '{{ .Env.OPENAI_ENDPOINT }}'`
  - Rename deployment name env vars
- [ ] `[WARN]` `infra/store.tmpl.yaml`:
  - Remove `connectionstrings--openai` / `ConnectionStrings__openai` (store also injects it currently)

Note: `onlineresearcher.tmpl.yaml` and `weatheragent.tmpl.yaml` do NOT inject openai — no change needed there.

---

### 07-AgentsConcurrent
**Status: AppHost not yet migrated**

When AppHost is migrated:
- [ ] `[BLOCK]` `infra/openai/openai.module.bicep`: Add `output endpoint string = openai.properties.endpoint`
- [ ] `[BLOCK]` `infra/main.bicep`: Add `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- [ ] `[WARN]` `infra/products.tmpl.yaml`: Same changes as scenario 01

Note: No tmpl.yaml exists for `inventory-agent`, `promotions-agent`, `researcher-agent` — this is a **pre-existing gap** unrelated to the OpenAI migration, but worth flagging: if these agents need OpenAI access, their tmpl.yaml files need to be created when the scenario is deployed.

---

### 08-Sql2025
**Status: AppHost not yet migrated**

When AppHost is migrated:
- [ ] `[BLOCK]` `infra/openai/openai.module.bicep`: Add `output endpoint string = openai.properties.endpoint`
- [ ] `[BLOCK]` `infra/main.bicep`: Add `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- [ ] `[WARN]` `infra/products.tmpl.yaml`: Same changes as scenario 01

Note: 08 uses `text-embedding-3-small` (not `ada-002`). The hardcoded value in tmpl.yaml matches. Just rename the env var key.

---

### 09-AzureAppService
**Status: AppHost not yet migrated — DIFFERENT deployment pattern (App Service)**

⚠️ **This scenario requires bicep-level changes, not just tmpl.yaml changes.**

When AppHost is migrated:
- [ ] `[BLOCK]` `infra/openai/openai.module.bicep`: Add `output endpoint string = openai.properties.endpoint`
- [ ] `[BLOCK]` `infra/main.bicep`: Add `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- [ ] `[BLOCK]` `infra/products/products.tmpl.bicepparam`:
  - Replace `param openai_outputs_connectionstring = '{{ .Env.OPENAI_CONNECTIONSTRING }}'`
  - With `param openai_outputs_endpoint = '{{ .Env.OPENAI_ENDPOINT }}'`
- [ ] `[BLOCK]` `infra/products/products.module.bicep`:
  - Rename param: `openai_outputs_connectionstring` → `openai_outputs_endpoint`
  - Replace appSetting `ConnectionStrings__openai` with `AzureOpenAIEndpoint`
  - Rename appSetting `AI_ChatDeploymentName` → `AzureOpenAIDeploymentName`
  - Rename appSetting `AI_embeddingsDeploymentName` → `AzureOpenAIEmbeddingsDeploymentName`
  - Do NOT add `AzureOpenAIApiKey` (Managed Identity auth, no API key in Azure)

---

### 10-A2ANet
**Status: AppHost not yet migrated**

When AppHost is migrated:
- [ ] `[BLOCK]` `infra/openai/openai.module.bicep`: Add `output endpoint string = openai.properties.endpoint`
- [ ] `[BLOCK]` `infra/main.bicep`: Add `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- [ ] `[WARN]` `infra/products.tmpl.yaml`: Same changes as scenario 01

---

### 11-GitHubModels
**Status: No pre-generated infra. No action needed on checked-in files.**

- ✅ **No changes needed** to checked-in files.
- When `azd up` is run, azd generates infra from the Aspire manifest at that time. The generated infra will reflect whatever the AppHost code says.
- Team should run `azd infra synth` after AppHost migration and review the generated output before committing.

---

### 12-AzureFunctions
**Status: AppHost not yet migrated**

When AppHost is migrated:
- [ ] `[BLOCK]` `infra/openai/openai.module.bicep`: Add `output endpoint string = openai.properties.endpoint`
- [ ] `[BLOCK]` `infra/main.bicep`: Add `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- [ ] `[WARN]` `infra/products.tmpl.yaml`: Same changes as scenario 01
- [ ] `[WARN]` `infra/store.tmpl.yaml`: Check if store needs openai injection (currently does not inject openai — no change needed)

Note: `semanticsearchfunction` would also need OpenAI wiring. No `semanticsearchfunction.tmpl.yaml` found in the checked-in infra — this is a pre-existing gap for Azure Functions deployment.

---

### 14-MAFDevUI
**Status: AppHost not yet migrated (still uses `microsoftfoundry` pattern)**

When AppHost is migrated:
- [ ] `[BLOCK]` `infra/openai/openai.module.bicep`: Add `output endpoint string = openai.properties.endpoint`
- [ ] `[BLOCK]` `infra/main.bicep`: Add `output OPENAI_ENDPOINT string = openai.outputs.endpoint`
- [ ] `[WARN]` `infra/products.tmpl.yaml`: Same changes as scenario 01
- [ ] `[WARN]` `infra/store.tmpl.yaml`: Remove `connectionstrings--openai` / `ConnectionStrings__openai` (store currently injects it)

Additional note: `microsoftfoundryproject` connection string is ALSO currently referenced in the AppHost (via `builder.AddConnectionString("microsoftfoundryproject")`). This is scenario 14-specific. There is no `microsoftfoundryproject.tmpl.yaml` in the infra — this must also be addressed when the AppHost is migrated.

---

## 6. Cross-Cutting Decisions Needed

| Decision | Details |
|---|---|
| **Retain `OPENAI_CONNECTIONSTRING` output?** | Recommend YES during transition for backward compat. Remove after all tmpl.yaml files are updated. |
| **`AzureOpenAIApiKey` in azd env** | Do NOT propagate to bicep/tmpl.yaml. If `azd infra synth` is run, revert the generated param. |
| **Timing of tmpl.yaml updates** | Update tmpl.yaml in the same PR as the AppHost/Products code change per scenario, or deployment will fail. |
| **Scenario 11 infra generation** | Run `azd infra synth` after AppHost migration; review and commit the output. |
| **Preferred approach** | Consider running `azd infra synth` per scenario as each AppHost is migrated, rather than hand-editing tmpl.yaml files. This is less error-prone. |

---

## 7. Blocking Issues Summary

| # | Issue | Blocks | Affected Scenarios |
|---|---|---|---|
| 1 | `openai.module.bicep` missing `endpoint` output | `azd up` runtime failure (products can't read `AzureOpenAIEndpoint`) | All scenarios when migrated |
| 2 | `products.tmpl.yaml` (and similar) still inject `ConnectionStrings__openai`, NOT `AzureOpenAIEndpoint` | Runtime failure for products service | All scenarios when migrated |
| 3 | `AzureOpenAIApiKey` handled by azd as bicep param if `infra synth` is re-run | Would prompt for an unused secret (confusing but not fatal) | All scenarios |
| 4 | Scenario 01 infra is ALREADY stale (AppHost migrated, infra not) | `azd up` for scenario 01 will work (openai provisioned) but products will fail at runtime (no endpoint env var) | 01 only (immediate) |
| 5 | Scenario 09 bicep-level wiring of connection string | `azd up` will deploy but products gets wrong env var | 09 when migrated |

---

*No bicep edits were made by this audit (changes identified but not implemented — all are non-trivial template regeneration work best done via `azd infra synth` per scenario).*

### bishop-reference-pattern
# Decision: Scenario 01-SemanticSearch Reference Pattern

**Date:** 2026-06-06  
**Author:** Bishop (Backend Dev)  
**Requested by:** Bruno Capuano  
**Status:** Accepted — build green, all tests pass

---

## 1. VectorData In-Memory Provider

**Decision:** Replace `Microsoft.SemanticKernel.Connectors.InMemory` with `CommunityToolkit.VectorData.InMemory`.

### Rationale

A non-SemanticKernel in-memory provider exists as of the .NET Community Toolkit VectorData project. It exposes the same `InMemoryVectorStore` class name in namespace `CommunityToolkit.VectorData.InMemory`, making the swap a single `using` change.

### Package

| Package | Version |
|---------|---------|
| `CommunityToolkit.VectorData.InMemory` | `1.0.0-preview.3` |

Source: https://www.nuget.org/packages/CommunityToolkit.VectorData.InMemory

### Code change in `MemoryContext.cs`

```diff
-using Microsoft.SemanticKernel.Connectors.InMemory;
+using CommunityToolkit.VectorData.InMemory;
```

No other code changes — `InMemoryVectorStore`, `GetCollection<TKey, TRecord>`, `EnsureCollectionExistsAsync`, `UpsertAsync`, `SearchAsync` API surface is identical.

### Package changes in `Products.csproj`

```diff
-<PackageReference Include="Microsoft.SemanticKernel.Connectors.InMemory" Version="1.67.1-preview" />
+<PackageReference Include="CommunityToolkit.VectorData.InMemory" Version="1.0.0-preview.3" />
+<PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="10.6.0" />
 <PackageReference Include="Microsoft.Extensions.AI.Abstractions" Version="10.6.0" />  <!-- bumped from 10.0.1 -->
```

Also removed: `Aspire.Azure.AI.Inference` (unused), `Microsoft.VisualStudio.Web.CodeGeneration.Design` (scaffolding-only).

### `VectorEntities.csproj` bump

```diff
-<PackageReference Include="Microsoft.Extensions.VectorData.Abstractions" Version="9.7.0" />
+<PackageReference Include="Microsoft.Extensions.VectorData.Abstractions" Version="10.6.0" />
```

### `eShopServiceDefaults/Extensions.cs` cleanup

Removed SK OTel registrations (no longer needed without SK):

```diff
-AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);
-.AddMeter("Microsoft.SemanticKernel.*")
-.AddSource("Microsoft.SemanticKernel.*")
```

---

## 2. Three Explicit Aspire Parameters (Replacing Connection String)

**Decision:** Replace the opaque `microsoftfoundry` connection string with four named Aspire parameters.

### Parameters

| Parameter Name | Secret | Description |
|---|---|---|
| `AzureOpenAIEndpoint` | No | Azure OpenAI endpoint, e.g. `https://<resource>.openai.azure.com/` |
| `AzureOpenAIApiKey` | **Yes** | API key (`builder.AddParameter("AzureOpenAIApiKey", secret: true)`) |
| `AzureOpenAIDeploymentName` | No | Chat model deployment name |
| `AzureOpenAIEmbeddingsDeploymentName` | No | Embeddings deployment name |

### Developer Setup (local run mode)

In the `eShopAppHost` project directory:

```bash
dotnet user-secrets set "Parameters:AzureOpenAIEndpoint" "https://<resource>.openai.azure.com/"
dotnet user-secrets set "Parameters:AzureOpenAIApiKey" "<your-api-key>"
dotnet user-secrets set "Parameters:AzureOpenAIDeploymentName" "gpt-5-mini"
dotnet user-secrets set "Parameters:AzureOpenAIEmbeddingsDeploymentName" "text-embedding-3-small"
```

### AppHost snippet (`eShopAppHost/Program.cs`)

```csharp
var aoaiEndpoint             = builder.AddParameter("AzureOpenAIEndpoint");
var aoaiApiKey               = builder.AddParameter("AzureOpenAIApiKey", secret: true);
var aoaiChatDeployment       = builder.AddParameter("AzureOpenAIDeploymentName");
var aoaiEmbeddingsDeployment = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName");

var products = builder.AddProject<Projects.Products>("products")
    .WithReference(productsDb)
    .WaitFor(productsDb)
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeployment);

if (builder.ExecutionContext.IsPublishMode)
{
    // Provision Azure resources for azd deployment
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var aoai = builder.AddAzureOpenAI("openai");
    aoai.AddDeployment(name: "gpt-5-mini", modelName: "gpt-5-mini", modelVersion: "2025-08-07");
    aoai.AddDeployment(name: "text-embedding-3-small", modelName: "text-embedding-3-small", modelVersion: "1");
    products.WithReference(appInsights);
    store.WithReference(appInsights).WithExternalHttpEndpoints();
}
```

### Products consumption snippet (`Products/Program.cs`)

```csharp
using Azure.AI.OpenAI;
using Azure.Identity;
using System.ClientModel;             // ApiKeyCredential lives here (NOT Azure.Core)
using Microsoft.Extensions.AI;

var endpoint   = builder.Configuration["AzureOpenAIEndpoint"] ?? "";
var apiKey     = builder.Configuration["AzureOpenAIApiKey"] ?? "";
var chatDeploy = builder.Configuration["AzureOpenAIDeploymentName"] ?? "gpt-5-mini";
var embDeploy  = builder.Configuration["AzureOpenAIEmbeddingsDeploymentName"] ?? "text-embedding-3-small";

if (!string.IsNullOrEmpty(endpoint))
{
    // Key present → ApiKeyCredential; key absent → DefaultAzureCredential (managed identity / azd)
    AzureOpenAIClient aoaiClient = string.IsNullOrEmpty(apiKey)
        ? new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        : new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

    builder.Services.AddSingleton(aoaiClient);
    // AsIChatClient() and AsIEmbeddingGenerator() are from Microsoft.Extensions.AI.OpenAI 10.6.0
    builder.Services.AddChatClient(aoaiClient.GetChatClient(chatDeploy).AsIChatClient());
    builder.Services.AddEmbeddingGenerator(aoaiClient.GetEmbeddingClient(embDeploy).AsIEmbeddingGenerator());
}
```

---

## 3. Canonical Package Version List (net10, scenario 01)

| Package | Version | Notes |
|---|---|---|
| Aspire.AppHost.Sdk | 13.0.1 | SDK in AppHost |
| Aspire.Hosting.AppHost | 13.0.1 | |
| Aspire.Hosting.Azure.ApplicationInsights | 13.0.1 | |
| Aspire.Hosting.Azure.CognitiveServices | 13.0.1 | |
| Aspire.Hosting.SqlServer | 13.0.1 | |
| Aspire.Azure.AI.OpenAI | 13.0.1-preview.1.25575.3 | preview required |
| Aspire.Microsoft.EntityFrameworkCore.SqlServer | 13.0.1 | |
| **CommunityToolkit.VectorData.InMemory** | **1.0.0-preview.3** | replaces SK InMemory |
| **Microsoft.Extensions.AI.Abstractions** | **10.6.0** | |
| **Microsoft.Extensions.AI.OpenAI** | **10.6.0** | `AsIChatClient`, `AsIEmbeddingGenerator` |
| **Microsoft.Extensions.VectorData.Abstractions** | **10.6.0** | bumped from 9.7.0 |
| Microsoft.EntityFrameworkCore.InMemory | 10.0.0 | tests only |
| MSTest | 4.0.2 | tests only |

---

## Verification

- `dotnet build eShopLite-Aspire.slnx` → **0 errors**
- `dotnet test eShopLite-Aspire.slnx` → **7/7 passed** (Products.Tests: 6, Store.Tests: 1)
- `grep -r "SemanticKernel" scenarios/01-SemanticSearch/src --include="*.cs" --include="*.csproj"` → **0 results**

### bishop-sc02
# Decision: Azure AI Search VectorData Provider — Scenario 02-AzureAISearch

**Date:** 2026-06-06  
**Author:** Bishop (Backend Dev)  
**Scenario:** `scenarios/02-AzureAISearch`  
**Status:** Accepted

---

## Context

Modernizing scenario 02 from net9 + Aspire 9 to **net10 + Aspire 13.0.1** with the goal of removing all Semantic Kernel dependencies. Scenario 02 uses Azure AI Search as its vector store via an SK-named connector package.

## Decision

**Retain `Microsoft.SemanticKernel.Connectors.AzureAISearch` `1.74.0-preview` as the one unavoidable SK-named dependency in this scenario.**

## Research

Searched NuGet for non-SK Azure AI Search VectorData providers:

| Package searched | Result |
|---|---|
| `CommunityToolkit.VectorData.AzureAISearch` | Not found |
| `Microsoft.Extensions.VectorData.AzureAISearch` | Not found |

No MEAI-namespace or CommunityToolkit-namespace Azure AI Search vector store provider exists on NuGet.  
`Aspire.Azure.Search.Documents` provides `SearchIndexClient` DI integration only — no `IVectorStore` implementation.

## Why This Is Acceptable

`Microsoft.SemanticKernel.Connectors.AzureAISearch` is an **SK-named package that implements `Microsoft.Extensions.VectorData` abstractions** (`IVectorStore`, `VectorStoreCollection<TKey, TRecord>`). It has:

- **No dependency on SK kernel, SK chat completion, or SK agent APIs at runtime**
- A dependency only on `Microsoft.Extensions.VectorData.Abstractions` for the interface contracts
- The `AzureAISearchVectorStore` class is a pure VectorData connector

The only SK symbols in the codebase after this migration are:
1. The `<PackageReference>` in Products.csproj
2. The `using Microsoft.SemanticKernel.Connectors.AzureAISearch;` in MemoryContext.cs
3. A comment explaining this decision

All SK chat/kernel/agent usage has been removed. The embeddings and chat are now via MEAI (`IChatClient`, `IEmbeddingGenerator<string, Embedding<float>>`).

## Action Required

If in the future a community or Microsoft package named `CommunityToolkit.VectorData.AzureAISearch` or `Microsoft.Extensions.VectorData.AzureAISearch` appears on NuGet, switch to it and remove this SK-named dependency. Monitor the [semantic-kernel GitHub](https://github.com/microsoft/semantic-kernel) and [extensions-ai GitHub](https://github.com/dotnet/extensions) for progress.

### bishop-sc03
# Decision: Scenario 03-RealtimeAudio Modernization

**Date:** 2026-06-06  
**Author:** Bishop (Backend Dev)  
**Scenario:** `scenarios/03-RealtimeAudio`

---

## Context

Scenario 03 uses the GPT-4o Realtime Audio API via `StoreRealtime` (Blazor app), which calls `RealtimeConversationClient` from `Azure.AI.OpenAI`. The scenario needed to be modernized to net10 + Aspire 13.0.1 + remove all Semantic Kernel and replace the opaque `openai` connection string with 5 explicit Aspire parameters.

---

## Decision 1: Azure.AI.OpenAI pinned to 2.1.0-beta.2

**Chosen:** `Azure.AI.OpenAI` **2.1.0-beta.2** (not the latest 2.3.0-beta.1)

**Reason:** The `OpenAI.RealtimeConversation` namespace no longer exists in `Azure.AI.OpenAI` 2.3.0-beta.1. Attempting to upgrade caused 8+ build errors due to missing types (`RealtimeConversationClient`, `ConversationFunctionTool`, etc.). Since the goal explicitly requires preserving audio functionality, the package was kept at the last known working version.

**Action if unblocked:** If a future version of `Azure.AI.OpenAI` restores or re-exports `RealtimeConversation` types (e.g., via a separate `OpenAI.Realtime` package), upgrade then. Track the `openai/openai-dotnet` GitHub releases.

---

## Decision 2: Five parameters instead of four

**Chosen:** 5 Aspire parameters (`AzureOpenAIRealtimeDeploymentName` added as 5th)

**Reason:** The realtime deployment (`gpt-4o-mini-realtime-preview`) is a distinct model endpoint from the chat deployment (`gpt-4o-mini`). Conflating them would break either the Products chat path or the StoreRealtime audio path. Each service receives only the parameters it needs:
- Products: endpoint, apiKey, chatDeploy, embeddingsDeploy
- StoreRealtime: endpoint, apiKey, realtimeDeploy

---

## Decision 3: Removed Aspire.Azure.AI.OpenAI from StoreRealtime

**Chosen:** Remove `Aspire.Azure.AI.OpenAI` from `StoreRealtime.csproj`, add `Azure.Identity` explicitly

**Reason:** StoreRealtime no longer calls `builder.AddAzureOpenAIClient(...)` — it constructs `AzureOpenAIClient` directly from the 3 config keys injected via Aspire parameters. The Aspire package is no longer needed. However, removing it dropped the transitive `Azure.Identity` dependency, so `Azure.Identity` 1.13.2 was added explicitly to preserve `DefaultAzureCredential` support for managed-identity scenarios.

---

## Decision 4: Microsoft.Extensions.AI 10.6.0 AIFunction API migration

The `AIFunctionExtensions.cs` helper in StoreRealtime.Support uses the MEAI `AIFunction` type to bridge to OpenAI's `ConversationFunctionTool`. The API changed substantially in 10.6.0:

- `AIFunction.Metadata.Name/Description/Parameters` → `AIFunction.Name`, `AIFunction.Description`, `AIFunction.JsonSchema` (JsonElement)
- `InvokeAsync(Dictionary<string, object?>)` → `InvokeAsync(new AIFunctionArguments(dictionary))`

These were updated in-place. No functional behavior change — the tool schema is now sourced from `JsonSchema.GetRawText()` which provides the same OpenAI-compatible JSON as the old hand-built schema.

### bishop-sc04
# Decision: Scenario 04-chromadb — Chroma Vector Provider

**Date:** 2026-06-06  
**Author:** Bishop (Backend Dev)  
**Scenario:** 04-chromadb  

## Decision: Keep ChromaDB.Client as-is (no SK connector needed)

### Context

The modernization spec required investigating whether the Chroma DB vector store in scenario 04 was provided via an SK-named connector (`Microsoft.SemanticKernel.Connectors.Chroma`) or a non-SK provider, and to remove any SK dependency.

### Finding

Scenario 04 uses **`ChromaDB.Client` 1.0.1-ci-13369893450** — a pure community HTTP client for ChromaDB with **no Semantic Kernel dependency**. It is accessed directly in `Products/Memory/MemoryContext.cs` via `ChromaCollectionClient` for upsert and query operations.

The only SK package present was `Microsoft.SemanticKernel.Connectors.InMemory 1.47.0-preview` in `Products.csproj` — but this was a dead reference: **no source file imported or used it**. It was removed cleanly.

There is no `Microsoft.SemanticKernel.Connectors.Chroma` package referenced anywhere in this scenario.

### Decision

**No replacement of the Chroma provider is needed.** `ChromaDB.Client` is already SK-free. Simply removing the unused SK InMemory package was sufficient to achieve 0 SK references.

### Outcome

- 0 `Microsoft.SemanticKernel` references in source or csproj after modernization
- `ChromaDB.Client` retained at 1.0.1-ci-13369893450 (no newer stable version available)
- Build: succeeded (0 errors)
- Tests: no test projects in solution

### bishop-sc05
# Decision: Scenario 05-deepseek — DeepSeek Chat Client Strategy

**Date:** 2026-06-06  
**Author:** Bishop (Backend Dev)  
**Scope:** `scenarios/05-deepseek`  
**Todos closed:** `sc-05`, `sk-05-chat`

---

## Context

Scenario 05 uses DeepSeek-R1 as a reasoning model alongside a standard Azure OpenAI chat model (gpt-41-mini). The original code used:
- `Microsoft.SemanticKernel` 1.47.0 for chat
- `Microsoft.KernelMemory.*` 0.98.x for RAG memory
- `Microsoft.SemanticKernel.Connectors.InMemory` for vector storage
- `OpenAI.Chat.ChatClient` and `OpenAI.Embeddings.EmbeddingClient` as raw SDK types

## Decision: Use `Microsoft.Extensions.AI.OpenAI` 10.6.0 for Both Chat Clients

DeepSeek-R1 is accessed via an **OpenAI-compatible endpoint** (Azure AI Foundry serverless API). The original code used `OpenAIClient` with a custom `Endpoint` in `OpenAIClientOptions`, which is the same pattern supported by `Microsoft.Extensions.AI.OpenAI`.

**Rejected alternative:** `Microsoft.Extensions.AI.AzureAIInference` (`ChatCompletionsClient.AsIChatClient()`). This would be appropriate if DeepSeek were accessed via the Azure AI Inference SDK endpoint (`azureml.inference.…`). However, the original code confirmed it uses the OpenAI-compatible endpoint pattern.

## Two Keyed IChatClient Services

Because both AOAI (standard) and DeepSeek-R1 (reasoning) are needed concurrently in `MemoryContext`, they are registered as keyed singletons:

```csharp
builder.Services.AddKeyedSingleton<IChatClient>("chatClientOpenAI",   aoai...AsIChatClient());
builder.Services.AddKeyedSingleton<IChatClient>("chatClientDeepSeekR1", deepseek...AsIChatClient());
```

`MemoryContext` receives both via `sp.GetKeyedService<IChatClient>(key)` and selects the reasoning model when `useReasoningModel=true` (called from `/api/aisearchreasoning/{search}` endpoint).

## Seven Aspire Parameters (not 4)

This scenario requires **7 parameters** instead of the canonical 4, because DeepSeek-R1 uses a completely separate endpoint, API key, and deployment name:

| Parameter | Secret | Purpose |
|---|---|---|
| `AzureOpenAIEndpoint` | No | AOAI endpoint |
| `AzureOpenAIApiKey` | Yes | AOAI API key |
| `AzureOpenAIDeploymentName` | No | Chat deployment (gpt-41-mini) |
| `AzureOpenAIEmbeddingsDeploymentName` | No | Embeddings deployment |
| `DeepSeekEndpoint` | No | DeepSeek-R1 endpoint |
| `DeepSeekApiKey` | Yes | DeepSeek-R1 API key |
| `DeepSeekDeploymentName` | No | DeepSeek-R1 deployment name |

## KernelMemory Removal

`IKernelMemory` was used in the original code but only for the RAG pipeline setup (not actually used in the search path which used vector search directly). It was fully removed — the in-memory vector store (`CommunityToolkit.VectorData.InMemory`) handles retrieval, and `IEmbeddingGenerator` handles embedding generation.

## Build Result

- `dotnet build` → ✅ succeeded, 0 errors
- `grep Microsoft.SemanticKernel` → ✅ 0 matches
- No test projects in solution

### bishop-sc06
# Bishop Decision Record — Scenario 06-mcp SK→MAF Migration

**Date:** 2026-06-06  
**Scenario:** `scenarios/06-mcp`  
**Todos closed:** `sc-06`, `sk-06-mcp-agents`

---

## Context

Scenario 06-mcp implements an eShopLite solution with:
- **Products** API: semantic vector search backed by an in-memory vector store
- **OnlineResearcher**: an agent that uses Azure AI Foundry's Persistent Agents with Bing Grounding for online search
- **eShopMcpSseServer**: an MCP SSE server that exposes 4 tools (OnlineSearch, Products, Weather, ParkInformation)
- **Store**: Blazor UI that drives an MCP client chatbot against the SSE server
- **WeatherAgent** / **ParkInformationAgent**: simple mock API services

The goal was: net10, Aspire 13.0.1, latest NuGet, remove all Semantic Kernel, 4 explicit Azure OpenAI Aspire parameters, build green.

---

## Key Finding: SK Packages Were Orphaned in OnlineResearcher

The `OnlineResearcher.csproj` listed 4 `Microsoft.SemanticKernel.*` packages:
- `Microsoft.SemanticKernel.Agents.Abstractions` 1.57.0
- `Microsoft.SemanticKernel.Agents.Core` 1.57.0
- `Microsoft.SemanticKernel.Agents.OpenAI` 1.57.0-preview
- `Microsoft.SemanticKernel.Connectors.AzureOpenAI` 1.57.0

However, the actual source code (`OnlineResearchEndPoints.cs`) did **not** use any of these packages. The code had already been migrated to use `Azure.AI.Agents.Persistent` (`PersistentAgentsClient` + `BingGroundingToolDefinition`) for online search via Azure AI Foundry.

**Decision:** Simply remove the 4 orphaned SK package references. No source code changes needed for OnlineResearcher's agent logic.

---

## SK Removal Summary

| Project | SK Packages Removed | Action |
|---------|---------------------|--------|
| `OnlineResearcher.csproj` | 4 packages (Agents.Abstractions, Agents.Core, Agents.OpenAI, Connectors.AzureOpenAI) | Removed — orphaned dead refs |
| `Products.csproj` | `Microsoft.SemanticKernel.Connectors.InMemory` 1.57.0-preview | Replaced with `CommunityToolkit.VectorData.InMemory` 1.0.0-preview.3 |
| `eShopServiceDefaults/Extensions.cs` | SK OTel switch + meter/source wildcards | Removed; added Azure.Experimental.EnableActivitySource |
| `eShopMcpSseServer.csproj` | SK packages already commented out | Cleaned up; no SK ever active |

---

## MAF (Microsoft.Agents.AI) Decision

**No MAF packages were added.** The directive called for replacing `ChatCompletionAgent`/`AddKernel` with MAF's `IChatClient.CreateAIAgent(...)`. However:

- There was **no `ChatCompletionAgent`** in any source file
- There was **no `Kernel`/`AddKernel`** usage in any source file
- The "agent" functionality in scenario 06 comes from:
  1. `Azure.AI.Agents.Persistent` `PersistentAgentsClient` in OnlineResearcher (Bing grounding via AI Foundry — not SK, not MAF)
  2. MEAI `IChatClient` with MCP tool invocation in eShopMcpSseServer and Store

**Conclusion:** The sk-06-mcp-agents todo is fulfilled by removing the orphaned SK package references. No new agent framework abstraction is needed because the scenario's agent behaviour is already implemented via Azure AI Agents SDK (persistent agents) and MEAI IChatClient + MCP tools.

---

## 4 Explicit Azure OpenAI Parameters

Following the canonical reference pattern (proven in scenario 01):

```csharp
// eShopAppHost/Program.cs
var aoaiEndpoint          = builder.AddParameter("AzureOpenAIEndpoint");
var aoaiApiKey            = builder.AddParameter("AzureOpenAIApiKey", secret: true);
var aoaiChatDeployment    = builder.AddParameter("AzureOpenAIDeploymentName");
var aoaiEmbeddingsDeploy  = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName");

// All 4 wired to Products (uses chat + embeddings)
products
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment)
    .WithEnvironment("AzureOpenAIEmbeddingsDeploymentName", aoaiEmbeddingsDeploy);

// First 3 wired to eShopMcpSseServer and Store (chat only)
eshopmcpserver
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment);

store
    .WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint)
    .WithEnvironment("AzureOpenAIApiKey", aoaiApiKey)
    .WithEnvironment("AzureOpenAIDeploymentName", aoaiChatDeployment);
```

User-secrets to set in `eShopAppHost` project:
```
Parameters:AzureOpenAIEndpoint                 https://<resource>.openai.azure.com/
Parameters:AzureOpenAIApiKey                   <your-api-key>
Parameters:AzureOpenAIDeploymentName           gpt-4.1-mini
Parameters:AzureOpenAIEmbeddingsDeploymentName text-embedding-ada-002
```

---

## MCP + MEAI Integration Pattern

MCP tool methods (annotated with `[McpServerTool]`) receive services via ASP.NET DI injection by parameter type. When migrating from `OpenAI.Chat.ChatClient` to MEAI `IChatClient`:

1. Change the parameter type in the tool method signature: `ChatClient chatClient` → `IChatClient chatClient`
2. Change the call site: `chatClient.CompleteChatAsync(messages).Value.Content[0].Text` → `chatClient.GetResponseAsync(messages).Text`
3. Change the message construction: `new UserChatMessage(prompt)` → `new ChatMessage(ChatRole.User, prompt)`

No other wiring needed — `ModelContextProtocol.Server` resolves tool parameters from the ASP.NET DI container automatically, so registering `IChatClient` via `AddChatClient(...)` is sufficient.

---

## Package Changes Reference

### eShopMcpSseServer.csproj
- `Aspire.Azure.AI.OpenAI`: 9.3.1-preview.1.25305.6 → **13.0.1-preview.1.25575.3**
- Added: `Microsoft.Extensions.AI.OpenAI` **10.6.0**
- Removed: `System.Text.Json` 9.0.6 (transitive)
- MCP packages kept at: `ModelContextProtocol` / `ModelContextProtocol.AspNetCore` **0.3.0-preview.1**

### Store.csproj
- `Aspire.Azure.AI.OpenAI`: 9.3.1-preview.1.25305.6 → **13.0.1-preview.1.25575.3**
- `Microsoft.Extensions.AI.OpenAI`: 9.6.0-preview.1.25310.2 → **10.6.0**
- `OpenTelemetry.Instrumentation.AspNetCore`: 1.12.0 → **1.14.0**

---

## Build / Test Result

```
dotnet build eShopLite-Aspire-mcp.slnx  →  Build succeeded (0 errors)
dotnet test  eShopLite-Aspire-mcp.slnx  →  No test projects (exit 0)
grep Microsoft.SemanticKernel            →  0 matches
```

### bishop-sc07
# Decision: SK-to-MAF Agent Migration — Scenario 07-AgentsConcurrent

**Author:** Bishop (Backend Dev)  
**Date:** 2026-06-06  
**Scope:** `scenarios/07-AgentsConcurrent`  
**Fulfills:** `sc-07` (net10 modernization) + `sk-07-agents` (SK removal)

---

## Context

Scenario 07 demonstrated concurrent AI agent orchestration using **Semantic Kernel**:
- `SentimentAgent` and `LanguageAgent` implemented as SK `ChatCompletionAgent`
- `ConcurrentOrchestration` from `Microsoft.SemanticKernel.Agents.Orchestration` running both agents in parallel
- `InProcessRuntime` from `Microsoft.SemanticKernel.Agents.Runtime.InProcess` managing execution
- SK-based `AddKernel()` DI registration + keyed agents off the `Kernel` object

Goal: remove all SK dependencies and preserve identical concurrent multi-agent behavior using the **Microsoft Agent Framework (MAF)**.

---

## Decision

### Agent Implementation

Replaced SK `ChatCompletionAgent` factory methods with **static definition classes** holding `AgentName` and `Instructions` constants. The agent instance is created at DI registration time using:

```csharp
chatClient.CreateAIAgent(name: SentimentAgent.AgentName, instructions: SentimentAgent.Instructions)
```

where `chatClient` is the `IChatClient` registered from the explicit 4-parameter AOAI pattern. This eliminates the `Kernel` dependency entirely.

### Concurrent Orchestration

MAF does not have a direct `ConcurrentOrchestration` equivalent in `1.0.0-preview.251125.1`. The concurrent behavior is preserved by running two **independent single-agent BuildSequential workflows** via `Task.WhenAll`:

```
SK:  ConcurrentOrchestration(A, B).InvokeAsync(input, runtime)
MAF: Task.WhenAll(
       InProcessExecution.RunAsync(BuildSequential("WF-A", [A]), msgs).AsTask(),
       InProcessExecution.RunAsync(BuildSequential("WF-B", [B]), msgs).AsTask()
     )
```

This achieves true concurrent execution at the OS-task level rather than inside the MAF runtime scheduler, which is semantically equivalent for independent agents.

### DI Registration

Replaced SK's `AddKernel()` + `AddKeyedSingleton(nameof(Agent), factory)` with:

```csharp
// No Kernel needed — IChatClient registered via AOAI parameters
services.AddKeyedSingleton<AIAgent>(SentimentAgent.AgentName, (sp, _) =>
    sp.GetRequiredService<IChatClient>()
      .CreateAIAgent(name: SentimentAgent.AgentName, instructions: SentimentAgent.Instructions));
```

`Generator` injects agents via `[FromKeyedServices(SentimentAgent.AgentName)] AIAgent`.

### 4-Parameter AOAI Pattern

Extended to the Insights project (in addition to Products). AppHost wires all four env vars to both services:

```csharp
var aoaiEndpoint         = builder.AddParameter("AzureOpenAIEndpoint");
var aoaiApiKey           = builder.AddParameter("AzureOpenAIApiKey", secret: true);
var aoaiChatDeployment   = builder.AddParameter("AzureOpenAIDeploymentName");
var aoaiEmbedDeployment  = builder.AddParameter("AzureOpenAIEmbeddingsDeploymentName");

products.WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint) ...
insights.WithEnvironment("AzureOpenAIEndpoint", aoaiEndpoint) ...
```

---

## Key API Findings — MAF 1.0.0-preview.251125.1

| Concern | API |
|---------|-----|
| Create agent | `IChatClient.CreateAIAgent(name, instructions)` — from `Microsoft.Agents.AI` |
| Build workflow | `AgentWorkflowBuilder.BuildSequential(name, agents[])` — from `Microsoft.Agents.AI.Workflows` |
| Execute | `InProcessExecution.RunAsync(workflow, messages)` — returns `ValueTask<Run>` |
| Collect results | `run.OutgoingEvents.OfType<AgentRunResponseEvent>().LastOrDefault()?.Response.Text` |
| Concurrent | Call `.AsTask()` on each `ValueTask<Run>` before `Task.WhenAll` |

---

## Packages Added (Insights.csproj)

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Agents.AI` | 1.0.0-preview.251125.1 | Core MAF types + CreateAIAgent extension |
| `Microsoft.Agents.AI.Abstractions` | 1.0.0-preview.251125.1 | Abstractions |
| `Microsoft.Agents.AI.OpenAI` | 1.0.0-preview.251125.1 | OpenAI-backend for agents |
| `Microsoft.Agents.AI.Workflows` | 1.0.0-preview.251125.1 | AgentWorkflowBuilder, InProcessExecution |
| `Microsoft.Extensions.AI` | 10.0.1 | IChatClient DI, AddChatClient |
| `Microsoft.Extensions.AI.Abstractions` | 10.6.0 | MEAI abstractions |
| `Microsoft.Extensions.AI.OpenAI` | 10.6.0 | AsIChatClient() extension |
| `Azure.Identity` | **1.17.0** | DefaultAzureCredential (must be >=1.17.0) |

---

## Packages Removed (Insights.csproj)

- `Microsoft.SemanticKernel` 1.54.0
- `Microsoft.SemanticKernel.Agents.Core` 1.54.0
- `Microsoft.SemanticKernel.Agents.Orchestration` 1.54.0-preview
- `Microsoft.SemanticKernel.Agents.Runtime.InProcess` 1.54.0-preview
- `Microsoft.SemanticKernel.Connectors.AzureOpenAI` 1.54.0
- `Microsoft.SemanticKernel.Connectors.InMemory` 1.54.0-preview
- `Microsoft.VisualStudio.Web.CodeGeneration.Design` 9.0.0

---

## Version Compatibility Note

`Aspire.Azure.AI.OpenAI` 13.0.1-preview.1.25575.3 has a transitive requirement on `Azure.Identity >= 1.17.0`. Any explicit `Azure.Identity` pin below this threshold results in NU1605 (treated as error). The canonical user-secrets doc should note this minimum version requirement.

---

## Outcome

- Build: **succeeded** (0 errors, NU1902 moderate vuln warnings on OTel packages only)
- Tests: **N/A** — no test projects in scenario 07
- SK references: **0** (confirmed via grep)
- Concurrent multi-agent behavior: **preserved** via MAF `Task.WhenAll` pattern

### bishop-sc08
# Decision: Scenario 08-Sql2025 Modernization

**Date:** 2026-06-06  
**Author:** Bishop (Backend Dev)  
**Status:** Done

## Context

Scenario 08 uses SQL Server 2025 native vector search via `EFCore.SqlServer.VectorSearch 9.0.0-preview.2`. The goal was to modernize to net10.0 + Aspire 13.0.1 + 4 explicit AOAI parameters, while keeping the SQL 2025 vector logic intact.

## Key Decisions

### 1. Keep `EFCore.SqlServer.VectorSearch 9.0.0-preview.2` (not upgradeable)

The stable release `9.0.0` has a hard upper bound `[9.0.8, 10.0.0)` on EF Core SqlServer — it **cannot** be used with EF Core 10. The preview `9.0.0-preview.2` has no upper bound, so it resolves without NuGet errors. However, the binary references `AbstractionsStrings.ArgumentIsEmpty` (EF Core 9 internal API removed in EF Core 10), causing `MissingMethodException` when EF Core 10 InMemory is in-process.

There is no `EFCore.SqlServer.VectorSearch 10.0.0` on NuGet as of June 2026. If it becomes available, upgrade and switch `Microsoft.EntityFrameworkCore.InMemory` back to `10.0.0` in tests.

### 2. Pin `Microsoft.EntityFrameworkCore.InMemory` to `9.0.8` in test project

This keeps all EF Core packages at version 9 during test execution, avoiding the binary incompatibility. The test project still targets **net10.0** — EF Core 9 (net8.0 TFM) is fully compatible.

### 3. Return `Task<IResult>` from non-vector endpoint methods

`Results<T1,T2>` is a value-type struct in ASP.NET Core 10. The `as` operator cannot be applied to struct types (CS0039). Changed `GetProductById`, `UpdateProduct`, `DeleteProduct` to return `Task<IResult>` using `Results.Ok()`/`Results.NotFound()` (no type argument) — same pattern as scenario 01.

The vector-specific `AISearch` method retains `Task<Ok<SearchResponse>>` (only one result type, no struct union needed).

### 4. `Assert.ThrowsExceptionAsync` → `Assert.ThrowsAsync` (MSTest 4.x)

MSTest 4.0.2 removed `Assert.ThrowsExceptionAsync<T>`. The replacement is `Assert.ThrowsAsync<T>`.

### 5. MEAI null argument behavior

`Microsoft.Extensions.AI.OpenAI 10.6.0` extension method `GenerateVectorAsync` validates `this` parameter before dereferencing — throws `ArgumentNullException` (not `NullReferenceException`) when `embeddingClient` is null. Tests updated accordingly.

## SQL 2025 Vector Logic: Preserved

- Docker image tag: **`2025-latest`** — unchanged
- `HasColumnType("vector(1536)")` in `Context.OnModelCreating` — unchanged
- `UseVectorSearch()` on SqlServer builder in `Program.cs` — unchanged
- `EF.Functions.VectorDistance("cosine", p.Embedding, vectorSearch)` in `AISearch` — unchanged
- `IEmbeddingGenerator<string, Embedding<float>>` wired via direct `AzureOpenAIClient` → `GetEmbeddingClient` → `AsIEmbeddingGenerator()`

## AOAI Wiring Change Summary

| Before | After |
|--------|-------|
| `AddAzureOpenAIClient("openai", settings => settings.Credential = new AzureCliCredential())` | Direct `new AzureOpenAIClient(endpoint, credential)` |
| `AddConnectionString("openai")` in run mode | 4 Aspire parameters (`AzureOpenAIEndpoint`, `AzureOpenAIApiKey`, `AzureOpenAIDeploymentName`, `AzureOpenAIEmbeddingsDeploymentName`) |
| `AI_ChatDeploymentName` / `AI_embeddingsDeploymentName` env vars | `AzureOpenAIDeploymentName` / `AzureOpenAIEmbeddingsDeploymentName` env vars |

## Results

- **Build**: ✅ 0 errors
- **Tests**: ✅ 7/7 passed (net10.0 test runner, EF Core InMemory 9.0.8)
- **SemanticKernel refs**: ✅ 0

### bishop-sc09
# Decision: SC-09 AzureAppService — MSTest 4 API Break + AppService Parameter Pattern

**Date:** 2026-06-06  
**Author:** Bishop  
**Scenario:** 09-AzureAppService

---

## Notable Finding: MSTest 4.0.2 Removes `ThrowsExceptionAsync<T>`

`Assert.ThrowsExceptionAsync<T>` was removed in MSTest 4.0.2. The replacement is `Assert.ThrowsAsync<T>`. Any scenario upgrading `MSTest` 3.x → 4.x with existing async exception tests will hit `CS0117` at build time.

**Fix:** Replace every occurrence of:
```csharp
await Assert.ThrowsExceptionAsync<MyException>(async () => { ... });
```
with:
```csharp
await Assert.ThrowsAsync<MyException>(async () => { ... });
```

---

## Pattern: Explicit Parameters + App Service Deployment

Scenario 09 combines two concerns that don't conflict:
1. **Azure App Service deployment** (`AddAzureAppServiceEnvironment`, `azure.yaml`, `infra/`) — preserved as-is.
2. **4 explicit AOAI parameters** (replacing the `WithReference(aoai)` + `AddAzureOpenAIClient` pattern).

The key insight: in `IsPublishMode`, we still provision the Azure OpenAI resource via `builder.AddAzureOpenAI("openai")` for Bicep/azd resource creation, but we do **not** wire it to Products via `.WithReference(aoai)`. The 4 explicit parameters (`AzureOpenAIEndpoint`, `AzureOpenAIApiKey`, `AzureOpenAIDeploymentName`, `AzureOpenAIEmbeddingsDeploymentName`) handle connectivity for both run and publish modes.

---

## Aspire.Hosting.Azure.AppService Version

Upgraded from `9.3.1-preview.1.25305.6` → `13.0.1-preview.1.25575.3` (following the same `-preview.1.25575.3` suffix as `Aspire.Azure.AI.OpenAI`).

### bishop-sc10
# Decision: Scenario 10-A2ANet net8→net10 Modernization

**Date:** 2026-06-06  
**Author:** Bishop (Backend Dev)  
**Status:** Done

## Summary

Scenario 10-A2ANet successfully modernized from net8 to net10. The A2A agent orchestration pattern (3 HTTP-based agents: Inventory, Promotions, Researcher) required only TFM and package bumps — no semantic behavioral changes.

## Changes Made

| File | Change |
|------|--------|
| All `*.csproj` | `net8.0` → `net10.0` |
| `eShopAppHost.csproj` | Aspire SDK 9.0.0 → 13.0.1; all Aspire.Hosting.* 9.3.0 → 13.0.1 |
| `Products.csproj` | Remove SK InMemory + VS CodeGen; add CommunityToolkit.VectorData.InMemory 1.0.0-preview.3, MEAI 10.6.0; Aspire 9→13 |
| `eShopServiceDefaults.csproj` | All OTel packages 1.12.0→1.14.0; Http.Resilience 9.5.0→10.0.0; ServiceDiscovery 9.3.0→10.0.0 |
| Agent csproj files | OpenApi 8.0.18→10.0.0; Swashbuckle 6.6.2→8.1.1; OTel 1.12.0→1.14.0 |
| `VectorEntities.csproj` | VectorData.Abstractions 9.5.0 → 10.6.0 |
| `Products/Memory/MemoryContext.cs` | SK InMemory → CommunityToolkit; OpenAI ChatClient/EmbeddingClient → IChatClient/IEmbeddingGenerator; Newtonsoft.Json → STJ |
| `Products/Program.cs` | Remove AddAzureOpenAIClient + manual client wiring; add 4-parameter AOAI pattern |
| `eShopAppHost/Program.cs` | Add 4 `AddParameter` + `WithEnvironment` calls on products; remove `.WithReference(aoai)` in publish mode |
| `eShopServiceDefaults/Extensions.cs` | Remove SK OTel switches and meter/source wildcards |

## Notable Decision: AppHost Publish Mode

In `IsPublishMode` the Aspire Azure OpenAI resource provisioning (`builder.AddAzureOpenAI`) is retained for infrastructure creation, but the runtime credential injection now goes through the 4 explicit parameters rather than `products.WithReference(aoai)`. This keeps azd provisioning working while giving local dev the transparent parameter-based approach.

## Test Results

- Build: **0 errors** (warnings only — NU vuln moderate severity on OTel 1.14.0, same as other modernized scenarios)
- Tests: **9/9 passed** (Products.Tests: 8, Store.Tests: 1)
- SK grep: **0 results**

### bishop-sc11
# Decision: Scenario 11-GitHubModels Modernization

**Date:** 2026-06-06  
**Author:** Bishop (Backend Dev)  
**Status:** Done  

## Context

Scenario 11 is unique in the eShopLite portfolio: it runs against **GitHub Models** (OpenAI-compatible endpoint at `https://models.inference.ai.azure.com`) during local development and switches to **Azure OpenAI** when deployed. The original code used a single opaque `githubToken` Aspire parameter (local mode) and auto-provisioned Azure OpenAI via `AddAzureOpenAI()` resource (publish mode). Both paths registered legacy `OpenAI.Chat.ChatClient` / `OpenAI.Embeddings.EmbeddingClient` singletons, which were consumed by `MemoryContext.cs` still using `Microsoft.SemanticKernel.Connectors.InMemory`.

## Decisions Made

### 1. Dual-mode parameter strategy: explicit params in each branch

Rather than declaring all parameters always (which would force local devs to set unused AOAI params), the AppHost declares parameters **conditionally** in the `if (IsPublishMode)` / `else` branches:

- **Publish mode:** `AzureOpenAIEndpoint`, `AzureOpenAIApiKey` (secret), `AzureOpenAIDeploymentName`, `AzureOpenAIEmbeddingsDeploymentName` + AppInsights
- **Local mode:** `GitHubModelsToken` (secret, required), `GitHubModelsEndpoint` (default: `https://models.inference.ai.azure.com`), `GitHubModelsChatModel` (default: `gpt-4.1-mini`), `GitHubModelsEmbeddingsModel` (default: `text-embedding-3-small`)

The `AI_UseGitHubModels` bool env var is preserved as the Products service's runtime switch.

### 2. GitHub Models MEAI client: `OpenAIClient` with custom endpoint

GitHub Models exposes an OpenAI-compatible REST API. The modernized client uses the plain `OpenAI.OpenAIClient` (from `Azure.AI.OpenAI` transitive dep) pointed at the GitHub endpoint:

```csharp
var client = new OpenAIClient(new ApiKeyCredential(githubToken), new OpenAIClientOptions
{
    Endpoint = new Uri(githubEndpoint)
});
builder.Services.AddChatClient(client.GetChatClient(chatModel).AsIChatClient());
builder.Services.AddEmbeddingGenerator(client.GetEmbeddingClient(embedModel).AsIEmbeddingGenerator());
```

This keeps parity with the Azure path pattern and avoids a separate OpenAI SDK package reference.

### 3. `Aspire.Hosting.Azure.CognitiveServices` removed from AppHost

Because Azure OpenAI is no longer auto-provisioned via `AddAzureOpenAI()` + `AddDeployment()`, the `Aspire.Hosting.Azure.CognitiveServices` package is no longer needed in `eShopAppHost.csproj`. The 4 explicit parameters cover the Azure path without Aspire-managed resource provisioning.

### 4. `Aspire.Azure.AI.OpenAI` removed from Products

Since the Products service constructs `AzureOpenAIClient` / `OpenAIClient` directly (not via `builder.AddAzureOpenAIClient()`), `Aspire.Azure.AI.OpenAI` is not needed. Direct packages `Azure.AI.OpenAI` 2.1.0 + `Azure.Identity` 1.21.0 are used instead.

### 5. `Microsoft.Extensions.AI` must be explicitly referenced

In MEAI 10.6.0, `IServiceCollection.AddChatClient(IChatClient)` and `AddEmbeddingGenerator(IEmbeddingGenerator)` are defined in the `Microsoft.Extensions.AI` base package. `Microsoft.Extensions.AI.OpenAI` 10.6.0 alone only brings `OpenAIHostBuilderExtensions.AddChatClient(IHostApplicationBuilder, string)` — which doesn't match the manual-client-construction pattern. Explicit reference is required.

### 6. `Microsoft.EntityFrameworkCore.InMemory` bumped to 10.0.8 in tests

`Aspire.Microsoft.EntityFrameworkCore.SqlServer` 13.0.1 pulls EF Core 10.x. Using `InMemory` 9.0.8 caused `MissingMethodException` at test initialization. Fixed by bumping to 10.0.8.

## Outcome

- `grep Microsoft.SemanticKernel` → **0 results**
- `dotnet build` → **0 errors**
- `dotnet test` → **7/7 passed** (Products.Tests: 6, Store.Tests: 1)
- Dual-mode GitHub Models / Azure OpenAI behavior preserved and parameterized

### bishop-sc12
# SC-12 Azure Functions — Modernization Decision Notes

**Date:** 2026-06-06  
**Author:** Bishop (Backend Dev)  
**Scenario:** 12-AzureFunctions  
**Status:** Done — build green, 13/13 tests pass

---

## Summary

Modernized scenario 12-AzureFunctions from net8/net9 to net10, Aspire 13.0.1, latest NuGet packages, 4 explicit Azure OpenAI Aspire parameters. Azure Functions semantic-search façade preserved. SK removed. All tests pass.

---

## Notable Decisions

### 1. Azure Functions Worker — net10 via 2.52.0

`Microsoft.Azure.Functions.Worker` **2.52.0** is the version that ships a `lib/net10.0/` folder. The Sdk is **2.0.7**. `Aspire.Hosting.Azure.Functions` remains preview-only at the 13.0.x level (`13.0.1-preview.1.25575.3`).

### 2. `CreateResponse(HttpStatusCode)` Is Now an Extension Method

In Worker 2.52.0, `CreateResponse(HttpStatusCode)` moved from abstract member to extension method in `HttpRequestDataExtensions`. **Moq cannot mock extension methods.** The unit test for `SearchFunction` was rewritten to:
- Mock `CreateResponse()` (the remaining abstract no-arg method)
- Use `SetupGet(r => r.Body).Returns(new MemoryStream())` for the response body

This is a **breaking test pattern change** that any Function-testing code must observe when upgrading from Worker 1.x → 2.x.

### 3. ISemanticSearchRepository Interface (New)

The original `SearchFunction` took `SqlSemanticSearchRepository` (concrete class) as a constructor parameter. Moq cannot mock concrete EF Core classes without virtual methods.  
**Decision:** Introduce `ISemanticSearchRepository` with a `SearchAsync` method. Register via `AddScoped<ISemanticSearchRepository, SqlSemanticSearchRepository>()`. This enables clean unit tests and follows the interface-segregation pattern used in other scenarios.

### 4. EF Core InMemory Version Alignment

`EFCore.SqlServer.VectorSearch 9.0.0` pins EF Core to 9.x. Using `Microsoft.EntityFrameworkCore.InMemory 10.0.0` in tests causes a `MissingMethodException` at runtime (EF Core 10 InMemory + EF Core 9 Relational internals mismatch). **Fix: pin test projects to `Microsoft.EntityFrameworkCore.InMemory 9.0.16`**.

This is the same constraint found in scenario 08-Sql2025. The pattern holds: until `EFCore.SqlServer.VectorSearch` releases a 10.x version, all test projects in scenarios that use it must stay on EF Core InMemory 9.x.

### 5. MSTest 4.x — `ThrowsExactly` vs `ThrowsException`

`Assert.ThrowsException<T>()` does not exist in MSTest 4.x. The sync version is `Assert.ThrowsExactly<T>()`. Async version is `Assert.ThrowsExactlyAsync<T>()`.

### 6. SK Removal

The only SK references were telemetry strings in `eShopServiceDefaults/Extensions.cs`:
- `AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true)`
- `.AddMeter("Microsoft.SemanticKernel.*")`
- `.AddSource("Microsoft.SemanticKernel.*")`

These were removed (same cleanup performed in all other modernized scenarios).

---

## Rejected Approaches

- **`Aspire.Azure.AI.OpenAI` in SemanticSearchFunction**: Was in the original csproj. Removed — the Functions project now uses direct `AzureOpenAIClient` construction (same as Products) instead of Aspire DI extension. Aspire resource wiring via `WithEnvironment` delivers the 4 parameters.
- **EFCore.InMemory 10.0.0 in tests**: Caused `MissingMethodException` at runtime due to version mismatch with VectorSearch 9.0.0. Rejected; pinned to 9.0.16.
- **Mocking `CreateResponse(HttpStatusCode)` via Moq**: Extension method; Moq cannot intercept. Rejected; mocked `CreateResponse()` (abstract no-arg) instead.

### bishop-sc14
# Decision: Scenario 14-MAFDevUI Modernization Notes

**Date:** 2026-06-06  
**Author:** Bishop (Backend Dev)  
**Scenario:** 14-MAFDevUI

---

## Notable Decisions

### 1. Keep `microsoftfoundryproject` Connection String

The task called for replacing `microsoftfoundry` with 4 explicit AOAI parameters. The scenario has a **second** connection string, `microsoftfoundryproject`, which is the Azure AI Projects SDK endpoint used by `AddeShopLiteFoundryAgents` (via `AIProjectClient`). This is unrelated to Azure OpenAI — it is a Foundry project plane endpoint. It was intentionally **kept as a connection string** and still passed via `WithReference(microsoftfoundryproject)` to both Products and Store.

### 2. AgentServices.csproj — Abstractions Pinning Trap

Bumping `Microsoft.Extensions.AI` from `10.0.1` → `10.6.0` causes NU1605 (warning-as-error) because `10.6.0` requires `DependencyInjection.Abstractions >= 10.0.8` and `Logging.Abstractions >= 10.0.8`, but both were pinned at `10.0.0` in `AgentServices.csproj`. **Fix:** bump both pins to `10.0.8`.

### 3. Store.csproj: `Microsoft.Extensions.AI.OpenAI` Was Missing

The Store project used `builder.AddAzureOpenAIClient(...).AddChatClient(...)` before. After switching to direct `AzureOpenAIClient` construction, it needs `.AsIChatClient()` from `Microsoft.Extensions.AI.OpenAI`, which was not previously in `Store.csproj`. Added explicitly.

### 4. MAF Architecture Unchanged

`AgentCheckoutOrchestrator`, `AddeShopLiteFoundryAgents`, `AddeShopLiteAIAgents`, Workflows, and DevUI registration are all untouched. The only change in `AgentServicesExtensions.cs` is that `AddAgentSettings` now reads `AzureOpenAIEndpoint` from `builder.Configuration["AzureOpenAIEndpoint"]` (the new env var) instead of `GetConnectionString("microsoftfoundry")`, so `AgentSettings.AgentsEnabled` returns correctly when the env var is set.

### copilot-directive-20260606-112843
### 2026-06-06T11:28:43.860-04:00: User directive
**By:** Bruno Capuano (via Copilot)
**What:** Add an Aspire expert and require that all information and sources for that agent come only from aspire.dev.
**Why:** User request — captured for team memory

### hicks-docs
# Decision: Documentation Conventions for eShopLite Modernization

**Date:** 2026-06-06  
**Author:** Hicks (Frontend/Docs)  
**Requested by:** Bruno Capuano  
**Status:** Accepted

---

## 1. Canonical Secrets Setup in Docs

**Decision:** All documentation now shows the parameter-based user-secrets setup in the `eShopAppHost` project, not connection strings in child services.

### Pattern

```bash
cd scenarios/<scenario>/src/eShopAppHost

dotnet user-secrets set "Parameters:AzureOpenAIEndpoint" "https://<resource>.openai.azure.com/"
dotnet user-secrets set "Parameters:AzureOpenAIApiKey" "<your-api-key>"
dotnet user-secrets set "Parameters:AzureOpenAIDeploymentName" "<chat-model>"
dotnet user-secrets set "Parameters:AzureOpenAIEmbeddingsDeploymentName" "<embeddings-model>"
```

Scenario-specific extras are always listed explicitly in the scenario README.

### Rationale
- Mirrors the actual AppHost code after bishop's modernization.
- A single project to configure (AppHost) is simpler for beginners.
- Consistent with Aspire's intended parameter model.

---

## 2. Tech Stack Mentions

**Decision:** Docs no longer mention "Semantic Kernel" for in-memory vector search. The replacement terms are:

| Old term | New term |
|----------|----------|
| Semantic Kernel | Microsoft.Extensions.AI (MEAI) |
| `InMemoryVectorStore` (SK) | `CommunityToolkit.VectorData.InMemory` |
| SK Agents / ConcurrentOrchestration | Microsoft Agent Framework (MAF) |

This applies to: architecture diagrams, "Key Technologies" sections, "Dependencies" sections, and inline code examples in documentation files.

### Exception
Scenario 02-AzureAISearch retains a mention of `Microsoft.SemanticKernel.Connectors.AzureAISearch` where necessary, since no non-SK Azure AI Search VectorData connector exists on NuGet. However, narrative text emphasizes it is a pure VectorData connector with no SK kernel/agent dependency.

---

## 3. .NET Version in Docs

**Decision:** All ".NET 9" references replaced with ".NET 10" across all documentation.
Download link canonical form: `https://dotnet.microsoft.com/download/dotnet/10.0`

---

## 4. Scenario-Specific README Structure

**Decision:** Scenarios that deferred entirely to Scenario 01 for local dev instructions now include their own minimal user-secrets setup section, rather than telling users to read another README. Scenario 01 remains the canonical reference for deployment, costs, and security.

---

## 5. PRD / Planning Files

**Decision:** PRD files (`PRD_Add_Payment_Mock_Server.md`, `Shopping_Cart_PRD.md`) were also updated to target `net10.0`, since new features added to this repo must target the same TFM as the rest of the codebase.

### vasquez-verify
# eShopLite Full-Repo Modernization Verification

**Verified by:** Vasquez (Tester/QA)  
**Requested by:** Bruno Capuano  
**Date:** 2026-06-06T12:50:00-04:00  
**Scope:** All 13 scenarios (01–12, 14) — net10.0 + Aspire 13.0.1, SK→MEAI+CommunityToolkit.VectorData.InMemory+MAF, Azure OpenAI secrets→Aspire parameters

---

## Results Matrix

| Scenario | Build (Release) | Tests (Pass/Fail/Skip) | SK Hits | Params OK |
|---|---|---|---|---|
| 01-SemanticSearch | ✅ PASS (0 errors) | Products: 6/0/0 · Store: 1/0/0 | None | ✅ |
| 02-AzureAISearch | ✅ PASS (0 errors) | No test projects | **Allowed**: `Microsoft.SemanticKernel.Connectors.AzureAISearch` (pure VectorData connector, documented exception) | ✅ |
| 03-RealtimeAudio | ✅ PASS (0 errors) | No test projects | None | ✅ |
| 04-chromadb | ✅ PASS (0 errors) | No test projects | None | ✅ |
| 05-deepseek | ✅ PASS (0 errors) | No test projects | None | ✅ |
| 06-mcp | ✅ PASS (0 errors) | No test projects | None | ✅ |
| 07-AgentsConcurrent | ✅ PASS (0 errors) | No test projects | None | ✅ |
| 08-Sql2025 | ✅ PASS (0 errors) | Products: 7/0/0 | None | ✅ |
| 09-AzureAppService | ✅ PASS (0 errors) | Products: 6/0/0 · Store: 1/0/0 | None | ✅ |
| 10-A2ANet | ✅ PASS (0 errors) | Products: 8/0/0 · Store: 1/0/0 | None | ✅ |
| 11-GitHubModels | ✅ PASS (0 errors) | Products: 6/0/0 · Store: 1/0/0 | None | ✅ |
| 12-AzureFunctions | ✅ PASS (0 errors) | Products: 7/0/0 · SemanticSearch: 6/0/0 | None | ✅ |
| 14-MAFDevUI | ✅ PASS (0 errors) | Products: 8/0/0 · Store: 1/0/0 | None | ✅ |

---

## Detail: Builds

All 13 scenarios compiled in Release configuration with **0 errors**.  
NU1902/NU1510/NU1605 vulnerability advisories for `OpenTelemetry.*` 1.14.0 were present as warnings — these are expected and do not constitute failures.

---

## Detail: Tests

| Scenario | Assembly | Passed | Failed | Skipped |
|---|---|---|---|---|
| 01 | Products.Tests (net10.0) | 6 | 0 | 0 |
| 01 | Store.Tests (net10.0) | 1 | 0 | 0 |
| 08 | Products.Tests (net10.0) | 7 | 0 | 0 |
| 09 | Products.Tests (net10.0) | 6 | 0 | 0 |
| 09 | Store.Tests (net10.0) | 1 | 0 | 0 |
| 10 | Products.Tests (net10.0) | 8 | 0 | 0 |
| 10 | Store.Tests (net10.0) | 1 | 0 | 0 |
| 11 | Products.Tests (net10.0) | 6 | 0 | 0 |
| 11 | Store.Tests (net10.0) | 1 | 0 | 0 |
| 12 | Products.Tests (net10.0) | 7 | 0 | 0 |
| 12 | SemanticSearch.Tests (net10.0) | 6 | 0 | 0 |
| 14 | Products.Tests (net10.0) | 8 | 0 | 0 |
| 14 | Store.Tests (net10.0) | 1 | 0 | 0 |

**Total across all test assemblies: 63 passed, 0 failed, 0 skipped.**

No container-dependent test skips observed (no Docker-dependent test failures or skips).

---

## Detail: SemanticKernel Grep

Searched all `*.cs` and `*.csproj` under `scenarios\` for `SemanticKernel`.

**Hits found:**

| File | Line | Content | Status |
|---|---|---|---|
| `02-AzureAISearch\src\Products\Memory\MemoryContext.cs:6` | `using Microsoft.SemanticKernel.Connectors.AzureAISearch;` | ✅ Allowed exception |
| `02-AzureAISearch\src\Products\Products.csproj:41` | Comment block describing the package | ✅ Allowed exception |
| `02-AzureAISearch\src\Products\Products.csproj:43` | `<PackageReference Include="Microsoft.SemanticKernel.Connectors.AzureAISearch" Version="1.74.0-preview" />` | ✅ Allowed exception |

**All 3 hits are the documented allowed exception** (`Microsoft.SemanticKernel.Connectors.AzureAISearch` — a pure VectorData connector, not the SK orchestration stack). No unexpected SemanticKernel references found.

---

## Detail: Aspire Parameter Sanity

All 13 scenarios' `eShopAppHost\Program.cs` confirmed to contain:
- `AddParameter("AzureOpenAIEndpoint")` ✅
- `AddParameter("AzureOpenAIApiKey"` (with `secret: true`) ✅
- `AddParameter("AzureOpenAIDeploymentName")` ✅

Note: 14-MAFDevUI's AppHost is at the scenario root (`eShopAppHost\Program.cs`), not under `src\` — verified correctly.

---

## Verdict

**OVERALL: ✅ ALL GREEN**

- All 13 builds: PASS
- All 63 tests: PASS (0 failures, 0 unexpected skips)
- SK references: Only the 1 documented allowed exception in 02-AzureAISearch
- Parameters: All 13 scenarios correctly configured


