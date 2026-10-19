# D1 — Business Requirements Document

**Project:** Digital Health Insurance Platform — MVP 1
**Document type:** D1 — what the business needs the system to do
**Version:** 0.5
**Prepared by:** BnK Solutions — Business Analysis
**Date:** 25 August 2026 (v0.5: an independent review of the whole set found twenty-nine findings against this document; every one that does not require a business decision is applied here, and the ones that do are opened as **B-18 … B-24** with **B-01** and **B-07** re-opened. See §0.2d. v0.4, 24 August 2026: the ten decisions of the *Solution Paper — Policy Terms, Rule Engine and Limit Management v1.1* are applied — the rule chain becomes **fourteen steps**, step 4 is rewritten against the **term register**, and the limit ledger gains a period, three counter types and a complete posting table. See §0.2b. v0.3, 21 August 2026: realigned to the Framework of that day — the Policy Data Store is not built; three systems, not four. See §0.2a)
**Baselined against:** *Solution Framework v2.5*
**Supersedes:** *D1 BRD v0.4* (24 August 2026)
**Annexes:** A1 Glossary v1.5 · A2 Message Catalogue v1.2 · A3 Permission Matrix v1.5 · A4 Document-Type Catalogue v1.3 · A5 Product Data Sheet v1.5 (`A5_seed_v1.5.json`) · D3 Integration Contracts v2.3
**Classification:** Internal — contains no client-identifiable material

---

## Part 0 — About this document

### 0.1 What it is, and what it is not

This BRD states **what the business needs**, in business language, with the rules stated precisely enough to be built from. It is the document a business reviewer signs.

It is **not** the screen specification. Field-by-field screen layouts, control behaviour, and per-action UI messages are D2. Where this document names a field, it is because the field carries a business rule, not because it defines the screen.

It does **not** restate what an annex already owns. Terminology is A1. Message wording is A2. Permissions are A3. Document types and extraction fields are A4. The pilot product's benefits, limits and waiting periods are A5. Interface payloads are D3. Where a rule depends on one of those, this document names the annex rather than copying it — so there is exactly one place to change each fact.

### 0.2 What changed in v0.2

v0.1 was reviewed independently on 18 August 2026. **Nine findings were severity-1**, and every one of them was a seam left by the two decisions that were reversed on 17 August — auto-approval retired then reinstated, and the deduction ratio changed from an assessor's discretion to a policy term.

| # | Finding | Fix |
|---|---|---|
| 1 | **§5.3's deduction-ratio step — the authoritative arithmetic — still described the retired discretionary deduction.** It was step 10 on the chain as it then stood and is **step 11 from v0.4** (§5.3). This is what D2 and the engine are built from | Rewritten to match A5 §6.1: a policy term, applied when its condition is met, no reason required |
| 2 | AR-4 listed the deduction ratio as an assessor action | Replaced with confirmation of a clinical recommendation |
| 3 | "Four conditions" survived in §3.2 J2-14, and §5.3 step 1 and §5.7 cited "condition 3" and "condition 4" **as guarantees the new gate no longer provides** | All rewritten; §5.7's closing argument re-argued on BR-TREE-5 alone |
| 4 | §3.2.3 promised no coverage failure is ever an automatic decline; the gate allowed exactly that | Condition 1 narrowed — see below. Both statements now hold |
| 5 | B-03 said auto-approval was retired, nine rows above B-12 saying it was in | Corrected |
| 6 | The lifecycle diagram had **no auto-approval transition at all**, and BR-AUTO-4's override had no path | Both edges added |
| 7 | Extraction confidence was a gate in A4 and not in D1 | A4 restated: confidence is an assessor **routing** signal |
| 8 | Two live `G-n` series — D1's reference gaps and D3's cross-interface guarantees — with overlapping numbers | D1's renamed **GAP-1…GAP-7** |
| 9 | The BHYT section sat inside the clinical-track discussion | Moved to **§5.6a**, beside the cost-share rules it belongs with |

**The gate was also narrowed.** Condition 1 now requires that **nothing was cut** — no cost line declined, no limit bound — because a wholly declined claim has a payable of zero and satisfied an amount test trivially. One predicate; it preserves §3.2.3 and §4.5 without amending either.

**Two substantive holes were closed**, neither a consistency defect: the deduction ratio's `PROVIDER_OFF_NETWORK` condition **had no data source** (it is now a flag on the provider master, F57 — otherwise an assessor would type it, reintroducing the discretion B-09 removed); and there was **no rounding rule anywhere** (now stated once, at each step, in §5.3 and A5 §6.2 — BR-TRACK-4 re-runs the chain, so rounding only at the end could produce a different total on the same inputs).

**One exposure was declared rather than fixed:** **S-20** — no contribution rule where a member holds a second private policy, which is common in this market.

### 0.2a What changed in v0.3

v0.3 is not a review pass. It is an **architecture change**, and it came from an answer rather than from a defect.

On 21 August 2026 InsureMO demonstrated — live, against A5 v1.2, benefit by benefit — that its product model can carry the **adjudicable** benefit schedule: a tree two to three levels deep, aggregation from child to parent, limits in count as well as money with an explicit unlimited representation, waiting periods that differ across the six condition classes, inherited co-payment, and the deduction ratio together with the condition that triggers it. It also holds the policy, member and coverage register, and it answers an as-at-a-date query. **The reason the Policy Data Store existed has gone, so the store is not built.** Separately, the client decided that **InsureMO is used for the policy side only** and that **the claim platform is built entirely by us** (**B-17**, Part XI).

| # | Change | Where |
|---|---|---|
| 1 | **Three systems, not four.** Every reference to the Policy Data Store is re-attributed to the system that now holds the thing. **Policy business is InsureMO's; claim business is ours** | §2.1, §3.1, §3.2.1, §3.3, §5.4, §5.9, §6.1, §7.1, §7.4 |
| 2 | **Feature codes.** Group A becomes **F01–F07** (F05 restated as the register itself; **F07** the benefit-schedule query as at a date, returned as a tree). Group E becomes **F41–F59**, gaining **F58** — the payee and payout-account master with account-name verification — and **F59**, the group policy and member bulk load. **Group B and F11–F14 are deleted and never reused.** 48 features, 43 at Priority 1 | §6.1 |
| 3 | **B-17 recorded and closed.** InsureMO for the policy side only; the claim platform built entirely by us. Framework v2.0 carries the same decision as **C-BUS-09** | Part XI |
| 4 | **AR-1 … AR-4 are unaffected, and they get stronger.** Stated in one line rather than left for a reader to infer | §1.4 |
| 5 | **P-02, the policy snapshot, matters more than it did — not less.** Sharpened where this document argues for it | §7.2, and §3.2 J2-8 |
| 6 | **Every rule that reads policy or member data was re-checked for a source.** One had lost it — **BR-PAY-1**'s account-name verification, because the payout account is now **F58**, ours, and not a field returned by a store. A new table in §7.1 lists all of them | §3.2.2, §7.1 |
| 7 | **The dependency register is rebuilt.** DE-4 and DE-9 close; DE-1 closes; DE-2 and DE-3 are **reclassified outside this engagement** rather than dropped; three are added, including the sandbox as a critical-path item | §9.2 |
| 8 | **The fallback is withdrawn.** There is no second place for the benefit schedule to live, so **InsureMO is a hard dependency** — RK-14 | §0.3, §9.2 |

**What did not change at v0.3: Parts IV, V and VIII, and the whole of the adjudication chapter.** The rule chain as it then stood, the two reduction mechanisms, the tree and its roll-up rules, the ledger, auto-approval's conditions, the two assessment tracks and the 13 lifecycle states were untouched by that reissue. **That is the result, not a coincidence.** The claim engine was specified against a *contract* for policy data rather than against a *component* (P-06), so changing who serves the contract changed the counterparty and nothing downstream of it. It is the clearest evidence available that contract-first was the right discipline — and it is also the half of P-06 that survives, because the other half, substitutability, did not.

### 0.2b What changed in v0.4

v0.4 is not an architecture change and it is not a review pass. It applies **ten decisions taken on 24 August 2026**, all recorded in the *Solution Paper — Policy Terms, Rule Engine and Limit Management v1.1* §11 and carried into *Solution Framework v2.4*. **The cause was a defect, and it is worth naming rather than absorbing quietly.**

**§5.3 step 4 — general exclusions — was a rule with no data behind it.** It read *"Per the policy wording — A5 §7"*, and A5 §7 is ten sentences of English prose. There is no predicate, no scope, no effect and no clause citation anywhere in the seed. **Nothing in the system could evaluate step 4.** A developer handed v0.3 would have hard-coded ten `if` statements against fields we never agreed to capture, or skipped the step. It is the fifth instance of the same defect class this project has found — a rule stated in prose with no evaluable data underneath — and the largest, because exclusions are the terms customers dispute.

| # | Change | Where |
|---|---|---|
| 1 | **The rule chain becomes fourteen steps.** **Benefit-level provisos** are inserted as a new **step 7**, evaluated by the term engine at `BENEFIT` scope; old steps 7–13 shift to 8–14. **The note that the order is configuration is withdrawn and replaced** — it was wrong, and it is corrected here rather than edited away | §5.3 |
| 2 | **Step 4 is rewritten against the term register** — claim-side master data keyed on `ruleSetRef`, with three term classes. **Class J and class E may never auto-decline** | §5.3 |
| 3 | **Six new business rules, `BR-TERM-1…6`**, for term evaluation: evaluate all not first-match, precedence, determinism, version binding, correction versus new version, and the `PAID`/`CLOSED` boundary. **A seventh, `BR-TERM-7`, was added by the same-day correction — §0.2c** | §5.3.1 |
| 4 | **The auto-approval gate gains a third condition — no open term checkpoint.** The straight-through rate falls, deliberately and measurably | §5.8 |
| 5 | **The counter period is defined for the first time — `L-1…L-4`.** Nothing in v0.3 said what *"annual"* meant operationally | §5.9.1 |
| 6 | **`BR-LEDGER-1…6` are restated to cover three counter types**, not money alone. This closes a live inconsistency: the chain tested a count limit no rule maintained and the console displayed *"remaining days"* no rule produced | §5.9.2 |
| 7 | **`BR-LEDGER-7`** — the complete posting table. `BR-LEDGER-4` covered one transition out of nine | §5.9.3 |
| 8 | **`BR-LEDGER-8`** — the approval race. The losing claim's retry must **re-adjudicate**, not re-post | §5.9.4 |
| 9 | **Mid-term benefit-level change**: consumption carries, the cap changes, the cap as at the posting is stored. **Conditional on C-IMO-14** | §5.9.5 |
| 10 | **Coordination of benefits across policies is declared out of scope — S-41, RK-15** | §10.1, Part XI |

**One of the ten is an admission rather than an addition, and it is item 6.** `benefit_schedule_line` carries `count_limit`, `per_day_cap` and `per_event_cap` on all 51 rows; chain step 9 tests *"days or visits remaining"*; D2's `S-11` limit screen displays *"days paid / remaining days"*. All three shipped in the specification. **`BR-LEDGER-1…6` governed money and nothing else**, so the chain tested a counter no rule maintained and the console showed a figure no rule produced. A screen presenting *"remaining days"* with no rule behind the number is worse than no screen, because an assessor will believe it.

**What did not change.** Parts I, II, III, IV, VII and VIII are untouched. `AR-1…AR-4` are untouched and the term-class model is the same posture extended from clinical assessment to policy terms — **the engine proposes, the platform's human decides**. The two reduction mechanisms, the benefit tree and its roll-up rules, the two assessment tracks and the 13 lifecycle states are unchanged.

### 0.2c Correction, 24 August 2026 — same day, same version

**Two modelling conflicts were found while the prototype was built against v0.4, and both were real: the documents disagreed with each other.** They are corrected here rather than carried to v0.5, because both were defects in what v0.4 issued that morning and neither changes a decision. **The version is deliberately not bumped**; the changelog carries them instead.

| # | The conflict | How it is settled | Where |
|---|---|---|---|
| 1 | **May a class-D term raise a checkpoint?** §5.3.1 as issued read as though the class decided the effect: class D declines, class J and E checkpoint. **A5 §7.1 is right and this document yields** — `termClass` and `effect` are **orthogonal axes**. `termClass` answers *can the system decide this?* and governs the auto-decline; `effect` answers *what happens when the term is in play*. Two of the pilot's ten terms select deterministically and resolve judgementally, and exclusion #3 is the case that proves it: `serviceCategory = SUPPLEMENT` selects deterministically, and the proviso *"not prescribed as treatment for the diagnosed condition"* means a prescribed supplement is covered | **The surviving rule is one-directional and is now stated in one direction: a class `J` or `E` term may never carry `DECLINE_EVENT` or `DECLINE_LINE`; a class `D` term may carry any effect, including `CHECKPOINT`** — `BR-TERM-7` | §5.3 step 4, §5.3.1, **`BR-TERM-7`** |
| 2 | **The `selector` / `predicate` split.** v0.4 gave `policy_term` a **single** `predicate`, *"null and not permitted for class J"*. **Applied literally the register is unbuildable**: with the one field forbidden on class J there is nothing left to decide *which* claims the term concerns, so every class-J term raises a checkpoint on every claim. D2 §5.28.2 specified the split and raised it as **`D2-173`**, correctly declining to fix its own upstream | **Adopted here.** `selector` decides whether the term is in play and is **mandatory and non-null on every class**; `predicate` decides the auto-decline, is **class `D` only**, and is mandatory only where a class-D term's effect is a decline. A5 §7 and `A5_seed_v1.4.json` adopt it in the same pass, so **`D2-173` closes** | §5.3.1, **`BR-TERM-7`** |

**Nothing else moved.** No step of the chain, no ledger rule, no gate condition, no state and no figure. Both corrections are to how a term is *described as data*, and the ten terms of the pilot product are unchanged in meaning by either of them.

### 0.2d What changed in v0.5

v0.5 is a **review pass**, and the review was of the document set against its own data rather than against itself. That distinction is what produced most of the findings: v0.4 stated rules in prose that the seed does not support, and a rule the data cannot satisfy is the same defect class §0.2b was written to close, one layer up.

**The single sentence that describes the pass:** *v0.4 built a mechanism and did not check that anything runs through it.* Step 7 was created and every term in the register is `PRODUCT` scope. A third gate condition was written for class J and class E and the pilot's checkpoints are mostly class D. A term register was made master data and given no version. The chain named a base amount for every percentage and never named the base *set of lines*.

| # | Change | Where |
|---|---|---|
| 1 | **Step 7 has no instance on the pilot product**, and now says so in the same terms step 13 already used. The register's ten terms are all `PRODUCT` scope. **Scope carries three values, not two** — `PRODUCT` · `BENEFIT` · `BENEFIT_SUBTREE` — with the roll-up semantics of the third stated here, where the rule belongs (**BR-TERM-8**). A real proviso is asked of the business as **B-23** | §5.3, §5.3.1 |
| 2 | **The auto-approval gate is rewritten and gains a fourth condition.** Condition 3 no longer names a class — *any* open checkpoint blocks, because two of the pilot's checkpoints are class D. Condition 4 is **no outstanding duplicate signal**, without which `BR-DUP-2`'s warning is inert on exactly the population that settles unreviewed | §5.8 |
| 3 | **A term may test only facts that exist at the step it runs at** (**BR-TERM-9**). A `PRODUCT`-scope term may not test `benefitCode`; one pilot term does, and A5 v1.5 corrects it | §5.3.1 |
| 4 | **An open checkpoint blocks the submission of a settlement plan**, not only the automatic route (**BR-TERM-13**). This **ratifies and closes D2-177** | §5.3.1 |
| 5 | **A step that cannot be evaluated is never a pass** (**BR-CHAIN-1**) — a new one-rule series covering every present and future fact gap | §5.3 |
| 6 | **The term register is versioned and bound at registration, onto the snapshot** — `ruleSetRef` alone does not identify what a claim was evaluated against, so `BR-TERM-3`'s determinism claim was false as written and `BR-TERM-5` was unimplementable. **BR-TERM-3 and BR-TERM-4 are restated**; §7.1 gains the row | §5.3.1, §7.1 |
| 7 | **Five new ledger rules, `BR-LEDGER-9…13`**, and a sixth, **`BR-LEDGER-14`**, defining the **visit** as a derived unit so `OCCURRENCE` can enforce a visit limit at all. Three of the six close defects that release money or a limit incorrectly | §5.9.2, §5.9.3, §5.9.4 |
| 8 | **A granularity column on the §5.3 table.** The chain named the base *amount* for every percentage and never the base *set of lines*, and granularity varies by step | §5.3 |
| 9 | **`CAP` is defined rather than dropped** (**BR-TERM-12**), and **`REQUIRE_DOC` is given a mechanism** (**BR-TERM-11**). Both were enum values with no behaviour behind them | §5.3.1 |
| 10 | **A term whose `clauseRef` is provisional may not decline** (**BR-TERM-10**). All ten pilot terms carry a `TBC-` reference, so step 4's auto-decline branch cannot lawfully fire today and the document now says so | §5.3 step 4, §5.3.1 |
| 11 | **Seven business decisions are opened, `B-18 … B-24`, and `B-01` and `B-07` are re-opened.** Each is a decision this document is not entitled to take — pro-ration, aggregation, allocation order, cumulative exposure, retention, a real proviso, a document type, product substitution and staffing | Part XI |

**Two things a reviewer should notice about this list.** First, **nothing in it changes a decision taken on 24 August** — every one of the ten stands, and most of the work here is making them true against the data. Second, **five of the eleven are money-affecting**: the gate's two new clauses, the ledger's release rules, the unevaluable-step rule and the visit definition all change what a claim pays or who sees it before it pays.

### 0.3 Status of this draft

**v0.5 is complete in structure and content and is baselined against *Solution Framework v2.5*.** It takes no new capability and opens no new dependency. **What it opens is nine business decisions** (Part XI), of which **two — B-19 and B-21 — change money on the pilot product** and should be taken before the first end-to-end settlement is demonstrated. None of the nine blocks build: each has a stated interim behaviour that is safe, and each is a choice between two defensible rules rather than a gap with no answer.

**The v0.4 statement below stands, with one correction.** The reissue is baselined against *Solution Framework v2.5* rather than v2.4, and the annex versions in the header line move with it.

**v0.4 is complete in structure and content and is baselined against *Solution Framework v2.4*.** It adds one capability — **terms as data** (Group G, F65–F68) — and closes the largest specification defect this project has found: **a rule chain step with no evaluable data behind it**. Three new dependencies are opened with it and **none of them blocks build**: **DE-13** (C-IMO-14, whether a mid-term endorsement is visible to the snapshot) and **DE-14** (Q-IDP-05 and Q-IDP-06, facility country and a treatment-day count). Each degrades a term or a counter to a human rather than stopping one.

**The v0.3 statement below stands unchanged, and is retained because it is the position this reissue builds on.**

**v0.3 is complete in structure and content, and the three places v0.2 was deliberately unconfirmed have all moved.** InsureMO answered on 21 August: the capability questions are closed and what remains open is **contract detail** — request and response schemas (C-IMO-03), the policy-issued integration style (C-IMO-04), effective-dating behaviour (C-IMO-05) — plus a sandbox (C-IMO-06) and one new question, the bulk member load (C-IMO-10). The IDP team confirmed **every** capability in writing, including the clinical appropriateness assessment and the versioned knowledge base. **What was confirmed is capability.** Effort, timeline, delivery scheduling and sourcing are outside this engagement's scope, and the items of that kind are reclassified in §9.2 rather than left open against this document. Of the business decisions in Part XI, **fifteen of seventeen are closed**; two remain, and neither blocks design or build. *(That count was true at v0.3 and is retained as part of the v0.3 statement. **The live count is at Part XI**: twenty-four items, thirteen closed, eleven open at v0.5.)*

**One paragraph of v0.2 is withdrawn outright, and it is the one a reader should be told about.** v0.2 said J2 carried *"almost none of the InsureMO dependency — InsureMO touches six features in total (F01–F06), of which only F06 sits in J2 and nothing in J2 waits on it."* **That is no longer true, and it was the reassurance that made working ahead of the confirmation safe.**

InsureMO now touches **seven** features (F01–F07) and, far more to the point, it holds **the benefit schedule and the member register that every claim reads**. J2 opens with a call to InsureMO — coverage and the benefit schedule as at the treatment date (I-05, **F07**) — and a claim cannot be registered, snapshotted or adjudicated without it. **There is no fallback**: the schedule used to have two possible homes and now has one, so if InsureMO cannot serve the contract as demonstrated, the remedy is a build rather than a swap (**RK-14**, mitigated only by hands-on evaluation of the sandbox — C-IMO-06).

The buy journey (J1) is still specified at lower resolution than the claim journey (J2), and that is still intentional: J2 carries the great majority of the scope. The reason has simply changed from *"J2 barely touches the vendor"* to *"J1 is deliberately thin"*.

### 0.4 How the reference material was used

Two mature Vietnamese claim systems were studied in depth: a non-life insurer's health claim system (**Reference System A** — requirement specifications for intake and adjudication, a benefit-formula configuration specification, a document checklist of 27 types at intake, expanding to 42 rows across seven sections in adjudication and a working clickable prototype), and a people-insurance claim and direct-billing system (**Reference System B**). A third, a customer mobile claim application (**Reference System C**), was reviewed for the customer journey.

**They were used as evidence of what a working system must handle, not as designs to copy.** Three rules governed the use:

1. **Structure is learned; content is not carried across.** Where the reference demonstrates that a benefit schedule must be a tree, we adopt the tree. We do not adopt its benefit codes, product names or figures — A5 is an original construction.
2. **A simplification against the reference is declared, not silent.** Framework §7.4 is the register — it now carries **forty-one, S-01…S-41**: S-01…S-25 from the original set and the gap analysis against Reference System B, S-26…S-38 from the difference register against Reference System A, and **S-39, S-40 and S-41 added at Framework v2.4** by the decisions of 24 August. §10.1 of this document argues S-09…S-16 with the same discipline.
3. **A gap in the reference is stated as a gap.** Reference System A has no duplicate-claim detection, no "policy not found" path and no handling of physical originals. Those are not features to inherit; they are requirements this system must add, and §10.2 records them.

No client is named in this document. Vietnamese business terminology is used throughout because it is the domain's vocabulary, not because it identifies anyone.

> **On verifiability.** Statements in this document about what a reference system does are drawn from specifications held under separate client confidentiality. They cannot be re-checked from within this document set, and a reviewer should read them as reported evidence rather than as citations. Where such a statement carries a design decision, the decision is justified on its own merits in the same paragraph — so that if the reported behaviour turns out to be wrong, the argument still stands or falls visibly.

---

## Part I — Business context and objectives

### 1.1 The problem

A Vietnamese health insurance customer today buys through an agent and claims on paper. They assemble a folder of hospital documents, deliver or post it, and then wait — typically weeks — with no visibility. On the insurer's side, a claim handler re-keys the same documents into a claim system by hand, looks up the benefit schedule in a separate system, computes the payable amount in a spreadsheet or in their head, and writes the settlement letter.

Both sides of that are addressable with technology that now exists. Neither is addressed by digitising the form: a PDF claim form emailed instead of posted still arrives as unstructured data.

### 1.2 The proposition

**A customer buys cover on their phone in minutes, and claims on it by photographing the documents they already hold.** The insurer's staff receive a claim that has already been read, structured, checked against the policy and priced — and in the simplest cases, settled without a human touching it.

### 1.3 Business objectives

| # | Objective | Measured by |
|---|---|---|
| BO-1 | Remove manual re-keying from the claim process | SC-01: ≥80% of mandatory claim fields populated without human keying |
| BO-2 | Make the extraction accurate enough to adjudicate on, not merely to read | SC-02: ≥95% on monetary amounts and dates, ≥90% on medical text |
| BO-3 | Compute every settlement from configured product data, never by hand | SC-03: 100% of claims receive a system-computed payable amount per benefit |
| BO-4 | Settle simple claims within one working day | SC-04: median submit → approved ≤ 1 working day for complete outpatient claims |
| BO-5 | Give the customer visibility they do not have today | SC-05: status, information requests and outcome all in-app |
| BO-6 | Prove the architecture is reusable, not a one-off | SC-06: a second product configurable with no code change |
| BO-7 | Prove the buy journey is genuinely self-service | SC-07: quote → certificate in under 5 minutes, unassisted |
| BO-8 | Prove the two journeys are one system, not two demos | SC-08: a policy bought in J1 claimable in J2 the same day |

**BO-3 is the one that distinguishes this from a document-processing pilot.** Reading documents accurately is necessary and not sufficient; a system that extracts perfectly and then asks a human what to pay has automated the typing and left the work. The benefit engine is the product.

### 1.4 The one architectural difference that defines this system

Everything else in this document follows from a single decision, and it is the decision that separates this system from the mature Vietnamese claim systems studied. It is worth stating before the requirements rather than discovering it in Part V.

**How Reference System A works.** Documents are pushed to an external OCR/AI partner. **The partner does the work there** — classification, extraction, and the benefit assessment itself. The claim system then **synchronises the result back**, and displays it. Every amount that matters on its assessment screen is marked *"Readonly, không cho phép NSD điều chỉnh"*: the claimed amount, the in-scope amount, the deduction ratio, the disability rate, and the payable amount. The claim system's remaining job is to validate the result against limits, aggregate it, split it by co-insurance and reinsurance, and route it for approval.

**How this system works.** Documents go to IDP to have data **read out of them**. IDP returns that data **through an API**, as a proposal. The Claim Platform pulls it in, and **everything after that happens in the Claim Platform**: review, correction, coverage assessment, benefit calculation, the settlement plan, approval, and payment.

| | Reference System A | This system |
|---|---|---|
| What the document service produces | A **decision** — the payable amount | **Data** — fields, cost lines, categories, confidence |
| How it reaches the claim system | A **sync** that overwrites claim rows | A **pull** the claim system performs once, into its own copy |
| Where the amount is computed | In the partner's engine | In the Claim Platform's benefit engine, from configured product data |
| Where an assessor corrects a value | Cannot — the fields are read-only | On the claim, always |
| What happens when a document is re-read | The sync **deletes** claim rows deleted upstream | A **new** extraction request is created. Nothing existing is mutated |
| Who can explain a settlement | Nobody inside the insurer | The Claim Platform, rule by rule (P-08) |

**Four requirements follow, and they are load-bearing:**

| # | Requirement |
|---|---|
| **AR-1** | **IDP proposes; the Claim Platform decides.** No decision, no amount and no coverage judgement originates outside the Claim Platform |
| **AR-2** | **IDP holds no claim state and never writes into claim data.** It has no notion of a claim's status, and there is no path by which it can change one |
| **AR-3** | **There is no synchronisation.** The extraction result is pulled once into the claim's own copy, and the two diverge from that moment. Re-reading a document creates a new extraction request; it never mutates an existing one, and it never removes anything from a claim |
| **AR-4** | **Every assessor action happens on the claim.** Correction, coverage assessment, line-level decline, confirmation of a clinical recommendation, the settlement plan and the approval are all Claim Platform functions and all audited there |

**Why this matters commercially, not just technically.** A document-reading service that also decides what to pay *is* the benefit engine, whatever the contract calls it. When the product changes, the vendor changes it. When a settlement is wrong, nobody in the insurer can say why. And the insurer cannot configure a second product without a vendor release — which is BO-6 and SC-06, failed. **AR-1 to AR-4 are what make the benefit engine an asset the insurer owns.**

> **v0.3: AR-1 … AR-4 are untouched by the deletion of the Policy Data Store, and they come out of it stronger.** They were only ever about the boundary with **IDP** — a document service that also decides what to pay is the benefit engine, whatever the contract calls it — and nothing in this reissue touches that boundary. What changed around them is that the same reasoning has now been applied to a second vendor and reached the same answer: InsureMO offers claim capability, and the client has decided not to use it (**B-17**). The claim platform is built entirely by us. **AR-1 to AR-4 are therefore no longer a rule about one supplier; they are the rule this solution applies to every supplier**, and they are the reason the benefit engine remains an asset the insurer owns even though the benefit *schedule* now sits in a vendor's system.

**Why AR-3 is stated so bluntly.** Reference System A's sync *deletes* claim rows that were deleted upstream — *"trường hợp các dòng thông tin bị xóa bên HT OCR, khi đồng bộ cũng sẽ bị xóa trên HSBT"*. That is a live hazard: a re-read while a claim sits with an approver silently changes the settlement being approved. Our design removes the hazard by removing the mechanism, rather than by locking around it.

### 1.5 Scope boundary in one paragraph

MVP 1 covers **one individual health product with three plan tiers, sold to individuals online and loaded in bulk for groups, claimed by reimbursement**. It excludes cashless/direct billing, reserving, co-insurance, reinsurance, recovery, multi-level approval authority, endorsements, renewals, and death/disability/critical-illness claims. Framework §7.2 is the authoritative list; §10 of this document explains the reasoning behind the ones a reviewer is most likely to challenge.

---

## Part II — Actors and organisation

### 2.1 Actors

| Actor | Who they are | What they need from the system |
|---|---|---|
| **Customer** | Policyholder or insured member, on a phone | To buy cover, understand what they are covered for, claim without a paper folder, and know where their claim stands |
| **Assessor** (*Cán bộ thẩm định*) | Claim handling staff, on a web console | To see a claim that is already read and priced, correct what the machine got wrong, decide coverage, and explain the decision |
| **Approver** (*Cán bộ phê duyệt*) | Staff with settlement authority | To review an assessor's settlement plan and approve or return it — without being able to alter it |
| **Administrator** | System and master-data staff | To configure the product, the thresholds, the document types and the users |
| **IDP** | The document intelligence platform | — (system actor; proposes, never decides) |
| **InsureMO** | The product, policy and benefit-schedule platform | — (system actor; provides quote and issue services, holds the policy, member and coverage register, and answers the benefit schedule as at a date). **Used for the policy side only** — B-17 |
| **Finance** | Payment execution | — (system actor; stub in MVP 1) |

> **Three systems, and only one of them is ours.** The Claim Platform is built by us; **BnK IDP** and **InsureMO** are platforms we consume. A fourth actor appeared in every version of this document up to v0.2 — the **Policy Data Store**, a minimal policy service we would have built to stand in for a core the pilot does not have. **It is not built** (Framework v2.0 §2.1): InsureMO demonstrated it can carry the adjudicable benefit schedule, so the thing the store existed to hold has a home. What it would have held is split on one rule — **policy business goes to InsureMO** (the benefit schedule, the policy, member and coverage register, the as-at-a-date query: F05, F07), **claim business comes to us** (the payout account, the payee, the group bulk load: F58, F59). A payout account exists to reimburse a claim; a payee is a party to a settlement rather than to a contract of insurance; and a loader that exists so claims can be demonstrated on group cover is a claim-side tool whatever it writes into.

Four human roles. A3 is the authoritative permission matrix. **Two structural rules matter at business level and are stated here because they are business controls, not technical ones:**

- **An assessor cannot approve a claim they assessed.** This is the only control on the amount paid.
- **No one may assess or approve a claim in which they are the insured, the claimant or the beneficiary** (A3 §4.12). The check is automatic on ID number, and any other relationship must be declared by the user. Reference System A splits the control the same way, and the split is the point: an automatic check catches the obvious case and nothing else, while a declaration alone is unenforceable. Note this binds **assessors as well as approvers** — an assessor who prices their own claim has already decided it.

### 2.2 Organisational simplification

Reference System A routes every claim through a **unit hierarchy** — a receiving unit, a handling unit derived from an authority check at intake, and an escalation to head office when a branch's authority is exceeded. It also runs a weighted work-allocation engine that assigns a handling officer automatically, filtered by business line, active status and job grade.

**MVP 1 has no unit hierarchy and no work-allocation engine.** There is one queue, and assessors take work from it or have it assigned. This is a declared simplification (§10.1, S-11) and it is safe only because MVP 1 is a pilot with a single team. It is also the simplification most likely to need reversing first: the moment a second branch is involved, both the data-scope model in A3 §0.3 and claim assignment (F43) change.

---

## Part III — Business processes

### 3.1 J1 — Buying an individual health policy

| # | Step | Actor | What happens |
|---|---|---|---|
| J1-1 | Browse | Customer | Sees the product, three plan tiers, benefits and limits per tier. No login required |
| J1-2 | Quote | Customer | Enters age and plan; sees the premium. Quote has a stated validity date |
| J1-3 | Apply | Customer | Personal details, insured members, a short health declaration |
| J1-4 | Eligibility screen | Claim Platform | Age band and declaration answers are evaluated immediately in the app so the customer is not made to complete an application that cannot succeed — **and re-evaluated server-side at submission, which is the enforcement point** (A3 §6). A disqualifying answer declines the application **with a route to a human**, never a dead end (A2 `POL-VAL-002`) |
| J1-5 | Pay | Customer | Mock payment. Boundary condition BC-3 — no real money in MVP 1 |
| J1-6 | Issue | InsureMO | Application submitted for issuance; policy number, period and plan returned |
| J1-7 | Register | InsureMO | The policy, its members and its benefit schedule are held in **InsureMO's policy, member and coverage register** (F05) and notified to the Claim Platform (I-04). **The Claim Platform keeps no working copy** — it reads them at claim registration and snapshots them there (§7.2). A cached copy would have to be kept in step with InsureMO, and a copy that drifts is worse than none: it is a second answer to a question with one right answer |
| J1-8 | Confirm | Customer | Certificate, and a "my cover" view showing benefits, limits and **waiting periods** |

**Two business rules that come from the reference and are easy to get wrong:**

- **A quote must either still hold or be visibly expired.** Silently repricing at issuance is the single most common trust failure in an online insurance journey. `validUntil` is a contract, not a hint (D3 §I-02).
- **The certificate email carries the waiting-period table** (A2 §7.3). Waiting periods are the largest single cause of a surprised, disputed claim. The cheapest moment to set that expectation is the day of purchase, not the day of decline.

**Group business does not use J1.** Group policies and member lists are loaded by Excel import (§3.3). Group underwriting is out of scope.

### 3.2 J2 — Making a reimbursement claim

This is the core process. It is specified in three phases: submission, processing, settlement.

#### Phase 1 — Submission (customer)

| # | Step | Actor | Business rules |
|---|---|---|---|
| J2-1 | Start a claim | Customer | Selects the insured member — themselves or a dependant on the same policy. The member list comes from their own cover only |
| J2-2 | Describe the episode | Customer | Episode type, treatment dates, provider, diagnosis if known, amount claimed |
| J2-3 | **Date validation** | Claim Platform | The full chain must hold — see §3.2.1. This is checked at capture, not at assessment |
| J2-4 | Capture documents | Customer | Guided per document type (A4). Quality hints at capture (A2 §3.2) |
| J2-5 | Payout account | Customer | Bank, account number, then **account-name verification** — see §3.2.2 |
| J2-6 | Submit | Claim Platform | Claim moves to `SUBMITTED`. **No claim number yet** — see J2-7 |

#### 3.2.1 The date validation chain

Reference System A validates treatment dates as a **chain of inequalities against the member's own cover period**, not the policy's. We adopt it in full:

```
eventDate  ≤  treatmentFrom  ≤  treatmentTo  ≤  today
                  and
coverageFrom  ≤  treatmentFrom  and  treatmentTo  ≤  coverageTo
                  and
treatmentTo  ≤  requestReceivedDate  ≤  today
```

**Why the member's period and not the policy's.** On a group policy, members join and leave mid-term. Policy-level dates are wrong for most claims on a group scheme, and using them either admits claims for people who had left or rejects claims for people who had joined. **InsureMO returns `coveragePeriod` per member precisely for this** (F05, R-IMO-10; D3 §I-05) — confirmed 21/08/2026 with the register, and it is one of the things C-IMO-01c had to establish before group cover was representable at all.

**Why the whole chain and not just the cover test.** Chained inequalities catch transposed and swapped dates — the commonest data-entry and extraction error class — at capture, when they cost the customer ten seconds, rather than at assessment, when they have already consumed an assessor's attention.

#### 3.2.2 Payout account verification

| Rule | Statement |
|---|---|
| **BR-PAY-1** | The account holder's name is **never typed**. It is populated only by the verification call |
| **BR-PAY-2** | If the account number is subsequently changed, the verified name is **cleared** and verification must run again |
| **BR-PAY-3** | A claim cannot be submitted for approval, and a payment cannot be instructed, unless `accountNameVerified` is true |
| **BR-PAY-4** | The customer may reuse a payee proven on a **previously paid** claim for the same insured, matched on name + date of birth + ID number |

**Why this is a business rule and not a UI nicety.** A misdirected payment is the most expensive and least recoverable error a claim system makes, and it is the primary vector for payment-redirection fraud: submit a clean claim, change the account after approval. BR-PAY-2 is what closes that specific attack, and A3 §4.5 adds the control that a post-approval account change requires an approver to re-release the payment.

> **v0.3 — where the payout account now lives, and the one thing this reissue broke.** The payout account and the payee are **Claim Platform master data (F58)**. They are not returned by the policy read: I-05 no longer carries `payoutAccount` or `beneficiary` (D3 v2.3 §I-05), so a claim assembles them from **our** records in a second read at registration. That is the right home — a payout account exists to reimburse a claim, and account-name verification is a claim-time control on a claim-time object, not a term of the contract.
>
> **But BR-PAY-1 now names a call that does not exist.** "Populated only by the verification call" was written when `accountNameVerified` arrived as a field on a policy read, and the store's admin screen was where it was set. With F58 the flag is ours to set, and **no interface in D3 defines the verification itself** — there is no counterparty, no request, no response and no error semantics (D2 open item **D2-106**). Until that contract exists, BR-PAY-1 states an intent rather than a buildable rule, and **BR-PAY-3 is enforceable only in the negative**: the console refuses to submit a settlement plan and I-11 refuses an instruction while the flag is `false`, which is correct behaviour and is also, today, the only behaviour. It is carried as dependency **DE-12** and it is the smallest of this reissue's open items with the largest consequence per occurrence — every settlement passes through it.
>
> **A3 §4.16** carries the matching control: an assessor or an administrator runs the verification; a customer supplies an account and can never assert it verified; an approver can do neither.

#### Phase 2 — Processing (system, then assessor)

| # | Step | Actor | Business rules |
|---|---|---|---|
| J2-7 | Registration and coverage validation | Claim Platform, reading **InsureMO** | Member covered on the service date? Policy active? The member, the policy and the **benefit schedule as at the treatment date, as a tree** are read from InsureMO in **one consistent read** (I-05, **F07**). **The Claim Platform derives the coverage verdict itself** — InsureMO supplies the facts, we draw the conclusion (P-01, §1.4). **The claim number is allocated here** (F41), once, and never changes. If coverage fails, the claim does not proceed to extraction — see §3.2.3 |
| J2-8 | **Policy snapshot** | Claim Platform | Benefit schedule as at the service date, plus policy and member data as they stand at registration, copied onto the claim and **frozen**. **This is the step that keeps a settled claim reproducible** — see §7.2, which v0.3 sharpens rather than restates |
| J2-8a | **Payee and payout account** | Claim Platform | Read from **our own** master (**F58**), not from the policy read, and pinned onto the claim alongside the snapshot. New at v0.3 as a distinct step, because it is a **second read of a second system** and specifying it as part of J2-7 would hide that |
| J2-9 | Submit to IDP | Claim Platform | Document package by reference, with claim context |
| J2-10 | Classify, extract, reconcile | IDP | Per A4 |
| J2-11 | Checklist reconciliation | Claim Platform | Package completeness state computed; missing required documents trigger an information request |
| J2-12 | Duplicate check | Claim Platform | See §3.2.4 |
| J2-13 | Adjudicate | Claim Platform | The rule chain in Part V. Produces a payable amount per benefit and per cost line |
| J2-13a | **Clinical appropriateness assessment** | IDP | Runs concurrently with J2-13 over the same lines (I-15/I-16). Returns **recommendations**, never decisions — §5.10 |
| J2-14 | Auto-approval test | Claim Platform | **Four conditions** (§5.8) from v0.5. If all four hold, the claim approves without an assessor |
| J2-15 | Assessment | Assessor | Reviews extraction, confirms coverage, adjusts, explains — §4 |
| J2-16 | Submit settlement plan | Assessor | The claim **locks** — see §3.2.5 |

#### 3.2.3 When coverage validation fails

Reference System A has no defined path for "no policy found": the policy number is mandatory, so the claim simply cannot be saved, and the officer's only options are to reject or cancel. **That is a gap and we close it.**

| Outcome | State | What happens |
|---|---|---|
| Member covered | proceed | Normal path |
| Member found, **not covered on the service date** | `UNDER_ASSESSMENT` | Proceeds to an assessor with the coverage reason attached (`PERIOD`). It is **not auto-declined** — the dates may be wrong, or an endorsement may not have loaded |
| **No member matches** | `PENDING_INFO` | The customer is asked to check the policy number and ID. The claim is **not** declined and **not** left in limbo |
| Several members match | `UNDER_ASSESSMENT` | The assessor picks. **The Claim Platform never auto-selects a policy** — Reference System A is explicit about this and it is the right call |

**A failure to *find* a policy is never a decline.** It is a data problem, and conflating the two produces declines that cannot be explained to a customer — hence the `PENDING_INFO` path above rather than an automatic rejection.

**A failure to *satisfy* cover is different**: it is a contractual answer, and the engine computes it. Such a claim always reaches a human before it settles. A coverage failure declines a line or reduces the payable, and §5.8's condition 1 keeps any claim where something was cut outside the auto-approval gate — so the contractual answer is seen before it becomes a settlement.

#### 3.2.4 Duplicate claim detection

Reference System A has **none** — its bulk-PDF intake will create two claims from the same folder scanned twice, and nothing detects it. This is the most significant omission found in the reference material, and it is a requirement here.

| Rule | Statement |
|---|---|
| **BR-DUP-1** | On submission, the Claim Platform searches for an existing non-cancelled claim with the **same insured member, same provider and overlapping treatment dates** |
| **BR-DUP-2** | A match raises a **warning to the assessor**, naming the other claim number. It does not block the customer |
| **BR-DUP-3** | Where an invoice number has been extracted, a second claim carrying the **same invoice number** is a stronger signal and is flagged as such |
| **BR-DUP-4** | The customer is never told a duplicate was detected. It is an internal control |

BR-DUP-2 is deliberate. A genuine second claim for the same episode is common — a follow-up visit, a supplementary invoice — so blocking would be wrong. The signal belongs to the assessor.

> **And that is precisely why the gate must test it — corrected at v0.5.** *"A warning to the assessor"* is the right control only where a claim reaches an assessor. An auto-approved claim never does, so **on the one population that settles unreviewed, the entire duplicate control was inert**: BR-DUP-1 would fire, BR-DUP-3 would fire, and the claim would settle with the warning addressed to nobody. It is the same shape of defect as a screen showing *"remaining days"* with no rule behind the number (§5.9.2) — a control that exists and is never delivered is worse than one that is declared missing, because it is counted as present. **§5.8's condition 4 closes it**: an outstanding duplicate signal under BR-DUP-1 or BR-DUP-3 keeps the claim out of the automatic route, where the assessor BR-DUP-2 addresses is standing.

#### 3.2.5 The claim lock

**On submission for approval, the entire claim becomes read-only** — amounts, coverage decisions, document actions, and the ability to submit a fresh extraction request alike — until an approver acts.

Reference System A locks explicitly and comprehensively, and the reason is worth stating: without it, an assessor can alter amounts or swap attachments while the claim sits in the approver's queue, and **the approver signs a different claim from the one they reviewed**. Note that the lock must cover *document* actions too, not only amounts: a re-run of extraction can delete lines, which changes the settlement under the approver.

The lock releases on rejection, or on approval. It is never released by the assessor.

#### Phase 3 — Settlement

| # | Step | Actor | Business rules |
|---|---|---|---|
| J2-17 | Approve or return | Approver | Cannot edit. Returning requires a reason and restores the **previous** state — see §4.4 |
| J2-18 | Ledger finalisation | Claim Platform | Consumption becomes final **at approval**, not at payment (§5.9) |
| J2-19 | Payment instruction | Claim Platform | Only for an approved claim, the approved amount, and only where `accountNameVerified` is true |
| J2-20 | Payment confirmation | Finance | `PAID`, `REJECTED` or `RETURNED` |
| J2-21 | Settlement notice | Claim Platform | Benefit-by-benefit, with declined lines shown in the same table (A2 §7.1) |
| J2-22 | Closure | Claim Platform | **Gated on payment having been confirmed**, not on approval — see §4.5 |

### 3.3 Group policy loading

Group business exists in MVP 1 so that claims can be demonstrated on group cover, without building group underwriting.

**The loader is ours — F59.** It was F13 of the Policy Data Store; it comes to the Claim Platform because it exists to make claims demonstrable, and a tool built for that reason is claim business whatever it writes into. An imported policy references the same product catalogue as the individually-sold one, so a group claim and an individual claim adjudicate through the same engine against the same schedule.

> **Where the imported members are written is open — C-IMO-10, and it is the one open item in this Part.** If InsureMO exposes a bulk member load, F59 writes there and there is **one** member list. If it does not, F59 maintains a **claim-side member extension** and group business has **two** — one in InsureMO for policies it issued, one in ours for policies we loaded — and every group claim then has to decide which to believe. The rules below hold in both branches, because all five are properties of the *load*, not of its destination. What the second branch adds is a permanent reconciliation obligation and a rule saying which list wins: **the policy's `source` decides it** — a policy we loaded is answered from our extension, a policy InsureMO issued is answered from InsureMO, and no policy is ever answered from both. **That should be taken as a decision, not discovered during build.** A3 §3.2.3 carries the matching permission question as A3-07.

| Rule | Statement |
|---|---|
| **BR-IMP-1** | Import is two-phase: validate, human review, then commit. Nothing is written before commit |
| **BR-IMP-2** | **No partial import.** Any error blocks the whole commit. A half-loaded group policy is worse than none, because claims will be assessed against it |
| **BR-IMP-3** | An unknown plan code is an **error**, not an auto-create — otherwise an importer invents plan codes and the benefit schedule silently does not exist |
| **BR-IMP-4** | Re-importing is **scoped to the benefit level being loaded**. It replaces the member list *for that level only*, and members absent from the new file have their coverage **end-dated, never deleted** — a departed member's past claims must stay assessable |
| **BR-IMP-5** | One benefit level per import run. A policy with three levels is three runs, and BR-IMP-4's scoping is what stops the second run erasing the first |

---

## Part IV — The claim lifecycle

### 4.1 States

13 states, one machine. A1 §5.1 is the authoritative list with the Vietnamese labels; it is not repeated here.

```
DRAFT ──► SUBMITTED ──► DOC_PROCESSING ──► UNDER_ASSESSMENT ──► PENDING_APPROVAL
                             │  ▲    │              ▲   │              │
                             │  └─ EXTRACTION_ERROR │   │              │  return
                             ▼                      │   │              │  (restores the
                        PENDING_INFO ───────────────┘   │              │   prior state)
                             ▲   └──────────────────────────────────►  │
                             └──────────── return ─────────────────────┘
                                          │                             │
              auto-approval (§5.8) ───────┘         ┌───────────────────┴───────────────┐
                     │                              ▼                                   ▼
                     └────────────────────────► APPROVED                            DECLINED
                                                    │  ▲                                │
                                                    │  └── override (BR-AUTO-4)          │
                                        PAYMENT_IN_PROGRESS ──► PAID ──► CLOSED ◄───────┘
                                                    │
                                                    └── payment rejected or returned ─────►
                                                        UNDER_ASSESSMENT  (§4.5, §5.9.3)

CANCELLED — reachable from DRAFT, SUBMITTED, DOC_PROCESSING, PENDING_INFO,
            EXTRACTION_ERROR and UNDER_ASSESSMENT. Not from PENDING_APPROVAL
            or later (§4.6).
```

**The payment-failure edge was missing from this diagram to v0.4 and is drawn at v0.5.** §4.5 has described it since v0.1 — *"a rejected or returned payment does not close the claim; it returns to the assessor with the reason"* — and §5.9.3 posts against it. A transition described in prose and absent from the picture is a transition somebody builds and somebody else does not.

Two edges are not drawn above because they bypass the assessor entirely, and both carry rules:

- **`DOC_PROCESSING → APPROVED`** — the auto-approval path (§5.8). A claim that meets **all four** conditions never enters `UNDER_ASSESSMENT`, and the transition records the conditions it was evaluated against (BR-AUTO-5).
- **`APPROVED → UNDER_ASSESSMENT`** — an approver overriding an auto-approval (BR-AUTO-4). Available **only** on a claim that was auto-approved and **only** before a payment instruction has been issued. It is not a general re-open: S-13 still holds, and a claim an approver approved by hand cannot be walked back this way.

Three further edges are easy to miss and all three carry rules:

- **`PENDING_INFO → PENDING_APPROVAL`** exists. A claim can be assessed and submitted while a non-blocking document is still outstanding; returning it must put it back in `PENDING_INFO` (§4.4).
- **`EXTRACTION_ERROR` is a branch off `DOC_PROCESSING` only** — it is a processing failure, never reachable from a state where the customer owes us something.
- **`DECLINED → CLOSED`** is a real edge. A declined claim never produces a payment, so it closes on approval of the decline rather than on cash (§4.5).

### 4.2 One machine, not two — and what we lose

Reference System A runs **two orthogonal state machines** on the claim file: a coarse lifecycle status (`Trạng thái HSBT` — three values: *Đang xử lý / Đã đóng / Đã hủy*) and a rich handling sub-status (`Tình trạng GQHS` — eighteen or more values covering document waits, calculation, four separate approval rounds, payment, and three distinct recovery waits). Its *intake* record carries a third, separate five-value status.

**We collapse them into one 13-state machine, and this is a declared simplification (S-01).** The collapse is safe because most of the reference's sub-statuses describe workflows we have excluded — guarantee approval, reserve approval rounds, co-insurance recovery, third-party recovery, advance repayment. What we must not lose is the property in §4.4.

### 4.3 Operational states are not decisions

`EXTRACTION_ERROR`, a stalled `DOC_PROCESSING`, and a stalled `PAYMENT_IN_PROGRESS` are **operational states with an operator queue**. None of them is ever expressed to the customer as a claim outcome, and none of them ever becomes `DECLINED`.

**A claim declined because a service was down is indistinguishable from a correct decline** unless the states are kept apart. This is the single most important rule in the lifecycle, and it is why A2 rule M-6 forbids phrasing any system failure as a decision about the claim.

### 4.4 Returning a claim restores the previous state

When an approver returns a claim, it does **not** go to a fixed state. It returns to **whatever state it was in before it was submitted for approval**.

Reference System A is emphatic about this — every rejection path reads *"trở về tình trạng trước khi trình phương án"* — and the reason is concrete. A claim that was waiting on customer documents when it was submitted must go back to waiting on customer documents, not jump into the assessor's active worklist ahead of claims that are genuinely ready.

**Requirement:** the state at the moment of submission is stored on the claim, and restored on return.

### 4.5 Closure is gated on money having moved

A claim closes when the payment is **confirmed**, not when it is approved.

Reference System A gates closure on `Ngày thanh toán ≠ null` — a date supplied by the accounting system. Treating "approved" as "closed" produces a reported claim population that diverges from the cash ledger.

**Three consequences to specify:**

- A **fully declined** claim closes on approval of the decline — there is no cash to wait for. A decline is a decision that needs approving, not an absence of one, and closing declined claims without that leaves the most disputable decisions the least audited. **This is why the auto-approval gate tests for more than an amount**: a wholly declined claim has a payable of zero and satisfies an amount condition trivially, so §5.8's condition 1 was narrowed on 17/08/2026 to require that nothing was cut. A decline cannot auto-approve.
- A **partially declined** claim follows the payment path: it closes on confirmation of the amount that was payable.
- A **rejected or returned payment** does not close the claim. It returns to the assessor with the reason, and a reversal is posted to the ledger (§5.9).

### 4.6 Cancellation

`CANCELLED` is reachable only **before approval**: by the customer up to `UNDER_ASSESSMENT` (A3 §1), and by an assessor or approver thereafter but **not from `PENDING_APPROVAL` or later** (A3 §2.1). A reason is mandatory. A claim opened in error against the wrong member has no other exit, which is why staff hold the function at all. Once anything has been approved the claim cannot be cancelled — it must be settled or declined.

Reference System A handles the post-approval unwind by adding two coverage values that mean *"wrong claim opened"* and *"customer withdrew"*, both applicable only after something was approved. MVP 1 does not have this path, and that is a declared limitation (§10.1, S-13): a claim approved in error must be corrected before payment, or handled as an overpayment outside the system.

---

## Part V — Adjudication rules

This is the part the Claim Platform exists for, and the part where the reference material was least helpful — for a reason worth stating plainly.

### 5.0 What the reference does, and why we do it differently

§1.4 states the architectural difference: **Reference System A does not compute the payable amount**, and this system does. That decision has a practical consequence for this part of the document.

**The reference gives us no cover on the arithmetic.** Because its numbers arrive pre-computed from a partner, its specification never states the base amount for co-payment, never states the order of co-payment against deduction ratio, and — in the current design — has dropped deductible (`mức miễn thường`) entirely. Its own successor design, a configurable formula engine, states the requirement but not the formulas. There is nothing to learn from and nothing to check against.

**So §5.3 to §5.7 are ours to specify, and they are specified exhaustively.** Ambiguity about a base amount is the commonest defect in health claim specifications, and it is the one that produces settlements nobody can reproduce.

### 5.1 The unit of adjudication

| Level | What it is |
|---|---|
| **Claim** | The case. One customer submission |
| **Insured event** | One episode of treatment: one provider, one date range, one diagnosis set. **A claim carries many** |
| **Visit** | **Derived, new at v0.5 — `BR-LEDGER-14`.** A cost line's **`treatmentDate` together with its provider**; lines sharing both are one visit. **It is not captured and it is not a level of the data model** — it is computed from facts already on every line. **An insured event carries many visits**: a twelve-session course of physiotherapy at one provider is *one event and twelve visits*, and without the distinction a per-visit cap and a visit limit both count courses (§5.9.2) |
| **Cost line** | One itemised charge from a cost statement. **The unit of the pay/decline decision** |

**One claim, many events.** Reference System A organises the claim as `Lần 1`, `Lần 2`, … each with its own event header, benefit table and cost table, rolled up at settlement. Our data model has always been correct here, but the narrative and worked examples in earlier documents described a single episode, and a reviewer familiar with the reference would read our design as single-event. It is not.

**The decision is at cost-line level, and that is a deliberate departure.** In Reference System A the coverage decision (`Phạm vi BH`) is taken per *event*, and per-cost-line accept/reject exists only as two read-only checkboxes populated by the OCR partner. We make the line-level decision first-class and editable, because it is where the value is: declining one drug on an invoice, with a reason a customer can read, is the difference between a settlement letter that answers questions and one that generates them.

### 5.2 Multi-diagnosis and multi-procedure splitting

Where an episode carries several diagnoses, **each diagnosis produces its own assessment line**. Reference System A splits on ICD codes where present, and on `;` `.` `/` `-` separators where the diagnosis is free text; likewise one line per surgical procedure.

We adopt the ICD-based split. We do **not** adopt the punctuation-based split of free text: it is a heuristic that mis-splits Vietnamese medical prose, and where no ICD code is present the correct behaviour is to raise it for the assessor rather than guess.

### 5.3 The rule chain

**Fourteen steps, in order.** A5 §6.1 is the authoritative version for the pilot product; this states the general rule and the reasoning.

| # | Step | Applies | **Granularity — the set the step is evaluated over** | Rule |
|---|---|---|---|---|
| 1 | Member eligibility | Always | **The whole claim** | Member active on the service date, policy in force, **premium recorded paid — a boolean, with no grace period** (a consequence of S-04, stated here rather than left silent: Vietnamese practice normally allows one, and a hard flag will decline claims that are in fact payable). A failure here produces a **computed decline**, and **a decline never auto-approves** — §4.5, and §5.8 condition 1 |
| 2 | Waiting period | Always | **Per insured event** — the condition class is stored on the event (BR-WAIT-3) | By condition class — §5.4 |
| 3 | Pre-existing condition | If `conditionClass = PRE` | **Per insured event** | **A no-op on the pilot product, and stated as one rather than left to look like a rule.** Step 2 already applies the waiting period by condition class, and A5 §5 gives `PRE` a 365-day wait, so this step re-tests what step 2 has already decided and can only agree with it. **The step exists because a product may exclude pre-existing conditions outright rather than defer them**, and that is a different answer from a longer wait — this product does not, so the step passes every line it sees. Read with step 13, which is the same construction for the same reason |
| 4 | **General exclusions** | Always | **Per cost line** where the term's effect is `DECLINE_LINE`; **per insured event** where it is `DECLINE_EVENT` (BR-TERM-2) | **Evaluated by the term engine against the term register** at `PRODUCT` scope — §5.3.1. Class **D** may auto-decline **where the term's effect is a decline**, reason code `EXCL`, citing the `clauseRef` — and **a class-D term may equally carry `CHECKPOINT`**, which two of the pilot's ten do (A5 §7.2). Class **J** and class **E** raise a **checkpoint** and may **never** carry a decline effect (**BR-TERM-7**). **A term whose `clauseRef` is provisional may not decline at all and raises a checkpoint instead — `BR-TERM-10`**, which on the pilot product's current register is every term |
| 5 | Benefit mapping | Always | **Per cost line** | Cost line → benefit code, per A5 §8 |
| 6 | **Line-level cover test** | Always | **Per cost line** | Each cost line is payable or declined **individually**, with a reason — §5.5 |
| 7 | **Benefit-level provisos** | **If a term is scoped to the benefit** | **Per cost line, within the scoped benefit** — or within the benefit **and every descendant** where the scope is `BENEFIT_SUBTREE` (**BR-TERM-8**) | **Evaluated by the term engine at scope `BENEFIT` or `BENEFIT_SUBTREE`** — §5.3.1. *"Outpatient physiotherapy only on referral"*, *"maximum 30 days per event"*: a term attached to one benefit, which is neither a general exclusion nor a numeric cap and until v0.4 had nowhere to live. **There is no instance on the pilot product** — all ten terms in the register are `PRODUCT` scope — so **the step is a mechanism with nothing to run, exactly as step 13 is**. See the note below and **B-23** |
| 8 | Per-visit / per-day / per-event sub-limit | If the benefit has one | **Per visit** where the benefit's `count_unit` is `VISIT`; **per treatment day** where it carries a per-day cap; **per insured event** otherwise. *Never* per claim | Capped at the child benefit's limit. **The visit is a derived unit — `treatmentDate` plus provider, `BR-LEDGER-14`** — and without it a twelve-session course of treatment is one event and a per-visit cap is applied once to the whole course |
| 9 | Count limit | If the benefit has one | **Per benefit, per counter period** | Days or visits remaining. **`0` means unlimited**. The counter behind it is maintained by the ledger as an `OCCURRENCE` or a `DAY` counter — §5.9.2. `OCCURRENCE` increments **once per distinct visit** (**BR-LEDGER-14**); `DAY` increments **once per distinct calendar date per counter** (**BR-LEDGER-12**) |
| 10 | **Co-payment** | If the benefit or an ancestor has one | **Per cost line** | Base: the amount remaining after step 9 |
| 11 | **Deduction ratio** | If the benefit carries one **and its condition is met** | **Per insured event** — §5.6. **Whether "the benefit" means the line's own benefit or an ancestor that carries the rate is an open decision, `B-19`**, and the two readings differ in money | **From the policy**, not from a user. Base: the amount remaining after step 10. The triggering condition is recorded on the claim — §5.6, A5 §4.3 |
| 12 | Parent annual cap | Always | **Per benefit, per counter period, across every line of the claim on that ancestor path** | Walk the ancestor chain to the root — §5.7. **The order in which lines consume a cap that binds is an open decision, `B-20`**; the interim rule is the one proposed there |
| 13 | Plan-level check | Always | **The whole claim** | No plan-wide aggregate cap in this product; the step exists for future products |
| 14 | Post to ledger | Always | **Per cost line**, to **every counter on the ancestor path** (BR-TREE-1, BR-TREE-2) | Provisional at calculation, final at approval — §5.9 |

**The granularity column is new at v0.5, and the reason it had to be added is worth one paragraph.** To v0.4 this table stated the base *amount* for every percentage — §5.0 argues at length that ambiguity there is the commonest defect in health claim specifications — and it never stated the base *set of lines*. Those are two different questions and only one of them was answered. Granularity genuinely **varies by step**: step 8 is per visit or per event, step 9 is per benefit per period, step 10 is per line, step 11 is per event, step 12 is per benefit across the whole claim. **A5 §6.1's blanket *"applied per cost line, then aggregated"* is therefore true of most steps and false of steps 8 and 11**, and A5 v1.5 is corrected to point here rather than to restate it. An implementer given a base amount and no base set can write five defensible engines that return five different settlements.

> **Step 7 has no instance, and it is said here in the same terms step 13 uses.** Every one of the ten terms in the pilot product's register is `PRODUCT` scope (A5 §7.2, `A5_seed_v1.5.json`), so **the term engine is called at step 7 and evaluates an empty set on every claim of this product**. The two examples in the row above — outpatient physiotherapy on referral, a maximum of 30 days per event — are illustrations of the shape, **not terms of this product**, and neither exists in the register. **This is stated rather than quietly true** because step 7 was created at v0.4 specifically to fix a step that could not be evaluated, and a step that evaluates nothing is the same defect wearing the fix. What it is not is a reason to remove the step: the mechanism is right, the scope model is right (**BR-TERM-8**), and a benefit-level proviso is an ordinary feature of a health wording. **What is missing is a real one, and it is asked of the business as `B-23`.** No proviso is invented here — inventing a contractual term to give a mechanism something to do is how a specification acquires a rule no wording supports.

**The renumbering, stated once so nothing tracks the wrong step.** Step 7 is new at v0.4 (decision D-4, 24/08/2026). Every step that was 7 to 13 is now 8 to 14, and the base amounts at the two percentage steps moved with them: co-payment now takes the amount remaining after step 9, and the deduction ratio takes the amount remaining after step 10. A5 §6.1 and A5 §6.2 carry the same rename, and any document still stating the old count is out of date rather than describing a variant.

**Why provisos sit after the line-level cover test and not before it.** A line that is **not covered at all** should be declined on that ground, not on a proviso it was never going to reach. A decline citing *"physiotherapy requires a referral"* on a line that was never a covered benefit is an answer to a question the customer did not ask, and it is the kind of decline that generates a complaint even when the outcome is right.

**It takes the slot A5 §6.1 had reserved for BHYT.** A5's note says that when the BHYT deduction is switched on (simplification **S-17**, §5.6a) it becomes *"a new step 7"*. Provisos are being built now and BHYT is not, so **provisos take step 7 and BHYT's reserved slot moves to step 8** — making the chain **fifteen steps when S-17 is enabled**. A5's note is edited to say so. Left unedited, the two would collide silently on the day someone turns BHYT on.

> **The claim that the order is configuration is withdrawn, and this replaces it.** Up to v0.3 this section read *"The order is configuration, not code. Each step can be switched off per benefit."* **The first sentence was wrong and is corrected here rather than deleted quietly**, because two versions of this document told a developer something untrue. The seed's `adjudication_rule` object is `{step, name, enabled}` — an **on/off list against a fixed sequence**. Matching the Framework's revised **P-05**: **the adjudication sequence is fixed in code and versioned with the release. What is configurable is *participation* — each step may be enabled or disabled per benefit and per product — and *terms*, which are data (§5.3.1). A change to the arithmetic or to the order of steps is a release.**
>
> **What that costs, and it is a cost rather than a tidying-up.** A new calculation rule carries **one release cycle** of lead time, against **same day** for a change to a term. Declared as simplification **S-39**. **SC-06 survives the correction** — a second product with a different cost-share model is still configurable without a release, because the variation across a health portfolio is in *which terms apply and what they mean*, not in the arithmetic. What SC-06 no longer promises is a new *formula* without a release, and it never should have.

**A step that cannot be evaluated is never a pass. A new series, `BR-CHAIN`, and it has one rule.**

| Rule | Statement |
|---|---|
| **BR-CHAIN-1** | **A step of the chain that cannot be evaluated because a fact it requires is unavailable is never treated as a pass.** The line is **held**, a **checkpoint is raised naming the missing fact**, and the claim is **outside the auto-approval gate** (§5.8, condition 3). It is not a decline either: an absent fact is a data problem, and §3.2.3's distinction between failing to *find* something and failing to *satisfy* it applies to a fact exactly as it applies to a policy |

**Why this is one sentence and why it is worth a series of its own.** §5.9.2 already admits that a `DAY` counter depends on a treatment-day count extraction does not return today (**Q-IDP-06**, **DE-14**), and to v0.4 **nothing anywhere said what step 9 does when it meets a line it cannot count**. A developer with no counter and no rule lets the line through, because passing is the path of least resistance — and the claim then satisfies condition 1 of the gate, because nothing was cut, and settles unreviewed against a limit nobody enforced. **The failure is silent, it favours the claimant, and it is invisible in every log.** `BR-CHAIN-1` is written over *any* fact rather than over that one, so **every future gap in the vocabulary is covered on the day it opens** rather than on the day someone notices. It costs a held line and a checkpoint; the alternative costs a limit.

**Rounding, stated once.** Every step that produces a money amount rounds to the **nearest whole dong, half away from zero**, and rounding happens **at each step, not once at the end**. This is not a detail: BR-TRACK-4 re-runs the whole chain after a clinical recommendation is confirmed, and a chain that rounded only at the end could return a different total from the first pass on the same inputs. Percentages are held as integer basis points (D3 §1.2); rounding applies to the money result, never to the rate. A5 §6.2 carries the same rule.

### 5.3.1 The term register, and the three classes of term

Steps 4 and 7 are the two points at which the chain calls **data rather than code**. What they call is the **term register**: **claim-side master data, keyed on `ruleSetRef`** — the rule-set version I-05 already returns — holding each contractual term as an evaluable object with its clause citation. **InsureMO owns *which* terms apply; we own *what they mean operationally*.** That is the same division already taken for the policy wording store, and it is what keeps **P-01** intact: a predicate arriving from a vendor is a decision-in-waiting.

**This replaces the pointer to prose.** To v0.3, step 4 read *"per the policy wording — A5 §7"*, and A5 §7 is ten sentences with no predicate, no scope, no effect and no clause reference behind them. **A rule that names a paragraph is not a rule a system can evaluate.** Features **F65** (the register), **F66** (the evaluation service) and **F67** (the checkpoint queue) are what make step 4 real.

**The register carries its own version, and `ruleSetRef` is not it.** `ruleSetRef` is **InsureMO's** identifier for the policy wording, returned on the I-05 read and carried onto the claim unchanged. **The term register is ours**, and `BR-TERM-5` expressly permits publishing a **new term version under the same rule set** — the wording did not move, our expression of it did. So `ruleSetRef` alone does **not** identify what a claim was evaluated against, and to v0.4 **`BR-TERM-3`'s determinism claim was false as stated and `BR-TERM-5` was unimplementable**: neither could distinguish two evaluations of the same claim under two versions of the same register. **The claim binds `ruleSetRef` *plus the register's own version identifier*.** Both rules are restated below to say so.

**And it binds at registration, onto the snapshot — not at submission.** `ruleSetRef` **arrives at registration**, on the I-05 read (J2-7), which is the same moment and the same read the benefit schedule arrives on. A rule that binds it at submission binds a value the claim does not yet hold. **`BR-KB-4` already gets this right for the knowledge base** — every claim records the version in force when it was assessed — and the term register had no equivalent. It has one now, it is the same rule, and the version lives where every other frozen contractual fact on a claim lives: **the policy snapshot** (§7.2). §7.1's master table gains the matching row.

**The central distinction, and the rest of this section rests on it: not every term can be evaluated, and pretending otherwise is how an insurer auto-declines a claim on a clause nobody read.**

| Class | Meaning | The engine **may** | The engine **may never** |
|---|---|---|---|
| **D — Deterministic** | Decidable from facts the claim already holds | **Auto-decline**, with reason code `EXCL` and the term's `clauseRef` quoted on the decline | — |
| **J — Judgemental** | Requires a person, or a clinical opinion — *"cosmetic"*, *"under the influence"* | Raise a **checkpoint** with the clause quoted, for a named human queue | **Auto-decline** |
| **E — Evidential** | Decidable only if a specific named document says so — a police or hospital statement | **Require the document**; raise a checkpoint if it is absent | **Auto-decline** |

**`termClass` and `effect` are two axes, not one**, and collapsing them into one is the mistake this paragraph exists to prevent. **`termClass` answers *can the system decide this?*** — and that is the question governing whether an auto-decline is permitted. **`effect` answers *what happens when the term is in play*** — `DECLINE_EVENT`, `DECLINE_LINE`, `REQUIRE_DOC`, `CHECKPOINT` or `CAP`. A class-D term is one whose **selector** fires deterministically; **that does not oblige it to decline.** Two of the pilot product's ten terms are class **D** and carry `CHECKPOINT` (A5 §7.2), and exclusion #3 is the clearest case: `serviceCategory = SUPPLEMENT` selects deterministically, but the wording's proviso — *"not prescribed as treatment for the diagnosed condition"* — means a supplement that **was** prescribed for the condition is covered by the wording. Declining it automatically would be wrong. So it **selects as class D and resolves as a `CHECKPOINT`**.

**The prohibition is therefore one-directional, and it is stated in one direction on purpose — `BR-TERM-7`. A class `J` or class `E` term may never carry `DECLINE_EVENT` or `DECLINE_LINE`. A class `D` term may carry any effect, including `CHECKPOINT`.** Read the other way round — *"a class-D term always declines"*, or *"only a class-J term may raise a checkpoint"* — the model forces an auto-decline wherever a fact happens to exist, which is precisely the outcome the three classes were introduced to prevent.

**The reason for the prohibition, stated once because it is the whole point of the class model.** A class-J term auto-declined is **a decline citing a clause on a judgement no human made** — invisible to us, unanswerable to the customer, and a regulatory and reputational exposure. This is the same posture as **AR-1…AR-4**, extended from clinical assessment to policy terms: **the engine proposes, the platform's human decides.** A class-D term is the one case where the engine is not proposing but reading, and the decline is reproducible from the claim's own facts.

**A term whose fact does not exist cannot be class D.** Both expressions below are written over a **published, closed fact vocabulary** — A1 §4.11, an author cannot invent a fact — and both are evaluated against **the policy snapshot and the claim, never against live data** (**P-02**, §7.2). A1 owns the vocabulary and freezes it per release; adding a fact is a specification change, not a configuration change.

**A term whose fact does not exist *yet* cannot be evaluated either, and that is a different failure.** A fact can be in the vocabulary and not be **available at the step the term runs at**, because the chain produces facts as it goes. `benefitCode` is the clearest case: **step 5 produces it**, so a `PRODUCT`-scope term — which runs at **step 4** — is testing a field that is null on every claim, every time. One of the pilot's ten terms does exactly this, and the failure is silent: the expression evaluates, it evaluates against nothing, and the term either never fires or always fires depending on how the engine treats a null. **`BR-TERM-9` closes it as a register-time rule rather than a run-time one** — the register screen refuses to save a term testing a fact its scope cannot see. **A1 v1.5 carries the enforcing data: an *available from step* column on the fact vocabulary**, so the constraint is checkable from the annex that owns the vocabulary rather than from a list in this document that would drift.

#### Scope, and what each effect actually does

**Scope has three values, not two, and this document owns the rule.** v0.4 named `PRODUCT` and `BENEFIT`; the third — `BENEFIT_SUBTREE` — was specified in D2 §5.28.3 F9 and F10 as the register screen's enum, and **the roll-up semantics it carries were stated nowhere in this document**. A scope value that changes which lines a contractual term reaches is a business rule, not a screen enum, so it is carried here and D2 cites it.

| Scope | Evaluated at | The set it reaches |
|---|---|---|
| **`PRODUCT`** | **Step 4** | Every cost line and every insured event on the claim. The `benefitCode` of a line is **not yet known** (BR-TERM-9) |
| **`BENEFIT`** | **Step 7** | Only the cost lines that mapped at step 5 to **the one benefit the term names** |
| **`BENEFIT_SUBTREE`** | **Step 7** | The cost lines that mapped to **the benefit the term names *and to every descendant of it*** — the same ancestor path **BR-TREE-1** uses for consumption, walked downward instead of upward |

**Why the third scope exists and why it is not a convenience.** A wording that says *"outpatient treatment requires a referral"* means the whole of outpatient treatment — consultation, medication and physiotherapy alike — and expressing that as three `BENEFIT`-scope terms is three objects to keep in step, three clause citations to keep identical, and one of them to forget when a fourth child benefit is added. **`BENEFIT_SUBTREE` names the node the wording names.** It is the same argument BR-TREE-1 makes for limits: the tree is the structure the contract is written in, so a term should be able to attach to a node of it.

**And the effects, all five, each with a mechanism.** Two of the five had none to v0.4 — they were enum values with no behaviour behind them, which is the defect class §0.2b was written about.

| `effect` | What happens | Where |
|---|---|---|
| **`DECLINE_LINE`** | The cost line is declined, reason `EXCL`, the `clauseRef` quoted | Step 4 or step 7 |
| **`DECLINE_EVENT`** | The whole insured event is declined, whatever the line results — **BR-TERM-2** | Step 4 or step 7 |
| **`CHECKPOINT`** | A checkpoint is raised on the named human queue (**F67**) with the clause quoted. It blocks the auto-approval gate (§5.8) **and the submission of a settlement plan** (**BR-TERM-13**) | Step 4 or step 7 |
| **`REQUIRE_DOC`** | **Defined at v0.5 — `BR-TERM-11`.** The term's named **A4 document type** is added to the claim's checklist at **`REQUIRED`** level and a **checkpoint is raised**, until the document is supplied or the requirement is waived | Step 4 or step 7 |
| **`CAP`** | **Defined at v0.5 — `BR-TERM-12`.** A monetary ceiling stated by a term rather than by the benefit schedule. It applies **at step 7**, to the amount as at the end of **step 6**, and **it is a limit bound** for the purposes of §5.8's condition 1 | Step 7 only |

> **`CAP` is defined rather than dropped, and the choice was live.** No term in the pilot product uses it, and an enum value nothing uses and nothing defines is exactly the thing this project keeps finding and removing. **It is kept for two reasons.** First, the value is already published in four places — A1's value set, A5 §7, the seed's schema and D2 §5.28.3 — so dropping it is a coordinated change across four documents to remove one line, and **the cost of that change is larger than the cost of the definition**. Second, the definition is not a judgement call: **step 7 is where a benefit-scoped term acts, step 6 is the last step before it, and a ceiling that reduces a payable is a cut** — so `CAP` binds a limit and takes a claim out of the automatic route exactly as a schedule limit does. Left undefined it would have done none of that, and a claim capped by a term would have auto-approved while a claim capped by the schedule did not.

#### The two expressions a term carries, and why one was not enough

**A term has to answer two different questions and until this correction the model gave it one field for both.** v0.4 as first issued gave `policy_term` a single `predicate` and said it was *"null and not permitted for class J"*.

| Question | What answers it |
|---|---|
| **Is this term in play for this claim, this event or this line at all?** | **`selector`** |
| **Given that it is in play, does it fire — is the line or the event declined?** | **`predicate`** |

**The single-field model has a hole, and it fails in the unsafe direction.** With the one field forbidden on class J there is nothing left to decide *which* claims a judgemental term concerns — and a term with no way to say which claims it concerns concerns all of them. **Evaluated literally, every class-J term raises a checkpoint on every claim**, and a queue that receives the whole portfolio is a queue nobody works.

| Field | Decides | Required | Permitted |
|---|---|---|---|
| **`selector`** | Whether the term is **in play** for this claim, event or line | **Mandatory and non-null on every class — `D`, `J` and `E` alike** | Always |
| **`predicate`** | Whether the term **fires** — the auto-decline | **Mandatory where the class is `D` and the effect is `DECLINE_EVENT` or `DECLINE_LINE`** | **Class `D` only. Null on `J` and on `E`** |

**For a class-D term the two may be the same expression** — *"the service category is `OPTICAL` or `HEARING`"* both selects and decides — and for a class-D term carrying `CHECKPOINT` the predicate is **null**, because nothing is auto-declined: the selector puts the clause in front of a person and the person decides. **For a class-J term the two can never be the same expression**, because the second question is the one the human is there to answer.

**Where a class-J term has no selecting fact at all**, the honest answer is not a null selector — that is the hole above — but a **narrower selector drawn from the vocabulary, with the residue reaching a person through the clinical track** (A5 §7.3 does this for the pilot's two irreducibly judgemental terms, and states plainly what each selector does not reach).

#### The business rules

**A new series, `BR-TERM-n`.** The existing series in this document are `BR-PAY`, `BR-DUP`, `BR-IMP`, `BR-WAIT`, `BR-LINE`, `BR-TREE`, `BR-AUTO`, `BR-LEDGER`, `BR-TRACK`, `BR-KB`, `BR-DOC`, `BR-MSG`, `BR-DATA` and `BR-AUDIT`; **`BR-TERM` is unused and is taken here.** **`BR-CHAIN` is taken at v0.5** (§5.3) and holds one rule; the two do not overlap, because `BR-CHAIN-1` is about the chain meeting a fact it does not have and `BR-TERM-9` is about a term being written to test one it cannot see.

| Rule | Statement |
|---|---|
| **BR-TERM-1** | **Evaluate all, not first-match.** Every term in scope is evaluated and **the result of every term is recorded whether it fires or not** — the assessor sees *"14 terms tested · 1 declined · 2 checkpoints raised · 11 passed"*, not only the first hit |
| **BR-TERM-2** | **Precedence.** `DECLINE_EVENT` outranks `DECLINE_LINE` — an event-level term declines the whole event whatever the line results. Within a line, **every** hit is recorded and **every** `clauseRef` is carried into the decline message, not only the first. A `CHECKPOINT` never overrides a decline; a declined line with an open checkpoint stays declined and the checkpoint closes as moot, with that reason recorded |
| **BR-TERM-3** | **Determinism. Restated at v0.5.** Same claim, same snapshot, same `ruleSetRef` **and same term register version** → **the same result, always**, and independent of evaluation order. **This is a release gate**, not an aspiration. **The register version is not optional in that list**: `BR-TERM-5` permits a new term version under an unchanged rule set, so without it the rule promises determinism across two evaluations that were genuinely evaluated against different terms |
| **BR-TERM-4** | **Version binding. Restated at v0.5.** **A claim binds the *term register version* — `ruleSetRef` plus the register's own version identifier — at *registration*, onto the *snapshot*, and is evaluated against it for its life.** A term added, changed or withdrawn tomorrow does not disturb a claim registered today. **Two things moved from v0.4 and both were defects**: `ruleSetRef` alone does not identify the terms, and **binding at submission was impossible** because `ruleSetRef` arrives at registration on the I-05 read (J2-7). **P-02 gives the mechanism and `BR-KB-4` gives the precedent** — the knowledge base has been versioned onto the claim since v0.1 and the term register was not |
| **BR-TERM-5** | **Correction versus new version — the author must declare which.** A **new version** (the terms have genuinely changed) leaves in-flight claims **untouched**. A **correction** (the term never expressed the intent — the selector matches the wrong population, the predicate tests the wrong field, the clause reference is wrong) **re-evaluates in-flight claims** and routes every claim whose result changes to a **named human queue**. **Never an automatic re-decision.** A correction requires a **written reason** and an **impact count shown before confirmation** |
| **BR-TERM-6** | **Neither operation may touch a claim in `PAID` or `CLOSED`.** Correcting a settled claim means recovering money from a customer or paying them more, and MVP 1 has neither. Declared as simplification **S-40** |
| **BR-TERM-7** | **The two expressions, and the one-directional effect rule.** **`selector` is mandatory and non-null on every class**, `D`, `J` and `E` alike; **`predicate` is permitted on class `D` alone**, is null on `J` and on `E`, and is **mandatory where a class-`D` term's effect is `DECLINE_EVENT` or `DECLINE_LINE`**. And `termClass` does not determine `effect`: **a class `J` or class `E` term may never carry `DECLINE_EVENT` or `DECLINE_LINE`; a class `D` term may carry any effect, including `CHECKPOINT`.** Added by the same-day correction of 24 August 2026 (§0.2c); enforced on the register screen as D2 `V-734` and `V-742` |
| **BR-TERM-8** | **Scope has three values and the third rolls up.** `PRODUCT` is evaluated at **step 4**; `BENEFIT` and `BENEFIT_SUBTREE` at **step 7**. `BENEFIT` reaches the cost lines that mapped to **the named benefit**; **`BENEFIT_SUBTREE` reaches the named benefit *and every descendant of it*, on the same ancestor path BR-TREE-1 uses for consumption**. A `PRODUCT`-scope term names no benefit; the other two must name one that exists in the snapshot's benefit tree. **New at v0.5** — the third value existed in D2's register screen and its semantics existed nowhere |
| **BR-TERM-9** | **A term may test only facts that are available at the step its scope is evaluated at.** A **`PRODUCT`-scope term may not test `benefitCode`**, because step 5 produces it and step 4 runs before step 5. This is enforced **when the term is saved**, not when it is evaluated — a term that cannot be evaluated must not reach the register. **A1 v1.5 carries an *available from step* column on the fact vocabulary**, which is the data this rule is checked against. **New at v0.5** |
| **BR-TERM-10** | **A term whose `clauseRef` is provisional may not decline.** Where the clause reference is a placeholder rather than a citation into filed wording, the term **raises a checkpoint instead of declining**, whatever its class and whatever its effect, and the checkpoint names the missing citation. **A decline must quote a clause that exists** — §5.5 and BR-MSG-3 both require the customer to be told which term was relied on, and a placeholder is not an answer to that. **On the pilot product's register today this applies to every term**, so **step 4's auto-decline branch cannot fire at all until filed wording exists**. A5 §7 carries the same prohibition. **New at v0.5** |
| **BR-TERM-11** | **`REQUIRE_DOC` has a mechanism.** A term whose effect is `REQUIRE_DOC` **adds its named A4 document type to the claim's checklist at `REQUIRED` level** (BR-DOC-1) and **raises a checkpoint** which stays open **until the document is supplied or the requirement is waived** by an assessor with a recorded reason. The information-request flow (§6.3, F29) is what asks the customer for it, so nothing new is built. **A4 carries no police or hospital-statement type today**, which is what the pilot's one class-E term needs — asked as **B-24**. **New at v0.5** |
| **BR-TERM-12** | **`CAP` has a mechanism.** A term whose effect is `CAP` applies a **monetary ceiling at step 7**, to the amount as at the end of **step 6**, and **counts as a limit bound** — so a claim it reduces fails §5.8's condition 1 exactly as a schedule limit does, and BR-TREE-5's obligation to explain a binding cap applies to it unchanged. No term in the pilot product uses it. **New at v0.5** |
| **BR-TERM-13** | **An open checkpoint blocks the submission of a settlement plan, not only the automatic route.** A settlement plan may **not** be submitted for approval while a checkpoint raised at **step 4 or step 7** is open: each must be **upheld**, **cleared**, or **closed as moot behind a decline** (BR-TERM-2). **New at v0.5, and it ratifies and closes `D2-177`** — D2 §5.10.2 specified the block as submission check 13 and `V-732`, correctly declined to invent the business rule, and asked D1 to ratify or refuse. **It is ratified.** Without it the two routes to an approval were governed differently for no reason anybody chose: the gate refused a claim carrying an unmade judgement and an assessor could submit the same claim by hand, to an approver who may not edit it and cannot supply the judgement either |

**BR-TERM-13 is the smaller half of the checkpoint rule and the one that was missing.** §5.8's condition 3 gates the **automatic** route. A checkpoint is a statement that **a person must decide something before this claim settles**, and settling by the manual route with it still open is the same outcome reached more slowly. The approver is not a substitute: A3 §2.1 gives them no edit and §3.2.5 locks the claim, so a plan submitted with an open checkpoint arrives at somebody who can only approve it or return it.

**BR-TERM-1 is what an auditor needs and an assessor cannot work without.** An assessor who sees only the first hit cannot tell whether the case was close. An auditor asking *"was clause 5.2 considered?"* must get an answer, and the answer must not be *"we stopped before we got there."*

**BR-TERM-5's asymmetry is deliberate and it is the same reasoning as class J.** A correction moves the claim **to a person**, with the before and the after both recorded. Automatic re-decision on a correction would be the engine settling a question the engine got wrong once already.

**Where BR-TERM-5's routing meets a locked claim, and how that is settled — new at v0.5.** *"Routes every claim whose result changes to a named human queue"* collides with **§3.2.5's claim lock**: a claim in `PENDING_APPROVAL` is **read-only**, so there is no person a correction can route it to who is permitted to act on it — the approver may not edit (A3 §2.1) and the assessor no longer holds it. **The resolution is the one §4.4 already uses: a corrected claim in `PENDING_APPROVAL` is *returned to the assessor* under §4.4, with the correction recorded as the reason.** The lock releases on a return, the claim goes back to whoever had it, and the person who can act on the correction is the person the correction reaches. **Nothing new is built** — the return path, its reason field and its restoration of the prior state all exist — and the alternative, letting a correction write through a lock, would be the sync hazard **AR-3** was written to remove, arriving from inside the system instead of from a vendor.

**Governance carries this, and it is not optional.** Authoring a term is making a contractual statement: author and approver must be different people (`TERM_AUTHOR`, `TERM_APPROVE` — A3), draft terms are invisible to the engine, back-dating past the earliest open claim on that rule set requires a recorded override, and every evaluation is retained with the term version that produced it for the life of the claim.

### 5.4 Waiting periods

**A benefit does not have a waiting period. It has up to six**, one per condition class. A5 §5 is the authoritative matrix; A1 §4 is the authoritative code list (`PRE`, `SPE`, `GEN`, `BIR`, `MAT`, `ACC`).

| Rule | Statement |
|---|---|
| **BR-WAIT-1** | The period is measured from the **member's coverage start date**, not the policy inception date |
| **BR-WAIT-2** | On a renewal without a break in cover, periods already served are **not re-served**. **Stated for completeness and not exercisable in MVP 1** — renewals are out of scope (Framework §7.2), so no renewal is ever performed and nothing in the register we read carries a prior-term link. It is a rule the product needs, not a rule this release can demonstrate. **v0.3 note:** the reason is now weaker than it was and worth re-checking later. It used to rest on a component we were building and had decided not to give a renewal object; it now rests on scope alone, and **whether InsureMO's register can express continuity of cover across terms has not been asked** — it was not part of the 21/08 demonstration. If it can, this rule becomes exercisable the moment renewals enter scope, which is a better position than v0.2 described |
| **BR-WAIT-3** | The condition class is determined at assessment and stored on the insured event |
| **BR-WAIT-4** | `n/a` in the matrix means the combination **cannot arise**. A claim that produces one is a **data error**, not a zero-day waiting period |
| **BR-WAIT-5** | A waiting-period decline shows the arithmetic: the period, the date it counts from, and the treatment date |

**BR-WAIT-5 is a business requirement, not a wording preference.** Waiting-period declines are the most disputed category. A customer who can see the calculation disputes it far less often than one who is told only that they claimed too early.

**Warning versus block.** Reference System A raises waiting-period breaches as **red status-bar warnings, not blocking validations** — the assessor sees *"NĐBH chưa đủ thời gian chờ 30 ngày"* and decides. We adopt the same posture: the engine computes the decline, the assessor can see and override it with a reason, and the override is audited. A waiting period that hard-blocks removes the assessor's ability to honour an endorsement the data does not carry.

### 5.5 The line-level cover test

Each cost line resolves to **payable** or **declined**, with a reason from the seven codes in A1 §5.2. Two of those codes — `NOTREL` (not related to the diagnosis) and `NOTCOV` (not a covered medical item) — exist **only** at line level.

| Rule | Statement |
|---|---|
| **BR-LINE-1** | A declined line carries a reason code **and** a rationale naming the item and the amount |
| **BR-LINE-2** | The rationale may be **AI-drafted**; the assessor may edit it before it is sent |
| **BR-LINE-3** | Both the original drafted text and the final text are retained |
| **BR-LINE-4** | A declined line appears in the settlement letter **in the same table as the paid lines**, never in an appendix |

**BR-LINE-3 is what makes the AI improvable.** Comparing what the machine drafted against what the assessor sent is the only measurement of whether the drafting is good enough — and it is the same principle as retaining the raw extraction beside the corrected claim data (§7.3).

**BR-LINE-4 is a trust decision.** Separating declined lines into an appendix makes a letter look as though it is hiding something.

### 5.6 Two reduction mechanisms, and why both exist

Framework §7.6 states the distinction; it matters enough to restate the business reasoning.

**Both come from the policy** (decided 17/08/2026, B-09). What separates them is *when* they apply.

| | **Co-payment** (*đồng chi trả*) | **Deduction ratio** (*tỷ lệ giảm trừ*) |
|---|---|---|
| Comes from | The **policy** | The **policy** |
| Applies | **Always**, when the benefit is used | **Only when its stated condition is met** — e.g. a provider outside the listed network |
| Editable by a user | No | **No** |
| Base | **Amount after sub-limits and count limits (steps 8–9) and line declines** — *not* after every limit. **Corrected at v0.5**: the parent annual cap is a limit too and it is **step 12**, four steps *after* co-payment, so *"after limits"* named a base that does not exist at step 10 | Amount after co-payment |
| Evaluated | Per benefit | **Per insured event** — one claim can carry a network visit and a non-network visit |
| Audited as | Product configuration | Product configuration, plus the triggering condition recorded on the claim |

**There is no discretionary reduction in MVP 1, and that is a stronger position than a bounded one.** Every reduction on a settlement traces to a term of the contract. An assessor who believes a line should be paid in part rather than in full has exactly one mechanism — **decline the line, with a reason** (§5.5) — and no way to move a number quietly. This removed a ceiling, a permission grant and an override rule from the design.

Reference System A carries both fields, with its deduction ratio editable. **We deliberately do not follow it there**, because an editable percentage with no contractual anchor is the easiest place in a claim system for money to leave without an explanation.

**Deductible (*mức miễn thường*) is out of scope.** The pilot product has none. Reference System A's current design has dropped it too, and it appears only in its legacy specification — which is itself evidence that health indemnity products in this market rarely use one. If a future product needs it, it enters as a step between 9 and 10 — between the count limit and co-payment, on the renumbered chain of §5.3.

### 5.6a BHYT — captured, not deducted

**Decision, 17 August 2026 (B-05, C-BUS-05): MVP 1 does not coordinate with BHYT.** The rule chain runs on the **billed** amount. This is declared simplification **S-17**.

#### Why the decision exists at all

A Vietnamese hospital cost statement decomposes every line by who pays it:

| Column | What it is |
|---|---|
| `BHYT chi trả` | Paid by social health insurance |
| `Người bệnh cùng chi trả` | The patient's share **under BHYT rules** — typically 20% of BHYT-covered cost |
| `Người bệnh tự trả` | Items BHYT does not cover at all |

A private indemnity product reimburses actual loss. What the member actually lost is `cùng chi trả + tự trả` — not the billed total. Paying on the billed total pays for costs the member never bore.

> **Two things called "co-payment", and they are not the same.** `Người bệnh cùng chi trả` is the patient's share under *social* insurance. `coPaymentBp` is a term of *our* contract. A1 §9 now carries both, because conflating them is the kind of error that survives review and surfaces in production.

#### What the decision costs, stated plainly

On a claim where BHYT paid, MVP 1 pays more than an indemnity product should. Worked, on `HC-STD`: an outpatient bill of **3,000,000** with BHYT paying **2,400,000** leaves the member bearing **600,000**.

| | MVP 1 (S-17 — no deduction) | With coordination |
|---|---|---|
| Amount entering the chain | 3,000,000 | 600,000 |
| Sub-limit `200.1` (1,500,000) | binds → 1,500,000 | does not bind |
| Co-payment 20% | −300,000 | −120,000 |
| **Insurer pays** | **1,200,000** | **480,000** |
| Member out of pocket | **−600,000** *(paid twice over)* | 120,000 |

The member is reimbursed **twice what they spent**. That is not a rounding difference; it is the wrong answer.

#### Why it is nonetheless acceptable for MVP 1

| # | Reason |
|---|---|
| 1 | **Boundary condition BC-3: no real money moves in MVP 1.** Payment is a stub. An over-payment costs nothing because nothing is paid |
| 2 | **The data is already captured.** A4 L8 and L9 extract the decomposition per cost line and it is stored on the claim. Nothing needs re-processing when coordination is turned on — including the ground-truth set |
| 3 | **It is one rule step, in a known position — and the position moved at v0.4.** A5 §6.1 had reserved *"a new step 7"* for it; **benefit-level provisos took that slot** (§5.3), so **BHYT's reserved slot is now step 8**: after the proviso test at 7 and before the per-visit sub-limit, which becomes 9. Decline what is inappropriate, apply the provisos, remove the BHYT share, then cap the residual. **The chain is fifteen steps when S-17 is enabled. No rule changes — but every step from 8 renumbers again**, and the base statements in §5.3, A5 §6.1 and A5 §6.2 are written in terms of those numbers. It is a one-step insert, not a no-op edit |
| 4 | Every other mechanism the MVP exists to prove — extraction, the benefit tree, limits, the ledger, the clinical track — is unaffected |

#### The condition on which this stops being acceptable

**BC-3.** The moment a real payment is made, S-17 becomes systematic over-indemnification, and coordination is not optional — it is what makes the product an indemnity product. This is recorded alongside the boundary conditions in Framework §9.4 rather than in a backlog, because it is a release gate and not a nice-to-have.

#### One risk to manage before the demonstration

If the demonstration uses real anonymised documents from public hospitals — which is exactly what the ground-truth set will contain — **the over-payment is visible on screen**. An audience of insurance people will see a bill showing 2,400,000 paid by BHYT and a settlement computed on 3,000,000, and they will ask about it within seconds.

Two mitigations, either sufficient: draw the demonstration claims from cases where BHYT was not applied (private facility, or `trái tuyến`), or state S-17 on the slide before anyone asks. **The failure mode is letting the audience find it.** Framework RK-11.

**And it is not only the amount.** The settlement notice (A2 §7.1) itemises benefit by benefit on the **billed** figures — numbers the customer never paid, which will not match the receipts in their hand. Under BC-3 no money moves, but a demonstration audience reads the letter, not the ledger.

### 5.7 The benefit tree and limit aggregation

A benefit schedule is a **tree**. Sub-limits are child rows, not columns.

```
200     Outpatient treatment                    annual cap · co-payment
 ├ 200.1   Consultation and diagnostic tests    per-visit cap
 ├ 200.2   Prescribed medication                per-visit cap
 └ 200.3   Physiotherapy                        per-visit cap · visit count
```

| Rule | Statement |
|---|---|
| **BR-TREE-1** | **Roll-up.** A payment consumes the counter of the benefit it is paid under *and every ancestor*. The claim is capped by the tightest binding limit anywhere on that path |
| **BR-TREE-2** | **No double counting.** Where a composite and one of its own sub-limits both apply to the same line, the composite consumption is counted once |
| **BR-TREE-3** | Limits exist in **money** and, independently, in **count** (days, visits, occurrences) |
| **BR-TREE-4** | **`0` means unlimited**, not zero. Honoured explicitly, never inferred |
| **BR-TREE-5** | **A cap that binds must be explained.** Where a limit reduces the payable amount, the settlement letter states the benefit, the remaining limit and the reduction |

**BR-TREE-5 answers a real design question the reference forced open.** Reference System A does **not** auto-cap. When an amount exceeds a limit it **blocks the save** with *"Số tiền thuộc phạm vi của quyền lợi [X] đã vượt quá hạn mức"* and requires a human to resolve it. That is deliberate: it guarantees the assessor sees the limit was hit.

**We compute the capped amount rather than refusing to compute**, because in our design the engine is the calculator and the capped figure *is* the correct payable amount — refusing to produce it would leave the assessor to do the arithmetic, which is what BO-3 exists to remove. The reference blocks because in that system a human computes the number, so a blocked save is the only way to make them look.

**BR-TREE-5 is what keeps the property the block was protecting.** Reference System A guarantees that a human *sees* the cap; we guarantee that the cap is **explained** — the settlement letter states the benefit, the remaining limit and the reduction, on every capped claim, whoever approved it. **The failure mode being designed against is an unexplained partial payment** — capping is allowed, capping silently is not. A human sees it as well, as it happens: a bound limit is a cut, so §5.8's condition 1 keeps a capped claim out of auto-approval. But BR-TREE-5 does not rest on that and would hold if the gate moved.

### 5.8 Auto-approval

**Decided 17/08/2026 (B-12, revised): auto-approval is in MVP 1, on a deliberately simple gate. A third condition was added on 24/08/2026 (decision D-5); a fourth, and a correction to the third, at v0.5. The gate now has four.**

A claim approves without an assessor when **all four** hold:

| # | Condition | Default |
|---|---|---|
| **1** | Total payable is at or below the threshold, **and nothing was cut** — no cost line declined, no limit bound, no term `CAP` applied (BR-TERM-12) | **2,000,000 VND**, changeable by an administrator without a release |
| **2** | The clinical appropriateness assessment **completed and returned no recommendation** | — |
| **3** | **No term checkpoint raised at step 4 or step 7 is open — whichever class raised it.** A checkpoint **closed as moot behind a decline** does not block (BR-TERM-2). A step held under **`BR-CHAIN-1`** raises a checkpoint and blocks here like any other | — |
| **4** | **No duplicate signal under `BR-DUP-1` or `BR-DUP-3` is outstanding** | — |

Anything else routes to an assessor **with the failing condition named**.

**Condition 3 named the wrong thing to v0.4, and the correction matters more than it looks.** It read *"no class-J and no class-E term raised one"* — and **`BR-TERM-7` establishes that a class-D term may carry `CHECKPOINT`**, which **two of the pilot's ten do**: screening and vaccination, and dietary supplements, **both `ACTIVE`**. Under the old wording a claim with an open supplement checkpoint **satisfied condition 3 and auto-approved with the judgement unmade** — the exact outcome the condition was written to prevent, on terms that were in the register when it was written. **The class is irrelevant to this test.** A checkpoint is a statement that a person must decide something; who raised it does not change that. **The gate now asks whether a judgement is outstanding, not who asked for it.**

**Condition 3's underlying argument is unchanged, and it is the class model's.** A class-J term is *by definition* one no engine may decide. If an open checkpoint did not block auto-approval, the platform would be auto-approving a claim **on which a judgement was required and never made** — precisely the failure mode the class distinction exists to prevent. It is the same argument as condition 2, applied to a policy term instead of a clinical opinion. What v0.5 fixes is that the argument was implemented against `termClass` when it should have been implemented against the checkpoint.

**Condition 4 is new at v0.5 and it closes a control that was inert.** `BR-DUP-2` raises *"a warning to the assessor"* — and **an auto-approved claim never reaches an assessor**. So the duplicate control, which §10.2 `GAP-1` records as **the most significant omission found in the reference material** and this system's answer to it, was **silently disabled on exactly the population that settles unreviewed**. It was not weakened; it was absent, while being counted as present. Condition 4 does not block the customer and does not change `BR-DUP-2` — it routes the claim to the assessor the warning is addressed to.

**The cost is real and is stated rather than discovered — and v0.4 stated it against the wrong figures.** **Any claim carrying an open checkpoint leaves the straight-through path.** v0.4 said *"three class-J exclusions"*; the register holds **two class J**, **two class D carrying `CHECKPOINT`**, and **one class E** — five terms that can raise a checkpoint, not three, and **the class-D pair is the high-volume half**. Screening, vaccination and dietary supplements are ordinary outpatient content; cosmetic treatment and substance-related conditions are a tail. **So the straight-through rate falls further than v0.4 implied, and it falls on routine claims rather than on unusual ones.** SC-04's median is measured across all claims and absorbs it; the straight-through *rate* does not. **The alternative is worse**: a gate that ignores an open checkpoint is a gate that auto-approves the claims most likely to be disputed. **What is worth taking from the arithmetic is not the rate but `B-23` and the register** — if screening and supplements are meant to be decided by a person on every claim, that is a product decision; if they are not, the terms should say so.

**Condition 1's second clause was added on 17/08/2026** after review found that a wholly declined claim has a payable of zero, which satisfies an amount test trivially — so the gate as first written could issue a **decline** with no human involved. It is one predicate, and it preserves two rules already agreed: §3.2.3 (a coverage failure is a contractual answer someone should see) and §4.5 (a decline is a decision that needs approving). The gate now reads **"small, clinically clean, nothing cut, no judgement outstanding, and not a possible duplicate"** — the fourth clause added at v0.5.

#### Why this is defensible, and why it is not "trusting the AI"

The reasoning matters more than the rule, because the obvious objection — *every claim goes through AI, and AI results need a human* — has a clean answer.

**A recommendation that reduces a payment needs a human. The absence of one does not.** When Track 2 returns nothing, the payable amount is the output of **Track 1 alone**: the deterministic rule chain in §5.3, reproducible from the same inputs and auditable step by step. **No AI judgement has entered the number.** Auto-approving that claim is auto-approving arithmetic, not a machine's opinion.

BR-TRACK-3 is therefore untouched — a recommendation still never alters an amount without confirmation. There simply is no recommendation.

#### The rules

| Rule | Statement |
|---|---|
| **BR-AUTO-1** | The threshold is master data, not code |
| **BR-AUTO-2** | Changing it requires a **second administrator's confirmation** and is audited with both identities (A3 §4.7). It is the most sensitive number in the Claim Platform: raising it silently approves larger claims with no human involved, and it is a single field edit |
| **BR-AUTO-3** | Condition 2 is satisfied **only when the assessment ran to completion** (`state = COMPLETE`) **and every cost line was assessed** (`linesAssessed` covers them all). An assessment that failed, timed out or skipped lines is **not** a clean result — BR-TRACK-5 |
| **BR-AUTO-4** | An approver may override an auto-approval after the fact |
| **BR-AUTO-5** | Every auto-approved claim records **all four conditions and the values they were evaluated against** — including the count of terms tested and checkpoints raised (BR-TERM-1), the term register version it was evaluated against (BR-TERM-4), and any duplicate signal considered and found not outstanding — so *"why was this approved without a human"* is answerable from the claim alone |
| **BR-AUTO-6** | **An auto-approved claim whose payout account is not verified routes to the operator queue** rather than to a payment instruction. `BR-PAY-3` blocks a settlement plan and a payment instruction while `accountNameVerified` is false; **auto-approval bypasses the settlement plan**, so without this rule an auto-approved claim would reach `APPROVED` and stop, with nothing naming why. **New at v0.5** |

**BR-AUTO-3 carries the weight.** An empty recommendation array and a failed assessment look identical to a naive check and mean opposite things.

**BR-AUTO-6 routes rather than gates, and that is the deliberate half.** The alternative was a fifth gate condition — *the payout account is verified* — and it was rejected. **The gate is about adjudication**: whether the contract paid this claim in full, at a small amount, with no judgement outstanding. Whether we know where to send the money is an operations question, and answering it inside the gate would mean a claim the contract settled cleanly is recorded as having *failed adjudication* because of a bank detail. **It matters practically as well as tidily**: `DE-12` means the verification call has no contract yet, so **`accountNameVerified` may be false on every claim in the pilot** — as a gate condition that would switch auto-approval off entirely and the measurement of SC-04 with it. As a routing rule the claim still auto-approves, still records that it did, and waits in a queue somebody works. **`APPROVED` with an unverified account is not an error state and is never expressed to the customer as one** — §4.3.

#### What this gate deliberately does not test

Recorded because these are choices, not oversights, and a reviewer will ask about each.

| Not tested | Consequence |
|---|---|
| **Extraction confidence** | A claim whose amounts were read with low confidence can auto-approve. The threshold limits the damage; it does not prevent it. Confidence stays a **routing** signal — it tells an assessor which field to look at (A4 §7) — but it is not a gate |
| **Post-hoc sampling** | No proportion of auto-approved claims is re-reviewed. A systematic false negative in Track 2 would surface as a customer complaint, not as a measurement |
| **Cumulative amount across a member's claims** — **declared at v0.5** | **The threshold is tested per claim, so three claims of 1,900,000 each pass a 2,000,000 gate and 5,700,000 settles with no human involved.** This is **S-12** — the reference computes approval authority on the **cumulative amount across all claim files for one incident**, and MVP 1 has one threshold, one level, checked per claim — and S-12's own reasoning applies here word for word: *per-claim thresholds are defeated by splitting an incident across claims.* §5.12 already records cumulative authority as deliberately not implemented; **what was missing is that the same hole is in the auto-approval gate, where there is no human at all rather than one human at one level.** The exposure is **declared here and not closed here**: a cumulative test is a business rule about how much a member may be paid unreviewed in a period, and that is a decision this document is not entitled to take — **`B-22`**. Bounded meanwhile by **BC-3** (no real money) and by the pilot's volume, and it should be **stated in the pilot's operating procedure** exactly as S-12 requires |

**Two further omissions were closed on 17/08/2026 by condition 1's second clause**, and the reasoning is worth keeping: a **coverage or waiting-period failure** produces a decline or a reduction, and a **bound limit** produces a cut. Both are now outside the gate, so neither can settle without a human. What remains is the narrow case the gate was meant for — a claim the contract paid in full, at a small amount, with nothing flagged.

**All three remaining omissions are bounded by BC-3 — no real money moves in MVP 1.** They stop being acceptable at the same moment S-17 does, and **the cumulative one stops being acceptable first**, because it is the only one of the three an ordinary claimant can exploit without knowing anything about how the system works.

**Auto-approval is genuinely novel here.** Neither reference system has it. Reference System A's nearest equivalent is an AI that pre-fills the settlement with a per-line **confidence indicator** shown as green / amber / red, plus a written rationale per line — a human still approves everything. That confidence-per-line presentation is worth adopting in the assessor workbench regardless (§6.2).

### 5.9 The limit ledger

**Restated at v0.4 to cover all three counter types.** To v0.3 every rule below governed **money and nothing else**, which was a defect rather than a scope choice — §5.9.2 sets out why. Wherever a rule says *consumption*, it means consumption of **any** counter type: `AMOUNT`, `OCCURRENCE` or `DAY`.

| Rule | Statement |
|---|---|
| **BR-LEDGER-1** | Consumption is **two-phase**: provisional at calculation, **final at approval**. **All three counter types**, on the same two phases |
| **BR-LEDGER-2** | Provisional consumption is **invisible to other in-flight claims**; final consumption is visible immediately. **All three counter types** |
| **BR-LEDGER-3** | Consumption is **never deleted** — only reversed or offset. **A day and an occurrence reverse exactly as an amount does**; there is no delete on any counter |
| **BR-LEDGER-4** | A failed payment posts a **reversal**. It does not un-finalise. Superseded in coverage by **BR-LEDGER-7**, which states every transition rather than this one |
| **BR-LEDGER-5** | Counters carry a version for optimistic concurrency. **Per counter, and therefore per `counterType`** — an `AMOUNT` counter and a `DAY` counter on the same benefit version independently |
| **BR-LEDGER-6** | A single payment posts to **every counter on the ancestor path** under **BR-TREE-1**, with BR-TREE-2 applied. **All three counter types roll up the same path** |

**Why a ledger and not a live query.** Reference System A holds no accumulator record at all. Remaining limit is computed on demand as a query over payment lines across *all claims of the same insured that are not cancelled*, matched on the composite `name + date of birth + ID number`. It works, and it has three properties we deliberately avoid:

- **Everything consumes, including drafts.** A claim being typed reduces the limit visible to another assessor. That is over-conservative but at least safe.
- **The only way to release limit is to cancel the claim.** Closing does not. If a claim is abandoned without cancellation, the limit leaks permanently.
- **The identity match is a composite of three text fields.** The same person under two policies or two member records double-consumes; a person whose name is recorded differently is wrongly blocked.

Our ledger is keyed on **the member identity InsureMO issues** (`memberId`, F05), posts explicitly, and reverses explicitly. **The ledger itself is entirely ours** — exclusively so from v0.3, by decision rather than by discovery (B-17, P-04): InsureMO holds the *shape* of the benefit tree, it accumulates nothing we read or write, and whatever consumption it may track internally is not part of this design. **But the failure mode Reference System A avoids by accident is one we must handle deliberately:** two claims for the same member adjudicating simultaneously against the same annual limit is a real race, not a theoretical one, and BR-LEDGER-5 is the answer.

**Why final at approval and not at payment.** An approved claim has committed the limit. A second claim adjudicating at the same moment must see that commitment, not a provisional figure that might evaporate. Deferring finalisation to payment would let two approved claims jointly exceed an annual limit — and both would be legitimately approved.

#### 5.9.1 The counter period — what "annual" means

A5 specifies an **annual** sum insured. **To v0.3 nothing in this document, or in D2, defined what that meant operationally**: policy year or calendar year, which date puts a cost line into a period, and what happens to a claim whose lines fall on both sides of a boundary. Three unanswered questions behind every limit in the product.

| Rule | Statement |
|---|---|
| **L-1** | The counter period is the **policy year**, derived from the **policy inception date on the snapshot** — **not** the calendar year |
| **L-2** | A cost line posts to the period containing its **treatment date**, **never** its submission date |
| **L-3** | A claim whose lines **span a period boundary posts to two periods, line by line**. One claim, two sets of counter postings |
| **L-4** | **`periodKey` is stored on every posting and is immutable once posted** |

**L-2 is the one that would have been got wrong.** Keying on submission date would let **a member reset their own limit by delaying a claim**, and would make the same treatment consume different periods depending on how fast the customer filed. Treatment date is the date the loss occurred; it is the only date the member cannot choose.

**L-3 has a visible consequence and it reaches the screen.** When a claim spans a boundary, **the remaining-limit figure at calculation is not a single number** — it is one figure per period, and an assessor shown a single number is being shown something that does not exist. **The console must display remaining limit per period** (D2 `S-11`). It is a small screen change and a large correctness gain.

**L-1 is what makes L-3 common rather than exotic.** On a policy-year basis every member's boundary falls on a different day, so a boundary-spanning course of treatment is an ordinary event rather than a December edge case.

> **L-1 contradicts §3.2.1 and `BR-WAIT-1`, and the contradiction is left standing at v0.5 because resolving it is a product decision — `B-21`.** §3.2.1 argues at length that **the member's own cover period is the right basis and the policy's is wrong**, because on a group policy members join and leave mid-term; `BR-WAIT-1` measures every waiting period from **the member's coverage start date, not the policy inception date**. **`L-1` then keys the counter period on the policy inception date, with no reasoning given.** The pilot's own seed has mid-term joiners — a spouse added two months into an individual policy year, an employee added three months into a group one — so this is the ordinary case, not an edge.
>
> **What it costs, precisely.** A member joining nine months into a policy year receives **a full annual sum insured for those nine months, and another full one from the anniversary** — **two annual limits inside twelve months of cover**. On the pilot product's outpatient benefit that is 40,000,000 of cover where the product sells 20,000,000.
>
> **Why this document does not simply fix it.** The obvious repair — key the period on the member's coverage start, as everything else does — is **not free either**: it gives every member a personal limit year, so a member's annual cap and the policy's renewal date diverge, and a group scheme's aggregate exposure stops aligning with its policy term. **And whether a mid-term joiner's annual limit is pro-rated is a term of the product, not a property of the ledger.** Both candidate rules are set out under **`B-21`**. **Until it is decided, `L-1` stands as written and the exposure is real**, bounded only by BC-2 and BC-3.

#### 5.9.2 Counter types — money, occurrences and days

**This is a defect being closed, not a feature being added, and it should be recorded as one.** Three places in the live specification already assume non-monetary counters:

- **`benefit_schedule_line`** carries `count_limit`, `per_day_cap` and `per_event_cap` — on **all 51 rows** of the pilot product;
- **Chain step 9** (§5.3) tests *"days or visits remaining, `0` means unlimited"*;
- **D2's limit-ledger screen `S-11`** displays *"days paid"* and *"remaining days"*.

**And `BR-LEDGER-1…6` governed money alone.** So the chain tested a counter **no rule maintained**, and the console displayed a figure **no rule produced**. A screen showing *"remaining days"* with nothing behind the number is worse than no screen, because an assessor will believe it.

**The counter key carries a type.**

```
counterKey  = (memberId, policyId, benefitCode, periodKey, counterType)
counterType ∈ { AMOUNT, OCCURRENCE, DAY }
```

| Type | Increments by |
|---|---|
| `AMOUNT` | The money posted |
| `OCCURRENCE` | **One, per distinct *visit*** — **restated at v0.5**, see below. It was *"one per qualifying event"*, which cannot enforce a visit limit |
| `DAY` | **One, per distinct calendar date, per counter** — **restated at v0.5** (**BR-LEDGER-12**). It was *"the treatment-day count of the line"*, which double-counts a date carrying two lines |

**All of `BR-LEDGER-1…6` apply to all three types** — two-phase, never deleted, ancestor path under **BR-TREE-1**, versioned for optimistic concurrency. That is the whole fix: no new mechanism, one word of scope in six existing rules, and **BR-TREE-3 stops being a statement the ledger cannot honour.**

**The counter key is deliberately plan-independent, and that is a requirement on the product catalogue rather than a property of the ledger.** `benefitCode` appears in the key; `planCode` does not. §5.9.5 relies on this — consumption carries when a member moves to a higher plan — and **it works only because the pilot product's three tiers share benefit codes**: `200.1` means *consultation and diagnostic tests* on all three, and only the figures differ. **`BR-LEDGER-13` states the obligation that keeps it true.** If a future product tier renamed or renumbered a benefit, an upgraded member's consumption would land on a different counter and their limit would silently reset — the same failure §5.9.5 exists to prevent, arriving through the catalogue instead of through the ledger.

**The visit, and why `OCCURRENCE` could not enforce a visit limit as written.** §5.1 defines an insured event as **one provider, one date range, one diagnosis set** — so a **twelve-session course of physiotherapy is one insured event**. Benefit `200.3` carries `count_limit: 20, count_unit: 'VISIT'` and holds its per-visit cap in `per_event_cap`. Under v0.4's *"one per qualifying event"*, that course **increments the counter by 1 against a limit of 20**, and the per-visit cap of 500,000 is **applied once to the whole course** instead of twelve times. The same defect sits on `200.1`, `200.2` and `300.1`, which is **every benefit in the product expressed in visits**. So a visit limit counted courses and a per-visit cap capped courses, and both did it silently.

**The visit is introduced as a derived unit — `BR-LEDGER-14`.** A visit is **a cost line's `treatmentDate` together with its provider**; lines sharing both are one visit. **It is derived, not captured**: both facts are on every cost line already (A4), nothing is asked of extraction, and no new field enters the model. `OCCURRENCE` increments **once per distinct visit**, and §5.3 step 8 applies a per-visit cap **per visit** for any benefit whose `count_unit` is `VISIT`.

> **The alternative was to declare visit limits unenforceable in MVP 1, and it was rejected.** That would have been the honest answer if the facts were missing — it is the answer this document gives for `DAY` counters where a treatment-day count is genuinely absent (**Q-IDP-06**). **But the facts are not missing here.** Date and provider are on every line, the derivation is two fields and an equality, and the alternative would leave **four of the pilot's benefits carrying limits the engine does not enforce and a console displaying "visits remaining" figures nothing maintains** — which is the precise defect §5.9.2 was opened to close, reproduced one level down. **A derived unit whose inputs are already captured is not a new capability.** What it does carry is a definition someone must agree with: two visits to the same provider on the same day count as one. That is the conventional reading of a per-visit cap and it is stated here so that it is a decision rather than an implementation detail.

**Two consequences to carry outward.** The key now has five parts rather than four, so **D2's `V-723` — the counter uniqueness key, member × benefit × policy number — gains `periodKey`** and is no longer unique without it. And **`DAY` depends on a treatment-day count per line, which extraction does not return today**: it is raised as **Q-IDP-06**. Until it is answered, `DAY` counters can be maintained only where the treatment date range on the line makes the count derivable, and a benefit whose limit is expressed in days is not fully enforceable — which is an honest statement of where the pilot product stands, not a reason to leave the rules money-only. **From v0.5 a line whose day count cannot be derived is held under `BR-CHAIN-1` rather than passed**, so the gap costs a checkpoint instead of an unenforced limit.

**Three rules follow, all new at v0.5.**

| Rule | Statement |
|---|---|
| **BR-LEDGER-12** | **A `DAY` counter is incremented once per distinct calendar date per counter, regardless of how many lines fall on that date.** `BR-TREE-2`'s no-double-counting rule is written for **the same line** and does not reach this case: on an **intensive-care-to-ward transfer day** a claim carries a `100.1.1` line *and* a `100.1` line for the same date, and `100.1`'s counter takes **+1 directly and +1 rolled up under BR-TREE-1** — two days consumed for one day in hospital, against a 60-day limit |
| **BR-LEDGER-13** | **The counter key is deliberately plan-independent** — `benefitCode`, not `planCode` — which is what lets §5.9.5 carry consumption across a plan change. **Benefit codes must therefore be stable across the tiers of a product**: the same code means the same benefit on every tier, and only the figures differ. A tier that renumbers a benefit silently resets an upgraded member's consumption |
| **BR-LEDGER-14** | **The visit is a derived unit: a cost line's `treatmentDate` together with its provider.** Lines sharing both are **one visit**. An `OCCURRENCE` counter increments **once per distinct visit**, and a per-visit cap (§5.3 step 8) applies **per visit** on any benefit whose `count_unit` is `VISIT`. Nothing new is extracted — both facts are already on every cost line |

#### 5.9.3 BR-LEDGER-7 — the complete posting table

**`BR-LEDGER-4` covered exactly one transition: payment fails.** Eight others either post to the ledger or deliberately do not, and to v0.3 every one of them was left to the implementer's inference.

| Rule | Statement |
|---|---|
| **BR-LEDGER-7** | **Every state transition either posts to the ledger or explicitly does not.** The table below is the complete list; nothing is left to inference |

| Transition | Ledger effect |
|---|---|
| Calculation completed | **Provisional** posting |
| Calculation **recalculated** | **Reverse** the prior provisional; **post** the new one |
| Claim **withdrawn or declined before approval** | **Reverse** the provisional |
| **Approval** | Provisional **reversed**, **final** posted |
| **Payment fails, and the claim returns to an assessor** | **Reversal** posted (BR-LEDGER-4, **narrowed at v0.5 by BR-LEDGER-11**). **The claim does not un-finalise** |
| **Payment fails, and is retried on the same instruction** | **No posting at all** — **new at v0.5, BR-LEDGER-11.** The claim has not stopped consuming the limit merely because the bank rejected the transfer |
| **Claim re-opened after approval** | Final posting **reversed**; a **fresh provisional** posted on recalculation. **Not reachable in MVP 1 — S-13**, stated so the ledger's shape is right |
| **Approved amount amended before payment** | **Reverse the final; post the corrected final. Both are retained.** **Not reachable in MVP 1** — no path exists to amend an approved amount: the claim is locked (§3.2.5) and `CANCELLED` is unreachable after approval (§4.6). **Stated for the same reason as the row above**, and marked at v0.5 so a reader does not go looking for the screen |
| Claim **cancelled after final posting** | **Reverse the final.** The claim closes with a **net-zero posting pair**. **Not reachable in MVP 1** — §4.6 makes `CANCELLED` unreachable from `PENDING_APPROVAL` or later, so nothing can be cancelled once anything is finally posted. **Marked at v0.5** |
| **`PAID` → any** | **No further postings.** A change after payment is a business matter — **S-40** |

**Three of the nine rows describe transitions the state machine does not permit, and all three are now marked as such.** To v0.4 only *claim re-opened after approval* carried the note; *approved amount amended before payment* and *cancelled after final posting* read as live paths, and **both are forbidden** — the first has no screen and no state edge at all, the second is refused by §4.6. **They stay in the table**, because the point of `BR-LEDGER-7` is that **every transition either posts or explicitly does not, with nothing left to inference**, and a table that quietly dropped the impossible ones would leave an implementer to guess what happens if a path is opened later. What they must not do is imply that MVP 1 has a mechanism it does not have.

**Three rows had no rule behind them at all before v0.4**, and all three are transitions a live system meets in its first months: *claim re-opened after approval*, *approved amount amended before payment*, and *approval itself* — the last of which was implied by BR-LEDGER-1 but never stated as a pair of postings. Specifying them costs nothing now and retrofitting them later would cost a migration of every posting.

**Two further rules travel with this table, both new at v0.5.**

| Rule | Statement |
|---|---|
| **BR-LEDGER-10** | **A reversal inherits the original posting's `periodKey`, `counterType` and cap-as-at-posting.** It is posted against the counter the original consumed, not against the counter that is current when the reversal is made. **Without this the pair does not net**: a reversal made after a period boundary would land in the new period, leaving the old one permanently consumed and the new one credited with a payment it never carried — and §5.9.5's requirement that a settlement stay reproducible from its own history would fail on the first boundary-spanning claim (L-3 makes those ordinary, not rare) |
| **BR-LEDGER-11** | **A payment failure posts a reversal only where the claim returns to an assessor. A failure retried on the same instruction posts nothing.** `BR-LEDGER-4` posted a reversal on every payment failure while stating that the claim does not un-finalise — **but a reversal *is* a release**. A wrong account number retried the next morning would free the member's annual limit overnight, and a second claim adjudicating in that window would be assessed against a limit the first claim is still consuming. **The distinction is whether the claim has stopped being a committed settlement**: it has when it goes back to an assessor, and it has not when the same instruction is being resent |

**The `PAID` row is the boundary, and it is the same boundary as BR-TERM-6.** Nothing after payment touches the ledger, because correcting a settled claim means money moving in the other direction and MVP 1 has no mechanism for that.

#### 5.9.4 BR-LEDGER-8 — the approval race

Two claims for the same member reaching approval at the same instant is a **real race, not a theoretical one** — it is exactly what the ledger exists to survive. `BR-LEDGER-5`'s version check makes one of the two **fail and retry**.

| Rule | Statement |
|---|---|
| **BR-LEDGER-8** | **The retry must re-run adjudication, not re-post the figure it computed the first time.** The counter it lost the race to has moved, so the payable amount it computed is stale. Re-posting the old figure **breaches the limit by a race** — two claims each individually within the limit, jointly over it, both legitimately approved |
| **BR-LEDGER-9** | **Where the re-run produces a different payable, the approval is void — new at v0.5.** A **manually approved** claim returns to **`PENDING_APPROVAL`**, with the movement shown — the amount approved, the amount now payable, and the counter that moved — and **requires fresh approval**. An **auto-approved** claim is **re-tested against every condition of the gate** on the new figure, and **routes to an assessor where any of the four now fails** |

**`BR-LEDGER-9` is the half `BR-LEDGER-8` stopped one step short of, and it is the half a customer would notice.** `BR-LEDGER-8` correctly requires the losing claim to re-adjudicate — and then leaves the new number to be posted **under an approval that was given for a different one**. On the manual route that is an approver's signature attached to an amount they never saw, which is the same hazard §3.2.5's claim lock exists to prevent, arriving through the ledger instead of through the assessor. On the automatic route it is worse rather than better: the re-run happens because a limit moved, so the new figure is **lower**, and a lower figure can newly bind a limit — which fails **condition 1** of the gate, the clause that keeps a cut claim in front of a human. **A claim that auto-approved at the first figure may not auto-approve at the second, and nothing in v0.4 re-asked the question.**

**This is stated as a rule because it is the kind of thing an implementer optimises away.** Retrying a failed write by replaying the same value is the obvious implementation and it is wrong here: the value is a function of the counter the write just collided with. `BR-TRACK-4` already requires the chain to be re-runnable and `BR-TERM-3` already requires it to be deterministic, so the re-run is cheap (NFR-3: ≤2 s) and produces the only defensible number.

**One presentation rule travels with it.** The remaining limit shown to an assessor during calculation is **final consumption only** (BR-LEDGER-2), and **it must be labelled as such on screen**. An assessor who reads it as *"available"* will be surprised when a concurrent claim approves first — one line of label, and a material reduction in support calls.

#### 5.9.5 Mid-term change of benefit level

An endorsement raises a member's sum insured mid-year, or moves them to a higher plan. **Does consumption already recorded carry forward against the new cap?**

**Consumption carries; the cap changes.** A member who has used **3m of a 10m limit** and is upgraded to **20m** has **17m remaining, not 20m**. The ledger therefore stores **the cap as at the posting**, alongside the consumption, so the history stays explicable — without it, a settlement computed under the old cap cannot be reproduced once the new one is in force. **A reversal of one of those postings inherits the cap it was made under, not the current one — `BR-LEDGER-10`.**

**This rule works because the counter key carries no plan dimension, and that is a dependency rather than a coincidence.** Consumption carries across a plan change only if the upgraded member's postings land on the **same counter**, which they do because the key is `benefitCode` and not `planCode`, and because the pilot product's three tiers **share benefit codes**. **`BR-LEDGER-13`** states the obligation that keeps it true for the next product.

> **This rule depends on an unanswered vendor question, and it cannot be implemented as written until that question is answered — C-IMO-14.** Does a mid-term endorsement in InsureMO produce a **new `ruleSetRef` or a new benefit-schedule version**, or does it **change fields silently on the existing one**? If it produces a version, the snapshot sees the change and the rule works as stated. **If it changes fields silently, the snapshot cannot detect the change at all** — a claim registered after the endorsement would carry the new cap with no record that anything moved, and a claim registered before it would carry the old cap with no way to know it was superseded. **The rule is not implementable in the silent case**, and that is a design consequence rather than a configuration detail. It is carried as dependency **DE-13**.

Endorsements are out of scope for MVP 1 (Framework §7.2), so nothing in this release exercises the rule. It is specified now because **the ledger's storage shape depends on it** — retrofitting a cap onto historical postings is not possible.

### 5.10 The two assessment tracks

Everything above describes one way of deciding what to pay. **There are two, and they run at the same time on the same claim.** This section is the design; §5.11 sets out what it costs.

#### 5.10.1 Why one track is not enough

The rule chain in §5.3 answers questions with a determinate answer: *is this member covered, has the waiting period been served, has the annual limit been reached, does this benefit have a per-visit cap*. A typical policy condition is exactly this shape — outpatient limited to three visits a year, 2,000,000 per visit, 5,000,000 across the year — and the model already expresses it without a new mechanism: `maxCount: 3, countUnit: VISIT` with `perEventLimit: 2000000` and `annualSumInsured: 5000000` on one node of the benefit tree.

It cannot answer a different class of question at all: **is this drug appropriate for this diagnosis? Is this laboratory test related to the condition being treated?** Those require medical knowledge, they are open-ended, and no achievable rule set covers them. A rule base large enough to try would be unmaintainable and would still be wrong at the edges — which is precisely where claims are disputed.

**So there are two tracks, with different natures and different authority:**

| | **Track 1 — Policy rule assessment** | **Track 2 — Clinical appropriateness assessment** |
|---|---|---|
| Question | Does the contract pay for this? | Is this item medically appropriate for this diagnosis? |
| Runs on | **Claim Platform** | **IDP** |
| Method | Deterministic rule chain (§5.3), configured from the benefit tree | AI over medical knowledge, plus a curated knowledge base |
| Input | Cost lines, benefit tree, ledger, member data | Diagnosis, prescription lines, test lines, cost lines |
| Output | **A decision.** A payable amount, computed and reproducible | **A recommendation.** A proposal to decline a line, with a rationale and a confidence |
| Authority | Binding. The engine's number *is* the answer | **None until a human confirms it** |
| Reproducible | Yes — the same inputs always give the same number | No. The same claim may produce different wording, and the model improves over time |
| Explainable | By naming the rule and the parameter | By the rationale text, which is itself generated |

**The asymmetry in the last three rows is the whole point.** A deterministic rule can be trusted because it can be re-run and audited. A clinical judgement cannot, so it never decides anything on its own.

#### 5.10.2 The sequence

```
                    ┌── Track 1 ── Claim Platform ──────────────────────┐
Extraction  ────┬──►│ Rule chain §5.3, all lines assumed appropriate    ├──┐
(I-07/08/09)    │   │ → provisional payable amount                      │  │
                │   └──────────────────────────────────────────────────┘  │
                │                                                          ▼
                │   ┌── Track 2 ── IDP (I-15/I-16) ────────────────────┐  Assessor
                └──►│ Clinical appropriateness over the same lines      ├──► reviews
                    │ → recommendations, each with rationale + confidence│  and confirms
                    └──────────────────────────────────────────────────┘   │
                                                                            ▼
                                            Rule chain §5.3 RE-RUN with confirmed declines
                                                                            │
                                                                            ▼
                                                                    Settlement plan
```

| Rule | Statement |
|---|---|
| **BR-TRACK-1** | Both tracks are triggered by the same event — a completed extraction — and **run concurrently**. Neither waits for the other |
| **BR-TRACK-2** | Track 1's first pass treats every cost line as **clinically appropriate**. It produces a payable amount that is provisional in exactly one respect: it has not yet been reduced by any confirmed clinical decline |
| **BR-TRACK-3** | A Track 2 recommendation **never alters an amount**. It is attached to a cost line, and it waits |
| **BR-TRACK-4** | When the assessor confirms a recommendation, the line's decline is applied and **the rule chain is re-run in full** — not patched. Limits, ancestors, co-payment and the deduction ratio all recompute against the reduced base |
| **BR-TRACK-5** | If Track 2 fails or times out, the claim proceeds on Track 1 alone with a warning to the assessor. **A clinical assessment that did not run is never treated as a clean result** |
| **BR-TRACK-6** | The claim records, per line: the recommendation, its rationale, its confidence, the assessor's decision, and whether that decision agreed with the recommendation |

**BR-TRACK-4 is what keeps the settlement reproducible.** Patching a confirmed decline into an already-computed settlement would leave the arithmetic dependent on the order the assessor clicked in. Re-running is cheap (NFR-3: ≤2 s) and it means the final number is always the output of the same deterministic chain given the same confirmed inputs.

**BR-TRACK-6 is what makes the AI improvable.** The agreement rate between recommendation and assessor decision is the only real measure of whether the clinical assessment is good enough, and it is the input to the knowledge base's feedback loop (§5.10.5).

#### 5.10.3 What a recommendation is

| Field | Meaning |
|---|---|
| Cost line | The line it applies to. **A recommendation is never claim-level** |
| Proposed reason code | `NOTREL` (not related to the diagnosis) or `NOTCOV` (not a covered medical item) — the two line-only codes in A1 §5.2 |
| Rationale | Human-readable, in Vietnamese, naming the item, the amount and the clinical reason |
| Confidence | A score, and a band: high / medium / low |
| Evidence | What the recommendation was based on: the diagnosis relied on, and the knowledge-base entries cited |

**The evidence field is not optional.** An assessor being asked to decline a customer's medication needs to see *why*, and "the model said so" is not a basis on which a person can take responsibility for a decision. It is also what a customer's complaint will eventually be tested against.

#### 5.10.4 Where the human sits — and where it does not need to

**The rule, stated once: a recommendation is confirmed by a person before it changes a settlement. An absence of recommendations is not a recommendation.**

| Track 2 returned | Who decides |
|---|---|
| One or more recommendations | **An assessor confirms or rejects each one** (F56), and the rule chain re-runs on what was confirmed — BR-TRACK-4 |
| Nothing, having assessed every line | Nobody needs to. The payable amount came from Track 1's deterministic chain alone, and it may **auto-approve** if it is also small, **no term checkpoint is open** and **no duplicate signal is outstanding** — §5.8 |
| Nothing, because it failed or skipped lines | **Not a clean result.** The claim goes to an assessor with `CLM-CONSOLE-015` — BR-TRACK-5 |

**This is the resolution of what looked like a contradiction.** An earlier draft concluded that because every claim passes through the clinical assessment, and clinical results need human confirmation, auto-approval could never fire. That reasoning conflated two different things: *a machine's judgement about a claim* and *a machine having no judgement to offer*. Only the first needs a person.

**What is therefore never automated:** any reduction proposed by AI. **What is:** a small, deterministic settlement on which AI raised nothing.

#### 5.10.5 The knowledge base — and where the boundary of this project now sits

**Decided 17 August 2026 (B-13, B-15).** The clinical track runs at **full breadth** — drugs *and* laboratory and imaging appropriateness — and **the clinical content is IDP's scope, not the Claim Platform's**. The IDP team builds and owns the knowledge base, sourced from public medical material. The Claim Platform **consumes recommendations through I-15/I-16 and builds no clinical capability of its own**.

**This is a clean division and it has one real consequence, which is recorded here rather than discovered later.**

| What it settles | What it does not settle |
|---|---|
| **Ownership.** F37 and F38 are IDP deliverables with a named team. The effort leaves the Claim Platform's critical path entirely | **Clinical assurance.** Public sources are not reviewed for this market, this product's exclusions, or Vietnamese drug naming. Nobody in the project is in a position to vouch for a clinical judgement |
| **The interface is the whole contract.** I-15/I-16 is now the complete specification of what the Claim Platform expects, and it is already written | **Accountability.** When a customer disputes a clinical decline, the answer *"our document vendor's knowledge base said so"* is not one an insurer can give. The decline is the insurer's decision |
| **Effort and timeline** move to the IDP work stream, where C-IDP-01 must now cover F37 and F38 as well as extraction | **Whether public-source knowledge is good enough** to act on. Only measurement answers this — BR-TRACK-6's agreement rate |

**Three controls therefore carry more weight than they did**, and none of them is expensive:

| Control | Why it matters more now |
|---|---|
| **BR-KB-2 — every recommendation cites the entries it relied on** | With no clinical reviewer, the evidence trail is the *only* thing standing between a recommendation and an unexplainable decline |
| **BR-TRACK-6 — record whether the assessor agreed** | The agreement rate is now the sole empirical measure of whether public-source knowledge is adequate. It is also the earliest warning if it is not |
| **BR-TRACK-3 — a recommendation never alters an amount** | A human confirming each one is what keeps the insurer, not the knowledge base, the decision-maker |

> **B-14 was closed on the same day, and its answer is what carries this risk.** A recommendation produced from unreviewed public material, confirmed by an assessor who is not a clinician, and expressed to a customer as a reason their medicine was not paid for, needed settled wording and a settled escalation route *before* it met a customer. Both are now fixed (§5.10.6, and the information-request route in Part XI B-14). **What remains unmitigated is the absence of clinical review behind the knowledge base itself** — Framework RK-08 and RK-09.

**Two things the IDP team should be asked for, and they are cheap:** the **provenance** of the knowledge base — which public sources, at what date — and a **named reviewer**, even one who is not a doctor, who is accountable for what goes in. Neither is a clinical sign-off. Both are the difference between an artefact someone owns and one that accumulated.

#### The knowledge base itself

The clinical assessment is only as good as what it knows, and general medical knowledge is not enough — it does not know this insurer's exclusions, this market's drug names, or the patterns this book of business actually sees.

| Rule | Statement |
|---|---|
| **BR-KB-1** | The knowledge base is a **curated, versioned artefact** with a named owner. It is not model weights and it is not a prompt |
| **BR-KB-2** | Every recommendation cites the entries it relied on (§5.10.3) |
| **BR-KB-3** | Assessor disagreements are the **primary feedback signal**. A pattern of overrides on the same entry is a defect in the entry |
| **BR-KB-4** | A change to the knowledge base is **versioned**, and every claim records the version in force when it was assessed |
| **BR-KB-5** | The knowledge base contains **no patient data**. It is clinical and product knowledge only |

**BR-KB-4 exists for the same reason the policy snapshot does.** Without it, a claim cannot be explained six months later, because the knowledge that produced the recommendation has moved.

**BR-KB-1 is the item with the longest lead in this whole design, and it should be planned like the ground-truth set.** Curating clinical appropriateness rules for the Vietnamese market needs medical input the project does not currently have. See §5.11 and RK-08.

#### 5.10.6 Five things this design must not become

| Not this | Why |
|---|---|
| **An AI that declines claims** | It proposes; a person decides. The moment a recommendation is applied without confirmation, the insurer has automated a medical judgement it cannot explain or defend |
| **A Track 1 feature wearing a Track 2 badge** | If clinical appropriateness reduces to a lookup table, it belongs in the policy rule assessment, where it is deterministic and auditable. Track 2 exists precisely for what a table cannot hold |
| **A decline the customer cannot understand** | BR-LINE-1 already requires the rationale to name the item and the amount. For a clinical decline it must also give the reason in language a patient can read — not an ICD code and a confidence score |
| **A judgement on the treating doctor** | **Decided 17/08/2026 (B-14).** A clinical decline is expressed as a **statement about cover**, never as an assertion that a prescription was wrong. *"Khoản này không thuộc phạm vi chi trả cho chẩn đoán đã ghi nhận"* — not *"thuốc này không phù hợp với chẩn đoán"*. The first is the insurer's to say; the second is a clinical opinion about someone else's practice. A2 §4 carries the approved wording |
| **Attributed to the AI** | The customer is **never told the reason came from a model.** A human confirmed it, so the decision is the insurer's, and saying otherwise is both inaccurate and an invitation to ask for the human review that already happened. The attribution is recorded internally (BR-TRACK-6), never externally |

> **Settled on 17/08/2026 (B-14), and worth restating because it is the sharpest exposure in Part V.** A statement that a prescribed medicine was inappropriate for a diagnosis is close to a clinical opinion, made by an insurer about a treating doctor's decision. The payment decision is the insurer's; the clinical characterisation of someone else's prescribing is not. **The resolution is the wording rule in the table above** — a decline states what is covered, never that a prescription was wrong — **and the escalation route through the information-request flow** (Part XI B-14). **The legal deferral in Framework §9.4 does not cover this** — it addresses personal data, not clinical judgement — so B-14's answer, not §9.4, is what carries it.

### 5.11 What it costs, and who carries it

Following B-13 and B-15, the clinical track is built at **full breadth** and the clinical work sits with **IDP**. The split of effort is therefore no longer a scope question for this project; it is a dependency.

| Item | Owner | Effort | Note |
|---|---|---|---|
| **F37** Clinical appropriateness service — drug↔diagnosis *and* test↔diagnosis reasoning, Vietnamese rationale generation, confidence calibration | **IDP** | Substantial | Now covered by **C-IDP-01**, which must be re-scoped: its original wording covered document templates only |
| **F38** Knowledge base — curation, versioning, evidence citation, the override feedback loop | **IDP** | Substantial, and the **longest-lead item in the clinical track** | Built from public sources (B-13). See §5.10.5 for what that does and does not settle |
| **F56** Recommendation review — per-line recommendation, evidence, confirm/reject with reason, rule-chain re-run | **Claim Platform** | Days | The only clinical-track work on our critical path |
| **I-15 / I-16** contracts and mocks | **Claim Platform** | Small | Already specified (D3 v2.3 §I-15, §I-16). The mock lets F56 be built and demonstrated before the knowledge base exists |
| Rule-chain re-run on confirmation | — | **None** | Already required to be deterministic and re-runnable |
| Agreement-rate measurement | **Claim Platform** | Small | BR-TRACK-6. Now the only empirical check on the knowledge base |

**What this changes for the timeline.** The Claim Platform's clinical-track work is **F56 plus two adapters** — days, not weeks. **RK-10 has moved rather than closed**: the schedule risk is now IDP's, and it is real, because F37 and F38 together are larger than the extraction work IDP was originally asked to size.

**What must not be traded away**, whatever the delivery pressure:

| | Rule | Why it cannot be retrofitted |
|---|---|---|
| 1 | **BR-TRACK-3** — a recommendation never alters an amount | The moment it does, the insurer has automated a medical judgement it cannot defend |
| 2 | **BR-TRACK-6** — record whether the assessor agreed | Without it there is no way to know whether the knowledge base works, and no signal when it stops working |
| 3 | **BR-KB-4** — version the knowledge, record the version on the claim | A claim assessed today is unexplainable in six months once the base has moved |

All three are cheap now. All three are what make the feature safe to keep.

**If delivery pressure does arrive**, the reduction that costs least and preserves most is **advisory mode**: recommendations shown to the assessor and recorded, but not wired into the settlement. Everything is measured, nothing depends on it, and F56's re-run behaviour and the whole of §5.10.6 defer with it. Recorded here so that it is a known option rather than an improvisation.

### 5.12 Rules deliberately not implemented

| Rule in the reference | Why not here |
|---|---|
| **Congenital-condition exclusion** (ICD chapter Q → force all amounts to zero; multiple diagnoses including a Q code → pay but warn) | The pilot product does not exclude congenital conditions. The **mechanism** — a diagnosis-code-driven exclusion that can zero a line — is what step 4 provides, and from v0.4 it is a **class-D term in the register** keyed on ICD chapter Q rather than a hard-coded branch (§5.3.1). This is the worked example of how it would be configured, and it depends on line-level ICD — **Q-IDP-02** |
| **Disability and surgery percentage tables** (`Tỷ lệ thương tật/ phẫu thuật`, with a four-step escalating lookup into an insurer table then a Ministry of Health circular, and a confidence flag when nothing matches) | Death, permanent disability and lump-sum benefits are out of scope, and A5 carries **indemnity benefits only**. No benefit in the pilot product is payable "by scale" |
| **BHYT coordination** — deducting the social-insurance share before the private benefit applies | **Decided 17/08/2026 (B-05): captured, not deducted, in MVP 1.** A4 extracts `BHYT chi trả` and `Người bệnh cùng chi trả` per cost line; the engine ignores them. Where the step would go and what it costs is set out in §5.6a. Declared simplification **S-17** |
| **Currency and FX** | VND only. Reference System A carries an FX rate per claim and per event, and evaluates authority thresholds in VND — relevant only when travel or expatriate products arrive |
| **Co-insurance, reinsurance and third-party recovery splits** | Out of scope. The reference's five-case payment matrix is substantial and none of it applies to a single-insurer pilot |
| **Cumulative authority across claim files** | MVP 1 has one threshold and one approval level. See §10.1 S-12 for what this costs — and **§5.8, where the same hole sits in the auto-approval gate and there is no human at all**. Whether a cumulative limit on unreviewed settlement is needed is **`B-22`** |

---

## Part VI — Functional requirements

### 6.1 Traceability

All **53** features are defined in Framework §7.1 and are not restated. This part states the **business rules** attached to the features that carry them; a feature not named here is adequately specified by its Framework line plus its annex.

| Group | Features | Where the business rules live |
|---|---|---|
| A — InsureMO services | **F01–F07** | §3.1 for the buy journey; **§3.2 J2-7 and §7.2 for F05 and F07**, which is where every claim reads them. Capability is confirmed; **contract detail is not** — C-IMO-03, C-IMO-04, C-IMO-05 |
| C — Customer app | F21–F30 | §3.2 Phase 1 |
| D — IDP | F31–F38 | A4, §6.3, and **§5.10 for F37 and F38** |
| E — Claim console | **F41–F60** | §6.2, §4, Part V, **§5.10 for F56**, **§5.6 for F57** — the provider master the deduction-ratio condition reads — **§3.2.2 for F58**, the payee and payout-account master, **§3.3 for F59**, the group bulk load, and **§6.2 step 1 and §7.2 for F60**, policy and cover enquiry from a claim, which is the pinned snapshot view an assessor works from. **The range is corrected at v0.5**: it read F41–F59 while the blockquote below counted 20 features in the group and named F60 as one of them |
| F — Cross-cutting | F61–F64 | §6.4, A2, A3, Part VIII |
| **G — Terms as data** | **F65–F68** | **§5.3.1** — the term register (F65), the evaluation service the chain calls at steps 4 and 7 (F66), the checkpoint queue (F67) and the test mode (F68). **New at v0.4** |

> **Group B is gone, and the codes are burned.** F11's benefit-schedule store and F12's policy and member register are InsureMO's — **F05** and **F07**. F12's payout account and payee, and F13's bulk load, are ours — **F58** and **F59**. **F11, F12, F13 and F14 are retired and never reused:** a document that still says "F12" must resolve to *deleted*, not to whatever inherited the number. Four features became three, and one system became none.
>
> **Group counts, unchanged at v0.5: A 7, C 10, D 8, E 20, F 4, G 4 — 53, of which 47 are Priority 1.** **Group G was new at v0.4** (F65–F68, terms as data) and **E gained F60**, policy and cover enquiry from a claim — **which is why the Group E range above reads F41–F60 and not F41–F59**; to v0.4 the range and this count disagreed, and F60 appeared in a total without appearing in a range. The six that are not Priority 1: F06, F43, F54, F55 and **F68** at 2, F36 at 3. **v0.5 adds no feature**: every fix in it is a rule, and the four Group G features are the mechanism they run on.
>
> **F65–F68 are grouped rather than appended to Group E, and the grouping is the point.** They are **one capability — terms as data** — with an authoring surface, an engine, a queue and a test harness. Read as four unrelated console screens they are four candidates for a cut list; read as one capability, cutting any of the first three leaves step 4 of the chain exactly where §0.2b found it — a rule with no data behind it.

### 6.2 The assessor workbench (F44–F49)

Reference System A's assessor screen is a **five-step wizard**: *Thông tin chung → Danh mục HS → OCR & Tổng hợp chi phí → Sự kiện bảo hiểm → Trình duyệt*. Its prototype confirms the arrangement is workable in practice, and our F44–F49 describe the same five things. **We adopt the sequence**, with one change: coverage assessment precedes cost review, because deciding whether an episode is covered before arguing about individual line items saves the argument entirely when it is not.

| Step | Feature | Business rules |
|---|---|---|
| 1. Claim overview | F45 | Pinned policy and benefit snapshot, always visible. The assessor must never have to leave the claim to see what the member is covered for |
| 2. Documents | F44 | Checklist with three requirement levels, copy-vs-original, completeness state, information requests — §6.3 |
| 3. Coverage assessment | F47 | Per event: ICD-10, condition class, in or out of cover, decline reason, and a **mandatory liability rationale** in the assessor's own words |
| 4. Cost review and calculation | F46, F48 | Original image beside extracted data, warnings anchored at the field, correction, per-line pay/decline, and the computed settlement |
| 5. Submit | F49 | Assemble, validate, submit. **The claim locks** (§3.2.5) |

**Two presentation requirements carried from the reference, because both change assessor behaviour:**

- **A remaining-limit lookup, available from the claim.** Reference System A's `Tra cứu Hạn mức NĐBH theo đơn` lists every benefit with sum insured, maximum days, total paid, total days paid, remaining amount and remaining days. Ours is the ledger read; the requirement is that it is one click from the claim and not a separate screen. **From v0.4 it carries the period dimension** — where a claim spans a policy-year boundary the remaining figure is **per period, not a single number** (L-3) — and the day and occurrence figures are read from the ledger rather than displayed from the schedule, which is what makes them true (§5.9.2). **The figure is also labelled as final consumption only** (BR-LEDGER-2, §5.9.4).
- **Confidence shown per line, not per claim.** Green / amber / red per settlement line with the reason, so the assessor's attention goes to the two lines that need it rather than to the whole claim.

**A third is ours, and it follows from §5.0:** the extracted value and the assessor's corrected value are **both** visible, side by side, at all times. IDP proposes and the Claim Platform decides; the interface must make that visible or assessors will treat the proposal as the answer.

### 6.3 Document checklist and information requests (F34, F44)

| Rule | Statement |
|---|---|
| **BR-DOC-1** | Three requirement levels: `REQUIRED` blocks settlement · `EXPECTED` triggers an information request the assessor may waive · `OPTIONAL` never blocks and is never requested |
| **BR-DOC-2** | A requested document ends in one of **two terminal states**: *supplied* or *requested-and-not-supplied* — see below |
| **BR-DOC-3** | A checklist tick is **one-way**: it can be set but not silently unset, and moving between states is audited old-value → new-value |
| **BR-DOC-4** | The package completeness state is produced by IDP and **consumed** by the claim engine, never re-derived |
| **BR-DOC-5** | Copy versus original is tracked per document, with an `AWAITING_ORIGINAL` state |

**BR-DOC-2 is the one a naive design gets wrong, and the reference is emphatic about it.** Reference System A tracks three flags per document type: `Cần bổ sung` (we asked), `Đã bổ sung` (they sent it), `Không bổ sung` (we asked, a batch came back, it still is not there). When a supplementary batch arrives, **every requested item leaves the "we asked" state**, whichever of the two outcomes it lands in, and the claim moves on.

Without the second terminal state, a claim waits on a document that will never arrive, the SLA clock never restarts, and no one is prompted to decide on the evidence available. `Không bổ sung` is also the audit trail that justifies settling partially or declining.

**On physical originals.** Reference System A has **no** handling of them at all — no original-versus-copy attribute, no received flag, no location tracking. Paper is scanned outside the claim system and uploaded. Given that Vietnamese practice generally requires original invoices and receipts, with photocopies accepted only under insurer certification, that is a gap rather than a simplification, and BR-DOC-5 closes it.

**On the document-type crosswalk.** Reference System A maintains an explicit mapping table between its own 27-type business taxonomy and the OCR partner's document classes — one business type may map to three OCR classes, and only 14 of its 27 types map at all. **The crosswalk is a maintained artefact, not an assumption of sameness.** In MVP 1 the crosswalk collapses to identity — A4's seven types *are* the classes IDP returns — so no table is needed yet. **The requirement is that the moment the two taxonomies diverge, a crosswalk becomes a maintained artefact with an owner**, rather than a mapping buried in adapter code.

### 6.4 Notifications and correspondence (F61)

A2 is authoritative. Three business rules belong here because they are decisions, not wording:

| Rule | Statement |
|---|---|
| **BR-MSG-1** | A push notification never carries a settlement amount, a diagnosis, a benefit name or a decline reason. It says a result exists |
| **BR-MSG-2** | The settlement notice itemises **benefit by benefit**, with declined lines in the same table |
| **BR-MSG-3** | The decline notice states the reason, the clause relied on, that cover continues, and **the route to challenge the decision**. In MVP 1 that route is **out of band** — a named contact channel — because a `DECLINED` claim has no inbound feature and re-opening is out of scope (S-13). Defensible under BC-2; it becomes a real feature when real customers arrive |

**BR-MSG-1** exists because a lock-screen preview is readable by anyone holding the phone. **BR-MSG-3**'s last element is the one most often omitted: a decline without a stated route to challenge it is the most reliable way to turn a decision into a complaint.

---

## Part VII — Data requirements

### 7.1 What each system masters

Framework §6.1 is authoritative. **Three systems master data in this solution, and from v0.3 the split is worth stating whole rather than by exception**, because the same reissue that moved the register into a vendor's platform also brought two masters the other way.

| Data | Master | Business consequence |
|---|---|---|
| Product definition, premium table, **benefit schedule as sold** | **InsureMO** (F01, F07) | Confirmed 21/08/2026 and demonstrated against A5. It is a term of the contract, and it lives with the contract |
| Policy, certificate, member, coverage dates, card number, premium-paid flag | **InsureMO** (F05) | The register is the vendor's. **We hold no policy table**, which removes a whole class of drift — and removes our ability to correct a wrong coverage date (A3 §3.2.2, A3-06) |
| **Adjudicable benefit configuration** | Claim Platform | The insurer can change how a claim is paid without a vendor release. This is BO-3, and it is the seam at which a vendor's product model stops and ours starts |
| **Policy terms as evaluable objects — the term register** | **Claim Platform** (F65), **keyed on the `ruleSetRef` I-05 returns** | **New at v0.4.** InsureMO masters the **identity** of the rule set; we master its **operational meaning** — the class, the scope, the effect, the predicate and the clause citation. It is master data of the same kind as the row above: it exists to decide a claim. §5.3.1 |
| **The term register *version* a claim was evaluated against** | **Claim Platform** (F65), **on the claim's snapshot** — bound at **registration** | **New at v0.5, and it is the row whose absence made two rules unimplementable.** `ruleSetRef` is InsureMO's identifier for the **wording**; the register's version identifier is ours and identifies **our expression of it**, which `BR-TERM-5` expressly allows to change under an unchanged rule set. **A claim binds both** — `BR-TERM-4` — so `BR-TERM-3`'s determinism is a claim that can be tested and `BR-TERM-5`'s correction can name what it is correcting. **Same construction as `BR-KB-4`**, which has versioned the knowledge base onto the claim since v0.1. §5.3.1, §7.2 |
| **Extraction result** | IDP, **immutable** | Raw and corrected stay comparable forever, which is what makes extraction accuracy measurable and the auto-approval threshold tunable |
| **Limit ledger** | Claim Platform | The remaining limit a customer sees and the one the engine enforces are the same number. **Exclusively ours by decision** (B-17), not by discovery |
| **Payout account and beneficiary/payee** | **Claim Platform** (F58) | Claim business, not policy business. It is created and verified at claim time and it is not a term of the contract |
| **Group member list as loaded** | **Claim Platform** (F59) — **provisional on C-IMO-10** | If InsureMO exposes a bulk load, InsureMO masters it and F59 is a loader only. **The only row in this table whose master is undetermined**, and it is undetermined for exactly one reason |
| Clinical recommendations | Claim Platform (IDP produces them) | The assessor's decision on each is recorded against the recommendation — §5.10 |

> **One master per row, and the row that has two is flagged as such.** Everything above has a single owner except the group member list, which is why a snapshot (§7.2) means anything at all.

#### 7.1.1 Every rule that reads policy or member data, and where it now reads from

Moving the register into a vendor's platform is the kind of change that leaves a rule pointing at nothing. Each rule below was re-checked at v0.3, and the one that had lost its source is named.

| Rule | What it reads | Source at v0.3 | Status |
|---|---|---|---|
| §3.2.1 date chain | `coverageFrom`, `coverageTo` per **member** | InsureMO, I-05 (F05) | Sound. Confirmed by C-IMO-01c |
| §3.2.3 coverage validation | Member match, policy status, cover on the service date | InsureMO, I-05 | Sound. **We derive the verdict; InsureMO supplies the facts** |
| A5 §6.1 step 1 | `premiumPaid` | InsureMO, I-05 | Sound. Reported by the vendor, **tested by us** |
| §5.4 waiting periods | Member coverage start date; days by condition class | InsureMO — the date from the register, the matrix from the schedule (F07) | Sound. The six classes were part of what C-IMO-01 demonstrated |
| Part V rule chain | The benefit tree, its limits, inherited co-payment, the deduction rate and its condition | InsureMO, I-05 as at the service date, **then the claim's snapshot** | Sound, and **read once**. After registration the chain reads the snapshot, never the vendor — which is what makes a re-run reproducible |
| §5.6 deduction condition | `in_network` on the provider | **Claim Platform**, F57 | Sound, and unchanged. The rate is a contract term; the fact that triggers it is ours to observe |
| **§5.3.1 term evaluation** | `ruleSetRef`, **the term register version**, and the claim's own facts | **InsureMO for `ruleSetRef` (I-05); the term register and its version are ours (F65)**, both **bound at registration onto the snapshot** | **New at v0.4, and it is the row that did not exist.** To v0.3 step 4 read *"per the policy wording"* and had **no source at all** — the defect §0.2b describes. **Corrected at v0.5**: `ruleSetRef` alone does not identify what the claim was evaluated against (BR-TERM-4). Two facts in the vocabulary are still missing: `facilityCountry` (**Q-IDP-05**) and a treatment-day count (**Q-IDP-06**), and from v0.5 a step that needs either and cannot get it **holds the line under `BR-CHAIN-1`** rather than passing it |
| §5.9 limit ledger key | `memberId` | InsureMO issues it; the ledger is ours | Sound |
| **§5.9.1 counter period** | Policy **inception date** | **InsureMO, I-05 — on the claim's snapshot** (L-1) | Sound, and it is the snapshot's copy that is read, never the vendor. **The policy year is derived, not stored per posting** — what is stored is the resulting `periodKey` (L-4) |
| **§5.9.5 cap as at the posting** | The benefit's cap when the posting was made | **Claim Platform ledger**, written from the snapshot | **Conditional.** Sound only if a mid-term endorsement is visible to the snapshot — **C-IMO-14, DE-13.** If InsureMO changes schedule fields silently, this rule cannot be implemented as written |
| §7.4 identity matching | `memberId`, plus name + date of birth + ID number | InsureMO for the id; the composite from the claim | Sound |
| **BR-PAY-1** | Account holder's name, and `accountNameVerified` | **Claim Platform, F58** — *not* a field on a policy read | **The one that broke.** The record has a home; **the verification call has no contract** (D2-106, DE-12). See §3.2.2 |
| BR-PAY-3, BR-PAY-4 | `accountNameVerified`; a previously paid payee | **Claim Platform, F58** | Sound. BR-PAY-4's composite match is unaffected — it was always a match across records that may not share a member id |
| BR-IMP-1…5 | The group member list being replaced | **Claim Platform, F59** — destination open, C-IMO-10 | Sound in both branches; see §3.3 |

### 7.2 The policy snapshot

**What is snapshotted onto a claim, precisely:** the *benefit schedule as at the service date*, plus *policy and member data as they stood when the claim was registered*, plus — **from v0.5** — the *`ruleSetRef` and the term register version in force at registration* (**BR-TERM-4**).

That is narrower than "the policy as at the service date", and the difference is honest rather than convenient. The benefit schedule carries effective dates and can genuinely be reproduced as at a past date; policy and member records do not, because MVP 1 has no endorsement model (S-05). Reference System B keys claim entitlements to the policy-and-endorsement version precisely to resolve this, and a real core does the same in production.

**Requirement:** the snapshot is taken **once, from one consistent read**, at registration. A snapshot assembled from two reads minutes apart can straddle an administrative edit and produce a schedule that never existed.

#### 7.2.1 Why the snapshot matters more at v0.3 than it did at v0.2

This is the paragraph that changed most in this reissue, and it changed in the direction a reader is least likely to expect.

Until v0.2 the snapshot was a **convenience with a good rationale**: a policy change tomorrow should not move the basis of a claim assessed today, reprocessing should be deterministic, and a settled claim should be explainable. All of that remains true. But the benefit schedule sat in a component we built and controlled, so if the snapshot were ever wrong or missing, the original was on the other side of a database connection we owned. The snapshot was a *copy* of something we still had.

**It is not a copy any more. It is the record.**

The adjudicable benefit tree now lives in **InsureMO**. Four things follow, and each is a reason the snapshot is load-bearing rather than prudent:

| # | What could happen | What the snapshot does about it |
|---|---|---|
| 1 | InsureMO publishes a **new product version**, or corrects an effective date on an old one | The claim still shows the schedule it was assessed against. Without the snapshot, re-opening a settled claim would silently re-price it against terms that did not exist on the treatment date |
| 2 | InsureMO is **unavailable** when someone asks why a claim paid what it paid — an outage, an expired credential, a contract that has ended | The answer is on the claim. Assessment, re-runs and dispute handling survive the vendor being down; **only new registrations stop** |
| 3 | The **as-at-a-date query behaves differently from the demonstration** — effective dating is still unverified in its detail (**C-IMO-05**) | Claims already registered are unaffected, because their basis was captured at registration rather than re-derived on demand |
| 4 | **BR-TRACK-4 re-runs the whole rule chain** after a clinical recommendation is confirmed | The re-run reads the snapshot, not the vendor, so the second pass and the first pass see identical inputs. A chain that re-read live could return a different total on the same claim, and nobody would be able to say which was right |

**Requirement, restated with the weight it now carries.** The snapshot is not an optimisation and must never be treated as a cache that can be refreshed. **It is the only local record of what the contract said when the claim was assessed**, and a settled claim that cannot reproduce its own basis is not auditable, not defensible in a dispute, and not correctable. Framework **P-02** says the same thing in one line; this section is where the BRD argues it.

> **What the snapshot does *not* fix, stated so it is not assumed away.** It preserves the *terms*; it does not preserve the *ability to write a new claim*. If InsureMO cannot serve the benefit-schedule contract at all, no new claim can be registered, because registration is where the read happens — and there is no second place to read from (**RK-14**). The snapshot protects every claim already in the system. It protects none of the claims that arrive tomorrow.

### 7.3 Raw versus corrected

| Rule | Statement |
|---|---|
| **BR-DATA-1** | The extraction result is **never overwritten** by a correction |
| **BR-DATA-2** | Corrections are written to the claim's own copy; the pair stays comparable |
| **BR-DATA-3** | Re-running extraction while a claim is pending approval is **prohibited** |

**BR-DATA-3 is the operational half of AR-3 (§1.4).** Because this system has no synchronisation, a re-read cannot mutate a claim — but it can still produce a *newer* proposal while an approver is looking at an older settlement. BR-DATA-3 closes that by forbidding the request, not by reconciling the result.

### 7.4 Identity matching

Reference System A matches an insured across claims on the composite `name + date of birth + ID number`, and matches an insured event on `provider + treatment date from + treatment date to`. Both are used for limit accumulation and for the payee-reuse shortcut.

**We use the member identity issued by InsureMO (`memberId`, F05) as the primary key**, because we have one and the reference does not. **From v0.3 that identity is a vendor's**, which changes nothing about the matching rule and one thing about its durability: an identifier we do not mint is one we cannot re-issue, so a member record replaced on InsureMO's side rather than updated would break the link to that member's claim history. Nothing in the 21/08 demonstration says whether that can happen. It is not a defect today — it is a question nobody has asked, and it is recorded as such rather than assumed away. The composite is retained as a **secondary match for duplicate detection** (§3.2.4) and for the payee shortcut (BR-PAY-4), where the point is precisely to match a person across records that may not share a member id.

### 7.5 Retention and the audit trail

| Rule | Statement |
|---|---|
| **BR-AUDIT-1** | The audit log is **append-only**. No role, including administrator, may edit or delete an entry |
| **BR-AUDIT-2** | Every decision and every data edit is logged with actor, timestamp, old value and new value |
| **BR-AUDIT-3** | A machine-performed step is logged with the **system as the actor**, not the last human |
| **BR-AUDIT-4** | **Reading a claim's clinical detail is itself logged** |
| **BR-AUDIT-5** | Every denied permission attempt is logged, not merely refused |

**BR-AUDIT-4 is cheap now and effectively impossible to reconstruct later.** Who looked at whose medical documents is a question that will eventually be asked.

---

## Part VIII — Non-functional requirements

Framework F64 is the feature; these are the business-level expectations behind it.

| # | Requirement | Target |
|---|---|---|
| NFR-1 | Claim submission responds within | 3 s to the customer, excluding upload time |
| NFR-2 | Extraction turnaround, complete package | Median ≤ 5 minutes, 95th percentile ≤ 30 minutes |
| **NFR-2a** | **Clinical appropriateness assessment turnaround** | Median ≤ 2 minutes, 95th percentile ≤ 10 minutes. **A claim waits at most 30 minutes for it**, then falls through to BR-TRACK-5 — Track 1 alone, with `CLM-CONSOLE-015` and no auto-approval. This matters because auto-approval condition 2 waits on this interface, and SC-04 waits on auto-approval |
| NFR-3 | Adjudication computation | ≤ 2 s per claim |
| NFR-4 | Availability, business hours | 99.5% |
| NFR-5 | Concurrent assessors supported | 20 |
| NFR-6 | Claims per day supported | 500 |
| NFR-7 | Document processing remains **inside the deployment boundary** | No document or extracted field leaves it (P-07) |
| NFR-8 | Retention of claim data and audit | **Unset, and now owned — `B-18`, opened at v0.5.** No retention period for claim data exists anywhere in this set: A3-05 covers audit entries and access logs only. It is a business and legal decision, and to v0.4 this row asked for a B-item that was never opened — **an NFR pointing at a decision nobody had been asked to take**. It is opened as **B-18**, owned by Business, and must be answered before release |
| NFR-9 | Mobile app supports | Android and iOS, current and one prior major version |
| NFR-10 | Language | Customer-facing Vietnamese only; console bilingual |

### 8.1 The settlement clock — one clock in MVP 1, and what that defers

**Decided 17/08/2026 (B-04): MVP 1 tracks an internal SLA only.** Ageing on the work list and the dashboard (F54) measure from **submission**, which is what the customer experiences and what SC-04 targets.

**What is not tracked.** The Law on Insurance Business sets **15 days from receipt of a complete and valid claim file** where the contract does not agree otherwise. MVP 1 does not run that clock and does not flag a claim approaching it.

**Why that is acceptable now, and precisely when it stops being so.** Boundary condition **BC-2** — no real customers — means no claim in MVP 1 carries a statutory obligation. The moment real claims are accepted, an untracked statutory clock is a compliance gap, not a simplification.

**What we keep anyway, because retrofitting it is expensive.** The **"complete and valid file" timestamp stays in the data model** and is recorded on every claim (Framework §9.4). It is one field and one event; it is the anchor the statutory clock will need; and reconstructing it after the fact from other timestamps is exactly the kind of derivation that a regulator will not accept. **Capture it, do not yet measure against it.**

Declared simplification **S-19**.

**NFR-2 is the one the customer experiences as the product.** Everything else can degrade gracefully; a claim that sits in `DOC_PROCESSING` for an hour makes the proposition look false.

---

## Part IX — Assumptions and dependencies

### 9.1 Assumptions

| # | Assumption | If wrong |
|---|---|---|
| AS-1 | The pilot product is the one in A5, or close to it | A5 is a representative construction; a materially different product changes the benefit tree, not the engine |
| AS-2 | Pilot providers issue itemised cost statements in a readable form | SC-01 and SC-02 become unachievable; the ground-truth benchmark is where this is discovered |
| AS-3 | Customers photograph documents adequately with guidance | Capture guidance (A2 §3.2) is the mitigation; the fallback is assessor keying |
| AS-4 | A ground-truth set of 25–30 anonymised packages across ≥4 hospitals can be assembled | **RK-02 — High/High, and the longest-lead item in the project.** Nothing in extraction can be built or measured without it. **v0.3: RK-02 is reclassified outside this engagement** (Framework §11.1) — sourcing is delivery. **The assumption is unchanged and so is the exposure**; only the register it is carried in moved |
| AS-5 | The four boundary conditions in Framework §9.4 hold throughout MVP 1 | The legal and data-protection deferral is void and review becomes required before release |
| AS-6 | Clinical appropriateness can be assessed usefully from the diagnosis, prescription and test lines alone — without a full medical record | Track 2's recommendations are too weak to act on, and advisory mode (D1 §5.11) becomes the only safe posture |

### 9.2 Dependencies

| # | Dependency | Owner | Status |
|---|---|---|---|
| ~~DE-1~~ | ~~IDP capability set~~ | IDP team | **Closed 21/08/2026.** The IDP team confirmed **every** capability in writing, including per-field confidence, the submit→webhook→pull contract, in-boundary inference, e-invoice XML and the review deep link. The preliminary status is discharged |
| **DE-2** | IDP effort and timeline for the seven document types **and for the clinical service and its knowledge base** | Delivery | **Reclassified 21/08/2026 — outside this engagement's scope** (Framework §9.2.1, C-IDP-01). It asks *how long*, not *whether*, and *whether* is answered. **Not closed and not improved** — it becomes the first item on the delivery plan's critical path, re-scoped to cover F37 and F38 |
| **DE-3** | Ground-truth set sourcing and anonymisation | Delivery | **Reclassified 21/08/2026 — outside this engagement's scope** (C-IDP-02, RK-02). **The requirement is unchanged and unrelieved**: SC-01 and SC-02 cannot be met or measured without it, and it is still the longest-lead item in the project and the one that cannot be compressed by adding people. What moved is who answers for *by when and from where*, not whether it is needed |
| ~~DE-4~~ | ~~InsureMO benefit-tree, count-limit and condition-class-waiting capability~~ | InsureMO | **Closed 21/08/2026 — confirmed by live demonstration against A5 v1.2** (C-IMO-01, C-IMO-01b, C-IMO-01c). **The fallback recorded here is withdrawn with it.** There is no second implementation; see DE-10 |
| **DE-5** | InsureMO **contract detail** — quotation and issuance schemas, the policy-issued integration style, effective-dating behaviour | InsureMO | Open — C-IMO-03, C-IMO-04, C-IMO-05. **Capability is confirmed; the request and the response are not.** Contract-first mocks absorb the rework (D3 §5); of the three, C-IMO-05 is the one where a surprise costs design rather than a mock, because **F07's as-at-a-date semantics rest on it** |
| DE-6 | Pilot product design confirmation | Business | Open — C-BUS-01. Structure confirmed (B-01) **and now demonstrated**; figures still to be replaced from a real product, against the checklist in A5 §0.1 |
| DE-7 | ~~Auto-approval threshold~~ | Business | **Closed** — B-03 and B-12. The value can be refined from the benchmark (DE-3) but the gate is live |
| DE-8 | **Clinical knowledge-base content and medical curation** | Business + IDP | **Open — RK-08, and it survives its own confirmation.** C-IDP-10 settled *provenance and accountability*: IDP builds it from named public sources with a named owner (B-13). **It is not a clinical sign-off and none was offered**, so the exposure of relying on it to decline a prescribed medicine is unchanged. The *scheduling* half moves to delivery with DE-2; the product-design half stays here |
| ~~DE-9~~ | ~~IDP's clinical assessment capability, distinct from extraction~~ | IDP team | **Closed 21/08/2026** — C-IDP-09 confirmed, expressly as a capability distinct from extraction. **RK-13 closes with it**: auto-approval's condition 2 has a mechanism behind it and the gate can fire |
| **DE-10** | **An InsureMO sandbox with credentials, evaluated hands-on against A5 v1.5 benefit by benefit** | InsureMO + BnK | **Open, and on the critical path — new at v0.3, and the most consequential line in this table.** C-IMO-06. With no Policy Data Store there is no second implementation, so the sandbox is the only place a **confirmed** capability becomes a **proven** one. It is the sole mitigation for **RK-14**, and RK-14 is mitigated by evaluation rather than by design |
| **DE-11** | **Does InsureMO expose a bulk member load?** | InsureMO | **Open — C-IMO-10, new at v0.3.** It decides where F59 writes, and therefore whether group business has one member list or two (§3.3). A reconciliation obligation is far cheaper taken as a decision than discovered during build |
| **DE-12** | **A counterparty and a contract for account-name verification** | BnK + Business | **Open — new at v0.3, and it is a gap this reissue created.** With the payout account moved to F58, `accountNameVerified` is ours to set and **no interface defines how** (D2-106). BR-PAY-1 currently names a call that does not exist. Every settlement passes through it |
| **DE-13** | **Does a mid-term endorsement produce a new `ruleSetRef` or benefit-schedule version, or change fields silently?** | InsureMO | **Open — C-IMO-14, new at v0.4.** It decides whether §5.9.5 is implementable at all. **In the silent case the snapshot cannot detect the change**, so consumption cannot be carried against a cap the system does not know moved. Endorsements are out of scope for MVP 1, so nothing is blocked today — **but the ledger's storage shape depends on the answer**, and retrofitting a cap onto historical postings is not possible |
| **DE-14** | **Two facts the term register needs and does not have** — the **country of the treating facility** and a **treatment-day count per line** | IDP | **Open — Q-IDP-05 and Q-IDP-06, new at v0.4.** Without the first, the *"treatment outside Vietnam"* exclusion cannot be class D and falls to a checkpoint; without the second, `DAY` counters (§5.9.2) cannot be maintained from extraction. **Neither blocks build** — both degrade a term or a counter to a human — and both should join the IDP round already open, because asking now costs nothing |

---

## Part X — Fit-gap against the reference systems

This part exists because a reviewer familiar with a production Vietnamese claim system will hold this design up against it, and every difference should already have an answer.

### 10.1 Declared simplifications

Framework §7.4 is the single register of declared simplifications and now holds **forty-one, S-01…S-41**: the original **S-01…S-08**; the eight below — **S-09…S-16** — which this document introduces and which are registered in the Framework alongside the others; four from the decisions of 17 August — **S-17** BHYT not deducted (§5.6a), **S-18** no claim-notification time limit (Part XI B-10), **S-19** internal SLA clock only (§8.1) and **S-20** no contribution rule where a member holds a second private policy (§10.2 GAP-7); **S-21…S-38** from the gap analysis against Reference System B and the difference register against Reference System A; and **three added at v0.4** by the decisions of 24 August.

**The three new ones, and each is argued in Part V rather than only listed here:**

| # | Simplification | Where it is argued |
|---|---|---|
| **S-39** | **The adjudication arithmetic and its order are code, not configuration.** A new calculation rule costs one release cycle, against same day for a change to a term. Reference System A can draft, approve and activate a new formula in a day; we cannot, deliberately, for MVP 1 | §5.3 — the withdrawal of the "order is configuration" claim, and the revised **P-05** |
| **S-40** | **Neither a new term version nor a term correction may touch a claim in `PAID` or `CLOSED`.** Correcting a settled claim means recovering money from a customer or paying them more, and MVP 1 has neither | §5.3.1 — **BR-TERM-6**; and §5.9.3 — the `PAID` row of **BR-LEDGER-7** |
| **S-41** | **No coordination of benefits across policies, and no cross-policy duplicate detection behind it.** A member on two policies may present the same cost to both and the platform will not detect it | §10.2 **GAP-7**, and Part XI. It carries **RK-15** rather than resting on the boundary conditions alone — the only entry in the register that does |

**S-41 is the sibling of S-20 and the two must be read together.** S-20 declares the **contribution** rule missing — who pays what share. S-41 declares the **detection** missing, and detection is the half that costs money.

| # | The reference does | MVP 1 does | Why |
|---|---|---|---|
| **S-09** | A separate **intake record** (`TNTT`) with its own number, status and audit trail, converted into a **claim file** (`HSBT`) with a different number only on acceptance | One claim record. `DRAFT` and `SUBMITTED` serve the intake period; **the claim number is allocated at registration**, after coverage validation | The two-object split exists to keep junk submissions, duplicate scans and wrong-insurer post out of the claim register and out of reserving. Our claims arrive from an authenticated customer against a known policy, so the population is far cleaner. **The property we must keep is that a claim number is never burned on something that was never a claim** — hence allocation at registration, not at first tap |
| **S-10** | Four independent approval rounds (reserve, guarantee, settlement plan, payment), each with its own submission counter, each approving **lines in waves** so approved lines never re-enter a later round | One approval, on the whole claim | Interim payments and per-line approval waves are what make the reference's model necessary. MVP 1 settles a claim once. **The cost is that a supplementary invoice on a settled claim cannot be paid without re-opening**, and MVP 1 has no re-open (S-13) |
| **S-11** | Unit hierarchy, authority-derived routing, weighted auto-assignment excluding director grades | One queue | §2.2. First thing to reverse when a second branch appears |
| **S-12** | Approval authority computed on the **cumulative amount across all claim files for one incident**, re-verified at the instant of the click, with retroactive warnings when an authority document is backdated | One threshold, one level, checked at submission and at approval | Per-claim thresholds are defeated by splitting an incident across claims. MVP 1 accepts that exposure because the threshold is small and the volume is a pilot. **State it in the pilot's operating procedure rather than pretending the control is complete** |
| **S-13** | A **re-open** workflow requiring two distinct permissions, which unlocks everything except already-approved lines | None. A closed claim is closed | Re-open needs the wave-approval model (S-10) to be safe. Without it, re-opening would let an approved settlement be silently altered |
| **S-14** | Post-approval unwind paths — coverage values meaning *"wrong claim opened"* and *"customer withdrew"*, usable only after something was approved | None | §4.6. A claim approved in error must be corrected before payment, or handled outside the system |
| **S-15** | A **blacklist screen** at intake, surfacing open fraud alerts with the prior claim numbers attached, before an assessor opens the file | None | Fraud and blacklist screening are out of scope (Framework §7.2). Worth noting that the reference does it at *intake*, which is the cheap moment |
| **S-16** | Automatic **reserve creation at intake**, with net retention computed as `amount × co-insurance share × (1 − reinsurance) × (1 − third-party recovery)` | None | Reserving is out of scope. Recorded because it explains why the reference creates so much on acceptance |

### 10.2 Gaps in the reference that this system closes

> Identified **GAP-1 … GAP-7** to avoid collision with D3 §4's **G-1 … G-7** cross-interface guarantees, which are cited by document across the set.

Not everything the reference does is worth copying, and not everything it omits is safe to omit.

| # | Gap in the reference | What we require |
|---|---|---|
| **GAP-1** | **No duplicate-claim detection of any kind.** Its bulk intake creates one claim per PDF and will happily create two from the same folder scanned twice | §3.2.4 — BR-DUP-1 to BR-DUP-4 |
| **GAP-2** | **No defined path when no policy matches.** The policy number is mandatory, so the claim simply cannot be saved; the de facto handling is to reject or cancel | §3.2.3 — a `PENDING_INFO` path that asks the customer, and never auto-declines |
| **GAP-3** | **No handling of physical originals** — no attribute, no received flag, no tracking | BR-DOC-5 and the `AWAITING_ORIGINAL` state |
| **GAP-4** | **The payable arithmetic is unspecified**, because it arrives from an external engine. Co-payment and deduction ratio are displayed but no base or order is stated | Part V §5.3 — fourteen steps with the base amount for every percentage **and, from v0.5, the granularity of every step** |
| **GAP-5** | **No claim-notification time limit check** (*thời hiệu*) | Closed as a decision, not inherited as a gap: **declared simplification S-18** (Part XI B-10). The reference omits it too, so there is no proven pattern to copy when it arrives |
| **GAP-6** | An internal contradiction: a returned intake record is described as read-only, while the permission table allows the officer to edit it | Our returned claim is explicitly editable by the assessor and explicitly not by the customer |
| **GAP-7** | **No contribution rule for a second private policy.** Neither reference system asks whether the member holds other cover, and neither apportions between insurers | **Out of scope in MVP 1 and declared as such** (S-20). Dual cover is common in Vietnam — an employer group scheme alongside a personal policy — so this is a real exposure rather than a theoretical one. BR-DUP-1 detects duplicates only *within this system*. A declaration question at submission is the cheap first step when it arrives. **At v0.4 the other half is declared too — S-41, and it carries RK-15**: the ledger's counter key includes `policyId`, so a member's two policies carry two independent sets of counters and **neither knows the other exists**. That independence is correct, and it is exactly what makes a duplicate invisible. **A member holding two policies may submit the same cost to both and the platform will not detect it.** Detection needs a **cross-policy duplicate check keyed on member, facility, treatment date and amount**, and it is not built. **This is a fraud exposure, not a cosmetic gap** — acceptable for a controlled pilot with one product and no real money (BC-2, BC-3), and not one of those conditions is a control. **Build the detection before the coordination rules, and build it as a flag to an assessor rather than a block** — a member legitimately holding two policies is not a fraudster, and a block would tell them they are |

### 10.3 What the reference confirms

Recorded because most of this design survived contact with two production systems:

- **Four-system split, and building claims ourselves.** Neither reference has a product platform doing adjudication. Reference System A's claim module sits beside an external OCR partner; Reference System B's sits inside a policy and accounting ERP.
- **IDP proposes, the Claim Platform decides** — confirmed by counter-example, and it is the difference §1.4 is built on. Reference System A makes every settlement amount read-only from its OCR partner, which is precisely the coupling P-01 forbids.
- **The limit ledger's shape** — sum insured, maximum days, total paid, total days paid, remaining amount, remaining days — is exactly the remaining-limit view both references expose to an assessor. (Neither *stores* it as a ledger; see §5.9 for why we do.)
- **The document checklist's tri-state.** Confirmed, and improved on: the reference's third state is what stops claims stalling forever.
- **The five event-level decline reasons** — limit exhausted, outside the cover period, benefit not held, waiting period not served, policy exclusion. The same five, as a closed list, in Reference System A.
- **Snapshot at service date.** Supported by Reference System B keying entitlements to the policy and endorsement version.
- **Excluding reserving, co-insurance, reinsurance, recovery and the authority matrix.** All five exist in the reference as substantial, separately-approved workflows. Excluding them is what makes a two-journey MVP possible in the time available.
- **Auto-approval is genuinely ours to invent.** Neither system has it.

---

## Part XI — Open items

| # | Item | Owner | Blocks |
|---|---|---|---|
| **B-01** | **Pilot product design — RE-OPENED at v0.5.** Closed 17/08/2026 on the reasoning *"keep A5's structure, replace the figures from a real product; replacing figures changes nothing downstream."* **That reasoning was true when it was given and is not true now.** It was given when A5 §7 was **ten prose sentences of policy wording**, which nothing evaluated and which substitution therefore could not disturb. **A5 §7 is now a term register** — ten evaluable objects with a class, a scope, a selector, a predicate, an effect and a clause citation — and **substituting a real product's wording replaces the terms wholesale**, not the figures under them. **The question to re-decide:** does the substitution keep A5's structure *and its term register*, replacing only figures and clause citations; or does it take the real product's exclusions, in which case the class mix, the checkpoint volume, the auto-approval rate and every worked example in A5 §6.2 move with them? **What has not changed** is the reason B-01 closed the way it did — A5's structure is the engine's test harness and every mechanism has a benefit that exercises it. **See the checklist in A5 §0.1 before any substitution**, and note that the checklist itself predates the term register | Business | **Open** — and it is the open item with the widest blast radius, because it reaches A5, the seed, D2's register screens and §5.8's expected straight-through rate |
| ~~**B-02**~~ | ~~Document checklist per claim type~~ — **Closed 17/08/2026: A4 §6 confirmed, with one change — `T1-02` prescription moves from Optional to Expected for outpatient-illness.** Track 2's drug-versus-diagnosis assessment needs the prescription; Expected requests it without stalling a claim that legitimately has none | Business | Closed |
| ~~**B-03**~~ | ~~Auto-approval threshold~~ — **Closed 17/08/2026: 2,000,000 VND confirmed as the live threshold** for the auto-approval gate (B-12 revised, §5.8 — **two conditions then, three from 24/08/2026, four from v0.5**). It is a real gate, not a marker; the value can be refined once the ground-truth benchmark exists. **The threshold is a per-claim figure and B-22 asks whether a cumulative one is also needed** | Business | Closed |
| ~~**B-04**~~ | ~~Settlement SLAs~~ — **Closed 17/08/2026: an internal SLA clock only.** Ageing and the dashboard (F54) run on an internal target measured from submission. **The statutory 15-day clock is not tracked in MVP 1** — see §8.1 for why that is acceptable now and what it becomes later | Business | Closed |
| ~~**B-05**~~ | ~~BHYT coordination~~ — **Closed 17/08/2026: captured, not deducted (S-17).** See §5.6a for the consequence and the re-entry point | Business | Closed |
| ~~**B-06**~~ | ~~Physical originals~~ — **Closed 17/08/2026: not required in the pilot.** Claims settle entirely on captured images. **BR-DOC-5, the `AWAITING_ORIGINAL` state and the copy/original flag all stay in the model**, unused, so enabling them later is configuration rather than a change to the state machine | Business | Closed |
| **B-07** | **Assessor and approver staffing — RE-OPENED at v0.5.** Closed 17/08/2026 on **a four-role pilot team** — customer, assessor, approver, administrator — with a deliberate two-level model, submit and approve. **The term layer added duties that team does not cover.** §5.3.1's governance requires **`TERM_AUTHOR` and `TERM_APPROVE`**, and it requires them to be **different people**, because authoring a term is making a contractual statement and a person approving their own is no control at all. That is **two further duties**, and neither maps onto assessor or approver without destroying the separation it exists to create — an assessor who authors the exclusions they later apply is the same defect the two-level model was built to avoid. **And the checkpoint queue (F67) has no owner named anywhere.** It is the queue every class-J, class-E and checkpoint-carrying class-D term routes to, it now blocks submission as well as auto-approval (**BR-TERM-13**), and **nobody in the four roles is assigned to work it**. **The question to re-decide:** who holds `TERM_AUTHOR`, who holds `TERM_APPROVE`, and who works the checkpoint queue — **four roles do not cover six duties**, and the answer may be that the pilot team is larger or that two of the duties sit outside it. **What has not changed:** A3 §4.1 segregation still binds per claim rather than per person, and S-12's one threshold and one approval level are unaffected | Business | **Open** — nothing in build waits on it, and **the pilot cannot be operated without it** |
| ~~**B-08**~~ | ~~E-invoice availability~~ — **Closed 17/08/2026: XML-only stands (A4 §1.5).** A claim without invoice XML still settles — the cost statement is the source of cost lines; only reconciliation rule R6 is skipped, with a warning. **The requirement that matters is on the ground-truth set, and A4 §10 already carries it: 10 invoice XMLs across ≥3 providers.** If XML proves scarce in the pilot providers, those 10 must be sourced elsewhere — an untested parser path is the real loss, not the settlements | Business | Closed |
| ~~**B-09**~~ | ~~Deduction-ratio ceiling and grant~~ — **Closed 17/08/2026, and the design changed with it: the deduction ratio is a policy term, not a discretion.** No ceiling, no grant, no override rule. §5.6 and A5 §4.3 | Business | Closed |
| ~~**B-10**~~ | ~~Claim-notification time limit~~ — **Closed 17/08/2026: not in MVP 1.** Recorded as declared simplification **S-18**. Note the reference systems omit it too (§10.2 GAP-5), so there is no proven pattern to inherit and it will need designing when it arrives | Business | Closed |
| **B-11** | Target dates for the demo and for development start | Client | Sequencing. The only genuinely date-shaped open item |
| ~~**B-12**~~ | ~~Auto-approval~~ — **Closed 17/08/2026, revised the same day: auto-approval is IN, on a two-condition gate** — payable at or below the threshold, and the clinical assessment completed with no recommendation. §5.8 states what the gate deliberately does not test. **Extended on 24/08/2026 by decision D-5: a third condition, no open term checkpoint.** The reasoning is the same as condition 2's, applied to a policy term instead of a clinical opinion. **Extended again at v0.5: a fourth condition, no outstanding duplicate signal — and condition 3 corrected to test the checkpoint rather than the class that raised it** (§5.8). **The decision to have auto-approval is not re-opened**; what changed is what the gate must ask before it fires | Business | Closed |
| ~~**B-13**~~ | ~~Clinical knowledge base ownership~~ — **Closed 17/08/2026: the IDP team owns and builds it, from public sources.** See §5.10.5 for what follows, including what this does *not* settle | Business | Closed |
| ~~**B-14**~~ | ~~Disputed clinical decline~~ — **Closed 17/08/2026 on both halves.** *Wording:* a clinical decline is expressed as a **statement about cover**, never as a judgement on a prescribing decision — see §5.10.6. *Escalation:* the existing information-request flow (**F29**) carries it; the customer sends further medical information and the claim returns to an assessor. **No new feature, no new state.** A formal appeals process belongs to MVP 2, when there are real customers (BC-2) | Business | Closed |
| ~~**B-15**~~ | ~~Clinical track scope~~ — **Closed 17/08/2026: full breadth — drugs *and* laboratory/imaging — and the clinical content is IDP's scope.** The Claim Platform consumes results and does not build clinical capability. See §5.10.5 and §5.11 | Business + delivery | Closed |
| **B-16** | **Target agreement rate for the clinical track** — the proportion of IDP recommendations an assessor accepts, tracked as Framework §1.3 **SC-09**. The measure exists and F56 records every accept/override; **the number the pilot is judged against does not.** It cannot be set responsibly before the ground-truth benchmark (C-IDP-02) produces a first reading — set it too high and the track looks like a failure, too low and a useless model passes. **Proposal: leave it unset until the first benchmark run, then agree it from observed performance rather than aspiration** | Business | Nothing in build. Blocks the pilot success assessment only |
| ~~**B-17**~~ | ~~Which platform is used for what~~ — **Closed 21/08/2026: InsureMO is used for the policy side only, and the claim platform is built entirely by us.** InsureMO *offers* claim capability; the client has decided not to use it. Adjudication, the limit ledger, the assessor workbench, approval, payment and the audit trail are ours — which is what makes the benefit engine an asset the insurer owns rather than a capability rented (§1.4, AR-1…AR-4). **Framework v2.0 carries the same decision as C-BUS-09.** Three consequences, and none of them is cosmetic: the ledger is exclusively ours **by decision rather than by discovery**, so the question of whether InsureMO accumulates consumption is **moot rather than confirmed** — marking it confirmed would claim an answer nobody gave; the single residue on the claim side is **F06**, a Priority-2 write-back of a claim *reference* onto the policy record, a notification and not adjudication, on which nothing depends; and, taken together with the deletion of the Policy Data Store, it settles the boundary for the whole solution — **three systems, one of them ours** | Business | Closed |
| **B-18** | **Retention period for claim data — new at v0.5, and it is the item that should have been opened at v0.1.** **How long is a settled claim's data kept, and what happens at the end of that period?** `NFR-8` has asked for this decision since v0.1 and pointed at a B-item **that was never opened**; `A3-05` covers **audit entries and access logs only**, and a claim carries medical documents, diagnoses, cost statements and bank details that the audit-log rule does not reach. **Three sub-questions, and they have different answers:** the retention period for **claim and clinical data**; the retention period for the **extraction result**, which `BR-DATA-1` requires never to be overwritten and which §7.3 keeps precisely so that accuracy stays measurable; and whether the end state is **deletion or anonymisation** — deletion breaks `BR-AUDIT-1`'s append-only log if the log holds the values, anonymisation does not. **It is a legal decision before it is a business one**, and Framework §9.4's data-protection deferral rests on **BC-1 and BC-2** — no real customers, no real personal data — **so it must be answered before either expires**, not before build | Business | **Open.** Nothing in build; **a release gate**, and the model should carry a retention marker on the claim from the start rather than acquire one later |
| **B-19** | **The deduction ratio — per line, or on the aggregate? Three documents currently give three arithmetics, and the two readings differ in money.** The three, quoted: **§5.3 step 11** — *"If the benefit carries one and its condition is met"*, which reads per line, against the line's own benefit; **§5.3 step 10** — co-payment applies *"if the benefit **or an ancestor** has one"*, so inheritance is stated for co-payment and **not** for the deduction ratio; **A5 §6.2's worked example** — applies the rate to **1,920,000, the post-co-payment aggregate**, i.e. at ancestor level, which is a step-**12** quantity being reduced at step **11**. **The data decides nothing and makes the gap bite**: in the seed `deduction_pct` sits on **`200` and `300` only**, both parents, and **every cost line maps to a child**, so **on the pilot product no line's own benefit carries a deduction ratio and step 11 never fires at all** under the per-line reading. **Two candidate rules, both defensible:** **(a)** `deduction_pct` gains an **`"inherit"` sentinel** exactly as `copay_pct` already has, and **step 11 stays per line** — cheapest, symmetrical with co-payment, and the seed already demonstrates the pattern; **(b)** an explicit **aggregation sub-step after step 11**, applying the rate **at the node that carries it, to the sum of its descendants' payables** — which is what A5 §6.2 actually computes, and which is closer to how a wording that says *"a 20% deduction applies to outpatient treatment"* reads. **They give different money** wherever rounding or a per-line cap intervenes, and (b) additionally changes the base of step 12. **This document does not choose**, because the choice is what the contract means, not how the engine is built | Business, with BA | **Open, and it changes money on the pilot product.** Interim: **step 11 does not fire**, which is what the seed produces today and is the safe direction — it over-pays rather than under-pays |
| **B-20** | **In what order do cost lines consume a cap that binds?** §5.7 and §5.3 step 12 say the claim is capped by the tightest binding limit on the ancestor path and never say **which lines get the remaining limit**. **Four lines under one benefit with 1,000,000 of cap left can be paid four different ways, every one of them satisfying every rule in this document, and every one producing a different settlement** — and a different set of lines shown to the customer as reduced. **`BR-TREE-5` makes it visible**, which is what turns an unspecified order into a question a customer asks. **Proposed, and it is a proposal rather than a decision:** **ascending treatment date, then ascending line sequence within a date**, with **the allocation recorded on the claim** so a re-run reproduces it (`BR-TRACK-4`, `BR-TERM-3`). The reasoning is that it is **chronological — the loss the member incurred first is paid first** — it is explicable to a customer in one sentence, and it is stable under a re-run, which a value-ordered or proportional rule is not once a line is declined. **The alternatives are real**: highest-value-first maximises the amount paid and is what a claimant would choose; proportional reduction spreads the cut across every line and is what several markets do by convention. **Whichever is chosen must be written down**, because the one thing that cannot stand is the current position, where the engine picks and nobody decided | Business, with BA | **Open.** Interim: the proposed rule above, implemented and recorded, so that behaviour is defined and reversible rather than accidental |
| **B-21** | **Is a mid-term joiner's annual limit pro-rated, and which period does the counter run on?** **`L-1` keys the counter period on the policy inception date**, and **§3.2.1 argues at length that the member's own period is right and the policy's is wrong** — *"on a group policy, members join and leave mid-term; policy-level dates are wrong for most claims on a group scheme"* — while **`BR-WAIT-1`** measures every waiting period from **the member's coverage start date**. **`L-1` reverts to the policy with no reasoning given, and the contradiction is inside one document.** **What it costs:** a member joining nine months into a policy year gets **a full annual sum insured for nine months and another full one from the anniversary — two annual limits inside twelve months of cover**. The pilot's own seed carries mid-term joiners on both policies, so this is the ordinary case. **Two candidate rules:** **(a)** the counter period follows the **policy year** as `L-1` says, **and a mid-term joiner's annual limit is pro-rated** for the short first period — keeps one boundary date per policy, which group administration wants, and makes the limit proportionate to the cover bought; **(b)** the counter period follows the **member's own coverage year**, consistent with §3.2.1 and `BR-WAIT-1`, with **no pro-ration** — every member gets a full year of limit for a full year of cover, at the cost of a personal boundary date per member and a group whose aggregate exposure no longer aligns with its policy term. **Pro-ration is a term of the product and this document cannot invent one**; the ledger implements whichever is chosen without difficulty | Business | **Open, and it changes money on the pilot product.** Interim: **`L-1` stands as written**, so the exposure above is live and bounded only by BC-2 and BC-3 |
| **B-22** | **May a member be paid more than the auto-approval threshold, unreviewed, across several claims in a period — and if not, what is the cumulative figure?** The gate tests **total payable per claim**, so **three claims of 1,900,000 pass a 2,000,000 threshold and 5,700,000 settles with no human involved**. This is **S-12** — the reference computes approval authority on the cumulative amount across all claim files for one incident; MVP 1 has one threshold, one level, per claim — and §5.12 already records cumulative authority as deliberately not implemented. **What v0.5 adds is that the same hole is in the automatic route**, where there is **no human at all** rather than one human at one level. **The decision needed is not an engineering one:** it is *how much may a member receive unreviewed, over what period* — a cumulative amount per member per counter period, a claim count per period, both, or neither. **A cumulative test is deliberately not added here.** Adding one would set a figure nobody agreed and would change the straight-through rate that SC-04 is measured against. **The exposure is declared instead** — §5.8's *"what this gate deliberately does not test"* table carries it — and §10.1 S-12's own instruction applies: **state it in the pilot's operating procedure rather than pretending the control is complete** | Business | **Open.** Bounded by **BC-3** — no real money moves — and by pilot volume. **It is the first of the gate's omissions to stop being acceptable**, because it needs no knowledge of the system to exploit |
| **B-23** | **At least one real benefit-level proviso, so that chain step 7 has something to evaluate.** All ten terms in the pilot product's register are **`PRODUCT`** scope, so **step 7 runs against an empty set on every claim** (§5.3). The step is right, the scope model is right (**BR-TERM-8**), and **no proviso is invented here** — inventing a contractual term to give a mechanism something to do produces a rule no wording supports, which is the defect this whole layer was built to remove. **What is asked for is one or more real provisos from the product's wording**, each with: the **benefit** it attaches to and whether it reaches that benefit's **descendants** (`BENEFIT` or `BENEFIT_SUBTREE`); its **class**; its **effect**; and its **clause reference**. *"Physiotherapy only on referral"* and *"maximum 30 days per event"* are the shapes this normally takes and are **used as illustrations only** — neither is a term of this product. **A second question travels with it, and it is the more interesting one:** the register's five checkpoint-raising terms include **screening and vaccination** and **dietary supplements**, which are high-volume outpatient content. **Is a human judgement genuinely wanted on every one of those claims?** If yes, §5.8's straight-through rate is lower than SC-04 assumes and that should be planned for; if no, the terms need selectors that reach less | Business | **Open.** Nothing blocks — step 7 evaluating nothing is harmless, and it is **declared** rather than silent. **It blocks the demonstration of step 7**, and therefore of `F66` at benefit scope |
| **B-24** | **One document type the term register needs and A4 does not carry: a police or hospital statement.** Class **E** is defined as *"decidable only if a specific named document says so — a police or hospital statement"* (§5.3.1), and **`BR-TERM-11`** now requires a `REQUIRE_DOC` term to name **an A4 document type**, add it to the checklist at `REQUIRED` level and raise a checkpoint. **A4 carries seven types and none of them is a police or hospital statement**, so the pilot's one class-E term — the accident-circumstances term — names a document type that does not exist. **What is asked:** confirm the document type the business actually requires for an accident claim (a police report, a hospital admission statement, or both as separate types), its **Vietnamese and English names**, whether it is **`REQUIRED` or `EXPECTED`** by default outside a term, and whether an **original** is needed (`BR-DOC-5`). **A4 v1.3 adds it once the answer exists**; until then the class-E term stays `DRAFT` and the engine never sees it, which is correct behaviour and not a workaround | Business | **Open.** Small, cheap, and it is the last thing standing between class E and a working mechanism |

**Thirteen of the twenty-four decisions are closed** — twelve on 17 August 2026 and **B-17 on 21 August 2026** — and **eleven are open**, which is more than at any point since v0.2 and is the honest result of the review this reissue applies.

| Open item | Owner | What it decides |
|---|---|---|
| **B-01** *(re-opened)* | Business | Whether a real product's substitution replaces A5's **terms** as well as its figures |
| **B-07** *(re-opened)* | Business | Who authors terms, who approves them, and who works the checkpoint queue — **six duties, four roles** |
| **B-11** | Client | The demo and development-start dates. Still the only genuinely date-shaped item |
| **B-16** | Business | The clinical agreement-rate target, deliberately deferred until there is data to set it from |
| **B-18** | Business | Claim-data retention |
| **B-19** | Business | The deduction ratio's base — **money** |
| **B-20** | Business | Allocation order against a binding cap — **which lines are cut** |
| **B-21** | Business | Mid-term joiner pro-ration and the counter period — **money** |
| **B-22** | Business | Cumulative unreviewed exposure per member |
| **B-23** | Business | A real benefit-level proviso, and whether the checkpoint volume is intended |
| **B-24** | Business | One document type for class E |

**None of the eleven blocks build, and that is a statement about their shape rather than about their importance.** Each has a stated interim behaviour that is defined, safe and reversible — `L-1` stands, step 11 does not fire, the proposed allocation order is implemented and recorded, the class-E term stays `DRAFT`. **What they block is the pilot being *operated*:** B-07 has nobody working the checkpoint queue, B-18 has no retention answer, B-19 and B-21 change what the product pays, and B-22 governs how much can leave unreviewed. **A decision deferred with a defined interim is a decision; a decision deferred with an undefined one is a defect**, and the seven opened here are all of the first kind.

**Four of the thirteen closed decisions changed the design rather than setting a value**, and each made it simpler or sharper:

- **B-09** turned the deduction ratio from an assessor's discretion into a policy term, removing a ceiling, a permission grant and an override rule, and eliminating discretionary money from MVP 1 entirely (§5.6).
- **B-15** moved the clinical content into IDP's scope, leaving the Claim Platform's clinical work at F56 plus two adapters (§5.11).
- **B-12** kept auto-approval, on a gate that only fires when no AI judgement entered the number — two conditions on 17 August, **three from 24 August, when D-5 added "no open term checkpoint"**, and **four from v0.5, when the duplicate signal was added and the checkpoint condition stopped asking which class raised it** (§5.8).
- **B-17** drew the boundary between the two platforms we consume and the one we build, and it is the only one of the four that changed the *shape* of the solution rather than a rule inside it (§2.1, §7.1).

> **B-17 is also the decision that removed a fallback, and that should be recorded beside it rather than in a risk register alone.** Together with the InsureMO confirmation it deletes the Policy Data Store, and with the store goes the promise that the benefit schedule had somewhere else to live. **InsureMO is a hard dependency for the adjudicable benefit tree** — RK-14, mitigated only by getting hands on a sandbox early (DE-10). A decision that makes the architecture simpler and the dependency sharper is still the right decision; it is not a free one, and a BRD that recorded only the first half would be misleading.

### 11.1 The ten decisions of 24 August 2026

These are not open items. **All ten were taken and approved on 24 August 2026**, on the *Solution Paper — Policy Terms, Rule Engine and Limit Management v1.1*, and this reissue is the result. They are recorded here so that a reader of the BRD alone can see what was decided and where each landed.

| # | Decision | Where it lands in this document |
|---|---|---|
| **D-1** | Build the **term register** as claim-side master data keyed on `ruleSetRef`, rather than taking predicates from InsureMO | §5.3.1, §7.1 |
| **D-2** | Adopt the **three-class model** — class J and class E may **never** auto-decline | §5.3.1 |
| **D-3** | **Fixed chain plus an authorable term layer**; no formula-authoring tool in MVP 1. Declared as **S-39** | §5.3, §10.1 |
| **D-4** | Insert **step 7, benefit-level provisos**. The chain becomes fourteen steps | §5.3 |
| **D-5** | The auto-approval gate gains **"no open term checkpoint"**, accepting a lower straight-through rate. **The decision stands; its expression was wrong and is corrected at v0.5** — it was written as *"no class-J and no class-E term raised one"*, and a class-D term may raise a checkpoint too | §5.8 |
| **D-6** | **Correction versus new version**, with re-evaluation and human routing on a correction | §5.3.1, BR-TERM-5 |
| **D-7** | The limit period is the **policy year**, keyed on **treatment date**; a claim may span two periods | §5.9.1, L-1…L-4 |
| **D-8** | Add **`OCCURRENCE` and `DAY`** counters — otherwise A5's day and visit limits are unenforceable | §5.9.2 |
| **D-9** | **Coordination of benefits across policies is out of scope**, declared as **S-41** with **RK-15** | §10.1, §10.2 GAP-7 |
| **D-10** | **F68**, the term test mode, at Priority 2 rather than 1 | §6.1 |

> **D-9 is the one to read twice, and it is recorded here rather than left in a simplification list.** **A member holding two policies may present the same cost to both, and this platform will not detect it.** The counter key includes `policyId`, so the two sets of counters are independent — which is correct — and that independence is exactly what makes the duplicate invisible. `BR-DUP-1` looks **within** this system and within a policy. Detection needs a **cross-policy duplicate check keyed on member, facility, treatment date and amount**, and **it is not built**. It is declared as **S-41** and it carries a risk of its own, **RK-15**, because a **fraud exposure recorded only in a simplification list is one nobody is accountable for**. What stands in for a mitigation is not a control but a set of pilot conditions — one product, a controlled launch, named testers (**BC-2**) and no real money (**BC-3**) — and **all four expire together**.
>
> **D-10 is the weakest of the ten and was approved as such.** F68 is the control that stops a bad term reaching production, and §5.3.1's governance argues that an approver cannot meaningfully approve a term set whose effect on historical claims they cannot see. It sits at Priority 2 **only** because one product and roughly ten terms make manual review survivable at launch. **If the term count grows materially before go-live it moves to 1.**

**D2 is unblocked** — and delivered: ***D2 Functional Specification v0.6*** is the issue current against this document. The v0.4 changes above reached D2 as the **term register admin**, the **term checkpoint queue**, the **terms-tested panel** on the calculation screen, the **third auto-approval condition**, and the **period dimension** on the `S-11` limit screen with `V-723` extended by `periodKey`.

> **What v0.5 owes D2, and the one thing D2 was owed and is now paid.** **`D2-177` closes**: D2 §5.10.2's submission check 13 and `V-732` blocked the submission of a settlement plan on an open checkpoint, correctly declined to invent the business rule, and asked D1 to ratify or refuse. **It is ratified as `BR-TERM-13`.** Travelling the other way, v0.5 reaches D2 as **the gate's fourth condition and the correction to its third** (`V-733` and §5.11.4), **`BR-AUTO-6`'s operator-queue route for an unverified payout account**, **`BR-CHAIN-1`'s held line and its checkpoint**, the **visit** as the unit of a per-visit cap on the calculation screen, **`BR-LEDGER-9`'s return to `PENDING_APPROVAL` on a re-adjudicated figure**, and the **term register version** on the snapshot panel and in the evaluation record.

---

## Part XII — Change control

This document changes by change request against the Framework. A rule stated here and contradicted elsewhere is a defect in the other document, **except** where the other document is an annex that owns the fact — A1 for terms, A2 for wording, A3 for permissions, A4 for document types and extraction fields, A5 for the pilot product, D3 for interface payloads.

**A new business rule is added here first**, with an identifier, before it appears in D2 or in code. A rule that appears in a screen specification without appearing here is a defect in the screen specification.

*End of D1 v0.5.*
