# DevUI Agent Validation Notes

> DevUI is optional and development-only.
>
> It is not part of the main session pitch and should not be presented as a production feature.

## Purpose

Use DevUI only if it helps validate agent or workflow behavior before integrating the behavior into eShopLite.

## Positioning

DevUI is useful for:

- testing agents,
- iterating prompts,
- testing workflows,
- inspecting tool calls,
- checking traces,
- capturing stable demo outputs.

DevUI is not:

- a production feature,
- a required live demo,
- the main session message,
- a replacement for the eShopLite UI.

## Recommended use in this repo

Use DevUI as a development aid for:

- Store Intelligence Agent,
- A2A Orchestrator Agent,
- Observability Agent,
- local NeMo-style agent adapter if applicable.

## Validation workflow

```text
1. Build the agent or workflow.
2. Validate the prompt and expected output in DevUI.
3. Capture a stable prompt and response.
4. Move the validated behavior into the eShopLite scenario.
5. Save sample output as a fallback demo asset.
```

## What to document

If DevUI is used, create notes that include:

- agent name,
- workflow name,
- prompt,
- input data,
- expected output,
- observed output,
- issues found,
- changes made,
- whether the prompt is ready for the live demo.

## Suggested note format

```markdown
## Agent

## Purpose

## Input

## Prompt

## Expected output

## Actual output

## Changes made

## Ready for demo?
```

## C# documentation note

The current DevUI documentation notes that C# documentation is coming soon and points readers to conceptual guidance.

For this reason:

- keep DevUI optional,
- avoid making the live demo depend on it,
- do not present it as production-ready session content.

## Reference

https://learn.microsoft.com/en-us/agent-framework/devui/
