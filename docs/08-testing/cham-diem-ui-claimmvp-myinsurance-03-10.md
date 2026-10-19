# Chấm điểm UI — ClaimMVP (console) & MyInsurance (portal) · 03/10/2026

Nguồn: xem trực tiếp trên bnk-dev (ClaimMVP rev 243+, MyInsurance rev 84), vai Approver và vai khách Nguyễn Văn An. Điểm là đánh giá chủ quan theo thang dưới, nhìn bằng mắt giám khảo hackathon.

## Thang 100
| Tiêu chí | Max | ClaimMVP | MyInsurance |
|---|---|---|---|
| Thẩm mỹ & nhất quán | 20 | 16 | 17 |
| Kiến trúc thông tin & điều hướng | 15 | 12 | 13 |
| Kể chuyện bằng dữ liệu (số, biểu đồ, trước/sau) | 15 | 8 | 10 |
| AI/agent nhìn thấy được & giải thích được | 15 | 6 | 10 |
| Ngôn ngữ & câu chữ | 10 | 5 | 9 |
| Tương tác, trạng thái, phản hồi | 10 | 8 | 7 |
| Vệ sinh dữ liệu demo | 10 | 6 | 7 |
| Responsive & khả năng đọc | 5 | 3 | 2 |
| **Tổng** | **100** | **64** | **75** |

## ClaimMVP — mạnh
- Bộ khung đẹp, cùng họ với portal: sidebar teal, card bo góc, badge màu, header hồ sơ có stepper 5 bước + "Proposed payable" nổi bật.
- Tab Extraction & costs: chứng từ gốc bên trái, lưới trích xuất có độ tin cậy (97%/94%) bên phải, dòng bị gắn cờ được tô — đúng thứ giám khảo muốn thấy.
- So sánh Run #3 / Run #4 bằng thẻ; banner "To do: Decide the proposal on line 4" rõ việc cần làm.

## ClaimMVP — yếu
1. **Agent bị giấu**: panel AG-2 nằm cuối tab Calculation & ledger, chữ tiếng Việt trong UI tiếng Anh ("Cuộc hội thoại mới", "Tính và giải thích"). Giám khảo không cuộn tới.
2. **Tab Calculation = log kỹ thuật**: "TERM-HC-002 — KHÔNG_THUỘC_PHẠM_VI", "[BR-WAIT-1-5]", "200.1", "posting AMOUNT"; tên bước tiếng Việt trong UI tiếng Anh; cột Result lặp 3.128.000 mà không làm nổi bước nào đổi tiền.
3. **Mã nội bộ lộ**: chip trạng thái "ExtractionError", "UnderReview", "PendingApproval" cạnh "Pending info", "Payment in progress"; "Payment (F58)"; biểu đồ tuần ghi "T1…T9".
4. **Dashboard mỏng**: 3/4 KPI = 0 ("Pending approval 0", "Awaiting payment 0", "Auto-approved today 0"); biểu đồ mỗi tuần 1 cột cao 1.
5. **Technical trace**: ~70 chip hồ sơ gồm cả fixture `TST-T-CH…` và 0034–0041; bảng sự kiện dạng log, tên agent seed AG-0…AG-6 không khớp agent thật (AG-2, Customer Assistant).
6. Administration đầy nút dev (Run IdpPing, Delete 9 orphan keys…).

## MyInsurance — mạnh
- Sạch, dễ hiểu, song ngữ đầy đủ, câu chữ lịch sự "Quý khách".
- Home có 3 KPI có nghĩa (người được BH, hạn mức còn lại, hồ sơ đang xử lý); stepper 4 trạng thái cho khách.
- Chatbot là agent thật, trả lời đúng dữ liệu (Atorvastatin "đang thẩm định, chưa phải kết quả cuối"), không bịa.
- Thông báo đọc từ dữ liệu, badge chưa đọc.

## MyInsurance — yếu
1. **Thiếu khoảnh khắc "kết quả"**: An không có hồ sơ nào đã chi trả → khối "Vì sao Quý khách tự chịu" (màn đắt giá nhất cho khách) không xuất hiện.
2. **Chat chờ ~40 s** chỉ có "Đang xem hồ sơ…"; nút chat là icon tròn không nhãn, khó thấy.
3. **Chưa kiểm mobile 390 px** — portal khách mà chỉ đẹp trên desktop là mất điểm.
4. Home nửa phải trống phía dưới; chỉ 2 hồ sơ nên trông "rỗng".
5. CLM-0001 ngoại trú vẫn bị đòi Giấy ra viện (chờ phiên ClaimMVP sửa dữ liệu).

## Cải thiện để gây ấn tượng — ưu tiên theo tác động / công sức
| # | Việc | App | Tác động | Công sức |
|---|---|---|---|---|
| 1 | **Thẻ "AI đã làm gì cho hồ sơ này"** ở đầu Claim detail: dòng thời gian Đọc chứng từ (9 mẫu, % trường không phải gõ) → Kiểm tra lâm sàng gắn cờ dòng 4 → Chuỗi 14 bước → Cổng 4/4 → Kết quả; nút "Hỏi AG-2" mở panel ngay đó | ClaimMVP | Rất cao | 2–3 lượt Mentor |
| 2 | **Biểu đồ thác tiền (waterfall)** đầu tab Calculation: 3.128.000 yêu cầu → −610.000 vượt trần → −168.000 không liên quan chẩn đoán → −470.000 đồng chi trả → 1.880.000. Log 14 bước thu vào "Chi tiết kỹ thuật" | ClaimMVP (+ portal dùng lại) | Rất cao | 2 lượt |
| 3 | **Một hồ sơ đã chi trả cho An** (chạy CLM-0001 tới PAID trước buổi pitch hoặc seed hồ sơ mới) để portal hiện kết quả + "Vì sao Quý khách tự chịu" + thông báo 005 | Cả hai | Rất cao | 1 lượt + thao tác |
| 4 | Chuẩn hoá nhãn trạng thái console (Read failed / Under review / Pending approval…), bỏ "F58", "T1…T9" → "W36…W40", tên bước + chi tiết bước theo ngôn ngữ đang chọn | ClaimMVP | Cao | 2 lượt |
| 5 | Technical trace: lọc bỏ `TST-`/`FX-`, chỉ agent thật; thêm **sơ đồ agent** (Portal → IDP → AG-2 → Gate → Customer Assistant) có tô bước đang chạy | ClaimMVP | Cao | 2 lượt |
| 6 | Dashboard: seed 6–8 tuần dữ liệu hoặc đổi KPI sang số đo thật (thời gian xử lý TB, % tự duyệt, % trường không phải gõ, phút tiết kiệm/hồ sơ) | ClaimMVP | Cao | 1–2 lượt |
| 7 | Chat: nút có nhãn "Hỏi trợ lý", hiện tiến trình từng bước khi chờ ("Đang đọc hồ sơ… Đang kiểm tra hạn mức…"); trong pitch dùng 3 câu gợi ý đã kiểm | MyInsurance | Trung bình–cao | 1 lượt |
| 8 | Kiểm & sửa 390 px, trình diễn portal trong khung điện thoại | MyInsurance | Trung bình | 1–2 lượt |
| 9 | Gom nút dev ở Administration vào mục "Developer tools" thu gọn | ClaimMVP | Thấp | 1 lượt |

## Kịch bản "wow" 3 phút gợi ý
1. Điện thoại (portal): khách nộp hồ sơ → "Đang xử lý chứng từ" + thông báo.
2. Console: chip "Reading…/Replay" → lưới trích xuất có độ tin cậy, dòng Atorvastatin bị gắn cờ.
3. Cán bộ xác nhận khuyến nghị → **waterfall** chạy lại 2.000.000 → 1.880.000 → cổng 4/4.
4. Hỏi AG-2 "Vì sao giảm 120.000?" → trả lời trích bước.
5. Quay lại điện thoại: kết quả + "Vì sao Quý khách tự chịu" + hỏi chatbot bằng tiếng Việt.
