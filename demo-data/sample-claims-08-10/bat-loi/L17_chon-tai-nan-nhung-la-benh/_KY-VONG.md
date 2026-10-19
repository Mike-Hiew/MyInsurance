# L17_chon-tai-nan-nhung-la-benh

**Khách chọn lossType Tai nạn, chứng từ là bệnh thoái hoá mạn tính (M47.8), không có tường trình/tổn thương**

- lossType chọn khi nộp: `ACC_OP`
- Ai phải bắt: AG-3 + ràng buộc ACC_* ↔ conditionClass ACC

| # | File | Template kỳ vọng | Mã chứng từ |
|---|---|---|---|
| 1 | `01_phieu-kham-benh.png` | 6094 | T1-03 |
| 2 | `02_bang-ke-chi-phi.png` | 6091 | T1-01 |
| 3 | `03_don-thuoc.png` | 6090 | T1-02 |

## Lỗi được cài & kỳ vọng

- NỘP với lossType = ACC_OP (Tai nạn ngoại trú). Chứng từ không có mã chấn thương (S/T), không có bản tường trình, chẩn đoán là bệnh thoái hoá.
- Kỳ vọng: không gán được conditionClass = ACC một cách hợp lệ → thẩm định viên phải đổi loại tổn thất hoặc giữ hồ sơ; không được hưởng quyền lợi tai nạn (không có thời gian chờ).
- Glucosamin dự kiến bị AG-3 hỏi (TPCN/không điều trị triệu chứng cấp).
