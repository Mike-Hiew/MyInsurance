# Spec — Data model & enums

> English translation for reviewers. Vietnamese original: [spec.md](spec.md).

> **Full text:** "Hackaton" Project → `claude/build-01-data-model-va-enum` (~177 KB).
> This file holds the **immutable constraints** — what every change must re-check before touching anything.

## Baseline scale

- **32 entities**: 10 policy · 15 claim · 7 IDP
- **31 enums**
- **30 constraints that must be added by hand** (ODC does not generate them)
- 25 traps recorded in the source doc

## Four enums that decide the demo — must not drift

| Enum | Number of values | Notes |
|---|---|---|
| `claimState` | **13** | → 11 states shown to the customer. Mapping table in `screens-wording` |
| `lossType` | **12** | 8 values live in MVP 1 · 4 counter-only |
| Decline codes | **7, closed list** | `LIMIT` `PERIOD` `NOBEN` `WAIT` `EXCL` `NOTREL` `NOTCOV` |
| `LineCategory` / `BenefitCode` | — | CONSULT/LAB/IMAGING/DRUG/SUPPLEMENT/DENTAL · 200.1 / 200.2 / 300 |

## Architecture invariants

1. **No agent — Mentor included — may create an Entity.** Entities are generated from the spec, via `outsystems-spec-driven-build`.
2. **Money flows only through `RunAdjudicationChain`.** No agent tool can write a money field, the limit ledger, a payout order or the claim status.
3. **Raw extraction results are immutable.** No one, not even an admin, edits `RawExtractionJson`. An in-place correction is written to the corrected value and **the original value is kept**.
4. **The limit ledger posts by the period containing each line's treatment date.** A claim spanning two policy years touches two counters and **is never summed back together**.

## ⚠ Actual status — 20/09/2026

The Entities currently on ClaimMVP were **inferred from `Prototype_Insurance_BnK_v0.5.html`** so the screens could run, and have **not been reconciled with this spec**.
→ See `changes/active/verify-entity-vs-build01/`.
