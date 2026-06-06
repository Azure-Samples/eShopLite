---
name: scenario-baseline-cloning
description: Clone the closest existing eShopLite scenario baseline before adding a new modernization story.
domain: project-conventions
confidence: medium
source: bishop implementation
---

## Pattern
Start new modernization scenarios from the closest working baseline: 01 for most demos, 06 for MCP, and 10 for A2A.

## When to use
Use this when a new scenario needs to stay runnable while the new docs, contracts, or UI are being added.

## Guidance
- Keep the existing baseline projects intact until the new story is stable.
- Document any non-01 baseline choice in the scenario README.
- Preserve explicit Aspire parameter wiring and avoid hidden fallback behavior.

