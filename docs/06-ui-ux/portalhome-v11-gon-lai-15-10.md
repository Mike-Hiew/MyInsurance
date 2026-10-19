# MyInsurance · PortalHome v11 "gọn lại" (15/10/2026 23:40, MI rev 220–221) + PortalClaimDetail theo mockup UI 1.0 (16/10, rev 223–224)

Mockup đích:
- PortalHome: canvas "My Insurance Portal – Dashboard" (Light + Dark) — bản gọn Mike duyệt 15/10.
- PortalClaimDetail: canvas "BnK Insurance — UI 1.0 concept" https://claude.ai/artifact/4R9xH5kY1G2VXckzj9fDbV, artboard "MyInsurance · Claim result" (PortalClaim.dc.html). Canvas này còn Console Sign in / Dashboard / Claim summary và MyInsurance Home.

## PortalHome — thay đổi widget (rev 220)
- V10Home thêm class `v11-home` (mọi CSS mới đều bắt đầu bằng `.v11-home`).
- LivePill: chỉ còn mã gói + "hiệu lực đến dd/MM/yyyy".
- NewClaimLink thêm NcSub "Chụp chứng từ, không cần gõ".
- V10G4 thêm G4Head: tiêu đề "Quyền lợi còn lại" + pill "Hiểu quyền lợi" → PortalBenefits.
- CardO/V/C thêm RingO/V/C (`.v11-ring`, style `--p:<% còn lại>`), số % + "còn lại".
- CardOLabel "Ngoại trú"; CardOMeta "trong …"; CardVMetaLeft "trong <hạn mức>".
- P2PhT chỉ mã hồ sơ; P2Sub = loại điều trị · cơ sở (như P1TxB2).
- P2SuppLink "Bổ sung chứng từ" → PortalSupplement, chỉ hiện khi Pending info.
- P2MiniBar/P2MiniBarFill: thanh tỷ lệ chi trả / tổng chi phí.
- P1PhT2 "Hồ sơ của tôi".

## PortalHome — CSS (theme: part A, part B, fixes)
- Lưới: header full → hero (Pane2) full → Quyền lợi (V10G4) | Hồ sơ (Pane1). V10G3 `display:contents`. ≤1024 px một cột.
- Ẩn bằng CSS (không xoá widget): StatusLink, SendDocsLink, CardY, thanh bar cũ, tag Full/In use, P1Cta (banner 2 phút), Pane3 (Thành viên + Giấy chứng nhận), P2MiniBear (Bạn tự trả).
- Hero navy có 2 quầng sáng; stepper bước hiện tại amber khi Pending info (`:has(.v10-sub.is-warn)`).
- Quyền lợi: nền chấm + 3 vầng pastel mờ, thẻ kính (backdrop-filter), vòng conic-gradient có glow. Biến `--v11-*`, tối theo `html.dark-mode`.
- Fix rev 221: triệt `ThemeGrid_MarginGutter` Mentor tự gắn trong `.v11-home`; padding thừa của P2MiniBar.

## PortalClaimDetail
- Rev 222 (đã thay ở rev 223): làm hero navy như trang chủ → Mike phản hồi "muốn như mockup", chỉ tới canvas UI 1.0.
- Rev 223: xoá block rev 222, thay bằng "v11 PortalClaimDetail = mockup UI 1.0 (16/10) part A" + "part B" (phạm vi `.v7-cols2d`, biến `--pc-*`):
  - Hero = thẻ `.rc` của mockup: gradient 140° #0D7C79 → #0B4F66 → #1A2A8A, bo 26, bóng teal, vệt sáng quét 7 s (`v11pcshine`, tắt khi reduced-motion). Thứ tự: dòng cơ sở · mã hồ sơ (13 px, 85%) → chip (đã trả: mint; có yêu cầu bổ sung: vàng) → số tiền/tiêu đề 56 px.
  - Donut 120 px (lỗ #0C6470) + legend chấm mint / trắng 35%, dòng tổng có kẻ trên. Widget DonutDiv: màu conic đổi sang #5CF2C0 và rãnh rgba(255,255,255,0.22). Hồ sơ chưa có kết quả: dòng "Tổng chi phí đã khai" có kẻ trên.
  - Stepper: chấm 28 px, xong #2EE6A6, đường nối mint; bước chờ khách bổ sung vàng #FFD36B, lỗi thật hồng #FF8A9B; nhãn căn giữa; "Một phần không chi trả" vàng.
  - Nút viên 46 px: nút thường trắng chữ #0B1250, nút trợ lý gradient tím #7C5CFF → #B03BC8.
  - Cột phải: panel kính (tối: trắng 5,5%, viền 8%, bo 20; sáng: thẻ trắng). "Vì sao Quý khách tự chịu": dòng `.v7-rc-row` kiểu `.why` (icon 40 px, số tiền 17 px trên, lý do dưới; xanh/vàng/hồng). "Chi tiết từng khoản": dòng kẻ mảnh, số đề nghị gạch ngang, số chi trả mint (sáng: #0E9F6E), bị cắt hồng. Thẻ yêu cầu bổ sung: dòng vàng nhạt; ẩn nút "Tải lên chứng từ" + ghi chú; ẩn "Bước tiếp theo" khi có thẻ yêu cầu.
  - Giữ ActBtn1Text "Bổ sung chứng từ" từ rev 222.
- Rev 224 — hiệu ứng "party" như mockup (block "v11 PortalClaimDetail party (16/10)", chỉ CSS):
  - Confetti có sẵn từ trước: CfWrap (`.v10-cf-wrap`, 14 mảnh CfPiece1–14, không JS), hiện khi hồ sơ Paid/Approved, chạy một lần mỗi lần mở trang. Lỗi do rev 223: rule `.v7-receipt>*{position:relative}` làm CfWrap (nằm trong ReceiptTop) bị bó trong phần đầu thẻ. Sửa: CfWrap absolute phủ cả thẻ (z 3, bo 26), ReceiptTop/Body chứa nó về `position:static` (`:has(>.v10-cf-wrap)`).
  - Mảnh 8×14, rơi 420 px, xoay 540°, 3,4 s (`v11cf`), giữ độ trễ lệch theo từng mảnh như cũ.
  - Donut "vẽ" vòng khi mở trang (mask conic + `@property --v11a`, `v11draw` 1,8 s); số tiền nổi lên (`v11pop`); các panel hiện dần lệch nhịp (`v11fade`). Tắt hết khi prefers-reduced-motion.
  - Đã kiểm trên CLM-0440: CfWrap phủ 647×547 (cả thẻ), 14 mảnh chạy `v11cf`, donut chạy `v11draw`; ảnh chụp chậm thấy confetti rải khắp thẻ.
- Kiểm: 0 lỗi mỗi rev.

## Kiểm chung
- Validation 0 lỗi (191 cảnh báo như cũ). Chưa xem được bản mobile.

## Lưu ý
- Giấy chứng nhận trước đây chỉ là dòng tĩnh (không tải được) nên không đưa sang pill ở trang chủ.
- Nội trú vẫn "100%" dù CLM-0440 nội trú đã chi trả 1.880.000 — lệch dữ liệu hạn mức, chưa sửa.
- Lệch logic giữa 2 màn với CLM-0497 (chưa sửa): trang chủ chip "Chờ bổ sung hồ sơ", bước 2 "Đọc chứng từ" đang chờ; trang chi tiết chip "Chờ thẩm định", bước 2 xong, bước 3 "Thẩm định" đang chờ.
- Mockup UI 1.0 Claim result có thêm thanh tỷ lệ từng khoản, số tiền đếm tăng dần và panel "Result letter" ở cột trái; hệ thống chưa có (thư nằm ở thẻ "Thư từ hồ sơ" cột phải).
- Confetti chỉ chạy với hồ sơ Paid/Approved (vd CLM-0440, 0456), không chạy với hồ sơ đang xử lý như 0497.
