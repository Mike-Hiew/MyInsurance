# Mentor prompts — A5Step06…09 (soạn trước, gửi từng lượt sau lượt E)

> Chữ ký chung như bước 1–5. Không bước nào ghi Entity. Kỳ vọng nằm ở `expected-71.md`; các test ID nhắc ở đây chỉ để trỏ.
>
> **Quy ước description từ 21/09 (PO) — dán vào mọi prompt bước 06–14, và một lượt sửa hồi tố cho 01–09 đã publish:** *"Set the Description of each `A5StepNN_*` Server Action to start with: `Bước nội bộ N của RunAdjudicationChain — không phải tool, không gọi trực tiếp.` followed by one sentence of what the step does. Do NOT expose any `A5Step*`, `A5Chain_*` or `RoundVnd` action to any agent or as a Service Action."*

## Lượt F — A5Step06_LineCoverTest + A5Step07_BenefitProvisos

**Step06** (per line, D1 §5.5, A5 §8.2), chỉ dòng chưa Declined:
1. `BenefitCode = "NONE"` ⇒ Declined `NOTCOV`, Cut(6, "NOTCOV", Running, "{Description}: {Claimed} — không thuộc danh mục được chi trả"), Running 0. (T-CH06-a)
2. `BenefitCode = "ASSESSOR"` ⇒ Held, HoldReason "OTHER — giám định viên chọn quyền lợi", Ctx.Held, Checkpoints += "[B6] dòng {n} ASSESSOR;". (BR-CHAIN-1)
3. `BenefitCode` là mã thật nhưng không có dòng `PolicyBenefitSnapshot` cho PolicyId ⇒ Declined `NOBEN`, Cut(6, "NOBEN", …, "{Description}: {Claimed} — chương trình không bao gồm quyền lợi {code}").
4. Aggregate `Recommendation` ⋈ `Assessment` where Assessment.ClaimId = Ctx.ClaimId (kiểm tên FK thật) — với dòng có Recommendation `ClaimLineId` = dòng: nếu `Decision = "Confirmed"` (Mentor đọc giá trị thật đang seed cho hồ sơ 0001 và báo) và `Code in (NOTREL, NOTCOV)` ⇒ Declined theo Code, Cut(6, Code, Running, Recommendation.Title), Running 0; nếu Decision khác ⇒ log "dòng {n}: khuyến nghị {Code} đang {Decision} — chưa áp (chờ người xác nhận)". (T-CH06-b)
Sau vòng: mỗi dòng `CoveredAmount = Running`; `Ctx.CoveredTotal = Σ`. StepLog: "{k} dòng thuộc phạm vi = {CoveredTotal}; {d} dòng từ chối ({codes}); {h} dòng chờ". Outcome CUT/HOLD/PASS. (T-CH06-c)

**Step07**: Aggregate TermRegister Status ACTIVE, Scope in (BENEFIT, BENEFIT_SUBTREE), RuleSetRef = Ctx.RuleSetRef. 0 dòng ⇒ Outcome SKIP, log "0 điều khoản cấp quyền lợi trên sản phẩm này — bước tồn tại, tập rỗng (D1 B-23)". ≥1 dòng ⇒ guard BR-CHAIN-1: Ctx.Held, checkpoint "[B7] {Code}: proviso chưa được cài", Outcome HOLD. ResultAmount = Σ Running. (T-CH07-a)

DryRun: + CallS06/AssignS06, CallS07/AssignS07.

## Lượt G — A5Step08_SubLimit + A5Step09_CountLimit

**Step08** (D1 granularity: per visit / per day / per event), dòng chưa Declined và BenefitCode là mã thật:
- Lấy PBS (PolicyId, BenefitCode): `LimitType`. Mentor đọc các giá trị LimitType thật đang seed (annual/per_visit/per_day/per_event — báo chính tả thật) và cap tương ứng `PerVisitCap/PerDayCap/PerEventCap`.
- Khoá nhóm: PER_VISIT ⇒ BenefitCode + "|" + VisitKey; PER_DAY ⇒ BenefitCode + "|" + ServiceDate; PER_EVENT ⇒ BenefitCode. annual ⇒ không áp ở bước 8.
- Mỗi nhóm: Σ Running so cap (>0). Vượt ⇒ cắt phần vượt, dòng tiêu trần theo Ordinal tăng dần (dòng cuối nhóm chịu cắt trước), `RoundVnd` mỗi dòng, Cut(8, "CAP_VISIT", cắt, "{benefit} {nhóm}: {Σ} so trần {cap}"). Log mỗi nhóm kể cả không cắt. (T-CH08-a/b/c)
- Outcome CUT nếu có cắt else PASS.

**Step09** (per benefit, per period):
- `LimitLedger` + `UsedCount` Integer + `CountUnit` Text 10 (seed: mọi dòng 0 / rỗng — `Seed_LimitLedger` và mọi nơi tạo ledger).
- Với mỗi BenefitCode thật có dòng chưa Declined: PBS `CountLimit`, `CountUnit`. `CountLimit = 0` ⇒ log "{benefit}: không giới hạn lượt" (T-CH09-b). `CountUnit` = VISIT (Mentor báo chính tả thật) ⇒ số visit mới = distinct VisitKey trên benefit; đọc `LimitLedger.UsedCount` (MemberId, PolicyId, BenefitCode, PeriodYear = Year(TreatmentFrom)); remaining = CountLimit − UsedCount; visit thứ (remaining+1) trở đi theo ServiceDate tăng dần ⇒ mọi dòng của visit Declined `LIMIT`, Cut(9, "LIMIT", Running, "{benefit}: lượt {i}/{CountLimit} vượt hạn mức, đã dùng {UsedCount}"). (T-CH09-a)
- `CountUnit = DAY` ⇒ dòng Held, HoldReason "thiếu fact: số ngày điều trị theo dòng (Q-IDP-06)", Ctx.Held, Checkpoints += "[B9] {benefit} DAY: Q-IDP-06;". Không decline, không pass. (T-CH09-c, BR-CHAIN-1)
- Không ghi ledger.

DryRun: + S08/S09; block Administration đổi tên "Chain dry-run (A5 steps 1–9)".
