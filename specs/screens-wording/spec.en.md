# Spec — Screens & wording

> English translation for reviewers. Vietnamese original: [spec.md](spec.md).

> **Full text:** Project → `claude/build-06-man-hinh-va-wording`.
> Upstream sources on disk: `../../Preparing/04_Nghiep-vu-va-du-lieu/A2_Message_and_Notification_Catalogue_v1.2.md` (sentences) and `D2_Functional_Specification_v0.6.md` (screens).

## ⚠ The actual scope has outgrown the spec

`build-06` §0.2 trims this to "6 must-have screens". In practice **17 screens** have been built (10 ClaimMVP console + 7 MyInsurance portal).
⇒ **The 53 messages in `build-06` §5 do not cover all the text currently shown on screen.** The 17-screen reconciliation table: Project → `trang-thai-build-2026-09-20` §8.

## Wording invariants

1. **Screens PLACE sentences, NEVER compose them.** Every Vietnamese sentence arrives as **one complete A2 template with placeholders already bound**. No string concatenation. Reason: classifiers and Vietnamese word order do not survive being assembled from fragments.
2. **A single render function** `(messageCode, placeholderMap, language) → string`. Every screen, every agent, every letter goes through it. A missing code **fails explicitly**, so that nobody writes a stopgap sentence that then goes straight out in a letter to a customer.
3. **Never expose internal identifiers to the customer** — the 13 `claimState`s, the 7 document codes `T1-nn`, `REQUIRED/EXPECTED/OPTIONAL`, the 12 `lossType`s. Render the Vietnamese names per A1 §6 and `"Bắt buộc"` (EN: "Required") / `"Nên có"` (EN: "Recommended") / `"Tùy chọn"` (EN: "Optional").
4. **A system error is never phrased as a decision** (M-6). `SYS-ERR-001` is the only error message a customer sees on a claim, emitted **once**, never repeated.
5. **A push notification never says what the result is** — it is always `"Đã có kết quả hồ sơ {claimNo}"` (EN: "A result is available for claim {claimNo}").
6. **A declined line sits IN THE SAME TABLE as a paid line** on the result screen. No accordion, no "view details".
7. **There is always an appeal path** on every declined or partially declined result.
8. **No `TBC-nn` in a letter sent to a customer.** A letter that cites `TBC-04` is a document with a placeholder, on company letterhead, about declining a payout — and it cannot be retracted because it has already been read.
9. **Permission is absence, not disablement.** A `—` cell in the matrix means the button **does not exist in the DOM**, and the Server Action re-checks role + scope itself.

## Four places deliberately masked from the customer

`EXTRACTION_ERROR` → `"Đang xử lý chứng từ"` (EN: "Processing documents") · `DOC_PROCESSING` stuck → say nothing more · `PAYMENT_IN_PROGRESS` stuck → say nothing more · `PENDING_APPROVAL` → `"Đang thẩm định"` (EN: "Under assessment").

## Currency unit

`1.200.000 đ` for **everything the customer reads** (A2 M-4). The console may use `₫`.

## Checklist before applying wording — run it on 17 screens, not 6

`build-06` §10. The most important item: **grep the portal's rendered HTML** for the 13 `claimState`s + 7 document codes + 3 levels + 12 `lossType`s — none may match. The three new portal screens (`PortalHome`, `PortalBenefits`, `PortalNotifications`) have never been within the scope of the A2 wording review ⇒ most likely to leak.
