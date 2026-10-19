# Spec — Adjudication chain & seed

> English translation for reviewers. Vietnamese original: [spec.md](spec.md).

> **Full text:** Project → `claude/build-04-chuoi-tinh-tien-va-seed` (~101 KB).
> **Baseline standard: A5 §6.1 + D1 §5.3.** Wherever the hackathon BRD (`CH-01…CH-14`) departs from A5, **A5 wins and the BRD must be corrected.**

## Invariants

1. **14 steps, one Server Action each.** The chain is deterministic: same input → same output, running it twice gives the same result.
2. **Rounding: to the nearest đồng, at EACH STEP.** The BRD says "round down to 1.000 ₫ in a separate step" — **wrong**.
3. **Never patch the amount. Re-run the chain.** A before/after comparison is between **two real runs**, not `confirmed ? P2 : P1`.
4. **A clinical recommendation stays `Proposed` until a person has `Confirmed`** it, with identity and timestamp. Only then does the step that applies it to the money run.
5. **Stale-figures indicator:** compare the input hash; on mismatch → banner + **lock the browser button**.

## Four places where the BRD departs from A5 (all change the money)

| # | BRD | A5 |
|---|---|---|
| 1 | Has a social-insurance (BHYT) deduction step | **Removed** |
| 2 | Has a Deductible step | **The product does not have one** |
| 3 | Round down to 1.000 ₫ in a separate step | Whole đồng, at each step |
| 4 | Merges away 2 term-engine steps | Both kept |

Detailed comparison table: `build-04` §1.1.

## Auto-approval gate

**4 conditions** — PO fixed 21/09. A5 §6.4 has 3; BRD §9.1.3 adds G4 (fraud, because the hackathon slice has AG-4); the set to implement is the **union = 4**, and the Approval screen was built accordingly. Content of the 4 conditions: `build-04` §3.1. **STILL OPEN:** the screen and the G1–G4 table list two different sets that both count to 4 — read `EvaluateAutoApprovalGate` and take the code as the standard.
→ See `changes/active/approval-gate-4-conditions/`.

## Seed

51 benefits + 51 waiting periods × 3 plans (HC-STD / HC-PLS / HC-PRM, 17 benefit items each) = **215 records**. `build-04` §5–§6.

## Test suite

**65 unit tests** — `build-04` §9. This is the only part of the system that already has a test suite, so it is the only part that is measurable right now.

## The numbers that must come out right in scenario B

~~`2.000.000 → 1.880.000` · line 4 `NOTCOV −80.000` · line 2 `CAP_VISIT −40.000`~~ — **copied in error, found 21/09** (C-PREP-2): this pair of numbers cannot be produced from A5 on any inpatient claim; 1.880.000 is prototype claim A, **outpatient HC-STD, 6 lines, total 3.128.000**. The correct target, hand-calculated to match A5 to the last đồng (`changes/active/verify-entity-vs-build01/batchB2-spec.md` §Scenario B number check, path C):

**`2.000.000 → 1.880.000` · line 4 `NOTREL −168.000` · `CAP_VISIT` 628.000 → 610.000 · `COPAY` 500.000 → 470.000.** Three kinds of deduction, all within the 8 causeCodes. **build-00 §5 conflict 5 (Project doc) states "2.000.000 → 1.880.000 inpatient, no co-pay", which is wrong — it must be corrected to match this line** (cannot be fixed from disk yet; PO to fix on claude.ai).

## Four cause codes without wording

`CAP_VISIT` · `COPAY` · `DEDUCT` · `LIMIT_TOTAL` — no sentence for them in A2. Co-payment applies to almost every claim ⇒ the `"Vì sao Quý khách tự chịu"` (EN: "Why you pay this yourself") block will be empty in exactly the most common group. `build-06` §9 gap 1.
