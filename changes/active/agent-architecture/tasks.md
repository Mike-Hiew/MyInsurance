# Change — Kiến trúc lớp agent sau khi SA trả lời

**Mở & chốt:** 21/09/2026 · **Trạng thái:** 🟢 quyết định xong · **21/09 chiều: PO chốt 5 agent → 5 tool + 4 ràng buộc bề mặt tool; áp ngay cho bước 6–14 đang viết**
**Spec bị đổi:** `specs/agents` — xem `./specs/agents/spec.md`

## Câu trả lời của SA OutSystems — 21/09

> Trong ODC, **một agent không thể gọi trực tiếp agent khác theo kiểu lồng nhau.** Ba lựa chọn:
> 1. **Agent gọi Server Action như tool.** Logic của agent thứ hai đặt trong Server Action thì agent thứ nhất gọi được.
> 2. Chuỗi tuần tự qua Service Action — gián tiếp và phức tạp hơn.
> 3. **Gộp logic vào cùng một agent** — đặt mọi bước vào Server Action riêng và để **một agent duy nhất điều phối** qua action handler.

Đây là câu hỏi ưu tiên số 1 trong `build-05` §9, mở từ 16/09. **Đóng 21/09.** Mặc định 26/09 (*Orchestrator = Server Action, bỏ AG-0*) **không cần dùng tới** — câu trả lời xác nhận chính hướng đó.

## Quyết định

**Chọn phương án 3.** Một agent duy nhất trên Agent Workbench.

| Thành phần | Hiện thực | Ghi chú |
|---|---|---|
| **AG-2 Adjudication** | **Agent thật** trên Agent Workbench | Tool list **đóng**: `RunAdjudicationChain` · các Server Action đọc ngữ cảnh · `WriteAgentRun`. **Không tool nào ghi trường tiền** |
| **AG-0 Orchestrator** | **Server Action**, không phải agent | Điều phối tất định |
| AG-1 · AG-3 · AG-4 · AG-5 | **Không build làm agent** — mỗi cái là **một tool** của AG-2 (bảng dưới) | Chỉ `RunAdjudicationChain` trong phạm vi trước pitch; bốn tool còn lại là *hình dạng để hướng tới* |

## Năm agent cũ = năm tool của một agent — PO 21/09

Năng lực không mất; nó đổi hình. Mỗi agent trong `build-05` trở thành **một Server Action mà AG-2 gọi như tool**. Chỉ dòng 2 nằm trong phạm vi trước pitch; bốn dòng còn lại là đích để **không thiết kế gì hôm nay chặn chúng ngày mai**.

| Agent cũ (`build-05`) | Tool của AG-2 | Phạm vi | Ghi chú |
|---|---|---|---|
| AG-1 Intake | `IntakeClaimDocuments` | Sau pitch | **Không gọi IDP trong lượt agent** (Q3, trần 60 s). Tool này chỉ đọc `RawExtractionJson` đã có; IDP chạy lúc upload |
| **AG-2 Adjudication** | **`RunAdjudicationChain`** | **Trước pitch** | Tool **duy nhất** được khai. 14 bước là ruột, không phải tool |
| AG-3 Clinical | `GetClinicalAssessment` | Sau pitch | Hôm nay: `Recommendation` là **seed** (xem mục trên) |
| AG-4 Fraud-signal | `EvaluateFraudSignals` | Sau pitch | |
| AG-5 Customer-communication | `DraftCustomerLetter` | Sau pitch | |

**Số hiệu AG-n — đóng 21/09 (PO):** chuẩn là **`build-01` §4.27 (16/09)**: AG-0 Orchestrator · AG-1 Intake · AG-2 Adjudication · AG-3 Clinical · AG-4 Fraud-signal · AG-5 Customer-communication. Chỗ lệch ("AG-5 Adjudication", "AG-0 Orchestrator" là agent) đến từ `plan-5-8-9-truoc-hackathon` (04/09) — tài liệu lịch sử từ 20/09, tenant seed từ thời đó (`AG5_Adjudication`). **Không phải hai chuẩn cạnh tranh; một cái cũ hơn.** Việc 9: đổi tên trong seed.

**Ràng buộc để bốn tool sau không bị chặn** (chi tiết ở `./specs/agents/spec.md` §Bề mặt tool): input phẳng · description viết cho agent · một action một việc, tên là động từ · **không bước nội bộ nào được khai tool**.

## ⚠ Điều tuyệt đối không được làm

> **Không ghi dòng `AgentRun` mang tên một agent không tồn tại.**

`build-01` §2.13 gọi `AgentRun` là *"vết audit… đây là thứ ban giám khảo sẽ xem"*. Ghi một dòng `Agent = AG-3 Clinical` trong khi AG-3 không được build là **bịa bằng chứng** — cùng loại lỗi với mọi phép đoán đã bị bác từ đầu dự án (ánh xạ `INPATIENT_OTHER → 100.3`, `LossType` chiếu từ `TreatmentType`, heuristic khớp token đầu của term).

Trace hiện **đúng cái đã chạy**:
- Bước do **agent** chạy → `Agent = AG-2`, `ToolCalled` = tên Server Action.
- Bước do **Server Action** chạy → đánh dấu là **hệ thống**, không phải agent.
- `HandoffTo` chỉ điền khi **có bàn giao thật**. Không có thì để null — **null nghĩa là không có, không phải chưa biết**.

## Khuyến nghị lâm sàng trên hồ sơ demo là DỮ LIỆU SEED

Đề xuất `NOTREL` trên dòng Atorvastatin của `CLM-2026-0001` được **seed**, không do AG-3 sinh ra (AG-3 không build). Phần **chạy thật** là: người xác nhận → chuỗi chạy lại → số đổi `2.000.000 → 1.880.000`.

**Phải nói rõ trong deck.** Không để giám khảo hiểu là có một agent lâm sàng đang chạy.

## Câu chuyện trong deck — đây là điểm mạnh, không phải hạn chế

Một agent thật, nhiều lần gọi tool, mỗi lần ghi lại; **tiền chỉ chảy qua chuỗi tất định**.

> *Luồng điều khiển là tất định. Agent chỉ nằm ở chỗ cần phán đoán.*

Đó đúng là luật `build-01` §0.2 ① mà cả dự án bảo vệ, và nó **trả lời trước** câu hỏi khó nhất của một hội đồng bảo hiểm: *"lỡ agent đi lạc thì sao?"* — nó không đi lạc được, vì nó không có tool nào ghi tiền.

Nếu ai hỏi vì sao không phải 6 agent: **ODC không cho agent gọi agent** (SA xác nhận 21/09), và một orchestrator tất định là lựa chọn đúng hơn cho tiền bảo hiểm. Đưa 6-agent vào phần *hướng phát triển*.

## Việc

- [ ] 1. Cập nhật `build-05` §9 — câu hỏi 1 đã đóng 21/09, ghi nguyên văn câu trả lời
- [x] 2a. **Q2 + Q3 của SA đã trả lời 21/09** — xem `specs/agents/spec.md`. Hai bất biến mới: description tool tiền phải cấm tự tính · IDP không bao giờ gọi trong lượt agent
- [x] 2a′. **Ghi bảng 5 agent → 5 tool** (PO 21/09) — mục "Năm agent cũ = năm tool" ở trên; 4 ràng buộc bề mặt tool → `./specs/agents/spec.md`
- [ ] 2. Định nghĩa tool list đóng của AG-2; kiểm **không tool nào** ghi `ClaimLine` tiền, `LimitLedger`, `PaymentOrder`, `Claim.State`
- [x] 2b. **Đo thời gian `RunAdjudicationChain`** — trace ODC rev 154: **719 ms** (lạnh, 370 span), round-trip 279 ms ấm (`chain-14-steps-a5/current-chain.md` readout rev 154). `ServerRequestTimeout` (app-level, trước = null/10 s) → **60 s** ở rev 157 (21/09)
- [ ] 2c. **Viết lại description `RunAdjudicationChain` cho agent đọc** — hiện *"FR-CORE-13 … STEP LOGIC NOT IMPLEMENTED"*; **làm CÙNG MỘT lượt Mentor với sửa hồi tố description `A5Step01…09`** (`chain-14-steps-a5` task 3e, PO 21/09 #5); bắt buộc có câu *"Không tự tính số tiền. Luôn gọi tool này và dùng đúng con số nó trả về."*; mỗi parameter có description riêng
- [ ] 2d. **Kiểm trên tenant khi tạo agent:** tool list của AG-2 chỉ có `RunAdjudicationChain` + action đọc + `WriteAgentRun`; **không** `A5StepNN_*`, `A5Step0N_*` helper, `A5Chain_*`, `RoundVnd`. Bằng chứng = screenshot tab Tools của Agent Workbench vào `../../../output/`
- [ ] 3. `WriteAgentRun` phân biệt `Agent` vs bước hệ thống; `Replay` flag bắt buộc
- [ ] 4. Màn `AgentTrace`: render bước hệ thống khác bước agent, **không nguỵ trang**
- [ ] 5. System prompt AG-2 theo 4 phần của `build-05`; mở đầu `Action taken: …` khi đã ghi dữ liệu
- [ ] 6. Deck: một slide nói rõ **một agent, orchestrator tất định, khuyến nghị là seed**
- [ ] 7. Merge vào `build-05` rồi archive
- [ ] 8. **Sửa `build-05` §9 tại nguồn (Project claude.ai):** dòng *"trần 120 s có đủ cho IDP không"* mang tiền đề sai từ 16/09. Ghi vào §9: **trần thật = 10 s mặc định / 60 s tối đa qua `ServerRequestTimeout` trên node gọi action — nguồn: SA OutSystems, 21/09/2026**; hệ quả: IDP chạy ngoài lượt agent. Không ghi thì người sau lại lập kế hoạch theo 120 s. *(Số hiệu AG-n: đã đóng 21/09 theo `build-01` §4.27 — không cần đối chiếu nữa)*
- [ ] 9. **Đổi tên seed theo `build-01` §4.27:** mọi chuỗi "AG-5" đang chỉ khuyến nghị lâm sàng (`Seed_AssessmentAndTerms` — Recommendation/Assessment/AgentRun trên 0001) → **"AG-3"**; kiểm `AgentRun.AgentName` không còn giá trị ngoài 6 mã. Asset tenant `AG5_Adjudication` (AIAgent, 08/09, rev 3) là đồ thử thời plan-5-8-9 — xoá hoặc đổi tên trong Studio (MCP không có rename). Doc: `verify-entity-vs-build01/batchB2-spec.md` dòng 233/250 đã sửa 21/09

## Xong nghĩa là

Một agent trên Agent Workbench gọi `RunAdjudicationChain`, ra một con số đúng, có bản ghi `AgentRun` — **và mọi dòng trên trace ứng với một thứ thật sự đã chạy.**
