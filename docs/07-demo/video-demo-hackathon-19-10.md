# Demo video for the hackathon (19/10)

The demo video was built from the "10-Minute Demo Video Script" (9 scenes). Narration is Kokoro TTS, voice am_michael, speed 1.05. Subtitles are burned in and also provided as an .srt file.

Pipeline: screenshots of bnk-dev (ClaimMVP rev 951 / MyInsurance rev 333) → HTML slides (Playwright) → Python/OpenCV renderer (Ken Burns, highlights, crossfades) → ffmpeg H.264 1080p30.

## Demo data on bnk-dev
| Claim | Case | Result |
|---|---|---|
| CLM-2026-0635 | Dental, An, Vinmec Times City, English documents | Auto-approved · paid 525,000 |
| CLM-2026-0634 | Hà, examination record missing → supplement | Resumed · paid 376,000 |
| CLM-2026-0638 | An, Tam Anh, cap + multivitamin excluded (AG-3 confirmed) | Submitted by mike · approved by Hieu · 1,360,000 |
| CLM-2026-0633 | Documents for LE VAN BINH (name mismatch, AG-4 CROSSCHECK_FAIL) | Rejection proposed by mike · confirmed by Hieu · 0 |
| CLM-2026-0611 | English USD invoice, Vinmec Central Park | Paid 1,336,400 ($52.00) |

Archived: 0632, 0636, 0637 and 56 old test claims (0535–0583, 0600–0603, 0605, 0610, 0631).

Four-eyes check: mike (assessor) clicking Approve on 0638 → the server refused: "Approver must differ from the latest assessor (mike)".

## Bugs found while capturing
1. Portal, new claim: the date picker does not update the bound values ("Treated-from date is required" even when a date is chosen).
2. EN mode still shows Vietnamese text in several places: checklist (Phiếu khám bệnh, "Received (Báo cáo y tế)"), "Đồng chi trả" in the waterfall, decision status "Đã chi trả / Đã duyệt / Từ chối", the calendar, supplement document names, the "Vietcombank – tỷ giá…" line, the 14-step descriptions, the AG-3/AG-5 payloads, the Return-to-assessor label "Lý do trả về…", and the Propose-rejection reasons.
3. The AG-5 letter is only in Vietnamese.
4. Portal 0633 (declined): the reason shows "Chứng từ không hợp 1" (cut off and in Vietnamese), and the heading reads "Why you pay part of this 620,000 VND".
5. Portal 0638: "Why you pay part of this 770,000" lists 280,000 twice ("Other policy terms" and "Multivitamin"), so the items add up to 1,050,000.
6. Portal claims list: declined claims still show a payout (0633: 496,000; 0612: $14.63).
7. Portal USD mode: the hero shows "5.200 ₫" instead of $52.00; after switching EN↔VI, the claim card mixes "1.360.000 ₫" and "VND".
8. Portal home on an approved claim still says "We are reading your documents"; on mobile, labels wrap badly ("Specialis t consultat ion") and the payout is cut off.
9. Portal account menu: "Phone view" does nothing.
10. BackOffice: after "Propose rejection", the Summary and the Approval queue still show "AI proposal: Approve 496,000"; only the approver's button changes to "Confirm rejection".
11. BackOffice 0638: after a human approved it, the stepper still says "Auto-approved".
12. BackOffice 0633: after the decline, the AI strip shows AG-4 "No signal" while the CROSSCHECK_FAIL card is still open.
13. BackOffice: the Approve button is shown to the assessor (the server blocks the approval, but the button shouldn't appear).
14. Clean claim 0635: System checks shows "✕ Cross-document checks have no failure" while R1–R9 have 0 warnings.
15. English "Gastroscopy" is not mapped to a benefit (it was declined as not covered).
16. Assistants: the staff assistant skips the per-visit cap cut; the customer assistant asks for a claim number when asked about "my latest claim", and in a phone frame it answered "Xin lỗi, hiện tôi chưa trả lời được".
17. Agent map is a plain, unstyled list.
18. Low-contrast "Decided · Paid" box in Auto-approval conditions; badges overlap labels on the checklist (VAT invoice, Prescription).
