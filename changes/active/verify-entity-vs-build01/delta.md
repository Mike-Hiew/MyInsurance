# Delta — data model ClaimMVP (rev 128) vs `build-01`

**Ngày:** 20/09/2026 · **Nguồn:** `current-model.md` + `current-enums.md` (MCP `context_entities`, rev 128) đối chiếu `build-01-data-model-va-enum` v2.0
**Trạng thái:** task 3–9 của `tasks.md`. Cột *Quyết* còn trống ở dòng nào thì dòng đó chưa được duyệt.

---

## 0. Kết luận trước

> **Model hiện tại là một mô hình MÀN HÌNH, không phải mô hình NGHIỆP VỤ.** Nó đủ để 17 màn render, và **không đủ để chuỗi 14 bước chạy** — vì bốn đầu vào mà chuỗi đọc ở bước 1, 2, 5 và 9 **không tồn tại ở bất kỳ đâu trong model**.

| Chiều | `build-01` | Hiện có | Khớp |
|---|---:|---:|---|
| Entity | **33** (10 hợp đồng · 16 claim · 7 IDP) | **18** (14 thường + 4 static) | **8 ánh xạ được** · 23 thiếu · 3 thừa |
| Enum / Static Entity | **~30 bộ** | **4** | **0 bộ khớp hoàn toàn** |
| Ràng buộc phải tự thêm | **30** | 0 kiểm được từ API | — |

**Bốn thứ chặn cứng, không thương lượng được:**

1. **`Member` không có `CoverageFrom` / `CoverageTo`** → bước 1 (`PERIOD`) và bước 2 (`WAIT`) **không có gì để so**.
2. **`PolicyBenefitSnapshot` không có cây quyền lợi** (`ParentBenefitCode`, `Depth`) → bước 8, 12, 13 **không kế thừa được trần**.
3. **`ClaimLine` không có `CoveredAmount` / `CustomerBorneAmount`** → chuỗi **không có chỗ ghi kết quả**, và màn "Vì sao Quý khách tự chịu" của portal không có nguồn.
4. **`DecisionCode` thiếu 5 trong 7 mã từ chối** → bước 1, 2, 5, 8, 9, 12, 13 sinh ra mã **không có chỗ lưu**.

**Một thứ sai thẳng chuẩn, sửa trong 2 phút:** `PolicyStatus` có `Expired` — `build-01` §1.5 nói **đúng hai giá trị `ACTIVE` · `CANCELLED`, và `EXPIRED` KHÔNG tồn tại**, vì hợp đồng quá hạn phải trượt phép kiểm ngày chứ không mang một trạng thái riêng. Giữ `Expired` là tạo nguồn sự thật thứ hai cho cùng một sự kiện.

---

## 1. Entity — ánh xạ, thiếu, thừa

### 1.1 Tám entity ánh xạ được (tên khác thì ghi rõ)

| Hiện có | `build-01` | Mức lệch attribute |
|---|---|---|
| `Claim` (17) | §2.1 `Claim` (23) | 🔴 thiếu 11, quan trọng nhất `LossType` |
| `ClaimDocument` (9) | §2.3 `ClaimDocument` (24) | 🔴 thiếu 17 |
| `ClaimLine` (14) | §2.4 `ClaimLine` (~30) | 🔴 thiếu 18 |
| `ChainRun` (13) | §2.6 `ChainRun` (15) | 🟡 thiếu 2 |
| `LimitLedger` (9) | §2.7 `LimitLedger` (13) | 🔴 khác hình dạng |
| `AgentRun` (15) | §2.13 `AgentRun` (13) | 🟢 khớp tốt nhất — thiếu `Replay` |
| `Assessment` (9) | §2.8 `ClinicalAssessment` (9) | 🟡 đổi tên + thiếu `AssessmentId` |
| `Recommendation` (12) | §2.9 `ClinicalRecommendation` (11) | 🟡 đổi tên |
| `Member` (7) | §1.7 `InsuredMember` (16) | 🔴 thiếu 9 |
| `Policy` (7) | §1.5 `Policy` (11) | 🟡 thiếu 5 |
| `PolicyBenefitSnapshot` (10) | §1.3 `BenefitScheduleLine` (16) *(dẹt theo policy)* | 🔴 mất cây + mất 3 sentinel |

*(11 dòng — `Member`, `Policy`, `PolicyBenefitSnapshot` ánh xạ sang tầng hợp đồng chứ không tầng claim.)*

### 1.2 Hai mươi ba entity THIẾU

| Tầng | Thiếu | Mức |
|---|---|---|
| **Hợp đồng (8/10)** | `Product` · `Plan` · `BenefitScheduleLine` · `BenefitWaitingPeriod` · `BenefitLevel` · `PayoutAccount` · `Beneficiary` · `Claimant` | 🔴 `BenefitWaitingPeriod` · 🟡 còn lại |
| **Claim (8/16)** | `InsuredEvent` · `ClaimLineBreakdown` · `FraudSignal` · `CrossCheckResult` · `ClaimEvent` · `ClaimLetter` · `DocumentChecklistRule` · `PaymentOrder` | 🔴 `ClaimLineBreakdown`, `DocumentChecklistRule` |
| **IDP (7/7)** | `IdpSession` · `Template` · `DocumentSet` · `IdpRequest` · `IdpWorkflowRequest` · `IdpFieldMap` · `IdpReplay` | 🔴 toàn bộ — chặn REST + chặn `ReplayMode` |

**Ba cái đau nhất trong danh sách này:**

- **`BenefitWaitingPeriod`** — thời gian chờ là `(PlanCode, BenefitCode, ConditionClass) → Days`, **6 nhóm bệnh**. Hiện `PolicyBenefitSnapshot.WaitingPeriodDays` là **một số vô hướng** cho mọi nhóm bệnh ⇒ thai sản 270 ngày và tai nạn 0 ngày **không cùng tồn tại được**. Bước 2 sai với mọi hồ sơ không phải `ILL_*`.
- **`ClaimLineBreakdown`** — 8 `causeCode` khách tự chịu. Không có nó thì khối *"Vì sao Quý khách tự chịu"* trên `PortalClaimDetail` **đang hiển thị cái gì?** (cần kiểm — có thể đang hardcode).
- **`DocumentChecklistRule`** — checklist 8+1 dòng theo `lossType`. Không có nó thì `PortalNewClaim` / `PortalSupplement` đang sinh checklist bằng gì?

### 1.3 Ba entity THỪA — và cả ba nên giữ

| Thừa | `build-01` làm gì thay | Quyết |
|---|---|---|
| `RecEvidence` (6) | Gộp thành `ClinicalRecommendation.EvidenceJson` (Text) | **Giữ. Sửa spec.** Chuẩn hoá tốt hơn JSON — ràng buộc #15 (*evidence không được rỗng*) kiểm bằng count dễ hơn parse JSON |
| `CalcSnapshot` (10) | Gộp thành `ChainRun.StepOutputsJson` | **Giữ cả hai. Sửa spec.** `StepOutputsJson` là bản ghi bất biến của lần chạy; `CalcSnapshot` là hàng hiển thị cho tab calc. Giữ `StepOutputsJson` làm nguồn, `CalcSnapshot` là chiếu ra |
| `TermRegister` (8) | Không định nghĩa entity — chỉ có enum `termStatus`/`termOutcome` §4.21 | **Giữ. Bổ sung vào spec.** `build-06` có màn Sổ điều khoản, spec data model bỏ sót |

---

## 2. Enum — bốn chỗ chặn demo

### 2.1 Bảng tổng

| `build-01` | Bộ | Hiện có | Trạng thái |
|---|---:|---|---|
| §4.7 `claimState` | 13 | `ClaimStatus` 8 | 🔴 **thiếu 5** |
| §4.10 `declineReason` | 7 đóng | `DecisionCode` 5 (lẫn 2 khái niệm) | 🔴 **thiếu 5, thừa 3** |
| §4.2 `lossType` | 12 | — | 🔴 **không tồn tại** |
| §4.5 `serviceCategory` | 20 | `ClaimLine.Category` Text(20) tự do | 🔴 **không tồn tại** |
| §4.3 `episodeType` | 5 | `TreatmentType` 4 | 🔴 **thiếu `ACCIDENT`** |
| §4.28 `policyStatus` | **2** | `PolicyStatus` 3 | 🔴 **thừa `Expired`** |
| §4.4 `conditionClass` | 6 | — | 🔴 khóa tra thời gian chờ |
| §4.23 `causeCode` | 8 | — | 🔴 nuôi màn portal |
| §4.15 `requirementLevel` | 3 | — | 🔴 checklist |
| §4.1 `documentType` | 8 | — | 🔴 checklist |
| §4.16 `checklistItemState` | 5 | — | 🔴 |
| §4.17 `packageState` | 6 | — | 🔴 |
| §4.13 `recDecision` | 3 | `Recommendation.Decision` Text tự do | 🟡 |
| §4.14 `confidenceBand` | 3 | `Recommendation.Band` Text(10) tự do | 🟡 |
| §4.25 `claimQueue` | 4 | — (`Claim` không có `Queue`) | 🟡 |
| §4.27 `agentCode` | 6 | `AgentRun.AgentName` Text(100) tự do | 🟡 |
| §4.8/4.9/4.11/4.18–4.22/4.24/4.26 | 12 bộ | — | 🟡 |
| §4.29–4.31 (IDP) | 3 bộ | — | 🔴 chặn REST |

### 2.2 `ClaimStatus` — 8 vs 13

| `build-01` | Hiện có | Ghi chú |
|---|---|---|
| `DRAFT` | **thiếu** | Portal có hồ sơ nháp không? |
| `SUBMITTED` | `Received` | 🟡 đổi tên |
| `DOC_PROCESSING` | `Extracting` | 🟡 đổi tên |
| `PENDING_INFO` | **thiếu** | 🔴 `PortalSupplement` sống trên trạng thái này |
| `EXTRACTION_ERROR` | `ExtractionError` | ✓ |
| `UNDER_ASSESSMENT` | `UnderReview` | 🟡 đổi tên |
| `PENDING_APPROVAL` | `PendingApproval` | ✓ |
| `APPROVED` | `Approved` | ✓ |
| `DECLINED` | `Declined` | ✓ |
| `PAYMENT_IN_PROGRESS` | **thiếu** | 🔴 `PAY-NOTIF-001` không có trạng thái để bám |
| `PAID` | `Paid` | ✓ |
| `CLOSED` | **thiếu** | |
| `CANCELLED` | **thiếu** | 🔴 `CLM-NOTIF-009` |

### 2.3 `DecisionCode` — lẫn hai khái niệm

Hiện: `COVERED` · `NOTCOV` · `NOTREL` · `PARTIAL` · `PENDING`.

`build-01` tách làm hai:
- **§4.8 `lineDecision`, 2 giá trị:** `CHI_TRẢ` · `TỪ_CHỐI`
- **§4.10 `declineReason`, 7 mã ĐÓNG:** `LIMIT` `PERIOD` `NOBEN` `WAIT` `EXCL` `NOTREL` `NOTCOV`

⇒ `COVERED` / `PARTIAL` thuộc khái niệm thứ nhất, `PENDING` không thuộc khái niệm nào (nó là `recDecision.Proposed`). **Năm mã `LIMIT` `PERIOD` `NOBEN` `WAIT` `EXCL` không có chỗ.**

### 2.4 ⚠ Mọi Static Entity thiếu `Value` và `LabelVi`

Cả 4 static entity đều chỉ có `Id` · `Label` · `Order` · `Is_Active`. `build-01` §0.4 yêu cầu **`Value` + `Label` + `LabelVi`**.

Hai hệ quả, cả hai đã chạm vào màn hình đang có:

1. **Không có `Value`** ⇒ mã nghiệp vụ ổn định chính là *identifier của record trong Studio*. Đổi tên record trong Studio là đổi mã nghiệp vụ — và API design-time không trả `Label`, nên **không có cách nào kiểm chéo**.
2. **Không có `LabelVi`** ⇒ quy tắc số 3 của `build-06` (*không bao giờ hiện mã nội bộ cho khách, luôn render tên tiếng Việt*) **không có nguồn để đọc**. Nhãn tiếng Việt trên 7 màn MyInsurance hiện đang đến từ đâu? Nếu hardcode trong màn thì đó là 🔴 và nó là cùng một lỗi với "màn hình soạn câu".

---

## 3. Attribute thiếu trên các entity đã có

### 3.1 `Member` ← `InsuredMember` 🔴 **đắt nhất**

| Thiếu | Ảnh hưởng | Mức |
|---|---|---|
| **`CoverageFrom` · `CoverageTo`** | **Phép kiểm cover là 5 phép so ngày của D2 §3.3.3. Không có hai field này thì bước 1 `PERIOD` không chạy** | 🔴🔴 |
| **`ContinuousCoverFrom` · `RenewalStatus`** | Thời gian chờ đo từ ngày tham gia **liên tục**. Đo từ `CoverageFrom` sẽ **từ chối sai mọi hồ sơ tái tục** | 🔴🔴 |
| **`CardNo`** | Khóa tìm kiếm chính lúc tiếp nhận, và **khóa duy nhất tìm ra người phụ thuộc** (trẻ em không có CCCD) | 🔴 |
| `BenefitLevelNo` | Thành viên lấy cover từ level nào | 🔴 |
| `IdNumber` · `Status` | | 🟡 |

> `Status` khi thêm phải kèm ghi chú: **KHÔNG được tham gia phép kiểm cover** (ràng buộc #5).

### 3.2 `PolicyBenefitSnapshot` ← `BenefitScheduleLine` 🔴

| Thiếu | Ảnh hưởng | Mức |
|---|---|---|
| **`ParentBenefitCode` · `Depth`** | Mất cây quyền lợi ⇒ `AnnualCap = null` (*included*) không giải được về tổ tiên ⇒ **bước 8/12/13 sai** | 🔴🔴 |
| **`CountLimit` · `CountUnit`** | Chiều hạn mức thứ hai (ngày / lượt). Sentinel `CountLimit = 0` = **không giới hạn** | 🔴 |
| **`DeductionPct` · `DeductionCondition`** | Khấu trừ 20% khi `PROVIDER_OFF_NETWORK`. `Claim.IsInNetwork` **đã có** nhưng **không có gì đọc nó** | 🔴 |
| `LimitType` | `annual` / `per_event` / `per_day` | 🔴 |
| `EffectiveFrom` · `EffectiveTo` | Ngữ nghĩa *as-at-a-date* | 🟡 |
| `MemberId` | Snapshot đang gắn theo **policy**, spec gắn theo **member × benefit level** | 🟡 |

Thêm: **`CoinsurancePct` đang là `Decimal(37,8)`**; spec §6.1 nói `pct` lưu **basis point số nguyên** (20% → `2000`). Giữ decimal là mở đường cho lệch làm tròn giữa hai lần chạy.

Và: **`WaitingPeriodDays` là một số vô hướng** — xem §1.2, phải tách thành `BenefitWaitingPeriod`.

### 3.3 `ClaimLine` 🔴

| Nhóm thiếu | Field | Mức |
|---|---|---|
| **Chuỗi ghi vào** | `CoveredAmount` · `CustomerBorneAmount` | 🔴🔴 |
| **Số học chứng từ** | `LineNo` · `UnitOfMeasure` · `UnitPrice` · `LineAmount` · `PaidBySocialInsurance` · `PatientCoPaymentBhyt` · `PatientSelfPaid` · `OtherPayerAmount` | 🔴 — `AmountClaimed = L7 − L8`, không có `L8` thì công thức không tồn tại |
| **Phân bổ & truy nguyên** | `EventNo` · `SourceDocumentId` · `DocumentDate` (L15) · `Icd10` (L16) · `TreatmentDaysLine` (L18) | 🔴 — `TreatmentDaysLine` là đầu vào bộ đếm `DAY` |
| **Quyết định** | `LineDecision` · `DeclineRationale` · `DeclineRationaleOriginal` · `Verified` | 🔴 — `DecisionReason` Text(500) hiện có **không đủ**, spec đòi `textarea(2000)` |
| **Thô vs đã sửa** | `OriginalValueJson` · `EditedValueJson` | 🔴 — ràng buộc "giá trị gốc không bao giờ bị ghi đè" |
| Khác | `DrugRegistrationNo` | 🟡 |

**`Category` Text(20) tự do** phải thành FK về static `ServiceCategory` 20 giá trị — nó là thứ ánh xạ quyền lợi tiêu thụ, `build-01` §4.5 ghi rõ *"đổi nó là re-map quyền lợi và làm mất hiệu lực phép tính"*.

### 3.4 `LimitLedger` — khác hình dạng, không chỉ thiếu field 🔴

| Spec | Hiện có |
|---|---|
| `Before` · `Consumed` · `After` (ràng buộc `After = Before − Consumed`) | `RemainingAmount` · `UsedAmount` |
| **`Phase`** `PROVISIONAL` / `FINAL` | **thiếu** 🔴 — hai hồ sơ thẩm định song song sẽ cùng vượt hạn mức |
| **`Reversed` · `ReversalOfId`** | **thiếu** 🔴 — chi trả hỏng phải sinh bút toán đảo, không xóa |
| **`PeriodKey`** Text(20) — **năm hợp đồng** | `PeriodYear` Integer — **năm dương lịch** 🔴 năm hợp đồng không trùng năm dương lịch |
| `CounterType` `MONEY`/`DAY`/`VISIT` | thiếu 🔴 |
| `ChainRunId` | thiếu 🟡 |

### 3.5 `Claim`

Thiếu: **`LossType`** 🔴🔴 (khóa duy nhất của checklist) · `TreatmentFrom`/`TreatmentTo` 🔴 (hiện có `AdmissionDate`/`DischargeDate` — chỉ đúng cho nội trú) · `Queue` 🟡 · `RuleSetRef` 🔴 (điều khoản trích dẫn được) · `EpisodeType` 🟡 (chiếu từ `LossType`) · `FacilityCountry` 🟡 · `RequestReceivedDate` 🟡 · `ProviderCode` 🟡 · `IdpRequestGroupId`/`IdpWorkflowRequestId` 🔴 (cầu nối sang tầng IDP).

**Đã có và đúng:** `IsInNetwork` ✓ — field nhạy cảm tiền nhất của sản phẩm, may là nó có sẵn.

### 3.6 `ClaimDocument`

Thiếu 17 field. Ba cái chặn:
- **`FileHash`** 🔴 — sha256, unique theo `ClaimId`, **và là khóa của `IdpReplay`**. Không có nó thì `ReplayMode` không dựng được.
- **`DocumentType`** 🔴 — `T1-01`…`T2-04` / `unclassified`. Không có nó thì không có checklist.
- **`ChecklistState`** 🔴 — 5 giá trị, hai trạng thái kết thúc.

Thêm: `TemplateId` đang là **Text(50)**, spec là **Long Integer** (template định danh bằng số, không bằng tên).

### 3.7 `ChainRun`

Thiếu **`ThresholdSnapshotJson`** 🔴 (chạy lại không được chấm theo ngưỡng chưa tồn tại lúc đăng ký) và **`TermRegisterVersion`** 🔴.

### 3.8 `AgentRun` — gần khớp nhất

Thiếu **`Replay`** Boolean 🔴 — audit `build-06` §2.2 nói rõ *"nếu chạy ReplayMode, cờ Replay phải hiện rõ; giấu nó là tự sát trước giám khảo kỹ thuật"*.
`AgentName` Text(100) nên thành FK static `AgentCode` 6 giá trị 🟡.

---

## 4. Kiểu và độ rộng

| # | Chỗ | Hiện có | Spec | Quyết |
|---|---|---|---|---|
| 1 | Tiền (mọi nơi) | `Decimal(18,0)` | `Currency` | **Giữ hiện có, sửa spec.** `Decimal(18,0)` ép số nguyên VND chặt hơn `Currency` — đúng tinh thần §6.1 |
| 2 | `Confidence` | `Decimal(37,8)` ở `ClaimDocument`/`ClaimLine`, **`Decimal(5,2)`** ở `Recommendation` | `[0,1]` | 🔴 **Ba entity ba kiểu.** Chuẩn hoá `Decimal(5,4)` |
| 3 | `CoinsurancePct` | `Decimal(37,8)` | Integer basis point | 🔴 sửa entity |
| 4 | `Claim.ClaimNo` | Text(30) | text(20) | 🟢 rộng hơn, vô hại |
| 5 | `Claim.IcdCode` | Text(10) | **text(20)** — mọi MÃ là 20 | 🔴 cắt cụt ICD-10 dài |
| 6 | `Claim.Diagnosis` | Text(200) | `InsuredEvent.DiagnosisText` **text(500)** | 🔴 cắt cụt |
| 7 | `ClaimLine.DecisionReason` | Text(500) | **textarea(2000)** | 🔴 lý do nhân viên nhập |
| 8 | `Recommendation.DecisionReason` | Text(500) | textarea(2000) | 🔴 |
| 9 | `Assessment.State` | Text(20) tự do | Static 4 giá trị | 🟡 |
| 10 | `Recommendation.Code` | Text(20) tự do | Static 2 giá trị `NOTREL`/`NOTCOV`, **từ chối tại adapter** | 🔴 ràng buộc #16 |

> **Dòng 6 và 7 là cùng một lỗi:** một snapshot hoặc một ô lý do **không bao giờ được hẹp hơn field nó sao chép** — `build-01` §6.2 gọi đây là mất dữ liệu *"đúng vào khoảnh khắc không ai đang nhìn"*.

---

## 5. Ràng buộc — 30 cái, hiện 0 kiểm được

API design-time không trả check constraint, nên đây là **danh sách phải kiểm tay trong Studio**, không phải kết luận là thiếu. Sáu cái có thể khẳng định là **không thể tồn tại** vì field/entity của chúng chưa có:

| # (build-01 §7.2) | Ràng buộc | Vì sao chắc chắn chưa có |
|---|---|---|
| 1 | `ParentBenefitCode` cùng `PlanCode` | Không có `ParentBenefitCode` |
| 2 | `CountLimit = 0` là KHÔNG GIỚI HẠN | Không có `CountLimit` |
| 3 | `CopayPct = 'INHERIT'` giải tại snapshot | Không có cơ chế |
| 10 | Σ `ClaimLineBreakdown.Amount` = `CustomerBorneAmount` | Thiếu cả entity lẫn field |
| 11 | `After = Before − Consumed` | Thiếu cả ba field |
| 12 | `PaymentOrder.Amount = ChainRun.TotalPayable` | Thiếu `PaymentOrder` |

Cần kiểm tay: #6 `CardNo` unique · #14 bốn mắt · #20 `FileHash` unique theo claim · #21–23 append-only.

---

## 6. Quyết định: sửa entity hay sửa spec

### 6.1 Sửa SPEC (spec sai hoặc hiện thực tốt hơn) — 5 dòng

| # | Nội dung | Lý do |
|---|---|---|
| S1 | Tiền dùng `Decimal(18,0)`, không phải `Currency` | Ép số nguyên VND chặt hơn, đúng §6.1 hơn chính §6.1 |
| S2 | Giữ `RecEvidence` là entity riêng | Ràng buộc #15 kiểm bằng count, không parse JSON |
| S3 | Giữ `CalcSnapshot` bên cạnh `ChainRun.StepOutputsJson` | JSON là bản ghi bất biến; bảng là hàng hiển thị |
| S4 | Bổ sung `TermRegister` vào §2 | `build-06` có màn Sổ điều khoản, §2 bỏ sót |
| S5 | §2 ghi "15 entity" nhưng liệt kê 16 (§2.1–2.16); §0 ghi "32 entity" nhưng 10+16+7 = 33 | Sửa số đếm |

### 6.2 Sửa ENTITY — theo thứ tự thi hành

| Đợt | Việc | Vì sao thứ tự này |
|---|---|---|
| **A — 4 enum chặn** | Tạo static `LossType` (12) · `ServiceCategory` (20) · `ConditionClass` (6) · `CauseCode` (8). Sửa `ClaimStatus` → 13. Tách `DecisionCode` → `LineDecision` (2) + `DeclineReason` (7). **Xóa `PolicyStatus.Expired`.** Thêm `ACCIDENT` vào `TreatmentType`. Thêm `Value` + `LabelVi` vào **mọi** static entity | Rẻ nhất, và mọi thứ sau đều tham chiếu chúng |
| **B — đầu vào của chuỗi** | `Member`: + `CoverageFrom/To`, `ContinuousCoverFrom`, `RenewalStatus`, `CardNo`, `BenefitLevelNo` · `PolicyBenefitSnapshot`: + `ParentBenefitCode`, `Depth`, `CountLimit/Unit`, `LimitType`, `DeductionPct/Condition`, `EffectiveFrom/To` · tạo `BenefitWaitingPeriod` · `Claim`: + `LossType`, `TreatmentFrom/To`, `RuleSetRef` | Bước 1, 2, 5, 8 không chạy được nếu thiếu |
| **C — đầu ra của chuỗi** | `ClaimLine`: + `CoveredAmount`, `CustomerBorneAmount`, `LineAmount`, `PaidBySocialInsurance`, `LineDecision`, `TreatmentDaysLine`, `OriginalValueJson/EditedValueJson` · tạo `ClaimLineBreakdown` · `LimitLedger`: đổi sang `Before/Consumed/After` + `Phase` + `Reversed` + `PeriodKey` · `ChainRun`: + `ThresholdSnapshotJson`, `TermRegisterVersion` | Chuỗi cần chỗ ghi |
| **D — checklist & chứng từ** | tạo `DocumentChecklistRule` + static `DocumentType` (8), `RequirementLevel` (3), `ChecklistItemState` (5), `PackageState` (6) · `ClaimDocument`: + `FileHash`, `DocumentType`, `ChecklistState`, `DeclaredType`, `PartnerClass` | Portal newclaim / supplement đang chạy trên cái gì đó chưa rõ |
| **E — tầng IDP** | 7 entity `CLM_IDP_Int` + 3 static IDP + entity nối `DocumentSetTemplate`, `IdpWorkflowRequestOcrRequest` | Chặn REST và `ReplayMode` |
| **F — còn lại** | `InsuredEvent` · `Claimant` · `PayoutAccount` · `Beneficiary` · `PaymentOrder` · `ClaimLetter` · `ClaimEvent` · `CrossCheckResult` · `FraudSignal` · `Product`/`Plan`/`BenefitLevel` · `AgentRun.Replay` · sửa 6 độ rộng ở §4 | Cắt được nếu trễ — trừ `AgentRun.Replay` |

**Cắt được khi trễ:** đợt F trừ `AgentRun.Replay`; `FraudSignal` (kịch bản C); `Product`/`Plan` (seed thẳng vào snapshot).
**Không cắt được:** A, B, C.

---

## 7. Ba câu hỏi phải trả lời trước khi sửa

1. **Nhãn tiếng Việt trên 7 màn MyInsurance đang đến từ đâu?** Static entity không có `LabelVi`. Nếu hardcode trong màn thì đợt A phải kèm việc gỡ chúng ra.
2. **Khối "Vì sao Quý khách tự chịu" trên `PortalClaimDetail` đang đọc gì?** `ClaimLineBreakdown` và `CauseCode` đều không tồn tại.
3. **`PortalNewClaim` sinh checklist chứng từ bằng gì?** `DocumentChecklistRule`, `LossType`, `DocumentType` đều không tồn tại.

Ba câu này cùng một dạng: **màn hình đang hiển thị một thứ mà model không có chỗ chứa** ⇒ hoặc nó hardcode, hoặc nó đọc từ một chỗ chưa ai biết. Trả lời xong mới biết đợt A–D là *thêm field* hay là *thêm field cộng gỡ hardcode*.

### 7.1 Trả lời — 20/09, bằng chứng từ MCP `context_actions` (MyInsurance rev 46) + `batch/batch6_portal_customer.html`

| # | Câu hỏi | Trả lời | Bằng chứng | Hệ quả cho đợt A–D |
|---|---|---|---|---|
| 1 | Nhãn tiếng Việt đến từ đâu? | **Hardcode trong 2 Client Action** — bảng ánh xạ `ClaimStatus.Label` → nhãn khách + màu, không đọc entity nào | `Portal_CustomerState` (*"Maps internal ClaimStatus label to customer-facing label and color"*, in `StatusLabel` → out `Label`,`Color`) · `StatusUi` (*"Maps internal ClaimStatus label to UI label and CSS class"*, in `Label` → out `UiLabel`,`CssClass`). Cả hai khoá theo **`Label` (Text)**, không theo `Id` | Đợt A = thêm `Value`+`LabelVi` **và** viết lại 2 action này đọc `ClaimStatus.LabelVi`. Sửa `ClaimStatus` lên 13 record mà không sửa 2 action ⇒ 5 trạng thái mới hiện rỗng trên portal |
| 2 | Khối "Vì sao Quý khách tự chịu" đọc gì? | **Hardcode.** Nguồn thiết kế là mảng `selfPay[]` cố định + 3 đoạn văn tường thuật theo kịch bản A/B/C (thai sản 90 ngày, "168.000 ₫ không thuộc phạm vi"…). App không có entity nào chứa được nó: không `ClaimLineBreakdown`, không `CauseCode`; Service Action portal chỉ trả `Claim`/`ClaimLine`/`ChainRun`/`CalcSnapshot` | `batch6_portal_customer.html`: `<h3>Vì sao bạn tự chịu ${money(c.total-p.payable)}</h3> ${selfPay.filter(...)}` + `note(t("<b>Về khoản này:</b> quyền lợi nha khoa có thời gian chờ 90 ngày…"))`. Không có Service Action nào của MyInsurance trả breakdown | Đợt C tạo `ClaimLineBreakdown` **chưa đủ** — phải nối `PortalClaimDetail` đọc nó và gỡ đoạn văn cố định; nếu không thì demo kịch bản khác A/B/C hiện sai số |
| 3 | `PortalNewClaim` sinh checklist bằng gì? | **Cố định 5 dòng, không theo `lossType`.** Thiết kế: mảng `st[]` cứng cho "Bệnh — ngoại trú"; app: `Portal_SubmitClaim3` *"Creates a new Claim with 5 ClaimDocument rows and 1 AgentRun row. Money fields are always 0"* — nhận `TreatmentTypeId` nhưng không có `LossType`, không tra rule nào | `Portal_SubmitClaim3` inputs: `MemberId`, `EventDate`, `AdmissionDate`, `DischargeDate`, `TreatmentTypeId`, `ProviderName`, `TotalClaimed` → out `NewClaimId` | Đợt D tạo `DocumentChecklistRule` + static `DocumentType`/`RequirementLevel` **và** viết lại `Portal_SubmitClaim3` sinh `ClaimDocument` từ rule theo `LossType`; `PortalNewClaim` phải có input `LossType` (hiện chỉ có `TreatmentType`) |

**Kết luận chung cho §7:** cả ba đều là *"màn hình soạn câu"* — đợt A, C, D đều là **thêm field + gỡ hardcode**, không phải chỉ thêm field. Ước lượng thêm cho mỗi đợt: 2 Client Action (A) · 1 màn + 1 Service Action (C) · 1 Service Action + 1 màn (D).

*Không kiểm được qua API:* biểu thức bên trong widget của `PortalClaimDetail` (context_* không trả nội dung màn). Câu 2 kết luận từ (a) nguồn thiết kế và (b) không tồn tại entity/Service Action nào có thể cấp dữ liệu đó — hai điều kiện đủ để khẳng định không có nguồn động.

---

## 8. Sửa đợt A–F sau khi §7 được trả lời — 20/09

§7.1 đổi bản chất của ba đợt: A, C, D **không phải thêm field, mà là thêm field + gỡ hardcode**. §6.2 viết trước khi biết điều đó. Bảng dưới thay thế phần tương ứng.

### 8.1 Đợt A — thứ tự BẮT BUỘC, không đảo được

`Portal_CustomerState` và `StatusUi` **khóa theo `ClaimStatus.Label` (Text), không theo `Id`**. Hệ quả: thêm record mới hoặc đổi tên record đều làm hai action đó **trả rỗng mà không lỗi**.

| Bước | Việc | Nếu làm sai thứ tự |
|---|---|---|
| A1 | Thêm `Value` + `LabelVi` vào **cả 4** static entity; seed cho 20 record đang có | — |
| A2 | **Viết lại `Portal_CustomerState` và `StatusUi` đọc `ClaimStatus.LabelVi` / `Value`**, bỏ bảng ánh xạ hardcode | — |
| A3 | Nâng `ClaimStatus` 8 → 13 record | Làm A3 trước A2 ⇒ **5 trạng thái mới hiện rỗng trên portal**, không có lỗi runtime |
| A4 | Xóa `PolicyStatus.Expired`; thêm `ACCIDENT` vào `TreatmentType` | Kiểm dữ liệu seed có dòng nào đang trỏ `Expired` không trước khi xóa |
| A5 | Tạo static `LossType` (12) · `ServiceCategory` (20) · `ConditionClass` (6) · `CauseCode` (8) | — |
| A6 | Tách `DecisionCode` → `LineDecision` (2) + `DeclineReason` (7); `ClaimLine.DecisionCodeId` trỏ lại | Kiểm `ClaimLine` đang có dòng nào mang `COVERED`/`PARTIAL`/`PENDING` — chúng không map sang `DeclineReason` |

**Kiểm A xong:** mở 7 màn MyInsurance, mọi chip trạng thái vẫn có chữ. So với `../../../output/bnk-dev-myinsurance/shots-rev6/`.

### 8.2 Đợt C — thêm một màn và một Service Action

Ngoài việc tạo `ClaimLineBreakdown` và sửa `LimitLedger`:

- Viết Service Action trả breakdown theo `causeCode` cho một `ClaimId`.
- **Gỡ đoạn văn cố định** trên `PortalClaimDetail` (hiện là 3 đoạn tường thuật theo kịch bản A/B/C, kể cả câu *"thai sản 90 ngày"* và số `168.000 ₫`) và nối vào Service Action đó.
- Không làm bước này thì **demo một hồ sơ ngoài A/B/C sẽ hiện số sai** — và đó là loại lỗi giám khảo bắt được bằng cách hỏi *"cho xem một hồ sơ khác"*.

### 8.3 Đợt D — `PortalNewClaim` phải có `LossType`

- `Portal_SubmitClaim3` hiện **sinh cứng 5 `ClaimDocument`**, nhận `TreatmentTypeId` chứ không nhận `LossType`.
- Việc thật: thêm input `LossType` vào màn và action, rồi sinh `ClaimDocument` **từ `DocumentChecklistRule`** theo `LossType` + `ClaimantStanding`.
- Dropdown trên màn phải mở **8 giá trị sống**, không phải 4 `TreatmentType` hiện có.

### 8.4 ⚠ Kiểm trước khi sửa: Service Action có bản sao đánh số

Audit portal rev 7 ghi `Portal_SubmitClaim2`; rev 46 có `Portal_SubmitClaim3`. Cùng một dạng với `GetLedgerCounters2`, `GetCalcSnapshotData2`, `Portal_GetCurrentMember2`.

⇒ **Mentor tạo bản mới đánh số thay vì sửa bản cũ.** Trước mỗi đợt, liệt kê `context_actions` và xác nhận **đang sửa đúng bản mà màn hình gọi** — sửa `_2` trong khi màn gọi `_3` là một buổi mất trắng. Dọn bản mồ côi sau khi đợt F xong, không dọn giữa chừng.
