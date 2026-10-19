# ClaimMVP Dashboard — v15 → v26 (16/10)

Tiếp nối `v14-dashboard-dai-phai-16-10.md`. Mọi thay đổi CSS đều append vào cuối theme của ClaimMVP qua Mentor, theo từng khối `/* ===== vNN ===== */`. **Bản live hiện tại: 790.**

**Chuẩn để đối chiếu:** bản mockup HTML mới nhất là `E:\Hackaton\Prototype_Insurance_BnK_v1.0.html` (sửa 15/10), không phải canvas "UI 1.0 concept". Dashboard console trong file nằm ở `BO.dashboard` (lớp `.hbd*`), quanh dòng 2160–2300 (CSS) và 5885–5960 (JS).

## Các bản
| Rev | Khối | Nội dung |
|---|---|---|
| 777–778 | v15 | Các dòng "Cần bạn" có màu theo loại việc; thêm 3 ô KPI (về sau đã bỏ); biểu đồ cao 260 px. |
| 779 | v17 | Bỏ ô KPI và dòng "Đọc chứng từ tháng này"; bố cục giống HomeBankingPortal. |
| 780–781 | v18 | Thử làm gradient cho cột biểu đồ bằng SVG defs. Expression bị escape nên SVG hiện thành chữ; đã gỡ widget. |
| 782 | v19 | Cả trang nằm trong một màn hình; biểu đồ kiểu Banking (tuần cũ màu trầm, tuần hiện tại màu sáng, nhãn dạng viên thuốc). |
| 783 | v20 | Dải nền cột phải tràn tới mép màn hình; thẻ "Dành cho bạn" kiểu `colored-card lightDark`. |
| 784 | v21 | Thẻ trạng thái xếp hàng ngang, theo canvas concept. **Sai chuẩn**, đã thay bằng v22. |
| 785 | logic | Bỏ cơ chế trượt thẻ. **Sai so với mockup v1.0**, đã khôi phục ở 786. |
| 786 | logic + v22 | Khôi phục carousel đúng mockup v1.0: biến FrontIdx; class `pos-((k-FrontIdx+3) mod 3)`; bấm thẻ phía sau thì gán FrontIdx = k, bấm thẻ đang ở trước thì mở trang. Thêm chấm phân trang `V10DbDots` (3 div, div đang chọn có class `on`) nằm trong `V10DbStack`. |
| 787 | v23 | Sửa CSS chấm phân trang: chấm là `div` chứ không phải `span`. |
| 788 | v24 | Canh đáy 3 cột (Cần bạn, biểu đồ, Mục tiêu) về cùng một đường. Chi tiết ở mục v24 bên dưới. |
| 789 | v25 | Nền light mode theo mockup v1.0 (áp cho mọi màn ClaimMVP). Chi tiết ở mục v25 bên dưới. |
| 790 | v26 | Nền thanh menu trên giống HomeBankingPortal (Curves.svg). Chi tiết ở mục v26 bên dưới. |

## v22 — carousel 3 thẻ theo `.hbd-cards` của mockup v1.0
- Khung `V10DbStack` cao 192 px, các thẻ đặt `position:absolute`. Chuyển động `left/top/width/height` 0,55 s `cubic-bezier(.2,.8,.2,1)`, `filter` và `box-shadow` 0,55 s.
- Vị trí và kích thước từng thẻ:

  | Vị trí | left | top | width | height | Hiệu ứng |
  |---|---|---|---|---|---|
  | pos-0 | 0 | 0 | 45% | 188 | bóng `0 26px 50px -22px rgba(228,44,102,.6)` |
  | pos-1 | 43% | 8 | 33% | 172 | `brightness(.62) saturate(.95)` |
  | pos-2 | 73% | 18 | 27% | 152 | `brightness(.55) saturate(.9)`, góc khuyết bằng `clip-path` |

- Màu theo trạng thái, gradient đúng biến của mockup, có thêm lớp ánh quét:
  - `is-o`: `--grad-orange` 150deg `#F55B46` → `#E42C66`.
  - `is-v`: `--grad-violet` `#6E3CF6` → `#3A1FA6`.
  - `is-c`: `--grad-cyan` 334deg `#005788` → `#00828A`.
- Chữ:
  - Nhãn 14 px; số 44 px (thẻ sau 36 px) kèm chữ "hồ sơ" 16 px; tiền 16/600.
  - Dòng chân "… · bấm để mở" 12 px; ở thẻ phía sau dòng này có `opacity:0`.
- Hover: thẻ sau sáng lên `.85` và nhích lên 4 px; thẻ trước nhích lên và xoay -0,4°.
- Chấm phân trang: 4×4 px, chấm đang chọn rộng 18 px màu trắng (light mode: `#CBCDDF` / `#040D3F`).
- Dưới 760 px: 3 thẻ xếp dọc, ẩn chấm.
- Đã kiểm tra trên trang live:
  - Bấm Card2 (thẻ sau) thì Card2 lên pos-0 và chấm 3 sáng.
  - Bấm Card2 lần nữa (lúc này ở trước) thì mở WorkList.
  - Trang vẫn không cuộn.

## v24 — canh đáy 3 cột (rev 788)
- **Nguyên nhân lệch:** cột trái và biểu đồ nằm ở hàng 4 của lưới `.content-middle`, nên co giãn theo chiều cao màn hình và kết thúc ở `100vh − 44px`. Cột phải `.v10-db-right` có chiều cao nội dung cố định, kết thúc ở khoảng 862 px. Thêm vào đó, hai thẻ `<script>` trong list "Cần bạn" bị CSS khác ép thành `display:flex`, nên mỗi thẻ tạo thêm một khoảng 12 px ở đầu và cuối list. Kết quả ở màn 945 px: hàng cuối "Cần bạn" kết thúc ở 851, biểu đồ ở 901, Mục tiêu ở 862.
- **Cách sửa (chỉ CSS):**
  - Ẩn `script` trong list.
  - Khi có đủ 3 dòng, các dòng "Cần bạn" chia đều chiều cao (`flex:1 1 0`, bỏ `max-height`).
  - Cột phải: `padding-bottom:44px`, bỏ `gap` và đặt lại khoảng cách giữa các khối (12/14/22 px). 3 thẻ Dành cho bạn và 2 thẻ Mục tiêu dùng `flex:1 0 auto`. Thẻ promo căn giữa nội dung theo chiều dọc. Ở thẻ mục tiêu, dòng giá trị cuối và thanh tiến độ được đẩy xuống đáy.
  - Chỉ áp dụng khi màn hình từ 1100×820 trở lên. Dùng chuỗi `:not(#v24a…#v24j)` để thắng các rule `:not(#v13…)` có sẵn.
- **Đã kiểm trên bản live** (dùng iframe để đổi chiều cao màn hình), cả 3 cột kết thúc cùng một vị trí:
  - Màn 889: đáy ở 845.
  - Màn 945: đáy ở 901.
  - Màn 1080: đáy ở 1036.
  - Không màn nào phát sinh cuộn trang.
  - Dưới 836 px, cột phải không co lại được nữa, nhưng ở mốc này trang đã chuyển sang chế độ được phép cuộn.

## v25 — nền light mode theo mockup v1.0 (rev 789)
- **Trước khi sửa:** chỉ dark mode có lớp nền động: vệt màu ở `html.dark-mode .layout.layout-side::before` và đường sóng ở `::after`. Light mode chỉ có nền phẳng `#F4F6F9`. Dải cột phải ngả lavender (`#E6E9F8` → `#F2F4FC`).
- **Mockup v1.0 ở light mode:**
  - `--bg:#F3F5FB`.
  - `.v10-bg`: 3 vệt màu với `opacity .14`, gồm `#3B2BD9` ở góc trên trái, `#1C4FE0` ở giữa phải, `#7A2BD9` ở đáy giữa. Đường sóng SVG trên và dưới với `opacity .35`.
  - `.hbd-right::before`: `linear-gradient(270deg, #F8F9FA, #EEF0F7)`.
- **Đã làm (CSS, `html:not(.dark-mode)`, dùng chuỗi `:not(#v25a…f)`):**
  - Nền trang `#F3F5FB`.
  - `.layout.layout-side::before`: 3 radial-gradient alpha khoảng 0,15, chuyển động `v10mDrift` 22 s.
  - `::after`: 2 SVG sóng của mockup (data URI), `opacity .35`, chuyển động `v25wave` 18 s.
  - Dải phải: `linear-gradient(90deg, #EEF0F7, #F3F4F9)`, phần nối dài `::after` màu `#F3F4F9`.
- **Kiểm tra trên bản live:** đủ 5 rule `#v25a`; nền và dải phải đúng giá trị; WorkList cũng hiện nền mới; dark mode không bị ảnh hưởng. Ảnh so sánh: `light_bg_mockup_vs_v25.jpg`.

## v26 — nền thanh menu giống HomeBankingPortal (rev 790)
- **HomeBankingPortal làm thế nào:** `.header:before` dùng ảnh `HomeBankingPortal.Curves__sVJ4iSK6qvHeHMC1NiZQ.svg` (SVG 1600×88, 65 KB), `background-size:cover`, `opacity .6`, nền header trắng. Ở dark mode: nền header `#141C4E`, lớp ảnh `mix-blend-mode:color-dodge` với `opacity 1`.
- **ClaimMVP:** thanh menu là `.layout .app-menu-content` (fixed, cao 76 px). Đã thêm `isolation:isolate` và lớp `::before` dùng chính ảnh Curves của HomeBankingPortal. Dark mode: nền `#141C4E`, color-dodge, bỏ blur. Light mode: nền trắng với `opacity .6`. Không dùng `overflow:hidden` để menu thả xuống (Tra cứu, Kỹ thuật) không bị cắt.
- **Lưu ý ODC:** nếu viết `url('/HomeBankingPortal/img/…')`, ODC hiểu đó là resource của app và báo 3 lỗi "Unknown Object". Phải dùng URL tuyệt đối `https://bnk-dev.outsystems.app/HomeBankingPortal/img/…`. Hệ quả: ảnh phụ thuộc HomeBankingPortal trên bnk-dev. Nếu HomeBankingPortal publish lại và tên file băm thay đổi thì phải cập nhật URL này.
- **Kiểm tra:** cả 5 rule `#v26a` có trong CSS live; ảnh tải về đủ 1600×88. Trước khi publish, mình đã xem trực quan cả dark lẫn light (bằng cách inject CSS) và thử menu thả xuống. Sau khi publish, phiên trình duyệt bị đăng xuất nên chưa chụp lại được bản live đã đăng nhập.

## v19–v20 (tóm tắt)
- Vừa một màn hình: `.content-middle` cao `calc(100vh - 76px)`. Màn thấp hơn 820 px hoặc hẹp hơn 1100 px được phép cuộn.
- Không đặt `overflow:hidden` lên `.v10-db-right`, vì sẽ cắt mất dải nền nối dài sang phải (`::after`).

## Lưu ý khi làm việc với Mentor
- Không dùng `content:""` trong CSS gửi qua Mentor.
- 3 thông báo "Unavailable feature" (multipart) là baseline; cần ghi trong prompt rằng các bản trước vẫn publish bình thường với 3 thông báo này.
- Phải kiểm tra trang live sau mỗi lần publish: đếm rule theo marker `#vNN`, kiểm tra HTML có bị escape không, và bấm thử các tương tác.
- Khi Mike nói "đúng theo mockup", phải mở file v1.0 trên máy anh để đối chiếu, không lấy canvas concept làm chuẩn.
- Mở trang HomeBankingPortal sẽ đổi locale chung của OutSystems sang en-US; quay lại ClaimMVP cần bấm VI.
