# Runtime UI Audit — ClaimMVP · rev 47 · 2026-09-19 (re-audit #2)

**Result: 71 % — Delightful** (numerator 50 / denominator 70, 0 N/A). Rev 12: 57 % Acceptable → rev 30: 69 % Delightful → **rev 47: 71 %**.
Surfaces: WorkList, Dashboard, ClaimDetail (8 tabs, claims A + C), Checkpoints, OpsQueue, AgentTrace (Technical trace), PortalHome, PortalNewClaim; assistant panel. Captured through the user's logged-in Chrome session at 1568×726 (the runtime is auth-gated; Playwright cannot pass the login). Mobile viewport not captured (extension resize unstable). Read-only — the app was not modified during the audit.

| # | Criterion | Tier | W | Evidence |
|---|---|---|---|---|
| C1 | Theme & Styling | Delightful (3) | 1× | Skin applied on every surface: body `#F4F6F9`, primary pill `rgb(12,48,54)` r=999, `th` white/`#6E7787`, solid status pills (`#C77700`/`#D33A3A`/`#1F9D57` on white), Manrope 800 22 px titles, tinted KPI cards, soft notes everywhere (global `.alert` rule). Drift: portal wizard dots all hollow; PortalNewClaim header title empty; portal menu highlights "Benefit details" on the New-claim screen. |
| C2 | Component Reuse | Delightful (3) | 1× | LayoutSideMenu + Menu, TableRecords, Tabs, Wizard, Popup (decision), Sidebar (assistant), Tag/pills, Alert, ProgressBar, BlankSlate, Notification; one button language (pill primary / ghost). |
| C3 | Component Fit | Delightful (3) | 1× | StepLine with real done/current states, 8-tab claim detail with per-line decision texts, AI recommendation card → reason popup, Technical trace table with kind/state pills, claim-chip filter, grouped settlement table, low-confidence To-do bar. |
| C4 | Colour Contrast | Delightful (3) | 1× | `#1B2129` on white; muted `#6E7787` 4.9:1; white on `#C77700` 4.5:1 / `#1F9D57` 4.6:1 / `#1E6FEB` 4.6:1; status conveyed by text + colour. |
| C5 | Tap / Click Targets | Broken (0) | 1× | Probe: WorkList 29 targets, **31 %** ≥ 44 px; ClaimDetail 42 targets, **43 %** (40×40 icon buttons, 40 px inputs/pills, table links). Regressed vs rev 30 (66 %) because the new 40 px header cluster and pill buttons follow the prototype's 40 px sizes. |
| C6 | Keyboard | Acceptable (2) | 1× | Theme carries a `:focus-visible` rule (probe `focusVisible:true`); ring not captured through the extension → present-but-unverified. |
| C7 | Grid Adherence | Delightful (3) | 1× | 264 px sidebar, 28 px gutters, Columns4 KPIs, ColumnsSmallLeft receipt/table, ColumnsMediumLeft cards; one left edge; 16/24 px rhythm. |
| C8 | Margin & Padding | Delightful (3) | 1× | Title + sub now live in the 70 px header on every console screen; cards 18–20 px; KV strips two-row; tab bar 54 px; no run-together labels (h2 scoped to 15 px). |
| C9 | IA & App Depth | Delightful (3) | 1× | 10 console + 7 portal screens; breadcrumb, F-code hints, Technical trace, Checkpoint/Ops queues, Administration with reseed; assistant panel scoped to the open claim. |
| C10 | State Communication | Delightful (3) | 1× | To-do bars drive the next action (proposal / low confidence), checks flip amber while a proposal is pending, Submit disabled until clean, BlankSlate on empty queues, wizard processing step, toasts; no skeleton/loading CTAs yet. |
| C11 | Animations | Delightful (3) | 1× | 19 sampled controls carry 0.18 s transitions; `prefers-reduced-motion` handled (probe true). |
| C12 | Micro-interactions | Acceptable (2) | 1× | Row hover, pill hover (#0f4149, 0.1 s), sort carets, badge counts; nothing beyond OS defaults. |
| C13 | Content & Data Quality | Market Leading (4) | 1× | Real Vietnamese claim data, VND via FormatVND, 14-row agent trace with real payloads, receipt mock-up, decision reasons in the audit trail, seeded low-confidence claim. |
| C14 | Modern vs Dated | Delightful (3) | 1.5× | **4/5** — dark-teal shell, pill header cluster, tinted KPIs, dark assistant panel, underline tabs, solid pills; held back by the hollow portal wizard and the empty portal header title. |
| C15 | WoW & AI | Market Leading (4) | 1.5× | Craft: composite claim surface (stepline + tabs + receipt + decision popup + grouped settlement), Technical trace, assistant panel with live agent steps · AI: in-context recommendation card "REC-CLIN-04 · 62 %" with Confirm/Reject + mandatory reason, low-confidence acknowledge flow → **5/5**. |
| C16 | Visual Identity | Delightful (3) | 1.5× | Palette authored (#0c3036 / #1E6FEB / #35d3a3), Typography authored (Manrope + Inter), Shape & depth authored (16 px cards, pills, layered shadow); Iconography still stock Phosphor + coloured squares → 3 of 4. |

## Notable findings (one root cause = one finding)
1. **40 px control size everywhere** (C5): the prototype's 40 px buttons/inputs were reproduced literally; only 31–43 % of targets reach 44 px.
2. **Portal screens skipped the header/wizard pass** (C1, C8, C14): PortalNewClaim has no header title, the wizard shows no current step, and the portal menu's active item is wrong.
3. **Loading states thin** (C10, C12): no skeleton while aggregates load; Submit/Send buttons have no IsLoading state.
4. **Settlement grouping bug** (C13, minor): Claimed per benefit is mis-split (ROOM 1.450.000 / OTHER 350.000) although the Total row is right.

## Mentor handoff (target: Market Leading ≥ 85 %)
1. **[C5] Raise targets to ≥ 44 px on every screen** — theme: `.cc-icon-btn,.cc-botbtn{width:44px;height:44px} .btn,.cc-btn-ghost,.btn-primary,.btn-sm{min-height:44px} .cc-search,.form-control,select{min-height:44px} .cc-table td a,.cc-lnk{min-height:44px;display:inline-flex;align-items:center;padding:0 8px}` (evidence: "WorkList 29 targets, 31 % ≥ 44 px; ClaimDetail 43 %").
2. **[C1/C8/C14] Finish the portal pass** — on `PortalNewClaim` fill the layout Title placeholder ("New claim" / "4 steps · ~3 minutes"), compute the Wizard's current step (step 1 current on load), and fix Portal/PortalMenu's active condition (evidence: "PortalNewClaim header title empty; portal menu highlights Benefit details").
3. **[C10/C12] Loading + completion states** — Skeleton block on WorkList/Dashboard/ClaimDetail tables while fetching; `IsLoading` on Submit/Send/Decide buttons; inline success Alert with "what happens next" after Send (evidence: "no skeleton/loading CTAs yet").
4. **[C13] Fix BuildBenefitRows** — group ClaimLines strictly by benefit code (ROOM 600.000 / OTHER 1.200.000) and render one glyph per pre-submission check (evidence: "Claimed per benefit is mis-split").
5. **[C16 (Iconography)] Author the icon layer** — replace stock Phosphor glyphs in the sidebar/KPI squares/trace kinds with the prototype's `ic()` set as an SVG sprite or a custom IIcon mapping so all four identity dimensions are authored (evidence: "Iconography still stock Phosphor → 3 of 4").

## Method
Chrome (user session) 1568×726: screenshots `shots-rev47/worklist.jpg`, `shots-rev47/claimdetail.jpg`, `shots-rev47/portal-newclaim.jpg` (+ the rev 46/47 verification captures of Dashboard, Costs, Settlement, Checkpoints, AgentTrace, PortalHome, assistant panel taken during the D-batches); JS probes for tap-target boxes, transitions, `prefers-reduced-motion`, `:focus-visible`, computed colours/fonts/radii. Design-fidelity companion: `review-vs-prototype-v05.md` (rev 47).

## Addendum — handoff executed (rev 48–50, 2026-09-19)
| Handoff item | Result | Verified |
|---|---|---|
| 1. Tap targets ≥ 44 px | Theme rules + `.cc-icon-btn` 44 px | WorkList **31 % → 94 %** (only the ☰ 36 px and one table link remain) |
| 2. Portal pass | Title/subtitle on all portal screens, menu active fixed | "New claim · 4 steps · about 3 minutes" ✓; wizard step 1 still hollow, Loss type still "0" |
| 3. Loading/completion states | CSS shimmer skeleton (no Skeleton block on the tenant) on WorkList/Dashboard/ClaimDetail, `IsSubmitting` on Decide/Submit/Acknowledge/Submit claim/Send, success Alert after Send | Not captured (aggregates resolve faster than the probe) |
| 4. BuildBenefitRows + single check glyph | Loop rewritten with ListIndexOf on Code | Not re-verified after rev 48 |
| 5. Iconography sprite | **Reverted** — the SVG sprite rendered as escaped text in the sidebar at rev 49; rev 50 restored the Phosphor IIcons | Sidebar icons OK |

Estimated re-score after rev 50: C5 Broken (0) → Acceptable (2) [94 %], others unchanged → **≈ 74 % Delightful**. Remaining to reach Market Leading: C5 ≥ 95 % (☰ button), C6 keyboard ring capture, C12 micro-interactions, C16 iconography (needs a proper sprite via a Script/resource, not an Expression), portal wizard current step.
