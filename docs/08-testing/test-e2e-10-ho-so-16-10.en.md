# End-to-end test of 10 claim sets — 16/10/2026 + bug-fix round 16–17/10 + UI/UX round 17/10 (updated 18:10 17/10)

> English translation for reviewers. Vietnamese original: [test-e2e-10-ho-so-16-10.md](test-e2e-10-ho-so-16-10.md).

Tracker: https://claude.ai/artifact/1obBuNLDUzNPkWEbzFD9P9 (152 test cases: 55 functional + 97 UI/UX; tab "Kết quả chạy 16/10" (EN: "Run results 16/10"); results are recorded as "Claude · chạy tự động 16/10" (EN: "Claude · automated run 16/10") / "Claude · chạy lại 17/10" (EN: "Claude · re-run 17/10") / "Claude · vòng UI 17/10" (EN: "Claude · UI round 17/10")). All 152/152 cases have a result.
Document set: `E:\Hackaton\output\bo-chung-tu-test-16-10\` (PNG/JPG + manifest.json). Uploaded via a canvas rebuilt on the page (localStorage `bnkR`/`bnkC`/`bnkF` on bnk-dev) because the Chrome extension does not allow file_upload. Re-run variants: R07, R07C, R05, R05B, R02, R01, R06 (Hà accident 13/10), R04A/R04H (An's itemised bill + examination record added in Hà's name), R04B/R04C/R04D.

**Current build: ClaimMVP rev 853, MyInsurance rev 281** (after the UI round; before that 835/268). ChainTests 46/46, gate 5/5 (re-run 17/10 14:50). Tracker: functional cases all Pass except FC-07 (awaiting a business decision); UI/UX 91 cases in round 3: 73 Pass · 7 Fail · 3 Could not be checked · 8 Not applicable. Safe rollback point before the fix round: ClaimMVP 790, MyInsurance 247. Mentor: ClaimMVP session `6ae8a2a7…` (opened from rev 806), MyInsurance `18356fac…`.

## Round 1 results (rev 790 / 247)
| Set | Claim | Result |
|---|---|---|
| TC01 An · clean | CLM-2026-0535 | ✅ Auto-approved 411.200 ₫, AG-5 letter sent |
| TC02 Hà · clean | CLM-2026-0536 | ❌ Wrongly recorded for An |
| TC03 An · over cap + vitamin | CLM-2026-0537 | ⏸ correct, needs a second approver |
| TC04 Hà · missing examination record | CLM-2026-0538 | ❌ AG-4 missed the name mismatch on the supplement flow → paid out |
| TC05 An · dental | CLM-2026-0539 | ❌ 470.000 instead of 455.000 |
| TC06 Hà · accident | CLM-2026-0540 | ❌ classified as outpatient; splint excluded |
| TC07 An · inpatient 3 days | CLM-2026-0541 | ❌ bed charge wrongly cut + Ceftriaxone excluded → 2.170.000 |
| TC08 Hà · maternity | CLM-2026-0542 | ❌ IDP lost 1 line; co-payment 20% (business question) |
| TC09 wrong name + outside catalogue | CLM-2026-0543 | ❌ old date kept; ICD "110" |
| TC10 duplicate submission | CLM-2026-0544 | ❌ officer could not close/decline |

## After the fix round — verified on real claims
| Bug | Result | Evidence |
|---|---|---|
| FC-03 bed cap/day × number of days | ✅ | CLM-0545/0547; 0541 re-run → 4.780.000 |
| FC-04 inpatient medication | ✅ | Ceftriaxone paid at 100 |
| FC-05 dental | ✅ | CLM-0546/0563 455.000 ₫ |
| FC-06 summary card by posted benefit | ✅ rev 801 | CLM-0559 Hà accident: group 500, no co-payment, 655.000/655.000 (splint paid) |
| FC-08 double deduction on Why | ✅ | |
| FI-05 AG-4 name comparison on supplement | ✅ | CLM-0560: supplementing an examination record in Hà's name into An's claim → CROSSCHECK_FAIL MEDIUM, auto-approval blocked |
| FP-02 choose a relative | ✅ | CLM-0558 Hà, auto-approved 417.600 ₫ |
| FP-03 go back and edit step 1 | ✅ (workaround) | a new draft is created when step 1 changes; the old draft is left behind |
| FG-06 "Trả về cán bộ" (EN: "Return to officer") | ✅ rev 802–803 | popup requires ≥10 characters; "Lý do trả về … · 17/10/2026 06:00" (EN: "Reason for return … · 17/10/2026 06:00") in correct local time |
| FG-07 "Đề xuất từ chối" (EN: "Propose rejection") | ✅ rev 800 + 804 | CLM-0544 → Awaiting approval; card title "Đề xuất từ chối" (EN: "Propose rejection") (reads the PROPOSE_REJECT event) |
| FG-05 approval lock for the proposer | ✅ code | needs a second approver account |
| FG-10 low-confidence line on a paid claim | ✅ | no longer shown once paid |
| FP-14 / UX-04 portal status ≠ console | ✅ MyInsurance 249+ | 0561/0562 yellow chip "Chờ bổ sung chứng từ" (EN: "Waiting for supplementary documents") |
| FP-16 supplement-documents page | ✅ MyInsurance 250–253 | title "Bổ sung chứng từ còn thiếu" (EN: "Supplement missing documents"), Send button enabled, banner "Hồ sơ CLM-2026-0562 (Nguyễn Văn An · Ngoại trú)…" (EN: "Claim CLM-2026-0562 (Nguyễn Văn An · Outpatient)…"), hidden when no claim is selected; menu opens the latest claim awaiting supplements |
| Date "Decided · Paid · …" | ✅ rev 807 | taken from the decision event's CreatedOn + 7h (GetClaimSummary → GetDecisionEventForSummary): 0558 → 17/10, 0546 (decided at 23:59 16/10) → 16/10 |
| ClaimDetail auto-refreshes when orchestration finishes | ✅ | CLM-0563 |
| Number of inpatient days | ✅ | shows 3 |

### Incidents during the fix round (resolved)
- rev 793–794: the expense line list went blank and then showed every line as "Excluded" → fixed in rev 795.
- **rev 795 (00:37–03:55): P0 — every portal claim reported "Claim is not in Draft status" on submit.** 6/13 Portal_* actions were missing `Claim.Id = ClaimId` → fixed in rev 796. Drafts 0549–0556 were moved to "Received" (not shown in the Work list).
- rev 799: Why/Evidence summed the ledger of all runs (0541 showed 16.250.000) → rev 800 filters to the latest run; rev 800 counted lines twice at the parent group (0541 showed 8.380.000) → rev 801 counts only at leaf benefits.
- User comments were being "translated" ("và" → "and") in the log → fixed in rev 802.
- Decision date: rev 806 fixed the wrong widget (DecidedTitle); the real line is ItemTitleExpr in the "What needs human judgement" list, built on the server → rev 807. Mentor session `0da75771…` expired (300s timeout, 3 "Unavailable feature" errors — pre-existing, do not block publishing) → closed, opened a new session.

### Still open after the fix round
- The "Đề xuất từ chối" (EN: "Propose rejection") popup does not yet show the duplicate claim number; the badge "1 open signals" after "Clear with reason" has not been re-checked; the Portal reports "Không thể gửi hồ sơ" (EN: "Cannot submit the claim") when the session has expired (it should ask to sign in again).
- Tracker still shows ❌ from the 16/10 run, not yet in this fix round:
  - Adjudication: FC-07 20% maternity co-payment (needs business confirmation), FC-09 "Over cap" empty group.
  - IDP/AG-4: FI-02 lost line TC08, FI-03 ICD "I10"→"110", FI-04/TY-02 "750,000 VND ₫", FI-06 same Vinmec name both in and out of network, FI-07 missing PROVIDER_UNMATCHED, FA-06.
  - Portal: FP-04 future date passes step 1, FP-06 treatment type, FP-07/FP-11/FP-13 checklist "Optional/Missing" wrong, FP-10 English message, FP-15 wrong "Next step", FP-17 "Xin chào, Hieu!" vs "Chào An" (EN: "Hello, Hieu!" vs "Hi An").
  - Letters: FM-03 Send button after sending, FM-04 letter download button while still a draft, FM-05 "will be forwarded" vs "has been forwarded".
  - Language: LN-01 English strings on the VI page, LN-02 tab title.


## Round 2 — 17/10 09:30–12:20 (fix batches to the end)
Sign-in: the session expires about once an hour; Claude signs back in with the one-tap card (Approver for the console, Customer for the portal), without typing a password.

| Batch | Items | Result | Evidence |
|---|---|---|---|
| 1 Portal | FP-17 greeting, LN-02 tab title, FP-10/LN-01 VI strings, FP-04 block future dates, FP-07/11/13 checklist, FP-15 next step, FM-04/05 letter & status | ✅ | CLM-0566, 0574, 0575, 0563 |
| 2 Console | TY-02/FI-04 amount "655.000 ₫", FM-03 lock sent letter, payment time (+7h twice), FA-05 badge, duplicate popup with claim number, VI waiting-for-documents block, VI IDP log, "Chờ tính" (EN: "Pending calculation") | ✅ | 0541/0558/0559/0563/0583 |
| 3 Logic | FC-09 ECG ("Điện tim") → 200.1; FI-03 ICD normalisation (110→I10); FI-06 "chưa đối chiếu" (EN: "not yet cross-checked"); FI-07 PROVIDER_UNMATCHED; FA-06 TC09 signal; FI-02 parser skips lines starting with "Tổng" (EN: "Total") | ✅ | 0574, 0575, 0580 (TC08 all 3 lines) |
| 4 FP-06 | "Hình thức điều trị: Ngoại trú/Nội trú" (EN: "Treatment type: Outpatient/Inpatient") for Accident/Maternity/Dental; console "Tai nạn · Nội trú (3 ngày)" (EN: "Accident · Inpatient (3 days)") | ✅ | CLM-0578 |
| Four-eyes | FG-05/UX-02 the submitter does not see Approve/Decline + explanatory sentence; FG-08 second person (mike) approves | ✅ | 0544, 0537 |
| Clean end-to-end | An rhinitis 15/10 → auto-approved 144.000 ₫, portal matches | ✅ | CLM-0582 |

### Regressions introduced and fixed in round 2
- The money-format fix made the "Why" card take the wrong source (0559 showed 20% co-payment, 0558 counted the line twice, 0541 gave 8.380.000 then 0) → GetMoneyFlow: each line takes exactly one most specific posting of the latest run that has a ledger entry; "ledger match" only when equal to the proposed amount.
- Changing the four-eyes check re-exposed the Approve button to the person who submitted the rejection proposal (0544) → the submitter is taken from the latest SUBMITTED/PROPOSE_REJECT event.
- The Mentor session often times out at 300s after ~10 turns → close/open a new session (ClaimMVP: 0da75771 → 6ae8a2a7 → beb23226 → 204029ca → 04b2f6ac; MyInsurance: 18356fac → 018fa3cc → 1291bd81).

### Other findings
- TC09 re-submitted: ICD read correctly as I10 (hypertension, pre-existing condition) → the chain declines the whole claim due to the 365-day waiting period — correct per the rules; the old 0543 paid partially only because of the wrong ICD "110".
- The TC08 document set prints "Tổng cộng 1.070.000" (EN: "Total 1.070.000") while the 3 lines = 870.000 → R5 reporting a mismatch is correct.
- The Sign out button now goes back to Login; clicking the one-tap card while a session exists signs out first.

### Waiting for Mike
- ~~FC-07~~ → Mike decided to keep the 20% co-payment (17/10 15:00).
- Archive the test claims 0535–0583 + drafts 0549–0556.
- ~~LY-09~~ → yes, to be done (completed in round 4).

## Round 3 — UI/UX 91 cases, 17/10 13:00–14:55 (Vietnam time)
How it ran: an audit script in the browser (localStorage `bnkA`/`bnkS`/`bnkW` + iframe frames `bnkF2`/`bnkF3` for widths 1440/1366/1100/420 px), scanning 19 console screens + 8 portal screens in dark and light, then manual checks of the bell, popups, menus, assistant panel, EN/VI, phone 420 px. Every publish was re-checked on the live page and the build number confirmed with `env_app` (Mentor repeatedly reported "done" before publishing).

**Results recorded in the tracker (91 cases, label "Claude · vòng UI 17/10" (EN: "Claude · UI round 17/10")):** 73 Pass · 7 Fail · 3 Could not be checked · 8 Not applicable. The tracker now has results for all 152/152 cases.

| Group | Fixed and re-checked | Build |
|---|---|---|
| Bell (console) | VI text, Esc closes, "Đánh dấu đã đọc tất cả" (EN: "Mark all as read") | ClaimMVP 840 |
| Irreversible actions | popup "Trình duyệt hồ sơ?" (EN: "Submit the claim for approval?") (Cancel changes nothing — tried on CLM-0545); popup has role=dialog, Esc closes, focus goes into the popup, focus returned | 840–843 |
| Money | "Cần bạn" (EN: "Needs you") 1,900,000 VND → 1.900.000 ₫ | 840 |
| Menu | Esc closes the dropdown; Administration moved into Technical ▾ ("Kỹ thuật ▾"); added Provider lookup ("Tra cơ sở y tế") to Lookup ▾ ("Tra cứu ▾") | 840, 843–845 |
| Contrast | brand sub 4.48 → ≥4.5 (@media override rule in the theme); AgentMap labels; C1/C2 badges; "Đổi tài khoản mẫu" (EN: "Switch sample account") | 841–842 |
| Accessibility | aria-label for bell, theme, EN/VI, DdStatusVi, PolDropdown, MemDropdown | 841, 847–849 |
| Layout | removed shadow on ordinary cards; synchronised title margins; scrollbar + ::selection follow the theme; ClaimDetail tab row scrolls; assistant panel no longer covers content | 842–847 |
| Phone | console header on one row with logo; Dashboard 420 px (summary block squeezed to 0–16 px) → stacked vertically; WorkList table scrolls inside its frame | 842–845 |
| VI text | Accident, OpsQueue extraction errors, Policies, AgentMap, AgentTrace, Administration, ROI (minutes/day), ChainTests, ISO date in AgentTrace | 842–848 |
| Portal header | no more green overlay in dark; theme button + bell moved to the top bar next to the greeting (dark/light); bell dropdown sits under the bell and above the content; phone header ☰ + menu drawer; tooltip for truncated rows | MyInsurance 274–281 |

**Current build after round 3: ClaimMVP rev 853, MyInsurance rev 281.** ChainTests 46/46, gate 5/5 (re-run 17/10 14:50).

### Regressions introduced in round 3 (resolved)
- MyInsurance 274: hiding the old header lost the theme button + bell on desktop; 277 moved them to the menu bar but in light it was white-on-white, 279 fixed; 280 pushed the bell dropdown off-screen, 281 fixed (and z-index).
- ClaimMVP 844: adding "Tra cơ sở y tế" (EN: "Provider lookup") as a top-level item overlapped "Kỹ thuật ▾" (EN: "Technical ▾") → 845 fixed.
- **ClaimMVP 840–850: the server-side C1 evidence string was changed to "1,120,000 VND" and then translated into Vietnamese → T-GATE-01/02 failed, gate 3/5.** 851 restored the original English string + VI amount ("1.120.000 ₫ vs threshold 2.000.000 ₫ · 0 limit cut(s)"), translating only on the display side → gate 5/5. Lesson: do not translate evidence/log strings stored on the server.
- The ClaimMVP Mentor session timed out at 300 s on a large translation batch → open a new session, split into small batches (session 17d5d38b → 6c26afe3; MyInsurance 55833a09).

### Still Fail (needs a decision or more work)
- TH-05 corner radius: 5–16 values per screen — need to fix a scale (card 16 / input 10 / button-pill 999) and then fix everything at once.
- TY-04 font size: Dashboard 17–19 sizes, ClaimDetail 18.
- TY-03 the native date field shows mm/dd/yyyy (browser locale, `<html lang="en-US">`) — needs a custom datepicker.
- DM-06 a white frame flashes on full page load (e.g. opening ClaimDetail).
- PF-02 Dashboard CLS 0.21 (> 0.1) — carousel/chart do not reserve space.
- LY-09 avatar menu lacks the switches "Chỉ xem phần đã có trên hệ thống" (EN: "Show only what is live in the system") and "Xem bản điện thoại" (EN: "View phone version") — waiting for Mike to decide whether to build them.
- RS-02 phone header uses ☰ + drawer, not yet the 2-row layout with a horizontally scrolling menu as in the mockup.
- Could not be checked: CP-04 (data loads < 1 s), CP-10 (the extension does not allow file upload), IX-06 (double-click would create a real claim). Not applicable: UX-01, RS-05, XA-05, PF-03 (need a real person/device), CP-06/IX-07/IX-11/UX-07 (avoid changing data or no suitable claim).
- Minor notes: the console Login was asked to have EN|VI + VI text (ClaimMVP 844) but could not be reviewed again because it requires signing out; after signing back in the console defaults to EN; the portal button "Bỏ qua, đến nội dung" (EN: "Skip to content") has the right colour on focus but still sits outside the frame (top −48 px).

### Impact on test data in round 3
- **CLM-2026-0541 was moved to "Chờ phê duyệt" (EN: "Awaiting approval")** when trying the "Trình duyệt" (EN: "Submit for approval") button before it had a confirmation step (this is bug IX-05 itself). No new claim was created. After the popup was added, tried on CLM-0545 and clicked Cancel (claim remains "Đang thẩm định" (EN: "Under assessment")).
- Signed out of the portal once (to view the Login page), signed back in with the one-tap card for both console and portal; console language reset to VI.
- ChainTests and the gate were re-run several times (only test results are recorded).

## Round 4 — per Mike's decisions, 17/10 15:00–15:50
Mike decided: **FC-07 keep the 20% co-payment** for routine prenatal visits (recorded as Pass); **yes, build the switches** in the avatar menu (LY-09). Corner-radius scale uses the proposal card 16 / input 10 / button-pill 999.

| Case | Result | Build |
|---|---|---|
| FC-07 | ✅ per business decision | — |
| LY-09 avatar menu | ✅ Switch sample account · switch "Chỉ xem phần đã có trên hệ thống" (EN: "Show only what is live in the system") (class `live-only` on `<html>`, key `bnk.liveOnly`; hides demo labels/buttons) · View phone version · Sign out | ClaimMVP 856–862, MyInsurance 283–285 |
| PF-02 layout shift | ✅ CLS 0.21 → 0 (layout frame has inline style navy background + content hidden until the theme loads) | ClaimMVP 854, MyInsurance 283 |
| TH-05 corner radius | ✅ 16/10/999/50% (minor leftovers PortalHome 14, PortalSupplement 20/12) | ClaimMVP 855–860, MyInsurance 284–286 |
| TY-04 font size | ✅ Dashboard 19 → 10, ClaimDetail 18 → 10, other screens 5–8 (CSS override by selector, tried live before sending) | ClaimMVP 857, MyInsurance 284 |
| TY-03 date field | ⚠️ console dd/MM/yyyy + VI calendar (overlay, original field hidden); portal still mm/dd/yyyy | ClaimMVP 858 |
| RS-02 phone header | ⚠️ console 2 complete rows (dark/light); portal already has 2 rows + scrolling menu but bell/avatar cannot yet be opened on a phone | ClaimMVP 858–862, MyInsurance after 289 |
| DM-06 white frame | ⚠️ no more page without CSS; a blank white frame of ~1–3 s remains on full page reload (before the app runs) | 854 / 283 |

**ClaimMVP rev 863:** ChainTests 46/46, gate 5/5 (17/10 15:45).

### Regressions in round 4 (handled)
- **MyInsurance 286: the OSUI DatePicker broke the Create claim screen** (toast "There was a problem…", 3 empty date fields, error `Cannot read properties of null (reading 'disabled')`) → 287 reverted to the original date field within minutes. No claim was created during that window from the test side.
- MyInsurance 289: the phone header disappeared entirely (CSS applied to the menu inside a closed drawer) → the next build builds `#bnkMobHdr` with a script as in the console.
- ClaimMVP 856–860: the Sign out button first overlapped the new row and then jumped up into the header → fixed with positioning CSS inside the menu panel (861).
- Lesson: Mentor cannot "find-and-replace" values in a ~600 KB theme; the reliable way is to measure on the page, write an override CSS with `:not(#a)…` for enough specificity, try inserting it live, then send.

### Round 4b — after the connector ran again (17:15–17:30)
- RS-02 portal ✅ (MyInsurance 291–292): the bell on a phone opens the Notifications screen; the avatar has initials and opens the drawer (name, role, the switch for showing only what is live, view phone version, Sign out); Vietnamese button labels; removed CSS 289 which broke the drawer.
- TY-03 portal ✅ (MyInsurance 293–294): the Create claim date fields show dd/mm/yyyy, Vietnamese calendar, future dates locked; the original date field keeps its binding/validation (only hidden). Build 293 did not run because the script ran before the form appeared → 294 uses MutationObserver. Tried picking 15/10 → original field 2026-10-15, no error; did not click Continue (no draft created).
- The "outsystems" connector on Mike's machine reported failed (Request timed out) from ~15:45 until Mike restarted it; "outsystems-stpl" is a different tenant (stpl-942), cannot be used as a substitute.

**Last running build: ClaimMVP rev 863, MyInsurance rev 294.** UI tracker 91 cases: 79 Pass · 1 Fail (DM-06 white frame on full page reload — platform limitation) · 3 Could not be checked · 8 Not applicable.

### Round 4c — cleaning up remaining UI bugs (17/10 17:45–18:10)
| Item | Result | Build |
|---|---|---|
| WorkList "Loại ngày" (EN: "Date type") | ✅ text field "Submitted" → dropdown "Ngày nộp" / "Submitted date", with aria-label | ClaimMVP 864–866 |
| "Cần bạn" (EN: "Needs you") in EN | ✅ "1,900,000 VND", "1 day" (VI keeps "1.900.000 ₫") | ClaimMVP 864 |
| English accessibility strings of the Dashboard chart | ✅ gone | ClaimMVP 864 |
| Terms ledger | ✅ English exclusion names carry the label "(EN)" when viewed in VI | ClaimMVP 864 |
| Console Login | ✅ EN\|VI, VI version has full text, tab title "Đăng nhập" (EN: "Sign in") (signed out to view, then signed back in with the Approver card) | ClaimMVP 864–867 |
| Portal corner radius | ✅ exactly 4 values on every scanned screen | MyInsurance 295–296 |
| Portal session expired | ✅ "Phiên đăng nhập đã hết hạn…" (EN: "Your session has expired…") + "Đăng nhập lại" (EN: "Sign in again") only for Security/NotRegistered errors; other errors still show "Không thể gửi hồ sơ" (EN: "Cannot submit the claim") (checked via Mentor, a real expired session not yet tried) | MyInsurance 296–297 |

**Last running build: ClaimMVP rev 867, MyInsurance rev 297.** ChainTests 46/46, gate 5/5 (18:08). No claims created/changed; signed out/in of the console 3 times to view Login.


## Round 5 — sort / filter / pagination / search buttons (17/10 18:20–20:15)
Clicked every sort/filter/pagination control in the 2 apps, fixed via Mentor. ClaimMVP 868→878, MyInsurance 298→301.

| Screen | Bug | Result |
|---|---|---|
| WorkList | Clicking a column header only sorted within each priority group | ✅ Sorts the whole list, ▲/▼ + aria-sort; default is still by priority |
| WorkList | The "20 / page" field was free text, typing 10 → shows all 35 | ✅ Replaced with "Số dòng: 10 · 20 · 50" (EN: "Rows: 10 · 20 · 50") |
| WorkList | On page 2, clicking a filter → "21–13 / 13", empty table | ✅ Every chip/dropdown/search/KPI card/sort goes back to page 1 |
| WorkList | Date range had no effect, default 01/08–03/09 | ✅ Filters by submitted date, default last 90 days, chip counts follow the date range |
| WorkList | The "Hiện hồ sơ kiểm thử" (EN: "Show test claims") switch would not turn on | ✅ On → 85, off → 35; chip counts follow the switch |
| WorkList | 4 KPI cards filtered out more than the number on the card (including closed claims) | ✅ 1/1/13/20 match the row counts |
| WorkList | Missing chip "Chờ bổ sung chứng từ" (EN: "Waiting for supplementary documents"); dropdown names differ from chips; "Chọn…" (EN: "Select…") | ✅ Chip added (2); names match; "Tất cả trạng thái" (EN: "All statuses") |
| Dashboard | Card "Đang thẩm định · bấm để mở" (EN: "Under assessment · click to open") opened an unfiltered WorkList | ✅ Opens WorkList?Status=… filtered |
| Header | Claim search box: Enter did nothing | ✅ Enter → WorkList?Q=… |
| ProviderLookup | Enter did not search | ✅ |
| AgentTrace | Chips only showed the 50 oldest claims, no "All", no keyboard use | ✅ Dropdown "Tất cả hồ sơ" (EN: "All claims") + all claims newest first, TST last |
| LimitLedgerView | Silently cut at 50 rows (An has 901 rows); EN placeholder | ✅ Pagination 50/page + "Hiển thị a–b / N bản ghi" (EN: "Showing a–b / N records"); "Chọn người được bảo hiểm…" (EN: "Select insured person…") |
| Policies | Placeholder "Select policy…" | ✅ "Chọn hợp đồng…" (EN: "Select policy…") |
| Portal PortalClaims | Oldest first, no filter, no pagination | ✅ Newest first; chips Tất cả/Đang xử lý/Cần bổ sung/Đã có kết quả (EN: All/In progress/Needs supplement/Has result) (29=14+2+13); 10 rows + "Xem thêm" (EN: "See more") + "Đang hiển thị X / Y" (EN: "Showing X / Y") |
| Portal PortalNotifications | 124 items in one go | ✅ 20 items + "Xem thêm" (EN: "See more") |

Incident in this round: rev 875 hung the browser on WorkList (the JS FixDropdownPlaceholders added by Mentor used a MutationObserver that re-triggered itself). Rev 876 removed the pagination JS, rev 877 removed the observer → hang gone (~20 minutes, 12:52–13:05Z).

Round 5b (17/10 21:20–21:35, ClaimMVP 879–880) — finishing the 4 small items:
- Screen-reader labels of the pagination buttons: "Trang trước / Trang N, đang xem / Trang sau" (EN: "Previous page / Page N, current / Next page") in WorkList and Limit ledger (VI); EN unchanged. Assigned after each data load, without a MutationObserver.
- EN mode: dropdown "Date type: Submitted date", the status dropdown has aria-label "Filter by status" (cause: JS read window.v7Lang before it was assigned → now reads cc.lang first).
- Sort by "Loại điều trị" (EN: "Treatment type") uses the label of the language in use (VI: Ngoại trú → Nha khoa → Nội trú → Tai nạn → Thai sản (EN: Outpatient → Dental → Inpatient → Accident → Maternity); EN: Accident → … → Outpatient).
- Dashboard: 3 status cards exclude test claims, numbers match the list they open (13 / 3 / 1).

**Last running build: ClaimMVP rev 880, MyInsurance rev 301.** ChainTests 46/46, gate 5/5 (21:35). No claims created/changed.

## Round 6 — English documents, foreign currency (17/10 22:00 – 18/10 00:40)
Question: claims entirely in English (foreign market), currency other than VND — can the system handle them. Generated 7 sample English document sets (inpatient/outpatient itemised bill, medical report, prescription, discharge summary, ER report, official receipt; footer "SAMPLE FOR SYSTEM TESTING ONLY"), submitted through the customer portal as real ones, read with BnK IDP.

**Short conclusion:** before the fix (rev 880) the system could read the English text but treated every number as VND → amounts wrong by 2–5 orders of magnitude. After the fix (ClaimMVP 881→900, MyInsurance 302) conversion is correct down to each line; what remains are business rules and IDP limitations.

### Changes
| Where | Change |
|---|---|
| Data | Entity CurrencyRate (Code, RateToVnd, RateDate, Source). Seeded 8 codes: VND 1 · USD 25.700 · EUR 28.679,9 · SGD 19.902 · THB 759,82 · JPY 159,42 · GBP 33.972,5 · AUD 17.735,4 — Vietcombank transfer buying rate 17/10/2026 (source chogia.vn). Claim: InvoiceCurrency (default VND), FxRateToVnd, FxRateDate, FxSource. ClaimLine: OriginalCurrency, OriginalUnitPrice, OriginalAmount |
| Number parsing | ParseAmountLocale: accepts 1,234.50 (UK/US) and 1.234,50 (European), strips currency symbols/text; VND keeps the old ParseVndAmount. ParseClaimAmount = original number × exchange rate, rounded to the dong |
| Expense lines | IdpTablesToClaimLines uses the claim's exchange rate, stores the original amount too; reads from all documents of the claim (previously only the first document) |
| Classification | InferServiceCategory adds English keywords (room & board, physician visit, X-ray, blood count, injection, consultation…) |
| Cross-checks | R4 accepts bed days = nights or nights + 1. R5 reads the final total by currency; a foreign-currency mismatch of exactly ×100 is treated as IDP losing the decimal point |
| AG-1 | Outpatient / outpatient accident / dental / outpatient maternity: the medical report (T2-03) can replace the examination record (T1-03) |
| Customer portal | Step 1 "Tạo hồ sơ" (EN: "Create claim") has a field "Loại tiền trên hoá đơn" (EN: "Invoice currency") (VND default + 7 foreign currencies), passed into Portal_CreateDraftClaim2/UpdateDraftClaim2 |
| Protection | CheckDecimalLoss: a foreign-currency claim in which every original amount is an integer and the largest number is ≥ 10.000 → CROSSCHECK_FAIL signal "Có thể IDP mất dấu thập phân…" (EN: "IDP may have lost the decimal point…"). Order at intake: build lines first, then cross-check R4/R5 (no more false DATE_IMPOSSIBLE) |
| Console | ClaimDetail: section "TIỀN HOÁ ĐƠN · 1 USD = 25,700.00 ₫ · Vietcombank 17/10/2026" (EN: "INVOICE CURRENCY · 1 USD = 25,700.00 ₫ · Vietcombank 17/10/2026"); the line table additionally shows "USD 760.00". Currency field + button "Đổi loại tiền & xử lý lại" (EN: "Change currency & reprocess") (ReprocessClaimCurrency: rebuilds lines, cross-checks R4/R5, AG-1, re-runs the chain). Tick box "Chứng từ mất dấu thập phân (÷100)" (EN: "Document lost decimal point (÷100)"). Blocked when the claim already has limit-ledger postings (postings are not deleted — BR-LEDGER-3) |

### Results before / after
| Claim | Document set | Before the fix (rev 880) | After the fix |
|---|---|---|---|
| 0584 | An · Lakeside Medical Centre Singapore · SGD 166.90 · outpatient | Claimed 337 ₫ (Amoxicillin 18.90 read as 189), R5 mismatch, missing examination record → Waiting for supplement | **3.321.644 ₫** (= 166,90 × 19.902), R5 matches, AG-1 documents complete → Under assessment. Chain waiting for the PROVIDER_UNMATCHED signal to be handled |
| 0585 | Hà · Harbourview Bangkok · USD 1,845.00 · inpatient 3 nights | 1.845 ₫, R5 mismatch, R4 DATE_IMPOSSIBLE, 5/6 lines "not a benefit", declined | **47.416.500 ₫**, R4/R5 gone, lines mapped correctly 100/100.1. Still declined at the waiting-period step — correct per the rule of policy card DEMO-W-001, not due to currency |
| 0586 | An · Zahnklinik München · EUR 410,00 (European style) · dental | 410 ₫, missing examination record | **11.758.758 ₫** (95,00 / 280,00 / 35,00 read correctly), Under assessment, proposed **1.503.037 ₫** (cap 300.1 1.000.000 ₫/visit + co-payment). The old R5 note remains (see below) |
| 0587 | Hà · Pacific Urgent Care Honolulu · USD 503.00 · outpatient accident | 503 ₫, receipt "Không nhận ra" (EN: "Not recognised"), missing examination record | **12.927.100 ₫**, R5 matches, Under assessment, PROVIDER_UNMATCHED awaiting handling |
| 0588 | An · Riverside Clinic TP.HCM · VND, English documents | 1.233.000 ₫ correct; missing examination record | Same as before for money; AG-1 complete → Under assessment |
| 0589 (new) | An · Harbourview Bangkok · USD 1,249.00 · inpatient 2 nights — submitted from the portal, USD chosen at step 1 | — | The portal immediately shows **32.099.300 ₫** (= 1.249 × 25.700). Proposed **17.067.300 ₫**: bed charge 19.532.000 ₫ cut to 1.500.000 ₫/day × 3 days; the 5 remaining lines paid in full 12.567.300 ₫. At intake a false DATE_IMPOSSIBLE was raised (R4 ran before the lines existed) — order fixed in rev 896 |
| 0590 (new) | An · Klinikum am Markt München · EUR 927,50 · inpatient 2 nights — submitted from the portal, EUR chosen | — | This time IDP dropped the comma in **every** number (620,00 → 62000) → the portal shows 2.660.060.725 ₫ (100× too much). The system raised the warning "Có thể IDP mất dấu thập phân" (EN: "IDP may have lost the decimal point"); the officer ticked ÷100 → **26.600.608 ₫** (17.781.538 + 5.449.181 + 1.218.896 + 2.150.993), warning closed automatically. No more DATE_IMPOSSIBLE at intake |

ChainTests after the fix: 46/46, gate 5/5 (23:45 on rev 892; 00:35 on rev 900).

### Still open
| # | Issue | Type |
|---|---|---|
| 1 | IDP (LLM) reads European-style numbers unreliably: 0586 only lost the comma on the total line ("410,00" → 41000, R5 reports a mismatch), 0590 lost it on every line. There is a warning + ÷100 button, but detection is threshold-based so genuinely large integers (e.g. EUR 12.000) are also warned, while small numbers that lose the comma (e.g. 95,00 → 9500) are not. The old R5 note on 0586 remains because 0586 is already posted and cannot be re-run | IDP |
| 2 | International hospitals charge per night (2 nights), the adjudication chain counts the bed cap by days inclusive of both ends (3 days) → cap 4.500.000 instead of 3.000.000 ₫ | Mike to decide |
| 3 | The exchange rate is fixed for one day (17/10/2026), with no updates and no decision on whether to use the rate of the treatment date or the payment date | Mike to decide |
| 4 | Foreign facilities are always PROVIDER_UNMATCHED (not in the catalogue) → the chain waits for an officer | Catalogue data |
| 5 | IDP: Official receipt not recognised; Medical report classified as T2-03; service names stick to the unit column ("Scaling and polishing Session") | IDP |
| 6 | The customer portal shows only VND (does not yet show the original USD/EUR amount); AG-5 letter is Vietnamese only | To do next |
| 7 | Changing currency is only possible before the chain posts to the ledger; afterwards it is blocked with a stated reason | Design |

### Impact on test data
- Newly created claims 0584–0590 (7 claims, 21 sample document images, uses IDP quota).
- 0584–0590: expense lines deleted and rebuilt (0590 three times); status of 0584/0586/0587/0588 from "Chờ bổ sung" (EN: "Waiting for supplement") → "Đang thẩm định" (EN: "Under assessment"); old CROSSCHECK_FAIL/DATE_IMPOSSIBLE signals closed.
- 0586 has 8 provisional limit-ledger postings for An (MB-2026-0001); 0585, 0589 ran the chain so may also have them. 0590 not yet posted (waiting for PROVIDER_UNMATCHED).
- Added 8 CurrencyRate rows.

**Last running build: ClaimMVP rev 900, MyInsurance rev 302.**

## Round 7 — international demo mode (USD), 18/10 08:30–10:05
> Replaced by Round 8: international package / John Carter dropped, claims 0584–0592 archived.
Mike chose: display USD + international package, with VND still used internally (ChainTests unchanged). ClaimMVP 901→910, MyInsurance 303→305.

### How to use in a demo
Console → account menu (top right) → row **"Tiền tệ demo": VN ₫ | INTL $** (EN: "Demo currency").
- **INTL $**: every amount in the console and customer portal is shown in $ (USD rate 25.700), the one-tap customer portal sign-in becomes **John Carter** (card INT-2026-0001, policy HC-INT-2026-0001), the "Loại tiền trên hoá đơn" (EN: "Invoice currency") field defaults to USD.
- **VN ₫**: back to the original (Nguyễn Văn An, ₫). Run ChainTests in this mode (the test log matches ₫ strings).

### Changes
| Where | Change |
|---|---|
| Data | Entity DemoSetting (SettingKey/SettingValue). GetDemoMode / SetDemoMode / Portal_GetDisplaySettings |
| ClaimMVP display | Variables DisplayCurrency, UsdRate; FormatMoney, FormatVND (client), FormatVND_Server, FormatVND (service) return "$1,234.56" when INTL |
| MyInsurance display | FormatAmount, FormatVND, FormatNotifText follow DisplayCurrency (fetched from ClaimMVP when the page opens); the greeting uses the first name when INTL ("Xin chào, John!" (EN: "Hello, John!")) |
| International package | Seed_IntlDemo (runs automatically when INTL is switched on, runs once): policy HC-INT-2026-0001 (package HC-STD, 01/01–31/12/2026), insured person John Carter; benefits copied from An's policy then rounded to 10 USD (e.g. bed cap $60/day, visit cap $60/visit, outpatient $780, inpatient $3,890, dental $190) |
| Customer portal | Portal_GetCurrentMember(2): returns John Carter when INTL |
| Other fixes | ReprocessClaimCurrency updates the claimed total again (TotalClaimed). RaiseFraudSignal strips NUL characters (AG-4 once sent 0x00 causing a DB error, the claim stuck at "AG-4 đang chạy" (EN: "AG-4 running")) |

### Testing
| Claim | Content | Result |
|---|---|---|
| 0591 | John Carter · outpatient · Harbourview Bangkok · USD 109.80 (itemised bill, medical report, prescription) | Portal: total $109.80, lines $55.00 / $32.00 / $16.80 / $6.00. Console: over cap 200.1 ($87.00 vs $60.00/visit, −$27.00), co-payment 20% (−$16.56), **proposed $66.24**. Itemised bill attempts 1–2 IDP "Không nhận ra" (EN: "Not recognised"), attempt 3 (uploaded by document type) recognised correctly |
| 0592 | John Carter · inpatient 2 nights · USD 1,249.00 | IDP lost the decimal point (760.00 → 76000) → portal shows $124,900; the warning "Có thể IDP mất dấu thập phân" (EN: "IDP may have lost the decimal point") fired correctly; tick ÷100 → $1,249.00. AG-4 stuck due to the NUL bug, fixed in rev 910 then re-run: **proposed $669.00** (bed $760 cut to $60/day × 3 days = $180). Portal: "Tổng chi phí $1,249.00 · Dự kiến chi trả $669.00" (EN: "Total cost $1,249.00 · Expected payout $669.00") |

ChainTests 46/46, gate 5/5 (18/10 10:00, rev 910, VN mode).

### Notes / still open
- Old history (agent logs, stored R5 results) still shows ₫ — only figures computed at display time changed to $.
- The auto-approval threshold 2.000.000 ₫ shows as $77.82 (odd number) — if a round number is needed, change the threshold in ODC Portal.
- The bed cap counts 3 days for 2 nights (as in item 2 of Round 6) — waiting for Mike to decide.
- IDP still sometimes loses the decimal point or fails to recognise an itemised bill; during a demo, if this happens: tick ÷100 + "Đổi loại tiền & xử lý lại" (EN: "Change currency & reprocess"), or upload the itemised bill into the field "Theo loại chứng từ" (EN: "By document type").
- Foreign facilities are always PROVIDER_UNMATCHED → the officer uses "Gỡ kèm lý do" (EN: "Clear with reason") (cleared on 0592 with the reason "Test demo quốc tế…" (EN: "International demo test…")).
- Portal: switch to EN with the "Tiếng Anh" (EN: "English") button in the account menu (the EN version in USD mode has not been fully checked).

### Impact on test data
- Added 1 DemoSetting row (currently left at **INTL**), 1 Policy + 1 Member (John Carter) + the PolicyBenefitSnapshot set of the new policy.
- Created claims 0591, 0592 (8 sample document images, including 2 unrecognised itemised-bill versions in 0591); 0592 lines rebuilt 2 times, the PROVIDER_UNMATCHED signal cleared with a reason; 0591/0592 may already have provisional postings on policy HC-INT.

**Last running build: ClaimMVP rev 910, MyInsurance rev 305. Demo mode on: INTL $.**

## Round 8 — international hospitals in Vietnam, English/USD invoices (18/10 10:10–12:30)
Mike changed the scope: **drop the foreign market**. The hospital is still a Vietnamese hospital in the IDP catalogue (table 322); only the invoice is in English and in USD because international hospitals charge foreign customers. Added a VND ↔ USD toggle for foreign customers. Wrong test data is to be cleaned up. ClaimMVP 911→927, MyInsurance 306→309.

### Changes
| Where | Change |
|---|---|
| Drop the international package | The customer portal is always Nguyễn Văn An (John Carter removed from one-tap sign-in). DemoSetting is now only a **display** setting for VND/USD, it does not change the policy |
| VND ↔ USD toggle | Console: account menu → "Hiển thị tiền" (EN: "Display currency") VND \| USD (system-wide). Customer portal: **VND \| USD** switch on the menu bar next to the light/dark button, per user. Internally still computed in VND, USD rate 25.700 |
| R4 (admission/discharge date) | **Bug fix**: compared the later document's discharge date with the concatenated string ("13/10/2026 \| 13/10/2026") so it always mismatched → every inpatient claim with ≥ 2 documents stating the discharge date was blocked from auto-approval by DATE_IMPOSSIBLE (HIGH). Now compares with the first value, ignoring empty cells |
| R3 (facility name) | International hospitals print the English and Vietnamese names on the same document set ("VINMEC CENTRAL PARK INTERNATIONAL HOSPITAL" / "BỆNH VIỆN ĐA KHOA QUỐC TẾ VINMEC CENTRAL PARK"). New function FacilityCoreName strips diacritics, Đ→D, removes generic words (Bệnh viện, Đa khoa, Quốc tế, Hospital, International, Clinic…) then compares the core part; equal or containing each other is a match |
| Expense line names | IDP often glues the unit column to the name ("Room & board - standard room Day"). CleanLineDescription strips the trailing unit when building lines; the button "Làm sạch tên dòng chi phí" (EN: "Clean up expense line names") for existing lines: scanned 310 lines of non-archived claims, fixed 31 lines (6 lines of 0599, 25 lines of old test claims) |
| Console ClaimDetail | The label "TIỀN HOÁ ĐƠN" has an EN version "INVOICE CURRENCY"; the exchange-rate source line no longer repeats the date |
| Customer portal step 2 | Outpatient / outpatient accident / dental / outpatient maternity: the medical report (T2-03) counts as the examination record (T1-03) — "Examination record — Required/Missing" disappears once a medical report is uploaded (same as AG-1) |
| Archiving test claims | ArchiveClaimsByNumbers now **releases provisional postings too** (Status = REVERSED, not deleted — BR-LEDGER-3), previously it only reversed FINAL postings. The button "Giải phóng bút toán tạm của hồ sơ đã lưu trữ" (EN: "Release provisional postings of archived claims") runs for all archived claims |
| Decimal-point loss | AutoFixDecimalLoss: a foreign-currency claim in which every original amount is an integer and the largest number is ≥ 10.000 → automatically divided by 100 when building lines, leaving a LOW note for the officer (previously ÷100 had to be ticked manually) |
| Provider lookup (LookupProvider) | Added a core-name match branch (CATALOGUE_CORE) for when the customer types a short name. **Not yet working** on real claims (see Still open) |

### Test data cleanup
| Claim | Reason | Action |
|---|---|---|
| 0584–0590 | Foreign hospital document sets (SGD/EUR/THB, PROVIDER_UNMATCHED) — outside the new scope | Archived |
| 0591, 0592 | John Carter (international package) — dropped | Archived |
| 0594 | IDP lost the decimal point (×100), cannot be changed back because already posted | Archived, re-submitted as 0595 |
| 0595 | False R3/R4 alerts (2 signals HIGH/MEDIUM) — bug fixed | Archived |
| 0596, 0597, 0598 | Tried entering the short facility name "Vinmec Central Park" → "Not in catalogue" | Archived |
Archived = hidden from every list, restorable with "Khôi phục" (EN: "Restore"). The provisional postings of these claims have been moved to REVERSED so An's limit is no longer wrongly deducted.

### Sample claims used in the demo
| Claim | Content | Result |
|---|---|---|
| **0593** | An · **Bệnh viện Quốc tế City** (EN: City International Hospital) · outpatient 16/10/2026 · influenza A (J10.1) · English invoice **USD 110.00** (consultation $45, flu test $28, Oseltamivir 10 × $3.20, Paracetamol 20 × $0.25) | In-network. 2.827.000 ₫; proposed **1.960.720 ₫** (visit cap 1.500.000 ₫/visit cut 376.100; co-payment 490.180). No open work |
| **0599** | An · **Bệnh viện Đa khoa Quốc tế Vinmec Central Park** (EN: Vinmec Central Park International General Hospital) · inpatient 2 nights 11–13/10/2026 · pneumonia (J18.9) · English invoice **USD 528.00** | In-network, AG-4 no signal, R1–R5, R7 pass. 13.569.600 ₫; proposed **10.359.600 ₫** (bed charge $300 = 7.710.000 ₫ cut to 1.500.000 ₫/day × 3 days). Remaining: 4 low-confidence IDP lines awaiting officer confirmation, 1 period checkpoint |
The customer portal in USD mode shows "$528.00", lines $300.00 / $120.00 / $40.00 / $18.00 / $38.00 / $12.00; clicking VND switches back to ₫.

ChainTests 46/46, gate 5/5 (18/10 12:28, rev 927, VND display).

### Still open
| # | Issue | Type |
|---|---|---|
| 1 | A customer who types a short/English name ("Vinmec Central Park") instead of choosing from the suggestions → claim "Not in catalogue" + PROVIDER_UNMATCHED, even though looking up table 322 finds the right facility (79525, score 91.17, CATALOGUE_PARTIAL). The CATALOGUE_CORE branch was added but retrying still does not take effect. In a demo: type "Quốc tế" or "Vinmec" then **choose the Vietnamese name in the suggestion list** | To do next |
| 2 | The portal's facility suggestions come from facility names already in claims, not from table 322 → hospitals that have never had a claim do not appear in the suggestions | Design |
| 3 | Bed cap counts 3 days for 2 nights (Round 6 item 2) | Mike to decide |
| 4 | IDP reads the NaCl quantity as 1 (the invoice says 4) — the amount is still correct | IDP |
| 5 | Old test claims 0535–0583 still exist, waiting for Mike to decide whether to archive them | Mike to decide |
| 6 | The name-cleaning button fixed 25 lines on old test claims (removed trailing unit words such as "Viên", "Lần", "Day") — not yet reviewed line by line | Re-check |

### Impact on test data
- Archived 0584–0592, 0594–0598 (14 claims); their provisional postings moved to REVERSED.
- Created 0593–0599 (7 claims, 21 English sample document images, uses IDP quota); 0593 and 0599 are in use.
- The John Carter policy/insured person remains in the DB but is no longer used anywhere.
- DemoSetting is currently set to **VN** (₫ display). An's customer portal: the switch is currently on VND.
- 31 expense lines renamed (trailing unit word removed), money/quantity unchanged.
- Administration has a new panel "Dọn hồ sơ kiểm thử" (EN: "Clean up test claims") (Archive / Release provisional postings / Clean line names / Restore).

**Last running build: ClaimMVP rev 927, MyInsurance rev 309.**

## Not done
- Switch FP-03 to Portal_UpdateDraftClaim2 once the tenant index accepts it (the index is stuck at ~16/10 16:41Z).
- Not yet tested: XML invoice (R6), dental waiting period, requester ≠ self.
- Foreign currency: show the original amount on the customer portal, English AG-5 letter, periodic exchange-rate updates, decide the rule for bed days by night (see Round 6 · Still open).
