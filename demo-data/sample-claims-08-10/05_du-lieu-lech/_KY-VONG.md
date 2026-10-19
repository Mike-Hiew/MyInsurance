# case10_ILL_OP_du-lieu-lech

**Bệnh ngoại trú — tên, ngày sinh, ngày khám, chẩn đoán, cơ sở và tổng tiền LỆCH nhau**

- lossType chọn khi nộp: `ILL_OP`
- Người được bảo hiểm: NGUYỄN VĂN AN

## Ảnh theo thứ tự nộp

| # | File | Template kỳ vọng | Mã chứng từ |
|---|---|---|---|
| 1 | `01_phieu-kham-benh.png` | 6094 | T1-03 |
| 2 | `02_bang-ke-chi-phi.png` | 6091 | T1-01 |
| 3 | `03_don-thuoc.png` | 6090 | T1-02 |

## Kỳ vọng / cần quan sát

- Đủ REQUIRED về mặt loại chứng từ, nhưng nội dung chéo nhau KHÔNG khớp — đây là case kiểm luật "không có gì được sửa âm thầm":
  - Tên: phiếu khám/đơn thuốc "NGUYỄN VĂN AN" ≠ bảng kê "NGUYỄN VĂN ANH"; ngày sinh bảng kê 14/05/1991 ≠ 14/05/1990.
  - Ngày khám: phiếu khám 03/10/2026 ≠ bảng kê 30/09/2026.
  - Cơ sở: đơn thuốc của "Bệnh viện Thử nghiệm số 2" (Hải Phòng) ≠ Bệnh viện Đa khoa Demo OS.
  - Chẩn đoán đơn thuốc J01.9 viêm xoang ≠ H10.3 viêm kết mạc, trong khi thuốc lại là thuốc mắt.
  - Tổng in trên bảng kê 2.150.000 đ ≠ cộng các dòng 1.650.000 đ (lệch 500.000 đ).
- Dòng "Kính cận gọng + tròng" 1.050.000 đ → dự kiến EXCL.
- Kỳ vọng: màn hình hiện CẢ HAI giá trị và gắn cờ tại field, chuỗi tính trên dòng (không lấy tổng in); cổng tự duyệt FAIL (G2/G4) → chuyển người, có thể yêu cầu bổ sung.

> "Dự kiến" nghĩa là theo rule trong build-03; kết quả thật còn phụ thuộc hợp đồng, hạn mức và ngày hiệu lực của khách demo.
