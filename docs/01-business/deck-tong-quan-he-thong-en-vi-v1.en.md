# "Agentic Health Claim Adjudication" system overview pack — EN deck + VI HTML + architecture diagrams v1.0

> English translation for reviewers. Vietnamese original: [deck-tong-quan-he-thong-en-vi-v1.md](deck-tong-quan-he-thong-en-vi-v1.md).

**Date:** 07/09/2026. **Audience:** hackathon judges + OutSystems SAs. **Scope:** the agentic slice (not the whole MVP1).

## Files
| File | Content |
|---|---|
| `BnK_Agentic_Health_Claim_Overview_EN_v1.0.pptx` | 16 slides, 16:9 (13.3×7.5"), English, speaker notes on 5 slides. Slide 6 is an **architecture diagram drawn with shapes** (replacing the old card layout) |
| `BnK_Tong_Quan_He_Thong_VI_v1.0.html` | A single file, Vietnamese, same content, responsive + print-friendly, no network dependency. Section 05 has the **architecture diagram as inline SVG** |
| `BnK_Architecture_Diagram.html` | An **interactive, bilingual VI/EN** diagram page: hover/click a block → a panel explains that block, the other blocks dim. Has a legend, light/dark theme, Esc key to deselect |
| `BnK_Agentic_Claim_Architecture.drawio` | A draw.io file with **2 pages** (`Architecture (EN)` / `Kiến trúc (VI)`), opens at app.diagrams.net or the VS Code extension, editable and exportable to PNG/SVG/PDF |

**Style:** inherited from the Business-to-Tech v1.0 deck — teal palette `0B4F53 / 0E7C7B / 9AD9C8 / EAF4F1`, Cambria titles, Calibri body, rounded cards, footer "BnK Solution | … | …" + page number. Generated with pptxgenjs (`/home/claude/deck/build.js`) and `mk_drawio.py`; validated + render QA on all 16 slides; HTML QA with Playwright at 1440/820/390 px, 0 JS errors, 0 horizontal overflow.

## Architecture diagram — the structure shared by all 4 files
The **OutSystems ODC** container is split into four layers from top to bottom, with layer labels in the left margin:
1. **Experience** — CLM_Portal (My Insurance) · CLM_Console (the claim officer's workbench)
2. **Agentic** — the dark block `CLM_Agents · Agent Workbench`: Orchestrator on top, 5 agents below (Intake, Adjudication, Clinical, Fraud-signal, Customer comms)
3. **Deterministic** — RunAdjudicationChain (14 steps) · the 4-condition auto-approval gate · Claim + LimitLedger · AgentRun
4. **Integration** — CLM_IDP_Int (6 REST Consume + adapter) · Core connectors (mock)

On the right are the **external systems**: BnK IDP (runs for real) and the insurance core (mock, not locked into any core).
The arrow between the Agentic layer and the Deterministic layer carries the label **"calls as a tool · never writes"** — this is the point to pause on during the pitch, because it answers the safety question before the judges ask it.

## Structure of the 16 slides
1. Cover · 2. Contents (6 sections)
- **01 The problem** — the manual status quo + baseline (100% hand-typed, SLA 3–5 days / 10–14 days, 990 claims in 2026, 4.3 billion); why now (ONE 2026 + Insurance Agents Kit Q3, IDP can already read, 13/64 requirements → the agent gap).
- **02 The solution** — 5-step journey (66 seconds); the **line "money is deterministic, judgement is agentic"** with the example 2.000.000 → 1.880.000 run twice.
- **03 Architecture** — slide 6 is the drawn diagram; slide 7 is the measured IDP (~16 s/page, ~30 s flow, 20 items, 13,328 hospitals) and what IDP cannot do.
- **04 The agents** — a table of 6 agents (role + tools); the 6-hop agent-to-agent slide read from AgentRun/handoffTo; a table of each agent's value with the corresponding measurements.
- **05 Governance** — 6 guardrails (money is code, the 4-condition gate, mandatory evidence, confirmation with a reason, AgentRun, fail-safe/PII).
- **06 Evidence & plan** — 3 scenarios A/B/C + offline replay; a 5-row ROI table with a "how it is measured" column; roadmap 7/9 → 22/10 → pilot Q4, the 3-person team commitment, 3 asks of OutSystems.
16. Thank you.

## Points to note when using it
- **Extraction accuracy is still marked "not yet measured"** — the deck says so plainly and promises a real figure from `POST /request/compare` before the pitch day. After running compare, update slide 14 + section 12 of the HTML.
- **Customer name left blank**: "top-five bancassurance life insurer already trialling BnK IDP" — MBAL/BIC not named yet, awaiting the customer's consent.
- The deck uses the names of the 6 agents (Orchestrator, Intake, Adjudication, Clinical, Fraud-signal, Customer-communication), matching `chien-luoc-hackathon-agentic-claim.md`, and does not use the AG-0…AG-7 numbering of the Business-to-Tech version.
- Dates in the deck: enablement 7/9, build 8/9, sprint 9–29/9, pitch 7–8/10, results 22/10, pilot Q4 — consistent with the KBYG.
- **If Agent Workbench does not let an agent call an agent**: the agent-to-agent slide and the Orchestrator block in all 4 files must be changed to Server Action/BPT, explained through the audit trail. The Orchestrator description in `BnK_Architecture_Diagram.html` already states this fallback.
