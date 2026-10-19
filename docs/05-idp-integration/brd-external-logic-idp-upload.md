# BRD — External Logic `IdpUpload`: nộp chứng từ multipart lên BnK IDP

**Ngày:** 25/09/2026 · **Trạng thái:** đề xuất, chờ duyệt để bắt tay
**Phạm vi:** chỉ phần nộp file. Không đụng 15 REST method hiện có, không đụng parser, không đụng logic tiền.
**Mốc:** phải xong trước pitch **07/10**. Timebox và điểm cắt ở §10 — đọc mục đó trước khi cam kết.

---

## 0. Quyết định cần duyệt

Xây một **External Logic library C# (.NET 8)** làm đúng một việc: dựng request `multipart/form-data` có `filename=` và gọi `POST /request` · `POST /request/sync` của BnK IDP, trả về JSON thô cho ODC.

Lý do tồn tại: **ODC không đặt được `filename=` trên một binary multipart part**, và server IDP bắt buộc phải có nó. Đây là giới hạn nền tảng đã xác minh, không phải lỗi cấu hình.

Hạng mục này **cạnh tranh trực tiếp** với hai việc đang là đường găng: **AG-2 trên Agent Workbench** (sàn tuyệt đối của bài dự thi, hiện chưa dựng) và **deck pitch** (hiện ở con số 0). Nó không nằm trong ba tiêu chí chấm. Duyệt nó nghĩa là chấp nhận đánh đổi đó một cách có ý thức.

---

## 1. Vì sao cần — nguyên nhân gốc

Phần `document` mà ODC gửi đi có dạng:

```
Content-Disposition: form-data; name="document"
Content-Type: image/png
<bytes>
```

Còn request chạy được, bắt từ chính Swagger UI của IDP:

```
Content-Disposition: form-data; name="document"; filename="6093_vnvision-giay-ra-vien.png"
Content-Type: image/png
<bytes>
```

Khác nhau đúng một token: **`filename=`**.

Busboy — parser mà multer dùng phía IDP — phân loại part theo đúng tiêu chí đó. Có `filename=` là **file**; không có thì part được đọc như một **text field**. Nên `req.file` là `undefined`, rồi `requestService.create()` gọi `minIOService.getObjectStream(file.path)` và ném:

```
500 {"statusCode":500,"message":"Cannot read properties of undefined (reading 'path')"}
```

`Content-Type` của part **không** tham gia vào việc phân loại. Đặt đúng `image/png` vẫn hỏng — đã thử.

---

## 2. Các đường đã thử và đã đóng

| Đường | Kết quả | Bằng chứng |
|---|---|---|
| Bộ dựng multipart của ODC REST Consume | **Đóng** | `IRestActionInput` chỉ có `InputPlacement`, `OriginalName`, `ContentType`, `DefaultValueBehavior`, `IsSensitive`. Không có thuộc tính nào sinh ra `filename=` |
| Chèn qua `OriginalName` (`document"; filename="doc.png`) | **Đóng** | Nền tảng escape ở tầng runtime; vẫn `500 reading 'path'` |
| Tự ghép body trong logic ODC | **Đóng** | Library `BinaryData` có 8 action, **không có hàm nối**. `TextToBinaryData` chỉ nhận `unicode`/`utf-8`/`utf-16`/`ascii` — không encoding nào bảo toàn byte cho dữ liệu nhị phân |
| `fetch` + `FormData` từ trình duyệt | **Đóng** | CORS chặn preflight từ origin `https://bnk-dev.outsystems.app`. Đã loại trừ CSP: không có `securitypolicyviolation` nào bắn ra |
| `POST /request/from-url` (nộp theo tham chiếu) | **Đóng** | Hỏng phía IDP: `create()` coi đường dẫn cục bộ như khoá MinIO → `400 "The specified key does not exist."` |

Kết luận: trong phạm vi ODC thuần, không còn đường nào.

---

## 3. Phạm vi

### 3.1 Làm

- Một External Logic library tên **`IdpUpload`**.
- Hai thao tác: nộp đồng bộ (`/request/sync`) và nộp bất đồng bộ (`/request`).
- Dựng multipart đúng chuẩn, có `filename=` và content-type cho part file.
- Gắn đủ header bắt buộc, kể cả `User-Agent` giả trình duyệt.
- Trả về **JSON thô nguyên văn** + HTTP status. Không diễn giải, không chuẩn hoá.

### 3.2 Không làm

- Không phân tích kết quả trích xuất. `IdpParseExtraction` ở ODC giữ nguyên vai trò đó.
- Không quản lý token. ODC truyền token vào; library không biết username/password.
- Không retry, không circuit breaker, không ghi entity. Những thứ đó thuộc `IdpSubmitSync` / `IdpSubmitDocument` phía ODC.
- Không đụng 13 method REST còn lại. Auth, poll, workflow, master data **vẫn chạy bằng REST Consume của ODC** và đang hoạt động tốt.
- Không dựng UI.

> **Nguyên tắc:** library là một cái ống. Mọi phán đoán ở lại ODC.

---

## 4. Hợp đồng giao diện

```csharp
[OSInterface(Name = "IdpUpload",
             Description = "Nộp chứng từ multipart lên BnK IDP, có filename= mà ODC REST Consume không đặt được")]
public interface IIdpUpload
{
    [OSAction(Description = "POST /request/sync — nộp và chờ kết quả trong một lời gọi")]
    IdpUploadResult SubmitSync(
        [OSParameter] string BaseUrl,
        [OSParameter] string AccessToken,
        [OSParameter] string UserAgent,
        [OSParameter] string Name,
        [OSParameter] string TemplatesCsv,
        [OSParameter] byte[] Document,
        [OSParameter] string FileName,
        [OSParameter] string ContentType,
        [OSParameter] int TimeoutSeconds);

    [OSAction(Description = "POST /request — nộp bất đồng bộ, trả requestId")]
    IdpUploadResult Submit(
        [OSParameter] string BaseUrl,
        [OSParameter] string AccessToken,
        [OSParameter] string UserAgent,
        [OSParameter] string Name,
        [OSParameter] string TemplatesCsv,
        [OSParameter] byte[] Document,
        [OSParameter] string FileName,
        [OSParameter] string ContentType,
        [OSParameter] string RequestGroupId,
        [OSParameter] int TimeoutSeconds);
}

[OSStructure(Description = "Kết quả thô của một lần nộp")]
public struct IdpUploadResult
{
    [OSStructureField] public bool TransportOk;      // true = có phản hồi HTTP, kể cả 4xx/5xx
    [OSStructureField] public int HttpStatus;
    [OSStructureField] public string RawJson;        // body nguyên văn, không sửa
    [OSStructureField] public string ErrorMessage;   // chỉ khi TransportOk = false
    [OSStructureField] public int ElapsedMs;
}
```

**`TransportOk` tách khỏi `HttpStatus` là có chủ đích.** `TransportOk = true` + `HttpStatus = 500` là *API đã trả lời và từ chối* — việc của ODC xử. `TransportOk = false` là *không nối được* — việc của hạ tầng. Gộp hai thứ là mất thông tin, và đó chính là thứ đã khiến buổi debug hôm nay phải đi vòng qua log của Portal.

---

## 5. Ràng buộc nền tảng — đọc kỹ, chúng định hình thiết kế

| Ràng buộc | Giá trị | Hệ quả |
|---|---|---|
| **Payload tối đa** | **5,5 MB** | Đây là trần cứng của toàn bộ giải pháp. Xem §5.1 |
| Thời gian chạy tối đa | 95 giây | Đủ cho `/request/sync` một trang (13–19 s), không đủ cho hồ sơ nhiều trang |
| RAM | 1 GB | Không giữ nhiều bản sao của mảng byte |
| **Cấm `async` trên method `[OSInterface]`** | — | `HttpClient` vốn bất đồng bộ ⇒ phải `.GetAwaiter().GetResult()`. Ghi chú rõ trong code vì sao |
| Môi trường build | **.NET 8 SDK** + outsystems MCP đã kết nối | Docker **không** bắt buộc — nó chỉ dùng cho integration test dạng container, và có bản thay thế không cần container |

### 5.1 Trần 5,5 MB là một khoảng trống phải khai báo

D3 §I-07 cho phép một hồ sơ tới **20 chứng từ, 50 trang, 100 MB**. Library này chịu **5,5 MB cho mỗi lời gọi**.

Xử lý đã chốt:

- Library **kiểm kích thước trước khi gọi**. Vượt trần → trả `TransportOk = false`, `ErrorMessage = "PAYLOAD_TOO_LARGE: <n> bytes > 5.5 MB"`. **Không** cắt file, **không** nén, **không** thử rồi để nền tảng giết.
- ODC bắt lỗi đó và chuyển hồ sơ sang `EXTRACTION_ERROR` vào hàng đợi vận hành.
- Bộ chứng từ demo phải nằm dưới trần. File test hiện tại 13 KB.
- Ghi vào tài liệu bàn giao rằng đây là **giới hạn của giải pháp tạm**, và nó biến mất khi đội IDP sửa phía họ (§12).

---

## 6. Hợp đồng gọi IDP

### 6.1 Ba header, bắt buộc trên mọi lời gọi

```
Authorization: Bearer <AccessToken>
User-Agent:    <UserAgent — chuỗi giả trình duyệt>
Accept:        application/json, text/plain, */*
```

> **`User-Agent` không phải tuỳ chọn.** Thiếu nó, Cloudflare trả `403` kèm body `error code: 1010`. Không có trong Swagger, và triệu chứng nhìn hệt như sai mật khẩu.
>
> Library **không** tự đặt `Content-Type` của cả request. `MultipartFormDataContent` của .NET tự sinh nó kèm boundary. Đặt tay là phá boundary — lỗi đã tái hiện được trong Postman.

### 6.2 Các part của body

| Part | Kiểu | Giá trị | Ghi chú |
|---|---|---|---|
| `name` | text | `Name` | **Chỗ duy nhất** mang được `ClaimNo` sang IDP. API không có correlation id, không có `Idempotency-Key` |
| `templates` | text | `TemplatesCsv` | **Chuỗi ngăn cách phẩy**, ví dụ `6090,6091,6092`. Gửi mảng JSON → `400 invalid input syntax for type bigint: "NaN"`. Swagger khai là `array` và Swagger sai |
| `document` | file | `Document` | **Phải có `filename=` và `Content-Type`.** Đây là lý do library tồn tại |
| `requestGroupId` | text | chỉ ở `Submit` | Bỏ qua nếu rỗng |

`separatorType`, `separatorValue`, `deleteFile`, `extraParams` là tuỳ chọn và **không gửi**. Bản capture 03/09 chạy được mà không có chúng.

### 6.3 Định dạng file

Content-type suy từ đuôi tên file, **một bảng duy nhất trong library**:

| Đuôi | Content-Type |
|---|---|
| `.pdf` | `application/pdf` |
| `.png` | `image/png` |
| `.jpg` · `.jpeg` | `image/jpeg` |
| `.tif` · `.tiff` | `image/tiff` |
| khác | **không gọi API** — trả `ErrorMessage = "UNSUPPORTED_FILE_TYPE: <đuôi>"` |

**Cấm dùng `application/octet-stream` làm giá trị mặc định.** Đó là giá trị tự nhiên nhất và cũng chính là thứ server từ chối.

Nếu `ContentType` được truyền vào không rỗng thì ưu tiên nó; rỗng thì suy từ đuôi file.

---

## 7. Hợp đồng lỗi

1. **HTTP 200 không có nghĩa là thành công.** Một số lỗi trả `200` kèm `errorCode` trong body — ví dụ `1014 Partner is expired`. Library **không** diễn giải; nó trả nguyên `RawJson` để ODC đọc `errorCode`.
2. **`templates_data` rỗng là LỖI**, không phải "không phân loại được". Nghĩa là template chưa gán cho tenant. Request vẫn `Completed`, `errorCode` null, và **vẫn mất một trang quota**. Việc nhận diện thuộc về `IdpParseExtraction` ở ODC.
3. Timeout, DNS, TLS, connection reset → `TransportOk = false` + `ErrorMessage` mang thông điệp exception **và** response body nếu có.
4. **Không một lỗi tích hợp nào được biến thành quyết định từ chối.** Mọi thất bại → `EXTRACTION_ERROR` vào hàng đợi vận hành, không bao giờ thành `DECLINED`. Luật này thuộc ODC, nhưng ghi ở đây để hợp đồng lỗi của library phục vụ được nó.

---

## 8. Bảo mật

- Library **không bao giờ** ghi `AccessToken` vào log, exception message, hay giá trị trả về. Token chỉ tồn tại như tham số và như header.
- Không ghi nội dung file ra đĩa. Dựng multipart trong bộ nhớ.
- Không ghi `RawJson` ra log — nó chứa dữ liệu chứng từ, có thể có PII.
- `ILogger` chỉ ghi: tên thao tác, kích thước byte, HTTP status, thời gian chạy. Không hơn.

---

## 9. Tiêu chí nghiệm thu

| # | Tiêu chí | Cách kiểm |
|---|---|---|
| A1 | `SubmitSync` với `6093_vnvision-giay-ra-vien.png` trả `HttpStatus 201` | Bấm nút trên màn IDP diagnostics. Tốn 1 trang quota |
| A2 | `RawJson` chứa `templates_data` với `templateId` = **6093** | Đọc trên màn |
| A3 | Thời gian chạy 13–19 giây, không bị cắt | `ElapsedMs` trả về |
| A4 | File `.xlsx` → `UNSUPPORTED_FILE_TYPE`, **không gọi API** | Test đơn vị, 0 quota |
| A5 | File 6 MB → `PAYLOAD_TOO_LARGE`, **không gọi API** | Test đơn vị, 0 quota |
| A6 | Thiếu `User-Agent` → lỗi `403` được trả về nguyên vẹn, không nuốt | Test tích hợp bằng container |
| A7 | Không có chuỗi token nào trong log của một lần chạy thật | Đọc log ODC Portal sau A1 |
| A8 | `Submit` (bất đồng bộ) trả `requestId` và `pageCount` | 1 trang quota |

A4, A5 chạy offline và **phải xanh trước** khi tiêu trang quota nào cho A1.

---

## 10. Kế hoạch, timebox và điểm cắt

| Bước | Nội dung | Ngân sách |
|---|---|---|
| 1 | Kiểm .NET 8 SDK + MCP, dựng project theo `outsystems-custom-code`, deploy thử một library rỗng | 1 giờ |
| 2 | Viết `IdpUpload` + `IdpUploadResult`, bảng content-type, kiểm trần | 2 giờ |
| 3 | Test đơn vị A4, A5 | 1 giờ |
| 4 | Deploy library lên tenant, tham chiếu từ ClaimMVP | 1 giờ |
| 5 | Nối vào `IdpSubmitSync` / `IdpSubmitDocument`, giữ nguyên chữ ký hai action | 1 giờ |
| 6 | Nghiệm thu A1–A3, A6–A8 | 1 giờ |
| | **Tổng** | **7 giờ, tức 1 ngày làm việc** |

> ### Điểm cắt — bắt buộc tôn trọng
>
> **Hạn: hết ngày 29/09.** Chưa qua được bước 4 (library deploy được lên tenant) thì **dừng hẳn**, revert, và chạy `ReplayMode` cho demo. Không gia hạn, không "thêm một buổi nữa".
>
> Lý do: hạng mục này không nằm trong tiêu chí chấm nào. **AG-2 chưa dựng và đó là sàn tuyệt đối; deck vẫn ở con số 0.** Một library upload hoàn hảo trên một bài dự thi không có agent là một bài dự thi trượt.

---

## 11. Rủi ro

| Rủi ro | Mức | Xử |
|---|---|---|
| Máy dev chưa có .NET 8 SDK, hoặc outsystems MCP chưa kết nối | 🔴 | Kiểm **trước** khi duyệt. Chưa có thì cộng thêm thời gian cài, và điểm cắt vẫn giữ nguyên |
| Không có Docker | 🟢 | Không chặn. Docker chỉ phục vụ integration test dạng container; dùng mock server chạy thẳng trên máy thay thế (SRD §12.3a) |
| `.GetAwaiter().GetResult()` gây deadlock | 🟡 | Dùng `HttpClient` với `ConfigureAwait(false)` xuyên suốt; test tích hợp bằng container phải bắt được |
| Deploy External Logic lên tenant lỗi quyền | 🟡 | Thử deploy một library rỗng ở bước 1, trước khi viết logic |
| Trần 5,5 MB chặn chứng từ thật của pilot | 🟡 | Đã khai báo ở §5.1. Không giải quyết trong phạm vi này |
| Thêm một asset phải publish mỗi lần đổi | 🟢 | Chấp nhận |

---

## 12. Việc song song, rẻ hơn nhiều — và có thể làm hạng mục này thành thừa

Đã gửi đội IDP yêu cầu cho `POST /request` và `/request/sync` **nhận part `document` không có `filename=`** — ví dụ đặt tên mặc định từ `Content-Type` khi thiếu. Đây là thay đổi vài dòng cấu hình multer phía họ, và nó mở đường cho **mọi client không phải trình duyệt**, không riêng ODC.

Nếu họ làm kịp trước 07/10:

- Bật lại đường REST Consume của ODC bằng **một lần publish**. Code đã sẵn sàng, đang bị chặn ở đầu kia.
- Library này thành không cần thiết.
- Trần 5,5 MB biến mất.

**Vì vậy: theo dõi phản hồi của đội IDP mỗi ngày.** Có tín hiệu họ sẽ sửa trong tuần này thì hoãn hạng mục này lại — đó là lựa chọn tốt hơn hẳn việc xây xong rồi vứt đi.

Yêu cầu thứ hai đã gửi kèm: sửa `POST /request/from-url`. Nếu endpoint đó chạy, bên mình nộp theo tham chiếu và không cần multipart ở bất kỳ đâu nữa.

---

## Phụ lục — dữ liệu tham chiếu

**Tenant đích:** 177 · user 204 · `roleId 2` (Tenant/End User) · partner 40 · hết hạn **31/10/2026**
**Base URL:** `https://idp-api.idpstg.bnksolution.com`
**Template CSV:** `6090,6091,6092,6093,6094,6095,6096,6097,6098`
**Document set:** 198 `HEALTH_CLAIM_OS_V1`
**TTL token:** **48 giờ** (`exp − iat = 172.800 s`, đo 25/09 — đóng mục 1 trong 15 việc "cần xác thực bằng gọi thật" của `build-02` §13)
**File test:** `insurance BNK\PMOInsure\PMOInsure\Claude outputs\6093_vnvision-giay-ra-vien.png` — 13.021 byte, giả lập hoàn toàn, không PII

**Request chạy được, bắt từ Swagger UI của IDP ngày 25/09:**

```
POST /request/sync
Authorization: Bearer <token>
accept: */*
Content-Type: multipart/form-data; boundary=<tự sinh>

-F name=TEST-DIAG
-F templates=6090,6091,6092,6093,6094,6095,6096,6097,6098
-F document=@6093_vnvision-giay-ra-vien.png;type=image/png
```

Đây là hình dạng mà library phải tái tạo cho bằng được.
