# Đợt B2 — spec thi hành: seed cây quyền lợi A5 thật (thay 5 mã prototype)

**Ngày:** 21/09/2026 · **Quyết của PO:** `batchB-spec.md` §Phát hiện 2 → **phương án (a)**. Lý do PO ghi lại: (c) bất khả — bước 5, 8, 12, 13 đọc cây; (b) là lỗi B1 chiều ngược (OUTPATIENT phủ 200.1+200.2+200.3 → mất thông tin; alias 5 mã là nhãn hiển thị, không phải cây); (a) không phải việc mới — build-04 §5–6 luôn định 51 dòng lịch quyền lợi × 3 plan, trong mốc 22/09.
**Nguồn giá trị:** A5 v1.5 §4.1 (cây), §4.2 (limit type, số tiền theo tier, count limit, co-pay), §4.3 (khấu trừ). A4 §4.5 (mapping ServiceCategory → mã, đã seed đợt A).

## Luật PO (nguyên văn rút gọn)

1. Seed 17 quyền lợi × 3 plan = 51 dòng, mã A5 thật, số tiền A5 thật, + 8 cột cây. Upsert idempotent, cùng dạng `Seed_WaitingPeriods`.
2. **SỐ TIỀN TRÊN MÀN SẼ ĐỔI.** 1.000.000 / 3.000.000 trên PortalHome là số prototype bịa; A5 là chuẩn. Màn hiện số khác sau đợt này là **ĐÚNG**, không phải hồi quy — ghi vào runtime-check trước khi chụp lại.
3. `ClaimLine.BenefitCode` trên 16 dòng: **không đoán**. Suy qua `ClaimLine.Category → ServiceCategory → mã A5` (A4 §4.5 đã seed), cột OP/IP lấy theo claim. Dòng không giải được → `UNMAPPED` + liệt kê (build-01 §2.4).
4. Gỡ 5 mã cũ **chỉ sau khi** 16 dòng đã remap. Báo dòng nào đổi.
5. Blast radius kiểm sau publish: Policies & benefits (5 → 17 dòng), PortalHome limit tile, LimitLedger, ClaimDetail costs tab.

## Phát hiện trước khi sửa (lượt Mentor read-only 21/09)

| # | Phát hiện | Hệ quả cho B2 |
|---|---|---|
| 1 | `Seed_ClaimLines` **không ghi `Category`** (16 dòng rỗng); chỉ `Seed_ResetDemoClaimData` đóng `ROOM` lên dòng đầu của 3 hồ sơ đầu | Luật 3 cần Category trên mọi dòng ⇒ seed phải **tự ghi Category** (dữ liệu nhập tay, bảng §B2.3, PO bác nếu sai) rồi mới suy mã |
| 2 | `GetCalcSnapshotData`/`GetCalcSnapshotData2` (portal "Outpatient limit left"), `ClaimDetail.GetCalcLedger`, `GetCalcPBS`, `LimitLedgerView.GetMembersAll` đã lọc `BenefitCode = "200"` | Chưa bao giờ khớp dòng seed; **bắt đầu chạy đúng** sau B2 — không cần sửa code |
| 3 | `Seed_LimitLedger`: 15 dòng = 5 mã cũ × 3 member, Used 0, Remaining 0, không claim. Ngoài ra chuỗi đã tự ghi dòng `200 / 200.1 / 200.2` có `LastClaimId` (màn "Consumption records") | Seed đổi sang 5 mã gốc `100/200/300/400/500`; gỡ 15 dòng cũ (chỉ khi Used = 0 và không có claim — không mất thông tin); **không đụng** dòng do chuỗi ghi |
| 4 | `ClaimDetail.BuildBenefitRows` nhóm settlement theo `Category = "ROOM"` với 2 slot nhãn cứng `INPATIENT_ROOM` / `INPATIENT_OTHER`; không join snapshot | Nhãn hiển thị thuần — không phụ thuộc dữ liệu, để đợt F (dọn inline console). Tab costs (`GetLines` join `PolicyBenefitSnapshot` theo BenefitCode) sẽ join được sau remap |
| 5 | Entity chỉ có `AnnualLimit` + `PerVisitCap`; A5 §4.2 có 4 loại giới hạn | Thêm `PerDayCap`, `PerEventCap` Decimal(18,0) nullable (luật 1 của PO liệt kê AnnualCap/PerEventCap/PerDayCap). `AnnualLimit` giữ tên (đợt F đổi tên). **Quy ước:** trên dòng con `AnnualLimit = 0` nghĩa là *included* — tính vào trần năm của tổ tiên (A5 §4.1); Decimal ODC không null |
| 6 | `Portal_SubmitClaim` không ghi `LossTypeId`, không tạo ClaimLine | Ngoài phạm vi B2; ghi để đợt sau |
| 7 | A1/A5 không có tên tiếng Việt chính thức cho 17 quyền lợi | `BenefitName` seed **tiếng Việt tạm** (portal là tiếng Việt), tên EN A5 trong bảng dưới; chờ soát wording như `ServiceCategory.LabelVi` đợt A |

## B2.1 — Model (+2, nullable)

| Attribute | Kiểu | Description |
|---|---|---|
| PerDayCap | Decimal(18,0) | "A5 per-day cap; only on LimitType = PER_DAY rows, else 0." |
| PerEventCap | Decimal(18,0) | "A5 per-event cap; only on LimitType = PER_EVENT rows, else 0." |

Description bổ sung cho `AnnualLimit`: "Annual cap. On a child row 0 means INCLUDED in the ancestor's annual cap (A5 §4.1)." — `PerVisitCap`: "A5 per-visit cap; only on LimitType = PER_VISIT rows, else 0."

## B2.2 — `Seed_BenefitSchedule` (51 dòng, upsert theo (PolicyId, BenefitCode))

Helper `Seed_BenefitSchedule_Upsert(PolicyId, BenefitCode, BenefitName, ParentBenefitCode, Depth, LimitType, Amount, CountLimit, CountUnit, DeductionPct, DeductionCondition, CopayPct)`; `Amount` được đổ vào **đúng một** cột theo `LimitType` (ANNUAL→AnnualLimit · PER_DAY→PerDayCap · PER_EVENT→PerEventCap · PER_VISIT→PerVisitCap), các cột cap còn lại = 0. `SumInsured` = AnnualLimit (gốc) / 0 (con). `CoinsurancePct` (legacy, đợt F gỡ) = CopayPct/100 khi CopayPct là số, 0 khi rỗng/INHERIT. `WaitingPeriodDays` = 0 (deprecated). `SnapshotDate` = `EffectiveFrom` = Policy.EffectiveDate; `EffectiveTo` = Policy.ExpiryDate. Tồn tại → update mọi cột; chưa → create. Không xoá.

| Code | Parent | Depth | LimitType | STD | PLS | PRM | CountLimit | Unit | Deduction | CopayPct STD/PLS/PRM | BenefitName (VI tạm · EN A5) |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 100 | | 0 | ANNUAL | 100.000.000 | 200.000.000 | 400.000.000 | 0 | | | "" | Điều trị nội trú · Inpatient treatment |
| 100.1 | 100 | 1 | PER_DAY | 1.500.000 | 2.500.000 | 4.000.000 | 60 | DAY | | "" | Tiền giường bệnh (theo ngày) · Room and board, per day |
| 100.1.1 | 100.1 | 2 | PER_DAY | 3.000.000 | 5.000.000 | 8.000.000 | 15 | DAY | | "" | Hồi sức tích cực (theo ngày) · Intensive care, per day |
| 100.2 | 100 | 1 | PER_EVENT | 20.000.000 | 40.000.000 | 80.000.000 | 0 | | | "" | Phẫu thuật (theo ca) · Surgery, per procedure |
| 100.3 | 100 | 1 | PER_EVENT | 3.000.000 | 5.000.000 | 8.000.000 | 0 | | | "" | Trước và sau nhập viện · Pre- and post-hospitalisation |
| 100.4 | 100 | 1 | PER_EVENT | 2.000.000 | 3.000.000 | 5.000.000 | 0 | | | "" | Vận chuyển cấp cứu · Emergency transport |
| 200 | | 0 | ANNUAL | 20.000.000 | 35.000.000 | 60.000.000 | 0 | | 2000 / PROVIDER_OFF_NETWORK | "2000"/"2000"/"1000" | Điều trị ngoại trú · Outpatient treatment |
| 200.1 | 200 | 1 | PER_VISIT | 1.500.000 | 2.500.000 | 4.000.000 | 0 | | | INHERIT | Khám và xét nghiệm chẩn đoán · Consultation and diagnostic tests |
| 200.2 | 200 | 1 | PER_VISIT | 1.000.000 | 1.500.000 | 2.500.000 | 0 | | | INHERIT | Thuốc kê đơn · Prescribed medication |
| 200.3 | 200 | 1 | PER_VISIT | 500.000 | 800.000 | 1.200.000 | 20 | VISIT | | INHERIT | Vật lý trị liệu · Physiotherapy |
| 300 | | 0 | ANNUAL | 5.000.000 | 8.000.000 | 15.000.000 | 0 | | 2000 / PROVIDER_OFF_NETWORK | "3000"/"3000"/"2000" | Nha khoa cơ bản · Dental — basic treatment |
| 300.1 | 300 | 1 | PER_VISIT | 1.000.000 | 1.500.000 | 2.500.000 | 0 | | | INHERIT | Điều trị nha khoa cơ bản (theo lượt) · Basic treatment, per visit |
| 400 | | 0 | ANNUAL | 20.000.000 | 35.000.000 | 60.000.000 | 0 | | | "" | Thai sản · Maternity |
| 400.1 | 400 | 1 | PER_EVENT | 12.000.000 | 20.000.000 | 35.000.000 | 0 | | | "" | Sinh thường · Normal delivery |
| 400.2 | 400 | 1 | PER_EVENT | 20.000.000 | 35.000.000 | 60.000.000 | 0 | | | "" | Sinh mổ · Caesarean section |
| 400.3 | 400 | 1 | PER_EVENT | 20.000.000 | 35.000.000 | 60.000.000 | 0 | | | "" | Biến chứng thai kỳ · Pregnancy complications |
| 500 | | 0 | ANNUAL | 50.000.000 | 80.000.000 | 150.000.000 | 0 | | | "" | Tai nạn — chi phí y tế · Accident — medical expenses |

CountLimit 0 ghi qua `COUNT_UNLIMITED()` (B4). Plan ↔ policy: HC-STD = `HC-2026-0001` · HC-PLS = `HC-2026-0002` · HC-PRM = `HC-2026-0003` (tra theo `Policy.PolicyCode`, không hard-code Id).

## B2.3 — `ClaimLine.Category` (nhập tay, PO bác nếu sai) và remap `BenefitCode`

`Seed_ClaimLines` ghi Category khi tạo; `Seed_BatchB2_Remap` điền Category cho dòng đang có **khi rỗng**, khớp theo (ClaimNo, Description nguyên văn seed).

| # | Claim | Description (seed) | Category | TreatmentType | → BenefitCode (A4 §4.5) |
|---|---|---|---|---|---|
| 1 | 0001 | Tiền giường bệnh phòng tiêu chuẩn (2 ngày) | ROOM | Inpatient | 100.1 |
| 2 | 0001 | Xét nghiệm công thức máu, sinh hóa máu | LAB | Inpatient | 100 |
| 3 | 0001 | Siêu âm ổ bụng tổng quát | IMAGING | Inpatient | 100 |
| 4 | 0001 | Thuốc điều trị nội trú theo y lệnh | DRUG | Inpatient | 100 |
| 5 | 0002 | Tiền giường bệnh phòng dịch vụ (1 ngày) | ROOM | Inpatient | 100.1 |
| 6 | 0002 | Xét nghiệm sinh hóa và nước tiểu | LAB | Inpatient | 100 |
| 7 | 0002 | Thuốc kháng sinh và giảm đau nội trú | DRUG | Inpatient | 100 |
| 8 | 0002 | Thực phẩm chức năng bổ sung vitamin | SUPPLEMENT | Inpatient | 100 |
| 9 | 0003 | Tiền phòng đơn (3 ngày) | ROOM | Inpatient | 100.1 |
| 10 | 0003 | Xét nghiệm tiền phẫu | LAB | Inpatient | 100 |
| 11 | 0003 | Chụp CT ổ bụng có cản quang | IMAGING | Inpatient | 100 |
| 12 | 0003 | Phẫu thuật nội soi cắt ruột thừa | PROCEDURE | Inpatient | 100.2 |
| 13 | 0003 | Thuốc gây mê và thuốc sau phẫu thuật | DRUG | Inpatient | 100 |
| 14 | 0004 | Khám nội tổng quát | CONSULT | Outpatient | 200.1 |
| 15 | 0004 | Xét nghiệm máu cơ bản | LAB | Outpatient | 200.1 |
| 16 | 0004 | Thuốc kê đơn ngoại trú | DRUG | Outpatient | 200.2 |

Phép suy (trong `Seed_BatchB2_Remap`, cho **mọi** ClaimLine có BenefitCode ∈ 5 mã cũ hoặc rỗng): `ServiceCategory` where `Value = ClaimLine.Category` → nếu `Claim.TreatmentTypeId = Inpatient` lấy `InpatientBenefit`, nếu `Outpatient` lấy `OutpatientBenefit`; TreatmentType khác, Category không khớp, hoặc cột đích rỗng → `"UNMAPPED"`. Dự kiến: 16/16 giải được, 0 UNMAPPED. Không đụng AmountClaimed/AmountPayable.

## B2.4 — `Seed_LimitLedger` và gỡ mã cũ (luật 4)

- `Seed_LimitLedger`: 5 mã gốc `100/200/300/400/500` × 3 member, UsedAmount 0, RemainingAmount 0 (như cũ — tiền là của chuỗi), upsert theo (MemberId, PolicyId, BenefitCode, PeriodYear).
- `Seed_BatchB2_Remap`, **sau** remap ClaimLine: xoá `PolicyBenefitSnapshot` có BenefitCode ∈ {INPATIENT_ROOM, INPATIENT_SURGERY, INPATIENT_OTHER, OUTPATIENT, DENTAL}; xoá `LimitLedger` cùng 5 mã **chỉ khi** UsedAmount = 0 và LastClaimId null; bỏ qua (và đếm) dòng còn tham chiếu.
- Dây: `BootstrapDemoData` (Seed_BenefitSnapshots → Seed_BenefitSchedule; cuối: Seed_BatchB2_Remap) và `RunSeedBatch0Now` (sau Seed_BatchB_Backfill: Seed_BenefitSchedule → Seed_BatchB2_Remap). `Seed_BenefitSnapshots` (5 mã cũ) **không còn được gọi** — giữ action để đợt F xoá.

## B2.5 — Readout kiểm dữ liệu (không có màn nào hiện được số dòng)

Server Action `BatchB_DataCheck` (read-only) → Administration › Providers, khối "Data check" (role ClaimOps): `BenefitWaitingPeriod` rows (kỳ vọng 51) · Claim có LossTypeId (12) · Snapshot rows theo plan (17/17/17) · Snapshot mã cũ còn lại (0) · ClaimLine UNMAPPED (0) · ClaimLine còn mã cũ (0) · LimitLedger mã cũ còn lại (0). Dùng để kiểm cả B lẫn B2 sau khi PO bấm "Re-run demo seed".

## Kiểm B2 xong (sau publish + Re-run demo seed)

1. Readout khớp kỳ vọng trên.
2. Policies & benefits: 17 dòng HC-STD với số A5 (100 = 100.000.000 …). PortalHome tile "Outpatient limit left" = 20.000.000 − used (số đổi là ĐÚNG). PortalBenefits 17 dòng.
3. LimitLedgerView: counters theo mã A5; "Consumption records" 200/200.1/200.2 vẫn còn.
4. ClaimDetail CLM-2026-0001 costs tab: dòng join được snapshot A5; settlement tab vẫn 2 nhãn cứng (đợt F).
5. `current-model.md` PolicyBenefitSnapshot 22.

## Kết quả thi hành — 21/09/2026 (Mentor session `4f3bf2d6…`, 1 read-only + 2 edit + 1 fix, 0 lỗi) → **ClaimMVP rev 135**

| Bước | Trạng thái | Ghi chú |
|---|---|---|
| B2.1 `PerDayCap`, `PerEventCap` + description AnnualLimit/PerVisitCap | ✅ | nullable |
| B2.2 `Seed_BenefitSchedule` + `Seed_BenefitSchedule_Upsert` (51 gọi) | ✅ | plan tra theo `Policy.PolicyCode`; CountLimit qua `COUNT_UNLIMITED()` |
| B2.3 `Seed_ClaimLines` ghi Category 16 dòng · `Seed_BatchB2_Remap` (a) backfill Category theo Description (b) suy BenefitCode qua ServiceCategory, fallback `UNMAPPED` (c) xoá snapshot 5 mã cũ (d) xoá ledger 5 mã cũ khi Used = 0 & không claim | ✅ | outputs Remapped/Unmapped/RetiredSnapshots/RetiredLedgerRows/SkippedLedgerRows |
| B2.4 `Seed_LimitLedger` → mã gốc 100–500, upsert | ✅ | Used/Remaining không đổi (0) |
| Dây | ✅ | `RunSeedBatch0Now`: Reset → AssessmentAndTerms → WaitingPeriods → BatchB_Backfill → **BenefitSchedule → BatchB2_Remap** → bed-line fixup → LogDone. `BootstrapDemoData`: cả hai nhánh If hội tụ ở PoliciesAndMembers → … → BenefitSchedule → LimitLedger → Claims → ClaimLines → BatchB_Backfill → BatchB2_Remap → End; **`Seed_BenefitSnapshots` không còn được gọi** (action giữ, đợt F xoá) |
| B2.5 `BatchB_DataCheck` + khối "Data check (batch B / B2)" trên Administration › Providers | ✅ | refresh sau toast "Seed re-run" |
| `current-model.md` | ✅ | rev 135: PolicyBenefitSnapshot **22** · Member 13 · Claim 21 · BenefitWaitingPeriod 5 · **rev 136: PolicyBenefitSnapshot 23 (+CapModeId), static CapMode** |

**Sự cố khi thi hành:** lượt edit B để `BootstrapDemoData` nhánh True thành `ClearDemoData → Seed_BenefitSnapshots → End` (xoá rồi không seed lại). Phát hiện bằng lượt read-only liệt kê node, sửa bằng một lượt riêng trước khi publish. Bài học: **sau mỗi lượt Mentor đụng dây flow, bắt Mentor liệt kê node sequence** — báo cáo "done" của nó không đủ.

**Chưa làm (chờ PO bấm "Re-run demo seed" lần 2):** kiểm readout + 5 màn blast radius (bảng trong `runtime-check-batchB.md`), chụp ảnh `shots-rev135`.

## § Kiểm số kịch bản B — tính tay 21/09 (C-PREP-2, không dùng Mentor)

**Đích trong deck / `specs/adjudication`:** `2.000.000 → 1.880.000` · dòng 4 `NOTCOV −80.000` · dòng 2 `CAP_VISIT −40.000`. PO: kịch bản B = hồ sơ **CLM-2026-0002** (nội trú ⇒ không đồng chi trả, build-00 §5 xung đột 5), 4 dòng, dòng 4 SUPPLEMENT.

### Dữ liệu seed của CLM-2026-0002 (sau B2)

| # | Dòng | Category | Số tiền | Mã A5 (IP) | Giới hạn A5 áp được |
|---|---|---|---|---|---|
| 1 | Tiền giường bệnh phòng dịch vụ (1 ngày) | ROOM | 800.000 | 100.1 | trần **ngày** 1.500.000 (STD) / 2.500.000 (PLS) |
| 2 | Xét nghiệm sinh hóa và nước tiểu | LAB | 400.000 | 100 | **không có trần dòng** — chỉ trần năm 100.000.000 / 200.000.000 |
| 3 | Thuốc kháng sinh và giảm đau nội trú | DRUG | 500.000 | 100 | không |
| 4 | Thực phẩm chức năng bổ sung vitamin | SUPPLEMENT | 300.000 | 100 | không (A4 §4.5 v1.3: **có chi trả**) |
| | **Tổng** | | **2.000.000** | | |

Hồ sơ thuộc **Trần Thị Bích · HC-2026-0002 · plan HC-PLS** (không phải HC-STD — màn Policies "Insured with claims").

### Chạy 14 bước bằng tay theo A5 HC-PLS, nội trú, in-network

| Bước | Kết quả |
|---|---|
| 4 term | TERM-HC-003 checkpoint trên dòng 4 (A5 §7.4) — nếu người xác nhận "không kê để điều trị" → `EXCL` −300.000. Theo lối PO chọn (AG-3 đề xuất → người xác nhận → chạy lại) mã là `NOTCOV` ở bước 6; dù mã nào, **cả dòng** −300.000 |
| 5 map | 1→100.1 · 2→100 · 3→100 · 4→100 |
| 8 trần dòng | 100.1: 800.000 ≤ 2.500.000/ngày → **0 cắt**. 100 không có trần/lần ⇒ dòng 2 **không thể** sinh CAP_VISIT |
| 10 đồng chi trả | 100 = "—" ⇒ **0** (khớp xung đột 5) |
| 11 ngoài mạng | chỉ trên 200/300 ⇒ n/a |
| 12–13 trần năm | 200.000.000 → không chạm |
| **Kết quả** | **Trước: 2.000.000 · Sau xác nhận dòng 4: 1.700.000** (−300.000 ở dòng 4, 0 ở dòng 2) |

### Kết luận: **KHÔNG khớp** — ba lý do độc lập

1. **`CAP_VISIT −40.000` ở dòng 2 không thể xảy ra**: dòng 2 là LAB → `100`, mà `100` không có trần/lần hay trần/ngày; trong nội trú chỉ `100.1` (giường, /ngày), `100.1.1` (ICU, /ngày), `100.2/100.3/100.4` (/ca) có trần dòng.
2. **`NOTCOV −80.000` ở dòng 4 ngụ ý dòng 4 = 80.000**; quyết định dòng là nhị phân (cả dòng), seed đang 300.000.
3. **Nguồn gốc con số 1.880.000 là prototype hồ sơ A — ngoại trú, có đồng chi trả 20 %** (`Prototype_Insurance_BnK_v0.5.html` dòng 1011–1029: 6 dòng, 3.128.000 yêu cầu; P1 = cắt trần 200.1 610.000 + 200.2 18.000, copay 500.000 → 2.000.000; P2 sau NOTREL dòng 4 168.000: 200.2 = 850.000 dưới trần, copay 470.000 → **1.880.000**). Số này **khớp A5 HC-STD ngoại trú** từng đồng (200.1 trần 1.500.000 · 200.2 trần 1.000.000 · co-pay 20 %). Chuyển kịch bản sang nội trú/không co-pay mà giữ 1.880.000 là điểm làm phép tính vỡ.

### Hai lối ra (PO chọn — cả hai rẻ hôm nay)

**A — đổi số tiền seed của CLM-2026-0002** (giữ tổng 2.000.000 và đích 1.880.000). Điều kiện bắt buộc: dòng 4 = **80.000**; một dòng vượt trần đúng 40.000. Trong nội trú, dòng vượt trần chỉ có thể là ROOM (100.1). Trên **HC-PLS** trần ngày 2.500.000 > tổng 2.000.000 ⇒ **bất khả**. Trên **HC-STD** (trần 1.500.000): `LAB 200.000 · ROOM 1.540.000 (1 ngày) · DRUG 180.000 · SUPPLEMENT 80.000` = 2.000.000 → payable 1.880.000 ✓ (−40.000 giường, −80.000 dòng 4). Kéo theo: (a) CLM-2026-0002 phải thuộc policy HC-STD — đổi member/policy của hồ sơ, hoặc đổi plan của HC-2026-0002 (mất HC-PLS trong demo → lịch quyền lợi còn 34 dòng); (b) để "dòng 2" là dòng bị cắt thì đảo thứ tự ROOM/LAB (bảng §B2.3 đã duyệt — thay đổi thứ tự, không đổi nội dung); (c) mã cắt là trần **/ngày** (`100.1` PER_DAY), deck ghi `CAP_VISIT` → cần đổi nhãn (CAP_DAY) hoặc chấp nhận.

**B — đổi số trong deck theo A5 với dữ liệu hiện có**: `2.000.000 → 1.700.000` · dòng 4 `NOTCOV −300.000` · **không có** cắt trần. Mất "hai loại khấu trừ" trên cùng hồ sơ; có thể minh hoạ cắt trần/lần bằng CLM-2026-0004 (ngoại trú HC-STD: 200.1 = 200.000+250.000 dưới trần → cũng không cắt; cần dòng LAB ≥ 1.250.000 mới cắt).

**Khuyến nghị:** lối **B** — không đụng dữ liệu/plan/thứ tự dòng đã duyệt, không kéo theo màn nào; deck sửa 3 con số. Nếu PO muốn giữ "2 khấu trừ trên 1 hồ sơ", lối A trên HC-STD với 4 số ở trên là bộ duy nhất thoả (đã thử: PLS bất khả).

### Ghi cho đợt C (PO 21/09)

- **SUPPLEMENT → `100` (IP) / `200.2` (OP) và CÓ CHI TRẢ** (A4 §4.5 v1.3, bốn lý do ở A5 §7.4). `NOTCOV` trên dòng 4 phải đến từ **AG-3 đề xuất → người xác nhận → chuỗi chạy lại**, không phải từ bảng mapping. **Bác mọi bước chuỗi làm SUPPLEMENT tự từ chối.** (Ghi thêm: theo A5 §7.4 mã "đúng chữ" cho TPCN loại trừ theo điều khoản là `EXCL` qua TERM-HC-003 bước 4; `NOTCOV` là lối bước 6. Cả hai đều qua người. Deck dùng NOTCOV — giữ, nhưng biết.)
- `CapMode` (OWN/INCLUDED) thêm ở C-PREP-1: chuỗi bước 8/12/13 đọc `CapModeId` trước, không bao giờ suy từ `AnnualLimit = 0`.

### Ghi cho đợt F / danh sách wording

- Khối **"Data check (batch B / B2)"** trên Administration: **giữ, không gỡ** (PO 21/09 — công cụ dry-run).
- **Wording (trước 29/09, nhãn "soạn cho hackathon"):** `COPAY` và `CAP_VISIT` đều nằm trong 4 causeCode chưa có câu A2 (build-00 §6). Hồ sơ hero 0001 hiện **cả hai** trên màn kết quả ("Vì sao Quý khách tự chịu") ⇒ trước giám khảo sẽ có **hai dòng lý do trống**. Phải soạn hai câu A2-style cho `COPAY` ("Đồng chi trả 20 % theo hợp đồng: {amount}") và `CAP_VISIT` ("Phần vượt trần {cap} mỗi lần khám của quyền lợi {benefit}: {amount}") — placeholder đã bind, dán nhãn "soạn cho hackathon" trong A2. Cùng lúc soát `DEDUCT`, `LIMIT_TOTAL`.
- Câu sân khấu (PO đã đưa vào build-00 §5.1): *loại một dòng 168.000 chỉ tiết kiệm cho công ty 120.000 — trần và đồng chi trả hấp thụ 48.000; phép trừ thường ra 168.000, chỉ chuỗi thật mới ra 120.000.*

### Đợt B3 — hồ sơ 0001 thành hero ngoại trú (PO duyệt 21/09, rev 137)

PO duyệt 0001 (0004 giữ vai ExtractionError). Đổi: `TreatmentTypeId` Outpatient · `LossTypeId` **ILL_OP** (bảng B5.1 dòng 0001 sửa theo) · Admission/Discharge null · TreatmentFrom/To = EventDate · TotalClaimed 3.128.000 · 4 dòng → 6 dòng bảng §Lối C (Category, BenefitCode, Confidence prototype). Đường re-run: `Seed_BatchB3_Claim0001` chạy **sau Reset, trước Seed_AssessmentAndTerms** (recommendation gắn ordinal [3] = Atorvastatin — không đổi cơ chế); bỏ bed-line fixup A trong `RunSeedBatch0Now`; claim C ordinal [3] `SURGERY` → `PROCEDURE` (nguyên nhân 1/2 dòng UNMAPPED). Readout thêm danh sách dòng UNMAPPED.

**Kết quả lượt Mentor B3 (21/09, 0 lỗi validation, 64 warning có sẵn):**
- `Seed_Claims` 0001: `TreatmentTypeId` Outpatient · `LossTypeId` IllnessOutpatient · Admission/Discharge `NullDate()` · TreatmentFrom/To = EventDate · TotalClaimed 3.128.000 · IsInNetwork True.
- `Seed_ClaimLines`: 4 cặp assign+create cũ xoá, 6 dòng mới đúng bảng §Lối C (CONSULT 300.000/0.97 · LAB 1.400.000/0.94 · IMAGING 410.000/0.96 · DRUG Atorvastatin 168.000 qty 30/0.91 · DRUG Ciprofloxacin 700.000/0.93 · SUPPLEMENT 150.000/0.88), BenefitCode 200.1/200.1/200.1/200.2/200.2/200.2, ServiceDate 10/06/2026.
- `Seed_AssessmentAndTerms`: `GetLinesA4` max 6; ordinal [0]–[3] CONSULT/LAB/IMAGING/DRUG, thêm [4] DRUG/0.93 và [5] SUPPLEMENT/0.88 trước `LogCLinesA`; claim C ordinal [3] `SURGERY` → `PROCEDURE`.
- Mới `Seed_BatchB3_Claim0001`: đọc 0001 + dòng; nếu đã 6 dòng và dòng đầu "Tiền khám" → thoát (idempotent); ngược lại xoá hết dòng cũ, tạo 6 dòng mới, cập nhật header. Output `LinesReplaced`.
- `RunSeedBatch0Now` — chuỗi node sau sửa (rule 8, Mentor liệt kê): 1 CallReset → **2 CallBatchB3** (mới) → 3 CallSeed (AssessmentAndTerms) → 4 CallWaitingPeriods → 5 CallBatchBBackfill → 6 CallBenefitSchedule → 7 CallBatchB2Remap → 8 GetBedLineB→Assign→UpdBedB → 9 GetBedLineC→Assign→UpdBedC → 10 LogDone → 11 End. Ba node `GetBedLineA→Assign→UpdBedA` (đóng dấu ROOM lên dòng đầu 0001) đã xoá; `CallBatchB2Remap` nối thẳng `GetBedLineB`.
- `Seed_BatchB2_Remap`: biểu thức Category theo mô tả đổi sang 6 mô tả mới của 0001; 4 mô tả nội trú cũ gỡ.
- `Seed_BatchB_Backfill`: nhánh IllnessInpatient thu hẹp còn 0002/0003; nhánh mới 0001 → IllnessOutpatient. TreatmentFrom/To giữ nguyên (Admission null ⇒ EventDate).
- `BatchB_DataCheck`: aggregate `GetUnmappedLines` (ClaimLine ⋈ Claim, BenefitCode = "UNMAPPED", cột `Row = ClaimNo + " / " + Description`), ForEach ghép `"; "` → output `UnmappedLines` (Text); Administration thêm dòng "Unmapped line details" = `If(UnmappedLines = "", "—", UnmappedLines)`.
- **Rev 138 (sửa sau seed #4):** `Seed_BatchB2_Remap.GetCodeLines` thêm `or ClaimLine.BenefitCode = "UNMAPPED"` (dòng đã đóng dấu UNMAPPED được tính lại mỗi lần — idempotent); `Seed_AssessmentAndTerms` claim D `GetLinesD.List[0]` Category `"ROOM"` → `"CONSULT"`. Readout seed #5 = 0 UNMAPPED. Chi tiết nguyên nhân: `runtime-check-batchB.md`.
- Lượt phụ: `CapMode` đặt Public để MyInsurance dùng `Entities.CapMode.Own` (bỏ `IntegerToIdentifier(1)` — sentinel, README §7 luật 9; danh sách records của CapMode là ["Included","Own"] nên số 1 có thể trỏ nhầm Included).

## Kết quả kiểm sau lần bấm seed #2 (rev 135) và sửa — 21/09 10:30–11:00

| Phát hiện | Bằng chứng | Xử lý |
|---|---|---|
| **Lần bấm #2 chưa chạy trên rev 135.** Log app: seed duy nhất trong ngày là 02:05Z (09:05, rev 134 — lần #1); runtime chỉ nhận rev 135 lúc 03:34Z. Policies vẫn 5 dòng cũ | `app_logs` ClaimMVP 4 h: `RunSeedBatch0Now done` 02:05:25Z (assetVersion 134); `VersionsManager … upgrade` 03:34:09Z | PO bấm lại trên rev 136 |
| **Readout hiện `1` ở mọi ô** | 9 phép gán đều đọc `GetX.Count` = số dòng của aggregate (luôn 1 vì Max Records 1 + cột Count), không đọc cột `WPCount`… | Sửa: `GetX.List.Current.<Col>` (rev 136). Mentor rà cả app: không chỗ nào khác mắc cùng lỗi (`GetExisting.Count > 0` trong Upsert là kiểm tồn tại, đúng) |

### C-PREP-1 — sentinel số 0 thứ hai (`AnnualLimit = 0` "included" ⟂ `CountLimit = 0` "unlimited") — rev 136

- Static `CapMode`: `Own` (OWN · "Trần năm riêng") · `Included` (INCLUDED · "Tính vào trần năm của quyền lợi cha"). Description ghi build-01 §8.1.
- `PolicyBenefitSnapshot.CapModeId` (nullable) — 5 gốc/plan = Own, 12 con/plan = Included, **truyền tường minh trong cả 51 lời gọi** `Seed_BenefitSchedule_Upsert` (input mới). Description `AnnualLimit` đổi: "Annual cap when CapModeId = OWN. Meaningless when INCLUDED — read CapModeId first, never the number."
- Readout thêm "Schedule rows without CapMode (expect 0)".
- **Đợt C:** bước 8/12/13 đọc `CapModeId` trước; **bác** mọi biểu thức `AnnualLimit = 0 ⇒ included`.

### Lối C — kịch bản A của prototype (ngoại trú HC-STD, 6 dòng) chạy tay trên cây A5 đã seed — 21/09 11:10, ≤30 phút, không Mentor

**Bác lối A (PO 21/09):** build-01 §4.23 có đúng 8 causeCode, không có mã trần/ngày → lối A giữ tổng nhưng phá khối "Vì sao Quý khách tự chịu". **Chưa lấy lối B**: 1.880.000 khớp A5 HC-STD ngoại trú từng đồng — kịch bản đúng spec tồn tại, chỉ gắn nhầm hồ sơ.

**Dữ liệu (prototype hồ sơ A, `Prototype_Insurance_BnK_v0.5.html` 1011–1029):** HC-STD · ngoại trú · in-network · N39.0 · một lần khám (một cơ sở, một ngày ⇒ **một visit**, BR-LEDGER-14) · tổng yêu cầu 3.128.000.

| # | Dòng | Category | Số tiền | Mã A5 (OP, A4 §4.5) |
|---|---|---|---|---|
| 1 | Tiền khám | CONSULT | 300.000 | 200.1 |
| 2 | Xét nghiệm nước tiểu, cấy khuẩn | LAB | 1.400.000 | 200.1 |
| 3 | Siêu âm hệ tiết niệu | IMAGING | 410.000 | 200.1 |
| 4 | Atorvastatin 20mg — 30 viên | DRUG | 168.000 | 200.2 |
| 5 | Ciprofloxacin 500mg và thuốc kê đơn | DRUG | 700.000 | 200.2 |
| 6 | Men vi sinh (thực phẩm chức năng) | SUPPLEMENT | 150.000 | 200.2 (A4 v1.3: có chi trả) |

**A5 HC-STD dùng đến:** `200` trần năm 20.000.000 · co-pay **2000 bp** (CopayPct "2000"; 200.1/200.2 INHERIT) · `200.1` PER_VISIT **1.500.000** · `200.2` PER_VISIT **1.000.000** · khấu trừ ngoài mạng 20 % chỉ khi PROVIDER_OFF_NETWORK (không áp — in-network). Làm tròn đồng nguyên từng bước — mọi số dưới đây là bội của 1.000, không phát sinh làm tròn.

**Lần chạy 1 — trước khuyến nghị lâm sàng**

| Bước | Tính | Kết quả |
|---|---|---|
| 4 term | không term nào (TERM-HC-003 chưa mở — clause ref TBC-) | 3.128.000 |
| 5 map | 200.1 = 300.000 + 1.400.000 + 410.000 = **2.110.000** · 200.2 = 168.000 + 700.000 + 150.000 = **1.018.000** | 3.128.000 |
| 6 dòng | không dòng nào loại | 3.128.000 |
| 8 trần/lần | 200.1: 2.110.000 → 1.500.000 (**CAP_VISIT −610.000**) · 200.2: 1.018.000 → 1.000.000 (**CAP_VISIT −18.000**) | 2.500.000 |
| 10 co-pay | 20 % × 2.500.000 = **COPAY −500.000** | **2.000.000** |
| 11 ngoài mạng | in-network → bỏ qua | 2.000.000 |
| 12–13 trần năm | 2.000.000 ≤ 20.000.000 còn lại | 2.000.000 |
| 14 sổ | tạm: 200.1 **1.200.000** · 200.2 **800.000** → gộp lên 200 | còn 18.000.000 |

**Lần chạy 2 — sau khi người xác nhận đề xuất AG-3 Clinical (văn bản cũ ghi "AG-5" theo plan-5-8-9; chuẩn `build-01` §4.27, sửa 21/09) trên dòng 4 (Atorvastatin — thuốc hạ mỡ máu, không liên quan N39.0)**

| Bước | Tính | Kết quả |
|---|---|---|
| 6 dòng | dòng 4 **NOTREL −168.000** (mã dòng, lối bước 6 — người xác nhận) | 2.960.000 |
| 8 trần/lần | 200.1: 2.110.000 → 1.500.000 (−610.000) · 200.2: **850.000 ≤ 1.000.000 → không cắt** | 2.350.000 |
| 10 co-pay | 20 % × 2.350.000 = **−470.000** | **1.880.000** |
| 14 sổ | 200.1 1.200.000 · 200.2 **680.000** | còn 18.120.000 |

**Khớp từng đồng** với prototype P1/P2 (`payable 2000000 / 1880000`, `b1.pay 1200000`, `b2.pay 800000 → 680000`, `cut8 628000 → 610000`, `copay 500000 → 470000`) và với A5 §4.2 HC-STD. **Ba loại khấu trừ trên một hồ sơ, đều có mã trong 8 causeCode:** `COPAY` (500.000 → 470.000) · `CAP_VISIT` (628.000 → 610.000) · **`NOTREL`** (168.000). Chênh trước/sau = 120.000 = 168.000 − 18.000 (bớt cắt trần 200.2) − 30.000 (bớt co-pay) — một câu đáng nói trên sân khấu: loại một dòng 168.000 chỉ tiết kiệm cho công ty 120.000.

**⚠ Mã dòng là `NOTREL`, không phải `NOTCOV`.** Atorvastatin là thuốc thật, "không liên quan chẩn đoán" (NOTREL); NOTCOV là "không phải khoản y tế được chi trả". Deck / `specs/adjudication` / `chain-14-steps-a5/tasks.md` task 7 hiện ghi NOTCOV — sửa thành NOTREL cùng lúc sửa hai số kia. (Ruling NOTCOV-vs-EXCL của PO là về TPCN, không đổi.)

**Hồ sơ nào chứa kịch bản — cần PO quyết, hai ứng viên:**

| | CLM-2026-**0001** (khuyến nghị) | CLM-2026-**0004** (PO đề xuất) |
|---|---|---|
| Đang mang gì | Header **đã** là hồ sơ A: `TotalClaimed 3.128.000`, `TotalPayable 2.000.000`, N39.0, Nguyễn Văn An HC-STD, "Proposal pending"; **Recommendation AG-3 (seed có thể còn ghi "AG-5" — đổi tên, `agent-architecture` việc 9) + 4 RecEvidence + Assessment + ChainRun + checkpoint T-07** đều gắn vào 0001 (`Seed_AssessmentAndTerms` ordinal A) | Vai **ExtractionError**: WorkList "Document processing error", OpsQueue incident "IDP errorCode MISSING", AgentRun lỗi — showcase luật kiến trúc #4 |
| Phải đổi | `TreatmentTypeId` Inpatient→Outpatient, **`LossTypeId` ILL_IP→ILL_OP (1 dòng bảng B5.1)**, Admission/Discharge → null, 4 dòng → 6 dòng prototype (giữ Vinmec hoặc đổi Medlatec — không ảnh hưởng số) | Chuyển cả Recommendation/Evidence/Assessment/ChainRun/checkpoint từ 0001 sang 0004 (sửa `Seed_AssessmentAndTerms` theo ordinal), **mất hồ sơ lỗi IDP** hoặc phải dựng hồ sơ lỗi khác |
| Nội trú còn lại | 0002 (Paid) · 0003 (low-confidence) | 0001 · 0002 · 0003 |
| Sai lệch thật so với prototype | 0001 bị chép nhầm thành nội trú 4 dòng (1.800.000) trong khi header vẫn 3.128.000/2.000.000 — **đây là chỗ chép nhầm** | — |

**Kết luận C-PREP-2: lối C khớp — lấy lối C.** Đích mới cho deck + `specs/adjudication` + `chain-14-steps-a5` task 7: **`2.000.000 → 1.880.000` · dòng 4 `NOTREL −168.000` · `CAP_VISIT` 628.000 → 610.000 · `COPAY` 500.000 → 470.000** (ngoại trú HC-STD, 6 dòng, tổng 3.128.000). Việc seed (đợt B3, một lượt Mentor, sau khi PO chọn hồ sơ): thay 4/3 dòng bằng 6 dòng trên, Category theo bảng, số tiền nguyên văn; không đụng tiền của chuỗi (`AmountPayable`, ledger do chuỗi ghi khi chạy).


### Đợt B4 — sửa F-4 + C-1 trước khi mở chuỗi (PO 21/09, rev 139)

PO: cả hai lộ trên hồ sơ hero; chuỗi thật chỉ thay text bước `CalcSnapshot`, **không** thay `Recommendation.Title/Rationale` (việc của AG-3, ngoài phạm vi) ⇒ text seed phải đúng ngay.

- **F-4** — ClaimDetail › General "Treatment dates": biểu thức đọc `AdmissionDate/DischargeDate` (null sau B3) → đọc `TreatmentFrom/TreatmentTo`: `If(TreatmentFrom = NullDate(), "—", FormatDateTime(TreatmentFrom,"dd/MM/yyyy") + If(TreatmentTo <> NullDate() and TreatmentTo <> TreatmentFrom, " – " + FormatDateTime(TreatmentTo,"dd/MM/yyyy"), ""))`. Một biểu thức, không đụng header/kv3.
- **C-1** — `Seed_AssessmentAndTerms` recommendation hồ sơ A (gắn ordinal [3] = Atorvastatin): text nguyên văn D3 §I-16 mẫu + A5 §8.2 "Unrelated medication":
  - `Code` = `NOTREL` (A1 §5.2, D3 I-16: chỉ NOTREL/NOTCOV)
  - `Title` = `Atorvastatin 20mg — 30 viên: 168.000 ₫ — thuốc hạ mỡ máu, không liên quan chẩn đoán N39.0`
  - `Rationale` = `Atorvastatin là thuốc hạ mỡ máu, dùng để điều trị rối loạn lipid máu; không liên quan đến chẩn đoán nhiễm khuẩn đường tiết niệu (N39.0). Bằng chứng: kb/lipid-lowering-agents/indications — nhóm statin chỉ định cho rối loạn lipid máu và dự phòng bệnh tim mạch; chẩn đoán N39.0.`
  - `Confidence` = 0.91 · `Band` = `HIGH`
  - Mọi chuỗi "nội trú"/"antibiotic course"/"discharge note" trong seed của hồ sơ A (kể cả "To do: Decide the proposal on line 6" nếu là chuỗi cứng → dòng 4) phải đi.
- Không đụng chuỗi bước `CalcSnapshot` (đợt C thay).

**Kết quả B4 (rev 139, 2 lượt Mentor — lượt giữa hỏng vì OS-AIWA-50002, không mất gì; 0 lỗi):**
- F-4: ClaimDetail › General "Treatment dates" (`expression10` trong `ClaimDl`) — trước đọc Admission/Discharge, sau đọc TreatmentFrom/To đúng biểu thức trên.
- C-1: Recommendation hồ sơ A: `Code` REC-CLIN-04 → **NOTREL** (màn hiển thị Code làm badge — đúng nghĩa mã lý do) · Title/Rationale/Confidence 0.62→0.91/Band Med→HIGH như spec · RecEvidence: DOC "Discharge summary p.2" → DIAGNOSIS "N39.0 — Phiếu khám bệnh (T1-03)"; TERM "Term T-07" → KB_ENTRY "kb/lipid-lowering-agents/indications"; KB_ENTRY "Formulary v4 §3.1" giữ · Assessment A LinesTotal/LinesAssessed 4 → 6 · to-do "line 6" là `IntegerToText(GetLines.List.Length)` (đếm tổng dòng!) → biến `RecLineOrdinal` đếm dòng có Id ≤ ClaimLineId của recommendation ⇒ "line 4".
- Không đổi (ngoài phạm vi, ghi cho đợt F): `TermRegister` T-07 "Antibiotic course > 5 days needs clinical review" (term demo của register, không gắn hồ sơ A nữa); text bước `CalcSnapshot` (đợt C thay).

## Đóng mâu thuẫn số học `build-04` §4.3 (PO 21/09 chiều)

**CLOSED bằng hand-run lối C** (§Kiểm số ở trên): **2.000.000 là PAYABLE của run 1, không phải số billed.** Billed của hồ sơ 0001 là **3.128.000**; đồng chi trả 20 % ngoại trú **có áp** (−500.000 ở run 1 / −470.000 ở run 2). Chuỗi `build-04` §4.3 từng đọc 2.000.000 như số billed nên cộng trừ không khớp — cách đọc đó bỏ. Đích deck giữ: `2.000.000 → 1.880.000`.
