# Spec — Business rules

> English translation for reviewers. Vietnamese original: [spec.md](spec.md).

> **Full text:** Project → `claude/build-03-nghiep-vu-va-rule`.

## Scale

8 document types (3 tiers) · **88 fields** · cross-document rules **R1–R9** · 6 `packageState` · term engine · ~45 `BR-*` rules.

## The eight document types

| Tier | Code | Behaviour |
|---|---|---|
| 1 | `T1-01` itemised bill · `T1-02` prescription · `T1-03` examination slip · `T1-04` VAT invoice | Full extraction. **`T1-04` accepts XML only**, no OCR path |
| 2 | `T2-01` identity papers · `T2-02` lab/imaging results · `T2-03` medical report/surgery certificate · `T2-04` | **Classification only** |
| 3 | — | Not processed |

**The `T1-01` line-item has 17 columns, not 14.** `L15` date per line · `L16` ICD per line · `L18` number of treatment days (`L17` deliberately left blank). All three carry `Prov = X?`, **not yet confirmed** by IDP.

## Six `packageState`

`COMPLETE` · `MISSING_DOCUMENT` · `MISSING_FIELD` · `INCONSISTENT` · `AWAITING_ORIGINAL` · `ERROR`.
The engine **consumes** this state, it does not re-derive it.

## Three levels of necessity, not two

*Required* blocks claim resolution · *Recommended* generates a supplement request, the claim officer may waive it · *Optional* is never asked for and never blocks.

## Term engine — a dead branch

**All 10 pilot terms carry `clauseRef` = `TBC-nn`**, and `BR-TERM-10` forbids declining by citing a provisional clause.
⇒ **The `EXCL` auto-decline branch cannot run.** The demo uses `NOTCOV` / `WAIT`.
If a judge asks: answer that this is a checkpoint for a human — it is a strong answer, not a weakness.

## Image-quality checks do NOT block

Blurry / missing edges / glare all give **two buttons: `"Chụp lại"` (EN: "Retake") and `"Dùng ảnh này"` (EN: "Use this photo")**. *Use this photo* is always available.
Reason: a bad photo has a recovery path, whereas a rejected shot produces nothing at all.
