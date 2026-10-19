# Runtime UI Audit #3 — ClaimMVP · rev 55 · 2026-09-19 — fidelity vs `Prototype_Insurance_BnK_v0.5.html`

**Result: 74 % — Delightful** (numerator 52 / denominator 70, 0 N/A). Trend: rev 12 57 % → rev 30 69 % → rev 47 71 % → **rev 55 74 %**.
Method: the prototype was served locally (`localhost:8765/_proto.html`) and driven through its own UI (Staff / Customer "Log in as sample user"); the app was captured in the same Chrome session at 1568×772. Pairs in `shots-rev55/` (`proto-*` vs `app-*`). Read-only.

## A. Rubric score

| # | Criterion | Tier | W | Evidence |
|---|---|---|---|---|
| C1 | Theme & Styling | Delightful (3) | 1× | Same skin as the prototype on every surface (dark-teal sidebar, 70 px white header, pill buttons `#0c3036`, solid pills, tinted KPIs, soft notes, Manrope titles). Residual drift: portal wizard is a dot stepper (prototype: 4 step cards). |
| C2 | Component Reuse | Delightful (3) | 1× | OS UI Layout/Menu, TableRecords, Tabs, Wizard, Popup, Sidebar, ProgressBar, BlankSlate, Notification; one button language. |
| C3 | Component Fit | Delightful (3) | 1× | StepLine states, decision popup, receipt + extraction table, trace table, grouped settlement, low-confidence flow. |
| C4 | Colour Contrast | Delightful (3) | 1× | Unchanged (≥ 4.5:1 on all pill/text pairs). |
| C5 | Tap / Click Targets | Acceptable (2) | 1× | WorkList **94 %** ≥ 44 px (rev 47: 31 %); remaining: ☰ 36 px, one table link. |
| C6 | Keyboard | Acceptable (2) | 1× | `:focus-visible` rule present, ring unverified. |
| C7 | Grid Adherence | Delightful (3) | 1× | Matches the prototype's 264 px sidebar / 28 px gutters / card grid. |
| C8 | Margin & Padding | Delightful (3) | 1× | Header title/sub, card padding, KV strip; app KV strip is ~1 row taller than the prototype (provider wraps). |
| C9 | IA & App Depth | Delightful (3) | 1× | 10 console + 7 portal screens; Technical trace even outside presenter mode. |
| C10 | State Communication | Delightful (3) | 1× | To-do bars, amber checks, disabled Submit, skeleton shimmer + IsSubmitting (rev 49), success alerts, BlankSlate. |
| C11 | Animations | Delightful (3) | 1× | 0.18 s transitions, reduced-motion handled. |
| C12 | Micro-interactions | Acceptable (2) | 1× | Hover/sort/badges only. |
| C13 | Content & Data Quality | Market Leading (4) | 1× | Real VN data, FormatVND, trace payloads, decision reasons; demo breadth thinner than the prototype (4 claims, all "Under assessment"). |
| C14 | Modern vs Dated | Delightful (3) | 1.5× | 4/5 — held back by the portal wizard and the thin Dashboard chart. |
| C15 | WoW & AI | Market Leading (4) | 1.5× | 5/5 — recommendation card → reason popup, low-confidence acknowledge, assistant panel with live agent steps. |
| C16 | Visual Identity | Delightful (3) | 1.5× | 3 of 4 authored; iconography still stock Phosphor (sprite attempt reverted). |

## B. Fidelity matrix — prototype vs app (pairs verified this pass)

| Surface | Matches | Differences (prototype → app) |
|---|---|---|
| Login | teal gradient, form, illustration | Staff/Customer toggle, "Log in as sample user", presenter link → not built |
| Sidebar | gradient, brand mark, groups, F-codes, badges, footer | brand sub "Digital Health Insurance Platform · MVP 1" → "MVP 1"; footer role "Claims assessor" → "Approver" (real role) |
| Top bar | ☰ · title/sub · bot+badge · pill search · VI · moon · bell | ✅ identical |
| Dashboard | tinted KPI cards + icon squares + money subs, chart card + "2026" chip, Age list, My queue | chart: 9 weeks × 2 series (T1–T9) → 4 bars W6–W9 single series; KPI values 11/8/4/6 → 4/0/0/0 (seed); My queue lacks the second sub-state pill ("Proposal pending", "Low-confidence document") that the Work list already has |
| Work list | filter card, ghost Take unassigned / Export, solid pills + sub-state pill, mono claim no., age chips | 8 claims with 6 different statuses → 4 claims all "Under assessment"; pagination "Prev 1 2 3 Next" → "1 to 4 of 4 items · 20/page" |
| Claim detail — header | breadcrumb, name + type chip + status pill, KV strip, tabs with F-codes + amber count | stepline labels "Claim submitted · Documents read · Assessment · Approval · Payment & notice" → "Received · Extracting · Under assessment · Pending approval · Paid"; "in-network" green chip and "name verified" chip → plain text |
| Claim detail — Costs | To-do bar, receipt (ORIGINAL — T1-01) with highlighted line, extraction table, cat chips, conf pills, "Proposal: out of cover" | Decision for normal lines "Payable" (green pill) → "Pending"; receipt column narrower with "Page 1/5 · Zoom" → wider, no pager; **claim A also shows the low-confidence bar** (line 4 seeded 0.62) — prototype claim A has none |
| Portal — My cover | hero HC + In force + Certificate, 3 tinted KPIs, Recent claims, Main benefits with bars, See all benefits | floating bot FAB bottom-right → absent; KPI 1 sub "Nguyễn Thị Mai · Nguyễn Văn Sinh" → none |
| Portal — New claim | title + sub, 4-step header, Claim request card, Continue button | wizard = 4 **cards** (number · title · sub, active tinted) → dot stepper; fields "Treated from — to", "Amount claimed (optional)", "Payout account" → missing; button right-aligned → left; sub "You never type a number already on your documents." → "4 steps · about 3 minutes" |
| Technical trace | (presenter mode) | ✅ matches; app also exposes it in the menu |

## C. Findings (one root cause each)
1. **Demo data breadth** (C13, Work list/Dashboard): the prototype demos 8 claims in 6 states and a 9-week history; the seed has 4 claims in one state and one week → charts and lists look empty.
2. **Vocabulary drift** (C1/C3): stepline labels and the "Payable" decision text differ from the prototype's wording.
3. **Portal New-claim form** (C1/C14): dot stepper and 4 missing fields vs the prototype's card stepper and full form.
4. **Claim A low-confidence bar** (C13): line 4 seeded at 0.62 trips the < 0.7 rule; the prototype keeps low confidence on claim C only.
5. **Chips** (C1): in-network / name-verified chips rendered as plain text.

## D. Mentor handoff (fidelity batch F, ≤ 6 items each)
1. **[C1/C3] Wording** — ClaimDetail StepLine labels → "Claim submitted · Documents read · Assessment · Approval · Payment & notice"; Costs Decision for lines without a proposal → "Payable" green pill (`cc-st cc-st-g`), keep "Proposal: out of cover"; header chips: "in-network" (`cc-conf` green) and "name verified" (green chip) next to Provider / Account (evidence: "stepline labels …; in-network chip → plain text").
2. **[C13] Seed breadth** — Seed_AssessmentAndTerms: set claim A line 4 Confidence 0.91 (keep the recommendation at 62 %); spread the 4 demo claims' CreatedOn across 9 weeks and add settled/paid rows so the Dashboard plots both series; give claims B–D distinct statuses (Paid+Auto-approved, Awaiting information, ExtractionError) using existing ClaimStatus rows only (evidence: "8 claims with 6 statuses → 4 claims all Under assessment").
3. **[C1] Dashboard My queue** — reuse the Work-list row block so the second sub-state pill appears; chart labels T1–T9, both series (evidence: "My queue lacks the second sub-state pill").
4. **[C1/C14] Portal New claim** — replace the dot stepper with 4 step cards (`.pt-step` number circle + title + sub, active tinted #e8f0fe), add fields Treated from–to, Amount claimed (optional), Payout account (read-only from member bank data), right-align Continue, subtitle "You never type a number already on your documents."; add the floating bot FAB (`.pt-fab`, 48 px teal, bottom-right) on portal screens (evidence: "wizard = 4 cards → dot stepper; fields … missing").
5. **[C1] Sidebar brand sub** — "Digital Health Insurance Platform · MVP 1" under both brands (evidence: "brand sub → MVP 1").
6. **[C5] ☰ button 44 px** — the menu toggle is 36 px (evidence: "☰ 36 px").

## Addendum — handoff F executed (rev 56–61, 2026-09-19)
| Item | Result | Verified in Chrome |
|---|---|---|
| ☰ hide/show menu | `cc-mini` rail 76 px (icons only, persisted in localStorage `cc.mini`), auto-mini while the panel is open | ✓ `app-dashboard-rev61.jpg` (expanded), rev 56 probe (76 px) |
| Chatbot / assistant panel | Inbox mode off-claim (to confirm · alerts · rows), on-claim: Steps that ran, "Needs confirmation" proposal card with Confirm/Reject, 3 chips → user/AI bubbles with data-driven answers, ask input, note; portal FAB chatbox | ✓ `app-assistant-panel-rev59.jpg`; bubbles 35/91 px; panel fixed 100 vh + scroll-to-bottom (rev 60) |
| Wording | StepLine "Claim submitted · Documents read · Assessment · Approval · Payment & notice"; "Payable" pills; in-network / name-verified chips | rev 60 (not re-screenshotted) |
| Seed breadth | Claims A UnderReview (proposal) · B Paid · C UnderReview low-confidence · D Document processing error; CreatedOn spread; claim A line 4 conf 0.91 | ✓ `app-worklist-rev60.jpg` (4 distinct pills), `app-dashboard-rev61.jpg` (T1–T9, Received + Settled) |
| Dashboard My queue | Work-list row block reused, T-labels | ✓ |
| Portal New claim | 4 step cards, subtitle, Treated from–to / Amount / Payout account, Continue right, old dot wizard removed (rev 61), FAB | ✓ `app-portal-newclaim-rev60.jpg` (rev 60 still showed the old wizard; removed at rev 61) |
| Brand sub / ☰ 44 px | "Digital Health Insurance Platform · MVP 1", mini hides brand text and badges | ✓ |

Estimated score after rev 61: C1 3→3, C5 2 (☰ now 44 px → ≈ 97 % → Delightful 3), C10 3, C12 2→3 (chat, mini rail, scroll), C13 4, C14 4/5→5/5 (Market Leading 4) → **≈ 80 % Delightful**. Remaining for Market Leading: keyboard ring capture (C6), authored iconography (C16 — needs a Script resource), calc-table descriptions, login Staff/Customer toggle.

## Addendum 2 — screen proportions (rev 62–63)
Root cause of "tỷ lệ màn chưa đúng": `.main-content.ThemeGrid_Container` had `max-width:1280px; margin:0 196px` (boxed, centred column) and the menu was 248 px; the prototype is full-bleed with 28 px side padding and a 264 px sidebar. Fixed in the theme (`max-width:none; width:100%; padding:24px 28px 40px`, menu 264 px, `.main` margin-left 264 / 76 px in mini). Tables now `width:100%`; the empty strip above the Work-list table removed. Verified `shots-rev55/app-worklist-rev63.jpg` — layout matches `proto-worklist.jpg` (content 158–910 vs 143–928 at 0.6 scale).
