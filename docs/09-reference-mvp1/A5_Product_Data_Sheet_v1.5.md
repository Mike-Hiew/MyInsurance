# A5 — Product Data Sheet (Pilot Health Product)

**Project:** Digital Health Insurance Platform — MVP 1
**Document:** Annex A5 to the Solution Framework — the pilot product, in full
**Version:** 1.5
**Prepared by:** BnK Solutions — Business Analysis
**Date:** 25 August 2026 (v1.5: the review corrections. **Not one benefit, limit, waiting period or premium figure has moved, again.** One term's predicate is rewritten because it could not fire and would have declined the wrong claim; §8.1 states what the accident redirect costs; §10's collection shapes are made to match the seed; and three product questions the model cannot settle on its own are opened rather than absorbed. v1.4: applies the *Solution Paper — Policy Terms, Rule Engine and Limit Management v1.1*, whose ten decisions were approved on 24 August 2026)
**Companion files:** `A5_Product_Data_Sheet_v1.2.xlsx` (machine-readable benefit tree, premium table and mappings) — **unchanged at v1.2, deliberately.** Not a single benefit, limit, waiting period or premium figure moved at v1.3 or at v1.4, so re-cutting the workbook would create a second artefact to keep in step for no gain
**Supersedes:** *A5 Product Data Sheet v1.4* (24 August 2026)
**Annex to:** *Solution Framework v2.5*
**Companions at this issue:** *D1 BRD v0.5* · *D2 Functional Specification v0.6* · *D3 Integration Contracts v2.3* · *A1 Glossary v1.5* (the closed fact vocabulary, and from v1.5 the step each fact is available from) · *A4 v1.3*
**Companion seed:** `A5_seed_v1.5.json` — **215 records** (v1.4: 209), with an ownership marker per collection
**Classification:** Internal

> **What changed at v1.5, and what did not.** **§1–§5's figures, §9's test script and §11's premium table are untouched for the third issue running**, and §0.1's substitution checklist still gates any figure substitution. **Nine things changed. Two of them alter what the system does; four state a consequence that was true and unwritten; three open a question rather than answer it.**
>
> 1. **`TERM-HC-006`'s predicate is rewritten, and it had two independent defects** (§7.2, §7.3). It tested **`benefitCode`** from a **`PRODUCT`**-scope term — step 5 produces that fact and a `PRODUCT`-scope term runs at step 4, so **the term could not fire at all**, on the one row this document relied on as proof that step 4 works. And where it could have been read it read wrongly: §8.1's accident redirect gives a dental line in an accident episode `benefitCode = 500`, which satisfied *"not under 300"* — **teeth broken in a road accident, auto-declined citing a dental exclusion.** The predicate is now written over `serviceCategory` and `lossType`, both available before the chain starts. **D1 v0.5's `BR-TERM-9` is the general rule; A1 v1.5 §4.11's *available from step* column is the data it is checked against.**
> 2. **§8.1 says what the accident redirect actually costs** — and it is more than a cap and a waiting period. `500` is a childless leaf carrying `annual_cap` alone, so redirecting to it discards **every sub-limit, every count limit and the co-payment** on the way. A 30-day accident admission pays billed cost to the annual cap. **The product decision is the business's and is opened as `A5-Q8`; the consequence is stated here because stating it is ours.**
> 3. **§10's collection shapes are corrected against the seed in five places.** A loader built from the v1.4 table failed on all five, and one of them named a collection — `line_decline_rule` — that has never existed.
> 4. **§6.1 stops saying a chain step may be switched off *per benefit*.** Framework v2.5 **P-05** as revised and **S-39** both say **per product**, and the seed's `adjudication_rule` object has never carried a benefit code. The claim is dropped rather than a column invented.
> 5. **§7.5's illustration said *"10 terms tested"* and the live count is six.** Four of the ten are `DRAFT`, §7.2 already said draft terms are invisible to the engine, and the illustration contradicted it. **The counting rule is now stated rather than implied.**
> 6. **§4.2 records that the visit is a derived unit** — `treatmentDate` plus provider, **`BR-LEDGER-14`**, new at D1 v0.5 — so `OCCURRENCE` increments once per distinct visit rather than once per insured event. Without it a twelve-session course of physiotherapy consumed **one** of `200.3`'s twenty visits.
> 7. **§4.3 and §6.2 say where the deduction ratio is ambiguous** and cite **`B-19`**, rather than presenting a worked example that quietly settles it. **No arithmetic in this document changes**: the example still applies the rate to the aggregate, which is one of the two candidate readings and is the one `B-19` will confirm or replace.
> 8. **Two more open questions are recorded rather than absorbed** — **`A5-Q9`**, one insured event carrying two condition classes, and **`A5-Q8`** above.
> 9. **The seed gains what three contracts already assumed it held**: a rule-set reference on `policy`, a continuous-cover date on `member`, and a `provider` collection with `in_network`. **Chain step 4 selected zero terms in every mock before this**, because no policy record carried the key the term register binds to.
>
> **What v1.5 does not do.** It does not choose between `B-19`'s two readings, it does not decide what an accident episode should pay, and it does not invent a benefit-level proviso to give chain step 7 something to evaluate — **D1 v0.5 asks the business for a real one as `B-23`** and §7.3 records that the step has no instance.

> **What changed at v1.4, and what did not.** §1–§5's figures, §9's test script and §11's premium table are untouched, and §0.1's substitution checklist and its seven mechanisms still stand as the gate on any figure substitution. Six things changed, and the first two are the substantive ones.
>
> 1. **§7 stopped being prose.** Ten English sentences became ten evaluable terms, each with a class, a scope, an effect, a decline reason, a clause reference and — where one is possible — a predicate. Step 4 of the chain had been testing a rule with nothing behind it; it now has something to test. The ten items are unchanged in meaning and remain illustrative.
> 2. **§7 names a defect the platform already had.** `SUPPLEMENT` and `ADMIN` were declined by the mapping table in §8.1 — at the wrong step, with the wrong code, citing no clause, and ignoring the wording's own proviso. §7.4 sets out what was wrong and what replaces it.
> 3. **§6.1 is fourteen steps.** Benefit-level provisos are inserted at 7 and everything after it renumbers. §6.2's base amounts were re-checked against the new numbers rather than merely renumbered, and the BHYT note's reserved slot moves from 7 to 8 so the two cannot collide.
> 4. **§6.1's header sentence was wrong and is corrected.** The order was never configuration.
> 5. **§4.2 says what enforces the count and day limits.** They had been specified with no counter rule behind them.
> 6. **§6.4's auto-approval gate gains a third condition**, and §8.1 gains four service categories and loses its two decline rows.
>
> **Corrected the same day, 24 August 2026, without a version bump.** Two modelling conflicts were found while the prototype was built against this issue. **One is settled in this document's favour and one against it.** (a) **D2 §5.29.2 read that a class-D term never raises a checkpoint. §7.1 is right and D2 has yielded** — `termClass` and `effect` are orthogonal, and the surviving rule is one-directional: a class `J` or `E` term may never carry `DECLINE_EVENT` or `DECLINE_LINE`. (b) **§7 carried the single-`predicate` model and D2 §5.28.2 was right that it is unbuildable.** The **`selector` / `predicate` split** is adopted at §7.1, §7.2 and in `A5_seed_v1.5.json`, D2's open item **`D2-173`** closes, and **§7.3 settles how the two irreducibly judgemental terms are selected** rather than leaving it open. **No benefit, limit, waiting period, premium figure, mapping row or term meaning is touched by either.**
>
> The v1.3 change stands unaltered underneath all of this: the Policy Data Store is not built (Framework §2.1), §9 records its own result, and §10 says where each part of the model lives rather than which table of a store it lands in.

---

## 0. What this document is, and what it is used for

This is the **complete definition of the one health product MVP 1 will sell and pay claims on**. It is an original construction, designed to exercise every mechanism the claim engine must support. It is not derived from, and carries no codes, names or figures of, any client's product.

It has four jobs, and each shapes how it is written:

| Used by | For what |
|---|---|
| **The InsureMO evaluation** | The artefact InsureMO configured live. §9 is the test script — **run 21 August 2026, all eight tests passed** |
| **The benefit calculation engine (F48)** | The adjudicable product model — the tree, the limits, the rule order, the base amounts |
| **The InsureMO product configuration, and the mock behind it (F01, F07)** | Seed data. §10 says which system holds each element, and `A5_seed_v1.5.json` carries the records with an ownership marker per collection |
| **The BRD, Part V** | The worked source for the calculation chapter |

**Status (17/08/2026, D1 B-01): the structure is confirmed; the figures are to be replaced from a real product.**

**Status (21/08/2026, C-IMO-01): the structure is not only confirmed, it is demonstrated.** InsureMO configured this schedule in its own product model, against this document, benefit by benefit. That is why the Policy Data Store is not built — and it is also why this document now carries more weight than it did: it is the artefact the demonstration was judged against, so a change to §4 or §5 is a change to something a vendor has already been shown to support.

### 0.1 Before substituting figures — the check that must pass

This product was constructed so that **every mechanism in the benefit engine has at least one benefit that exercises it**. Replacing the amounts is free. Replacing the *structure* is not: an engine branch that no benefit reaches is a branch nobody tests, and SC-06 — a second product configurable with no code change — is then unproven rather than proven.

Before any substitution, confirm the replacement still exercises all seven:

| # | Mechanism | Where A5 exercises it today |
|---|---|---|
| 1 | **Three-level tree** with grandchild roll-up | `100 → 100.1 → 100.1.1` |
| 2 | **Count limit in days** | `100.1` 60 days; `100.1.1` 15 days |
| 3 | **Count limit in visits** | `200.3` 20 visits |
| 4 | **Unlimited sentinel (`0`)** | Every benefit with no count cap |
| 5 | **Co-payment inherited down the tree** | `200` 20% inherited by `200.1/.2/.3` |
| 6 | **Co-payment differing by plan tier** | `300` — 30% / 30% / 20% |
| 7 | **Waiting periods by condition class**, including `n/a` combinations | §5, six classes across five benefit groups |

**If the real product does not reach one of these, keep a benefit that does** — even a small one — or accept in writing that the mechanism ships untested.

> **v1.3: this checklist and all seven mechanisms are unaffected by the deletion of the Policy Data Store, and are stated as unaffected so no reader has to work it out.** They are properties of the *product*, and the product did not change. What did change is that each mechanism now has a second reason to survive substitution: **all seven are also what InsureMO was asked to demonstrate** (§9, tests T1–T7), so a substitute product that drops one drops the evidence behind **C-IMO-01** as well as an engine branch. With no fallback left (**RK-14**), that evidence is the only thing standing behind the benefit tree.

---

## 1. Product identity

| Attribute | Value |
|---|---|
| Product name | **HealthCare Pilot** (working name) |
| Product code | `HC` |
| Line of business | Health — individual, indemnity |
| Currency | VND only |
| Policy term | 12 months |
| Plan tiers | 3 — `HC-STD` Standard · `HC-PLS` Plus · `HC-PRM` Premium |
| Sale channels in MVP 1 | Mobile app (individual). Group policies are loaded as data, not sold |
| Claim types supported | Reimbursement only |

**Deliberately excluded from the product**, because the corresponding claim types are out of MVP scope: death benefit, permanent disability, critical illness, income replacement, hospital cash. **No benefit in this product is payable "by scale"** — that would require the statutory disablement percentage schedule, which is not in the MVP data model.

---

## 2. Eligibility

| Rule | Value |
|---|---|
| Entry age | 0 to 65 (age at inception, last birthday) |
| Renewal age | To 70 |
| Residency | Vietnam resident |
| Children | A child aged 0–17 may only be insured together with at least one adult on the same policy |
| Maternity benefit | Applies only to a female insured aged 18–45 at inception. On other insureds the benefit group is present but with a zero limit |
| Members per policy | 1 to 6 (self, spouse, children, parents) |
| Underwriting | **Declaration only** — see §3. No medical examination, no underwriter referral |

---

## 3. Health declaration and the eligibility screen

Five questions. **Any "yes" declines the application** with a message; there is no referral queue in MVP 1.

| # | Question (EN) | Question (VI) |
|---|---|---|
| Q1 | Are you currently receiving, or have you been advised to receive, treatment for any condition? | Anh/chị hiện đang điều trị hoặc được chỉ định điều trị bệnh lý nào không? |
| Q2 | Have you been hospitalised or had surgery in the last 24 months? | Trong 24 tháng qua anh/chị có nằm viện hoặc phẫu thuật không? |
| Q3 | Have you been diagnosed with cancer, heart disease, stroke, diabetes, kidney or liver disease, or HIV? | Anh/chị đã được chẩn đoán ung thư, bệnh tim, đột quỵ, tiểu đường, bệnh thận, bệnh gan hoặc HIV chưa? |
| Q4 | Do you have a congenital condition or disability? | Anh/chị có bệnh bẩm sinh hoặc khuyết tật không? |
| Q5 | Are you currently pregnant? | Anh/chị hiện có đang mang thai không? |

*Q5 does not decline the application; it sets `maternity_eligible = false` for that insured, because the childbirth waiting period could not be satisfied.* This is the one exception and it must be implemented as such.

---

## 4. The benefit tree

**Read this alongside §5.** A benefit's limit is not the whole story — the rule order in §6 determines how the limits combine.

Notation: `included` means the benefit has no annual cap of its own and is capped by its parent. `0` in a count column means **unlimited**, per the sentinel convention. `—` means not applicable.

### 4.1 Structure

```
100     Inpatient treatment                          annual cap
 ├ 100.1    Room and board, per day                  per-day cap · day count
 │  └ 100.1.1  Intensive care, per day               per-day cap · day count
 ├ 100.2    Surgery, per procedure                   per-event cap
 ├ 100.3    Pre- and post-hospitalisation            per-event cap
 └ 100.4    Emergency transport                      per-event cap
200     Outpatient treatment                         annual cap · co-payment
 ├ 200.1    Consultation and diagnostic tests        per-visit cap
 ├ 200.2    Prescribed medication                    per-visit cap
 └ 200.3    Physiotherapy                            per-visit cap · visit count
300     Dental — basic treatment                     annual cap · co-payment
 └ 300.1    Basic treatment, per visit               per-visit cap
400     Maternity                                    annual cap
 ├ 400.1    Normal delivery                          per-event cap
 ├ 400.2    Caesarean section                        per-event cap
 └ 400.3    Pregnancy complications                  per-event cap
500     Accident — medical expenses                  annual cap
```

### 4.2 Limits by plan tier (VND, illustrative)

| Code | Benefit | Limit type | Standard | Plus | Premium | Count limit | Co-pay |
|---|---|---|---|---|---|---|---|
| **100** | **Inpatient treatment** | annual | 100,000,000 | 200,000,000 | 400,000,000 | 0 | — |
| 100.1 | Room and board, per day | per day | 1,500,000 | 2,500,000 | 4,000,000 | 60 days | — |
| 100.1.1 | Intensive care, per day | per day | 3,000,000 | 5,000,000 | 8,000,000 | 15 days | — |
| 100.2 | Surgery, per procedure | per event | 20,000,000 | 40,000,000 | 80,000,000 | 0 | — |
| 100.3 | Pre- and post-hospitalisation | per event | 3,000,000 | 5,000,000 | 8,000,000 | 0 | — |
| 100.4 | Emergency transport | per event | 2,000,000 | 3,000,000 | 5,000,000 | 0 | — |
| **200** | **Outpatient treatment** | annual | 20,000,000 | 35,000,000 | 60,000,000 | 0 | 20% / 20% / 10% |
| 200.1 | Consultation and diagnostic tests | per visit | 1,500,000 | 2,500,000 | 4,000,000 | 0 | inherits |
| 200.2 | Prescribed medication | per visit | 1,000,000 | 1,500,000 | 2,500,000 | 0 | inherits |
| 200.3 | Physiotherapy | per visit | 500,000 | 800,000 | 1,200,000 | 20 visits | inherits |
| **300** | **Dental — basic treatment** | annual | 5,000,000 | 8,000,000 | 15,000,000 | 0 | 30% / 30% / 20% |
| 300.1 | Basic treatment, per visit | per visit | 1,000,000 | 1,500,000 | 2,500,000 | 0 | inherits |
| **400** | **Maternity** | annual | 20,000,000 | 35,000,000 | 60,000,000 | 0 | — |
| 400.1 | Normal delivery | per event | 12,000,000 | 20,000,000 | 35,000,000 | 0 | — |
| 400.2 | Caesarean section | per event | 20,000,000 | 35,000,000 | 60,000,000 | 0 | — |
| 400.3 | Pregnancy complications | per event | 20,000,000 | 35,000,000 | 60,000,000 | 0 | — |
| **500** | **Accident — medical expenses** | annual | 50,000,000 | 80,000,000 | 150,000,000 | 0 | — |

#### What enforces the count and day limits — stated at v1.4, because nothing did before

Until v1.3 the count column above, and the `per_day_cap` and `per_event_cap` columns behind it, were specified with **no counter rule underneath them**. The data existed on all 51 benefit schedule rows, chain step 9 tested it, and D2's limit screen `S-11` displayed *"remaining days"* — while `BR-LEDGER-1…6` governed money and nothing else. So the chain tested a counter no rule maintained and a screen showed a figure no rule produced, **which is worse than showing nothing, because an assessor will believe it.** Closed as gap **G3**, decision **D-8**.

| | Rule |
|---|---|
| Counter key | `(memberId, policyId, benefitCode, periodKey, counterType)` |
| `counterType` | `AMOUNT` · `OCCURRENCE` · `DAY` |
| `count_limit` where `count_unit = visits` — e.g. `200.3`, 20 visits | An **`OCCURRENCE`** counter, incremented **once per distinct visit** — **restated at v1.5**, see below |
| `count_limit` where `count_unit = days` — `100.1` 60 days, `100.1.1` 15 days | A **`DAY`** counter, incremented by the treatment-day count of the line |
| `per_day_cap` | A money cap tested against the same **`DAY`** dimension |
| `per_event_cap` | A money cap tested against the same **`OCCURRENCE`** dimension |
| Governing rules | **`BR-LEDGER-1…8`**, restated to cover all three counter types rather than money alone — two-phase (provisional at calculation, final at approval), never deleted but reversed, versioned for optimistic concurrency, and posted to **every counter on the ancestor path** |
| Counter period | The **policy year**, derived from the policy inception date on the snapshot — **L-1**. Not the calendar year |
| Which period a line falls in | The period containing its **treatment date**, never its submission date — **L-2** |
| A claim whose lines span a boundary | Posts to **two periods**, line by line — **L-3**. It is one claim with two sets of postings, and the remaining-limit figure shown at calculation is then not a single number |
| `periodKey` | Stored on every posting and immutable once posted — **L-4** |

#### The visit is a derived unit — restated at v1.5, because *"one per qualifying event"* could not enforce a visit limit

**D1 v0.5 introduces the visit as a derived unit (`BR-LEDGER-14`) and this section is brought into line with it.** Until then the `OCCURRENCE` counter incremented *"once per qualifying event"*, and an insured event is **one provider, one date range, one diagnosis set** — so **a twelve-session course of physiotherapy is one insured event.** Under the old rule that course incremented `200.3`'s counter by **1** against a limit of **20**, and its 500,000 per-visit cap was applied **once to the whole course** instead of twelve times. The same defect sat on `200.1`, `200.2` and `300.1` — **every benefit in this product whose limit is expressed in visits.**

| | Rule |
|---|---|
| **What a visit is** | **A cost line's `treatmentDate` together with its provider.** Lines sharing both are **one visit** — `BR-LEDGER-14` |
| **Derived, not captured** | Both facts are on every cost line already (A4). **Nothing is asked of extraction and no new field enters the model** |
| **`OCCURRENCE` increments** | **Once per distinct visit** |
| **A per-visit cap (step 8)** | Applied **per visit**, on any benefit whose `count_unit` is `VISIT` |
| **The convention it fixes** | **Two visits to the same provider on the same date are one visit.** That is the conventional reading of a per-visit cap; it is written down so that it is a decision rather than an implementation detail |

**The crosswalk between this table's `count_unit` and the ledger's `counterType` is registered in A1 v1.5 §4.10**, not here: `VISIT` and `OCCURRENCE` are the same dimension under two names, one belonging to the benefit schedule and one to the ledger, and until v1.5 the mapping existed only in this section's prose.

**L-2 is the one that must not be got wrong.** Keying on submission date would let a member reset their own limit by delaying a claim, and would make the same treatment consume different periods depending on how fast the customer filed.

**One dependency, and it is open.** The `DAY` counter needs a treatment-day count per line, which extraction does not return today — **Q-IDP-06**. Until it does, mechanism 2 of §0.1 — the count limit in days, the one `100.1` and `100.1.1` exist to exercise — cannot be maintained automatically, and the 60-day and 15-day counts stand as data with a manual step behind them.

### 4.3 Deduction ratio — a policy term, not a discretion

Decided 17/08/2026 (D1 B-09). The deduction ratio **comes from the policy** and applies **only when its stated condition is met**. No assessor sets it, and there is no ceiling to configure — the contract says what it is.

| Condition | Deduction | Applies to |
|---|---|---|
| Treatment at a **provider outside the listed network** | **20%** *(illustrative — a figure, replaced with the rest under D1 B-01)* | Benefit groups `200`, `300` |
| All other circumstances | none | — |

**Where the condition's answer comes from.** `PROVIDER_OFF_NETWORK` is evaluated from the **provider recorded on the insured event**, against an `in_network` flag on the **provider master** (Claim Platform master data, F57, maintained by an administrator — A3 §3.1). **It is not typed by the assessor**, and it is not a judgement: the provider is either on the list or it is not. A provider absent from the master is treated as **off-network**, and that is deliberate — the safe default is the one that does not silently pay at the higher rate.

**Where the network list itself lives.** From seed v1.5 the provider master is in the shared fixture as the **`provider`** collection — **Claim Platform master data (F57)**, six rows, with `in_network` and one deliberately off-network provider so that step 11 is testable in both directions. Until v1.5 it was not there at all, and D2 §5.17 states what that costs: *if the provider master is not loaded, every claim the deduction reaches is cut by 20%* — because a provider absent from the master is treated as off-network, which is the right default and the wrong outcome when the reason for the absence is that nobody loaded the file.

**How it behaves in the chain (step 11):** the rate applies to the amount remaining *after* co-payment, and the **condition that triggered it is recorded on the claim** so the settlement letter can state why. Where no condition is met, the step is skipped entirely — it is not a zero-rate deduction, it is no deduction.

> **The one thing this section does not settle, and it changes money: *which* benefit's rate applies to a cost line. `B-19`, and it is open.**
>
> `deduction_pct` sits on **`200` and `300` only** — both parents — and **every cost line maps to a child**: `200.1`, `200.2`, `200.3`, `300.1`. Nothing in the fourteen steps applies an ancestor's deduction ratio to a descendant's amount. **Read step 11 as *"the line's own benefit"* and the deduction never fires on this product at all**, because no benefit a line can map to carries one. **Read it as *"the benefit or an ancestor"* — the way step 10 states it for co-payment — and it fires as §6.2's worked example computes it.**
>
> **The inheritance contract is stated for co-payment and for nothing else**, and that is the root of it: §4.3's third structural point below says co-payment inherits down the tree, D3 §I-05 carries an explicit `"INHERIT"` marker for it on the wire, and the deduction pair — `deduction_pct` and `deduction_condition` — has neither.
>
> **D1 v0.5 opens `B-19` with two candidate rules, both defensible, and this annex does not choose:** either `deduction_pct` gains an `"inherit"` sentinel exactly as `copay_pct` has and step 11 stays per line; or an explicit aggregation applies the rate **at the node that carries it, to the sum of its descendants' payables**, which is what §6.2 actually computes and which additionally changes the base of step 12. **They give different money** wherever rounding or a per-line cap intervenes. **The interim behaviour is the safe one and it is what the seed produces today: step 11 does not fire.** It over-pays rather than under-pays.

> **This is the one mechanism in the product that is conditional on a fact about the episode rather than on the benefit alone.** It is therefore the mechanism most likely to be got wrong: the engine must evaluate the condition per insured event, not per claim, because one claim can carry a network visit and a non-network visit.

**Why the ceiling and the grant disappeared.** An earlier draft made this an assessor's discretionary reduction, which needed a maximum rate, a permission grant and an override rule. Sourcing it from the policy removes all three, and it removes discretionary money from MVP 1 entirely: **every reduction on a settlement now traces to a term of the contract.** An assessor who thinks a line should be paid in part has exactly one mechanism — decline the line, with a reason (§6.3).

**Three structural points the engine must honour:**

1. **`included` benefits have no annual cap of their own.** `100.1` room and board is capped at 1,500,000 per day and by the 100,000,000 annual cap of `100` — nothing in between.
2. **Co-payment is inherited down the tree unless overridden.** `200.1`, `200.2` and `200.3` all inherit the 20% co-payment of `200`. A child may override with its own rate; none does in this product.
3. **`100.1.1` intensive care is a child of `100.1` room and board.** A day in ICU consumes the ICU per-day limit, *and* one day of the 60-day room-and-board count, *and* the annual inpatient cap. This is the single best test of whether a platform implements the tree correctly.

---

## 5. Waiting periods

Waiting periods are **per benefit group and per condition class**, not a single value per benefit. This is the second structural requirement and the one most often missed.

| Condition class | Code | Inpatient (100) | Outpatient (200) | Dental (300) | Maternity (400) | Accident (500) |
|---|---|---|---|---|---|---|
| Pre-existing condition | `PRE` | 365 d | 365 d | 365 d | n/a | 0 d |
| Specified condition *(see list)* | `SPE` | 365 d | 365 d | n/a | n/a | 0 d |
| General illness | `GEN` | 30 d | 30 d | 90 d | n/a | 0 d |
| Childbirth | `BIR` | n/a | n/a | n/a | 270 d | n/a |
| Maternity complication | `MAT` | n/a | n/a | n/a | 90 d | n/a |
| Accident | `ACC` | 0 d | 0 d | 0 d | n/a | 0 d |

**Specified conditions (`SPE`)** — illustrative list, to be confirmed by the business: hernia of all types · haemorrhoids · tonsils and adenoids · sinusitis · nasal septum deviation · gallstones · kidney and urinary stones · benign tumours and cysts · uterine fibroids · endometriosis · cataract · thyroid disorders.

**Rules:**

- The waiting period is measured from the **member's coverage start date**, not the policy inception date. On a renewal without a break in cover, waiting periods already served are not re-served.
- The class is determined at assessment time and stored on the insured event (`condition_class`).
- A claim that fails the waiting period is declined with reason `WAIT` — *"Chưa đủ thời gian chờ"*.
- `n/a` means the combination cannot arise; if it does, it is a data error, not a zero-day waiting period.

> **One insured event carries exactly one condition class, and there are real events that carry two. `A5-Q9`, and it is open — §12.**
>
> A **caesarean section performed because of a pregnancy complication** is one event, one admission, one set of cost lines, and it is `BIR` — **270 days** — and `MAT` — **90 days** — at the same time. The model holds one `condition_class` on the insured event, so whichever is stored, **one of the two benefits is served the wrong waiting period**, and the direction of the error depends on which was chosen rather than on any rule. The same shape arises without maternity: an admission that treats a **`PRE`** condition and an unrelated **`GEN`** condition is one event with one class and two correct answers.
>
> **This is a product-model question, not an implementation detail**, which is why it is opened rather than resolved here: the two candidates are *(a)* accept the simplification and declare it — one class per event, chosen by a stated precedence rule, with the assessor able to split the episode into two insured events where it matters — or *(b)* move `conditionClass` from the insured event to the **cost line**, so each line is served its own waiting period. **(b) is the correct model and it is not free**: `conditionClass` is an A1 §4.11 fact at **event** level, D2's assessment screen captures it once per event, and every rule and screen that reads it would move with it. **The interim behaviour is (a) with no precedence rule written down**, which is the part that must not survive the decision.

---

## 6. Adjudication configuration

### 6.1 The rule chain

**The sequence below is fixed in code and versioned with the release. What is configurable is *participation* — each step may be enabled or disabled **per product** — and *terms*, which are data (§7).**

> **Two corrections at v1.5, and both are to this paragraph rather than to the chain.**
>
> **1. *"Switched off per benefit"* is withdrawn.** Framework v2.5's **P-05** as revised and **S-39** both say **per product** — *"each step may be enabled or disabled per product, and that is the whole of what `adjudication_rule` configures"* — and the seed's `adjudication_rule` object is `{step, name, enabled}` and has never carried a benefit code. The choice was between adding a column to fourteen rows that would all read *all benefits* and dropping a claim no principle supports; **the claim is dropped.** §10's shape is corrected to match.
>
> **2. *"Applied per cost line, then aggregated"* is withdrawn as a blanket statement.** It is true of most steps and **false of steps 8 and 11**: step 8 is per visit or per event, step 9 is per benefit per counter period, step 11 is per insured event, and step 12 is per benefit across the whole claim. **D1 v0.5 §5.3 carries a granularity column per step and is normative for it.** An implementer given a base *amount* and no base *set of lines* can write five defensible engines that return five different settlements, which is the same class of defect §6.2 was written to close one column over.

> **This replaces the claim made here until v1.3**, that *"the order is configuration"*. It was overstated, and it would have given a reader and a developer a false expectation. The seed's `adjudication_rule` object is `{step, name, enabled}` — an on/off list against a fixed sequence — and it never expressed an order at all. Framework **P-05 is revised to the same wording**, and the consequence is declared as **S-39**: a change to the arithmetic, or to the order of steps, is a release, where a change to a term is same-day data.

| # | Step | Applies | Notes |
|---|---|---|---|
| 1 | Member eligibility | Always | Member active on the service date; policy in force; premium recorded paid |
| 2 | Waiting period | Always | By condition class per §5 |
| 3 | Pre-existing condition rule | If `condition_class = PRE` | Subject to the 365-day waiting period; not an outright exclusion in this product |
| 4 | General exclusions | Always | Evaluated by the term engine against the term register, §7. Every term in scope is tested and every result recorded, whether it fires or not |
| 5 | Benefit mapping | Always | Cost line → benefit code, per §8. **It no longer declines anything** — §7.4 |
| 6 | **Line-level cover test** | Always | Each cost line is payable or declined **individually**, with a reason. §8.2 |
| 7 | **Benefit-level provisos** | If the benefit carries one | Evaluated by the term engine, scope = `BENEFIT`, §7 |
| 8 | Per-visit / per-day / per-event sub-limit | If the benefit has one | Capped at the child benefit's limit |
| 9 | Count limit | If the benefit has one | Days or visits remaining; `0` = unlimited. Tested against the `OCCURRENCE` and `DAY` counters — §4.2 |
| 10 | Co-payment | If the benefit or an ancestor has one | **Base amount: the amount remaining after step 9** |
| 11 | **Deduction ratio** | If the benefit carries one **and its condition is met** | **From the policy**, not from the assessor. Base: the amount remaining after step 10. The triggering condition is recorded on the claim. See §4.3 and Framework §7.6 |
| 12 | Parent annual cap | Always | Walk the ancestor chain to the root |
| 13 | Plan-level check | Always | No plan-wide aggregate cap in this product; the step exists for future products |
| 14 | Post to ledger | Always | Provisional at calculation; final at approval. Posts to the benefit **and every ancestor**, and to every counter type on that path (§4.2). A composite and its own sub-limit are never counted twice on the same claim line |

**Fourteen steps from v1.4, and step 7 is the new one** (decision **D-4**). Two things about where it sits are deliberate. **Provisos come after the line-level cover test, not before it** — a line that is not covered at all should be declined on that ground, not on a proviso it was never going to reach. And a proviso is neither a general exclusion nor a numeric cap, so until now *"outpatient physiotherapy only on referral"* or *"maximum 30 days per event"* had nowhere in this product to live. Everything from the old step 7 renumbers by one; **§6.2's base amounts were re-derived against the new numbering rather than renumbered on sight**, and the derivation is shown there.

> **On BHYT — the step that is deliberately absent.** A Vietnamese hospital cost statement decomposes each line by payer: `BHYT chi trả`, `Người bệnh cùng chi trả`, `Người bệnh tự trả`. **MVP 1 does not deduct the BHYT share** (declared simplification S-17, decided 17/08/2026). The chain therefore runs on the **billed** amount, not on what the member actually bore.
>
> Where it would go, when it is turned on: a **new step 8**, after the benefit-level provisos and before the sub-limit — so that an inappropriate line is declined first, the BHYT share is removed second, and the caps then apply to the residual. **The chain becomes fifteen steps.** Everything after it is unchanged, which is why this is a deferral rather than a redesign.
>
> **The reserved slot moved at v1.4, and the move is the point of writing it down.** Until v1.3 this note reserved *step 7*. Benefit-level provisos are being built now and BHYT is not, so provisos took step 7 and BHYT's reservation moved to 8. Had the note been left as it stood, two different rules would have claimed one position in a sequence that is fixed in code, and nobody would have found out until the day someone switched BHYT on.
>
> **The consequence, stated plainly:** on a claim where BHYT paid, MVP 1 pays more than an indemnity product should. That is affordable only because boundary condition BC-3 means no real money moves. It is not affordable in production.

### 6.2 Base amounts for every percentage — stated explicitly

Ambiguity here is the commonest defect in health claim specifications, so it is settled in writing:

| Percentage | Base amount |
|---|---|
| Co-payment (**step 10**) | The **amount payable after all sub-limits and count limits have been applied** — that is, the amount remaining after **step 9** — and after any line-level decline at step 6 and any exclusion or proviso decline at steps 4 and 7. *Not* the amount billed |
| Deduction ratio (**step 11**, from the policy) | The amount remaining **after** co-payment has been deducted at **step 10** — i.e. the insurer's share, not the billed amount. **The base *amount* is settled; the base *set of lines* is not — `B-19`, and §4.3 states it** |

> **Re-derived at v1.4, not renumbered on sight.** Inserting a step at 7 shifts every later number by one, and a base amount that still names the old number is the most damaging error this section can carry — it would be arithmetically silent and would show up only as a wrong settlement. So each base was traced again from first principles. **Co-payment's base is the amount after the count-limit step**: the count limit is now step 9 (was 8), so co-payment at step 10 takes the amount remaining after step 9. **The deduction ratio's base is the amount after co-payment**: co-payment is now step 10 (was 9), so the deduction ratio at step 11 takes the amount remaining after step 10. **Neither base amount changed. Only the numbers naming the steps did**, and the relationship *"co-payment follows the count limit, the deduction ratio follows co-payment"* is the one that is normative — if a future insertion moves the numbers again, that sentence is what must be preserved.

Both come from the policy and neither is editable by an assessor. They differ in **when** they apply: co-payment always, the deduction ratio only when its stated condition is met. Framework §7.6 sets out the full distinction. **There is no discretionary reduction in this product.**

> **Rounding — stated once, because two implementations of the same fourteen steps must agree.** Every step that produces a money amount rounds to the **nearest whole dong, half away from zero**, and rounding happens **at each step, not once at the end**. This matters for BR-TRACK-4: the rule chain re-runs after a clinical recommendation is confirmed, and a chain that rounded only at the end could return a different total from the first pass on the same inputs. Percentages are held as integer basis points (D3 §1.2); the rounding applies to the money result, never to the rate.

**Worked example.** Outpatient visit, Standard plan. Four lines billed at 3,000,000: consultation 300,000 and laboratory 1,400,000, both mapping to `200.1`; medication 900,000, mapping to `200.2`; and a dietary supplement at 400,000.

| Step | What happens | Running total |
|---|---|---|
| 4 | The dietary supplement raises a **term checkpoint** on `TERM-HC-003`. The assessor confirms it was not prescribed as treatment for the diagnosed condition, so the line is declined **`EXCL`**, citing the clause | 2,600,000 |
| 8 | `200.1` holds 1,700,000 against a 1,500,000 per-visit cap — **200,000 cut**. `200.2` holds 900,000, under its 1,000,000 cap | 2,400,000 |
| 10 | 20% co-payment on 2,400,000 — the member bears 480,000 | **1,920,000** |
| 11 | Skipped — the provider is in-network, so no deduction condition was met | 1,920,000 |
| 12 | 1,920,000 checked against `200`'s remaining annual cap | 1,920,000 |
| 14 | Posted provisionally: 1,200,000 to `200.1`, 720,000 to `200.2`, both rolling up to `200` | remaining 18,080,000 |

**What moved in this example at v1.4, and what did not.** No figure moved: the payable is still 1,920,000. The supplement used to be declined at **step 6** with **`NOTCOV`** — *"not a covered medical item"*. It is now stopped at **step 4** with **`EXCL`** and a clause reference, because the ground is an exclusion and always was. And the outcome is no longer automatic: had the assessor found the supplement **was** prescribed for the diagnosed condition, the line would have been payable under `200.2`, which would then have held 1,300,000 against its 1,000,000 per-visit cap — a different settlement, reached by the wording rather than by the mapping table. §7.4 explains why that mattered enough to change.

**The cap must be applied per benefit, not to the claim total.** That is the point of choosing these figures: `200.1` breaches its per-visit cap and `200.2` does not, so an engine that capped 2,600,000 at 1,500,000 would under-pay by 900,000 and look, from the outside, exactly like one that got it right.

**Note what else this example shows.** The payable amount is under the 2,000,000 auto-approval threshold — but a line was declined *and* a limit bound, so condition 1 of §6.4 fails and the claim reaches an assessor. It now fails **condition 3** as well, because a term checkpoint was opened at step 4. **A cheap claim is not automatically a clean one**, and that distinction is the whole reason condition 1 has two halves.

**The same example with a deduction.** If the visit had been at a provider outside the listed network, the policy's 20% deduction ratio would apply at **step 11**, reducing 1,920,000 to **1,536,000** — and that, not 1,920,000, is what **steps 12 and 14** carry. Nothing about it is a judgement; the condition either held or it did not.

> **Read that last paragraph against `B-19` before building from it.** It applies the 20% to **1,920,000 — the aggregate of two lines, `200.1` and `200.2`, after co-payment.** Neither of those benefits carries a `deduction_pct`; **`200`, their parent, does.** So the arithmetic above is the **ancestor** reading of step 11, applied to a quantity that step 12 is the one that aggregates. **Under the other reading — step 11 tests the line's own benefit — nothing fires and the payable stays 1,920,000.**
>
> **The arithmetic is deliberately left exactly as it is**, because it is the reading that produces a deduction at all and therefore the one that shows the mechanism working. **What is not left implicit is that it is a reading.** `B-19` will confirm it or replace it with the `"inherit"` sentinel on `deduction_pct`, and if it replaces it, this paragraph's 1,536,000 is computed line by line and may differ once each line's own cap and the rounding rule have been applied. **The interim behaviour in the built system is that step 11 does not fire** — over-paying rather than under-paying — which is what the seed produces and what D1 v0.5 records.

### 6.3 Decline reasons

| Code | Reason (EN) | Reason (VI) | Level |
|---|---|---|---|
| `LIMIT` | Benefit limit exhausted | Hết hạn mức | Event or line |
| `PERIOD` | Event date outside the cover period | Ngày xảy ra sự kiện BH không thuộc thời hạn BH | Event |
| `NOBEN` | The plan does not include this benefit | Chương trình bảo hiểm không bao gồm quyền lợi này | Event or line |
| `WAIT` | Waiting period not yet served | Chưa đủ thời gian chờ | Event or line |
| `EXCL` | Falls under a policy exclusion | Thuộc điểm loại trừ của quy tắc BH | Event or line |
| `NOTREL` | Not related to the diagnosis or treatment | Không liên quan đến chẩn đoán và điều trị | **Line only** |
| `NOTCOV` | Item is not a covered medical item | Không thuộc danh mục được chi trả | **Line only** |

The last two exist only at cost-line level and are what the AI-drafted rationale populates.

### 6.4 Auto-approval

**Revised 17/08/2026 (D1 B-12): auto-approval is in MVP 1. Revised again 24/08/2026 (decision D-5): the gate has three conditions, not two.** A claim approves without an assessor when **all three** hold:

| # | Condition | Value |
|---|---|---|
| **1** | Total payable at or below the configured threshold, **and nothing was cut** — no cost line declined, no limit bound | **2,000,000 VND** (D1 B-03), changeable by an administrator without a release |
| **2** | The clinical appropriateness assessment **completed and returned no recommendation** | — |
| **3** | **No term checkpoint is open** — added at v1.4 | — |

Anything else routes to an assessor **with the failing condition named**.

**Why condition 3 exists, and what it costs.** A class-J term is by definition one no engine may decide (§7.1). If an open checkpoint did not block the gate, the platform would auto-approve a claim on which a judgement was required and never made — precisely the failure the three-class model exists to prevent. The cost is real and was accepted with it stated: **any claim touching a judgemental exclusion leaves the straight-through path**, and with three judgemental exclusions out of ten, cosmetic treatment among them, that will affect a measurable share of dental and dermatology claims.

Where Track 2 raised nothing, the payable amount came from this rule chain alone — no AI judgement entered it, so there is nothing for a human to confirm. **Extraction confidence is deliberately not a condition of the gate**: it drives assessor routing only (A4 §7). D1 §5.8 states the rules and the full list of what the gate does not test.

---

## 7. Exclusions — as terms the engine can evaluate

**Illustrative and to be confirmed with the business.** Ten items, deliberately few, because every exclusion is a rule someone must implement and test. The caveat is unchanged and it still governs everything below: **none of this wording is filed, and A5-Q3 is still open** (§12).

**What changed at v1.4.** Until v1.3 this section was ten sentences of English prose, and the seed carried them as `product_exclusion` — `product_code`, `seq`, `text_en`. There was no predicate, no code, no scope and no effect, so **nothing in the system could evaluate step 4 of the chain.** A developer handed that specification would have hard-coded ten `if` statements against fields we had not agreed to capture, or quietly skipped the step. **The ten items are unchanged in meaning.** What they now carry is a class, a scope, an effect, a decline reason, a clause reference, a **`selector`** on every row and — where the term may decline — a **`predicate`**. They live in the seed as `policy_term`, **Claim Platform master data (F65)**, keyed on the `ruleSetRef` InsureMO returns on I-05: InsureMO owns *which* terms apply, we own *what they mean operationally* (decision **D-1**).

### 7.1 The three classes, and the one rule that matters

| Class | Meaning | The engine may | The engine may never |
|---|---|---|---|
| **D — deterministic** | Decidable from facts the claim already holds | Decline automatically | — |
| **J — judgemental** | Requires a person, or a clinical opinion | Raise a **checkpoint** with the clause quoted | Decline automatically |
| **E — evidential** | Decidable only if a named document says so | Require the document; raise a checkpoint if it is absent | Decline automatically |

**Only class D may auto-decline. J and E may never** (decision **D-2**). A class-J term auto-declined is a decline citing a clause on a judgement no human made — invisible to us, and a regulatory and reputational exposure. This is the same posture as AR-1…AR-4, extended from clinical assessment to policy terms: **the engine proposes, a person decides.**

**`termClass` and `effect` are two axes, not one**, and the seed carries them as two columns for that reason. `termClass` answers *can the system decide this?* — which is what governs the auto-decline. `effect` answers *what happens when the term is in play*. A class-D term may carry `CHECKPOINT`, and two of the ten do: the clause selects the line deterministically and then decides it judgementally. What is forbidden is the other direction — **a class-J or class-E term carrying `DECLINE_EVENT` or `DECLINE_LINE`**. That is how the *"D/J"* rows of the analysis are represented in a single-valued field without softening the rule. **D1 `BR-TERM-7` and D2 `V-742` state the same rule in the same direction**, and D2's §5.29.2 — which briefly read that a class-D term never raises a checkpoint — was corrected to this on the same day.

**Two expressions, not one — added by the same-day correction of 24 August 2026.** As first issued this section gave each term a single `predicate`, null on class J. **Applied literally that is unbuildable**: with the one field forbidden on class J, nothing decides *which* claims a judgemental term concerns, so every class-J term raises a checkpoint on every claim. D2 §5.28.2 found it and raised it as `D2-173`. **The split is adopted here and in the seed, and the item closes.**

| Field | Decides | Required |
|---|---|---|
| **`selector`** | Whether the term is **in play** for this claim, this event or this line | **Mandatory and non-null on every class — `D`, `J` and `E` alike** |
| **`predicate`** | Whether the term **fires** — the auto-decline | **Class `D` only**, and only where the effect is `DECLINE_EVENT` or `DECLINE_LINE`. **Empty on `J`, on `E`, and on a class-D term carrying `CHECKPOINT`** — nothing is auto-declined, so there is nothing for a predicate to decide |

**For a class-D declining term the two are often the same expression** (#4, #5, #9, #10 below). **Where they differ, the difference is the point**: #6 is in play on every dental line and declines only the ones the mapping did not put under `300` — a term that selected on the whole condition would report nine passes as nine non-selections and tell the assessor nothing.

### 7.2 The ten terms

`clauseRef` is a placeholder in every row, in the visibly-provisional `TBC-nn` form, because the filed wording does not exist. **No decline may be issued citing a `TBC-` reference** — a decline letter cannot quote a clause number that was invented to fill a column.

| # | Term | Class | Scope | Effect | Reason | Clause | `selector` — is the term in play? | `predicate` — does it decline? |
|---|---|---|---|---|---|---|---|---|
| 1 | Cosmetic and aesthetic treatment, and its complications | **J** | `PRODUCT` | `CHECKPOINT` | `EXCL` | `TBC-01` | `diagnosisCode startsWith Z41` — an encounter coded as *a procedure for a purpose other than remedying a health state*. **Deliberately narrow, and it does not reach most cosmetic cases** — §7.3 | **None. Class J may not carry one.** Cosmetic intent is a clinical opinion, not a fact |
| 2 | Health checks, screening and vaccination not connected to a diagnosed condition | **D** | `PRODUCT` | `CHECKPOINT` | `EXCL` | `TBC-02` | `serviceCategory in [SCREENING, VACCINATION]` — deterministic selection, which is what makes the term class **D** | **None** — the effect is a checkpoint, so nothing is auto-declined. *"Not connected to a diagnosed condition"* is the judgement and no fact carries it |
| 3 | Dietary supplements, functional foods, vitamins and minerals not prescribed as treatment | **D** | `PRODUCT` | `CHECKPOINT` | `EXCL` | `TBC-03` | `serviceCategory = SUPPLEMENT` | **None** — the proviso *"not prescribed as treatment for the diagnosed condition"* has no fact behind it, so a person decides — §7.4 |
| 4 | Spectacles, contact lenses, hearing aids and their fitting | **D** | `PRODUCT` | `DECLINE_LINE` | `EXCL` | `TBC-04` | `serviceCategory in [OPTICAL, HEARING]` | **The same expression.** The wording carries no proviso, so selection and decision coincide and this one may auto-decline |
| 5 | Infertility treatment, assisted reproduction, contraception, voluntary termination | **D** | `PRODUCT` | `DECLINE_LINE` | `EXCL` | `TBC-05` | `lineDiagnosisCode startsWith` any of `N46` · `N97` · `Z30` · `Z31` · `O04` | **The same expression.** **`DRAFT` — waits on Q-IDP-02** |
| 6 | Dental treatment other than the basic treatment covered under `300` | **D** | `PRODUCT` | `DECLINE_LINE` | `EXCL` | `TBC-06` | `serviceCategory = DENTAL` — **every dental line is in play**, which is what the assessor's *"tested"* count should say | `serviceCategory = DENTAL` **and** `lossType in [ILL_IP, DEN_IP, MAT_IP]`. **Rewritten at v1.5 — see below; it tested `benefitCode` and could not fire.** Still the one row where the two genuinely differ |
| 7 | Treatment for alcohol or drug dependency, and injury under their influence | **J** | `PRODUCT` | `CHECKPOINT` | `EXCL` | `TBC-07` | `diagnosisCode startsWith F1` — ICD-10 `F10`…`F19`, mental and behavioural disorders due to psychoactive substance use. **The dependency limb only; *"under the influence"* has no fact** — §7.3 | **None. Class J may not carry one.** Dependency and *"under the influence"* are a clinical and a factual judgement respectively |
| 8 | Self-inflicted injury, and injury sustained committing a criminal offence | **E** | `PRODUCT` | `REQUIRE_DOC` | `EXCL` | `TBC-08` | `lossType in [ACC_OP, ACC_IP]` — the events on which the evidence is required | **None. Class E may not carry one.** The named document decides, and a person reads it. **`DRAFT`** — the document type, a police or hospital statement, is not in A4's catalogue |
| 9 | Congenital conditions and their consequences | **D** | `PRODUCT` | `DECLINE_LINE` | `EXCL` | `TBC-09` | `lineDiagnosisCode startsWith Q` | **The same expression.** **`DRAFT` — waits on Q-IDP-02** |
| 10 | Treatment received outside Vietnam | **D** | `PRODUCT` | `DECLINE_EVENT` | `EXCL` | `TBC-10` | `facilityCountry ≠ VN` | **The same expression.** Event-level, so it declines the whole event and outranks any line result. **`DRAFT` — waits on Q-IDP-05** |

Two disciplines are doing the work in that table and both are worth naming.

1. **Every `fact` above comes from a published, closed vocabulary** — `lossType`, `serviceCategory`, `lineDiagnosisCode`, `facilityCountry` and `diagnosisCode` are the **five** used here from v1.5, and an author cannot invent a sixth. **`benefitCode` was the sixth until v1.5 and is no longer used by any term in this register**, for the reason §7.3 gives: no `PRODUCT`-scope term may test it. If the fact does not exist, the term cannot be class D. That is what produced four `DRAFT` rows rather than four optimistic ones, and adding a fact is a specification change, not a configuration change. **`diagnosisCode` is used only in the two `selector` cells of #1 and #7** and is A1 §4.11's event-level fact, held today.
2. **Draft terms are invisible to the engine.** A term whose fact we do not yet hold is not silently evaluated against a null; it is not evaluated at all, and it does not appear in the count the assessor sees.

### 7.3 What is evaluable, when, and what unlocks the rest

| State | Terms | What unlocks them |
|---|---|---|
| **Evaluable before v1.4** | **#6**, and **#3 in part** | Facts already held — `benefitCode` and `serviceCategory` after mapping |
| **Unlocked by the four new service categories** | **#2**, **#4** | `SCREENING`, `VACCINATION`, `OPTICAL`, `HEARING`, added to §8.1 by this version. **Ours to add — no vendor question, no dependency, one seed change.** The cheapest correction available, and it converts two exclusions from unevaluable to class D |
| **Unlocked by line-level ICD** | **#5**, **#9** | `lineDiagnosisCode` — **Q-IDP-02**, already open. Its priority rises, because it now blocks two exclusions as well as what it blocked before |
| **Unlocked by facility country** | **#10** | `facilityCountry` — **Q-IDP-05** |
| **Blocked on a document type** | **#8** | A police or hospital statement, which A4's document catalogue does not carry |
| **Irreducibly judgemental** | **#1**, **#7**, and the proviso half of **#3** | Nothing. A person decides, and the platform's job is to put the clause in front of them with the case. **Each still carries a `selector`** — see the resolution below |

Read the first two rows together: **#6 and the category half of #3 were evaluable before this version; #2 and #4 become evaluable with it.** **Three are irreducibly judgemental**, and that is the honest floor — no quantity of data turns *"cosmetic"* or *"under the influence"* into a fact.

#### #6 — the row relied on as proof that step 4 works, and the two defects it carried

**It is the row this document points at when it says step 4 has something to test, so it is the row a defect in matters most on.** As issued at v1.4 its predicate read `serviceCategory = DENTAL` **and not** `benefitCode in [300, 300.1]`. **Two things were wrong with it and they are independent.**

**Defect 1 — it could not fire.** `benefitCode` is produced by **chain step 5**, benefit mapping. `TERM-HC-006` is **`PRODUCT`** scope, and a `PRODUCT`-scope term is evaluated at **step 4**. So the predicate tested a field that is null on every claim, every time. The failure is silent in the worst way: the expression evaluates, it evaluates against nothing, and the term either never fires or always fires depending on how an engine treats a null. **D1 v0.5 closes the general case as `BR-TERM-9`** — *a term may test only facts available at the step its scope is evaluated at*, enforced when a term is **saved** — and **A1 v1.5 §4.11 carries the *available from step* column that rule is checked against.**

**Defect 2 — where it could have been read, it declined the wrong claim.** §8.1's last line redirects **every** mapped benefit to `500` where the episode's loss type is accident. A dental line in an accident episode therefore carries `benefitCode = 500`, which is not in `[300, 300.1]`, so the predicate was **true** — and the term is class `D` with effect `DECLINE_LINE` and status `ACTIVE`. **Teeth broken in a road accident is the archetypal covered dental claim, and this term auto-declined it, citing a dental exclusion clause.**

**The rewrite, and why it is written this way.**

| | |
|---|---|
| **`selector`** | **Unchanged — `serviceCategory = DENTAL`.** Every dental line stays in play, which is what makes the assessor's *tested* count honest and what the `selector`/`predicate` split exists for |
| **`predicate`** | `serviceCategory = DENTAL` **and** `lossType in [ILL_IP, DEN_IP, MAT_IP]` |
| **Why `lossType`** | It is **captured on the insured event and never derived** (A1 §4.2), so it is available before the chain starts. It carries the treatment setting, which is the dimension §8.1's mapping keys on. The three values named are the live **inpatient** ones, and an inpatient episode is exactly where §8.1 leaves a `DENTAL` line unmapped |
| **Why accident is absent** | **Deliberately.** `ACC_OP` and `ACC_IP` are not in the list, so an accident dental line is never declined by this term. Under §8.1's redirect it is payable under `500`, and that is the correct outcome |
| **Why the scope stays `PRODUCT`** | Moving it to `BENEFIT_SUBTREE` would have made `benefitCode` visible, because subtree terms run at step 7. **It was rejected, and the reason is the term's own logic**: a `BENEFIT_SUBTREE` term scoped at `300` is evaluated only within `300`'s subtree, and **every line this term exists to catch is one the mapping put outside `300`.** The fix would have removed the term's entire reach in exchange for making it evaluable |

**The reach is narrower than the wording, and that was true before v1.5 and is still true.** §8.1 maps every `DENTAL` cost line to `300.1` in an outpatient episode, whether the treatment is basic or not. Separating basic dental treatment from orthodontics, implants and prosthetics needs either a finer service category or a line-level procedure code, and the product has neither today. **The term is correct; it reaches inpatient dental treatment and not the orthodontics its title names**, and that is worth knowing before it is cited.

#### Chain step 7 has no instance on this product, and it is said here rather than left true and unwritten

**All ten terms in the register are `PRODUCT` scope**, so they are all evaluated at step 4. **`scope` carries three values** — `PRODUCT`, `BENEFIT`, `BENEFIT_SUBTREE` (A1 §4.10) — and the second and third are what step 7 evaluates. **Nothing in this product uses either, so the term engine is called at step 7 and evaluates an empty set on every claim.**

**That is a mechanism with no pilot instance, and it is not a reason to remove the step.** The mechanism is right, the scope model is right (**`BR-TERM-8`**), and a benefit-level proviso — *"outpatient physiotherapy only on referral"*, *"maximum 30 days per event"* — is an ordinary feature of a health wording that until v1.4 had nowhere in this product to live. **The two examples just given are illustrations of the shape and are not terms of this product**; neither exists in the register and neither is put there by this document.

**No proviso is invented here.** Inventing a contractual term to give a mechanism something to do produces a rule no wording supports, which is the precise defect the whole term layer was built to remove. **D1 v0.5 asks the business for a real one as `B-23`**, with what it needs stated: the benefit it attaches to and whether it reaches that benefit's descendants, its class, its effect and its clause reference.

**And it was not solved by moving `TERM-HC-006`.** Making that term `BENEFIT_SUBTREE` would have given step 7 exactly one real instance and it was rejected on the term's own merits, above — a term that catches lines *outside* benefit `300` cannot be scoped to benefit `300`'s subtree. **A step that evaluates an empty set is harmless and declared. A term scoped where it cannot see the lines it exists to catch is neither.**

**#1 and #7 — how the two irreducibly judgemental terms are selected, and it is settled rather than left open.** Until the same-day correction of 24 August this section said the two were triggered from outside the term engine altogether, because a class-J term could carry no expression at all and a term with no expression raises a checkpoint on every claim. **The `selector` / `predicate` split removes the reason for that, and the question it leaves is a real one: what selects a term whose whole substance is a judgement?** Two answers were available and one is taken.

| | Option | Taken? |
|---|---|---|
| **A** | **Give each a `selector` from the closed vocabulary of A1 §4.11**, accepting that it will be narrower than the wording | **Yes** |
| **B** | State that the two cannot be selected by the term engine at all, leave `selector` null, and make **D2 `V-734`**'s *"non-null on every class"* rule conditional | No |

**Option A, for one reason that outweighs the others: a null `selector` does not mean "not selected" — it means "in play for every claim".** That is the exact hole the split was introduced to close, and reopening it on two rows would make `V-734` a rule with an exception large enough to drive the whole portfolio through. **`V-734` therefore stands unqualified: `selector` is mandatory and non-null on every class.**

**What each selector reaches, stated plainly, because a narrow selector presented as a complete one is worse than no selector at all.**

- **#1 — `diagnosisCode startsWith Z41`.** It selects encounters coded as *a procedure for a purpose other than remedying a health state*. **Most cosmetic treatment is not coded that way**, so the term engine will see a minority of the cases the clause covers.
- **#7 — `diagnosisCode startsWith F1`.** It selects the dependency limb — `F10`…`F19`, mental and behavioural disorders due to psychoactive substance use. **It does not select *"injury sustained under the influence"* at all**, because no fact in A1 §4.11 records intoxication. Selecting every accident event instead was considered and rejected: it would put every accident claim in the checkpoint queue and off the straight-through path (§6.4) to catch a small subset, which is a product decision nobody has taken and not one this annex may take on its own.
- **The residue of both reaches a person exactly as it did before** — through the **Track 2 clinical assessment** or an assessor invoking the clause. **The register's job for the residue is the clause text and the audit record, not the trigger**, and that sentence is unchanged from v1.4 as first issued.

**And no fact would fix this, which is what distinguishes it from #5, #9 and #10.** *Cosmetic intent* and *under the influence* are judgements, not observations; there is no `cosmeticIntent` fact waiting on a vendor answer. Contrast §7.4's `prescriptionLinked`, which **is** a fact and would restore an auto-decline if the vocabulary were extended. **These two are the floor, and the selector is a filter on who gets asked, not a step towards deciding it.**

### 7.4 The `SUPPLEMENT` and `ADMIN` defect, and what replaces it

**The platform already declined supplements before any of this was written — in the wrong place, with the wrong code, citing nothing, and unconditionally.** §8.1's mapping table carried `SUPPLEMENT → DECLINE:NOTCOV`, and `ADMIN` did too. Four things were wrong with that, and the fourth is the one that arrives as a complaint.

| | What the mapping table did | Why it is wrong |
|---|---|---|
| 1 | Declined at **step 5**, benefit mapping | The contractual ground is an exclusion, and exclusions are step 4. A mapping table decides *which benefit*, not *whether cover exists* |
| 2 | Cited **`NOTCOV`** — *"item is not a covered medical item"* | The ground is an exclusion, so the correct code is **`EXCL`** — *"thuộc điểm loại trừ của quy tắc BH"*. The two are different things to say to a customer and they carry different appeal routes |
| 3 | Cited **no clause at all** | The decline letter cannot quote the term it relies on, and the assessor cannot see which clause was applied |
| 4 | Declined **unconditionally** | The wording says *"not prescribed as treatment for the diagnosed condition"*. **A supplement that was prescribed for the condition is covered by the wording and was declined by the system.** |

**What replaces it.**

- **`SUPPLEMENT` maps to `200.2` outpatient and `100` inpatient** — where a prescribed supplement would in fact be paid — and `TERM-HC-003` decides at step 4 whether the exclusion bites. Mapping it to a real benefit rather than to a decline is the whole change: the table's job is now only to say where a covered line lands.
- **`ADMIN` maps to nothing in either setting.** An administrative or non-medical fee is not excluded by a clause — it is simply not a covered medical item, which is a **cover test, not an exclusion**. It therefore has no benefit, and §8.2's non-medical-item rule declines it `NOTCOV` at step 6. The outcome is the same as before; it is now reached at the right step, by the right rule, for the reason actually stated. `ADMIN` deliberately does **not** get a term: putting it in the register would assert a contractual exclusion that the wording does not contain.
- **§8.1 no longer declines anything**, and that is the general principle underneath both rows: *a mapping table that declines is a mapping table making cover decisions.* A category with no benefit falls to the line-level cover test at step 6; a category the contract excludes falls to the term register at step 4.

**The cost, stated rather than absorbed.** `TERM-HC-003` carries `CHECKPOINT`, not `DECLINE_LINE`, because the proviso needs a fact — a link from the supplement line to a prescription for the diagnosed condition — that the closed vocabulary does not contain. Until it does, **every supplement line raises a checkpoint instead of being auto-declined**, which moves work to assessors and, under §6.4's third condition, takes those claims off the straight-through path entirely. That is the direction the wording requires and it is the safe direction, but it is a real operational cost and it should be a decision rather than a surprise. **Adding a `prescriptionLinked` fact to the vocabulary would restore the auto-decline for the unprescribed case** — and it is a specification change, not a configuration change, which is exactly the discipline the closed vocabulary exists to impose.

### 7.5 What the assessor sees, and what an auditor is owed

Every term in scope is evaluated and **every result is recorded, whether it fired or not** — *"6 terms tested · 1 declined · 3 checkpoints raised · 2 passed"*, not the first hit alone.

> **Six, not ten, and the illustration said ten until v1.5.** **A `DRAFT` term is invisible to the engine** (§7.2, A1 §4.10 `termStatus`): it is not evaluated, so it produces no outcome and it is not counted. **Four of the ten are `DRAFT`** — #5 and #9 on `Q-IDP-02`, #10 on `Q-IDP-05`, #8 on a document type A4 does not carry — so **the live count on this product is six**, and a screen that said *"10 terms tested"* would be telling an assessor that four terms had been considered when the engine had never looked at them. **The counting rule, stated so that two documents cannot count differently:**
>
> **The *terms tested* figure is the number of `ACTIVE` terms, inside their effective window, bound to the claim's `ruleSetRef`, whose `selector` was evaluated. It excludes every `DRAFT` and every `WITHDRAWN` term, and it includes terms whose selector did not match** — those are recorded as `KHÔNG_THUỘC_PHẠM_VI` (A1 §4.10 `termOutcome`), because *considered and not applicable* is a different answer from silence and an auditor is owed the difference.
>
> The six live terms are **#1, #2, #3, #4, #6 and #7**. On a given claim most will report `KHÔNG_THUỘC_PHẠM_VI`; the illustration above is a worst case, not a typical one. An assessor who sees only the first hit cannot tell whether the case was close, and an auditor asking *"was clause `TBC-06` considered?"* must get an answer that is not *"we stopped before we got there."* Where more than one term declines the same line, **all the clause references are carried into the decline message**, not only the first. `DECLINE_EVENT` outranks `DECLINE_LINE`; a `CHECKPOINT` never overrides a decline, and a checkpoint left open on a declined line is closed as moot with that reason recorded.

---

## 8. Cost line to benefit mapping

### 8.1 Service category mapping

The extraction layer classifies each cost line into a service category; this table maps the category to a benefit, conditional on the treatment setting.

| Category code | Service category | Outpatient episode | Inpatient episode |
|---|---|---|---|
| `CONSULT` | Consultation / examination fee | 200.1 | 100 |
| `LAB` | Laboratory test | 200.1 | 100 |
| `IMAGING` | Diagnostic imaging | 200.1 | 100 |
| `PROCEDURE` | Surgery and procedure | 200.1 | 100.2 |
| `DRUG` | Medication | 200.2 | 100 |
| `SUPPLEMENT` | Supplement / functional food | 200.2 | 100 |
| `PHYSIO` | Physiotherapy | 200.3 | 100 |
| `DENTAL` | Dental procedure | 300.1 | n/a |
| `ROOM` | Room and board | n/a | 100.1 |
| `ICU` | Intensive care | n/a | 100.1.1 |
| `AMBULANCE` | Ambulance / emergency transport | 100.4 | 100.4 |
| `DELIVERY_NORMAL` | Delivery — normal | n/a | 400.1 |
| `DELIVERY_CS` | Delivery — caesarean | n/a | 400.2 |
| `MATERIAL` | Medical materials and consumables | 200.1 | 100 |
| `ADMIN` | Administrative and non-medical fee | n/a | n/a |
| `SCREENING` | Health check / screening | 200.1 | 100 |
| `VACCINATION` | Vaccination / immunisation | 200.2 | 100 |
| `OPTICAL` | Spectacles, contact lenses and optical appliances | 200.1 | 100 |
| `HEARING` | Hearing aids and audiology appliances | 200.1 | 100 |
| `OTHER` | Anything else | assessor decides | assessor decides |

*Twenty categories from v1.4. The category is produced by IDP; this table is the mapping the benefit engine applies to it, and it is the only direction the arrow runs — `serviceCategory` is an **extraction** output (A1 v1.5 §4.11) and `benefitCode` is what this table produces from it.*

**Two sentinels in the seed, documented at v1.5 because an undocumented sentinel is how a loader guesses.** The `outpatient` and `inpatient` columns above read `n/a` and *assessor decides*; in `A5_seed_v1.5.json` those are the strings **`"NONE"`** and **`"ASSESSOR"`**.

| Sentinel | Means | Why it is a string |
|---|---|---|
| **`"NONE"`** | **This category has no benefit in this treatment setting.** The line is not declined by this table — the table declines nothing — it falls to the line-level cover test at **step 6** | It held `null` until v1.5, which collided head-on with **D3 §1.2**: `null` means ***not known*** across every payload and every fixture in this project. *No benefit exists here* is a **known** fact and must not be written in the vocabulary of an unknown one |
| **`"ASSESSOR"`** | **No rule maps this category; a person decides which benefit the line belongs to.** `OTHER` alone | A sentinel of the same kind as `"INHERIT"` on the wire (D3 §I-05), and a string for the same reason: **code that treats it as a benefit code fails loudly instead of looking up a benefit called *ASSESSOR*.** Neither value is a benefit code and **neither may be sent on any interface** |

**Three changes at v1.4, and none of them is cosmetic.**

1. **Four categories added** — `SCREENING`, `VACCINATION`, `OPTICAL`, `HEARING`. These are **Claim Platform master data, ours to define**: no vendor question, no dependency, one seed change. They exist because exclusions #2 and #4 could not be evaluated without them (§7.3), and they are mapped to the benefit a *covered* instance of each would land under — a screening ordered for a diagnosed condition is a consultation or a test, a therapeutic vaccination is a prescribed medication. Whether the exclusion bites is decided at step 4 by the term register, not here.

   > **`OPTICAL` and `HEARING` carry a mapping although term #4 declines them unconditionally, and the justification above covered only `SCREENING` and `VACCINATION`. Completed at v1.5.** Term #4 is class `D`, effect `DECLINE_LINE`, and its predicate is `serviceCategory in [OPTICAL, HEARING]` with no proviso — so on today's wording **every** optical and hearing line is declined at step 4 and never reaches step 5. Mapping them to `200.1` and `100` looks, on that reading, like mapping something that can never arrive. **It is deliberate, for three reasons.** **(a)** The mapping table's job is to say *where a covered line lands*, and whether a line is covered is step 4's question, not step 5's — that separation is the whole of §7.4 and a category left unmapped because a term happens to decline it puts the cover decision back into the mapping table by the back door. **(b)** A category with no mapping does not decline nothing — it falls to step 6 and is declined **`NOTCOV`**, *"not a covered medical item"*, which is the **wrong code, with no clause**, for a line the contract excludes by a term. If term #4 is ever amended, withdrawn or given a proviso — a hearing aid prescribed as treatment, say — the mapping must already be there or the line lands on the same defect §7.4 removed. **(c)** These are illustrative exclusions and `A5-Q3` is open; a business that narrows #4 changes one term and no mapping row.
2. **`SUPPLEMENT` and `ADMIN` lose their `DECLINE:NOTCOV` mapping.** §7.4 sets out why at length. `SUPPLEMENT` now maps where a prescribed supplement would be paid; `ADMIN` maps to nothing and is declined at step 6 as a non-medical item.
3. **This table no longer declines anything.** Every value in the two right-hand columns is now a benefit code, `n/a`, or *assessor decides*.

> **A4 §4.5 must follow.** Its derived-field table carries the original sixteen, so until it is reissued the two lists disagree by four rows. **This table is the one the benefit engine reads**, and the four new categories are inert until the extraction layer can produce them — a category the engine knows and IDP never emits declines nothing and breaks nothing, which is why the order of the two changes does not matter. It is recorded here so nobody has to discover the discrepancy.

#### The accident redirect, and what it discards

**Where the episode's loss type is accident, every mapped benefit is redirected to `500`** — accident medical expenses have their own annual cap and no waiting period. **That sentence is the whole of the rule and, until v1.5, the whole of what this document said about it. It is not the whole of what it does.**

**`500` is a childless leaf.** Look at §4.1: it has no children, and in `benefit_schedule_line` it carries an `annual_cap` and nothing else — **no `per_day_cap`, no `per_event_cap`, `count_limit: 0`, `count_unit: null`, `copay_pct: null`, `deduction_pct: null`.** So the redirect does not merely change which annual pot a line draws on. **It discards every sub-limit, every count limit and the co-payment on the way**, because the chain applies those from the benefit the line is mapped to and after the redirect that benefit is `500`.

| A line that would have been… | …under an accident episode becomes | What is lost |
|---|---|---|
| `100.1` room and board — 1,500,000 per day, **60-day count** | `500` | **The per-day room cap and the 60-day count.** Billed room cost is payable to the annual cap |
| `100.1.1` intensive care — 3,000,000 per day, **15-day count** | `500` | **The ICU per-day cap and the 15-day count**, and with them the one benefit §0.1 mechanism 1 exists to exercise |
| `100.2` surgery — 20,000,000 per procedure | `500` | The per-event cap |
| `200.1` consultation — 1,500,000 per visit, **20% co-payment** inherited from `200` | `500` | **The per-visit cap *and* the co-payment.** An accident outpatient visit is paid in full to the annual cap where the same visit for an illness bears 20% |
| `300.1` dental — 1,000,000 per visit, **30% co-payment** | `500` | The per-visit cap and the co-payment |

**Stated as a number: on the Standard plan, a 30-day accident admission with 10 days in ICU pays the billed cost up to 50,000,000, with no room cap, no ICU cap and no day count applied.** The same admission for an illness is capped per day, counted, and rolled up under `100`'s 100,000,000. **Which of those the product intends is a business decision and it is opened as `A5-Q8` (§12). Stating the consequence is this document's job and it is done here.**

> **This is the one case the product model cannot express, and it is worth naming as a modelling limit rather than as a product question.** What an accident wording usually means is *"the same benefit structure, under a different annual pot"* — room capped as room, ICU counted as ICU, and the whole lot drawing on the accident sum insured instead of the inpatient one. **The benefit tree has nowhere to put that.** A benefit carries exactly one shape — one set of caps, one count, one co-payment, one parent — so *"benefit `100.1`, but rolling up to `500`"* cannot be written: it would need either a second `100.1` under a second parent, or a per-line override of the ancestor path, and the tree admits neither. **Every alternative is a change to the model, not to a figure**, which is why `A5-Q8` is asked before the figures are substituted rather than after: replacing the amounts is free (§0.1) and this is not an amount.

### 8.2 Line-level decline rules

These fire at step 6 and are what makes the AI rationale possible.

| Rule | Condition | Decline code | Drafted rationale (pattern) |
|---|---|---|---|
| Non-medical item | Category = administrative / non-medical | `NOTCOV` | *"{item}: {amount} — administrative fee, not a covered medical item"* |
| ~~Supplement~~ | ~~Item matches the supplement / functional-food list~~ | ~~`NOTCOV`~~ | **Withdrawn at v1.4.** A supplement is excluded by a clause, conditionally, not un-covered by category. `TERM-HC-003` decides it at step 4 and cites `EXCL` with the clause — §7.4. Leaving this row would have reproduced the same defect one step later |
| Unrelated test | Test's ICD group is unrelated to the episode diagnosis | `NOTREL` | *"{item}: {amount} — diagnostic test not related to the diagnosis {icd}"* |
| Unrelated medication | Drug's therapeutic class is unrelated to the episode diagnosis | `NOTREL` | *"{item}: {amount} — treats {class}, not related to the diagnosis {icd}"* |
| Excluded category | Item falls under a term in the register — §7.2 | `EXCL` | *"{item}: {amount} — falls under {clauseRef}: {term text}"*. **The clause reference is now part of the pattern**, because a decline that names an exclusion must be able to quote it |

**Every drafted rationale is a proposal.** The assessor may accept, edit or reverse it, and the original AI text is retained beside the final text.

---

## 9. InsureMO configuration test script

Hand this section to InsureMO with the workbook. **A pass requires all eight demonstrated live, not described.**

> **Result, 21 August 2026: all eight passed, demonstrated rather than described.** The script below is kept exactly as it was written, because a test script rewritten after it passes is no longer evidence of anything. C-IMO-01, C-IMO-01b and C-IMO-01c are closed on it, and the consequence is Framework §2.1 (written at v2.0, carried unchanged to **v2.5**): **the Policy Data Store is not built.**

| # | Test | Pass criterion |
|---|---|---|
| T1 | Configure the tree | `100 → 100.1 → 100.1.1` exists as a three-level parent/child structure |
| T2 | Child-to-parent aggregation | A payment configured under `100.1.1` is shown to consume `100.1` and `100` |
| T3 | Two limit dimensions | `100.1` carries both 1,500,000 per day **and** a 60-day count |
| T4 | Unlimited representation | `100.2` carries a per-event money limit and **no** count limit, expressed unambiguously |
| T5 | Inherited co-payment | `200.1` inherits 20% from `200` without the value being repeated on the child |
| T6 | Waiting period by condition class | Benefit group `200` carries 365 / 365 / 30 days for `PRE` / `SPE` / `GEN` |
| T7 | Three plan tiers | The same tree with three sets of amounts, without duplicating the structure |
| T8 | Several levels on one policy | A policy carrying `HC-STD` and `HC-PLS`, with members assigned to each |

**~~If T1 or T2 fails, the benefit schedule is mastered in the Policy Data Store~~** — **withdrawn 21/08/2026, and it is worth being blunt about why.** That sentence promised a fallback: if InsureMO could not carry the tree, the schedule moved into a component we controlled and no claim-side code changed. The tests passed, so the component is not built — and the promise went with it. **There is now no second place for the benefit schedule to live.** If a future product exercises something this one did not, or the sandbox turns out not to behave as the demonstration did, the remedy is a build rather than a swap. That exposure is carried once, as **RK-14**, and its only real mitigation is hands-on evaluation of the sandbox against this document, benefit by benefit — **C-IMO-06**, which is on the critical path for exactly this reason.

---

## 10. Where the product model lives

**This document describes the product. It does not describe the component that holds it** — and until v1.3 those two things were run together, because there was only ever one candidate holder. There are now two, so this section says which, for every element above.

Nothing in §1–§5 or §11 changed at v1.3. The tree is the same tree and the limits are the same limits. **At v1.4 the rule chain became fourteen steps and one row of the table below changed owner** — §7's exclusions are no longer InsureMO's to master.

> **Every shape in the *where it lands* column was checked against `A5_seed_v1.5.json` at v1.5, and five of them were wrong.** This table is what a mock builder writes a loader from, so a wrong column list here is not a documentation defect — **a loader built from the v1.4 table failed on all five collections it named.** What was wrong: `service_category_benefit_map` was given a `setting` column and a single `benefit_code` where the seed has two benefit columns; `adjudication_rule` was given a `benefit_code` it has never had; `premium_rate` was described as a long table where the seed is a wide one; `product` was given a `status` and effective dates the seed object does not carry; and **`line_decline_rule` was named as a seeded collection and has never existed in any version of the seed.** All five are corrected below, and the correction is in this direction — **the seed is the artefact, and this table describes it** — except for `adjudication_rule`, where the seed was right and §6.1's claim about it was wrong.

| Element in this document | Held by | Where it lands |
|---|---|---|
| §1 product identity | **InsureMO** (F01) | Its product model; mocked as `product` — **a single object, not an array** — `code`, `name`, `line`, `currency`, `term_months`. **Corrected at v1.5:** this row also listed `status` and `effective_from/to`, and the seed object carries neither. **They are not added.** A product's status and effective dating are InsureMO's product-model concern (F01) and nothing in the claim chain reads them; inventing two fields so that a table matched would have put a lifecycle into the fixture that no rule, no screen and no interface consumes |
| §1 plan tiers | **InsureMO** (F01) | `plans` — `product_code`, `plan_code`, `name` |
| §4.1 tree structure | **InsureMO** (F07) | `benefit_schedule_line` — `benefit_code`, `parent_benefit_code`, `level`, `name`. **The element whose home changed**, and the one C-IMO-01 was about |
| §4.2 limits | **InsureMO** (F07) | `benefit_schedule_line` — `plan_code`, `annual_cap`, `per_event_cap`, `per_day_cap`, `count_limit`, `count_unit`, `copay_pct` |
| §4.3 deduction ratio and its condition | **InsureMO** carries the rate and the condition (F07); **the Claim Platform evaluates the condition** (F48) against the provider master (F57), which is ours | Framework §7.6. The rate is a contract term; the fact that triggers it is a claim-time observation. **Whether the rate on an ancestor reaches a descendant's line is open — `B-19`, §4.3** |
| §4.3 the provider network itself | **Claim Platform** master data (F57), maintained by an administrator (A3 §3.1) | `provider` — `provider_id`, `name`, `province`, `country`, `in_network`. **New at seed v1.5**, six rows, one of them off-network and one of them outside Vietnam. **Not mock data**: it is loaded into a real table and survives the day the InsureMO mock is switched off, exactly as `payout_account` and `beneficiary` do |
| §5 waiting periods | **InsureMO** (F07) | `benefit_waiting_period` — `plan_code`, `benefit_code`, `condition_class`, `days` |
| §5 the continuous-renewal waiver | **InsureMO** (F05) | `member.continuous_cover_from` — **new at seed v1.5.** The seed half of `continuousCoverFrom` on D3 §I-05, held at **member-on-policy** level. `null` means **first-time member** and never *could not tell*. Until v1.5 the field was absent, so §5's *"a renewal without a break does not re-serve the waiting period"* rule had no fixture and could not be exercised in any mock |
| §7 which rule set governs a policy | **InsureMO** (F05) | `policy.rule_set_ref` — **new at seed v1.5.** The key the claim-side term register binds to (D3 §I-05). **Its absence was not cosmetic:** all ten `policy_term` rows bind to `HC-TC-2026-01` and no policy record carried the value, so **chain step 4 selected zero terms in every mock** — a step that reports *"0 terms tested"* and passes every claim |
| §6.1 rule chain | **Claim Platform** configuration — unchanged, and it never was the store's | `adjudication_rule` — **`step`, `name`, `enabled`**, fourteen rows from v1.4. **Corrected at v1.5: there is no `benefit_code` column and there never has been.** It is a *participation switch list against a sequence fixed in code*, not an authoring model, and the seed says so in `meta`. **P-05 as revised, and `S-39` with it, say a step is enabled or disabled *per product*** — so the missing column is not a gap in the seed, it is the correct shape, and §6.1's *"per benefit"* was the error |
| §6.3 decline reasons | **Claim Platform** reference data | `decline_reason` (`code`, `text_vi`, `text_en`, `level`). A1 §5.2 is normative for the codes |
| §7 exclusions | **Claim Platform** master data (F65) — **the one row whose owner changed at v1.4** | `policy_term` — the term register, keyed on `ruleSetRef`. Exclusions are terms of the contract and R-IMO-02 names them, so InsureMO still owns *which* rule set governs a policy and returns the reference on I-05. But a term the claim engine must **evaluate** is a claim-side object with a predicate, a class and an effect, and that is ours (decision **D-1**, option B — the same division already agreed for the wording store). `product_exclusion` is withdrawn |
| §8.1 mapping | **Claim Platform** reference data | `service_category_benefit_map` — **`category`, `name`, `outpatient`, `inpatient`**: one row per category, with **a benefit column per treatment setting**, not a `setting` discriminator and a single `benefit_code`. **Corrected at v1.5.** Twenty rows from v1.4, **and none of them declines**; the two sentinels `"NONE"` and `"ASSESSOR"` are documented at §8.1 |
| §8.2 line rules | **Claim Platform** configuration | **Not a seeded collection, and `line_decline_rule` is withdrawn.** The v1.4 table named it as one; **no version of the seed has ever contained it**, and a mock builder splitting a loader on this table looked for a collection that does not exist. The four surviving rules of §8.2 are **rule bodies in code** — the same status as the fourteen chain steps (`meta.adjudication_rule_note`) — with the one contractual ground among them, the withdrawn supplement rule, now living in `policy_term` where it can carry a clause. **If §8.2 is ever made configurable it needs a collection, a shape and an owner**; it has none of the three today and this row says so rather than naming one |
| §11 premium | **InsureMO** (F03) | `premium_rate` — **a wide table, seven rows, one per age band**: `band`, `age_from`, `age_to`, and **one money column per plan code** (`HC-STD`, `HC-PLS`, `HC-PRM`). **Corrected at v1.5:** the v1.4 shape — `plan_code`, `age_from`, `age_to`, `annual_premium` — is the long form, which would be twenty-one rows, and the seed has never held it that way. The wide form is kept because it is the shape §11's table above is read in and because a plan is a column of this product rather than a dimension of it; **a loader normalising it to the long form is doing a legitimate transformation and should say so** |

**One benefit-tree row per plan per benefit.** Seventeen benefits × three plans = **51 rows** in `benefit_schedule_line`, plus **six condition classes** (`PRE`, `SPE`, `GEN`, `BIR`, `MAT`, `ACC` — §5) × 5 benefit groups × 3 plans in `benefit_waiting_period`, **excluding the `n/a` combinations**, which are not seeded as zero and not seeded at all. Those counts are properties of the product and did not change.

> **What is no longer here, and where it went.** This section used to be the seed-data specification for a store with eight tables. Two of those tables — the **payout account** and the **beneficiary/payee** — were never product data at all: they are claim-side master data, they are now **F58**, and they are ours. They do not appear above because nothing in §1–§11 describes them. `A5_seed_v1.5.json` carries them, marked as Claim Platform records rather than mock data, and it also records the consequence of the split: **nothing now prevents a payout account outliving the member it names**, because the two sit in different systems and no foreign key crosses the gap.

> **The seed file is the machine-readable half of this section.** `A5_seed_v1.5.json` holds **215 records against v1.4's 209**, with an ownership marker per collection so a mock builder can load the InsureMO-owned collections into the InsureMO mock and the Claim-Platform-owned ones into ours, rather than into a single schema that no longer exists.
>
> **Where the six came from at v1.5, and what else moved without changing a count.** All six are the new **`provider`** collection. No other collection gains or loses a record. **Four collections gain a field or change a value**: `policy` gains `rule_set_ref`, `member` gains `continuous_cover_from`, `service_category_benefit_map` replaces `null` with `"NONE"`, and one `policy_term` row — `TERM-HC-006` — has a rewritten `predicate`. **Two of those four are InsureMO-owned, which is the first time since v1.2 that an InsureMO-owned collection has changed**, and it is recorded rather than absorbed: both fields are ones **D3 §I-05 is already contracted to return**, and without them the mock cannot serve the contract. **No figure, no limit, no waiting period and no premium moved.**
>
> **Where the five came from, and where nothing came from.** Four are the new service categories (16 → 20) and one is the new chain step (13 → 14). `product_exclusion`'s ten rows are replaced one-for-one by `policy_term`'s ten, so the ten exclusions neither gained nor lost a member — they gained a shape. **Not one InsureMO-owned record moved**: the product, the three plans, the two policies, the three benefit levels, the twelve members, the 51 benefit schedule lines, the 51 waiting periods and the seven premium rates are byte-for-byte what they were at v1.2.

> **On `copay_pct` and the wire format.** The seed column is a whole-number percentage (`20`). Every interface payload carries co-payment as **integer basis points** (`2000`), per D3 §1.2. The conversion is `basis points = pct × 100` and it belongs in the seeding script, not in the benefit engine — a percentage and a basis-point value that both look like plausible integers are indistinguishable at runtime, so the boundary must be exactly one place.

---

## 11. Premium

Annual premium per insured person, VND, illustrative. Age at inception, last birthday.

| Age band | Standard | Plus | Premium |
|---|---|---|---|
| 0 – 5 | 4,800,000 | 7,900,000 | 13,500,000 |
| 6 – 17 | 3,600,000 | 5,900,000 | 10,200,000 |
| 18 – 30 | 4,200,000 | 6,800,000 | 11,800,000 |
| 31 – 40 | 5,100,000 | 8,300,000 | 14,300,000 |
| 41 – 50 | 6,900,000 | 11,200,000 | 19,400,000 |
| 51 – 60 | 9,800,000 | 15,900,000 | 27,500,000 |
| 61 – 65 | 14,200,000 | 23,100,000 | 39,800,000 |

**Rules:** premium is the sum of the per-member premiums; no family discount, no loading, no short-period pro-rata in MVP 1 (all members take the policy period). Maternity is included in the premium above and is simply unavailable to ineligible members — no separate rate.

---

## 12. What the business must confirm

**These are A5's own questions, badged `A5-Q…` so they cannot be mistaken for the business decisions themselves** — those live in **D1 Part XI (`B-01…B-17`)** and are mirrored in Framework §9 as `C-BUS-…`. Where a decision there answers a question here, the row is struck with what was decided.

| # | Item | Why it matters |
|---|---|---|
| ~~A5-Q1~~ | ~~The **structure** of §4 — three levels, `included` children, inherited co-payment~~ — **closed 17/08/2026 (D1 B-01, C-BUS-01): the structure is confirmed; the figures are to be replaced from a real product** | Everything downstream is built on it. **§0.1 is the checklist any substitute must still pass** |
| A5-Q2 | The waiting-period matrix in §5, and the specified-conditions list | Drives the `condition_class` field and six decline paths |
| A5-Q3 | The exclusions in §7 | Each one is a rule to implement and test. **Still open, and sharper at v1.4**: §7.2 now says of each item what class it is, what it can be tested against and what it costs to evaluate, so the business is being asked to confirm ten *terms* rather than ten sentences. Three of them can never be automated whatever the business says, and the wording of #3 is the one that changes a customer outcome |
| A5-Q4 | The service-category mapping in §8.1 | It is the contract between the extraction layer and the benefit engine |
| A5-Q5 | The line-decline rules in §8.2, especially the supplement and unrelated-medication tests | This is the most visible AI behaviour in the product |
| **A5-Q8** | **What an accident episode should actually pay. New at v1.5, and it is the one open question here that changes money on a claim the pilot will certainly see.** §8.1's redirect sends every mapped benefit to `500` where the loss type is accident. `500` is a childless leaf carrying an annual cap and nothing else, so **the redirect discards the room cap, the 60-day count, the ICU cap and the 15-day count on an inpatient accident, and the per-visit cap and the co-payment on an outpatient one** — §8.1 sets out the full table. **On the Standard plan a 30-day accident admission with 10 days in ICU pays billed cost to 50,000,000 with no sub-limit and no count applied.** Three candidates, and the business chooses: **(a)** confirm it as intended — accident cover is a simple indemnity to a separate pot, which is a real and common wording; **(b)** keep the accident annual cap and **apply the ordinary sub-limits, counts and co-payment of the benefit the line would otherwise have mapped to**, which is what most accident wordings mean; **(c)** give `500` children of its own, mirroring the structures that matter. **This annex takes none of them** | **(b) is the reading most wordings carry and the product model cannot express it.** A benefit carries exactly one shape — one set of caps, one count, one co-payment, one parent — so *"benefit `100.1`, but rolling up to `500`"* has nowhere to live: it needs either a duplicated subtree under a second parent or a per-line override of the ancestor path, and the tree admits neither. **So this is a change to the model, not a substitution of figures**, and §0.1's *"replacing the amounts is free"* does not cover it. Ask it before the figures are replaced, not after |
| **A5-Q9** | **Whether one insured event may carry two condition classes. New at v1.5.** A caesarean performed because of a pregnancy complication is `BIR` (270 days) and `MAT` (90 days) in **one** event; an admission treating a `PRE` condition and an unrelated `GEN` one is the same shape without maternity. The model holds **one** class per event, so one of the two benefits is served the wrong waiting period. **Two candidates: (a)** accept the simplification, **write down the precedence rule** that decides which class is stored, and let an assessor split the episode into two insured events where it matters; **(b)** move `conditionClass` to **cost-line** level, so each line is served its own waiting period | **(b) is the correct model and it is not a figure change either.** `conditionClass` is an event-level fact in A1 §4.11, D2 captures it once per event, and A5 §5's matrix, chain steps 2 and 3 and every waiting-period decline move with it. **The interim is (a) with no precedence rule stated**, and that is the part that must not survive the decision: a simplification nobody wrote down is indistinguishable from a defect |
| ~~A5-Q6~~ | ~~The auto-approval threshold in §6.4~~ — **closed 17/08/2026 (D1 B-03, C-BUS-03): 2,000,000 VND confirmed as the live threshold.** It is a real gate, not a placeholder | The value can be refined once the ground-truth benchmark exists; the gate is live either way |
| ~~A5-Q7~~ | ~~Whether the amounts in §4.2 and §11 should be replaced with a real schedule~~ — **closed 17/08/2026 (D1 B-01): they are to be replaced from a real product** | The figures are illustrative and were always meant to go. What must survive substitution is the structure — §0.1 |

**Six remain open: A5-Q2 … A5-Q5, and A5-Q8 and A5-Q9, both new at v1.5.** The first four are product content the business owns, and none of them blocks the engine's construction, because each is data the engine reads rather than behaviour it hard-codes. **The two new ones are a different kind and the difference is worth stating: neither is answered by data the engine reads.** `A5-Q8`'s most likely answer and `A5-Q9`'s correct answer both need a change to the **product model** — a benefit that can carry two shapes, and a condition class that sits on a line rather than on an event — so they are asked now, while the structure is still being confirmed, rather than after the figures are substituted under §0.1.

> **Three more open items live elsewhere and are named here so this section is the whole picture.** **`B-19`** (D1 v0.5) — whether the deduction ratio at step 11 tests the line's own benefit or an ancestor that carries the rate, which changes money and is set out at §4.3 and §6.2. **`B-23`** (D1 v0.5) — a real benefit-level proviso, so that chain step 7 has something to evaluate; §7.3 records that it has none. **`X-20`** (D3 v2.3) — that `ruleSetRef` is a `lookup()` with nothing to look up: no interface lists the rule sets available to a term author. **None of the three is A5's to answer**, and all three reach the product.

> These figures are illustrative and were constructed for this project. They are not an offer, not a filed product, and not priced. Any use with a real insurer requires actuarial pricing and regulatory filing — this document is a specification input, not a product approval.

*End of Product Data Sheet v1.5.*
