# Nút trợ lý AI = orb BnK (15/10/2026 tối)

Tiếp nối: claude/tien-do-v10-giong-mockup-15-10.md (ClaimMVP 759 / MyInsurance 216).

## Thiết kế
- Quả cầu thủy tinh, chất lỏng xanh ngọc → xanh dương → navy (#16DCA8 / #00AFA8 / #0A84B2 / #0B1650) dập dềnh, đường sóng sáng, vệt phản quang, quầng sáng "thở" phía sau.
- Logo BnK (khiên lục giác + B + ô pixel, lấy từ vector "Crafted by BnK Solution" trong mockup v1.0) nằm giữa, lật như đồng xu mỗi 6 s. `is-thinking` → chảy nhanh, logo xoay liên tục, vòng sóng lan ra. Tắt chuyển động khi prefers-reduced-motion.
- Chỉ CSS + 6 Container. Trang demo/mã: artifact "Nút AI Orb".
- File: `E:\Hackaton\assets\ai-orb-bnk.css` và `.min.css` (9,3 KB). Mockup v1.0 đã gắn (backup "truoc ai orb 15-10").

## Cấu trúc widget (giống nhau ở 2 app)
Button (thêm class `ai-orb`, giữ class cũ + logic is-open) → sau Icon cũ:
- `OrbCore` .ai-orb__core → `OrbSwirl` .ai-orb__swirl · `OrbWaveBack` .ai-orb__wave.ai-orb__wave--back · `OrbWave` .ai-orb__wave · `OrbLogo` .ai-orb__logo
- `OrbGloss` .ai-orb__gloss
Icon robot cũ ẩn bằng CSS (`.ai-orb>i{display:none}`), không xoá.

## Đã áp
| App | Rev | Nút | Kiểm |
|---|---|---|---|
| ClaimMVP | 760 (22:14) | `b1-ConsoleFabBtn` `.cc-console-fab` | Orb 58 px góc phải dưới, badge số giữ (z-index 5), nhãn "Ask the assistant" giữ |
| MyInsurance | **217** (22:35) | `b1-FabBtn` `.pt-fab.pt-chat-fab` | Validate 0 lỗi. Trên trang thật: orb 58 px, logo đúng (SVG có path B), mở/đóng chatbox bình thường, nhãn "Hỏi trợ lý" giữ |

## Lưu ý
- Mentor lại tự gắn `OSInline ThemeGrid_MarginGutter` cho Container mới (MyInsurance) → đã có rule `.ai-orb>.ai-orb__core,.ai-orb>.ai-orb__gloss{margin:0!important}` nên không lệch.
- Chưa nối `is-thinking` với lúc trợ lý đang trả lời (cần biến IsThinking trong block chat: True trước khi gọi agent, False khi có kết quả; Style Classes thêm " is-thinking").
- Chưa chạy ChainTests sau rev 760/217 (chỉ đổi giao diện).
