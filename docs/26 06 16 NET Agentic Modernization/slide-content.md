# App Modernization Done. Now Let's Make It Smarter — slide content (v05)

> v05 keeps all v04 messaging and adds a premium visual system: an accent bar under
> every content title, premium cards (soft shadow + lavender border + purple accent rail),
> and a signature **dark "detail panel"** on every demo slide (the element liked in slide 6).
> Items marked **[v05 added]** are new on-slide content introduced by the redesign;
> everything else is unchanged from v04.

## 1 — Title (brand cover)
.NET Day · Agentic Modernization. June 16, 2026 · 9:00 AM–1:00 PM PDT. *(unchanged)*

## 2 — Speaker intro
App Modernization Done. Now Let's Make It Smarter. Bruno Capuano · Principal Cloud Advocate · Microsoft. Headshot. *(unchanged)*

## 3 — Pitch
"We already saw how to modernize an app, use Aspire, and more. Now that foundation is done, let's make our app smarter." (panel over the agenda brand art). *(unchanged)*

## 4 — Where AI fits in an enterprise app  *(premium 6-card grid)*
Observability · User experience · Business operations · Integrations · Workflows · Deployment (each with a one-line description).
- **[v05 added]** Takeaway strip: "Targeted AI, not one big chatbot — add intelligence where the app already has signals or actions."

## 5 — eShopLite · semantic search (GIF visual). *(unchanged)*

## 6 — Demo 1 · Smarter product discovery (cloud AI)  *(unchanged from v04)*
Subtitle: Smarter UX: AI over product search — users search with intent, not keywords.
Flow: Keyword search → Semantic discovery → Grounded answer.
**MemoryContext.cs** code panel: startup catalog embedding · query embedding + top-3 · Score > 0.3 gate · grounded prompt to the cloud chat model.
Footer: Scenario 14 · ProductDiscoveryCopilot → 01-SemanticSearch · cloud chat model.
Transition: "That used a cloud model. Now let's bring AI local."

## 7 — Demo 2 · Smarter operations (local AI)  *(redesigned)*
Subtitle: Local AI over logs and traces — runs on Foundry Local, no cloud key.
Flow: 1 Trigger → 2 Evidence → 3 AI explanation → 4 Action.
- **[v05 added]** Dark **sample output** panel (local model): Summary (3 services errored in 10 min — products · store · observability-assistant) · Root cause (PaymentClient timeouts after the 14:32 deploy) · Next (3 checks).
Footer: Scenario 13 · ObservabilityAssistantFoundryLocal → 01-SemanticSearch · Foundry Local.
Contrast/transition: "Cloud AI for users · local AI for operations — same modernized app."

## 8 — Demo 3 · Smarter business signals  *(redesigned)*
- **[v05 added]** Subtitle: App signals become a business report — for people who don't read logs.
Flow: 1 App signals → 2 Store report → 3 Decisions.
- **[v05 added]** Dark **sample report** panel: Top searches · Failed search ("all-day walking shoes" — catalog gap) · Actions.
Footer: Scenario 15 · StoreIntelligenceReport → 01-SemanticSearch.

## 9 — Demo 4 · App capabilities as tools  *(redesigned as a terminal panel)*
- **[v05 added]** Subtitle: MCP exposes safe, callable tools — the agent uses the app, not the database.
Dark **terminal panel** (eShopLite.Mcp) listing the six tools: search_catalog · get_product_details · get_failed_searches · get_store_summary · get_recent_operational_summary · generate_store_intelligence_report, plus a sample call line.
Footer: Scenario 16 · MCPStoreOperationsTools → 06-mcp.

## 10 — Demo 5 · Agents collaborate  *(redesigned)*
- **[v05 changed]** Restructured from a six-step strip to three specialist agent cards + a combined-plan panel.
Subtitle: An Orchestrator coordinates three specialist agents, then combines their findings.
Cards: Catalog Agent · Observability Agent · Business Insights.
- **[v05 added]** Dark **combined action plan** panel: Question · Findings · Plan.
Footer: Scenario 17 · A2AStoreOperationsNetwork → 10-A2ANet · Hosted Agents as an evaluation path.

## 11 — From local agents to Hosted Agents  *(premium 3-card flow)*
Local for the demo → Evaluate → Foundry Hosted Agent.
- **[v05 added]** Takeaway: "Agent-ready capabilities already exist — hosting is a deployment choice, not a rewrite. Slide-only."

## 12 — Path to production  *(premium 6-card grid)*
Same AppHost model · Azure Container Apps · Azure App Service · Kubernetes/AKS · Docker Compose · Custom pipelines.
- **[v05 added]** Takeaway: "One app model, many targets — Aspire keeps it consistent from local to production. Slide-only, no live deploy."

## 13 — Why this matters  *(premium 6-card grid)*
Logs nobody reads · Search isn't keywords · Signals, no meaning · Custom glue · One agent isn't enough · Add AI after modernizing.
- **[v05 added]** Takeaway: "Modernization first — then the app has the signals and capabilities that make AI useful."

## 14 — Quote: "Modernization is not the finish line. It is the foundation that lets us make apps smarter." *(unchanged)*

## 15 — Thank you (Bruno Capuano credit + headshot). *(unchanged)*

## 16 — Appendix A · Scenario map  *(premium 6-card grid)*
13 → 01-SemanticSearch · 14 → 01-SemanticSearch · 15 → 01-SemanticSearch · 16 → 06-mcp · 17 → 10-A2ANet · Reliability note.
- **[v05 added]** Takeaway: "Scenarios 13–15 share the search baseline; 16 and 17 reuse MCP and A2A infrastructure for runnable demos."

## 17 — Appendix B · Out of scope
Smart Components · eShopSupport · eShopOnWeb · Aspire MCP / AI coding-agent pitch · Live deployment demo · DevUI as a production feature. *(unchanged)*

## 18 — References
eShopLite · Microsoft Reactor event · Aspire 13.4 Hosted Agents · Aspire deployment · Microsoft Agent Framework · A2A integration · DevUI · Foundry Local. *(unchanged)*

---

**Note on sample data:** the dark detail panels on slides 7, 8, 9, and 10 show
illustrative *sample* output (clearly labelled "sample") to make each demo concrete on
the slide. Replace with real captured output from each scenario's `demo-assets/` folder
before presenting if you prefer live values.
