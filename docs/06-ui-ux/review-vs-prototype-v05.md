# Overall review — ClaimMVP vs `Prototype_Insurance_BnK_v0.5.html` (v6.1 full prototype)

Baseline captured at rev 32 (2026-09-18); **this version reflects the state verified in the browser at rev 47 (2026-09-19)** after the D3–D6 console-fidelity batches. Reference renders: `prototype-v05/*.jpg`. Every ✅ below was checked in the logged-in Chrome session (screenshot + DOM probe), not taken from Mentor's reports.

Legend: ✅ matches · 🟡 present but differs · ❌ missing / broken

## A. Chrome (shell)

| Area | Prototype | App rev 46 | Status |
|---|---|---|---|
| Login | Two-tone teal wave + Staff/Customer toggle + "Log in as sample user" | Teal gradient + form; no Staff/Customer toggle, no sample-user button | 🟡 |
| Sidebar | 264 px dark-teal gradient, brand, nav with F-codes, badges, footer avatar + name + role + sign-out | Dark-teal ✅, brand ✅, nav incl. new "Technical trace F70" ✅, badges ✅, footer "NA · Nguyễn Hoài An · Approver" + sign-out ✅, single active item ✅ | ✅ |
| Top bar | ☰ · title + sub · bot button with badge · pill search · VI · moon · bell | ☰ · 22 px Manrope title + 12.5 px sub · dark bot button · 250 px pill search · VI · moon · bell-with-dot ✅; portal shows "+ New claim" pill instead of the bot ✅ | ✅ |
| AI panel "Claim activity" | Fixed right 360 px dark teal; Steps that ran; Proposals awaiting decision; quick-question chips; ask input | Opens from the bot button without freezing ✅; dark-teal 360 px ✅; Steps that ran with done/blocked pills for the open claim ✅; chips + ask input ✅; "Proposals awaiting decision" card renders "No open proposals" on claim A although REC-CLIN-04 is pending | 🟡 |
| Inline notes / toasts | soft `.note` with 4 px left border; bottom toast | Global soft Alert rule (info/warning/error variants) ✅; OS UI Notification ✅ | ✅ |
| Presenter mode (agents / kpi / pitch / osmap) | hand-over slides | Not built (by plan) | — |

## B. Console screens

| Screen | Status | Notes |
|---|---|---|
| Work list (F42) | ✅ | Solid pills, mono claim no., age chips, ghost "Take unassigned"/"Export"; second sub-state pill not built |
| Dashboard (F54) | ✅ | Tinted KPI cards with icon squares + money subs ✅, 15 px section titles ✅, weekly chart ✅, "View all →" ✅ |
| Ops queue (F53) | ✅ | 1 incident, Waiting pill, Handle / Skip ghost pills, soft note |
| Policies (F60) | ✅ | "Insured with claims" names blank |
| Term register (F65) | ✅ | — |
| Checkpoint queue (F67) | ✅ | REC-CLIN-04 / T-07 row + Decide pill (regressed at rev 41, fixed rev 44); warning note soft |
| Limit ledger (F48) | 🟡 | counters map plan benefits (200 ↔ OUTPATIENT) with 0 used |
| Administration (F55) | ✅ | Providers / thresholds / roles; "Re-run demo seed" button (extra, needed for demos) |
| Technical trace (AgentRun) | ✅ | Screen `AgentTrace` rebuilt: claim chips, Time / Agent→Stage / Kind pills (system·document·check) / Event / State (done·blocked) — matches the prototype |

## C. Claim detail (F45–F53)

| Tab | Status | Notes |
|---|---|---|
| Header | ✅ | breadcrumb, 22 px name, solid status pill, KV strip, StepLine 26 px green ✓ / dark current (step 3 for UnderReview), underline tabs with amber count badge |
| General | ✅ | — |
| Documents | ✅ | solid Required/Expected/Received pills, Pages, confidence, ghost "View" + "Request information" / "Waive expected" |
| Extraction & costs | ✅ | To-do bar "Decide the proposal on line 4…", receipt card with highlighted line 4, cat chips, Decision "Proposal: out of cover", amber Conf pills < 70 %, rec card "Proposal pending · REC-CLIN-04 · 62 %" with **Confirm / Reject → reason popup → DecideRecommendation**; low-confidence bar + "Acknowledge lines" on claim C (rev 47: moved above the columns per Mentor; row tint not verified) |
| Events & cover | ✅ | "Add event" not built |
| Calculation & ledger | 🟡 | table narrow, no rule descriptions; ledger card OK |
| Settlement plan | 🟡 | rows grouped by benefit with names + limits + 20 % co-pay ✅, Payment KV one line each + Verified pill ✅, checks amber while a proposal is pending ✅, Export ghost ✅; Claimed per row still mis-split (ROOM 1.450.000 / OTHER 350.000 instead of 600.000 / 1.200.000 — BuildBenefitRows grouping bug, 3 attempts); check icon shows "!" + "✓" together |
| Approval & payment | ✅ | condition 1 ✕ (co-pay cut), condition 2 "1 proposal awaiting a human" ✕, 3–4 ✓; approver actions hidden for the assessor (four-eyes) |
| History & log | ✅ | — |

## D. Flows

| Flow | App rev 46 | Status |
|---|---|---|
| Decide proposal (Confirm / Reject, mandatory reason) | popup (≥ 10 chars) → DecideRecommendation → refresh + toast; verified end-to-end at rev 36 (a test Confirm wrote NOTREL on line 4; "Re-run demo seed" restores) | ✅ |
| Acknowledge low-confidence lines (claim C) | bar + button → AcknowledgeLowConfidenceLines | ✅ (button not pressed in the review, data kept) |
| Submit for approval / Approve / Pay | buttons present, disabled while a proposal is pending; approver path not exercised | 🟡 |
| Customer: new claim wizard, send documents | ✅ (Batch 6) | ✅ |
| Assistant Q&A chips | Notification stubs (by design) | 🟡 |

## E. Verdict (rev 47)
The console now reproduces the prototype's modern skin and structure end to end: shell (sidebar, top bar, assistant panel), work list, dashboard, all reference screens, technical trace, and the eight claim-detail tabs including the real decision flow. Remaining deltas are small: settlement Claimed split per benefit + overlapping check glyph, low-confidence row tint, assistant "proposals" card on claim A, login Staff/Customer toggle, ledger counter mapping, calc-table descriptions, portal progress bars (added at rev 47, not re-verified). No new entities were created; money is still written only by RunAdjudicationChain; every screen keeps an explicit role (the stray `SeedAdmin` screen Mentor added at rev 44 was deleted at rev 45).

### Mentor batches in this pass
D3b/D3c (rev 34–35 header/stepline/footer) · D4/D4b/D4c (rev 36–39 tabs + decide popup) · D5a/D5b/D5c (rev 40–42 dashboard, IsRecPending, trace, assistant) · D6a/D6b/D6c/D6d (rev 43–47 checkpoints, claim C seed, settlement grouping, portal polish). Specs: `~/.claude/cache/outsystems-design-to-app/ClaimMVP/mentor-console*.md`.
