# L09_vao-vien-sau-ra-vien

**Bảng kê nội trú: vào viện 26/09, ra viện 22/09**

- lossType chọn khi nộp: `ILL_IP`
- Ai phải bắt: AG-4 ValidateClaimDates → DATE_IMPOSSIBLE (ADMIT_AFTER_DISCHARGE)

| # | File | Template kỳ vọng | Mã chứng từ |
|---|---|---|---|
| 1 | `01_bang-ke-chi-phi-noi-tru.png` | 6091 | T1-01 |
| 2 | `02_bao-cao-y-te.png` | 6096 | T2-03 |

## Lỗi được cài & kỳ vọng

- Bảng kê ghi vào viện 26/09/2026, ra viện 22/09/2026 (đảo ngược); báo cáo y tế ghi ngày khám 22/09/2026.
- ILL_IP chỉ bắt buộc T1-01 nên checklist xanh; tổng 2.926.000 đ.
- Kỳ vọng: DATE_IMPOSSIBLE (HIGH) "vào viện sau ra viện" → chặn. Số ngày nằm viện không được tự suy ra.
