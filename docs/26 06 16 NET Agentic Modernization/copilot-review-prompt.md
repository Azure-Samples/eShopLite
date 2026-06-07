# Copilot Review Prompt

Use this prompt after copying the updated docs into the repo.

```text
I updated the session documentation under:

/docs/26 06 16 NET Agentic Modernization/

Please review the new documentation and align the repo with it.

Important rules:
- Work only in the branch: bruno-NETAgenticModernizationSession
- The session focus is: we already modernized the .NET app, now we make it smarter with targeted AI features.
- Do not add Aspire MCP / AI coding-agent content to the session pitch. That belongs to the separate Aspire session.
- New scenarios should start from Scenario 01 - Semantic Search by default.
- Use another scenario as the baseline only when needed, for example MCP, A2A, or deployment-specific infrastructure.
- Keep deployment slide-only.
- Keep DevUI optional and development-only.
- For agent scenarios, evaluate whether agents could later be exposed as Microsoft Foundry Hosted Agents through Aspire, but do not make Hosted Agents a live-demo requirement.

Tasks:
1. Review session-flow.md and slide-content.md as the source of truth.
2. Check whether README.md, demo-runbook.md, demo-prompts.md, scenario-map.md, hosted-agents-evaluation.md, agent-framework-positioning.md, devui-agent-validation-notes.md, deploy-with-aspire-slide-notes.md, top-5-pain-points.md, and references.md match the same story.
3. Suggest or apply documentation updates to remove inconsistencies.
4. Check scenario folders 13-17 and verify that each scenario README explains:
   - the goal,
   - the source scenario it was derived from,
   - how to run it,
   - the demo flow,
   - expected output,
   - fallback plan.
5. Do not make large code changes yet unless they are required to make documentation accurate.
6. Produce a summary of gaps before implementing code changes.
```
