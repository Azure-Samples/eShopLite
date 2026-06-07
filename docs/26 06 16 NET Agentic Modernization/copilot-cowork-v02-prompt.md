# Copilot Cowork v02 - Upload list and prompt

## Upload these files to Copilot Cowork

### Required

1. `docs/26 06 16 NET Agentic Modernization/slides/NET Day June 16.pptx`
2. `docs/26 06 16 NET Agentic Modernization/slides/App Modernization - Now Make It Smarter v01.pptx`
3. `docs/26 06 16 NET Agentic Modernization/slides/Bruno Capuano - Headshopt.jpg`
4. `docs/26 06 16 NET Agentic Modernization/images/06eShopLite-SearchSemantic.gif`
5. `docs/26 06 16 NET Agentic Modernization/slide-content.md`
6. `docs/26 06 16 NET Agentic Modernization/session-flow.md`
7. `docs/26 06 16 NET Agentic Modernization/scenario-map.md`

### Recommended

8. `docs/26 06 16 NET Agentic Modernization/demo-prompts.md`
9. `docs/26 06 16 NET Agentic Modernization/demo-runbook.md`
10. `docs/26 06 16 NET Agentic Modernization/deploy-with-aspire-slide-notes.md`
11. `docs/26 06 16 NET Agentic Modernization/hosted-agents-evaluation.md`
12. `docs/26 06 16 NET Agentic Modernization/top-5-pain-points.md`

## Prompt (copy/paste)

```text
Use the uploaded template deck and assets as authoritative for style and branding.

Generate a v02 deck for:
"App Modernization Done. Now Let's Make It Smarter"

Hard constraints:
- Slide 1 must be template slide 1 from "NET Day June 16.pptx" (keep layout/style).
- Slide 2 must be speaker intro: Bruno headshot + session title.
- Slide 3 must use template slide 3 style and this pitch:
  "We already saw how to modernize an app, use Aspire, and more. Now that foundation is done, let's make our app smarter."
- Slide 5 must use the GIF "06eShopLite-SearchSemantic.gif" as the main visual (visual-first, minimal text).
- Slide 6 (Demo 1 · Smarter operations) must be redesigned as a story flow:
  Trigger -> Evidence -> AI explanation -> Next actions.
- Slide 12 title must be: "Path to production".

Design/readability constraints:
- Avoid generic two-box text-heavy layouts.
- Replace dense text blocks with narrative cards, process flows, timeline, or evidence-to-outcome visuals.
- Increase readability for room projection:
  - Titles: 40-48 pt
  - Section headings: 24-30 pt
  - Body: 20-24 pt minimum where possible
- Keep slide text concise; move detail to speaker notes.

Scope guardrails:
- Do NOT include Smart Components, eShopSupport, or eShopOnWeb.
- Do NOT make Aspire MCP / AI coding-agent workflow a main pitch.
- Do NOT include a live deployment demo.
- Hosted Agents and Deploy with Aspire remain slide-only sections.

Story/timing:
- Keep the 30-minute flow from session-flow.md.
- Keep scenario mapping explicit:
  - 13 -> 01-SemanticSearch baseline
  - 14 -> 01-SemanticSearch baseline
  - 15 -> 01-SemanticSearch baseline
  - 16 -> 06-mcp baseline
  - 17 -> 10-A2ANet baseline

Output requirements:
- Return a polished slide deck preserving template brand language.
- Include speaker notes for all major slides.
- End each major section with a transition line.
- Also provide a slide-by-slide title list so we can verify numbering quickly.
```
