# Runtime UI Audit #9 — ClaimMVP (claim console) · rev 87 → 91 · 2026-09-20

**Result: 86 % — Market Leading** (numerator 60 / denominator 70, 0 N/A).

> **Arithmetic correction of audits #7 and #8.** Both earlier reports summed the weighted tiers one point too high: the rev 84 table (11 × Delightful at 1×, C5 + C13 Market Leading at 1×, C14/C15 ML + C16 Delightful at 1.5×) is 57.5 / 70 = **82 %**, not 84 %; the rev 86 table (same, C16 → ML) is 59 / 70 = **84 %**, not 86 %. The tiers themselves were right; only the totals were off. This audit re-derives the total from the table below, and rev 89–91 add the one criterion (C6) that lifts the app to a genuine ≥ 85 %.

Scope of this run: the customer portal was moved out of this app (rev 87: 7 Portal screens, PortalMenu, portal chatbox and the Customer role deleted; portal now lives in **MyInsurance**, see `../bnk-dev-myinsurance/runtime-audit-rev7.md`). Re-crawled: WorkList, Dashboard, ClaimDetail (claim 17, assistant box), Login redirect. Shots: `shots-rev88/`.

| # | Criterion | Tier | W | Evidence |
|---|---|---|---|---|
| C1 | Theme & Styling | Delightful (3) | 1× | Unchanged skin (teal rail #0C3036, mint accent, Inter/Manrope, ccicons); no `#1068eb` / `#f3f6f8` hits; portal removal left no dangling widgets (`newClaimVisible:false` on all console screens, `NewClaimBtn` now role-gated instead of CSS-hidden). |
| C2 | Component reuse | Delightful (3) | 1× | Unchanged. |
| C3 | Component fit | Delightful (3) | 1× | Unchanged. |
| C4 | Contrast | Delightful (3) | 1× | Unchanged (section labels ≈ 4.4:1 still the borderline pair). |
| C5 | Tap / Click Targets | Market Leading (4) | 1× | **100 %**: WorkList 30/30, Dashboard 21/21 (Highcharts a11y proxies excluded), ClaimDetail 38/38 after the `Technical trace` link got `cc-tap` (was 101×17). |
| C6 | Keyboard | **Market Leading (4)** | 1× | Rev 89–91: `Skip to content` link (`a.cc-skip`, appears on focus, moves focus to `#b1-MainContentWrapper` via client action — no URL change; template `skip-nav` duplicate removed); assistant box `role=dialog` + `aria-label`, list `aria-live=polite`, FAB `aria-expanded` / `aria-controls` (verified true/false), Escape closes the box and returns focus to the FAB (document listener, verified by dispatching `keydown Escape`), focus moves into the box on open (chat input, or the selected tab when Activity is default); header search `aria-label="Search claims"`; designed focus rings on every control. |
| C7 | Grid & alignment | Delightful (3) | 1× | Unchanged. |
| C8 | Spacing | Delightful (3) | 1× | Unchanged. |
| C9 | IA & depth | Delightful (3) | 1× | 10 console screens + Technical trace group; the "+ New claim" affordance now opens MyInsurance in a new tab only for non-console roles. |
| C10 | State communication | Delightful (3) | 1× | Unchanged. |
| C11 | Animations | Delightful (3) | 1× | Unchanged (reduced-motion honoured). |
| C12 | Micro-interactions | Delightful (3) | 1× | Unchanged. |
| C13 | Content & data quality | Market Leading (4) | 1× | Unchanged; WorkList now lists 5 seeded claims (0002 Dental "Processing documents", 0004 "Document processing error") consistent with the portal. |
| C14 | Modern vs dated | Market Leading (4) | 1.5× | 5/5. |
| C15 | WoW & AI | Market Leading (4) | 1.5× | 5/5. |
| C16 | Visual identity | Market Leading (4) | 1.5× | Palette, typography, shape & depth, iconography (ccicons) authored. |

Score: 1× rows = 10 × 3 + 3 × 4 = 42; 1.5× rows = 3 × 4 × 1.5 = 18 → **60 / 70 = 86 %**.

## Findings
1. Portal split verified: no console screen references a deleted Portal screen; entities/static entities public; service actions consumed by MyInsurance (rev 87–88).
2. `Technical trace` link under 44 px on ClaimDetail → `cc-tap` (rev 88).
3. Keyboard: first skip-link attempt used an `href="#…"`, which the SPA router turned into a navigation to the root screen; fixed with an OnClick client action (rev 90). Focus-on-open initially ran before the box was visible / while the Activity tab was showing (rev 90–91).
4. Open, not scored down: mobile 390 px not captured (maximised 1920 px Chrome).

## Method
Chrome (tab 717093198) 1920×945; per-screen JS probe (visible controls ≥ 44×44, transitions, reduced-motion, `:focus-visible`, OS-default colours); skip link / FAB / Escape verified with `activeElement` + `aria-expanded` read-back. Companion: `runtime-audit-rev86.md` (audit #8), `../bnk-dev-myinsurance/runtime-audit-rev7.md`.
