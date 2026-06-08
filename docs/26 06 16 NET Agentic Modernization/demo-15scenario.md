# Demo 3 Playbook - Scenario 15 Store Intelligence Report

> **Format: live baseline + prepared report.** The report pipeline is **not implemented**
> yet in this scenario (it is the Scenario 01 baseline). For the session we run the real app
> to generate **real signals** (searches, failed searches), then present a **deterministic
> Store Intelligence Report** and map every section back to those app signals. This keeps the
> demo reliable while telling the "signals become business decisions" story.

## Objective and presenter story

**Objective:** show that the same modernized app that serves users (Demo 1) and explains its
operations (Demo 2) can also roll its raw signals up into a **business summary** that a
non-developer — a store manager — can act on.

**Story line:**

1. The app already produces signals: catalog data, searches, failed searches, cart/checkout events.
2. Developers and ops see logs and traces; **business users** need patterns and next actions.
3. We ask for a daily **Store Intelligence Report**.
4. The report turns scattered signals into: top intents, gaps (no-result searches), operational
   issues, and recommended actions.
5. This proves AI value is not only UX and ops — it is **decision support** built from the app's
   own data.

## Where this sits in the arc

- **Demo 1 (Product Discovery, cloud AI):** better answers for *users*.
- **Demo 2 (Observability Assistant, local AI):** better understanding for *developers/ops*.
- **Demo 3 (Store Intelligence, this demo):** better summaries for *business users*.

Same modernized app, same signals — a third audience served.

## Current state (be transparent on stage)

- Scenario 15 is the **Scenario 01 Semantic Search baseline**: `sql` + `products` + `store`,
  with keyword and semantic search endpoints.
- There is **no report generation endpoint or report page yet** — see
  `scenario-map.md` ("report pipeline not implemented").
- So Demo 3 is presented as **prepared report fallback**: run the app to create genuine
  activity, then show the deterministic report below and tie each section to the signals the
  audience just generated.

## Prerequisites

- .NET 10 SDK, Aspire CLI, and Docker Desktop running.
- Scenario source available at:
  - `D:\azure-samples\eShopLite\scenarios\15-StoreIntelligenceReport`
- If you run **semantic** search live, the Products service needs the four Azure OpenAI
  parameters (same as Scenario 01/14):
  - `Parameters:AzureOpenAIEndpoint`, `Parameters:AzureOpenAIApiKey`,
    `Parameters:AzureOpenAIDeploymentName`, `Parameters:AzureOpenAIEmbeddingsDeploymentName`
  - Keyword search needs no secrets, so the demo works even without Azure OpenAI configured.

Quick validation:

```pwsh
dotnet --version
aspire --version
docker --version
```

## Run instructions (preferred: `aspire start`)

### Preferred flow

```pwsh
cd D:\azure-samples\eShopLite\scenarios\15-StoreIntelligenceReport\src\eShopAppHost
aspire stop
aspire start --non-interactive
aspire ps
```

What to confirm before demo:

- `sql`, `products`, and `store` resources are `Running`.
- Open the Dashboard URL shown by Aspire and pin the `store` endpoint in a browser tab.

### Alternative (clean rebuild, then start)

```pwsh
cd D:\azure-samples\eShopLite\scenarios\15-StoreIntelligenceReport\src
dotnet restore
dotnet build
cd .\eShopAppHost
aspire start --non-interactive
```

> Note: always launch from **this scenario's** `eShopAppHost`. Running `aspire start` from the
> wrong folder can attach to a different scenario's AppHost (a known footgun across scenarios).

## Step-by-step live demo script

1. **Set context (20s):**
   "We served users, then developers. Now the store manager: same signals, a daily business report."
2. **Open Store UI:** from the Aspire Dashboard, click the `store` endpoint.
3. **Generate real signals on the Search page** — run a mix so the report has something to summarize:
   Searches queries
     - do you have something for cooking
     - do you have something for a rainy day
     - winter camping tent four season
     - something to paint my room white (no results → a *failed search* signal)
     - do you have hiking boots size 12 (no results → another *failed search* signal)
   Toggle `Use Semantic Search` on for a couple of these to vary intent signals.
4. **Show the raw signals:** in the Aspire Dashboard, open `products` / `store` logs and point at
   the search requests and the no-result responses. "This is the raw material — scattered events."
5. **Ask for the report (the business ask):** state the prompt the report answers (below). Then
   show the **deterministic Store Intelligence Report** (prepared output below) as the result.
6. **Walk the report top-down:** executive summary → top searches → failed searches → product
   gaps → operational issues → recommended actions.
7. **Map each section to what they just did (the key move):** use the mapping table below to point
   from a report line back to a real search the audience watched you run.
8. **Close with value line (15s):**
   "The app already had this data. The report just turns it into decisions a non-developer can act on."

## The business ask (the report prompt)

This is the question the report answers. Say it out loud; you do not type it anywhere in the
baseline app (the generation pipeline is the gap this scenario will fill).

```text
Create today's store intelligence report. Include top searches, failed searches, product
opportunities, operational issues that may affect customers, and recommended next actions.
```

## Prepared Store Intelligence Report (deterministic)

Show this as the report output. It follows the scenario's report schema
(`scenarios/15-StoreIntelligenceReport/docs/report-schema.md`).

```text
Store Intelligence Report — {today}

Executive summary
- Customers searched mostly for outdoor and seasonal camping gear today.
- Two searches returned no results, pointing at real catalog gaps and lost intent.
- No blocking operational issues; one latency signal worth a quick check.

Top customer intents (top searches)
- Rainy-day / weather-ready gear
- Four-season / winter camping (tents)
- Cooking / camp kitchen

Searches with no results (failed searches)
- "something to paint my room white"  → off-catalog intent (out of scope, expected)
- "hiking boots size 12"               → on-catalog intent, no SKU → product/size gap

Product opportunities
- Add or surface winter / four-season tents prominently.
- Review footwear size coverage (size 12 demand with no result).

Operational issues
- Search latency slightly elevated during the activity burst — verify products API + DB.

Recommended actions
1. Fill the footwear size-12 gap (catalog or supplier).
2. Promote seasonal (winter/rainy) gear on the storefront.
3. Run a quick latency check on the products search path.
```

> Exact wording is illustrative; the **shape** (executive summary → intents → failed searches →
> gaps → operational issues → actions) is what to land. The sections are deterministic so the
> demo never depends on a live model call.

## How the report maps to app signals

This is the slide-worthy point: every report line traces to a signal the app already emits.

| Report section | App signal it comes from | Where in the app |
|---|---|---|
| Top searches | Search terms hitting the Products API | `GET /api/Product/search/{term}` and `GET /api/aisearch/{term}` |
| Failed searches | Searches that returned zero products | Empty result sets from the same search endpoints |
| Product opportunities | On-catalog intents with no matching SKU | Failed-but-relevant searches (e.g., "size 12") |
| Operational issues | Latency / errors on the search path | OpenTelemetry traces/logs on `products` / `store` |
| Recommended actions | Derived from the gaps + ops signals above | Business interpretation layer (the report) |

## Expected output shape

```text
Store Intelligence Report
- Executive summary
- Top customer intents
- Searches with no results
- Product gaps
- Operational issues
- Recommended actions
```

## Fallback plan

Because the report itself is prepared/deterministic, this demo has **no live-AI failure mode**.
If anything goes wrong:

- If the **app** will not start: skip the live activity and present the prepared report directly,
  describing what searches would have produced each section.
- If **semantic** search is unavailable (no Azure OpenAI secrets): use **keyword** search only —
  the signals (top searches, failed searches) are identical for the report story.
- Then continue to **Demo 4 (MCP Store Tools)** without retry loops.

## Key files to talk about

The report feature is the gap to build; for now, point at the baseline that produces the signals.

| File | Role | What to say |
|---|---|---|
| `scenarios/15-StoreIntelligenceReport/src/eShopAppHost/Program.cs` | **Orchestration** | Aspire composes `sql`, `products`, `store`; this is the signal-producing app the report summarizes. |
| `scenarios/15-StoreIntelligenceReport/src/Products/Endpoints/ProductEndpoints.cs` | **Signal source** | `GET /api/Product/search/{term}` (keyword) and `GET /api/aisearch/{term}` (semantic) — every search here is a row in tomorrow's report. |
| `scenarios/15-StoreIntelligenceReport/src/Products/Memory/MemoryContext.cs` | **Intent capture** | Same grounding pipeline from Demo 1 — it is what turns a raw query into an *intent* the report can group on. |
| `scenarios/15-StoreIntelligenceReport/src/Store/Components/Pages/Search.razor` | **Where signals are born** | The search box + semantic toggle the presenter drives to generate today's activity. |
| `scenarios/15-StoreIntelligenceReport/docs/report-schema.md` | **Report contract** | The section list the report will produce; the prepared report above follows it. |

## Gap to implement (post-session, optional)

To make this a fully live demo later (tracked in `scenario-map.md`):

1. Add a deterministic input fixture (recent searches, failed searches, cart events).
2. Add a report-generation endpoint on `products` (or a small service) that aggregates signals
   and asks a model for the summary, following `report-schema.md`.
3. Add a **Store Intelligence** page in `store` that calls the endpoint and renders the report.
4. Keep a deterministic fallback (the prepared report above) so the demo never blocks.

## Talking points / FAQ

- **"Is this report generated live?"** Not yet — the pipeline is the gap this scenario will fill.
  Today we run the real app for real signals and present a deterministic report so the story is
  reliable. The *technique* (aggregate signals → summarize → recommend) is what matters.
- **"Where would the data come from?"** The app's own signals: search terms, no-result searches,
  cart/checkout events, and OpenTelemetry traces — not an external dataset.
- **"Cloud or local?"** Report generation could run either way. Demo 1 showed cloud, Demo 2 showed
  local (Foundry Local) — the same report could be produced by either host.
- **"Why does this matter after Observability?"** Observability serves developers; this serves
  business users. Same signals, a different audience and a different decision.

## Notes

- Always start from `scenarios/15-StoreIntelligenceReport/src/eShopAppHost` so `aspire start`
  attaches to **this** scenario, not another one.
- The prepared report is intentionally deterministic; treat it as the demo's source of truth until
  the generation pipeline is implemented.
- This is **Demo 3** in the session. Previous: Demo 2 (Observability, local AI). Next: Demo 4
  (MCP Store Operations Tools, Scenario 16).
