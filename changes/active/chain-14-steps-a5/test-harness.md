# Bộ khung test 71 — thiết kế (21/09, PO: dựng NGAY, chạy song song với bước 10–14)

> **Luật:** cột "Kỳ vọng (nguyên văn)" chép từ `expected-71.md`; **assertion** là phần máy kiểm được của kỳ vọng đó (bước, outcome, số tiền, chuỗi log). Không assertion nào lấy từ output code. Phần kỳ vọng ngoài chuỗi (F48 render, cổng G1–G4, ledger post) ghi `AssertScope = PARTIAL` và nêu phần chưa kiểm.

## 1. Model

**Entity `ChainTest`** (public): `TestId` Text 20 (khoá nghiệp vụ, vd `T-CH01-a`) · `Section` Text 5 (`9.1`…`9.4`) · `StepNo` Int (0 = toàn chuỗi/cổng/seed) · `InputText` Text 500 (cột "Input tối thiểu" nguyên văn) · `ExpectedText` Text 1000 (cột "Output kỳ vọng" nguyên văn) · `CatchText` Text 300 (cột "Bắt lỗi gì") · `Status` Text 12 (`RUNNABLE` = có fixture và bước đã cài · `PENDING` = bước chưa cài · `MANUAL` = ngoài chuỗi) · `FixtureClaimNo` Text 30 (ClaimNo hồ sơ fixture, `TST-…`) · `AssertStep` Int · `AssertOutcome` Text 10 · `AssertAmountSet` Bool · `AssertAmount` Decimal · `AssertLogContains` Text 200 (nhiều mảnh cách nhau `||`, tất cả phải có) · `AssertLogNotContains` Text 100 · `AssertScope` Text 10 (`FULL`/`PARTIAL`) · `AssertNote` Text 300 · `LastOutcome` Text 10 · `LastAmount` Decimal · `LastLog` Text 2000 · `Pass` Bool · `LastRunAt` DateTime · `SortNo` Int.

**`A5Chain_DryRun`** thêm output `StepResults` List<ChainStepResult> (Structure: `StepNo`, `Outcome`, `ResultAmount`, `StepLog`) — đang ghép chuỗi text; runner cần dữ liệu có cấu trúc.

**`Seed_ChainTests`**: xoá hết `ChainTest`, tạo 71 dòng (text nguyên văn) — chạy từ `RunSeedBatch0Now` (cuối) và từ nút riêng. **`Seed_ChainTestFixtures`**: xoá mọi Claim có `ClaimNo` bắt đầu `TST-` (kèm ClaimLine, Assessment/Recommendation, LimitLedger fixture, TermCheckpoint fixture), rồi tạo lại theo §2. Fixture policy/member: `PolicyNo` bắt đầu `FX-`; PBS của fixture policy = **copy 17 dòng** từ policy demo cùng PlanCode (không tự chế số).

**`RunChainTests`** (Server Action, ghi Entity `ChainTest` — không phải tiền): với mỗi test `RUNNABLE`: tìm Claim theo `FixtureClaimNo` → `A5Chain_DryRun` → lấy StepResult của `AssertStep` → Pass = (AssertOutcome = "" or Outcome = AssertOutcome) and (not AssertAmountSet or ResultAmount = AssertAmount) and mọi mảnh `AssertLogContains` có trong StepLog and (AssertLogNotContains = "" or không có). Ghi LastOutcome/LastAmount/LastLog/Pass/LastRunAt.

**Màn `ChainTests`** (menu Technical hand-over, dưới Technical trace): header "{pass}/{runnable} pass · {fail} fail · {pending} chờ bước · {manual} ngoài chuỗi · mốc 71/71"; nút "Seed fixtures", "Run runnable tests", "Delete fixtures" (trước demo); bảng 71 dòng: TestId · Bước · Kỳ vọng (nguyên văn) · Assertion · Kết quả (✓/✗/–) · Actual (outcome · amount) · log (expand).

## 2. Fixture chung

| Đối tượng | Giá trị |
|---|---|
| Policy `FX-STD` | PolicyCode `HC-STD`, PolicyNo `FX-STD-2026`, Effective **01/04/2026**, Expiry **31/03/2027**, Status Active, PremiumPaid **True**, holder "Fixture Co." — PBS copy từ policy demo HC-STD |
| Policy `FX-STD-UNPAID` | như trên, PolicyNo `FX-STD-UNPAID`, PremiumPaid **False** |
| Policy `FX-PRM` | PolicyCode `HC-PRM`, PolicyNo `FX-PRM-2026`, cùng ngày — PBS copy từ policy demo HC-PRM (dùng T-CH10-b) |
| Member `FX-M1` | FX-STD, CoverageFrom **01/04/2026**, ContinuousCoverFrom null, MemberNo `FX-M1` |
| Member `FX-M2` | FX-STD, CoverageFrom **01/09/2026** (T-CH02-d) |
| Member `FX-M3` | FX-STD-UNPAID, CoverageFrom 01/04/2026 (T-CH01-c) |
| Member `FX-M4` | FX-STD, CoverageFrom 01/04/2026, **ContinuousCoverFrom 01/04/2025** (tái tục, T-CH03-a) |
| Member `FX-M5` | FX-PRM, CoverageFrom 01/04/2026 |
| Claim mặc định | Status Received, ClaimNo `TST-<TestId>`, LossType `ILL_OP`, TreatmentType Outpatient, ConditionClass `GEN`, TreatmentFrom = EventDate = **14/07/2026**, IcdCode `N39.0`, FacilityName "Phòng khám Fixture", IsInNetwork True, RuleSetRef `A5-v1.5/HC-STD` (FX-PRM: `A5-v1.5/HC-PRM`), TotalClaimed = Σ dòng; dòng mặc định ServiceDate = TreatmentFrom |

## 3. 29 test chạy được (bước 1–9) — fixture + assertion

| TestId | Fixture khác mặc định | Dòng (Category · số tiền · ngày) | AssertStep | Outcome | Amount | LogContains | Scope / ghi chú |
|---|---|---|---|---|---|---|---|
| T-CH01-a | FX-M1 | CONSULT 300.000 | 1 | PASS | | `hiệu lực` | FULL |
| T-CH01-b | FX-M1, TreatmentFrom **01/04/2027** | CONSULT 300.000 | 1 | FAIL | | `PERIOD` | PARTIAL — "không auto-approve" thuộc cổng |
| T-CH01-c | FX-M3 (policy UNPAID) | CONSULT 300.000 | 1 | FAIL | | `phí` | FULL |
| T-CH02-a | FX-M1 | CONSULT 300.000 | 2 | PASS | | `30 ngày` | FULL |
| T-CH02-b | LossType **DEN_OP** (nhóm 300), TreatmentType Dental, TreatmentFrom **20/06/2026** | DENTAL 500.000 | 2 | FAIL | | `90 ngày` ‖ `01/04/2026` ‖ `20/06/2026` | FULL |
| T-CH02-c | ConditionClass **PRE** | CONSULT 300.000 | 2 | FAIL | | `365` | FULL |
| T-CH02-d | **FX-M2**, TreatmentFrom **15/09/2026** | CONSULT 300.000 | 2 | FAIL | | `01/09/2026` | FULL — LogNotContains `từ 01/04/2026` |
| T-CH02-e | LossType **ILL_IP** (nhóm 100), Inpatient, ConditionClass **BIR** | ROOM 1.000.000 | 2 | HOLD | | `BR-WAIT-4` | FULL |
| T-CH03-a | **FX-M4** (ContinuousCoverFrom 01/04/2025), ConditionClass **PRE** | CONSULT 300.000 | 3 | PASS | | `không loại trừ` | PARTIAL — "hiện trên F48" là màn |
| T-CH04-a | | SUPPLEMENT 400.000 | 4 | HOLD | | `TERM-HC-003 — checkpoint` | PARTIAL — "ngoài cổng theo G3" thuộc cổng |
| T-CH04-b | | OPTICAL 800.000 | 4 | HOLD | | `TERM-HC-004` ‖ `TBC-04` ‖ `checkpoint` | FULL |
| T-CH04-c | LossType **ACC_OP**, ConditionClass **ACC** | DENTAL 500.000 | 4 | PASS | | `TERM-HC-006 — KHÔNG_THUỘC_PHẠM_VI` | FULL |
| T-CH04-d | (dùng fixture T-CH04-a) | | 4 | | | `10 điều khoản trong sổ · 6 ACTIVE` | FULL (x+y+z=m kiểm bằng mắt trên log) |
| T-CH04-e | IcdCode **Z41.1** | SUPPLEMENT 400.000 | 4 | HOLD | | `TBC-01` ‖ `TBC-03` | PARTIAL — hai term cùng **checkpoint** một dòng (mọi clause TBC- nên không thể decline); mang cả hai clauseRef |
| T-CH05-a | | CONSULT 300.000 | 5 | PASS | | `→ 200.1` | FULL |
| T-CH05-b | LossType **ILL_IP**, Inpatient | CONSULT 300.000 | 5 | PASS | | `→ 100` | FULL — LogNotContains `100.1` |
| T-CH05-c | LossType **ILL_IP**, Inpatient | DENTAL 500.000 | 5 | PASS | | `NONE` | FULL (bước 6 sẽ NOTCOV) |
| T-CH05-d | | OTHER 200.000 | 5 | PASS | | `ASSESSOR` | FULL |
| T-CH05-e | LossType **ACC_IP**, Inpatient, ConditionClass **ACC** | ROOM 1.000.000 | 5 | PASS | | `→ 500` ‖ `tai nạn` | FULL |
| T-CH06-a | | ADMIN 150.000 | 6 | CUT | | `NOTCOV` | FULL |
| T-CH06-b | + Assessment + Recommendation(ClaimLineId = dòng LAB, Code `NOTREL`, Decision **`Confirmed`**) | CONSULT 300.000 · LAB 400.000 | 6 | CUT | | `NOTREL` | FULL (nhánh Proposed = hồ sơ 0001) |
| T-CH06-c | | CONSULT 300.000 · LAB 400.000 · DRUG 500.000 · ADMIN 150.000 | 6 | CUT | **1.200.000** | | FULL |
| T-CH07-a | | CONSULT 300.000 | 7 | SKIP | | `0 điều khoản` | PARTIAL — render là màn |
| T-CH08-a | | CONSULT 300.000 · LAB 1.400.000 · DRUG 900.000 | 8 | CUT | **2.400.000** | `cắt 200000` | FULL |
| T-CH08-b | | PHYSIO 600.000 × **12**, ServiceDate 14/07…25/07/2026 | 8 | CUT | **6.000.000** | | FULL |
| T-CH08-c | | CONSULT 900.000 · CONSULT 900.000 (cùng ngày) | 8 | CUT | **1.500.000** | `cắt 300000` | FULL |
| T-CH09-a | + LimitLedger (FX-M1, FX-STD, `200.3`, 2026) **UsedCount 18**, CountUnit VISIT | PHYSIO 600.000 × **5**, 14/07…18/07/2026 | 9 | CUT | **1.000.000** | `vượt hạn mức` | FULL — 2 lượt trong hạn (2 × 500.000 sau bước 8), 3 lượt LIMIT |
| T-CH09-b | | CONSULT 100.000 × **40**, 40 ngày từ 14/07/2026 | 9 | PASS | **4.000.000** | `không giới hạn lượt` | FULL |
| T-CH09-c | LossType **ILL_IP**, Inpatient | ROOM 1.000.000 | 9 | HOLD | | `Q-IDP-06` | FULL |

(‖ = ngăn cách nhiều mảnh `AssertLogContains`, trong DB dùng `||`.)

## 4. 42 test còn lại

`9.1` bước 10–14 (14 test) → `PENDING` cho tới khi bước cài xong; `9.2` T-CHAIN-01…07 → PENDING (01/02/04 chạy được sau bước 14; 03 determinism; 05 BR-TRACK-4; 06 V-546; 07 cây 3 cấp); `9.3` T-GATE-01…07 → MANUAL/PENDING (change `approval-gate-4-conditions`); `9.4` T-SEED-01…11 → assertion đếm bảng (lượt sau: runner đếm `PolicyBenefitSnapshot` theo plan, `BenefitWaitingPeriod`, `ServiceCategory`, `TermRegister` 10/6/4…). Tất cả 71 dòng **có mặt trong bảng từ đầu** với text nguyên văn.

## 5. Test seed (9.4) — assertion đếm bảng (lượt I6, sau I5)

Runner phần 2 `RunSeedTests`: mỗi dòng 9.4 có `AssertStep = 0`; kiểm bằng aggregate đếm, kỳ vọng từ `expected-71.md`:

| TestId | Kiểm bằng | Kỳ vọng (từ spec) | Trạng thái trên ClaimMVP |
|---|---|---|---|
| T-SEED-01 | `PolicyBenefitSnapshot` của 3 policy demo (không FX-) | 51 (17 × 3) | RUNNABLE |
| T-SEED-02 | `BenefitWaitingPeriod` | 51; không dòng nào cho ô n/a (không có dòng `BIR`/`MAT` với nhóm 100/200/300/500, không `PRE`/`SPE`/`GEN`/`ACC` với 400…) | RUNNABLE |
| T-SEED-03 | `ServiceCategory` | 20 record; mọi `OutpatientBenefit`/`InpatientBenefit` ∈ {mã benefit, NONE, ASSESSOR}; không giá trị chứa "DECLINE" | RUNNABLE |
| T-SEED-04 | `adjudication_rule` | 14 dòng {step, name, enabled} | **MANUAL** — ClaimMVP không có bảng adjudication_rule (thứ tự cố định trong code, A5 §6.1); ghi chú |
| T-SEED-05 | `DeclineReason` static | 7 record: LIMIT, PERIOD, NOBEN, WAIT, EXCL, NOTREL, NOTCOV | RUNNABLE |
| T-SEED-06 | `TermRegister` | 10 dòng; 6 ACTIVE; 4 DRAFT; mọi dòng SelectorFact ≠ ""; PredicateText ≠ "" chỉ trên TermClass D có Effect DECLINE_* | RUNNABLE |
| T-SEED-07 | `provider` | 6 dòng, ≥1 in_network = false, ≥1 country ≠ VN | **MANUAL** — ClaimMVP chưa có Entity Provider (màn Providers đọc từ đâu? — kiểm ở I6); nếu có Entity thì RUNNABLE |
| T-SEED-08 | `Claim.RuleSetRef` (MVP: trên Claim, không trên Policy) | non-null trên mọi Claim | RUNNABLE (ghi chú khác chỗ đặt) |
| T-SEED-09 | `PolicyBenefitSnapshot.CopayPct` | seed "20", wire 2000 bp — MVP: `CopayPct` Text "20"/"INHERIT"; quy đổi bp ở bước 10 | RUNNABLE một nửa: kiểm giá trị seed = "20" trên `200` HC-STD |
| T-SEED-10 | `line_decline_rule` | không tồn tại | RUNNABLE (kiểm không có Entity tên đó — bằng cách: luôn PASS kèm ghi chú; hoặc MANUAL) → MANUAL |
| T-SEED-11 | Cột Deductible | null, không phải 0 | MANUAL — ClaimMVP không có cột Deductible (đúng spec: sản phẩm không có) |

## 6. Thứ tự 33 test còn lại (PO 21/09 tối) + chế độ chạy của runner

PO: *"1. T-CHAIN-05 — nửa sau của demo, chưa chứng minh · 2. T-CHAIN-03 — xác định tính, SC-H4 · 3. T-CHAIN-01/02/04/06/07 · 4. T-CH14 (6) · 5. T-SEED (11). T-GATE (7) BLOCKED — câu hỏi 3 hay 4 điều kiện (20/09) chưa có trả lời."* Lỗi AnnualLimit = **W-8 build-04 §8.1 "Snapshot KHÔNG BAO GIỜ đọc lại"** — đã sửa rev 151 (I6-E), thêm test tường minh T-CH12-d (hồ sơ không có dòng ledger).

Runner `RunChainTests` có cột **`Mode`** trên `ChainTest`:
| Mode | Cách chạy | Pass khi |
|---|---|---|
| `DRYRUN` (mặc định) | `A5Chain_DryRun` → bước AssertStep | như §3 |
| `CONFIRM_RERUN` (T-CHAIN-05) | `RunAdjudicationChain` lần 1 → TotalA · set `Recommendation.Decision = "Confirmed"` (Code NOTREL của hồ sơ) · chạy lần 2 → TotalB | TotalA = `AssertAmountBefore` và TotalB = `AssertAmount` và hai ChainRun mới có RunNo liên tiếp |
| `RUNTWICE` (T-CHAIN-03) | `RunAdjudicationChain` hai lần liên tiếp, đọc 2 `ChainRun` mới nhất | `InputsHash` giống nhau, `StepOutputsJson` giống nhau byte-for-byte, TotalPayable giống nhau (= `AssertAmount` nếu đặt) |

Fixture bổ sung (Part5): `TST-T-CHAIN-01` (A5 §6.2: CONSULT 300.000 · LAB 1.400.000 · DRUG 900.000 · SUPPLEMENT 400.000 + `TermCheckpoint` UPHELD TERM-HC-003 trên dòng SUPPLEMENT) · `TST-T-CHAIN-02` (= 01, IsInNetwork False) · `TST-T-CHAIN-04` (CONSULT 333 · CONSULT 333 — copay từng dòng 67+67 = 134 ≠ 133 làm tròn một lần ⇒ payable 532) · `TST-T-CHAIN-05` (bản sao 6 dòng hồ sơ 0001 + Assessment + Recommendation NOTREL **Proposed** trên dòng Atorvastatin 168.000 ⇒ run 1 = 2.000.000, run 2 sau xác nhận = **1.880.000**; dòng 168.000 chỉ tiết kiệm 120.000 vì 200.2 đã bị trần).

| Test | Mode | Fixture | Assert |
|---|---|---|---|
| T-CHAIN-05 | CONFIRM_RERUN | TST-T-CHAIN-05 | Before 2.000.000 → After **1.880.000** |
| T-CHAIN-03 | RUNTWICE | TST-T-CHAIN-01 | hash/StepOutputsJson/total giống nhau, total 1.920.000 |
| T-CHAIN-01 | DRYRUN | TST-T-CHAIN-01 | bước 13 = 1.920.000 |
| T-CHAIN-02 | DRYRUN | TST-T-CHAIN-02 | bước 13 = 1.920.000, bước 11 log∋"quy tắc tạm B-19" (kiểm ở bước 11: PASS 1.920.000) |
| T-CHAIN-04 | DRYRUN | TST-T-CHAIN-04 | bước 10 CUT = 532 |
| T-CH12-d (bổ sung PO) | DRYRUN | TST-T-CH10-a | bước 12 PASS = 1.920.000, log∋"đã dùng 0/20000000"‖"trong hạn" |
## 7. T-CHAIN-06 / T-CHAIN-07 — Mode `CONCURRENT` và `RUN` (21/09 tối, thứ tự PO: 06 runner → 07 fixture → T-CH14 → T-SEED)

| Mode | Chạy gì | Pass khi |
|---|---|---|
| `CONCURRENT` (T-CHAIN-06) | `ChainTest_Concurrent(ClaimId1, ClaimId2)`: (0) reset — `A5Ledger_Reverse` cả hai hồ sơ (idempotent, chỉ FINAL) · (1) `RunAdjudicationChain(1)` → TotalA1 · (2) `RunAdjudicationChain(2)` → TotalB1, RunNoB1 · (3) `A5Ledger_Finalize(1)` ("claim 1 được duyệt trước") → đọc `LimitLedger(200).UsedAmount` sau · (4) `RunAdjudicationChain(2)` → TotalB2, RunNoB2 · (5) đếm `LedgerPosting` hồ sơ 2: ChainRunId = runB1 còn PROVISIONAL (phải 0), ChainRunId = runB2 PROVISIONAL (> 0) · (6) teardown `A5Ledger_Reverse(1)` để ledger về mốc seed | TotalB1 = `AssertAmountBefore` ∧ TotalB2 = `AssertAmount` ∧ RunNoB2 = RunNoB1 + 1 ∧ posting run B1 PROVISIONAL = 0 ∧ posting run B2 > 0. LastOutcome `MOVED`. Log: "hồ sơ 1 = {A1} → FINAL, ledger 200 {trước} → {sau}; hồ sơ 2 run {B1} = {TotalB1}; run {B2} = {TotalB2} (còn {remaining}); posting run {B1} đảo {n}/{n}" |
| `RUN` (T-CHAIN-07) | một `RunAdjudicationChain` thật, rồi `ChainTest_FindStep(AssertStep)` trên `StepResults` của run (như DRYRUN nhưng có bước 14) | assertion chuẩn §3 trên bước AssertStep |

`ChainTest` +`FixtureClaimNo2` Text 40 (hồ sơ thứ hai của CONCURRENT).

**Fixture Part6** (tôi đặt số — expected-71 chỉ cho hành vi; số chọn để cộng trừ đọc được bằng mắt):
- Member **FX-M8** (FX-STD, CoverageFrom 01/04/2026) + `LimitLedger`(FX-M8, FX-STD, "200", 2026, UsedAmount **19.000.000**, RemainingAmount 1.000.000, UsedCount 0). Thành viên riêng vì Finalize/Reverse đụng ledger — không dùng chung với FX-M1/M6/M7.
- `TST-T-CHAIN-06a`, `TST-T-CHAIN-06b`: cùng FX-M8, ngoại trú, trong mạng, mỗi hồ sơ **1 dòng CONSULT 800.000** (→ 200.1; < trần 1.500.000; copay 20 % → 640.000).
  - Hồ sơ 2 run 1: còn 20.000.000 − 19.000.000 = 1.000.000 ≥ 640.000 → **640.000**.
  - Finalize hồ sơ 1: ledger 200 Used 19.000.000 → **19.640.000** (posting AMOUNT trên 200.1 và tổ tiên 200).
  - Hồ sơ 2 run 2: còn 360.000 → bước 12 cắt 280.000 → **360.000**.
- `TST-T-CHAIN-07`: FX-M1, **nội trú** (TreatmentType Inpatient, LossType tương ứng), 1 dòng Category `ICU` "1 ngày ICU" AmountClaimed **4.000.000**, ServiceDate = TreatmentFrom = TreatmentTo = 14/07/2026, **`TreatmentDays = 1`** (A4 L18). Không có ledger 100.x.
  - Bước 5: ICU → 100.1.1 (A4 §4.5). Bước 8: PER_DAY 3.000.000 → cắt 1.000.000 → 3.000.000. Bước 9: 100.1.1 1/15 ngày, 100.1 1/60 ngày — PASS. Bước 10: copay INHERIT → 100 "" → 0. Bước 12: 100 hạn 100.000.000 — trong hạn. Bước 14: AMOUNT ×3 (100.1.1, 100.1, 100) 3.000.000; **DAY ×2** (100.1.1 +1, 100.1 +1).

| Test | Mode | Fixture | Assert |
|---|---|---|---|
| T-CHAIN-06 | CONCURRENT | TST-T-CHAIN-06a + 06b | Before **640.000** → After **360.000**, RunNo +1, posting run 1 đảo |
| T-CHAIN-07 | RUN | TST-T-CHAIN-07 | bước 14 POSTED = **3.000.000**, log∋"2 DAY"‖"100.1.1 +1"‖"100.1 +1"‖"3 posting AMOUNT" |
| T-CH09-c (hồi quy) | DRYRUN | không đổi | vẫn HOLD Q-IDP-06 khi `TreatmentDays = 0` |

**Khoảng trống ghi nhận (không phải test xong):** T-CHAIN-06 mới thử **hàm** `A5Ledger_Finalize`; khi Finalize nối vào Approve thật (sau khi PO chốt 3/4 điều kiện) phải **chạy lại qua Approve**.

### Bài học seed (21/09 đêm, rev 160)
`Seed_ChainTestFixtures` từ I4 đến rev 159 **chưa bao giờ xoá** fixture: cleanup xoá `Claim` trước khi xoá con (LedgerPosting/CalcSnapshot/ChainRun…) → lỗi FK bị nuốt, vòng lặp chạy tiếp, mỗi lần bấm "Seed fixtures" nối thêm trọn bộ (4 bản TST-T-CH01…09, 3 bản Part4, 2 bản Part5 trước khi sửa). Runner `max 1` chọn bản bất kỳ nên bộ test vẫn xanh cho tới khi một bản bị test làm đổi (T-CHAIN-05 Confirmed). Từ rev 160: `ChainTest_DeleteClaimTree` xoá con trước cha; `ChainTest_ConfirmRerun` tự reset NOTREL → Proposed. **Quy tắc:** mọi test làm đổi dữ liệu fixture phải tự reset ở đầu (không dựa vào seed), và seed phải kiểm bằng cách đếm Work list sau 2 lần bấm.
