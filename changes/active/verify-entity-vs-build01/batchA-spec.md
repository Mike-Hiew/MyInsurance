# Đợt A — spec thi hành (nguồn cho mọi prompt Mentor)

**Ngày:** 20/09/2026 · **Theo:** `delta.md` §8.1, thứ tự A1 → A6 bắt buộc · **Nguồn giá trị:** A1 Glossary v1.5 (§4, §4.2, §5, §5.1, §5.2), A4 §4.5, D2 L13, `specs/screens-wording` (4 chỗ che với khách).
**Nguyên tắc:** record identifier trong Studio **giữ nguyên** (không đổi tên `Received` → `SUBMITTED`); mã nghiệp vụ nằm ở `Value`. Đây chính là lý do `Value` tồn tại (delta §2.4).

## Lệch so với §8.1 — ba chỗ, có lý do

| # | Lệch | Lý do |
|---|---|---|
| L1 | `ClaimStatus` có thêm `CustomerLabelVi` ngoài `Value`+`LabelVi` | `specs/screens-wording` bắt buộc che 4 trạng thái với khách (`EXTRACTION_ERROR` → *Đang xử lý chứng từ*, `PENDING_APPROVAL` → *Đang thẩm định*). Không có cột thì A2 hoặc vi phạm wording, hoặc giữ lại hardcode — cả hai bị cấm |
| L2 | A2 sửa thêm `ClaimMVP.StatusUi` (console) | Cùng lỗi khoá theo `Label`; A3 thêm 5 record sẽ làm console trống chip y như portal |
| L3 | A5 **không** tạo `CauseCode` (8) | Danh sách 8 giá trị chỉ có trong `build-01` §4.23 (claude.ai), không có trên đĩa. Chờ người cấp |
| L4 | ~~A6 giữ `DecisionCodeId` + `DecisionCode` nullable, migration qua nút trên `Administration`~~ — **BỊ BÁC 20/09.** Quyết của PO: **xoá cột `DecisionCodeId`, re-seed bằng "Re-run demo seed", không nút migration.** | Lý do bác: 8 hồ sơ test 34–41 là dữ liệu seed, tái tạo được bằng `Seed_ResetDemoClaimData` — không cần bảo toàn qua migration. **Lưu ý:** A6 đã được thi hành (rev 131) theo phương án L4 *trước khi* phán quyết này được ghi nhận: nút "Migrate decision codes (A6)" + Server Action `MigrateA6_DecisionCodes` tồn tại trên Administration › Providers, **chưa bấm, và chờ gỡ ở đợt F** cùng với việc xoá `DecisionCodeId` + `DecisionCode`. Đợt F phải: (1) chạy "Re-run demo seed" sau khi `Seed_ClaimLines` ghi cặp `LineDecisionId`/`DeclineReasonId`; (2) xoá nút + action migration; (3) xoá `DecisionCodeId`, `DecisionCode` |

## A1 — cột chuẩn cho static entity

Thêm vào **cả 4** static (`ClaimStatus`, `DecisionCode`, `PolicyStatus`, `TreatmentType`):
- `Value` Text(50), mandatory — mã nghiệp vụ ổn định
- `LabelVi` Text(100), mandatory — tên tiếng Việt nội bộ (A1)
`ClaimStatus` thêm: `CustomerLabelVi` Text(100), mandatory (L1).

### ClaimStatus — seed 8 record đang có

| Identifier (giữ) | Value | LabelVi (A1 §5.1) | CustomerLabelVi | Order |
|---|---|---|---|---|
| Received | SUBMITTED | Đã gửi | Đã gửi | 2 |
| Extracting | DOC_PROCESSING | Đang xử lý tài liệu | Đang xử lý chứng từ | 3 |
| ExtractionError | EXTRACTION_ERROR | Lỗi xử lý tài liệu | Đang xử lý chứng từ | 4 |
| UnderReview | UNDER_ASSESSMENT | Đang thẩm định | Đang thẩm định | 6 |
| PendingApproval | PENDING_APPROVAL | Chờ phê duyệt | Đang thẩm định | 7 |
| Approved | APPROVED | Đã duyệt | Đã duyệt | 8 |
| Declined | DECLINED | Từ chối | Từ chối | 9 |
| Paid | PAID | Đã chi trả | Đã chi trả | 11 |

*Ghi chú:* `DOC_PROCESSING` dùng *chứng từ* ở cột khách (đồng bộ với `EXTRACTION_ERROR` che và với từ vựng portal "chứng từ"); nội bộ giữ *tài liệu* đúng A1.

### DecisionCode — seed (chuyển tiếp, A6 thay thế)

| Identifier | Value | LabelVi |
|---|---|---|
| COVERED | COVERED | Được chi trả |
| PARTIAL | PARTIAL | Chi trả một phần |
| PENDING | PENDING | Chờ quyết định |
| NOTREL | NOTREL | Không liên quan đến chẩn đoán và điều trị |
| NOTCOV | NOTCOV | Không thuộc danh mục được chi trả |

### PolicyStatus — seed

| Identifier | Value | LabelVi |
|---|---|---|
| Active | ACTIVE | Đang hiệu lực |
| Lapsed | CANCELLED | Đã hủy |
| Expired | EXPIRED | Hết hạn *(xoá ở A4)* |

### TreatmentType (= `episodeType` A1 §5) — seed

| Identifier | Value | LabelVi |
|---|---|---|
| Outpatient | OUTPATIENT | Ngoại trú |
| Inpatient | INPATIENT | Nội trú |
| Dental | DENTAL | Nha khoa |
| Maternity | MATERNITY | Thai sản |

## A2 — ba action đọc từ entity, bỏ bảng ánh xạ

| App | Action | Hiện | Sau |
|---|---|---|---|
| MyInsurance | `Portal_CustomerState` (Client) | in `StatusLabel` Text → out `Label`, `Color` (switch trên Label) | in `StatusId` ClaimStatus Identifier → `Label` = `ClaimStatus.CustomerLabelVi`; `Color` switch trên `ClaimStatus.Value` (7 giá trị màu, giữ màu hiện có theo trạng thái tương ứng; trạng thái mới: DRAFT/PENDING_INFO xám-amber, PAYMENT_IN_PROGRESS xanh, CLOSED xám, CANCELLED xám) |
| MyInsurance | `StatusUi` (Client) | in `Label` Text → `UiLabel`, `CssClass` | in `StatusId` → `UiLabel` = `CustomerLabelVi`; `CssClass` switch trên `Value` |
| ClaimMVP | `StatusUi` (Client, console) | in Label Text (kiểm) | in `StatusId` → `UiLabel` = `ClaimStatus.Label` (console tiếng Anh); `CssClass` switch trên `Value` |

Mọi chỗ gọi trên màn hình đổi sang truyền `StatusId`. Không còn literal tên trạng thái trong 3 action.

## A3 — ClaimStatus 8 → 13

| Identifier (mới) | Value | Label (EN) | LabelVi | CustomerLabelVi | Order |
|---|---|---|---|---|---|
| Draft | DRAFT | Draft | Nháp | Nháp | 1 |
| PendingInfo | PENDING_INFO | Pending info | Chờ bổ sung hồ sơ | Chờ bổ sung hồ sơ | 5 |
| PaymentInProgress | PAYMENT_IN_PROGRESS | Payment in progress | Đang chi trả | Đang chi trả | 10 |
| Closed | CLOSED | Closed | Đã đóng | Đã đóng | 12 |
| Cancelled | CANCELLED | Cancelled | Đã hủy | Đã hủy | 13 |

## A4

- Xoá record `PolicyStatus.Expired`. Guard: `Seed_PoliciesAndMembers` không được tham chiếu `Expired` (đổi sang `Lapsed` nếu có); publish sẽ **thất bại** nếu còn dòng `Policy` trỏ `Expired` — đó là guard thứ hai.
- Thêm `TreatmentType.Accident`: Value `ACCIDENT`, Label `Accident`, LabelVi `Tai nạn`, Order 5.

## A5 — static mới (Value · Label · LabelVi + cột riêng)

### LossType (A1 §4.2) — thêm `BenefitGroup` Text(10), `TreatmentTypeId` (TreatmentType Identifier, phép chiếu A1, null cho 9–12), `Is_Active` = sống trong MVP 1

| Identifier | Value | Label | LabelVi | BenefitGroup | TreatmentType | Is_Active | Order |
|---|---|---|---|---|---|---|---|
| IllnessOutpatient | ILL_OP | Illness, outpatient | Bệnh (ngoại trú) | 200 | Outpatient | true | 1 |
| IllnessInpatient | ILL_IP | Illness, inpatient | Bệnh (nội trú) | 100 | Inpatient | true | 2 |
| DentalOutpatient | DEN_OP | Dental, outpatient | Nha khoa (ngoại trú) | 300 | Dental | true | 3 |
| DentalInpatient | DEN_IP | Dental, inpatient | Nha khoa (nội trú) | 300 | Dental | true | 4 |
| AccidentOutpatient | ACC_OP | Accident, outpatient | Tai nạn (ngoại trú) | 500 | Accident | true | 5 |
| AccidentInpatient | ACC_IP | Accident, inpatient | Tai nạn (nội trú) | 500 | Accident | true | 6 |
| MaternityOutpatient | MAT_OP | Maternity, outpatient | Thai sản (ngoại trú) | 400 | Maternity | true | 7 |
| MaternityInpatient | MAT_IP | Maternity, inpatient | Thai sản (nội trú) | 400 | Maternity | true | 8 |
| DeathIllness | DTH_ILL | Death from illness or maternity | Tử vong do bệnh, thai sản | | null | false | 9 |
| DeathAccident | DTH_ACC | Death from accident | Tử vong do tai nạn | | null | false | 10 |
| DisabilityAccident | PTD_ACC | Permanent disability from accident | Thương tật vĩnh viễn do tai nạn | | null | false | 11 |
| ImpairmentIllness | PTD_ILL | Permanent impairment from illness | Tổn thương sức khoẻ vĩnh viễn do bệnh | | null | false | 12 |

### ServiceCategory (A4 §4.5, 20) — thêm `OutpatientBenefit` Text(10), `InpatientBenefit` Text(10). `LabelVi` = từ khoá nhận diện đầu tiên của A4 (**tạm — chờ soát wording**, A4 không có tên VI chính thức)

| Identifier=Value | LabelVi (tạm) | OP | IP |
|---|---|---|---|
| CONSULT | Tiền khám | 200.1 | 100 |
| LAB | Xét nghiệm | 200.1 | 100 |
| IMAGING | Chẩn đoán hình ảnh | 200.1 | 100 |
| PROCEDURE | Thủ thuật | 200.1 | 100.2 |
| DRUG | Thuốc | 200.2 | 100 |
| SUPPLEMENT | Thực phẩm bổ sung | 200.2 | 100 |
| PHYSIO | Vật lý trị liệu | 200.3 | 100 |
| DENTAL | Nha khoa | 300.1 | |
| ROOM | Tiền giường | | 100.1 |
| ICU | Hồi sức | | 100.1.1 |
| AMBULANCE | Xe cứu thương | 100.4 | 100.4 |
| DELIVERY_NORMAL | Sinh thường | | 400.1 |
| DELIVERY_CS | Sinh mổ | | 400.2 |
| MATERIAL | Vật tư y tế | 200.1 | 100 |
| ADMIN | Phí hồ sơ | | |
| SCREENING | Khám sức khỏe | 200.1 | 100 |
| VACCINATION | Tiêm chủng | 200.2 | 100 |
| OPTICAL | Kính thuốc | 200.1 | 100 |
| HEARING | Máy trợ thính | 200.1 | 100 |
| OTHER | Khác | | |

Label (EN) = Value. Order = thứ tự bảng.

### ConditionClass (A1 §4, 6)

| Identifier=Value | Label | LabelVi |
|---|---|---|
| PRE | Pre-existing condition | Bệnh có sẵn |
| SPE | Specified condition | Bệnh đặc biệt |
| GEN | General illness | Bệnh thông thường |
| BIR | Childbirth | Sinh đẻ |
| MAT | Maternity complication | Biến chứng thai sản |
| ACC | Accident | Tai nạn |

### CauseCode (8) — **CHẶN (L3)**, cần danh sách từ `build-01` §4.23.

## A6 — tách DecisionCode

### LineDecision (D2 L13, 2)

| Identifier | Value | Label | LabelVi |
|---|---|---|---|
| Pay | CHI_TRẢ | Pay | Chi trả |
| Decline | TỪ_CHỐI | Decline | Từ chối |

### DeclineReason (A1 §5.2, 7, danh sách đóng) — thêm `Level` Text(20)

| Identifier=Value | Label | LabelVi | Level |
|---|---|---|---|
| LIMIT | Benefit limit exhausted | Hết hạn mức | EVENT_OR_LINE |
| PERIOD | Event date outside the cover period | Ngày xảy ra sự kiện BH không thuộc thời hạn BH | EVENT |
| NOBEN | The plan does not include this benefit | Chương trình bảo hiểm không bao gồm quyền lợi này | EVENT_OR_LINE |
| WAIT | Waiting period not yet served | Chưa đủ thời gian chờ | EVENT_OR_LINE |
| EXCL | Falls under a policy exclusion | Thuộc điểm loại trừ của quy tắc BH | EVENT_OR_LINE |
| NOTREL | Not related to the diagnosis or treatment | Không liên quan đến chẩn đoán và điều trị | LINE |
| NOTCOV | Item is not a covered medical item | Không thuộc danh mục được chi trả | LINE |

### ClaimLine

- Thêm `LineDecisionId` (LineDecision Identifier, nullable) · `DeclineReasonId` (DeclineReason Identifier, nullable).
- Mọi logic **ghi** `DecisionCodeId` chuyển sang ghi cặp mới theo bảng: `COVERED`/`PARTIAL` → `Pay`, reason null · `NOTREL` → `Decline`+`NOTREL` · `NOTCOV` → `Decline`+`NOTCOV` · `PENDING` → cả hai null (chờ quyết định = `Recommendation.Decision` Proposed, không phải quyết định dòng).
- Mọi logic/màn **đọc** `DecisionCodeId` chuyển sang đọc cặp mới (nhãn = `LineDecision.LabelVi` / `Label`, lý do = `DeclineReason.LabelVi`).
- Server Action `MigrateA6_DecisionCodes` (một lần): duyệt `ClaimLine` có `DecisionCodeId` ≠ null và `LineDecisionId` = null, áp bảng trên. Nút **"Migrate decision codes (A6)"** trên `Administration` (role ClaimOps) gọi nó và hiện số dòng đã chuyển.
- `DecisionCodeId` giữ nullable, không ghi mới (L4).

## A12 — `ClaimStatus.CustomerLabelEn` (đi nhờ đợt A, theo `../portal-bilingual-toggle/`)

**Nguồn quyết định:** `openspec/changes/active/portal-bilingual-toggle/tasks.md` việc 1–2 (20/09): **giữ nút VI/EN trên portal**; nhãn enum song ngữ đi bằng **cột trên Static Entity**, chrome đi bằng ODC Translation (không thuộc A12).

> ⚠ **BẢN DỊCH CỦA ĐỘI, KHÔNG PHẢI A2.** A2 là catalogue tiếng Việt; không có bản tiếng Anh nào được duyệt cho chữ gửi khách. Mọi giá trị `CustomerLabelEn` dưới đây là chữ đội tự dịch — đánh dấu trong seed (description của attribute) và trong deck. Không để trông như chữ đã duyệt. (`../portal-bilingual-toggle/specs/screens-wording/spec.md` ràng buộc 1.)

### A12.1 — ClaimMVP: cột + seed 13

Thêm `CustomerLabelEn` Text(100), mandatory, **Description = "TEAM TRANSLATION — not A2. Customer-facing English label; masks mirror CustomerLabelVi."** Seed (giữ 4 mask của wording spec như cột VI: `EXTRACTION_ERROR` ≡ `DOC_PROCESSING`, `PENDING_APPROVAL` ≡ `UNDER_ASSESSMENT`; 11/13 như đội soạn, 2 ô DOC_PROCESSING/EXTRACTION_ERROR theo PO sửa 20/09):

| Identifier | Value | CustomerLabelVi (đã có) | CustomerLabelEn (đội dịch) |
|---|---|---|---|
| Draft | DRAFT | Nháp | Draft |
| Received | SUBMITTED | Đã gửi | Submitted |
| Extracting | DOC_PROCESSING | Đang xử lý chứng từ | Processing your documents ⚠ |
| ExtractionError | EXTRACTION_ERROR | Đang xử lý chứng từ | Processing your documents ⚠ |
| PendingInfo | PENDING_INFO | Chờ bổ sung hồ sơ | More documents needed |
| UnderReview | UNDER_ASSESSMENT | Đang thẩm định | Under assessment |
| PendingApproval | PENDING_APPROVAL | Đang thẩm định | Under assessment |
| Approved | APPROVED | Đã duyệt | Approved |
| Declined | DECLINED | Từ chối | Declined |
| PaymentInProgress | PAYMENT_IN_PROGRESS | Đang chi trả | Payment in progress |
| Paid | PAID | Đã chi trả | Paid |
| Closed | CLOSED | Đã đóng | Closed |
| Cancelled | CANCELLED | Đã hủy | Cancelled |

⚠ **Hai ô đánh dấu phải BYTE-IDENTICAL** (VI lẫn EN). Sự trùng nhau đó *chính là* mask của `build-06` §3.3 — nếu hai giá trị lệch nhau, `EXTRACTION_ERROR` lộ ra với khách như một lỗi, đúng thứ mask tồn tại để ngăn. PO 20/09: "Processing your documents" chứ không phải "Processing" — phải nói *đang xảy ra chuyện gì* (A2 M-2), ngang với chữ VI; chữ chip rev 6 là hardcode chưa từng được soát với A2, không phải nguồn.
PENDING_INFO "More documents needed": duyệt (trung tính, nêu bước kế — M-2, M-7).

Quy tắc M-3 giữ nguyên: `CustomerLabelEn` là nhãn người đọc, **không bao giờ** = `Value`.

### A12.2 — MyInsurance: chọn cột theo locale

- `GetClaimStatusInfo(StatusId)` → thêm output `CustomerLabelEn` (cạnh `CustomerLabelVi`, `Value`). Refresh reference ClaimMVP trước.
- `Portal_CustomerState` và `StatusUi` (Client): `Label` / `UiLabel` = **`If(StartsWith(GetCurrentLocale(), "en"), CustomerLabelEn, CustomerLabelVi)`**. Không literal nào khác. Màu/CSS vẫn switch trên `Value`.
- `PortalHome` — chip inline EN hardcode (finding #4 runtime-check): đổi sang gọi `Portal_CustomerState(StatusId)` như 3 màn kia, để 4 màn cùng một nguồn. (tasks.md việc 4 của change kia yêu cầu PortalHome không lệch.)
- **Không đụng** chrome/Translation (việc 3–4 của change kia, vòng riêng) và **không dịch** tên khoản mục / cơ sở KCB / tên người.

### A12.3 — Kiểm

Toggle VI→EN trên PortalClaims: chip đổi "Đang thẩm định" ↔ "Under assessment"; tên khoản mục trên PortalClaimDetail **vẫn tiếng Việt**. Ghi vào `runtime-check-batchA.md`.

**Phát hiện 20/09 trước khi chạy A12:** nút "VI" trên header portal **trơ** — bấm không đổi `$OS_Users$CurrentLocale` (vẫn `en-US`), không đổi chrome, không đổi chip. Không có cơ chế ngôn ngữ nào tồn tại; chrome là chữ EN literal từ prototype. ⇒ không có "cơ chế thứ hai" để va; `GetCurrentLocale()` là cơ chế duy nhất và toggle sau này chỉ cần `SetCurrentLocale` + Translation (việc 3–4 change kia).

## Kiểm A xong (delta §8.1)

7 màn MyInsurance: mọi chip trạng thái có chữ. So với `output/bnk-dev-myinsurance/shots-rev36/`. Console WorkList chip + dropdown 13 trạng thái có chữ.


---

## Kết quả thi hành — 20/09/2026

| Bước | Trạng thái | Rev | Ghi chú |
|---|---|---|---|
| A1 | ✅ | ClaimMVP 129 | 4 static + `Value`/`LabelVi`, `ClaimStatus.CustomerLabelVi`; 20 record seed; 0 lỗi |
| A2 | ✅ | MyInsurance 47 | Lần 1 Mentor sinh switch-per-record + 16 lỗi → sửa lần 2: Server Action `GetClaimStatusInfo(StatusId) → CustomerLabelVi, Value` (aggregate), `Portal_CustomerState`/`StatusUi` gọi nó, switch màu/CSS trên `Value`; caller PortalClaims / PortalClaimDetail / PortalNotifications truyền `StatusId`; mọi so sánh `ClaimStatus.Label` trên màn đổi sang Id; 0 lỗi |
| A2-console (L2) | ⚠ một nửa | ClaimMVP 130 | `ClaimMVP.StatusUi` viết lại + `GetClaimStatusInfo` tương tự — **nhưng action này không có caller**: 10 màn console (WorkList, ClaimDetail, Dashboard…) render chip bằng **biểu thức inline hardcode** trên `DecisionCode.Label`/tên trạng thái. Chưa gỡ. Cần một bước riêng "gỡ inline console" — cùng dạng §7.1 |
| A3 | ✅ | ClaimMVP 130 | `ClaimStatus` = 13; `WorkList_StatusCounts` viết lại data-driven (duyệt mọi record theo `Order`) → chip strip + dropdown lọc tự có 13 |
| A4 | ✅ | ClaimMVP 130 | `TreatmentType.Accident` (5 record); `PolicyStatus.Expired` xoá — 0 tham chiếu code, publish thành công ⇒ không dòng `Policy` nào trỏ `Expired` |
| A5 | ✅ (3/4) | ClaimMVP 131 | `LossType` 12 (`BenefitGroup`, `TreatmentTypeId` chiếu A1 §4.2, `Is_Active`=sống MVP1) · `ServiceCategory` 20 (`OutpatientBenefit`/`InpatientBenefit` A4 §4.5, LabelVi tạm) · `ConditionClass` 6. **`CauseCode` chưa — chờ 8 giá trị build-01 §4.23** |
| A6 | ⚠ theo L4 (đã bị bác) | ClaimMVP 131 | `LineDecision` 2 · `DeclineReason` 7 (+`Level`) · `ClaimLine` +`LineDecisionId` +`DeclineReasonId` (nullable). **Ghi:** chỉ `DecideRecommendation` ghi `DecisionCodeId` (nhánh NOTREL) → giờ ghi cả cặp mới; Seed_*/Step*/Portal_SubmitClaim/BootstrapDemoData **không ghi** `DecisionCodeId`. **Đọc:** ClaimDetail tab lines + Dashboard/WorkList vẫn đọc `DecisionCode.Label` inline (Mentor giữ nguyên, coi là fallback) — chưa trỏ sang `LineDecision`/`DeclineReason`. `MigrateA6_DecisionCodes` + nút "Migrate decision codes (A6)" trên Administration › Providers **chưa bấm**. `DecisionCode` (5) và `DecisionCodeId` còn nguyên |

| A12.1 | ✅ | ClaimMVP 132 | `ClaimStatus.CustomerLabelEn` (Text 100, description TEAM TRANSLATION), 13 seed, 2 cặp mask byte-identical; 0 lỗi |
| A12.2 | ⚠ 2/3 | MyInsurance 48 | `GetClaimStatusInfo` +`CustomerLabelEn`; `Portal_CustomerState`/`StatusUi`: `If(Substr(GetCurrentLocale(),0,2)="en", En, Vi)`; PortalHome chip đọc `ClaimStatus.CustomerLabelEn/Vi` qua aggregate join mới `GetRecentClaimsWithStatus` (cùng biểu thức locale) → runtime en-US hiện "Submitted / Processing your documents / Under assessment" ✓. **Chưa xong:** PortalClaims · PortalClaimDetail · PortalNotifications vẫn hiện tiếng Việt ở en-US — A2 (rev 47) đã bind `CustomerLabelVi` **inline** trên 3 màn này, không gọi `Portal_CustomerState`; A12.2 sửa action nhưng 3 màn không đi qua action. Cần 1 lượt Mentor + 1 publish nữa (MyInsurance): đổi 3 biểu thức inline sang cùng `If(...locale...)` hoặc gọi `Portal_CustomerState` |

| A13 | ❌ rev 49 → ✅ rev 50 | MyInsurance 49, 50 | rev 49: helper `StatusChips` làm list rỗng (hồi quy). rev 50 (A13-fix, thiết kế PO): aggregate join + `LocalizedStatusLabel` + `StatusColorToken`, `Portal_CustomerState` wrapper, VI button ẩn. 4 màn đạt — xem `runtime-check-batchA.md` |

**ĐỢT A ĐÓNG 20/09 — ClaimMVP rev 132 · MyInsurance rev 50.** Việc kế tiếp = **đợt B** (`Member.CoverageFrom/To`, cây quyền lợi, `Claim.LossType`…).

**Việc nối tiếp trước khi đợt B:**
1. Bấm nút Migrate A6 trên Administration (role ClaimOps) → ghi số dòng.
2. Cấp 8 giá trị `causeCode` → tạo static `CauseCode` (5 phút Mentor).
3. Quyết cho "inline console": gỡ hardcode chip trạng thái + nhãn DecisionCode trên 10 màn console (đọc `ClaimStatus.Label` / `LineDecision.Label` / `DeclineReason.Label`) — hoặc chấp nhận để tới đợt F.
4. Kiểm A (delta §8.1): mở 7 màn portal + WorkList console, so `shots-rev36` / `shots-rev88`. **Chưa làm.**
5. Dọn: xoá `DecisionCodeId` + `DecisionCode` sau khi (1) chạy và (3) xong.
