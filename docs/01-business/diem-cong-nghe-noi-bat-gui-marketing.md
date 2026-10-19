# Những điểm công nghệ nổi bật từ OutSystems Partner Hackathon APAC 2026

*Tài liệu gửi đội Marketing làm nguyên liệu viết bài. Cập nhật 16/09/2026.*

**Lưu ý trước khi dùng:** phần cuối có danh sách những thông tin **không được đưa lên bài** — đọc trước khi viết.

---

## 1. OutSystems đã đổi định vị: từ "low-code platform" sang "Agentic Systems Platform"

Đây là thay đổi lớn nhất và là cái nền của mọi điểm bên dưới. OutSystems không còn tự giới thiệu là nền tảng low-code, mà là **nền tảng mở để build, điều phối (orchestrate) và quản trị (govern) các hệ thống agentic ở quy mô doanh nghiệp**.

Bốn trụ cột của nền tảng:

| Trụ cột | Nội dung |
|---|---|
| Govern with a Unified Platform | Một nơi quản trị toàn bộ danh mục hệ thống agentic, có Enterprise Context Graph và lựa chọn nơi triển khai |
| Engineer with Mentor | Build từ bất kỳ công cụ AI coding nào, bàn giao cho Mentor để tạo ứng dụng enterprise |
| Orchestrate with Agent Workbench | Điều phối và quản trị toàn bộ "đội ngũ agent" |
| Accelerate with Industry Solutions | Giải pháp agentic dựng sẵn theo ngành |

**Góc viết:** thị trường đang nói về AI agent như một thứ demo được nhưng không đưa vào production được. OutSystems đưa ra lời giải về **governance** — và đây là cái thị trường Việt Nam, đặc biệt ngành tài chính - bảo hiểm, quan tâm nhất.

---

## 2. Agent-to-Agent (A2A) đã là tính năng chính thức trong nền tảng

Trước đây muốn hai AI agent nói chuyện với nhau phải tự viết tích hợp. Giờ ODC có **A2A connection** ngay trong Portal: dán URL của agent bên ngoài, nền tảng tự đọc "agent card" (file mô tả năng lực mà agent đó công bố), tự hiểu agent đó làm được gì, cấu hình xác thực và test được ngay.

**Vì sao đáng chú ý:** đây là bước chuyển từ "một chatbot" sang "một đội agent phối hợp". Ví dụ trong thẩm định bảo hiểm: một agent bóc tách hồ sơ, một agent đối chiếu quyền lợi, một agent ra quyết định — ba agent nói chuyện với nhau theo chuẩn mở, không phải code tích hợp thủ công.

**Câu dùng được:** *"Chuẩn A2A cho phép agent của doanh nghiệp bạn làm việc với agent của đối tác mà không cần viết một dòng tích hợp nào."*

---

## 3. MCP chạy được cả hai chiều

MCP (Model Context Protocol) là chuẩn để AI kết nối với hệ thống và dữ liệu. Trên OutSystems giờ có đủ hai chiều:

| Chiều | Nghĩa là gì |
|---|---|
| **Gọi ra** | Ứng dụng OutSystems kết nối tới MCP server bên ngoài; mỗi tool của server đó trở thành một server action dùng được ngay trong app. Bản cập nhật gần đây bổ sung **Private Gateway** — gọi được cả MCP server nằm trong mạng nội bộ của doanh nghiệp |
| **Gọi vào** | Công cụ AI coding bên ngoài (Claude Code, Cursor, Kiro...) kết nối vào tenant OutSystems và hiểu được toàn bộ hệ thống hiện có của doanh nghiệp |

Bộ công cụ MCP của OutSystems là **mã nguồn mở công khai**: `github.com/OutSystems/outsystems-mcp`

**Góc viết:** khách hàng lo "dùng AI xong dữ liệu chạy đi đâu". Câu trả lời: kết nối đi qua chuẩn mở, chạy trong tenant của khách, và với Private Gateway thì MCP server nội bộ không cần phơi ra internet.

---

## 4. Lập trình viên không còn bị buộc phải rời công cụ quen thuộc

Mô hình mới: đội dev bắt đầu ở **bất kỳ công cụ AI coding nào họ đang dùng**, rồi bàn giao yêu cầu cho Mentor (AI của OutSystems) để dựng ứng dụng enterprise — với toàn bộ bối cảnh doanh nghiệp, bảo mật và quy trình duyệt giữ nguyên.

Số liệu OutSystems công bố:

| Chỉ số | Giá trị |
|---|---|
| Tăng tốc phát triển ứng dụng | **50%** (khách hàng SRS Distribution, thuộc Home Depot Group) |
| Số ứng dụng tạo ra kể từ khi Mentor ra mắt (tháng 4/2026) | **6.000+** |
| Tỷ lệ lập trình viên chấp nhận kết quả Mentor sinh ra | **98%** |

**Vì sao con số 98% đáng nói:** nó trả lời trực tiếp nỗi lo lớn nhất về AI sinh code — "code AI viết ra có dùng được không". 98% nghĩa là gần như mọi thứ Mentor sinh ra đều được dev giữ lại.

---

## 5. Đổi mô hình AI để giảm chi phí token — ngay trong nền tảng

Agent Workbench cho phép **hoán đổi model AI** cho từng tác vụ, với **hơn 100 model** truy cập qua Amazon Bedrock. Task đơn giản chạy model rẻ, task khó chạy model mạnh — không bị khoá vào một nhà cung cấp.

Đi kèm là **guardrails** (chặn dữ liệu nhạy cảm) và **evaluations** (kiểm chứng hành vi agent) có sẵn, không phải lắp thêm công cụ ngoài.

**Bối cảnh thị trường để dẫn bài** (số liệu OutSystems trích dẫn):

| Số liệu | Nguồn |
|---|---|
| Chi phí inference của doanh nghiệp tăng **4,1 lần** so với năm trước | RapidData, State of Enterprise AI 2026 |
| Chỉ **12%** doanh nghiệp vừa tăng doanh thu vừa giảm chi phí nhờ AI | Khảo sát CEO của PwC |
| Hơn **47.400** công cụ AI tự phục vụ đang tồn tại trên thị trường | thebestvpn.com |

Đây là bộ ba số liệu rất mạnh để mở bài: *"Doanh nghiệp đang đốt tiền vào AI gấp 4 lần năm ngoái, nhưng chỉ 12% thấy được kết quả ở cả hai đầu doanh thu và chi phí."*

---

## 6. Enterprise Context Graph — điểm khác biệt kỹ thuật cốt lõi

Đa số AI agent trên thị trường làm việc trong chân không: chúng biết ngôn ngữ, không biết doanh nghiệp. Enterprise Context Graph là **lớp bản đồ sống** về toàn bộ ứng dụng, dữ liệu, quy trình, quy tắc nghiệp vụ và hệ thống tích hợp của doanh nghiệp — để agent ra quyết định dựa trên thực tế vận hành chứ không phải suy đoán.

Hai hệ quả đáng nói:
- **Tiết kiệm token:** agent không phải "đọc lại" cả hệ thống mỗi lần chạy.
- **Agent học từ nhau:** agent truy cập được vết quyết định (decision trace) của các agent khác trong cùng hệ sinh thái.

**Câu dùng được:** *"Một AI agent giỏi mà không biết doanh nghiệp bạn vận hành thế nào thì vẫn chỉ là một con chatbot. Enterprise Context Graph là phần khiến agent trở nên dùng được thật."*

---

## 7. Giải pháp agentic dựng sẵn theo ngành, tải về từ Forge

OutSystems công bố **Agentic Industry Solutions** cho 4 ngành: **Ngân hàng, Bảo hiểm, Sản xuất, Khu vực công**. Đây không phải bản mẫu trình diễn mà là kiến trúc production-grade, tải về từ Forge và tùy biến tiếp.

Mỗi giải pháp gồm 3 lớp: mô hình dữ liệu chuẩn ngành + agent đã có guardrails + connector tới hệ thống lõi, kèm nhật ký audit sẵn sàng cho kiểm toán.

| Ngành | Trạng thái |
|---|---|
| Ngân hàng | Agents Kit đã có trên Forge; Loan Origination đã ra |
| Bảo hiểm | Agents Kit quý 3/2026; First Notice of Loss nửa cuối 2026 |
| Sản xuất | Supplier Portal nửa cuối 2026 |
| Khu vực công | Citizen Portal nửa cuối 2026 |

**Lập luận bán hàng hay nhất, lấy nguyên từ OutSystems:** *"Viết được một con agent là phần nhanh. Phần khó nằm ở tất cả những gì bên dưới: mô hình dữ liệu, nhật ký audit, connector có quản trị tới hệ thống lõi, và luồng xử lý xác định cho những bước mà chỉ cần sai một lần là hỏng cả cuộc chơi."*

---

## 8. Triển khai theo yêu cầu chủ quyền dữ liệu

Nền tảng chạy được ở: cloud native, **sovereign cloud / regulated cloud**, on-premise, hoặc hybrid — cùng một trải nghiệm phát triển.

**Rất đáng nhấn cho thị trường Việt Nam:** ngân hàng và bảo hiểm trong nước chịu ràng buộc về nơi lưu trữ dữ liệu. Đây là câu trả lời thẳng cho câu hỏi "dữ liệu của chúng tôi có phải ra nước ngoài không".

---

## 9. Bối cảnh: BNK trong nhóm partner đầu tiên tiếp cận Agent Experience

BNK nằm trong nhóm partner đầu tiên trên toàn cầu được cấp quyền truy cập **Agent Experience** — chương trình Early Access của OutSystems cho phép công cụ AI bên ngoài làm việc trực tiếp với nền tảng.

Đội đã dự **Technical Enablement (07/09)** và **Partner Hackathon (08/09)** tại Partner Summit APAC 2026, W Sentosa, Singapore, và hiện đang trong giai đoạn hoàn thiện giải pháp cho vòng trình bày.

**Giới hạn khi viết:** chỉ nhắc tên chương trình ở mức này. Không mô tả chi tiết kỹ thuật, danh sách công cụ, giới hạn hay lộ trình của Early Access.

---

## 10. Số liệu nền của OutSystems (dùng làm chứng cứ trong bài)

| Chỉ số | Giá trị |
|---|---|
| Số AI agent khách hàng đã xây dựng | **30.000+** |
| Lập trình viên được chứng nhận AI Ready | **3.700+** |
| Thành viên cộng đồng | **929.000+** |
| Đối tác toàn cầu | **600+** |
| Quốc gia | **70+** |
| ROI được kiểm chứng độc lập | **363%** (Forrester Total Economic Impact) |
| Xếp hạng G2 | **#1 về mức độ hài lòng của khách hàng**; Leader ở nhiều bảng, gồm AI Agent Builders |
| NPS | **95** |

**Ba hãng phân tích lớn đều xếp OutSystems vào nhóm Leader:**

- **Gartner** Magic Quadrant for Enterprise Low-Code Application Platforms 2025 — Leader 9 năm liên tiếp, đứng cao nhất trục Ability to Execute
- **Forrester Wave™** Low-Code Platforms For Professional Developers, Q2 2025 — Leader
- **IDC MarketScape 2025** Worldwide Low-Code and No-Code Developer Technologies — Leader

---

## 11. Ba câu chuyện khách hàng có số liệu cụ thể

| Khách hàng | Ngành | Kết quả |
|---|---|---|
| **Petrobras** | Năng lượng | **ROI gấp 15 lần trong chưa đầy 1 năm**. Agent "Raí" cho đội kiểm toán nội bộ: tra cứu quy định và hồ sơ audit cũ bằng ngôn ngữ tự nhiên, rút từ **vài ngày xuống vài phút** |
| **ButlerPlus** | Dịch vụ vận hành (Mỹ) | Đội agent phân tích **6.000 ảnh mỗi đêm** để kiểm tra chất lượng dịch vụ, tiết kiệm **485.000 USD** chi phí vận hành và nhân công |
| **Grihum Housing Finance** | Cho vay mua nhà (Ấn Độ) | Nền tảng thế chấp có AI: **onboarding khách hàng nhanh hơn 70%**, 100% hồ sơ vay không giấy tờ, mở rộng 72% phạm vi dự án số mà không tăng ngân sách CNTT |

Ngoài ra OutSystems công bố các khách hàng đang chạy hệ thống trọng yếu trên nền tảng: Bentley, Không quân Bồ Đào Nha, Pokémon, KeyBank, Western Union, Chevron, Zurich, Toyota, Bosch, Takeda, DHL, Vodafone, HEINEKEN, Đại học Yale.

---

## ⚠️ KHÔNG đưa vào bài

| Nội dung | Lý do |
|---|---|
| Chi tiết kỹ thuật của Agent Experience Early Access: tên/số lượng công cụ, kiến trúc gateway, cơ chế xác thực | Chương trình chưa GA, thông tin dành cho người tham gia |
| Lộ trình, mốc GA, hay bất kỳ ngày phát hành nào chưa được OutSystems công bố công khai | Chưa cam kết chính thức |
| Các giới hạn kỹ thuật và lỗi đã biết của bản Early Access | Nội bộ chương trình |
| Nội dung video demo nội bộ của OutSystems, tên tenant, tên ứng dụng demo | Tài liệu nội bộ |
| Repo Skills riêng tư và cách xin quyền truy cập | Private repo |
| **Bất kỳ tuyên bố nào về kết quả hackathon** | Vòng trình bày và công bố kết quả chưa diễn ra — nói "đã thắng" hoặc ám chỉ kết quả là sai sự thật |
| Danh sách các partner khác tham gia chương trình | Thông tin của bên thứ ba |

**Nguyên tắc chung cho đội marketing:** mọi số liệu và tính năng trong tài liệu này đều lấy từ nguồn OutSystems công bố công khai (deck doanh nghiệp, Forge, repo công khai, báo cáo hãng phân tích). Nếu cần trích dẫn thêm gì ngoài danh sách trên, hỏi lại trước khi đăng.
