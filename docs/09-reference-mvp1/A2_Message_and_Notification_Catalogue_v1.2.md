# A2 — Message and Notification Catalogue (Bilingual)

**Project:** Digital Health Insurance Platform — MVP 1
**Document type:** Annex A2 — every piece of text the system says to a person
**Version:** 1.2
**Prepared by:** BnK Solutions — Business Analysis
**Date:** 25 August 2026 (17/08: written. 18/08: re-baselined against Framework v1.5; A2-02 closed by D1 B-06. 24/08: staff become a recipient group — §8.1; manual re-send is defined and bounded — §8.2. **25/08: the term layer gets its messages — §8.3, `CLM-CONSOLE-019` … `-027`; `EXCL` and `CLM-LETTER-002` are rebuilt to carry what a term produces; three staff notifications added; A2-06 closed**)
**Annex to:** *Solution Framework v2.5* (F61 notification and correspondence engine)
**Companion:** *A1 Glossary v1.5* — all terminology here is drawn from it
**Supersedes:** *A2 Message and Notification Catalogue v1.1* (24 August 2026)
**Classification:** Internal

> **Change note — v1.1. Until now this catalogue had one recipient: the customer.** Staff appeared only as readers of `CONSOLE` messages, which D2 §6.1.2 is explicit are **not sent** — they are synchronous responses to an action, with no queue, no retry and no outbox. The consequence, which register row **B-33** made unarguable, is that **a member of staff learns nothing about their own work unless they happen to be looking at the right screen at the right moment.** A claim assigned to somebody on leave is discovered by the overdue bucket — that is, after it is late.
>
> **§8.1 adds the staff recipient group and eight messages**, on a new `STAFF_INBOX` channel. **The engine is not being extended**: the outbox, the versioned templates, the immutability constraint, the retry schedule and the operator queue on failure all exist (D2 §6.1.5–§6.1.7). What was missing was a recipient group and eight sentences.
>
> **§8.2 defines the manual re-send, and defines it narrowly.** Register rows **B-34** and **S-44** asked for what Reference System A does — an officer picks a letter type from a menu filtered by the claim's state and sends it to an address they type. **We take the address and refuse the menu**, and §8.2 argues why: a console control that *chooses* which message to send moves ownership of that decision out of this catalogue, and a catalogue that does not know what the system sends is not a catalogue. **Re-sending is a delivery act. Composing is not on the table** (§9 is unchanged on that point).
>
> **Codes added: `STAFF-NOTIF-001` … `-008`, `CLM-CONSOLE-016` … `-018`.** Nothing is retired and no wording changes.

---

> **Change note — v1.2. The term layer has been specified for a day and a half and this catalogue had not a single sentence in it.**
>
> **The gap, stated plainly.** F65 to F67 put contractual terms into the system as data. A term validates, is approved by a second person, may be back-dated, may be corrected, raises a checkpoint, and blocks a settlement plan. **Every one of those is something the system says to a person, and A2 carried `CLM-CONSOLE-001` … `-018` and nothing for terms.** D2 v0.6 raised four items for it — `D2-153`, `D2-154`, `D2-155`, `D2-156` — **de-numbered its own proposals and stopped**, on `D2-88`'s instruction that *A2 allocates every console code above `-015` in one pass*. **§8.3 is that pass for the term layer**, and it allocates **`CLM-CONSOLE-019` to `-027`**, honouring §11's rule that a code is permanent and never reused.
>
> | Family | Codes | The D2 item it answers |
> |---|---|---|
> | **Term validation** — the five content refusals on one screen | `-019` | `D2-153` (a)–(e) |
> | **The two governance refusals** — self-approval, back-dating | `-020`, `-021` | `D2-153` (f), (g), `V-736`, `V-737` |
> | **The correction confirmation**, with its impact count | `-022` | `D2-154`, `V-738` |
> | **Return to draft** | `-023` | `D2-153` (h) |
> | **The checkpoint outcomes**, and the refusal of one | `-025`, `-026` | `D2-155`, `V-741` |
> | **Settlement-plan submission refused on an open checkpoint** | `-027` | `D2-156`, `V-732`, D1 `BR-TERM-13` |
>
> **`-024` is reserved and not allocated, and that is a decision rather than an oversight** — §8.3 says why.
>
> **Two customer-facing repairs, and they are the half nobody had asked for.** **`D2-178`** found that the `EXCL` decline template **cannot carry what the term engine produces**: it binds neither the item nor the amount where `NOTREL` and `NOTCOV` both do, and it carries **one** clause reference where the engine returns a **list**. `CLM-LETTER-002`'s *Basis* section is singular in the same way. **§4 and §7.2 are rebuilt for it**, and until they were, D2 §5.29.3 was carving `V-531` out of every `EXCL` rationale so that an uphold could be submitted at all. **And §7.2 gains a third thing the decline letter must never do — render an unissued clause reference** — because all ten pilot terms carry `TBC-nn` placeholders and D1 v0.5's **`BR-TERM-10`** forbids declining on one. **A2-04 is restated as what it now is:** the item that currently stops any exclusion decline leaving the building.
>
> **`DOC-NOTIF-006` is new**, and it exists because `DOC-NOTIF-001` reads as a routine missing item. A class-E `REQUIRE_DOC` term is not a routine missing item — it is *we need a statement because an exclusion may apply* — and asking for it in the same words as a missing prescription is how a customer sends the wrong thing twice.
>
> **`LIMIT` and `CLM-CONSOLE-003` gain `{periodKey}`.** `V-730` renders the ledger once per period and forbids summing; `L-3` posts a boundary-spanning claim to two periods. A single *remaining limit* figure with no period on it is a number that is wrong for one of them.
>
> **§8.1 gains three notifications** on its own test — *would a person who is not looking at this claim need to know?* A checkpoint raised on a claim you hold, a correction that moved your claim back to the queue, and a term waiting on your approval all pass it, and none of the three notified anybody.
>
> **A2-06 is closed.** D2 §6.1.9 closed `D2-127` on 24/08 — Part VI carries the staff channel and now excludes what §9 excludes. The contradiction §8.1 recorded no longer exists and the note says so rather than being quietly deleted.
>
> **Nothing is retired.** `{exclusionRef}` is superseded by `{clauseRefList}` as a **placeholder name**, which §0.2's permanence rule does not cover and §4 explains.

---

## 0. What this is for

Every message a system sends is a small product decision, and if those decisions are made one at a time inside screen specifications they come out inconsistent: the same failure phrased four ways, a decline the customer cannot act on, an English string on a Vietnamese screen. This annex takes the decisions once.

**Scope.** Validation errors, claim status notifications, information requests, settlement and decline correspondence, system errors, **and — from v1.1 — notifications to the insurer's own staff** (§8.1) — for the customer app, the web console, push, email, SMS and the console's staff inbox.

**Out of scope.** Screen labels, field names and button text — those are D2. A message is something the system *tells* you happened, not something a screen is *called*.

### 0.1 The rules every message obeys

These are not style preferences. Each one exists because its absence produces a support call or a complaint.

| # | Rule | Why |
|---|---|---|
| **M-1** | **Vietnamese is the primary language.** The customer app is Vietnamese-first; English exists for the console and for testing | The customers are Vietnamese. The English column is for the team, not the market |
| **M-2** | **Every message says what to do next**, or explicitly says nothing is required | "Hồ sơ chưa hợp lệ" tells the customer nothing. "Thiếu Bảng kê chi phí — vui lòng chụp và tải lên" tells them exactly what to do |
| **M-3** | **Never expose an internal state name, code or identifier to a customer.** `PENDING_INFO` and `NOTCOV` are for the console | A customer reading `EXTRACTION_ERROR` learns only that something is broken |
| **M-4** | **A money amount is always formatted and always carries its unit** — `1.200.000 đ`, with `.` as the thousands separator per Vietnamese convention | A bare `1200000` in a settlement notice reads as an error |
| **M-5** | **A decline always names the reason and the benefit or item it applies to.** Never a bare "từ chối". *One exception, stated so it is a decision: the push and summary notification (`CLM-NOTIF-006`) says only that a result exists — the reason lives one tap away in the app and in the letter, because §0.3 forbids putting a decline reason on a lock screen* | The customer's next question is always "why", and the assessor's time is the cost of not answering it |
| **M-6** | **A system failure is never phrased as a decision.** "Hệ thống đang xử lý, chúng tôi sẽ thông báo khi có kết quả" — never "hồ sơ không hợp lệ" | D3 G-3: a claim stalled by an outage must not look declined |
| **M-7** | **No message blames the customer.** "Ảnh chưa đủ rõ để đọc" not "Bạn đã chụp ảnh sai" | The photo is usually fine; our reader is what failed |
| **M-8** | **Placeholders are named, never positional** — `{benefitName}`, not `{0}` | A translator reordering a sentence must not change its meaning |
| **M-9** | **One message, one channel decision.** Every message states its channel(s). A message with no channel is not implemented | Otherwise the same event notifies twice, or not at all |
| **M-10** | **Tone: plain, respectful, no exclamation marks.** Address the customer as *Quý khách* in correspondence, *bạn* nowhere | This is insurance. Cheerfulness reads as insincerity when a claim is declined |

### 0.2 Structure of a message code

```
<AREA>-<TYPE>-<NNN>          e.g. CLM-NOTIF-004
```

| Segment | Values |
|---|---|
| `AREA` | `AUTH` · `POL` (policy and purchase) · `CLM` (claim) · `DOC` (documents) · `PAY` (payment) · `SYS` (system) · **`STAFF`** (addressed to a member of the insurer's staff — §8.1) |
| `TYPE` | `VAL` validation · `NOTIF` notification · `LETTER` correspondence · `ERR` system error · `CONSOLE` console-only message for staff (§8). Note `CONSOLE` is also the name of a channel in §0.3 — the token appears in both positions and means the same thing: staff-facing, never seen by a customer |
| `NNN` | Sequence within area and type |

Codes are permanent. A retired message keeps its code; the code is never reused.

> **`STAFF` and `CONSOLE` are both staff-facing and they are not the same thing, and confusing them is the mistake this catalogue most invites.** A `CLM-CONSOLE-nnn` message is the **answer to something a person just did** — it is rendered on the screen they are already looking at, it is not queued, not retried and not stored. A `STAFF-NOTIF-nnn` message is **sent to a person who is not looking**, and it goes through the outbox with everything that implies: a delivery outcome, a retry schedule, and a row in the claim's correspondence history. **The test is one question — is there a person waiting for this response?** If yes, it is `CONSOLE`. If the event happened whether or not anyone was watching, it is `STAFF`.

### 0.3 Channels

| Channel | Used for | Notes |
|---|---|---|
| `IN_APP` | Everything the customer needs to see | The timeline on the claim is the record |
| `PUSH` | State changes the customer would otherwise wait for | Never carries an amount or a diagnosis — a lock-screen preview is visible to anyone holding the phone |
| `EMAIL` | Correspondence with a document attached | Settlement and decline letters. **From v1.1, `EMAIL` may address a member of staff as well as a customer** — the constraint was always the recipient group, never the channel |
| `SMS` | Nothing in MVP 1 | Specified as a channel so the engine supports it; no message uses it |
| `CONSOLE` | Assessor and approver messages | Never shown to a customer. **Synchronous — not a delivery channel.** No queue, no retry, no outbox row (D2 §6.1.2) |
| **`STAFF_INBOX`** | **Notifications to a named member of staff** — the bell on the console shell, readable from every screen | **New at v1.1.** A real delivery channel: queued, stored, and carrying a read/unread state. **A notification is addressed to a user and no role reads another user's** (D2 §5.23, A3 §5) |

### 0.4 Recipient groups

**Every message in this catalogue is addressed to exactly one group, and the group — not the channel — decides what the message may contain.**

| Group | Who | What it may contain | What it may never contain |
|---|---|---|---|
| **Customer** | The app user who submitted the claim, or the policyholder | Everything in §§1–7, under rules M-1 to M-10 | An internal state name, a code, an identifier (M-3), an assessor's internal note, or an AI draft before it is finalised |
| **Staff** | A named `ASSESSOR`, `APPROVER` or `ADMIN` | §8's console messages and §8.1's notifications. Internal identifiers are **expected**, not hidden — M-3 is a customer rule and is scoped to that group | **Clinical detail in a `STAFF_INBOX` notification** — no diagnosis, no ICD-10, no item name, no amount (§8.1 rule S-3) |

> **One rule earns its own line.** M-3 forbids exposing an internal state name to a **customer**. It has never applied to staff, and §8 has always named `benefitCode` and `conditionClass` freely. **Stating the scope matters now** that a second staff-facing group exists — otherwise the first person to write a `STAFF-NOTIF` message reads M-3 and writes a sentence too vague to act on, which is the failure that produced M-2.

> **On push notifications and privacy.** A push preview is readable without unlocking the phone. No push message in this catalogue contains a settlement amount, a diagnosis, a benefit name or a decline reason — it says a result is ready and the customer opens the app. This is a deliberate constraint, and it is why the push and in-app text differ for the same event.

---

## 1. Authentication and account — `AUTH`

| Code | Type | Trigger | VI | EN | Channel |
|---|---|---|---|---|---|
| `AUTH-VAL-001` | VAL | Phone number malformed | Số điện thoại không hợp lệ. Vui lòng nhập 10 chữ số. | Invalid phone number. Please enter 10 digits. | `IN_APP` |
| `AUTH-VAL-002` | VAL | OTP wrong | Mã xác thực không đúng. Số lần thử còn lại: {attemptsLeft}. | Incorrect verification code. {attemptsLeft} attempts remaining. | `IN_APP` |
| `AUTH-VAL-003` | VAL | OTP expired | Mã xác thực đã hết hạn. Vui lòng yêu cầu mã mới. | Verification code has expired. Please request a new one. | `IN_APP` |
| `AUTH-VAL-004` | VAL | Too many attempts | Đã vượt quá số lần thử cho phép. Vui lòng thử lại sau {minutes} phút. | Too many failed attempts. Please try again in {minutes} minutes. | `IN_APP` |
| `AUTH-VAL-005` | VAL | Member linking failed | Không tìm thấy hợp đồng phù hợp với thông tin đã nhập. Vui lòng kiểm tra lại số hợp đồng và số giấy tờ tùy thân. | No policy matches the details entered. Please check the policy number and ID number. | `IN_APP` |

> `AUTH-VAL-005` deliberately does not say *which* of the two values was wrong. Telling an unauthenticated caller that a policy number exists but the ID does not is an enumeration weakness.

---

## 2. Purchase journey — `POL`

| Code | Type | Trigger | VI | EN | Channel |
|---|---|---|---|---|---|
| `POL-VAL-001` | VAL | Age outside the product's band | Sản phẩm này áp dụng cho người từ {minAge} đến {maxAge} tuổi. | This product is available for ages {minAge} to {maxAge}. | `IN_APP` |
| `POL-VAL-002` | VAL | Health declaration disqualifies | Với thông tin sức khỏe đã khai báo, chúng tôi chưa thể cấp đơn trực tuyến. Vui lòng liên hệ {contactChannel} để được tư vấn. | Based on the health information provided, we cannot issue this policy online. Please contact {contactChannel}. | `IN_APP` |
| `POL-VAL-003` | VAL | Quote expired | Báo giá đã hết hiệu lực. Vui lòng lập báo giá mới. | This quote has expired. Please request a new one. | `IN_APP` |
| `POL-NOTIF-001` | NOTIF | Policy issued | Hợp đồng {policyNo} đã được cấp. Hiệu lực từ {effectiveFrom} đến {effectiveTo}. | Policy {policyNo} has been issued, effective {effectiveFrom} to {effectiveTo}. | `IN_APP`, `PUSH`, `EMAIL` |
| `POL-NOTIF-002` | NOTIF | Certificate available | Giấy chứng nhận bảo hiểm của Quý khách đã sẵn sàng. | Your insurance certificate is ready. | `IN_APP`, `EMAIL` |
| `POL-LETTER-001` | LETTER | Certificate email | *(template — see §7.3)* | | `EMAIL` |

**`POL-VAL-002` is the one to get right.** A declined application is a lost customer and a possible complaint. It does not say *which* answer disqualified them — that is medical information the app should not surface in a validation message — and it always routes to a human.

---

## 3. Claim submission and documents — `CLM`, `DOC`

### 3.1 Validation at submission

| Code | Trigger | VI | EN | Channel |
|---|---|---|---|---|
| `CLM-VAL-001` | Service date in the future | Ngày điều trị không thể là ngày trong tương lai. | The treatment date cannot be in the future. | `IN_APP` |
| `CLM-VAL-002` | Service date before cover started | Ngày điều trị {serviceDate} nằm ngoài thời hạn bảo hiểm ({coverageFrom} – {coverageTo}). | Treatment date {serviceDate} is outside the cover period ({coverageFrom} – {coverageTo}). | `IN_APP` |
| `CLM-VAL-003` | No document attached | Vui lòng tải lên ít nhất một chứng từ để tiếp tục. | Please upload at least one document to continue. | `IN_APP` |
| `CLM-VAL-004` | Requested amount not positive | Số tiền yêu cầu phải lớn hơn 0. | The requested amount must be greater than zero. | `IN_APP` |
| `CLM-VAL-005` | Payout account not verified | Chưa xác thực được tên chủ tài khoản. Vui lòng kiểm tra lại số tài khoản và ngân hàng. | We could not verify the account holder's name. Please check the account number and bank. | `IN_APP` |
| `CLM-VAL-006` | File too large / wrong format | Tệp {fileName} vượt quá {maxSize} hoặc không đúng định dạng. Định dạng hỗ trợ: JPG, PNG, PDF. | {fileName} exceeds {maxSize} or is not a supported format. Supported: JPG, PNG, PDF. | `IN_APP` |

### 3.2 Document capture guidance

Shown at capture, before submission. These prevent the majority of extraction failures at the cheapest possible moment.

| Code | Trigger | VI | EN | Channel |
|---|---|---|---|---|
| `DOC-VAL-001` | Image blurred | Ảnh chưa đủ rõ để đọc. Vui lòng chụp lại ở nơi đủ sáng, giữ máy cố định. | This image is not clear enough to read. Please retake it in good light, holding the camera steady. | `IN_APP` |
| `DOC-VAL-002` | Edges cropped | Có vẻ chứng từ bị cắt mất một phần. Vui lòng chụp lại toàn bộ trang. | Part of the document appears to be cut off. Please capture the whole page. | `IN_APP` |
| `DOC-VAL-003` | Glare detected | Ảnh bị lóa. Vui lòng tránh ánh sáng chiếu trực tiếp. | There is glare on the image. Please avoid direct light. | `IN_APP` |
| `DOC-VAL-004` | Type differs from the customer's choice | Chứng từ này có vẻ là {detectedType}, không phải {declaredType}. Chúng tôi sẽ xử lý theo nội dung thực tế. | This looks like a {detectedType} rather than a {declaredType}. We will process it based on its actual content. | `IN_APP` |

> `DOC-VAL-004` informs, it does not ask. Per D3 §I-07 the classification wins over the customer's choice — but silently overriding what someone told you erodes trust, so the message says what happened and that nothing is required of them.

### 3.3 Claim status notifications

| Code | State | VI (in-app) | EN | Push text (VI) | Channel |
|---|---|---|---|---|---|
| `CLM-NOTIF-001` | `SUBMITTED` | Đã nhận hồ sơ bồi thường {claimNo}. Chúng tôi sẽ thông báo khi có kết quả. | Claim {claimNo} received. We will notify you when there is an update. | Đã nhận hồ sơ {claimNo} | `IN_APP`, `PUSH` |
| `CLM-NOTIF-002` | `DOC_PROCESSING` | Đang xử lý chứng từ của Quý khách. | We are processing your documents. | *(no push)* | `IN_APP` |
| `CLM-NOTIF-003` | `PENDING_INFO` | Hồ sơ {claimNo} cần bổ sung chứng từ. Xem chi tiết trong ứng dụng. | Claim {claimNo} needs additional documents. | Hồ sơ {claimNo} cần bổ sung chứng từ | `IN_APP`, `PUSH`, `EMAIL` |
| `CLM-NOTIF-004` | `UNDER_ASSESSMENT` | Hồ sơ đang được thẩm định. | Your claim is being assessed. | *(no push)* | `IN_APP` |
| `CLM-NOTIF-005` | `APPROVED` | Hồ sơ {claimNo} đã được duyệt. Số tiền chi trả: {settlementAmount}. | Claim {claimNo} approved. Settlement: {settlementAmount}. | Đã có kết quả hồ sơ {claimNo} | `IN_APP`, `PUSH`, `EMAIL` |
| `CLM-NOTIF-006` | `DECLINED` | Hồ sơ {claimNo} không thuộc phạm vi chi trả. Xem lý do chi tiết trong ứng dụng. | Claim {claimNo} is not payable. See the app for the full reason. | Đã có kết quả hồ sơ {claimNo} | `IN_APP`, `PUSH`, `EMAIL` |
| `CLM-NOTIF-007` | `PAID` | *Retired 17/08/2026 — duplicated `PAY-NOTIF-002` on the same trigger, in breach of rule M-9. The `PAID` event is notified once, by `PAY-NOTIF-002` (§5), which also carries the bank reference. The code is retained and never reused.* | | Đã có cập nhật hồ sơ {claimNo} *(push moves to `PAY-NOTIF-002`)* | — |
| `CLM-NOTIF-008` | Partial settlement | Hồ sơ {claimNo} được chi trả một phần: {settlementAmount} trên {amountClaimed} yêu cầu. Xem chi tiết từng khoản trong ứng dụng. | Claim {claimNo} partially settled: {settlementAmount} of {amountClaimed} claimed. | Đã có kết quả hồ sơ {claimNo} | `IN_APP`, `PUSH`, `EMAIL` |

| `CLM-NOTIF-009` | `CANCELLED` | Hồ sơ {claimNo} đã được hủy theo yêu cầu của Quý khách ngày {cancelledAt}. Quý khách có thể nộp hồ sơ mới bất kỳ lúc nào. | Claim {claimNo} was cancelled at your request on {cancelledAt}. You may submit a new claim at any time. | Đã hủy hồ sơ {claimNo} | `IN_APP`, `PUSH` |

**`CLM-NOTIF-009` exists because A3 §1 lets the customer withdraw a claim.** A state change the customer initiated, with no confirmation, leaves them unsure whether it took effect — and the second thing they need to know is that withdrawing has not cost them the right to claim again. Both sentences are load-bearing.

**Note the push column.** Every push says a result exists; none says what it is. `CLM-NOTIF-005` carries the amount in-app and by email, never on a lock screen — rule M-9 and the privacy note in §0.3.

**`CLM-NOTIF-008` exists separately from 005 on purpose.** A partial settlement is the most common cause of a complaint, and a customer who is told "approved" and then receives less than they claimed feels misled. The partial case is named as such, up front.

### 3.4 Information requests

| Code | Trigger | VI | EN | Channel |
|---|---|---|---|---|
| `DOC-NOTIF-001` | Required type missing | Hồ sơ {claimNo} còn thiếu: {missingDocuments}. Vui lòng tải lên để chúng tôi tiếp tục xử lý. | Claim {claimNo} is missing: {missingDocuments}. Please upload these so we can continue. | `IN_APP`, `PUSH`, `EMAIL` |
| `DOC-NOTIF-002` | Field unreadable | Chúng tôi chưa đọc được {fieldLabel} trên {documentType}. Vui lòng chụp lại chứng từ này. | We could not read the {fieldLabel} on your {documentType}. Please retake this document. | `IN_APP`, `PUSH` |
| `DOC-NOTIF-003` | Reconciliation failed | Thông tin trên {documentA} và {documentB} chưa khớp nhau. Chúng tôi sẽ kiểm tra và liên hệ nếu cần. | Details on your {documentA} and {documentB} do not match. We are checking and will contact you if needed. | `IN_APP` |
| `DOC-NOTIF-004` | Original required | Hồ sơ cần bản gốc của {documentType}. Vui lòng gửi về {address} trước ngày {deadline}. | We need the original {documentType}. Please send it to {address} by {deadline}. | `IN_APP`, `EMAIL` |
| `DOC-NOTIF-005` | Reminder, no response | Hồ sơ {claimNo} đang chờ bổ sung chứng từ từ ngày {requestedAt}. | Claim {claimNo} has been awaiting documents since {requestedAt}. | `IN_APP`, `PUSH` |
| **`DOC-NOTIF-006`** | **A term requires a specific document** — a class-E `REQUIRE_DOC` term (D1 `BR-TERM-11`) | Để tiếp tục xử lý hồ sơ {claimNo}, chúng tôi cần thêm: {requiredDocument}. Đây là chứng từ mà quy tắc bảo hiểm yêu cầu đối với trường hợp của Quý khách, nên hồ sơ sẽ tạm dừng cho đến khi nhận được. Vui lòng tải lên hoặc liên hệ {contactChannel} nếu Quý khách không có chứng từ này. | To continue with claim {claimNo} we need one further document: {requiredDocument}. The policy terms require it for a claim of this kind, so the claim is held until it arrives. Please upload it, or contact {contactChannel} if you do not have it. | `IN_APP`, `PUSH`, `EMAIL` |

> **`DOC-NOTIF-006` is new at v1.2 and it exists because `DOC-NOTIF-001` was being asked to do a job it does not do.** `-001` says *your claim is missing X, please upload it*, and every claim it is sent on is missing X for an ordinary reason: the customer forgot, or the provider did not print one. **A class-E `REQUIRE_DOC` term is not that.** It is the contract saying *this exclusion may apply and only a named document can settle it* — the claim is **held**, not merely incomplete, and the customer has a real chance of not being able to produce the document at all. **Three things are therefore said that `-001` does not say:** that the requirement comes from the policy terms rather than from our checklist, that the claim is stopped until it arrives, and **what to do if the document does not exist** — which is the sentence `-001` has no reason to carry and this one cannot do without. **It still obeys M-3**: it names no term, no clause and no class. *A term requires it* is a customer-readable fact; `TERM-HC-008` is not.

**`DOC-NOTIF-001` always names the documents.** `{missingDocuments}` renders the Vietnamese document names from A1 §6 — *Bảng kê chi phí khám chữa bệnh*, *Đơn thuốc* — never the type codes. `DOC-NOTIF-003` tells the customer we noticed and that they need do nothing, because a reconciliation mismatch is usually ours to resolve.

---

## 4. Decline reasons, as the customer reads them

The seven codes in A1 §5.2 are internal. This is what appears in the app and in the letter. Each is a sentence, not a label, and each names the item it applies to.

| Code | VI | EN |
|---|---|---|
| `LIMIT` | Hạn mức của quyền lợi {benefitName} trong năm hợp đồng **{periodKey}** không đủ cho khoản chi phí này. Hạn mức còn lại trong năm hợp đồng này: {remainingLimit}. | The remaining limit for {benefitName} in policy year **{periodKey}** is not sufficient for this cost. Remaining limit in that year: {remainingLimit} đ. |
| `PERIOD` | Ngày điều trị {serviceDate} nằm ngoài thời hạn bảo hiểm ({coverageFrom} – {coverageTo}). | Treatment date {serviceDate} falls outside the cover period ({coverageFrom} – {coverageTo}). |
| `NOBEN` | Chương trình bảo hiểm {planName} không bao gồm quyền lợi {benefitName}. | Plan {planName} does not include {benefitName}. |
| `WAIT` | Quyền lợi {benefitName} có thời gian chờ {waitingDays} ngày, tính từ {coverageFrom}. Ngày điều trị {serviceDate} chưa đủ thời gian chờ. | {benefitName} has a {waitingDays}-day waiting period from {coverageFrom}. Treatment on {serviceDate} falls within it. |
| `EXCL` | **{itemName}: {itemAmount}** — khoản này thuộc điểm loại trừ **{clauseRefList}** của quy tắc bảo hiểm. Nội dung của (các) điều khoản được trích dẫn nguyên văn trong thư thông báo. | **{itemName}: {itemAmount}** — excluded under **{clauseRefList}** of the policy terms. The text of the clause or clauses is quoted verbatim in the letter. |
| `NOTREL` | {itemName}: {itemAmount} — khoản này không thuộc phạm vi chi trả cho chẩn đoán {diagnosis} được điều trị trong lần khám này. Nếu Quý khách có thông tin y tế bổ sung, vui lòng gửi để chúng tôi xem xét lại. | {itemName}: {itemAmount} — not payable in respect of the diagnosis {diagnosis} treated during this visit. If you have further medical information, please send it and we will review. |
| `NOTCOV` | {itemName}: {itemAmount} — không thuộc danh mục chi phí y tế được chi trả. | {itemName}: {itemAmount} — not a covered medical item. |

> **`EXCL` was rebuilt at v1.2, and the defect it had is worth stating because it survived four versions unnoticed.** `NOTREL` and `NOTCOV` both bind `{itemName}` and `{itemAmount}`; **`EXCL` bound neither.** It had exactly one placeholder and that placeholder was the clause. So a customer declined on an exclusion was told which article of the policy applied and **not told which of their costs it applied to** — on a statement of eleven lines, an unanswerable letter. D2 `V-740` and its submission check both require a decline to name the item and the amount, and the check would have refused every uphold the term layer produced; D2 §5.29.3 was carving the check out to keep the path submittable. **The carve-out ends when this version is in force.**
>
> **And the clause is a list, not a value — `{exclusionRef}` becomes `{clauseRefList}`.** The term engine returns **every** `clauseRef` that fired on a line, in order (D1 `BR-TERM-2`, D2 `V-729`), because a line may be excluded by two clauses at once. **A letter that cites one of two clauses is a letter the customer can answer by disproving the wrong one**, and the second clause then arrives as a surprise in the reply. `{clauseRefList}` renders every reference that fired, in the engine's order, separated by `·`. **Renaming a placeholder is not renaming a code** — §0.2's permanence rule binds message codes, and `{exclusionRef}` is retired as a name because it described the wrong shape and the wrong thing: it was singular, and it said *exclusion* where the register's own word is *clause*.
>
> **The clause text lives in the letter, not in this line.** §7.2's *Basis* section quotes each clause's `textVi` verbatim, at the version the claim was evaluated against. The in-app sentence carries the references and points at the letter, because a decline reason rendered inside a cost-line grid cannot carry three paragraphs of contractual wording and stay readable.

> **`NOTREL` and `NOTCOV` are per cost line, and they are the two the AI drafts.** Per A5 §8.2 the assessor may edit the drafted text before it is sent, and both the original and the final version are retained. The pattern above is the drafting template, not a fixed string — but an edited message must still name the item and the amount, and the console enforces that.

**Waiting-period declines are the most disputed, so `WAIT` shows the arithmetic**: the period, the start date it counts from, and the treatment date. A customer who can see the calculation disputes it far less often than one who is told only that they were too early.

---

## 5. Payment — `PAY`

| Code | Trigger | VI | EN | Channel |
|---|---|---|---|---|
| `PAY-NOTIF-001` | Instruction sent | Đang chi trả cho hồ sơ {claimNo}. | Payment for claim {claimNo} is in progress. | `IN_APP` |
| `PAY-NOTIF-002` | Paid | Đã chi trả {settlementAmount} vào tài khoản {maskedAccount} ngày {paidAt}. Mã giao dịch: {bankReference}. | {settlementAmount} paid to {maskedAccount} on {paidAt}. Reference: {bankReference}. | `IN_APP`, `PUSH`, `EMAIL` — push text: *Đã có cập nhật hồ sơ {claimNo}* |
| `PAY-NOTIF-003` | Rejected or returned | Giao dịch chi trả chưa thành công. Chúng tôi đang kiểm tra thông tin tài khoản và sẽ liên hệ với Quý khách. | The payment did not go through. We are checking the account details and will contact you. | `IN_APP`, `PUSH`, `EMAIL` |

`{maskedAccount}` shows the last four digits only. **`PAY-NOTIF-003` does not ask the customer to fix anything** — a returned payment is investigated by an operator first, because the cause is as often our data as theirs.

---

## 6. System errors — `SYS`

Rule M-6 governs this section entirely: **a system failure is never phrased as a decision about the claim.**

| Code | Internal cause | VI | EN | Channel |
|---|---|---|---|---|
| `SYS-ERR-001` | Extraction failed (`EXTRACTION_ERROR`) | Chúng tôi đang xử lý chứng từ của Quý khách và sẽ thông báo ngay khi có kết quả. | We are still processing your documents and will notify you as soon as there is a result. | `IN_APP` |
| `SYS-ERR-002` | Policy Data Store unavailable | Hệ thống đang bận. Vui lòng thử lại sau ít phút. | The system is busy. Please try again in a few minutes. | `IN_APP` |
| `SYS-ERR-003` | Upload failed | Tải tệp không thành công. Vui lòng kiểm tra kết nối và thử lại. | Upload failed. Please check your connection and try again. | `IN_APP` |
| `SYS-ERR-004` | Unexpected error | Đã có lỗi xảy ra. Chúng tôi đã ghi nhận và đang xử lý. Mã tham chiếu: {correlationId}. | Something went wrong. We have logged it and are looking into it. Reference: {correlationId}. | `IN_APP` |

**`SYS-ERR-001` deliberately looks like a progress update**, because from the customer's side it is one — an extraction failure is recoverable and invisible to them. **`SYS-ERR-004` is the only customer message that carries an internal identifier**, and it does so because support cannot help without it. It is a correlation id, not a state name, so rule M-3 holds.

---

## 7. Correspondence templates

Three documents are generated and emailed. All are Vietnamese, all attach a PDF, and all are produced by F61.

### 7.1 `CLM-LETTER-001` — Settlement notice (Thông báo chi trả bồi thường)

| Section | Content |
|---|---|
| Header | Insurer name, logo, correspondence date |
| Reference | Số hồ sơ `{claimNo}` · Số hợp đồng `{policyNo}` · Người được bảo hiểm `{memberName}` |
| Opening | Confirmation that the claim has been assessed and the outcome |
| **Benefit-by-benefit table** | Quyền lợi · Chi phí yêu cầu · Được chấp nhận · Đồng chi trả · Chi trả — **one row per benefit paid**, plus a row per declined item with its reason |
| Deductions | Co-payment, and the deduction ratio **with the condition that triggered it** where one applied — e.g. treatment at a provider outside the listed network. It is a contract term, so there is no assessor's reason to state |
| Total | `{settlementAmount}`, in figures and in words |
| Remaining limits | Hạn mức còn lại per benefit group after this claim |
| Payment | Account (masked), expected date |
| Closing | How to query the outcome, and the deadline for doing so |

**The benefit-by-benefit table is the point of the letter.** A total alone generates a phone call whenever it differs from what was claimed; the itemisation answers the question before it is asked. Declined lines appear in the same table as paid ones, with the §4 wording — separating them into an appendix makes the letter look as though it is hiding something.

### 7.2 `CLM-LETTER-002` — Decline notice (Thông báo từ chối bồi thường)

Same header and reference block, then:

| Section | Content |
|---|---|
| Outcome | A single clear sentence that the claim is not payable |
| **Reason** | The §4 wording for the applicable code, with every placeholder filled |
| **Basis** | **The clause or clauses of the policy terms relied on — one or many, each with its article and clause number and its `textVi` quoted verbatim, at the version the claim was evaluated against.** Where more than one clause fired on a line, every one of them appears, in the order the engine returned them. **Never a summary, never a paraphrase, and never only the first** |
| What is not affected | An explicit statement that cover continues and other benefits are unaffected |
| Right of review | How to ask for the decision to be reconsidered, to whom, and by when |

**Three things this letter must never do:** cite a reason not in the catalogue, omit the review route, and **render an unissued clause reference**. A decline without a stated route to challenge it is the single most reliable way to turn a decision into a complaint.

> **The third one is new at v1.2 and it is currently binding on every exclusion decline this platform could produce.** A clause reference beginning `TBC-` is a placeholder standing in for a citation into filed wording that does not exist yet. **All ten terms on the pilot product's register carry one** (A5 §7.2), and D1 v0.5's **`BR-TERM-10`** forbids a term whose `clauseRef` is provisional from declining at all — such a term **raises a checkpoint instead**, whatever its class and whatever its effect. So the engine will not hand this letter a `TBC-` reference; **this rule is the second lock, on the letter rather than on the engine**, and it exists because the two failures are different sizes. A term that declines when it should not is a wrong decision an assessor can reverse. **A letter that quotes `TBC-04` at a customer is a document with a placeholder in it, on the insurer's letterhead, about a refusal to pay** — and it cannot be reversed, because it has already been read.
>
> **This is what A2-04 has become**, and §10 restates it accordingly. It began as a tidiness question — *do our exclusion references cite real article numbers?* — and it is now **the item standing between the term layer and any exclusion decline reaching a customer at all.**

### 7.3 `POL-LETTER-001` — Certificate email (Giấy chứng nhận bảo hiểm)

Sent on issuance, alongside `POL-NOTIF-002`.

| Section | Content |
|---|---|
| Header | Insurer name, logo, issue date |
| Reference | Số hợp đồng `{policyNo}` · Bên mua bảo hiểm `{policyholderName}` |
| Cover | Plan `{planName}`, period `{effectiveFrom}` – `{effectiveTo}`, premium `{premium}` |
| Insured members | One row per member: name, date of birth, relationship, card number, coverage period |
| **Benefit summary** | Annual sum insured per benefit group, co-payment, and **waiting periods by condition class** |
| Attachment | The certificate PDF |
| Closing | How to claim, and where to find the full policy terms |

**The waiting-period table belongs in this letter, not only in the policy terms.** It is the single most common cause of a surprised, disputed claim, and the cheapest moment to set the expectation is the day the customer buys — not the day they are declined.

---

## 8. Console messages — `CONSOLE`

Never shown to a customer. Written for a working assessor, so they are terse and name the internal identifiers rule M-3 hides from customers.

| Code | Trigger | Text (EN) |
|---|---|---|
| `CLM-CONSOLE-001` | Extraction confidence below threshold | Confidence {confidence} below threshold {threshold}. Review: {fieldList} |
| `CLM-CONSOLE-002` | Reconciliation warning | {ruleId}: {message} — {documentId}, field {fieldName} |
| `CLM-CONSOLE-003` | Limit would be exceeded | {benefitCode} {benefitName}, policy year {periodKey}: remaining {remaining}, claimed {claimed} |
| `CLM-CONSOLE-004` | Waiting period not served | {benefitCode}: {conditionClass} requires {waitingDays} days from {coverageFrom}; service date {serviceDate} |
| `CLM-CONSOLE-005` | Approval blocked, same user assessed | You assessed this claim and cannot approve it. Reassign to another approver. |
| `CLM-CONSOLE-006` | Account not verified | Payout account name not verified. Payment cannot be instructed. |
| `CLM-CONSOLE-007` | Snapshot unavailable | No benefit schedule effective on {serviceDate} for plan {planCode}. Escalate — do not proceed. |

| `CLM-CONSOLE-008` | Second-administrator confirmation required | This change requires confirmation by a second administrator. Requested by {requestedBy} at {requestedAt}. |
| `CLM-CONSOLE-009` | Second-administrator confirmation refused | You requested this change and cannot confirm it. A different administrator must confirm. |
| `CLM-CONSOLE-010` | Payout account changed after approval | The payout account changed after approval. Payment is held until an approver re-releases it. |
| `CLM-CONSOLE-013` | Clinical recommendation raised | {lineCount} cost line(s) have a clinical recommendation. Review each and confirm or reject before submitting. |
| `CLM-CONSOLE-014` | Rejecting a recommendation | State why you disagree. This is recorded against the recommendation and is how the knowledge base improves. |
| `CLM-CONSOLE-015` | Clinical assessment unavailable | The clinical assessment did not complete for this claim. It has **not** been assessed as clean — review the cost lines manually. |
| `CLM-CONSOLE-011` | Conflict of interest, ID match | You are named on this claim as the insured, claimant or beneficiary and cannot work on it. Return it to the queue. |
| `CLM-CONSOLE-012` | Conflict of interest, declaration | Declare any relationship to the insured on this claim. Declaring a relationship returns the claim to the queue and is recorded. |

`CLM-CONSOLE-008` and `-009` serve A3 §4.7 (auto-approval threshold) and §4.9 (import commit); `-010` serves A3 §4.5. Each of A3's conditional rules that a user can hit now has a message that says why they were stopped — a rule enforced with a generic "permission denied" gets worked around rather than understood.

`CLM-CONSOLE-001` and `-003` **name the specific field or benefit**, never "review required" — A4 §7's rule that a below-threshold result must route to an assessor **with the specific field named**. `CLM-CONSOLE-007` instructs the assessor not to proceed, because the alternative is a claim assessed against the wrong schedule.

> **`CLM-CONSOLE-003` gained `{periodKey}` at v1.2, and `LIMIT` gained it in §4 for the same reason.** The ledger counter is keyed on the period as well as the member and the benefit (D2 `V-723`), **`V-730` renders it once per period and forbids summing**, and D1 `L-3` posts a claim whose lines span a policy-year boundary **to two periods, line by line**. A message that says *remaining 400.000* on such a claim is answering a question nobody asked: the member has one remaining figure in the closing year and another in the opening one, and **which one the assessor is being shown was, until this version, not stated anywhere in the message.** The customer-facing `LIMIT` sentence had the same hole and it mattered more there, because the customer has no second screen to check it against.

---

### 8.1 Staff notifications — `STAFF`

**New at v1.1, and it closes register row B-33. Extended at v1.2 by three.** Reference System A carries fifteen staff e-mail templates and seven in-system notices — twenty-two touch points where work changes hands or a claim stops moving. **We carry eleven**, and the eleven are chosen on one test: *would a person who is not looking at this claim need to know?*

**Why this exists at all, stated once so it is not re-argued.** The console measures a claim's age and sorts three overdue buckets from it, **and has no mechanism to tell anyone before a claim falls into one.** A metric with no alerting channel is a number to read in a report after the work has already gone wrong. Three specific failures follow from the gap and all three are ordinary: a claim assigned to somebody on leave, an extraction failure whose `CLM-CONSOLE-015` reaches nobody who has not opened that claim, and a returned claim that waits until somebody's filter happens to catch it.

> **The three added at v1.2 are the term layer's, and they are added because they pass the test, not because the term layer is new.** A **checkpoint raised on a claim you are holding** stops that claim moving and nothing told you. A **term correction that re-evaluated your claim and routed it back to the checkpoint queue** (D2 §5.28.5) is a claim that already had an answer and whose answer moved — **and it is delivered by a queue whose scope is not the assessor's**, so without a notification the person who owns the work learns nothing at all. And a **term version awaiting approval** is a four-eyes control (A3 §4.25) whose second pair of eyes has no idea it has been asked for; a register that stalls because nobody was told is a control that looks like an outage.

| Code | Trigger (D2 §5.23.1) | Recipient | VI | EN | Channel |
|---|---|---|---|---|---|
| `STAFF-NOTIF-001` | A claim is assigned to a user (§5.3.3) | The assignee | Bạn được phân giao hồ sơ {claimNo}. Hạn xử lý: {dueAt}. | Claim {claimNo} has been assigned to you. Due {dueAt}. | `STAFF_INBOX` |
| `STAFF-NOTIF-002` | A claim is reassigned | **The former assessor, the new assessor and the acting approver** | Hồ sơ {claimNo} được chuyển từ {fromUser} sang {toUser} bởi {actorName}. | Claim {claimNo} was reassigned from {fromUser} to {toUser} by {actorName}. | `STAFF_INBOX` |
| `STAFF-NOTIF-003` | An approver returns a claim (§5.11.3) | The assessor who submitted it | Hồ sơ {claimNo} được trả về. Lý do: {returnReason}. | Claim {claimNo} has been returned. Reason: {returnReason}. | `STAFF_INBOX`, `EMAIL` |
| `STAFF-NOTIF-004` | The customer supplies the documents requested (§5.5.4) | The assessor holding the claim | Khách hàng đã gửi chứng từ bổ sung cho hồ sơ {claimNo}. | The customer has supplied documents for claim {claimNo}. | `STAFF_INBOX` |
| `STAFF-NOTIF-005` | Extraction failed or stalled (§5.6.8) | The assessor holding the claim | Trích xuất tài liệu hồ sơ {claimNo} chưa hoàn tất. Hồ sơ đang chờ xử lý sự cố. | Document extraction did not complete for claim {claimNo}. It is queued for operational handling. | `STAFF_INBOX` |
| `STAFF-NOTIF-006` | Extraction completed | The assessor holding the claim | Trích xuất tài liệu hồ sơ {claimNo} đã hoàn tất. | Document extraction has completed for claim {claimNo}. | `STAFF_INBOX` |
| `STAFF-NOTIF-007` | The claim crosses into the overdue bucket (§5.3.2) | The assessor holding the claim | Hồ sơ {claimNo} đã ở nhóm {ageingBucket} — {ageDays} ngày xử lý. | Claim {claimNo} is now in the {ageingBucket} bucket — {ageDays} days in handling. | `STAFF_INBOX`, `EMAIL` |
| `STAFF-NOTIF-008` | A settlement plan is submitted (§5.10.3) | The approver queue | Có phương án bồi thường chờ phê duyệt: hồ sơ {claimNo}, cán bộ trình {submittedBy}. | A settlement plan is awaiting approval: claim {claimNo}, submitted by {submittedBy}. | `STAFF_INBOX` |
| **`STAFF-NOTIF-009`** | **A term checkpoint is raised on a claim the user is holding** (D2 §5.29) | The assessor holding the claim | Hồ sơ {claimNo} có {checkpointCount} điểm kiểm điều khoản đang mở. Hồ sơ dừng cho đến khi được xử lý. | Claim {claimNo} has {checkpointCount} open term checkpoint(s). It will not move until they are answered. | `STAFF_INBOX` |
| **`STAFF-NOTIF-010`** | **A term correction re-evaluates the claim and routes it to the checkpoint queue** (D2 §5.28.5, `V-747`) | The assessor holding the claim | Một điều khoản áp dụng cho hồ sơ {claimNo} đã được sửa lỗi. Hồ sơ đã được đánh giá lại và chuyển vào hàng đợi điểm kiểm. Kết quả trước và sau đều được lưu. | A term applied to claim {claimNo} has been corrected. The claim has been re-evaluated and routed to the checkpoint queue. Both the previous and the new result are recorded. | `STAFF_INBOX`, `EMAIL` |
| **`STAFF-NOTIF-011`** | **A term version is submitted for approval** (D2 §5.28.4, `V-736`) | **Every holder of `TERM_APPROVE` except the author of that version** | Có điều khoản chờ phê duyệt: {clauseRef}, bản {versionNo}, do {authoredBy} soạn ngày {submittedAt}. | A term is awaiting approval: {clauseRef} version {versionNo}, authored by {authoredBy} on {submittedAt}. | `STAFF_INBOX` |

**The rules these eight obey, and they are not the customer rules.**

| # | Rule | Why |
|---|---|---|
| **S-1** | **A notification names the claim and the event, and nothing else.** It is not a summary and it is not a substitute for opening the claim | A notification that tries to carry the work is a second, unversioned view of the claim that will drift from the first |
| **S-2** | **A notification is never a claim outcome.** It reports that something happened; whether the claim moved is Part VII's answer, not this message's | D3 G-3, and the same reasoning as M-6. A staff member who treats a notification as the state of the claim will act on a stale one |
| **S-3** | **A `STAFF_INBOX` notification carries no clinical detail** — no diagnosis, no ICD-10 code, no item name, no amount | The bell is visible on every screen and over a shoulder. This is the staff-side equivalent of the lock-screen constraint in §0.3, and it exists for the same reason |
| **S-4** | **`EMAIL` is used on exactly three of the eleven** — the returned claim, the overdue claim and **the corrected term** — because those are the three that matter when the person is *not at the console at all*. **`STAFF-NOTIF-010` earns the third slot and `-009` and `-011` do not**: a checkpoint and a pending approval are work waiting on the bell, and both are seen next time the person logs in; **a correction has already changed a claim that person had an answer on**, and the longer they do not know, the longer they are working from a result the register has withdrawn | Everything on the bell is seen next time they log in. E-mailing all eleven trains people to filter the sender, and then the three that mattered are filtered with them |
| **S-5** | **Addressed to a user, read by that user.** No role reads another user's inbox, and there is no digest to a supervisor | A3 §5: a cross-user report of *what is person X doing* is an audit function, and no audit role exists in MVP 1 |
| **S-6** | **Every one of these writes a row in the claim's correspondence history** (D2 §5.22, V-718), delivered or failed | The history answers *what did we send about this claim*. A staff message excluded from it would make that answer quietly incomplete |
| **S-7** | **Vietnamese and English both carry, and the parameters are named** (M-8) | The console is bilingual and these are read at speed |

> **Two things §8.1 deliberately does not do.** There is **no browser push and no digest e-mail**, which §9 records; and there is **no notification for a state change nobody is waiting on** — a claim entering `DOC_PROCESSING` notifies the customer and nobody else, because no member of staff is blocked by it. **Twenty-two touch points became eleven by applying that second test**, not by rationing.

> **S-3 does real work on the three added at v1.2, and it is worth showing rather than asserting.** A checkpoint carries the clause, the facts the selector matched on and a clinical judgement (A3 §4.26). **`STAFF-NOTIF-009` carries a count and a claim number and nothing else** — not the clause reference, not the clause text, and certainly not the class. The reason is that the clause text on this product's register includes wordings about substance-related conditions and about pregnancy: **a bell notification naming one of those clauses states a fact about a named person's health on a screen that is visible from the next desk.** `-010` and `-011` carry a clause reference because a term version is master data with no member attached to it — **and `-010` names the claim without naming the clause**, which is the line S-3 draws.

> **A2-06 is closed, and it closed from the other end.** Until 24/08 this section was written and **D2 Part VI forbade the channel it needed**: §6.1.2 had no staff recipient group and §6.1.9 excluded staff e-mail outright. That contradiction was recorded here rather than left for a reviewer to find, as D2 open item **`D2-127`** and as **A2-06**. **D2 §6.1.9 closed `D2-127` on 24/08 by yielding** — §6.1.2 now carries the `STAFF_INBOX` channel, **§6.1.2b** specifies the engine's half of it (how a recipient is resolved, how the inbox is read and marked read, what happens when the addressee has left), and §6.1.9 now excludes what **§9 of this catalogue** excludes: browser push and the digest. **No engine work was implied and none was done.** The note is kept and rewritten rather than deleted, because *the two documents contradicted each other for six days and here is how it ended* is worth more to the next reader than a clean page.

### 8.2 Manual re-send — and the sharp line under it

**Register rows B-34 and S-44, applied reduced.** An officer may **re-send a message this claim has already generated**, to one or more addresses they key. That is the whole of it.

| Code | Trigger | Text (EN) |
|---|---|---|
| `CLM-CONSOLE-016` | Confirming a re-send | Re-send {messageCode} — *{messageName}* — as it was rendered on {sentAt}? It will go to: {addressList}. The content is not re-generated. |
| `CLM-CONSOLE-017` | Re-send refused | This message cannot be re-sent. {reason} — a staff-facing notification is never re-sent to an external address, and a message whose body was not retained cannot be re-sent as it was. |
| `CLM-CONSOLE-018` | Re-send recorded | Sent to {addressList} at {sentAt}. Recorded on the claim's correspondence history. |

**The four rules, and they are the mechanism.**

| # | Rule |
|---|---|
| **R-1** | **What is sent is a message this claim already generated.** The body is exactly what was rendered when it was first sent — **nothing is re-rendered and no placeholder is re-resolved.** A template versioned since would otherwise produce a message the customer never received, which is the one thing the correspondence history exists to prevent |
| **R-2** | **What the officer supplies is the address, and only the address** |
| **R-3** | **Every re-send is a row** in the correspondence history, carrying the addresses, the actor and the timestamp (D2 V-718, V-719) |
| **R-4** | **The claim's state does not change and no rule reads a re-send.** It is a delivery act, not a claim act |

> **This is not free-text composition, and it is not template selection either. Both distinctions are load-bearing and the second is the one that gets lost.**
>
> **Free composition** — an officer typing a letter to a customer — remains excluded, unchanged, at §9. That has never been in question.
>
> **Template *selection*** is the one worth arguing, because it looks harmless. Reference System A offers a menu of about ten letter types filtered by the claim's current state, and lets an officer pick one and send it. **We refuse the menu**, and the reason is not effort. **This catalogue owns which message fires on which event.** A console control that lets a person choose a letter makes that ownership unenforceable overnight: A2 would describe the messages the platform sends, and the console would send others. The catalogue would stop being true, and a catalogue that is not true is worse than none, because people rely on it.
>
> **What the register found is that the reference's mechanism is not composition either** — it is re-delivery of an existing template to a different address. **That is the half carrying the value and none of the cost**, and it is what §8.2 grants. Earlier analysis (**G-B-30**) priced this against the wrong assumption and concluded it was expensive; it is not.
>
> **The state-conditioned list of permitted letter types is therefore not adopted.** That condition is a property of a template — *when does this letter apply* — and if it is ever wanted it belongs **here**, beside each message, as a machine-checkable applicability rule. **Not on a screen.** Open item **A2-07**, and register row **B-36** is its other half.

### 8.3 The term layer — `CLM-CONSOLE-019` … `-027`

**New at v1.2, and this is `D2-88`'s allocation pass for the term layer.** D2 v0.6 raised four items against this catalogue — `D2-153`, `D2-154`, `D2-155` and `D2-156` — **proposed no numbers for any of them, and de-numbered the five its v0.5 had proposed.** That was the right thing for it to do and it is the reason this section exists: **two documents numbering the same space independently is how `-021`, `-023` and `-025` came to be proposed twice inside D2 alone.** A2 owns the numbering; the allocation is made here, once, against §8 and §8.2 as issued.

**Written for a working term author and a working assessor**, so they name `clauseRef`, `ruleSetRef` and the version number that M-3 hides from customers — §0.4's staff rule.

| Code | Trigger | Text (EN) |
|---|---|---|
| **`CLM-CONSOLE-019`** | A term version fails validation on save (`V-734`, `V-742`, D2 §5.28.3) | This term cannot be saved. {validationReason} — {detail}. Correct it and save again; nothing has been written. |
| **`CLM-CONSOLE-020`** | Approval refused — the acting user authored this version (`V-736`, A3 §4.25) | You authored version {versionNo} of {clauseRef} and cannot approve it. A different holder of `TERM_APPROVE` must approve this version. |
| **`CLM-CONSOLE-021`** | Back-dating refused — the effective date precedes an open claim (`V-737`, A3 §4.27) | Effective date {effectiveFrom} is earlier than the oldest open claim bound to rule set {ruleSetRef}: {blockingClaimCount} claim(s), the earliest registered {earliestRegisteredAt}. Back-dating past an open claim requires a recorded override by a holder of `TERM_APPROVE`. |
| **`CLM-CONSOLE-022`** | A correction is declared and the impact analysis has completed (`V-738`, `V-739`, A3 §4.28) | **Correction to {clauseRef}, rule set {ruleSetRef}.** {boundCount} in-flight claim(s) are bound to this rule set. {changedCount} would produce a different result and each will be routed to a person — **none is re-decided automatically**. {excludedCount} claim(s) are excluded because they are `PAID` or `CLOSED` and cannot be touched. State your reason, then confirm. |
| **`CLM-CONSOLE-023`** | A version is returned to draft (A3 §4.25) | State why version {versionNo} of {clauseRef} is being returned to draft. The reason is recorded on the version and is shown to its author. |
| ~~`CLM-CONSOLE-024`~~ | *Reserved, not allocated — see the note below* | |
| **`CLM-CONSOLE-025`** | A checkpoint outcome is being recorded — uphold or clear (D2 §5.29.5) | State why you are recording **{checkpointOutcome}** on the checkpoint raised by {clauseRef}. **An uphold declines {scopeLabel} and the customer will be told;** a clear applies nothing and the customer sees nothing. Both are recorded against this claim and against the term version. |
| **`CLM-CONSOLE-026`** | A checkpoint outcome is refused (D2 §5.29.5) | This checkpoint cannot be answered. {reason} — the term version it was raised against cannot be resolved, or the cost line or insured event it attaches to no longer exists. Escalate rather than clearing it. |
| **`CLM-CONSOLE-027`** | Settlement-plan submission refused — an open term checkpoint (`V-732`, D1 `BR-TERM-13`) | This settlement plan cannot be submitted. {checkpointCount} term checkpoint(s) are still open on this claim: {checkpointList}. Answer each one — uphold or clear, with a reason — and submit again. |

**`{validationReason}` renders one of five, and the list is closed.** *(1)* the clause reference is already in use in this rule set; *(2)* the term has no `selector`; *(3)* a `predicate` is present on a class `J` or class `E` term, which may never carry one; *(4)* a decline effect is present on a class `J` or class `E` term, which may never decline (`V-742`, D1 `BR-TERM-7`); *(5)* the scope code is not in this rule set's benefit tree. **`{detail}` names the offending value** — the clause reference, the field, the scope code — never a bare restatement of the reason.

**`{checkpointList}` names each open checkpoint and the clause it quotes, and never a count on its own.** `D2-156` asked for exactly that, and the reason is worth keeping: *"three checkpoints are open"* tells an assessor they have work and not what the work is, so they open the claim, find the queue, and read what the message could have told them. **The count is in the sentence because the list may be long; the list is in the sentence because the count is useless alone.**

**The four rules this family obeys, and two of them are new here.**

| # | Rule |
|---|---|
| **T-1** | **A term message names the clause, never the term's internal identifier.** `{clauseRef}` is what a term author, an assessor and a policy document all call the same thing. `TERM-HC-003` is a row key and belongs in the audit log |
| **T-2** | **A message that refuses an action says what would make it succeed.** `-019` says correct and save again, `-020` names who may approve instead, `-021` names the override and who holds it, `-027` says answer each checkpoint. This is M-2 applied to staff, and the term layer is where it matters most: **every one of these refusals is a control, and a control enforced with a bare refusal gets routed around within a week** |
| **T-3** | **`-021` and `-022` render counts and dates, never claim numbers** — D2 `V-759`, and **A3 v1.5 §4.29 settles it: counts and dates only.** These two screens sit at data scope `MASTER`, which A3 defines as carrying no personal data, and **a claim number is not master data** — it is the key every other console screen opens a clinical record with. `{blockingClaimCount}`, `{earliestRegisteredAt}`, `{boundCount}`, `{changedCount}` and `{excludedCount}` are the whole of what these messages may bind |
| **T-4** | **`-022` is shown before the confirmation, not after it.** An impact count that arrives after the act is a receipt. **A3 §4.28 makes the count a precondition of the grant**, and this message is where that precondition is met or is not |

> **`CLM-CONSOLE-024` is reserved and deliberately not allocated.** D2 v0.6's §8 note names **`CLM-CONSOLE-024`** for the template-validation failure message that `V-008` needs — while the `V-008` table cell itself still reads *(new A2 item needed — `D2-88`)*. So the number is cited in D2's live prose and defined nowhere. **Allocating it here to a checkpoint outcome would give one code two meanings across two documents**, which is precisely what §11 forbids and precisely what `D2-88` was raised to stop. **Skipping a number costs nothing; a collision costs a reader.** The number is held for the message D2 has already called by it, and **the rest of `D2-88` — §5.18's sixteen console messages and §6.5.1's five — is not allocated in this pass** and is recorded at §10 as **A2-10**. This section allocates the term layer and says so; it does not quietly claim to have done the whole sweep.

---

## 9. What is deliberately not in this catalogue

| Not included | Why |
|---|---|
| SMS text | No message uses SMS in MVP 1. The channel exists in the engine; adding text later is configuration |
| Marketing or re-engagement messages | Out of scope, and mixing them with transactional messages damages both |
| Renewal and expiry reminders | Renewals are out of scope — Framework §7.2 |
| Assessor-to-customer free text | MVP 1 has no message thread. Information requests are structured (§3.4) so they can be measured and audited. **Unchanged by v1.1** — §8.2 grants re-delivery of an existing message, never composition of a new one |
| **Staff-composed correspondence, and the state-filtered menu of letter types** | §8.2's note. The menu is not free text and is still refused: it would move ownership of *which message fires when* out of this catalogue and onto a screen |
| **Browser push and digest e-mail to staff** | §8.1 delivers to the console bell, plus e-mail on the two events that matter when nobody is at the console. **A digest is the mechanism that trains people to stop reading**, and it would arrive alongside the two alerts that were worth reading |
| **Notifications to the eleven further touch points Reference System A covers** | §8.1's test — *would a person who is not looking need to know?* — excludes them. **Fourteen at v1.1, eleven from v1.2**, because §8.1 grew by three. Recorded so the count difference reads as a decision |
| English customer-facing text | The app is Vietnamese-only for customers. The EN column is for the team and for the console |

---

## 10. Open items

| # | Item | Owner |
|---|---|---|
| A2-01 | Insurer legal entity name, logo and correspondence address for §7 templates | Business |
| ~~A2-02~~ | ~~The postal address for physical originals (`DOC-NOTIF-004`) and the deadline policy~~ — **Closed 18/08/2026 by D1 B-06: physical originals are not required in the pilot.** `DOC-NOTIF-004` stays in the catalogue, unused, so enabling it later is content rather than a new message | Closed |
| A2-03 | Review route and deadline wording in `CLM-LETTER-002` — should be checked against the policy terms | Business |
| **A2-04** | **The clause references the term register holds are placeholders, and until they are replaced no exclusion decline can leave the building. Restated at v1.2 — it is no longer the question it was.** It began as *do our exclusion references cite article numbers of the actual policy wording?* **The answer is now known and it is no.** All ten terms on the pilot product's register carry a `TBC-nn` reference (A5 §7.2); D1 v0.5's **`BR-TERM-10`** forbids a term with a provisional reference from declining, so it **raises a checkpoint instead**; and §7.2's third prohibition stops the letter rendering one even if the engine ever handed it over. **The three locks agree, which is right, and none of them is the fix.** **What is needed is the filed policy wording, with the article and clause numbers each of the ten terms actually relies on** — supplied once, entered against the register, and the `TBC-` references replaced. Until then the term layer's decline path is specified, built and unusable, and **every exclusion the product carries settles as a checkpoint on a person's desk.** `C-IMO-12` and `C-IMO-13` are the vendor half of the same question — where the wording document lives, and whether the rule set is a set of codes or prose | **Business — this is the one to move** |
| A2-05 | Vietnamese wording review by a native insurance writer before UAT | Business |
| ~~**A2-06**~~ | ~~**D2 Part VI must be edited to match §8.1.**~~ — **Closed 25/08/2026 by D2 §6.1.9, which closed `D2-127` on 24/08.** §6.1.2 carries the `STAFF_INBOX` channel and cites §8.1 as its owner; **§6.1.2b** is new and specifies the engine's half — recipient resolution, the read state, the departed addressee; and §6.1.9 now excludes browser push and the digest, which is exactly what §9 of this catalogue excludes. **No engine work was implied and none was done**, which is what the item said at the time. D2 closed it *by yielding*, on the principle that the annex owning a fact settles it — see §8.1's note | Closed |
| **A2-07** | **Should a template carry a machine-checkable applicability condition — *when does this letter apply* — beside it?** Register row **B-36**, and the half of it a screen cannot own. Not needed while every message fires from an event; needed the moment anyone asks for the state-filtered menu §8.2 refuses. **Decide it here or not at all** | BA + Business |
| **A2-08** | **The insurer's staff e-mail domain and the sender identity for `STAFF-NOTIF-003`, `-007` and — from v1.2 — `-010`.** §7's templates need a customer-facing sender (A2-01); these need an internal one, and they are not necessarily the same | Business |
| **A2-09** | **`CLM-NOTIF-005` fires on `APPROVED` and `CLM-NOTIF-008` on a partial settlement, and nothing in this catalogue says where the boundary between them lies. Under the fourteen-step chain, almost every claim is settled for less than was claimed — so `-005` is close to a dead message and nobody has decided that it should be.** §3.3 gives `-005` the `APPROVED` state and `-008` the trigger *"partial settlement"*, written when a partial settlement meant a decline on a line. **Step 10 of the chain applies a co-payment**, which the pilot product carries on ordinary outpatient benefits, so a claim with nothing declined, nothing capped and no term applied **still settles below the amount claimed**. On the strictest reading of *"less than claimed"*, `-008` fires on nearly everything and `-005` on nearly nothing. **D2 §7.1 has picked a rule — *`CLM-NOTIF-005`, or `CLM-NOTIF-008` where the settlement is less than claimed* — and this catalogue has not, which is the wrong way round**: §0 says the message decision is taken here, once, and §11 makes changing a customer-facing wording a change request. **The question, and it is a business question rather than a specification one: at what point does a customer need to be told *partially settled* rather than *approved*?** Three candidate rules, and they give materially different letters: **(a)** any shortfall at all, which is D2's current reading and makes `-005` near-dead; **(b)** a shortfall caused by anything a person decided — a declined line, a term upheld, a cap that bound — with a contractual co-payment alone still counted as *approved in full*, on the argument that a co-payment is a term the customer bought and `CLM-LETTER-001` itemises it anyway; **(c)** a stated materiality threshold. **Not decided here.** `-008`'s own note in §3.3 says a customer told *approved* who receives less than they claimed feels misled, and that reasoning points at (a); the counter-argument is that a customer told *partially settled* on a claim where nothing was refused will telephone to ask what was refused. **Both calls are real and the choice is the business's** | **Business** — and it changes the wording a customer reads on most claims |
| **A2-10** | **`D2-88`'s remaining allocation — the console messages Parts V and VI still need above `-015`, outside the term layer.** §8.3 makes the term layer's allocation and **explicitly does not make the rest.** D2 §5.18 lists sixteen console messages it needs and §6.5.1 five more; **all of them now read *(new A2 item needed — `D2-nn`)* with no number**, which is correct and is also a document with twenty-one unwritten sentences in it. **Two consequences are live rather than theoretical:** D2's own prose still names **`CLM-CONSOLE-024`** for a template-validation failure whose table cell carries no number, which is why §8.3 reserved it; and **`CLM-CONSOLE-028`** survives in a v0.5 residue that v0.6's de-numbering did not reach. **What is asked is one pass, like §8.3, across both parts together** — not twenty-one items decided one screen at a time, which is the failure §0 was written to prevent | **BA** — before D2 v0.7 |

**Three of these are not equal, and the ordering has changed at v1.2.** **A2-04 is now the blocking one** — until filed wording exists, the term layer's decline path cannot produce a letter at all, and every exclusion on the product becomes a person's judgement instead. **A2-09 is the one that changes what most customers read** and is the only item on this list a specification cannot settle: it is a question about what *approved* should mean to a person receiving money. **A2-10 is the one that will be forgotten**, because twenty-one absent messages produce no error and each is individually trivial.

**A2-05 is not a formality.** The Vietnamese here is written to be correct and plain, but claim correspondence is a register of its own and a customer notices when it is slightly off. One pass by someone who writes these for a living, before UAT, is cheap.

---

## 11. Change control

A message changes here first. Codes are permanent and never reused. Adding a message is additive; changing the wording of a decline reason or a letter section is a change request, because both may be quoted back to the insurer.

**From v1.2, the numbering is stated as a rule rather than left as a practice, because it has already failed once.** **This catalogue allocates every `CLM-CONSOLE` code, in one pass per family, and no other document proposes one.** D2 v0.5 proposed five and had proposed three of them twice inside itself; v0.6 withdrew every number it held and asked A2 for the allocation, which is `D2-88`. **A number that is cited in one document and defined in another is a code with two owners**, and the failure mode is not a clash — it is quieter than that. It is a message that gets built twice, once per citation, with two different sentences. **§8.3 reserves `-024` rather than taking it for exactly this reason**, and the general rule is: **where a number is cited anywhere in the set and defined nowhere, it is reserved, not reused.**

**From v1.1, one more thing changes here first: the recipient group.** Moving a message between the customer group and the staff group is **not** additive, because the two groups obey different rules — M-3 hides internal identifiers from one and expects them in the other, and §8.1's S-3 forbids clinical detail that §8's console messages carry freely. **A message re-pointed at a new group without being re-read against that group's rules is the most likely way this catalogue produces a privacy failure**, and it is the only edit here that can do so silently.

*End of A2 v1.2.*
