# MyInsurance — audit runtime rev 54 (01/10/2026)

**App:** MyInsurance `<redacted-app-key>` · rev 54 (publish 17:07 VN 01/10; rev 52–54 do Mike sửa).
**Cách kiểm:** Chrome đã đăng nhập vai khách (Nguyễn Văn An, HC-STD), đọc `innerText` 7 màn + 3 chi tiết hồ sơ, grep mã nội bộ bằng regex (có control dương), thử chatbox 2 câu gợi ý. Không bấm gì ghi dữ liệu.
**Lưu ý:** Context Service của MyInsurance còn index ngày 19/09 → đọc model qua MCP không phản ánh rev 54; chỉ tin runtime.

## Đạt

- Grep sạch trên Home, Benefits, Claims, NewClaim, Supplement, Notifications, ClaimDetail: **0** mã UPPER_SNAKE, 0 mã trạng thái/chứng từ/lossType nội bộ, 0 `TST-/FX-`, 0 `agent/IDP/AI/template id`, 0 `null/undefined/NaN`.
- 0 lỗi console.
- Tổng dòng CLM-0001 = 3.128.000 ₫ khớp "Amount you claimed".

## Lỗi — theo mức ưu tiên trước pitch

| # | Màn | Lỗi | Mức |
|---|---|---|---|
| 1 | Login | Portal khách hiện **"Health Insurance · Backoffice · Claim Console"** (sót từ clone ClaimMVP) | 🔴 |
| 2 | Supplement | CLM-0001 (ngoại trú) bị đòi **Giấy ra viện — Mandatory**; dropdown "Document type" **không có** loại Discharge paper → khách không nộp được đúng thứ bị đòi | 🔴 |
| 3 | Dữ liệu demo | 8 hồ sơ rác **CLM-0034…0041** (Medlatec, 20/09, 6 Maternity + 2 Dental) chiếm Home/Claims/Notifications; Maternity cho NĐBH nam "Nguyễn Văn An" | 🔴 |
| 4 | Chatbox | Trả lời đóng hộp, **sai nghiệp vụ**: "Atorvastatin" giải thích là vượt trần/lượt (thực tế kịch bản A là NOTREL – không liên quan chẩn đoán), nhắc "Explanation of Benefits" không tồn tại; câu hạn mức không đọc số, chỉ bảo mở Benefits | 🔴 |
| 5 | ClaimDetail CLM-0004 | "Amount you claimed" **2.400.000 ₫** nhưng 3 dòng chỉ cộng **800.000 ₫**; trạng thái "Processing your documents" mà đã hiện dòng chi phí | 🟡 |
| 6 | Benefits | Nha khoa "Applies from **01/01/2026**" trong khi chú thích cuối trang nói **nha khoa chờ 90 ngày** | 🟡 |
| 7 | NewClaim | Báo lỗi đỏ ngay khi mở màn ("Loss type is required", "Treated-from date is required") trước khi khách nhập gì; Provider điền sẵn Medlatec; Loss type có Maternity cho NĐBH nam | 🟡 |
| 8 | Claims | Ghi chú thiết kế lộ lên UI: "Only four states for you… Internal technical incidents never appear as a separate label." | 🟡 |
| 9 | Toàn app | Bước 2 có 3 tên: "Processing your documents" (badge) / "Processing documents" (chú thích) / "Documents read" (stepper). Badge "Processing your documents" màu đỏ như lỗi | 🟢 |
| 10 | ClaimDetail | Ngày cạnh số hồ sơ (10/06/2026, 15/01/2026) không có nhãn — dễ đọc nhầm là ngày gửi | 🟢 |
| 11 | Song ngữ | Không còn nút VI/EN trên top bar (chỉ dark mode + chuông). UI tiếng Anh, tên quyền lợi/khoản chi tiếng Việt → trộn ngôn ngữ. `build-06` §8.5 yêu cầu **portal thuần VI** | 🟡 — cần PO chốt |
| 12 | Brand | Subtitle "Digital Health Insurance Platform · MVP 1" | 🟢 |
| 13 | ClaimDetail CLM-0038 | Dòng "Items appear once your documents are read" nằm **trên** tiêu đề "Item by item" | 🟢 |

## Chưa kiểm được

- **Wizard bước 2–4** — bấm Continue sẽ tạo hồ sơ thật (thêm rác). Kiểm sau khi dọn dữ liệu.
- **Khối "Vì sao Quý khách tự chịu"** — member này không có hồ sơ nào ở trạng thái Result. Cần chạy hồ sơ A qua console tới PAID rồi xem.
- **390 px** — cửa sổ Chrome không resize được (đang maximize).
- **Dark mode.**
- Phiên đăng nhập hết hạn giữa chừng (về `/MyInsurance/Login`).
