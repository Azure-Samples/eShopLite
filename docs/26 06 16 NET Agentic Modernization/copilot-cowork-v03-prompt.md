# Copilot Cowork v03 - Upload list and prompt (demo reorder)

Goal of this revision: **reorder the two demo slides** so the deck opens with
*Smarter Product Discovery* (cloud AI, better answers for users) and then moves to
*Smarter Operations* (local AI with Foundry Local). Everything up to slide 5 stays the
same; only slides 6 and 7 swap and are restyled to match the new story.

## Upload these files to Copilot Cowork

### Required

1. `docs/26 06 16 NET Agentic Modernization/slides/App Modernization - Now Make It Smarter v03.pptx` (current deck = template + source of truth for slides 1-5)
2. `docs/26 06 16 NET Agentic Modernization/slides/Bruno Capuano - Headshopt.jpg` (speaker headshot)
3. `docs/26 06 16 NET Agentic Modernization/slide-content.md` (authoritative new slide copy — Slides 6 & 7 are already swapped here)
4. `docs/26 06 16 NET Agentic Modernization/session-flow.md` (new narrative order + timing)

### Recommended

5. `docs/26 06 16 NET Agentic Modernization/scenario-map.md` (scenario ↔ slide mapping with demo positions)
6. `docs/26 06 16 NET Agentic Modernization/demo-runbook.md` (Demo 1 / Demo 2 details)
7. `docs/26 06 16 NET Agentic Modernization/images/06eShopLite-SearchSemantic.gif` (search visual, optional for slide 6)

## Prompt (copy/paste)

```text
Use the uploaded deck "App Modernization - Now Make It Smarter v03.pptx" as the
authoritative template for style, branding, fonts, and layout.

Produce an updated deck. The ONLY structural change is reordering the two demo
slides so the demo arc goes cloud-AI first, then local-AI.

Hard constraints:
- Keep slides 1 through 5 EXACTLY as they are in the uploaded v03 deck
  (same content, layout, visuals, and styling). Do not restyle them.
- SWAP the demo order:
  - New Slide 6 = "Demo 1: Smarter Product Discovery" (was the operations demo).
  - New Slide 7 = "Demo 2: Smarter Operations" (was the product discovery demo).
- Keep slides 8 onward in their existing order and styling.

New Slide 6 - Demo 1: Smarter Product Discovery (cloud AI, code walkthrough):
- Title: "Smarter UX: AI over product search".
- Main message: users search with intent, not database keywords.
- Frame as a CODE WALKTHROUGH (no live app run) of
  scenarios/14-ProductDiscoveryCopilot/src/Products/Memory/MemoryContext.cs.
- Show the grounding flow as a process visual (not text boxes):
  Keyword search -> Semantic / intent-based discovery -> Grounded product explanation.
- Talking points: startup embedding of the catalog, query embedding + top-3 match,
  the Score > 0.3 honesty gate (no match = no invented products), and the grounded
  prompt that only sends matched products to the cloud chat model.
- Transition line: "That used a cloud model. Now let's bring AI local."

New Slide 7 - Demo 2: Smarter Operations (local AI with Foundry Local, live demo):
- Title: "Smarter operations: local AI over logs and traces".
- Main message: observability gives data; AI helps us understand it - running
  locally with Foundry Local (no cloud key, runs on the machine).
- Use a 4-step story flow with icons/arrows (NOT a two-text-box layout):
  1. Trigger - "Checkout errors increased after deployment"
  2. Evidence - logs + traces + failing calls across products / store / observabilityassistant
  3. AI explanation - grouped incidents + likely root cause + next checks
  4. Action - top 3 checks for the developer
- Call out the contrast: Demo 1 used a CLOUD model for users; this runs AI LOCALLY
  for operations on the same modernized app.

Design / readability constraints:
- Match the existing v03 template visual language exactly for the two new slides.
- Avoid dense two-box text layouts; use narrative cards, process flows, or
  evidence-to-outcome visuals.
- Titles 40-48 pt, section headings 24-30 pt, body 20-24 pt minimum.
- Keep on-slide text concise; move detail to speaker notes.

Scope guardrails:
- Do NOT change the messaging of slides 1-5 or 8+.
- Do NOT add Smart Components, eShopSupport, or eShopOnWeb.
- Do NOT add a live deployment demo.

Output requirements:
- Return the full deck preserving the v03 template brand language.
- Include speaker notes for the two reordered slides.
- End each demo slide with its transition line.
- Provide a slide-by-slide title list so numbering can be verified quickly.
```
