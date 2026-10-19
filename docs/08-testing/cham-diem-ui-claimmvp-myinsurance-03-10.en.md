# UI scoring — ClaimMVP (console) & MyInsurance (portal) · 03/10/2026

> English translation for reviewers. Vietnamese original: [cham-diem-ui-claimmvp-myinsurance-03-10.md](cham-diem-ui-claimmvp-myinsurance-03-10.md).

Source: viewed live on bnk-dev (ClaimMVP rev 243+, MyInsurance rev 84), as Approver and as the customer Nguyễn Văn An. Scores are a subjective assessment on the scale below, through the eyes of a hackathon judge.

## Scale of 100
| Criterion | Max | ClaimMVP | MyInsurance |
|---|---|---|---|
| Aesthetics & consistency | 20 | 16 | 17 |
| Information architecture & navigation | 15 | 12 | 13 |
| Data storytelling (numbers, charts, before/after) | 15 | 8 | 10 |
| Visible & explainable AI/agent | 15 | 6 | 10 |
| Language & wording | 10 | 5 | 9 |
| Interaction, states, feedback | 10 | 8 | 7 |
| Demo data hygiene | 10 | 6 | 7 |
| Responsive & readability | 5 | 3 | 2 |
| **Total** | **100** | **64** | **75** |

## ClaimMVP — strengths
- Good-looking frame, same family as the portal: teal sidebar, rounded cards, coloured badges, claim header with a 5-step stepper + prominent "Proposed payable".
- Extraction & costs tab: source document on the left, extraction grid with confidence (97%/94%) on the right, flagged lines highlighted — exactly what judges want to see.
- Run #3 / Run #4 comparison using cards; the banner "To do: Decide the proposal on line 4" makes the required action clear.

## ClaimMVP — weaknesses
1. **Agent is hidden**: the AG-2 panel sits at the bottom of the Calculation & ledger tab, with Vietnamese text in an English UI ("Cuộc hội thoại mới" (EN: "New conversation"), "Tính và giải thích" (EN: "Calculate and explain")). Judges will not scroll that far.
2. **Calculation tab = technical log**: "TERM-HC-002 — KHÔNG_THUỘC_PHẠM_VI" (EN: "NOT_IN_SCOPE"), "[BR-WAIT-1-5]", "200.1", "posting AMOUNT"; step names in Vietnamese in an English UI; the Result column repeats 3.128.000 without highlighting which step changes the money.
3. **Internal codes exposed**: status chips "ExtractionError", "UnderReview", "PendingApproval" next to "Pending info", "Payment in progress"; "Payment (F58)"; the weekly chart is labelled "T1…T9".
4. **Thin dashboard**: 3 of 4 KPIs are 0 ("Pending approval 0", "Awaiting payment 0", "Auto-approved today 0"); the weekly chart has one bar of height 1 per week.
5. **Technical trace**: ~70 claim chips including `TST-T-CH…` fixtures and 0034–0041; the event table looks like a log, and the seeded agent names AG-0…AG-6 do not match the real agents (AG-2, Customer Assistant).
6. Administration is full of dev buttons (Run IdpPing, Delete 9 orphan keys…).

## MyInsurance — strengths
- Clean, easy to understand, fully bilingual, polite wording ("Quý khách" (EN: "Dear customer")).
- Home has 3 meaningful KPIs (insured persons, remaining limit, claims in progress); a 4-state stepper for the customer.
- The chatbot is a real agent and answers correctly from the data (Atorvastatin "đang thẩm định, chưa phải kết quả cuối" (EN: "under assessment, not the final result")), without making things up.
- Notifications are read from data, with an unread badge.

## MyInsurance — weaknesses
1. **Missing the "result" moment**: An has no paid claim → the block "Vì sao Quý khách tự chịu" (EN: "Why you bear this cost") (the most valuable screen for the customer) does not appear.
2. **Chat waits ~40 s** showing only "Đang xem hồ sơ…" (EN: "Looking at your claim…"); the chat button is an unlabelled round icon, hard to spot.
3. **Mobile 390 px not checked** — a customer portal that looks good only on desktop loses points.
4. The lower right half of Home is empty; with only 2 claims it looks "hollow".
5. CLM-0001 (outpatient) is still asked for a Discharge certificate (waiting for a ClaimMVP session to fix the data).

## Improvements to make an impression — prioritised by impact / effort
| # | Task | App | Impact | Effort |
|---|---|---|---|---|
| 1 | **"What the AI did for this claim" card** at the top of Claim detail: timeline Read documents (9 templates, % of fields not typed) → Clinical check flags line 4 → 14-step chain → Gate 4/4 → Result; a "Ask AG-2" button opens the panel right there | ClaimMVP | Very high | 2–3 Mentor turns |
| 2 | **Money waterfall chart** at the top of the Calculation tab: 3.128.000 claimed → −610.000 over cap → −168.000 unrelated to diagnosis → −470.000 co-payment → 1.880.000. The 14-step log is collapsed into "Technical details" | ClaimMVP (+ reused in portal) | Very high | 2 turns |
| 3 | **One paid claim for An** (run CLM-0001 to PAID before the pitch, or seed a new claim) so the portal shows the result + "Vì sao Quý khách tự chịu" (EN: "Why you bear this cost") + notification 005 | Both | Very high | 1 turn + manual action |
| 4 | Standardise console status labels (Read failed / Under review / Pending approval…), remove "F58", "T1…T9" → "W36…W40", step names + step details in the selected language | ClaimMVP | High | 2 turns |
| 5 | Technical trace: filter out `TST-`/`FX-`, show only real agents; add an **agent diagram** (Portal → IDP → AG-2 → Gate → Customer Assistant) highlighting the running step | ClaimMVP | High | 2 turns |
| 6 | Dashboard: seed 6–8 weeks of data or switch KPIs to real measures (average processing time, % auto-approved, % of fields not typed, minutes saved per claim) | ClaimMVP | High | 1–2 turns |
| 7 | Chat: a labelled button "Hỏi trợ lý" (EN: "Ask the assistant"), show step-by-step progress while waiting ("Đang đọc hồ sơ… Đang kiểm tra hạn mức…" (EN: "Reading the claim… Checking limits…")); in the pitch use 3 pre-verified suggested questions | MyInsurance | Medium–high | 1 turn |
| 8 | Check & fix 390 px, present the portal in a phone frame | MyInsurance | Medium | 1–2 turns |
| 9 | Group the dev buttons in Administration into a collapsed "Developer tools" section | ClaimMVP | Low | 1 turn |

## Suggested 3-minute "wow" script
1. Phone (portal): the customer submits a claim → "Đang xử lý chứng từ" (EN: "Processing documents") + notification.
2. Console: "Reading…/Replay" chip → extraction grid with confidence, the Atorvastatin line is flagged.
3. The officer confirms the recommendation → the **waterfall** re-runs 2.000.000 → 1.880.000 → gate 4/4.
4. Ask AG-2 "Vì sao giảm 120.000?" (EN: "Why is it reduced by 120.000?") → the answer cites the steps.
5. Back to the phone: result + "Vì sao Quý khách tự chịu" (EN: "Why you bear this cost") + ask the chatbot in Vietnamese.
