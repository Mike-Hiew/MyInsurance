# Runtime UI Audit — ClaimMVP (authenticated console) · rev 12 · 2026-09-18

**Result: 57 % — Acceptable** (numerator 40 / denominator 70, 0 N/A). Surfaces: WorkList, ClaimDetail (General, Extraction & costs, plus a JS probe of all 8 tabs). Captured through the user's logged-in Chrome session (Playwright cannot pass the login).

| # | Criterion | Tier | Weight | Evidence |
|---|---|---|---|---|
| C1 | Theme & Styling | Delightful (3) | 1× | No OS defaults (#1068eb / #f3f6f8 absent); `--color-primary #1E6FEB`, body `#F4F6F9`, cc-* system applied everywhere. −1: `--font-family` names Inter but Inter is never loaded → renders in -apple-system/Segoe; brand block rendered TWICE (sidebar Menu + top-bar ApplicationTitle). |
| C2 | Component Reuse | Delightful (3) | 1× | Tag, Tabs, Wizard, Columns*, TableRecords, Card, Alert, Pagination, Sidebar are real OS UI blocks; the cc-table row markup is reused between WorkList and ClaimDetail. |
| C3 | Component Fit | Acceptable (2) | 1× | Tabs/Tag/Wizard fit. Misfits: Extraction tab badge shows a literal `0` instead of hiding; Status dropdown prompt renders `0`; Wizard marks every step "next" for status Received; ICD column is bound to BenefitCode (`INPATIENT_ROOM`) instead of `Claim.IcdCode`. |
| C4 | Color Contrast | Delightful (3) | 1× | White-on-#1E6FEB table headers and #1B2129 text on #FFFFFF/#F4F6F9 all ≥ 4.5:1. The Info Alert is a solid #6236C7 band with white text — passes, but heavier than the light-tint note in the design. |
| C5 | Tap / Click Targets | Unpleasant (1) | 1× | Measured on ClaimDetail: 29 interactive elements, **55 %** ≥ 44 px (tab pills ≈ 40 px, header sort links, "View" links). |
| C6 | Keyboard | Unpleasant (1) | 1× | Focused tab header: `outline: none`, `box-shadow: none` → no visible focus indicator on tabs, links or inputs. |
| C7 | Grid Adherence | Delightful (3) | 1× | Columns5 KV strip, Columns2 cards and ColumnsSmallLeft costs tab sit on the OS grid; the table card and KV strip share one left edge. |
| C8 | Margin & Padding | Acceptable (2) | 1× | 16 px card gaps are consistent, but: the "Work list" h1 and its subtitle run together on one line; "Proposed payable 0 ₫" wraps the ₫ to a second line; the Date column wraps "10/0 6"; the WorkList table does not fill its card (empty right band). |
| C9 | IA & App Depth | Delightful (3) | 1× | Sidebar with 8 links in 2 groups, WorkList → ClaimDetail with 8 tabs + assistant panel, breadcrumb back; 5 reference links still point at WorkList until Batch 5. |
| C10 | State Communication | Acceptable (2) | 1× | BlankSlate on History ("No log entries yet"), Tag statuses, pagination counter — but the counter is DUPLICATED ("1 – 4 of 4 items" and "1 to 4 of 4 items"); no loading state observed. |
| C11 | Animations | Acceptable (2) | 1× | 16 of 60 sampled controls carry the OS UI 0.18 s transition; no reduced-motion handling detected on console screens; tab switch is instant. |
| C12 | Micro-interactions | Acceptable (2) | 1× | Row hover (`cc-table tbody tr:hover`) and button hover exist; sort icons in headers; nothing beyond OS defaults. |
| C13 | Content & Data Quality | Unpleasant (1) | 1× | Real Vietnamese claim data from `Seed_Claims`, BUT the Batch 0/2/3 seeds did not attach to these claims: Proposed payable = 0 ₫ on every row, Run #1 = 0 ₫, no Recommendation card, History empty, Confidence "—" on every line. Money renders `1,800,000 ₫` (comma) instead of `1.800.000 ₫` — FormatDecimal separators are swapped. |
| C14 | Modern vs Dated | Delightful (3) | 1.5× | **4/5** — pill tabs, gradient table header, 12 px cards, Phosphor icons, breadcrumb + KV-strip header read as a current SaaS console; the duplicated brand and the raw `0` badge keep it from 5. |
| C15 | WoW & AI | Acceptable (2) | 1.5× | **3/5** — AI: claimed (assistant panel, recommendation card, 14-step chain) but on the audited claim none of it renders (no rec card, empty log, 0 ₫ chain), so the AI story is invisible; Craft solid, not surprising. |
| C16 | Visual Identity | Delightful (3) | 1.5× | Palette authored (#1E6FEB / #0F8A86 / #1F9D57, teal login); Shape & depth authored (12 px radius, soft shadow, pill tabs); Iconography authored (Phosphor + cc-logo mark); **Typography default** (Inter never loads → system font) → 3 of 4 dimensions authored. |

## Notable findings (one root cause = one finding)
1. **Demo data not attached to the seeded claims** (C13, C15, C3, C8 converge). `Seed_Claims` creates CLM-2026-0001..4 with its own names/items; the Batch 0/2/3 seeds guard on entity-level `Count > 0` — `ChainRun` and `AgentRun` already had rows from the earlier app, so ChainRun, CalcSnapshot, AgentRun (and apparently Recommendation) were skipped, and `Claim.TotalPayable` was never set. Result: Proposed payable 0 ₫, Run #1 0 ₫, no recommendation card, empty log.
2. **FormatVND separators swapped** (C13): renders `1,800,000 ₫`; must be `1.800.000 ₫` → `FormatDecimal(Amount, 0, ",", ".")`.
3. **Brand rendered twice** (C1, C14): the Menu block carries cc-brand AND Common/ApplicationTitle renders in the top bar.
4. **No focus indicator** on tabs/links/inputs (C6); 55 % of controls under 44 px (C5).
5. **Rendering defects** (C3, C8, C10): Extraction badge shows `0`; Status dropdown prompt shows `0`; h1 + subtitle inline; `₫` wraps in Proposed payable; Date column wraps; ICD column bound to BenefitCode; duplicated pagination counter; Wizard never marks Past/Active.
6. **Inter font not loaded** (C16 Typography default, C1).

## Mentor handoff (target: Delightful ≥ 65 %)
1. **[C13/C15] Attach the seeds to the existing 4 claims** — change every seed guard from `Count(Entity) > 0` to `Count(Entity where ClaimId = <claim>) > 0`; in `Seed_AssessmentAndTerms` set `Claim.TotalPayable = ChainRun.TotalPayable` for the 4 demo claims (seed-only write); verify ChainRun / CalcSnapshot / Recommendation / AgentRun rows exist for ClaimId 17 after `RunSeedBatch0Now`.
2. **[C13] FormatVND** — `FormatDecimal(Amount, 0, ",", ".")` (decimal `,`, group `.`) in the client function and in FormatVND_Server.
3. **[C1/C14/C16] One brand + real font** — keep cc-brand in the sidebar Menu, empty Common/ApplicationTitle (or hide `.header .application-title` at ≥ tablet); add `@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap')` at the top of the theme so `--font-family` resolves.
4. **[C6/C5] Focus + targets** — theme: `.osui-tabs__header-item:focus-visible, a:focus-visible, .btn:focus-visible, input:focus-visible{outline:2px solid var(--color-primary);outline-offset:2px}`; `.cc-tabs .osui-tabs__header-item{min-height:44px}`; `.cc-table th a, .cc-lnk{min-height:44px;display:inline-flex;align-items:center}`.
5. **[C3/C8/C10] Rendering defects** — hide the Extraction badge when 0; Status dropdown prompt "All statuses"; `.cc-page-sub{display:block}`; wrap Proposed payable in the `<b>` with `white-space:nowrap`; Date column `white-space:nowrap`; ICD column → `Claim.IcdCode`; remove the custom "1 – 4 of 4 items" text (keep Pagination's own counter); Wizard status from `ClaimStatus.Order`.

## Method
Chrome (user's own session) at 1568×772: screenshots of WorkList and ClaimDetail (General, Extraction & costs); JS probe for tap-target boxes, focus styles, fonts and CSS variables; DOM check of all 8 tab panels. Mobile capture failed after resize (renderer timeout) — the Login mobile stacking was verified only in the earlier Playwright run. Read-only; the app was not modified.
