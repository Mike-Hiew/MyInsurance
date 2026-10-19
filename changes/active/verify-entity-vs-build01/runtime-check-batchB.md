# Runtime check — đợt B (rev 134) và B2 (rev 135) · 21/09/2026

## ⚠ ĐỌC TRƯỚC KHI SO ẢNH: số tiền trên màn ĐỔI LÀ ĐÚNG (quyết PO 21/09, batchB2-spec luật 2)

Trước B2, mọi số quyền lợi trên màn là số prototype bịa (5 mã `INPATIENT_ROOM … DENTAL`, PortalHome "1.000.000 / 3.000.000"). B2 seed cây A5 thật (17 quyền lợi × 3 plan). Sau khi PO bấm "Re-run demo seed" trên rev 135:

| Màn | Trước (rev ≤134) | Sau (rev 135 + seed) | Đọc là |
|---|---|---|---|
| Policies & benefits (HC-STD) | 5 dòng, DENTAL 2.000.000 / OUTPATIENT 3.000.000 … | **17 dòng**, 100 = 100.000.000 · 200 = 20.000.000 · 300 = 5.000.000 · 400 = 20.000.000 · 500 = 50.000.000 + 12 dòng con | ✅ đúng |
| PortalHome tile "Outpatient limit left" | 1.000.000 / 3.000.000 | **… / 20.000.000** (used đọc từ LimitLedger `200` do chuỗi ghi) | ✅ đúng |
| PortalBenefits | 5 dòng | 17 dòng, tên VI tạm | ✅ đúng (wording chờ soát) |
| LimitLedgerView counters | 5 mã cũ, used 0 | mã A5; `200/200.1/200.2` hiện used từ postings CLM-2026-0001 | ✅ đúng |
| ClaimDetail › Settlement plan | 2 nhãn cứng INPATIENT_ROOM / INPATIENT_OTHER | **không đổi** (nhãn hiển thị cứng trong `BuildBenefitRows`, đợt F) | ⏸ biết trước |
| ClaimDetail › Extraction & costs | join snapshot theo mã cũ | dòng join được snapshot A5 (100.1 / 100 / 100.2 / 200.1 / 200.2) | ✅ đúng |

Ảnh so sánh: `output/bnk-dev-claimmvp/shots-rev88/` là **trước** A5 — khác số ở các màn trên là kỳ vọng, không phải hồi quy.

## Đợt B — kiểm sau khi PO bấm seed lần 1 (rev 134, 21/09 ~10:00)

- 17 màn render: console WorkList · Dashboard · Policies · LimitLedgerView · Checkpoints · OpsQueue · TermRegisterList · AgentTrace · Administration · ClaimDetail (8 tab, Id 17) — portal PortalHome · PortalClaims · PortalBenefits · PortalNotifications · PortalNewClaim · PortalSupplement · PortalClaimDetail (Id 34). Không lỗi console. Số tiền chưa đổi (đúng cho B).
- 51 dòng `BenefitWaitingPeriod` và 12 `Claim.LossTypeId`: **không có màn nào hiện được**; `db_query` MCP v1 không trả dòng → thêm readout `BatchB_DataCheck` trên Administration ở rev 135. **Kiểm sau lần bấm seed thứ 2.**

## Đợt B2 — seed trên rev 136, 21/09 11:2x (Claude bấm theo uỷ quyền PO — hai lần bấm của PO không tới server)

Readout sau toast "Seed re-run": **51 · 12 · 17 / 17 / 17 · 0 · 2 · 0 · 0 · 0**. Một lệch: **2 dòng chi phí `UNMAPPED`** (kỳ vọng 0) — đang truy (Mentor read-only: `Seed_AssessmentAndTerms` "claim A lines updated (conf+category)" là nghi phạm).

| Màn | Kết quả | |
|---|---|---|
| Policies & benefits HC-STD | 17 dòng A5: 100 = 100.000.000 · 200 = 20.000.000 (co-pay 20 %) · 200.1 per-visit 1.500.000 · 200.2 1.000.000 … | ✅ |
| LimitLedgerView (Nguyễn Văn An) | counters theo mã A5; 200 used 2.000.000 · 200.1 1.200.000 · 200.2 800.000 (postings chuỗi của hồ sơ A) | ✅ |
| ClaimDetail 0001 › Extraction & costs | 4 dòng render, Category ROOM/LAB/IMAGING/DRUG | ✅ |
| **PortalHome** tile "Outpatient limit left" | **"0 đ of 0 đ/yr"**, Main benefits **rỗng** | ❌ **hồi quy** — MyInsurance đọc entity public của ClaimMVP với mã cũ; Mentor rà mã cũ chỉ trong ClaimMVP |
| PortalBenefits | chưa chụp được (renderer treo) — cùng nguyên nhân | ⏳ |

Xử lý: sửa MyInsurance (đợt B2-portal, một lượt Mentor + publish rev 51): tile = dòng `200`, Main benefits = 5 gốc (Depth 0), PortalBenefits = 17 dòng.

**Lượt Mentor MyInsurance (21/09, 0 lỗi validation, 37 warning có sẵn):** `GetOutpatientBenefit` filter `"OUTPATIENT"` → `"200"` · `GetBenefitsHome` join `LimitLedger.BenefitCode = PolicyBenefitSnapshot.BenefitCode` (bỏ 3 cặp cầu mã cũ), filter `PolicyId` + `CapModeId = Own` + `BenefitCode IN ("200","100","300")`, SortOrder 200→1, 100→2, còn lại 3 · xoá `GetLedger200` (không gì bind) · `PortalBenefits.GetBenefits` join đẳng thức mã, `LimitExpr`/`LeftExpr` đọc `CapModeId` (INCLUDED ⇒ "within parent"/"—"), `CopayExpr` đọc `CopayPct` Text ("" ⇒ "—", "INHERIT" ⇒ "as parent", số bp ⇒ `/100 %`), `PerVisitExpr` theo `LimitType` (PER_VISIT/PER_DAY/PER_EVENT → PerVisitCap/PerDayCap/PerEventCap), cột "Per visit" → "Per visit / day / event" · rà: không còn literal `INPATIENT_ROOM/INPATIENT_SURGERY/INPATIENT_OTHER/OUTPATIENT/DENTAL` trong MyInsurance.
**Lệch cần sửa trước publish:** Mentor so `CapModeId = IntegerToIdentifier(1)` vì `CapMode` chưa public bên ClaimMVP — số 1 là sentinel (README §7 luật 9). Xử lý: ClaimMVP đặt `CapMode` Public (rev 137), MyInsurance refresh reference và thay bằng `Entities.CapMode.Own` trước khi publish.


## Kiểm sau seed #4 trên rev 137 (B3) + MyInsurance rev 51 — 21/09 11:20–11:50

Seed: tôi bấm (PO uỷ quyền), toast "Seed re-run", log `RunSeedBatch0Now done` 04:19:09Z assetVersion 137 (runtime nhận rev 137 lúc 04:18:42Z — đã warm trước khi bấm). Readout: **51 · 12 · 17/17/17 · 0 · 2 · 0 · 0 · 0** — vẫn 2 dòng UNMAPPED: `CLM-2026-0003 / Phẫu thuật nội soi cắt ruột thừa` · `CLM-2026-0004 / Khám nội tổng quát`.

**Nguyên nhân (Mentor read-only, đã đối chiếu):**
1. 0003 dòng phẫu thuật: lần seed rev 136 (Category còn `SURGERY`, không có trong ServiceCategory) đã đóng dấu `UNMAPPED`; `Seed_BatchB2_Remap.GetCodeLines` chỉ lọc 5 mã cũ + `""` nên dòng đã `UNMAPPED` **không bao giờ được tính lại** (remap không idempotent với UNMAPPED). `Seed_ResetDemoClaimData` không reset `BenefitCode`.
2. 0004 dòng đầu "Khám nội tổng quát": `Seed_AssessmentAndTerms` đóng dấu `Category = "ROOM"` lên `GetLinesD.List[0]` (hack giường bệnh áp cả cho hồ sơ D — ngoại trú) → `ServiceCategory.ROOM.OutpatientBenefit = ""` → UNMAPPED. Đúng phải là `CONSULT` → 200.1.

**Sửa (rev 138, một lượt Mentor, 0 lỗi):** `GetCodeLines` thêm `or ClaimLine.BenefitCode = "UNMAPPED"`; claim D ordinal [0] `"ROOM"` → `"CONSULT"`. Cần bấm seed lại.

| Màn | Kết quả | |
|---|---|---|
| ClaimDetail 0001 (Id 17) header | Outpatient · N39.0 · HC-STD · 10/06/2026 · Total claimed 3.128.000 · Proposed 2.000.000 · "To do: Decide the proposal on line 6 (Atorvastatin…)" | ✅ |
| ClaimDetail 0001 › Extraction & costs | 6 dòng CONSULT/LAB/IMAGING/DRUG/DRUG/SUPPLEMENT, số tiền và conf đúng bảng §Lối C; recommendation REC-CLIN-04 gắn Atorvastatin 168.000 | ✅ |
| ClaimDetail 0004 (Id 20) | "Document processing error", Proposed —, K04.0 dental 15/01/2026 (trong 90 ngày chờ nha khoa) — showcase luật 4 giữ nguyên | ✅ |
| LimitLedgerView (Nguyễn Văn An) | 17 counter A5; postings 0001: 200 = 2.000.000 · 200.1 = 1.200.000 · 200.2 = 800.000 (khớp run #1 tính tay) | ✅ |
| Policies & benefits HC-STD | 17 dòng, 200 cap 20.000.000 co-pay 20 %, 300 co-pay 30 %, per-visit 200.1/200.2/200.3/300.1 | ✅ |
| **PortalHome** (MyInsurance rev 51) | tile **18.000.000 ₫ of 20.000.000 ₫/yr** · Main benefits: Ngoại trú left 18.000.000 · Nội trú 100.000.000 · Nha khoa 5.000.000 | ✅ hồi quy đã sửa |
| **PortalBenefits** (rev 51) | 17 dòng cây A5, con hiện "within parent"/"as parent", co-pay 20 %/30 %, cột "Per visit / day / event" đọc PerVisit/PerDay/PerEventCap | ✅ |

**Phát hiện thêm (không sửa trong B — ghi cho đợt F / C):**
- F-1 **PortalBenefits "Applies from" = 01/01/2026 cho MỌI dòng** kể cả nha khoa (90 ngày) và thai sản — cột không đọc `BenefitWaitingPeriod`; chú thích cuối trang ("Dental waits 90 days so it applies later") tự mâu thuẫn. Console Policies cột "Wait" cũng toàn "—". Sửa: join `BenefitWaitingPeriod` (plan + code) → `CoverStart + WaitingDays`.
- F-2 Console Policies cột "Per visit" trống với quyền lợi PER_DAY/PER_EVENT (100.1 1.500.000/ngày, 100.2 20.000.000/ca…) — cần biểu thức theo `LimitType` như portal đã làm.
- F-3 ClaimDetail 0001 › Calculation & ledger: bảng "Event / Benefit" vẫn hiện 2 dòng `INPATIENT_ROOM` / `INPATIENT_OTHER` 10.000.000 — nguồn KHÔNG phải LimitLedger (readout legacy ledger = 0); là dữ liệu seed của một entity khác (nghi `ClaimEventBenefit`/bảng usage theo event) chưa remap. Chuỗi thật (đợt C) sẽ thay; nếu không, remap ở F.
- F-4 ClaimDetail 0001: "Treatment dates —" (0004 hiện đủ) · Event SK-1 detail: "Admission 01/01/1900", "Event date"/"Discharge" bind sai trường (hiện ICD / tổng tiền) — lỗi bind màn có sẵn, lộ ra khi Admission null. Đang truy phần dữ liệu (TreatmentFrom/To).
- C-1 Chuỗi seed strings trên 0001 (step 4 "4 lines → CONSULT/LAB/IMAGING/DRUG", step 7 "Per-visit cap 500.000", recommendation title "Thuốc điều trị nội trú theo y lệnh", history "1 NOTREL recommendation — line 4") là text seed của hồ sơ cũ — chuỗi thật + AG-3 thật thay ở đợt C; recommendation title phải nói Atorvastatin/NOTREL (N39.0 ⟂ statin).
- Ghi chú kỹ thuật: `Seed_ResetDemoClaimData` vẫn đóng dấu `Category = "ROOM"` lên dòng đầu của A/B/C (kể cả 0001 "Tiền khám") — `Seed_AssessmentAndTerms` chạy sau ghi đè lại CONSULT nên màn đúng, nhưng thứ tự node là điều kiện sống còn (rule 8).

### Seed #5 trên rev 138 — 21/09 11:27 · **đợt B đóng**

Runtime nhận rev 138 lúc 04:27:08Z; tôi bấm seed, log `RunSeedBatch0Now done` 04:27:46Z (assetVersion 138). Readout: **51 · 12 · 17/17/17 · 0 · 0 · 0 · 0 · 0 · Unmapped line details "—"** — đúng đích. ClaimDetail 0001 sau lần seed thứ hai vẫn 6 dòng (CONSULT/LAB/IMAGING/DRUG·Proposal/DRUG/SUPPLEMENT), Total claimed 3.128.000 — `Seed_BatchB3_Claim0001` idempotent như thiết kế. Năm màn blast-radius (Policies · LimitLedger · ClaimDetail costs · PortalHome · PortalBenefits) ✅ ở bảng trên (rev 137/51; rev 138 chỉ đổi 2 biểu thức seed, không chạm màn).

**Trạng thái cuối đợt B: ClaimMVP rev 138 · MyInsurance rev 51.** Việc tiếp theo: `chain-14-steps-a5`.

### B4 trên rev 139 — 21/09 12:00

Runtime nhận rev 139 04:58:16Z; seed #6 `RunSeedBatch0Now done` 04:59:01Z (assetVersion 139). ClaimDetail 0001:
- General "Treatment dates" = **10/06/2026** (F-4 ✅)
- To-do: "Decide the proposal on **line 4** (Atorvastatin 20mg — 30 viên)" ✅ (trước: "line 6" = `GetLines.List.Length`)
- Proposal card: **NOTREL · 91 %** · title "Atorvastatin 20mg — 30 viên: 168.000 ₫ — thuốc hạ mỡ máu, không liên quan chẩn đoán N39.0" · rationale D3 I-16 nguyên văn (C-1 ✅)
Còn F-1/F-2/F-3 (đợt F) và text bước `CalcSnapshot` (đợt C).
