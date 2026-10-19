# Runtime UI Audit #6 — ClaimMVP · rev 79 · 2026-09-19

**Result: 79 % — Delightful** (numerator 55.5 / denominator 70, 0 N/A). Trend: rev 12 57 % → rev 30 69 % → rev 47 71 % → rev 55 74 % → rev 63 78 % → rev 65 78 % → **rev 79 79 %**.
Surfaces crawled: WorkList, Dashboard, ClaimDetail (claims 17 + 19: General · Costs · Calculation), Checkpoints, OpsQueue, AgentTrace, PortalHome, PortalNewClaim, PortalClaims; console assistant (FAB + Chat/Activity box) and portal chatbox. Captured through the user's logged-in Chrome session at 1920×945; JS probes for tap targets, transitions, `prefers-reduced-motion`, `:focus-visible`, scroll state, hidden-focusable controls. Mobile not captured. Read-only. Shots: `shots-rev79/` (+ `shots-rev65/` for the rev 78 assistant pairs).

## What changed since audit #5 (rev 65 → 79)
Settlement split fixed (ROOM 600.000 / OTHER 1.200.000), calc-tab rule details, "Payable" wording, Continue button, sidebar menus restyled to the prototype (both apps), full-width header, console assistant rebuilt as a single FAB → Chat | Activity box, chatbox audit fixes (Enter bug, dark controls, motion, a11y labels).

| # | Criterion | Tier | W | Evidence |
|---|---|---|---|---|
| C1 | Theme & Styling | Delightful (3) | 1× | Prototype skin on every surface incl. the 240×44 rail items, pill badges, 82 px brand, full-width 70 px header, floating dark assistant card (r16, pslide). Drift: an empty white `.cc-card.ThemeGrid_Width1` (99×36) sits between "My queue" and its table on Dashboard; **ClaimDetail opens pre-scrolled 274 px** (`.active-screen.scrollTop = 274` on load) so the claim header/KV strip is hidden until the user scrolls up. |
| C2 | Component Reuse | Delightful (3) | 1× | LayoutSideMenu + Menu, TableRecords, Tabs, Popup, ProgressBar, BlankSlate, Notification, Tag, UserAvatar, step cards; FAB/chatbox pattern shared by console and portal. |
| C3 | Component Fit | Delightful (3) | 1× | Activity tab = inbox counts + To confirm / Warning / Alerts (WorkList) and Proposals-first + Steps that ran (ClaimDetail); chat chips → data answers; low-confidence To-do on claim 19 (2 lines); calc rules with detail lines. |
| C4 | Colour Contrast | Delightful (3) | 1× | Unchanged (≥ 4.5:1 everywhere; chat bubbles ≈ 9:1; section labels rgba(255,255,255,.45) ≈ 4.4:1 borderline at 10.5 px). |
| C5 | Tap / Click Targets | Delightful (3) | 1× | WorkList **100 %**, ClaimDetail 97 %, OpsQueue 95 %, Checkpoints 94 %, AgentTrace 94 %, Dashboard 90 % (only Highcharts a11y proxies), PortalNewClaim 95 % / PortalClaims 95 % once the hidden chatbox is excluded. The one real residual on every screen: the sidebar **logout link 14×21 px**. |
| C6 | Keyboard | Acceptable (2) | 1× | Designed ring (`#1E6FEB` / `#35d3a3` in the chatbox), tabs `role=tab` + `aria-selected`, FAB/send/input labelled. **Defect:** the portal chatbox is hidden with `opacity:0; transform:scale(.95)` — its 6 controls stay focusable while invisible (keyboard trap into an invisible box). |
| C7 | Grid Adherence | Delightful (3) | 1× | Full-bleed content, 264/76 px rail, 28 px gutters, Columns4 KPIs, step-card row, Continue right-aligned. |
| C8 | Margin & Padding | Delightful (3) | 1× | 70 px header, 18–20 px cards, 44 px rail items, 12/14 px chatbox rhythm. |
| C9 | IA & App Depth | Delightful (3) | 1× | 10 console + 7 portal screens, Technical trace under its own group, breadcrumbs, F-codes, single assistant entry with routing rows. |
| C10 | State Communication | Delightful (3) | 1× | FAB badge (2), default Activity tab when work is pending, empty state "Nothing waiting for you", To-do bars, disabled Submit, BlankSlate, skeleton shimmer. Residual: chat body scroll-to-latest stops ~30 px short (console) and clips the reply (portal body ≈ 90 px). |
| C11 | Animations | Delightful (3) | 1× | 22 controls with 0.18 s transitions on WorkList; chatbox `pslide .18s`; FAB transform transition; `prefers-reduced-motion` handled. |
| C12 | Micro-interactions | Delightful (3) | 1× | Rail toggle persisted, chip hover #35d3a3, FAB hover lift, X ↔ robot, tab switch, row hover, sort carets. |
| C13 | Content & Data Quality | Market Leading (4) | 1× | Settlement now ROOM 600.000 / OTHER 1.200.000, calc steps with detail lines + payable amount, "Payable" decisions, 4 claims in 4 states over 9 weeks, trace 49 rows, inbox rows with claim · name · reason · time, data-driven chat answers. |
| C14 | Modern vs Dated | Market Leading (4) | 1.5× | **5/5** — dark-teal rail with pill badges, full-width header cluster, tinted KPIs, floating dark assistant with tabs, step cards, solid pills. |
| C15 | WoW & AI | Market Leading (4) | 1.5× | **5/5** — proposal card → Confirm/Reject with mandatory reason, low-confidence acknowledge, live agent steps, inbox routing, data-driven chat, badge-driven FAB. |
| C16 | Visual Identity | Delightful (3) | 1.5× | Palette / Typography / Shape & depth authored; Iconography still stock Phosphor (inline SVG not viable on this tenant) → 3 of 4. |

## Findings (one root cause each)
1. **ClaimDetail opens scrolled** (C1/C10): `.active-screen` is at scrollTop 274 right after load (resetting it holds; tab clicks do not re-scroll) — an init-time `scrollIntoView` (Tabs header) inside the screen container since the header became `position:fixed`.
2. **Invisible-but-focusable portal chatbox** (C6/C5): closed state uses opacity/transform only.
3. **Leftover empty card on Dashboard** (C1): `.cc-card.ThemeGrid_Width1` under "My queue".
4. **Logout link 14×21** (C5): the only sub-44 control left on every console/portal screen.
5. **Chat scroll-to-latest / portal body height** (C10): console stops 32 px short, portal body too small so the answer is clipped; "Needs confirmation" still lacks the pill background (class not applied).

## Mentor handoff — batch P6 (target: Market Leading ≥ 85 %)
1. **[C1/C10] ClaimDetail scroll** — in the LayoutSideMenu (or ClaimDetail) OnReady add `setTimeout(function(){var s=document.querySelector('.active-screen');if(s)s.scrollTop=0;},0)` and again at 150 ms; if the Tabs block exposes a "scroll into view on init" behaviour, disable it (evidence: "scrollTop = 274 on load").
2. **[C6] Portal chatbox closed state** — `.pt-chatbox:not(.is-open){visibility:hidden;pointer-events:none} .pt-chatbox.is-open{visibility:visible}` (keep the opacity/transform for the animation) (evidence: "6 controls stay focusable while invisible").
3. **[C1] Dashboard** — delete the empty `.cc-card.ThemeGrid_Width1` container between the "My queue" heading and the table (evidence: "99×36 white card").
4. **[C5] Logout** — make the footer sign-out link a 44×44 `.cc-icon-btn` with `aria-label="Sign out"` on both layouts (evidence: "14×21").
5. **[C10] Chat body** — after appending a message run the scroll on the next frame (`requestAnimationFrame(function(){b.scrollTop=b.scrollHeight})`) and set `.pt-chatbox .pt-chatbox-body{min-height:180px}`; add class `cc-tag-warn` to the "Needs confirmation" expression (evidence: "stops 32 px short / reply clipped").
Skipped: C16 (Iconography) — design-scope gap on this tenant (inline SVG escapes); would need an icon font.

## Method
Chrome (user session, tab 717093194) 1920×945 → 0.6-scale JPEGs in `shots-rev79/`; per-screen JS probes (tap-target boxes incl. `role=tab`, transitions, reduced-motion, focus-visible, scroll containers, hidden-focusable counts); real keyboard typing into both chat inputs. Companion docs: `chatbox-audit-rev78.md`, `runtime-audit-rev65.md` (+ addenda H2/M1/M2/P1–P4).

## Addendum — handoff P6 executed (rev 80–82, 2026-09-19)
| Item | Result | Verified in Chrome |
|---|---|---|
| 1. ClaimDetail pre-scroll | LayoutSideMenu OnReady `ScrollReset` (0 / 200 ms) | ✓ `.active-screen.scrollTop = 0`, breadcrumb at y 169 |
| 2. Portal chatbox keyboard trap | `.pt-chatbox:not(.is-open){visibility:hidden;pointer-events:none}` (+ console) | ✓ `visibility hidden`; PortalHome **100 %** targets |
| 3. Dashboard empty card | deleted | ✓ 0 elements between "My queue" and the table |
| 4. Sign-out 44×44 | console + portal `.cc-icon-btn[aria-label="Sign out"]`, transparent (rev 81), portal avatar wrapper pinned to 34 px so the name fits (rev 82) | ✓ 44×44 both; "Nguyễn Hoài An" / "Nguyễn Văn An" on one line |
| 5. Chat scroll-to-latest + tag | rAF scroll after append; `min-height:180px`; `cc-tag-warn` on "Needs confirmation" | ✓ `atBottom: true`; pill rgba(224,162,74,.18) r999 |

Re-score after rev 82: C1 3 (both drifts gone), C5 → **Market Leading (4)** (WorkList 100 %, PortalHome 100 %, ClaimDetail 97 %, others ≥ 94 % with only Highcharts a11y proxies below 44), C6 → **Delightful (3)** (trap removed; designed ring + labels + roles), C10 3 → numerator 58.5 / 70 = **84 % — Delightful** (one point short of Market Leading; the remaining gap is C16 Iconography, a design-scope item on this tenant).
