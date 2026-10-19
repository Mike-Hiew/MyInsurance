# Chạy thử toàn luồng BnK IDP trên bộ template mới — đánh giá và đề nghị gửi đội IDP

**Ngày:** 17/09/2026 · tenant 177 "Phòng 2" · bộ template 6090–6098 · bộ chứng từ 198
**Chứng từ:** 9 ảnh giả lập tự vẽ, không có dữ liệu cá nhân thật. Cố tình trộn **một chứng từ
của bệnh nhân khác** (khác ngày điều trị, khác mã ICD) để thử khả năng đối chiếu chéo.
**Quota đã dùng:** 14 trang. **Chi tiết tương tác:** `So-tay-API-BnK-IDP.html`, tab "Chạy thử toàn luồng".

## Đã chạy tới đâu

| Chặng | Tình trạng |
|---|---|
| Xác thực, kiểm vai, quota, quyền partner | ✔ chạy thật |
| Tạo group → nộp 9 chứng từ → poll → đọc kết quả | ✔ chạy thật, request 21393–21402 |
| Phân loại 9 chứng từ (nộp kèm cả 9 template) | ✔ **9/9 đúng** (8/9 lần đầu) |
| Trích xuất field + bảng 13 cột, số tiền | ✔ đủ chữ số |
| Tạo phiên đánh giá → kích hoạt → poll → đọc 20 mục | ✔ chạy thật trên chứng từ y tế, phiên 1406, **118 giây** |
| Tra master data bệnh viện (OpenSearch 322) | ✔ chạy thật |
| Sửa template tại chỗ `PUT /template/{id}` | ✔ chạy thật |

→ **Đã chạy trọn chuỗi cần cho hackathon.** Trước hôm nay, nửa đánh giá lâm sàng mới chỉ
chạy trên một hoá đơn giả lập; nay đã chạy trên đúng bộ chứng từ y tế sẽ dùng để demo.

## Đánh giá

**Chạy được và tin được**

1. **Nửa trích xuất ổn định.** 9/9 phân loại đúng, confidence 70–90, không request nào lỗi
   ngoài dự đoán. Tiền về đủ chữ số (`150000`, `4500000`, `1500`, tổng `5820000`).
2. **Nửa đánh giá bắt được mâu thuẫn giữa các chứng từ.** Bốn điểm lệch cài sẵn đều bị bắt:
   ngày vào viện (02/09 lạc giữa 10/09), ngày ra viện, giấy tờ tuỳ thân, mã ICD (K29.1 lạc
   giữa K35.8). **Đây là phần kể chuyện tốt nhất cho pitch** — máy đọc chéo 9 chứng từ và chỉ
   ra chỗ không khớp, việc mà người làm bằng tay rất dễ bỏ sót.

**Hạn chế phải thiết kế quanh nó**

3. **Flow không đụng tới tiền.** Không một mục nào trong 20 mục nói về số tiền, dòng chi phí
   hay quyền lợi. Khẳng định nguyên tắc kiến trúc "tiền nằm trong Server Action tất định" —
   nhưng cũng nghĩa là I-16 không giúp gì cho phần khó nhất.
4. **Không có đánh giá theo dòng.** Không có `linesAssessed[]`, `proposedReasonCode`,
   `evidence[]`. Kết quả là 20 khối văn bản tiếng Việt cho cả hồ sơ. AG-3 phải lấp, và Track 2
   phải trình bày là **chế độ tư vấn**, không phải chốt kiểm soát.
5. **Master data là mắt xích yếu.** `HospitalCode` trả `00000`, `PlaceOfTreatment` trả `"null"`,
   `SurgeryCodes` trả `00000` — dù mọi chứng từ đều ghi rõ tên bệnh viện. Lần trước với hoá đơn
   không có bệnh viện, flow lại **bịa** ra `01001` (dòng đầu bảng 322). Hai hành vi khác nhau
   cho cùng một trường → không dùng được ba trường này từ flow, phải tự tra bảng 322/316.
6. **`warning_level` trộn hai loại ý nghĩa.** "Ngày vào viện không đồng nhất" mức 1 (cảnh báo
   thật) nhưng "Kiểm tra Danh mục bệnh viện" cũng mức 1 (chỉ liệt kê). Số lẻ 0.888 xác nhận đây
   là trung bình cộng. Không dùng làm cổng tự động.
7. **Thời gian ~3 phút cho một hồ sơ 9 chứng từ**: trích xuất song song ~65 giây, đánh giá
   118 giây. Demo phải có màn hình chờ.

**Phát hiện kỹ thuật mới (đã đưa vào sổ tay API)**

- Dòng bảng nằm ở khoá **`data`**, không phải `rows`. Code đọc `rows` sẽ thấy bảng rỗng,
  không lỗi, không cảnh báo — hồ sơ thành "không có dòng chi phí nào".
- Mỗi bảng có **hai** confidence: `confidence_du` và `confidence_ocr`.
- `PUT /template/{id}` sửa mô tả tại chỗ, id giữ nguyên. **Mô tả quyết định phân loại, không
  phải danh sách field** — viết theo kiểu "PHẢI có X" / "TUYỆT ĐỐI KHÔNG có X".

## Chưa chạy

PDF nhiều trang và `separatorType/separatorValue` · `extraInputData` (bơm dữ liệu khai báo vào
flow) · `/request/compare` để đo độ chính xác · trần quota một block · TTL token và rate limit
thật · `mbal/template-map` cho 6090–6098 (subDocumentType vẫn null).

## Chín đề nghị gửi đội BnK IDP

| # | Mức | Việc | Đề nghị cụ thể |
|---|---|---|---|
| 1 | **CHẶN** | Trần trang của một block | Con số trần + nâng quota tenant 177 tới 31/10. Demo 3 hồ sơ × 9 chứng từ = 27 trang |
| 2 | **CHẶN** | Ghim flow `5ed8e4fc` | Nội dung đã đổi giữa 03/09 và 17/09 dù cùng workflowId. Cam kết ghim suốt sprint, sửa flow thì bump workflowId |
| 3 | CAO | HospitalCode/PlaceOfTreatment/SurgeryCodes trả giữ chỗ | Khi không tra được thì trả null nhất quán, tuyệt đối không trả dòng đầu bảng master |
| 4 | CAO | I-16 cần đánh giá theo dòng | Bổ sung `linesAssessed[]`, `proposedReasonCode` (chỉ NOTREL/NOTCOV), `evidence[]` — hoặc xác nhận ngoài phạm vi |
| 5 | CAO | `warning_level` trộn ý nghĩa | Tách mức nghiêm trọng khỏi mục thông tin, hoặc xác nhận rõ ý nghĩa |
| 6 | VỪA | `mbal/template-map` cho 6090–6098 | Nạp map, hoặc xác nhận không cần |
| 7 | VỪA | TTL token, MAX_FILE_SIZE, rate limit | Ba con số thật trên staging |
| 8 | VỪA | `POST /request/from-url` hỏng | Sửa sau hackathon; xác nhận raw-file còn đọc được sau khi MinIO dọn |
| 9 | VỪA | PDF nhiều trang | Giới hạn trang, thời gian, cách dùng separatorType/separatorValue + một ví dụ chạy được |

Mục 1 và 2 là hai thứ duy nhất có thể làm demo đứng. Bảy mục còn lại đều đã có đường vòng phía ta.
