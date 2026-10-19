# Runtime UI Audit #10 — ClaimMVP (claim console) · rev 91 → 95 · 2026-09-20 (desktop + phone)

**Result: 86 % — Market Leading** (numerator 60 / denominator 70, 0 N/A). Same tiers as audit #9; this run adds the **390×844 phone viewport** that no earlier audit had captured, finds the phone layout broken, and fixes it (rev 92–95) before scoring.

Method note: the maximised Chrome window cannot be resized, so the phone view is a same-origin 390×844 `<iframe>` of the app injected into the desktop tab (OutSystems detects `body.phone`, real cookies/session, real CSS). Shots: `shots-rev94/` (`before-rev92-phone-worklist.png` → `phone-worklist.png`, `phone-worklist-toolbar.png`, `phone-drawer.png`, `phone-dashboard.png`).

| # | Criterion | Tier | W | Evidence |
|---|---|---|---|---|
| C1 | Theme & Styling | Delightful (3) | 1× | Desktop unchanged. **Phone before rev 92**: the 264/76 px rail stayed fixed (`.layout-side .main{margin-left}`), content 299 px, header actions overflowing to x = 544, filter labels one letter per line. **After rev 95**: rail becomes an off-canvas drawer (`.layout.cc-drawer`, 280 px, teal, dimmed backdrop), header 0–390 px, subtitle hidden, title ellipsis. |
| C2 | Component reuse | Delightful (3) | 1× | Unchanged; the drawer reuses the existing LayoutSideMenu / Menu / ILink and the OS `app-menu-content` off-canvas. |
| C3 | Component fit | Delightful (3) | 1× | WorkList filter `Columns6` now `PhoneBehavior = BreakAll` (block setting, not CSS). |
| C4 | Contrast | Delightful (3) | 1× | Unchanged. |
| C5 | Tap / Click Targets | Market Leading (4) | 1× | Desktop 100 % (WorkList 30/30, Dashboard 21/21, ClaimDetail 38/38). Phone: WorkList **19/19 = 100 %** (header icon buttons 44×44 after `.phone .header .cc-icon-btn`), Dashboard 10/10 excluding the two Highcharts a11y proxies. |
| C6 | Keyboard | Market Leading (4) | 1× | Audit #9 items hold; phone drawer: `aria-label="Toggle navigation"` + `aria-expanded` true/false verified after open and close (rev 95 fixed the race between the outside-click listener and the toggle action), Escape closes; language / dark-mode / bell buttons now have `aria-label`s. |
| C7 | Grid & alignment | Delightful (3) | 1× | Phone: KPI Columns4 stack to 287 px cards, table card scrolls horizontally (`.phone .cc-card{overflow-x:auto}`, table `min-width:760px`), no horizontal page scroll (`scrollWidth 390`). |
| C8 | Spacing | Delightful (3) | 1× | Phone padding 12/16 px, toolbar caption on its own line (`.phone .cc-toolbar{flex-wrap:wrap}`), 96 px bottom room for the FAB. |
| C9 | IA & depth | Delightful (3) | 1× | Unchanged. |
| C10 | State communication | Delightful (3) | 1× | Unchanged. |
| C11 | Animations | Delightful (3) | 1× | Drawer `transform .2s` + `left .2s`, disabled under `prefers-reduced-motion`. |
| C12 | Micro-interactions | Delightful (3) | 1× | Unchanged. |
| C13 | Content & data quality | Market Leading (4) | 1× | Unchanged. |
| C14 | Modern vs dated | Market Leading (4) | 1.5× | 5/5. |
| C15 | WoW & AI | Market Leading (4) | 1.5× | 5/5. |
| C16 | Visual identity | Market Leading (4) | 1.5× | Unchanged (ccicons). |

Score: 10 × 3 + 3 × 4 + 3 × 4 × 1.5 = 30 + 12 + 18 = **60 / 70 = 86 %**.

## Findings (fixed rev 92–95)
1. **Phone layout broken** (C1 / C7 / C8 at 390 px): fixed rail, overflowing header, 6-across filter, tables overflowing the card. Root cause: the console theme's desktop rail rules had no `.phone` counterpart and the persisted `cc.mini` rail state was re-applied on phone. Fix: `.phone` drawer rules appended to the theme, `RestoreMini` skips phone, the toggle action switches `cc-drawer` on phone, Columns6 → BreakAll.
2. **Drawer content hidden** on the first attempt: the OS `nav.app-menu-content` is its own off-canvas (`left:-248px`), so the aside transform alone showed an empty strip → `.cc-drawer … .app-menu-content{left:0!important}` (rev 94).
3. **Toggle could not close the drawer**: the document click listener closed it and the toggle action re-opened it → the listener now ignores the toggle button (rev 95).
4. Not scored: Highcharts a11y proxy buttons (69×17) are excluded by convention.

## Method
Chrome tabs 717093198 / 717093389, 1920×945 desktop + 390×844 same-origin iframe; per-screen JS probe (visible controls ≥ 44×44, `scrollWidth`, computed layout of `.aside-navigation` / `.app-menu-content` / `.main`, `aria-expanded` read-back after click and Escape). Companion: `runtime-audit-rev90.md` (audit #9), `../bnk-dev-myinsurance/runtime-audit-rev14.md`.
