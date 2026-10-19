# L14_kham-suc-khoe-khong-ICD

**Khám sức khoẻ định kỳ, không có bệnh, không có mã ICD**

- lossType chọn khi nộp: `ILL_OP`
- Ai phải bắt: AG-3 / chuỗi (BR-CHAIN-1) — thiếu ICD + NOTCOV

| # | File | Template kỳ vọng | Mã chứng từ |
|---|---|---|---|
| 1 | `01_phieu-kham-benh.png` | 6094 | T1-03 |
| 2 | `02_bang-ke-chi-phi.png` | 6091 | T1-01 |
| 3 | `03_don-thuoc.png` | 6090 | T1-02 |

## Lỗi được cài & kỳ vọng

- Không có mã ICD ở bất kỳ chứng từ nào; chẩn đoán "chưa phát hiện bất thường"; có Vitamin tổng hợp.
- Kỳ vọng: không gán được conditionClass → dòng bị GIỮ + checkpoint nêu tên dữ kiện thiếu (BR-CHAIN-1), KHÔNG được coi là đạt; khám sức khoẻ định kỳ dự kiến NOTCOV; vitamin dự kiến NOTCOV/EXCL.
