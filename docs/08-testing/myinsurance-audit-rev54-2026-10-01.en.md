# MyInsurance — runtime audit rev 54 (01/10/2026)

> English translation for reviewers. Vietnamese original: [myinsurance-audit-rev54-2026-10-01.md](myinsurance-audit-rev54-2026-10-01.md).

**App:** MyInsurance `<redacted-app-key>` · rev 54 (published 17:07 VN time 01/10; rev 52–54 edited by Mike).
**Method:** Chrome already signed in as the customer (Nguyễn Văn An, HC-STD), read `innerText` of 7 screens + 3 claim details, grepped for internal codes with a regex (with a positive control), tried the chatbox with 2 suggested questions. Nothing that writes data was clicked.
**Note:** MyInsurance's Context Service still indexes 19/09 → reading the model via MCP does not reflect rev 54; trust only the runtime.

## Passed

- Clean grep on Home, Benefits, Claims, NewClaim, Supplement, Notifications, ClaimDetail: **0** UPPER_SNAKE codes, 0 internal status/document/lossType codes, 0 `TST-/FX-`, 0 `agent/IDP/AI/template id`, 0 `null/undefined/NaN`.
- 0 console errors.
- CLM-0001 line total = 3.128.000 ₫ matches "Amount you claimed".

## Bugs — by priority before the pitch

| # | Screen | Bug | Severity |
|---|---|---|---|
| 1 | Login | The customer portal shows **"Health Insurance · Backoffice · Claim Console"** (left over from cloning ClaimMVP) | 🔴 |
| 2 | Supplement | CLM-0001 (outpatient) is asked for a **Discharge certificate — Mandatory**; the "Document type" dropdown **has no** Discharge paper type → the customer cannot submit what is being requested | 🔴 |
| 3 | Demo data | 8 junk claims **CLM-0034…0041** (Medlatec, 20/09, 6 Maternity + 2 Dental) occupy Home/Claims/Notifications; Maternity for a male insured person "Nguyễn Văn An" | 🔴 |
| 4 | Chatbox | Canned answers, **wrong business logic**: explains "Atorvastatin" as exceeding the per-visit cap (in fact scenario A is NOTREL – not related to the diagnosis), mentions an "Explanation of Benefits" that does not exist; the limit question does not read the figure, only tells the user to open Benefits | 🔴 |
| 5 | ClaimDetail CLM-0004 | "Amount you claimed" **2.400.000 ₫** but the 3 lines add up to only **800.000 ₫**; status is "Processing your documents" yet cost lines are already shown | 🟡 |
| 6 | Benefits | Dental "Applies from **01/01/2026**" while the footnote says **dental has a 90-day waiting period** | 🟡 |
| 7 | NewClaim | Red errors appear as soon as the screen opens ("Loss type is required", "Treated-from date is required") before the customer has entered anything; Provider pre-filled with Medlatec; Loss type includes Maternity for a male insured person | 🟡 |
| 8 | Claims | A design note leaks into the UI: "Only four states for you… Internal technical incidents never appear as a separate label." | 🟡 |
| 9 | Whole app | Step 2 has 3 names: "Processing your documents" (badge) / "Processing documents" (caption) / "Documents read" (stepper). The "Processing your documents" badge is red like an error | 🟢 |
| 10 | ClaimDetail | The date next to the claim number (10/06/2026, 15/01/2026) has no label — easily misread as the submission date | 🟢 |
| 11 | Bilingual | The VI/EN button is gone from the top bar (only dark mode + bell). The UI is in English while benefit/expense names are in Vietnamese → mixed languages. `build-06` §8.5 requires a **pure-VI portal** | 🟡 — needs PO decision |
| 12 | Brand | Subtitle "Digital Health Insurance Platform · MVP 1" | 🟢 |
| 13 | ClaimDetail CLM-0038 | The line "Items appear once your documents are read" sits **above** the "Item by item" heading | 🟢 |

## Could not be checked

- **Wizard steps 2–4** — clicking Continue would create a real claim (more junk). Check after the data is cleaned up.
- **The block "Vì sao Quý khách tự chịu"** (EN: "Why you bear this cost") — this member has no claim in Result status. Claim A needs to be run through the console to PAID, then viewed.
- **390 px** — the Chrome window cannot be resized (it is maximised).
- **Dark mode.**
- The login session expired midway (back to `/MyInsurance/Login`).
