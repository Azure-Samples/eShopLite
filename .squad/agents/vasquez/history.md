# Vasquez History

## Project Snapshot
- Project: eShopLite
- Requested by: Bruno Capuano
- Stack: xUnit test projects in scenario `src` folders

## Learnings
- Team setup includes explicit QA ownership from day one for cross-scenario reliability.
- 14-MAFDevUI solution is at the scenario root (not under `src\`); AppHost is at `eShopAppHost\Program.cs` relative to scenario root.
- NU1902/NU1510/NU1605 OTel 1.14.0 vulnerability warnings are expected and non-blocking.
- `Microsoft.SemanticKernel.Connectors.AzureAISearch` in 02-AzureAISearch is the sole documented allowed SK exception (pure VectorData connector).

## Session Log

### 2026-06-06T12:50:00-04:00 — Full-Repo Modernization Verification (requested by Bruno Capuano)

**Task:** Verify all 13 scenarios (01–12, 14) upgraded to net10.0 + Aspire 13.0.1, SK replaced by MEAI+CommunityToolkit.VectorData.InMemory+MAF, Azure OpenAI secrets moved to Aspire parameters.

**Verdict: ✅ ALL GREEN**

| Scenario | Build | Tests | SK Hits | Params |
|---|---|---|---|---|
| 01-SemanticSearch | ✅ | 7P/0F/0S | None | ✅ |
| 02-AzureAISearch | ✅ | n/a | Allowed (SK.Connectors.AzureAISearch) | ✅ |
| 03-RealtimeAudio | ✅ | n/a | None | ✅ |
| 04-chromadb | ✅ | n/a | None | ✅ |
| 05-deepseek | ✅ | n/a | None | ✅ |
| 06-mcp | ✅ | n/a | None | ✅ |
| 07-AgentsConcurrent | ✅ | n/a | None | ✅ |
| 08-Sql2025 | ✅ | 7P/0F/0S | None | ✅ |
| 09-AzureAppService | ✅ | 7P/0F/0S | None | ✅ |
| 10-A2ANet | ✅ | 9P/0F/0S | None | ✅ |
| 11-GitHubModels | ✅ | 7P/0F/0S | None | ✅ |
| 12-AzureFunctions | ✅ | 13P/0F/0S | None | ✅ |
| 14-MAFDevUI | ✅ | 9P/0F/0S | None | ✅ |

Total tests: 63 passed, 0 failed, 0 skipped.  
Full details in `.squad\decisions\inbox\vasquez-verify.md`.

## Team Update: .NET 10 Modernization Shipped
**Date:** 2026-06-06 **Status:** ✅ Complete

Fleet completion: All 13 scenarios (.NET 10/Aspire 13) + 1 reference pattern.
- Bishop: 12 scenario modernizations + reference pattern
- Apone: CI bump + infra audit/apply (11 scenarios)
- Hicks: Docs/READMEs/PRDs/speaker-scripts updated
- Vasquez: Verification complete (13/13 Release builds green, 63/63 tests pass)

Approved exceptions documented. Ready for production deployment.
