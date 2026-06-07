# Copilot Quality Checklist

Use this before committing changes.

## Branch

Confirm:

```powershell
git branch --show-current
```

Expected:

```text
bruno-NETAgenticModernizationSession
```

## Docs quality

Check all docs under:

```text
docs/26 06 16 NET Agentic Modernization/
```

The docs must:

- support the same session story,
- avoid contradictions,
- feel like a speaker kit,
- include practical demo guidance,
- include fallback plans,
- avoid placeholder language.

## Required docs

Expected files:

```text
README.md
session-flow.md
slide-content.md
demo-runbook.md
demo-prompts.md
scenario-map.md
hosted-agents-evaluation.md
agent-framework-positioning.md
devui-agent-validation-notes.md
deploy-with-aspire-slide-notes.md
top-5-pain-points.md
references.md
copilot-handoff-context.md
copilot-next-steps.md
copilot-start-prompt.md
```

## Scope checks

Search for terms that should not appear as main pitch items:

```powershell
Select-String -Path "docs/26 06 16 NET Agentic Modernization/*.md" -Pattern "Smart Components","eShopSupport","eShopOnWeb","Aspire MCP","AI coding agents"
```

Acceptable:

- "Aspire MCP" may appear only as out-of-scope guidance.
- "AI coding agents" may appear only as out-of-scope guidance.

Not acceptable:

- main slide content promoting Aspire MCP,
- main session flow about AI coding agents,
- Smart Components scenario.

## Scenario docs checks

For each scenario 13-17 README:

- has source scenario,
- explains why source was chosen,
- has run commands,
- has demo flow,
- has prompt,
- has expected output,
- has fallback plan,
- maps to slide/session section.

## Slide checks

`slide-content.md` should have:

- title slide,
- modernization foundation,
- enterprise app map,
- eShopLite baseline,
- five demo slides,
- Hosted Agents evaluation slide,
- Deploy with Aspire slide,
- top 5 pain points,
- final message,
- appendix.

## Session flow checks

`session-flow.md` should have:

- 30-minute timing,
- section goals,
- speaker lines,
- transitions,
- audience takeaways,
- fallback plans.

## Commit suggestion

```powershell
git add "docs/26 06 16 NET Agentic Modernization"
git commit -m "Refine .NET Agentic Modernization session handoff docs"
```
