# Final E2E round before recording the demo — 18/10/2026

> English translation for reviewers. Vietnamese original: [e2e-vong-cuoi-truoc-quay-demo-18-10.md](e2e-vong-cuoi-truoc-quay-demo-18-10.md).

Final build: **ClaimMVP rev 951 · MyInsurance rev 333** (bnk-dev environment). ChainTests: **46/46 pass · gate 5/5** (re-run on ClaimMVP 951 at 22:32).

## 1. Result per case (claims created today)

| Case | Claim | Scenario | System result | Pass? |
|---|---|---|---|---|
| E04 + E04S | CLM-2026-0604 | Outpatient, Hà, Thanh Nhàn, missing medical examination slip → supplement | Waiting for supplement → supplemented via the portal → continues; AG-4 catches **DUPLICATE HIGH** (duplicates 0602/0603 from repeated tests) | ✅ (supplement flow + duplicate) |
| E05 | CLM-2026-0606 | Dental, An, Vinmec | 750.000 → **525.000 ₫** (30% co-payment), auto-approved, Paid | ✅ |
| E06 | CLM-2026-0607 | Outpatient accident, Hà, Bạch Mai | **830.000 ₫**, no co-payment, auto-approved, Paid | ✅ |
| E08 | CLM-2026-0608 | Outpatient maternity, Hà, Vinmec | 1.600.000 → cap 200.1 1,5tr → **1.200.000 ₫**; officer submits → approver approves | ✅ (see note 6) |
| E09 | CLM-2026-0609 | I10, An, Tâm Anh | 14-step chain declines at the waiting-period step; officer submits → approver **Decline** | ✅ |
| E07B + E07BS | CLM-2026-0614 | Inpatient, An, Tâm Anh, missing discharge certificate → supplement | Waiting for supplement → discharge certificate supplemented → **pipeline re-runs automatically** → **5.900.000 ₫** (bed cap 1,5tr/day); AG-4 DATE_IMPOSSIBLE because of the signing date on the sample discharge certificate (test data error, AG-4 catches it correctly) | ✅ |
| E11 | CLM-2026-0611 | English USD invoice, Vinmec Central Park | 65 USD × 25.700 = 1.670.500 → **1.336.400 ₫**, auto-approved, Paid | ✅ |
| E12 | CLM-2026-0612 | Documents in the name LÊ VĂN BÌNH, claimed for An | AG-4 **CROSSCHECK_FAIL** (R1 name mismatch); officer proposes decline → approver confirms | ✅ |
| E03 | CLM-2026-0613 | "Tam Anh General Hospital", with dietary supplements | In-network facility matched; AG-3 NOTCOV vitamin; cap 200.1 → **1.296.000 ₫** awaiting a person's decision | ✅ |
| S0445 | CLM-2026-0445 | Discharge certificate supplement for an old claim | "Waiting for supplement" cleared, pipeline continues | ✅ |
| Final check (build 951/333) | CLM-2026-0631 | Outpatient E01, An, Vinmec 16/10 | Checklist 2/2 "Đã đủ giấy tờ" (EN: "All documents provided") → submit → read 3 documents/4 lines, 484.000 ₫ → AG-1 Package complete, AG-4 no signal → **387.200 ₫** (20% co-payment), gate 4/4, auto-approved, Paid | ✅ |
| Four-eyes | 0604/0608/0609/0612 | Proposer ≠ approver | Server blocks "Approver must differ…"; officer → approver works correctly | ✅ |

## 2. Bugs fixed in this round (published, re-verified in the UI)

**ClaimMVP (937 → 951)**
1. **P0 VND amounts divided by 1000** (150.000 → 150): IDP returned the cell "150" for "150.000". Added a safety net after lines are created: if sum of lines × 1000 ≈ TOTAL_COST (±2%) then multiply by 1000 and write AgentRun "VND scale fix ×1000". Verified 0604 → 0614: amounts are correct.
2. Inpatient claims: require "Giấy ra viện" (EN: "Discharge certificate") **or** "Báo cáo y tế" (EN: "Medical report") (previously only Medical report was accepted → stuck waiting for supplement).
3. Pipeline **stuck after documents were supplemented** (old track state blocked AG-3/AG-4/chain): reset when the claim leaves "waiting for supplement".
4. The "AI processing" card shows **Paused — waiting for customer supplement**, the clock stops.
5. The header/decline-proposal card no longer prints the "DUP:…" note verbatim in large type; it shows "Proposed: Reject · reason · 0 VND".
6. The Approver sees the **Confirm rejection** button (instead of "Approve · 0 VND"); confirming a rejection goes via the decline path (0 ₫, reverses the provisional ledger entry), not blocked by an open fraud signal. Fixed the save error "value too long varchar(20)" when confirming a rejection.
7. Four-eyes uses the **most recent** proposer; label "Lý do trả về" (EN: "Return reason"); chip "Thời gian chờ" (EN: "Waiting period") replaces "Cap reached" for claims declined because of the waiting period; cost lines before the chain runs show "Pending calculation".
8. Console: **Sign out is in the account menu** (last row), the button outside the header is hidden; it still goes through a confirmation popup (rev 949–950).
9. Signing out / entering the Login page: **clears stale error toasts** left over from the previous screen (rev 951).

**MyInsurance (311 → 333)**
1. The notification bell **did not show claims of dependants** (Hà) → filtered by the same policy, newest first.
2. Supplement documents page: the list of claims was missing the dependants' claims, did not pre-select the claim from the link, and the banner showed the wrong patient name → fixed.
3. **Creating a 2nd claim in the same session could not upload documents** (OS-CLRT-60801, StartAutoUpload of the old screen) → listener re-attached on every screen entry.
4. The "Hình thức điều trị" (EN: "Treatment type") buttons (Dental/Accident/Maternity) had no CSS → now have a segmented style.
5. Document checklist: outpatient accepts Medical report/Discharge certificate in place of the Examination slip; inpatient requires Discharge certificate/Medical report; VAT invoice is only "Nice to have" (fixed the loss-type code mapping bug).
6. Declined claims: hero "0 ₫ · Không chi trả" (EN: "No payout"), shows **Lý do: …** (EN: "Reason: …") (the text sent to the customer).
7. Session expiry: redirects to Login + toast "Phiên đăng nhập đã hết hạn" (EN: "Your login session has expired") instead of a generic error.
8. Sign-out (both apps): confirmation popup → Login page → toast "Bạn đã đăng xuất thành công" (EN: "You have signed out successfully"), EN/VI, light/dark, Esc/Cancel work.
9. **Step 2 checklist (Create claim) wrongly reported "Còn thiếu: Phiếu khám bệnh" (EN: "Missing: Medical examination slip")** even though everything had been uploaded → fixed (rev 329): checklist rows are recomputed from the document type recognised by the AI; substitute documents are also accepted (Medical report/Discharge certificate in place of the Examination slip; Discharge certificate ↔ Medical report for inpatient). Verified: outpatient E01 → 2/2 mandatory "Đã đủ giấy tờ" (EN: "All documents provided"); inpatient E07 → missing Discharge certificate, adding E07S → 2/2.
10. **Supplement page, "Attach + Send" area**: pressing Send with no claim selected / empty content and no file → previously no response (or an empty submission was sent) → now shows "Vui lòng chọn hồ sơ cần bổ sung." (EN: "Please select the claim to supplement.") / "Vui lòng nhập nội dung hoặc đính kèm chứng từ." (EN: "Please enter content or attach a document."); a successful send shows a notification (rev 330–333).
11. A stale warning toast still showed on the Login page after sign-out → cleared on sign-out and when the Login page opens; the "signed out successfully" toast still shows (rev 332).

## 3. Remaining / notes for recording the demo

1. **The two apps share one login session** in the same browser: signing out of the console also signs out of the customer portal, and signing in to the console with a sample account changes the identity at the portal. When recording: **open the customer portal and the console in 2 different Chrome profiles (or 1 incognito window)**.
2. The session expires after about 1 hour — sign in again right before recording.
3. Supplement documents page: the upload area at the top (AI recognises automatically, submits automatically) is the best path to record. The old claim CLM-2026-0004 has 4 documents stuck at "Đang nhận diện…" (EN: "Recognising…") since the start of the month — do not choose this claim when recording.
4. Mike needs to confirm the business logic: (a) outpatient maternity is currently calculated under benefit 200.1 (cap 1,5tr/visit, 20%) → 1.200.000 ₫; (b) the Events & cover screen shows "2 days" for the inpatient stay 08–10/10 while the bed cap is calculated for 3 days.
5. The medical facility catalogue does not yet have "Bệnh viện Thanh Nhàn" (can still be submitted, shown as "Không có trong danh mục" (EN: "Not in the catalogue")).
6. **Dirty test data** (proposal for Mike to decide on archiving): CLM-2026-0535–0583 (old), 0600–0603 (amounts wrongly ÷1000 before the fix + duplicates), 0605 (broken draft), 0610 (stuck before the fix), 0631 (final check submission), unsubmitted drafts created while testing the checklist (An · Vinmec 16–17/10, An · Tâm Anh 01–03/10), and **CLM-2026-0004 received 3 trial supplements** (2 empty + 1 "Test nội dung" (EN: "Test content")) while testing the Send button. These claims make AG-4 report a duplicate if the demo is re-run on the same day/same facility.

## 4. Checklist before recording

- [ ] Sign out: console = the "Sign out" row at the bottom of the account menu; portal = "Đăng xuất" (EN: "Sign out") in the account menu — both have a confirmation popup.
- [ ] 2 Chrome profiles: (A) customer portal — card "Khách hàng · mẫu" (EN: "Customer · sample"); (B) console — Claim officer / Approver.
- [ ] Sign in again to both right before recording (avoid session expiry).
- [ ] Use a treatment date/facility that has **not been used** to avoid duplicate warnings (or archive the test data in section 3.6).
- [ ] Suggested script: E05 (fast auto-approval) → E11 USD → E04 missing documents + supplement → E12 name fraud → officer proposes decline → approver confirms → customer sees the reason.
