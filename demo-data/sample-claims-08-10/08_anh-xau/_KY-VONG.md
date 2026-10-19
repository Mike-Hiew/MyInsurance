# L15_anh-chup-kem

**Bảng kê chụp nghiêng, mờ, tối một góc và mất phần chân trang (dòng tổng)**

- lossType chọn khi nộp: `ILL_OP`
- Ai phải bắt: Kiểm ảnh lúc chụp (DOC-VAL-001/002) + IDP confidence thấp → MISSING_FIELD

| # | File | Template kỳ vọng | Mã chứng từ |
|---|---|---|---|
| 1 | `01_phieu-kham-benh.png` | 6094 | T1-03 |
| 2 | `02_bang-ke-chi-phi_ANH-XAU.png` | 6091 | T1-01 |
| 3 | `03_don-thuoc.png` | 6090 | T1-02 |

## Lỗi được cài & kỳ vọng

- Ảnh bảng kê: xoay ~6°, nhoè, bóng tối góc phải, bị cắt ngay dưới bảng — mất toàn bộ dòng tổng, BHYT, người bệnh trả và chữ ký.
- Kỳ vọng: app cảnh báo ảnh (không chặn, vẫn "Dùng ảnh này"); IDP đọc thiếu field tổng → MISSING_FIELD → yêu cầu bổ sung đúng tên field + chứng từ; R5 SKIPPED.
- Hồ sơ KHÔNG được tự duyệt khi field tổng vắng (BR-CHAIN-1).
