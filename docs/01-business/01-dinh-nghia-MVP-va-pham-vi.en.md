# Defining the MVP and scope — what we build, what we deliberately do not

> English translation for reviewers. Vietnamese original: [01-dinh-nghia-MVP-va-pham-vi.md](01-dinh-nghia-MVP-va-pham-vi.md).

## Three things called "MVP" — don't mix them up

| Name | What it is | Role in the hackathon |
|---|---|---|
| **MVP1** (BnK project documents) | The full contract scope: J1 + J2, 8 document types, console F42–F67, 8-tab claim detail, 13 mobile screens, real core | Business source. **Not taken to the competition.** |
| **Prototype v4/v5/v6** (HTML files) | A clickable mockup, no backend | **Spec** and fallback video. The organisers do not accept it. |
| **The "MVP version"** the organisers require | A real, running, smallest-possible build on ODC + Agent Workbench that proves the use case | **This is the submission.** |

## Definition of the submission

> A health claim goes through the full loop: the customer uploads document photos → BnK IDP really reads them → agents on Agent Workbench interpret and decide → the deterministic chain computes the amount → the claim officer sees the result with reasons → there is a log of who did what.

**In scope:**

- 1 treatment type (inpatient), 3–4 document types that already have IDP templates
- 3 scenarios: **B** clean, auto-approved · **A** has a clinical recommendation needing human confirmation · **D** declined because of the waiting period
- 5 agents: Orchestrator, Intake, Adjudication, Clinical, Customer-communication
- Real IDP via REST; policies seeded into the Entity from the P-02 snapshot
- Console: queue + claim detail + AgentRun log

**Deliberately out of scope:** J1, native mobile, 8 document types, InsureMO sandbox, the full customer portal, bilingual support, reissuing BA documents.

## The MVP for 8/9 alone (3 hours on site)

Much smaller, because 3 hours cannot build the above:

**Floor (must not be breached):** one claim-B flow · one agent (AG-5) · one Server Action · one correct number · one `AgentRun` record.

**Target (since 04/09, after learning that MCP can create agents):** **two agents that can talk to each other** — AG-0 orchestrates AG-5 — with a readable handoff log.

AG-5 can now be built at home on the evening of 6/9 with `app_create` kind `AIAgent`, so the 3 hours in the hall no longer go on building the first agent. Spend them on something more expensive: verifying whether **one agent's callable-action list accepts another agent**, and asking the SA that question directly.

## Cut line when running late

Cut in this order; cutting early beats cutting late:

1. Drop claims A, C, D — keep only B
2. Drop the queue screen — go straight to the detail by URL
3. Drop AG-0 — having only AG-5 run is enough to prove Agent Workbench (but loses the agent-to-agent points)
4. Drop real upload — run ReplayMode with recorded JSON

**Floor that must not be breached:** one agent on Agent Workbench, calling one Server Action, producing one correct number, with an `AgentRun` record.

## 6-agent architecture (target 7–8/10)

| Agent | Role | Tools |
|---|---|---|
| **Orchestrator (AG-0)** | Coordinates, waits for both branches to settle, guards the 4-condition gate, writes the audit | Server Actions in CLM_Core |
| **Intake (AG-3)** | Receives files, calls IDP, checklist by `templateId`, supplement requests, `ExtractionError` | `IdpSubmitSync`, `IdpGetRawFile` |
| **Adjudication (AG-5)** | Calls the 14-step chain as a tool, explains each step in Vietnamese — **does not compute itself** | `RunAdjudicationChain` |
| **Clinical (AG-4)** | Receives the 28/8 flow warning + lines, looks up the KB, returns NOTREL/NOTCOV + evidence + band | `IdpStartAssessment`, `IdpGetAssessment`, KB |
| **Fraud-signal (AG-6)** | Looks up the OpenSearch hospital, duplicate claims, condition 4 of the gate | `IdpSearchHospital`, Entity Claim |
| **Customer-communication (AG-7)** | Vietnamese result/decline letters from templates, never says `"kê sai"` (EN: "prescribed wrongly") | Letter templates |

The Clinical agent exists for one specific reason: IDP does **not** return `proposedReasonCode`, `evidence[]` or a band per cost line (gaps H7–H9 in the comparison table). That is exactly the gap the agent fills — and a strong argument for building on Agent Workbench instead of waiting for IDP.
