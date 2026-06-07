# Scenario Implementation Plan (13-17)

## Objective

Deliver reliable **local runnable demos** for session scenarios 13-17, without adding large new feature scope in this pass.

## Prioritization principle

1. Keep each scenario runnable from `src\eShopAppHost` with `dotnet run`.
2. Close doc/code mismatches that break demo confidence.
3. Add deterministic fallback data/output before optional cloud enhancements.

## Scenario 13 - ObservabilityAssistantFoundryLocal

- **Current status (code readiness):**
  - App code is effectively a copy of Scenario 01 baseline.
  - README says scaffold-only, but `src\` is runnable baseline.
- **Missing code/docs/assets:**
  - Missing scenario-specific assistant endpoint/UI flow.
  - `docs\README.md` is still Semantic Search-focused.
  - No explicit deterministic incident fixture described.
- **Minimum runnable milestone:**
  - Keep current app runnable.
  - Add deterministic observability-summary script/output path for demo.
  - Update README/docs to match actual behavior.
- **Dependencies:**
  - **Local required:** .NET, Docker SQL container via Aspire.
  - **Cloud optional:** Azure OpenAI/Foundry Local provider swap.
- **Validation steps:**
  1. `cd scenarios\13-ObservabilityAssistantFoundryLocal\src\eShopAppHost`
  2. `dotnet run`
  3. Verify Store + Products load and demo script steps execute.
- **Demo fallback:**
  - Prepared “last 10 minutes” summary output from deterministic fixture.
- **Implementation order + estimate:**
  - Docs alignment first, then deterministic summary path.
  - **Effort:** 0.5-1.0 day.

## Scenario 14 - ProductDiscoveryCopilot

- **Current status (code readiness):**
  - Runnable baseline copied from Scenario 01.
  - Copilot-specific flow not implemented yet.
- **Missing code/docs/assets:**
  - No dedicated product-discovery/copilot UI or endpoint.
  - `docs\README.md` still references Semantic Search scenario text.
  - No scenario-specific screenshots/prompts package.
- **Minimum runnable milestone:**
  - Use current semantic search as local runnable core.
  - Add deterministic “explain why matched” response layer (minimal).
  - Update docs/prompts to avoid claiming missing features.
- **Dependencies:**
  - **Local required:** baseline scenario services.
  - **Cloud optional:** Azure OpenAI for richer explanations.
- **Validation steps:**
  1. `cd scenarios\14-ProductDiscoveryCopilot\src\eShopAppHost`
  2. `dotnet run`
  3. Run keyword and natural-language prompts; verify grounded response text.
- **Demo fallback:**
  - Prepared grounded recommendation output using fixed catalog inputs.
- **Implementation order + estimate:**
  - Prompt/output shaping before UI polish.
  - **Effort:** 1.0-1.5 days.

## Scenario 15 - StoreIntelligenceReport

- **Current status (code readiness):**
  - Runnable baseline copied from Scenario 01.
  - Store intelligence report feature is not implemented.
- **Missing code/docs/assets:**
  - No report-generation endpoint/page pipeline yet.
  - Docs still mostly Semantic Search inherited content.
  - No committed deterministic event fixture for report inputs.
- **Minimum runnable milestone:**
  - Add deterministic input fixture + simple report generator output.
  - Expose one UI or API entry point for “today report”.
  - Align README/demo script with real commands and expected output.
- **Dependencies:**
  - **Local required:** baseline services + fixture data.
  - **Cloud optional:** LLM-based summarization.
- **Validation steps:**
  1. `cd scenarios\15-StoreIntelligenceReport\src\eShopAppHost`
  2. `dotnet run`
  3. Generate and verify report sections against fixture data.
- **Demo fallback:**
  - Pre-generated report artifact tied to fixture timestamp.
- **Implementation order + estimate:**
  - Fixture + schema first, generation second, UI last.
  - **Effort:** 1.0-2.0 days.

## Scenario 16 - MCPStoreOperationsTools

- **Current status (code readiness):**
  - Runnable, based on Scenario 06 MCP assets.
  - Active tools are in `src\eShopMcpSseServer\Tools` and include weather/park/online research plus product search.
- **Missing code/docs/assets:**
  - Tool contracts are not aligned with session store-operations tool list.
  - Docs are minimal and do not include full run/validate matrix.
- **Minimum runnable milestone:**
  - Keep existing MCP flow runnable.
  - Publish “session-safe tool subset” mapping current tools to demo prompts.
  - Add explicit gap list for store-ops-target tools.
- **Dependencies:**
  - **Local required:** MCP server + Products + Store.
  - **Cloud optional:** Azure OpenAI-assisted tool chaining.
- **Validation steps:**
  1. `cd scenarios\16-MCPStoreOperationsTools\src\eShopAppHost`
  2. `dotnet run`
  3. Confirm MCP server starts and at least 2 mapped tools execute successfully.
- **Demo fallback:**
  - Tool contract walkthrough + captured request/response transcripts.
- **Implementation order + estimate:**
  - Contract alignment docs first, tool renaming/additions after.
  - **Effort:** 1.0 day for docs alignment; 1.0-2.0 days for tool alignment.

## Scenario 17 - A2AStoreOperationsNetwork

- **Current status (code readiness):**
  - Runnable, based on Scenario 10 A2A assets.
  - Current agent set is Inventory/Promotions/Researcher.
- **Missing code/docs/assets:**
  - Session narrative expects Catalog/Observability/Business Insights roles.
  - README says scaffold, but code is not scaffold-only.
  - Need role-mapping doc to avoid mismatch during live demo.
- **Minimum runnable milestone:**
  - Keep current A2A flow runnable.
  - Add role mapping: current agents -> session role language.
  - Update prompts and demo-script to match implemented behavior.
- **Dependencies:**
  - **Local required:** A2A projects in scenario `src\`.
  - **Cloud optional:** Hosted agent evaluation only (not required live).
- **Validation steps:**
  1. `cd scenarios\17-A2AStoreOperationsNetwork\src\eShopAppHost`
  2. `dotnet run`
  3. Execute A2A search and verify orchestrated multi-agent output.
- **Demo fallback:**
  - Sequence diagram + sample inter-agent messages + expected synthesis output.
- **Implementation order + estimate:**
  - Role/prompt remap first, deeper refactor later.
  - **Effort:** 0.5-1.0 day for docs/demo alignment; 2.0+ days for role refactor.

## Remove Zava remnants

The following files contain Zava-specific text that conflicts with eShopLite session guidance:

1. `scenarios\13-ObservabilityAssistantFoundryLocal\PRD_Add_Payment_Mock_Server.md`
2. `scenarios\14-ProductDiscoveryCopilot\PRD_Add_Payment_Mock_Server.md`
3. `scenarios\15-StoreIntelligenceReport\PRD_Add_Payment_Mock_Server.md`

### Replacement actions

- Replace title/reference text:
  - `Zava-Aspire` -> `eShopLite Aspire scenario`
  - `src/ZavaAppHost` -> `src/eShopAppHost`
- Add a header note:
  - “Legacy PRD imported from prior repo context; pending adaptation for this scenario.”
- If payment mock server is out of session scope, move these PRDs to `docs\archive` (or remove from scenario root) to avoid demo confusion.

## Recommended implementation order (session reliability first)

1. **Documentation truth pass** (13-17): align README/demo-script/status with current runnable reality.  
   **Estimate:** 0.5 day.
2. **Fallback hardening** (13-15): deterministic outputs for observability/discovery/report demos.  
   **Estimate:** 1.0-2.0 days.
3. **MCP alignment pass** (16): map or add store-operations tools used in session prompts.  
   **Estimate:** 1.0-2.0 days.
4. **A2A narrative alignment pass** (17): role mapping + prompt/script consistency; optional refactor later.  
   **Estimate:** 0.5-1.0 day (docs only), 2.0+ days (code role refactor).

## Exit criteria for session-ready demos

- Each scenario 13-17 starts with `dotnet run` from `src\eShopAppHost`.
- Each demo has a verified happy path and a deterministic fallback.
- No scenario-level docs claim capabilities not present in code.
- No visible Zava references remain in session-target scenario docs.
