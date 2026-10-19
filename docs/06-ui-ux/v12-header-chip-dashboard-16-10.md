# v12 — Header nổi bật, chip rõ ở nền tối · v13 — Dashboard theo mockup (16/10/2026)

**Yêu cầu Mike (11:50):** Dashboard ClaimMVP bố cục không hợp lý; header ở mọi màn của 2 app phải nổi bật, rõ ràng; chip dùng opacity thấp nên sang dark mode khó nhìn, khó phân biệt. Muốn rõ, đồng bộ, sáng tạo, hiện đại.
**13:51:** "dashboard điều chỉnh lại giống mockup đi, sao lại khác vậy" → bỏ bố cục Dashboard v12 (5 thẻ KPI / lưới 15 cột), làm lại theo artboard "Console · Dashboard" của canvas "BnK Insurance — UI 1.0 concept" (project/Main.dc.html) = v13.

## Bản hiện tại (16/10 ~14:00)
- **ClaimMVP 775** (trước đợt: 770). 771 = CSS v12 + "Cần bạn" song ngữ (vỡ Dashboard); 772 = sửa vỡ; 773–774 = tiêu đề bảng Quản trị + cỡ tiêu đề Agent map; 775 = xoá CSS Dashboard v12, thêm v13 theo mockup. Validation giữ 3 lỗi nền.
- **MyInsurance 247** (trước: 246), 0 lỗi.
- Điểm lùi: ClaimMVP 770, MyInsurance 246. Mọi thay đổi là CSS nối cuối theme (khối `v12 · Header & chip`, các rule thead/mc-table/.cc-page-title, khối `v13 · Dashboard theo mockup` part A/B) + 1 thay đổi nhãn "Cần bạn".
- Kiểm sau publish 775: 0 rule Dashboard v12 còn lại, 61 rule v13 có hiệu lực; Dashboard nền tối VI khớp bản xem trước (cmp: dashboard-mockup-775.jpg). Nền sáng đã xem trước, ổn.
- ChainTests chưa chạy lại (chỉ CSS + nhãn hiển thị).

## Dashboard v13 (theo mockup Console · Dashboard)
- Lưới `.content-middle:has(> .v10-db)`: `1fr | 26px | 2fr | 32px | 300px` (row-gap 30). `.v10-db`, `.v10-db-main`, `[id$=ChartCols]` và `.columns` bên trong = `display:contents` (cần `:not(#v13)` vì rule OSUI `.columns` mạnh hơn).
- Trái, cột 1–3: hàng 1 nhãn "Tổng giá trị đang xử lý" (13 px #9FB0FF) trên số 34/700, nút phụ bên phải · hàng 2 ba thẻ xếp so le như mockup (flex, chồng mép −14 px, cao 196/172/152, lệch 0/10/20 px; gradient #FF7A59→#F0406F, #6E3CF6→#3A1FA6, #0D7C79→#0A4F5A; chữ trắng; số 52/42 px; dòng gợi ý dạng viên trắng 22 %) · hàng 3 nút Nhận việc / Danh sách (trắng) / Nhận thêm (viền), cao 46 · hàng 4 "Cần bạn" (1 phần, không khung) | biểu đồ trong panel (2 phần) · hàng 5 ba ô tuổi hồ sơ (panel, chấm tròn 44 px, số 24/700 màu, nhãn dưới).
- Phải, cột 5 (300 px): `.v10-db-right` thành cột flex gap 20, bỏ nền dải phải; 3 thẻ minh hoạ (Promo1 nền #2B2CA8→#1A1470, khối lập phương chuyển sang phải dưới; Promo2/3 kính trắng 12→4 %) · "Mục tiêu" · thẻ "Tự duyệt hôm nay" #FF7A59→#E8336E · thẻ "Tuổi hồ sơ đang mở" #FFA41B→#F06A1D với thanh tiến độ trắng. Ẩn "Dành cho bạn", "Hạn xử lý", chấm carousel.
- Dưới cùng: "Hàng đợi của tôi" + bảng toàn chiều rộng.
- Khác mockup do dữ liệu thật: không có lời chào/ngày "Live", thẻ tự duyệt không có vòng %, ô tuổi không có dòng phụ, chưa có nút Weekly/Monthly ở biểu đồ.
- ≤1100 px: 1 cột; ≤640 px: ô tuổi 1 cột.
- "Cần bạn" song ngữ (Mentor, 771): Chờ phê duyệt, Đề xuất AG-3, Tín hiệu AG-4, Thư cần gửi, Chờ chứng từ, Cần xem; "N ngày", "hôm nay".

## Header (cả 2 app)
- Tiêu đề trang `.content-top-title h1` 30/800, chữ gradient (tối: trắng→lavender→cyan; sáng: navy→indigo→teal). `.cc-page-title` nằm trong nội dung (Agent map) giữ 22 px.
- Tiêu đề mục: `h2/h3/h4` trong `.main-content` + `.v10-ny-t, .v10-db-rh, .cc-section-title, .cc-card-title, .v11-g4-t, .v10-ph-t, .mk-h, .mc-h, .cc-inbox-title` → 17–20 px/780 + vạch tín hiệu gradient cyan→tím→hồng 4 px (background-image). Tối: chữ trắng có quầng tím; sáng: #0B1033.
- Ngoại lệ: tiêu đề lồng nhau chỉ giữ vạch lớp ngoài; `h3[id*=Qh]` (ClaimDetail có ô số 1–4) không vạch; `[id$=P2PhT]` (mã hồ sơ trong hero PortalHome); `[id$=MemberNameHtml]`.
- Tiêu đề bảng `thead th`: tối nền đặc #2B2F7E→#1D2368, chữ #E8ECFF 700, giãn chữ, viền dưới; sáng #EEF1FF / #26307A. `.columns-item .mc-table thead th` bỏ sticky (trước đó trượt 52 px đè dòng đầu ở màn Quản trị).

## Chip (cả 2 app)
- Biến màu theo nhóm: `--v12h` (RGB), `--v12t` (chữ nền tối), `--v12d` (chữ nền sáng). Nhóm: amber, cam, xanh lá, đỏ, hồng, tím, xanh dương, cyan, trung tính.
- Áp cho `.cc-st, .mc-badge-status, .v7-status-badge, .mk-badge, .tag.border-radius-rounded, .mc-tag, .mc-age, .cc-provider-chip, .mf-chip, .cc-pill, .cc-chip, .cc-kv-chip, .au-chip, .v11-pill`.
- Tối: nền gradient màu 34→18 %, viền 62 %, chữ sáng, quầng màu. Sáng: nền 13 %, viền 42 %, chữ đậm.
- Chip **trạng thái** (cc-st, mc-badge-status, v7-status-badge, mk-badge) có chấm LED đầu chip → phân biệt với chip cờ/ghi chú.
- Chip trên hero luôn tối của PortalHome (`[id$=Pane2]`) dùng kiểu nền tối cả khi app ở chế độ sáng.

## Bẫy đã gặp
- **Mentor lưu `content:""` thành `content:"`** → chuỗi không đóng, trình duyệt bỏ toàn bộ CSS phía sau (ClaimMVP 771 vỡ Dashboard). Không dùng `content:` trong CSS gửi qua Mentor. Kiểm tra sau publish bằng cách đếm rule trong `document.styleSheets`.
- Xem trước khi thay CSS đã publish: xoá rule cũ khỏi CSSOM (`deleteRule`) rồi chèn `<style>` mới — mô phỏng đúng bản sau publish.
- `:is()` lấy specificity cao nhất trong danh sách → biến mặc định đặt trong `:where()`.
- Rule theme có `!important` specificity cao → dùng `:not(#v12)`/`:not(#v13)` để cộng mức ID.
- `.cc-chart-h` là khung biểu đồ, không phải tiêu đề.
- Connector OutSystems trên máy Mike treo ~12:15–13:25; hồi lại sau khi Mike khởi động lại.

## Còn lại
- Agent map (Kỹ thuật): thẻ tác nhân bố cục chật, nút 7d/30d/90d chưa có kiểu — có từ trước.
- Danh sách "Cần bạn" ở EN hiện số tiền kiểu "1.900.000 ₫" (định dạng VI) — có từ trước.
