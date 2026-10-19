# Runtime UI Audit #4 — ClaimMVP · rev 63 · 2026-09-19

**Result: 78 % — Delightful** (numerator 54.5 / denominator 70, 0 N/A). Trend: rev 12 57 % → rev 30 69 % → rev 47 71 % → rev 55 74 % → **rev 63 78 %**.
Surfaces: WorkList, ClaimDetail (General + Costs, assistant panel), Dashboard, PortalNewClaim, PortalHome. Captured through the user's logged-in Chrome session at 1920×945 / 1568×772 (auth-gated runtime; Playwright cannot log in). Mobile not captured. Read-only. Shots: `shots-rev63/`.

| # | Criterion | Tier | W | Evidence |
|---|---|---|---|---|
| C1 | Theme & Styling | Delightful (3) | 1× | Full-bleed content (`.main-content` 1656 px on a 1920 viewport, 28 px gutters), 264 px sidebar, primary `rgb(12,48,54)` r=999, `th` white/`#6E7787`, solid pills (`#C77700`/`#1F9D57`/`#D33A3A`), Manrope 800 22 px titles, brand sub "Digital Health Insurance Platform · MVP 1". Drift: the new "in-network"/"name verified" chips render as full-width blocks under the KV values instead of inline chips. |
| C2 | Component Reuse | Delightful (3) | 1× | LayoutSideMenu + Menu, TableRecords, Tabs, Popup, Sidebar, ProgressBar, BlankSlate, Notification, Wizard replaced by step cards on the portal; one button language. |
| C3 | Component Fit | Delightful (3) | 1× | StepLine "Claim submitted · Documents read · Assessment · Approval · Payment & notice" with done/current states, decision popup, assistant panel with inbox / steps / proposal card / chat, mini rail, grouped settlement, trace table. |
| C4 | Colour Contrast | Delightful (3) | 1× | Unchanged (≥ 4.5:1 on all pill/text pairs; chat bubbles `#35d3a3`/`#082227` 9:1). |
| C5 | Tap / Click Targets | Acceptable (2) | 1× | WorkList **97 %** ≥ 44 px (rev 47: 31 %); ClaimDetail **85 %** (tab headers 54 px ✓; small: table "View" links, chat chips 36 px, panel close 40 px). |
| C6 | Keyboard | Acceptable (2) | 1× | `:focus-visible` rule present (probe true); ring not captured through the extension. |
| C7 | Grid Adherence | Delightful (3) | 1× | Proportions now match the prototype (content 158–910 vs 143–928 at 0.6 scale); Columns4 KPIs, ColumnsSmallLeft receipt, 16/24 px rhythm. |
| C8 | Margin & Padding | Delightful (3) | 1× | Header 70 px with title/sub, cards 18–20 px, tables 100 % of their card, empty strip removed. |
| C9 | IA & App Depth | Delightful (3) | 1× | 10 console + 7 portal screens; assistant inbox routes to claims/OpsQueue; Technical trace; breadcrumbs, F-code hints. |
| C10 | State Communication | Delightful (3) | 1× | To-do bars, amber checks, disabled Submit, skeleton shimmer, `IsSubmitting`, success alerts, BlankSlate, inbox counts ("1 to confirm · 1 alerts"). |
| C11 | Animations | Delightful (3) | 1× | 19 sampled controls with 0.18 s transitions; `prefers-reduced-motion` handled. |
| C12 | Micro-interactions | Delightful (3) | 1× | Mini-rail toggle persisted, panel auto-collapses the menu, chat bubbles with scroll-to-latest, chips, row hover, sort carets. |
| C13 | Content & Data Quality | Market Leading (4) | 1× | 4 demo claims in 4 states (Under assessment + proposal, Paid, Low-confidence, Document processing error), 9-week history, VND via FormatVND, trace payloads, decision reasons, data-driven chat answers. |
| C14 | Modern vs Dated | Market Leading (4) | 1.5× | **5/5** — dark-teal shell with mini rail, pill header cluster, tinted KPIs, dark assistant panel with chat, step cards, solid pills; no dated element left. |
| C15 | WoW & AI | Market Leading (4) | 1.5× | 5/5 — recommendation → reason popup, low-confidence acknowledge, assistant with live agent steps + proposals + Q&A, portal chatbot FAB. |
| C16 | Visual Identity | Delightful (3) | 1.5× | Palette / Typography / Shape & depth authored; Iconography still stock Phosphor (sprite via Expression is not viable on this tenant) → 3 of 4. |

## Notable findings (one root cause each)
1. **Header chips stretched** (C1): "in-network" / "name verified" use a block-level container → full-width green bars under the KV values instead of inline chips.
2. **Small secondary controls on ClaimDetail** (C5): table "View" links (30 px), chat chips (36 px), panel close (40 px) keep the screen at 85 %.
3. **Keyboard ring unverified** (C6): rule exists, capture tooling cannot show it.
4. **Iconography still stock** (C16): needs a Script resource / image sprite, not an Expression.

## Mentor handoff (target: Market Leading ≥ 85 %)
1. **[C1] Inline chips** — on ClaimDetail set the in-network / name-verified chips to `display:inline-flex;width:auto;margin-left:8px;vertical-align:middle` inside the same line as the value (evidence: "render as full-width blocks under the KV values").
2. **[C5] Remaining small targets** — `.cc-table td a{min-height:44px;display:inline-flex;align-items:center}`, `.cc-assist-chip{min-height:44px}`, panel close 44 px (evidence: "ClaimDetail 85 %").
3. **[C6] Designed focus ring** — verify with a real Tab press that `:focus-visible` outline `2px solid #1E6FEB` shows on pills, tabs and menu links; add `outline-offset:2px` (evidence: "ring not captured").
4. **[C16 (Iconography)] Icon set** — upload the prototype's `ic()` glyphs as an SVG sprite **image resource** and reference symbols with `<use href="/ClaimMVP/img/cc-icons.svg#ic-list">` from IIcon-free AdvancedHtml blocks (the inline-Expression attempt at rev 49 rendered as escaped text) (evidence: "Iconography still stock").
5. **[C1] Calculation tab descriptions** — rule sub-lines + amounts as in the prototype (carried over from audit #2).

## Method
Chrome (user session): screenshots `shots-rev63/worklist.jpg`, `claimdetail.jpg`, `dashboard-rev61.jpg`, `portal-newclaim-rev60.jpg`, `assistant-panel-rev59.jpg`; JS probes for tap-target boxes, transitions, reduced-motion, focus-visible, computed colours/fonts, content/menu widths. Companion docs: `runtime-audit-rev55.md` (fidelity matrix + addenda), `review-vs-prototype-v05.md`.

## Addendum — handoff H1 executed (rev 65, 2026-09-19)
| Item | Result | Verified |
|---|---|---|
| 1. Inline chips | `.cc-chip-ok` inline-flex next to the value | ✓ `shots-rev63/claimdetail-rev65.jpg` ("name verified" sits by Card ID rather than Account — minor placement) |
| 2. 44 px residuals | table links / chips / popup buttons 44 px | ClaimDetail **85 % → 91 %** (left: breadcrumb link, search input 40 px, one 40 px icon button) |
| 3. Focus ring | `button:focus-visible{outline:2px solid #1E6FEB}` present in the theme (probe: `rgb(30,111,235) solid 2px`) | rule verified; visual capture still not possible through the extension |
| 4. SVG sprite as image resource | Mentor reports the HTML widget still escapes the markup on this tenant → **not viable**, Phosphor icons kept (no escaped text on screen) | ✓ |
| 5. Calculation tab | step sub-lines + FormatVND amounts added | not re-screenshotted |

Estimated re-score after rev 65: C6 Acceptable → Delightful (designed ring), C5 unchanged (91 %) → **≈ 79 % Delightful**. Path to ≥ 85 % now depends on C5 ≥ 95 % (search input + icon buttons 44 px), C16 iconography (requires a custom icon font / image icons rather than inline SVG), and C12/C10 polish.
