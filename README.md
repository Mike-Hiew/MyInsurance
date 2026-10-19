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
| `docs/01-business/` | Hackathon BRD, scope & judging criteria, strategy, business-to-tech deck, component checklist |
| `docs/02-architecture/` | Architecture diagrams (draw.io / SVG / HTML), tenant and app architecture, claim flow and user flow |
| `docs/03-build-specs/` | Full build specs `build-00…06` (scope & decisions, data model, IDP, rules, chain & seed, agents & prompts, screens) and build status |
| `docs/04-agents/` | Agent design and system prompts (AG-2, AG-3, AG-5, staff/customer assistants), AG-4 investigation |
| `docs/05-idp-integration/` | BnK IDP API notes, OpenAPI subset, ODC REST consume spec, External Logic BRD/SRD, IDP test results |
| `docs/06-ui-ux/` | Final UI concept (`prototypes/Prototype_Insurance_BnK_v1.0.html`), source prototype v0.5, design specs and fidelity criteria |
| `docs/07-demo/` | Pitch deck & presenter script (HTML + PDF), real screenshots, demo scenarios, demo-video script |
| `docs/08-testing/` | Runtime audits per revision (both apps), end-to-end test runs, approval-gate results, open bugs |
| `docs/09-reference-mvp1/` | Baseline business specs from the MVP1 programme: A1 Glossary, A2 Messages, A3 Permissions, A4 Document catalogue, A5 Product data sheet, D1 BRD, D2 Functional spec, D3 Integration spec |
| `docs/10-worklog/` | Plans and daily summaries (how the build progressed) |
| `src/external-logic/` | C# External Logic libraries for ODC: `IdpUpload` (multipart upload to BnK IDP) and `IdpTables` (table parsing), with unit/acceptance tests and a mock IDP server |
| `src/ui-assets/` | CSS, icon font and images used in the ODC theme (AI orb, AI chat panel, tokens) |
| `demo-data/` | **Synthetic** claim documents for the demo and tests, each with an `_KY-VONG.md` (expected outcome) file |

---

## Where to start

1. `docs/07-demo/` — the pitch and the screenshots
2. `docs/02-architecture/` — the picture
3. `specs/agents/spec.md` and `specs/adjudication/spec.md` — the agentic design and the money rules
4. `docs/08-testing/` — evidence that it runs

## Notes

- Code lives in the ODC tenant (visual apps), so this repo holds the **specs, decisions, evidence and the custom C# code**, not an exported OAP.
- All demo documents are generated/synthetic. No real customer data is included; tokens and personal e-mails are replaced with `<redacted…>`.
- Most working documents are in Vietnamese; this README and the pitch are in English.

---
© 2026 BnK Solution
