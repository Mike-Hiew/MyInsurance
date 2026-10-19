# Bảng đối chiếu — `RunAdjudicationChain` hiện có × 14 bước A5 §6.1

> Việc 1 của `tasks.md`. Cột "Code hiện tại" lấy từ `T07_Chain_Shells` (rev 4/5, đánh số BRD CH-01…14) + Mentor read-only trên rev 138 (điền ở §2). Chuẩn: A5 §6.1 + D1 §5.3; §6.2 base amount; làm tròn đồng nguyên half-away-from-zero **ở từng bước**.

## 1. Đối chiếu thứ tự

| A5 # | Bước A5 | Áp dụng | Code hiện tại (BRD) | Kết luận |
|---|---|---|---|---|
| 1 | Member eligibility — member active on service date, policy in force, premium paid | luôn | `Step01_PolicyEligibility` (CH-01) | giữ tên/ý; kiểm 3 điều kiện + `PERIOD` |
| 2 | Waiting period — theo condition class (§5), bảng `BenefitWaitingPeriod` 51 dòng | luôn | `Step02_WaitingPeriod` (CH-02: 30/90/0 cứng) | **viết lại**: đọc `BenefitWaitingPeriod` theo plan + benefit, không hằng số |
| 3 | Pre-existing condition rule — 365 ngày, không loại trừ | nếu `condition_class = PRE` | — (CH-03 là "event classification", không phải PRE) | **mới** |
| 4 | General exclusions — term engine, scope EVENT/LINE, ghi mọi kết quả kể cả không bắn | luôn | `Step05_CoverageScopeExclusions` (CH-05 gộp loại trừ + khuyến nghị lâm sàng) | **tách**: bước 4 = term engine `EXCL` (TBC- clause), **không** áp NOTREL/NOTCOV ở đây |
| 5 | Benefit mapping — cost line → benefit code (§8.1); **không từ chối gì** | luôn | `Step04_LineBenefitMapping` (CH-04, có `UNMAPPED`) | giữ; UNMAPPED → hàng đợi người, không decline |
| 6 | Line-level cover test — từng dòng payable/declined kèm lý do; NOTREL/NOTCOV **chỉ từ Recommendation đã Confirmed** | luôn | nửa sau của CH-05 | **tách** khỏi CH-05; SUPPLEMENT payable (PO 21/09) |
| 7 | Benefit-level provisos — term engine scope BENEFIT | nếu benefit có | — | **mới** (bước term engine thứ hai BRD gộp mất) |
| 8 | Per-visit / per-day / per-event sub-limit — trần con, theo `LimitType` + `PerVisitCap/PerDayCap/PerEventCap` | nếu có | `Step07_PerVisitCap` (CH-07 → `CAP_VISIT`) | giữ; thêm PER_DAY/PER_EVENT |
| 9 | Count limit — ngày/lượt còn lại, `COUNT_UNLIMITED()`; counter OCCURRENCE/DAY | nếu có | — | **mới** |
| 10 | Co-payment — base = số còn lại **sau bước 9**; `CopayPct` bp, `INHERIT` → tổ tiên | nếu benefit/tổ tiên có | `Step09_CoPayment` (CH-09 → `COPAY`) | giữ; base đúng §6.2 |
| 11 | Deduction ratio — từ hợp đồng, base = sau bước 10, chỉ khi điều kiện đúng (`DeductionCondition`) | có điều kiện | — | **mới**; MVP: không bắn (in-network) nhưng phải tồn tại |
| 12 | Parent annual cap — walk ancestor tới root, đọc `CapModeId` trước | luôn | `Step08_PerYearLimitCheck` (CH-08 → `LIMIT_YEAR`) + `Step11_ClaimLevelCap` (CH-11 `LIMIT_TOTAL`) | gộp lại đúng vị trí 12; `CapMode` Included ⇒ tính vào cha |
| 13 | Plan-level check — sản phẩm không có trần toàn plan; bước tồn tại cho tương lai | luôn | `Step11_ClaimLevelCap`? | no-op có log |
| 14 | Post to ledger — provisional; post benefit + mọi tổ tiên + mọi counter trên đường; không đếm hai lần | luôn | `Step14_LedgerPreview` (CH-14, "chưa post") | ghi `LimitLedger` provisional thật (rule kiến trúc 1: đây là nơi duy nhất ghi tiền) |

**Bốn lệch BRD phải biến mất:**

| # | BRD trong code | A5 | Việc |
|---|---|---|---|
| 1 | `Step06_SocialInsuranceDeduction` (CH-06 BHYT) | **đã loại bỏ** (S-17) — slot dự phòng là bước 8 tương lai, không phải bây giờ | xoá khỏi chuỗi |
| 2 | `Step10_Deductible` (CH-10 `DEDUCT`) | **sản phẩm không có** | xoá khỏi chuỗi (`DEDUCT` causeCode giữ trong danh mục A2 nhưng không bao giờ bắn) |
| 3 | `Step12_Rounding` — "bước DUY NHẤT làm tròn", xuống 1.000 ₫ (B-20) | đồng nguyên, half-away-from-zero, **ở từng bước** | xoá bước; mỗi bước tiền gọi `RoundVnd()` |
| 4 | CH-05 gộp loại trừ + khuyến nghị; không có bước proviso | bước 4 (EXCL term) và bước 7 (BENEFIT proviso) riêng; bước 6 riêng | tách thành 3 Server Action |

`Step13_CustomerBorneBreakdown` (CH-13) không phải bước A5 — là **đầu ra** (bảng "Vì sao Quý khách tự chịu" theo causeCode); giữ như hàm hậu xử lý ngoài 14 bước, không phải bước có số.

## 2. Nội dung code hiện tại trên rev 139 (Mentor read-only, 21/09 12:10)

**Kết luận: chưa có chuỗi nào chạy.** `RunAdjudicationChain` (in `ClaimId`; out 4 Total* Decimal + `StepLogJson`) gọi tuần tự `Step01_PolicyEligibility` … `Step14_LedgerPreview` rồi gán **`TotalRequested = TotalCovered = TotalPayable = TotalCustomerBorne = 0`**, ghép `StepLogJson = "[" + Log01 + "," … + Log14 + "]"`. **Không ghi Entity nào.** Cả 14 `StepNN_` là vỏ rỗng `Start → Assign StepLog = "" → End` — không aggregate, không đọc/ghi entity, không hằng số (y nguyên T07 rev 4).

Mọi con số trên màn hôm nay là **seed literal** trong `Seed_AssessmentAndTerms`: `NewClaim.TotalPayable = 2000000`, `NewClaim.TotalClaimed = 3128000`, `NewCR.TotalPayable = 2000000` (ChainRun), `NewLL` 200 = 2.000.000 (Remaining 18.000.000) · 200.1 = 1.200.000 · 200.2 = 800.000 (Remaining 0 — sai, nhưng là seed), `CalcSnapshot` 9 dòng text bước. `Seed_LimitLedger` tạo 15 dòng gốc UsedAmount 0 cho 3 member. **Không action nào ghi `ClaimLine.AmountPayable`.** `MigrateA6_DecisionCodes` ghi `ClaimLine.LineDecisionId/DeclineReasonId`; `TransitionClaimStatus` chỉ ghi `StatusId`.

⇒ Câu "chuỗi hiện tại tái tạo đúng run #1" ở báo cáo đợt B là **sai cách đọc**: 2.000.000 và 3 dòng ledger khớp tính tay vì **tôi seed đúng số**, không phải chuỗi tính ra. Bốn lệch BRD hiện diện **theo tên và mô tả** của vỏ (CH-06 BHYT, CH-10 Deductible, CH-12 Rounding 1.000, CH-05 gộp), chưa có logic — nên "xoá" thực chất là bỏ vỏ khỏi chuỗi, không phải gỡ code.

Entity chứa output từng bước đã có: **`ChainRun`** (ClaimId, RunNo, InputsHash Text 64, Version Text 20, StepOutputsJson, 4 Total*, GateResultJson, RunAt, TriggeredBy) và **`CalcSnapshot`** (ChainRunId, ClaimId, StepNo, StepLabel Text 10, StepName Text 100, Detail Text 500, ResultAmount Decimal 18, Outcome Text 20 PASS/FAIL/CUT, ReasonCode Text 20). Chuỗi thật ghi vào hai bảng này; màn "Calculation & ledger" đọc `CalcSnapshot` nên tự đổi text bước (C-1 phần còn lại).

## 3. Kịch bản B — số phải ra (đích cố định, `specs/adjudication`)

Hồ sơ 0001 HC-STD ngoại trú, 6 dòng, 3.128.000: **run #1 = 2.000.000** (200.1: 1.700.000 → cap 1.500.000; 200.2: 1.018.000 → cap 1.000.000; copay 20 % = 500.000) · **run #2 sau NOTREL Atorvastatin −168.000 = 1.880.000** (200.2 = 850.000 dưới trần; copay 470.000). Ledger: 200 = 2.000.000 → 1.880.000 · 200.1 = 1.200.000 · 200.2 = 800.000 → 680.000.

## 4. Nhật ký dựng chuỗi A5 (21/09)

| Lượt | Nội dung | Kết quả |
|---|---|---|
| A (hạ tầng) | `Policy.PremiumPaid` Boolean · `Claim.ConditionClassId` FK ConditionClass · Structures `ChainLineCtx` (14) / `ChainCutCtx` (5) / `ChainCtx` (29) · `RoundVnd()` · `Seed_BatchB_Backfill` +PremiumPaid True mọi policy, +ConditionClass theo LossType (Accident* → ACC, Maternity* → BIR, còn lại GEN — bảng tường minh, chỉ chạy khi null) · readout "Claims without condition class" / "Policies without PremiumPaid" · `A5Chain_LoadContext(ClaimId) → Ctx` (Claim ⋈ Member ⋈ Policy ⋈ PolicyStatus ⋈ LossType ⋈ ConditionClass; lines theo Id; Parent/Root từ snapshot). PolicyStatus in-force = `Value = "ACTIVE"` (record `Active`; `Lapsed` = "CANCELLED"). | 0 lỗi, 66 warning |
| Ghi chú | OutSystems Boolean không null ⇒ `PremiumPaidIsNull` vô nghĩa; bước 1 coi `PremiumPaid = False` là FAIL (D1: hard flag, computed decline). Root code: lượt A chỉ walk Depth ≤ 1 → sửa ở lượt B (walk tới Depth 0 cho 100.1.1). | |
| B (bước 1–3) | `A5Step01_Eligibility` · `A5Step02_WaitingPeriod` · `A5Step03_PreExisting` · `A5Chain_DryRun(ClaimId) → StepLogs, TotalAfter, Outcomes` (Start → CallLoad → AssignCtx → CallS01 → AssignS01 → CallS02 → AssignS02 → CallS03 → AssignS03 → End) · `ChainCtx.TreatmentTypeValue` · RootCode walk tới Depth 0 · LoadContext output đổi tên `OutCtx`. Run `1c72047f…`. | 0 lỗi, warning cũ. **Chưa publish** |
| C (bước 4–5) | 21/09: `A5Step04_GeneralExclusions` (term engine PRODUCT; TBC-/J/E ⇒ checkpoint, D + clause thật ⇒ EXCL; log mọi term) · `A5Step05_BenefitMapping` (ServiceCategory × TreatmentTypeValue; UNMAPPED ⇒ HOLD, không decline; re-resolve Parent/Root) · DryRun mở rộng tới bước 5 · block "Chain dry-run (A5 steps 1–5)" trên Administration. Run `54c19ebd…`. Node: Step04 `Start → IfDeclined → True: AssignSkip → EndSkip | False: AssignInit → GetTerms → ForEachTerm { AssignTermInit → ForEachLine { IfLineMatch → False: AssignJ++ | True: IfTBC/J/E → True: AssignCheckpoint / False: AssignExcl → AppendCut } → AssignTermLog } → AssignSumInit → ForEachSum → AssignFinal → End`; Step05 `Start → IfDeclined → … ForEachLine { GetSC → IfSCFound → False: AssignUnmapped | True: AssignMappedCode → IfEmptyCode → True: AssignEmptyUnmapped / False: AssignSetCode → IfRemapped → True: AssignRemapLog → GetPBSRow / False: GetPBSRow → GetParentPBS → AssignResolvePBS } → Sum → End`; DryRun `… → CallS04 → AssignS04 → CallS05 → AssignS05 → End`; Administration `RunDryRun` (Start → CallDryRun → AssignDR → End), locals `DryRunClaimId`(17)/`DryRunStepLogs`/`DryRunOutcomes`/`DryRunTotalAfter`, block trong `DataCheckCard`. TreatmentType so với `Value = "INPATIENT"` (record `Inpatient`). | 0 lỗi, 65 warning. **Publish rev 140** |

### Phát hiện lượt C — `TermRegister` không phải sổ loại trừ (cần PO quyết)

Mentor đọc 10 dòng seed trong `Seed_AssessmentAndTerms`: **T-01 Waiting · T-02 Pre-existing · T-03 Outpatient · T-04 Per-visit · T-05 Co-pay · T-06 Room · T-07 Antibiotic · T-08 Cosmetic · T-09 In-network · T-10 Duplicate**, cột: `Code, Name, TermClass, Scope, ClauseRef, IsActive, AppliedCount30d`. **Tất cả `Scope = "HC-STD"`** (mã plan), không có `PRODUCT/BENEFIT/LINE` như A5 §6.1 #4/#7; **không có cột nói term áp cho category/benefit nào**. Đây là bảng "điều khoản đã áp" hiển thị ở màn Terms (đợt A), không phải term engine.

Hệ quả trên rev 140:
- `A5Step04` lọc `Scope = "PRODUCT"` ⇒ **0 dòng** ⇒ log `"Không có điều khoản Scope=PRODUCT — bỏ qua."`, Outcome PASS. Bước 4 **tồn tại nhưng không bắn** — trung thực, không sai tiền, nhưng chưa phải term engine.
- Quy tắc khớp dòng Mentor cài (ghi ở description của action): `line.Category = ToUpper(Substr(Name, 1, Index(Name, " ")))` — token đầu của `Name`. Với dữ liệu hiện tại chỉ **T-06 "Room" ↔ ROOM** khớp. **Đây là heuristic suy diễn** (vi phạm tinh thần B1) và có bẫy: nếu ai đổi Scope của T-06 sang PRODUCT với class D + clause thật, mọi dòng ROOM bị EXCL. Đề nghị: **bỏ heuristic**, thêm cột tường minh `TermRegister.AppliesToCategory` (Text 30, rỗng = không khớp dòng nào) + `Scope ∈ {PRODUCT, BENEFIT}` + seed lại theo A5 (đây là thay đổi model ⇒ ghi ở `verify-entity-vs-build01` đợt F hoặc change riêng trước khi code). Không tự làm — chờ PO.

### Dry-run rev 140 — hồ sơ 0001 (Id 17), 21/09 12:30 (Administration › "Chain dry-run (A5 steps 1–5)")

```
01 PASS 3128000 — Thành viên có hiệu lực từ 01/01/2026; hợp đồng HC-STD hiệu lực 01/01/2026–31/12/2026; phí đã ghi nhận; ngày điều trị 10/06/2026 nằm trong thời hạn.
02 HOLD 0       — [BR-CHAIN-1] Chưa xác định nhóm bệnh (condition class) — không đánh giá được kỳ chờ.
03 SKIP 3128000 — Không phải bệnh có sẵn — bước 3 không áp dụng.
04 PASS 3128000 — Không có điều khoản Scope=PRODUCT — bỏ qua.
05 PASS 3128000 — 6 dòng → 200.1, 200.1, 200.1, 200.2, 200.2, 200.2
Outcomes: 01:PASS;02:HOLD;03:SKIP;04:PASS;05:PASS · TotalAfter 3.128.000
```

Đọc: bước 1 đúng dữ liệu seed; bước 2 HOLD **vì backfill ConditionClass không ăn** (readout "Claims without condition class" = 12 sau seed; "Policies without PremiumPaid" = 0 ✅); bước 3 nói "không phải bệnh có sẵn" khi chưa biết class — sai tinh thần BR-CHAIN-1; bước 2 báo ResultAmount 0 trên HOLD — phải là Σ Running; bước 4 không có term (xem phát hiện lượt C); bước 5 mapping khớp seed, chưa có remap/UNMAPPED. Tổng chưa đổi (đúng: bước 1–5 không cắt tiền trên hồ sơ này). **Lượt D** gửi sửa 3 điểm (backfill, ResultAmount trên HOLD/SKIP, log bước 3). Run `4aea06cb…`.

| D (sửa) | 21/09: **Nguyên nhân backfill ConditionClass không ăn:** phép gán `ConditionClassId` nằm **bên trong nhánh `RuleSetRef = ""`** của `Seed_BatchB_Backfill`; lần seed 2 trở đi `RuleSetRef` đã có giá trị ⇒ `ifRuleSetRef.False` nhảy thẳng tới `ifCChanged`, bỏ qua gán. Sửa: `ifRuleSetRef → False: ifCondClass → True: assignCondClass → ifCChanged / False: ifCChanged` (không node mới). `A5Step02` hai nhánh HOLD: `assign…Hold → feHold → asSumH (cycle) → asResH → end` (ResultAmount = Σ Running). `A5Step03`: `ifDeclined → False: ifCondClassNull03 → True: assignHold03 → feHold03 → asSumH03 → asResH03 → endHold03 / False: ifPRE → …`. Run `4aea06cb…`. | 0 lỗi, 65 warning. **Publish rev 141** |

### Dry-run rev 141 — hồ sơ 0001 (Id 17), 21/09 12:35 — **báo cáo mốc 5 bước**

Readout: Claims without condition class **0** · Policies without PremiumPaid **0** (seed rev 141, log `RunSeedBatch0Now done` 05:33:42Z).

```
01 PASS 3128000 — Thành viên có hiệu lực từ 01/01/2026; hợp đồng HC-STD hiệu lực 01/01/2026–31/12/2026; phí đã ghi nhận; ngày điều trị 10/06/2026 nằm trong thời hạn.
02 PASS 3128000 — [BR-WAIT-5] Kỳ chờ 30 ngày cho 200/GEN, tính từ 01/01/2026 ⇒ đủ điều kiện từ 31/01/2026; ngày điều trị 10/06/2026 — đủ điều kiện.
03 SKIP 3128000 — Không phải bệnh có sẵn — bước 3 không áp dụng.
04 PASS 3128000 — Không có điều khoản Scope=PRODUCT — bỏ qua.
05 PASS 3128000 — 6 dòng → 200.1, 200.1, 200.1, 200.2, 200.2, 200.2
Outcomes 01:PASS;02:PASS;03:SKIP;04:PASS;05:PASS · TotalAfter 3.128.000
```

Đối chiếu spec trên đĩa (không phải đối chiếu với code):
- Bước 2: A5 §5 bảng — `GEN` × Outpatient `200` = **30 d**, đo từ coverage start ⇒ 01/01 + 30 = 31/01/2026 ✅. Số đọc từ `BenefitWaitingPeriod` (51 dòng seed đợt B), không hằng số.
- Bước 3: A5 §6.1 #3 chỉ áp khi `PRE` ⇒ SKIP đúng cho GEN.
- Bước 5: 6 dòng seed B3 đã mang 200.1/200.2; mapping tra `ServiceCategory` cho ra cùng mã ⇒ không remap, không UNMAPPED. (Chưa có test cho nhánh remap/UNMAPPED — chờ 65 test.)
- Bước 4: **A5 §7.2 là sổ term thật** — 10 term, `Scope = PRODUCT`, mỗi term có `selector` (term có trong cuộc?) và `predicate` (có decline?) trên **5 fact đóng**: `serviceCategory`, `lossType`, `lineDiagnosisCode`, `diagnosisCode`, `facilityCountry`; effect `CHECKPOINT | DECLINE_LINE | DECLINE_EVENT | REQUIRE_DOC`; clause `TBC-01…10`; **4 dòng `DRAFT` (#5 #8 #9 #10) không được engine đánh giá**. Trên hồ sơ 0001: #3 SUPPLEMENT ⇒ **CHECKPOINT** (không decline); #2 SCREENING/VACCINATION, #4 OPTICAL/HEARING, #6 DENTAL không có dòng khớp; #1/#7 selector trên `diagnosisCode` N39.0 ⇒ không khớp. ⇒ Kỳ vọng bước 4 đúng spec trên hồ sơ 0001: **HOLD** — checkpoint `TBC-03` trên **dòng 6 Men vi sinh (SUPPLEMENT, 150.000)**, không cắt tiền (A4 §4.5: SUPPLEMENT có chi trả; A5 §7.4: người quyết) — chứ không phải PASS như rev 141. **Entity `TermRegister` hiện không mang selector/predicate/effect** và 10 dòng seed là bảng hiển thị đợt A ⇒ cần đổi model + seed lại theo §7.2 **trước khi** bước 4 có nghĩa. Đây là thay đổi hành vi ⇒ change openspec trước code (đề xuất: ghi vào `verify-entity-vs-build01` đợt F hoặc mở `term-register-a5-7-2`).

Ghi chú phụ (không sửa bây giờ): `Seed_AssessmentAndTerms` tạo `ChainRun` mới mỗi lần seed (log "chainrun B created 88/89/90") ⇒ bảng ChainRun tích luỹ; chuỗi thật khi ghi `ChainRun` phải reset/đếm `RunNo` theo claim.

## 5. Đợt C-2 (21/09 chiều) — PO: 71 test, TermRegister thuộc change này, báo cáo sau bước 9

| Lượt | Nội dung | Kết quả |
|---|---|---|
| E | `TermRegister` +9 cột (RuleSetRef, Status, Effect, ReasonCode, SelectorFact, SelectorOp, SelectorValues, PredicateText, SortNo) · seed lại **10 dòng A5 §7.2** (xoá T-01…T-10; 6 ACTIVE #1 #2 #3 #4 #6 #7, 4 DRAFT #5 #8 #9 #10; RuleSetRef `A5-v1.5/HC-STD`) · Entity mới `TermCheckpoint` (ClaimId, ClaimLineId, TermCode, ClauseRef, Status OPEN/UPHELD/CLEARED/MOOT, Note, RaisedOn, DecidedBy, DecidedOn) · `ServiceCategory` NONE/ASSESSOR (DENTAL IP, ROOM/ICU/DELIVERY_* OP, ADMIN cả hai = NONE; OTHER cả hai = ASSESSOR) · `ChainLineCtx` +VisitKey/CoveredAmount/HoldTermCodes · `ChainCtx` +DiagnosisCode/FacilityName/RuleSetRef/LossTypeValue/Terms*/CoveredTotal · Step05: NONE/ASSESSOR không hold không decline, accident redirect → 500 · Step04 viết lại (6 predicate có tên + guard BR-CHAIN-1, bỏ heuristic). Run `066772bc…`. | Model/seed/Step05 OK; **Step04 19 lỗi** "Invalid Server Action Flow" — ForEach lồng nhau nhiều back-edge ("Ambiguous paths to For Each") |
| E2 | Tách Step04 thành 1 vòng lặp/action: `A5Term_InScope` (Server Action thuần, 2 output nên không là Function: `Start → Assign(Known, InScope) → End`) · `A5Step04_FindCheckpoint` (`AssignInit → ForEach(Checkpoints) → If(match) → AssignStatus+Found → I++ → ForEach / exit End`) · `A5Step04_EvalTerm` (`AssignInit → ForEachLine → IfLineDeclined → True: AssignSkipLine → JoinLine / False: CallInScope → IfUnknown → True: AssignUnknown → JoinLine / False: IfInScope → False: AssignNotInScope → JoinLine / True: AssignMatched → CallFindCp → IfUpheld → True: AssignExcl → AppendCut → JoinLine / False: IfCleared → True: AssignClearedLog → JoinLine / False: AssignHold → JoinLine; JoinLine → ForEachLine; exit AssignTermLog → End`) · `A5Step04_GeneralExclusions` (`IfDeclined → True: AssignSkip → ForEachSkipSum → AssignSkipRes → EndSkip / False: IfNoRSR → True: … HOLD / False: GetTermsCount → GetTerms → GetCheckpoints → AssignInit → ForEachTerm { CallEvalTerm → AssignAfterTerm } → ForEachSum → AssignSum → AssignFinal → End`). Run `7460da2f…`. | **0 lỗi**, 68 warning |
| F | `A5Step06_LineCoverTest`: `IfDeclined → True: skip / False: GetRecs (Recommendation ⋈ Assessment where Assessment.ClaimId) → GetPBS (PolicyId) → AssignInit → ForEachLine { IfAlreadyDeclined → True: AssignSkipLine → JoinLine / False: IfNONE → True: AssignNONE(NOTCOV) → AppendCutNONE → JoinLine / False: IfASSESSOR → True: AssignASSESSOR(Held) → JoinLine / False: IfEmpty → True: AssignEmpty → JoinLine / False: CallHasBenefit → IfNOBEN → True: AssignNOBEN → AppendCutNOBEN → JoinLine / False: CallFindRec → IfRecFound → False: AssignRecNotFound → JoinLine / True: IfConfirmedDecline → True: AssignConfirmedDecline → AppendCutRec → JoinLine / False: AssignRecPending → JoinLine; JoinLine(CoveredAmount = Running) } → ForEachCoveredSum → AssignFinal → End`. Helper `A5Step06_FindRec` / `A5Step06_HasBenefit` (1 loop). **Seed `Recommendation.Decision = ""`** (đề xuất, chưa xác nhận); giá trị xác nhận của UI = `"Confirmed"` (DecideRecommendation). FK: Recommendation.AssessmentId → Assessment.Id, Assessment.ClaimId → Claim.Id. `A5Step07_BenefitProvisos`: `IfDeclined → … / False: GetBenefitTerms (ACTIVE, Scope BENEFIT/BENEFIT_SUBTREE, RuleSetRef) → IfNoTerms → True: SKIP log "0 điều khoản…" / False: ForEachTerm { AssignHoldTerm } → HOLD`. DryRun `… AssignS05 → CallS06 → AssignS06 → CallS07 → AssignS07 → End`. Run `ec5955cd…`. | **0 lỗi**, 69 warning |
| G | Chuỗi seed thật: `LimitType` ∈ {`ANNUAL`→AnnualLimit, `PER_VISIT`→PerVisitCap, `PER_DAY`→PerDayCap, `PER_EVENT`→PerEventCap}; `CountUnit` ∈ {`""`, `VISIT` (200.3, CountLimit 20), `DAY`}. `LimitLedger` +UsedCount/CountUnit (Seed_LimitLedger 15 assign + Seed_AssessmentAndTerms 3 assign vá). `ChainLineCtx` +GroupKey. `A5Step08_GetBenefitLimit` (1 loop) · `A5Step08_ApplyGroupCap`: `AssignInit(Used=0) → ForEach(Lines) { IfInGroup → False: skip / True: AssignAllowed(GroupSum+=, Allowed=Min(Running, Cap−Used)) → IfCut(Running>Allowed) → True: AssignCut(CutAmount+=, Running=Allowed, Used+=) → AppendCut / False: AssignNoCut(Used+=Running) → JoinLine }` · `A5Step08_SubLimit`: `IfDeclined → … / GetPBS → AssignInit → ForEachPass1 { IfRealBenefit → CallGetBL → AssignGroupKey } → ForEachPass2 { IfHasGroupKey & chưa xử lý → CallGetBL2 → CallApplyGC → AssignAfterCap(ProcessedKeys+=, HasCut) } → ForEachSum → AssignFinal(CUT/PASS) → End` · `A5Step09_CountVisits` (distinct VisitKey) · `A5Step09_DeclineExcessVisits` (VisitIdx > Remaining ⇒ LIMIT) · `A5Step09_CountLimit`: `GetPBS → ForEachLines { IfRealBenefit chưa xử lý → CallGetBL → IfUnlimited(CountLimit=0) → log / IfVISIT → CallCountVisits → GetLedger(Member, Policy, Benefit, Year) → IfWithinLimit → log / CallDeclineEV → AssignAfterDecline / DAY → AssignDAY(Held, Checkpoints) } → ForEachDAYHold → ForEachSum → AssignFinal(CUT/HOLD/PASS)`. DryRun `… AssignS07 → CallS08 → AssignS08 → CallS09 → AssignS09 → End`. Run `b1a50b5a…`. | **0 lỗi**, 69 warning. **Publish rev 142** |
| G2 | **Rev 142 runtime:** dry-run 17 → HTTP 250, log `OS-BERT-60000 Detected a recursive iteration over the same list` tại `A5Step08_ApplyGroupCap` ← `A5Step08_SubLimit` (ForEachPass2 trên `Ctx.Lines` đang mở trong khi helper lặp `OutCtx.Lines` — cùng một list lúc runtime). Bước 9 cùng hình. Sửa: pass 1 gom khoá vào **list Text riêng**, pass 2 lặp list khoá rồi gọi helper. Run `414a7a83…` (Step08 xong rồi provider lỗi `OS-AIWA-50002`; run `cb1771b4…` lỗi provider, không đổi gì). **Bẫy ghi nhớ:** helper nhận Structure chứa list và lặp list đó KHÔNG được gọi từ bên trong ForEach của chính list ấy. | Step08 sửa, 0 lỗi. **Publish rev 143** |

### Dry-run rev 143 — hồ sơ 0001 (Id 17), 21/09 14:35 — **báo cáo mốc 9 bước** (Administration › "Chain dry-run (A5 steps 1–9)")

```
01 PASS 3128000 — Thành viên có hiệu lực từ 01/01/2026; hợp đồng HC-STD 01/01/2026–31/12/2026; phí đã ghi nhận; ngày điều trị 10/06/2026
02 PASS 3128000 — [BR-WAIT-5] Kỳ chờ 30 ngày cho 200/GEN, tính từ 01/01/2026 ⇒ đủ điều kiện từ 31/01/2026; ngày điều trị 10/06/2026 — đủ
03 SKIP 3128000 — Không phải bệnh có sẵn — bước 3 không áp dụng
04 HOLD 3128000 — 10 điều khoản trong sổ · 10 ACTIVE đã kiểm · 9 không thuộc phạm vi · 1 checkpoint · 0 EXCL
                  TERM-HC-001/002/004/005/006/007/008/009/010 — KHÔNG_THUỘC_PHẠM_VI · TERM-HC-003 — checkpoint dòng 6 (TBC-03 chưa chốt)
05 PASS 3128000 — 6 dòng → 200.1, 200.1, 200.1, 200.2, 200.2, 200.2
06 PASS 3128000 — 6 dòng thuộc phạm vi bảo hiểm = 3128000 ₫; 0 từ chối; 0 chờ; dòng 4: khuyến nghị NOTREL đang (chưa quyết định) — chưa áp (chờ người xác nhận)
07 SKIP 3128000 — 0 điều khoản cấp quyền lợi trên sản phẩm này — bước tồn tại, tập rỗng (D1 B-23)
08 CUT  2500000 — 200.1 |2026-06-10|Vinmec: 2110000 so trần 1500000 — cắt 610000; 200.2 |…: 1018000 so trần 1000000 — cắt 18000
09 PASS 2500000 — 200.1: không giới hạn lượt; 200.2: không giới hạn lượt
Outcomes 01:PASS;02:PASS;03:SKIP;04:HOLD;05:PASS;06:PASS;07:SKIP;08:CUT;09:PASS · TotalAfter 2.500.000
```

Đối chiếu (spec trên đĩa, không phải code):
- Bước 4: TERM-HC-003 checkpoint trên dòng 6 SUPPLEMENT ✅ (A5 §6.2, T-CH04-a: CHECKPOINT không auto-decline; tiền chưa cắt — chỉ cắt khi checkpoint UPHELD). **Lỗi:** header `m = 10 ACTIVE` — 4 dòng DRAFT (#5 #8 #9 #10) đang **hiện với engine** (T-CH04-d đòi m = 6; A5 §7.2 "draft terms are invisible"); và guard BR-CHAIN-1 cho selector chưa cài không bắn (in KHÔNG_THUỘC_PHẠM_VI thay vì checkpoint). Đang sửa (lượt G3).
- Bước 6: NOTREL dòng 4 (Atorvastatin) **chưa áp** vì `Decision = ""` — đúng T-CH06-b ("Proposed → không áp, G3 fail"). CoveredTotal 3.128.000 ✅ (T-CH06-c: trước cap/copay).
- Bước 7: tập rỗng, vẫn render ✅ (T-CH07-a).
- Bước 8: 200.1 (300.000+1.400.000+410.000 = 2.110.000) → trần 1.500.000, cắt 610.000; 200.2 (168.000+700.000+150.000 = 1.018.000) → trần 1.000.000, cắt 18.000; **2.500.000** = hand-run lối C run #1 (`batchB2-spec.md` §Kiểm số) ✅; cap theo benefit con, một visit (cùng ngày, cùng cơ sở) ✅ (T-CH08-a/c). Cosmetic: nhãn nhóm 2 in "200.1 200.2|…" (biến sai) — sửa G3.
- Bước 9: CountLimit 0 = không giới hạn ✅ (T-CH09-b).
- Sau bước 9 = 2.500.000 ⇒ bước 10 copay 20 % sẽ ra 2.000.000 = đích run #1 của deck — chưa cài, chưa khẳng định.
| G3 | Ba lần liên tiếp `OS-AIWA-50002` trên session `5cb88e55…` (run `cb1771b4…`, `6055fdfb…` — không đổi gì). **Mở session Mentor mới** `9528b433-0541-42fc-9f43-2f022a42d505` (load ClaimMVP latest = rev 143). Lượt H: Step09 lặp list mã benefit riêng + helper `A5Step09_HoldBenefitLines`; chẩn đoán DRAFT (Status seed vs filter GetTerms); guard selector chưa cài; nhãn log Step08; xác nhận AssignCut không set Declined. Run `9c2b4e00…`. **Kết quả:** seed đúng (`Status = "DRAFT"` cho #5 #8 #9 #10); lỗi ở filter `GetTerms` = `IsActive = True and Scope and RuleSetRef` — thiếu `Status = "ACTIVE"` ⇒ sửa, m = 6. Guard: EvalTerm exit ghi đè TermLog bằng KHÔNG_THUỘC_PHẠM_VI khi MatchedCount = 0 và AssignAfterTerm không đếm Unknown ⇒ sửa (EvalTerm + SelectorFact/SelectorOp input; AssignAfterTerm: TermsCheckpoint+1, Held, Checkpoints `[B4] {Code}: selector chưa được cài;`). Step09: pass 1 gom BenefitCodes → pass 2 ForEachBenefit; helper `A5Step09_HoldBenefitLines`. Step08 log dùng `GroupBenefitCodes[J]`; **AssignCut chỉ set Running = Allowed + Cut StepNo 8 CAP_VISIT, không đụng Declined** ✅. | **0 lỗi**, 69 warning. **Publish rev 144** |

## 6. Bộ khung test 71 (21/09 chiều — PO: dựng ngay, song song với bước 10–14) — thiết kế ở `test-harness.md`

**Xác nhận rev 144, T-CH04-d:** header bước 4 = `10 điều khoản trong sổ · 6 ACTIVE đã kiểm · 5 không thuộc phạm vi · 1 checkpoint · 0 EXCL` ⇒ n = 10, m = 6, 5+1+0 = 6 ✅. Nguyên nhân gốc (README §7 rule 10): `IsActive` (cột kỹ thuật, True mọi dòng) ≠ `Status` — filter không loại gì trông y hệt filter đúng.

| Lượt | Nội dung | Kết quả |
|---|---|---|
| I1 | Structure `ChainStepResult` · `A5Chain_DryRun` + output `StepResults` (AssignS01…09 → AssignStepRow → ListAppend) · Entity `ChainTest` (23 attr) · `Seed_ChainTests` (xoá hết → A → B → C) · `Seed_ChainTests_A` 23 dòng (rows 1–23, text nguyên văn từ `expected-71.md` qua `chain-tests-71.json`) — Mentor phải chia nhỏ vì hết bộ nhớ. Run `ae2b8b06…`. | 0 lỗi |
| I2 | `Seed_ChainTests_B` 23 dòng (24–46). Run `441b9911…`. | 0 lỗi |
| I3 | `Seed_ChainTests_C` 25 dòng (47–71) · `RunSeedBatch0Now`: … → BatchB2Remap → UpdBedB → UpdBedC → **CallSeedChainTests** → LogDone. Run `8c261c27…`. | 0 lỗi |
| I4 | `Seed_ChainTestFixtures` (+ `Seed_CopyPBS`, `Seed_FixtureClaim`, `Seed_FixtureLine`): 3 policy FX-*, 5 member FX-M*, 28 hồ sơ `TST-*` theo `test-harness.md` §2–3; prompt lưu `mentor-prompt-fixtures.md`. Run `9c2de798…` — helper + delete + policy + member xong, đứt giữa claims part 1 (`OS-AIWA-50002` sau ~12 phút, changeApplied, 0 lỗi). | một phần |
| I4b | Tiếp: main đã có 14 claim (T-CH01-a…T-CH05-a); thêm `Seed_ChainTestFixtures_Part2` (T-CH05-b…T-CH08-a 9 + T-CH08-b/08-c/09-a 3) và `_Part3` (T-CH09-b 40 dòng, T-CH09-c) — tổng **28** ✅. Record: ConditionClass `GEN/PRE/BIR/ACC`, RenewalStatus `FirstTime`/`Renewing` (FX-M4). Run `5a4bb0b4…`. | 0 lỗi, 83 warning |
| I5 | `RunChainTests` (+ `ChainTest_FindStep`, `ChainTest_Assert`) · màn `ChainTests` (menu Technical hand-over › Chain tests); prompt `mentor-prompt-runner.md`. Run `67f5fcb7…`. | 0 lỗi, 88 warning. **Publish rev 145** (`ca37b7e9…`, Finished) |
| **Run #1** | Runtime rev 145: Re-seed 71 rows → Seed fixtures → Run runnable tests. | **26/29 pass, 3 fail** (T-CH04-e, T-CH05-e, T-CH09-a) — chi tiết `test-run.md` |
| K | Sửa sau run #1: chẩn đoán T-CH04-e (IcdCode fixture / Ctx.DiagnosisCode / predicate #1) · log accident redirect bước 5 · wording bước 9 "vượt hạn mức (LIMIT)" · Reason nêu tên mảnh thiếu · header counters PENDING/MANUAL + last run · xác nhận RunSeedBatch0Now → Seed_ChainTests. Run `dd15cc8b…` (~25 phút). **Nguyên nhân gốc T-CH04-e = lỗi engine thật:** `Substr` OutSystems **0-based** ⇒ `Substr(DiagnosisCode, 1, 3)` trả `"41."` ≠ `"Z41"` — predicate #1 (Z41) **và #7 (F1)** chưa bao giờ khớp; sửa start = 0. Fixture IcdCode Z41.1 đúng; LoadContext lấy Claim.IcdCode đúng. T-CH05-e: log redirect dùng MappedCode thay Category, thiếu chữ "redirect" ⇒ đổi thành `<Category> → 500 (redirect tai nạn — mất sub-limit/count/copay của <MappedCode>)`. Step09 vượt hạn: `<code>: <n> lượt mới, đã dùng <used>/<limit> — <k> lượt vượt hạn mức, từ chối <k> dòng (LIMIT)`. Assert Reason nêu tên mảnh. Counters: mọi aggregate đếm có **MaxRecords = 1** ⇒ Count ≤ 1 → 50000; RunTests refresh thêm Pending/Manual/LastRunAt. Mục 6 xác nhận không đổi. | **0 lỗi**, 88 warning. **Publish rev 146** (`2923b452…`) |
| **Run #2** | Runtime rev 146. | **28/29 pass** — còn T-CH05-e: log bước 5 "1 dòng → 100.1" ⇒ accident redirect không bắn (lỗi engine/fixture, không phải wording) → chẩn đoán J-b |
| J | `A5Step10…13` + helpers (`ResolveCopay`, `GetDeduction`, `FindAnnualNode`, `ApplyAnnualCap`) + DryRun tới 13; prompt `mentor-prompt-steps10-13.md`. Run `c7e55228…`. | **Đứt WebSocket** `OS-IDE-CONVERSATIONAL-50035` ở phút ~22 khi nối pass 2 Step12 + DryRun; changeApplied, 0 lỗi, 91 warning — một phần |
| J-b | Tiếp J: kiểm kê, hoàn tất Step12 pass 2 / Step13 / DryRun 10–13 · chẩn đoán T-CH05-e (LossTypeId fixture, LossType.Value, Ctx.LossTypeValue, điều kiện redirect). Run `9779005d…` (~25 phút). Kiểm kê: Step10 ResolveCopay/CoPayment, Step11 GetDeduction/DeductionRatio, Step12 ApplyAnnualCap đã có; **tạo** FindAnnualNode, ParentAnnualCap (pass 1 + 2), Step13; **nối** DryRun 10–13 (CallS→AssignS→AssignRow→AppendS ×4 → FinalAssign; TotalAfter = bước 13); heading Administration "steps 1–13". T-CH05-e: fixture LossTypeId = AccidentInpatient ✅, Value `ACC_IP` ✅, LoadContext lấy LossType.Value ✅ — **lỗi = `Substr(LossTypeValue, 1, 3)` = "CC_"** (0-based, lần thứ ba cùng lỗi) → sửa start 0. ⚠ FindAnnualNode Mentor tự cài = lấy `RootBenefitCode` trực tiếp (không đi cây theo CapModeId = Own + ANNUAL) — đúng trên HC-STD/PLS/PRM (mọi gốc Own, mọi con Included) nhưng lệch batchB2-spec "đọc CapModeId trước" → sửa ở lượt I6. | **0 lỗi**, 89 warning. **Publish rev 147** (`be0bde0d…`). **Runtime rev 147: `A5Step05_BenefitMapping` ném `OS-BERT-60202 Index 1 is out of range`** — bộ test chặn ở bước 5 → K3 |
| I6 | Fixtures Part4 (7 hồ sơ TST-T-CH10-a…12-c, FX-M6/M7 + LimitLedger 200 = 19.000.000) · lật 11 dòng 9.1 bước 10–13 sang RUNNABLE/MANUAL với assertion · viết lại `A5Step12_FindAnnualNode` theo CapModeId. Prompt `mentor-prompt-fixtures-10-13.md`. Run `053ef5aa…`. | **`OS-AIWA-50002`** sau ~22 phút, không áp gì |
| K3 | Sửa `OS-BERT-60202` ở Step05 (prompt `mentor-prompt-k3.md`). Run `4ba002d9…` trên session cũ. | **`OS-AIWA-50002`** sau ~33 phút — session `9528b433…` đóng; Mentor báo "A lot of people are currently using Mentor" 3 lần khi load asset |
| K3' | Session mới `f59176bb-cbb3-45d1-b101-caea82b0fa9e` (load rev 147). Run `4a8db439…`. Mentor báo: nhánh redirect đọc `Ctx.Lines[I].Category` **sau** `I = I + 1` → đảo thứ tự. | 0 lỗi — **nhưng publish (`83a4b73e…`) không sinh rev mới** (app_revisions vẫn 147) và runtime 09:25 vẫn ném cùng lỗi ⇒ "sửa" là no-op. Lỗi bắn ngay ở TST-T-CH01-a (1 dòng, không tai nạn) ⇒ `Lines[1]` trên list 1 phần tử — không nằm ở nhánh redirect |
| I6-A | Fixtures Part4: FX-M6/M7 + LimitLedger 200 = 19.000.000; 7 hồ sơ TST-T-CH10-a…12-c; T-CH11-b IsInNetwork = False bằng UpdateClaim sau Seed_FixtureClaim; Part2 input đổi tên (MemberIdM5/M6/M7). Run `5b14cc39…`. | 0 lỗi, 86 warning (chưa publish) |
| K3'' | Liệt kê mọi `Lines[` trong Step05, bỏ counter, dùng `Lines.Current` (prompt `mentor-prompt-k3b.md`). Run `f0d67d5a…`. Step05 cũ: **hai** ForEach trên Ctx.Lines dùng chung counter I (reset 0 giữa hai vòng), nhánh redirect đọc `Lines[I]` sau `I + 1`; viết lại toàn bộ trên `Lines.Current`. | 0 lỗi, 86 warning. **Publish rev 148** (`a93ae55c…`, gồm I6-A). **Run #3: 29/29 pass** |
| I6-B | Lật 11 dòng 9.1 bước 10–13: 9 RUNNABLE (T-CH10-a/b/c, 11-a/b, 12-a/b/c, 13-a) + 2 MANUAL (11-c/d) với assertion (`mentor-prompt-fixtures-10-13.md` Part B). Run `1bf25b7e…`. | 0 lỗi. RUNNABLE = 23 + 15 = **38** (tôi tính nhầm 39). **Publish rev 149** (`98ff847e…`) |
| I6-C | `A5Step12_FindAnnualNode` theo CapModeId = Own + ANNUAL (đi Benefit → Parent → Root), log nút theo spec · bước 10–13 nhánh SKIP trả Σ Running thay vì 0. Run `a87581b2…`. FindAnnualNode: một vòng snapshot, OwnQual/ParentQual/RootQual + limit, chọn theo thứ tự; Step10/11/12 skip path sửa, Step13 đã đúng. | 0 lỗi, 85 warning. **Publish rev 150** (`99e9a20d…`) |
| **Run #4** | Runtime rev 149 (38 RUNNABLE). | **37/38** — fail T-CH10-c (log thiếu "từ 300"); **T-CH12-c CUT · 0** với log "100: dùng 0/0 — còn 0 — cắt 2000000" và T-CH11-a/T-CH13-a = 0 ⇒ nghi pass 2 Step12 đọc `LimitLedger.RemainingAmount` (không có dòng ledger ⇒ 0 ⇒ cắt sạch); hồ sơ 0001 "đúng" chỉ vì có ledger seed. Chi tiết `test-run.md` |
| I6-D | Log copay nêu nguồn "(từ <SourceCode>)" · kiểm/sửa Used/Remaining pass 2 Step12 (UsedAmount, AnnualLimit − Used, không đọc RemainingAmount) · siết assertion T-CH11-a/13-a = 240.000, T-CH12-c PASS 2.000.000 "trong hạn". Run `2d4b97ba…`. **Xác nhận lỗi:** pass 2 cũ `AnnualLimit = ledger.RemainingAmount + ledger.UsedAmount`, `Remaining = ledger.RemainingAmount` ⇒ không có dòng ledger ⇒ 0 ⇒ **cắt sạch mọi hồ sơ không có ledger** (hồ sơ 0001 thoát vì có ledger seed). Mentor "sửa" vẫn lấy AnnualLimit từ ledger → chưa đạt. Log copay + 3 assertion OK. | 0 lỗi (chưa publish) |
| **Run #5** | Runtime rev 150. | 37/38 — T-CH11-a nay 240.000 ✅ (skip path), T-CH12-c vẫn CUT · 0, T-CH13-a = 0 (cùng nguyên nhân pass 2) |
| I6-E | Pass 2: AnnualLimit lấy từ list thu ở pass 1 (FindAnnualNode), Used = UsedAmount hoặc 0, Remaining = AnnualLimit − Used; không đọc RemainingAmount. Run `2fc3813c…`. Pass 1 thêm `NodeLimits` (List Decimal) song song `NodeCodes`; pass 2: `AnnualLimit = NodeLimits[J]`, `Used = If(GetLedger.List.Length > 0, …UsedAmount, 0)`, `Remaining = AnnualLimit − Used`. | 0 lỗi. **Publish rev 151** (`b7a6f1ee…`, gồm I6-D). **Run #6: 38/38 pass** |
| J2-a | `LedgerPosting` entity · ChainRun +DurationMs (+ thiếu) · ChainCtx.StepResults · `A5Step14_PostLedger` (ReverseOld → pass 1 AmountPayable/Total + gom list → pass 2 `A5Step14_PostLine` AMOUNT lên benefit + tổ tiên (BR-TREE-2) → OCCURRENCE theo VisitKey cho CountUnit VISIT → DAY không post (Q-IDP-06) → UpdateClaim.TotalPayable → CalcSnapshot 14 dòng). Run `1eee190b…` (~36 phút). ChainRun/CalcSnapshot/ChainCtx.StepResults đã đủ attribute; `LedgerPosting` tạo. | 0 lỗi, 90 warning. **Publish rev 152** (`1320b099…`) |
| J2-b | `RunAdjudicationChain(ClaimId, TriggeredBy)` (+ `A5Chain_InputsHash`) · `A5Ledger_ApplyPosting/Finalize/Reverse` · `Seed_AssessmentAndTerms` bỏ ChainRun/CalcSnapshot · `RunSeedBatch0Now` gọi chuỗi trên CLM-2026-0001 · nút "Run chain (writes)" trên Administration. Prompt `mentor-prompt-j2b.md`. Run `9bb329fb…` (~57 phút). 5/6 xong; **`Seed_AssessmentAndTerms` cleanup bị bỏ** ("action too large for in-place edit" — script hết bộ nhớ) → lượt riêng. `StepOutputsJson` = text phẳng (JSONSerialize không có trong Mentor) — vẫn so sánh được cho T-CHAIN-03. | 0 lỗi, 110 warning. **Publish rev 153** (`e3e6ea25…`) |
| **Run #7** | Runtime rev 152 (sau J2-a). | **38/38** — bước 14 không ảnh hưởng dry-run |
| Runtime 153 | Administration › "Run chain (writes)" hồ sơ 17: screen action trả về sau **422 ms** (đo từ trình duyệt, PerformanceObserver) nhưng **ném `22001: value too long for type character varying(64)`** ở `CreateChainRun` — `ChainRun.InputsHash` Text(64) quá ngắn cho chuỗi InputsHash phẳng. | → J2-c |
| J2-c | `ChainRun.InputsHash` → 2000; kiểm StepOutputsJson/Note/Detail. Run `55a37978…`. Chỉ InputsHash cần đổi (StepOutputsJson unbounded, Note 300, Detail 500). | 0 lỗi. Publish `619225c3…` **thất bại `OS-DPL-50210`** (fetch application configurations) sau ~9 phút → publish lại `a6f3bca5…` = rev 154 |
| J2-d | Xoá riêng các node Create/Delete ChainRun/CalcSnapshot trong `Seed_AssessmentAndTerms` (nối tắt predecessor → successor). Run `43768466…`. Xoá được 3 node CalcSnapshot hồ sơ A (CrCSA7/8/9) rồi **hết bộ nhớ sandbox 500 MB** (action ~200 node, mỗi lần duyệt `.Nodes` nạp cả graph). Mentor kết luận: sửa tay trong Service Studio hoặc tách action. | 0 lỗi (một phần). **Việc còn lại cho PO/Studio:** bỏ nốt CreateChainRun/CalcSnapshot của hồ sơ B/C/D trong `Seed_AssessmentAndTerms` |

### Readout rev 148 — hồ sơ 0001 (Id 17), dry-run 13 bước, 21/09 16:33

```
Outcomes 01:PASS;02:PASS;03:SKIP;04:HOLD;05:PASS;06:PASS;07:SKIP;08:CUT;09:PASS;10:CUT;11:SKIP;12:PASS;13:SKIP · TotalAfter 2.000.000
08 CUT  2500000 — 200.1 … 2110000 so trần 1500000 — cắt 610000; 200.2 … 1018000 so trần 1000000 — cắt 18000
09 PASS 2500000 — 200.1: không giới hạn lượt; 200.2: không giới hạn lượt
10 CUT  2000000 — Đồng chi trả: 500000 ₫; dòng 1 200.1 copay 20% = 60000; dòng 2 200.1 = 240000; dòng 3 200.1 = 0; dòng 4 200.2 = 33600; dòng 5 200.2 = 140000; dòng 6 200.2 = 26400
11 SKIP 0       — KHÔNG_ÁP_DỤNG — cơ sở trong mạng lưới, không điều kiện khấu trừ nào đúng
12 PASS 2000000 — 200: dùng 2000000/20000000 — còn 18000000
13 SKIP 2000000 — KHÔNG_ÁP_DỤNG — sản phẩm không có trần toàn plan; bước tồn tại cho sản phẩm tương lai
```

- **Bước 10 = 2.000.000 = mốc PO** (run #1 lối C: copay 20 % × 2.500.000 = 500.000; 200.1: 300.000 = 20 % × 1.500.000 ✅; 200.2: 200.000 = 20 % × 1.000.000 ✅). Base là số sau bước 9, không phải billed (T-CH10-a catch).
- Bước 11 trong mạng ⇒ KHÔNG_ÁP_DỤNG (T-CH11-a). Ghi chú "quy tắc tạm B-19" chỉ xuất hiện ở nhánh ngoài mạng (T-CH11-b / T-CHAIN-02) — kiểm sau I6-B.
- **Lỗi nhỏ:** bước 11 SKIP trả `ResultAmount = 0` thay vì Σ Running (2.000.000) — F48 sẽ hiện 0 ở dòng 11. Sửa ở lượt J2 (cùng step 14). Log bước 12 "200: dùng 2000000/20000000 — còn 18000000" đọc `LimitLedger.UsedAmount` seed của hồ sơ 0001 (2.000.000, batch B) — wording khác spec ("{Σ} so hạn mức còn {Remaining} (đã dùng {Used}/{AnnualLimit}) — trong hạn"); kiểm T-CH12-a sẽ cho biết nhánh cắt có nêu "còn"/"cắt".

### Readout rev 154 — `RunAdjudicationChain(17, "admin")` thật, 21/09 17:04–17:07

| Lần | ChainRunId | TotalPayable | Outcomes | Round-trip trình duyệt (screen action `ActionRunAdjudicationChain`) |
|---|---|---|---|---|
| 1 | **104** | **2.000.000 ₫** | `01:PASS;02:PASS;03:SKIP;04:HOLD;05:PASS;06:PASS;07:SKIP;08:CUT;09:PASS;10:CUT;11:SKIP;12:PASS;13:SKIP;14:POSTED` | **811 ms** (lạnh) |
| 2 | **105** | **2.000.000 ₫** | y hệt | **279 ms** (ấm) |

- `DurationMs` trên ChainRun = 0 vì `DiffSeconds` (độ phân giải giây) ⇒ chuỗi chạy < 1 s. Trace ODC (`app_traces`) cho run lỗi lúc 09:58 UTC: `RunAdjudicationChain` ServerAction **338,98 ms**, 84 span (tới CreateChainRun). **Trace ODC run 104 (10:04:54 UTC, rev 154): `RunAdjudicationChain` ServerAction = 719,22 ms, 370 span, 0 lỗi, traceId `993e6406…`** (lạnh; round-trip trình duyệt 811 ms). Run 105 (ấm) round-trip 279 ms — trace chưa index.
- Hai lần chạy cùng input ⇒ cùng 14 outcome, cùng tổng (T-CHAIN-03 xác định tính: còn phải so `InputsHash`/`StepOutputsJson` byte-for-byte — làm khi lật T-CHAIN-03 sang RUNNABLE).
- Bằng chứng: `output/bnk-dev-claimmvp/shots-rev154/admin-run-chain-claim17-chainrun104.png`.
- **Lỗi UI phát hiện:** F48 tab "Calculation & ledger" vẫn render CalcSnapshot 9 bước BRD của **RunNo 1 (seed)** dù panel gate đã trích log bước 8 của chuỗi A5 ⇒ tab ghim RunNo 1 thay vì run mới nhất → lượt K4.

| K4 | F48: `GetCalcRun` sort RunNo **ascending** max 1 (luôn = seed RunNo 1) → desc; `GetCalcSteps` theo ChainRunId mới nhất, 14 dòng theo StepNo; card "RUN #<RunNo> (<TriggeredBy>)" + RunAt + DurationMs (trước: text cứng "Run #1 (system)"). Run `12928dd0…` (~50 phút). | 0 lỗi, 110 warning. **Publish rev 155** (`1c05fb23…`) |

| Runtime 155 | F48 hồ sơ 17 hiện **RUN #3 (ADMIN)** với B01…B13 tiếng Việt + số chạy từng bước (2.500.000 sau B08, 2.000.000 từ B10). **Hồi quy:** (a) thiếu B14 — Step14 ghi CalcSnapshot từ `Ctx.StepResults` trước khi append dòng 14 của chính nó; (b) card run "0 ₫ · 0 ms"; (c) tổng Settlement Claimed/Payable/Customer pays = 0/0/0 (trước K4: 3.128.000/2.000.000/1.128.000). | → K5 |
| K5 | Step14 append dòng 14 trước khi ghi snapshot; card = Claim.TotalPayable + "< 1 s" khi DurationMs = 0; tổng Settlement = TotalClaimed / TotalPayable / hiệu. Run `775b3602…`. Trước: card đọc `ChainRun.TotalPayable`, footer đọc `ChainRun.TotalRequested` — hai attribute BRD cũ chuỗi không bao giờ ghi ⇒ 0; RunAdjudicationChain append dòng 14 sau khi Step14 đã ghi snapshot (node `ApS14`, xoá). | 0 lỗi, 110 warning. **Publish rev 156** (`78a41d43…`) |
| Runtime 156 | `RunAdjudicationChain(17)` → **ChainRun 106**, 2.000.000, 14:POSTED (round-trip 707 ms; DurationMs 1000 do vắt qua mốc giây). F48: **RUN #4 (admin) · 2.000.000 ₫ · 21/09/2026 17:18 · 1000 ms**, 14 dòng B01…B14 ("Ghi sổ tạm" 2.000.000), footer 3.128.000 / 2.000.000 / 1.128.000. Ảnh `output/bnk-dev-claimmvp/shots-rev154/f48-claim17-run4-rev156.jpg`. **Run #8 bộ test: 38/38** (17:21). | ✅ |

## 7. T-CHAIN (21/09 tối — PO: T-CHAIN-05 trước, rồi 03, rồi 01/02/04/06/07; T-GATE BLOCKED bởi câu hỏi 3/4 điều kiện)

Session Mentor mới `5ece5edc-a874-46ba-a2c4-25255529e200` (load rev 156).

| Lượt | Nội dung | Kết quả |
|---|---|---|
| L1 | `ChainTest` +Mode/+AssertAmountBefore · fixtures Part5 (TST-T-CHAIN-01/02 + TermCheckpoint UPHELD, 04, 05 = bản sao 0001 + NOTREL Proposed) · lật T-CHAIN-01/02/03/04/05 + thêm T-CH12-d (72 dòng). Run `470e8b23…` (~42 phút). Part5: 4 hồ sơ (39 tổng), TermCheckpoint UPHELD ×2, Assessment+Recommendation NOTREL Proposed cho T-CHAIN-05. | 0 lỗi, 110 warning (chưa publish) |
| L2 | `RunChainTests` Switch theo `Mode`: helper `ChainTest_ConfirmRerun` (chạy thật → Confirmed NOTREL → chạy thật) và `ChainTest_RunTwice` (2 ChainRun: InputsHash/StepOutputsJson/tổng) · Server Request Timeout 60 s cho `RunAdjudicationChain` + `RunChainTests` (task 2b agent-architecture). Prompt `mentor-prompt-l2.md`. Run `016b8d78…` (~27 phút). Switch Mode: CONFIRM_RERUN → `ChainTest_ConfirmRerun` (RunA → UpdateRecommendation NOTREL Confirmed → RunB → RunNo A/B) · RUNTWICE → `ChainTest_RunTwice` (2 run, ChainRun desc max 2, HashSame/StepsSame). **`ServerRequestTimeout` = app-level trong ODC (không có per-action): null (mặc định 10 s) → 60 s** ⇒ task 2b agent-architecture đóng. | 0 lỗi, 110 warning. **Publish rev 157** (`f64e4024…`) |
| L3 | Màn `ChainTests`: `GetAllTests` Max Records 50 → **200** (cả 3 Refresh Data trong ReseedRows/RunTests/SeedFixtures); nhãn "Mốc: 71/71" → "Mốc: 71/71 (+1 bổ sung PO)". Lý do: hàng 68 (T-CHAIN-05) và 72 (T-CH12-d) không hiện trên grid 50 dòng dù đã pass. Run `38a3cec8…`. | 0 lỗi, 110 warning. **Publish rev 158** (`fc41ffa3…`) |

**Readout rev 157 (21/09 21:38) — run #9 = 44/44 pass, 2,47 s round-trip (6 lần chạy thật bên trong):** T-CHAIN-01 SKIP 1.920.000 · T-CHAIN-02 PASS 1.920.000 (B-19) · T-CHAIN-03 SAME 1.920.000 — log "run 1/2: InputsHash giống nhau; StepOutputsJson giống nhau; tổng 1920000 / 1920000; hash = 168|436:CONSULT:300000:2026-07-14;437:LAB:1400000:…;438:DRUG:900000:…;439:SUPPLEMENT:400000:…" · T-CHAIN-04 CUT 532 ("67 + 67") · T-CHAIN-05 và T-CH12-d pass (đếm trong 44/44) nhưng chưa đọc được log vì ngoài grid → L3.

## 8. T-CHAIN-06 / 07 (21/09 đêm — PO: 06 qua runner → 07 fixture L18 → T-CH14 → T-SEED; T-GATE BLOCKED)

Session Mentor mới `09ea19c2-dd44-4f74-b1f4-5916998d3e4f` (load rev 158). Spec trước code: `a5-chain-spec.md` §1.5 (DAY qua A4 L18) + §1.6 (V-546/BR-LEDGER-8), `test-harness.md` §7, tasks 5d-iii…vi.

| Lượt | Nội dung | Kết quả |
|---|---|---|
| M1 | `ChainTest` +FixtureClaimNo2 · fixtures Part6 (FX-M8 + ledger 200 Used 19.000.000; TST-T-CHAIN-06a/06b CONSULT 800.000) · Mode `CONCURRENT` → `ChainTest_Concurrent` (Reverse×2 → run 1 → run 2 → Finalize 1 → run 2 lại → đếm posting → Reverse 1) · lật T-CHAIN-06 (Before 640.000 → After 360.000). Prompt `mentor-prompt-m1-tchain06.md`. Run `bb82ec01…` (~17 phút). `ChainTest_Concurrent` 25 node: RevClaim1 → RevClaim2 → GetUsedBefore(ledger ⋈ Claim, 200, Year(TreatmentFrom)) → RunA1 → RunB1 → GetRunB1 → Finalize1 → GetUsedAfter → RunB2 → GetRunB2 → GetPostingsB1 (REVERSED) → GetPostingsB1Pend (PROVISIONAL) → GetPostingsB2Prov → Teardown Reverse(1) → Log. Switch 3 nhánh + CONCURRENT (GetClaim2 → NOFIX / CallConcurrent → Pass → UpdTestConc → Join). Fixtures Part6 tách action `Seed_ChainTestFixtures_Part6` (gọi từ action chính sau Part5). Row T-CHAIN-06 RUNNABLE/CONCURRENT 640.000 → 360.000. | 0 lỗi, 115 warning. **Publish rev 159** (`fa4b4ee5…`) |
| **Run #10** | Runtime rev 159 (45 RUNNABLE). | **43/45** — T-CHAIN-06 MOVED · 360.000, log máy đúng từng số (`hồ sơ 1 = 640000 → FINAL, ledger 200 19000000 → 19640000; hồ sơ 2 run 1 = 640000; run 2 = 360000; posting run 1 đảo 2, còn tạm 0; run 2 tạm 2`) nhưng ✗ vì row seed AssertAmountBefore 2.000.000; T-CHAIN-05 ✗ `run 1 1880000 ≠ 2000000` (NOTREL vẫn Confirmed). **Gốc:** `Seed_ChainTestFixtures` không xoá gì — 4 bộ TST- chồng nhau (Work list 41 mã / ~150 hồ sơ). Chi tiết `test-run.md` run #10 |
| M2 | T-CHAIN-07: `ClaimLine.TreatmentDays` (A4 L18, 0 = thiếu fact) + `ChainLineCtx.TreatmentDays` (LoadContext copy) · `A5Step09_CheckDayNode` (GetBL → CountUnit/CountLimit → If ≠DAY∨0 → Ok / GetLedger → UsedCount, Remaining, Ok = Days ≤ Remaining, LogLine) gọi 3 lần Benefit/Parent/Root; nhánh DAY của Step09: TreatmentDays = 0 → HOLD Q-IDP-06 như cũ, > 0 → check, vượt → HOLD "[B9] … DAY vượt hạn" · `A5Step14_PostDay` (posting DAY Count = Days lên benefit + tổ tiên CountUnit DAY) + StepLog "…, {k} DAY ({Names})" · `A5Ledger_ApplyPosting` DAY → UsedCount ± Count, CountUnit "DAY" · `RunAdjudicationChain` +output StepResults · fixtures Part7 (`TST-T-CHAIN-07`: LossType.IllnessInpatient, TreatmentType.Inpatient, ICU "1 ngày ICU" 4.000.000, 100.1.1, TreatmentDays 1) · Mode `RUN` · row T-CHAIN-07 (Mentor nối thêm assignment trùng "last wins" → M3 dọn). Prompt `mentor-prompt-m2-tchain07.md`. Run `7619a4ba…` (~25 phút). | 0 lỗi, 116 warning (chưa publish — gộp với M3) |
| M3 | Seed cleanup thật (`ChainTest_DeleteClaimTree`: LedgerPosting → CalcSnapshot → ChainRun → TermCheckpoint → Recommendation → Assessment → ClaimLine → ClaimDocument → Claim; rồi FX- ledger/member/snapshot/policy) · `ChainTest_ConfirmRerun` reset NOTREL → Proposed trước run 1 · row T-CHAIN-06 AssertAmountBefore 640.000 · dọn assignment trùng T-CHAIN-06/07. Prompt `mentor-prompt-m3-seedfix.md`. Run `bce09cac…` (~35 phút, hai lần suýt hết bộ nhớ sandbox trên action seed lớn — xoá/tạo theo lô). **Gốc theo Mentor:** cleanup cũ xoá `Claim` mà chưa xoá LedgerPosting/CalcSnapshot/ChainRun → lỗi FK bị nuốt, ForEach chạy tiếp → không xoá gì bao giờ. Thứ tự mới: LedgerPosting → CalcSnapshot → ChainRun → TermCheckpoint → RecEvidence → Recommendation → Assessment → ClaimLine → ClaimDocument → Claim. Row T-CHAIN-06: Before 640.000 / After 360.000; T-CHAIN-07 mỗi attribute gán đúng một lần. | 0 lỗi, 116 warning. **Publish rev 160** (`62125c38…`, gồm M2) |
| Publish 160 | `62125c38…` và `0c334c2b…` (M2+M3) **đều thất bại `OS-BEW-50000`** ở "Deploying asset" sau khi DB script chạy xong — không phải transient (2 lần). | → M4 |
| M4 | Audit reachability/dangling/duplicate trên Seed_ChainTestFixtures + 6 action mới + RunAdjudicationChain.StepResults + entity. Run `23dacc34…`. Tìm thấy và xoá **ForEach chết + biến dead-path** trong `Seed_ChainTestFixtures` (di sản xoá/tạo theo lô ở M3); các action khác sạch; không trùng tên; StepResults chỉ trên ChainCtx. | 0 lỗi, 111 warning. **Publish rev 161** (`d4a972e1…`) |
| **Runtime 161** | "Seed fixtures" 25 s, **vẫn không xoá** (mỗi mã +1 bản: CH01 ×5, CHAIN-05 ×3, 06a ×2; log app 0 lỗi ⇒ aggregate cleanup trả 0 dòng hoặc không được chạy) → M5. "Run runnable tests" **0/46**: `A5Step09_CountLimit` ném `OS-BERT-60202 Index 1 is out of range` (trace `cee717af…`, qua A5Chain_DryRun ← RunChainTests) — nhánh DAY mới ở M2 lại truy cập `Lines[I]` theo chỉ số (bẫy K3 lặp lại) → M6 | |
| M5 (huỷ) | Session `09ea19c2…` run `5f9a8485…`: Mentor báo cấu trúc `Seed_ChainTestFixtures` "không nhất quán" và định **dựng lại từ đầu** (nguy cơ mất Part 1–5 inline) → **cancel**; sandbox còn 6 lỗi validation → **đóng session không publish**. Rev 161 (M2+M3+M4) là mốc sạch. | Session mới `44484633-9eab-4373-995e-cd4cb3ac64a0` (load rev 161) |
| M6 | Sửa hồi quy `A5Step09_CountLimit` (nhánh DAY truy cập list theo chỉ số) → chỉ dùng `.Current`; audit CheckDayNode/PostLedger/PostDay. Run `60863952…` (~15 phút). Tìm thấy `Ctx.Lines[I].TreatmentDays / .ParentBenefitCode / .RootBenefitCode` trong CallDay1/2/3, CallHoldDay, ifNode5/7 → thay bằng record hiện tại của ForEach; 3 action kia sạch. | 0 lỗi, 111 warning (chưa publish — gộp M5') |
| M5' | Cleanup tách action `ChainTest_CleanupFixtures` (Claim like "TST-%" → DeleteClaimTree; FX- ledger/member/snapshot/policy; out DeletedClaims/RemainingClaims), gọi ngay sau Start của `Seed_ChainTestFixtures` (+out DeletedClaims/CreatedClaims), toast "Đã xoá N hồ sơ TST-, hiện có M". Run `3250457b…` (~12 phút). `ChainTest_DeleteClaimTree` (đã có từ M3): LedgerPosting → CalcSnapshot → ChainRun → TermCheckpoint → Assessment{Recommendation{RecEvidence}} → ClaimLine → ClaimDocument → Claim; helper `ChainTest_DeleteMemberLedger`, `ChainTest_DeletePolicySnapshot`. | 0 lỗi, 111 warning. **Publish rev 162** (`d6f1660f…`) |

## Readout read-only 21/09 ~23:40 (rev 162) — ChainRun/CalcSnapshot

**Giới hạn công cụ, ghi lại để không ai đọc nhầm thành số liệu:** không tool nào đọc được SQL trên DB ODC (`db_query` chỉ template, không trả row ở v1; Mentor không đọc data). Quan sát được chỉ là F48 từng hồ sơ: RunNo lớn nhất + số dòng CalcSnapshot của đúng ChainRunId đó. Đã mở 54 hồ sơ.

| ClaimId | ClaimNo | RUN mới nhất | CalcSnapshot |
|---|---|---|---|
| 17 | CLM-2026-0001 | #5 · SEED · 23:26 · 2.000.000 | **14** ✅ chuỗi thật |
| 18 | CLM-2026-0002 (B) | #1 · TriggeredBy rỗng · 680.000 | **0** |
| 19 | CLM-2026-0003 (C) | #1 · rỗng · 1.680.000 | **0** ⚠️ |
| 20 | CLM-2026-0004 (D) | #1 · rỗng · 0 ₫ | **0** |
| 34–41 | — | không có ChainRun | — |
| 290/293/294/295/296 | TST-T-CHAIN-01/05/06a/06b/07 | #1–#2 · TEST | **14** mỗi cái |
| 255–289, 291, 292 | 37 fixture dry-run | không có ChainRun | — |

**Lần seed 23:26 là Mike bấm, theo đúng chỉ thị PO** ("publish → Re-run demo seed → chạy lại 46 test"). Không có tác nhân lạ nào đụng tenant. *(PO đã hỏi lại "ai chạy seed" — hỏi sai, vì chính PO bảo chạy. Ghi lại để không ai đi điều tra một chuyện không tồn tại.)*

**PHÁT HIỆN CHƯA GIẢI THÍCH ĐƯỢC — hồ sơ C (0003) phải có 9, quan sát 0.**
Lượt xoá 21/09 chỉ bỏ 12 node CalcSnapshot của **B và D**; `CrCSC1–9` còn nguyên (đếm sau = 18 = A 9 + C 9). Vậy mà C ra 0 dòng.
Ba khả năng, chưa cái nào được kiểm: (1) app chưa publish ⇒ đang chạy bản cũ — **khả năng yếu nhất**: bản cũ thì B phải ra 9 và D ra 3, quan sát cả hai đều 0, tức là bản đã sửa ĐANG chạy; (2) `Seed_ResetDemoClaimData` xoá CalcSnapshot **sau** khi seed tạo; (3) `CrCSC*` ghi `ChainRunId` từ biến không được gán ⇒ dòng mồ côi `ChainRunId = 0`, không thuộc run nào nên không đếm được ở F48.
**Không đoán. Kiểm (3) trước** vì nó là đúng dạng hỏng đã gặp hôm qua.

**Dấu vết TriggeredBy:** ChainRun do seed tạo có `TriggeredBy` **rỗng**; chuỗi thật ghi `"seed"`/`"TEST"`. ⇒ dùng `TriggeredBy = ""` làm dấu nhận biết rác seed, rẻ hơn mọi cách khác.

**Chưa quan sát được (nói rõ):** tổng số dòng ChainRun theo ClaimId (F48 chỉ hiện max RunNo — suy ra 16 dòng, **là suy diễn chưa kiểm**) · CalcSnapshot của các run cũ · dòng mồ côi của 185 hồ sơ TST- đã xoá lúc 22:40.

**Batch F:** dropdown hồ sơ ở Technical trace cắt ở 50 dòng (Max Records mặc định) — thiếu TST-T-CHAIN-05/06a/06b/07. Cùng lớp lỗi với grid `ChainTests` đã sửa ở L3.

## 9. Chẩn đoán dữ liệu chuỗi (22/09 — PO: read-only trước, rồi một panel read-only; KHÔNG publish)

Yêu cầu PO: (1) `Seed_AssessmentAndTerms` — node gán biến dùng làm `NewCS.ChainRunId` cho CrCSC1–9 (hồ sơ C) và có được gán ở đâu không; đối chứng hồ sơ A (CrCSA1–9). (2) Panel read-only "Chain data" trên Administration: `ChainRun` group by ClaimId (count, max RunNo, distinct TriggeredBy) · `CalcSnapshot` group by ChainRunId (count) + liệt kê riêng dòng ChainRunId = 0 hoặc không khớp ChainRun nào. Không entity mới, không action ghi.

| Lượt | Nội dung | Kết quả |
|---|---|---|
| N1 (read-only) | Session `db5fbb92-c5ea-483e-b1da-df0624269f76` (load rev 162). Run `4cb8a758…`, không sửa gì (0 lỗi, 103 warning). **Hồ sơ C:** `assignNode56`: `NewCS.ChainRunId = ChainRunIdC` (biến local ChainRun Identifier); `ChainRunIdC` gán **đúng một lần** ở `assignNode55` = `CrCRC.Id` ngay sau `CrCRC` (CreateChainRun, `NewCR.ClaimId = ClaimIdC`) → cùng biến, có gán. **Hồ sơ A (đối chứng):** y hệt — `assignNode24`: `NewCS.ChainRunId = ChainRunIdA`; `assignNode23` = `CrCRA.Id`; **CrCSA1–9 đều còn** (J2-d báo xoá 7/8/9 — không đúng/không persist). **Hồ sơ B:** `CrCRB` tạo ChainRun, `assignNode42–50` gán NewCS (ChainRunIdB) nhưng **không có CreateCalcSnapshot nào**. **Hồ sơ D:** `CrCRD` + 3 Assign bước 1–3, **không có CreateCalcSnapshot**. CreateChainRun: CrCRA/B/C/D (ClaimIdA/B/C/D). CreateCalcSnapshot: CrCSA1–9 (ChainRunIdA), CrCSC1–9 (ChainRunIdC). | đọc xong |
| N2 | Panel read-only "Chain data" trên Administration (tổng · ChainRun theo ClaimId: Runs/MaxRunNo · TriggeredBy theo ClaimId · CalcSnapshot theo ChainRunId · CalcSnapshot mồ côi). Prompt `mentor-prompt-n2-chaindata.md`. Run `819dacc2…` (~22 phút). 7 aggregate: GetChainRunCount · GetCalcSnapshotCount · GetChainRunByClaim (ChainRun ⋈ Claim **inner** — API Mentor không đặt được join With-or-Without) · GetChainRunByTriggeredBy · GetCalcSnapshotByChainRun · GetOrphanCalcSnapshots (ChainRunId = NullIdentifier(), group ChainRunId+ClaimId) · GetOrphanCount. Giới hạn: bảng A bỏ run của hồ sơ đã xoá; "mồ côi" chỉ bắt ChainRunId = 0. | 0 lỗi, 103 warning — **không publish** |
| N3 | Bù giới hạn bằng 2 aggregate không cần LEFT JOIN: `GetChainRunByClaimNoJoin` (ChainRun group ClaimId — dòng có ở đây mà không có ở bảng A = run của hồ sơ đã xoá) · `GetCalcSnapshotMatched` + `GetCalcSnapshotMatchedCount` (CalcSnapshot ⋈ ChainRun inner — ChainRunId có ở bảng B mà không có ở đây = trỏ tới ChainRun không tồn tại). Run `321c3bf8…`. | 0 lỗi, 103 warning — **không publish**; session `db5fbb92…` để mở |

### Đọc code 22/09 (session db5fbb92, rev 162, read-only) — C đúng ở tầng code

`CrCSC1–9` đọc `NewCS.ChainRunId = ChainRunIdC` (assignNode56); `ChainRunIdC = CrCRC.Id` gán đúng **một lần** ở assignNode55 ngay sau `CrCRC`. **Giống hệt hồ sơ A** (assignNode23/24). ⇒ **Giả thuyết mồ côi `ChainRunId = 0` cho hồ sơ C bị loại ở tầng code.** C ra 0 dòng là chuyện **runtime**, không phải cấu trúc.

Trạng thái model hiện tại: **A 9 · B 0 · C 9 · D 0** = 18 node `CreateCalcSnapshot`, 4 node `CreateChainRun` (A/B/C/D). Khớp phép đếm 30 → 18.

**Hai điều lượt này đính chính:**

1. **J2-d báo đã xoá `CrCSA7/8/9` là SAI (hoặc không persist)** — cả 9 node `CrCSA1–9` còn nguyên. ⇒ **30 luôn là con số thật từ đầu**, và lượt xoá 12 node hôm 22/09 là thứ **duy nhất** từng đọng lại. Mọi kết luận dựa trên "đã xoá 3 node" đều phải bỏ.
2. **Warning 111 → 103** trong session này. Chênh 8 khớp với việc bỏ 12 node ⇒ **bằng chứng độc lập** rằng model đang mang bản đã sửa. (Trước nay ta chỉ có lời Mentor tự báo.)

**Giới hạn công cụ (ghi để không ai trách panel):** API Mentor **không đặt được join "With or Without" (LEFT)**, và **không có aggregate function cho "distinct"**. Nên panel dùng cách bù: A/B' là inner join, A'/B là bản không join để dò chênh lệch bằng mắt; A2 group theo `TriggeredBy` thay cho distinct. Bảng **C (mồ côi, `ChainRunId = NullIdentifier()`)** là bảng quan trọng nhất vì inner join không bao giờ hiện được dòng mồ côi.
| N4 (read-only) | PO yêu cầu diff session vs rev 162 trước khi publish, kỳ vọng 2 nhóm (12 CreateCalcSnapshot xoá khỏi Seed_AssessmentAndTerms B:9/D:3 + panel). Run `aff76a22…`. **Diff thật chỉ có 1 nhóm:** Administration +10 aggregate (GetChainRunCount, GetCalcSnapshotCount, GetChainRunByClaim, GetChainRunByClaimNoJoin, GetChainRunByTriggeredBy, GetCalcSnapshotByChainRun, GetCalcSnapshotMatched, GetCalcSnapshotMatchedCount, GetOrphanCalcSnapshots, GetOrphanCount) + container `ChainDataCard` (h3/h4/Expression/TableRecords). **`Seed_AssessmentAndTerms`: 0 thay đổi, 97 node** (có handler AllExceptions → LogError — nguồn nuốt lỗi). Entities/structures/client actions/screens khác/roles/site properties: không đổi. 0 lỗi, 103 warning. **Không publish** — danh sách không khớp kỳ vọng (thiếu nhóm 1). Lưu ý: N4 đếm "3 CalcSnapshot create cho D", N1 nói D chỉ có 3 Assign không có Create — hai lần đọc của Mentor mâu thuẫn về D. | dừng, chờ PO |

### 22/09 — DIFF THẬT vs rev 162: `Seed_AssessmentAndTerms` KHÔNG ĐỔI. Toàn bộ chuyện "xoá 12 node" là bịa.

Lượt diff (run aff76a22, read-only) liệt kê khác biệt giữa session db5fbb92 và **rev 162 đã publish**. Kết quả:

- **Nhóm duy nhất = panel "Chain data"** (10 aggregate + 1 widget trên Administration).
- `Seed_AssessmentAndTerms`: **97 node, y nguyên rev 162.**
- Entity / structure / server action / client action / màn khác / role / site property: **0 thay đổi.**

**Hệ quả — phải đọc kỹ, nó lật nhiều thứ:**

1. **Hồ sơ B và D CHƯA BAO GIỜ có node `CreateCalcSnapshot`.** B chỉ có 9 Assign (assignNode42–50), D chỉ có 3 Assign. Chỉ A và C có Create, 9 + 9 = **18**.
2. ⇒ **"Trước 30, sau 18, lệch đúng 12" là BỊA CẢ HAI ĐẦU.** Không có 30, và không có lượt xoá nào. Model chưa từng đổi. Cả hai ngày 21–22/09 đi tìm và xoá những node **không tồn tại**.
3. ⇒ Mục "đã dọn seed ChainRun/CalcSnapshot cho hồ sơ B và D" trong `tasks.md` và `build-00` §8 là **SAI, đã rút**. Trạng thái thật: seed vẫn tạo **4 ChainRun + 18 CalcSnapshot** (A và C).
4. ⇒ Runtime khớp: B = 0 và D = 0 **vì chưa bao giờ có node**, không phải vì đã dọn. **C = 0 vẫn là chỗ duy nhất chưa giải thích được.**

**PO tự đính chính hai chỗ:**

- Tôi khẳng định lượt xoá 12 node "đọng thật". Không hề. Nó chưa từng xảy ra.
- Tôi viện **warning 111 → 103** làm *"bằng chứng độc lập rằng model mang bản đã sửa"*. **Sai phương pháp** — 103 là baseline của một session khác, không phải số đo trước/sau trên cùng một phép đếm. Đó đúng là thứ rule 15 vừa cấm, và tôi phạm ngay ở tin kế tiếp.

**Nghi phạm mới cho C:** `Seed_AssessmentAndTerms` có handler **`AllExceptions → LogError → End`** — cơ chế nuốt lỗi. Nếu `CrCSC1` ném lỗi thì action vẫn "thành công", C có ChainRun mà không có dòng nào. *(Điểm chưa khớp: D vẫn có ChainRun #1, tức luồng đi qua được C — chưa kết luận. Panel trả lời bằng số.)*

**Mâu thuẫn nội bộ chưa giải quyết:** lượt N1 đọc D = 3 Assign; lượt N4 đọc D = "3 CalcSnapshot create". Hai lần đọc của Mentor nói khác nhau về cùng một chỗ. **Không kết luận bằng đọc code nữa** — chỉ tin số từ panel.
| Publish | PO: "publish đi" → `62a32754…` = **rev 166**. Sửa nhận định: session `db5fbb92…` load **rev 165** (latest lúc mở, sáng 22/09), không phải 162 — rev 163/164/165 do PO publish từ Studio 23:09–23:25 tối 21/09 (trước seed 23:26). Vì vậy N1 thấy B/D chỉ còn Assign không có Create = 12 node PO đã xoá; diff vs 162 đúng là 2 nhóm như PO kỳ vọng; rev 166 = 165 + panel, không đè gì. | chờ publish |
| **Run #12/#13** | Rev 166. #12 45/46 — T-CH01-c ✗ vì `FX-STD-UNPAID` đã thành "premium paid" (data drift sau seed 23:26 của PO); #13 46/46 sau Seed fixtures (xoá 42/hiện 42) + Re-seed rows. Chi tiết `test-run.md`. | ghi nợ: bảo vệ fixture FX- khỏi seed ngoài, hoặc reseed trong RunChainTests |

### 22/09 run #12/#13 (rev 166) — data drift, không phải hồi quy

Run #12 (00:53): **45/46**, ✗ duy nhất `T-CH01-c` (`premiumPaid = false` → kỳ vọng bước 1 FAIL) ra PASS · 300000. Hồ sơ 257 vẫn trỏ `FX-STD-UNPAID`, nhưng policy đó hiển thị *premium paid*.
Run #13 (00:54): **46/46** sau khi bấm "Seed fixtures" (toast *"Đã xoá 42 hồ sơ TST-, hiện có 42"* — cleanup giờ idempotent thật) + "Re-seed 71 rows". **Cùng rev, chỉ đổi dữ liệu ⇒ drift dữ liệu, chuỗi không hồi quy.** `T-CHAIN-03` SAME 1.920.000 · `05` RERUN 1.880.000 · `06` MOVED 360.000 · `07` POSTED 3.000.000 giữ nguyên.

Nghi phạm: seed 23:26 (`RunSeedBatch0Now` / seed policy) đặt `PremiumPaid = True` trên **mọi** Policy, kể cả `FX-STD-UNPAID`. Chưa xác nhận bằng code.

**PO quyết 22/09 — lối (b): harness tự bảo vệ dữ liệu.** `RunChainTests` gọi `ChainTest_CleanupFixtures` + Part 1–7 **trước mỗi lần chạy** (~5 s). Lý do: một bộ test phụ thuộc vào việc ai đã chạy seed gì trước đó thì **không phải bộ test**. Lối (a) — seed batch 0 bỏ qua `PolicyNo like "FX-%"` — vẫn đúng và vẫn nên làm, nhưng nó chỉ chặn **một** kẻ ghi đè đã biết; mai có kẻ khác. **(a) đẩy batch F.**

**Nghiệm thu cho (b), bắt buộc:** sau khi cài, **cố ý làm bẩn** (bấm "Re-run demo seed") rồi chạy suite **hai lần liên tiếp, không bấm Seed fixtures bằng tay**. Cả hai phải 46/46. Nếu không có bước này thì ta mới chỉ làm tay đúng cái mà (b) đáng lẽ tự làm — và chưa biết (b) có chạy không.

**`T-CH01-c` đã làm đúng việc của nó.** Một fixture bị ghi đè âm thầm, và bộ test bắt được. Ghi lại vì ba ngày qua toàn chuyện test xanh nhầm.
| Đọc rev 163/164/165 (read-only, 3 session Mentor load theo revision, đã đóng) | Không có tool diff revision; so sánh **inventory node ghi của `Seed_AssessmentAndTerms`** do Mentor đọc từng revision. **163** (170 node): CrCRA, CrCRC, CrCRD (không có CrCRB), CrCSA1–9, CrCSB1–6, CrCSC1–9, CrCSD1–3. **164** (187 node): +CrCRB, +CrCSB7–9 (B đủ ChainRun + 9 snapshot). **165** (175 node): −CrCSB1–9, −CrCSD1–3 = **đúng 12 CreateCalcSnapshot** (187−12 = 175 khớp); CrCRB/CrCRD giữ. **Nguồn drift PremiumPaid:** `Seed_BatchB_Backfill.assignNode2`: `If PRow.PremiumPaid = False → PRow.PremiumPaid = True → UpdatePolicy` (có ở cả 163 và 165) — chạy là lật mọi policy chưa đóng phí, kể cả FX-STD-UNPAID. Ngoài ra chỉ Seed_ChainTestFixtures gán PremiumPaid (True/False lúc tạo). Phần còn lại của module (screens/entities/site props) chưa so được — Mentor không cho diff, fingerprint tên quá dài để đọc hết. | |

### 22/09 — ĐỌC REV 163/164/165 TỪ NODE THẬT: lượt xoá 12 node CÓ THẬT, publish ở rev 165

Không tool nào diff hai revision. Cách làm: mở 3 session Mentor load đúng 163/164/165, liệt kê node ghi của `Seed_AssessmentAndTerms` và mọi node ghi `Policy.PremiumPaid`. Phần còn lại (màn hình, entity, aggregate, site property) **không so được** — ghi rõ để không ai đọc thành "đã so hết".

| Rev | Thay đổi trong `Seed_AssessmentAndTerms` | Node |
|---|---|---|
| 163 | hồ sơ B: `ChainRunIdB` **được gán** nhưng **không có** `CrCRB`; chỉ 6 `CrCSB` | 170 |
| 164 | **+** `CrCRB`, **+** `CrCSB7/8/9` (B về đủ 9) | 187 |
| **165** | **− `CrCSB1…9` và `CrCSD1/2/3` = đúng 12** · giữ `CrCRB`, `CrCRD`, mọi Assign của B/D, `UpdClaimB/D` | **175** |

⇒ **Rev 165 (21/09 16:25Z) là nơi lượt xoá 12 node thực sự đọng lại và được publish.** Khớp với runtime: B và D có ChainRun nhưng 0 CalcSnapshot.

**PO tự đính chính lần hai — tin trước tôi kết luận "toàn bộ chuyện xoá 12 node là bịa". SAI.** Nó có thật, ở rev 165. Cái sai là lượt diff của session db5fbb92 (báo `Seed_AssessmentAndTerms` 97 node, y nguyên 162). **97 vs 170 vs 175 vs 187 cho cùng một action** ⇒ số node giữa các session **không so được với nhau**, và tôi đã dùng đúng loại số đó để lật một kết luận. Đây là lần thứ ba tôi đổi chiều về cùng một chuyện.

Rev 163 vs 162: không đọc 162, **không kết luận**.

### NGUỒN DRIFT `PremiumPaid` — ĐÃ ĐÓNG ĐINH VÀO MỘT NODE

**`Seed_BatchB_Backfill.assignNode2`** — trong `If PRow.PremiumPaid = False → PRow.PremiumPaid = True → UpdatePolicy`. Có ở **cả 163 và 165**, không phải thay đổi mới. Chạy backfill là lật **mọi** policy chưa đóng phí thành đã đóng, kể cả `FX-STD-UNPAID` ⇒ đúng hiện tượng `T-CH01-c` ✗ lúc 00:53.

Ngoài nó, chỉ `Seed_ChainTestFixtures` gán `PremiumPaid` (True cho FX-STD/PRM, False cho FX-STD-UNPAID lúc tạo). **Không màn hình hay client action nào đụng.**

**PO quyết 22/09 — KHÔNG phải "bỏ qua FX-%". `Seed_BatchB_Backfill` phải NGỪNG GHI `PremiumPaid` hoàn toàn.**
Lý do: hợp đồng của chính batch B là *"chỉ điền null, không tạo/xoá, không đụng tiền"* (`verify-entity-vs-build01/batchB-spec.md`). `False` **không phải null** — nó là một dữ kiện nghiệp vụ có nghĩa: *chưa đóng phí*. Lật nó thành True là backfill **tự phá hợp đồng của mình** và ghi đè sự thật nghiệp vụ. Lọc `FX-%` chỉ che triệu chứng cho fixture, trong khi lỗi vẫn còn với mọi policy thật.
Đây không chỉ là chuyện test: bước 1 của chuỗi kiểm `premiumPaid`. Một backfill lật cờ đó là **xoá mất nhánh từ chối `PERIOD`/chưa đóng phí trên toàn hệ**.
| Đọc panel + log (22/09 ~01:20) | Panel rev 166: **17 ChainRun · 144 CalcSnapshot · Khớp 144 · Mồ côi 0**. Bảng B = bảng B' (11 ChainRunId có dòng: 166→9, 168→9, 170/179–186→14). 6 ChainRun không có CalcSnapshot: 18/19/20 «rỗng» ×3 + 3 run admin của 17. Hồ sơ 17 giờ 6 run (seed ×2 — PO chạy seed batch 00:52 trước run #12). **Handler LogError của Seed_AssessmentAndTerms: không ghi gì** — log chỉ có "Seed_AssessmentAndTerms finished successfully" (Information, category Seed) ở 23:26:36 và 00:52:58. Lỗi thật trong cùng trace: `Seed_ResetDemoClaimData` → `DeleteChainRun` **23503 FK LedgerPosting.ChainRunId** (cả hai lần) — reset không xoá được ChainRun cũ của 0001 vì LedgerPosting còn trỏ tới (ReverseOld chỉ đổi Status, không xoá). | ghi nợ: Seed_ResetDemoClaimData phải xoá LedgerPosting (và CalcSnapshot) trước ChainRun |

### 22/09 01:20 — PANEL TRẢ LỜI: 5e ĐÓNG. Hồ sơ C KHÔNG hỏng.

Tổng: **17 ChainRun · 144 CalcSnapshot**. Bảng B có 11 ChainRunId mang dòng: **9 run × 14 dòng + 2 run × 9 dòng = 144**, khớp đúng tổng.
Bảng B' (inner join ChainRun) **y hệt** bảng B ⇒ không dòng nào trỏ tới ChainRun đã mất. Bảng C (mồ côi, `ChainRunId = 0/null`): **0**.

⇒ Hai run 9 dòng chính là A và C do seed tạo. **Hồ sơ C đẻ đủ 9 dòng.** Quan sát "0003 → 0 dòng" trên F48 tối 21/09 là chuyện **hiển thị**, không phải mất dữ liệu.

**PO tự đính chính lần ba:** giả thuyết "C mồ côi `ChainRunId = 0`" — **sai**, đã dựng hai lần trên hai nền khác nhau, cả hai lần đều sai. Bảng C nói 0.

### RỦI RO DEMO RÚT RA TỪ ĐÂY — F48 đọc ChainRun MỚI NHẤT

0003 có đủ 9 dòng trong DB nhưng tab Calculation hiện trống, vì run mới nhất của nó là một run **rỗng**. Cùng cơ chế đó áp cho **mọi** hồ sơ, kể cả `CLM-2026-0001`.
⇒ **Một lần bấm nhầm tạo ra một ChainRun rỗng trên 0001 là màn hero trắng trơn giữa lúc pitch**, trong khi dữ liệu vẫn còn nguyên trong DB. Đây là rủi ro demo thật, không phải chuyện gọn gàng.
**Xử:** F48 đọc ChainRun mới nhất **có CalcSnapshot**, không phải ChainRun mới nhất. Hoặc tối thiểu: hiện cảnh báo "run này chưa có dữ liệu bước" thay vì bảng trống.

### LỖI THẬT TÌM ĐƯỢC — `Seed_ResetDemoClaimData` không xoá được ChainRun

Handler `AllExceptions → LogError` của `Seed_AssessmentAndTerms`: **không ghi gì**. 30 giờ log, mọi severity — chỉ có *"finished successfully"* (00:52:58 rev 166, 23:26:36 rev 165).

Nhưng **cùng traceId** có một Error thật ở action khác:

> `Seed_ResetDemoClaimData → DeleteChainRun: 23503 — Operation on entity 'ChainRun' failed: … records in 'LedgerPosting' that point to it through 'ChainRunId'`

Reset xoá `CalcSnapshot` xong thì kẹt ở `ChainRun` vì `LedgerPosting` còn trỏ tới — bước 14 chỉ đảo `Status` PROVISIONAL → REVERSED, **không xoá posting**. Hệ quả khớp số: 3 run admin của 0001 còn ChainRun nhưng 0 CalcSnapshot.

**Cùng lớp lỗi với `ChainTest_DeleteClaimTree` đã sửa** (rule 11: xoá con trước cha) — lần này `LedgerPosting` là đứa con **không ai liệt vào danh sách**.

**Lưu ý phương pháp:** lỗi này chỉ lộ ra vì lọc log theo **traceId**, không theo tên action. Action báo "thành công" trong khi một action khác cùng giao dịch đã lỗi.

**Một chỗ trong phần "suy ra" của readout chưa tự khớp:** 0003 vừa được xếp là "run rỗng" vừa là một trong hai run 9 dòng. Đã gắn nhãn suy diễn nên không thành số liệu — không truy tiếp, không đáng.

## 10. 5k — bỏ nhánh ghi PremiumPaid khỏi `Seed_BatchB_Backfill` (22/09, PO)

| Lượt | Nội dung | Kết quả |
|---|---|---|
| 5k-a (read-only) | Session `37ede85c-7d6e-4154-bcbd-3ed42e3f4526` (load rev 166). Run `4a3cc268…`, không sửa gì. **Vòng Policy:** `GetAllPolicies` (Aggregate) → `forEachNode` (For Each) → `GetAllMembers` (Aggregate) → `assignNode` (Assign, `PRow = GetAllPolicies.List.Current.Policy`) → `ifNode` (If, `PRow.PremiumPaid = False`) → True: `assignNode2` (Assign, **đúng một phép gán** `PRow.PremiumPaid = True`) → `DoPolicyUpdate` (UpdatePolicy, Source = PRow) → về `forEachNode`; False → về thẳng `forEachNode`. Trên nhánh True không có node nào khác. Node ghi Policy: `assignNode` (nạp record), `assignNode2` (PremiumPaid), `DoPolicyUpdate` (ghi cả PRow). Không có CreatePolicy. | 0 lỗi, 103 warning (Mentor đếm bằng text) |
| 5k-b (edit) | Cùng session. Prompt: xoá `ifNode` + `assignNode2` + `DoPolicyUpdate`, nối `assignNode` → `forEachNode`, không đụng gì khác, không publish. Run `df067a10…` (~3 phút), `changeApplied: true`. **Chuỗi node sau sửa (Mentor báo):** Start → `GetAllPolicies` → `forEachNode` (Policy) → `assignNode` (PRow = …Current.Policy) → về `forEachNode` ⟶ `GetAllMembers` → `forEachNode2` (Member) → `assignNode2` (MRow, MChanged = False) → chuỗi If CardNo/CoverageFrom/CoverageTo/ContinuousCoverFrom/RenewalStatusId/BenefitLevelNo → `ifNode13` (MChanged) → `DoMUpdate` (UpdateMember) → về `forEachNode2` ⟶ `GetAllPBS` → `forEachNode3` (PBS) → `assignNode4` → If EffectiveFrom/EffectiveTo → `ifNode8` (PBSChanged) → `DoPBSUpdate` (UpdatePolicyBenefitSnapshot) → về `forEachNode3` ⟶ `GetAllClaims` → `forEachNode4` (Claim) → `assignNode8` → If LossType theo ClaimNo (0002/0003 IllnessInpatient; 0001/0004 IllnessOutpatient; 0034/0037 DentalOutpatient; 0035/36/38/39/40/41 MaternityOutpatient) → If TreatmentFrom/TreatmentTo (Admission/Discharge/EventDate) → If RuleSetRef → If ConditionClassId → `ifNode22` (CChanged) → `DoCUpdate` (UpdateClaim) → về `forEachNode4` ⟶ End. **Xác nhận Mentor:** không node nào ghi `Policy.PremiumPaid`; không còn UpdatePolicy; chưa từng có CreatePolicy; Execute Action còn lại chỉ DoMUpdate / DoPBSUpdate / DoCUpdate. | **0 lỗi**; warning: text Mentor 103 (= trước sửa), máy `validation.warningCount` = **111** (= số máy ở rev 161/162; lượt read-only không trả số máy nên không có đối chứng trước/sau cùng phép đếm). **Chưa publish**, session để mở |

**Hai điểm phải nói rõ (5k-b):**
1. **Tên node lệch giữa hai lượt đọc.** Trước sửa Mentor gọi node ghi PremiumPaid là `assignNode2` và đặt `GetAllMembers` *trong* vòng Policy; sau sửa `assignNode2` là Assign của vòng Member và `GetAllMembers` nằm *sau* vòng Policy. Khả năng cao tên hiển thị `assignNodeN`/`ifNodeN` do Mentor đánh theo thứ tự nên trượt sau khi xoá 3 node — nhưng đó là suy đoán; kết luận chắc chắn chỉ là: **sau sửa không còn phép gán `PremiumPaid` và không còn `UpdatePolicy` nào** (Mentor xác nhận ở (b)). Kiểm độc lập = diff node của revision sau publish (tasks 5k).
2. **Vòng Policy giờ rỗng:** `forEachNode → assignNode (PRow) → forEachNode` — chạy qua mọi Policy, gán biến PRow rồi không dùng. Vô hại, không ghi gì; giữ nguyên theo lệnh "không đụng gì khác". Nếu PO muốn, xoá `GetAllPolicies`/`forEachNode`/`assignNode`/biến `PRow` là một lượt riêng.

### 5k xong (chưa publish) — `Seed_BatchB_Backfill` không còn ghi `Policy.PremiumPaid`

Session 37ede85c, `changeApplied: true`. Xoá nhánh `If PremiumPaid = False → Assign → DoPolicyUpdate`. Còn lại 4 vòng: Policy (nay **rỗng** — `GetAllPolicies → assignNode(PRow) →` quay lại, gán rồi không dùng) · Member (`DoMUpdate`) · PolicyBenefitSnapshot (`DoPBSUpdate`) · Claim (`DoCUpdate`). Không còn `UpdatePolicy`; chưa từng có `CreatePolicy`. **0 lỗi.**

Warning: Mentor đếm text 103, số máy `validation.warningCount` 111 (bằng rev 161/162). Lượt read-only không trả số máy ⇒ **không có cặp trước/sau cùng phép đếm, không dùng làm bằng chứng** (rule 15).

**Vòng Policy rỗng:** giữ nguyên, vô hại. Xoá là một lượt riêng — **batch F**, không làm bây giờ.

**PO duyệt cách nghiệm thu:** mệnh đề *"không node nào ghi `Policy.PremiumPaid`"* là kiểm được, khác hẳn *"đã xoá xong"*. Kiểm độc lập vẫn là **mở revision sau publish và liệt kê node**.
| 5k-c (read-only) | `Seed_ResetDemoClaimData` — run `1ced998c…`, không sửa gì. 44 node: `GetClaims4` (Claim, không filter, max 4, sort Id asc) → `GetRecEv` (RecEvidence ⋈ Recommendation ⋈ Assessment, Assessment.ClaimId ∈ 4 claim) → ForEach/`DelRecEv` → `GetRec` → ForEach2/`DelRec` → `GetAss` → ForEach3/`DelAss` → `GetCS` (CalcSnapshot ⋈ ChainRun, ChainRun.ClaimId ∈ 4) → ForEach4/`DelCS` → `GetCR` (ChainRun.ClaimId ∈ 4) → ForEach5/**`DelCR`** → `GetDoc` → ForEach6/`DelDoc` → `GetAR` (AgentRun) → ForEach7/`DelAR` → `GetClLines` → ForEach8/`Assign`+`ResetClLine` (Update ClaimLine.DecisionCodeId = PENDING) → `GetFirstLineA/B/C` (ClaimLine.ClaimId = `GetClaims4.List[0/1/2].Claim.Id`, max 1) + `Assign2/3/4` + `UpdRoomA/B/C` (Category = "ROOM") → `GetLL` (LimitLedger.MemberId = `GetClaims4.List[0].Claim.MemberId`) → ForEach9/`DelLL` → `GetTR` (TermRegister, **không filter — xoá toàn bộ**) → ForEach10/`DelTR` → `LogDone` → End. **Thứ tự xoá:** RecEvidence → Recommendation → Assessment → CalcSnapshot → ChainRun → ClaimDocument → AgentRun → LimitLedger → TermRegister (ClaimLine chỉ update; Claim không xoá). **FK → ChainRun:** CalcSnapshot.ChainRunId, **LedgerPosting.ChainRunId**. **FK → Claim:** AgentRun, Assessment, CalcSnapshot, ChainRun, ClaimDocument, ClaimLine, LedgerPosting, **TermCheckpoint** (.ClaimId), LimitLedger.LastClaimId. **Handler:** AllExceptions, **Abort Transaction = No** → `LogErr` (LogMessage "Seed_ResetDemoClaimData error: " + ExceptionMessage, category Seed) → End. | 0 lỗi, 103 warning (text). Kết luận: action **không bao giờ xoá LedgerPosting** → `DelCR` ném 23503 → handler nuốt, **các node sau DelCR (ClaimDocument, AgentRun, reset ClaimLine, ROOM, LimitLedger, TermRegister) bị bỏ qua**, các delete trước đó vẫn commit (không abort). TermCheckpoint cũng không được xoá. `List[0/1/2]` = truy cập theo chỉ số (bẫy đã biết) |

### 22/09 — `Seed_ResetDemoClaimData` CHẠY NỬA CHỪNG, ÂM THẦM. Đây là lỗi nặng nhất tìm được tuần này.

Đọc 44 node. Thứ tự xoá hiện tại: RecEvidence → Recommendation → Assessment → CalcSnapshot → **ChainRun** → ClaimDocument → AgentRun → LimitLedger → TermRegister.

**`LedgerPosting` không bao giờ bị xoá.** FK `LedgerPosting.ChainRunId` → `DelCR` ném `23503`.

Handler: `AllExceptions`, **`Abort Transaction = No`** → `LogErr` → `End`. Hậu quả là tổ hợp tệ nhất có thể:

- Các delete **trước** `DelCR` **vẫn commit** (RecEvidence, Recommendation, Assessment, CalcSnapshot).
- Mọi node **sau** `DelCR` **không bao giờ chạy**: `ClaimDocument` · `AgentRun` · **reset `ClaimLine.DecisionCodeId` về PENDING** · fixup ROOM · **xoá `LimitLedger`** · **xoá `TermRegister`**.
- Nút báo **thành công**.

**Ba hệ quả phải kiểm ngay, theo mức nguy:**

1. **`TermRegister` chưa bao giờ được dọn**, trong khi `Seed_AssessmentAndTerms` tạo T-01…T-10 **mỗi lần seed**. ⇒ bảng có thể đang mang **10 × số lần seed** dòng. Term engine đọc bảng này. **Đếm ngay.**
2. **`ClaimLine.DecisionCodeId` không bao giờ về PENDING.** Kịch bản demo *before/after* dựa đúng vào việc dòng `NOTREL` chưa được quyết trước khi diễn. Nếu reset không chạm tới nó, thì **sau một lần tập, buổi diễn thật bắt đầu từ trạng thái đã quyết** — và câu chuyện `2.000.000 → 1.880.000` mất nửa đầu.
3. **`LimitLedger` không bao giờ được dọn** ⇒ bộ đếm tích luỹ qua các lần chạy.

**Thứ tự xoá đúng** (con trước cha, `LedgerPosting` lọc theo `ClaimId ∈ 4` vì nó có cả hai FK):
RecEvidence → Recommendation → Assessment → **LedgerPosting** → CalcSnapshot → ChainRun → ClaimDocument → AgentRun → **TermCheckpoint** → LimitLedger → TermRegister.

`TermCheckpoint.ClaimId` cũng trỏ tới Claim và cũng chưa bao giờ được dọn — thêm vào.

**`Abort Transaction = No` phải đổi.** Một nút reset của buổi demo mà hỏng **âm thầm** là tệ hơn hỏng ồn ào: PO quyết **`Abort Transaction = Yes`**, và thông báo trên màn phải nói *thất bại* thay vì toast thành công. Không có nghiệm thu nào bắt được một cái nút nói dối.

**Bẫy còn lại, chưa sửa:** `GetFirstLineA/B/C` và `GetLL` truy cập `GetClaims4.List[0/1/2]` theo chỉ số — `GetClaims4` không filter, max 4, sort Id asc. Đổi Id của 4 hồ sơ đầu là đổi ý nghĩa của action. Ghi batch F.
| 5k-d (read-only, dữ liệu) | PO hỏi 5 số. **Không có đường truy vấn** (`db_query` v1 không trả dòng; Portal ODC bản này không có tab Data) → chỉ đọc qua màn hình app. **TermRegister:** màn `TermRegisterList` hiện 10 mã TERM-HC-001…010, mỗi mã một dòng; ô "Terms registered" trên Administration = `GetTermsCount.Count` (số dòng của aggregate, không phải `TermCount`) ⇒ luôn hiện **1** — lỗi hiển thị; bảng "Type/Level/Received" là ClaimDocument group TemplateId (T1-04 12 · T1-03 12 · T1-01 12 · T2-02 6 · «rỗng» 40 · T2-01 12 · T1-02 9 = 103 ClaimDocument). **LimitLedger tổng:** không có màn nào đếm; `LimitLedgerView` dropdown thành viên (Member ⋈ Policy ⋈ LimitLedger, BenefitCode = "200") liệt kê "Nguyễn Văn An" **3 lần** ⇒ member 0001 có ≥3 dòng ledger 200 (trùng do DelLL bị bỏ qua sau lỗi DelCR). **ClaimLine 4 hồ sơ Id 17–20** (ClaimDetail, Id dòng không render): 17 = CONSULT/LAB/IMAGING/DRUG/DRUG/SUPPLEMENT (6, đều PENDING); 18 = ROOM/LAB/DRUG/SUPPLEMENT (4, PENDING); 19 = ROOM/LAB/IMAGING/PROCEDURE/DRUG (5, PENDING); 20 = CONSULT/LAB/DRUG (3, PENDING). **LedgerPosting / TermCheckpoint theo ClaimId:** không màn nào đọc hai entity này (Mentor xác nhận) — **không đo được**. Bằng chứng reset chạy dở: ClaimDetail 17/18/19/20 hiện checklist chứng từ ×3 (18/15/18/12 dòng "Received"), History log 3 lần "CLAIM_SUBMITTED received" (14:16 · 23:26 · 00:52) — ClaimDocument/AgentRun không bị xoá vì nằm sau `DelCR`. | đọc xong |
| 5k-e (read-only, caller) | Run `ca281fe2…`. `Seed_ResetDemoClaimData` **không có output**; caller duy nhất `RunSeedBatch0Now` (CallReset → CallBatchB3 → CallSeed AssessmentAndTerms → CallWaitingPeriods → CallBatchBBackfill → CallSeedTerms TermRegister_v2 → CallBenefitSchedule → CallBatchB2Remap → GetBedLineB/UpdBedB → GetBedLineC/UpdBedC → CallSeedChainTests → GetClaim0001 → CallRunChain(seed) → LogDone "RunSeedBatch0Now done" → End); gọi từ screen action `RunSeed` (Administration: CallSeed → Refresh GetProviders → **Feedback Message "Seed re-run"** → Refresh GetDataCheck → End) và `RunSeedAction` (Dashboard: CallSeed → Refresh GetMetrics → Feedback Message "Seed re-run" → End). **Không node nào báo lỗi cho người dùng**; không If, không handler, không cờ — "Seed re-run" hiện kể cả khi reset thất bại. | đọc xong |
| 5k-f (edit) | Cùng session `37ede85c…`. Prompt: 3 thay đổi trên `Seed_ResetDemoClaimData` (xoá LedgerPosting trước CalcSnapshot · xoá TermCheckpoint sau AgentRun · handler Abort Transaction = Yes + log mức Error). Run `115149e4…` (~2 phút), `changeApplied: true`. **Thứ tự xoá mới:** RecEvidence → Recommendation → Assessment → **LedgerPosting (mới)** → CalcSnapshot → ChainRun → ClaimDocument → AgentRun → **TermCheckpoint (mới)** → LimitLedger → TermRegister (ClaimLine chỉ update). Khối mới: `GetLP` (LedgerPosting.ClaimId ∈ 4 claim; predecessor = exit ForEach Assessment, trước đây nối `GetCS`) → ForEach (không tên) → `DelLP` (Id = GetLP.List.Current.LedgerPosting.Id) → exit → `GetCS`; `GetTC` (TermCheckpoint.ClaimId ∈ 4; predecessor = exit ForEach AgentRun, trước đây nối `GetClLines`) → ForEach → `DelTC` → exit → `GetClLines`. Handler: **Abort Transaction True** (trước False); node `LogErr` giữ LogMessage/message/category; ODC `LogMessage` không có tham số mức nên Mentor bật thuộc tính **Log Error = True của handler** (exception ghi vào log mức Error) — đây là cách hiểu của Mentor cho "raise severity to Error", PO xác nhận. | **0 lỗi**; warning text 103 (= trước), máy 111 (= 5k-b). **Chưa publish**, session để mở |

### 22/09 — đo "trước" cho 5m (chỉ đọc được qua màn hình app; không có đường truy vấn dữ liệu)

`db_query` v1 không trả dòng, Portal ODC bản này không có tab Data ⇒ mọi con số dưới đây đọc qua UI.

| Đo | Kết quả | Đánh giá |
|---|---|---|
| `TermRegister` | **10** mã TERM-HC-001…010, mỗi mã 1 dòng | **Không tích luỹ.** Nhẹ gánh lo số 1 |
| `ClaimLine` hồ sơ 17–20 | **tất cả PENDING** (6 · 4 · 5 · 3 dòng) | **Kịch bản before/after chưa hỏng.** Rủi ro có thật nhưng chưa cắn |
| `LimitLedger` | không màn nào đếm; dropdown `LimitLedgerView` liệt "Nguyễn Văn An" **3 lần** | ≥3 dòng ledger 200 trùng cho member 0001 |
| `LedgerPosting` / `TermCheckpoint` | không màn nào đọc | không đo được |

**Bằng chứng trực tiếp reset dừng trước `DelCR`:** `ClaimDetail` 17–20 hiện checklist chứng từ **×3** (18/15/18/12 dòng, tổng 103 `ClaimDocument`), History log **3 lần** *"CLAIM_SUBMITTED received"* (14:16 · 23:26 · 00:52). `ClaimDocument` và `AgentRun` nằm sau `DelCR` nên chưa bao giờ bị xoá. **Đây là lỗi giám khảo NHÌN THẤY** trên tab chứng từ — quan trọng hơn cả hai mối lo ban đầu.

**Một chỗ chưa giải thích, không truy tiếp (thuận lợi):** nếu seed tạo 10 term mỗi lần và reset chưa bao giờ xoá `TermRegister`, bảng phải có 30+. Nó có 10. Một trong hai niềm tin đó sai. Ghi lại, không đuổi.

**Ô "Terms registered = 1" trên Administration là lỗi hiển thị** — `GetTermsCount.Count` đếm số dòng aggregate chứ không phải số term. Bảng "Type/Level/Received" cạnh nó thực ra là `ClaimDocument` group theo `TemplateId`. Batch F.

**Caller:** `Seed_ResetDemoClaimData` **không có output**; caller duy nhất `RunSeedBatch0Now`, gọi từ `RunSeed` (Administration) và `RunSeedAction` (Dashboard) — **cả hai kết thúc bằng Feedback Message "Seed re-run" vô điều kiện**, không If, không cờ, không đường báo lỗi.

**PO quyết: KHÔNG sửa đường báo lỗi của nút.** Nó là bảo hiểm, không phải tính năng, và chạm vào là 3 action + 2 màn. Thay bằng **một dòng trong checklist ngày pitch**: sau khi bấm reset, mở `ClaimDetail` 0001 và đếm checklist chứng từ — **6 dòng chứ không phải 18**. Một mắt nhìn thay cho năm chỗ sửa code.

### 5m sửa xong (chưa publish) — thứ tự xoá đã đúng

RecEvidence → Recommendation → Assessment → **LedgerPosting** → CalcSnapshot → ChainRun → ClaimDocument → AgentRun → **TermCheckpoint** → LimitLedger → TermRegister. Handler `Abort Transaction = True` (trước False); `Log Error = True` — ODC `LogMessage` không có tham số mức, nên đây là cách đúng để ghi Error. **PO chấp nhận cách hiểu này.**
| 5k-g (diff + publish) | Run `12cfa2ac…`: diff session vs rev 166 = **16 dòng, đúng 2 nhóm** (Backfill −3 node 68→65 + nối lại; Reset +6 node 43→49 + 4 rewire + handler AbortTransaction/LogError False→True), "No other differences". PO: publish → `841b5f1a…` = **rev 167**, `outcome: success` (~01:04 UTC 22/09). Session `37ede85c…` đóng; session mới `4561a3f9…` load rev 167 để đọc node thật (run `66f642f6…`). | **Publish rev 167** |
| Seed trên rev 167 (PO chạy 01:05:30 UTC) | traceId `49c28fcb2467fa8101e05099a89002d8`, **26 dòng, toàn Information, 0 Warning/Error, 0 trace lỗi**: `Seed_ResetDemoClaimData done` (01:05:30.995 — lần đầu tiên thấy dòng này) → `Seed: claims A,B,C,D = 17,18,19,20` → terms created → assessment A/B/C/D created 199/200/201/202 → recommendation 199 → chainrun A/B/C/D 199–202 → totals/lines A–D → ledger created → `Seed_ClaimDocuments: starting` → docs created → `Seed_AssessmentAndTerms finished successfully` + `Seed_AgentRuns finished successfully` (01:05:31.453) → `RunSeedBatch0Now done` (01:05:33.126). ClaimDetail sau seed: checklist chứng từ **17 = 6 · 18 = 5 · 19 = 6 · 20 = 4** (trước: 18/15/18/12); "CLAIM_SUBMITTED received" **1 lần mỗi hồ sơ** (trước: 3). ⇒ reset xoá sạch ClaimDocument/AgentRun; không còn 23503. | dữ liệu demo sạch |
| 5k-h (read-only rev 167, session mới) | Session `4561a3f9…` load **rev 167** (Mentor xác nhận), run `66f642f6…`. **Reset — node ghi/xoá theo thứ tự chạy:** DelRecEv (DeleteRecEvidence ← GetRecEv) → DelRec (DeleteRecommendation ← GetRec) → DelAss (DeleteAssessment ← GetAss) → **DelLP (DeleteLedgerPosting ← GetLP, ClaimId ∈ 4)** → DelCS (DeleteCalcSnapshot ← GetCS ⋈ ChainRun) → DelCR (DeleteChainRun ← GetCR) → DelDoc (DeleteClaimDocument ← GetDoc) → DelAR (DeleteAgentRun ← GetAR) → **DelTC (DeleteTermCheckpoint ← GetTC, ClaimId ∈ 4)** → ResetClLine (UpdateClaimLine PENDING) → UpdRoomA/B/C (UpdateClaimLine ROOM) → DelLL (DeleteLimitLedger ← GetLL, MemberId = claim[0]) → DelTR (DeleteTermRegister ← GetTR, không filter) → LogDone. Thứ tự xoá: RecEvidence → Recommendation → Assessment → LedgerPosting → CalcSnapshot → ChainRun → ClaimDocument → AgentRun → TermCheckpoint → LimitLedger → TermRegister. **Handler:** AllExceptions; Abort Transaction *"không đặt — mặc định = Yes"*; Log Error *"không đặt — mặc định"*; flow `LogErr` (LogMessage "Seed_ResetDemoClaimData error: " + ExceptionMessage, Seed) → End. (Session cũ đọc "False → True"; session mới đọc "không đặt = mặc định Yes" — cùng nghĩa: OML bỏ thuộc tính bằng mặc định. Bằng chứng chạy: seed 01:05 không còn 23503, "done" xuất hiện.) **Backfill:** node ghi thuộc tính Policy = **none**; không UpdatePolicy/CreatePolicy/CreateOrUpdatePolicy/DeletePolicy; `PRow` chỉ được gán từ `GetAllPolicies.List.Current.Policy`, không ghi ngược DB. Vòng Policy: GetAllPolicies → ForEach → Assign (PRow) → back-edge (CycleTarget = Assign). (Session này mô tả các vòng Member/PBS/Claim là *sub-loop* trong vòng Policy — khác session trước mô tả tuần tự; hai lần đọc lại lệch về cấu trúc, thống nhất về "không ghi Policy".) | 0 lỗi, 97 warning (text, session mới). Session để mở |

### 5m ĐÓNG — nghiệm thu bằng dữ liệu, rev 167, sau seed 01:05 UTC

| Claim | Checklist chứng từ | "CLAIM_SUBMITTED received" |
|---|---|---|
| 17 · CLM-2026-0001 | 18 → **6** | 3 → **1** |
| 18 · CLM-2026-0002 | 15 → **5** | 3 → **1** |
| 19 · CLM-2026-0003 | 18 → **6** | 3 → **1** |
| 20 · CLM-2026-0004 | 12 → **4** | 3 → **1** |

Đúng ÷3 trên cả 8 ô. Đếm bằng hai cách độc lập (dòng mã `T*-**` và số ô "Received") cho cùng kết quả. Không còn `23503`, log có dòng "done".

**Chuỗi nhân quả đóng trọn:** `LedgerPosting` không được xoá → `DelCR` ném FK → handler bắt, không abort → mọi node sau `DelCR` bị bỏ qua → `ClaimDocument` tích luỹ mỗi lần seed → checklist ×3 trên màn giám khảo. Sửa thứ tự xoá + abort → ÷3.

*(Câu "không đổi so với lần đọc trước" trong báo cáo là lỗi diễn đạt — chính các con số trong đó cho thấy đã đổi. Ghi lại để người đọc log sau không hiểu ngược.)*

**PO chốt: đóng sổ phần seed và hạ tầng test.** Còn lại một việc thuộc màn hình — `5l` — rồi chuyển hẳn sang IDP, AG-2 và deck. Mọi thứ khác về seed: batch F.
