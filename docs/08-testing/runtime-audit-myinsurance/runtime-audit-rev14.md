# Runtime UI Audit #2 — MyInsurance (customer portal) · rev 9 → 14 · 2026-09-20 (desktop + phone)

**Result: 86 % — Market Leading** (numerator 60 / denominator 70, 0 N/A). Same tiers as audit #1; this run adds the **390×844 phone viewport** (same-origin iframe technique, see the ClaimMVP audit #10 report), finds the phone layout broken and fixes it (rev 10–14) before scoring.

Shots: `shots-rev12/` (`before-rev9-phone-portalhome.png` → `phone-portalhome.png`, `phone-drawer-rev13.png`, `phone-portalclaims.png`).

| # | Criterion | Tier | W | Evidence |
|---|---|---|---|---|
| C1 | Theme & Styling | Delightful (3) | 1× | **Phone before rev 10**: fixed 264 px rail, content 111 px with one-letter-per-line text, header actions past 390 px. **After rev 14**: drawer (`cc-drawer`, 280 px teal, backdrop), header fits (toggle · title with ellipsis · + · VI · dark · bell), subtitle hidden, `pt-big` 26 px so "2.000.000 ₫" no longer breaks. |
| C2 | Component reuse | Delightful (3) | 1× | Unchanged. |
| C3 | Component fit | Delightful (3) | 1× | Unchanged (ColumnsMediumRight, Table, Tag, Alert). |
| C4 | Contrast | Delightful (3) | 1× | Unchanged. |
| C5 | Tap / Click Targets | Market Leading (4) | 1× | Desktop 100 % on all 7 screens; phone PortalHome 12/12 after `.phone .cc-hdr-actions .btn-primary{width:44px}` (was 40×44), PortalClaims 12/12. |
| C6 | Keyboard | Market Leading (4) | 1× | Audit #1 items hold; phone drawer `aria-expanded` verified false → true → false across open / close (rev 14 removed the widget attribute binding that overwrote the JS value on every render); Escape / outside click close it; header icon buttons labelled. |
| C7 | Grid & alignment | Delightful (3) | 1× | Phone: KPI cards stack full-width, claims table scrolls inside the card (`min-width:760px`, `overflow-x:auto`), `scrollWidth 390`. |
| C8 | Spacing | Delightful (3) | 1× | Phone padding 12/16 px, 96 px bottom room, chatbox stretches to `left:8px; right:8px`. |
| C9 | IA & depth | Delightful (3) | 1× | Sidebar active state fixed: each item highlights only on its own screen (rev 12; `New claim` keeps its CTA styling by design). |
| C10 | State communication | Delightful (3) | 1× | Unchanged (status-bound stepline / Tag / Alert / Notifications). |
| C11 | Animations | Delightful (3) | 1× | Drawer transitions, reduced-motion respected. |
| C12 | Micro-interactions | Delightful (3) | 1× | Unchanged. |
| C13 | Content & data quality | Market Leading (4) | 1× | Unchanged. |
| C14 | Modern vs dated | Market Leading (4) | 1.5× | 5/5. |
| C15 | WoW & AI | Market Leading (4) | 1.5× | 5/5. |
| C16 | Visual identity | Market Leading (4) | 1.5× | Unchanged (ccicons). |

Score: 10 × 3 + 3 × 4 + 3 × 4 × 1.5 = **60 / 70 = 86 %**.

## Findings (fixed rev 10–14)
1. Phone layout broken (theme shared with the console) — same root cause and fix as ClaimMVP audit #10; plus `+ New claim` becomes a 44 px "+" pill with `aria-label="New claim"`.
2. Drawer panel empty (OS `app-menu-content` off-canvas) → rev 12; toggle could not close (listener race) → rev 13; `aria-expanded` inverted (widget binding vs JS) → rev 14.
3. Sidebar "New claim" + "Claims & progress" both active on PortalClaims → exact-screen ActiveItem index (rev 12).
4. Note for the tester: the Chrome tab renderer occasionally stalls on `Page.captureScreenshot` after many iframe injections; DOM read-backs stay reliable, and a fresh tab clears it.

## Method
Chrome tabs 717093194 / 717093389, 1920×945 desktop + 390×844 same-origin iframe; probes as in the ClaimMVP report. Companion: `runtime-audit-rev7.md` (audit #1).
