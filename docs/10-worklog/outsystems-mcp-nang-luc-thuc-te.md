# outsystems-mcp — năng lực thực tế, kiểm chứng trên tenant (04/09/2026, cập nhật 08/09)

Kết quả chạy prompt khám phá `tools/list` trên tenant thật. **Đảo ngược giả định trước đó** rằng MCP không tạo được AI Agent.

## 1. Tạo và sửa AI Agent trên Agent Workbench — CÓ, trực tiếp

**Tạo, hai đường độc lập:**

- `app_create` với `kind: "AIAgent"` → clone từ Agent template và **tự động wire vào AI model connection** (`model` = tên hoặc asset key).
  - Tenant có đúng 1 connection → tự chọn.
  - Nhiều connection mà không truyền `model` → lỗi, liệt kê ra danh sách.
  - Không có connection nào → tạo agent unwired kèm warning.
  - Đây chính xác là wizard của Agent Workbench.
- `mentor_create_asset` với `assetType: "Agent"` → tạo agent và **nạp luôn vào session** để sửa ngay.

**Sửa:** `mentor_load_asset` (Agent nằm trong danh sách asset type được phép load) → `mentor_prompt` → `mentor_get_run` → `mentor_publish`.

**Đọc:** `context_agents` — chỉ inventory.

> `app_create` dựng ra vỏ và nối model. Chất lượng agent nằm ở **system prompt** và **tool list đóng** — phần đó vẫn là việc của người. Tạo được agent ≠ có agent tốt.

### 1b. T01b — chạy thật trên tenant (08/09, probe ZZ_ProbeAgent)

| Câu hỏi | Kết quả |
|---|---|
| `app_create` kind `AIAgent` có chạy trên tenant này không | **Có.** Asset `ZZ_ProbeAgent` (`0f524d23-aafe-4ad5-90b3-9ae98cbf321c`), assetType `Agent`, revision 1, clone từ `template_Agent` |
| Có tự wire vào model connection không | **Chưa xác minh** — response bị nuốt do timeout; phải mở Studio nhìn mới biết |
| Số AI model connection trên tenant | **3**: `TrialGPT5` (Azure OpenAI, Trial) · `TrialGPT4o` (Azure OpenAI, Trial) · `OpenAI_Model` (OpenAI, **Customer**) |
| Warning agent unwired | Không lấy được — chỉ phát sinh khi tenant có 0 connection |
| Thời gian | Asset xuất hiện sau **~21 s**; client timeout ở **~66 s** |
| `mentor_load_asset` trên asset type Agent | **Load thành công** (`{}`), session đóng sạch |

**Ba hệ quả bắt buộc nhớ:**

1. **Timeout ≠ thất bại.** `app_create` (và publish) có thể hoàn tất phía server sau khi client đã bỏ cuộc. Sau mỗi timeout: **không retry** — đọc `context_agents` hoặc asset để xác minh trước. Retry mù → asset trùng hoặc lỗi `OS-APPS-40018`.
2. **Luôn truyền `model` tường minh.** 3 connection nên bỏ trống là lỗi ngay.
3. **Wiring không đọc được bằng tool read-only.** `app_refs` không hiện `AIModelConnection` (kiểm chứng chéo bằng `IntakeAgent` — agent thật, đã publish, cũng không hiện). `context_agents` trả `providerName: null` cho cả 14 agent. Muốn chắc thì mở Studio.

**Điểm quyết định cho ngày pitch:** `TrialGPT5`/`TrialGPT4o` là entitlement **Trial** — có hạn mức. `OpenAI_Model` là **Customer**. Nếu demo live phụ thuộc vào agent, cân nhắc wire AG-5 vào `OpenAI_Model`, hoặc hỏi SA giới hạn Trial cụ thể là gì (gộp vào câu hỏi số 6).

## 2. REST Consume — không có tool chuyên dụng

Không tồn tại `rest_*`, không có import OpenAPI/Swagger. Hai đường vòng:

- `app_create` với `kind: "ExternalConnection"` (cùng nhóm `MCPConnection`, `A2AConnection`) — tạo được asset nhưng **không có wizard template**, chỉ ra vỏ blank chưa cấu hình endpoint/method.
- **Chưa verify:** `mentor_start_session` → `mentor_load_asset` → `mentor_request_upload` đẩy file OpenAPI (`.json`/`.xml` nằm trong extension cho phép) → `mentor_prompt` tham chiếu `attachmentRefs`.

Với 3 endpoint của MVP và các quirk Mentor không biết (User-Agent giả trình duyệt, `errorCode` trong HTTP 200, `result` lồng 2 lớp ở `/sync`), **dựng tay trong Studio vẫn nhanh và chắc hơn**. Dự án cũng chưa có file OpenAPI cho IDP.

## 3. Đọc entity schema — `context_entities`, ba tham số quyết định

| Tham số | Giá trị đúng | Nếu bỏ qua |
|---|---|---|
| `app` | Asset key UUID hoặc **substring tên**, không phân biệt hoa thường | — |
| `owned_only` | **`false`** | Mặc định `true` khi có `app` → entity kế thừa biến mất, dễ tạo trùng. Với `false`, mỗi dòng gắn `producerAssetName` |
| `limit` / `offset` | `limit: 100` (trần), phân trang bằng `offset` tới khi `next_offset` rỗng | Mặc định chỉ **20** → đọc cụt mà không biết, rồi báo "thiếu entity X" trong khi X vẫn có |

Sâu hơn schema phẳng: `context_graph` (toàn model dạng XRE) · `context_search` với `objects: ["Entities"]`.

## 4. Ảnh hưởng tới kế hoạch hackathon

| Trước | Sau |
|---|---|
| Lớp agent làm tay, dựng tại hội trường 8/9 | **AG-5 dựng ở nhà tối 6/9** bằng MCP |
| Ngày 8/9 tiêu vào việc dựng agent đầu tiên | Ngày 8/9 dồn cho **AG-0 và câu hỏi agent-gọi-agent**, cộng thời gian hỏi SA |
| Sàn: một agent chạy được | Sàn giữ nguyên; **mục tiêu nâng lên hai agent nói chuyện với nhau** |
| Câu hỏi số 4 cho SA: "Mentor có tạo được agent không" | Đã tự trả lời. **Câu thay thế: callable-action list của agent có nhận AGENT KHÁC không, hay chỉ Server Action?** |

Giả định rủi ro nhất còn lại của cả kiến trúc chính là câu thay thế đó — và nó cũng là thứ được chấm cao nhất (tiêu chí ưu tiên tương tác agent-to-agent, với điều kiện đội diễn giải được luồng).

## 5. Đã cập nhật ở đâu

`E:\Hackaton\Preparing\` — `CLAUDE.md` mục 5 · `06_OutSystems-ODC/00-nang-luc-va-gioi-han.md` · `02_Ke-hoach-va-prompt/00-plan-5-9-den-8-9.md` · `01_Boi-canh-va-chien-luoc/01-dinh-nghia-MVP-va-pham-vi.md` · hai file xlsx (Plan bản v2 có T01b và T12b; Prompt bản v2 có T01b, T16 dựng bằng MCP, T16b test bịa số, T18 hai nhánh, và 3 quy tắc mới).
