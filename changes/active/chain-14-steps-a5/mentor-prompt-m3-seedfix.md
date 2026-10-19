# Prompt lượt M3 — seed cleanup thật + reset NOTREL + AssertAmountBefore T-CHAIN-06

Turn M3 — three harness fixes found at runtime on rev 159. No chain step changes.

1. Seed_ChainTestFixtures cleanup does not delete anything: after four "Seed fixtures" clicks the Work list has 4 copies of every TST-T-CH01…09 claim (Ids 67–94, 95–122, 133–160, 172–199), 3 of Part 4, 2 of Part 5 (168–171, 207–210). First inspect the existing cleanup block at the start of Seed_ChainTestFixtures and report why it is a no-op (wrong filter? aggregate Max Records? a delete that raises on a foreign key and is swallowed? a branch never reached?). Then make it work: aggregate Claim where ClaimNo starts with "TST-" (Max Records 5000) → one loop; per claim delete, in this order, the rows referencing it: LedgerPosting, CalcSnapshot, ChainRun, TermCheckpoint, Recommendation, Assessment, ClaimLine, ClaimDocument (if such rows exist), then the Claim. Then LimitLedger rows whose MemberId belongs to a Member whose MemberNo starts with "FX-"; then Member "FX-"; then PolicyBenefitSnapshot of Policy "FX-"; then Policy "FX-". Use helper actions for the per-claim child deletes so no action has nested loops over the same list. The fixture creation parts (1–7) stay as they are.

2. ChainTest_ConfirmRerun: before the first RunAdjudicationChain, reset every Recommendation of the claim with Code = "NOTREL" to Decision = "Proposed" (DecidedBy "", DecidedOn NullDate()) so the test is idempotent regardless of seed state. Keep everything else.

3. Seed_ChainTests: the T-CHAIN-06 row currently carries AssertAmountBefore = 2000000 (the runtime reported "[FAIL: TotalB1 640000 ≠ 2000000]"). Set AssertAmountBefore = 640000, AssertAmount = 360000 on that row and report the row's full field list as seeded. Also confirm T-CHAIN-05 keeps AssertAmountBefore 2000000 / AssertAmount 1880000.

Report: the cleanup root cause, the new delete sequence, the two row value sets, validation error count.
