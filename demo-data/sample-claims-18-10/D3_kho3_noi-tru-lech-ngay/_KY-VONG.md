# D3 · Ca khó 3 — Nội trú lệch ngày
- Bệnh – nội trú · Vinmec, khoa Nội hô hấp · J18.9 Viêm phổi
- Bảng kê nội trú: vào 28/09/2026, ra **30/09/2026** nhưng tính **4 ngày giường**; Đơn thuốc ra viện và Giấy ra viện ghi ngày **02/10/2026**
- Nộp: Bảng kê nội trú (ô Bảng kê), Đơn thuốc ra viện (ô Đơn thuốc), Giấy ra viện (ô Giấy ra viện/báo cáo y tế). Tổng 3.682.000 ₫
- Ngày sự kiện nhập 28/09/2026
- **Kỳ vọng: R4 (ngày dịch vụ nằm trong đợt điều trị) WARN vì đơn thuốc 02/10 ngoài khoảng 28/09–30/09 → AG-4 nêu DATE_IMPOSSIBLE (trung bình) → không tự duyệt.**

## Đã chạy (11/10/2026)
- CLM-2026-0481: 3 chứng từ, 6 dòng, tổng 3.682.000 ✓ (chỉ T1-01 tạo dòng – F1).
- R4 → AG-4 **DATE_IMPOSSIBLE · HIGH** ✓ ("discharge date in the claim vs supporting documents") → gate 1/4, không tự duyệt ✓.
