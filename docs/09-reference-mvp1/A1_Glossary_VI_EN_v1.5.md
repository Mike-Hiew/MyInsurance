# A1 — Glossary and Canonical Term List (VI ↔ EN)

**Project:** Digital Health Insurance Platform — MVP 1
**Document type:** Annex A1 — the one place a term is defined
**Version:** 1.5
**Prepared by:** BnK Solutions — Business Analysis
**Date:** 24 August 2026 (17/08: written. 18/08: re-baselined against Framework v1.5 — no term changes. 21/08: realigned to Framework v2.0 — one term retired, seven added, three restated. 24/08: the value-set register opens at §4.1; four new sets registered and thirteen homeless ones brought in; four terms added. **24/08 evening: the term layer arrives — seven value sets registered at §4.10, the closed fact vocabulary published at §4.11, and seven terms added at §2.1.** 25/08: the review corrections — **§4.11 gains an *available from step* column**, three more value sets are registered, and two fact rows are corrected)
**Annex to:** *Solution Framework v2.5*. Supersedes the short glossary at Framework Appendix A, which now points here (Framework Appendix A updated 17/08/2026)
**Companions at this issue:** *D1 BRD v0.5* · *D2 Functional Specification v0.6* · *D3 Integration Contracts v2.3* · *A2 v1.2* · *A3 v1.5* · *A4 v1.3* · *A5 Product Data Sheet v1.5* (`A5_seed_v1.5.json`)
**Supersedes:** *A1 Glossary v1.4* (24 August 2026)
**Classification:** Internal

> **Change note — v1.5. The fact vocabulary gains the column that would have prevented a defect, and three sets that were named on a screen and registered nowhere.**
>
> **Nothing is retired and no value changes.** Five things are added or corrected, and the first is the one that earns the reissue.
>
> **1. §4.11 gains an *available from step* column, and it is not a convenience.** The vocabulary said *which* facts a term may test and never said *when* each one exists. The chain produces facts as it runs, so a fact can be in the vocabulary and still be null at the step a term is evaluated at — and one of the pilot product's ten terms was written that way: `TERM-HC-006`, a **`PRODUCT`**-scope term evaluated at **chain step 4**, tested **`benefitCode`**, which **step 5** produces. It could never fire, and it was the row A5 §7.3 relied on as proof that step 4 works. **D1 v0.5 closes it as `BR-TERM-9`** — *a term may test only facts available at the step its scope is evaluated at*, enforced when the term is **saved** rather than when it is evaluated — and **this column is the data that rule is checked against**. A1 v1.5 is therefore a dependency of the term register screen, not only a reference for it.
>
> **2. `serviceCategory`'s source was recorded backwards and is corrected.** It read *"Mapping — A5 §8"*. The mapping **consumes** `serviceCategory` and **produces** `benefitCode`; A5 §8.1 opens by saying the extraction layer classifies each cost line into a service category, and D3 §I-15 carries it on the wire. The source is **document extraction — IDP**, which is also why A4 §4.5 owns the set (§4.1) and why a document that recorded it as produced by the mapping had the only two facts in the table that come from step 5 pointing at each other.
>
> **3. `facilityType` moves from *Held* to *Missing*.** It was graded **Held** and it exists nowhere: no A4 field table extracts it, no D2 screen captures it, no D3 payload carries it and no D1 rule reads it. A fact graded *Held* is a fact a term author may write a class-`D` term against, so a wrong grade here does not mislead a reader — **it authorises an unevaluable term**. It needs an IDP question alongside **`Q-IDP-05`**, and §4.11 says so.
>
> **4. Three more value sets are registered — `termOutcome`, `stepOutcome` and `termOperator` — and D2 open item `D2-174` does not close at v1.4.** §4.10 as issued registered seven sets and said D2-174 was *"closed here"*. **D2-174 named eight**, and the eighth was **`termOutcome`**, which D2 §5.9.1a uses as `enum(termOutcome)`. **`stepOutcome`** is worse than unregistered: D2 §5.9.1 defines its four values **inside a screen table**, which D2's own §2.3 forbids. Both are held at §4.10 from this version, and **`D2-174` closes at A1 v1.5 rather than at v1.4.** **`termOperator`** is not in D2-174 and had no home either: it is the operator set of the `selector` / `predicate` grammar — the thing every term in the register is written in — and until now the operators existed only as examples in three documents.
>
> **5. `countUnit` and `counterType` are one dimension in two vocabularies, and the mapping between them is registered rather than left in prose.** The seed's `count_unit` is `DAY` · `VISIT`; the ledger's `counterType` is `AMOUNT` · `OCCURRENCE` · `DAY`. **`VISIT` and `OCCURRENCE` are the same thing under two names.** The crosswalk was carried only in A5 §4.2's prose, and a mapping that lives in one document's prose is a mapping the other document does not have. §4.10 holds it.

> **Change note — v1.4. One capability's worth of vocabulary, and one distinction that will otherwise be got wrong.**
>
> The ten decisions of the *Solution Paper — Policy Terms, Rule Engine and Limit Management v1.1* were approved on 24 August 2026, and between them they put **the policy's terms into the system as data** (Framework v2.4 Group G, F65–F68). A new capability brings a new vocabulary, and a vocabulary with no home is the exact defect §0 was written about.
>
> **Seven value sets are registered** — `termClass`, `termScope`, `termEffect`, `termStatus`, `checkpointOutcome`, `counterType` and `periodKey`, all at **§4.10**. D2 open item **D2-174** asked for them and is closed by that section. *(**Corrected at v1.5, and the correction is left visible rather than rewritten**: `D2-174` named **eight** sets, not seven — the eighth was **`termOutcome`** — so it did **not** close at v1.4. It closes at v1.5, together with `stepOutcome`, which D2 defined inside a screen table.)*
>
> **The fact vocabulary is published at §4.11, and it is CLOSED.** It is the complete list of facts a term may test, taken from the *Solution Paper* §2.5 item for item. **An author cannot invent a fact.** If the fact does not exist the term **cannot be class `D`**, and it falls to a checkpoint instead — which is the discipline that stops the platform auto-declining a claim on something it does not actually know. Adding a fact is a **specification change**, not a configuration change, and it is frozen per release (§10).
>
> **Seven terms are added at §2.1:** *policy term*, *term register*, *selector*, *predicate*, *term checkpoint*, *rule set* (`ruleSetRef`), and *coordination of benefits* — the last of these registered in order to be recorded as **out of scope, S-41**, rather than left as a silence. **Two terms are added to §3**: *counter period* (`periodKey`) and *counter type* (`counterType`).
>
> **`selector` and `predicate` are two fields and not one, and confusing them is the likeliest modelling error in the whole term layer.** D2 §5.28 specifies the split and this annex states it plainly because a glossary is where a reader looks first:
>
> | | Answers | Required | Permitted |
> |---|---|---|---|
> | **`selector`** | **Is this term in play** for this claim, this event or this line at all? | **Mandatory and non-null on all three classes — `D`, `J` and `E` alike** | Always |
> | **`predicate`** | **Does it fire** — the auto-decline? | Mandatory on class `D` | **Class `D` only. Null on `J` and on `E`** |
>
> A class-`J` term with a null selector is in play on **every** claim and raises a checkpoint on every claim, and a queue that receives every claim is a queue nobody works. A class-`J` term carrying a predicate **can auto-decline**, which is the one thing the class model exists to forbid. One field cannot do both jobs, and the *Solution Paper* §2.3's single-field model is corrected by D2 §5.28.2 rather than reproduced here.
>
> **Two counts move with this reissue and are recorded rather than left to be noticed.** The adjudication chain is **fourteen steps**, not thirteen (A5 §6.1) — so *treatment days* feeds **step 9**, the count limit, and not step 8. And **`serviceCategory` carries twenty values, not sixteen**: A5 v1.4 adds `SCREENING`, `VACCINATION`, `OPTICAL` and `HEARING`, which are Claim Platform master data with no IDP dependency. **Auto-approval now has three conditions, not two** (decision D-5) — §5.

> **Change note — v1.2, and this one is structural.** §2.3 of D2 requires that every `enum(X)` on a screen names a set that **lives in A1 §4**. Until now A1 §4 held one set — the six condition classes — and the rest of the project's enumerations lived wherever they were first written: some in A5, some in an annex that owns the concept, and **thirteen nowhere at all** (D2 §6.5.2 lists them; D2 open item **D2-108** calls it what it is). A rule that names a home which does not exist is a rule nobody can follow. **§4.1 is now that home** — a register of every value set, with its owner, its values and the fields that read it — and §4 is renamed accordingly. Nothing moved out of A5 or out of the annexes that legitimately own a set; **what §4.1 does is say where each one lives**, and hold outright the ones that lived nowhere.
>
> **Four sets are registered because the difference register put them on a screen** (D2 v0.3, open item **D2-124**): **`lossType`** — twelve values, **benefit group × treatment setting** (corrected 24/08; an earlier draft had it as condition class × setting, which collided with the condition class itself — see §4.2); **`claimantCapacity`** and **`claimantRelationship`**, which are two dimensions and not one; **`intakeSource`**, the channel a claim arrived through; and **`dateFilterType`**, which date a list filter applies to. **`lossType` subsumes `episodeType`; it does not replace it**, and §4.2 says exactly why and under what condition that should change.
>
> **Four terms are added:** *claimant* is promoted from a one-line entry to a modelled party distinct from both the insured and the payee; *occurrence date*, distinct from the treatment date; *treatment days*; and *renewal status*. **Two are restated:** *episode type*, which now points at `lossType`; and *insured event*, which now carries the occurrence date.
>
> **§4's six condition-class codes, §5.1's thirteen states, §5.2's seven decline reasons and §6's document types are unchanged**, and that is recorded rather than left for a reader to verify. `lossType` is built **from** the condition classes; it does not alter them.

---

## 0. Why this annex exists, and how to use it

A glossary is usually the least-read document in a set. This one has a specific job, and it comes from evidence.

On 17 August 2026, a cross-check of D3 against the Framework, A4 and A5 found three defects, all of the same kind:

- **`level` meant two different things** — the depth of a node in the benefit tree, and a plan attached to a policy — and both appeared in one payload.
- **The claim identifier had two names**, `claimNo` and `claimRef`, in documents written a day apart.
- **The condition-class codes in D3 did not match A5's** — `SPECIAL` against `SPE`, `MATERNITY` against `BIR` — and one of them, `MAT`, meant *childbirth* in one document and *maternity complication* in the other.

None was a thinking error. Each was a naming error, and each would have become a defect in code. The BRD will multiply the vocabulary several times over, so this annex is written **before** the BRD rather than after it.

**How to use it.** Three rules, and they are the whole point:

1. **If a term is here, use exactly this spelling** — in prose, in payloads, in column names, in screen labels, in code. Do not introduce a synonym because it reads better in one sentence.
2. **If a term is not here and you need it, add it here first.** A term that appears in a specification without appearing here is a defect in the specification, not a gap in the glossary.
3. **The Vietnamese column is normative for anything customer-facing.** A2 draws its wording from it. Where the Vietnamese is an established insurance term, it wins over a literal translation of the English.

### 0.1 Conventions in this document

| Column | Meaning |
|---|---|
| **EN** | The canonical English term. Used in specifications, code, and English-language screens |
| **VI** | The canonical Vietnamese term. Used in customer-facing text, Vietnamese screens, and A2 |
| **Identifier** | The exact string used in payloads, database columns and enums. Blank where the term is conceptual and never appears as data |
| **Definition** | What it means here, in this project. Not a dictionary definition |

Identifiers follow D3 §1.1: `lowerCamelCase` for JSON properties, `UPPER_SNAKE_CASE` for enum values, `snake_case` for database columns.

---

## 1. The three systems and the project

**Three, from 21 August 2026.** A fourth — the Policy Data Store — was specified through five versions and is **not built**: InsureMO demonstrated that it can carry the adjudicable benefit schedule, so the reason for the store went (Framework v2.0 §2.1). The term is retired and forwarded in §8.

| EN | VI | Identifier | Definition |
|---|---|---|---|
| **Claim Platform** | Hệ thống bồi thường | — | The application built in this project: the customer mobile app, the insurer web console, and the claim services behind them. Owns the claim lifecycle, adjudication, the limit ledger, the payee and payout-account master (F58), the group bulk load (F59) and the audit trail. **From v2.0, the only system in the solution we build** |
| **BnK IDP** | Hệ thống xử lý tài liệu thông minh | — | The intelligent document processing platform: classification, extraction, validation and reconciliation, and the clinical appropriateness assessment. Proposes; never decides |
| **InsureMO** | — | — | The external insurance platform. Provides product definition, **the benefit schedule**, quotation, issuance, **the policy, member and coverage register**, and the **as-at-a-date query** over all of it (F01–F07). A vendor name; not translated. Used for the **policy side only** — its claim capability exists and is deliberately unused (C-BUS-09) |
| **Policy, member and coverage register** | Danh mục hợp đồng và người được bảo hiểm | — | The record of policies, certificates, insured members and their coverage dates. **Held by InsureMO** (F05, R-IMO-10); read by the Claim Platform on every claim registration through I-05 and snapshotted onto the claim. It is not a component of ours and has no admin screen of ours |
| **MVP 1** | Giai đoạn 1 | — | The scope defined in Framework §7. Two journeys, **53 features** (47 at Priority 1), no cashless, no reserving |
| **Journey J1** | Hành trình mua bảo hiểm | — | Individual health policy purchase, browse to certificate |
| **Journey J2** | Hành trình bồi thường | — | Reimbursement claim, submission to payment |

---

## 2. Policy and product

| EN | VI | Identifier | Definition |
|---|---|---|---|
| **Product** | Sản phẩm bảo hiểm | `productCode` | The insurance product as a saleable thing. One in MVP 1 |
| **Plan / plan tier** | Chương trình bảo hiểm | `planCode` | A tier within the product, each with its own benefit schedule and premium. Three in MVP 1: `HC-STD`, `HC-PLS`, `HC-PRM` |
| **Sold product** | Sản phẩm đã bán | — | The product as configured for sale: plan, premium, sums insured. Contrast **adjudicable product** |
| **Adjudicable product** | Cấu hình tính toán bồi thường | — | The configuration needed to *pay* a claim: benefit options, cost-share settings, rule ordering. Mastered by the Claim Platform, never by InsureMO |
| **Policy** | Hợp đồng bảo hiểm / Đơn bảo hiểm | `policyNo` | The contract. Individual (sold via J1) or group (loaded via **F59** / I-14). Held by InsureMO |
| **Policy source** | Nguồn hợp đồng | `source` | How the policy came to exist: issued through J1, or loaded through the group bulk load. **Not cosmetic** — where a group member list is maintained claim-side (the open branch of C-IMO-10), `source` is what decides which member list a claim should believe |
| **Certificate** | Giấy chứng nhận bảo hiểm | — | The document issued to the customer evidencing cover |
| **Policyholder** | Bên mua bảo hiểm | — | The party that owns the policy and pays the premium. Not necessarily an insured member |
| **Insured member** | Người được bảo hiểm | `memberId` | The person whose cover is used. A policy has one or many |
| **Dependant** | Người phụ thuộc | — | An insured member related to the policyholder — spouse, child, parent |
| **Card number** | Số thẻ bảo hiểm | `cardNo` | Per-member identifier, issued per member and per dependant. A primary search key at claim intake, alongside policy number and ID number |
| **Benefit level** | Chương trình quyền lợi (trên một hợp đồng) | `benefitLevelNo` | A complete benefit schedule attached to a policy. A policy may carry several; each member points at one. **Never called "level" unqualified** — see §8 |
| **Cover period** | Thời hạn bảo hiểm | `policyPeriod` | The policy's own from/to dates |
| **Member coverage period** | Thời hạn bảo hiểm của người được BH | `coveragePeriod` | The member's own from/to dates, which may be narrower than the policy's. Both are tested |
| **Premium** | Phí bảo hiểm | `premium` | Integer VND |
| **Premium paid flag** | Trạng thái đóng phí | `premiumPaid` | Boolean. **Reported by InsureMO** on I-05; **tested by the Claim Platform** (A5 §6.1 step 1), not part of `covered`. The split is the point: a vendor supplies the fact, we draw the conclusion (P-01) |
| **Endorsement** | Sửa đổi bổ sung / Phụ lục hợp đồng | — | A versioned change to a policy. **Out of scope in MVP 1** — declared simplification S-05 |
| **As-at-a-date query** | Truy vấn tại thời điểm | — | A read of member coverage detail **and the benefit schedule as a tree** as they stood on a given date. **F07**, answered by InsureMO on I-05, and the thing that makes the policy snapshot (P-02) possible. It is only as good as the effective dating beneath it — C-IMO-05 |
| **Group policy and member bulk load** | Nhập khẩu hợp đồng nhóm và danh sách thành viên | — | The Excel upload that creates a group policy, its benefit levels and its member list, so claims can be demonstrated on group cover without building group underwriting. **F59, and it is ours** — the loader is a Claim Platform function whatever it writes into. **Never called "the group import feature of the policy store"**: there is no policy store. **Where it writes is open — C-IMO-10** |
| **Import validation report** | Báo cáo lỗi nhập khẩu | — | The per-row error listing produced by the validate phase of F59, before any commit. Import is two-phase and **nothing is written before commit** (D1 BR-IMP-1); the report is what a human reviews in between |
| **Claim-side member extension** | Danh sách thành viên phía hệ thống bồi thường | — | A member list maintained in the Claim Platform for group policies we loaded, **used only if InsureMO exposes no bulk member load** (the open branch of C-IMO-10). If it exists, group business has **two** member lists and a permanent reconciliation obligation, resolved by `source`. Defined here so the branch has a name before anyone has to build it |

### 2.1 Terms as data — the term register vocabulary

**New at v1.4.** Until now *"điều kiện điều khoản"* named a paragraph in a document. From v1.4 it names an object the engine can evaluate, and the words below are that object's parts. **A5 §7 is the normative term list for the pilot product; D2 §5.28 is the normative screen; D1 §5.3.1 owns the rules. This is where the words are fixed.**

| EN | VI | Identifier | Definition |
|---|---|---|---|
| **Policy term** | Điều khoản hợp đồng | `policy_term` | **One contractual term of the policy, held as an evaluable object** rather than as prose: a clause reference, the clause text as written, a class, a scope, an effect, a `selector`, a `predicate` where the class permits one, effective dating and a lifecycle state. **It is a contractual statement, not reference data** — which is why authoring it is governed (A3 §4.24–§4.28) and why a wrong one declines a claim on a clause the customer can read for themselves |
| **Term register** | Sổ điều khoản hợp đồng | — | **Claim Platform master data holding the policy terms, keyed on `ruleSetRef`.** **F65.** InsureMO owns *which* terms apply and returns the key; **we own what they mean operationally** (*Solution Paper* §2.6 option B, decision **D-1**). A predicate arriving from a vendor would be a decision-in-waiting, which breaches **P-01**, and that is the reason the register is ours rather than a preference about where data lives |
| **Term class** | Loại điều khoản | `termClass` | `D` · `J` · `E` — §4.10. **The distinction the whole term layer rests on: a class `J` or class `E` term may never auto-decline.** Not every term can be evaluated, and pretending otherwise is how an insurer auto-declines on a clause nobody read |
| **Selector** | Điều kiện chọn | `selector` | **The structured expression that decides whether a term is *in play*** for this claim, this event or this line. **Mandatory and non-null on every class**, `D`, `J` and `E` alike (D2 V-734). Written over the closed fact vocabulary of §4.11 and validated against it on save. **Never the same question as the predicate** — see §8 |
| **Predicate** | Điều kiện từ chối | `predicate` | **The structured expression that decides whether a term *fires*** — the auto-decline. **Permitted on class `D` alone; null on `J` and on `E`** (D2 V-734). Same grammar, same vocabulary, same validation as the selector. On most class-`D` terms the two expressions are identical, and the console copies one into the other rather than inviting them to be re-keyed and drift apart |
| **Term checkpoint** | Điểm kiểm điều khoản | — | **A question about a claim, raised by a term, with the clause quoted and the facts the selector matched on beside it.** **F67**, queue **S-14** (D2 §5.29). It is **not a decline** — a checkpoint nobody answers is not an outcome and never becomes one — and it is **not a claim state**: the claim sits in `UNDER_ASSESSMENT` exactly as it would otherwise. **An open checkpoint blocks auto-approval** (the third condition, §5) and blocks submission of the settlement plan. Answered by `GIỮ_NGUYÊN` (uphold — the line or event is declined citing the clause), `BỎ_QUA` (clear), or `KHÔNG_CÒN_HIỆU_LỰC` (moot), the last of which the platform writes and no person chooses |
| **Rule set** | Bộ quy tắc bảo hiểm | `ruleSetRef` | **The identifier of the wording and rule set governing a policy**, returned by InsureMO on I-05 and pinned onto the claim's snapshot. **Opaque, carried unchanged, never parsed for meaning** (D3 §1.2). **It is the key the term register binds to**, so a claim is evaluated against the terms that governed the policy it was written under. **Whether it comes back as codes we can bind terms to, or as prose only, is open — `C-IMO-13`**; and **whether it carries a version is open — D3 `X-16`**. Not to be confused with the **term register**, which is ours: the rule set says *which* terms, the register says *what they mean* |
| **Coordination of benefits** | Phối hợp quyền lợi giữa các hợp đồng | — | **Deciding which of two policies covering the same member pays first, whether the second may pay the remainder, and whether the same cost may be presented to both.** **Out of scope in MVP 1 — declared simplification S-41, with risk RK-15.** Registered here rather than omitted, and the consequence stated rather than glossed: **a member holding two policies may submit the same cost to both and the platform will not detect it.** Detection needs a cross-policy duplicate check on member, facility, treatment date and amount, and it is not built. **Distinct from the BHYT deduction** (§9, S-17), which is a different mechanism with a similar-sounding name |

---

---

## 3. Benefits, limits and the tree

This is the section where naming discipline matters most, because every term here appears in both a payload and a calculation.

| EN | VI | Identifier | Definition |
|---|---|---|---|
| **Benefit** | Quyền lợi bảo hiểm | `code` in a benefit-tree node; `benefitCode` when referenced from elsewhere (a cost line, a ledger counter, a console message) | A payable category of cost. Hierarchical — see **benefit tree**. The two names are the same value in different positions: inside the tree the node property is `code` because `parentCode` points at it; outside the tree it is always `benefitCode` |
| **Benefit tree** | Cây quyền lợi | — | The hierarchical benefit schedule. A benefit may have a parent, and a payment consumes every ancestor's counter |
| **Benefit group** | Nhóm quyền lợi | — | A root benefit and its descendants — `100` inpatient, `200` outpatient, `300` dental, `400` maternity, `500` accident |
| **Parent benefit** | Quyền lợi cha | `parentCode` | The benefit one level up. `null` at a root |
| **Depth** | Cấp | `depth` | The node's position in the tree: 1 at a root, 2 for a child, 3 for a grandchild. **Called `depth`, not `level`** — see §8 |
| **Sub-limit** | Hạn mức phụ | — | A limit expressed as a **child benefit row**, never as a column on the parent. This is the C-1 correction and it is structural |
| **Annual sum insured** | Số tiền bảo hiểm năm | `annualSumInsured` | The per-policy-year cap on a benefit. `included` in A5 means capped by the parent, with no cap of its own |
| **Per-event limit** | Hạn mức mỗi sự kiện | `perEventLimit` | Cap on a single insured event or procedure |
| **Per-day limit** | Hạn mức mỗi ngày | `perDayLimit` | Cap per day of hospitalisation |
| **Count limit** | Hạn mức số lần / số ngày | `maxCount` + `countUnit` | The second limit dimension: days, visits or occurrences. **`0` means unlimited, not zero.** A limit is expressed in money *and*, independently, in count |
| **Unlimited sentinel** | Giá trị không giới hạn (quy ước) | `0` | The convention above, stated as its own term because it must be honoured explicitly and never inferred |
| **Roll-up** | Cộng dồn lên quyền lợi cha | — | A payment consumes the counter of the benefit it is paid under *and every ancestor*. The claim is capped by the tightest binding limit on that path |
| **Co-payment** | Đồng chi trả | `coPaymentBp` | The share the member bears. **From the policy, read-only.** Integer basis points on the wire (`2000` = 20%); whole percent in the A5 seed (`copay_pct`) |
| **Deduction ratio** | Tỷ lệ giảm trừ | `deductionRatioBp` | A reduction **from the policy**, applied after co-payment, **only when its stated condition is met** — for example treatment outside the listed provider network. **Not discretionary**; no assessor sets it (D1 B-09). Framework §7.6, A5 §4.3 |
| **Deduction condition** | Điều kiện áp dụng giảm trừ | `deductionCondition` | The fact about the insured event that triggers the deduction ratio. Evaluated **per insured event**, not per claim, and recorded on the claim so the settlement letter can state it |
| **Limit ledger** | Sổ theo dõi hạn mức | — | The accumulator recording consumption against each benefit limit, per insured per policy per period |
| **Limit counter** | Bộ đếm hạn mức | — | One counter per insured × policy × benefit node × aggregation level |
| **Consumption record** | Bản ghi sử dụng hạn mức | — | One entry per claim line per service date. **Never deleted, only reversed or offset** |
| **Provisional posting** | Ghi nhận tạm | — | Consumption written at calculation, invisible to other in-flight claims |
| **Final posting** | Ghi nhận chính thức | — | Consumption confirmed **at approval** — not at payment. A failed payment posts a reversal; it does not un-finalise |
| **Remaining limit** | Hạn mức còn lại | — | Sum insured minus final consumption. Shown to the customer in "my cover" |
| **Counter period** | Kỳ hạn mức | `periodKey` | **The period a limit counter accumulates over — §4.10.** The **policy year**, derived from the **policy inception date on the snapshot**, and **never the calendar year** (D1 `L-1`). A cost line posts to the period containing its **treatment date, never its submission date** (`L-2`); a claim whose lines straddle a boundary posts to **two** periods, line by line (`L-3`); and `periodKey` is written on every posting and is **immutable once posted** (`L-4`). **New at v1.4** |
| **Counter type** | Loại bộ đếm | `counterType` | `AMOUNT` · `OCCURRENCE` · `DAY` — §4.10. **The dimension a counter accumulates in.** **New at v1.4**, and it closes a real inconsistency rather than adding a feature: `count_limit` and `per_day_cap` sit on all 51 benefit rows, chain step 9 tests them and D2's `S-11` displays *"remaining days"* — while `BR-LEDGER-1…6` governed money alone. A screen showing a figure no rule maintains is worse than no screen |

---

## 4. Condition classes, waiting periods, and the value-set register

**This section is the home §2.3 of D2 names.** It holds three things: the six condition-class codes, which are canonical here and nowhere else; **§4.1, the register of every value set in the project** — what it is called, who owns it, what its values are, and which field reads it; and, from v1.4, **§4.11, the closed fact vocabulary** a policy term is permitted to test.

**The six codes below are canonical.** A5 §5 is the normative table of day-counts; this is the normative list of codes.

| EN | VI | Code | Definition |
|---|---|---|---|
| **Condition class** | Nhóm tình trạng (xác định thời gian chờ) | `conditionClass` | The classification that determines which waiting period applies. Determined at assessment and stored on the insured event |
| Pre-existing condition | Bệnh có sẵn | `PRE` | A condition existing before cover started |
| Specified condition | Bệnh đặc biệt | `SPE` | A named condition on the product's specified list (A5 §5) |
| General illness | Bệnh thông thường | `GEN` | Any illness that is neither pre-existing nor specified |
| Childbirth | Sinh đẻ | `BIR` | Normal delivery or caesarean section |
| Maternity complication | Biến chứng thai sản | `MAT` | A complication of pregnancy or delivery |
| Accident | Tai nạn | `ACC` | An accidental injury. Zero waiting period in this product |
| **Waiting period** | Thời gian chờ | `waitingPeriodDays` | Days from the **member's coverage start date** — not the policy inception date — before a benefit becomes payable for a given condition class |
| **Not applicable** | Không áp dụng | `null` | The class-and-benefit combination cannot arise. **A claim that produces one is a data error, not a zero-day waiting period** |

> **`MAT` means maternity complication, not maternity.** Childbirth is `BIR`. The two carry different waiting periods — 270 days and 90 days — so swapping them is a silent, expensive mistake. This is the exact defect found on 17 August 2026.

---

### 4.1 The value-set register

**Every enumerated set in this project has exactly one owner and one definition, and this table says which.** A set is either *held here* — the values are written out below and nowhere else — or *homed elsewhere*, meaning another document legitimately owns the concept and this register points at it. **There is no third state.** A set that appears in a screen specification, a payload or a column name without a row here is a defect in that document, not a gap in this one (rule 2 of §0).

| Set | Held / homed | Where | What it answers |
|---|---|---|---|
| `conditionClass` | **Held** | §4 above | Which waiting period applies |
| `claimState` | **Held** | §5.1 | Where a claim is in its lifecycle |
| `declineReason` | **Held** | §5.2 | Why a claim, event or line is not payable |
| `documentType` | **Held** | §6 | Which of the seven types a document is |
| `role` | **Held** | §7 | Which of the four roles a user holds |
| **`lossType`** | **Held** | **§4.2** | What kind of loss this event is — condition × setting. **New at v1.2** |
| **`claimantCapacity`** | **Held** | **§4.3** | On what legal footing this person is claiming. **New at v1.2** |
| **`claimantRelationship`** | **Held** | **§4.3** | Who this person is to the insured. **New at v1.2** |
| **`intakeSource`** | **Held** | **§4.4** | Which channel the claim arrived through. **New at v1.2** |
| **`dateFilterType`** | **Held** | **§4.5** | Which date a list filter applies to. **New at v1.2** |
| **`renewalStatus`** | **Held** | **§4.6** | Whether this is a first-time or a continuing participation. **Live from v1.3 — conditional on a vendor answer** |
| **`cancellationReason`** | **Held** | **§4.7** | Why a claim was cancelled |
| **`staffNotificationType`** | **Held** | **§4.8** | Which of the eight staff events occurred |
| **`termClass`** | **Held** | **§4.10** | Whether a term is deterministic, judgemental or evidential — and therefore whether the engine may decline on it. **New at v1.4** |
| **`termScope`** | **Held** | **§4.10** | What a term applies to: the product, a benefit, or a benefit and its descendants. **New at v1.4** |
| **`termEffect`** | **Held** | **§4.10** | What happens when a term fires. **New at v1.4** |
| **`termStatus`** | **Held** | **§4.10** | Where a term version stands in its lifecycle. **New at v1.4** |
| **`checkpointOutcome`** | **Held** | **§4.10** | How a term checkpoint was answered. **New at v1.4** |
| **`termOutcome`** | **Held** | **§4.10** | How one term resolved on one claim — the value the assessor's *terms tested* line is counted from (D2 §5.9.1a). **New at v1.5; it is the eighth set `D2-174` asked for and v1.4 did not register** |
| **`stepOutcome`** | **Held** | **§4.10** | How one step of the adjudication chain resolved (D2 §5.9.1). **New at v1.5**, and brought in because D2 defined it **inside a screen table**, which D2's own §2.3 forbids |
| **`termOperator`** | **Held** | **§4.10** | The operators a term's `selector` or `predicate` may use, and the three grouping keywords. **New at v1.5** — the grammar every term in the register is written in, registered nowhere until now |
| **`counterType`** | **Held** | **§4.10** | Which dimension a limit counter accumulates in. **New at v1.4** |
| **`periodKey`** | **Held** | **§4.10** | Which counter period a posting belongs to. A derived key rather than a typed list, and registered all the same. **New at v1.4** |
| **The fact vocabulary** | **Held** | **§4.11** | Which facts a term's `selector` or `predicate` may test. **CLOSED — an author cannot invent a fact.** New at v1.4 |
| `episodeType` | **Held** | §5 | Which treatment type the customer declared. **See §4.2 — subsumed by `lossType`, not retired** |
| `serviceCategory` | Homed | **A4 §4.5** | Which of the **twenty** categories a cost line falls in. **Sixteen until A5 v1.4**, which adds `SCREENING`, `VACCINATION`, `OPTICAL` and `HEARING` — **Claim Platform master data, ours to add, no IDP dependency and one seed change**, and the reason exclusions #2 and #4 move from unevaluable to class `D`. A4 owns the set because IDP derives it |
| `countUnit` | Homed | A5 | Days or visits — the unit a **count limit on a benefit schedule row** is expressed in. **Not the same vocabulary as `counterType`**, which is the ledger's; **§4.10 holds the crosswalk between them** and it is normative there rather than in A5 §4.2's prose |
| `packageState` | Homed | A4 §6 | How complete a document package is. Produced by IDP, consumed here |
| `requirementLevel` | Homed | A4 | `REQUIRED` · `EXPECTED` · `OPTIONAL` |
| `channel` | Homed | **A2 §0.3** | Which channel a message goes out on |
| `grantLevel`, `dataScope` | Homed | **A3 §0.1, §0.3** | The permission vocabulary |
| **`actorType`** | **Held** | **§4.9** | Whether an actor is a person, a system or a vendor. **Moved here at v1.2** — it was defined in D2 §1.2, which breaches D2's own §2.3 |
| **`deliveryOutcome`** | **Held** | **§4.9** | What happened to a sent message |
| **`operationalIssueType`**, **`queueItemState`**, **`interfaceCode`** | **Held** | **§4.9** | The operator queue's vocabulary |
| **`language`**, **`templateStatus`**, **`attachmentType`**, **`userStatus`**, **`roleStatus`**, **`surface`**, **`auditAction`**, **`auditOutcome`**, **`province`** | **Held** | **§4.9** | Sets Part VI of D2 uses and nothing owned. **Brought in at v1.2** |

> **Why thirteen sets had no home, and why that is worth a sentence rather than a silent fix.** Each was invented at the moment a screen or a payload needed it, by someone who was writing a different document. None was wrong; all of them were **unfindable**, which is the same defect §0 was written about — a set nobody can find is a set someone re-invents with different values. **D2 open item D2-108 raised it and D2-124 asked for four more. Both are closed by §4.1.**

### 4.2 `lossType` — twelve values, and its relationship to `episodeType`

> **Corrected 24/08/2026, and the correction matters.** An earlier draft of this section registered `lossType` as *condition class × treatment setting* — `PRE_OP`, `SPE_IP` and so on. **That was wrong on both counts.** It is not what the reference system carries, and it collided with the condition class, which is a **separate field** in the reference and in our own design (§4.4). The set below is the one the reference actually uses and the one D2 implements (v0.3, unchanged at v0.5): **benefit group × treatment setting**, plus four death and permanent-disability variants. A1 was the outlier; D2 was right.

**The loss type is the clinical benefit group crossed with where the treatment happened.** It is one field with two dimensions folded into it — which is exactly how the reference expresses it, and it is what the document checklist keys on.

| # | Code | VI | EN | Benefit group | Setting | MVP 1 |
|---|---|---|---|---|---|---|
| 1 | `ILL_OP` | Bệnh (ngoại trú) | Illness, outpatient | `200` | Ngoại trú | ✔ |
| 2 | `ILL_IP` | Bệnh (nội trú) | Illness, inpatient | `100` | Nội trú | ✔ |
| 3 | `DEN_OP` | Nha khoa (ngoại trú) | Dental, outpatient | `300` | Ngoại trú | ✔ |
| 4 | `DEN_IP` | Nha khoa (nội trú) | Dental, inpatient | `300` | Nội trú | ✔ |
| 5 | `ACC_OP` | Tai nạn (ngoại trú) | Accident, outpatient | `500` | Ngoại trú | ✔ |
| 6 | `ACC_IP` | Tai nạn (nội trú) | Accident, inpatient | `500` | Nội trú | ✔ |
| 7 | `MAT_OP` | Thai sản (ngoại trú) | Maternity, outpatient | `400` | Ngoại trú | ✔ |
| 8 | `MAT_IP` | Thai sản (nội trú) | Maternity, inpatient | `400` | Nội trú | ✔ |
| 9 | `DTH_ILL` | Tử vong do bệnh, thai sản | Death from illness or maternity | — | — | ✘ |
| 10 | `DTH_ACC` | Tử vong do tai nạn | Death from accident | — | — | ✘ |
| 11 | `PTD_ACC` | Thương tật vĩnh viễn do tai nạn | Permanent disability from accident | — | — | ✘ |
| 12 | `PTD_ILL` | Tổn thương sức khoẻ vĩnh viễn do bệnh | Permanent impairment from illness | — | — | ✘ |

**Eight values are live in MVP 1; four are registered and out of scope.** Framework §7.2 excludes death, permanent disability and income-benefit claims, so rows 9 to 12 exist here **only so that a value arriving from anywhere has a name**. They are registered rather than omitted because a set that silently cannot express a real claim type is worse than one that names it and refuses it. No screen offers them, and a claim carrying one is a data error, not a claim.

**`lossType` is captured, not derived.** An assessor sets it on the insured event (D2 §5.7.2 F18); the customer's app sets it on the claim form from the same eight live values (D2 §4.5). Nothing computes it.

**It is orthogonal to the condition class.** The condition class (§4.4 — `PRE`, `SPE`, `GEN`, `BIR`, `MAT`, `ACC`) answers *what kind of medical condition*, and it drives the **waiting period**. The loss type answers *which benefit group and where treated*, and it drives the **document checklist** and the benefit mapping. A pre-existing condition treated as an outpatient is `conditionClass = PRE` and `lossType = ILL_OP`; the two facts are set separately and neither derives the other. The reference keeps them as two dropdowns for the same reason.

**One constraint runs between them**, and it is a constraint rather than a derivation: `ACC_*` admits only `conditionClass = ACC`; `MAT_*` admits only `BIR` or `MAT`. D2 §5.7.2 carries the table.

> **`lossType` subsumes `episodeType`. It does not replace it — yet.**
>
> **`episodeType`** (§5, five values) remains the field that **A5 §8.1 keys its benefit mapping on**, and A5 is unchanged. It is now a **projection** of `lossType` rather than something anyone types: `ILL_OP` → `OUTPATIENT`, `ILL_IP` → `INPATIENT`, `DEN_*` → `DENTAL`, `MAT_*` → `MATERNITY`, `ACC_*` → `ACCIDENT`. D2 §5.7.2 F5 renders it read-only and states the projection.
>
> **Why not retire it now.** Retiring it means re-keying A5 §8.1's mapping and A4 §6's checklist in the same pass, and A4 §6 is already being re-cut. Doing both at once, before the checklist has been confirmed with the business, would change two things and be able to attribute a defect to neither.
>
> **The condition for retiring it is specific:** when A4 §6's checklist is re-cut per loss type — which is what makes the selection non-circular (D2 §5.5.1) — `episodeType` has no remaining reader outside A5 §8.1, and A5 can then be re-keyed on its own. **Until then the two must never be independently settable.** `episodeType` is a projection; code that lets them disagree has a defect, not a feature.

### 4.3 `claimantCapacity` and `claimantRelationship` — two dimensions, not one

**These answer two different questions and collapsing them loses one of the answers.**

`claimantCapacity` — **on what legal footing is this person claiming?**

| Code | VI | EN |
|---|---|---|
| `BẢN_THÂN` | Bản thân | Self — the claimant is the insured member |
| `NGƯỜI_GIÁM_HỘ` | Người giám hộ | Legal guardian |
| `NGƯỜI_THỪA_KẾ` | Người thừa kế | Heir |
| `NGƯỜI_ĐƯỢC_UỶ_QUYỀN` | Người được uỷ quyền | Authorised representative |

`claimantRelationship` — **who is this person to the insured?** Shown and mandatory **only where capacity is not `BẢN_THÂN`**; absent otherwise.

| Code | VI | EN |
|---|---|---|
| `BỐ` | Bố | Father |
| `MẸ` | Mẹ | Mother |
| `VỢ` | Vợ | Wife |
| `CHỒNG` | Chồng | Husband |
| `CON` | Con | Child |
| `ÔNG` | Ông | Grandfather |
| `BÀ` | Bà | Grandmother |
| `KHÁC` | Khác | Other |

> **Why they are two sets and not one, stated plainly because the temptation to merge them is real.** *Capacity* is a **legal fact that decides what the claimant may do and what they must prove**. *Relationship* is a **family fact that decides nothing on its own**. A mother claiming for her minor child is capacity `NGƯỜI_GIÁM_HỘ`, relationship `MẸ`. A mother claiming for her adult child who has died is capacity `NGƯỜI_THỪA_KẾ`, relationship `MẸ` — **the same relationship, an entirely different legal standing and an entirely different set of documents**. Merge them and one of those two claims cannot be expressed.
>
> **The dependency runs one way and it runs to documents.** Capacity drives the checklist: a guardian must evidence guardianship, an heir must evidence inheritance and any authority to receive the payment. Relationship does not drive anything by itself — it is context for the assessor and for the conflict-of-interest declaration (A3 §4.12).
>
> **`claimantRelationship` is also a different set from `relationship` on the beneficiary record** (`SELF` · `SPOUSE` · `CHILD` · `PARENT`, D2 §5.20 F9), which is deliberately narrower because it describes who the *money* goes to. **Do not use one where the other is meant.** Both are listed in §8.

### 4.4 `intakeSource` — the channel the claim arrived through

| Code | VI | EN |
|---|---|---|
| `ỨNG_DỤNG` | Ứng dụng di động | Mobile app — the customer submitted it themselves |
| `QUẦY` | Tiếp nhận tại quầy | Counter — an officer created it from documents received in person, by post or by e-mail |

**Two values in MVP 1, and the set is open.** Reference System A carries four; ours carries the two channels that exist, and the register row that asked for it (**B-02**) was approved as *nên sửa*. **Set once at registration and read-only thereafter** (D2 v0.5 `V-715`): it is one of the two measurement anchors on the claim, and an officer who could edit it could make a counter claim look like an app claim, which would silently corrupt the one number the pilot is running to produce. **Adding a value is data, not code** — a web channel or a provider-guarantee channel drops in without a schema change.

### 4.5 `dateFilterType` — which date a list filter applies to

| Code | VI | EN | The claim-level date it selects |
|---|---|---|---|
| `NGÀY_GỬI` | Ngày gửi | Submission date | When the customer submitted — the internal clock's origin |
| `NGÀY_DUYỆT` | Ngày duyệt | Approval date | When the settlement plan was approved |
| `NGÀY_CHI_TRẢ` | Ngày chi trả | Payment date | When payment was confirmed |
| `NGÀY_ĐÓNG_HỒ_SƠ` | Ngày đóng hồ sơ | Closure date | When the claim reached `CLOSED` |

**Four values, because these are the four dates the console records at claim level and can therefore filter on.** Called **`dateType`** in the difference register (rows **S-48**, **B-26**); **`dateFilterType`** is the canonical identifier, because a bare `dateType` invites use on a document date or a treatment date, which this set has nothing to do with. Both names appear in §8.

**One behaviour belongs with the set rather than with the screen.** A claim that has not reached the selected milestone is **excluded from the result, never shown with a blank date**. Filtering on *ngày chi trả* returns claims that were paid — that is the question being asked, and a list padded with unpaid claims answers a different one.

### 4.6 `renewalStatus` — first-time or continuing participation

| Code | VI | EN |
|---|---|---|
| `LẦN_ĐẦU` | Tham gia lần đầu | First-time participant |
| `TÁI_TỤC` | Tái tục | Renewing — cover has been continuous |
| `KHÔNG_XÁC_ĐỊNH` | Không xác định | Not known |

**Live from 24 August 2026 — D2-129 answered yes.** InsureMO returns the attribute on `I-05` at **member-on-policy** level, which is the level the attribute actually belongs to: not to the policy, and not to the person. So A5 §5's carry-over rule now has an input, and a renewing member is no longer blocked for thirty days — or three hundred and sixty-five, on a pre-existing condition.

**One verification is still owed, and it is not a formality.** What was confirmed is that the attribute exists at member level. **A renewal flag held at *policy* level would not answer for a member who joined mid-term** — that member's participation is first-time even though the policy is a renewal, and reading the policy's flag would waive a waiting period that should apply. The level must therefore be re-checked against the real schema when the sandbox arrives (**C-IMO-06**). Until then the field is specified, buildable, and carries that caveat.

**`KHÔNG_XÁC_ĐỊNH` stays in the set**, and now for a different reason than when it was added. It is no longer the fallback for *we never asked* — it is the value for *this member's history is genuinely ambiguous*, which happens when a policy is re-issued rather than renewed. A waiting-period decision resting on a value we guessed is a decision we cannot defend, so the set keeps a way to say we do not know.

### 4.6a Contact on the claim — `claimContactEmail`, `claimContactPhone`

**Decided 24 August 2026 (B4).** The customer types an **email address and a telephone number when submitting a claim**, and those are the contact details for **that claim**.

| Term | VI | Identifier | Where it lives |
|---|---|---|---|
| Claim contact email | Email liên hệ trên hồ sơ | `claimContactEmail` | **On the claim.** Captured at submission (D2 §4.5), snapshotted with it |
| Claim contact telephone | Điện thoại liên hệ trên hồ sơ | `claimContactPhone` | Same |

**These are properties of a claim, not of a member — and that is the point.** A contact captured with the claim **cannot be redirected afterwards**. A change to the customer's account tomorrow does not alter where a decline letter about a claim decided yesterday was sent, and the record of where it went is on the claim itself. The alternative — reading an address from a member register at send time — makes correspondence about a settled claim depend on data that has since moved.

**It also removes a dependency.** Correspondence no longer needs InsureMO to hold an email address at all, which was open item D2-14 and is now closed by design rather than by answer.

> **The consequence a reader will otherwise miss: there are two addresses and two journeys.**
>
> The **certificate at purchase** goes to the **account's** address — the one registered at sign-up (D2 §4.2 S10 F3), because at that moment there is no claim to hang anything on.
>
> **Claim correspondence** goes to **what was typed on the claim**. For an individual policy these are usually the same address and nobody notices. **For a group scheme where a customer claims for a dependant they are routinely different**, and that is the case the design exists for.

**One risk, stated rather than papered over.** A customer can type any address, and a decline letter is a document about somebody's health. This is not the payment-redirection risk — correspondence is not money — but it is not nothing either. D2 §4.5 specifies whether the address is echoed back for confirmation before submission; A3 governs whether anyone on the console may change it afterwards.

### 4.7 `cancellationReason` — why a claim was cancelled

| Code | VI | EN |
|---|---|---|
| `KHÁCH_HÀNG_RÚT` | Khách hàng rút yêu cầu | Withdrawn by the customer |
| `MỞ_NHẦM_HỒ_SƠ` | Mở nhầm hồ sơ | Opened in error |
| `TRÙNG_HỒ_SƠ` | Trùng với hồ sơ khác | Duplicate of another claim |
| `KHÁC` | Khác | Other — a free-text reason is then mandatory |

**A controlled list rather than free text, and that is the whole value of it.** Cancellation is the only exit MVP 1 has before assessment (Framework **S-35**), so it carries two situations the reference separates into two actions — *this was never a valid claim* and *this needs work first*. A free-text reason cannot be counted; a four-value list can, and **how often intake quality was the problem is one of the few numbers a pilot exists to produce**.

### 4.8 `staffNotificationType` — the eight events that reach a member of staff

| Code | VI | EN |
|---|---|---|
| `ĐƯỢC_PHÂN_GIAO` | Hồ sơ được phân giao cho bạn | A claim was assigned to you |
| `ĐỔI_CÁN_BỘ` | Hồ sơ được chuyển cán bộ | A claim was reassigned |
| `TRẢ_HỒ_SƠ` | Người phê duyệt trả hồ sơ về | An approver returned your claim |
| `KHÁCH_ĐÃ_BỔ_SUNG` | Khách hàng đã gửi chứng từ bổ sung | The customer supplied what you asked for |
| `TRÍCH_XUẤT_LỖI` | Trích xuất tài liệu lỗi | Extraction failed or stalled |
| `TRÍCH_XUẤT_XONG` | Trích xuất tài liệu hoàn tất | Extraction completed |
| `QUÁ_HẠN` | Hồ sơ sắp hoặc đã quá hạn | The claim crossed into the overdue bucket |
| `CHỜ_PHÊ_DUYỆT` | Có phương án chờ bạn phê duyệt | A settlement plan is waiting for you |

**Eight, matching D2 §5.23.1 exactly.** Reference System A carries twenty-two staff templates; these are the eight where work changes hands or a claim stops moving. **The sentences themselves are A2's, not this annex's** — the codes are registered here because they are an `enum` on a field (D2 §5.23.2 F3), and the text they render is a message (A2 §9, open item **D2-126**).

### 4.9 Sets held here for want of a better owner

**These are real, they are used, and no annex owns the concept.** They are held here so that D2 §2.3 can be satisfied and so nobody re-invents one with different values. Where the values are self-evidently technical they are listed without commentary; where a value carries a decision, the decision is stated.

| Set | Values | Note |
|---|---|---|
| `actorType` | `USER` · `SYSTEM` · `VENDOR` | **Moved here from D2 §1.2 at v1.2**, which breached D2's own §2.3. `VENDOR` is distinct from `SYSTEM` on purpose: an audit entry that says a *vendor* acted names an accountable third party, and one that says *system* does not |
| `deliveryOutcome` | `SENT` · `DELIVERED` · `FAILED` · `UNDELIVERABLE` | `FAILED` is retryable; `UNDELIVERABLE` means the address itself is rejected and no retry will help. **Keeping them apart is what stops the retry schedule hammering a dead mailbox** |
| `operationalIssueType` | `INTERFACE_ERROR` · `EXTRACTION_ERROR` · `MESSAGE_FAILURE` · `VALUE_UNREADABLE` | The operator queue's classification (D2 §5.19) |
| `queueItemState` | `MỚI` · `ĐANG_XỬ_LÝ` · `ĐÃ_XỬ_LÝ` · `ĐÃ_BỎ_QUA` | **`ĐÃ_BỎ_QUA` requires a reason.** A dismissal without one is how a stuck claim becomes invisible |
| `interfaceCode` | `I-01` … `I-16` | The interface identifiers of D3. Held here so an operator-queue row can name the interface that failed |
| `templateStatus` | `DRAFT` · `ACTIVE` · `SUPERSEDED` | A2 template lifecycle. **A superseded template is never deleted** — a sent message must stay renderable as it was sent (D2 §5.22.1 F8) |
| `auditAction` | The action vocabulary of the audit trail | **Open** — the values follow from D2 open item **D2-128**, which restates the audit entry action-first. Registered as a named set now so that the restatement has somewhere to write its values |
| `auditOutcome` | `SUCCESS` · `DENIED` · `ERROR` | `DENIED` is logged, not merely refused (A3 §6) |
| `language` | `vi` · `en` | Two, and `vi` is primary for anything customer-facing (A2 rule M-1) |
| `attachmentType` | `PDF` · `CSV` · `JSON` | What a message or an export may carry |
| `userStatus` | `ACTIVE` · `SUSPENDED` · `CLOSED` | |
| `roleStatus` | `ACTIVE` · `WITHDRAWN` | |
| `surface` | `MOBILE` · `CONSOLE` · `SYSTEM` | Where an action was performed |
| `province` | The 34 provinces and centrally-governed cities of Vietnam | **Reference data, not a hand-maintained list.** Held as a named set so the provider master (D2 §5.17.1 F13) has one to point at; the values themselves are loaded, and they change by legislation |

### 4.10 The term layer's value sets — ten sets: seven from v1.4, three added at v1.5

**These exist because terms are now data.** Every one of them appears as an `enum` on D2 §5.9's, §5.28's or §5.29's field table, and until v1.4 not one of them had a home — which is D2 open item **D2-174**.

> **`D2-174` closes here, at v1.5, and not at v1.4.** v1.4 registered seven sets and said the item closed with them. **The item named eight.** The eighth was **`termOutcome`**, which D2 §5.9.1a uses as `enum(termOutcome)` on the assessor's terms panel, and it was missed. A second set, **`stepOutcome`**, was never in the item at all because D2 had defined its four values **inside a screen table** — which D2's own §2.3 forbids, and which is exactly the condition §4.1 exists to end. Both are held below. A third, **`termOperator`**, is added on the same reasoning and from the same place: the operator vocabulary of the `selector` / `predicate` grammar appeared as examples in D1 §5.3.1, D2 §5.28.2 and A5 §7.2, and as a registered set nowhere.

**`termClass` — what kind of term this is, and therefore what the engine may do with it.** This is the set the whole term layer rests on and it is not an ordinary enumeration: each value is a *permission and a prohibition*, not a label.

| Code | VI | EN | The engine **may** | The engine **may never** |
|---|---|---|---|---|
| `D` | Điều khoản xác định | **Deterministic** — decidable from facts the claim already holds | **Auto-decline**, with reason code `EXCL` and the term's `clauseRef` carried into the decline | — |
| `J` | Điều khoản cần phán đoán | **Judgemental** — requires a person, or a clinical opinion | Raise a **term checkpoint** with the clause quoted | **Auto-decline** |
| `E` | Điều khoản cần chứng cứ | **Evidential** — decidable only if a specific named document says so | **Require the document**; raise a checkpoint where it is absent | **Auto-decline** |

> **The prohibition is the point of the set, and it is stated here rather than only in D1.** A class-`J` term auto-declined is **a decline citing a clause on a judgement no human made** — invisible to us, unanswerable to the customer. **A term whose fact does not exist in §4.11 cannot be class `D`.** That is not a limitation to work around; it is the mechanism.

**`termScope` — what the term applies to.**

| Code | VI | EN | Evaluated at |
|---|---|---|---|
| `PRODUCT` | Toàn sản phẩm | The whole product | Chain **step 4**, general exclusions |
| `BENEFIT` | Một quyền lợi | One benefit code | Chain **step 7**, benefit-level provisos |
| `BENEFIT_SUBTREE` | Quyền lợi và các quyền lợi con | A benefit and every descendant under `BR-TREE-1` | Chain **step 7** |

**`termEffect` — what happens when the term fires.**

| Code | VI | EN | Note |
|---|---|---|---|
| `DECLINE_EVENT` | Từ chối cả sự kiện | Decline the insured event | **Outranks `DECLINE_LINE`** (`BR-TERM-2`). Class `D` only |
| `DECLINE_LINE` | Từ chối dòng chi phí | Decline the cost line | Class `D` only. Every hit is recorded and **every** `clauseRef` reaches the decline message, not only the first |
| `REQUIRE_DOC` | Yêu cầu chứng từ | Require a named document | The class-`E` effect. Names a document type from §6 |
| `CHECKPOINT` | Điểm kiểm | Raise a term checkpoint | The only effect a class-`J` term may carry alongside `REQUIRE_DOC` — **and available to class `D` as well.** `termClass` and `effect` are orthogonal: class `D` means the *selector* fires deterministically, which does not oblige the term to decline. Two of the ten pilot terms are class `D` with `CHECKPOINT` (D1 `BR-TERM-7`, D2 `V-742`) |

> **`CAP` is offered by D2 §5.28.3 F11 and by the *Solution Paper* §2.3, and it is deliberately not registered here.** **No term in A5 v1.4 or in `A5_seed_v1.4.json` uses it** — the four values above are the whole live set, and a numeric cap in this product is a benefit-schedule row rather than a term. **Registering a value nothing produces would put an option on a screen with no meaning behind it**; leaving the divergence unsaid would be worse. **Either D2 §5.28.3 drops `CAP` from F11, or A1 registers it as a fifth value** — and the choice belongs to whoever next passes over D2, because it is that document's screen. Recorded here so it is a decision and not a discovery.

**`termStatus` — the lifecycle of a term version.** `DRAFT → PENDING_ACTIVE → ACTIVE → WITHDRAWN`, **and there is no other edge** (D2 `V-735`).

| Code | VI | EN | Note |
|---|---|---|---|
| `DRAFT` | Bản nháp | Draft | **Invisible to the engine.** A half-written predicate can never decide a claim |
| `PENDING_ACTIVE` | Chờ hiệu lực | Approved, effective date not yet reached | **A real state and not a formality** — a term authored and approved but not yet in force is a thing an insurer needs, and it would otherwise have been discovered late |
| `ACTIVE` | Đang hiệu lực | In force | **Only `ACTIVE` terms inside their effective window are evaluated** |
| `WITHDRAWN` | Đã thu hồi | Withdrawn | Claims already bound to the version are **unaffected** (`BR-TERM-4`); the version is retained and stays readable forever. **A term is never deleted** |

**`checkpointOutcome` — how a term checkpoint was answered** (D2 §5.29.2 F8).

| Code | VI | EN | Note |
|---|---|---|---|
| `GIỮ_NGUYÊN` | Giữ nguyên điều khoản | **Uphold** — the line or the event is declined citing the clause | **An uphold is a decline** and carries every rule a decline carries: the reason code, the wording rule, the item and the amount named |
| `BỎ_QUA` | Bỏ qua điểm kiểm | **Clear** | Nothing on the claim changes except the record. **A reason is mandatory**, because it is a decision *not* to apply a contractual term |
| `KHÔNG_CÒN_HIỆU_LỰC` | Không còn hiệu lực | **Moot** — the line or event was declined on another ground | **Written by the platform and never chosen by a person** (D2 `V-741`). Nobody decided it; the claim decided it, and recording it as a human judgement would put a name against a decision nobody made |

**`termOutcome` — how one term resolved on one claim** (D2 §5.9.1a). **New at v1.5.** This is the set the assessor's *"n terms tested"* line is counted from, so its values decide what that line can honestly say.

| Code | VI | EN | Note |
|---|---|---|---|
| `ĐẠT` | Đạt | **Passed** — the term was in play and did not fire | The selector matched; the predicate did not, or there was none to match |
| `TỪ_CHỐI` | Từ chối | **Declined** — the term declined the line or the event | **Only ever a class-`D` term whose effect is `DECLINE_EVENT` or `DECLINE_LINE`** (`termClass`, above). A `J` or an `E` producing this value is a defect, not a decision |
| `ĐIỂM_KIỂM` | Điểm kiểm | **Checkpoint raised** | **Any class may produce it** — a class-`D` term carrying `CHECKPOINT` does (§4.10 `termEffect`), and two of the pilot's ten do |
| `KHÔNG_THUỘC_PHẠM_VI` | Không thuộc phạm vi | **Not in play** — the selector did not match | **Recorded, not omitted.** An auditor asking *"was clause `TBC-06` considered?"* is owed *considered and not applicable*, which is a different answer from silence |

> **The counting rule, stated here because two documents count differently unless it is.** A term in **`DRAFT`** is invisible to the engine (`termStatus`) and **produces no `termOutcome` at all** — it is not evaluated, so it is not counted. The *terms tested* figure an assessor sees is the number of **`ACTIVE`** terms inside their effective window bound to the claim's `ruleSetRef`, and on the pilot product that is **six of ten**, not ten. A5 §7.5 carries the same arithmetic.

**`stepOutcome` — how one step of the adjudication chain resolved** (D2 §5.9.1). **New at v1.5**, and it is held here rather than on the screen that uses it because §2.3 of D2 forbids a screen specification from defining a set.

| Code | VI | EN | Note |
|---|---|---|---|
| `ÁP_DỤNG` | Áp dụng | **Applied** — the step ran and had an effect | |
| `BỎ_QUA` | Bỏ qua | **Skipped** — the step is switched off for this product | The `enabled` flag on `adjudication_rule`, and the whole of what that collection configures (Framework **P-05** as revised, **S-39**) |
| `KHÔNG_ÁP_DỤNG` | Không áp dụng | **Not applicable** — the benefit carries nothing for this step to act on | No co-payment, no count limit, no deduction ratio. **Distinct from `BỎ_QUA`**: one is a configuration choice, the other is a property of the benefit, and collapsing them loses the reason |
| `KHÔNG_ĐẠT` | Không đạt | **Failed** — the step declined or reduced the amount | |

**`termOperator` — the operators a term's `selector` or `predicate` may use.** **New at v1.5.** Closed, like the fact vocabulary it is written against: the expression builder validates on save, and an operator not on this list is a specification change.

| Operator | Applies to | Meaning |
|---|---|---|
| `eq` · `ne` | any scalar fact | Equal / not equal |
| `in` · `notIn` | any scalar fact, against a list | Member / not a member of the list |
| `gt` · `gte` · `lt` · `lte` | numeric and date facts | Ordinary comparison. **A date fact compares as a date, never as a string** |
| `startsWith` | string facts | Prefix match. The operator ICD-chapter and ICD-block tests are written with — `lineDiagnosisCode startsWith Q` |
| `between` | numeric and date facts | Inclusive at both ends, and **stated as inclusive here** because half the industry reads it the other way |

**Three grouping keywords, and no others:** **`all`** (every child expression must hold), **`any`** (at least one must), and **`not`** (the single child must not). They nest. There is no `or` and no `and` — `any` and `all` are those operators under names that cannot be confused with a screen label, and one spelling per concept is rule 1 of §0.

> **The grammar is deliberately smaller than it could be.** There is no arithmetic, no string concatenation, no regular expression and no reference from one fact to another. A term is a **test on facts**, not a calculation: the calculation is the fourteen-step chain, it is code, and it is versioned with the release (**P-05**, **S-39**). An expression language that can compute is an expression language somebody will compute a settlement in.

**`counterType` — the dimension a limit counter accumulates in.** The counter key is `(memberId, policyId, benefitCode, periodKey, counterType)`.

| Code | VI | EN | Increments by |
|---|---|---|---|
| `AMOUNT` | Số tiền | Money | The payable amount of the line, integer VND |
| `OCCURRENCE` | Số lần | Occurrences | **One per qualifying event** |
| `DAY` | Số ngày | Days | **The treatment-day count of the line** — which extraction does not return today, **`Q-IDP-06`**. Until it does, a `DAY` counter is maintainable only where the line's treatment date range makes the count derivable |

> **`countUnit` and `counterType` are one dimension in two vocabularies, and this is the crosswalk.** **Registered at v1.5.** It was carried only in A5 §4.2's prose, which meant the ledger's document and the product's document each held half of a two-column mapping.
>
> | `count_unit` on a benefit schedule row (A5 §4.2, seed) | `counterType` on the ledger (D1 §5.9.2) | Increments by |
> |---|---|---|
> | `DAY` | `DAY` | One per distinct calendar treatment date per counter — **`BR-LEDGER-12`** |
> | `VISIT` | **`OCCURRENCE`** | One per distinct **visit** — **`BR-LEDGER-14`** |
> | *(none — `count_limit` is `0`, unlimited)* | no counter of that dimension is maintained | — |
> | — | `AMOUNT` | The payable amount of the line, integer VND. **Maintained on every benefit**, whatever its `count_unit` |
>
> **`VISIT` and `OCCURRENCE` are the same thing under two names**, and the rename happens exactly once, at the ledger boundary. Two names survive because the schedule row is InsureMO's and the counter is ours, and neither side may quietly adopt the other's spelling — **but a document that used them interchangeably without saying so would be describing one mechanism as two.**
>
> **The visit is a derived unit, and that is new at D1 v0.5.** A visit is **a cost line's `treatmentDate` together with its provider**; lines sharing both are one visit (**`BR-LEDGER-14`**). It is derived, never captured — both facts are on every cost line already (A4) and nothing is asked of extraction. Without it, `OCCURRENCE` incremented *once per qualifying event*, and a twelve-session course of physiotherapy is **one** insured event: it consumed 1 of `200.3`'s 20 visits and had its 500,000 per-visit cap applied once to the whole course. **Two visits to the same provider on the same date are one visit**, which is the conventional reading of a per-visit cap and is stated so that it is a decision rather than an implementation detail.

**`periodKey` — which counter period a posting belongs to.** Not a typed list; a **derived key**, registered because §4.1 admits no third state and because getting it wrong is silent.

| Rule | Statement |
|---|---|
| **The period is the policy year** | Derived from the **policy inception date on the snapshot** — **not the calendar year**, and not the member's coverage start (D1 `L-1`) |
| **A line belongs to the period containing its treatment date** | **Never its submission date** (`L-2`). Keying on submission would let a member reset their own limit by delaying a claim, and would make the same treatment consume different periods depending on how fast the customer filed |
| **A claim may span two periods** | Line by line. One claim, two sets of postings (`L-3`), and the remaining-limit figure at calculation is **not a single number** — the console shows it per period |
| **Immutable once posted** | `periodKey` is written on every posting and never recomputed (`L-4`) |

---

### 4.11 The fact vocabulary — CLOSED

**This is the complete list of facts a term's `selector` or `predicate` may test.** It is taken from the *Solution Paper* §2.5 item for item, and it is **closed**: an author cannot invent a fact, the expression builder validates against this list on save, and **if the fact does not exist the term cannot be class `D`** — it falls to a checkpoint instead. **Adding a fact is a specification change against the Framework, not a configuration change** (§10), and the list is frozen per release.

| Fact | Level | Source | **Available from step** | Status |
|---|---|---|---|---|
| `benefitCode` | Line | **Benefit mapping — A5 §8.1** | **5** | **Held** |
| `lossType` | Event | Claim — captured on the insured event, never derived | **1** | **Held** |
| `conditionClass` | Event | Assessment | **2** | **Held** |
| `treatmentDate` | Line | Document extraction | **1** | **Held** |
| `memberAge` at treatment date | Line | Snapshot + treatment date | **1** | **Derivable** |
| `continuousCoverDays` | Event | `continuousCoverFrom` — I-05, snapshot | **1** | **Held** |
| `claimantCapacity` | Claim | Claim | **1** | **Held** |
| `diagnosisCode` | Event | Extraction | **1** | **Held** |
| `lineDiagnosisCode` | Line | Extraction — IDP | **1** *(the day it lands)* | **Open — `Q-IDP-02`** |
| `serviceCategory` | Line | **Document extraction — IDP.** Twenty values, owned by A4 §4.5 | **1** | **Held** |
| `facilityCountry` | Event | Extraction — IDP | **1** *(the day it lands)* | **Missing — `Q-IDP-05`** |
| `facilityType` | Event | **Nowhere. No field, no screen, no payload, no rule** | **—** | **Missing — no question open** |

**Twelve facts. Three of them are not available**, and two of the three are IDP questions rather than design gaps: **`lineDiagnosisCode`** (`Q-IDP-02`, already open, and its priority **rises** — it now blocks two policy exclusions as well as what it blocked before) and **`facilityCountry`** (`Q-IDP-05`). **The third, `facilityType`, has no question open at all** — see note 4.

#### The *available from step* column — new at v1.5, and it is the column a defect was found through

**A fact can be in this vocabulary and still be null at the moment a term is evaluated**, because the fourteen-step chain (A5 §6.1) *produces* facts as it runs. The column says the earliest chain step at which the fact is dependable. **`1` means before the chain runs at all** — it is on the claim, on the snapshot or on the extraction result by the time step 1 executes, and every term step can see it.

**Read it against `termScope` (§4.10), which is what decides when a term runs:** a **`PRODUCT`**-scope term is evaluated at **step 4**; a **`BENEFIT`** or **`BENEFIT_SUBTREE`** term at **step 7**. So:

| A term of this scope | runs at | may test any fact available from step | may **not** test |
|---|---|---|---|
| `PRODUCT` | **4** | 1, 2, 3 or 4 | **`benefitCode`** — step 5 produces it |
| `BENEFIT`, `BENEFIT_SUBTREE` | **7** | 1 … 7 | *(nothing in this vocabulary)* |

**One line of the pilot product's register broke this, and the failure was silent.** `TERM-HC-006` is `PRODUCT` scope, so it is evaluated at step 4, and its predicate tested **`benefitCode`**, which step 5 produces. The expression evaluated; it evaluated against nothing; and the term either never fired or always fired depending on how an engine treats a null — **on the one row A5 §7.3 relied on as proof that step 4 works**. **D1 v0.5 closes it as `BR-TERM-9`**, a **register-time** rule rather than a run-time one: the term register screen refuses to save a term testing a fact its scope cannot see. **This column is the data that rule is checked against**, which is why it belongs in the annex that owns the vocabulary and not in a list inside D1 that would drift from it. A5 v1.5 rewrites the term over `serviceCategory` and `lossType`, both available from step 1.

**On `conditionClass` — step 2, and it is the only fact in the table that is neither an input nor a step-5 output.** It is determined at assessment and stored on the insured event (§4), and step 2 is the waiting-period test that consumes it. It is dependable from step 2 onward, which is early enough for every term step and too late for step 1.

> **A note on precision, because the review that produced this column recorded `serviceCategory` as *step 4*.** It is available from **step 1** — the extraction layer classifies the cost line before the chain starts. The distinction changes no term and no rule: what `BR-TERM-9` needs is that `serviceCategory` is available **at** step 4, where every `PRODUCT`-scope term runs, and it is. The column is written as *earliest step* throughout so that one reading serves both the register-time check and a reader tracing where a value comes from.

**Four notes, because each one has been got wrong at least once already:**

1. **`serviceCategory` is held, it always was, and its *source* was recorded backwards until v1.5.** The entry read *"Mapping — A5 §8"*. **That is the wrong direction.** A5 §8.1 opens by saying the extraction layer classifies each cost line into a service category; the mapping table then **consumes** it and **produces** `benefitCode`. D3 §I-15 carries it on the wire and **A4 §4.5 owns the set** (§4.1) precisely because IDP derives it. As written, the two facts that a mapping relates pointed at each other and neither pointed at extraction. What *was* genuinely missing were four **values** — `SCREENING`, `VACCINATION`, `OPTICAL`, `HEARING` — and those are **Claim Platform master data, ours to add, one seed change, no vendor question at all** (A5 v1.4). That was the cheapest correction in the whole term analysis and it converted two exclusions from unevaluable to class `D`.
2. **`itemCategory` is not on this list and never will be.** It was proposed as a second taxonomy for a distinction `serviceCategory` already carries. **Two ways to classify one cost line is two ways to get it wrong**, and it was dropped from the design rather than registered here.
3. **The level column is load-bearing.** `lineDiagnosisCode` and `diagnosisCode` are the *same clinical idea at two levels*, and they are two facts because a term testing the wrong one tests the wrong thing: a claim with one event and eight lines has one `diagnosisCode` and up to eight `lineDiagnosisCode`s. A term written against the event-level fact when it meant the line-level one declines the whole event.
4. **`facilityType` was graded *Held* and exists nowhere — corrected to *Missing* at v1.5, and it is the most consequential of the three corrections.** There is **no A4 extraction field** for it, **no D2 screen** that captures it, **no D3 payload** that carries it and **no D1 rule** that reads it. A fact graded *Held* is a fact a term author is entitled to write a **class-`D`** term against, and a class-`D` term may **auto-decline** — so a wrong grade in this column does not merely mislead a reader, **it authorises a decline on a fact the system cannot read.** Unlike the other two gaps it has **no question open against it**: `Q-IDP-05` asks for `facilityCountry`, and a facility *type* — clinic, general hospital, specialist hospital — is a different field with a different value set and a different source. **It needs an IDP question of its own, asked alongside `Q-IDP-05` while that round is still open**, and until one is asked and answered `facilityType` is not available to any term of any class. Nothing in the pilot product uses it today, which is why the wrong grade survived three issues.

---

---

## 5. Claims

| EN | VI | Identifier | Definition |
|---|---|---|---|
| **Claim** | Hồ sơ bồi thường | `claimNo` | The case. Format `CLM-YY-NNNNNN` (Framework §7.5). **Never `claimRef`, never `claimId`** |
| **Insured event** | Sự kiện bảo hiểm | `eventNo` | One episode of treatment within a claim. Format `CLM-26-000417/1`. **One claim carries many events.** Carries its own **occurrence date**, treatment dates, **treatment days**, condition class, provider and cover decision — and **every cost line is allocated to exactly one of them** (D2 §5.6.3 L17, register row **S-07**). Restated at v1.2: before the occurrence date existed, an event was a date range and a provider, and the waiting-period arithmetic had nothing correct to count from |
| **Episode type** | Loại hình điều trị | `episodeType` | `OUTPATIENT` · `INPATIENT` · `DENTAL` · `MATERNITY` · `ACCIDENT`. Five in MVP 1 — declared simplification S-03. **A distinct enum from service category** (A4 §4.5), which also has a `DENTAL` member: the two never appear in the same field, and an episode is a visit while a service category is a cost line. **Restated at v1.2: `episodeType` is a projection of `lossType` (§4.2), not an independent fact.** It stays the captured field because the app, A4 §6's checklist and A5 §8.1's benefit mapping all read it; `lossType` is derived from it and from the condition class, for display |
| **Loss type** | Loại hình tổn thất | `lossType` | The **twelve-value pair** — condition class × treatment setting — carried on the insured event. **Derived, never captured; read by no rule** (D2 §5.7.2 F18). §4.2 holds the values and the relationship to `episodeType`. **Not the same thing as episode type and not the same thing as condition class** — it is both of them together, and its only purpose is that a person can read one label instead of two fields |
| **Claimant** | Người yêu cầu bồi thường | `claimant` | **The party asserting the claim** — the person who says *this loss happened and I am entitled to be paid for it*. **A distinct party from both the insured member and the payee**, and the distinction is not academic: the insured is *whose cover is used*, the payee is *who receives the money*, and the claimant is *who has the standing to ask*. A parent claiming for a minor is all three at once for the money and none of them for the cover. **Modelled on the claim from v1.2** (D2 §5.4.2 F34–F37, register rows **S-47**, **B-07**) with name, identity number, **`claimantCapacity`** and **`claimantRelationship`** (§4.3). Before v1.2 the term was defined here and had no fields anywhere, which is why A3 §4.12's conflict-of-interest comparison against "the claimant's identity number" compared against nothing |
| **Claimant capacity** | Tư cách pháp lý của người yêu cầu bồi thường | `claimantCapacity` | The **legal footing** on which the claimant asks — self, guardian, heir, authorised representative. §4.3. **It drives the document checklist**: a guardian evidences guardianship, an heir evidences inheritance. **Distinct from the family relationship** — see §4.3's note, which is the one place that distinction is argued |
| **Occurrence date** | Ngày xảy ra sự kiện bảo hiểm | `occurrenceDate` | **The date the insured event happened** — the accident, the onset — as opposed to the date it was treated. Constrained: `occurrenceDate ≤ treatmentFrom ≤ today`. **This is the date the waiting-period arithmetic counts from**, and the third signal in duplicate detection (D2 §5.7.2 F16, V-503). **Never conflated with the service date**, which is the date a *cost* was incurred and drives the policy snapshot. For an accident treated three weeks later the two differ by three weeks, and counting from the wrong one approves a claim that should have been declined |
| **Treatment days** | Số ngày điều trị | `treatmentDays` | The number of days an insured event's treatment ran, **counted inclusive of both end dates**, with **same-day treatment counting as one day**. Derived on the event (D2 §5.7.2 F17). It is the input to **step 9** of the **fourteen-step** calculation chain — the count limit (**step 8 until v1.4**, when benefit-level provisos were inserted at 7) — and **without it every inpatient benefit with a day cap is undecidable** (A5 §6.1, register row **S-09**). Note that Reference System A takes this value from its extraction partner rather than computing it; **ours is computed**, so it cannot disagree with the dates beside it |
| **Renewal status** | Tình trạng tái tục | `renewalStatus` | Whether this member's participation **in this policy** is a first-time one or a continuation. §4.6. **An attribute of the participation, not of the policy and not of the person** — the same person may be renewing on one policy and new on another. It gates the waiting-period accumulation rule in A5 §5, **and whether InsureMO can supply it is open item D2-129**. Snapshotted like everything else (P-02); Reference System A deliberately re-reads its equivalent under an open claim and **we deliberately do not**, because a value that can change mid-assessment would have to be recorded at the instant the chain read it |
| **Intake source** | Nguồn tiếp nhận | `intakeSource` | Which channel the claim arrived through — §4.4. Set at registration, **read-only thereafter**, and one of the two measurement anchors on the claim |
| **Beneficiary / payee** | Người thụ hưởng | `beneficiary` | The party the settlement is paid to. **A distinct party** from the insured and the claimant; defaults to the insured. **Mastered by the Claim Platform (F58)**, not by InsureMO — a payee is a party to a *settlement*, not to a contract of insurance |
| **Payout account** | Tài khoản nhận tiền | `payoutAccountRef` | The bank account a settlement is paid into: bank, account number, account name, verified flag. **Claim Platform master data (F58).** A member may hold more than one; the account a claim uses is the one its payee points at, pinned onto the claim's snapshot at registration. It is **not** returned by I-05 — that is the change at v1.1, and a specification that still reads it from the policy read is wrong |
| **Account-name verification** | Xác thực tên chủ tài khoản | `accountNameVerified` | The check that the name on the account matches the name given. The account name is **never typed** — it is populated only by the verification call (D1 BR-PAY-1), and changing the account number clears both the name and the flag (BR-PAY-2). Say `accountNameVerified`, never `verified` — see §8 |
| **Payee and payout-account master** | Danh mục người thụ hưởng và tài khoản nhận tiền | — | The Claim Platform register holding the two entries above, and the one place the payment preconditions in A3 §4.6 read from. **F58.** It exists because the payee and the account are **claim business, not policy business** — a payout account exists to reimburse a claim, and verification is a claim-time control on a claim-time object |
| **Cost line** | Dòng chi phí | — | One itemised charge from a cost statement. The unit of the line-level cover test |
| **Service date** | Ngày điều trị / Ngày phát sinh chi phí | `serviceDate` | The calendar date the cost was incurred. Drives the policy snapshot and the waiting-period test |
| **Policy snapshot** | Bản ghi nhận hợp đồng tại thời điểm điều trị | — | The immutable copy of policy, member and benefit data taken onto a claim. Precisely: *benefit schedule as at the service date, plus policy and member data as they stood at registration* (Framework §7.4 S-05). **From v1.1 it carries more weight, not less**: the schedule lives in a vendor's system, so the snapshot is the only local record of what the contract said when the claim was assessed |
| **Requested amount** | Số tiền yêu cầu | `amountClaimed` | What the customer asks for |
| **Assessed amount** | Số tiền được duyệt | — | What the engine computes as payable |
| **Settlement** | Số tiền chi trả | — | What is actually paid, after approval |
| **Complete and valid file** | Hồ sơ đầy đủ và hợp lệ | — | The legally significant point at which the file is complete, starting the statutory settlement clock. Recorded explicitly and immutably, never derived later from other timestamps |
| **Information request** | Yêu cầu bổ sung hồ sơ | — | A request to the customer naming exactly which documents or fields are missing |
| **Auto-approval** | Duyệt tự động | — | Approval without an assessor. **Three conditions from v1.4** (D1 §5.8, decision **D-5**): the payable amount is at or below the threshold **and nothing was cut** (no line declined, no limit bound), **and** the clinical assessment completed with no recommendation, **and no term checkpoint is open** (§2.1). It fires only on a claim the contract paid in full, small, with nothing flagged. **The third condition follows from the class model and its cost is real and accepted:** any claim touching a judgemental term leaves the straight-through path, because an open checkpoint is by definition a judgement no engine may make |
| **Policy rule assessment** | Đánh giá theo điều kiện điều khoản | — | **Track 1.** The deterministic rule chain on the Claim Platform: cover, waiting periods, **policy terms (steps 4 and 7, §2.1)**, benefit mapping, limits, co-payment. Produces **a decision** |
| **Clinical appropriateness assessment** | Đánh giá phù hợp y khoa | — | **Track 2.** AI reasoning on IDP over the diagnosis and the prescription, test and cost lines. Produces **recommendations**, never decisions |
| **Recommendation** | Khuyến nghị | — | A proposal to decline one cost line, carrying a reason code, a rationale, a confidence and cited evidence. **Binds nothing until a human confirms it.** Distinct from a decision and from data |
| **Clinical knowledge base** | Cơ sở tri thức y khoa | — | The curated, versioned content behind Track 2. Contains clinical and product knowledge, **never patient data**. Every claim records the version in force when it was assessed |
| **Agreement rate** | Tỷ lệ đồng thuận | — | How often an assessor confirms a recommendation rather than rejecting it. The only real measure of whether Track 2 is good enough |
| **Overpayment recovery** | Thu hồi chi trả thừa | — | **Out of scope in MVP 1**, stated so it is a decision rather than an omission |

### 5.1 Claim lifecycle states

**Thirteen states, one machine** — declared simplification S-01. (Earlier drafts said twelve: the count omitted `EXTRACTION_ERROR`, which is a branch but is still a state a claim can be in. Corrected here and in Framework §5.3 on 17/08/2026.)

| Identifier | VI | Meaning |
|---|---|---|
| `DRAFT` | Nháp | Started in the app, not yet submitted |
| `SUBMITTED` | Đã gửi | Received; awaiting document processing |
| `DOC_PROCESSING` | Đang xử lý tài liệu | With IDP |
| `EXTRACTION_ERROR` | Lỗi xử lý tài liệu | Recoverable branch from `DOC_PROCESSING`. **An operational state, never a decline** |
| `PENDING_INFO` | Chờ bổ sung hồ sơ | Awaiting documents or a hard-copy original from the customer |
| `UNDER_ASSESSMENT` | Đang thẩm định | With an assessor |
| `PENDING_APPROVAL` | Chờ phê duyệt | Assessed; awaiting an approver |
| `APPROVED` | Đã duyệt | Payable. **Ledger consumption is final at this point** |
| `DECLINED` | Từ chối | Not payable, with a reason from §5.2 |
| `PAYMENT_IN_PROGRESS` | Đang chi trả | Instruction sent; awaiting confirmation |
| `PAID` | Đã chi trả | Payment confirmed |
| `CLOSED` | Đã đóng | Terminal |
| `CANCELLED` | Đã hủy | Withdrawn before approval |

### 5.2 Decline reasons

| Code | EN | VI | Level |
|---|---|---|---|
| `LIMIT` | Benefit limit exhausted | Hết hạn mức | Event or line |
| `PERIOD` | Event date outside the cover period | Ngày xảy ra sự kiện BH không thuộc thời hạn BH | Event |
| `NOBEN` | The plan does not include this benefit | Chương trình bảo hiểm không bao gồm quyền lợi này | Event or line |
| `WAIT` | Waiting period not yet served | Chưa đủ thời gian chờ | Event or line |
| `EXCL` | Falls under a policy exclusion | Thuộc điểm loại trừ của quy tắc BH | Event or line |
| `NOTREL` | Not related to the diagnosis or treatment | Không liên quan đến chẩn đoán và điều trị | **Line only** |
| `NOTCOV` | Item is not a covered medical item | Không thuộc danh mục được chi trả | **Line only** |

> **A decline reason is never used for a system failure.** If a service is unavailable, the claim takes an operational state and an operator sees it. A claim declined because something was broken is indistinguishable from a correct decline, and that is the worst outcome this system can produce (D3 G-3).

---

## 6. Documents and extraction

| EN | VI | Identifier | Definition |
|---|---|---|---|
| **Document package** | Bộ hồ sơ | — | All documents submitted for one claim. The unit IDP processes |
| **Document type** | Loại tài liệu | `T1-01` … | Seven in MVP 1 — declared simplification S-02. Defined in A4 |
| **Cost statement** | Bảng kê chi phí khám chữa bệnh | `T1-01` | The itemised bill. The primary extraction target |
| **Prescription** | Đơn thuốc | `T1-02` | |
| **Examination record** | Phiếu khám bệnh | `T1-03` | |
| **VAT invoice** | Hóa đơn GTGT | `T1-04` | **Parsed from e-invoice XML only.** A4 §1.5 closes the OCR path for invoice images deliberately — an invoice read by OCR cannot be verified, and an unverifiable invoice is the main fraud surface |
| **Identity document** | Giấy tờ tùy thân | `T2-01` | Classification only. CCCD, hộ chiếu or giấy khai sinh |
| **Laboratory and imaging result** | Kết quả cận lâm sàng | `T2-02` | Classification only. Supports the "test unrelated to diagnosis" decline |
| **Medical report / surgery certificate** | Báo cáo y tế / Giấy chứng nhận phẫu thuật | `T2-03` | Classification only |
| **Extraction request** | Yêu cầu trích xuất | `requestId` | One submission to IDP. Its result is **immutable** |
| **Per-field confidence** | Độ tin cậy theo trường | `confidence` | A score in `[0,1]` per extracted field |
| **Claim-level confidence** | Độ tin cậy hồ sơ | `confidence` | The **minimum** across the mandatory field set — deliberately not the mean, so one badly-read amount cannot be averaged away |
| **Package completeness state** | Trạng thái đầy đủ hồ sơ | `state` | `COMPLETE`, `MISSING_DOCUMENT`, `MISSING_FIELD`, `INCONSISTENT`, `AWAITING_ORIGINAL`, `ERROR`. Produced by IDP, **consumed** by the claim engine, never re-derived |
| **Requirement level** | Mức độ bắt buộc | — | `REQUIRED` blocks settlement · `EXPECTED` triggers an information request the assessor may waive · `OPTIONAL` never blocks and is never requested |
| **Copy vs original** | Bản sao / Bản gốc | — | Whether the physical original is required and received |
| **Ground-truth set** | Bộ dữ liệu chuẩn | — | Manually verified documents used to build and measure extraction accuracy. 25–30 anonymised packages across at least four hospitals |

---

## 7. Roles

Four roles in MVP 1. A3 is the normative permission matrix; this fixes the names.

| EN | VI | Identifier | Definition |
|---|---|---|---|
| **Customer** | Khách hàng | `CUSTOMER` | The app user: policyholder or insured member. Mobile only |
| **Assessor** | Cán bộ thẩm định | `ASSESSOR` | Reviews extraction, confirms coverage, computes and submits the settlement plan. **Cannot approve** |
| **Approver** | Cán bộ phê duyệt | `APPROVER` | Approves or rejects a settlement plan. **Cannot assess the claim they approve** |
| **Administrator** | Quản trị hệ thống | `ADMIN` | Users, roles, thresholds, document types, master data. **No claim decision rights** |

> **Assessor and approver are separated on purpose.** One person doing both removes the only control on the amount paid. A3 §4 states the segregation rule and how it is enforced.

---

## 8. Terms that must never be used

Recorded because each of these appeared in a project document and caused, or nearly caused, a defect.

| Do not use | Use instead | Why |
|---|---|---|
| **Core Simulator**, **Policy Data Store**, **Kho dữ liệu hợp đồng** | Name the system that holds the thing: **InsureMO** for the benefit schedule, the policy, the member and the coverage register (F05, F07); **Claim Platform** for the payout account and the payee (F58); **Claim Platform** for the group bulk load (F59) | "Core Simulator" was renamed to "Policy Data Store" at Framework v1.1 because two names for one component made reviewers assume two components. **At v2.0 the component itself is retired** — InsureMO demonstrated it can carry the adjudicable benefit schedule, so the store is not built. Both names are now banned, and for a stronger reason than the first: they name a **system that does not exist**, and a specification that reads a field "from the store" has no source at all. Feature codes **F11–F14 are retired with it and never reused** — a document that still says F12 must resolve to *deleted*, not to whatever inherited the number |
| **`level`**, unqualified | `depth` (tree) or `benefitLevelNo` (plan on a policy) | The single most dangerous collision in this project: both senses appeared in one payload |
| **`claimRef`**, `claimId` | `claimNo` | One name per concept — D3 §1.2 |
| **`eventRef`** | `eventNo` | Same |
| `SPECIAL`, `OTHER`, `MATERNITY`, `MAT_COMPLICATION` **as condition-class codes** | `SPE`, `GEN`, `BIR`, `MAT` | Non-canonical codes coined from prose. `MATERNITY`/`MAT` in particular swap a 270-day and a 90-day waiting period. Note this ban is scoped to condition classes: `MATERNITY` remains valid as an **episode type**, which is a different enum (§5) |
| **"waiting period" (singular, per benefit)** | waiting period **by condition class** | A benefit has up to six, not one |
| **"limit" unqualified** | annual sum insured · per-event limit · per-day limit · count limit | Four different things, four different behaviours |
| **"level" for a sub-limit** | child benefit | A sub-limit is a row in the tree, not an attribute |
| **`verified`** on an account | `accountNameVerified` | Says what was verified. "Verified" alone invites the assumption that the account itself was validated |
| **"the OCR result"** | the extraction result | IDP does more than OCR — classification, validation and reconciliation. The narrow word invites a narrow design |
| **"the AI decided"** / "the AI declined" | the AI **recommended**; the assessor decided | Track 2 produces recommendations. Language that says the AI decided describes a system we deliberately did not build, and it is the exact claim a disputing customer will quote back |
| **"rule engine"** unqualified | **policy rule assessment** (Track 1) or **clinical appropriateness assessment** (Track 2) | Two mechanisms, two authorities. Calling both a rule engine hides that only one of them is deterministic |
| **"the system"** | name the system | With three systems in the solution, "the system" is never unambiguous — and it is *more* dangerous at three than at four, because the reader's default guess is now a vendor's platform rather than one of ours |
| **"the claimant"** where the **insured member** or the **payee** is meant | name the party | **Three parties, three questions**: whose cover is used, who has standing to ask, who receives the money. They coincide on most claims and diverge on exactly the claims that go wrong — a minor, a deceased member, a claim brought by an authorised representative. A rule written against "the claimant" that meant the payee is a rule that pays the wrong person |
| **`dateType`** | **`dateFilterType`** | `dateType` was the difference register's informal name (rows S-48, B-26). A bare `dateType` invites use on a document date, a treatment date or an occurrence date, and this set is about **none** of those — it names which *claim-level milestone date* a list filter applies to (§4.5) |
| **`relationship`** where **`claimantRelationship`** is meant, and the reverse | name which one | Two different sets with overlapping members. **`relationship`** (`SELF` · `SPOUSE` · `CHILD` · `PARENT`) sits on the **beneficiary** record and describes who the *money* goes to; **`claimantRelationship`** (eight values, §4.3) sits on the **claimant** and describes who they are to the patient. Mixing them is how a payment ends up validated against the wrong party |
| **"loss type"** and **"episode type"** interchangeably | say which | `episodeType` is captured and read by three documents; `lossType` is derived and read by none (§4.2). They are not synonyms and they must never be allowed to disagree |
| **"occurrence date"** and **"service date"** interchangeably | say which | The event happened on one date and the cost was incurred on another. **Waiting periods count from the occurrence date; the policy snapshot is taken as at the service date.** Swapping them approves accident claims that fell inside a waiting period |
| **`predicate`** where **`selector`** is meant, and the reverse | say which | **The likeliest modelling error in the term layer, and it fails silently in both directions.** `selector` decides whether a term is **in play** and is mandatory on every class; `predicate` decides whether it **fires** and is permitted on class `D` alone (§2.1, D2 `V-734`). A class-`J` term with the two confused either raises a checkpoint on every claim in the portfolio, or auto-declines on a judgement no human made |
| **"the rule set"** where **the term register** is meant, and the reverse | name which | **Two systems, two owners.** `ruleSetRef` is **InsureMO's** identifier for the wording a policy was written under; the **term register** is **ours** and holds what those terms mean operationally (§2.1). *"The rules come from InsureMO"* is true of the first and false of the second, and a specification that reads a predicate "from the rule set" has no source at all — it would also breach **P-01** |
| **"the rule engine" for the term engine** | **term evaluation service** (F66) | The existing ban on *"rule engine"* unqualified now has a third claimant. F66 evaluates **terms**; the fourteen-step chain around it is code. Calling F66 the rule engine implies the arithmetic is configurable, which **P-05 (revised)** says it is not |
| **"a checkpoint was declined"** / "the checkpoint declined the line" | the checkpoint was **upheld**; the **assessor** declined the line | A checkpoint is a question, not an outcome. **An uphold is a person's decline**, and the language has to say so — this is the same discipline as *"the AI recommended, the assessor decided"*, applied to terms |
| **"reserve"** for the dashboard's *money in flight* tile | **money in flight** | Framework **S-16**: MVP 1 has no reserving of any kind. The tile sums requested and computed-payable amounts on open claims. **It is not a reserve, it has no approval round behind it, and calling it one would be a statement to finance that is not true** |

---

## 9. Vietnamese terms kept untranslated

Some Vietnamese terms have no clean English equivalent in this domain, and translating them loses precision. They are used in Vietnamese in specifications.

| Term | Meaning in this project |
|---|---|
| **BHYT** (Bảo hiểm y tế) | Vietnam's social health insurance scheme. **In MVP 1 amounts it pays are captured but *not* deducted** — declared simplification S-17, decided 17/08/2026. The cost-statement fields (A4 L8, L9) are extracted and stored so the deduction is a rule step away, not a re-design |
| **BHYT co-payment** (Người bệnh cùng chi trả) | The patient's share **under social insurance rules** — typically 20% of BHYT-covered cost. **Not the same thing as the private policy's co-payment** (`coPaymentBp`), which is a term of our contract. Two different mechanisms with confusingly similar Vietnamese names; never conflate them |
| **Bảng kê chi phí** | The itemised hospital cost statement — `T1-01`. "Cost statement" is the English term used, but this is what a provider calls it |
| **Hóa đơn GTGT** | VAT invoice conforming to Ministry of Finance e-invoicing standards |
| **Số tiền đồng chi trả** | The member's co-payment share as printed on a cost statement |

---

## 10. Change control

A term changes here **before** it changes anywhere else. Adding a term is additive and needs no approval; changing or retiring one is a change request against the Framework, because it invalidates text in every document that used it.

When a term is retired, it moves to §8 with the replacement and the reason. §8 is not a tidy-up list — it is the record of what went wrong, and it is why this annex works.

**From v1.5 the fact vocabulary carries a second dimension, and it is governed the same way.** The *available from step* column is part of the contract, not commentary on it: **changing the step at which a fact becomes available is the same kind of change as adding or removing a fact**, because a term already saved against it may become unevaluable. Any change to the fourteen-step chain that moves where a fact is produced is a change to this table in the same release, and `BR-TERM-9` is re-checked across the whole register.

**From v1.4 the fact vocabulary (§4.11) is stricter than either.** It is not a value set with a governing document elsewhere; it is a **contract with the term engine**. **Adding a fact is a specification change against the Framework and a release**, because a fact is a value the engine must be able to read off a claim or a snapshot, and an author offered a fact the engine cannot supply writes a term that silently never fires. **Removing one is a change request twice over**, because a term already authored against it becomes unevaluable and a claim already evaluated under it becomes unexplainable. The list is **frozen per release** and named in the release note.

**From v1.2 the same discipline applies to value sets.** A set is added to §4.1 before it appears in a screen, a payload or a column name. **Adding a *value* to an existing set is additive and needs no approval** — a new intake channel, a new cancellation reason, a new province — because the sets were built to take one. **Adding a *set*, or removing a value from one, is a change request**, because either may invalidate a stored record: a value that has been written to a claim cannot be removed from the set that explains it.

*End of A1 v1.5.*
