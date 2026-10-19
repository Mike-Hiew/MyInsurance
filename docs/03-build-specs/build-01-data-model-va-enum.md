# Build 01 — Data model & Enum

| Mục | Giá trị |
|---|---|
| Tài liệu | Build 01 — Data model & Enum cho "Agentic Health Claim Adjudication" trên OutSystems ODC + Agent Workbench |
| Đội | BnK Solution — dựng lại từ đầu |
| Mốc | Hackathon 07–08/10/2026 |
| **Chuẩn gốc** | **Claim MVP** — D1/D2/D3 + A4/A5. **KHÔNG phải MBAL-IDP.** |
| Phiên bản | v2.0 — thay đổi nền tảng so với v1.0 (v1.0 lấy MBAL-IDP làm chuẩn; xem §9) |
| Nguồn verbatim | `D2_Functional_Specification_v0.6.md` (Part III data model, §2.3, §2.9, §4.5, §4.6, §5.5, §5.10, §7.1) · `D3_Integration_and_Data_Contract_Spec_v2.3.md` (§1.2, §1.3, §1.5, I-05, I-07, I-08, I-09, I-15, I-16) · `A4_Document_Type_and_Field_Catalogue_v1.3.md` (§1, §2, §3, §4, §5, §6, §7, §8) · `IDP_APIs_for_Claim_MVP_v1.0.md` (template id thật, tenant 176, enum trạng thái — gọi thật 03/09/2026 lên staging) |

---

## 0. Nguyên tắc

### 0.1 Chuẩn gốc là Claim MVP

Mọi tên entity, tên field, kiểu dữ liệu, độ rộng và giá trị enum trong tài liệu này lấy từ bộ deliverable **Claim MVP** (D1 BRD, D2 FS v0.6, D3 v2.3, A4 v1.3, A5 v1.5). Đây là **chuẩn chính**, không phải mô hình vay mượn.

**Bốn hệ quả bắt buộc:**

1. **Loại chứng từ** dùng bộ **7 loại `T1-01` … `T2-03` theo 3 tier** của A4 v1.3 (cộng `T2-04` mới ở v1.3, chưa chốt tên). **Không dùng `CL05` … `CLM_04`.**
2. **Phân loại tổn thất** dùng **`lossType` 12 giá trị** (pathology × setting + 4 giá trị tử vong/thương tật vĩnh viễn). **Không dùng 4 `claim_type` `MEDICAL_*`.**
3. **Template IDP** là bộ của tenant **176**: `5384`/`5127` giấy ra viện · `5383` bảng kê · `5299` đơn thuốc · `5340` xét nghiệm · `5312` biên lai *(tùy chọn)*. **Không dùng `777`/`776`/`1091`/`2645`/`1141`.**
4. **Ba entity MBAL — `MBALDocsetMap`, `MBALTemplateMap`, `MBALStatistics` — BỎ HẲN.** Thay bằng cấu hình checklist theo `lossType` của A4 §6 / D2 §4.6, dựng thành entity `DocumentChecklistRule` (§2.15).

### 0.2 Hai câu luật xuyên suốt

> **① Tiền là code, phán đoán là agent.** Mọi số tiền do `RunAdjudicationChain` sinh ra, chạy lại được, kiểm toán được. Agent đọc, diễn giải, đề xuất và giải thích; **không agent nào ghi được một field tiền**.
>
> **② Sự cố vận hành không bao giờ được diễn đạt thành kết quả bồi thường.** *(D2 §2.10)* Trích xuất kẹt, InsureMO không với tới được, chi trả hỏng — mỗi thứ sinh ra một **trạng thái vận hành** kèm hàng đợi vận hành. **Không màn hình nào được hiển thị cho khách một thông điệp mà một sự cố hệ thống có thể bị nhầm thành một quyết định về hồ sơ của họ.**

Câu thứ ba, nhỏ hơn nhưng hỏng thì đắt:

> **③ Không có gì được sửa âm thầm.** *(D2 §2.10)* Khi một giá trị từ IDP mâu thuẫn với giá trị từ chứng từ khác, màn hình **hiện cả hai và gắn cờ ngay tại field**; nó không bao giờ tự chọn một. **Việc của hệ thống là nhận ra, không phải là giải quyết.**

### 0.3 Ba hệ master dữ liệu — biết ai làm chủ cái gì

| Hệ | Làm chủ | Trong build hackathon |
|---|---|---|
| **InsureMO** | `product`, `plan`, `benefit_schedule_line`, `benefit_waiting_period`, `policy`, `benefit_level`, `insured_member` | **Mock** — seed vào Entity ODC qua interface `CLM_Core_Mock`. Đổi sang InsureMO thật sau là hiện thực lại cùng một interface |
| **Claim Platform (ta)** | `payout_account`, `beneficiary`, `claimant`, và **toàn bộ** tầng claim §2 | Xây thật |
| **IDP** | Trích xuất (`I-07`/`I-08`/`I-09`) và đánh giá lâm sàng (`I-15`/`I-16`) | Gọi thật lên staging tenant 176, có `ReplayMode` để demo offline |

**Quy tắc của Framework v2.0 §2.1, một câu:** *policy business goes to InsureMO, claim business comes to us.*

> **Một field mang dấu InsureMO là thứ ta phải ĐI XIN và phải kiểm chứng là đã nhận được. Một field mang dấu Claim Platform là thứ ta XÂY.** Dấu này là phần chịu lực của bảng, không phải chú thích.

### 0.4 Quy ước đặt tên entity trong ODC

| Quy tắc | Nội dung |
|---|---|
| Module | `CLM_Core` (entity + chain + gate + sổ cái) · `CLM_IDP_Int` (7 entity tích hợp §3) · `CLM_Agents` (Agent Workbench, **không sở hữu entity nào**) · `CLM_Console` · `CLM_Portal` |
| Tên Entity | **PascalCase, số ít.** D2 viết `benefit_schedule_line` (snake_case) vì đó là quy ước tài liệu; trong ODC là `BenefitScheduleLine` |
| Attribute | **PascalCase.** D2 `copay_pct` → ODC `CopayPct`. **Giá trị bên trong không đổi** |
| **Tên wire** | D3 §1.2 chốt **một tên cho một khái niệm** trên payload: `claimNo`, `eventNo`, `requestId`, `assessmentId`, `instructionRef`, `importId`, `policyNo`, `memberId`, `ruleSetRef`. **Attribute ODC lấy tên wire khi có**, không tự đặt tên khác |
| Hai chỗ tên wire khác tên field | `benefit_schedule_line.level` → wire **`depth`** (vì *level* còn nghĩa là `benefit_level_no`) · `copay_pct` → wire `coPaymentBp` |
| Identifier | ODC tự sinh `Id` (Long Integer) làm khóa kỹ thuật. **Khóa nghiệp vụ lưu song song** ở attribute riêng (`ClaimNo`, `PolicyNo`, `MemberId`, `AccountRef`) và **không bao giờ parse để lấy nghĩa** |
| Static Entity | Mọi enum ở §4 dựng bằng **Static Entity** với `Value` + `Label` + `LabelVi`. Mã enum là **tiếng Anh, UPPERCASE_SNAKE** *(D2 §2.6)*; nhãn tiếng Việt nằm ở `LabelVi` và **không bao giờ hiển thị mã thô cho khách** |
| Ngoại lệ tiếng Việt | 5 enum của Claim MVP có mã **viết bằng tiếng Việt HOA có dấu** (`CHI_TRẢ`, `CHƯA_NHẬN`, `PHÊ_DUYỆT`, `BẢN_SAO`, `KHÁCH_HÀNG`). Đây là verbatim D2 — giữ nguyên, kèm ghi chú rằng chúng vi phạm §2.6 của chính D2 và A1 là nơi hòa giải |

### 0.5 Quy ước đánh dấu độ tin cậy

| Dấu | Nghĩa |
|---|---|
| *(verbatim)* | Trích đúng từ nguồn đã đọc |
| **⚠ cần xác thực bằng gọi thật** | Chưa có bằng chứng từ response thật hoặc từ source. **Không đưa vào ODC trước khi gọi thật một lần** |
| **⚠ cần chốt với PO / A1 / A4** | Mâu thuẫn giữa hai tài liệu, hoặc tập giá trị chưa có chủ. Quyết định nghiệp vụ, không phải kỹ thuật |
| `[InsureMO]` · `[Claim Platform]` · `[IDP]` | Hệ làm chủ, theo §0.3 |

---

## 1. Entity tầng hợp đồng và thành viên

> **Mười entity**, theo D2 Part III §3.2. Bảy entity đầu `[InsureMO]` — trong hackathon là **mock seed**, nhưng **schema phải đúng ngay từ đầu** vì đổi sang InsureMO thật là đổi hiện thực, không đổi mô hình. Ba entity cuối `[Claim Platform]` — xây thật.

```
InsureMO
  Product ──< Plan ──< BenefitScheduleLine ──< BenefitWaitingPeriod
                            (tự trỏ trên ParentBenefitCode)
  Policy ──< BenefitLevel ─── Plan
     │
     └──< InsuredMember

Claim Platform
  PayoutAccount ─── InsuredMember (bằng MemberId)
  Beneficiary   ─── PayoutAccount
  Claimant      ─── InsuredMember (bằng MemberId)   một dòng một hồ sơ
```

### 1.1 `Product` `[InsureMO]` — F01, F05

| Attribute | Field D2 | Kiểu ODC | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| `Code` | `code` `text(16)` | Text(16) | ✅ | **Khóa chính.** `HC` trong pilot. Wire: `productCode` |
| `Name` | `name` `text(120)` | Text(120) | ✅ | `HealthCare Pilot` |
| `Line` | `line` `enum(productLine)` | Text(20) | ✅ | `HEALTH_INDIVIDUAL` trong pilot |
| `Currency` | `currency` `enum(currency)` | Text(20) | ✅ | **`VND` duy nhất.** Đơn vị tiền nằm ở **product, không ở plan** — giữ ở hai nơi là để chúng mâu thuẫn |
| `TermMonths` | `term_months` `int` | Integer | ✅ | `12`. **Mô tả thôi** — không có gì tính kỳ hạn từ nó |
| `Status` | `status` `enum(productStatus)` | Text(20) | ✅ | `ACTIVE` · `WITHDRAWN`. **⚠ Tập này là của D2 vì chưa nguồn nào liệt kê** — ánh xạ sang tập của InsureMO là `D2-03` |
| `EffectiveFrom` | `effective_from` `date` | Date | ✅ | Chịu lực cho ngữ nghĩa *as-at-a-date* của F07 |
| `EffectiveTo` | `effective_to` `date` | Date | — | `null` = mở |

### 1.2 `Plan` `[InsureMO]` — F01

| Attribute | Field D2 | Kiểu ODC | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| `ProductCode` | `product_code` `lookup(product)` | Text(16) | ✅ | Khóa ngoại |
| `PlanCode` | `plan_code` `text(16)` | Text(16) | ✅ | Khóa chính cùng `ProductCode`. `HC-STD` · `HC-PLS` · `HC-PRM`. Wire: `planCode` |
| `Name` | `name` `text(60)` | Text(60) | ✅ | `Standard` · `Plus` · `Premium` |

**Ba dòng trong MVP 1.** *(verbatim)* **Tuyệt đối không có cột hạn mức ở cấp plan** — mọi hạn mức là một `BenefitScheduleLine`.

> Đây là ràng buộc ta thực thi **bằng cách từ chối đọc** chứ không phải bằng cách không xây: một hạn mức cấp plan mà InsureMO có mang và ta bỏ qua là một hạn mức ràng buộc hợp đồng nhưng không ràng buộc quyết toán.

### 1.3 `BenefitScheduleLine` `[InsureMO]` — F07

**Một dòng mỗi plan mỗi quyền lợi. 51 dòng trong pilot** — 17 quyền lợi × 3 plan.

| Attribute | Field D2 | Kiểu ODC | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| `PlanCode` | `plan_code` | Text(16) | ✅ | Khóa chính phần 1 |
| `BenefitCode` | `benefit_code` `text(16)` | Text(16) | ✅ | Khóa chính phần 2. Ký hiệu chấm `100`, `100.1`, `100.1.1`. **Không bao giờ parse để lấy cấu trúc cây** — con trỏ cha mới là chuẩn, dấu chấm chỉ là quy ước đặt tên. Wire: `code` trong cây, `benefitCode` ở mọi chỗ khác |
| `ParentBenefitCode` | `parent_benefit_code` | Text(16) | — | `null` ở gốc. **Phải trỏ tới dòng có CÙNG `PlanCode`.** Wire: `parentCode` |
| `Depth` | `level` `int` | Integer | ✅ | 1 ở gốc, 2 con, 3 cháu. **Tên wire là `depth`, KHÔNG phải `level`** |
| `Name` | `name` `text(120)` | Text(120) | ✅ | Tên tiếng Anh nội bộ. Tên tiếng Việt cho khách lấy từ A1 |
| `LimitType` | `limit_type` `enum(limitType)` | Text(20) | ✅ | `annual` · `per_event` · `per_day`. **Không lên wire** |
| `AnnualCap` | `annual_cap` `money` | Currency | ⚠ C | Bắt buộc khi `LimitType = annual`. **`null` nghĩa là `included`**: không có trần riêng, bị chặn bởi tổ tiên gần nhất có trần. Wire: `annualSumInsured` |
| `PerEventCap` | `per_event_cap` `money` | Currency | ⚠ C | Bắt buộc khi `LimitType = per_event`. Wire: `perEventLimit` |
| `PerDayCap` | `per_day_cap` `money` | Currency | ⚠ C | Bắt buộc khi `LimitType = per_day`. Wire: `perDayLimit` |
| `CountLimit` | `count_limit` `int` | Integer | ✅ | **Chiều hạn mức thứ hai, độc lập với tiền.** **`0` nghĩa là KHÔNG GIỚI HẠN** — xem cảnh báo dưới. Wire: `maxCount` |
| `CountUnit` | `count_unit` `enum(countUnit)` | Text(20) | ⚠ C | `DAY` · `VISIT`. Bắt buộc khi `CountLimit > 0`; cho phép `null` khi `CountLimit = 0`. Wire: `countUnit` |
| `CopayPct` | `copay_pct` **`text(8)`** | **Text(8)** | ✅ | **Là TEXT, không phải số** — nó mang hai loại giá trị: basis point dạng chữ số, hoặc chuỗi `INHERIT`. Wire: `coPaymentBp` |
| `DeductionPct` | `deduction_pct` `pct` | Integer | — | Basis point. `null` khi quyền lợi không có khấu trừ. Wire: `deductionRatioBp` |
| `DeductionCondition` | `deduction_condition` | Text(30) | ⚠ C | Bắt buộc khi `DeductionPct` khác null, và `null` khi nó null. **Một thành viên duy nhất trong MVP 1: `PROVIDER_OFF_NETWORK`.** Wire: `deductionCondition` |
| `EffectiveFrom` | `effective_from` `date` | Date | ✅ | **Toàn bộ F07 dựa trên field này** |
| `EffectiveTo` | `effective_to` `date` | Date | — | `null` = còn hiệu lực |

#### ⚠⚠ Ba sentinel — mỗi cái đọc sai vẫn HỢP LỆ VỀ SỐ HỌC, nên hỏng âm thầm

| Sentinel | Giá trị | Đọc sai thì sao |
|---|---|---|
| **`CountLimit = 0`** | **Không giới hạn**, không phải "cho phép 0 ngày / 0 lượt" | Consumer coi `0` là trần số → **từ chối mọi hồ sơ** trên `100.2` phẫu thuật, `200.1` khám và `200.2` thuốc — tức là phần lớn hồ sơ của sản phẩm này. **Không có cách nào diễn đạt "cho phép 0" và cũng không cần**: quyền lợi không chi trả thì không nằm trong biểu |
| **`CopayPct = 'INHERIT'`** | Lấy tỷ lệ của tổ tiên gần nhất **có giá trị nguyên**, giải tại thời điểm snapshot, ghi số đã giải lên hồ sơ. Trong pilot: `200.1`, `200.2`, `200.3`, `300.1` đều kế thừa | `INHERIT` chạy tới gốc mà không gặp số nguyên = **lỗi biểu quyền lợi, fail tại biên adapter**. Ba phương án thay thế đều bị loại có lý do: `null` đã có nghĩa *không biết*; bỏ property đi thì theo luật ignore-unknown-fields sẽ **âm thầm trả đủ** một quyền lợi đồng chi trả 20%; một cờ dạng số thì hợp lệ về số học và do đó im lặng. **Quyền lợi thực sự KHÔNG có đồng chi trả mang số nguyên `0`** — một tỷ lệ bằng không, không phải sự vắng mặt của một tỷ lệ |
| **`waitingPeriodDays` vắng mặt** | **Tổ hợp không thể xảy ra**, không phải "chờ 0 ngày" | Hồ sơ sinh ra tổ hợp đó là **lỗi dữ liệu**, không phải quyền lợi chờ 0 ngày |

> **Đây là thứ sắc nhất mà Part III đòi ở một nhà cung cấp.** Ở v0.1 ta bảo đảm chúng bằng cách tự viết cái store. Giờ ta bảo đảm chúng bằng cách **đặc tả trong hợp đồng, kiểm tại adapter, và thử trên sandbox trước khi tin**.

**Tại sao `CopayPct` là `text(8)` chứ không phải `pct`:** giữ dạng text là thứ làm cho **phép tính trên một sentinel chưa giải bị fail to tiếng thay vì âm thầm cho ra số không**.

**Cặp khấu trừ và điều kiện kích hoạt.** Đây là **cơ chế duy nhất trong sản phẩm phụ thuộc vào một sự kiện của ĐỢT ĐIỀU TRỊ chứ không phải của quyền lợi**. InsureMO giữ cả hai nửa và **không đánh giá nửa nào**. `PROVIDER_OFF_NETWORK` do Claim Platform trả lời, đối chiếu cờ `InNetwork` trên danh mục cơ sở y tế (F57), **theo từng sự kiện bảo hiểm chứ không theo hồ sơ** — vì một hồ sơ có thể mang một lượt trong mạng lưới và một lượt ngoài mạng lưới. Trong pilot cặp này chỉ nằm ở `200` và `300`, ở mức 20%, và **không kế thừa xuống đâu cả**.

### 1.4 `BenefitWaitingPeriod` `[InsureMO]` — F07

Dạng chuẩn hóa của **một field** trên `BenefitScheduleLine`. Trả về dưới dạng map `waitingPeriodDays`.

> **Đây KHÔNG phải entity thứ mười một.** Thời gian chờ theo nhóm bệnh là một **field** của dòng biểu quyền lợi; sáu nhóm bệnh không thể là một cột vô hướng nên nó được chuẩn hóa. **Thứ ta yêu cầu là cái map trên node**, còn InsureMO mô hình hóa nó thành bảng, map hay sáu cột là việc của InsureMO.

| Attribute | Field D2 | Kiểu ODC | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| `PlanCode` | `plan_code` | Text(16) | ✅ | Khóa chính phần 1 |
| `BenefitCode` | `benefit_code` | Text(16) | ✅ | Khóa chính phần 2. **Chỉ mang ở gốc nhóm quyền lợi** — `100`, `200`, `300`, `400`, `500`. Hậu duệ lấy theo gốc của nó |
| `ConditionClass` | `condition_class` `enum(conditionClass)` | Text(20) | ✅ | Khóa chính phần 3. **Đúng sáu giá trị** — §4.4 |
| `Days` | `days` `int` | Integer | ✅ | Ngày tính từ **ngày bắt đầu bảo hiểm CỦA THÀNH VIÊN**, không phải ngày hiệu lực hợp đồng |

**Pilot mang 17 trong 30 tổ hợp gốc × nhóm bệnh mỗi plan** *(verbatim)*: `400` thai sản chỉ mang `BIR` 270 và `MAT` 90; `300` nha khoa không mang `SPE`; `500` tai nạn mang cả bốn ở mức 0.

> **Không ai ở thượng nguồn làm phép tính thời gian chờ.** InsureMO trả `days` và trả `coveragePeriod.from`. **Phép so với ngày dịch vụ là của Claim Platform.** Hai sự kiện và không có phép trừ nào — **sự phân chia đó là toàn bộ thiết kế**.

### 1.5 `Policy` `[InsureMO]` — F05

| Attribute | Field D2 | Kiểu ODC | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| `PolicyNo` | `policy_no` `text(20)` | Text(20) | ✅ | Khóa chính. Format `POL-YY-NNNNNN`. **Không bao giờ parse để lấy nghĩa.** Wire: `policyNo` |
| `Type` | `type` `enum(policyType)` | Text(20) | ✅ | `INDIVIDUAL` · `GROUP` |
| `HolderName` | `holder_name` `text(200)` | Text(200) | ✅ | Người với `INDIVIDUAL`, công ty với `GROUP`. *Bên mua bảo hiểm* — **không nhất thiết là một thành viên được bảo hiểm** |
| `HolderIdNumber` | `holder_id_number` `text(20)` | Text(20) | ✅ | CCCD với người, mã số thuế với công ty |
| `PeriodFrom` | `period_from` `date` | Date | ✅ | **Bao gồm.** Wire: `policyPeriod.from` |
| `PeriodTo` | `period_to` `date` | Date | ✅ | **Bao gồm.** Phải ≥ `PeriodFrom`. Wire: `policyPeriod.to` |
| `Status` | `status` `enum(policyStatus)` | Text(20) | ✅ | **`ACTIVE` · `CANCELLED` — đúng hai thành viên.** Xem cảnh báo |
| `Source` | `source` `enum(policySource)` | Text(20) | ✅ | `ISSUED_J1` · `IMPORTED` |
| `RuleSetRef` | `rule_set_ref` `text(200)` | Text(200) | ✅ | **Bộ quy tắc / bản quy tắc bảo hiểm có hiệu lực** — chính là tham chiếu in trên tài liệu khách hàng đang cầm. Snapshot **tại lúc đăng ký hồ sơ**. **Không có gì đọc nó**: không bước nào của chain tra nó. Nó tồn tại để một quyết định từ chối **trích dẫn được điều khoản trong tài liệu khách đang cầm** |
| `PremiumPaid` | `premium_paid` `bool` | Boolean | ✅ | Được báo cáo, **không bao giờ bị phía hợp đồng kiểm** |

> **⚠ Hai giá trị trạng thái, và HẾT HẠN KHÔNG nằm trong đó.** Hợp đồng quá `PeriodTo` **không phải `EXPIRED`** — nó trượt phép kiểm ngày và trả `AFTER_POLICY_END`. Một trạng thái `EXPIRED` sẽ là **nguồn sự thật thứ hai cho cùng một sự kiện**, và hai nguồn sẽ lệch nhau ngay lần đầu một batch job không chạy.
>
> **Đây là yêu cầu thật đối với InsureMO:** nếu mô hình hợp đồng của nó mang một vòng đời nhiều hơn hai giá trị — mà một nền tảng có thế mạnh là phát hành thì gần như chắc chắn có — thì **ánh xạ từ tập của nó sang tập của ta phải được đặc tả, thống nhất và ghim lại**, vì **mọi giá trị chưa ánh xạ đều âm thầm trở thành "không phải `ACTIVE`" và từ chối hồ sơ.**

### 1.6 `BenefitLevel` `[InsureMO]` — F05

Một hợp đồng có thể mang nhiều. Mỗi thành viên trỏ tới đúng một.

| Attribute | Field D2 | Kiểu ODC | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| `PolicyNo` | `policy_no` | Text(20) | ✅ | Khóa chính phần 1 |
| `BenefitLevelNo` | `benefit_level_no` `int` | Integer | ✅ | Khóa chính phần 2. Tuần tự từ 1 trong hợp đồng. Wire: `benefitLevelNo` |
| `PlanCode` | `plan_code` | Text(16) | ✅ | Ba tầng nào được cấp |
| `EffectiveFrom` | `effective_from` `date` | Date | ✅ | **Phải nằm trong kỳ hạn hợp đồng** |
| `EffectiveTo` | `effective_to` `date` | Date | — | `null` = tới hết kỳ hạn. Phải nằm trong kỳ hạn |

> **Không bao giờ gọi trống là "level".** *Level* trên dòng biểu quyền lợi là **độ sâu trong cây**; *benefit level* là **một plan gắn vào một hợp đồng**. Hai khái niệm khác nhau, cả hai đều là số nguyên, và D3 đổi tên cái thứ nhất thành `depth` chính vì **một payload không được mang cả hai nghĩa**.

### 1.7 `InsuredMember` `[InsureMO]` — F05

| Attribute | Field D2 | Kiểu ODC | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| `MemberId` | `member_id` `text(20)` | Text(20) | ✅ | Khóa chính. Format `MBR-NNNNNN`. Wire: `memberId` |
| `PolicyNo` | `policy_no` | Text(20) | ✅ | |
| `MemberNo` | `member_no` `int` | Integer | ✅ | Tuần tự từ 1 trong hợp đồng. Unique cùng `PolicyNo` |
| `FullName` | `full_name` `text(200)` | Text(200) | ✅ | Tiếng Việt có dấu. **Tìm kiếm KHÔNG phân biệt dấu** |
| `DateOfBirth` | `date_of_birth` `date` | Date | ✅ | Wire: `dateOfBirth` |
| `Sex` | `sex` `enum(sex)` | Text(20) | ✅ | `M` · `F` |
| `IdNumber` | `id_number` `text(20)` | Text(20) | — | **Cho phép null, và đây là chỗ chịu lực.** Trẻ em không có CCCD. **Tìm theo `idNumber` do đó KHÔNG tìm ra trẻ em** — đó là lý do `CardNo` tồn tại |
| `CardNo` | `card_no` `text(20)` | Text(20) | ✅ | *Số thẻ bảo hiểm.* Cấp cho **mọi** thành viên **kể cả người phụ thuộc**. **Unique toàn danh bộ.** **Khóa tìm kiếm chính lúc tiếp nhận hồ sơ** |
| `Relationship` | `relationship` `enum(relationship)` | Text(20) | ✅ | `SELF` · `SPOUSE` · `CHILD` · `PARENT` · `EMPLOYEE`. Quan hệ với **bên mua bảo hiểm**, không phải với thành viên khác |
| `BenefitLevelNo` | `benefit_level_no` | Integer | ✅ | Cùng `PolicyNo`. Cover của thành viên này lấy từ level nào |
| `CoverageFrom` | `coverage_from` `date` | Date | ✅ | Bao gồm. **Có thể MUỘN HƠN `Policy.PeriodFrom`** — người tham gia giữa kỳ. Wire: `coveragePeriod.from` |
| `CoverageTo` | `coverage_to` `date` | Date | ✅ | Bao gồm. **Có thể SỚM HƠN `Policy.PeriodTo`** — người rời đi. Wire: `coveragePeriod.to` |
| `ContinuousCoverFrom` | `continuous_cover_from` `date` | Date | ✅ | **Ngày bắt đầu tham gia LIÊN TỤC của thành viên này** — có thể sớm hơn `CoverageFrom` và có thể nằm trong một hợp đồng trước đó. **`null` khi VÀ CHỈ KHI `RenewalStatus = LẦN_ĐẦU`.** Wire: `continuousCoverFrom` |
| `RenewalStatus` | `renewal_status` `enum(renewalStatus)` | Text(20) | ✅ | `LẦN_ĐẦU` · `TÁI_TỤC` · `KHÔNG_XÁC_ĐỊNH`. Thuộc tính của **sự tham gia**, không phải của hợp đồng và không phải của con người. Wire: `renewalStatus` |
| `Status` | `status` `enum(memberStatus)` | Text(20) | ✅ | `ACTIVE` · `ENDED`. **Chỉ mô tả — xem cảnh báo** |

> **⚠⚠ `Status` KHÔNG được tham gia phép kiểm cover.** Phép kiểm là đúng năm phép so của D2 §3.3.3 và trạng thái thành viên **không nằm trong đó**. Một thành viên `ENDED` với `CoverageTo = 2026-09-30` mà hồ sơ có ngày dịch vụ tháng 8/2026 thì **vẫn được bảo hiểm**. **Ngày tháng mới mang sự thật.**
>
> Consumer nào lọc `Status = ACTIVE` sẽ **từ chối mọi hồ sơ hợp lệ mà người rời đi nộp sau khi họ rời** — tức là phần lớn hồ sơ mà người rời đi nộp. **Phải nói điều này với InsureMO như một yêu cầu trên response, không phải như một giả định về cái store**: nếu `I-05` lọc theo status ở bất kỳ đâu trong hiện thực của nó, mọi hồ sơ muộn của người rời đi biến mất và **không gì trong hệ của ta biết được là nó đã biến mất**.

> **`ContinuousCoverFrom` + `RenewalStatus` là field đã thay đổi kết cục trên hồ sơ bình thường nhất có.** Cụ thể: một thành viên tham gia liên tục 5 năm, hợp đồng vừa tái tục tháng trước, tuần này đi khám ngoại trú. `CoverageFrom` là tháng trước. **Thời gian chờ 30 ngày đo từ `CoverageFrom` sẽ TỪ CHỐI hồ sơ.** Mặc định ngược lại còn tệ hơn và đắt hơn: giả định mọi thành viên đều đang tiếp tục thì **thời gian chờ thai sản 270 ngày không bao giờ áp dụng cho ai cả.**
>
> **⚠ `KHÔNG_XÁC_ĐỊNH` là một câu trả lời thật, không phải sự vắng mặt** — nghĩa là InsureMO có giữ thành viên nhưng không xác lập được tính liên tục.
>
> **⚠ D3 §I-05 KHÔNG mang hai field này.** Một năng lực được xác nhận trong hội thoại mà vắng trong đặc tả giao diện là một việc không ai bắt đầu được. **⚠ cần xác thực bằng gọi thật.**

> **`CardNo` unique giờ là bất biến của người khác.** Nó là khóa tìm kiếm duy nhất tìm ra người phụ thuộc. **Điều duy nhất ta còn làm được là phát hiện trùng lúc đăng ký và TỪ CHỐI ĐOÁN** — đúng là đường xử lý đa-kết-quả đã có. Đường đó viết cho một lỗi dữ liệu ta định ngăn chặn; giờ nó là **kiểm soát chính**.

### 1.8 `PayoutAccount` `[Claim Platform]` — F58

| Attribute | Field D2 | Kiểu ODC | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| `AccountRef` | `account_ref` `text(20)` | Text(20) | ✅ | Khóa chính. Format `ACC-NNNNNN-N`. Wire: `payoutAccountRef` |
| `MemberId` | `member_id` `text(20)` | Text(20) | ✅ | **Một THAM CHIẾU vào danh bộ InsureMO, không phải khóa ngoại.** Một thành viên có thể giữ nhiều tài khoản |
| `BankCode` | `bank_code` `text(10)` | Text(10) | ✅ | `999999` là BIN test dành riêng xuyên suốt D3 |
| `AccountNo` | `account_no` `text(30)` | Text(30) | ✅ | Chữ số. **Mask ở MỌI chỗ hiển thị** trừ panel chi tiết §5.20 |
| `AccountName` | `account_name` `text(140)` | Text(140) | ✅ | **Latin HOA KHÔNG DẤU, như ngân hàng giữ** — `PHAM QUANG DUY`, không phải `Phạm Quang Duy`. **KHÔNG BAO GIỜ được gõ tay** |
| `Verified` | `verified` `bool` | Boolean | ✅ | Xác minh tên với ngân hàng. Wire: `accountNameVerified`. **Lệnh chi trả vào tài khoản chưa xác minh bị từ chối `422 ACCOUNT_NOT_VERIFIED`** |

> **`MemberId` vượt biên hệ thống, và đó là điều mới.** Ở v0.1 đây là khóa ngoại trong một database và toàn vẹn tham chiếu là miễn phí. Giờ nó là tham chiếu từ bản ghi của ta tới bản ghi InsureMO giữ, nên **không gì ngăn một tài khoản chi trả sống lâu hơn thành viên nó nêu tên**. Giảm thiểu: thành viên **được kết thúc hiệu lực, không bao giờ bị xóa**, và tài khoản chỉ được đọc trong ngữ cảnh một hồ sơ đã mang snapshot.

**Xác minh do thẩm định viên hoặc quản trị viên thực hiện, trên console, sau khi hồ sơ được đăng ký. KHÔNG do khách hàng thực hiện.** Đổi tài khoản **kích hoạt lại xác minh** và, nếu đã duyệt quyết toán, cần người phê duyệt **giải phóng lại** khoản chi.

### 1.9 `Beneficiary` `[Claim Platform]` — F58

Giữ **tách biệt với thành viên được bảo hiểm và với người yêu cầu**, vì người thụ hưởng quyết định **tiền đi đâu**, và đối soát việc đó sau khi đã xảy ra là kiểu hỏng đáng ngại nhất.

| Attribute | Field D2 | Kiểu ODC | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| `PolicyNo` | `policy_no` `text(20)` | Text(20) | ✅ | Khóa chính phần 1. Tham chiếu vào danh bộ InsureMO |
| `MemberId` | `member_id` `text(20)` | Text(20) | ✅ | Khóa chính phần 2. **Người được bảo hiểm có cover được dùng, KHÔNG phải người nhận tiền** |
| `PayeeName` | `payee_name` `text(200)` | Text(200) | ✅ | Mặc định lấy `InsuredMember.FullName` |
| `RelationshipToInsured` | `relationship_to_insured` | Text(20) | ✅ | Cùng tập với `InsuredMember.Relationship`. `SELF` trong ca thường gặp |
| `IdNumber` | `id_number` `text(20)` | Text(20) | ✅ | **CỦA NGƯỜI NHẬN TIỀN**, không phải của người được bảo hiểm. **Bắt buộc kể cả khi người nhận là cha/mẹ của trẻ vị thành niên** — đây là danh tính khoản chi được thực hiện đối với |
| `PayoutAccountRef` | `payout_account_ref` | Text(20) | ✅ | **Có thể trỏ tới tài khoản của một thành viên KHÁC** |

**Một dòng mỗi thành viên, tạo tự động, mặc định là chính người được bảo hiểm.** Seed mang 12 thành viên và 12 dòng. **Hai trong số đó là lý do entity này tồn tại**: `MBR-000912` và `MBR-000914` là trẻ vị thành niên có người nhận tiền là `Phạm Quang Duy`, `PARENT`, chi vào tài khoản **của ông** `ACC-000911-1`. **Người được bảo hiểm, người yêu cầu và người nhận tiền không trùng nhau** — và một thiết kế giữ người nhận tiền thành ba cột trên bản ghi thành viên sẽ phải xây lại ngay lần đầu chuyện này xảy ra.

### 1.10 `Claimant` `[Claim Platform]` — F41, F45

Giữ **tách biệt với thành viên được bảo hiểm và với người nhận tiền**, và mang **hai phân loại trả lời hai câu hỏi khác nhau**. **Một dòng mỗi hồ sơ.**

| Attribute | Field D2 | Kiểu ODC | Bắt buộc | Ghi chú |
|---|---|---|---|---|
| `ClaimNo` | `claim_no` `text(20)` | Text(20) | ✅ | **Khóa chính. Một người yêu cầu mỗi hồ sơ** |
| `MemberId` | `member_id` `text(20)` | Text(20) | ✅ | Tham chiếu vào danh bộ InsureMO |
| `Standing` | `standing` `enum(claimantStanding)` | Text(20) | ✅ | **Câu hỏi PHÁP LÝ.** `SELF` · `GUARDIAN` · `HEIR` · `AUTHORISED`. **Bốn giá trị, và mỗi giá trị KÉO THEO yêu cầu chứng từ riêng** — §4.22 |
| `Relationship` | `relationship` `enum(claimantRelationship)` | Text(20) | ⚠ C | **Câu hỏi GIA ĐÌNH.** `FATHER` · `MOTHER` · `SPOUSE` · `CHILD` · `GRANDFATHER` · `GRANDMOTHER` · `SIBLING` · `OTHER`. **Bắt buộc khi `Standing ≠ SELF`; PHẢI RỖNG khi `Standing = SELF`** |
| `FullName` | `full_name` `text(200)` | Text(200) | ✅ | **Mặc định lấy `FullName` của thành viên và BỊ KHÓA vào nó khi `Standing = SELF`** |
| `IdNumber` | `id_number` `text(20)` | Text(20) | ✅ | **CỦA NGƯỜI YÊU CẦU.** Cùng khóa như `FullName`. **Đây là field làm cho phép kiểm xung đột lợi ích tự động CHẠY ĐƯỢC** |
| `ContactEmail` | `contact_email` `text(160)` | Text(160) | ✅ | **Email liên hệ CHO HỒ SƠ NÀY** — khách gõ trên form, không đọc từ bản ghi bên nào. **Mọi thông điệp về hồ sơ này gửi tới đây.** Wire: `contactEmail` |
| `ContactPhone` | `contact_phone` `text(10)` | Text(10) | ✅ | 10 chữ số. Wire: `contactPhone` |
| `ClaimantUserId` | `claimant_user_id` `text(20)` | Text(20) | — | Tài khoản app đã nộp. **`null` với hồ sơ do nhân viên đăng ký tại quầy** |
| `CreatedBy` / `CreatedAt` | | Long Integer / Date Time | ✅ | `SYSTEM` khi app tạo |

**Hai chiều, vì chúng trả lời hai câu hỏi, và câu này không phải câu kia.**

*Standing* trả lời **người này đòi tiền trên tư cách pháp lý nào**. *Relationship* trả lời **người này là gì của bệnh nhân**. Cha đòi cho con vị thành niên là `GUARDIAN` + `FATHER`; góa phụ đòi theo quyền lợi tử vong là `HEIR` + `SPOUSE`; đồng nghiệp mang giấy ủy quyền tới quầy là `AUTHORISED` + `OTHER`. **Gộp hai chiều làm một là mất chiều thứ hai trong MỌI ca mà chiều thứ nhất không phải `SELF`** — và chính chiều thứ hai là thứ một yêu cầu chứng từ khóa vào.

> **⚠ Tại sao `ContactEmail` an toàn với snapshot theo cấu tạo.** Entity này có `ClaimNo` là khóa chính, nên thông tin liên hệ của một hồ sơ được ghi **cùng với** hồ sơ và **không thể bị sửa bởi bất cứ thứ gì sửa một tài khoản**. Khách đổi email trên profile tháng Ba **không chuyển hướng thư từ chối của hồ sơ đã quyết tháng Hai**. Không có gì phải cấm vì **không có nơi thứ hai mà địa chỉ đó sống**.
>
> **⚠ Khách có thể gõ bất kỳ địa chỉ nào, và giảm thiểu là ĐỌC LẠI ĐỂ XÁC NHẬN, không phải một control.** Đây **không phải** rủi ro chuyển hướng thanh toán và **không được đối xử như vậy** — thư từ không phải tiền, và không lá thư nào chuyển một đồng nào. **Nhưng một lá thư từ chối là tài liệu về sức khỏe của ai đó**, và một ký tự sai gửi nó cho người lạ. Vì vậy địa chỉ được **đọc lại để xác nhận trước khi nộp** — **đọc lại cái đã gõ, không phải một ô nhập lần hai. Gõ lại thì sinh ra cùng một lỗi gõ hai lần.**
>
> **Cái KHÔNG được xây, có chủ ý:** một vòng xác minh — gửi mã tới địa chỉ trước khi cho nộp hồ sơ — sẽ **chặn một hồ sơ vì một mail server**, tức là đúng §0.2 câu ②.

> **⚠ Hai tên cho một tập, cần A1 chốt.** D2 §5.4.2 F36 mang cùng phân loại này dưới tên `enum(claimantCapacity)` với các thành viên tiếng Việt `BẢN_THÂN` · `NGƯỜI_GIÁM_HỘ` · `NGƯỜI_THỪA_KẾ` · `NGƯỜI_ĐƯỢC_UỶ_QUYỀN`. **Tập tiếng Anh ở trên là tập tuân thủ** (§2.6 của D2: mã enum là tiếng Anh UPPERCASE_SNAKE, A1 ánh xạ sang tiếng Việt). Cho tới khi A1 cấp phát, **coi `claimantStanding` và `claimantCapacity` là CÙNG MỘT TẬP dưới hai tên, và cặp này KHÔNG tạo ra tập mới.**

---

## 2. Entity tầng claim

> **Mười lăm entity.** Đây là phần Claim Platform xây thật, và là nơi luật §0.2 ① được thực thi: **các attribute tiền chỉ do `RunAdjudicationChain` và `PostLimitLedger` ghi.** Agent không có tool nào chạm tới chúng.

### 2.1 `Claim`

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `ClaimNo` | Text(20) | ✅ | — | Format **`CLM-YY-NNNNNN`** *(D3 §1.2)*. **`YY` là năm ĐĂNG KÝ, không phải năm dịch vụ** — hồ sơ dịch vụ tháng 12 đăng ký tháng 1 mang năm của tháng 1. **Không bao giờ parse để lấy nghĩa.** Là correlation id xuyên ODC ↔ IDP ↔ thư ↔ log |
| `PolicyNo` | Text(20) | ✅ | — | Tham chiếu InsureMO |
| `MemberId` | Text(20) | ✅ | — | Tham chiếu InsureMO |
| `LossType` | Text(20) | ✅ | — | **12 giá trị §4.2.** Là **khóa duy nhất** của checklist A4 §6 |
| `EventDate` | Date | ✅ | — | |
| `TreatmentFrom` | Date | ✅ | — | `TreatmentFrom ≤ TreatmentTo`; `≥ CoverageFrom`; không ở tương lai |
| `TreatmentTo` | Date | ✅ | — | |
| `ProviderName` | Text(200) | ✅ | — | §6.2 "Provider name" |
| `ProviderCode` | Text(20) | — | null | Khớp danh mục cơ sở y tế (F57) |
| `InNetwork` | Boolean | ⚠ C | — | **Field nhạy cảm tiền nhất trong sản phẩm.** Bật/tắt nó đổi số tiền mọi hồ sơ đăng ký từ đó về sau, **20% trên nhóm quyền lợi `200` và `300`**. **Ghim tại lúc đăng ký, không đọc lại** — sửa danh mục về sau **không** chạm hồ sơ đã đăng ký |
| `FacilityCountry` | Text(2) | — | null | ISO-3166 alpha-2, **cấp chứng từ, không phải cấp dòng**. **`VN` KHÔNG phải mặc định: quốc gia không được nêu là `null`.** **Tuyệt đối cấm đoán quốc gia từ TÊN cơ sở** |
| `RequestedAmount` | Currency | — | null | Khách khai |
| `RequestReceivedDate` | Date | ✅ | — | Mốc đồng hồ SLA |
| `State` | Text(20) | ✅ | `DRAFT` | **13 giá trị §4.7.** Chỉ `TransitionClaimState` được ghi |
| `Queue` | Text(20) | — | null | §4.25 |
| `AssignedTo` | Long Integer | — | null | Một người được giao đang hoạt động |
| `RuleSetRef` | Text(200) | ✅ | — | **Snapshot tại ĐĂNG KÝ.** Điều khoản sửa, thêm hay rút sau đó **không làm nhiễu hồ sơ** |
| `EpisodeType` | Text(20) | ✅ | derived | **Chiếu từ `LossType`, CHỈ ĐỌC.** `ILL_*` → `OUTPATIENT`/`INPATIENT` · `DEN_*` → `DENTAL` · `ACC_*` → `ACCIDENT` · `MAT_*` → `MATERNITY`. **Code nào cho phép hai cái lệch nhau là code có lỗi** |
| `IdpRequestGroupId` | Long Integer | — | null | Cầu nối sang §3 |
| `IdpWorkflowRequestId` | Long Integer | — | null | Cầu nối sang §3 |
| `CreatedBy` / `CreatedAt` | Long Integer / Date Time | ✅ | — | |

> **⚠ `InNetwork` là dòng duy nhất trên màn hình đó mang đường lỗi, và lý do đáng đọc:** nó bắt buộc, nó là boolean, **nó không có ai sửa ở hạ nguồn vì quyền quyết định của thẩm định viên đã bị gỡ**, và nó không đọc lại được khi hồ sơ đã đăng ký. **Một rule không có cách báo cho người ta biết là nó đã chạy thì ở đâu cũng là lỗi; ở đây nó là khác biệt giữa một khoản trừ 20% có chủ ý và một khoản trừ 20% do bấm nhầm mà không ai được báo.**
>
> **Ca còn lại không có lời giải sạch:** một cơ sở *vốn* trong mạng lưới nhưng thiếu trong danh mục lúc hồ sơ được đăng ký thì **không sửa được trên hồ sơ đó** — cờ đã ghim, thẩm định viên không có quyền, sửa danh mục không hồi tố. **Lối ra duy nhất là hủy hồ sơ và nộp lại**, kéo theo số hồ sơ mới và ngày đăng ký mới, và chỉ làm được trước `PENDING_APPROVAL`. **⚠ cần chốt với PO.**

### 2.2 `InsuredEvent`

**Một hồ sơ có thể mang nhiều sự kiện bảo hiểm.** Định danh `CLM-YY-NNNNNN/n` *(D3 §1.2)*.

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `EventNo` | Text(20) | ✅ | — | `CLM-26-000417/1`. **Không parse để lấy nghĩa.** Wire: `eventNo` |
| `ClaimId` | Long Integer | ✅ | — | |
| `EventDate` | Date | ✅ | — | |
| `AdmissionDate` | Date | — | null | |
| `DischargeDate` | Date | — | null | |
| `TreatmentDays` | Integer | — | null | Suy ra từ `TreatmentFrom`/`TreatmentTo`, **đếm bao gồm cả hai đầu**. **⚠ `TreatmentTo` thiếu thì đây phải là `null`, không được mặc định thành 1 ngày** — xem §8.2 |
| `DiagnosisText` | Text(500) | ✅ | — | Chẩn đoán của hồ sơ |
| `Icd10` | Text(20) | ✅ | — | §6.2 "Any code" |
| `ConditionClass` | Text(20) | ✅ | — | **6 giá trị §4.4.** Khóa tra thời gian chờ |
| `EventDecision` | Text(20) | — | null | §4.9 |
| `DeclineReason` | Text(20) | ⚠ C | null | **Chỉ mã CẤP SỰ KIỆN: `LIMIT` · `PERIOD` · `NOBEN` · `WAIT` · `EXCL`. `NOTREL` và `NOTCOV` KHÔNG được chào ở đây** — chúng chỉ tồn tại ở cấp dòng |

### 2.3 `ClaimDocument`

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `DocumentId` | Text(20) | ✅ | — | `DOC-NNNN`. Wire: `documentId` |
| `ClaimId` | Long Integer | ✅ | — | |
| `DocumentType` | Text(20) | — | null | **`T1-01` … `T2-04` hoặc `unclassified`** — §4.1 |
| `PartnerClass` | Text(200) | — | null | **Giá trị phân loại THÔ của IDP, giữ nguyên bên cạnh loại của ta.** Khi một ánh xạ về sau bị phát hiện sai, các chứng từ đã xử lý **ánh xạ lại được từ giá trị đã lưu**; không có nó thì phải phân loại lại, tức là xử lý lại |
| `CompetingType` | Text(20) | — | null | Loại thứ hai khi hai signature cùng đạt. **Hiển thị, KHÔNG BAO GIỜ tự giải quyết** |
| `ClassificationConfidence` | Decimal | — | null | `[0,1]` |
| `IdpTemplateId` | Long Integer | — | null | Template thật đã khớp — `5384`/`5127`/`5383`/`5299`/`5340`/`5312` |
| `FileHash` | Text(64) | ✅ | — | **sha256 bắt buộc.** IDP kiểm lại sau khi tải; lệch thì **fail request chứ không trích xuất từ một file không mong đợi**. Cũng là khóa của `IdpReplay` |
| `Uri` | Text(500) | ✅ | — | **Chứng từ nộp BẰNG THAM CHIẾU, không bằng giá trị.** Gửi bytes hai lần là nhân đôi lưu trữ và tạo ra bản thứ hai phải bảo vệ |
| `SizeBytes` | Long Integer | ✅ | `0` | |
| `MimeType` | Text(100) | ✅ | — | `application/xml` + `DeclaredType = T1-04` → **đường XML, không phải OCR** |
| `PageCount` | Integer | ✅ | `0` | |
| `DeclaredType` | Text(20) | — | null | **Lựa chọn của KHÁCH lúc chụp — CHỈ LÀ GỢI Ý.** Nếu IDP phân loại khác, **IDP thắng và sinh cảnh báo. Không bao giờ để cú chạm của khách âm thầm quyết định loại chứng từ** |
| `IdpRequestId` | Long Integer | — | null | |
| `RawExtractionJson` | Text | — | null | **Append-only, ghi một lần, KHÔNG BAO GIỜ update.** Kết quả cho một `requestId` **không bao giờ đổi** — một hiệu chỉnh tạo request MỚI, không mutate cái này. **Chính điều đó làm cho so sánh thô-với-đã-sửa khả thi** |
| `Confidence` | Decimal | — | null | `[0,1]`. **Cấp hồ sơ là MIN trên tập field bắt buộc — không phải trung bình** |
| `ChecklistState` | Text(30) | ✅ | `CHƯA_NHẬN` | §4.16 |
| `FileProcessingState` | Text(20) | — | null | §4.18. **Chỉ hiển thị, không thao tác** |
| `CopyOrOriginal` | Text(20) | — | null | §4.19. **Trong model, ẩn bằng cấu hình trong pilot** |
| `Source` | Text(30) | ✅ | — | §4.20 `KHÁCH_HÀNG` · `CÁN_BỘ_THẨM_ĐỊNH` |
| `ReceivedAt` | Date Time | — | null | Ngày **TA** nhận |
| `CustomerSentAt` | Date | — | null | Ngày **KHÁCH** gửi. **Hai đồng hồ, hai mốc neo — giữ cả hai, hiển thị cả hai, KHÔNG ghi đè** |
| `RequestedAt` | Date Time | — | null | Đặt bởi hành động *Yêu cầu bổ sung* |
| `WaiveReason` | Text(2000) | ⚠ C | null | **Bắt buộc khi miễn yêu cầu.** Audit cũ → mới |
| `RawImageBinary` | Binary Data | — | null | |

### 2.4 `ClaimLine`

**Cột `Prov` của A4 §4.1 nói giá trị đến từ đâu**, và nó tồn tại vì sáu cột trong số này giờ **được hiển thị trên lưới của thẩm định viên**. **Một cột người ta đọc thì phải có câu trả lời cho *ai đã đặt cái này vào đây*.**

| Prov | Nghĩa |
|---|---|
| **X** | Chờ từ trích xuất. IDP trả về; thẩm định viên sửa được và **giá trị thô được giữ lại** |
| **X?** | Chờ từ trích xuất — **và IDP CHƯA XÁC NHẬN**. Console phải ghi *"chưa xác nhận nguồn trích xuất"* ngay tại field |
| **K** | Thẩm định viên gõ. **Không có mục tiêu trích xuất và cũng không đòi hỏi một cái nào** |
| **D** | IDP suy ra từ các cột khác |

| Attribute | A4 | Kiểu ODC | Bắt buộc | Prov | Ghi chú |
|---|---|---|---|---|---|
| `ClaimId` | | Long Integer | ✅ | — | |
| `EventNo` | L17 | Text(20) | ✅ | D | Sự kiện dòng này phát sinh. **Phân bổ nhầm hoặc không phân bổ được thì GẮN CỜ, KHÔNG BAO GIỜ ĐOÁN.** Hiển thị là **cột đầu tiên** |
| `SourceDocumentId` | L19 | Text(20) | ✅ | D | Đọc từ tờ nào — mã A4 + `documentId`. Chỉ đọc |
| `LineNo` | L1 | Integer | — | X | |
| `ItemName` | L2 | Text(200) | ✅ | X | **Văn bản được trích dẫn nguyên văn trong diễn giải từ chối** |
| `ServiceGroup` | L3 | Text(100) | — | X | Khi cơ sở y tế có in |
| `UnitOfMeasure` | L4 | Text(20) | — | **X?** | *viên*, *lọ*, *lần*, *ngày*. **KHÔNG chuẩn hóa về danh sách có kiểm soát** — một chuẩn hóa chưa ai kiểm chứng còn tệ hơn chuỗi đã in. **Trích từ v1.0 nhưng CHƯA CÓ GÌ ĐỌC cho tới v1.2 ⇒ độ chính xác của nó CHƯA BAO GIỜ được đo** |
| `Quantity` | L5 | Decimal | ✅ | X | **> 0** |
| `UnitPrice` | L6 | Currency | ✅ | **X?** | **≥ 0** |
| `LineAmount` | L7 | Currency | ✅ | X | Đối chiếu `L5 × L6` với dung sai **±1 VND** → cảnh báo khi lệch |
| `PaidBySocialInsurance` | L8 | Currency | — | X | Mặc định `0`. **Trích xuất trong MVP 1 nhưng KHÔNG được engine tiêu thụ.** Cứ trích: bật phối hợp chi trả về sau **không được phép đòi xử lý lại bộ ground-truth** |
| `PatientCoPaymentBhyt` | L9 | Currency | — | X | Mặc định `0` |
| `PatientSelfPaid` | L10 | Currency | — | X | Mặc định `0` |
| `OtherPayerAmount` | L11 | Currency | — | **X?** | **Phần do một bên chi trả KHÔNG PHẢI BHYT và KHÔNG PHẢI bệnh nhân** — bảo hiểm tư thứ hai, chương trình của chủ lao động, quỹ từ thiện. **KHÔNG BỊ KHẤU TRỪ** và MVP 1 không có quy tắc đóng góp, **nhưng nó ĐƯỢC HIỂN THỊ** — một dòng mà bên thứ ba đã trả là tín hiệu rõ nhất về rủi ro bảo hiểm trùng |
| `DrugRegistrationNo` | L12 | Text(20) | — | **X?** | Có trên dòng thuốc. Là một trong ba tín hiệu suy ra `serviceCategory = DRUG` |
| `ServiceCategory` | L13 | Text(20) | ✅ | **D** | **20 giá trị §4.5.** Giá trị mà ánh xạ quyền lợi tiêu thụ. **Đổi nó là RE-MAP quyền lợi và LÀM MẤT HIỆU LỰC phép tính** |
| `AmountClaimed` | L14 | Currency | ✅ | **D** | **`L7 − L8`, sàn ở 0.** Số tiền trình cho engine. **Có sẵn trên mọi dòng để engine không phải tính lại** |
| `DocumentDate` | L15 | Date | — | **X?** | Ngày in **ngay trên dòng này**, khi bảng kê in mỗi dòng một ngày. **Khác header field 5.** Là thứ phân bổ dòng vào đúng sự kiện. **Không in thì `null` và phân bổ lùi về ngày header — nó KHÔNG mặc định thành ngày header, vì một giá trị được suy ra không được trông giống một giá trị được đọc** |
| `Icd10` | L16 | Text(20) | — | **X?** | Mã chẩn đoán in **trên dòng này**. **Khác chẩn đoán của sự kiện** |
| `TreatmentDaysLine` | L18 | Integer | — | **X?** | Số ngày **dòng chi phí này** phủ, khi dòng tính theo ngày — tiền giường, tiền phòng, ngày ICU. **`null` khi dòng không tính theo ngày, KHÔNG BAO GIỜ suy từ `L5`** — `L5` đếm đơn vị của một thứ không xác định |
| `LineDecision` | | Text(20) | ⚠ C | K | §4.8. **Mọi dòng phải mang một quyết định TRƯỚC KHI trình duyệt** |
| `DeclineReasonCode` | | Text(20) | ⚠ C | K | §4.10. Bắt buộc khi `TỪ_CHỐI`. Cấp dòng: `LIMIT`, `NOBEN`, `WAIT`, `EXCL`, **`NOTREL`**, **`NOTCOV`** |
| `DeclineRationale` | | Text(2000) | ⚠ C | K | AI soạn, **sửa được**. **Phải nêu TÊN KHOẢN và SỐ TIỀN** |
| `DeclineRationaleOriginal` | | Text(2000) | — | — | **Bản gốc hệ thống soạn, giữ song song** |
| `BenefitCode` | | Text(20) | — | D | `UNMAPPED` → chuyển thẩm định viên. **Agent không được đoán** |
| `Confidence` | L12(A4 §7) | Decimal | — | D | **Row confidence = MIN các ô của nó** |
| `Verified` | | Boolean | ✅ | K | Bắt buộc `true` trước khi trình nếu dưới sàn |
| `OriginalValueJson` | | Text | — | — | **Giá trị gốc KHÔNG BAO GIỜ bị ghi đè** |
| `EditedValueJson` | | Text | — | — | |
| `CoveredAmount` | | Currency | ✅ | — | **Chỉ chain ghi** |
| `PayableAmount` | | Currency | ✅ | — | **Chỉ chain ghi** |
| `CustomerBorneAmount` | | Currency | ✅ | — | **Chỉ chain ghi** |

> **Tại sao `L15` và `L16` giữ số ngoài thứ tự, có chủ ý.** `L13` và `L14` là **suy ra** và đã được trích dẫn theo số trong D2, A5 và payload kết quả từ v1.0. **Một số field là một ĐỊA CHỈ, không phải một vị trí** — đánh số lại cho bảng đọc gọn sẽ âm thầm trỏ lại mọi trích dẫn đang có.
>
> **`L17` cố ý KHÔNG dùng lại cho "số ngày điều trị"** — `L17` đã là phân bổ sự kiện bảo hiểm, và một `L17` thứ hai mang nghĩa khác sẽ làm token này nhập nhằng trên toàn bộ tài liệu. Vì thế số ngày theo dòng là **`L18`**.

> **Tại sao ta DỪNG ở một cột ICD chứ không hai.** Hệ tham chiếu mang **hai** cột mã bệnh trên mỗi dòng: mã trích từ dòng, và một *chẩn đoán liên quan* do thẩm định viên đặt. **Ta lấy cái thứ nhất, không lấy cái thứ hai.** Mã trích xuất có một chứng từ đứng sau nó. Chẩn đoán liên quan **không có gì đứng sau ngoài một cú gõ phím trên mọi dòng của mọi hồ sơ, và sẽ KHÔNG ĐƯỢC GÌ ĐỌC.** Một cú gõ bắt buộc không nuôi quyết định nào là cách một cái lưới trở thành thứ thẩm định viên bấm bỏ qua.

**Validation cấp chứng từ** *(verbatim A4 §4.1)*: `V-01-1` Σ L7 = footer `Tổng cộng` (±1 VND) → **cảnh báo, neo ở footer** · `V-01-2` Σ(L8+L9+L10+L11) = L7 mỗi dòng → **cảnh báo trên dòng** · `V-01-3` có ít nhất một dòng → **LỖI, chứng từ không dùng được** · `V-01-4` ngày điều trị nằm trong kỳ bảo hiểm của thành viên → cảnh báo, engine quyết.

### 2.5 `ClaimLineBreakdown`

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `ClaimLineId` | Long Integer | ✅ | — | |
| `ChainRunId` | Long Integer | ✅ | — | Thuộc về một lần chạy chain cụ thể |
| `CauseCode` | Text(20) | ✅ | — | §4.23 — 8 mã |
| `Amount` | Currency | ✅ | `0` | |

**Ràng buộc:** `Σ Amount` theo `ClaimLineId` của một `ChainRunId` **phải bằng** `ClaimLine.CustomerBorneAmount`. **Lệch là bug chain, không phải cảnh báo** — chặn `SubmitForApproval`.

### 2.6 `ChainRun`

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `ClaimId` | Long Integer | ✅ | — | |
| `RunNo` | Integer | ✅ | `1` | |
| `InputsHash` | Text(64) | ✅ | — | sha256 toàn bộ input. **Cùng hash ⇒ phải cùng output.** Console dùng nó để biết số tiền đang hiển thị đã cũ chưa |
| `Version` | Text(20) | ✅ | — | Phiên bản rule chain |
| `StepOutputsJson` | Text | ✅ | — | Một phần tử mỗi bước: `{stepCode, input, output, explainVi, explainEn}` |
| `TotalRequested` / `TotalCovered` / `TotalPayable` / `TotalCustomerBorne` | Currency | ✅ | `0` | |
| `GateResultJson` | Text | ✅ | — | Kết quả từng điều kiện cổng + lý do |
| `ThresholdSnapshotJson` | Text | ✅ | — | **Ghim ngưỡng tại lúc chạy.** Không ghim thì chạy lại có thể chấm theo ngưỡng chưa tồn tại lúc hồ sơ được đăng ký |
| `TermRegisterVersion` | Text(64) | ✅ | — | Phiên bản sổ điều khoản đã dùng |
| `RunAt` | Date Time | ✅ | now | |
| `TriggeredBy` | Text(20) | ✅ | — | `SYSTEM` · `AGENT` · `OFFICER` |

### 2.7 `LimitLedger`

**Ghi hai pha: TẠM TÍNH khi chạy chain, CHỐT khi PHÊ DUYỆT — không phải khi chi trả.**

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `PolicyNo` / `MemberId` | Text(20) | ✅ | — | |
| `BenefitCode` | Text(20) | ✅ | — | |
| `PeriodKey` | Text(20) | ✅ | — | Năm hợp đồng. **Không bao giờ cộng dồn giữa các kỳ** |
| `ClaimId` / `ChainRunId` | Long Integer | ✅ | — | |
| `CounterType` | Text(20) | ✅ | `MONEY` | `MONEY` · `DAY` · `VISIT`. **⚠ `DAY` chỉ duy trì được khi `ClaimLine.TreatmentDaysLine` có giá trị** — §8.2 |
| `Before` / `Consumed` / `After` | Currency | ✅ | `0` | **Ràng buộc `After = Before − Consumed`** |
| `Phase` | Text(20) | ✅ | `PROVISIONAL` | `PROVISIONAL` · `FINAL` |
| `PostedAt` | Date Time | ✅ | now | |
| `Reversed` | Boolean | ✅ | `false` | **Chi trả hỏng sinh BÚT TOÁN ĐẢO, không xóa dòng cũ, và sổ cái KHÔNG bị bỏ chốt** |
| `ReversalOfId` | Long Integer | — | null | |

> **Tại sao chốt tại phê duyệt chứ không tại chi trả.** Một hồ sơ đã duyệt **đã cam kết hạn mức**. Một hồ sơ thứ hai đang thẩm định cùng lúc phải thấy **cam kết đó**, không phải một con số tạm có thể bốc hơi — nếu không **hai hồ sơ hợp lệ được duyệt sẽ cùng nhau vượt hạn mức năm**.
>
> **⚠ Khi đụng phiên bản** — hai hồ sơ cùng một thành viên thẩm định cùng lúc — **phép tính được CHẠY LẠI trên bộ đếm hiện hành** và thẩm định viên được cho biết con số đã dịch và vì sao. **Không bao giờ âm thầm hợp nhất.** Nửa dễ hiện thực sai: lần thử lại phải **chạy lại phép thẩm định**, không phải ghi lại con số lượt đầu đã tính — con số ấy là hàm của cái bộ đếm mà lệnh ghi đã va vào, nên **phát lại nó là vi phạm hạn mức bằng một race**.

### 2.8 `ClinicalAssessment`

Ánh xạ 1–1 với một lần gọi `I-15` / `I-16`.

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `AssessmentId` | Text(64) | ✅ | — | Wire: `assessmentId` |
| `ClaimId` / `EventNo` | Long Integer / Text(20) | ✅ | — | |
| `State` | Text(20) | ✅ | `QUEUED` | `QUEUED` · `COMPLETE` · `PARTIAL` · `ERROR` |
| `KnowledgeBaseVersion` | Text(64) | ✅ | — | **Do hồ sơ gửi đi, echo lại trong kết quả, lưu trên hồ sơ.** Hồ sơ thẩm định hôm nay phải tái lập được khi tri thức đã dịch chuyển. **⚠ Ai đúc phiên bản và theo lược đồ nào vẫn còn mở** |
| `LinesAssessedJson` | Text | — | null | **Dòng vắng khỏi danh sách này là dòng KHÔNG ĐƯỢC ĐÁNH GIÁ — khác với sạch** |
| `RawResponseJson` | Text | — | null | |
| `StartedAt` / `FinishedAt` | Date Time | ✅ / — | now / null | |

> **⚠ `PARTIAL` là một KẾT QUẢ, không phải một lỗi** — hồ sơ đi tiếp với `LinesAssessedJson` nêu tên khoảng trống.
>
> **⚠ Khi hỏng dứt điểm, hồ sơ đi tiếp trên Track 1 một mình và một cảnh báo console được nêu: MỘT PHÉP ĐÁNH GIÁ KHÔNG CHẠY KHÔNG BAO GIỜ ĐƯỢC GHI NHẬN LÀ MỘT PHÉP ĐÁNH GIÁ SẠCH.** Điều kiện cổng tương ứng do đó chuyển sang **bất hoạt** chứ không phải **mở**.

### 2.9 `ClinicalRecommendation`

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `ClaimLineId` | Long Integer | ✅ | — | Một dòng, một đề xuất |
| `AssessmentId` | Text(64) | ✅ | — | |
| `ProposedReasonCode` | Text(20) | ✅ | — | **CHỈ `NOTREL` và `NOTCOV`** — §4.12. **Mọi mã khác bị TỪ CHỐI TẠI ADAPTER và không bao giờ tới được panel này** |
| `Confidence` | Decimal | ✅ | — | `[0,1]` |
| `ConfidenceBand` | Text(20) | ✅ | — | `HIGH` · `MEDIUM` · `LOW` |
| `RationaleVi` | Text(2000) | ✅ | — | **Tiếng Việt, khách đọc được** — là cơ sở của thứ khách rốt cuộc được cho biết |
| `EvidenceJson` | Text | ✅ | — | **Bằng chứng rỗng là một response DỊ DẠNG, không phải một response độ tin cậy thấp.** Nguồn: `KB_ENTRY` · `DIAGNOSIS` |
| `Status` | Text(20) | ✅ | `Proposed` | §4.13 |
| `DecidedBy` | Long Integer | — | null | |
| `DecisionReason` | Text(2000) | ⚠ C | null | **Bắt buộc khi `Status ≠ Proposed`** |
| `DecidedAt` | Date Time | — | null | |

> **⚠ Không có chữ ký chuyên môn y khoa đứng sau tri thức nền, và không có kế hoạch nào cho việc đó.** Ba kiểm soát gánh rủi ro này và **không cái nào là trang trí**: **bằng chứng trích dẫn trên mọi đề xuất**, **một con người xác nhận từng cái**, và **tỷ lệ đồng thuận được đo từ ngày đầu**. Nói thẳng giới hạn này khi pitch.
>
> **⚠ Giao diện này KHÔNG được phép trở thành cái gì.** Nếu các đề xuất rút gọn thành một phép tra `itemName × icd10` thì **năng lực đó thuộc về Track 1 dưới dạng một bảng loại trừ tất định**, nơi nó kiểm toán và tái lập được. **Một mock LÀ một cái bảng thì đúng; một dịch vụ production là một cái bảng thì đó là một tính năng Track 1 đeo phù hiệu Track 2.**

> **Không có ảnh hưởng của tiền.** `lineAmount` được gửi đi để một diễn giải nêu được số tiền. **Phép đánh giá KHÔNG ĐƯỢC biến thiên theo nó** — một đề xuất phụ thuộc vào việc một dòng tốn bao nhiêu **không phải là một phán đoán lâm sàng**.
>
> **Không có danh tính bệnh nhân trên payload đi ra.** **Dải tuổi và giới tính, hết.** Không tên, không ngày sinh, không số định danh, không mã thành viên.

### 2.10 `FraudSignal`

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `ClaimId` | Long Integer | ✅ | — | |
| `Type` | Text(30) | ✅ | — | §4.24 |
| `Severity` | Text(20) | ✅ | — | `Low` · `Medium` · `High` |
| `EvidenceJson` | Text | ✅ | — | |
| `Status` | Text(20) | ✅ | `Open` | `Open` · `Cleared` |
| `ClearedBy` | Long Integer | — | null | |
| `ClearReason` | Text(2000) | ⚠ C | null | **Bắt buộc khi `Cleared`** |
| `ExplanationVi` | Text(2000) | ✅ | — | Một câu tiếng Việt cho thẩm định viên |

> **⚠ Phát hiện chứng từ giả bằng pixel (font / cắt ghép / con dấu) KHÔNG nằm trong phạm vi Claim MVP và cũng không có trong IDP.** `FraudSignal` ở build này **chỉ phủ ngày bất khả thi, cơ sở y tế không khớp, hồ sơ trùng và khoảng điều trị chồng lấn**. **Nói rõ giới hạn này khi pitch** — đừng để nó bị hiểu là đã có.

### 2.11 `CrossCheckResult`

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `ClaimId` | Long Integer | ✅ | — | |
| `RuleCode` | Text(20) | ✅ | — | **`R1` … `R9`** — §4.26 |
| `Result` | Text(20) | ✅ | — | `PASS` · `WARNING` · `FAIL` |
| `DocumentId` | Text(20) | — | null | **Mọi cảnh báo NÊU TÊN chứng từ và field nó gắn vào, để UI neo được** |
| `FieldName` | Text(100) | — | null | |
| `Detail` | Text(2000) | — | null | |

> **Không gì được tự sửa.** Mỗi rule sinh ra một **kết quả có nhãn neo tại field**. Và: **khi phán quyết của IDP và chứng từ mâu thuẫn, phán quyết của IDP được dùng và một cảnh báo được nêu.**

### 2.12 `ClaimEvent`

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `ClaimId` | Long Integer | ✅ | — | |
| `Type` | Text(30) | ✅ | — | |
| `Actor` | Text(200) | ✅ | — | Người hoặc agent |
| `Detail` | Text(2000) | — | null | |
| `CustomerVisible` | Boolean | ✅ | `false` | Lọc dòng thời gian cho khách |
| `At` | Date Time | ✅ | now | |

### 2.13 `AgentRun`

**Vết audit của từng lượt agent / từng lần gọi tool. Đây là thứ ban giám khảo sẽ xem.** Append-only: không có action update, không có action delete.

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `ClaimId` | Long Integer | ✅ | — | |
| `Agent` | Text(20) | ✅ | — | §4.27 |
| `Step` | Text(200) | ✅ | — | |
| `ToolCalled` | Text(200) | — | null | |
| `InputSummary` / `OutputSummary` | Text(2000) | — | null | |
| `HandoffTo` | Text(20) | — | null | **Cột làm nên câu chuyện agent-to-agent** |
| `KnowledgeVersion` | Text(64) | — | null | |
| `ModelId` | Text(200) | — | null | |
| `DurationMs` | Long Integer | ✅ | `0` | |
| `Replay` | Boolean | ✅ | `false` | |
| `CorrelationId` | Text(64) | ✅ | — | **UUIDv4.** Hành trình đầy đủ của một hồ sơ **xuyên ba hệ thống** phải tái lập được từ một giá trị này |
| `At` | Date Time | ✅ | now | |

### 2.14 `ClaimLetter`

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `ClaimId` | Long Integer | ✅ | — | |
| `Type` | Text(30) | ✅ | — | `RESULT` · `DECLINE` · `INFO_REQUEST` |
| `DraftText` | Text | ✅ | — | |
| `SentText` | Text | — | null | **Giữ cả hai bản** |
| `DraftedBy` | Text(20) | ✅ | — | Mã agent |
| `SentBy` | Long Integer | — | null | |
| `SentAt` | Date Time | — | null | `null` = vẫn là bản nháp |
| `SentToEmail` | Text(160) | — | null | **Lấy từ `Claimant.ContactEmail` của hồ sơ, KHÔNG từ profile tài khoản** |

> **⚠ Màn hình ĐỊNH VỊ câu, nó KHÔNG BAO GIỜ soạn câu.** Ứng dụng không bao giờ ghép `"{benefitName}"` + `" không được chi trả"`. `NOTCOV` là **một** template — *"{itemName}: {itemAmount} — không thuộc danh mục chi phí y tế được chi trả."* — và nó được render **nguyên khối hoặc không render**.
>
> **⚠ Văn bản dòng bị từ chối là DỮ LIỆU HỒ SƠ, không phải template ứng dụng điền.** Diễn giải `NOTREL` / `NOTCOV` do AI soạn rồi **thẩm định viên sửa trước khi gửi**, và **cả hai bản được giữ**. Thứ tới màn hình là văn bản **cuối cùng**. Ứng dụng **render nguyên văn và KHÔNG hậu xử lý gì** — không cắt, không ba chấm, không xuống dòng lại làm đổi câu, không nối thêm tên quyền lợi.
>
> **⚠ Không bao giờ dùng "kê sai" hay "gian lận".** Một quyết định từ chối lâm sàng là **phát biểu về PHẠM VI BẢO HIỂM**, không phải khẳng định rằng một đơn thuốc đã sai. **Đường leo thang là một yêu cầu thông tin** — mẫu `NOTREL` đã mang sẵn lời mời *"Nếu Quý khách có thông tin y tế bổ sung, vui lòng gửi để chúng tôi xem xét lại."* và **cơ chế giữ lời hứa đó phải tồn tại**, không phải một trạng thái mới.

### 2.15 `DocumentChecklistRule`

**Thay thế cho ba entity MBAL đã bỏ.** Cấu hình checklist theo `lossType`, seed từ bảng 8+1 dòng của D2 §4.6 / A4 §6.

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `LossType` | Text(20) | ⚠ C | null | **Khóa 1.** `null` khi dòng này do khóa 2 hoặc khóa 3 sinh ra |
| `ClaimantStanding` | Text(20) | ⚠ C | null | **Khóa 2.** Khi `≠ SELF` thì thêm **một** dòng chứng từ tư cách |
| `TermRef` | Text(64) | ⚠ C | null | **Khóa 3 — `REQUIRE_DOC` của một điều khoản.** **⚠ A4 §6 chưa có khóa này; cần A4 chốt.** Cho tới khi có, điều khoản nêu yêu cầu **dưới dạng văn bản** và checklist **không mang dòng cho nó**, và **checkpoint mới là thứ giữ hồ sơ** |
| `DocumentType` | Text(20) | ✅ | — | `T1-01` … `T2-04` |
| `RequirementLevel` | Text(20) | ✅ | `OPTIONAL` | §4.15 |
| `IsActive` | Boolean | ✅ | `true` | |

**Seed — bảng 8+1 dòng** *(verbatim D2 §4.6)*:

| `LossType` | **Bắt buộc** `REQUIRED` | **Nên có** `EXPECTED` | **Tùy chọn** `OPTIONAL` |
|---|---|---|---|
| `ILL_OP` — Bệnh, ngoại trú | `T1-01`, `T1-03` | `T1-04`, **`T1-02`** | `T2-01`, `T2-02`, `T2-03` |
| `ILL_IP` — Bệnh, nội trú | `T1-01` | `T1-04`, `T2-03` | `T1-02`, `T2-01`, `T2-02` |
| `DEN_OP` — Nha khoa, ngoại trú | `T1-01`, `T1-03` | `T1-04` | `T2-01` |
| `DEN_IP` — Nha khoa, nội trú | `T1-01` | `T1-04`, `T2-03` | `T2-01`, `T2-02` |
| `ACC_OP` — Tai nạn, ngoại trú | `T1-01`, `T1-03` | `T1-04`, `T2-02` | `T1-02`, `T2-01` |
| `ACC_IP` — Tai nạn, nội trú | `T1-01` | `T1-04`, `T2-02`, `T2-03` | `T1-02`, `T2-01` |
| `MAT_OP` — Thai sản, ngoại trú | `T1-01`, `T1-03` | `T1-04` | `T1-02`, `T2-01`, `T2-03` |
| `MAT_IP` — Thai sản, nội trú | `T1-01` | `T1-04`, `T2-03` | `T1-02`, `T2-01`, `T2-02` |
| **Bất kỳ, khi `ClaimantStanding ≠ SELF`** | **chứng từ tư cách §4.22** | — | — |

**Hai sự kiện cấu trúc mà bảng 5 giá trị không thể hiện được:**
1. **Nha khoa và thai sản CÓ dạng nội trú**, và checklist của chúng khác dạng ngoại trú **đúng theo cách mọi bệnh lý khác khác** — `T1-03` phiếu khám rơi ra và `T2-03` báo cáo y tế đi vào, vì **bệnh nhân nội trú ra viện với một báo cáo chứ không phải một phiếu khám**.
2. ***Ngoại trú có thuốc* KHÔNG PHẢI một loại tổn thất.** Nó là một dòng trong bảng cũ đặt tên cho một tổ hợp bệnh-lý-và-điều-trị **mà khách không thể biết lúc chụp**. **Cái khóa cũ đó vòng tròn:** cái quyết định checklist nào áp dụng là *đã đính đơn thuốc chưa*, trong khi **chính checklist là thứ bảo khách đi đính một cái**. Một thẩm định viên mở hồ sơ chưa có đơn thuốc, và cùng thẩm định viên đó mở đúng hồ sơ ấy mười phút sau khi đơn thuốc tới, **sinh ra hai checklist khác nhau từ một sự kiện không đổi**.

> **⚠ 12 giá trị `lossType`, 8 dòng checklist, và khoảng trống được NÊU TÊN chứ không lấp.** **Bốn giá trị tử vong và thương tật vĩnh viễn KHÔNG có checklist và tài liệu này KHÔNG bịa ra một cái.** Hồ sơ đăng ký trên một trong bốn giá trị đó render dòng tư cách, bất kỳ chứng từ nào cán bộ đính kèm, và **một lời tuyên bố vắng mặt** — *"chưa có danh mục chứng từ cho loại tổn thất này"* — chứ **không phải một checklist rỗng đọc như là KHÔNG CẦN GÌ CẢ**. **⚠ cần A4 chốt.**

> **Ba mức chứ không hai, vì mức giữa là chỗ chứa phán đoán.** `T1-02` đơn thuốc là *Nên có* cho ngoại trú-bệnh **chính là để** một lượt khám hợp lệ mà không sinh ra đơn thuốc **không bị treo**, trong khi nhánh lâm sàng vẫn được hỏi xin tài liệu nó cần. `REQUIRED` sẽ treo nó; `OPTIONAL` sẽ không bao giờ hỏi.

### 2.16 `PaymentOrder`

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `InstructionRef` | Text(64) | ✅ | — | Wire: `instructionRef`. **Khóa idempotent** |
| `ClaimId` | Long Integer | ✅ | — | |
| `Amount` | Currency | ✅ | `0` | **= `ChainRun.TotalPayable` của lần chạy HIỆN HÀNH** (`InputsHash` khớp). Lệch → chặn |
| `PayoutAccountRef` | Text(20) | ✅ | — | **Tài khoản phải `Verified = true`, nếu không thì `422 ACCOUNT_NOT_VERIFIED`** |
| `AccountMasked` | Text(30) | ✅ | — | |
| `Status` | Text(20) | ✅ | `ISSUED` | `ISSUED` · `IN_PROGRESS` · `CONFIRMED` · `REJECTED` · `RETURNED` |
| `IssuedAt` | Date Time | ✅ | now | |

> **⚠ Một lệnh chi trả trùng là kiểu hỏng TỆ NHẤT hệ này có thể sinh ra.** Mọi `POST` tạo hay kích hoạt đều mang `Idempotency-Key` (UUIDv4 do client sinh). Bên nhận **lưu khóa cùng response trong 24 giờ** và trả lại response đã lưu, **không đổi**, khi gặp lại. **Cùng khóa, khác body → `409 CONFLICT`. Đây là bug của client và nó phải TO TIẾNG.**

---

## 3. Entity tầng tích hợp IDP `[IDP]`

> **Bảy entity** trong module `CLM_IDP_Int`. Chúng tồn tại vì **IDP staging của Claim MVP là hệ IDP thật, tenant 176**, và hợp đồng của D3 (`I-07`/`I-08`/`I-09`) là hợp đồng **mục tiêu**, còn API đang chạy trên staging là API **hiện có**. Đội phải sống với khoảng cách giữa hai thứ đó, và đó là việc của tầng này.

**Ba thứ áp dụng cho mọi lời gọi** *(verbatim IDP API spec)*:
```
Authorization: Bearer <access_token>
User-Agent: Mozilla/5.0 (…) Chrome/126.0.0.0 Safari/537.36
Accept: application/json, text/plain, */*
```
> **⚠ `User-Agent` là BẮT BUỘC.** Cloudflare chặn user-agent mặc định của thư viện HTTP với **`403 · error code: 1010`** — không phải lỗi ứng dụng và **không có trong Swagger**. Đây là thứ làm mất nửa ngày nếu không biết trước.

### 3.1 `IdpSession`

**Mục đích.** Cache token JWT. **Không có refresh token trong response** — 401 thì login lại.

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `AccessToken` | Text | ✅ | — | Từ `POST /auth/login` → `{access_token}`, **~340 ký tự**. Lưu trong ODC Secret, **không log** |
| `ObtainedAt` | Date Time | ✅ | — | Re-login khi quá 30 phút hoặc khi gặp 401 |
| `TenantId` | Long Integer | ✅ | — | **`176`** — tenant của Claim MVP *(verbatim `GET /user/me`)* |
| `UserId` | Long Integer | ✅ | — | `203` trên staging |
| `RoleId` | Integer | ✅ | — | **Phải `= 2` (`Tenant` / End User).** `1` = `Partner` (Admin) |

> **⚠⚠ Đây là thứ chặn tích hợp nếu sai, và nó đã chặn thật một lần.** `GET /workflow/request/search` lọc dữ liệu theo `tenantId` trong token. Bản đầu của tài liệu IDP API render bằng tài khoản **Admin (tenant 175)** nên ra `{items: [], totalItems: 0}`; gọi bằng **End User (tenant 176)** thì có bản ghi.
>
> **Bài học chung: với mọi endpoint lọc theo tenant, RESPONSE RỖNG có thể chỉ nghĩa là SAI TÀI KHOẢN, không phải KHÔNG CÓ DỮ LIỆU.** Kiểm `roleId` một lần sau login và lưu vào `RoleId`.
>
> **⚠ Một số endpoint cả hai vai đều lỗi** (`GET /user/me`, `GET /template/document-set` ghi *"Cả hai vai đều lỗi"* mà vẫn trả `200`) — **⚠ cần xác thực bằng gọi thật** trên tài khoản của đội trước khi tin bảng quyền.

### 3.2 `Template`

**Mục đích.** Định nghĩa một loại giấy tờ trên IDP: `description` là **prompt phân loại**, `fields` là schema trường lẻ, `tableFields` là schema cột bảng.

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `IdpTemplateId` | Long Integer | ✅ | — | **Định danh bằng SỐ, không bằng tên** |
| `TenantId` | Long Integer | — | null | **`null` = dùng chung.** Cả 6 template của Claim MVP đều `tenantId: null` |
| `Code` | Text(255) | — | null | `Custom-T5384`, `Custom-T5383` |
| `Name` | Text(255) | ✅ | — | **Back Office đổi tên là mọi thứ dựa trên tên sẽ vỡ, âm thầm** |
| `Description` | Text(4096) | ✅ | — | **Prompt phân loại**, không phải mô tả cho người đọc |
| `Category` | Text(255) | ✅ | — | `Insurance - MBAL` · `Chứng từ y tế` · `Giấy tờ` — **⚠ category còn mang chữ MBAL trên tenant 176**; đây là di sản đặt tên, không phải dấu hiệu sai tenant |
| `FieldsJson` | Text | ✅ | — | Mảng `{name, description}`. **`fields[].name` CHÍNH LÀ khóa sẽ về trong `fields[].k` của kết quả** |
| `TableFieldsJson` | Text | ✅ | — | Mảng cột. **Mỗi template một bộ cột khác nhau** — xem §3.2.1 |
| `MapsToDocumentType` | Text(20) | — | null | **Crosswalk sang `T1-01`…`T2-04` — §7.3.** Đây là dữ liệu bảo trì, **không phải hằng số trong adapter** |

#### 3.2.1 Sáu template của Claim MVP, tenant 176 *(verbatim)*

| templateId | `name` | `code` | Ánh xạ A4 | Vai trò |
|---:|---|---|---|---|
| **5384** | `VNVision-Giấy ra viện/Hospital discharge papers` | `Custom-T5384` | ⚠ *(không có T1 tương ứng — A4 §1.4 loại giấy ra viện khỏi Tier 1)* | Nộp kèm, đường nhập tay |
| **5127** | `Giấy ra viện/Hospital discharge papers` | | idem | Bản thứ hai của cùng loại |
| **5383** | `Bảng kê chi phí khám chữa bệnh/List of medical expenses` | `Custom-T5383` | **`T1-01`** | **Chứng từ quan trọng nhất** — mọi dòng chi phí đến từ đây |
| **5299** | *(đơn thuốc)* | | **`T1-02`** | **⚠ Tên verbatim cần xác thực bằng gọi thật** `GET /template/5299` |
| **5340** | *(kết quả xét nghiệm)* | | **`T2-02`** | Phân loại thôi |
| **5312** | `Biên lai thanh toán viện phí` | | ⚠ *(gần `T1-01`, 5 cột)* | **Tùy chọn** |

**Hai bộ chứng từ có sẵn** *(verbatim `GET /template/document-set`)*:

| id | `name` | `templates` |
|---:|---|---|
| **193** | `Chứng từ y tế _ CSYT` | `5445`, `5444`, `5443`, **`5384`**, **`5127`**, **`5340`**, **`5299`** |
| **194** | `Claim assessment Demo` | **`5127`**, **`5383`**, **`5312`**, **`5299`**, `5291`, `5385`, `5287` |

Ba template phụ của set 193: `5445` *Money transfer request* · `5444` *Sales Invoice* · `5443` *Passport*.

> **⚠ Set 193 KHÔNG chứa `5383`** — tức là **không chứa bảng kê chi phí 13 cột**, tức là **không sinh được dòng chi phí nào**. Set 194 mới có. **Chọn set 194 cho demo, hoặc tạo set mới.** **⚠ cần xác thực bằng gọi thật.**
>
> **⚠ `description` của set 193 ghi nguyên văn "Bộ chứng từ y tế cho claim_type MEDICAL_CARE (chăm sóc y tế)"** — đây là **di sản MBAL trên tenant 176**. Đừng để nó kéo đội quay lại bộ enum MBAL: **mô tả là văn bản tự do, không phải ràng buộc.**

#### 3.2.2 ⚠⚠ Bộ cột `tableFields` khác nhau theo từng template *(verbatim)*

| templateId | Tên | Số cột | Cột |
|---:|---|---:|---|
| **5383** | Bảng kê chi phí khám chữa bệnh | **13** | `Nội dung` · `Đơn vị tính` · `Số lượng` · `Đơn giá BV` · `Đơn giá BH` · `Tỷ lệ thanh toán theo dịch vụ` · `Thành tiền BV` · `Tỷ lệ thanh toán BHYT` · `Thành tiền BH` · `Quỹ BHYT` · `Người bệnh cùng chi trả` · `Khác` · `Người bệnh tự trả` |
| **5218** | Bảng kê chi phí khám chữa bệnh *(CÙNG TÊN, KHÁC BỘ CỘT)* | **7** | `Nội dung` · `Đơn vị tính` · `Số lượng` · `Đơn giá` · `Thanh toán bảo hiểm y tế` · `Người bệnh thanh toán` · `Các phương thức thanh toán khác` |
| **5125** | Phiếu thanh toán nội trú | **9** | `STT` · `Nội dung` · `Mã` · `Đơn vị` · `Đơn giá` · `Số lượng` · `Thành tiền` · `BHYT trả` · `BN trả` |
| **5312** | Biên lai thanh toán viện phí | **5** | `STT` · `Tên DV kỹ thuật` · `Số lượng` · `Đơn giá` · `Thành tiền` |

**Ba kết luận cho việc map sang `ClaimLine`:**

1. **Tên cột là NHÃN TIẾNG VIỆT, không phải khóa máy** ⇒ phải có bảng ánh xạ cột **theo từng `templateId`**, **không dùng một map chung**.
2. **`PaidBySocialInsurance` CÓ, dưới BA tên khác nhau**: `Quỹ BHYT` (5383) · `Thanh toán bảo hiểm y tế` (5218) · `BHYT trả` (5125).
3. **`ServiceCategory` thì KHÔNG template nào có** ⇒ **`L13` phải do adapter ODC suy ra** theo bảng từ khóa §4.5, hoặc do thẩm định viên gõ. **⚠ cần hỏi IDP.**

**`AmountClaimed` cũng tách theo template:** `5383` có `Thành tiền BV` và `Thành tiền BH` **riêng**; `5125` và `5312` chỉ có **một** cột `Thành tiền`. Quy tắc adapter: **mặc định lấy `Thành tiền BV` khi có cả hai**, và vẫn đối chiếu với tổng footer theo `R5`.

**`UnitOfMeasure`:** cột `Đơn vị tính` ở `5383` và `5218`, `Đơn vị` ở `5125`.
**`OtherPayerAmount`:** cột `Các phương thức thanh toán khác` ở `5218`, `Khác` ở `5383`.
**`DrugRegistrationNo`:** **⚠ KHÔNG template bảng kê nào có** — nó nằm trên **đơn thuốc `T1-02`**, nên phải lấy từ template khác. **⚠ cần hỏi IDP.**

> **Điều quý nhất ở endpoint `GET /template/{id}`: nó cho phép kiểm chứng cấu trúc dòng chi phí MÀ KHÔNG CẦN chạy một hóa đơn thật.** `tableFields` khai đúng các cột IDP sẽ trích. Vẫn nên chạy một hóa đơn thật để xác nhận **giá trị**, nhưng **cấu trúc thì đã chốt được ngay hôm nay**.

### 3.3 `DocumentSet`

**Có dùng** — khái niệm này còn sống trong API của Claim MVP (`GET /template/document-set`, `templateSetCode` của D3 §I-07 tương ứng với nó).

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `IdpDocumentSetId` | Long Integer | ✅ | — | `193`, `194` |
| `Name` | Text(255) | ✅ | — | |
| `Description` | Text(512) | — | null | **⚠ Văn bản tự do — set 193 còn mang chữ `MEDICAL_CARE`, bỏ qua** |
| `TemplatesJson` | Text | ✅ | — | **⚠ Là mảng JSON các ID, KHÔNG phải quan hệ** — xem §8.9 |
| `PartnerId` | Long Integer | — | null | `40` trên staging |
| `UserId` | Long Integer | ✅ | — | `203` trên staging |

> **⚠⚠ Khi NỘP request, `templates` là CHUỖI NGĂN CÁCH BỞI DẤU PHẨY, không phải JSON array** *(verbatim)*:
> ```
> "templates": "5445,5444,5443,5384,5127,5340,5299"
> ```
> **Gửi array thì lỗi `400 · invalid input syntax for type bigint: "NaN"`. Swagger khai là `array` — đây là chỗ dễ sai nhất.**

### 3.4 `IdpRequest`

**Một lần nộp chứng từ để trích xuất.**

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `IdpRequestId` | Long Integer | ✅ | — | Wire: `requestId` |
| `ClaimDocumentId` | Long Integer | — | null | Cầu nối sang §2.3 |
| `Name` | Text(255) | ✅ | — | **Đặt `= ClaimNo`** — đây là kênh correlation duy nhất |
| `TemplatesCsv` | Text(500) | ✅ | — | **Chuỗi CSV** — §3.3 |
| `Status` | Integer | ✅ | `1` | **6 giá trị §4.29** |
| `PageCount` | Integer | ✅ | `0` | Đơn vị tính quota |
| `RequestGroupId` | Long Integer | — | null | |
| `ExtraParamsJson` | Text | — | null | Kênh truyền context, trả lại nguyên vẹn. **⚠ cần xác thực bằng gọi thật** rằng IDP trả lại khóa lạ ngoài tập nó khai |
| `IdempotencyKey` | Text(64) | ✅ | — | UUIDv4 |
| `SubmittedAt` / `CompletedAt` | Date Time | ✅ / — | now / null | |
| `RawResultJson` | Text | — | null | **Copy lần đọc đầu.** Xem cảnh báo dưới |
| `ErrorCode` | Integer | — | null | §5 |

> **⚠⚠ Trên lần đọc đầu, hệ hồ sơ COPY kết quả vào store của mình và từ khoảnh khắc đó hai bên PHÂN KỲ.** **Trích xuất là một ĐỀ XUẤT; bản ghi hồ sơ là QUYẾT ĐỊNH.** Mọi hiệu chỉnh sau đó ghi vào bản copy, và **cặp đôi giữ nguyên tính so sánh được MÃI MÃI** — chính điều đó làm cho *"thẩm định viên bác máy bao nhiêu lần"* trở thành một câu hỏi trả lời được, và do đó làm cho ngưỡng tự động duyệt **chỉnh được**.
>
> **⚠ KHÔNG CÓ giao diện đồng bộ, và sự vắng mặt đó là CÓ CHỦ Ý.** Không hợp đồng nào cho IDP ghi vào một hồ sơ, và không cái nào đối soát lại kết quả trích xuất với dữ liệu hồ sơ sau lần đọc đầu. **Cả hai hệ tham chiếu đã nghiên cứu đều có đúng một cái sync như vậy, và ở một trong hai nó XÓA các dòng hồ sơ đã bị xóa ở thượng nguồn** — nghĩa là một lần đọc lại **có thể đổi một khoản quyết toán trong khi người phê duyệt đang xem nó.** Ta gỡ mối nguy bằng cách **gỡ cơ chế**.

### 3.5 `IdpWorkflowRequest`

**Một phiên đánh giá.** Trên staging đây là workflow Langflow; theo hợp đồng mục tiêu D3 đây là `I-15`/`I-16`.

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `IdpWorkflowRequestId` | Long Integer | ✅ | — | |
| `ClaimId` | Long Integer | ✅ | — | |
| `WorkflowId` | Text(64) | ✅ | — | uuid flow. **Ghim theo Site Setting, mỗi môi trường một giá trị** |
| `Name` | Text(512) | — | null | Đặt `ASSESS-<ClaimNo>` |
| `Status` | Integer | ✅ | `1` | **7 giá trị §4.30** |
| `AutoStart` | Boolean | — | `false` | **Chỉ set được khi tạo qua API.** Trên UI luôn `false` |
| `OcrRequestIdsJson` | Text | ✅ | — | Quan hệ N–N với `IdpRequest`. **Trong ODC dựng entity nối `IdpWorkflowRequestOcrRequest`** |
| `ExtraInputDataJson` | Text | — | `[]` | |
| `ResultJson` | Text | — | null | **Toàn bộ kết quả, một lần.** Không có cột progress, không có partial |
| `StartedAt` / `FinishedAt` | Date Time | ✅ / — | now / null | |

> **⚠ Điều kiện chạy: MỌI OCR request con phải `Completed` (2).** Không thì ném lỗi.
>
> **⚠ Chỉ 3 lần ghi DB trong toàn vòng đời** ⇒ **không có tiến độ %**. Đừng dựng progress bar theo `Status`.
>
> **⚠ Không có cơ chế cứu request kẹt.** ODC phải tự dựng Timer timeout → `ClinicalAssessment.State = PARTIAL` + đẩy hàng đợi vận hành.

### 3.6 `IdpFieldMap`

**Mục đích.** Ánh xạ nhãn field IDP trả về (`fields[].k`, tiếng Việt, tự do) sang mã field ổn định của ODC. **Bắt buộc phải có**, vì `fields[].name` của template **là nhãn, không phải code**, và Back Office sửa prompt là `k` đổi ngầm.

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `IdpTemplateId` | Long Integer | ✅ | — | **Ánh xạ theo TỪNG template, không dùng map chung** |
| `K` | Text(512) | ✅ | — | Nhãn verbatim như IDP trả về |
| `FieldCode` | Text(20) | ✅ | — | Mã ổn định phía ODC. §6.2 "Any code" |
| `TargetEntity` | Text(30) | ✅ | — | `ClaimLine` · `InsuredEvent` · `ClaimDocument` |
| `DataType` | Text(20) | ✅ | `TEXT` | `TEXT` · `DATE` · `MONEY` · `INT` · `CODE` · `ENUM` |
| `IsMandatory` | Boolean | ✅ | `false` | Tham gia tính claim confidence (**MIN** trên tập field bắt buộc **CÓ GIÁ TRỊ**) |
| `Version` | Integer | ✅ | `1` | |
| `IsActive` | Boolean | ✅ | `true` | |

**Seed tối thiểu cho `5384` giấy ra viện** *(verbatim `fields[].name`)*: `Place Of Treatment` · `Số vào viện` · `Tên bệnh nhân` · `Tuổi` · `Giới tính` · `Ngày/ tháng/ năm sinh` · `Dân tộc` · `Nghề nghiệp` · `Số thẻ bảo hiểm y tế` · `Thời hạn hiệu lực của thẻ bảo hiểm y tế` · `Địa chỉ` · `Admission Date` · `Discharge Date` …

**Seed tối thiểu cho `5383` bảng kê** *(verbatim `fields[].name`)*: `Place Of Treatment` · `Tên bệnh nhân` · `Địa chỉ` · `Ngày sinh` · `Giới tính` · `Số CCCD` · `Thẻ bảo hiểm y tế` · `Số bảo hiểm y tế` · `Không có thẻ bảo hiểm y tế` · `Cơ sở đăng ký bảo hiểm y tế ban đầu` · `Mã số` …

> **⚠ Hai template dùng hai tên khác nhau cho CÙNG một khái niệm ngày sinh**: `5384` dùng `Ngày/ tháng/ năm sinh`, `5383` dùng `Ngày sinh`. **Đây chính là lý do bảng ánh xạ phải theo từng `templateId`.**
>
> **⚠ `Thẻ bảo hiểm y tế` và `Không có thẻ bảo hiểm y tế` là hai CHECKBOX riêng biệt** trên `5383`, mỗi cái trả `"có"` hoặc `"không"`. **Hai checkbox cùng `"có"` là mâu thuẫn dữ liệu — gắn cờ, không tự giải quyết** (§0.2 ③).
>
> **⚠ `fields[].name` của `5384` chứa `Place Of Treatment` — tiếng Anh, có khoảng trắng, chữ hoa giữa câu.** Giữ **verbatim**. Một `trim()` hay một `toLowerCase()` vô tình ở adapter là mất field.

### 3.7 `IdpReplay`

**Mục đích.** Chạy demo offline. Site Setting `ReplayMode = On` → mọi wrapper REST trả JSON đã ghi thay vì gọi mạng.

| Attribute | Kiểu ODC | Bắt buộc | Mặc định | Ghi chú |
|---|---|---|---|---|
| `Key` | Text(64) | ✅ | — | sha256 của file, hoặc `ClaimNo` |
| `Endpoint` | Text(255) | ✅ | — | `POST /request/sync`, `GET /workflow/request/{id}/result/json` … |
| `ResponseJson` | Text | ✅ | — | Body nguyên văn |
| `HttpStatus` | Integer | ✅ | `200` | **Phải ghi cả case lỗi** — §5.3 |
| `RecordedAt` | Date Time | ✅ | now | |
| `TemplateSetId` | Long Integer | — | null | |
| `FlowVersion` | Text(64) | — | null | `workflowId` lúc ghi. **Flow đổi là bản ghi hết giá trị** |

> **Mock của D3 §I-07 cho phép bốn kết cục viết kịch bản được**, chọn bằng một giá trị ma thuật trong `providerNameHint`: **kết quả sạch · độ tin cậy thấp · thiếu chứng từ · lỗi cứng**. Dựng đúng bốn cái đó trong `IdpReplay` là đủ để demo toàn bộ vòng xem-xét-và-chạy-lại.

---

## 4. Bộ enum đầy đủ

> **Ba mươi bộ.** Mọi enum dựng bằng **Static Entity**. Mã enum là **tiếng Anh UPPERCASE_SNAKE** *(D2 §2.6)*, trừ năm bộ có mã tiếng Việt verbatim — giữ nguyên và ghi rõ.

### 4.1 `documentType` — 8 giá trị, 3 tier ⭐ *(verbatim A4 §1)*

| Tier | IDP làm gì | Số loại |
|---|---|---:|
| **Tier 1 — Trích xuất** | Phân loại, rồi trích field và bảng có cấu trúc | **4** (3 bằng OCR, 1 bằng XML) |
| **Tier 2 — Chỉ phân loại** | Nhận ra loại chứng từ; **không trích field nào** | **4** |
| **Tier 3 — Không xử lý** | Lưu và đính kèm; **không AI** | mọi thứ còn lại |

| Giá trị | Tên VI | Tên EN | Tier | Phương thức | Vì sao nó ở đây |
|---|---|---|:---:|---|---|
| **`T1-01`** | Bảng kê chi phí khám chữa bệnh | Itemised cost statement | 1 | OCR | **Chứng từ quan trọng nhất một cách tuyệt đối.** Mọi dòng chi phí chi trả được đến từ nó |
| **`T1-02`** | Đơn thuốc | Prescription | 1 | OCR | Tên thuốc, hàm lượng, số lượng — đầu vào cho rule từ chối thuốc-với-chẩn-đoán. **Chứng từ này KHÔNG mang tiền** |
| **`T1-03`** | Phiếu khám bệnh (có chẩn đoán) | Outpatient examination record | 1 | OCR | **Chịu lực gấp đôi**: chẩn đoán vừa dẫn ra nhóm bệnh (thời gian chờ) **vừa** là đầu vào nhánh lâm sàng lập luận trên đó |
| **`T1-04`** | Hóa đơn GTGT / hóa đơn bán hàng | VAT / sales invoice | 1 | **CHỈ XML** | Bên phát hành, mã số thuế, định danh hóa đơn, tổng tiền |
| **`T2-01`** | Giấy tờ tùy thân (CCCD / Hộ chiếu / Giấy khai sinh) | Identity document | 2 | phân loại | Danh tính đã có trên hợp đồng |
| **`T2-02`** | Kết quả cận lâm sàng | Laboratory and imaging result | 2 | phân loại | Chống đỡ quyết định từ chối *"xét nghiệm không liên quan chẩn đoán"* |
| **`T2-03`** | Báo cáo y tế / Giấy chứng nhận phẫu thuật | Medical report / surgery certificate | 2 | phân loại | Đầy đủ checklist |
| **`T2-04`** | Biên bản / xác nhận về hoàn cảnh xảy ra sự cố *(tên tạm)* | Statement of the circumstances of an incident | 2 | phân loại | **Mới ở A4 v1.3.** Loại duy nhất tồn tại vì **một ĐIỀU KHOẢN hợp đồng** đòi, chứ không vì một loại đợt điều trị đòi |
| `unclassified` | — | — | — | — | **Mặc định, và KHÔNG BAO GIỜ là loại gần giống nhất** |

> **⚠ `T2-04` được thêm vào với tên và mức mặc định CỐ Ý CHƯA CHỐT, và đó không phải sự lười biếng.** Chưa chốt: yêu cầu là **biên bản công an**, **giấy xác nhận nhập viện**, hay **cả hai thành hai loại riêng**; tên tiếng Việt và tiếng Anh; **`REQUIRED` hay `EXPECTED`** mặc định trên hồ sơ tai nạn *ngoài* mọi điều khoản; và có cần **bản gốc** không. **Mức mặc định CỐ Ý VẮNG khỏi bảng §2.15** — một loại xuất hiện ở mức `REQUIRED` cho mọi hồ sơ tai nạn là một đòi hỏi **khác về bản chất** với một loại chỉ đến qua một điều khoản. **Cho tới khi chốt, `T2-04` chỉ tới được hồ sơ qua một điều khoản. Đó là một trạng thái làm việc được, không phải một cách lách.** **⚠ cần chốt với PO / A4.**

**Loại trừ CÓ CHỦ Ý, và vì sao** *(verbatim A4 §1.4)*:

| Bị loại | Lý do |
|---|---|
| **Giấy yêu cầu bồi thường** | **App thay thế nó.** Người yêu cầu, quan hệ, tài khoản nhận tiền và số tiền đòi đều là field form. **Không có lý do gì OCR một chứng từ mà chính ta đã sinh ra dữ liệu cho nó** |
| **Giấy ra viện** | Đợt nội trú xử lý bằng **nhập tay** trong MVP 1 |
| **Sổ khám bệnh** | Viết tay; chứng từ khó nhất trong bộ; nội dung trùng với phiếu khám |
| **Phiếu điều trị răng** | Hồ sơ nha khoa dùng bảng kê và phiếu khám như mọi hồ sơ khác |
| **Biên bản tai nạn** | Hồ sơ tai nạn trong phạm vi; biên bản là đính kèm Tier 3 do thẩm định viên xem |

> **⚠⚠ Hệ quả của việc bỏ giấy ra viện, nêu ra để nó là một QUYẾT ĐỊNH chứ không phải một bất ngờ.** Đợt **nội trú** có các sự kiện được **thẩm định viên gõ tay** thay vì trích xuất. Hồ sơ nội trú **vẫn chạy được đầu-tới-cuối**: bảng kê vẫn trích bình thường, cây quyền lợi vẫn áp trần tiền giường theo ngày và đếm 60 ngày, quyết toán vẫn tính như thường. **Thứ thay đổi là PHÉP ĐO: chỉ tiêu tự động điền field và chỉ tiêu độ chính xác đo TRÊN HỒ SƠ NGOẠI TRÚ MÀ THÔI. Hồ sơ nội trú là một đường ĐƯỢC HỖ TRỢ, không phải một đường ĐƯỢC TỰ ĐỘNG HÓA, trong MVP 1.**

**Quyết định e-invoice và ba hệ quả** *(verbatim A4 §1.5)*:

> **Hóa đơn GTGT được tiêu thụ CHỈ dưới dạng XML e-invoice có cấu trúc. KHÔNG CÓ đường OCR cho ảnh hóa đơn.**

1. **Bảng kê, KHÔNG PHẢI hóa đơn, là nguồn của dòng chi phí.** Thẩm định **do đó không dừng lại nếu XML vắng mặt**.
2. **Không có XML** ⇒ hồ sơ đi tiếp trên bảng kê một mình, mục checklist hóa đơn đánh dấu **đã-nhận-nhưng-chưa-xác-minh**, và rule `R6` **bị bỏ qua kèm một ghi chú. Đây KHÔNG ĐƯỢC là một lỗi chặn.**
3. **Không có xác minh với cơ quan thuế trong dự án này**, và cũng không có kế hoạch cho MVP 2. Mã cơ quan thuế và khối chữ ký được bắt vì **chúng đến miễn phí cùng phép parse; không gì tiêu thụ chúng.**

### 4.2 `lossType` — 12 giá trị ⭐ *(verbatim D2 §4.5)*

**Hai chiều: BỆNH LÝ × NƠI ĐIỀU TRỊ.**

| Bệnh lý \ Nơi | Ngoại trú | Nội trú |
|---|---|---|
| **Bệnh** | `ILL_OP` | `ILL_IP` |
| **Nha khoa** | `DEN_OP` | `DEN_IP` |
| **Tai nạn** | `ACC_OP` | `ACC_IP` |
| **Thai sản** | `MAT_OP` | `MAT_IP` |

Cộng **4 giá trị tử vong / thương tật vĩnh viễn**: *tử vong do bệnh hoặc thai sản* · *tử vong do tai nạn* · *thương tật vĩnh viễn do tai nạn* · *tổn thương sức khoẻ vĩnh viễn do bệnh*.

> **⚠ Mã định danh của 4 giá trị này KHÔNG có trong D2 §4.5 — cần A1 §4.2 cấp phát. ⚠ cần chốt với A1.**

**⚠ Ba ràng buộc bề mặt:**
1. **App khách hàng chỉ hiển thị 8 giá trị đầu.** Bốn giá trị tử vong/thương tật bị bỏ khỏi **APP**, không bỏ khỏi **MODEL**. Lý do: *"Một hồ sơ tử vong không do người được bảo hiểm nộp từ điện thoại của chính họ; nó do một người thừa kế nộp, thường là tại quầy, với những chứng từ mà checklist của app không chứa. Chào khách một lựa chọn tử vong trên một màn hình rồi bảo họ chụp một bảng kê chi phí còn tệ hơn là không chào."*
2. **Console và đường tiếp nhận tại quầy hiển thị đủ 12.**
3. **Server phải chặn CẢ TẬP LẪN BỀ MẶT** — một client chào bốn giá trị kia sẽ sinh ra một hồ sơ **có checklist không thể thỏa mãn**.

> **⚠ A1 và D2 từng đăng ký 12 giá trị KHÁC NHAU dưới cùng một tên.** A1 dùng **nhóm bệnh × nơi điều trị** (`PRE_OP`, `SPE_IP`, `GEN_OP`, `ACC_IP`, `BIR_OP`, `MAT_IP`…); D2 dùng **bệnh lý × nơi điều trị**. **Cùng số lượng, cùng tên, khác thành viên.** A1 v1.5 đã cắt lại theo D2 và ghi lại lý do trong một câu: *"A1 was the outlier; D2 was right."* **Tập ở trên là tập chuẩn.**

### 4.3 `episodeType` — 5 giá trị *(verbatim)*

| Giá trị | Chiếu từ `lossType` |
|---|---|
| `OUTPATIENT` | `ILL_OP` |
| `INPATIENT` | `ILL_IP` |
| `DENTAL` | `DEN_OP` · `DEN_IP` |
| `ACCIDENT` | `ACC_OP` · `ACC_IP` |
| `MATERNITY` | `MAT_OP` · `MAT_IP` |

> **⚠ `episodeType` KHÔNG bị khai tử. `lossType` BAO HÀM nó chứ không thay thế nó.** Nó sống tiếp như một **PHÉP CHIẾU CHỈ ĐỌC** trên sự kiện bảo hiểm, vì đó là thứ ánh xạ quyền lợi của A5 §8.1 khóa lên, và A5 không đổi.
>
> **⚠ Chiều là CỐ ĐỊNH và MỘT HƯỚNG:** `lossType` được nhập, `episodeType` được suy ra từ nó, và **code nào cho phép hai cái lệch nhau là code có lỗi.**

### 4.4 `conditionClass` — 6 giá trị ⭐ *(verbatim D2 §3.2.4)*

| Giá trị | Nhãn VI |
|---|---|
| `PRE` | Bệnh có sẵn |
| `SPE` | Bệnh đặc biệt |
| `GEN` | Bệnh thông thường |
| `BIR` | Sinh đẻ |
| `MAT` | Biến chứng thai sản |
| `ACC` | Tai nạn |

**Đúng sáu. A1 §4 là danh sách chuẩn của mã; A5 §5 là bảng chuẩn của số ngày.**

> **⚠ Tổ hợp VẮNG MẶT là VẮNG MẶT, không phải BẰNG KHÔNG.** A5 §5 đánh dấu các tổ hợp không thể xảy ra là `n/a` và **yêu cầu chúng KHÔNG ĐƯỢC MANG theo chút nào**. Trên wire chúng là `null`, và **`null` nghĩa là tổ hợp không thể xảy ra** — một hồ sơ sinh ra một tổ hợp như vậy là **LỖI DỮ LIỆU, không phải thời gian chờ 0 ngày.**

### 4.5 `serviceCategory` — 20 giá trị ⭐ *(verbatim A4 §4.5)*

| Giá trị | Nhận ra từ | Ngoại trú | Nội trú |
|---|---|---|---|
| `CONSULT` | Tiền khám, Công khám, Khám bệnh | 200.1 | 100 |
| `LAB` | Xét nghiệm, Sinh hóa, Huyết học, Nước tiểu | 200.1 | 100 |
| `IMAGING` | Chụp, Siêu âm, X-quang, CT, MRI, Nội soi | 200.1 | 100 |
| `PROCEDURE` | Thủ thuật, Phẫu thuật, Tiểu phẫu | 200.1 | 100.2 |
| `DRUG` | Dòng thuốc khớp, hoặc có `Số đăng ký`, hoặc nhóm dịch vụ đọc là Thuốc | 200.2 | 100 |
| `SUPPLEMENT` | Khớp danh sách thực phẩm chức năng | **200.2** | **100** |
| `PHYSIO` | Vật lý trị liệu, Phục hồi chức năng | 200.3 | 100 |
| `DENTAL` | Răng, Nha khoa, Trám, Nhổ | 300.1 | n/a |
| `ROOM` | Tiền giường, Giường bệnh, Tiền phòng | n/a | 100.1 |
| `ICU` | Hồi sức, Cấp cứu tích cực, ICU, Chăm sóc đặc biệt | n/a | 100.1.1 |
| `AMBULANCE` | Xe cứu thương, Vận chuyển cấp cứu | 100.4 | 100.4 |
| `DELIVERY_NORMAL` | Sinh thường, Đẻ thường | n/a | 400.1 |
| `DELIVERY_CS` | Sinh mổ, Mổ lấy thai | n/a | 400.2 |
| `MATERIAL` | Vật tư y tế, Vật tư tiêu hao | 200.1 | 100 |
| `ADMIN` | Phí hồ sơ, Phí dịch vụ, Phí photocopy, Phí đăng ký | **không ánh xạ** | **không ánh xạ** |
| `SCREENING` | Khám sức khỏe, Tầm soát, Khám định kỳ | 200.1 | 100 |
| `VACCINATION` | Tiêm chủng, Vắc xin, Tiêm phòng | 200.2 | 100 |
| `OPTICAL` | Kính thuốc, Kính áp tròng, Đo khúc xạ, Gọng kính | 200.1 | 100 |
| `HEARING` | Máy trợ thính, Đo thính lực, Thiết bị trợ thính | 200.1 | 100 |
| `OTHER` | Mọi thứ còn lại | thẩm định viên quyết | thẩm định viên quyết |

**Khi loại tổn thất của đợt là TAI NẠN, mọi quyền lợi đã ánh xạ đều được chuyển hướng về `500`.**

> **⚠⚠ `SUPPLEMENT` và `ADMIN` ĐÃ THÔI TỪ CHỐI ở v1.3.** Cả hai từng đọc là *từ chối `NOTCOV`*. **Bốn thứ sai với điều đó:**
> **(1) Sai BƯỚC.** Từ chối về phạm vi là của bước 4 hoặc bước 6; §4.5 là một **ÁNH XẠ** — việc của nó là nói **một dòng được bảo hiểm rơi vào đâu**, và một category tự từ chối là đã đặt quyết định bảo hiểm vào trong bảng ánh xạ.
> **(2) Sai MÃ.** `NOTCOV` nghĩa là *không phải một khoản y tế được bảo hiểm*; một thực phẩm chức năng bị loại trừ bởi câu chữ hợp đồng thì bị loại trừ bởi một **ĐIỀU KHOẢN**, và mã cho việc đó là `EXCL`.
> **(3) KHÔNG CÓ ĐIỀU KHOẢN NÀO.** Một quyết định từ chối sinh ra ở đây **không trích dẫn gì cả**, mà khách phải được cho biết điều khoản nào đã được dựa vào.
> **(4) VÔ ĐIỀU KIỆN — và đây là cái sẽ đến dưới dạng một lời khiếu nại.** Câu chữ hợp đồng đọc là *"thực phẩm chức năng … **không được kê như một liệu trình điều trị cho tình trạng đã chẩn đoán**"*, còn §4.5 đang từ chối **MỌI** dòng thực phẩm chức năng **kể cả một dòng đã được kê.**
>
> **Thứ thay thế:** `SUPPLEMENT` ánh xạ vào `200.2` ngoại trú và `100` nội trú — nơi một thực phẩm chức năng được kê **thực sự sẽ được trả** — và một điều khoản quyết ở bước 4 xem loại trừ có cắn không, **nêu lên một checkpoint** vì mệnh đề *"được kê như một liệu trình"* **không có sự kiện nào đứng sau và một con người phải quyết**. **`ADMIN` ánh xạ vào KHÔNG GÌ CẢ**, và một dòng không có ánh xạ rơi xuống bước 6 và bị từ chối `NOTCOV` **ở đó** — *không phải một khoản y tế được bảo hiểm*, điều mà với một khoản phí photocopy là **đúng mã áp ở đúng bước**.

> **⚠ Bốn category mới KHÔNG MIỄN PHÍ với IDP.** A5 ghi `SCREENING`, `VACCINATION`, `OPTICAL`, `HEARING` là *"master data của Claim Platform, của ta để định nghĩa — không câu hỏi vendor, không phụ thuộc, một lần đổi seed"*, và **với ÁNH XẠ QUYỀN LỢI thì đúng y như vậy**. **Nhưng `L13` do IDP SUY RA** — nên **ai đó phải dạy phép suy ra bốn giá trị mới, và ai đó là IDP.** Hai phát biểu không mâu thuẫn; chúng nói về **hai nửa khác nhau của cùng một field**, và nửa A5 không sở hữu là nửa chưa ai hỏi tới. **Nếu câu trả lời là *không phải giai đoạn này*, bốn giá trị đó do THẨM ĐỊNH VIÊN GÕ.** **⚠ cần hỏi IDP.**

> **Danh sách category và từ khóa của nó là DỮ LIỆU THAM CHIẾU, sửa được lúc chạy. Phép suy ra sinh `L13` từ một tên khoản đã in thì KHÔNG — nó của IDP, và nó đổi bằng change request.**

### 4.6 `itemClass` (D9 trên `T1-02`) — 4 giá trị *(verbatim)*

`Thuốc` · `Thực phẩm chức năng` · `Vật tư` · `Khác` — **suy ra; dẫn ra rule từ chối thực phẩm chức năng**. Mã tiếng Việt verbatim.

### 4.7 `claimState` — 13 giá trị ⭐ *(verbatim D2 §7.1)*

| # | Giá trị | Loại | Khách thấy là | Sửa được? | Vị thế sổ cái |
|---:|---|---|---|---|---|
| 1 | `DRAFT` | Khách hàng | *Nháp* | Có, bởi khách | Không có |
| 2 | `SUBMITTED` | Chuyển tiếp | *Đã gửi* | Không | Không có |
| 3 | `DOC_PROCESSING` | **Vận hành** | *Đang xử lý chứng từ* | Không | Không, tới khi chain chạy |
| 4 | `PENDING_INFO` | Khách hàng | *Cần bổ sung chứng từ* | Chứng từ, bởi khách | Tạm tính, nếu đã chạy chain |
| 5 | `EXTRACTION_ERROR` | **Vận hành** | *Đang xử lý chứng từ* — **KHÔNG BAO GIỜ hiện chính nó** | Có, bởi thẩm định viên | Không có |
| 6 | `UNDER_ASSESSMENT` | Nội bộ | *Đang thẩm định* | Có, bởi người được giao | Tạm tính |
| 7 | `PENDING_APPROVAL` | Nội bộ | *Đang thẩm định* — **hiện như `UNDER_ASSESSMENT`** | **Không — KHÓA** | Tạm tính |
| 8 | `APPROVED` | Kết quả | *Đã duyệt* | Không | **CHỐT** |
| 9 | `DECLINED` | Kết quả | *Từ chối* | Không | Tạm tính bị đảo |
| 10 | `PAYMENT_IN_PROGRESS` | **Vận hành** | *Đang chi trả* | Không | Chốt |
| 11 | `PAID` | Kết quả | *Đã chi trả* | Không | Chốt |
| 12 | `CLOSED` | Kết thúc | *Đã đóng* | Không | Chốt |
| 13 | `CANCELLED` | Kết thúc | *Đã hủy* | Không | Mọi bút toán tạm tính bị đảo |

**Mười ba trạng thái nội bộ ánh xạ sang MƯỜI trạng thái khách nhìn thấy.**

> **⚠⚠ Ba trạng thái Vận hành là LÝ DO bảng này có cột "khách thấy là".** Trích xuất kẹt, dịch vụ không với tới, chi trả treo — **đó là hỏng của TA, không phải quyết định về hồ sơ của khách.** `EXTRACTION_ERROR` **không bao giờ xuất hiện với khách dưới bất kỳ nhãn nào của chính nó** — nó hiện là *Đang xử lý chứng từ*, y hệt `DOC_PROCESSING`, và dòng thời gian ghi một thông điệp **cố ý viết để đọc như một cập nhật tiến độ, vì từ phía khách thì nó đúng là một cập nhật tiến độ.**
>
> **`PENDING_APPROVAL` cũng bị che, nhưng vì lý do khác.** Nó không phải một hỏng vận hành; nó là một bước nội bộ. **Đặt tên một giai đoạn phê duyệt cho khách là phơi bày cấu trúc phân tách nhiệm vụ mà chẳng được gì.**
>
> **`UNDER_ASSESSMENT` thì KHÔNG bị che** — nó là *Đang thẩm định*, **khác** *Đang xử lý chứng từ*, và nó **phải** khác, vì một thông báo bắn đi khi vào trạng thái đó. **Một ánh xạ gộp hai cái làm một sẽ bắn một thông báo loan báo một thay đổi mà màn hình của khách không hiển thị.**
>
> **⚠ Cột sổ cái là cột dễ làm sai nhất.** Ghi sổ hai pha: **tạm tính khi phép tính chạy, CHỐT TẠI PHÊ DUYỆT — KHÔNG PHẢI TẠI CHI TRẢ.** Một khoản chi hỏng sinh ra **bút toán đảo, không bao giờ là một phép xóa**, và **sổ cái KHÔNG bị bỏ chốt.**

> **⚠ Một hồ sơ quá `PeriodTo` KHÔNG phải `EXPIRED`** — không có trạng thái đó, và §1.5 nói vì sao.

### 4.8 `lineDecision` — 2 giá trị *(verbatim, mã tiếng Việt)*

`CHI_TRẢ` · `TỪ_CHỐI`. **Mọi dòng phải mang một quyết định trước khi trình duyệt.**

### 4.9 `eventDecision`

`TRONG_PHẠM_VI` · `NGOÀI_PHẠM_VI`. **⚠ Mã verbatim cần xác thực với A1.**

### 4.10 `declineReason` — 7 mã ĐÓNG ⭐ *(verbatim A5 §6.3)*

| Giá trị | Cấp áp dụng | Sinh bởi bước nào |
|---|---|---|
| `LIMIT` | Sự kiện + Dòng | Bước 8, 9, 12, 13 |
| `PERIOD` | Sự kiện + Dòng | Bước 1 |
| `NOBEN` | Sự kiện + Dòng | Bước 5 |
| `WAIT` | Sự kiện + Dòng | Bước 2 |
| `EXCL` | Sự kiện + Dòng | Tầng điều khoản (loại trừ hợp đồng) |
| **`NOTREL`** | **CHỈ cấp dòng** | Bước 6 — rà từng dòng |
| **`NOTCOV`** | **CHỈ cấp dòng** | Bước 6 — rà từng dòng |

**⚠ Bốn ràng buộc:**
1. **`NOTREL` và `NOTCOV` là hai mã DUY NHẤT chỉ tồn tại ở cấp dòng.** Màn cấp sự kiện **không được hiển thị chúng**.
2. **Đây là BỘ ĐÓNG 7 MÃ. Không màn hình nào được mở rộng.** Màn điều khoản chỉ chào **một** mã: `EXCL` — *"một điều khoản là một loại trừ hợp đồng và mã trung thực duy nhất cho nó là `EXCL`"*.
3. **`NOTREL` và `NOTCOV` ràng buộc `{itemName}` và `{itemAmount}`; `EXCL` KHÔNG ràng buộc cái nào** — nó là *"Khoản chi phí này thuộc điểm loại trừ {exclusionRef}…"*, một placeholder, và đó là điều khoản.
4. **⚠ Mẫu `EXCL` hiện chỉ mang MỘT `{exclusionRef}` trong khi quy tắc đòi MỌI `clauseRef` đã bắn trên một dòng, theo thứ tự.** **Một lá thư trích dẫn một trong hai điều khoản là một lá thư có thể bị bác bằng cách bác nhầm cái kia.** **⚠ cần A2 chốt.**

### 4.11 `approvalDecision` — 3 giá trị *(verbatim, mã tiếng Việt)*

`PHÊ_DUYỆT` · `TRẢ_LẠI` · `TỪ_CHỐI_TOÀN_BỘ`

### 4.12 `proposedReasonCode` — 2 giá trị ⭐ *(verbatim D3 §I-16)*

`NOTREL` · `NOTCOV`

> **⚠ CHỈ HAI MÃ.** Mọi thứ khác là một câu hỏi về **chính sách** và thuộc về Track 1. **Một phép đánh giá trả về bất kỳ mã nào khác BỊ TỪ CHỐI TẠI ADAPTER.**
>
> **⚠ Một dòng KHÔNG CÓ đề xuất thì KHÔNG PHẢI "đã duyệt".** Nó đơn giản là **không bị đề xuất chống lại**. `linesAssessed` nói cái gì đã được nhìn; **một dòng vắng khỏi nó là một dòng KHÔNG ĐƯỢC ĐÁNH GIÁ, và đó khác với SẠCH.**

### 4.13 `recDecision` — 3 giá trị

| Giá trị | Ghi chú |
|---|---|
| `Proposed` | Trạng thái khi agent ghi. **Không tác động tới tiền** |
| `Confirmed` | **Bắt buộc có `DecisionReason`.** Kích hoạt chạy lại chain |
| `Rejected` | **Bắt buộc có `DecisionReason`** |

### 4.14 `confidenceBand` — 3 giá trị

`HIGH` · `MEDIUM` · `LOW`

> **⚠⚠ Confidence dẫn ra ĐỊNH TUYẾN THẨM ĐỊNH VIÊN, KHÔNG PHẢI cổng tự động duyệt.** *(verbatim A4 §7)* Nó quyết định hồ sơ nào và **field nào** một thẩm định viên được chỉ tới trước. **Cổng của D1 §5.8 kiểm ba điều kiện và confidence KHÔNG nằm trong đó** — sự vắng mặt ấy được ghi lại ở đó, **có chủ ý**.
>
> **⚠ Dưới ngưỡng thì định tuyến tới thẩm định viên kèm TÊN FIELD CỤ THỂ — không phải một câu "cần xem lại" chung chung.**
>
> **⚠ Giá trị ngưỡng ĐẶT TỪ BENCHMARK GROUND-TRUTH, KHÔNG PHẢI ĐOÁN.** **⚠ cần xác thực bằng gọi thật** trên bộ chứng từ đã ẩn danh trước khi chốt.
>
> **Claim-level confidence = MIN trên tập field bắt buộc — CỐ Ý là min chứ không phải trung bình, để MỘT số tiền đọc sai KHÔNG THỂ bị trung bình hóa đi mất.** Row confidence = **min các ô của dòng**.

### 4.15 `requirementLevel` — 3 giá trị ⭐ *(verbatim D2 §5.5.1)*

| Giá trị | Nhãn VI | Hành vi |
|---|---|---|
| `REQUIRED` | **Bắt buộc** | **Chặn quyết toán. KHÔNG miễn được** — nút *Miễn yêu cầu* **VẮNG MẶT, không phải bị disable** |
| `EXPECTED` | **Cần có** | Sinh yêu cầu bổ sung mà thẩm định viên **có thể miễn**, kèm lý do |
| `OPTIONAL` | **Tùy chọn** | **Không bao giờ yêu cầu, không bao giờ chặn.** Hiện ra để thẩm định viên thấy cái gì đã tới dù không được hỏi |

> **⚠ Không có chứng từ Tier 3 nào xuất hiện ở cột `REQUIRED` hay `EXPECTED`** — **đó chính là thứ làm cho Tier 3 thực sự tùy chọn thay vì tùy chọn trên danh nghĩa.**
>
> **⚠ Không có nút disable vì lý do nghiệp vụ.** Một hành động không được liệt kê cho một trạng thái thì **KHÔNG TỒN TẠI** trong trạng thái đó — control **vắng mặt**, và server **từ chối lời gọi**. **Không có hạng mục thứ ba "nhìn thấy nhưng bị vô hiệu"; một control bị vô hiệu mà người dùng không giải thích được là một cuộc gọi hỗ trợ.**

### 4.16 `checklistItemState` — 5 giá trị *(verbatim, mã tiếng Việt)*

`CHƯA_NHẬN` · `ĐÃ_NHẬN` · `ĐÃ_YÊU_CẦU` · **`ĐÃ_YÊU_CẦU_KHÔNG_BỔ_SUNG`** *(kết thúc 1)* · **`MIỄN_YÊU_CẦU`** *(kết thúc 2)*

**Hai trạng thái kết thúc, không phải một** — "đã hỏi và không nhận được" khác "đã quyết định không cần".

### 4.17 `packageState` — 6 giá trị ⭐ *(verbatim A4 §6)*

| Giá trị | Nghĩa |
|---|---|
| `COMPLETE` | **Mọi** loại chứng từ bắt buộc cho đợt này đều có mặt và đã trích xuất |
| `MISSING_DOCUMENT` | Một loại bắt buộc vắng mặt — **response NÊU TÊN loại nào** |
| `MISSING_FIELD` | Đủ loại, nhưng một hoặc nhiều field bắt buộc không đọc được — **response NÊU TÊN field và chứng từ** |
| `INCONSISTENT` | Có mặt và đọc được, nhưng một hoặc nhiều rule đối chiếu thất bại |
| `AWAITING_ORIGINAL` | Đủ về mặt số, chờ bản gốc giấy |
| `ERROR` | Hỏng xử lý dứt điểm |

> **⚠ Sinh bởi `R9` và trả về cùng kết quả. Nó là thứ dẫn ra chuyển trạng thái `PENDING_INFO`, nên ENGINE TIÊU THỤ TRẠNG THÁI CHỨ KHÔNG SUY LẠI.**
>
> **⚠ `AWAITING_ORIGINAL` ở trong model nhưng TẮT BẰNG CẤU HÌNH trong pilot** — **bản gốc KHÔNG bắt buộc; hồ sơ quyết toán hoàn toàn trên ảnh đã chụp. Không màn hình nào hỏi bản gốc và KHÔNG HỒ SƠ NÀO CHỜ GIẤY.** Chúng còn lại vì **bật chúng về sau phải là CẤU HÌNH, không phải một thay đổi máy trạng thái**.
>
> **⚠ ĐỪNG NHẦM `state` với `outcome`.** Webhook mang **`outcome`** (`COMPLETE | PARTIAL | ERROR`) — kết cục **XỬ LÝ**. Kết quả `I-09` mang **`state`** — độ **ĐẦY ĐỦ CỦA BỘ HỒ SƠ**. **Hai enum khác nhau dưới một tên property, một cái tả xử lý và một cái tả bộ chứng từ, là đúng kiểu va chạm sinh ra một nhánh sai sáu tháng sau.**

### 4.18 `fileProcessingState` — 3 giá trị *(verbatim, mã tiếng Việt)*

`CHƯA_XỬ_LÝ` · `ĐÃ_XỬ_LÝ` · `LỖI`. **Theo từng FILE, không theo mục checklist và không theo bộ hồ sơ. CHỈ HIỂN THỊ, không thao tác được.**

> **Lý do của sự giảm thiểu đó:** hệ tham chiếu cho mỗi file một ô tick và cho thẩm định viên gửi lại một tập con để trích xuất lại. **Ta KHÔNG.** Trích xuất lại ở đây là một **request MỚI trên TOÀN BỘ bộ hồ sơ**, không mutate gì. Một lần gửi lại theo từng file sẽ sinh ra một hồ sơ mà kết quả trích xuất là **một bức khảm của hai lần chạy với hai `requestId` và hai chuẩn confidence**, và claim confidence — min trên tập field bắt buộc — khi đó là **một min trên một tập không nhất quán**.
>
> **Nhưng nó PHẢI được hiển thị:** một bộ 12 file mà một trang hỏng hôm nay chỉ hiện một trạng thái cấp hồ sơ, nên **thẩm định viên nhìn một hồ sơ treo không thấy được FILE NÀO treo và không có gì hữu ích để nói với hàng đợi vận hành.**

### 4.19 `copyOrOriginal` — 2 giá trị *(verbatim, mã tiếng Việt)*

`BẢN_SAO` · `BẢN_GỐC`. **Trong model, ẩn bằng cấu hình trong pilot.**

### 4.20 `documentSource` — 2 giá trị *(verbatim, mã tiếng Việt)*

`KHÁCH_HÀNG` · `CÁN_BỘ_THẨM_ĐỊNH`

### 4.21 `termOutcome` và `termStatus` *(verbatim D2 §5.29)*

**`termOutcome` — 4 giá trị:** `ĐẠT` *(đã kiểm, không bắn)* · `TỪ_CHỐI` *(điều khoản từ chối)* · `ĐIỂM_KIỂM` *(nêu một checkpoint — **bất kỳ hạng nào cũng có thể sinh ra nó**)* · `KHÔNG_THUỘC_PHẠM_VI` *(bộ chọn không khớp, điều khoản chưa bao giờ vào cuộc)*

**`termStatus` — 4 giá trị:** `DRAFT` · `PENDING_ACTIVE` · `ACTIVE` · `WITHDRAWN`

> **⚠ CHỈ điều khoản `ACTIVE` trong cửa sổ hiệu lực được đánh giá, và CHỈ những cái đó được ĐẾM.** Một điều khoản `DRAFT` **vô hình với engine**: nó không được đánh giá, không sinh `termOutcome`, và **không được đếm**. **Một panel đếm cả `DRAFT` sẽ nói với thẩm định viên rằng bốn điều khoản đã được xem xét trong khi engine chưa bao giờ nhìn tới chúng — CÙNG HẠNG NÓI DỐI với việc chỉ đếm những cái đã bắn.**
>
> **Đánh giá MỌI điều khoản trong phạm vi, KHÔNG BAO GIỜ dừng ở cái khớp đầu tiên**, và trả một kết quả cho từng cái.

### 4.22 `claimantStanding` — 4 giá trị, và chứng từ mỗi giá trị kéo theo ⭐

| `standing` | Nhãn VI | Người yêu cầu phải nộp THÊM | Mức |
|---|---|---|---|
| `SELF` | bản thân | **Không gì ngoài giấy tờ tùy thân đã có trên checklist** | — |
| `GUARDIAN` | người giám hộ | Chứng minh quan hệ với người được bảo hiểm — giấy khai sinh, sổ hộ khẩu hoặc tương đương | **`REQUIRED`** |
| `HEIR` | người thừa kế | Chứng minh quyền thừa kế hoặc chỉ định thụ hưởng | **`REQUIRED`** |
| `AUTHORISED` | người được uỷ quyền | Văn bản ủy quyền | **`REQUIRED`** |

> **⚠ A4 còn NỢ hai loại chứng từ mới** — *Giấy tờ chứng minh mối quan hệ* và *Giấy xác nhận quyền thừa kế / uỷ quyền* — **và quy tắc điều kiện chọn chúng.** **⚠ cần A4 chốt.**

`claimantRelationship` — **8 giá trị**: `FATHER` · `MOTHER` · `SPOUSE` · `CHILD` · `GRANDFATHER` · `GRANDMOTHER` · `SIBLING` · `OTHER`

### 4.23 `causeCode` — 8 mã khách tự chịu

`WAIT` (thời gian chờ) · `NOTCOV` (không thuộc quyền lợi) · `NOTREL` (không liên quan chẩn đoán) · `CAP_VISIT` (vượt trần/lần) · `LIMIT_YEAR` (hết hạn mức năm) · `COPAY` (đồng chi trả) · `DEDUCT` (khấu trừ) · `LIMIT_TOTAL` (vượt trần tổng)

Đây là bộ mã nuôi màn **"Vì sao Quý khách tự chịu"** ở portal.

### 4.24 `fraudSignalType` + `severity`

| `Type` | Ý nghĩa | `Severity` |
|---|---|---|
| `DATE_IMPOSSIBLE` | Ngày sự kiện ở tương lai · vào viện > ra viện · ngày ngoài khoảng điều trị | `High` |
| `HOSPITAL_UNMATCHED` | Không khớp danh mục cơ sở y tế, hoặc quốc gia ≠ VN mà không có quyền lợi nước ngoài | `Medium` |
| `DUPLICATE` | Cùng thành viên + khoảng điều trị chồng lấn · trùng hash chứng từ · trùng số bảng kê | `High` |
| `WINDOW_OVERLAP` | Khoảng điều trị chồng lấn giữa hai hồ sơ | `Medium` |

`Severity`: `Low` · `Medium` · `High`.

### 4.25 `claimQueue` — 4 giá trị

`ASSESSMENT` · `APPROVAL` · `OPERATOR` · `BLOCKED`

### 4.26 `crossCheckRule` — 9 rule ⭐ *(verbatim A4 §5)*

| ID | Rule | So cái gì | Dung sai | Hành động khi trượt |
|---|---|---|---|---|
| **R1** | Tên bệnh nhân nhất quán | `T1-01.6` · `T1-02.4` · `T1-03.5` | **Không phân biệt dấu và hoa thường; khoảng trắng chuẩn hóa** | Cảnh báo trên **mọi** chứng từ |
| **R2** | Ngày sinh nhất quán | `T1-01.7` · `T1-03.6` | **Chính xác** | Cảnh báo |
| **R3** | Cơ sở y tế nhất quán | `T1-01.1/.2` · `T1-02.1` · `T1-03.1` · `T1-04.5/.6` | **Mã số thuế chính xác khi có, nếu không thì khớp tên** | Cảnh báo |
| **R4** | Ngày dịch vụ nằm trong đợt | `T1-01.15/.16` · `T1-02.3` · `T1-03.4` | Cùng ngày lịch hoặc trong khoảng | Cảnh báo |
| **R5** | Số học bảng kê | Σ dòng vs footer | **±1 VND** | Cảnh báo **trên footer** |
| **R6** | Hóa đơn khớp bảng kê | `T1-04.13` vs footer `T1-01`, cho phép phần tách BHYT | **±1 VND** | Cảnh báo. **BỎ QUA KÈM GHI CHÚ khi không có XML** |
| **R7** | Thuốc trên đơn có trên bảng kê | `T1-02 D2` vs `T1-01 L2` khi `L13 = DRUG` | **Khớp tên mờ** | Cảnh báo **nêu tên các thuốc không khớp** |
| **R8** | Chẩn đoán nhất quán | `T1-01.13/.14` · `T1-02.8/.9` · `T1-03.12/.13` | ICD chính xác khi cả hai có; nếu không thì tương tự văn bản | Cảnh báo |
| **R9** | Đầy đủ bộ hồ sơ | Các loại bắt buộc có mặt cho loại đợt | — | **Đặt `packageState`** — §4.17 |

> **⚠ TÁM TRONG CHÍN RULE CHỈ SINH RA CẢNH BÁO.** Không rule nào tự sửa gì. **Nguyên tắc thiết kế, kế thừa từ các hệ tham chiếu và đáng nhắc lại: khi phán quyết của IDP và chứng từ mâu thuẫn, PHÁN QUYẾT CỦA IDP ĐƯỢC DÙNG VÀ MỘT CẢNH BÁO ĐƯỢC NÊU. Không gì được sửa âm thầm và không gì được suy diễn.**

### 4.27 `agentCode` — 6 giá trị

| Mã | Agent | Ghi vào |
|---|---|---|
| `AG-0` | Orchestrator | `AgentRun`, `Claim.State` (**chỉ qua Server Action**) |
| `AG-1` | Intake | `ClaimDocument`, `ClaimLine` (thô), `AgentRun` |
| `AG-2` | Adjudication | **CHỈ `AgentRun`** — chain ghi tiền |
| `AG-3` | Clinical | `ClinicalAssessment`, `ClinicalRecommendation`, `AgentRun` |
| `AG-4` | Fraud-signal | `FraudSignal`, `AgentRun` |
| `AG-5` | Customer-communication | `ClaimLetter`, `AgentRun` |

**Ràng buộc:** **không agent nào có tool ghi được `ClaimLine` tiền, `LimitLedger`, `PaymentOrder` hay `Claim.State` trực tiếp.**

### 4.28 Enum phụ của tầng hợp đồng *(verbatim D2 Part III)*

| Enum | Giá trị | Ghi chú |
|---|---|---|
| `productStatus` | `ACTIVE` · `WITHDRAWN` | **⚠ Tập của D2 vì chưa nguồn nào liệt kê** |
| `productLine` | `HEALTH_INDIVIDUAL` | Một giá trị trong pilot |
| `currency` | `VND` | Nằm ở **product, không ở plan** |
| `policyType` | `INDIVIDUAL` · `GROUP` | |
| `policyStatus` | `ACTIVE` · `CANCELLED` | **Đúng hai. `EXPIRED` KHÔNG tồn tại** — §1.5 |
| `policySource` | `ISSUED_J1` · `IMPORTED` | |
| `memberStatus` | `ACTIVE` · `ENDED` | **CHỈ MÔ TẢ — KHÔNG được vào phép kiểm cover** — §1.7 |
| `relationship` | `SELF` · `SPOUSE` · `CHILD` · `PARENT` · `EMPLOYEE` | Quan hệ với **bên mua bảo hiểm** |
| `renewalStatus` | `LẦN_ĐẦU` · `TÁI_TỤC` · `KHÔNG_XÁC_ĐỊNH` | Mã tiếng Việt verbatim. **`KHÔNG_XÁC_ĐỊNH` là câu trả lời thật** |
| `sex` | `M` · `F` | |
| `limitType` | `annual` · `per_event` · `per_day` | **Mã chữ thường verbatim.** Không lên wire |
| `countUnit` | `DAY` · `VISIT` | |
| `deductionCondition` | `PROVIDER_OFF_NETWORK` | **Một thành viên duy nhất trong MVP 1** |
| `counterType` | `MONEY` · `DAY` · `VISIT` | Sổ cái |

### 4.29 `RequestStatus` (IDP) — 6 giá trị *(verbatim IDP API spec)*

| Giá trị | Text | Nghĩa |
|---:|---|---|
| `0` | `Sending` | Đang đẩy sang lõi OCR |
| `1` | `Processing` | Đang xử lý |
| `2` | `Completed` | **Xong** |
| `3` | `Failed` | Lỗi |
| `4` | `Cancelling` | Đang huỷ |
| `5` | `Cancelled` | Đã huỷ |

**Mặc định của cột là `1 Processing`, không phải `0 Sending`.**

### 4.30 `WorkflowRequestStatus` (IDP) — 7 giá trị *(verbatim IDP API spec)*

| Giá trị | Tên |
|---:|---|
| `1` | `WaitingForInput` |
| `7` | `InQueue` |
| `2` | `Running` |
| `3` | `Finished` |
| `4` | `Error` |
| `5` | `NotFound` |
| `6` | `Unknown` |

> **⚠ Thứ tự khai báo trong source là `1, 7, 2, 3, 4, 5, 6` — `InQueue = 7` nằm GIỮA DÃY.** Giữ đúng thứ tự này khi seed Static Entity.

### 4.31 `Roles` (IDP) — 2 giá trị *(verbatim)*

`1` = `Partner` (Admin) · `2` = `Tenant` (End User). **Tài khoản tích hợp phải là `2`** — §3.1.

---

## 5. Mã lỗi nghiệp vụ

**Hai mô hình lỗi cùng tồn tại trong build này, và đội phải xử lý cả hai.**

### 5.1 Mô hình mục tiêu — RFC 7807 *(D3 §1.5)*

Mọi lỗi từ mọi hệ **ta xây** dùng `application/problem+json`. Đây là thứ `CLM_Core` và `CLM_Console` sinh ra.

| Quy tắc | Chi tiết *(verbatim D3 §1.3)* |
|---|---|
| **Cái gì retry được** | `5xx`, timeout, lỗi kết nối, và `429` |
| **Cái gì KHÔNG** | **`4xx` không bao giờ được retry** — nó nói request sai, và **lặp lại một request sai thì cho ra cùng một câu trả lời sai đồng thời thêm tải cho một bên đã từ chối nó** |
| **⚠⚠ MỘT ngoại lệ `4xx` duy nhất** | **`409 NOT_READY`, và KHÔNG GÌ KHÁC.** Nó là status **duy nhất** trong đặc tả nơi `4xx` không nghĩa là *request của bạn sai* mà là *câu trả lời CHƯA TỒN TẠI*. `I-09` trả nó khi trích xuất còn chạy, `I-16` khi đánh giá lâm sàng còn chạy. **NÓ PHẢI ĐƯỢC RETRY**, tôn trọng `Retry-After` thay cho backoff — **một adapter áp quy tắc chung ở đây sẽ BỎ RƠI MỌI phép trích xuất và MỌI phép đánh giá không sẵn sàng tức thì** |
| **Phân biệt với `409` idempotency** | **Bằng `type` của problem, KHÔNG phải bằng status code** |
| **Backoff** | 1s, 2s, 4s, 8s, 16s — **năm lần thử lại, sáu lần gọi tổng cộng** — rồi dừng và nêu cảnh báo vận hành |
| **Timeout** | Connect 5s, read 30s, **trừ `I-07` (nộp bộ chứng từ) cho phép read 60s** cho upload lớn |
| **At-least-once** | **Mọi webhook có thể tới NHIỀU HƠN MỘT LẦN.** Handler idempotent trên **khóa nghiệp vụ**, không phải trên id của lần giao |

**Mã lỗi nghiệp vụ của hợp đồng D3** *(verbatim)*:

| Mã | Giao diện | Nghĩa |
|---|---|---|
| `409 NOT_READY` | `I-09`, `I-16` | **Retry được** — kèm `Retry-After` |
| `409 CONFLICT` | mọi `POST` | Cùng `Idempotency-Key`, khác body. **Bug của client, phải TO TIẾNG** |
| `422 PACKAGE_TOO_LARGE` | `I-07` | **Tối đa 20 chứng từ, 50 trang, 100 MB tổng.** Response **kèm con số thực tế** |
| `422` unfetchable URI · hash mismatch · unknown `templateSetCode` | `I-07` | |
| `422` unknown `serviceCategory` · unknown `knowledgeBaseVersion` · no lines | `I-15` | |
| `422 ACCOUNT_NOT_VERIFIED` | `I-11` | **Lệnh chi trả vào tài khoản chưa xác minh** |
| `422 NO_SCHEDULE_AT_DATE` | `I-05` | **⚠ Mock chưa exercise được vì seed thiếu `effective_from`/`effective_to`** |
| `503` | mọi | Bên kia không sẵn sàng |

### 5.2 Mô hình thật của IDP staging — `API_CODE`

> **⚠⚠ HTTP status của IDP LUÔN LÀ 400; `statusCode` trong BODY mới là mã nghiệp vụ.** Client kiểm `body.statusCode`, **KHÔNG** kiểm HTTP status.

| Nhóm | Mã | Tên | Message |
|---|---:|---|---|
| OCR Request | **1000** | `UNKNOWN_ERROR` | Ocr a document has failed |
| | **1001** | `INVADLID_TENANT_CODE` *(sic)* | Invalid tenant code! |
| | **1002** | `OVER_LIMIT_PAGE_PER_REQUEST` | Error! Too many pages detected. |
| | **1003** | `INVADLID_QUOTA_CODE` *(sic)* | Invalid request quota! |
| | **1004** | `TENANT_EXPIRED_CODE` | Tenant is expired |
| | **1005** | `UPLOAD_FAIL` | Upload failed! |
| | **1006** | `NOT_FOUND` | Not found! |
| | **1007** | `INVALID_FILE_TYPE` | Invalid excel file type |
| | **1008** | `TENANT_INACTIVE_CODE` | Tenant is inactive |
| | **1009–1013** | *(chỉ message)* | Invalid request parameter · Unauthorized access · Request timeout · Conflict detected · Resource already exists |
| | **1014** | `PARTNER_EXPIRED_CODE` | Partner is expired |
| | **1015** | `PARTNER_DEACTIVE` | Partner is deactivated! |
| | **1016** | `ASYNC_REQUEST_STATUS_NOT_FOUND` | **⚠ THIẾU message** → `undefined` |
| | **1017** | `ASYNC_REQUEST_STATUS_RUNNING` | **⚠ THIẾU message → `undefined`. ⚠⚠ TRẢ HTTP 200 — §5.3** |
| | **1018** | `ASYNC_RESULT_BAD` | **⚠ THIẾU message** |
| | **1019** | `ASYNC_REQUEST_STATUS_NOT_RUNNING` | Request is not running! |
| | **1020** | `REQUEST_GROUP_NOT_FOUND` | Request group not found. |
| | **16** | `REQUESR_CANCEL_COMPLETED` *(sic — thiếu "T", và giá trị **16** không theo dải 10xx)* | — |
| | **1021** | `REQUEST_RETRY_FAILED` | **⚠ THIẾU message** |
| OCR / DU | **1101** / **1201** | `OCR_FAIL` / `DU_FAIL` | OCR failed! / DU failed! |
| Setting | **2002** / **2003** | `INVALID_TENANT_CURRENT_MONTH_PACKAGE` / `_BLOCK` | Current month package/block must be greater or equal this one! |
| User/Tenant | **3000** | `USER_EMAIL_EXIST` | Email address already exists in the system! |
| | **3001** | `TENANT_NAME_EXIST` | The department already exists in the system! |
| | **3002** | `CAN_NOT_DELETE_USER` | Cannot delete this user! |
| Workflow | **4000** | `WORKFLOW_NOT_FOUND` | The workflow not found. |
| | **4001** | `WORKFLOW_OCRRESULT_NOT_READY_YET` | The OCRResults haven't been completed. Please check again. |
| | **4002** | `WORKFLOW_REQUEST_NOT_READY` | Workflow request is not ready to run. |
| | **4003** | `WORKFLOW_UNABLE_TO_CREATE_FOLDER` | Unable to create folder. |
| | **4004** | `WORKFLOW_FOUND_NOT_FOUND` *(sic — "FOUND" thay vì "FOLDER")* | Folder not found. |
| | **4005** | `WORKFLOW_IS_RUNNING` | Workflow is running, please wait. |
| | **4006** | *(chỉ message)* | Something is wrong, unable to export workflows |
| | **4007** | `WORKFLOW_BAD_RESULT` | Workflow result is bad, please check again. |
| | **4008** | `WORKFLOW_BAD_RESULT_BAD_OUTPUT_COMPONENT` | Workflow last component must be an JSON output component. |
| | **4009** | `WORKFLOW_COMMON_EXCEPTION` | **⚠ THIẾU message** |
| Database | **5000–5003** | `ENTITY_NOT_FOUND` / `_FAILED_TO_CREATE` / `_UPDATE` / `_DELETE` | Entity not found · Failed to create/update/delete entity. |
| Tenant delete | **5004–5008** | `TENANT_FAILED_TO_DELETE*` | Failed to delete tenant / user / template / invoice / usage group. |
| Metadata | **7000** | `METADATA_COMMON_EXCEPTION` | Metadata request failed. |
| | **7001** | `METADATA_DELETE_PRIMARY_COLUMN_EXCEPTION` | Primary column cannot be deleted. |

**Hai mã kèm `data` — phải đọc thêm trường:**
- `1003` → body `{ remainPageCount, pageCount, statusCode: 1003, message: 'Invalid request quota!' }`
- `1002` → kèm số trang thực tế · `4007` → `{ detail }`

### 5.3 ⚠⚠ Mã 1017 trả HTTP 200 — bẫy nguy hiểm nhất

`ASYNC_REQUEST_STATUS_RUNNING = 1017` là trạng thái *"đang chạy, chưa có kết quả"*. Nó **không đi qua đường ném exception**: nó về trong **body của một response HTTP 200 thành công**.

| Nếu ODC làm thế này | Chuyện gì xảy ra |
|---|---|
| `if (httpStatus == 200) → coi là có kết quả` | **SAI.** Hồ sơ 1017 được coi là đã xong, `RawExtractionJson` lưu một body rỗng, chain chạy trên **0 dòng**, cổng trượt vì "thiếu chứng từ" — **và không ai biết nguyên nhân thật là poll quá sớm** |
| `if (body.statusCode == 1017) → tiếp tục poll` | **ĐÚNG** |

**Quy tắc bắt buộc cho `CLM_IDP_Int`:** mọi wrapper REST **luôn đọc `body.statusCode` TRƯỚC, bất kể HTTP status là gì.**

| Điều kiện | `IdpOutcome` | Trạng thái hồ sơ |
|---|---|---|
| HTTP 200 + `statusCode` vắng + có `result` | `Success` | tiếp tục |
| HTTP 200 + `statusCode == 1017` | `Running` | **tiếp tục poll, KHÔNG ghi gì** |
| `statusCode ∈ {1016, 1018, 1101, 1201, 4007, …}` | `Error(code, message)` | `EXTRACTION_ERROR` / `ClinicalAssessment.ERROR` |
| Timeout · 4xx · 5xx · `result: {}` | `Error` | **`EXTRACTION_ERROR`, TUYỆT ĐỐI KHÔNG BAO GIỜ `DECLINED`** |

**⚠ `message` có thể là `undefined`** với 5 mã (1016, 1017, 1018, 1021, 4009). Màn hình vận hành **phải hiển thị MÃ SỐ khi `message` rỗng**, không hiển thị chuỗi rỗng.

> **⚠ `403 · error code: 1010` KHÔNG phải mã của ứng dụng** — đó là Cloudflare chặn user-agent mặc định. Thiếu header `User-Agent` là gặp nó. §3.

---

## 6. Quy ước kiểu và độ rộng field

### 6.1 Bảng kiểu *(verbatim D2 §2.3, cột ODC do build này thêm)*

| Kiểu D2 | Ý nghĩa | Ràng buộc | Kiểu ODC |
|---|---|---|---|
| `text(n)` | Văn bản tự do, tối đa n ký tự | **Trim khi lưu; khoảng trắng bên trong GỘP LẠI** | `Text` + Length = n |
| `textarea(n)` | Văn bản nhiều dòng | **GIỮ NGUYÊN ký tự xuống dòng** | `Text` + Length = n |
| `int` | Số nguyên | Không phân cách nghìn khi nhập; nhóm khi hiển thị | `Integer` / `Long Integer` |
| `money` | Đồng Việt Nam | **Số nguyên đồng. KHÔNG thập phân.** Hiển thị nhóm 3 chữ số: `1.920.000`. Làm tròn: **tới đồng nguyên, nửa làm tròn RA XA 0, TẠI MỖI BƯỚC của chain** | `Currency` |
| `pct` | Phần trăm | Nhập một chữ số thập phân; **lưu dạng BASIS POINT số nguyên**. `20%` lưu là `2000` | `Integer` |
| `date` | Ngày lịch | Hiển thị `dd/mm/yyyy`. Lưu ISO-8601 date. **KHÔNG có phần giờ** | `Date` |
| `datetime` | Thời điểm | Hiển thị `dd/mm/yyyy HH:mm`. Lưu ISO-8601 kèm offset, **`Asia/Ho_Chi_Minh (+07:00)`** | `Date Time` |
| `enum(X)` | Một trong tập cố định | Tập có tên và có **chủ sở hữu tài liệu** | `Text(20)` + Static Entity |
| `lookup(X)` | Tham chiếu bản ghi | Hiển thị nhãn người đọc được, gửi đi bằng mã hoặc id | `Long Integer` / `Text` + FK |
| `bool` | Có / không | **KHÔNG BAO GIỜ là tri-state nullable** | `Boolean` |
| `file` | Tài liệu / ảnh | | `Binary Data` |
| `derived` | Tính ra, không bao giờ nhập | **Công thức hoặc nguồn phải ghi NGAY TRÊN DÒNG** | — (Server Action) |

> **⚠⚠ `money` KHÔNG BAO GIỜ nhận dấu thập phân, `pct` KHÔNG BAO GIỜ lưu số thực.** Cả hai được chọn như vậy vì **một chênh lệch làm tròn giữa hai cách hiện thực cùng một chain là VÔ HÌNH cho tới khi nó tới tay khách hàng.**
>
> **⚠ Trên wire:** tiền là **số nguyên VND. Không thập phân, không dấu phân cách, không dấu phẩy động, BAO GIỜ.** Một payload mang `1500000.0` **bị từ chối là dị dạng**. **Adapter chuyển đổi** bất cứ thứ gì InsureMO trả về — số thập phân, chuỗi, hay số nguyên đơn vị nhỏ — và **một phép chuyển đổi làm mất độ chính xác thì FAIL chứ không LÀM TRÒN.**
>
> **⚠ Ngày là ngày lịch, KHÔNG có timezone.** *(D3 §1.2)* **Thêm timezone vào một ngày dịch vụ sinh ra bug lệch một ngày đúng tại biên kỳ hạn hợp đồng.** Ngược lại, **một timestamp KHÔNG có offset thì BỊ TỪ CHỐI.**
>
> **⚠ `null` nghĩa là KHÔNG BIẾT. Chuỗi rỗng nghĩa là BIẾT LÀ RỖNG. Chúng KHÁC NHAU và một payload KHÔNG BAO GIỜ được thay cái này bằng cái kia.** A4 §2 đi xa hơn cho trích xuất: **một giá trị thiếu được ghi là `null`, KHÔNG BAO GIỜ là chuỗi rỗng, và KHÔNG BAO GIỜ được đoán.**

### 6.2 Độ rộng chuẩn ⭐ *(verbatim D2 §2.9)*

**Cùng một giá trị được giữ trên nhiều màn hình và trong nhiều bản ghi. Các độ rộng này được CỐ ĐỊNH ở đây để không màn hình nào cắt cụt thứ mà một màn hình khác lưu** — một lỗi chỉ lộ ra trên một khách hàng có tên dài.

| Giá trị | Độ rộng | Ghi chú |
|---|---|---|
| Số hồ sơ bồi thường | `text(20)` | Format `CLM-YY-NNNNNN` + hậu tố `/n` tùy chọn — **15 ký tự. Biên dư là để hậu tố dài quá một chữ số** |
| Số hợp đồng | `text(20)` | |
| Số thẻ thành viên | `text(20)` | |
| **Họ tên đầy đủ của một người** | **`text(200)`** | Tên tiếng Việt đủ dấu |
| **Tên cơ sở y tế** | **`text(200)`** | |
| Số giấy tờ tùy thân | `text(20)` | |
| Số tài khoản ngân hàng | `text(30)` | |
| **Lý do / diễn giải tự do do NHÂN VIÊN nhập** | **`textarea(2000)`** | Lý do từ chối, lý do ghi đè, lý do miễn yêu cầu, lý do bác bỏ. *Ghi chú của khách hàng là thứ khác và hẹp hơn* |
| **Mọi MÃ** (quyền lợi, lý do từ chối, loại chứng từ, ICD-10) | **`text(20)`** | |

> **⚠⚠ Một SNAPSHOT KHÔNG BAO GIỜ thu hẹp field mà nó sao chép.** Tên ở `text(200)` thì bản snapshot của nó cũng phải `text(200)` — bất cứ thứ gì khác đều **âm thầm mất dữ liệu ĐÚNG VÀO KHOẢNH KHẮC hồ sơ được đăng ký, tức là khoảnh khắc KHÔNG AI ĐANG NHÌN.**
>
> **⚠ Đây giờ cũng là thứ ta YÊU CẦU ở response của InsureMO.** Một danh bộ giữ tên ở 100 ký tự thì **cắt cụt nó ở thượng nguồn, và không độ rộng nào ta đặc tả ở hạ nguồn phục hồi được** — phép kiểm thuộc về sandbox, **quyền lợi theo quyền lợi và field theo field.**

**Các độ rộng khác đã dùng trong tài liệu này** *(verbatim D2 Part III)*: `text(16)` mã sản phẩm / mã plan / mã quyền lợi · `text(10)` mã ngân hàng và số điện thoại · `text(140)` tên chủ tài khoản ngân hàng · `text(160)` email liên hệ · `text(120)` tên sản phẩm và tên quyền lợi · `text(60)` tên plan · **`text(8)` `copay_pct`** — §1.3.

### 6.3 Quy ước trích xuất *(verbatim A4 §2)*

| Quy ước | Quy tắc |
|---|---|
| **Trạng thái field** | `M` bắt buộc — **trích xuất THẤT BẠI cho chứng từ đó nếu vắng** · `O` tùy chọn · `D` IDP suy ra, không đọc từ trang giấy |
| **Giá trị thiếu** | Ghi là `null`, **không bao giờ là chuỗi rỗng và không bao giờ đoán.** Một field bắt buộc vắng mặt **nêu một cảnh báo đầy đủ NÊU TÊN FIELD** |
| **KHÔNG BAO GIỜ SUY DIỄN** | **IDP không tính một giá trị không có trên trang giấy.** Tổng được **ĐỌC**, rồi được đối chiếu với tổng cộng — **một chỗ lệch là một CẢNH BÁO, không phải một PHÉP SỬA** |
| **Ngày** | Chuẩn hóa về `YYYY-MM-DD`. Dạng nguồn chấp nhận: `dd/mm/yyyy`, `dd-mm-yyyy`, `ngày DD tháng MM năm YYYY` |
| **Số tiền** | Số nguyên VND, không dấu phân cách, không thập phân. **⚠ Nguồn tiếng Việt dùng `.` làm dấu phân cách nghìn — `1.500.000` là 1500000, KHÔNG BAO GIỜ là 1.5** |
| **Văn bản** | **Giữ dấu tiếng Việt CHÍNH XÁC như đã in. KHÔNG chuẩn hóa hoa thường** |
| **Confidence** | Mọi field trích xuất mang confidence trong `[0,1]`. **Ô bảng mang confidence riêng; DÒNG mang MIN các ô của nó** |
| **Provenance** | Mọi field mang `documentId`, số trang và **bounding box**, để UI xem lại nhảy tới được |
| **Thô là BẤT BIẾN** | **Kết quả trích xuất KHÔNG BAO GIỜ bị một phép hiệu chỉnh ghi đè. Hiệu chỉnh sống trên hồ sơ; cặp đôi giữ nguyên tính so sánh được** |

### 6.4 Quy ước giao diện *(verbatim D3 §1.2)*

| Quy ước | Quy tắc |
|---|---|
| **Correlation** | Mọi request mang `X-Correlation-Id` (UUIDv4). Vắng thì bên nhận sinh ra và trả lại. **Lan truyền qua mọi lời gọi hạ nguồn và ghi vào MỌI dòng log** |
| **Xác thực webhook** | **HMAC-SHA256** trên **raw body**, trong `X-Signature`, kèm `X-Timestamp`. **Request cũ hơn 5 phút bị từ chối.** **Chữ ký xác minh trên raw bytes TRƯỚC KHI parse JSON — parse một payload chưa xác minh MỚI là lỗ hổng, không phải việc nhận nó** |
| **Endpoint webhook** | **Là một đường ghi công khai, mặc định không xác thực, vào hệ hồ sơ. Đối xử với nó như ĐẦU VÀO THÙ ĐỊCH**: chữ ký trước, cửa sổ thời gian sau, parse thứ ba, và handler **không làm gì mà một lần phát lại không hoàn tác được** |
| **Idempotency** | Bên nhận **lưu khóa cùng response trong 24 GIỜ** và trả response đã lưu, **không đổi**, khi gặp lại |

---

## 7. Quan hệ và ràng buộc

### 7.1 Sơ đồ quan hệ

```
════════════════ InsureMO (mock trong hackathon) ════════════════
Product ──1:N──► Plan ──1:N──► BenefitScheduleLine
                                  │ ParentBenefitCode (tự trỏ, CÙNG PlanCode)
                                  └──1:N──► BenefitWaitingPeriod
                                               (PlanCode, BenefitCode, ConditionClass)
Policy ──1:N──► BenefitLevel ──N:1──► Plan
   └──1:N──► InsuredMember ──N:1──► BenefitLevel

════════════════ Claim Platform ════════════════
InsuredMember ◄──(tham chiếu, KHÔNG phải FK)── PayoutAccount ◄──1:1── Beneficiary
InsuredMember ◄──(tham chiếu)── Claimant ──1:1── Claim

                        ┌────────────┐
                        │   Claim    │  ClaimNo = correlation id
                        │  LossType  │──derive──► EpisodeType (CHỈ ĐỌC)
                        │   State    │
                        │ RuleSetRef │  ghim TẠI ĐĂNG KÝ
                        └─────┬──────┘
        ┌──────┬───────┬──────┼───────┬────────┬─────────┬────────┐
        ▼      ▼       ▼      ▼       ▼        ▼         ▼        ▼
 InsuredEvent  ClaimDocument  ChainRun  FraudSignal  CrossCheck  ClaimEvent
        │            │           │                                AgentRun
        │            │           │                                ClaimLetter
        │            ▼           ▼                                PaymentOrder
        │       ClaimLine ──1:N──► ClaimLineBreakdown ──N:1──► ChainRun
        │            │
        │            └──1:1──► ClinicalRecommendation ──N:1──► ClinicalAssessment
        │
        └── EventNo ◄── ClaimLine.EventNo  (phân bổ theo V-705)

ChainRun ──1:N──► LimitLedger  (PolicyNo, MemberId, BenefitCode, PeriodKey, CounterType)
DocumentChecklistRule: (LossType | ClaimantStanding | TermRef) ──► DocumentType + RequirementLevel

════════════════ Cầu nối sang IDP ════════════════
Claim.IdpRequestGroupId ──────────► (nhóm request)
Claim.IdpWorkflowRequestId ───────► IdpWorkflowRequest
ClaimDocument.IdpRequestId ───────► IdpRequest
ClaimDocument.IdpTemplateId ──────► Template
ClinicalAssessment.AssessmentId ──► (I-16)
Template.MapsToDocumentType ──────► DocumentType (crosswalk, §7.3)
```

**Sáu cầu nối là TOÀN BỘ bề mặt tiếp xúc giữa tầng claim và tầng IDP. Mọi thứ khác không được đi thẳng.**

### 7.2 Ràng buộc PHẢI TỰ THÊM

| # | Ràng buộc | Vì sao | ODC làm thế nào |
|---:|---|---|---|
| **1** | `BenefitScheduleLine.ParentBenefitCode` **phải trỏ tới dòng CÙNG `PlanCode`** | Một con trỏ cha vượt plan là một cây quyền lợi lai giữa hai gói | Check constraint trong `PostBenefitSnapshot` |
| **2** | `CountLimit = 0` **phải được diễn dịch là KHÔNG GIỚI HẠN** | §1.3 | Hằng số có tên `COUNT_UNLIMITED = 0`, **không bao giờ so sánh trần trực tiếp** |
| **3** | `CopayPct = 'INHERIT'` **phải giải tại snapshot; chạy tới gốc không gặp số nguyên = FAIL** | §1.3 | Fail **tại biên adapter**, không phải trong chain |
| **4** | `BenefitLevel.EffectiveFrom/To` **phải nằm trong kỳ hạn hợp đồng** | | Validate lúc seed |
| **5** | **`InsuredMember.Status` KHÔNG ĐƯỢC vào phép kiểm cover** | §1.7 — lọc `ACTIVE` là từ chối mọi hồ sơ muộn của người rời đi | Phép kiểm cover là một Server Action riêng, **code review bắt buộc** |
| **6** | `InsuredMember.CardNo` **unique toàn danh bộ** | Khóa duy nhất tìm ra người phụ thuộc | Unique index. **Trùng thì TỪ CHỐI ĐOÁN — hiện đường đa-kết-quả** |
| **7** | `Claimant.Relationship` **bắt buộc khi `Standing ≠ SELF`; PHẢI RỖNG khi `= SELF`** | | Validate hai chiều |
| **8** | `Claimant.FullName` và `IdNumber` **KHÓA vào thành viên khi `Standing = SELF`** | | Read-only trên UI **và** chặn ở server |
| **9** | `Claimant.ClaimNo` **unique** — một người yêu cầu mỗi hồ sơ | Làm cho thông tin liên hệ **an toàn với snapshot theo cấu tạo** | Primary key |
| **10** | **Σ `ClaimLineBreakdown.Amount` = `ClaimLine.CustomerBorneAmount`** theo từng `ChainRunId` | Lệch là bug chain | **Chặn `SubmitForApproval`** |
| **11** | **`LimitLedger.After = Before − Consumed`** | | Kiểm trong `PostLimitLedger` + **test đơn vị** |
| **12** | **`PaymentOrder.Amount = ChainRun.TotalPayable`** của lần chạy có `InputsHash` khớp | | Chặn `IssuePaymentOrder` |
| **13** | **`PaymentOrder` yêu cầu `PayoutAccount.Verified = true`** | | `422 ACCOUNT_NOT_VERIFIED` |
| **14** | **`ApproverId ≠ Claim.AssignedTo`** — bốn mắt | | Chặn `ApproveClaim` |
| **15** | **`ClinicalRecommendation.EvidenceJson` KHÔNG ĐƯỢC rỗng** | Bằng chứng rỗng là response **dị dạng**, không phải confidence thấp | `ProposeRecommendation` từ chối ghi |
| **16** | **`ProposedReasonCode ∈ {NOTREL, NOTCOV}`** | Mọi mã khác là câu hỏi chính sách thuộc Track 1 | **Từ chối TẠI ADAPTER**, không để tới panel |
| **17** | **`ClaimLine.LineDecision` bắt buộc trên MỌI dòng trước khi trình** | | Checklist tiền-trình-duyệt |
| **18** | **`DeclineReasonCode` bắt buộc khi `LineDecision = TỪ_CHỐI`**, và chỉ trong bộ đóng 7 mã | | Static Entity + FK |
| **19** | **`NOTREL`/`NOTCOV` KHÔNG được chào ở cấp sự kiện** | Chúng chỉ tồn tại ở cấp dòng | Lọc tập giá trị theo màn hình |
| **20** | **`ClaimDocument.FileHash` unique theo `ClaimId`** | Chống nộp trùng | Unique index |
| **21** | **`RawExtractionJson` append-only** | Kết quả cho một `requestId` không bao giờ đổi | **Không tạo Server Action Update/Delete** |
| **22** | **`AgentRun` và `ClaimEvent` append-only** | | Idem |
| **23** | **`Claim.State` chỉ do `TransitionClaimState` ghi** | | Entity để `Private`; chỉ Server Action đó public |
| **24** | **`Claim.InNetwork` ghim tại đăng ký, không đọc lại** | §2.1 | Copy vào snapshot; **không join live tới danh mục** |
| **25** | **`Claim.RuleSetRef` + `ChainRun.TermRegisterVersion` ghim tại đăng ký** | Điều khoản sửa sau không nhiễu hồ sơ | Snapshot |
| **26** | **`ChainRun.ThresholdSnapshotJson` ghim tại mỗi lần chạy** | Chạy lại không được chấm theo ngưỡng chưa tồn tại lúc đăng ký | Snapshot |
| **27** | **`Template.MapsToDocumentType` là DỮ LIỆU, không phải hằng số trong code** | §7.3 rule X-3 | Entity + màn admin |
| **28** | **`ClaimLine.EventNo` phân bổ nhầm thì GẮN CỜ, không đoán** | | Trạng thái `UNALLOCATED` riêng |
| **29** | **`InsuredEvent.TreatmentDays` = `null` khi `TreatmentTo` vắng** | §8.2 — mặc định thành 1 ngày là mất tiền | Không có giá trị mặc định |
| **30** | **`ClaimLine.DocumentDate` = `null` khi không in, KHÔNG mặc định thành ngày header** | Một giá trị suy ra không được trông giống một giá trị đọc được | Không có giá trị mặc định |

### 7.3 Crosswalk phân loại — 5 rule làm cho nó là HỢP ĐỒNG chứ không phải một bảng tra ⭐

**IDP trả về một giá trị `classification` từ TỪ VỰNG CỦA CHÍNH NÓ**, không phải của ta và **cũng không cần phải là của ta** — một nền tảng document intelligence phân loại chứng từ cho mọi khách hàng nó có, không phải cho ta.

> **Sự vắng mặt này KHÔNG phải lý thuyết.** Payload kết quả mang `classification`, catalogue nêu tên bảy loại, và **không gì trong bộ tài liệu nói một cái trở thành cái kia như thế nào.** Phép dịch đó **sắp được viết, một lần, bởi bất cứ ai dựng adapter, từ một cuộc hội thoại không ai ghi lại** — và **lần đầu tiên một lớp của đối tác bị đổi tên, không ai biết cái nào trong bảy loại của ta đã ngừng tới.**

| # | Rule *(verbatim)* |
|---|---|
| **X-1** | **Ánh xạ là NHIỀU-VỀ-MỘT, không bao giờ một-về-nhiều theo chiều của ta.** Nhiều lớp của đối tác có thể giải về một loại của ta; **một lớp của đối tác KHÔNG BAO GIỜ được giải về hai loại của ta**, vì khi đó checklist sẽ không biết mục nào đã được thỏa mãn |
| **X-2** | **Một lớp của đối tác KHÔNG có trên bảng thì giải về `unclassified`**, được đưa ra cho thẩm định viên, và **KHÔNG BAO GIỜ về loại trông giống nhất. Một chứng từ bị âm thầm xếp sai loại sẽ thỏa mãn một mục checklist mà nó KHÔNG thỏa mãn, và một hồ sơ khi đó quyết toán trên bằng chứng không có ở đó** |
| **X-3** | **Ánh xạ là DỮ LIỆU ĐƯỢC BẢO TRÌ, KHÔNG PHẢI CODE** — nó sống cùng catalogue loại chứng từ và **đổi không cần release. Đối tác SẼ đổi tên và thêm lớp và SẼ KHÔNG BÁO TRƯỚC**, và đó chính xác là lý do nó không được là một hằng số trong một adapter |
| **X-4** | **Giá trị THÔ của đối tác được GIỮ trên chứng từ bên cạnh loại của ta.** Khi một ánh xạ về sau bị phát hiện sai, các chứng từ đã xử lý **ánh xạ lại được từ giá trị đã lưu**; không có nó thì phải **phân loại lại**, tức là **xử lý lại** |
| **X-5** | **Một thay đổi trên bảng này là một CHANGE REQUEST**, ngang hàng với việc thêm một loại chứng từ — vì nó **đổi chứng từ nào thỏa mãn mục checklist nào, và do đó hồ sơ nào đi tiếp được** |

**Cardinality dự kiến** *(verbatim A4 §3.1)*: `T1-01` **một-về-nhiều** (bảng kê, bảng kê chi tiết và bảng kê nhà thuốc có thể là ba lớp với đối tác và một loại với ta) · `T1-02` một-về-nhiều (đơn thuốc và phiếu lĩnh thuốc thường là hai lớp riêng) · `T1-03` một-về-nhiều · **`T1-04` một-về-một** (XML, đường đi là schema validation chứ không phải phân loại) · `T2-01` một-về-nhiều (CCCD, hộ chiếu và giấy khai sinh gần như chắc chắn là ba lớp của đối tác và một loại ở đây) · `T2-02` một-về-nhiều · `T2-03` một-về-nhiều · **`T2-04` một-về-nhiều, và là dòng ít có khả năng có câu trả lời nhất** — *đây không phải một chứng từ y tế và từ vựng y khoa của đối tác có thể không với tới nó chút nào.*

> **⚠⚠ CỘT BÊN PHẢI ĐANG RỖNG và đó là tình trạng trung thực của nó. CHỈ đội IDP mới điền được — đó là từ vựng của họ. Cho tới khi nó được điền, `classification` đang được tiêu thụ TRÊN MỘT ÁNH XẠ GIẢ ĐỊNH, và một ánh xạ giả định là MỘT LỖI ĐANG CHỜ LẦN ĐỔI TÊN ĐẦU TIÊN CỦA NÓ.** **⚠ cần hỏi IDP — ưu tiên cao nhất.**

### 7.4 Quy tắc xung đột phân loại và tách trang *(verbatim A4 §3)*

- **Khi hai signature cùng đạt, cái có tỷ lệ khớp CAO HƠN thắng, và lớp cạnh tranh được GHI LẠI trên chứng từ.**
- **Ca nhập nhằng đã biết: một bảng kê và một ảnh hóa đơn trên CÙNG MỘT TRANG** — giải theo hướng **bảng kê**, vì hóa đơn không được đọc từ ảnh chút nào trong MVP 1.
- **Một file tải lên có thể chứa nhiều chứng từ.** IDP tách theo ranh giới chứng từ **trước khi** phân loại. **Một bảng kê nhiều trang vẫn là MỘT chứng từ; các dòng của nó nối lại theo thứ tự trang.**
- **Một trang không khớp gì cả rơi vào `unclassified` và được đưa ra cho thẩm định viên — nó KHÔNG BAO GIỜ bị ép vào lớp gần nhất.**

### 7.5 Ràng buộc append-only phải cài trong ODC

| Entity / Attribute | Cách cài |
|---|---|
| `AgentRun` | **Không tạo Server Action Update/Delete.** Chỉ `WriteAgentRun` (Create) + các Read |
| `ClaimEvent` | Idem |
| `ClaimDocument.RawExtractionJson` | `StoreClaimDocument` ghi một lần; **mọi action update khác PHẢI LOẠI attribute này khỏi record được ghi** |
| `LimitLedger` | Không update; đảo bằng dòng mới `Reversed = true` + `ReversalOfId` |
| `ClaimLine.OriginalValueJson` | Ghi lúc tạo; `EditClaimLine` **chỉ ghi `EditedValueJson`** |
| `ClaimLetter.SentText` | Ghi một lần lúc gửi |
| `ClinicalRecommendation.RationaleVi` | Bản AI soạn giữ ở `DeclineRationaleOriginal` của dòng; **bản sửa ghi riêng** |

---

## 8. Bẫy đã biết

> **Đọc hết mục này trước khi viết dòng ODC đầu tiên.** Mỗi mục là một lỗi đã được nêu tên trong nguồn, không phải rủi ro lý thuyết.

### 8.1 ⚠⚠ Ba sentinel hợp lệ về số học — hỏng âm thầm

Đã nêu ở §1.3. Nhắc lại vì đây là **thứ sắc nhất** trong toàn bộ tài liệu:

| Sentinel | Đọc sai | Hậu quả |
|---|---|---|
| `CountLimit = 0` | coi là trần số | **Từ chối mọi hồ sơ** trên phẫu thuật, khám và thuốc — phần lớn hồ sơ |
| `CopayPct = 'INHERIT'` | ép thành số → 0 | **Âm thầm trả đủ** một quyền lợi đồng chi trả 20% |
| `waitingPeriodDays` vắng | coi là 0 ngày | **Che một lỗi dữ liệu thành một quyền lợi hợp lệ** |

**Cả ba đều hợp lệ về số học nếu bị đọc sai, và do đó FAIL ÂM THẦM chứ không FAIL TO TIẾNG.**

### 8.2 ⚠⚠ `TreatmentTo` KHÔNG ĐƯỢC mặc định — đây là bẫy có thể mất tiền

A4 v1.2 từng viết *"Mặc định bằng field 15 cho bảng kê một lượt khám"*. **A4 v1.3 phát biểu điều NGƯỢC LẠI và nêu rõ vì sao.**

**Chuỗi hỏng, cụ thể:**
1. `TreatmentTo` không đọc được trên một đợt **nội trú**
2. Mặc định thành `TreatmentFrom` ⇒ số ngày điều trị **= 1**
3. Bước chain kiểm số ngày đó với hạn mức ngày của quyền lợi ⇒ **một đợt nằm viện 14 đêm được định giá như MỘT ngày**
4. Hạn mức ngày **vượt qua một cách tầm thường**
5. **KHÔNG GÌ Ở BẤT KỲ ĐÂU BÁO RẰNG MỘT CON SỐ ĐÃ ĐƯỢC CHẾ TẠO RA**

> **Một giá trị mặc định KHÔNG PHÂN BIỆT ĐƯỢC với một giá trị đã đọc, một khi nó đã nằm trong payload.**

**Từ v1.3: `null` khi vắng, provenance ghi là VẮNG MẶT, và ENGINE quyết định làm gì với nó** — mà với một đợt nội trú là **GIỮ bước đó lại chứ không cho qua**.

**Ca một-lượt-khám vốn là động cơ của cái mặc định thì được xử lý ở đúng chỗ của nó**: một bảng kê có một ngày điều trị và một dòng là một sự kiện một ngày, và **đó là phép suy diễn CỦA ENGINE để làm và để GHI LẠI NHƯ MỘT PHÉP SUY DIỄN**, không phải của trích xuất để làm và ghi lại như một phép đọc.

**Cùng một luật áp cho `ClaimLine.DocumentDate` (L15) và `ClaimLine.TreatmentDaysLine` (L18).**

### 8.3 ⚠ `FacilityCountry`: `VN` KHÔNG phải mặc định, và CẤM đoán từ tên

Loại trừ *điều trị ngoài lãnh thổ Việt Nam* chọn **duy nhất trên field này**. `ProviderName` là địa chỉ tự do và **tùy chọn**, nên hôm nay **không có sự kiện nào đứng sau loại trừ đó cả**.

> **⚠ Thứ RÕ RÀNG KHÔNG ĐƯỢC PHÉP là đoán quốc gia từ TÊN cơ sở** — đó chính là thứ một người hiện thực với tay tới khi cần một fallback. A4 §2 *never infer* cấm nó, **và một phép đoán dựa trên tên sẽ nêu một checkpoint trên MỌI cơ sở có tên đọc nghe giống nước ngoài.**

**Một quốc gia không được nêu là `null`.** **⚠ cần hỏi IDP.**

### 8.4 ⚠⚠ `1017` trả HTTP 200

Xem §5.3. **Bẫy nguy hiểm nhất ở tầng tích hợp.**

### 8.5 ⚠⚠ `409 NOT_READY` là `4xx` DUY NHẤT được retry

Xem §5.1. **Một adapter áp quy tắc "không bao giờ retry 4xx" ở đây sẽ BỎ RƠI MỌI phép trích xuất và MỌI phép đánh giá lâm sàng không sẵn sàng tức thì.** Phân biệt với `409 CONFLICT` **bằng `type` của problem, KHÔNG phải bằng status code**.

### 8.6 ⚠ `templates` là CHUỖI CSV, không phải JSON array

*(verbatim IDP API spec)*
```
"templates": "5445,5444,5443,5384,5127,5340,5299"
```
**Gửi array → `400 · invalid input syntax for type bigint: "NaN"`. Swagger khai là `array`. Đây là chỗ dễ sai nhất.**

### 8.7 ⚠ Thiếu `User-Agent` → `403 · error code: 1010`

Cloudflare chặn user-agent mặc định của thư viện HTTP. **Không phải lỗi ứng dụng và KHÔNG CÓ TRONG SWAGGER.**

### 8.8 ⚠ Response rỗng có thể nghĩa là SAI TÀI KHOẢN, không phải KHÔNG CÓ DỮ LIỆU

Xem §3.1. Endpoint lọc theo tenant trong token. **Admin (tenant 175) và End User (tenant 176) thấy hai thế giới khác nhau.**

Ba lý do một endpoint trả rỗng, **và không lý do nào là lỗi** *(verbatim)*: ① **endpoint hành động** — chúng làm một việc rồi kết thúc, không có dữ liệu để trả · ② **staging chưa có dữ liệu** · ③ **gọi nhầm vai**.

### 8.9 ⚠ `DocumentSet.templates` là mảng JSON, không phải quan hệ

Xóa một template **không tự dọn khỏi set**; sửa template gốc thì **set không tự cập nhật**. Trong ODC, dựng **entity nối `DocumentSetTemplate(DocumentSetId, IdpTemplateId, SortOrder)` với FK thật cả hai chiều + unique** — và **sinh chuỗi CSV chỉ khi gọi ra IDP**, không lưu nó.

**⚠ `description` của set 193 còn mang chữ `MEDICAL_CARE`** — đó là di sản văn bản tự do, không phải ràng buộc. §3.3.

### 8.10 ⚠ Mỗi template một bộ cột khác nhau — không có map chung

Xem §3.2.2. **`5383` 13 cột · `5218` cùng tên nhưng 7 cột · `5125` 9 cột · `5312` 5 cột.** `PaidBySocialInsurance` tồn tại dưới **ba tên**. `ServiceCategory` **không template nào có**. `DrugRegistrationNo` **không bảng kê nào có**.

### 8.11 ⚠ `fields[].name` là NHÃN, không phải khóa máy

`5384` dùng `Ngày/ tháng/ năm sinh`, `5383` dùng `Ngày sinh` — **cùng khái niệm, hai nhãn**. Và `Place Of Treatment` là **tiếng Anh, có khoảng trắng, hoa giữa câu**. **Một `trim()` hay `toLowerCase()` vô tình ở adapter là mất field.**

**Back Office sửa prompt là `k` đổi ngầm** ⇒ `IdpFieldMap` phải có `Version`.

### 8.12 ⚠ Hai checkbox mâu thuẫn trên `5383`

`Thẻ bảo hiểm y tế` và `Không có thẻ bảo hiểm y tế` là **hai checkbox riêng**, mỗi cái trả `"có"`/`"không"`. **Cả hai cùng `"có"` là mâu thuẫn — GẮN CỜ, không tự giải quyết** (§0.2 ③).

### 8.13 ⚠ `state` vs `outcome` — hai enum, một cái tên

Webhook mang **`outcome`** (kết cục **xử lý**); kết quả `I-09` mang **`state`** (độ **đầy đủ bộ hồ sơ**). **Hai enum khác nhau dưới một tên property là đúng kiểu va chạm sinh ra một nhánh sai sáu tháng sau.**

### 8.14 ⚠ `depth` vs `benefit_level_no` — hai số nguyên, một từ "level"

*Level* trên dòng biểu quyền lợi là **độ sâu trong cây**; *benefit level* là **một plan gắn vào một hợp đồng**. D3 đổi tên cái thứ nhất thành **`depth`** chính vì **một payload không được mang cả hai nghĩa**.

### 8.15 ⚠ `L15`/`L16`/`L18` giữ số ngoài thứ tự CÓ CHỦ Ý

**Một số field là một ĐỊA CHỈ, không phải một VỊ TRÍ.** Đánh số lại cho bảng đọc gọn sẽ **âm thầm trỏ lại mọi trích dẫn đang có** trong D2, A5 và payload. Và **`L17` cố ý không dùng lại** cho số ngày, vì `L17` đã là phân bổ sự kiện.

### 8.16 ⚠ Ba field `X?` chưa ai đo độ chính xác

`UnitOfMeasure` (L4), `UnitPrice` (L6), `OtherPayerAmount` (L11), `DrugRegistrationNo` (L12), `DocumentDate` (L15), `Icd10` (L16), `TreatmentDaysLine` (L18) — **trích xuất từ v1.0 nhưng KHÔNG CÓ GÌ ĐỌC cho tới v1.2** ⇒ **độ chính xác của chúng CHƯA BAO GIỜ được đo**.

**Console phải ghi *"chưa xác nhận nguồn trích xuất"* NGAY TẠI FIELD** cho tới khi IDP trả lời.

### 8.17 ⚠ `SUPPLEMENT` và `ADMIN` KHÔNG còn tự từ chối

Xem §4.5. **Một category tự từ chối là đã đặt quyết định bảo hiểm vào trong bảng ánh xạ** — sai bước, sai mã, không trích dẫn điều khoản, và **vô điều kiện trong khi câu chữ hợp đồng có một mệnh đề điều kiện**.

### 8.18 ⚠ Bốn category mới không miễn phí với IDP

`SCREENING`, `VACCINATION`, `OPTICAL`, `HEARING` — **A5 sở hữu nửa ÁNH XẠ, IDP sở hữu nửa SUY RA.** Nửa A5 không sở hữu là nửa chưa ai hỏi tới.

### 8.19 ⚠ `T2-04` chưa có signature và có thể chưa có lớp tương ứng ở IDP

**Tier 2 được phân loại chứ không trích xuất, và signature của nó CHƯA BAO GIỜ được viết ra** — `T2-01` tới `T2-03` cũng không có. **`T2-04` khác ba cái kia ở một điểm quan trọng: chúng là chứng từ catalogue này luôn mong đợi, và một classifier huấn luyện trên bộ hồ sơ y tế Việt Nam sẽ đã từng thấy chúng. MỘT BIÊN BẢN CÔNG AN KHÔNG PHẢI CHỨNG TỪ Y TẾ CHÚT NÀO.**

Nếu câu trả lời là *không có lớp nào như vậy tồn tại*, thì **rule X-2 áp dụng** và mọi cái như vậy tới dưới dạng `unclassified` — **an toàn, và cũng là toàn bộ cơ chế checklist mà điều khoản dựa vào để hỏng một cách lặng lẽ.**

### 8.20 ⚠ Ba entity nằm cuối một tham chiếu vượt biên hệ thống

`PayoutAccount.MemberId`, `Beneficiary.MemberId`, `Claimant.MemberId` — **tham chiếu, không phải khóa ngoại**. Không gì ngăn chúng sống lâu hơn thành viên chúng nêu tên. Giảm thiểu: **thành viên được kết thúc hiệu lực, KHÔNG BAO GIỜ bị xóa**.

### 8.21 ⚠ Bốn giá trị `lossType` không có checklist

Xem §2.15. **Không bịa ra một cái. Render một LỜI TUYÊN BỐ VẮNG MẶT** — *"chưa có danh mục chứng từ cho loại tổn thất này"* — **chứ không phải một checklist rỗng đọc như là KHÔNG CẦN GÌ CẢ**.

### 8.22 ⚠ Hai tên cho một tập người yêu cầu

`claimantStanding` (tiếng Anh) vs `claimantCapacity` (tiếng Việt) — **cùng một tập dưới hai tên. Cặp này KHÔNG tạo ra tập mới.** §1.10.

### 8.23 ⚠ Mẫu `EXCL` không mang được thứ tầng điều khoản sinh ra

Ba yêu cầu gặp nhau trên một thông điệp và mẫu hiện tại **không thỏa cái nào**: ① `NOTREL`/`NOTCOV` ràng buộc tên khoản và số tiền, **`EXCL` không ràng buộc cái nào** · ② quy tắc đòi **MỌI** `clauseRef` đã bắn trên một dòng, theo thứ tự, **`EXCL` mang MỘT** · ③ quy tắc đòi **trích dẫn NGUYÊN VĂN** văn bản điều khoản của phiên bản đã đánh giá, **không gì ràng buộc nó**.

> **Một lá thư trích dẫn một trong hai điều khoản là một lá thư có thể bị bác bằng cách bác nhầm cái kia.** **⚠ cần A2 chốt.**

### 8.24 ⚠ Không có chữ ký chuyên môn y khoa sau tri thức nền

Xem §2.9. **Ba kiểm soát gánh rủi ro này và không cái nào là trang trí.** Nói thẳng khi pitch.

### 8.25 ⚠ Đua phiên bản trên sổ cái phải CHẠY LẠI, không phải GHI LẠI

Xem §2.7. **Con số là hàm của bộ đếm mà lệnh ghi đã va vào, nên phát lại nó là vi phạm hạn mức bằng một race.**

---

## 9. Phụ lục — đối chiếu sang MBAL-IDP

> **⚠⚠ ĐỌC CẢNH BÁO NÀY TRƯỚC KHI DÙNG BẤT KỲ DÒNG NÀO Ở DƯỚI.**
>
> **MBAL-IDP là một HỆ KHÁC.** Khác tenant (MBAL dùng tenant khác, Claim MVP dùng **176**), khác bộ template (MBAL `777`/`776`/`1091`/`2645`/`1141`/… — **40 templateId**; Claim MVP `5384`/`5127`/`5383`/`5299`/`5340`/`5312`), khác từ vựng nghiệp vụ, khác partner.
>
> **Phụ lục này CHỈ dùng để THAM CHIẾU khi cần kiểm chứng thực tế** — ví dụ: *"IDP có thực sự đọc được bảng kê tiếng Việt không?"*, *"độ chính xác thực đo được là bao nhiêu?"*, *"prompt phân loại trông như thế nào?"*.
>
> **KHÔNG ĐƯỢC dùng nó để:** seed dữ liệu · đặt tên enum · chọn templateId · dựng checklist · viết adapter. **Một ánh xạ ở đây bị chép vào code là một lỗi.**

### 9.1 Ánh xạ `lossType` ↔ `claim_type` MBAL

| `lossType` (chuẩn — Claim MVP) | `claim_type` MBAL gần nhất | Mã eBao | Ghi chú |
|---|---|---:|---|
| `ILL_OP`, `ILL_IP` | `MEDICAL_CARE` | 5 | MBAL không tách ngoại/nội trú ở tầng này |
| `DEN_OP`, `DEN_IP` | `MEDICAL_CARE` | 5 | **MBAL KHÔNG có giá trị nha khoa** |
| `ACC_OP`, `ACC_IP` | `MEDICAL_INJURY` | 12 | |
| `MAT_OP`, `MAT_IP` | `MEDICAL_CARE` | 5 | **MBAL KHÔNG có giá trị thai sản** |
| *(bệnh hiểm nghèo — không có trong `lossType`)* | `MEDICAL_ILLNESS` | 4 | **MBAL có, Claim MVP KHÔNG** |
| *4 giá trị tử vong / thương tật vĩnh viễn* | `MEDICAL_DEATH` | 2 | Ánh xạ thô |

**Ba lệch không khép được:**
1. **Chiều bệnh lý khác nhau.** MBAL chia theo **loại quyền lợi eBao**; Claim MVP chia theo **bệnh lý × nơi điều trị**. Nha khoa và thai sản **không có chỗ** trong bộ MBAL.
2. **`MEDICAL_ILLNESS` (bệnh hiểm nghèo) không có tương ứng** trong `lossType` — đó là một dòng sản phẩm khác.
3. **MBAL luôn nhận `claim_type` từ hệ gọi, KHÔNG BAO GIỜ trích từ chứng từ.** Claim MVP thì `lossType` **do khách chọn trên app** hoặc cán bộ gõ tại quầy. **Hai nguồn sự thật khác nhau.**

### 9.2 Ánh xạ `T1-xx` / `T2-xx` ↔ `CLxx`

| Loại chuẩn (Claim MVP) | `document_type` MBAL gần nhất | templateId MBAL tham khảo |
|---|---|---|
| **`T1-01`** Bảng kê chi phí | `CL26` *Bảng kê chi tiết* — hoặc `CL10` *Chứng từ y tế* | `776` (Custom-T102), `1091` |
| **`T1-02`** Đơn thuốc | `CL10` | `2645` |
| **`T1-03`** Phiếu khám bệnh | `CL10` | *(không có tương ứng rõ — MBAL có `2638` Sổ khám bệnh)* |
| **`T1-04`** Hóa đơn GTGT | `CL11` *Hoá đơn chi phí y tế* | `1089` |
| **`T2-01`** Giấy tờ tùy thân | `CLM_04` *Các chứng từ khác* | `1167` (CCCD), `2637` (Thẻ BHYT) |
| **`T2-02`** Kết quả cận lâm sàng | `CL10` hoặc `CL15` *Kết quả giải phẫu bệnh* | `2245`, `1141`, `1138`, `2639`, `2643` |
| **`T2-03`** Báo cáo y tế / GCN phẫu thuật | `CL06` *Chứng nhận phẫu thuật* | `973`/`2659`, `2641`, `1145` |
| **`T2-04`** Biên bản hoàn cảnh sự cố | `CL08` *Chứng từ tai nạn* hoặc `CL12` *Biên bản giám định* | `2656`, `2651`, `2814` |
| *(không có — A4 §1.4 loại khỏi Tier 1)* | `CL05` *Giấy ra viện* | `777` (Custom-T101) — **và `5384`/`5127` trên tenant 176** |
| *(không có)* | `CL07` *Trích lục khai tử* | `1133` |
| — | *fallback* | `231` = **Template khác (Other)** — **không phải `unclassified`** |

**Bốn lệch không khép được:**
1. **`T1-04` là XML-only ở Claim MVP; MBAL OCR ảnh hóa đơn.** Đây là **quyết định kiến trúc**, không phải khác biệt cấu hình.
2. **Giấy ra viện (`CL05`) là chứng từ BẮT BUỘC ở MBAL, nhưng bị LOẠI KHỎI Tier 1 ở Claim MVP** — đợt nội trú nhập tay.
3. **MBAL có 36 loại / 40 templateId; Claim MVP có 8 loại / 6 template.** Không phải cùng độ phân giải.
4. **`231 = Other` của MBAL là một FALLBACK ĐÃ PHÂN LOẠI; `unclassified` của Claim MVP là CHƯA PHÂN LOẠI ĐƯỢC.** **Hai thứ khác nhau về nghiệp vụ và phải khác nhau trong `ClaimDocument.ChecklistState`.**

### 9.3 Bẫy CHỈ thuộc MBAL — không áp cho build này

Những bẫy dưới đây đã được ghi lại từ MBAL production. **Chúng KHÔNG áp cho build Claim MVP** vì các entity liên quan đã bị bỏ. Giữ lại **chỉ để** khi ai đó nhìn vào MBAL thì không tưởng nhầm là mô hình tốt.

| Bẫy MBAL | Vì sao không áp cho build này |
|---|---|
| **`DOCUMENT_SETS.templates` là mảng jsonb, không phải quan hệ** — xóa template để lại id mồ côi, bị loại âm thầm chỉ với `logger.warn` | **Vẫn còn ở API tenant 176** — xem §8.9, và ODC dựng entity nối thay thế |
| **Cặp `(claim_type, document_type)` KHÔNG unique** — `findOne` lấy bừa một dòng khi trùng | **`MBALDocsetMap` ĐÃ BỎ.** Thay bằng `DocumentChecklistRule` khóa trên `LossType` + FK về Static Entity |
| **`claim_type`/`document_type` là varchar trần, không FK danh mục** — gõ sai vẫn lưu, lỗi chỉ lộ khi gọi thật | Idem — **FK về Static Entity chặn sai chính tả LÚC NHẬP** |
| **Template bị loại âm thầm khi thiếu quyền** — `getTemplates` lọc `tenantId = ? OR tenantId IS NULL` | **Vẫn còn ở tenant 176.** ODC phải **đếm số template yêu cầu vs số thực gửi và hiện chênh lệch** |
| **`Template → sub_document_type` bị ép N:1** — `findOneOrFail({templateId})`, không có ngữ cảnh claim_type | **`MBALTemplateMap` ĐÃ BỎ.** Thay bằng crosswalk §7.3 với 5 rule X-1…X-5 |
| **`MBALStatistics` chỉ đo, không so ngưỡng nào** | **ĐÃ BỎ.** Đo độ chính xác ở Claim MVP là `AccuracyRun` trên bộ ground-truth |
| **Sai chính tả trong schema MBAL**: `Idenification number` (thiếu "t") · `"Event Date "` (thừa dấu cách) · `DiagnosiCodes` (thiếu "s") · `deathDate` (viết thường) · `KhámChữaĐôngY` (tiếng Việt có dấu) | **KHÔNG áp** — đó là field của flow đánh giá MBAL, không phải của `I-15`/`I-16`. **Nhưng nếu đội có lúc nào gọi flow MBAL để so sánh thì phải giữ verbatim** |
| **`c_llm` có trong type nội bộ nhưng không có trong DTO công bố** | Ở Claim MVP, confidence là `[0,1]` mỗi field, **so sánh được giữa field và giữa loại chứng từ** |
| **Rule R5a (font/cắt ghép/con dấu) KHÔNG TỒN TẠI ở MBAL** | Cũng không có trong phạm vi Claim MVP — §2.10 |
| **Rule R3 (logic ngày) KHÔNG TỒN TẠI ở MBAL** | Ở Claim MVP, `R4` *ngày dịch vụ nằm trong đợt* có trong bộ 9 rule — §4.26 |

**Vẫn còn giá trị tham chiếu từ MBAL:**

| Thứ | Con số | Dùng để làm gì |
|---|---|---|
| Thời gian trích xuất | **26,3 s/chứng từ · ~16 s/trang** | Đặt kỳ vọng SLA |
| End-to-end | **27 s** · 821 chứng từ/giờ · 33 trang = 2ph44s | Idem |
| **Độ chính xác toàn bộ** | **80,5%** (100 lượt, 2.413 field) | **Số đo THẬT, không phải minh họa** |
| **Bảng kê chi phí** | **60,9%** — **thấp nhất, do merge cell** | **Cảnh báo trực tiếp cho `T1-01`** — chứng từ quan trọng nhất là chứng từ khó nhất |
| Tóm tắt bệnh án | 91,1% | |
| Phân loại đúng loại | 100% (vòng 2) | |
| Cần người can thiệp | **44,4%** ở ngưỡng 95% · **26,7%** ở ngưỡng 85% | **Đầu vào để chọn `MinClaimConfidence`** |

> **⚠ Con số 60,9% trên bảng kê là thứ đáng lo nhất chuyển được từ MBAL sang.** `T1-01` là **chứng từ mà mọi dòng chi phí đến từ đó**, và nó là loại chứng từ có độ chính xác **thấp nhất** đã đo được. **Đây là lý do `ClaimLine.Confidence` phải là MIN các ô và phải hiện ra trên lưới.**

---

## 10. Việc phải làm ngay, theo thứ tự

| # | Việc | Chặn cái gì | Hạn |
|---:|---|---|---|
| 1 | Xin **tài khoản service role `Tenant` (roleId=2), tenant 176** | Toàn bộ tầng IDP | trước khi code |
| 2 | **Gọi thật một vòng** `login → request/sync → check-status → result` và **ghi lại TỪNG BODY**, kể cả body `1017` | §5.3, §8.4 | ngay |
| 3 | `GET /template/5299` và `GET /template/5340` — **lấy tên và `fields[]` verbatim** | §3.2.1 | ngay |
| 4 | `GET /template/5383` **đầy đủ** — 13 cột `tableFields` để dựng bảng ánh xạ cột | §3.2.2 | ngay |
| 5 | **Hỏi IDP: điền cột phải của crosswalk §7.3** — lớp phân loại nào của họ về loại nào của ta | **Ưu tiên cao nhất.** `classification` đang tiêu thụ trên ánh xạ giả định | tuần này |
| 6 | **Hỏi IDP: `serviceCategory` có suy ra được không, và có suy ra được 4 giá trị mới không** | `L13`, và hai loại trừ | tuần này |
| 7 | **Hỏi IDP: `facilityCountry` cấp chứng từ** | Loại trừ điều trị ngoài lãnh thổ | tuần này |
| 8 | **Hỏi IDP: số ngày điều trị theo dòng** | Bộ đếm `DAY` | tuần này |
| 9 | **Hỏi IDP: `L15` ngày theo dòng và `L16` ICD theo dòng** — bảng kê VN có in đủ thường xuyên không | Nhánh lâm sàng + hai loại trừ | tuần này |
| 10 | **Hỏi IDP: `drugRegistrationNo` lấy từ template nào** | §3.2.2 | tuần này |
| 11 | Kiểm **set 194 vs set 193** — set nào có `5383` | §3.2.1 — không có bảng kê thì không có dòng chi phí | ngay |
| 12 | **Chốt với PO: mã của 4 giá trị `lossType` tử vong / thương tật vĩnh viễn** | §4.2 | **15/09** |
| 13 | **Chốt với PO/A4: `T2-04`** — biên bản công an hay xác nhận nhập viện hay cả hai; `REQUIRED` hay `EXPECTED` | §4.1 | **15/09** |
| 14 | **Chốt với A4: checklist cho 4 giá trị tử vong**, hoặc tuyên bố chúng ngoài phạm vi pilot | §2.15, §8.21 | **22/09** |
| 15 | **Chốt với A4: hai loại chứng từ tư cách** (`GUARDIAN`/`HEIR`/`AUTHORISED`) + khóa thứ ba `REQUIRE_DOC` | §2.15, §4.22 | **22/09** |
| 16 | **Chốt với A1: `claimantStanding` vs `claimantCapacity`** — một tên, một tập | §1.10, §8.22 | **22/09** |
| 17 | **Chốt với A2: mẫu `EXCL`** ràng buộc tên khoản, số tiền và DANH SÁCH điều khoản | §8.23 | **22/09** |
| 18 | **Đo** `MinClaimConfidence` / `LineConfidenceFloor` trên bộ 10–20 chứng từ đã ẩn danh | §4.14 — ngưỡng đặt từ benchmark, **không phải đoán** | **29/09** |
| 19 | Kiểm `continuousCoverFrom` + `renewalStatus` có trên `I-05` không | §1.7 — thiếu là mọi hồ sơ tái tục bị từ chối sai | trước 22/09 |
| 20 | Kiểm seed có `effective_from`/`effective_to` trên biểu quyền lợi không | §1.3 — thiếu thì `422 NO_SCHEDULE_AT_DATE` không test được | trước 22/09 |
| 21 | Ghi **4 kịch bản `IdpReplay`**: sạch · độ tin cậy thấp · thiếu chứng từ · lỗi cứng | §3.7 | trước 29/09 |

---

## 11. Phụ lục B — Tổng hợp "cần xác thực" / "cần chốt"

| Mục | Nội dung chưa chắc | Loại |
|---|---|---|
| §1.1 | Tập `productStatus` và ánh xạ sang tập InsureMO | cần chốt |
| §1.3 | `effective_from`/`effective_to` có trong seed và có được InsureMO tôn trọng không | cần xác thực |
| §1.5 | Vòng đời trạng thái hợp đồng của InsureMO rộng hơn 2 giá trị — ánh xạ chưa chốt | cần chốt |
| §1.7 | `continuousCoverFrom` + `renewalStatus` vắng khỏi `I-05` | cần xác thực |
| §2.1 | Đường sửa `InNetwork` khi danh mục thiếu lúc đăng ký | cần chốt với PO |
| §2.2 | Mã của `eventDecision` | cần chốt với A1 |
| §3.1 | Bảng quyền endpoint — vài dòng ghi "cả hai vai đều lỗi" mà vẫn trả 200 | cần xác thực |
| §3.2.1 | Tên verbatim của template `5299` và `5340` | cần xác thực |
| §3.2.1 | Set 193 không có `5383` — dùng set nào cho demo | cần xác thực |
| §3.2.2 | `serviceCategory` và `drugRegistrationNo` lấy từ đâu | cần hỏi IDP |
| §3.4 | IDP có trả lại nguyên vẹn khóa lạ trong `extraParams` không | cần xác thực |
| §4.1 | `T2-04` — tên, mức mặc định, có cần bản gốc | cần chốt với PO/A4 |
| §4.2 | Mã của 4 giá trị `lossType` tử vong / thương tật vĩnh viễn | cần chốt với A1 |
| §4.5 | IDP có suy ra được `L13`, và 4 giá trị mới | cần hỏi IDP |
| §4.14 | Ngưỡng `MinClaimConfidence` / `LineConfidenceFloor` | cần đo |
| §5.2 | Dải mã 1009–1013 và 4006 — chỉ có message, không có entry `API_CODE` | cần xác thực |
| §7.3 | **Cột phải của crosswalk phân loại — ưu tiên cao nhất** | cần hỏi IDP |
| §8.3 | `facilityCountry` cấp chứng từ | cần hỏi IDP |
| §8.19 | `T2-04` — IDP có lớp tương ứng không | cần hỏi IDP |
| §8.22 | `claimantStanding` vs `claimantCapacity` | cần chốt với A1 |
| §8.23 | Mẫu `EXCL` | cần chốt với A2 |

*Hết tài liệu.*
