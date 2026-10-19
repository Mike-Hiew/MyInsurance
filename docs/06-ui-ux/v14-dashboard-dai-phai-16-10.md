# v14 — Dashboard: dải phải có nền riêng, bỏ thông tin trùng, hiệu ứng (16/10/2026 ~14:10)

Tiếp nối claude/v12-header-chip-dashboard-16-10.md (v13 = Dashboard theo mockup, ClaimMVP 775).

**Yêu cầu Mike (14:05, kèm ảnh dải phải "For you / Your goals / Service level"):** dải phải có background chia phần như ảnh; bỏ thông tin lặp; bố cục lại cho "wow".

**Bản: ClaimMVP 776** (CSS nối cuối theme, khối `v14`, chỉ ghi đè — v13 vẫn còn bên dưới). Validation 3 lỗi nền. Kiểm sau publish: 40 rule `#v14` + 2 keyframes `v14shine`, `v14live` có hiệu lực; Dashboard tối VI khớp bản xem trước (dashboard-776.jpg). Nền sáng đã xem trước.
Lùi: 775 (v13 không dải nền), 774 (bố cục 5 thẻ), 770 (trước đợt).

## Thay đổi
- Lưới `1fr | 26 | 2fr | 40 | 380px`, hàng `auto×5 + 1fr` (hàng 1fr hấp thụ chiều cao dải phải để các hàng trái không giãn).
- Dải phải `.v10-db-right`: cột 5, hàng 1/7, kéo lên sát thanh menu (margin-top −32) và xuống đáy trang (margin-bottom −118), padding 30/34/64, min-height 100vh−76. Nền tối gradient 270° rgba(40,48,110,.85)→rgba(22,30,86,.55); sáng #E6E9F8→#F2F4FC. Bật lại `::after` của v10 (đã có content) để nền kéo tới mép phải màn hình. Viền trái là vạch gradient cyan→tím→hồng (border-image).
- Hiện lại 3 tiêu đề trong dải: Dành cho bạn · Mục tiêu · Hạn xử lý. Thẻ minh hoạ trả về kiểu v10 (#475484→#2D3657, khối lập phương bên trái). Nút "Xem" trên thẻ màu: viền trắng 1.5 px, cao 36.
- Bỏ trùng: ẩn 3 ô tuổi hồ sơ (trùng dòng "Trong ngày 1 · 1–2 ngày 0 · trên 2 ngày 3" của thẻ Tuổi hồ sơ); ẩn "Hàng đợi của tôi" + bảng (trùng "Cần bạn"; xem đủ ở Danh sách việc). Dòng "Đọc chứng từ tháng này…" giữ lại thành viên nhỏ dưới biểu đồ.
- Hiệu ứng: vệt sáng quét qua 3 thẻ màu (lớp gradient trên background, animate background-position, không dùng pseudo), số tổng 40/800 chữ gradient, chấm xanh nhấp nháy cạnh nhãn "Tổng giá trị đang xử lý" (radial-gradient + keyframes), khung biểu đồ có quầng tím. Tắt khi prefers-reduced-motion.
- ≤1100 px: 1 cột, dải phải thành khối bo góc, tắt nền tràn.

## Lưu ý
- Hiện lại bảng "Hàng đợi của tôi": bỏ rule `.content-middle:has(> .v10-db) > :is(.cc-section-head,[id$=QueueTable]):not(#v13):not(#v14){display:none}`.
- Không dùng `content:` trong CSS gửi Mentor (bẫy `content:""` → `content:"`).
