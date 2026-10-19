# Open bugs — My Insurance Portal + BackOffice (19/10/2026)

> English translation for reviewers. Vietnamese original: [open-bugs-19-10.md](open-bugs-19-10.md).

Handover to the bug-fixing session. The bugs below were found while recording the hackathon demo video on 19/10 on **bnk-dev** (ClaimMVP rev 951 = My Insurance BackOffice, MyInsurance rev 333 = My Insurance Portal). Fix status per batch: see section **D** at the end of this doc.

## 0. Context for the session picking this up

| Item | Value |
|---|---|
| Environment | `https://bnk-dev.outsystems.app` (ODC) |
| BackOffice | `/ClaimMVP/...` — ClaimDetail, ApprovalQueue, AgentMap, AgentTrace, ChainTests, Administration |
| Portal | `/MyInsurance/...` — PortalHome, PortalNewClaim, PortalClaims, PortalClaimDetail, PortalSupplement |
| BackOffice sample accounts | Login → Quick sign-in: **Claim officer** (mike), **Approver** (Mai Trung Hieu) |
| Portal sample account | Login → **"Khách hàng · mẫu"** (EN: "Customer · sample") (Nguyễn Văn An) |
| Session note | The two apps share one sign-in session: switching accounts in BackOffice signs the Portal out |
| Language | BackOffice: avatar menu → Language EN/VI. Portal: avatar menu → EN/VI |

**Claims used for reproduction (do NOT delete/archive — they are used in the demo video):**

| Claim | Scenario | Current status |
|---|---|---|
| CLM-2026-0635 | Dental, An, Vinmec Times City | Auto-approved · Paid 525,000 |
| CLM-2026-0634 | Hà, missing examination record → supplement | Paid 376,000 |
| CLM-2026-0638 | An, Tam Anh, over cap + multivitamin excluded | Approved 1,360,000 (mike submitted, Hieu approved) |
| CLM-2026-0633 | Documents in the name LE VAN BINH (AG-4 CROSSCHECK_FAIL) | Declined 0 (mike proposed, Hieu confirmed) |
| CLM-2026-0611 | English invoice, USD, Vinmec Central Park | Paid 1,336,400 ($52.00) |
| CLM-2026-0612 | Old claim already Declined | Declined |

**Priority convention:** P1 = seen directly by customers / judges in the demo · P2 = seen by claim officers in the BackOffice · P3 = technical pages or data.

**General verification for group L (localisation):** switch the app to **EN**, repeat the reproduction steps, take a screenshot. No Vietnamese characters may remain on the UI (except proper names such as people's names). Then switch to **VI** and re-check to make sure the Vietnamese version was not broken.

---

## A. Group L — Vietnamese still showing when the app is in EN mode

### Portal

**L-01 · P1 · Date-picker calendar shows Vietnamese** — ✅ Fixed (batch 3)
- Reproduce: Portal EN → New claim → click the Event date field.
- Actual: "Tháng 10 2026" (EN: "October 2026"), weekdays "T2 T3 T4 T5 T6 T7 CN" (EN: "Mo Tu We Th Fr Sa Su").
- Expected: "October 2026", "Mo Tu We Th Fr Sa Su" (locale follows the selected language).

**L-02 · P1 · Document checklist: label "Received (Báo cáo y tế)"** — 🟡 Fixed in code (batch 2), not yet verified with a real document upload
- Reproduce: Portal EN → New claim → step Photograph documents → upload a document → look at the "Documents this claim needs" column.
- Actual: the Examination record row has the badge "Received (Báo cáo y tế)" (EN: "Received (Medical report)").
- Expected: "Received (Medical report)" — the document type name follows the language.

**L-03 · P1 · Decision step shows Vietnamese status** — ✅ Fixed (batch 4)
- Reproduce: Portal EN → PortalClaimDetail of 0635 / 0638 / 0633.
- Actual: under the Decision step: "Đã chi trả" / "Đã duyệt" / "Từ chối" (EN: "Paid" / "Approved" / "Declined").
- Expected: "Paid" / "Approved" / "Declined".

**L-04 · P1 · Decline reason shows Vietnamese and is truncated** — ✅ Fixed (batch 1)
- Reproduce: Portal EN → `PortalClaimDetail?ClaimId=633`.
- Actual: "Reason: Chứng từ không hợp 1" (EN: "Reason: Invalid documents") — the word "lệ" is lost, and it is in Vietnamese.
- Expected: "Reason: Invalid documents" shown in full. Related to L-09 (reason stored as a code).
- Result: EN "Reason: Invalid documents", VI "Lý do: Chứng từ không hợp lệ" (MyInsurance rev 334).

**L-05 · P1 · Missing-documents list is in Vietnamese** — ✅ Fixed (batch 2, verified on CLM-2026-0497)
- Reproduce: Portal EN → a claim in Waiting for documents → block "We need a few more documents"; and the Send documents page (PortalSupplement) → "This claim needs".
- Actual: "Phiếu khám bệnh" (EN: "Examination record"), "Đơn thuốc" (EN: "Prescription"), "Hoá đơn GTGT điện tử (XML)" (EN: "VAT e-invoice (XML)").
- Expected: "Examination record", "Prescription", "VAT e-invoice (XML)".
- Note: a new claim missing the examination record must be created to reproduce (0634 is already complete).

**L-06 · P2 · Amount formats mixed VI/EN after switching language** — ✅ Fixed (batch 3)
- Reproduce: Portal → switch to VI → open 0638 → switch back to EN (no reload).
- Actual: "Insurer pays 1.360.000 ₫" next to "You bear 770,000 VND".
- Expected: everything in EN locale: "1,360,000 VND".

**L-07 · P2 · Customer assistant replies/greets in Vietnamese while in EN** — ✅ Fixed (batch 7)
- Reproduce: Portal EN → Ask the assistant → ask "CLM-2026-0638 - why was it reduced?".
- Actual: the answer is in English but the amount is "280.000 ₫". In some runs (after switching VI→EN) the greeting is "Xin chào anh An! …" (EN: "Hello, An! …") and the error is "Xin lỗi, hiện tôi chưa trả lời được. Quý khách vui lòng thử lại sau ít phút." (EN: "Sorry, I can't answer right now. Please try again in a few minutes.")
- Expected: greeting, answer, error message and number format all follow the selected language.

**L-08 · P3 · Medical facility names show the Vietnamese version**
- Reproduce: Portal EN → Claims / Home / Claim detail.
- Actual: "Bệnh viện Đa khoa Quốc tế Vinmec Times City", "Bệnh viện Đa khoa Quốc tế Vinmec Central Park".
- Expected (subject to a product decision): if the catalogue has an English name, show the English name when EN ("Vinmec Times City International Hospital"); if not, keep as is — record the decision.
- Decision (19/10): add a NameEn column for medical facilities; when empty, translate using a fallback rule.

### BackOffice

**L-09 · P1 · Propose-rejection reasons are hard-coded Vietnamese** — ✅ Fixed (batch 1)
- Reproduce: BackOffice EN, sign in as Claim officer → open a claim that is Under review and has an AG-4 signal → What needs human judgement → Propose rejection.
- Actual: the options "Trùng hồ sơ / Chứng từ không hợp lệ / Ngoài phạm vi bảo hiểm / Khác" (EN: "Duplicate claim / Invalid documents / Not covered by the policy / Other").
- Expected: "Duplicate claim / Invalid documents / Not covered by the policy / Other".
- Suggestion: store the reason as a code (`DUPLICATE`, `INVALID_DOCS`, `NOT_COVERED`, `OTHER`) and translate at display time. Fixing this one place also resolves L-04 and L-10. Old data (0633 stores the text "Chứng từ không hợp lệ" (EN: "Invalid documents")) must be mapped to codes.
- Result: EN dialog "Duplicate claim / Invalid documents / Not covered by the policy / Other", VI keeps the old labels; only the code is stored (ClaimMVP rev 952). Verified on CLM-2026-0593 (dialog opened only, not submitted).

**L-10 · P1 · Decline reason shows Vietnamese on later screens** — ✅ Fixed (batch 1)
- Reproduce: BackOffice EN → ClaimDetail 633.
- Actual: header "Proposed: Reject · Chứng từ không hợp lệ" (EN: "Invalid documents"); the What the AI proposes card "Chứng từ không hợp lệ"; dialog "Confirm rejection · CLM-2026-0633/1 · Chứng từ không hợp lệ".
- Expected: "Invalid documents". Depends on L-09.
- Result: EN "Proposed: Reject · Invalid documents"; VI "Đề xuất: Từ chối · Chứng từ không hợp lệ" (EN: "Proposed: Reject · Invalid documents").

**L-11 · P1 · Label of the reason field in the Return to assessor dialog** — ✅ Fixed (batch 6)
- Reproduce: sign in as Approver → Approval queue → open a claim → Return to assessor.
- Actual: label "Lý do trả về (bắt buộc, tối thiểu 10 ký tự)" (EN: "Reason for return (required, at least 10 characters)").
- Expected: "Reason (required, at least 10 characters)".

**L-12 · P2 · Four-eyes toast prints an extra Vietnamese sentence** — 🟡 Fixed in code (batch 6); can no longer be reproduced because the button is now locked (F-13)
- Reproduce: Claim officer submits a claim (Submit for approval), then the same account clicks Approve → Approve.
- Actual: "Approver must differ from the latest assessor (mike) — four-eyes rule. / Người phê duyệt phải khác người thẩm định gần nhất (mike) — quy tắc bốn mắt." (the Vietnamese sentence means: "The approver must differ from the latest assessor (mike) — four-eyes rule.")
- Expected: a single language according to locale. (The server-side blocking logic is correct — keep it.)

**L-13 · P2 · Waiting-for-documents claim: missing document name and AG-1 check result** — ✅ Fixed (batch 2, verified on CLM-2026-0497)
- Reproduce: BackOffice EN → a claim in Waiting for documents.
- Actual: block "Waiting for documents from the customer" → MISSING "Phiếu khám bệnh" (EN: "Examination record"); under the Intake step: "✓ R1 Tên bệnh nhân nhất quán" (EN: "R1 Patient name consistent").
- Expected: "Examination record"; "R1 Patient name consistent".

**L-14 · P2 · Exchange-rate source in the Invoice currency field**
- Reproduce: BackOffice EN → ClaimDetail 611 → header, INVOICE CURRENCY field.
- Actual: "Vietcombank – tỷ giá mua chuyển khoản" (EN: "Vietcombank – transfer buying rate").
- Expected: "Vietcombank – transfer buying rate".

**L-15 · P2 · AG-5 customer letter exists only in Vietnamese**
- Reproduce: BackOffice EN → ClaimDetail 635 → Advanced mode → Approval & payment → Customer letters → Edit.
- Actual: "Kính gửi Quý khách Nguyễn Văn An, Hồ sơ bồi thường CLM-2026-0635 … đã được xét duyệt." (EN: "Dear Mr. Nguyễn Văn An, Your claim CLM-2026-0635 … has been adjudicated.")
- Expected: the letter in the customer's language (add a preferred-language attribute to the member and pass it into the AG-5 prompt). At minimum, an English version must be generated for customers who choose EN.

**L-16 · P2 · Descriptions of the 14 steps in Calculation & ledger**
- Reproduce: BackOffice EN → ClaimDetail 635 → Advanced mode → Calculation & ledger.
- Actual: "TERM-HC-001 — KHÔNG_THUỘC_PHẠM_VI" (EN: "NOT_IN_SCOPE"), "KHÔNG_ÁP_DỤNG — cơ sở trong mạng lưới, không điều kiện loại trừ nào đúng" (EN: "NOT_APPLICABLE — in-network facility, no exclusion condition applies"), "Post lặp 4 posting … trạng thái PROVISIONAL" (EN: "Repeat posting of 4 postings … status PROVISIONAL"), "copay 30% (tỉ lệ 300)" (EN: "co-pay 30% (rate 300)").
- Expected: English descriptions; enum codes may stay but a label should be displayed ("Not in scope", "Not applicable — in-network provider…").

**L-17 · P2 · "Đồng chi trả" in the waterfall — NEEDS CONFIRMATION**
- Seen in an earlier recording session (before 19/10); in the last recording the waterfall already showed "Co-payment". Re-check in ClaimDetail (Summary → Why) and Portal (Why you pay part…); if it no longer appears, close it. ("Đồng chi trả" = "Co-payment".)

**L-18 · P3 · AG-4 signals write the catalogue name in Vietnamese**
- Reproduce: ClaimDetail 635 → Approval & payment → Anomaly signals.
- Actual: "Provider 'Bệnh viện Đa khoa Quốc tế Vinmec Times City' partially matched 'BỆNH VIỆN ĐA KHOA QUỐC TẾ VINMEC' in catalogue".
- Expected: acceptable because it is data; fix only if the catalogue has an English name.
- Decision (19/10): AG-4 keeps the original catalogue name, with the EN name in parentheses when available.

**L-19 · P3 · Agent payload in Technical trace**
- Reproduce: `/ClaimMVP/AgentTrace?ClaimId=635`.
- Actual: "Lưu bản nháp RESULT cho CLM-2026-0635. Cần cán bộ lưu ý: không có." (EN: "Saved the RESULT draft for CLM-2026-0635. Points for the officer's attention: none."), AG-3 "Tôi đã đọc clinical context bằng Agent_GetClinicalContext…" (EN: "I read the clinical context using Agent_GetClinicalContext…").
- Expected: the agent replies in the session language (pass the locale into the AG-3/AG-5 system prompt).

**L-20 · P3 · Input/Expected columns of Chain Tests**
- Reproduce: `/ClaimMVP/ChainTests`.
- Actual: "Policy hiệu lực 01/04/2026-31/03/2027, ngày dịch vụ…" (EN: "Policy effective 01/04/2026-31/03/2027, service date…"), "Not eligible, decline PERIOD cấp event…" (EN: "… per-event cap …").
- Expected: English (or bilingual in a separate column). Low priority.

---

## B. Group F — Other functional / display bugs

**F-01 · P1 · Portal date picker does not update the value** — ✅ Fixed (batch 5)
- Reproduce: Portal → New claim → pick Event date, Treated from, Treated to using the calendar → Continue.
- Actual: reports "Treated-from date is required" even though the field has a date. The displayed value is not pushed into the screen variable.
- Expected: picking a date with the calendar saves the correct value. (During recording a script calling onChange had to be used to get past it.)

**F-02 · P1 · Claims list: declined claims still show a payout amount** — ✅ Fixed (batch 4)
- Reproduce: Portal → Claims.
- Actual: 0633 Declined · Payout 496,000 VND; 0612 Declined · Payout 376,000 VND ($14.63 when USD).
- Expected: Payout "—" or 0 when Declined.

**F-03 · P1 · "Why you pay part of this…" of 0638 adds up wrongly** — ✅ Fixed (batch 4)
- Reproduce: Portal → `PortalClaimDetail?ClaimId=638`.
- Actual: lists 340,000 (co-pay) + 150,000 (over cap) + 280,000 "Other policy terms (waiting period, exclusions, annual limit)" + 280,000 "Multivitamin … not covered" = 1,050,000 ≠ 770,000.
- Expected: the 280,000 for the multivitamin appears only once (not counted in "Other policy terms"); the items total 770,000.

**F-04 · P1 · Declined claim: heading "Why you pay part of this 620,000 VND"** — ✅ Fixed (batch 4)
- Reproduce: Portal → claim 633.
- Expected: a heading suited to a declined claim, e.g. "Why this claim was not paid".

**F-05 · P2 · Portal USD: hero card shows "5.200 ₫"** — ✅ Fixed (batch 3)
- Reproduce: Portal → select USD in the header → Home (claim 0611).
- Actual: "5.200 ₫".
- Expected: "$52.00".

**F-06 · P2 · Home: an Approved claim still says "We are reading your documents."** — ✅ Fixed (batch 7)
- Reproduce: Portal → Home, latest claim card (0638 Approved).
- Expected: a status-based description (e.g. "Approved — payment on the way").

**F-07 · P2 · Phone layout (390px) breaks text** — ✅ Fixed (batch 7)
- Reproduce: open the Portal at a 390×844 viewport.
- Actual: "Specialis t consultat ion" broken mid-word; the number "1,360,000" is cut off; stepper labels overlap ("Assessmen Decision").
- Expected: text wraps by word, amounts shrink or wrap.

**F-08 · P2 · Portal menu: "Phone view" has no effect** — ✅ Fixed (batch 7)
- Reproduce: Portal → avatar → Phone view.
- Expected: turn on the phone frame as in the BackOffice, or hide this item.

**F-09 · P2 · Customer assistant does not understand "my latest claim"** — ✅ Fixed (batch 7)
- Reproduce: Portal → Ask the assistant → "What is the status of my latest claim?".
- Actual: "I need a specific claim number…".
- Expected: automatically picks the customer's own latest claim (the built-in suggestion "What's the current step of my latest claim?" must work too).

**F-10 · P2 · After "Propose rejection", Summary and Approval queue still show an Approve proposal** — ✅ Fixed (batch 6)
- Reproduce: an officer uses Propose rejection on a claim → look at Summary and Approval queue.
- Actual: the What the AI proposes card still says "Approve 496,000 VND"; the AI PROPOSAL column in the queue still says "Approve". Only the approver's button changes to "Confirm rejection".
- Expected: clearly show "Proposed by officer: Reject · <reason>" in both places.

**F-11 · P2 · Stepper says "Auto-approved" although an approver clicked Approve** — ✅ Fixed (batch 6)
- Reproduce: BackOffice → ClaimDetail 638.
- Expected: "Approved" (or "Approved by approver") when a person approved.

**F-12 · P2 · AG-4 shows "No signal" after the claim is declined** — ✅ Fixed (batch 6)
- Reproduce: BackOffice → ClaimDetail 633.
- Actual: strip "What AI did" → Anomaly "No signal", while the card "AG-4 · CROSSCHECK_FAIL (MEDIUM) — open" is still below.
- Expected: the strip keeps "CROSSCHECK_FAIL · MEDIUM" (the signal does not disappear once a decision is made).

**F-13 · P3 · Approve button shown to the person who assessed the claim** — ✅ Fixed (batch 6)
- Reproduce: an officer (mike, who also has the Approver role) submits a claim → still sees "Approve · 1,360,000 VND".
- Actual: the server correctly blocks it (see L-12), but the button should not be shown.
- Expected: hide/disable the Approve button with the tooltip "You assessed this claim — another approver must decide."

**F-14 · P2 · System checks report wrongly on a clean claim**
- Reproduce: BackOffice → ClaimDetail 635 → Advanced mode → Extraction & costs → System checks.
- Actual: "✕ Cross-document checks have no failure" (✕ mark) while R1–R9 = 0 warnings.
- Expected: ✓ and the wording "Cross-document checks passed".

**F-15 · P2 · "Gastroscopy" (English) does not map to a benefit**
- Reproduce: create a claim with a "Gastroscopy" line on an English invoice.
- Actual: excluded as a non-covered benefit.
- Expected: map to 200.1 (or the appropriate diagnostic-procedure group), like the Vietnamese version "Nội soi dạ dày" (EN: "Gastroscopy").

**F-16 · P2 · Officer assistant omits the per-visit cap reduction**
- Reproduce: BackOffice → ClaimDetail 638 → Ask the assistant → "Why was this claim reduced?".
- Actual: only says the multivitamin was excluded, "No other line-level reduction reason".
- Expected: state everything: 280,000 excluded (multivitamin), 150,000 over cap 200.1, 340,000 co-payment.

**F-17 · P3 · Agent map is a raw list**
- Reproduce: `/ClaimMVP/AgentMap`.
- Expected: cards/diagram in the style of the other screens.

**F-18 · P3 · Small display issues** — ✅ Fixed (batch 7; the checklist badge part in batch 2)
- The "Decided · Paid" cell in Auto-approval conditions (Approval & payment) is almost unreadable (light text on a white background).
- Portal checklist: a badge overlaps the label on the "VAT invoice" and "Prescription" rows.

---

## C. Suggested order of work

1. **L-09 → L-10 → L-04** (store the decline reason as a code) — one change resolves 3 P1 bugs.
2. **Document type names by language** → L-02, L-05, L-13.
3. **Date/number locale** → L-01, L-06, F-05.
4. **F-02, F-03, F-04** (wrong amounts on the Portal) — the most visible to judges.
5. **F-01** date picker.
6. The rest by priority P2 → P3.

After each group: publish, run `/ClaimMVP/ChainTests` (Run runnable tests + Run gate tests, expect 46/46 and 5/5), and walk through the E2E flow of the related claims again.

---

## D. Fix status (session 19/10)

Constraint: do not archive/delete/change the status of the demo claims 0611, 0612, 0633, 0634, 0635, 0638.

| Batch | Bugs | Status | Revision | ChainTests | EN / VI |
|---|---|---|---|---|---|
| 1 | L-09, L-10, L-04 | ✅ Done | ClaimMVP 952 · MyInsurance 334 | 46/46 · gate 5/5 (12:48) | ✅ / ✅ |
| 2 | L-02, L-05, L-13 (+ F-18 badge) | ✅ Done (L-02 not yet verified with a real upload) | ClaimMVP 953 · MyInsurance 337 | 46/46 · gate 5/5 (13:23) | ✅ / ✅ |
| 3 | L-01, L-06, F-05 | ✅ Done | MyInsurance 338 | 46/46 · gate 5/5 (13:29) | ✅ / ✅ |
| 4 | F-02, F-03, F-04, L-03 | ✅ Done | MyInsurance 339 → 342 | 46/46 · gate 5/5 (13:44) | ✅ / ✅ |
| 5 | F-01 | ✅ Done | MyInsurance 343 → 344 | 46/46 · gate 5/5 (13:58) | not language-related |
| 6 | L-11, L-12, F-10–F-13 | ✅ Done (L-12 could not be reproduced) | ClaimMVP 954 → 962 | 46/46 · gate 5/5 (14:19) | ✅ / ✅ |
| 7 | L-07, F-06–F-09, F-18 | ✅ Done | MyInsurance 345 → 350 · ClaimMVP 963, 966 | 46/46 · gate 5/5 (14:48) | ✅ / ✅ |
| 8 | L-14–L-17, L-19, F-14–F-16 | ⏳ Not started | | | |
| 9 | L-08, L-18, L-20, F-17 | ⏳ Not started | | | |

### Batch 1 — Decline reason stored as a code (19/10, 12:30–12:55)

**ClaimMVP rev 952**
- `Claim.DecisionReasonCode` Text(20) → Text(50) (the cause of the truncated text "Chứng từ không hợp l").
- Static entity `RejectReason`: `DUPLICATE`, `INVALID_DOCS`, `NOT_COVERED`, `OTHER` + LabelEn / LabelVi.
- `RejectReasonLabel(Code, Lang)` server + client versions: accepts a code, also accepts old Vietnamese text (complete or truncated at 20 characters), and returns the text unchanged if nothing matches.
- ClaimDetail: the Propose rejection dialog uses the code as the value, labels by language, default `DUPLICATE` (unchanged). 5 display places (header Proposed, What the AI proposes card ×2, Confirm rejection dialog title, Submitted by) call `RejectReasonLabel`.
- `Portal_GetClaimProgress2` additionally returns `DecisionReasonCode`, `ReasonLabelEn`, `ReasonLabelVi`.
- Administration → **Migrate rejection reason codes** (idempotent, only fixes the reason field). Run once: 5 ClaimEvent rows (821, 1106, 1135 → `DUPLICATE`; 1179, 1303 → `INVALID_DOCS`) + 2 Claims (CLM-2026-0633, CLM-2026-0612: "Chứng từ không hợp l" → `INVALID_DOCS`). No change to status, amounts or IsArchived.

**MyInsurance rev 334**
- Client function `RejectReasonLabel(Code, Lang)` (also accepts prefixes of ≥ 10 characters of the old labels). The "Reason:" line in PortalClaimDetail and `V7ReasonText` use this function; no text truncation.

**Verification**
- BackOffice 633 — EN: "Proposed: Reject · Invalid documents"; VI: "Đề xuất: Từ chối · Chứng từ không hợp lệ".
- Portal 633 — EN: "Reason: Invalid documents"; VI: "Lý do: Chứng từ không hợp lệ"; switching back to EN is correct.
- Propose rejection dialog (opened on CLM-2026-0593, not submitted) — EN and VI labels correct.
- ChainTests: 46/46 pass, gate 5/5 (T-GATE-01…05 ✓, 06–07 manual).
- Still seen on 633 but belonging to other batches: "Từ chối" (EN: "Declined") under the Decision step in Portal EN (L-03), the heading "Why you pay part of this…" (F-04), Vietnamese medical facility name (L-08).

### Batch 2 — Document type names by language (19/10, 12:55–13:25)

Cause: the `DocumentType` catalogue already has `LabelEn` / `LabelVi` (11 rows) but the display places read the Vietnamese text stored in the event or used a hand-written mapping table.

**ClaimMVP rev 953**
- Client function `DocNamesEn(Text)`: converts the Vietnamese labels of the 11 document types to `LabelEn` (replacing longer strings first).
- ClaimDetail: `PiValMissing` (MISSING row in the header) and `EvDetailExpr` (Detail column of the `DOCS_REQUESTED` event) use `DocNamesEn` when EN. The R1–R9 labels (`CcRuleExpr`) already used `RuleNameEn`, no fix needed.

**MyInsurance rev 335 → 337**
- Function `DocTypeLabelEn(Code)` gets the EN name by document type code (e.g. `T1-04` → "VAT e-invoice (XML)"), used in PortalSupplement ("This claim needs"), PortalClaimDetail ("We need a few more documents") and the "Received (…)" badge in PortalNewClaim.
- Rev 335 showed "VAT invoice" for T1-04 (hand-written mapping table) → rev 337 takes it correctly from the catalogue.
- F-18 (checklist part): the "Recommended" badge no longer overlaps the document name.

**Verification (CLM-2026-0497, waiting for an e-invoice)**
- BackOffice EN: "MISSING · VAT e-invoice (XML)", R1–R9 in English ("R1 · Patient name consistent"). VI: "CÒN THIẾU · Hoá đơn GTGT điện tử (XML)" (EN: "MISSING · VAT e-invoice (XML)").
- Portal EN: "This claim needs: VAT e-invoice (XML) · Recommended", "We need a few more documents: VAT e-invoice (XML)". VI: "Hồ sơ đang cần: Hoá đơn GTGT điện tử (XML) · Nên có" (EN: "This claim needs: VAT e-invoice (XML) · Recommended").
- L-02: the "Received (…)" badge was switched to `DocTypeLabelEn` but a real document has not been uploaded to see it (a draft claim will be created).
- ChainTests: 46/46, gate 5/5 (13:23).
- The remaining Vietnamese on 497 is data (expense line names on the invoice, medical facility name → L-08).
- Additional findings when turning on Advanced mode on 497 (moved to batch 8): "Trần per-visit/per-day/per-event" (EN: "Cap per-visit/per-day/per-event"), "Đồng chi trả" (EN: "Co-payment") (→ L-17 still open), the AG-4 sentence "Không phát hiện vi phạm ngày… R4 SKIPPED…" (EN: "No date violation detected… R4 SKIPPED…") (→ L-19).
- Operations note: the old ClaimMVP Mentor session timed out at 300 s twice when scanning the whole ClaimDetail screen; opening a new session and assigning small steps worked.

### Batch 3 — Date / number locale on the Portal (19/10, 13:25–13:30)

**MyInsurance rev 338**
- The date picker (Event date, Treated from/to) takes month / weekday names from the selected language.
- `FormatAmount` formats by language and the currency is shown at render time. The count-up animation on amount cards was turned off because it overwrote the formatted number with the old value — this was the cause of L-06 and F-05. If this effect is needed for the video it must be rewritten to read the current format.

**Verification**
- L-01: EN "October 2026 · Mo Tu We Th Fr Sa Su"; VI "Tháng 10 2026 · T2 … CN".
- L-06 (0638, switch VI then EN without reload): VI all "1.360.000 ₫ / 770.000 ₫"; EN all "1,360,000 VND / 770,000 VND".
- F-05 (USD): Home hero 0638 "$52.92", My claims cards all in $; PortalClaimDetail 0611 "$52.00"; PortalClaims no longer has "₫". Switched back to VND after checking.
- ChainTests: 46/46, gate 5/5 (13:29).

### Batch 4 — Amounts & statuses on the Portal (19/10, 13:30–13:45)

**MyInsurance rev 339 → 342**
- F-02: PortalClaims and the "My claims" card on Home show "—" in the Payout column when the claim is Declined (previously it took the proposed amount). Hero on Home: a Declined claim shows 0.
- F-04: Declined claims use the EN heading "Why this claim was not paid" / VI "Vì sao hồ sơ không được chi trả", keeping the Reason line.
- L-03: the label under the Decision step follows the language (Paid / Approved / Declined).
- F-03: the "Other policy terms" line = the remainder after subtracting co-payment, over-limit and the ✕ lines (lines whose paid amount = 0); hidden when zero. Rev 340 computed it in the Claim's OnAfterFetch while the line list had not finished loading, so it still gave 280,000 → rev 341 moved it to the OnAfterFetch of the line list → rev 342 subtracts only the ✕ lines (not the co-payment part of normal lines).

**Verification**
- PortalClaims EN: 0633 Declined "—", 0612 Declined "—", 0638 1,360,000, 0635 525,000.
- 0638 EN: 340,000 + 150,000 + ✕ Multivitamin 280,000 = 770,000 (no more Other). VI: 340.000 ₫ + 150.000 ₫ + 280.000 ₫ = 770.000 ₫.
- 0633 EN: "Why this claim was not paid · Reason: Invalid documents" · Decision "Declined". VI: "Vì sao hồ sơ không được chi trả · Lý do: Chứng từ không hợp lệ" · "Từ chối" (EN: "Declined").
- 0635 EN: Decision "Paid", co-pay 225,000 = header. 0611: co-pay 334,100 = header.
- ChainTests: 46/46, gate 5/5 (13:44).

### Batch 5 — Portal date picker (19/10, 13:45–14:00)

Cause: the custom-drawn calendar puts the value into the `input type=date` fields (#EvtInput, #FromInput, #ToInput) and fires input/change events, but the OutSystems binding does not listen on these elements, so the screen variables (`FEventDate`, `FTreatedFrom`, `FTreatedTo`) kept their default values.

**MyInsurance rev 343 → 344**
- Rev 343 added a `ReadDomDates` step in the Continue action but looked up the elements by widget id so it did not find them → still failing.
- Rev 344: `ReadDomDates` runs first, looks up by the real id (`EvtInput` / `FromInput` / `ToInput`), clears the old Valid=False state before re-checking; the calendar updates the variables as soon as a day is clicked.

**Verification** (no claim created): picked Treated from 14/10 and Treated to 15/10 with the calendar, left Loss type and medical facility empty, clicked Continue → only "Loss type is required" and "Please enter the healthcare provider" remain; "Treated-from date is required" is gone. ChainTests 46/46, gate 5/5 (13:58).

### Batch 6 — BackOffice approve / decline flow (19/10, 13:30–14:20)

**ClaimMVP rev 954 → 962** (after a timeout, switched to a new Mentor session at rev 960)
- L-11: the label and placeholder of the reason field in "Return to assessor" follow the language ("Reason for return (required, at least 10 characters)…" / "Lý do trả về (bắt buộc, tối thiểu 10 ký tự)…").
- L-12: the four-eyes toast shows only the sentence in the current language; the server-side check is unchanged.
- F-11: step 4 of the stepper says "Auto-approved / Tự duyệt" only when the auto-approval gate approved; when an approver approved it says "Approval / Phê duyệt" (0638).
- F-12: `GetClaimProgress` → aggregate `GetOpenSignal` changed its condition from Status = OPEN to Status ≠ SUPERSEDED, sorted by RaisedOn descending. `SigCountExpr` (Approval & payment) uses `GetTopActiveSignal`.
- F-13: the cause was that the SUBMITTED event stores Actor = display name, PROPOSE_REJECT stores Actor = GUID, and the aggregate that fetches the latest event had no sort. Fixed: sort by ClaimEvent.Id, compare both forms. The latest assessor no longer sees the Approve / Confirm rejection buttons, replaced by the note "You assessed this claim — another approver must decide." / "Bạn đã thẩm định hồ sơ này — cần người phê duyệt khác quyết định." (EN: "You assessed this claim — another approver must decide.")
- F-10: ClaimDetail Summary shows "Proposed by officer: Reject · <reason>"; ApprovalQueue gets an additional `OfficerRejectCode` (the latest event by Id is PROPOSE_REJECT), label translated on the client. Rev 960 fetched the code off by one character ("UPLICATE") → 961 fixed → 962 moved the label to the client because the server returned Vietnamese while in EN.

**Verification**
- 0633 strip "What AI did": "CROSSCHECK_FAIL · MEDIUM" (before: "No signal"); 0635 still "No signal".
- 0638 stepper: "Phê duyệt" (EN: "Approval") (before: "Tự duyệt" (EN: "Auto-approved")).
- CLM-2026-0604 (mike proposed rejection "Trùng hồ sơ" (EN: "Duplicate claim"), awaiting approval), signed in as mike: only "Return to assessor" remains, with the four-eyes note. Summary EN "Proposed by officer: Reject · Duplicate claim"; VI "Cán bộ đề xuất: Từ chối · Trùng hồ sơ" (EN: "Proposed by officer: Reject · Duplicate claim"). ApprovalQueue EN/VI likewise.
- ChainTests: 46/46, gate 5/5 (14:19).
- No approve/decline action was performed on any claim; the status of 0604 is unchanged.

### Batch 7 — Portal: assistant, Home, mobile; BackOffice contrast (19/10, 14:20–14:50)

**MyInsurance rev 345 → 350**
- F-06: the sub-line on the latest-claim card on Home follows the status (Approved → "Approved — payment on the way", Declined, Pending information…) instead of the fixed "We are reading your documents."
- F-08: hid the "Phone view" item in the Portal avatar menu (there is no phone frame as in the BackOffice).
- L-07: the customer assistant sends the selected language with every turn; greeting, answer and error message follow EN/VI.
- F-09: "my latest claim" / "hồ sơ mới nhất" (EN: "latest claim") automatically takes the customer's own latest claim (sorted by Claim.Id descending, rev 347). Rev 345 wrongly locked the answer to the previously selected claim (0633) → rev 346 gives priority back to a claim number stated explicitly in the question.
- F-07: CSS ≤ 480px — header wraps, stepper no longer overlaps labels (349); hero amount `.v7-receipt-top .mk-receipt-amt` 30px, ellipsis removed; expense line name `.v7-item-name` breaks by word (350).

**ClaimMVP rev 963, 966**
- L-07 (data side): `CustomerAssist_GetClaim` formats amounts by language (EN "280,000 VND", VI "280.000 ₫").
- F-18 (BackOffice part): the "Decided · Paid" cell in Auto-approval conditions changed to a dark background with light text — readable on the dark theme.

**Verification**
- 390×844 frame, PortalClaimDetail 0638: "1,360,000 VND" shown in full (30px, scrollWidth = clientWidth); "Specialist consultation", "Multivitamin (dietary supplement)" wrap by word; no horizontal scroll (386px).
- Portal assistant EN: "latest claim" → CLM-2026-0638; amount "280,000 VND"; VI replies in Vietnamese, "280.000 ₫".
- 0635 Approval & payment: "Decided · Paid · 19/10/2026" clearly readable.
- ChainTests: 46/46, gate 5/5 (14:48).
- Not fixed (noted): the aria-label of the "Treated to" field is still "Điều trị đến" (EN: "Treated to") when in EN; expense line names / medical facility names in Vietnamese are data (medical facilities belong to L-08, batch 9).
