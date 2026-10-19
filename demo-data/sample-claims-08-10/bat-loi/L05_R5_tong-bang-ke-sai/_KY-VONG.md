# L05_R5_tong-bang-ke-sai

**Dòng tổng in trên bảng kê lớn hơn cộng các dòng**

- lossType chọn khi nộp: `ILL_OP`
- Ai phải bắt: Đối chiếu chéo R5 → AG-4 CROSSCHECK_FAIL

| # | File | Template kỳ vọng | Mã chứng từ |
|---|---|---|---|
| 1 | `01_phieu-kham-benh.png` | 6094 | T1-03 |
| 2 | `02_bang-ke-chi-phi.png` | 6091 | T1-01 |
| 3 | `03_don-thuoc.png` | 6090 | T1-02 |

## Lỗi được cài & kỳ vọng

- Cộng các dòng 732.000 đ, dòng tổng in 984.000 đ (lệch 252.000 đ — đúng bằng 1 dòng thuốc bị đếm 2 lần).
- Kỳ vọng: R5 WARN trên footer → CROSSCHECK_FAIL (MEDIUM). Chuỗi tính trên DÒNG, không lấy tổng in.
