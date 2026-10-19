# Build 04 — Bổ sung v1.1: Cổng tự duyệt C1–C4 (thay thế §3 và §9.3)

**Ngày:** 01/10/2026 · **Quyết định:** PO (Mike) chốt phương án A ngày 01/10 · **Hiệu lực:** thay thế toàn bộ `build-04` §3.1–§3.4 và bảng §9.3. Phần còn lại của `build-04` giữ nguyên.
**Lý do không sửa trực tiếp trong `build-04`:** tránh chép lại tài liệu 45 KB bằng tay. Đọc `build-04` thì đọc kèm file này; chỗ nào lệch, **file này thắng**.

## §3.1 (mới) — Bốn điều kiện

| Cond. | Tên | Quy tắc (như code `EvaluateAutoApprovalGate` đang tính, rev ≥ 213) | Tham số | Nguồn |
|---|---|---|---|---|
| **C1** | Số tiền & không gì bị cắt | Payable của **run mới nhất** (`CalcSnapshot.ResultAmount` của bước cuối) **≤** ngưỡng, **và** không `CalcSnapshot` nào của run đó có `Outcome = CUT` hoặc `ReasonCode = WAIT`, **và** không `ClaimLine` nào `LineDecision = Decline` | App setting **`AutoApprovalThreshold`** = 2.000.000 (đổi trong ODC Portal, không cần publish) | A5 §6.4 cond.1 |
| **C2** | Đánh giá lâm sàng xong, không khuyến nghị đang chờ | Assessment mới nhất `State = COMPLETE`, `LinesAssessed = LinesTotal`, **0** Recommendation có `Decision` rỗng hoặc `PENDING` (đếm tất cả) | — | A5 §6.4 cond.2 |
| **C3** | Không term checkpoint mở, không bước bị giữ | 0 `TermCheckpoint` với `ClaimId` này và `Status = OPEN` **và** run mới nhất có 0 `CalcSnapshot` với `Outcome = HOLD` (rev 213 — chuỗi không ghi bảng `TermCheckpoint`; bước 4 chỉ đánh dấu HOLD) | — | A5 §6.4 cond.3 · BR-TERM-13 |
| **C4** | Không tín hiệu trùng / gian lận (AG-4) | 0 Claim **khác** cùng `MemberId` + cùng `FacilityName` + `EventDate` trong ±90 ngày. Chưa có entity FraudSignal | — | BRD hackathon G4 |

**Confidence / chất lượng trích xuất KHÔNG nằm trong cổng** (A5 §6.4, A4 §7, D1 §5.8). G2 cũ của BRD bỏ khỏi cổng; nó thuộc pre-submission checks + định tuyến thẩm định viên.
Mọi `Reason` sinh từ dữ liệu. Cổng chỉ đọc, không ghi.

## §3.2 (mới) — Khi fail

Bất kỳ điều kiện nào fail → **cần người** (định tuyến thẩm định viên, nêu tên điều kiện fail và lý do). Trạng thái `BLOCKED` riêng cho C4 **chưa hiện thực** trong MVP — ghi là hạn chế, không phải hành vi.

## §3.3 (giữ ý cũ)

- B-22: ngưỡng xét theo từng claim — 3 × 1.900.000 qua cổng, 5.700.000 không người xem. Chưa có mitigation.

## §9.3 (mới) — Test cổng

`AssertOutcome` = mẫu P/F cho C1 C2 C3 C4 AllPass (`?` = không kiểm). Chạy bằng nút **Run gate tests** trên màn Chain tests (`RunGateTests` gọi `EvaluateAutoApprovalGate`).

| Test | Trạng thái | Hồ sơ | Mẫu | Log phải chứa | Kỳ vọng |
|---|---|---|---|---|---|
| T-GATE-01 | GATE | CLM-2026-0003 | PPPPP | `1.680.000 ₫ vs threshold` | Hồ sơ sạch qua cả 4 |
| T-GATE-02 | GATE | TST-T-CHAIN-01 | F???F | `1.920.000 ₫ vs threshold 2.000.000 ₫` · `limit cut` | Dưới ngưỡng nhưng có cắt → C1 fail |
| T-GATE-03 | GATE | CLM-2026-0001 | ??F?F | `step(s) on hold` | Run #3 có bước 4 HOLD (1 checkpoint điều khoản) → C3 fail (fixture từ rev 214) |
| T-GATE-04 | GATE | CLM-2026-0001 | ?F??F | `1 proposal(s) pending` | Khuyến nghị chưa quyết → C2 fail |
| T-GATE-05 | GATE | TST-T-CHAIN-01 | ???FF | `other claim(s) for same insured + provider within 90 days` | Trùng → C4 fail |
| T-GATE-06 | MANUAL | CLM-2026-0003 | — | — | Đặt `AutoApprovalThreshold = 1680000` → C1 Đạt (≤); `1679999` → C1 Không đạt |
| T-GATE-07 | MANUAL | — | — | — | B-22, để hành vi được biết |

Nguồn dữ liệu test: `E:\Hackaton\openspec\changes\active\chain-14-steps-a5\chain-tests-71.json` (cập nhật 01/10; bản trước khi đổi C1–C4: `.bak-before-cgate`; bản trước khi có fixture T-GATE-03: `.bak-before-gate03`). Trên tenant: action `UpdateGateTests` (cũng được gọi cuối `Seed_ChainTests`).

**Ghi chú 01/10 (rev 213):** trước rev 213, C3 chỉ đọc `TermCheckpoint` — bảng này không có chỗ nào trong chuỗi ghi vào, nên C3 luôn Đạt. Hồ sơ CLM-2026-0001 có B04 "1 checkpoint" mà C3 vẫn ✓. Màn Checkpoint queue đọc `Recommendation`, không đọc `TermCheckpoint` (hạn chế MVP 1; ghi TermCheckpoint từ chuỗi để sau hackathon).
