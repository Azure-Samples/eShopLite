# eShopLite Changelog

## 2026-10-14

### Added

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
- `scenarios\14-MAFDevUI` was not refreshed. Its local instructions require an unavailable
  Azure best-practices tool before Azure-related work. This is a tooling prerequisite
  blocking upgrade validation, not evidence of a reproduced application runtime failure.
- Scenario 18 remains an experimental local sample, without production authentication,
  checkout, payment, durable carts, or a cloud deployment template.
