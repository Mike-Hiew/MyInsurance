# AG-4 bỏ sót hồ sơ trùng CLM-2026-0461 — điều tra và sửa (09/10/2026)

## Hiện tượng
- Nộp bộ mẫu 02 (hồ sơ trùng) qua MyInsurance → CLM-2026-0461 trùng người, cơ sở (Vinmec), ngày 08/10/2026 với CLM-2026-0460.
- AG-4 trả "No signal" → cổng 4/4 → tự duyệt, Paid 320.000 ₫ (Development, lệnh chi giả lập).

## Nguyên nhân (đã kiểm bằng AgentTrace + app_logs)
- Tool `Agent_FindDuplicateClaims` trả đúng `found=True; CLM-2026-0460`.
- AG-4 (CLM_FraudSignalAgent rev 6) gọi đủ 5 tool đọc rồi gọi `Agent_SetFraudCheckDone`, **không gọi `RaiseFraudSignal`**. Log ODC xác nhận không có lần gọi nào.
- Agent không đổi từ 04/10; cùng tình huống ở CLM-0451 (05/10) AG-4 có ghi DUPLICATE. → Lỗi do mô hình không ổn định, không phải lỗi tool.

## Sửa (anh chọn "cả hai")
1. **Chốt chặn cứng — ClaimMVP rev 484:** service action `Agent_FindDuplicateClaims`, sau `FindDuplicateClaims`: nếu `DuplicateFound` → gọi `RaiseFraudSignal(DUPLICATE, HIGH, EvidenceCsv = số hồ sơ trùng, ExplanationVi …)`, rồi mới `WriteAgentRun`. Không phụ thuộc mô hình.
2. **Siết prompt — CLM_FraudSignalAgent rev 7:** thêm khối MANDATORY RULES cuối system prompt (BuildMessages): found=True thì bắt buộc RaiseFraudSignal DUPLICATE/HIGH, kể cả khi hồ sơ trước đã chi; vi phạm ngày thì DATE_IMPOSSIBLE; không SetFraudCheckDone trước khi ghi đủ tín hiệu; câu trả lời cuối phải liệt kê tín hiệu.

## Kiểm chứng
- CLM-2026-0462 (bộ 02 lần nữa): tool thấy trùng 0460 + 0461; AG-4 tự ghi DUPLICATE/HIGH (SignalId 22) → "Auto-approval blocked — AG-4 signal", cổng 3/4, không lệnh chi, chuyển người duyệt.
- Test chuỗi 46/46, cổng 5/5 trên rev 484.
- Chưa quan sát riêng được chốt chặn: lần chạy này AG-4 cũng tự ghi nên tín hiệu cùng loại bị cập nhật đè (RaiseFraudSignal upsert theo loại).

## Còn lại
- CLM-0461 đang ở trạng thái Paid dù trùng (dữ liệu Development). Cần PO quyết: để nguyên làm bằng chứng lỗi cũ, hay mở lại tín hiệu bằng công cụ sửa dữ liệu.
- Rollback: ClaimMVP rev 483, CLM_FraudSignalAgent rev 6.
