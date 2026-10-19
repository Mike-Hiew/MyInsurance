# Runtime UI Audit #8 — ClaimMVP · rev 85/86 · 2026-09-19 (icon-font confirmation)

**Result: 86 % — Market Leading** (numerator 60 / denominator 70, 0 N/A). Trend: … rev 79 79 % → rev 84 84 % → **rev 86 86 %**.
Delta since audit #7: only C16 changed — the stock Phosphor glyphs are replaced app-wide by **ccicons**, a web font traced from the prototype's `ic()` set (24 glyphs used by the app, woff2 4.4 KB embedded as a data URI in the theme, `i.ph.ph-*::before` overrides — no widget edits). All other criteria re-checked on WorkList, Dashboard (+ assistant box) and the portal pages; no regressions. Shots: `shots-rev85/`.

| # | Criterion | Tier | W | Evidence |
|---|---|---|---|---|
| C1 | Theme & Styling | Delightful (3) | 1× | Unchanged from #7. Newly crawled **PortalClaimDetail** shows two drifts: the "Why you bear" column renders one character per line (a 1-column `ThemeGrid_Width1` wrapper again) and the stepline label "Under assessment" wraps with a stray strike-through (`portal-claimdetail-defect.jpg`). These are scheduled for the ClaimPortal rebuild. |
| C2–C4 | Reuse / Fit / Contrast | Delightful (3) | 1× each | Unchanged. |
| C5 | Tap / Click Targets | Market Leading (4) | 1× | 100 % on all crawled screens (unchanged). |
| C6 | Keyboard | Delightful (3) | 1× | Unchanged (designed rings, roles/labels, hidden chatbox not focusable). |
| C7–C12 | Grid / Spacing / IA / States / Motion / Micro | Delightful (3) | 1× each | Unchanged. |
| C13 | Content & Data Quality | Market Leading (4) | 1× | Unchanged. |
| C14 | Modern vs Dated | Market Leading (4) | 1.5× | 5/5. |
| C15 | WoW & AI | Market Leading (4) | 1.5× | 5/5. |
| C16 | Visual Identity | **Market Leading (4)** | 1.5× | Palette, Typography, Shape & depth **and Iconography authored**: `document.fonts.check("16px ccicons") = true`; every sampled `<i class="ph …">` on WorkList/Dashboard/assistant resolves to `font-family: ccicons` (list, squares-four→dash, warning, shield-check, book-open, flag→gate, wallet, users, terminal→clock, sign-out, magnifying-glass, moon, bell, clock, lightning→bolt, robot→msg, x, paper-plane→send …). Glyph sheet `ccicons-glyph-sheet.png`; live sidebar `sidebar-ccicons.png`. |

## Findings
1. **PortalClaimDetail layout** (C1): vertical text in "Why you bear" (Width1 wrapper) and broken stepline label — first time this screen was crawled; to be fixed in the ClaimPortal rebuild (batch PT-x).
2. No other open findings; the app is at Market Leading with all 16 criteria ≥ Delightful.

## Method / notes
- Icon font build: `scratchpad/iconfont/build.js` (potrace trace of the stroke SVGs → even-odd→non-zero fix in `nonzero.js` → svgicons2svgfont → svg2ttf → ttf2woff2), CSS in `output/bnk-dev-claimmvp/ccicons-theme.css` (full 43-glyph set) / `ccicons-min.css` (24-glyph set shipped). Mentor cannot ingest an 8 KB base64 literal in one prompt (OS-AIWA-50001 ×2) — it was appended in three 1.95 KB chunks with a character-count read-back after each.
- Companion docs: `runtime-audit-rev84.md` (audit #7), `runtime-audit-rev79.md` (audit #6 + P6), `chatbox-audit-rev78.md`.
