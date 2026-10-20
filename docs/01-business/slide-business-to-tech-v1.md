# Bộ slide "Nền tảng Bồi thường Bảo hiểm Sức khoẻ Số — Từ Business đến Kỹ thuật" v1.0

**File:** `E:\Hackaton\BnK_Agentic_Health_Claim_Business_to_Tech_v1.0.pptx` (27 slide, 16:9, tiếng Việt, thuật ngữ kỹ thuật giữ tiếng Anh). Đối tượng: Ban lãnh đạo + đội dự án BnK.


## Cấu trúc
1. Bìa · 2. Nội dung (6 mục)
- **01 Bối cảnh & Bài toán** — quy trình thủ công hôm nay + baseline BIC; bảng SC-01…SC-09 + phạm vi MVP1 + trọng tâm hackathon; vì sao bây giờ (OutSystems Industry Solutions, IDP đã đọc được chứng từ y tế, core còn khoảng trống).
- **02 Giải pháp & Trải nghiệm** — hành trình khách hàng 5 bước (66 giây, 16 s/trang, 4 trạng thái); console 8 tab (F44–F53) + nguyên tắc "việc cần làm"; chuỗi 14 bước tất định (2.000.000 → 1.880.000 ₫); bốn kịch bản demo A/B/C/D.
- **03 Kiến trúc & Tích hợp** — ba hệ thống + bảng sở hữu năng lực; bản đồ 5 module ODC (CLM_Core/Agents/IDP_Int/Console/Portal) + Entity/Server Action/Timer/Roles; tích hợp BnK IDP (6 REST Consume, số đo thật, 13/64 vs 32/64 D3, quy tắc adapter); InsureMO I-01…I-06 Provisional + RK-14 + chiến lược mock/seed.
- **04 AI hỗ trợ & Quản trị rủi ro** — bảng 7 tác vụ AG-0…AG-7 (tool vs agent, tri thức, Server Action) + "đường ranh"; 6 guardrail (cổng 4 điều kiện, evidence bắt buộc, PII, AgentRun, kiểu hỏng, khoảng trống IDP → AG-5).
- **05 Kế hoạch & Chuẩn bị** — lộ trình 3–6/9 → 7–8/9 enablement → sprint 1–2 → sprint 3–4 → 7–8/10 pitch → 22/10; sau đó pilot MVP1 Q4, Phase 2/3; checklist 4 nhóm (môi trường & giấy phép, dữ liệu & mẫu, tích hợp, nghiệp vụ & tổ chức); đội hình 7 vai trò.
- **06 Rủi ro, Quyết định & Bước tiếp** — 7 rủi ro (EAP, RK-14, RK-08, PII, flow 28/8, agent-gọi-agent, B-19/20/21); 5 quyết định cần chốt + hai tuần tới; slide cảm ơn.

## Điểm cần lưu ý khi dùng
- Khách hàng tham chiếu để ở dạng **câu hỏi cần chốt** (không khẳng định MBAL); tên hãng chưa nêu.
- Độ chính xác trích xuất ghi "chưa đo" (chưa chạy POST /request/compare).
- Ngày trong deck: enablement 07/09, build 08/09, pitch 7–8/10, kết quả 22/10 — khớp KBYG.
- Không có URL công ty trên slide cuối (chưa xác nhận domain).
