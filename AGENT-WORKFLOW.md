# Cách làm việc trong repo này — dành cho agent (Claude Code + `outsystems-mcp`)

Đọc `../CLAUDE.md` trước nếu chưa đọc. File này nói **làm thế nào**, không nói **làm gì**.

---

## 0. Vào việc trong 3 bước

```
1. openspec/project.md            → 3 change đang mở, cái nào chặn cái nào
2. changes/active/<x>/tasks.md    → tick tiếp từ chỗ dừng, đừng bắt đầu lại
3. specs/<capability>/spec.md     → ràng buộc bất biến của vùng sắp đụng vào
```

**Không bắt đầu một việc không có trong `changes/active/`.** Nếu việc mới phát sinh: tạo thư mục change trước, viết `tasks.md`, rồi mới làm.

---

## 1. Ba lớp, và luật duy nhất

| Thư mục | Là gì | Sửa khi nào |
|---|---|---|
| `specs/<cap>/spec.md` | Nguồn chân lý ổn định | Chỉ khi archive một change |
| `changes/active/<name>/` | Delta đang chạy | Tự do |
| `changes/archive/<name>/` | Đã xong | Không sửa |

> **Một thay đổi về hành vi hệ thống phải tồn tại ở `changes/active/` trước khi nó tồn tại trong code.**

Lý do có luật này: *cổng tự duyệt 4 điều kiện* sống song song với một câu "chuẩn là 3" trong `build-00` suốt ~90 rev mà không nằm ở đâu cả. Phân xử 21/09: **code đúng, tài liệu sai**.

**Vòng đời:** `mkdir changes/active/<ten>/` → `tasks.md` → (nếu đổi spec) `specs/<cap>/spec.md` chứa **đúng đoạn khác biệt** → làm → merge delta vào Project doc → `mv` sang `archive/`.

---

## 2. Verify ở đây KHÔNG phải git diff

OpenSpec giả định code nằm trong git. **Ở đây không có repo, không có diff, không có PR** — code là một app ODC trên cloud do Mentor sửa.

Thứ thay thế: **runtime audit + screenshot** trong `../output/`. Quy ước đã chạy 9 vòng và hoạt động tốt:

```
output/bnk-dev-<app>/
  runtime-audit-rev<N>.md     bảng 16 tiêu chí + findings + method
  shots-rev<N>/               screenshot .jpg, zoom .png cho defect
```

**Sau mỗi batch Mentor:** chụp màn → probe DOM → ghi audit → so với shots của rev trước.
Mentor **regress lặng lẽ** — màn Checkpoint queue hỏng ở rev 41, phát hiện ở rev 44. Chụp màn là cách rẻ nhất bắt được.

**Không báo "xong" khi chưa có screenshot.** Report của Mentor không phải bằng chứng.

---

## 3. Skill nào cho việc nào

12 skill `outsystems-*` cài ở `%USERPROFILE%\.claude\skills\`. Kích hoạt bằng **câu tiếng Anh tự nhiên**, không slash command.

### Rẻ — đọc cái đang có (vài cent)

| Việc | Skill | Câu gọi |
|---|---|---|
| Xem toàn tenant | `outsystems-tenant-architecture` | *"show me my tenant"* |
| Xem bên trong 1 app | `outsystems-app-architecture` | *"architecture of ClaimMVP"* |
| Kiểm agent + model connection | `outsystems-ai-agent-landscape` | *"AI inventory"* |
| Ai gãy nếu publish X | `outsystems-dependency-impact` | *"who depends on ClaimMVP"* |

> `app-architecture` nạp arch-cache → `app-documentation` dùng lại **gần như miễn phí**. Chạy arch trước, doc sau.

### Tốn tiền — sinh/sửa app qua Mentor (~$1–15/lần)

| Việc | Skill | Lưu ý |
|---|---|---|
| Entity + seed từ spec | `outsystems-spec-driven-build` | **Khai RBAC role cho TỪNG screen ngay trong spec** — Mentor hỏng chỗ này nhiều nhất, và `context_screens` không trả role per-screen nên không có gì tự sửa hộ |
| Màn hình từ design | `outsystems-design-to-app` | Nguồn fidelity: **front-end code > HTML mockup > Figma > ảnh**. Có **cổng block-mapping bắt buộc** trước khi bắn Mentor — không bỏ qua |
| Review plan trước khi đưa Mentor | `outsystems-plan-to-mentor` | Ma trận coverage khoá theo ID |
| Audit / demo-readiness / demo-data | `outsystems-mentor-copilot` | `demo-readiness` chạy 1 lần trước dry-run |
| C# External Logic | `outsystems-custom-code` | Trần **95 s / 1 GB / 5.5 MB payload** |

### Ship & tài liệu

| Việc | Skill |
|---|---|
| An toàn để promote? | `outsystems-deploy-preview` |
| Markdown doc cho deck | `outsystems-app-documentation` |
| **Kỷ luật polling** | `outsystems-mentor-polling-behavior` — **bật TRƯỚC batch đầu, không bật sau.** Giảm ~99 % payload poll vào context |

---

## 4. Việc Mentor KHÔNG được làm

Mentor là agent. Bốn quy tắc kiến trúc ở `../CLAUDE.md` §2 áp cho nó y như mọi agent khác.

| Không cho Mentor làm | Làm thế nào thay vào đó |
|---|---|
| Sinh **Entity** | Spec → `outsystems-spec-driven-build`, có review tay |
| Sinh **logic tiền** — 14 bước, cổng tự duyệt, sổ hạn mức, máy trạng thái | Viết tay trong Studio. Cho Mentor **gọi**, không cho **sinh** |
| Quyết **RBAC** | Khai trong spec, kiểm tay trong Studio |
| Chọn **cơ chế song ngữ** | Chốt một lần, áp cho mọi màn |
| Quyết **định dạng tiền** | Một Server Action `FormatVND` dùng chung |

Mentor được làm: **màn hình, binding, layout, block** — và kết quả vẫn phải qua screenshot.

---

## 5. Quirk MCP phải nhớ

| # | Quirk | Hệ quả |
|---|---|---|
| 1 | `context_*` chặn `limit` ở **100** — trên 100 trả HTTP 400 | Luôn `limit: 100` |
| 2 | `app_list` **không truyền `limit`/`offset` thì chỉ trả 100 đầu** dù docstring nói "fetch every page" | Truyền `limit: 1000` |
| 3 | `app_refs` **timeout với app OML lớn** | Bọc timeout ngắn + cache khi thành công |
| 4 | `app_refs` **không chịu nổi >2 call song song** — 3+ làm nghẽn queue upstream | Throttle 1–2 parallel, nghỉ ~5 s giữa batch |
| 5 | `context_themes` trả **>150 K ký tự** | Bỏ qua nếu không cần |
| 6 | `deploy_impact` trả `{impacts:null, status:null}` nhất quán trên stage | **Đừng phụ thuộc.** Dùng pattern so sánh của `deploy-preview` |
| 7 | `context_screens` **KHÔNG trả role per-screen**, chỉ `isPublic` | **Tuyệt đối không bịa role.** Đã có field report sai vì việc này |
| 8 | `app_revisions` phân trang như `app_list` | Tránh enumerate đầy đủ |
| 9 | Token OAuth hết hạn giữa session | Xử lý lỗi "requires re-authorization" tử tế, **đừng loop**. Session remote: dán full URL `localhost:7890/callback?…` rồi gọi `complete_authentication` |

**Cache:** `~/.claude/cache/outsystems-<skill>/<id>/`, TTL 1 giờ. Nói *"refresh"* / *"rebuild"* để ép lấy mới.

**Cổng xác nhận chi phí:** `design-to-app`, `spec-driven-build`, `mentor-copilot`, `dependency-impact` đều hỏi go/cancel kèm ước tính **trước khi** tiêu tiền. Cố ý thiết kế vậy — **đọc kỹ rồi mới "go"**.

---

## 6. Tiết kiệm token

1. **Gộp call độc lập vào một message** (N MCP fetch + N Write + 1 Bash) — mỗi turn thừa tốn ~30 s wall-clock.
2. **Compact-Write**: khi payload trả inline, chỉ ghi ra **đúng field mà `build.py` đọc**, không ghi lại full MCP shape.
3. Không transform bằng Bash trong output của model — `build.py` lo hết. Không dùng `jq`.
4. Chạy lại từ cache nên dưới ~3 K token. Nếu tốn hơn, cache hỏng — kiểm trước khi chạy tiếp.
5. Kỷ luật polling Tier 1/2/3: Tier 1 đồng bộ không poll (`app_list`, `context_*`) · Tier 2 poll 30 s · Tier 3 drain cursor rồi nghỉ 30 s cho `mentor_get_run`.

---

## 7. Bài học từ pass UI — đọc trước khi mở change UI mới

1. **Luôn dùng widget `Link`, không bao giờ `Container` OnClick.** OnClick Mentor gắn lên Container/Table row **không bao giờ chạy** — lỗi này làm portal mất hẳn đường vào màn chi tiết tới rev 3.
2. **`ColumnsMediumLeft` là block hẹp-bên-TRÁI.** Bố cục 2/3–1/3 dùng `ColumnsMediumRight`.
3. **Skip-link không được dùng `href="#…"`** — router SPA điều hướng về màn gốc. Dùng client action OnClick.
4. **Rule `.cc-table td a { inline-flex }` bóp chữ trong ô bảng.** Kiểm lại bảng sau mỗi lần đụng tap-target.
5. **Icon dùng font riêng `ccicons`**, không `IIcon`/Phosphor — chỗ này bảng map block ban đầu sai.
6. **Theme:** ghi đè biến OS UI có sẵn ở `:root` trên stylesheet của **THEME**, không khai biến custom trên stylesheet của Screen. Làm đúng thì dark mode của ODC chạy sẵn.
7. **Quyền là vắng mặt, không phải `Enabled = False`.** Nút không được phép phải **không tồn tại trong DOM**, và Server Action tự kiểm lại vai + scope.
8. **Sau mỗi lượt Mentor đụng dây flow, bắt Mentor liệt kê node sequence.** Báo cáo "done" của nó không đủ. Đã bắt được một lần: lượt edit để nhánh True của `BootstrapDemoData` thành `ClearDemoData → Seed_BenefitSnapshots → End` — **xoá dữ liệu rồi không seed lại**, publish sạch 0 lỗi. Một lượt read-only liệt kê node là rẻ hơn một buổi truy tìm dữ liệu biến mất.
9. **Cảnh giác sentinel bằng 0.** Model này đang có hai số 0 mang hai nghĩa **ngược nhau**: `CountLimit = 0` là *không giới hạn*, `AnnualLimit = 0` trên dòng con là *included*. Một sentinel hợp lệ về số học **không bao giờ fail to tiếng** — `build-01` §8.1. Mỗi lần thêm một cột số có nghĩa đặc biệt, hỏi: nếu ai đó để mặc định thì nó có vô tình đúng không?
10. **Hai trường cùng nghĩa "active" là một cái bẫy — và dự án này đã dính bốn lần.** `GetTerms` lọc `IsActive = True` (cột kỹ thuật của static entity, **True trên mọi dòng**) thay vì `Status = "ACTIVE"` (trường nghiệp vụ `termStatus`: DRAFT/PENDING_ACTIVE/ACTIVE/WITHDRAWN) ⇒ 4 term `DRAFT` lọt vào engine, và **bộ lọc không lọc gì cả**. Cùng họ với: `level` (độ sâu cây) vs `benefit level` (plan gắn vào hợp đồng) → D3 đổi tên thành `depth` · `state` (độ đầy đủ bộ hồ sơ) vs `outcome` (kết cục xử lý) · tra cứu khoá theo `Label` thay vì `Id`.
    **Luật:** khi hai trường trên cùng một entity đọc lên nghe giống nhau, viết ra **nghĩa** của từng cái ngay tại chỗ lọc, và hỏi *"bộ lọc này có bao giờ loại được dòng nào không?"* Một bộ lọc không bao giờ loại gì trông giống hệt một bộ lọc đúng.
11. **Một bộ test xanh vì fixture trùng nhau trông giống hệt một bộ test đúng.** `Seed_ChainTestFixtures` không bao giờ xoá (xoá `Claim` trước con ⇒ FK nuốt lỗi im lặng) ⇒ **185 hồ sơ `TST-` chồng nhau, 4–5 bản mỗi mã**. 46 test vẫn xanh suốt vì các bản giống nhau; nó chỉ vỡ khi `T-CHAIN-05` **làm đổi** một bản.
    Cùng họ với luật 10, nhưng ở tầng nguy hiểm hơn: luật 10 làm sai một kết quả, luật 11 làm **mọi kết quả mất ý nghĩa**.
    **Luật:** fixture phải **dọn trước khi tạo**, thứ tự xoá là **con trước cha**, và sau mỗi lần sửa cơ chế fixture phải **chạy lại toàn bộ trên fixture sạch** — xanh trên fixture cũ không chuyển sang được.
12. **Không bịa hành vi ở chỗ spec im lặng.** Bước 9 vượt hạn mức ngày: `HOLD`, **không cắt pro-rata**, vì A5 không quy định cắt pro-rata. Cám dỗ là chọn một hành vi "hợp lý"; nhưng một hành vi hợp lý tự nghĩ ra **không phân biệt được** với một hành vi đã đặc tả, và sáu tháng sau không ai biết con số đó từ đâu ra.

---

## 8. Ba câu trả lời cho câu hỏi hay gặp

**"Spec đầy đủ ở đâu?"** — Project "Hackaton" trên claude.ai, doc `claude/build-00…06`. `specs/<cap>/spec.md` ở đây là bản tóm ràng buộc + con trỏ, cố ý không phải bản sao (400 KB, hai bản sẽ lệch). Nguồn cấp trên (A1–A5, D1–D3) nằm trên đĩa ở `../Preparing/04_Nghiep-vu-va-du-lieu/`.

**"BRD nói khác spec thì theo cái nào?"** — **A5/D2 thắng**. 8 xung đột đã phân xử ở Project doc `build-00` §5. Nếu gặp xung đột thứ 9: mở change, đừng tự quyết.

**"Xong rồi, báo được chưa?"** — chưa, nếu chưa có screenshot trong `../output/` và chưa tick hết `tasks.md`.

13. **Sửa một nửa còn tệ hơn không sửa.** 21/09, `Seed_AssessmentAndTerms`: lượt N1-b xoá 9 node `CreateCalcSnapshot` hồ sơ B — báo xong, 0 lỗi, **không đọng** (đếm vẫn 27). Lượt N1-c xoá `CrCRB` + 2 Assign quanh nó — **đọng thật** (ChainRun 4 → 3). Kết quả: hồ sơ B còn node tạo `CalcSnapshot` trỏ vào biến `ChainRunIdB` mà **không còn node nào gán** ⇒ sẽ ghi `ChainRunId = 0`. **Validation vẫn 0 lỗi**, vì một local variable không được gán là hợp lệ về cú pháp. Đây là trạng thái tệ hơn cả hai đầu mút. Luật: sau mỗi lượt sửa cấu trúc, **đếm lại bằng một lượt read-only riêng**; nếu con số không đổi thì lượt đó không đọng — dừng cơ chế, và **quay lại đầu mút an toàn trước khi thử cách khác**, đừng chồng lượt tiếp theo lên một nửa. Mentor giải thích mâu thuẫn bằng một chuyện không thể xảy ra (Assign node "bị đánh số lại" thành Execute Action node) — nó bịa để lấp, không phải báo sai. Kèm theo: **"save" không phải "publish"** — Mentor báo "saved, revision 943" trong khi app đang chạy ở rev 162. Số revision model và số revision app là hai bộ đếm khác nhau; chỉ con số thứ hai có nghĩa. Một bản save sửa dở sẽ theo lần publish kế tiếp của bất kỳ ai lên môi trường.

14. **Mentor sửa model, không deploy.** 21/09: Mentor báo "saved, revision 943" và sau đó nói thẳng Model API không có publish. Số revision nó báo là **revision của model**, không phải revision của app đang chạy — hai bộ đếm khác nhau, và chỉ số thứ hai có nghĩa. Một model đã save nhưng chưa publish là một thay đổi **vô hình ở runtime mà vẫn đi theo lần publish kế tiếp của bất kỳ ai**. Luật: sau mỗi lượt Mentor, ghi rõ *đã save* hay *đã publish*; nếu chỉ save thì **không được coi là xong** và không được để treo qua đêm. Publish là việc của outsystems MCP (Claude Code) hoặc 1-Click Publish trong ODC Studio.

15. **Một readout không tách quan sát khỏi suy diễn là một readout không dùng được.** Readout 21/09 làm đúng: nói thẳng "không tool nào đọc được SQL trên DB ODC", liệt kê cái **nhìn thấy** (RunNo lớn nhất + số dòng CalcSnapshot của đúng ChainRunId đó, trên 54 hồ sơ) và tách riêng cái **suy ra** ("nếu RunNo luôn = max+1 và chưa dòng nào bị xoá thì tổng là 16 — chưa kiểm"). Nhờ tách như vậy mà bắt được chỗ không khớp: hồ sơ C phải có 9 dòng CalcSnapshot, quan sát ra 0. Nếu gộp hai loại làm một thì con số 16 đã thành "số liệu" và chỗ lệch đã chìm. Luật: mọi readout ghi rõ **đọc bằng gì**, và mỗi con số mang nhãn *quan sát* hay *suy ra*. Con số suy ra không bao giờ được dùng làm căn cứ quyết định — xem rule 13, nơi một con số bịa ("27") đã làm hỏng một ngày.

16. **Con số Mentor tự báo không phải bằng chứng. Diff với một revision đã publish mới là.** 21–22/09, cùng một action `Seed_AssessmentAndTerms`, số node Mentor báo qua các session: **97 · 170 · 175 · 187 · "30" · "27" · "18"**. Bảy con số cho một thứ. Mỗi lần đều kèm một câu chuyện mạch lạc, và tôi đã ba lần đổi kết luận theo chúng: tin lượt xoá đọng → kết luận "chưa từng xoá, tất cả là bịa" → hoá ra **có xoá thật, ở rev 165**. Luật: **số node Mentor tự báo không so được giữa hai session**, nên không bao giờ dùng nó làm căn cứ lật một kết luận. Cách duy nhất đã chứng minh được: **mở từng revision đã publish, liệt kê node thật của từng revision, rồi so** — và nói rõ phần nào KHÔNG so được (màn hình, entity, aggregate, site property không so được qua stream). Kèm theo: một kết luận chỉ được lật bằng bằng chứng **mạnh hơn** bằng chứng đã dựng nên nó, không phải bằng bằng chứng mới nhất.

17. **Một bộ test phụ thuộc vào việc ai chạy gì trước đó thì không phải bộ test.** 22/09: cùng rev 166, run #12 ra 45/46 và run #13 ra 46/46 — khác nhau ở **dữ liệu**, không ở code. Một lần bấm "Re-run demo seed" đã đặt `PremiumPaid = True` lên `FX-STD-UNPAID`, đúng fixture mà `T-CH01-c` dựng ra để kiểm nhánh chưa đóng phí. Rule 11 nói *dọn trước khi tạo*; chưa đủ — luật đầy đủ là **dọn và dựng lại fixture ngay trước mỗi lần CHẠY**, để một lần chạy xanh luôn có nghĩa là cái nó nói. Và khi cài cơ chế tự bảo vệ đó, **nghiệm thu bằng cách cố ý làm bẩn rồi chạy hai lần liên tiếp** — nếu không, ta chỉ đang làm tay đúng cái mà cơ chế đáng lẽ tự làm, và không biết nó có chạy không.

18. **Lọc log theo traceId, không theo tên action.** 22/09: `Seed_AssessmentAndTerms` ghi *"finished successfully"*, handler `AllExceptions → LogError` của nó không ghi một dòng nào trong 30 giờ. Cùng `traceId` đó lại có một Error thật: `Seed_ResetDemoClaimData → DeleteChainRun: 23503` — FK từ `LedgerPosting`. Tìm theo tên action thì không bao giờ thấy, vì action báo thành công **là action khác** với action lỗi. Luật: khi một thứ sai ở runtime mà action liên quan báo thành công, lấy `traceId` của lần chạy đó rồi đọc **toàn bộ** dòng log cùng trace, mọi severity, mọi action.

19. **Một màn hình đọc "bản ghi mới nhất" sẽ trống khi bản ghi mới nhất rỗng, dù dữ liệu còn nguyên.** F48 đọc `ChainRun` mới nhất của hồ sơ. Hồ sơ 0003 có đủ 9 dòng `CalcSnapshot` trong DB nhưng tab Calculation trống, vì run mới nhất của nó là một run rỗng — và ta đã mất một buổi đi tìm "dữ liệu bị mất" không hề mất. Áp cho **mọi** hồ sơ: một lần bấm nhầm tạo run rỗng trên hồ sơ demo là màn hero trắng trơn giữa lúc pitch. Luật: màn đọc bản ghi mới nhất phải đọc bản ghi mới nhất **có dữ liệu**, hoặc nói rõ "run này chưa có dữ liệu bước" thay vì hiện bảng trống.

20. **Tên node Mentor báo không phải định danh ổn định.** 22/09, `Seed_BatchB_Backfill`: trước khi sửa, `assignNode2` là node gán `PremiumPaid` trong vòng Policy; sau khi xoá 3 node, `assignNode2` là một Assign **khác hẳn**, trong vòng Member. Tên `assignNodeN` nhiều khả năng đánh theo thứ tự nên trượt sau mỗi lần thêm/xoá. Hệ quả trực tiếp: một prompt viết *"xoá assignNode2"* dựa trên lượt đọc trước có thể xoá **đúng tên, sai node**. Luật khi viết prompt: định danh node bằng **việc nó làm** (*"the node that writes `Policy.PremiumPaid`"*), hoặc bằng **tên cộng nội dung** làm khoá kép; và mọi lượt sửa phải nằm ngay sau lượt đọc của chính nó, không dùng lại kết quả đọc của lượt cũ. Nghiệm thu cũng phải phát biểu theo hành vi (*"không node nào ghi `Policy.PremiumPaid`"*), không theo tên node.
