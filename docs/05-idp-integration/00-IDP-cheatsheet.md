# BnK IDP — cheat sheet một trang

Staging: `https://idp-api.idpstg.bnksolution.com` · Số liệu đo thật ngày 03/09/2026.
Chi tiết đầy đủ: `IDP_APIs_for_Claim_MVP_v1.0_da-che-PII.md` (128 trang) · Spec dựng module: `ODC_REST_Consume_Spec_BnK_IDP_v0.1.md`.

## Quy tắc chung cho MỌI lời gọi

| Mục | Giá trị | Vì sao |
|---|---|---|
| `Authorization` | `Bearer {AccessToken}` | Token từ `POST /auth/login`, cache trong Entity `IdpSession` |
| `User-Agent` | Chuỗi giả trình duyệt (Chrome/126 trên Windows) | **Bắt buộc.** Thiếu → Cloudflare `403 error 1010` |
| `Accept` | `application/json, text/plain, */*` | |
| Tài khoản | **Vai Tenant (End User)** | Vai Partner/Admin bị `403` trên toàn bộ `/request/*` |
| Rate limit | Thiết kế ≤ 100 request/60 giây | Đọc từ code; giá trị thật trên staging chưa xác nhận |
| Lỗi nghiệp vụ | **Luôn đọc `errorCode` trong body** | Request hỏng vẫn trả `200` với `result: {}` |
| Correlation | Nhét `ClaimNo` vào trường `name` khi nộp | IDP không có correlation id / Idempotency-Key |
| **Không bao giờ gọi** | `PUT /request/{id}/result` | Làm kết quả trích xuất không còn bất biến |

## Chuỗi ngắn nhất chạy được

```
POST /auth/login                              → AccessToken
POST /request/sync                            → requestId + kết quả trích xuất (~16 s/trang)
PUT  /workflow/request                        → workflowRequestId
GET  /workflow/run-workflow-request/{id}      → kích hoạt (lần đầu có thể 400 → retry x3)
GET  /workflow/request/{id}                   → Timer poll tới status 3 (Finished) / 4 (Error)
GET  /workflow/request/{id}/result/json       → 20 mục cảnh báo/field (~30 giây)
```

## Bảy điều dễ làm sai nhất

1. Coi `recommendations = []` là hồ sơ sạch — phải kiểm cả `state` và `linesAssessed`.
2. Coi `HTTP 200` là thành công — đọc `errorCode` trong body.
3. Vá số tiền thay vì chạy lại chuỗi tính.
4. Cho agent một tool ghi Entity tiền "cho tiện".
5. Đọc lại hợp đồng lúc tính — mọi giá trị phải lấy từ bản chụp.
6. Cho bác bỏ khuyến nghị mà không ghi lý do.
7. Đưa chứng từ thật lên tenant demo — **che PII trước**.

## Bẫy theo từng endpoint

| Endpoint | Bẫy |
|---|---|
| `POST /request/sync` | `templates` là **chuỗi ngăn cách phẩy**, không phải array · `result` **lồng hai lớp** ở `/sync` nhưng một lớp ở `GET /request/{id}/result` → viết một hàm `IdpParseExtraction` dùng chung |
| `GET /request/{id}/raw-file` | Trả **application/zip**, không phải PDF → unzip trước khi hiển thị |
| `PUT /workflow/request` | **Ghim** `workflowId 5ed8e4fc-27a5-45a7-b104-1216fcfd0797` ("Workflow Claim 28/8") — flow duy nhất trả JSON đủ 4/4 lần; flow "Claim 25/08" trả `{}`. `autoStart` **vô tác dụng** |
| `GET /workflow/.../result/json` | `400` kèm `statusCode 4007` → chờ và gọi lại · `data` rỗng sau khi Finished = **hỏng**, không phải "sạch" · `"None"`, `"null"`, `"00000"` là giá trị rỗng → chuẩn hoá thành null |
| Poll | IDP chỉ có socket.io, ODC không nghe được → **Timer poll** backoff 3→5→10→30 giây, trần 15 phút |

## Chuẩn hoá trong adapter

- `fields[].k` là **nhãn tiếng Việt của template**, không phải khoá máy → giữ bảng ánh xạ `templateId + k → FieldCode` có version, cảnh báo khi gặp `k` lạ.
- `v = ""` → `null`. **Field rỗng vẫn có `c = 1`** → confidence của hồ sơ = **min `c` trên các field bắt buộc có giá trị**, kèm chỉ số completeness riêng.
- Hai thang confidence: `fields[].c` là 0–1, `data.confidence` là 0–100 → quy về 0–1.
- Ngày có cả `dd/MM/yyyy` và `yyyy-MM-dd`; tiền là text có dấu chấm/phẩy → parse cả hai.

## Số đo thật

Trích xuất **~16 giây/trang** · đánh giá flow **~30 giây** · bảng Hospital **13.328 dòng** có `hospital_code` và `country`.
Chưa có số về **độ chính xác** — đo bằng `POST /request/compare` với XLSX ground-truth trước khi lên slide ROI.

## Khoảng trống phải xin IDP (không vá được phía mình)

`documentId` + `bbox` trong `/result` (để neo field vào vùng ảnh) · flow nhận `lines[]` để đánh giá theo dòng · `proposedReasonCode` + `evidence[]` + band · `icd10`/`documentDate` mức dòng · khoá field ổn định thay nhãn · sửa `from-url` đang hỏng.

**Chính những khoảng trống này là lý do Clinical agent tồn tại.**
