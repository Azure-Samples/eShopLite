---
name: "aspire-session-packaging"
description: "Package an Aspire modernization session with docs, demo flow, and architecture notes"
domain: "aspire-session"
confidence: "medium"
source: "earned"
---

## Context

Use this skill when turning an Aspire modernization plan into a session package. The goal is to keep the story documentation-first, grounded in the AppHost model, and easy to demo.

## Patterns

### 1. Build the session bundle
Create a folder that includes:
- README
- session abstract
- flow/timing
- speaker script
- demo runbook
- demo prompts
- slide outline
- top pain points
- scenario map
- hosted-agent evaluation
- agent-framework positioning
- DevUI notes
- deployment notes
- references

### 2. Anchor to real AppHost baselines
Point the session at concrete AppHost examples in the repo. Reuse existing scenario code when the planned scenario scaffolds do not exist yet.

### 3. Keep deployment story separate
Use a slide-based closing for deployment. Do not let deployment mechanics take over the live demo.

### 4. Keep agent stories focused
Document what each agent does, what it returns, and how it stays grounded in app data. Avoid generic chatbot framing.

## Anti-patterns
- Treating the session as a single monolithic demo
- Mixing hosted-agent evaluation into the live pitch
- Inventing new app behavior without a repo baseline
- Rewriting the story around framework features instead of app value
