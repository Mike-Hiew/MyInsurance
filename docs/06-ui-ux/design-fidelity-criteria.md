# Design-fidelity audit — ClaimMVP (rev 27) vs. Prototype v6.1 mockups

Screens compared (mockup rendered from `E:\Hackaton\batch\*.html`, app from `https://bnk-dev.outsystems.app/ClaimMVP`):
`fidelity/mock-dashboard.jpg` ↔ `app-dashboard.jpg` · `mock-claimdetail.jpg` ↔ `app-claimdetail.jpg` · `mock-portal-home.jpg` ↔ `app-portal-home.jpg` · `app-worklist.jpg` (mockup Work list captured live).

## Root cause — why style / colour / form drifted

The prototype's CSS has **two layers**: the base tokens at `:root` (`--primary #1E6FEB`, blue table header, 12 px cards, white sidebar) and, appended at the end, a **`/* ===== MODERN SKIN ===== */` block** that *overrides* them — dark-teal sidebar, pill buttons in `#0c3036`, flat white table header, solid status pills, Manrope headings, 16 px border-less cards. The cascade makes the **skin** what you actually see.

The Batch 1 spec (and the theme Mentor built) was derived from the **base** layer, so the app renders the *un-skinned* prototype: blue gradient table header, white sidebar with blue active link, blue rounded-8 px buttons, tinted (not solid) status tags, 12 px bordered cards, Inter everywhere. Every later batch inherited that theme. This is a spec error, not a Mentor error — the fix is a **theme-level restyle** (one CSS batch) plus a few widget swaps.

## Fidelity criteria (measurable) — F1…F12

Each criterion = what the mockup renders → what the app renders → acceptance check. ✔ = already matches.

| # | Element | Mockup (effective skin) | App rev 27 | Acceptance check |
|---|---|---|---|---|
| F1 | **Sidebar** | 264 px, `radial-gradient(120% 70% at 0% 0%, #12454a 0%, #0c3036 45%, #082227 100%)`, white text (80 %), labels 42 % white, active item `rgba(255,255,255,.13)` + `inset 3px 0 0 #35d3a3`, nav radius 12 px, brand name white, footer border `rgba(255,255,255,.09)` | White sidebar, dark text, blue tint active, 200 px | Computed `background-image` of the menu contains `#0c3036`; active link `box-shadow` contains `#35d3a3`; width 264 px at ≥ 1024 px |
| F2 | **Logo / brand** | 34 px square, radius 9 px, `linear-gradient(135deg,#1E6FEB,#4E8CF5)` (console) / teal for portal, shadow `0 6px 16px rgba(0,0,0,.3)`, name 800 weight | Similar square, but on white; name 700 | Gradient present; brand name `font-weight ≥ 800` |
| F3 | **Typography** | Body Inter 14 px / 1.5; **headings Manrope 800** (`.page-title` 22 px, `-.01em`; `h3.h` 14 px 800; KPI values 800) | Inter everywhere, h1 ~20 px 700, h2 24 px | Google Fonts import includes `Manrope:wght@700;800`; `h1,h2,h3,.cc-k-val,.pt-k-val,.cc-big,.pt-big` computed `font-family` starts with `Manrope`, weight 800 |
| F4 | **Top bar** | White, 16 px 28 px padding, title left, right cluster: round 40 px icon buttons (`border-radius:50%`, 1 px `#E4E7EC` border), **pill search** 250 px (`border-radius:999px`, bg `#FAFBFD`), lang toggle "VI", theme toggle, bell with dot | Only a floating search box centred; no icon cluster; title lives in the content area | Search input `border-radius ≥ 999px`; ≥ 3 round icon buttons (40×40) right-aligned; page title + subtitle inside the header row (h1 22 px + 12.5 px muted sub) |
| F5 | **Buttons** | `.btn` pill `999px`, 10 px 18 px, 600 weight 13.5 px; **primary `#0c3036`** (hover `#0f4149`), ghost = white + `#E4E7EC` border; icon 16 px before label | Blue `#1E6FEB`, radius 8 px, ghost = blue outline | Primary computed `background-color rgb(12,48,54)`, `border-radius 999px`; ghost has neutral border and dark text |
| F6 | **Cards** | `border:none`, **radius 16 px**, shadow `0 1px 2px rgba(16,24,40,.05), 0 8px 22px rgba(16,24,40,.05)`, padding 18–20 px | 12 px radius + 1 px border, hard-ish shadow | Card computed `border-width 0`, `border-radius 16px` |
| F7 | **Tables** | **Flat white header**: `th` bg white, colour `#6E7787`, 12.5 px 700, no uppercase, bottom 1 px `#E4E7EC`; rows 13 px 14 px padding; sort caret ⇅ muted; claim no. mono 12 px muted; secondary line (policy) mono 11.5 px | Blue gradient header, white uppercase text, blue link colour for claim no. | `th` computed `background-color rgb(255,255,255)`, `text-transform none`, colour `#6E7787`; no gradient |
| F8 | **Status pills** | `.badge-status` **solid**: radius 999 px, 5 px 12 px, 11.5 px 700, **white text**; `st-g #1F9D57`, `st-y #C77700`, `st-r #D33A3A`, `st-b #1E6FEB`, `st-i #6236C7`, `st-n #9AA2AE`; a second pill for the sub-state (e.g. "Proposal pending" purple) | Tinted Tag (light bg + coloured text), single pill, internal labels ("UnderReview") | Tag computed `color rgb(255,255,255)` + solid bg from the list; label text uses the mockup wording ("Under assessment", "Paid", "Awaiting information") |
| F9 | **Age chip / secondary tags** | `.pill` 999 px, `#FAFBFD` bg, `#E4E7EC` border, 12 px 600 muted (age "2 days" in tint green/amber) | ok (tinted) — colour mapping off for "66 s"/"today" | Chip radius 999 px; green for < 1 day, amber ≥ 2 days |
| F10 | **KPI cards (Dashboard, Portal)** | Card with **tinted gradient background** per KPI (`linear-gradient(135deg, tint, #fff)`), coloured **icon square** 34 px radius 10 px (amber/purple/blue/green), value 28 px Manrope 800, label 12.5 px muted, money sub 13 px 700, trend green | Plain white card, small outline icon, value 26 px, label inline-fixed | Each KPI card has an icon square with solid colour bg and white glyph; card `background-image` is a gradient; value weight 800 |
| F11 | **Tabs / Wizard / Stepline** | Claim-detail tabs: underline tabs, active `border-bottom 2px #1E6FEB`, small mono code "F45" muted, count Badge amber; stepline: 26 px circles, done = green filled ✓ (`#1F9D57`), current = dark filled with number, line green up to current | Pill tabs (OS UI default), wizard circles hollow, all "next" | Active tab has 2 px bottom border, not a pill; step 1–2 rendered done (green ✓), step 3 active |
| F12 | **Portal specifics** | Same dark sidebar (logo teal "MI"), **"+ New claim" pill dark teal in the top bar**, KPI cards tinted teal/blue/amber, status pill solid amber "Under assessment", Main benefits ProgressBar green gradient (`linear-gradient(90deg,#1F9D57,#35d3a3)`), "See all benefits" pill ghost full width | Light sidebar, blue "New claim" rectangle, no ProgressBars, tinted tags | Sidebar as F1; ProgressBar fill uses the green gradient; the 3 benefit rows each have a bar |

Also: **hero band** (portal "My cover") = gradient `linear-gradient(135deg,#e8f0fe,#fff)` with the HC logo square + "Certificate" ghost pill (app has no logo/button); **inline `.note`** = left 4 px primary border on `#FAFBFD` (app Alert is a solid purple band → keep `cc-alert-soft` but apply everywhere).

## Fix plan (one Mentor batch, theme-first)

1. Theme tokens (Template_WebApp): add `--color-primary-dark:#0c3036`, `--pt-teal-accent:#35d3a3`, import Manrope, set the heading font rule, card/button/table/tag rules above (F3, F5, F6, F7, F8, F9).
2. Layout blocks: LayoutSideMenu sidebar background + link styles (F1, F2); header right cluster in `Common/ApplicationTitle`/`UserInfo` → move page title into header, add pill search + 3 round icon buttons (F4).
3. Widgets: status Tag → class `cc-st cc-st-g|y|r|b|i|n` with the mockup wording via a client function `StatusLabelForUi(label)`; KPI cards get `cc-kpi cc-kpi-amber|purple|blue|green` + icon square; ProgressBars on portal; tabs switch to the underline variant (`osui-tabs` with `is-justified`, remove pill CSS) (F8, F10, F11, F12).
4. Verify each acceptance check with the JS probe below; re-run the runtime UI audit.

Probe (run on any screen): `getComputedStyle(document.querySelector('.app-menu')).backgroundImage`, `getComputedStyle(document.querySelector('.btn-primary, .btn.btn-primary')).backgroundColor`, `getComputedStyle(document.querySelector('th')).backgroundColor`, `getComputedStyle(document.querySelector('.card')).borderRadius`, `getComputedStyle(document.querySelector('h1')).fontFamily`.
