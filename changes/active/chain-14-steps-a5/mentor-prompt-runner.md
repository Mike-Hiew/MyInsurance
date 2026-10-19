# Prompt lượt I5 — RunChainTests + màn ChainTests

Turn I5 — test runner and the 71/71 screen. Nothing touches the chain steps.

1. Server Action RunChainTests (out: RunCount, PassCount, FailCount Integer). Aggregate ChainTest where Status = "RUNNABLE", sorted by SortNo. ForEachTest (one loop):
   - GetClaim: aggregate Claim where ClaimNo = ChainTest.FixtureClaimNo (max 1). If empty → UpdateChainTest with Pass = False, LastOutcome = "NOFIX", LastLog = "Không có hồ sơ fixture " + FixtureClaimNo, LastRunAt = CurrDateTime() → Join.
   - Else call A5Chain_DryRun(ClaimId) → take StepResults. Use a helper ChainTest_FindStep (in: StepResults list, StepNo; out: Found, Outcome, ResultAmount, StepLog) — one flat loop, like A5Step04_FindCheckpoint.
   - Evaluate with a helper ChainTest_Assert (in: AssertOutcome, AssertAmountSet, AssertAmount, AssertLogContains, AssertLogNotContains, Outcome, ResultAmount, StepLog; out: Pass Boolean, Reason Text). Logic: Pass = Found and (AssertOutcome = "" or Outcome = AssertOutcome) and (not AssertAmountSet or ResultAmount = AssertAmount) and every "||"-separated non-empty fragment of AssertLogContains has Index(StepLog, fragment) <> -1 and (AssertLogNotContains = "" or Index(StepLog, AssertLogNotContains) = -1). Implement the "||" split without a list: loop with Index/Substr on a working copy of the text (a While-style loop is fine; if only a ForEach exists, split into at most 4 fragments with Index calls). Reason lists which condition failed ("outcome CUT ≠ HOLD", "amount 2500000 ≠ 2400000", "log thiếu «cắt 200000»", "log có «100.1»").
   - UpdateChainTest: LastOutcome = Outcome, LastAmount = ResultAmount, LastLog = Substr(StepLog, 0, 2000), Pass, LastRunAt = CurrDateTime(); if not Pass append Reason at the start of LastLog as "[FAIL: …] ".
   - Counters RunCount/PassCount/FailCount.
   No other entity is written by RunChainTests. (The helper calls A5Chain_DryRun which writes nothing.)

2. Screen ChainTests (MainFlow, layout and menu style of Administration; add a side-menu entry "Chain tests" under "Technical trace" in the TECHNICAL HAND-OVER group). Content:
   - Header card: title "Chuỗi 14 bước — bộ test 71 (build-04 §9)"; four counters from aggregates on ChainTest: "{PassCount}/{RunnableCount} pass" · "{FailCount} fail" · "{PendingCount} chờ bước" · "{ManualCount} ngoài chuỗi"; a line "Mốc: 71/71"; last run time = max LastRunAt.
   - Buttons: "Seed fixtures" (client action → Seed_ChainTestFixtures → refresh), "Run runnable tests" (→ RunChainTests → refresh, toast "{PassCount}/{RunCount} pass"), "Re-seed 71 rows" (→ Seed_ChainTests → refresh).
   - Table of all ChainTest rows sorted by SortNo, columns: TestId · Bước (StepNo) · Input (InputText) · Kỳ vọng (ExpectedText, nguyên văn) · Assertion (text built as: "bước " + AssertStep + If(AssertOutcome <> "", " " + AssertOutcome, "") + If(AssertAmountSet, " = " + FormatDecimal(AssertAmount,0,".",""), "") + If(AssertLogContains <> "", " log∋" + AssertLogContains, "") + If(AssertScope = "PARTIAL", " (partial)", "")) · Kết quả: icon/text "✓" green when Pass and LastRunAt <> NullDate(); "✗" red when not Pass and LastRunAt <> NullDate() and Status = "RUNNABLE"; "–" grey otherwise, with Status shown small · Actual (LastOutcome + " · " + FormatDecimal(LastAmount,0,".","")) · a collapsible "log" (LastLog in a pre-wrap expression, hidden by default, toggled per row).
   - Phone width must not break (Administration already handles this — reuse its patterns).

Report: node sequences of RunChainTests and the two helpers, the screen's widget tree summary, the menu entry, validation error count.
