# L10_ngoai-tru-qua-60-ngay

**Một bảng kê ngoại trú gộp 4 lần khám từ 05/07 đến 25/09 (82 ngày)**

- lossType chọn khi nộp: `ILL_OP`
- Ai phải bắt: AG-4 ValidateClaimDates → OP_WINDOW_OVER_60

| # | File | Template kỳ vọng | Mã chứng từ |
|---|---|---|---|
| 1 | `01_phieu-kham-benh.png` | 6094 | T1-03 |
| 2 | `02_bang-ke-chi-phi.png` | 6091 | T1-01 |
| 3 | `03_don-thuoc.png` | 6090 | T1-02 |

## Lỗi được cài & kỳ vọng

- Bảng kê gộp 4 lần khám trải 82 ngày; phiếu khám + đơn thuốc chỉ của lần 25/09.
- Kỳ vọng: OP_WINDOW_OVER_60 → DATE_IMPOSSIBLE/WINDOW (MEDIUM); R4 cũng có thể WARN vì 3 dòng khám nằm ngoài ngày phiếu khám.
