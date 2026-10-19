# Prompt lượt M1 — T-CHAIN-06 (V-546 / BR-LEDGER-8) qua runner

Turn M1 — T-CHAIN-06 through the test runner. Nothing in A5Step01…14 or RunAdjudicationChain changes. Three pieces:

1. Entity ChainTest: add FixtureClaimNo2 Text 40 (empty default).

2. Seed_ChainTestFixtures — append a Part 6 block after Part 5 (same style; the "FX-"/"TST-" cleanup at the start already covers it):
   - Member FX-M8 on policy FX-STD (CoverageFrom 01/04/2026, same attributes as FX-M6), then LimitLedger(MemberId FX-M8, PolicyId FX-STD, BenefitCode "200", PeriodYear 2026, UsedAmount 19000000, RemainingAmount 1000000, UsedCount 0, CountUnit "", UpdatedOn CurrDateTime()).
   - Two claims, clones of the TST-T-CHAIN-05 header pattern (outpatient, IsInNetwork True, same LossType/TreatmentType/ConditionClass/provider/status, TreatmentFrom = TreatmentTo = ServiceDate = 14/07/2026) but member FX-M8 and NO Assessment/Recommendation/TermCheckpoint rows: ClaimNo "TST-T-CHAIN-06a" and "TST-T-CHAIN-06b", each with exactly one ClaimLine: Category "CONSULT", Description "CONSULT fixture", AmountClaimed 800000, Quantity 1, Confidence 0.95, ServiceDate 14/07/2026, BenefitCode "200.1". TotalClaimed 800000.

3. RunChainTests — add Switch branch Mode = "CONCURRENT" (converging on the same single Join as the other branches). It resolves a second claim: aggregate Claim where ClaimNo = ChainTest.FixtureClaimNo2 (max 1); if empty → same NOFIX update as the first claim ("Không có hồ sơ fixture " + FixtureClaimNo2). Else call helper ChainTest_Concurrent(ClaimId1 = GetClaim.Id, ClaimId2) → outputs TotalA1, TotalB1, TotalB2, RunNoB1, RunNoB2, UsedBefore, UsedAfter, ReversedB1 Integer, ProvisionalB2 Integer, Log Text.
   Helper body (no loop over Ctx structures; may loop LedgerPosting aggregates):
   a. Reset: call A5Ledger_Reverse(ClaimId1) and A5Ledger_Reverse(ClaimId2) (they only touch FINAL postings — idempotent).
   b. Read UsedBefore = LimitLedger.UsedAmount where MemberId = Claim1.MemberId and PolicyId = Claim1.PolicyId and BenefitCode = "200" and PeriodYear = Year(Claim1.TreatmentFrom) (0 if none).
   c. RunAdjudicationChain(ClaimId1, "test") → TotalA1.
   d. RunAdjudicationChain(ClaimId2, "test") → TotalB1, ChainRunIdB1; RunNoB1 = that ChainRun's RunNo.
   e. A5Ledger_Finalize(ClaimId1). Read UsedAfter the same way as b.
   f. RunAdjudicationChain(ClaimId2, "test") → TotalB2, ChainRunIdB2, RunNoB2.
   g. ReversedB1 = count of LedgerPosting where ClaimId = ClaimId2 and ChainRunId = ChainRunIdB1 and Status = "REVERSED"; also PendingB1 = count with the same ChainRunId and Status = "PROVISIONAL" (must be 0). ProvisionalB2 = count where ChainRunId = ChainRunIdB2 and Status = "PROVISIONAL".
   h. Teardown: A5Ledger_Reverse(ClaimId1) so the ledger returns to the seeded value for the next run.
   i. Log = "hồ sơ 1 = " + TotalA1 + " → FINAL, ledger 200 " + UsedBefore + " → " + UsedAfter + "; hồ sơ 2 run " + RunNoB1 + " = " + TotalB1 + "; run " + RunNoB2 + " = " + TotalB2 + " (còn " + (20000000 − UsedAfter) + "); posting run " + RunNoB1 + " đảo " + ReversedB1 + ", còn tạm " + PendingB1 + "; run " + RunNoB2 + " tạm " + ProvisionalB2.
   Back in the runner: Pass = (TotalB1 = AssertAmountBefore) and (TotalB2 = AssertAmount) and (RunNoB2 = RunNoB1 + 1) and (PendingB1 = 0) and (ProvisionalB2 > 0); LastOutcome = "MOVED", LastAmount = TotalB2, LastLog = Log prefixed "[FAIL: <failing condition>] " when not Pass (same style as the other modes).

4. Seed_ChainTests: row T-CHAIN-06 → Status "RUNNABLE", Mode "CONCURRENT", FixtureClaimNo "TST-T-CHAIN-06a", FixtureClaimNo2 "TST-T-CHAIN-06b", AssertStep 14, AssertOutcome "", AssertAmountSet True, AssertAmountBefore 640000, AssertAmount 360000, AssertLogContains "", AssertLogNotContains "". ExpectedText/InputText unchanged (verbatim).

Report: node sequence of ChainTest_Concurrent, the Switch wiring, Part 6 rows, validation error count.
