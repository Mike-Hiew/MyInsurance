# AI assistant button = BnK orb (evening of 15/10/2026)

> English translation for reviewers. Vietnamese original: [ai-orb-bnk-nut-tro-ly-15-10.md](ai-orb-bnk-nut-tro-ly-15-10.md).

Continues: claude/tien-do-v10-giong-mockup-15-10.md (ClaimMVP 759 / MyInsurance 216).

## Design
- A glass sphere, teal → blue → navy liquid (#16DCA8 / #00AFA8 / #0A84B2 / #0B1650) sloshing, glowing wave lines, a reflection streak, a "breathing" glow behind it.
- The BnK logo (hexagonal shield + B + pixel cell, taken from the "Crafted by BnK Solution" vector in mockup v1.0) sits in the middle and flips like a coin every 6 s. `is-thinking` → fast flow, the logo rotates continuously, a wave ring spreads out. Motion off under prefers-reduced-motion.
- CSS only + 6 Containers. Demo/code page: artifact "Nút AI Orb" (EN: "AI Orb button").
- Files: `E:\Hackaton\assets\ai-orb-bnk.css` and `.min.css` (9.3 KB). Mockup v1.0 has it attached (backup "truoc ai orb 15-10").

## Widget structure (same in both apps)
Button (add class `ai-orb`, keep the old classes + is-open logic) → after the old Icon:
- `OrbCore` .ai-orb__core → `OrbSwirl` .ai-orb__swirl · `OrbWaveBack` .ai-orb__wave.ai-orb__wave--back · `OrbWave` .ai-orb__wave · `OrbLogo` .ai-orb__logo
- `OrbGloss` .ai-orb__gloss
The old robot icon is hidden by CSS (`.ai-orb>i{display:none}`), not deleted.

## Applied
| App | Rev | Button | Check |
|---|---|---|---|
| ClaimMVP | 760 (22:14) | `b1-ConsoleFabBtn` `.cc-console-fab` | 58 px orb bottom-right corner, number badge kept (z-index 5), label "Ask the assistant" kept |
| MyInsurance | **217** (22:35) | `b1-FabBtn` `.pt-fab.pt-chat-fab` | Validate 0 errors. On the real page: 58 px orb, correct logo (SVG has the B path), chatbox opens/closes normally, label "Hỏi trợ lý" (EN: "Ask the assistant") kept |

## Notes
- Mentor again attached `OSInline ThemeGrid_MarginGutter` to the new Containers by itself (MyInsurance) → there is already a rule `.ai-orb>.ai-orb__core,.ai-orb>.ai-orb__gloss{margin:0!important}` so there is no misalignment.
- `is-thinking` is not yet wired to the moment the assistant is answering (needs an IsThinking variable in the chat block: True before calling the agent, False when the result arrives; add " is-thinking" to Style Classes).
- ChainTests not run after rev 760/217 (UI-only changes).
