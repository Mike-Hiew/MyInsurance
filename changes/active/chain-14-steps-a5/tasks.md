# Change — Chuỗi 14 bước theo A5 + 65 unit test

**Trạng thái:** 🟡 21/09 — việc 1–2 xong · **bước 1–5 dựng xong, publish ClaimMVP rev 141, dry-run hồ sơ 0001 = 5/5 chạy** (`current-chain.md` §4) · **chờ PO:** (a) dán build-04 §1.1 + §9 → `expected-65.md`; (b) quyết term register theo A5 §7.2 (model + seed) trước khi bước 4 có nghĩa · **Hạn:** 25/09 · **Chặn:** số tiền, tức là chặn bài dự thi
**Spec liên quan:** `specs/adjudication/spec.md` · delta ở `./specs/adjudication/spec.md`
**Bị chặn bởi:** `../verify-entity-vs-build01` (chuỗi ghi vào entity) và `../approval-gate-4-conditions` (bước cuối)

## Vấn đề

`RunAdjudicationChain` đã tồn tại trên ClaimMVP và đang chạy — nhưng **chưa ai xác nhận nó khớp A5**. BRD lệch A5 ở **4 chỗ làm đổi tiền**, và chuỗi hiện tại được dựng trong giai đoạn mà BRD còn được coi là chuẩn.

Đây là phần duy nhất của hệ **có sẵn bộ test** (65 unit test, `build-04` §9) ⇒ là phần duy nhất đo được ngay thay vì tranh luận.

## Việc

- [x] **1. Dựng bảng đối chiếu** `RunAdjudicationChain` hiện tại × 14 bước A5 (`build-04` §1.1) → `current-chain.md`
- [x] **2. Xác nhận 4 chỗ lệch có mặt hay không trong code hiện tại:** — cả 4 có mặt theo tên/mô tả vỏ (`current-chain.md` §1–2); xử lý = không đưa vào chuỗi A5 mới
  - [x] Bước khấu trừ BHYT — **A5 đã loại bỏ** — `Step06_SocialInsuranceDeduction` vỏ rỗng, bỏ khỏi chuỗi
  - [x] Bước Deductible — **sản phẩm không có** — `Step10_Deductible` vỏ rỗng, bỏ khỏi chuỗi
  - [x] Làm tròn — `Step12_Rounding` (1.000 ₫, bước riêng) bỏ; `RoundVnd()` half-away-from-zero gọi ở bước 8/10/11/12
  - [x] Hai bước term engine bị BRD gộp — `Step05_CoverageScopeExclusions` gộp; A5 tách 4 (EXCL) / 6 (line cover) / 7 (proviso)
- [ ] **3. Tách mỗi bước thành một Server Action riêng** — dựng mới `A5Step01…14` theo `specs/adjudication/spec.md` §Thiết kế; **đích trung gian: báo cáo sau bước 1–5** (PO 21/09)
  - [x] 3a. Hạ tầng: `ChainCtx/ChainLineCtx/ChainCutCtx`, `RoundVnd`, `Policy.PremiumPaid`, `Claim.ConditionClassId` + backfill, `A5Chain_LoadContext` (rev 140)
  - [x] 3b. `A5Step01…05` + `A5Chain_DryRun` + block Administration › "Chain dry-run (A5 steps 1–5)" (rev 140, sửa rev 141) — không ghi entity
  - [x] 3c. Bước 4 = 6 predicate có tên + guard BR-CHAIN-1 trên `TermRegister` A5 §7.2 (10 dòng, 6 ACTIVE / 4 DRAFT vô hình) — rev 144; T-CH04-d n = 10, m = 6 ✅
  - [ ] 3d. `A5Step06…14` — `expected-71.md` đầy 21/09
    - [x] 3d-i. `A5Step06…09` + DryRun tới 9 (rev 144) — hồ sơ 0001 sau bước 9 = 2.500.000 = hand-run lối C
    - [x] 3d-ii. `A5Step10…13` + DryRun tới 13 (rev 147 → sửa 148–151); hồ sơ 0001 sau bước 10 = **2.000.000** ✅
    - [x] 3d-iii. `A5Step14_PostLedger` + `LedgerPosting` (rev 152) · `RunAdjudicationChain` + `A5Ledger_Finalize/Reverse` + Admin "Run chain (writes)" (rev 153) · InputsHash 2000 (rev 154). Chạy thật hồ sơ 17: ChainRun 104/105 = 2.000.000, 14:POSTED, round-trip 811/279 ms
    - [ ] 3d-iv. `Seed_AssessmentAndTerms` ngừng tạo ChainRun/CalcSnapshot — Mentor xoá được 3/… node rồi hết bộ nhớ sandbox (~200 node) → **sửa tay ODC Studio** hoặc tách action. **Prompt chia nhỏ 7 lượt: `mentor-prompt-n1-seedchainrun-cleanup.md`** (21/09) — mỗi lượt xoá tối đa 4 node, lượt đầu chỉ đọc; **22/09 — RÚT TOÀN BỘ. Diff với rev 162: `Seed_AssessmentAndTerms` CHƯA TỪNG ĐỔI.** Hồ sơ B và D **không bao giờ có** node `CreateCalcSnapshot` (B: 9 Assign, D: 3 Assign). Chỉ A và C có Create, 9 + 9 = **18**. Con số "30" và "đã xoá 12 node" đều do Mentor bịa, bịa nhất quán với nhau nên qua được mọi lần đếm lại — xem README rule 16. **Trạng thái thật: seed vẫn tạo 4 ChainRun + 18 CalcSnapshot.** Việc dọn A và C (18 node) vẫn còn nguyên, chưa ai chạm. Nghi phạm cho việc C ra 0 dòng ở runtime: handler `AllExceptions → LogError → End` nuốt lỗi. Chờ panel trả lời bằng số.
  - [ ] 3e. **Bề mặt tool (PO 21/09):** description mọi `A5StepNN_*` (kể cả 01–09 đã publish) mở đầu *"Bước nội bộ N của RunAdjudicationChain — không phải tool, không gọi trực tiếp."* · description `RunAdjudicationChain` viết lại cho agent (mẫu ở `../agent-architecture/specs/agents/spec.md`) · input tool phẳng · **không khai bước nào làm tool** — kiểm bằng screenshot tab Tools khi tạo agent
- [ ] **4. Seed 215 bản ghi sản phẩm** (`build-04` §5–§6) — không có seed thì không test được bước nào
- [ ] **5. Chạy 71 unit test** (`build-04` §9 = `expected-71.md`, PO dán 21/09; 65 là ước lượng cũ). Kỳ vọng nguyên văn là dữ liệu — không bao giờ lấy từ output code. **Bộ khung** (`test-harness.md`): Entity `ChainTest` 71 dòng (I1–I3 ✅) · fixtures 28 hồ sơ `TST-*` (I4) · `RunChainTests` + màn `ChainTests` (I5) · 29 test bước 1–9 chạy được ngay; pass/fail ghi vào `test-run.md`
  - [x] 5a. Harness rev 145 (I1–I5): 71 dòng · 28 fixture · `RunChainTests` · màn `ChainTests`
  - [x] 5b. Run #1 rev 145: **26/29 pass** — fail T-CH04-e (TERM-HC-001 không bắn trên Z41.1), T-CH05-e (log redirect), T-CH09-a (wording) → lượt K
  - [x] 5c. Run #3 rev 148: **29/29** (sau K: Substr 0-based ×3, Step05 Lines.Current)
  - [x] 5d-i. Lật 11 test bước 10–13 (I6-B/D): **run #6 rev 151 = 38/38** (bắt lỗi thật: AnnualLimit đọc từ ledger)
  - [x] 5d-ii. Lật T-CHAIN-01/02/03/04/05 + T-CH12-d (W-8, không ledger): **run #9 rev 157 = 44/44**; T-CHAIN-05 RERUN 2.000.000 → 1.880.000 (máy sinh lần đầu); T-CHAIN-03 SAME hash/steps (rev 158 grid 200 dòng)
  - [x] 5d-iii. T-CHAIN-06 qua runner (Mode CONCURRENT, fixture Part6 FX-M8 + 06a/06b): **run #11 rev 162 MOVED 640.000 → 360.000**, posting run 1 đảo, RunNo +1 — thử hàm `A5Ledger_Finalize`, không phải luồng Approve
  - [ ] 5d-iv. **KHOẢNG TRỐNG:** khi PO chốt 3/4 điều kiện và `A5Ledger_Finalize` nối vào Approve thật (`approval-gate-4-conditions`) → **chạy lại T-CHAIN-06 qua Approve**. Chưa làm = T-CHAIN-06 chưa xong.
  - [x] 5d-v. T-CHAIN-07 (Mode RUN, `ClaimLine.TreatmentDays` = A4 L18, bước 9 DAY đọc L18, bước 14 post DAY lên 100.1.1 + 100.1, ApplyPosting DAY): **run #11 rev 162 POSTED 3.000.000, "2 DAY (100.1.1 +1, 100.1 +1)"**; T-CH09-c vẫn HOLD — spec §1.5
  - [x] 5d-v-b. Seed cleanup thật (`ChainTest_CleanupFixtures`, toast "Đã xoá 185 hồ sơ TST-, hiện có 42") · `ChainTest_ConfirmRerun` tự reset NOTREL
  - [x] 5k. **Publish rev 167 (22/09 ~01:04 UTC)** — Backfill không còn ghi Policy (kiểm node thật rev 167: none); cùng rev: `Seed_ResetDemoClaimData` +xoá LedgerPosting/TermCheckpoint, handler Abort=Yes; seed 01:05 sạch (0 lỗi, docs ×1). Bỏ nhánh ghi `Policy.PremiumPaid` khỏi `Seed_BatchB_Backfill` (`assignNode2`: If PRow.PremiumPaid = False → PRow.PremiumPaid = True → UpdatePolicy) — nguồn drift T-CH01-c (run #12). Không đụng phần backfill khác. (PO 22/09)
  - [ ] 5d-vi. Còn 26: T-CH14-a…f · T-SEED-01…11 · T-GATE BLOCKED (3/4 điều kiện) · T-CH11-c/d MANUAL
- [ ] **5l. TRƯỚC PITCH — F48 đọc ChainRun mới nhất CÓ CalcSnapshot**, không phải mới nhất. Hoặc tối thiểu hiện *"run này chưa có dữ liệu bước"* thay vì bảng trống. Lý do: 0003 có đủ 9 dòng trong DB mà tab Calculation trống. Cùng cơ chế áp cho `CLM-2026-0001` ⇒ **một run rỗng tạo nhầm là màn hero trắng giữa lúc pitch**. README rule 19
- [x] **5m. ĐÓNG 22/09 (rev 167) — checklist 18/15/18/12 → 6/5/6/4, CLAIM_SUBMITTED 3× → 1×, không còn 23503. ~~cũ:~~ ƯU TIÊN 1 — `Seed_ResetDemoClaimData` chạy nửa chừng âm thầm.** `LedgerPosting` không bao giờ bị xoá ⇒ `DelCR` ném `23503` ⇒ handler `AllExceptions` + **`Abort Transaction = No`** nuốt lỗi: phần trước commit, **mọi node sau `DelCR` không chạy** (ClaimDocument · AgentRun · **reset ClaimLine về PENDING** · ROOM · **xoá LimitLedger** · **xoá TermRegister**), nút vẫn báo thành công. **Sửa:** (1) thứ tự xoá RecEvidence → Recommendation → Assessment → **LedgerPosting (ClaimId ∈ 4)** → CalcSnapshot → ChainRun → ClaimDocument → AgentRun → **TermCheckpoint** → LimitLedger → TermRegister; (2) **`Abort Transaction = Yes`**; (3) màn hình báo *thất bại* thay vì toast thành công. **Kiểm trước khi sửa:** đếm `TermRegister` (phải 10, nếu là 10×N thì đã tích luỹ), `ClaimLine.DecisionCodeId` của 4 hồ sơ đầu, số dòng `LimitLedger`. **Kiểm sau khi sửa:** bấm reset → không dòng `23503` nào cùng traceId → TermRegister = 10 → ClaimLine = PENDING → LimitLedger dọn sạch
- [~] **5k. ĐÃ SỬA 22/09, CHƯA PUBLISH — gộp publish với 5m, rồi diff node của revision mới. `Seed_BatchB_Backfill` NGỪNG GHI `PremiumPaid` (PO quyết 22/09).** Xoá nhánh `If PRow.PremiumPaid = False → True → UpdatePolicy` ở `assignNode2`. **Không** dùng cách lọc `PolicyNo not like "FX-%"` — `False` không phải null, nó là dữ kiện *chưa đóng phí*; lọc chỉ che triệu chứng cho fixture còn policy thật vẫn bị ghi đè. Batch B có hợp đồng *"chỉ điền null, không tạo/xoá, không đụng tiền"* — nhánh này tự phá hợp đồng đó. Bước 1 của chuỗi kiểm `premiumPaid`, nên đây là lỗi nghiệp vụ, không phải lỗi test. **Làm trước 5h**, và kiểm bằng diff node của revision sau khi publish
- [ ] **5h. Harness tự bảo vệ dữ liệu (PO quyết 22/09 — lối b).** `RunChainTests` gọi `ChainTest_CleanupFixtures` + Part 1–7 trước mỗi lần chạy (~5 s). **Nghiệm thu:** cố ý bấm "Re-run demo seed" để làm bẩn, rồi chạy suite **hai lần liên tiếp không bấm Seed fixtures tay** — cả hai phải 46/46. Xem README rule 17
- [x] **5i. ĐÃ ĐỌC 22/09 01:20** — 17 ChainRun · 144 CalcSnapshot · 0 mồ côi. ~~cũ:~~ số từ panel "Chain data".** Rev 163 publish để đọc số này, và số chưa về. Bảng B (CalcSnapshot theo ChainRunId) và bảng C (mồ côi) trả lời 5e
- [x] **5j. Rev 164/165 — ĐÃ ĐỌC 22/09.** 164 thêm lại `CrCRB` + `CrCSB7/8/9` (170→187); **165 xoá `CrCSB1–9` + `CrCSD1–3` = đúng 12 (187→175)** ⇒ lượt xoá CÓ THẬT, publish 21/09 16:25Z. Phần màn hình/entity/aggregate/site property **không so được**, đừng đọc thành đã so hết. ~~cũ:~~ PO duyệt rev 163 (panel). Hai rev sau publish mà không qua PO. Đọc và ghi lại từng rev đổi gì; rule 16 áp dụng — một revision không ai giải thích được là một revision không tin được
- [x] **5e. ĐÓNG 22/09 — hồ sơ C đẻ đủ 9 dòng, 0 mồ côi, panel khớp 144 = 9×14 + 2×9. "0003 ra 0 dòng" là lỗi HIỂN THỊ (F48 đọc run mới nhất, run đó rỗng). Xem 5l/5m. ~~cũ:~~ CHẶN — giải thích hồ sơ C ra 0 dòng CalcSnapshot.** *(22/09: loại được giả thuyết (a) mồ côi — code của C giống hệt A. Còn (b) `Seed_ResetDemoClaimData` xoá sau khi seed tạo, và (c) trạng thái publish. Trả lời bằng panel sau khi publish rev 163.)* ~~cũ:~~ (readout 21/09 23:40). Phải có 9 (`CrCSC1–9` không bị xoá; đếm sau lượt xoá = 18 = A 9 + C 9). Kiểm theo thứ tự, **không đoán**: (a) `CrCSC*` ghi `ChainRunId` từ biến nào, biến đó có được gán không — nếu không thì là dòng mồ côi `ChainRunId = 0`, đúng dạng hỏng 21/09; (b) `Seed_ResetDemoClaimData` có xoá `CalcSnapshot` **sau** khi seed tạo không; (c) app đang chạy rev nào — đã publish bản sửa chưa
- [x] **5f. Panel read-only "Chain data" trên Administration (PO DUYỆT 22/09) — DỰNG XONG 22/09** trong session db5fbb92, 0 lỗi, warning 103. **Chờ publish rev 163 mới đọc được số.** Hai aggregate group by, **chỉ đọc, không entity mới, không action ghi, một lượt Mentor**: (1) `ChainRun` group by `ClaimId` → số dòng, max `RunNo`, `TriggeredBy` distinct; (2) `CalcSnapshot` group by `ChainRunId` → số dòng, **kèm dòng `ChainRunId = 0` hoặc trỏ tới ChainRun không tồn tại**. Lý do duyệt: đã đếm sai ba lần bằng mắt, và mở 54 màn F48 vẫn không trả lời được 5e. Panel này cũng là công cụ kiểm fixture sạch trước pitch
- [ ] **5g. Quét Max Records mặc định trên MỌI aggregate đọc danh sách** — làm NGAY, không đẩy batch F. Đã cắn hai lần: grid `ChainTests` (sửa ở L3) và dropdown hồ sơ ở Technical trace (cắt 50 dòng, thiếu TST-T-CHAIN-05/06a/06b/07). Một danh sách bị cắt trông giống hệt một danh sách đầy đủ
- [ ] **6. Kiểm determinism:** chạy hồ sơ B hai lần, cùng đầu vào → cùng đầu ra, byte-for-byte
- [ ] **7. Kiểm kịch bản B ra đúng số:** `2.000.000 → 1.880.000` · dòng 4 `NOTREL −168.000` · `CAP_VISIT` 628.000 → 610.000 · `COPAY` 500.000 → 470.000 *(sửa 21/09 — cặp cũ NOTCOV −80.000 / CAP_VISIT −40.000 là chép nhầm; xem `verify-entity-vs-build01/batchB2-spec.md` §Kiểm số)*
- [ ] **8. Kiểm before/after là chạy lại thật**, không phải `confirmed ? P2 : P1`
- [ ] **9. Hash đầu vào + banner "số liệu cũ"** — không khớp thì khoá nút trình duyệt
- [ ] **10. Sửa `BuildBenefitRows`** — settlement đang chia `Claimed` sai theo benefit (ROOM 1.450.000 / OTHER 350.000 thay vì 600.000 / 1.200.000, đã thử sửa 3 lần). **Làm bước này SAU 1–9**: nhiều khả năng lỗi nằm ở dữ liệu vào chứ không ở hàm gộp
- [ ] **11. Chụp lại tab calc + settle**, so với `../../../output/bnk-dev-claimmvp/shots-rev88/claimdetail.jpg`

## Xong nghĩa là

- 65/65 test pass
- Hồ sơ B ra đúng ba con số ở việc 7
- Chạy hai lần cho cùng kết quả
- Tab settle hiển thị đúng phần chia theo benefit
- `AG-2` có một Server Action đáng để gọi
