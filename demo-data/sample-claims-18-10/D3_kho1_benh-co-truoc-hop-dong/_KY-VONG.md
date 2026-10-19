# D3 · Ca khó 1 — Bệnh có trước khi mua hợp đồng
- Bệnh – ngoại trú · Vinmec, khoa Nội tiết · Ngày 07/10/2026 · E11.9 Đái tháo đường type 2
- Phiếu khám ghi tiền sử "phát hiện năm 2019, điều trị Metformin liên tục" — trước ngày hiệu lực hợp đồng (01/04/2026)
- Tổng 630.000 ₫ (khám 250.000, HbA1c 180.000, glucose 50.000, Metformin 60 viên 150.000)
- **Kỳ vọng: không tự duyệt; bị bắt ở nhóm bệnh/thời gian chờ (bệnh có sẵn/bệnh đặc biệt → cán bộ) hoặc AG-3 nêu bệnh có trước.**

## Đã chạy (11/10/2026)
- CLM-2026-0480: chẩn đoán E11.9 điền đúng; **không tự duyệt** (gate 3/4 – C3 giữ ở Step 7, điều kiện quyền lợi) ✓; đề xuất 550.000 ₫ (trước rev 559/560, cần chạy lại để ra số mới).
- AG-3: "No proposal" — chưa nêu bệnh có trước hợp đồng ✗ (đang sửa prompt AG-3).
- Lần 2–4 (CLM-0485/0486/0487, rev 562–563, AG-3 rev 10): sau khi sửa map xét nghiệm "Định lượng", hồ sơ bị **tự duyệt 504.000 ₫** ✗. Nguyên nhân: IDP chỉ trả mã ICD, không trả chữ (tiền sử/chẩn đoán) → AG-3 không có dữ liệu để nhận ra bệnh có trước.
- Sửa (rev 567): AssignConditionClass gán nhóm **PRE** cho ICD mạn tính (E10–E14, I10–I13, I15, J44, J45, N18, C) → dùng quy tắc chờ có sẵn BR-WAIT 365 ngày (test T-CH02-c).
- **CLM-2026-0489 (rev 567): đề xuất Decline ở bước Kiểm tra kỳ chờ — "quyết định từ chối luôn cần người duyệt", không tự duyệt ✓.** ChainTests 46/46, gate 5/5.
- Phiếu khám đã sinh lại: chẩn đoán ghi thêm "(phát hiện năm 2019, đang điều trị)" (IDP hiện không đọc chữ này, chỉ để người xem).
