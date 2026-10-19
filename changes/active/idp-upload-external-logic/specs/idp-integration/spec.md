# DELTA — `specs/idp-integration` · đường nộp chứng từ đổi sang External Logic

> Đoạn khác biệt, không phải spec đầy đủ. Merge vào Project doc `build-02` khi archive.
> Mở 28/09/2026. Bằng chứng: smoke cục bộ 28/09 (`extension/basic_template/`, mock cổng 8099) + library đã Published trên tenant.

## Quirk THỨ BẢY — thêm vào danh sách sáu quirk

> **7. Part `document` phải có `filename=` trong `Content-Disposition`.** Busboy (multer) phía IDP quyết một part là *file* hay *text field* **chỉ bằng sự có mặt của `filename=`**. `Content-Type` của part không tham gia quyết định đó. Part không có `filename=` → `req.file` undefined → `requestService.create()` đọc `file.path` trên undefined → **HTTP 500 `Cannot read properties of undefined (reading 'path')`**.

Hệ quả: **`IRestActionInput` của ODC không phơi thuộc tính `FileName`** ⇒ multipart do ODC dựng không bao giờ có `filename=` ⇒ **REST Consume không nộp được chứng từ lên IDP**. Đây là một giới hạn của nền tảng, không phải lỗi cấu hình.

Đã thử và đóng, mỗi đường một bằng chứng (BRD §2):

| Đường | Kết luận | Bằng chứng |
|---|---|---|
| Builder multipart của ODC | Đóng | Không có `FileName` trên `IRestActionInput` |
| Chèn `filename=` qua `OriginalName` | Đóng | ODC escape lúc chạy |
| Tự ghép body trong ODC | Đóng | Không nối được BinaryData; không có encoding giữ nguyên byte (chỉ unicode/utf-8/utf-16/ascii) |
| `fetch` + `FormData` phía trình duyệt | Đóng | CORS preflight bị chặn; **không** có `securitypolicyviolation` ⇒ không phải CSP |
| `POST /request/from-url` | Đóng | Hỏng ở server: `400 "The specified key does not exist."` |
| **External Logic C#** | **Chọn** | Đã build, 45/45 test xanh, smoke thấy đúng `filename=` trên dây |

## Bất biến MỚI

> **Mọi byte chứng từ đi lên IDP đều đi qua External Logic `IdpUpload`. Không Server Action nào tự dựng multipart.**

> **`IdpUpload` là một cái ống.** Không parse `RawJson`, không lấy/làm mới token, không retry, không ghi Entity, không ghi file, không cache. Mọi phán đoán ở lại ODC. Lý do: phán đoán trong C# là phán đoán không nhìn thấy được từ Service Center, và không chạy lại được bằng `ReplayMode`.

> **`TransportOk` tách khỏi `HttpStatus`.** `TransportOk=True` nghĩa là *IDP đã trả lời*, kể cả khi trả 403 hay 500. `RawJson` là body **nguyên văn**, kể cả khi lỗi — ODC đọc `errorCode` trong đó (quirk 3 vẫn nguyên giá trị).

> **Mọi lỗi từ library → `EXTRACTION_ERROR`, không bao giờ `Declined`.** (CLAUDE.md §2.4) Không đọc được chứng từ không phải là căn cứ từ chối quyền lợi.

> **Không bao giờ log `AccessToken`, `Document`, `RawJson`.** `RawJson` mang PII bệnh án. Log đúng một dòng mỗi lời gọi: operation, name, bytes, contentType, status, elapsedMs, và mã lỗi nếu có.

## Giới hạn MỚI phải khai trong deck

**Trần 5 242 880 byte (5 MiB) cho mỗi lời gọi**, chặn ở library **trước khi chạm mạng** — mỗi lời gọi thật tốn một trang quota không hoàn lại. Trần thật của IDP là ~5,5 MB/lời gọi; 5 MiB chừa chỗ cho overhead multipart.

Đối chiếu D3 §I-07 (100 MB mỗi hồ sơ): **không xung đột trong phạm vi bản dự thi**, vì đường bất đồng bộ đã gửi **một file mỗi lời gọi**, nên trần này là trần *mỗi chứng từ*, không phải mỗi hồ sơ.

## Chữ ký — chốt từ mã đã Published, không phải từ dự định

`IdpUpload` · `f8055c8d-da68-abe0-fa18-66fe5b6bd0a8` · rev 1 · `2026-09-28T03:37:05Z` · 0 validation message.
`dist/IdpUpload.zip` mtime `2026-09-28T03:32:49Z`, mọi file nguồn ≤ `03:16:51Z` ⇒ **zip khớp bản trên tenant, không lệch.**

```
SubmitSync(BaseUrl, AccessToken, UserAgent, Name, TemplatesCsv,
           Document, FileName, ContentType, TimeoutSeconds) → IdpUploadResult      // POST /request/sync
Submit    (BaseUrl, AccessToken, UserAgent, Name, TemplatesCsv,
           Document, FileName, ContentType, RequestGroupId, TimeoutSeconds)        // POST /request
IdpUploadResult { TransportOk : Boolean, HttpStatus : Integer,
                  RawJson : Text, ErrorMessage : Text, ElapsedMs : Integer }
```

Quy ước tham số, đã kiểm trên dây:
- `AccessToken` **không** kèm chữ `Bearer` — library tự gắn.
- `TemplatesCsv` là chuỗi ngăn cách phẩy (quirk 2 vẫn nguyên giá trị), ví dụ `6090,6091,…,6098`.
- `ContentType` rỗng ⇒ suy từ đuôi `FileName`. **Không có fallback `application/octet-stream`** — đuôi lạ trả `UNSUPPORTED_FILE_TYPE` và không chạm mạng.
- `UserAgent` rỗng ⇒ **không gắn header** ⇒ 403 `error code: 1010` đi về ODC nguyên vẹn. Cố tình: để lỗi cấu hình hiện ra thay vì bị library đoán hộ.
- `TimeoutSeconds` ≤ 0 ⇒ 60. Trần Server Action của ODC là 10 s mặc định / 60 s tối đa ⇒ `SubmitSync` (13–19 s thực đo) **bắt buộc** chạy trong Server Action đã nâng timeout.

## Lệch so với SRD, có chủ ý và đã duyệt

| Chỗ | SRD viết | Thực làm | Lý do |
|---|---|---|---|
| Target framework | `net8.0` | **`net10.0`** | .NET 8 hết hỗ trợ trên ODC 10/11/2026 và Portal gắn cảnh báo; đổi trước lần upload đầu nên không tốn version |
| SDK NuGet | ghim 1.5.0 | **1.7.0** | theo SRD §3 "bản ổn định mới nhất" |
| Thứ tự kiểm | content type trước | **`MISSING_INPUT` trước** (thêm `FileName`, `BaseUrl`) | mọi kiểm vẫn trước khi chạm mạng; thông báo lỗi chỉ đúng chỗ hơn |
| Test tích hợp | Testcontainers | **mock server TCP trong tiến trình** + `mock_idp.py` | Docker daemon tắt; mock đếm được số request, thứ Testcontainers không cho |
| `Content-Disposition` | mặc định .NET | **đặt lại tay có nháy** | .NET sinh `name=document; filename=a.png` không nháy kèm `filename*=`; đặt lại cho giống curl |

## Chỗ cần đóng khi archive — KHÔNG sửa `specs/` bây giờ

`specs/idp-integration/spec.md` đang ghi **tenant 176 · document set 194 · flow `5ed8e4fc`**. Mọi bằng chứng chạy thật của change này và của `idp-test-thuc-te-tenant-177-2026-09-17` là **tenant 177 · set 198 · template 6090–6098**. Hai bản đang lệch.

Không sửa `specs/` ở đây vì quy ước: spec chỉ đổi khi archive. Nhưng **phải chốt trước pitch** — một con số tenant sai trong spec là đúng loại lỗi mà thư mục này được lập ra để chặn (`build-00` §4 #7 đã xảy ra một lần rồi).
