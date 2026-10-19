# Runtime UI Audit — https://bnk-dev.outsystems.app/ClaimMVP (2026-09-18, rev 10)

## Result: NOT SCORED — runtime is auth-gated

The landing URL redirects to `/Login` ("Log in"). Per the audit contract a login form is not scored as the app; the only public surfaces are **Login** and **RecoverPasswordRequest**. The authenticated surface (WorkList, roles ClaimOfficer/ClaimApprover) is **unverified**, not N/A.

## Observations on the public surface (Login) — evidence only, no tier

| Signal | Value | Note |
|---|---|---|
| OS default tells (#1068eb / #f3f6f8) | `osDefaults.present: false` | Theme override took effect |
| Fonts / accents / radii | 1 font · 4 accents · 2 radii | Coherent, matches prototype tokens |
| Tap targets ≥44px | **25 %** (1 of 4) | Inputs/links under 44px on desktop |
| Focus ring | `outlineStyle: none`, `boxShadow: none` | **No visible focus state** — prototype had a 2px mint box-shadow (`rgba(53,211,163,.55)`) |
| Motion | 3/4 elements with 0.18s transitions, reduced-motion handled | OK |
| Mobile 390×844 | **Two columns did not stack** — dark form column squeezed to 50 % width, title wraps to 3 lines | `Columns2.PhoneBehavior=All` was not applied |
| Right column illustration | Only a ~16px `files` icon + caption | `Icon size=96` not applied; prototype has a 440px SVG artwork |

Screenshots: `shots/desktop.png`, `shots/mobile.png`, `shots/focus.png`, `shots/screen-01-recoverpasswordrequest.png`. Probe: `shots/probe.json`.

## Fix list for the Login screen (for the next Mentor turn)
1. On Login, set `Columns2.PhoneBehavior = Entities.BreakColumns.All` so the form stacks on phone.
2. On Login, add a focus style to `.cc-login-form input:focus { box-shadow: 0 0 0 2px rgba(53,211,163,.55) }` (theme stylesheet) — currently no focus ring.
3. On Login, raise inputs/button/link to ≥44px height (`.cc-login-form input`, `.cc-login-btn` padding 14px already gives ~48px; the "Forgot password?" link needs `min-height:44px`).
4. On Login, right column: replace the 16px icon with a proper illustration (InlineSVG of the prototype `lg-svg` artwork or `Icon Size=Large` + `font-size:96px` class).

## Method
Playwright + system Chrome, desktop 1440×900 + mobile 390×844, shallow crawl (1 surface reachable), focus/hover/motion probe. Read-only.
