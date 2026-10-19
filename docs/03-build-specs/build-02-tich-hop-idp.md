# Build 02 — Tích hợp BnK IDP

**Chuẩn gốc: Claim MVP.** Nguồn chính là `IDP_APIs_for_Claim_MVP_v1.0.md` — đặc tả dành riêng cho Claim MVP, payload **thật**, gọi ngày **03/09/2026** lên staging bằng tài khoản test (tenant `176`, user `203`, partner `40`). Nguồn phụ: `request-response-json/INDEX.md` (149 file JSON mẫu) và `D3_Integration_and_Data_Contract_Spec_v2.3.md` (hợp đồng I-07 · I-08 · I-09 · I-10 · I-15 · I-16).

Đọc cùng: `build-00` (ranh giới) · `build-01` (entity `IdpSession`, `IdpFieldMap`, `IdpReplay`) · `build-03` (rule).

> **Thay đổi so với bản trước.** Bản trước viết theo chuẩn MBAL-IDP (`POST /mbal/create-request` làm đường chính, `MBALDocsetMap`, `extraParams` với khoá `mbal_*`, template `777/776/1091/2645/1141`). Thân bài nay **chỉ nói về chuẩn Claim MVP**. Những gì thuộc MBAL-IDP mà vẫn còn giá trị tham chiếu nằm ở **Phụ lục A**, có nhãn rõ ràng.
>
> Phần **confidence** (§7) và **bounding box** (§8) giữ nguyên nội dung kỹ thuật của bản cũ, vì phía sau vẫn là cùng một engine `llm-du` — chỉ đổi tên endpoint và tham số theo chuẩn mới.

---

## 1. Nguyên tắc và môi trường

| Hạng mục | Giá trị |
|---|---|
| **Base URL (FO)** | `https://idp-api.idpstg.bnksolution.com` |
| **Tenant test** | `176` — *"Phòng 1"*, `expiredDate 2027-08-31`, `blockLimit 1` |
| **User test** | `203` — `<redacted-email>`, `roleId 2` |
| **Partner** | `40` (lưu ý: `refPartnerId` trong payload kết quả OCR là `"78"`, **không** phải `40` — hai không gian id khác nhau, **cần xác thực bằng gọi thật** nếu định dùng để correlate) |
| **Prefix** | Không có global prefix. Đường dẫn là gốc: `/auth/login`, `/request`, `/workflow`, `/metadata` |
| **Service duy nhất expose ra ngoài** | FO `partner-portal-backend`. `ocr-service`, Langflow (`/workflow/forwarder/*`), `metadata-svc` (Baserow), OpenSearch đều **nằm sau nó** và chỉ gọi được qua proxy của FO |
| **Webhook** | **Không tồn tại.** `CreateRequestDto` không có `callback_url`. Chỉ có polling và socket.io — xem §10 |
| **Rate limit** | Đọc từ code FO: `ThrottlerModule` mặc định `ttl 60000ms` / `limit 100` ⇒ **100 request / 60 giây**, `RequestController` có gắn `ThrottlerGuard`. Override được bằng env `THROTTLE_TTL` / `THROTTLE_LIMIT` ⇒ **giá trị thật trên staging chưa chắc là mặc định — cần xin IDP xác nhận** |
| **Giới hạn file** | `POST /request/merge-multiple` giới hạn **cứng 10 file** (`FilesInterceptor('document', 10, …)`). Kích thước một file do env `MAX_FILE_SIZE` quyết định — **giá trị chưa biết, cần xin IDP**. `POST /request/group` **không giới hạn số request trong nhóm** ở tầng code |
| **Giới hạn gói theo D3** | D3 §I-07: tối đa **20 chứng từ, 50 trang, 100 MB**. Con số 20 **cao hơn** giới hạn 10 file của `merge-multiple` ⇒ adapter phải chia gói nhưng vẫn gom về **một `requestGroupId`** |
| **Quota** | Tính theo **số trang**, theo tenant. Theo dõi bằng `GET /usage/current`. Partner hết hạn thì **mọi** request OCR dừng với `statusCode 1014 · Partner is expired` **kèm HTTP 200** |
| **Thời gian xử lý thật đo được** | 1 trang PDF: `processingTime` **16,14 s** (request 21376) · **16,57 s** (21378) · **19,04 s** (21383). Poll cả nhóm xong sau **~18 giây**. Flow đánh giá chạy tới cùng **~30 giây** |

**Ba thứ áp dụng cho mọi lời gọi** (trích verbatim):

```
Authorization: Bearer <access_token>
User-Agent: Mozilla/5.0 (…) Chrome/126.0.0.0 Safari/537.36
Accept: application/json, text/plain, */*
```

---

## 2. Ba quirk phải xử lý ngay ở REST Consume

Không có trong swagger. Bỏ qua là mất nửa ngày debug.

| # | Quirk | Xử lý |
|---|---|---|
| 1 | **`User-Agent` bắt buộc** — thiếu thì Cloudflare trả `403 · error code 1010`. Tài liệu nguồn nói rõ: *"không phải lỗi ứng dụng và không có trong swagger"* | Set header giả trình duyệt trên **mọi** REST method:<br>`Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36` |
| 2 | **`templates` truyền dạng chuỗi ngăn cách bởi dấu phẩy**, không phải mảng JSON | `"5384,5127,5299"`. Gửi array thì lỗi `400 · invalid input syntax for type bigint: "NaN"`. **Swagger khai là `array` — đây là chỗ dễ sai nhất.**<br>⚠️ **Ngược lại ở `POST /request/from-url`**: endpoint đó đòi **mảng số**, gửi chuỗi CSV thì lỗi `ids.join is not a function` |
| 3 | **HTTP 200 vẫn có thể là lỗi** | Luôn đọc `errorCode` / `statusCode` **trong body** trước khi coi là thành công. Ba ví dụ thật: `1014 Partner is expired` trả HTTP 200 · request hỏng trả HTTP 200 với `result: {}` · `/result/json` của flow sai trả `{}` kèm HTTP 200.<br>Viết thành **một hàm dùng chung** để không ai quên (D3 §I-09 G4) |

**Thêm bốn điều cùng loại:**

- `GET /request/{id}/raw-file` trả **ZIP** (`content_type: application/zip`), không phải ảnh.
- `GET /template/document-set-list` **bắt buộc có tham số `sort`**; thiếu thì `400 · missing FROM-clause entry for table "template"` (lỗi SQL của IDP: nhánh mặc định chạy `orderBy('template.id')` trong khi alias là `document_sets`). Đã thử `sort=id`, `sort=name`, `sort=-id`, `sort=-createdAt` — đều `200`.
- `GET /workflow/forwarder/api/v1/flows/assigns/all` **bắt buộc có `target_tenant_id`**; thiếu thì `500 · invalid input syntax for type bigint: "NaN"`.
- Response rỗng **không phải lúc nào cũng là hỏng**: 7 endpoint là *lệnh hành động* trả `""` / `{message:"Done"}` đúng theo thiết kế; và với mọi endpoint lọc theo tenant, **rỗng có thể chỉ nghĩa là sai tài khoản**.

---

## 3. Xác thực

Chỉ JWT. Không có API key cho bên thứ ba, **không có refresh token trong response**.

```
POST /auth/login
```

**Request** (verbatim):

```json
{
  "username": "<redacted-email>",
  "password": "<redacted>"
}
```

**Response** — HTTP `201`:

```json
{
  "access_token": "<340 ký tự, đã ẩn>"
}
```

Gửi kèm mọi request: `Authorization: Bearer <access_token>`.

### 3.1 Hai vai

Tài liệu nguồn ghi rõ, **trích verbatim**:

> **Hai vai:** `Partner = 1` — gọi tắt **FO Admin**; `Tenant = 2` — gọi tắt **FO End User**. Cột *"Ai gọi được"* trong tài liệu này **không suy từ swagger** mà là kết quả gọi thật bằng cả hai tài khoản.

Kiểm vai bằng `GET /user/me` → đọc `roleId`. Tài liệu nguồn: *"Cách nhanh nhất để biết token là Admin hay End User: đọc `roleId` — `1` = Partner (Admin), `2` = Tenant (End User)."*

**Response `GET /user/me`** (verbatim):

```json
{
  "id": "203",
  "email": "<redacted-email>",
  "name": "Brian",
  "tenantId": "176",
  "roleId": 2,
  "tenant": {
    "expiredDate": "2027-08-31",
    "name": "Phòng 1"
  }
}
```

### 3.2 Endpoint nào vai nào gọi được

Cột *"Ai gọi được"* dưới đây **trích nguyên văn** từ tài liệu nguồn — nó là **kết quả gọi thật**, không phải suy từ swagger.

| Endpoint | Ai gọi được (verbatim) |
|---|---|
| `POST /auth/login` | **Chỉ End User** |
| `GET /user/me` | Cả hai vai đều lỗi (Admin `None` · End User `None`) — trạng thái thật `200` |
| `GET /partner/permissions` | **Cả hai** |
| `GET /tenant/my-tenant` | **Chỉ End User** (Admin: 403) |
| `GET /usage/current` · `GET /usage` | **Cả hai** |
| `GET /user` · `GET /tenant` · `GET /tenant/compact` · `GET /user/{id}` · `GET /tenant/usage-detail` | **Chỉ Admin** (End User: 403) |
| `GET /template/compact` | Cả hai vai đều lỗi (Admin `None` · End User `None`) — thực tế FO Admin `403` · FO EndUser `200` |
| `GET /template/document-set` | Cả hai vai đều lỗi (Admin `None` · End User `None`) — thực tế FO Admin `403` · FO EndUser `200` |
| `GET /template/document-set/{id}` | **Chỉ End User** (Admin: 403) |
| `GET /template/{id}` | **Cả hai** *(với 5384)* · **Chỉ End User** (Admin: None) *(với 5383)* |
| `POST /template/document-set` | **Chỉ End User** (Admin: None) |
| `GET /template?page&limit` | **Chỉ End User** (Admin: 403) |
| `GET /template/partner-templates` · `/compact-for-partner` · `/get-sync-templates-bo` · `/sync-bo-templates` | **Chỉ Admin** (End User: 403) |
| `GET /mbal/docset-map` · `/mbal/template-map` · `/mbal/docsets` · `/mbal/templates` · `/mbal/docset-map/{id}` · `/mbal/template-map/{id}` | **Cả hai** |
| `POST /mbal/docset-map` · `POST /mbal/template-map` · `POST /mbal/create-request` | **Chỉ End User** (Admin: None) |
| **Toàn bộ `/request/*`** | **Chỉ End User.** Tài liệu nguồn: *"`RequestController` gắn quyền `Tenant` ở cấp class, tài khoản Admin trả `403`"* |
| `PUT /workflow/request` | **Chỉ End User** |
| `GET /workflow/run-workflow-request/{id}` | **Cả hai** |
| `GET /workflow/request/{id}` · `/result/json` | **Chỉ End User** (Admin: None / Admin `400`) |
| `GET /workflow/request/search` · `/workflow/export` · `/workflow/forwarder/health_check` · `/workflow/forwarder/api/v1/flows/assigns/all` | **Cả hai** |
| `GET /workflow/sync-bo-workflows` | **Chỉ Admin** (End User: 403) |
| `GET /metadata/forwarder/table-list` · `rows/table/{id}` · `table-fields/{id}` · `opensearch/.../search` | **Cả hai** |
| `GET /metadata/forwarder/api/database/fields/table/{id}/` | **Chỉ Admin** (End User: None) |
| `GET /metadata/forwarder/database/list` · `partner-table-list` | **Chỉ Admin** (End User: 403) |

> ⚠️ **Kết luận cho service account:** luồng chính (`/request/*`, `PUT /workflow/request`, `/workflow/request/{id}/result/json`) **chỉ vai Tenant gọi được**. ⇒ **Service account tích hợp bắt buộc là vai Tenant (`roleId = 2`).** Vai Partner bị `403` trên toàn bộ `/request/*`.
>
> D3 §A1 ghi rõ giải pháp: **tạo một Tenant riêng cho tích hợp**, adapter cache token và refresh trước hạn; song song **xin IDP cấp service account thật**.

### 3.3 TTL token và refresh

- **Không có refresh token.** Tài liệu nguồn: *"Đăng nhập, lấy JWT. Không thấy refresh token trong response."* Gặp `401` thì **login lại**.
- **TTL token thật: chưa biết.** D3 §A1 ghi *"TTL chưa rõ"*. Bản build cũ giả định 30 phút. **Cần xác thực bằng gọi thật** — xem §13.
- Cache token trong entity `IdpSession` (1 dòng), re-login khi 401 hoặc khi quá ngưỡng cấu hình.
- `POST /auth/logout` giết token ngay kể cả còn hạn. INDEX.md ghi rõ cách test an toàn: *"Gọi bằng tài khoản Admin để không làm hỏng token End User đang dùng."*
- `tenantId` và `roleId` nằm trong token, không truyền qua tham số. Mọi truy vấn bị lọc theo tenant ⇒ **mỗi hệ tích hợp cần một service account riêng**.

**Đối chiếu D3 §1.4:** D3 quy định *"Claim Platform → IDP: Service token in `Authorization: Bearer`, issued per environment"*. Hiện trạng chỉ có login bằng tài khoản người dùng ⇒ **lệch, vá bằng ADAPTER** (một Tenant riêng cho tích hợp).

---

## 4. Các REST method cần dựng

### 4.1 Bảng đầy đủ

**Ưu tiên:** `P0` = phải dựng cho MVP1 · `P1` = dựng khi cần · `—` = biết để không gọi.

| # | Ưu tiên | Method | Path | Mục đích | Vai |
|---|---|---|---|---|---|
| 1 | **P0** | `POST` | `/auth/login` | Lấy JWT | Chỉ End User |
| 2 | **P0** | `GET` | `/user/me` | Xác nhận `roleId = 2` và `tenantId` | Cả hai |
| 3 | **P0** | `POST` | `/request/group` | Tạo nhóm cho một claim — để poll một lần | Chỉ End User |
| 4 | **P0** | `POST` | `/request` | Nộp chứng từ (multipart) | Chỉ End User |
| 5 | **P0** | `GET` | `/request/group/{groupId}/check-status` | Poll cả nhóm | Chỉ End User |
| 6 | **P0** | `GET` | `/request/{id}/check-status` | Poll một request — **khi `Completed` trả kèm luôn kết quả** | Chỉ End User |
| 7 | **P0** | `GET` | `/request/{id}/result` | Kết quả trích xuất có cấu trúc | Chỉ End User |
| 8 | **P0** | `GET` | `/request/{id}/raw-file` | Tải lại file gốc (ZIP) — ảnh cho màn thẩm định + đối chiếu `sha256` | Chỉ End User |
| 9 | **P0** | `PUT` | `/workflow/request` | Tạo yêu cầu đánh giá | Chỉ End User |
| 10 | **P0** | `GET` | `/workflow/run-workflow-request/{id}` | **Kích hoạt chạy flow — bắt buộc gọi vì `autoStart` không hoạt động** | Cả hai |
| 11 | **P0** | `GET` | `/workflow/request/{id}` | Poll trạng thái đánh giá tới `status = 3` | Chỉ End User |
| 12 | **P0** | `GET` | `/workflow/request/{id}/result/json` | Lấy 20 mục đánh giá dạng JSON | Chỉ End User |
| 13 | **P0** | `GET` | `/template/document-set` | Đọc `templates[]` của bộ chứng từ → dựng chuỗi CSV cho `templates` | Chỉ End User |
| 14 | **P0** | `GET` | `/metadata/opensearch/table/322/search` | Tra bệnh viện → `hospital_code` + `country` | Cả hai |
| 15 | P1 | `POST` | `/request/sync` | Nộp + chờ + trả kết quả trong **một** lời gọi (~16 s / 1 trang) | Chỉ End User |
| 16 | P1 | `GET` | `/request/{id}/get-ocr` | OCR thô — **nguồn duy nhất của bounding box** | Chỉ End User |
| 17 | P1 | `POST` | `/template/document-set` | Tạo bộ chứng từ có tên — hiện thực `templateSetCode` của D3 | Chỉ End User |
| 18 | P1 | `GET` | `/template/{id}` | Định nghĩa `fields[]` / `tableFields[]` của template — master data để map sang A4 §8 | Cả hai (5384) / End User (5383) |
| 19 | P1 | `GET` | `/template/compact` | 321 template, để dựng `IdpFieldMap` | Chỉ End User |
| 20 | P1 | `GET` | `/usage/current` | Cảnh báo trước khi hết quota (cron ngày) | Cả hai |
| 21 | P1 | `GET` | `/workflow/forwarder/health_check` | Giám sát tầng đánh giá | Cả hai |
| 22 | P1 | `POST` | `/request/merge-multiple` | Gộp **nhiều ảnh** (≤ 10) thành một request | Chỉ End User |
| 23 | P1 | `PATCH` | `/request/{id}/retry` | Chạy lại request đã lỗi | Chỉ End User |
| 24 | P1 | `PATCH` | `/request/{id}/cancel` | Huỷ request đang chạy | Chỉ End User |
| 25 | P1 | `POST` | `/request/compare` · `/request/compare-multiple` | **Công cụ đo độ chính xác so với ground-truth XLSX** — A4 §10 yêu cầu, có sẵn | Chỉ End User |
| 26 | P1 | `GET` | `/metadata/forwarder/table-list` + `/api/database/rows/table/{id}/` | Đồng bộ 5 bảng master data | Cả hai |
| 27 | P1 | `GET` | `/workflow/forwarder/api/v1/flows/assigns/all?target_tenant_id={id}` | Kiểm flow nào đã gán cho tenant (nhẹ, thay cho `/flows` 2,8 MB) | Cả hai |
| 28 | P1 | `GET` | `/partner/permissions` | Kiểm partner có bật `MBAL` và `Workflows` không — **nên gọi trước khi tích hợp** | Cả hai |
| 29 | — | `POST` | `/mbal/create-request` | Nộp theo `claim_type` + `document_type`. **Xem §5.4 — có dùng được, nhưng không phải đường chính của Claim MVP** | Chỉ End User |
| 30 | — | `POST` | `/request/from-url` | Nộp theo tham chiếu (D3 I-07 muốn cái này) — **ĐANG HỎNG**, xem §12 | End User `400` |
| 31 | ❌ **CẤM** | `PUT` | `/request/{id}/result` | Sửa kết quả gốc. **Cấm gọi ở tầng adapter** — xem §6.4 | Chỉ End User |
| 32 | ❌ **CẤM** | `POST` | `/tool/*` | `redis-reset`, `update-format-db`, `move-document-to-minio`, `force-update-workflow-status` — thao tác vận hành nguy hiểm | — |

### 4.2 `POST /request/group` — tạo nhóm

**Request** (verbatim):

```json
{}
```

**Response** — HTTP `201`:

```json
{
  "id": "4",
  "createdAt": "2026-09-03T04:56:00.735Z"
}
```

Một claim là một nhóm. `id` là số tăng dần theo tenant.

### 4.3 `POST /request` — nộp chứng từ

`multipart/form-data`. Field **bắt buộc**: `name`, `templates`, `document`. Field **tuỳ chọn**: `requestGroupId`, `separatorType`, `separatorValue`, `deleteFile`, `extraParams`.

**Request** (verbatim):

```json
{
  "name": "CLM-TEST-1788411360",
  "templates": "5445,5444,5443,5384,5127,5340,5299",
  "requestGroupId": "4",
  "document": "<file DP_DATA_case1.pdf, 45091 bytes, application/pdf>"
}
```

**Response** — HTTP `201`:

```json
{
  "pageCount": 1,
  "requestId": 21376
}
```

> **Mẹo từ tài liệu nguồn, trích verbatim:** *"nhét `claimNo` / `eventNo` vào `name` — đây là trường duy nhất mang được định danh của hệ thống claim sang IDP."*
>
> Đây cũng là câu trả lời cho **D3 §A2** (correlation id xuyên suốt): không header nào mang được, nên quy ước cố định trên `name` **là** cơ chế correlate. Ghi correlation id vào log adapter ở **cả hai chiều**.

**`extraParams`:** tài liệu nguồn ghi *"type đóng, chỉ 4 khoá MBAL"* (D3 §B5) ⇒ **không dùng `extraParams` làm đường correlate cho Claim MVP.** Trong mọi payload thật của tenant 176, `extraParams` trả `{}`. Giữ `context` của D3 I-07 (`episodeType`, `serviceDateFrom/To`, `expectedTypes`) **ở phía claim, không gửi sang IDP** — mất `context` chỉ làm IDP không thu hẹp được vùng tìm, không chặn gì.

### 4.4 `POST /request/sync` — nộp và nhận kết quả trong một lời gọi

Tài liệu nguồn: *"Đây có thể là endpoint phù hợp nhất cho Claim Platform. Nó gộp `POST /request` + poll + `GET /result` thành một lời gọi, và trả `confidence` ngay ở tầng ngoài. Đổi lại là phải chờ đồng bộ (~16 giây cho 1 trang), nên chỉ hợp khi số trang nhỏ."*

**Request** (verbatim):

```json
{
  "name": "SYNC-1788416655",
  "templates": "5384,5127,5299",
  "document": "<file DP_DATA_case1.pdf, 45091 bytes, application/pdf>"
}
```

**Response** — HTTP `201` (cắt bớt, phần đầu verbatim):

```json
{
  "id": "21503",
  "requestId": "21378",
  "confidence": "97.81",
  "result": {
    "result": {
      "errorCode": null,
      "minioPath": "f33c99bf-a9ff-43fb-9fda-3831dd2dbc0e.pdf",
      "minioBucket": "idp-gateway-bucket",
      "refTenantId": "176",
      "errorMessage": null,
      "refPartnerId": "78",
      "refRequestId": "21378",
      "processingTime": 16.57,
      "templates_data": [ … ]
    }
  }
}
```

⚠️ **`confidence` ở đây là chuỗi `"97.81"`, thang 0–100**, trong khi `fields[].c` là số thang `[0,1]`. Xem §7.

### 4.5 `GET /request/{id}/check-status`

Tài liệu nguồn: *"Tiện ích không có trong tài liệu nào: khi `requestStatus = Completed`, response mang cả kết quả trong trường `data`. Poll bằng endpoint này thì không cần gọi thêm `/result`."*

**Response** (verbatim, cắt bớt):

```json
{
  "requestId": 21376,
  "requestStatus": "Completed",
  "data": {
    "result": {
      "errorCode": null,
      "minioPath": "a1d86331-9c2d-4c43-99e9-3ad7185bdf5d.pdf",
      "minioBucket": "idp-gateway-bucket",
      "refTenantId": "176",
      "errorMessage": null,
      "refPartnerId": "78",
      "refRequestId": "21376",
      "processingTime": 16.14,
      "templates_data": [ … ]
    }
  }
}
```

### 4.6 `GET /request/group/{groupId}/check-status`

Endpoint chính để poll. **Response** (verbatim):

```json
[
  {
    "requestId": "21376",
    "requestStatus": "Processing"
  }
]
```

Enum `requestStatus` (verbatim): `Sending`(0) · `Processing`(1) · `Completed`(2) · `Failed`(3) · `Cancelling`(4) · `Cancelled`(5).

### 4.7 `GET /request/{id}/result`

Xem §6 cho schema đầy đủ.

### 4.8 `GET /request/{id}/raw-file`

**Response** (verbatim):

```json
{
  "__binary__": true,
  "content_type": "application/zip",
  "content_length": 42134,
  "bytes": 42134
}
```

Hai công dụng, trích verbatim: *"**thay thế cho deep link I-10** — hiện ảnh gốc cạnh giá trị đã trích xuất ngay trong console của mình; và **đối chiếu `sha256`** sau khi nộp, bù cho việc IDP không nhận hash lúc nộp."*

### 4.9 `PUT /workflow/request`

Xem §9.

### 4.10 `GET /metadata/opensearch/table/322/search`

Tham số: `searchTerm` (bắt buộc), `fields` (bắt buộc, tên cột), `size`, `from`.

*Gọi thật:* `/metadata/opensearch/table/322/search?searchTerm=N%E1%BB%99i%20ti%E1%BA%BFt&fields=hospital_name&size=5`

**Response** (verbatim, 2 phần tử đầu):

```json
{
  "total": 344,
  "results": [
    {
      "id": 2160,
      "score": 55.240772,
      "data": {
        "id": 2160,
        "hospital_name": "BỆNH VIỆN NỘI TIẾT",
        "hospital_code": "15113",
        "country": "Viet Nam"
      },
      "highlights": {
        "hospital_name": [
          "BỆNH VIỆN <mark>NỘI</mark> <mark>TIẾT</mark>"
        ]
      }
    },
    {
      "id": 251,
      "score": 51.542492,
      "data": {
        "id": 251,
        "hospital_name": "BỆNH VIỆN NỘI TIẾT TW",
        "hospital_code": "",
        "country": "Viet Nam"
      },
      "highlights": {
        "hospital_name": [
          "BỆNH VIỆN <mark>NỘI</mark> <mark>TIẾT</mark> TW"
        ]
      }
    }
  ]
}
```

> Đây là đường đóng gap **`facilityCountry` (Q-IDP-05)** của D3 §I-09 mà không cần IDP thay đổi gì: tên cơ sở trích ra từ chứng từ → tra bảng `322` → ra `hospital_code` và `country`.
>
> ⚠️ Lưu ý dữ liệu: bảng có **dòng trùng** (`id 2160` và `id 1912` cùng `hospital_name`, cùng `hospital_code 15113`) và **dòng thiếu mã** (`id 251` có `hospital_code: ""`). Adapter phải chọn theo `score` và **xử lý `hospital_code` rỗng thành `null`**, không được coi `""` là mã.
>
> Bảng `322` phải được **đánh index trước** khi search dùng được: `GET /metadata/opensearch/table/322/index/async`, kiểm bằng `/index/status`.

### 4.11 Master data — 5 bảng

`GET /metadata/forwarder/table-list` → 5 bảng, trích verbatim:

```json
{
  "tables": [
    { "id": 322, "name": "Hospital",            "order": 74, "database_id": 6, "data_sync": null },
    { "id": 321, "name": "Country",             "order": 73, "database_id": 6, "data_sync": null },
    { "id": 316, "name": "Surgery Code",        "order": 69, "database_id": 6, "data_sync": null },
    { "id": 314, "name": "MBAL Document Types", "order": 67, "database_id": 6, "data_sync": null },
    { "id": 313, "name": "MBAL Claim Types",    "order": 66, "database_id": 6, "data_sync": null }
  ],
  "pagination": { "page": 1, "page_size": 10, "total": 5 }
}
```

| Bảng | Số dòng | Cột | Dùng để |
|---|---|---|---|
| `322` Hospital | **13.328** | `hospital_name`, `hospital_code`, `country` | `facilityCountry` (Q-IDP-05), chuẩn hoá tên cơ sở |
| `321` Country | **242** | `country_name`, `country_code`, `country_iso3` | Loại trừ *"điều trị ngoài Việt Nam"* |
| `316` Surgery Code | **95** | `surgery_name`, `surgery_code` | Tra mã phẫu thuật |
| `314` MBAL Document Types | **10** | `Name` | Enum `document_type` |
| `313` MBAL Claim Types | **4** | `claim_type` | Enum `claim_type` |

**Đường lấy dữ liệu đúng** (verbatim): **không phải** `GET /metadata/forwarder/table-data/{id}` (endpoint đó nhận **VIEW ID** chứ không phải TABLE ID, nên gọi bằng table id luôn `400`), mà qua proxy chung:

```
GET /metadata/forwarder/api/database/rows/table/{tableId}/?user_field_names=true&size=200
GET /metadata/forwarder/api/database/fields/table/{tableId}/
```

Đồng bộ 5 bảng này về master data của mình định kỳ, **không gọi trực tiếp mỗi claim**.

---

## 5. Template và bộ chứng từ

### 5.1 Bảng templateId ↔ loại chứng từ ↔ `T1-xx` / `T2-xx`

Ba cột đầu là **dữ liệu thật đọc được từ API**. Cột `T1-xx / T2-xx` là **đề xuất của adapter, chưa được IDP xác nhận** — đây chính là **`Q-IDP-01`**, câu hỏi mà A4 §3.1 gọi là *"câu hỏi duy nhất chặn chứ không làm suy giảm"*. Đoán sai một dòng là xếp chứng từ vào loại nó không phải, và checklist coi như đã đủ.

| templateId | `code` | Tên template (verbatim) | `category` | `sub_document_type` | Đề xuất `T1-xx`/`T2-xx` — **cần IDP xác nhận** |
|---|---|---|---|---|---|
| `5125` | `Custom-T5125` | Phiếu thanh toán nội trú | `Giấy tờ` | *(chưa nạp)* | `T1-01` — 9 cột, có `Thành tiền`, `BHYT trả`, `BN trả` |
| `5126` | `Custom-T5126` | Phiếu mua thuốc | `MBAL` | *(chưa nạp)* | `T1-02` |
| `5127` | `Custom-T5127` | Giấy ra viện/Hospital discharge papers | `Chứng từ y tế` | **`CL05`** *(map id 15, có sẵn)* | **`T1-05` — Phase 2**, A4 §1.4 **loại khỏi Tier 1** |
| `5128` | `Custom-T5128` | Biên bản giám định y khoa | `MBAL` | *(chưa nạp)* | `T2-03` |
| `5129` | `Custom-T5129` | Other | — | *(chưa nạp)* | **fallback đã-phân-loại** — ≠ `unclassified` |
| `5130` | `Custom-T5130` | VNVision-Giấy chứng nhận phẫu thuật | — | *(chưa nạp)* | `T2-03` |
| `5133` | `Custom-T5133` | Đơn bổ sung cho yêu cầu giải quyết quyền lợi bảo hiểm | — | *(chưa nạp)* | ngoài bộ 8 |
| `5135` | `Custom-T5135` | VNVision-Bản tường trình | — | *(chưa nạp)* | `T2-04` |
| `5137` | `Custom-T5137` | Trích lục khai sinh | — | *(chưa nạp)* | `T2-01` |
| `5138` | `Custom-T5138` | Phiếu thanh toán ra viện | — | *(chưa nạp)* | `T1-01` |
| `5218` | — | Bảng kê chi phí khám chữa bệnh *(cùng tên `5383`, khác bộ cột)* | — | *(chưa nạp)* | **`T1-01`** — 7 cột |
| `5287` | — | **cần xác thực bằng gọi thật** | — | *(chưa nạp)* | — |
| `5291` | — | **cần xác thực bằng gọi thật** | — | *(chưa nạp)* | — |
| `5299` | — | **cần xác thực bằng gọi thật** | — | *(chưa nạp)* | — |
| `5312` | — | Biên lai thanh toán viện phí | — | *(chưa nạp)* | `T1-01` — 5 cột |
| `5340` | — | **cần xác thực bằng gọi thật** | — | *(chưa nạp)* | — |
| `5383` | `Custom-T5383` | Bảng kê chi phí khám chữa bệnh/List of medical expenses | `Insurance - MBAL` | *(chưa nạp)* | **`T1-01`** — 13 cột, **chứng từ quan trọng nhất** |
| `5384` | `Custom-T5384` | VNVision-Giấy ra viện/Hospital discharge papers | `Insurance - MBAL` | **`CL10`** *(map id 16, nạp 03/09)* | **`T1-05` — Phase 2** |
| `5385` | — | **cần xác thực bằng gọi thật** | — | *(chưa nạp)* | — |
| `5443` | `Custom-T5443` | Passport | `PERS - Personal documents` | *(chưa nạp)* | `T2-01` |
| `5444` | `Custom-T5444` | Sales Invoice | `OTHR - Sales` | *(chưa nạp)* | `T1-04` — nhưng D3 §I-07: **T1-04 là XML-only**, không có đường OCR |
| `5445` | `Custom-T5445` | Money transfer request | `BANK - Payment ` | *(chưa nạp)* | ngoài bộ 8 |

> ⚠️ **Hai điều phải nói thẳng.**
>
> 1. **`subDocumentType` chỉ có giá trị khi bảng `mbal/template-map` đã được nạp.** Trên tenant 176 hiện **mới có 2 dòng** (`5127→CL05`, `5384→CL10`). Mọi template còn lại trả `subDocumentType: null`. Đây là **việc cấu hình**, không phải phát triển — nhưng **phải làm trước demo**.
> 2. **Mâu thuẫn trong tài liệu nguồn, cần xác thực bằng gọi thật:** bản ghi `POST /mbal/template-map` nạp `5384 → CL10`, nhưng `GET /request/21383/result` (sau khi nạp) được mô tả là trả **`"CL05"`**. Nếu template khớp thật là `5127` thì hợp lý; nếu là `5384` thì mô tả sai. **Kiểm bằng cách đọc `templateId` trong `templates_data[]` của chính response đó.**
>
> **Vì `T1-04` là XML-only theo D3 §I-07 và A4 §1.5**, MVP1 settle trên **bảng kê chi phí** (`5383`) và chỉ mất quy tắc đối chiếu `R6`. Xin IDP xác nhận có parser XML hoá đơn không.

### 5.2 Document set — `templateSetCode` của D3 có tương ứng thật

**`GET /template/document-set`** — response verbatim:

```json
[
  {
    "id": "193",
    "name": "Chứng từ y tế _ CSYT",
    "description": "Bộ chứng từ y tế cho claim_type MEDICAL_CARE (chăm sóc y tế)",
    "templates": ["5445", "5444", "5443", "5384", "5127", "5340", "5299"],
    "userId": 203,
    "createdAt": "2026-08-24T10:13:26.676Z",
    "updatedAt": "2026-08-24T10:13:26.676Z",
    "deletedAt": null,
    "partnerId": "40"
  },
  {
    "id": "194",
    "name": "Claim assessment Demo",
    "description": "Claim assessment Demo",
    "templates": ["5127", "5383", "5312", "5299", "5291", "5385", "5287"],
    "userId": 203,
    "createdAt": "2026-08-28T03:51:05.573Z",
    "updatedAt": "2026-08-28T03:51:05.573Z",
    "deletedAt": null,
    "partnerId": "40"
  }
]
```

**`POST /template/document-set`** — đã chạy thật, tạo bộ `HEALTH_CLAIM_V1`. Request verbatim:

```json
{
  "name": "HEALTH_CLAIM_V1",
  "description": "Bộ chứng từ sức khoẻ MVP1 — tạo để kiểm chứng đường ánh xạ templateSetCode của D3 I-07",
  "templates": [5384, 5127, 5299, 5125, 5126, 5130, 5128]
}
```

Response — HTTP `201`:

```json
{
  "name": "HEALTH_CLAIM_V1",
  "description": "Bộ chứng từ sức khoẻ MVP1 — tạo để kiểm chứng đường ánh xạ templateSetCode của D3 I-07",
  "templates": [5384, 5127, 5299, 5125, 5126, 5130, 5128],
  "userId": "203",
  "partnerId": "40",
  "id": "195",
  "createdAt": "2026-09-03T07:09:57.829Z",
  "updatedAt": "2026-09-03T07:09:57.829Z",
  "deletedAt": null
}
```

⚠️ Chú ý: **request gửi `templates` là mảng số**, response cũng trả mảng số — nhưng `GET /template/document-set` trả **mảng chuỗi**. Adapter phải chịu được cả hai.

> ✅ **Đây là bằng chứng trực tiếp rằng `templateSetCode = "HEALTH_CLAIM_V1"` của D3 §I-07 hiện thực được bằng cơ chế sẵn có của IDP, không cần vendor thay đổi gì.**

⚠️ **Bộ `HEALTH_CLAIM_V1` (195) hiện KHÔNG chứa template bảng kê chi phí** (`5383`, `5218`, `5312`, `5138`). Với Claim MVP, **bảng kê chi phí là chứng từ mà mọi dòng chi phí đến từ đó** ⇒ **phải bổ sung `5383` vào bộ trước khi demo**, hoặc tạo bộ mới.

### 5.3 Cách truyền `templates`

1. Đọc `templates[]` của document set (`GET /template/document-set/{id}`).
2. `join(',')` → chuỗi, ví dụ `"5445,5444,5443,5384,5127,5340,5299"`.
3. Truyền vào field `templates` của multipart.

**Luôn nộp cả danh sách template của bộ, không bao giờ chỉ định một template theo lựa chọn của khách** (A4 §2: lựa chọn của khách hàng chỉ là gợi ý). Đã kiểm chứng: nộp 7 template, đúng 1 khớp.

⚠️ **Rủi ro đã ghi nhận ở tầng dữ liệu IDP:** `DOCUMENT_SETS.templates` là mảng jsonb, không phải quan hệ — xoá template để lại id mồ côi, bị loại **âm thầm** chỉ với `logger.warn`; và `getTemplates` lọc `tenantId = ? OR tenantId IS NULL`, template thiếu quyền cũng bị loại âm thầm. ⇒ **Adapter phải đếm số template yêu cầu vs số thực gửi và hiện chênh lệch.**

### 5.4 `POST /mbal/create-request` — đường tắt nghiệp vụ

Vẫn dùng được trên chuẩn Claim MVP: backend tự tra `mbal/docset-map` theo `(claim_type, document_type, partnerId)`, lấy `documentSet.templates` rồi gọi tiếp như `POST /request`.

Cấu hình đã chạy thật (verbatim):

```json
// POST /mbal/docset-map  →  201
{ "documentSetId": 195, "claim_type": "MEDICAL_ILLNESS", "document_type": "CL06" }
// response
{ "documentSetId": 195, "claim_type": "MEDICAL_ILLNESS", "document_type": "CL06", "partnerId": "40", "id": "78" }
```

```json
// POST /mbal/create-request  →  201
{ "name": "VERIFY-…", "claim_type": "MEDICAL_ILLNESS", "document_type": "CL06", "document": "<DP_DATA_case1.pdf>" }
// response
{ "pageCount": 1, "requestId": 21383 }
```

Ánh xạ hiện có trên tenant 176: `id 77` = `MEDICAL_CARE` + `CL10` → set `193`; `id 78` = `MEDICAL_ILLNESS` + `CL06` → set `195`.

Enum hợp lệ (verbatim từ master data):

- **`claim_type`** (bảng 313): `MEDICAL_CARE` · `MEDICAL_ILLNESS` · `MEDICAL_INJURY` · `MEDICAL_DEATH`
- **`document_type`** (bảng 314): `CL05` · `CL06` · `CL07` · `CL08` · `CL10` · `CL11` · `CL12` · `CL15` · `CL26` · `CLM_04`

> **Quyết định cho bản build:** dùng **`POST /request` với `templates` CSV** làm đường chính, vì Claim MVP điều khiển bộ chứng từ bằng `templateSetCode` của mình chứ không bằng từ vựng `claim_type`/`document_type` của MBAL. Giữ `/mbal/create-request` làm đường dự phòng khi cần demo "chỉ cần biết loại claim".
>
> Bẫy đã ghi nhận ở tầng DB IDP: **cặp `(claim_type, document_type)` KHÔNG unique** — `findOne` lấy bừa một dòng khi trùng; và `claim_type`/`document_type` là **varchar trần, không FK danh mục** — gõ sai vẫn lưu, lỗi chỉ lộ khi gọi thật. Đây là lý do nữa để không đặt luồng chính lên đường này.

---

## 6. Cấu trúc kết quả trích xuất

### 6.1 Schema đầy đủ

```jsonc
{
  "result": {
    "errorCode":      null,                    // ⚠️ đọc TRƯỚC khi coi là thành công
    "errorMessage":   null,
    "minioPath":      "a1d86331-9c2d-4c43-99e9-3ad7185bdf5d.pdf",
    "minioBucket":    "idp-gateway-bucket",
    "refTenantId":    "176",
    "refPartnerId":   "78",                    // ≠ partnerId 40 — không gian id khác
    "refRequestId":   "21376",
    "processingTime": 16.14,                   // giây
    "templates_data": [
      {
        "templateId":      5384,               // ⚠️ ĐỊNH DANH BẰNG SỐ NÀY
        "templateName":    "…",
        "template":        "Custom-T5384|VNVision-Giấy ra viện/Hospital discharge papers",
        "subDocumentType": "CL05",             // null nếu chưa nạp mbal/template-map
        "data": {
          "confidence": 98.90,                 // ⚠️ thang 0–100
          "fields": [
            {
              "k":           "Place Of Treatment",   // nhãn tiếng Việt / tên template — KHÔNG phải khoá máy
              "v":           "Benh vien Noi tiet Trung uong",
              "c":           0.9,                    // ⚠️ thang [0,1]
              "c_llm":       0,                      // ⚠️ LUÔN = 0, xem §7.1
              "match_w":     "Ho so: Nguyen Hoang Ha - Benh vien Noi tiet Trung uong - 03/2025",
              "page_number": 1
            }
          ],
          "tables": [ … ]
        }
      }
    ]
  }
}
```

**Payload thật, trích verbatim** (`GET /request/21376/result`, cắt bớt):

```json
{
  "result": {
    "errorCode": null,
    "minioPath": "a1d86331-9c2d-4c43-99e9-3ad7185bdf5d.pdf",
    "minioBucket": "idp-gateway-bucket",
    "refTenantId": "176",
    "errorMessage": null,
    "refPartnerId": "78",
    "refRequestId": "21376",
    "processingTime": 16.14,
    "templates_data": [
      {
        "data": {
          "fields": [
            { "c": 1, "k": "Số vào viện",       "v": "",                "c_llm": 0, "match_w": null,                "page_number": 1 },
            { "c": 1, "k": "Tên bệnh nhân",     "v": "NGUYỄN HOÀNG HÀ", "c_llm": 0, "match_w": "NGUYỄN HOÀNG HÀ",   "page_number": 1 },
            { "c": 1, "k": "Tuổi",              "v": "",                "c_llm": 0, "match_w": null,                "page_number": 1 },
            { "c": 1, "k": "Giới tính",         "v": "Nữ",              "c_llm": 0, "match_w": ":",                 "page_number": 1 },
            { "c": 1, "k": "Ngày/ tháng/ năm sinh", "v": "1973-10-09",  "c_llm": 0, "match_w": "1973-10-09",        "page_number": 1 },
            { "c": 1, "k": "Dân tộc",           "v": "",                "c_llm": 0, "match_w": null,                "page_number": 1 },
            { "c": 1, "k": "Nghề nghiệp",       "v": "",                "c_llm": 0, "match_w": null,                "page_number": 1 },
            { "c": 1, "k": "Số thẻ bảo hiểm y tế", "v": "<redacted>", "c_llm": 0, "match_w": ": <redacted>", "page_number": 1 },
            { "c": 1, "k": "Thời hạn hiệu lực của thẻ bảo hiểm y tế", "v": "", "c_llm": 0, "match_w": null, "page_number": 1 },
            { "c": 1, "k": "Địa chỉ",           "v": "",                "c_llm": 0, "match_w": null,                "page_number": 1 },
            { "c": 1, "k": "Admission Date",    "v": "20/03/2025",      "c_llm": 0, "match_w": "20/03/2025",        "page_number": 1 },
            { "c": 1, "k": "Discharge Date",    "v": "26/03/2025",      "c_llm": 0, "match_w": "20/03/2025",        "page_number": 1 },
            { "c": 1, "k": "Khoa phòng",        "v": "",                "c_llm": 0, "match_w": null,                "page_number": 1 },
            { "c": 1, "k": "Diagnosis Codes",   "v": "",                "c_llm": 0, "match_w": null,                "page_number": 1 }
          ]
        }
      }
    ]
  }
}
```

> Quan sát trực tiếp từ payload này: **`Discharge Date` = `26/03/2025` nhưng `match_w` = `20/03/2025`** — giá trị field **không khớp** từ OCR mà nó tự nhận là đã khớp, và vẫn mang `c: 1`. Đây là bằng chứng cụ thể nhất cho §7.3.

### 6.2 Ba việc bắt buộc trong adapter `IdpParseExtraction`

**1. Bóc tiền tố của `templateName` nếu có.**
Định dạng quan sát được ở tenant 176: `code` là `Custom-T5384`, `name` là tên thật. Chuỗi hợp nhất có dạng `Custom-T<id>|<Tên thật>`, và trong `GET /template/compact` là `Custom-T5445 - Money transfer request`. Bản MBAL còn có dạng `SET<n>_Custom-T<id>|<Tên thật>`.

```
Regex an toàn cho cả ba dạng:
^(?:SET\d+_)?Custom-T\d+\s*(?:\||-)\s*(.+)$
```

**2. Định danh bằng `templateId` (số), không bao giờ bằng tên.**
Back Office đổi tên template là vỡ. Tên còn không nhất quán trong cùng một hệ: `5127` = *"Giấy ra viện/Hospital discharge papers"*, `5384` = *"VNVision-Giấy ra viện/Hospital discharge papers"* — cùng loại chứng từ, hai tên. Và `5383` với `5218` **cùng tên** *"Bảng kê chi phí khám chữa bệnh"* nhưng **13 cột vs 7 cột**.

**3. Chuẩn hoá rỗng.**
`""`, `"None"`, `"null"`, `"00000"` → `null`, và **loại khỏi phép tính confidence trung bình** (§7.3).

D3 §1.2 quy định: *"`null` means not known. An empty string means known to be empty. They are different."* A4 §2 còn chặt hơn: *"a missing value is written as `null`, **never** as an empty string, and never guessed."* IDP trả **chuỗi rỗng** cho field không đọc được ⇒ **chuẩn hoá ngay ở normalizer, một dòng code**.

Giá trị `"00000"` là giá trị **giữ chỗ** thật quan sát được: `HospitalCode` trả `"00000"` dù bệnh viện có thật trong danh mục; `PlaceOfTreatment` trả chuỗi `"null"` (bốn ký tự, không phải null JSON).

### 6.3 Bốn việc chuẩn hoá thêm (từ D3 / A4)

| Việc | Vì sao |
|---|---|
| **Ngày** — nhận cả `dd/mm/yyyy`, `dd-mm-yyyy`, `ngày DD tháng MM năm YYYY` → `YYYY-MM-DD` | Payload thật **không nhất quán**: `"1973-10-09"` đúng chuẩn, `"20/03/2025"` thì không |
| **Tiền** — bỏ dấu `.` phân cách nghìn, ép về **số nguyên VND** | D3 §1.2: *"Integer VND. No decimals, no separators, no floating point, ever."* Các cột tiền trong `tableFields` đều có `type: null` ⇒ **template không ép kiểu**, phải phòng cả trường hợp trả chuỗi có dấu phân cách |
| **Confidence cấp claim** — **tự tính `min` trên bộ field bắt buộc**, bỏ hẳn số IDP trả | A4 §7 và D3 §I-09: confidence mức claim là **nhỏ nhất**, không phải trung bình. IDP tính **trung bình** |
| **Completeness tách rời confidence** | **12 trong 21 field rỗng vẫn mang `c: 1`.** Lấy min ra `0.87` cho một chứng từ thiếu quá nửa số trường. Hàng đợi thẩm định xếp theo **cả hai chỉ số** |

### 6.4 `PUT /request/{id}/result` — CẤM gọi

Đã chạy thật, trả `200` với `affected: 1`:

```json
// Request
{ "index": 0, "fields": [ { "c": 1, "k": "Tên bệnh nhân", "v": "NGUYỄN HOÀNG HÀ (đã sửa tay)", "match_w": "", "page_number": 1 } ], "tables": [] }
// Response
{ "generatedMaps": [], "raw": [], "affected": 1 }
```

⇒ **Kết quả trích xuất của IDP KHÔNG bất biến.** D3 §I-09 dựa vào tính bất biến để so raw-versus-corrected và để tái lập một claim đã settle.

**Hai hệ quả bắt buộc:**
1. **Copy kết quả sang store của mình ngay lần đọc đầu.** Ghi `RawExtractionJson` **một lần, không bao giờ update**. Mọi audit đọc bản copy.
2. **Cấm gọi `PUT /request/{id}/result`** ở tầng adapter. Mọi chỉnh sửa của thẩm định viên ghi vào **bản copy**, giữ cặp giá trị *máy đọc* và *người sửa* để so lâu dài.

---

## 7. Confidence

Nội dung kỹ thuật giữ nguyên từ bản trước (cùng engine `llm-du`), cập nhật tên endpoint và ví dụ theo chuẩn Claim MVP. Đây là phần dễ đặt ngưỡng sai nhất.

### 7.1 `c_llm` hiện **luôn bằng 0**

```
process_llm_du.py — mọi call site truyền logprobs=False
   (dòng 458, 610, 745, 843, 1268, 1524, 1563)
→ conf_list = []  →  word_lookup = {}  →  c_llm = 0.0
→ combine_ocr_du_confidence_scores_fields:  if conf_c_llm <= 0: continue
→ c cuối cùng = confidence OCR thuần + word-count penalty
```

**Xác nhận bằng dữ liệu Claim MVP:** trong **mọi** field của **mọi** payload thật ngày 03/09/2026 (`/request/sync`, `/request/21376/result`, `/request/21383/result`, `/request/21376/check-status`), `"c_llm": 0` — không có ngoại lệ.

Muốn có `c_llm` thật thì phải bật `logprobs=True` — OpenAI hỗ trợ, Gemini không (code tự tắt). `engineType` của các lần chạy này là **`azure`**.

### 7.2 `c` được tính thế nào

Khớp giá trị field với từ OCR, theo thang ưu tiên:

| Ưu tiên | Điều kiện | `c` |
|---|---|---|
| 1 | exact match, Levenshtein ≤ 2 | giữ nguyên confidence OCR, **không phạt** |
| 2 | substring match | `conf − penalty` |
| 3 | fuzzy, similarity ≥ 0.8 | `conf − penalty` |
| 4 | fuzzy, similarity ≥ 0.7 | `conf − penalty` |
| 5 | fuzzy, similarity ≥ 0.5 | `conf − penalty` |
| — | không match | `c = -1`, bị loại khỏi mean |

**Word-count penalty:**
```
ratio = min(field_words, match_words) / max(field_words, match_words)
ratio ≥ 0.8 → 0.00      ratio ≥ 0.6 → 0.05      ratio ≥ 0.4 → 0.10
ratio ≥ 0.2 → 0.20      ratio < 0.2 → 0.30      (một bên = 0 → 0.30)
```

Ví dụ thật khớp công thức này: field `Place Of Treatment` có `v` = `"Benh vien Noi tiet Trung uong"` (5 từ) và `match_w` = `"Ho so: Nguyen Hoang Ha - Benh vien Noi tiet Trung uong - 03/2025"` (14 từ) ⇒ `ratio = 5/14 ≈ 0.36` ⇒ penalty `0.10` ⇒ **`c = 0.9`**. Đúng bằng giá trị trong payload.

**Field có giá trị nhưng `match_w` là null:**
```
normalize_conf = 0.8 − (số ký tự × 0.005)
c = max(normalize_conf, c_hiện_tại − 0.05)
```

**Field có cả `c` < 0.2 và `c_llm` < 0.2** → hỏi lại LLM bằng prompt riêng, gán thẳng điểm LLM trả về (clamp `[0.51, 0.99]`); lỗi → fallback `0.7`.

### 7.3 Hai quirk thổi phồng confidence trung bình

| Quirk | Hệ quả |
|---|---|
| **Field rỗng → `c = 1`** | Template nhiều field không xuất hiện trên giấy sẽ kéo mean lên cao giả |
| **Ô bảng rỗng hoặc `---` → `0.95`** | Bảng kê thưa trông như đọc rất tốt |

**Bằng chứng trực tiếp từ payload Claim MVP:** trong `GET /request/21376/result`, 8 trong 14 field đầu tiên có `v: ""` và **tất cả đều mang `c: 1`**. `data.confidence` của template này là **`98.90`** — một con số sẽ lọt qua bất kỳ ngưỡng tự duyệt nào, cho một chứng từ mà hơn nửa số trường không đọc được gì.

⇒ **Trước khi so ngưỡng, loại mọi field có `v` rỗng (sau chuẩn hoá §6.2) ra khỏi phép tính.** Nếu không, cổng tự duyệt sẽ pass những hồ sơ nó không nên pass.

### 7.4 Confidence cấp document

```
max = max(field_conf, table_conf); min = min(...)
max == 0 → 0;  min == 0 → max;  ngược lại → (max + min) / 2
```

### 7.5 Hai thang đo trong cùng một payload

| Trường | Thang | Ví dụ thật |
|---|---|---|
| `fields[].c` · `fields[].c_llm` | `[0, 1]`, số | `0.9`, `1` |
| `data.confidence` (mức template) | `0–100`, số | `98.90` |
| `confidence` (mức request, ở `/request/sync` và `results[]`) | `0–100`, **chuỗi** | `"97.81"`, `"98.90"` |

⇒ Adapter **chia mức template/request cho 100**, giữ mức field nguyên, và **ép kiểu chuỗi → số**.

### 7.6 Kết luận cho cổng tự duyệt

Ngưỡng `MinClaimConfidence 0.90` / `LineConfidenceFloor 0.80` là **do BnK đặt**. Với `c_llm ≡ 0` và hai quirk trên, con số đang chấm là **OCR-confidence có bias**.

**Bốn việc trước khi chốt ngưỡng:**

1. **Loại field rỗng khỏi mean** (bắt buộc).
2. **Tự tính `min` trên bộ field bắt buộc**, không dùng `result.confidence` của IDP (A4 §7, D3 §I-09).
3. **Tách completeness khỏi confidence** và xếp hàng đợi theo cả hai (D3 §D5).
4. Chạy thử 10–20 chứng từ thật, xem phân bố `c` rồi mới đặt ngưỡng.

> **Cân nhắc:** MBAL thiết kế ngưỡng **theo từng loại chứng từ và từng trường** (khai lúc tạo template), ví dụ *"Họ tên" ≥ 0,95 · "Chẩn đoán" ≥ 0,90*. Cách này hợp lý hơn một `MinClaimConfidence` đơn nhất.

---

## 8. Bounding box

Field output chỉ có `k`, `v`, `c`, `c_llm`, `match_w`, `page_number` — **không có toạ độ**. Nhưng **tầng OCR thì có**:

- **Azure Document Intelligence**: box cấp **line** (PDF), cấp **word** (ảnh)
- **Google Document AI**: `layout.bounding_poly.vertices`
- **Bảng**: `"box": [x1, y1, x2, y2]`
- **QR**: `points` + `bbox_xyxy`

Truy cập bằng **`GET /request/{id}/get-ocr`**. Response thật (verbatim, cắt bớt):

```json
{
  "refPartnerId": "78",
  "refRequestId": "21376",
  "engineType": "azure",
  "ocrResult": {
    "tables": "[{'page_number': 1, 'box': [1.0269, 1.9678, 7.2629, 6.0961], 'data': …}]",
    "document": "\n-----BEGINING OF PAGE 1-----\nDU LIEU KHACH HANG KHAI\n…\n-----END OF PAGE 1-----\n",
    "document2d": "\n-----BEGINING OF PAGE 1-----\n                     DU LIEU KHACH HANG KHAI\n…"
  }
}
```

⚠️ **Hai bẫy ở endpoint này:**

1. **`ocrResult.tables` là chuỗi Python repr (nháy đơn), không parse JSON được.** Phải chuyển `'` → `"` cẩn thận hoặc parse bằng thư viện riêng.
2. `box` ở đây là `[1.0269, 1.9678, 7.2629, 6.0961]` — **đơn vị inch**, không phải pixel (Azure trả toạ độ theo inch). Phải nhân với DPI khi vẽ lên ảnh.

**Kết luận:** màn side-by-side highlight vị trí **làm được** — join raw OCR theo `match_w` + `page_number`. Không phải chờ IDP bổ sung gì.

**Nhưng đó là neo một chiều có giảm cấp**, không phải neo hai chiều mà D2 §5.6.1 yêu cầu. D3 §D6 và §I2 nói rõ: `documentId` **không tồn tại**, `bbox` ở endpoint khác, khớp lại theo thứ tự thì **mong manh**. ⇒ **Xin IDP trả `documentId` và `bbox` ngay trong `/result`.** Trước mắt:

- **Giảm cấp xuống neo theo trang**: chọn field thì nhảy đúng trang. Ghi vào D2 là **tính năng giảm cấp có chủ ý**.
- Nếu muốn highlight vùng: join `match_w` với `words`/`lines` của raw OCR, **chấp nhận sai khi `match_w` trùng lặp trên trang**.

⚠️ **Ràng buộc kiến trúc từ MBAL vẫn đúng ở đây:** `ocr-service` gọi `minioService.deleteFile()` ngay trong `handleOCRResult`. ⇒ **Phải gọi `raw-file` ngay trong bước Intake**, không lazy-load lúc mở màn thẩm định. **Cần xác thực bằng gọi thật** trên tenant 176: `raw-file` của `21376` vẫn trả `200` với 42.134 bytes sau khi đã Completed — nên hành vi có thể đã khác, nhưng **đừng dựa vào đó**.

---

## 9. Luồng đánh giá / clinical assessment

Toàn bộ đánh giá nằm dưới `/workflow`. **Không tồn tại API đánh giá độc lập** — D3 §H1 gọi đây là **quyết định cần sếp chốt**. Coi `PUT /workflow/request` là một REST endpoint bình thường và **không đưa logic nghiệp vụ vào flow**.

### 9.1 `workflowId` — ghim đúng flow, đây là việc cấu hình bắt buộc

| `workflowId` | Tên | Kết quả `/result/json` |
|---|---|---|
| **`5ed8e4fc-27a5-45a7-b104-1216fcfd0797`** | **"Workflow Claim 28/8"** — *"bỏ phần convert markdown cuối cùng"* | **4/4 lần trả JSON đầy đủ** (4.171 · 8.369 · 8.618 ký tự) — ✅ **DÙNG FLOW NÀY** |
| `8d873709-4cac-4f26-82f1-b1f9d28c6e73` | "Workflow Claim 25/08" | **0/4** — luôn trả `{}` |
| `7b649d06-…` | — | 0/1 |

> Tài liệu nguồn, verbatim: *"Nghĩa là **cái quyết định là chọn flow nào, không phải endpoint hỏng**."*

`workflowId` **khác nhau giữa STG và PRD** ⇒ để trong Site Setting `IdpWorkflowId`, và **từ chối mọi `workflowId` lạ** (D3 §H4: ghim `workflowId` làm phiên bản tri thức; xin IDP cam kết *sửa flow là bump `workflowId`*).

Kiểm flow nào đã gán cho tenant bằng `GET /workflow/forwarder/api/v1/flows/assigns/all?target_tenant_id=176` (nhẹ), thay vì `GET /workflow/forwarder/api/v1/flows` (**~2,8 MB** vì kèm toàn bộ đồ thị node — **cache, đừng gọi mỗi claim**).

Flow `5ed8e4fc` có **154 node**, trong đó có sẵn các bộ so sánh chuyên dụng (verbatim): `Insured Name Comparator` · `DOB Comparator` · `Gender Comparator` · `Identification number Comparator` · `Admission Date Comparator` · `Discharge Date Comparator` · `Death Date Comparator` · `Event Date Comparator` · `Diagnosis Codes Comparator` · `Place Of Treatment Comparator` · `Metatable Find Matched Hospital` · `LLM Find Surgery Code`.

### 9.2 `PUT /workflow/request` — tạo yêu cầu đánh giá

**Request thật** (verbatim):

```json
{
  "ocrRequestIds": ["21376"],
  "workflowId": "8d873709-4cac-4f26-82f1-b1f9d28c6e73",
  "name": "ASSESS-B-1788411746",
  "autoStart": "true"
}
```

**Response** — HTTP `200`:

```json
{
  "workflowRequestId": "1403"
}
```

`ocrRequestIds`, `workflowId`, `name` là **bắt buộc**. `extraInputData[] { template, data }` là **chỗ duy nhất nhét thêm dữ liệu** vào flow.

> ⚠️ **Ba điều đã kiểm chứng** (trích verbatim): *"`autoStart: "true"` **không có tác dụng**, bản ghi lưu `false` và trạng thái đứng ở `1` · khoá trả về tên `workflowRequestId` nhưng khi `GET` thì bản ghi lại có `id` và một `workflowRequestId: null` riêng · **nhận cả OCR request đang `Failed`**, không kiểm tra gì lúc tạo."*

**Ba hệ quả cho adapter:**
1. **Luôn gọi `GET /workflow/run-workflow-request/{id}` sau khi tạo.** Không trông vào `autoStart`.
2. **Đọc `id` chứ không đọc `workflowRequestId`** khi `GET /workflow/request/{id}` — hai khoá khác nhau, `workflowRequestId` trả `null`.
3. **Tự kiểm mọi OCR request đã `Completed` (`status = 2`) trước khi tạo phiên đánh giá.** IDP không kiểm.

### 9.3 Bơm dữ liệu ground truth — `extraInputData`

D3 §I-15 muốn gửi `clinicalContext` (`icd10`, `episodeType`, `ageBand`, `sex`) và `lines[]` kèm `serviceCategory`. **IDP không có chỗ nhận trực tiếp.** Đường duy nhất là `extraInputData`:

```jsonc
{
  "ocrRequestIds": ["21376", "21377", "21378"],
  "workflowId": "5ed8e4fc-27a5-45a7-b104-1216fcfd0797",
  "name": "CLM-26-000417",
  "extraInputData": [{
    "template": "Dữ liệu từ DP",
    "data": {
      "fields": [
        { "k": "Insured Name", "v": "NGUYỄN HOÀNG HÀ" },
        { "k": "DOB",          "v": "1973-10-09" }
      ],
      "tables": []
    }
  }]
}
```

**Bốn ràng buộc:**

1. **Tên template phải đúng** `Dữ liệu từ DP` hoặc `MBAL_DP_DATA`. Flow nhận diện đúng hai tên này rồi tách ra làm **mốc đối chiếu**, không xếp vào nhóm chứng từ bị so khớp. **Sai tên → dữ liệu khai bị đem đi so như một chứng từ.**
2. **Validate:** `k` 1–512 · `v` 0–512 · `name` 1–512 · `workflowId` 1–64 · `tables.data` phải là mảng 2 chiều **toàn string** ≤ 512 ký tự.
3. D3 §H2 đề xuất gói payload I-15 vào một template ảo **`CLINICAL_CONTEXT_V1`** — nhưng **không có gì ràng buộc flow đọc đúng template ảo đó**. **Cần xin IDP cam kết** trước khi dựa vào.
4. **`lines[]` không bơm được** (D3 §H3): flow đọc thẳng kết quả OCR, không nhận dòng chi phí. ⇒ **không vá được ở phía mình**, và mọi khuyến nghị hiện chỉ ở **mức claim**, không theo dòng.

> **Bằng chứng flow đã đọc được dữ liệu DP:** chính bộ chứng từ test là file `DP_DATA_case1.pdf` — một PDF mô phỏng dữ liệu Data Platform, và `GET /request/21376/get-ocr` cho thấy nội dung của nó (`Claim Type`, `Idenification number`, `Insured Name`, `Gender`, `DOB`, `Admission Date`, `Discharge Date`, `Event Date`, `Document type`, `Death Date`). Flow có node `So sánh với DP` đối chiếu `ho_va_ten` · `gender` · `dob` với `ho_va_ten_dp` · `gender_dp` · `dob_dp`.
>
> ⚠️ Nghĩa là **cách bơm ground truth hiện đang chạy thật là "in thành PDF rồi nộp như một chứng từ"**, không phải qua `extraInputData`. Với demo hackathon đây là đường **đã chứng minh hoạt động**; `extraInputData` là đường sạch hơn nhưng **cần xác thực bằng gọi thật**.

### 9.4 Kích hoạt — `GET /workflow/run-workflow-request/{id}`

**Bắt buộc phải gọi vì `autoStart` không hoạt động.**

⚠️ **Hành vi không ổn định**, trích verbatim: *"Cùng flow `5ed8e4fc-…`, cùng OCR request `21376`, cùng tham số: lần đầu trả lỗi dưới đây, lần sau trả `200` và chạy tới cùng sau ~30 giây. Adapter phải **thử lại** thay vì kết luận flow hỏng ngay lần đầu."*

Lỗi của lần đầu (verbatim):

```json
{ "statusCode": 400, "message": "Unexpected token '\"', \"#\" is not valid JSON", "error": "Bad Request" }
```

*"Lỗi phát sinh khi FO dựng tham số cho flow, và **không nói component nào hỏng**."*

⇒ **Đây là ngoại lệ với quy tắc "`4xx` không bao giờ retry" của D3 §1.3.** Ghi rõ trong code: `400` trên riêng endpoint này **được retry** (khuyến nghị 3 lần, cách nhau 5 s).

### 9.5 Trạng thái

Enum `status` (verbatim): `1` `WaitingForInput` · `7` `InQueue` · `2` `Running` · `3` `Finished` · `4` `Error` · `5` `NotFound` · `6` `Unknown`.

Poll bằng `GET /workflow/request/{id}` tới `status = 3`. **Ba trường quyết định, phần còn lại là nhiễu** (verbatim):

| Trường | Ý nghĩa |
|---|---|
| `status` | `3` = chạy xong. Đây là thứ để poll |
| `result.success` | `true` = flow chạy thành công |
| `result.result` | Nội dung đánh giá thật, dạng chuỗi |

> **`result.logs` không phải lỗi.** Tài liệu nguồn: *"Nó là stderr của lần chạy flow, và trong bản ghi này toàn bộ 3.200 ký tự chỉ là `UserWarning` của thư viện Langflow về tham số `top_p` — không có một dòng `Error` hay `Traceback` nào."*

Bản ghi thật (verbatim, rút gọn):

```json
{
  "id": "1404",
  "name": "RETRY-FLOWA-1788420635",
  "workflowRequestId": null,
  "workflowId": "5ed8e4fc-27a5-45a7-b104-1216fcfd0797",
  "tenantId": "176",
  "status": 3,
  "result": {
    "logs": "… UserWarning: Parameters {'top_p'} should be specified explicitly … [3200 ký tự, không phải lỗi]",
    "type": "message",
    "result": "```JSON\n[\n  {\n    \"instruction_name\": \"Kiểm tra Danh mục bệnh viện\", …"
  },
  "resultTransactions": [],
  "autoStart": false,
  "extraInputData": [],
  "retries": 0,
  "version": 2,
  "ocrRequests": [ … ]
}
```

### 9.6 Cấu trúc kết quả — `GET /workflow/request/{id}/result/json`

**Cấu trúc mỗi mục** (verbatim): `instruction_name` · `instruction_type` (`Cảnh báo` | `Trích xuất field`) · `warning_level` (`0` / `0.5` / `1`, **chỉ có ở loại Cảnh báo**) · `fieldname` + `field_validation` (**chỉ có ở loại Trích xuất field**) · `result` (văn bản tiếng Việt).

Lần chạy 03/09: **10 mục `Cảnh báo`** và **10 mục `Trích xuất field`** = 20 mục.

**Response thật** (verbatim, cắt bớt):

```json
{
  "data": [
    {
      "instruction_name": "Kiểm tra Danh mục bệnh viện",
      "instruction_type": "Cảnh báo",
      "warning_level": 1,
      "result": "VNVision-Giấy ra viện/Hospital discharge papers: Bệnh viện Nội tiết Trung ương. Bệnh viện Nội tiết Trung ương Đây là bệnh viện mới"
    },
    {
      "instruction_name": "Tìm kiếm HospitalCode",
      "instruction_type": "Trích xuất field",
      "fieldname": "HospitalCode",
      "field_validation": true,
      "result": "00000"
    },
    {
      "instruction_name": "Tìm kiếm PlaceOfTreatment",
      "instruction_type": "Trích xuất field",
      "fieldname": "PlaceOfTreatment",
      "field_validation": true,
      "result": "null"
    },
    {
      "instruction_name": "Kiểm tra Ngày tử vong",
      "instruction_type": "Cảnh báo",
      "warning_level": 0,
      "result": "Không tìm thấy thông tin ngày tử vong trên các chứng từ."
    },
    {
      "instruction_name": "Kiểm tra Ngày vào viện",
      "instruction_type": "Cảnh báo",
      "warning_level": 0,
      "result": "VNVision-Giấy ra viện/Hospital discharge papers: 20/03/2025\nThông tin ngày vào viện đồng nhất."
    },
    {
      "instruction_name": "Kiểm tra Ngày xuất viện",
      "instruction_type": "Cảnh báo",
      "warning_level": 0,
      "result": "VNVision-Giấy ra viện/Hospital discharge papers: 26/03/2025\nThông tin ngày xuất viện đồng nhất."
    },
    {
      "instruction_name": "Kiểm tra Giấy tờ tùy thân",
      "instruction_type": "Cảnh báo",
      "warning_level": 1,
      "result": "VNVision-Giấy ra viện/Hospital discharge papers: Không có thông tin giấy tờ tùy thân.\n\nCảnh báo: Không thể đối soát do thiếu thông tin giấy tờ tùy thân trên chứng từ."
    },
    {
      "instruction_name": "Kiểm tra Ngày xảy ra sự kiện bảo hiểm",
      "instruction_type": "Cảnh báo",
      "warning_level": 1,
      "result": "VNVision-Giấy ra viện/Hospital discharge papers: EventDate (Admission Date) là 20/03/2025.\n\nCảnh báo: Ngày xảy ra sự kiện bảo hiểm (20/03/2025) thuộc về tương lai so với thời điểm hiện tại."
    },
    {
      "instruction_name": "Kiểm tra Mã phẫu thuật từ metadata",
      "instruction_type": "Cảnh báo",
      "warning_level": 0,
      "result": "Không có cảnh báo"
    },
    {
      "instruction_name": "Cảnh báo tài liệu không thể phân loại",
      "instruction_type": "Cảnh báo",
      "warning_level": 0,
      "result": "Tài liệu phân loại thành công."
    },
    {
      "instruction_name": "Tìm kiếm DeathDate",
      "instruction_type": "Trích xuất field",
      "fieldname": "deathDate",
      "field_validation": true,
      "result": null
    },
    {
      "instruction_name": "Tìm kiếm AdmissionDate",
      "instruction_type": "Trích xuất field",
      "fieldname": "AdmissionDate",
      "field_validation": true,
      "result": "20/03/2025"
    },
    {
      "instruction_name": "Tìm kiếm DischargeDate",
      "instruction_type": "Trích xuất field",
      "field_validation": true,
      "result": "…"
    }
  ]
}
```

**`fieldname` thực tế** (10 giá trị, **giữ nguyên sai chính tả — đó là giá trị thật**):
`HospitalCode` · `PlaceOfTreatment` · `deathDate` · `AdmissionDate` · `DischargeDate` · `DiagnosiCodes` · `CongenitalIndi` · `SurgeryCodes` · `DiagnosisDate` · `KhámChữaĐôngY`

Cùng loại sai chính tả trong nguồn dữ liệu: `Idenification number` (thiếu `t`) · `"Event Date "` (thừa dấu cách cuối).

### 9.7 Cảnh báo — bốn điều phải biết trước khi dùng

**1. `{}` phải luôn bị coi là HỎNG.**
Trích verbatim: *"Chạy với flow `8d873709-…`: đánh giá **chạy xong, `success: true`, sinh 3.778 ký tự nội dung** — endpoint vẫn trả `{}` kèm **HTTP 200**. Nguyên nhân: flow đó trả markdown chứ không trả khối ```json. Service tìm khối JSON, không thấy, thử parse cả chuỗi, hỏng, rồi **nuốt lỗi và trả object rỗng**."*

**Ba tình huống, ba hành vi khác nhau:**

| Tình huống | Trả về |
|---|---|
| Flow đúng, đã xong | `200` + `{ "data": [ … 20 mục … ] }` |
| Flow sai (xuất markdown) | **`200` + `{}`** ← **coi là HỎNG**, không bao giờ là *"không có khuyến nghị"* |
| Chưa có kết quả | `400` + `{ "detail": "Bad Workflow result, something is wrong.", "statusCode": 4007, "message": "Workflow result is bad, please check again." }` |

**2. `warning_level` là **mức nghiêm trọng**, không phải độ tin cậy.**
D3 §H9 cho phép map tạm để dựng panel: `1` → `HIGH` · `0.5` → `MEDIUM` · `0` → `LOW`, **và ghi rõ trên UI rằng đây là mức nghiêm trọng, không phải độ tin cậy**.

⚠️ `warning_level` **trung bình cộng** các nhận xét con ⇒ một lỗi nặng lẫn giữa nhiều dòng bình thường sẽ ra điểm thấp. Đo trên bộ demo: hồ sơ sạch TB **0,555** (4 cảnh báo mức 1.0), hồ sơ 3 lỗi TB **0,728** — cách nhau **0,17**, **không đủ đặt ngưỡng tự động**.

**3. Đầu ra hiện tại KHÔNG phải hợp đồng I-16.**

| D3 §I-16 yêu cầu | Hiện trạng |
|---|---|
| `proposedReasonCode` chỉ `NOTREL` \| `NOTCOV` | ❌ Chỉ có `Cảnh báo` và `Trích xuất field` — **gần R1–R5 của Track 1 hơn Track 2** |
| `evidence[]` **bắt buộc** trên mọi khuyến nghị | ❌ Không có. D3 §H8: *"không thương lượng được"* — **mất một trong ba chốt kiểm soát mà D1 RK-08 dựa vào** |
| Khuyến nghị gắn với **một dòng chi phí** | ❌ Chỉ ở mức claim |
| `linesAssessed[]` | ❌ Không có ⇒ **không được coi dòng nào là đã đánh giá sạch** |
| `confidence` + band `HIGH`/`MEDIUM`/`LOW` | ⚠️ Chỉ có `warning_level` |

⇒ **Nếu IDP không trả được `evidence[]`, Track 2 phải chạy ở chế độ advisory và ghi rõ.** Dựng panel F56 **rút gọn** (chỉ diễn giải + mức nghiêm trọng), đánh dấu rõ là chưa đủ spec.

**4. Kết quả đánh giá không bao giờ tự động vào sổ.**
D1 `BR-TRACK-3/4`, D3 G-7: khuyến nghị chỉ ghi vào **store riêng**; chỉ khi thẩm định viên xác nhận thì mới chạy lại chuỗi quy tắc.

**5. Hai giá trị giữ chỗ cần xử lý:** `HospitalCode` trả `"00000"` dù bệnh viện có thật trong danh mục; `PlaceOfTreatment` trả chuỗi `"null"`. ⇒ Đừng dùng đầu ra của flow cho hai trường này — **tra thẳng bằng `/metadata/opensearch/table/322/search`** (§4.10) thì ra đúng `hospital_code`.

**6. Yêu cầu flow xuất JSON thuần.** Nếu node cuối là LLM convert JSON→markdown, backend phải bóc bằng regex ` ```json … ``` `; bóc không được sẽ ném `WorkflowBadResult` / `4007`.

---

## 10. Nhận kết quả — polling, socket, webhook

| Cách | Cơ chế | Đánh giá |
|---|---|---|
| **Polling** | `GET /request/group/{groupId}/check-status` (cả nhóm 1 lần) → `GET /request/{id}/check-status` (trả kèm luôn kết quả khi `Completed`) · `GET /workflow/request/{id}` tới `status = 3` | **Khuyến nghị — đây là nguồn sự thật.** D3 §G2: backoff **3s → 5s → 10s → 30s**, trần **15 phút** rồi chuyển `EXTRACTION_TIMEOUT` vào hàng đợi operator |
| **socket.io** | Room `Channel_{tenantId}`, event `updateOcrResult` / `updateWorkflowResult` | Payload chỉ `{id, workflowId, status}` — **vẫn phải gọi REST lấy dữ liệu**. Room theo tenant nên nhận cả event hồ sơ khác, phải tự lọc theo `id`. D3 §G1: *"nghe socket để giảm độ trễ nhưng mỗi tin chỉ kích hoạt một lượt gọi, không tin nội dung"* |
| **Webhook** | — | **KHÔNG TỒN TẠI.** `CreateRequestDto` không có `callback_url`. D3 §I-08 (`POST /v1/webhooks/idp` ký HMAC) **không có đối ứng ở IDP** |

**Bốn hệ quả kiến trúc:**

1. **Adapter tự phát sự kiện nội bộ đúng shape webhook I-08** để phần còn lại của hệ thống không phải biết là đang poll. Shape của D3 §I-08:
   ```jsonc
   { "kind": "EXTRACTION", "requestId": "…", "claimNo": "CLM-26-000417",
     "outcome": "COMPLETE", "completedAt": "…", "resultUrl": "…",
     "summary": { "documentCount": 3, "classifiedCount": 3, "confidence": 0.94, "warningCount": 1 } }
   ```
   Lưu ý: D3 dùng **`outcome`**, không dùng `state` — `state` đã bị A4 §6/§8 chiếm cho enum *độ đầy đủ bộ hồ sơ*.
2. **`409 NOT_READY` + `Retry-After` của D3 §I-09 không tồn tại ở IDP.** Thay bằng: đọc `errorCode` trong body; `requestStatus` khác `Completed` ⇒ poll tiếp.
3. **Kết quả async ở tầng `ocr-service` chỉ đọc được một lần** (`cacheManager.del` sau khi parse thành công). **Lưu ngay lần đọc đầu vào `RawExtractionJson`**, không đọc lại. Trùng với yêu cầu bất biến ở §6.4.
4. **Flow đánh giá chạy tuần tự, mỗi pod một flow** ⇒ 3 kịch bản chạy liên tiếp sẽ **xếp hàng**. Tính vào kịch bản demo: đừng bấm 3 hồ sơ cùng lúc rồi chờ cả ba xong.

**Idempotency:** IDP **không có `Idempotency-Key`** trên endpoint nào (D3 §B3). Vá hoàn toàn ở phía mình: bảng `idempotency_key → (groupId, requestIds[])` trong DB của mình, tra trước khi gọi. **Gap đóng hoàn toàn, không cần IDP.**

---

## 11. ReplayMode và 149 file JSON mẫu

Site Setting `ReplayMode`. Khi bật, mọi REST wrapper trả JSON đã ghi từ entity `IdpReplay` theo key = hash file hoặc `ClaimNo`. `AgentRun` đánh dấu `Replay = true`.

**Nguồn:** `New_Project_Documents/IDP_API_Spec/request-response-json/`. Mỗi file là **một endpoint**: request đã gửi và response nhận về **của từng vai**, dữ liệu thật ngày 03/09/2026. Token và mật khẩu đã thay bằng `<token>` / `<redacted>`. Tên file dạng `METHOD__đường-dẫn.json`.

### 11.1 Tám nhóm

| Thư mục | Số file | Dùng lúc nào |
|---|---|---|
| `00-chuoi-end-to-end/` | **37** | Dựng luồng chính theo đúng thứ tự |
| `01-xac-thuc-va-danh-tinh/` | 20 | Dựng `/auth/login`, `/user/me`, kiểm vai |
| `02-template-va-bo-chung-tu/` | 16 | Dựng `IdpFieldMap`, document set |
| `03-anh-xa-mbal/` | 10 | Khi cần đường `claim_type` + `document_type` |
| `04-nop-chung-tu-va-trich-xuat/` | 19 | Dựng `POST /request`, `check-status`, `result`, `get-ocr`, `raw-file` |
| `05-danh-gia/` | 14 | Dựng `PUT /workflow/request` và `/result/json` |
| `06-master-data/` | 32 | Đồng bộ 5 bảng master data |
| `07-van-hanh/` | 1 | `GET /tool` |
| **Tổng** | **149** | |

### 11.2 Chuỗi end-to-end 50 bước

Thư mục `00-` được đánh số theo **đúng thứ tự thời gian gọi** (số nhảy cách, 37 file trong dải 01→50):

| Bước | File / Endpoint | Kết quả |
|---|---|---|
| 01 | `POST /auth/login` | `201` |
| 02 | `GET /user/me` | `200` |
| 03 | `GET /template/compact` | `200` |
| 04 | `GET /template/document-set` | `200` |
| 05 | `GET /mbal/docset-map` | `200` |
| 06 | `POST /request/group` | `201` |
| 07 | `POST /request` | `201` |
| **08–14** | `GET /request/group/4/check-status` × **7 lần** | `200` |
| 15 | `GET /workflow/forwarder/api/v1/flows` | `200` |
| 21 | `GET /request/21376/check-status` | `200` |
| 22 | `GET /request/21376/result` | `200` |
| 23 | `GET /request/21376/get-ocr` | `200` |
| 24 | `GET /request/21376/check-workflow-request` | `200` |
| 25 | `PUT /workflow/request` | `200` |
| 31 | `GET /workflow/run-workflow-request/1402` | **`400`** ← lần đầu lỗi |
| 32–36 | `GET /workflow/request/1402` × 5 | `200` |
| 37 | `GET /workflow/request/1402/result/json` | **`400`** |
| 41 | `GET /workflow/get-workflow-components?flowId=8d873709-…` | `200` |
| 42 | `PUT /workflow/request` | `200` |
| 43 | `GET /workflow/run-workflow-request/1403` | `200` ← lần sau OK |
| 44–47 | `GET /workflow/request/1403` × 4 | `200` |
| 48 | `GET /workflow/request/1403/result/json` | **`400`** |
| 49 | `GET /workflow/request/1403/result/json` | `200` |
| 50 | `GET /workflow/request/1403` | `200` — bản ghi cuối cùng |

> **Chuỗi này tự nó là tài liệu vận hành**: 7 lần poll nhóm ≈ 18 giây; `run-workflow-request` lỗi lần đầu rồi OK; `/result/json` `400` rồi `200`. Dựng retry theo đúng hình dạng này.

### 11.3 Cách dùng

**Mở đúng file lúc dựng từng REST method** — dán response vào REST Consume để ODC sinh structure. **Đừng đọc trước cả 149 file**: tổng dung lượng lớn (riêng `GET /workflow/forwarder/api/v1/flows` đã ~2,8 MB), và 90% nội dung là đồ thị node Langflow không liên quan tới việc dựng method.

Quy trình đề xuất:

1. Dựng method → mở **đúng một** file trong nhóm tương ứng → dán response → ODC sinh structure.
2. Ghi chính response đó vào `IdpReplay` với key = tên endpoint + `ClaimNo` giả.
3. Bật `ReplayMode`, **tắt mạng**, chạy lại toàn luồng.
4. Chỉ mở file thứ hai khi gặp biến thể (ví dụ `/result/json` có 3 biến thể: `200` + data, `200` + `{}`, `400` + `4007` — cả ba đều có file riêng).

---

## 12. Mã lỗi

### 12.1 Mã lỗi quan sát được trên chuẩn Claim MVP

| Mã | HTTP | Thông điệp / nghĩa | Xử lý |
|---|---|---|---|
| `1010` | **403** | Cloudflare chặn User-Agent | Set `User-Agent` giả trình duyệt — §2 |
| `1014` | **200** | `Partner is expired` | **Dừng, báo vận hành.** Mọi request OCR sẽ hỏng |
| `4007` | 400 | `Bad Workflow result, something is wrong.` / `Workflow result is bad, please check again.` | Kết quả đánh giá **chưa có** hoặc flow xuất sai định dạng. Poll tiếp nếu chưa quá trần |
| — | 400 | `The specified key does not exist.` | `POST /request/from-url` — **lỗi code của IDP**, không cứu được từ phía gọi. Xem §12.2 |
| — | 400 | `invalid input syntax for type bigint: "NaN"` | `templates` gửi dạng **array** cho `POST /request` (phải là chuỗi CSV) |
| — | — | `ids.join is not a function` | `templates` gửi dạng **chuỗi CSV** cho `POST /request/from-url` (phải là mảng số) |
| — | 400 | `missing FROM-clause entry for table "template"` | `GET /template/document-set-list` thiếu tham số `sort` |
| — | **500** | `invalid input syntax for type bigint: "NaN"` | `/workflow/forwarder/api/v1/flows/assigns/all` thiếu `target_tenant_id` |
| — | 400 | `Unexpected token '"', "#" is not valid JSON` | `GET /workflow/run-workflow-request/{id}` — **không ổn định, PHẢI RETRY** |
| — | 400 | `Failed to merge images` | `POST /request/merge-multiple` nhận PDF (chỉ nhận ảnh) |
| — | 400 | `Unsupported file type` | `POST /request/compare` nhận file không phải XLSX |
| — | 400 | `Ocr Request Retry Failed Exception` | `PATCH /request/{id}/retry` trên request đã `Completed` |
| — | 400 | `Request is not running!` | `PATCH /request/{id}/cancel` trên request đã `Completed` |
| — | 403 | Sai vai | Toàn bộ `/request/*` với tài khoản Admin. Xem §3.2 |
| — | 401 | Token hết hạn hoặc đã logout | **Tự re-login** (không có refresh token) |

### 12.2 `POST /request/from-url` — đang hỏng, đã tìm ra nguyên nhân gốc

Đây là endpoint gần nhất với **nộp theo tham chiếu** của D3 §I-07 (IDP tự `GET` url phía server và **không gắn header nào**, nên pre-signed URL dùng được). Nhưng nó **hỏng**.

**Request thật** (verbatim):

```json
{
  "name": "FROMURL-1788417780",
  "templates": [5384, 5127],
  "url": "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf"
}
```

**Response** (verbatim):

```json
{
  "statusCode": 400,
  "message": "The specified key does not exist."
}
```

**Nguyên nhân gốc** (trích verbatim): *"`from-url` tải file về **đĩa cục bộ** rồi truyền `{ originalname, path: <đường dẫn cục bộ> }` sang `requestService.create()`. Nhưng hàm đó gọi thẳng `minIOService.getObjectStream(file.path)` — nó coi `path` là **khoá đối tượng trên MinIO** … Đường dẫn cục bộ không phải khoá MinIO ⇒ lỗi. Ở một hàm khác trong cùng service có nhánh `if (file.storageType == StorageType.MinIO) … else fs.createReadStream(...)`; nhánh `create()` **thiếu đúng nhánh đó**."*

⇒ **Trước mắt nộp bytes từ object storage của mình.** Xin IDP sửa.

### 12.3 Chính sách retry

**Theo D3 §1.3, áp dụng nguyên văn:**

| Loại | Retry? |
|---|---|
| `5xx`, timeout, connection failure, `429` | **Có** — backoff `1s, 2s, 4s, 8s, 16s` (5 lần retry, 6 lần gọi), rồi dừng và báo động vận hành |
| `4xx` nói chung | **Không** — *"repeating a wrong request produces the same wrong answer while adding load"* |
| **Ngoại lệ 1** — `409 NOT_READY` | Có (nhưng **IDP không dùng `409`**; ở đây tương đương với `requestStatus != Completed`) |
| **Ngoại lệ 2 — riêng của IDP** — `400` trên `GET /workflow/run-workflow-request/{id}` | **CÓ.** Đây là ngoại lệ do hành vi không ổn định đã kiểm chứng, **phải ghi chú rõ trong code** |

**Retry mạng** theo danh sách errno cứng: `ECONNREFUSED`, `ECONNRESET`, `ETIMEDOUT`, `ENOTFOUND`, `EAI_AGAIN`, và bất kỳ lỗi nào có message chứa `"timeout"`. Các lỗi này **không** đánh `Failed`.

**Timeout** (D3 §1.3): connect **5 s**, read **30 s**, riêng nộp gói (`POST /request`) cho phép read **60 s**. `POST /request/sync` cần **ít nhất 60 s** vì bản thân nó chờ đồng bộ ~16 s/trang.

**Circuit breaker** (D3 G-6): **5 lỗi liên tiếp mở 60 giây**. Thiết kế theo ngưỡng **100 req/phút** cho an toàn.

### 12.4 Nguyên tắc bất di bất dịch

> **D3 G-3: Không một lỗi tích hợp nào được biến thành quyết định từ chối.**
> Mọi lỗi IDP chuyển thành `EXTRACTION_ERROR` hoặc `DOC_PROCESSING` timeout vào **hàng đợi operator**, **không bao giờ** thành `DECLINED`.
> *"A claim declined because a service was down … is indistinguishable from a correct decline unless the states are kept separate."*

---

## 13. Cần xác thực bằng gọi thật

| # | Việc | Vì sao quan trọng | Ai trả lời |
|---|---|---|---|
| 1 | **TTL token thật** | Đang giả định 30 phút. Quyết định chu kỳ re-login | Đội IDP |
| 2 | **`MAX_FILE_SIZE` thật trên staging** | Quyết định có phải tách file không | Đội IDP |
| 3 | **`THROTTLE_TTL` / `THROTTLE_LIMIT` thật** | Mặc định 100 req/60 s, nhưng override được bằng env | Đội IDP |
| 4 | **Mâu thuẫn `5384 → CL10` vs `subDocumentType: "CL05"`** ở `/request/21383/result` | Nếu hiểu sai thì crosswalk sai từ gốc | Gọi lại, đọc `templateId` trong `templates_data[]` |
| 5 | **Tên của template `5287`, `5291`, `5299`, `5340`, `5385`** | 5 template nằm trong document set 193/194/195 mà chưa biết là chứng từ gì | `GET /template/{id}` |
| 6 | **`refPartnerId: "78"` vs `partnerId: "40"`** | Nếu dùng nhầm để correlate thì hỏng lặng lẽ | Hỏi IDP hoặc gọi thật |
| 7 | **`raw-file` sau khi MinIO đã xoá file** | Ảnh hưởng trực tiếp màn thẩm định. Hiện `21376` vẫn trả `200`/42.134 bytes | Gọi lại sau vài ngày |
| 8 | **`extraInputData` với template ảo `CLINICAL_CONTEXT_V1`** | Flow có đọc đúng không, hay chỉ nhận `Dữ liệu từ DP` / `MBAL_DP_DATA` | Gọi thật + xin IDP cam kết |
| 9 | **`POST /request/sync` có dùng được cho nhiều trang không** | Nếu được thì bỏ hẳn vòng poll ở kịch bản A | Gọi thật với PDF 5–10 trang |
| 10 | **Ngữ nghĩa `separatorType` / `separatorValue`** | FO chỉ `JSON.stringify` rồi chuyển sang lõi OCR — **ngoài tầm cả swagger lẫn source FO** | Chỉ đội lõi OCR trả lời được |
| 11 | **`c` trả ra có bao giờ bao gồm `c_llm` không** | Quyết định ngưỡng cổng tự duyệt | Đội IDP |
| 12 | **Định dạng số tiền thật trong `tables`** | `tableFields` có `type: null` ⇒ template không ép kiểu. Chứng từ mẫu không có trường tiền | Chạy một bảng kê chi phí thật qua `5383` |
| 13 | **IDP có parser XML hoá đơn không** (`T1-04`) | A4 §1.5: XML là đường **duy nhất** cho `T1-04` | Đội IDP |
| 14 | **Ghim flow `5ed8e4fc` suốt 4 tuần sprint** | Đổi flow là vỡ toàn bộ `/result/json` | Đội IDP — **hạn trước 22/09** |
| 15 | **Service account vai Tenant riêng cho tích hợp** | Đang dùng chung tài khoản `203` | Đội IDP — **hạn trước 22/09** |

---

## Phụ lục A — những gì thuộc MBAL-IDP, không phải chuẩn

> **Toàn bộ phụ lục này thuộc MBAL-IDP, KHÔNG nằm trong phạm vi Claim MVP.** Giữ lại để khi cần kiểm chứng thực tế thì biết tìm ở đâu, và để khi ai đó nhìn vào MBAL thì không tưởng nhầm là chuẩn của bản build này.

### A.1 Contract cảnh báo con dấu / "Dấu hiệu bất thường"

**Nguồn:** `01. Requirement/Note.xlsx`, sheet `18_9_Con dấu + Bất thường` — **MBAL-IDP Phase 2**.

**KHÔNG nằm trong phạm vi Claim MVP.** Lý do: A4 của Claim MVP xếp **Tier 2 là classify-only** và **không có kiểm tra giả mạo**. Đối chiếu bằng cấu hình flow thật: tìm `giả mạo`, `làm giả` trong toàn bộ flow `5ed8e4fc` — **0 lần xuất hiện**, không có node nào liên quan. PO (BIA) cũng xác nhận mục *"Giấy tờ có bị làm giả không"* là **chưa phát triển vì thiếu dữ liệu ngành**.

Tóm tắt contract, **để tham chiếu, không để dựng**:

- Kết quả sinh ra **hai field thường trong mảng `fields`**, không có trường riêng: `"Dấu hiệu bất thường"` (chỉ khi phát hiện bất thường font chữ **hoặc** cắt ghép; giá trị `"Có tại trang số..."`) và `"Kiểm tra con dấu"` (chỉ khi chứng từ có con dấu).
- Giá trị `"Kiểm tra con dấu"`: 1 dấu có trong thư viện → `"Con dấu thuộc {tên bệnh viện}"` · chưa có, lần đầu → lưu vào thư viện + `"Con dấu không tồn tại."` · chưa có, lần sau → `"Con dấu thuộc yyyy_mm_dd_tenbenhvien."` · không đạt chuẩn → `"Con dấu không đạt chuẩn."` · nhiều dấu khác bệnh viện → `"Các con dấu thuộc nhiều cơ sở y tế khác nhau trên cùng 1 chứng từ"`.
- Thứ tự ưu tiên hiển thị: không tồn tại → không đạt chuẩn → nhiều cơ sở y tế → PASS.
- Đặt tên thư viện dấu: `yyyy_mm_dd__hhmmss_tenbenhvien` (bản cập nhật 29/9). Chỉ đối chiếu với dấu đã import trước đó **≥ 1 tiếng**.
- Phạm vi đã chốt, verbatim: *"không bao gồm cắt ghép, chỉnh sửa làm giả nội dung"*.
- Data test theo từng kịch bản dấu: `04. Testing/Phase 2/Data con dấu/`.

### A.2 Số đo độ chính xác 83,49 % / 41,67 %

| Chỉ số | Giá trị | Nguồn |
|---|---|---|
| Độ chính xác **trường** | **83,49 %** | `04. Testing/Testing/MBAL- Test report.xlsx` — 15 chứng từ / 13 template, staging |
| Độ chính xác **bảng** | **41,67 %** | cùng nguồn — bảng kê 6 trang chỉ đọc được trang đầu |

> ⚠️ **KHÔNG dùng được làm số đo của hệ này.** Hai con số này đo trên **bộ template MBAL** (`777`, `1145`, `973`…), **không phải** template Claim MVP (`5384`, `5383`, `5127`, `5312`…). Bộ template khác, prompt khác, chứng từ khác ⇒ con số không chuyển sang được.
>
> **Số đo của Claim MVP phải đo lại** bằng `POST /request/compare` / `/request/compare-multiple` trên bộ ground-truth XLSX theo A4 §10 — công cụ **có sẵn, không phải tự viết**.

Các số MBAL khác cùng loại, cùng cảnh báo: độ chính xác toàn bộ 80,5 % (100 lượt, 2.413 field) · bảng kê chi phí 60,9 % (thấp nhất, do merge cell) · tóm tắt bệnh án 91,1 % · cần người can thiệp 44,4 % ở ngưỡng 95 % và 26,7 % ở ngưỡng 85 %.

**Một điều từ MBAL vẫn đáng mang sang**, không phải như số đo mà như **cảnh báo thiết kế**: bảng kê chi phí là loại chứng từ có độ chính xác **thấp nhất** đã đo được, và ở Claim MVP nó chính là **`T1-01` — chứng từ mà mọi dòng chi phí đến từ đó**. Chứng từ quan trọng nhất là chứng từ khó nhất. Cùng hướng: comparator của MBAL **chỉ đọc mảng `fields`, không đọc `tables`** — nên toàn bộ dòng chi phí chưa hề được đối chiếu.

Thời gian xử lý MBAL (dùng để đặt kỳ vọng SLA, chấp nhận được vì cùng hạ tầng): 26,3 s/chứng từ · ~16 s/trang · end-to-end 27 s · 821 chứng từ/giờ · 33 trang / 2 template = 2 ph 44 s · uptime 99 % (scale node 110 s).

### A.3 MBAL Docset Map và bộ mã MBAL

**Cơ chế vẫn tồn tại và vẫn gọi được trên tenant 176** — xem §5.4, nơi nó được dùng như **đường dự phòng**. Những gì dưới đây là **bộ mã và template của MBAL production**, **không phải** của Claim MVP:

- **Template MBAL:** 36 loại chứng từ / **40 templateId** — `777` (Custom-T101, Giấy ra viện), `776` (Custom-T102), `1091`, `2645`, `1141`, `1145`, `973`/`2659`, `1089`, `1167` (CCCD), `2637` (Thẻ BHYT), `1133`, `231` (= *Template khác / Other*, một **fallback đã-phân-loại**, ≠ `unclassified`).
- **Ánh xạ tham khảo `T1-xx`/`T2-xx` ↔ `CLxx` ↔ templateId MBAL** đã ghi ở `build-01` §9.2. Bốn lệch không khép được: `T1-04` XML-only ở Claim MVP nhưng MBAL OCR ảnh hoá đơn · Giấy ra viện (`CL05`) bắt buộc ở MBAL nhưng **bị loại khỏi Tier 1** ở Claim MVP · MBAL 36 loại / 40 template vs Claim MVP 8 loại — **không cùng độ phân giải** · `231 = Other` ≠ `unclassified`.
- **Bẫy dữ liệu của `MBALDocsetMap`** (vẫn còn ở API tenant 176, nên vẫn phải phòng): cặp `(claim_type, document_type)` **không unique** — `findOne` lấy bừa một dòng khi trùng; `claim_type` / `document_type` là **varchar trần, không FK danh mục**; `Template → sub_document_type` bị ép **N:1** (`findOneOrFail({templateId})`, không có ngữ cảnh `claim_type`).
- **`extraParams` với khoá `mbal_*`** (`mbal_claim_type`, `mbal_document_type`, `mbal_docset_map_id`, `document_set`): **không dùng cho Claim MVP.** D3 §B5 ghi `extraParams` là **type đóng, chỉ 4 khoá MBAL**; mọi payload thật của tenant 176 trả `extraParams: {}`. Correlate bằng trường **`name`** — §4.3.
- **`MBALStatistics`** chỉ đo, không so ngưỡng nào — đã bỏ. Đo độ chính xác ở Claim MVP là `AccuracyRun` trên bộ ground-truth, chạy bằng `POST /request/compare`.
