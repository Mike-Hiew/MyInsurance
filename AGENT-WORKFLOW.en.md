# How to work in this repo — for agents (Claude Code + `outsystems-mcp`)

> English translation for reviewers. Vietnamese original: [AGENT-WORKFLOW.md](AGENT-WORKFLOW.md).

Read `../CLAUDE.md` first if you haven't. This file says **how** to work, not **what** to work on.

---

## 0. Getting started in 3 steps

```
1. openspec/project.md            → the 3 open changes, which one blocks which
2. changes/active/<x>/tasks.md    → keep ticking from where it stopped, don't start over
3. specs/<capability>/spec.md     → the immutable constraints of the area you're about to touch
```

**Do not start work that is not in `changes/active/`.** If new work appears: create the change folder first, write `tasks.md`, and only then do the work.

---

## 1. Three layers, and the only rule

| Folder | What it is | When to edit |
|---|---|---|
| `specs/<cap>/spec.md` | Stable source of truth | Only when archiving a change |
| `changes/active/<name>/` | The running delta | Freely |
| `changes/archive/<name>/` | Done | Never edit |

> **A change to system behaviour must exist in `changes/active/` before it exists in code.**

The reason for this rule: the *4-condition auto-approval gate* lived side by side with a sentence saying "the standard is 3" in `build-00` for ~90 revs without being recorded anywhere. Ruling on 21/09: **the code was right, the document was wrong**.

**Lifecycle:** `mkdir changes/active/<name>/` → `tasks.md` → (if the spec changes) `specs/<cap>/spec.md` containing **only the differing passage** → do the work → merge the delta into the Project doc → `mv` to `archive/`.

---

## 2. Verification here is NOT a git diff

OpenSpec assumes the code lives in git. **Here there is no repo, no diff, no PR** — the code is an ODC app in the cloud, edited by Mentor.

The replacement: **runtime audit + screenshots** in `../output/`. A convention that has run for 9 rounds and works well:

```
output/bnk-dev-<app>/
  runtime-audit-rev<N>.md     16-criteria table + findings + method
  shots-rev<N>/               screenshots as .jpg, zooms as .png for defects
```

**After every Mentor batch:** capture the screens → probe the DOM → write the audit → compare with the previous rev's shots.
Mentor **regresses silently** — the Checkpoint queue screen broke at rev 41 and was found at rev 44. Capturing screens is the cheapest way to catch this.

**Do not report "done" without a screenshot.** Mentor's report is not evidence.

---

## 3. Which skill for which job

12 `outsystems-*` skills are installed at `%USERPROFILE%\.claude\skills\`. Trigger them with **natural English phrases**, not slash commands.

### Cheap — read what already exists (a few cents)

| Job | Skill | Trigger phrase |
|---|---|---|
| View the whole tenant | `outsystems-tenant-architecture` | *"show me my tenant"* |
| View inside one app | `outsystems-app-architecture` | *"architecture of ClaimMVP"* |
| Check agents + model connection | `outsystems-ai-agent-landscape` | *"AI inventory"* |
| Who breaks if X is published | `outsystems-dependency-impact` | *"who depends on ClaimMVP"* |

> `app-architecture` loads the arch-cache → `app-documentation` reuses it **almost for free**. Run arch first, doc second.

### Costs money — generate/edit the app via Mentor (~$1–15 each)

| Job | Skill | Note |
|---|---|---|
| Entity + seed from spec | `outsystems-spec-driven-build` | **Declare the RBAC role for EACH screen right in the spec** — this is where Mentor fails most, and `context_screens` does not return per-screen roles, so nothing fixes it for you |
| Screens from a design | `outsystems-design-to-app` | Fidelity source: **front-end code > HTML mockup > Figma > image**. There is a **mandatory block-mapping gate** before firing Mentor — do not skip it |
| Review the plan before giving it to Mentor | `outsystems-plan-to-mentor` | Coverage matrix keyed by ID |
| Audit / demo-readiness / demo-data | `outsystems-mentor-copilot` | Run `demo-readiness` once before the dry run |
| C# External Logic | `outsystems-custom-code` | Ceiling of **95 s / 1 GB / 5.5 MB payload** |

### Ship & documentation

| Job | Skill |
|---|---|
| Safe to promote? | `outsystems-deploy-preview` |
| Markdown doc for the deck | `outsystems-app-documentation` |
| **Polling discipline** | `outsystems-mentor-polling-behavior` — **turn it on BEFORE the first batch, not after.** Cuts ~99 % of the poll payload entering context |

---

## 4. What Mentor must NOT do

Mentor is an agent. The four architecture rules in `../CLAUDE.md` §2 apply to it exactly as to every other agent.

| Do not let Mentor | What to do instead |
|---|---|
| Generate **Entities** | Spec → `outsystems-spec-driven-build`, with manual review |
| Generate **money logic** — the 14 steps, the auto-approval gate, the limit ledger, the state machine | Write by hand in Studio. Let Mentor **call** it, not **generate** it |
| Decide **RBAC** | Declare it in the spec, check it by hand in Studio |
| Choose the **bilingual mechanism** | Decide once, apply to every screen |
| Decide the **money format** | One shared `FormatVND` Server Action |

Mentor may do: **screens, bindings, layout, blocks** — and the result must still pass a screenshot.

---

## 5. MCP quirks to remember

| # | Quirk | Consequence |
|---|---|---|
| 1 | `context_*` caps `limit` at **100** — above 100 returns HTTP 400 | Always `limit: 100` |
| 2 | `app_list` **returns only the first 100 if `limit`/`offset` are not passed**, even though the docstring says "fetch every page" | Pass `limit: 1000` |
| 3 | `app_refs` **times out on large OML apps** | Wrap in a short timeout + cache on success |
| 4 | `app_refs` **cannot handle >2 parallel calls** — 3+ clog the upstream queue | Throttle to 1–2 parallel, rest ~5 s between batches |
| 5 | `context_themes` returns **>150 K characters** | Skip it unless needed |
| 6 | `deploy_impact` consistently returns `{impacts:null, status:null}` on stage | **Don't depend on it.** Use the comparison pattern from `deploy-preview` |
| 7 | `context_screens` **does NOT return per-screen roles**, only `isPublic` | **Never invent roles.** A field report has already gone wrong because of this |
| 8 | `app_revisions` paginates like `app_list` | Avoid full enumeration |
| 9 | The OAuth token expires mid-session | Handle the "requires re-authorization" error properly, **do not loop**. Remote session: paste the full `localhost:7890/callback?…` URL then call `complete_authentication` |

**Cache:** `~/.claude/cache/outsystems-<skill>/<id>/`, TTL 1 hour. Say *"refresh"* / *"rebuild"* to force a fresh fetch.

**Cost-confirmation gate:** `design-to-app`, `spec-driven-build`, `mentor-copilot` and `dependency-impact` all ask go/cancel with an estimate **before** spending money. This is deliberate — **read carefully, then say "go"**.

---

## 6. Saving tokens

1. **Batch independent calls into one message** (N MCP fetches + N Writes + 1 Bash) — every extra turn costs ~30 s wall-clock.
2. **Compact-Write**: when a payload comes back inline, write out only **the fields `build.py` reads**, not the full MCP shape.
3. Do not transform with Bash in the model's output — `build.py` handles it all. Don't use `jq`.
4. A re-run from cache should cost under ~3 K tokens. If it costs more, the cache is broken — check before continuing.
5. Polling discipline Tier 1/2/3: Tier 1 synchronous, no polling (`app_list`, `context_*`) · Tier 2 poll every 30 s · Tier 3 drain the cursor then rest 30 s for `mentor_get_run`.

---

## 7. Lessons from the UI pass — read before opening a new UI change

1. **Always use the `Link` widget, never a `Container` OnClick.** An OnClick that Mentor attaches to a Container/Table row **never fires** — this bug left the portal with no path into the detail screen until rev 3.
2. **`ColumnsMediumLeft` is the narrow-on-the-LEFT block.** For a 2/3–1/3 layout use `ColumnsMediumRight`.
3. **A skip-link must not use `href="#…"`** — the SPA router navigates back to the root screen. Use a client action OnClick.
4. **The rule `.cc-table td a { inline-flex }` squeezes text in table cells.** Re-check tables after every tap-target change.
5. **Icons use the custom font `ccicons`**, not `IIcon`/Phosphor — the original block-mapping table got this wrong.
6. **Theme:** override the built-in OS UI variables at `:root` in the **THEME** stylesheet; do not declare custom variables in a Screen's stylesheet. Done right, ODC's dark mode works out of the box.
7. **Permission is absence, not `Enabled = False`.** A button that is not allowed must **not exist in the DOM**, and the Server Action re-checks role + scope itself.
8. **After every Mentor pass that touches flow wiring, make Mentor list the node sequence.** Its "done" report is not enough. Caught once: an edit left the True branch of `BootstrapDemoData` as `ClearDemoData → Seed_BenefitSnapshots → End` — **deleting the data and not re-seeding**, a clean publish with 0 errors. A read-only pass that lists nodes is cheaper than a session hunting for vanished data.
9. **Beware of sentinels equal to 0.** This model has two zeros with **opposite** meanings: `CountLimit = 0` means *unlimited*, `AnnualLimit = 0` on a child line means *included*. A sentinel that is arithmetically valid **never fails loudly** — `build-01` §8.1. Every time you add a numeric column with a special meaning, ask: if someone leaves it at the default, is it accidentally correct?
10. **Two fields that both mean "active" are a trap — and this project has fallen into it four times.** `GetTerms` filtered on `IsActive = True` (a technical column of the static entity, **True on every row**) instead of `Status = "ACTIVE"` (the business field `termStatus`: DRAFT/PENDING_ACTIVE/ACTIVE/WITHDRAWN) ⇒ 4 `DRAFT` terms slipped into the engine, and **the filter filtered nothing**. Same family: `level` (tree depth) vs `benefit level` (the plan attached to a policy) → D3 renamed it to `depth` · `state` (how complete the document set is) vs `outcome` (the processing result) · looking up a key by `Label` instead of `Id`.
    **Rule:** when two fields on the same entity sound alike, write out the **meaning** of each right at the filter, and ask *"will this filter ever exclude a row?"* A filter that never excludes anything looks identical to a correct filter.
11. **A test suite that is green because the fixtures are duplicates looks identical to a correct test suite.** `Seed_ChainTestFixtures` never deleted (deleting `Claim` before its children ⇒ the FK silently swallowed the error) ⇒ **185 overlapping `TST-` claims, 4–5 copies per code**. 46 tests stayed green throughout because the copies were identical; it only broke when `T-CHAIN-05` **changed** one copy.
    Same family as rule 10, but at a more dangerous level: rule 10 makes one result wrong, rule 11 makes **every result meaningless**.
    **Rule:** fixtures must **be cleaned before creation**, deletion order is **children before parents**, and after every change to the fixture mechanism **re-run everything on a clean fixture** — green on the old fixture does not carry over.
12. **Do not invent behaviour where the spec is silent.** Step 9, exceeding the daily limit: `HOLD`, **no pro-rata cut**, because A5 does not specify a pro-rata cut. The temptation is to choose a "reasonable" behaviour; but a self-invented reasonable behaviour is **indistinguishable** from a specified one, and six months later nobody knows where that number came from.

---

## 8. Three answers to common questions

**"Where is the full spec?"** — The "Hackaton" Project on claude.ai, docs `claude/build-00…06`. The `specs/<cap>/spec.md` files here are a constraint summary + pointers, deliberately not copies (400 KB; two copies would drift). The upstream sources (A1–A5, D1–D3) are on disk at `../Preparing/04_Nghiep-vu-va-du-lieu/`.

**"The BRD says something different from the spec — which one wins?"** — **A5/D2 win**. 8 conflicts have been ruled on in Project doc `build-00` §5. If you hit a 9th conflict: open a change, do not decide on your own.

**"It's done, can I report it?"** — Not if there is no screenshot in `../output/` and `tasks.md` is not fully ticked.

13. **A half-fix is worse than no fix.** 21/09, `Seed_AssessmentAndTerms`: pass N1-b deleted the 9 `CreateCalcSnapshot` nodes for claim B — reported done, 0 errors, **did not persist** (the count was still 27). Pass N1-c deleted `CrCRB` + the 2 Assigns around it — **persisted for real** (ChainRun 4 → 3). Result: claim B still had a node creating a `CalcSnapshot` pointing at the variable `ChainRunIdB` with **no node left that assigns it** ⇒ it would write `ChainRunId = 0`. **Validation still showed 0 errors**, because an unassigned local variable is syntactically valid. This is a state worse than either endpoint. Rule: after every structural edit, **re-count with a separate read-only pass**; if the number did not change, the pass did not persist — stop the mechanism, and **go back to the safe endpoint before trying another way**; do not stack the next pass on top of a half-state. Mentor explained the contradiction with something impossible (an Assign node being "renumbered" into an Execute Action node) — it invented that to fill the gap; it was not a mistaken report. Also: **"save" is not "publish"** — Mentor reported "saved, revision 943" while the app was running at rev 162. The model revision number and the app revision number are two different counters; only the second means anything. A half-edited save will ride along with the next publish by anyone to the environment.

14. **Mentor edits the model, it does not deploy.** 21/09: Mentor reported "saved, revision 943" and later said outright that the Model API has no publish. The revision number it reports is the **model's revision**, not the running app's — two different counters, and only the second means anything. A model that is saved but not published is a change that is **invisible at runtime yet still travels with the next publish by anyone**. Rule: after every Mentor pass, state explicitly whether it was *saved* or *published*; if only saved, it **does not count as done** and must not be left hanging overnight. Publishing is the job of the outsystems MCP (Claude Code) or 1-Click Publish in ODC Studio.

15. **A readout that does not separate observation from inference is an unusable readout.** The 21/09 readout did it right: it said outright "no tool can read SQL on the ODC DB", listed what was **seen** (the largest RunNo + the number of CalcSnapshot rows for exactly that ChainRunId, across 54 claims) and separately what was **inferred** ("if RunNo is always max+1 and no rows were ever deleted then the total is 16 — unchecked"). Because of that separation a mismatch was caught: claim C should have 9 CalcSnapshot rows, 0 were observed. Had the two kinds been merged, the number 16 would have become "data" and the mismatch would have been buried. Rule: every readout states **what it was read with**, and every number carries the label *observed* or *inferred*. An inferred number is never used as the basis for a decision — see rule 13, where an invented number ("27") wrecked a day.

16. **A number Mentor reports about itself is not evidence. A diff against a published revision is.** 21–22/09, for the same action `Seed_AssessmentAndTerms`, the node count Mentor reported across sessions: **97 · 170 · 175 · 187 · "30" · "27" · "18"**. Seven numbers for one thing. Each came with a coherent story, and I changed my conclusion three times because of them: believed the deletion persisted → concluded "it was never deleted, all of it is made up" → it turned out **there was a real deletion, at rev 165**. Rule: **a node count Mentor reports itself cannot be compared between two sessions**, so never use it as grounds to overturn a conclusion. The only way that has been proven: **open each published revision, list the real nodes of each revision, then compare** — and say plainly which parts can NOT be compared (screens, entities, aggregates, site properties cannot be compared through the stream). Also: a conclusion may only be overturned by evidence **stronger** than the evidence that built it, not by the most recent evidence.

17. **A test suite that depends on who ran what beforehand is not a test suite.** 22/09: on the same rev 166, run #12 gave 45/46 and run #13 gave 46/46 — different in **data**, not in code. A single click on "Re-run demo seed" set `PremiumPaid = True` on `FX-STD-UNPAID`, exactly the fixture that `T-CH01-c` builds to test the unpaid-premium branch. Rule 11 says *clean before creating*; that is not enough — the full rule is **clean and rebuild the fixture immediately before every RUN**, so a green run always means what it says. And when installing that self-protection mechanism, **accept it by deliberately dirtying the data and running twice in a row** — otherwise we are just doing by hand exactly what the mechanism is supposed to do itself, and we don't know whether it runs.

18. **Filter logs by traceId, not by action name.** 22/09: `Seed_AssessmentAndTerms` logged *"finished successfully"*, and its `AllExceptions → LogError` handler wrote not a single line in 30 hours. The same `traceId` had a real Error: `Seed_ResetDemoClaimData → DeleteChainRun: 23503` — an FK from `LedgerPosting`. Searching by action name would never find it, because the action that reports success **is a different action** from the one that fails. Rule: when something is wrong at runtime while the related action reports success, take the `traceId` of that run and read **all** log lines with the same trace, every severity, every action.

19. **A screen that reads the "latest record" goes blank when the latest record is empty, even though the data is intact.** F48 reads the latest `ChainRun` of the claim. Claim 0003 had all 9 `CalcSnapshot` rows in the DB but the Calculation tab was empty, because its latest run was an empty run — and we lost a session hunting for "lost data" that was never lost. This applies to **every** claim: one mistaken click creating an empty run on a demo claim means a blank hero screen in the middle of the pitch. Rule: a screen that reads the latest record must read the latest record **that has data**, or say plainly "this run has no step data yet" instead of showing an empty table.

20. **The node name Mentor reports is not a stable identifier.** 22/09, `Seed_BatchB_Backfill`: before the edit, `assignNode2` was the node assigning `PremiumPaid` in the Policy loop; after deleting 3 nodes, `assignNode2` was a completely **different** Assign, in the Member loop. The `assignNodeN` names are most likely numbered by order and so shift after every add/delete. Direct consequence: a prompt written *"delete assignNode2"* based on an earlier read could delete **the right name, the wrong node**. Rule when writing prompts: identify a node by **what it does** (*"the node that writes `Policy.PremiumPaid`"*), or by **name plus content** as a compound key; and every edit pass must come immediately after its own read pass, never reusing the result of an older read. Acceptance must also be stated in terms of behaviour (*"no node writes `Policy.PremiumPaid`"*), not node names.
