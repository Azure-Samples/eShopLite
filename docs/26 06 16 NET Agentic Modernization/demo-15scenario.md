# Demo 3 Playbook - Scenario 15 Store Intelligence Report

> **Format: fully live.** The report pipeline **is implemented** in this scenario. The app
> captures real search signals (including failed/no-result searches), and a **Store Intelligence**
> page generates a business summary from those signals on demand. It uses the same Azure OpenAI
> `IChatClient` as Demo 1 for the narrative, with a **deterministic fallback** so the demo never
> blocks — the report is identical in shape whether the model is reachable or not, and a small
> `source: ai | fallback` badge proves which path ran.

## Objective and presenter story

**Objective:** show that the same modernized app that serves users (Demo 1) and explains its operations (Demo 2) can also roll its raw signals up into a **business summary** that a non-developer — a store manager — can act on.

**Story line:**

1. The app already produces signals: searches and failed (no-result) searches.
2. Developers and ops see logs and traces; **business users** need patterns and next actions.
3. We click **Generate report** on the Store Intelligence page.
4. The report turns scattered signals into: top intents, gaps (no-result searches), product opportunities, operational issues, and recommended actions.
5. This proves AI value is not only UX and ops — it is **decision support** built from the app's own data.

## Where this sits in the arc

- **Demo 1 (Product Discovery, cloud AI):** better answers for *users*.
- **Demo 2 (Observability Assistant, local AI):** better understanding for *developers/ops*.
- **Demo 3 (Store Intelligence, this demo):** better summaries for *business users*.

Same modernized app, same signals — a third audience served.

## How it works (the one-paragraph version)

Every keyword and semantic search records a `StoreSignal` (term, semantic flag, result count,
timestamp) into an in-memory `StoreSignalStore` on the `products` service. The store is **seeded
with 8 sample signals at startup** (two of them no-result), so the report is never empty even
before you search. The **Store Intelligence** page calls `GET /api/intelligence/report`, which
aggregates the signals (top searches, failed searches, product opportunities, operational issues),
asks the chat model to write the executive summary + recommended actions, and falls back to a
deterministic narrative when the model is unavailable. The page renders the sections and shows the
`source` badge.

## Current state

- Scenario 15 builds on the **Scenario 01 Semantic Search baseline** (`sql` + `products` + `store`)
  and **adds** the Store Intelligence Report end-to-end:
  - signal capture in the search endpoints,
  - a `StoreSignalStore` + `StoreIntelligenceReportService` on `products`,
  - two endpoints (`/api/intelligence/signals`, `/api/intelligence/report`),
  - a **Store Intelligence** page (`/intelligence`) + nav link in `store`.
- Verified end-to-end: page renders, **Generate report** returns a full report with
  `source: ai`, failed searches (e.g., "hiking boots size 12", "paint my room white") surface as
  product opportunities and an elevated no-result-rate operational issue.

## Prerequisites

- .NET 10 SDK, Aspire CLI, and Docker Desktop running.
- Scenario source available at:
  - `D:\azure-samples\eShopLite\scenarios\15-StoreIntelligenceReport`
- For the **AI** narrative (`source: ai`), the Products service needs the four Azure OpenAI
  parameters (same as Scenario 01/14):
  - `Parameters:AzureOpenAIEndpoint`, `Parameters:AzureOpenAIApiKey`,
    `Parameters:AzureOpenAIDeploymentName`, `Parameters:AzureOpenAIEmbeddingsDeploymentName`
  - Without them the report still generates with `source: fallback` — same sections, deterministic
    text. The demo works either way.

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
2. **Open the Store Intelligence page:** from the Aspire Dashboard open the `store` endpoint, then
   click **Store Intelligence** in the left nav (or browse to `/intelligence`).
3. **Show the raw signals first:** click **Refresh signals**. The table fills from the **seeded
   sample signals** — point at the two no-result rows (`resultCount = 0`). "These are the app's own
   signals — scattered events, not yet a story."
4. **Generate real signals (optional but great on stage):** open **Search**, run a mix so the
   report reflects what the room just watched:
   - `do you have something for cooking`
   - `do you have something for a rainy day`
   - `winter camping tent four season`
   - `something to paint my room white`  → no results → a *failed search* signal
   - `do you have hiking boots size 12`  → no results → another *failed search* signal
   Toggle **Use Semantic Search** on for a couple to vary the intent signals. Return to **Store
   Intelligence** and click **Refresh signals** — your new searches now appear in the table.
5. **Click Generate report (the business ask):** this calls `GET /api/intelligence/report`. In a
   second or two the report card renders.
6. **Walk the report top-down:** executive summary → top customer intents → searches with no
   results → product opportunities → operational issues → recommended actions.
7. **Point at the proof line:** the `source: ai` badge (or `source: fallback` if no Azure OpenAI).
   "Same report shape either way — the badge tells you which path ran. The fallback means the demo
   never blocks."
8. **Map a section to what they just did (the key move):** tie a report line (e.g., "hiking boots
   size 12 — no matching product") back to the exact search the audience watched you run.
9. **Close with value line (15s):**
   "The app already had this data. The report just turns it into decisions a non-developer can act on."

## The business ask (what Generate report answers)

The **Generate report** button answers this question. Say it out loud as you click:

```text
Create today's store intelligence report. Include top searches, failed searches, product
opportunities, operational issues that may affect customers, and recommended next actions.
```

## Expected report shape

The rendered report follows the scenario's report schema
(`scenarios/15-StoreIntelligenceReport/docs/report-schema.md`):

```text
Store Intelligence Report          (source: ai | fallback · N signals analyzed)
- Executive summary
- Top customer intents
- Searches with no results
- Product opportunities
- Operational issues
- Recommended actions
```

Example (real output captured from a live run, `source: ai`, 8 signals):

```text
Executive summary
- 8 search signals analyzed.
- Top searches (each x1): rainy day gear; camp cooking; four season tent; winter camping tent; rain jacket.
- 2 searches returned no results: hiking boots size 12; paint my room white.
- Product opportunity: demand for hiking boots size 12 with no matching product; operational
  issue: elevated no-result rate (2/8) — verify catalog coverage and search path.

Searches with no results
- hiking boots size 12
- paint my room white

Product opportunities
- Demand for "hiking boots size 12" with no matching product — review catalog/size coverage.

Operational issues
- Elevated no-result rate (2/8 searches) — verify catalog coverage and the search path.

Recommended actions
1. Review catalog and size coverage for hiking boots size 12.
2. Verify catalog coverage and investigate the search path for the elevated no-result rate.
```

> Exact wording varies (the AI writes the summary); the **shape** (executive summary → intents →
> failed searches → opportunities → operational issues → actions) is what to land.

## How the report maps to app signals

This is the slide-worthy point: every report line traces to a signal the app already emits.

| Report section | App signal it comes from | Where in the app |
|---|---|---|
| Top customer intents | Search terms hitting the Products API | `GET /api/Product/search/{term}` and `GET /api/aisearch/{term}`, recorded as `StoreSignal` |
| Searches with no results | Searches that returned zero products | `StoreSignal.Failed` (`resultCount == 0`) from the same endpoints |
| Product opportunities | On-catalog intents with no matching SKU | Failed-but-relevant searches (e.g., "size 12") |
| Operational issues | Elevated no-result rate on the search path | Aggregated from the signal set in `StoreIntelligenceReportService` |
| Recommended actions | Derived from the gaps + ops signals above | AI narrative (or deterministic fallback) in the report service |

## Fallback plan

The report has a **built-in deterministic fallback**, so this demo has no hard live-AI failure mode:

- If **Azure OpenAI is unavailable**: the report still generates with `source: fallback` — identical
  sections, deterministic text. Just narrate the badge.
- If **semantic** search is unavailable (no secrets): use **keyword** search only — the signals
  (top searches, failed searches) are identical for the report story.
- If the **app** will not start: present the example report above and describe which searches
  produce each section, then continue to **Demo 4 (MCP Store Tools)** without retry loops.

## Key files to talk about

| File | Role | What to say |
|---|---|---|
| `scenarios/15-StoreIntelligenceReport/src/Products/Intelligence/StoreSignalStore.cs` | **Signal capture** | Thread-safe in-memory ring of recent searches; seeded with 8 sample signals (2 no-result) so the report is never empty. |
| `scenarios/15-StoreIntelligenceReport/src/Products/Intelligence/StoreIntelligenceReportService.cs` | **The report engine** | Aggregates signals into sections, asks `IChatClient` for the summary + actions, falls back deterministically (sets `source`). The heart of the demo. |
| `scenarios/15-StoreIntelligenceReport/src/Products/Endpoints/IntelligenceEndpoints.cs` | **The API** | `GET /api/intelligence/signals` and `GET /api/intelligence/report` — what the page calls. |
| `scenarios/15-StoreIntelligenceReport/src/Store/Components/Pages/StoreIntelligence.razor` | **The business UI** | The `/intelligence` page: Refresh signals, Generate report, rendered sections, `source` badge. |
| `scenarios/15-StoreIntelligenceReport/src/Products/Endpoints/ProductApiActions.cs` | **Where signals are born** | `SearchAllProducts` records a `StoreSignal` for every keyword search (semantic search records in `ProductAiActions.cs`). |
| `scenarios/15-StoreIntelligenceReport/docs/report-schema.md` | **Report contract** | The section list the report produces. |

## Talking points / FAQ

- **"Is this report generated live?"** Yes. The page calls a real endpoint that aggregates the
  app's signals and asks the model for the summary. The `source` badge shows whether AI or the
  deterministic fallback produced it.
- **"Where does the data come from?"** The app's own signals: search terms and no-result searches,
  captured as `StoreSignal` on the `products` service — not an external dataset.
- **"What if there are no searches yet?"** The store seeds 8 sample signals at startup, so the
  report always has material. Live searches simply add to it.
- **"Cloud or local?"** This demo uses the same cloud `IChatClient` as Demo 1. Demo 2 showed the
  local (Foundry Local) path — the same report could be produced by either host.
- **"Why does this matter after Observability?"** Observability serves developers; this serves
  business users. Same signals, a different audience and a different decision.

## Notes

- Always start from `scenarios/15-StoreIntelligenceReport/src/eShopAppHost` so `aspire start`
  attaches to **this** scenario, not another one.
- The report is deterministic in **shape**; only the executive-summary/recommended-actions wording
  changes between the AI and fallback paths.
- This is **Demo 3** in the session. Previous: Demo 2 (Observability, local AI). Next: Demo 4
  (MCP Store Operations Tools, Scenario 16).

## Reference links

- [Microsoft.Extensions.AI `IChatClient`](https://learn.microsoft.com/en-us/dotnet/ai/) — Unified interface for chat requests; powers the report narrative in `StoreIntelligenceReportService`.
- [.NET Aspire Orchestration](https://aspire.dev) — Multi-project composition (`sql` + `products` + `store`); wires services and AI clients for the business intelligence pipeline.
- [Building with Blazor and .NET](https://learn.microsoft.com/en-us/aspnet/core/blazor/) — Blazor powers the Store UI; the Store Intelligence page renders the report and signals.
