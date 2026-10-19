# Prompt lượt I4 — Seed_ChainTestFixtures (29 hồ sơ TST-*)

Turn I4 — Seed_ChainTestFixtures (Server Action). Idempotent: Start → delete every Claim whose ClaimNo starts with "TST-" together with its ClaimLine rows, its Assessment + Recommendation rows, and any TermCheckpoint rows; delete LimitLedger rows whose PolicyId belongs to a Policy whose PolicyNo starts with "FX-"; delete Member rows whose MemberNo starts with "FX-"; delete Policy rows whose PolicyNo starts with "FX-" (and their PolicyBenefitSnapshot rows). Then create, in this order:

A. Policies (Status = Entities.PolicyStatus.Active, PolicyHolderName "Fixture Co.", PremiumPaid as stated):
- FX-STD: PolicyCode "HC-STD", PolicyNo "FX-STD-2026", EffectiveDate 01/04/2026, ExpiryDate 31/03/2027, PremiumPaid True.
- FX-STD-UNPAID: PolicyCode "HC-STD", PolicyNo "FX-STD-UNPAID", same dates, PremiumPaid False.
- FX-PRM: PolicyCode "HC-PRM", PolicyNo "FX-PRM-2026", same dates, PremiumPaid True.
For each fixture policy copy the PolicyBenefitSnapshot rows of the existing demo policy with the same PolicyCode (aggregate PolicyBenefitSnapshot joined Policy where Policy.PolicyCode = <code> and Policy.PolicyNo does not start with "FX-", take the first demo PolicyId found): create one PolicyBenefitSnapshot per row with PolicyId = the fixture policy, SnapshotDate = CurrDate(), every other attribute copied verbatim (BenefitCode, BenefitName, ParentBenefitCode, Depth, LimitType, AnnualLimit, PerVisitCap, PerDayCap, PerEventCap, CountLimit, CountUnit, CopayPct, CoinsurancePct, DeductionPct, DeductionCondition, CapModeId, SumInsured, WaitingPeriodDays, EffectiveFrom, EffectiveTo). Do NOT invent any figure. Implement as a helper Seed_CopyPBS(FromPolicyId, ToPolicyId) with one loop.

B. Members (FullName "Fixture Member <n>", RelationshipToHolder "Self", CoverageTo NullDate(), RenewalStatusId = whatever the demo members use — read it):
- FX-M1: FX-STD, CoverageFrom 01/04/2026, ContinuousCoverFrom NullDate().
- FX-M2: FX-STD, CoverageFrom 01/09/2026.
- FX-M3: FX-STD-UNPAID, CoverageFrom 01/04/2026.
- FX-M4: FX-STD, CoverageFrom 01/04/2026, ContinuousCoverFrom 01/04/2025.
- FX-M5: FX-PRM, CoverageFrom 01/04/2026.

C. Claims — defaults unless a row says otherwise: MemberId FX-M1, PolicyId FX-STD, StatusId = Entities.ClaimStatus.Received, LossTypeId = Entities.LossType.IllnessOutpatient, TreatmentTypeId = Entities.TreatmentType.Outpatient, ConditionClassId = Entities.ConditionClass.<GEN record>, TreatmentFrom = TreatmentTo = EventDate = 14/07/2026, IcdCode "N39.0", Diagnosis "Fixture", FacilityName "Phòng khám Fixture", IsInNetwork True, RuleSetRef "A5-v1.5/HC-STD", CreatedOn CurrDateTime(), ClaimNo "TST-<TestId>", TotalClaimed = sum of its lines, TotalPayable 0. Lines: ServiceDate = TreatmentFrom, Description = "<Category> fixture", Quantity 1, Confidence 0.95, BenefitCode "" (step 5 maps), Category as given, AmountClaimed as given. Use a helper Seed_FixtureClaim(TestId, MemberId, PolicyId, LossTypeId, TreatmentTypeId, ConditionClassId, TreatmentFrom, IcdCode, RuleSetRef) → ClaimId and a helper Seed_FixtureLine(ClaimId, Category, Amount, ServiceDate). For loops of N lines use a ForEach over a counter.

| TestId | overrides | lines |
| T-CH01-a | — | CONSULT 300000 |
| T-CH01-b | TreatmentFrom/To/EventDate 01/04/2027 | CONSULT 300000 |
| T-CH01-c | Member FX-M3, Policy FX-STD-UNPAID | CONSULT 300000 |
| T-CH02-a | — | CONSULT 300000 |
| T-CH02-b | LossType DentalOutpatient, TreatmentType Dental, dates 20/06/2026 | DENTAL 500000 |
| T-CH02-c | ConditionClass PRE | CONSULT 300000 |
| T-CH02-d | Member FX-M2, dates 15/09/2026 | CONSULT 300000 |
| T-CH02-e | LossType IllnessInpatient, TreatmentType Inpatient, ConditionClass BIR | ROOM 1000000 |
| T-CH03-a | Member FX-M4, ConditionClass PRE | CONSULT 300000 |
| T-CH04-a | — | SUPPLEMENT 400000 |
| T-CH04-b | — | OPTICAL 800000 |
| T-CH04-c | LossType AccidentOutpatient, ConditionClass ACC | DENTAL 500000 |
| T-CH04-e | IcdCode "Z41.1" | SUPPLEMENT 400000 |
| T-CH05-a | — | CONSULT 300000 |
| T-CH05-b | LossType IllnessInpatient, TreatmentType Inpatient | CONSULT 300000 |
| T-CH05-c | LossType IllnessInpatient, TreatmentType Inpatient | DENTAL 500000 |
| T-CH05-d | — | OTHER 200000 |
| T-CH05-e | LossType AccidentInpatient, TreatmentType Inpatient, ConditionClass ACC | ROOM 1000000 |
| T-CH06-a | — | ADMIN 150000 |
| T-CH06-b | after lines: create Assessment for the claim (copy the field pattern used by Seed_AssessmentAndTerms for claim A, with sensible neutral values) and one Recommendation with ClaimLineId = the LAB line, Code "NOTREL", Decision "Confirmed", Title "LAB fixture — NOTREL", Rationale "fixture", Confidence 0.9, Band "HIGH", DecidedBy "fixture", DecidedOn CurrDateTime() | CONSULT 300000 · LAB 400000 |
| T-CH06-c | — | CONSULT 300000 · LAB 400000 · DRUG 500000 · ADMIN 150000 |
| T-CH07-a | — | CONSULT 300000 |
| T-CH08-a | — | CONSULT 300000 · LAB 1400000 · DRUG 900000 |
| T-CH08-b | — | PHYSIO 600000 × 12, ServiceDate 14/07/2026 + (i−1) days, i = 1..12; TreatmentTo 25/07/2026 |
| T-CH08-c | — | CONSULT 900000 · CONSULT 900000 (same ServiceDate) |
| T-CH09-a | after lines: create LimitLedger(MemberId FX-M1, PolicyId FX-STD, BenefitCode "200.3", PeriodYear 2026, UsedAmount 0, RemainingAmount 0, UsedCount 18, CountUnit "VISIT", UpdatedOn CurrDateTime()) | PHYSIO 600000 × 5, ServiceDate 14/07/2026 + (i−1) days; TreatmentTo 18/07/2026 |
| T-CH09-b | — | CONSULT 100000 × 40, ServiceDate 14/07/2026 + (i−1) days; TreatmentTo 22/08/2026 |
| T-CH09-c | LossType IllnessInpatient, TreatmentType Inpatient | ROOM 1000000 |

(T-CH04-d reuses TST-T-CH04-a — no claim of its own.)

Report: the record names you used for ConditionClass GEN/PRE/BIR/ACC and RenewalStatus, the number of Claim rows the action creates (must be 28), node sequence of Seed_ChainTestFixtures and the helpers, validation error count.
