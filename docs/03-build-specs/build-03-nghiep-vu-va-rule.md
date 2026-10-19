# Build 03 — Nghiệp vụ chứng từ & Rule

| Mục | Giá trị |
|---|---|
| Tài liệu | Build 03 — Nghiệp vụ chứng từ & Rule cho "Agentic Health Claim Adjudication" trên OutSystems ODC + Agent Workbench |
| Đội | BnK Solution |
| Mốc | Hackathon 07–08/10/2026 |
| **Chuẩn gốc** | **Claim MVP** — A4 v1.3 + D2 v0.6 + D1 v0.5. **KHÔNG phải MBAL-IDP.** |
| Phiên bản | **v2.0 — ghi đè hoàn toàn v1.0.** v1.0 viết theo MBAL-IDP (4 `claim_type` `MEDICAL_*`, 10 `document_type` `CL05…CLM_04`, 40 templateId, Docset Map, rule R1–R5 của MBAL). **Toàn bộ phần đó đã bị bỏ.** Phần MBAL còn lại duy nhất nằm ở Phụ lục, gắn nhãn "chỉ tham chiếu" |
| Nguồn verbatim | `A4_Document_Type_and_Field_Catalogue_v1.3.md` (§1, §2, §3, §4, §5, §6, §7) — **nguồn chính** · `D2_Functional_Specification_v0.6.md` (§4.5, §4.6, §4.11, §5.28, §5.29, §5.9.1a) · `D1_BRD_v0.5.md` (BR-DOC, BR-LINE, BR-WAIT, BR-TREE, BR-PAY, BR-DUP, BR-TERM, BR-CHAIN) |
| Doc anh em | `build-01-data-model-va-enum.md` v2.0 (entity, enum) — tài liệu này là phần **nghiệp vụ và rule** của cùng một chuẩn |

> **Quy ước trích dẫn.** Mọi bảng field và mọi rule dưới đây là **trích nguyên văn** từ A4/D2/D1, giữ nguyên tên VI và EN, giữ nguyên mã field. Đây là tài liệu dùng để **seed cấu hình và viết rule**, nên một chữ sai là một dòng cấu hình sai. Chỗ nào tài liệu này diễn giải chứ không trích, có ghi rõ.

---

## 1. Mô hình phân loại

### 1.1 Ba tier — A4 §1.1 *(verbatim)*

| Tier | What IDP does | Types | Why |
|---|---|---|---|
| **Tier 1 — Extract** | Classify, then extract structured fields and tables | **4** (3 by OCR, 1 by XML) | These carry the money and the medical facts. Every number in the settlement comes from here |
| **Tier 2 — Classify only** | Recognise the document type; no field extraction | **4** *(3 to v1.2)* | **Confirms the insured event occurred and the required evidence exists.** No value is read from the page. Classification is far cheaper than extraction |
| **Tier 3 — Not handled** | Stored and attached; no AI | everything else | **Optional.** The customer is never required to upload a tier-3 document, and the claim is never held for one |

> **Hệ tham chiếu trong ngành phân khoảng 20 document class; MVP 1 xử lý 8 — 7 tới v1.2 cộng `T2-04` — và trong 8 loại đó chỉ 3 loại cần đọc ảnh.** (A4 §0)

### 1.2 Bảy loại chứng từ *(trích, cộng cột templateId của tenant 176)*

| Mã | Tên VI | Tên EN | Tier | IDP làm gì | templateId tenant 176 |
|---|---|---|---|---|---|
| **T1-01** | Bảng kê chi phí khám chữa bệnh | Itemised cost statement | 1 | **Classify + extract header + bảng line-item** — OCR. *"The single most important document. Every payable cost line comes from it"* | **`5383`** |
| **T1-02** | Đơn thuốc | Prescription | 1 | Classify + extract header + bảng drug-line — OCR. *"Drug names, strengths and quantities — the input to the drug-versus-diagnosis decline rules"* | **`5299`** |
| **T1-03** | Phiếu khám bệnh (có chẩn đoán) | Outpatient examination record | 1 | Classify + extract 16 header field — OCR. *"Load-bearing twice over"* — chẩn đoán dẫn ra `conditionClass` (thời gian chờ) **và** là đầu vào nhánh lâm sàng | *(chưa có template riêng ở tenant 176 — dùng `5384`/`5127` Giấy ra viện làm gần đúng cho demo; xem §10)* |
| **T1-04** | Hóa đơn GTGT / hóa đơn bán hàng | VAT / sales invoice | 1 | **Parse XML, KHÔNG OCR.** Issuer, tax code, invoice identity, totals | *(không dùng template — schema validation)* |
| **T2-01** | Giấy tờ tùy thân (CCCD / Hộ chiếu / Giấy khai sinh) | Identity document | 2 | **Chỉ phân loại.** Không đọc field nào | — |
| **T2-02** | Kết quả cận lâm sàng | Laboratory and imaging result | 2 | **Chỉ phân loại.** Hỗ trợ decline *"test unrelated to diagnosis"* | `5340` *(xét nghiệm — chỉ để nhận diện)* |
| **T2-03** | Báo cáo y tế / Giấy chứng nhận phẫu thuật | Medical report / surgery certificate | 2 | **Chỉ phân loại** | — |
| **T2-04** | Biên bản / xác nhận về hoàn cảnh xảy ra sự cố *(tên VI tạm)* | Statement of the circumstances of an incident | 2 | **Chỉ phân loại. Mới ở A4 v1.3, tên và mức độ CHƯA CHỐT** | — |

> **⚠ `T2-04` là loại duy nhất trong danh mục tồn tại vì một ĐIỀU KHOẢN HỢP ĐỒNG đòi, chứ không vì một loại đợt điều trị nào đòi.** Một term class-E duy nhất của sản phẩm pilot (`BR-TERM-11`) yêu cầu biên bản công an hoặc xác nhận nhập viện trên hồ sơ tai nạn, và tới v1.3 **A4 chưa hề có loại này**, nên trường `documentType` ở D2 §5.29.2 đang trỏ tới một giá trị nằm ngoài chính enum của nó. **Chưa chốt (D1 `B-24`):** là biên bản công an, hay xác nhận nhập viện, hay **hai loại riêng**; tên VI và EN của từng loại; mặc định là `REQUIRED` hay `EXPECTED` trên hồ sơ tai nạn **ngoài** bất kỳ điều khoản nào; có cần bản gốc không. **Mức độ mặc định được CỐ Ý bỏ trống khỏi bảng §2** — một loại xuất hiện ở `REQUIRED` cho mọi hồ sơ tai nạn là một đòi hỏi khác hẳn một loại chỉ tới qua điều khoản. **Cho tới khi `B-24` có câu trả lời, `T2-04` chỉ tới được hồ sơ qua `BR-TERM-11`** và **không xuất hiện ở dòng loại đợt nào.**

### 1.3 Loại bị CỐ Ý loại trừ — A4 §1.4 *(verbatim, để đội không tự thêm lại)*

| Excluded | Reason |
|---|---|
| **Giấy yêu cầu bồi thường** (claim request letter) | **The mobile app replaces it.** Claimant, relationship, payout account and amount claimed are captured as form fields. There is no reason to OCR a document we generated the data for |
| **Giấy ra viện** (discharge form) | Inpatient episodes are handled with **manual entry of episode facts** in MVP 1 — see §1.6 |
| **Sổ khám bệnh** (examination book) | Handwritten; the hardest document in the set; its content duplicates the examination record |
| **Phiếu điều trị răng** (dental treatment record) | Dental claims use the cost statement and examination record like any other |
| **Biên bản tai nạn** (accident report) | Accident claims are in scope; the report is a Tier 3 attachment reviewed by the assessor |
| **Giấy chứng nhận bảo lãnh** and everything in the credit and death/disability sections | The corresponding claim types are out of MVP scope |

> **Hệ quả nội trú (A4 §1.6).** Bỏ Giấy ra viện khỏi Tier 1 nghĩa là **sự kiện nội trú do thẩm định viên nhập tay**, không trích xuất. Hồ sơ nội trú vẫn chạy trọn vẹn: bảng kê vẫn được trích xuất bình thường, cây quyền lợi vẫn áp trần tiền giường theo ngày và đếm 60 ngày, số tiền vẫn tính như thường. **Cái đổi là phép đo: `SC-01` (≥ 80 % field bắt buộc tự điền) và `SC-02` (độ chính xác) chỉ đo trên hồ sơ NGOẠI TRÚ.** Hồ sơ nội trú là một đường **được hỗ trợ**, không phải một đường **được tự động hóa**, trong MVP 1.

### 1.4 `lossType` — 12 giá trị *(verbatim D2 §4.5)*

| Pathology \ Place | Ngoại trú (outpatient) | Nội trú (inpatient) |
|---|---|---|
| **Bệnh** (illness) | `ILL_OP` | `ILL_IP` |
| **Nha khoa** (dental) | `DEN_OP` | `DEN_IP` |
| **Tai nạn** (accident) | `ACC_OP` | `ACC_IP` |
| **Thai sản** (maternity) | `MAT_OP` | `MAT_IP` |

Cộng **4 giá trị tử vong / thương tật vĩnh viễn**: *tử vong do bệnh hoặc thai sản* · *tử vong do tai nạn* · *thương tật vĩnh viễn do tai nạn* · *tổn thương sức khoẻ vĩnh viễn do bệnh*.

> **App render 8; console và counter-intake render 12.** `V-401`: *"Loss type is one of the eight values the app offers, and the four death and permanent-disability values of `enum(lossType)` are **not selectable on the mobile surface**. The server enforces the surface restriction as well as the set, because a client that offered the other four would produce a claim whose checklist cannot be satisfied."*
>
> **Màn hình render MỘT control, không phải hai** — 8 nhãn tiếng Việt ghép sẵn (*Bệnh (ngoại trú)*, *Bệnh (nội trú)*, …), *"because a customer choosing two dropdowns can produce a combination and then change one half of it, and the app would have to explain the result. **The model holds two dimensions; the control offers eight rows.**"*

### 1.5 `episodeType` — 5 giá trị, là PHÉP CHIẾU chứ không phải field nhập

`OUTPATIENT` · `INPATIENT` · `DENTAL` · `ACCIDENT` · `MATERNITY`

Phép chiếu *(verbatim D2 §5.7.2)*: `ILL_*` → `OUTPATIENT`/`INPATIENT` · `DEN_*` → `DENTAL` · `ACC_*` → `ACCIDENT` · `MAT_*` → `MATERNITY`.

> **Chiều chạy là một chiều và cố định: `lossType` được NHẬP, `episodeType` được SUY RA, và code nào cho phép hai cái lệch nhau là code có lỗi.** `episodeType` sống sót vì **A5 §8.1 map quyền lợi trên nó** và A5 không đổi. **Phép chiếu mất mát theo một chiều** — 8 chiếu xuống 5 được, 5 không chiếu ngược lên 8 được: *nha khoa nội trú* và *thai sản nội trú* **không có biểu diễn nào trong bộ 5 giá trị**. *"A platform that stored the five and derived the eight would be storing the answer it can compute from and deriving the one it cannot."*

### 1.6 `conditionClass` — 6 giá trị *(verbatim D2 §5.7.2 F8 và Part III)*

**Đúng sáu**: `PRE` *Bệnh có sẵn* · `SPE` *Bệnh đặc biệt* · `GEN` *Bệnh thông thường* · `BIR` *Sinh đẻ* · `MAT` *Biến chứng thai sản* · `ACC` *Tai nạn*.
**Không bao giờ** là `SPECIAL`, `MATERNITY` hay `OTHER`.

**Nó BỊ RÀNG BUỘC bởi `lossType`, KHÔNG được suy ra từ `lossType`** *(verbatim)*:

> `ACC_*` admits only `ACC`; `MAT_*` admits `BIR` or `MAT`; `ILL_*` and `DEN_*` admit `PRE`, `SPE` or `GEN`. **The assessor still chooses within the admitted set** — that is a clinical judgement and F18 does not make it.

Lý do app chỉ xác định được 8 giá trị còn màn hình thẩm định 12: **app không bao giờ bắt khách phân biệt sinh đẻ với biến chứng thai sản** — 270 ngày chờ so với 90 ngày — nên app thu `MAT_OP`/`MAT_IP` và thẩm định viên tách `BIR` khỏi `MAT`. `conditionClass` được xác định lúc thẩm định và lưu trên sự kiện (`BR-WAIT-3`).

---

## 2. Checklist chứng từ theo `lossType`

### 2.1 Bảng 8+1 dòng — *(TRÍCH NGUYÊN VĂN D2 §4.6, nguồn seed duy nhất)*

| Loss type (S11 F3) | Bắt buộc (`REQUIRED`) | Nên có (`EXPECTED`) | Tùy chọn (`OPTIONAL`) |
|---|---|---|---|
| `ILL_OP` — Bệnh, ngoại trú | T1-01, T1-03 | T1-04, **T1-02** | T2-01, T2-02, T2-03 |
| `ILL_IP` — Bệnh, nội trú | T1-01 | T1-04, T2-03 | T1-02, T2-01, T2-02 |
| `DEN_OP` — Nha khoa, ngoại trú | T1-01, T1-03 | T1-04 | T2-01 |
| `DEN_IP` — Nha khoa, nội trú | T1-01 | T1-04, T2-03 | T2-01, T2-02 |
| `ACC_OP` — Tai nạn, ngoại trú | T1-01, T1-03 | T1-04, T2-02 | T1-02, T2-01 |
| `ACC_IP` — Tai nạn, nội trú | T1-01 | T1-04, T2-02, T2-03 | T1-02, T2-01 |
| `MAT_OP` — Thai sản, ngoại trú | T1-01, T1-03 | T1-04 | T1-02, T2-01, T2-03 |
| `MAT_IP` — Thai sản, nội trú | T1-01 | T1-04, T2-03 | T1-02, T2-01, T2-02 |
| **Any, where S11 F11 ≠ `SELF`** | **the standing document of §3.2.10** | — | — |

**Ba mức, không phải hai** *(verbatim A4 §6)*: *Required* chặn quyết toán. *Expected* kích một yêu cầu bổ sung tới khách, thẩm định viên có thể miễn. *Optional* không bao giờ được hỏi và không bao giờ chặn — khách tải lên chỉ khi họ muốn.

**Dòng tư cách là `REQUIRED` và là dòng duy nhất KHÔNG do `lossType` sinh ra.** `V-118` đặt nó ở đó; khách thấy nó được nêu tên trên S11 trước khi tới màn chụp.

### 2.2 Bốn quy tắc phải seed cùng bảng

| # | Quy tắc | Nguồn verbatim |
|---|---|---|
| **1** | **"Giấy tờ khác" KHÔNG BAO GIỜ thỏa một dòng `REQUIRED`** | `V-405`: *"A page in the other-documents group never satisfies a `REQUIRED` checklist row, **before or after classification**, until classification has actually moved it into that row."* D2 §4.6: *"**It never satisfies a `REQUIRED` row.** Gửi hồ sơ still requires a page in every `REQUIRED` row, and a customer who puts their cost statement in the other-documents group has not supplied a cost statement until classification says so. **A group that could satisfy a requirement would be a way around the checklist.**"* Báo cáo là **một dòng checklist chưa thỏa**, không phải một lỗi file |
| **2** | **`T1-04` CHỈ nhận XML, không có đường OCR** | D2 §4.6: *"`T1-04` VAT invoice accepts XML only (A4 §1.5, §2.7). The row's capture guidance says so and **the file picker filters to `text/xml`**. There is no OCR path for invoice images, and **a photographed invoice is not accepted here — it would silently produce a document nothing reads**."* Không có XML thì khách vẫn đi tiếp: hóa đơn là `EXPECTED` ở mọi loại đợt, và **rule R6 bị bỏ qua kèm cảnh báo chứ không làm hồ sơ kẹt** |
| **3** | **Hồ sơ nội trú CHỈ `T1-01` là `REQUIRED`** | D2 §4.6: *"**Inpatient claims capture `T1-01` only as `REQUIRED`.** A4 §1.6: the discharge form is out of Tier 1 and inpatient episode facts are keyed by an assessor. **The screen should not ask the customer for a document nothing will read.**"* Đúng với cả 4 dòng `*_IP` |
| **4** | **Không chứng từ Tier 3 nào xuất hiện ở cột `REQUIRED` hay `EXPECTED`** | A4 §6: *"No tier-3 document appears in any Required or Expected column, **which is what makes tier 3 genuinely optional rather than nominally optional**."* |

### 2.3 Hai sự kiện cấu trúc mà bảng 5 giá trị cũ không thể hiện được *(verbatim D2 §4.6)*

1. ***Nha khoa* và *thai sản* CÓ dạng nội trú**, và checklist của chúng khác dạng ngoại trú đúng theo cách mọi bệnh lý khác khác — **`T1-03` phiếu khám rơi ra và `T2-03` báo cáo y tế đi vào, vì một bệnh nhân nhập viện ra viện với một báo cáo chứ không phải một phiếu khám** (A4 §1.6).
2. ***Ngoại trú có thuốc* KHÔNG PHẢI một `lossType`.** Nó là một dòng trong bảng v0.2, đặt tên cho một tổ hợp bệnh-lý-và-điều-trị mà **khách không thể biết lúc chụp**. Nó đã bị bỏ; `T1-02` là `EXPECTED` trên `ILL_OP` và việc đối chiếu quyết định **về sau** liệu có nên tồn tại một đơn thuốc hay không.

> **Vì sao khóa cũ là khóa VÒNG TRÒN** *(D2 §5.5.1, verbatim)*: *"A4 §6's table offers *outpatient — illness* and *outpatient — with medication* as two rows of one five-value key, and **which of the two applied depended on whether a prescription had been attached — while the checklist is the thing that tells the customer to attach one.** That is circular… an assessor opening a claim with no prescription, and an assessor opening the same claim ten minutes after one arrived, generated two different checklists from one unchanged event."*

### 2.4 `T1-02` là `EXPECTED` trên `ILL_OP` — và đó là một quyết định, không phải một mặc định

D1 `B-02` (17/08/2026) chuyển `T1-02` từ *Optional* lên *Expected* cho ngoại trú-bệnh. Lý do *(verbatim)*: *"The clinical appropriateness track (D1 §5.10) reasons about drugs against the diagnosis, and **it has nothing to work with when the prescription is absent**. *Expected* asks for it without stalling a visit that legitimately did not produce one — which *Required* would do."* Màn hình **phải hỏi** và **phải cho khách đi tiếp** khi không có. `REQUIRED` sẽ làm kẹt; `OPTIONAL` sẽ không bao giờ hỏi.

---

## 3. Danh mục field theo từng loại chứng từ

### 3.0 Quy ước chung — A4 §2 *(verbatim, áp cho MỌI loại)*

| Convention | Rule |
|---|---|
| **Field status** | `M` mandatory — extraction fails for that document if absent · `O` optional · `D` derived by IDP, not read from the page |
| **Missing value** | Written as `null`, never as an empty string and never guessed. A mandatory field that is absent raises a completeness warning naming the field |
| **Never infer** | IDP does not compute a value that is not on the page. Totals are read, then checked against the sum — a mismatch is a warning, not a correction |
| **Dates** | Normalised to `YYYY-MM-DD`. Source forms accepted: `dd/mm/yyyy`, `dd-mm-yyyy`, `ngày DD tháng MM năm YYYY` |
| **Amounts** | Integer VND, no separators, no decimals. Vietnamese sources use `.` as the thousands separator — `1.500.000` is 1500000, never 1.5 |
| **Text** | Preserved with diacritics exactly as printed. Case is not normalised |
| **Confidence** | Every extracted field carries a confidence in `[0,1]`. Table cells carry their own confidence; the row carries the minimum of its cells |
| **Provenance** | Every field carries the document id, page number and bounding box, so the review UI can jump to it |
| **Raw is immutable** | The extraction result is never overwritten by a correction. Corrections live on the claim; the pair stays comparable |

### 3.0a Chữ ký phân loại Tier 1 — A4 §3 *(verbatim)*

| ID | Required markers (any one) | Supporting markers (≥2 of) | Threshold | Layout cue |
|---|---|---|---|---|
| **T1-01** | `BẢNG KÊ CHI PHÍ`, `BẢNG KÊ CHI TIẾT CHI PHÍ`, `CHI PHÍ KHÁM CHỮA BỆNH` | `Đơn giá`, `Thành tiền`, `Số lượng`, `ĐVT`, `BHYT`, `Người bệnh tự trả`, `Tổng cộng` | 50% of supporting | A multi-column table occupying most of the page |
| **T1-02** | `ĐƠN THUỐC`, `PHIẾU LĨNH THUỐC` | `Liều dùng`, `Cách dùng`, `Hàm lượng`, `Bác sĩ kê đơn`, `Chẩn đoán`, `Số lượng` | 50% | Numbered drug lines with a dosage sentence beneath each |
| **T1-03** | `PHIẾU KHÁM BỆNH`, `PHIẾU KHÁM`, `SỔ KHÁM BỆNH` | `Lý do khám`, `Chẩn đoán`, `Khám lâm sàng`, `Hướng điều trị`, `Bác sĩ khám`, `ICD` | 50% | Form-style label/value pairs, few or no table rows |
| **T1-04** | XML root `HDon` / `TDLap` present | `MSTNBan`, `KHHDon`, `SHDon`, `NLap` | n/a — schema validation | Not applicable |

**Conflict rule** *(verbatim)*: *"Where two signatures both pass, the one with the higher match ratio wins, and the competing class is recorded on the document. **A cost statement and an invoice image on the same page is the known ambiguous case** — resolved in favour of the cost statement, because the invoice is not read from images at all in MVP 1."*

**Page splitting** *(verbatim)*: *"A single uploaded file may contain several documents. IDP splits by document boundary before classifying. **A multi-page cost statement stays one document; its line items concatenate in page order.**"*

**Trang không khớp gì cả → `unclassified`, KHÔNG BAO GIỜ bị ép vào class gần nhất.**

---

### 3.1 T1-01 — Bảng kê chi phí khám chữa bệnh

#### Header — 18 field *(TRÍCH NGUYÊN VĂN A4 §4.1)*

| # | Field (VI) | Field (EN) | Type | St | Validation / note |
|---|---|---|---|---|---|
| 1 | Tên cơ sở y tế | Provider name | text | M | Matched to the provider catalogue; unmatched → warning, not a block |
| 2 | Mã số thuế cơ sở y tế | Provider tax code | text(10\|13) | O | 10 or 13 digits; reconciles with the invoice XML (R3) |
| 3 | Địa chỉ cơ sở y tế | Provider address | text | O | |
| 4 | Số bảng kê | Statement number | text | O | |
| 5 | Ngày lập | Statement date | date | M | ≤ today |
| 6 | Họ tên bệnh nhân | Patient name | text | M | Reconciles across documents (R1) |
| 7 | Ngày sinh | Date of birth | date | O | |
| 8 | Giới tính | Sex | enum | O | `Nam` \| `Nữ` |
| 9 | Mã bệnh nhân | Patient code | text | O | Provider-internal identifier |
| 10 | Số thẻ BHYT | Social insurance card no. | text | O | Presence indicates BHYT applied |
| 11 | Khoa / phòng khám | Department | text | O | |
| 12 | Bác sĩ điều trị | Attending doctor | text | O | |
| 13 | Chẩn đoán | Diagnosis text | text | M | |
| 14 | Mã ICD-10 | ICD-10 code | text | O | Pattern `[A-Z][0-9]{2}(\.[0-9])?`; validated against the ICD-10 catalogue |
| 15 | Ngày khám / điều trị — từ | Treatment date from | date | M | |
| 16 | Ngày khám / điều trị — đến | Treatment date to | date | O | **`null` where the page does not carry it. Never defaulted to field 15 — corrected at v1.3** |
| 17 | Hình thức điều trị | Treatment setting | enum | **D** | `Ngoại trú` \| `Nội trú` — derived from markers; drives the benefit mapping |
| **18** | **Quốc gia của cơ sở y tế** | **Facility country** | **text / ISO-3166 alpha-2** | **O** | **New at v1.3.** The country in which treatment was given, at **document level** — not per line. Derived from the printed facility address where the address makes it derivable, **and `null` otherwise**, never inferred from the facility's name. `VN` is not a default: an unstated country is `null`. **This is `Q-IDP-05`** |

> **⚠ Field 16 KHÔNG ĐƯỢC mặc định, và đây là chỗ sửa trên trang này có thể làm mất tiền** *(verbatim A4 §4.1)*: *"A failed 'to' extraction on an inpatient stay **silently becomes one day**, so a fourteen-night admission is priced as a single day, the day limit passes trivially, and **nothing anywhere reports that a number was manufactured**. A default is indistinguishable from a reading once it is in the payload. **From v1.3: `null` on absence, with provenance recorded as absent, and the claim engine decides what to do about it** — which for an inpatient episode is to hold the step rather than pass it (D1 `BR-CHAIN-1`)."*
> D2 §5.7.2 `F17` suy ra **số ngày điều trị** từ khoảng 15–16, tính **bao gồm cả hai đầu**, và **bước 9 của chuỗi** đối chiếu số đó với trần ngày của quyền lợi. Trường hợp một lần khám được xử ở nơi nó thuộc về: một bảng kê có một ngày điều trị và một dòng **là một sự kiện một ngày, và đó là suy diễn CỦA ENGINE, ghi nhận như một suy diễn** — không phải suy diễn của lớp trích xuất, ghi nhận như một giá trị đã đọc.

> **⚠ Field 18 là một trong hai dữ kiện lớp điều khoản đòi mà danh mục này chưa mang.** Loại trừ **#10 — điều trị ngoài lãnh thổ Việt Nam** chọn trên nó và không trên gì khác. Field 3 là địa chỉ tự do và tùy chọn, **nên hôm nay không có dữ kiện nào đứng sau loại trừ đó cả**. **Cái bị cấm dứt khoát là ĐOÁN quốc gia từ TÊN cơ sở** — đúng thứ một người lập trình vớ lấy khi cần fallback: quy ước *never infer* của §2 cấm, và một phép đoán theo tên sẽ sinh checkpoint trên mọi nhà cung cấp có tên nghe như nước ngoài.

#### Line items — 17 cột *(TRÍCH NGUYÊN VĂN A4 §4.1)*

**Cột `Prov` là cột phải đọc kỹ.** Nó nói giá trị được kỳ vọng đến từ đâu, và nó tồn tại vì **sáu cột trong bảng này giờ được render trên lưới của thẩm định viên** (D2 §5.6.3) trong khi trước đó chúng hoặc vô hình hoặc không có. **Một cột có người đọc thì phải có câu trả lời cho *ai đã đặt giá trị này vào đây*.**

| Value | Meaning |
|---|---|
| **X** | **Expected from extraction.** IDP returns it; the assessor may correct it and the raw value is preserved |
| **X?** | **Expected from extraction — and not yet confirmed by the IDP team.** New at v1.2, and the console must say *"chưa xác nhận nguồn trích xuất"* at the field until §9's Q-IDP-01 to Q-IDP-04 are answered |
| **K** | **Assessor-keyed.** No extraction target exists and none is being asked for |
| **D** | **Derived** by IDP from the other columns |

| # | Column (VI) | Column (EN) | Type | St | **Prov** | Validation / note |
|---|---|---|---|---|---|---|
| L1 | STT | Line no. | int | O | X | |
| L2 | Tên dịch vụ / thuốc / vật tư | Item name | text | M | X | The text quoted in a decline rationale |
| L3 | Nhóm dịch vụ | Service group | text | O | X | Where the provider prints one |
| L4 | Đơn vị tính | Unit of measure | text | O | **X?** | **Consumed from v1.2** — rendered beside quantity on the assessor's grid (register **S-15**). Extracted since v1.0 and read by nothing until now, so **its accuracy has never been measured**. Free text as printed: *viên*, *lọ*, *lần*, *ngày*. **Not normalised to a controlled list** — a normalisation nobody validated would be worse than the printed string |
| L5 | Số lượng | Quantity | decimal | M | X | > 0 |
| L6 | Đơn giá | Unit price | int | M | **X?** | **Consumed from v1.2** (register **S-15**). Already mandatory and already checked against L5 × L7, so it has been *exercised*; what is new is that a person now reads it and may act on it. ≥ 0 |
| L7 | Thành tiền | Line amount | int | M | X | Checked against L5 × L6 with a ±1 VND rounding tolerance → warning on mismatch |
| L8 | BHYT chi trả | Paid by social insurance | int | O | X | Defaults 0. **Extracted in MVP 1 but not consumed by the benefit engine** — S-17. Keep extracting it: turning coordination on later must not require re-processing the ground-truth set |
| L9 | Người bệnh cùng chi trả | Patient co-payment (BHYT) | int | O | X | Defaults 0 |
| L10 | Người bệnh tự trả | Patient self-paid | int | O | X | Defaults 0 |
| L11 | Nguồn khác | **Other-payer share** | int | O | **X?** | **Consumed from v1.2** and **renamed for what it is** (register **S-15**). It is the amount borne by a payer who is neither social insurance nor the patient — a second private policy, an employer scheme, a charity. **It is not deducted** and MVP 1 has no contribution rule (Framework **S-20**), but it is now **shown to the assessor**, because a line where a third party already paid is the single clearest signal of the duplicate-cover exposure S-20 declares. **Displaying it costs nothing and is the only visibility we have** |
| L12 | Số đăng ký thuốc | Drug registration no. | text | O | **X?** | **Consumed from v1.2** (register **S-15**). Present on medication lines. It is also one of the three signals `serviceCategory` = `DRUG` is derived from (§4.5), so it has been *used* — what is new is that it is *displayed*, and a derivation input a person can see is a derivation a person can check |
| **L15** | **Ngày chứng từ** | **Document date (line level)** | date | **O** | **X?** | **New at v1.2** (register **S-15**). The date printed **against this line**, where the statement prints one per line rather than only in the header. **Not the same as header field 5** — a multi-visit statement carries one statement date and several service dates, and the line date is what allocates the line to the right insured event (D2 §5.6.3 L17, V-705). **Where the provider prints no per-line date, this is `null` and the allocation falls back to the header date** — it does **not** default to it, because a value that was inferred must not look like one that was read |
| **L16** | **Mã ICD-10 (theo dòng)** | **ICD-10 code (line level)** | text | **O** | **X?** | **New at v1.2** (register **S-16**). The diagnosis code printed against **this line**, where the statement carries one. Pattern as header field 14, validated against the ICD-10 catalogue. **Two things now depend on this field.** **(1) The clinical track**, which currently reasons a *"not related to the diagnosis"* recommendation against the **event's** diagnosis, so on a claim carrying two diagnoses it argues against the wrong one. **(2) Two contractual exclusions, added at v1.3** — A5 §7's **#5** and **#9**, the congenital-condition exclusion (which tests chapter Q of the ICD) and the infertility / assisted-reproduction / contraception / termination group. **Both are `DRAFT` today because this field does not exist** |
| **L18** | **Số ngày điều trị (theo dòng)** | **Treatment days (line level)** | int | **O** | **X?** | **New at v1.3.** The number of days **this cost line** covers, where the line is priced per day — a room charge, a bed charge, an ICU day. **`null` where the line is not day-priced or the page does not carry it**, never derived from `L5` quantity, which counts units of an unspecified thing. **This is `Q-IDP-06`.** Today the only day count in the system is **per insured event**, derived from header fields 15 and 16 (D2 §5.7.2 `F17`), and **chain step 9 tests a `DAY` counter that nothing populates per line** — which is how the limit panel came to render a *"số ngày còn lại"* figure that no rule produces. **`L17` is deliberately not used** |
| L13 | Danh mục dịch vụ | Service category | enum | **D** | D | **Derived by IDP.** The value the benefit mapping consumes — see §4 |
| L14 | Số tiền yêu cầu | Amount claimed | int | **D** | D | `L7 − L8`, floored at 0. The amount presented to the benefit engine |

> **L15, L16 và L18 giữ số ngoài thứ tự là CỐ Ý.** *"L13 and L14 are **derived** and have been cited by number in D2, A5 and the result payload since v1.0. **A field number is an address, not a position** — renumbering to make the table read tidily would silently re-point every existing citation."* `L17` bị cố ý bỏ trống vì lưới dòng chi phí ở D2 §5.6.3 đã dùng `L17` cho việc phân bổ dòng vào sự kiện.

> **Hệ tham chiếu có HAI cột ICD trên mỗi dòng; ta lấy một.** Cột thứ hai — *related diagnosis* do thẩm định viên đặt — **không được áp dụng**: *"The extracted code has a document behind it. The related diagnosis has nothing behind it but a keystroke on every line of every claim, and **it would be read by nothing**… A mandatory keystroke that feeds no decision is how a grid becomes something assessors click past."*

#### Footer — 3 field *(verbatim)*

`Tổng cộng` (total, **M**) · `Tổng BHYT chi trả` (O) · `Tổng người bệnh tự trả` (O).

#### Validation cấp tài liệu *(verbatim)*

| Code | Rule | Action |
|---|---|---|
| V-01-1 | Σ L7 over all lines = footer `Tổng cộng` (±1 VND) | Warning, anchored on the footer |
| V-01-2 | Σ (L8 + L9 + L10 + L11) = L7 per line | Warning on the line |
| V-01-3 | At least one line item present | **Error** — the document is unusable |
| V-01-4 | Treatment dates fall inside the member's coverage period | Warning; the claim engine decides |

**Khó khăn đã biết, cần IDP lượng hóa** *(verbatim)*: *"two-row grouped headers with a `Yêu cầu bồi thường` / `Bảo hiểm chi trả` span; provider-specific column ordering; statements that run to several pages with a repeated header; handwritten annotations in the margin; and drug lines whose names abbreviate the active ingredient."*

#### Field nào là "consumed" chứ không chỉ "extracted"

| Field | Trạng thái | Hệ quả cho build |
|---|---|---|
| `L4`, `L6`, `L11`, `L12` | **Trích xuất từ v1.0, KHÔNG AI TIÊU THỤ cho tới v1.2.** Từ v1.2 **được render trên lưới thẩm định viên** | *"A field that is extracted and consumed by nobody **has never had its accuracy measured**."* Bốn cột này hôm nay là **bốn con số độ chính xác không ai có**. `L6` và `L11` nằm cạnh tiền. Console phải hiện *"chưa xác nhận nguồn trích xuất"* tại field |
| `L8` | **Trích xuất nhưng KHÔNG được engine quyền lợi tiêu thụ** (S-17) | Vẫn trích. Bật phối hợp chi trả về sau **không được phép bắt xử lý lại bộ ground-truth** |
| `L14` (`amountClaimed`) | **Derived và LUÔN có mặt trên mọi dòng** | *"the benefit engine never has to recompute it"* |
| `L13` (`serviceCategory`) | **Derived bởi IDP** (giả định `I-2`), là giá trị map quyền lợi tiêu thụ | Xem §4.1 |
| Header 14, `MCCQT`, `DSCKS` | **Captured, nothing consumes them** | Bắt vì chúng tới miễn phí cùng phép parse |

---

### 3.2 T1-02 — Đơn thuốc (prescription)

**Chứng từ này KHÔNG MANG TIỀN.** *"Its sole purpose is to give the decline rules the drug identity, and to establish the prescriber's diagnosis. **Extraction accuracy on names matters more than on numbers.**"*

#### Header — 10 field *(TRÍCH NGUYÊN VĂN A4 §4.2)*

| # | Field (VI) | Field (EN) | Type | St | Note |
|---|---|---|---|---|---|
| 1 | Tên cơ sở y tế | Provider name | text | M | R3 |
| 2 | Số đơn thuốc | Prescription number | text | O | |
| 3 | Ngày kê đơn | Prescription date | date | M | Within the episode dates (R4) |
| 4 | Họ tên bệnh nhân | Patient name | text | M | R1 |
| 5 | Ngày sinh / Tuổi | DOB or age | text | O | |
| 6 | Giới tính | Sex | enum | O | |
| 7 | Cân nặng | Weight | text | O | Printed for paediatric prescriptions |
| 8 | Chẩn đoán | Diagnosis text | text | M | R8 |
| 9 | Mã ICD-10 | ICD-10 code | text | O | R8 |
| 10 | Bác sĩ kê đơn | Prescriber | text | O | |

#### Drug lines — 9 cột *(TRÍCH NGUYÊN VĂN)*

| # | Column (VI) | Column (EN) | Type | St | Note |
|---|---|---|---|---|---|
| D1 | STT | Line no. | int | O | |
| D2 | Tên thuốc | Drug name (brand) | text | M | Quoted verbatim in a decline rationale |
| D3 | Hoạt chất | Active ingredient | text | O | Where printed separately |
| D4 | Hàm lượng | Strength | text | O | |
| D5 | Đơn vị | Unit | text | O | |
| D6 | Số lượng | Quantity | decimal | M | |
| D7 | Liều dùng / cách dùng | Dosage instruction | text | O | Free text |
| D8 | Số đăng ký | Registration number | text | O | Links to the national drug catalogue when present |
| D9 | Phân loại | Item class | enum | **D** | `Thuốc` \| `Thực phẩm chức năng` \| `Vật tư` \| `Khác` — **derived; drives the supplement decline rule** |

**Validation** *(verbatim)*: `V-02-1` at least one drug line, else the document is unusable. `V-02-2` prescription date within the episode dates → warning.

**Consumed:** `D2` (tên thuốc) được **rule R7** đối chiếu với `L2` của bảng kê nơi `L13 = DRUG`, và được **trích nguyên văn vào diễn giải từ chối**. `D9` là **derived và dẫn ra rule từ chối thực phẩm chức năng** — tức là nó không chỉ được trích, nó **quyết định**.

---

### 3.3 T1-03 — Phiếu khám bệnh — 16 header field *(TRÍCH NGUYÊN VĂN A4 §4.3)*

| # | Field (VI) | Field (EN) | Type | St | Note |
|---|---|---|---|---|---|
| 1 | Tên cơ sở y tế | Provider name | text | M | R3 |
| 2 | Mã số thuế | Provider tax code | text | O | |
| 3 | Số phiếu | Record number | text | O | |
| 4 | Ngày khám | Examination date | date | M | The episode date if no other source |
| 5 | Họ tên bệnh nhân | Patient name | text | M | R1 |
| 6 | Ngày sinh | Date of birth | date | M | R2 |
| 7 | Giới tính | Sex | enum | O | |
| 8 | Địa chỉ | Address | text | O | |
| 9 | Mã bệnh nhân | Patient code | text | O | |
| 10 | Lý do khám | Reason for visit | text | O | |
| 11 | Khám lâm sàng | Clinical findings | text | O | |
| 12 | Chẩn đoán | Diagnosis text | text | M | The diagnosis of record for the episode |
| 13 | Mã ICD-10 | ICD-10 code | text | M | Validated against the catalogue. If absent on the page, taken from T1-01 field 14 with provenance recorded |
| 14 | Hướng điều trị | Treatment plan | text | O | |
| 15 | Hẹn tái khám | Follow-up date | date | O | |
| 16 | Bác sĩ khám | Examining doctor | text | O | |

**Consumed nặng nhất trong bộ:** field **12 và 13** là *"the diagnosis of record for the episode"* — chúng **dẫn ra `conditionClass`** (và qua đó dẫn ra thời gian chờ, `BR-WAIT-1`…`BR-WAIT-5`), và chúng là **đầu vào mà nhánh lâm sàng lập luận chống lại**. Field 13 là `M` và có **đường dự phòng duy nhất trong cả danh mục**: vắng trên trang thì lấy từ `T1-01` field 14, **và provenance được ghi lại** — đây là ngoại lệ duy nhất của quy tắc *never infer*, và nó được cho phép vì nó là một **phép sao chép có nguồn**, không phải một phép đoán.

---

### 3.4 T1-04 — Hóa đơn GTGT, từ e-invoice XML — 15 field *(TRÍCH NGUYÊN VĂN A4 §4.4)*

Parsed from the signed XML, not read from an image. Field names follow the Vietnamese e-invoice structure.

| # | XML element | Field (EN) | St | Note |
|---|---|---|---|---|
| 1 | `MSo` | Form number | O | |
| 2 | `KHHDon` | Invoice serial | M | |
| 3 | `SHDon` | Invoice number | M | |
| 4 | `NLap` | Issue date | M | |
| 5 | `NBan/Ten` | Seller name | M | R3 against the cost statement |
| 6 | `NBan/MST` | Seller tax code | M | R3 |
| 7 | `NBan/DChi` | Seller address | O | |
| 8 | `NMua/Ten` | Buyer name | O | Often the patient; may be blank |
| 9 | `NMua/MST` | Buyer tax code | O | |
| 10 | `HHDVu[]` | Line items — name, unit, quantity, unit price, amount | M | Compared with the cost statement (R6) |
| 11 | `TgTCThue` | Total before VAT | M | |
| 12 | `TgTThue` | VAT amount | M | |
| 13 | `TgTTTBSo` | Total after VAT | M | |
| 14 | `MCCQT` | Tax authority code | O | Captured because it arrives with the parse. **Not verified — no tax-authority integration in this project** |
| 15 | `DSCKS` | Digital signature block | O | Presence recorded; validity not checked |

**Validation** *(verbatim)*: `V-04-1` the XML validates against the published schema, else the document is rejected with a clear message. `V-04-2` `TgTCThue + TgTThue = TgTTTBSo` (±1) → warning.

**Ba hệ quả của quyết định chỉ-XML** *(verbatim A4 §1.5)*:

1. **Bảng kê, KHÔNG PHẢI hóa đơn, là nguồn của các dòng chi phí.** Điều này đã đúng trong thực tế: `bảng kê` mang chi tiết, hóa đơn mang header và tổng. **Việc xét duyệt do đó không dừng lại nếu XML vắng mặt.**
2. **Nếu không có XML**, hồ sơ đi tiếp trên bảng kê một mình, mục hóa đơn trong checklist được đánh dấu **đã nhận-nhưng-chưa-xác-minh**, và **rule R6 bị bỏ qua kèm cảnh báo**. Thẩm định viên có thể nhập header hóa đơn bằng tay. **Đây KHÔNG ĐƯỢC là một lỗi chặn.**
3. **Không có xác minh với cơ quan thuế trong dự án này.** Ngoài phạm vi, **và cũng không nằm trong kế hoạch MVP 2**. `MCCQT` và khối chữ ký được bắt vì chúng tới miễn phí cùng phép parse; **không gì tiêu thụ chúng**.

> **Xác nhận 17/08/2026 (D1 `B-08`): quyết định chỉ-XML ĐỨNG VỮNG**, bất kể tỷ lệ sẵn có hóa đơn XML hóa ra là bao nhiêu. **Cái nó đổi là bộ ground-truth: 10 XML hóa đơn ở A4 §10 trở thành KHÔNG THƯƠNG LƯỢNG**, lấy từ nhà cung cấp nào có phát hành nếu nhà cung cấp pilot không phát hành. **Một parser chưa được test mới là tổn thất thật.**

---

## 4. Derived field

**Bốn field derived, cộng một field mới ở v1.3.** Giả định `I-2` của A4 §9: **cả bốn được sinh BÊN TRONG IDP**, không phải bên engine, *"because they need the document context"*. **Đây là giả định, chưa được IDP xác nhận** — xem §10.

### 4.1 `serviceCategory` (`L13`) — 20 giá trị *(TRÍCH NGUYÊN VĂN A4 §4.5)*

Đây là field mà **phép map quyền lợi ở A5 §8.1 tiêu thụ**, nên nó **phải được sinh ra một cách xác định (deterministic) và phải cấu hình được**.

| Category | Recognised from | Outpatient | Inpatient |
|---|---|---|---|
| `CONSULT` | Tiền khám, Công khám, Khám bệnh | 200.1 | 100 |
| `LAB` | Xét nghiệm, Sinh hóa, Huyết học, Nước tiểu | 200.1 | 100 |
| `IMAGING` | Chụp, Siêu âm, X-quang, CT, MRI, Nội soi | 200.1 | 100 |
| `PROCEDURE` | Thủ thuật, Phẫu thuật, Tiểu phẫu | 200.1 | 100.2 |
| `DRUG` | A matching drug line, or `Số đăng ký` present, or the service group reads Thuốc | 200.2 | 100 |
| `SUPPLEMENT` | Matches the supplement / functional-food list | **200.2** | **100** |
| `PHYSIO` | Vật lý trị liệu, Phục hồi chức năng | 200.3 | 100 |
| `DENTAL` | Răng, Nha khoa, Trám, Nhổ | 300.1 | n/a |
| `ROOM` | Tiền giường, Giường bệnh, Tiền phòng | n/a | 100.1 |
| `ICU` | Hồi sức, Cấp cứu tích cực, ICU, Chăm sóc đặc biệt | n/a | 100.1.1 |
| `AMBULANCE` | Xe cứu thương, Vận chuyển cấp cứu | 100.4 | 100.4 |
| `DELIVERY_NORMAL` | Sinh thường, Đẻ thường | n/a | 400.1 |
| `DELIVERY_CS` | Sinh mổ, Mổ lấy thai | n/a | 400.2 |
| `MATERIAL` | Vật tư y tế, Vật tư tiêu hao | 200.1 | 100 |
| `ADMIN` | Phí hồ sơ, Phí dịch vụ, Phí photocopy, Phí đăng ký | **no mapping** | **no mapping** |
| **`SCREENING`** | **Khám sức khỏe, Tầm soát, Khám định kỳ** | **200.1** | **100** |
| **`VACCINATION`** | **Tiêm chủng, Vắc xin, Tiêm phòng** | **200.2** | **100** |
| **`OPTICAL`** | **Kính thuốc, Kính áp tròng, Đo khúc xạ, Gọng kính** | **200.1** | **100** |
| **`HEARING`** | **Máy trợ thính, Đo thính lực, Thiết bị trợ thính** | **200.1** | **100** |
| `OTHER` | Anything else | assessor decides | assessor decides |

*Twenty categories, identical to the mapping table in A5 §8.1.* **Where the episode loss type is accident, every mapped benefit is redirected to 500.**

> **⚠ `SUPPLEMENT` và `ADMIN` THÔI KHÔNG TỪ CHỐI nữa từ v1.3.** Cả hai đọc *declined `NOTCOV`* ở v1.1 và v1.2. **Bốn thứ sai với điều đó:** **(1) sai BƯỚC** — một quyết định từ chối về phạm vi là việc của bước 4 hoặc bước 6; §4.5 là một **phép map**, việc của nó là nói *một dòng được bảo hiểm rơi vào đâu*; **(2) sai MÃ** — `NOTCOV` nghĩa là *không phải một mục y tế được bảo hiểm*; một thực phẩm chức năng bị loại bởi câu chữ hợp đồng là bị loại bởi một **điều khoản**, và mã cho điều đó là `EXCL`; **(3) KHÔNG CÓ ĐIỀU KHOẢN** — một từ chối sinh ra ở đây không viện dẫn được gì, mà `BR-MSG-3` bắt buộc phải nói cho khách biết điều khoản nào được dựa vào; **(4) VÔ ĐIỀU KIỆN** — câu chữ loại trừ đọc là *"dietary supplements … **not prescribed as treatment for the diagnosed condition**"*, và §4.5 đang từ chối **mọi** dòng thực phẩm chức năng **kể cả dòng được bác sĩ kê**.
> **Thay bằng:** `SUPPLEMENT` map về `200.2` ngoại trú và `100` nội trú, và `TERM-HC-003` quyết định ở **bước 4** liệu loại trừ có cắn không, **raise một CHECKPOINT** vì mệnh đề *"prescribed as treatment"* không có dữ kiện nào đứng sau. **`ADMIN` map về KHÔNG GÌ CẢ**, và một dòng không map được rơi xuống **bước 6** và bị từ chối `NOTCOV` **ở đó** — đúng mã, đúng bước.

> **⚠ Bốn giá trị mới ở v1.3 KHÔNG MIỄN PHÍ cho IDP.** A5 ghi `SCREENING`, `VACCINATION`, `OPTICAL`, `HEARING` là *"Claim Platform master data, ours to define — no vendor question, no dependency, one seed change"*, và **với phép map quyền lợi thì đúng vậy**. **Nhưng `L13` do IDP suy diễn** — đó là status `D` và giả định `I-2` — nên **phải có ai đó dạy phép suy diễn bốn giá trị mới**, và người đó là IDP. **Là một change request dưới §11 và là `Q-IDP-08`.** Nếu câu trả lời là *không trong phase này*, bốn giá trị đó **do thẩm định viên nhập tay tại `L13`**, và hai loại trừ chúng mở khóa vẫn dừng ở checkpoint thay vì đạt class `D`.

**Ranh giới sở hữu, viết một lần** *(verbatim)*: *"**The category list and its keywords are reference data, editable at run time** (`REQ`: the service-category rule table must be maintained without a release). **The derivation that produces `L13` from a printed item name is not** — it is IDP's, and it changes by change request."*

### 4.2 `itemClass` (`D9` trên đơn thuốc) — 4 giá trị

`Thuốc` · `Thực phẩm chức năng` · `Vật tư` · `Khác`.
**Derived** từ tên thuốc và số đăng ký trên dòng đơn thuốc. **Dẫn ra rule từ chối thực phẩm chức năng.** Một trong ba field derived mà giả định `I-2` nói là sinh trong IDP.

### 4.3 `treatmentSetting` (header field 17) — 2 giá trị

`Ngoại trú` · `Nội trú`. **Derived from markers** trên trang bảng kê; **dẫn ra phép map quyền lợi** (cột nào của bảng §4.1 được dùng).

> **⚠ Nó là BẰNG CHỨNG ở cấp TÀI LIỆU, KHÔNG BAO GIỜ là một khóa thứ hai** *(D2 §5.6.2 F11, verbatim)*: *"it **proposes** the setting, it does not set it. **Where it disagrees with the loss type on an event this document's lines are allocated to, both are shown and the disagreement is flagged at the field** — never silently resolved."* Thẩm định viên có thể sửa. Chấp nhận đề xuất thì **suy lại `F18` (`lossType` của sự kiện), suy lại `episodeType` mà A5 §8.1 map trên, và LÀM MẤT HIỆU LỰC phép tính tiền.**

### 4.4 `facilityCountry` (header field 18) — mới v1.3, `Q-IDP-05`

**Quy tắc suy diễn, đầy đủ:**

- Suy từ **địa chỉ cơ sở y tế IN TRÊN TRANG** (field 3), **khi và chỉ khi địa chỉ đó cho phép suy ra**.
- **`null` trong mọi trường hợp khác.**
- **`VN` KHÔNG PHẢI mặc định.** Một quốc gia không được nêu là `null`, không phải `VN`.
- **CẤM đoán quốc gia từ TÊN cơ sở.** Quy tắc *never infer* của §2 cấm; một phép đoán theo tên sẽ raise checkpoint trên mọi nhà cung cấp có tên nghe như nước ngoài.
- Cấp **tài liệu**, không phải cấp dòng.
- Kiểu: `text` / ISO-3166 alpha-2. Status `O`.

**Ai tiêu thụ:** loại trừ **#10 — điều trị ngoài lãnh thổ Việt Nam** của sản phẩm pilot chọn trên nó **và không trên gì khác**. Loại trừ đó hôm nay ở trạng thái `DRAFT` **vì field này chưa tồn tại**.

### 4.5 Per-line treatment-day count (`L18`) — mới v1.3, `Q-IDP-06`

**Quy tắc suy diễn, đầy đủ:**

- Số ngày **dòng chi phí này** bao phủ, **khi dòng được tính giá theo ngày** — tiền phòng, tiền giường, ngày ICU.
- **`null` khi dòng không tính theo ngày, hoặc khi trang không mang con số đó.**
- **KHÔNG BAO GIỜ suy từ `L5` số lượng** — `L5` đếm đơn vị của một thứ không xác định.
- Kiểu `int`, status `O`.

**Vì sao nó cần tồn tại** *(verbatim)*: *"Today the only day count in the system is **per insured event**, derived from header fields 15 and 16 (D2 §5.7.2 `F17`), and **chain step 9 tests a `DAY` counter that nothing populates per line** — which is how the limit panel came to render a *'số ngày còn lại'* figure that no rule produces."*

**Liên quan `BR-LEDGER-12`** — rủi ro đếm đôi đã biết và phải xử lý khi build: *"A `DAY` counter is incremented **once per distinct calendar date per counter**, regardless of how many lines fall on that date. `BR-TREE-2`'s no-double-counting rule is written for **the same line** and does not reach this case: on an **intensive-care-to-ward transfer day** a claim carries a `100.1.1` line *and* a `100.1` line for the same date, and `100.1`'s counter takes **+1 directly and +1 rolled up under BR-TREE-1** — two days consumed for one day in hospital, against a 60-day limit."*

### 4.6 `amountClaimed` (`L14`)

`L7 − L8`, **sàn ở 0**. Là con số trình cho engine quyền lợi. **Luôn có mặt trên mọi dòng, để engine không bao giờ phải tính lại.**

---

## 5. Rule đối chiếu chéo R1–R9

Chạy **sau khi trích xuất, trên toàn bộ chứng từ trong bộ hồ sơ**. *(A4 §5, verbatim)*

> **Nothing is auto-corrected**; each rule produces a labelled outcome anchored at the field.

| ID | Rule | Compares | Tolerance | Action if it fails |
|---|---|---|---|---|
| **R1** | Patient name consistent | T1-01.6 · T1-02.4 · T1-03.5 | Diacritic- and case-insensitive; whitespace normalised | Warning on every document |
| **R2** | Date of birth consistent | T1-01.7 · T1-03.6 | Exact | Warning |
| **R3** | Provider consistent | T1-01.1/.2 · T1-02.1 · T1-03.1 · T1-04.5/.6 | Tax code exact when present, else name match | Warning |
| **R4** | Service dates within the episode | T1-01.15/.16 · T1-02.3 · T1-03.4 | Same calendar day or within the range | Warning |
| **R5** | Cost statement arithmetic | Σ lines vs footer | ±1 VND | Warning on the footer |
| **R6** | Invoice reconciles with the cost statement | T1-04.13 vs T1-01 footer, allowing the BHYT split | ±1 VND | Warning. **Skipped with a note when no XML was supplied** |
| **R7** | Prescription drugs appear on the cost statement | T1-02 D2 vs T1-01 L2 where L13 = `DRUG` | Fuzzy name match | Warning naming the unmatched drugs |
| **R8** | Diagnosis consistent | T1-01.13/.14 · T1-02.8/.9 · T1-03.12/.13 | ICD exact where both present; else text similarity | Warning |
| **R9** | Package completeness | Required types present for the episode type | — | Sets the completeness state — see §6 |

### 5.1 Mức độ — đọc bảng trên cho đúng

**Tám trên chín rule chỉ sinh `Warning`.** Không có rule nào trong R1–R8 sinh `Fail` và không rule nào tự sửa gì. Chỉ có **R9 sinh một TRẠNG THÁI** chứ không sinh cảnh báo.

`Fail`/`Error` chỉ tồn tại ở **validation cấp tài liệu**, không ở lớp đối chiếu chéo:

| Ở đâu | Mức | Cái gì |
|---|---|---|
| `V-01-3` | **Error — tài liệu không dùng được** | Bảng kê không có dòng nào |
| `V-02-1` | **Error — tài liệu không dùng được** | Đơn thuốc không có dòng thuốc nào |
| `V-04-1` | **Rejected kèm thông điệp rõ ràng** | XML không validate được với schema |
| `V-01-1`, `V-01-2`, `V-01-4`, `V-02-2`, `V-04-2` | Warning | Số học, ngày ngoài khoảng bảo hiểm |
| `R1`…`R8` | **Warning, neo tại field** | Đối chiếu chéo |
| `R9` | **Sinh `packageState`** | Độ đầy đủ bộ hồ sơ |

### 5.2 Quy tắc thiết kế xuyên suốt *(verbatim, in đậm trong nguồn)*

> **Where IDP's determination and the document disagree, IDP's determination is used and a warning is raised. Nothing is silently corrected and nothing is inferred.**

### 5.3 Hai tính chất engine phụ thuộc vào *(verbatim A4 §8)*

> **Every warning names the document and field it attaches to**, so the review UI can anchor it; and **`amountClaimed` is present on every line**, so the benefit engine never has to recompute it.

Hình dạng một warning trong payload *(verbatim)*:

```jsonc
"warnings": [
  { "rule": "R6", "severity": "warning",
    "message": "Invoice total does not match the cost statement total",
    "documentId": "…", "field": "totalAfterVat" }
]
```

---

## 6. Sáu trạng thái package completeness

Sinh bởi **R9** và trả về cùng kết quả. *(A4 §6, verbatim)*

| State | Meaning |
|---|---|
| `COMPLETE` | Every required document type for this episode type is present and extracted |
| `MISSING_DOCUMENT` | A required type is absent — the response names which |
| `MISSING_FIELD` | All types present, but one or more mandatory fields could not be read — the response names field and document |
| `INCONSISTENT` | Present and readable, but one or more reconciliation rules failed |
| `AWAITING_ORIGINAL` | Digitally complete, awaiting a hard-copy original |
| `ERROR` | Terminal processing failure |

### 6.1 Khi nào vào, ai thấy, dẫn tới hành động gì

| State | Vào khi | Ai thấy | Dẫn tới hành động gì |
|---|---|---|---|
| `COMPLETE` | Mọi loại `REQUIRED` của `lossType` này có mặt **và đã trích xuất xong** | Thẩm định viên (tab checklist); khách thấy tiến trình đã qua bước hồ sơ | Hồ sơ đi tiếp sang xét duyệt. **Không kích yêu cầu bổ sung nào** |
| `MISSING_DOCUMENT` | Một loại `REQUIRED` vắng mặt | Thẩm định viên, **kèm TÊN loại thiếu**; khách qua yêu cầu bổ sung | **`PENDING_INFO`.** Sinh yêu cầu bổ sung, `DOC-NOTIF-001`. `REQUIRED` **chặn quyết toán** (`BR-DOC-1`) |
| `MISSING_FIELD` | Đủ loại, nhưng ≥1 field `M` không đọc được | Thẩm định viên, **kèm TÊN field VÀ TÊN chứng từ** | **`PENDING_INFO`.** `DOC-NOTIF-002` nêu đích danh field và chứng từ — *"a specific, actionable ask"*. Đây là **đường phục hồi của một tấm ảnh xấu**, và là lý do kiểm tra chất lượng lúc chụp **không chặn** (xem §9) |
| `INCONSISTENT` | Đủ và đọc được, nhưng ≥1 rule R1–R8 thất bại | Thẩm định viên, cảnh báo **neo tại từng field** trên từng chứng từ | **Không chặn tự động.** Thẩm định viên xử từng cảnh báo. Nếu cảnh báo đủ nặng, họ sinh yêu cầu bổ sung bằng tay |
| `AWAITING_ORIGINAL` | Đủ về mặt số, chờ bản gốc giấy (`BR-DOC-5`) | Thẩm định viên | **⚠ TRONG MODEL NHƯNG TẮT BẰNG CẤU HÌNH TRONG PILOT** — bản gốc không bắt buộc, hồ sơ quyết toán hoàn toàn trên ảnh. **Không màn hình nào hỏi bản gốc và không hồ sơ nào chờ giấy.** Giữ lại để bật về sau là **cấu hình, không phải một thay đổi máy trạng thái** |
| `ERROR` | Hỏng xử lý dứt điểm | **Hàng đợi VẬN HÀNH**, không phải hàng đợi hồ sơ | **Không màn hình nào được hiển thị cho khách một thông điệp mà sự cố hệ thống có thể bị nhầm thành một quyết định về hồ sơ của họ** (D2 §2.10) |

> **⚠ Engine TIÊU THỤ trạng thái, KHÔNG SUY LẠI.** `BR-DOC-4`: *"The package completeness state is produced by IDP and **consumed** by the claim engine, never re-derived."* A4 §6: *"It is what drives the claim's `PENDING_INFO` transition."*

> **⚠ ĐỪNG NHẦM `state` với `outcome`.** Webhook mang **`outcome`** (`COMPLETE | PARTIAL | ERROR`) — kết cục **xử lý**. Kết quả `I-09` mang **`state`** — độ **đầy đủ của bộ hồ sơ**. Hai enum khác nhau dưới một tên property, một cái tả xử lý một cái tả bộ chứng từ, là đúng kiểu va chạm sinh ra một nhánh sai sáu tháng sau.

### 6.2 `checklistItemState` — trạng thái từng DÒNG checklist, 5 giá trị

`CHƯA_NHẬN` · `ĐÃ_NHẬN` · `ĐÃ_YÊU_CẦU` · **`ĐÃ_YÊU_CẦU_KHÔNG_BỔ_SUNG`** *(kết thúc 1)* · **`MIỄN_YÊU_CẦU`** *(kết thúc 2)*

`BR-DOC-2` *(verbatim)*: *"A requested document ends in one of **two terminal states**: *supplied* or *requested-and-not-supplied*."*

**Đây là chỗ một thiết kế ngây thơ làm sai** *(D1 §6.3, verbatim)*: *"Reference System A tracks three flags per document type: `Cần bổ sung` (we asked), `Đã bổ sung` (they sent it), `Không bổ sung` (we asked, a batch came back, it still is not there). When a supplementary batch arrives, **every requested item leaves the 'we asked' state**, whichever of the two outcomes it lands in, and the claim moves on. **Without the second terminal state, a claim waits on a document that will never arrive, the SLA clock never restarts, and no one is prompted to decide on the evidence available.** `Không bổ sung` is also the audit trail that justifies settling partially or declining."*

`BR-DOC-3`: **tick checklist là MỘT CHIỀU** — đặt được, không bỏ âm thầm được, và mọi chuyển trạng thái được audit **giá-trị-cũ → giá-trị-mới**.

---

## 7. Term engine (lớp điều khoản)

### 7.1 `termClass` — 3 giá trị *(TRÍCH NGUYÊN VĂN D2 §5.28.1)*

| Class | Meaning | The engine **may** | The engine **may never** |
|---|---|---|---|
| **D — Deterministic** | Decidable from facts the claim already holds | **Auto-decline**, reason code `EXCL`, the term's `clauseRef` carried into the decline | — |
| **J — Judgemental** | Requires a person, or a clinical opinion — *cosmetic*, *under the influence* | Raise a **checkpoint** with the clause quoted, routed to §5.29 | **Auto-decline** |
| **E — Evidential** | Decidable only if a specific named document says so — a police or hospital statement | **Require the document**; raise a checkpoint where it is absent | **Auto-decline** |

### 7.2 `termClass` và `termEffect` là HAI TRỤC, không phải một *(§5.28.1a, verbatim)*

| Field | The question it answers | What it governs |
|---|---|---|
| **`termClass`** | ***Can the system decide this?*** | Whether an **auto-decline** is permitted — and nothing else |
| **`effect`** | ***What happens when the term is in play?*** | `DECLINE_EVENT` · `DECLINE_LINE` · `REQUIRE_DOC` · `CHECKPOINT` · `CAP` |

**Luật sống sót là MỘT CHIỀU, và phải đọc theo đúng một chiều** *(verbatim)*:

> **A class `J` or class `E` term may never carry `DECLINE_EVENT` or `DECLINE_LINE`. A class `D` term may carry any effect, including `CHECKPOINT`.**

*"Read the other way — **'class D declines, class J and E checkpoint'** — the model forces an auto-decline wherever a fact happens to exist, which is the outcome the three classes were introduced to prevent."* Enforce bằng `V-742` (D1 `BR-TERM-7`).

Ca chứng minh: `TERM-HC-003` thực phẩm chức năng — `serviceCategory = SUPPLEMENT` chọn một cách xác định, và mệnh đề *"not prescribed as treatment for the diagnosed condition"* nghĩa là **một thực phẩm chức năng được kê ĐƯỢC BẢO HIỂM**, nên từ chối tự động sẽ sai. **Chọn như class `D`, giải quyết như một `CHECKPOINT`.**

### 7.3 `termScope` — 3 giá trị *(verbatim, D1 `BR-TERM-8`)*

| Giá trị | Chạy ở bước | Với tới cái gì |
|---|---|---|
| `PRODUCT` | **bước 4** | Không nêu tên quyền lợi nào |
| `BENEFIT` | **bước 7** | Các dòng chi phí đã map về **quyền lợi được nêu tên** |
| `BENEFIT_SUBTREE` | **bước 7** | **Quyền lợi được nêu tên VÀ MỌI HẬU DUỆ của nó**, trên cùng đường tổ tiên mà `BR-TREE-1` dùng để tiêu thụ counter |

`BENEFIT` và `BENEFIT_SUBTREE` **phải nêu tên một quyền lợi tồn tại trong cây quyền lợi của snapshot.**

### 7.4 `termEffect` — 5 giá trị

`DECLINE_EVENT` · `DECLINE_LINE` · `REQUIRE_DOC` · `CHECKPOINT` · `CAP`

| Giá trị | Cơ chế |
|---|---|
| `DECLINE_EVENT` | Từ chối cả sự kiện. **Xếp trên `DECLINE_LINE`** (`BR-TERM-2`) — một term cấp sự kiện từ chối cả sự kiện bất kể kết quả từng dòng |
| `DECLINE_LINE` | Từ chối dòng. Trong một dòng, **MỌI** hit được ghi và **MỌI** `clauseRef` được mang vào thông điệp từ chối, không chỉ cái đầu tiên |
| `REQUIRE_DOC` | `BR-TERM-11`: **thêm loại chứng từ A4 mà nó nêu tên vào checklist của hồ sơ ở mức `REQUIRED`** và **raise một checkpoint** đứng mở **cho tới khi chứng từ được nộp hoặc yêu cầu được miễn** bởi thẩm định viên kèm lý do ghi lại. Luồng yêu cầu bổ sung (F29) là cái đi hỏi khách, **nên không phải dựng gì mới** |
| `CHECKPOINT` | Raise một câu hỏi cho người. **Mọi class đều có thể sinh ra nó** |
| `CAP` | `BR-TERM-12`: áp **trần tiền ở bước 7**, lên số tiền **tại cuối bước 6**, và **tính như một ràng buộc hạn mức** — nên hồ sơ bị nó giảm sẽ trượt điều kiện 1 của cổng tự động đúng như một hạn mức lịch trình. `BR-TREE-5` (phải giải thích một trần đã cắn) áp cho nó nguyên vẹn. **Không term nào trong sản phẩm pilot dùng nó.** ⚠ A1 §4.10 đăng ký **4** giá trị `termEffect` và **không có `CAP`** — lệch này là `D2-181`, còn treo |

**`CHECKPOINT` KHÔNG BAO GIỜ đè lên một từ chối** (`BR-TERM-2`): một dòng đã bị từ chối mà có checkpoint mở thì **vẫn bị từ chối**, và checkpoint đóng lại như **moot**, với lý do đó được ghi lại.

### 7.5 `termOutcome` — 4 giá trị *(TRÍCH NGUYÊN VĂN D2 §5.9.1a)*

| Giá trị | Nghĩa |
|---|---|
| `ĐẠT` | tested, did not fire |
| `TỪ_CHỐI` | the term declined — **only ever a class-`D` term whose effect is `DECLINE_EVENT` or `DECLINE_LINE`** |
| `ĐIỂM_KIỂM` | a checkpoint was raised — **any class may produce it** |
| `KHÔNG_THUỘC_PHẠM_VI` | the selector did not match, so **the term was never in play** |

**Đồng nhất thức phải đúng trên màn hình: `x + y + z + k = m`, và `m ≤ n`.**
Dòng header: *"n điều khoản trong sổ · m đã xét · x từ chối · y điểm kiểm · z đạt · k không thuộc phạm vi"*, với **nhóm `KHÔNG_THUỘC_PHẠM_VI` thu gọn mặc định**.

> **Vì sao phải có giá trị thứ tư** *(verbatim)*: *"A term whose selector did not match was being counted as though it had passed, and **'considered and not applicable' is a different answer from 'tested and satisfied'** — an auditor is owed the difference."*

**`BR-TERM-1` — xét TẤT CẢ, không phải first-match** *(verbatim)*: *"Every term in scope is evaluated and **the result of every term is recorded whether it fires or not**."* Trong phạm vi nghĩa là: **`ACTIVE`, trong cửa sổ hiệu lực, gắn với `ruleSetRef` của hồ sơ.**

### 7.6 `termStatus` — 4 giá trị

`DRAFT` · `PENDING_ACTIVE` · `ACTIVE` · `WITHDRAWN` — **do các hành động vòng đời ghi, KHÔNG BAO GIỜ được gõ tay.**

**Chỉ term `ACTIVE` trong cửa sổ hiệu lực mới được xét** (`V-735`) **và chỉ chúng được đếm** (`V-752`).

> **`V-752`, verbatim:** *"A `DRAFT` term is invisible to the engine: it is not evaluated, it produces no `termOutcome`, and it is not counted. **On the pilot product four of the ten registered terms are `DRAFT`** — two waiting on `Q-IDP-02`, one on `Q-IDP-05`, one on a document type A4 does not carry — **so `n` is ten and `m` is six**. A panel that counted ten would be telling an assessor that four terms had been considered when the engine had never looked at them, which is the same class of untruth as counting only the hits."*

**`V-748` — một term `WITHDRAWN` KHÔNG rút lại checkpoint đã mở**: rút một term là đưa nó ra khỏi các lần xét **tương lai**; nó **không thu hồi một câu hỏi đã hỏi** về một hồ sơ đang được thẩm định. Dòng trong hàng đợi mang **nhãn nói term đã bị rút**.

### 7.7 `termOperator` — 13 toán tử *(TRÍCH NGUYÊN VĂN D2 §5.28.3a, A1 §4.10)*

**10 toán tử so sánh:** `eq` · `ne` · `in` · `notIn` · `gt` · `gte` · `lt` · `lte` · `startsWith` · `between`
**3 từ khóa nhóm:** **`all`** · **`any`** · **`not`**

> ***"There is no `and` and no `or`: `all` and `any` are those operators under names that cannot be confused with a screen label."***

**Luật ghép hợp lệ** *(verbatim `F22`)*: `gt`, `gte`, `lt`, `lte`, `between` **chỉ trên fact kiểu số và ngày**; `startsWith` **chỉ trên fact kiểu chuỗi**; `eq`, `ne`, `in`, `notIn` trên **mọi scalar**. **Một cặp không hợp lệ KHÔNG ĐƯỢC ĐƯA RA để chọn.**
`between` **bao gồm cả hai đầu**. `not` **nhận đúng MỘT điều kiện** và control **từ chối cái thứ hai chứ không âm thầm bỏ nó**. `all` là mặc định.

**MỘT cấp lồng, không hơn** *(verbatim)*: *"A group may contain leaves; **a group may not contain another group**… **The limit is deliberate and it is stated as a decision rather than a phase-1 shortcut**: a nested expression is one an author cannot check by reading, a reviewer cannot four-eyes by reading, and an assessor… cannot reconstruct from the facts it matched on. **An exclusion that genuinely needs two levels is two terms.**"*

**Dạng lưu:** leaf là `{"fact": …, "op": …, "value": …}`; group là `{"all": […]}`, `{"any": […]}` hoặc `{"not": {…}}`.

**`V-744` — bốn phép kiểm lúc LƯU, cả bốn phía server, và một thất bại liệt kê MỌI vi phạm chứ không chỉ cái đầu:**
**(1)** fact tồn tại trong từ vựng đóng của A1 §4.11. **(2)** toán tử hợp lệ với kiểu của fact. **(3)** giá trị hợp lệ với miền của fact — một `serviceCategory` ngoài 20, một `lossType` ngoài 12, một tiền tố ICD sai dạng, một danh sách rỗng dưới `in`. **(4)** **fact có sẵn ở bước mà scope của term chạy** (`BR-TERM-9`).
**Phép kiểm 4 là cái đã từng cắn thật:** `TERM-HC-006` được viết là term scope `PRODUCT` test `benefitCode`, mà bước 5 mới sinh ra `benefitCode` còn bước 4 chạy trước bước 5 — nên **biểu thức đánh giá trên null ở mọi hồ sơ**, đúng trên dòng mà A5 §7.3 dựa vào làm bằng chứng rằng bước 4 chạy được.
**Kiểm lúc lưu, không phải lúc xét** — một term không xét được thì **không được vào sổ**. **Lưu bị TỪ CHỐI, không bao giờ được tự sửa.**

### 7.8 `checkpointOutcome` — 3 giá trị *(verbatim D2 §5.29.2 F8)*

| Giá trị | Nghĩa |
|---|---|
| `GIỮ_NGUYÊN` | **uphold** — từ chối dòng hoặc sự kiện, viện dẫn điều khoản |
| `BỎ_QUA` | **clear** |
| `KHÔNG_CÒN_HIỆU_LỰC` | **closed as moot — do NỀN TẢNG ghi, KHÔNG BAO GIỜ do một người chọn** (`V-741`, `V-749`) |

**Checkpoint bám vào đâu** *(verbatim `V-745`)* — khóa đúng là **fact mà selector khớp trên**:

| Selector khớp trên | Cấp | Checkpoint bám vào |
|---|---|---|
| `serviceCategory` · `benefitCode` · `lineDiagnosisCode` · `treatmentDate` · `memberAge` | **Line** | **Dòng chi phí đó.** Một checkpoint **mỗi dòng khớp**, không phải một mỗi term |
| `diagnosisCode` · `lossType` · `facilityCountry` · `facilityType` · `conditionClass` · `continuousCoverDays` | **Event** | **Sự kiện bảo hiểm đó** |
| `claimantCapacity` | **Claim** | **Hồ sơ**, không nêu dòng cũng không nêu sự kiện |
| **Một checkpoint `REQUIRE_DOC`** | — | **Sự kiện bảo hiểm mà loại tổn thất của nó đã chọn term.** Chứng từ là bằng chứng về một SỰ KIỆN, không phải về một dòng |

**Khi selector khớp ở cả hai cấp, checkpoint bám ở CẤP DÒNG, mỗi dòng khớp một cái.** **Chiều ngược lại KHÔNG BAO GIỜ xảy ra**: một selector thuần cấp sự kiện **không bao giờ được bung ra trên các dòng của sự kiện đó**.

**`BR-TERM-13`** — **một checkpoint đang mở chặn việc nộp phương án quyết toán, không chỉ chặn đường tự động.** Mỗi checkpoint raise ở **bước 4 hoặc bước 7** phải được **`GIỮ_NGUYÊN`**, **`BỎ_QUA`**, hoặc **đóng như moot đằng sau một từ chối**.

### 7.9 ⚠ CẢNH BÁO — 10 term pilot đều mang `clauseRef` `TBC-`, nên nhánh EXCL CHƯA DÙNG ĐƯỢC

`BR-TERM-10` *(verbatim D1 v0.5)*:

> **A term whose `clauseRef` is provisional may not decline.** Where the clause reference is a placeholder rather than a citation into filed wording, the term **raises a checkpoint instead of declining, whatever its class and whatever its effect**, and the checkpoint names the missing citation. **A decline must quote a clause that exists** — §5.5 and `BR-MSG-3` both require the customer to be told which term was relied on, and a placeholder is not an answer to that. **On the pilot product's register today this applies to every term**, so **step 4's auto-decline branch cannot fire at all until filed wording exists.**

`V-743` *(verbatim D2)*: mọi `clauseRef` bắt đầu bằng `TBC-` đánh dấu term là **unciteable**. **Lưu vẫn thành công, term vẫn được duyệt, term vẫn được xét** — **cái bị từ chối là phép TỪ CHỐI.** Ở màn checkpoint (§5.29.5), control *Giữ nguyên* **VẮNG MẶT, không phải bị disable** — *"a control a role may not use is absent, not disabled, because a disabled control the user cannot explain is a support call."* Banner nói lý do trong một câu: *"điều khoản này chưa có số điều khoản chính thức; chưa thể giữ nguyên"*.

**Cái vẫn chạy được, và cái không** *(verbatim `D2-159`)*:

> **What still works is everything else**: terms select, terms are counted, checkpoints are raised, and an assessor can clear one with a reason. **What does not work is the decline**, which is the half the exclusions exist for. **The business must file the wording and map each term to a real clause number**, and until it does, **every claim that touches an exclusion reaches a person and none of them can be declined on it.**

**Sổ term pilot — 10 term, `TBC-01` … `TBC-10`, 6 `ACTIVE` / 4 `DRAFT`. Những cái đã biết:**

| clauseRef | Class | Effect | Selector |
|---|---|---|---|
| `TBC-01` — phẫu thuật thẩm mỹ | `J` | `CHECKPOINT` | `diagnosisCode startsWith Z41` |
| `TBC-02` — khám sức khỏe, tầm soát, tiêm chủng | `D` | `CHECKPOINT` | `serviceCategory in [SCREENING, VACCINATION]` |
| `TBC-03` — thực phẩm chức năng | `D` | `CHECKPOINT` | `serviceCategory = SUPPLEMENT` |
| `TBC-04` — kính, máy trợ thính | `D` | `DECLINE_LINE` | `serviceCategory in [OPTICAL, HEARING]` |
| `TBC-06` — điều trị răng ngoài quyền lợi `300` | `D` | `DECLINE_LINE` | `serviceCategory = DENTAL` |
| `TBC-07` — lệ thuộc rượu, chất gây nghiện | `J` | `CHECKPOINT` | `diagnosisCode startsWith F1` |

**4 term `DRAFT` và lý do treo:** 2 chờ **`Q-IDP-02`** (`L16` ICD theo dòng) · 1 chờ **`Q-IDP-05`** (`facilityCountry`) · 1 chờ **một loại chứng từ A4 chưa mang** (`T2-04`).

> **Hệ quả cho demo hackathon.** Kịch bản nào dựa vào một **từ chối tự động trên loại trừ** đều **không chạy đúng chuẩn**. Cái chạy đúng chuẩn là: term được chọn, term được đếm, **checkpoint được raise**, và thẩm định viên **`BỎ_QUA`** kèm lý do. Nếu muốn demo một từ chối `EXCL`, phải **seed một `clauseRef` THẬT** (ví dụ `Art. 5.2(c)`) và **nói rõ trên slide rằng đó là dữ liệu demo**, vì trên sổ pilot thật thì không term nào từ chối được.

### 7.10 `selector` / `predicate` — hai biểu thức, không phải một

*(D2 §5.28.2, đã được D1 `BR-TERM-7` phê chuẩn)*

| Câu hỏi | Trả lời bởi |
|---|---|
| **Term này có trong cuộc với hồ sơ / sự kiện / dòng này không?** | `selector` — **bắt buộc và non-null trên MỌI class** `D`, `J`, `E` |
| **Đã trong cuộc rồi, nó có cắn không?** | `predicate` — **chỉ cho phép trên class `D`**, null trên `J` và `E`, và **bắt buộc khi effect của một term class-`D` là `DECLINE_EVENT` hoặc `DECLINE_LINE`** |

**Vì sao không gộp một field** *(verbatim)*: với một field duy nhất bị cấm non-null trên class J thì **không còn gì trả lời câu hỏi thứ nhất**, và **một term không có cách nói nó liên quan hồ sơ nào thì liên quan MỌI hồ sơ** — mọi term class-J sẽ raise checkpoint trên mọi hồ sơ, và **một hàng đợi nhận cả danh mục là một hàng đợi không ai làm.**

**`V-734`:** một `selector` rỗng **bị từ chối lúc lưu** — *"an empty selector does not mean *not selected*, **it means *in play for every claim***"*, và tác giả được nói điều đó **ngay trên builder** chứ không phải lúc save.

---

## 8. Quy tắc nghiệp vụ BR-*

Trích nguyên văn D1 v0.5. Cột cuối là ánh xạ sang thứ phải dựng.

### 8.1 `BR-DOC` — checklist chứng từ

| Mã | Nội dung *(verbatim)* | Ảnh hưởng tới |
|---|---|---|
| **BR-DOC-1** | Three requirement levels: `REQUIRED` blocks settlement · `EXPECTED` triggers an information request the assessor may waive · `OPTIONAL` never blocks and is never requested | **Seed `DocumentChecklistRule`** (§2.1) · màn S12 chụp chứng từ · tab checklist của console · `V-588` gate nộp phương án |
| **BR-DOC-2** | A requested document ends in one of **two terminal states**: *supplied* or *requested-and-not-supplied* | **Enum `checklistItemState` 5 giá trị** (§6.2) · đồng hồ SLA · luồng yêu cầu bổ sung F44 |
| **BR-DOC-3** | A checklist tick is **one-way**: it can be set but not silently unset, and moving between states is audited old-value → new-value | Entity checklist item + **bảng audit**, `BR-AUDIT-2` |
| **BR-DOC-4** | The package completeness state is produced by IDP and **consumed** by the claim engine, never re-derived | **`packageState` đọc từ `I-09`**, engine KHÔNG tính lại (§6) |
| **BR-DOC-5** | Copy versus original is tracked per document, with an `AWAITING_ORIGINAL` state | Thuộc tính bản-gốc/bản-sao trên document · **tắt bằng cấu hình trong pilot** |

### 8.2 `BR-LINE` — dòng bị từ chối

| Mã | Nội dung *(verbatim)* | Ảnh hưởng tới |
|---|---|---|
| **BR-LINE-1** | A declined line carries a reason code **and** a rationale naming the item and the amount | Lưới dòng chi phí §5.6.3 · **enum `declineReason`** · thư quyết toán |
| **BR-LINE-2** | The rationale may be **AI-drafted**; the assessor may edit it before it is sent | **Đây là chỗ agent tạo giá trị** — agent soạn, người sửa |
| **BR-LINE-3** | Both the original drafted text and the final text are retained | **Hai cột text trên settlement line.** Là bằng chứng đo được cho pitch: tỷ lệ thẩm định viên sửa |
| **BR-LINE-4** | A declined line appears in the settlement letter **in the same table as the paid lines**, never in an appendix | Template thư quyết toán · `BR-MSG-2` |

### 8.3 `BR-WAIT` — thời gian chờ

| Mã | Nội dung *(verbatim)* | Ảnh hưởng tới |
|---|---|---|
| **BR-WAIT-1** | The period is measured from the **member's coverage start date**, not the policy inception date | Bước thời gian chờ trong chuỗi · đọc từ snapshot thành viên |
| **BR-WAIT-2** | On a renewal without a break in cover, periods already served are **not re-served**. **Stated for completeness and not exercisable in MVP 1** — renewals are out of scope | **Không dựng.** Ghi lại để không ai tưởng là thiếu |
| **BR-WAIT-3** | The condition class is determined at assessment and stored on the insured event | **`conditionClass` là field NHẬP trên sự kiện** (§1.6), do thẩm định viên chọn trong tập được `lossType` cho phép |
| **BR-WAIT-4** | `n/a` in the matrix means the combination **cannot arise**. A claim that produces one is a **data error**, not a zero-day waiting period | **Bẫy lớn.** `n/a` ≠ `0`. Rơi vào ô `n/a` → hàng đợi vận hành, không phải cho qua |
| **BR-WAIT-5** | A waiting-period decline shows the arithmetic: the period, the date it counts from, and the treatment date | Thư từ chối phải in **ba con số**, không phải một câu |

### 8.4 `BR-TREE` — cây quyền lợi và hạn mức

| Mã | Nội dung *(verbatim)* | Ảnh hưởng tới |
|---|---|---|
| **BR-TREE-1** | **Roll-up.** A payment consumes the counter of the benefit it is paid under *and every ancestor*. The claim is capped by the tightest binding limit anywhere on that path | **Lõi của `RunAdjudicationChain`** · `BR-LEDGER-6` · `BENEFIT_SUBTREE` của `BR-TERM-8` dùng cùng đường tổ tiên |
| **BR-TREE-2** | **No double counting.** Where a composite and one of its own sub-limits both apply to the same line, the composite consumption is counted once | ⚠ **Viết cho CÙNG MỘT DÒNG.** Không với tới ca chuyển ICU→thường cùng ngày — xem `BR-LEDGER-12` |
| **BR-TREE-3** | Limits exist in **money** and, independently, in **count** (days, visits, occurrences) | **3 `counterType`** · bước 9 test counter `DAY` |
| **BR-TREE-4** | **`0` means unlimited**, not zero. Honoured explicitly, never inferred | **Bẫy seed.** Một `0` bị hiểu là "không đồng nào" sẽ từ chối sạch một quyền lợi không giới hạn |
| **BR-TREE-5** | **A cap that binds must be explained.** Where a limit reduces the payable amount, the settlement letter states the benefit, the remaining limit and the reduction | Thư quyết toán · áp nguyên vẹn cho effect `CAP` (`BR-TERM-12`) |

### 8.5 `BR-PAY` — tài khoản nhận tiền

| Mã | Nội dung *(verbatim)* | Ảnh hưởng tới |
|---|---|---|
| **BR-PAY-1** | The account holder's name is **never typed**. It is populated only by the verification call | Màn S20 · **ô tên chủ tài khoản KHÔNG CÓ BÀN PHÍM** |
| **BR-PAY-2** | If the account number is subsequently changed, the verified name is **cleared** and verification must run again | Logic màn S20 · cờ `accountNameVerified` |
| **BR-PAY-3** | A claim cannot be submitted for approval, and a payment cannot be instructed, unless `accountNameVerified` is true | **Gate nộp phương án** · gate lệnh chi |
| **BR-PAY-4** | The customer may reuse a payee proven on a **previously paid** claim for the same insured, matched on **name + date of birth + ID number** | Tra cứu payee · **khóa ghép 3 thành phần**, không phải memberId |

> ⚠ **`BR-PAY-1` là cái đã hỏng:** bản ghi có chỗ ở (`F58`), nhưng **dịch vụ xác minh KHÔNG CÓ HỢP ĐỒNG nào trong D3** — `D2-106`, `DE-12`. Xem §10.
> **`BR-AUTO-6`:** một hồ sơ được **duyệt tự động** mà tài khoản chưa xác minh **đi vào hàng đợi vận hành**, không đi vào lệnh chi — vì duyệt tự động **bỏ qua phương án quyết toán**, nên không có `BR-PAY-3` chặn.

### 8.6 `BR-DUP` — trùng hồ sơ

| Mã | Nội dung *(verbatim)* | Ảnh hưởng tới |
|---|---|---|
| **BR-DUP-1** | On submission, the Claim Platform searches for an existing non-cancelled claim with the **same insured member, same provider and overlapping treatment dates** | Agent duplicate-signal · chạy **lúc nộp**, không phải lúc thẩm định |
| **BR-DUP-2** | A match raises a **warning to the assessor**, naming the other claim number. **It does not block the customer** | Banner trên màn hồ sơ · điều kiện 4 của cổng duyệt tự động |
| **BR-DUP-3** | Where an invoice number has been extracted, a second claim carrying the **same invoice number** is a stronger signal and is flagged as such | **Cần `T1-04.3` `SHDon`** — mà `T1-04` chỉ có nếu khách nộp XML. Tín hiệu mạnh nhất lại là tín hiệu ít sẵn nhất |
| **BR-DUP-4** | **The customer is never told a duplicate was detected. It is an internal control** | ⚠ **Tuyệt đối.** App **KHÔNG CÓ BỀ MẶT NÀO** cho nó, **kể cả một cảnh báo mềm** |

### 8.7 Ba rule xuyên suốt phải nhớ khi viết bất kỳ rule nào

| Mã | Nội dung *(verbatim)* |
|---|---|
| **BR-CHAIN-1** | **A step of the chain that cannot be evaluated because a fact it requires is unavailable is never treated as a pass.** The line is **held**, a **checkpoint is raised naming the missing fact**, and the claim is **outside the auto-approval gate**. **It is not a decline either**: an absent fact is a data problem |
| **BR-DATA-1 / -2 / -3** | The extraction result is **never overwritten** by a correction · Corrections are written to the claim's own copy; **the pair stays comparable** · **Re-running extraction while a claim is pending approval is prohibited** |
| **BR-MSG-3** | The decline notice states **the reason, the clause relied on**, that cover continues, and the route to challenge the decision | 

> **`BR-CHAIN-1` là rule quan trọng nhất trong bộ cho việc build agent.** Một agent trả về *"không đủ dữ kiện"* **không được** ánh xạ thành *pass*. Nó ánh xạ thành **giữ dòng lại + raise checkpoint nêu tên dữ kiện thiếu + hồ sơ ra ngoài cổng tự động**.

---

## 9. Chất lượng đầu vào và kiểm tra ảnh

*(D2 §4.6, verbatim)*

| Element | Specification |
|---|---|
| **Where it runs** | **On the device, at capture**, before any upload. *"a bad photograph rejected at capture is not a failed extraction three minutes later"* |
| **Blur** | **Variance-of-Laplacian** over the detected document region, against a threshold held as `config` and **tunable without a release**. Below threshold → `DOC-VAL-001` |
| **Edges** | **Quadrilateral detection**; a page whose detected boundary **touches or leaves the frame on any side** → `DOC-VAL-002` |
| **Glare** | **Saturated-pixel proportion** over the document region above a `config` threshold → `DOC-VAL-003` |
| **What the customer sees** | The captured image, the A2 message beneath it, and two controls: **Chụp lại** and **Dùng ảnh này**. The message is advisory in tone by construction — **A2 rule M-7 phrases all three as a property of the image, never of the photographer** (*"Ảnh chưa đủ rõ để đọc"*, not *"bạn chụp sai"*) |
| **Whether it blocks** | **It does not.** *Dùng ảnh này* is always available |

### 9.1 Vì sao một phép kiểm trượt KHÔNG chặn — hai lý do, và lý do thứ hai mạnh hơn *(verbatim)*

> **First**, some documents cannot be photographed to threshold — a faded thermal cost statement, a carbon-copy prescription — and **a blocking check traps the customer with a valid claim and no exit.**
>
> **Second, a poor image has a recovery path and a blocked capture does not**: an unreadable field produces `MISSING_FIELD`, an information request and `DOC-NOTIF-002` naming the field and the document, **which is a specific, actionable ask. A capture the app refused produces nothing at all.**
>
> The check is worth running because **a retake at capture costs ten seconds and a retake after extraction costs a day** — but **its value is in the prompt, not in the veto.**

### 9.2 Ba thứ kèm theo, dễ quên khi dựng

| Thứ | Quy tắc |
|---|---|
| **Lệch loại là THÔNG TIN, không phải lỗi** | `DOC-VAL-004` nói chuyện gì xảy ra và **không đòi hỏi gì ở khách**. **Phân loại của IDP thắng.** *"Silently overriding what someone told you erodes trust; asking them to fix a disagreement the system has already resolved wastes their time."* |
| **Chụp offline** | Trang ghi vào **hàng đợi cục bộ MÃ HÓA**, F8 = *Chờ tải lên*, hồ sơ giữ `DRAFT`. **Kiểm tra chất lượng VẪN CHẠY — chúng chạy trên máy và không cần mạng.** Giữ **7 ngày**. Ảnh bị xóa khỏi máy khi khách hủy **và khi nộp thành công** |
| **Nộp phải chờ upload xong** | *Gửi hồ sơ* đòi **mọi file đã upload**. Màn hình **nói còn bao nhiêu trang** chứ không báo lỗi. Lý do: `I-07` nộp gói **bằng tham chiếu** — một gói trỏ tới object chưa tới là một gói hỏng, hồ sơ sẽ vào `DOC_PROCESSING` rồi chết ở đó với `EXTRACTION_ERROR`, **vì một nguyên nhân đã biết từ trên máy trước khi nộp** |

**Giới hạn §2.7:** ≤ **10 MB mỗi file**, ≤ **20 trang mỗi loại**, ≤ **50 file mỗi hồ sơ**. Giới hạn áp cho **cả hàng đợi offline**, không chỉ cho lần upload. Giới hạn của nhóm "giấy tờ khác" **dùng CHUNG với checklist, không cộng thêm**.

### 9.3 Confidence dùng để làm gì — A4 §7 *(verbatim)*

| Item | Rule |
|---|---|
| Per-field confidence | Returned for every extracted field and every table cell |
| Row confidence | **The minimum** confidence among that row's cells |
| **Mandatory field set** | Every field marked `M` in §4, plus **every `M` column of a line item** |
| **Claim-level extraction confidence** | The **minimum** confidence across the mandatory field set — **deliberately the minimum, not the mean, so one badly-read amount cannot be averaged away** |
| **What it drives** | **Assessor routing, not the auto-approval gate.** It decides which claims and which fields an assessor is pointed at first. **D1 §5.8's gate tests three conditions and confidence is not one of them** — that omission is recorded there, deliberately. The threshold value is **set from the ground-truth benchmark, not guessed** |
| Below threshold | Routes to an assessor with the **specific field** named — **not a generic "needs review"** |

---

## 10. Cần xác thực / còn treo

### 10.1 Câu hỏi cho IDP — chặn hoặc làm suy giảm *(A4 §9)*

| # | Nội dung | Nếu câu trả lời là "không" |
|---|---|---|
| **`Q-IDP-01`** | **Crosswalk document-class — cột phải trống của A4 §3.1.** Với mỗi trong **8** loại của ta, giá trị `classification` nào của đối tác map về nó? **Đây là câu CHẶN chứ không làm suy giảm**: `classification` đang được tiêu thụ trên một phép map GIẢ ĐỊNH, và một giả định sai **xếp một chứng từ vào một loại nó không phải, thỏa một mục checklist mà không gì thỏa cả.** Hỏi luôn: **đối tác báo việc đổi tên / thêm class bằng cách nào** | Adapter chạy trên giả định = một defect đang chờ lần đổi tên đầu tiên |
| **`Q-IDP-02`** | **`L15` (ngày theo dòng) và `L16` (ICD theo dòng) có trích được từ bảng kê Việt Nam không — và nhà cung cấp IN chúng ở cấp dòng thường xuyên đến đâu?** **Tần suất quan trọng ngang khả thi**: một field in trên 1/20 bảng kê không đáng một model | Cột do thẩm định viên nhập tay, console nói vậy. **Với `L16` nghĩa là nhánh lâm sàng vẫn lập luận trên chẩn đoán cấp SỰ KIỆN trên hồ sơ nhiều chẩn đoán**, và **2 loại trừ ở lại `DRAFT`** |
| **`Q-IDP-03`** | **Kỳ vọng độ chính xác trên `L4`, `L6`, `L11`, `L12`?** Cả bốn trích từ v1.0 và **không ai tiêu thụ**, nên **chưa từng có con số độ chính xác nào**. **Nếu chúng chưa từng được annotate trong bộ ground-truth, NÓI NGAY** — đó là lỗ hổng ở §10 chứ không phải ở model, và **rẻ trước khi annotate, đắt sau khi annotate** | 4 con số độ chính xác không ai có, 2 trong số đó nằm cạnh tiền |
| **`Q-IDP-04`** | **Payload kết quả có mang `documentDate` và `icd10` theo dòng không, và có thêm được `unitOfMeasure`, `drugRegistrationNo`, `otherPayerAmount` vào mảng `lines` không?** §8 sửa payload trên giả định là có | Nếu hình dạng object dòng cố định → **một call thứ hai hoặc một kênh phụ, cả hai đều tệ hơn việc biết trước** |
| **`Q-IDP-05`** | **`facilityCountry`** (header field 18) — suy từ địa chỉ in trên trang | Loại trừ #10 (điều trị ngoài lãnh thổ VN) **không có dữ kiện nào đứng sau**, term ở lại `DRAFT` |
| **`Q-IDP-06`** | **`L18` số ngày điều trị theo dòng** | Bước 9 test một counter `DAY` **không gì populate theo dòng** |
| **`Q-IDP-07`** | **`T2-04`** — chữ ký phân loại và mẫu huấn luyện. **Một biên bản công an KHÔNG PHẢI tài liệu y tế**, nên nó là loại tier-2 duy nhất mà chữ ký và mẫu huấn luyện **không thể giả định rơi ra từ bộ sẵn có** | Mọi biên bản công an tới như `unclassified` — an toàn, **và cũng là toàn bộ cơ chế checklist mà `BR-TERM-11` phụ thuộc vào, hỏng một cách im lặng** |
| **`Q-IDP-08`** | **4 giá trị `serviceCategory` mới** — IDP có dạy được phép suy diễn 4 giá trị mới không | 4 giá trị do thẩm định viên nhập tay tại `L13`; 2 loại trừ ở lại checkpoint thay vì đạt class `D` |
| **`I-2`** | **Field derived sinh TRONG IDP hay trong engine?** (`L13` serviceCategory, `D9` item class, header 17 treatment setting). **Giả định của ta: trong IDP** | Nếu là engine, **§4 của tài liệu này phải viết lại thành đặc tả cho Server Action, không phải cho IDP** |
| **`I-7`** | **Rule R1–R9 chạy TRONG IDP hay trong engine?** **Giả định của ta: trong IDP**, vì chúng là cross-document và IDP giữ mọi chứng từ | Nếu là engine, **§5 thành đặc tả `RunReconciliation` Server Action** — đây là **giả định đắt nhất trong tài liệu này** |

### 10.2 Còn treo phía nghiệp vụ

| # | Treo gì | Ai gỡ |
|---|---|---|
| **`D2-159`** | **CHẶN NẶNG NHẤT.** Câu chữ hợp đồng chưa được nộp, **cả 10 term mang `clauseRef` `TBC-nn`**, `BR-TERM-10` cấm từ chối viện dẫn một cái tạm → **nhánh auto-decline của bước 4 không chạy được**. **Không gì gỡ được ngoài việc nộp câu chữ và map từng term về một số điều khoản thật** — không phải một câu trả lời từ vendor, không phải một lỗ hổng dữ liệu, không phải một việc build | **Business + BA** |
| **`B-24`** | **`T2-04`**: là biên bản công an, xác nhận nhập viện, hay **hai loại riêng**; tên VI/EN; mặc định `REQUIRED` hay `EXPECTED` trên hồ sơ tai nạn; có cần bản gốc không | **Business** |
| **`D2-114`** | **A4 §6 phải được cắt lại** trên 8 `lossType` + tư cách người yêu cầu. **Tới khi đó, CẢ HAI bề mặt đọc bảng 8+1 dòng của D2 §4.6** — đó là *"a transitional read of the annex's data through the one place D2 has written it down, not a second definition of it"*. **Và mục này mang câu hỏi thứ ba: `lossType` có 12 giá trị còn bảng có 8 dòng, nên 4 giá trị tử vong / thương tật vĩnh viễn KHÔNG CÓ CHECKLIST NÀO** | **A4 (BA)** |
| **`D2-181`** | **A1 §4.10 đăng ký 4 `termEffect` và cố ý KHÔNG có `CAP`**, trong khi D1 `BR-TERM-12` đã định nghĩa `CAP`. Và **A1 đăng ký 4 `stepOutcome` — không cái nào nghĩa là *không xét được*** | **A1 (BA)** |
| **`D2-106`** | **Dịch vụ xác minh tên chủ tài khoản KHÔNG CÓ HỢP ĐỒNG nào trong D3.** `BR-PAY-1` và `BR-PAY-3` đều dựa vào cờ này | **BA + D3** |
| **Phase 2 — dedup** | **Còn treo: so trường nào, và trong bao lâu (6 tháng hay 1 năm).** `BR-DUP-1` chỉ nói *cùng thành viên + cùng nhà cung cấp + ngày điều trị chồng lấn* | **PO** |

### 10.3 Treo phía build của đội

| # | Vấn đề | Ghi chú |
|---|---|---|
| **1** | **`T1-03` Phiếu khám bệnh chưa có template ở tenant 176.** Bộ có `5384`/`5127` Giấy ra viện, `5383` bảng kê, `5299` đơn thuốc, `5340` xét nghiệm, `5312` biên lai. **`T1-03` là loại `REQUIRED` trên 4/8 `lossType`** và là nguồn của chẩn đoán-của-hồ-sơ | Phải tạo template mới, **hoặc** chấp nhận demo trên `ILL_IP` (chỉ `T1-01` là `REQUIRED`) và nói rõ |
| **2** | **`T1-04` chưa có XML thật nào để test parser** | A4 §10 nói 10 XML là **không thương lượng**. Với hackathon: dựng **1 XML hợp lệ tự tạo** và đánh dấu là dữ liệu demo |
| **3** | **Ngưỡng blur / edge / glare là `config`, chưa có giá trị** | Nếu demo có bước chụp, phải chọn 3 con số. **Chúng phải chỉnh được không cần release** |
| **4** | **Ngưỡng confidence chưa chốt** | A4 §7: *"set from the ground-truth benchmark, not guessed"*. **Đội chưa có benchmark.** Xem §11 phụ lục — MBAL có 2 con số làm mốc thô |
| **5** | **`serviceCategory` — bảng từ khóa là master data của ta, phép suy diễn là của IDP** | Nếu demo tự suy diễn `L13` trong ODC thì **đang làm việc của IDP**, và phải nói rõ trên slide |
| **6** | **Bốn giá trị `lossType` tử vong / thương tật vĩnh viễn không có checklist** | Nếu counter-intake vào demo, chúng sẽ sinh hồ sơ **không thỏa checklist được** |

---

## Phụ lục — đối chiếu sang MBAL-IDP

> ## ⚠ ĐỌC TRƯỚC
> **Không một dòng nào trong phụ lục này là CHUẨN.** Chuẩn gốc là **Claim MVP** (§1 đến §10). Phụ lục này tồn tại vì đội đã bỏ 4 tuần đọc MBAL-IDP và **có vài thứ đo được thật** ở đó mà Claim MVP chỉ có trên giấy. Dùng nó để **kiểm chứng thực tế**, **không bao giờ** để seed cấu hình.
>
> **Ba điều cấm:** ① không seed `claim_type` `MEDICAL_*` hay `document_type` `CL*` vào bất cứ entity nào; ② không dùng templateId 776/777/1091/2645/1141 — tenant 176 dùng bộ khác; ③ không trình bày con số của MBAL như con số của Claim MVP trên slide mà không ghi nguồn và điều kiện.

### P.1 Ánh xạ loại chứng từ — `T1-xx` ↔ `CLxx`

| Claim MVP | MBAL-IDP tương đương | Chất lượng ánh xạ |
|---|---|---|
| `T1-01` Bảng kê chi phí KCB | `CL26` Bảng kê chi tiết · template `776` / `1091` | **Gần đúng.** `CL26` chỉ là một dòng trong `document_type`; Claim MVP coi đây là chứng từ quan trọng nhất còn MBAL đặt nó ở mức *không bắt buộc* |
| `T1-02` Đơn thuốc | template `2645` VNVision-Đơn thuốc, nằm trong `CL10` | **Lỏng.** MBAL không có `document_type` riêng cho đơn thuốc; nó là một template trong rổ `CL10` |
| `T1-03` Phiếu khám bệnh | template `2638` Sổ khám bệnh / `1145` Tóm tắt bệnh án, trong `CL10` | **Lỏng.** ⚠ Claim MVP **CỐ Ý loại trừ** Sổ khám bệnh (*"Handwritten; the hardest document in the set"*) |
| `T1-04` Hóa đơn GTGT | `CL11` Hóa đơn chi phí y tế · template `1089` | **Khác về BẢN CHẤT.** MBAL đọc hóa đơn bằng **OCR trên ảnh**; Claim MVP **chỉ nhận XML, không có đường OCR** |
| `T2-01` Giấy tờ tùy thân | `CLM_04` → template `1167` CCCD, `2637` Thẻ BHYT, `2649` Trích lục khai sinh | Khớp về ý định (chỉ phân loại) |
| `T2-02` Kết quả cận lâm sàng | `CL10` → `1141` xét nghiệm, `2245` cận lâm sàng, `1138` siêu âm, `2639` X-Quang, `2643` CT | **1 loại của ta ↔ 5 template của MBAL** — đúng kiểu one-to-many mà rule `X-1` nói tới |
| `T2-03` Báo cáo y tế / GCN phẫu thuật | `CL06` Chứng nhận phẫu thuật · `2641` Báo cáo y tế · `973`/`2659` GCNPT | Khớp |
| `T2-04` Biên bản hoàn cảnh sự cố | `CL08` Chứng từ tai nạn · `2656` Bản tường trình tai nạn | **Gần nhất trong bộ MBAL.** Nhưng `B-24` chưa chốt nên chưa map được chắc |
| **không có** | `CL05` Giấy ra viện · `CL07` Trích lục khai tử · `CL12` Biên bản giám định y khoa · `CL15` Giải phẫu bệnh | **Claim MVP KHÔNG CÓ các loại này.** `CL05` bị loại trừ có chủ ý (A4 §1.4); `CL07`/`CL12`/`CL15` thuộc nghiệp vụ tử vong và thương tật vĩnh viễn, **ngoài phạm vi MVP** |

### P.2 Ánh xạ `lossType` ↔ `claim_type` `MEDICAL_*`

| Claim MVP `lossType` | MBAL `claim_type` | Ghi chú |
|---|---|---|
| `ILL_OP`, `ILL_IP`, `DEN_OP`, `DEN_IP`, `MAT_OP`, `MAT_IP` | `MEDICAL_CARE` (mã eBao 5) | **6 giá trị của ta gộp vào 1 của MBAL.** MBAL không phân biệt nha khoa, thai sản, và **không phân biệt ngoại trú với nội trú** — đúng chỗ Claim MVP nói `episodeType` 5 giá trị là sai |
| `ACC_OP`, `ACC_IP` | `MEDICAL_CARE` khi điều trị; `MEDICAL_INJURY` (12) khi yêu cầu theo tỷ lệ thương tật | **Không map 1-1 được.** Trục của MBAL là *loại quyền lợi được yêu cầu*, trục của ta là *bệnh lý × nơi điều trị* |
| 4 giá trị tử vong / thương tật vĩnh viễn | `MEDICAL_DEATH` (2), `MEDICAL_INJURY` (12) | Ngoài phạm vi MVP 1 của ta |
| *(không có)* | `MEDICAL_ILLNESS` (4) — bệnh lý hiểm nghèo | **Claim MVP không có khái niệm này.** Bệnh hiểm nghèo là một loại quyền lợi khác, không phải một `lossType` |

> **Kết luận của phép đối chiếu:** hai bộ **không cùng trục**. MBAL phân theo **loại quyền lợi được yêu cầu**; Claim MVP phân theo **bệnh lý × nơi điều trị**. **Một phép chuyển đổi tự động giữa hai bộ là không thể**, và bất kỳ ai thử sẽ tạo ra một hàm hoặc mất thông tin hoặc bịa thông tin.

### P.3 Ba asset của MBAL nên MƯỢN để kiểm chứng thực tế

**Không phải chuẩn. Là bằng chứng.**

#### P.3.1 Từ điển field đồng nghĩa — cấu hình 10 comparator production

Đây là thứ có giá trị nhất mượn được: **một danh sách các cách một cơ sở y tế Việt Nam thực sự in cùng một khái niệm.** Rule **R1, R2, R3, R4, R8** của Claim MVP đều là đối chiếu chéo tên field, và danh mục A4 chỉ cho **một** tên VI mỗi field.

| `standalone_field_display_name` | `standalone_field_names` | Dùng kiểm chứng rule nào của ta |
|---|---|---|
| `Họ và Tên` | Tên bệnh nhân · Họ và tên · Họ tên · Họ và Tên bệnh nhân · Họ và tên người bệnh · Insured Name | **R1** (T1-01.6 · T1-02.4 · T1-03.5) |
| `DOB` | Ngày sinh · Năm sinh · DOB · Ngày/ tháng/ năm sinh · Ngày, tháng, năm sinh | **R2** (T1-01.7 · T1-03.6) |
| `Gender` | Gender · Giới tính | T1-01.8 · T1-03.7 |
| `IdentificationNumber` | Số CCCD · Số định danh cá nhân · Hộ chiếu · Số hộ chiếu · Giấy tờ tùy thân · Identification number | `T2-01` — ta chỉ phân loại, không đọc |
| `AdmissionDate` | Admission Date · Ngày vào viện · Vào viện ngày · Vào viện lúc | **T1-01 field 15** |
| `DischargeDate` | Discharge Date · Ra viện ngày · Xuất viện lúc · Đến ngày | **T1-01 field 16** — field không được mặc định |
| `DeathDate` | Đã chết vào lúc · Death Date | Ngoài phạm vi |
| `EventDate` | Thời gian | D2 §5.7.2 `F16` |
| `DiagnosisCodes` | Diagnosis Codes · Mã ICD · Mã bệnh · Mã bệnh kèm theo | **R8** (T1-01.13/.14 · T1-02.8/.9 · T1-03.12/.13) |
| `PlaceOfTreatment` | Place Of Treatment · Bệnh viện · Tên bệnh viện | **R3** (T1-01.1 · T1-02.1 · T1-03.1) |

**Cách dùng đúng:** không seed bảng này. Dùng nó làm **danh sách kiểm tra khi viết prompt trích xuất cho `T1-01`/`T1-02`/`T1-03`** — nếu prompt chỉ tìm *"Họ tên bệnh nhân"* thì nó sẽ trượt trên tờ in *"Họ và tên người bệnh"*.

**Cấu hình production của 10 comparator:** `is_cross_compare = true` (so mọi cặp chứng từ) · `case_insensitive = true` · `skip_compare_on_not_found_fields = true`. **Cái thứ ba là một quyết định nghiệp vụ quan trọng và Claim MVP đồng ý với nó**: không có dữ liệu để so thì **im lặng**, không cảnh báo. MBAL học điều này qua **8 bug false-positive**.

> ⚠ **Khoảng trống lớn nhất của MBAL, và nó là chỗ agent của ta tạo giá trị đo được: comparator CHỈ ĐỌC mảng `fields`. Dữ liệu trong `tables` KHÔNG THAM GIA ĐỐI CHIẾU** — toàn bộ dòng chi phí trong bảng kê **chưa hề được kiểm tra chéo**. Rule **R5** và **R7** của Claim MVP đều là rule **trên bảng** — đây chính là hai rule MBAL không có.

#### P.3.2 Contract cảnh báo con dấu — R5a của MBAL

**Không nằm trong phạm vi Claim MVP** — A4 không có rule giả mạo nào, và §2.10 của Claim MVP không mang nó.

Cái mượn được là **cách MBAL đóng khung phạm vi**, verbatim từ biên bản chốt Phase 2:

> **"Scope chỉ tập trung kiểm tra dấu trên chứng từ (không bao gồm cắt ghép, chỉnh sửa làm giả nội dung)."**

Cơ chế: thư viện ~150 file ảnh con dấu, so khớp ảnh, trả **2 field** vào mảng `fields`. **Quy tắc chung R5 của MBAL:** chỉ cần **một** chứng từ trong hồ sơ có dấu hiệu giả mạo → **cả bộ hồ sơ FAIL**.

**Dùng để làm gì:** ① làm **mẫu cho cách viết một dòng phạm vi** khi cam kết với khách — một câu, nêu rõ cái KHÔNG bao gồm; ② nếu đội muốn thêm một agent tín hiệu gian lận vào demo, **đây là mức phạm vi đã có người làm thật và biết là làm được**, thay vì hứa phát hiện cắt ghép.

**⚠ Nếu thêm vào demo, phải nói rõ đây là ngoài phạm vi Claim MVP.**

#### P.3.3 Số đo 83,49 % / 41,67 % trên BỘ TEMPLATE KHÁC

| Chỉ số | Giá trị | Nguồn và điều kiện |
|---|---|---|
| Độ chính xác **trường** | **83,49 %** | `04. Testing/Testing/MBAL- Test report.xlsx` — **15 chứng từ / 13 template, môi trường staging** |
| Độ chính xác **bảng** | **41,67 %** | cùng nguồn — **bảng kê 6 trang chỉ đọc được trang đầu** |
| Độ chính xác toàn bộ *(bộ đo khác, lớn hơn)* | **80,5 %** | 100 lượt, 2.413 field |
| **Bảng kê chi phí** *(bộ đo lớn)* | **60,9 %** — **thấp nhất trong mọi loại, do merge cell** | idem |
| Tóm tắt bệnh án | 91,1 % | idem |
| Phân loại đúng loại | 100 % (vòng 2) | vòng 1 sai 14/100, sửa prompt |
| Cần người can thiệp | **44,4 %** ở ngưỡng 95 % · **26,7 %** ở ngưỡng 85 % | idem |

> **Khoảng cách 83,49 % trường vs 41,67 % bảng là câu chuyện mạnh nhất mượn được**, và nó **trùng khớp với phát hiện kỹ thuật ở P.3.1**: comparator chỉ đọc `fields`, không đọc `tables`.

**Ba cách dùng đúng:**

1. **Cảnh báo trực tiếp cho `T1-01`.** Con số **60,9 %** là trên **bảng kê chi phí** — và `T1-01` là *"the single most important document; every payable cost line comes from it"*. **Chứng từ quan trọng nhất của Claim MVP là loại chứng từ có độ chính xác thấp nhất từng đo được.** Nguyên nhân đã biết: **merge cell** và **bảng nhiều trang chỉ đọc trang đầu**. **Phòng vệ phải dựng: cảnh báo khi số dòng đọc được < số trang × ngưỡng.**
2. **Đầu vào thô để chọn ngưỡng confidence.** A4 §7 nói ngưỡng phải đặt từ benchmark ground-truth; **đội chưa có benchmark**. 44,4 % / 26,7 % là mốc thô cho *"bao nhiêu phần trăm hồ sơ sẽ tới tay người"*.
3. **Mượn cách MBAL đóng khung cam kết:** cam kết hợp đồng **85 % (GĐ1) / 95 % (GĐ2)** — nhưng **chỉ áp cho tài liệu Nhóm 1** (CCCD, hộ chiếu, khai sinh — cấu trúc chuẩn, layout cố định). Nhóm 2 (bán cấu trúc, có viết tay) 85–94 %; Nhóm 3 (phi cấu trúc, ảnh điện thoại) 70–85 %. **Cách đóng khung này đáng học khi cam kết với khách.**

**⚠ Ba giới hạn phải nói kèm mọi lần trích những con số này:** ① đo trên **bộ template MBAL**, không phải trên `T1-01`…`T2-04`; ② **15 chứng từ / 13 template** là mẫu nhỏ; ③ trên **staging**, không phải production.

**Không dùng được từ QA của MBAL:** 48/48 test case Phase 1 ghi `Incomplete`; 52/61 Phase 2 `Untested`; `Bug_Management` Phase 2 trống. **Không có bộ QA hoàn chỉnh để trích dẫn ngoài hai bảng trên.**

#### P.3.4 Một thứ thứ tư nên mượn — ngưỡng theo TỪNG TRƯỜNG

Hạng mục 3 của Phase 2 MBAL: confidence từng trường **cộng ngưỡng khai báo theo từng loại chứng từ VÀ từng trường**, khai lúc tạo template. Ví dụ: *"Họ tên" ≥ 0,95* · *"Chẩn đoán" ≥ 0,90*.

> **MBAL thiết kế ngưỡng theo từng trường, không phải một ngưỡng chung cho cả hồ sơ.** Claim MVP §7 lấy **min trên tập field bắt buộc** làm claim confidence — hợp lý (một số tiền đọc sai không bị trung bình hóa đi) **nhưng thô hơn**: nó không phân biệt một `Số đăng ký thuốc` đọc kém với một `Thành tiền` đọc kém. **Đáng cân nhắc áp mô hình ngưỡng-theo-trường lên trên phép min của A4** — và đây là một cải tiến kể được trên slide, có nguồn thật đứng sau.

---

*Hết Build 03 v2.0. Mọi bảng field và mọi rule ở §1–§9 trích nguyên văn từ A4 v1.3, D2 v0.6 và D1 v0.5. Phụ lục P là tham chiếu MBAL-IDP, KHÔNG phải chuẩn.*
