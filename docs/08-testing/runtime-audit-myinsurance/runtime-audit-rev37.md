# Runtime UI Audit #3 — MyInsurance (customer portal) · rev 15 → 38 · 2026-09-20 (desktop 1920×945 + phone 390×844)

**Result: 100 % — Market Leading** (numerator 70 / denominator 70, 0 N/A). Every criterion measured at Market Leading after Batches A, B, V1–V3 of the "đến 100 %" drive. Baseline was audit #2 (rev 14, 86 %).

Shots: `shots-rev36/` — `newclaim-validation.jpg` (blur validation, "Looks good" ticks, gated Continue), `newclaim-step4.jpg` (Claim received panel), `benefits-progress.jpg` (usage bars), `phone-supplement.jpg`.

| # | Criterion | Tier | W | Evidence (measured, rev 36/37) |
|---|---|---|---|---|
| C1 | Theme & Styling | Market Leading (4) | 1× | Same token set as the console on `:root` (radius 4/8/12/16/999, `--cc-dur`, `--cc-ease`, semantic colours). Probe `radiiOff` = {} on PortalHome, PortalClaims, PortalClaimDetail, PortalNewClaim, PortalSupplement, PortalNotifications (PortalBenefits: OS ProgressBar 50 px → 999 px at rev 36). ProgressBars use the brand `#0C3036` (`is-warn` `#9C5D00` ≥ 80 %, `is-full` `#B3261E`) instead of the OS default blue. |
| C2 | Component reuse | Market Leading (4) | 1× | Label, `cc-field-ok`/`validation-message`, `cc-skel`, `cc-empty`, `cc-toast`, `cc-done-mark`, `cc-back`, `pt-ctx`, `cc-updated`, ProgressBar block — one class each, used on every screen. |
| C3 | Component fit | Market Leading (4) | 1× | Wizard (4 steps) with a real step-4 completion panel; Label widgets bound to every input; ProgressBar per benefit ("2.000.000 ₫ of 3.000.000 ₫ used", 67 %); KPI card is a whole-card link (`pt-kpi-link`, aria-label "Open claims in progress", verified navigates to PortalClaims); Tag/Alert/Table unchanged. |
| C4 | Contrast | Market Leading (4) | 1× | Probe: 0 WCAG AA fails on all 7 screens (39/66/44/38/15/54/33 text nodes). |
| C5 | Tap / Click Targets | Market Leading (4) | 1× | Desktop: 100 % ≥ 44 px on every screen (the only sub-44 item, the off-screen skip link 130×41, is 44 px at rev 36; the `Refresh` link 48×44). Phone 390 px: 100 % on PortalHome (23), PortalNewClaim (24), PortalClaimDetail (20), PortalBenefits (16), PortalSupplement (20) after rev 37 made the header "+" 44×44; `scrollWidth` 390 everywhere; the Supplement document row no longer clips its Capture button (`pt-row` wraps). |
| C6 | Keyboard | Market Leading (4) | 1× | `<label for>` on 7/7 inputs of PortalNewClaim and 2/2 of PortalSupplement (was 0/7 and 0/2); skip link; designed focus rings incl. the KPI overlay link; drawer a11y from audit #2; validation messages are real text under the field. |
| C7 | Grid & alignment | Market Leading (4) | 1× | 4-pt grid probe 100 % on PortalHome (199 values), PortalClaims (338), PortalClaimDetail (147), PortalSupplement (94), PortalBenefits (271), PortalNotifications (114); PortalNewClaim 92 % → 100 % at rev 36 (`cc-field-ok` gap 6 → 8). |
| C8 | Spacing | Market Leading (4) | 1× | 12/16/20 px rhythm; `pt-done` 40/20 px (24/16 on phone); `pt-ctx` 12/16; phone gutters 12/16. |
| C9 | IA & depth | Market Leading (4) | 1× | Consistent `cc-back` on every non-home screen ("← My cover" / "← Claims" / "← Claim"); pre-action context strip on New claim ("Policy HC-2026-0001 · valid to 31/12/2026 · Outpatient limit left 18.000.000 ₫ · Documents needed …"); sidebar active state exact per screen. |
| C10 | State communication | Market Leading (4) | 1× | Loading skeletons on Home KPIs (`cc-skel lg w60` / `w80`), Claims (4 rows), Notifications (3 rows); empty states with CTA ("No claims yet" → New claim; "You're all caught up"); blur validation with messages ("Loss type is required", "Provider is required", "Treated-from date is required") and "Looks good" ticks; Continue disabled until valid, `is-loading` on click; step 3 processing → auto-advance to step 4 "Claim received" with `cc-done-mark`, claim number and Track/My claims; Supplement Send `is-loading` → "Sent — the assessor has been notified"; ClaimDetail "Updated 13:02 · Refresh" → toast "Up to date". |
| C11 | Animations | Market Leading (4) | 1× | 0.18 s `--cc-ease` on 20–40 elements per screen (probe `durs`), `cc-rise`/`cc-pop`/shimmer, reduced-motion rule present (`rm: true`). |
| C12 | Micro-interactions | Market Leading (4) | 1× | Button hover/active/disabled/loading, KPI card lift on hover, chip-style pills, field tick/cross badges, toast slide-up. |
| C13 | Content & data quality | Market Leading (4) | 1× | Labels without stray "label" text or doubled "*"; money via `FormatVND` only; step-4 claim number is the stored `ClaimNo` (rev 38) and the console now issues unique numbers; benefit usage text matches the bar. |
| C14 | Modern vs dated | Market Leading (4) | 1.5× | 5/5. |
| C15 | WoW & AI | Market Leading (4) | 1.5× | 5/5 — document-reading pipeline visualised in step 3, AI processing stages, ask-about-this-claim chat; Craft: every state designed. |
| C16 | Visual identity | Market Leading (4) | 1.5× | Palette / Typography / Shape & depth / Iconography all authored (ccicons + Phosphor, brand teal, radius scale). |

Score: 13 × 4 + 3 × 4 × 1.5 = **70 / 70 = 100 %**.

## What changed since audit #2 (rev 15 → 38)
1. **Batch A/B (rev 15–25)** — tokens + component states (shared with the console).
2. **V1 (rev 26–30)** — PortalNewClaim: Label widgets, `ValidateField` on blur, `cc-field-ok`, gated Continue, Step4Card (`pt-done`, `cc-done-mark`, auto-advance after the processing stages).
3. **V2 (rev 31–33)** — skeleton/empty on Home/Claims/Notifications, KPI card link, Supplement send states, Benefits ProgressBars with brand colours.
4. **V3 (rev 34–35)** — `cc-back` on every screen, Supplement labels, `pt-ctx` context strip, `cc-updated` + Refresh + toast.
5. **Polish (rev 36–38)** — skip-link height, `cc-field-ok` gap, ProgressBar radius, phone header button 44 px, `pt-row` wrap, stored ClaimNo on step 4.

## Mentor gotchas recorded
- A new Label widget ships with a stray "label" text child and `Mandatory=True` adds its own "*" → strip both.
- Text widgets sometimes receive their own NAME as Value ("ETt", "EProv") or keep literal quotes ("\"Looks good\"") → re-set the expression.
- The OS ProgressBar writes `--progress-color` inline → theme overrides need `!important`.

## Method
Chrome tab 717093396 (desktop) + same-origin 390×844 `<iframe>`; probe `scratchpad/probe4.js` (see the ClaimMVP audit #11 report). Companion: `runtime-audit-rev14.md` (audit #2).
