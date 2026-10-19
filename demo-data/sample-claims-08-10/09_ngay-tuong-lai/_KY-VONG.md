# L08_ngay-kham-o-tuong-lai

**Mọi chứng từ ghi ngày 20/10/2026 — sau ngày nộp**

- lossType chọn khi nộp: `ILL_OP`
- Ai phải bắt: AG-4 ValidateClaimDates → DATE_IMPOSSIBLE (FUTURE_DATE)

| # | File | Template kỳ vọng | Mã chứng từ |
|---|---|---|---|
| 1 | `01_phieu-kham-benh.png` | 6094 | T1-03 |
| 2 | `02_bang-ke-chi-phi.png` | 6091 | T1-01 |
| 3 | `03_don-thuoc.png` | 6090 | T1-02 |

## Lỗi được cài & kỳ vọng

- Ngày khám/bảng kê/đơn thuốc đều 20/10/2026, trong khi hồ sơ được nộp trước ngày đó.
- Kỳ vọng: DATE_IMPOSSIBLE (HIGH) → chặn. Nếu form MyInsurance chặn ngày tương lai thì nhập ngày hôm nay — ngày đọc từ chứng từ vẫn lệch và R4 phải bắt.
