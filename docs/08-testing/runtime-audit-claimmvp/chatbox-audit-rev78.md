# Chatbox / assistant audit — ClaimMVP · rev 78 · 2026-09-19

Scope: the console assistant (`.cc-console-fab` + `.cc-console-chatbox`, tabs Chat | Activity) on ClaimDetail (claim 17) and WorkList, plus the portal chatbox (`.pt-fab` + `.pt-chatbox`) for consistency. Measured in the user's Chrome session (1920×945) with JS probes; read-only. Shots in `shots-rev65/`: `assistant-activity-rev78.jpg`, `assistant-chat-rev78.jpg`, `assistant-inbox-rev78.jpg`, `chatbox-activity-proposal-rev78.jpg`, `chatbox-scrollbar-rev78.png`, `chatbox-chat-after-send-rev78.jpg`, `portal-chatbox-rev78.jpg`.

## Score (component-level, same 16-criterion rubric, N/A where the component has no evidence)
| # | Criterion | Tier | Evidence |
|---|---|---|---|
| C1 Theme | Delightful (3) | Card 360 px · r16 · 1 px rgba(255,255,255,.1) · shadow 0 18 48 · teal radial gradient = prototype `.pnl`/`.chatbox`; FAB 54 px gradient + amber badge (#C77700, 2 px #0c3036 ring). **Drift:** close button is a LIGHT control (white bg, #E4E7EC border, #6E7787 icon) on the dark card — both console and portal; default white OS scrollbar inside the dark card. |
| C3 Fit | Delightful (3) | Tabs, inbox counts ("1 to confirm · 1 alerts"), To confirm / Warning / Alerts rows, Steps that ran (11 rows), proposal card `.cc-pcard` (rgba(255,255,255,.08), r12, Confirm #35d3a3 / Reject ghost), chips, ask input. Prototype's "Needs confirmation" is a pill; here it is plain amber text (#FFD580, no bg). |
| C4 Contrast | Delightful (3) | Bubbles #fff on rgba(255,255,255,.12) over #0c3036 ≈ 9:1; sub/meta rgba(255,255,255,.55) ≈ 5.2:1; section labels .45 ≈ 4.4:1 (borderline, 10.5 px uppercase). |
| C5 Targets | Market Leading (4) | FAB 54, close 44, tabs 36 × 57/96 (≥44 wide, 36 tall — acceptable for tabs), chips 44, input 44, send 44, Confirm/Reject 44. 100 % ≥ 44 on the primary controls (tabs 36 px are the only sub-44 element). |
| C6 Keyboard / a11y | Unpleasant (1) | FAB has no `aria-label` (announces "2"); tabs are buttons without `role="tab"` / `aria-selected`; send button has no label; input has no `aria-label`; no `:focus-visible` rule targets the chatbox controls (only the global button rule). |
| C10 States | Acceptable (2) | Default tab = Activity when work is pending ✓, badge hides when open ✓, X icon ✓. **Missing:** no scroll-to-latest after a message (body `scrollTop 0`, latest bubble hidden below the fold), no empty state for Activity when nothing is pending, no "typing/answering" state. |
| C11 Animation | Acceptable (2) | `animationName: none` on the card (prototype: `pslide .18s` translateY 8 px + fade); FAB has `transition 0s`. |
| C12 Micro | Acceptable (2) | Chips → bubbles ✓, tab switch ✓, X ↔ robot ✓; no hover lift on FAB, no chip hover colour (prototype: border #35d3a3). |
| C13 Content | Delightful (3) | Data-driven answers (documents missing, limit left, per-visit cap), inbox rows with claim no. + name + reason + time, steps with lane and ms. Free-text fallback copies the prototype ("I only answer about the open claim… logged on claim…"). |

Weighted (C1 3, C3 3, C4 3, C5 4, C6 1, C10 2, C11 2, C12 2, C13 3 = 23 / 36) → **64 % — Acceptable** for the component (the app-level score is unaffected; this is a drill-down).

## Findings (one root cause each)
1. **Enter sends on the first keystroke** (bug, C10/C13): typing "Han muc…" + Enter produced TWO user bubbles — "H" and the full sentence — each with an AI reply. The input's OnChange handler (or the keydown handler) fires the send before the value is complete. `chatbox-chat-after-send-rev78.jpg`.
2. **No scroll-to-latest** (C10): after sending, `.pt-chatbox-body` stays at `scrollTop 0`; the answer is out of view. The body also has `max-height:none` inside a 520 px card, so the chip/footer block squeezes the history to ~124 px.
3. **Light close button + default scrollbar on a dark card** (C1): `.cc-icon-btn` keeps the header styling (white/#E4E7EC) in both chatboxes; the Activity/body scrollbars are the OS default (white track). `chatbox-scrollbar-rev78.png`.
4. **No entrance animation / transitions** (C11/C12): card pops in; FAB has no transition; chips have no hover state.
5. **Accessibility labels missing** (C6): FAB, send, input, tabs.
6. Minor: "Needs confirmation" tag is text-only (prototype pill `rgba(224,162,74,.18)` / #e8bd7c); Activity list is 1037 px tall inside 380 px (11 steps before the proposal) — put PROPOSALS AWAITING DECISION above STEPS THAT RAN so the actionable card is visible first; portal box is 320 px vs console 360 px and the portal FAB keeps the robot icon while open.

## Mentor handoff (batch P5, ≤ 6 items)
1. **[bug] Send only on explicit Enter/click** — remove the send call from the input's OnChange; keep one keydown handler `if(event.key==='Enter'&&value.Trim()<>'')` → SendChat; clear the input after send. Test: type a 10-char sentence, exactly one user bubble.
2. **Scroll-to-latest + body sizing** — `.cc-console-chatbox .pt-chatbox-body{flex:1;min-height:160px;max-height:260px;overflow-y:auto}`; after appending a message run `body.scrollTop = body.scrollHeight` (OnAfterFetch/OnRender JS on the ChatMsgs list); same on the portal.
3. **Dark-card controls** — `.cc-console-chatbox .cc-icon-btn,.pt-chatbox .cc-icon-btn{background:transparent;border-color:rgba(255,255,255,.16);color:rgba(255,255,255,.7)} …:hover{color:#fff;border-color:rgba(255,255,255,.4)}`; scrollbar `.cc-console-chatbox *::-webkit-scrollbar{width:6px} …::-webkit-scrollbar-thumb{background:rgba(255,255,255,.22);border-radius:8px} …::-webkit-scrollbar-track{background:transparent}` (+ `scrollbar-color: rgba(255,255,255,.22) transparent`).
4. **Motion** — `@keyframes pslide{from{opacity:0;transform:translateY(8px)}to{opacity:1;transform:none}} .cc-console-chatbox.is-open,.pt-chatbox.is-open{animation:pslide .18s ease}`; `.cc-console-fab,.pt-fab{transition:transform .15s,box-shadow .15s} …:hover{transform:translateY(-1px)}`; `.pt-chatbox-chip:hover{border-color:#35d3a3;color:#fff}`; respect `prefers-reduced-motion`.
5. **A11y** — FAB `aria-label="Claim assistant, 2 items waiting"` (bind the count), send `aria-label="Send"`, input `aria-label="Ask about this claim"`, tab buttons `role="tab"` + `aria-selected`, tab strip `role="tablist"`; add `.cc-console-chatbox button:focus-visible{outline:2px solid #35d3a3;outline-offset:2px}`.
6. **Activity polish** — order PROPOSALS AWAITING DECISION before STEPS THAT RAN; `.cc-tag-warn{background:rgba(224,162,74,.18);color:#e8bd7c;border-radius:999px;padding:2px 8px}` for "Needs confirmation"; empty state "Nothing waiting for you" when InboxCount = 0 and no proposal; portal box width 360 and X icon while open.
