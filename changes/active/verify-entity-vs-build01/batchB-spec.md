# Đợt B — spec thi hành: đầu vào của chuỗi 14 bước

**Ngày:** 21/09/2026 · **Theo:** `delta.md` §6.2 hàng B (§8 không ghi đè B) · **Chi tiết:** §3.1 (Member), §3.2 (PolicyBenefitSnapshot), §3.5 (Claim), §1.2 (`BenefitWaitingPeriod`).
**Nguồn giá trị:** A5 v1.5 §4.1 (cây), §4.2 (limit type, count limit), §4.3 (khấu trừ), §5 (thời gian chờ) · A1 §4.6 (`renewalStatus`) · D2 §3.3.3 (phép kiểm cover).
## Sửa của PO 21/09 (trước khi chạy)

| # | Sửa | Lý do |
|---|---|---|
| **B1** | ~~`LossTypeId` chiếu tạm từ `TreatmentTypeId`~~ **BÁC.** Seed `LossTypeId` **bằng tay, từng hồ sơ** (12 hồ sơ, dữ liệu thật). `TreatmentType` giữ nguyên; đợt F biến nó thành phép chiếu read-only **từ** `LossType` (đúng chiều) | build-01 §4.2: `lossType` **được nhập**, không bao giờ suy ra; §4.3: `episodeType` là phép chiếu một chiều *từ* `lossType` — chiếu ngược là chạy sai chiều, và **mất thông tin** (4–5 giá trị không phân biệt được ILL_OP / ACC_OP / DEN_OP) ⇒ ghi giá trị SAI vào đúng field quyết định checklist |
| **B2** | `BenefitWaitingPeriod` seed **cả 3 plan** (HC-STD, HC-PLS, HC-PRM — mã theo A5 §1 & D2 §3.1, **không phải** HC-PLUS/HC-PRE như bản nháp 08:30): 17 tổ hợp × 3 = **51 dòng** | build-01 §1.4; +34 dòng seed, 0 lượt Mentor, bớt một vòng sau này |
| **B3** | Thêm **`CopayPct` Text(8)** vào `PolicyBenefitSnapshot`, **cạnh** `CoinsurancePct` (để đợt F gỡ `CoinsurancePct`). Phải là Text vì chứa **hoặc** basis point nguyên **hoặc** literal `INHERIT`; pilot: `200.1`, `200.2`, `200.3`, `300.1` đều `INHERIT` | Cột decimal không giữ được sentinel; build-01 §1.3: sentinel chưa giải phải **fail to**, không được âm thầm thành 0 |
| **B4** | `CountLimit` phải được **seed tường minh trên mọi dòng**; 0 = KHÔNG GIỚI HẠN, không phải "để trống". Định nghĩa hằng có tên **`COUNT_UNLIMITED` = 0** và không bao giờ so với số 0 trần | Nếu Mentor để cột mặc định, mặc định trùng sentinel — đúng hôm nay, và không ai biết vì sao về sau |

**Chỉ ClaimMVP.** Không đụng MyInsurance, không đụng logic tiền (`RunAdjudicationChain`, `Step*`), không đụng portal. Bước nào của chuỗi *đọc* các cột này là việc của change `adjudication`, không phải của đợt này.

## Nguyên tắc seed (theo phán quyết L4 20/09)

Cột mới **không mandatory** (entity đang có dữ liệu). Dữ liệu cho cột mới đi qua **seed hiện có** (`Seed_PoliciesAndMembers`, `Seed_BenefitSnapshots`, `Seed_Claims`) được mở rộng để ghi cột mới, rồi bấm **"Re-run demo seed"** trên Administration — không migration riêng, không nút mới. **Số tiền demo giữ nguyên** (AnnualLimit, PerVisitCap, CoinsurancePct của 5 dòng snapshot hiện có không đổi — chúng nuôi con số trên Dashboard/portal).

## B1 — Static `RenewalStatus` (A1 §4.6)

| Identifier | Value | Label | LabelVi |
|---|---|---|---|
| FirstTime | LẦN_ĐẦU | First-time participant | Tham gia lần đầu |
| Renewing | TÁI_TỤC | Renewing — continuous cover | Tái tục |
| Unknown | KHÔNG_XÁC_ĐỊNH | Not known | Không xác định |

## B2 — `Member` (+6)

| Attribute | Kiểu | Ghi chú | Seed (`Seed_PoliciesAndMembers`) |
|---|---|---|---|
| CoverageFrom | Date | ngày bắt đầu cover **của thành viên** (không phải inception hợp đồng) | = `Policy.EffectiveDate` của policy chứa member |
| CoverageTo | Date | | = `Policy.ExpiryDate` |
| ContinuousCoverFrom | Date | thời gian chờ đo từ đây (A5 §5 rule 1) | = CoverageFrom (mọi member demo là lần đầu) |
| RenewalStatusId | RenewalStatus Identifier | | `FirstTime` |
| CardNo | Text(20) | khoá tìm kiếm chính; **khoá duy nhất tìm người phụ thuộc** | = giá trị `MemberNo` hiện có (màn đang dán nhãn "Card ID" cho MemberNo) |
| BenefitLevelNo | Integer | thành viên lấy cover từ level nào | 1 |

Description của `CoverageFrom/To`: "Cover window of the member. The 5 date checks of D2 §3.3.3 read these, never Policy.EffectiveDate/ExpiryDate." — không thêm `Status` (🟡, ngoài hàng B; ràng buộc #5 ghi ở delta).

## B3 — `PolicyBenefitSnapshot` (+10)

| Attribute | Kiểu | Giá trị theo A5 |
|---|---|---|
| ParentBenefitCode | Text(50) | code cha trong cây §4.1; rỗng cho gốc (100, 200, 300, 400, 500) |
| Depth | Integer | 0 gốc · 1 con · 2 cháu (`100.1.1`) |
| LimitType | Text(20) | `ANNUAL` · `PER_DAY` · `PER_EVENT` · `PER_VISIT` (§4.2 cột *Limit type*) |
| CountLimit | Integer | §4.2 cột *Count limit*; **seed tường minh trên MỌI dòng** qua hằng `COUNT_UNLIMITED` (= 0) khi không giới hạn — không để mặc định (B4) |
| CountUnit | Text(10) | `DAY` (100.1, 100.1.1) · `VISIT` (200.3) · rỗng khi CountLimit = 0 |
| DeductionPct | Integer | **basis point**: 2000 trên `200` và `300`; 0 nơi khác (§4.3) |
| DeductionCondition | Text(50) | `PROVIDER_OFF_NETWORK` trên `200` và `300`; rỗng nơi khác |
| EffectiveFrom | Date | = `Policy.EffectiveDate` |
| EffectiveTo | Date | = `Policy.ExpiryDate` |
| **CopayPct** | **Text(8)** | basis point nguyên **hoặc** `INHERIT`: `200` → `"2000"` · `300` → `"3000"` (A5 §4.2 Standard 20 % / 30 %; theo tier nếu snapshot có tier khác: Plus 20/30, Premium 10/20) · `200.1` `200.2` `200.3` `300.1` → `"INHERIT"` · còn lại rỗng. `CoinsurancePct` **giữ nguyên** (đợt F gỡ) |

Bảng cây A5 §4.1 (Mentor áp theo `BenefitCode`; dòng nào seed không có thì bỏ qua — **không tạo dòng mới, không đổi tiền**):

| Code | Parent | Depth | LimitType | CountLimit | CountUnit | Deduction |
|---|---|---|---|---|---|---|
| 100 | | 0 | ANNUAL | 0 | | |
| 100.1 | 100 | 1 | PER_DAY | 60 | DAY | |
| 100.1.1 | 100.1 | 2 | PER_DAY | 15 | DAY | |
| 100.2 | 100 | 1 | PER_EVENT | 0 | | |
| 100.3 | 100 | 1 | PER_EVENT | 0 | | |
| 100.4 | 100 | 1 | PER_EVENT | 0 | | |
| 200 | | 0 | ANNUAL | 0 | | 2000 / PROVIDER_OFF_NETWORK |
| 200.1 | 200 | 1 | PER_VISIT | 0 | | |
| 200.2 | 200 | 1 | PER_VISIT | 0 | | |
| 200.3 | 200 | 1 | PER_VISIT | 20 | VISIT | |
| 300 | | 0 | ANNUAL | 0 | | 2000 / PROVIDER_OFF_NETWORK |
| 300.1 | 300 | 1 | PER_VISIT | 0 | | |
| 400 | | 0 | ANNUAL | 0 | | |
| 400.1 / 400.2 / 400.3 | 400 | 1 | PER_EVENT | 0 | | |
| 500 | | 0 | ANNUAL | 0 | | |

**Không làm ở B:** `MemberId` trên snapshot (🟡) · `CoinsurancePct` → basis point (§4 dòng 3, cùng nhóm "độ rộng", đợt F) · `WaitingPeriodDays` giữ nguyên nhưng **đánh dấu deprecated** trong Description ("Superseded by BenefitWaitingPeriod — do not read").

## B4 — Entity mới `BenefitWaitingPeriod` (A5 §5)

| Attribute | Kiểu | |
|---|---|---|
| Id | Long Integer | PK |
| PlanCode | Text(20) | mandatory — `Policy.PolicyCode` (demo: `HC-STD`) |
| BenefitGroupCode | Text(10) | mandatory — gốc cây: 100 / 200 / 300 / 400 / 500 |
| ConditionClassId | ConditionClass Identifier | mandatory (static A5) |
| Days | Integer | mandatory |

Description: "(PlanCode, BenefitGroupCode, ConditionClass) → Days. A5 §5. A missing row means the combination cannot arise (n/a) — a lookup miss is a DATA ERROR, never a 0-day wait." Index duy nhất trên (PlanCode, BenefitGroupCode, ConditionClassId).

Seed (**51 dòng** = 17 tổ hợp × 3 plan `HC-STD` / `HC-PLS` / `HC-PRM` — B2; `n/a` = không có dòng; cùng bảng ngày cho cả 3 plan vì A5 §5 không phân theo tier) qua `Seed_PoliciesAndMembers` (hoặc action seed mới `Seed_WaitingPeriods` gọi từ `BootstrapDemoData` — Mentor chọn, miễn "Re-run demo seed" chạy nó):

| Class | 100 | 200 | 300 | 400 | 500 |
|---|---|---|---|---|---|
| PRE | 365 | 365 | 365 | — | 0 |
| SPE | 365 | 365 | — | — | 0 |
| GEN | 30 | 30 | 90 | — | 0 |
| BIR | — | — | — | 270 | — |
| MAT | — | — | — | 90 | — |
| ACC | 0 | 0 | 0 | — | 0 |

## B5 — `Claim` (+4)

| Attribute | Kiểu | Seed (`Seed_Claims`) + backfill dòng đang có |
|---|---|---|
| LossTypeId | LossType Identifier (static A5), nullable | **Seed bằng tay từng hồ sơ (B1)** — bảng dưới. Không suy từ `TreatmentType` |
| TreatmentFrom | Date | = `AdmissionDate` nếu có, không thì `EventDate` |
| TreatmentTo | Date | = `DischargeDate` nếu có, không thì `EventDate` |
| RuleSetRef | Text(50) | `A5-v1.5/HC-STD` |

### B5.1 — `LossTypeId` từng hồ sơ (PO: "run it" 21/09 — 6 hồ sơ thai sản suy từ *phòng khám + số tiền cỡ khám thai*, ghi rõ để PO bác nếu sai)

| Claim | Dữ liệu thật đang có | LossType đề xuất | Căn cứ |
|---|---|---|---|
| CLM-2026-0001 (Id 17) | ~~Inpatient · 4 dòng~~ → **Outpatient · N39.0 · 6 dòng prototype hồ sơ A (B3, PO duyệt 21/09)** | ~~`ILL_IP`~~ **`ILL_OP`** | bệnh, ngoại trú — chép nhầm thành nội trú, sửa ở B3 (`batchB2-spec.md` §Lối C) |
| CLM-2026-0002 (Id 18) | Inpatient · Paid · ROOM/LAB/thuốc/TPCN | `ILL_IP` | bệnh, nội trú |
| CLM-2026-0003 (Id 19) | Inpatient · CT ổ bụng, phẫu thuật nội soi cắt ruột thừa | `ILL_IP` | bệnh, nội trú (không phải tai nạn) |
| CLM-2026-0004 (Id 20) | Outpatient · Victoria Healthcare · khám nội, XN máu, thuốc | `ILL_OP` | bệnh, ngoại trú |
| CLM-2026-0034 (Id 34) | Dental · Medlatec · portal | `DEN_OP` | nha khoa ngoại trú |
| CLM-2026-0037 (Id 37) | Dental · Medlatec · portal | `DEN_OP` | |
| CLM-2026-0035 / 0036 (35, 36) | Maternity · Medlatec · chưa có số tiền | `MAT_OP` | Medlatec là phòng khám → khám thai ngoại trú; **cần PO xác nhận** (không có chẩn đoán) |
| CLM-2026-0038 / 0039 / 0040 / 0041 (38–41) | Maternity · Medlatec · 1.250.000 / 980.000 / 750.000 / 640.000 ₫ | `MAT_OP` | số tiền cỡ khám thai, không phải sinh; **cần PO xác nhận** |

`AdmissionDate`/`DischargeDate` **giữ** (nội trú vẫn cần), Description ghi "inpatient only; the chain reads TreatmentFrom/To".

## Kiểm B xong

1. `current-model.md` cập nhật lại từ `context_entities` (rev mới): Member 13 · PolicyBenefitSnapshot 19 · Claim 21 · `BenefitWaitingPeriod` 5 · static `RenewalStatus` 3 record.
2. Sau "Re-run demo seed": Policies & benefits (console) vẫn hiện 5 quyền lợi với **số tiền cũ**; PortalHome "Outpatient limit left 1.000.000 / 3.000.000" **không đổi**.
3. 17 màn render như trước (WorkList, ClaimDetail, 7 portal).

## Kết quả thi hành — 21/09/2026 (Mentor session `e4214354…`, 4 lượt, 0 lỗi)

| Bước | Trạng thái | Rev | Ghi chú |
|---|---|---|---|
| B1 static `RenewalStatus` | ✅ | ClaimMVP 133 | 3 record FirstTime / Renewing / Unknown, cùng shape `ConditionClass` |
| B2 `Member` +6 | ✅ | 133 | nullable; `Seed_PoliciesAndMembers` ghi khi tạo; backfill điền dòng đang có |
| B3 `PolicyBenefitSnapshot` +10 (kể cả `CopayPct` Text(8)) | ✅ cột · ⏸ **giá trị** | 133 | cột đã có, `WaitingPeriodDays` đánh dấu DEPRECATED. **Giá trị cây chưa seed** — xem §Phát hiện 2 |
| B4 `BenefitWaitingPeriod` + unique index | ✅ | 133 | `UQ_BenefitWaitingPeriod` (PlanCode, BenefitGroupCode, ConditionClassId) |
| B4 seed 51 dòng | ✅ | 134 | `Seed_WaitingPeriods` → 51 × `Seed_WaitingPeriod_Upsert` (idempotent, không xoá); HC-STD / HC-PLS / HC-PRM |
| B5 `Claim` +4 | ✅ | 133 | nullable; `AdmissionDate/DischargeDate` description "Inpatient only" |
| B5.1 `LossTypeId` từng hồ sơ | ✅ code · ⏳ chạy | 134 | bảng 12 ClaimNo nằm trong `Seed_BatchB_Backfill`; **chỉ ghi khi đang null** |
| `COUNT_UNLIMITED()` | ✅ | 133 | Server Function, trả 0, description ghi luật "không so với 0 trần" |
| Dây vào "Re-run demo seed" | ✅ | 134 | `RunSeedBatch0Now` = Reset → AssessmentAndTerms → **Seed_WaitingPeriods → Seed_BatchB_Backfill**; `BootstrapDemoData` cũng gọi cả hai |
| `current-model.md` | ✅ | 134 | Member 13 · PolicyBenefitSnapshot **20** (không phải 19 — B3 thêm CopayPct) · Claim 21 · BenefitWaitingPeriod 5 · static RenewalStatus 6 attr. Bản cũ: `current-model.rev128.bak.md` |

**Chưa làm (chờ PO bấm "Re-run demo seed"):** kiểm §"Kiểm B xong" mục 2–3, đếm dòng `BenefitWaitingPeriod` = 51, `Claim.LossTypeId` ≠ null trên 12 hồ sơ.

### Phát hiện khi thi hành (lượt read-only trước khi sửa)

1. **"Re-run demo seed" KHÔNG chạy `Seed_PoliciesAndMembers` / `Seed_BenefitSnapshots` / `Seed_Claims`.** Nút gọi `RunSeed` → `RunSeedBatch0Now` → `Seed_ResetDemoClaimData` (chỉ dọn dependent của 4 hồ sơ đầu, **không xoá** Claim/Member/Policy/Snapshot) → `Seed_AssessmentAndTerms`. `BootstrapDemoData` (xoá sạch + tạo lại) không có nút nào gọi. ⇒ Mở rộng seed tạo mới là chưa đủ; cần `Seed_BatchB_Backfill` (chỉ điền null, không tạo/xoá, không đụng tiền) và nối vào `RunSeedBatch0Now`. Hồ sơ 34–41 **sống sót** qua nút này (khớp quyết "giữ" 20/09) — backfill theo `ClaimNo` là đủ.
2. **`PolicyBenefitSnapshot.BenefitCode` không phải mã A5.** 15 dòng = `INPATIENT_ROOM · INPATIENT_SURGERY · INPATIENT_OTHER · OUTPATIENT · DENTAL` × 3 policy (HC-STD `HC-2026-0001` / HC-PLS `-0002` / HC-PRM `-0003`, đều 2026-01-01→12-31). Bảng cây §B3 khoá theo `100 · 100.1 · 200.1…` ⇒ **0 dòng khớp**. `delta.md` §3.2 so attribute, không so *giá trị* mã — lỗ hổng của delta. Đã seed **chỉ** `EffectiveFrom/To` (= ngày policy, không phải đoán). 8 cột cây (`ParentBenefitCode, Depth, LimitType, CountLimit, CountUnit, DeductionPct, DeductionCondition, CopayPct`) **để null chờ PO phán**, vì gán `INPATIENT_OTHER → 100.3` là một phép chiếu đoán — đúng loại việc B1 đã bác. B4 ("CountLimit seed tường minh mọi dòng") vì thế chưa thoả trên 15 dòng cũ. Lựa chọn cho PO: (a) đợt B2: re-seed snapshot thành **17 dòng A5 × 3 plan = 51** (khớp `specs/adjudication` "51 benefit + 51 waiting period"), gỡ 5 mã cũ — kéo theo `ClaimLine.BenefitCode`, màn Policies & benefits, PortalHome "Outpatient limit left" phải trỏ mã mới; (b) giữ 5 dòng cũ, PO cấp bảng ánh xạ tường minh 5 mã → cột cây; (c) hoãn tới đợt F.
3. Mã plan trong bản nháp 08:30 sai (`HC-PLUS/HC-PRE`); A5 §1 và D2 §3.1 là `HC-PLS/HC-PRM` — đã sửa trước khi chạy, và seed policy hiện có cũng dùng đúng mã này.
4. `Seed_Claims` giờ suy `RuleSetRef = "A5-v1.5/" + Policy.PolicyCode` (không hard-code `HC-STD`) — cùng giá trị cho hồ sơ demo, nhưng đúng khi hồ sơ thuộc plan khác.
