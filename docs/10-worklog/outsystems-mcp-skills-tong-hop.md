# Bộ skill `outsystems-mcp-skills` — tổng hợp năng lực (dùng cho hackathon)

**Nguồn:** `E:\Outsystems-mcp-skill` (README.md + docs/INTRO.md, SKILL-INDEX.md, TRIGGER-MAP.md, CONVENTIONS.md + 12 SKILL.md)
**Phiên bản repo:** v0.6 · MIT · Maintainer: Deniz Arin (OutSystems R&D) · Repo gốc: `github.com/denwx/outsystems-mcp-skills`
**Ngày tổng hợp:** 16/09/2026

---

## 1. Nó là cái gì

11 skill lõi + 1 skill companion tuỳ chọn, chạy **trên nền OutSystems MCP server** (`https://<tenant>.outsystems.dev/mcp`). Mỗi skill gói một quy trình ODC nhiều bước thành **một câu hỏi tiếng Anh tự nhiên** — không slash command, agent tự match theo trigger phrase.

Cấu trúc mỗi skill: `SKILL.md` (front-door + procedure) + `scripts/build.py` (Python stdlib thuần, không `pip install`) + `assets/templates` (HTML/Markdown) + `references/` với skill mang domain knowledge.

> ⚠️ **Đây là prototype R&D nội bộ, không phải sản phẩm.** README ghi rõ: không dùng cho công việc customer-facing production; có thể bị xoá/viết lại bất kỳ lúc nào; output phải coi là **bản nháp cần người review**.

---

## 2. Bảng năng lực đầy đủ (12 skill)

### Nhóm EXPLORE — hiểu cái đang có (rẻ, vài cent/lần)

| Skill | Ver | Làm được gì | Câu kích hoạt | MCP tool dùng |
|---|---|---|---|---|
| `outsystems-tenant-architecture` | 1.5.0 | Đồ thị HTML tương tác **toàn bộ tài sản tenant** (app web/mobile, AI agent, model connection, library, integration), filter theo type, dark theme OutSystems. Lần đầu 2–5 phút, cache sau đó ~5s | *"show me my tenant"*, *"tenant overview"*, *"architecture diagram"* | `env_list`, `app_list` |
| `outsystems-app-architecture` | 1.4.0 | Đồ thị HTML **một app**: UI flow + screen, server action + function, entity, static enum, structure, role, AI model connection, library deps. **1 app / 1 lần gọi** | *"architecture of [app]"*, *"what's inside [app]"* | `app_info`, `app_refs`, `context_screens/actions/entities/structures/roles` |
| `outsystems-ai-agent-landscape` | 1.2.0 | Dashboard HTML **mọi AI agent + model connection** trong tenant: phân tách provider (Bedrock / Azure OpenAI / Custom), audit **Trial vs Customer entitlement**, agent freshness, heuristic bắt agent tên "test/demo" lọt prod | *"AI inventory"*, *"audit my AI"*, *"which models are we using"* | `context_agents`, `context_connections` |
| `outsystems-dependency-impact` | 1.3.0 | **Reverse-dependency explorer** HTML: "ai gãy nếu tôi publish library X?". Scan 1 lần (20 asset ≈ 1–3 phút; 150 asset ≈ 9–25 phút), sau đó query miễn phí trên cache | *"who depends on [lib]"*, *"blast radius"* | `app_list`, `app_refs` |

### Nhóm BUILD — sinh / mở rộng app qua ODC Mentor (tốn tiền: ~$1–15/lần build)

> **Ghi chú trung thực từ repo:** Mentor được định vị chính thức cho *in-flow edit app có sẵn*. Dùng nó để dựng app greenfield end-to-end là **pattern thử nghiệm** các skill này chồng lên. Kết quả = draft scaffold, không phải app ship được.

| Skill | Ver | Làm được gì | Câu kích hoạt |
|---|---|---|---|
| `outsystems-design-to-app` | 1.3.0 | **Design → app**. 4 nguồn: Figma URL / ảnh screenshot / HTML mockup / **front-end code TSX-React (fidelity cao nhất)**. Trích design token + cấu trúc → soạn `spec.json` **map từng element sang OS UI block thật** (không phải CSS giả) gồm token, data model, sample data, chart series → drive Mentor theo **batch** → publish. Có **cổng bắt buộc block-mapping** trước khi bắn Mentor. Kèm ~30 file reference: catalogue OS UI (layouts, patterns, charts, maps, design tokens), 18 screen-archetype recipe (dashboard, kanban, wizard, master-detail, timeline, calendar, inbox…), 12 file gotcha | *"build this Figma in OutSystems"*, *"design to model"*, dán URL Figma/ảnh/HTML |
| `outsystems-spec-driven-build` | 1.2.0 | **Spec text → app**, không cần design. 3 mode: file spec markdown / **interview** / clone-from-example. Pre-flight validate những thứ Mentor hay hỏng (RBAC role theo từng screen, quan hệ entity, integration point), rồi drive Mentor kèm **anti-failure guardrail** (RBAC, UI default, terminology) | *"build a new app from this spec"*, *"create [app] from scratch"* |
| `outsystems-plan-to-mentor` | 1.4.0 | **Coverage-review + patch plan** đã có, đối chiếu PRD gốc, trước khi đưa cho Mentor. Nguồn plan thay thế được (`superpowers:writing-plans`, output của spec-driven-build, hoặc viết tay). Bắt buộc **ma trận coverage khoá theo ID** (BR-/UC-/C-) + checker stdlib tính verdict READY cơ học; artifact có version; vòng coverage 2 pass; xuất prompt Mentor-ready (paste hoặc MCP). Có **pre-plan mode**: chỉ có PRD đã duyệt → sinh capability-plan brief trước khi viết plan đầu tiên | *"review my plan against the PRD"*, *"make this plan Mentor-ready"* |
| `outsystems-mentor-copilot` | 1.3.0 | **Thư viện 11 task Mentor** dựng sẵn: quality review · perf audit · security review · accessibility · test generation · doc gap fill · refactor · AI model migration · add feature · demo data · **demo readiness**. Cộng **3 workflow nhiều bước**: audit-and-fix, add-feature-then-test, publish-after-mentor. Tự lo vòng polling, xuất báo cáo Markdown sạch | *"have Mentor audit [app]"*, *"run security-review on [app]"* |
| `outsystems-custom-code` | 1.1.0 | **C# External Logic cho ODC**: SDK attribute (`[OSInterface]`, `[OSAction]`, `[OSParameter]`, `[OSStructure]`, `[OSIgnore]`), kiểu dữ liệu được hỗ trợ, optional parameter, log qua `ILogger`, cấu trúc project, **ràng buộc runtime 95s / 1 GB RAM / 5.5 MB payload**, cấm signature `async` trên method `[OSInterface]`, unit test + integration test bằng container, deploy qua MCP. Cần .NET 8 SDK + Docker; **không cần Python** | *"build a C# library for OutSystems"*, *"ODC external logic"* |

### Nhóm OPERATE — ship, kiểm tra, tài liệu hoá

| Skill | Ver | Làm được gì | Câu kích hoạt |
|---|---|---|---|
| `outsystems-deploy-preview` | 1.2.0 | Preview HTML rủi ro khi promote Dev→Test / Test→Prod: revision nguồn vs đích, phân loại fresh-deploy vs update, khoảng cách revision, chỉ báo rủi ro. **Tính client-side** để né bug `deploy_impact` trả null vô hạn | *"is it safe to promote [app] to Test?"* |
| `outsystems-app-documentation` | 1.4.0 | Markdown doc cho 1 app (dán thẳng Confluence/Notion/README): overview, UI flow + screen, server action, data model (entity/enum/structure), security (role), dependency. **Render-only** — đọc lại arch-cache của `app-architecture` nên gần như miễn phí nếu đã chạy skill kia | *"generate docs for [app]"*, *"compliance docs for [app]"* |

### Companion tuỳ chọn

| Skill | Ver | Làm được gì |
|---|---|---|
| `outsystems-mentor-polling-behavior` ⬡ | 1.5.0 | Kỷ luật polling **Tier 1/2/3** cho mọi lần gọi Mentor. Tier 1 = đồng bộ, không poll (`app_list`, `context_*`); Tier 2 = poll 30s; Tier 3 = drain cursor rồi nghỉ 30s cho `mentor_get_run` (thay vì mặc định 500ms) → **giảm ~99% payload poll-response vào context** trong một run Mentor ~6 phút. Ghi telemetry từng poll (im lặng, không auto-render). 3 mode: (1) enforce kỷ luật, (2) sinh dashboard HTML benchmark theo yêu cầu, (3) tổng hợp + phân tích xu hướng cross-session kèm khuyến nghị. **Không sinh artifact app**; chỉ ghi nhận cái chạy *qua* nó |

---

## 3. Yêu cầu chạy & tương thích harness

| Hạng mục | Chi tiết |
|---|---|
| Harness hỗ trợ | ✅ **Claude Code** (rẻ nhất — harness tự lưu MCP response quá cỡ ra disk, chỉ inject path) · ✅ **Codex CLI** (cùng kết quả, first-pass đắt hơn với skill payload lớn) |
| Không hỗ trợ | ❌ Kiro CLI (đắt gấp 3–5×) · ❌ VS Code + Copilot (token hết hạn mỗi 2–5 phút) · ❓ Cursor / Windsurf / Continue.dev (chưa test, nhiều khả năng hỏng) |
| Thư mục cài | Claude Code: `~/.claude/skills/` (`%USERPROFILE%\.claude\skills` trên Windows) · Codex: `~/.agents/skills/` (**không** dùng `~/.codex/skills/`) |
| Phụ thuộc | Python 3.7+ (`python3`, stdlib thuần) · MCP server `outsystems` đã đăng ký + xác thực · `.NET 8 + Docker` riêng cho `custom-code` · Figma MCP riêng cho `design-to-app` nguồn Figma |
| Lệnh đăng ký MCP | `claude mcp add -s user --transport http --client-id service_studio --callback-port 7890 outsystems https://<tenant>.outsystems.dev/mcp` |
| Sau khi cài | **Phải restart session** — harness chỉ đọc skill description lúc khởi động |

**Cache:** `~/.claude/cache/outsystems-<skill>/<id>/`, TTL 1 giờ, freshness check bằng 1 call nhỏ (`limit:1` hoặc `app_info`) so `meta.json`. Nói "refresh" / "rebuild" để ép lấy mới.

**Cổng xác nhận chi phí:** design-to-app, spec-driven-build, mentor-copilot, dependency-impact đều hỏi "go / cancel" + ước tính chi phí **trước khi** tiêu tiền. Không bỏ qua được — cố ý thiết kế vậy.

---

## 4. Bộ MCP tool của OutSystems mà catalog này dùng (rút từ toàn bộ skill)

| Nhóm | Tool |
|---|---|
| Auth | `auth_status`, `authenticate`, `complete_authentication` |
| App / env | `app_list`, `app_info`, `app_refs`, `app_create`, `app_revisions`, `env_list`, `env_apps`, `env_app` |
| Context (đọc metadata app) | `context_screens`, `context_actions`, `context_entities`, `context_structures`, `context_roles`, `context_themes`, `context_agents`, `context_connections`, `context_search` |
| Mentor (session-based) | `mentor_start_session`, `mentor_create_asset`, `mentor_load_asset`, `mentor_prompt`, `mentor_get_run`, `mentor_cancel_prompt`, `mentor_publish`, `mentor_close_session` |
| Publish / deploy | `publish_status`, `publish_logs`, `deploy_start`, `deploy_impact` |
| External Logic | `extlib_upload`, `extlib_status`, `extlib_publish`, `extlib_logs`, `extlib_contents` |

---

## 5. Quirk API đã biết — cần nhớ khi tự viết flow (từ CONVENTIONS §7b)

1. **`context_*` chặn `limit` ở 100** — trên 100 trả HTTP 400. Luôn dùng `limit: 100`.
2. **`app_list` nhận `limit: 1000` bình thường**; nhưng **không truyền `limit`/`offset` thì chỉ trả 100 kết quả đầu** dù docstring nói "fetch every page".
3. **`app_refs` timeout với app có OML lớn** (ví dụ Banking Portal rev 574) → bọc timeout ngắn + cache khi thành công.
4. **`app_refs` không chịu được >2 call song song** — 3+ làm nghẽn queue upstream, mọi call đang bay (và ~10s call sau) đều timeout. Bắt buộc throttle 1–2 parallel, nghỉ ~5s giữa batch. Đây là lý do scan dependency toàn tenant chậm.
5. **`context_themes` trả payload rất lớn** (>150K ký tự) → bỏ qua nếu không cần.
6. **`deploy_impact` trả `{impacts:null, status:null}`** một cách nhất quán trên stage tenant → **đừng phụ thuộc tool này**; dùng pattern so sánh đồng bộ như `deploy-preview`.
7. **`context_screens` KHÔNG trả role-assignment theo từng screen** — chỉ có `isPublic`. Tuyệt đối **không được bịa** role cho screen (đã có field report bị sai: skill ghi mọi screen có role X trong khi Studio là "Accessible by: Everyone").
8. **`app_revisions` phân trang** giống `app_list` — tránh enumerate đầy đủ.
9. **Token OAuth hết hạn giữa session** → xử lý lỗi "requires re-authorization" tử tế, đừng loop. Trên session remote/SSH, callback page không load → bảo user dán full URL `localhost:7890/callback?…` rồi gọi `complete_authentication`.

---

## 6. Kỹ thuật tiết kiệm token (áp dụng được cho mọi flow ODC + AI của mình)

1. **Cố ý "oversize" request MCP nặng** để harness Claude Code tự lưu response ra disk, chỉ inject path → dữ liệu asset **không bao giờ vào context model**.
2. **Compact-Write** khi payload trả inline (<~25 KB): model chỉ ghi ra **đúng các field mà `build.py` đọc**, không bao giờ ghi lại full MCP shape. Mỗi SKILL.md có whitelist field riêng. Tiết kiệm được ~4–5K token/skill.
3. Không transform bằng Bash trong output của model — `build.py` lo hết. Không dùng `jq` (không chắc có sẵn).
4. Cache mạnh — chạy lại từ cache nên dưới ~3K token.
5. Gộp call độc lập vào **một message** (N MCP fetch + N Write + 1 Bash) — mỗi turn thừa tốn ~30s wall-clock.
6. Kỷ luật polling Tier 1/2/3 (xem `mentor-polling-behavior`) — đây là đòn giảm chi phí lớn nhất cho mọi thứ chạm Mentor.

---

## 7. Cách các skill compose với nhau

- `app-architecture` → nạp arch-cache → `app-documentation` dùng lại **miễn phí**.
- `spec-driven-build` xuất plan → `plan-to-mentor` coverage-check → Mentor.
- `mentor-polling-behavior` **không cạnh tranh** mà **bọc** mọi skill chạm Mentor (design-to-app, spec-driven-build, mentor-copilot, plan-to-mentor).
- **Conductor command** (layer trên catalog, không nằm trong repo): `change-app`, `demo-prep`, `ship`, `review-app`, `explore-app`, `tenant-audit`. Khi conductor và catalog skill cùng match → **conductor thắng**, đúng thiết kế.

**Companion ngoài repo, nên cài kèm:**

| Tool | Là gì | Dùng khi |
|---|---|---|
| `donnieprakoso/mcp-outsystems-docs` | MCP server riêng, **semantic search tài liệu ODC + O11**, chạy offline hoàn toàn (embedding local, tôn trọng license CC BY-NC-ND). Tool: `search_docs`, `get_doc`, freshness timestamp | Muốn tra doc OutSystems ngay trong session đang chạy skill — nhất là khi Mentor dựng ra feature mình chưa chắc |
| `PauloACRibeiro/portable-agent-skills` → `outsystems-mentor-implementation` | Skill companion của Paulo Ribeiro (OutSystems R&D): biến business intent thành **pseudocode Studio-native** (tên element thật, label parameter thật, ràng buộc runtime) rồi sinh prompt Mentor paste-safe. ~1 MB / ~80 file reference | Muốn sinh pseudocode tất định **trước** khi gọi Mentor. **Cần trước:** MCP server `outsystems-tech-content` + `workspace-knowledge-cc` |

---

## 8. Dùng gì cho hackathon "Agentic Health Claim Adjudication"

Đối chiếu với `claude/chien-luoc-hackathon-agentic-claim.md`:

| Việc trong lịch hackathon | Skill dùng được | Ghi chú |
|---|---|---|
| Dựng app ODC 4 module + Entity + seed (giai đoạn 3–6/9) | `outsystems-spec-driven-build` (mode spec file) | Spec đã có sẵn dạng `claude/build-01…06` → đóng gói thành 1 spec markdown là chạy được. Nhớ khai RBAC per-screen rõ trong spec — đó là thứ Mentor hay hỏng nhất |
| Màn hình chi tiết hồ sơ, hàng đợi, panel F56 (16–22/9) | `outsystems-design-to-app` | Nếu có prototype HTML (`claude/prototype-v6-1-changelog.md`) thì **đưa thẳng HTML/TSX vào** — đây là nguồn fidelity cao nhất, hơn Figma |
| Kiểm chứng "1 agent chạy trên tenant" ngày 8/9 | `outsystems-ai-agent-landscape` | Chụp dashboard agent + model connection làm bằng chứng cho slide; đồng thời audit Trial vs Customer entitlement — liên quan trực tiếp rủi ro **EAP chưa bật** (mục 6.1 chiến lược) |
| Chuỗi 14 bước tính tiền là Server Action deterministic | `outsystems-custom-code` *(nếu cần)* | Chỉ dùng nếu phần tính toán vượt khả năng Server Action. Nhớ trần **95s / 1 GB / 5.5 MB** — payload IDP nhiều trang có thể chạm 5.5 MB |
| Audit trước pitch: quality / security / accessibility / **demo readiness** | `outsystems-mentor-copilot` | Task `demo-readiness` chạy 1 lần trước dry-run tenant sạch (30/9–6/10). Task `demo-data` sinh seed cho 3 kịch bản |
| Deck + tài liệu kiến trúc cho ban giám khảo | `outsystems-app-architecture` → `outsystems-app-documentation` | Đồ thị HTML tương tác **rất hợp để chiếu live** trong phần "Solution & agent architecture"; Markdown dán thẳng vào deck/appendix. Chạy arch trước, doc sau để khỏi tốn tiền hai lần |
| Trước khi promote lên tenant demo | `outsystems-deploy-preview` | Sanity check trước ngày pitch |
| Kiểm soát chi phí khi chạy Mentor nhiều lần trong 4 tuần | `outsystems-mentor-polling-behavior` | Cài ngay từ đầu, không cài sau. Dashboard cross-session cho biết Mentor đang nhanh/chậm/đắt lên hay xuống |
| Tra tài liệu ODC khi Mentor sinh ra thứ lạ | `mcp-outsystems-docs` (companion ngoài) | Offline, không rò dữ liệu hồ sơ bồi thường ra ngoài |

### Ba cảnh báo cần nhớ cho hackathon

1. **Không demo live cái gì do Mentor sinh mà chưa review tay.** Repo nói thẳng: output là draft scaffold. Với hồ sơ bảo hiểm, một logic sai = mất điểm nặng ở phần "deterministic money".
2. **Chi phí thật:** mỗi lần build qua Mentor ~$1–15. Lịch 4 tuần có nhiều vòng lặp → bật `mentor-polling-behavior` + tôn trọng cổng xác nhận.
3. **PII trong chứng từ** (rủi ro BC-1) — `app-documentation` và các dashboard HTML sẽ kết xuất tên entity/field ra file; che PII trước, và không đưa dữ liệu thật vào bất kỳ nguồn nào rời máy.

---

## 9. Ba việc làm ngay

1. Cài vào `%USERPROFILE%\.claude\skills\` rồi **restart Claude Code**, gõ `/skills` xác nhận đủ 12 entry `outsystems-*`.
2. Đăng ký MCP `outsystems` trỏ về tenant BnK (`bnk.outsystems.dev`) — xem lại `claude/odc-mcp-huong-dan-tao-connection.md` để không làm lại từ đầu, và chạy OAuth một lần.
3. Chạy thử **rẻ tiền trước**: `outsystems-tenant-architecture` → `outsystems-ai-agent-landscape`. Hai cái này tốn vài cent, cho ra ảnh chụp tenant hiện tại dùng ngay cho slide, và xác nhận MCP + auth đã thông trước khi động vào nhóm Mentor tốn tiền.
