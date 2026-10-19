# T07 — 14 vỏ rỗng chuỗi tất định + RunAdjudicationChain

**App:** ClaimMVP · `2a401c4d-d87e-4c44-8692-9ad661840125` · Dev · **revision 4**
**Publish:** `137311fb-1b7d-4062-b5be-f9369b8ddb00` — `outcome: success`, 08/09/2026
**Đánh số:** theo **BRD §9.1.3** (CH-01…CH-14), không theo runbook — xem mục "Xung đột tên" bên dưới.

---

## 15 Server Action đã dựng

| Action | Node | Description (CH-nn) |
|---|---|---|
| `Step01_PolicyEligibility` | 3 | CH-01 hợp đồng còn hiệu lực + đã đóng phí tại EventDate |
| `Step02_WaitingPeriod` | 3 | CH-02 thời gian chờ theo loại tổn thất (bệnh 30 / bệnh đặc biệt 90 / tai nạn 0) |
| `Step03_EventClassification` | 3 | CH-03 map LossType + TreatmentType + ICD → nhóm quyền lợi |
| `Step04_LineBenefitMapping` | 3 | CH-04 map ServiceGroup → benefit item, hoặc UNMAPPED |
| `Step05_CoverageScopeExclusions` | 3 | CH-05 loại trừ + khuyến nghị lâm sàng đã xác nhận (NOTCOV / NOTREL) |
| `Step06_SocialInsuranceDeduction` | 3 | CH-06 trừ phần BHYT đã chi trả |
| `Step07_PerVisitCap` | 3 | CH-07 trần mỗi lượt / mỗi ngày → CAP_VISIT |
| `Step08_PerYearLimitCheck` | 3 | CH-08 hạn mức năm còn lại từ LimitLedger → LIMIT_YEAR |
| `Step09_CoPayment` | 3 | CH-09 đồng chi trả theo sản phẩm → COPAY |
| `Step10_Deductible` | 3 | CH-10 mức miễn thường năm → DEDUCT |
| `Step11_ClaimLevelCap` | 3 | CH-11 trần tổng của rider → LIMIT_TOTAL |
| `Step12_Rounding` | 3 | CH-12 làm tròn 1.000 VND, B-20 làm tròn xuống. **Bước DUY NHẤT được làm tròn** |
| `Step13_CustomerBorneBreakdown` | 3 | CH-13 tách phần khách tự chịu theo nguyên nhân |
| `Step14_LedgerPreview` | 3 | CH-14 tính tiêu hao hạn mức (chưa post) + 4 tổng |
| `RunAdjudicationChain` | 31 | Gọi Step01→Step14 tuần tự |

**Chữ ký chung 14 bước:** in `ClaimId` (Long Integer) · out `StepLog` (Text) · flow `Start → Assign StepLog="" → End`

**`RunAdjudicationChain`:** in `ClaimId` · out `TotalRequested` / `TotalCovered` / `TotalPayable` / `TotalCustomerBorne` (Currency) + `StepLogJson` (Text) · 14 local var `Log01`…`Log14`

Validation: **0 error**, 16 warning (đều là "unused action" / "unused ClaimId input" — đúng với vỏ rỗng chưa có caller).

---

## Hai quyết định thiết kế

**1. Vỏ rỗng KHÔNG ghi Entity nào.** `RunAdjudicationChain` trả `Total* = 0` cứng và không đụng `Claim.TotalPayable` hay `ClaimLine.AmountPayable`. Lý do: 14 bước còn rỗng, nếu cho ghi thì nó ghi đè số seed bằng 0 và hỏng dữ liệu demo. Lệnh ghi tiền thêm vào khi điền logic — và chỉ ở đây, đúng quy tắc kiến trúc số 1.

**2. `StepLog` (Text) trên mỗi bước** — chỗ chứa câu diễn giải tiếng Việt cho AG-5 đọc. Agent diễn giải, không tự tính.

## Xung đột tên đã giải quyết

Runbook 8/9 và BRD §9.1.3 đánh số khác nhau, chỉ 1/4 khớp:

| Runbook | BRD (đã chọn) |
|---|---|
| `Step05_MapLinesToBenefit` | `Step04_LineBenefitMapping` |
| `Step07_ApplyPerVisitCap` | `Step07_PerVisitCap` ✓ |
| `Step08_ApplyCoinsurance` | `Step09_CoPayment` |
| `Step11_SumPayable` | `Step11_ClaimLevelCap` |

**Chốt theo BRD** để truy vết được về CH-01…CH-14 và MVP1 FS F48. Bốn bước runbook định tự viết, đọc theo tên mới: `Step04_LineBenefitMapping`, `Step07_PerVisitCap`, `Step09_CoPayment`, `Step11_ClaimLevelCap`.

## Hai việc treo — ĐÃ GIẢI QUYẾT ở revision 5

Publish `03ac7d2d-2610-4839-ba4f-821886453b45`, `outcome: success`. Đã đọc lại tenant xác minh cả hai.

### 1. Kiểu tiền → `Decimal(18,0)`

Chốt theo BRD DR-05. VND không có đơn vị nhỏ hơn, và `Step12_Rounding` là bước duy nhất được làm tròn — nếu bước trung gian sinh phân số thì SC-H4 "chạy hai lần ra cùng số" mất nền và hồ sơ B không tái tạo đúng 1.880.000. Đổi lúc này không tốn gì vì 6 bảng còn rỗng (`BootstrapDemoData` chưa bấm lần nào).

**9/9 attribute đã đổi, đọc lại từ tenant xác nhận `Decimal(18,0)`:**

| Entity | Attribute |
|---|---|
| `PolicyBenefitSnapshot` | `SumInsured` · `PerVisitCap` · `AnnualLimit` |
| `Claim` | `TotalClaimed` · `TotalPayable` |
| `ClaimLine` | `AmountClaimed` · `AmountPayable` |
| `LimitLedger` | `UsedAmount` · `RemainingAmount` |

Kéo theo: 45 local var `*_SumInsured` / `*_PerVisitCap` / `*_AnnualLimit` trong `Seed_BenefitSnapshots` đổi sang Decimal (Default Value giữ nguyên), và 4 output của `RunAdjudicationChain` đổi sang Decimal.

**Không đổi** (không phải tiền): `CoinsurancePct` và `Confidence` vẫn `Decimal(37,8)`.

### 2. `ChainRun` — đã tạo, 13 attribute

`ClaimId` (FK Claim, mandatory) · `RunNo` (Integer, mandatory) · `InputsHash` (Text 64) · `Version` (Text 20) · `StepOutputsJson` (Text 2.147.483.647) · `TotalRequested` / `TotalCovered` / `TotalPayable` / `TotalCustomerBorne` (Decimal 18,0) · `GateResultJson` (Text 2.147.483.647) · `RunAt` (Date Time, mandatory) · `TriggeredBy` (Text 100)

**Hệ quả đã xử lý:** `ChainRun.ClaimId` là FK tới `Claim`, nên `Seed_ClearDemoData` phải xoá ChainRun trước Claim. Action đã sửa, 26 → 29 node, thứ tự xoá giờ là:

`ClaimLine → AgentRun → ClaimDocument → ChainRun → LimitLedger → Claim → PolicyBenefitSnapshot → Member → Policy`

Không sửa chỗ này thì `BootstrapDemoData` hết chạy lại được ngay khi chuỗi ghi bản ghi ChainRun đầu tiên.

## Việc còn lại

`BootstrapDemoData` vẫn **chưa được bấm lần nào** — 6 bảng đang rỗng. Bấm ở `/ClaimMVP/SeedAdmin` rồi đối chiếu 3 / 3 / 15 / 15 / 4 / 16.

## Giới hạn Mentor đã đo được hôm nay

Mentor nối được flow tuyến tính tới **~35 node**; ở **~140 node** nó gãy wiring và rebuild vô hạn (4 vòng, phải cancel). Giá trị hằng nên đặt ở **Default Value của local variable**, không phải node Assign — 75 node Assign là thứ làm gãy lượt đầu.

---

*Cập nhật 08/09/2026. Xem thêm `T02b_Schema_Verification_ClaimMVP_2026-09-08.md`.*
