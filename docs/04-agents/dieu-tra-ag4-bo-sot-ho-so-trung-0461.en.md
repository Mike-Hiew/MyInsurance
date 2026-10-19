# AG-4 missed duplicate claim CLM-2026-0461 — investigation and fix (09/10/2026)

> English translation for reviewers. Vietnamese original: [dieu-tra-ag4-bo-sot-ho-so-trung-0461.md](dieu-tra-ag4-bo-sot-ho-so-trung-0461.md).

## Symptom
- Submitted sample set 02 (duplicate claim) through MyInsurance → CLM-2026-0461 duplicates CLM-2026-0460 on person, facility (Vinmec) and date 08/10/2026.
- AG-4 returned "No signal" → gate 4/4 → auto-approved, Paid 320.000 ₫ (Development, simulated payment order).

## Cause (verified with AgentTrace + app_logs)
- The tool `Agent_FindDuplicateClaims` correctly returned `found=True; CLM-2026-0460`.
- AG-4 (CLM_FraudSignalAgent rev 6) called all 5 read tools and then called `Agent_SetFraudCheckDone`, **without calling `RaiseFraudSignal`**. The ODC log confirms there was no such call.
- The agent has not changed since 04/10; in the same situation for CLM-0451 (05/10) AG-4 did record DUPLICATE. → A model instability bug, not a tool bug.

## Fix (the owner chose "both")
1. **Hard stop — ClaimMVP rev 484:** service action `Agent_FindDuplicateClaims`, after `FindDuplicateClaims`: if `DuplicateFound` → call `RaiseFraudSignal(DUPLICATE, HIGH, EvidenceCsv = duplicate claim numbers, ExplanationVi …)`, and only then `WriteAgentRun`. Independent of the model.
2. **Tighten the prompt — CLM_FraudSignalAgent rev 7:** add a MANDATORY RULES block at the end of the system prompt (BuildMessages): if found=True, RaiseFraudSignal DUPLICATE/HIGH is mandatory, even when the earlier claim has already been paid; for date violations, DATE_IMPOSSIBLE; do not call SetFraudCheckDone before all signals are recorded; the final answer must list the signals.

## Verification
- CLM-2026-0462 (set 02 once more): the tool saw duplicates 0460 + 0461; AG-4 recorded DUPLICATE/HIGH itself (SignalId 22) → "Auto-approval blocked — AG-4 signal", gate 3/4, no payment order, routed to an approver.
- Chain tests 46/46, gate 5/5 on rev 484.
- The hard stop was not observed separately: in this run AG-4 also recorded the signal itself, so the same-type signal was overwritten by an update (RaiseFraudSignal upserts by type).

## Remaining
- CLM-0461 is still in Paid status despite being a duplicate (Development data). The PO needs to decide: leave it as evidence of the old bug, or reopen the signal with a data-fix tool.
- Rollback: ClaimMVP rev 483, CLM_FraudSignalAgent rev 6.
