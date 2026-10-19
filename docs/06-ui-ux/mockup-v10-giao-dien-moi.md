# Mockup v1.0: giao diện mới (13/10/2026)

File: `E:\Hackaton\Prototype_Insurance_BnK_v1.0.html`, dựng trên v0.9.

v1.0 giữ đủ mọi màn hình, chức năng và thông tin của v0.9 (đã đồng bộ với ClaimMVP rev 689 và MyInsurance rev 172). Chỉ phần giao diện được làm lại, theo concept trên canvas "BnK Insurance — UI 1.0 concept".

Phần mới nằm ở khối `#v10css` (CSS) và khối JS `v1.0` ở cuối file. Hàm `render()` gốc gọi `v10Shell()` để dựng khung trang.

## Nguồn tham khảo trên ODC (đọc theme thật)
- **HomeBankingPortal** (rev 22):
  - Bố cục `LayoutTopMenu`, font Sora.
  - Thang màu `--color-neutral-0…10`, đảo màu khi bật `:root.dark-mode`.
  - Các lớp: `.colored-card.orange/.cyan/.yellow`, `.account-card`, `.item-card`, `.circle-bg-icon`, `.counter-card`, `.status-filter`, `.button-group`, `.floating-ai-chat`, `.ai-chat`.
  - Nền `Curves.svg` có hiệu ứng `mix-blend-mode: color-dodge`.
- **MyInsuranceBackoffice**:
  - Bố cục `LayoutSideMenu` với màu menu `#04313F`, font Sora.
  - `.ai-sidebar`, `.sidebar-card.required/approved/rejected`, `.activity-item`, `.counter-card`.

## Thay đổi chính
- **Khung trang:**
  - Bỏ sidebar, dùng top menu như Home Banking.
  - Console có menu: Tổng quan · Work list · (Approval queue khi ở vai người duyệt) · Operations queue · Tham chiếu ▾ · Kỹ thuật ▾.
  - Portal có menu: My cover · Benefits · New claim · Claims · Send documents. Thông báo không còn nằm trên menu (xem mục Chuông thông báo).
  - Menu người dùng (avatar) chứa: đổi vai, công tắc "Chỉ xem phần đã có trên hệ thống", xem bản điện thoại, đăng xuất.
  - Nút EN|VI, giao diện sáng/tối, chuông thông báo.
  - Trên điện thoại (≤ 760px) header có 2 hàng: hàng trên là logo và các nút, hàng dưới là menu cuộn ngang.
- **Chuông thông báo (13/10 tối):** bấm vào chuông thì danh sách sổ xuống ngay dưới chuông.
  - Mỗi dòng có biểu tượng màu, mã hồ sơ, nội dung, thời gian và chấm "mới". Bấm vào một dòng là mở đúng hồ sơ (với console thì mở đúng tab).
  - Có nút "Đánh dấu đã đọc"; số trên chuông về 0.
  - Đóng khi bấm ra ngoài hoặc nhấn Esc.
  - MyInsurance: thông báo hồ sơ; chân danh sách có "Xem tất cả thông báo" (màn Thông báo vẫn còn).
  - Console: lấy từ hộp việc, có nhãn mức (Cần xác nhận / Cảnh báo / Lỗi – rủi ro); chân danh sách mở hộp việc trong panel trợ lý.
  - Trên ODC làm bằng Balloon của OSUI hoặc danh sách `.balloon .card-detail` như Task Notification của Home Banking.
- **Theme:**
  - Mặc định là nền tối navy `#040D3F` (Home Banking dark-mode). Có bản sáng.
  - Nền có vệt màu và sóng chuyển động.
  - Thẻ có viền mờ; nút bo tròn (nút chính trắng, nút phụ có viền).
  - Trạng thái dạng viên: nền nhạt màu, chữ sáng, đủ tương phản.
  - Tab chi tiết hồ sơ dạng viên (giống status-filter).
- **Dashboard console (bản cuối 13/10, chép bố cục HomeBankingPortal/Dashboard):**
  - Cột trái:
    - **Ba thẻ xếp chồng kiểu carousel** (`account-card`): Đang thẩm định, Chờ phê duyệt, Chờ chi trả.
      - Thẻ trước to; hai thẻ sau tối dần, thẻ cuối có góc khuyết. Có chấm phân trang.
      - Bấm thẻ phía sau thì thẻ đó trượt lên trước. Bấm thẻ đang ở trước thì mở danh sách tương ứng.
    - **Hàng nút viên** (như Transfer / Pay / Add): Nhận việc ⇄ · Danh sách (hoặc Phê duyệt khi ở vai người duyệt) · Nhận thêm ⊕.
    - **"Cần bạn"** như Last Transactions:
      - Thẻ `item-card horizontal listItem` cao 100px.
      - Lấy từ todos() của từng hồ sơ; bấm vào là mở thẳng đúng tab.
    - **"Luồng hồ sơ"** như biểu đồ Balance:
      - Cột đôi xanh/đỏ, chọn Theo tuần hoặc Theo tháng.
      - Kỳ hiện tại (13/10) sáng lên, nhãn trục dạng viên `.selected-label`, rê chuột hiện tooltip trắng.
    - **Ba `counter-card`:** tự duyệt 10%, độ chính xác đọc chứng từ 100%, xử lý trung bình 1,1 ngày.
    - Bên dưới vẫn giữ "Hàng đợi của tôi".
  - Dải phải (như layout-right-side của Home Banking, nền gradient 270deg kéo hết chiều cao):
    - **Ba `colored-card lightDark`**, hình minh hoạ tràn lên trên mép thẻ và trôi nhẹ:
      - 3.199 lượt agent → Trace.
      - AG-4 chặn 1 hồ sơ → tab Phê duyệt của hồ sơ E.
      - 5 việc vận hành → Operations queue.
    - **"Mục tiêu (1)"** là thẻ cam (như Your Goals): Tự duyệt hôm nay 6 trên 15 hồ sơ, 41%. Có nút viền trắng "Xem" và thanh tiến độ.
    - **"Hạn xử lý (1)"** là thẻ vàng (như Loans): tuổi hồ sơ đang mở, trung vị 2 ngày, mục tiêu ≤ 1 ngày.
  - Màn hình hẹp:
    - Dưới 1180px: dải phải xuống dưới cột trái.
    - Dưới 760px: ba thẻ xếp dọc.
- **Chi tiết hồ sơ:**
  - Đầu trang thành thẻ hero: số tiền to, các ô thông tin, thanh bước có hiệu ứng nhịp.
  - Ở tab Tóm tắt, câu 1 thành thẻ gradient lớn.
  - Panel trợ lý nền tối, nằm đè bên phải; nội dung trang chừa chỗ cho panel.
- **MyInsurance trang chủ:**
  - Ba thẻ quyền lợi (Ngoại trú / Nội trú / Nha khoa) như thẻ tài khoản ngân hàng.
  - Hàng nút: Tạo hồ sơ / Gửi chứng từ / Giấy chứng nhận.
  - Danh sách hồ sơ; thẻ theo dõi hồ sơ với 4 bước có hiệu ứng.
  - Thẻ cho từng thành viên được bảo hiểm.
- **Kết quả hồ sơ:** thẻ đầu nền teal; có confetti khi hồ sơ đã được chi trả.
- **Đăng nhập:** nền navy, form bên trái, minh hoạ isometric bên phải (thẻ hồ sơ, chồng xu, khiên, nhãn AG-3).

## Mang sang ODC thế nào
1. Theme: dùng cùng token với Home Banking (neutral 0–10, `:root.dark-mode`) và font Sora. Có thể lấy theme HomeBankingPortal làm điểm xuất phát.
2. Layout: ClaimMVP và MyInsurance đổi sang `LayoutTopMenu`. Hai nhóm Tham chiếu ▾ và Kỹ thuật ▾ là menu thả xuống.
3. Dashboard console: chép cấu trúc màn Dashboard của HomeBankingPortal.
   - Cột chính gồm carousel `account-card`, `button-group`, list `item-card`, chart và `counter-card`.
   - Placeholder layout-right-side gồm `colored-card lightDark` và các thẻ `colored-card orange/yellow`.
   - Chỉ cần thay dữ liệu bằng aggregate hồ sơ.
4. Thẻ:
   - `account-card` cho các thẻ số liệu xếp chồng.
   - `colored-card.orange/.cyan/.yellow/.lightDark` cho thẻ mục tiêu và thẻ thông báo.
   - `item-card horizontal` cho danh sách "Cần bạn" và "Hồ sơ của tôi".
   - `counter-card` cho các ô chỉ số.
5. Biểu đồ: dùng Column chart của OutSystems Charts. Nhãn tuần đang chọn dùng `.selected-label`, giống Home Banking.
6. Panel trợ lý: dùng `.ai-sidebar` và `.sidebar-card` của MyInsuranceBackoffice, hoặc `.floating-ai-chat` của Home Banking.
7. Chuông thông báo: Balloon của OSUI gắn vào nút chuông. Nội dung lấy từ bảng thông báo (portal) và hộp việc (console).
8. Hiệu ứng (số chạy, thanh dài dần, vòng tròn, ánh quét, confetti, carousel) chỉ cần CSS keyframes/transition và một client action nhỏ để chạy số và đổi vị trí thẻ.

## Kiểm tra
- Đã mở bằng Playwright: mọi màn BO và CUST, đủ 9 tab của mọi hồ sơ, panel trợ lý, đổi vai, bản điện thoại, VI và giao diện sáng. Không có lỗi JS.
- Chuông thông báo: mở và đóng đúng; bấm một dòng thì mở đúng hồ sơ và tab; menu MyInsurance không còn mục Thông báo.
- Dashboard: bấm thẻ sau thì thẻ đó lên trước; bấm thẻ trước thì mở Work list. Ở bề ngang 420px không có cuộn ngang.
