# Demo 1 Playbook - Scenario 13 Observability Assistant

## Objective and presenter story

**Objective:** show that a modernized .NET app can turn raw operational telemetry into clear developer actions.

**Story line:**  
1. The app is already modernized and instrumented.  
2. We generate realistic user activity and one controlled failure signal.  
3. The assistant summarizes what happened, where it happened, and what to check next.  
4. This proves AI value comes from existing app signals, not from a generic chatbot.

## Prerequisites

- .NET 10 SDK, Aspire CLI, and Docker Desktop running.
- Scenario source available at:
  - `D:\azure-samples\eShopLite\scenarios\13-ObservabilityAssistantFoundryLocal`
- Azure OpenAI parameters already set for this scenario AppHost (or local model path prepared).
- Presenter has one saved fallback answer (included below) ready to paste/show.

Quick validation:

```pwsh
dotnet --version
aspire --version
docker --version
```

## Run instructions (preferred: `aspire start`)

### Preferred flow

```pwsh
cd D:\azure-samples\eShopLite\scenarios\13-ObservabilityAssistantFoundryLocal\src\eShopAppHost
aspire stop
aspire start --non-interactive
aspire ps
```

What to confirm before demo:
- `products`, `store`, and `observabilityassistant` resources are `Running`.
- Open the Dashboard URL shown by Aspire and pin it in a browser tab.

### Alternative A (if Aspire CLI behaves unexpectedly)

```pwsh
cd D:\azure-samples\eShopLite\scenarios\13-ObservabilityAssistantFoundryLocal\src\eShopAppHost
dotnet run
```

### Alternative B (clean rebuild, then start)

```pwsh
cd D:\azure-samples\eShopLite\scenarios\13-ObservabilityAssistantFoundryLocal\src
dotnet restore
dotnet build
cd .\eShopAppHost
aspire start --non-interactive
```

## Step-by-step live demo script

1. **Set context (20s):**  
   “This is the modernized eShopLite baseline; now I’ll use telemetry to explain an issue.”
2. **Open Store UI:** from Aspire Dashboard, click `store` endpoint.
3. **Generate activity:** go to **Search**, run 2-3 searches (normal and semantic toggle).
4. **Create a visible low-signal issue:** search a term with no likely result (for example `winter expedition gloves pro`) to create failure/no-match telemetry.
5. **Show raw evidence:** in Aspire Dashboard, open `products` and `observabilityassistant` logs and highlight warnings/errors or noisy request traces.
6. **Run analysis windows from the Store page:** 5, 10, 15, and 30 minutes.
7. **Callout architecture in one line:** Store page sends the request to `observabilityassistant`; the backend summarizes telemetry and returns findings to Store for display.
8. **Read the answer in sections:** summary, affected service, likely cause, next checks.
9. **Close with value line (15s):**  
   “Same app, same telemetry, better developer decision speed.”

## Exact prompts to run

### Window analysis prompts (run in order)

```text
Summarize the last 5 minutes of application activity. Group issues by service, identify the most likely root cause, include relevant trace IDs if available, and suggest the next three things a developer should check.
```

```text
Summarize the last 10 minutes of application activity. Group issues by service, identify the most likely root cause, include relevant trace IDs if available, and suggest the next three things a developer should check.
```

```text
Summarize the last 15 minutes of application activity. Group issues by service, identify the most likely root cause, include relevant trace IDs if available, and suggest the next three things a developer should check.
```

```text
Summarize the last 30 minutes of application activity. Group issues by service, identify the most likely root cause, include relevant trace IDs if available, and suggest the next three things a developer should check.
```

### Backup short prompt

```text
Summarize the last 10 minutes from observabilityassistant and tell me what I should check next.
```

## Expected output

Expected shape:

```text
## Summary
- High request volume on search endpoints
- No-match queries increased after semantic searches

## Services involved
- products
- store
- observabilityassistant

## Likely root cause candidates
1) Query terms not mapped to current catalog vocabulary
2) Semantic result ranking weaker for niche terms

## Supporting signals
- Trace IDs for repeated no-result responses
- Request/response patterns from products API

## Next checks
1) Review failed search terms and add synonym mapping
2) Inspect semantic search endpoint latency/errors
3) Validate catalog coverage for queried terms
```

## Fallback plan (model/service failure)

If the model call fails, do this immediately:

1. Keep the Dashboard open and show the same logs/traces.
2. Say: “Assistant is unavailable, but the workflow is unchanged: signals in, action plan out.”
3. Show this deterministic fallback response:

```text
Summary: In the last 10 minutes, Store called observabilityassistant, which summarized products/store telemetry with repeated no-result searches.
Services involved: products, store, observabilityassistant.
Most likely root cause: search vocabulary mismatch for user phrasing, not infrastructure outage.
Supporting signals: repeated /api/Product/search and /api/aisearch calls returning no useful matches; no SQL connectivity failures.
Next three checks: (1) review top failed terms, (2) add synonym mapping and test semantic ranking, (3) verify product catalog coverage for travel/winter intents.
```

4. Continue to Demo 2 without retry loops.

## Code files to show and story mapping

| Story beat | File | What to show |
|---|---|---|
| “Aspire wires 3 services for Demo 1.” | `scenarios/13-ObservabilityAssistantFoundryLocal/src/eShopAppHost/Program.cs` | `products`, `store`, `observabilityassistant` composition and local-first wiring. |
| “Telemetry is enabled by default in services.” | `scenarios/13-ObservabilityAssistantFoundryLocal/src/eShopServiceDefaults/Extensions.cs` | OpenTelemetry logging/tracing setup and OpenAI activity source toggles. |
| “Store delegates analysis to backend assistant.” | `scenarios/13-ObservabilityAssistantFoundryLocal/src/Store/*` | Store requests a 5/10/15/30-minute analysis window from `observabilityassistant` and renders backend findings. |
| “Products API is AI-capable but still app-grounded.” | `scenarios/13-ObservabilityAssistantFoundryLocal/src/Products/Program.cs` | `AddServiceDefaults`, AOAI client setup, `AddChatClient`, `AddEmbeddingGenerator`, memory initialization logs. |
| “Semantic endpoint is explicit and inspectable.” | `scenarios/13-ObservabilityAssistantFoundryLocal/src/Products/Endpoints/ProductEndpoints.cs` | `/api/aisearch/{search}` plus conventional `/api/Product/search/{search}` routes. |
| “UI can toggle semantic vs regular search live.” | `scenarios/13-ObservabilityAssistantFoundryLocal/src/Store/Components/Pages/Search.razor` | `Use Semantic Search` toggle and search button flow (`DoSearch`). |

## Optional local-only note: `ElBruno.MAF.FoundryLocal`

If you need a strict local-only MEAI path (no Azure endpoint), wire it in the **Products service**:

- Primary wiring point:  
  `scenarios/13-ObservabilityAssistantFoundryLocal/src/Products/Program.cs`
- Replace or conditionally branch the current `AddChatClient` / `AddEmbeddingGenerator` registration block to use `ElBruno.MAF.FoundryLocal`.
- Keep endpoint contracts unchanged (`ProductEndpoints.cs`) so the demo script remains identical.
- Optional config switch: add a provider selector in `appsettings.Development.json` and branch registration in `Program.cs`.

Presenter note: this is an implementation swap, not a story change.
