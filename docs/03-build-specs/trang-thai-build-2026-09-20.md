# Trạng thái build — 20/09/2026

**Vai trò:** nguồn chân lý về **tiến độ**. (`build-00` vẫn là nguồn chân lý về **chuẩn và phạm vi**.)
**Thay thế phần tiến độ của:** `plan-5-8-9-truoc-hackathon.md` (đã qua), `prototype-v0-5-len-outsystems-lo-trinh.md` §3 và §6, `build-00` §8 và §10, `build-06` §0.2 và §0.3.
**Nguồn:** audit runtime chạy thật trên trình duyệt đã đăng nhập — `E:\Hackaton\output\bnk-dev-claimmvp\runtime-audit-rev90.md`, `...\bnk-dev-myinsurance\runtime-audit-rev7.md`, `...\review-vs-prototype-v05.md`.

> ### ⚠️ Cập nhật 21/09/2026 — đọc trước §3, §4, §5, §6
>
> **(a) Cổng tự duyệt: chuẩn là 4 điều kiện, không phải 3.** Rủi ro #2 bên dưới (🔴) đã **đóng và bị đảo chiều**. PO chốt 21/09: **BRD đúng, 4 điều kiện**; câu "chuẩn là 3" trong `build-00` §4 #7 là câu sai và đã được sửa tại nguồn. Màn Approval đang dựng theo bộ 4 ⇒ **không sửa UI, không sửa BRD**. Nguồn chuẩn cho nội dung 4 điều kiện là **`build-04` §3.1**. Còn một việc mở, xem (d).
>
> **(b) Số unit test là 71, không phải 65.** `build-04` §9 = 46 + 7 + 7 + 11. Mọi chỗ ghi 65 bên dưới đọc là 71.
>
> **(c) Chuỗi 14 bước ĐÃ XONG 21/09** — sớm hơn ngân sách 22–25/09 bốn ngày. `RunAdjudicationChain` chạy 719 ms, **46/71 test pass**, hồ sơ `CLM-2026-0001` ra `2.000.000 → 1.880.000` **do máy tính, không hard-code**. Việc #2 và #3 ở §3 và mục 4 ở §5 coi như đã xong. Còn 25 test: `T-CH14` ×6 · `T-SEED` ×11 · `T-GATE` ×7 · `T-CH11-c/d` thủ công.
>
> **(d) VIỆC MỞ MỚI — "4 điều kiện NÀO?"** Con số đã chốt, **danh sách thì chưa**. `build-04` §3.1 liệt G1 tiền & không có gì bị cắt · G2 chứng từ · G3 lâm sàng & không có điểm kiểm mở · G4 gian lận. Audit rev 47 đọc màn Approval lại thấy **điều kiện #2 trên màn là "đề xuất chờ người duyệt"** — thứ mà doc xếp vào G3. Hai bộ khác nhau, cùng đếm ra 4. Phải đọc `EvaluateAutoApprovalGate` trên tenant, **lấy code làm chuẩn**, rồi sửa tài liệu theo — trước khi viết 7 test `T-GATE`.

---

## 1. Thay đổi lớn nhất so với mọi kế hoạch cũ

> **Lớp giao diện (lớp C) đã xong, trên hai app riêng biệt.** Kế hoạch cũ giả định UI là phần việc phía trước; thực tế nó đã ở phía sau. Toàn bộ quỹ thời gian còn lại dành cho **logic nghiệp vụ + IDP + agent**.

| App | Vai trò | Màn hình | Rev | Điểm audit |
|---|---|---|---|---|
| **ClaimMVP** | Console cho cán bộ | 10 màn console + nhóm Technical trace | 91 | 86 % — Market Leading |
| **MyInsurance** | Portal cho khách | 7 màn khách | 9 | 86 % — Market Leading |

`https://bnk-dev.outsystems.app/MyInsurance` · key `<redacted-app-key>`

**Tách app xảy ra ở rev 87:** 7 màn Portal, `PortalMenu`, chatbox portal và role `Customer` đã bị xóa khỏi ClaimMVP. MyInsurance là bản clone rút gọn, **đọc Entity public của ClaimMVP và gọi Service Action của ClaimMVP** — không có data model thứ hai.

Service Action đang được MyInsurance tiêu thụ: `Portal_GetCurrentMember2` · `Portal_SubmitClaim2` · `Portal_SendSupplement2` · `GetLedgerCounters2` · `GetCalcSnapshotData2` · `FormatVND`.

---

## 2. Đã xong — đối chiếu với 6 batch trong lộ trình v0.5

| Batch | Nội dung | Trạng thái |
|---|---|---|
| 1 | App shell + sidebar + login + worklist (F42) | ✅ — sidebar 264 px, top bar, badge, footer avatar |
| 2 | Detail tab 1–4: general, docs, costs, events | ✅ — kể cả luồng quyết định thật trên tab costs |
| 3 | Detail tab 5–8: calc, settle, approval, history | 🟡 — calc thiếu mô tả rule; settle sai chia `Claimed` theo benefit |
| 4 | Dashboard (F54) + ops queue (F53) | ✅ |
| 5 | Policy, terms, checkpoints, ledger, admin | ✅ — ledger 🟡 (counter map 200 ↔ OUTPATIENT, used = 0) |
| 6 | Portal khách | ✅ — nay là app MyInsurance riêng |
| — | `trace` (AgentRun) | ✅ — màn `AgentTrace` đã dựng, đọc từ Entity |

**Ngoài kế hoạch, đã có thêm:**

- Luồng quyết định khuyến nghị **chạy thật end-to-end**: Confirm / Reject → popup lý do bắt buộc ≥ 10 ký tự → `DecideRecommendation` → refresh + toast. Đã kiểm ở rev 36 (một lần Confirm thật ghi `NOTREL` lên dòng 4).
- `AcknowledgeLowConfidenceLines` cho hồ sơ C.
- Nút **"Re-run demo seed"** trên màn Administration — cần cho demo, giữ lại.
- `FormatVND` đã có và dùng chung ở cả hai app (`2.000.000 ₫`, `3.128.000 ₫`, `18.000.000 ₫`).
- Chatbox trợ lý ở cả console và portal (hiện là stub, xem §4).
- Bộ icon riêng `ccicons` (woff2) + theme authored, dark sidebar teal `#0C3036`, accent mint `#35D3A3`.

**Kỷ luật kiến trúc vẫn giữ:** không Entity nào mới được Mentor đẻ ra; **tiền chỉ chảy qua `RunAdjudicationChain`**; mỗi màn có role tường minh (màn `SeedAdmin` Mentor tự thêm ở rev 44 đã bị xóa ở rev 45).

---

## 3. Chưa xong — đây là toàn bộ việc còn lại

| # | Việc | Nguồn chuẩn | Ghi chú |
|---|---|---|---|
| 1 | ~~**Verify data model** so với `build-01`~~ | `build-01` | ✅ **xong 21/09** — đợt A và B, static entity chuẩn hoá, 4 đầu vào của chuỗi, cây quyền lợi A5 51 dòng × 3 plan |
| 2 | ~~**Chuỗi 14 bước theo A5**~~ | `build-04` §1.1 | ✅ **xong 21/09** — 14 Server Action theo đúng thứ tự A5, chạy 719 ms |
| 3 | **71 unit test** chuỗi tính tiền *(không phải 65)* | `build-04` §9 | 🟡 **46/71 pass.** Còn `T-CH14` ×6 · `T-SEED` ×11 · `T-GATE` ×7 (chờ (d)) · `T-CH11-c/d` thủ công |
| 4 | ~~**Seed đầy đủ** 215 bản ghi sản phẩm~~ | `build-04` §5–6 | ✅ **xong 21/09** |
| 5 | **REST Consume IDP** thật (`auth/login`, `user/me`, `request/sync`) | `build-02` | 🔴 Chưa bắt đầu — **chưa có cả change folder**. Đây là việc lớn nhất còn lại |
| 6 | `IdpParseExtraction` + tính confidence | `build-03` | 🔴 |
| 7 | **`ReplayMode`** + JSON đã ghi | `build-00` §10 | 🔴 Đường lùi khi mạng hỏng ngày pitch |
| 8 | **AG-2 trên Agent Workbench** gọi `RunAdjudicationChain`, ghi `AgentRun` | `build-05` | 🔴 **Sàn không được thủng.** Màn trace đã có, chỉ thiếu agent thật ghi vào. Kèm: description mọi `A5StepNN_*` phải mở đầu *"bước nội bộ, không phải tool"* |
| 9 | `POST /request/compare` lấy số accuracy thật | `build-00` §7 | 🔴 Việc đáng giá nhất cho phần ROI của pitch |
| 10 | Áp **53 message nguyên văn** + 4 mã nguyên nhân còn thiếu câu | `build-06` §5, §9 | 🟡 **2 câu `COPAY` và `CAP_VISIT` đã được PO duyệt 21/09** (`build-00` §6). `DEDUCT`/`LIMIT_TOTAL` đẩy đợt F |
| 11 | **Grep portal** tìm mã nội bộ rò ra màn khách | `build-06` §10 | 🔴 13 `claimState` + 7 mã chứng từ + `REQUIRED/EXPECTED/OPTIONAL` + 12 `lossType` — không mã nào được xuất hiện trên MyInsurance |
| 12 | **Deck + kịch bản pitch** | — | 🔴 **Ở con số 0.** Cùng với #5 đây là đường găng thật |

---

## 4. Rủi ro mở — xếp theo mức đắt khi phát hiện muộn

**1. ~~Data model chưa được đối chiếu với `build-01`~~.** ✅ **ĐÓNG 21/09** — đợt A và B xong, delta đã merge.

**2. ~~Màn Approval đang hiện 4 điều kiện, chuẩn là 3~~.** ✅ **ĐÓNG 21/09 — và đảo chiều.**
Chuẩn là **4**. PO chốt: BRD đúng, câu "3 điều kiện" trong `build-00` §4 #7 sai và đã sửa tại nguồn. UI không phải sửa. Nguồn chuẩn cho nội dung 4 điều kiện là `build-04` §3.1.
**Nhưng mở ra rủi ro mới #10 bên dưới.**

**3. Settlement chia sai `Claimed` theo benefit.** 🟡
`BuildBenefitRows` ra ROOM 1.450.000 / OTHER 350.000 thay vì 600.000 / 1.200.000. Đã thử sửa 3 lần chưa được. Đây là **số tiền hiện trên màn chấm điểm** — không phải lỗi UI. *(21/09: làm sau khi chuỗi đúng — nhiều khả năng lỗi ở dữ liệu vào chứ không ở hàm gộp.)*

**4. Chatbox trợ lý hiện là stub.** 🟡
Chip Q&A trả về Notification stub (cố ý). Khi AG-2 chạy thật thì đây là chỗ nối vào. Trong deck phải nói rõ phần nào là agent thật, phần nào là gợi ý câu hỏi.

**5. Phạm vi màn đã vượt xa `build-06` §0.2.** 🟡
`build-06` cắt còn "6 màn phải có" và xếp Dashboard / Policies / Terms / Checkpoints / Admin / PortalHome / PortalBenefits / PortalNotifications vào nhóm **BỎ**. Thực tế đã dựng hết. Không phải lỗi — nhưng nghĩa là **53 message của `build-06` §5 không phủ hết chữ đang hiện trên màn**, và checklist §10 của `build-06` phải chạy trên 17 màn chứ không phải 6. Bảng đối chiếu ở §8.

**6. Limit ledger counter map thô** (200 ↔ OUTPATIENT, used = 0). 🟡

**7. Song ngữ vi/en chưa chốt cơ chế.** 🟡
Prototype dùng `t(vi,en)` + `EN_MAP`; app hiện chỉ có nút VI trên top bar. `build-06` §8.5 yêu cầu console mặc định EN có nút VI, portal thuần VI. *(20/09 đã kiểm: nút VI hiện **trơ**, chrome là EN literal; cơ chế duy nhất có sẵn là `GetCurrentLocale()` ⇒ toggle cần `SetCurrentLocale` + ODC Translation. Đo công trên `PortalHome` trước khi làm 6 màn còn lại.)*

**8. Login chưa có toggle Staff/Customer.** 🟢
Giờ hai app tách hẳn nên toggle có thể không cần nữa — nhưng kịch bản demo chuyển từ portal sang console cần một đường đi rõ ràng. Hiện "+ New claim" mở MyInsurance ở tab mới cho role ngoài console.

**9. Chưa chụp ở 390 px.** 🟢 Audit chạy trên Chrome 1920 px.

**10. "4 điều kiện NÀO?" — hai bộ cùng đếm ra 4.** 🔴 **MỚI 21/09**
`build-04` §3.1 liệt G1 tiền & không có gì bị cắt · G2 chứng từ · G3 lâm sàng & không có điểm kiểm mở · G4 gian lận. Audit rev 47 đọc màn Approval thấy điều kiện **#2 là "đề xuất chờ người duyệt"** — thứ doc xếp vào G3, và #1 fail vì *cắt co-pay* chứ không vì vượt ngưỡng tiền.
Đây đúng họ lỗi đã ăn của dự án này nhiều lần (`IsActive` vs `Status`, `level` vs `benefit level`, fixture trùng nhau): **hai thứ cùng tên, cùng hình dạng, khác nội dung** — và nếu không đối chiếu thì cả hai bên đều tưởng mình đúng.
**Xử:** đọc `EvaluateAutoApprovalGate` trên tenant, liệt kê điều kiện **như code đang tính** kèm biểu thức/filter, lấy code làm chuẩn, sửa tài liệu theo. Làm **trước** khi viết `T-GATE`.

**11. Hai card `0 ₫` trên màn hero.** 🔴 **MỚI 21/09**
`F-CH-1` card tĩnh *"AFTER CONFIRMATION 0 ₫"* đứng cạnh card thật. `F-CH-3` panel ledger hiện *"Left 0 ₫"* vì đọc `LimitLedger` thay vì `PolicyBenefitSnapshot` — chính là **W-8 ở tầng hiển thị**, cùng lỗi vừa sửa trong chuỗi. Cả hai nằm trên màn giám khảo sẽ mở ⇒ đã đưa khỏi đợt F, làm trước pitch.

---

## 5. Việc làm ngay, theo thứ tự — *cập nhật 21/09*

1. ~~Đối chiếu entity + enum với `build-01`~~ ✅ · ~~seed 215 bản ghi~~ ✅ · ~~chuỗi 14 bước~~ ✅
2. **Publish ClaimMVP** (bản dọn seed `ChainRun`/`CalcSnapshot` hồ sơ B và D đang chờ publish) → bấm "Re-run demo seed" → **chạy lại cả 46 test trên fixture sạch**. Đụng cơ chế fixture thì chạy lại toàn bộ, không suy ra.
3. **Đọc `EvaluateAutoApprovalGate`** để chốt *4 điều kiện nào* (rủi ro #10), rồi sửa bảng cổng tab Approval — **mỗi điều kiện Đạt/Không đạt kèm lý do cụ thể, không chỉ ✓/✕**. Đây mới là thứ giám khảo hỏi, không phải con số 3-hay-4.
4. **`F-CH-1` + `F-CH-3`** (rủi ro #11) — hai card `0 ₫` trên màn hero.
5. **25 test còn lại** — `T-CH14` ×6 → `T-SEED` ×11 → `T-GATE` ×7 sau khi (3) xong.
6. **Nối `A5Ledger_Finalize`/`Reverse` vào Approve/Decline**, rồi chạy lại `T-CHAIN-06` + `T-CH14-b/c` **qua nút thật** — hiện mới test hàm, chưa test luồng.
7. **REST Consume IDP + ghi `RawExtractionJson` + bật `ReplayMode`.** Việc lớn nhất còn lại.
8. **AG-2 trên Agent Workbench** nối vào `RunAdjudicationChain`, ghi `AgentRun` → màn trace sống dậy.
9. **`POST /request/compare`** lấy số accuracy trước 29/09.
10. Áp wording trên **17 màn** (gồm 2 câu `COPAY`/`CAP_VISIT` đã duyệt) + grep mã nội bộ trên portal + quay video backup.
11. **Deck + kịch bản pitch** — song song từ hôm nay, không chờ code xong.
12. **Sửa `build-05` §9 tại nguồn** — trần server action 120 s là sai; thật là **10 s mặc định / 60 s tối đa** qua `ServerRequestTimeout` (nguồn: SA OutSystems, 21/09).

---

## 6. Lịch còn lại (cập nhật 21/09)

Hôm nay **21/09**. Pitch **07–08/10**.

| Mốc | Phải xong | Ghi chú |
|---|---|---|
| ~~22/09~~ | ~~Delta entity + seed 215 + chốt cổng 3/4~~ | ✅ **xong sớm 21/09**, cả ba |
| ~~25/09~~ | ~~Chuỗi 14 bước + test pass~~ | ✅ **chuỗi xong 21/09, sớm 4 ngày.** Còn 25/71 test |
| **22–25/09** | 25 test còn lại · chốt "4 điều kiện nào" · `F-CH-1`/`F-CH-3` · nối Finalize vào Approve | Ngân sách đã có đệm 4 ngày từ việc chuỗi xong sớm |
| **26/09** | **Mốc quyết định.** Chưa pass 71/71 thì cắt tiếp: bỏ upload thật, bỏ AG-0 | AG-0 thực ra đã bỏ — SA xác nhận 21/09 agent không gọi lồng nhau |
| 27–29/09 | REST IDP gọi thật + `ReplayMode` + AG-2 + `compare` lấy accuracy + deck | Đường găng thật nằm ở đây |
| 06/10 | Wording + 3 lần tập pitch + dry-run tenant sạch + video backup | Như cũ |

**Đường cắt khi trễ — cập nhật:**

1. Chỉ giữ kịch bản **`CLM-2026-0001`** (A, C, D đã cắt 21/09)
2. ~~Bỏ AG-0~~ — đã bỏ, quyết định đóng
3. Bỏ upload thật, chạy **ReplayMode**
4. **Hy sinh `T-SEED` trước, không bao giờ hy sinh IDP hay deck** — 11 test đó bảo vệ dữ liệu demo, nhưng thiếu IDP hoặc thiếu deck thì bài dự thi hỏng

> Sàn tuyệt đối không được thủng, không đổi: **một agent trên Agent Workbench, gọi một Server Action, ra một con số đúng, có bản ghi `AgentRun`.**

---

## 7. Bài học đã rút được từ pass UI (giữ cho các batch sau)

- **Luôn dùng widget `Link`, không bao giờ `Container` OnClick** — OnClick Mentor gắn lên Container/Table row không bao giờ chạy. Đây là lỗi làm portal mất hẳn đường vào màn chi tiết.
- **`ColumnsMediumLeft` là block hẹp-bên-trái**, không phải ngược lại. Dùng `ColumnsMediumRight` cho bố cục 2/3–1/3.
- **Skip-link không được dùng `href="#…"`** — router SPA sẽ điều hướng về màn gốc. Phải dùng client action OnClick.
- Sửa tap-target bằng rule `.cc-table td a { inline-flex }` sẽ **bóp chữ trong ô bảng** — kiểm lại bảng sau mỗi lần đụng rule đó.
- Mentor **regress lặng lẽ**: màn Checkpoint queue hỏng ở rev 41, phát hiện ở rev 44. Chụp màn sau mỗi batch là việc rẻ nhất để bắt chuyện này.

> **Bộ luật đầy đủ nay nằm ở `openspec/README.md` §7 — 14 rule.** Những cái đắt nhất, thêm 21/09: **rule 11** một bộ test xanh vì fixture trùng nhau trông giống hệt một bộ test đúng · **rule 13** sửa một nửa còn tệ hơn không sửa, và đếm trước/đếm sau phải nằm trong **cùng một lượt** · **rule 14** Mentor sửa model chứ không deploy — "saved revision 943" không phải "published rev 163".

---

## 8. Đối chiếu màn đã dựng với `build-06`

`build-06` §0.2 cắt phạm vi còn **6 màn phải có + 7 nên có**, phần còn lại là "ảnh trong deck". Thực tế đã dựng **17 màn**. Bảng này để khi áp wording không sót màn nào và không ai đi tìm một màn theo mã cũ.

| `build-06` | Màn thật | App | Trạng thái |
|---|---|---|---|
| **M1** — Hàng đợi hồ sơ (F42) | `WorkList` | ClaimMVP | ✅ |
| **M2** — Trích xuất & dòng chi phí (F46, nhúng F44 + F47) | `ClaimDetail` tab *costs*, *docs*, *events* | ClaimMVP | ✅ — **dựng thành 3 tab riêng, không nhúng** như build-06 đề xuất. Giữ nguyên, tốt hơn |
| **M3** — Tính tiền & sổ hạn mức (F48) | `ClaimDetail` tab *calc* + card ledger | ClaimMVP | 🟡 — thiếu mô tả rule từng bước; card ledger đọc sai nguồn (`F-CH-3`) |
| **M4** — Phương án + Duyệt (F49+F50 gộp) | `ClaimDetail` tab *settle* + tab *approval* | ClaimMVP | 🟡 — **tách 2 tab, không gộp**; **4 điều kiện là ĐÚNG chuẩn** (sửa 21/09 — dòng này trước ghi "4 thay vì 3" như một lỗi); `Claimed` chia sai |
| **M5 / S14** — Kết quả chi trả | `PortalClaimDetail` | MyInsurance | ✅ — có khối "Vì sao Quý khách tự chịu", stepline theo `ClaimStatus`. 🟡 hai câu `COPAY`/`CAP_VISIT` đã duyệt, chưa áp |
| **M6** — Chế độ trình bày | `AgentTrace` | ClaimMVP | 🟡 — có trace `AgentRun`; **chưa có** bản đồ 4 tầng, bảng số đo, bảng ROI |
| **S11** — Tạo hồ sơ | `PortalNewClaim` | MyInsurance | ✅ — wizard 4 bước |
| **S12** — Chụp chứng từ | `PortalSupplement` | MyInsurance | ✅ |
| **S13** — Danh sách & tiến trình | `PortalClaims` | MyInsurance | ✅ — nhãn trạng thái đã đồng bộ 3 nơi (rev 5–7) |
| *xếp BỎ* — F54 Dashboard | `Dashboard` | ClaimMVP | ✅ đã dựng |
| *xếp BỎ* — F53 Hàng đợi vận hành | `OpsQueue` | ClaimMVP | ✅ đã dựng |
| *xếp BỎ* — F60 Hợp đồng | `Policies` | ClaimMVP | ✅ — cột "Insured with claims" còn trống |
| *xếp BỎ* — F65 Sổ điều khoản | `TermRegister` | ClaimMVP | ✅ đã dựng |
| *xếp BỎ* — F67 Hàng đợi xác minh | `Checkpoints` | ClaimMVP | ✅ |
| *xếp BỎ* — F48 Sổ hạn mức | `LimitLedger` | ClaimMVP | 🟡 counter map thô |
| *xếp BỎ* — F55 Quản trị | `Administration` | ClaimMVP | ✅ — thêm nút re-run demo seed |
| *xếp BỎ* — F53 Lịch sử & nhật ký | `ClaimDetail` tab *history* | ClaimMVP | ✅ |
| *xếp BỎ* — S08 Bảo hiểm của tôi | `PortalHome` | MyInsurance | ✅ |
| *xếp BỎ* — S08 Quyền lợi | `PortalBenefits` | MyInsurance | ✅ |
| *xếp BỎ* — thông báo | `PortalNotifications` | MyInsurance | ✅ — message theo từng trạng thái |

**Hệ quả cho bước áp wording:** `build-06` §10 có một mục *"grep HTML render của portal với 13 trạng thái nội bộ + 7 mã chứng từ + 3 mức REQUIRED/EXPECTED/OPTIONAL + 12 mã lossType — không được khớp cái nào"*. Nay phải chạy trên **7 màn MyInsurance**, không phải 4. Ba màn portal mới (`PortalHome`, `PortalBenefits`, `PortalNotifications`) chưa từng nằm trong phạm vi soát câu chữ của A2 — đây là chỗ nhiều khả năng rò mã nội bộ nhất. **Thêm: fixture `TST-`/`FX-` phải xoá khỏi dữ liệu demo trước khi chạy grep**, nếu không sẽ bắt nhầm.
