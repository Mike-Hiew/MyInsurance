# MyInsurance pitch-ready — phiên Cowork 01–02/10 (ĐỌC TRƯỚC KHI SỬA MyInsurance / ClaimMVP)

> **⚠️ 05/10 ~11:00 (VN) — MyInsurance hiện ở rev 121 (phiên Cowork, UI v7 đợt 3 + U8 + U16).** Phiên nào mở MyInsurance từ rev ≤ 120: nạp lại trước khi publish.
> (cũ) 03/10 ~02:00 — rev 97.
> (cũ) 02/10 ~21:30 — MyInsurance ở rev 89, do phiên ClaimMVP sửa.** PO cho phép vì phiên Cowork đã dừng từ 11:51. Phiên nào đang mở MyInsurance từ rev ≤ 88: **nạp lại trước khi publish**, nếu không sẽ xoá mất rev 85–89.
> (cũ) 02/10 ~11:55 — MyInsurance ở rev 84 (phiên Cowork).
> **Hai phiên chạy song song (PO xác nhận 02/10 11:40).** Chia việc: **ClaimMVP = phiên IDP/Thông báo** (`ke-hoach-hoan-thien-02-10`); **MyInsurance + agent trợ lý = phiên Cowork**. Phiên Cowork KHÔNG publish ClaimMVP.

## UI v7 — ý tưởng design đã vào mockup (03/10, chờ PO chốt)
- Mockup: `E:\Hackaton\Prototype_Insurance_BnK_v0.6.html` (v0.5 + lớp v7; nút tím "Kịch bản demo" chạy 9 bước).
- Spec implement: `E:\Hackaton\00-v7-design-upgrade.md` = project doc `claude/ui-v7-design-upgrade-spec.md` (D1–D10: màn ODC đích, nguồn dữ liệu, tiêu chí chấp nhận, mẫu prompt Mentor).
- Phân công khi chốt: ClaimMVP D2/D3/D4/D7/D8 → phiên ClaimMVP; MyInsurance D5/D6/D10 → phiên Cowork; D1/D9 mỗi phiên làm phần app mình.
- **Bản 2 (PO 03/10): chỉ enhance UI trên dữ liệu đang có** — bỏ mọi số minh hoạ/ROI/giả định, bước AG-3/AG-5 chưa tồn tại, nút Thư kết quả, câu hứa thời gian. D7 chỉ còn dọn nhãn.
- **MyInsurance đã implement (rev 90–95, xem bảng rev).** ClaimMVP chưa làm (chờ phiên ClaimMVP xong P1). Còn 2 câu mở ở §6 (D8 cue card hay StoryBar; thống nhất tên khách Mai/An).

## Quyết định PO 02/10 11:40 — việc cho phiên ClaimMVP
1. **CLM-2026-0001 (ngoại trú): bỏ yêu cầu bổ sung Giấy ra viện.** → **Xong ở MyInsurance 85–86.** Không làm được bằng dữ liệu, vì khối đòi Giấy ra viện là chữ viết cứng trong PortalSupplement (điều kiện `ClaimNo = "CLM-2026-0001"`).
2. **Console VI/EN** → **Xong** ở ClaimMVP 244–259: nút EN|VI thật; rev 256 sửa CSS để nút hiện rõ.

## Lịch sử
- Xung đột 02/10 ~01:00: ClaimMVP rev 241 ghi đè rev 239–240; đã khôi phục ở rev 242–243 (`CustomerAssist_GetClaim/GetLimits`). 8 hồ sơ rác chuyển sang member FX-M1 (không xoá).
- **Luật:** trước mỗi `mentor_publish`, so `app_revisions` với rev phiên đang nạp; khác thì nạp lại. Một phiên một app tại một thời điểm.

## MyInsurance — các rev
| Rev | Phiên | Thay đổi |
|---|---|---|
| 55–75 | Cowork | Login portal khách; badge theo nhãn khách; ClaimDetail nhãn ngày + ẩn dòng khi chưa thẩm định; NewClaim không báo lỗi khi mở; Supplement đủ 7 loại; **song ngữ `T(En, Vi)` đủ 7 màn**; chat gọi agent (member lấy server-side) |
| 76–80 | Cowork | Căn phải cột tiền "Chi tiết từng khoản"; header chat xuống dòng; "Hỏi về hồ sơ này" mở chat bằng JS click `.pt-chat-fab` (trạng thái panel = biến local `IsChatOpen`, không dùng Client var) |
| 81–82 | IDP/Thông báo | Thông báo đọc `CustomerNotification`, badge chưa đọc, mặc định "vi", chat gửi kèm số hồ sơ đang xem |
| 83 | Cowork | Bỏ nút dark mode (không có theme tối, nút chết); bỏ câu "Quyền lợi ngoại trú áp dụng từ 31/03/2026" trong thông báo hợp đồng (mâu thuẫn màn Quyền lợi 01/01/2026); EN hint "GIẤY RA VIỆN (discharge paper)" |
| 84 | Cowork | Badge sidebar = 0 thì ẩn thật: thêm `.cc-nav-badge.cc-hidden{display:none !important}` (rule `.cc-hidden` cũ thua `.cc-nav-badge{display:inline-block !important}` khai báo sau). Badge "Hồ sơ & tiến trình" là **placeholder cứng "1" chưa nối dữ liệu**, nay ẩn; muốn hiện thì nối với số hồ sơ đang xử lý |
| 85–86 | ClaimMVP (PO cho phép) | Bỏ khối đòi Giấy ra viện + banner viết cứng cho CLM-0001 ở PortalSupplement. NewClaim bước 2: bỏ trạng thái giả "2 photos · sharp" / "file uploaded"; mỗi dòng hiện tên tệp thật hoặc "Chưa có tệp"; nút "Tải lên" / "Chụp ảnh"; dòng Hoá đơn VAT nối `Portal_ReadDocument2` (T1-04) |
| 87 | ClaimMVP | Câu báo sau khi nộp song ngữ: "Đã gửi hồ sơ. Chúng tôi đã nhận N/M chứng từ và đang đọc." |
| 88–89 | ClaimMVP | Nút "Tiếp tục — chứng từ" (bước 1) luôn chạy `OnStep1Continue`: tính lại mọi ô, thiếu ô nào thì báo ô đó; "Đến ngày" không bắt buộc. Rev 88 nối sai thứ tự node, rev 89 sửa |
| 90–95 | Cowork (UI v7) | **D1** tokens `--ai*`, `.v7-ai-chip` (chip "Trợ lý AI" ở header chat), nút "Hỏi về hồ sơ này" gradient tím. **D5** PortalClaimDetail: PayoutCard thành "biên nhận" (đầu teal + mép răng cưa); donut conic-gradient + chú giải (chỉ hiện khi hồ sơ đã có kết quả — cùng điều kiện WhyCardIf); WhyCard thêm danh sách dòng bị giảm (−số tiền + mô tả + lý do decode sẵn có); **bỏ chữ viết cứng "Vietcombank •••• 4182"**; WhatNext bỏ câu viết cứng "một khoản thuốc… trong hôm nay" → theo GetPendingRec, "khi có kết quả". **D6** PortalHome dải chào "Chào anh An," + hồ sơ mới nhất (bản ghi đầu của GetRecentClaimsWithStatus). **D10** nhãn "Hỏi trợ lý" cạnh FAB (ẩn <600 px), dòng "Đang chờ trợ lý · N s" đếm giây thật khi chờ agent. **D9** animation fade/donut, tôn trọng reduced-motion. Rev 91 lỗi Index -1 (đọc list trước khi fetch) → rev 92 bọc If IsDataFetched. Kiểm VI/EN, 390 px (iframe) OK. |
| 96–97 | Cowork | PO 03/10: chat tiếng Việt vỡ layout (tiêu đề 2 dòng đẩy ô nhập ra ngoài khung). **Bỏ 3 câu hỏi gợi ý**; Chatbox thành flex cột cao min(560px, 100vh−120px): header cố định, `.pt-chatbox-body.v7-chat-msgs` co giãn + cuộn, footer ô nhập luôn ở đáy. Kiểm VI: hỏi Atorvastatin → trả lời đầy đủ, không đè header. |
| 111–118 | Cowork (05/10, đợt 3 kế hoạch `ui-ux-plan-sat-mockup-05-10`) | PortalClaimDetail: **S12** thẻ chứng từ cần bổ sung (chỉ khi Chờ bổ sung; chip Bắt buộc/Tuỳ chọn, hạn, nút Tải lên → PortalSupplement) + **S13** thư kết quả (in được) qua `GetClaimExtras` (Portal_GetMissingDocuments2 / Portal_GetLetters2, member lấy server-side). Tiến trình đọc chứng từ sống `.v7-live` ("Đang đọc chứng từ · x/y" → "Đã đọc xong… đang chuyển thẩm định"), tự làm mới 15 s khi chưa ở trạng thái cuối, vòng nhấp nháy bước hiện tại. Count-up số tiền (biên nhận, chú giải, KPI Home), fade, skeleton, empty state (Hồ sơ, Thông báo). Lý do giảm tiền tiếng Việt `V7ReasonText` (NOTCOV/NOTREL/COPAY/CAP/WAIT, fallback "Điều chỉnh theo quyền lợi hợp đồng"); số "Vì sao" = TotalClaimed − TotalPayable. Rev 116: nộp hồ sơ xong → vào thẳng chi tiết hồ sơ mới; thông báo `CLM-NOTIF-LETTER` → chi tiết với `OpenLetters=true`. Rev 117–118: cuộn tới thẻ thư đợi thẻ render và màn chuyển xong (chuyển màn của ODC đặt lại scroll về 0) + viền sáng `.v7-flash`; đã kiểm: bấm thông báo thư 0440 → màn cuộn tới thẻ thư. Kiểm: 0440 (Paid: thư, donut, 1.880.000/120.000/2.000.000, lý do VI/EN), 0444 (S12 ẩn khi không chờ bổ sung), 0431 (thanh 5/5), 390 px không tràn. |
| 119 | Cowork | **U8 toast sau thao tác:** `window.v7Toast(msg, kind)` định nghĩa ở LayoutSideMenu OnReady (host cố định giữa đáy, aria-live, tự ẩn ~4 s, mobile cách đáy 88 px tránh FAB; tím = AI đang đọc, teal = xong, đỏ = lỗi; reduced-motion). Gọi ở: PortalNewClaim trước khi chuyển sang chi tiết ("CLM-… · Đã gửi hồ sơ. Chúng tôi đã nhận N/M chứng từ và đang đọc."); PortalSupplement OnSend thay Message thành công ("Đã gửi chứng từ bổ sung. Chúng tôi đang đọc.", lỗi giữ nguyên); PortalClaimDetail: nút Làm mới → "Đã cập nhật", trạng thái đổi khi tự làm mới → "Trạng thái mới: …" (tự làm mới không đổi thì im). Kiểm: 0440 bấm Làm mới → toast teal "Đã cập nhật" tự ẩn; 0444 tự làm mới 35 s → không toast. Hai đường nộp/bổ sung chưa chạy thật (tránh tạo dữ liệu rác). |
| 120–121 | Cowork | **U16 thẻ kịch bản demo 9 bước** (ẩn với khách; bật bằng `?demo=1`, tắt `?demo=0`): JS `V7StoryInit` ở LayoutSideMenu OnReady (Lang = Client.Lang), trạng thái localStorage chung origin nên dùng được cả ClaimMVP khi gắn cùng đoạn mã. Bước 8 viền sáng `.v7-receipt`. Câu dẫn không có số minh hoạ. Rev 121 sửa nhận diện "đang ở màn này" bỏ qua tham số thừa (`demo`). Chi tiết + đoạn mã cho ClaimMVP: `claude/u16-the-kich-ban-demo.md`. |

## Audit 02/10 11:30 (rev 82) + kiểm rev 83–84
- VI: 7 màn + 2 chi tiết hồ sơ — 0 chữ Anh sót, 0 `T(` lộ, 0 mã nội bộ/fixture/chữ kỹ thuật. EN: chỉ còn tiếng Việt ở tên người, cơ sở y tế, tên khoản chi (cố ý).
- Chat trên CLM-0001: "Khoản Atorvastatin 20mg — 30 viên hiện vẫn đang được thẩm định nên kết quả chưa phải là cuối cùng…" — đúng dữ liệu, không đoán kết quả.
- Rev 83–84: không còn nút dark mode; thông báo hợp đồng chỉ còn "Giấy chứng nhận đã sẵn sàng."; không badge nào hiện khi = 0.
- Lưu ý kiểm bằng Claude in Chrome: lần bấm toạ độ đầu tiên sau `navigate` đôi khi không tới trang (0 sự kiện, đo bằng listener) — không phải lỗi app. Ngay sau một lần publish, tab đang mở chạy VersionsManager nâng cấp, nên vài giây đầu bấm nút không có tác dụng → tải lại trang.

## Kiểm 02/10 tối (rev 85–89)
- Wizard nộp hồ sơ đã chạy thật với IDP: CLM-2026-0432 và 0433. Ảnh khách tải lên được gửi IDP, đọc ra 3 dòng; hồ sơ tự chuyển *Đang thẩm định*; portal hiện từng khoản. Chi tiết ở `ke-hoach-hoan-thien-02-10`.

## Còn mở
- PortalClaimDetail "Bước tiếp theo" khi *Đang thẩm định* là câu viết cứng của kịch bản CLM-0001 ("cán bộ đang xem xét một khoản thuốc") — sai cho hồ sơ khác (thấy trên CLM-2026-0433).
- Donut + danh sách "Vì sao" chỉ hiện khi hồ sơ đã có kết quả — An chưa có hồ sơ nào đã quyết, nên chưa thấy trên demo cho tới khi phiên ClaimMVP xong P1 (duyệt → Paid).
- Lỗi dữ liệu ngoài phạm vi UI: CLM-2026-0432/0433 chi phí "1.050 ₫" (lỗi ×1000 IDP, phiên ClaimMVP đang theo).
