# A5 §6.1 — chuẩn của chuỗi (trích từ `build-04` §1.1, Project claude.ai)

> Chép xuống đĩa 21/09 để Claude Code không phải suy lại. **Nguồn:** Project doc `build-04-chuoi-tinh-tien-va-seed` §1.1, §1.3, §1.4.

## Bốn chỗ BRD sai — đều làm đổi tiền

| # | Khác biệt | BRD (**sai**) | A5 §6.1 / D1 §5.3 (**chuẩn**) |
|---|---|---|---|
| **X-1** | Khấu trừ BHYT | `CH-06 SocialInsuranceDeduction` | **Không có bước này.** S-17 (17/08/2026): MVP 1 **capture BHYT nhưng không trừ**. Chuỗi chạy trên số **billed**. Vẫn lưu `PaidBySocialInsurance` để hiển thị |
| **X-2** | Deductible | `CH-10 Deductible` | **Ngoài phạm vi.** Sản phẩm pilot không có. Slot tương lai nằm **giữa bước 9 và 10** |
| **X-3** | Làm tròn | `CH-12 Rounding` — *round down to 1,000 VND* | **Làm tròn ở MỌI bước sinh tiền**, tới **đồng nguyên, nửa làm tròn RA XA số 0**. Không có bước làm tròn riêng. Chú thích *"(B-20 interim)"* trong BRD là nhầm — B-20 là thứ tự tiêu thụ cap |
| **X-4** | Term engine | Gộp vào `CH-05 CoverageScopeExclusions` | **Hai bước riêng**: bước 4 (`PRODUCT` scope) và bước 7 (`BENEFIT`/`BENEFIT_SUBTREE`). Bước 7 chạy tập rỗng và **vẫn phải render** |

## Bảng mã bước — dùng khi đặt tên Server Action

| Bước | Tên | Server Action | `CH-nn` BRD |
|---|---|---|---|
| 1 | Member eligibility | `CH-01 MemberEligibility` | `CH-01 PolicyEligibility` |
| 2 | Waiting period | `CH-02 WaitingPeriod` | `CH-02 WaitingPeriod` |
| 3 | Pre-existing condition rule | `CH-03 PreExistingRule` | `CH-03 EventClassification` *(khác nghĩa)* |
| 4 | General exclusions | `CH-04 GeneralExclusions` | gộp trong `CH-05` |
| 5 | Benefit mapping | `CH-05 BenefitMapping` | `CH-04 LineBenefitMapping` |
| 6 | Line-level cover test | `CH-06 LineCoverTest` | gộp trong `CH-05` |
| 7 | Benefit-level provisos | `CH-07 BenefitProvisos` | — |
| 8 | Per-visit / per-day / per-event sub-limit | `CH-08 SubLimit` | `CH-07 PerVisitCap` |
| 9 | Count limit | `CH-09 CountLimit` | — |
| 10 | Co-payment | `CH-10 CoPayment` | `CH-09 CoPayment` |
| 11 | Deduction ratio | `CH-11 DeductionRatio` | — |
| 12 | Parent annual cap | `CH-12 ParentAnnualCap` | `CH-08` + `CH-11` |
| 13 | Plan-level check | `CH-13 PlanLevelCheck` | — |
| 14 | Post to ledger | `CH-14 PostLedger` | `CH-14 LedgerPreview` |
| *(không phải bước)* | Customer-borne breakdown | `BuildCustomerBorneBreakdown` | `CH-13` |

`CH-13` của BRD **không phải bước tính tiền** — nó phân loại số tiền đã tính theo nguyên nhân. Server Action **sau** bước 14, không đánh số.

## §1.3 Làm tròn — nguyên văn A5 §6.2

> Every step that produces a money amount rounds to the **nearest whole dong, half away from zero**, and rounding happens **at each step, not once at the end**. This matters for BR-TRACK-4: the rule chain re-runs after a clinical recommendation is confirmed, and a chain that rounded only at the end could return a different total from the first pass on the same inputs. Percentages are held as integer basis points; the rounding applies to the money result, never to the rate.

ODC: `Round(x, 0, MidpointRounding.AwayFromZero)` sau **mỗi** bước sinh tiền. Tỷ lệ giữ **basis point nguyên** (`2000` = 20%); quy đổi `bp = pct × 100` **chỉ ở script seed**, không bao giờ trong engine.

## §1.4 BR-CHAIN-1 — bước không đánh giá được KHÔNG BAO GIỜ là pass

> **A step of the chain that cannot be evaluated because a fact it requires is unavailable is never treated as a pass.** The line is **held**, a **checkpoint is raised naming the missing fact**, and the claim is **outside the auto-approval gate**. It is not a decline either: an absent fact is a data problem.

Hai fact vắng mặt kích hoạt ngay: **số ngày điều trị theo dòng** (`Q-IDP-06`, chặn bước 9 kiểu `DAY`) và **ICD theo dòng** (`Q-IDP-02`, chặn term #5 và #9). Bước phải ghi **tên fact thiếu** vào ô *Diễn giải* — **không để ô Tác động bằng 0**, vì bước lặng lẽ pass và bước không chạy được nhìn giống hệt nhau trong một tổng số.

## Base amount — câu bất biến nếu số bước đổi

> **co-payment follows the count limit, the deduction ratio follows co-payment**

- Bước 10 co-payment: base = số còn lại **sau bước 9** (sau sub-limit và count limit, sau mọi decline ở bước 4/6/7). **Không phải số billed.**
- Bước 11 deduction: base = số còn lại **sau bước 10** — phần của công ty bảo hiểm.
- Bước 12 parent annual cap là **bốn bước SAU** co-payment. Hạn mức năm **không** nằm trong base của co-payment.

## §1.5 Bộ đếm DAY — fact theo dòng A4 L18 (thêm 21/09 tối, cho T-CHAIN-07)

A4 v1.3 **L18 "Số ngày điều trị (theo dòng)"** — int, `null` khi dòng không tính theo ngày hoặc trang không mang; **không bao giờ suy từ L5 Quantity**. Đây chính là `Q-IDP-06`: IDP chưa trả L18 cho dữ liệu sản xuất, nhưng **fixture có thể mang L18 tường minh** — Q-IDP-06 chặn dữ liệu thật, không chặn phép thử cây.

- Entity: `ClaimLine.TreatmentDays` Integer (L18). ODC Integer không null ⇒ **0 = chưa có fact** (giữ nguyên nghĩa `null` của A4). Không có nơi nào gán từ `Quantity`.
- **Bước 9, `CountUnit = DAY`:** `TreatmentDays = 0` ⇒ giữ nguyên hành vi hiện tại — Held, checkpoint "[B9] {benefit} DAY: Q-IDP-06" (T-CH09-c, BR-CHAIN-1). `TreatmentDays > 0` ⇒ với benefit của dòng **và mọi tổ tiên có `CountUnit = DAY`** (100.1.1 → 100.1; 100 không có DAY): `Remaining = CountLimit − LimitLedger.UsedCount` (0 nếu không có dòng ledger); `TreatmentDays ≤ Remaining` ⇒ log "{benefit}: {days}/{CountLimit} ngày, đã dùng {UsedCount}"; **vượt ⇒ Held + checkpoint "[B9] {benefit} DAY vượt hạn: {days} > còn {Remaining}"** — không tự cắt pro-rata (A5 không quy định cách chia một dòng nhiều ngày; BR-CHAIN-1 → người quyết).
- **Bước 8 `PER_DAY`:** không đổi trong lượt này — trần/ngày áp trên nhóm `Benefit|ServiceDate`; dòng một ngày (T-CHAIN-07) đúng; dòng nhiều ngày (cap × days) ghi nợ **B-21** cho batch F.
- **Bước 14:** posting `DAY` Count = `TreatmentDays` PROVISIONAL trên benefit của dòng và mọi tổ tiên `CountUnit = DAY` (100.1.1 +1 **và** 100.1 +1 — nguyên văn T-CHAIN-07); `AMOUNT` vẫn roll-up tới root 100 (BR-TREE-2). Dòng DAY với `TreatmentDays = 0` không tới được bước 14 (đã Held ở bước 9). Log: "Post tạm: {n} posting AMOUNT ({Σ} ₫), {m} OCCURRENCE, {k} DAY ({benefit} +{days}, …); trạng thái PROVISIONAL".
- **`A5Ledger_ApplyPosting`:** `DAY` ⇒ `LimitLedger.UsedCount += Sign × Count`, `CountUnit = "DAY"` (đối xứng Finalize/Reverse như OCCURRENCE).

## §1.6 V-546 / BR-LEDGER-8 — hai hồ sơ cùng bộ đếm (T-CHAIN-06)

Nguyên văn expected-71: *"Claim 2 chạy lại trên bộ đếm hiện tại và giám định viên được báo số đã dịch chuyển. Không post lại con số lần chạy đầu."* Trong chuỗi đã dựng, điều này là hệ quả của ba việc có sẵn: bước 12 đọc `LimitLedger.UsedAmount` (chỉ FINAL), `A5Step14_PostLedger` **đảo mọi posting PROVISIONAL cũ của hồ sơ trước khi post** (14.a ReverseOld), `A5Ledger_Finalize` chuyển PROVISIONAL → FINAL và cộng vào ledger. Phép thử: hai hồ sơ cùng thành viên/quyền lợi, chạy cả hai, Finalize hồ sơ 1, chạy lại hồ sơ 2 ⇒ tổng mới thấp hơn, posting run 1 của hồ sơ 2 = REVERSED (không post lại), RunNo +1. "Giám định viên được báo số đã dịch chuyển" = F48 hiện run mới nhất + lịch sử ChainRun; **banner "số đã dịch chuyển" riêng chưa có** — ghi nợ batch F.

**Khoảng trống được ghi nhận:** lượt này gọi `A5Ledger_Finalize` trực tiếp từ runner. Khi PO chốt 3/4 điều kiện và Finalize được nối vào đường Approve thật (`approval-gate-4-conditions`), **T-CHAIN-06 phải chạy lại qua Approve** — hiện mới thử hàm, chưa thử luồng.
