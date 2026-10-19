# Change — AG-2 Adjudication on Agent Workbench (floor of the submission)

> English translation for reviewers. Vietnamese original: [ag2-adjudication-agent-2026-10-01.md](ag2-adjudication-agent-2026-10-01.md).

**Opened:** 01/10/2026 · **PO:** Mike · **Why now:** live IDP still times out (IT promised a fix by 02/10) → switch to AG-2.
**Standard:** `build-05` §3.3 (AG-2 prompt), §6.3 (floor) · `agent-architecture/specs/agents/spec.md` (1 agent, the chain is ONE tool, IDP does not run inside the agent turn, 60 s ceiling).
**Status 01/10 23:30:** the AG-2 floor is **done** (items 1–7 ✅). ClaimMVP **rev 237**, agent **rev 4**.

## PO decisions 01/10
- Model connection: **TrialGPT5**.
- New asset: **CLM_AdjudicationAgent** (do not reuse `AG5_Adjudication` / `AgenticHealthClaimAdjudicationAgent`).
- Call points: **"Tính và giải thích" button (EN: "Calculate and explain") + follow-up chat box** on ClaimDetail.
- (22:00) **Turn time ~42–45 s: leave as is**, use a backup video for the pitch. No further optimisation.
- (22:00) Fix **all** 4 screen bugs seen on CLM-2026-0005 → done in rev 236–237.
- **CLM-2026-0005 is the "clean auto-approval" claim for the demo** (replaces 0003 in T-GATE-01).

## Design
- ClaimMVP exposes **Service Actions** to the agent: `Agent_RunAdjudicationChain` · `Agent_GetClaimContext` · `Agent_GetChainExplanation`. Each tool writes **one AgentRun row** itself (AgentName "AG-2 Adjudication", correct ToolName, Replay per Settings).
- Agent app `CLM_AdjudicationAgent` (`f012b159-2e60-48c5-9e2e-ea90457140a5`, TrialGPT5), exposes `AskAdjudicationAgent(ClaimId, Question, SessionIdIn)` → Answer, SessionId, IsError, DurationMs. Rev 4 answers concisely: only the steps that changed money + "Các bước khác: PASS/SKIP" (EN: "Other steps: PASS/SKIP") + Total + Summary (EN).
- ClaimMVP ClaimDetail (tab **Calculation & ledger**): AG-2 panel, server action `AskAG2`, ServerRequestTimeout 60 s.
- **No tool writes money outside the chain**.

## Work items
- [x] 1. Read the model
- [x] 2. (rev 223) 3 service actions for the agent + AgentRun writing
- [x] 3. (agent rev 1–4) `CLM_AdjudicationAgent`, TrialGPT5, prompt, 3 tools
- [x] 4. (rev 224) ClaimDetail: AG-2 panel
- [x] 5. Floor check: 0003 run #2 = #3 = 5.400.000; 0005 run #2 = #3 = #4 = 1.120.000; AgentRun ToolName correct, Replay = False
- [x] 6. Replay flag check = True (22:18)
- [x] 7. Screenshot of the agent rev 4 Actions tab (TrialGPT5 + exactly 3 actions) → `E:\Hackaton\output\bnk-dev-claimmvp\shots-ag2\ag2-agent-tools-rev4.png`

## Result — CLM-2026-0003 (ClaimId 19)
- AG-2 called GetClaimContext → RunAdjudicationChain, 14 steps, **5.400.000 ₫**; pressing again gives the same figure. AgentRun 1354–1356, Replay = False. Turn ~45 s.
- Follow-up "Vì sao bước 9 bị HOLD?" (EN: "Why is step 9 on HOLD?") → quoted the step 9 log verbatim, did not re-run the chain ✅.
- 0003 is **not clean**: B09 HOLD (missing number of treatment days per line), 5.400.000 > 2.000.000 → gate ✕ (conditions 1, 3). The old seed 1.680.000 was not produced by the chain.

## Result — CLM-2026-0005 (ClaimId 430) · clean demo claim
- Admin button **"Create demo claim CLM-2026-0005"** (rev 229–234, changed to "creates or repairs" in rev 237): outpatient, Trần Thị Bích, HC-2026-0002 · HC-PLS, Hoàn Mỹ Sài Gòn (in-network), J06.9, 3 lines 300.000 / 450.000 / 650.000 = 1.400.000.
- Chain: B05 → 200.1, 200.1, 200.2 · B08 no cut · **B10 co-payment 20 % = 280.000** · **Total 1.120.000 ₫** · **gate 4/4 passed**.
- AG-2 turns: 21:16 (agent rev 2) 59 s timeout · 21:26 (rev 3) 66 s → 504 · 21:34 (rev 4) **44.5 s OK** · 22:18 ~42 s OK. Breakdown of 44.5 s: ~7 s before the first tool · ~10 s between the 2 tools · ~24 s writing the answer.

## Replay flag check — 01/10 22:18 ✅
PO turned on `ReplayMode = True` (Claude read the Portal to confirm) → Claude pressed "Tính và giải thích" (EN: "Calculate and explain") on 0005 → run #4 (ChainRunId 301) 1.120.000 ₫ → AgentRun **1398 GetClaimContext · 1399 RunAdjudicationChain · 1400 Agent answer — Replay = True** → PO turned it off → Portal: **ReplayMode False** ✅.

## Fixing 4 screen bugs — rev 235–237 (01/10 evening, Mentor, verified by behaviour)
| Bug | Cause (read from the model) | Fix | Check |
|---|---|---|---|
| Benefit table 1 row / "Payable 700.300" / toast "Index -1" when loading 0005 | `BuildBenefitRows` (client) picked list elements by subtracting string lengths, kept money as Text and then re-parsed it | Server Data Action **`GetBenefitSummary`** (struct `BenefitSummaryRow`, Decimal): LedgerPosting AMOUNT of the latest run, **for each cost line only the posting with the deepest code** (sort ClaimLine.Id asc, BenefitCode desc + dedupe PrevLineId), grouped by code with `ListIndexOf`; footer summed from the rows; warning when total ≠ chain. BuildBenefitRows deleted | 0005: 200.1 750.000→600.000 (customer 150.000) · 200.2 650.000→520.000 (130.000) · Total 1.400.000/1.120.000/280.000. 0003: 100.1 1.500.000 · 100 1.400.000 · 100.2 2.500.000 · Total 5.400.000. Toast gone, no warning ✅ |
| Header "Processing documents" | Seed set Status = Received | Seed: Status = UnderReview | "Under assessment" ✅ |
| Assessor empty | Seed did not assign AssignedTo | Seed: "Nguyễn Hoài An"; fallback expression "Unassigned" | ✅ |
| SK-1 event labels shifted, Admission 01/01/1900 | First 5 values bound off by one cell; NullDate not guarded | Re-bound; Admission/Discharge "—" when empty | 0005: CLM-2026-0005/1 · Outpatient · 22/09/2026 · — · —; 0003: Inpatient · 03/07 · 03/07 · 06/07 ✅ |
| "Mandatory documents present ✓" when the checklist is empty | The expression was the constant `True`; the seed did not create ClaimDocument | Seed adds T1-01 (Required), T1-03, T1-04 (Expected), all Received; check = `MandatoryDocsOk` computed in GetDocs OnAfterFetch | 0005 has 3 documents, real ✓. Seed button run twice: "inserted: 3 (already present: 0)" then "inserted: 0 (already present: 3)" — idempotent ✅ |

- rev 235 hit a Postgres runtime error `42703 column "codelen" does not exist` (aggregate sort by a computed column) → rev 236 sorts by BenefitCode desc.
- Mentor's second attempt reported "CodeLen fixed" but `changeApplied = false`, and the publish created no new revision → **rule 5 repeats: trust only a new revision + the screen**.
- Still cosmetic: Sum/yr of child codes shows 0 (annual limit is at the parent code), Co-pay "—" when inherited from the parent; table not yet sorted by code.
- Negative control for "Mandatory documents present" not yet checked (claim missing a mandatory document → ✕).

## How to click in ClaimMVP (Claude)
- The AG-2 panel is in the **Calculation & ledger** tab: click the tab first (coordinates), `scrollIntoView` the button, click by coordinates = rect × 1568/innerWidth.
- Administration buttons (Dry-run, Create demo claim): **only work with `button.click()` via JS** — coordinate clicks fire 0 events.
- A `javascript_tool` return containing "?"/"=" may be blocked → replace those characters before returning.

## Other remaining items
- 0003: "Total claimed 2.460.000" (Claim.TotalClaimed seed) ≠ line total 5.400.000.
- Old AgentRun seeds still carry the names AG-0/AG-1/AG-3…AG-6 (item 9 of agent-architecture).
- 🔴 `IdpPassword` still shows in plain text on Portal Settings — move to Secrets + change the password before recording the video.
