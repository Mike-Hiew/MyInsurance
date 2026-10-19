# A4 — Document-Type & Extraction-Field Catalogue

**Project:** Digital Health Insurance Platform — MVP 1
**Document:** Annex A4 to the Solution Framework — the contract between the document layer and the claim engine
**Version:** 1.3
**Prepared by:** BnK Solutions — Business Analysis
**Date:** 25 August 2026 (v1.1, 14/08: tier-3 documents optional; tax-authority verification removed; category list aligned with A5. v1.2, 24/08: six line-level fields become consumed rather than merely extracted, two of them new; §3.1 adds the document-class crosswalk. **v1.3, 25/08: the term layer's two facts and its document type — `facilityCountry`, a per-line treatment-day count, and an eighth document type; `serviceCategory` opens to twenty; the `SUPPLEMENT` and `ADMIN` decline rows are withdrawn; header field 16 stops defaulting**)
**Primary audience:** the BnK IDP team
**Annex to:** *Solution Framework v2.5*
**Supersedes:** *A4 Document-Type & Extraction-Field Catalogue v1.2* (24 August 2026)
**Classification:** Internal

> ## ⚠ Read this before anything else — v1.2 changes what IDP is being asked for
>
> **This version adds to the ask, and §11 says an addition to this catalogue is a change request.** It is being flagged at the top rather than buried in §4.1 because **the IDP team has already confirmed capability against v1.1, and v1.1 is not what this document now says.**
>
> **What changed, exactly.** The console's cost-line grid now renders six columns it did not render before — register rows **S-15** and **S-16**, approved by the business owner. Of the six, **four already exist in this catalogue and were extracted but never consumed** (unit of measure, unit price, drug registration number, other-payer amount) and **two do not exist at all** (a line-level document date, and a line-level ICD-10 code). §4.1 now marks every line-item column with **whether it is expected from extraction or keyed by an assessor** — a column that was never there before, because until v1.2 the question never had two answers.
>
> **The four that exist are not a free ride either.** A field that is extracted and consumed by nobody has never had its accuracy measured. Two of them — **unit price (L6)** and **other source (L11)** — now feed something a person reads on a settlement, and an accuracy figure nobody has ever collected is not an accuracy figure.
>
> **Nothing in v1.2 is agreed until the IDP team answers §9's Q-IDP-01 to Q-IDP-04.** Where the answer is *no*, the column is **assessor-keyed** and the console says so at the field — which is a materially worse outcome than asking, and is why the questions are asked rather than assumed. **Open item D2-134** carries the same request from the specification side.

---

> ## ⚠ v1.3 adds to the ask again, and three of the additions are formal change requests under §11
>
> **v1.2 warned that the IDP team had confirmed capability against v1.1. That is still true, and v1.3 moves the line again.** It is flagged here rather than left in §4 and §9 because **nobody has put any of it to IDP.**
>
> | # | What is added | §11 status |
> |---|---|---|
> | **1** | **An eighth document type — a police or hospital statement (`T2-04`)**, tier 2, classification only. The pilot's one class-E contractual term requires it and **A4 carried no such type**, so D2 §5.29.2's `documentType` field names a value outside its own enumeration | **Change request.** A new document type lands in the crosswalk, the classification signature set, the ground-truth set and the checklist simultaneously |
> | **2** | **Four more `serviceCategory` values — `SCREENING`, `VACCINATION`, `OPTICAL`, `HEARING`.** A5 v1.5 §8.1 closes at **twenty**; §4.5 closed at sixteen. **IDP derives `serviceCategory` (`L13`, and assumption `I-2` says inside IDP), so IDP must derive four more values** | **Change request.** The keyword table is master data and ours; **the derivation is IDP's** |
> | **3** | **Two facts the term layer needs and this catalogue does not carry** — the **country of the treating facility** (document level) and a **treatment-day count per cost line**. Both are already vendor questions in the register, as `Q-IDP-05` and `Q-IDP-06`; **§9 stopped at `Q-IDP-04` and did not carry either of them** | **Change request** for the per-line count; the facility country is a new header field |
> | **4** | **`SUPPLEMENT` and `ADMIN` stop declining in §4.5.** Not an addition — a **withdrawal**, and it corrects a defect this catalogue was specifying: a decline at the wrong step, with the wrong code, citing no clause | Correction |
> | **5** | **Header field 16 stops defaulting to field 15.** A single line, and it is the one on this page that could cost money | Correction |
>
> **Items 1 to 3 are in the vendor register as `Q-IDP-05`, `Q-IDP-06`, `Q-IDP-07` and `Q-IDP-08`, with an owner and a cost column.** Until v1.3 two of the four had no register entry at all, which meant this catalogue was specifying work nobody had been asked to do — **which is exactly the failure §11 exists to prevent, arriving from the direction §11 does not watch.**

---

## 0. What this document is

This is the **contract between IDP and the claim engine**. It states, per document type: how the document is recognised, which fields are extracted, what each field must satisfy, and what happens when it cannot be read. Everything IDP builds for this project traces to a row in this catalogue, and everything the benefit engine consumes traces to the same rows.

It is deliberately narrow. The reference systems in this domain classify around twenty document classes; **MVP 1 handles eight** — seven to v1.2, plus `T2-04` — **of which only three require reading an image.**

---

## 1. Scope — three tiers

### 1.1 The tiers

| Tier | What IDP does | Types | Why |
|---|---|---|---|
| **Tier 1 — Extract** | Classify, then extract structured fields and tables | **4** (3 by OCR, 1 by XML) | These carry the money and the medical facts. Every number in the settlement comes from here |
| **Tier 2 — Classify only** | Recognise the document type; no field extraction | **4** *(3 to v1.2)* | **Confirms the insured event occurred and the required evidence exists.** No value is read from the page. Classification is far cheaper than extraction |
| **Tier 3 — Not handled** | Stored and attached; no AI | everything else | **Optional.** The customer is never required to upload a tier-3 document, and the claim is never held for one |

### 1.2 Tier 1 — full extraction

| ID | Document type (VI) | Document type (EN) | Method | Why it is here |
|---|---|---|---|---|
| **T1-01** | Bảng kê chi phí khám chữa bệnh | Itemised cost statement | OCR | **The single most important document.** Every payable cost line comes from it |
| **T1-02** | Đơn thuốc | Prescription | OCR | Drug names, strengths and quantities — the input to the drug-versus-diagnosis decline rules |
| **T1-03** | Phiếu khám bệnh (có chẩn đoán) | Outpatient examination record | OCR | Episode facts: patient, provider, date, diagnosis, ICD-10. **Load-bearing twice over**: the diagnosis drives the condition class (waiting periods) *and* is the input the clinical track reasons against |
| **T1-04** | Hóa đơn GTGT / hóa đơn bán hàng | VAT / sales invoice | **XML only** | Issuer, tax code, invoice identity, totals — for verification and fraud control |

### 1.3 Tier 2 — classification only

| ID | Document type (VI) | Document type (EN) | Purpose |
|---|---|---|---|
| **T2-01** | Giấy tờ tùy thân (CCCD / Hộ chiếu / Giấy khai sinh) | Identity document | Checklist completeness; identity is already held on the policy |
| **T2-02** | Kết quả cận lâm sàng | Laboratory and imaging result | Checklist completeness; supports the "test unrelated to diagnosis" decline |
| **T2-03** | Báo cáo y tế / Giấy chứng nhận phẫu thuật | Medical report / surgery certificate | Checklist completeness |
| **T2-04** | **Biên bản / xác nhận về hoàn cảnh xảy ra sự cố** *(provisional VI name — see below)* | **Statement of the circumstances of an incident** — a police report, a hospital admission statement, or both | **New at v1.3.** Checklist completeness, and it is the only document type in this catalogue that exists because a **contractual term** asks for it rather than because an episode type does. D1 v0.5 **`BR-TERM-11`** |

> **`T2-04` is added with its naming and its default level deliberately unsettled, and that is not laziness.** The pilot's one **class-E** term — decidable only if a specific named document says so — requires a **police or hospital statement** on an accident claim, and until v1.3 **A4 carried no such type at all**, so D2 §5.29.2's `documentType` field named a value outside its own enumeration and the term stayed `DRAFT`. **The type is added so the enumeration is honest.** **What is not settled, and is the business's to settle — D1 v0.5 `B-24`:** whether the requirement is a **police report**, a **hospital admission statement**, or **both as two separate types**; the **Vietnamese and English names** of each; whether it is **`REQUIRED` or `EXPECTED`** by default on an accident claim *outside* any term; and whether an **original** is needed. **The default level is deliberately absent from §6's table below**, because a type that appears at `REQUIRED` for every accident claim is a materially different ask from one that only ever arrives through a term. **Until `B-24` is answered, `T2-04` reaches a claim only through `BR-TERM-11`** — a term names it, the checklist gains a `REQUIRED` row and a checkpoint opens — **and it appears in no episode-type row.** That is a working state, not a workaround.

### 1.4 Deliberately excluded, and why

| Excluded | Reason |
|---|---|
| **Giấy yêu cầu bồi thường** (claim request letter) | **The mobile app replaces it.** Claimant, relationship, payout account and amount claimed are captured as form fields. There is no reason to OCR a document we generated the data for |
| **Giấy ra viện** (discharge form) | Inpatient episodes are handled with **manual entry of episode facts** in MVP 1 — see §1.6 |
| **Sổ khám bệnh** (examination book) | Handwritten; the hardest document in the set; its content duplicates the examination record |
| **Phiếu điều trị răng** (dental treatment record) | Dental claims use the cost statement and examination record like any other |
| **Biên bản tai nạn** (accident report) | Accident claims are in scope; the report is a Tier 3 attachment reviewed by the assessor |
| **Giấy chứng nhận bảo lãnh** and everything in the credit and death/disability sections | The corresponding claim types are out of MVP scope |

### 1.5 The e-invoice decision and what follows from it

**Decision: the VAT invoice is consumed as structured e-invoice XML only. There is no OCR path for invoice images.**

This is a real reduction — the highest-value document is read with perfect accuracy and no model risk — but it has three consequences that must be designed, not discovered:

1. **The cost statement, not the invoice, is the source of cost lines.** This is already true in practice: the `bảng kê` carries the itemisation; the invoice carries the header and totals. Adjudication therefore does not stop if the XML is absent.
2. **If no XML is supplied**, the claim proceeds on the cost statement alone, the invoice checklist item is marked received-but-unverified, and reconciliation rule **R6** (§5) is skipped with a warning. The assessor may key the invoice header manually. This must not be a blocking error.
3. **There is no tax-authority verification in this project.** Cross-checking an invoice against the tax authority (lookup by tax code + serial + invoice number) is out of scope and is not planned for MVP 2 either — it is noted only because the reference system has it. The XML's own tax-authority code (`Mã CQT`) and signature block are captured because they arrive free with the parse; nothing consumes them.

> **Confirmed 17/08/2026 (D1 B-08): the XML-only decision stands**, whatever the availability turns out to be. A claim without XML still settles — consequence 1 above is what makes that true — so low availability costs the R6 reconciliation, not the settlement. **What it does change is the ground-truth set: the 10 invoice XMLs in §10 become non-negotiable**, sourced from providers who do issue them if the pilot providers do not. An untested parser is the real loss.

### 1.6 The inpatient consequence — stated so it is a decision, not a surprise

Dropping the discharge form from Tier 1 means **inpatient episode facts are keyed by the assessor** rather than extracted. Inpatient claims still work end to end: the cost statement is extracted normally, the benefit tree still applies room-and-board per-day caps and the 60-day count, and the settlement is computed as usual.

What changes is the measurement. **SC-01 (≥80% of mandatory fields auto-populated) and SC-02 (accuracy) are measured on outpatient claims only.** Inpatient claims are a supported path, not an automated one, in MVP 1.

*Recommended and assumed unless corrected: keep inpatient claims in scope on the manual-entry path, and add T1-05 Giấy ra viện in Phase 2.*

---

## 2. Common conventions

Applied to every type in this catalogue.

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

---

## 3. Tier 1 — classification signatures

Classification runs before extraction. Each type is recognised by a **signature**: required markers, supporting markers, and a match threshold. A page that matches nothing lands in `unclassified` and is surfaced for the assessor — it is never forced into the nearest class.

| ID | Required markers (any one) | Supporting markers (≥2 of) | Threshold | Layout cue |
|---|---|---|---|---|
| **T1-01** | `BẢNG KÊ CHI PHÍ`, `BẢNG KÊ CHI TIẾT CHI PHÍ`, `CHI PHÍ KHÁM CHỮA BỆNH` | `Đơn giá`, `Thành tiền`, `Số lượng`, `ĐVT`, `BHYT`, `Người bệnh tự trả`, `Tổng cộng` | 50% of supporting | A multi-column table occupying most of the page |
| **T1-02** | `ĐƠN THUỐC`, `PHIẾU LĨNH THUỐC` | `Liều dùng`, `Cách dùng`, `Hàm lượng`, `Bác sĩ kê đơn`, `Chẩn đoán`, `Số lượng` | 50% | Numbered drug lines with a dosage sentence beneath each |
| **T1-03** | `PHIẾU KHÁM BỆNH`, `PHIẾU KHÁM`, `SỔ KHÁM BỆNH` | `Lý do khám`, `Chẩn đoán`, `Khám lâm sàng`, `Hướng điều trị`, `Bác sĩ khám`, `ICD` | 50% | Form-style label/value pairs, few or no table rows |
| **T1-04** | XML root `HDon` / `TDLap` present | `MSTNBan`, `KHHDon`, `SHDon`, `NLap` | n/a — schema validation | Not applicable |

> **`T2-04` has no signature row above, and that is a stated gap rather than an omission.** The four rows in this table are the **tier-1** types, which are the only ones extraction reads. **Tier 2 is classified and not extracted**, and its signatures have never been written here — `T2-01` to `T2-03` do not appear either. **`T2-04` differs from the other three in one respect that matters:** they are documents this catalogue has always expected, and a classifier trained on Vietnamese medical packages will have seen them. **A police report is not a medical document at all**, so it is the one tier-2 type whose signature and training samples cannot be assumed to fall out of the existing set. That is asked as **`Q-IDP-07`** in the vendor register, together with its crosswalk row and its ground-truth count.

**Conflict rule.** Where two signatures both pass, the one with the higher match ratio wins, and the competing class is recorded on the document. **A cost statement and an invoice image on the same page is the known ambiguous case** — resolved in favour of the cost statement, because the invoice is not read from images at all in MVP 1.

**Page splitting.** A single uploaded file may contain several documents. IDP splits by document boundary before classifying. A multi-page cost statement stays one document; its line items concatenate in page order.

### 3.1 The document-class crosswalk

**New at v1.2, register row B-18. Extended to eight at v1.3.** This catalogue defines eight document types, `T1-01` to `T2-04`. **IDP returns a `classification` value from its own vocabulary**, which is not this one and does not have to be — a document intelligence platform classifies documents for every customer it has, not for ours. **What has been missing is the mapping between the two**, written down in one place with an owner.

**The absence was not theoretical.** D3 §I-09 carries `classification` in the result payload and this catalogue names seven types, and **nothing in the document set said how one becomes the other.** The translation was going to be written, once, by whoever built the adapter, from a conversation nobody recorded — and the first time a partner class was renamed, nobody would know which of our seven stopped arriving.

| Our type | Partner classification value(s) | Cardinality | Note |
|---|---|---|---|
| `T1-01` Bảng kê chi phí | *(to be supplied by IDP)* | **One-to-many** | The reference maps a single checklist row to several partner classes, and we should expect the same: a cost statement, a detailed cost statement and a pharmacy cost statement may be three classes to a partner and one type to us |
| `T1-02` Đơn thuốc | *(to be supplied by IDP)* | One-to-many | Prescription and dispensing slip are commonly separate classes |
| `T1-03` Phiếu khám bệnh | *(to be supplied by IDP)* | One-to-many | |
| `T1-04` Hóa đơn GTGT | *(to be supplied by IDP)* | One-to-one | **XML only** (§1.5). The mapping exists for completeness; the path is schema validation, not classification |
| `T2-01` Giấy tờ tùy thân | *(to be supplied by IDP)* | One-to-many | CCCD, passport and birth certificate are almost certainly three partner classes and are one type here |
| `T2-02` Kết quả cận lâm sàng | *(to be supplied by IDP)* | One-to-many | Laboratory and imaging are likely separate |
| `T2-03` Báo cáo y tế / GCN phẫu thuật | *(to be supplied by IDP)* | One-to-many | |
| **`T2-04`** Biên bản / xác nhận hoàn cảnh sự cố | *(to be supplied by IDP)* | **One-to-many** | **New at v1.3, and it is the row least likely to have an answer.** A police report, a hospital admission statement and a hospital discharge summary may be three partner classes, one, or none — **this is not a medical document and the partner's medical vocabulary may not reach it at all.** Where the answer is *no such class exists*, X-2 applies and every one of these arrives as `unclassified`, which is safe and is also the whole checklist mechanism `BR-TERM-11` depends on failing quietly. **Asked as `Q-IDP-07`** |
| `unclassified` | Everything else, **including every partner class not on this table** | — | **The default is `unclassified` and never the nearest match.** See the rules below |

**The five rules that make this a contract rather than a lookup table.**

| # | Rule |
|---|---|
| **X-1** | **The mapping is many-to-one, never one-to-many in our direction.** Several partner classes may resolve to one of our types; **one partner class must never resolve to two of ours**, because the checklist would then not know which item was satisfied |
| **X-2** | **A partner class not on this table resolves to `unclassified`**, is surfaced to the assessor, and **never** to the nearest-looking type. A document silently filed as the wrong type satisfies a checklist item it did not satisfy, and a claim then settles on evidence that is not there |
| **X-3** | **The mapping is maintained data, not code** — it lives with the document-type catalogue under F55 and changes without a release. **The partner will rename and add classes and will not tell us in advance**, which is precisely why this must not be a constant in an adapter |
| **X-4** | **The partner's raw value is retained on the document alongside our type.** When a mapping is later found to be wrong, the documents already processed can be re-mapped from the stored value; without it they have to be re-classified, which means re-processing |
| **X-5** | **A change to this table is a change request under §11**, on the same footing as adding a document type — because it changes which documents satisfy which checklist item, and therefore which claims proceed |

> **The right-hand column is empty and that is the honest state of it.** **Only the IDP team can populate it** — it is their vocabulary. §9's **Q-IDP-01** asks for it. **Until it is filled, `classification` is being consumed on an assumed mapping**, and an assumed mapping is a defect waiting for its first rename.

---

## 4. Tier 1 — field catalogues

### 4.1 T1-01 — Bảng kê chi phí khám chữa bệnh (itemised cost statement)

**Header fields**

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
| 14 | Mã ICD-10 | ICD-10 code | text | O | Pattern `[A-Z][0-9]{2}(\\.[0-9])?`; validated against the ICD-10 catalogue |
| 15 | Ngày khám / điều trị — từ | Treatment date from | date | M | |
| 16 | Ngày khám / điều trị — đến | Treatment date to | date | O | **`null` where the page does not carry it. Never defaulted to field 15 — corrected at v1.3, and see the block below** |
| 17 | Hình thức điều trị | Treatment setting | enum | D | `Ngoại trú` \| `Nội trú` — derived from markers; drives the benefit mapping |
| **18** | **Quốc gia của cơ sở y tế** | **Facility country** | **text / ISO-3166 alpha-2** | **O** | **New at v1.3.** The country in which treatment was given, at **document level** — not per line. Derived from the printed facility address where the address makes it derivable, **and `null` otherwise**, never inferred from the facility's name. `VN` is not a default: an unstated country is `null`. **This is `Q-IDP-05`** |

> **Field 16 must not default, and this is the correction on this page that could cost money.** v1.2 read *"Defaults to field 15 for a single-visit statement."* **This catalogue states the opposite principle two rows down at `L15`** — *a value that was inferred must not look like one that was read* — and it is right there and wrong here. **The failure is concrete.** D2 §5.7.2's `F17` derives the **treatment-day count** from this range, counted inclusive of both ends, and chain **step 9** tests that count against a benefit's day limit. **A failed "to" extraction on an inpatient stay silently becomes one day**, so a fourteen-night admission is priced as a single day, the day limit passes trivially, and **nothing anywhere reports that a number was manufactured.** A default is indistinguishable from a reading once it is in the payload. **From v1.3: `null` on absence, with provenance recorded as absent, and the claim engine decides what to do about it** — which for an inpatient episode is to hold the step rather than pass it (D1 `BR-CHAIN-1`). **The single-visit case that motivated the default is handled where it belongs**: a statement with one treatment date and one line is a one-day event, and that is the *engine's* inference to make and to record as an inference, not extraction's to make and record as a reading.

> **Field 18 is one of the two facts the term layer asked for and this catalogue did not carry.** A1 §4.11 publishes a **closed vocabulary** of facts a contractual term is permitted to test, and `facilityCountry` is in it. The pilot product's exclusion **#10 — treatment outside Vietnamese territory** — selects on it and on nothing else. **Field 3 is a free-text provider address and is optional**, so today there is no fact behind the exclusion at all. **What is explicitly not permitted is guessing the country from the facility's name**, which is what an implementer reaching for a fallback would reach for: §2's *never infer* forbids it, and a name-based guess would raise a checkpoint on every provider whose name reads foreign. **See `Q-IDP-05`, and note that the exclusion is `DRAFT` today** — the vendor register carries what that actually means, which is not what the register said before v1.4.

**Line items** — the core table.

**The `Prov` column is new at v1.2 and it is the one to read.** It says where the value is expected to come from, and it exists because six of these columns are now **rendered on the assessor's grid** (D2 §5.6.3) where before they were either invisible or absent. A column a person reads must have an answer to *who put this here*.

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
| **L16** | **Mã ICD-10 (theo dòng)** | **ICD-10 code (line level)** | text | **O** | **X?** | **New at v1.2** (register **S-16**). The diagnosis code printed against **this line**, where the statement carries one. Pattern as header field 14, validated against the ICD-10 catalogue. **Two things now depend on this field and v1.2 described only the first.** **(1) The clinical track**, which currently reasons a *"not related to the diagnosis"* recommendation against the **event's** diagnosis, so on a claim carrying two diagnoses it argues against the wrong one. **(2) Two contractual exclusions, added at v1.3** — A5 §7's **#5** and **#9**, the congenital-condition exclusion (which tests chapter Q of the ICD) and the infertility / assisted-reproduction / contraception / termination group. **Both are `DRAFT` today because this field does not exist**, and the two consequences are different in kind rather than in degree: **a clinical track with a coarser input still produces a recommendation a person reviews; an exclusion that cannot be evaluated produces nothing at all**, and the customer is paid for something the contract excludes. **Genuinely uncertain whether Vietnamese cost statements print it at line level often enough to be worth extracting — that is question `Q-IDP-02`, with `L15`, and it is a real question, not a formality** |
| **L18** | **Số ngày điều trị (theo dòng)** | **Treatment days (line level)** | int | **O** | **X?** | **New at v1.3.** The number of days **this cost line** covers, where the line is priced per day — a room charge, a bed charge, an ICU day. **`null` where the line is not day-priced or the page does not carry it**, never derived from `L5` quantity, which counts units of an unspecified thing. **This is `Q-IDP-06`.** Today the only day count in the system is **per insured event**, derived from header fields 15 and 16 (D2 §5.7.2 `F17`), and **chain step 9 tests a `DAY` counter that nothing populates per line** — which is how the limit panel came to render a *"số ngày còn lại"* figure that no rule produces. **`L17` is deliberately not used**: D2 §5.6.3's cost-line grid already cites `L17` for the insured-event allocation, and a second `L17` meaning something else would make the token ambiguous across the set. §4.1's own rule — *a field number is an address, not a position* — cuts both ways |
| L13 | Danh mục dịch vụ | Service category | enum | **D** | D | **Derived by IDP.** The value the benefit mapping consumes — see §4.5 |
| L14 | Số tiền yêu cầu | Amount claimed | int | **D** | D | `L7 − L8`, floored at 0. The amount presented to the benefit engine |

> **L15 and L16 keep their numbers out of sequence on purpose.** L13 and L14 are **derived** and have been cited by number in D2, A5 and the result payload since v1.0. **A field number is an address, not a position** — renumbering to make the table read tidily would silently re-point every existing citation, which is exactly the class of defect A1 exists to prevent.

> **What the reference does with the *related diagnosis*, and why we stop short.** Reference System A carries **two** ICD columns on each cost line: the code extracted from that line, and a separate *related diagnosis* an assessor sets. **We take the first and not the second.** The extracted code has a document behind it. The related diagnosis has nothing behind it but a keystroke on every line of every claim, and **it would be read by nothing** — no rule consumes it and the clinical track reasons from the extracted code. A mandatory keystroke that feeds no decision is how a grid becomes something assessors click past.

**Footer fields:** `Tổng cộng` (total, M) · `Tổng BHYT chi trả` (O) · `Tổng người bệnh tự trả` (O).

**Document-level validations**

| Code | Rule | Action |
|---|---|---|
| V-01-1 | Σ L7 over all lines = footer `Tổng cộng` (±1 VND) | Warning, anchored on the footer |
| V-01-2 | Σ (L8 + L9 + L10 + L11) = L7 per line | Warning on the line |
| V-01-3 | At least one line item present | **Error** — the document is unusable |
| V-01-4 | Treatment dates fall inside the member's coverage period | Warning; the claim engine decides |

**Known difficulties, to be sized by the IDP team:** two-row grouped headers with a `Yêu cầu bồi thường` / `Bảo hiểm chi trả` span; provider-specific column ordering; statements that run to several pages with a repeated header; handwritten annotations in the margin; and drug lines whose names abbreviate the active ingredient.

### 4.2 T1-02 — Đơn thuốc (prescription)

**This document carries no money.** Its sole purpose is to give the decline rules the drug identity, and to establish the prescriber's diagnosis. Extraction accuracy on names matters more than on numbers.

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

**Drug lines**

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

**Validation:** V-02-1 at least one drug line, else the document is unusable. V-02-2 prescription date within the episode dates → warning.

### 4.3 T1-03 — Phiếu khám bệnh (outpatient examination record)

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

### 4.4 T1-04 — Hóa đơn GTGT, from e-invoice XML

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

**Validation:** V-04-1 the XML validates against the published schema, else the document is rejected with a clear message. V-04-2 `TgTCThue + TgTThue = TgTTTBSo` (±1) → warning.

### 4.5 Derived field — service category (L13)

This is the field the benefit mapping in the Product Data Sheet §8.1 consumes, so it must be produced deterministically and be configurable.

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

*Twenty categories, identical to the mapping table in A5 §8.1. Where the episode loss type is accident, every mapped benefit is redirected to 500.*

> **The `SUPPLEMENT` and `ADMIN` rows stopped declining at v1.3, and A5 v1.5 §7.4 is where the reasoning lives.** Both read **declined `NOTCOV`** in v1.1 and v1.2. **Four things were wrong with that and this catalogue was the document specifying it.** **(1) The wrong step.** A decline on cover is chain step 4's or step 6's; §4.5 is a **mapping** — its job is to say *where a covered line lands*, and a category that declines has put the cover decision inside the mapping table. **(2) The wrong code.** `NOTCOV` means *not a covered medical item*; a dietary supplement excluded by the wording is excluded by a **term**, and the code for that is `EXCL`. **(3) No clause.** A decline produced here cites nothing, and D1 `BR-MSG-3` requires the customer to be told which term was relied on. **(4) Unconditionally** — and this is the one that arrives as a complaint. The wording's own exclusion reads *"dietary supplements … **not prescribed as treatment for the diagnosed condition**"*, and §4.5 was declining **every** supplement line including a prescribed one. **What replaces it:** `SUPPLEMENT` maps to `200.2` outpatient and `100` inpatient — where a prescribed supplement would in fact be paid — and the term register's `TERM-HC-003` decides at step 4 whether the exclusion bites, raising a **checkpoint** because the proviso *"prescribed as treatment"* has no fact behind it and a person has to decide. **`ADMIN` maps to nothing**, and a line with no mapping falls to step 6 and is declined `NOTCOV` there — *not a covered medical item*, which for a photocopying fee is the correct code applied at the correct step.

> **Four categories were added at v1.3 and they are not free to IDP, whatever A5 says about them.** A5 v1.5 records `SCREENING`, `VACCINATION`, `OPTICAL` and `HEARING` as *"Claim Platform master data, ours to define — no vendor question, no dependency, one seed change"*, and **for the benefit mapping that is exactly right**: the rows above are ours and the keyword table is maintained without a release. **But `L13` is derived by IDP** — that is this catalogue's `D` status and assumption **`I-2`** — so **somebody has to teach the derivation four new values**, and that somebody is IDP. **The two statements are not in conflict; they are about different halves of the same field**, and the half A5 does not own is the half nobody had asked for. **It is a change request under §11 and it is `Q-IDP-08` in the vendor register.** Where the answer is *not in this phase*, the four values are **assessor-keyed at `L13`** like any other unanswered `X?`, and the two exclusions they unlock — A5 §7's #2 and #4 — stay at a checkpoint rather than reaching class `D`.

**The category list and its keywords are reference data, editable at run time** (`REQ`: the service-category rule table must be maintained without a release). **The derivation that produces `L13` from a printed item name is not** — it is IDP's, and it changes by change request.

---

## 5. Cross-document reconciliation

Run after extraction, across every document in the package. **Nothing is auto-corrected**; each rule produces a labelled outcome anchored at the field.

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

**Design rule, inherited from the reference systems and worth restating: where IDP's determination and the document disagree, IDP's determination is used and a warning is raised. Nothing is silently corrected and nothing is inferred.**

---

## 6. Package completeness state

Produced by R9 and returned with the result. It is what drives the claim's `PENDING_INFO` transition, so the claim engine consumes the state rather than re-deriving it.

| State | Meaning |
|---|---|
| `COMPLETE` | Every required document type for this episode type is present and extracted |
| `MISSING_DOCUMENT` | A required type is absent — the response names which |
| `MISSING_FIELD` | All types present, but one or more mandatory fields could not be read — the response names field and document |
| `INCONSISTENT` | Present and readable, but one or more reconciliation rules failed |
| `AWAITING_ORIGINAL` | Digitally complete, awaiting a hard-copy original |
| `ERROR` | Terminal processing failure |

**Required types by episode type (MVP 1)**

| Episode type | Required (claim cannot settle without) | Expected (requested if absent) | Optional (never requested) |
|---|---|---|---|
| Outpatient — illness | T1-01, T1-03 | T1-04, **T1-02** | T2-01, T2-02, T2-03, tier 3 |
| Outpatient — with medication | T1-01, T1-03, T1-02 | T1-04 | T2-01, T2-02, T2-03, tier 3 |
| Dental | T1-01, T1-03 | T1-04 | T2-01, tier 3 |
| Accident | T1-01, T1-03 | T1-04, T2-02 | T1-02, T2-01, tier 3 accident report |
| Inpatient *(manual episode path)* | T1-01 | T1-04, T2-03 | T1-02, T2-01, T2-02, tier 3 |
| Maternity | T1-01, T1-03 | T1-04, T2-03 | T1-02, T2-01, T2-02, tier 3 |

**Three levels, not two.** *Required* blocks settlement. *Expected* triggers an information request to the customer, which the assessor may waive. *Optional* is never requested and never blocks — the customer uploads it only if they want to.

> **Changed 17/08/2026 (D1 B-02): `T1-02` prescription moves from Optional to Expected for outpatient-illness.** The clinical appropriateness track (D1 §5.10) reasons about drugs against the diagnosis, and it has nothing to work with when the prescription is absent. *Expected* asks for it without stalling a visit that legitimately did not produce one — which *Required* would do. **No tier-3 document appears in any Required or Expected column**, which is what makes tier 3 genuinely optional rather than nominally optional.

---

## 7. Confidence, and what it is used for

| Item | Rule |
|---|---|
| Per-field confidence | Returned for every extracted field and every table cell |
| Row confidence | The minimum confidence among that row's cells |
| **Mandatory field set** | Every field marked `M` in §4, plus every `M` column of a line item |
| **Claim-level extraction confidence** | The **minimum** confidence across the mandatory field set — deliberately the minimum, not the mean, so one badly-read amount cannot be averaged away |
| **What it drives** | **Assessor routing, not the auto-approval gate.** It decides which claims and which fields an assessor is pointed at first. **D1 §5.8's gate tests three conditions and confidence is not one of them** — that omission is recorded there, deliberately. The threshold value is set from the ground-truth benchmark, not guessed |
| Below threshold | Routes to an assessor with the **specific field** named — not a generic "needs review" |

---

## 8. Result payload

Returned by `GET /v1/extraction-requests/{requestId}/result` (I-09).

> **This section is the canonical definition of the result payload.** D3 §I-09 specifies the transport rules around it — retention, immutability, not-ready behaviour, size — and deliberately does not restate the shape. A field is added or changed here, once. (Earlier drafts of this section deferred the schema to D3 while D3 deferred it back here; that circle is closed as of 17/08/2026 in favour of A4.)

```jsonc
{
  "requestId": "…",
  "claimNo": "CLM-26-000417",            // one name per concept — D3 §1.2
  "state": "COMPLETE | MISSING_DOCUMENT | MISSING_FIELD | INCONSISTENT | AWAITING_ORIGINAL | ERROR",
  "confidence": 0.94,                    // minimum over the mandatory field set
  "documents": [
    {
      "documentId": "…", "type": "T1-01", "pages": [1,2],
      "classification": { "confidence": 0.99, "competingType": null,
                          "partnerClass": "…" },   // raw partner value — §3.1 rule X-4
      "fields": [
        { "name": "providerName", "value": "…", "confidence": 0.97,
          "page": 1, "bbox": [x,y,w,h], "status": "M" }
      ],
      "lines": [
        { "index": 1, "itemName": "…", "quantity": 1, "unitPrice": 300000,
          "unitOfMeasure": "lần",              // L4  — new in the payload at v1.2
          "lineAmount": 300000, "paidBySocialInsurance": 0,
          "otherPayerAmount": 0,               // L11 — new in the payload at v1.2
          "drugRegistrationNo": null,          // L12 — new in the payload at v1.2
          "documentDate": null,                // L15 — new field, Q-IDP-02
          "icd10": null,                       // L16 — new field, Q-IDP-02
          "serviceCategory": "CONSULT", "amountClaimed": 300000,
          "confidence": 0.95, "page": 1 }
      ]
    }
  ],
  "warnings": [
    { "rule": "R6", "severity": "warning",
      "message": "Invoice total does not match the cost statement total",
      "documentId": "…", "field": "totalAfterVat" }
  ],
  "missing": { "documentTypes": [], "fields": [] }
}
```

**Two properties the claim engine depends on:** every warning names the document and field it attaches to, so the review UI can anchor it; and `amountClaimed` is present on every line, so the benefit engine never has to recompute it.

---

## 9. What the IDP team is being asked to confirm

| # | Item |
|---|---|
| **I-1** | Effort and elapsed time to build classification and extraction for **T1-01, T1-02, T1-03** and classification for **T2-01, T2-02, T2-03** |
| **I-2** | Whether the derived fields — service category (L13), item class (D9), treatment setting (header 17) — are produced inside IDP or by the claim engine. **Our assumption: inside IDP**, because they need the document context |
| **I-3** | Feasibility and effort of the **e-invoice XML** path, and whether the parser can be reused rather than built |
| **I-4** | Whether per-field confidence and per-cell table confidence are both available in the result payload |
| **I-5** | Realistic accuracy expectation per document type, separating printed from handwritten content |
| **I-6** | Whether page splitting of a mixed multi-document upload is available, and how a multi-page cost statement is kept as one document |
| **I-7** | Whether the reconciliation rules in §5 run inside IDP or in the claim engine. **Our assumption: inside IDP**, because they are cross-document and IDP holds all the documents |
| **I-8** | The ground-truth requirement in §10 — is it sufficient, and who sources it |

**Four more, added at v1.2, and they are a different kind of question from I-1 to I-8.** Those eight asked *can you build what we specified*. **These four ask whether something we have already put on a screen actually exists**, and until they are answered the affected columns carry a visible *"source not confirmed"* marker on the assessor's grid.

| # | Item |
|---|---|
| **Q-IDP-01** | **The document-class crosswalk — §3.1's empty column.** For each of our **eight** types, which partner `classification` values map to it? **Eight from v1.3** — `T2-04` is new and is the row least likely to have an answer, which is why `Q-IDP-07` asks it separately rather than letting it disappear into a table of eight. **This is the one that blocks rather than degrades**: `classification` is consumed today on an assumed mapping, and a wrong assumption files a document as a type it is not, which satisfies a checklist item nothing satisfied. **Please also state how the partner notifies a class rename or addition** — rule X-3 exists because we assume it does not |
| **Q-IDP-02** | **Are `L15` (line-level document date) and `L16` (line-level ICD-10) extractable from a Vietnamese itemised cost statement — and how often do providers actually print them at line level?** Both are **new asks**, neither is in the v1.1 scope the team confirmed against, and **frequency matters as much as feasibility**: a field printed on one statement in twenty is not worth a model. Where the answer is no, or rarely, the column is assessor-keyed and the console says so — **and for L16 that means the clinical track keeps reasoning against the event-level diagnosis on multi-diagnosis claims**, which is the cost of the *no* |
| **Q-IDP-03** | **What accuracy should we expect on `L4` (unit of measure), `L6` (unit price), `L11` (other-payer share) and `L12` (drug registration number)?** All four have been extracted since v1.0 and **consumed by nothing**, so no accuracy figure has ever been collected for them. **From v1.2 an assessor reads all four**, and two of them sit beside money. **If they were never annotated in the ground-truth set, say so** — that is a gap in §10, not a gap in the model, and it is cheap to close before annotation starts and expensive after |
| **Q-IDP-04** | **Does the result payload carry a per-line `documentDate` and a per-line `icd10`, and can `unitOfMeasure`, `drugRegistrationNo` and `otherPayerAmount` be added to the `lines` array?** §8's payload is amended in this version on the assumption that they can. **If the line object's shape is fixed, tell us now** — the alternative is a second call or a side-channel, and both are worse than knowing |

> **What we are not asking for, recorded so the scope of the addition is unambiguous.** No new document type. No new reconciliation rule. No change to `serviceCategory` or to any derived field. **No `related diagnosis` field** — §4.1's note explains why the reference's second ICD column is not adopted. The addition is **two new line-level fields and one crosswalk**, and everything else in this version is a change to what we *do* with values that were already being returned.

---

## 10. Ground-truth requirement — revised down

The reduction from nine types to seven, and from four OCR types to three, changes what has to be collected.

| Type | Samples needed | Providers | Note |
|---|---|---|---|
| T1-01 Cost statement | **40** | ≥ 4 | The highest layout variance in the set; the single most important number in the estimate |
| T1-02 Prescription | 25 | ≥ 3 | Include at least 5 with handwritten content |
| T1-03 Examination record | 25 | ≥ 3 | Include at least 5 with handwritten diagnosis |
| T1-04 Invoice XML | 10 | ≥ 3 | Structured; needed for parser coverage, not model training. **Non-negotiable (B-08, 17/08/2026):** this is the only path by which the XML parser is tested. If the pilot providers rarely issue compliant e-invoices, these 10 must be sourced from providers who do — reducing the count leaves the path untested, and §1.5's XML-only decision then rests on nothing |
| T2-01/02/03 Classification | 15 each | ≥ 3 | Classification only — no field annotation required |

**Roughly 25–30 complete anonymised claim packages** yield these counts, since one package typically contains a cost statement, a prescription, an examination record and several Tier 2 documents. That is materially less than the 30–50 packages required under the nine-type scope, and it is the direct saving from this decision.

**Annotation effort is concentrated on T1-01.** Field-level annotation is required for the three OCR types; Tier 2 needs only a type label per document.

> **v1.2 changes what "field-level annotation" means for T1-01, and it changes it before annotation starts rather than after — which is the only reason it is cheap.** The forty cost statements must now be annotated for **`L15` and `L16` where the provider prints them**, and — importantly — **the absence must be annotated too**: *how many of the forty carry a per-line date at all* is the answer to question **Q-IDP-02**, and it can only come from the ground-truth set. **The same set answers Q-IDP-03**, since `L4`, `L6`, `L11` and `L12` have never had an accuracy figure and one cannot be produced without annotated truth to measure against. **If any of those four was skipped during annotation because nothing consumed it, say so now.** Adding four columns to forty documents before annotation begins is an afternoon; adding them afterwards is a second pass over the whole set.

> **Precondition — anonymisation.** Every document entering the ground-truth set must be **anonymised before it reaches the project**: patient name, date of birth, identity number, patient code, social-insurance card number, address and phone removed or replaced. Layout, service names, amounts, dates and diagnoses are retained — they are what the model learns. This is not a compliance formality: the decision to defer legal review to MVP 2 (Framework §9.4) rests on the MVP never processing identifiable patient data, and the ground-truth set is the one place that assumption could quietly break.

---

## 11. Change control

This catalogue is the contract. **Adding a document type, a mandatory field or a reconciliation rule is a change request**, because each one lands in the ground-truth set, the extraction model, the result payload, the claim engine's consumption and the test suite simultaneously.

Two changes are already anticipated and should be planned rather than treated as scope creep:

| Anticipated | Trigger |
|---|---|
| **T1-05 Giấy ra viện** (discharge form) | Inpatient claims moving to the automated path — Phase 2 |
| *(removed)* | Tax-authority invoice verification was considered and is **out of scope for this project**, not deferred. Now also declared at Framework **S-27** |
| **A per-insured-event document checklist** | §6's required-types table is keyed on episode type and produces **one checklist per claim**. Framework **S-38** declares the per-event version out; when the app can lodge several visits on one claim, this table is re-keyed and A2's supplementary-document messages change with it. **Anticipate it; do not build it early** |

**Two things v1.2 adds to change control, and both are the same lesson from different ends.**

**The document-class crosswalk (§3.1) is under change control from the day it is populated**, on the same footing as a document type — because it decides which documents satisfy which checklist item, and therefore which claims proceed. **It is also the entry most likely to change without us initiating it**, since the partner may rename or add a class; rule X-3 makes it maintained data and rule X-4 retains the raw value so a re-map does not mean re-processing.

**A field that is extracted but consumed by nothing is not free, and v1.2 is the bill.** Four columns sat in this catalogue since v1.0, correctly extracted, read by no one, and therefore **never measured**. The moment a screen rendered them they became four accuracy figures nobody has. **Keep extracting fields ahead of need — L8's note argues for exactly that and it is right — but record which of them have been validated and which have merely been collected**, because the two look identical in a field catalogue and are not the same thing at all.

*End of catalogue v1.3.*
