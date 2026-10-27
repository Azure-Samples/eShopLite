# eShopLite Changelog

## 2026-10-27

### Changed

- Completed the deferred MAFDevUI dependency migration to the repository's .NET 10,
  Aspire 13.6.1, Agent Framework 1.24.0, MEAI 10.11.0, and OpenTelemetry 1.19 baseline.
  Retained compatible DevUI/hosting prereleases and replaced legacy AzureAI integration
  with `Microsoft.Agents.AI.Foundry`.
- Migrated agent construction, existing Foundry agent references, and sequential
  workflow streaming events. Registered the workflow as a DevUI agent and removed
  the nested service provider used during Foundry setup.
- Made local agents the default and Foundry mode explicitly opt-in. Foundry mode
  validates its project endpoint; DevUI remains Development-only and loopback-only.
- Switched Store model calls to the OpenAI-compatible Responses API so reasoning
  models can invoke stock tools. Avoided the Azure SDK Responses wrapper's binary
  incompatibility with the newer OpenAI SDK required by Agent Framework.
- Patched transitive MessagePack and PKCS dependencies and removed redundant
  System.Text.Json references. Added scenario setup and compatibility documentation.

### Validation and known boundaries

- The scenario solution builds in Debug and Release. Eight Products tests and nineteen Store tests pass,
  including partial streamed JSON, no-fallback checkout, actual Store-host Responses
  execution with a fake chat client, key/identity Responses client registration,
  inference endpoint validation, and Production route exclusion.
- The resolved direct/transitive dependency audit reports no known vulnerabilities.
- Isolated Aspire startup reached healthy SQL, Products, and Store resources;
  Store, DevUI, and agent discovery returned HTTP 200, with nine catalog products.
- Live Azure CLI identity validation passed all nine product embeddings, semantic
  search with a generated summary, real stock function invocation, buffered/SSE
  Responses workflows, and the actual checkout orchestrator for Gold, Silver, and
  Normal tiers without deterministic fallback.
- After the user replaced the saved API key, key-based validation also passed
  chat, all nine embeddings, semantic search, three checkout tiers, and streaming
  stock function calls without identity overrides. The earlier HTTP 401 is resolved.
  No Foundry project was configured for optional hosted-agent validation, and no
  cloud deployment was performed.

## 2026-10-14

### Added

- A three-step Scenario 18 quick start, linked requirements/troubleshooting guide,
  blog-to-code walkthrough, detailed reference, and screenshots of the real Store flow.

- [Scenario 18: Blazor Agentic UI](./scenarios/18-BlazorAgenticUI/README.md), an isolated
  .NET 11 RC2 sample using experimental Blazor AI components, Microsoft Agent Framework,
  AG-UI streaming, typed catalog cards, and human-approved session cart actions.
- An explicitly labeled offline fixture using SQLite and deterministic tool calls,
  alongside the real SQL Server 2025 and Foundry model integration.
- Shared Foundry provisioning in `scripts\Initialize-FoundryModels.ps1`, including
  model availability, SKU and quota checks, dry-run support, and AppHost secret setup.
- Mocked setup-script regression coverage and 29 Scenario 18 tests covering catalog
  bounds, cart isolation and approval, AG-UI, Responses transport, and safe error notices.

### Changed

- Refreshed 17 existing scenario solutions to the latest compatible dependency releases,
  including Aspire 13.6.1, Microsoft.Extensions.AI 10.11.0, Agent Framework 1.24.0,
  MCP 2.2.0, A2A 1.0.0-preview3, and OpenTelemetry 1.19 where applicable.
- Pinned the stable root SDK to .NET 10.0.402. Scenario 18 separately pins
  .NET 11 SDK 11.0.100-rc.2.26504.105 for the experimental component package.
- Migrated MCP transports, A2A text parts, concurrent agent APIs, and related tests
  to the updated package APIs. Updated CI setup and local Entity Framework tools.
- Changed the shared setup scripts' default chat deployment to `gpt-6-luna`, with
  `text-embedding-3-small` embeddings. Existing scenario-specific deployment declarations
  are not silently replaced.
- Extended `Set-AzureOpenAISecrets.ps1` to discover Scenario 18 and other AppHost layouts,
  check SDK resolution before requesting credentials, and support a child-only
  `-DotNetRoot` override with environment restoration.

### Fixed

- Added scenario-local Aspire configuration for all 19 AppHosts so bare `aspire start`
  selects the current scenario rather than inheriting the root MAFDevUI selection.
  Added a regression check for the relative paths and effective CLI configuration.

- Scenario 18 loads AppHost user secrets in Development and seeds fixed catalog IDs
  correctly into SQL Server identity columns.
- Scenario 18 uses Responses rather than Chat Completions for `gpt-6-luna` tools with
  reasoning enabled. Added visible progress and safe operational failure notices.
- Updated direct PKCS references to patched 10.0.13 versions in affected dependency graphs.

### Validation and known boundaries

- All 17 refreshed existing solutions built and their 80 existing tests passed.
  Scenario 18 built, passed formatting checks and all 29 tests, and completed a real
  Foundry/SQL browser walkthrough, including rejection, approval, and restart.
- SQL-vector scenarios 08 and 12 retain EF Core and EF tools 9.0.21 because
  `EFCore.SqlServer.VectorSearch` 9.0.0 requires EF 9. A native EF 10 vector migration
  is separate work.
- MAFDevUI was initially deferred; its subsequent migration and validation boundaries
  are recorded in the October 27 entry above.
- Scenario 18 remains an experimental local sample, without production authentication,
  checkout, payment, durable carts, or a cloud deployment template.
