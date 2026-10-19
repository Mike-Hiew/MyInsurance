# Khung chat trợ lý AI theo phong cách BnK (15/10/2026 tối)

Tiếp nối: claude/ai-orb-bnk-nut-tro-ly-15-10.md (nút orb BnK).
Yêu cầu: chatbot đồng bộ icon, hiệu ứng, màu với logo/orb BnK; "wow", tương lai, dễ chịu.

## Thiết kế (chỉ CSS, không đổi widget)
- **Khung:** kính navy (rgba(8,24,60,.93) → rgba(4,14,40,.95)), backdrop blur 22px, bo 26 px, quầng xanh ngọc góc trên phải trôi chậm (14 s), **viền sáng xoay** xanh ngọc → xanh dương (conic-gradient + `@property --chA`, 9 s/vòng), mở ra có hiệu ứng nảy từ góc phải dưới.
- **Header:** orb mini 40 px có logo BnK thay icon robot (`.pt-chatbox-hdr::before`), phát sáng nhịp 3,2 s; chấm "online" nhấp nháy sau tiêu đề; vạch sáng chạy dưới header; nút đóng tròn kính, xoay 90° khi rê.
- **Bong bóng:** AI = kính xanh ngọc–xanh dương mờ, viền sáng nhẹ, bo 18/18/18/6; người dùng = gradient #00AFA8 → #0A84B2 → #3E8BFF, bo 18/18/6/18. Xuất hiện bằng hiệu ứng nảy nhẹ.
- **Chip gợi ý:** viên kính viền xanh ngọc, icon sao 4 cánh gradient (mask SVG), hiện lần lượt, rê chuột sáng lên + nhấc 1 px.
- **Ô nhập:** viên kính tối (bỏ nền lavender–mint cũ), focus = vòng sáng xanh ngọc; nút gửi = orb gradient xanh ngọc, rê chuột phóng to + nghiêng.
- **MyInsurance thêm:** nút "Hỏi trợ lý về hồ sơ này" (`.mk-btn.mk-btn-ai`, và `.btn-ai-receipt`) đổi từ tím sang gradient xanh ngọc.
- Tắt mọi chuyển động khi prefers-reduced-motion. Khung chat giữ nền tối ở cả chế độ sáng (đồng bộ với orb).
- Bộ chọn dạng `html body .pt-chatbox[class] …` / `html body .cc-console-chatbox[class] …` để thắng các rule cũ có !important.

## Đã áp
| App | Rev | Ghi chú |
|---|---|---|
| MyInsurance | 218 (khung chat), **219** (nút mk-btn-ai) | Validate 0 lỗi. Kiểm trên trang thật (PortalClaimDetail 440, nền tối). |
| ClaimMVP | 761 (khung chat BnK) | 2 lượt Mentor (part A/B ~8 KB). Validate 3 lỗi = nền. |
| ClaimMVP | 762 (chỉ còn Chat, giống MyInsurance) | Xem mục dưới. |
| ClaimMVP | **763** (chip gợi ý dưới lời chào) | Xem mục dưới. |

## ClaimMVP rev 762 — bỏ tab Activity, khung chat giống MyInsurance (22:55, theo phản hồi "Activity thừa")
- Biến `ActiveTab` vốn mặc định "chat"; 2 chỗ chuyển sang Activity khi mở panel (`ToggleConsoleChat`, `TriggerOpenAssistant`) nay đặt "chat" → panel luôn mở vào Chat.
- Tiêu đề `CCHdrTitleExpr` = T("Claim assistant","Trợ lý hồ sơ"); phụ đề `CCHdrSubExpr` = T("Ask about claims, documents & limits","Hỏi về hồ sơ, chứng từ & hạn mức") (trước: "Work inbox / Proposals & alerts").
- CSS: ẩn `.cc-ab-tabs` và `.cc-ab-activity` (không xoá widget/logic Activity); khung nổi 360 × min(560, 100vh−120) px, neo phải 26 / dưới 96 px ngay trên orb (bộ chọn 3 lần class để thắng `.layout.layout-side .cc-console-chatbox{top:…!important}`); ẩn badge số trên orb (`.cc-fab-n`) vì danh sách Activity không còn hiện.
- Còn nhỏ: phụ đề tiếng Anh xuống 2 dòng trong header (chưa sửa).
- Muốn mở lại Activity: bỏ 2 dòng CSS ẩn tab/activity + đổi 2 action về như cũ.

## ClaimMVP rev 763 — chip gợi ý ngay dưới lời chào như MyInsurance (23:10)
- Mentor không chuyển được widget giữa container → thêm Container mới `CCChipsTop` (class "pt-chatbox-chips cc-chips-top") trong `CCBody`, giữa `CCWelcomeBubble` và `CCChatList`, với 3 Button `CCTChip1–3` là bản sao chức năng của chip footer (OnClick `ChipWaiting` / `ChipProposals` / `ChipAlerts`, Enabled = not IsAsking, cùng điều kiện hiện như `CCGenChips`: chỉ hiện khi không mở hồ sơ). Nằm trong block `LayoutSideMenu`.
- Chip footer cũ (`CCGenChips`) giữ nguyên, ẩn bằng CSS `[id$=CCFooter] .pt-chatbox-chips{display:none}`.
- Kiểm trên Work list: lời chào → 3 chip xếp dọc → ô nhập; chip footer ẩn.

File CSS: `E:\Hackaton\assets\ai-chat-bnk-myinsurance.css`, `ai-chat-bnk-claimmvp.css` (chưa gồm khối rev 762–763).

## Cách làm / bài học
- Xem trước bằng cách tiêm CSS nguồn (có placeholder $S/$LOGO/$STAR) vào trang thật, lấy logo từ `getComputedStyle(.ai-orb__logo)` → khỏi dán lại data URL.
- Mỗi app tách 2 lượt Mentor ≤ 8 KB; Mentor chép đúng (kiểm: viền xoay `ch-spin` chạy, orb header có path logo).
- Chưa kiểm bong bóng người dùng trên trang thật (không gửi tin nhắn thử để tránh gọi agent/ghi AgentRun).
- Mockup v1.0 chưa cập nhật phần khung chat (class mockup khác ODC).
