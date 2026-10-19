# Change — AG-2 Adjudication trên Agent Workbench (sàn của bản dự thi)

**Mở:** 01/10/2026 · **PO:** Mike · **Lý do làm bây giờ:** IDP live còn timeout (IT hẹn sửa 02/10) → chuyển sang AG-2.
**Chuẩn:** `build-05` §3.3 (prompt AG-2), §6.3 (sàn) · `agent-architecture/specs/agents/spec.md` (1 agent, chuỗi là MỘT tool, IDP không chạy trong lượt agent, trần 60 s).

## Quyết định PO 01/10
- Model connection: **TrialGPT5**.
- Asset mới: **CLM_AdjudicationAgent** (không dùng lại `AG5_Adjudication` / `AgenticHealthClaimAdjudicationAgent`).
- Điểm gọi: **nút "Tính và giải thích" + ô chat hỏi tiếp** trên ClaimDetail.

## Thiết kế
- ClaimMVP lộ **Service Action** cho agent (tham số phẳng, description viết cho agent, câu cấm tự tính):
  `Agent_RunAdjudicationChain` · `Agent_GetClaimContext` · `Agent_GetChainExplanation`. Mỗi tool tự ghi **một dòng AgentRun** (AgentName "AG-2 Adjudication", ToolName đúng tên, Replay theo Settings) — trace chỉ ghi cái đã thật chạy.
- Agent app `CLM_AdjudicationAgent` (TrialGPT5): system prompt theo `build-05` §3.3 rút gọn cho 3 tool có thật; lộ Service Action `AskAdjudicationAgent(ClaimId, Question, …)` → câu trả lời.
- ClaimMVP ClaimDetail: panel AG-2 (nút + chat), ServerRequestTimeout 60 s trên lời gọi.
- **Không tool nào ghi tiền ngoài chuỗi**; không khai `A5StepNN_*`, `A5Chain_*`, `RoundVnd`.

## Việc
- [x] 1. Đọc model (ChainStepResult, WriteAgentRun, CalcSnapshot/ChainRun, timeout, service action có sẵn)
- [x] 2. (rev 223) ClaimMVP: 3 service action cho agent + ghi AgentRun mỗi lần gọi
- [x] 3. (CLM_AdjudicationAgent f012b159…, rev 1–2) Tạo agent app `CLM_AdjudicationAgent`, nối TrialGPT5, prompt, 3 tool, service action `AskAdjudicationAgent`
- [x] 4. (rev 224) ClaimDetail: panel AG-2 (nút + chat)
- [ ] 5. Kiểm sàn: chạy trên hồ sơ demo → đúng số tiền; chạy lại → cùng số; AgentRun có `ToolName = RunAdjudicationChain`, `Replay = False`
- [ ] 6. Kiểm cờ Replay: PO bật ReplayMode → chạy lại → `Replay = True` → PO tắt
- [ ] 7. Ảnh chụp tab Tools của agent (chỉ 3 tool) → `output/`

## Kết quả 01/10 tối (đã kiểm trên màn)
- CLM-2026-0003 (ClaimId 19), nút "Tính và giải thích": agent gọi GetClaimContext → RunAdjudicationChain, in đủ 14 bước + "Tổng chi trả: 5.400.000 ₫" + Summary (EN). Run #2 (ChainRunId 265).
- Bấm lại: run #3 (ChainRunId 266) → **cùng 5.400.000 ₫** ✅ (InputsHash: chưa đối chiếu).
- AgentRun: 1354 GetClaimContext · 1355 RunAdjudicationChain · 1356 Agent answer — AgentName "AG-2 Adjudication", **Replay = False** ✅.
- Thời gian một lượt ~45 s (18:26:43 → 18:27:28) — sát trần 60 s ⚠.
- "Unavailable feature" ×2 trong agent app (Standalone Agent) không chặn publish.

## ⚠ Phát hiện cần PO quyết
1. **CLM-2026-0003 đổi số: 1.680.000 → 5.400.000.** Run #1 (1.680.000) là **seed**, không do chuỗi tạo. Chuỗi thật: bước 8 dùng trần 4.000.000 cho 100.1 (bảng quyền lợi trên màn ghi Per visit 500.000), bước 10 đồng chi trả 0 ₫ (bảng ghi 20 %), bước 9 HOLD (thiếu số ngày điều trị theo dòng). ⇒ T-GATE-01 (0003 sạch, PPPPP) sẽ trượt khi chạy lại; hồ sơ "sạch tự duyệt" cho demo cần chọn lại hoặc sửa dữ liệu/chuỗi. Cần đối chiếu nguồn trần/đồng chi trả (PolicyBenefitSnapshot vs bảng hiển thị).
2. Trace trên ClaimDetail gắn nhãn dòng AG-2 là "Document reading" — sai nhãn, cần map AgentName "AG-2 Adjudication".
3. Seed AgentRun cũ còn tên AG-0/AG-1/AG-3…AG-6 (việc 9 của agent-architecture).

## Còn lại
- [x] Kiểm hỏi tiếp (memory): "Vì sao bước 9 bị HOLD?" → "No data written." + trích nguyên văn log bước 9, không chạy lại chuỗi ✅
- [ ] Kiểm cờ Replay = True (PO bật ReplayMode)
- [x] (rev 228) Nhãn trace: "AG-2 Adjudication agent · Tool call · <ToolName>" / "Answer to assessor" (còn lặp tên tool ở dòng phụ — cosmetic)
- [ ] Giảm thời gian lượt (<30 s)
- [ ] Ảnh chụp tab Tools (3 tool) → output/

## 02/10 (rạng sáng) — rev 225–228
- rev 225–227: Administration **"Dry-run all claims (no writes)"** (A5Chain_DryRun bước 1–13, không ghi DB). Nút chỉ chạy chắc khi bấm bằng JS `click()`.
  Kết quả: **không hồ sơ demo nào "sạch & ≤ 2.000.000"**: 0001 2.000.000 (HOLD 1, CUT 2) · 0002 2.000.000 (HOLD 1) · 0003 5.400.000 (HOLD 1) · 0004 claimed 800.000 → payable 0 · 0034–0041 không có dòng chi phí.
  Fixture test sạch: TST-T-CH04-c (tai nạn ngoại trú 500.000 → 500.000) · TST-T-CH05-b (nội trú 300.000 → 300.000) — nhưng là fixture chuỗi, chưa có assessment (C2) .
- rev 228: bảng quyền lợi ClaimDetail đọc PolicyBenefitSnapshot (trước: hard-code 500.000 / 20 %). 0003 giờ hiện 4.000.000 / 80.000.000, đồng chi trả —.
  ⚠ Còn lệch: "Total Claimed 2.460.000" trong khi dòng chi phí cộng 5.400.000 (Claim.TotalClaimed cũ); cột Payable từng dòng hiện "—".

## 02/10 — PO chốt 3 quyết định (rev 229–231)
PO: (a) loại bước 10–11 khỏi C1 · (b) tạo hồ sơ demo mới cho ca tự duyệt · (c) giữ CLM-2026-0003 HOLD làm ca "cần người".
- rev 229: `EvaluateAutoApprovalGate` — aggregate đếm cắt: `CalcSnapshot.ChainRunId = ChainRunId and ((Outcome = "CUT" and StepNo <> 10 and StepNo <> 11) or ReasonCode = "WAIT")`. C1Pass không đổi (`Payable <= Threshold and CutCount = 0 and DeclinedCount = 0`).
- rev 230–231: C1Reason = `FormatVND(Payable) + " vs threshold " + FormatVND(Threshold) + " · " + n + " limit cut(s) in run #" + RunNo + " (co-pay and deduction ratio, steps 10-11, not counted) · " + d + " declined"`.
  Kiểm trên màn hồ sơ 17 (0001): "1 limit cut(s) in run #3" (trước: 2 — bước 8 + bước 10) ✅.
- rev 230: server action `Seed_DemoClaim0005` (idempotent: có rồi thì không đổi gì) + nút `CreateDemo0005Btn` trên Administration.
  CLM-2026-0005: member/policy/RuleSetRef/ConditionClass chép từ CLM-2026-0002 (Trần Thị Bích, HC-2026-0002) · IllnessOutpatient/Outpatient · 22/09/2026 · J06.9 · "Phòng khám Đa khoa Hoàn Mỹ Sài Gòn" · 3 dòng CONSULT 300.000 / LAB 450.000 / DRUG 650.000 = 1.400.000 · Assessment COMPLETE 3/3, 0 Recommendation.
- [x] Tạo: "CLM-2026-0005 created (Id 430), 3 lines, 1.400.000". Dry-run: payable 1.120.000 · HOLD 0 · CUT 1 (bước 10) · declined 0.
- [x] AG-2 trên 430: run #1 (ChainRunId ?) rồi run #3 (ChainRunId 300) → **1.120.000 ₫ cả hai lần** ✅. Cổng: **all met** — C1 "1.120.000 ₫ vs threshold 2.000.000 ₫ · 0 limit cut(s) in run #1 … · 0 declined"; C2 COMPLETE 3/3, 0 pending; C3 0 hold; C4 0 trùng.
- [x] (rev 232) UpdateGateTests: T-GATE-01 → CLM-2026-0005 (PPPPP, log "1.120.000 ₫ vs threshold 2.000.000 ₫||0 limit cut(s)"), T-GATE-02/03/06 cập nhật. Bấm Update T-GATE rows + Run gate tests: **01–05 đều ✓**. `chain-tests-71.json` cập nhật (bản trước: `.bak-before-demo0005`).

### Thời gian lượt AG-2 (02/10)
- Phát hiện: lời gọi từ màn tối đa **60 s** (nền tảng chặn — 160 s bị từ chối). Quá 60 s → client nhận 504 và **nền tảng hủy luôn request phía server, kể cả lời gọi agent** (log OS-BERT-00000 "The operation was canceled" ở ClaimMVP.AskAG2 và CLM_AdjudicationAgent.AskAdjudicationAgent). Agent không chạy tiếp ngầm; không có dòng "Agent answer".
- Lượt đầu trên 430 (21:16): 3 tool chạy xong (+28/+38/+41 s), câu trả lời bị hủy. Chuỗi vẫn đã ghi run #1.
- Agent app rev 3: không gọi GetChainExplanation ngay sau khi chạy chuỗi; mỗi tool 1 lần. Rev 4: định dạng trả lời ngắn — chỉ các bước khác PASS/SKIP, ≤ 120 từ.
- Kết quả rev 4: **~43 s** (click 21:34:48 → answer 21:35:31), tool GetClaimContext +8 s, RunAdjudicationChain +18 s. Câu trả lời: bước 10 CUT 280.000, bước 14 POSTED 1.120.000, "Các bước khác: PASS/SKIP", Tổng 1.120.000 ₫.
- ClaimMVP (chờ publish cùng lượt sau): AskAG2 có exception handler ghi AgentRun "Agent answer" Error; timeout service call 58 s; màn báo "AG-2 không trả lời kịp trong 60 giây nên lượt này đã bị hủy…".
- ⚠ Biên chỉ ~17 s. Nếu ngày pitch LLM chậm → hủy. Hướng chắc chắn: chạy agent nền (Timer đọc hàng đợi + màn polling) — chưa làm, chờ PO.

### Phát hiện hiển thị
- Bảng quyền lợi ClaimDetail luôn 2 dòng INPATIENT_ROOM / INPATIENT_OTHER kể cả hồ sơ ngoại trú (0001, 0005); cột Payable từng dòng "—". Đang điều tra nguồn dữ liệu đúng (mã benefit sau bước 5, posting bước 14).
- Câu trả lời AG-2 in số tiền thô "1120000 ₫" (không dấu chấm) — số đúng tool trả về.
