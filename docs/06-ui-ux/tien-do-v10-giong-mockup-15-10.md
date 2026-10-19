# Tiến độ "giống mockup 100%" v1.0 — 15/10/2026

Tiếp nối: claude/tien-do-big-update-v10-14-10.md (đến rev ClaimMVP 723 / MyInsurance 204).
Mockup đích: `E:\Hackaton\Prototype_Insurance_BnK_v1.0.html` (cột 1264 px, menu "Reference ▾ / Technical ▾", nền navy có vệt màu + sóng, font Sora).

## Revision
| App | Trước đợt | Hiện tại | Cổng |
|---|---|---|---|
| ClaimMVP | 723 | **759** | **3 lỗi nền** (từ rev 756). ChainTests lần cuối rev 759: 46/46, gate 5/5. |
| MyInsurance | 204 | **216** | 0 lỗi |

## Kết luận khi so với mockup
- Khoảng 90% khác biệt là do chưa làm tới mức chi tiết, không phải giới hạn OutSystems.
- Giới hạn thật:
  - Hệ thống chạy dữ liệu thật, mockup dùng dữ liệu mẫu.
  - Một số nội dung do logic sinh ra.
  - Các mục mockup ghi "NOT IN THE SYSTEM YET".
  - Lịch chọn ngày là của trình duyệt.
  - Mentor không chuyển widget sang container khác.
- Ảnh mockup PNG chụp trong môi trường không tải được Google Fonts nên hiện font thay thế. Trên trình duyệt thật, mockup cũng dùng Sora như hệ thống.

## Đợt 1 – 2: khung chung (ClaimMVP rev 724, MyInsurance rev 205)
- **Nền:** body #040D3F, 3 vệt màu (#3B2BD9/#1C4FE0/#7A2BD9) và sóng SVG chuyển động.
- **Thanh trên cùng:** cao 76 px, mờ kính, một hàng.
  - Logo viền (khiên / tim), menu 14.5 px, mục đang chọn có gạch trắng 2 px.
  - Nút tìm, EN|VI, sáng/tối, chuông là các nút tròn 40 px.
  - "Welcome, <tên>!" và avatar có gradient.
  - Rê chuột vào avatar mở bảng tên, vai trò, nút Sign out.
- **ClaimMVP:**
  - Menu "Home · Work list · Approval queue · Operations · Reference ▾ · Technical ▾".
  - Badge bằng 0 thì ẩn.
  - Phụ đề "Digital Health Insurance · v1.0".
- **MyInsurance:**
  - Menu "My cover · Benefits · New claim · Claims · Send documents".
  - **Mặc định tối** khi chưa có lựa chọn lưu. Lựa chọn đã lưu vẫn được tôn trọng.
- **Bẫy z-index:** `.header` là flex item có z-index nên tạo stacking context, che các nút hành động. Sửa bằng `.header{z-index:auto}`.

## Đợt 3: bộ icon mockup (rev 725 / 206)
- 51 icon Phosphor và 6 icon `v7-ic-*` được thay bằng nét kiểu mockup (Lucide, stroke 2).
- Cách làm: CSS `mask` với SVG data URI qua biến `--v10ic`; `::before` để `content:''` và `background:currentColor`.
- Bộ chọn `.ph.ph.ph-x::before` có specificity cao hơn rule `i.ph.ph-x::before` cũ của font ccicons.
- Đã kiểm: không còn glyph Phosphor/ccicons nào hiện trên các màn đã rà.

## Đợt 4: từng màn (rev 725 / 206)
- **Nhịp tiêu đề trang:**
  - Tiêu đề 26 px, cách thanh trên 46 px.
  - Nội dung bắt đầu cách phụ đề 24 px như mockup.
- **Work list:**
  - Thanh lọc dạng grid: ô tìm có icon, nhãn chữ hoa STATUS / DATE TYPE / DATE RANGE.
  - Hai ô ngày ghép thành một khung có mũi tên; nút Filter trắng bo tròn; checkbox thành công tắc.
  - Thứ tự: chip trạng thái lên trước thanh Table/By stage.
  - Tiêu đề bảng chữ hoa; avatar chữ viết tắt cho người được BH (`InsuredAvatarExpr`); nhãn trạng thái cùng cỡ 12.5 px.
  - Nút Take unassigned / Export dạng viên viền.
  - "From" đổi thành "Date range" / "Khoảng ngày".
- **Home (Dashboard):**
  - Bỏ tiêu đề trang như mockup; dải "For you" chạy từ ngay dưới thanh trên.
  - Lưới 276 | 24 | 584 | 380: "Needs your attention" thành danh sách thẻ dọc ở cột trái, biểu đồ và tuổi hồ sơ ở cột giữa.
  - Biểu đồ màu xanh lá / đỏ. Cần kiểm lại: bản xem trước chưa ăn màu vì class series của Highcharts có thể khác.
- **Claim detail:**
  - Bỏ tiêu đề trang; hero rộng hơn cột 28 px mỗi bên.
  - Thanh tab thành dải nền có công tắc Advanced mode nằm bên phải trong dải.
- **Portal Home:**
  - Bỏ tiêu đề trang; lời chào 32 px một dòng; 2 nút cùng hàng bên phải.
  - Ẩn nút tìm, vì mockup portal không có.
  - EN đang chọn là viên trắng.
- **Claims & progress:**
  - Ghi chú thành thẻ riêng; bảng là thẻ riêng, không còn thanh cuộn ngang.
  - Cost / Payout căn phải; bỏ vạch dọc giữa các cột.
- **Claim result:**
  - Bỏ tiêu đề trang; "← Claims" ở vị trí như mockup.
- **Benefit details:**
  - Bảng sát mép thẻ, hàng tiêu đề có nền.
  - Nhãn ngày xanh ngọc nhạt, nhãn waiting màu cam.
- **Sửa lỗi:**
  - "Welcome, !" thiếu tên: `WelcomeExpr` nay dùng `UserDisplayName`, lấy từ cuối.
  - Chữ "Sign out" trong bảng người dùng bị mất: Mentor đã biến `content:'Sign out'` thành `''`. Thay bằng Expression `LogoutTextExpr` T("Sign out","Đăng xuất").

## Sửa Home theo phản hồi (12:00–12:07, ClaimMVP rev 726–727)
- **3 thẻ xếp chồng:**
  - Lỗi: thẻ sau đè lên thẻ trước vì dùng opacity .72/.5 (trong suốt).
  - Sửa: làm theo mockup — vị trí 0/43/73%, rộng 45/33/27%, cao 188/172/152, thẻ sau tối đi bằng filter brightness, opacity 1.
  - Số 44 px (thẻ sau 36 px), "claims" có khoảng cách.
- **Biểu đồ:** cột xanh lá / đỏ. Rule cũ `html.dark-mode .highcharts-series-0.highcharts-column-series .highcharts-point` thắng → tăng specificity.
- **Tuổi hồ sơ:**
  - 3 dòng thành 3 thẻ đếm như hàng counter của mockup (icon tròn, nhãn, số màu).
  - Ẩn tiêu đề "Age of open claims" vì dải phải đã có.
- **Bảng "My queue":**
  - Trước bị bóp: mã hồ sơ gãy 3 dòng, vạch dọc giữa cột, tuổi là viên tròn.
  - Nay: bảng thẻ bo 16, không vạch dọc, tiêu đề chữ hoa, mã/tuổi/số tiền không xuống dòng, tuổi dạng chấm màu + chữ.
- **Lời chào:** "Welcome, Hieu!".
  - Biến UserDisplayName rỗng trong block Menu → `WelcomeExpr` đọc thẳng `GetCurrentUser.List.Current.User.Name`.
  - Mentor lần đầu vẫn giữ UserDisplayName; phải đưa biểu thức đầy đủ.
- **Giới hạn còn lại:**
  - Biểu đồ cao 200 px (cấu hình Highcharts), không đổi được bằng CSS.
  - Cột "received/settled" đang chồng lên nhau (stacked), mockup đặt cạnh nhau. Đổi cần sửa cấu hình chart.

## My queue trải hết cột như mockup (13:30, ClaimMVP rev 728)
- Dải phải "For you" chỉ chiếm 2 hàng đầu của lưới (grid-row 1/3), bỏ margin-bottom âm. Nhờ vậy "My queue" và bảng trải hết cột 1264 px bên dưới dải phải.
- Bảng nằm trong thẻ bo 16; tiêu đề cột chữ thường như mockup; tuổi dạng viên viền (xanh / cam / đỏ); "View all →" trắng đậm.

## "For you" có hình minh hoạ như mockup (13:38, ClaimMVP rev 729)
- Thay 3 ô icon vuông (Phosphor) bằng đúng 3 hình SVG của mockup:
  - Khối lập phương tím có dấu ✓, đặt bên trái thẻ "Agent runs".
  - Khiên đỏ có dấu !, trồi lên mép trên thẻ AG-4.
  - Vòng răng xanh ngọc, thẻ vận hành.
- Cách làm: hình nằm ở background-image (data URI) của `.v10-db-pi`, ẩn icon con bằng CSS. Bóng đổ drop-shadow, hình trôi nhẹ (`@keyframes v10fyfloat`, đặt tên riêng để không đè hiệu ứng ở Login).
- Thẻ: nền gradient #475484 → #2D3657, viền trắng 20%, cao tối thiểu 118 px, cách nhau 20 px; chữ tiêu đề 16.5/700.
- Khác mockup do dữ liệu/nhãn: "Agent runs" (mockup "3,199 agent runs · Last 30 days").

## Bỏ gạch chân khi rê chuột (13:45, ClaimMVP rev 730, MyInsurance rev 207)
- Nguyên nhân: rule gốc OSUI `a:hover, a:focus{text-decoration:underline}` làm mọi thẻ dạng link (vd "0 claims waiting for customer documents") bị gạch chân khi rê chuột.
- Sửa toàn app: `a:hover,a:focus,a:hover *,a:focus *{text-decoration:none !important}` (ClaimMVP thêm `.cc-pnl-view:hover`). Viền focus bàn phím giữ nguyên.

## Chế độ sáng: menu bị mất chữ (13:55, ClaimMVP rev 731, MyInsurance rev 208)
- Lỗi: ở chế độ sáng, chữ các mục menu (Home, Work list, Approval queue, Operations / My cover, Benefits…) là màu trắng trên nền trắng → chỉ thấy badge. Do rule chữ trắng của shell áp cho span bên trong link.
- Sửa: chế độ sáng chữ menu #5B6475, mục đang chọn / rê chuột #0B1033; số trên badge trắng. Chỉ áp cho span con trực tiếp của link để không đè chữ trong badge.
- Home sáng (ClaimMVP): bỏ quầng tím quanh tiêu đề "Needs your attention"; số 3 thẻ tuổi hồ sơ đậm hơn (xanh #0E9F6E, cam #C77700, đỏ #D92D3F); chữ phụ xám #667085.

## Bỏ số đếm trên menu + căn bảng thông báo (14:00, ClaimMVP rev 732, MyInsurance rev 209)
- Ẩn mọi badge số trên menu ngang (desktop ≥1025 px), gồm cả badge trong Reference ▾. Theo yêu cầu người dùng; menu mobile giữ nguyên.
- ClaimMVP: bảng chuông trước dính mép phải màn hình (right:20px) → nay mép phải trùng mép phải nút chuông (cùng công thức với nhóm nút: max(24px,(100%-1264)/2)+170px; ≤1240 px dùng +52px).
- Dòng tên / mô tả trong mỗi thông báo bị thụt 5.6 px so với mã hồ sơ (ThemeGrid_MarginGutter) → bỏ lề, thẳng hàng. Áp cả 2 app; thanh cuộn bảng chuông mảnh.

## Bảng chuông neo bên phải như mockup (14:08, ClaimMVP rev 733, MyInsurance rev 210)
- Theo mockup (.hb-bell .bp): mép phải bảng lệch sang phải chuông 60 px, cách chuông 12 px, rộng 390, bo 18, có mũi nhọn chỉ vào chuông.
- ClaimMVP (fixed): right = max(24px,(100%-1264)/2)+110px, top 70px; ≤1240 px dùng -8px. MyInsurance (absolute trong nhóm nút): right -60px, top 52px.
- Mũi nhọn vẽ bằng ::after của nút chuông khi bảng mở (:has(> .v10-bell-pop)), z-index 1101 để nằm trên bảng; có bản màu cho chế độ sáng.

## Menu hồ sơ giống bảng chuông + chuyển EN/VI vào menu (14:40, ClaimMVP rev 734–736, MyInsurance rev 211–213)
- Rev 734 / 211 (Mentor thêm widget): trong khung tên/vai trò thêm hàng "Language / Ngôn ngữ" (UmLangRow, nhãn là Expression T(...)) với 2 nút EN | VI. ClaimMVP: nút gọi block action mới SetLang2(LangCode); MyInsurance dùng cơ chế đổi ngôn ngữ sẵn có. Nút EN/VI trên header ẩn từ 1025 px trở lên (mobile giữ nguyên).
- Rev 735 / 212 (CSS): menu hồ sơ giống bảng chuông — rộng 300, bo 18, nền #0B1650, viền mờ, đổ bóng, mũi nhọn chỉ vào avatar, mép trên trùng mép trên bảng chuông (y 70). Có "cầu" vô hình 22 px để rê từ avatar xuống menu không bị đóng.
- Bố cục trong menu: tên 15/600 → vai trò 12.5 → đường kẻ → "Language" + công tắc tròn EN | VI (nút đang chọn nền trắng, chữ navy) → đường kẻ → "Sign out" (nút rộng hết menu, nền nhạt khi rê).
- Rev 736 / 213: chế độ sáng — chữ nút đang chọn thành trắng trên nền navy, icon đăng xuất xám (trước đó trắng nên không thấy).
- Đã thử trực tiếp cả 2 app: bấm VI → toàn trang sang tiếng Việt, bấm EN → trở lại; đã đưa cả 2 app về EN, chế độ tối như trước.
- Ảnh: cmp/profile_menu_rev736.jpg.
- Rev 737 / 214 (theo phản hồi "nên hướng sang bên phải"): menu hồ sơ neo giống bảng chuông — mép phải lệch sang phải avatar 60 px (thu dần về 0 khi màn hẹp: clamp(0, (100vw-1264)/2-24, 60px)); mũi nhọn và nút Sign out dịch theo. Ảnh: cmp/profile_menu_right_rev737.jpg.

## Chế độ sáng: menu thả xuống Reference / Technical mất chữ (15:15, ClaimMVP rev 738)
- Nguyên nhân: rule gốc `.layout .aside-navigation .app-menu-links a span` tô chữ trắng 82% !important; rule chế độ sáng ở rev 731 chỉ sửa span con trực tiếp của menu (`> a > span`), không chạm span trong `.v10-dd-panel`. Nút "Reference/Technical" khi mở cũng trắng nhạt vì thiếu bản sáng cho `.v10-dd:hover .v10-dd-btn`.
- Sửa (chỉ chế độ sáng, ≥1025 px): chữ mục #344054, rê/đang chọn #0B1033, icon #475467; nút thả xuống khi mở/đang có trang con active #0B1033. Badge số giữ chữ trắng. MyInsurance không có menu thả xuống nên không cần sửa.
- Ảnh: cmp/nav_dropdown_light_rev738.png.

## Chế độ sáng: Work list (15:30, ClaimMVP rev 739–740)
- Nút "Table | By stage": rule `.cc-toolbar .v10-vbtn.is-on` (rev 725) ép chữ navy !important, trong khi chế độ sáng nền nút đang chọn cũng navy → mất chữ. Nút chưa chọn bị màu link xanh. Sửa: chế độ sáng chữ nút đang chọn trắng, nút chưa chọn #5B6475 (rê #0B1033); chế độ tối nút chưa chọn #A9B0DA. Bỏ gạch chân.
- Công tắc "Show test claims" ở chế độ sáng gần như vô hình (nền trắng 20%) → nền #D0D5DD, bật = #040D3F.
- Dòng mô tả dưới tiêu đề trang (cc-page-sub) ở chế độ sáng đang là #A9B0DA (màu cho nền tối) → #667085.
- Ô "đến ngày" bị cắt chữ "09/03/202…" → giảm padding hai ô ngày.
- Ảnh: cmp/worklist_light_rev740.png.

## Claim detail: thanh tab khi bật Advanced mode (16:10, ClaimMVP rev 741)
- Lỗi: bật Advanced mode có 9 tab, tổng rộng 1377 px > cột 1264 → tab "History & log" chui dưới nút "Advanced mode" (chữ đè nhau). Chế độ sáng: tab đang chọn nền đen, chữ xám → không đọc được.
- Sửa: mỗi tab padding 0 10px (trước 16), khoảng chừa cho nút Advanced 176 px (trước 210) → vừa 1 hàng ở tiếng Anh. Nếu vẫn dài hơn (tiếng Việt) thanh tab cuộn ngang, thanh cuộn ẩn, nút Advanced có nền trùng màu thanh + bóng mờ bên trái (giống mockup .v8-adv sticky).
- Chế độ sáng: tab đang chọn nền #040D3F chữ trắng; rê tab khác chữ #0B1033. Nền nút Advanced: sáng #E9ECF6, tối #131B4A (lấy đúng màu thanh tab).
- Ảnh: cmp/claimdetail_tabs_rev741.png.

## Claim detail · tab Extraction & costs theo mockup (16:30, ClaimMVP rev 742–743)
- Lỗi: bảng "Extracted data" để table-layout fixed với 9 cột cố định rộng (16 px padding) → cột Item chỉ còn 60 px, chữ xuống dòng từng ký tự; "Sum of lines" cũng gãy. Cột trái (chứng từ gốc) 360 px.
- Theo mockup (`grid-template-columns:260px 1fr; gap:16px`, cột trái sticky): cột trái 260 px sticky; độ rộng cột bảng 64/32/116/auto/72/132/60/76/116/40, padding ô 10 px → cột Item ~280 px, một dòng. Dòng tổng (v7-tfoot-sum) dùng cùng lưới cột.
- Banner "… confirmed by …", "AG-3 technical detail", thẻ Clinical assessment và ghi chú "Confidence guides workflow…" dời vào cột phải (margin-left 276 px) như mockup (rec-card nằm trong cột phải). Thẻ AG-3 nối liền dưới summary (bo 0 0 12 12), tiêu đề 16/700 thay vì 22 px, ẩn chữ "PARTIAL" trùng với pill.
- Chế độ tối (rev 743): banner nền kem chữ trắng → chữ #1B2129; summary AG-3 nền trắng → nền mờ, chữ #A9B0DA.
- Ảnh: cmp/extraction_rev743.jpg.

## Extraction & costs chế độ tối + ca nhiều mã ICD (16:50, ClaimMVP rev 744 — kiểm trên CLM-0497, chỉ xem)
- Thẻ "AG-3 clinical proposal" (PendingCard): nền gradient sáng viết inline (#F1EDFD→trắng) nhưng chữ trắng ở chế độ tối → mất tiêu đề và nội dung. Sửa: chế độ tối nền tím mờ rgba(124,92,255,.18)→trong suốt, viền tím mờ; bỏ thụt lề nội dung (RecPropBody).
- Cột ICD 72 px không đủ cho "K04.0; K05.1" → đè lên số tiền. Độ rộng mới: 78/32/112/auto/104/124/60/80/136/32 (Item ~230 px), ICD cho xuống dòng nếu cần.
- Giờ xác minh "13/10 14:34" dưới ô tick tràn cột → cột Verified 78 px. Chip "Edited" ở cột Source bị gãy → 80 px, không xuống dòng. "Proposal: exclude" ở chế độ tối: nền cam mờ, chữ #FFB86B.
- Ảnh: cmp/extraction_dark_rev744.jpg.

## Thẻ đề xuất AG-3: gọn, tách cụm, chỉ thông tin cần (17:00, ClaimMVP rev 745)
- Phản hồi: rối mắt, cần màu nhấn phù hợp, tách cụm, chỉ hiện thứ người duyệt cần.
- Mỗi thẻ là một khối riêng (cách nhau 12 px), nền phẳng (sáng: trắng; tối: trắng 4%), viền trái 4 px màu hổ phách = "đang chờ bạn quyết".
- Hàng đầu: "AG-3 CLINICAL PROPOSAL — LINE n" + chip AI + "confidence n%" (chữ nhạt, không viền) … nhãn "Your decision" đẩy sang phải (nền hổ phách nhạt).
- Ẩn: chip mã NOTCOV/NOTREL (tiêu đề đã nói rõ), khối "Evidence cited" (KB_ENTRY/ITEM_CLASS — kỹ thuật, vẫn xem được trong "AG-3 technical detail").
- Tiêu đề 15.5/700, nội dung 13 px màu phụ; chip "You decide…" màu xanh nhạt, bỏ đường gạch đứt.
- Trong "AG-3 technical detail": chip HIGH gọn 22 px, tên/lý do/bằng chứng mỗi thứ một dòng.
- Lưu ý dữ liệu (không sửa): đề xuất dòng 2 có tiêu đề "not related to the diagnosis" nhưng lý do lại viết "…nên liên quan đến lần khám này" — nội dung AG-3 tự mâu thuẫn.
- Ảnh: cmp/ag3_cards_rev745.jpg.

## Thẻ đề xuất AG-3: nền tím AI (17:20, ClaimMVP rev 746)
- So 3 phương án nền (trắng hiện tại / tím AI nhạt / hổ phách nhạt — cmp/ag3_bg_options.jpg); chọn tím AI, làm đậm hơn theo phản hồi "ấn tượng hơn, nền tối chưa khác".
- Sáng: gradient 135° #EFE9FF → #F8F5FF → trắng, viền #D9CFFB, bóng tím nhẹ. Tối: gradient tím 30% → 14% → 6%, viền tím 40%, glow tím. Viền trái hổ phách giữ nguyên (= việc cần bạn quyết).
- Ảnh: cmp/ag3_violet_rev746.jpg.

## Thẻ đề xuất AG-3 = "AI decision card" (17:48, ClaimMVP rev 747)
- Viền gradient tím → hồng → hổ phách quanh thẻ (thay vạch trái hổ phách); ô biểu tượng AI 44 px gradient tím-hồng + sao lấp lánh trắng (mask SVG); quầng tím góc trái trên; bóng tím, rê chuột nhấc 2 px.
- "Your decision": chấm hổ phách nhấp nháy (v10aipulse).
- Confidence nổi bật: pill indigo (#EEF0FF / chữ #3538CD; tối: indigo 16%, chữ #C7CBFF) + biểu tượng đồng hồ đo.
- Chip "You decide — money changes only when you confirm": icon khiên-tick, viền xanh, hiệu ứng ánh sáng quét ngang (v10shine 3.2 s) + glow nhịp (v10glow). Tắt chuyển động khi người dùng bật prefers-reduced-motion.
- Ảnh: cmp/ag3_ai_card_rev747.jpg.

## Ẩn "AG-3 technical detail" khỏi tab Extraction & costs (17:58, ClaimMVP rev 748)
- Quyết định với PO: người duyệt không cần phiên bản KB, run id, band, KB_ENTRY/ITEM_CLASS; hai thẻ đề xuất đã đủ. Thông tin kỹ thuật vẫn có ở Technical ▸ Technical trace.
- CSS: `[id$='ExtractCols'] ~ AG3TechDetails / AG3AssessmentCard {display:none}` (chỉ ẩn, không xoá widget); ghi chú "Confidence guides workflow…" cách thẻ trên 16 px.
- Lỗi dữ liệu AG-3 phát hiện khi xem phần này (chưa sửa): dòng 2 "Chụp phim răng toàn cảnh" dẫn KB_ENTRY "vat-dung-ca-nhan — Vật dụng cá nhân, tiện nghi"; ITEM_CLASS cả hai dòng là CONSULT (bảng ghi IMAGING/CONSULT); tiêu đề NOTREL "not related" nhưng lý do nói "có liên quan".
- Ảnh: cmp/extraction_no_techdetail_rev748.jpg.

## Claim detail · Summary theo mockup (18:05, ClaimMVP rev 749)
- Lấy đúng CSS mockup (.a5-verdict, .a5-q .qn, .v9-fr): ô "What the AI proposes" = hero gradient --grad-ai (120° #6E3CF6 → #B03BC8 → #F0406F), bo 20, chữ trắng, "Approve in part" 26/800, số tiền 36/800, bóng hồng-tím, vệt sáng quét 7 s (v10vshine).
- Ô số thứ tự 1/2/3/4: --grad-violet (#6E3CF6 → #3A1FA6), bo 10. Thẻ: bo 16; tối = trắng 5.5%→3% trên #0E1752.
- "Why" (mf-step): bỏ nền kẻ sọc (mf-ghost), rãnh 14 px phẳng (tối trắng 7%, sáng #EEF0F4), màu thanh như mockup: Claimed #7D84B6/#C9CFD8, Not covered #D33A3A, Over cap #8C95A3, Co-pay #E3A008, Payable #1F9D57 (chữ xanh); dòng = 0 chữ xám.
- Ảnh: cmp/summary_mockup_rev749.jpg.

## Ghi chú nguyên tắc "Confidence guides workflow, not money…" nổi bật + động (18:15, ClaimMVP rev 750)
- Chỉ ở tab Extraction & costs (`[id$='ExtractCols'] ~ [id$='InfoAlertInst'] > .alert`).
- Viền gradient cyan → xanh → tím, bo 16; nền sáng #EFF6FF→#F5F3FF / tối xanh-tím mờ trên #0E1752; bóng xanh.
- Icon "i" trong vòng tròn 38 px gradient xanh-tím, vòng sóng lan ra 2.4 s (v10ring); vệt sáng quét ngang 6 s (v10vshine); chữ 13.5/600 (sáng #1E3A8A, tối #EAF2FF). Tắt chuyển động khi prefers-reduced-motion.
- Icon gốc là glyph Phosphor ở ::after của .alert-icon (absolute) → chuyển static để căn giữa, không đổi content.
- Ảnh: cmp/note_rev750.jpg.

## Extraction: cột chứng từ gốc rộng hơn + nút sửa không sát mép (19:12, ClaimMVP rev 751)
- Cột trái 260 → 290 px (chữ chứng từ 11 px); các khối dưới bảng dời theo (margin-left 306 px).
- Cột bảng: 78/32/108/auto/100/124/56/72/128/48 (Item ~212 px); cột cuối (nút ✎) 48 px, căn giữa, chừa 16 px bên phải → nút không còn dính mép thẻ. Dòng tổng cùng lưới.
- Ảnh: cmp/extraction_wider_rev751.jpg (CLM-0497, ca này không có nút ✎ nên chỉ thấy cột trống).

## Nút sửa dòng (✎) rõ ràng (19:22, ClaimMVP rev 752)
- Phản hồi "nút edit đâu, sao lại mất": kiểm trên CLM-0498/0499/0501/0504 sau rev 751 — nút `.v7-pen` vẫn có ở mọi dòng (inline-flex, 30 px), nhưng chỉ là ký tự ✎ 14 px màu link xanh, rất khó thấy (nhất là chế độ tối).
- Rev 752: nút thành ô vuông 30×30 bo 9, nền indigo nhạt #EEF0FF, viền #C7CBFF, ký tự 15 px #3538CD; rê chuột nền đặc #3538CD chữ trắng, nhấc 1 px. Tối: indigo 16%, chữ #C7CBFF, rê #6366F1. Không đổi OnClick/nội dung.
- Lưu ý vận hành: người dùng đang dùng chung tab Chrome 717100787 → từ giờ mình kiểm/test trong tab riêng (717101237) để không bấm nhầm lên trang người dùng đang xem.
- Ảnh: cmp/pen_rev752.jpg.

## Còn mở
- 7 lỗi nền ClaimMVP = "Multipart request format in REST APIs" (IdpAuth, IdpDocuments ×4, IdpWorkflow, IdpMasterData). Không chặn publish. Để sau: đối chiếu xem IdpSubmitSyncMethod/IdpSubmitMethod còn được gọi không (Mentor nói còn, ghi chép IDP nói đã chuyển sang External Logic), gỡ multipart thừa, chạy lại kiểm IDP.
- Đăng nhập lại để: kiểm trực tiếp rev 725/206 (sáng/tối, 1280 px, mobile 390 px), chạy ChainTests (46/46, gate 5/5), chụp ảnh so sánh sau publish.
- Màu biểu đồ Home.
- Khác biệt cấu trúc còn lại (cần thêm widget nếu muốn giống hẳn):
  - Home: "Needs you" của mockup là danh sách từng hồ sơ có số tiền; hệ thống là 4 nhóm đếm.
  - Home: mockup có 3 thẻ đếm dưới biểu đồ.
  - Ảnh minh hoạ 3D trong "For you".
  - Số tiền trên thẻ Portal: mockup không có "VND" và chữ lớn hơn.

## Bài học với Mentor
- Câu "keep content:'' with single quotes" khiến Mentor đổi **mọi** `content:'chữ'` thành `''`. Chữ hiển thị phải dùng Expression widget, không dùng CSS content.
- Mentor tự gắn `ThemeGrid_MarginGutter` cho Expression mới (thêm lề trái). Kiểm và triệt tiêu bằng CSS nếu cần.
- Xem trước bằng cách lưu CSS vào localStorage (`v10pv_*`) và chạy một loader nhỏ sau mỗi lần chuyển trang: nhẹ hơn dán lại cả khối CSS.
- Rule cũ `... > div:not(.avatar):not(.v10-welcome) span:first-child` (0,7,2) và bản `html:not(.dark-mode)` (0,8,3) tô màu mọi span đầu tiên trong menu hồ sơ → rule mới cho chữ trong menu phải có specificity cao hơn.
- Ghi đè rule của shell cần cùng hoặc cao hơn specificity: `.layout.layout-side.fixed-header .header .header-content` là (0,5,0).
- Lượt Mentor ≤ ~11 KB chạy ổn: ClaimMVP 4 lượt (icon 1, icon 2, Work list, phần còn lại + widget + publish), MyInsurance 3 lượt.

## 15/10 tối — Banner trạng thái Pending info (CM 753–755)
- Phát hiện: nút edit dòng chỉ hiện khi `StatusId = UnderReview` (đúng thiết kế, giữ khoá theo ý Mike). Ở Pending info: edit, tick Verified, quyết định AG-3 đều khoá.
- 753: thêm `PendingInfoBanner` dưới header ClaimDetail (If StatusId = PendingInfo): tiêu đề, "since … · requested by …" (event DOCS_REQUESTED), Missing (detail), Customer letter (thư ADDDOCS: draft/sent), dòng giải thích khoá. CSS `.v10-pi*` (amber, icon đồng hồ cát).
- 754: tên người yêu cầu thay GUID; CSS bước hiện tại màu amber khi có banner.
- 755: sửa bug StepLine — so sánh `ClaimStatus.Label = "PendingInfo"` (Label thật là "Pending info") → đổi sang so StatusId. 0497 giờ: bước 1 xong, bước 2 "Waiting for documents" (amber). 0445 vẫn bước 4.
- Kiểm: 7 lỗi nền; ChainTests 46/46, gate 5/5 (20:17).
- Còn mở: thư AG-5 yêu cầu bổ sung cho 0497 mới là **bản nháp, chưa gửi** khách. T1-04 (XML) chỉ là "Expected" (không chặn) nhưng hồ sơ đã chuyển Pending info.

## 15/10 21:00 — Xử lý các mục còn mở (CM 756–759, MI 215–216, CLM_ClinicalAgent 13)
| Mục | Kết quả |
|---|---|
| 7 lỗi nền multipart | CM 756: xoá 4 structure không dùng (IdpDocuments.RequestPart2, IdpWorkflow.RequestPart3, IdpMasterData.RequestPart4, IdpAuth.RequestPart). **Còn 3 lỗi** ở IdpSubmitSyncMethod ×2 và IdpSubmitMethod — 2 method này vẫn được IdpSubmitSync / IdpSubmitDocument gọi (đường IDP thật), không gỡ. **Baseline mới: 3 lỗi.** |
| Lỗi đề xuất AG-3 | CLM_ClinicalAgent rev 13: thêm mục "NHẤT QUÁN GIỮA MÃ, LÝ DO VÀ BẰNG CHỨNG" vào system prompt (NOTREL phải nói không liên quan; chẩn đoán hình ảnh đúng vùng = có liên quan; KB_ENTRY phải đúng loại; ITEM_CLASS = serviceCategory nguyên văn). Chỉ áp dụng lần chạy mới, không chạy lại 0497. Validate agent: 2 lỗi "Unavailable feature" (chưa đối chiếu với rev 12). |
| Miễn hoá đơn XML trên 0497 | **Chưa làm được.** Nút "Waive expected" chỉ là stub (thông báo "not in the pilot", không ghi gì). PendingInfo → UnderReview chỉ xảy ra trong TryMoveClaimAfterRead (sau khi đọc xong chứng từ mới). Dựng chức năng miễn thật (ghi event DOCS_WAIVED + đổi trạng thái) bị chặn vì ghi DB/đổi trạng thái — chờ Mike duyệt. Banner đã sửa câu khoá (CM 759) cho khớp thực tế. |
| Home ClaimMVP | CM 757–759: "Needs you" thành danh sách từng hồ sơ (GetNeedsYou, ưu tiên AG-4 → chờ duyệt → AG-3 → thư cần gửi → chờ chứng từ → cần xem), 4 bộ đếm cũ ẩn bằng CSS. Biểu đồ: cột đôi (không xếp chồng), màu mockup #23794F/#C2454B, tuần hiện tại sáng #2EE59D/#FF6B6B. Icon nền tối. |
| MyInsurance home | MI 215–216: số tiền 3 thẻ quyền lợi 36 px, bỏ ₫; ô nhỏ bỏ ₫; thêm ô "Bạn tự trả"; 3 ô một hàng; câu trạng thái khi Pending info: "Chúng tôi cần bạn bổ sung chứng từ — mở mục Bổ sung chứng từ." (trước đó nói sai "đang đọc chứng từ"). |
| Kiểm tra | CM 759: 3 lỗi nền; ChainTests 46/46, gate 5/5 (21:xx). Nền tối banner + Home xem trước OK. Chưa chạy IDP thật sau khi gỡ structure (tốn quota, ghi dữ liệu). |
| "For you" 3D, 3 thẻ dưới biểu đồ | Đã có từ trước (cube/khiên/bánh răng; 3 thẻ tuổi hồ sơ). Không đổi. |

Ảnh: cmp/home_needsyou_rev758.jpg, cmp/home_dark_rev758.jpg, cmp/banner_dark_rev755.jpg, cmp/myinsurance_home_rev216.jpg.

## 16/10 — ClaimDetail: "Mở →" ra trang trắng (CM rev 770)
- Lỗi: vừa tải lại trang, bấm "Mở →" ở thẻ "Việc cần người phán đoán" thì tiêu đề tab đã chuyển sang Trích xuất nhưng phần nội dung không trượt theo (osui-tabs__content giữ scrollLeft 0). Khi đó vẫn hiện ô của tab Tóm tắt, và trang lại đang cuộn quá cuối nội dung nên chỉ thấy nền trống. Thêm nữa, mọi tab đều cao bằng tab dài nhất (khoảng 7.984 px), nên tab ngắn để lại khoảng trống rất dài.
- Sửa: node JS AlignActiveTab ở cuối action chuyển tab, căn scrollLeft theo tab đang chọn rồi cuộn tới thanh tab. CSS cho tab không chọn max-height 0, tab đang chọn cao tự động (Tóm tắt 1.177, Chung 413, Chứng từ 926, Trích xuất 1.037, Lịch sử 7.984).
- Kiểm: tải lại hồ sơ 533 → bấm Mở → thấy tab Trích xuất, dòng 6 được đánh dấu. Chuyển qua đủ 9 tab, nội dung khớp tab đang chọn. Validate 3 lỗi nền. ChainTests rev 770: 46/46, gate 5/5.
