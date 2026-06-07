# Scenario README Template

Use this template for each new scenario README.

```markdown
# Scenario XX - Scenario Name

## Goal

Explain the app modernization value this scenario demonstrates.

## Source scenario

This scenario is derived from:

```text
01 - Semantic Search
```

If this scenario uses another source baseline, explain why.

## Why this scenario exists

Explain how this scenario supports the session:

```text
The app was upgraded. Now this scenario makes one part of the app smarter.
```

## Session section

Map to the session section:

```text
Smarter operations / Smarter UX / Smarter business operations / Smarter integrations / Smarter workflows
```

## Architecture

Describe the main components.

Optional diagram:

```text
Input
  ↓
App capability / data / telemetry
  ↓
AI service / agent / tool
  ↓
Grounded output
```

## How to run

```bash
dotnet run --project src/eShopLite.AppHost
```

Adjust the command to the actual scenario project structure.

## Demo flow

1. Step one.
2. Step two.
3. Step three.
4. Expected result.

## Demo prompts

```text
Prompt here.
```

## Expected output

Describe the expected output shape.

## Fallback plan

Explain what to show if the live demo fails.

## Troubleshooting

List known issues and fixes.

## Session speaker notes

Add the speaker lines Bruno should use during the demo.

## Out of scope

Mention any limitations or future work.
```
