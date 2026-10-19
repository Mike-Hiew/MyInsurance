# Change — External Logic `IdpUpload`: nộp chứng từ multipart có `filename=`

**Trạng thái:** 🟡 Đang làm · **Mở:** 28/09/2026 · **Điểm cắt: hết 29/09** — chưa qua bước 4 thì dừng, revert, chạy `ReplayMode`
**Nguồn:** BRD + SRD `IdpUpload` ngày 25/09 (dán vào phiên Claude Code 28/09)
**Code:** `extension/basic_template/IdpUpload/` + `IdpUpload.Tests/` (mock IDP nằm trong test, `MockIdpServer.cs`) · zip deploy: `extension/basic_template/dist/IdpUpload.zip`
**Delta spec:** `specs/idp-integration/spec.md` trong chính thư mục này — quirk thứ 7, bất biến mới, chữ ký chốt từ mã đã Published, và 5 chỗ lệch SRD có chủ ý.
**Quan hệ:** thay ruột `IdpSubmitSync` / `IdpSubmitDocument` của `idp-integration-live`; **giữ nguyên chữ ký**. Không đụng parser, không đụng logic tiền, không đụng 13 REST method khác.

## Vì sao

ODC REST Consume không sinh được `filename=` trên part binary. Busboy phía IDP coi part không có `filename=` là text field → `req.file` undefined →
`500 Cannot read properties of undefined (reading 'path')`. Năm đường thuần ODC đã thử và đóng (BRD §2).

## Hợp đồng (bất biến trong change này)

- Library là **một cái ống**: không parse `RawJson`, không token, không retry, không ghi entity/file, không cache.
- `TransportOk` tách khỏi `HttpStatus`. `RawJson` là body nguyên văn kể cả khi lỗi.
- Chặn trước khi chạm mạng: `UNSUPPORTED_FILE_TYPE`, `PAYLOAD_TOO_LARGE` (> 5 242 880 byte), `MISSING_INPUT:<tên>`.
- Không bao giờ log `AccessToken`, `Document`, `RawJson`.
- Mọi lỗi từ library → ODC chuyển `EXTRACTION_ERROR`, **không bao giờ** `Declined` (CLAUDE.md §2.4).

## Tasks

### 1. Chuẩn bị
- [x] outsystems MCP đăng nhập `bnk.outsystems.dev`
- [x] **Lệch BRD §5 có chủ đích — target `net10.0`, không phải .NET 8** (PO duyệt 28/09). Tài liệu ODC `upgrade-net`: .NET 10 có trên ODC từ 22/06/2026; **.NET 8 hết hỗ trợ 10/11/2026** và Portal gắn cảnh báo. Đổi trước lần upload đầu nên không tốn thêm version. Cài SDK 10.0.401 bằng winget; SDK 8 vẫn giữ trên máy
- [x] SDK NuGet mới nhất ổn định: **1.7.0** (SRD ghim 1.5.0 → dùng 1.7.0 theo SRD §3)
- [x] Docker daemon **tắt** → §12.3 thay bằng mock server TCP trong tiến trình test (BRD §11)
- [x] Dependency ngoài SDK: `Microsoft.Extensions.Logging.Abstractions` 10.0.12 — bắt buộc cho `ILogger` (SRD §11), SDK không kéo theo

### 2. Library
- [x] `ContentTypeMap` — một bảng duy nhất, không `octet-stream`
- [x] `MultipartBuilder` — part `document` có `filename=`, part text không có `Content-Type`
  - Phát hiện: .NET 8 sinh `name=document; filename=a.png` **không nháy** + `filename*=`. Đặt lại tay cho giống curl: `name="document"; filename="a.png"`
- [x] `IIdpUpload` / `IdpUpload` / `IdpUploadResult` (namespace `BnK.IdpUpload`)
- [x] HttpClient static, timeout qua `CancellationTokenSource`, `ConfigureAwait(false)`
  - Lệch SRD §7 có chủ đích: kiểm `MISSING_INPUT` **trước** content type (thêm `FileName`, `BaseUrl`). Mọi kiểm vẫn trước khi chạm mạng

### 3. Test offline (0 quota)
**45/45 xanh trên net10.0 (28/09).**
- [x] ContentTypeMap
- [x] Validation: `.xlsx`, 6 MB, `Document` rỗng — không có lời gọi HTTP (A4, A5) — mock server đếm 0 request
- [ ] **A5 trên smoke thật + kiểm biên** (case 5 và 6 đã thêm vào `IdpUpload.Smoke/Program.cs` ngày 28/09): `5 MiB + 1 byte` → `PAYLOAD_TOO_LARGE: 5242881 bytes > 5242880`, `TransportOk=False`, `HttpStatus=0`, **mock không nhận request** · `đúng 5 MiB` → **201**, tức trần là `<=` chứ không off-by-one. Chạy: `python mock_idp.py` rồi `dotnet run --project IdpUpload.Smoke`
- [x] Hình dạng body có `name="document"; filename="` (§12.2)
- [x] Mock server local: đúng 3 header + 3 part; thiếu `User-Agent` → 403 trả nguyên vẹn (A6)
- [x] Log một dòng, không token, không RawJson (A7 mức library)
- [x] **Smoke cục bộ (SRD §12.3a), 28/09:** `python mock_idp.py` (cổng 8099) + `dotnet run --project IdpUpload.Smoke`, file 6093 thật 13 021 byte.
  Mock thấy trên dây: 3 header (`Authorization`, `User-Agent`, `Accept`), boundary tự sinh, 3 part — `name`/`templates` không Content-Type,
  `document` có `Content-Type: image/png` + `filename="6093_vnvision-giay-ra-vien.png"`, đủ 13 021 byte → 201.
  Thiếu UA → 403 `error code: 1010` về nguyên vẹn · `.xlsx` → chặn, mock **không** nhận request · `Submit` → `/request`, `requestId` + `pageCount`.

### 4. Deploy lên tenant — **cổng của điểm cắt**
- [x] `dotnet publish` → zip nội dung (không `.pdb`) → `dist/IdpUpload.zip`, 73 KB, `deps.json` runtimeTarget `.NETCoreApp,Version=v10.0`
- [x] Upload qua **ODC Portal** (MCP HTTP chỉ nhận base64 inline — zip 95 KB b64, không đẩy tin cậy được) → publish
  **Đã Published, kiểm trên tenant 28/09:** `IdpUpload` · `f8055c8d-da68-abe0-fa18-66fe5b6bd0a8` · rev 1 · `2026-09-28T03:37:05Z` · **0 validation message** · phơi `SubmitSync`, `Submit`, `IdpUploadResult`.
- [x] **Đối chiếu mốc zip ↔ bản trên tenant:** `dist/IdpUpload.zip` = `03:32:49Z`, mọi `.cs` ≤ `03:16:51Z`, upload `03:37:05Z` ⇒ **khớp, không cần build lại**
- [ ] Tham chiếu từ ClaimMVP — **chưa làm**, ClaimMVP hiện chưa thấy library

### 5. Nối vào ODC (làm tay trong Studio / Mentor có review)
- [x] **Chốt 9 biểu thức (28/09, đọc Settings trong Portal + callback trong Studio — XRE không thấy cả hai):**
  `BaseUrl`=`Settings.IdpBaseUrl` · `AccessToken`=`Token` (KHÔNG `"Bearer "`) · `UserAgent`=`Settings.IdpUserAgent` ·
  `Name`=`ClaimNo` · `TemplatesCsv`=`Settings.IdpTemplateCsv` (**số ít**) · `Document`=`Document` ·
  `FileName`=`FileName` (input mandatory **đang chết**, không ai truyền đi đâu — đó chính là hình dạng của bug) ·
  `ContentType`=`""` · `TimeoutSeconds`=`55` · `Submit` thêm `RequestGroupId`=`""`
- [x] **Đóng 3 ngộ nhận:** (1) Server Action **không có** property timeout — `TimeoutinSeconds: 60` là của REST method và hết tác dụng ngay khi thôi gọi method đó ⇒ `TimeoutSeconds` của library là đồng hồ duy nhất. (2) `IdpDocuments/OnBeforeRequest` **có** set `User-Agent` từ `Settings.IdpUserAgent` và override BaseURL từ `Settings.IdpBaseUrl` — báo cáo MCP nói phần này đã bị dọn là **sai**, XRE không phát ra thân callback. (3) Khớp suy luận độc lập: lỗi ODC là `500 reading 'path'` (busboy, origin) chứ không `403 1010` (Cloudflare, biên) ⇒ UA đã được gửi
- [ ] ⚠️ **Bỏ REST method là bỏ luôn `OnBeforeRequest`** — base URL override và header `User-Agent` biến mất im lặng. Phải truyền tay, đó là lý do library nhận 2 tham số này
- [ ] `IdpSubmitSync` + `IdpSubmitDocument` gọi library, giữ chữ ký; `RawJson` → `IdpParseExtraction` không đổi
- [ ] Hai REST method `IdpSubmitSyncMethod` / `IdpSubmitMethod` **để nguyên, không xoá**

### 6. Nghiệm thu (tốn quota — chỉ sau khi 3.x xanh)
- [ ] A1 `SubmitSync` file 6093 → 201 (1 trang)
- [ ] A2 `templateId` = 6093 · A3 `ElapsedMs` 13–19 s
- [ ] A7 không có token trong log Portal
- [ ] A8 `Submit` → `requestId`, `pageCount` (1 trang)

## Giới hạn khai báo
Trần 5,5 MB/lời gọi so với D3 §I-07 (100 MB/hồ sơ). Là giới hạn của giải pháp tạm; biến mất nếu đội IDP nhận part không có `filename=` (BRD §12).
