# Copilot Next Steps - .NET Agentic Modernization Session

## Goal

Refine the session documentation and prepare the repo for implementation/rehearsal of the new eShopLite scenarios.

The current work should focus on content quality first:

1. session flow,
2. slide content,
3. demo story,
4. scenario documentation,
5. runbook and prompts.

Do not start with large code changes.

## Phase 1 - Review and gap analysis

Before editing files, inspect:

```text
docs/26 06 16 NET Agentic Modernization/
```

Then produce a summary with:

```text
1. What files exist.
2. Which files already match the intended story.
3. Which files need more detail.
4. Which files have inconsistencies.
5. Which scenario folders exist or are missing.
6. Which scenario README files exist or are missing.
7. Which content still sounds like a placeholder.
8. Recommended changes before code implementation.
```

## Phase 2 - Refine session-flow.md

File:

```text
docs/26 06 16 NET Agentic Modernization/session-flow.md
```

Improve:

- minute-by-minute flow,
- speaker lines,
- transitions,
- audience takeaways,
- demo intent,
- fallback plans,
- timing realism.

Acceptance criteria:

- The session can be rehearsed from this file.
- Every section has a purpose and transition.
- It clearly avoids Aspire MCP / AI coding-agent pitch.
- It keeps the focus on smarter upgraded apps.

## Phase 3 - Refine slide-content.md

File:

```text
docs/26 06 16 NET Agentic Modernization/slide-content.md
```

Improve:

- slide titles,
- slide messages,
- bullets,
- speaker notes,
- suggested visuals,
- demo prompts,
- transitions,
- appendix slides.

Acceptance criteria:

- Content is slide-ready.
- Slides are not too text-heavy.
- Speaker notes carry the detail.
- The deck tells one coherent story.
- Hosted Agents are positioned as evaluation/extension, not a required live demo.
- Deploy with Aspire is slide-only.

## Phase 4 - Refine scenario-map.md

File:

```text
docs/26 06 16 NET Agentic Modernization/scenario-map.md
```

Improve:

- mapping from slides to scenarios,
- scenario source baseline,
- scenario goals,
- demo type,
- expected output.

Acceptance criteria:

- Every scenario maps to a session section.
- Each scenario explains why it exists.
- Scenario 01 is the default source baseline.
- MCP/A2A exceptions are clearly explained.

## Phase 5 - Refine demo-runbook.md

File:

```text
docs/26 06 16 NET Agentic Modernization/demo-runbook.md
```

Improve:

- prerequisites,
- commands,
- exact demo steps,
- prompts,
- expected results,
- fallback assets,
- troubleshooting.

Acceptance criteria:

- Bruno can rehearse from this file.
- Every demo has a fallback.
- Commands are realistic for the repo.
- No live cloud deployment dependency.

## Phase 6 - Refine demo-prompts.md

File:

```text
docs/26 06 16 NET Agentic Modernization/demo-prompts.md
```

Improve:

- short prompt,
- main prompt,
- detailed prompt,
- expected answer shape,
- fallback prompt.

Acceptance criteria:

- Prompts are app-specific.
- Prompts request grounded output.
- Prompts avoid unsupported claims.
- Prompts are easy to paste during the session.

## Phase 7 - Refine Hosted Agents docs

Files:

```text
docs/26 06 16 NET Agentic Modernization/hosted-agents-evaluation.md
docs/26 06 16 NET Agentic Modernization/agent-framework-positioning.md
docs/26 06 16 NET Agentic Modernization/devui-agent-validation-notes.md
```

Improve:

- decision table,
- agent roles,
- local vs hosted decision,
- Agent Framework positioning,
- DevUI optional/dev-only position,
- practical next steps.

Acceptance criteria:

- Hosted Agents are an extension/evaluation path.
- The live demo can remain local.
- DevUI is not described as production.
- Microsoft Agent Framework is positioned as app-code layer.
- A2A is positioned as agent collaboration, not a framework comparison.

## Phase 8 - Review scenario folders 13-17

Expected folders:

```text
scenarios/13-ObservabilityAssistantFoundryLocal
scenarios/14-ProductDiscoveryCopilot
scenarios/15-StoreIntelligenceReport
scenarios/16-MCPStoreOperationsTools
scenarios/17-A2AStoreOperationsNetwork
```

For each scenario, verify or create README sections:

```text
# Scenario title

## Goal
## Source scenario
## Why this scenario exists
## Architecture
## How to run
## Demo flow
## Demo prompts
## Expected output
## Fallback plan
## Troubleshooting
## Session speaker notes
```

Do not implement large code changes until the scenario docs are coherent.

## Phase 9 - Produce implementation plan

After docs are refined, produce a separate implementation plan with:

```text
1. Required scenario code changes.
2. Shared components that should be reused.
3. AI/model dependencies.
4. Local vs cloud dependencies.
5. Test/validation steps.
6. Demo risk level.
7. Recommended implementation order.
```

## Recommended implementation order

```text
1. Refine docs.
2. Validate scenario folders and READMEs.
3. Implement Scenario 13.
4. Implement Scenario 14.
5. Implement Scenario 15.
6. Implement Scenario 16.
7. Implement Scenario 17.
8. Add fallback demo assets.
9. Rehearse.
```

## Quality gate for docs

Before committing:

```powershell
git diff -- "docs/26 06 16 NET Agentic Modernization"
```

Check:

- no obsolete Smart Components references,
- no eShopSupport,
- no eShopOnWeb,
- no Aspire MCP / AI coding-agent pitch,
- no live deployment requirement,
- no DevUI production framing,
- core story repeated consistently.

## Quality gate for scenario docs

For each scenario README:

- source scenario is stated,
- source reason is stated,
- run commands are present,
- demo prompt is present,
- expected output is present,
- fallback plan is present.
