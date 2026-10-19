# DELTA — `specs/adjudication` · chuỗi 14 bước

> Đoạn khác biệt giữa **code hiện tại** và **A5**, ghi ra để không ai phải nhớ.
> Khi 65 test pass, merge vào Project doc `build-04` rồi archive change.

## Chuẩn: A5 §6.1 + D1 §5.3 thắng BRD `CH-01…CH-14`

| # | BRD hackathon | A5 (chuẩn) | Có trong code? |
|---|---|---|---|
| 1 | Có bước khấu trừ BHYT | **Đã loại bỏ** | **CÓ** — `Step06_SocialInsuranceDeduction` (T07, CH-06) |
| 2 | Có bước Deductible | **Sản phẩm không có** | **CÓ** — `Step10_Deductible` (CH-10 `DEDUCT`) |
| 3 | Làm tròn xuống 1.000 ₫ ở một bước riêng | **Đồng nguyên gần nhất, ở từng bước** | **CÓ** — `Step12_Rounding` "bước DUY NHẤT làm tròn", B-20 xuống 1.000 |
| 4 | Gộp mất 2 bước term engine | **Giữ đủ 2 bước** | **CÓ** — `Step05_CoverageScopeExclusions` gộp loại trừ + NOTREL/NOTCOV; không có bước proviso (A5 bước 7) |

`SC-H4` chỉ có nghĩa sau khi bốn dòng trên được sửa.

## Lỗi đã biết trong code

**`BuildBenefitRows` chia `Claimed` sai theo benefit.** Quan sát ở rev 47:
- Ra: ROOM `1.450.000` / OTHER `350.000`
- Đúng: ROOM `600.000` / OTHER `1.200.000`

Đã thử sửa 3 lần trong hàm gộp, không được ⇒ giả thuyết hiện tại là **lỗi ở dữ liệu vào, không ở hàm gộp**. Kiểm lại sau khi chuỗi khớp A5.

## Ghi nhận khi kiểm xong

> *(để trống — điền kết quả từng dòng, kèm ngày)*


## Thiết kế 14 Server Action theo A5 (21/09) — ràng buộc rút từ A5 §6 + D1 §5.3

Tên: `A5Step01_Eligibility` … `A5Step14_PostLedger` (tiền tố `A5Step` để không nhầm với vỏ `StepNN_` theo BRD; vỏ BRD xoá sau khi 65 test pass). Chữ ký chung: in `ClaimId`, `RunNo`; in/out `Ctx` (Structure `ChainContext`: danh sách dòng với `AmountClaimed / AfterStep4 / AfterStep6 / AfterStep8 / AfterStep9 / AfterStep10 / AfterStep11 / AfterStep12 / Payable / DeclineCode / CauseCodes`, cộng event + snapshot); out `StepLog` (Text, tiếng Việt). **Chỉ `A5Step14_PostLedger` và `RunAdjudicationChain` ghi Entity tiền.**

> **Bề mặt tool (PO 21/09 — bất biến, chi tiết `../../agent-architecture/specs/agents/spec.md` §Bề mặt tool):** `RunAdjudicationChain(ClaimId)` là **tool duy nhất** agent thấy. **Không `A5StepNN_*`, helper, `A5Chain_*`, `RoundVnd` nào được khai tool** — khai lẻ là cho agent gọi bước sai thứ tự, tiền hết chảy qua chuỗi. `ChainCtx` lồng List là hợp lệ vì chỉ đi giữa các bước nội bộ; input tool phải phẳng. Description mỗi `A5StepNN_*` mở đầu: *"Bước nội bộ N của RunAdjudicationChain — không phải tool, không gọi trực tiếp."* Description `RunAdjudicationChain` viết cho agent, có câu *"Không tự tính số tiền…"*.

| # | Server Action | Granularity (D1) | Đọc | Quy tắc chốt |
|---|---|---|---|---|
| 1 | `A5Step01_Eligibility` | cả hồ sơ | Member (CoverStart/End, PremiumPaid), Policy | 3 boolean; fail ⇒ decline `PERIOD` cả hồ sơ, không bao giờ auto-approve |
| 2 | `A5Step02_WaitingPeriod` | mỗi event | `BenefitWaitingPeriod` (plan × benefit gốc × condition class), `Member.CoverStart`, `Claim.TreatmentFrom` | `WaitingDays` từ bảng, **không hằng số**; `n/a` ⇒ BR-CHAIN-1 hold + checkpoint; fail ⇒ `WAIT`, log ghi kỳ chờ + ngày đếm từ + ngày điều trị (BR-WAIT-5) |
| 3 | `A5Step03_PreExisting` | mỗi event | condition class | no-op có log trên sản phẩm này (D1) |
| 4 | `A5Step04_GeneralExclusions` | dòng / event theo effect | `TermRegister` scope PRODUCT | term engine; **mọi clauseRef TBC- ⇒ checkpoint, không decline** (BR-TERM-10); ghi mọi kết quả kể cả không bắn |
| 5 | `A5Step05_BenefitMapping` | dòng | `ClaimLine.Category` → `ServiceCategory` | ghi `BenefitCode`; `UNMAPPED` ⇒ hold + checkpoint (BR-CHAIN-1), **không decline** |
| 6 | `A5Step06_LineCoverTest` | dòng | `Recommendation` với `Decision = Confirmed` | NOTREL/NOTCOV **chỉ** từ recommendation đã Confirmed; SUPPLEMENT payable; log gồm item + amount (BR-LINE-1) |
| 7 | `A5Step07_BenefitProvisos` | dòng trong benefit | `TermRegister` scope BENEFIT/BENEFIT_SUBTREE | tập rỗng trên sản phẩm này — chạy, log "0 term" |
| 8 | `A5Step08_SubLimit` | visit / ngày / event | snapshot `LimitType`, `PerVisitCap/PerDayCap/PerEventCap` | cap **theo benefit con, theo visit** (BR-LEDGER-14: visit = ngày + provider); cắt ⇒ `CAP_VISIT` (mã dùng cho cả 3 loại — build-01 8 causeCode) |
| 9 | `A5Step09_CountLimit` | benefit / kỳ | `CountLimit`, `CountUnit`, ledger counter OCCURRENCE/DAY | `CountLimit = COUNT_UNLIMITED()` ⇒ bỏ qua; thiếu counter ⇒ hold (BR-CHAIN-1) |
| 10 | `A5Step10_CoPayment` | dòng | `CopayPct` (bp, `INHERIT` → tổ tiên gần nhất có giá trị) | base = sau bước 9; `COPAY`; round từng dòng |
| 11 | `A5Step11_DeductionRatio` | event | `DeductionPct`, `DeductionCondition`, Provider.InNetwork | chỉ khi điều kiện đúng; B-19 chưa chốt ⇒ **không bắn** (đọc benefit của dòng, không tổ tiên); log điều kiện đã kiểm |
| 12 | `A5Step12_ParentAnnualCap` | benefit / kỳ, gộp mọi dòng | `CapModeId` (Own ⇒ AnnualLimit; Included ⇒ bỏ qua nút này), `LimitLedger` Final | walk tới root; cắt ⇒ `LIMIT_YEAR`; thứ tự dòng tiêu hạn mức = thứ tự dòng trên bảng kê (B-20 tạm) |
| 13 | `A5Step13_PlanLevel` | cả hồ sơ | — | no-op có log |
| 14 | `A5Step14_PostLedger` | dòng → mọi counter trên đường tổ tiên | — | ghi `LimitLedger` Provisional cho benefit + tổ tiên (BR-TREE-1/2), `ClaimLine.AmountPayable`, `Claim.TotalPayable`, `ChainRun` (InputsHash, StepOutputsJson) |

Làm tròn: `RoundVnd(x) = Round half away from zero` gọi ở bước 8, 10, 11, 12. Phần trăm giữ basis points nguyên. Chạy hai lần cùng input ⇒ cùng `StepOutputsJson` (SC-H4).

Hậu xử lý (không phải bước): `BuildCustomerBorneBreakdown` (bảng "Vì sao Quý khách tự chịu" theo causeCode) và `EvaluateAutoApprovalGate` (change `approval-gate-4-conditions`).


### Hình dạng kỹ thuật (đợt C-1, bước 1–5) — chốt 21/09

**Structure (không phải Entity — chuỗi tính trong bộ nhớ, chỉ bước 14 ghi tiền):**
- `ChainLineCtx`: `ClaimLineId` Long · `Ordinal` Int (1-based theo Id) · `Description` · `Category` · `BenefitCode` · `ParentBenefitCode` · `RootBenefitCode` · `ServiceDate` Date · `Claimed` Decimal · `Running` Decimal (số còn lại sau bước gần nhất) · `Declined` Bool · `DeclineCode` Text · `Held` Bool · `HoldReason` Text
- `ChainCutCtx`: `LineOrdinal` Int · `StepNo` Int · `CauseCode` Text (8 mã build-01 §4.23) · `Amount` Decimal · `Detail` Text
- `ChainCtx`: `ClaimId` · `RunNo` · `PolicyId` · `MemberId` · `PlanCode` · `LossTypeId` · `BenefitGroupCode` (từ `LossType.BenefitGroup`) · `ConditionClassId` · `TreatmentFrom/To` · `IsInNetwork` · `CoverageFrom/To` · `PolicyEffective/Expiry` · `PremiumPaid` · `Lines` List<ChainLineCtx> · `Cuts` List<ChainCutCtx> · `ClaimDeclined` Bool · `ClaimDeclineCode` · `Held` Bool · `Checkpoints` Text (JSON list) · `TotalClaimed/TotalPayable/TotalCustomerBorne`
- Chữ ký mỗi `A5StepNN_*`: in `Ctx` (ChainCtx) → out `Ctx`, `StepLog` Text (tiếng Việt, cho `CalcSnapshot.Detail`), `Outcome` Text (PASS/FAIL/CUT/HOLD/SKIP), `ResultAmount` Decimal (tổng `Running` sau bước).
- `A5Chain_LoadContext(ClaimId)` → `ChainCtx` (đọc Claim, ClaimLine, Member, Policy, PolicyBenefitSnapshot; **không** ghi gì). `RoundVnd(x)` Server Function = half away from zero: `If(x >= 0, Trunc(x + 0.5), -Trunc(-x + 0.5))`.

**Hai attribute thiếu để bước 1–2 chạy trên dữ liệu thật (không suy diễn lúc chạy):**
- `Policy.PremiumPaid` Boolean, nullable — D1 §5.3 bước 1 "premium recorded paid — a boolean, no grace period". Seed: True cho 3 policy (màn Policies đang hiện "Premium Paid" từ literal).
- `Claim.ConditionClassId` FK `ConditionClass`, nullable — A5 §5 "class is determined at assessment and stored on the insured event" (MVP: event = claim). Seed tường minh theo hồ sơ trong `Seed_BatchB_Backfill` (bảng: 0001 GEN · 0002/0003 GEN · 0004 GEN (nha khoa, GEN 90 ngày ⇒ WAIT đúng showcase) · tai nạn → ACC · thai sản → BIR); **không** suy từ ICD lúc chạy chuỗi. Thiếu ⇒ bước 2 HOLD (BR-CHAIN-1), không PASS.

**Bước 2 tra `BenefitWaitingPeriod`** theo (`PlanCode` = `Policy.PolicyCode`, `BenefitGroupCode` = `LossType.BenefitGroup`, `ConditionClassId`). Không có dòng ⇒ `n/a` ⇒ HOLD + checkpoint "Tổ hợp chờ không tồn tại" (BR-WAIT-4). `Days = 0` là 0 ngày thật (ACC), không phải sentinel. Ngày đếm từ `Member.ContinuousCoverFrom` nếu có, else `CoverageFrom` (BR-WAIT-1/2); so với `TreatmentFrom`.

**Bước 4 term engine** (đợt này): đọc `TermRegister` `IsActive` and `Scope = "PRODUCT"`; mọi term có `ClauseRef` bắt đầu `TBC-` ⇒ ghi kết quả "checkpoint" (không decline, BR-TERM-10); term class J/E ⇒ checkpoint; class D với clause thật ⇒ `EXCL` theo effect. Trên dữ liệu hiện tại (mọi clause TBC-) bước này chỉ ghi log + checkpoint cho term khớp; **không** cắt tiền. Cách khớp term ↔ dòng: `TermRegister.Code` ↔ `ServiceCategory`/`Category` (TERM-HC-003 SUPPLEMENT) — đọc từ register, không hard-code.

**Bước 5** ghi `BenefitCode` vào Ctx từ `ServiceCategory` theo `TreatmentType` (Inpatient/Outpatient benefit); giữ `ClaimLine.BenefitCode` đã seed nếu khớp, lệch ⇒ log; `UNMAPPED` ⇒ HOLD dòng.

### Hình dạng kỹ thuật (đợt C-2, bước 4 thật + 6–9) — chốt 21/09 chiều, theo `a5-chain-spec.md` + `expected-71.md` (PO: TermRegister thuộc change này)

**Model (ClaimMVP) — thêm/sửa, đều là dữ liệu spec, không suy diễn:**
- `TermRegister` (giữ 8 cột cũ cho màn Term register) **+** `RuleSetRef` Text 50 (`A5-v1.5/HC-STD`, khớp `Claim.RuleSetRef`) · `Status` Text 10 (`ACTIVE`/`DRAFT`) · `Effect` Text 20 (`CHECKPOINT`/`DECLINE_LINE`/`DECLINE_EVENT`/`REQUIRE_DOC`) · `ReasonCode` Text 10 (`EXCL`) · `SelectorFact` Text 30 · `SelectorOp` Text 12 (`IN`/`STARTS_WITH`) · `SelectorValues` Text 200 (CSV) · `PredicateText` Text 300 (nguyên văn A5, chỉ để hiển thị) · `SortNo` Int. **Seed lại 10 dòng nguyên văn A5 §7.2** (`TERM-HC-001…010`, Scope `PRODUCT`, Class J/D/D/D/D/D/J/E/D/D, 6 ACTIVE #1 #2 #3 #4 #6 #7, 4 DRAFT #5 #8 #9 #10, ClauseRef `TBC-01…10`). DRAFT **có trong bảng, vô hình với engine** (T-SEED-06, T-CH04-d: n = 10, m = 6).
- **Không dựng expression engine.** Bước 4 cài **6 predicate có tên** theo `Code`: #1 `diagnosisCode startsWith Z41` (event: `Claim.IcdCode`) · #2 `serviceCategory in [SCREENING, VACCINATION]` · #3 `= SUPPLEMENT` · #4 `in [OPTICAL, HEARING]` · #6 `= DENTAL` **and** `lossType in [ILL_IP, DEN_IP, MAT_IP]` · #7 `diagnosisCode startsWith F1`. **Guard BR-CHAIN-1:** term ACTIVE mà engine không có predicate tên ⇒ checkpoint "term {Code} không đánh giá được — selector {SelectorFact} chưa được cài", không skip, không pass. Bỏ heuristic token-đầu-của-Name.
- Kết quả từng term (BR-TERM-1): `KHÔNG_THUỘC_PHẠM_VI` (selector không khớp dòng nào) / `CHECKPOINT` / `EXCL`. Mọi term hôm nay `TBC-` ⇒ **không term nào decline** (BR-TERM-10): #4 DECLINE_LINE cũng thành checkpoint nêu "thiếu trích dẫn TBC-04" (T-CH04-b). Nhiều term cùng bắn một dòng ⇒ log mang **mọi** clauseRef (BR-TERM-2). Header log: `n = 10 · m = 6 · x không thuộc phạm vi · y checkpoint · z EXCL` với x + y + z = m (T-CH04-d).
- `TermCheckpoint` Entity mới: `ClaimId` FK · `ClaimLineId` FK (null = event) · `TermCode` · `ClauseRef` · `Status` Text 12 (`OPEN`/`UPHELD`/`CLEARED`/`MOOT`) · `Note` Text 500 · `RaisedOn` DateTime · `DecidedBy` Text 100 · `DecidedOn` DateTime. Bước 4 **đọc** quyết định đã có: `UPHELD` ⇒ dòng Declined `EXCL` (Running 0, Cut StepNo 4) — đây là đường 3.000.000 → 2.600.000 của T-CHAIN-01; `CLEARED` ⇒ term ghi "đã xoá checkpoint"; không có/OPEN ⇒ HOLD. **Ghi** `TermCheckpoint` OPEN là việc của bước 14/wrapper, không phải dry-run. Seed: T-CHAIN-01 fixture có 1 dòng `UPHELD` cho TERM-HC-003.
- `ServiceCategory` static: ô `n/a` = **`"NONE"`**, `OTHER` = **`"ASSESSOR"`** (A5 §8.1 v1.5 — hai chuỗi có tài liệu, không phải null): DENTAL IP · ROOM OP · ICU OP · DELIVERY_NORMAL OP · DELIVERY_CS OP · ADMIN OP+IP ⇒ NONE; OTHER OP+IP ⇒ ASSESSOR. Bước 5: NONE/ASSESSOR **không decline** (T-CH05-c/d), ghi vào `BenefitCode` y nguyên, dòng rơi xuống bước 6. **Accident redirect** (A5 §8.1): `LossType.Value` bắt đầu `ACC` ⇒ mọi mã tra được → `500` (Parent "", Root 500), log ghi rõ mất sub-limit/count/copay (T-CH05-e).
- `LimitLedger` **+** `UsedCount` Int (bộ đếm OCCURRENCE/DAY dùng trong kỳ) · `CountUnit` Text 10 (`VISIT`/`DAY`/rỗng). Một dòng ledger = (member, policy, benefit, kỳ) mang cả AMOUNT và COUNT. Seed: mọi dòng hiện có `UsedCount = 0`.
- `ChainLineCtx` **+** `VisitKey` Text (ServiceDate + "|" + FacilityName) · `CoveredAmount` Decimal (số sau bước 6 — "thuộc phạm vi bảo hiểm", T-CH06-c) · `HoldTermCodes` Text. `ChainCtx` **+** `DiagnosisCode` (Claim.IcdCode) · `FacilityName` · `RuleSetRef` · `TermsTested` Int · `TermsCheckpoint` Int · `TermsDeclined` Int · `CoveredTotal` Decimal.

**Bước 6 `A5Step06_LineCoverTest`** (per line, D1 §5.5, A5 §8.2): thứ tự kiểm trên dòng chưa Declined: (a) `BenefitCode = "NONE"` ⇒ `NOTCOV` — log "{item}: {amount} — không thuộc danh mục được chi trả" (ADMIN, T-CH06-a); (b) `BenefitCode = "ASSESSOR"` ⇒ HOLD "OTHER — giám định viên chọn quyền lợi" (BR-CHAIN-1); (c) `BenefitCode` không có trong `PolicyBenefitSnapshot` của policy ⇒ `NOBEN`; (d) `Recommendation` gắn `ClaimLineId` với `Decision = "Confirmed"` và `Code in (NOTREL, NOTCOV)` ⇒ decline theo Code, rationale = `Recommendation.Rationale`; `Decision` khác (Proposed/null) ⇒ **không áp**, log "khuyến nghị {Code} đang Proposed — chưa áp" (T-CH06-b). Sau bước: `CoveredAmount = Running`, `Ctx.CoveredTotal = Σ` (T-CH06-c). Cut StepNo 6, CauseCode = decline code.

**Bước 7 `A5Step07_BenefitProvisos`**: đọc `TermRegister` ACTIVE, Scope in (`BENEFIT`, `BENEFIT_SUBTREE`), cùng RuleSetRef. 0 dòng ⇒ Outcome `SKIP`, log "0 điều khoản cấp quyền lợi trên sản phẩm này — bước tồn tại, tập rỗng (D1 B-23)" (T-CH07-a — phải render). Có dòng ⇒ guard BR-CHAIN-1 checkpoint (chưa có predicate).

**Bước 8 `A5Step08_SubLimit`** (granularity D1: per visit / per day / per event): với mỗi dòng chưa Declined, lấy PBS của `BenefitCode` (policy): `LimitType` = `PER_VISIT` ⇒ nhóm theo (`BenefitCode`, `VisitKey`) — hai dòng cùng ngày cùng cơ sở = **một** visit (T-CH08-c), 12 ngày khác nhau = 12 visit (T-CH08-b); `PER_DAY` ⇒ nhóm theo (`BenefitCode`, `ServiceDate`); `PER_EVENT` ⇒ nhóm theo `BenefitCode` cả claim. Cap = `PerVisitCap`/`PerDayCap`/`PerEventCap` (>0); Σ Running nhóm > cap ⇒ cắt phần vượt; **dòng tiêu trần theo thứ tự Ordinal tăng dần** (cùng quy ước B-20 tạm của bước 12: ngày tăng dần rồi số thứ tự dòng) ⇒ dòng cuối của nhóm chịu cắt trước, dòng đầu giữ nguyên; `RoundVnd` mỗi dòng, Cut StepNo 8 CauseCode `CAP_VISIT` (mã dùng cho cả 3 loại — build-01 §4.23). Cap theo **benefit con**, không bao giờ theo tổng claim (T-CH08-a: 200.1 cắt 200.000, 200.2 không). `annual` LimitType ⇒ không áp ở bước 8. Log mỗi nhóm: "{benefit} visit {ngày}: {Σ} so trần {cap} — cắt {x} / không cắt".

**Bước 9 `A5Step09_CountLimit`** (per benefit, per period): PBS `CountLimit = 0` ⇒ "Không giới hạn" (T-CH09-b, quy ước A5 §6.1 #9, không phải sentinel README); `CountUnit = VISIT` ⇒ số visit mới của claim = số `VisitKey` distinct trên benefit; đọc `LimitLedger.UsedCount` (member, policy, benefit, `PeriodYear` = năm của `TreatmentFrom`); còn = `CountLimit − UsedCount`; visit vượt (theo thứ tự ngày tăng dần) ⇒ mọi dòng của visit đó Declined `LIMIT`, Cut StepNo 9 — **CauseCode:** danh sách 8 causeCode build-01 §4.23 không có trên đĩa; dùng `LIMIT` (= DeclineCode A1 §5.2) và log nêu "hạn mức lượt"; ghi vào batch F việc xác nhận causeCode riêng cho count limit. `CountUnit = DAY` ⇒ cần số ngày điều trị theo dòng (Q-IDP-06) — **không có** ⇒ dòng HOLD, checkpoint "thiếu fact: số ngày điều trị theo dòng (Q-IDP-06)" (T-CH09-c, BR-CHAIN-1). Không ghi ledger.

**Dry-run** mở rộng tới bước 9; block Administration đổi tên "Chain dry-run (A5 steps 1–9)". Bảng test tương ứng: T-CH04-a…e, T-CH05-a…e, T-CH06-a…c, T-CH07-a, T-CH08-a…c, T-CH09-a…c, T-SEED-03/06 — **kỳ vọng ở `expected-71.md`, không ở đây.**

### Hình dạng kỹ thuật (đợt C-3, bước 10–14 + wrapper ghi tiền) — chốt 21/09 tối, theo `a5-chain-spec.md` §1.3–1.4 + A5 §4.2/§4.3/§6.2 + `expected-71.md`

**Bước 10 `A5Step10_CoPayment`** (per line): với dòng chưa Declined và BenefitCode thật: `CopayPct` từ PBS của dòng; `"INHERIT"` ⇒ đi lên `ParentBenefitCode` rồi `RootBenefitCode` cho tới khi gặp giá trị số; rỗng ⇒ không copay. Giá trị là **basis point** Text (`"2000"` = 20 %, HC-PRM `200` = `"1000"`, `300` = `"3000"`) — engine không quy đổi %. `Copay = RoundVnd(Running × bp / 10000)`; `Running −= Copay`; Cut(10, `COPAY`, Copay, "{benefit} {bp/100}% thừa kế từ {node}"). Base = số sau bước 9 (đã cắt cap/count/decline) — không phải billed (T-CH10-a: 2.400.000 → 1.920.000). Log mỗi dòng có copay + tổng khách chịu. Outcome CUT nếu có copay else PASS.

**Bước 11 `A5Step11_DeductionRatio`** (per event = per claim ở MVP): điều kiện `PROVIDER_OFF_NETWORK` = `not Claim.IsInNetwork` (MVP không có provider master ⇒ T-CH11-d/T-CH11-c ngoài phạm vi: ghi MANUAL). Trong mạng ⇒ Outcome SKIP, log "KHÔNG_ÁP_DỤNG — cơ sở trong mạng lưới, không điều kiện khấu trừ nào đúng" (T-CH11-a). Ngoài mạng ⇒ với mỗi dòng: `DeductionPct` của **benefit của dòng** (`DeductionScope = LINE_OWN_BENEFIT`, quy tắc tạm B-19) — trên sản phẩm này con `200.x/300.x` mang 0 ⇒ **không fire**; log "Điều kiện PROVIDER_OFF_NETWORK đúng; benefit {code} không mang deduction_pct — quy tắc tạm B-19 (DeductionScope = LINE_OWN_BENEFIT): không áp; nếu đọc theo tổ tiên sẽ là {2000 bp trên 200}" (T-CH11-b, T-CHAIN-02). Nếu một benefit dòng có DeductionPct > 0 (tương lai) ⇒ `RoundVnd(Running × bp / 10000)`, Cut(11, `DEDUCT`). Outcome SKIP/PASS/CUT.

**Bước 12 `A5Step12_ParentAnnualCap`** (per benefit gốc, gộp mọi dòng): với mỗi dòng chưa Declined, nút mang hạn mức năm = đi lên cây tới nút có `CapModeId = Own` và `LimitType = ANNUAL` và `AnnualLimit > 0` (100.1/100.1.1 là Included ⇒ lên 100; T-CH12-c). Nhóm theo nút đó; `Remaining = AnnualLimit − LimitLedger.UsedAmount` (member, policy, benefit gốc, PeriodYear = Year(TreatmentFrom); không có dòng ⇒ UsedAmount 0). Nếu Σ Running nhóm > Remaining ⇒ cắt phần vượt, **thứ tự tiêu hạn mức: ServiceDate tăng dần rồi Ordinal** (B-20 tạm; T-CH12-b), dòng cuối chịu cắt; `RoundVnd`; Cut(12, `LIMIT`, cắt, "{benefit gốc}: hạn mức năm {AnnualLimit}, đã dùng {Used}, còn {Remaining} — cắt {x}" (BR-TREE-5, T-CH12-a). Log cả nhóm không cắt: "{benefit}: {Σ} so hạn mức còn {Remaining} — trong hạn". Outcome CUT/PASS.

**Bước 13 `A5Step13_PlanLevel`**: Outcome SKIP, log "KHÔNG_ÁP_DỤNG — sản phẩm không có trần toàn plan; bước tồn tại cho sản phẩm tương lai" (T-CH13-a). ResultAmount = Σ Running.

**Bước 14 `A5Step14_PostLedger`** — **nơi DUY NHẤT ghi tiền** (CLAUDE.md §2 quy tắc 1). Input Ctx + `ChainRunId`. Ghi: (a) `ClaimLine.AmountPayable = Running` (0 nếu Declined), `LineDecisionId`/`DeclineReasonId` **không** đụng (A6 migrate giữ); (b) `Claim.TotalPayable = Σ Running`, `TotalClaimed` giữ; (c) `LedgerPosting` (Entity mới: ClaimId, ChainRunId, ClaimLineId, MemberId, PolicyId, BenefitCode, PeriodYear, CounterType `AMOUNT`/`DAY`/`OCCURRENCE`, Amount Decimal, Count Int, Status `PROVISIONAL`/`FINAL`/`REVERSED`, PostedOn, Note): trước khi post, mọi posting `PROVISIONAL` cũ của claim ⇒ `REVERSED` (BR-LEDGER-3: không xoá); rồi mỗi dòng payable > 0 post AMOUNT lên benefit của dòng **và mọi tổ tiên** (Parent, Root — composite đếm một lần/dòng, BR-TREE-2; T-CH14-a/f); OCCURRENCE +1 per distinct VisitKey trên benefit có `CountUnit = VISIT` (và tổ tiên mang count — trên sản phẩm này chỉ 200.3); DAY: **không post** khi thiếu số ngày (Q-IDP-06) — ghi log; PeriodYear theo `ServiceDate` từng dòng (L-2/L-3, T-CH14-e). `LimitLedger.UsedAmount` **không đổi** ở bước 14 (đó là số chính thức; đổi khi FINAL). (d) `CalcSnapshot` 14 dòng (StepNo, StepLabel "B01"…, StepName, Detail = StepLog cắt 500, ResultAmount, Outcome, ReasonCode = mã decline/cut đầu tiên của bước) sau khi xoá snapshot của ChainRun cũ **cùng RunNo** (không xoá run cũ). Log bước 14: "Post tạm: {n} posting AMOUNT ({Σ}), {m} OCCURRENCE; trạng thái PROVISIONAL".

**`RunAdjudicationChain(ClaimId, TriggeredBy)`** — wrapper thật (thay vỏ BRD): `A5Chain_LoadContext` → tạo `ChainRun` (RunNo = max(RunNo của claim) + 1, `InputsHash` = SHA-256 hoặc chuỗi tất định của (ClaimId, mọi dòng: Id·Category·Claimed·ServiceDate, ConditionClass, TreatmentFrom, IsInNetwork, RuleSetRef, TermRegister version = số dòng ACTIVE, danh sách TermCheckpoint UPHELD/CLEARED, Recommendation Confirmed), `Version` "A5-v1.5/chain-1", `RunAt`, `TriggeredBy`, `DurationMs` (attribute mới, đo bằng `CurrDateTime` đầu/cuối — độ phân giải giây; số ms thật lấy từ ODC trace)) → bước 1…13 (in-memory) → bước 14 (ghi) → `ChainRun.StepOutputsJson` = JSON 14 StepResult, 4 Total*. `A5Chain_DryRun` giữ nguyên (không ghi). **`Seed_AssessmentAndTerms` ngừng tạo `ChainRun`/`CalcSnapshot`** cho hồ sơ A/B/C/D (PO 21/09: chuỗi sở hữu RunNo); màn Calculation & ledger đọc `CalcSnapshot` của ChainRun mới nhất ⇒ hồ sơ demo phải chạy chuỗi thật một lần sau seed (nút "Run chain" trên ClaimDetail hoặc `RunSeedBatch0Now` gọi `RunAdjudicationChain(17)` cuối cùng — chọn cách 2 để demo không trống). `Claim.TotalPayable` seed 2.000.000 bị ghi đè bởi chuỗi ⇒ **phải bằng 2.000.000** (mốc PO). Hai action nhỏ cho change cổng: `A5Ledger_Finalize(ClaimId)` (PROVISIONAL → FINAL + cộng `LimitLedger.UsedAmount`/`UsedCount`) và `A5Ledger_Reverse(ClaimId)` — chưa nối vào Approve/Decline (thuộc `approval-gate-4-conditions`).

Test tương ứng: T-CH10-a…c, T-CH11-a/b (c/d MANUAL), T-CH12-a…c, T-CH13-a, T-CH14-a (b/c MANUAL tới khi nối approve), T-CH14-d (DAY không post — PARTIAL), T-CH14-e/f, T-CHAIN-01/02/03/04/07.
