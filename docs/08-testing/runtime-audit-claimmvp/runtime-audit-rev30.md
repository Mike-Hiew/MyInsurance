# Runtime UI Audit — ClaimMVP · rev 30 · 2026-09-18 (re-audit)

**Result: 69 % — Delightful** (numerator 48 / denominator 70, 0 N/A). Baseline rev 12: 57 % Acceptable (+12 pts).
Surfaces: WorkList, Dashboard, ClaimDetail (General + tab probe), Policies/Terms/Checkpoints/Ledger/Admin (batch 5), PortalHome, PortalBenefits, PortalClaims, PortalClaimDetail, PortalNewClaim, PortalSupplement, PortalNotifications. Captured through the user's logged-in Chrome session at 1568×772 (Playwright cannot pass the login). Read-only.

| # | Criterion | Tier | W | Evidence |
|---|---|---|---|---|
| C1 | Theme & Styling | Acceptable (2) | 1× | Authored skin now live: body `#F4F6F9`, sidebar `radial-gradient(#12454a→#0c3036→#082227)`, primary pill `rgb(12,48,54)` r=999, cards 16 px/no border, Manrope+Inter loaded; no `#1068eb`/`#f3f6f8`. Drift in 3 places: Dashboard KPI cards still white while Portal KPI cards are tinted; ClaimDetail header status is the old tinted Tag ("Received") while tables use solid pills; Dashboard section `h2` at 32 px vs 15 px elsewhere. |
| C2 | Component Reuse | Delightful (3) | 1× | OS UI LayoutSideMenu/Menu, TableRecords, Tabs, Wizard, Counter, Tag, Alert, Timeline, ProgressBar reused; one button language (pill primary/ghost) after fix C; labels persistent on the New-claim form. |
| C3 | Component Fit | Delightful (3) | 1× | Curated 7-column work list with sort, 4-step customer timeline, 4-step wizard with simulated processing, 8-tab claim detail, KPI counters with money subs, per-week ColumnChart with W1–W9 labels; stepline still all "next" on claim 17 (generic). |
| C4 | Colour Contrast | Delightful (3) | 1× | `#1B2129` on white; muted `#6E7787` on white 4.9:1; white on `#C77700` 4.5:1 / on `#1F9D57` 4.6:1 / on `#1E6FEB` 4.6:1; status conveyed by text + colour; no colour-only states. |
| C5 | Tap / Click Targets | Unpleasant (1) | 1× | Probe: ClaimDetail 44 targets, **66 %** ≥ 44 px (small: "View" links 30×16, table action buttons 32 px high, icon button 40×40); PortalHome 9 targets, 67 % (ghost/primary pills 40 px high). |
| C6 | Keyboard | Acceptable (2) | 1× | Theme carries a designed `:focus-visible{outline:2px solid var(--color-primary);outline-offset:2px}` rule (probe `focusVisibleRule:true`); a keyboard focus state could not be captured through the extension, so scored as present-but-unverified. |
| C7 | Grid Adherence | Delightful (3) | 1× | 264 px sidebar, 28 px content gutter, Columns4/2 KPI + chart rows, ColumnsMediumLeft lists; cards share one left edge; 16/24 px rhythm. |
| C8 | Margin & Padding | Acceptable (2) | 1× | Card padding 18–20 px consistent; but h1 + subtitle still run inline on ClaimDetail/PortalBenefits/PortalClaimDetail ("← Claims Claim result Item by item…"); Dashboard `h2` oversized; portal "New claim" pill floats alone in the header. |
| C9 | IA & App Depth | Delightful (3) | 1× | 9 console + 7 portal screens, 2 nav groups per role, breadcrumb/back links, wizard with pre-action guidance and post-action "what happens next", Checkpoint/Ops queues, Admin; BlankSlate empty states. |
| C10 | State Communication | Acceptable (2) | 1× | BlankSlate on History/Ops/Checkpoints, wizard processing state with spinner + progress, Notification toasts on stubbed actions; no skeleton/loading CTAs; success state after Send is a toast only. |
| C11 | Animations | Delightful (3) | 1× | 9/15 sampled controls carry OS UI 0.18 s transitions; `prefers-reduced-motion` handled in the stylesheet (probe true); no decorative motion. |
| C12 | Micro-interactions | Acceptable (2) | 1× | Row hover fill, pill button hover (#0f4149), sort carets; nothing beyond OS defaults. |
| C13 | Content & Data Quality | Market Leading (4) | 1× | Real Vietnamese claim data (Nguyễn Văn An, HC-2026-0001, N39.0), VND via FormatVND ("2.000.000 ₫"), 9-step calc chain with PASS results, AI recommendation card with 62 % confidence and a Decide CTA in the Checkpoint queue, 14-row agent log. |
| C14 | Modern vs Dated | Delightful (3) | 1.5× | **4/5** — dark-teal sidebar with accent bar, pill buttons, 16 px soft-shadow cards, Manrope 800 headings, solid status pills; held back by the untinted Dashboard KPIs and hollow stepline. |
| C15 | WoW & AI | Market Leading (4) | 1.5× | Craft: composite claim-detail surface (stepline + 8 tabs + calc chain + ledger card on `ClaimDetail`), 4-step new-claim wizard with live processing pipeline (`PortalNewClaim`) · AI: recommendation card "REC-CLIN-04 · 62 %" with Decide/Ack CTAs on the costs tab and `Checkpoints` → **5/5** (Craft 4/5 raised by in-context AI). |
| C16 | Visual Identity | Delightful (3) | 1.5× | Palette authored (#0c3036 / #1E6FEB / #35d3a3 accent), Typography authored (Manrope + Inter), Shape & depth authored (16 px radius, pill buttons, layered soft shadow); Iconography: Phosphor stock glyphs + CC/MI marks + coloured KPI icon squares (partial) → 3 of 4 authored; the teal sidebar passes a loose logo-removal test. |

## Notable findings (one root cause = one finding)
1. **Skin not applied uniformly** (C1, C8, C14): Dashboard Counter cards keep `background-neutral-0` (no tint/icon square) while Portal KPIs are tinted; ClaimDetail header still uses the tinted OS Tag; `h2` section titles 32 px on Dashboard; h1 + subtitle inline on ClaimDetail / PortalBenefits / PortalClaimDetail.
2. **Tap targets < 44 px** (C5): "View" links, 32 px table action buttons, 40 px pills/icon buttons — 66–67 % compliant.
3. **Stepline never shows progress** (C3, C14): all five steps render as "next" on a Received claim.
4. **Header cluster missing** (fidelity F4): no pill search + VI/moon/bell buttons in the top bar; portal shows only the "New claim" pill.
5. **Loading/completion states thin** (C10): no skeleton or CTA loading state; Send/Submit feedback is a toast.

## Mentor handoff (target: Market Leading ≥ 85 %)
1. **[C1/C8/C14] Finish the skin on Dashboard + ClaimDetail** — on `Dashboard` set each Counter's ExtendedClass `cc-kpi cc-kpi-amber|purple|blue|green`, drop `background-neutral-0`, add the `.cc-kpi-ic` icon square; on `ClaimDetail` replace the header Tag with the `span.cc-st` pill via StatusUi; set `h2{font-size:15px}`; put h1 and `.cc-page-sub` in two block rows on ClaimDetail, PortalBenefits, PortalClaimDetail.
2. **[C5] Raise targets to ≥ 44 px** — theme: `.btn,.cc-btn-ghost,.btn-primary{min-height:44px}`; `.cc-table td a, .cc-lnk{min-height:44px;display:inline-flex;align-items:center;padding:0 8px}`; row action buttons `min-height:44px`.
3. **[C3/C14] Stepline states** — on `ClaimDetail`, `PortalClaims`, `PortalClaimDetail` compute step status from ClaimStatus (Received → 1–2 done, 3 current) and render done = green filled ✓, current = `#0c3036` filled.
4. **[F4] Header cluster** — place `Common/HeaderActions` (pill search / "+ New claim", VI, moon, bell-with-dot at 40 px round) in both layouts' header right placeholder; remove the centred search box.
5. **[C10] Loading + completion** — Skeleton (OS UI Skeleton block) on WorkList/Dashboard tables while fetching; Submit/Send buttons `IsLoading` during the server call; after Send show an inline success Alert with "what happens next".

## Method
Chrome (user session) 1568×772: screenshots of WorkList, Dashboard, ClaimDetail, PortalHome, PortalBenefits, PortalClaims, PortalClaimDetail, PortalNewClaim, PortalSupplement, PortalNotifications, Term register, Checkpoints, Limit ledger, Ops queue; JS probes for tap-target boxes, transitions, `prefers-reduced-motion`, focus-visible rule, fonts, computed colours. Mobile viewport not captured (extension resize is unstable). Read-only; the app was not modified during the audit. Design-fidelity companion: `design-fidelity-criteria.md` (F1–F12).
