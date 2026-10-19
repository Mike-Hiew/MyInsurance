# Change — Harness gọi `IdpParseExtraction` offline, không tốn quota

**Trạng thái:** 🟢 Part 1 **đã publish rev 179**, harness chạy được · Part 2 chờ capture · **Mở:** 28/09/2026
**App:** `ClaimMVP` (`2a401c4d-d87e-4c44-8692-9ad661840125`), rev khởi điểm **178**

## Vì sao làm harness trước, sửa parser sau

`IdpParseExtraction(RawJson, IsSyncShape)` là **hàm thuần trên một chuỗi text** — không cần mạng,
không cần gọi IDP. Nhưng hiện chỉ có một cách chạy nó: bấm nút diagnostics, và **mỗi lần bấm là
mất một trang quota tenant, không hoàn lại**. Tenant 177 đang `blockLimit 1`, đã dùng 14 trang.

Nên: dựng đường gọi parser offline trước, rồi mới sửa parser dựa vào nó. **Không đảo thứ tự, và
không "sửa thử rồi bấm nút thật để xem".**

## Phạm vi

Được: thêm **một** section vào screen diagnostics đang có (Part 1) · đổi phần thân của
`IdpParseExtraction` (Part 2).

**Không:** đổi input/output của `IdpParseExtraction` · chạm `IdpSubmitSync`, `IdpSubmitDocument`,
`IdpUpload`, hay bất kỳ REST method · chạm `A5Step*`, trường tiền, trạng thái hồ sơ · tạo Entity ·
**gọi IDP thật, vì bất kỳ lý do gì, ở bất kỳ thời điểm nào**.

## Part 1 — section "Parse a captured response"

Thêm vào screen diagnostics đang có, **dưới** phần IDP diagnostics hiện tại:

- Text input **nhiều dòng, ≥ 10 rows**, bind vào local var `RawJsonInput`
- Checkbox bind `IsSyncShapeInput`, **default TRUE**, label
  `Two-level result.result shape (POST /request/sync)`
- Nút **"Run IdpParseExtraction"** — gọi `IdpParseExtraction(RawJsonInput, IsSyncShapeInput)`
  **và không gì khác**: không gọi IDP, không ghi entity
- Hiển thị **mọi scalar** của `IdpExtractionResult` trả về: `RequestId`, `PageCount`, `ErrorCode`,
  `ErrorMessage`, `ProcessingTime`, `MinioPath`, `ClaimConfidence`, `Completeness`, và **số lượng**
  `Templates`
- Với **từng** template trong `Templates`: `TemplateId`, `TemplateName`, `SubDocumentType`,
  `Confidence`, số `Fields`, số `Tables`
- Bảng **mọi field**: `K`, `V`, `C`, `CLlm`, `PageNumber` — **scroll, không phân trang**, để so
  cùng lúc với JSON gốc

Section này **không bao giờ gọi IDP**. Thêm nó tốn **0 quota**, và nó ở lại trong app làm harness
regression cho mọi shape response sau này.

## Đối chiếu shape — các attribute cần hiển thị đều đã tồn tại

`IdpExtractionResult`: `PageCount`, `Completeness`, `MinioPath`, `RequestId`, `ProcessingTime`,
`Templates` (IdpTemplateData List), `RawJson`, `ErrorMessage`, `ClaimConfidence`, `ErrorCode`.
`IdpTemplateData`: `Tables` (IdpTable List), `Confidence`, `TemplateId`, `Fields` (IdpField List),
`SubDocumentType`, `TemplateName`. `IdpField`: `MatchW`, `PageNumber`, `C`, `V`, `K`, `CLlm`.

⇒ Không cần tạo Structure hay Entity nào cho Part 1.

## Part 2 — chỉ sau khi PO nói "go" và đã dán capture thật vào harness

Chẩn đoán **trước khi sửa**, đúng thứ tự: (1) `IdpSubmitSync` truyền gì cho `IsSyncShape` — trích
nguyên biểu thức; nếu nó là False thì riêng điều đó đã giải thích triệu chứng. (2) Trong
`IdpParseExtraction`, trích nguyên biểu thức đi xuống object `result` cho **cả hai** shape.
(3) Nói rõ cái nào sai — cờ của caller, hay đường đi xuống của parser. **Không sửa cả hai theo
phỏng đoán.**

Luật từ `build-02`, không diễn giải lại:

- `POST /request/sync` lồng payload ở `result.result` — **hai** tầng.
  `GET /request/{id}/result` lồng ở `result` — **một** tầng.
  `GET /request/{id}/check-status` đặt dưới `data.result` khi Completed.
  **Một parser xử cả ba. Không viết ba parser.**
- `fields[].c` thang **0–1**. `data.confidence` thang **0–100**. Lưu cả hai **chuẩn hoá về 0–1**.
- Field có `v = ""` là **RỖNG**, và vẫn về kèm `c = 1`. Map `""` → null, **không** tính là có mặt.
  Trong capture có đúng một field như vậy (`"Phương pháp phẫu thuật"`) — ca thật, không phải giả định.
- Ngày về ở **cả** `dd/MM/yyyy` và `yyyy-MM-dd`. Parse cả hai.
- `templates_data` là **list rỗng** ⇒ **LỖI** `TEMPLATE_NOT_ASSIGNED`, không phải "chưa phân loại".
- Parser **không bao giờ throw**. Shape không đọc được ⇒ `ErrorCode = "PARSE_FAILED"`, giữ nguyên
  body trong `RawJson`.
- Mọi lỗi IDP ⇒ `EXTRACTION_ERROR`, **không bao giờ** `Declined`.

## Việc

- [x] 1. Part 1: thêm section harness vào screen diagnostics
- [x] 2. Publish — **rev 179**, harness chạy được tại `https://bnk-dev.outsystems.app/ClaimMVP` → Administration
- [ ] 3. Part 2: chẩn đoán 3 bước, báo cáo, chờ "go", rồi sửa thứ nhỏ nhất
- [ ] 4. Iterate theo output harness PO dán lại

## Giới hạn verify đang tồn tại

`context_graph` **đang timeout với toàn bộ ClaimMVP** từ ~14:37 ngày 28/09 (mọi root, kể cả root
nhỏ; app khác chạy bình thường — xem chẩn đoán trong
`changes/active/idp-rewire-to-idpupload-library/tasks.md`). Nên Part 1 **không verify được bằng
graph** ở thời điểm này. Báo cáo của Mentor không phải bằng chứng; verify khi dịch vụ hồi.


---

## Part 1 — kết quả (session `2471ff98…`, run `7e727f4f…`)

`changeApplied: true` · `errorCount: 7` · `warningCount: 136` — **y nguyên baseline rev 178**,
không thêm error hay warning nào. **Chưa publish.**

**Vị trí:** screen `Administration` (MainFlow). Section mới `ParseCard` đặt **ngay dưới**
`IdpDiagCard` — container "IDP diagnostics" đang giữ control "Run IdpSubmitSync". Tiêu đề
`ParseH3Text` = `Parse a captured response`.

**Local variable trên `Administration`:** `RawJsonInput : Text` (không default) ·
`IsSyncShapeInput : Boolean`, **default `True`** · `ParseResult : IdpExtractionResult` ·
`ParseRan : Boolean`.

**Screen action:** `RunIdpParseExtraction`.

**Flow của nó — đúng 4 node, đây là phần an toàn quan trọng nhất:**

1. `Start`
2. `CallIdpParseExtraction` (ExecuteServerActionNode) — gọi
   `IdpParseExtraction(RawJson: RawJsonInput, IsSyncShape: IsSyncShapeInput)`
3. `Assign` — `ParseResult ← CallIdpParseExtraction.Result`, `ParseRan ← True`
4. `End`

**Không** gọi `IdpSubmitSync`, `IdpSubmitDocument`, `IdpGetToken`, `IdpUpload`, hay REST method nào.
Server action duy nhất được gọi là `IdpParseExtraction`. Không ghi entity. ⇒ **0 quota mỗi lần bấm.**

**Widget:** `RawJsonTextArea` (TextArea, **10 dòng**, bind `RawJsonInput`) · `CheckboxRow` chứa
`SyncShapeCheckbox` (bind `IsSyncShapeInput`) + `SyncShapeLabel`
(`Two-level result.result shape (POST /request/sync)`) · `ParseBtn`
(`Run IdpParseExtraction`, OnClick → `RunIdpParseExtraction`) · `ParseResultIf` (If, condition
`ParseRan`) → `ParseResultCont` → `ScalarDl` với các cặp Lbl/Expr cho `RequestId`, `PageCount`,
`ErrorCode`, `ErrorMessage`, `ProcessingTime`, `MinioPath`, `ClaimConfidence`, `Completeness`, và
số lượng `Templates`; danh sách per-template; `FieldsTable2`.

**Bảng field scroll, không phân trang:** `FieldsTable2` là `TableRecords` **không gắn pagination
widget**. Cột: header `HK2/HV2/HC2/HCLlm2/HPage2`, cell `RcK2/ExprK2`, `RcV2/ExprV2`,
`RcC2/ExprC2`, `RcCLlm2/ExprCLlm2`, `RcPage2/ExprPage2` — tức `K`, `V`, `C`, `CLlm`, `PageNumber`.

**Ghi chú từ run:** Mentor vấp một lần — *"local functions aren't allowed in this context"* — rồi
viết lại inline. Và nó báo một scalability warning cho `FieldsTmplList` đã hết.

**Chưa verify bằng máy.** `context_graph` vẫn timeout với toàn bộ ClaimMVP, nên toàn bộ mục trên là
**lời Mentor**, không phải bằng chứng đọc từ model. Verify khi dịch vụ hồi.

**Đã publish.** `mentor_publish` → `publicationKey 689b08e6-e767-4c2a-b931-00689d55ae01`,
`revision 179`; `publish_status` → `outcome: success`, `status: Finished`.
`app_info` → rev **179**, `2026-09-28T09:00:55.231947Z`,
`modelDigest 3832c0ea… → 541efcad-6d32-c3a7-adce-946971f11a90`.
`env_app` → deploy rev **179**, `buildKey 505ee8ca-caa7-40bb-8bb5-ba703aef80ec`,
`deploymentDateTime 2026-09-28T09:00:55.452Z`, url `https://bnk-dev.outsystems.app/ClaimMVP`.

**Kiểm tra an toàn PO phải làm trước khi dùng thật:** bấm "Run IdpParseExtraction" với ô Raw JSON
**để trống**. Kỳ vọng: ra kết quả (rỗng hoặc `PARSE_FAILED`) mà **không** có lần gọi mạng nào và
**không** trừ trang quota. Đây là cách xác nhận harness thật sự offline, độc lập với lời Mentor.


---

## Part 2 — wire model tách khỏi domain model (28/09, rev khởi điểm 179)

### Bằng chứng từ harness (PO chạy, capture `/request/sync`, cả `IsSyncShape` True và False)

`RequestId 21408` **đúng** (top-level) · `ProcessingTime 0` **sai** · `MinioPath` rỗng **sai** ·
`Templates count 0` **sai** · `ErrorCode TEMPLATE_NOT_ASSIGNED`.
`processingTime`, `minioPath`, `templates_data` đều nằm trong `result.result`. Cả ba rỗng trong khi
`requestId` đúng ⇒ **không có gì bên trong `result.result` bind được.**

### Task 1 — đọc được từ `context_structures` (rev 179, 22 structure)

Không structure nào trong app mang tên snake_case. Khớp case-insensitive:
**CÓ** — `templateId`, `subDocumentType`, `fields`, `tables`, `k`, `v`, `c`, `confidence`,
`processingTime`, `minioPath`, `errorCode`, `errorMessage`.
**KHÔNG** — `result`, `templates_data`, `template`, `data`, `c_llm`, `match_w`, `page_number`,
`confidence_standalone`.

**Điều chỉnh root cause:** `processingTime`/`minioPath` **có** attribute khớp tên, nên snake_case
một mình không giải thích được. Nguyên nhân chính là **không có attribute `result` ở bất kỳ tầng
nào** ⇒ không có đường descend vào `result.result`. Snake_case là defect thật và sẽ cắn
`templates_data`/`c_llm`/`match_w`/`page_number` ngay sau khi sửa nesting. Wire model sửa cả hai.

**Chưa đọc được:** parser hiện deserialize vào structure nào — cần `context_graph`, vẫn timeout.

### Wire structure (tên attribute gương JSON key CHÍNH XÁC)

```
IdpWireField    { k: Text, v: Text, c: Decimal, c_llm: Decimal,
                  match_w: Text, page_number: Integer }
IdpWireData     { confidence: Decimal, confidence_standalone: Decimal,
                  fields: IdpWireField List, tables: Text List }
IdpWireTemplate { template: Text, templateId: Text, subDocumentType: Text,
                  data: IdpWireData }
IdpWireInner    { errorCode, errorMessage, minioPath, minioBucket, refTenantId,
                  refPartnerId, refRequestId: Text, processingTime: Decimal,
                  templates_data: IdpWireTemplate List }
IdpWireOuter    { result: IdpWireInner, + toàn bộ field phẳng như IdpWireInner }
IdpWireEnvelope { id, requestId, confidence: Text, result: IdpWireOuter,
                  data: IdpWireOuter, createdAt: Text }
```

`IdpWireOuter` cố ý mang **cả** `result` lồng **và** các field phẳng ⇒ một lần deserialize phủ mọi
shape.

### Coalesce — chọn cái đầu tiên thực sự có dữ liệu (`templates_data` không rỗng HOẶC `processingTime > 0`)

1. `Envelope.result.result` — POST `/request/sync`, hai tầng
2. `Envelope.result` — GET `/request/{id}/result`, một tầng
3. `Envelope.data.result` — GET `/request/{id}/check-status` khi Completed

`IsSyncShape` **giữ trong signature** (không được đổi signature) nhưng **hạ xuống thành gợi ý** chỉ
đổi thứ tự danh sách trên. Parser tự tìm được dữ liệu thì không bị cờ sai của caller giết —
cờ sai đã tốn hai vòng hôm nay.

### Map wire → domain

`templateId` string `"6093"` → Long Integer · top-level `confidence` là **string** `"70.87"`,
`data.confidence` là **number** `71.01` (khác type ở khác tầng — không dùng chung wire type) ·
`c` về cả `1` và `0.664` → Decimal, **không bao giờ** Integer · `data.confidence` 0–100 → lưu 0–1,
`fields[].c` đã 0–1 · `v = ""` ⇒ **null**, **không** tính là có mặt, dù kèm `c = 1` ·
ngày cả `dd/MM/yyyy` và `yyyy-MM-dd` · `templates_data` rỗng ⇒ `TEMPLATE_NOT_ASSIGNED`
**chỉ khi** envelope parse được và list thực sự rỗng — **không bao giờ** khi deserialize không tìm
thấy gì (đây đúng là chỗ đang hỏng) · parser **không throw**, shape không đọc được ⇒
`PARSE_FAILED` + giữ body trong `RawJson` · mọi lỗi IDP ⇒ `EXTRACTION_ERROR`, **không** `Declined`.

**Không đổi tên** attribute nào của `IdpExtractionResult`, `IdpTemplateData`, `IdpField`.

---

## Part 2 — kết quả run Mentor `aedca980…` (session `2471ff98…`)

**Terminal:** `succeeded` · `attemptedChange: true` · `changeApplied: true` ·
`errorCount: 7` · `warningCount: 134` (bản văn của Mentor nói 112 — **số máy đọc được là 134**;
lệch này chưa giải thích được, ghi lại nguyên trạng). `IdpParseExtraction` = **44 node**.
Polling: 1 poll cuối, tổng 14 m 51 s.

**Cảnh báo sai của Mentor đã bác:** Mentor báo 7 lỗi multipart "sẽ chặn publish". rev 178, 179
và 181 đều publish được với `errorCount 7` → sai. Không chuyển tiếp claim này.

### 1. Sáu structure wire đã tạo (như Mentor báo)

```
IdpWireField    k Text(512) · v Text(2000) · c Decimal · c_llm Decimal ·
                match_w Text(100) · page_number Integer
IdpWireData     confidence Decimal (0–100) · confidence_standalone Decimal ·
                fields IdpWireField List · tables Text List
IdpWireTemplate template Text(200) · templateId Text(50) · subDocumentType Text(100) ·
                data IdpWireData
IdpWireInner    errorCode Text(50) · errorMessage Text(500) · minioPath Text(500) ·
                minioBucket Text(200) · refTenantId Text(100) · refPartnerId Text(100) ·
                refRequestId Text(100) · processingTime Decimal ·
                templates_data IdpWireTemplate List
IdpWireOuter    result IdpWireInner  +  toàn bộ field phẳng y như IdpWireInner
IdpWireEnvelope id Text(100) · requestId Text(100) · confidence **Text(50)** ·
                result IdpWireOuter · data IdpWireOuter · createdAt Text(50)
```

`IdpWireEnvelope.confidence` là **Text** có chủ đích — tầng ngoài trả string `"70.87"`.

### 2. Coalesce + vai trò của `IsSyncShape`

Deserialize `RawJson` **một lần** vào `IdpWireEnvelope`. Sau đó một node If trên `IsSyncShape`
chia hai nhánh, cùng đổ vào biến `Candidate` (kiểu `IdpWireInner`) + cờ `CandidateFound`:

| `IsSyncShape` | Thứ tự thử |
|---|---|
| True | `result.result` → `result` (phẳng) → `data.result` |
| False | `result` (phẳng) → `data.result` → `result.result` |

"Có dữ liệu" = `templates_data.Length > 0` **hoặc** `processingTime > 0`. Hai nhánh gặp nhau ở
`IfCandidateFound`. Cờ chỉ **đổi thứ tự**; cờ sai vẫn tìm ra node thật sự có dữ liệu.

### 3. Map wire → domain

`IdpExtractionResult`:

| Domain | ← |
|---|---|
| `RequestId` | `Envelope.requestId` (key tầng ngoài, luôn đúng) |
| `ProcessingTime` | `Candidate.processingTime` |
| `MinioPath` | `Candidate.minioPath` |
| `ErrorCode` | `Candidate.errorCode` |
| `ErrorMessage` | `Candidate.errorMessage` |
| `Templates` | ForEach trên `Candidate.templates_data` |
| `ClaimConfidence` | `If(MinCSet, MinC, 0)` — min `c` trên các field bắt buộc **có giá trị** |
| `Completeness` | `If(MandatoryTotal > 0, MandatoryWithValue / MandatoryTotal, 0)` |
| `RawJson` | = input `RawJson` (bất biến) |
| `PageCount` | **không set** — không có trong wire shape |

`IdpTemplateData` (mỗi phần tử `Candidate.templates_data`):
`TemplateId ← TextToLongInteger(…Current.templateId)` (string `"6093"` → Long Integer) ·
`TemplateName ← …Current.template` · `SubDocumentType ← …Current.subDocumentType` ·
`Confidence ← …Current.data.confidence / 100` (0–100 → 0–1) ·
`Fields ← ForEach lồng` · `Tables ← rỗng` (wire `tables` là Text List, không map sang `IdpTable`).

`IdpField` (mỗi `…Current.data.fields`): `K ← .k` · `V ← .v` · `C ← .c` (**đã 0–1, không chia**) ·
`CLlm ← .c_llm` · `MatchW ← .match_w` · `PageNumber ← .page_number`.

### 4. `v = ""`

`V` map thẳng từ wire `v`; `v = ""` lưu thành `""`. Phép tính confidence kiểm
`…Fields.Current.V <> ""` trước khi đếm ⇒ field `v = ""` (dù `c = 1`) **không** vào
`MandatoryWithValue` và **không** vào `MinC`. Đúng ca "Phương pháp phẫu thuật" của spec.

> Lệch spec cần biết: kế hoạch ghi `v = ""` ⇒ **null**. Mentor lưu `""`, không phải null.
> Hệ quả đếm giống nhau (đều bị loại), nhưng giá trị lưu khác. Chưa sửa.

### 5. `PARSE_FAILED` vs `TEMPLATE_NOT_ASSIGNED`

`PARSE_FAILED` ở hai chỗ: (a) exception handler — `JSONDeserialize` throw hoặc bất kỳ runtime
exception nào; (b) node `AssignParseFailed` khi `CandidateFound = False` — envelope parse được
nhưng **cả ba** node candidate đều rỗng.

`TEMPLATE_NOT_ASSIGNED` **chỉ khi** `CandidateFound = True` **và**
`Candidate.templates_data.Length = 0`. Đây chính là chỗ bug cũ gộp hai ca:
nó kiểm `Result.Templates.Length = 0` **sau** một lần deserialize vào domain structure —
bằng 0 cả khi parse chết và khi templates thật sự rỗng.

### 6. Publish

`mentor_publish` → `publicationKey de502036…`, **revision 181**.

### Còn mở

- `context_graph` trên ClaimMVP vẫn timeout ⇒ **chưa đọc được bằng máy** 44 node này.
  Toàn bộ §1–§5 ở trên là **report của Mentor**, chưa verify độc lập. Đóng hồ sơ khi outage hết.
- `warningCount` 134 (máy) vs 112 (văn bản Mentor).
- `PageCount` không có nguồn trong wire shape — nếu harness cần, lấy từ
  `IdpSubmitResponse2.PageCount` ở chỗ gọi, không phải từ parser.

### Verify độc lập phần đọc được (rev 181, `context_structures owned_only=true`)

`context_graph` chết nhưng `context_structures` sống. Đọc được 28 structure; **sáu structure wire
có thật**, timestamp `2026-09-28T09:51:51.716Z`, attribute và type **khớp đúng** report của Mentor:

| Structure | key | Đã kiểm |
|---|---|---|
| `IdpWireField` | `2a8a2fa7…` | `k` Text 512 · `v` Text 2000 · `c` Decimal · `c_llm` Decimal · `match_w` Text 100 · `page_number` Integer |
| `IdpWireData` | `71786aaf…` | `confidence` Decimal · `confidence_standalone` Decimal · `fields` IdpWireField List · `tables` Text List |
| `IdpWireTemplate` | `6c90cf6c…` | `template` Text 200 · `templateId` **Text 50** · `subDocumentType` Text 100 · `data` IdpWireData |
| `IdpWireInner` | `a05d95f4…` | 9 attr, có `templates_data` IdpWireTemplate List · `processingTime` Decimal · `minioPath/minioBucket/refTenantId/refPartnerId/refRequestId/errorCode/errorMessage` |
| `IdpWireOuter` | `80230fab…` | `result` **IdpWireInner** + 9 field phẳng y hệt IdpWireInner ⇒ đúng thiết kế "một lần deserialize phủ mọi shape" |
| `IdpWireEnvelope` | `c34eb65b…` | `id` Text 100 · `requestId` Text 100 · `confidence` **Text 50** · `result` IdpWireOuter · `data` IdpWireOuter · `createdAt` Text 50 |

Ba structure domain **không bị đổi tên attribute** — `IdpExtractionResult` (10 attr),
`IdpTemplateData` (6), `IdpField` (6) giữ nguyên timestamp `2026-09-22`.

**Vẫn chưa verify:** 44 node flow của `IdpParseExtraction` — coalesce, ForEach, phép tính
confidence, hai nhánh `PARSE_FAILED`/`TEMPLATE_NOT_ASSIGNED`. Cần `context_graph` hoặc Studio.

---

## Part 3 — năm fix nhỏ (run `a9f65b18…`, session `2471ff98…`) — 28/09

Fence: `IdpParseExtraction` · `IdpWire*` · `IdpField` · section harness trên Administration ·
`IdpSubmitSync`. **Không** `IdpUpload`, **không** REST method, **không** `A5Step*`,
**không** trường tiền.

### FIX 1 — `match_w` Text(100) → Text(2000)

`match_w` là **trường provenance**: đoạn text gốc mà giá trị được khớp vào. Giá trị dài nhất
trong capture là 70 ký tự — dư 30. Chứng từ dày hơn sẽ vượt 100 và ODC **cắt im lặng**.
Provenance bị cắt tệ hơn không có provenance, vì nó vẫn trông như đủ.
Đổi cả `IdpWireField.match_w` **và** `IdpField.MatchW`. Chỉ đổi length.

### FIX 2 — thêm cột MatchW vào bảng field của harness

Bảng đang có K / V / C / CLlm / PageNumber. MatchW là cột **duy nhất chưa verify binding**
và là cột đang có rủi ro length. Thêm làm cột cuối, giữ scrollable.

### FIX 3 — `FIELDMAP_MISSING`

Template không có active `IdpFieldMap` row ⇒ mandatory count = 0 ⇒ ClaimConfidence và
Completeness **không tính được**. 0 là giá trị đúng để lưu, nhưng 0 im lặng đọc y như
"zero confidence" — và đó là con số cổng auto-approval sẽ đọc. Nên khi tổng mandatory = 0:

```
ErrorCode    = "FIELDMAP_MISSING"
ErrorMessage = "No IdpFieldMap rows for template <ids> — ClaimConfidence and
                Completeness are not computed, not zero."
```

**Chỉ set khi `ErrorCode` đang rỗng** — errorCode từ body IDP là lỗi thật, không được ghi đè.
**Không** xoá `Templates`/`Fields`. **Không** map sang `Declined`. Đây là khoảng trống cấu hình,
không phải lỗi trích xuất.

> Điểm mơ hồ đã tự quyết: brief nói "khi mandatory count của **một template** = 0", nhưng
> `ErrorCode`/`ErrorMessage` là **cấp result**, một giá trị. Chọn: fire khi **tổng cấp result** = 0
> (đúng ca "không tính được"), và liệt kê trong message mọi template id góp 0 row. Nếu template A
> có row và B không, tổng > 0 ⇒ không fire — vì lúc đó hai con số **vẫn tính được**, dựng cờ lỗi
> sẽ làm hỏng một result đang đúng.

### FIX 4 — lookup `IdpFieldMap.K` chịu được khác biệt NFC/NFD

Tiếng Việt có hai cách encode cho cùng ký tự hiển thị: composed (NFC) và decomposed. IDP luôn
trả NFC. Một row decomposed trong `IdpFieldMap` sẽ **miss im lặng** — không lỗi, không cảnh báo,
một field đơn giản không bao giờ map. File seed đã normalise, nhưng dữ liệu chỉ là nửa cách sửa.

**Đã kiểm bằng docs ODC:** danh sách Text built-in đúng 15 hàm — `Chr` `EncodeHtml`
`EncodeJavaScript` `EncodeSql` `EncodeUrl` `Index` `Length` `NewLine` `Replace` `Substr`
`ToLower` `ToUpper` `Trim` `TrimEnd` `TrimStart`. **Không có `Normalize`.** Nên normalise bằng
expression là **không làm được**.

**Đường đi được:** đưa phép so sánh K **xuống database**. ODC dùng PostgreSQL
**non-deterministic collation**, `=` là case- **và accent-insensitive** mặc định trên mọi column
ODC tạo (docs `differences-sql.md`), và đó chính là tầng giải quyết canonical equivalence.
So sánh `=` giữa hai Text **trong bộ nhớ** là ordinal .NET ⇒ miss.

Cụ thể: mỗi wire field resolve map row bằng Aggregate có **filter**
`IdpFieldMap.K = <wire k>` + `IdpTemplateId` + `IsActive`. **Không** fetch list rồi so trong
memory bằng expression/ListFilter/If. Tổng mandatory lấy từ một Aggregate riêng, không cần filter K.

> Chưa chứng minh được bằng thực nghiệm: `db_query` v1 trả `rowcount 0` cho **mọi** template
> kể cả SELECT, nên không quan sát được hành vi collation. `context_graph` vẫn chết nên cũng
> không đọc được shape hiện tại. **Harness chạy lần tới là phép thử** — nếu row decomposed
> khớp thì collation đúng như suy luận.

### FIX 5 — `PageCount` đặt ở chỗ thật sự biết số trang

Parser **giữ nguyên** không set `PageCount` — `POST /request/sync` không trả `pageCount`, parser
không được bịa. Thay vào đó `IdpSubmitSync` gán input `PageCount` của chính nó (input đã có,
đang dùng cho guard `IdpSyncMaxPages`) lên result sau khi parser trả về.

### Sự cố 28/09 ~09:59 — mất session Mentor giữa run, và hai revision không phải của tôi

Run `a9f65b18…` (năm fix, session `2471ff98…`) chạy được ~5 phút, phát reasoning event bình thường
("I'm widening the provenance field in both wire and domain structures…"), rồi `mentor_get_run`
trả `not_found`. Ba lần liên tiếp, cả khi bỏ cursor.

**Chẩn đoán:** không phải run chết — **session chết**. Chứng cứ: `mentor_get_run` trên run **cũ**
`aedca980…` (run Part 2 đã drain tới `succeeded` 20 phút trước, đã publish rev 181) **cũng**
trả `not_found`. Mất cả run history của session, không phải mất một run.

**Hai revision không do tôi publish:**

| Rev | Thời điểm | Ai |
|---|---|---|
| 178 | 07:35:38Z | tôi (rewire IdpUpload) |
| 179 | 09:00:55Z | tôi (harness Part 1) |
| **180** | **09:19:20Z** | **không phải tôi** |
| 181 | 09:50:47Z | tôi (wire model Part 2) |
| **182** | **09:59:07Z** | **không phải tôi** |

Cả 180 và 182 có `modelDigest` riêng ⇒ thay đổi model thật, không phải republish rỗng.
`revisionUserKey` giống rev 181 (`9a04cb12…`). Giả thuyết khả dĩ nhất: PO làm tay trong Studio
(khớp với "The seed file is now normalised" và với tiền lệ tự dựng `OnBeforeRequest`), và một
publish từ Studio làm mất hiệu lực session Mentor. **Chưa xác nhận** — không có tool nào truy ra
tác giả một revision.

**Rev 182 KHÔNG chứa năm fix.** Kiểm bằng máy: `IdpField.MatchW` và `IdpWireField.match_w` vẫn
**Text(100)**. Nên draft năm fix mất sạch, không có trạng thái nửa vời phải dọn.

**Khôi phục:** session mới `044c023b…` → `mentor_load_asset` (lấy baseline **rev 182**, không phải
181 — quan trọng, để không ghi đè cái rev 182 mang) → gửi lại nguyên văn prompt năm fix,
run `87bd461b…`.

**Rút ra:** Mentor session không bền qua một publish từ nguồn khác. Draft chưa publish là
**mất được**. Với việc dài, publish theo từng chặng nhỏ thay vì giữ draft lớn.

### Kết quả năm fix — run `059ce963…`, session `044c023b…`, publish rev 183

`succeeded` · `changeApplied: true` · `errorCount: 7` · `warningCount: 134` (máy đọc; bản văn
Mentor nói 118 — lại lệch như lần trước, **tin số máy**). Không lỗi/cảnh báo mới.

**FIX 1** — `IdpWireField.match_w` và `IdpField.MatchW` đều 100 → 2000. Không attribute nào khác bị chạm.

**FIX 2** — thêm header cell `HMatchW2` (label "MatchW") + row cell `RcMatchW2` bind
`Fields.Current.MatchW` vào table `FieldsTable2`; heading section đổi thành
"K / V / C / CLlm / PageNumber / MatchW". Vẫn scrollable. *(Hậu tố `2` trên tên widget gợi ý đã có
`HMatchW`/`RcMatchW` từ trước — cần soi trong Studio xem có widget mồ côi không.)*

**FIX 3** — local var `ZeroMandatoryIds`, assign `AccumZeroMandatory` trong loop template gom
`IdpTemplateId` có `MandatoryTotal = 0`; sau `FinalConf` chèn If `FieldMapMissing?` điều kiện
`MandatoryTotal = 0 and Result.ErrorCode = ""` → nhánh true set `FIELDMAP_MISSING` + message.
Không chạm `Templates`, `Fields`, claim state, decision. Đúng như quyết định đã ghi ở trên.

**FIX 4 — phát hiện lớn hơn cả lỗi Unicode.**

*Shape TRƯỚC (Mentor báo):* `GetMandatoryFields` fetch **toàn bộ** active mandatory row của
**cả bảng `IdpFieldMap`** — **không filter template, không filter K** — rồi `FilterMandatory`
(system action `ListFilter`) so `K` **trong memory .NET bằng ordinal string equality**.

Tức là hai lỗi, không phải một:
1. So K trong memory ⇒ miss NFC/NFD (lỗi đang đi sửa).
2. **Fetch không filter `IdpTemplateId`** ⇒ `MandatoryTotal` đếm mandatory row của **mọi template**,
   và một `K` của template A có thể khớp row thuộc template B. Trái trực tiếp mô tả entity
   *"Maps an IDP field label (K) to a stable internal field code, **per template**.
   Never map by template name."* Nghĩa là `ClaimConfidence`/`Completeness` ở rev 179/181/182 sai
   mẫu số, không chỉ sai vì Unicode.

*Shape SAU:* hai Aggregate, K so ở DB:
- `GetMandatoryCount` (đổi tên từ `GetMandatoryFields`) — filter `IsMandatory = True` +
  `IsActive = True` + `IdpTemplateId = <template hiện tại>`, **không** filter K ⇒ `MandatoryTotal`.
- `GetFieldMap` (mới) — per-field, filter `K = <wire k>` + `IdpTemplateId = <template hiện tại>` +
  `IsActive = True`. K do **database** đánh giá.
- Node `FilterMandatory` cũ đã xoá.

Đánh đổi: `GetFieldMap` nằm trong loop field ⇒ N round-trip DB mỗi template. Chứng từ vài chục
field thì chấp nhận được; nếu sau này có chứng từ hàng trăm field thì xem lại.

**FIX 5** — tên input đúng là **`PageCount`**. Thêm `Result.PageCount = PageCount` vào **cả hai**
assign node set `Result` từ `ParseResult.Result` (nhánh HTTP 200/201 và nhánh non-200), nên
`PageCount` có mặt trong mọi trường hợp sau khi parser trả về. Không chạm call
`IdpUpload.SubmitSync`, không chạm signature, không chạm guard `IdpSyncMaxPages`. Parser vẫn
không tự set `PageCount`.

7 lỗi pre-existing được Mentor định danh: 4× "Unavailable feature: Multipart request format in
REST APIs" trên `IdpDocuments`/`IdpWorkflow`/`IdpMasterData`/`IdpAuth`, cộng 3 lỗi liên quan
method của `IdpDocuments`.

**Cần verify khi `context_graph` hồi:** shape trước của FIX 4 (đây là claim của Mentor, chưa đọc
bằng máy) · widget mồ côi `HMatchW`/`RcMatchW` · điều kiện If `FieldMapMissing?`.

### Bẫy mới: Context Service index trễ sau publish — đừng kết luận "fix không ăn"

Ngay sau khi publish rev 183, `context_search` **và** `context_structures` đều báo
`IdpWireField.match_w` và `IdpField.MatchW` vẫn **Text(100)**, timestamp không nhích. Trông y như
FIX 1 không ăn. Đọc lại sau ~45 phút: **cả hai Text(2000)**, timestamp
`2026-09-28T10:48:05.307Z`. **Index trễ, fix ăn thật.** Mentor nói đúng, tôi kết luận sớm.

Hai điều rút ra:

1. **`timestamp` của Context Service không đổi khi ruột một action thay đổi.**
   `IdpParseExtraction` vẫn mang timestamp `2026-09-22T15:59:36` dù đã bị viết lại ở rev 179 và
   rev 181. Timestamp chỉ nhích khi signature/metadata đổi, hoặc khi element được tạo. **Không
   dùng timestamp làm dấu hiệu tươi/cũ cho thay đổi nội bộ.**
2. **Structure thì timestamp CÓ nhích** khi attribute đổi — nhưng **trễ hàng chục phút** sau
   publish. Nên "đọc ngay sau publish rồi thấy y như cũ" **không chứng minh được gì**.
   Muốn xác nhận: đọc lại sau một lúc, hoặc hỏi Mentor đọc model.

Đây là bẫy cùng họ với bài học đã ghi ở `idp-useragent-and-poller-wake`:
*"không dùng context_* để chứng minh một element KHÔNG tồn tại"* — nay mở rộng thành
**không dùng context_* ngay sau publish để chứng minh một thay đổi KHÔNG xảy ra.**

**FIX 1 verify xong bằng máy:** `IdpWireField.match_w` = Text(2000) ·
`IdpField.MatchW` = Text(2000), cả hai timestamp `10:48:05`. Bốn fix còn lại vẫn chờ
`context_graph` (FIX 5 đã xác nhận được tiền đề: `IdpSubmitSync` có input `PageCount` Integer,
mandatory).

---

## Chỉnh hướng 29/09 — PO bác phương án DB collation, và đặt lại tên năm fix

### Tên chuẩn (bỏ số — số đã lệch giữa ba nguồn)

| Tên | Nội dung | Trạng thái |
|---|---|---|
| `match-w-widen` | `IdpWireField.match_w` + `IdpField.MatchW` → Text(2000) | ✅ verify bằng máy, rev 183 |
| `harness-matchw-column` | cột MatchW (`HMatchW2`/`RcMatchW2`) vào `FieldsTable2` | ⏳ chờ harness |
| `fieldmap-missing-code` | `FIELDMAP_MISSING` khi tổng mandatory cấp result = 0 **và** `ErrorCode` rỗng | ⏳ test được ngay |
| `fieldmap-k-lookup` | so K + phạm vi template khi tra `IdpFieldMap` | ⚠️ revert một nửa 29/09 |
| `submitsync-pagecount` | `Result.PageCount = PageCount` ở cả hai assign node | ⏳ tiền đề đã verify |

### Bác phương án DB collation (PO, 29/09) — nhận

Lý do PO, đã kiểm lại bằng máy trên `insurance BNK/IDP_API_Spec/IdpFieldMap-seed.csv`:
**164 dòng dữ liệu · 0 dòng không-NFC · 0 dòng chứa dấu kết hợp rời**
(U+0300/0301/0303/0309/0323). IDP luôn trả NFC (21/21 nhãn trong fixture).

⇒ Đẩy so sánh xuống DB làm tra cứu **không phân biệt dấu** — trong chính bảng ánh xạ ra
**cột tiền** — để chữa một lỗi đã hết, dựa trên hành vi collation **chưa quan sát được**.
Rủi ro: hai nhãn chỉ khác dấu khớp nhầm. Bỏ.

**`fieldmap-k-lookup` gộp hai thay đổi; chỉ revert cái thứ nhất:**

| | Trước rev 183 | rev 183 | Sau revert (PO chốt) |
|---|---|---|---|
| So K | memory, ordinal | **DB, accent-insensitive** | **memory, ordinal** |
| Phạm vi template | **không có filter** | DB filter `IdpTemplateId` | **DB filter `IdpTemplateId`** |
| Round-trip DB | 1 / toàn bảng | 1 / **field** | 1 / **template** |

Shape mới: `GetFieldMapRows` (1 lần/template, filter `IdpTemplateId` + `IsActive`, **không** filter K)
→ so K trong memory bằng ordinal. `GetMandatoryCount` **giữ nguyên** kèm filter `IdpTemplateId`.
Tốt hơn cả rev 183 lẫn bản gốc: phân biệt dấu **và** đúng phạm vi template **và** ít round-trip hơn.

### Giới hạn: không normalise được trong ODC, chỉ từ chối được

15 hàm Text của ODC không có `Normalize`. Trong Server Action **chỉ làm được nửa TỪ CHỐI**
(dò 5 dấu kết hợp tiếng Việt bằng `Index()`), **không làm được nửa chuẩn hoá**.
Chuẩn hoá phải nằm phía đĩa (`unicodedata.normalize`).

**`Seed_IdpFieldMap` chưa tồn tại.** Quét 40 action: có 21 `Seed_*` khác, không có cái nào cho
`IdpFieldMap`; khớp với việc entity đang **0 dòng**. Nên guard NFC là **ràng buộc cho bước seed
sẽ dựng**, chưa có chỗ gắn. Khi dựng: chuẩn hoá NFC phía đĩa, và seed action **từ chối** dòng
chứa dấu kết hợp rời.

### Denominator — phép thử cũ vô nghĩa, phép thử mới sắc

PO đúng: `IdpFieldMap` 0 dòng ⇒ mẫu số 0/0 ⇒ `Completeness = 0` ở **mọi** giả thuyết
(đã kiểm 21406, 21408, 21410 đều 0/0). Phép thử "so Completeness trước/sau" **không phân biệt
được gì** trước khi seed. Tôi dựng nó sai. Giữ giả thuyết, không kết luận từ 0.

Đọc file seed ra dự đoán sắc cho **sau** khi seed: 22 dòng `IsMandatory=True`, chỉ ở **hai**
template — **6091 (14)** và **6093 (8)**. Capture 21406 là template **6093**:

- code **đúng** (có filter template) → `MandatoryTotal` = **8**
- code **lỗi** (không filter) → `MandatoryTotal` = **22**

**Test miễn phí, không cần seed:** `IdpFieldMap` đang 0 dòng ⇒ tổng mandatory = 0 ⇒ harness ở
rev 183 **phải** cho `ErrorCode = FIELDMAP_MISSING`. Không thấy ⇒ `fieldmap-missing-code` sai.

### Hai lỗi dữ liệu trong seed CSV (cần sửa trước khi seed)

- **1 dòng `IsMandatory` để trống** — entity khai `IsMandatory` là mandatory Boolean ⇒ seed sẽ
  chết hoặc nhận default `False` một cách im lặng.
- 1 dòng `IsActive=False`.
- Không trùng `(IdpTemplateId, K)` nào — 0 cặp trùng.

### Kết quả revert `fieldmap-k-lookup` — run `82249488…`, publish rev 184

`succeeded` · `changeApplied: true` · `errorCount 7` · `warningCount 134` — **y hệt baseline
rev 183**, Mentor xác nhận 0 lỗi mới, 0 cảnh báo mới.

**Đã xoá:** aggregate `GetFieldMap` — bản per-field, chạy trong cycle `ForEachFieldConf`, ba filter:
`K = <wire k>` · `IdpTemplateId = TextToLongInteger(Candidate.templates_data.Current.templateId)` ·
`IsActive = True`.

**Đã thêm:** aggregate `GetFieldMapRows` — đặt trong cycle **`ForEachTmplConf`** (lặp trên
`Result.Templates`), **một lần mỗi template**. Đúng **hai** filter, không filter K, không `MaxRecords`:

```
IdpFieldMap.IdpTemplateId = Result.Templates.Current.TemplateId
IdpFieldMap.IsActive      = True
```

**So K trong memory:** trong `ForEachFieldConf`, node `ListFilter` tên `FilterFieldMap` lọc
`GetFieldMapRows.List` với điều kiện `IdpFieldMap.K = CurField.K`; kết quả vào local
`FieldMapMatch`. Đây là so text thường do runtime OutSystems đánh giá — **ordinal,
phân biệt hoa/thường, phân biệt dấu** — không phải database filter.
`AssignIsMandatory` đọc `FieldMapMatch.Length > 0 and FieldMapMatch.Current.IsMandatory`.

**`GetMandatoryCount` xác nhận KHÔNG đổi**, giữ cả ba filter:

```
IdpFieldMap.IsMandatory   = True
IdpFieldMap.IsActive      = True
IdpFieldMap.IdpTemplateId = TextToLongInteger(Candidate.templates_data.Current.templateId)
```

⇒ Phạm vi template của mẫu số **còn nguyên**. Đạt đúng cả hai mục tiêu: **phân biệt dấu** ở phép
so K, **đúng phạm vi template** ở mẫu số, và **1 round-trip DB mỗi template** thay vì mỗi field.

Chưa verify bằng máy (`context_graph` vẫn chết) — đây là report của Mentor. Harness là phép thử.

### ✅ `fieldmap-missing-code` verify bằng hành vi (rev 184, PO bấm harness 29/09)

Kết quả: **`ErrorCode = FIELDMAP_MISSING`**. Đúng dự đoán.

**Suy ra được gì — và đây là lần verify bằng hành vi ĐẦU TIÊN của cả đợt:**

1. Parser **chạy tới hết, không throw** — nhánh exception handler không nổ.
2. **Coalesce tìm được candidate** (`CandidateFound = True`). Lý do: điều kiện If là
   `MandatoryTotal = 0 and Result.ErrorCode = ""`. Nếu `CandidateFound = False` thì `ErrorCode`
   đã là `PARSE_FAILED`, guard `ErrorCode = ""` sẽ chặn ⇒ không thể thấy `FIELDMAP_MISSING`.
   ⇒ **wire model + coalesce của Part 2 chạy được trên JSON thật.** Đây là điều đáng giá hơn cả
   bản thân fix này, và là thứ `context_graph` đang chết không cho đọc.
3. `MandatoryTotal = 0`, khớp `IdpFieldMap` rỗng.
4. Guard `ErrorCode = ""` không chặn oan.
5. Phân biệt `PARSE_FAILED` vs `TEMPLATE_NOT_ASSIGNED` vs `FIELDMAP_MISSING` hoạt động — ba mã
   không lẫn nhau ở ca này.

**KHÔNG suy ra được** (đừng nhận quá):

- Guard có **kiêng** đúng khi body IDP đã mang `errorCode` — cần một capture có `errorCode`.
- Guard có **kiêng** đúng khi `MandatoryTotal > 0` — cần seed.
- Phạm vi template của mẫu số — cần seed. Dự đoán: 21406 là template 6093 ⇒ **8**, không phải 22.
- `submitsync-pagecount` — **harness không bao giờ kiểm được**, vì harness gọi thẳng
  `IdpParseExtraction`, không đi qua `IdpSubmitSync`. Chỉ verify được bằng `context_graph`
  hoặc Studio.

### ✅ Stage 0 (a) — `ErrorMessage` nêu đúng 6093 (PO chạy trên rev 187, 30/09)

Kết quả: `ErrorMessage` **có nêu template 6093**.

**Đóng lại `fieldmap-missing-code` hoàn toàn, bằng hành vi.** Cộng với lần chạy trước
(`ErrorCode = FIELDMAP_MISSING`), giờ đã chứng minh được cả hai nửa:

1. Mã **nổ đúng lúc** — tổng mandatory = 0 và `ErrorCode` đang rỗng.
2. **`ZeroMandatoryIds` gom đúng id** — assign `AccumZeroMandatory` trong loop template chạy đúng,
   và chỗ trống `<ids>` trong message được điền bằng id thật, không rỗng, không sai template.

Đây là thứ **chỉ quan sát được khi `IdpFieldMap` còn 0 dòng**. Sau khi seed, 6093 có 8 dòng
mandatory ⇒ `FIELDMAP_MISSING` không bao giờ nổ cho 6093 nữa ⇒ (a) vĩnh viễn không đo lại được.
Đã chốt kịp.

**Còn thiếu của Stage 0:**

- **(c) khối scalar** — **phụ thuộc cửa sổ**, chỉ đọc được TRƯỚC khi seed. Giá trị trước seed
  (`ClaimConfidence 0` · `Completeness 0` · `ErrorCode FIELDMAP_MISSING`) sau seed sẽ đổi hết.
- **(b) cột MatchW** — **không** phụ thuộc cửa sổ, đo lúc nào cũng được. Dự đoán đã tính từ fixture:
  17/21 field có giá trị, 4 null (`Phương pháp phẫu thuật` · `Tuổi` · `Giới tính` ·
  `Kết quả chẩn đoán`), max length **70**; ba dòng đầu
  `BỆNH VIỆN THỬ NGHIỆM SỐ 2` / `Số vào viện: 2026/00915` / `Tên bệnh nhân: TRẦN THỊ DEMO`.
