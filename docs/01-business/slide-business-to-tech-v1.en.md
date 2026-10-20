# Slide pack `"Nền tảng Bồi thường Bảo hiểm Sức khoẻ Số — Từ Business đến Kỹ thuật"` (EN: "Digital Health Insurance Claims Platform — From Business to Tech") v1.0

> English translation for reviewers. Vietnamese original: [slide-business-to-tech-v1.md](slide-business-to-tech-v1.md).

**File:** `E:\Hackaton\BnK_Agentic_Health_Claim_Business_to_Tech_v1.0.pptx` (27 slides, 16:9, Vietnamese, technical terms kept in English). Audience: BnK leadership + the project team.


## Structure
1. Cover · 2. Contents (6 sections)
- **01 Context & Problem** — today's manual process + BIC baseline; table SC-01…SC-09 + MVP1 scope + hackathon focus; why now (OutSystems Industry Solutions, IDP can already read medical documents, the core still has gaps).
- **02 Solution & Experience** — 5-step customer journey (66 seconds, 16 s/page, 4 states); 8-tab console (F44–F53) + the principle of `"việc cần làm"` (EN: "things to do"); the deterministic 14-step chain (2.000.000 → 1.880.000 ₫); four demo scenarios A/B/C/D.
- **03 Architecture & Integration** — three systems + capability ownership table; map of the 5 ODC modules (CLM_Core/Agents/IDP_Int/Console/Portal) + Entity/Server Action/Timer/Roles; BnK IDP integration (6 REST Consume, real measurements, 13/64 vs 32/64 D3, adapter rules); InsureMO I-01…I-06 Provisional + RK-14 + mock/seed strategy.
- **04 AI assistance & Risk governance** — table of 7 tasks AG-0…AG-7 (tool vs agent, knowledge, Server Action) + the "dividing line"; 6 guardrails (4-condition gate, mandatory evidence, PII, AgentRun, failure modes, IDP gap → AG-5).
- **05 Plan & Preparation** — roadmap 3–6/9 → 7–8/9 enablement → sprints 1–2 → sprints 3–4 → 7–8/10 pitch → 22/10; then MVP1 pilot Q4, Phase 2/3; 4-group checklist (environment & licences, data & samples, integration, business & organisation); team of 7 roles.
- **06 Risks, Decisions & Next steps** — 7 risks (EAP, RK-14, RK-08, PII, 28/8 flow, agent-calls-agent, B-19/20/21); 5 decisions to settle + the next two weeks; thank-you slide.

## Points to note when using it
- The reference customer is left as a **question to settle** (MBAL is not asserted); the company name is not stated.
- Extraction accuracy is marked "not yet measured" (POST /request/compare has not been run).
- Dates in the deck: enablement 07/09, build 08/09, pitch 7–8/10, results 22/10 — consistent with the KBYG.
- No company URL on the last slide (domain not yet confirmed).
