# Agentic Health Claim — BnK Solution × OutSystems ODC

**OutSystems Partner Hackathon APAC 2026 — submission repository**

A health-insurance reimbursement claim goes from **photographed documents to a payment decision**, driven by cooperating AI agents on **OutSystems ODC + Agent Workbench**. **BnK IDP** is the real document-reading engine, and a person steps in only where judgement is needed.

> Governing principle: **money is deterministic, judgement is agentic.** Every amount is produced by one auditable Server Action (`RunAdjudicationChain`, 14 steps). Agents read, interpret, recommend and explain. They never write a monetary field.

---

## What runs on the ODC tenant (`bnk-dev`)

| App | Title | Who uses it | What it does |
|---|---|---|---|
| `ClaimMVP` | **My Insurance BackOffice** | Claim officers, approvers | Work list, AI work card, IDP extraction, R1–R9 cross-document checks, 14-step adjudication chain + payout waterfall, 4-condition auto-approval gate, four-eyes approval, technical trace (`AgentRun`) |
| `MyInsurance` | **My Insurance Portal** | Customers | Submit a claim with photos, track status, supplement missing documents, read the decision letter, AI assistant, EN/VI and VND/USD display |

**Agents (Agent Workbench):** AG-0 Orchestrator · AG-1 Intake (IDP + document checklist) · AG-2 Adjudication (calls the chain as a tool, explains each step) · AG-3 Clinical (NOTREL / NOTCOV recommendations with evidence) · AG-4 Fraud-signal (hospital lookup, duplicates, impossible dates) · AG-5 Customer communication (result / decline letters). Every hand-off is written to `AgentRun`.

**Four architecture rules** (see `specs/`):
1. Money is written only by the deterministic chain; no agent has a tool that writes money.
2. Benefit values come from `PolicyBenefitSnapshot`, never from re-reading the policy.
3. IDP extraction results are immutable (`RawExtractionJson`, copied on first read).
4. Every IDP error becomes `ExtractionError` and goes to a human queue, never an automatic decline.

---

## Repository map

| Folder | Contents |
|---|---|
| `specs/` | **Stable specs** (OpenSpec): data model, IDP integration, claim rules, adjudication, agents, screens & wording |
| `changes/active/`, `changes/archive/` | Spec-driven change log: every behaviour change was written here before it was built (tasks, deltas, test runs) |
| `project.md`, `AGENT-WORKFLOW.md` | Conventions and how the AI coding agents (Claude Code + OutSystems MCP / Mentor) were run on this project |
| `docs/01-business/` | Hackathon BRDs (architecture, claim console, claim portal, context), MVP scope, business-to-tech deck, component checklist |
| `docs/02-architecture/` | Architecture diagrams (draw.io / SVG / HTML), tenant and app architecture, claim flow and user flow |
| `docs/03-build-specs/` | Full build specs `build-00…06` (scope & decisions, data model, IDP, rules, chain & seed, agents & prompts, screens) and build status |
| `docs/04-agents/` | Agent design and system prompts (AG-2, AG-3, AG-5, staff/customer assistants), AG-4 investigation |
| `docs/05-idp-integration/` | BnK IDP API notes, OpenAPI subset, ODC REST consume spec, External Logic BRD/SRD, IDP test results |
| `docs/06-ui-ux/` | Final UI concept (`prototypes/Prototype_Insurance_BnK_v1.0.html`), source prototype v0.5, design specs and fidelity criteria |
| `docs/07-demo/` | Real screenshots, demo scenarios, sample-account login, demo-video script, internal demo deck of 08/10 (HTML + PDF, Vietnamese) |
| `docs/08-testing/` | Runtime audits per revision (both apps), end-to-end test runs, approval-gate results, open bugs |
| `docs/10-worklog/` | Plans and daily summaries (how the build progressed) |
| `src/external-logic/` | C# External Logic libraries for ODC: `IdpUpload` (multipart upload to BnK IDP) and `IdpTables` (table parsing), with unit/acceptance tests and a mock IDP server |
| `src/ui-assets/` | CSS, icon font and images used in the ODC theme (AI orb, AI chat panel, tokens) |
| `demo-data/` | **Synthetic** claim documents for the demo and tests, each with an `_KY-VONG.md` (expected outcome) file |

---

## Where to start

1. [`docs/07-demo/`](docs/07-demo/) — the demo script, sample accounts and the screenshots
2. [`docs/02-architecture/`](docs/02-architecture/) — the picture
3. [`specs/agents/spec.en.md`](specs/agents/spec.en.md) and [`specs/adjudication/spec.en.md`](specs/adjudication/spec.en.md) — the agentic design and the money rules
4. [`docs/08-testing/`](docs/08-testing/) — evidence that it runs

## English reading guide

Working documents were written in Vietnamese. The ones a reviewer is most likely to open have an English version next to them, named `*.en.md` / `*.en.html` / `*.en.svg`. The Vietnamese original stays the source of truth.

| Area | English versions |
|---|---|
| Project conventions | [project.en.md](project.en.md) · [AGENT-WORKFLOW.en.md](AGENT-WORKFLOW.en.md) (how Claude Code + OutSystems MCP / Mentor were run) |
| Specs | [adjudication](specs/adjudication/spec.en.md) · [agents](specs/agents/spec.en.md) · [claim rules](specs/claim-rules/spec.en.md) · [data model](specs/data-model/spec.en.md) · [IDP integration](specs/idp-integration/spec.en.md) · [screens & wording](specs/screens-wording/spec.en.md) |
| Business | [MVP definition & scope](docs/01-business/01-dinh-nghia-MVP-va-pham-vi.en.md) · [system overview deck](docs/01-business/deck-tong-quan-he-thong-en-vi-v1.en.md) · [business-to-tech slide](docs/01-business/slide-business-to-tech-v1.en.md) — the four BRDs in this folder are already in English |
| Architecture | [Agentic claim architecture v2 (HTML)](docs/02-architecture/Kien-truc-Agentic-Claim-v2.en.html) · [OutSystems-style diagram (SVG)](docs/02-architecture/Kien-truc-OutSystems-style.en.svg) · [ClaimMVP app](docs/02-architecture/ClaimMVP.en.md) |
| Agents | [agent line-up](docs/04-agents/agents-forhackaton-06-10.en.md) · [AG-2 Adjudication](docs/04-agents/ag2-adjudication-agent-2026-10-01.en.md) · [AG-3 Clinical prompt](docs/04-agents/ag3-clinical-system-prompt.en.md) · [AG-5 Communication prompt](docs/04-agents/ag5-comms-system-prompt.en.md) · [AG-5 prompt v1.0](docs/04-agents/T12_AG5_SystemPrompt_v1.0.en.md) · [AG-4 duplicate-claim investigation](docs/04-agents/dieu-tra-ag4-bo-sot-ho-so-trung-0461.en.md) · [assistant chat panel](docs/04-agents/ai-chat-bnk-khung-tro-ly-15-10.en.md) · [AI orb button](docs/04-agents/ai-orb-bnk-nut-tro-ly-15-10.en.md) · [console prompts VI/EN](docs/04-agents/prompt-console-vi-en.en.md) |
| Demo | [demo README](docs/07-demo/README-demo-08-10.en.md) · [demo scenario cards](docs/07-demo/u16-the-kich-ban-demo.en.md) · [one-click sample login](docs/07-demo/dang-nhap-mau-1-cham-17-10.en.md) · [portal pitch-ready notes](docs/07-demo/myinsurance-pitch-ready-02-10.en.md) · [demo video script](docs/07-demo/video-demo-hackathon-19-10.en.md) |
| Testing | [E2E run, 10 claim files](docs/08-testing/test-e2e-10-ho-so-16-10.en.md) · [final E2E before recording](docs/08-testing/e2e-vong-cuoi-truoc-quay-demo-18-10.en.md) · [open bugs](docs/08-testing/open-bugs-19-10.en.md) · [auto-approval gate results](docs/08-testing/cong-tu-duyet-ket-qua-rev200-203-2026-10-01.en.md) · [gate conditions read from tenant](docs/08-testing/cong-tu-duyet-4-dieu-kien-doc-tu-tenant-2026-10-01.en.md) · [medical-facility name matching](docs/08-testing/khop-ten-co-so-y-te-rev933-18-10.en.md) · [UI scoring](docs/08-testing/cham-diem-ui-claimmvp-myinsurance-03-10.en.md) · [portal audit rev54](docs/08-testing/myinsurance-audit-rev54-2026-10-01.en.md) — the per-revision runtime audits are already in English |

## Notes

- Code lives in the ODC tenant (visual apps), so this repo holds the **specs, decisions, evidence and the custom C# code**, not an exported OAP.
- All demo documents are generated/synthetic. No real customer data is included; tokens and personal e-mails are replaced with `<redacted…>`.
- Build specs (`docs/03-build-specs/`), IDP notes, UI working notes, `changes/` and `docs/10-worklog/` remain in Vietnamese; see the reading guide above for the English versions.
- Demo claim documents in `demo-data/` are intentionally Vietnamese: they are Vietnamese medical documents that BnK IDP reads.

---
© 2026 BnK Solution
