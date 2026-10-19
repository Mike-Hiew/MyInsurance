# Ghi chú thư mục `E:\Hackaton\insurance BNK\IDP_API_Spec` — API thật của BnK IDP cho Claim MVP

Ngày thu thập: 03/09/2026 (cùng ngày dữ liệu trong thư mục được gọi lên staging). Đọc trực tiếp trên máy Mike qua thư mục kết nối; đọc toàn bộ Phần 1 + Phụ lục của tài liệu chính, các mục endpoint trọng yếu của Phần 2, `INDEX.md`, `README.md` nội bộ và một số file JSON để kiểm chứng.

Tài liệu này bổ sung cho bản tổng hợp thư mục `Bnk IDP` (bộ tài liệu thiết kế MVP1) — nếu bộ Deliverables mô tả **IDP nên như thế nào** theo hợp đồng D3/A4, thì thư mục này ghi lại **IDP thực tế đang có gì** trên staging, và khoảng cách giữa hai bên.

> Lưu ý: đây là API của **BnK IDP** (idp-api.idpstg.bnksolution.com), **không phải InsureMO**. Tính đến 03/09/2026 vẫn chưa có payload InsureMO nào được ghi lại.

---

## 1. Thư mục này là gì

Bản **giao hàng kỹ thuật** của đội tích hợp: kết quả gọi thật toàn bộ API của IDP (FO – `https://idp-api.idpstg.bnksolution.com`) bằng hai tài khoản (**FO Admin** = vai Partner, **FO End User** = vai Tenant), rồi đối chiếu từng yêu cầu của Claim Platform (rút từ D3, A4, A5, D1, D2 của bộ Deliverables) với mức độ IDP đáp ứng.

| Đường dẫn | Nội dung | Ghi chú |
|---|---|---|
| `IDP_APIs_for_Claim_MVP_v1.0.md` (188 KB, 5.103 dòng) | **Tài liệu chính**, 3 phần: (1) bảng đối chiếu yêu cầu Claim Platform ↔ IDP, 10 nhóm A–J, 64 dòng + mục kiểm chứng nhận định PO; (2) danh sách API, 103 mục / 93 endpoint duy nhất, 6 nhóm; (3) phụ lục enum | Sinh tự động bởi script `10_dung_tai_lieu.py` từ captures |
| `IDP_APIs_for_Claim_MVP_v1.0.pdf` (2,9 MB, 128 trang) | Bản in Chromium của cùng tài liệu, 10:01 UTC 03/09/2026 | Cùng nội dung |
| `request-response-json/` + `INDEX.md` | **149 file JSON**, mỗi file một endpoint: request + response thật của từng vai, token/mật khẩu đã thay bằng `<token>`/`<redacted>`. 8 thư mục: `00-chuoi-end-to-end` (37), `01-xac-thuc-va-danh-tinh` (20), `02-template-va-bo-chung-tu` (16), `03-anh-xa-mbal` (10), `04-nop-chung-tu-va-trich-xuat` (19), `05-danh-gia` (14), `06-master-data` (32), `07-van-hanh` (1) | Tài liệu chính ghi "192 file" — con số cũ, thực đếm 149 |
| `_noi-bo/` | Ghi chú **không gửi ra ngoài**: 13 script Python + `captures/flow`, `captures/sweep` | Script đọc tài khoản từ `../../api-and-accounts.txt` (**plaintext, nằm ngoài thư mục này**) |

Cảnh báo dữ liệu cá nhân: chứng từ test là **giấy ra viện thật** (`DP_DATA_case1.pdf`), nên cả tài liệu lẫn JSON chứa họ tên bệnh nhân, ngày sinh, số thẻ BHYT, tên bệnh viện. README nội bộ yêu cầu chạy `12_che_du_lieu_ca_nhan.py` (xuất ra `_da-che/`) trước khi gửi ra ngoài công ty, và **không nén cả thư mục cha `New_Project_Documents`** vì có file mật khẩu.

---

## 2. Luồng tích hợp end-to-end đã chạy thật (thư mục `00-`)

Chuỗi A1 → D4 với 1 chứng từ 1 trang, ngày 03/09/2026:

1. `POST /auth/login` (201) → `GET /user/me` xác nhận role/tenantId (tenant test **176**, partner 78/40).
2. `GET /template/compact` + `GET /template/document-set` → bộ chứng từ **193 "Chứng từ y tế _ CSYT"** (7 template); `GET /mbal/docset-map` → bảng `claim_type + document_type → bộ chứng từ`.
3. `POST /request/group` (201, `id: 4`) → `POST /request` multipart (`name`, `templates` = chuỗi ngăn cách phẩy `"5445,5444,5443,5384,5127,5340,5299"`, `requestGroupId`, `document`) → `requestId 21376`, `pageCount 1`.
4. Poll `GET /request/group/4/check-status` 7 lần (0 Sending → 1 Processing → 2 Completed; ~18 giây) → `GET /request/21376/result` / `get-ocr` / `check-workflow-request`.
5. `PUT /workflow/request` (`ocrRequestIds`, `workflowId`, `name`, `autoStart` — vô tác dụng) → `workflowRequestId 1402`; `GET /workflow/run-workflow-request/1402` trả **400**, poll 5 lần, `/result/json` **400**.
6. Tạo lại `1403` → `run-workflow-request/1403` **200** → poll 4 lần → `result/json` lần đầu 400, lần sau **200**; request **1404** chạy bằng flow chuẩn "Workflow Claim 28/8" trả JSON 20 mục.

Thời gian: trích xuất ~16 giây/1 trang (`processingTime 16.14`), đánh giá ~30 giây.

---

## 3. Bản đồ API (Phần 2 tài liệu chính)

| Nhóm | Endpoint chính | Ai gọi được | Điểm cần nhớ |
|---|---|---|---|
| 2.1 Xác thực & danh tính (21) | `POST /auth/login`, `/auth/logout`, `GET /user/me`, `/partner/permissions`, `/tenant/*`, `/usage/*`, `/encryption/public-key`, `GET /` health | Tenant/Partner lệch nhau | Chỉ login bằng tài khoản người dùng, **không có API key/OAuth2 client-credentials, không thấy refresh token**, TTL chưa rõ. `GET /usage/current` giám sát hạn dùng (hết hạn → OCR dừng, lỗi kèm HTTP 200) |
| 2.2 Template & bộ chứng từ (15) | `GET /template/compact`, `/template/{id}` (có `tableFields`), `/template/document-set[-list]`, `POST /template/document-set`, `/template/sync-bo-templates` | Phần lớn chỉ End User | `document-set-list` bắt buộc `sort` (thiếu → 400); `sync-bo-templates` bắt buộc `override` |
| 2.3 Ánh xạ MBAL (10) | `GET/POST /mbal/docset-map`, `/mbal/template-map`, `GET /mbal/docsets`, `/mbal/templates`, `/mbal/statistics`, `POST /mbal/create-request` | Cả hai vai đọc; POST chỉ End User | Cầu nối từ vựng nghiệp vụ ↔ templateId. Đã tạo docset **195** + docset-map **78** |
| 2.4 Nộp chứng từ & trích xuất (21) | `POST /request/group`, `/request`, `/request/sync`, `/request/from-url`, `/request/merge-multiple`, `GET /request/{id}/check-status`, `/result`, `/get-ocr`, `/raw-file`, `/export`, `PUT /request/{id}/result`, `PATCH .../retry`, `.../cancel`, `POST /request/compare[-multiple]` | **Chỉ End User** (Admin 403 toàn bộ) | `from-url` **đang hỏng**; `merge-multiple` chỉ ảnh, tối đa 10 file; `/request/sync` gộp nộp+poll+kết quả (~16 s/trang); `compare` nhận XLSX ground-truth; `PUT .../result` làm kết quả **không bất biến** |
| 2.5 Đánh giá (17) | `GET /workflow/forwarder/api/v1/flows[/{id}]`, `/workflow/get-workflow-components`, `PUT /workflow/request`, `GET /workflow/run-workflow-request/{id}`, `/workflow/request/{id}[/result/json]`, `/workflow/request/search`, `GET /tool` | Lọc theo `tenantId` | Engine **Langflow**, `/flows` ~2,8 MB; `autoStart` không tác dụng → gọi `run-workflow-request`, lần đầu có thể 400 |
| 2.6 Master data (19) | `GET /metadata/forwarder/table-list`, `.../api/database/rows/table/{id}/`, `.../fields/table/{id}/`, `.../views/table/{id}/`, `/metadata/forwarder/table-data/{viewId}`, `/metadata/opensearch/table/{id}/{index/async,index/status,info,search}` | Hầu hết cả hai vai | `table-data` nhận **VIEW id**; `POST table-data` không dùng được |

Áp dụng mọi lời gọi: `Authorization: Bearer`, `Accept: application/json`, và **`User-Agent` giả trình duyệt là bắt buộc** (Cloudflare chặn UA mặc định, `403 error 1010`). Rate limit đọc từ code: 100 request/60 giây (giá trị thật trên staging chưa xác nhận).

### 3.1 Payload trích xuất (`GET /request/{id}/result`)
`result.templates_data[]` → `data.fields[]` với `k` (nhãn), `v`, `c` (0–1), `c_llm`, `match_w`, `page_number`; `data.tables[]`; `data.confidence` thang **0–100**; `subDocumentType` (mã `CL*`, `null` nếu chưa nạp `mbal/template-map`); `errorCode`, `minioPath`, `processingTime`. Ba điểm: `k` là **nhãn tiếng Việt** chứ không phải khoá máy; hai thang confidence; **field rỗng vẫn `c: 1`**.

### 3.2 Kết quả đánh giá (`GET /workflow/request/{id}/result/json`)
`data[]` 20 mục: `instruction_name`, `instruction_type` (`Cảnh báo` | `Trích xuất field`), `warning_level` (0 / 0,5 / 1), `fieldname` + `field_validation`, `result` (tiếng Việt). **Flow chuẩn phải ghim: `5ed8e4fc-27a5-45a7-b104-1216fcfd0797` "Workflow Claim 28/8"** (154 node; comparator Insured Name, DOB, Gender, ID, Admission/Discharge/Death/Event Date, Diagnosis Codes, Place Of Treatment, `Metatable Find Matched Hospital`, `LLM Find Surgery Code`). Flow "Claim 25/08" trả `{}` 0/4 lần.

### 3.3 Enum và master data
`RequestStatus`: 0 Sending, 1 Processing, 2 Completed, 3 Failed, 4 Cancelling, 5 Cancelled. `WorkflowRequestStatus`: 1 WaitingForInput, 7 InQueue, 2 Running, 3 Finished, 4 Error, 5 NotFound, 6 Unknown.
Master data (database 6): **322 Hospital** (13.328 dòng, có `country`, `hospital_code`), **321 Country** (242), **316 Surgery Code** (95), **314 MBAL Document Types** (10 mã `CL05 CL06 CL07 CL08 CL10 CL11 CL12 CL15 CL26 CLM_04`, tất cả `Active: false`, không tên), **313 MBAL Claim Types** (`MEDICAL_CARE`, `MEDICAL_ILLNESS`, `MEDICAL_INJURY`, `MEDICAL_DEATH`).
Bộ chứng từ 193 gồm 7 template `5445 Money transfer request · 5444 Sales Invoice · 5443 Passport · 5384 VNVision-Giấy ra viện · 5127 Giấy ra viện · 5340 Kết quả xét nghiệm · 5299 VNVision-Đơn thuốc` — **3/7 không phải chứng từ y tế**; 194 "Claim assessment Demo" (7 template). Template bảng kê có `tableFields` khác nhau: 5383 13 cột, 5218 7 cột, 5125 9 cột, 5312 5 cột.

---

## 4. Đối chiếu yêu cầu Claim Platform ↔ IDP (Phần 1)

Thang: ✅ đáp ứng · ⚠️ một phần · ❌ không · ❔ chưa kiểm chứng. Giải pháp: `ADAPTER` · `XIN IDP` · `CẤU HÌNH` · `HOÃN`.

| Nhóm | Số dòng | ✅ | ⚠️ | ❌ | ❔ | Điểm chốt |
|---|---|---|---|---|---|---|
| A Xác thực & vận hành | 5 | 1 | 3 | 1 | 0 | Không service account/OAuth2 (A1), không correlation id (A2 → `claimNo` vào `name`), rate limit 100/phút (A3), giả User-Agent (A5) |
| B Nộp chứng từ I-07 | 8 | 1 | 2 | 5 | 0 | `from-url` hỏng (B1), không `Idempotency-Key` (B3), không `callbackUrl` (B4), `extraParams` đóng (B5), `merge-multiple` 10 file (B7), XML T1-04 không có đường (B8 HOÃN). `templateSetCode` làm được bằng Document Set + docset-map (B6 ✅) |
| C Phân loại | 5 | 1 | 2 | 1 | 1 | Tự phân loại khi nộp nhiều template (C1 ✅); crosswalk `CL*` → `T1-nn` chưa có (C3); không báo mã mới (C4) |
| D Trích xuất field | 10 | 0 | 2 | 7 | 1 | **`k` là nhãn tiếng Việt (D1)**; `""` thay `null` (D2); hai thang confidence (D3); trung bình thay min (D4); rỗng `c: 1` (D5); **không `documentId`/`bbox` trong `/result` (D6)**; ngày/tiền không nhất quán (D8, D9); `serviceCategory` không tính (D10) |
| E Dòng chi phí | 6 | 0 | 4 | 2 | 0 | Mỗi template một bộ cột (E1); không `serviceCategory` (E2); `paidBySocialInsurance` 3 tên (E3); **không `documentDate`/`icd10` theo dòng (E4)**; `drugRegistrationNo` không có (E5); `amountClaimed` tách cột (E6) |
| F Đối chiếu chéo & trạng thái | 5 | 1 | 1 | 3 | 0 | R1–R9 tự tính (F1–F4); **`POST /request/compare` đo độ chính xác có sẵn (F5 ✅)** |
| G Thông báo & kết quả I-08/I-09 | 6 | 3 | 1 | 2 | 0 | Không webhook, chỉ socket.io `Channel_{tenantId}` event `updateOcrResult` (G1); poll nhóm ✅ (G2); **request hỏng trả HTTP 200 với `result: {}` (G4 → luôn đọc `errorCode`)**; kết quả không bất biến (G5 → copy lần đầu, cấm PUT) |
| H Đánh giá lâm sàng I-15/I-16 | 12 | 2 | 2 | 8 | 0 | Không API độc lập, tất cả qua `/workflow` (H1); `clinicalContext` qua `extraInputData[]` (H2); **không đánh giá theo dòng (H3, H10, H11)**; `autoStart` vô tác dụng (H5); **không `proposedReasonCode` NOTREL/NOTCOV, không `evidence[]`, không band (H7–H9)** → Track 2 chạy advisory |
| I Màn hình thẩm định F46/F56 | 5 | 2 | 0 | 3 | 0 | Ảnh gốc qua `raw-file` ✅ (I1); **neo field ↔ vùng ảnh không làm được (I2)**; panel F56 thiếu 4/5 trường (I4); deep link I-10 HOÃN (I5) |
| J Master data | 2 | 2 | 0 | 0 | 0 | `facilityCountry` từ bảng Hospital → **đóng Q-IDP-05** (J1); đồng bộ 5 bảng (J2) |
| **Tổng** | **64** | **13** | **17** | **32** | **2** | |

### 4.1 Kiểm chứng nhận định PO (BIA, 29/08/2026)
PO **đúng 4/6**, hai chỗ lệch theo hướng tốt hơn: (2) flow đã đối chiếu thông tin khách khai với DataPlatform; (3) flow đã kiểm tra ngày tháng và bắt được lỗi thật ("ngày xảy ra sự kiện 20/03/2025 thuộc về tương lai"). (1) đủ giấy tờ, (4) giả mạo, (5) đánh giá y khoa NOTREL/NOTCOV, (6) confidence — PO đúng. Tra bệnh viện/phẫu thuật: `HospitalCode` trả `00000` dù có trong danh mục — "chưa đáng tin" là đúng; tra thẳng `/metadata/opensearch` thì ra đúng.

---

## 5. Đánh giá nhanh và việc cần làm

### 5.1 Giá trị
Tài liệu **duy nhất trong toàn bộ dự án có dữ liệu đo được**: ~16 s/trang trích xuất, ~30 s đánh giá, payload thật, mã lỗi thật, quyền theo vai thật. Chưa có số về độ chính xác. Phát hiện hai thứ có sẵn mà MVP1 chưa biết: **`POST /request/compare`** (đo ground-truth) và **bảng Hospital 13.328 dòng có `country`** (đóng Q-IDP-05).

### 5.2 Khoảng cách phải xin IDP (không vá được phía mình)
`documentId` + `bbox` trong `/result` (D6, I2); flow nhận `lines[]` để đánh giá theo dòng (H3, H10, H11); `proposedReasonCode` + `evidence[]` + band (H7–H9); `icd10`/`documentDate` mức dòng (E4); khoá field ổn định (D1); sửa `from-url` (B1); mở `extraParams` (B5); crosswalk `CL*` ↔ `T1-nn` (C3); giá trị thật `MAX_FILE_SIZE`, `THROTTLE_*`, TTL token; parser XML hoá đơn (B8).

### 5.3 Quy tắc adapter đã chốt (checklist)
Tenant riêng cho tích hợp, cache token; `claimNo` vào `name`; User-Agent giả trình duyệt; 100 req/phút, circuit breaker 5 lỗi/60 s; sha256 trước khi nộp và so qua `raw-file`; bảng idempotency riêng; luôn nộp cả danh sách template của bộ; chia gói >10 file, gom một `requestGroupId`; poll backoff 3→5→10→30 s trần 15 phút; **luôn đọc `errorCode`**; copy kết quả lần đầu và **cấm `PUT /request/{id}/result`**; chuẩn hoá `""` → `null`, ngày, tiền; confidence claim = min trên field bắt buộc + completeness; ghim `workflowId 5ed8e4fc`, coi `{}` là hỏng; luôn gọi `run-workflow-request` và thử lại khi 400; mọi lỗi IDP → `EXTRACTION_ERROR`/timeout vào hàng đợi operator, không bao giờ thành `DECLINED`.

### 5.4 Bất nhất trong thư mục
192 vs 149 file JSON. Bộ 193 "y tế" chứa Passport, Sales Invoice, Money transfer. MBAL Document Types 10 mã đều `Active: false`, không tên — crosswalk chưa tồn tại thật. Cột "Ai gọi được" một số mục hiển thị "Cả hai vai đều lỗi" trong khi trạng thái thật 200 — lỗi render script. PII thật chưa che.

### 5.5 Ý nghĩa cho hackathon
- IDP có thể là **tool thật** cho agent trên ODC (REST Consume + Timer/BPT poll), nhưng phải qua adapter: login user (không service account), header `User-Agent` tùy chỉnh, poll thay webhook, đọc `errorCode` trong HTTP 200.
- Các ❌ ở nhóm H (đánh giá theo dòng, NOTREL/NOTCOV, evidence, band) chính là **khoảng trống mà một Clinical agent trên Agent Workbench có thể lấp** — nhận `lines[]` + cảnh báo của flow 28/8, tra KB, trả reason code + evidence. Đây là lý lẽ mạnh để xây Track 2 trên Agent Workbench thay vì chờ IDP.
- **Phải che PII** trước khi demo (BC-1 của MVP1 và quy tắc chung của hackathon); không dùng `DP_DATA_case1.pdf` gốc.
