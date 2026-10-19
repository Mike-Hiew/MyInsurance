# DELTA — `specs/agents` · lớp agent rút về một agent

> Merge vào Project doc `build-05` khi archive.

## Đổi

`build-05` mô tả **6 agent** với 7 lần hand-off agent-to-agent. **SA OutSystems xác nhận 21/09: ODC không cho agent gọi agent theo kiểu lồng nhau.**

⇒ Kiến trúc bản dự thi: **một agent (AG-2) + orchestrator là Server Action.** AG-0, AG-1, AG-3, AG-4, AG-5 không build.

## Bất biến giữ nguyên, không đổi một chữ

- **Không tool nào của agent ghi được trường tiền**, sổ hạn mức, lệnh chi trả hay `Claim.State`.
- Agent **đề xuất và diễn giải**; **con người quyết định**; **chuỗi tính tiền**.
- `AgentRun` là **append-only**, và `Replay` phải hiện rõ khi chạy `ReplayMode`.

## Bất biến MỚI

> **`AgentRun` chỉ ghi cái đã thật sự chạy.** Không dòng nào mang tên một agent không tồn tại. `HandoffTo` null khi không có bàn giao — null nghĩa là *không có*, không phải *chưa biết*.

Lý do: `build-01` §2.13 định nghĩa `AgentRun` là vết audit mà ban giám khảo sẽ mở ra xem. Một dòng bịa ở đó không khác gì một con số bịa trong chuỗi.

## Quyết định

**Chốt 21/09/2026** theo phương án 3 của SA. Sáu agent chuyển sang phần *hướng phát triển* của deck.

---

## Bề mặt tool — bốn ràng buộc bắt buộc từ 21/09 (PO), áp ngay cho bước 6–14 đang viết

### ⭐ Bất biến MỚI — **chuỗi là MỘT tool, các bước là ruột**

> **Chỉ `RunAdjudicationChain` được khai là tool của agent. Mười bốn `A5StepNN_*`, các helper `A5StepNN_*`, `A5Chain_LoadContext`, `A5Chain_DryRun`, `RoundVnd` KHÔNG BAO GIỜ được khai là tool — không trên Agent Workbench, không qua Service Action, không "để test cho tiện".**

Lý do: khai lẻ từng bước là cho agent quyền gọi bước 10 trước bước 8, hoặc gọi bước 14 mà không qua 1–13. Khi đó **tiền không còn chảy qua chuỗi** — vi phạm `build-01` §0.2 ① bằng cửa sau, dù không tool nào ghi Entity tiền trực tiếp. Thứ tự bước là một phần của tính tất định; nó chỉ được bảo vệ khi thứ tự đó nằm **trong** một Server Action.

Hệ quả cho code: description của mọi `A5StepNN_*` mở đầu bằng câu *"Bước nội bộ N của RunAdjudicationChain — không phải tool, không gọi trực tiếp."* Nếu ai đó lỡ khai nó, agent đọc thấy và không dùng — lớp phòng thủ thứ hai, không thay lớp thứ nhất. **PO duyệt 21/09:** hai lớp cho một luật về tiền là liều đúng.

### Ba ràng buộc còn lại

| # | Ràng buộc | Áp cho | Trạng thái 21/09 |
|---|---|---|---|
| **a** | **Input phẳng** — chỉ kiểu nguyên thuỷ, không Structure lồng, không List | **CHỈ chữ ký tool** (`RunAdjudicationChain` và 4 tool tương lai) — **PO duyệt 21/09**: lời SA nói về thứ LLM phải điền; bước nội bộ do Server Action gọi nhau, `ChainCtx` giữ nguyên, không làm phẳng 1–9 đã publish | `RunAdjudicationChain(ClaimId: Long)` ✅ · `ChainCtx` (Structure có List) đi **giữa các bước nội bộ** — không phải input tool, được phép |
| **b** | **Description viết cho AGENT đọc**, không cho developer — agent chọn tool theo description | **Mọi** Server Action. Tool ⇒ nói *khi nào gọi*, *khi nào không*, *cấm tự tính* · bước nội bộ ⇒ câu "không phải tool" ở trên | `RunAdjudicationChain` hiện *"FR-CORE-13 … STEP LOGIC NOT IMPLEMENTED"* ❌ · `A5Step01…09` hiện tham chiếu spec cho developer ❌ — sửa ở việc 2c |
| **c** | **Một action một việc, tên là động từ** | Mọi Server Action | `RunAdjudicationChain` · `WriteAgentRun` · `IntakeClaimDocuments` · `GetClinicalAssessment` · `EvaluateFraudSignals` · `DraftCustomerLetter` ✅ |
| **d** | **Chỉ chuỗi là tool** | Agent Workbench | Bất biến ở trên |

### Description mẫu cho `RunAdjudicationChain` (dán khi làm việc 2c)

> Tính số tiền chi trả cho một hồ sơ bồi thường bằng chuỗi 14 bước tất định theo A5. **Gọi tool này khi** cần biết hồ sơ được chi trả bao nhiêu, hoặc sau khi một khuyến nghị lâm sàng được xác nhận và cần tính lại. **Không tự tính số tiền. Luôn gọi tool này và dùng đúng con số nó trả về.** Cùng đầu vào cho cùng kết quả. Trả về TotalPayable, TotalCustomerBorne và nhật ký từng bước (StepLogJson) để giải thích cho người dùng.
>
> `ClaimId` — Id nội bộ của hồ sơ (số nguyên), lấy từ ngữ cảnh hội thoại; không phải mã `CLM-2026-nnnn`.

### Ánh xạ 5 agent → 5 tool (PO 21/09)

Tên agent theo **`build-01` §4.27** (chuẩn, 16/09): `AG-1 Intake → IntakeClaimDocuments` · `AG-2 Adjudication → RunAdjudicationChain` · `AG-3 Clinical → GetClinicalAssessment` · `AG-4 Fraud-signal → EvaluateFraudSignals` · `AG-5 Customer-communication → DraftCustomerLetter`; AG-0 Orchestrator = Server Action. Chỉ `RunAdjudicationChain` trước pitch. Bốn tool sau phải khai được **mà không đổi** chữ ký hay description của tool đầu — đó là phép thử "không chặn".

---

## Trả lời SA — Q2 và Q3, 21/09/2026

### Q2 — kiểu input và sức nặng của description

> Agent ODC hỗ trợ **cả hai**: kiểu nguyên thuỷ (LLM điền trực tiếp, dễ nhất) · **Structure** (điền được nhưng **phải mô tả rõ từng trường** trong description của parameter) · **List** (kỹ thuật hỗ trợ nhưng **LLM thường khó điền chính xác — nên tránh nếu không cần**).
>
> Description ảnh hưởng **rất lớn** tới việc chọn tool: description của action handler là **tín hiệu chính** để LLM quyết định khi nào gọi; description của **từng parameter** giúp LLM biết điền gì. Nên viết dạng hướng dẫn: *"Gọi tool này khi…"*, *"Không tự tính — luôn gọi tool này"*.

### ⭐ Bất biến mới — bắt buộc trên mọi tool chạm tiền

> **Mọi description của tool liên quan tới tiền PHẢI mang câu cấm tự tính**, nguyên văn dạng: *"Không tự tính số tiền. Luôn gọi tool này và dùng đúng con số nó trả về."*

Đây là **rủi ro lớn nhất của việc đặt một LLM cạnh một hồ sơ bồi thường**: nó tự cộng trừ ra một con số trông hợp lý thay vì gọi chuỗi. `build-01` §0.2 ① cấm agent **ghi** tiền; câu description này là thứ cấm agent **tính** tiền. Hai lớp khác nhau, cần cả hai.

### Ràng buộc kiểu input cho bản dự thi

| Quy tắc | |
|---|---|
| Chỉ dùng **kiểu nguyên thuỷ** | Tool của ta không cần Structure — `RunAdjudicationChain(ClaimId)` là đủ |
| **Không dùng List** | SA khuyến cáo; và ta không có nhu cầu |
| **Mọi parameter phải có description riêng** | Không để trống, kể cả khi tên đã rõ |

---

### Q3 — trần thời gian: **10 giây mặc định, tối đa 60 giây**

> ⚠ **`build-05` §9 (16/09) mang tiền đề sai** — câu *"trần 120 s có đủ cho IDP không"* giả định trần 120 s. Trần thật thấp hơn hai lần. Phải sửa **tại `build-05` §9** kèm ngày + nguồn (task 8), không chỉ ở change này.

> Server action gọi từ agent chạy trong **request đồng bộ**. Timeout mặc định của ODC là **10 giây**, nâng tối đa **60 giây** qua thuộc tính `ServerRequestTimeout` trên node gọi action.
>
> Ba cách xử lý tool chậm: **(1)** job bất đồng bộ bằng Timer, agent trả `jobId` rồi lượt sau hỏi trạng thái · **(2)** phân trang, mỗi lần 1–2 trang dưới 60 s · **(3)** **pre-process ngoài agent** — chạy IDP lúc upload, lưu vào entity, tool của agent **chỉ đọc kết quả đã có**.

### ⭐ Bất biến mới — IDP KHÔNG BAO GIỜ được gọi bên trong một lượt agent

**Chọn cách (3).** Lý do, theo thứ tự:

1. **Nó là kiến trúc đã có sẵn.** `build-01` §3.4 nói `RawExtractionJson` được copy vào hồ sơ **ngay lần đọc đầu** và từ đó bất biến. Trích xuất thuộc **intake**, không thuộc **adjudication** — đặt nó trong lượt agent là trộn hai tầng.
2. **16 giây/trang.** Một hồ sơ vài trang vượt 60 giây. Trần cứng, không lách được.
3. **Không cần entity mới, không cần Timer.** Cách (1) đòi thêm entity job + Timer + tool hỏi trạng thái — công của một ngày, để giải một vấn đề mà cách (3) làm biến mất.

⇒ Tool của agent **chỉ đọc** `ClaimDocument.RawExtractionJson` và kết quả đánh giá đã lưu. Với `ReplayMode` thì càng nhanh.

**Cách (1) Timer ghi lại làm đường đi nếu sau này IDP phải chạy trong lượt agent — không dùng cho bản dự thi.**

### Phải đo, không được giả định

`RunAdjudicationChain` chạy 14 bước trên ~6 dòng chi phí, có đọc ledger và ghi posting. **Mặc định là 10 giây** — không ai được cho rằng nó đủ.

| Việc | |
|---|---|
| **Đo** thời gian chạy thật của `RunAdjudicationChain` khi xong bước 14 | Ghi vào `current-chain.md` |
| **Set `ServerRequestTimeout` tường minh** trên mọi node gọi nó | Không để mặc định 10 s |
| Nếu vượt ~30 giây | Cảnh báo sớm — còn kịp tối ưu, không kịp nếu phát hiện ngày 28/09 |

Ràng buộc này áp cho **cả màn hình**, không riêng agent: mọi nơi gọi `RunAdjudicationChain` đều nằm dưới cùng trần đó.
