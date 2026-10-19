# Change — Chuyển nộp chứng từ từ REST Consume sang External Library `IdpUpload`

**Trạng thái:** 🟢 **đã publish rev 178** · verify: `IdpSubmitDocument` xong, `IdpSubmitSync` còn treo · **Mở:** 28/09/2026
**App:** `ClaimMVP` (`2a401c4d-d87e-4c44-8692-9ad661840125`), rev khởi điểm **177**
**Library:** `IdpUpload` (`f8055c8d-da68-abe0-fa18-66fe5b6bd0a8`, ExternalLibrary rev 1,
published 28/09, 0 validation message)

## Vì sao

ODC REST Consume không đặt được token `filename=` trên phần binary của multipart, và parser
phía IDP chỉ coi một part là file khi token đó có mặt. `IdpUpload` (C# external logic) tự dựng
body multipart nên đặt được `filename=`.

## Phạm vi — chỉ đổi phần thân của hai Server Action

- `IdpSubmitSync`: thay node gọi `IdpSubmitSyncMethod` bằng `IdpUpload.SubmitSync`
- `IdpSubmitDocument`: thay node gọi `IdpSubmitMethod` bằng `IdpUpload.Submit`

**Không** đổi input/output của hai action đó · **không** chạm `IdpParseExtraction` · **không**
xoá/đổi tên `IdpSubmitSyncMethod` và `IdpSubmitMethod` (để lại, không dùng, làm bằng chứng) ·
**không** tạo Setting mới · **không** chạm Entity, Structure, Timer, screen, A5Step*, logic tiền.

## Hệ quả phải nhớ

Khi hai action thôi gọi REST method, **`OnBeforeRequest` của `IdpDocuments` không chạy nữa**.
Nó vốn đặt hai thứ: ghi đè BaseURL bằng `Settings.IdpBaseUrl`, và thêm header
`User-Agent = Settings.IdpUserAgent`. Cả hai biến mất **im lặng**. Đó chính là lý do library
nhận `BaseUrl` và `UserAgent` làm tham số tường minh. **Truyền chuỗi rỗng cho một trong hai là
lỗi, không phải mặc định** — thiếu User-Agent thì Cloudflare trả 403 `error code: 1010`.

## Giá trị Setting (đọc tay trong Studio/Portal — XRE không đọc được)

| Setting | Giá trị |
|---|---|
| `IdpBaseUrl` | `https://idp-api.idpstg.bnksolution.com` |
| `IdpUserAgent` | `Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36` |
| `IdpTemplateCsv` | `6090,6091,6092,6093,6094,6095,6096,6097,6098` |

## Map input

Chung cho cả hai: `BaseUrl = Settings.IdpBaseUrl` · `AccessToken = Token` (local var đang có,
**bỏ tiền tố `"Bearer "`** — library tự thêm) · `UserAgent = Settings.IdpUserAgent` ·
`Name = ClaimNo` · `TemplatesCsv = Settings.IdpTemplateCsv` (số ít `Template`) ·
`Document = Document` · `FileName = FileName` (input mandatory đang bị nhận rồi bỏ không dùng;
truyền thẳng, **không** tự thêm phần mở rộng) · `ContentType = ""` · `TimeoutSeconds = 55`.
Riêng `IdpSubmitDocument` thêm `RequestGroupId` — dùng `GroupId` nếu action đã có, ngược lại `""`.

## Map kết quả — dùng đúng output đang có, không thêm/đổi tên/đổi type

| Điều kiện | Xử lý |
|---|---|
| `TransportOk = False` | Chưa tới IDP. Lỗi kỹ thuật ⇒ `EXTRACTION_ERROR`. `ErrorMessage` vào output đang chứa text lỗi. Output raw để **trống** |
| `TransportOk = True` và `HttpStatus` ∈ {200, 201} | Thành công. `RawJson` truyền **nguyên trạng** vào output đi tiếp sang `IdpParseExtraction`. Không parse/trim/reformat |
| `TransportOk = True`, `HttpStatus` khác | IDP trả lời và từ chối. Vẫn `EXTRACTION_ERROR`, **nhưng vẫn truyền `RawJson` nguyên trạng** — IDP đặt `errorCode` trong body, HTTP 200 không phải thứ duy nhất đáng đọc |

**Tuyệt đối không** map lỗi thành `Declined`, hay bất kỳ quyết định / số tiền / trạng thái hồ sơ.
Đọc không được chứng từ không phải là lý do từ chối quyền lợi.

Giữ nhánh `ExceptionHandler / AllExceptions`. Library không throw (trả `TransportOk=False`),
nhưng `IdpGetToken` và `IdpParseExtraction` vẫn throw được — handler không phải code chết.

## Log — không thương lượng

Không ghi `AccessToken`, `Document`, `RawJson` vào log, Entity, exception message hay screen.
`RawJson` chứa dữ liệu bệnh nhân. Được phép log: `ClaimNo`, `FileName`, số byte, `HttpStatus`,
`ElapsedMs`, `ErrorMessage`.

## Không có property timeout trên Server Action

Đã xác nhận: node `IServerAction` trong XRE không phát ra property timeout nào (chỉ
`CacheInMinutes: null`). Bỏ hẳn yêu cầu "đặt timeout 60 s" của bản brief trước. Con số 60 duy
nhất đọc được là `TimeoutinSeconds: 60` trên **REST method** `IdpSubmitSyncMethod` — thứ khác.

## Việc

- [x] 1. Mentor: `IdpSubmitSync` → gọi `IdpUpload.SubmitSync`, map 9 input, map kết quả
- [x] 2. Mentor: `IdpSubmitDocument` → gọi `IdpUpload.Submit`, map 10 input, map kết quả
- [x] 3. Không publish. Báo cáo element đã đổi + biểu thức từng input + validation + việc không làm được
- [~] 4. Verify sau khi publish — `IdpSubmitDocument` XONG, `IdpSubmitSync` chưa đọc được: `context_graph` root hai action, xác nhận cạnh
      `--Action-->` trỏ vào action của library và **không còn** trỏ vào `RestAction`

## Rủi ro đã biết

**Reference tới `IdpUpload` chưa kiểm được bằng MCP.** `app_refs` trên tenant này **không phơi
producer kiểu ExternalLibrary**: thử đối chứng trên `SendEmail_IS` cho ra **đúng cùng 4 producer**
(`OutSystemsCharts`, `(System)`, `OutSystemsUI`, `OutSystemsMaps`), tất cả `kinds: ["entities"]`.
Hai app khác nhau cho danh sách y hệt ⇒ kết quả âm của nó vô nghĩa. `context_actions
--owned_only false` cũng không có positive control (ClaimMVP không import action từ library nào).
**Phép thử thật là Mentor**: nếu nó không tìm thấy `IdpUpload.SubmitSync`/`.Submit` thì phải dừng
và báo, không được bịa hay thay bằng tên gần giống.

**Chuỗi `"Bearer "` chưa đọc được.** Biểu thức hiện tại là `<Text literal dài 7, hash 8311e512db>
+ Token`. Độ dài 7 khớp `"Bearer "` kể cả dấu cách, nhưng XRE băm literal nên không đọc được nội
dung. Mentor phải xác nhận trong Studio; nếu không phải `"Bearer "` thì dừng và báo.

**Tên tham số của library chưa xác minh.** `extlib_contents` chỉ trả `key`/`name`/`description`
cho `SubmitSync` và `Submit`, **không có danh sách parameter**. Chín/mười tên trong bảng map trên
lấy từ brief, chưa đối chiếu với signature thật. Mentor phải báo signature thật và dừng nếu lệch.


---

## Kết quả run Mentor 28/09 (session `1c23209b…`, run `f4632e0f…`)

`changeApplied: true` · `errorCount: 7` · `warningCount: 138` · **chưa publish**.

### Ba hard stop đều qua

1. **Reference:** Mentor xác nhận `IdpUpload` **được app tham chiếu**, cả
   `IdpUpload.SubmitSync` và `IdpUpload.Submit` gọi được. ⇒ Kết luận Stage 1 của tôi
   ("không được tham chiếu") **sai**. Nguyên nhân: `app_refs` không phơi producer kiểu
   ExternalLibrary — đối chứng `SendEmail_IS` cho ra đúng cùng 4 producer platform.
   **Rút ra: `app_refs` không phải phép thử reference tới ExternalLibrary.**
2. **Signature:** khớp brief **chính xác**, không lệch tên nào.
   `SubmitSync(BaseUrl:Text, AccessToken:Text, UserAgent:Text, Name:Text, TemplatesCsv:Text,
   Document:Binary Data, FileName:Text, ContentType:Text, TimeoutSeconds:Integer)
   → SubmitSync:IdpUploadResult`.
   `Submit(… + RequestGroupId:Text, TimeoutSeconds:Integer) → Submit:IdpUploadResult`.
   `IdpUploadResult` mang: `TransportOk`, `HttpStatus`, `RawJson`, `ErrorMessage`, `ElapsedMs`.
3. **Bearer:** literal cũ đúng là `"Bearer "` — word Bearer + một dấu cách, 7 ký tự.

### Element đã đổi

- `IdpSubmitSync`: node `CallSubmitSync` (trước gọi `IdpDocuments.IdpSubmitSyncMethod`) thay
  bằng node mới gọi `IdpUpload.SubmitSync`; thêm `IfTransportOk` + `IfHttpStatus` sau nó.
  `GetToken`, `ParseResult`, `AllExceptions` giữ nguyên.
- `IdpSubmitDocument`: node `CallSubmit` (trước gọi `IdpDocuments.IdpSubmitMethod`) thay bằng
  node mới gọi `IdpUpload.Submit`; thêm `IfTransportOk` + `IfHttpStatus`. `AllExceptions` giữ nguyên.

### Regression phải quyết trước khi publish

`IdpUploadResult` **không có** `RequestId` và **không có** `PageCount`. REST method cũ trả hai
trường đó. Vì bị cấm thêm/đổi output, `IdpSubmitDocument.RequestId` và `.PageCount` giờ **để
nguyên giá trị mặc định** (`""` và `0`) **trên mọi nhánh** — sinh 2 warning mới. Luồng async mất
`RequestId`, tức mất thứ để đối chiếu request với IDP về sau.

Và `IdpSubmitDocument` **không có output RawJson**, nên luật "truyền RawJson nguyên trạng ở nhánh
HttpStatus khác 200/201" không có đích. Mentor đặt `EXTRACTION_ERROR` + `HttpStatus`, bỏ RawJson.

Hai lựa chọn: mở rộng library để `Submit` trả request id, hoặc sửa các caller của
`IdpSubmitDocument` để không dựa vào hai output đó. **Chưa quyết.**

### 7 error

Toàn bộ là `"Unavailable feature: Multipart request format in REST APIs"` trên `IdpDocuments`,
`IdpWorkflow`, `IdpMasterData`, `IdpAuth`. Mentor khẳng định **không có cái nào do run này sinh ra**.
Số này từng là 6 ở rev 171 và 7 ở rev 177 — thay đổi xảy ra trước run này. **Chưa kiểm độc lập.**


---

## Stage 3 — patch RequestId/PageCount (run `5aa2cf89…`, cùng session `1c23209b…`)

`changeApplied: true` · `errorCount: 7` (không đổi) · `warningCount: **136**` (từ 138 — hai
warning unused-output đã hết) · **chưa publish**.

**Inventory (task 1).** Chỉ đúng hai output bị mất giá trị, cả hai trên `IdpSubmitDocument`:
`RequestId` (Text, trước lấy từ `CallSubmit.Response.Id`) và `PageCount` (Integer, trước lấy từ
`CallSubmit.Response.PageCount`). `IdpSubmitSync` **không bị**: `Result` lấy từ
`ParseResult.Result` trên cả nhánh 200/201 và nhánh khác, `Outcome` được set trên mọi nhánh kể cả
transport-fail và exception. Không có cái nào khác.

**Patch (task 2, 3).** Thêm node `DeserializeResponse` (JSON Deserialize) ngay sau `IfHttpStatus`
và trước assign thành công, **chỉ trên nhánh** `TransportOk = True` và `HttpStatus` ∈ {200, 201}:

- Structure: **`IdpDocuments.IdpSubmitResponse`** — **đã tồn tại**, không tạo mới. Quyền tạo một
  Structure không dùng đến.
- JSON input của node: `CallSubmit.Submit.RawJson`
- `RequestId = DeserializeResponse.Data.Id`
- `PageCount = DeserializeResponse.Data.PageCount`

Mọi nhánh lỗi giữ nguyên default.

**Bảy error multipart (task 5), từng cái trên method nào:**

| # | Vị trí |
|---|---|
| 1 | `IdpDocuments / RequestPart2` — multipart structure signature trên REST client `IdpDocuments` |
| 2 | `IdpWorkflow / RequestPart3` — trên REST client `IdpWorkflow` |
| 3 | `IdpMasterData / RequestPart4` — trên REST client `IdpMasterData` |
| 4 | `IdpAuth / RequestPart` — trên REST client `IdpAuth` |
| 5 | `IdpDocuments / IdpSubmitSyncMethod / Document / Content-Type in Multipart` |
| 6 | `IdpDocuments / IdpSubmitSyncMethod / Request Format` |
| 7 | `IdpDocuments / IdpSubmitMethod / Request Format` |

Bốn cái đầu là một-per-REST-client, **gồm cả `IdpAuth`** — mà `IdpAuth` chỉ gửi JSON
(`/auth/login`), không có method multipart nào. Structure `RequestPart` ở đó là **defect tiềm ẩn
có từ trước**, không do các thay đổi này sinh ra. Chưa xử lý.

**Task 4.** **Không element nào** đọc `IdpSubmitDocument.RequestId` hay `.PageCount` —
`IdpSubmitDocument` không được action nào khác trong app gọi. Nếu không patch thì cũng chưa có
caller nào gãy ngay; nhưng giá trị vẫn sai cho lần đầu có caller.

### Chưa xác minh

`RequestId` được map từ `DeserializeResponse.Data.**Id**`, trong khi brief nói body trả
`{"requestId": ..., "pageCount": ...}`. Tên field trong structure do ODC sinh là `Id`, và code
trước Stage 2 cũng đọc `Response.Id` — nên khả năng cao `NameInJSON` của nó trỏ đúng `requestId`.
**Không đọc được qua MCP.** Nếu `NameInJSON` lệch, `RequestId` sẽ về rỗng mà không báo lỗi.
Kiểm trong Studio: `IdpDocuments` → `IdpSubmitResponse` → attribute `Id` → `NameInJSON`.


---

## Stage 4 — sửa mapping RequestId (run `86d5734f…`, cùng session `1c23209b…`)

`changeApplied: true` · `errorCount: 7` (không đổi) · `warningCount: 136` (không đổi) ·
**chưa publish**. Không sinh error/warning mới.

### Mapping Stage 3 đã SAI — đọc trực tiếp từ XRE, không qua Mentor

`IdpDocuments.IdpSubmitResponse` (node `RestStructure`, attribute là `RestStructureAttribute`
có phơi `NameInJSON`):

| Attribute | `NameInJSON` | Length | Kiểu |
|---|---|---|---|
| `Id` | `id` | 100 | Text |
| `PageCount` | `pageCount` | — | Integer |
| `ErrorCode` | `errorCode` | 50 | Text |
| `ErrorMessage` | `errorMessage` | 500 | Text |

**Không có attribute nào mang JSON key `requestId`**, và `Id` là **Text** chứ không phải numeric.
`POST /request` trả `{"requestId": 21376, "pageCount": 1}` ⇒ `Data.Id` **không bao giờ bind**,
`RequestId` sẽ im lặng về rỗng. Structure này được sinh từ sample dùng key `id` — khớp
`POST /request/group` → `{"id":"4"}` — chứ không phải `POST /request`.

Lập luận "code trước Stage 2 cũng đọc `Response.Id`" **không có giá trị làm bằng chứng**: code đó
chưa bao giờ hoàn thành một `POST /request` nào, vì ODC không gửi được multipart — đó chính là lý
do library tồn tại.

### Đã sửa

**Structure mới — dùng đúng một quyền được cấp:** `IdpSubmitResponse2`, top-level app structure,
đúng hai attribute, không có field nào khác, **không** đặt `NameInJSON` override:

| Attribute | Kiểu |
|---|---|
| `RequestId` | Long Integer |
| `PageCount` | Integer |

Tên attribute khớp key `requestId` / `pageCount` theo quy ước, nên không cần override.

**Node deserialize:** `DeserializeResponse` — **repoint, không xoá**. Target đổi từ
`IdpDocuments.IdpSubmitResponse` sang `IdpSubmitResponse2`. Tên node, vị trí, JSON input
(`CallSubmit.Submit.RawJson`) và toàn bộ wiring giữ nguyên. Không để lại node chết.

**Hai assign, nhánh thành công only:**

```
RequestId = LongIntegerToText(DeserializeResponse.Data.RequestId)
PageCount = DeserializeResponse.Data.PageCount
```

Mọi nhánh lỗi giữ nguyên default. `IdpDocuments.IdpSubmitResponse` **không bị xoá/đổi tên** — nó
thuộc REST method `IdpSubmitMethod` vẫn được giữ làm bằng chứng. `IdpSubmitSync` không bị chạm
trong stage này.

### Ghi nhận phương pháp

`RestStructureAttribute` trong XRE **có** phơi `NameInJSON`. Đây là cách đọc được mapping JSON của
structure REST mà không cần mở Studio — dùng lại khi cần đối chiếu contract.


---

## Publish rev 178 + verify (28/09)

`mentor_publish` session `1c23209b…` → `publicationKey f113db81-eb7e-476a-b12d-9df445e0ca80`,
`revision 178`. `publish_status` → `outcome: success`, `status: Finished`.
`app_info` → `revision 178`, `revisionDateTime 2026-09-28T07:35:38.578807Z`,
`modelDigest 9e6d2395… → 3832c0ea-87fb-520e-dde8-2222f8998842`.

### `IdpSubmitDocument` — verify ĐẦY ĐỦ trên model đã publish

`context_graph` root=`IdpSubmitDocument` (612 node):

- `[25] IExecuteServerActionNode/CallSubmit --Action--> [64] ReferenceIServerAction/Submit` —
  gọi action của library. **Không còn cạnh nào tới `RestAction/IdpSubmitMethod`.**
- `RestAction` còn lại trong graph chỉ có `IdpLoginMethod` và `IdpUserMeMethod`, đến từ
  `IdpGetToken`/`IdpRefreshIdentity` (IdpAuth) — đúng chủ ý, không chạm.
- `Submit` có đủ 10 input theo thứ tự: `BaseUrl, AccessToken, UserAgent, Name, TemplatesCsv,
  Document, FileName, ContentType, RequestGroupId, TimeoutSeconds`.
  `AccessToken` ← `ILocalVariable/Token` (Identifier trơn, **không** BinaryOperation Plus ⇒ đã bỏ
  tiền tố `"Bearer "`). `BaseUrl` ← Identifier tới external reference (hình dạng Setting).
- `[26] IJSONDeserializeNode/DeserializeResponse`, `DateFormat: ISO`,
  `--DataType--> [79] IStructure/IdpSubmitResponse2`, `RuntimeProperty/Data` cũng kiểu đó.
- `IdpSubmitResponse2` có đúng hai attribute `RequestId`, `PageCount`, `NameInJSON = None` cả hai.
- `JSONString` = **`CallSubmit.Submit.RawJson`** (giải chuỗi ReferedObject/IdentifierPart).
- Thứ tự luồng: `CallSubmit` → `[21] IIfNode` → `[35] IIfNode` (Condition `BinaryOperation Op=Or`)
  → `[26] DeserializeResponse` → `[17] IAssignNode`. Deserialize nằm **sau cả hai cổng**.
- `[17]` (nhánh thành công) gán: `Outcome.Success`, `Outcome.StatusCode = CallSubmit.Submit.HttpStatus`,
  **`RequestId = LongIntegerToText(DeserializeResponse.Data.RequestId)`**,
  **`PageCount = DeserializeResponse.Data.PageCount`**.
- Hai nhánh lỗi chỉ gán field của `Outcome`: `[16]` transport-fail →
  `Outcome.Success/ErrorCode/ErrorMessage`; `[28]` non-200/201 →
  `Outcome.Success/ErrorCode/StatusCode`. **Không** nhánh nào gán `RequestId`/`PageCount` ⇒ giữ
  default, đúng yêu cầu.

### `IdpSubmitSync` — CHƯA VERIFY

`context_graph` root=`IdpSubmitSync` **timeout 3 lần**, không lần nào sinh file. Sau đó chạy
**control** root=`IdpGetToken` (root nhỏ, từng chạy được) — **cũng timeout**. `auth_status` trả
`logged_in: true` với token mới ⇒ không phải lỗi auth, và không riêng `IdpSubmitSync`:
**`context_graph` đang timeout cho toàn bộ app này**. Cùng root `IdpSubmitSync` chạy bình thường
lúc 13:37 cùng ngày ở rev 177, nên là tình trạng tạm thời phía dịch vụ chuyển OML→XRE (tool tự mô
tả là giữ memory budget theo replica). **Chưa có bằng chứng** cho `IdpSubmitSync` ở rev 178. Việc còn lại: chạy lại root đó và xác nhận
`CallSubmitSync --Action--> ReferenceIServerAction/SubmitSync`, không còn cạnh tới
`RestAction/IdpSubmitSyncMethod`, và 9 input mapping đúng.


### Chẩn đoán `context_graph` timeout (28/09, sau publish rev 178)

| Phép thử | Kết quả |
|---|---|
| `context_graph` ClaimMVP root=`IdpSubmitSync` | timeout **×4**, không lần nào sinh file |
| `context_graph` ClaimMVP root=`IdpGetToken` (control, từng chạy được) | **timeout** |
| `context_graph` ClaimMVP root=`FormatVND` (root nhỏ, chưa dùng lần nào) | **timeout** |
| `context_graph` **app khác** — `PizzaOrder` (`08adfdd5…`), **toàn app, không root** | **THÀNH CÔNG**, 3.2 MB |
| `auth_status` | `logged_in: true`, token mới |
| `app_revisions` ClaimMVP | rev **178** là mới nhất, `modelDigest 3832c0ea…` khớp `app_info`; tổng 178 revision |

**Kết luận: dịch vụ không sập — lỗi riêng ClaimMVP.** Mọi root của ClaimMVP đều timeout, kể cả root
nhỏ và root từng chạy; một app khác chuyển OML→XRE bình thường ngay sau đó.

Tình trạng này xuất hiện **giữa 14:37 và ~15:00**: ClaimMVP còn chạy được lúc 13:37 (rev 177,
root `IdpSubmitSync`) và 14:37 (rev 178, root `IdpSubmitDocument`).

**Giả thuyết** (khớp mô tả của chính tool, **chưa chứng minh được**): phần chuyển OML→XRE giữ một
memory budget theo replica; các call cho **cùng một app** chia sẻ/xếp hàng trên một conversion, và
một tenant chỉ được giữ tối đa nửa budget của replica. Loạt thử lại liên tiếp trên ClaimMVP có thể
để lại conversion còn bay, chiếm chỗ của chính ClaimMVP — nên mọi call ClaimMVP mới xếp hàng rồi
chết ở client timeout, trong khi app khác là conversion khác nên đi qua.

**Hệ quả thực hành: ngừng gọi một lúc.** Thử lại liên tục là thứ duy trì tình trạng này.
