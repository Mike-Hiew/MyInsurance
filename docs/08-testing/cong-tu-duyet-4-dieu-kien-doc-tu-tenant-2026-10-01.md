# Cổng tự duyệt — "4 điều kiện NÀO?" · đọc từ tenant 01/10/2026

ClaimMVP **rev 199**. Đóng việc mở (d) của `trang-thai-build-2026-09-20` và `approval-gate-4-conditions` 1b.

> ## ✅ PO CHỐT 01/10 — Phương án A: lấy code (màn hiện tại) làm chuẩn — về **danh sách**
>
> **C1** Payable ≤ ngưỡng (2.000.000 ₫, admin sửa được) **và không gì bị cắt** — A5 §6.4 cond.1
> **C2** Đánh giá lâm sàng hoàn tất, **không** khuyến nghị nào đang chờ — A5 §6.4 cond.2
> **C3** Không term checkpoint nào đang mở — A5 §6.4 cond.3
> **C4** Không tín hiệu gian lận / trùng lặp — BRD hackathon G4 (agent **AG-4**, không phải "AG-6")
>
> **Confidence / chất lượng trích xuất (G2 cũ) KHÔNG nằm trong cổng** — đúng A4 §7 / D1 §5.8; thuộc **pre-submission checks + định tuyến thẩm định viên**.
>
> ⚠️ "Lấy code làm chuẩn" chỉ đúng cho **danh sách 4 điều kiện**. **Công thức** trên màn (§6) sai ở cả 4 — 2 trong 4 là trang trí. Không được dùng biểu thức hiện có làm đặc tả cho server action.

## 1. Không có `EvaluateAutoApprovalGate`

- `context_actions` trên ClaimMVP (chỉ mục 30/09 11:22 UTC): **146 action, không action nào** liên quan cổng. Control dương: `RunAdjudicationChain` tìm thấy qua cùng truy vấn.
- Cổng **chỉ được tính trên màn `ClaimDetail`** (block `gateCard`, tab *Approval & payment*). 7 test `T-GATE` không có gì để gọi; đường tự duyệt thật không tồn tại. Cổng hiện là **hiển thị**, chưa phải **logic**.

## 2. Bốn điều kiện như màn hiện — hồ sơ `CLM-2026-0001` (Id 17)

*"Auto-approval conditions · not met — needs a person"*

| # | Nhãn | Hồ sơ 17 | Lý do màn hiện |
|---|---|---|---|
| 1 | Payable ≤ 2,000,000 ₫ and nothing cut | ✕ | Per-visit cap bound at step 8 — 200.1 cắt 610.000; 200.2 cắt 18.000 |
| 2 | Clinical assessment complete, no recommendation | ✕ | 1 proposal awaiting a human |
| 3 | No open term checkpoint | ✓ | 6 terms evaluated at step 4, no checkpoint open |
| 4 | No duplicate / fraud signal (AG-6) | ✓ | No claim with same insured + provider + date in 90 days · hospital_code matched via OpenSearch table 322 |

## 3. Đối chiếu với `build-04` §3.1 (trước khi chốt)

Màn = **A5 §6.4 ba điều kiện + BRD G4**; `build-04` = **BRD G1–G4** (gộp A5 cond.2+3 thành G3, thêm G2 confidence). G2 trái `build-01` §4.14 (A4 §7, D1 §5.8). Luật dự án: *BRD hackathon lệch A5/D2 → A5/D2 thắng* ⇒ phương án A.

## 6. Công thức thật trên màn — Mentor đọc model rev 199 (01/10, chỉ đọc, không sửa)

Nguồn: phiên Mentor "Claim auto-approval conditions audit". *Đây là Mentor đọc model, chưa phải hành vi — nhưng khớp với mọi thứ màn hiện.*

### C1 — tính từ dữ liệu, có 3 lỗi

- ✓/✕: `GetCalcRun…ChainRun.TotalPayable <= 2000000 and GetWaitStep.List.Length = 0 and GetCutSteps.List.Length = 0`
- `GetCalcRun`: ChainRun, `ClaimId = ClaimId`, sort `-RunNo`, Max 1 (run mới nhất).
- `GetWaitStep`: CalcSnapshot, `ClaimId = ClaimId and ReasonCode = "WAIT"`, Max 1.
- `GetCutSteps`: CalcSnapshot, `ClaimId = ClaimId and Outcome = "CUT"`.
- Lý do: computed (`CalcSnapshot.Detail`, `FormatVND(TotalPayable)`); nhánh WAIT là literal *"Both lines declined (WAIT) — lines were cut"*.

**Lỗi:** (a) **ngưỡng 2.000.000 là literal trong biểu thức** — chân bảng và màn Administration nói "admin-editable" nhưng cổng không đọc chỗ nào; (b) WAIT/CUT lọc theo **ClaimId**, không theo **run mới nhất** → snapshot của run cũ vẫn làm C1 fail (CalcSnapshot đang có 144 dòng, nhiều run/hồ sơ); (c) "nothing cut" chỉ xét WAIT và CUT — dòng bị loại vì **NOTREL / NOTCOV / EXCL** không làm C1 fail, dù lý do ✓ nói *"no line declined"*.

### C2 — tính từ dữ liệu, có 2 điểm đáng ngờ

- ✓/✕: `Assessment.State = "COMPLETE" and RecommendationCount = 0 and LinesAssessed = LinesTotal and not(GetRecommendation.List.Length > 0 and IsRecPending(…Decision))`
- `GetAssessment`: Assessment, `ClaimId = ClaimId`, sort `AssessedOn`, Max 1 — **chiều sort chưa rõ**: nếu tăng dần thì đang đọc assessment **cũ nhất**.
- `GetRecommendation`: Recommendation ⋈ Assessment ⋈ ClaimLine, `Assessment.ClaimId = ClaimId`, **Max 1** — chỉ xét **một** khuyến nghị.
- Lý do: computed (`LinesAssessed`, `LinesTotal`, `RecommendationCount`).
- Ghi chú: `RecommendationCount = 0` nghĩa là khuyến nghị đã Confirm/Reject vẫn làm C2 fail mãi — cần chốt: "không có khuyến nghị nào" hay "không khuyến nghị nào đang **chờ**" (A5 §6.4 / build-04 G3 nói *Status = Proposed*).

### C3 — 🔴 TRANG TRÍ

- Ký tự **✓ là literal tĩnh** (`G3CkText.Text = "✓"`). Chỉ **class CSS** đổi theo `GetWaitStep.List.Length = 0` — tức là theo **kỳ chờ (bước 2)**, không phải checkpoint (bước 4).
- **Không đọc `TermCheckpoint`** — entity mà `A5Step04_GeneralExclusions` ghi `Status = "OPEN"`.
- Lý do: **literal** cả hai nhánh — *"Clause 5.2 waiting period — checkpoint open, approver needed"* / *"6 terms evaluated at step 4, no checkpoint open"*.
- ⇒ Giải thích mâu thuẫn hồ sơ 17: B04 ghi checkpoint TERM-HC-003 mở, C3 vẫn ✓ vì nó không nhìn vào đó.

### C4 — 🔴 TRANG TRÍ

- Style **cứng** `"cc-ck cc-ok"` — **luôn ✓**, không `If`, không binding.
- **Không aggregate, không biến, không data action.**
- Lý do: **literal** — *"No claim with same insured + provider + date in 90 days · hospital_code matched via OpenSearch table 322"*. Không phép kiểm trùng nào chạy; bảng 322 chưa nối.

### Hệ quả cho demo

Màn cổng là màn giám khảo sẽ mở. Hiện **C3 và C4 luôn ✓ bất kể dữ liệu**, và nói đã kiểm những thứ không kiểm. Nếu giám khảo hỏi "đổi dữ liệu thì C3/C4 có đổi không" — không. Đây là rủi ro uy tín lớn hơn mọi lỗi số liệu khác.

## 4. Điều lạ khác trên cùng màn — còn mở

- Nhãn **"AG-6"** → **AG-4** (`build-01` §4.27).
- Card **"RUN #2 (AFTER CONFIRMATION) 0 ₫"** vẫn hiện — `F-CH-1` (rủi ro #11 từ 21/09), **chưa sửa**.

## 5. Việc tiếp theo — đặc tả cho `EvaluateAutoApprovalGate(ClaimId)`

Output: `C1..C4 {Pass: Boolean, Reason: Text}` + `AllPass`. **Mọi Reason sinh từ dữ liệu.** Một phiên Mentor cho action, một phiên cho màn.

| | Đúng phải là |
|---|---|
| C1 | `TotalPayable` của **ChainRun mới nhất** ≤ **ngưỡng đọc từ nơi màn Administration lưu** (tìm trước — không literal) **và** không CalcSnapshot nào **của run đó** có Outcome cắt/loại (CUT, WAIT, NOTREL, NOTCOV, EXCL — liệt kê theo enum thật) |
| C2 | Assessment **mới nhất** `State = COMPLETE` và `LinesAssessed = LinesTotal` và **không** Recommendation nào `Decision = Proposed` (đếm tất cả, không Max 1) |
| C3 | Không `TermCheckpoint` nào `ClaimId = ClaimId and Status = "OPEN"` |
| C4 | Không Claim khác cùng người được BH + cùng cơ sở + ngày điều trị trong ±90 ngày (aggregate thật). Lý do nêu đúng phép kiểm đã chạy — **không nhắc bảng 322** cho tới khi nối |

Sau đó: màn `ClaimDetail` chỉ hiển thị output action · đổi nhãn AG-4 · 7 `T-GATE` theo C1–C4 · sửa `build-04` §3.1 tại nguồn.
