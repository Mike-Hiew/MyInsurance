# Cổng tự duyệt — chạy thật trên tenant · rev 200–203 · 01/10/2026

Tiếp theo của `claude/cong-tu-duyet-4-dieu-kien-doc-tu-tenant-2026-10-01.md` (đặc tả, PO chốt phương án A).

## 7. Đã làm 01/10 — cổng chạy thật (rev 200 → 203), kiểm trên màn

| Rev | Thay đổi | Kiểm |
|---|---|---|
| 200 | Server action `EvaluateAutoApprovalGate(ClaimId)` — C1..C4 Pass/Reason, AllPass, Threshold, ChainRunId. Chỉ đọc. | — |
| 201 | `gateCard` trên ClaimDetail đọc action qua Data Action `GetGate`; nhãn **AG-4**; chân bảng trung thực "Threshold … fixed in this build (not yet an admin setting)" | Màn hiện; nhưng **mọi hồ sơ đều "1"** ở mọi phép đếm → phát hiện lỗi dưới |
| 202 | **Sửa lỗi đếm**: 5 aggregate có cột COUNT nên luôn trả 1 dòng → `.Count` luôn = 1. Đổi sang `If(X.List.Empty, 0, X.List.Current.<alias>)`. `GetLatestChainRun` sort RunNo DESC, Id DESC. | Hồ sơ 17/19/20 ra số khác nhau, đúng dữ liệu |
| 203 | **C1 payable** lấy `CalcSnapshot.ResultAmount` của bước cuối (StepNo lớn nhất) của run mới nhất — vì `RunAdjudicationChain` **không bao giờ ghi** `ChainRun.TotalPayable` (luôn 0) | Hồ sơ 17: 2.000.000 ₫ |

**Kết quả trên màn sau rev 203 (hard reload):**

| Hồ sơ | Header | C1 | C2 | C3 | C4 |
|---|---|---|---|---|---|
| 17 · CLM-2026-0001 | not met — needs a person | ✕ 2.000.000 ₫ · 2 limit cut(s) in run #2 · 0 declined | ✕ 6/6 assessed · 1 proposal pending | ✓ 0 checkpoint | ✓ 0 other claim |
| 19 · CLM-2026-0003 | **all met** | ✓ 1.680.000 ₫ · 0 cut · 0 declined | ✓ 4/4 · 0 pending | ✓ | ✓ |

⇒ Demo có cặp đối chứng: **19 qua cổng, 17 không qua** — đều tính từ dữ liệu.

### Phát hiện còn mở (chưa sửa)

1. **`ChainRun.TotalPayable / TotalCovered / TotalCustomerBorne` không bao giờ được ghi** (`RunAdjudicationChain`: `CreateCR` chỉ set ClaimId/RunNo/InputsHash/Version/RunAt/TriggeredBy; `UpdateCR` chỉ set StepOutputsJson/DurationMs). Đây là gốc của **F-CH-1** (card "RUN #2 (AFTER CONFIRMATION) 0 ₫"). Sửa gốc: `UpdateCR` ghi TotalPayable = kết quả bước 14. Hồ sơ 19 có 1.680.000 vì run của nó được seed thẳng.
2. **C3 / TermCheckpoint:** hồ sơ 17 có **0 dòng** TermCheckpoint, trong khi chi tiết bước B04 của run #2 ghi "TERM-HC-003 — checkpoint dòng 6 (TBC-03 chưa chốt)". Cổng đúng theo entity; dữ liệu entity và text B04 lệch nhau. Checkpoint queue hiện 1 mục nhưng đó là **khuyến nghị NOTREL** (Recommendation), không phải TermCheckpoint.
3. **Ngưỡng 2.000.000** vẫn là literal — chưa có Setting/entity; màn Administration vẫn là chữ tĩnh.
4. Hồ sơ 20 (CLM-2026-0004, Victoria, 15/01/2026) là hồ sơ thử của cùng người được BH — không trùng với 17 (khác cơ sở) ⇒ C4 = 0 là đúng.

## 8. Rev 204 — sửa gốc F-CH-1 (01/10)

`RunAdjudicationChain`, node Assign giữa `GetCRForUpdate` và `UpdateCR` thêm:
- `NewCR.TotalPayable = CallS14.ResultAmount`
- `NewCR.TotalCovered = RunCtx.CoveredTotal`
- `NewCR.TotalCustomerBorne = RunCtx.TotalCustomerBorne`
- `TotalRequested` chưa gán (không có sẵn trong action).

Node Assign trước `CreateCR` không đổi (đã kiểm). Publish rev 204 thành công, 7 lỗi/135 cảnh báo như cũ.
**Chỉ có tác dụng cho lần chạy chuỗi mới.** Các ChainRun cũ vẫn 0 ⇒ card "RUN #2 (AFTER CONFIRMATION) 0 ₫" của hồ sơ 17 chỉ đổi sau khi chạy lại chuỗi (ghi dữ liệu demo — cần Mike cho phép). Cổng không phụ thuộc vì đã đọc CalcSnapshot (rev 203).

## 9. Rev 205–206 (01/10, chiều)

| Rev | Thay đổi | Kiểm trên màn |
|---|---|---|
| 205 | App setting **`AutoApprovalThreshold`** (Decimal, mặc định 2.000.000). `EvaluateAutoApprovalGate` đọc setting thay literal. Server action `GetAutoApprovalThreshold` + data action trên Administration. Bỏ dòng giả "Since 01/08/2026" và "Lê Minh Tuấn · 28/07/2026 · 2 administrators confirmed". | Administration: "Current 2.000.000 ₫ · Set by app setting AutoApprovalThreshold in ODC Portal · no change history in MVP 1"; tab Thresholds đọc setting. ClaimDetail footer: "Threshold 2.000.000 — app setting AutoApprovalThreshold, changed in ODC Portal." |
| 206 | **Data check trung thực** — không xoá dữ liệu nào. | "Claims WITHOUT loss type (expect 0)" = 0 ✓ · Schedule HC-STD 51 rows / 3 policies ✓ · HC-PLS 17/1 ✓ · HC-PRM 34/2 ✓ |

**Kết luận việc "dọn trùng" (mục 3):** không có bản ghi trùng. Các số đỏ cũ là vì kỳ vọng viết khi DB có 1 policy/plan và 12 claim; sau đó thêm fixture (FX-STD-2026, FX-STD-UNPAID, FX-PRM-2026 và các claim TST-*). Xoá theo các số cũ sẽ **phá fixture test**.

Còn đỏ:
- **Legacy/empty cost-line codes = 121**: `ClaimLine.BenefitCode` ∈ {INPATIENT_ROOM, INPATIENT_SURGERY, INPATIENT_OTHER, OUTPATIENT, DENTAL, ""} — mã prototype cũ hoặc rỗng. Chưa đụng (cần biết chuỗi có đọc cột này không trước khi migrate).
- **Policies without PremiumPaid (excl. FX-) = 1**: bộ lọc loại FX- của Mentor chưa ăn (nhiều khả năng sai thuộc tính). Policy chưa đóng phí dự kiến là fixture FX-STD-UNPAID (cho T-CH01-c). Chưa xác minh.

**Chạy lại chuỗi hồ sơ 17 (mục 1):** hệ thống chặn Claude bấm "Run chain (writes)" (ghi dữ liệu demo). Mike tự bấm: Administration → Chain dry-run → Claim Id 17 → Run chain (writes). Mốc trước khi chạy: ChainRun hồ sơ 17 = 2 run; limit ledger 200 Used 2.000.000 / Left 18.000.000.

**T-GATE-01..07** trên Chain Tests vẫn viết theo G1–G4 cũ (T-GATE-04 = confidence 0,80; T-GATE-05 = BLOCKED) — trái phương án A. Cần viết lại theo C1–C4.

## 10. Rev 207 — T-GATE theo C1–C4 (01/10, tối)

- Action `UpdateGateTests` (viết lại 7 dòng T-GATE), `RunGateTests` (gọi `EvaluateAutoApprovalGate`, so mẫu P/F `C1 C2 C3 C4 AllPass` + chuỗi trong lý do). `Seed_ChainTests` gọi `UpdateGateTests` ở cuối. Màn Chain tests thêm 2 nút + bộ đếm gate. Lỗi 7 (cũ), cảnh báo 137 (+2 kiểu Long Integer ở badge, như các badge cũ).
- Đặc tả: `claude/build-04-bo-sung-v1-1-cong-c1-c4.md`. File nguồn `chain-tests-71.json` đã đồng bộ (bản cũ `.bak-before-cgate`). `approval-gate-4-conditions/tasks.md`: tick 1b, 3, 4, 6.
- **Chờ Mike bấm** (Claude bị chặn ghi dữ liệu): Chain tests → *Update T-GATE rows (C1–C4)* → *Run gate tests*. Kỳ vọng 4/4 gate pass (01, 02, 04, 05); 03 PENDING (thiếu fixture checkpoint OPEN); 06, 07 MANUAL.

## 11. Kết quả chạy thật (01/10, rev 208)

- Mike bấm "Run chain (writes)" hồ sơ 17 → **run #3** (ChainRun 232, 14 snapshot, TriggeredBy admin): payable 2.000.000, card "RUN #3 … 2.000.000 ₫" (hết 0 ₫ — F-CH-1 đóng trên dữ liệu). Hạn mức 200 vẫn Used 2.000.000 / Left 18.000.000 — **không cộng đôi**.
- Rev 208: sửa `UpdateGateTests` (gán lệch cột Input/Expected/Catch) và `RunGateTests` (exception làm rollback im lặng → nay ghi ERROR lên dòng).
- Mike bấm Update + Run gate tests → **4/4 gate pass**:

| Test | Hồ sơ | Kỳ vọng | Thực tế |
|---|---|---|---|
| T-GATE-01 | CLM-2026-0003 | PPPPP | ✓ PPPPP |
| T-GATE-02 | TST-T-CHAIN-01 | F???F | ✓ FFPFF |
| T-GATE-04 | CLM-2026-0001 | ?F??F | ✓ FFPPF |
| T-GATE-05 | TST-T-CHAIN-01 | ???FF | ✓ FFPFF |

- Còn: T-GATE-03 PENDING (thiếu fixture checkpoint OPEN); 06/07 MANUAL. Bộ đếm "x/46 pass" đang cộng cả dòng GATE (4/46) — chuỗi 46 test chưa chạy lại kể từ re-seed.
- Phát hiện phụ: nhãn card "RUN #n (AFTER CONFIRMATION)" gán cho mọi run ≥ 2 dù chưa ai xác nhận; màn Limit ledger hiện khách Nguyễn Văn An đã dùng 19.000.000 (kỳ 04/2026–03/2027, không gắn hồ sơ) — nghi dữ liệu fixture T-CH12-a.

## 12. Rev 209 — hai card run nói thật (01/10)

Card 1 = run mới nhất + ai chạy + payable từ CalcSnapshot bước cuối; card 2 = run trước + chênh lệch. Bỏ chữ cứng "after confirmation · line excluded · re-capped · co-pay recomputed". Hồ sơ 17 sau hard reload: "RUN #3 · ADMIN · LATEST 2.000.000 ₫" / "RUN #2 · SEED · PREVIOUS 2.000.000 ₫ · same payable as the latest run".

## 13. 19.000.000 trên Limit ledger — ĐÍNH CHÍNH (01/10)

**Kết luận trước đó SAI, không có gì phải sửa.** Lần đọc đầu, phím End+Enter trên dropdown đã chọn nhầm *Fixture Member 6* chứ không phải Nguyễn Văn An. Kiểm lại bằng chọn đúng option:
- **Nguyễn Văn An**: Consumption records = CLM-2026-0001 · 200 · 2.000.000 · Provisional; 200.1 · 1.200.000; 200.2 · 800.000. **Khớp màn hồ sơ 17.** Bộ đếm "used" = 0 vì chỉ tính FINAL (provisional chưa chốt) — đúng BR-LEDGER-1.
- **19.000.000** là dòng fixture của test T-CH12-a: `Seed_ChainTestFixtures`, node `assignNode24` → `MemberIdM6` (Fixture Member 6), `PolicyIdStd` (FX-STD-2026), BenefitCode 200, PeriodYear 2026, UsedAmount 19000000, RemainingAmount 1000000. Đúng chủ đích, không đụng khách demo.
- Phiên Mentor định sửa `Seed_LimitLedger` (dựa trên câu trả lời sai của Mentor) đã **huỷ, không publish** — rev vẫn 209.
- Còn một điểm nhỏ chưa rõ: nhãn kỳ trên màn Limit ledger cho Nguyễn Văn An ghi 01/04/2026–31/03/2027, trong khi chuỗi ghi hợp đồng 01/01–31/12/2026.

## 14. Rev 210 — header Policies + nhãn kỳ Limit ledger theo đúng lựa chọn (01/10)

**Lỗi:** màn Policies luôn hiện hợp đồng đầu tiên ở header dù chọn hợp đồng khác; nhãn kỳ trên Limit ledger lấy `GetMembersAll.List.Current` (= thành viên đầu danh sách) nên Nguyễn Văn An bị hiện kỳ của Fixture Member 6 (01/04/2026–31/03/2027). Đây là nguồn gốc điểm "chưa rõ" ở §13.

**Sửa (1 phiên Mentor, publish rev 210):**
- Policies: thêm aggregate `GetSelectedPolicy` (Policy ⋈ PolicyStatus, Policy.Id = SelectedPolicyId, max 1); header DdPolicy / DdHolder / DdProduct / phí bind vào đó; refresh trong RefreshSnapshot và OnAfterFetchPolicies.
- LimitLedgerView: thêm `GetSelectedMemberPolicy`; nhãn kỳ bind vào đó thay vì `GetMembersAll.List.Current`.

**Kiểm trên màn (chọn bằng form_input, xác nhận option đã chọn):**
| Màn | Lựa chọn | Kết quả |
|---|---|---|
| Policies | HC-2026-0001 · Nguyễn Văn An | Policy HC-2026-0001 · HC-STD · Holder Nguyễn Văn An · HC-STD v4 · quyền lợi 200 = 20.000.000, 20% ✓ |
| Limit ledger | Nguyễn Văn An | period **01/01/2026 – 31/12/2026** ✓ (khớp chuỗi); 3 dòng provisional CLM-2026-0001 (2.000.000 / 1.200.000 / 800.000) |
| Limit ledger | Fixture Member 6 → 7 | period 01/04/2026 – 31/03/2027 (hợp đồng fixture FX-STD-2026); nhãn đổi theo lựa chọn ✓ |

→ Điểm "chưa rõ" ở §13 đã đóng.

## 15. Rev 211–212 — hai bộ đếm sai + phát hiện lỗ hổng C3 (01/10)

**Rev 211 — ChainTests:** bộ đếm hiện "50/46 pass" vì `GetCntPass` (lọc `Pass and LastRunAt > #1900-01-01#`) đếm cả 4 dòng T-GATE. Thêm `and ChainTest.Status <> "GATE"` (Mentor đề xuất `Index(TestId,"T-GATE") <> 1` — sai vì Index trả vị trí 0-based; đã sửa lại). `GetCntFail` vốn lọc `Status = "RUNNABLE"` nên không đổi. Sau hard reload: **46/46 pass · 0 fail · 18 chờ bước · 4 ngoài chuỗi · 4/4 gate**. → 46 test chuỗi đều pass sau re-seed (lần chạy 01/10 12:50).

**Rev 212 — Work list:** header "0 claims · all handlers" trong khi bảng có 54 hồ sơ. `CountExpr` dùng `TotalAll` từ `WorkList_StatusCounts`, vốn chỉ đếm hồ sơ giao cho user hiện tại. Đổi sang `GetClaims.Count + " claims · all handlers"`. Sau hard reload: **54 claims · all handlers** ✓.

**Phát hiện (chưa sửa — cần quyết định):**
- **Không có chỗ nào ghi bảng `TermCheckpoint`.** Bước 4 (`A5Step04_GeneralExclusions` → `A5Step04_EvalTerm`) chỉ đánh dấu dòng Held trong `ChainCtx` (in-memory: `Checkpoints`, `CheckpointCount`, `HoldTermCodes`/`HoldReason`), không gọi `CreateTermCheckpoint`. Hệ quả: **C3 luôn Đạt** — một hồ sơ có dòng bị giữ ở bước 4 (vd. SUPPLEMENT → TERM-HC-003) vẫn có thể qua cổng nếu C1/C2/C4 đạt. Trái BR-TERM-13.
- **Màn Checkpoint queue** đọc `Recommendation` (khuyến nghị lâm sàng chờ quyết), không đọc `TermCheckpoint`. Item duy nhất hiện tại là NOTREL Atorvastatin trên CLM-2026-0001 — đúng là việc chờ người, nhưng tiêu đề màn nói về "term checkpoint".
- **T-GATE-03** để PENDING vì `FixtureClaimNo = ""` và `Status = "PENDING"` (RunGateTests chỉ lấy `Status = "GATE"`). Không thể có fixture chừng nào chưa có nơi ghi TermCheckpoint.
- Chuỗi test (`TST-*`) chạy in-memory, không lưu ChainRun cho hồ sơ TST (vd. TST-T-CH04-a hiện "No chain run yet").

## 16. Rev 213–214 — C3 bắt cả bước bị giữ (01/10)

**Quyết định của Mike:** sửa phía cổng, không đụng chuỗi.

**Rev 213 — `EvaluateAutoApprovalGate`, chỉ C3:** thêm aggregate `GetHoldSteps` (CalcSnapshot, lọc `ChainRunId = <run mới nhất> and Outcome = "HOLD"`, cột `COUNT(Id)` alias `HoldCount`), đọc bằng `If(GetHoldSteps.List.Empty, 0, LongIntegerToInteger(GetHoldSteps.List.Current.HoldCount))`. C3 Đạt khi 0 TermCheckpoint OPEN **và** 0 bước HOLD. Lý do: "<n> step(s) on hold in run #<số> — a person must clear them".

**Kiểm trên màn (hard reload):**
| Hồ sơ | Trước | Sau |
|---|---|---|
| CLM-2026-0001 (17) | C3 ✓ "No open checkpoint (0 …)" — **sai**, B04 ghi "1 checkpoint" | C3 ✕ **"1 step(s) on hold in run #3 — a person must clear them"** |
| CLM-2026-0003 (19) | all met | vẫn **all met**; C3 "No open checkpoint (0 checkpoint(s) on this claim, all decided) · 0 step(s) on hold" |

**Rev 214 — `UpdateGateTests`, chỉ dòng T-GATE-03:** FixtureClaimNo = CLM-2026-0001, Status = GATE, mẫu ??F?F, log∋"step(s) on hold". `chain-tests-71.json` đồng bộ (diff đúng 6 trường; backup `.bak-before-gate03`). Kỳ vọng sau khi Mike bấm *Update T-GATE rows* → *Run gate tests*: T-GATE-03 = FFFPF (khớp ??F?F); T-GATE-04 cùng hồ sơ thành FFFPF (vẫn khớp ?F??F).

**Còn lại:** Checkpoint queue đọc Recommendation; chuỗi chưa ghi TermCheckpoint (sau hackathon). Bộ đếm "x/4 gate" có thể đang cố định mẫu số 4 — kiểm sau khi chạy.
