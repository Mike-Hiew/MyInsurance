# T02b — Đối chiếu schema thật trên tenant với T02

**App:** ClaimMVP · `2a401c4d-d87e-4c44-8692-9ad661840125` · WebApplication · **revision 2**
**Environment:** Dev · `09d830a7-8a06-4652-9840-41b49ba67cc9`
**Publish:** `8b9a71c3-601a-4281-b75c-020ac150dcc4` — `outcome: success`, 08/09/2026 02:14:53 UTC
**Nguồn yêu cầu:** prompt T02 (`Prompt_OutSystems_MCP_theo_plan.xlsx`, cell E8) + BRD hackathon v1.0 §10.1
**Cách đọc:** `context_entities` (app ClaimMVP, owned_only true, limit 100) + `context_roles` — đọc lúc 02:15:51 UTC

---

## Kết luận

**0 dòng MISSING · 0 dòng DIFFERENT TYPE.** DoD của A2 đã đạt.

12 object đọc về: 8 entity nghiệp vụ + 4 static entity. 5 role đúng tên.

---

## Entity nghiệp vụ

| Entity | Attr (không kể Id) | Trạng thái |
|---|---|---|
| `Policy` | PolicyNo Text 50 · PolicyCode Text 20 · PolicyHolderName Text 200 · EffectiveDate Date · ExpiryDate Date · **StatusId → PolicyStatus Identifier** | OK |
| `Member` | **PolicyId → Policy** · MemberNo Text 50 · FullName Text 200 · DateOfBirth Date · Gender Text 10 · RelationshipToHolder Text 50 | OK |
| `PolicyBenefitSnapshot` | **PolicyId → Policy** · BenefitCode Text 50 · BenefitName Text 200 · SumInsured Currency · PerVisitCap Currency · CoinsurancePct Decimal(37,8) · WaitingPeriodDays Integer · AnnualLimit Currency · SnapshotDate Date | OK |
| `Claim` | ClaimNo Text 30 · **PolicyId → Policy** · **MemberId → Member** · **TreatmentTypeId → TreatmentType** · EventDate Date · AdmissionDate Date · DischargeDate Date · FacilityName Text 200 · **StatusId → ClaimStatus** · TotalClaimed Currency · TotalPayable Currency · CreatedOn Date Time | OK |
| `ClaimDocument` | **ClaimId → Claim** · TemplateId Text 50 · FileName Text 260 · IdpRequestId Text 100 · **RawExtractionJson Text 2.147.483.647** · Confidence Decimal(37,8) · ErrorCode Text 50 · UploadedOn Date Time | OK |
| `ClaimLine` | **ClaimId → Claim** · **ClaimDocumentId → ClaimDocument** · ServiceDate Date · Description Text 500 · BenefitCode Text 50 · Quantity Integer · AmountClaimed Currency · AmountPayable Currency · Confidence Decimal(37,8) · **DecisionCodeId → DecisionCode** · DecisionReason Text 500 | OK |
| `LimitLedger` | **PolicyId → Policy** · **MemberId → Member** · BenefitCode Text 50 · PeriodYear Integer · UsedAmount Currency · RemainingAmount Currency · **LastClaimId → Claim** · UpdatedOn Date Time | OK |
| `AgentRun` | **ClaimId → Claim** · AgentName Text 100 · ToolName Text 100 · **InputJson Text 2.147.483.647** · **OutputJson Text 2.147.483.647** · StartedOn Date Time · FinishedOn Date Time · IsError Boolean (default False) | OK |

Toàn bộ khoá ngoại là **entity Identifier thật** (có `foreignKey.entityKey` trỏ đúng đích), không có cột Text hay Long Integer lỏng.

## Static entity

| Static | Số bản ghi | Giá trị đọc về |
|---|---|---|
| `PolicyStatus` | 3 | Lapsed, Active, Expired |
| `ClaimStatus` | 8 | UnderReview, Paid, Approved, Declined, Received, ExtractionError, PendingApproval, Extracting |
| `TreatmentType` | 4 | Maternity, Dental, Outpatient, Inpatient |
| `DecisionCode` | 5 | NOTCOV, PARTIAL, NOTREL, COVERED, PENDING |

Đúng số lượng, đúng tên, không có giá trị thừa. Thứ tự đọc về khác thứ tự khai báo — không ảnh hưởng, tra cứu bằng Label.

## Role

| Role | Có |
|---|---|
| `ClaimOfficer` · `ClaimApprover` · `ClaimOps` · `Customer` · `Presenter` | 5/5, kèm description |
| `Template_WebApp` | **thừa** — role đi kèm template Web lúc `app_create` clone. Vô hại, xoá được trong Studio. |

---

## Ba điểm cần biết trước khi làm T03 / T07

### 1. Long text đã sửa từ 2.000 lên 2.147.483.647

Mentor lần dựng đầu đặt `RawExtractionJson`, `InputJson`, `OutputJson` = Text **2.000 ký tự**. Đo trên 150 file JSON thật đã ghi lại của IDP:

| Nhóm | File | JSON lớn nhất |
|---|---|---|
| 04 — nộp chứng từ & trích xuất | 19 | 19.671 byte |
| 05 — đánh giá | 15 | 549.231 byte |
| 00 — chuỗi end-to-end | 37 | 2.937.460 byte |

2.000 ký tự cắt cụt response trích xuất gấp ~10 lần, vi phạm quy tắc kiến trúc số 3 (kết quả trích xuất là bất biến — bản gốc bị cắt thì không còn là bản gốc). Đã sửa bằng một lượt Mentor thứ hai trước khi publish.

### 2. Tiền đang là `Currency`, BRD DR-05 nói `Decimal(18,0)`

T02 khai `Currency` và tenant đã dựng đúng như vậy — nên đây **không phải lỗi lệch so với T02**. Nhưng BRD §10.3 DR-05 ghi *"Money is Decimal(18,0) VND; rounding only in chain step 12"*. `Currency` trong ODC có phần thập phân, nên chuỗi 14 bước có thể sinh ra số lẻ VND ở các bước trung gian (CH-08 đồng chi trả, CH-09 co-pay theo %) trước khi tới bước làm tròn CH-12.

Cần quyết định trước khi viết `Step08_ApplyCoinsurance`: hoặc đổi các trường tiền sang Decimal(18,0), hoặc chấp nhận Currency và làm tròn tường minh trong từng step. **Chưa quyết.**

### 3. `UpdateBehavior = Optimized`

Mentor tự đặt trên các entity có long text (khuyến nghị scalability của nền tảng). Không đổi schema, để nguyên.

---

## Chưa dựng (đúng out-of-scope của lượt này)

Screen · UI flow · Server Action ngoài CRUD tự sinh · **seed data** · REST Consume tới IDP · AI agent · Timer / workflow · entity thứ 9 trở đi (`ChainRun`, `ClaimEvent`, `ClinicalRecommendation`, `FraudSignal`, `ClaimLetter`, `PaymentOrder`…).

**Việc kế tiếp theo runbook:** A3 / T03 — seed hợp đồng + 4 hồ sơ mẫu, điền số quyền lợi thật từ bản chụp P-02, không để `// TODO`.

---

*Hồ sơ đầy đủ của lượt build: `~/.claude/cache/outsystems-spec-driven-build/2a401c4d-d87e-4c44-8692-9ad661840125/` — `spec.md`, `mentor-prompt.txt`, `mentor-result.json`, `build-report.md`.*
