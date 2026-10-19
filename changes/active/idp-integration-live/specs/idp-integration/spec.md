# DELTA — `specs/idp-integration` · gọi IDP thật

> Nguồn: **Sổ tay API BnK IDP** (35 endpoint, chạy thật 17/09/2026 trên **tenant 177**) — bản HTML đặt cạnh file này.
> Đây là đoạn khác biệt so với `build-00` và `build-02`, không phải spec đầy đủ.

## 0. BA CON SỐ `build-00` §1 ĐANG GHI SAI

`build-00` §1 (viết 16/09) ghi **tenant 176 · user 203 · set 194**. Chạy thật 17/09 là:

| | `build-00` §1 (cũ) | THẬT (17/09, sổ tay) |
|---|---|---|
| Tenant | 176 | **177** |
| User | 203 | **204** (roleId 2, vai **Tenant** — vai Partner KHÔNG gọi được `/request/*`) |
| Bộ chứng từ | 194 `HEALTH_CLAIM_V1` | **198 `HEALTH_CLAIM_OS_V1`**, 12 template |
| Template | — | **6090–6098**, 9 template **tự dựng** bằng `POST /template` (tenant 177 không có template y tế nào) |

Ba con số này đi thẳng vào Site Setting. Sai một cái là hồ sơ trôi qua **im lặng** (xem bẫy 1).
⇒ `build-00` §1 và §4 #5 phải sửa tại nguồn. Phán xử: **sổ tay 17/09 thắng**, vì nó là lần gọi thật.

## 1. Chuỗi 11 bước — đã chạy thật, giữ nguyên thứ tự

| # | Method | Path | Việc | Đo được |
|---|---|---|---|---|
| 1 | POST | `/auth/login` | lấy JWT | 356 ms |
| 2 | POST | `/request/group` | mở nhóm cho hồ sơ | 111 ms |
| 3 | POST | `/request` | nộp từng chứng từ (lặp) | 682 ms |
| 4 | GET | `/request/group/{id}/check-status` | poll 3→5→10→30 s | 12–17 s |
| 5 | GET | `/request/{id}/result` | **copy vào `RawExtractionJson` ngay** | 98 ms |
| 6 | GET | `/request/{id}/raw-file` | giữ bản gốc trước khi MinIO xoá | 170 ms |
| 7 | PUT | `/workflow/request` | mở phiên đánh giá lâm sàng | 152 ms |
| 8 | GET | `/workflow/run-workflow-request/{id}` | kích hoạt, **retry 3×5 s** | 580 ms |
| 9 | GET | `/workflow/request/{id}` | poll tới status 3 | **72 s · 118 s (9 chứng từ)** |
| 10 | GET | `/workflow/request/{id}/result/json` | đọc 20 mục đánh giá | 85 ms |
| 11 | GET | `/metadata/opensearch/table/322/search` | tra bệnh viện thật | 180 ms |

**Tổng ~3 phút cho một hồ sơ 9 chứng từ** (trích xuất ~65 s song song + đánh giá 118 s).

## 2. Hệ quả kiến trúc — khoá lại, không bàn lại

- **IDP KHÔNG chạy trong lượt agent.** Trần server action là **10 s mặc định / 60 s tối đa** (SA 21/09). Bước 9 một mình đã 118 s. ⇒ IDP chạy **ngoài** lượt agent; agent đọc kết quả đã có.
- **Flow không đụng tới tiền.** Không một mục nào trong 20 mục nói về số tiền, dòng chi phí hay quyền lợi. Khẳng định nguyên tắc: *tiền nằm trong Server Action tất định, phán đoán nằm trong agent*. Cũng nghĩa là I-16 **không giúp gì** cho phần khó nhất.
- **Không có đánh giá theo dòng.** Không `linesAssessed[]`, không `proposedReasonCode`, không `evidence[]` — 20 khối văn bản tiếng Việt cho cả hồ sơ. ⇒ **AG-3 phải tự lấp, và Track 2 trình bày là chế độ tư vấn, KHÔNG phải chốt kiểm soát.** Nói rõ câu này trong deck trước khi giám khảo hỏi.
- **Demo phải có màn hình chờ.** 3 phút không được đứng im.

## 3. Mười bẫy — adapter phải chặn hết

| # | Bẫy | Mức | Cách chặn |
|---|---|---|---|
| 1 | `templates_data` **rỗng** nhưng HTTP 200, `errorCode` null, **vẫn trừ quota** | Nặng | Coi `templates_data = []` là **`EXTRACTION_ERROR` (lỗi cấu hình)**, không phải "không phân loại được". Đếm template yêu cầu vs thực khớp |
| 2 | HTTP 200 vẫn có thể là lỗi — partner hết hạn `statusCode 1014`; flow hỏng trả `{}` | Nặng | Luôn đọc `errorCode`/`statusCode` **trong body** trước. `{}` từ `/result/json` **không bao giờ** nghĩa là "sạch" |
| 3 | Tiền bị cắt phần nghìn — OCR đọc đúng `300.000`, extraction trả `300` | Nặng | Thêm *"chỉ lấy chữ số, bỏ dấu phân cách hàng nghìn"* vào mô tả **mọi field tiền**. Đã xác thực trên 6091. Nghi ngờ thì đối chiếu `get-ocr` |
| 4 | Flow **bịa** mã bệnh viện — trả `01001` = dòng đầu bảng 322, cho chứng từ không có bệnh viện | Nặng | **Không dùng** `HospitalCode`/`PlaceOfTreatment`/`SurgeryCodes` từ flow. Tra bảng 322 qua OpenSearch |
| 5 | Dòng bảng nằm ở khoá **`data`**, không phải `rows` — `templates_data[].data.tables[0].data`, mảng 2 chiều, dòng đầu là header | Nặng | Đọc `data`. Code đọc `rows` nhận mảng rỗng — **im lặng** |
| 6 | `templates` phải là **chuỗi CSV** `"6090,6091"`, không phải mảng (Swagger khai sai) | Vừa | Nhưng `/request/from-url` và `POST /template/document-set` lại nhận **mảng số** — ba endpoint, hai quy ước |
| 7 | `autoStart` không có tác dụng — nhận `true`, trả 200, lưu `false`, phiên nằm im ở status 2 | Vừa | Luôn gọi bước 8 ngay sau bước 7, **retry 3×5 s — đây là ngoại lệ 4xx-retry duy nhất** |
| 8 | Nội dung flow đổi mà `workflowId` không đổi (03/09 vs 17/09: thêm 2 mục, 30 s → 72 s) | Vừa | Trần poll ≥ 3 phút; **ghi lại phiên bản nội dung nhận được** |
| 9 | Tham số bắt buộc Swagger không khai — `document-set-list` thiếu `sort` → 400; `flows/assigns/all` thiếu `target_tenant_id` → 500 | Nhẹ | Truyền đủ ngay từ đầu, đừng tin Swagger |
| 10 | `POST /template/document-set` **không validate** — tạo được bộ chứa id mồ côi, rồi chính nó gây bẫy 1 | Nhẹ | Sau khi tạo bộ, `GET /template/{id}` từng id. Đã làm cho đủ 12 id của bộ 198 |

**Header `User-Agent` kiểu trình duyệt là BẮT BUỘC cho mọi call** — thiếu thì Cloudflare trả 403 mã 1010.
**Không có refresh token.** Gặp 401 thì đăng nhập lại, không retry mù. TTL giả định 30 phút, cache 1 dòng trong `IdpSession`.
**`POST /auth/logout` giết token ngay** — không gọi khi còn phiên đang chạy.

## 4. Hai endpoint CẤM, một endpoint HỎNG

- **CẤM** `PUT /request/{id}/result` — sửa được kết quả gốc ⇒ kết quả IDP **không bất biến**. Tự copy `RawExtractionJson` ngay lần đọc đầu (bước 5).
- **CẤM** `POST /tool/*` — thao tác vận hành nguy hiểm.
- **HỎNG** `POST /request/from-url`.

## 5. Chất lượng đã đo — 9 template tự dựng

9/9 phân loại đúng sau khi tinh chỉnh mô tả. Confidence 70–90.

| Template | Tên | Khớp | Conf. | Field |
|---|---|---|---|---|
| 6090 | Đơn thuốc | ✓ | 89.38 | 9 + bảng |
| 6091 | Bảng kê chi phí | ✓ | 90.50 | 21 + bảng 13 cột |
| 6092 | Giấy ra viện (có bảng kê) | ✓ | 86.69 | 16 + bảng |
| 6093 | Giấy ra viện (không bảng) | → 6092 | 79.64 | 15 |
| 6094 | Phiếu khám bệnh | ✓ | 86.56 | 23 |
| 6095 | Kết quả cận lâm sàng | ✓ | 83.52 | 16 |
| 6096 | Báo cáo y tế | ✓ | 83.11 | 18 |
| 6097 | GCN phẫu thuật | ✓ | 77.07 | 19 |
| 6098 | Bản tường trình | ✓ | 70.39 | 11 |

> `build-00` §4 #4 ghi *"`T1-03` phiếu khám chưa có template ở tenant 176"* — **đã tự giải quyết**: template 6094, khớp, confidence 86.56. Dòng đó trong `build-00` hết hiệu lực.

## 6. Câu chuyện tốt nhất cho pitch

Trộn cố ý một chứng từ **của bệnh nhân khác** vào hồ sơ 9 chứng từ. Flow bắt đúng **cả bốn chỗ lệch**: ngày vào viện, ngày ra viện, giấy tờ tuỳ thân, mã ICD (bảy cái `K35.8`, một cái `K29.1` — đúng cái lạc).

Máy đọc chéo 9 chứng từ và chỉ ra chỗ không khớp. Đây là phần kể chuyện mạnh nhất, và nó **đã chạy thật**.

Hai mục "đúng nhưng ồn" phải biết trước: *Kiểm tra Danh mục bệnh viện* và *Kiểm tra Mã phẫu thuật* đều ở mức 1 dù chỉ là thông tin. Số lẻ `0.888` cho thấy `warning_level` là **trung bình cộng**, không phải mức nghiêm trọng ⇒ **không dùng nó làm cổng**.
