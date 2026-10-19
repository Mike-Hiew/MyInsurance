# L12_ben-khac-da-chi-tra

**Bảng kê ghi "Nguồn khác chi trả 500.000 đ" (công ty bảo hiểm khác bảo lãnh)**

- lossType chọn khi nộp: `ILL_OP`
- Ai phải bắt: Chuỗi 14 bước (trừ otherPayerShare) + AG-4 OTHER_PAYER_PRESENT

| # | File | Template kỳ vọng | Mã chứng từ |
|---|---|---|---|
| 1 | `01_phieu-kham-benh.png` | 6094 | T1-03 |
| 2 | `02_bang-ke-chi-phi.png` | 6091 | T1-01 |
| 3 | `03_don-thuoc.png` | 6090 | T1-02 |

## Lỗi được cài & kỳ vọng

- Tổng 678.000 đ, nguồn khác đã chi 500.000 đ, người bệnh trả 178.000 đ.
- Kỳ vọng: số được xét KHÔNG vượt phần khách thực trả; nếu AG-4 đã có tool thì nêu OTHER_PAYER_PRESENT. Nếu hệ chi trả trên toàn bộ tổng → lỗi.
- Lưu ý: dòng "Nguồn khác" nằm ở footer, không ở từng dòng — xem IDP có đọc được field "Nguồn khác" không.
