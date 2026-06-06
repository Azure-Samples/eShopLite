---
name: "docs-standards"
description: "Keeps scenario README files and session docs aligned with the repo's documentation conventions."
domain: "documentation"
confidence: "high"
source: "earned"
tools:
  # - name: "view"
  #   description: "Inspect existing docs and README structure"
  #   when: "when updating scenario documentation"
---

## Context
Use this skill when adding or editing scenario READMEs, session documentation, or docs that need to explain architecture and deployment flows.

## Patterns
- State what the scenario demonstrates.
- Explicitly name the scenario it was derived from and why.
- Include prerequisites, configuration, run steps, expected output, and troubleshooting.
- Link back to shared session docs when the scenario supports a larger story.

## Examples
- Scenario 09 now calls out that it is derived from Scenario 01 - Semantic Search.
- Scenario docs include a session-docs backlink so readers can move from the implementation to the presentation material.

## Anti-Patterns
- Leaving the source scenario implicit.
- Documenting only screenshots without setup or troubleshooting.
- Splitting the story across docs without a backlink to the session package.
