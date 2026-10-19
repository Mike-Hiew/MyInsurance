# case01_ILL_OP_ngoai-tru-day-du

**Bệnh ngoại trú — đủ chứng từ, thuốc hợp lý (happy path)**

- lossType chọn khi nộp: `ILL_OP`
- Người được bảo hiểm: NGUYỄN VĂN AN

## Ảnh theo thứ tự nộp

| # | File | Template kỳ vọng | Mã chứng từ |
|---|---|---|---|
| 1 | `01_phieu-kham-benh.png` | 6094 | T1-03 |
| 2 | `02_bang-ke-chi-phi.png` | 6091 | T1-01 |
| 3 | `03_don-thuoc.png` | 6090 | T1-02 |
| 4 | `04_ket-qua-x-quang-nguc.png` | 6095 | T2-02 |

## Kỳ vọng / cần quan sát

- Đủ 2 chứng từ REQUIRED (T1-01, T1-03) + T1-02 (EXPECTED) + T2-02 (OPTIONAL).
- Thiếu T1-04 hoá đơn XML (EXPECTED) → R6 bỏ qua kèm ghi chú, KHÔNG chặn hồ sơ.
- Tổng bảng kê 1.089.500 đ, không có BHYT. 3 thuốc đều hợp với J20.9 → AG-3 không đề xuất cắt dòng nào.
- Kỳ vọng: checklist xanh, chuỗi 14 bước chạy hết; nếu hợp đồng còn hạn mức và không có tín hiệu bất thường thì có thể qua cổng tự duyệt.

> "Dự kiến" nghĩa là theo rule trong build-03; kết quả thật còn phụ thuộc hợp đồng, hạn mức và ngày hiệu lực của khách demo.
