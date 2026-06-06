# Hicks History

## Project Snapshot
- Project: eShopLite
- Requested by: Bruno Capuano
- Stack: Blazor on .NET 10 with Aspire 13.0.1-orchestrated services

## Learnings
- Scenario-driven UI work must stay compatible with per-scenario AppHost orchestration.

## 2026-06-06 — Documentation Modernization (All Scenarios)

### Summary
Updated all markdown documentation to reflect the .NET 10 / Aspire 13.0.1 / MEAI modernization performed by Bishop.

### Files Changed

| File | Changes |
|------|---------|
| `README.md` (top-level) | `.NET 9` → `.NET 10` in prerequisites |
| `AGENTS.md` | `.NET 9` → `.NET 10` throughout; `ConnectionStrings:openai` → new parameter-based secrets; added MEAI and MAF to key technologies |
| `copilot-instructions.md` | `.NET 9` → `.NET 10` in all mentions |
| `scenarios/01-SemanticSearch/README.md` | `.NET 9` → `.NET 10`; replaced old `ConnectionStrings:openai` setup with 4 `Parameters:*` user-secrets in eShopAppHost |
| `scenarios/02-AzureAISearch/README.md` | `.NET 9` → `.NET 10`; old Aspire 9.1 note removed; AOAI connection strings → 4 parameters in eShopAppHost; Azure AI Search still uses Azure CLI credential |
| `scenarios/03-RealtimeAudio/README.md` | `.NET 9` → `.NET 10`; replaced old connection strings for Products+RealtimeStore with 5 parameters in eShopAppHost (adds `AzureOpenAIRealtimeDeploymentName`) |
| `scenarios/04-chromadb/README.md` | `.NET 9` → `.NET 10`; old `openai` user secret → 4 parameters in eShopAppHost; removed "Aspire 9.1" note |
| `scenarios/05-deepseek/README.md` | `.NET 9` → `.NET 10`; replaced 2 connection strings with 7 parameters (`AzureOpenAI*` × 4 + `DeepSeek*` × 3); updated troubleshooting note |
| `scenarios/06-mcp/README.md` | `.NET 9` → `.NET 10`; `ConnectionStrings:openaidev` → 4 parameters; removed duplicate .NET Aspire section |
| `scenarios/07-AgentsConcurrent/README.md` | `.NET 9` → `.NET 10`; `ConnectionStrings:openai` → 4 parameters; title/description/features/references updated from Semantic Kernel to Microsoft Agent Framework; removed old Aspire 9.1 note |
| `scenarios/08-Sql2025/README.md` | Updated `EmbeddingClient` code snippet → `IEmbeddingGenerator`; added local dev user-secrets section |
| `scenarios/09-AzureAppService/README.md` | Added local dev user-secrets section with 4 AOAI parameters |
| `scenarios/10-A2ANet/README.md` | Removed dangling `ConnectionStrings:openai` line at top of file; `.NET 9` → `.NET 10` |
| `scenarios/11-GitHubModels/README.md` | `.NET 9 SDK` → `.NET 10 SDK`; `GitHubToken` → `GitHubModelsToken`; added `dotnet user-secrets` command for local setup |
| `scenarios/12-AzureFunctions/README.md` | Added local dev user-secrets section with 4 AOAI parameters |
| `scenarios/01-SemanticSearch/docs/README.md` | Replaced "Semantic Kernel" with "CommunityToolkit.VectorData.InMemory" in architecture and key technologies; updated link |
| `scenarios/01-SemanticSearch/docs/semantic-search.md` | Updated vector store description from SK to CommunityToolkit; updated Dependencies section package list |
| `scenarios/08-Sql2025/docs/README.md` | `.NET 9.0` → `.NET 10.0` in prerequisites; updated embedding generation snippet; replaced `AI_*` env vars with `Parameters:*` user-secrets |
| `scenarios/08-Sql2025/docs/native-vector-search.md` | `.NET 9.0` → `.NET 10.0` |
| `scenarios/08-Sql2025/docs/sql-server-2025-setup.md` | `.NET 9.0` → `.NET 10.0` in prerequisites |
| `scenarios/01-SemanticSearch/PRD_Add_Payment_Mock_Server.md` | `net9.0` → `net10.0` in assumptions, design, acceptance criteria, and checklist |
| `scenarios/01-SemanticSearch/Shopping_Cart_PRD.md` | `net9.0` → `net10.0` in csproj snippet |

### Key Decisions

1. **Secrets approach**: The new canonical pattern sets 4 Aspire parameters in `eShopAppHost` via `dotnet user-secrets`. No more per-service connection strings in `Products` or `Store` projects.
2. **Scenario 07**: Title, description, features, and references updated from "Semantic Kernel" to "Microsoft Agent Framework (MAF)" — the SK packages were fully replaced.
3. **Scenario 11**: `GitHubToken` parameter renamed to `GitHubModelsToken` to match the actual AppHost `AddParameter` call.
4. **Scenario 02**: Azure AI Search still relies on Azure CLI credential (Aspire provisioning); only the AOAI side switches to explicit parameters.
5. **Code snippets in docs**: Updated outdated `EmbeddingClient`/`AddAzureOpenAIClient` examples to MEAI pattern where present in README/docs files.

### Notable Gotchas Preserved
- 03-RealtimeAudio requires 5 parameters (adds `AzureOpenAIRealtimeDeploymentName`).
- 05-deepseek requires 7 parameters (4 AOAI + 3 DeepSeek).
- 11-GitHubModels is dual-mode: local uses `GitHubModelsToken`; deployed uses the 4 AOAI params.
- 08-Sql2025 test projects pin `Microsoft.EntityFrameworkCore.InMemory` at 9.0.x (not 10.x) due to EFCore.SqlServer.VectorSearch constraint — this is intentional and not reflected in user-facing docs.

## Team Update: .NET 10 Modernization Shipped
**Date:** 2026-06-06 **Status:** ✅ Complete

Fleet completion: All 13 scenarios (.NET 10/Aspire 13) + 1 reference pattern.
- Bishop: 12 scenario modernizations + reference pattern
- Apone: CI bump + infra audit/apply (11 scenarios)
- Hicks: Docs/READMEs/PRDs/speaker-scripts updated
- Vasquez: Verification complete (13/13 Release builds green, 63/63 tests pass)

Approved exceptions documented. Ready for production deployment.
