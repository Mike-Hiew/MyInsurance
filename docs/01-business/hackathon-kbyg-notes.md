# OutSystems Partner Hackathon APAC 2026 — Ghi chú chi tiết buổi "Know Before You Go"

**Ngày họp:** Thứ Hai, 24/08/2026
**Người trình bày:** Donnie Prakoso (Solution Architect for Partners & Strategic Customers, APAC — SA partner đầu tiên của OutSystems tại APAC), Lynn Wang (Partner Manager), Muralee Kanagaratnam "MK" (chủ trì chương trình Hackathon), Abhishek Dubey.
**Mục đích:** Giải thích toàn bộ về Partner Hackathon đầu tiên của OutSystems: lý do, yêu cầu, tiêu chí, timeline, giải thưởng và liên hệ với Technical Enablement. Khoảng 20 phút trình bày, phần còn lại Q&A.

---

## 1. Sự kiện và lịch trình

Hackathon nằm trong **Partner Summit 2026 APAC**, tổ chức tại **W Sentosa, Singapore**.

| Ngày | Thời gian | Nội dung | Đối tượng |
|---|---|---|---|
| Thứ Hai 07/09 | 13:00 – 17:00 | Technical Enablement | Pre-sales, tech, developer |
| Thứ Ba 08/09 | 09:00 – 12:00 | Partner Hackathon ("Build live, win big") | Developer, tech, pre-sales |

### Toàn bộ hành trình — Key dates & milestones

| Mốc | Ngày | Hình thức |
|---|---|---|
| Registration mở | Ngay bây giờ — đăng ký sớm | Form |
| Pre-Hackathon Webinar Briefing (KBYG) — **bắt buộc dự** | 24/08, 11:00–11:45 SGT | Zoom |
| **Partner Hackathon** tại Partner Summit | **08/09**, 9:00–12:00 | Onsite, W Sentosa |
| **Pitching & Presentation** | **7–8/10** (TBC) | Zoom |
| **Công bố người thắng** | **22/10** (TBC) | Email + LinkedIn |

Donnie nhấn mạnh: ngày 8/9 thực chất là **3 giờ full enablement** (hỏi đáp, được chỉ thêm nhiều thứ), phần pitch/demo diễn ra **sau đó** qua Zoom vào 7–8/10. Cơ chế pitch và công bố còn đang hoàn thiện, nhưng các mốc ngày đã cố định. Khuyên chụp lại slide này vì chưa chắc deck được gửi ra.

### Technical Enablement (07/09, 13:00–17:00) — agenda chi tiết

| Giờ | Thời lượng | Session | Diễn giả | Hình thức | Nội dung chính |
|---|---|---|---|---|---|
| 13:00–14:00 | 50' | **Session 1: FAQ — How OutSystems Solves Enterprise Problems** | Donnie Prakoso, Andri Augustinus (VP), Abhishek Dubey | Đối thoại + Q&A | Các phản đối/lo ngại phổ biến của khách hàng (vibe coding…), OutSystems giải quyết thế nào; giới thiệu ASP cho enterprise; tự tin nói chuyện với khách về rào cản áp dụng AI |
| 14:00–14:15 | 10' | Quiz + swag | | | |
| 14:15–14:45 | 30' | Break — networking | | | |
| 14:45–15:35 | 50' | **Session 2: Agentic Systems Platform — Demos & Integration** | Donnie + các SA | Multi-demo rotation | Agent Workbench (multi-agent orchestration, guardrails, audit trail); Mentor (ngôn ngữ tự nhiên → app/code, deterministic, an toàn, lặp lại được); Claude Code/Skills tích hợp OutSystems; định vị Bedrock, Kiro, Transform bên cạnh OutSystems; kịch bản thật: **Loan origination workflow với nhiều AI agent** |
| 15:35–15:45 | 10' | Quiz + swag | | | |
| 15:45–17:00 | | **Session 3: Best Practices** | Donnie, Huy (PS), Professional Services | | Thiết kế app scalable (distributed architecture, loose coupling, event-driven); triển khai enterprise (hybrid self-hosted, sovereign cloud, data isolation); case study PS đã giao dự án agentic AI; staffing & timeline (kỹ năng cần, cách tổ chức đội); guardrails, governance, enterprise context |

**Ba trụ cột** của enablement: (1) Agentic AI với Agentic Systems Platform, (2) Best Practices với Mentor, (3) Business Logic với OutSystems Workflow — OutSystems tin rằng ba mảng này là thứ giúp đội thắng hackathon. Nội dung thiết kế cho cả người mới lẫn người đã build trên OutSystems; các chủ đề nâng cao (distributed/event-driven) **không** được dạy lại trong buổi hackathon, nên Donnie khuyên đừng bỏ.

### Agentic Systems Platform (ASP) — tóm tắt nhanh

Là "bước tiến hoá tiếp theo" của nền tảng: platform **mở, có quản trị** để build, orchestrate và govern ứng dụng agentic và AI agent ở quy mô enterprise. Bốn đặc tính: **Unified, Agile, Enterprise Proven, Contextual**. Tooling bên trên có thể là Bedrock, Claude, Kiro, Cursor. Hai nhánh:

- **Agentic Systems Engineering** — dùng bất kỳ AI coding tool nào (Claude Code…) để build ứng dụng agentic trên OutSystems, thông qua **Enterprise Context Graph** (nội bộ gọi "big pie").
- **Agentic Systems Orchestration** — build single-agent, multi-agent, orchestrated agents bằng **Agent Workbench**.

ASP là trọng tâm của OutSystems kể từ ONE Conference; nếu chưa nghe, hãy nhờ PSM sắp một buổi briefing + demo **trước** sự kiện để tránh quá tải thông tin tại chỗ.

---

## 2. Bối cảnh: Agentic Industry Solutions

Công bố tại ONE Conference (2–3/06/2026). Ý tưởng: Enterprise AI theo ngành, **đóng gói sẵn thay vì build từ đầu**; mỗi solution là một kiến trúc hoàn chỉnh, deploy được, partner/khách hàng tùy biến thêm.

**Ba trụ cột:**
- **Pre-built industry architecture:** data model chuẩn ngành, governed agents, ứng dụng, workflow, UX, integration đặc thù ngành. Là **production-grade**, không phải pilot/POC, có trên **Forge**.
- **Intelligent Model Orchestration với Amazon Bedrock:** model phù hợp cho từng task, không khoá vào một nhà cung cấp. OutSystems chạy 100% trên AWS; Bedrock là nền của Agent Workbench.
- **Open platform:** trung lập về model, agent, tooling; khách hàng kiểm soát model nào chạy task nào, chi phí và cách kiến trúc tiến hóa.

**Kiến trúc phân lớp** (từ trên xuống): Your Innovation & Differentiation (Differentiated CX, Products & IP, Proprietary Data) → Agentic Layer (Use-case-specific Agents, Orchestration Patterns, Agentic UX) → Common Layer (Workflows, Data Models, Core System Connectors) → Agentic Systems Platform.

**Bốn ngành ra mắt** (tất cả sẽ có trên Forge):

| Ngành | Thành phần & thời điểm |
|---|---|
| Banking | Agents Kit (GA), Loan Origination (07/2026), Connectors Kit (Q3 2026), Onboarding (H2 2026) |
| Insurance | Agents Kit (Q3 2026), Connectors Kit (H2 2026), First Notice of Loss (H2 2026) |
| Manufacturing | Agents Kit (H2 2026), Connectors Kit (Q3 2026), Supplier Portal (H2 2026) |
| Public Sector | Agents Kit (H2 2026), Connectors Kit (H2 2026), Citizen Portal (H2 2026) |

Hiện **Banking và Insurance đã tìm được trên Forge** — đây là ứng dụng agentic (tích hợp Agent Workbench), không chỉ app thường. Donnie cho biết đã tự build vài ứng dụng trên nền này, nhanh và tốt.

---

## 3. Chủ đề và tiêu chí chấm Hackathon

- **Bắt buộc** thuộc 1 trong 4 ngành focus: **Banking/Financial Services, Insurance, Manufacturing, Public Sector/Government**.
- Ví dụ gợi ý: agentic loan origination, agentic payroll (banking); fraud detection, auto-claim agentic workflow (insurance); tương tự cho manufacturing/public sector.
- Yêu cầu: **build real, deployable Agentic AI — live code, không phải slideware**. Sau khi build phải **nộp bài** để OutSystems review; bộ tiêu chí chi tiết chưa được tiết lộ.
- **Judging priority** (3 điểm):
  1. Use case **bên ngoài** (external, tức của khách hàng thật) trong ngành focus.
  2. **Câu chuyện ROI rõ ràng**.
  3. **Nguồn lực cam kết** (committed resources) để triển khai.
- Thông điệp cốt lõi: **"Arrive with a focus-industry, customer-relevant use case in mind"** — đến với ý tưởng đã thống nhất trước với SA/dev trong công ty.
- Trong buổi hackathon, OutSystems sẽ làm hands-on enablement và có mặt để gỡ vướng; nên tận dụng để hỏi.
- **Được phép mang solution build sẵn** (Q&A): Donnie nói không ai cấm, thậm chí là best practice — đừng chỉ mang ý tưởng, hãy mang prototype và dùng thời gian tại chỗ để cải tiến, thêm tính năng.
- Demo live trước đồng nghiệp và lãnh đạo OutSystems APAC sẽ **không diễn ra ngay trong buổi hackathon**; cơ chế demo/công bố kết quả đang được xây dựng và sẽ thông báo sau (MK phụ trách).

---

## 4. Đội hình

- **Bắt buộc 3 thành viên**: ít nhất **1 business** (product owner / pre-sales), **2 Solution Architect / Developer** + **1 engineer** (theo slide).
- Lý do: OutSystems muốn đội làm cầu nối giữa business logic và công nghệ, không chỉ thuần kỹ thuật.
- Mở cho **mọi partner dự Summit**.

---

## 5. Bốn yêu cầu bắt buộc về công cụ ("hard requirements")

### 5.1 Claude license trả phí (tự mang)
- OutSystems **không cấp**. Bất kỳ gói **paid** nào (cá nhân hay enterprise) đều được — vì cần **Claude Code**, thứ sẽ dùng rất nhiều trong enablement.
- Lý do chọn Claude: toàn bộ skills, workflow, roadmap tooling của Agentic Systems Platform đều được "battle-tested" với Claude. Có thể dùng Kiro (AWS), OpenCode, hay agent tự build (pydantic-ai…) nhưng OutSystems **không hỗ trợ** vì không quen.

### 5.2 Quyền truy cập OutSystems tenancy
Cần để build và để đăng nhập OAuth khi dùng MCP.

### 5.3 Agent Experience EAP (Early Access Program) đã được bật + MCP đã cấu hình
- Phải **nộp Tenant URL** qua form (link Lynn gửi trong chat Zoom) để OutSystems allow-list ở backend. Hạn ban đầu 14/08; Lynn gia hạn: **nộp muộn nhất thứ Tư (26/08)**. Quá trình bật mất thời gian nên phải làm ngay.
- Không có EAP thì "sẽ rất lạc" trong hackathon — đây là nơi trải nghiệm thực Agent Experience.
- **10 công ty đợt đầu đã nộp**: Codeme Pro Sdn Bhd, Solverines Technology Solution, Temus Pte Ltd, NLLN, THEDIGIFAC SERVICES PRIVATE LIMITED, NCS Pte Ltd, Bahwan Cybertek, PhoenixDX, **TECHWORLD SOLUTIONS VIET NAM**, FONE TECHNOLOGIES.
- Khi được bật, nhận email từ **Deniz Arin (<redacted-email>)**, ngày 18/08, tiêu đề *"You're live — getting started with Agent Experience Early Access for ODC"*. Email gồm: video onboarding, onboarding deck (phạm vi hỗ trợ, quản trị dữ liệu, FAQ, troubleshooting), hướng dẫn set up MCP (~5 phút), link đặt lịch feedback session định kỳ với product team, và gợi ý bắt đầu bằng query read-only kiểu "Give me a high-level picture of my tenant".
- Nếu đã nộp mà chưa thấy email: kiểm tra spam/junk; nếu vẫn không có, liên hệ **Partner Manager** (Lynn) hoặc PSM.

### 5.4 ODC Studio
Tải bản **mới nhất**, đăng nhập **đúng tenant** đã allow-list — sai tenant thì MCP tools sẽ không hiện.

---

## 6. OutSystems MCP và Skills

- Repo public: **github.com/OutSystems/outsystems-mcp** (đã mở vài tháng).
- **Cài với Claude Code:** paste đoạn prompt trong README; Claude Code tự chạy `claude plugin marketplace add OutSystems/outsystems-mcp`, `claude plugin install outsystems@outsystems`, hỏi tenant hostname, rồi `claude mcp add -s user --transport http outsystems https://<tenant>/mcp`, restart. Có lệnh `/outsystems-feedback <message>` để gửi phản hồi cho maintainers.
- **Cài với Claude Desktop:** Settings → Connectors → Add custom connector → nhập `https://<tenant>/mcp` → Add. Lần gọi tool đầu sẽ mở trình duyệt OAuth về OutSystems portal. Trên gói Team/Enterprise có thể cần Owner thêm connector ở Admin settings.
- Sau khi authenticate: hiện có **56 tools** (lúc ra mắt chỉ ~40–41; thêm ~15 tools mới). Ví dụ: liệt kê app/asset trong tenant.
- **Skills**: có trong repo MCP chính thức, và một repo riêng cần **gửi email kèm GitHub ID** để xin quyền. Ví dụ skills: tích hợp Figma, Snowflake, visual design, mentor, BRD… Khi GA sẽ gộp về repo MCP chính.

---

## 7. Giải thưởng

| Hạng | MDF (OutSystems tài trợ 100%) | Exam voucher |
|---|---|---|
| Nhất | €10.000 | 20 |
| Nhì | €5.000 | 10 |
| Ba | €3.000 | 5 |

Kèm incentive độc quyền và brand recognition.

### Mục đích chiến lược (MK chia sẻ)
- Hackathon ra đời từ feedback của partner: muốn **hands-on** hơn và tiếp cận "latest and greatest", trong đó có Agent Experience. Partner tham gia sẽ nằm trong **nhóm partner đầu tiên trên toàn cầu** được dùng Agent Experience.
- OutSystems đang chuyển sang **mô hình kinh doanh theo industry solution**; hackathon là điểm khởi đầu để partner build giải pháp mà OutSystems có thể **đưa ra thị trường**. Chi tiết sẽ nói thêm tại Partner Summit.
- Có thể mang giải pháp sẵn có, dùng Agent Experience để nâng cấp. Mục tiêu dài hạn: một số giải pháp sẽ được đưa lên **marketplace** và OutSystems cùng đi bán với khách hàng.
- **Lưu ý:** thắng hackathon **không đảm bảo** giải pháp được vào industry solutions showcase; nhưng nếu thắng, OutSystems sẽ chuyển tiếp cho đội Industry Solutions đánh giá (đã có nhiều giải pháp toàn cầu được duyệt vào portal).
- Ý định trước mắt: partner build giải pháp dùng nội bộ hoặc, tốt hơn, cho khách hàng rồi nhân rộng.

---

## 8. Q&A đã ghi nhận

- **Build tại chỗ hay build trước rồi nộp?** → Được build trước; nên mang prototype đến.
- **Loại Claude license nào?** → Bất kỳ gói paid, cá nhân hay enterprise.
- **Sau hackathon thì sao, khi nào công bố kết quả?** (Abhishek hỏi) → Pitch & presentation qua Zoom 7–8/10 (TBC), công bố người thắng 22/10 (TBC) qua email và LinkedIn.
- **Chatbot agent có bắt buộc bằng tiếng Anh không?** (Kim Yoon-Sik) → **Không.** OutSystems khuyến khích bản địa hoá — chatbot nói tiếng Hàn, tiếng Việt… với khách hàng đều được. Nhưng **phần trình bày/pitch cho OutSystems phải bằng tiếng Anh**.

### MK làm rõ về khung thời gian và cách chấm
- Hôm nay (24/8) là **kickoff**. Ngày 8/9 đến với ý tưởng (kể cả "half-baked"), build tại chỗ 3 giờ — **không kỳ vọng hoàn thành**. Từ đó có **~4 tuần sprint** để hoàn thiện, rồi pitch (7–8/10), chọn winner và kết thúc (22/10).
- Chấm điểm dựa trên việc dùng **ODC + Agent Experience để build một giải pháp agentic thực sự**. Giải pháp có **nhiều tương tác agent-to-agent** sẽ được đánh giá cao — ví dụ underwriting: agent A nói chuyện với agent B, phối hợp với agent C — và đội phải **diễn giải được** luồng đó.
- Khuyến khích **không chỉ dồn vào Banking**; Manufacturing, Insurance, Public Sector cũng được ưu tiên như nhau.
- **Nộp càng nhiều giải pháp càng tốt** — một partner có thể nộp nhiều bài. Donnie: OutSystems cố ý không giới hạn số lượng vì cơ cấu đội mỗi công ty khác nhau. Lynn bổ sung: dù nộp 1, 2 hay 10 bài, **OutSystems chỉ chọn một bài tốt nhất của mỗi partner** để chấm, nhưng các bài còn lại vẫn giúp OutSystems thấy partner đã làm được gì trong mảng agentic.
- **Yêu cầu kỹ thuật bắt buộc** (trả lời câu hỏi của Michael): giải pháp phải **build trên ODC** và **dùng Agent Workbench**.
- Thông điệp của Lynn: ngoài MDF, giá trị chính của hackathon là **cơ hội học cách build Agentic AI**; kể cả không thắng, partner vẫn có sẵn một giải pháp agentic để **đem đi bán cho khách hàng** trong thời gian tới.
- Nhắc lại: chỉ cần tenant đã được allow-list ở backend là "good to go".

---

## 9. Checklist hành động cho đội

- [ ] **KHẨN — BNK Solution chưa có trong danh sách 10 công ty đã nộp EAP.** Nộp Tenant URL qua form của Lynn **trước thứ Tư 26/08**, rồi chờ email "You're live" từ Deniz Arin; nếu không nhận được thì báo Partner Manager.
- [ ] Mỗi thành viên kỹ thuật có **Claude paid license** và cài Claude Code.
- [ ] Cài **OutSystems MCP**, authenticate, kiểm tra thấy 56 tools.
- [ ] Xin quyền repo **skills** (gửi GitHub ID).
- [ ] Cài **ODC Studio** bản mới nhất, đăng nhập đúng tenant.
- [ ] Chốt đội **3 người** (1 business + 2 SA/dev + engineer).
- [ ] Chọn **use case khách hàng thật** trong 4 ngành, chuẩn bị **ROI story** và cam kết nguồn lực.
- [ ] **Build prototype trước** ngày 8/9; tận dụng Banking/Insurance kits trên Forge nếu phù hợp.
- [ ] Đăng ký dự **Technical Enablement 07/09**.
- [ ] Nhờ PSM sắp buổi **briefing về Agentic Systems Platform** trước sự kiện nếu đội chưa quen.
- [ ] Giữ lịch **7–8/10** (pitch qua Zoom) và **22/10** (công bố kết quả); lên kế hoạch **sprint 4 tuần** 8/9 → 7/10.
- [ ] Thiết kế use case có **nhiều agent phối hợp với nhau** (agent-to-agent), không chỉ một chatbot đơn; **bắt buộc build trên ODC và dùng Agent Workbench**.
- [ ] Cân nhắc nộp **hơn một giải pháp** và ở ngành ngoài Banking (ít cạnh tranh hơn).
- [ ] Chatbot có thể tiếng Việt; chuẩn bị **pitch bằng tiếng Anh**.
