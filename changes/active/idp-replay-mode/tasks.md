# Change — ReplayMode (Stage 3) + guard errorCode mọi cấp (Stage 2)

**Trạng thái:** 🟠 Đang dựng · **Mở:** 29/09/2026 · **App:** `ClaimMVP`, rev khởi điểm **185**
**Ưu tiên:** PO đẩy Stage 3 lên **ngay sau Stage 1** ngày 29/09 vì Stage 6 bị chặn từ bên ngoài.

---

## Vì sao đổi thứ tự: partner IDP hết hạn

Lời gọi thật 29/09 **11:35:26.888Z** trả **HTTP 201** với body:

```json
{"id":"21532","requestId":"21413","confidence":"0.00",
 "result":{"result":{},"errorCode":1014,"pageCount":1,"requestId":"21413",
 "extraParams":{}},"createdAt":"2026-09-29T11:35:26.888Z"}
```

`1014` = `PARTNER_EXPIRED_CODE` "Partner is expired" (build-01 §5.2). Mọi lời gọi thật sẽ trả
đúng thế này tới khi đội IDP gia hạn — **và vẫn tính `pageCount = 1`**, tức vẫn trừ quota.
⇒ **Stage 6 chờ bên ngoài.** ReplayMode không còn là dự phòng mà là cách duy nhất làm việc tiếp,
và nếu 07/10 chưa gia hạn thì **nó chính là demo**.

## Ngoại lệ có chủ đích với CLAUDE.md §2

CLAUDE.md §2 ghi: *"Mentor không được sinh Entity"*. Stage 3 cần Entity `IdpReplay`.
**PO cho phép ngoại lệ này ngày 29/09**, kèm điều kiện tôi tự đặt:

- Mentor **chỉ** được tạo `IdpReplay`; không chạm entity nào khác ngoài `AgentRun` (thêm 1 attribute)
- Mentor phải **liệt kê từng attribute** đã tạo
- **Verify bằng `context_entities` trước khi publish**
- `IdpReplay` **không chứa trường tiền** ⇒ rủi ro hẹp, nhưng luật vẫn là luật nên ghi lại ở đây

**Touch entity thứ hai, ngoài phạm vi câu hỏi đã hỏi:** `AgentRun` hiện có 15 attribute và
**không có `Replay`** (đã đọc bằng máy 29/09). Brief yêu cầu *"AgentRun must record Replay = true"*
⇒ cần thêm `Replay : Boolean`, default False, mandatory. Đây là **sửa** entity có sẵn, không phải
sinh mới, và do chính brief yêu cầu — nhưng vẫn là touch entity thứ hai nên báo rõ.

## Đọc fixture 1014 bằng máy — hai rủi ro làm chính cái fix bị vô hiệu

`fixtures/6093-sync-201-errorCode-1014.json`, 241 byte:

| Đường dẫn | JSON type | Giá trị |
|---|---|---|
| `id` | str | `"21532"` |
| `requestId` | str | `"21413"` |
| `confidence` | str | `"0.00"` |
| `result.result` | dict | **rỗng** (len 0) |
| **`result.errorCode`** | **int** | **1014** |
| **`result.pageCount`** | **int** | **1** |
| `result.requestId` | str | `"21413"` |
| `result.extraParams` | dict | rỗng |

**Rủi ro 1 — `errorCode` là số nguyên, không phải chuỗi.** Wire model đang khai
`IdpWireOuter.errorCode` và `IdpWireInner.errorCode` = **Text(50)**. Nếu JSONDeserialize của ODC
không tự chuyển int→Text thì guard đọc ra rỗng ⇒ **`Outcome.Success = True` trên một 1014**, đúng
cái bug đang sửa, bị tái tạo **bởi chính bản fix**. ⇒ đổi cả hai sang **Integer**.
Đây là **lệch so với câu chữ** của brief ("Add ErrorCode : Text") nhưng phục vụ đúng ý định; báo rõ
theo rule 7. Nếu về sau có shape trả `errorCode` dạng chuỗi thì sẽ ra 0 ⇒ rơi về `PARSE_FAILED` —
suy giảm, **không** phải âm thầm báo thành công. Chấp nhận được.

**Rủi ro 2 — `pageCount` vô hình.** Cả `IdpWireOuter` lẫn `IdpWireInner` **không có** attribute
`pageCount`. Trang **đã bị trừ** mà parser không thấy. ⇒ thêm `pageCount : Integer` ở cả hai.
Cũng thiếu `requestId` ở cấp `result` (chỉ có `refRequestId`) — không chặn vì `requestId` tầng ngoài
vẫn có.

**Ba nhánh coalesce đều fail trên body này** (`result.result` rỗng · `result` không có
`templates_data`/`processingTime` · không có key `data`) ⇒ hôm nay ra `PARSE_FAILED`.
Đúng như PO nói: phải ra **1014**.

## Việc — Stage 3

- [ ] **1. Data model** (run riêng, verify trước khi đi tiếp)
  - Entity `IdpReplay`: `Key` Text(100) · `Endpoint` Text(100) · `RawJson` Text(max) ·
    `Note` Text(500) · `CreatedAt` Date Time
  - Setting `ReplayMode` : Boolean, default **False**
  - `AgentRun.Replay` : Boolean, default False, mandatory
- [ ] **2. Logic**
  - `ReplayMode = True` ⇒ `IdpSubmitSync` và `IdpSubmitDocument` trả `RawJson` đã lưu theo key,
    **thay vì** gọi `IdpUpload`. Cùng output, cùng parser, cùng downstream. Không mạng, không quota.
  - `AgentRun.Replay = True` khi chạy dưới ReplayMode. *Một run replay trông giống run thật là một
    lời nói dối trong audit trail.*
  - Harness: nút **"Load from IdpReplay"** đổ `RawJson` vào ô.
- [ ] **3. Nạp dữ liệu replay — lệch cách làm, có lý do**
  Brief nói "Seed IdpReplay with the captured 6093 response". Tôi **không** nhúng 7.100 byte JSON
  vào literal OML: Mentor transcribe 7 KB JSON tiếng Việt là rủi ro sai âm thầm, mà đây lại là
  **dữ liệu dự phòng cho demo** — sai một ký tự là demo chết.
  Thay bằng: thêm nút **"Save to IdpReplay"** cạnh ô RawJson của harness. PO dán fixture (việc đã
  làm thường xuyên) rồi bấm Save ⇒ row `TEST-DIAG` tạo từ nội dung **byte-exact**, không qua tay
  Mentor. Lợi thêm: mọi capture sau này đều nạp được, không cần sửa code.

## Việc — Stage 2 (sau Stage 3)

- [ ] `errorCode` **mọi cấp** (`result.errorCode`, `result.result.errorCode`, `errorCode` ngoài
  cùng): khác null và khác 0 ⇒ **EXTRACTION_ERROR**, **không bao giờ Declined**, `ErrorMessage`
  mang con số. build-01 §5.3 + BRD IdpUpload §7.1 đã nêu đích danh ví dụ 1014.
- [ ] `IdpParseExtraction`: không tìm được candidate **nhưng** có `errorCode` ⇒ trả mã đó,
  **không** phải `PARSE_FAILED`. "Không đọc được" ≠ "IDP từ chối và nói rõ vì sao".
- [ ] `RequestId <= 0` ⇒ không gán RequestId/PageCount, ra EXTRACTION_ERROR.
- [ ] `Settings.IdpUserAgent` rỗng ⇒ dừng với thông điệp 403/1010.
- [ ] `IdpSubmitResponse2`: thêm ErrorCode / ErrorMessage / StatusCode.
- **Test bằng fixture 1014, zero quota.**

## Bỏ khỏi Stage 1 (PO, 29/09)

Yêu cầu "đọc flow của một `Seed_*` có sẵn" **đã bỏ**. Mục đích của nó — không tự đặt quy ước mới —
đã đạt bằng suy luận từ tên (`Seed_ChainTests_A/B/C`, `Seed_BenefitSchedule` + `_Upsert`).
Phần còn lại kiểm bằng **hành vi**: `163 of 163`, bấm hai lần không thành 326.

## context_graph — thêm điểm tựa cho giả thuyết kích thước OML

PO cung cấp mốc: còn chạy ở **rev 177 (13:37)** và **rev 178 (14:37)**, chết **sau khi harness thêm
node ở rev 179**. Khớp chiều với giả thuyết OML vượt ngân sách thời gian conversion.
Positive control 29/09: ClaimMVP `root=FormatVND` **timeout**, PizzaOrder cả app **thành công
3.264.912 ký tự** ⇒ tool tốt, lỗi riêng ClaimMVP.
⇒ **Mọi verify trên app này từ giờ dựa vào HÀNH VI.** Harness là **thiết bị đo chính**, không phải
tiện ích — thêm một lý do ReplayMode phải lên trước.

---

## Guard — publish **rev 188** (hai run, `errorCount 7` · `warningCount 134` cả hai)

### Phần 1 — wire model + parser (run `a2ef284c…`)

| Structure | Attribute | Type sau sửa |
|---|---|---|
| `IdpWireOuter` | `errorCode` | **Integer** (từ Text(50)) |
| `IdpWireOuter` | `pageCount` | **Integer** (mới) |
| `IdpWireInner` | `errorCode` | **Integer** (từ Text(50)) |
| `IdpWireInner` | `pageCount` | **Integer** (mới) |

`errorMessage` giữ Text(500) ở cả hai. Attribute khác không đổi.

**Lấy mã khác 0 đầu tiên, ba tầng, đúng thứ tự:**
```
If(Envelope.result.result.errorCode <> 0, IntegerToText(Envelope.result.result.errorCode),
If(Envelope.result.errorCode        <> 0, IntegerToText(Envelope.result.errorCode),
If(Envelope.data.result.errorCode   <> 0, IntegerToText(Envelope.data.result.errorCode), "")))
```
Gán vào `Result.ErrorCode` **trước** chỗ kiểm `FIELDMAP_MISSING` ⇒ mã IDP **thắng**, đúng thứ tự
đã dặn (vì `FIELDMAP_MISSING` chỉ nổ khi `ErrorCode` còn rỗng).

### Phần 2 — hai wrapper (run `b41c297b…`)

**Guard A — `IdpSubmitSync`**, đọc `ParseResult.Result.ErrorCode` (Text):
- mã số: `TextToInteger(ParseResult.Result.ErrorCode) > 0` ⇒ `Success = False`,
  `ErrorCode = "EXTRACTION_ERROR"`, message nêu con số.
- ký hiệu: `= "PARSE_FAILED"` hoặc `= "TEMPLATE_NOT_ASSIGNED"` ⇒ `Success = False`,
  `EXTRACTION_ERROR`, message phân biệt hai ca.

**Guard A — `IdpSubmitDocument`**, đọc `DeserializeResponse.Data.ErrorCode` (Integer):
`> 0` ⇒ `Success = False`, `EXTRACTION_ERROR`, message nêu con số.

**`FIELDMAP_MISSING` giữ `Success = True`** — Mentor xác nhận: `TextToInteger("FIELDMAP_MISSING")`
= 0 nên không khớp nhánh mã số, và cũng không khớp nhánh ký hiệu, nên flow rơi xuống End với
`Success` vẫn `True`. **Đây là quyết định của tôi, không phải yêu cầu của PO** — lý do: trích xuất
đã chạy được, thiếu là field map của phía ta, tức khoảng trống cấu hình. Ghi ra để bác được.

**Không nhánh nào ghi claim status, không nhánh nào map sang `Declined`.** `EXTRACTION_ERROR` là
mã lỗi duy nhất các node mới đặt.

**Guard B — `IdpSubmitResponse2` sau sửa:**
`RequestId` Long Integer · `PageCount` Integer · **`ErrorCode` Integer (mới)** ·
**`ErrorMessage` Text(500) (mới)** · **`StatusCode` Integer (mới)**

Điều kiện: `DeserializeResponse.Data.RequestId > 0` — **khẳng định dương**, nhánh nghịch `<= 0` đi
đường lỗi. Đặt **ngay sau `DeserializeResponse` và TRƯỚC** assign gán `RequestId`/`PageCount`, nên
assign thành công **chỉ tới được khi `RequestId > 0`**. Message dựng từ ba trường, mỗi trường chỉ
thêm vào khi khác 0 / khác rỗng.

**Guard C — `Settings.IdpUserAgent` TỒN TẠI.** Đây là lần đầu xác nhận được — ghi chú 23/09 nói
"chưa xác minh được Setting này có thật" vì MCP không đọc được Settings. Mentor đọc model và
xác nhận có, kiểu Text. Rỗng ⇒ dừng trước khi gọi `IdpUpload`.

### Phép thử miễn phí cho Guard A, chưa chạy

Dán `fixtures/6093-sync-201-errorCode-1014.json` vào harness:
**kỳ vọng `ErrorCode = "1014"`, KHÔNG phải `PARSE_FAILED`** · `PageCount = 1` (trang đã bị trừ).
Đây là phép thử trực tiếp cho cả phần 1 (đọc errorCode mọi tầng) lẫn phần 2 (Guard A).

---

## ReplayMode step 1 — data model (run `f589b742…`)

Thứ tự PO chốt 30/09: Stage 0 → seed → **guard** → ReplayMode. Guard xong ở rev 188, nên ReplayMode
là việc cuối. Không phụ thuộc Stage 0 hay seed nên làm được song song với phần PO bấm nút.

Chia hai run theo đúng điều kiện tôi tự đặt khi PO cho ngoại lệ tạo Entity:
**data model trước, verify bằng máy, rồi mới logic.**

### Thiết kế khoá tra cứu — quyết định của tôi

Brief nói *"return the stored RawJson for the matching key"* nhưng không nói khoá gồm gì.
Chọn **`(Key, Endpoint)`**, không phải `Key` một mình:

- `Key` = `ClaimNo` truyền vào wrapper. Plain Text, **không** FK tới `Claim`, để một capture sống
  lâu hơn hồ sơ nó được bắt từ đó.
- `Endpoint` = `"POST /request/sync"` hoặc `"POST /request"` ⇒ **đường sync và async replay độc lập**
  từ cùng một `Key`. Nếu chỉ khoá theo `Key` thì hai đường tranh nhau một dòng.

Không tìm thấy dòng khớp ⇒ ra `EXTRACTION_ERROR` nêu rõ *"ReplayMode đang bật nhưng không có dòng
IdpReplay nào có Key = ..."*. **Không** fallback ngầm sang `TEST-DIAG` — fallback ngầm là đúng loại
"trông như chạy được" mà cả nhánh này đang cố diệt.

### Data model xong — publish **rev 189**

Mentor báo (`errorCount 7` · `warningCount **136**`, +2 notice *"unused"* cho `IdpReplay` và Setting
`ReplayMode` — đúng, chưa wire):

| # | Attribute | Type | Length | Mandatory |
|---|---|---|---|---|
| 1 | `Id` | Long Integer | — | PK, auto |
| 2 | `Key` | Text | 100 | ✓ |
| 3 | `Endpoint` | Text | 100 | ✓ |
| 4 | `RawJson` | Text | 2147483647 | ✓ |
| 5 | `Note` | Text | 500 | |
| 6 | `CreatedAt` | Date Time | — | ✓ |

**Không FK nào** — `Key` là Text thuần, không trỏ `Claim`.
`AgentRun` **15 → 16** attribute, chỉ thêm `Replay`. Setting `ReplayMode` Boolean default `False`.

> Mentor bắt đúng một chỗ tôi viết lỏng: tôi nói "AgentRun hiện có 16 attribute" rồi liệt kê 15.
> Tôi đã dặn "nếu thấy 16 thì báo lại chứ đừng cho là tôi sai" — nó báo **trước 15, sau 16**.
> Đúng cách.

### ❗ Sửa lại cam kết của tôi: không thể verify draft bằng máy

Tôi hứa "verify bằng `context_entities` **trước** khi publish". **Không làm được như phát biểu** —
`context_*` đọc model **đã publish**, còn entity mới chỉ nằm trong draft. Tra `IdpReplay` lúc chưa
publish trả về 3 entity khác, **không có** `IdpReplay` — không phải vì nó không tồn tại mà vì
nó chưa publish. Cùng họ với bài học index-lag đã ghi.

⇒ Thứ tự đúng là **publish rồi verify**, và publish data model là an toàn vì nó **inert**:
Setting default `False`, `AgentRun.Replay` default `False`, `IdpReplay` 0 dòng, chưa ai đọc.
Giữ được tinh thần cam kết (verify **trước khi logic vào**), chỉ đổi chỗ publish.

### Verify được ngay ở rev 187 — `IdpFieldMap.Origin`

`Origin` : Text(100), mandatory, **`defaultValue "field"`**, timestamp `2026-09-30T04:03:11`.
Đúng đặc tả. Đây là **nửa** của bẫy `Origin = ""` — nửa còn lại là coalesce trong helper, chỉ
chứng minh được bằng phân bố sau seed.

## ReplayMode wrapper logic — applied (Mentor run cb70d620, unpublished at time of writing)

Source: Mentor's terminal report. **Not machine-verified** — `context_*` reads the
published model only, so this is verified after publish, or by behaviour in the harness.

Branch sits after Guard C, before any `IdpUpload` call, in both wrappers:

| | `IdpSubmitSync` | `IdpSubmitDocument` |
|---|---|---|
| Decision node | `SyncReplayMode?` after `SyncUaEmpty?` = False | `DocReplayMode?` after `DocUaEmpty?` = False |
| True | `SyncGetReplayRow` → `SyncReplayRowFound?` → `SyncParseReplay` / `SyncReplayMissing` | `DocGetReplayRow` → `DocReplayRowFound?` → `DocDeserializeReplay` / `DocReplayMissing` |
| False | existing `PageCount > IdpSyncMaxPages` → `GetToken` → `CallSubmitSync` | `GetToken` → `CallSubmit` |
| Aggregate | `Key = ClaimNo` AND `Endpoint = "POST /request/sync"`, MaxRecords 1 | `Key = ClaimNo` AND `Endpoint = "POST /request"`, MaxRecords 1 |

No-row `ErrorMessage`:

    "ReplayMode is on but no IdpReplay row has Key = " + ClaimNo + " and Endpoint = POST /request/sync."
    "ReplayMode is on but no IdpReplay row has Key = " + ClaimNo + " and Endpoint = POST /request."

Only exit from the no-row branch is an End node with `Outcome.Success = False`.
No fallback to a live call. No fallback to another row.

Replay parse path: `SyncParseReplay` calls `IdpParseExtraction` with
`IsSyncShape = True` and `RawJson = SyncGetReplayRow.List.Current.IdpReplay.RawJson`
— identical arguments to the live `ParseResult` node. `FIELDMAP_MISSING` falls
through to the same `Outcome.Success = True` End node as the live path.

`AgentRun`: Mentor was told to check rather than assume. Result — **neither wrapper
touches `AgentRun`**; no write was invented. `WriteAgentRun` is the append-only
audit action, called from the orchestration layer above these wrappers.

Untouched, per Mentor: Guard A, Guard B, Guard C, the `IdpSyncMaxPages` check,
`IdpUpload.SubmitSync` / `IdpUpload.Submit` on the `ReplayMode? = False` branch
with arguments unchanged. Nothing maps to `Declined`; nothing writes a claim status.

### Two deltas to verify after publish

1. **The replay path has its own parallel guard nodes, not a join back into the
   live ones.** `SyncReplayNumericError?`, `SyncReplaySymbolicError?`,
   `DocReplayNumericError?`, `DocReplayRequestIdOk?` duplicate the live conditions
   instead of reusing the nodes. Conditions are stated identical. Consequence: a
   future change to a guard condition must be made in two places. Accepted for now;
   recorded so the duplication is not discovered later as a surprise.
2. **`IdpSubmitResponse2`** — Mentor named the document-side deserialise target
   this. Unknown whether that is the node name or a Structure Mentor created
   without authorisation (CLAUDE.md §2 forbids Mentor creating Entities; a
   Structure was not authorised either). Read `context_structures` after publish
   and confirm. If it is a new Structure, it comes out.

Validation on this run: `errorCount 7`, `warningCount 135` — baseline. (Mentor's
prose said 136; the structured `validation` field said 135. The field wins.)

## Behaviour test for ReplayMode — the instrument, written before the run

`context_graph` is dead on ClaimMVP and `context_*` cannot see action internals,
so ReplayMode is verified by BEHAVIOUR, not by a model read. Steps, in order:

1. Administration → "Replay fixture store": Key `TEST-DIAG`,
   Endpoint `POST /request/sync`, paste the 6093 fixture into the RawJson box,
   press **Save to IdpReplay**. Expect: `action=created`, `bytes=7100`.
2. Press **Load from IdpReplay** with the box cleared. Expect the box refills and
   `bytes=7100`. This proves the row is byte-exact without a 7 KB literal in the OML.
3. Set `Settings.IdpReplayMode = True`.
4. Run the real diagnostics button against a claim whose `ClaimNo` = `TEST-DIAG`.
   Expect: a parse result identical to the offline harness on the same fixture
   (`MandatoryTotal 8`, `Completeness 1.0000`, `ClaimConfidence 0.5544`,
   `LineItemsExtracted True`), and **the Portal Integrations log empty for that run**.
   The empty log is the actual proof no HTTP call left the app.
5. Change the Key to something no row has. Expect `Outcome.Success = False`,
   `ErrorCode = EXTRACTION_ERROR`, message naming Key and Endpoint. Expect NOT:
   a live call, a different row, or any claim state change.
6. Save the 1014 fixture under Key `TEST-1014`, Endpoint `POST /request/sync`,
   replay it. Expect `ErrorCode = "1014"` (not `PARSE_FAILED`) and `PageCount = 1`.

Step 4 is the one that matters: it is the only step that can distinguish
"ReplayMode branch exists" from "ReplayMode branch is actually taken".

## CORRECTION — rev 190 replaces the account above

The Mentor session holding the first ReplayMode logic draft (run `cb70d620`) **died
before it was published**. Positive control: an older already-succeeded run in the same
session also returned `not_found`, which is session loss, not run loss. `app_info` showed
revision 189 with my own timestamp — so, unlike the 2026-09-28 occurrence, **no outside
publish was involved**. A Mentor session can simply die.

Cost was doubled by my own sequencing: the harness-button run had been queued onto the
same unpublished draft. Both were lost. Rule going forward, now in memory: publish after
EVERY behaviour-changing Mentor run; never chain a second run onto an unpublished draft.

### Rebuilt and published as rev 190 (run 012836cd) — this is the version that exists

The rebuild asked for what the original brief actually specified and the first attempt
did not deliver: **one shared guard chain, not duplicates.** Mentor achieved it.

| | `IdpSubmitSync` | `IdpSubmitDocument` |
|---|---|---|
| Decision node | `SyncReplayMode?` after Guard C `SyncUaEmpty?` | `DocReplayMode?` after Guard C `DocUaEmpty?` |
| Condition | `Settings.ReplayMode` | `Settings.ReplayMode` |
| Aggregate | `GetSyncReplay`, MaxRecords 1 | `GetDocReplay`, MaxRecords 1 |
| Filters | `IdpReplay.Key = ClaimNo` AND `IdpReplay.Endpoint = "POST /request/sync"` | `IdpReplay.Key = ClaimNo` AND `IdpReplay.Endpoint = "POST /request"` |
| Convergence variable | `ParsedResult` (`IdpExtractionResult`) | `ParsedData` (`IdpSubmitResponse2`) |

Guard nodes that now exist **exactly once** per action, read from the convergence variable:

- `IdpSubmitSync`: `LiveHttpStatus = 200 or LiveHttpStatus = 201` (Guard B) ·
  `TextToInteger(ParsedResult.ErrorCode) > 0` → `SyncIdpNumericError?` (Guard A1) ·
  `ParsedResult.ErrorCode = "PARSE_FAILED" or ParsedResult.ErrorCode = "TEMPLATE_NOT_ASSIGNED"`
  → `SyncIdpSymbolicError?` (Guard A2) · one success assignment
  (`Outcome.Success = True`, `Result = ParsedResult`).
- `IdpSubmitDocument`: `ParsedData.RequestId > 0` → `DocRequestIdOk?` (Guard B) ·
  `ParsedData.ErrorCode > 0` → `DocIdpNumericError?` (Guard A) · one success assignment
  (`Outcome.Success = True`, `RequestId = LongIntegerToText(ParsedData.RequestId)`).

No-row `ErrorMessage`, verbatim:

    "ReplayMode is on but no IdpReplay row has Key = " + ClaimNo + " and Endpoint = POST /request/sync."
    "ReplayMode is on but no IdpReplay row has Key = " + ClaimNo + " and Endpoint = POST /request."

No fallback to a live call, no fallback to another row; only exit is an End node with
`Outcome.Success = False`.

`AgentRun`: checked, not assumed. Neither wrapper touches it. `WriteAgentRun` is called by
`TransitionClaimStatus`, `Portal_SubmitClaim`, `Portal_SendSupplement` and their service-action
counterparts. No `AgentRun` write was invented in the wrappers.

No Entity and no Structure created. `IdpExtractionResult` and `IdpSubmitResponse2` were reused.
Validation 7 errors / 135 warnings — baseline.

### Verified by machine (context_search on the published rev 190), not by Mentor's report

- `IdpReplay` attributes: `Id` (Long Integer, PK) · `Key` (Text 100) · `Endpoint` (Text 100)
  · `RawJson` (Text 2147483647) · `CreatedAt` (Date Time, **mandatory**) · `Note` (Text 500, optional).
- Both wrapper actions carry timestamp `2026-09-30T06:36:22Z`, i.e. this publish, and their
  descriptions now state the ReplayMode behaviour. The actions did change.
- The Setting is **`Settings.ReplayMode`**. My prompt asked for `Settings.IdpReplayMode`;
  Mentor used the real name created at rev 189. The correction is to my wording, not the build.

### New limitation to record, arising from the convergent design

`IdpSubmitSync`'s Guard B now reads a local `LiveHttpStatus`, which the **replay path sets to
a literal 200**. Consequences:
- The replay path cannot exercise a non-2xx HTTP status. Any future fixture representing an
  HTTP-level failure (502, 429, Cloudflare 403 error 1010) will NOT be reproducible through
  ReplayMode as built; it would need the stored status carried on `IdpReplay`.
- The stored 6093 fixture was HTTP 201, and the guard accepts 200 or 201, so the fixture
  passes either way. The limitation is about what ReplayMode can never test, not about the
  fixture in hand.
Not fixed, not smuggled in as scope. Recorded as a known gap.

## Harness buttons — published as rev 191 (run 753b9493)

New section "Replay fixture store" on Administration, after "Parse a captured response".
Inputs: `ReplayKey` (default `TEST-DIAG`), `ReplayEndpoint` (default `POST /request/sync`).

The existing textarea's local variable is **`RawJsonInput`** — machine-relevant name, both
new actions read/write only that one variable from the existing section.

`SaveToIdpReplay` — guard on empty first, then Aggregate `GetReplayRowForSave`
(`Key = ReplayKey` AND `Endpoint = ReplayEndpoint`, MaxRecords 1), then
`CreateOrUpdateIdpReplay` via a local `ReplayRec`:

| | create | update |
|---|---|---|
| `Key` | `ReplayKey` | `ReplayKey` |
| `Endpoint` | `ReplayEndpoint` | `ReplayEndpoint` |
| `RawJson` | `RawJsonInput` | `RawJsonInput` |
| `CreatedAt` | `CurrDateTime()` | **preserved** from the existing row |
| `Note` | empty | **preserved** from the existing row |
| `Id` | auto | existing row's Id |

Preserving `CreatedAt` on update matters: re-saving a fixture does not reset when it was captured.

Feedback expressions as built, verbatim:

    "Saved to IdpReplay. Key=" + ReplayKey + " Endpoint=" + ReplayEndpoint + " bytes=" + IntegerToText(Length(RawJsonInput)) + " action=" + If(GetReplayRowForSave.List.Empty, "created", "updated")
    "Loaded from IdpReplay. Key=" + ReplayKey + " Endpoint=" + ReplayEndpoint + " bytes=" + IntegerToText(Length(GetReplayRowForLoad.List.Current.IdpReplay.RawJson))
    "No IdpReplay row with Key=" + ReplayKey + " Endpoint=" + ReplayEndpoint

Byte count only; RawJson content never appears in a message. Nothing in "Parse a captured
response" changed — textarea, checkbox, button and `RunIdpParseExtraction` untouched.
No Entity or Structure created; no IDP or REST call in either action.

### TWO DEFECTS IN REV 191 — not fixed yet

**1. The Ajax Refresh was never built.** Mentor's model API rejected the `ParseCard`
container as an Ajax Refresh target. Consequence: `LoadFromIdpReplay` assigns `RawJsonInput`
but **the textarea does not visibly refill** until the next interaction or screen refresh.
The button looks broken even though the data is loaded. Step 2 of the behaviour test above
cannot pass as written until this is wired.

**2. `warningCount` is 136, not 135 — one above baseline.** Mentor's prose asserted
"135 — identical to baseline. No new errors or warnings were introduced." The structured
`validation` field returned 136. The field wins over the prose. Most likely the unwired
refresh, but that is a hypothesis, not a reading — being chased in the next run.

Lesson reinforced: Mentor's own count in prose is not evidence. Read `result.validation`.

## Diagnosis run (bddde47d) — `changeApplied: false`, nothing published, rev 191 stands

Mentor was asked to (a) identify the extra warning by reading it, not guessing, and
(b) wire the Ajax Refresh or stop and report exact failures. It did both. Note: an
intermediate reasoning event claimed "the Ajax Refresh wired successfully to
TextArea_RawJsonInput" — the terminal report contradicts it. Never act on a
non-terminal Mentor event; that claim was false.

### The 136th warning, identified — it is MINE, not pre-existing

Exact text:

    Security Warning — You're exposing a database operation in the client side.
    Validate the data in a Server Action before changing the database.

Element: `IExecuteServerActionNode` at
`/ClaimMVP/MainFlow/Administration/SaveToIdpReplay/UpsertReplayRow`

Cause: `SaveToIdpReplay` is a **screen action** (client side) and calls the entity action
`CreateOrUpdateIdpReplay` directly. ODC flags any client-side database write. This warning
did not exist at baseline because `SaveToIdpReplay` did not exist. It is a direct
consequence of the save button I specified.

It is an advisory, not an error, and it does not block publish (rev 191 published clean).
But it is a real finding: an unauthenticated-path client-side write to an Entity.
Mitigating factors: Administration is a console screen behind a role, and `IdpReplay`
holds no money fields (CLAUDE.md §2 is not violated). Still, the correct shape is a
Server Action that performs the upsert.

**BLOCKED ON A FENCE — needs an explicit decision, not a workaround.**
Fixing it requires creating ONE new Server Action (e.g. `UpsertIdpReplay`) to hold the
write, and having `SaveToIdpReplay` call that instead. That is outside the fence I set
("Administration screen only"), so it was NOT done. Mentor stopped at the fence and said
so, which is the correct behaviour. Options for the PO:
- (A) extend the fence to allow one new Server Action `UpsertIdpReplay` → warning returns to 135;
- (B) accept the warning as a known harness-only advisory and record it → stays at 136.
Not decided. Not worked around.

### The Ajax Refresh CANNOT be created through Mentor's API — hand edit required

Both targets were found successfully and both rejected at the `WidgetOrWebBlock` setter
with the identical exception, no inner message:

    Exception has been thrown by the target of an invocation.

- Target 1 `TextArea_RawJsonInput` (the textarea widget itself) — found, setter threw.
- Target 2 `ParseCard` (its immediate parent container) — found, setter threw.

The `IAjaxRefreshNode` is created without error; assigning ANY widget to it fails at
runtime. This is a limitation of the model API, not of the app. It is now a recorded
Mentor limitation alongside "ODC cannot represent a List of List".

**Hand edit for the PO, in ODC Studio:** open `LoadFromIdpReplay`, find the Info message
node, insert an Ajax Refresh node between it and the End node, target
`TextArea_RawJsonInput`. Fallback target if Studio also rejects it: `ParseCard`.
Until that node exists, "Load from IdpReplay" loads the data but the textarea does not
visibly refill — step 2 of the behaviour test cannot pass.

## Rev 192 — fence extended by PO decision, Security Warning resolved, back to baseline

PO extended the fence to allow exactly ONE new Server Action. Run 1163af3c,
`changeApplied: true`, validation **7 errors / 135 warnings — back to baseline**.
The Security Warning on `SaveToIdpReplay/UpsertReplayRow` is gone.

New Server Action `UpsertIdpReplay`:
- In: `Key` (Text, mandatory) · `Endpoint` (Text, mandatory) · `RawJson` (Text, mandatory)
- Out: `WasCreated` (Boolean) · `ByteCount` (Integer)
- Local: `Rec` (IdpReplay record), staged before the entity action
- Empty guard: `RawJson = ""` → writes NOTHING, returns `WasCreated = False`, `ByteCount = 0`
- Create: `Rec.Key`, `Rec.Endpoint`, `Rec.RawJson` from inputs · `Rec.CreatedAt = CurrDateTime()`
  · `Note` left empty · `Id` auto via `CreateIdpReplay` → `WasCreated = True`
- Update: `Rec.Id`, `Rec.Note`, `Rec.CreatedAt` all taken from
  `GetExistingRow.List.Current.IdpReplay.*` (**preserved**) · `Rec.RawJson` from input
  → `UpdateIdpReplay` → `WasCreated = False`
- `ByteCount = Length(RawJson)` on both write branches

`SaveToIdpReplay` is now Start → `RawJsonEmpty?` → (True) Warning → End /
(False) `CallUpsertIdpReplay` → Info → End. Deleted from the screen action:
`GetReplayRowForSave` aggregate, its `RefreshData` node, `ReplayRec` local variable,
`UpsertReplayRow` (the old `CreateOrUpdateIdpReplay` call), `AssignCreate`,
`AssignUpdate`, `ReplayRowFound?`, and the old Info/End pair.
**No database write remains in any screen action.**

Info feedback expression as built, verbatim:

    "Saved to IdpReplay. Key=" + ReplayKey + " Endpoint=" + ReplayEndpoint + " bytes=" + IntegerToText(CallUpsertIdpReplay.ByteCount) + " action=" + If(CallUpsertIdpReplay.WasCreated, "created", "updated")

Untouched: `LoadFromIdpReplay` and its `GetReplayRowForLoad` aggregate (a client-side
READ, which the warning does not cover) · `IdpSubmitSync` · `IdpSubmitDocument` ·
the "Parse a captured response" section. `RawJsonInput` is only read by
`SaveToIdpReplay`, passed as the `RawJson` argument.

### Still open on rev 192

- **The Ajax Refresh node is still missing.** Mentor cannot create it (see the
  diagnosis run above). Hand edit in Studio: `LoadFromIdpReplay`, insert an Ajax Refresh
  between the Info message and End, target `TextArea_RawJsonInput`, fallback `ParseCard`.
  Until then Load fetches the row but the textarea does not visibly refill.
- Baseline for every future run is **7 errors / 135 warnings** again.

## CORRECTION — there is no Ajax Refresh defect. The load already works.

Read-only investigation, run c4a6438d, `changeApplied: false`. This overrides the
"TWO DEFECTS IN REV 191" and the "hand edit in Studio" instructions above. **No Studio
trip is needed. Nothing is blocked.**

### PO hypothesis tested and falsified — but the conclusion it pointed at is right

Hypothesis was: `AjaxRefresh` does not exist in ODC at all (ODC is Reactive-only;
Ajax Refresh is an OutSystems 11 Traditional Web concept), so "every target rejected"
means wrong node class, not wrong target.

Enumerated node types the model API allows in a screen action:

    IStartNode, IEndNode, IIfNode, ISwitchNode, IForEachNode, IAssignNode,
    IExecuteServerActionNode, IExecuteClientActionNode, IMessageNode,
    IRefreshDataNode, IAggregateNode, IJavaScriptNode, IJSONDeserializeNode,
    IJSONSerializeNode, IRecordListToExcelNode, IExcelToRecordListNode,
    IDownloadNode, IAttachFileNode, ISendEmailNode, ISQLNode, IRaiseExceptionNode,
    IExceptionHandlerNode, IDestinationNode, ICommentNode, ICallAgentNode,
    ICallAgentCoreNode, ISemanticSearchNode, ITriggerNode, ITriggerGlobalNode,
    IDecisionOutcomeNode, IAjaxRefreshNode

**`IAjaxRefreshNode` IS in the list.** The type exists in the API. The failure was
genuinely the `WidgetOrWebBlock` setter rejecting every widget, not a wrong node class.

### But the node is not needed, because LoadFromIdpReplay is already correct

Verbatim dump of `LoadFromIdpReplay` on rev 192:

    1. IStartNode        — (unnamed)
    2. IRefreshDataNode  — "Refresh GetReplayRowForLoad", DataSource = GetReplayRowForLoad
    3. IIfNode           — "LoadRowFound?", condition: not GetReplayRowForLoad.List.Empty
                             True  -> node 4
                             False -> node 7
    4. IAssignNode       — (unnamed)
                             RawJsonInput = GetReplayRowForLoad.List.Current.IdpReplay.RawJson
    5. IMessageNode      — Info, "Loaded from IdpReplay. Key=" + ... + " bytes=" + IntegerToText(Length(...))
    6. IEndNode
    7. IMessageNode      — Warning, "No IdpReplay row with Key=" + ReplayKey + " Endpoint=" + ReplayEndpoint
    8. IEndNode

- Is there an Assign writing to `RawJsonInput`? **Yes**, node 4.
- Its right-hand side: `GetReplayRowForLoad.List.Current.IdpReplay.RawJson`.
- Position: **after** the `RefreshData` (node 2) and **after** the `LoadRowFound?` check
  (node 3), on that check's True branch.

Alternative hypotheses (i) missing Assign and (ii) Assign before the data arrives:
**neither applies.** In ODC Reactive, assigning a local variable that a textarea is bound
to re-renders the widget automatically. The load works as built.

### My error, plainly

"The textarea does not visibly refill" was never observed. It came from Mentor's rev 191
report and I passed it on as a defect, then escalated it into a request for manual Studio
work. I should have read `LoadFromIdpReplay` before reporting a defect in it. The Assign
was there the whole time.

Corollary: the `IRefreshDataNode` in `LoadFromIdpReplay` is the correct ODC pattern for
re-running a Fetch-on-Demand aggregate inside a screen action. That is the node to reach
for, not Ajax Refresh.

**Status: nothing outstanding on the harness. The behaviour test can run as written.**

## ReplayMode wiring CONFIRMED by model read — no build work remains (run e61dfe30)

Read-only: `attemptedChange: false`, `changeApplied: false`. Asked instead of tested,
because pressing the real button costs a page of quota if the answer is "not wired", and
every runtime probe (including measuring elapsed time) only reveals "it was live" after
the page is already spent.

### `IdpSubmitSync` — node numbers that matter

`SyncReplayMode?` = **node 4** · `GetToken` = node 10 · `CallSubmitSync` = node 15

    1  Start                                   → 2
    2  If  SyncUaEmpty?     Settings.IdpUserAgent = ""
                            TRUE  → 3 (SyncUaFail → End)      FALSE → 4
    4  If  SyncReplayMode?  Settings.ReplayMode
                            TRUE  → 5                          FALSE → 8
    5  Aggregate GetSyncReplay                 → 6
    6  If  SyncReplayFound?  GetSyncReplay.Count > 0
                            TRUE  → 7a                         FALSE → 7b
    7a ExecuteAction ReplayParse → IdpParseExtraction          → 7c
    7b Assign SyncReplayNoRow    Success=False, EXTRACTION_ERROR → End
    7c Assign SyncReplayAssign   ParsedResult = ReplayParse.Result
                                 LiveHttpStatus = 200          → 13
    8  If  PageCount > Settings.IdpSyncMaxPages?
                            TRUE  → 9 (PAGE_LIMIT_EXCEEDED → End)  FALSE → 10
    10 ExecuteAction GetToken → IdpGetToken    → 11 → 12
    12 If  Outcome.Success?  TRUE → 15          FALSE → 13a (TOKEN_FAILED → End)
    15 ExecuteAction CallSubmitSync  ← THE LIVE HTTP CALL  → 15a → 15b/15c
    13 If  LiveHttpStatus = 200 or 201?  ← shared convergence point
    14a Assign Success=True → 16 · 14b Assign EXTRACTION_ERROR → End
    16 If SyncIdpNumericError? → 17a / 17b → 18a → End
    -- ExceptionHandler AllExceptions → Assign EXCEPTION → End

**Reachability:** `CallSubmitSync` (15) ← 12-True ← 10 ← 8-False ← **node 4 FALSE only**.
The `ReplayMode = True` path is 4 → 5 → 6 → 7a → 7c → 13 → 14a → 16 → End and passes
through neither node 10 nor node 15.

### `IdpSubmitDocument` — same shape

`DocReplayMode?` = **node 4**, condition `Settings.ReplayMode`; TRUE → 5 `GetDocReplay`,
**FALSE → 8 `GetToken`**. Node 6 `DocReplayFound?` (`GetDocReplay.Count > 0`) → 7a
`ReplayDeserialize` (JSON Deserialize, input
`GetDocReplay.List.Current.IdpReplay.RawJson`) → 7c `DocReplayAssign`, or → 7b
`DocReplayNoRow` (EXTRACTION_ERROR → End). Neither replay path reaches `GetToken` or
`CallSubmit`.

### The two answers

- `ReplayMode = True` + matching row → **no HTTP call leaves the app.** Not even the token
  call: `GetToken` is on the False branch of node 4.
- `ReplayMode = True` + no matching row → **no HTTP call either**; `SyncReplayNoRow` /
  `DocReplayNoRow` assign `EXTRACTION_ERROR` and end.

### Save / Load verified by BEHAVIOUR, not by node read (PO, rev 192)

    Save → "bytes=3536 action=updated"
    clear box → 0 chars → Load → box refilled itself to 3536 chars, byte-identical

This retires the "textarea does not refill" defect a second time, and this time by the
thing happening rather than by reading the Assign. `UpsertIdpReplay` works; no
client-side Entity write remains.

### Expected when step 3 finally runs — so a failure is not misattributed

`ReplayParse` calls the same `IdpParseExtraction`, so the `errorCode` → `"0"` bug is
upstream of the replay path too: `IdpRefused?` will fire and the result will be hollow.
The Integrations-log-empty check still proves what it is for (no HTTP left the app), but
the parse assertions (`MandatoryTotal 8`, `ClaimConfidence 0.5544`, `Completeness 1.00`)
will fail, and that failure belongs to the coalesce bug, not to ReplayMode.

**Remaining ReplayMode work: none in the model.** Only the PO actions: flip
`Settings.ReplayMode`, run, read the Integrations log.

## Agreed order (PO, unchanged)

1. coalesce `errorCode` — treat 0 / absent as no error
2. the three arithmetic bugs in `IdpParseExtraction`
3. then the ReplayMode log-read above

## Two consequences of the node trace — PO-flagged, recorded as standing constraints

### (a) ReplayMode does not rehearse the authentication layer

`GetToken` is node 10 in `IdpSubmitSync` and node 8 in `IdpSubmitDocument`, and in both it
sits on the **False** branch of the `ReplayMode?` decision at node 4. So a replay run never
calls `IdpGetToken`, never exchanges credentials, and never exercises the token path.

**Rule, binding before the pitch:** there must be at least one genuinely live run before any
demo. A silent, green ReplayMode run is NOT evidence that authentication works — it is
evidence that authentication was skipped. Do not read ReplayMode's silence as "everything is
fine". This also means Stage 4 (credentials → ODC Secrets, verified via the free `IdpPing`)
cannot be validated through ReplayMode at all.

### (b) The replay branch hard-codes `LiveHttpStatus = 200`

Node 7c `SyncReplayAssign` sets `LiveHttpStatus = 200` unconditionally. Consequence: **a
replayed run is indistinguishable from a real HTTP 200** downstream of node 13 — same guard
path, same `Outcome.StatusCode`, same shape in any log that records status.

`AgentRun.Replay` was created as an attribute at rev 189 precisely so a run could be marked
as replayed. Open question being read now: **does anything actually set it?** Node 7c is the
only place in `IdpSubmitSync` that could. Earlier reads established that neither wrapper
touches `AgentRun` at all, which strongly suggests the flag is declared but never written —
in which case the audit trail cannot distinguish a replay from a real call, and that is the
last remaining piece of ReplayMode work, not a cosmetic gap.

Related known limitation already recorded above: because `LiveHttpStatus` is forced to 200,
ReplayMode can never reproduce a non-2xx HTTP failure (502, 429, Cloudflare 403 error 1010).

## Execution plan agreed with PO — publishes kept SEPARATE

    0. Read IdpParseExtraction verbatim (node order + conditions). No edits. Send the dump
       before building anything. Reason: Mentor's account of this action is already proven
       wrong, and designing against a wrong picture fixes the wrong place.
    1. Publish the errorCode coalesce ALONE. Stop. Wait for the harness measurement.
    2. Publish the three arithmetic fixes ALONE. Stop. Wait for the harness measurement.

Not combined: if the numbers come out wrong, the measurement has to identify which change
did it.

### Coalesce decision — FINAL (PO)

Keep the wire attributes as **Integer**. Do NOT switch them to Text. Map zero to empty at
the point of assignment:

    If(wire.errorCode = 0, "", IntegerToText(wire.errorCode))

Applied at all three: `IdpWireOuter.errorCode`, `IdpWireInner.errorCode`,
`IdpSubmitResponse2.errorCode`. `IdpRefused?` is NOT touched. The 1014 path is NOT touched.
Rationale: a bare JSON number deserialising into a Text attribute risks silently yielding
`""`, which would break the 1014 guard that currently works; and IDP has no code 0 (codes
are 16 and >= 1000), so treating 0 as absent is lossless.

## STEP 0 — IdpParseExtraction read verbatim (run 92a55b4c, read-only, changeApplied false)

This read overturns the diagnosis both Mentor and I were working from. **Do not build from
the earlier account.**

### Structure: two mutually exclusive paths that never converge

    1   Start                → Init
    2   Assign  Init          Result.RawJson = RawJson; Result.ErrorCode = ""
                              Result.ErrorMessage = ""; Result.LineItemsExtracted = True
    3   JSONDeserialize       IdpWireEnvelope, input RawJson
    4   Assign  AssignEnvelope   Envelope = ...Data
    5   If      IsSyncShape   TRUE → 6s (sync coalesce)   FALSE → 6a (async coalesce)
    6s..12s / 6a..12a         three-level candidate pick; each sets Candidate.* and
                              CandidateFound True, or Sync:None / Async:None → False
    13  If      CandidateFound?     TRUE → 17    FALSE → 14
    14  Assign  IdpRefused    Result.ErrorCode = If(...<>0, IntegerToText(...), ... , "")
    15  If      IdpRefused?   Result.ErrorCode <> ""
                              TRUE → 16T SetIdpRefusedMsg → End
                              FALSE → 16F ParseFailed (PARSE_FAILED) → End
    17  Assign  TopLevel      Result.RequestId / ProcessingTime / MinioPath
                              Result.ErrorCode = Candidate.errorCode      ← NO GUARD
                              Result.ErrorMessage = Candidate.errorMessage
    18  If      NoTemplates?  Candidate.templates_data.Length = 0
                              TRUE → 18T TEMPLATE_NOT_ASSIGNED → End   FALSE → 19
    19  ForEach ForEachTemplate  over Candidate.templates_data → 20 MapTemplate
    21  ForEach ForEachField     over ...Current.data.fields → 22 MapField → 23 AppendField
    24  AppendTemplate → Result.Templates
    25  Aggregate GetMandatoryTableCount
    26  Aggregate GetMandatoryCount
    27  Assign  ConfInit      MandatoryTotal = LongIntegerToInteger(GetMandatoryCount.List.Current.Count)
                              MandatoryWithValue = 0; MinC = 1; MinCSet = False
                              ZeroMandatoryIds = ""
                              Result.LineItemsExtracted = If(GetMandatoryTableCount... > 0, False, ...)
    28  ForEach ForEachTmplConf over Result.Templates → 29 GetFieldMapRows
    30  ForEach ForEachFieldConf over ...Current.Fields → 31 FilterFieldMap → 32/33/34
    34  Assign  FieldConf     MandatoryTotal = MandatoryTotal + If(IsMandatory, 1, 0)   ← BUG
                              MandatoryWithValue += If(IsMandatory and V <> "", 1, 0)
                              MinC / MinCSet running minimum over mandatory-with-value
    35  Assign  AccumZeroMandatory   ZeroMandatoryIds accumulation
    36  Assign  FinalConf     Result.Completeness   = If(MandatoryTotal > 0, MandatoryWithValue / MandatoryTotal, 0)
                              Result.ClaimConfidence = If(MinCSet, MinC, 0)
    37  If      FieldMapMissing?   MandatoryTotal = 0 and Result.ErrorCode = ""
                              TRUE → 37T FIELDMAP_MISSING → End   FALSE → End
    --  ExceptionHandler AllExceptions → ParseFailed_Exc (PARSE_FAILED) → End

**`IdpRefused?` (node 15) and `FinalConf` (node 36) are on OPPOSITE branches of node 13 and
never share a path.** The real gate is `CandidateFound?`, not `IdpRefused?`.

### CORRECTION — I relayed a wrong mechanism as fact

I reported that `IdpRefused?` fires on `"0"` and therefore "the parser treats every
successful extraction as a refused request", hollowing out every result. **That was wrong.**
On a successful extraction `CandidateFound = True`, so node 15 is never reached; the loop
runs and the arithmetic executes. The PO's harness reading (`Comp 20/29`, `Conf 0.5337` with
`ErrorCode = "0"`) was right and was the evidence that broke the claim. I passed on Mentor's
account without reconciling it against that measurement. The measurement should have won
immediately.

### The coalesce bug is at node 17, NOT at the three wire attributes

Node 14 `IdpRefused` **already has** the correct guard:

    Result.ErrorCode = If(Envelope.result.result.errorCode <> 0, IntegerToText(Envelope.result.result.errorCode),
                       If(Envelope.result.errorCode        <> 0, IntegerToText(Envelope.result.errorCode),
                       If(Envelope.data.result.errorCode   <> 0, IntegerToText(Envelope.data.result.errorCode), "")))

Node 17 `TopLevel` has **no guard at all**:

    Result.ErrorCode = Candidate.errorCode

That single unguarded assign is what puts `"0"` into `Result.ErrorCode` on every successful
parse. Applying the agreed `If(... = 0, "", IntegerToText(...))` to `IdpWireOuter.errorCode`
/ `IdpWireInner.errorCode` / `IdpSubmitResponse2.errorCode` would have been the wrong place —
those feed `Candidate.errorCode`, and the fix belongs where `Candidate.errorCode` is written
into the Text output. **Reading first prevented fixing the wrong line.**

### Consequence: two of the three "arithmetic" symptoms may be one bug

Node 37 requires `MandatoryTotal = 0 AND Result.ErrorCode = ""`. Because node 17 sets
`ErrorCode = "0"`, the second condition is false and **FIELDMAP_MISSING is suppressed**. That
explains `templateId 9999 → ErrorCode "0", no FIELDMAP_MISSING` without any arithmetic defect.
Hypothesis, to be confirmed by measurement after publish 1, not assumed.

### The genuine arithmetic bug — double count of the denominator

`MandatoryTotal` is seeded at node 27 from the database and then incremented again per field
at node 34. Denominator becomes `db_count + fields_matched_as_mandatory`:
`8 + 21 = 29` and `8 + 1 = 9`, matching the measurements exactly. `MandatoryWithValue` is
only incremented in the loop, so the **numerator is correct and only the denominator is
inflated**. Expected fix: drop the `MandatoryTotal` increment at node 34, keep the node 27
seed (the seed is what the 6093 fixture's expected `MandatoryTotal 8` refers to).

Third symptom (`1 mandatory + 1 non-mandatory → Comp 0, Conf 0`): consistent with the same
defect plus an empty mandatory value — `MandatoryWithValue = 0`, so `0/9 = 0`, and `MinCSet`
stays False so `ClaimConfidence = 0`. Not necessarily a separate bug. Confirm by measurement.

### `AgentRun.Replay` — nothing sets it

Searched: **no assignment anywhere in the app has `Replay` on the left-hand side**, in any
server action. The attribute was created at rev 189 and is never written. Combined with
node 7c hard-coding `LiveHttpStatus = 200`, a replayed run is indistinguishable from a real
HTTP 200 in the audit trail. This is the last real ReplayMode gap (PO item (b) above),
now confirmed rather than suspected.

## PUBLISH 1 — rev 193. errorCode coalesce at node 17, ALONE. Stopped for measurement.

Run 6e75ba1e, `changeApplied: true`, validation **7 errors / 135 warnings — baseline**.
Published rev 193, outcome success.

Exactly one expression changed. Node 17 `TopLevel` in `IdpParseExtraction`:

    Result.ErrorCode = If(Candidate.errorCode <> 0, IntegerToText(Candidate.errorCode), "")

Verified unchanged by pasting back for comparison:
- node 14 `IdpRefused` — the three-level coalesce, byte-for-byte identical, and its
  `Result.PageCount` companion expression also unchanged
- node 15 `IdpRefused?` — condition still `Result.ErrorCode <> ""`
- `IdpWireOuter.errorCode`, `IdpWireInner.errorCode`, `IdpSubmitResponse2.errorCode` — all
  still **Integer**; no attribute data type touched
- nodes 27 `ConfInit`, 34 `FieldConf`, 36 `FinalConf`, 37 `FieldMapMissing?` — not modified

Elements changed in the run: one — `IdpParseExtraction` / `TopLevel`, the `Result.ErrorCode`
right-hand side only.

### PO correction accepted on where the guard belongs

The instruction had been "apply at all three wire attributes". That was wrong and the PO said
so: **a guard is an expression, it belongs to the assignment, not to the attribute.** Reading
node 17 before building is what caught it. Had the three attributes been changed instead, the
1014 fixture might still have passed while `"0"` kept flowing through node 17 — a fix that
looks green and changes nothing.

### To measure on rev 193, before anything else is built

- 6093 fixture (`errorCode: null`) → expect `ErrorCode = ""`, not `"0"`
- missing `errorCode` key → expect `""`
- 1014 fixture → expect `ErrorCode = "1014"` still, and `PageCount = 1` — the 1014 path must
  not have regressed
- **templateId 9999 → expect `FIELDMAP_MISSING` to come back.** This is the test of my
  hypothesis that node 37's second condition (`Result.ErrorCode = ""`) was being blocked by
  `"0"`. If it does come back, one of the three "arithmetic" symptoms was never arithmetic.

## Hole in my arithmetic diagnosis — PO caught it, not fixed, not designed

I claimed "the numerator is correct, only the denominator is inflated". The measurements
disagree:

    1 mandatory field                  → 1/9   numerator 1 — correct
    1 mandatory + 1 NON-mandatory      → 0     numerator 0 — WRONG, should be 1/10
    ClaimConfidence in that same case  → 0.5 drops to 0

Three variables (`MandatoryWithValue`, `MinC`, `MinCSet`) all degrade in the same case, which
points at ONE cause, and it is not the `MandatoryTotal` line I quoted. Adding a
non-mandatory field cannot legitimately zero the mandatory numerator.

**Next read before any arithmetic design** (PO instruction): the COMPLETE left-hand side of
node 34 `FieldConf` — every variable assigned in that loop, not just `MandatoryTotal` — plus
nodes 31 `FilterFieldMap`, 32 `AssignFilterResult` and 33 `AssignIsMandatory`, because
`IsMandatory` is computed there and is the common input to all three degraded variables.

One suspicion to test rather than assume: node 31's filter condition was read as
`IdpFieldMap.K = CurField.K`. `CurField` is the mapping variable of the FIRST loop
(node 22, inside `ForEachField`). The confidence loop at node 30 iterates
`Result.Templates.Current.Fields`. If node 31 really references `CurField.K`, it is reading a
stale variable holding the LAST field of the first loop, which with two fields would make
`IsMandatory` False for both iterations and zero all three variables at once. That is a
hypothesis from a partial read — it must be confirmed against the full node 31–34 dump before
a single expression is changed.

## AgentRun.Replay promoted to REQUIRED, not cleanup

Confirmed: never written anywhere. Combined with `LiveHttpStatus = 200` hard-coded at node 7c,
the audit trail cannot distinguish a replay from a real call. ReplayMode already skips
`GetToken`, so authentication is not rehearsed either. **Two blind spots overlap**: the run
does not exercise auth, and the record does not say it was a replay. This is now a
must-finish item of ReplayMode, not a later tidy-up.

## Rev 193 measured — publish 1 green on all four (PO)

    errorCode: null           → ErrorCode = ""      PASS
    errorCode key absent      → ErrorCode = ""      PASS
    1014 fixture              → ErrorCode = "1014"  PASS (1014 path not regressed)
    templateId 9999           → FIELDMAP_MISSING back  PASS

Confirms the hypothesis: node 37's second condition (`Result.ErrorCode = ""`) was blocked by
`"0"`. **One of the three "arithmetic" symptoms was never arithmetic.** templateId 6090 still
returns FIELDMAP_MISSING — expected, not yet fixed, not in scope here.

## Root cause of the arithmetic, established by behaviour (PO) — order dependence

`IsMandatory` was computed from the **last** field of the list and then applied to every
iteration of the confidence loop. Six measurements, one model:

    1 mandatory                                  → 1/9
    [mandatory, non-mandatory]                   → 0
    [non-mandatory, mandatory]                   → 2/10     same two fields, order swapped
    [mandatory, mandatory]                       → 2/10
    21 fields (last = mandatory)                 → 20/29
    21 fields, one NON-mandatory moved to last   → 0

The last line was **predicted before being measured**. Same data, order only, 0.6896 → 0.

The denominator was governed by the same variable — `8 + field_count` only when the last
field happened to be mandatory — on top of accumulating over the seed. One cause, two
symptoms.

Mechanism: node 31 `FilterFieldMap` filtered on `CurField.K`. `CurField` is the mapping
variable of the FIRST loop (node 22, inside `ForEachField` at node 21) and is stale by the
time the confidence loop (node 30 `ForEachFieldConf`) runs — it holds the last field the
first loop processed. So every iteration of the confidence loop matched the same fieldmap
row, and `IsMandatory` took one value for the whole loop.

## PUBLISH 2 — rev 194. Both fixes together, as PO required. Awaiting measurement.

Run 9a64ae72, `changeApplied: true`, validation **7 errors / 135 warnings — baseline**.
Published rev 194, outcome success.

Not split, on the PO's reasoning: (a) alone leaves the denominator at `8 + 8 = 16` → 0.5,
still wrong; (b) alone leaves the numerator order-dependent.

**(a) node 31 `FilterFieldMap`, `Condition` argument now:**

    IdpFieldMap.K = Result.Templates.Current.Fields.Current.K

(`SourceList` = `GetFieldMapRows.List` untouched.)

**(b) node 34 `FieldConf` — the `MandatoryTotal` line is deleted.** Remaining assignments,
verbatim, in order, all unchanged:

    MandatoryWithValue = MandatoryWithValue + If(IsMandatory and Result.Templates.Current.Fields.Current.V <> "", 1, 0)
    MinC               = If(IsMandatory and Result.Templates.Current.Fields.Current.V <> "", If(not MinCSet or Result.Templates.Current.Fields.Current.C < MinC, Result.Templates.Current.Fields.Current.C, MinC), MinC)
    MinCSet            = MinCSet or (IsMandatory and Result.Templates.Current.Fields.Current.V <> "")

`MandatoryTotal` is now written in exactly one place, node 27 `ConfInit`, confirmed unchanged:

    MandatoryTotal = LongIntegerToInteger(GetMandatoryCount.List.Current.Count)

Verified unmodified: nodes 32, 33, 36, 37 and 17. `CurField` local variable kept — the first
loop still uses it. Elements changed: exactly the two in the fence.

### Acceptance criteria for rev 194 (PO) — the last four are the real test

    21 fields                    → ClaimConfidence 0.5544 · Completeness 1.00
    1 mandatory field            → Completeness 0.125
    [mandatory, non-mandatory]   → Completeness 0.125   adding a non-mandatory field must NOT change it
    [non-mandatory, mandatory]   → Completeness 0.125   order must NOT change it
    21 fields, order reversed    → still 0.5544 · 1.00

The result must be invariant under field order. That invariance, not any single number, is
what proves the stale-reference cause is gone.

## Rev 194 measured — ALL SEVEN acceptance cases green, plus four regression cases (PO)

    21 fields, original order                → ClaimConfidence 0.5544 · Completeness 1.00
    21 fields, reversed                      → 0.5544 · 1.00
    21 fields, non-mandatory moved to last   → 0.5544 · 1.00
    1 mandatory                              → Completeness 0.125
    [mandatory, non-mandatory]               → 0.125
    [non-mandatory, mandatory]               → 0.125
    [mandatory, mandatory]                   → 0.25 · ClaimConfidence 0.5

Regression set, all still correct:

    1014 fixture      → ErrorCode "1014"
    errorCode absent  → ErrorCode ""
    templateId 9999   → FIELDMAP_MISSING
    empty string      → PARSE_FAILED

**Denominator is a constant 8 in every case. Order invariance achieved.** Both publishes hold
independently — neither broke the other, which is what publishing them separately bought.

### Closed: "empty string → PARSE_FAILED"

That was the FIRST defect found in this whole line of work — discovered on 2026-09-28 by
pressing the harness button with an empty box. It is now confirmed still correct after four
subsequent publishes. Closed.

### Status of IdpParseExtraction after revs 193 + 194

    node 17 TopLevel        Result.ErrorCode = If(Candidate.errorCode <> 0, IntegerToText(Candidate.errorCode), "")
    node 31 FilterFieldMap  IdpFieldMap.K = Result.Templates.Current.Fields.Current.K
    node 34 FieldConf       MandatoryTotal line removed; three accumulators intact
    node 27 ConfInit        sole writer of MandatoryTotal

## REMAINING WORK, in PO's order

### 1. `AgentRun.Replay` is never written — must-finish for ReplayMode

Confirmed by search: no assignment anywhere has `Replay` on the left-hand side. With
`LiveHttpStatus = 200` hard-coded at node 7c, and `GetToken` skipped on the replay branch,
the run neither rehearses authentication nor records that it was a replay. Two blind spots
overlapping.

**FENCE QUESTION — needs a decision before building.** An earlier constraint, set
deliberately, was that neither wrapper may write `AgentRun`: "if neither wrapper touches
AgentRun, do NOT invent an AgentRun write here." `WriteAgentRun` is called from the
orchestration layer (`TransitionClaimStatus`, `Portal_SubmitClaim`, `Portal_SendSupplement`).
So marking a run as replayed has two possible shapes:

- (A) the wrapper writes `AgentRun` directly — **violates the standing constraint**;
- (B) the wrapper reports it upward: a new Boolean on the `IdpCallOutcome` Structure (e.g.
  `WasReplay`), set True on the replay branch and False on the live branch, which the
  orchestration layer then passes to `WriteAgentRun`. Keeps the wrapper free of `AgentRun`
  writes and honours the constraint — but it changes a Structure and touches the
  orchestration layer, both outside every fence used so far.

Recommend (B). Not started — the fence extension is the PO's call.

### 2. FIELDMAP_MISSING still fires for 6090 (14 rows, 0 mandatory)

PO instruction: the condition must change from "no mandatory rows" to "no rows at all".
Current node 37 condition is `MandatoryTotal = 0 and Result.ErrorCode = ""`.

**Two design points to settle before building, both flagged rather than assumed:**

(i) "No rows at all" is not available at node 37. `GetFieldMapRows` is node 29, **inside** the
per-template loop (node 28 `ForEachTmplConf`); node 37 runs after the loop has exited. Testing
"no rows for any template" therefore needs an accumulator across the loop, the way
`ZeroMandatoryIds` already accumulates. That means a new local variable, initialised at node 27
and accumulated inside the loop — three touch points, not a one-expression change.

(ii) **Undecided business question, adjacent and not in the instruction:** with 6090 having 14
rows but 0 mandatory, `MandatoryTotal = 0`, so node 36 yields `Completeness = 0` via the else
branch. Is 0 the intended reading for a template with no mandatory fields, or should it be
1.00 (nothing required is missing), or "not applicable"? Changing the error code alone leaves
`Completeness 0` in place for 6090. Needs a decision; not guessed.

### 3. Batch E not published — seed button still says "163 rows across 8 templates"

The CSV is the 192-row / 9-template version (md5 `a60cb97c4b2ed64bf34824ce4004b0ed`), and
`ExpectedRows` was updated, but the button's label text still reads the old figures. Cosmetic
in the UI, misleading in a demo.

### 4. Business decisions still open on the seed data

- the 29 6092 rows: `FieldCode` and `IsMandatory` were inherited mechanically — PO noted
  "đừng coi là đã chốt", treat as provisional
- the 8 `REVIEW_*` placeholder codes for 6093

---

## Read-only pass for the two named changes — 30/09, ClaimMVP baseline rev 194

Mentor session `9fab8e83-4e1b-4fdd-8bc6-deb3401bc9c0`, run
`e849f556-78a0-4ce6-99b5-8581be21b138`. Terminal: `status succeeded`,
`attemptedChange: true`, **`changeApplied: false`** — read-only respected.
Validation 7 errors / 135 warnings, identical to the rev-194 baseline.
**Everything below is machine-read from the published model, not inference.**

### A · `AgentRun.Replay` — for change `agentrun-replay-flag`

`AgentRun.Replay` **exists**: Boolean, mandatory, **no default set** (so a Boolean
default of False). Its description already states the intent:

> "True when this run executed under Settings.ReplayMode against a stored IdpReplay
> body instead of a live IDP call. A replayed run that looks identical to a live one
> is a lie in the audit trail."

Search for `Replay` on the left-hand side of any assignment: **none found.** The flag
has never been written since the attribute was created.

`WriteAgentRun` callers — **four**, not the three recorded earlier in this file:
`TransitionClaimStatus`, `Portal_SendSupplement`, `Portal_SubmitClaim`, **`Seed_AgentRuns`**.

`WriteAgentRun` node list:

```
1 Start
2 Assign (unnamed)  <- ALL run metadata, 14 assignments, Replay ABSENT
   AgentRun.ClaimId | AgentName | Step | ToolName | InputJson | OutputJson
   HandoffTo | KnowledgeVersion | ModelId | CorrelationId | DurationMs | IsError
   AgentRun.StartedOn  = If(StartedOn  = NullDate(), CurrDateTime(), StartedOn)
   AgentRun.FinishedOn = If(FinishedOn = NullDate(), CurrDateTime(), FinishedOn)
3 ExecuteAction CreateAgentRun(Source: AgentRun)
4 Assign AgentRunId = CreateAgentRun.Id
5 End
```

No `Replay` input parameter exists on `WriteAgentRun`, and none is needed:
`Settings` is a global singleton readable by name from any server action — exactly how
`IdpSubmitSync` reads it in its `SyncReplayMode?` condition. Node 2 is therefore the
single correct insertion point, and no call site changes.

**Delta owed (standing rule 7) — Mentor's own positive control for "nothing assigns
Replay" was weaker than it looked.** It cited `ClaimNo` assignments found in
`Seed_FixtureClaim`, `Seed_Claims`, `Portal_SubmitClaim`, `Seed_ChainTests_A/B/C`,
`GetIncidents`. But **`AgentRun` has no `ClaimNo` attribute** — it is `ClaimId`. Those
hits are all `Claim.ClaimNo`, so they prove the query works on *some* entity, not on
this one. The control that actually holds is the node-2 dump above: fourteen
`AgentRun.*` attributes assigned in one place, `Replay` not among them.

### C · `Seed_IdpFieldMap` — for change `fieldmap-batch-e`

```
1  Start
2  ExecuteAction CallA -> Seed_IdpFieldMap_A   (no arguments)
3  ExecuteAction CallB -> Seed_IdpFieldMap_B
4  ExecuteAction CallC -> Seed_IdpFieldMap_C
5  ExecuteAction CallD -> Seed_IdpFieldMap_D
6  ExecuteAction CallE -> Seed_IdpFieldMap_E
7  Aggregate CountAllRows        no filter
8  Aggregate CountFieldRows      Origin = "field"
9  Aggregate CountKcbRows        Origin = "table:CHI PHÍ KHÁM CHỮA BỆNH"
10 Aggregate CountBangKeRows     Origin = "table:Bảng kê chi phí khám chữa bệnh"
11 Aggregate CountThuocRows      Origin = "table:Thuốc điều trị"
12 Assign AssignOutputs
     ExpectedRows          = 192
     ActualRows            = LongIntegerToInteger(CountAllRows.List.Current.Count)
     OriginFieldRows       = LongIntegerToInteger(CountFieldRows.List.Current.Count)
     OriginTableKcbRows    = LongIntegerToInteger(CountKcbRows.List.Current.Count)
     OriginTableBangKeRows = LongIntegerToInteger(CountBangKeRows.List.Current.Count)
     OriginTableThuocRows  = LongIntegerToInteger(CountThuocRows.List.Current.Count)
     OriginOtherRows       = ActualRows - OriginFieldRows - OriginTableKcbRows
                             - OriginTableBangKeRows - OriginTableThuocRows
13 End
```

Inputs: **none**. Outputs: the seven Integers above.

**How rows are written.** Every child action A–E calls `Seed_IdpFieldMap_Upsert` once
per row. The upsert is keyed on `(IdpTemplateId, K)`: if a row with that key exists it
updates all columns, else it creates. **It never deletes.** This is verbatim the orphan
risk the brief predicted — nine 6091 rows keyed on the old `K` values would survive a
re-seed under new keys and the entity would hold 201 rows, nine of them stale.

**NFC guard** — lives in `Seed_IdpFieldMap_Upsert` as the very first If after Start,
labelled `HasCombiningMark?`. Condition verbatim:

```
Index(K, Chr(768), 0) <> -1 or Index(K, Chr(769), 0) <> -1 or Index(K, Chr(771), 0) <> -1
or Index(K, Chr(777), 0) <> -1 or Index(K, Chr(803), 0) <> -1
```

True -> `LogNfcReject` (logs `SEED_IDPFIELDMAP_NFC_REJECT template=… K='…'`) -> End.
**The row is then silently skipped: the caller gets no count and no flag.** The brief
requires reject *and report*, so a `RejectedRows` output is missing, not merely unwired.

### D · `IdpFieldMap` attribute lengths — the truncation check

| Attribute | Type | Declared length | Longest value in the CSV |
|---|---|---|---|
| `K` | Text | **512** | 39 |
| `FieldCode` | Text | **50** | 22 |
| `DataType` | Text | 20 | 5 (`MONEY`) |
| `Origin` | Text | 100 | 36 |
| `TargetEntity` | Text | 30 | 13 |
| `IdpTemplateId` | Long Integer | — | — |
| `Version` | Integer | — | 1 |
| `IsActive`, `IsMandatory` | Boolean, mandatory | — | — |
| `Id` | Long Integer, PK | — | — |

**No truncation risk anywhere.** `K` at Text(512) has 13× the headroom it needs, and
the old `FieldCode` Text(20) that silently truncated a 22-character code has already
been widened to Text(50). This is the one thing the brief asked me to say loudly if it
were wrong; it is not wrong.

`IdpFieldMap` has **no `TemplateLabel` and no `Note` attribute**, and no free-text
column of any kind — so the CSV's `TemplateLabel` and `Note` columns are documentation
only and are not stored. Nothing to build for them.

### E · The seed button on Administration

- Button widget: `IdpFmSeedBtn`, calls screen action `RunIdpFieldMapSeed`.
- Button's own label widget `IdpFmSeedBtnText` reads `"Run IdpFieldMap seed"` —
  contains neither number, so it does not need touching.
- The stale figures live in a **separate Text widget `IdpFmSeedNote`**, immediately
  above the button inside container `IdpFieldMapSeedCard`. Exact current string:

  > `163 rows across 8 templates. Safe to run more than once — it upserts on (template, K) and never deletes.`

`192` appears in exactly one place in logic: `ExpectedRows` in `AssignOutputs`.
`8` appears as a literal nowhere in `Seed_IdpFieldMap`; the `8` in "8 templates" is
only in `IdpFmSeedNote`.

### Deltas found that the brief did not ask about

1. **`ExpectedRows` is already `192`.** A previous session bumped the expectation
   without publishing the data rows. So the action already measures itself against 192
   while the child actions may still write fewer — the button's own summary
   (`"IdpFieldMap seeded: " + ActualRows + " of " + ExpectedRows + " rows."`) is the
   honest readout and would already be showing the shortfall.
2. **The aggregate-based summary the brief asked me to add already exists** — seven
   outputs, all read back from the entity after the writes, none of them loop counters.
   What is genuinely absent is the per-template **mandatory** counts (6091=14, 6092=13,
   6093=8) that the brief's acceptance numbers include; no aggregate produces those.
3. **`IdpFmSeedNote` will be wrong in substance, not just in its numbers.** Once the
   seed becomes delete-then-insert, the clause "it upserts on (template, K) and never
   deletes" becomes false. Changing only 163→192 and 8→9 would leave a true sentence
   next to a false one. Same widget, inside the fence — fixing it with the numbers.
4. **Name ambiguity not yet resolved:** section E says the button calls
   `RunIdpFieldMapSeed`; section C says `RunSeedIdpFieldMapNow` builds the `Summary`
   string. Either two actions or one renamed. Not guessed — to be confirmed in the
   write run before either is touched.
5. **`db_query` cannot read entity rows.** Its own contract: "NO template (SELECT
   included) returns rows in v1, every template comes back with rowcount 0." So there
   is no tool path by which I can read what `IdpFieldMap` currently holds. The row
   count is observable only by pressing the button — which is why the delete count has
   to be *reported by the action*, not predicted by me.
6. **Two verification steps in the brief are outside my tool reach:** toggling
   `Settings.ReplayMode` in the ODC Portal, and pressing buttons in the running app. I
   can build and publish; every behaviour measurement in this change record so far has
   been made by the PO, and these two will be as well.

### F · The brief's premise for `fieldmap-batch-e` was wrong — batch E is already live

Two further read-only runs in the same session, `4da68237-2dc7-4051-b150-bde9812ce691`
and `6f957edb-4828-433d-8c84-3d0369c07bba`, both `attemptedChange: false`.

`Seed_IdpFieldMap_Upsert` inputs: `IdpTemplateId` (Long Integer), `K`, `FieldCode`,
`TargetEntity`, `DataType` (Text), `IsMandatory` (Boolean), `Origin` (Text) — all
mandatory. `Version` and `IsActive` are **not** inputs; the upsert sets them internally.

Upsert calls per child action, machine-counted:

| Action | Calls | Template ids |
|---|---|---|
| `_A` | 53 | 6090, 6091 |
| `_B` | 44 | 6093, 6094 |
| `_C` | 35 | 6095, 6096 |
| `_D` | 31 | 6097, 6098 |
| `_E` | **29** | **6092** |
| | **192** | **nine templates** |

**`Seed_IdpFieldMap_E` is populated — 29 calls, template 6092.** Batch E is already in
the model. The entry in this file saying "Batch E not published" was wrong, and so was my
own prediction that the entity holds 163 rows across 8 templates.

**The nine corrected 6091 `K` values are also already in the model.** All 13 upsert calls
with `Origin = "table:CHI PHÍ KHÁM CHỮA BỆNH"` were quoted verbatim and match the CSV's
13 rows exactly, in the same order:

```
Nội dung · Đơn vị tính · Số lượng · Đơn giá BV · Đơn giá BH ·
Tỷ lệ thanh toán theo dịch vụ · Thành tiền BV · Tỷ lệ thanh toán BHYT ·
Thành tiền BH · Quỹ BHYT · Người bệnh cùng chi trả · Khác · Người bệnh tự trả
```

None carries a `(đồng)` suffix. The suffix survives only in `_E` (6092, origin
`table:Bảng kê chi phí khám chữa bệnh`, 8 rows) — which is exactly where the CSV keeps it.
Checked from the CSV side too: `(đồng)` appears in 8 rows, all template 6092.

Note also that my prompt asserted "I expect nine" of these; the model has **13**, and
Mentor pushed back rather than agreeing. Thirteen is right — the CSV has 13 rows with that
origin, of which nine were the ones whose `K` was corrected. My number was the wrong one.

**So nothing in the row data needs re-seeding for correctness of the keys.** What the
delete-then-insert is actually for is different and still real: the upsert has never
deleted, so if the button was pressed at any time while the old `(đồng)` keys were in
place, the entity still holds those 9 stale rows under their old keys — 201 rows, nine
of them orphans. That is invisible in the model and unreadable by any tool I have; only
`ActualRows` on a press reveals it.

### G · NEW DEFECT — `Origin = "field"` is never written. All 164 field rows carry `""`.

Distinct `Origin` literal per child action, counted from the model, sums verified:

| Action | `""` | `table:CHI PHÍ KHÁM CHỮA BỆNH` | `table:Bảng kê chi phí khám chữa bệnh` | `table:Thuốc điều trị` | Sum |
|---|---|---|---|---|---|
| `_A` | 38 | 13 | — | 2 | 53 |
| `_B` | 44 | — | — | — | 44 |
| `_C` | 35 | — | — | — | 35 |
| `_D` | 31 | — | — | — | 31 |
| `_E` | 16 | — | 13 | — | 29 |
| **total** | **164** | **13** | **13** | **2** | **192** |

**`Origin = "field"` is passed zero times in the entire seed.** No call omits the
argument — every one passes it explicitly, and 164 of them pass the empty string.

The CSV, checked independently on this machine: `field` 164 · `table:CHI PHÍ KHÁM CHỮA
BỆNH` 13 · `table:Bảng kê chi phí khám chữa bệnh` 13 · `table:Thuốc điều trị` 2 · empty 0.
**The 164 empties correspond one-to-one with the 164 rows the CSV marks `field`** — so
`""` was written where `"field"` was meant. These are not rows of genuinely unknown origin.

Consequence, and this is why it matters more than it looks: `Seed_IdpFieldMap`'s
aggregate `CountFieldRows` filters `Origin = "field"`, so it returns 0. The summary
would read

```
field 0 · KCB 13 · BangKe 13 · Thuoc 2 · other 164 · total 192
```

against the expected `field 164 … other 0`. Total is right, the split is inverted.
**This is standing rule 8's exact failure shape** — a wrong value wearing the costume of
a plausible other state. "OriginOtherRows 164" reads like an unclassified-rows question,
not like a bug, and nothing errors.

**Two ways to fix it, and the choice is the PO's:**

- **(i) One expression, one place.** In `Seed_IdpFieldMap_Upsert`, coerce on write:
  `Origin = If(Origin = "", "field", Origin)`. Safe because the CSV has zero legitimately
  empty origins and this seed is the only writer. Cost: the 164 call sites still *say*
  `""`, so anyone reading a call site is misled, and the truth lives one level down.
- **(ii) 164 argument edits**, passing `"field"` at each call site across all five
  actions. Model then reads true everywhere. Cost: it is 164 edits in one Mentor run on
  an app whose session has already died twice mid-run; a partial application would leave
  the seed in a mixed state that the summary could not distinguish from (i).

Recommend **(i) now, (ii) later as a tidy-up**, because (i) makes the numbers correct
immediately and is one reviewable expression, and because a half-applied (ii) is worse
than either. Not done — awaiting the decision.

### Revised scope for `fieldmap-batch-e`

What the brief asked for, against what the read shows is actually needed:

| Brief item | Status after the read |
|---|---|
| re-seed 192 rows / 9 templates | **already in the model** — nothing to author |
| nine corrected 6091 `K` values | **already in the model** |
| delete-then-insert, not upsert | **still needed**, but for the database's stale rows, not the action's keys |
| report rows deleted | still needed |
| stop if the entity holds a template id not in the file | still needed |
| keep the NFC guard, reject + report | **needs a `RejectedRows` output** — today it logs and silently skips |
| button label 163/8 → 192/9 | still needed, in Text widget `IdpFmSeedNote`; its "never deletes" clause also goes false |
| summary from aggregates read back from the entity | **already exists** — seven outputs, none a loop counter |
| the expected per-template mandatory counts (6091=14, 6092=13, 6093=8) | **no aggregate produces these** — three to add |
| — | **NEW: `Origin = "field"` written 0 times instead of 164** (section G) |

CSV mandatory counts confirmed on this machine: 6091=14, 6092=13, 6093=8, total 35,
no other template has a mandatory row.

### Publish 3 — rev 195 — change `agentrun-replay-flag` DONE

Run `572a163c-2b85-4bad-b3bf-db9fd4ddd6c4`, session
`9fab8e83-4e1b-4fdd-8bc6-deb3401bc9c0`. Terminal result:
`attemptedChange: true`, **`changeApplied: true`**, validation **7 errors / 135
warnings — equal to the rev-194 baseline**, not "about the same": the numbers match.

Published: revision **195**, publication key `b03bd384-f52a-429c-9210-9b4f1cdc33f0`,
environment `09d830a7-8a06-4652-9840-41b49ba67cc9`.

`WriteAgentRun` node 2 now holds **fifteen** assignments. The fourteen pre-existing ones
are byte-identical and in the same order as the read before the edit; the new one is
appended last:

```
15. AgentRun.Replay  ←  Settings.ReplayMode
```

`WriteAgentRun`'s input parameter list is unchanged — no `Replay` input was added, and
none of the four call sites (`TransitionClaimStatus`, `Portal_SendSupplement`,
`Portal_SubmitClaim`, `Seed_AgentRuns`) was touched. `Settings.ReplayMode` is read
directly inside the action, the same way `IdpSubmitSync` reads it in `SyncReplayMode?`.

`LiveHttpStatus` was NOT touched, as instructed — the hard-coded 200 on the replay branch
remains open.

**Status: verified by machine (model read back after the edit), NOT yet verified by
behaviour.** The behaviour check needs `Settings.ReplayMode = True` in the ODC Portal and
a run against claim `TEST-DIAG`, then reading the `AgentRun` row's `Replay`. Expected:
`True`. Neither the Portal toggle nor the button press is reachable with my tools.

**Deviation from the brief, stated rather than buried:** the brief asked for one publish
round covering both changes. This publish carries only `agentrun-replay-flag`.
`fieldmap-batch-e` is held because the read overturned its premise (sections F and G) and
one decision is now the PO's. Publishing the finished half immediately also follows the
rule learned when session `babbb708` died with two runs chained onto one unpublished
draft: never hold a completed behaviour change in a draft to save a publish cycle.

---

## Item zero — the seed button pressed once on rev 195, 30/09

**Verified by behaviour.** Runtime `https://bnk-dev.outsystems.app/ClaimMVP/Administration`,
rev 195 deployed (`env_app` confirms revision 195, deployed 2026-09-30T09:25:47Z), logged in
as Nguyễn Hoài An (Approver). Button `Run IdpFieldMap seed` pressed **once**. The IDP
diagnostics buttons were not touched; no tenant quota was spent.

Rendered result, verbatim:

```
IdpFieldMap seeded: 201 of 192 rows. Origin: field=0 kcb=0 bangke=0 thuoc=0 other=0.
ROW COUNT MISMATCH. Check the log for SEED_IDPFIELDMAP_NFC_REJECT.
```

### ActualRows = 201 → the orphan branch

201 − 192 = 9. The seed has never deleted, so the nine rows keyed on the old `(đồng)`
spellings survived when the keys were corrected. **Per the PO's own rule this stops
delete-then-insert from being built**; the fix is a one-time targeted delete of nine rows.

The nine `K` values believed orphaned, all template **6091**, all origin
`table:CHI PHÍ KHÁM CHỮA BỆNH` — old key on the left, current key on the right:

| # | orphaned `K` (still in the entity) | current `K` (written by the seed) |
|---|---|---|
| 1 | `Đơn giá BV (đồng)` | `Đơn giá BV` |
| 2 | `Đơn giá BH (đồng)` | `Đơn giá BH` |
| 3 | `Tỷ lệ thanh toán theo DV` | `Tỷ lệ thanh toán theo dịch vụ` |
| 4 | `Thành tiền BV (đồng)` | `Thành tiền BV` |
| 5 | `Thành tiền BH (đồng)` | `Thành tiền BH` |
| 6 | `Quỹ BHYT (đồng)` | `Quỹ BHYT` |
| 7 | `Người bệnh cùng chi trả (đồng)` | `Người bệnh cùng chi trả` |
| 8 | `Khác (đồng)` | `Khác` |
| 9 | `Người bệnh tự trả (đồng)` | `Người bệnh tự trả` |

**How strong this list is, stated honestly.** The count matches exactly (9), the mechanism
is proven (upsert keyed on `(IdpTemplateId, K)`, never deletes — read from the model), and
these nine are the only documented key change in the project. But 201 − 192 = 9 is
consistent with the hypothesis, it does not prove these particular nine are the survivors.
Nothing available to me can read entity rows — `db_query` is template-only and returns
rowcount 0 for every template including SELECT. Confirming the list needs either a
temporary read-only display of 6091 rows, or the delete itself reporting which keys it
removed. Not built; awaiting the decision.

### Three of the four origin numbers are impossible — a second, deeper defect

`kcb=0` cannot be true if `Origin` reaches the database: `Seed_IdpFieldMap_A` passes
`Origin = "table:CHI PHÍ KHÁM CHỮA BỆNH"` on 13 calls and `_E` passes the BangKe literal
on 13, both machine-read. So either the upsert never writes `Origin` onto the record, or
the aggregate filter literals do not match what is stored.

`other=0` is impossible on its own terms. The published expression is
`OriginOtherRows = ActualRows − field − kcb − bangke − thuoc`; with `ActualRows = 201` and
the other four at 0 it must be **201**. It rendered 0. And because it rendered 0, the
`ORIGIN MISMATCH` clause in the Summary — which fires on `OriginOtherRows <> 0` and exists
precisely to catch rows with an unexpected or empty Origin — **never fired.** The guard
written to catch this bug is silenced by the same bug.

Rule 8 again, twice over: `other=0` reads as "no unclassified rows", which is good news,
while the truth is 164 rows with an empty Origin. And the one warning clause that would
have said so stayed quiet.

**This blocks `fieldmap-origin-default`.** Its whole acceptance test is
`Origin "field" 164` appearing in this Summary. If `Origin` never reaches the database, or
if this arithmetic is broken, the coalesce could work perfectly and the number would still
read 0 — and `OriginDefaulted` would claim 164 rows repaired next to a column that is still
empty. Read fired before building anything.

### Where the seven outputs actually go — answering the binding question

All seven **do** reach the screen, contrary to what the blank card suggested. The chain,
machine-read:

```
button IdpFmSeedBtn
  -> screen action RunIdpFieldMapSeed
       node 2  ExecuteAction CallRunSeedIdpFieldMapNow -> server action RunSeedIdpFieldMapNow
       node 3  Assign  IdpFieldMapSeedSummary <- CallRunSeedIdpFieldMapNow.Summary
  -> screen variable IdpFieldMapSeedSummary
  -> expression widget IdpFmSeedResultExpr, inside container IdpFmSeedResultCont,
     visible only when IdpFieldMapSeedSummary <> ""
```

`RunSeedIdpFieldMapNow` calls `Seed_IdpFieldMap` (node `CallSeedIdpFieldMap`) and
concatenates all seven outputs into its single `Summary` text output. There is **no
Feedback/Toast node** anywhere in the chain. So the earlier note in this file that the card
"shows nothing under the button" was a statement about the *idle* state only — the
container is hidden until a run produces a summary. Nothing needs binding; item (c) of
`fieldmap-seed-outputs` is already satisfied.

**Name ambiguity resolved:** `RunIdpFieldMapSeed` (screen action, what the button calls)
and `RunSeedIdpFieldMapNow` (server action, what builds `Summary`) are two different
things, not one renamed.

### The mismatch message points at the wrong cause

`ROW COUNT MISMATCH. Check the log for SEED_IDPFIELDMAP_NFC_REJECT.` fires on
`ActualRows <> ExpectedRows` in either direction. But an NFC rejection *skips* a row, so it
can only make Actual **lower** than Expected. Here Actual is nine **higher**, which NFC
rejects cannot cause. An operator following that instruction searches a log that has
nothing in it and concludes the app is fine.

### `agentrun-harness-readout` — two of the requested columns do not exist

Machine-read, with the explicit yes/no the positive control asked for:

- attribute named `LiveHttpStatus` on `AgentRun`? **No.** `LiveHttpStatus` is a local
  variable of the `IdpSubmit*` wrappers; it is never persisted on `AgentRun`.
- attribute named `CreatedAt`? **No**, and no `CreatedOn` either. The closest is
  **`StartedOn`** — mandatory, set to `CurrDateTime()` when not supplied, the earliest time
  recorded for the row.
- run-identifying attributes actually available: `AgentName`, `Step`, `ToolName`.

So the readout will show `Id · ClaimId · AgentName · Step · ToolName · Replay · StartedOn`.
`LiveHttpStatus` is omitted rather than invented, and this is flagged rather than silently
substituted.

### Read-only question — can a seeded AgentRun row be told apart from a real one?

**No. Nothing on the row distinguishes it.** Machine-read from `Seed_AgentRuns`, which makes
**48** `WriteAgentRun` calls:

- `CorrelationId` — argument omitted on all 48. Not a discriminator: it is empty on many
  real rows too.
- `ModelId` — argument omitted on all 48. Same problem.
- `KnowledgeVersion` — `"kb-2026-08-01"` on four calls (`WrA10`, `WrB10`, `WrC10`, `WrD10`,
  the "Clinical check" calls), omitted on the rest. **This is exactly what a real
  clinical-check run would write**, so it identifies the step, not the provenance.
- `AgentName` — the same strings a real run uses.
- There is no `IsSeed`, `Source`, `Origin`, `IsDemo`, `Fixture` or any equivalent attribute.

After rev 195 these 48 seeded rows will also carry `Replay = Settings.ReplayMode`, i.e.
`False`, asserting they were live IDP runs. The PO's reading is correct: it is the same
false claim rev 195 removed, in different clothes. **Consequence for a slide: any accuracy
or volume figure taken from `AgentRun` silently includes 48 synthetic rows and there is no
field to filter them out by.** Reported, not changed — a marker attribute is an Entity
change and outside the fence.

---

## Round of 30/09 continued — item zero measured, one change published, two held

### `agentrun-harness-readout` — DONE, rev 196

Run `31ac5bda-5c49-437f-897b-a002130883df`. Terminal: `attemptedChange: true`,
**`changeApplied: true`**, validation **7 errors / 135 warnings — equal to the rev-195
baseline**. Published revision **196**, publication `ef02d23d-75b1-4848-8e2d-3cbb2afac5ed`.

New card `AgentRun (last 5)` on the Administration screen, after the `Replay fixture
store` block. Aggregate `GetLastAgentRuns`: Entity `AgentRun`, sort `AgentRun.Id`
**Descending**, `MaxRecords = 5`, **no filters**, no joins, no grouping, no aggregated
attributes. Table `ArTable`, columns in order:

| Column | Cell | Expression |
|---|---|---|
| Id | `RcArId` | `LongIntegerToText(GetLastAgentRuns.List.Current.AgentRun.Id)` |
| ClaimId | `RcArClaimId` | `LongIntegerToText(IdentifierToLongInteger(…AgentRun.ClaimId))` |
| AgentName | `RcArAgentName` | `…AgentRun.AgentName` |
| Step | `RcArStep` | `…AgentRun.Step` |
| ToolName | `RcArToolName` | `…AgentRun.ToolName` |
| Replay | `RcArReplay` | `If(…AgentRun.Replay, "True", "False")` |
| StartedOn | — | `…AgentRun.StartedOn` |

**Two substitutions, made deliberately and flagged rather than silently swapped:**
`LiveHttpStatus` was requested but **does not exist on `AgentRun`** — it is a local of the
IdpSubmit wrappers and is never persisted, so the column is omitted rather than invented.
`CreatedAt` does not exist either (nor `CreatedOn`); **`StartedOn`** is used, being the
row's effective creation timestamp.

This unblocks rev 195's behaviour check: the `Replay` value is now readable on screen.

### `fieldmap-origin-default` — HELD. Its premise is already in the model.

`Seed_IdpFieldMap_Upsert` **already contains the exact coalesce that was requested**, in
both branches — in `PrepareCreate` and in `SetUpdateFields`:

```
ExistingRow.Origin  ←  If(Origin = "", "field", Origin)
```

So there is nothing to add there. `Version ← 1` and `IsActive ← True` are likewise set in
both branches.

**But the behaviour contradicts the model, twice measured.** The button was pressed a
second time; the result is byte-identical to the first:

```
IdpFieldMap seeded: 201 of 192 rows. Origin: field=0 kcb=0 bangke=0 thuoc=0 other=0.
ROW COUNT MISMATCH. Check the log for SEED_IDPFIELDMAP_NFC_REJECT.
```

Total stable at 201 across two presses — **not 384**, so upsert idempotency holds and the
PO's repeat-press check passes on that axis. The origin numbers do not move.

Everything in the chain has now been read and none of it explains the zeros:

- the upsert writes `Origin`, coalescing `""` to `"field"` — so 164 rows should hold `"field"`;
- the four aggregate filters are plain equality on the right literals:
  `IdpFieldMap.Origin = "field"`, `= "table:CHI PHÍ KHÁM CHỮA BỆNH"`,
  `= "table:Bảng kê chi phí khám chữa bệnh"`, `= "table:Thuốc điều trị"`;
- `AssignOutputs` stored order is `ExpectedRows`(1), `ActualRows`(2) … `OriginOtherRows`(7),
  so `OriginOtherRows = ActualRows − …` reads an `ActualRows` that is already set;
- `RunSeedIdpFieldMapNow` has **no local variables at all** — my hypothesis that the bare
  names resolved to unassigned locals was **falsified** — and all seven outputs are
  captured from `CallSeedIdpFieldMap`.

The model says the summary should read `field=164 kcb=13 bangke=13 thuoc=2 other=0`. The
running app says all five are 0. `other=0` in particular is arithmetically impossible
given `ActualRows=201`: it should be 201 under any reading of that expression. And because
it is 0, the `ORIGIN MISMATCH` clause — which fires on `OriginOtherRows <> 0` and exists
precisely to warn that rows carry an unexpected or empty Origin — **never fires.**

**A caveat I owe on the evidence itself.** Twice in this round Mentor opened its answer
with "I already have the full model text from the earlier read" and answered from its own
prior output rather than re-reading. In the run before it found the coalesce, I had told
it in my own words that these rows "should be `field`" — so the coalesce it then reported
is the one finding in this chain that could have been primed rather than read. The
behavioural evidence (two presses, `field=0`) is the stronger of the two and it points the
other way. I am not asserting the coalesce is absent; I am recording that the model read
and the runtime disagree and that the model read is the softer of the two.

**Nothing was built on top of this.** Adding `OriginDefaulted` now would produce a counter
claiming 164 rows repaired beside a column the app reports as empty — a second number that
cannot be checked against the first.

**Cheapest next instrument, not built, one line of scope:** a read-only display beside the
seed card listing `IdpFieldMap` grouped by `Origin` with a row count, plus the nine 6091
`K` values. It is inside the fence, writes nothing, spends no quota, and would settle in
one press both what the `Origin` column actually holds and whether the nine orphans are the
ones named below. Say the word and it goes in the next publish.

### `fieldmap-seed-outputs` — HELD, except one part now known to be unnecessary

- (a) `RejectedRows` — still wanted, still absent. Expected 0 today.
- (b) three mandatory-count aggregates — still wanted, still absent.
- (c) **not needed.** All seven outputs already reach the screen: `Seed_IdpFieldMap` →
  `RunSeedIdpFieldMapNow.Summary` → screen variable `IdpFieldMapSeedSummary` → expression
  widget `IdpFmSeedResultExpr` inside container `IdpFmSeedResultCont`, which is hidden
  while the summary is empty. The card looked blank only because nothing had been run yet.
- (d) the note rewrite — deferred with the rest, and per the PO's own branch the
  "never deletes" clause must now be replaced rather than kept, since item zero returned
  201 and that clause is the mechanism that produced the orphans.

Held because (a), (b) and (d) all publish into the same summary string whose numbers are
currently untrustworthy. Fixing the label while the figures beside it are wrong would make
the card read *more* authoritative and no more correct.

### `agentrun-harness-readout` verified by behaviour on rev 196

Administration screen reloaded after publish. The card renders with all seven columns in
order — `Id · ClaimId · AgentName · Step · ToolName · Replay · StartedOn` — the caption
above it, and five rows newest first: Id 1303, 1302, 1301, 1300, 1299, all ClaimId 20,
`Replay` rendered as the literal text `False`, `StartedOn` populated, `ToolName` empty.

**`ToolName` being empty is not a rendering fault** — it matches the read of
`Seed_AgentRuns`, which omits that argument on most of its 48 calls.

**What this does NOT yet prove.** All five visible rows are dated 2026-09-22, i.e. written
long before rev 195. Their `Replay = False` is the Boolean attribute's default, not
evidence that the new assignment executes. A positive confirmation needs a **new**
`AgentRun` row created after rev 195 — which means running something that calls
`WriteAgentRun` with `Settings.ReplayMode = True`, then reading the top row here. The
instrument now exists; the measurement is still outstanding.
