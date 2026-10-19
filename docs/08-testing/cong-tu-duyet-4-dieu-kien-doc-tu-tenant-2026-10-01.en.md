# Auto-approval gate — "WHICH 4 conditions?" · read from the tenant 01/10/2026

> English translation for reviewers. Vietnamese original: [cong-tu-duyet-4-dieu-kien-doc-tu-tenant-2026-10-01.md](cong-tu-duyet-4-dieu-kien-doc-tu-tenant-2026-10-01.md).

ClaimMVP **rev 199**. Closes open item (d) of `trang-thai-build-2026-09-20` and `approval-gate-4-conditions` 1b.

> ## ✅ PO DECISION 01/10 — Option A: take the code (current screen) as the standard — for the **list**
>
> **C1** Payable ≤ threshold (2.000.000 ₫, editable by admin) **and nothing cut** — A5 §6.4 cond.1
> **C2** Clinical assessment complete, **no** recommendation pending — A5 §6.4 cond.2
> **C3** No term checkpoint open — A5 §6.4 cond.3
> **C4** No fraud / duplicate signal — BRD hackathon G4 (agent **AG-4**, not "AG-6")
>
> **Confidence / extraction quality (old G2) is NOT part of the gate** — per A4 §7 / D1 §5.8; it belongs to the **pre-submission checks + assessor routing**.
>
> ⚠️ "Take the code as the standard" holds only for the **list of 4 conditions**. The **formulas** on the screen (§6) are wrong in all 4 — 2 of the 4 are decoration. The existing expressions must not be used as the specification for the server action.

## 1. There is no `EvaluateAutoApprovalGate`

- `context_actions` on ClaimMVP (index of 30/09 11:22 UTC only): **146 actions, none** related to the gate. Positive control: `RunAdjudicationChain` is found by the same query.
- The gate is **computed only on the `ClaimDetail` screen** (block `gateCard`, tab *Approval & payment*). The 7 `T-GATE` tests have nothing to call; a real auto-approval path does not exist. The gate is currently **display**, not yet **logic**.

## 2. The four conditions as the screen shows them — claim `CLM-2026-0001` (Id 17)

*"Auto-approval conditions · not met — needs a person"*

| # | Label | Claim 17 | Reason shown on screen |
|---|---|---|---|
| 1 | Payable ≤ 2,000,000 ₫ and nothing cut | ✕ | Per-visit cap bound at step 8 — 200.1 cuts 610.000; 200.2 cuts 18.000 |
| 2 | Clinical assessment complete, no recommendation | ✕ | 1 proposal awaiting a human |
| 3 | No open term checkpoint | ✓ | 6 terms evaluated at step 4, no checkpoint open |
| 4 | No duplicate / fraud signal (AG-6) | ✓ | No claim with same insured + provider + date in 90 days · hospital_code matched via OpenSearch table 322 |

## 3. Comparison with `build-04` §3.1 (before the decision)

Screen = **A5 §6.4 three conditions + BRD G4**; `build-04` = **BRD G1–G4** (merges A5 cond.2+3 into G3, adds G2 confidence). G2 contradicts `build-01` §4.14 (A4 §7, D1 §5.8). Project rule: *BRD hackathon deviates from A5/D2 → A5/D2 wins* ⇒ option A.

## 6. The real formulas on the screen — Mentor reads model rev 199 (01/10, read-only, no changes)

Source: Mentor session "Claim auto-approval conditions audit". *This is Mentor reading the model, not yet observed behaviour — but it is consistent with everything the screen shows.*

### C1 — computed from data, 3 bugs

- ✓/✕: `GetCalcRun…ChainRun.TotalPayable <= 2000000 and GetWaitStep.List.Length = 0 and GetCutSteps.List.Length = 0`
- `GetCalcRun`: ChainRun, `ClaimId = ClaimId`, sort `-RunNo`, Max 1 (latest run).
- `GetWaitStep`: CalcSnapshot, `ClaimId = ClaimId and ReasonCode = "WAIT"`, Max 1.
- `GetCutSteps`: CalcSnapshot, `ClaimId = ClaimId and Outcome = "CUT"`.
- Reason: computed (`CalcSnapshot.Detail`, `FormatVND(TotalPayable)`); the WAIT branch is the literal *"Both lines declined (WAIT) — lines were cut"*.

**Bugs:** (a) **the 2.000.000 threshold is a literal inside the expression** — the table footer and the Administration screen say "admin-editable" but the gate does not read it anywhere; (b) WAIT/CUT are filtered by **ClaimId**, not by the **latest run** → snapshots of old runs still make C1 fail (CalcSnapshot currently has 144 rows, several runs per claim); (c) "nothing cut" only considers WAIT and CUT — lines excluded for **NOTREL / NOTCOV / EXCL** do not make C1 fail, even though the ✓ reason says *"no line declined"*.

### C2 — computed from data, 2 suspicious points

- ✓/✕: `Assessment.State = "COMPLETE" and RecommendationCount = 0 and LinesAssessed = LinesTotal and not(GetRecommendation.List.Length > 0 and IsRecPending(…Decision))`
- `GetAssessment`: Assessment, `ClaimId = ClaimId`, sort `AssessedOn`, Max 1 — **sort direction unclear**: if ascending, it reads the **oldest** assessment.
- `GetRecommendation`: Recommendation ⋈ Assessment ⋈ ClaimLine, `Assessment.ClaimId = ClaimId`, **Max 1** — only **one** recommendation is considered.
- Reason: computed (`LinesAssessed`, `LinesTotal`, `RecommendationCount`).
- Note: `RecommendationCount = 0` means a recommendation that has been Confirmed/Rejected still makes C2 fail forever — it must be decided: "no recommendation at all" or "no recommendation **pending**" (A5 §6.4 / build-04 G3 say *Status = Proposed*).

### C3 — 🔴 DECORATION

- The **✓ character is a static literal** (`G3CkText.Text = "✓"`). Only the **CSS class** changes according to `GetWaitStep.List.Length = 0` — i.e. according to the **waiting period (step 2)**, not the checkpoint (step 4).
- It **does not read `TermCheckpoint`** — the entity in which `A5Step04_GeneralExclusions` writes `Status = "OPEN"`.
- Reason: **literal** in both branches — *"Clause 5.2 waiting period — checkpoint open, approver needed"* / *"6 terms evaluated at step 4, no checkpoint open"*.
- ⇒ Explains the contradiction on claim 17: B04 records checkpoint TERM-HC-003 as open, yet C3 is still ✓ because it does not look there.

### C4 — 🔴 DECORATION

- **Hard-coded** style `"cc-ck cc-ok"` — **always ✓**, no `If`, no binding.
- **No aggregate, no variable, no data action.**
- Reason: **literal** — *"No claim with same insured + provider + date in 90 days · hospital_code matched via OpenSearch table 322"*. No duplicate check runs; table 322 is not wired up.

### Consequence for the demo

The gate screen is the one judges will open. Currently **C3 and C4 are always ✓ regardless of the data**, and claim to have checked things they do not check. If a judge asks "if the data changes, do C3/C4 change?" — no. This is a bigger credibility risk than any numeric bug.

## 4. Other oddities on the same screen — still open

- The label **"AG-6"** → **AG-4** (`build-01` §4.27).
- The card **"RUN #2 (AFTER CONFIRMATION) 0 ₫"** is still shown — `F-CH-1` (risk #11 from 21/09), **not yet fixed**.

## 5. Next step — specification for `EvaluateAutoApprovalGate(ClaimId)`

Output: `C1..C4 {Pass: Boolean, Reason: Text}` + `AllPass`. **Every Reason is generated from data.** One Mentor session for the action, one for the screen.

| | What is correct |
|---|---|
| C1 | `TotalPayable` of the **latest ChainRun** ≤ **threshold read from wherever the Administration screen stores it** (find it first — no literal) **and** no CalcSnapshot **of that run** has a cut/excluded Outcome (CUT, WAIT, NOTREL, NOTCOV, EXCL — list according to the real enum) |
| C2 | The **latest** Assessment has `State = COMPLETE` and `LinesAssessed = LinesTotal` and **no** Recommendation has `Decision = Proposed` (count all, not Max 1) |
| C3 | No `TermCheckpoint` with `ClaimId = ClaimId and Status = "OPEN"` |
| C4 | No other Claim with the same insured person + same facility + treatment date within ±90 days (a real aggregate). The reason states exactly the check that ran — **do not mention table 322** until it is wired up |

After that: the `ClaimDetail` screen only displays the action's output · rename the label to AG-4 · 7 `T-GATE` tests per C1–C4 · fix `build-04` §3.1 at the source.
