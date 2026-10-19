# case02_ILL_OP_thieu-phieu-kham

**Bệnh ngoại trú — THIẾU phiếu khám (REQUIRED), kèm biên lai Tier 3**

- lossType chọn khi nộp: `ILL_OP`
- Người được bảo hiểm: NGUYỄN VĂN AN

## Ảnh theo thứ tự nộp

| # | File | Template kỳ vọng | Mã chứng từ |
|---|---|---|---|
| 1 | `01_bang-ke-chi-phi.png` | 6091 | T1-01 |
| 2 | `02_don-thuoc.png` | 6090 | T1-02 |
| 3 | `03_bien-lai-thu-tien.png` | — | Tier 3 |

## Nộp bổ sung (thư mục `bo-sung/`)

| # | File | Template kỳ vọng | Mã chứng từ |
|---|---|---|---|
| 1 | `bo-sung/04_phieu-kham-benh.png` | 6094 | T1-03 |

## Kỳ vọng / cần quan sát

- Nộp T1-01 + T1-02 + 1 biên lai — KHÔNG có T1-03 phiếu khám (REQUIRED với ILL_OP).
- Biên lai không có template trên tenant 177 → rơi vào "giấy tờ khác"/không phân loại và KHÔNG được thoả dòng REQUIRED nào (luật V-405).
- Kỳ vọng: C1 / G2 chứng từ = FAIL "Thiếu hoặc chưa đọc: Phiếu khám bệnh"; hồ sơ chờ bổ sung, khách nhận yêu cầu bổ sung.
- Test bổ sung: nộp tiếp ảnh trong thư mục bo-sung/ → checklist phải chuyển xanh, chuỗi chạy lại.

> "Dự kiến" nghĩa là theo rule trong build-03; kết quả thật còn phụ thuộc hợp đồng, hạn mức và ngày hiệu lực của khách demo.
