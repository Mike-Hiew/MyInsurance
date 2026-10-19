# Runtime UI Audit #5 — ClaimMVP · rev 65 · 2026-09-19

**Result: 78 % — Delightful** (numerator 54.5 / denominator 70, 0 N/A). Trend: rev 12 57 % → rev 30 69 % → rev 47 71 % → rev 55 74 % → rev 63 78 % → **rev 65 78 %** (C6 +1 for the designed focus ring, C13 −1 for three data-display defects found on re-verification).
Surfaces: WorkList, ClaimDetail (General · Costs · Calculation & ledger · Settlement, assistant panel), Dashboard, PortalHome, PortalNewClaim. Captured through the user's logged-in Chrome session at 1920×945 (auth-gated runtime; Playwright cannot log in; mobile not captured). Prototype re-served on `localhost:8765` for the calc-tab pair. Read-only. Shots: `shots-rev65/`.

## A. Verification of handoff H1 (what rev 65 actually shows)

| H1 item | Claimed | Verified in Chrome |
|---|---|---|
| 1. Inline chips | done | ✓ "name verified" / "in-network" inline (`display:block` on a 76–139 px span next to the value) — `claimdetail-calc.jpg` |
| 2. 44 px residuals | done | ClaimDetail 91 %, WorkList **97 %**, Dashboard 86 %, PortalNewClaim 76 %, PortalHome **68 %** (chatbox chips 23 px, send 30 px, close 42 px are measured even while the box is closed) |
| 3. Focus ring | done | rule `button:focus-visible{outline:2px solid #1E6FEB}` present on every screen (probe `focusVisible:true`) |
| 4. SVG sprite | not viable | ✓ no escaped markup; Phosphor icons |
| 5. Calculation tab | "sub-lines + amounts added" | **✗ not visible** — the `RuleDetailExpr` span exists but is empty and the Result column shows only a PASS tag for all 9 steps; the prototype shows a detail line under every rule and money on steps 5/6/8/10/14 (`claimdetail-calc.jpg` vs `proto-claimdetail-calc.jpg`) |

Other regressions found while verifying:
- **PortalNewClaim "Continue — documents →" is squeezed** into a 97 px column (its wrapper carries `ThemeGrid_Width1` + `justify-content-flex-end`) → 111 × 44 px button, text wrapped on 3 lines, left-aligned (`portal-newclaim.jpg`). Rev 60 had it right-aligned full-width.
- **Costs Decision column says "Pending"** for lines 1–3 (no `cc-dec-*` class); the prototype and handoff F said "Payable" (green 700). Line 4 "Proposal: out of cover" ✓.
- **Settlement split still wrong**: `INPATIENT_ROOM 1.450.000 / INPATIENT_OTHER 350.000`; the four lines are ROOM 600.000 + LAB 350.000 + IMAGING 400.000 + DRUG 450.000 → expected ROOM 600.000 / OTHER 1.200.000 (BuildBenefitRows rev 48 rewrite did not fix it).

## B. Rubric score

| # | Criterion | Tier | W | Evidence |
|---|---|---|---|---|
| C1 | Theme & Styling | Delightful (3) | 1× | body `rgb(244,246,249)`, primary pill `rgb(12,48,54)` r=999, `th` white/`#6E7787`, solid pills `#C77700`/`#1F9D57`/`#D33A3A` + tinted sub-state pills, Inter/Manrope, 264 px teal rail, tinted KPIs. Drift: squeezed Continue button on PortalNewClaim; "Pending" instead of "Payable"; calc rows without detail lines. |
| C2 | Component Reuse | Delightful (3) | 1× | LayoutSideMenu + Menu, TableRecords, Tabs, Popup, Sidebar panel, ProgressBar, BlankSlate, Notification, Tag, step cards; one button language. |
| C3 | Component Fit | Delightful (3) | 1× | StepLine done/current, decision popup, assistant panel (steps · proposal · chips · ask), mini rail on panel open (`claimdetail-panel.jpg`), 4-step cards with active tint `rgb(232,240,254)`, dropdown placeholder "Select loss type…". Calc table lacks the prototype's cut/changed row states. |
| C4 | Colour Contrast | Delightful (3) | 1× | Unchanged (≥ 4.5:1 on all pill/text pairs; chat bubbles 9:1). |
| C5 | Tap / Click Targets | Acceptable (2) | 1× | WorkList 97 % · ClaimDetail 91 % · Dashboard 86 % · PortalNewClaim 76 % · PortalHome 68 % → mean ≈ 84 %. Under-sized: chatbox chips 23 px, chatbox send 30 px, close 42 px, "View all" 14×21, breadcrumb, search 40 px. |
| C6 | Keyboard | Delightful (3) | 1× | Designed, consistent ring `2px solid #1E6FEB` + `outline-offset:2px` on every screen; visual still not capturable through the extension. |
| C7 | Grid Adherence | Delightful (3) | 1× | Full-bleed content 1656 px on 1920 (28 px gutters), 264 px sidebar / 76 px mini, Columns4 KPIs, step-card row. One grid slip: Continue button in a `ThemeGrid_Width1` column. |
| C8 | Margin & Padding | Delightful (3) | 1× | 70 px header with title/sub, 18–20 px cards, tables 100 % of card, KV strip 2 rows, tab bar 54 px. |
| C9 | IA & App Depth | Delightful (3) | 1× | 10 console + 7 portal screens; assistant inbox routes; Technical trace; breadcrumbs; F-code hints. |
| C10 | State Communication | Delightful (3) | 1× | To-do bars, amber checks, disabled Submit, `IsSubmitting`, skeleton shimmer, BlankSlate, inbox counts, "After your decision —" placeholder box. |
| C11 | Animations | Delightful (3) | 1× | 19 controls with 0.18 s transitions; `prefers-reduced-motion` handled (probe true). |
| C12 | Micro-interactions | Delightful (3) | 1× | Mini-rail toggle persisted, panel auto-collapses menu, chat bubbles + scroll-to-latest, chips, row hover, sort carets, FAB chatbox on portal. |
| C13 | Content & Data Quality | Delightful (3) | 1× | 4 claims in 4 states over 9 weeks (T1–T9, 2 series), VND via FormatVND, trace payloads. **Three display defects**: calc steps carry no detail/amount, settlement Claimed mis-split (1.450.000/350.000), undecided lines read "Pending" not "Payable". |
| C14 | Modern vs Dated | Market Leading (4) | 1.5× | **5/5** — dark-teal shell + mini rail, pill header cluster, tinted KPIs, dark assistant panel with chat, step cards, solid pills. |
| C15 | WoW & AI | Market Leading (4) | 1.5× | **5/5** — proposal card → reason popup, low-confidence acknowledge, live agent steps, data-driven Q&A, portal chatbot. |
| C16 | Visual Identity | Delightful (3) | 1.5× | Palette / Typography / Shape & depth authored; Iconography stock Phosphor (inline SVG not viable on this tenant) → 3 of 4. |

## C. Findings (one root cause each)
1. **Calc chain writes no step detail** (C13, C1, C3): `CalcSnapshot.Detail` and `ResultAmount` are empty for claim 17, so the Calculation tab is 9 × "name + PASS" instead of the prototype's rule sub-line + running amount (`proto-claimdetail-calc.jpg`).
2. **Settlement per-benefit grouping** (C13): BuildBenefitRows still sums lines into the wrong benefit (ROOM 1.450.000 / OTHER 350.000).
3. **Continue button in a 1/12 column** (C7, C1): PortalNewClaim's button wrapper has `ThemeGrid_Width1`.
4. **"Pending" wording on undecided lines** (C1, C13): Decision expression returns "Pending" where the prototype says "Payable".
5. **Portal small targets** (C5): chatbox chips 23 px, send 30 px, close 42 px, "View all" link 21 px.

## D. Mentor handoff H2 (target: Market Leading ≥ 85 %; ≤ 5 items)
1. **[C7/C1] PortalNewClaim** — on the container that wraps "Continue — documents →" remove the `ThemeGrid_Width1` width (set Width to Fill / remove the class) so the flex row spans the card and the button sits right-aligned at its natural width, `white-space:nowrap` (evidence: "squeezed into a 97 px column, text wrapped on 3 lines").
2. **[C13/C1] Calculation & ledger tab** — in RunAdjudicationChain write `CalcSnapshot.Detail` (e.g. "Member active · premium paid", "No exclusion for N39.0", "1→ROOM · 2→OTHER…", "Provisional · committed at approval") and `ResultAmount` for money steps; on ClaimDetail render `RuleDetailExpr` as `.cc-step-sub` under the rule name and show `FormatVND(ResultAmount)` in Result when ResultAmount > 0 (PASS tag otherwise); row class `cut` (red text) when Outcome = CUT, `chg` (amber tint) when changed (evidence: "RuleDetailExpr empty, Result only PASS for all 9 steps").
3. **[C13] BuildBenefitRows** — group strictly by ClaimLine.Category → benefit: ROOM (category ROOM/CONSULT "Tiền giường") vs OTHER (LAB, IMAGING, DRUG); expected on claim 17: ROOM 600.000 / OTHER 1.200.000, Total 1.800.000 (evidence: "INPATIENT_ROOM 1.450.000 / INPATIENT_OTHER 350.000").
4. **[C1/C13] Costs Decision text** — for a line where `IsRecPending` is false and no NotCovered/Rejected state exists show "Payable" with `cc-dec-ok` (green 700); keep "Proposal: out of cover" `cc-dec-warn` and "Declined" `cc-dec-bad` (evidence: "Decision column says Pending for lines 1–3").
5. **[C5] Portal + Dashboard targets** — `.pt-chatbox-chip{min-height:44px}`, `.pt-chatbox-send{width:44px;height:44px}`, chatbox close 44 px, Dashboard "View all →" link `min-height:44px;display:inline-flex;align-items:center`, ClaimDetail search input `min-height:44px` (evidence: "PortalHome 68 %, Dashboard 86 %").

## Method
Chrome (user session, tab 717093194) 1920×945 → 0.6-scale JPEGs in `shots-rev65/`: `worklist.jpg`, `dashboard.jpg`, `claimdetail-calc.jpg`, `claimdetail-panel.jpg`, `portal-home.jpg`, `portal-newclaim.jpg`, prototype pair `proto-claimdetail-calc.jpg` (served from `E:\Hackaton\batch\_proto.html`, state forced via `go('detail',{claim:'A',tab:'calc'})`). JS probes: tap-target boxes per screen, transition durations, `prefers-reduced-motion`, `:focus-visible`, computed colours/fonts/radii, content/menu widths, table cell text, CalcSnapshot schema via `context_search`. Companion docs: `runtime-audit-rev63.md` (audit #4 + H1 addendum), `runtime-audit-rev55.md` (fidelity matrix), `review-vs-prototype-v05.md`.

## Addendum — handoff H2 + menu batches M1/M2 executed (rev 66–71, 2026-09-19)
| Item | Result | Verified in Chrome |
|---|---|---|
| H2.1 Continue button | `ThemeGrid_Width1` removed, `.pt-continue{white-space:nowrap}` | ✓ 206×44 right-aligned (`shots-rev65/portal-newclaim-rev66.jpg`) |
| H2.2 Calc tab | seed writes `CalcSnapshot.Detail`; tab renders `.cc-step-sub` + `FormatVND(ResultAmount)` | ✓ 9 rules with detail lines, step 9 "2.000.000 ₫" (steps 4–8 still amount-less: ResultAmount 0) |
| H2.3 Settlement split | 3 attempts: rev 66 (one row), rev 67 (ROOM 0 — diacritic literal mismatch), rev 68–71 (seed sets `Category="ROOM"` on line 1 of claims A/B/C by ClaimNo; BuildBenefitRows tests `Category="ROOM"`) | ✓ ROOM 600.000 / OTHER 1.200.000, Costs chip ROOM |
| H2.4 Decision wording | "Payable" `cc-dec-ok` on undecided lines | ✓ |
| H2.5 Tap targets | chatbox chips/send/close 44 px, View-all, search | ✓ (chips measure 42 while the closed chatbox is scaled .95) |
| M1 Console/portal menu | items 240×44 r12 inset 12, active `.13` + inset 3 px #35d3a3, icon 18 px in 22 px, badge pill 14 px #C77700/#D33A3A, labels .1em, third group "TECHNICAL HAND-OVER · PRESENTER" with Technical trace (AW), portal "Claims & progress" count badge, brand 82 px, MI teal gradient | ✓ both menus match the prototype's sidebar |
| M2 | avatar UserAvatar Medium 34 px with real name ("NH" console purple tint, "NV" portal teal tint); group labels no longer styled active | ✓ |

**Lesson:** "Re-run demo seed" takes ≈15 s (Seed_ClaimDocuments); navigating away earlier cancels it server-side (`OS-BERT-00000 The operation was canceled`) and leaves the data half-updated — wait for the "Seed re-run" notification before leaving Administration.

Estimated re-score after rev 71: C13 back to Market Leading (4) with the three data defects fixed, C5 unchanged (portal chips only 44 px when open) → **≈ 79–80 % Delightful**.

## Addendum — batch P1 (rev 72): layout defects reported by the user
| Defect | Root cause | Fix | Verified |
|---|---|---|---|
| Console menu 64 px lower than the portal | empty `div.header-logo.placeholder-empty` (264×64) injected above `.cc-brand` in the console nav | `.app-menu-content .header-logo{display:none}` | ✓ brand at y=0 (`worklist-rev72.jpg`) |
| Header content shifted right (console + portal) | `.header-top.ThemeGrid_Container` boxed 1280 px, `margin:0 188px` | full-width header, `padding:0 28px`, `.header{left:264px}` / mini `left:76px` | ✓ ☰ at x=292, title at 350 on both; header follows the mini rail |
| Console assistant = flush full-height drawer | `.cc-assist-panel.is-open` top/right/bottom 0, radius 0 | prototype `.pnl` floating card: top 78 / right 18 / bottom 18, 360 px, radius 16, 1 px rgba(255,255,255,.1) border, 0 18px 48px shadow, slide-in | ✓ 1542,78 360×793 (`assistant-panel-rev72.jpg`) — same card language as the portal chatbox |

## Addendum — batches P2/P3 (rev 73–74): console chat split out like the portal
Console now has a FAB `.cc-console-fab` (54 px, bottom-right, ph-robot → ph-x + `is-open` while open) and a floating chatbox `.cc-console-chatbox` (360 px, radius 16, dark gradient, title "Claim assistant", welcome bubble, 3 context chips 44 px, ask input, note). The "Claim activity" panel keeps only Inbox / Steps that ran / Proposals — no chat list, no ask block. Panel ⇄ chatbox are mutually exclusive (verified: opening one hides the other). Minor: closing the panel via the FAB leaves the mini rail collapsed until ☰ is clicked. Shot: `console-chatbox-rev74.jpg`.

## Addendum — batch P4 (rev 75–78): single assistant entry point on the console
Header bot button removed; the bottom-right FAB `.cc-console-fab` is the only entry (amber `.cc-fab-n` badge = pending proposals + alerts, e.g. "2"; X icon while open). One floating box `.cc-console-chatbox` with tabs **Chat** (welcome bubble, chips, ask) and **Activity (n)** (off-claim: "Finished by the system, waiting for you" counts + To confirm / Warning / Alerts rows; on-claim: Steps that ran + Proposals awaiting decision with Confirm/Reject). Default tab = Activity when something is pending. Old `.cc-assist-panel` and the auto-mini-rail behaviour removed. Took 4 Mentor runs (2 × OS-AIWA-50002 on the old session → new session `b05bba2a…`; then reversed DOM order / Activity rendered outside the box at rev 76–77, fixed at rev 78 by naming widget ids). Shots: `assistant-activity-rev78.jpg`, `assistant-chat-rev78.jpg`, `assistant-inbox-rev78.jpg`.
