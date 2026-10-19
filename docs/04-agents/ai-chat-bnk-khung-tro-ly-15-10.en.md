# BnK-style AI assistant chat panel (evening of 15/10/2026)

> English translation for reviewers. Vietnamese original: [ai-chat-bnk-khung-tro-ly-15-10.md](ai-chat-bnk-khung-tro-ly-15-10.md).

Continues: claude/ai-orb-bnk-nut-tro-ly-15-10.md (BnK orb button).
Requirement: a chatbot whose icon, effects and colours match the BnK logo/orb; "wow", futuristic, pleasant.

## Design (CSS only, no widget changes)
- **Panel:** navy glass (rgba(8,24,60,.93) → rgba(4,14,40,.95)), 22px backdrop blur, 26 px radius, a teal glow drifting slowly in the top-right corner (14 s), a **rotating glowing border** teal → blue (conic-gradient + `@property --chA`, 9 s/turn), opens with a bounce from the bottom-right corner.
- **Header:** 40 px mini orb with the BnK logo in place of the robot icon (`.pt-chatbox-hdr::before`), glowing on a 3.2 s pulse; a blinking "online" dot after the title; a light streak running under the header; round glass close button that rotates 90° on hover.
- **Bubbles:** AI = translucent teal–blue glass, faint glowing border, radius 18/18/18/6; user = gradient #00AFA8 → #0A84B2 → #3E8BFF, radius 18/18/6/18. Appear with a gentle bounce.
- **Suggestion chips:** glass pills with a teal border, a 4-point gradient star icon (SVG mask), appearing one after another, brightening on hover + lifting 1 px.
- **Input box:** dark glass pill (old lavender–mint background removed), focus = teal glowing ring; send button = teal gradient orb, scales up and tilts on hover.
- **MyInsurance extras:** the button "Hỏi trợ lý về hồ sơ này" (EN: "Ask the assistant about this claim") (`.mk-btn.mk-btn-ai`, and `.btn-ai-receipt`) changed from purple to the teal gradient.
- All motion off under prefers-reduced-motion. The chat panel keeps a dark background in light mode too (in sync with the orb).
- Selectors of the form `html body .pt-chatbox[class] …` / `html body .cc-console-chatbox[class] …` to beat old rules that use !important.

## Applied
| App | Rev | Notes |
|---|---|---|
| MyInsurance | 218 (chat panel), **219** (mk-btn-ai button) | Validate 0 errors. Checked on the real page (PortalClaimDetail 440, dark background). |
| ClaimMVP | 761 (BnK chat panel) | 2 Mentor turns (part A/B ~8 KB). Validate 3 errors = background. |
| ClaimMVP | 762 (Chat only, like MyInsurance) | See section below. |
| ClaimMVP | **763** (suggestion chips under the greeting) | See section below. |

## ClaimMVP rev 762 — remove the Activity tab, chat panel like MyInsurance (22:55, following the feedback "Activity is redundant")
- The variable `ActiveTab` already defaulted to "chat"; the 2 places that switched to Activity when opening the panel (`ToggleConsoleChat`, `TriggerOpenAssistant`) now set "chat" → the panel always opens on Chat.
- Title `CCHdrTitleExpr` = T("Claim assistant","Trợ lý hồ sơ"); subtitle `CCHdrSubExpr` = T("Ask about claims, documents & limits","Hỏi về hồ sơ, chứng từ & hạn mức") (before: "Work inbox / Proposals & alerts").
- CSS: hide `.cc-ab-tabs` and `.cc-ab-activity` (widgets/logic of Activity not deleted); floating panel 360 × min(560, 100vh−120) px, anchored right 26 / bottom 96 px right above the orb (selector with the class repeated 3 times to beat `.layout.layout-side .cc-console-chatbox{top:…!important}`); the number badge on the orb (`.cc-fab-n`) hidden because the Activity list is no longer shown.
- Minor leftover: the English subtitle wraps to 2 lines in the header (not fixed).
- To reopen Activity: remove the 2 CSS lines hiding the tab/activity + change the 2 actions back to how they were.

## ClaimMVP rev 763 — suggestion chips right under the greeting like MyInsurance (23:10)
- Mentor could not move widgets between containers → added a new Container `CCChipsTop` (class "pt-chatbox-chips cc-chips-top") in `CCBody`, between `CCWelcomeBubble` and `CCChatList`, with 3 Buttons `CCTChip1–3` that are functional copies of the footer chips (OnClick `ChipWaiting` / `ChipProposals` / `ChipAlerts`, Enabled = not IsAsking, same show condition as `CCGenChips`: shown only when no claim is open). Inside the `LayoutSideMenu` block.
- The old footer chips (`CCGenChips`) are kept, hidden with CSS `[id$=CCFooter] .pt-chatbox-chips{display:none}`.
- Checked on the Work list: greeting → 3 chips stacked vertically → input box; footer chips hidden.

CSS files: `E:\Hackaton\assets\ai-chat-bnk-myinsurance.css`, `ai-chat-bnk-claimmvp.css` (not yet including the rev 762–763 block).

## Method / lessons
- Preview by injecting the source CSS (with placeholders $S/$LOGO/$STAR) into the real page, taking the logo from `getComputedStyle(.ai-orb__logo)` → no need to paste the data URL again.
- Each app split into 2 Mentor turns ≤ 8 KB; Mentor copied correctly (checked: the `ch-spin` rotating border runs, the header orb has the logo path).
- User bubble not yet checked on the real page (no test message sent, to avoid calling the agent / writing AgentRun).
- Mockup v1.0 not yet updated for the chat panel (mockup classes differ from ODC).
