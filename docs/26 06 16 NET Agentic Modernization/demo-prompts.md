# Demo Prompts

## Purpose

This file contains prompt scripts for the session demos.

Use these during rehearsal and live delivery.

## Prompt style rules

- Keep prompts short enough to type or paste live.
- Use app-specific language.
- Ask for structured output.
- Ask the model to stay grounded in app data.
- Avoid generic chatbot prompts.
- Save good outputs as demo backups.

---

## Demo 1 - Observability Assistant

### Short prompt

```text
Summarize the last 10 minutes of application activity and tell me what I should check next.
```

### Main prompt

```text
Summarize the last 10 minutes of application activity. Group issues by service, identify the most likely root cause, include relevant trace IDs if available, and suggest the next three things a developer should check.
```

### Detailed prompt

```text
You are helping a .NET developer understand recent eShopLite activity.

Use only the logs, traces, metrics, and operational events provided by the application.

Create a concise summary with:
1. What happened.
2. Services involved.
3. Errors or warnings grouped by service.
4. Likely root cause candidates.
5. Relevant trace IDs or correlation IDs if available.
6. The next three checks a developer should perform.

Do not invent errors that are not present in the input.
```

### Expected answer shape

```text
## Summary

## Services involved

## Issues grouped by service

## Likely root cause

## Supporting signals

## Next checks
```

---

## Demo 2 - Product Discovery

### Short prompt

```text
Find products that are good for walking all day and explain why each result matches.
```

### Main prompt

```text
Find products that are good for walking all day and explain why each result matches using only the product catalog data.
```

### Alternate prompts

```text
Show me products under $100 that would be useful for a summer trip.
```

```text
Compare these three products and recommend the best one for daily commuting.
```

```text
Find products that would make sense for someone buying a gift for a developer.
```

### Expected answer shape

```text
## Recommended products

For each product:
- Name
- Why it matches
- Relevant catalog attributes
- Confidence / limitations
```

---

## Demo 3 - Store Intelligence Report

### Short prompt

```text
Create today's store intelligence report.
```

### Main prompt

```text
Create today's store intelligence report. Include top searches, failed searches, product opportunities, operational issues that may affect customers, and recommended next actions.
```

### Detailed prompt

```text
You are helping a store operations team understand today's eShopLite activity.

Use only the provided app signals:
- product catalog data,
- search events,
- failed searches,
- cart or order events,
- operational warnings or errors.

Create a concise report with:
1. Executive summary.
2. Top customer intents.
3. Searches with no good result.
4. Product gaps or catalog opportunities.
5. Operational issues that may affect customers.
6. Recommended next actions.

Do not invent revenue, conversion rates, or inventory data unless it is present in the input.
```

### Expected answer shape

```text
## Executive summary

## Top customer intents

## Failed searches

## Product opportunities

## Operational issues

## Recommended actions
```

---

## Demo 4 - MCP Store Tools

### Short prompt

```text
Use the store tools to explain why customers are searching for travel products but not finding good matches.
```

### Main prompt

```text
Customers are searching for travel products. Use the store tools to find matching products, identify failed searches, and summarize product gaps.
```

### Detailed prompt

```text
Use the available eShopLite tools to investigate customer interest in travel products.

Steps:
1. Search the catalog for travel-related products.
2. Get product details for the strongest matches.
3. Check recent failed searches related to travel.
4. Summarize product gaps.
5. Recommend catalog or UX improvements.

Use tool results only. Do not invent products.
```

### Expected answer shape

```text
## Tool calls used

## Matching products

## Failed searches

## Product gaps

## Recommended actions
```

---

## Demo 5 - A2A Store Operations Network

### Short prompt

```text
Customers are searching for travel products but not converting. Investigate why.
```

### Main prompt

```text
Customers are searching for travel products but not converting. Investigate the likely reasons and suggest actions.
```

### Detailed prompt

```text
Use the Store Operations agent network to investigate why customers are searching for travel products but not converting.

Expected collaboration:
1. Catalog Agent checks whether the product catalog has relevant matches.
2. Business Insights Agent checks recent searches, failed searches, and conversion-related signals.
3. Observability Agent checks whether search, catalog, or checkout errors may be affecting the experience.
4. Orchestrator Agent combines the findings into a final action plan.

Return:
- key findings from each agent,
- likely reasons,
- recommended next actions,
- confidence and limitations.
```

### Expected answer shape

```text
## Investigation summary

## Catalog Agent findings

## Business Insights Agent findings

## Observability Agent findings

## Likely reasons

## Recommended action plan

## Confidence and limitations
```

---

## Hosted Agents evaluation prompt

Use this only as a documentation/rehearsal prompt, not as a live demo requirement.

```text
Review the A2A Store Operations Network and decide which agents should remain local for the demo, which could become Microsoft Foundry Hosted Agents later, and what configuration or app changes would be required.
```

## DevUI validation prompt

Use this only for development/testing notes.

```text
Validate the Store Insights Agent behavior with the sample store activity input. Confirm that the output follows the expected report schema and does not invent unsupported metrics.
```
