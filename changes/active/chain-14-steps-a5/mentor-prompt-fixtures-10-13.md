# Prompt lượt I6 — fixtures + assertion cho test bước 10–13 (sau lượt J)

Turn I6 — fixtures and assertions for the step 10–13 tests. Two parts.

Part A — Seed_ChainTestFixtures: add (in a new sub-action Seed_ChainTestFixtures_Part4, called at the end of the main action, same helpers Seed_FixtureClaim / Seed_FixtureLine, same defaults as before: FX-M1/FX-STD, ILL_OP, Outpatient, GEN, 14/07/2026, N39.0, "Phòng khám Fixture", IsInNetwork True, RuleSetRef "A5-v1.5/HC-STD"):
- Members: FX-M6 (FX-STD, CoverageFrom 01/04/2026) and FX-M7 (FX-STD, CoverageFrom 01/04/2026), created in the member block of the main action; the "FX-" cleanup already deletes them.
- LimitLedger rows (create after the members): (FX-M6, FX-STD, BenefitCode "200", PeriodYear 2026, UsedAmount 19000000, RemainingAmount 1000000, UsedCount 0, CountUnit "", UpdatedOn CurrDateTime()) and the same for FX-M7.
| TestId | overrides | lines |
| T-CH10-a | — | CONSULT 300000 · LAB 1400000 · DRUG 900000 |
| T-CH10-b | Member FX-M5, Policy FX-PRM | CONSULT 2400000 |
| T-CH10-c | LossType DentalOutpatient, TreatmentType Dental | DENTAL 500000 |
| T-CH11-b | IsInNetwork False | CONSULT 300000 · LAB 1400000 · DRUG 900000 |
| T-CH12-a | Member FX-M6 | CONSULT 300000 · LAB 1400000 · DRUG 900000 |
| T-CH12-b | Member FX-M7; TreatmentTo 17/07/2026 | CONSULT 400000 ×4, ServiceDate 14/07, 15/07, 16/07, 17/07/2026 |
| T-CH12-c | LossType IllnessInpatient, TreatmentType Inpatient | ICU 2000000 |
(T-CH11-a and T-CH13-a reuse TST-T-CH01-a — no claim of their own.) Total Claim rows must become 28 + 7 = 35. Report the count.

Part B — Seed_ChainTests_B / _C: update these rows (keep InputText/ExpectedText/CatchText untouched — they are verbatim spec):
| TestId | Status | FixtureClaimNo | AssertStep | AssertOutcome | AssertAmountSet/AssertAmount | AssertLogContains | AssertLogNotContains | AssertScope |
| T-CH10-a | RUNNABLE | TST-T-CH10-a | 10 | CUT | True / 1920000 | 20% | | FULL |
| T-CH10-b | RUNNABLE | TST-T-CH10-b | 10 | CUT | True / 2160000 | 10% | 20% | FULL |
| T-CH10-c | RUNNABLE | TST-T-CH10-c | 10 | CUT | True / 350000 | 30%||từ 300 | | FULL |
| T-CH11-a | RUNNABLE | TST-T-CH01-a | 11 | SKIP | False | KHÔNG_ÁP_DỤNG | | FULL |
| T-CH11-b | RUNNABLE | TST-T-CH11-b | 11 | PASS | True / 1920000 | quy tắc tạm B-19 | | FULL |
| T-CH11-c | MANUAL | | 11 | | False | | | — (AssertNote "MVP: một claim = một insured event; không có provider master — ngoài phạm vi chuỗi") |
| T-CH11-d | MANUAL | | 11 | | False | | | — (AssertNote "Không có provider master trong MVP; IsInNetwork là fact duy nhất") |
| T-CH12-a | RUNNABLE | TST-T-CH12-a | 12 | CUT | True / 1000000 | 200: ||còn 1000000||cắt 920000 | | FULL |
| T-CH12-b | RUNNABLE | TST-T-CH12-b | 12 | CUT | True / 1000000 | cắt 280000 | | PARTIAL (AssertNote "Thứ tự B-20 chỉ kiểm qua số tiền; 'ghi lên claim' thuộc bước 14") |
| T-CH12-c | RUNNABLE | TST-T-CH12-c | 12 | | False | 100: | 100.1: | PARTIAL (AssertNote "Dòng HOLD ở bước 9 (DAY, Q-IDP-06) nhưng nút hạn mức năm vẫn phải là 100") |
| T-CH13-a | RUNNABLE | TST-T-CH01-a | 13 | SKIP | False | KHÔNG_ÁP_DỤNG | | FULL |

Part C — A5Step12_FindAnnualNode currently returns RootBenefitCode directly. Rewrite it per spec: inputs (Snapshot list, BenefitCode, ParentBenefitCode, RootBenefitCode) → outputs (NodeCode, AnnualLimit); one loop over the snapshot list capturing, for each of the three codes, whether its row has CapModeId = Entities.CapMode.Own and LimitType = "ANNUAL" and AnnualLimit > 0 (and that AnnualLimit); after the loop pick the first of [BenefitCode, ParentBenefitCode, RootBenefitCode] that qualifies; if none qualifies NodeCode = "" and AnnualLimit = 0 (pass 1 then skips the line — log "không có nút hạn mức năm"). Never infer "included" from AnnualLimit = 0 — read CapModeId. Keep pass 1 / pass 2 of A5Step12_ParentAnnualCap unchanged apart from calling the rewritten helper.

Report: the rows changed, the total number of RUNNABLE rows (must be 39), Claim rows created (35), the new FindAnnualNode node sequence, validation error count.
