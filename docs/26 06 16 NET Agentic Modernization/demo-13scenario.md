# Demo 1 Playbook - Scenario 13 Observability Assistant

## Objective and presenter story

**Objective:** show that a modernized .NET app can turn raw operational telemetry into clear developer actions.

**Story line:**  
1. The app is already modernized and instrumented.  
2. We generate realistic user activity and one controlled failure signal.  
3. The assistant summarizes what happened, where it happened, and what to check next.  
4. This proves AI value comes from existing app signals, not from a generic chatbot.
5. The assistant analyzes real ingested events (not synthetic `BuildLogs`).

## Prerequisites

- .NET 10 SDK, Aspire CLI, and Docker Desktop running.
- Scenario source available at:
  - `D:\azure-samples\eShopLite\scenarios\13-ObservabilityAssistantFoundryLocal`
- Foundry Local model selection configured in `src\ObservabilityAssistant\appsettings*.json` using:
  - `FoundryLocal:SelectedModel`
  - `FoundryLocal:Models` (model catalog entries with aliases/options)
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

## Inspect Foundry Local models (fully local)

Foundry Local runs the LLM on the presenter's machine — no cloud calls. Use the
Foundry Local CLI to show the audience which models are downloaded, available, and
loaded. (Reference: Microsoft Learn — Foundry Local CLI,
`https://learn.microsoft.com/azure/foundry-local/reference/reference-cli`.)

```pwsh
# Version + service status (confirms Foundry Local is installed and running)
foundry --version
foundry service status

# Models DOWNLOADED to the local cache on this machine  <-- show this for "what's local"
foundry cache list

# Where the local model cache lives on disk
foundry cache location

# All catalog models compatible with this hardware (not necessarily downloaded yet)
foundry model list

# Models currently LOADED in the running service (in memory, ready to serve)
foundry service ps
```

What each command proves to the audience:

| Command | Shows | Demo talking point |
|---|---|---|
| `foundry cache list` | Models physically downloaded to the local cache | "These models live on my disk — nothing is called in the cloud." |
| `foundry model list` | Catalog of models available for this hardware | "I can pick any of these; the assistant just needs a model alias." |
| `foundry service ps` | Models loaded in memory right now | "This is the exact model answering my analysis prompts." |
| `foundry service status` | Service endpoint + health | "The local inference service is up and serving on localhost." |

Tie it back to config: the model the assistant uses is selected in
`src\ObservabilityAssistant\appsettings.json` via `FoundryLocal:SelectedModel`
(for example `phi-4-mini`), resolved from the `FoundryLocal:Models` catalog. Switching
models is a one-line config change — point at a different catalog key and restart.

## Key ObservabilityAssistant files to talk about

Show these three files to explain how local log analysis works end to end.

| File | Role | What to say |
|---|---|---|
| `src/ObservabilityAssistant/Program.cs` | **Main app / composition root** | Where Foundry Local model catalog is bound (`FoundryLocal` section) and the selected model is chosen, the `IChatClient` is registered via the Foundry Local adapter, and the in-memory log store, local-embeddings clustering, and analyzer are wired into DI. Comments call out where config is read and how to switch models or disable embeddings. |
| `src/ObservabilityAssistant/ObservabilityEndpoints.cs` | **Web endpoints** | The minimal-API surface: `GET /observability/windows` (selectable 5/10/15/30 windows), `GET /observability/analyze?minutes=N` (run the analysis), and `POST /observability/events` (ingest real telemetry). This is the contract the Store calls. |
| `src/ObservabilityAssistant/ObservabilityAnalyzer.cs` | **Analysis pipeline** | The heart of the demo: pull the time window from the log store -> **cluster semantically-similar lines with local embeddings (ElBruno.LocalEmbeddings, ONNX)** to remove noise -> build a compact prompt with representative lines and occurrence counts -> summarize with the local Foundry Local model -> deterministic fallback if the model is unavailable. The response also reports how many raw entries were folded into how many clusters. |

Presenter note: everything in these three files runs locally — Foundry Local for the
LLM and ElBruno.LocalEmbeddings for the embeddings. There is no Azure or cloud
dependency in the ObservabilityAssistant service.

## Step-by-step live demo script

1. **Set context (20s):**  
   “This is the modernized eShopLite baseline; now I’ll use telemetry to explain an issue.”
2. **Open Store UI:** from Aspire Dashboard, click `store` endpoint.
3. **Keep fault injection ON:** on **Search**, confirm `Inject Search Failure` stays enabled (default-on).
4. **Generate activity + intentional errors:** run 2-3 searches (normal and semantic toggle), including one unlikely term (for example `winter expedition gloves pro`) to force no-match/error telemetry.
5. **Show raw evidence:** in Aspire Dashboard, open `products` and `observabilityassistant` logs and highlight warnings/errors or noisy request traces.
6. **(Optional) Show the local model:** in a terminal run `foundry cache list` and `foundry service ps` to prove the LLM is downloaded and loaded locally (see "Inspect Foundry Local models" above).
7. **Run analysis windows from the Store page in order:** 5, 10, 15, and 30 minutes.
8. **Callout architecture in one line:** Store page sends the request to `observabilityassistant`; the backend clusters similar log lines with local embeddings, summarizes with the local model, and returns findings to Store for display.
9. **Point at the embeddings proof:** in the "Backend call proof" card, read the **Local embeddings clustering** line (e.g. "120 entries grouped into 14 clusters") — this is ElBruno.LocalEmbeddings de-noising the logs before the model sees them.
10. **Read the answer in sections:** summary, affected service, likely cause, next checks.
11. **Close with value line (15s):**  
    “Same app, same telemetry, better developer decision speed — and it all runs locally.”

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
| “UI can toggle semantic vs regular search live and inject failures.” | `scenarios/13-ObservabilityAssistantFoundryLocal/src/Store/Components/Pages/Search.razor` | `Use Semantic Search`, default-on `Inject Search Failure`, and search flow (`DoSearch`). |
| “Local embeddings de-noise logs before the model.” | `scenarios/13-ObservabilityAssistantFoundryLocal/src/ObservabilityAssistant/LogClusteringService.cs` | `ElBruno.LocalEmbeddings` (ONNX) embeds log lines and groups near-duplicates by cosine similarity; graceful pass-through fallback if offline. |
| “Analysis pipeline is local end to end.” | `scenarios/13-ObservabilityAssistantFoundryLocal/src/ObservabilityAssistant/ObservabilityAnalyzer.cs` | Window -> embeddings clustering -> compact prompt with `(xN)` counts -> Foundry Local summary -> fallback; surfaces cluster stats in the response. |

## Optional local-only note: `ElBruno.MAF.FoundryLocal`

If you need a strict local-only MEAI path (no Azure endpoint), wire it in the **Products service**:

- Primary wiring point:  
  `scenarios/13-ObservabilityAssistantFoundryLocal/src/Products/Program.cs`
- Replace or conditionally branch the current `AddChatClient` / `AddEmbeddingGenerator` registration block to use `ElBruno.MAF.FoundryLocal`.
- Keep endpoint contracts unchanged (`ProductEndpoints.cs`) so the demo script remains identical.
- Optional config switch: add a provider selector in `appsettings.Development.json` and branch registration in `Program.cs`.

Presenter note: this is an implementation swap, not a story change.
