# ODC ↔ BnK IDP — Spec REST Consume cho module `CLM_IDP_Int` (v0.1)

**Ngày:** 03/09/2026 · **Nguồn:** `E:\Hackaton\insurance BNK\IDP_API_Spec` (149 file JSON gọi thật ngày 03/09/2026 lên staging `https://idp-api.idpstg.bnksolution.com`) · **Mục đích:** cho developer ODC dựng module tích hợp IDP trong hackathon — 6 REST Consume, 1 Timer, 2 Server Action điều phối, không cần đọc lại 128 trang tài liệu gốc.

> Phạm vi hackathon: chỉ lấy **đường ngắn nhất chạy được** (mục 2). Các endpoint quản trị, master data, compare… liệt kê ở mục 6 để tham khảo, không dựng trong sprint đầu.

---

## 1. Quy tắc chung cho mọi REST Consume

| Mục | Giá trị | Vì sao |
|---|---|---|
| Base URL | `https://idp-api.idpstg.bnksolution.com` | Staging. Đặt thành **Setting** của module để đổi theo stage |
| Header `Authorization` | `Bearer {AccessToken}` | Token lấy từ `POST /auth/login`, cache trong Entity `IdpSession` |
| Header `User-Agent` | `Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36` | **Bắt buộc.** Cloudflare trả `403 error 1010` với UA mặc định. Trong ODC: thêm Header input trên từng method, hoặc set trong `OnBeforeRequest` |
| Header `Accept` | `application/json, text/plain, */*` | |
| Tài khoản | **Vai Tenant (End User)** — không dùng vai Partner/Admin | Admin bị `403` trên toàn bộ `/request/*` |
| Rate limit | Thiết kế ≤ 100 request/60 giây | Đọc từ code (`ThrottlerModule`), giá trị thật trên staging chưa xác nhận |
| Lỗi nghiệp vụ | **Luôn đọc `errorCode` trong body**, không suy từ HTTP status | Request hỏng vẫn trả `200` với `result: {}` và `errorCode` khác null |
| Không bao giờ gọi | `PUT /request/{id}/result` | Làm kết quả trích xuất không bất biến — vi phạm D3 §I-09 |
| Correlation | Nhét `ClaimNo` vào trường `name` khi nộp | IDP không có correlation id / `Idempotency-Key` |

**Entity `IdpSession`** (1 dòng): `AccessToken (Text 400)`, `ObtainedAt`, `TenantId`, `UserId`. Server Action `IdpGetToken()`: nếu token cũ hơn *N* phút (TTL chưa rõ — bắt đầu bằng 30 phút, hoặc login lại khi gặp `401`) thì gọi login lại. Không có refresh token.

---

## 2. Chuỗi hackathon — 6 REST method là đủ

```
[Cán bộ/khách upload PDF]
   → (1) POST /auth/login          → AccessToken
   → (2) POST /request/sync        → requestId + result trích xuất (đồng bộ, ~16 s/trang)
   → (3) PUT  /workflow/request    → workflowRequestId
   → (4) GET  /workflow/run-workflow-request/{id}   (kích hoạt; 400 lần đầu → thử lại)
   → (5) GET  /workflow/request/{id}                (Timer poll tới status 3 / 4)
   → (6) GET  /workflow/request/{id}/result/json    (20 mục cảnh báo/field)
```

Với chứng từ nhiều trang hoặc nhiều file, thay (2) bằng bộ `POST /request/group` → `POST /request` (×n) → poll `GET /request/group/{id}/check-status` → `GET /request/{id}/result` — xem mục 4.

### 2.1 `POST /auth/login`
Request body (JSON): `{"username": "<email>", "password": "<pw>"}`
Response `201`: `{"access_token": "<jwt ~340 ký tự>"}`
Sau đó gọi `GET /user/me` một lần để lấy `tenantId`, `roleId` (End User = 2) và `tenant.expiredDate`.

### 2.2 `POST /request/sync` — nộp + chờ + nhận kết quả trong một lời gọi
Multipart/form-data:

| Field | Kiểu | Ví dụ | Ghi chú |
|---|---|---|---|
| `name` | text | `CLM-2026-000123` | **Đưa ClaimNo vào đây** |
| `templates` | text | `5384,5127,5299,5383,5340` | Chuỗi id ngăn cách phẩy — **không phải array**. Luôn gửi cả danh sách template của bộ; IDP tự chọn template khớp |
| `document` | file | PDF/JPG/PNG | 1 file. `MAX_FILE_SIZE` thật chưa xác nhận |

Response `201`:
```json
{
  "id": "21503", "requestId": "21378", "confidence": "97.81",
  "result": { "result": { "errorCode": null, "errorMessage": null, "processingTime": 16.57,
      "minioPath": "…pdf", "refRequestId": "21378",
      "templates_data": [ { "templateId": "5384", "template": "SET1_Custom-T5384|VNVision-Giấy ra viện/…",
          "subDocumentType": null,
          "data": { "confidence": 98.9, "confidence_standalone": 98.9,
                    "fields": [ { "k": "Admission Date", "v": "20/03/2025", "c": 1, "c_llm": 0, "match_w": "…", "page_number": 1 } ],
                    "tables":  [ ] } } ] } },
  "createdAt": "…"
}
```
Lưu ý: `result.result` **lồng hai lớp** ở `/sync`; ở `GET /request/{id}/result` chỉ một lớp `result`. Viết một hàm `IdpParseExtraction(json)` dùng chung cho cả hai.

ODC Structures gợi ý:
```
IdpExtractionResult { RequestId, ErrorCode, ErrorMessage, ProcessingTime, Templates: List<IdpTemplateData> }
IdpTemplateData     { TemplateId, TemplateName, SubDocumentType, Confidence(0-100), Fields: List<IdpField>, Tables: List<IdpTable> }
IdpField            { K (nhãn), V (Text), C (Decimal 0-1), CLlm, MatchW, PageNumber }
IdpTable            { Name, Rows: List<Record> }   // cột theo tableFields của template — 5383 có 13 cột
```

### 2.3 `PUT /workflow/request` — nộp yêu cầu đánh giá (I-15)
Body (JSON):
```json
{ "ocrRequestIds": ["21378"],
  "workflowId": "5ed8e4fc-27a5-45a7-b104-1216fcfd0797",
  "name": "ASSESS-CLM-2026-000123",
  "autoStart": "true",
  "extraInputData": [] }
```
Response `200`: `{"workflowRequestId": "1402"}`
- **Ghim `workflowId 5ed8e4fc-…` ("Workflow Claim 28/8")** — flow duy nhất trả JSON đầy đủ 4/4 lần. Flow `8d873709-…` trả `{}`.
- `autoStart` **vô tác dụng** → phải gọi bước 2.4.
- `extraInputData[] { template, data }` là chỗ duy nhất nhét thêm dữ liệu (ví dụ `clinicalContext`) — flow hiện tại **không đọc**, để trống trong hackathon.

### 2.4 `GET /workflow/run-workflow-request/{workflowRequestId}` — kích hoạt
Không body. `200` trả bản ghi workflow request (`status: 2` Running). **Lần đầu có thể `400`** → chờ 2 s, gọi lại tối đa 3 lần.

### 2.5 `GET /workflow/request/{workflowRequestId}` — poll
Response: `{ id, name, workflowId, tenantId, status, result: {logs…}, ocrRequests: [...], retries, createdAt, updatedAt }`
`status`: `1` WaitingForInput · `7` InQueue · `2` Running · `3` **Finished** · `4` **Error** · `5` NotFound · `6` Unknown.
Poll bằng **Timer `IdpAssessmentPoller`** (hoặc BPT wait): backoff 3 → 5 → 10 → 30 s, trần 15 phút; quá trần → coi như ngã ngũ, chạy Track 1 một mình, bật cảnh báo `CLM-CONSOLE-015` (đúng thiết kế NFR-2a trong prototype v3.2). Thực đo: ~30 s.

### 2.6 `GET /workflow/request/{workflowRequestId}/result/json` — kết quả (I-16)
Response `200`: `{"data": [ 20 mục ]}` — mỗi mục:
```json
{ "instruction_name": "Kiểm tra Ngày xảy ra sự kiện bảo hiểm", "instruction_type": "Cảnh báo", "warning_level": 1,
  "result": "… Cảnh báo: Ngày xảy ra sự kiện bảo hiểm (20/03/2025) thuộc về tương lai …" }
{ "instruction_name": "Tìm kiếm AdmissionDate", "instruction_type": "Trích xuất field",
  "fieldname": "AdmissionDate", "field_validation": true, "result": "20/03/2025" }
```
- `instruction_type` ∈ {`Cảnh báo`, `Trích xuất field`}; `warning_level` ∈ {0, 0.5, 1} chỉ có ở Cảnh báo.
- Chuỗi `"None"`, `"null"`, `"00000"` là **giá trị rỗng/không tìm thấy** → chuẩn hoá thành null.
- Nếu `400` với `statusCode: 4007 "Workflow result is bad"` → chờ và gọi lại (đã gặp 400 rồi 200 trong cùng một request); nếu `data` là `{}` hoặc rỗng sau khi Finished → coi là **hỏng**, không phải "sạch".

ODC Structure: `IdpAssessmentItem { InstructionName, InstructionType, WarningLevel (Decimal), FieldName, FieldValidation (Boolean), Result (Text) }`.

Entity đích (đã có trong prototype v3.2): `ClinicalAssessment` (State COMPLETE/PARTIAL/ERROR, KnowledgeBaseVersion = `workflowId`) và `ClinicalRecommendation` — **nhưng** flow hiện chỉ trả cảnh báo mức hồ sơ, không có `proposedReasonCode`/`evidence[]`/band, không theo dòng chi phí. Xem mục 5.

---

## 3. Quy tắc chuẩn hoá trong adapter (`IdpParseExtraction`)

1. `fields[].k` là **nhãn tiếng Việt/Anh của template**, không phải khoá máy. Giữ **bảng ánh xạ `templateId + k → FieldCode` trong Entity `IdpFieldMap`** (có version); cảnh báo khi gặp `k` lạ thay vì bỏ qua im lặng.
2. `v = ""` → `null`. **Field rỗng vẫn có `c = 1`** → confidence của claim tính = **min `c` trên các field bắt buộc có giá trị**, kèm chỉ số completeness riêng (số field bắt buộc có giá trị / tổng).
3. Hai thang confidence: `fields[].c` 0–1, `data.confidence` 0–100. Quy về 0–1 khi lưu.
4. Ngày xuất hiện cả `dd/MM/yyyy` và `yyyy-MM-dd` → parse cả hai. Tiền là text có dấu chấm/phẩy → ép kiểu.
5. `subDocumentType` trả `null` nếu chưa nạp `POST /mbal/template-map` cho template đó → nạp trước cho mọi template trong bộ; crosswalk `CL*` → `T1-nn/T2-nn` giữ trong `IdpFieldMap`.
6. Mọi lỗi IDP (`errorCode != null`, timeout, 4xx/5xx) → claim sang `EXTRACTION_ERROR`, vào hàng đợi operator; **không bao giờ** thành `DECLINED`.
7. Copy kết quả vào Entity ngay lần đọc đầu (`RawExtractionJson`), coi đó là bản gốc bất biến.

---

## 4. Biến thể nhiều file (khi cần)

| Bước | Endpoint | Body / Response |
|---|---|---|
| Tạo nhóm | `POST /request/group` body `{}` | `201 {"id": "4"}` |
| Nộp từng file | `POST /request` multipart `name`, `templates`, `requestGroupId`, `document` | `201 {"requestId": 21376, "pageCount": 1}` |
| Poll nhóm | `GET /request/group/{groupId}/check-status` | `200 [{"requestId": "21376", "requestStatus": "Completed"}]` — `Sending/Processing/Completed/Failed/Cancelling/Cancelled` |
| Kết quả | `GET /request/{requestId}/result` | `{ result: {…}, requestId, extraParams }` (1 lớp `result`) — hoặc `GET /request/{id}/check-status` khi Completed đã kèm `data.result` |
| Ảnh gốc | `GET /request/{requestId}/raw-file` | **Trả `application/zip`** (không phải PDF) → unzip trước khi hiển thị. Thay cho deep link I-10 |
| Retry / Cancel | `PATCH /request/{id}/retry`, `PATCH /request/{id}/cancel` | Cancel chỉ hợp lệ khi đang chạy |

Đường tắt theo nghiệp vụ: `POST /mbal/create-request` multipart `name`, `claim_type` (`MEDICAL_CARE/MEDICAL_ILLNESS/MEDICAL_INJURY/MEDICAL_DEATH`), `document_type` (`CL05…`), `document` — backend tự tra `docset-map`. Cần cấu hình trước bằng `POST /template/document-set` + `POST /mbal/docset-map`.

Không dùng: `POST /request/from-url` (đang hỏng), `POST /request/merge-multiple` (chỉ ảnh, ≤10 file), socket.io `Channel_{tenantId}` (ODC không nghe được — poll thay).

---

## 5. Ánh xạ sang lớp agent (Agent Workbench)

| Agent | Tool gọi IDP | Ghi chú |
|---|---|---|
| **Intake agent** | `IdpSubmitSync` (2.2), `IdpGetRawFile` | Đọc `templates_data[].templateId` để đối chiếu checklist chứng từ; `errorCode` → hỏi bổ sung / `EXTRACTION_ERROR` |
| **Adjudication agent** | — (đọc Entity đã chuẩn hoá) | Gọi Server Action chuỗi 14 bước trong `CLM_Core`; không gọi IDP |
| **Clinical agent** | `IdpStartAssessment` (2.3+2.4), `IdpGetAssessment` (2.6), `IdpSearchHospital` | Nhận 20 mục của flow 28/8 làm **đầu vào**, tự sinh `proposedReasonCode` (NOTREL/NOTCOV), `evidence[]`, band — lấp đúng khoảng trống H7–H9 mà IDP chưa có |
| **Duplicate/Fraud-signal agent** | `IdpSearchHospital` | Tra `hospital_code`, `country` từ bảng Hospital (đóng Q-IDP-05) |
| **Orchestrator** | — | Điều phối, chờ Track 1 + Track 2 ngã ngũ, gác cổng 4 điều kiện, ghi audit |

`IdpSearchHospital`: `GET /metadata/opensearch/table/322/search?searchTerm=<tên>&fields=hospital_name&size=5` → `{ total, results: [ { id, score, data: { hospital_name, hospital_code, country } } ] }`. Bảng 322 = 13.328 dòng; bảng 316 Surgery Code (95 dòng, `surgery_code` dạng `1.1`), 313 Claim Types, 314 Document Types, 321 Country.

---

## 6. Endpoint tham khảo (không dựng trong sprint đầu)

`GET /usage/current` (`pageUsed`, `totalBlockRegistered` — giám sát hạn dùng; hết hạn thì OCR dừng nhưng vẫn `200`) · `GET /template/compact`, `GET /template/{id}` (`fields[]`, `tableFields[]` — xem cột `lines[]` của bảng kê) · `GET /template/document-set` (bộ 193, 194) · `GET /mbal/docset-map`, `/mbal/template-map`, `/mbal/templates` · `POST /request/compare` (đo độ chính xác với XLSX ground-truth — dùng cho slide KPI) · `GET /workflow/forwarder/api/v1/flows/{id}` (chi tiết flow, nhẹ hơn `/flows` 2,8 MB) · `GET /metadata/forwarder/api/database/rows/table/{id}/?user_field_names=true&size=200`.

---

## 7. Việc phải làm ngoài code

- **Tài khoản tích hợp riêng vai Tenant** cho tenant demo; không dùng tài khoản cá nhân đang có trong `api-and-accounts.txt`.
- **Che PII**: file test hiện tại là tờ mô phỏng "DATA PLATFORM - MBAL" nhưng vẫn chứa họ tên, ngày sinh, số thẻ BHYT. Dựng bộ chứng từ demo **ẩn danh hoàn toàn** trước khi đưa lên tenant ODC hoặc chiếu cho OutSystems.
- **Bộ chứng từ sạch**: tạo Document Set mới chỉ gồm template y tế (`5384/5127 Giấy ra viện`, `5299 Đơn thuốc`, `5383 Bảng kê chi phí`, `5340 Kết quả xét nghiệm`, `5312 Biên lai viện phí`) — bộ 193 hiện lẫn Passport, Sales Invoice, Money transfer.
- **Xác nhận với đội IDP**: TTL token, `MAX_FILE_SIZE`, `THROTTLE_*` thật; flow 28/8 có được giữ ổn định trong 4 tuần sprint không.
- **Xin IDP (không vá được phía mình, sau hackathon)**: `documentId` + `bbox` trong `/result`; flow nhận `lines[]`; `proposedReasonCode` + `evidence[]`; khoá field ổn định thay nhãn; sửa `from-url`.
