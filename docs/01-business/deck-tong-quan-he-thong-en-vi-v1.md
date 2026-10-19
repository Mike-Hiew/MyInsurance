# Bộ tổng quan hệ thống "Agentic Health Claim Adjudication" — EN deck + VI HTML + sơ đồ kiến trúc v1.0

**Ngày:** 07/09/2026. **Đối tượng:** hackathon judges + SA OutSystems. **Phạm vi:** lát cắt agentic (không phải toàn MVP1).

## File
| File | Nội dung |
|---|---|
| `BnK_Agentic_Health_Claim_Overview_EN_v1.0.pptx` | 16 slide, 16:9 (13,3×7,5"), tiếng Anh, có speaker notes ở 5 slide. Slide 6 là **sơ đồ kiến trúc vẽ bằng shape** (thay layout thẻ cũ) |
| `BnK_Tong_Quan_He_Thong_VI_v1.0.html` | Một file, tiếng Việt, cùng nội dung, responsive + print-friendly, không phụ thuộc mạng. Mục 05 có **sơ đồ kiến trúc dạng inline SVG** |
| `BnK_Architecture_Diagram.html` | Trang sơ đồ **tương tác, song ngữ VI/EN**: hover/bấm một khối → panel giải thích khối đó, các khối khác mờ đi. Có legend, theme sáng/tối, phím Esc để bỏ chọn |
| `BnK_Agentic_Claim_Architecture.drawio` | File draw.io **2 trang** (`Architecture (EN)` / `Kiến trúc (VI)`), mở ở app.diagrams.net hoặc VS Code extension, sửa và export PNG/SVG/PDF được |

**Style:** kế thừa deck Business-to-Tech v1.0 — palette teal `0B4F53 / 0E7C7B / 9AD9C8 / EAF4F1`, tiêu đề Cambria, thân Calibri, card bo góc, footer "BnK Solution | … | …" + số trang. Sinh bằng pptxgenjs (`/home/claude/deck/build.js`) và `mk_drawio.py`; đã validate + render QA 16 slide; HTML QA bằng Playwright ở 1440/820/390 px, 0 lỗi JS, 0 tràn ngang.

## Sơ đồ kiến trúc — cấu trúc dùng chung cho cả 4 file
Container **OutSystems ODC** chia bốn lớp từ trên xuống, nhãn lớp ở lề trái:
1. **Trải nghiệm** — CLM_Portal (My Insurance) · CLM_Console (bàn làm việc cán bộ)
2. **Agentic** — khối tối `CLM_Agents · Agent Workbench`: Orchestrator ở trên, 5 agent ở dưới (Intake, Adjudication, Clinical, Fraud-signal, Customer comms)
3. **Tất định** — RunAdjudicationChain (14 bước) · cổng tự duyệt 4 điều kiện · Claim + LimitLedger · AgentRun
4. **Tích hợp** — CLM_IDP_Int (6 REST Consume + adapter) · Core connectors (mock)

Bên phải là **hệ thống ngoài**: BnK IDP (chạy thật) và core bảo hiểm (mock, không khoá vào core nào).
Mũi tên giữa lớp Agentic và lớp Tất định có nhãn **"calls as a tool · never writes"** — đây là điểm cần dừng lại khi pitch, vì nó trả lời câu hỏi an toàn trước khi giám khảo hỏi.

## Cấu trúc 16 slide
1. Bìa · 2. Contents (6 mục)
- **01 Bài toán** — hiện trạng thủ công + baseline (100% gõ tay, SLA 3–5 ngày / 10–14 ngày, 990 hồ sơ 2026, 4,3 tỷ); vì sao bây giờ (ONE 2026 + Insurance Agents Kit Q3, IDP đã đọc được, 13/64 yêu cầu → khoảng trống agent).
- **02 Giải pháp** — hành trình 5 bước (66 giây); **đường ranh "tiền tất định, phán đoán agentic"** với ví dụ 2.000.000 → 1.880.000 chạy hai lần.
- **03 Kiến trúc** — slide 6 là sơ đồ vẽ; slide 7 là IDP đo được (~16 s/trang, ~30 s flow, 20 mục, 13.328 bệnh viện) và cái IDP không làm được.
- **04 Các agent** — bảng 6 agent (vai trò + tool); slide agent-to-agent 6 chặng đọc từ AgentRun/handoffTo; bảng giá trị từng agent + số đo tương ứng.
- **05 Quản trị** — 6 guardrail (tiền là code, cổng 4 điều kiện, bắt buộc evidence, xác nhận kèm lý do, AgentRun, fail-safe/PII).
- **06 Bằng chứng & kế hoạch** — 3 kịch bản A/B/C + replay offline; bảng ROI 5 dòng với cột "đo bằng cách nào"; lộ trình 7/9 → 22/10 → pilot Q4, cam kết đội 3 người, 3 điều đề nghị OutSystems.
16. Thank you.

## Điểm cần lưu ý khi dùng
- **Độ chính xác trích xuất vẫn ghi "chưa đo"** — deck nói thẳng điều này và hứa con số thật từ `POST /request/compare` trước ngày pitch. Chạy compare xong phải cập nhật slide 14 + mục 12 của HTML.
- **Tên khách hàng để trống**: "top-five bancassurance life insurer already trialling BnK IDP" — chưa nêu MBAL/BIC, chờ khách đồng ý.
- Deck dùng tên 6 agent (Orchestrator, Intake, Adjudication, Clinical, Fraud-signal, Customer-communication), khớp `chien-luoc-hackathon-agentic-claim.md`, không dùng cách đánh số AG-0…AG-7 của bản Business-to-Tech.
- Ngày trong deck: enablement 7/9, build 8/9, sprint 9–29/9, pitch 7–8/10, kết quả 22/10, pilot Q4 — khớp KBYG.
- **Nếu Agent Workbench không cho agent gọi agent**: phải sửa slide agent-to-agent và khối Orchestrator trong cả 4 file thành Server Action/BPT, diễn giải qua audit trail. Nội dung mô tả Orchestrator trong `BnK_Architecture_Diagram.html` đã nêu sẵn phương án dự phòng này.
