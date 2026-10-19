# Hai câu hỏi đang nợ — soạn 21/09, mở từ 16/09 (5 ngày trễ)

> Tôi không có kênh gửi mail/chat từ phiên này. Hai bản dưới đây dán được nguyên văn; PO gửi và ghi lại ngày gửi ở cuối file. Cả hai đều **gửi miễn phí, trả lời chậm** — gửi trước khi mở chuỗi.

---

## Q1 · Đội BnK IDP — `Q-IDP-01` crosswalk mã loại chứng từ (A4 §3.1, C3)

**Chủ đề:** [ClaimMVP] Q-IDP-01 — bảng đối chiếu `sub_document_type` (CL*) ↔ mã loại chứng từ T1/T2 của Claim Platform

Chào team IDP,

Bên Claim Platform đang chạy phân loại chứng từ trên **ánh xạ giả định** và đây là điểm duy nhất A4 xếp vào loại "chặn chứ không làm suy giảm" — đoán sai một dòng là chứng từ bị xếp vào loại nó không phải và checklist coi như đã đủ. Nhờ team xác nhận hoặc điền bảng dưới, đúng 10 mã `CL*` hiện có trong `GET /mbal/template-map` (tenant 176, partnerId 40).

| `sub_document_type` (CL*) | Tên template trên IDP | Mã Claim Platform (A4 §1) | Ghi chú |
|---|---|---|---|
| CL05 | Giấy ra viện / Hospital discharge papers | *(ngoài bộ MVP 1 — T1-05 Phase 2?)* | |
| CL06 | ? | ? | đã map thử vào bộ `HEALTH_CLAIM_V1` (id 195) |
| CL… | | | |

Bộ mã của chúng tôi (8 loại): `T1-01` Bảng kê chi phí KCB · `T1-02` Đơn thuốc · `T1-03` Phiếu khám bệnh (có chẩn đoán) · `T1-04` Hoá đơn GTGT (XML) · `T2-01` Giấy tờ tuỳ thân · `T2-02` Kết quả cận lâm sàng · `T2-03` Báo cáo y tế / Giấy chứng nhận phẫu thuật · `T2-04` Biên bản/xác nhận hoàn cảnh sự cố.

Ba câu đi kèm, trả lời ngắn là đủ:
1. Mã `CL*` nào ↔ `T1-nn`/`T2-nn` nào (bảng trên). Mã `CL*` không có đối ứng thì ghi "không dùng".
2. `T1-03` (phiếu khám bệnh) hiện **chưa có template ở tenant 176** dù là `REQUIRED` trên 4/8 loại tổn thất — team có template sẵn để nạp không, hay cần chúng tôi gửi mẫu?
3. Khi team thêm/đổi tên mã `CL*`, có kênh báo nào không? (Nếu không, adapter của chúng tôi sẽ đọc `template-map` định kỳ và cảnh báo khi thấy mã lạ — chỉ cần team biết.)

Cần trước **26/09** để kịp demo 07–08/10. Cảm ơn team.

---

## Q2 · OutSystems SA — một câu duy nhất

**Subject:** [Agent Workbench] Can an agent call another agent?

Hi —

One question on Agent Workbench (ODC), for a hackathon build due 7–8 Oct:

**Can an agent invoke another agent as a tool — or is agent-to-agent orchestration expected to live outside the agents (e.g. an Orchestrator implemented as a Server Action / BPT that calls each agent in turn)?**

Context: we have six agents (AG-0 orchestrator, AG-1/2 extraction, AG-3 clinical, AG-4 completeness, AG-5 fraud, AG-6 duplicate). If agents cannot call agents, we will drop AG-0 and run the orchestration as a deterministic Server Action — which we are fine with, we just need to know which way to build.

If we hear nothing by **26 Sep** we will default to "Orchestrator = Server Action, no AG-0".

Thanks,

---

## Nhật ký gửi

| Câu | Kênh | Ngày gửi | Trả lời | Quyết định mặc định nếu im |
|---|---|---|---|---|
| Q1 IDP | | | | phân loại chạy trên ánh xạ giả định — **vẫn chặn** phần phân loại |
| Q2 SA | PO | ≤ 21/09 | **21/09 — phương án 3, một agent** → `changes/active/agent-architecture/` | *(không cần)* |
| Q2′ SA — input phẳng? description ảnh hưởng thế nào? | PO | 21/09 | **21/09 — cả ba kiểu đều được; List nên tránh; description là tín hiệu chính** → `agent-architecture/specs/agents/spec.md` §Q2 | *(không cần)* |
| Q3′ SA — trần thời gian lượt agent? IDP > 60 s thì sao? | PO | 21/09 | **21/09 — 10 s mặc định, tối đa 60 s (`ServerRequestTimeout`); IDP chạy ngoài lượt agent** → cùng file §Q3 | *(không cần)* |

> **Số hiệu AG-n trong Q2 ở trên (AG-4 completeness · AG-5 fraud · AG-6 duplicate) là văn bản đã gửi — giữ nguyên, nhưng sai.** Chuẩn là **`build-01` §4.27 (16/09)**: AG-0 Orchestrator (Server Action) · AG-1 Intake · AG-2 Adjudication · AG-3 Clinical · AG-4 Fraud-signal · AG-5 Customer-communication. Lệch đến từ `plan-5-8-9-truoc-hackathon` (04/09, lịch sử từ 20/09). Đóng 21/09 (PO). Sáu câu còn lại của `build-05` §9 chưa gửi, không chặn code.
