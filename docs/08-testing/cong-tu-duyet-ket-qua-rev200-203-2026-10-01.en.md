# Auto-approval gate — real run on the tenant · rev 200–203 · 01/10/2026

> English translation for reviewers. Vietnamese original: [cong-tu-duyet-ket-qua-rev200-203-2026-10-01.md](cong-tu-duyet-ket-qua-rev200-203-2026-10-01.md).

Follow-up to `claude/cong-tu-duyet-4-dieu-kien-doc-tu-tenant-2026-10-01.md` (specification, PO chose option A).

## 7. Done 01/10 — gate runs for real (rev 200 → 203), verified on screen

| Rev | Change | Verification |
|---|---|---|
| 200 | Server action `EvaluateAutoApprovalGate(ClaimId)` — C1..C4 Pass/Reason, AllPass, Threshold, ChainRunId. Read-only. | — |
| 201 | `gateCard` on ClaimDetail reads the action through Data Action `GetGate`; label **AG-4**; honest table footer "Threshold … fixed in this build (not yet an admin setting)" | Screen displays; but **every claim shows "1"** for every count → bug found below |
| 202 | **Fixed counting bug**: 5 aggregates have a COUNT column so they always return 1 row → `.Count` always = 1. Changed to `If(X.List.Empty, 0, X.List.Current.<alias>)`. `GetLatestChainRun` sorts RunNo DESC, Id DESC. | Claims 17/19/20 show different numbers, matching the data |
| 203 | **C1 payable** takes `CalcSnapshot.ResultAmount` of the last step (largest StepNo) of the latest run — because `RunAdjudicationChain` **never writes** `ChainRun.TotalPayable` (always 0) | Claim 17: 2.000.000 ₫ |

**Result on screen after rev 203 (hard reload):**

| Claim | Header | C1 | C2 | C3 | C4 |
|---|---|---|---|---|---|
| 17 · CLM-2026-0001 | not met — needs a person | ✕ 2.000.000 ₫ · 2 limit cut(s) in run #2 · 0 declined | ✕ 6/6 assessed · 1 proposal pending | ✓ 0 checkpoint | ✓ 0 other claim |
| 19 · CLM-2026-0003 | **all met** | ✓ 1.680.000 ₫ · 0 cut · 0 declined | ✓ 4/4 · 0 pending | ✓ | ✓ |

⇒ The demo has a control pair: **19 passes the gate, 17 does not** — both computed from data.

### Findings still open (not fixed)

1. **`ChainRun.TotalPayable / TotalCovered / TotalCustomerBorne` are never written** (`RunAdjudicationChain`: `CreateCR` only sets ClaimId/RunNo/InputsHash/Version/RunAt/TriggeredBy; `UpdateCR` only sets StepOutputsJson/DurationMs). This is the root of **F-CH-1** (card "RUN #2 (AFTER CONFIRMATION) 0 ₫"). Root fix: `UpdateCR` writes TotalPayable = result of step 14. Claim 19 has 1.680.000 because its run was seeded directly.
2. **C3 / TermCheckpoint:** claim 17 has **0 rows** in TermCheckpoint, while the B04 step detail of run #2 says "TERM-HC-003 — checkpoint dòng 6 (TBC-03 chưa chốt)" (EN: "checkpoint line 6 (TBC-03 not yet decided)"). The gate is correct with respect to the entity; the entity data and the B04 text disagree. The checkpoint queue currently shows 1 item but that is a **NOTREL recommendation** (Recommendation), not a TermCheckpoint.
3. **The 2.000.000 threshold** is still a literal — no Setting/entity yet; the Administration screen is still static text.
4. Claim 20 (CLM-2026-0004, Victoria, 15/01/2026) is a trial claim of the same insured person — not a duplicate of 17 (different facility) ⇒ C4 = 0 is correct.

## 8. Rev 204 — root fix of F-CH-1 (01/10)

`RunAdjudicationChain`, an Assign node between `GetCRForUpdate` and `UpdateCR` adds:
- `NewCR.TotalPayable = CallS14.ResultAmount`
- `NewCR.TotalCovered = RunCtx.CoveredTotal`
- `NewCR.TotalCustomerBorne = RunCtx.TotalCustomerBorne`
- `TotalRequested` not assigned (not available in the action).

The Assign node before `CreateCR` is unchanged (verified). Publish of rev 204 succeeded, 7 errors/135 warnings as before.
**Only takes effect for new chain runs.** Old ChainRuns are still 0 ⇒ the card "RUN #2 (AFTER CONFIRMATION) 0 ₫" of claim 17 will only change after the chain is re-run (writes demo data — needs Mike's permission). The gate does not depend on it because it already reads CalcSnapshot (rev 203).

## 9. Rev 205–206 (01/10, afternoon)

| Rev | Change | Verification on screen |
|---|---|---|
| 205 | App setting **`AutoApprovalThreshold`** (Decimal, default 2.000.000). `EvaluateAutoApprovalGate` reads the setting instead of the literal. Server action `GetAutoApprovalThreshold` + data action on Administration. Removed the fake lines "Since 01/08/2026" and "Lê Minh Tuấn · 28/07/2026 · 2 administrators confirmed". | Administration: "Current 2.000.000 ₫ · Set by app setting AutoApprovalThreshold in ODC Portal · no change history in MVP 1"; Thresholds tab reads the setting. ClaimDetail footer: "Threshold 2.000.000 — app setting AutoApprovalThreshold, changed in ODC Portal." |
| 206 | **Honest data check** — no data deleted. | "Claims WITHOUT loss type (expect 0)" = 0 ✓ · Schedule HC-STD 51 rows / 3 policies ✓ · HC-PLS 17/1 ✓ · HC-PRM 34/2 ✓ |

**Conclusion on the "dedupe cleanup" task (item 3):** there are no duplicate records. The old red numbers were because the expectations were written when the DB had 1 policy/plan and 12 claims; fixtures were added afterwards (FX-STD-2026, FX-STD-UNPAID, FX-PRM-2026 and the TST-* claims). Deleting according to the old numbers would **break the test fixtures**.

Still red:
- **Legacy/empty cost-line codes = 121**: `ClaimLine.BenefitCode` ∈ {INPATIENT_ROOM, INPATIENT_SURGERY, INPATIENT_OTHER, OUTPATIENT, DENTAL, ""} — old prototype codes or empty. Not touched (need to know whether the chain reads this column before migrating).
- **Policies without PremiumPaid (excl. FX-) = 1**: Mentor's filter excluding FX- did not take effect (most likely wrong attribute). The unpaid policy is expected to be fixture FX-STD-UNPAID (for T-CH01-c). Not verified.

**Re-running the chain for claim 17 (item 1):** the system blocked Claude from clicking "Run chain (writes)" (writes demo data). Mike clicks it himself: Administration → Chain dry-run → Claim Id 17 → Run chain (writes). Baseline before the run: ChainRun of claim 17 = 2 runs; limit ledger 200 Used 2.000.000 / Left 18.000.000.

**T-GATE-01..07** on Chain Tests are still written per the old G1–G4 (T-GATE-04 = confidence 0,80; T-GATE-05 = BLOCKED) — contrary to option A. They must be rewritten per C1–C4.

## 10. Rev 207 — T-GATE per C1–C4 (01/10, evening)

- Action `UpdateGateTests` (rewrites the 7 T-GATE rows), `RunGateTests` (calls `EvaluateAutoApprovalGate`, compares the P/F pattern `C1 C2 C3 C4 AllPass` + strings in the reason). `Seed_ChainTests` calls `UpdateGateTests` at the end. The Chain tests screen gets 2 more buttons + a gate counter. 7 errors (old), 137 warnings (+2 Long Integer type warnings on badges, like the old badges).
- Specification: `claude/build-04-bo-sung-v1-1-cong-c1-c4.md`. Source file `chain-tests-71.json` synchronised (old version `.bak-before-cgate`). `approval-gate-4-conditions/tasks.md`: ticked 1b, 3, 4, 6.
- **Waiting for Mike to click** (Claude is blocked from writing data): Chain tests → *Update T-GATE rows (C1–C4)* → *Run gate tests*. Expected 4/4 gate pass (01, 02, 04, 05); 03 PENDING (missing OPEN checkpoint fixture); 06, 07 MANUAL.

## 11. Real run result (01/10, rev 208)

- Mike clicked "Run chain (writes)" for claim 17 → **run #3** (ChainRun 232, 14 snapshots, TriggeredBy admin): payable 2.000.000, card "RUN #3 … 2.000.000 ₫" (no longer 0 ₫ — F-CH-1 closed on data). Limit 200 still Used 2.000.000 / Left 18.000.000 — **not double counted**.
- Rev 208: fixed `UpdateGateTests` (misaligned Input/Expected/Catch columns) and `RunGateTests` (an exception caused a silent rollback → now writes ERROR onto the row).
- Mike clicked Update + Run gate tests → **4/4 gate pass**:

| Test | Claim | Expected | Actual |
|---|---|---|---|
| T-GATE-01 | CLM-2026-0003 | PPPPP | ✓ PPPPP |
| T-GATE-02 | TST-T-CHAIN-01 | F???F | ✓ FFPFF |
| T-GATE-04 | CLM-2026-0001 | ?F??F | ✓ FFPPF |
| T-GATE-05 | TST-T-CHAIN-01 | ???FF | ✓ FFPFF |

- Remaining: T-GATE-03 PENDING (missing OPEN checkpoint fixture); 06/07 MANUAL. The counter "x/46 pass" is also counting GATE rows (4/46) — the 46-test chain has not been re-run since the re-seed.
- Side finding: the card label "RUN #n (AFTER CONFIRMATION)" is assigned to every run ≥ 2 even if nobody has confirmed; the Limit ledger screen shows customer Nguyễn Văn An as having used 19.000.000 (period 04/2026–03/2027, not linked to a claim) — suspected fixture data from T-CH12-a.

## 12. Rev 209 — the two run cards tell the truth (01/10)

Card 1 = latest run + who ran it + payable from the last-step CalcSnapshot; card 2 = previous run + difference. Removed the hard-coded text "after confirmation · line excluded · re-capped · co-pay recomputed". Claim 17 after hard reload: "RUN #3 · ADMIN · LATEST 2.000.000 ₫" / "RUN #2 · SEED · PREVIOUS 2.000.000 ₫ · same payable as the latest run".

## 13. 19.000.000 on the Limit ledger — CORRECTION (01/10)

**The earlier conclusion was WRONG, nothing needs fixing.** On the first read, pressing End+Enter on the dropdown selected *Fixture Member 6* instead of Nguyễn Văn An. Re-checked by selecting the right option:
- **Nguyễn Văn An**: Consumption records = CLM-2026-0001 · 200 · 2.000.000 · Provisional; 200.1 · 1.200.000; 200.2 · 800.000. **Matches the claim 17 screen.** The "used" counter = 0 because it counts only FINAL (provisional is not yet finalised) — correct per BR-LEDGER-1.
- **19.000.000** is a fixture row of test T-CH12-a: `Seed_ChainTestFixtures`, node `assignNode24` → `MemberIdM6` (Fixture Member 6), `PolicyIdStd` (FX-STD-2026), BenefitCode 200, PeriodYear 2026, UsedAmount 19000000, RemainingAmount 1000000. As intended, does not touch the demo customer.
- The Mentor session that was about to fix `Seed_LimitLedger` (based on Mentor's wrong answer) was **cancelled, not published** — the rev is still 209.
- One minor point remains unclear: the period label on the Limit ledger screen for Nguyễn Văn An shows 01/04/2026–31/03/2027, while the chain shows the policy as 01/01–31/12/2026.

## 14. Rev 210 — Policies header + Limit ledger period label follow the selection (01/10)

**Bug:** the Policies screen always shows the first policy in the header even when another policy is selected; the period label on the Limit ledger took `GetMembersAll.List.Current` (= the first member in the list), so Nguyễn Văn An was shown the period of Fixture Member 6 (01/04/2026–31/03/2027). This is the origin of the "unclear" point in §13.

**Fix (1 Mentor session, published rev 210):**
- Policies: added aggregate `GetSelectedPolicy` (Policy ⋈ PolicyStatus, Policy.Id = SelectedPolicyId, max 1); header DdPolicy / DdHolder / DdProduct / premium bind to it; refreshed in RefreshSnapshot and OnAfterFetchPolicies.
- LimitLedgerView: added `GetSelectedMemberPolicy`; the period label binds to it instead of `GetMembersAll.List.Current`.

**Verification on screen (selected using form_input, selected option confirmed):**
| Screen | Selection | Result |
|---|---|---|
| Policies | HC-2026-0001 · Nguyễn Văn An | Policy HC-2026-0001 · HC-STD · Holder Nguyễn Văn An · HC-STD v4 · benefit 200 = 20.000.000, 20% ✓ |
| Limit ledger | Nguyễn Văn An | period **01/01/2026 – 31/12/2026** ✓ (matches the chain); 3 provisional rows CLM-2026-0001 (2.000.000 / 1.200.000 / 800.000) |
| Limit ledger | Fixture Member 6 → 7 | period 01/04/2026 – 31/03/2027 (fixture policy FX-STD-2026); label changes with the selection ✓ |

→ The "unclear" point in §13 is closed.

## 15. Rev 211–212 — two wrong counters + discovery of a C3 gap (01/10)

**Rev 211 — ChainTests:** the counter shows "50/46 pass" because `GetCntPass` (filter `Pass and LastRunAt > #1900-01-01#`) also counts the 4 T-GATE rows. Added `and ChainTest.Status <> "GATE"` (Mentor proposed `Index(TestId,"T-GATE") <> 1` — wrong because Index returns a 0-based position; corrected). `GetCntFail` already filtered `Status = "RUNNABLE"` so unchanged. After hard reload: **46/46 pass · 0 fail · 18 waiting for a step · 4 outside the chain · 4/4 gate**. → All 46 chain tests pass after the re-seed (run of 01/10 12:50).

**Rev 212 — Work list:** header "0 claims · all handlers" while the table has 54 claims. `CountExpr` used `TotalAll` from `WorkList_StatusCounts`, which only counts claims assigned to the current user. Changed to `GetClaims.Count + " claims · all handlers"`. After hard reload: **54 claims · all handlers** ✓.

**Discovery (not fixed — decision needed):**
- **Nothing writes the `TermCheckpoint` table.** Step 4 (`A5Step04_GeneralExclusions` → `A5Step04_EvalTerm`) only marks the Held line in `ChainCtx` (in-memory: `Checkpoints`, `CheckpointCount`, `HoldTermCodes`/`HoldReason`), and does not call `CreateTermCheckpoint`. Consequence: **C3 always Passes** — a claim with a line held at step 4 (e.g. SUPPLEMENT → TERM-HC-003) can still pass the gate if C1/C2/C4 pass. Contrary to BR-TERM-13.
- **The Checkpoint queue screen** reads `Recommendation` (clinical recommendations awaiting decision), not `TermCheckpoint`. The only current item is the NOTREL Atorvastatin on CLM-2026-0001 — genuinely work waiting for a person, but the screen title speaks of "term checkpoint".
- **T-GATE-03** is left PENDING because `FixtureClaimNo = ""` and `Status = "PENDING"` (RunGateTests only takes `Status = "GATE"`). A fixture is impossible as long as there is no place that writes TermCheckpoint.
- The test chain (`TST-*`) runs in-memory and does not save a ChainRun for TST claims (e.g. TST-T-CH04-a shows "No chain run yet").

## 16. Rev 213–214 — C3 also catches held steps (01/10)

**Mike's decision:** fix on the gate side, do not touch the chain.

**Rev 213 — `EvaluateAutoApprovalGate`, C3 only:** added aggregate `GetHoldSteps` (CalcSnapshot, filter `ChainRunId = <latest run> and Outcome = "HOLD"`, column `COUNT(Id)` alias `HoldCount`), read with `If(GetHoldSteps.List.Empty, 0, LongIntegerToInteger(GetHoldSteps.List.Current.HoldCount))`. C3 passes when there are 0 OPEN TermCheckpoints **and** 0 HOLD steps. Reason: "<n> step(s) on hold in run #<number> — a person must clear them".

**Verification on screen (hard reload):**
| Claim | Before | After |
|---|---|---|
| CLM-2026-0001 (17) | C3 ✓ "No open checkpoint (0 …)" — **wrong**, B04 records "1 checkpoint" | C3 ✕ **"1 step(s) on hold in run #3 — a person must clear them"** |
| CLM-2026-0003 (19) | all met | still **all met**; C3 "No open checkpoint (0 checkpoint(s) on this claim, all decided) · 0 step(s) on hold" |

**Rev 214 — `UpdateGateTests`, T-GATE-03 row only:** FixtureClaimNo = CLM-2026-0001, Status = GATE, pattern ??F?F, log∋"step(s) on hold". `chain-tests-71.json` synchronised (diff of exactly 6 fields; backup `.bak-before-gate03`). Expected after Mike clicks *Update T-GATE rows* → *Run gate tests*: T-GATE-03 = FFFPF (matches ??F?F); T-GATE-04 on the same claim becomes FFFPF (still matches ?F??F).

**Remaining:** Checkpoint queue reads Recommendation; the chain does not yet write TermCheckpoint (post-hackathon). The "x/4 gate" counter may have a fixed denominator of 4 — check after running.
