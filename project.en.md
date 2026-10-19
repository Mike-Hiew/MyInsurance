# openspec — Agentic Health Claim (BnK · hackathon 07–08/10/2026)

> English translation for reviewers. Vietnamese original: [project.md](project.md).

Directory conventions following **spec-driven development** (OpenSpec), trimmed down for one person over 9 days.

## Three layers

| Folder | What it is | Who edits |
|---|---|---|
| `specs/<capability>/spec.md` | **Stable source of truth** — what the system *is* | Only edited when a change is archived |
| `changes/active/<name>/` | **The running delta** — what is *changing*, not yet in the spec | Edit freely while working |
| `changes/archive/<name>/` | Finished changes, kept as a record | Do not edit |

The only rule: **a change to system behaviour must exist in `changes/active/` before it exists in code.**
The pain we hit: the "4-condition auto-approval gate" lived side by side with a sentence saying "the standard is 3" in `build-00` for ~90 revs without being recorded anywhere. Ruling on 21/09: **the code was right, the document was wrong** — and this only came to light when someone put the two side by side.

## Differs from original OpenSpec in three deliberate ways

1. **No `proposal.md` / `design.md`.** One person; every architecture decision is already in `build-00` §5. Only `tasks.md` is kept.
2. **No `schemas/`** until after the pitch.
3. **Verification is not by git diff.** The code is an ODC app in the cloud edited by Mentor — no repo, no PRs. The replacement is **runtime audit + screenshots** in `../output/`. Don't drop that to chase a git process.

## specs/ — where the full content lives

The full text of the 7 specs currently lives in the **"Hackaton" Project on claude.ai** (`claude/build-00…06`). Each `specs/<cap>/spec.md` below is a **summary of the immutable constraints + pointers**, not a copy.

| `specs/` | Source doc in the Project | Content |
|---|---|---|
| `data-model/` | `build-01-data-model-va-enum` | 32 entities · 31 enums · 30 constraints |
| `idp-integration/` | `build-02-tich-hop-idp` | 32 endpoints · 22 templateIds |
| `claim-rules/` | `build-03-nghiep-vu-va-rule` | 8 document types · 88 fields · R1–R9 · ~45 BR-* |
| `adjudication/` | `build-04-chuoi-tinh-tien-va-seed` | 14 steps · 7 decline codes · seed · 65 unit tests |
| `agents/` | `build-05-agent-va-system-prompt` | 6 agents · 39 tool bindings |
| `screens-wording/` | `build-06-man-hinh-va-wording` | screens · 53 verbatim messages |
| *(scope & rulings)* | `build-00-chuan-va-pham-vi` | the baseline standard, 10 findings, 8 conflicts ruled on |
| *(progress)* | `trang-thai-build-2026-09-20` | which app has what, what is still missing |

**The upstream sources of those specs are on disk** at `../Preparing/04_Nghiep-vu-va-du-lieu/` — A1 Glossary, A2 Message Catalogue, A3 Permission Matrix, A4 Document Catalogue, A5 Product Data Sheet, D1 BRD, D2 Functional Spec, D3 Integration Spec. When a spec and the BRD disagree, **A5/D2 win** (`build-00` §5).

## Lifecycle of a change

1. `mkdir changes/active/<short-name>/` → write `tasks.md`
2. If the change alters a spec: add `specs/<cap>/spec.md` containing **only the differing passage**, don't copy the whole spec
3. Do the work, tick tasks
4. Done → merge the delta passage into the corresponding Project doc → `mv changes/active/<x> changes/archive/`

## Status today — 20/09/2026

| Change | Status | What it blocks |
|---|---|---|
| `verify-entity-vs-build01` | 🟡 batch A closed (rev 132/50) · **batches B/B2/B3 closed 21/09** — ClaimMVP **rev 139** (B4: F-4 + C-1 21/09) + MyInsurance **rev 51**: readout `51 · 12 · 17/17/17 · 0 · 0 · 0 · 0 · 0`, 5 blast-radius screens ✅, claim 0001 = outpatient hero with 6 lines, scenario B matches A5 to the last đồng (path C, NOTREL) · findings F-1…F-4 + C-1 recorded in `runtime-check-batchB.md` · task 12 remains | **Next: `chain-14-steps-a5`** — 14 Server Actions in A5 order, 65 tests. Nothing else · 2 outstanding questions (IDP Q-IDP-01, SA agent↔agent) drafted in `questions-pending.md` — PO to send |
| `idp-integration-live` | 🔴 not started — **critical path** | Tenant **177**, set **198**, templates **6090–6098**. The flow has a pinned mail through 08/10; **quota not yet closed** |
| `approval-gate-4-conditions` | 🟡 PO fixed **4** (21/09) | Still open: *WHICH 4 conditions* — the screen and the doc list two sets that both count to 4 |
| `chain-14-steps-a5` | 🟢 21/09 — **the 14 A5 steps run for real on ClaimMVP rev 162**: `A5Step01…14`, `RunAdjudicationChain(ClaimId, TriggeredBy)` (ChainRun RunNo/InputsHash/DurationMs/StepResults, `LedgerPosting` PROVISIONAL AMOUNT/OCCURRENCE/**DAY**, CalcSnapshot 14 rows, only step 14 writes money), `A5Ledger_Finalize/Reverse` (not yet wired to approve); `ClaimLine.TreatmentDays` = A4 L18 (step 9 DAY, step 14 posts DAY to 100.1.1 + 100.1); claim 0001 = **2.000.000** (trace 719 ms; ServerRequestTimeout 60 s) · test suite 71+1 (`ChainTests`, Mode DRYRUN/CONFIRM_RERUN/RUNTWICE/CONCURRENT/RUN): **46/46 pass** (run #11) — T-CHAIN-05 RERUN 2.000.000 → **1.880.000** (machine: +18.000 cap 200.2, +30.000 copay), T-CHAIN-06 MOVED 640.000 → 360.000 (BR-LEDGER-8, Finalize via runner), T-CHAIN-07 POSTED 3.000.000 "2 DAY (100.1.1 +1, 100.1 +1)", T-CH12-d W-8 PASS · seed fixture cleanup real from rev 162 ("Deleted 185, now 42") · log `current-chain.md` §5–8, results `test-run.md` | **26 tests remain:** T-CH14 (6) · T-SEED (11) · T-GATE BLOCKED (3/4 conditions) · T-CH11-c/d MANUAL · **gap:** T-CHAIN-06 must be re-run through Approve when Finalize is wired to the gate · F48: static card "AFTER CONFIRMATION 0 ₫", gate card reads an old run, ledger panel Left 0 (F-CH-1…3, batch F) · `Seed_AssessmentAndTerms` — PO fixes by hand in Studio · step 11 log "(không có)" (EN: "(none)") → batch F · delete TST-/FX- fixtures before the demo (now 1 click) · 2 outstanding questions not yet sent |
| `ui-100pct-drive` | 🟡 code done (ClaimMVP rev 128 · MyInsurance rev 46, audit 100 %), **recorded retroactively** · remaining: merge the delta into `build-01`/`build-06` then archive | Blocks nothing |

Done and in `changes/archive/`: the whole UI pass (console rev 30–91, portal rev 1–9). The "100 %" pass (console 96–119, portal 15–38) is in `changes/active/ui-100pct-drive/` awaiting archive.
