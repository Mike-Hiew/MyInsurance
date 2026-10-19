# Runtime UI Audit #1 — MyInsurance (customer portal, split from ClaimMVP) · rev 7 → 9 · 2026-09-20

**Result: 86 % — Market Leading** (numerator 60 / denominator 70, 0 N/A) — 84 % at rev 7, C6 lifted to Market Leading in rev 8–9.
App: https://bnk-dev.outsystems.app/MyInsurance (key b883f015-a3c1-47a7-b66f-a1d8719258c6) — a clone of ClaimMVP reduced to the 7 portal screens, reading ClaimMVP's public entities and calling its service actions (Portal_GetCurrentMember2, Portal_SubmitClaim2, Portal_SendSupplement2, GetLedgerCounters2, GetCalcSnapshotData2, FormatVND). Role Customer (per-app, granted in ODC Portal). Crawled: PortalHome, PortalBenefits, PortalNewClaim, PortalClaims, PortalClaimDetail (claims 17 / 20), PortalSupplement, PortalNotifications; chatbox opened on PortalHome. Shots: `shots-rev6/`.

| # | Criterion | Tier | W | Evidence |
|---|---|---|---|---|
| C1 | Theme & Styling | Delightful (3) | 1× | Same authored theme as ClaimMVP (teal #0C3036 sidebar, mint #35D3A3 accent, Inter/Manrope, ccicons). Rev 3–6 fixed the three drifts found on first crawl: "Why you bear" one-character-per-line (Width1 wrapper), stepline title/sub wrapping, PortalClaims table letter-wrapping under the `.cc-table td a` inline-flex rule (`portalclaims-table-zoom.png`). No `#1068eb`/`#f3f6f8` hits (probe: 0). |
| C2 | Component reuse | Delightful (3) | 1× | Card/Tag/Alert/Table/Columns blocks reused from the console; portal rows are now Link widgets (`a.pt-claim-row`, `.pt-rowlink`). |
| C3 | Component fit | Delightful (3) | 1× | ColumnsMediumRight for the 2/3–1/3 claims split (prototype 1fr/360px); Table for claims; Tag for status; Alert for notes. |
| C4 | Contrast | Delightful (3) | 1× | Body text #1F2A2E on white, sub text `cc-sub` ≥ 4.5:1, Tag yellow-darker on yellow-lightest. |
| C5 | Tap / Click Targets | Market Leading (4) | 1× | **100 %** on all 7 screens (probe: 18/18 Home, 14/14 Benefits, 22/22 NewClaim, 20/20 Claims, 18/18 Detail, 18/18 Supplement, 14/14 Notifications). |
| C6 | Keyboard | **Market Leading (4)** | 1× | Designed focus ring (`solid 2px rgba(232,250,245,.82)`), scoped `:focus-visible` rules, row links reachable by Tab, back link `← Claims`; rev 8–9: `Skip to content` (`a.cc-skip`, focus → `#b1-MainContentWrapper` via client action, URL unchanged), chatbox `role=dialog` + `aria-label="Claims assistant"`, list `aria-live=polite`, FAB `aria-expanded`/`aria-controls`, focus moves to the chat input on open (verified `INPUT … Ask about your claims…`), Escape closes and returns focus to the FAB with `is-open`/`aria-expanded` in sync, search `aria-label`. |
| C7 | Grid & alignment | Delightful (3) | 1× | 12-col OS grid; KPI Columns3, cards align to 24 px gutters. |
| C8 | Spacing | Delightful (3) | 1× | 16/24 px rhythm, table rows 44 px min-height. |
| C9 | IA & depth | Delightful (3) | 1× | Sidebar (My cover / Benefit details / New claim / Claims & progress / Send documents / Notifications), Home → row → Claim result → back; 4-step wizard on New claim. |
| C10 | State communication | Delightful (3) | 1× | Status-bound stepline / Tag / Alert on Claim result (Processing vs Under assessment verified on claims 20 and 17), Notifications messages per status, "Only four states for you" note. |
| C11 | Animations | Delightful (3) | 1× | 22–24 transitions per screen, `prefers-reduced-motion` handled (probe rm=true). |
| C12 | Micro-interactions | Delightful (3) | 1× | Row hover (`a.pt-claim-row:hover`), chatbox chips answer inline, FAB toggles the chatbox. |
| C13 | Content & data quality | Market Leading (4) | 1× | Real VND amounts via FormatVND (2.000.000 ₫, 3.128.000 ₫, 18.000.000 ₫ left), real providers, claim numbers, dates; status labels identical across Claims / Claim result / Notifications (rev 5–7). |
| C14 | Modern vs dated | Market Leading (4) | 1.5× | 5/5 — same visual system as the console. |
| C15 | WoW & AI | Market Leading (4) | 1.5× | 5/5 — claims assistant chatbox with suggested questions, "Why you bear" explanation, item-by-item breakdown. AI: claimed (assistant). |
| C16 | Visual identity | Market Leading (4) | 1.5× | Palette, typography (Inter + Manrope), shape & depth (12 px radius, soft shadow) and iconography (ccicons font, `document.fonts.check("16px ccicons") = true`) all authored. |

Score: C1–C4, C7–C12 = 10 × 3 = 30; C5 + C6 + C13 = 3 × 4 = 12; C14–C16 = 3 × 4 × 1.5 = 18 → **60 / 70 = 86 %** (rev 7 without the C6 work: 59 / 70 = 84 %).

## Findings (all fixed in rev 3–7 unless noted)
1. **Navigation gap** — no UI path to PortalClaimDetail; Container/Table-row OnClick wired by Mentor never fires. Fixed with Link widgets on PortalHome rows and on the Claim/Treatment cells (rev 3). *Lesson: always use Link, never Container OnClick.*
2. **PortalClaimDetail layout** — Width1 wrapper, stepline wrapping, missing back link (rev 2).
3. **Status hard-coded** on Claim result and Notifications (rev 4, 5, 7): stepline/Tag/Alert and notification message now follow ClaimStatus; labels shared with the table (Paid / Result ready / Not payable / Under assessment / Processing).
4. **Table wrapping** — `.cc-table td a` inline-flex rule (tap-target fix from the console) squeezed the treatment link; ColumnsMediumLeft is the *narrow-left* block — replaced by ColumnsMediumRight (rev 5–6).
5. Keyboard (rev 8–9): first skip link used `href="#…"` → SPA navigated to the root screen; replaced by an OnClick client action. Focus-on-open needed a 220 ms delay (box is `visibility:hidden` during the slide transition).
6. Open (not scored down): mobile 390 px not captured — the audit ran in a maximised 1920 px Chrome; the OS grid `phone break-all` rules are present.

## Method
Chrome (tab 717093194) 1920×945; per-screen JS probe (visible `a,button,input,select,[role=button],[role=tab]` ≥ 44×44, transitions, reduced-motion, `:focus-visible` rules, OS-default colour hits, `.active-screen.scrollTop`); Tab-key focus check; chatbox opened and a chip answered. Companion: `../bnk-dev-claimmvp/runtime-audit-rev88.md`.
