# D3 — Integration & Data Contract Specification

**Project:** Digital Health Insurance Platform — MVP 1
**Document type:** D3 — the contracts between the three systems, and the mocks that stand in for them
**Version:** 2.3
**Prepared by:** BnK Solutions — Business Analysis
**Date:** 25 August 2026 (v2.3: the review corrections — **§I-05 gains a fifth property**, because the payload contracted an inheritance rule for co-payment and for none of the other four inheritable properties; the `ruleSetRef` **lookup with nothing to look up** is stated and opened as **X-20**; §5.1's ownership table covers all sixteen seed collections instead of eleven, and gains the `accountNameVerified` field mapping and the two `service_category_benefit_map` sentinels; **G-8 stops naming I-01 as a source of the snapshot**; the benefit tree's **count mapping is stated and its example corrected**; §7's register is repaired — `X-15` and `X-16` restored, `X-12` struck as closed, the series now X-01 … X-20; and three fixture gaps that made three contracted behaviours unmockable are closed in `A5_seed_v1.5.json`. **No interface changes shape and no status moves.** v2.2, 24 August 2026: the term layer's two vendor questions — **`C-IMO-13`** on `ruleSetRef` granularity and **`C-IMO-14`** on mid-term endorsements — and the IDP extraction payload's two additions, **`Q-IDP-05`** and **`Q-IDP-06`**; `serviceCategory` confirmed present and widened to twenty. v2.1: `I-05` gains four confirmed pieces of content and does not move grade; `I-14`'s branch B is withdrawn; two guarantees are restated for P-02's new exception)
**Baselined against:** *Solution Framework v2.5* §6 (integration inventory), §4 (capability baseline), §3.1 (P-02's one exception) and §9.1 (C-IMO-10, C-IMO-11, **C-IMO-13, C-IMO-14**)
**Supersedes:** *D3 — Integration & Data Contract Specification v1.1* (17 August 2026); *D3 v2.0* (21 August 2026); *D3 v2.1* (24 August 2026); *D3 v2.2* (24 August 2026)
**Companion:** *A4 Document-Type & Extraction-Field Catalogue v1.3* · *A5 Product Data Sheet v1.5* · `A5_seed_v1.5.json` · *A1 Glossary v1.5* (the closed fact vocabulary this document's payloads have to feed, and from v1.5 the chain step each fact is available from)
**Classification:** Internal — contains no client-identifiable material

---

## 0. What this document is, and the one thing it is for

The Priority Plan splits this work in two, and the split matters: **P0-4a** is the contracts — this document, complete — and **P0-4b** is the sixteen mocks that have to stand behind them, which **is not built**. Both unblock the entire build, and only one of them is done.

Its purpose is narrow and worth stating plainly: **no development team should ever be blocked waiting for another party to answer a question.** Every one of the sixteen interfaces below is specified completely enough to code against, and every one has a mock specified beside it, in enough detail that building it is transcription rather than design. When the real system arrives, the mock is swapped out and nothing above the transport layer changes.

That is only achievable because of principle **P-06** in the framework — *depend on the interface, not the implementation*. This document is where P-06 becomes sixteen written contracts. It becomes a working property of the system only when P0-4b builds the sixteen mocks; until then P-06 is a principle we have specified, not one we have.

**One half of P-06 is gone, and v2.0 is where that landed.** Until v1.5, six of these contracts had two possible implementations — InsureMO or the Policy Data Store — and the second one was ours. The store is not built (Framework §2.1), so those contracts now have exactly one implementation and it belongs to someone else. Contract-first still does what it always did for the *schedule*: the mock is written from the contract, the claim engine is built against it, and the build never waits. It no longer does anything for the *outcome*. If InsureMO cannot serve a contract as written, there is no second place to serve it from, and the cost is a build rather than a swap — Framework **RK-14**.

**What v2.1 is for, in one sentence.** On 24 August 2026 InsureMO answered seven outstanding questions and the client took one decision on its own account, and **five of the eight land on this document**: four of them add content to `I-05`, one closes `I-14`'s open branch, and one — P-02's new bounded exception — reaches two of the cross-interface guarantees in §4, which is the part the brief for this reissue did not anticipate and the part a reader should look at first.

**What this document does not do.** It does not specify screens, validations or business rules — those are D2. It does not restate the adjudication rule chain — that is A5 §6. It does not define the extraction field catalogue — that is A4. Where a payload carries a field defined elsewhere, this document names the source rather than copying the definition, so there is one place to change it.

### 0.1 Confidence status — read this before relying on any contract

Each interface carries a status. It is not decoration; it tells a developer how much to trust the shape they are coding against.

| Status | Meaning | How many |
|---|---|---|
| **Confirmed** | The contract is ours to define, **or** the counterparty has confirmed it in writing. Build against it directly | **8** — I-07, I-08, I-09, I-10, I-11, I-12, I-15, I-16 |
| **Preliminary** | The counterparty has confirmed the capability, we have already seen shapes it produces, and only the exact payload is outstanding. Expect field-level, not structural, change | **0** — the band is empty in v2.0; see below |
| **Provisional** | The shape below is our reasoned specification, not the counterparty's. **Build against the mock only** | **8** — I-01, I-02, I-03, I-04, I-05, I-06, I-13, I-14 |

**Eight of sixteen can be built now, and eight are Provisional.** The arithmetic is unchanged from v1.1 and everything underneath it has moved.

> **And it is unchanged again at v2.1, for the second reissue running, and again the reasons underneath it moved.** Eight / zero / eight. **Nothing was promoted and nothing was demoted on 24 August 2026**, on a day when seven vendor questions were answered favourably — which is a result worth explaining rather than presenting as a non-event. **§0.1a does the explaining.** The short version: what arrived was **content**, and a status in this document tracks **who stands behind a shape**. Those are different things, and the day they stop being different is the day the sandbox opens.

**Where the eight Confirmed came from.** Six are IDP interfaces — I-07, I-08, I-09, I-10, I-15 and I-16 — and they are Confirmed because the IDP team confirmed **every** capability in writing on 21 August 2026, including the two that were expressly outside the preliminary confirmation of 17 August: the clinical appropriateness assessment (C-IDP-09) and the versioned clinical knowledge base (C-IDP-10). I-11 and I-12 are Confirmed for the reason they always were: in MVP 1 the counterparty is a stub and the contract is ours.

**Where the eight Provisional came from, and why two of them are new.** Six are InsureMO interfaces — I-01 through I-06 — and Framework §6 is explicit that D3 keeps all six Provisional until C-IMO-03, C-IMO-04 and C-IMO-05 return the schemas. I-13 is the premium-payment mock, provisional not because a vendor has to answer but because boundary condition BC-3 (Framework §9.4) forbids real money in MVP 1. **I-05 and I-14 are demotions**, and they are the substance of this reissue:

- **I-05 was Confirmed.** It was Confirmed under the first limb of the definition — *the contract is ours to define* — because both ends were ours: the Claim Platform at one end, the Policy Data Store at the other. The store is not built. The far end is now a vendor whose request and response schemas we have never seen. Nothing about the payload got worse; what got worse is our standing to promise it. A contract you write against yourself and a contract you write against a vendor are different documents even when the JSON is identical, and pretending otherwise is how a developer ends up coding a guess as though it were an agreement.
- **I-14 was Confirmed.** Its first phase still is ours — upload, validate, report, commit. Its second phase, the onward write of the member list into InsureMO, depends on a capability nobody has confirmed exists (**C-IMO-10**). An interface is graded at its weakest part, because a status is a warning to a developer and a warning that covers only half an interface is not a warning.

**Why the Preliminary band is empty, and why it is not retired.** Preliminary is a promise about the *size* of the expected change — field-level, not structural. That promise can only be made about a counterparty whose shapes we have already seen. It was the right grade for the IDP interfaces in v1.1, because BnK's IDP platform had a documented integration pattern and a working financial-statement implementation we could read. It is **not** the right grade for InsureMO, even though C-IMO-03/04/05 read "capability confirmed, contract detail open" and sound like the definition: we have never seen an InsureMO payload of any kind, so structural change is entirely possible and promising otherwise would be a courtesy to a vendor at a developer's expense. IDP has moved past the band; InsureMO has never entered it. The band stays defined because the next confirmation may land in it.

### 0.1a The 24 August answers, and why no status moved

Framework **§9.1** records seven answers from InsureMO and one client decision, taken on 24 August 2026. Five of them reach this document. They were assessed one interface at a time, against the definitions in the table above, and the conclusion in each case is written out here rather than left to be inferred from an unchanged table — **an unchanged status with no stated reason is indistinguishable from an unreviewed one.**

| Interface | What arrived | Grade | Why |
|---|---|---|---|
| **I-05** | **Four of its unknowns closed.** The renewal status / continuous-cover start is returned **at member-on-policy level** (B1); the policy's **rule-set / wording reference** is exposed (B2); InsureMO can return a coverage verdict **and can return the facts instead**, which is what we take (B5); and **customer code, distribution channel, agent and customer group** all arrive in the ordinary payload (B6) | **Provisional — unchanged** | **This is the honest answer and it is not a close call.** *Preliminary* is a promise about the **size** of expected change — field-level, not structural — and this document has already stated the only ground on which that promise can be made: *"we have never seen an InsureMO payload of any kind, so structural change is entirely possible."* **Four answers in prose do not change that sentence.** We now know four things the payload contains; we do not know its shape, its field names, its types, its nesting, or — for the renewal attribute specifically — the **level** it is held at. Promoting on this evidence would be a courtesy to a vendor at a developer's expense, which is the exact phrase the band was defined with, and it would be worse here than in v2.0 because a developer reading *Preliminary* would reasonably stop writing defensive adapter code |
| **I-14** | **C-IMO-10 is confirmed: InsureMO exposes a bulk member load, for creation *and* end-dating** (B7). **X-12 closes with it.** Branch B — a claim-side member extension and two member lists — is **withdrawn** | **Provisional — unchanged, and its cause is replaced rather than removed** | v2.0 demoted I-14 because *"its second phase depends on a capability nobody has confirmed exists."* **That cause is gone.** A second one is now load-bearing in its place: phase two writes into **a vendor whose schema we have not seen**, which is precisely why I-02, I-03 and I-06 are Provisional. §8's demotion rule says a demotion names its cause; **the same discipline applies to a status that stays put while its cause is replaced**, and §8 now says so. **Note the shape of this, because it is counter-intuitive and worth reading twice:** had the answer been *no*, phase two would have been entirely ours, and I-14 would have returned to **Confirmed** under the first limb — *the contract is ours to define*. **The better outcome for the architecture keeps the weaker grade on the interface.** That is not a defect in the scheme; it is the scheme working. A status measures standing, not quality |
| **I-01, I-02, I-03, I-06** | Nothing directly. B6's four attributes are policy-register content and arrive through I-05 | **Provisional — unchanged** | C-IMO-03 and C-IMO-05 are re-graded in the Framework to *capability confirmed; content described, schema unseen*. That is a better description of what we know and it is not a different instruction to a developer |
| **I-04** | **Nothing.** None of the eight answers addresses the policy-issued integration | **Provisional — unchanged** | Stated explicitly because I-04 is the interface most likely to be swept along by a good day. **C-IMO-04 is untouched**, X-10 is untouched, and the compensating control — `GET /v1/policies?issuedSince=` — is still an endpoint we specified and have never seen |

**Two open items close, and two are created.** **X-12** closes on B7. Framework **C-IMO-10** closes with it. **X-15** and **X-16** are new, and both come from the answers rather than from anywhere else: the renewal attribute's exact name and its level, and whether the rule-set reference carries a version. §7 carries them.

**The deliberate position recorded in Framework §12 has changed, and it changed against us.** v1.1 said InsureMO touched 6 of 49 features and 5 of 16 interfaces, that the claim side was untouched, and that P-06 already designed the fallback. Framework v2.0 replaced all three clauses and **v2.5 carries the replacement unchanged**: InsureMO touches **7 of 48 features** (F01–F07) and **6 of 16 interfaces** (I-01…I-06); the claim side is **not** untouched, because InsureMO now holds data that **every** claim reads; and **there is no fallback**. What is still open on InsureMO is contract detail rather than capability, which is why the build proceeds — but the exposure is no longer bounded to six contracts. It is **RK-14**, and it is mitigated by getting hands on a sandbox early (**C-IMO-06**), not by anything written here.

### 0.2 What changed in v2.3

**Nine corrections, no new contract and no moved grade.** Five of the nine are places where this document stated a rule for one field and left the same rule unstated for the fields beside it — which is the failure mode a contract specification is most prone to, because the sentence that is present looks like the whole of the rule. Three more are places where a **list** was short: a table of sixteen collections that carried eleven, and a register of twenty open items that carried seventeen. **A short list is the more dangerous of the two failures**, because the rows that are present make the ones that are absent look considered.

| # | Change | Why |
|---|---|---|
| 1 | **§I-05 gains a fifth property: waiting periods and the deduction pair resolve to the nearest ancestor carrying them, at snapshot time** | **Property 4 makes `coPaymentBp` mandatory on every node with an explicit `"INHERIT"` string, and argues at length that omitting a property means *"none"*. The same payload then puts `annualSumInsured`, `deductionRatioBp`, `deductionCondition` and `waitingPeriodDays` on node `200` and omits all four from `200.1` and `200.2`.** By property 4's own reasoning an absent `waitingPeriodDays` on `200.1` means **no waiting period** — which silently turns a 365-day `PRE` wait into zero on every child in the tree. The seed holds waiting periods at **group level only**, so this is not hypothetical: it is what the fixture produces |
| 2 | **§I-05 states that `ruleSetRef` is a `lookup()` with nothing to look up, and opens `X-20`** | D2 §5.28.3 F2 is `lookup(ruleSetRef)`, sourced `imo:I-05`. **I-05 returns the rule-set reference for *one member's coverage*. No interface — I-01 through I-06 — lists the rule sets available to a term author**, so the control has no list behind it. The vendor half may need an I-01 operation and is not ours to decide; **the fallback is specified so nothing waits** |
| 3 | **§5.1's ownership table covers all sixteen seed collections. It covered eleven** | **§5.1 is what a mock builder splits the loader on**, and five collections were absent from it: `condition_class` — the only **Shared reference** one in the fixture — `service_category_benefit_map`, `decline_reason`, `adjudication_rule`, and `provider`, new at seed v1.5. A collection with no row in this table has no stated side of the split |
| 4 | **§4 G-8 stops saying I-01 populates the snapshot** | G-8 read *"I-01 and I-05 populate the snapshot"*. **§I-01 says the opposite in the same document**: its output is **design-time**, feeding the adjudicable product model under **P-03**, and only I-05's output is snapshotted under **P-02**. Two sections, one page apart, on the guarantee that makes a settled claim reproducible |
| 5 | **Three fixture gaps are closed in `A5_seed_v1.5.json`, and each of them made a behaviour this document contracts unmockable** | **(a)** No `policy` record carried a rule-set reference, while all ten `policy_term` rows bind to one — **chain step 4 selected zero terms in every mock**, and reported a clean pass. **(b)** No `member` carried `continuous_cover_from`, so §I-05's three required mock shapes could not be built and neither A5 §5's renewal waiver nor the bounded re-read had a fixture. **(c)** There was **no `provider` collection at all**, so `PROVIDER_OFF_NETWORK` could not be evaluated — D2 §5.17: *"if the provider master is not loaded, every claim the deduction reaches is cut by 20%."* |
| 6 | **§I-05 states the count mapping, and the benefit-tree example is corrected to match the fixture** | The example showed node `200` with **`"countUnit": "VISIT"` beside `"maxCount": 0`** — a count unit on a benefit that has no counter — where the seed row carries **`count_unit: null`**. The seed and the payload this document says is seeded from it disagreed. **The example is corrected and, more importantly, the rule is stated**: `count_unit: null` returns `countUnit: null` and is **never** mapped to `0` or to a unit. **`0` is the unlimited sentinel and `null` is *not known*; §1.2 fixes that distinction, so mapping one silently onto the other is precisely what §1.2 forbids** |
| 7 | **§5.1 gains §5.1.1 and §5.1.2 — the `accountNameVerified` mapping, and the two sentinels in `service_category_benefit_map`** | **`payout_account.verified` in the seed and `accountNameVerified` in §I-11 and §I-05 are one field under two names**, and no document said so. The mapping is recorded here rather than the field renamed, because the fixture is owned by A5 and already reissued. Separately, the mapping table carries **`"NONE"`** — *no benefit in this treatment setting* — and **`"ASSESSOR"`** — *a person decides* — and **both were undocumented sentinels of exactly the kind §I-05 documents `"INHERIT"` as**. An undocumented sentinel is how a loader guesses |
| 8 | **§7's table is repaired: `X-15` and `X-16` are restored, `X-12` is struck as closed, `X-20` is added. The series runs X-01 … X-20** | **§0.1a, §0.2.2 and §I-05 all cite `X-15` and `X-16`; the table skipped from X-14 to X-17.** The numbering was never wrong — the table was incomplete, and a cross-reference to a row that does not exist is worse than a missing row, because it reads as though somebody checked. `X-12` closed on answer B7 at v2.1 and was still listed as open |
| 9 | **Cross-references re-pointed to the current parallel versions** | The fixture is **`A5_seed_v1.5.json`** in §5, §5.1 and §I-05, where three of them still read v1.4; the sandbox evaluation in §6 is against **A5 v1.5** rather than the v1.2 the original demonstration used. **Statements inside a changelog block are left alone** — they record what was true at a version, and re-pointing them would destroy the only record of when something changed |

**One requirement is withdrawn rather than corrected, and it is listed rather than absorbed.** **§5.1's second consequential requirement on the fixture — the branch-B divergence case — is struck.** It asked the seed to carry one member present in a claim-side extension and absent from InsureMO, so that the reconciliation obligation of two member lists would show up in every test run. **Answer B7 confirmed the bulk member load and branch B is withdrawn, so there is nothing left to diverge.** It was withdrawn in prose at v2.1 and left standing in the list until now; it is **struck and annotated rather than deleted**, on the same principle that keeps `X-12` in §7 with its number.

> **What v2.3 does not do.** **No interface changes shape, no status moves, and the split stays 8 / 0 / 8.** Property 5 is not a new field: it is a rule about fields the payload already carries, and the adapter already had to do the resolution for `coPaymentBp`. **`X-20` is a question, and §8's third rule is explicit that a question opened is not a status changed** — it is opened here precisely so that the answer, whenever it comes, arrives at a build that never waited for it. Nothing in this reissue was confirmed or withdrawn by a counterparty, and **a correction is not a confirmation** any more than a question is (§8).

#### 0.2.1 What changed in v2.2 — retained, because v2.3 builds on it

**One capability reaches this document and it does not change a single contract's shape.** The ten decisions of the *Solution Paper — Policy Terms, Rule Engine and Limit Management v1.1* were approved on 24 August 2026 and put the policy's terms into the claim system as evaluable data (Framework v2.4, F65–F68). **A term engine is only as good as the facts it can read**, and the facts come from here — which is why a document about integration has anything to say about it at all.

| # | Change | Why |
|---|---|---|
| 1 | **Baselined against *Solution Framework v2.4***, which records the ten decisions and opens `C-IMO-13` and `C-IMO-14` | The term register binds to a key this document already returns. Everything below follows from that one sentence |
| 2 | **§I-05: `ruleSetRef` is recorded as the key the claim-side term register binds to**, and **`C-IMO-13` is opened** — does the rule set come back as **codes we can bind terms to**, or as prose only? | It **does not block.** If codes are unavailable we bind the register to `ruleSetRef` alone and accept **one register per rule-set version**. Recorded because *"a field that opens nothing"* was the state §5.4.2 of D2 described before v2.1, and this is what it now opens |
| 3 | **§I-05: `C-IMO-14` is opened, and it BLOCKS** — does a **mid-term endorsement** produce a new `ruleSetRef`, or a new benefit-schedule version, or does it **change fields silently**? | **The silent case makes D1 §5.9.5 unimplementable as written.** The mid-term benefit-level rule says consumption carries and the cap changes, and the ledger stores the cap as at the posting — **all of which requires the snapshot to be able to detect that the cap moved.** If InsureMO edits the schedule in place, it cannot |
| 4 | **§I-09: the extraction result's `lines` array gains two requested fields** — **`facilityCountry`** (**`Q-IDP-05`**) and a **treatment-day count per line** (**`Q-IDP-06`**) | Both are **facts the term engine and the ledger need and do not have** (A1 §4.11). The first is the whole of exclusion #10, *treatment received outside Vietnam*; the second is what a `DAY` counter increments by. **A4 §8 owns the payload and this document names the requirement** |
| 5 | **`Q-IDP-02` — line-level ICD — is recorded as already open, and its priority RISES** | It no longer blocks only the clinical track. **It now also blocks two policy exclusions** — congenital conditions (ICD chapter Q) and the infertility/reproduction group — which is a different kind of consequence: a track that reasons less well, against a contractual term that cannot be evaluated at all |
| 6 | **`serviceCategory` is confirmed present and already carried, and the set widens from sixteen to twenty** | **An earlier draft wrongly recorded it as missing. It is not, and it never was** — §I-15 has carried the canonical list since v1.1 and A4 §4.5 owns it. **A5 v1.4 adds `SCREENING`, `VACCINATION`, `OPTICAL` and `HEARING`**: Claim Platform master data, **no IDP dependency, no vendor question, one seed change** |
| 7 | **The IDP question series is renumbered `Q-IDP-nn`, and this document is the reason** | The vendor register identified IDP questions as `I-9` … `I-12`, and **`I-01` … `I-16` mean interfaces here.** §2 carries the note. **No identifier in this document changes** |
| 8 | **§5.1: the fixture is `A5_seed_v1.4.json`**, and it gains a `policy_term` collection that is **Claim Platform master data and not mock fixture** | The seed's `product_exclusion` becomes `policy_term` at v1.4. It is the same distinction §5.1 already draws for `payout_account` and `beneficiary`, and getting it wrong empties the term register the day the sandbox arrives |
| 9 | **§6 gains two rows and §7 gains three open items — X-17, X-18, X-19** | The X series stays dense: X-01 … X-19, none retired, none reused. **Two of the three are vendor questions this document is the consumer of rather than the owner of**, and they are carried here because §7 is where a developer looks for what is not yet answered |
| 10 | **`I-05` returns facts, not a verdict — restated deliberately and not merely left in place** | **P-01.** A term register keyed on a vendor's `ruleSetRef` is the point at which somebody will propose taking the vendor's predicates too. §I-05 says why we do not, and *Solution Paper* §2.6 rejected exactly that option |

> **What v2.2 does not do, said plainly because the absence is a decision.** **No interface changes shape, no status moves, and the split stays 8 / 0 / 8.** Nothing was confirmed and nothing was withdrawn on 24 August evening; what happened is that a new consumer — the term engine — turned out to need two facts nobody had asked for and one key nobody had asked the granularity of. **A question opened is not a status changed** (§8), and a reissue that moved a grade on the strength of a question we have just written would be recording confidence nobody holds.

#### 0.2.2 What changed in v2.1 — retained, because v2.2 builds on it

| # | Change | Why |
|---|---|---|
| 1 | **Baselined against Solution Framework v2.2**, which records the eight decisions of 24 August 2026 | Five of the eight reach this document. §0.1a grades each one against the interface it touches, and explains why the 8 / 0 / 8 split did not move |
| 2 | **§I-05 gains three pieces of confirmed content**: `continuousCoverFrom` (the renewal / continuous-cover attribute, **name pending the schema — X-15**), the policy's **rule-set reference**, and the **four policy attributes** from B6, returned on member search rather than on the coverage read | B1, B2 and B6. `continuousCoverFrom` is the input A5 §5's continuous-renewal rule has been specified against and unable to run on since v1.2. The four attributes restore the columns register row **B-14** dropped |
| 3 | **§I-05 states plainly that it returns facts and not a coverage verdict — and that InsureMO can return a verdict, which we deliberately do not consume** | B5. v2.0 said *"if InsureMO offers its own coverage verdict, we do not consume it"* — a precaution against a possibility. It is now a **choice against a confirmed capability**, which is a stronger statement and a **P-01** one |
| 4 | **§I-05 gains the controlled re-read** — the contract side of Framework P-02's one exception (§3.1) | A1. One field, before `PENDING_APPROVAL`, logged, marking the calculation stale. Specified here because it is a second call on an interface whose whole design assumed one |
| 5 | **§I-14: branch B is withdrawn. The staged-commit design is rewritten around the confirmed path**, and `Re-import`'s end-dating is confirmed rather than assumed | B7 closes C-IMO-10 and **X-12** in one answer. There is **one** member list. What survives is the staged commit, which is the real cost of a remote write, and it is now the whole of phase two rather than one branch of it |
| 6 | **`policy.source` keeps its place on the coverage response and loses its job** | It was the branch-B routing discriminator — *disjoint by source*, the adapter routing on it. With branch B withdrawn nothing routes on it. It stays as **provenance**, and §I-05 says so, because a field whose stated purpose has evaporated is a field somebody will later implement against the stated purpose |
| 7 | **§4: G-1 and G-8 are restated for P-02's exception** | **This is the part the brief for this reissue did not anticipate.** G-1 guaranteed the snapshot is *"written exactly once"*; G-8 guaranteed *"every value the engine adjudicates against is read from the claim's snapshot."* A renewal re-read breaks the first and leaves the second intact, and a document that added the exception in §3 while leaving §4 asserting the opposite would be contradicting itself two sections apart |
| 8 | **§5.1: the fixture is `A5_seed_v1.3.json`, its metadata correction is recorded as done, and the branch-B divergence requirement is withdrawn** | v2.0 raised two requirements on the fixture. The first — that `meta.policy_data_note` stop describing the Policy Data Store — was met in the seed's own v1.3. The second existed only for branch B and goes with it |
| 9 | **Two open items close (X-12, and Framework C-IMO-10 with it); two are added, X-15 and X-16** | The X series stays dense: X-01 … X-16, none retired, none reused. **Both new items come from the answers themselves** — the level a renewal flag is held at, and whether a rule-set reference carries a version. An answer that raises a sharper question is a good answer, and the sharper question has to be written down |
| 10 | **§8 gains a third rule: a status that stays put while its cause changes must say so** | I-14 is Provisional for a completely different reason than it was on 21 August. A scheme that records only movements loses the reason behind everything that did not move |

#### 0.2.3 What changed in v2.0 — retained

| # | Change | Why |
|---|---|---|
| 1 | **Baselined against Solution Framework v2.0. Three systems, not four** | InsureMO confirmed, demonstrated against A5 v1.2, that it can carry the adjudicable benefit schedule — the tree, child-to-parent aggregation, count limits with an unlimited sentinel, waiting periods across the six condition classes, inherited co-payment, and the deduction ratio with its condition. The reason the Policy Data Store existed has gone (Framework §2.1) |
| 2 | **I-01 reverses direction: Claim Platform → InsureMO** | It was a push from InsureMO into a store we wrote. It is now a pull from a system we read. A reversed direction also loses its trigger — see §3, I-01, where the trigger is decided rather than assumed |
| 3 | **I-04 is re-targeted: InsureMO → Claim Platform** | The policy-issued event has nowhere else to land. Its *failure* changes meaning as well as its address — a missed I-04 no longer produces a wrongly declined claim, because I-05 reads InsureMO live at registration. It now produces a policy its owner cannot see |
| 4 | **I-05 is re-targeted to InsureMO and demoted Confirmed → Provisional** | §0.1. The payload is unchanged in substance; our standing to promise it is not. `payoutAccount` and `beneficiary` are **removed** from the response — they are Claim Platform master data now (F58) |
| 5 | **I-14 is re-targeted: Excel → Claim Platform, with the onward write conditional on C-IMO-10.** Confirmed → Provisional | The loader is ours whatever it writes into (Framework §2.1). Both branches are specified, including what two member lists cost and the rule that makes them decidable |
| 6 | **I-07 … I-10 move Preliminary → Confirmed; I-15 and I-16 move Provisional → Confirmed** | The IDP team confirmed every capability in writing on 21/08/2026 — C-IDP-03, C-IDP-04, C-IDP-05, C-IDP-07, C-IDP-08, and the two that were outside the 17/08 confirmation, C-IDP-09 and C-IDP-10. Framework §9.2: *"D3 stands as written; no contract changes"* |
| 7 | **The split is 8 Confirmed / 0 Preliminary / 8 Provisional** | It was 4 / 4 / 8. The total that can be built now is the same number for entirely different reasons, and the document says so rather than letting the unchanged eight look like nothing happened |
| 8 | **§1.4 loses the internal-service-token row** | There is no internal counterparty left. The most-used read in the system now crosses an organisational trust boundary, and policy and member data sit outside our deployment boundary — which is also what sharpens C-IMO-07 |
| 9 | **§1.6 gains a fifth adapter responsibility: own the shape** | Where we do not own the counterparty, the payloads in §3 stop being a description of what arrives and become a specification of what the adapter must **produce**. This is the only honest form contract-first can take against a vendor |
| 10 | **§4: G-1 is restated and G-8 is added** | "One consistent read" was a property of a store we controlled. Across two calls into a vendor there is no transaction and no read-consistency guarantee. The guarantee survives as a requirement on the **snapshot** (P-02) and on the adapter, not as a property of the counterparty |
| 11 | **§5 states the shared fixture's ownership** | `A5_seed_v1.2.json` still carries `policy`, `benefit_level`, `member`, `payout_account` and `beneficiary`. They do not disappear: the first three seed the **InsureMO mock**, and the last two seed the **Claim Platform's own master data** (F58) and are not mock data at all |
| 12 | **§6 is rewritten. The old fallback table is void** | Every row of it ended *"master it in the Policy Data Store, and the claim engine never changes."* There is no store. The first three rows are no longer fallbacks; they are RK-14 triggers, and the cost column now reads *a build* |
| 13 | **Six open items added: X-09 … X-14** | Every one of them is a question that did not exist while we owned both ends: a vendor's error shape, a vendor's webhook signing and delivery, read consistency across two vendor calls, bulk update as distinct from bulk create, a vendor's idempotency and rate limits, and whether a vendor echoes our correlation id |
| 14 | **§8 gains a demotion rule** | v1.1's change control only described promotion. This reissue demotes two interfaces, and a scheme that can only move one way is a scheme that will eventually lie |

---

## 1. Conventions that apply to every interface

These are not negotiable per-interface. A contract below that does not mention idempotency inherits the rule from here.

**One thing to read first, because v2.0 changes who these rules bind.** Every convention below is either a rule we impose on ourselves, or an expectation we hold of a counterparty. While the Policy Data Store existed, most of them were the first kind. They are not any more. Each table therefore states, where it differs, **what we do** and **what we require of InsureMO** — and a requirement on a vendor that has not been agreed is an assumption, named as one.

### 1.1 Transport and shape

| Convention | Rule |
|---|---|
| **Protocol** | HTTPS only, TLS 1.2 minimum. No plaintext, in any environment, including local development |
| **Format** | `application/json; charset=utf-8` on request and response. The only exceptions are I-14 (multipart file upload) and the e-invoice XML path inside I-07 |
| **Versioning** | Version in the path: `/v1/…`. A breaking change increments the path version and both versions run in parallel for one release. Additive fields are **not** breaking and do not increment. **This binds the interfaces we serve.** For the six InsureMO interfaces we control neither the scheme nor the release cadence, and we cannot require a vendor to run two versions in parallel for our convenience. What we do instead is §1.6 responsibility 2 — validate every response against the schema at the boundary, so a vendor's unannounced change fails loudly at the adapter rather than three layers deeper. **Assumption to confirm — X-01** |
| **Unknown fields** | Every consumer must ignore fields it does not recognise. This is what makes additive change safe, and it must be tested, not assumed. We can test our side; on the InsureMO side we can only test that *we* tolerate additions, which is the half that protects us |
| **Casing** | `lowerCamelCase` for JSON properties, everywhere, including inbound payloads we do not control (we normalise at the adapter boundary). **This rule was written for the case that has now become the common one**, and it carries more weight in v2.0 than it did in v1.1 |
| **Enumerations** | `UPPER_SNAKE_CASE` string constants, never integers. An unrecognised enum value is an error, not a silent default. **Whose enums?** Ours, on the wire the claim engine sees. InsureMO's enum values are its own and the adapter maps them — `policyStatus`, the condition-class codes, count units, the `0 = unlimited` sentinel, the `"INHERIT"` co-payment marker and, from v2.1, **whatever InsureMO uses to express renewal status** — **every one of them our invention or our naming, and none of them anything a vendor is obliged to share.** The mapping table is an artefact, not a line of code, and it is written when the schemas arrive. **v2.1 makes the renewal attribute the item to watch on that table**, because it is the only one whose *level* is also in question and not merely its vocabulary — X-15 |

### 1.2 Identity, time and money

| Convention | Rule |
|---|---|
| **Correlation** | Every request carries `X-Correlation-Id` (UUIDv4). If absent, the receiver generates one and returns it. It is propagated through every downstream call and written to every log line. One claim's full journey across **three** systems must be reconstructable from this one value — **and see G-5, which now states where that reconstruction stops** |
| **Claim reference** | `claimNo`, format `CLM-YY-NNNNNN` per Framework §7.5. Insured events within a claim are `CLM-26-000417/1`. Never parsed for meaning |
| **Timestamps** | ISO-8601 with explicit offset: `2026-08-21T14:32:05+07:00`. Stored in UTC, displayed in `Asia/Ho_Chi_Minh`. A timestamp without an offset is rejected |
| **Dates** | `YYYY-MM-DD`, no time component, no timezone. A service date is a calendar date and adding a timezone to it causes off-by-one-day bugs at the policy-period boundary |
| **Money** | **Integer VND.** No decimals, no separators, no floating point, ever. Consistent with A4 §2. A payload carrying `1500000.0` is rejected as malformed. **The adapter converts** whatever InsureMO returns — a decimal, a string, a minor-unit integer — and a conversion that loses precision fails rather than rounds |
| **Percentages** | Integer basis points (`2000` = 20.00%), not decimals. Co-payment and deduction ratio both use this. **Both are policy terms** — neither is ever set by a user (D1 B-09). A5 §10 fixes the conversion `basis points = pct × 100` in exactly one place; with the schedule mastered in InsureMO that place is now the **adapter**, not the seeding script, for every path except the mock's own seed |
| **Nulls** | `null` means *not known*. An empty string means *known to be empty*. They are different, and a payload must never substitute one for the other. A4 §2 goes further for extraction specifically: a missing value is written as `null`, **never** as an empty string, and never guessed |
| **Identifier naming** | One name per concept across every payload: `claimNo` for the claim (never `claimRef`), `eventNo` for an insured event, `requestId` for an extraction request, `assessmentId` for a clinical assessment, `instructionRef` for a payment instruction, `importId` for an import run. **`policyNo` and `memberId` are now InsureMO's identifiers**, carried unchanged; we mint neither, and the adapter must not normalise their format. **`ruleSetRef`, added at v2.1, is a third identifier of that kind** — InsureMO's, carried unchanged, never parsed for meaning, and **not yet known to carry a version** (X-16) |

### 1.3 Idempotency and retry

The MVP settles money, and a duplicate payment instruction is the worst failure this system can produce. So:

| Rule | Detail |
|---|---|
| **Idempotency key** | Every `POST` that creates or triggers something carries `Idempotency-Key` (client-generated UUIDv4). The receiver stores the key with the response for **24 hours** and returns the stored response, unchanged, on a repeat. **This is an obligation on the receiver.** Where the receiver is ours (I-08, I-12, I-14 phase 1) it is specified and delivered. Where the receiver is IDP it is confirmed (C-IDP-04). Where the receiver is **InsureMO — I-02, I-03, I-06 and I-14's onward write — it is a requirement we are stating, not a behaviour we have been promised.** **v2.1 moves I-14's onward write from hypothetical to real**: C-IMO-10 confirms the bulk load exists, so *whether InsureMO honours an idempotency key on it* is now a live question about an interface we are certainly building, and it governs whether a re-drive after a partial sync completes or duplicates — see §I-14, and **X-13**. If InsureMO does not honour an idempotency key, a retried application submission issues a second policy, and the compensating control is a business-key check before every retry: never re-submit an application whose `quoteId` already has a `policyNo`. **X-13** |
| **Same key, different body** | Returns `409 CONFLICT`. This is a client bug and must be loud |
| **Retry policy** | Exponential backoff before each retry: 1s, 2s, 4s, 8s, 16s — **five retries, six attempts in total** — then stop and raise an operational alert. **Against a vendor this is also a load decision.** Six attempts from every adapter under an outage is a burst we would previously have aimed at our own store. InsureMO's rate limits and its `429` behaviour are unknown — **X-13** — and until they are known the circuit breaker (G-6) is the control that matters more than the backoff |
| **What is retryable** | `5xx`, timeouts, connection failures, and `429`. A `4xx` is otherwise **never** retried — it says the request is wrong, and repeating a wrong request produces the same wrong answer while adding load to a counterparty that has already refused it |
| **The one retryable `4xx`** | **`409 NOT_READY`, and nothing else.** It is the single status in this specification where `4xx` does not mean *your request is wrong* but *the answer does not exist yet* — I-09 returns it while an extraction is still running, I-16 while a clinical assessment is. It **must** be retried, honouring the `Retry-After` the response carries in place of the backoff above; an adapter that applies the general rule here abandons every extraction and every assessment that was not instantly ready. It is distinguished from an idempotency `409` by the problem `type`, not by the status code — see §1.5. **Both interfaces that use it are IDP interfaces, and IDP has confirmed the behaviour.** No InsureMO interface uses it, and none should be assumed to |
| **Timeouts** | Connect 5s, read 30s, except I-07 (submit package) which allows 60s read for large uploads. **I-05 is the read that a customer waits on**, at claim registration; if InsureMO's latency does not fit 30s the claim must go to the operator queue (G-3), never to a decline |
| **At-least-once** | Every webhook we receive (I-04, I-08, I-12) may arrive more than once. Handlers are idempotent on the business key, not on the delivery id — `policyNo`, `requestId` or `assessmentId`, `instructionRef`. **At-least-once is a property of the sender.** IDP's is confirmed (C-IDP-04); Finance's is a stub we write. **InsureMO's is unknown, and it may be at-most-once or fire-and-forget** — in which case I-04's polling fallback is not a fallback at all but the only delivery guarantee there is. **X-10** |

### 1.4 Authentication

| Direction | Mechanism |
|---|---|
| Claim Platform → InsureMO | OAuth 2.0 client credentials. Token cached until 60s before expiry, refreshed on `401` once, then failed. **This now covers I-01 and I-05 as well as I-02, I-03 and I-06** — five of the six, and the two added are the two the claim journey depends on most |
| Claim Platform → IDP | Service token in `Authorization: Bearer`, issued per environment |
| Inbound webhooks (IDP, InsureMO, Finance) | **HMAC-SHA256** over the raw request body, in `X-Signature`, with `X-Timestamp`. Requests older than **five minutes** are rejected. Signature verified against the raw bytes **before** JSON parsing — parsing an unverified payload is the vulnerability, not receiving it. **For I-08 and I-12 this is agreed or ours. For I-04 it is a requirement on InsureMO and nothing more** — if InsureMO signs differently, or offers only a shared secret in a header, or does not sign at all, the endpoint must compensate: mutual TLS, or source-address restriction, or treating the webhook purely as a *hint* that triggers an authenticated I-01/I-05 read rather than as data. **The last of those is the safest and it is the default until C-IMO-04 says otherwise — X-10** |

> **The row that was deleted, and why it is worth a sentence.** v1.1 carried a fourth row: *Claim Platform ↔ Policy Data Store — internal service token; both sit inside the deployment boundary; this is defence in depth, not a trust boundary.* There is no such row now, because there is no such counterparty. The most-used read in the system crosses an organisational boundary, and policy and member data — names, identity numbers, card numbers, coverage dates — are held by a third-party platform rather than by a service we run. That does not breach **P-07**, which is about medical document content and is unaffected. It does sharpen **C-IMO-07** (data residency), and Framework §9.4 records exactly that.

> **On webhook security.** A webhook endpoint is a publicly reachable, unauthenticated-by-default write path into the claim system. It is treated as hostile input: signature first, timestamp window second, parse third, and the handler does nothing that cannot be undone by a replay. **An unsigned webhook is not a smaller version of a signed one; it is a different thing, and the handler treats it as a notification rather than as a payload.**

### 1.5 The error model — one shape, everywhere

All errors, from every system we build, use RFC 7807 `application/problem+json`:

```jsonc
{
  "type":     "https://bnk.local/errors/policy-not-found",
  "title":    "Policy not found",
  "status":   404,
  "detail":   "No policy with number POL-26-004417",
  "instance": "/v1/policies/POL-26-004417",
  "correlationId": "9f2c1e6a-…",
  "errors": [                                  // field-level, only on 400/422
    { "field": "serviceDate", "code": "OUT_OF_PERIOD",
      "message": "Service date 2026-09-02 is outside the cover period" }
  ]
}
```

| Status | Used for | Retryable |
|---|---|---|
| `400` | Malformed — bad JSON, wrong type, unparseable date | No |
| `401` / `403` | Not authenticated / not permitted | No (refresh token once on 401) |
| `404` | The named resource does not exist | No |
| `409` `…/idempotency-conflict`, `…/version-conflict` | Idempotency conflict, or optimistic-concurrency version clash | No |
| `409` `…/not-ready` | **`NOT_READY` — the async result does not exist yet** (I-09, I-16). Carries `Retry-After` | **Yes**, on `Retry-After` |
| `422` | Well-formed but business-invalid — this is the interesting one | No |
| `429` | Rate limited. Honour `Retry-After` | Yes |
| `5xx` | Our fault, or the counterparty's | Yes |

**Two different `409`s, and the adapter must tell them apart by `type`, never by status.** One means *you sent this twice with a different body* and must never be repeated; the other means *come back in three seconds*. They share a status code because both are conflicts with current state, but they are opposite instructions, and an adapter that branches on `409` alone gets one of them catastrophically wrong. This is the only place in the specification where retry behaviour depends on the problem `type` rather than the status, and it is why RFC 7807's `type` is mandatory on every error we emit.

**`422` is separated from `400` on purpose.** "Service date outside the cover period" is not a malformed request — it is a correct request with a business answer, and the UI must show it to the user as a reason rather than as a system error.

**The sentence "from every system we build" was doing quiet work in v1.1, and now it has to be said out loud.** InsureMO will not return `application/problem+json` with our `type` URIs. Whatever it returns, the adapter **synthesises** a problem document from it, and the mapping from InsureMO's error shape to this table is an unwritten artefact — **X-09**. Three rules govern that synthesis, and they matter more than the shape:

1. **An unmapped counterparty error becomes a `5xx`, never a `4xx`.** A `4xx` says *we asked wrongly* and stops the retry. An error we do not recognise is not evidence that we asked wrongly.
2. **A `404` from InsureMO is not an answer about cover.** It means *we could not establish cover*, which routes to an operator (G-3). It never becomes "no cover" and never becomes a decline. This distinction cost nothing while we owned the store, because our store's `404` meant exactly one thing.
3. **`422 NO_SCHEDULE_AT_DATE` is now an adapter obligation, not a counterparty behaviour.** Our store raised it deliberately when a plan had no schedule effective on `asOfDate`. A vendor may instead return an empty schedule, or today's schedule, or the nearest one — and *today's schedule silently answering a question about March* is the single failure this error was invented to prevent. So the adapter checks the returned effective range itself and fails closed if it does not cover `asOfDate`. It does not wait to be told. See I-05, and C-IMO-05.

### 1.6 What every adapter must do

Each of the sixteen interfaces is implemented behind an **adapter** with five responsibilities, and no business logic:

1. **Normalise** the counterparty's shape into ours (casing, date formats, money units, enum names).
2. **Validate** the response against the schema before it reaches the domain. A counterparty that starts returning a new enum value must fail at the boundary, not three layers deeper.
3. **Record** the raw request and response, with the correlation id, for the retention period. For I-07/08/09 this is a compliance-relevant trace, not just debugging.
4. **Fail closed.** An adapter that cannot get a trustworthy answer returns an error. It never invents a default — an invented `coverageActive: true` pays a claim that should not have been paid.
5. **Own the shape.** *(new in v2.0)* **Where we do not own the counterparty, the payload specified in §3 is the adapter's *output* contract, not the counterparty's.** It is what the adapter must hand to the claim engine, whatever arrives on the wire. The claim engine is coded against it and does not change when the real schemas land; the adapter absorbs the difference, or fails.

> **This fifth responsibility is what contract-first actually means against a vendor, and it is worth being precise about.** Against our own store, §3's payloads were a specification of both ends and the two were the same artefact. Against InsureMO they are a specification of one end only. A developer reading I-05 below should read it as *"this is what my code receives"*, and an integrator reading it should read it as *"this is what I must construct."* Marking those interfaces **Provisional** is the honest label for the gap between the two, and it is why the demotions in §0.1 are not pessimism — they are the difference showing up in the one column a developer looks at.

---

## 2. Interface register

| ID | From → To | Purpose | Pattern | Status | Journey |
|---|---|---|---|---|---|
| **I-01** | **Claim Platform → InsureMO** | Product catalogue and benefit schedule retrieval, for the adjudicable product mapping (P-03) | Pull on product publish | **Provisional** | J1, J2 |
| **I-02** | Claim Platform → InsureMO | Quotation | Sync REST | **Provisional** | J1 |
| **I-03** | Claim Platform → InsureMO | Application submission and issuance | Sync REST | **Provisional** | J1 |
| **I-04** | **InsureMO → Claim Platform** | Policy issued: policy, members, benefits | Webhook preferred; polling fallback — style still open, C-IMO-04 | **Provisional** | J1 |
| **I-05** | **Claim Platform → InsureMO** | Member search, policy detail, coverage **as at a date**, **benefit schedule as at a date, as a tree** | Sync REST, snapshotted on use | **Provisional** *(was Confirmed)* | J2 |
| **I-06** | Claim Platform → InsureMO | Claim reference posted against the policy record | Sync REST | **Provisional**, optional | J2 |
| **I-07** | Claim Platform → IDP | Submit document package by reference | Async, returns requestId | **Confirmed** *(was Preliminary)* | J2 |
| **I-08** | IDP → Claim Platform | Processing complete | Webhook | **Confirmed** *(was Preliminary)* | J2 |
| **I-09** | Claim Platform → IDP | Retrieve structured result | Sync REST | **Confirmed** *(was Preliminary)* | J2 |
| **I-10** | Claim Platform → IDP review UI | Deep link with context | Signed URL | **Confirmed** *(was Preliminary)* | J2 |
| **I-11** | Claim Platform → Finance | Payment instruction | Stub, contract-first | **Confirmed** | J2 |
| **I-12** | Finance → Claim Platform | Payment date and reference | Stub callback | **Confirmed** | J2 |
| **I-13** | Claim Platform → Payment mock | Premium collection | Mock | **Provisional** | J1 |
| **I-14** | **Excel → Claim Platform** | Group policy and member bulk load; **onward write into InsureMO subject to C-IMO-10** | Multipart upload | **Provisional** *(was Confirmed)* | J2 setup |
| **I-15** | Claim Platform → IDP | **Submit a clinical appropriateness assessment** | Async, returns assessmentId | **Confirmed** *(was Provisional)* | J2 |
| **I-16** | Claim Platform → IDP | **Retrieve clinical recommendations** | Sync REST | **Confirmed** *(was Provisional)* | J2 |

**Eight of sixteen are Confirmed and can be built now** — I-07, I-08, I-09, I-10, I-11, I-12, I-15 and I-16. The **eight** Provisional ones — I-01, I-02, I-03, I-04, I-05, I-06, I-13 and I-14 — are built against mocks; §5 explains how the swap works. **No interface is Preliminary**, for the reason set out in §0.1.

> **Still sixteen interfaces, and no identifier is retired or reused.** The four that pointed at the Policy Data Store are **re-pointed**: I-01 and I-05 reverse direction, because a store we wrote into has become a system we read from; I-04 lands in the Claim Platform; I-14 lands in the Claim Platform with a conditional second leg. A hole in the series is how a document starts meaning two things at once, and a reused code is worse than a hole.

> **`I-01` … `I-16` mean interfaces, and from v2.2 they mean nothing else.** The vendor question register identified its four IDP questions as **`I-9`, `I-10`, `I-11`, `I-12`**, which collided head-on with four of the identifiers above: **`I-10`, `I-11` and `I-12` each meant two different things across the live set** — an open question in one document and an integration contract in this one. **The questions are renumbered `Q-IDP-01` … `Q-IDP-06`**; the vendor register carries the mapping table and the reason. **Nothing in this document changes**, and that is the point of resolving it in the other direction: an interface identifier appears in `interfaceCode` (A1 §4.9), in operator-queue rows, in adapter configuration and in nine files, and a question identifier appears in one.
>
> **`C-IDP-01` … `C-IDP-10` are a closed series and are not extended.** They are IDP **capability confirmations**, recorded in the *IDP Engagement Close-Out Note v1.0*, and §0.1 and §3 cite them as the artefacts behind six Confirmed grades. **A confirmation and an open question are opposite things**, and giving them one namespace — which an intermediate draft did — makes *"C-IDP-13"* — the provisional identifier for the facility-country question, now **`Q-IDP-05`** — read as a capability IDP has confirmed, when it is a question nobody has answered.

> **Read the two demotions together and the pattern is visible.** Six interfaces moved *up* and two moved *down*, and the two that moved down are the two whose counterparty changed from us to a vendor. Nothing was discovered about them. That is the whole point: the status column tracks who stands behind a contract, not how good the contract is.

---

## 3. The contracts

Each contract states: what it is for, when it fires, the request, the response, what can go wrong, and what the mock does. Where a field is defined in A4 or A5, that is named rather than restated. **For every interface whose counterparty is InsureMO, read the payload as §1.6 responsibility 5 requires: it is the adapter's output, not the vendor's.**

---

### I-05 — Claim Platform → InsureMO · policy and coverage queries

**Status: Provisional.** *(v1.1: Confirmed. v2.0: demoted. v2.1: content confirmed, grade unchanged — §0.1a.)* This is the most-used interface in the system and the one the entire claim journey depends on, so it is specified first — and it is the one that changed most in v2.0 and again in v2.1. It was Confirmed because both ends were ours. The far end is now a vendor whose schemas we have not seen. **Build against the mock; the shape below is what the adapter must produce, and what the claim engine is coded against.**

> **v2.1 in one paragraph.** Four of this interface's unknowns closed on 24 August 2026 — Framework **C-IMO-11**. It now carries the **renewal / continuous-cover attribute at member-on-policy level** (B1), the **policy's rule-set reference** (B2), and **four policy attributes** on member search (B6); and it is confirmed that InsureMO **can** return a coverage verdict and that we **take the facts instead** (B5). It also gains a **second, bounded call** it never had: the controlled renewal re-read behind Framework P-02's one exception (§3.1). **None of that promotes it.** What we have is a description of contents, not a payload — and one of the four contents carries a caveat we are keeping, below.

**Four operations, and one exception.** Endpoint paths are ours, exposed by the adapter; InsureMO's own paths are unknown and are mapped behind them.

```
GET /v1/members?name=&dob=&idNumber=&cardNo=      → member search
GET /v1/policies/{policyNo}                        → policy detail
GET /v1/members/{memberId}/coverage?asOfDate=      → the one derived read
GET /v1/plans/{planCode}/benefits?asOfDate=        → benefit schedule, as a tree
```

**Search rules.** At least one parameter is required; an empty search returns `400`. `cardNo` and `idNumber` are exact-match. `name` is a diacritic-insensitive contains-match, and `dob` narrows it. More than 50 results returns `422 TOO_BROAD` rather than a truncated list — a truncated list silently hides the right member. **Whether InsureMO's member search supports diacritic-insensitive matching is unknown; if it does not, the adapter narrows on the parameters InsureMO does support and filters locally, and the 50-result ceiling is applied to the filtered set.**

**There is no `issuingUnit` search parameter, and its absence is a decision rather than a gap — Framework C-BUS-10.** Whether InsureMO models an issuing organisational unit, and whether member search can filter on it, was **not asked**, because MVP 1 has exactly one organisational unit and a filter with one value is a control that does nothing. A reader who finds no unit parameter here must not conclude the platform lacks one; **nobody has looked**, and Framework S-36 records why.

**Member search result — the four attributes B6 restored**

```jsonc
{
  "results": [
    { "memberId": "MBR-000913", "fullName": "…", "dateOfBirth": "1976-05-02",
      "cardNo": "H-2026-000913", "policyNo": "POL-26-004417",
      "policyStatus": "ACTIVE", "planCode": "HC-STD",
      "customerCode":        "CUS-000271",   // ─┐ B6 — all four arrive in
      "distributionChannel": "AGENCY",       //  │ InsureMO's ordinary payload
      "agentCode":           "AG-0042",      //  │ at no extra cost to us
      "customerGroup":       "CORP_SME"      // ─┘
    }
  ],
  "resultCount": 1
}
```

**These four are display and reporting attributes, and nothing in the engine may read them.** Register row **B-14** dropped them from the search grid on the assumption that asking for them would cost something; **B6 confirms they arrive in the ordinary payload**, so they cost a column each. They are carried to the work list and to the member picker (D2 §5.2) and they are **deliberately not snapshotted onto the claim** — nothing in the adjudication chain (A5 §6.1) reads a distribution channel or an agent code, and a claim whose payable could vary with the channel it was sold through is a claim nobody can defend. **`distributionChannel` and `customerGroup` are InsureMO enumerations, mapped at the adapter, and displayed as received where the mapping has no entry** — an unmapped value here is a label, not a decision, so it degrades to a string rather than failing the boundary. That is the one place in this specification where §1.1's *unrecognised enum is an error* is relaxed, and it is relaxed because the consequence is a slightly ugly column instead of a wrong number.

**`GET /v1/members/{memberId}/coverage?asOfDate=2026-07-14`**

```jsonc
{
  "memberId": "MBR-000913",
  "policyNo": "POL-26-004417",
  "policyStatus": "ACTIVE",
  "asOfDate": "2026-07-14",
  "covered": true,
  "reasons": [],                       // populated only when covered = false
  "planCode": "HC-STD",
  "benefitLevelNo": 1,                 // which plan-on-policy this member points at
  "policyPeriod":   { "from": "2026-01-01", "to": "2026-12-31" },
  "coveragePeriod": { "from": "2026-03-01", "to": "2026-12-31" },
  "premiumPaid": true,
  "cardNo": "H-2026-000913",
  "source": "ISSUED_J1",               // ISSUED_J1 | IMPORTED — provenance only, see below
  "continuousCoverFrom": "2024-04-01", // v2.1, B1 — renewal / continuous-cover start,
                                       // at MEMBER-on-policy level. null = first-time member.
                                       // Adapter output name; InsureMO's own name, type and
                                       // representation are unknown — X-15
  "ruleSetRef": "HC-TC-2026-01"        // v2.1, B2 — the policy's governing rule set / wording.
                                       // Opaque. Never parsed. Version-carrying? Unknown — X-16
}
```

**`continuousCoverFrom` is the field this interface was missing, and it is also the one to be most careful about.** Answer **B1** confirms that `I-05` returns the renewal status / continuous-cover start **at member-on-policy level**, which is what A5 §5's *"an uninterrupted renewal does not serve the waiting period again"* rule needs and has never had. Until it lands, D2 §3.3.3's `V-119` makes every waiting-period test on a renewing member return **inconclusive** rather than fail — safe in both directions, and it means A5 §5 does not run. **This field is what turns that rule on.**

**Three cautions, and the first is not a formality:**

1. **The level is confirmed in prose and not in a schema, and *level* is exactly what prose gets wrong.** A renewal flag held at **policy** level is an entirely ordinary way to build a register, it answers *"does I-05 return renewal status?"* with a truthful *yes*, and **it does not answer for a member who joined the policy mid-term** — the group case, where the policy has renewed four times and this member joined last month. **Verify the level against the real schema before anything is built on it** (Framework C-IMO-06, X-15). If it turns out to be policy-level, `V-119`'s inconclusive result must stay in force for members whose coverage start is later than the policy start, and A5 §5 runs for the rest.
2. **`null` means first-time member; it never means "we could not tell".** §1.2 fixes `null` as *not known*, and this field is the one place that reading would be actively dangerous — a member we cannot classify must not be silently treated as new and served a fresh 365-day pre-existing-condition wait. **So the adapter fails closed**: if the attribute is absent from the response rather than present-and-null, that is a schema failure at the boundary (§1.6 responsibility 2), not a first-time member.
3. **A date, not a flag.** *Renewal status* as a boolean cannot answer *how long has cover been continuous*, which is what a 365-day pre-existing-condition period requires. If InsureMO returns only a flag, the adapter cannot synthesise the date and must say so — the field becomes unusable rather than approximated, and A5 §5 stays off. **An approximated continuous-cover date is a waiting period computed from a guess.**

**`ruleSetRef` closes the source half of Framework S-22.** Answer **B2** confirms `I-05` exposes the policy's rule set / wording reference, so a claim can record **which wording it was assessed under** — the failure S-22 declared was that a claim assessed under version 2 and disputed under version 1 could not be told apart after the fact, and carrying this reference onto the snapshot ends it. **It is opaque and it is never parsed** (§1.2). **What it does not yet do** is prove it identifies a *version* rather than a rule set; if it is unversioned, two wordings under one reference are still indistinguishable and S-22 has moved less than it looks — **X-16**, and it belongs on the sandbox list beside C-IMO-05, because effective dating would be the only other thing that could separate them.

**From v2.2, `ruleSetRef` has a second job and it is the larger one: it is the key the claim-side term register binds to.** The *Solution Paper* §2.6 asked where the policy's terms should be authored and answered **option B, approved as decision D-1**: **InsureMO owns *which* terms apply and tells us so through this field; we own *what they mean operationally*.** F65's register is keyed on it, F66 evaluates only the terms bound to the value pinned on the claim's snapshot, and `BR-TERM-4` makes that binding survive for the life of the claim. **A field that until v2.1 opened nothing now decides which contractual terms a claim is assessed under.**

**`C-IMO-13` — does the rule set come back as codes we can bind terms to, or as prose only?** New at v2.2, to InsureMO, and **it does not block.**

| | |
|---|---|
| **The question** | `I-05` returns `ruleSetRef`. Is what stands behind it a **set of identifiers** — clause codes, term codes, a structured rule set we can bind individual terms to — or is it **a reference to a document**, with the terms themselves in prose? |
| **If codes** | The register binds term by term, and a term can be traced to the vendor's own identifier for it |
| **If prose only** | **We bind the register to `ruleSetRef` alone and accept one register per rule-set version.** That is the design as specified today and it works; what it costs is that two rule sets sharing a term hold it twice, and a change to that term is two edits |
| **Blocks** | **No.** The fallback is the current specification, not a redesign |

**`C-IMO-14` — does a mid-term endorsement produce a new `ruleSetRef`, or a new benefit-schedule version, or does it change fields silently? This one BLOCKS.**

| | |
|---|---|
| **The question** | A member's sum insured is raised mid-year, or they move to a higher plan. **What does `I-05` look like afterwards?** A new `ruleSetRef`; a new `benefitLevelNo` or a new `scheduleEffective` range; or **the same identifiers with different numbers underneath them** |
| **Why it blocks** | **D1 §5.9.5** — the mid-term benefit-level rule — says consumption **carries** and the cap **changes**, and that the ledger stores **the cap as at the posting** so the history stays explicable. Every part of that requires the snapshot to be able to **detect that the cap moved**. **In the silent case it cannot**: two claims registered a week apart return the same `ruleSetRef` and the same `benefitLevelNo`, with different sums insured behind them and nothing on either snapshot to say which was which. **The rule cannot be implemented as written**, and consumption would be carried against a cap the system does not know changed |
| **What is not blocked** | Endorsements are **out of scope for MVP 1** (S-05), so nothing stops today. **What the answer decides is the ledger's storage shape**, and retrofitting a cap onto historical postings is not possible — which is why this is asked now rather than at MVP 2 |
| **The adapter's position until it is answered** | `scheduleEffective` is already compared against `asOfDate` at every read (§1.5 rule 3). **That check catches a re-dated schedule and does not catch a silently edited one**, and this document should not pretend otherwise |

> **Both questions are about the same thing seen from two ends: how much of the policy's meaning is expressible in what `I-05` returns.** `C-IMO-13` asks whether the *terms* are addressable; `C-IMO-14` asks whether a *change* to the schedule is detectable. **Neither is answerable from a payload description, and both belong on the sandbox list (C-IMO-06)** beside C-IMO-05 and X-16 — they are the same conversation.

**`ruleSetRef` is a `lookup()` with nothing to look up, and this is the document that has to say so.** *(New at v2.3.)* D2 §5.28.3 field **F2** — the rule-set binding on the term-authoring screen — is typed **`lookup(ruleSetRef)`** and sourced **`imo:I-05`**, mandatory, with the note *"a term with no rule set binds to nothing and is never evaluated."* **`I-05` does return `ruleSetRef`. It returns exactly one of them: the reference governing one member's coverage, on one policy, as at one date.** A `lookup()` needs a **list**, and **no interface in this specification lists the rule sets a term author may choose from** — not I-01, which returns a product's benefit schedule; not I-02 or I-03, which quote and issue; not I-04, which announces one issued policy; not I-05, which answers about one member; and not I-06. **The control is typed against a source that cannot populate it**, and that is a gap in the sixteen rather than a defect in the screen.

| | |
|---|---|
| **What F2 needs** | The set of rule-set references a term may be bound to, so that an author **picks** rather than types |
| **What the sixteen provide** | **One reference at a time**, arriving on a coverage read for one member |
| **The fallback — and it is the specification, not a workaround for one** | **F2 validates against the distinct `ruleSetRef` values already seen on snapshots.** Every registration pins the reference onto the claim (§4 G-8, `BR-TERM-4`); the distinct values across every snapshot taken to date **are** the platform's list, they are **ours**, and they need no interface and no vendor answer. **An author can therefore bind a term only to a rule set the platform has actually met** |
| **Why that is the safer control and not merely the available one** | A free-text field lets an author bind a term to a reference **no claim will ever carry** — a term that is `ACTIVE`, approved under four eyes, and silently never evaluated. The fixture is the worked example of that failure in the other direction: ten `policy_term` rows bound to one rule set and, until `A5_seed_v1.5.json`, no `policy` record carrying it, so **chain step 4 selected zero terms in every mock and reported a clean pass** |
| **What the fallback costs** | **The first policy under a new rule set cannot have its terms authored in advance.** The reference becomes selectable only after a claim under it has been registered, and until then that policy is assessed under no terms at all. **D2 §5.28.7's empty-state banner is what makes that visible**, which is why the banner is a control rather than a courtesy |
| **Blocks** | **No.** The build proceeds on the fallback, and a vendor list — if one ever arrives — replaces the validation source and changes nothing else |

**The vendor half is left open on purpose, and it is `X-20`.** Whether InsureMO can expose a **rule-set list operation** — *"return the rule sets in force for this product, or for this tenant"* — is not a question this document can answer or should assume the answer to. **It would sit on I-01**, the only one of the six whose subject is the **product** rather than a policy or a member, and it would turn F2's `lookup()` into a real one. **Pursuing it with the vendor is the client's to decide**, and it is recorded as an open exception in this document's own `X-nn` series rather than raised here as a new `C-IMO-nn`, because **§7 is where a developer looks for what is not yet answered** — and the developer's answer is already given above. **It does not block.**

**`source` keeps its place and loses its job.** In v2.0 it was the **routing discriminator** for I-14 branch B: `IMPORTED` meant the adapter answered from a claim-side member extension instead of from InsureMO. **Branch B is withdrawn** (B7 closed C-IMO-10), so **nothing routes on this field.** It stays because it is genuine provenance — it tells an operator whether a policy arrived through J1 or through a spreadsheet, which is useful when a coverage date looks wrong — and it is called out here rather than quietly left in place, because **a field whose stated purpose has evaporated is a field somebody will later implement against the stated purpose.**

**`payoutAccount` and `beneficiary` are gone from this payload, and that is a change of ownership, not a trim.** v1.1 returned both here because both lived in the Policy Data Store. They are now **Claim Platform master data — F58** (Framework §2.1, §4.3): a payout account exists to reimburse a claim, and a payee is a party to a settlement rather than to a contract of insurance. InsureMO neither holds them nor should be asked to. The claim reads them from its own store with no interface at all, which is one fewer external dependency on the path to a payment instruction, and it is why I-11's `payee` block needs no external read.

**`covered` is now derived by us, and that is the more important half of this section.** In v1.1 it was *"the only computed value in the entire Policy Data Store"*, and we owned the computation. We still own it — but now by taking the inputs from InsureMO and computing on our side. The derivation is unchanged and is exactly three tests: *is `asOfDate` inside the policy period, is it inside the member's coverage period, and is the policy status `ACTIVE`.* When `false`, `reasons` carries one or more of `POLICY_NOT_ACTIVE`, `BEFORE_POLICY_START`, `AFTER_POLICY_END`, `BEFORE_MEMBER_COVERAGE`, `AFTER_MEMBER_COVERAGE`.

**`I-05` returns facts, not a coverage verdict — and from v2.1 that is a confirmed choice rather than a precaution.** Answer **B5** settles it in both halves: **InsureMO can return whatever we need, including its own coverage verdict — and we deliberately do not consume it.** v2.0 could only write *"if InsureMO offers its own verdict, we do not consume it"*, a rule against a possibility. It is now a decision taken against a confirmed capability, which is a stronger statement and a more expensive one to reverse casually.

That is principle **P-01** applied at the one place it is easiest to give away: external platforms are sources, never deciders. **A vendor's `isCovered` is a decision**, and it may fold in grace periods, premium status or lapse rules that are not ours, that we did not write, and that we could not reproduce in an audit eighteen months later when a customer disputes a decline. We take dates and status — facts — and derive the verdict from them, so that the three tests stay inspectable and every decline reason traces to a rule we wrote. The claim engine maps the reason codes to decline reasons; it does not re-derive them.

**The consequence for the adapter is a rule, not a preference.** If a verdict field appears in InsureMO's response, the adapter **drops it at the boundary and does not carry it onto the claim.** It is not stored "for reference", not logged onto the claim record, and not shown on any screen — because a vendor verdict sitting beside ours on an assessor's screen is a second answer to a question with one right answer, and the first time the two disagree somebody will act on the wrong one. The raw response is retained in the adapter trace (§1.6 responsibility 3), which is where a discrepancy can be investigated without being actionable.

#### I-05's one exception — the controlled renewal re-read

**Framework §3.1, decided 24 August 2026 (answer A1).** P-02 says policy data is snapshotted and not read live. It now has exactly one exception, and this is the interface it runs on.

```
GET /v1/members/{memberId}/coverage?asOfDate={the claim's service date}&fields=continuousCoverFrom
```

| Rule | Detail |
|---|---|
| **When** | Only while the claim is **before `PENDING_APPROVAL`**. After that the adapter refuses the call — this is enforced at the adapter and not only on the screen, because a control that exists only in a UI is a control that survives until the first API client |
| **What it replaces** | **`continuousCoverFrom` on the snapshot, and nothing else.** It is not a re-snapshot. The benefit schedule, the policy period, the coverage period, `planCode`, `premiumPaid` and `ruleSetRef` are **not** re-read and must not be written even if the response carries them |
| **Consistency** | G-1a's checks still apply to whatever comes back: `asOfDate` echoed, `planCode` and `benefitLevelNo` unchanged from the snapshot. **A re-read that disagrees with the snapshot on anything other than the renewal attribute is not applied** — it goes to the operator queue (G-3). A policy that changed underneath a registered claim is not something to absorb silently into one field |
| **Trace** | Logged with the correlation id, the user, the timestamp, the value before and the value after (Framework P-08, F53). **The previous value is retained**, because *"the figure changed after somebody pressed a button"* is a question an auditor asks |
| **Effect** | **The calculation is marked stale.** The prior payable is not carried forward; the chain re-runs against the snapshot as amended, and the assessor is told the basis moved |
| **Failure** | A failed or timed-out re-read leaves the snapshot **exactly as it was** and raises an operational message. It never blanks the field, and it never falls back to *first-time member* |

> **Why this is in the contract and not only in D2.** It is a **second call on an interface whose entire design assumed one** — G-1 was written around a single read at registration, G-8 around a snapshot that is never written again, and §4 restates both for it. A screen specification adding a button cannot amend a cross-interface guarantee, and if this had been recorded only in D2 the two documents would have contradicted each other with nobody obliged to notice.

**`premiumPaid` is returned but is deliberately *not* part of `covered`.** Whether an unpaid premium blocks a claim is an adjudication rule, and adjudication belongs to the Claim Platform — A5 §6.1 step 1 tests it there. **The reason for the separation has changed and got stronger.** v1.1's reason was that the store existed to hold no business rules. The store is gone; the reason now is that we must not inherit a vendor's business rules by accident. Reading a fact and reading a view of a fact are different operations, and only the first one is reproducible six months later.

**`GET /v1/plans/HC-STD/benefits?asOfDate=2026-07-14` — returned as a tree, not a flat list**

```jsonc
{
  "planCode": "HC-STD", "asOfDate": "2026-07-14", "currency": "VND",
  "scheduleEffective": { "from": "2026-01-01", "to": null },   // must cover asOfDate — see errors
  "benefits": [
    {
      "code": "200", "name": "Outpatient treatment",
      "parentCode": null, "depth": 1,
      "annualSumInsured": 20000000,
      "perEventLimit": null, "perDayLimit": null,
      "maxCount": 0, "countUnit": null,        // 0 = unlimited, per Framework §4.1.1.
                                               // The unit is null because the seed row's
                                               // count_unit is null — the count mapping
                                               // below; a unit is never invented
      "coPaymentBp": 2000,                     // 20.00%; A5 seed column copay_pct × 100
                                               // the string "INHERIT" passes through unconverted
      "deductionRatioBp": 2000,                // policy term, not a discretion — A5 §4.3
      "deductionCondition": "PROVIDER_OFF_NETWORK",   // null when the benefit carries no deduction
      "waitingPeriodDays": {                   // codes and values per A5 §5
        "PRE": 365, "SPE": 365, "GEN": 30, "ACC": 0,
        "BIR": null, "MAT": null               // null = the combination cannot arise
      },
      "effectiveFrom": "2026-01-01", "effectiveTo": null,
      "children": [
        { "code": "200.1", "name": "Consultation and diagnostic tests",
          "parentCode": "200", "depth": 2,
          "perEventLimit": 1500000, "maxCount": 0, "countUnit": "VISIT",
          "coPaymentBp": "INHERIT",            // takes the nearest ancestor's rate — see below
          // annualSumInsured, deductionRatioBp, deductionCondition and waitingPeriodDays
          // are ABSENT on this node, and absent does NOT mean none — property 5
          "children": [] },
        { "code": "200.2", "name": "Prescribed medication",
          "parentCode": "200", "depth": 2,
          "perEventLimit": 1000000, "maxCount": 0, "countUnit": "VISIT",
          "coPaymentBp": "INHERIT",            // same four absences — property 5
          "children": [] }
      ]
    }
  ]
}
```

**Two naming decisions, both to avoid a collision that exists in the source documents.** `depth` is the node's position in the benefit tree — the framework calls it *level*, but *level* is also the word for a plan-on-a-policy (`benefitLevelNo` above), and one payload carrying both senses of "level" is a bug waiting to happen. And `waitingPeriodDays` carries **A5 §5's six condition-class codes verbatim** — `PRE`, `SPE`, `GEN`, `BIR`, `MAT`, `ACC`. A `null` means A5's *"n/a — the combination cannot arise"*; per A5 it is a **data error if such a claim appears, not a zero-day waiting period**, and the adapter treats it as such.

**Five properties the claim engine relies on, and they are contractual — on the adapter:**

1. **`children` is always present**, as `[]` at a leaf. A consumer never has to test for the property's existence before walking.
2. **`maxCount: 0` means unlimited.** Not zero, not null. A5 §4 and Framework §4.1.1 use the same sentinel and it is honoured explicitly rather than inferred.
3. **The tree is complete in one response.** No lazy loading, no follow-up calls. The claim engine snapshots the whole thing onto the claim in one operation (P-02), and a partially-loaded snapshot is a corrupt snapshot.
4. **`coPaymentBp` is mandatory on every node, and carries the literal string `"INHERIT"` where the rate comes from an ancestor.** This is A5's `copay_pct: "inherit"` sentinel on the wire, and it is a string rather than a number precisely so that arithmetic on it fails loudly instead of quietly producing zero. Neither of the two obvious alternatives works: **`null` is already spoken for** — §1.2 fixes it as *not known*, and a co-payment we do not know is not a co-payment we inherit — and **omitting the property means "no co-payment"**, which under §1.1's ignore-unknown-fields rule would silently pay a 20%-co-pay benefit in full. The adapter resolves `"INHERIT"` to the nearest ancestor carrying an integer rate, at snapshot time, and writes the resolved integer onto the claim; an `"INHERIT"` that reaches the root without finding one is a schedule error and fails at the boundary (§1.6, responsibility 2). A benefit that genuinely carries no co-payment sends integer **`0`** — a rate of nothing, not an absence of a rate.
5. **The other inheritable properties resolve to the *nearest ancestor carrying them*, at snapshot time — and an absence on a child is not a *none*.** *(New at v2.3.)* **Property 4 is right about absence and it stops one field short.** It makes `coPaymentBp` mandatory on every node precisely because *"omitting the property means 'no co-payment'"*, and the payload above then omits **four other properties** from `200.1` and `200.2`: `annualSumInsured`, `deductionRatioBp`, `deductionCondition` and `waitingPeriodDays`. Read the way property 4 reads absence, an absent `waitingPeriodDays` on `200.1` means **no waiting period** — and a 365-day `PRE` wait, held once on the group node, silently becomes **zero on every child under it**. **This is not a hypothetical reading of an example.** The seed holds waiting periods on the group node only — `benefit_waiting_period` carries rows for `100`, `200`, `300`, `400` and `500` and for nothing below them — and it holds the deduction pair on `200` and `300` only. **A mock built from the fixture and read by property 4's logic waives every waiting period in the product**, and reports nothing.

| Property | How a node with no value of its own resolves | Absent on a **root** node |
|---|---|---|
| `waitingPeriodDays` | The **nearest ancestor carrying the map, taken whole**. A node never merges its own map with an ancestor's — partial inheritance across six condition classes is how five of them end up at zero while one looks correct | **Schedule error.** Fails at the boundary (§1.6 responsibility 2) |
| `deductionRatioBp` **and** `deductionCondition` | The **nearest ancestor carrying the pair**, and **as a pair**. Never one without the other: a ratio inherited without its condition deducts unconditionally, and a condition inherited without its ratio deducts nothing | **Schedule error.** A group with no deduction says so explicitly — `deductionRatioBp: 0`, `deductionCondition: null` — exactly as a benefit with no co-payment sends integer `0` |
| `annualSumInsured` | The **nearest ancestor carrying it**. This is what child-to-parent aggregation already means (A5 §4.2): a child with no annual cap of its own consumes the ancestor's, and does not acquire an unlimited one | **Schedule error.** A branch with no sum insured anywhere on it prices nothing |

**Three things follow from property 5, and the third is the one that reaches the ledger.** First, **the resolution happens at snapshot time and the resolved value is written**, exactly as `"INHERIT"` is — G-8 requires the snapshot to be the whole of what the claim was adjudicated against, and a snapshot that stores *"inherits from somewhere"* is a snapshot that has to re-read a schedule to be understood. Second, **the root is where absence stops being ambiguous**: every root node arrives with all four properties explicit, which is the same discipline property 4 imposes one level lower, and an adapter that cannot make that true rejects the schedule rather than defaulting it. Third, **the seed's nulls at a root are mapped, not passed through**: `deduction_pct: null` on `100`, `400` and `500` becomes `deductionRatioBp: 0` with `deductionCondition: null` — *no deduction*, stated — and it is the same mapping as `copay_pct: null → 0` in the list below.

**The count pair maps from the seed without reinterpretation, and `maxCount` and `countUnit` are read together.** `maxCount` comes from `count_limit` and `countUnit` from `count_unit`, each unchanged. **`count_unit: null` is returned as `countUnit: null` and is never mapped to `0` and never to a unit.** Node `200` above is the case: `count_limit: 0` with `count_unit: null`, which is *unlimited, in no unit* — there is no counter on the benefit, so there is nothing for a unit to describe. Its children carry `VISIT` because they do have counters. **The two values mean different things and §1.2 forbids substituting one for the other**: `0` is the unlimited sentinel — a known quantity — and `null` is *not known*. A `countUnit` supplied where the source has none is an invented fact, and a `maxCount` synthesised from a missing unit is a limit nobody wrote. The one combination the adapter rejects is the reverse: **a non-zero `maxCount` with a `null` `countUnit` is a schedule error** — a limit of sixty of something unnamed cannot be counted against.

> **All five are now the adapter's to guarantee, and the fourth needs saying twice.** Framework C-IMO-01 confirmed that InsureMO can carry **inherited co-payment**; it did not tell us how it *represents* inheritance on the wire. Three behaviours are all plausible and all different: a marker like ours, a materialised rate repeated on every descendant, or an absent property meaning *inherit*. The third is the dangerous one, because §1.1's ignore-unknown-fields rule and an absent property together pay a co-paid benefit in full. **So the adapter does not infer inheritance from absence.** Either InsureMO's representation is known and mapped, or every node must arrive with an explicit rate, or the schedule fails validation at the boundary. The mapping from the A5 seed is unchanged for the mock and belongs in the seeding script with the basis-point conversion (A5 §10) — exactly three cases: `20` → `2000`, `"inherit"` → `"INHERIT"`, `null` → `0`.
>
> **Property 5 adds nothing to that list of cases and one obligation beside it.** The seeding script must emit `waitingPeriodDays`, the deduction pair and `annualSumInsured` **on every root node**, mapping a seed `null` at a root to the explicit *none* the table above states — and must leave them **absent on every child**, so the mock exercises the resolution rather than materialising round it. **A mock that repeats the resolved values on every descendant is the one mock that cannot fail**, and it would prove the adapter's inheritance logic works by never invoking it. **The vendor half is the same question property 4 already asks and it has the same three plausible answers** — a marker, a materialised value on every node, or absence-means-inherit — and it is unanswered for these four properties for the same reason: C-IMO-01 confirmed InsureMO can *carry* waiting periods, a deduction ratio with its condition and an annual sum insured, and told us nothing about how it *represents* a child that does not restate them.

**Errors.** `404` unknown member, policy or plan — **which per §1.5 rule 2 means *cover could not be established*, routes to an operator, and never becomes a decline** · `422 NO_SCHEDULE_AT_DATE` when the plan exists but has no schedule effective on `asOfDate`. That second one is now checked by **us**: the adapter compares `scheduleEffective` against `asOfDate` and raises it, rather than waiting for InsureMO to raise something equivalent. A claim arriving for a service date before the schedule was configured is a real condition, and it must not be silently answered with today's schedule. **The behaviour it defends against is precisely what C-IMO-05 has not yet told us** — effective dating is the one open contract-detail item where a surprise costs design rather than a mock.

**Mock.** The **InsureMO mock**, seeded from `A5_seed_v1.5.json`: the A5 pilot product, three plans, the full benefit tree, and **twelve members across one individual and one group policy** — deliberately including one member whose coverage ends mid-year, one policy with `premiumPaid: false`, and one payout account with `accountNameVerified: false`, so the unhappy paths are exercised from day one rather than found in UAT. The payout account now seeds the **Claim Platform's** F58 master rather than this mock's response — §5.

**`accountNameVerified` and the seed's `payout_account.verified` are one field under two names, and neither document said so until v2.3.** The seed column is `verified`; this document calls it `accountNameVerified`, here and on **I-11**'s `payee` block, and the longer name is deliberate — *verified* alone does not say **what** was verified, and what was verified is that the **account name matches the payee**, not that the account exists. **The name in this document is the wire name and the name in the seed is the column name; the loader maps one to the other and neither is wrong.** §5.1 carries the mapping, and it is recorded there rather than corrected in the fixture because the seed is reissued under A5 and a field rename would break every loader written against it for the sake of a synonym.

**Three mock behaviours v2.1 adds, and the third is the point of the exercise.** One member with a `continuousCoverFrom` two years before the current policy period — a genuine renewal, so A5 §5's waiting-period waiver has something to fire on. One with `continuousCoverFrom: null` — a first-time member, so the full waiting periods are exercised. And **a settable renewal value, so the controlled re-read changes something**: the re-read path, the stale-calculation marking and the re-run are otherwise built against a call that always returns what is already on the snapshot, which tests the transport and none of the behaviour.

> **All three exist in the fixture from `A5_seed_v1.5.json`, and until it they did not.** `member` had no `continuous_cover_from` at all, so the three shapes above could be specified and not built. **Two further gaps closed with them and both were of the same kind — a behaviour this document contracts, with no fixture behind it.** No `policy` record carried a rule-set reference while all ten `policy_term` rows bound to one, so **chain step 4 selected zero terms in every mock and reported a clean pass**; `policy.rule_set_ref` is what `ruleSetRef` above is served from. And there was **no provider list**, so `deductionCondition: "PROVIDER_OFF_NETWORK"` — returned on this very payload — could not be evaluated against anything; the fixture's `provider` collection now carries it, and it is **Claim Platform master data, not this mock's** (§5.1).

---

### I-01 — Claim Platform → InsureMO · product catalogue and benefit schedule retrieval

**Status: Provisional.** *(v1.1: InsureMO → Policy Data Store.)* **The direction reverses**, and that is the whole change: a push into a store we wrote has become a pull from a system we read.

**`GET /v1/products/{code}/plans/{plan}/benefits?asOfDate=`** → **the same tree shape as I-05.**

**Why the same shape, and why it is still two interfaces.** In v1.1 the shape was shared so that the store could hold either its own schedule or a synced one without the claim engine noticing which. That reason is gone with the store. The shape is shared now for a better reason: **I-01 and I-05 are the same read taken at two different moments, for two different purposes.**

- **I-01 fires at product publish.** Its output feeds the **adjudicable product model and the versioned mapping from the sold product** — principle **P-03**. This is design-time work: a human reviews the mapping, and it is a tested, versioned artefact.
- **I-05 fires at claim registration.** Its output is **snapshotted onto the claim** — principle **P-02**. This is run-time, per-claim, and nobody reviews it.

They stay separate identifiers because their triggers, their caching, their latency budgets and their failure behaviour all differ. I-01 failing delays a product launch; I-05 failing stops a claim. One shape, two contracts, two blast radii.

**The reversal loses the trigger, and the trigger is decided here rather than assumed.** A push arrives when the publisher decides; a pull has to be told when to go. Framework §6 says *"pull on product publish"*, which names the moment without naming the mechanism. For MVP 1:

| Trigger | Rule |
|---|---|
| **Manual** | An administrator triggers the retrieval from the console when a product version is published (part of F55). This is the primary path in MVP 1, because product publishes are rare, deliberate, and already a human event |
| **Scheduled reconciliation** | A daily poll of the catalogue compares the product version and effective dates against the last retrieval and raises an operational alert on a difference. It does **not** auto-apply — an adjudicable mapping that changed itself overnight is a settlement that changed itself overnight |
| **Event-driven** | Deferred. If C-IMO-04 reveals an event mechanism that covers product publish as well as policy issuance, it becomes the trigger and the daily poll becomes the safety net |

**What the retrieval must survive.** The tree two to three levels deep with child-to-parent aggregation, per-day and per-event sub-limits, limits in count with the `0 = unlimited` sentinel, waiting periods differing across the six condition classes, inherited co-payment, and the deduction ratio with its triggering condition. **Every one of those is what C-IMO-01, C-IMO-01b and C-IMO-01c confirmed, demonstrated against A5 v1.2** — which is why this interface is Provisional on *shape* and not on *capability*. If a benefit in the full Product Data Sheet turns out not to be expressible after all, that is not an interface problem and there is no fallback for it: it is **RK-14**, and **C-IMO-06** — a sandbox, evaluated by us, benefit by benefit — is the only mitigation.

> **What this paragraph used to say.** v1.1 ended this contract with: *"if InsureMO cannot produce a tree with parent/child aggregation, count limits with an unlimited sentinel and waiting periods by condition class, the fallback is that the schedule is mastered in the Policy Data Store and I-01 is never built. The claim side does not change either way."* **That is withdrawn in full.** It named a system that is not being built. Recording the withdrawal rather than deleting the sentence is deliberate: the reasoning was correct when it was written, and the thing that changed was the world, not the argument.

**Mock.** Served by the **InsureMO mock** from the same fixture as I-05, at the same endpoint family, with a settable product version so that the scheduled-reconciliation path has something to detect.

---

### I-04 — InsureMO → Claim Platform · policy issued

**Status: Provisional.** *(v1.1: InsureMO → Policy Data Store.)* Webhook preferred, **polling fallback mandatory**. The integration style is still open — **C-IMO-04**.

**Payload:** policy, members and the benefit schedule as sold, delivered to `POST /v1/webhooks/insuremo`, HMAC-signed per §1.4 — *with the caveat in §1.4 that InsureMO's signing is a requirement we are stating and not a behaviour we have been promised.* Handler idempotent on `policyNo` (G-4).

**Why this exists as a separate interface from I-03.** I-03 assumes synchronous issuance. If InsureMO issues asynchronously, I-03 returns an application id only and **I-04 absorbs the change** — the buy journey gains a "we are issuing your policy" state and nothing else moves. That was true in v1.1 and it is still true.

**What a missed I-04 costs — and this is different from what v1.1 said.** v1.1's reasoning was: *"a policy that exists in InsureMO but not in the Policy Data Store is a policy whose claims will be declined for no cover, and that failure is invisible until a customer complains."* **That consequence no longer follows**, and it is worth being precise about why rather than quietly reusing the old sentence. The Claim Platform deliberately keeps **no copy of the policy or the schedule that adjudication depends on** (Framework §5.1): it reads them from InsureMO at claim registration over I-05 and snapshots them there. So the policy data is intact whether or not the webhook arrived, and a claim on a missed policy adjudicates correctly.

What a missed I-04 actually costs is **visibility**: the Claim Platform's registration of the policy as claimable, which is what F21's member linking, F25's "my cover" and F24's certificate delivery all read. The customer has bought a policy they cannot see and cannot start a claim against from the app. That is a worse customer failure and a better system failure than the old one, and it changes the priority of the fallback rather than removing it.

**The polling fallback, and the new problem with it.** `GET /v1/policies?issuedSince=`, reconciled every 15 minutes. **It is mandatory, not optional** — for the reason above, and for a second one that v1.1 did not have to consider: §1.3 records that at-least-once delivery is a property of the *sender*, and InsureMO's is unknown. If InsureMO delivers at-most-once, the polling fallback is not a fallback at all; **it is the only delivery guarantee in the interface.**

> **And the fallback now depends on an endpoint we have not confirmed exists.** In v1.1 the reconciliation query ran into our own store, so it could always be built. It now runs into InsureMO, and `GET /v1/policies?issuedSince=` is our specification of something we have never seen. **This is the sharpest thing the direction change did to this contract**: the mandatory compensating control for an unconfirmed webhook is itself unconfirmed. It is carried as **X-10**, and it belongs on the list for the sandbox evaluation (C-IMO-06) rather than being discovered at integration.

**The safe default until C-IMO-04 answers.** Treat the webhook as a **notification, not a payload**: on receipt, verify what can be verified, then perform an authenticated I-01/I-05 read against InsureMO to obtain the policy rather than trusting the body. This costs one extra call per issued policy, removes the dependence on a signing scheme we have not agreed, and makes an unsigned or replayed webhook harmless — it can only cause us to re-read data we are entitled to read.

**Mock.** The **InsureMO mock** fires this webhook a configurable delay after an I-03 issuance, from the fixture, in both styles — signed body, and bare notification — so the handler is built against the safe default and the optimistic case from day one.

---

### I-14 — Excel → Claim Platform · group policy and member bulk load

**Status: Provisional.** *(v1.1: Excel → Policy Data Store, Confirmed. v2.0: demoted, cause = C-IMO-10 unanswered. v2.1: **that cause is discharged and replaced** — §0.1a.)* This is how group business gets into the MVP without building group underwriting. **The loader is ours** (Framework §2.1, F59), and from 24 August 2026 **where the member list lands is settled: InsureMO.**

> **C-IMO-10 closed on 24/08/2026, and X-12 closed with it.** InsureMO exposes a bulk member load, **for creation and for end-dating** — which is what X-12 asked, deliberately in the same breath, because a platform can perfectly well offer an import and no bulk amendment and we would have found out on the *second* import rather than the first. **There is one member list.** InsureMO masters the register for group and individual business alike (F05, R-IMO-10, R-IMO-12), and I-05 answers group coverage queries exactly as it answers individual ones.
>
> **Branch B is withdrawn, not deferred.** With it go: the claim-side member extension; two member lists that every group claim would have had to choose between; the *disjoint by `policy.source`* invariant that nothing but our own code would ever have enforced; a second implementation of the coverage derivation behind one adapter; a group policy that could never be converted to an issued one without a migration; and an as-at-a-date fidelity for imported policies that would have been ours to provide rather than InsureMO's. **That is the largest single reduction in scope any of these eight answers produced, and it was produced by a vendor saying yes to one question.**
>
> **The interface stays Provisional all the same, and §0.1a explains why**: phase two now writes into a vendor whose schema we have not seen, which is the same reason I-02, I-03 and I-06 are Provisional. **Had the answer been *no*, phase two would have been entirely ours and I-14 would have gone back to Confirmed.** The better architectural outcome carries the weaker grade, because a grade measures standing and not quality.

**Phase one is ours and is unchanged.** `POST /v1/imports/policies` · `multipart/form-data`, one `.xlsx`, maximum 5 MB / 2,000 member rows.

```
POST /v1/imports/policies            → 202 { importId, state: "VALIDATING" }
GET  /v1/imports/{importId}          → validation report
POST /v1/imports/{importId}/commit   → applies it
```

**Nothing is written until commit.** The validation report is reviewed by a human first:

```jsonc
{ "importId": "IMP-000042", "state": "VALIDATED",
  "policyCount": 1, "memberCount": 148,
  "errors":   [ { "row": 37, "column": "dateOfBirth",
                  "code": "BAD_DATE", "value": "31/02/1990",
                  "message": "Not a valid date" } ],
  "warnings": [ { "row": 51, "column": "cardNo",
                  "code": "DUPLICATE_IN_FILE", "value": "H-2026-000913" } ] }
```

| Rule | Detail |
|---|---|
| Errors block | Any error → commit returns `422`. Fix the file and re-upload. **No partial import** — a half-loaded group policy is worse than none, because claims will be assessed against it |
| Warnings do not block | Recorded on the import and visible afterwards |
| Plan codes | Must already exist in **InsureMO's** product catalogue, retrieved over I-01. An unknown `planCode` is an **error**, not an auto-create. Consistency Review v1.0, L-7: otherwise an importer invents plan codes and the benefit schedule silently does not exist. **The check itself is now an external read**, so an import cannot be validated while InsureMO is unavailable — validation returns `503` and the import stays `VALIDATING`, which is correct and must not be relaxed into a warning |
| Benefit levels | One level per import run. A policy with three levels is three runs, and the constraint is enforced rather than documented |
| Re-import | Same policy number replaces the member list wholesale, and the diff is reported before commit. Members not in the new file have their coverage **end-dated**, never deleted — a departed member's past claims must stay assessable |

**Phase two is conditional, and both branches are specified here so that C-IMO-10 is a choice rather than a discovery.**

**Branch A — InsureMO exposes a bulk member load.** The commit writes the policy, its benefit levels and its member list into InsureMO. There is **one** member list, InsureMO masters the register for group and individual business alike (F05, R-IMO-10), and I-05 answers group coverage queries the same way it answers individual ones. This is the outcome to want.

It costs one guarantee, and the guarantee is the `No partial import` rule above. **A commit that writes 148 members into a remote system over an API is not atomic**, and the rule that a half-loaded group policy is worse than none does not stop being true because the write became remote. So under branch A the commit is staged:

| Step | Rule |
|---|---|
| **Commit locally first** | The import is written to the Claim Platform's own import record atomically, with `state: "COMMITTED"`. This is the record of what was intended, and it survives every failure below |
| **Then sync** | `state: "SYNCING"` while members are written to InsureMO. The write must be **idempotent on `policyNo` + `cardNo`**, so a re-drive after a partial failure completes rather than duplicates. If InsureMO does not offer an idempotent upsert, the sync reads back and writes the difference — slower, and the only safe alternative |
| **Not claimable until synced** | The group policy is **not marked claimable** until every member row is acknowledged. `state: "SYNCED"` is the gate. A group whose members are half-loaded must decline no claims and accept none; it must be invisible |
| **Failure** | `state: "SYNC_FAILED"` with the rows that did not land, an operator queue entry, and a re-drive action. Never a silent partial |

> **Bulk create is not bulk update, and C-IMO-10 as written asks only the first question.** The `Re-import` rule above end-dates members who have left. Under branch A that requires InsureMO to accept a **member update and an end-date**, not merely a member creation. A platform can perfectly well offer a member-list import and no bulk amendment, and we would find out on the second import rather than the first. **X-12** carries it, and it should be asked in the same breath as C-IMO-10.

**Branch B — InsureMO does not expose a bulk member load.** F59 maintains a **claim-side member extension**, and group business has **two member lists**: InsureMO's, for policies it issued, and ours, for policies we loaded. Framework §9.1 is blunt about the cost — *"every group claim then has to decide which to believe, with no rule saying which is right."*

**So here is the rule, because the reconciliation obligation is survivable only if it is decidable.** Under branch B the two lists are **disjoint by policy, and `policy.source` is the discriminator**:

| `source` | Register | Read path |
|---|---|---|
| `ISSUED_J1` | **InsureMO** | I-05 reads InsureMO, unchanged |
| `IMPORTED` | **Claim Platform** member extension | I-05's adapter answers from the extension, using the same four operations and the same output contract |

A policy appears in exactly one register, never both, and the adapter routes on `source` — which is why `source` is on I-05's coverage response above. The benefit schedule still comes from InsureMO in **both** cases, because an imported policy's benefit levels point at plan codes in InsureMO's catalogue (the `Plan codes` rule) — so branch B duplicates the member list and **not** the schedule. Adjudication therefore reads the same tree from the same master for every claim, which is the property worth protecting.

**What branch B costs, stated so it is not rediscovered:** a second implementation of the coverage derivation behind one adapter; a group policy that can never be converted to an issued one without a migration; an as-at-a-date query whose fidelity for imported policies is ours to provide rather than InsureMO's; and a permanent invariant — *disjoint by source* — that nothing enforces except code we write. It is a decision worth taking deliberately. It is a poor thing to discover during build.

**Mock.** Phase one is a real service in the Claim Platform, not a mock, and it is seeded from the fixture's group policy. Phase two is exercised against the **InsureMO mock** under branch A, including a scripted partial-sync failure so that `SYNC_FAILED` and the re-drive are built rather than assumed. §5 records what the fixture must carry for branch B.

---

### I-02, I-03, I-06 — the remaining InsureMO interfaces · and I-13, the premium mock

**Status: Provisional — all four.** Build against mocks only. The three InsureMO shapes below are our reasoned specification of what we need, derived from A5 §9's eight-test script, and they are deliberately what the InsureMO Engagement Pack asks the vendor to demonstrate — so the session either confirms these shapes or tells us precisely which one to change. I-13 is grouped here for a different reason: it has no counterparty at all yet.

**I-02 · quotation.** `POST /v1/quotes` with `{ productCode, planCode, insureds: [{ dateOfBirth, relationship }], optionCodes[], effectiveDate }` → `{ quoteId, premium, breakdown[], validUntil }`. Premium is integer VND. `validUntil` matters: a quote the customer returns to three days later must either still hold or be visibly expired, not silently repriced at issuance.

**I-03 · application and issuance.** `POST /v1/applications` carrying the `quoteId`, applicant, insureds and the declaration answers → `{ applicationId, state, policyNo?, effectiveFrom, effectiveTo }`. Synchronous issuance is assumed; if InsureMO issues asynchronously this becomes a submit-plus-webhook pair and **I-04 absorbs the change**.

> **The idempotency exposure on I-03 is the largest one this document carries against a vendor, and §1.3 names the compensating control.** If InsureMO does not honour `Idempotency-Key`, a retried submission issues a **second policy** — a real contract, a real premium, a duplicate the customer did not buy. Until **X-13** is answered, the adapter performs a business-key check before every retry of I-03: never re-submit an application whose `quoteId` already resolves to a `policyNo`. That is a weaker control than an idempotency key and it is what we have.

**I-06 · claim reference posted against the policy record.** Optional, and marked so. It exists only to keep InsureMO's view of the policy complete, and it corresponds to **F06 at Priority 2**. **Nothing in the claim journey waits on it** — fire-and-forget with a retry, and its failure raises an operational alert without blocking settlement. If InsureMO has no such endpoint, I-06 is dropped at no cost. Framework D-B closes as *none*: InsureMO offers claim capability and the client has decided not to use it, so I-06 is a notification against a policy and not a use of a claims module. Nothing in the ledger, the assessment or the settlement depends on it.

**I-13 · premium collection.** Mock only in MVP 1 (boundary condition BC-3 in Framework §9.4 — no real money). `POST /v1/payments/mock` → immediate confirmation. Specified as a real contract so the eventual gateway is a swap.

> **One consequence of the store's deletion lands here and the brief for this reissue did not name it.** v1.1 ended this contract with *"then the policy's `premiumPaid` flag is set"* — a write into the Policy Data Store. **`premiumPaid` is now InsureMO's field**, part of the policy register (F05, R-IMO-10), and I-13 must not write it. The J1 sequence already resolves this without a new interface: the customer confirms and pays (mock), the mock confirms, and only then does the Claim Platform submit the application over I-03 — so the policy is issued as paid and InsureMO sets the flag itself. **No write-back interface is created, and none should be.** For imported group policies the flag comes from the import file (the fixture's group policy carries `premium_paid: false` deliberately, so that A5 §6.1 step 1 has something to test).

---

### I-07 — Claim Platform → IDP · submit document package

**Status: Confirmed.** *(v1.1: Preliminary.)* The submit → webhook → pull contract, including the idempotency key and at-least-once webhook delivery, was confirmed in writing on 21 August 2026 — **C-IDP-04**. Framework §9.2: *"I-07, I-08 and I-09 are final in shape."*

**`POST /v1/extraction-requests`** · `Idempotency-Key` required.

Documents are submitted **by reference**, not by value. They already live in the claim system's object storage; sending bytes twice doubles the storage and creates a second copy to secure.

```jsonc
{
  "claimNo": "CLM-26-000417",
  "eventNo": "CLM-26-000417/1",
  "templateSetCode": "HEALTH_CLAIM_V1",         // A4 §3 classification signatures
  "callbackUrl": "https://claims.internal/v1/webhooks/idp",
  "documents": [
    { "documentId": "DOC-8891", "uri": "s3://claims/2026/08/DOC-8891.pdf",
      "sha256": "…", "sizeBytes": 2411903, "mimeType": "application/pdf",
      "declaredType": null,                      // customer's guess; IDP may disagree
      "pageCount": 3 }
  ],
  "context": {                                   // narrows extraction; never overrides
    "episodeType": "OUTPATIENT",
    "serviceDateFrom": "2026-07-14", "serviceDateTo": "2026-07-14",
    "providerNameHint": null,
    "expectedTypes": ["T1-01", "T1-02", "T1-03"]
  }
}
```

**Response `202 ACCEPTED`:** `{ "requestId": "…", "state": "QUEUED", "acceptedAt": "…", "documentCount": 3 }`

| Rule | Detail |
|---|---|
| `sha256` | Mandatory. IDP verifies it after fetching. A mismatch fails the request rather than extracting from an unexpected file |
| `declaredType` | The customer's category choice at capture. A **hint only** — if IDP classifies differently, IDP wins and a warning is raised. Never let the customer's tap silently determine the document type |
| `context` | Narrows the search space. It must never cause IDP to report a value that is not on the page — A4 §2 *"never infer"* holds |
| Package size | Maximum 20 documents, 50 pages, 100 MB total. Exceeding returns `422 PACKAGE_TOO_LARGE` with the actual figures |

**The e-invoice path.** A document with `mimeType: "application/xml"` and `declaredType: "T1-04"` is routed to the XML parser rather than OCR (A4 §1.5). It is not a separate interface — same submission, different internal path. **C-IDP-07 confirmed it on 21/08/2026**, and A4 §1.5's position holds: it is the **only** path for T1-04, there is no OCR route for invoice images, and a claim without XML still settles on the cost statement, losing only reconciliation rule R6.

**Errors.** `409` idempotency conflict · `422` unfetchable URI, hash mismatch, unknown `templateSetCode`, package too large · `503` IDP unavailable → the claim sits in `DOC_PROCESSING` and the submission is retried on the standard backoff.

**Mock.** Returns a `requestId` immediately, then fires the I-08 webhook after a configurable delay (default 8s). Four scriptable outcomes selected by a magic value in `providerNameHint`: clean result, low confidence, missing document, hard error. **This is what lets the claim UI be built and demoed before a single template exists** — and R-IDP-06 records that no template exists yet, so the mock is not a convenience.

**What is still open, and it is field-level.** IDP's actual `templateSetCode` values and the `context` fields it honours — **X-02**. The structure holds; the vocabulary does not yet. That is the difference between Confirmed and finished.

---

### I-08 — IDP → Claim Platform · processing complete

**Status: Confirmed.** *(v1.1: Preliminary.)* C-IDP-04.

**`POST /v1/webhooks/idp`** — HMAC-signed per §1.4.

```jsonc
{
  "kind": "EXTRACTION",          // EXTRACTION | CLINICAL_ASSESSMENT — see I-15
  "requestId": "…", "claimNo": "CLM-26-000417",
  "outcome": "COMPLETE",         // COMPLETE | PARTIAL | ERROR — see note below
  "completedAt": "2026-08-21T14:32:05+07:00",
  "resultUrl": "/v1/extraction-requests/…/result",
  "summary": { "documentCount": 3, "classifiedCount": 3,
               "confidence": 0.94, "warningCount": 1 }
}
```

**The property is `outcome`, not `state`, and that is deliberate.** A4 §6 and §8 already use `state` for the *package completeness* enum (`COMPLETE | MISSING_DOCUMENT | MISSING_FIELD | INCONSISTENT | AWAITING_ORIGINAL | ERROR`). Two different enums under one property name, one describing processing and one describing the document set, is exactly the kind of collision that produces a wrong branch six months later. The webhook's processing outcome is `outcome`; the package's completeness stays `state` and is read from the I-09 result.

**The webhook carries a notification, not the result.** The payload is deliberately small: the claim system pulls the full result over I-09. This keeps the signed body small enough to verify cheaply, avoids a webhook body that varies in size by two orders of magnitude, and means a lost webhook is recoverable by polling rather than by asking IDP to resend data.

| Rule | Detail |
|---|---|
| Delivery | **At-least-once**, confirmed (C-IDP-04). The handler is idempotent on `requestId` — a second delivery for a `requestId` whose result has already been pulled is acknowledged with `200` and otherwise ignored |
| Response | `200` on accept, always within 5s. Processing happens asynchronously afterwards. A handler that does the work inline will time out and cause redelivery storms |
| Failure | Non-`2xx` triggers IDP's own retry. After its final attempt, our **polling fallback** takes over: any request in `DOC_PROCESSING` for more than 15 minutes is polled directly on I-09 |
| `PARTIAL` | Some documents processed, some failed. Treated as a result, not an error — the claim proceeds with a completeness warning |

> **Why the polling fallback is not optional.** A missed webhook without it means a claim sits in `DOC_PROCESSING` forever with no operator visibility. The fallback is fifteen lines of scheduled work and it converts a silent stall into a delay. **Note the contrast with I-04**, where the same sentence is true and the fallback rests on an endpoint nobody has confirmed exists. Here the counterparty confirmed both the delivery guarantee and the pull endpoint, which is what Confirmed means.

---

### I-09 — Claim Platform → IDP · retrieve structured result

**Status: Confirmed.** *(v1.1: Preliminary.)* C-IDP-04, and **C-IDP-03** for the confidence semantics.

**`GET /v1/extraction-requests/{requestId}/result`**

The payload is defined in **A4 §8** and is not restated here — that is the single source of truth, and duplicating it would guarantee the two drift. This contract adds only the transport rules:

| Rule | Detail |
|---|---|
| Availability | Retained and re-readable for **90 days**. The claim system copies it on first read but must be able to re-fetch for audit |
| Immutability | The result for a `requestId` **never changes**. A correction creates a new request; it does not mutate this one. This is what makes raw-versus-corrected comparison possible |
| Not ready | `409 NOT_READY` with `Retry-After`, never a partial body. **This is the one retryable `4xx`** — §1.3 and §1.5. An adapter that treats it as a normal `4xx` abandons every extraction that was not instantly ready |
| `paidBySocialInsurance` | Present on **every** cost line (A4 §8), integer VND, defaulting to `0`. The adapter carries it onto the claim **unchanged and unused**: MVP 1 runs the rule chain on the billed amount and deducts no BHYT share (declared simplification S-17, A5 §6.1). Capturing it now is what makes turning coordination on later a configuration change rather than a re-extraction of the whole ground-truth set — the release gate on that is at **I-11** |
| Size | Up to 2 MB for a large package. Responses are gzipped |
| Confidence semantics | `confidence` at claim level is the **minimum** across the mandatory field set, per A4 §7 — not the mean. **C-IDP-03 confirmed a numeric score in `[0,1]` per extracted field, comparable across fields and document types**, which is now the agreed semantics rather than our assumption. The adapter still asserts the minimum-not-mean rule on arrival rather than trusting it. Confidence remains a **routing** signal and stays out of the auto-approval gate (D1 §5.8) |

#### The `lines` array — two additions requested at v2.2, and one open question whose priority rises

**A4 §8 owns the extraction payload and this section does not restate it.** What v2.2 adds is the requirement, because the consumer that needs these two fields did not exist when A4 §8 was written: **the term engine (F66) and the limit ledger's non-monetary counters.** A1 §4.11 publishes the closed fact vocabulary a term may test, and **two of its twelve facts are not available today.** Both are questions for IDP, both are cheap to ask now, and **neither blocks the build** — each degrades a term or a counter to a human.

| Request | Level | What it feeds | Consequence if it is not returned |
|---|---|---|---|
| **`facilityCountry`** — the country of the treating facility · **`Q-IDP-05`** | Event, carried on the document header rather than on the item | **Exclusion #10, *treatment received outside Vietnam*** (A5 §7) | The exclusion **cannot be class `D`**. It falls to a checkpoint on every claim it selects, and a term that raises a checkpoint on every foreign-looking provider name is a queue nobody works. The alternative — inferring a country from a provider name — is exactly what A4 §2's *"never infer"* forbids |
| **A treatment-day count** — the number of treatment days the line covers · **`Q-IDP-06`** | **Per line** | **`DAY` counters** — `counterType = DAY` increments by the treatment-day count of the line (A1 §4.10, D1 §5.9.2) | A benefit whose limit is expressed in **days** is not fully enforceable. `DAY` counters can be maintained only where the line's own treatment date range makes the count derivable, and A5's per-day room caps are the benefits that need them most |

> **Both are cheap now and expensive later, and that is the whole argument for asking them in the round already open.** They are questions about **what the extraction model is trained and configured to return**. Asked before the model is fixed and the ground-truth set is labelled, they cost an entry on a list. Asked afterwards, `Q-IDP-05` costs a re-pass over the labelled documents and `Q-IDP-06` may cost a change to the extraction model itself.

**`Q-IDP-02` — line-level document date (`L15`) and line-level ICD (`L16`) — is already open, and from v2.2 its priority RISES.** It was raised as a question about the **clinical track**: without a line-level ICD, Track 2 reasons about eight cost lines against one event-level diagnosis, which is the weakness it was meant to remove. **It now blocks two policy exclusions as well** — congenital conditions, which is an ICD chapter-Q test, and the infertility / assisted-reproduction / contraception / termination group, which is a diagnosis-or-procedure test (A5 §7, exclusions #5 and #9). **That is a different kind of consequence and it should not be averaged in with the first.** A track that reasons less well still produces recommendations a human reviews; **a contractual term that cannot be evaluated produces nothing at all**, and the customer is paid for something the policy excludes.

**There is no synchronisation interface, and its absence is deliberate.** No contract in this specification lets IDP write into a claim, and none re-reconciles an extraction result against claim data after the first read. Both reference systems studied have exactly such a sync, and in one of them it *deletes* claim rows that were deleted upstream — which means a re-read can change a settlement while an approver is reviewing it. We remove the hazard by removing the mechanism (Framework §4.2 IDP-B3, D1 §1.4 AR-3).

**On first read, the claim system copies the result into its own store and the two diverge from that moment.** Extraction is a proposal; the claim record is the decision. Every subsequent assessor correction is written to the claim copy, and the pair stays comparable forever — which is what makes "how often does the assessor overrule the machine" an answerable question, and therefore what makes the auto-approval threshold tunable.

---

### I-10 — Claim Platform → IDP review UI · deep link

**Status: Confirmed.** *(v1.1: Preliminary.)* C-IDP-08 confirmed 21/08/2026. Priority 3 — F36. **Confirmed, and still nothing depends on it.**

`GET {idpBaseUrl}/review?requestId=…&documentId=…&returnUrl=…&sig=…&exp=…`

Opened in a new tab. `sig` is an HMAC over the parameters, `exp` a Unix timestamp; links expire after **15 minutes**. The assessor's identity is carried by their IDP session, not by this link — a deep link is a pointer, never an authentication mechanism.

The v1.1 note that *"if C-IDP-08 comes back negative, F36 drops"* is spent: it came back positive. F36 stays at Priority 3 on its own merits, which is that it is a convenience for deep investigation and no workflow routes through it.

---

### I-11 / I-12 — Claim Platform ↔ Finance · payment instruction and confirmation

**Status: Confirmed** — the contract is ours, because in MVP 1 the counterparty is a stub. It is specified as though it were real, because the whole point is that the stub is later replaced by a bank or treasury integration without the claim system noticing.

**I-11 · `POST /v1/payment-instructions`** · `Idempotency-Key` required — **this is the one place where a duplicate costs real money.**

```jsonc
{
  "instructionRef": "PAY-26-000417-1",
  "claimNo": "CLM-26-000417",
  "amount": 1200000, "currency": "VND",
  "payee": {                                 // from the beneficiary, not the insured — F58
    "name": "…", "bankCode": "999999",              // reserved test BIN; never a live institution
    "accountNo": "…", "accountNameVerified": true
  },
  "narrative": "BT CLM-26-000417",
  "valueDate": "2026-08-22"
}
```

Response `202`: `{ "instructionRef": "…", "state": "ACCEPTED", "acceptedAt": "…" }`

| Rule | Detail |
|---|---|
| `instructionRef` | Generated by the claim system, unique, and **the business idempotency key**. A repeat with the same ref returns the original state and does not create a second instruction |
| `accountNameVerified` | Must be `true`. An instruction against an unverified account is rejected `422 ACCOUNT_NOT_VERIFIED`. Framework F26 requires the verification and **F58 masters the account**; this is where it is enforced. **It is the seed's `payout_account.verified` under this document's name — the mapping is §5.1.1** |
| Amount | Must equal the claim's approved settlement total. The adapter re-checks against the claim before sending — an instruction that disagrees with its claim is refused, not corrected |
| Terminal | `ACCEPTED` is not `PAID`. The claim moves to `PAYMENT_IN_PROGRESS` and waits for I-12 |

> **The `payee` block improved in v2.0, quietly.** In v1.1 the beneficiary and the payout account were read from the Policy Data Store over I-05 and carried here. They are now **Claim Platform master data (F58)** — read locally, with no external call anywhere on the path from approval to payment instruction. The money path touches no third party at all, which is the correct shape for it: a payout account is created and verified at claim time, and a payee is a party to a settlement rather than to a contract of insurance (Framework §4.3).

> **The BHYT gate sits on this interface, and nowhere else.** `paidBySocialInsurance` is extracted on every cost line (A4 §8) and carried onto the claim by I-09, but no step of the rule chain deducts it — declared simplification **S-17**. The consequence is arithmetic, not opinion: on a claim where social insurance already paid part of the bill, the `amount` in the payload above is **larger than an indemnity product owes**. That is tolerable only while boundary condition **BC-3** holds and this interface talks to a stub. **Coordination of benefits is therefore a release gate on the first real payment instruction sent from I-11 — not a backlog item, and not something to discover in production** (Framework §7.4 S-17, §9.4 BC-3). The field is already on the wire so that turning the gate off is a rule-chain change and nothing more.

**I-12 · `POST /v1/webhooks/payments`** — HMAC-signed.

```jsonc
{ "instructionRef": "PAY-26-000417-1",
  "state": "PAID",                         // PAID | REJECTED | RETURNED
  "paidAt": "2026-08-22T09:14:00+07:00",
  "bankReference": "FT26230A9K21",
  "failureReason": null }
```

`PAID` → claim to `PAID`, customer notified. `REJECTED` → back to the assessor with the reason, and a **reversal** is posted against the ledger. `RETURNED` (paid, then returned by the receiving bank) → an operational state requiring a corrected account and a new `instructionRef`; the original is never reused, and a reversal is posted.

> **The ledger is already final by this point.** Consumption is finalised at *approval* (P-04, A5 §6.1 step 14), before any payment instruction is sent. A failed payment therefore posts a reversal rather than un-finalising — consumption is never deleted, only reversed. This matters for concurrency: an approved claim has committed the member's limit, and a second claim adjudicating at the same moment must see that commitment, not a provisional figure that might evaporate. **The ledger is exclusively ours — by decision now, not by expectation** (Framework D-B, C-BUS-09): InsureMO holds no consumption we read or write, which is why C-IMO-02 is moot rather than confirmed.

---

### I-15 / I-16 — Claim Platform → IDP · clinical appropriateness assessment

**Status: Confirmed.** *(v1.1: Provisional.)* This is a **second, distinct IDP capability**, and in v1.1 it sat expressly **outside** the preliminary confirmation of 17 August, which concerned extraction only. **On 21 August 2026 the IDP team confirmed it in writing: C-IDP-09** for the assessment itself, **C-IDP-10** for the versioned knowledge base behind it, with named public sources and a named accountable owner. Framework **RK-13 closes with C-IDP-09** — auto-approval's condition 2 now has a mechanism behind it and the gate can fire rather than being permanently unsatisfiable.

> **What Confirmed does and does not say here.** It says the capability exists and the contract below is ours to define — the first limb of §0.1's definition. It does **not** say the knowledge base has clinical sign-off; none was offered and none is planned, which is why Framework **RK-08 survives its own confirmation** at High/High. The controls that carry it are in this contract and are not decoration: evidence cited on every recommendation, a human confirming each one, and the agreement rate measured from day one.

**Why it is a separate interface and not a field on I-09.** Extraction reads a document; this reasons about a diagnosis. They have different inputs, different latencies, different failure modes, and — most importantly — different output authority: I-09 returns **data**, I-15/I-16 return **recommendations that bind nothing**. Folding them together would make it impossible to run one without the other, and advisory mode (D1 §5.11) depends on exactly that separation.

**I-15 · `POST /v1/clinical-assessments`** · `Idempotency-Key` required.

```jsonc
{
  "claimNo": "CLM-26-000417",
  "eventNo": "CLM-26-000417/1",
  "callbackUrl": "https://claims.internal/v1/webhooks/idp",
  "clinicalContext": {
    "diagnoses": [ { "icd10": "N39.0", "text": "Nhiễm khuẩn đường tiết niệu" } ],
    "episodeType": "OUTPATIENT",
    "serviceDateFrom": "2026-07-14", "serviceDateTo": "2026-07-14",
    "patient": { "ageBand": "50-59", "sex": "M" }   // band, never a date of birth
  },
  "lines": [
    { "lineNo": 1, "sourceDocumentId": "DOC-8891",
      "itemName": "Atorvastatin 20mg", "serviceCategory": "DRUG",
      "quantity": 30, "lineAmount": 168000 },
    { "lineNo": 2, "sourceDocumentId": "DOC-8891",
      "itemName": "Siêu âm hệ tiết niệu", "serviceCategory": "IMAGING",
      "quantity": 1, "lineAmount": 410000 }
  ],
  "knowledgeBaseVersion": "kb-2026-08-01"    // echoed back; pinned per claim
}
```

Response `202`: `{ "assessmentId": "…", "state": "QUEUED", "acceptedAt": "…" }`

| Rule | Detail |
|---|---|
| **No patient identity** | Age **band** and sex only. No name, no date of birth, no ID number, no member id. The assessment needs clinical context, not a person. **This is also why the clinical track is unaffected by the store's deletion**: it never read policy data and never will |
| **No money influence** | `lineAmount` is sent so a rationale can name the amount. **The assessment must not vary with it** — a recommendation that depends on how much a line costs is not a clinical judgement |
| **Line numbers are the join key** | `lineNo` refers to the claim's own cost lines. IDP never sees or returns a claim's internal identifiers beyond `claimNo` |
| **Knowledge-base version is pinned** | Sent by the claim, echoed in the result, stored on the claim (D1 BR-KB-4). A claim assessed today must be reproducible when the base has moved. **Who mints a version, and the scheme, is still open — X-08** |
| **`serviceCategory` is the canonical twenty** *(sixteen until v2.2)* | `CONSULT`, `LAB`, `IMAGING`, `PROCEDURE`, `DRUG`, `SUPPLEMENT`, `PHYSIO`, `DENTAL`, `ROOM`, `ICU`, `AMBULANCE`, `DELIVERY_NORMAL`, `DELIVERY_CS`, `MATERIAL`, `ADMIN`, `OTHER`, and — added when the set widened at v2.2 — **`SCREENING`, `VACCINATION`, `OPTICAL`, `HEARING`** — A5 §8.1 and A4 §4.5 are normative for the set, and it is the same list the benefit engine maps. **Not a free-text label and not a synonym**: an unrecognised value is rejected `422` at the adapter, per §1.1. **Which of the twenty IDP can actually reason over is still open — X-07** |

> **`serviceCategory` already exists and is already carried on this payload, and v2.2 says so because an intermediate draft said the opposite.** A working draft of the *Solution Paper* recorded the category as **missing** and raised a vendor question asking IDP to supply one. **It is not missing.** It has been on this contract since v1.1, A4 §4.5 owns the set, A5 §8.1 maps it to benefits, and the seed has carried it throughout. **The withdrawn question is recorded rather than deleted** — the vendor register carries it in its mapping table — because a question asked and then silently dropped reads, to the party who received it, exactly like a question they failed to answer.
>
> **What was genuinely missing was four *values*, and they are ours.** `SCREENING`, `VACCINATION`, `OPTICAL` and `HEARING` do not exist among the original sixteen, which is why exclusions #2 and #4 were unevaluable. **They are Claim Platform master data: no vendor question, no dependency, one seed change** (A5 §8.1 owns the set; the four values are carried in `A5_seed_v1.5.json`). **No IDP capability is required to add a value to a set IDP maps into** — what IDP must do is return a value in the set, and X-07 already asks which of them it can reason over.

**I-08 carries both.** The existing webhook gains a discriminator: `"kind": "EXTRACTION" | "CLINICAL_ASSESSMENT"`, with `requestId` or `assessmentId` accordingly. One endpoint, one signature scheme, two payload shapes.

**I-16 · `GET /v1/clinical-assessments/{assessmentId}`**

```jsonc
{
  "assessmentId": "…", "claimNo": "CLM-26-000417",
  "state": "COMPLETE",                    // COMPLETE | PARTIAL | ERROR
  "knowledgeBaseVersion": "kb-2026-08-01",
  "recommendations": [
    { "lineNo": 1,
      "proposedReasonCode": "NOTREL",     // NOTREL | NOTCOV only — A1 §5.2
      "confidence": 0.91, "confidenceBand": "HIGH",
      "rationaleVi": "Atorvastatin là thuốc hạ mỡ máu, dùng để điều trị rối loạn lipid máu; không liên quan đến chẩn đoán nhiễm khuẩn đường tiết niệu (N39.0).",
      "evidence": [
        { "type": "KB_ENTRY", "ref": "kb/lipid-lowering-agents/indications",
          "summary": "Nhóm statin — chỉ định cho rối loạn lipid máu và dự phòng bệnh tim mạch" },
        { "type": "DIAGNOSIS", "ref": "N39.0" }
      ] }
  ],
  "linesAssessed": [1, 2],
  "notes": null
}
```

| Rule | Detail |
|---|---|
| **Only two reason codes** | `NOTREL` and `NOTCOV`. Everything else is a policy question and belongs to Track 1. An assessment returning any other code is rejected at the adapter |
| **A line with no recommendation is not "approved"** | It is simply not recommended against. `linesAssessed` says what was looked at; a line absent from it was **not assessed**, and that is different from being clean |
| **`evidence` is mandatory on every recommendation** | Empty evidence is a malformed response, not a low-confidence one (D1 BR-KB-2). **With no clinical sign-off behind the knowledge base (RK-08), citation is one of the three controls carrying the exposure — it is not a nicety** |
| **Rationale is Vietnamese, patient-readable** | It is the basis of what the customer will eventually be told (A2 §4, `NOTREL`) |
| **`PARTIAL`** | Some lines assessed, some not. A result, not an error — the claim proceeds with `linesAssessed` naming the gap |

**Errors and failure.** `409 NOT_READY` with `Retry-After` — **retried, per §1.3's single `4xx` exception**; an adapter that abandons the assessment here loses Track 2 on every claim that took longer than one round trip · `422` unknown `serviceCategory`, unknown `knowledgeBaseVersion`, or no lines · `503` unavailable. **On any terminal failure the claim proceeds on Track 1 alone with `CLM-CONSOLE-015` raised** — D1 BR-TRACK-5: an assessment that did not run is never recorded as a clean one. **Auto-approval's condition 2 therefore goes inactive rather than open**, which is the fail-safe RK-13 recorded and which survives RK-13's closure.

**Mock.** Seeded with a small fixture set keyed on `itemName`: one drug whose therapeutic class is unrelated to the episode's diagnosis, one imaging line that is related to it, one clean claim, one `PARTIAL`, one hard failure. That is enough to build and demonstrate F56's whole review-and-re-run loop before any knowledge base exists — which is exactly what the advisory-mode fallback in D1 §5.11 needs, and advisory mode remains the recorded reduction option even though capability is now confirmed.

> **A note on what this interface must never become.** If the recommendations reduce to a lookup on `itemName` × `icd10`, the capability belongs in Track 1 as a deterministic exclusion table, where it is auditable and reproducible. Track 2 exists for judgements a table cannot hold. A mock that *is* a table is correct; a production service that is one is a Track 1 feature wearing a Track 2 badge (D1 §5.10.6).

---

## 4. Cross-interface guarantees

Individual contracts are the easy part. These are the properties that only exist across several of them, and they are where integration projects actually fail.

**Each guarantee is checked against v2.0 below.** Three survive untouched because they were always properties of systems we build; four are restated with their boundary named; one is materially changed; and one is added, because a guarantee that was a property of our own store has to be re-established somewhere.

| # | Guarantee | Status in v2.0 | Why, and where it is enforced |
|---|---|---|---|
| **G-1** | **A claim's snapshot is taken from one consistent read, and it is written exactly once.** I-05's coverage and benefit calls are made together at registration and the results are written to the claim atomically | **Materially changed** | The reason is unchanged: a snapshot assembled from two reads minutes apart can straddle an administrative edit and produce a schedule that never existed. **What has changed is that we can no longer deliver the first half.** Across two calls into a vendor there is no transaction, no isolation level and no read-consistency guarantee, and none can be asserted by us. **So the guarantee splits.** *Written exactly once, atomically* survives in full — it is our write, into our claim. *One consistent read* becomes two things: a **requirement on the snapshot** (see G-8) and a set of **adapter checks** (below) |
| **G-1a** | **The adapter proves consistency rather than assuming it.** The two I-05 responses must agree, or the snapshot is not assembled | **New, and it is G-1's replacement** | Both responses carry `asOfDate`, and both must echo the one we sent. The `planCode` and `benefitLevelNo` on the coverage response must be the plan the schedule was fetched for. `scheduleEffective` must cover `asOfDate` (§1.5 rule 3). If any check fails, the adapter **fails closed** and the claim goes to the operator queue (G-3) — it does not re-read and hope, and it does not assemble a snapshot from two answers that disagree. **What would make this unnecessary is a single composite read, or an as-of/version token echoed by both calls; that is the right thing to ask InsureMO for — X-11** |
| **G-2** | **Ledger consumption is two-phase, and the second phase is _approval_ — not payment.** Provisional at calculation, final at approval, invisible to other in-flight claims until final. I-11/I-12 happen *after* finalisation and do not move the ledger | **Holds, unchanged and strengthened** | P-04 and A5 §6.1 step 13 both say "final at approval". A payment later rejected is handled by a **reversal** (G-2a), not by leaving the consumption provisional — an approved claim has already committed the limit and a concurrent claim must see that. **The ledger is now exclusively ours by decision** (Framework D-B, C-BUS-09), not merely by expectation: InsureMO holds no consumption we read or write, which is why C-IMO-02 is moot rather than confirmed |
| **G-2a** | **A `REJECTED` or `RETURNED` payment posts a reversal, it does not un-finalise.** The consumption record carries reversal information; consumption is never deleted | **Holds, unchanged** | P-04: *"consumption is never deleted, only reversed or offset"*. Un-finalising would make the ledger's history unreconstructable. Entirely within systems we build |
| **G-3** | **No interface failure silently declines a claim.** Every adapter failure produces a system state (`EXTRACTION_ERROR`, `DOC_PROCESSING` timeout, `PAYMENT_IN_PROGRESS` stall, **`COVERAGE_UNVERIFIED`**) with an operator queue — never a `DECLINED` | **Holds, and carries far more weight** | A claim declined because a service was down is the single worst outcome this system can produce, and it is indistinguishable from a correct decline unless the states are kept separate. **In v1.1 the coverage read hit our own store, so the failure was ours to prevent. It now hits a vendor, so the failure is ours only to *absorb*** — which makes the state, not the uptime, the control. §1.5 rule 2 is the operative sentence: **a `404` or a timeout from InsureMO means *cover could not be established*, never *there is no cover*.** The new state name is spelled out here because "the claim sits in registration" is not a state an operator can find |
| **G-4** | **Every webhook handler is idempotent on the business key**, not the delivery id: I-04 on `policyNo`, I-08 on `requestId` or `assessmentId` per its `kind`, I-12 on `instructionRef` | **Holds on our side; its premise is now unconfirmed on one interface** | Exactly-once processing is our responsibility and is unaffected. The premise — *at-least-once delivery is guaranteed by the senders* — is confirmed for IDP (C-IDP-04) and true of the Finance stub. **For I-04 it is unknown, and InsureMO may deliver at-most-once or not retry at all.** Idempotent handling protects against duplicates; nothing in this guarantee protects against a webhook that is simply never re-sent, which is why I-04's polling fallback is mandatory and why **X-10** matters |
| **G-5** | **One correlation id spans all three systems** for one claim, from submission to payment | **Holds, with its boundary now stated** | Without it, diagnosing a stuck claim means correlating logs by timestamp, which does not work under load. **Inside the Claim Platform and across the IDP interfaces it is genuinely end-to-end** — the header convention is ours and IDP accepted it. **At the InsureMO boundary it stops.** We record it on our side of every call; whether InsureMO carries it into its own logs, or returns a reference of its own that we can store beside it, is unknown — **X-14**. Until it is answered, the adapter log is the join, and diagnosing an InsureMO-side problem means handing them a timestamp and a request. That is a real reduction from v1.1, where the fourth system's logs were ours to grep |
| **G-6** | **Every adapter has a circuit breaker.** Five consecutive failures opens it for 60 seconds | **Holds, and it is now a load control as well as a self-protection control** | It prevents a slow counterparty from exhausting the claim system's connection pool and turning one system's outage into two. **Against a vendor it does something v1.1 did not need it to do:** InsureMO's rate limits are unknown (**X-13**), six retries per adapter under an outage is a burst, and we cannot raise a vendor's capacity the way we could have raised our own store's. An open breaker on I-05 degrades to G-3's operator queue — never to a decline |
| **G-7** | **A clinical recommendation never reaches the ledger.** I-16's output is attached to cost lines and waits. Only an assessor's confirmation causes the rule chain to re-run, and only that re-run changes an amount | **Holds, unchanged** | D1 BR-TRACK-3 and BR-TRACK-4. This is the guarantee that keeps every settlement reproducible from deterministic inputs, and it is enforced by the adapter refusing to write to anything but the recommendation store. Both ends of the reasoning are ours; nothing in this reissue touches it |
| **G-8** | **Every value the engine adjudicates against is read from the claim's snapshot, never fetched at calculation time.** **`I-05` alone populates the snapshot**; no other interface reads policy data onto a claim, and no rule step calls out | **New in v2.0. Corrected at v2.3** | **This is where G-1's lost half re-establishes itself, and it is principle P-02 restated as a cross-interface property rather than a design preference.** With the adjudicable benefit tree in a vendor's system, the snapshot is the only local record of what the contract said when the claim was assessed. It is what makes a settled claim reproducible if InsureMO changes a product version, loses an effective date, or is unavailable when someone asks why a claim paid what it paid. **Concretely:** the schedule is snapshotted whole (I-05 property 3), `"INHERIT"` is resolved to an integer at snapshot time and the resolved value is written, the four properties of I-05 property 5 are resolved to the nearest ancestor and the resolved values are written, and re-running the chain on a settled claim reads the snapshot and makes no external call. An engine that re-fetches is an engine whose past settlements can change without anyone editing anything. **v2.3 removes `I-01` from this row.** It read *"I-01 and I-05 populate the snapshot"*, and **§I-01 says the opposite one page earlier**: I-01's output is **design-time**, feeding the adjudicable product model under **P-03**, and nothing it returns is written onto a claim. Naming it here made the guarantee describe a path that does not exist, on the one guarantee that makes a settled claim reproducible — and it is the kind of error that would have been discovered by somebody implementing the wrong half of it |

> **The pattern across the whole table is worth naming.** Every guarantee that survives untouched — G-2, G-2a, G-7 — is a property of systems we build. Every guarantee that needed restating is one that reached across a boundary that used to be internal. And the one guarantee that could not survive as written, G-1, is the one that assumed a transaction. **That is the general shape of what this reissue did to the document**, and §4 is where it is most visible.

---

## 5. Mock strategy — the part that unblocks the build

Every interface has a mock **specified** for it, here and in §3. **None of the sixteen is built.** That is **P0-4b** in the Priority Plan, it is owned by the dev lead, and until it is done this section describes an intention rather than a running system — the contracts unblock specification, the mocks unblock development, and only the first half is delivered.

What follows is therefore a set of **requirements the mocks must meet**, not a description of ones that exist; the per-interface **Mock** notes in §3 read the same way. They are not throwaway test doubles: they are a **deliverable** in their own right, to be versioned alongside this specification.

**And they matter more in v2.0 than they did in v1.1.** P-06 has lost the half that promised substitutability; what it still delivers is that the build never waits, and the mocks are the entire mechanism by which it delivers it. A mock that does not exist is a principle we have written down.

| Property | Rule |
|---|---|
| **Same contract** | A mock serves the same OpenAPI schema as the real service. If the schema changes, both change. **For the six InsureMO interfaces, "the same schema" means the adapter's output contract (§1.6 responsibility 5) — the mock stands in for the adapter's far side, and a mock written from a guess can only ever prove our side of the guess** |
| **Selection** | One environment variable per interface: `IFACE_I07_MODE = mock \| real`. Not a build flag, not a code branch — switchable per environment and per interface, so integration proceeds one interface at a time. **This is how the InsureMO sandbox gets evaluated when it arrives (C-IMO-06): one interface at a time, against the same contract tests, with the rest of the system still running on mocks** |
| **Scriptable** | Each mock exposes happy path, business-error, slow (5s), and hard-failure modes. Selected by magic values in the request, documented per interface. **The InsureMO mock adds two modes the store never needed: an inconsistent pair (G-1a) and a schedule whose effective range does not cover `asOfDate` (§1.5 rule 3)** |
| **Seeded consistently** | All mocks share **one** seed dataset, **`A5_seed_v1.5.json`**, built from A5: same members, same policies, same plan codes. A mock that disagrees with another mock teaches the team wrong behaviour |
| **Contract tests** | The same test suite runs against mock and real. **A real service that fails the suite has not been integrated, whatever a demonstration showed.** That sentence was a general principle in v1.1. In v2.0 it is the operative acceptance criterion for the only capability with no fallback behind it (RK-14) |

### 5.1 The shared fixture, and who owns which collection

**`A5_seed_v1.5.json` is the current fixture, and it holds sixteen collections.** It was revised on 19 August, at v1.2, to add five — `policy`, `benefit_level`, `member`, `payout_account` and `beneficiary` — because without them I-05 could not be mocked at all. Its own metadata described those five as *"the five policy-side tables of the Policy Data Store's eight."* **The store is not built, and none of the five disappears with it.** What changes is what each one *is*: three of them now back a vendor's mock, and two of them are not mock data at all.

**This table is what a mock builder splits the loader on, so it lists every collection in the file and not only the ones a reissue happened to touch.** Until v2.3 it carried **eleven of sixteen**, and a collection with no row here has no stated side of the split — which is not a neutral omission, because the two sides have opposite lifetimes: one is deleted the day `IFACE_I05_MODE = real` and one is never deleted. **The seed's own `meta.collection_ownership` carries the same sixteen markers, and the two are to be read together; where they ever disagree, the seed is the fixture and this table is the contract, and the disagreement is a defect to be closed rather than a choice to be made.**

| Collection | What it is | Backs |
|---|---|---|
| **`product`** | **InsureMO mock fixture** — the product definition. InsureMO masters it (Framework §6.1) | I-01, I-02, I-03 |
| **`plans`** | **InsureMO mock fixture** — the three plan tiers, returned with the catalogue | I-01, I-02 |
| **`policy`** | **InsureMO mock fixture** — the policy and certificate register, F05 / R-IMO-10. **From seed v1.5 it carries `rule_set_ref`**, which is what `ruleSetRef` on I-05 is served from | I-03, I-04, I-05 |
| **`benefit_level`** | **InsureMO mock fixture** — several benefit levels per policy with members pointing at one, which is what C-IMO-01c confirmed and what group cover needs | I-05 |
| **`member`** | **InsureMO mock fixture** — the insured-member and coverage register, F05. **From seed v1.5 it carries `continuous_cover_from`**, the seed half of `continuousCoverFrom` | I-04, I-05, and I-14 phase two |
| **`benefit_schedule_line`** | **InsureMO mock fixture** — the adjudicable benefit tree InsureMO confirmed it can carry (C-IMO-01, C-IMO-01b) | I-01, I-05 |
| **`benefit_waiting_period`** | **InsureMO mock fixture** — waiting periods by condition class, carried on the schedule. **Held on the group node only**, which is what makes I-05 property 5 load-bearing rather than tidy | I-01, I-05 |
| **`premium_rate`** | **InsureMO mock fixture** — the premium table behind quotation. A term of the product, so it sits with the product | I-02 |
| **`payout_account`** | **Claim Platform master data — F58.** Not a mock fixture. It seeds a real table in a system we build. **Its `verified` column is this document's `accountNameVerified`** — see below | F58, and I-11's `payee` block |
| **`beneficiary`** | **Claim Platform master data — F58.** Not a mock fixture. Same | F58, and I-11's `payee` block |
| **`provider`** *(new at seed v1.5)* | **Claim Platform master data — F57.** Not a mock fixture, and loaded exactly as the two above are. **`in_network` is the fact `PROVIDER_OFF_NETWORK` is evaluated against** at chain step 11; a provider absent from the list is treated as **off-network**, which is the safe default and is deliberate | **Nothing on an interface reads it.** It is the master behind `deductionCondition`, which I-05 returns on the schedule and the engine evaluates on our side |
| **`policy_term`** *(new at seed v1.4)* | **Claim Platform master data — F65 / F66.** Not a mock fixture. **It replaces `product_exclusion`**, which held three fields — `product_code`, `seq`, `text_en` — and no selector, no predicate, no scope and no effect | **F65, F66**, and chain steps 4 and 7. **Nothing on an interface reads it**: it is bound to `ruleSetRef` from I-05 and evaluated entirely on our side |
| **`service_category_benefit_map`** | **Claim Platform configuration — F48.** Not a mock fixture. **The contract between the extraction layer and the benefit engine**, twenty categories, and **it declines nothing** (A5 §7.4). **It carries two sentinels — `"NONE"` and `"ASSESSOR"` — documented below** | **Nothing on an interface reads it.** `serviceCategory` arrives on I-09 and is sent on I-15; the mapping from a category to a benefit code is ours and happens after |
| **`decline_reason`** | **Claim Platform master data — F47.** Not a mock fixture. Reference data behind the seven decline codes; **A1 §5.2 is normative for them and this collection is a copy** | **Nothing on an interface reads it.** I-16 may propose `NOTREL` or `NOTCOV` and no other code, which is enforced at the adapter (§I-16) |
| **`adjudication_rule`** | **Claim Platform configuration — F48.** Not a mock fixture. **A participation switch list, not an authoring model** — `{step, name, enabled}` against a fourteen-step sequence fixed in code and versioned with the release (P-05 as revised, S-39) | **Nothing on an interface reads it**, and nothing should: an interface that could switch a rule step off would put the chain's shape in a vendor's gift |
| **`condition_class`** | **Shared reference data — F07 / F48.** **The only collection of this kind in the fixture, and the only one both loaders read.** The canonical six codes; **A1 §4 is normative and this collection is a copy for convenience, never a second source** | **Both sides.** InsureMO's waiting periods and the Claim Platform's assessment must use the same six, and `waitingPeriodDays` on I-05 carries them verbatim |

**Sixteen collections: eight InsureMO mock fixture, seven Claim Platform master data or configuration, one shared reference.** The count is stated because it is checkable — the loader is correct when sixteen collections have found a side, and no other number is a passing result.

**`policy_term` is the collection of the second kind whose loss would be least visible.** An empty payee master stops a payment and somebody notices within a day. **An empty term register stops nothing** — chain step 4 tests no terms, every claim passes it, and the console reports *"0 terms tested"* on a screen nobody has learned to read yet. D2 §5.28.7 makes that state carry a standing banner for exactly this reason.

**The distinction is not bookkeeping, and getting it wrong has a specific failure mode.** A mock fixture is *pretend data standing in for a system that is not there yet, to be discarded when the real one arrives.* Master-data seed is *real data in a real table, which the system keeps.* `payout_account` and `beneficiary` are the second kind: they belong to the Claim Platform (Framework §2.1, §4.3 — a payout account exists to reimburse a claim, and a payee is a party to a settlement rather than to a contract of insurance). **If they are loaded as part of "the InsureMO mock", then on the day the sandbox arrives and the mock is switched off, the payee master empties and every settlement loses its destination.** So the loader splits them:

- one path seeds the **InsureMO mock**, and it is deleted when `IFACE_I05_MODE = real`;
- one path seeds the **Claim Platform's F58 tables**, and it is never deleted — it is ordinary reference data for a demo environment.

#### 5.1.1 One field, two names — `accountNameVerified` and `payout_account.verified`

**They are the same field, and until v2.3 no document said so.** The seed's column is **`verified`**; this specification calls it **`accountNameVerified`** — on **I-11**'s `payee` block, where it is a release condition on a payment instruction, and in **§I-05**'s mock note. **The mapping is one line and it belongs here, in the section that owns the collection:**

| Seed column | Wire field | Rule |
|---|---|---|
| `payout_account.verified` *(boolean)* | `payee.accountNameVerified` *(boolean)* | **Value carried unchanged. Name mapped by the loader.** `true` is the only value I-11 accepts; `false` is rejected `422 ACCOUNT_NOT_VERIFIED` at the adapter (§I-11) |

**The longer name is kept and the shorter one is not adopted**, because *verified* alone does not say **what** was verified — and what was verified is that the **account name matches the payee**, not that the account exists or that the bank code is live. On a field whose only job is to gate a payment, a name that invites the wider reading is a name somebody will eventually satisfy with the wrong check. **The seed is not renamed to match**, and that is a decision rather than an omission: the fixture is owned by A5, it is already reissued at v1.5, and a rename would break every loader written against it in exchange for a synonym. **A mapping written down costs a line; a mapping left implicit costs a loader that silently drops the column and a mock in which every payout account is unverified.**

#### 5.1.2 The two sentinels in `service_category_benefit_map`

**`service_category_benefit_map` carries two sentinel values, and they are documented here on the same terms §I-05 property 4 documents `"INHERIT"`** — because an undocumented sentinel is how a loader guesses, and a loader that guesses at this table guesses at which benefit a cost line is paid from.

| Sentinel | Means | Why it is a string, and what must not be done with it |
|---|---|---|
| **`"NONE"`** in `outpatient` or `inpatient` | **This category has no benefit in this treatment setting** — A5 §8.1's *n/a*. Seven categories carry it in one setting or the other, and `ADMIN` carries it in both | **Not `null`.** §1.2 fixes `null` as *not known*, and *no benefit exists here* is a **known fact** — writing it in the vocabulary of an unknown one is exactly the substitution §1.2 forbids. **A line landing on `"NONE"` is not declined by this table** — the table declines nothing (A5 §7.4) — it falls through to the line-level cover test at chain step 6 |
| **`"ASSESSOR"`** in `outpatient` or `inpatient` | **No rule maps this category; a person decides which benefit the line belongs to.** It appears on `OTHER` alone, in both settings | **It is a sentinel of the same kind as `"INHERIT"`, and a string for the same reason**: code that treats it as a benefit code fails loudly instead of looking up a benefit called *ASSESSOR*. It is **not** a decline, **not** a checkpoint of its own, and **not** a default benefit |

**Neither value is a benefit code, and neither may be sent on any interface.** They live in the fixture and in the Claim Platform's own configuration; nothing in the sixteen carries them, and an adapter that ever emitted one would be putting a word where a code belongs. **This is the third sentinel in the document and the last**: `"INHERIT"` on the wire (§I-05 property 4), and these two behind the boundary. Each of the three exists for the same reason — **there is a state that is neither a value nor an absence, and the only safe way to express it is a token that cannot be mistaken for either.**

**One consequential requirement on the fixture, and one that is withdrawn:**

1. **The file's own metadata must describe what the table above describes, and at seed v1.5 it does.** `meta.policy_data_note` once described five collections as tables of the Policy Data Store; it was corrected at seed v1.3 and extended at v1.4 and v1.5, and **`meta.collection_ownership` now carries an ownership marker on all sixteen** — with the file's own rule that *anything added to it must carry one too*. **That rule is what keeps this table and the fixture from drifting apart again**, which is how it drifted to eleven of sixteen in the first place. *(The seed file is not changed by this document; it is maintained under A5.)*
2. ***Withdrawn.*** **The requirement that the fixture represent both branches of C-IMO-10 is gone with branch B.** It asked for a deliberate divergence case — one member present in a claim-side extension and absent from InsureMO — so that the reconciliation obligation two member lists create would be visible in every test run. **B7 confirmed the bulk load and there is one member list**, so there is nothing to diverge. It was withdrawn in prose at v2.1 and left standing in this list until v2.3; **it is struck rather than deleted, because a requirement that quietly disappears reads like one somebody forgot.**

**The rule that makes this work: no feature is "done" against a mock alone, and no feature is "blocked" because only a mock exists.** Development proceeds on the mock; integration is a separate, scheduled, per-interface activity with its own definition of done.

---

## 6. What changes if InsureMO answers differently

Recorded now, while the reasoning is fresh, so the decision does not have to be reconstructed under time pressure later.

> **v1.1's version of this table is void, and it should be read once before the new one.** Every row of it ended the same way — *the schedule is mastered in the Policy Data Store, and the claim engine never changes* — and it closed with the sentence *"the fallback is always 'master it in the Policy Data Store'… this table is the evidence that the choice was correct."* **The choice was correct on the evidence available. The evidence changed.** InsureMO demonstrated the capability, the store's reason for existing went with it, and the fallback went with the store. What follows is the same question answered without one.

| If InsureMO… | Then | Cost |
|---|---|---|
| Cannot model a benefit **tree** with parent/child aggregation | **There is no second place for the schedule to live.** The adjudicable benefit tree has to be built and mastered somewhere, and the only candidate left is the Claim Platform — which means building, in a hurry, the component we deleted | **A build, not a swap.** This is **RK-14** firing. J2 cannot adjudicate without the tree, and **SC-03 and SC-08 both rest on it** |
| Cannot express **count limits** or the unlimited sentinel | Same as above. A schedule that holds money but not days cannot price a per-day room cap | Same |
| Cannot express **waiting periods by condition class** | Same as above. Adjudication is ours either way, but the *input* has to come from somewhere | Same |
| Cannot express **inherited co-payment** as anything the adapter can distinguish from absence | I-05's property 4 fails at the boundary rather than paying a co-paid benefit in full. Either every node arrives with an explicit rate, or the schedule is rejected | Contained — provided the adapter never infers inheritance from absence, which is why that rule is stated twice in I-05 |
| Has **no as-at-a-date semantics** behind the schedule, or its effective dating behaves unexpectedly | **P-02's snapshot degrades quietly, which is worse than failing.** The adapter's `scheduleEffective` check (§1.5 rule 3) is what converts it into a loud failure | Contained by the check, and it is why **C-IMO-05** is the contract-detail item where a surprise costs design rather than a mock |
| Issues policies **asynchronously** | I-03 returns an application id only; I-04 carries the issued policy. The buy journey gains a "we are issuing your policy" state | Half a day in the app; the contract already anticipates it |
| Does **not sign webhooks** the way §1.4 requires, or does not redeliver | I-04's body is treated as a notification and the policy is fetched over an authenticated read; the 15-minute polling reconciliation becomes the delivery guarantee rather than the safety net | Contained — **provided `GET /v1/policies?issuedSince=` exists**, which is itself unconfirmed (**X-10**) |
| Has **no bulk member load** | I-14 branch B: a claim-side member extension, two member lists, disjoint by `policy.source` | A permanent reconciliation obligation, specified in I-14 so it is taken as a decision |
| Does **not honour an idempotency key** | The business-key check before every I-03 retry (§1.3, I-03) | Weaker than an idempotency key, and the exposure is a duplicate policy rather than a duplicate payment |
| Returns `ruleSetRef` as **a document reference and not a set of codes** | The term register binds to `ruleSetRef` alone — **one register per rule-set version**, which is the design as specified today | **None to the build.** Two rule sets sharing a term hold it twice, and changing that term is two edits. **C-IMO-13** |
| Changes a benefit schedule **silently** on a mid-term endorsement — same `ruleSetRef`, same `benefitLevelNo`, different numbers | **D1 §5.9.5 cannot be implemented as written.** The snapshot cannot detect that the cap moved, so consumption cannot be carried against a cap the system does not know changed. The ledger's *cap as at the posting* becomes a record of what we happened to read rather than of what applied | **This is the one row in this table that is not contained.** Endorsements are out of scope for MVP 1 (S-05), so nothing stops today — but **the ledger's storage shape depends on the answer and a cap cannot be retrofitted onto historical postings**. **C-IMO-14**, and it is the reason the question is asked now |
| Has **no claim registration endpoint** | I-06 is dropped | None. It is already marked optional, nothing waits on it, and D-B closes as *none* regardless |
| Cannot provide a **sandbox** in time | Development continues on mocks indefinitely; integration moves to the end of the project as a scheduled block | **This is no longer only RK-04.** Contract-first still bounds *interface* rework to six contracts. What it no longer covers is the capability behind them, because there is no second implementation to swap to — that half is **RK-14**, and **C-IMO-06 is on the critical path for both** |

**The pattern across the new table, stated as plainly as the old one was.** The last five rows behave exactly as v1.1 promised: the contract absorbs the difference and the claim engine does not change. **The first three do not, and no amount of interface design will make them.** They are not integration problems; they are the single dependency this architecture now rests on. The mitigation is not in this document — it is **C-IMO-06**, a sandbox, evaluated by us, benefit by benefit, against **A5 v1.5** — the current data sheet, not the v1.2 the original demonstration used — early enough that a *confirmed* capability becomes a *proven* one while there is still time to do something about it.

---

## 7. Open items in this specification

The **X-nn** series is unchanged in numbering — X-01 to X-08 are the same items, restated where a reissue moved them — six were added at v2.0, two at v2.1, three at v2.2 and **one at v2.3, `X-20`. The series runs X-01 … X-20, none retired, none reused.** **X-09 to X-14 exist for one reason: they are questions that could not arise while we owned both ends of every contract.**

> **`X-12` is closed and is listed for the record.** B7 confirmed the bulk member load for creation **and** end-dating, and Framework `C-IMO-10` closed with it (§0.1a). It keeps its number and its row; **a closed item is struck, not removed**, because a number that vanishes is a number somebody will reuse.
>
> **Two rows below were carried in prose from v2.1 and never reached this table: `X-15` and `X-16`.** §0.1a and §I-05 both cite them, §0.2.2 records them as added, and the table skipped from X-14 to X-17. **They are restored at v2.3 in their own numbers** — the numbering was never wrong, only the table was incomplete, and a cross-reference to a row that does not exist is worse than a missing row because it reads as though somebody checked.

| # | Item | Owner | Blocks |
|---|---|---|---|
| X-01 | Exact InsureMO payload shapes for **I-01 … I-06** — I-05 joins the list with its demotion, and I-13 leaves it, having no InsureMO counterparty | InsureMO — C-IMO-03, C-IMO-04, C-IMO-05 | **Nothing in the build — mocks stand in. Everything in the proof** — the mocks can only demonstrate our side, and there is no second implementation behind them (RK-14) |
| X-02 | IDP's actual `templateSetCode` values and the `context` fields it honours | IDP team | Field-level only; I-07's structure holds and is Confirmed |
| X-03 | Confidence threshold value for **assessor routing** — which claims and fields are surfaced first | Ground-truth benchmark | Presentation tuning (A4 §7). **Not the auto-approval gate**, which does not test confidence (D1 §5.8). The *semantics* are now confirmed (C-IDP-03); only the threshold is open |
| X-04 | Whether Finance is a bank API or a treasury file in production | Business, MVP 2 | Nothing in MVP 1 — the stub contract is the abstraction |
| X-05 | Object-storage URI scheme and access model between the claim system and IDP | Dev lead | I-07 implementation, not its shape |
| X-06 | Retention period for adapter request/response logs — **now sharper: the I-01, I-04 and I-05 traces contain policy and member personal data crossing an organisational boundary, which they did not when the counterparty was our own store** | ISMS, and it is adjacent to C-IMO-07 | Nothing in the build; it is a decision that must land before a pilot with real testers |
| X-07 | **The `serviceCategory` values IDP can actually reason over.** *The capability half of this item is closed* — C-IDP-09 confirmed the clinical assessment on 21/08/2026 | IDP team | Nothing — the mock stands in, and advisory mode (D1 §5.11) works entirely on it. What is open is vocabulary, not existence |
| X-08 | **Knowledge-base versioning scheme and who mints a version.** *Narrowed:* C-IDP-10 confirmed provenance — named public sources — and a **named accountable owner**. The scheme and the minting event are still open | Business + IDP | I-15's `knowledgeBaseVersion` semantics, and D1 BR-KB-4's reproducibility claim |
| **X-09** | **InsureMO's error shape, and its mapping into §1.5's model.** Specifically: what it returns when a plan has no schedule effective at `asOfDate`, and whether it distinguishes *unknown member* from *member not covered* | InsureMO — with C-IMO-05 | The adapter's error mapping. Until it is answered, §1.5's three synthesis rules apply and the adapter checks `scheduleEffective` itself rather than waiting to be told |
| **X-10** | **I-04's webhook: signing scheme, replay window, and whether delivery is at-least-once.** And, inseparably, **whether `GET /v1/policies?issuedSince=` exists** — because it is the mandatory compensating control and it is unconfirmed | InsureMO — C-IMO-04 | Nothing in the build. It determines whether I-04's body is trusted as a payload or treated as a notification; the safe default is specified and costs one extra read |
| **X-11** | **Read consistency across I-05's two calls** — is there a composite read, or an as-of / version token both calls echo? | InsureMO — with C-IMO-05 | Nothing — **G-1a's adapter checks stand in**, and they fail closed rather than assembling an inconsistent snapshot. A token would let us prove consistency instead of inferring it |
| ~~**X-12**~~ | ~~**Bulk member *update* and *end-dating*, as distinct from bulk *create***~~ — **CLOSED, 24 August 2026.** Answer **B7**: InsureMO exposes a bulk member load **for creation and for end-dating**. Framework **C-IMO-10** closes with it, and I-14 branch B is withdrawn | InsureMO — C-IMO-10, **closed** | **Nothing.** I-14's `Re-import` rule is confirmed rather than assumed (§I-14) |
| **X-13** | **InsureMO's idempotency, rate limits and `429` behaviour.** §1.3 states these as obligations on the receiver; for I-02, I-03, I-06 and I-14's onward write the receiver is a vendor | InsureMO — with C-IMO-03 | Nothing in the build. It determines whether I-03's compensating business-key check (never re-submit an application whose `quoteId` already has a `policyNo`) is a belt-and-braces control or the only one |
| **X-14** | **Whether InsureMO echoes or records `X-Correlation-Id`**, or returns a reference of its own we can store beside ours | InsureMO — with C-IMO-03 | **G-5's boundary.** Nothing in the build; it determines whether diagnosing an InsureMO-side problem is a join or a conversation |
| **X-15** | **The renewal / continuous-cover attribute's exact name, type and — the part that matters — the *level* it is held at.** B1 says member-on-policy, in prose. **A policy-level flag answers the question truthfully and answers nothing for a member who joined a renewed policy mid-term** | InsureMO — with C-IMO-06, on the sandbox | **Nothing in the build**, and something in the result. If the level turns out to be policy-level, `V-119`'s inconclusive verdict stays in force for members whose coverage start is later than the policy start, and **A5 §5's renewal waiver runs for the rest** rather than for all |
| **X-16** | **Whether `ruleSetRef` carries a *version* or only identifies a rule set.** If it is unversioned, two wordings under one reference are still indistinguishable and **Framework S-22 has moved less than it looks** | InsureMO — with C-IMO-05, on the sandbox | **Nothing in the build.** The reference is snapshotted either way; what is open is whether it is enough to reconstruct *which wording* a settled claim was assessed under. **Read it with X-17 and X-20** — the three are the same field seen from three sides: is it versioned, is it decomposable, and is it enumerable |
| **X-17** | **`ruleSetRef`'s granularity — codes we can bind terms to, or a document reference only?** Carried here because this document returns the field and the term register consumes it | InsureMO — **C-IMO-13** | **Nothing.** The fallback *is* the current design: bind the register to `ruleSetRef` alone, one register per rule-set version. **Read it together with X-16** — that one asks whether the reference carries a version, and a reference that is neither versioned nor decomposable is the weakest of the four possible answers |
| **X-18** | **Whether a mid-term endorsement is visible to the snapshot** — a new `ruleSetRef`, a new schedule version, or a silent field change | InsureMO — **C-IMO-14** | **D1 §5.9.5, the mid-term benefit-level rule.** **The only item in this series that blocks a written rule rather than an implementation choice.** The adapter's `scheduleEffective` check catches a re-dated schedule and does **not** catch a silently edited one, and §I-05 says so rather than implying the check covers it |
| **X-19** | **Two extraction fields the term engine and the ledger need: `facilityCountry` (`Q-IDP-05`) and a treatment-day count per line (`Q-IDP-06`).** And, inseparably, **`Q-IDP-02`** — line-level ICD — whose priority rises because it now blocks two policy exclusions and not only the clinical track | IDP — **A4 §8 owns the payload** | **Nothing in the build.** Each degrades a term or a counter to a human: exclusion #10 falls to a checkpoint, `DAY` counters are maintainable only where derivable. **Cheap to ask now and expensive after the extraction model is fixed and the ground-truth set is labelled**, which is the whole argument for putting all three in the round already open |
| **X-20** | **Whether InsureMO can list the rule sets available to a term author** — *"return the rule sets in force for this product, or for this tenant"*. **New at v2.3.** D2 §5.28.3 F2 is typed `lookup(ruleSetRef)` and sourced `imo:I-05`, and **I-05 returns one reference for one member's coverage; no interface in the sixteen returns a list.** If the operation exists it would sit on **I-01**, the only one of the six whose subject is the product | **The client, with InsureMO.** Deliberately **not** raised here as a new `C-IMO-nn`: whether to put it to the vendor at all is theirs to decide, and this document's job is to make sure nothing waits on the answer | **Nothing.** **The fallback is specified and is the current design** (§I-05): F2 validates against the distinct `ruleSetRef` values already seen on snapshots, so an author can bind a term only to a rule set the platform has actually met. What it costs is that **the first policy under a new rule set cannot have its terms authored in advance** — D2 §5.28.7's empty-state banner is what surfaces that |

> **Six new items, and not one of them is about a payload.** They are about signing, delivery, consistency, idempotency, rate limits, error semantics and log correlation — the properties a system has that you never write down while you own it, because you simply build them. That is the difference between a contract written against yourself and a contract written against a vendor, and it is the reason this reissue is longer than a re-point of four rows would suggest.
>
> **`X-20`, added at v2.3, is a different shape again and worth naming as such.** X-09 to X-14 ask what a vendor's system *does*. **X-20 asks whether an operation exists at all** — not how it behaves, but whether there is anything to call. It is the only item in the series that reports a **missing interface** rather than an unknown property of a present one, and it is the only one whose owner is **the client rather than a counterparty**, because the decision it waits on is whether to ask the question, not what the answer is. **The build does not wait on either.**

---

## 8. Change control

This document changes when a contract changes, and a contract changes only by change request against the framework.

**Additive is not breaking.** A new optional field does not increment the version and does not require a coordinated release — which is exactly why §1.1's "ignore unknown fields" rule must be tested rather than assumed.

**Breaking changes** — removing a field, changing a type, changing an enum's meaning, changing an error's semantics — increment the path version, and both versions run in parallel for one release cycle. **For the six InsureMO interfaces this binds the adapter's output contract, which is ours, and not the vendor's endpoint, which is not.**

**Status promotion.** When a counterparty confirms an interface, its status moves Provisional → Preliminary → Confirmed in §2, the confirming artefact is named, and any mock that diverges is corrected in the same change. **A status that is promoted without a named artefact is not promoted.**

**Status demotion.** *(new in v2.0.)* **A status can move down, and v2.0 moved two.** It happens when the counterparty changes, when a confirmation is withdrawn, or when the basis on which a status was granted turns out not to have been what we thought. The same rule applies in reverse: **a demotion names its cause, in the row and in the changelog.** I-05 and I-14 were Confirmed under the limb *the contract is ours to define*, and that limb ceased to be true of them on 21 August 2026 — not because anything was discovered about the contracts, but because the system at the other end of them is not being built. A scheme that can only move upward will eventually record a confidence nobody holds.

**A question opened is not a status changed.** *(new in v2.2.)* **v2.2 opens three vendor questions and moves no grade**, and the rule behind that is worth stating with the other two. A status records **who stands behind a shape**. `C-IMO-13`, `C-IMO-14`, `Q-IDP-05` and `Q-IDP-06` are questions about **content and capability**, asked by a consumer that did not exist when the contracts were written — they tell a developer what is not yet knowable, which is what §7 is for, and they tell them nothing new about who stands behind I-05 or I-09. **A reissue that moved a grade on the strength of a question it had just written would be recording a confidence nobody holds, by the opposite route.** The same discipline as the third rule above: **the reason a status did not move is recorded, not assumed.**

*End of D3 v2.3.*
