# Business Requirements Document (BRD)
## Agentic Health Claim Adjudication — Hackathon Slice on OutSystems ODC + Agent Workbench

| Item | Value |
|---|---|
| Document | BRD — Agentic Health Claim Adjudication (hackathon slice of Claim MVP1) |
| Version | 1.0 — 08 Sep 2026 |
| Owner | BnK Solution — Claim Platform team |
| Audience | BnK project team (PO, SA, developers), OutSystems Solution Architects, hackathon judges |
| Status | Draft for review |
| Basis | MVP1 Deliverables (D1 BRD, D2 FS, D3 Integration Spec, Prototype v1.6 / Console v6.1), `IDP_APIs_for_Claim_MVP_v1.0`, hackathon strategy note (03/09/2026), KBYG briefing (24/08/2026) |

---

## 0. Document control

### 0.1 Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 1.0 | 08/09/2026 | BnK Claim Platform team | First complete BRD for the hackathon slice, function-level detail |

### 0.2 How to read this document

- **BR-nn** — business requirements (why / what the business needs).
- **FR-\<module\>-nn** — functional requirements, one per function, grouped by ODC module (`CORE`, `AGT`, `IDP`, `CON`, `POR`).
- **DR-nn** — data requirements. **IR-nn** — integration requirements. **NFR-nn** — non-functional requirements. **GR-nn** — governance / guardrail requirements.
- **Priority:** `M` Must (needed for the 3 demo scenarios and the pitch), `S` Should (needed for a credible pilot, built if time allows before 29/09), `C` Could (roadmap, documented only).
- MVP1 screen codes (F42–F67, S07–S14) and interface codes (I-07…I-16) are kept so every function traces back to the MVP1 Functional Specification.
- Vietnamese business terms are kept where they are the domain vocabulary (NOTREL, NOTCOV, "thời gian chờ", "đồng chi trả"); UI copy shown to customers is Vietnamese, pitch and technical artefacts are English.

---

## 1. Executive summary

BnK Solution is entering the OutSystems Partner Hackathon APAC 2026 with **"Agentic Health Claim Adjudication"**: a health-insurance reimbursement claim goes from photographed documents to a payment decision, driven by a group of cooperating AI agents on OutSystems ODC Agent Workbench, with BnK IDP as the real document-reading tool and humans stepping in only where judgement is required.

This BRD does **not** cover the whole Claim MVP1. It covers the **agentic slice** that must run live on the ODC tenant by the pitch (7–8 Oct 2026): one policy product family (HC-STD / PLS / PRM, seeded), four medical document types the IDP already has templates for, three end-to-end demo scenarios (A, B, C), six agents, a deterministic 14-step adjudication chain, a 4-condition auto-approval gate, a claim-officer console and a minimal customer portal.

The governing design principle, repeated throughout: **money is deterministic, judgement is agentic.** Every amount is produced by a Server Action (`RunAdjudicationChain`) that can be re-run and audited; agents read, interpret, recommend and explain, but never write a monetary field.

---

## 2. Business context

### 2.1 Current state (reference customer baseline)

The reference baseline comes from a Vietnamese bancassurance life insurer's health-claim operation (name withheld pending customer consent; referred to as "the reference insurer").

| Metric | Baseline today | Source |
|---|---|---|
| Data entry from claim documents | 100 % keyed by hand | Reference insurer, 2026 |
| Processing SLA (simple / complex) | 3 / 5 working days | Reference insurer |
| Provisioning / reserve SLA (simple / complex) | 10 / 14 days | Reference insurer |
| Claim volume | 990 claims in 2026 (YTD basis used for ROI) | Reference insurer |
| Amount paid | ≈ 4.3 billion VND | Reference insurer |
| Straight-through processing (STP) rate | 0 % — every claim is touched by an adjudicator | Observed |
| Extraction accuracy | Not yet measured (to be measured with `POST /request/compare` before pitch) | IDP staging |

Pain points, as stated by the business:

1. Adjudicators spend most of their time transcribing hospital documents (discharge papers, itemised bills, prescriptions, lab results) rather than deciding.
2. Coverage judgement (is this item a covered benefit? is it related to the diagnosis?) is inconsistent across adjudicators and poorly evidenced in the file.
3. Customers get little visibility: they learn the outcome days later and the explanation of what they must pay themselves is opaque.
4. Duplicate submissions, mismatched hospitals and impossible dates are caught late or not at all.

### 2.2 Why now

- OutSystems launched Agentic Industry Solutions at ONE 2026 (2–3 Jun 2026); the Insurance Agents Kit lands Q3 2026 and Connectors Kit / FNOL in H2 2026. Being first with a claims agentic solution on Forge is a partner-level opportunity.
- BnK IDP already reads Vietnamese medical documents on staging (measured 03/09/2026: ~16 s per page extraction, ~30 s assessment flow, 20 checks, 13,328-hospital master list).
- The IDP requirement fit assessment shows the IDP meets only **13 of 64** Claim Platform requirements outright; the biggest gaps (line-level clinical assessment, reason codes, evidence, confidence bands) are exactly what a Clinical agent on Agent Workbench can fill.
- Hackathon judging (KBYG 24/08/2026) explicitly rewards: real ODC + Agent Workbench build, many agent-to-agent interactions that the team can explain, an external customer use case, a clear ROI story and committed resources.

### 2.3 Business objectives

| ID | Objective | Measure |
|---|---|---|
| BO-1 | Remove manual data entry from health claims | % of claim lines created without keying (target ≥ 90 % on demo document set) |
| BO-2 | Auto-approve clean, low-value claims without adjudicator touch | STP rate on scenario-A-type claims (target: 100 % in demo, ≥ 30 % pilot ambition) |
| BO-3 | Make coverage judgement consistent and evidenced | Every NOTREL / NOTCOV decision carries a reason code + evidence + officer confirmation |
| BO-4 | Cut cycle time for the customer | Submission → decision in minutes for clean claims (demo journey 66 s) vs. 3–5 days |
| BO-5 | Catch fraud/duplicate signals before payment | Every claim passes a 4-condition gate; blocked claims never reach payment |
| BO-6 | Demonstrate an auditable agentic architecture judges can inspect | Complete `AgentRun` trail with hand-offs for each demo claim |

### 2.4 Success criteria for the hackathon submission

| ID | Criterion | Target / evidence |
|---|---|---|
| SC-H1 | Runs live on the BnK ODC tenant using Agent Workbench | SA of OutSystems has seen scenario B run on tenant (08/09), all three scenarios by 22/09 |
| SC-H2 | Agent-to-agent interaction is real and explainable | `AgentRun` log with `HandoffTo` for ≥ 6 hand-offs per claim; architecture diagram + narrative |
| SC-H3 | IDP is called for real (not mocked) with a replay fallback | 6 REST methods consumed; `ReplayMode` site setting proven offline |
| SC-H4 | Deterministic money | Same claim run twice gives identical amounts; before/after re-run on scenario B shows 2,000,000 → 1,880,000 VND |
| SC-H5 | Measured, not illustrative, numbers | 16 s/page, ~30 s flow, first extraction-accuracy figure from `POST /request/compare` on 10–20 anonymised documents |
| SC-H6 | ROI story | One-page ROI from the baseline in §2.1, each line showing "how measured" |
| SC-H7 | External customer + committed resources | Reference insurer named (or described) in pitch; 3-person team committed after 22/10; pilot Q4 2026 |
| SC-H8 | Pitch in English, 7–8 Oct | Deck + live demo + backup video |

---

## 3. Scope

### 3.1 In scope

| Area | In scope for the hackathon slice |
|---|---|
| Products | Health-care riders HC-STD, HC-PLS, HC-PRM — benefit trees seeded into Entities from the P-02 snapshot |
| Claim journey | J2 reimbursement claim only: submit → documents read → assess → adjudicate → approve → pay/decline → explain |
| Documents | 4 medical document types with existing IDP templates: discharge paper (5384 / 5127), itemised cost statement (5383), prescription (5299), lab result (5340). Hospital receipt (5312) optional |
| Treatment types | Outpatient and inpatient; illness and injury (`MEDICAL_ILLNESS`, `MEDICAL_INJURY`) |
| Agents | Orchestrator, Intake, Adjudication, Clinical, Fraud-signal, Customer-communication |
| Deterministic core | 14-step adjudication chain, 4-condition auto-approval gate, limit ledger, claim state machine |
| Integration | BnK IDP (real, staging tenant), insurance core (mocked / seeded) |
| UIs | Claim Console (officer, approver, presenter mode), My Insurance portal (customer, web) |
| Scenarios | A — clean claim < 2,000,000 VND auto-approved; B — NOTCOV supplement + over per-visit cap, Clinical agent proposes, officer confirms, chain re-runs; C — future event date / hospital mismatch, Fraud-signal blocks the gate |
| Languages | Customer-facing Vietnamese; console default English with VI toggle; agent explanations Vietnamese with English summary |

### 3.2 Out of scope (deliberately)

| Excluded | Reason |
|---|---|
| J1 policy purchase journey (13 steps) | Not agentic, not judged |
| Native mobile app (CLM_MobileApp) | Portal is responsive web; mobile is MVP1 pilot scope |
| Remaining 4 of 8 MVP1 document types | No IDP template; would require training |
| InsureMO / Bicins / DataPlatform live connection (I-01…I-06) | Core is mocked from seed data → "not locked to any core" |
| Re-issuing MVP1 BA documents (D1/D2/D3) | This BRD is additive; MVP1 documents remain the pilot baseline |
| Closing all 7 MVP1 open items | Only B-19 / B-20 / B-21 (they change payout amounts) get an interim decision, recorded in §11 |
| Line-level clinical assessment inside IDP, `bbox` field anchoring, `from-url` submission, webhook | IDP gaps — requested from the IDP team for post-hackathon (§9.4) |
| Injury disability-rate lookup (former AG-6) | Not applicable to HC products; roadmap |

### 3.3 Assumptions

| ID | Assumption | Impact if false |
|---|---|---|
| AS-1 | Agent Experience EAP is enabled on the BnK tenant and Agent Workbench is available | Cannot build the judged layer — escalate to Partner Manager immediately |
| AS-2 | Agent Workbench allows an agent to call a Server Action as a tool | Agents become advisory only; chain is called from BPT |
| AS-3 | Agent Workbench allows an agent to invoke another agent (confirmed/denied at Enablement 07/09) | Orchestrator becomes a Server Action / BPT; hand-offs are narrated through `AgentRun` (fallback already designed, §7.1) |
| AS-4 | IDP staging stays up and flow "Workflow Claim 28/8" (`5ed8e4fc-…`) is not changed during the sprint | Replay mode with recorded JSON |
| AS-5 | A dedicated Tenant-role IDP account is provisioned for the ODC integration | Personal accounts are used — not acceptable for demo, blocker |
| AS-6 | Demo document set is fully anonymised before upload to any tenant | PII exposure — hard stop |
| AS-7 | Reference insurer agrees to be named by 29/09 | Pitch uses "top-five bancassurance life insurer already trialling BnK IDP" |

### 3.4 Constraints

- ODC only; Agent Workbench mandatory (hard hackathon requirement). Claude Code + OutSystems MCP used for build.
- IDP: user-login only (no service account / OAuth2 client credentials, no refresh token), rate limit ~100 req/60 s, custom `User-Agent` required, errors returned inside HTTP 200, `raw-file` returns ZIP, no webhook (polling only).
- Team of one developer until 08/09; three people from 22/10.
- All demo data synthetic or anonymised; no real money, no real policyholders.

---

## 4. Stakeholders and actors

### 4.1 Stakeholders

| Stakeholder | Interest |
|---|---|
| BnK management | Win/placement at hackathon; sellable agentic claim solution; Forge / Industry Solutions listing |
| Reference insurer (claims operations) | Faster cycle time, less keying, consistent coverage decisions, audit trail |
| OutSystems (SA, Partner Manager, Industry Solutions) | Real ODC + Agent Workbench build; explainable multi-agent design; enterprise governance |
| BnK IDP team | Clear list of API gaps to close after hackathon |
| Compliance / DPO | PII handling, no autonomous money decisions without deterministic control |

### 4.2 Human actors (roles)

| Role | ODC role | Responsibilities in the slice |
|---|---|---|
| Customer / Policyholder | `Customer` | Submits claim and documents in My Insurance, tracks status, reads result and explanation, adds documents |
| Claim Officer (adjudicator) | `ClaimOfficer` | Works the queue, reviews extraction and lines, confirms/rejects agent recommendations with a reason, submits for approval |
| Claim Approver | `ClaimApprover` | Approves / declines claims above the auto-approval gate or flagged claims; approves decline decisions |
| Operations / Integration operator | `ClaimOps` | Handles `EXTRACTION_ERROR` queue, IDP outages, replay mode, retries |
| Presenter (project team / judges) | `Presenter` (flag on `ClaimOfficer`) | Sees the technical hand-over screens: agent map, `AgentRun` trace, KPI/ROI, hackathon entry, OutSystems map |

### 4.3 System actors

| Actor | Type | Description |
|---|---|---|
| Orchestrator agent (AG-0) | Agent Workbench agent (or Server Action/BPT fallback) | Coordinates the five specialist agents, waits for Track 1 and Track 2 to settle, calls the gate, writes the audit trail |
| Intake agent (AG-1) | Agent | Receives documents, calls IDP, builds the document checklist, requests missing documents |
| Adjudication agent (AG-2) | Agent | Calls `RunAdjudicationChain` as a tool and explains each step in Vietnamese; never computes money itself |
| Clinical agent (AG-3) | Agent | Turns IDP flow warnings + claim lines + knowledge base into NOTREL / NOTCOV recommendations with evidence and confidence band |
| Fraud-signal agent (AG-4) | Agent | Hospital lookup, duplicate detection, impossible dates → gate condition 4 |
| Customer-communication agent (AG-5) | Agent | Drafts Vietnamese result / decline letters from templates using approved reason codes |
| BnK IDP | External system (real) | Document extraction (`/request`), assessment flow (`/workflow`), master data (`/metadata`) |
| Insurance core (mock) | Seeded Entities | Policy, benefit snapshot, member, limit balances |

---

## 5. Business process (to-be)

### 5.1 End-to-end claim journey (5 steps, customer view — 66 s demo)

```
S09 Create claim  →  S10 Photograph documents  →  "Processing documents"  →  Received  →  Result / Explanation
   (customer)            (customer)                  (Intake + IDP, ~16 s/page)     (agents + chain)   (S13, letter)
```

### 5.2 Internal orchestration (agent view)

```
Claim SUBMITTED
   │
   ▼
[AG-0 Orchestrator] ──► [AG-1 Intake] ──► IDP /request/sync (Track 1: extraction)
   │                        │
   │                        └─► document checklist ─► missing? ─► PENDING_DOCS (customer notified)
   │
   ├──► [AG-3 Clinical] ──► IDP /workflow (flow 28/8, Track 2: 20 checks) ──► NOTREL/NOTCOV proposals + evidence
   │
   ├──► [AG-4 Fraud-signal] ──► hospital lookup, duplicate check, date sanity ──► signal list
   │
   ▼  (wait until Track 1 + Track 2 + Fraud settle)
[AG-2 Adjudication] ──► RunAdjudicationChain (14 deterministic steps) ──► amounts + ledger preview
   │
   ▼
EvaluateAutoApprovalGate (4 conditions)
   ├─ all pass ──► APPROVED ──► PaymentOrder ──► PAID ──► [AG-5 Comms] result letter
   └─ any fail ──► PENDING_REVIEW (officer)  ──► confirm/reject proposals (reason mandatory)
                       └─► chain re-run ──► PENDING_APPROVAL ──► approver ──► APPROVED / DECLINED ──► [AG-5] letter
```

Every arrow between agents, and every tool call, is one `AgentRun` record with `HandoffTo`.

### 5.3 Claim state machine (slice)

| State | Entered by | Exits to | Visible to customer as |
|---|---|---|---|
| `DRAFT` | Customer starts S09 | `SUBMITTED` | Draft |
| `SUBMITTED` | Customer confirms upload | `EXTRACTING` | "Received" |
| `EXTRACTING` | Orchestrator → Intake | `ASSESSING`, `PENDING_DOCS`, `EXTRACTION_ERROR` | "Processing documents" |
| `PENDING_DOCS` | Intake checklist incomplete | `EXTRACTING` (after S12 upload) | "Additional documents needed" |
| `EXTRACTION_ERROR` | IDP error / timeout | `EXTRACTING` (retry by Ops) | "Processing documents" |
| `ASSESSING` | Track 2 + Fraud running | `ADJUDICATED` | "Under review" |
| `ADJUDICATED` | Chain completed, gate evaluated | `APPROVED` (gate pass), `PENDING_REVIEW` (gate fail), `BLOCKED` (fraud) | "Under review" |
| `PENDING_REVIEW` | Gate failed on condition 2/3 | `PENDING_APPROVAL` | "Under review" |
| `BLOCKED` | Gate condition 4 failed | `PENDING_REVIEW` (officer clears), `DECLINED` | "Under review" |
| `PENDING_APPROVAL` | Officer submits | `APPROVED`, `DECLINED`, `PENDING_REVIEW` (returned) | "Under review" |
| `APPROVED` | Gate or approver | `PAID` | "Approved" |
| `DECLINED` | Approver | `CLOSED` | "Declined" |
| `PAID` | Payment order issued (mock) | `CLOSED` | "Paid" |
| `CLOSED` | System after letter sent | — | "Closed" |

The customer sees only four statuses: Received · Processing / Under review · Additional documents needed · Result (Approved / Paid / Declined).

### 5.4 The line: deterministic vs. agentic

| Deterministic (Server Actions in `CLM_Core`) | Agentic (Agent Workbench in `CLM_Agents`) |
|---|---|
| Benefit mapping, waiting period, per-visit caps, co-payment, remaining limits, rounding, ledger posting | Reading and interpreting documents, judging medical relevance and coverage, spotting fraud signals, explaining, drafting letters |
| Always re-runnable, identical output for identical input | Always produces a proposal + evidence, never a final monetary value |
| Writes `ClaimLine.PayableAmount`, `LimitLedger` | Writes `ClinicalRecommendation`, `FraudSignal`, `AgentRun`, letter drafts |
| Approves only through the 4-condition gate | Cannot approve, decline or pay |

---

## 6. Demo scenarios (acceptance backbone)

| # | Scenario | Claim | Path | Expected outcome | Proves |
|---|---|---|---|---|---|
| A | Clean outpatient claim, low value | Outpatient consultation + prescription, 1,450,000 VND, all documents present, hospital in list, dates valid | Intake → Clinical (no warnings) → Fraud (none) → Adjudication → gate passes 4/4 → auto-approved → paid → letter | `APPROVED` and `PAID` with no human touch; customer sees result in ≈ 66 s | STP, deterministic money, agent-to-agent |
| B | Supplement + over per-visit cap | Outpatient, requested 2,000,000 VND; line 4 is a dietary supplement (NOTCOV), line 2 exceeds per-visit cap | Clinical proposes NOTCOV on line 4 with evidence; gate fails condition 3 → `PENDING_REVIEW`; officer confirms; chain re-runs: 2,000,000 → 1,880,000 payable; officer submits; approver approves; letter explains "why you pay this part yourself" per cause | Before/after amounts shown side by side; reason mandatory on reject | Human-in-the-loop, re-runnable chain, evidence |
| C | Fraud signal | Inpatient, event date in the future (20/03/2025 style error), hospital name not matching master list, 3 cost lines below confidence threshold | Flow 28/8 raises `warning_level 1`; Fraud-signal agent flags date + hospital; gate fails condition 4 → `BLOCKED`; officer records manual check on 3 lines, clears or declines | Claim never reaches payment automatically | Guardrail, fraud signal, gate condition 4 |
| D (optional) | Waiting-period decline | Illness inside waiting period | Chain step "waiting period" excludes all lines; officer submits decline; approver approves decline; decline letter | `DECLINED` with policy-clause reference | Deterministic exclusion, decline letter |

Replay: each scenario has recorded IDP responses so the whole flow runs with `ReplayMode = On` and no network.

---

## 7. Solution overview

### 7.1 Architecture (ODC application, 5 modules)

| Layer | Module | Type | Contents |
|---|---|---|---|
| Experience | `CLM_Console` | Reactive Web app | Officer / approver console (worklist, claim detail 8 tabs, approvals), presenter mode |
| Experience | `CLM_Portal` | Reactive Web app (responsive) | My Insurance customer portal (S07–S14 subset) |
| Agentic | `CLM_Agents` | Agent Workbench | 6 agents, tool bindings to Server Actions, system prompts, knowledge base (benefit clauses, NOTREL/NOTCOV rules) |
| Deterministic | `CLM_Core` | Library / service | Entities, `RunAdjudicationChain`, `EvaluateAutoApprovalGate`, `LimitLedger`, state machine, `AgentRun` writer, seed data |
| Integration | `CLM_IDP_Int` | Library / service | 6 REST Consume methods to BnK IDP, adapter (`IdpParseExtraction`, `IdpFieldMap`), Timer `IdpAssessmentPoller`, `ReplayMode` |
| Integration | `CLM_Core_Mock` (part of `CLM_Core`) | Seed data + connectors | Policy / member / limit data standing in for InsureMO / Bicins / DataPlatform |

Arrow between Agentic and Deterministic layers is labelled **"calls as a tool · never writes"**.

**Orchestrator fallback (if Agent Workbench does not support agent→agent calls):** `AG-0` is implemented as a Server Action / BPT process `OrchestrateClaim` that invokes the five agents sequentially; hand-offs are still written to `AgentRun` with `HandoffTo`, so the narrative and the audit trail are unchanged.

### 7.2 Agent roster

| Code | Agent | Inputs | Tools (Server Actions / REST) | Outputs | Writes |
|---|---|---|---|---|---|
| AG-0 | Orchestrator | Claim id, state | `RunAgent` (or direct agent call), `GetClaimContext`, `EvaluateAutoApprovalGate`, `RouteToQueue`, `WriteAgentRun`, `TransitionClaimState` | Next step decision, gate result, officer "to-do" list (F56 panel) | `AgentRun`, `Claim.State` (via Server Action only) |
| AG-1 | Intake | Uploaded files, claim type, document set | `IdpSubmitSync`, `IdpGetRawFile`, `GetDocumentChecklist`, `RequestAdditionalDocuments`, `WriteAgentRun` | Document checklist status, per-document extraction, missing-document request, `EXTRACTION_ERROR` | `ClaimDocument`, `ClaimLine` (raw, from adapter), `AgentRun` |
| AG-2 | Adjudication | Normalised lines, policy snapshot, confirmed recommendations | `RunAdjudicationChain`, `GetChainExplanation`, `WriteAgentRun` | Vietnamese step-by-step explanation, before/after summary | `AgentRun` only (chain writes amounts) |
| AG-3 | Clinical | 20 flow-28/8 items, claim lines, diagnosis (ICD), knowledge base | `IdpStartAssessment`, `IdpGetAssessment`, `SearchKnowledgeBase`, `ProposeRecommendation`, `WriteAgentRun` | `ClinicalRecommendation` per line: reason code NOTREL / NOTCOV / OK, evidence[], confidence band | `ClinicalAssessment`, `ClinicalRecommendation`, `AgentRun` |
| AG-4 | Fraud-signal | Extracted facility name, event/admission/discharge dates, member id, claim history | `IdpSearchHospital`, `FindDuplicateClaims`, `ValidateDates`, `RaiseFraudSignal`, `WriteAgentRun` | `FraudSignal` list with severity; gate condition 4 input | `FraudSignal`, `AgentRun` |
| AG-5 | Customer-communication | Final claim, lines with reason codes, letter templates | `GetLetterTemplate`, `DraftLetter`, `WriteAgentRun` | Vietnamese result / decline / additional-documents letter (draft, officer-approved) | `ClaimLetter`, `AgentRun` |

### 7.3 Hand-off sequence (what the judges are shown)

| # | From → To | Trigger | `AgentRun.HandoffTo` |
|---|---|---|---|
| 1 | AG-0 → AG-1 | Claim `SUBMITTED` | `Intake` |
| 2 | AG-1 → AG-0 | Extraction done / docs missing / error | `Orchestrator` |
| 3 | AG-0 → AG-3 and AG-0 → AG-4 (parallel) | Track 1 complete | `Clinical`, `FraudSignal` |
| 4 | AG-3 → AG-0, AG-4 → AG-0 | Recommendations / signals ready | `Orchestrator` |
| 5 | AG-0 → AG-2 | Track 1 + Track 2 + Fraud settled (or Track 2 timed out) | `Adjudication` |
| 6 | AG-2 → AG-0 | Chain result + explanation | `Orchestrator` |
| 7 | AG-0 → gate → AG-5 | `APPROVED` / `DECLINED` | `CustomerComms` |

---

## 8. Business requirements

| ID | Requirement | Priority | Trace |
|---|---|---|---|
| BR-01 | The system shall accept a health reimbursement claim from a customer with photographed/PDF documents and return a decision without human touch when the claim is clean and under the auto-approval threshold. | M | BO-2, Scenario A |
| BR-02 | All claim monetary values (requested, covered, payable, customer-borne, limit consumption) shall be computed only by a deterministic, re-runnable rule chain. | M | BO-6, GR-01 |
| BR-03 | AI agents shall be allowed to read, interpret, recommend and explain, but shall not create, approve, decline or pay a claim, and shall not write monetary fields. | M | GR-01 |
| BR-04 | Every coverage recommendation (NOTREL / NOTCOV) shall carry a reason code, at least one piece of evidence and a confidence band, and shall require officer confirmation or rejection with a mandatory reason before it affects money. | M | BO-3, Scenario B |
| BR-05 | A claim shall be auto-approved only when all four gate conditions pass; failing any condition routes the claim to a human. | M | BO-2, BO-5 |
| BR-06 | Any fraud/duplicate/impossible-date signal shall block automatic approval and payment. | M | BO-5, Scenario C |
| BR-07 | Every agent step, tool call and hand-off shall be recorded in an immutable audit trail viewable by officers and presenters. | M | BO-6, SC-H2 |
| BR-08 | The customer shall see claim status in four plain-language states and, on completion, an itemised result explaining "why you pay this part yourself" per cause, in Vietnamese. | M | BO-4 |
| BR-09 | Document extraction shall use BnK IDP as a real integration; IDP failures shall never produce a decline — they route to an operator queue. | M | SC-H3, IR-07 |
| BR-10 | The demo shall run end to end offline using recorded IDP responses. | M | SC-H3 |
| BR-11 | Personal data in demo documents shall be anonymised before upload to any tenant, slide or video. | M | AS-6 |
| BR-12 | Extraction accuracy shall be measured (not asserted) on an anonymised ground-truth set before the pitch. | S | SC-H5 |
| BR-13 | The solution shall not depend on a specific insurance core; core data shall be reachable through a connector interface that is mocked in the hackathon. | S | §3.2 |
| BR-14 | Limit consumption shall be posted to a ledger only on approval and reversed on decline/cancel, so remaining limits shown to the customer are always consistent with paid claims. | M | BO-4 |
| BR-15 | Console default language English with Vietnamese toggle; customer portal Vietnamese; document content, item names, facility and person names stay Vietnamese. | S | §3.1 |

---

## 9. Functional requirements

### 9.1 Module `CLM_Core` — deterministic core

#### 9.1.1 Claim lifecycle

| ID | Function | Description | Inputs | Outputs / rules | Pri |
|---|---|---|---|---|---|
| FR-CORE-01 | `CreateClaim` | Creates a claim in `DRAFT` for a member/policy with treatment metadata. | PolicyId, MemberId, LossType (Illness/Injury), TreatmentType (Outpatient/Inpatient), EventDate, TreatmentFrom/To, FacilityName, RequestedAmount (optional), PayoutAccount | `Claim` with `ClaimNo` format `CLM-YYYY-nnnnnn`; validates member belongs to policy and policy active on EventDate | M |
| FR-CORE-02 | `TransitionClaimState` | The only way to change `Claim.State`; enforces the state machine in §5.3. | ClaimId, TargetState, Actor (agent/user), Reason | Rejects illegal transitions; writes `ClaimEvent` (timeline) for every transition | M |
| FR-CORE-03 | `GetClaimContext` | Returns the full claim context for agents and screens in one structure. | ClaimId | Claim, lines, documents, policy snapshot, remaining limits, recommendations, signals, latest chain result | M |
| FR-CORE-04 | `RouteToQueue` | Assigns the claim to a work queue. | ClaimId, Queue (`Adjudication`, `Approval`, `Operator`, `Blocked`) | Sets `Claim.Queue`, `AssignedTo` null; used by Orchestrator and gate | M |
| FR-CORE-05 | `AssignClaim` | Officer takes/assigns a claim. | ClaimId, UserId | One active assignee; logged in `ClaimEvent` | S |
| FR-CORE-06 | `CancelClaim` | Customer or officer cancels before approval. | ClaimId, Reason | Reverses any ledger reservation; state `CLOSED` with `Outcome = Cancelled` | C |

#### 9.1.2 Documents and lines

| ID | Function | Description | Inputs | Outputs / rules | Pri |
|---|---|---|---|---|---|
| FR-CORE-07 | `GetDocumentChecklist` | Builds the required / expected / optional document list for a claim. | ClaimType, TreatmentType, LossType | Rows: DocumentType (T1-nn), Requirement level (Required / Expected / Optional), IDP `templateId` list, Status (Missing / Received / Read / Failed). Rules: inpatient requires discharge paper + cost statement; outpatient requires prescription or cost statement; lab result expected when diagnosis needs it | M |
| FR-CORE-08 | `StoreClaimDocument` | Persists an uploaded document and its immutable IDP raw result. | ClaimId, File, sha256, IdpRequestId, RawExtractionJson | `ClaimDocument` row; `RawExtractionJson` written once, never updated (I-09) | M |
| FR-CORE-09 | `UpsertClaimLinesFromExtraction` | Converts normalised extraction into cost lines. | ClaimDocumentId, normalised fields/table rows | `ClaimLine` rows with: LineNo, ServiceDate, ItemName (Vietnamese as printed), ServiceGroup, ICD10 (if any), Quantity, RequestedAmount, PaidBySocialInsurance, Confidence (0–1), Source = `IDP`, EditedValue null. Original values retained when officer edits | M |
| FR-CORE-10 | `EditClaimLine` | Officer corrects an extracted value. | ClaimLineId, Field, NewValue, Reason | Stores `EditedValue` next to `OriginalValue`; `Source = Officer`; triggers chain re-run flag | M |
| FR-CORE-11 | `MarkLineVerified` | Officer records manual verification against the original image (scenario C, 3 low-confidence lines). | ClaimLineId, VerifiedBy | `Verified = true`; required before submission when Confidence < threshold | M |
| FR-CORE-12 | `CrossCheckDocuments` | Deterministic cross-document checks R1–R9 (MVP1). | ClaimId | Checks: patient name/DOB consistent across documents; admission ≤ discharge; service dates within treatment window; cost statement total = Σ lines; prescription date within window; facility name consistent; ICD present for inpatient; document dates ≤ today; duplicate document hash. Output `CrossCheckResult` list (Pass / Warn / Fail) | M |

#### 9.1.3 Adjudication chain (14 deterministic steps)

| ID | Function | Description | Pri |
|---|---|---|---|
| FR-CORE-13 | `RunAdjudicationChain` | Executes steps 1–14 below in order for a claim, in one transaction, writes `ChainRun` (version, inputs hash, per-step output) and updates `ClaimLine` monetary fields. Idempotent: same inputs → same `ChainRun` output hash. Callable by AG-2 as a tool and by console "Re-run". | M |
| FR-CORE-14 | `GetChainExplanation` | Returns per-step human-readable output (VI + EN) and before/after diff between two `ChainRun`s. | M |
| FR-CORE-15 | `EvaluateAutoApprovalGate` | Evaluates the four gate conditions and returns pass/fail per condition with reasons. | M |

**Chain steps** (each step is a separate Server Action so it can be unit-tested and explained; step names and codes trace to MVP1 FS F48):

| Step | Code | Rule | Data used | Output |
|---|---|---|---|---|
| 1 | `CH-01 PolicyEligibility` | Policy active and premium paid on EventDate; member covered under the rider | `Policy`, `PolicyBenefitSnapshot`, `Member` | Eligible / Not eligible (claim-level) |
| 2 | `CH-02 WaitingPeriod` | EventDate − rider effective date ≥ waiting period by LossType (illness 30 days, specified illness 90 days, injury 0) per snapshot | `PolicyBenefitSnapshot.WaitingPeriods` | Per line: Excluded (`WAIT`) or continue |
| 3 | `CH-03 EventClassification` | Map LossType + TreatmentType + ICD → benefit category (Outpatient / Inpatient / Surgery / Maternity…) | `BenefitCategoryMap` | Benefit category per claim |
| 4 | `CH-04 LineBenefitMapping` | Map each `ClaimLine.ServiceGroup` to a benefit item in the snapshot | `BenefitItemMap` | Benefit item per line or `UNMAPPED` (→ officer) |
| 5 | `CH-05 CoverageScopeExclusions` | Apply policy exclusions and **confirmed** recommendations: `NOTCOV` (not a covered benefit — e.g. supplements, cosmetics), `NOTREL` (not related to diagnosis) | `ClinicalRecommendation` with `Status = Confirmed`, `ExclusionList` | Covered / Excluded with reason code per line |
| 6 | `CH-06 SocialInsuranceDeduction` | Requested − amount paid by social insurance (BHYT) where document shows it | `ClaimLine.PaidBySocialInsurance` | Net requested per line |
| 7 | `CH-07 PerVisitCap` | Cap per visit/day per benefit item (e.g. outpatient 500,000 VND/visit) | Snapshot caps | Capped amount, `CAP_VISIT` excess |
| 8 | `CH-08 PerYearLimitCheck` | Remaining annual limit per benefit item from ledger | `LimitLedger` | Amount within remaining, `LIMIT_YEAR` excess |
| 9 | `CH-09 CoPayment` | Apply co-payment % (đồng chi trả) per product (e.g. PLS 20 %) | Snapshot | Insurer share, customer share `COPAY` |
| 10 | `CH-10 Deductible` | Apply annual deductible if the product has one | Snapshot, ledger | `DEDUCT` amount |
| 11 | `CH-11 ClaimLevelCap` | Total payable ≤ overall annual limit of the rider | Snapshot, ledger | `LIMIT_TOTAL` excess |
| 12 | `CH-12 Rounding` | Round payable to 1,000 VND per product rule (B-20 interim: round down) | Config | Rounded payable |
| 13 | `CH-13 CustomerBorneBreakdown` | For each line: customer-borne amount split by cause (`WAIT`, `NOTCOV`, `NOTREL`, `CAP_VISIT`, `LIMIT_YEAR`, `COPAY`, `DEDUCT`, `LIMIT_TOTAL`) — feeds the customer explanation | Steps 2–11 | `ClaimLineBreakdown` rows |
| 14 | `CH-14 LedgerPreview` | Compute limit consumption per benefit item (not yet posted) and claim totals: Requested, Covered, Payable, CustomerBorne | Steps 1–13 | `ChainRun` totals; `LimitLedger` preview rows |

Scenario B worked example (must reproduce): requested 2,000,000 → line 4 supplement 80,000 `NOTCOV` (confirmed) + line 2 excess over per-visit cap 40,000 `CAP_VISIT` → payable **1,880,000 VND**.

**Gate conditions** (`EvaluateAutoApprovalGate`):

| Cond. | Name | Rule | Parameter |
|---|---|---|---|
| G1 | Amount | `ChainRun.Payable` ≤ auto-approval threshold and claim is outpatient/illness-injury | `AutoApproveThreshold` = 2,000,000 VND (Site Setting) |
| G2 | Documents & extraction quality | All Required documents `Read`; claim confidence (min `c` on mandatory fields with values) ≥ threshold; completeness = 100 %; `CrossCheckDocuments` has no `Fail`; no line `UNMAPPED`; no unverified line under confidence floor | `MinClaimConfidence` = 0.90, `LineConfidenceFloor` = 0.80 |
| G3 | Clinical clear | Track 2 finished (`ClinicalAssessment.State = COMPLETE`); no `ClinicalRecommendation` with `Status = Proposed`; no flow item with `warning_level = 1` unresolved | — |
| G4 | No fraud / duplicate signal | No open `FraudSignal` with severity ≥ Medium (future dates, hospital not found, duplicate claim/document, treatment window overlap) | — |

Gate output: `GateResult` (4 booleans + reasons), stored on `ChainRun`; pass → `TransitionClaimState(APPROVED)`; fail on G4 → `BLOCKED`; fail on G1/G2/G3 → `PENDING_REVIEW`.

#### 9.1.4 Approval, payment, ledger

| ID | Function | Description | Inputs | Rules | Pri |
|---|---|---|---|---|---|
| FR-CORE-16 | `SubmitForApproval` | Officer submits claim; runs pre-submission checklist (7 points). | ClaimId, OfficerId | Checklist: all Required docs read; all proposals resolved; all low-confidence lines verified; cross-checks no Fail; chain run current (inputs hash matches); fraud signals cleared/annotated; letter draft exists. All must pass → `PENDING_APPROVAL` | M |
| FR-CORE-17 | `ApproveClaim` | Approver approves. | ClaimId, ApproverId, Comment | Approver ≠ officer (4-eyes); posts ledger; state `APPROVED` | M |
| FR-CORE-18 | `DeclineClaim` | Approver declines (or approves a decline proposal). | ClaimId, ApproverId, ReasonCode, Comment | Reason code mandatory from list (`WAIT`, `NOTCOV`, `NOTREL`, `FRAUD`, `NOT_ELIGIBLE`, `DOCS`); releases ledger reservation; state `DECLINED` | M |
| FR-CORE-19 | `ReturnToOfficer` | Approver returns with comment. | ClaimId, Comment | State `PENDING_REVIEW` | S |
| FR-CORE-20 | `PostLimitLedger` | Posts consumption rows on approval. | ChainRunId | One `LimitLedger` row per benefit item: Before, Consumed, After; reversible by `ReverseLimitLedger` | M |
| FR-CORE-21 | `GetRemainingLimits` | Remaining limits per benefit item for a member/policy year. | PolicyId, MemberId, PolicyYear | Snapshot limit − Σ posted ledger; used by portal S07/S08 and chain step 8 | M |
| FR-CORE-22 | `IssuePaymentOrder` | Creates a payment order (mock core) and moves claim to `PAID`. | ClaimId | `PaymentOrder` with amount = Payable, account = PayoutAccount (masked), status `Issued`; timeline event F58 | M |
| FR-CORE-23 | `CloseClaim` | Closes after letter sent. | ClaimId | State `CLOSED` | S |

#### 9.1.5 Audit and agent run log

| ID | Function | Description | Rules | Pri |
|---|---|---|---|---|
| FR-CORE-24 | `WriteAgentRun` | Appends one immutable audit row per agent turn / tool call. | Fields: ClaimId, Agent, Step, ToolCalled, InputSummary, OutputSummary, HandoffTo, KnowledgeVersion (e.g. workflowId, KB version), ModelId, DurationMs, Timestamp, CorrelationId. No update/delete | M |
| FR-CORE-25 | `GetAgentTrace` | Returns the ordered trace for a claim for the console (F53 log and presenter trace). | Filter by agent, type (System / Extraction / AutoCheck / Rule / Officer) | M |
| FR-CORE-26 | `ClaimEvent` timeline | Every state change, officer action and letter is a timeline event visible to officer (F53) and, filtered, to customer (S11). | Customer-visible flag per event type | M |

#### 9.1.6 Configuration and seed

| ID | Function | Description | Pri |
|---|---|---|---|
| FR-CORE-27 | Seed data | Seeds products HC-STD / HC-PLS / HC-PRM (benefit trees, caps, co-pay, waiting periods per P-02 snapshot), 1 policy per product, members, 4 sample claims (A, B, C, D) with recorded IDP JSON. Re-runnable (`ResetDemoData`). | M |
| FR-CORE-28 | Site Settings | `AutoApproveThreshold`, `MinClaimConfidence`, `LineConfidenceFloor`, `ReplayMode`, `IdpBaseUrl`, `IdpWorkflowId`, `IdpPollMaxMinutes`, `RoundingRule`. | M |
| FR-CORE-29 | Exclusion & mapping tables | `ExclusionList` (NOTCOV item patterns), `BenefitItemMap` (ServiceGroup → benefit), `BenefitCategoryMap`, reason-code dictionary with Vietnamese customer wording rules (never "kê sai", always "không thuộc phạm vi…"). | M |

### 9.2 Module `CLM_Agents` — Agent Workbench

Common requirements for all agents:

| ID | Requirement | Pri |
|---|---|---|
| FR-AGT-00a | Each agent has a closed tool list (only the Server Actions listed in §7.2); no agent has a tool that writes `ClaimLine` money fields, `LimitLedger`, `PaymentOrder` or `Claim.State` directly. | M |
| FR-AGT-00b | System prompt of each agent contains: entity schema it may read, intent → tool mapping rules, "ask, don't invent" rule when information is missing, and the convention to start any answer that changed data with `Action taken: …`. | M |
| FR-AGT-00c | Every agent turn calls `WriteAgentRun` with `HandoffTo`; failure to log is a failure of the turn. | M |
| FR-AGT-00d | Agents run under a service identity with `ClaimAgent` role; PII fields (national ID, health-insurance card number, account number) are masked in agent context. | M |
| FR-AGT-00e | Agent output to customers is Vietnamese; output to console has Vietnamese + one-line English summary. | S |
| FR-AGT-00f | Time-boxing: any agent turn > 120 s is aborted and the claim is routed to the operator queue with `AGENT_TIMEOUT`. | M |

#### AG-0 Orchestrator

| ID | Function | Description | Pri |
|---|---|---|---|
| FR-AGT-01 | `StartClaimOrchestration` | Triggered on `SUBMITTED` (event or BPT). Loads context, hands off to Intake. | M |
| FR-AGT-02 | `WaitForTracks` | Waits until Track 1 (extraction) is complete and Track 2 (clinical) + Fraud are `COMPLETE`, `PARTIAL` or timed out (`IdpPollMaxMinutes`, default 15). On Track 2 timeout: proceed with Track 1 only and raise console warning `CLM-CONSOLE-015`. | M |
| FR-AGT-03 | `DispatchAdjudication` | Hands off to AG-2 with the settled context; receives chain result. | M |
| FR-AGT-04 | `ApplyGate` | Calls `EvaluateAutoApprovalGate`; routes: pass → `APPROVED` + `IssuePaymentOrder` + AG-5; G4 fail → `BLOCKED`; else `PENDING_REVIEW`. | M |
| FR-AGT-05 | `BuildOfficerTodo` | Produces the "Việc cần làm" panel content (F56): concrete to-dos by state (e.g. "Confirm or reject recommendation on line 4", "Verify 3 lines against original image") + "System has checked" list. | M |
| FR-AGT-06 | `ResumeAfterOfficerAction` | On confirm/reject/edit → re-dispatch AG-2 (chain re-run) and re-evaluate gate; on submit → route to approval; on approve/decline → AG-5. | M |
| FR-AGT-07 | Fallback implementation | Same behaviour as a Server Action / BPT `OrchestrateClaim` if agent→agent calls are unavailable (AS-3). | M |

#### AG-1 Intake

| ID | Function | Description | Pri |
|---|---|---|---|
| FR-AGT-08 | `ReceiveDocuments` | For each uploaded file: compute sha256, reject duplicates already read for the same claim, call `IdpSubmitSync` with the full template list of the document set and `name = ClaimNo`. | M |
| FR-AGT-09 | `ClassifyAndChecklist` | Read `templates_data[].templateId` → document type; update `GetDocumentChecklist` status; identify Required documents still Missing. | M |
| FR-AGT-10 | `NormaliseAndCreateLines` | Call adapter output → `UpsertClaimLinesFromExtraction`; report unknown field labels (`k` not in `IdpFieldMap`) as a warning, never silently drop. | M |
| FR-AGT-11 | `RequestAdditionalDocuments` | When Required documents missing or a document unreadable: draft a Vietnamese request listing exactly what is needed and why; move claim to `PENDING_DOCS`; notify customer (S12/S14). | M |
| FR-AGT-12 | `HandleExtractionError` | On `errorCode != null`, timeout, 4xx/5xx or empty result: claim → `EXTRACTION_ERROR`, operator queue, never `DECLINED`. | M |
| FR-AGT-13 | `ImageQualityHint` | If confidence of a document < threshold, tell the customer which document to re-photograph (blur/cut-off) before the officer sees it. | S |

#### AG-2 Adjudication

| ID | Function | Description | Pri |
|---|---|---|---|
| FR-AGT-14 | `RunChainAsTool` | Calls `RunAdjudicationChain`; does not compute, estimate or round any amount itself; if the tool fails, reports the failure. | M |
| FR-AGT-15 | `ExplainChain` | Produces a Vietnamese explanation per step (14 lines) from `GetChainExplanation`, plus an English summary; cites step codes. | M |
| FR-AGT-16 | `ExplainBeforeAfter` | After a re-run, explains which confirmed recommendation / edit changed which step and by how much (2,000,000 → 1,880,000: "line 4 excluded NOTCOV −80,000; line 2 over per-visit cap −40,000"). | M |
| FR-AGT-17 | `FlagUnmapped` | Lists lines the chain returned as `UNMAPPED` and asks the officer to map them; never guesses a benefit. | M |

#### AG-3 Clinical

| ID | Function | Description | Pri |
|---|---|---|---|
| FR-AGT-18 | `StartAssessment` | Calls `IdpStartAssessment` (PUT `/workflow/request` + GET `run-workflow-request`, retry on first 400) with pinned `IdpWorkflowId`; records `ClinicalAssessment` with `KnowledgeBaseVersion = workflowId`. | M |
| FR-AGT-19 | `CollectAssessment` | Reads the 20 flow items via `IdpGetAssessment` when the poller marks Finished; normalises `"None"/"null"/"00000"` → null; `{}` or empty data after Finished = `ERROR`, not clean. | M |
| FR-AGT-20 | `AssessLines` | For each `ClaimLine`: decide `OK` / `NOTREL` (not related to the diagnosis / ICD) / `NOTCOV` (not a covered benefit: supplements, cosmetics, non-medical items) using flow warnings, ICD, item name and knowledge base; produce `evidence[]` (source: `IDP_WARNING`, `KB_CLAUSE`, `ICD_MISMATCH`, `ITEM_PATTERN`) and `confidence band` (High / Medium / Low). | M |
| FR-AGT-21 | `ProposeRecommendation` | Writes `ClinicalRecommendation` with `Status = Proposed`; one per line; never changes money. | M |
| FR-AGT-22 | `SummariseWarnings` | Turns `warning_level` 1 / 0.5 items (future date, name/DOB mismatch, diagnosis mismatch, hospital not found) into console-readable findings and forwards date/hospital ones to AG-4. | M |
| FR-AGT-23 | `PartialMode` | If Track 2 times out, mark `ClinicalAssessment.State = PARTIAL` and produce recommendations from KB + lines only, flagged as partial. | S |
| FR-AGT-24 | `KnowledgeBase` | Knowledge base content: benefit definitions per product, exclusion clauses, NOTREL/NOTCOV rule wording, common supplement/cosmetic item patterns; versioned (`KnowledgeVersion` in `AgentRun`). No medical sign-off claimed (RK-08 stated as limitation). | M |

#### AG-4 Fraud-signal

| ID | Function | Description | Pri |
|---|---|---|---|
| FR-AGT-25 | `LookupHospital` | `IdpSearchHospital` (OpenSearch table 322, `hospital_name`, size 5); match score threshold; returns `hospital_code`, `country`. Not found or country ≠ VN without overseas benefit → signal `HOSPITAL_UNMATCHED` (Medium). | M |
| FR-AGT-26 | `ValidateDates` | Event date not in the future; admission ≤ discharge; treatment window ≤ 60 days for outpatient; document dates ≥ event date. Violation → `DATE_IMPOSSIBLE` (High). | M |
| FR-AGT-27 | `FindDuplicateClaims` | Same member + overlapping treatment window, or same document hash, or same cost-statement number across claims → `DUPLICATE` (High). | M |
| FR-AGT-28 | `RaiseFraudSignal` | Writes `FraudSignal` rows (type, severity, evidence, status Open); explains in one Vietnamese sentence per signal for the officer. | M |
| FR-AGT-29 | `ClearSignal` (officer tool, not agent) | Officer marks a signal `Cleared` with a reason; gate G4 re-evaluates. | M |

#### AG-5 Customer-communication

| ID | Function | Description | Pri |
|---|---|---|---|
| FR-AGT-30 | `DraftResultLetter` | On `APPROVED`/`PAID`: Vietnamese letter with claim number, total requested / payable, itemised table, "why you pay this part yourself" grouped by cause using the reason-code dictionary; never uses "kê sai", "gian lận"; NOTREL wording per rule. | M |
| FR-AGT-31 | `DraftDeclineLetter` | On `DECLINED`: reason code → policy clause reference, appeal instructions, contact. | M |
| FR-AGT-32 | `DraftAdditionalDocsRequest` | On `PENDING_DOCS`: exact document list, how to photograph, deadline. | M |
| FR-AGT-33 | `LetterApproval` | Letters are drafts until an officer/approver clicks "Send"; edits tracked; sent copy stored as `ClaimLetter` and shown in portal S13. | M |
| FR-AGT-34 | `AnswerClaimQuestion` (portal chat) | Answers customer questions about *their own* claim status, amounts and reasons from `GetClaimContext` only; refuses medical advice and other people's claims; escalates to officer on request. | S |

### 9.3 Module `CLM_IDP_Int` — BnK IDP integration

| ID | Function | Description | Rules | Pri |
|---|---|---|---|---|
| FR-IDP-01 | `IdpGetToken` | Login and token cache. | `POST /auth/login` (Tenant-role account) → `IdpSession` (1 row: AccessToken, ObtainedAt, TenantId, UserId); re-login when older than 30 min or on 401; `GET /user/me` once to confirm `roleId = 2` | M |
| FR-IDP-02 | `IdpSubmitSync` | Submit one file and get extraction synchronously. | `POST /request/sync` multipart `name = ClaimNo`, `templates` = comma-separated string of the document set, `document`; ~16 s/page; response parsed by `IdpParseExtraction` (handles double-nested `result.result`) | M |
| FR-IDP-03 | `IdpSubmitGroup` (multi-file) | Group submission for > 1 file or > 1 page. | `POST /request/group` → `POST /request` ×n → poll `GET /request/group/{id}/check-status` → `GET /request/{id}/result`; batches of ≤ 10 files under one `requestGroupId` | S |
| FR-IDP-04 | `IdpGetRawFile` | Original image for side-by-side view (F46). | `GET /request/{id}/raw-file` returns ZIP → unzip → store as binary; replaces deep link I-10 | M |
| FR-IDP-05 | `IdpStartAssessment` | Start flow 28/8. | `PUT /workflow/request` {ocrRequestIds, workflowId = Site Setting, name = `ASSESS-<ClaimNo>`, autoStart ignored} → `GET /workflow/run-workflow-request/{id}`; retry up to 3× on 400 with 2 s wait | M |
| FR-IDP-06 | `IdpAssessmentPoller` (Timer) | Poll assessment status. | `GET /workflow/request/{id}`; backoff 3 → 5 → 10 → 30 s; ceiling `IdpPollMaxMinutes` (15); status 3 Finished → fetch result; 4 Error → `ClinicalAssessment.ERROR`; ceiling → `PARTIAL` + `CLM-CONSOLE-015` | M |
| FR-IDP-07 | `IdpGetAssessment` | Fetch 20-item result. | `GET /workflow/request/{id}/result/json`; on 400 `4007 Workflow result is bad` retry; map to `IdpAssessmentItem` (InstructionName, InstructionType, WarningLevel, FieldName, FieldValidation, Result) | M |
| FR-IDP-08 | `IdpSearchHospital` | Hospital master lookup. | `GET /metadata/opensearch/table/322/search?searchTerm=&fields=hospital_name&size=5` | M |
| FR-IDP-09 | `IdpParseExtraction` (adapter) | Normalise extraction payload. | Rules: `fields[].k` label → `FieldCode` via `IdpFieldMap` (templateId + k, versioned; unknown k → warning); `""` → null; confidence scales unified to 0–1; claim confidence = min `c` over mandatory fields with values + completeness ratio; dates `dd/MM/yyyy` and `yyyy-MM-dd`; amounts with `.`/`,` → decimal; `subDocumentType` null tolerated; tables per template `tableFields` (5383 = 13 columns) → line rows | M |
| FR-IDP-10 | Error contract | Every IDP error becomes a typed outcome. | `errorCode != null` in HTTP 200, timeout, 4xx/5xx, `result: {}` → `IdpOutcome.Error(code, message)`; caller maps to `EXTRACTION_ERROR` / `ClinicalAssessment.ERROR`; never to a decline | M |
| FR-IDP-11 | Headers & limits | Mandatory request shape. | `Authorization: Bearer`, `Accept: application/json, text/plain, */*`, `User-Agent` = browser string (Cloudflare 403 1010 otherwise); ≤ 100 req/60 s; circuit breaker 5 errors/60 s → replay/operator | M |
| FR-IDP-12 | Immutability | Never mutate IDP results. | `PUT /request/{id}/result` is not consumed; first read copied to `RawExtractionJson` | M |
| FR-IDP-13 | `ReplayMode` | Offline demo. | Site Setting `ReplayMode = On` → every REST wrapper returns the recorded JSON for the document hash / claim from `IdpReplay` entity; `AgentRun` marks `Replay = true` | M |
| FR-IDP-14 | `IdpCompareAccuracy` | Accuracy measurement. | `POST /request/compare` with anonymised XLSX ground truth for 10–20 documents; stores field-level precision/recall in `AccuracyRun` for the KPI screen | S |
| FR-IDP-15 | `IdpUsageMonitor` | Quota watch. | `GET /usage/current` daily; warn when `pageUsed` > 80 % of `totalBlockRegistered` (quota exhaustion returns HTTP 200 with error) | C |

### 9.4 Module `CLM_Console` — Claim Console (officer / approver / presenter)

#### Navigation and worklist

| ID | Screen / function | Description | Pri |
|---|---|---|---|
| FR-CON-01 | Login & roles | ODC users with `ClaimOfficer`, `ClaimApprover`, `ClaimOps`; presenter mode via login link or `Shift+D`. | M |
| FR-CON-02 | F42 Worklist | Columns: ClaimNo, insured, treatment type, state + flags (recommendation pending, fraud signal, low confidence), requested/payable, age (days), assignee. Filters by state/queue/assignee; sort; click → detail. 8 seeded claims incl. A/B/C/D. | M |
| FR-CON-03 | F54 Dashboard | Tiles: in adjudication, pending approval, pending payment, blocked; claim age distribution; extraction quality (avg confidence, % lines verified); STP rate today. | S |
| FR-CON-04 | Operator queue | `EXTRACTION_ERROR`, `AGENT_TIMEOUT`, Track 2 `PARTIAL`; actions Retry extraction, Switch to replay, Reassign. | M |
| FR-CON-05 | Language toggle | Default EN, toggle VI; document content, item names, facility and person names remain Vietnamese. | S |

#### Claim detail — 8 tabs

| ID | Tab | Functions | Pri |
|---|---|---|---|
| FR-CON-06 | F45 General | Claim header (ClaimNo, state, queue, assignee), insured & policy card, treatment summary, payout account (masked), timeline mini-view. | M |
| FR-CON-07 | F44 Documents | Checklist in 3 requirement levels with status; document set status; cross-check results R1–R9; actions **Request additional documents** (opens AG-1 draft) and **Waive** (reason mandatory, approver visibility). | M |
| FR-CON-08 | F46 Extraction & cost lines | Left: original image (from `IdpGetRawFile`), zoom, page switch. Right: line grid — ServiceDate, ServiceGroup, ICD, Qty, Requested, Confidence, Decision. Inline edit keeps original value visible (FR-CORE-10). **Recommendation card** under the grid for lines with `Proposed` recommendation: reason code, evidence list, band, buttons **Confirm** / **Reject (reason mandatory)**. Scenario C card "3 lines to verify on original image" with **Mark verified**. Low-confidence rows highlighted. | M |
| FR-CON-09 | F47 Event & scope | Event table (event date, admission, discharge, facility, matched hospital code/country), loss type, illness classification, waiting-period computation with dates, conclusion; applicable clauses; fraud signals list with **Clear signal (reason)**. | M |
| FR-CON-10 | F48 Calculation & limit ledger | 14-step chain table (step, rule, input, output, VI/EN explanation), **Re-run chain** button, before/after diff view between two runs, ledger preview per benefit item (Before / Consumed / After). | M |
| FR-CON-11 | F49 Settlement proposal | Per-benefit grid: sum insured / remaining / cap / co-pay / requested / in scope / payable / customer-borne; totals; payment section (F58); **7-point pre-submission checklist** (FR-CORE-16) with live status; **Submit for approval**. | M |
| FR-CON-12 | F50/F51 Approval & payment | Gate panel (4 conditions with pass/fail and reason), approval timeline, approver actions **Approve / Decline (reason code) / Return**, payment order status, letter preview & **Send**. | M |
| FR-CON-13 | F53 History & log | Single stream of `ClaimEvent` + `AgentRun` filtered by type: System / Document reading / Auto check / Rule / Officer; expandable input/output summaries. | M |
| FR-CON-14 | F56 Right panel "To do" | Content from AG-0 `BuildOfficerTodo`: to-dos by state + "System has checked" list; collapses to a bar on F46/F49 so grids have room; never overlays content (padding rule). | M |

#### Other console screens

| ID | Screen | Description | Pri |
|---|---|---|---|
| FR-CON-15 | F60 Policy & benefits | Snapshot vs current benefit tree, member list, limits used/remaining. | S |
| FR-CON-16 | F65 Clause book | Searchable product clauses (source of AG-3 KB), exclusion list. | C |
| FR-CON-17 | F67 Verification queue | Claims with lines below confidence floor, pending manual verification. | S |
| FR-CON-18 | F48 Limit ledger | Ledger browser per policy/member/benefit item. | S |
| FR-CON-19 | F55/F57 Administration | Site settings editor (thresholds, replay), `IdpFieldMap` editor, `ExclusionList` editor, `ResetDemoData`. | S |

#### Presenter mode (technical hand-over, hidden from officers)

| ID | Screen | Description | Pri |
|---|---|---|---|
| FR-CON-20 | Agent architecture map | 4-layer diagram, 6 agents, tool lists, the "calls as a tool · never writes" boundary; bilingual. | M |
| FR-CON-21 | Agent trace | `AgentRun` timeline per claim with `HandoffTo`, durations, replay flag; updates live when the officer confirms a recommendation. | M |
| FR-CON-22 | Results & ROI | Measured tiles (16.1 s/page, ~30 s flow, 13,328 hospitals, 93 endpoints, extraction accuracy from `AccuracyRun` or "not measured"), baseline vs. target table, editable ROI calculator with every input labelled *assumption*. | M |
| FR-CON-23 | Hackathon entry | KBYG criteria vs status, 8-minute demo script (B → A → C → architecture), hard Q&A. | S |
| FR-CON-24 | OutSystems map | Modules, entities, server actions, timers, REST methods, roles; build order. | S |

### 9.5 Module `CLM_Portal` — My Insurance (customer, responsive web)

| ID | Screen | Functions | Pri |
|---|---|---|---|
| FR-POR-01 | Login | Customer login (seeded accounts); Vietnamese UI. | M |
| FR-POR-02 | S07 My insurance | Policy card(s), covered members, remaining limits per benefit (from `GetRemainingLimits`), recent claims. | M |
| FR-POR-03 | S08 Benefit details | Benefit table with "applies from" (= waiting period end) per benefit. | S |
| FR-POR-04 | S09 Create claim | Wizard: treated person → loss type → event date → treatment from/to → facility (type-ahead from hospital lookup) → requested amount (optional) → payout account (masked list). Validations: event date ≤ today, treatment window valid, member covered. Creates `DRAFT`. | M |
| FR-POR-05 | S10 Photograph / upload documents | Checklist (Required / Expected / Optional) from FR-CORE-07; camera or file; client-side blur/brightness check; per-document status; **Submit** → `SUBMITTED`. | M |
| FR-POR-06 | Processing screen | "Đang xử lý chứng từ" progress (no mention of AI/agents); auto-refreshes; transitions to Received / Result. | M |
| FR-POR-07 | S11 My claims & progress | List with 4 customer statuses; timeline of customer-visible events. | M |
| FR-POR-08 | S13 Result | Total requested / payable / customer-borne; itemised lines; **"Vì sao bạn tự chịu"** grouped by cause (`WAIT`, `NOTCOV`, `NOTREL`, `CAP_VISIT`, `LIMIT_YEAR`, `COPAY`, …) with rule-compliant wording; letter download; remaining limits after this claim. Same numbers as console. | M |
| FR-POR-09 | S12 Add documents | When `PENDING_DOCS`: list of what is missing (from AG-1), upload, re-submit. | M |
| FR-POR-10 | S14 Notifications | In-app notifications on each customer-visible state change and letter. | S |
| FR-POR-11 | Ask about my claim (chat) | AG-5 `AnswerClaimQuestion` with suggested questions; scoped to own claims. | S |

---

## 10. Data requirements

### 10.1 Core entities (`CLM_Core`)

| Entity | Key attributes | Notes |
|---|---|---|
| `Policy` | PolicyNo, ProductCode (HC-STD/PLS/PRM), HolderId, EffectiveDate, Status, PolicyYearStart | Seeded (mock core) |
| `PolicyBenefitSnapshot` | PolicyId, BenefitItemCode, Category, AnnualLimit, PerVisitCap, CoPayPct, Deductible, WaitingDaysIllness, WaitingDaysSpecified, WaitingDaysInjury, SnapshotDate | Snapshot at claim time; "snapshot vs current" in F60 |
| `Member` | PolicyId, FullName, DOB, Gender, Relationship, MaskedNationalId, MaskedHealthCardNo | PII masked at rest for demo |
| `Claim` | ClaimNo, PolicyId, MemberId, LossType, TreatmentType, EventDate, TreatmentFrom, TreatmentTo, FacilityName, MatchedHospitalCode, RequestedAmount, State, Queue, AssignedTo, PayoutAccountMasked, CreatedBy, CreatedAt | State per §5.3 |
| `ClaimDocument` | ClaimId, DocumentType (T1-nn), TemplateId, FileHash, IdpRequestId, RawExtractionJson (immutable), Confidence, Status, RawImageBinary | |
| `ClaimLine` | ClaimId, DocumentId, LineNo, ServiceDate, ItemName, ServiceGroup, ICD10, Qty, RequestedAmount, PaidBySocialInsurance, Confidence, Verified, OriginalValueJson, EditedValueJson, Source, BenefitItemCode, CoveredAmount, PayableAmount, CustomerBorneAmount, ExclusionReasonCode | Money fields written only by chain |
| `ClaimLineBreakdown` | ClaimLineId, CauseCode, Amount | Feeds S13 "why you pay" |
| `ChainRun` | ClaimId, RunNo, InputsHash, Version, StepOutputsJson, TotalRequested, TotalCovered, TotalPayable, TotalCustomerBorne, GateResultJson, RunAt, TriggeredBy | Deterministic; compare runs |
| `LimitLedger` | PolicyId, MemberId, BenefitItemCode, PolicyYear, ClaimId, ChainRunId, Before, Consumed, After, PostedAt, Reversed | |
| `ClinicalAssessment` | ClaimId, WorkflowRequestId, WorkflowId (KnowledgeBaseVersion), State (COMPLETE/PARTIAL/ERROR), ItemsJson (20), StartedAt, FinishedAt | |
| `ClinicalRecommendation` | ClaimLineId, ReasonCode (OK/NOTREL/NOTCOV), EvidenceJson[], Band, Status (Proposed/Confirmed/Rejected), DecidedBy, DecisionReason, DecidedAt | |
| `FraudSignal` | ClaimId, Type, Severity, EvidenceJson, Status (Open/Cleared), ClearedBy, ClearReason | |
| `CrossCheckResult` | ClaimId, RuleCode (R1–R9), Result, Detail | |
| `ClaimEvent` | ClaimId, Type, Actor, Detail, CustomerVisible, At | Timeline |
| `AgentRun` | ClaimId, Agent, Step, ToolCalled, InputSummary, OutputSummary, HandoffTo, KnowledgeVersion, ModelId, DurationMs, Replay, CorrelationId, At | Append-only |
| `ClaimLetter` | ClaimId, Type, DraftText, SentText, DraftedBy (agent), SentBy, SentAt | |
| `PaymentOrder` | ClaimId, Amount, AccountMasked, Status, IssuedAt | Mock |
| `ExclusionList`, `BenefitItemMap`, `BenefitCategoryMap`, `ReasonCodeDictionary` | Config | Editable in F55/F57 |

### 10.2 Integration entities (`CLM_IDP_Int`)

`IdpSession`, `IdpFieldMap` (TemplateId, K, FieldCode, Version), `IdpReplay` (Key, Endpoint, ResponseJson), `AccuracyRun`.

### 10.3 Data rules

| ID | Rule |
|---|---|
| DR-01 | `RawExtractionJson` and `AgentRun` are append-only; no update/delete actions exist. |
| DR-02 | Original extracted values are never overwritten by officer edits; both are stored and shown. |
| DR-03 | Confidence is stored 0–1 everywhere; IDP 0–100 values converted in the adapter. |
| DR-04 | Empty IDP fields (`""`, `"None"`, `"null"`, `"00000"`) are stored as null and excluded from confidence minimum. |
| DR-05 | Money is Decimal(18,0) VND; rounding only in chain step 12. |
| DR-06 | PII (national ID, health-insurance card, bank account) stored masked in demo data; full values are never present in the demo tenant. |
| DR-07 | `ClaimNo` is the correlation id across ODC, IDP (`name`), letters and logs. |
| DR-08 | Every `ChainRun` stores `InputsHash` so the console can tell whether the current amounts are stale after an edit/confirmation. |

---

## 11. Integration requirements

| ID | Requirement | Pri |
|---|---|---|
| IR-01 | BnK IDP is consumed through exactly the six REST methods in §9.3 plus hospital search and compare; base URL, workflow id and account are Site Settings per stage. | M |
| IR-02 | Tenant-role integration account dedicated to the demo tenant; no personal accounts; credentials in ODC secrets, not in source. | M |
| IR-03 | Polling replaces webhooks/socket.io; Timer cadence per FR-IDP-06. | M |
| IR-04 | Insurance core connectors (`GetPolicy`, `GetBenefitSnapshot`, `GetMember`, `GetLimitBalance`, `PostPayment`) are defined as Server Action interfaces with a mock implementation reading seed entities; swapping to InsureMO / Bicins / DataPlatform is a later implementation of the same interface (I-01…I-06 remain Provisional). | S |
| IR-05 | Recorded IDP responses for each demo claim are stored in `IdpReplay` and refreshed whenever the flow changes. | M |
| IR-06 | IDP gaps formally requested for post-hackathon: stable field keys instead of labels (D1), `documentId` + `bbox` in `/result` (D6/I2), line-level assessment input `lines[]` (H3/H10/H11), `proposedReasonCode` + `evidence[]` + band (H7–H9), `icd10`/`documentDate` per line (E4), fixed `from-url` (B1), `extraParams` (B5), crosswalk `CL*` ↔ `T1-nn` (C3), real `MAX_FILE_SIZE` / throttle / token TTL. | C |
| IR-07 | Interim decisions for MVP1 open items affecting money: B-19, B-20 (rounding: round down to 1,000 VND), B-21 — each recorded as a Site Setting with the interim value and shown in F48 explanation as "interim rule". | M |

---

## 12. Non-functional requirements

| ID | Category | Requirement | Pri |
|---|---|---|---|
| NFR-01 | Performance | Clean claim (scenario A, 2 documents, 1 page each) from submit to `PAID` ≤ 90 s with live IDP; ≤ 20 s in replay. | M |
| NFR-02 | Performance | Chain re-run ≤ 2 s; console screens ≤ 2 s load. | S |
| NFR-03 | Determinism | Identical claim inputs → identical `ChainRun` outputs and hash, across runs and environments. | M |
| NFR-04 | Availability (demo) | Replay mode makes the full demo independent of IDP, Google Fonts and external images; console bot icon has an embedded fallback. | M |
| NFR-05 | Auditability | 100 % of agent turns, tool calls, state changes and officer decisions logged with actor and timestamp; log is append-only. | M |
| NFR-06 | Security | ODC roles per §4.2; agents run under a restricted role; no agent tool can write money/state; secrets in ODC settings; PII masked in agent context and demo data. | M |
| NFR-07 | Privacy | No real policyholder documents on any tenant/slide/video; anonymisation checklist signed before upload (BC-1). | M |
| NFR-08 | Resilience | IDP timeouts, 4xx/5xx, quota exhaustion, empty results → typed errors and operator queue; never a decline; circuit breaker 5 errors/60 s. | M |
| NFR-09 | Observability | Presenter trace shows per-agent duration and replay flag; KPI screen reads measured values from entities, not constants. | S |
| NFR-10 | Localisation | Customer UI VI; console EN default + VI; date `dd/MM/yyyy`, amounts `1.880.000 ₫`. | S |
| NFR-11 | Accessibility / responsive | Portal usable at 390 px width; console at ≥ 1180 px with panel collapsing rule. | S |
| NFR-12 | Maintainability | Each chain step is its own Server Action with unit tests for scenarios A–D; `IdpFieldMap` and thresholds configurable without republish. | S |
| NFR-13 | Rate limits | ≤ 100 IDP requests/60 s; agent turn ≤ 120 s; Track 2 poll ceiling 15 min. | M |

---

## 13. Governance and guardrails

| ID | Guardrail | Implementation |
|---|---|---|
| GR-01 | Money is code | Only `RunAdjudicationChain` and `PostLimitLedger` write monetary fields; agents have no such tool (FR-AGT-00a). |
| GR-02 | 4-condition gate | No automatic approval without G1–G4 all true; G4 failure blocks payment regardless of amount. |
| GR-03 | Evidence mandatory | A recommendation without at least one evidence item is rejected by `ProposeRecommendation`. |
| GR-04 | Human confirmation with reason | Confirm/Reject of a recommendation, Clear of a fraud signal, Waive of a document, Decline — all require a reason; stored with actor. |
| GR-05 | Complete audit trail | `AgentRun` + `ClaimEvent` append-only; presenter trace readable by judges. |
| GR-06 | Fail-safe and PII | IDP/agent failures never decline; PII masked; anonymised demo set; letters never accuse ("kê sai") — wording dictionary enforced. |
| GR-07 | Four-eyes | Approver ≠ officer; auto-approval only under threshold. |
| GR-08 | Knowledge versioning | Every clinical recommendation records KB version and IDP workflow id. |

---

## 14. Reporting, KPIs and ROI

| KPI | Definition | How measured in the slice |
|---|---|---|
| Extraction time per page | IDP `processingTime` | Stored per `ClaimDocument`; measured 16.1 s |
| Assessment time | Flow 28/8 start → Finished | `ClinicalAssessment` timestamps; ~30 s |
| Extraction accuracy | Field-level match vs ground truth | `POST /request/compare`, 10–20 anonymised docs → `AccuracyRun` (**not yet measured**) |
| STP rate | Claims `APPROVED` by gate / total | `ChainRun.GateResult` |
| Cycle time | `SUBMITTED` → `APPROVED/DECLINED` | `ClaimEvent` |
| Officer touch time | Time in `PENDING_REVIEW` + `PENDING_APPROVAL` | `ClaimEvent` |
| Lines keyed manually | Lines with `Source = Officer` created (not edited) / total | `ClaimLine` |
| Recommendation acceptance | Confirmed / Proposed | `ClinicalRecommendation` |

ROI one-pager inputs (all labelled assumption unless measured): 990 claims/year, 4.3 bn VND paid, 3/5-day SLA, adjudicator minutes per claim before/after, STP share, cost per hour. Output: hours saved per year, cycle-time reduction, leakage avoided from NOTCOV/NOTREL consistency.

---

## 15. Acceptance criteria (per scenario)

| ID | Scenario | Acceptance test | Pass condition |
|---|---|---|---|
| AC-A1 | A | Submit 2 anonymised documents via portal with live IDP | Claim reaches `PAID` with no officer action; `AgentRun` shows hand-offs 1–7; customer S13 shows payable = requested; letter sent |
| AC-A2 | A | Same claim in `ReplayMode` | Same amounts, same `ChainRun` hash, ≤ 20 s |
| AC-B1 | B | Submit claim with supplement line + over-cap line | `ClinicalRecommendation` Proposed on line 4 with ≥ 1 evidence; gate fails G3; state `PENDING_REVIEW`; F56 lists "Confirm or reject line 4" |
| AC-B2 | B | Officer rejects without reason | Blocked by UI; reason mandatory |
| AC-B3 | B | Officer confirms; chain re-runs | Before/after shows 2,000,000 → 1,880,000; breakdown NOTCOV 80,000 + CAP_VISIT 40,000; checklist 7/7 green; submit → approver approves → `PAID`; S13 shows "why you pay" by cause |
| AC-C1 | C | Submit claim with future event date + unknown hospital + 3 low-confidence lines | Flow item `warning_level 1`; `FraudSignal` DATE_IMPOSSIBLE (High) + HOSPITAL_UNMATCHED; gate fails G4; state `BLOCKED`; no payment order exists |
| AC-C2 | C | Officer marks 3 lines verified, clears/keeps signals | Verified lines required before submit; if signal kept → decline path with reason `FRAUD`; letter wording does not accuse |
| AC-D1 | D | Claim inside waiting period | Chain step 2 excludes all lines with `WAIT`; decline letter cites clause |
| AC-X1 | Any | IDP returns HTTP 200 with `errorCode` | Claim `EXTRACTION_ERROR`, operator queue, never `DECLINED` |
| AC-X2 | Any | Track 2 exceeds 15 min | Claim continues with Track 1, `PARTIAL`, `CLM-CONSOLE-015` shown |
| AC-X3 | Any | Attempt to call money-writing action from an agent tool list | Not possible — tool not exposed; verified by tool list review with SA |
| AC-X4 | Any | Presenter opens Agent trace | Every hand-off has `HandoffTo`, duration, model, KB version |

---

## 16. Risks and dependencies

| ID | Risk | Impact | Mitigation |
|---|---|---|---|
| RK-01 | Agent Workbench does not support agent→agent calls | Orchestrator design | Fallback Server Action/BPT (FR-AGT-07); narrative via `AgentRun` |
| RK-02 | EAP / tenant allow-list issues on pitch day | No live demo | Dry-run on clean tenant 30/09–06/10; backup video |
| RK-03 | IDP staging down or flow 28/8 changed | Track 1/2 fail | `ReplayMode`; pin workflow id; ask IDP team to freeze flow for 4 weeks |
| RK-04 | PII in demo documents | Compliance, disqualification | Anonymised set only; checklist before upload (NFR-07) |
| RK-05 | Clinical KB has no medical sign-off (RK-08 MVP1) | Credibility | Present as advisory with officer confirmation; state limitation openly |
| RK-06 | MVP1 open items B-19/20/21 change amounts | Demo numbers challenged | Interim decisions as Site Settings, shown as "interim rule" (IR-07) |
| RK-07 | Extraction accuracy still "not measured" at pitch | Weak ROI | Run compare by 29/09; otherwise show honest "not measured" tile |
| RK-08 | Single developer until 22/10 | Schedule | Cut order: drop D → drop portal chat → drop dashboard → keep floor "one agent, one Server Action, one correct number, one `AgentRun`" |
| RK-09 | Reference insurer not named | Judging criterion 1 | Descriptive placeholder agreed in advance |

Dependencies: OutSystems EAP enablement; BnK IDP tenant account; anonymised document set; P-02 benefit snapshot; SA answers from Enablement 07/09 (agent tools, agent→agent, guardrails, long-running handling).

---

## 17. Delivery plan (traceability to build)

| Phase | Dates | Delivers | BRD items |
|---|---|---|---|
| Foundation | 05–06/09 | Entities + seed, REST login/sync, chain 14 steps, gate, 2 console screens, replay JSON | FR-CORE-01…29, FR-IDP-01/02/09–13, FR-CON-02/08/10 |
| Enablement | 07/09 | Architecture decisions (AS-2, AS-3) | §7.1 fallback |
| Hackathon day | 08/09 | AG-2 + AG-0 on tenant; scenario B end to end seen by SA | FR-AGT-01…07, 14–17 |
| Sprint 1 | 09–15/09 | Full console detail tabs, F56, queues, scenarios A + B live | FR-CON-06…14, FR-AGT-08…13 |
| Sprint 2 | 16–22/09 | AG-3, AG-4, AG-5, portal S07–S13, scenario C | FR-AGT-18…34, FR-POR-01…09 |
| Sprint 3 | 23–29/09 | Accuracy run, KPI/ROI screen, EN deck, backup video | FR-IDP-14, FR-CON-20…24, §14 |
| Rehearsal | 30/09–06/10 | 3 pitch rehearsals, clean-tenant dry run, submission | §15 all AC |
| Pitch | 07–08/10 | Live demo B → A → C → architecture | — |
| Result / pilot | 22/10 → Q4 | 3-person team, pilot with reference insurer, IDP gap requests | IR-06 |

---

## 18. Glossary

| Term | Meaning |
|---|---|
| NOTCOV | Item is not a covered benefit under the product (e.g. dietary supplement, cosmetic) |
| NOTREL | Item is not related to the diagnosed condition |
| Track 1 / Track 2 | Track 1 = document extraction (IDP `/request`); Track 2 = clinical/consistency assessment (IDP `/workflow` flow 28/8 + Clinical agent) |
| Gate (G1–G4) | Four conditions for automatic approval |
| Chain | `RunAdjudicationChain`, the 14 deterministic steps that produce all amounts |
| `AgentRun` | Append-only audit record of one agent turn / tool call with hand-off target |
| Replay mode | Site setting that serves recorded IDP responses for offline demo |
| Thời gian chờ | Waiting period |
| Đồng chi trả | Co-payment |
| Trần/lần | Per-visit cap |
| P-02 snapshot | Benefit snapshot of the reference products captured for MVP1 |
| KBYG | "Know Before You Go" hackathon briefing, 24/08/2026 |
| STP | Straight-through processing (no human touch) |

---

## 19. Open points for sign-off

| # | Question | Owner | Needed by |
|---|---|---|---|
| 1 | Confirm exact step names/order of the 14-step chain against MVP1 FS F48 (this BRD's CH-01…CH-14 is the working set), and the line-level split of the 120,000 VND difference in scenario B (80,000 NOTCOV + 40,000 CAP_VISIT is illustrative; only the 2,000,000 → 1,880,000 totals are fixed) | BA | 12/09 |
| 1b | Scenario lettering: this BRD uses A = clean/auto-approve, B = NOTCOV + cap, C = fraud (per strategy note 03/09). Prototype v4/v5 label the clean claim "B" and the clinical one "A" — align labels in prototype, deck and demo script | PO | 12/09 |
| 2 | Confirm G2 thresholds (0.90 claim / 0.80 line) and `AutoApproveThreshold` 2,000,000 VND | PO | 12/09 |
| 3 | Interim values for B-19 / B-20 / B-21 | PO | 12/09 |
| 4 | Agent→agent support in Agent Workbench (Enablement outcome) | SA | 08/09 |
| 5 | Reference insurer naming consent | BnK management | 29/09 |
| 6 | Anonymised document set (10–20) + ground-truth XLSX | Integration team | 20/09 |
| 7 | Whether scenario D and portal chat are kept in scope | PO | 15/09 |

*End of document.*
