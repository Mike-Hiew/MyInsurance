# Change — Cổng tự duyệt: **4 điều kiện** (PO chốt 21/09)

> Thư mục này từng tên `approval-gate-3-conditions`. Đổi tên 21/09 vì tên cũ khẳng định một điều sai.

**Trạng thái:** 🟡 PO chốt **4 điều kiện** (21/09, Mike) — còn chặn ở "4 nào" · **Hạn:** 22/09 · **Chặn:** màn Approval + bước cuối của chuỗi
**Spec liên quan:** `specs/adjudication/spec.md` · delta ở `./specs/adjudication/spec.md`

## Vấn đề

`build-00` §4 #7 (bản cũ) ghi **3 điều kiện** và nói BRD phải sửa. **PO chốt 21/09: sai — chuẩn là 4, BRD đúng**, `build-00` đã sửa tại nguồn. `build-04` §3.1 ghi đúng từ đầu: A5 §6.4 có 3, BRD §9.1.3 thêm G4 fraud, bộ phải hiện thực hoá là hợp nhất **4**.
Màn Approval trên ClaimMVP đang dựng theo BRD — audit rev 47 ghi: *"condition 1 ✕ (co-pay cut), condition 2 '1 proposal awaiting a human' ✕, 3–4 ✓"*.

Đây là một delta đã tồn tại trong code suốt ~90 rev mà **không nằm ở đâu cả**. Chính là lý do thư mục `changes/` này được lập.

## Việc

- [x] **1. Hỏi PO/BA** — trả lời 21/09: **4 điều kiện**, BRD đúng, `build-00` §4 #7 sai
- [x] **1b. (xong 01/10 — PO chốt phương án A: C1 tiền & không cắt · C2 lâm sàng xong, không khuyến nghị chờ · C3 không checkpoint mở · C4 không trùng/gian lận AG-4; confidence không thuộc cổng)** CHẶN: xác định 4 điều kiện nào. Màn Approval (audit rev 47) và bảng G1–G4 trong doc là hai bộ khác nhau, cùng đếm ra 4. Đọc `EvaluateAutoApprovalGate` trên tenant, liệt kê điều kiện như code tính, chốt một danh sách duy nhất
- [ ] **2. Ghi câu trả lời vào `./specs/adjudication/spec.md`** — đúng đoạn khác biệt, không chép cả spec
- [x] **3. (xong rev 200–205, 01/10: action mới, đếm đúng rev 202, payable từ CalcSnapshot rev 203, ngưỡng = app setting AutoApprovalThreshold rev 205)** Sửa `EvaluateAutoApprovalGate` (Server Action) theo con số đã chốt
- [x] **4. (xong rev 201: gateCard đọc GetGate, nhãn AG-4, footer nói thật về ngưỡng)** Sửa bảng cổng trên tab Approval của `ClaimDetail`
- [ ] **5. Sửa `build-00` §4 #7 tại nguồn** (KHÔNG sửa BRD — BRD đúng): "cổng tự duyệt 3 điều kiện" → **4**, ghi ngày 21/09 và người quyết
- [ ] **5b. Soạn 2 câu A2 đã duyệt** cho `COPAY` và `CAP_VISIT`, nhãn "soạn cho hackathon"; `DEDUCT`/`LIMIT_TOTAL` đẩy batch F
- [x] **6. (kiểm 01/10 trên màn: hồ sơ 19 all met; hồ sơ 17 C1 ✕ 2.000.000 · 2 cut, C2 ✕ 1 pending, C3 ✓, C4 ✓ — mọi lý do từ dữ liệu)** Kiểm: hồ sơ B đi qua cổng ra đúng kết quả; mỗi điều kiện Đạt/Không đạt **kèm lý do cụ thể**, không chỉ ✓/✕
- [ ] **7. Merge delta vào Project doc `build-04`** rồi `mv` change này sang `../../archive/`

## Xong nghĩa là

- Màn Approval và `EvaluateAutoApprovalGate` cùng một con số
- Con số đó có một nguồn duy nhất, viết ra, ký bởi PO
- BRD không còn nói điều ngược lại


## Cập nhật 01/10/2026

- Đặc tả + test mới: Project doc `claude/build-04-bo-sung-v1-1-cong-c1-c4.md` (thay §3 và §9.3 của build-04) và `claude/cong-tu-duyet-ket-qua-rev200-203-2026-10-01.md`.
- T-GATE-01..07 viết lại theo C1–C4 (rev 207: `UpdateGateTests`, `RunGateTests`; `chain-tests-71.json` đã đồng bộ). Chờ Mike bấm *Update T-GATE rows* rồi *Run gate tests*.
- Còn mở: mục 2 (spec delta), 5, 5b; mục 7 chỉ archive sau khi 5 test GATE pass. T-GATE-03 thiếu fixture có TermCheckpoint OPEN.

- **01/10 tối (rev 208):** Mike chạy Update + Run gate tests → **4/4 gate pass** (T-GATE-01 PPPPP, 02 FFPFF, 04 FFPPF, 05 FFPFF). Hồ sơ 17 chạy lại chuỗi → run #3 payable 2.000.000, ledger không cộng đôi.
- **01/10 khuya (rev 211–214):**
  - Phát hiện: chuỗi KHÔNG ghi bảng `TermCheckpoint` (bước 4 chỉ giữ dòng trong `ChainCtx`) → C3 cũ luôn Đạt. Hồ sơ 17 thật ra có B04 "1 checkpoint" mà C3 vẫn ✓.
  - Mike chọn sửa phía cổng (rev 213): C3 Không đạt khi run mới nhất có CalcSnapshot `Outcome = "HOLD"` **hoặc** có TermCheckpoint OPEN. Hồ sơ 17 giờ: C3 ✕ "1 step(s) on hold in run #3 — a person must clear them"; hồ sơ 19 vẫn all met ("… · 0 step(s) on hold").
  - rev 214: T-GATE-03 có fixture = CLM-2026-0001, Status GATE, log∋"step(s) on hold" (`chain-tests-71.json` đã đồng bộ, backup `.bak-before-gate03`). Chờ Mike bấm *Update T-GATE rows* → *Run gate tests* → kỳ vọng 5/5 gate.
  - rev 211: bộ đếm ChainTests không cộng dòng GATE nữa → 46/46 pass. rev 212: Work list "54 claims · all handlers".
  - Còn mở: màn Checkpoint queue đọc `Recommendation`, không đọc TermCheckpoint; chuỗi chưa ghi TermCheckpoint (để sau hackathon).
