# SRD — `IdpUpload` External Logic (.NET 8) cho ODC

**Dành cho:** Claude Code. Đọc hết trước khi viết dòng đầu tiên.
**Tài liệu mẹ:** `claude/brd-external-logic-idp-upload.md` — phần *vì sao* nằm ở đó, ở đây chỉ có *làm thế nào*.
**Ngày:** 25/09/2026 · **Điểm cắt: hết 29/09.** Chưa deploy được lên tenant thì dừng, revert, không gia hạn.

---

## 1. Nhiệm vụ một câu

Một class library .NET 8 phơi ra cho ODC hai thao tác nộp chứng từ lên BnK IDP bằng `multipart/form-data`, trong đó part `document` **phải có `filename=`** — thứ mà ODC REST Consume không đặt được và server IDP bắt buộc phải có.

Library là **một cái ống**. Không parse kết quả, không quản token, không retry, không ghi dữ liệu. Mọi phán đoán ở lại ODC.

---

## 2. Điều kiện tiên quyết — kiểm trước, đừng bắt đầu nếu thiếu

```bash
dotnet --list-sdks     # BẮT BUỘC, phải có 8.x
```

Cộng **outsystems MCP đã kết nối và đăng nhập** — đó là đường deploy (`extlib_upload` · `extlib_status` · `extlib_publish`).

**Docker KHÔNG bắt buộc.** Frontmatter của `outsystems-custom-code` phân vai rõ: *".NET 8 SDK locally; Docker for integration tests; and the outsystems MCP server connected and authenticated for deployment."* Build, đóng gói, deploy không dùng Docker ở bất kỳ bước nào. Không có Docker thì bỏ §12.3b và dùng §12.3a.

Thiếu .NET 8 SDK hoặc MCP chưa kết nối: dừng, báo lại. Điểm cắt 29/09 không dời.

---

## 3. Cấu trúc project

```
IdpUpload/
├─ IdpUpload.csproj
├─ IIdpUpload.cs           # interface [OSInterface]
├─ IdpUpload.cs            # implementation
├─ IdpUploadResult.cs      # [OSStructure]
├─ ContentTypeMap.cs       # bảng đuôi file → mime, DUY NHẤT một bản
└─ MultipartBuilder.cs     # dựng body, tách riêng để test được
IdpUpload.Tests/
├─ IdpUpload.Tests.csproj
├─ ContentTypeMapTests.cs
├─ ValidationTests.cs      # không chạm mạng
└─ MultipartShapeTests.cs  # kiểm byte của body
```

`IdpUpload.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="OutSystems.ExternalLibraries.SDK" Version="1.5.0" />
  </ItemGroup>
</Project>
```

Kiểm phiên bản SDK mới nhất trên NuGet trước khi ghim; nếu khác thì dùng bản mới nhất ổn định và ghi lại số.

---

## 4. Hợp đồng công khai

```csharp
[OSInterface(
    Name = "IdpUpload",
    Description = "Nộp chứng từ multipart lên BnK IDP, có filename= mà ODC REST Consume không đặt được")]
public interface IIdpUpload
{
    [OSAction(Description = "POST /request/sync — nộp và chờ kết quả trong một lời gọi")]
    IdpUploadResult SubmitSync(
        [OSParameter(Description = "https://idp-api.idpstg.bnksolution.com")] string BaseUrl,
        [OSParameter(Description = "JWT, không kèm chữ Bearer")]              string AccessToken,
        [OSParameter(Description = "Chuỗi user-agent giả trình duyệt")]        string UserAgent,
        [OSParameter(Description = "ClaimNo — chỗ duy nhất mang được sang IDP")] string Name,
        [OSParameter(Description = "Chuỗi ngăn cách phẩy, KHÔNG phải mảng")]  string TemplatesCsv,
        [OSParameter(Description = "Nội dung file")]                           byte[] Document,
        [OSParameter(Description = "Tên file kèm đuôi, ví dụ a.png")]          string FileName,
        [OSParameter(Description = "Để rỗng thì suy từ đuôi file")]            string ContentType,
        [OSParameter(Description = "Mặc định 60 nếu <= 0")]                    int TimeoutSeconds);

    [OSAction(Description = "POST /request — nộp bất đồng bộ, trả requestId")]
    IdpUploadResult Submit(
        string BaseUrl, string AccessToken, string UserAgent,
        string Name, string TemplatesCsv,
        byte[] Document, string FileName, string ContentType,
        [OSParameter(Description = "Bỏ qua nếu rỗng")] string RequestGroupId,
        int TimeoutSeconds);
}

[OSStructure(Description = "Kết quả thô của một lần nộp")]
public struct IdpUploadResult
{
    [OSStructureField(Description = "true = có phản hồi HTTP, kể cả 4xx/5xx")]
    public bool TransportOk;
    [OSStructureField] public int    HttpStatus;
    [OSStructureField(Description = "Body nguyên văn, không sửa")]
    public string RawJson;
    [OSStructureField(Description = "Chỉ điền khi TransportOk = false")]
    public string ErrorMessage;
    [OSStructureField] public int    ElapsedMs;
}
```

> `TransportOk` tách khỏi `HttpStatus` là bắt buộc. `true` + `500` là *API đã trả lời và từ chối*; `false` là *không nối được*. Gộp hai thứ là mất thông tin — đúng cái đã khiến một buổi debug phải đi vòng qua log của Portal.

---

## 5. Phần cốt lõi — dựng multipart

Đây là lý do library tồn tại. Viết đúng chỗ này, mọi thứ khác là phụ.

```csharp
using var form = new MultipartFormDataContent();

// Các part text: KHÔNG đặt Content-Type. curl -F 'name=X' cũng không đặt.
var nameContent = new StringContent(Name, Encoding.UTF8);
nameContent.Headers.ContentType = null;
form.Add(nameContent, "name");

var tplContent = new StringContent(TemplatesCsv, Encoding.UTF8);
tplContent.Headers.ContentType = null;
form.Add(tplContent, "templates");

// Part file: overload 3 tham số mới sinh ra filename=
var fileContent = new ByteArrayContent(Document);
fileContent.Headers.ContentType = new MediaTypeHeaderValue(resolvedContentType);
form.Add(fileContent, "document", FileName);   // ← tham số thứ 3 = filename
```

Kết quả trên dây phải là:

```
Content-Disposition: form-data; name="document"; filename="6093_vnvision-giay-ra-vien.png"
Content-Type: image/png
```

**Ba điều cấm:**

1. **Không** đặt `Content-Type` của cả request. `MultipartFormDataContent` tự sinh nó kèm boundary. Đặt tay là phá boundary — lỗi đã tái hiện được trong Postman.
2. **Không** dùng overload 2 tham số `form.Add(fileContent, "document")`. Nó không sinh `filename=`, và đó chính là bug đang sửa.
3. **Không** ghi file ra đĩa. Dựng toàn bộ trong bộ nhớ.

**Một bẫy đã biết:** `MultipartFormDataContent` của .NET sinh boundary có dấu nháy kép (`boundary="..."`). Busboy xử lý được, nhưng nếu server trả lỗi parse bất thường thì đây là chỗ nhìn đầu tiên — gỡ nháy bằng cách set lại `form.Headers.ContentType.Parameters` thủ công.

---

## 6. HttpClient và ràng buộc không được `async`

`[OSInterface]` **cấm chữ ký `async`**. Nhưng `HttpClient` vốn bất đồng bộ.

```csharp
private static readonly HttpClient _http = new HttpClient();   // static, dùng lại

// trong method đồng bộ:
var response = _http.SendAsync(request).ConfigureAwait(false)
                    .GetAwaiter().GetResult();
var body = response.Content.ReadAsStringAsync().ConfigureAwait(false)
                   .GetAwaiter().GetResult();
```

- `HttpClient` **static** — mới mỗi lần gọi là cạn socket.
- `ConfigureAwait(false)` ở **mọi** await, nếu không có nguy cơ deadlock.
- Timeout đặt trên `CancellationTokenSource` cho từng request, **không** đặt `_http.Timeout` (nó là toàn cục cho instance static).
- Ghi một comment ngay trên dòng `.GetAwaiter().GetResult()` giải thích vì sao — người sau sẽ tưởng là ẩu.

---

## 7. Thứ tự xử lý trong mỗi thao tác

```
1. Chuẩn hoá đầu vào
   TimeoutSeconds <= 0        → 60
   BaseUrl bỏ dấu / ở cuối

2. Xác định content type
   ContentType != ""          → dùng nguyên
   ngược lại                  → ContentTypeMap.Resolve(FileName)
   không map được             → trả UNSUPPORTED_FILE_TYPE, DỪNG, không gọi API

3. Kiểm kích thước
   Document.Length > 5_242_880  → trả PAYLOAD_TOO_LARGE, DỪNG, không gọi API

4. Kiểm đầu vào rỗng
   Document rỗng / Name rỗng / TemplatesCsv rỗng / AccessToken rỗng
                                → trả MISSING_INPUT:<tên>, DỪNG

5. Dựng request  (§5)
6. Gắn header    (§8)
7. Gửi, đo thời gian
8. Trả IdpUploadResult
```

Bước 2, 3, 4 chạy **trước khi** chạm mạng. Mỗi lời gọi thật tốn một trang quota của tenant và không hoàn lại — chặn sớm là tiết kiệm thật.

Trần 5,5 MB lấy tròn **5.242.880 byte** (5 MiB) cho an toàn, vì phần overhead của multipart cũng tính vào payload.

---

## 8. Header — ba dòng, bắt buộc

```csharp
request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + AccessToken);
request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
```

> **`User-Agent` không phải tuỳ chọn.** Thiếu nó, Cloudflare trả `403` kèm body `error code: 1010`. Không có trong Swagger, và triệu chứng nhìn hệt như sai mật khẩu.

Dùng `TryAddWithoutValidation` cho `User-Agent` vì chuỗi giả trình duyệt không qua được validator mặc định của .NET.

---

## 9. Bảng content type — một bản duy nhất

```csharp
public static class ContentTypeMap
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"]  = "application/pdf",
        [".png"]  = "image/png",
        [".jpg"]  = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".tif"]  = "image/tiff",
        [".tiff"] = "image/tiff",
    };
    public static string? Resolve(string fileName) =>
        Map.TryGetValue(Path.GetExtension(fileName ?? ""), out var v) ? v : null;
}
```

**Cấm `application/octet-stream` làm giá trị mặc định.** Đó là lựa chọn tự nhiên nhất và cũng chính là thứ server từ chối. Không map được thì báo lỗi, không đoán.

---

## 10. Hợp đồng lỗi

| Tình huống | `TransportOk` | `HttpStatus` | `ErrorMessage` |
|---|---|---|---|
| API trả `201` | `true` | 201 | `""` |
| API trả `400` / `500` | `true` | mã thật | `""` — body nằm ở `RawJson` |
| Đuôi file lạ | `false` | 0 | `UNSUPPORTED_FILE_TYPE: .xlsx` |
| Quá 5 MiB | `false` | 0 | `PAYLOAD_TOO_LARGE: 6291456 bytes > 5242880` |
| Đầu vào rỗng | `false` | 0 | `MISSING_INPUT: Document` |
| Timeout | `false` | 0 | `TIMEOUT: 60s` |
| DNS / TLS / reset | `false` | 0 | `TRANSPORT: <ex.GetType().Name>: <ex.Message>` |

**`RawJson` luôn là body nguyên văn, kể cả khi lỗi.** Không format lại, không parse, không cắt. ODC cần nó để đọc `errorCode` — API này trả lỗi nghiệp vụ kèm HTTP 200.

Không bao giờ ném exception ra khỏi method `[OSAction]`. Bắt hết, map thành `IdpUploadResult`.

---

## 11. Logging

Dùng `ILogger` do SDK tiêm vào. Mỗi lời gọi ghi **đúng một dòng**:

```
IdpUpload.SubmitSync name={Name} bytes={len} contentType={ct} status={code} elapsedMs={ms}
```

**Tuyệt đối không ghi:** `AccessToken`, nội dung `Document`, `RawJson`. `RawJson` chứa dữ liệu chứng từ và có thể có PII.

---

## 12. Test

### 12.1 Không chạm mạng — phải xanh trước khi tiêu trang quota nào

| Test | Kỳ vọng |
|---|---|
| `ContentTypeMap.Resolve("a.PNG")` | `image/png` — không phân biệt hoa thường |
| `ContentTypeMap.Resolve("a.xlsx")` | `null` |
| `SubmitSync` với `.xlsx` | `UNSUPPORTED_FILE_TYPE`, **không có lời gọi HTTP nào** |
| `SubmitSync` với 6 MB | `PAYLOAD_TOO_LARGE`, **không có lời gọi HTTP nào** |
| `SubmitSync` với `Document` rỗng | `MISSING_INPUT: Document` |

### 12.2 Kiểm hình dạng body — quan trọng nhất

Dựng `MultipartFormDataContent`, đọc ra chuỗi, khẳng định:

- chứa `name="document"; filename="` ← **đây là bài test tồn tại của cả library**
- part `document` có dòng `Content-Type: image/png`
- part `name` và `templates` **không** có dòng `Content-Type`
- header của request **không** có `Content-Type` do mình đặt tay

Test này chạy offline và bắt được đúng cái bug đang sửa. Viết nó trước khi viết implementation.

### 12.3a Chạm mạng, không cần Docker — đường mặc định

Dựng một HTTP server giả chạy thẳng trên máy, in ra **nguyên văn cái đi trên dây**. Không tiêu quota của tenant 177, không cần container.

```python
# mock_idp.py — chạy: python mock_idp.py
# rồi gọi library với BaseUrl = http://localhost:8099
from http.server import BaseHTTPRequestHandler, HTTPServer

class H(BaseHTTPRequestHandler):
    def do_POST(self):
        n = int(self.headers.get('Content-Length', 0))
        body = self.rfile.read(n)
        print('=== HEADERS ===')
        for k, v in self.headers.items():
            print(f'{k}: {v}')
        print('=== BODY (2000 byte đầu) ===')
        print(body[:2000].decode('utf-8', 'replace'))
        self.send_response(201)
        self.send_header('Content-Type', 'application/json')
        self.end_headers()
        self.wfile.write(b'{"requestId":"99999","pageCount":1}')
    def log_message(self, *a): pass

HTTPServer(('127.0.0.1', 8099), H).serve_forever()
```

Khẳng định trên output:

- `Content-Disposition: form-data; name="document"; filename="..."` ← **điều kiện sống còn**
- part `document` có dòng `Content-Type: image/png`
- part `name` và `templates` **không** có dòng `Content-Type`
- header request có `Authorization: Bearer`, `User-Agent` giả trình duyệt, `Accept`
- `Content-Type` của request là `multipart/form-data; boundary=...` do .NET sinh

### 12.3b Bản container — chỉ khi máy đã có Docker

`outsystems-custom-code` mô tả chạy integration test trong container khớp môi trường ODC, base image `mcr.microsoft.com/dotnet/aspnet:8.0`. Tốt hơn về độ trung thực môi trường, nhưng **không bắt buộc** và không nằm trên đường găng. Bỏ qua nếu chưa có Docker.

---

## 13. Build, đóng gói, deploy

```bash
dotnet restore
dotnet build -c Release
dotnet test                       # 12.1 + 12.2 phải xanh
dotnet publish -c Release -o ./publish
```

Đóng gói theo đúng cách `outsystems-custom-code` mô tả — đọc skill đó trước khi tự nghĩ ra cấu trúc zip.

Deploy qua **outsystems MCP**, ba tool theo thứ tự:

```
extlib_upload    → đẩy gói lên tenant
extlib_status    → chờ tới khi xử lý xong
extlib_publish   → publish để ClaimMVP tham chiếu được
```

Không dùng Docker ở bước nào trong mục này.

**Ở bước 1 của kế hoạch, deploy thử một library rỗng trước.** Nếu quyền hoặc quy trình deploy có vấn đề thì biết ngay trong giờ đầu, không phải sau khi viết xong.

---

## 14. Nối vào ODC — không đổi chữ ký

Sau khi library có trên tenant, `IdpSubmitSync` và `IdpSubmitDocument` **giữ nguyên chữ ký hiện tại**. Chỉ thay ruột: bỏ lời gọi `IdpSubmitSyncMethod` / `IdpSubmitMethod`, gọi library thay vào.

Giá trị truyền vào lấy từ Settings đang có: `IdpBaseUrl`, `IdpUserAgent`, `IdpTemplateCsv`. Token lấy từ `IdpGetToken`.

Kết quả trả về: `RawJson` đưa thẳng vào `IdpParseExtraction` — **không đổi gì trong parser**.

Hai REST method `IdpSubmitSyncMethod` và `IdpSubmitMethod` sau đó thành không dùng. **Để nguyên, đừng xoá** — nếu đội IDP sửa phía họ (§12 của BRD) thì chỉ cần đổi ruột ngược lại là xong.

---

## 15. Cổng nghiệm thu

Chiếu đúng A1–A8 của BRD.

| # | Tiêu chí | Quota |
|---|---|---|
| A4 | `.xlsx` → `UNSUPPORTED_FILE_TYPE`, không gọi API | 0 |
| A5 | 6 MB → `PAYLOAD_TOO_LARGE`, không gọi API | 0 |
| — | Body có `filename=` (§12.2) | 0 |
| A6 | Thiếu `User-Agent` → `403` trả về nguyên vẹn | 0 (container) |
| **A1** | `SubmitSync` với `6093_vnvision-giay-ra-vien.png` → `201` | **1 trang** |
| A2 | `RawJson` có `templates_data` với `templateId` = **6093** | — |
| A3 | `ElapsedMs` trong khoảng 13.000–19.000 | — |
| A7 | Không có chuỗi token nào trong log ODC Portal | 0 |
| A8 | `Submit` trả `requestId` và `pageCount` | **1 trang** |

Bốn cổng đầu và A6, A7 **phải xanh trước** khi chạy A1.

---

## 16. Cấm — đọc lại trước khi commit

- Không parse `RawJson`. Không đọc `errorCode`. Không map sang structure nghiệp vụ.
- Không gọi `/auth/login`. Library không biết username/password.
- Không retry, không backoff, không circuit breaker. Những thứ đó ở ODC.
- Không ghi entity, không ghi file, không cache.
- Không đụng 13 REST method còn lại của ODC.
- Không thêm dependency ngoài `OutSystems.ExternalLibraries.SDK` và thư viện test.
- Không commit, không push, không mở PR nếu chưa được yêu cầu.

---

## 17. Dữ liệu tham chiếu

| | |
|---|---|
| Base URL | `https://idp-api.idpstg.bnksolution.com` |
| Tenant | 177 · user 204 · `roleId 2` · partner 40 · hết hạn 31/10/2026 |
| Templates CSV | `6090,6091,6092,6093,6094,6095,6096,6097,6098` |
| User-Agent | `Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36` |
| TTL token | 48 giờ |
| File test | `insurance BNK\PMOInsure\PMOInsure\Claude outputs\6093_vnvision-giay-ra-vien.png` — 13.021 byte, giả lập, không PII |

**Request chạy được, bắt từ Swagger UI của IDP ngày 25/09 — hình dạng phải tái tạo cho bằng được:**

```
POST /request/sync
Authorization: Bearer <token>
accept: */*
Content-Type: multipart/form-data; boundary=<tự sinh>

-F 'name=TEST-DIAG'
-F 'templates=6090,6091,6092,6093,6094,6095,6096,6097,6098'
-F 'document=@6093_vnvision-giay-ra-vien.png;type=image/png'
```
