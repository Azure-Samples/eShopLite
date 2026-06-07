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
  - `FoundryLocal:SelectedModel` (default demo key: `phi3-5-mini` -> alias `phi-3.5-mini`)
  - `FoundryLocal:Models` (catalog: `phi3-5-mini`, `phi4-mini`, `phi4`, `qwen2-5-coder`)
- **Pre-flight the model so the first Analyze is instant:**

  ```pwsh
  foundry model run phi-3.5-mini   # load the demo model into the local service
  foundry service ps               # confirm a model is loaded
  ```

  The service also warms the selected model on startup (see `ModelWarmupService`), but
  pre-loading guarantees the very first click is a fast inference, not a cold load.
- The app shows a deterministic fallback automatically if the model is unavailable (no paste needed).

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
(demo default `phi3-5-mini` -> alias `phi-3.5-mini`), resolved from the
`FoundryLocal:Models` catalog (which also defines `phi4-mini`, `phi4`, and
`qwen2-5-coder`). Switching models is a one-line config change — point
`SelectedModel` at a different catalog key (and make sure that model is downloaded /
loadable) and restart.

## Key ObservabilityAssistant files to talk about

Show these three files to explain how local log analysis works end to end.

| File | Role | What to say |
|---|---|---|
| `src/ObservabilityAssistant/Program.cs` | **Main app / composition root** | Where Foundry Local model catalog is bound (`FoundryLocal` section) and the selected model is chosen, the `IChatClient` is registered via the Foundry Local adapter, and the in-memory log store, local-embeddings clustering, and analyzer are wired into DI. Comments call out where config is read and how to switch models or disable embeddings. |
| `src/ObservabilityAssistant/ObservabilityEndpoints.cs` | **Web endpoints** | The minimal-API surface: `GET /observability/windows` (selectable 5/10/15/30 windows), `GET /observability/analyze?minutes=N` (run the analysis), and `POST /observability/events` (ingest real telemetry). This is the contract the Store calls. |
| `src/ObservabilityAssistant/ObservabilityAnalyzer.cs` | **Analysis pipeline** | The heart of the demo: pull the time window from the log store -> **cluster semantically-similar lines with local embeddings (ElBruno.LocalEmbeddings, ONNX)** to remove noise -> build a compact prompt with representative lines and occurrence counts -> summarize with the local Foundry Local model -> deterministic fallback if the model is unavailable. The response also reports how many raw entries were folded into how many clusters. |

Presenter note: everything in these three files runs locally — Foundry Local for the LLM and ElBruno.LocalEmbeddings for the embeddings. There is no Azure or cloud dependency in the ObservabilityAssistant service.

## Step-by-step live demo script

1. **Set context (20s):**  
   “This is the modernized eShopLite baseline; now I’ll use telemetry to explain an issue.”
2. **Open Store UI:** from Aspire Dashboard, click `store` endpoint.
3. **Keep fault injection ON:** on **Search**, confirm `Inject Search Failure` stays enabled (it is on by default). With it on, the Products call returns HTTP 500 and the Store emits an **Error** telemetry event the assistant will report. `Use Semantic Search` is off by default — toggle it on for a couple of searches to vary the signals.
4. **Generate activity + intentional errors:** run the searches below (the failure term forces the HTTP 500 / Error telemetry):
  Searches queries
    - do you have something for cooking
    - do you have something for a rainy day
    - something to paint my room white (no results)
    - do you have winter expedition gloves pro (trigger search failure)

5. **Show raw evidence:** in Aspire Dashboard, open `products` and `observabilityassistant` logs and highlight warnings/errors or noisy request traces.
6. **(Optional) Show the local model:** in a terminal run `foundry cache list` and `foundry service ps` to prove the LLM is downloaded and loaded locally (see "Inspect Foundry Local models" above).
7. **Run analysis windows from the Store page in order:** select 5, then 10, 15, and 30 minutes in the **Time window** dropdown and click **Analyze** each time (there is no prompt to type).
8. **Callout architecture in one line:** Store page sends the request to `observabilityassistant`; the backend clusters similar log lines with local embeddings, summarizes with the local model, and returns findings to Store for display.
9. **Point at the embeddings proof:** in the "Backend call proof" card, read the **Local embeddings clustering** line (e.g. "8 entries grouped into 4 clusters") — this is ElBruno.LocalEmbeddings de-noising the logs before the model sees them. Confirm **Analysis source: foundry-local** (if it says `fallback`, the **Why fallback** line explains why).
10. **Read the answer in sections:** summary, affected service, likely cause, next checks.
11. **Close with value line (15s):**  
    “Same app, same telemetry, better developer decision speed — and it all runs locally.”

## How analysis is triggered (there is no prompt box)

The Observability Assistant page has **no free-text prompt**. The presenter only:

1. Selects a **Time window** from the dropdown (5, 10, 15, or 30 minutes).
2. Clicks **Analyze**.

The Store calls `GET /observability/analyze?minutes=N` on the `observabilityassistant`
service. The backend pulls that window from the in-memory log store, clusters similar
lines with local embeddings, then builds the prompt below **internally** and sends it to
the local Foundry Local model. The presenter never types this — it is shown here so the
audience understands exactly what the app asks the model.

### The prompt the app sends to the local model (built in `ObservabilityAnalyzer.cs`)

```text
You are an observability assistant.
Summarize incidents and customer impact for the last {minutes} minutes in at most 6 bullet points.
The log lines below are grouped by similarity; "(xN)" means the line represents N similar events.
Mention probable root cause and immediate next action.

Logs:
{clustered representative log lines, each with an optional (xN) occurrence count}
```

Run Analyze for the 5, 10, 15, and 30-minute windows in turn to show how the window
size changes what the model reports.

## Expected output

With Foundry Local serving the selected model, the page shows a **Backend call proof**
card plus a model-written summary. Real example from a run with the inject-failure
searches:

**Backend call proof**

- Status: HTTP 200
- Endpoint: `https+http://observabilityassistant/observability/analyze?minutes=10`
- Entries analyzed: 3
- Local embeddings clustering: 3 entries grouped into 1 clusters (ElBruno.LocalEmbeddings, ONNX)
- Analysis source: `foundry-local`
- (If the model is unavailable, a **Why fallback** line shows the exact reason.)

**Model summary (<=6 bullets, root cause + next action):**

```text
- Incident: Semantic search for 'do you have winter expedition gloves pro' failed, occurring 3 times in the last 10 minutes.
- Impact: Customers cannot find the requested gloves, risking dissatisfaction and lost sales.
- Probable Root Cause: HTTP 500 indicates a server-side issue (search service or database connectivity).
- Immediate Next Action: Inspect the search system and server logs to resolve the HTTP 500 errors.
- Preventive Measures: Harden search error handling to prevent recurrence.
```

Exact wording varies per model and run; the shape (bulleted incident / impact / root
cause / next action) stays consistent.

## Fallback plan (model/service unavailable)

If the local model cannot answer, the app does **not** error out. It automatically shows
a deterministic, multi-section analysis built from the same clustered telemetry, and the
**Backend call proof** card adds a **Why fallback** line with the exact reason (for
example, the model is not loaded). Nothing to type or paste.

1. Keep the Dashboard open and show the same logs/traces.
2. Say: "The model is unavailable, but the workflow is unchanged: signals in, action plan out."
3. The page shows (example):

```text
**Summary**
- Analyzed 3 entries grouped into 1 clusters in the last 10 minutes.
- 3 error-level and 0 warning-level events after de-duplication.
- Primary concern: [Error] Products semantic search ... failed with HTTP 500. (x3).

**Services involved**
- products (3)

**Likely root cause**
- An error-level signal dominates the window; treat it as the primary incident.

**Next checks**
1. Inspect the top error cluster above and its originating service.
2. Correlate timestamps across services for the same request.
3. Re-run analysis on a wider window (15/30 min) to confirm the trend.
```

To switch back to the model path, ensure Foundry Local is serving the selected model
(`foundry service ps`, or `foundry model run phi-3.5-mini`) and click Analyze again.

1. Continue to Demo 2 without retry loops.

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
