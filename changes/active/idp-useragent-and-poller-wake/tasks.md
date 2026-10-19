# Change — User-Agent cho 3 REST integration + Wake cho IdpAssessmentPoller

**Trạng thái:** 🟠 Fix 1 huỷ (làm tay trong Studio) · **Fix 2 đã publish rev 170 + verify bằng XRE** · **Mở:** 23/09/2026
**App:** `ClaimMVP` (`2a401c4d-d87e-4c44-8692-9ad661840125`), rev khởi điểm **169**
**Nguồn:** audit read-only 22–23/09 qua `context_search` + `context_graph` (XRE)
**Quan hệ:** hẹp hơn `idp-integration-live`; sửa code đã tồn tại ở rev 169, không thay thế change đó

Hai lỗi chặn. Không tái cấu trúc gì khác.

---

## Bằng chứng (đo được, không suy từ tên)

**Lỗi 1 — thiếu User-Agent.** `context_search` → `objectType: restIntegrations`, đọc
`additionalData.methods[].inputParameters[]`:

| Integration | Method có `httpName == "User-Agent"`, `placement: Header` |
|---|---|
| `IdpAuth` | **2/2** |
| `IdpDocuments` | **0/7** |
| `IdpWorkflow` | **0/4** |
| `IdpMasterData` | **0/2** |

Không có callback nào bù vào: `additionalData` của cả 4 integration không có field
`onBeforeRequest`, và quét 148 action trong app không có tên/mô tả nào khớp
`OnBefore|OnAfter|UserAgent|header`. 13/15 method **không có đường nào** để gửi header.

Cloudflare đứng trước API này và chặn User-Agent mặc định của HTTP client bằng
**HTTP 403, body `error code: 1010`**. Không có trong Swagger. Trùng bẫy #1 ở `CLAUDE.md` §3.

**Lỗi 2 — timer không bao giờ chạy.** `context_graph` root=`IdpAssessmentPoller`:
node `ITimer` có `"Schedule": null`, `"TimeoutInMinutes": 20`. Quét **13 graph phủ đủ 17/17
action `Idp*`**: trong toàn bộ kho chỉ có **một** cạnh chạm `Wake` —
`[17] ITimer/IdpAssessmentPoller --TimerActions--> [22] ReferenceIAction/WakeIdpAssessmentPoller`,
tức là **khai báo** của timer chứ không phải lời gọi. `IdpStartAssessment` (2085 node) không có
node `Wake*`; `IdpAssessmentPoller_Run` cũng không. Hệ quả: mọi `IdpAssessmentRun` tạo ra
nằm nguyên trạng thái ban đầu vĩnh viễn.

---

## Việc

- [~] **1. HUỶ — làm tay trong Studio.** `IdpDocuments` — thêm OnBeforeRequest. Đặt property **On Before Request** của
  integration = `New OnBeforeRequest`. Trong callback: khai local var kiểu `HTTPHeader`,
  set `Name = "User-Agent"`, `Value = Settings.IdpUserAgent`; `ListAppend` vào
  `Request.Headers`. Lặp cho header thứ hai: `Name = "Accept"`,
  `Value = "application/json, text/plain, */*"`. Kết thúc bằng **Assign
  `CustomizedRequest = Request`**. Áp cho cả 7 method qua đúng một callback.
- [~] **2. HUỶ — làm tay trong Studio.** `IdpWorkflow`, 4 method.
- [~] **3. HUỶ — làm tay trong Studio.** `IdpMasterData`, 2 method.
- [x] **4. `IdpAuth` — KHÔNG ĐỘNG VÀO.** Đã giữ nguyên qua cả hai run Mentor. Hai method của nó đã gửi User-Agent bằng input
  parameter và đây là đường duy nhất hiện đang qua được Cloudflare. Không thêm callback,
  không đổi param, không "thống nhất cho đẹp".
- [x] **5. (rev 170, đã verify) `IdpStartAssessment` — Wake sau khi chạy thành công.** Ở cuối flow, sau khi
  `IdpRunAssessment` trả về thành công, gọi `WakeIdpAssessmentPoller`. Chỉ trên nhánh
  thành công; nhánh lỗi không wake.
- [x] **6. (rev 170, đã verify) `IdpAssessmentPoller_Run` — tự nạp lại.** Ở cuối flow, nếu còn `IdpAssessmentRun`
  đang bay thì gọi `WakeIdpAssessmentPoller`; không còn thì **không** wake.
  "Đang bay" = `State` không thuộc {`COMPLETE`, `PARTIAL`, `ERROR`} — theo mô tả attribute,
  `State` để trống cho tới khi poller đặt. Dùng lại khái niệm của aggregate
  `GetInFlightRuns` đã có sẵn đầu flow, kiểm tra **sau** khi các Update chạy xong.
- [x] **7. Timer — KHÔNG đặt Schedule.** Mentor xác nhận không đổi `Schedule` và `TimeoutInMinutes`. Giữ `Schedule = null`, giữ
  `TimeoutInMinutes = 20`. Lịch cố định không diễn tả được backoff 3s→5s→10s→30s với trần
  15 phút; tự wake mới là thứ hiện thực hoá nó. 20 phút phải **lớn hơn** trần 15 phút để
  timer không chết giữa lượt poll.
- [x] **8. Kiểm chứng sau khi publish (Fix 2 — xong 23/09)** — `context_graph` lại 3 root
  (`IdpStartAssessment`, `IdpAssessmentPoller_Run`, và một method bất kỳ của
  `IdpDocuments`), xác nhận có cạnh vào `WakeIdpAssessmentPoller` và có node callback.
  Đọc bằng máy, không tin report của Mentor.

---

## Rủi ro đã biết, chưa gỡ được

**`Settings.IdpUserAgent` chưa xác minh là có tồn tại.** MCP không đọc được Settings của app
này: Context Service không có objectType `settings`, `app_info` không mang config, và XRE cắt
tham chiếu Setting thành `dst = -1` đồng thời thay literal bằng `TextHash`. Bằng chứng gián
tiếp: trong `IdpGetToken`, argument nối vào `RestActionInput/UserAgent` của cả
`IdpLoginMethod` và `IdpUserMeMethod` được cấp bởi một external reference chưa resolve —
đúng hình dạng của một Setting. **Trước khi làm việc 1–3, mở Studio → Data → Settings xác
nhận `IdpUserAgent` có mặt và giá trị là chuỗi user-agent trình duyệt.** Nếu không có thì
tạo Setting trước, đừng để callback trỏ vào khoảng không.

**Ràng buộc của callback (ODC docs):** chỉ **thêm hoặc sửa** header, **không xoá** được
header sẵn có; giá trị header phải **ASCII thuần** — chuỗi user-agent trình duyệt thoả.

---

## Xong nghĩa là

- Cả 15 method đều gửi được User-Agent kiểu trình duyệt; `IdpAuth` vẫn y nguyên
- `IdpStartAssessment` wake poller; poller tự nạp lại **chỉ khi** còn việc
- `Schedule` vẫn null, `TimeoutInMinutes` vẫn 20
- Không action/entity/structure/screen nào khác bị sửa — kiểm bằng `context_graph`, không bằng lời Mentor


---

## Nhật ký phiên 23/09 (hai run Mentor, session `f34d1431…`)

**Run 1 `bb711326…`** — làm cả Fix 1 + Fix 2. `changeApplied: true`, `errorCount: 6`
(`"Unavailable feature"` ×3), `warningCount: 144`. Không publish.

**Run 2 `7ea66cc2…`** — huỷ Fix 1, dọn sạch, sửa điều kiện Fix 2b.
`changeApplied: true`, `errorCount: 6` (không đổi), `warningCount: 144`.

Mentor báo đã gỡ **21 element** (7 mỗi API × 3 API) khỏi ba callback `OnBeforeRequest`:
local var `UAHeader`, local var `AcceptHeader`, assign "Set UA header", assign
"Set Accept header", execute `AppendUA` (ListAppend), execute `AppendAccept` (ListAppend),
assign "CustomizedRequest = Request". **Ba callback KHÔNG bị xoá** — chúng đã tồn tại từ
trước và chứa một assign đặt base URL.

Fix 2b: filter của aggregate `CheckRemaining` đổi từ `State = ""` sang điều kiện đúng —
wake chỉ khi có ít nhất một `IdpAssessmentRun` có `State` không thuộc
{COMPLETE, PARTIAL, ERROR}.

### Hai điều chỉnh lại so với audit 22–23/09

1. **Ba callback `OnBeforeRequest` ĐÃ tồn tại từ trước** trên `IdpDocuments`,
   `IdpWorkflow`, `IdpMasterData`, và chứa assign đặt base URL. Audit kết luận không có,
   dựa trên việc Context Service không trả field `onBeforeRequest` và không action nào tên
   `OnBefore*`. Cả hai chỉ là vắng mặt trong index, không phải vắng mặt trong model.
   **Rút ra: không dùng context_* để chứng minh một element KHÔNG tồn tại.**

2. **6 lỗi `"Unavailable feature"` có từ trước phiên này**, không phải do Mentor sinh ra.
   Mentor quy chúng về request format multipart trên `IdpSubmitMethod`,
   `IdpSubmitSyncMethod` và bốn structure `RequestPart`. rev 169 publish được nên 6 lỗi này
   không chặn publish. **Chưa kiểm độc lập** — cần xem trong Studio.

### Trạng thái cần biết trước khi làm tiếp

- Thay đổi Fix 2 đang nằm trong **draft của session Mentor `f34d1431…`, chưa publish**.
  Session vẫn để mở có chủ đích: chưa rõ đóng session có làm mất draft không.
- Không verify được bằng `context_graph`: nó đọc OML đã publish, mà draft thì chưa publish.
  Verify chỉ làm được **sau** khi publish, hoặc mở Studio xem trực tiếp.


---

## Publish + verify 23/09 (rev 170)

`mentor_publish` session `f34d1431…` → `publicationKey d1345568…`, `revision 170`.
`publish_status` → `outcome: success`, `status: Finished`.
`app_info` → `revision 170`, `revisionDateTime 2026-09-23T07:14:08.452011Z`,
`modelDigest d88add57… → da8d91fa…`. Session đã đóng (`closed: true`).

**Verify bằng `context_graph` trên model ĐÃ PUBLISH, không dùng report của Mentor:**

- root=`IdpStartAssessment` (2087 node): `[41] IExecuteServerActionNode/CreateRun
  --Connector--> [25] IExecuteServerActionNode/WakePoller --Action-->
  [60] ReferenceIAction/WakeIdpAssessmentPoller`, và `[25] --Connector--> [27] IEndNode`.
- root=`IdpAssessmentPoller_Run` (2125 node): `[44] IIfNode --Connector-->
  [21] IExecuteServerActionNode/WakePoller --Action--> [59] ReferenceIAction/WakeIdpAssessmentPoller`.
  Điều kiện của `[44]`: `[102] BinaryOperation Operator=GT` giữa
  `CheckRemaining.Count` (`[18] IAggregateNode/CheckRemaining` → `RuntimeProperty/Count`)
  và `[105] Integer Value='0'` — tức `Count > 0`.
- Filter của `CheckRemaining` (`[115] ExpressionAST`): ba phép `NEq`, cả ba đều so
  `[294] IEntityAttribute/State` của `IdpAssessmentRun` với một literal Text, nối bằng
  `And`: `(State <> A) And (State <> B) And (State <> C)`.

**Giới hạn đọc:** ba literal A/B/C **không đọc được** — XRE trả `Value: null` kèm
`TextHash`. Chỉ đọc được độ dài: **8, 7, 5**, khớp với `COMPLETE` (8), `PARTIAL` (7),
`ERROR` (5). Độ dài khớp không phải là chứng minh nội dung. Muốn chắc phải mở Studio.
