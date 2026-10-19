# Change — Seed_IdpFieldMap + nới FieldCode

**Trạng thái:** 🟠 Đang dựng · **Mở:** 29/09/2026
**App:** `ClaimMVP` (`2a401c4d-d87e-4c44-8692-9ad661840125`), rev khởi điểm **184**
**Nguồn dữ liệu:** `insurance BNK/IDP_API_Spec/IdpFieldMap-seed.csv`
(md5 `21732d3ebb6e8323b1cae96108196273`, **163 dòng dữ liệu**)
**Chặn:** phép thử mẫu số per-template của `fieldmap-k-lookup`, và mọi logic downstream đọc `FieldCode`

---

## Vì sao cần

`IdpFieldMap` đang **0 dòng**. Hệ quả đo được: mẫu số `MandatoryTotal` = 0 ⇒ `ClaimConfidence`
và `Completeness` = 0 ở **mọi** hồ sơ, và `ErrorCode = FIELDMAP_MISSING` ở mọi lần chạy harness
(đã xác nhận rev 184). Không seed thì không phân biệt được code đúng với code lỗi.

## Dữ liệu — đã kiểm bằng máy 29/09

| Kiểm | Kết quả |
|---|---|
| Số dòng | **163** |
| `IsMandatory` | 141 `False` · 22 `True` · **0 trống** |
| `IsActive` | 163 `True` · 0 `False` |
| Template | 6090(14) 6091(39) 6093(21) 6094(23) 6095(16) 6096(19) 6097(19) 6098(12) |
| Mandatory theo template | **6091 = 14** · **6093 = 8** · còn lại 0 |
| NFC | **163/163 NFC**, 0 dòng chứa dấu kết hợp rời (U+0300/0301/0303/0309/0323) |
| Trùng `(IdpTemplateId, K)` | **0** |
| `Version` | tất cả `1` |
| `DataType` | TEXT 109 · DATE 21 · ENUM 12 · MONEY 11 · CODE 6 · INT 4 |
| `TargetEntity` | ClaimDocument 87 · InsuredEvent 60 · ClaimLine 16 |

> File đã đổi giữa hai lần tôi đọc (PO sửa): bản trước có 164 dòng, 1 dòng `IsMandatory` trống,
> 1 dòng `IsActive=False`, và template 6092 với 1 dòng. Bản hiện tại sạch cả ba.

## Việc

- [ ] **1. Nới `IdpFieldMap.FieldCode`: Text(20) → Text(50).** PO chốt 29/09.
  **Lý do:** `LINE_PATIENT_SELF_PAID` dài **22** ⇒ Text(20) **cắt âm thầm** thành
  `LINE_PATIENT_SELF_PA`. Dòng đó là `IsMandatory=True`, `DataType=MONEY`,
  `TargetEntity=ClaimLine`, K = `Người bệnh tự trả (đồng)` — tức một **khoá** ánh xạ ra cột tiền.
  Khoá bị cắt tệ hơn khoá thiếu, vì nó vẫn trông như đủ (cùng họ với bẫy `match_w`).
  Không đổi mã trong CSV vì `LINE_UNIT_PRICE_HOSP` đang ở **đúng 20** — hết dư, mã 21 ký tự sau
  này lại cắt âm thầm. Nới cột diệt cả lớp lỗi. Bảng 0 dòng ⇒ không rủi ro migration.
  **Đây là thay đổi data model** — chỉ đúng một attribute length, không sinh entity, không sinh
  logic tiền.
- [ ] **2. `Seed_IdpFieldMap_Upsert`** — helper private, UPSERT một dòng, khoá `(IdpTemplateId, K)`:
  có thì update, không có thì create. **Không bao giờ delete.**
  **Guard NFC:** nếu `K` chứa bất kỳ dấu kết hợp rời nào (U+0300 U+0301 U+0303 U+0309 U+0323)
  thì **TỪ CHỐI** dòng đó — không ghi, ghi log. ODC không có hàm normalise (15 hàm Text, không có
  `Normalize`) nên chỉ làm được nửa **từ chối**; nửa **chuẩn hoá** nằm phía đĩa.
- [ ] **3. `Seed_IdpFieldMap_A/B/C/D`** — 4 batch, gom theo template cho dễ soi tay:
  A = 6090+6091 (53) · B = 6093+6094 (44) · C = 6095+6096 (35) · D = 6097+6098 (31). Tổng **163**.
- [ ] **4. `Seed_IdpFieldMap`** — orchestrator: gọi A→B→C→D, rồi một Aggregate đếm
  `IdpFieldMap`. Output **`ExpectedRows`** (=163, literal) và **`ActualRows`** (đếm thật).
  Lệch ⇒ có dòng bị từ chối hoặc ghi lỗi. Theo đúng khuôn `RequestedCount` vs `ReturnedCount`
  đã dùng ở `IdpGetTemplateCsv` cho bẫy "template bị drop âm thầm".
- [ ] **5. Publish + verify.** Sau seed, chạy lại harness trên capture 21406 (template **6093**).

## Xong nghĩa là

- `IdpFieldMap` có **163** dòng · `ActualRows == ExpectedRows == 163`
- `FieldCode` không dòng nào bị cắt — `LINE_PATIENT_SELF_PAID` còn đủ 22 ký tự
- Harness trên 21406: `ErrorCode` **không còn** `FIELDMAP_MISSING`, và
  **`MandatoryTotal` = 8** (không phải 22) ⇒ chứng minh filter `IdpTemplateId` ăn thật
- Seed **idempotent**: chạy hai lần cho cùng kết quả, không nhân đôi dòng

## Không làm

- Không đổi `K`, `FieldCode`, `IsMandatory` của bất kỳ dòng nào so với CSV
- Không sinh entity mới, không chạm trường tiền, không chạm `RunAdjudicationChain`
- Không delete dòng nào — seed chỉ create/update

---

## Nhật ký

### Scaffold — run `ba3f07d8…` (session `044c023b…`), chưa publish

`succeeded` · `changeApplied: true` · `errorCount 7` (không đổi) · `warningCount 136` (+2 so với
rev 184: hai notice *"Unused user action"* cho `Seed_IdpFieldMap` và `Seed_IdpFieldMap_Upsert` —
đúng, vì chưa có caller).

- ✅ `IdpFieldMap.FieldCode` → **Text(50)**. Mentor xác nhận chỉ đổi property `Length`, không đổi
  tên/type/mandatory, không chạm attribute nào khác.
- ✅ `Seed_IdpFieldMap_Upsert` — guard NFC là một node If `HasCombiningMark?` ngay sau Start:

  ```
  Index(K, Chr(768), …) <> -1 or Index(K, Chr(769), …) <> -1
  or Index(K, Chr(771), …) <> -1 or Index(K, Chr(777), …) <> -1
  or Index(K, Chr(803), …) <> -1
  ```
  768=U+0300 grave · 769=U+0301 acute · 771=U+0303 tilde · 777=U+0309 hook above ·
  803=U+0323 dot below. Đúng cả năm.
- ✅ Upsert lookup: aggregate `GetExistingRow`, `MaxRecords = 1`, hai filter
  `IdpTemplateId = IdpTemplateId` và `K = K`.
- ✅ **Không nhánh nào có delete** — cả nhánh reject lẫn nhánh upsert.
- ✅ 4 shell `Seed_IdpFieldMap_A/B/C/D` + orchestrator `Seed_IdpFieldMap`
  (`ExpectedRows` / `ActualRows`).

**Hai lỗi tôi bắt được trong scaffold, đã gửi sửa:**

1. **`Index()` lệch một ô.** Mentor truyền start position = **1**, nhưng `Index` **0-based** ⇒
   bỏ qua ký tự ở vị trí 0. Dấu kết hợp ở đầu chuỗi sẽ không bị bắt. Thực tế gần như không xảy ra
   (dấu kết hợp phải đi sau một chữ nền), nhưng guard thì phải kín, và sửa không tốn gì → đổi về **0**.
   Cùng họ với bẫy `Substr` 0-based đã ghi ở `mentor-odc-traps-2026-09-21`.
2. **Rejection không để lại dấu.** Nhánh True ghi vào local var `RejectionLog` — biến chết khi
   action return ⇒ dòng bị từ chối **vô hình**. Đổi sang `LogMessage` với chuỗi
   `SEED_IDPFIELDMAP_NFC_REJECT` + `IdpTemplateId` + `K`. (Node `Message` không dùng được trong
   Server Action — Mentor tự phát hiện đúng; `LogMessage` mới là cái đúng.)
   Lệch `ExpectedRows` vs `ActualRows` vẫn là tín hiệu phát hiện, nhưng muốn biết **dòng nào**
   thì phải có log thật.

### Batch A — run `7fa42ee8…` (đang chạy)

Gồm hai fix trên + 53 dòng (6090 × 14, 6091 × 39). Kỳ vọng: 53 lời gọi Upsert, **14** dòng
`IsMandatory = True` (tất cả thuộc 6091).

**Bẫy transcribe đã dặn Mentor:** 6090 có `Chuẩn đoán` còn 6091 có `Chẩn đoán` — **khác nhau có
thật**, không được "sửa lỗi chính tả"; `K` phải đúng từng ký tự vì nó là khoá tra cứu.
`Địa chỉ` · `Ngày sinh` · `Giới tính` · `Số lượng` xuất hiện ở **cả hai** template — là hai dòng
khác nhau, không được dedupe.

### Còn thiếu để seed chạy được

`Seed_IdpFieldMap` **chưa có đường kích hoạt**. Khuôn có sẵn trong app: `RunSeedBatch0Now`
("Calls Seed_AssessmentAndTerms so the seed can be re-triggered without republishing.
Expose to Administrators via a screen."). Cần thêm một nút trên màn Administration, làm sau
khi 4 batch xong.

### Batch A — run `7fa42ee8…` ✅

Hai fix helper đã vào:
- `Index(K, Chr(n), 0)` cho **cả năm** mã 768/769/771/777/803 — start position về **0**.
- Node `LogNfcReject` gọi `LogMessage`, `Category = "Seed_IdpFieldMap_Upsert"`, `Message =`
  `"SEED_IDPFIELDMAP_NFC_REJECT template=" + LongIntegerToText(IdpTemplateId) + " K='" + K + "'"`.
  Giữ End node riêng ⇒ dòng bị từ chối **không bao giờ được ghi**. Local var `RejectionLog` đã xoá.

`Seed_IdpFieldMap_A`: **53** lời gọi Upsert · **14** dòng `IsMandatory = True`, tất cả thuộc 6091.
Mentor liệt kê đủ 14 tên và **khớp đúng** nguồn CSV: Place Of Treatment · Tên bệnh nhân · Ngày sinh ·
Giới tính · Admission Date · Discharge Date · Diagnosis Codes · Chẩn đoán · Tổng chi phí điều trị ·
Nội dung · Số lượng · Thành tiền BV (đồng) · Quỹ BHYT (đồng) · Người bệnh tự trả (đồng).

`errorCount 7` · `warningCount 135` (giảm 1 vì `Seed_IdpFieldMap_Upsert` đã có caller) · 0 mới.

### Batch B — run `d5c50941…` ✅

`Seed_IdpFieldMap_B`: **44** lời gọi · **8** dòng mandatory, tất cả thuộc 6093. Mentor liệt kê đủ 8
và **khớp đúng**: Place Of Treatment · Tên bệnh nhân · Giới tính · **Ngày/ tháng/ năm sinh** ·
Admission Date · Discharge Date · Diagnosis Codes · Kết quả chẩn đoán.
`Ngày/ tháng/ năm sinh` transcribe đúng khoảng trắng sau mỗi dấu `/`.

`errorCount 7` · `warningCount 135` · 0 mới.

> **Xác nhận dự đoán mẫu số:** 6093 có đúng **8** dòng mandatory. Sau khi seed, harness trên
> capture 21406 phải cho `MandatoryTotal = 8`. Ra **22** nghĩa là filter `IdpTemplateId` không ăn.

### Batch C — run `612f5687…` (đang chạy)

6095 + 6096, 35 dòng, **0 dòng mandatory** (đã dặn Mentor: không được tự bịa ra dòng mandatory nào).
Bẫy: 6095 ghi `Chuẩn đoán`, 6096 ghi `Chẩn đoán` — khác nhau có thật.
`REVIEW_93823` (`Thông tin bổ sung`) xuất hiện ở 6096 và còn lặp lại ở 6097, 6098 — dòng riêng,
không merge.

### Batch C — run `612f5687…` ✅

`Seed_IdpFieldMap_C`: **35** lời gọi · **0** mandatory (đúng — 6095 và 6096 không có dòng bắt buộc).
`errorCount 7` · `warningCount 135` · 0 mới.

### Batch D — run `cf466400…` ✅

`Seed_IdpFieldMap_D`: **31** lời gọi · **0** mandatory.
Bẫy đã dặn: `Ngày mổ/Ngày phẫu thuật` **không** có khoảng trắng quanh dấu `/`, khác
`Ngày/ tháng/ năm sinh` ở batch B (có khoảng trắng) — cả hai đúng, **không được làm cho giống nhau**.
`REVIEW_93823` xuất hiện ở 6096 + 6097 + 6098 = **ba dòng riêng**, không merge.

**Tổng bốn batch: 53 + 44 + 35 + 31 = 163** — khớp `ExpectedRows`.
**Mandatory: 14 + 8 + 0 + 0 = 22** — khớp CSV.
`errorCount 7` · `warningCount 135` · 0 mới.

### Trigger — run `59ca4239…` (đang chạy)

`RunSeedIdpFieldMapNow` (theo khuôn `RunSeedBatch0Now` đã có trong app) + một section **mới** trên
màn Administration, **tách khỏi** section "Parse a captured response" và không sửa section đó.
Section mới: heading "Seed IdpFieldMap" · một dòng giải thích · nút "Run IdpFieldMap seed" ·
vùng hiện `Summary`. `Summary` nêu rõ `ActualRows / ExpectedRows` và in **MISMATCH** kèm hướng dẫn
tra log `SEED_IDPFIELDMAP_NFC_REJECT` khi hai số lệch.
Ràng buộc đã nêu: section **không** gọi IDP, **không** chạm claim / trường tiền / trạng thái claim.

### Trigger — run `59ca4239…` ✅ · publish **rev 185**

- `RunSeedIdpFieldMapNow` (Server Action) — gọi `Seed_IdpFieldMap`, chuyển tiếp
  `ExpectedRows`/`ActualRows`, và dựng `Summary`:

  ```
  "IdpFieldMap seeded: " + IntegerToText(ActualRows) + " of "
    + IntegerToText(ExpectedRows) + " rows."
    + If(ActualRows <> ExpectedRows,
         " MISMATCH - check the log for SEED_IDPFIELDMAP_NFC_REJECT.",
         " OK.")
  ```
- Màn Administration: section **mới** "Seed IdpFieldMap" — nút "Run IdpFieldMap seed" →
  screen action `RunIdpFieldMapSeed` → gán `Summary` vào local `IdpFieldMapSeedSummary`, hiện bằng
  expression widget dạng text thường (không Message/Toast, không ghi Entity).
- Mentor xác nhận section **"Parse a captured response" không bị sửa** — container `parseCard` và
  toàn bộ nội dung (`ParseH3`, `RawJsonInputLabel`, TextArea…) giữ nguyên.

`errorCount 7` · `warningCount **134**` — về đúng baseline rev 184, hai notice "Unused user action"
đã tự hết vì mọi action mới đều có caller. 0 lỗi/cảnh báo mới.

---

## Trạng thái: code xong, **chưa chạy seed**

Bảng `IdpFieldMap` vẫn **0 dòng** — publish chỉ đưa code lên, seed chỉ chạy khi bấm nút.

**Việc của PO, theo thứ tự:**

1. Vào màn **Administration** → section **"Seed IdpFieldMap"** → bấm **"Run IdpFieldMap seed"**.
   Kỳ vọng dòng kết quả: `IdpFieldMap seeded: 163 of 163 rows. OK.`
   Nếu thấy **MISMATCH** ⇒ có dòng bị guard NFC loại; tra log `SEED_IDPFIELDMAP_NFC_REJECT` để biết
   dòng nào. (Dự kiến không xảy ra — file nguồn 163/163 NFC, 0 dấu kết hợp rời.)
2. Bấm **lần thứ hai** để kiểm idempotent — phải vẫn ra `163 of 163`, **không** thành 326.
3. Chạy lại harness trên capture **21406** (template **6093**). Hai điều cần nhìn:
   - `ErrorCode` **không còn** `FIELDMAP_MISSING`
   - **`MandatoryTotal` = 8** ⇒ filter `IdpTemplateId` ăn thật.
     Nếu ra **22** ⇒ mẫu số vẫn đếm cả 8 template ⇒ `fieldmap-k-lookup` còn lỗi.

---

## Đối chiếu 163 nhãn với NGUỒN — 30/09, zero quota, không gọi API

PO yêu cầu gọi `GET /template/6090…6098` + `/template/compact` + `/template/document-set` để đối
chiếu với nguồn. **Không cần gọi:** bản export định nghĩa template của tenant 177 **đã có trên đĩa**
tại `insurance BNK/Bnk IDP/00. Deliverables/Templates_Claim_MVP_tenant177/definitions/*.json`
(9 file, tạo 2026-09-17, `manifest.json` ghi `tenant.id 177`, `partnerId 40`,
`baseUrl https://idp-api.idpstg.bnksolution.com`). Mỗi file có `fields[].name` và
`tableFields[].fields[].name` — đúng thứ IDP trả về trong `k`.

> Giới hạn: bản export ngày **17/09**. Nếu tenant đã đổi template sau đó thì phải gọi API mới biết.
> Nhưng đây **là** định nghĩa nguồn, không phải bản đọc từ tài liệu — đúng thứ PO muốn đối chiếu.

### Kết quả, đã normalise NFC hai phía

| Template | CSV | def scalar | def table | Kết quả |
|---|---|---|---|---|
| 6090 | 14 | 12 | 2 | OK |
| **6091** | 39 | 26 | 13 | **9 LỆCH** |
| **6092** | **0** | 16 | 13 | **KHÔNG CÓ DÒNG NÀO** |
| 6093 | 21 | 21 | 0 | OK |
| 6094 | 23 | 23 | 0 | OK |
| 6095 | 16 | 16 | 0 | OK |
| 6096 | 19 | 19 | 0 | OK |
| 6097 | 19 | 19 | 0 | OK |
| 6098 | 12 | 12 | 0 | OK |

**Tổng lệch thật: 9 / 163.** Đúng con số "9/13" PO nhớ, và đúng 9 cột đó.

### Nguyên nhân gốc: 9 nhãn cột bảng của 6091 chính là nhãn của **6092**

Kiểm 9/9: từng nhãn lệch đều **có mặt nguyên văn** trong `tableFields` của **6092**.

| CSV ghi (= nhãn 6092) | 6091 thật sự là | FieldCode | mand |
|---|---|---|---|
| `Đơn giá BV (đồng)` | `Đơn giá BV` | LINE_UNIT_PRICE_HOSP | |
| `Đơn giá BH (đồng)` | `Đơn giá BH` | LINE_UNIT_PRICE_INS | |
| `Tỷ lệ thanh toán theo DV` | `Tỷ lệ thanh toán theo dịch vụ` | LINE_SERVICE_RATE | |
| `Thành tiền BV (đồng)` | `Thành tiền BV` | LINE_AMOUNT_HOSP | **True** |
| `Thành tiền BH (đồng)` | `Thành tiền BH` | LINE_AMOUNT_INS | |
| `Quỹ BHYT (đồng)` | `Quỹ BHYT` | LINE_PAID_BY_SI | **True** |
| `Người bệnh cùng chi trả (đồng)` | `Người bệnh cùng chi trả` | LINE_PATIENT_COPAY | |
| `Khác (đồng)` | `Khác` | LINE_OTHER_SOURCE | |
| `Người bệnh tự trả (đồng)` | `Người bệnh tự trả` | LINE_PATIENT_SELF_PAID | **True** |

4 cột còn lại (`Nội dung`, `Đơn vị tính`, `Số lượng`, `Tỷ lệ thanh toán BHYT`) trùng ở cả hai
template nên khớp — vì thế là 9/13 chứ không phải 13/13.

**Hệ quả đo được:** 3 trong 14 dòng mandatory của 6091 **không bao giờ khớp được**. Với một chứng
từ 6091 hoàn hảo, `MandatoryTotal = 14` nhưng tối đa chỉ tìm được 11 ⇒ `Completeness` chặn trên ở
**0,7857** và `ClaimConfidence` tính trên 11 field. Âm thầm báo thiếu.

### 6093 — "lệch" là dương tính giả, và là bằng chứng NFC còn thiếu bấy lâu

`Phương pháp phẫu thuật` ban đầu báo lệch. Đọc codepoint ba nguồn:

| Nguồn | len | NFC? |
|---|---|---|
| **definition trên đĩa** | **25** | **KHÔNG** — chứa U+0301, U+0303, U+0323 rời |
| seed CSV | 22 | có |
| fixture 21408 (`k` IDP trả về) | 22 | có |

`fixture == CSV` → **True**. `fixture == definition` → **False**.

⇒ **CSV đúng, file definition trên đĩa mới là bản decomposed.** IDP trả về NFC.
Census toàn bộ: **1/192** tên trong 9 file definition là decomposed — đúng cái tên này.
Đây là bằng chứng cứng đầu tiên rằng tiếng Việt decomposed **thật sự tồn tại trong artifact nguồn
của chính chúng ta**, và là lý do guard NFC ở `Seed_IdpFieldMap_Upsert` không phải phòng xa vô ích.
**Không được "sửa" CSV ở dòng này.**

### 6092 — TODO đã đóng: nó có gì

`Giấy ra viện/Hospital discharge papers`, **16 scalar + 1 bảng 13 cột**, và **có trong
`csvChoPostRequest`** (`6090,…,6098`) ⇒ IDP **có thể** phân loại một chứng từ về 6092, và khi đó
ta **không có dòng field map nào** ⇒ `FIELDMAP_MISSING`.

16 scalar: `Place Of Treatment` · `Số vào viện` · `Tên bệnh nhân` · `Tuổi` · `Giới tính` ·
`Ngày/ tháng/ năm sinh` · `Số thẻ bảo hiểm y tế` · `Địa chỉ` · `Admission Date` · `Discharge Date` ·
`Diagnosis Codes` · `Kết quả chẩn đoán` · `Phương pháp điều trị` · `Phương pháp phẫu thuật` ·
`Ngày phát hành` · `Thông tin bổ sung`

13 cột bảng `Bảng kê chi phí khám chữa bệnh`: `Nội dung` · `Đơn vị tính` · `Số lượng` ·
`Đơn giá BV (đồng)` · `Đơn giá BH (đồng)` · `Tỷ lệ thanh toán theo DV` · `Thành tiền BV (đồng)` ·
`Tỷ lệ thanh toán BHYT` · `Thành tiền BH (đồng)` · `Quỹ BHYT (đồng)` ·
`Người bệnh cùng chi trả (đồng)` · `Khác (đồng)` · `Người bệnh tự trả (đồng)`

Gán `FieldCode` cho 29 field này là **quyết định nghiệp vụ** — tôi không tự đặt.

### 8 dòng REVIEW_* của 6093 — nhãn đúng, thiếu quyết định

Cả 8 (`REVIEW_86540` `REVIEW_92128` `REVIEW_3389` `REVIEW_28406` `REVIEW_64317` `REVIEW_69072`
`REVIEW_47362` `REVIEW_93823`) đều **khớp definition**. Việc còn lại không phải sửa nhãn mà là
**đặt FieldCode nội bộ thật** thay placeholder — cũng là quyết định nghiệp vụ.

### ⚠️ Bẫy thứ tự: sửa nhãn TRƯỚC khi bấm seed

`Seed_IdpFieldMap_Upsert` upsert theo khoá `(IdpTemplateId, K)` và **không bao giờ delete**.
Seed bây giờ với 9 nhãn sai rồi sửa sau ⇒ 9 dòng mới được tạo, **9 dòng cũ ở lại vĩnh viễn** ⇒
172 dòng, `ExpectedRows` 163 ⇒ **MISMATCH vĩnh viễn** + 9 mapping chết.
Không chặn Stage 0 (Stage 0 cần 0 dòng, vẫn đúng).

### Câu hỏi còn mở, chỉ một capture 6091 thật trả lời được

16 dòng `TargetEntity = ClaimLine` (6091 có 13, 6090 có 2, 6098 có 1) khoá theo **tên cột bảng**.
Nhưng dữ liệu bảng về trong `data.tables[]`, còn parser khớp `IdpFieldMap.K` với `fields[].k`.
Nếu cột bảng **không** xuất hiện trong `fields[]` thì **cả 16 dòng ClaimLine không bao giờ khớp**,
bất kể nhãn đúng hay sai — tức sửa 9 nhãn là **cần nhưng chưa đủ**.
Fixture 21408 là 6093 (0 bảng) nên không trả lời được. Đúng là "rủi ro lớn nhất còn lại của
IdpTables" mà PO nêu ở điểm 3.

---

## Seed CSV v2 — 192 dòng, PO sửa 30/09. Đã verify độc lập bằng máy

md5 `a60cb97c4b2ed64bf34824ce4004b0ed` (bản trước: `21732d3e…`)

| Kiểm | PO nói | Tôi đo | |
|---|---|---|---|
| Số dòng | 192 | **192** | ✓ |
| Theo template | — | 6090:14 6091:39 **6092:29** 6093:21 6094:23 6095:16 6096:19 6097:19 6098:12 | ✓ |
| Mandatory | 6091→14 · 6092→13 · 6093→8 | **14 · 13 · 8**, tổng **35** | ✓ |
| Lệch vs definition | 0 | **0** trên cả 9 template | ✓ |
| NFC | 192/192 | **0 dòng không NFC** | ✓ |
| Trùng `(template,K)` | — | **0** | ✓ |
| `IsActive` | — | 192/192 True, 0 dòng `IsMandatory` trống | ✓ |
| `FieldCode` dài nhất | — | **22** (`LINE_PATIENT_SELF_PAID`) ≤ 50 sau khi nới | ✓ |

**Kiểm mạnh hơn PO yêu cầu — `Origin` có khớp chỗ nhãn thật sự nằm không:**
mỗi dòng `Origin='field'` có `K` **thật sự** là scalar field, và mỗi dòng `Origin='table:X'` có `K`
**thật sự** là cột của một bảng **tên đúng là X**. **0 vấn đề / 192.** Ba tên bảng thật:
`Thuốc điều trị` (6090, 2 cột) · `CHI PHÍ KHÁM CHỮA BỆNH` (6091, 13) ·
`Bảng kê chi phí khám chữa bệnh` (6092, 13). Tổng 28 dòng table-origin, 164 dòng field.

**Dự đoán fixture 21408 không đổi:** `MandatoryTotal 8` · `WithValue 8` ·
`Completeness 1.0000` · `ClaimConfidence 0.5544`. `MandatoryTotal` nếu filter template chết = **35**.

### Sửa lại một con số tôi báo sai lượt trước

Tôi nói "3 dòng mandatory của 6091 không bao giờ khớp". **Sai — là 5.**
Tôi chỉ đếm những dòng có **nhãn sai**; còn `Nội dung`→`LINE_ITEM_NAME` và `Số lượng`→`LINE_QTY`
cũng mandatory và cũng là **cột bảng**, nhãn đúng nên không hiện trong danh sách lệch.
⇒ Trần thật của `Completeness` cho 6091 hôm nay là **9/14 = 0,643**, không phải 0,786.

### `IdpFieldMap` chưa có chỗ chứa `Origin`

Attribute hiện tại (đọc máy rev 185): `Id` `IdpTemplateId` `K` `FieldCode` `TargetEntity`
`DataType` `IsMandatory` `IsActive` `Version`. **Không có `Origin`.**
⇒ "miễn là phần map dùng nó" hiện **chưa dùng được**. Cần: attribute `Origin` + input cho
`Seed_IdpFieldMap_Upsert` + nhánh trong parser.

### Quyết định phạm vi: lưu `Origin` ngay, rẽ nhánh bảng SAU

**SỬA LẠI LÝ DO HOÃN (PO, 30/09). Lý do tôi ghi trước đó là SAI.**

Tôi viết "chưa biết shape bảng nên hoãn". Sai — **shape đã biết** từ hai capture trên đĩa.

**Lý do thật: ODC không biểu diễn được List của List.** Một bảng = danh sách dòng, mỗi dòng =
danh sách ô ⇒ không dựng được trong ODC. Nên nhánh bảng **KHÔNG BAO GIỜ đi qua parser ODC** —
nó chờ **library `IdpTables`** (SRD đã có).

Hệ quả bắt buộc:
- `IdpWireData.tables` **giữ `Text List` và KHÔNG BAO GIỜ được dùng**.
- **ĐỪNG thiết kế `IdpWireTable`** — đường đó **đã đóng**, không phải "chờ dữ liệu".
- Thứ duy nhất còn thiếu là **một bảng kê có dòng chi phí thật**, để kiểm phần **ép kiểu tiền**
  trong library — không phải để biết shape.

**Nhưng `Origin` lưu ngay vẫn có lợi ích đo được:** cho phép loại dòng `Origin<>'field'` khỏi
`MandatoryTotal` tới khi nhánh bảng tồn tại. Hệ quả tính bằng máy:

| Template | mandatory | chỉ field | chỉ table | sau khi loại |
|---|---|---|---|---|
| 6091 | 14 | **9** | 5 | 14 → 9 |
| 6092 | 13 | **8** | 5 | 13 → 8 |
| 6093 | 8 | 8 | 0 | **không đổi** |

**Không template nào về 0** ⇒ `FIELDMAP_MISSING` không bị nổ oan. `Completeness` thành trung thực
thay vì bị chặn trần. Và **phép thử 6093 của PO không đổi** (6093 có 0 dòng bảng).

> `IsMandatory` của 6092 là kế thừa cơ học, mỗi dòng có Note *"PO xác nhận"* — **chưa chốt**,
> theo đúng lời PO. Không dùng nó làm chuẩn nghiệm thu.

### Việc cập nhật seed — nhỏ hơn dựng lại

| Batch | rows | thay đổi cần làm |
|---|---|---|
| A (6090+6091) | 53 | **9 sửa nhãn** + **15 thêm arg Origin** (2 của 6090, 13 của 6091) |
| B (6093+6094) | 44 | **không đổi** — toàn bộ `Origin='field'` |
| C (6095+6096) | 35 | **không đổi** |
| D (6097+6098) | 31 | **không đổi** |
| **E (6092)** | **29** | **mới** — 13 mandatory, 13 table-origin |
| | **192** | `ExpectedRows` **163 → 192** |

`Origin` làm input **không bắt buộc** của helper, mặc định `'field'` ⇒ chỉ **28** lời gọi cần
truyền thêm, 164 lời gọi còn lại giữ nguyên. Ít sửa, ít rủi ro Mentor làm hỏng chỗ đang đúng.

### Session Mentor chết lần thứ hai

`f214a501…` → *"Mentor couldn't restore the conversations in your session"*. Draft data-model
ReplayMode mất. Không mất gì có giá trị: rev vẫn **185**, và ReplayMode giờ là bước cuối.
**Hai lần trong hai ngày** ⇒ coi là chuyện thường xuyên. Đối phó: chia nhỏ, publish theo chặng.

---

## Thay đổi data model — ghi theo yêu cầu PO 30/09

**`IdpFieldMap` : 9 → 10 attribute.** build-01 §2 khai entity này **9 attribute**; nay thêm:

```
Origin : Text(100), mandatory, default "field"
```

"Nhãn này nằm ở đâu trong body IDP: `field` = một entry scalar trong `data.fields[]`;
`table:<tên bảng>` = một cột của bảng tên đó trong `data.tables[]`."

Verify bằng máy sau run `cf5de3de…`: entity có **đúng 10** attribute — `Id` `IdpTemplateId` `K`
`FieldCode` `TargetEntity` `DataType` `IsMandatory` `Version` `IsActive` **`Origin`**;
`FieldCode` **vẫn Text(50)**. `errorCount 7` · `warningCount 134` — bằng baseline rev 185.

Đây là thay đổi data model **thứ ba** trên app này: `IdpFieldMap.FieldCode` 20→50 (29/09),
`IdpFieldMap.Origin` mới (30/09), và `AgentRun.Replay` **chưa làm** (draft mất khi session chết).

> Delta nhỏ so với lệnh của tôi: tôi bảo "không sửa A/B/C/D", nhưng Mentor báo nó đã gán
> `Origin = ""` vào cả 163 call site để chúng dùng default. Hiệu quả **giống hệt**
> (`If(Origin="", "field", Origin)` ⇒ `"field"`), nhưng nó **có** chạm A–D. Ghi lại cho đúng.

---

## `LineItemsExtracted` — PO duyệt việc loại table-origin KÈM hai điều kiện không tách rời

**Điều kiện (a)** — thêm `LineItemsExtracted : Boolean` vào `IdpExtractionResult`.
`False` khi template **có** dòng mandatory gốc bảng mà nhánh bảng chưa chạy.
**KHÔNG nhét vào `ErrorCode`.**

**Điều kiện (b)** — `LineItemsExtracted = False` ⇒ hồ sơ **không đủ điều kiện tự duyệt**, với những
template có dòng mandatory gốc bảng.

**Lý do (PO):** 5 dòng bị loại của 6091 **chính là các cột tiền**. Loại mà không có cờ thì một hồ sơ
**chưa từng đọc bảng chi phí** sẽ báo `Completeness = 1.00` — biến "chưa nhìn" thành "đã đủ", và con
số đó **đi thẳng vào cổng tự duyệt**.

### ⚠️ (b) chưa có chỗ gắn — cổng tự duyệt KHÔNG TỒN TẠI

Kiểm bằng máy 30/09, kèm positive control theo rule 2:

- `context_search` objects=`["actions","structures"]`, query `Gate` → **0 kết quả**
- positive control: query `RunAdjudicationChain` → **21 kết quả**, tìm thấy action đó ⇒ **search hoạt động**, nên "không có" là kết luận có cơ sở
- `ChainRun.GateResultJson` **có tồn tại** — nhưng đó là **ô trống dành sẵn** để lưu kết quả cổng, không phải logic cổng
- Không có action nào tên/mô tả là auto-approve; `ClaimStatus` **có** `PendingApproval` và `Approved` nên máy trạng thái có chỗ, nhưng **không ai tự động quyết định**

⇒ **(b) không thể wire vào đâu lúc này.** Ghi thành **ràng buộc bắt buộc cho cổng khi dựng**, và
nói rõ (b) **CHƯA xong** — không tính là đã làm.

### ⚠️ Cờ sẽ bay hơi nếu không có chỗ lưu

`IdpExtractionResult` là **Structure** (trong bộ nhớ), không persist. `ClaimDocument` có
`Confidence` và `ErrorCode` nhưng **không có `Completeness`, không có `LineItemsExtracted`**
(đọc máy: 9 attribute — `UploadedOn` `RawExtractionJson` `Confidence` `IdpRequestId` `TemplateId`
`ErrorCode` `FileName` `ClaimId` `Id`).

⇒ Cờ trả về rồi **mất**, và cổng tương lai không đọc được nó. Cần thêm
`ClaimDocument.LineItemsExtracted : Boolean` (và có lẽ cả `Completeness`) — **thay đổi data model
thứ tư**, chưa làm, chưa xin phép. Nêu ra chứ không tự làm.

---

## Thay đổi data model thứ tư — PO duyệt 30/09

```
ClaimDocument.Completeness        : Decimal
ClaimDocument.LineItemsExtracted : Boolean
```

**Vì sao `Completeness` quan trọng hơn cả cái cờ (PO):** `ClaimDocument` có `Confidence` nhưng
**không có** `Completeness` ⇒ **một trong hai số nuôi điều kiện G2 đang bị tính rồi vứt đi**.
Khoảng trống này **có từ trước**, không phải do thay đổi này sinh ra.

`ClaimDocument` trước: 9 attribute — `Id` `ClaimId` `FileName` `UploadedOn` `TemplateId`
`IdpRequestId` `Confidence` `ErrorCode` `RawExtractionJson`. Sau: **11**.

### ❗ CHƯA QUYẾT — ghi ra để không ai lặng lẽ tự chọn

Cả `Completeness` và `LineItemsExtracted` ở **hạt CHỨNG TỪ** (`ClaimDocument`), còn cổng quyết định
ở **hạt HỒ SƠ** (`Claim`). **Cách gộp lên cấp hồ sơ chưa ai trả lời:**

- `min` trên mọi chứng từ?
- trung bình có **trọng số**? (trọng số theo gì — số field? số tiền?)
- hay **bắt buộc mọi chứng từ đều đạt**?

Ba cách cho ba kết quả khác nhau trên cùng một hồ sơ. **Phải quyết TRƯỚC khi dựng cổng.**
Không được để người dựng cổng tự chọn ngầm.

---

## ❗ Bẫy `Origin = ""` — phải KIỂM, không được coi là xong

PO chỉ ra: **default của Entity chỉ áp dụng khi attribute KHÔNG được set.** Gán `""` tường minh thì
ghi `""`, **không** thành `"field"`.

Mentor báo nó đã gán `Origin = ""` vào **cả 163 call site** của A–D. Chuỗi an toàn phụ thuộc **hoàn
toàn** vào việc helper có gộp `"" → "field"` đúng như tôi yêu cầu
(`If(Origin = "", "field", Origin)`, **cả nhánh create và nhánh update**). Mentor **báo** là có,
nhưng `context_graph` chết nên **tôi chưa đọc được bằng máy**.

**Nếu helper không gộp:** 164 dòng mang `Origin = ""` ⇒ filter `Origin = "field"` khớp **0 dòng** ⇒
**`MandatoryTotal = 0` ở MỌI template** ⇒ `FIELDMAP_MISSING` nổ khắp nơi và `Completeness` = 0 vĩnh
viễn. Hỏng lặng lẽ, và hỏng theo hướng trông giống "chưa seed".

### Nên đếm 192 là KHÔNG ĐỦ. Phải kiểm phân bố Origin

Kỳ vọng (tính từ CSV, khớp đúng con số PO đưa):

| Origin | số dòng |
|---|---|
| `field` | **164** |
| `table:CHI PHÍ KHÁM CHỮA BỆNH` | **13** |
| `table:Bảng kê chi phí khám chữa bệnh` | **13** |
| `table:Thuốc điều trị` | **2** |
| **OTHER / rỗng** | **0** |

**Cách quan sát:** harness không xem được row của entity, nên tôi thêm vào `Seed_IdpFieldMap` bốn
aggregate đếm theo đúng bốn giá trị trên, cộng `OTHER = ActualRows − tổng bốn cái`. `OTHER > 0`
**chính là** cái bắt được ca `""`. Đưa cả breakdown vào `Summary` của nút seed ⇒ một lần bấm là thấy.

---

## Điều kiện (b) — trạng thái CHƯA XONG, ràng buộc bắt buộc cho cổng

`LineItemsExtracted = False` ⇒ hồ sơ **không đủ điều kiện tự duyệt**, với những template có dòng
mandatory gốc bảng.

**Chưa gắn được**: cổng tự duyệt không tồn tại (kiểm 30/09 kèm positive control —
query `Gate` 0 kết quả, control `RunAdjudicationChain` 21 kết quả nên search hoạt động;
`ChainRun.GateResultJson` chỉ là ô trống dành sẵn). **Không tự dựng cổng** (PO).
Ghi ở đây là **ràng buộc bắt buộc**, trạng thái **CHƯA XONG**.

### Batch A — run `cb64eab7…` ✅ · 24 thay đổi · `errorCount 7` · `warningCount 134`

**9 nhãn 6091 đã hoàn nguyên theo definition:**
`LINE_UNIT_PRICE_HOSP → Đơn giá BV` · `LINE_UNIT_PRICE_INS → Đơn giá BH` ·
`LINE_SERVICE_RATE → Tỷ lệ thanh toán theo dịch vụ` · `LINE_AMOUNT_HOSP → Thành tiền BV` ·
`LINE_AMOUNT_INS → Thành tiền BH` · `LINE_PAID_BY_SI → Quỹ BHYT` ·
`LINE_PATIENT_COPAY → Người bệnh cùng chi trả` · `LINE_OTHER_SOURCE → Khác` ·
`LINE_PATIENT_SELF_PAID → Người bệnh tự trả`

`LINE_SI_RATE` (`Tỷ lệ thanh toán BHYT`) **không bị chạm** — đúng, nhãn đó vốn đã đúng.

**15 call truyền `Origin`:** 6090 ×2 (`LINE_ITEM_NAME`, `LINE_QTY`) → `table:Thuốc điều trị`;
6091 ×13 → `table:CHI PHÍ KHÁM CHỮA BỆNH`. Hai cặp `LINE_ITEM_NAME`/`LINE_QTY` ở hai template
nhận **hai giá trị khác nhau** — đúng, không bị gộp.

`Seed_IdpFieldMap_A` vẫn **53** call, mandatory vẫn **14**.

### ✅ Bẫy `Origin = ""` — hai biểu thức trích từ model

| Nhánh | Node | Biểu thức ghi cột `Origin` |
|---|---|---|
| Create | `PrepareCreate` | `If(Origin = "", "field", Origin)` |
| Update | `SetUpdateFields` | `If(Origin = "", "field", Origin)` |

**Cả hai nhánh đều có coalesce** ⇒ 163 call truyền `""` sẽ ghi `"field"`, không ghi `""`.
Bẫy đã đóng ở phía helper.

> **Nhưng đây vẫn là Mentor đọc-và-trích, không phải tôi đọc bằng máy** (`context_graph` chết).
> Theo rule 1, bằng chứng ràng buộc là **phân bố Origin sau khi seed** — đúng phép kiểm PO yêu cầu.
> Biểu thức đúng là điều kiện cần; `field 164 / OTHER 0` mới là điều kiện đủ.

### Batch E — run `07c7139a…` ✅

`Seed_IdpFieldMap_E`: **29** call · **13** truyền `Origin` tường minh
= `table:Bảng kê chi phí khám chữa bệnh` (**sentence case**, khác `CHI PHÍ KHÁM CHỮA BỆNH`
upper case của 6091) · 16 dòng scalar truyền `""` để helper coalesce về `"field"` ·
**13** mandatory. `errorCount 7` · `warningCount 134`.
Trong lúc chạy có 2 flow error tạm, Mentor tự sửa xong.

13 nhãn cột của 6092 **giữ nguyên** hậu tố `(đồng)` và `theo DV` — **không** bị "sửa cho giống"
9 nhãn 6091 vừa đổi ở batch A. Đây là bẫy dễ mắc nhất của batch này và đã tránh được.

### Publish rev 186 — checkpoint bảo vệ draft

Ba run (`cf5de3de` scaffold · `cb64eab7` batch A · `07c7139a` batch E) nằm draft trên rev 185.
Session Mentor đã chết **hai lần** trong hai ngày ⇒ publish tại điểm trọn vẹn là cách đối phó đã
ghi. `publicationKey 1a3fc863…`, revision **186**.

**Publish này an toàn với Stage 0:** `IdpFieldMap` vẫn **0 dòng** (publish chỉ đưa code lên, seed chỉ
chạy khi bấm nút) ⇒ `MandatoryTotal = 0` bất kể filter ⇒ `FIELDMAP_MISSING` vẫn nổ cho 6093 và
bằng chứng Stage 0 (a) vẫn quan sát được. Filter `Origin = "field"` **không** đổi kết quả Stage 0.

### Ba việc cuối trước seed — run `947f37d7…` · publish **rev 187**

`errorCount 7` · `warningCount 134` = baseline rev 186. Trong lúc chạy có một lần wiring output
parameter không ăn, Mentor tự sửa.

**1. `ClaimDocument` → 11 attribute**
`Completeness : Decimal` (không mandatory) · `LineItemsExtracted : Boolean` mandatory default `False`.
9 attribute cũ không bị đổi tên/type.

**2. `IdpExtractionResult` + `LineItemsExtracted : Boolean`** — `ErrorCode` **không bị chạm**,
vẫn Text(50) và vẫn chỉ dành cho `PARSE_FAILED` / `FIELDMAP_MISSING` / `TEMPLATE_NOT_ASSIGNED` /
mã IDP. Đúng yêu cầu "đừng nhét vào ErrorCode".

**`GetMandatoryCount` — 4 filter sau khi sửa:**
```
IsMandatory   = True
IsActive      = True
IdpTemplateId = TextToLongInteger(Candidate.templates_data.Current.templateId)
Origin        = "field"          <- MỚI
```

**`GetMandatoryTableCount` — mới:** ba filter đầu y hệt, cộng `Origin <> "field"`.

**Cờ sticky — đúng yêu cầu không reset:**
- Khởi tạo `True` ở assign node **`Init`**, node đầu tiên sau Start, chạy **một lần mỗi lời gọi parser**.
- Đặt `False` ở assign node **`ConfInit`** trong loop template, ngay sau `GetMandatoryTableCount`:
  ```
  If(LongIntegerToInteger(GetMandatoryTableCount.List.Current.Count) > 0,
     False,
     Result.LineItemsExtracted)
  ```
  Nhánh else **giữ nguyên giá trị hiện tại** ⇒ một khi False thì **False mãi**, template sau không
  kéo lại thành True. Đúng: một result phủ nhiều template thì **bất kỳ** template thiếu dòng bảng
  cũng làm cả result không line-item-complete.

**3. Phân bố Origin** — 4 aggregate + 5 output trên `Seed_IdpFieldMap`
(`OriginFieldRows` `OriginTableKcbRows` `OriginTableBangKeRows` `OriginTableThuocRows`
`OriginOtherRows`), chuyển tiếp qua `RunSeedIdpFieldMapNow` và vào `Summary`.
`OriginOtherRows <> 0` ⇒ in cảnh báo `ORIGIN MISMATCH`. Giữ `ExpectedRows 192` và `ActualRows`.

---

## Trạng thái rev 187 — sẵn sàng cho Stage 0 rồi seed

| Kỳ vọng sau khi bấm seed | Giá trị |
|---|---|
| `ActualRows` / `ExpectedRows` | **192 / 192** |
| `field` | **164** |
| `kcb` (CHI PHÍ KHÁM CHỮA BỆNH) | **13** |
| `bangke` (Bảng kê chi phí khám chữa bệnh) | **13** |
| `thuoc` (Thuốc điều trị) | **2** |
| **`other`** | **0** ← `> 0` là bẫy `Origin = ""` |

| Kỳ vọng harness trên fixture 21408 (6093) sau seed | Giá trị |
|---|---|
| `MandatoryTotal` | **8** (không phải 35) |
| `ClaimConfidence` | **0.5544** (min c, ICD10) |
| `Completeness` | **1.00** |
| `LineItemsExtracted` | **True** (6093 có 0 dòng bảng) |
| `ErrorCode` | **rỗng** |

Tất cả tính từ đĩa, không đoán.
