# Build 04 — Chuỗi tính tiền & Seed sản phẩm

| Mục | Giá trị |
|---|---|
| Tài liệu | Build 04 — Chuỗi tính tiền & Seed sản phẩm (Agentic Health Claim Adjudication) |
| Phiên bản | 1.0 — 16/09/2026 |
| Đội | BnK Solution — Claim Platform team |
| Đích | Hiện thực hoá thành Server Action `RunAdjudicationChain` + `EvaluateAutoApprovalGate` + seed Entity trên OutSystems ODC |
| Chuẩn | **A5 Product Data Sheet v1.5** (§4, §5, §6, §7, §8, §10, §11) và **D1 BRD v0.5** (§5.1–§5.9, Part XI B-xx) là **normative** cho mọi số tiền, mọi công thức và thứ tự bước |
| Hiển thị | **D2 FS v0.6 §5.9 (F48)** quy định cách render chuỗi, ledger và panel điều khoản |
| Phải sửa theo | **BRD hackathon v1.0 §9.1.3** — bốn khác biệt làm đổi tiền, xem §1.1 |

---

## 0. Từ vựng phân loại claim

**A5 §6 và D1 §5.3 là chuẩn của tầng tính tiền.** Mọi con số, mọi công thức và mọi thứ tự bước trong tài liệu này lấy từ hai nguồn đó; D2 §5.9 (F48) quy định cách hiển thị chúng. Không có quy tắc tính tiền nào trong tài liệu này đến từ nơi khác.

Ba trường phân loại chi phối toàn bộ chuỗi. Chúng độc lập với nhau và **không trường nào suy ra trường nào**.

### 0.1 `lossType` — 12 giá trị, 8 live (A1 v1.5 §4.2, nguyên văn)

**The loss type is the clinical benefit group crossed with where the treatment happened.** Nó là một trường gói hai chiều: nhóm quyền lợi × nơi điều trị.

| # | Code | VI | EN | Benefit group | Setting | MVP 1 |
|---|---|---|---|---|---|---|
| 1 | `ILL_OP` | Bệnh (ngoại trú) | Illness, outpatient | `200` | Ngoại trú | ✔ |
| 2 | `ILL_IP` | Bệnh (nội trú) | Illness, inpatient | `100` | Nội trú | ✔ |
| 3 | `DEN_OP` | Nha khoa (ngoại trú) | Dental, outpatient | `300` | Ngoại trú | ✔ |
| 4 | `DEN_IP` | Nha khoa (nội trú) | Dental, inpatient | `300` | Nội trú | ✔ |
| 5 | `ACC_OP` | Tai nạn (ngoại trú) | Accident, outpatient | `500` | Ngoại trú | ✔ |
| 6 | `ACC_IP` | Tai nạn (nội trú) | Accident, inpatient | `500` | Nội trú | ✔ |
| 7 | `MAT_OP` | Thai sản (ngoại trú) | Maternity, outpatient | `400` | Ngoại trú | ✔ |
| 8 | `MAT_IP` | Thai sản (nội trú) | Maternity, inpatient | `400` | Nội trú | ✔ |
| 9 | `DTH_ILL` | Tử vong do bệnh, thai sản | Death from illness or maternity | — | — | ✘ |
| 10 | `DTH_ACC` | Tử vong do tai nạn | Death from accident | — | — | ✘ |
| 11 | `PTD_ACC` | Thương tật vĩnh viễn do tai nạn | Permanent disability from accident | — | — | ✘ |
| 12 | `PTD_ILL` | Tổn thương sức khoẻ vĩnh viễn do bệnh | Permanent impairment from illness | — | — | ✘ |

**Tám giá trị live, bốn giá trị counter-only.** Dòng 9–12 tồn tại **chỉ để một giá trị đến từ bất cứ đâu cũng có tên**. Không màn hình nào chào chúng, và một claim mang một trong bốn giá trị đó là **lỗi dữ liệu, không phải một claim**. Trong slice hackathon chỉ hai nhóm được dùng: `ILL_*` và `ACC_*`.

**`lossType` được NHẬP, không được suy ra.** Giám định viên set nó trên insured event (D2 §5.7.2 F18); app khách hàng set nó trên form claim từ cùng tám giá trị live (D2 §4.5). **Không có gì tính ra nó.**

### 0.2 `episodeType` — 5 giá trị, là phép chiếu

`episodeType` là trường mà **A5 §8.1 key bảng benefit mapping lên**. Nó là **phép chiếu** của `lossType`, không phải thứ ai gõ:

| `lossType` | → `episodeType` |
|---|---|
| `ILL_OP` | `OUTPATIENT` |
| `ILL_IP` | `INPATIENT` |
| `DEN_OP` · `DEN_IP` | `DENTAL` |
| `MAT_OP` · `MAT_IP` | `MATERNITY` |
| `ACC_OP` · `ACC_IP` | `ACCIDENT` |

D2 §5.7.2 F5 render nó **read-only** và nêu phép chiếu. **Hai trường không bao giờ được set độc lập** — code cho phép chúng lệch nhau là code có defect, không phải có feature.

### 0.3 `conditionClass` — 6 giá trị, trực giao với `lossType`

`PRE` · `SPE` · `GEN` · `BIR` · `MAT` · `ACC`.

`conditionClass` trả lời *loại tình trạng y tế nào* và nó điều khiển **thời gian chờ**. `lossType` trả lời *nhóm quyền lợi nào và điều trị ở đâu* và nó điều khiển **danh mục chứng từ** và **benefit mapping**. Một tình trạng có sẵn điều trị ngoại trú là `conditionClass = PRE` **và** `lossType = ILL_OP`; hai fact set riêng và **không fact nào suy ra fact kia**.

**Một ràng buộc chạy giữa hai trường** — và nó là ràng buộc chứ không phải phép suy diễn: `ACC_*` chỉ nhận `conditionClass = ACC`; `MAT_*` chỉ nhận `BIR` hoặc `MAT`.

`conditionClass` được xác định **tại thời điểm thẩm định** và lưu trên insured event (`BR-WAIT-3`). Một insured event mang **đúng một** condition class — và có những sự kiện thật mang hai (xem `A5-Q9`, §8.3).

### 0.4 Ba fact còn thiếu ở tầng trích xuất

Ba fact của từ vựng đóng chưa có hôm nay, và mỗi cái chặn một cơ chế cụ thể. `BR-CHAIN-1` áp cho cả ba: **bước không đánh giá được không bao giờ là pass**.

| Fact thiếu | Mã | Chặn cái gì |
|---|---|---|
| `lineDiagnosisCode` — ICD theo từng dòng chi phí | `Q-IDP-02` | Term #5 và #9 đứng `DRAFT` |
| `facilityCountry` — mã quốc gia của cơ sở y tế | `Q-IDP-05` | Term #10 đứng `DRAFT` |
| Số ngày điều trị theo dòng | `Q-IDP-06` | Bộ đếm `DAY` — tức **cơ chế duy nhất mà `100.1` (60 ngày) và `100.1.1` (15 ngày) tồn tại để thực thi** |

Hệ quả trực tiếp: **term engine chỉ chạy 6 trong 10 term** (A5 §7.5, D2 `V-752`). Màn hình hiện "10 điều khoản đã xét" là nói dối giám định viên. Xem §7.4.

`serviceCategory` (20 giá trị, §5.4) là **output của tầng trích xuất**, không phải thứ benefit engine tự sinh; adapter phải sinh nó và phải **cảnh báo khi gặp nhãn lạ, không bỏ qua im lặng**.

## 1. Chuỗi tính tiền — từng bước

### 1.1 ★ A5 §6.1 LÀ CHUẨN. BRD hackathon §9.1.3 SAI Ở BỐN CHỖ VÀ PHẢI SỬA THEO

**Đọc mục này trước khi viết một dòng code nào của chuỗi.**

BRD hackathon §9.1.3 và A5 §6.1 đều nói "14 bước" nhưng **không phải cùng 14 bước**. Đây **không** phải hai nguồn cạnh tranh cần hoà giải: **A5 §6.1 + D1 §5.3 là chuẩn của tầng tính tiền**, `CH-01…CH-14` của BRD là một bộ làm việc viết trước khi A5 §6.2 được đọc kỹ, và **bốn khác biệt dưới đây đều làm đổi số tiền chi trả**. Mỗi dòng cột *Xử lý* là một thay đổi phải làm **trong BRD**, không phải trong A5.

| # | Khác biệt | BRD hackathon (**sai**) | A5 §6.1 / D1 §5.3 (**chuẩn**) | Xử lý — sửa BRD |
|---|---|---|---|---|

| **X-1** | **Khấu trừ BHYT** | `CH-06 SocialInsuranceDeduction` — trừ phần BHYT đã trả | **Không có bước này.** S-17, quyết ngày 17/08/2026: MVP 1 **capture BHYT nhưng không trừ**. Chuỗi chạy trên số **billed**. Nếu bật, nó là **step 8 mới** và chuỗi thành 15 bước | **Bỏ `CH-06` khỏi chuỗi tính tiền.** Vẫn lưu `PaidBySocialInsurance` trên `ClaimLine` để hiển thị. Ghi là simplification đã tuyên bố |
| **X-2** | **Deductible / mức miễn thường** | `CH-10 Deductible` | **Ngoài phạm vi.** Sản phẩm pilot không có. D1 §5.6 nguyên văn: *"If a future product needs it, it enters as a step between 9 and 10"* | **Bỏ `CH-10`.** Giữ slot cho tương lai giữa bước 9 và 10 |
| **X-3** | **Làm tròn** | `CH-12 Rounding` — *"round down to 1,000 VND (B-20 interim)"* | **Làm tròn ở MỌI bước sinh tiền**, tới **đồng nguyên, nửa làm tròn ra xa số 0**. Không có bước làm tròn riêng | **`CH-12` bị bỏ làm bước độc lập.** Xem §1.3. **Và chú thích "(B-20 interim)" trong BRD là nhầm** — B-20 là thứ tự tiêu thụ cap, không phải làm tròn. Phải sửa BRD |
| **X-4** | **Term engine (loại trừ có cấu trúc)** | Không có bước riêng; gộp vào `CH-05 CoverageScopeExclusions` | **Hai bước riêng**: step 4 (general exclusions, scope `PRODUCT`) và step 7 (benefit-level provisos, scope `BENEFIT`/`BENEFIT_SUBTREE`) | **Tách lại thành 2 bước.** Step 7 chạy tập rỗng trên sản phẩm này (`B-23`) và **vẫn phải hiện ra màn hình** với câu "không có điều khoản riêng của quyền lợi trên sản phẩm này" (D2 §5.9.1a) |

**Bảng đối chiếu mã bước — dùng bảng này khi đặt tên Server Action:**

| Bước chuẩn (A5 §6.1) | Tên | Mã Server Action đề nghị | `CH-nn` BRD tương ứng |
|---|---|---|---|
| 1 | Member eligibility | `CH-01 MemberEligibility` | `CH-01 PolicyEligibility` |
| 2 | Waiting period | `CH-02 WaitingPeriod` | `CH-02 WaitingPeriod` |
| 3 | Pre-existing condition rule | `CH-03 PreExistingRule` | `CH-03 EventClassification` *(khác nghĩa)* |
| 4 | General exclusions | `CH-04 GeneralExclusions` | gộp trong `CH-05` |
| 5 | Benefit mapping | `CH-05 BenefitMapping` | `CH-04 LineBenefitMapping` |
| 6 | Line-level cover test | `CH-06 LineCoverTest` | gộp trong `CH-05` |
| 7 | Benefit-level provisos | `CH-07 BenefitProvisos` | — |
| 8 | Per-visit / per-day / per-event sub-limit | `CH-08 SubLimit` | `CH-07 PerVisitCap` |
| 9 | Count limit | `CH-09 CountLimit` | — |
| 10 | Co-payment | `CH-10 CoPayment` | `CH-09 CoPayment` |
| 11 | Deduction ratio | `CH-11 DeductionRatio` | — |
| 12 | Parent annual cap | `CH-12 ParentAnnualCap` | `CH-08 PerYearLimitCheck` + `CH-11 ClaimLevelCap` |
| 13 | Plan-level check | `CH-13 PlanLevelCheck` | — |
| 14 | Post to ledger | `CH-14 PostLedger` | `CH-14 LedgerPreview` |
| *(không phải bước)* | Customer-borne breakdown | `BuildCustomerBorneBreakdown` | `CH-13 CustomerBorneBreakdown` |

**Bốn thay đổi bắt buộc trong BRD, viết ra để không ai phải suy lại:** (1) xoá `CH-06 SocialInsuranceDeduction`; (2) xoá `CH-10 Deductible`; (3) xoá `CH-12 Rounding` như một bước và thay bằng quy tắc làm tròn từng bước ở §1.3; (4) tách `CH-05 CoverageScopeExclusions` thành bước 4 và bước 7. Sau bốn thay đổi này, BRD và A5 mô tả **cùng một chuỗi**, và `SC-H4` (*"same claim run twice gives identical amounts"*) mới có nghĩa.

`CH-13` của BRD (*"customer-borne split by cause"*) **không phải một bước tính tiền** — nó không đổi số tiền, nó phân loại số tiền đã tính ra theo nguyên nhân để hiện lên S13 và thư gửi khách. Giữ nó làm một Server Action **sau** bước 14, không đánh số trong chuỗi.

### 1.2 Bốn cấp adjudication — đọc trước khi đọc từng bước

**Sai lầm phổ biến nhất là áp một bước ở sai cấp.** D1 §5.1, nguyên văn:

| Level | What it is |
|---|---|
| **Claim** | The case. One customer submission |
| **Insured event** | One episode of treatment: one provider, one date range, one diagnosis set. **A claim carries many** |
| **Visit** | **Derived, new at v0.5 — `BR-LEDGER-14`.** A cost line's **`treatmentDate` together with its provider**; lines sharing both are one visit. **It is not captured and it is not a level of the data model** — it is computed from facts already on every line. **An insured event carries many visits**: a twelve-session course of physiotherapy at one provider is *one event and twelve visits* |
| **Cost line** | One itemised charge from a cost statement. **The unit of the pay/decline decision** |

**Hai visit tới cùng một cơ sở trong cùng một ngày là MỘT visit.** Đây là quy ước đã viết xuống, không phải chi tiết hiện thực.

### 1.3 Quy tắc làm tròn — áp cho mọi bước, viết một lần

> **Rounding — stated once, because two implementations of the same fourteen steps must agree.** Every step that produces a money amount rounds to the **nearest whole dong, half away from zero**, and rounding happens **at each step, not once at the end**. This matters for BR-TRACK-4: the rule chain re-runs after a clinical recommendation is confirmed, and a chain that rounded only at the end could return a different total from the first pass on the same inputs. Percentages are held as integer basis points (D3 §1.2); the rounding applies to the money result, never to the rate.
> *(A5 §6.2, lặp lại nguyên văn ở D1 §5.3)*

**Hiện thực trên ODC:** `Money` là `Decimal(18,0)` VND (BRD DR-05). Mọi bước kết thúc bằng `Round(x, 0, MidpointRounding.AwayFromZero)`. Tỷ lệ giữ nguyên dạng **basis point nguyên** (`2000` = 20%), chuyển đổi `bp = pct × 100` **chỉ làm ở script seed**, không bao giờ trong engine (A5 §10).

### 1.4 Nguyên tắc BR-CHAIN-1 — bước không đánh giá được KHÔNG BAO GIỜ là pass

| Rule | Statement |
|---|---|
| **BR-CHAIN-1** | **A step of the chain that cannot be evaluated because a fact it requires is unavailable is never treated as a pass.** The line is **held**, a **checkpoint is raised naming the missing fact**, and the claim is **outside the auto-approval gate**. It is not a decline either: an absent fact is a data problem |

Trên sản phẩm này, hai fact vắng mặt kích hoạt BR-CHAIN-1 ngay: **số ngày điều trị theo dòng** (`Q-IDP-06`, chặn bước 9 kiểu `DAY`) và **ICD theo dòng** (`Q-IDP-02`, chặn 2 term). Bước phải ghi **tên fact thiếu** vào ô *Diễn giải*, không để ô *Tác động* bằng 0 — bước lặng lẽ pass và bước không chạy được nhìn giống hệt nhau trong một tổng số.

---

### Bước 1 — `CH-01 MemberEligibility`

| | |
|---|---|
| **Cấp áp dụng** | **The whole claim** |
| **Quy tắc (verbatim A5 §6.1)** | *"Member active on the service date; policy in force; premium recorded paid"* |
| **Quy tắc (verbatim D1 §5.3)** | *"Member active on the service date, policy in force, **premium recorded paid — a boolean, with no grace period**. A failure here produces a **computed decline**, and **a decline never auto-approves**"* |
| **Dữ liệu vào** | `Policy` (hiệu lực, trạng thái), `Member` (ngày bắt đầu bảo hiểm), `PolicyBenefitSnapshot`, ngày dịch vụ của từng cost line |
| **Đầu ra** | Eligible / Not eligible ở cấp claim. Not eligible → toàn bộ claim decline |
| **Mã từ chối sinh ra** | `PERIOD` — *"Ngày xảy ra sự kiện BH không thuộc thời hạn BH"* (cấp event) |
| **Base amount cho bước sau** | Toàn bộ số tiền yêu cầu (`billed`) của mọi cost line. Bước này không cắt tiền, nó chỉ mở hoặc đóng cổng |

**Cạm bẫy:** `premium recorded paid` là **boolean, không có thời gian ân hạn**. Đừng hiện thực hoá một khoảng ân hạn nào.

---

### Bước 2 — `CH-02 WaitingPeriod`

| | |
|---|---|
| **Cấp áp dụng** | **Per insured event** — condition class lưu trên event (`BR-WAIT-3`) |
| **Quy tắc (verbatim A5 §6.1)** | *"By condition class per §5"* |
| **Dữ liệu vào** | `conditionClass` trên insured event; `benefit_waiting_period` từ snapshot (plan × benefit group × condition class); **ngày bắt đầu bảo hiểm của thành viên** (không phải ngày phát hành hợp đồng); ngày điều trị |
| **Đầu ra** | Per line: Excluded (`WAIT`) hoặc tiếp tục |
| **Mã từ chối sinh ra** | `WAIT` — *"Chưa đủ thời gian chờ"* (cấp event hoặc line) |
| **Base amount cho bước sau** | Tổng billed của các dòng **không bị** `WAIT` |

**Năm quy tắc bắt buộc (D1 §5.4, nguyên văn):**

| Rule | Statement |
|---|---|
| **BR-WAIT-1** | The period is measured from the **member's coverage start date**, not the policy inception date |
| **BR-WAIT-2** | On a renewal without a break in cover, periods already served are **not re-served**. **Stated for completeness and not exercisable in MVP 1** |
| **BR-WAIT-3** | The condition class is determined at assessment and stored on the insured event |
| **BR-WAIT-4** | `n/a` in the matrix means the combination **cannot arise**. A claim that produces one is a **data error**, not a zero-day waiting period |
| **BR-WAIT-5** | A waiting-period decline shows the arithmetic: the period, the date it counts from, and the treatment date |

**Waiting period KHÔNG hard-block.** Engine tính ra decline, giám định viên **xem được và override được kèm lý do**, override bị audit.

---

### Bước 3 — `CH-03 PreExistingRule`

| | |
|---|---|
| **Cấp áp dụng** | **Per insured event** |
| **Áp dụng khi** | `condition_class = PRE` |
| **Quy tắc (verbatim A5 §6.1)** | *"Subject to the 365-day waiting period; not an outright exclusion in this product"* |
| **Quy tắc (verbatim D1 §5.3)** | *"**A no-op on the pilot product.** Step 2 already applies the waiting period by condition class, and A5 §5 gives `PRE` a 365-day wait. **The step exists because a product may exclude pre-existing conditions outright rather than defer them**"* |
| **Dữ liệu vào** | `conditionClass` |
| **Đầu ra** | Trên sản phẩm pilot: luôn pass. `stepOutcome = KHÔNG_ÁP_DỤNG` |
| **Mã từ chối sinh ra** | Không có trên sản phẩm này. (Trên sản phẩm loại trừ thẳng: `EXCL`) |
| **Base amount cho bước sau** | Không đổi so với bước 2 |

**Vẫn phải hiện lên màn hình.** D2 V-540: *"Every step is shown, including the ones that did nothing."*

---

### Bước 4 — `CH-04 GeneralExclusions`

| | |
|---|---|
| **Cấp áp dụng** | **Per cost line** khi effect là `DECLINE_LINE`; **per insured event** khi effect là `DECLINE_EVENT` (`BR-TERM-2`) |
| **Quy tắc (verbatim A5 §6.1)** | *"Evaluated by the term engine against the term register, §7. Every term in scope is tested and every result recorded, whether it fires or not"* |
| **Quy tắc (verbatim D1 §5.3)** | *"Class **D** may auto-decline **where the term's effect is a decline**, reason code `EXCL`, citing the `clauseRef`. Class **J** and class **E** raise a **checkpoint** and may **never** carry a decline effect (**BR-TERM-7**). **A term whose `clauseRef` is provisional may not decline at all and raises a checkpoint instead — `BR-TERM-10`**"* |
| **Dữ liệu vào** | `policy_term` (10 dòng, key trên `ruleSetRef` của policy); fact từ vựng đóng: `lossType`, `serviceCategory`, `lineDiagnosisCode`, `facilityCountry`, `diagnosisCode` — **5 fact, không được bịa ra fact thứ 6** |
| **Đầu ra** | Per term per line/event: `ĐẠT` · `TỪ_CHỐI` · `ĐIỂM_KIỂM` · `KHÔNG_THUỘC_PHẠM_VI` |
| **Mã từ chối sinh ra** | `EXCL` — *"Thuộc điểm loại trừ của quy tắc BH"* (cấp event hoặc line) |
| **Base amount cho bước sau** | Tổng billed của các dòng còn lại sau khi trừ dòng/sự kiện bị `EXCL` |

**Hai quy tắc chặn sai:**

- **`BR-TERM-9`** — *a term may test only facts available at the step its scope is evaluated at*, kiểm tra khi **lưu** term. `benefitCode` do bước 5 sinh ra, nên **không một term scope `PRODUCT` nào được test `benefitCode`**.
- **`BR-TERM-10`** — `clauseRef` dạng `TBC-nn` là tạm. **Không được phát hành decline trích dẫn một `TBC-`.** Term đó raise checkpoint thay vì decline.

**Đếm term:** `n` = mọi term trong sổ (10). `m` = term thực sự được đánh giá — `ACTIVE`, trong cửa sổ hiệu lực, bind đúng `ruleSetRef` (**6**). `n − m` = bản `DRAFT` và `WITHDRAWN`. Đẳng thức: `x + y + z + k = m`, `m ≤ n`. **Màn hình hiện 10 là nói dối giám định viên.**

**Nhiều term cùng decline một dòng → mang TẤT CẢ `clauseRef` vào thông điệp decline, không chỉ cái đầu.** `DECLINE_EVENT` thắng `DECLINE_LINE`; `CHECKPOINT` không bao giờ đè lên decline; checkpoint còn mở trên dòng đã decline được đóng là *moot* kèm lý do.

---

### Bước 5 — `CH-05 BenefitMapping`

| | |
|---|---|
| **Cấp áp dụng** | **Per cost line** |
| **Quy tắc (verbatim A5 §6.1)** | *"Cost line → benefit code, per §8. **It no longer declines anything** — §7.4"* |
| **Dữ liệu vào** | `serviceCategory` của dòng (20 giá trị); `episodeType` của sự kiện; bảng `service_category_benefit_map` |
| **Đầu ra** | `benefitCode` trên từng dòng, hoặc sentinel `"NONE"` / `"ASSESSOR"` |
| **Mã từ chối sinh ra** | **Không có.** Bảng mapping không decline bất cứ thứ gì |
| **Base amount cho bước sau** | Không đổi |

**Hai sentinel, và chúng là chuỗi ký tự có chủ ý:**

| Sentinel | Nghĩa | Vì sao là string |
|---|---|---|
| **`"NONE"`** | **This category has no benefit in this treatment setting.** The line is not declined by this table — the table declines nothing — it falls to the line-level cover test at **step 6** | It held `null` until v1.5, which collided head-on with **D3 §1.2**: `null` means ***not known*** across every payload and every fixture in this project. *No benefit exists here* is a **known** fact and must not be written in the vocabulary of an unknown one |
| **`"ASSESSOR"`** | **No rule maps this category; a person decides which benefit the line belongs to.** `OTHER` alone | A sentinel of the same kind as `"INHERIT"` on the wire (D3 §I-05), and a string for the same reason: **code that treats it as a benefit code fails loudly instead of looking up a benefit called *ASSESSOR*.** Neither value is a benefit code and **neither may be sent on any interface** |

**Chuyển hướng tai nạn (accident redirect), nguyên văn:** *"Where the episode's loss type is accident, every mapped benefit is redirected to `500`."* **`500` là lá không con** — nó mang `annual_cap` và không gì khác: `per_day_cap` null, `per_event_cap` null, `count_limit: 0`, `count_unit: null`, `copay_pct: null`, `deduction_pct: null`. Vì vậy redirect **vứt bỏ mọi sub-limit, mọi count limit và cả đồng chi trả** trên đường đi. Đây là `A5-Q8`, còn mở — xem §8.

---

### Bước 6 — `CH-06 LineCoverTest`

| | |
|---|---|
| **Cấp áp dụng** | **Per cost line** |
| **Quy tắc (verbatim A5 §6.1)** | *"Each cost line is payable or declined **individually**, with a reason. §8.2"* |
| **Dữ liệu vào** | `benefitCode` (kể cả `"NONE"`); `serviceCategory`; ICD của sự kiện; nhóm điều trị của thuốc; **khuyến nghị lâm sàng đã được giám định viên xác nhận** |
| **Đầu ra** | Per line: payable / declined + reason. **Số tiền rời khỏi bước 6 có tên riêng: *Số tiền thuộc phạm vi bảo hiểm*** (D2 §5.9.1) |
| **Mã từ chối sinh ra** | `NOTCOV` (line only) · `NOTREL` (line only) · `NOBEN` · `EXCL` (từ rule *Excluded category*) |
| **Base amount cho bước sau** | **Số tiền thuộc phạm vi bảo hiểm** — *"The amount leaving step 6 — after the line-by-line pay/decline decision and before any cap, co-payment or deduction touches it. It is the amount the policy accepts as a claim, distinct from the amount it pays on it"* |

**Bốn rule cấp dòng (A5 §8.2, nguyên văn):**

| Rule | Condition | Decline code | Drafted rationale (pattern) |
|---|---|---|---|
| Non-medical item | Category = administrative / non-medical | `NOTCOV` | *"{item}: {amount} — administrative fee, not a covered medical item"* |
| Unrelated test | Test's ICD group is unrelated to the episode diagnosis | `NOTREL` | *"{item}: {amount} — diagnostic test not related to the diagnosis {icd}"* |
| Unrelated medication | Drug's therapeutic class is unrelated to the episode diagnosis | `NOTREL` | *"{item}: {amount} — treats {class}, not related to the diagnosis {icd}"* |
| Excluded category | Item falls under a term in the register — §7.2 | `EXCL` | *"{item}: {amount} — falls under {clauseRef}: {term text}"* |

**Rule *Supplement* đã bị rút bỏ ở v1.4** — thực phẩm chức năng bị loại trừ **bởi điều khoản, có điều kiện**, không phải *không thuộc danh mục theo phân loại*. `TERM-HC-003` quyết nó ở bước 4 và trích `EXCL` kèm clause. Giữ lại rule cũ là tái tạo đúng defect ấy chậm một bước.

**Không có giảm trừ tuỳ ý ở đây.** Giám định viên tin một dòng chỉ nên trả một phần có đúng **một** cơ chế: **decline dòng đó, kèm lý do**.

---

### Bước 7 — `CH-07 BenefitProvisos`

| | |
|---|---|
| **Cấp áp dụng** | **Per cost line, within the scoped benefit** — hoặc trong benefit **và mọi hậu duệ** khi scope là `BENEFIT_SUBTREE` (`BR-TERM-8`) |
| **Áp dụng khi** | Benefit mang một proviso |
| **Quy tắc (verbatim A5 §6.1)** | *"Evaluated by the term engine, scope = `BENEFIT`, §7"* |
| **Dữ liệu vào** | `policy_term` có `scope in [BENEFIT, BENEFIT_SUBTREE]` |
| **Đầu ra** | **Tập rỗng trên sản phẩm này.** Cả 10 term đều `PRODUCT` scope |
| **Mã từ chối sinh ra** | `EXCL` (khi có instance thật); `LIMIT` khi term mang `CAP` (`BR-TERM-12`) |
| **Base amount cho bước sau** | Không đổi trên sản phẩm này |

**Vì sao provisos đứng sau bước 6 chứ không trước:** *"a line that is not covered at all should be declined on that ground, not on a proviso it was never going to reach."*

**Bước chạy tập rỗng vẫn phải render**, với câu *"không có điều khoản riêng của quyền lợi trên sản phẩm này"* — **không được ẩn cả nhóm**, vì nhóm vắng mặt và nhóm rỗng nhìn giống nhau và nghĩa ngược nhau. Đây là `B-23`, còn mở.

---

### Bước 8 — `CH-08 SubLimit`

| | |
|---|---|
| **Cấp áp dụng** | **Per visit** khi `count_unit = VISIT`; **per treatment day** khi benefit có per-day cap; **per insured event** trong các trường hợp còn lại. ***Never* per claim** |
| **Áp dụng khi** | Benefit có sub-limit |
| **Quy tắc (verbatim A5 §6.1)** | *"Capped at the child benefit's limit"* |
| **Dữ liệu vào** | `per_day_cap`, `per_event_cap`, per-visit cap từ snapshot; visit = `treatmentDate` + provider; số ngày điều trị của sự kiện (D2 F17) |
| **Đầu ra** | Số tiền sau trần, phần vượt |
| **Mã từ chối sinh ra** | `LIMIT` — *"Hết hạn mức"* (cấp event hoặc line) |
| **Base amount cho bước sau** | Số tiền sau trần, **làm tròn tại bước này** |

**Đây là bước bị hiện thực sai nhiều nhất.** A5 §6.2 nói thẳng: *"**The cap must be applied per benefit, not to the claim total.** That is the point of choosing these figures: `200.1` breaches its per-visit cap and `200.2` does not, so an engine that capped 2,600,000 at 1,500,000 would under-pay by 900,000 and look, from the outside, exactly like one that got it right."*

**Một liệu trình 12 buổi vật lý trị liệu tại cùng cơ sở là MỘT insured event và MƯỜI HAI visit.** Trần 500.000/lần của `200.3` áp **mười hai lần**, không phải một lần cho cả liệu trình.

---

### Bước 9 — `CH-09 CountLimit`

| | |
|---|---|
| **Cấp áp dụng** | **Per benefit, per counter period** |
| **Áp dụng khi** | Benefit có count limit |
| **Quy tắc (verbatim A5 §6.1)** | *"Days or visits remaining; `0` = unlimited. Tested against the `OCCURRENCE` and `DAY` counters — §4.2"* |
| **Quy tắc (verbatim D1 §5.3)** | *"**`0` means unlimited**. `OCCURRENCE` increments **once per distinct visit** (**BR-LEDGER-14**); `DAY` increments **once per distinct calendar date per counter** (**BR-LEDGER-12**)"* |
| **Dữ liệu vào** | `count_limit`, `count_unit` từ snapshot; bộ đếm từ ledger key `(memberId, policyId, benefitCode, periodKey, counterType)` |
| **Đầu ra** | Số tiền trong phần đếm còn lại; phần vượt |
| **Mã từ chối sinh ra** | `LIMIT` |
| **Base amount cho bước sau** | **Số tiền còn lại sau bước 9 — đây chính là base của đồng chi trả ở bước 10** |

**Quy tắc bộ đếm (A5 §4.2, nguyên văn):**

| | Rule |
|---|---|
| Counter key | `(memberId, policyId, benefitCode, periodKey, counterType)` |
| `counterType` | `AMOUNT` · `OCCURRENCE` · `DAY` |
| `count_limit` where `count_unit = visits` — e.g. `200.3`, 20 visits | An **`OCCURRENCE`** counter, incremented **once per distinct visit** |
| `count_limit` where `count_unit = days` — `100.1` 60 days, `100.1.1` 15 days | A **`DAY`** counter, incremented by the treatment-day count of the line |
| `per_day_cap` | A money cap tested against the same **`DAY`** dimension |
| `per_event_cap` | A money cap tested against the same **`OCCURRENCE`** dimension |
| Counter period | The **policy year**, derived from the policy inception date on the snapshot — **L-1**. Not the calendar year |
| Which period a line falls in | The period containing its **treatment date**, never its submission date — **L-2** |
| A claim whose lines span a boundary | Posts to **two periods**, line by line — **L-3** |
| `periodKey` | Stored on every posting and immutable once posted — **L-4** |

> **L-2 is the one that must not be got wrong.** Keying on submission date would let a member reset their own limit by delaying a claim, and would make the same treatment consume different periods depending on how fast the customer filed.

**`Q-IDP-06`:** bộ đếm `DAY` cần số ngày điều trị theo dòng mà extraction chưa trả. Cho tới khi có, cơ chế đếm ngày (60 ngày `100.1`, 15 ngày `100.1.1`) **đứng như dữ liệu với một bước thủ công phía sau**, và BR-CHAIN-1 áp dụng.

---

### Bước 10 — `CH-10 CoPayment`

| | |
|---|---|
| **Cấp áp dụng** | **Per cost line** |
| **Áp dụng khi** | Benefit **hoặc một tổ tiên** của nó có tỷ lệ |
| **Quy tắc (verbatim A5 §6.1)** | *"**Base amount: the amount remaining after step 9**"* |
| **Base amount (verbatim A5 §6.2)** | *"The **amount payable after all sub-limits and count limits have been applied** — that is, the amount remaining after **step 9** — and after any line-level decline at step 6 and any exclusion or proviso decline at steps 4 and 7. *Not* the amount billed"* |
| **Dữ liệu vào** | `copay_pct` trên benefit, hoặc `"INHERIT"` → lấy của tổ tiên gần nhất có giá trị |
| **Đầu ra** | Phần bảo hiểm trả; phần khách tự chịu mang mã `COPAY` |
| **Mã từ chối sinh ra** | **Không có.** Đồng chi trả không phải từ chối — nó là chia sẻ chi phí theo hợp đồng |
| **Base amount cho bước sau** | Số tiền sau đồng chi trả, **làm tròn tại bước này** |

**Thừa kế được tuyên bố cho đồng chi trả và CHỈ cho đồng chi trả.** `200.1`, `200.2`, `200.3` đều thừa kế 20% của `200`. Con có thể override; **không con nào trong sản phẩm này override.** Trên wire, `coPaymentBp` có mặt trên **mọi** node, mang chuỗi `"INHERIT"` ở nơi tỷ lệ đến từ tổ tiên (D3 §I-05).

**Đồng chi trả luôn áp dụng khi benefit được dùng.** Nó không có điều kiện. Không ai sửa được nó.

---

### Bước 11 — `CH-11 DeductionRatio`

| | |
|---|---|
| **Cấp áp dụng** | **Per insured event** — *"one claim can carry a network visit and a non-network visit"* |
| **Áp dụng khi** | Benefit mang tỷ lệ **và điều kiện của nó được thoả** |
| **Quy tắc (verbatim A5 §6.1)** | *"**From the policy**, not from the assessor. Base: the amount remaining after step 10. The triggering condition is recorded on the claim"* |
| **Base amount (verbatim A5 §6.2)** | *"The amount remaining **after** co-payment has been deducted at **step 10** — i.e. the insurer's share, not the billed amount. **The base *amount* is settled; the base *set of lines* is not — `B-19`**"* |
| **Dữ liệu vào** | `deduction_pct`, `deduction_condition` từ snapshot; cơ sở y tế ghi trên insured event; cờ `in_network` trên `provider` master (F57) |
| **Đầu ra** | Số tiền sau giảm trừ + **điều kiện kích hoạt ghi lên claim** |
| **Mã từ chối sinh ra** | **Không có** |
| **Base amount cho bước sau** | Số tiền sau giảm trừ, làm tròn. **Trên seed hiện tại: không đổi so với bước 10, vì bước này không fire** |

**Bảng điều kiện (A5 §4.3, nguyên văn):**

| Condition | Deduction | Applies to |
|---|---|---|
| Treatment at a **provider outside the listed network** | **20%** *(illustrative)* | Benefit groups `200`, `300` |
| All other circumstances | none | — |

**Khi không thoả điều kiện, bước bị BỎ QUA hoàn toàn** — *"it is not a zero-rate deduction, it is no deduction."* D2 V-540: bước 11 vắng khỏi màn hình và bước 11 áp ở 0% là **không phân biệt được**, nên mọi bước đều phải hiện.

**Cơ sở y tế vắng mặt trong master được coi là ngoài mạng lưới**, và đó là cố ý — mặc định an toàn là mặc định không âm thầm trả ở mức cao hơn. **Cái giá:** D2 §5.17 — *"if the provider master is not loaded, every claim the deduction reaches is cut by 20%"*.

**`B-19` — bước này KHÔNG FIRE trên sản phẩm pilot.** `deduction_pct` chỉ nằm trên `200` và `300` (đều là **cha**), và **mọi cost line map vào con** (`200.1`, `200.2`, `200.3`, `300.1`). Không có gì trong 14 bước áp tỷ lệ giảm trừ của tổ tiên lên số tiền của hậu duệ. Xem §8 và §3 — hành vi tạm là **không fire**, tức **trả thừa chứ không trả thiếu**.

---

### Bước 12 — `CH-12 ParentAnnualCap`

| | |
|---|---|
| **Cấp áp dụng** | **Per benefit, per counter period, across every line of the claim on that ancestor path** |
| **Quy tắc (verbatim A5 §6.1)** | *"Walk the ancestor chain to the root"* |
| **Dữ liệu vào** | `annual_cap` trên từng node tổ tiên; bộ đếm `AMOUNT` đã tiêu thụ **chính thức** trong `periodKey` |
| **Đầu ra** | Số tiền trong hạn mức năm còn lại; phần vượt |
| **Mã từ chối sinh ra** | `LIMIT` |
| **Base amount cho bước sau** | Số tiền sau hạn mức năm, làm tròn |

**`included` nghĩa là gì:** *"`included` benefits have no annual cap of their own. `100.1` room and board is capped at 1,500,000 per day and by the 100,000,000 annual cap of `100` — nothing in between."*

**`BR-TREE-2`:** khi một composite và chính sub-limit của nó cùng áp lên một dòng, tiêu thụ của composite được đếm **một lần**. Roll-up hiện một posting cho mỗi tổ tiên, không phải một posting mỗi cấp.

**`BR-TREE-5`:** **cap nào bind thì phải giải thích, luôn luôn.** Dòng phải nêu benefit, hạn mức còn lại và mức cắt, và đúng câu đó chảy vào `CLM-LETTER-001`. **Cắt thì được; cắt lặng lẽ thì không.**

**`B-20` — thứ tự dòng tiêu thụ cap khi cap bind là chưa quyết.** Bốn dòng dưới một benefit còn 1.000.000 hạn mức có thể trả bốn cách, mọi cách đều thoả mọi rule, và mỗi cách cho một settlement khác nhau. Xem §8.

---

### Bước 13 — `CH-13 PlanLevelCheck`

| | |
|---|---|
| **Cấp áp dụng** | **The whole claim** |
| **Quy tắc (verbatim A5 §6.1)** | *"No plan-wide aggregate cap in this product; the step exists for future products"* |
| **Dữ liệu vào** | — |
| **Đầu ra** | `stepOutcome = KHÔNG_ÁP_DỤNG` |
| **Mã từ chối sinh ra** | Không có trên sản phẩm này |
| **Base amount cho bước sau** | Không đổi |

Như bước 7: cơ chế không có instance, **và vẫn phải hiện lên màn hình**.

---

### Bước 14 — `CH-14 PostLedger`

| | |
|---|---|
| **Cấp áp dụng** | **Per cost line**, tới **mọi bộ đếm trên đường tổ tiên** (`BR-TREE-1`, `BR-TREE-2`) |
| **Quy tắc (verbatim A5 §6.1)** | *"Provisional at calculation; final at approval. Posts to the benefit **and every ancestor**, and to every counter type on that path (§4.2). A composite and its own sub-limit are never counted twice on the same claim line"* |
| **Dữ liệu vào** | Số tiền chi trả cuối cùng theo dòng; đường tổ tiên; `periodKey` theo ngày điều trị |
| **Đầu ra** | Bản ghi `LimitLedger` **tạm** (provisional). Chuyển thành **chính thức tại thời điểm approval**, không phải tại payment |
| **Mã từ chối sinh ra** | Không có |
| **Base amount cho bước sau** | — (bước cuối) |

**Tám quy tắc ledger (`BR-LEDGER-1…8`) áp cho **cả ba** loại bộ đếm, không chỉ tiền:** hai pha (tạm tại tính toán, chính thức tại duyệt); **không bao giờ xoá, chỉ đảo hoặc bù** (`BR-LEDGER-3`); version hoá cho optimistic concurrency **theo từng `counterType`** (`BR-LEDGER-5`); post lên **mọi** bộ đếm trên đường tổ tiên (`BR-LEDGER-6`); khi va version thì **chạy lại adjudication**, không post lại con số lần chạy đầu (`BR-LEDGER-8`).

**Vì sao final tại approval chứ không tại payment:** *"An approved claim has committed the limit. A second claim adjudicating at the same moment must see that commitment, not a provisional figure that might evaporate — otherwise two legitimately approved claims jointly exceed an annual limit."*

**`BR-LEDGER-2`:** tiêu thụ tạm **vô hình với các claim đang chạy khác**. Hai claim cùng thành viên thẩm định cùng lúc đều thấy hạn mức đầy và đều tính ra payable đầy. Khi claim thứ nhất được duyệt, số của claim thứ hai **dịch chuyển**, và `V-546` là cơ chế: chuỗi chạy lại trên bộ đếm hiện tại và giám định viên được báo cái gì đổi và vì sao. Đó là cái giá, và nó mua được điều đắt hơn: **hai claim không bao giờ cùng được duyệt vào một tổng vượt hạn mức năm.**

---

## 2. Bảy mã từ chối

**Nguyên văn A5 §6.3, kèm cột bước sinh ra (suy từ §6.1 và §8.2):**

| Code | Reason (EN) | Reason (VI) | Level | Bước sinh ra |
|---|---|---|---|---|
| `LIMIT` | Benefit limit exhausted | Hết hạn mức | Event or line | **8**, **9**, **12** (và **7** khi term mang `CAP`, `BR-TERM-12`) |
| `PERIOD` | Event date outside the cover period | Ngày xảy ra sự kiện BH không thuộc thời hạn BH | Event | **1** |
| `NOBEN` | The plan does not include this benefit | Chương trình bảo hiểm không bao gồm quyền lợi này | Event or line | **6** (dòng rơi vào `"NONE"` ở bước 5) |
| `WAIT` | Waiting period not yet served | Chưa đủ thời gian chờ | Event or line | **2** |
| `EXCL` | Falls under a policy exclusion | Thuộc điểm loại trừ của quy tắc BH | Event or line | **4** (term register), **7** (proviso), **6** (rule *Excluded category*) |
| `NOTREL` | Not related to the diagnosis or treatment | Không liên quan đến chẩn đoán và điều trị | **Line only** | **6** |
| `NOTCOV` | Item is not a covered medical item | Không thuộc danh mục được chi trả | **Line only** | **6** |

> **The last two exist only at cost-line level and are what the AI-drafted rationale populates.** *(A5 §6.3)*

**Bảy mã này là tập đóng.** `decline_reason` là reference data của Claim Platform (`code`, `text_vi`, `text_en`, `level`); A1 §5.2 là normative cho mã.

**Ba mã nguyên nhân KHÔNG phải mã từ chối** nhưng xuất hiện trong bảng "Vì sao bạn tự chịu" (BRD FR-POR-08): `COPAY` (bước 10), `CAP_VISIT`/`CAP_DAY`/`CAP_EVENT` (bước 8), `LIMIT_YEAR` (bước 12). Đừng trộn chúng vào `decline_reason` — chúng là **cause code cho breakdown**, không phải lý do từ chối.

**Cạm bẫy `NOTCOV` vs `EXCL`** (A5 §7.4, nguyên văn về defect đã sửa): *"The ground is an exclusion, so the correct code is **`EXCL`** — *"thuộc điểm loại trừ của quy tắc BH"*. The two are different things to say to a customer and they carry different appeal routes."* Thực phẩm chức năng **không** phải `NOTCOV` — nó là `EXCL` kèm clause, quyết ở bước 4. Phí hành chính **mới là** `NOTCOV`, quyết ở bước 6.

---

## 3. Cổng tự động duyệt

### 3.1 Bốn điều kiện — và chúng đến từ hai nguồn

A5 §6.4 quy định **ba** điều kiện. BRD hackathon §9.1.3 quy định **bốn** (thêm G4 fraud signal, vì slice hackathon có agent AG-4 mà Claim MVP không có). **Bốn điều kiện dưới đây là hợp nhất của hai bộ và là bộ phải hiện thực hoá.**

| Cond. | Tên | Quy tắc | Tham số ngưỡng | Nguồn |
|---|---|---|---|---|
| **G1** | **Số tiền & không có gì bị cắt** | Tổng payable **ở hoặc dưới** ngưỡng cấu hình, **và không có gì bị cắt** — không cost line nào bị decline, không limit nào bind | **`AutoApproveThreshold` = 2.000.000 VND** (D1 B-03, đã chốt 17/08/2026), đổi được bởi administrator **không cần release** | A5 §6.4 cond.1 + BRD G1 |
| **G2** | **Chứng từ & chất lượng trích xuất** | Mọi chứng từ Required ở trạng thái `Read`; claim confidence ≥ ngưỡng; completeness = 100%; `CrossCheckDocuments` không có `Fail`; không dòng nào `UNMAPPED`; không dòng nào dưới sàn confidence mà chưa được verify | **`MinClaimConfidence` = 0.90** · **`LineConfidenceFloor` = 0.80** | BRD G2 |
| **G3** | **Lâm sàng sạch & không có điểm kiểm mở** | Track 2 hoàn tất và **không trả về khuyến nghị nào**; không `ClinicalRecommendation` nào ở `Status = Proposed`; **không term checkpoint nào đang mở** | — | A5 §6.4 cond.2 **+** cond.3 (thêm ở v1.4) + BRD G3 |
| **G4** | **Không có tín hiệu gian lận / trùng lặp** | Không `FraudSignal` nào đang mở với severity ≥ Medium (ngày tương lai, bệnh viện không tra được, claim/chứng từ trùng, cửa sổ điều trị chồng lấn) | — | BRD G4 |

> **Anything else routes to an assessor with the failing condition named.** *(A5 §6.4)*

### 3.2 Hành vi khi fail từng điều kiện

| Fail | Trạng thái claim | Hàng đợi | Hiển thị cho khách | Ai gỡ |
|---|---|---|---|---|
| **G1** | `PENDING_REVIEW` | Adjudication | *"Đang xem xét"* | Giám định viên xem lại số tiền và các dòng bị cắt, gửi duyệt |
| **G2** | `PENDING_REVIEW` | Adjudication (hoặc Operator nếu là `EXTRACTION_ERROR`) | *"Đang xem xét"* / *"Cần bổ sung chứng từ"* | Giám định viên verify dòng confidence thấp (`MarkLineVerified`), map dòng `UNMAPPED`, xử `Fail` cross-check |
| **G3** | `PENDING_REVIEW` | Adjudication | *"Đang xem xét"* | Giám định viên **xác nhận hoặc bác** từng khuyến nghị **kèm lý do bắt buộc**; đóng mọi checkpoint. Chuỗi **chạy lại toàn bộ** (`BR-TRACK-4`), không vá |
| **G4** | **`BLOCKED`** | Blocked | *"Đang xem xét"* | Giám định viên `ClearSignal` kèm lý do → G4 đánh giá lại. Nếu giữ signal → đường decline với lý do `FRAUD`. **Claim không bao giờ tự động tới payment** |

**Điểm phải làm đúng:** G4 fail cho ra **`BLOCKED`**, không phải `PENDING_REVIEW`. Hai trạng thái khác nhau vì `BLOCKED` chặn payment bất kể số tiền.

### 3.3 Những gì cổng KHÔNG kiểm

| Không kiểm | Hệ quả |
|---|---|
| **Confidence trích xuất như một điều kiện của cổng** | *"Extraction confidence is deliberately not a condition of the gate: it drives assessor routing only"* (A5 §6.4). **Lưu ý:** BRD G2 đưa confidence **vào** cổng. Đây là một khác biệt có chủ ý của slice hackathon — ghi lại, đừng để nó thành defect im lặng |
| **Tổng tích luỹ qua nhiều claim của một thành viên** | *"The threshold is tested per claim, so three claims of 1,900,000 each pass a 2,000,000 gate and 5,700,000 settles with no human involved"* — **`B-22`**, còn mở |

### 3.4 Vì sao G3 tồn tại, và nó tốn gì

> **Why condition 3 exists, and what it costs.** A class-J term is by definition one no engine may decide (§7.1). If an open checkpoint did not block the gate, the platform would auto-approve a claim on which a judgement was required and never made — precisely the failure the three-class model exists to prevent. The cost is real and was accepted with it stated: **any claim touching a judgemental exclusion leaves the straight-through path**, and with three judgemental exclusions out of ten, cosmetic treatment among them, that will affect a measurable share of dental and dermatology claims.
> *(A5 §6.4)*

**Định lượng tác động lên tỷ lệ STP:** sổ điều khoản có **2 class J**, **2 class D mang `CHECKPOINT`**, **1 class E** → **5 term có thể raise checkpoint**. Cặp class-D (khám sức khoẻ/tiêm chủng, thực phẩm chức năng) là **phần khối lượng lớn** — tỷ lệ auto-approve giảm **trên claim thường quy**, không phải claim bất thường.

---

## 4. Ví dụ chạy tay

### 4.1 Ví dụ gốc A5 §6.2 — trích nguyên văn, dùng làm test case #1

> **Worked example.** Outpatient visit, Standard plan. Four lines billed at 3,000,000: consultation 300,000 and laboratory 1,400,000, both mapping to `200.1`; medication 900,000, mapping to `200.2`; and a dietary supplement at 400,000.

| Step | What happens | Running total |
|---|---|---|
| 4 | The dietary supplement raises a **term checkpoint** on `TERM-HC-003`. The assessor confirms it was not prescribed as treatment for the diagnosed condition, so the line is declined **`EXCL`**, citing the clause | 2,600,000 |
| 8 | `200.1` holds 1,700,000 against a 1,500,000 per-visit cap — **200,000 cut**. `200.2` holds 900,000, under its 1,000,000 cap | 2,400,000 |
| 10 | 20% co-payment on 2,400,000 — the member bears 480,000 | **1,920,000** |
| 11 | Skipped — the provider is in-network, so no deduction condition was met | 1,920,000 |
| 12 | 1,920,000 checked against `200`'s remaining annual cap | 1,920,000 |
| 14 | Posted provisionally: 1,200,000 to `200.1`, 720,000 to `200.2`, both rolling up to `200` | remaining 18,080,000 |

**Bảng đầy đủ 14 bước — cùng hồ sơ, mọi bước hiện ra như D2 V-540 yêu cầu:**

| Bước | Tên | Kết quả | Cơ sở tính | Tham số | Tác động | Còn lại |
|---|---|---|---|---|---|---|
| 1 | Member eligibility | `ÁP_DỤNG` | 3.000.000 | policy in force, premium paid | — | 3.000.000 |
| 2 | Waiting period | `ÁP_DỤNG` | 3.000.000 | `GEN` / nhóm `200` → 30 ngày, đã phục vụ | — | 3.000.000 |
| 3 | Pre-existing condition | `KHÔNG_ÁP_DỤNG` | 3.000.000 | `conditionClass ≠ PRE` | — | 3.000.000 |
| 4 | General exclusions | `KHÔNG_ĐẠT` | 3.000.000 | `TERM-HC-003`, `serviceCategory = SUPPLEMENT`, `EXCL` | **−400.000** | **2.600.000** |
| 5 | Benefit mapping | `ÁP_DỤNG` | 2.600.000 | `CONSULT`→`200.1`, `LAB`→`200.1`, `DRUG`→`200.2` | — | 2.600.000 |
| 6 | Line-level cover test | `ÁP_DỤNG` | 2.600.000 | không dòng nào `NOTCOV`/`NOTREL` | — | **2.600.000** ← *Số tiền thuộc phạm vi bảo hiểm* |
| 7 | Benefit-level provisos | `KHÔNG_ÁP_DỤNG` | 2.600.000 | tập rỗng trên sản phẩm này | — | 2.600.000 |
| 8 | Sub-limit | `KHÔNG_ĐẠT` | 2.600.000 | `200.1` trần 1.500.000/lần (giữ 1.700.000); `200.2` trần 1.000.000/lần (giữ 900.000) | **−200.000** | **2.400.000** |
| 9 | Count limit | `KHÔNG_ÁP_DỤNG` | 2.400.000 | `200.1`, `200.2` đều `count_limit = 0` → không giới hạn | — | 2.400.000 |
| 10 | Co-payment | `ÁP_DỤNG` | **2.400.000** | `coPaymentBp = 2000` thừa kế từ `200` | **−480.000** | **1.920.000** |
| 11 | Deduction ratio | `KHÔNG_ÁP_DỤNG` | 1.920.000 | cơ sở trong mạng lưới → **điều kiện không thoả, bỏ qua** | — | 1.920.000 |
| 12 | Parent annual cap | `ÁP_DỤNG` | 1.920.000 | `200` hạn mức năm 20.000.000, còn 20.000.000 | — | 1.920.000 |
| 13 | Plan-level check | `KHÔNG_ÁP_DỤNG` | 1.920.000 | không có cap toàn plan | — | 1.920.000 |
| 14 | Post to ledger | `ÁP_DỤNG` | 1.920.000 | tạm: 1.200.000 → `200.1`, 720.000 → `200.2`, roll-up → `200` | — | **1.920.000 chi trả** · `200` còn **18.080.000** |

**Số tiền yêu cầu 3.000.000 → chi trả 1.920.000. Khách tự chịu 1.080.000**, chia theo nguyên nhân: `EXCL` 400.000 · `CAP_VISIT` 200.000 · `COPAY` 480.000.

**Cổng:** payable 1.920.000 **dưới** ngưỡng 2.000.000 — **nhưng G1 vẫn fail**, vì một dòng bị decline *và* một limit bind. **Và G3 cũng fail**, vì một term checkpoint đã mở ở bước 4. A5 §6.2 nói thẳng: *"**A cheap claim is not automatically a clean one**, and that distinction is the whole reason condition 1 has two halves."*

### 4.2 Biến thể off-network — trích nguyên văn, dùng làm test case #2

> **The same example with a deduction.** If the visit had been at a provider outside the listed network, the policy's 20% deduction ratio would apply at **step 11**, reducing 1,920,000 to **1,536,000** — and that, not 1,920,000, is what **steps 12 and 14** carry. Nothing about it is a judgement; the condition either held or it did not.

> **Read that last paragraph against `B-19` before building from it.** It applies the 20% to **1,920,000 — the aggregate of two lines, `200.1` and `200.2`, after co-payment.** Neither of those benefits carries a `deduction_pct`; **`200`, their parent, does.** So the arithmetic above is the **ancestor** reading of step 11, applied to a quantity that step 12 is the one that aggregates. **Under the other reading — step 11 tests the line's own benefit — nothing fires and the payable stays 1,920,000.**
>
> **The interim behaviour in the built system is that step 11 does not fire** — over-paying rather than under-paying — which is what the seed produces and what D1 v0.5 records.

**Kết luận cho build:** **hiện thực hoá cách đọc per-line** → bước 11 không fire → payable **1.920.000**. Ghi lên màn hình F48 dòng bước 11 là *"điều kiện không thoả / quy tắc tạm B-19"*. **Đừng hard-code 1.536.000 vào bất kỳ test nào** — nó là cách đọc thứ hai của một open item.

### 4.3 Kịch bản B hackathon — test case #3

BRD §9.1.3: *"requested 2,000,000 → line 4 supplement 80,000 `NOTCOV` (confirmed) + line 2 excess over per-visit cap 40,000 `CAP_VISIT` → payable **1,880,000 VND**."*

| | |
|---|---|
| **Cái đã cố định** | Chỉ tổng: **2.000.000 → 1.880.000** |
| **Cái là minh hoạ** | Việc chia 120.000 thành 80.000 + 40.000 (BRD §19 điểm 1) |
| **Cái phải sửa** | **Mã `NOTCOV` cho dòng thực phẩm chức năng là SAI** theo A5 §7.4. Phải là **`EXCL`** kèm `clauseRef`, quyết ở **bước 4**, không phải bước 6. Sửa kịch bản demo, không sửa A5 |
| **Và** | Ví dụ này **không có đồng chi trả** (2.000.000 − 120.000 = 1.880.000). Nếu là claim ngoại trú thật trên HC-STD thì 20% đồng chi trả phải áp ở bước 10 và số sẽ là 1.504.000. **Kịch bản B phải chọn: hoặc là claim nội trú (`100`, không copay), hoặc là claim tai nạn (`500`, không copay), hoặc phải tính lại tổng.** Đây là một mâu thuẫn số học phải đóng trước 22/09 |

---

## 5. Seed sản phẩm

### 5.1 Bảng quyền lợi — TRÍCH NGUYÊN VĂN A5 §4.2

> Notation: `included` means the benefit has no annual cap of its own and is capped by its parent. `0` in a count column means **unlimited**, per the sentinel convention. `—` means not applicable.

**Limits by plan tier (VND, illustrative)**

| Code | Benefit | Limit type | Standard | Plus | Premium | Count limit | Co-pay |
|---|---|---|---|---|---|---|---|
| **100** | **Inpatient treatment** | annual | 100,000,000 | 200,000,000 | 400,000,000 | 0 | — |
| 100.1 | Room and board, per day | per day | 1,500,000 | 2,500,000 | 4,000,000 | 60 days | — |
| 100.1.1 | Intensive care, per day | per day | 3,000,000 | 5,000,000 | 8,000,000 | 15 days | — |
| 100.2 | Surgery, per procedure | per event | 20,000,000 | 40,000,000 | 80,000,000 | 0 | — |
| 100.3 | Pre- and post-hospitalisation | per event | 3,000,000 | 5,000,000 | 8,000,000 | 0 | — |
| 100.4 | Emergency transport | per event | 2,000,000 | 3,000,000 | 5,000,000 | 0 | — |
| **200** | **Outpatient treatment** | annual | 20,000,000 | 35,000,000 | 60,000,000 | 0 | 20% / 20% / 10% |
| 200.1 | Consultation and diagnostic tests | per visit | 1,500,000 | 2,500,000 | 4,000,000 | 0 | inherits |
| 200.2 | Prescribed medication | per visit | 1,000,000 | 1,500,000 | 2,500,000 | 0 | inherits |
| 200.3 | Physiotherapy | per visit | 500,000 | 800,000 | 1,200,000 | 20 visits | inherits |
| **300** | **Dental — basic treatment** | annual | 5,000,000 | 8,000,000 | 15,000,000 | 0 | 30% / 30% / 20% |
| 300.1 | Basic treatment, per visit | per visit | 1,000,000 | 1,500,000 | 2,500,000 | 0 | inherits |
| **400** | **Maternity** | annual | 20,000,000 | 35,000,000 | 60,000,000 | 0 | — |
| 400.1 | Normal delivery | per event | 12,000,000 | 20,000,000 | 35,000,000 | 0 | — |
| 400.2 | Caesarean section | per event | 20,000,000 | 35,000,000 | 60,000,000 | 0 | — |
| 400.3 | Pregnancy complications | per event | 20,000,000 | 35,000,000 | 60,000,000 | 0 | — |
| **500** | **Accident — medical expenses** | annual | 50,000,000 | 80,000,000 | 150,000,000 | 0 | — |

**Cột "Co-pay" đọc theo thứ tự Standard / Plus / Premium.** `200` = 20% / 20% / **10%**; `300` = 30% / 30% / **20%**. Plan Premium có tỷ lệ đồng chi trả thấp hơn — đây là một số thật, không phải lỗi sao chép.

**Ba mã plan:** `HC-STD` (Standard) · `HC-PLS` (Plus) · `HC-PRM` (Premium). **17 benefit × 3 plan = 51 dòng** trong `benefit_schedule_line`.

**Khấu trừ (deductible):** **không có trên sản phẩm này, ở mọi plan, mọi benefit.** D1 §5.6 nguyên văn: *"Deductible (*mức miễn thường*) is out of scope. The pilot product has none. If a future product needs it, it enters as a step between 9 and 10."* **Đừng seed cột `Deductible` với 0** — 0 và "không tồn tại" khác nhau; để `null`.

### 5.2 Thời gian chờ theo loại tổn thất — TRÍCH NGUYÊN VĂN A5 §5

Waiting periods are **per benefit group and per condition class**, not a single value per benefit.

| Condition class | Code | Inpatient (100) | Outpatient (200) | Dental (300) | Maternity (400) | Accident (500) |
|---|---|---|---|---|---|---|
| Pre-existing condition | `PRE` | 365 d | 365 d | 365 d | n/a | 0 d |
| Specified condition *(see list)* | `SPE` | 365 d | 365 d | n/a | n/a | 0 d |
| General illness | `GEN` | 30 d | 30 d | 90 d | n/a | 0 d |
| Childbirth | `BIR` | n/a | n/a | n/a | 270 d | n/a |
| Maternity complication | `MAT` | n/a | n/a | n/a | 90 d | n/a |
| Accident | `ACC` | 0 d | 0 d | 0 d | n/a | 0 d |

**Specified conditions (`SPE`)** — illustrative list, to be confirmed by the business: hernia of all types · haemorrhoids · tonsils and adenoids · sinusitis · nasal septum deviation · gallstones · kidney and urinary stones · benign tumours and cysts · uterine fibroids · endometriosis · cataract · thyroid disorders.

**`n/a` KHÔNG được seed là 0 và KHÔNG được seed chút nào.** `n/a` nghĩa là tổ hợp **không thể phát sinh**; nếu nó phát sinh thì đó là lỗi dữ liệu, không phải thời gian chờ 0 ngày (`BR-WAIT-4`). Số dòng `benefit_waiting_period` **mỗi plan = 17** (nhóm 100: 4 · 200: 4 · 300: 3 · 400: 2 · 500: 4), **× 3 plan = 51 dòng**.

### 5.3 Tỷ lệ giảm trừ — TRÍCH NGUYÊN VĂN A5 §4.3

| Condition | Deduction | Applies to |
|---|---|---|
| Treatment at a **provider outside the listed network** | **20%** *(illustrative)* | Benefit groups `200`, `300` |
| All other circumstances | none | — |

`deduction_condition` có **một** giá trị trong MVP 1: **`PROVIDER_OFF_NETWORK`**. Bắt buộc non-null khi `deduction_pct` non-null, và null khi `deduction_pct` null (`V-114`).

### 5.4 Mapping `serviceCategory` → benefit — TRÍCH NGUYÊN VĂN A5 §8.1

| Category code | Service category | Outpatient episode | Inpatient episode |
|---|---|---|---|
| `CONSULT` | Consultation / examination fee | 200.1 | 100 |
| `LAB` | Laboratory test | 200.1 | 100 |
| `IMAGING` | Diagnostic imaging | 200.1 | 100 |
| `PROCEDURE` | Surgery and procedure | 200.1 | 100.2 |
| `DRUG` | Medication | 200.2 | 100 |
| `SUPPLEMENT` | Supplement / functional food | 200.2 | 100 |
| `PHYSIO` | Physiotherapy | 200.3 | 100 |
| `DENTAL` | Dental procedure | 300.1 | n/a |
| `ROOM` | Room and board | n/a | 100.1 |
| `ICU` | Intensive care | n/a | 100.1.1 |
| `AMBULANCE` | Ambulance / emergency transport | 100.4 | 100.4 |
| `DELIVERY_NORMAL` | Delivery — normal | n/a | 400.1 |
| `DELIVERY_CS` | Delivery — caesarean | n/a | 400.2 |
| `MATERIAL` | Medical materials and consumables | 200.1 | 100 |
| `ADMIN` | Administrative and non-medical fee | n/a | n/a |
| `SCREENING` | Health check / screening | 200.1 | 100 |
| `VACCINATION` | Vaccination / immunisation | 200.2 | 100 |
| `OPTICAL` | Spectacles, contact lenses and optical appliances | 200.1 | 100 |
| `HEARING` | Hearing aids and audiology appliances | 200.1 | 100 |
| `OTHER` | Anything else | assessor decides | assessor decides |

Trong seed JSON, `n/a` là chuỗi **`"NONE"`** và *assessor decides* là chuỗi **`"ASSESSOR"`**. **20 dòng, và không dòng nào decline.**

### 5.5 Phí bảo hiểm — TRÍCH NGUYÊN VĂN A5 §11

Annual premium per insured person, VND, illustrative. Age at inception, last birthday.

| Age band | Standard | Plus | Premium |
|---|---|---|---|
| 0 – 5 | 4,800,000 | 7,900,000 | 13,500,000 |
| 6 – 17 | 3,600,000 | 5,900,000 | 10,200,000 |
| 18 – 30 | 4,200,000 | 6,800,000 | 11,800,000 |
| 31 – 40 | 5,100,000 | 8,300,000 | 14,300,000 |
| 41 – 50 | 6,900,000 | 11,200,000 | 19,400,000 |
| 51 – 60 | 9,800,000 | 15,900,000 | 27,500,000 |
| 61 – 65 | 14,200,000 | 23,100,000 | 39,800,000 |

**Rules:** premium is the sum of the per-member premiums; no family discount, no loading, no short-period pro-rata in MVP 1 (all members take the policy period). Maternity is included in the premium above and is simply unavailable to ineligible members — no separate rate.

Seed là **bảng rộng, 7 dòng**: `band`, `age_from`, `age_to`, và **một cột tiền cho mỗi mã plan** (`HC-STD`, `HC-PLS`, `HC-PRM`). Loader chuẩn hoá sang dạng dài (21 dòng) là phép biến đổi hợp lệ **và phải nói ra**.

### 5.6 Bảng shape của từng collection seed

| Collection | Chủ sở hữu | Cột | Số bản ghi |
|---|---|---|---|
| `product` | InsureMO (F01) | `code`, `name`, `line`, `currency`, `term_months` — **một object, không phải mảng**. **Không có `status`, không có `effective_from/to`** | 1 |
| `plans` | InsureMO (F01) | `product_code`, `plan_code`, `name` | 3 |
| `benefit_schedule_line` | InsureMO (F07) | `benefit_code`, `parent_benefit_code`, `level`, `name`, `plan_code`, `annual_cap`, `per_event_cap`, `per_day_cap`, `count_limit`, `count_unit`, `copay_pct`, `deduction_pct`, `deduction_condition` | **51** |
| `benefit_waiting_period` | InsureMO (F07) | `plan_code`, `benefit_code`, `condition_class`, `days` | **51** |
| `premium_rate` | InsureMO (F03) | `band`, `age_from`, `age_to`, `HC-STD`, `HC-PLS`, `HC-PRM` — **bảng rộng** | 7 |
| `policy` | InsureMO (F05) | + `rule_set_ref` — **mới ở seed v1.5**. Thiếu nó thì **bước 4 chọn 0 term trên mọi mock** | 2 |
| `member` | InsureMO (F05) | + `continuous_cover_from` — **mới ở seed v1.5**. `null` nghĩa **thành viên lần đầu**, không bao giờ nghĩa *không xác định được* | 12 |
| `provider` | **Claim Platform** master data (F57) | `provider_id`, `name`, `province`, `country`, `in_network` — **6 dòng, một dòng cố ý off-network, một dòng ngoài Việt Nam** | 6 |
| `policy_term` | **Claim Platform** master data (F65) | `termId`, `termClass`, `scope`, `effect`, `reason`, `clauseRef`, `selector`, `predicate`, `termStatus` | **10** (6 `ACTIVE`, 4 `DRAFT`) |
| `service_category_benefit_map` | Claim Platform reference | `category`, `name`, `outpatient`, `inpatient` — **hai cột benefit, không có cột `setting`** | **20** |
| `adjudication_rule` | Claim Platform config | **`step`, `name`, `enabled`** — **không có `benefit_code`, và chưa từng có** | **14** |
| `decline_reason` | Claim Platform reference | `code`, `text_vi`, `text_en`, `level` | **7** |
| `line_decline_rule` | — | **KHÔNG TỒN TẠI.** Chưa từng có phiên bản seed nào chứa collection này. 4 rule của §8.2 là **thân rule trong code** | 0 |

**Tổng: 215 bản ghi** trong `A5_seed_v1.5.json` (v1.4 có 209; 6 bản ghi mới đều là collection `provider`).

**`copay_pct` trong seed là phần trăm nguyên (`20`). Mọi payload interface mang basis point nguyên (`2000`).** Quy đổi `bp = pct × 100` **thuộc về script seed, không thuộc về benefit engine** — một phần trăm và một basis point đều trông như số nguyên hợp lý và **không phân biệt được lúc chạy**, nên ranh giới phải nằm ở đúng một chỗ.

---

## 6. Cây quyền lợi

### 6.1 Cấu trúc — TRÍCH NGUYÊN VĂN A5 §4.1

```
100     Inpatient treatment                          annual cap
 ├ 100.1    Room and board, per day                  per-day cap · day count
 │  └ 100.1.1  Intensive care, per day               per-day cap · day count
 ├ 100.2    Surgery, per procedure                   per-event cap
 ├ 100.3    Pre- and post-hospitalisation            per-event cap
 └ 100.4    Emergency transport                      per-event cap
200     Outpatient treatment                         annual cap · co-payment
 ├ 200.1    Consultation and diagnostic tests        per-visit cap
 ├ 200.2    Prescribed medication                    per-visit cap
 └ 200.3    Physiotherapy                            per-visit cap · visit count
300     Dental — basic treatment                     annual cap · co-payment
 └ 300.1    Basic treatment, per visit               per-visit cap
400     Maternity                                    annual cap
 ├ 400.1    Normal delivery                          per-event cap
 ├ 400.2    Caesarean section                        per-event cap
 └ 400.3    Pregnancy complications                  per-event cap
500     Accident — medical expenses                  annual cap
```

**Năm gốc · 17 node · độ sâu tối đa 3 (`100.1.1`).** `maxCount = 0` trên **14/17** node — ba ngoại lệ là `100.1` (60 `DAY`), `100.1.1` (15 `DAY`), `200.3` (20 `VISIT`). `coPaymentBp = "INHERIT"` trên **4** node (`200.1`, `200.2`, `200.3`, `300.1`). `deductionRatioBp = 2000` với `deductionCondition = PROVIDER_OFF_NETWORK` chỉ trên **`200` và `300`**.

### 6.2 Ba điểm cấu trúc engine PHẢI tôn trọng — nguyên văn A5 §4.3

1. **`included` benefits have no annual cap of their own.** `100.1` room and board is capped at 1,500,000 per day and by the 100,000,000 annual cap of `100` — nothing in between.
2. **Co-payment is inherited down the tree unless overridden.** `200.1`, `200.2` and `200.3` all inherit the 20% co-payment of `200`. A child may override with its own rate; none does in this product.
3. **`100.1.1` intensive care is a child of `100.1` room and board.** A day in ICU consumes the ICU per-day limit, *and* one day of the 60-day room-and-board count, *and* the annual inpatient cap. **This is the single best test of whether a platform implements the tree correctly.**

### 6.3 Tám test cấu hình InsureMO — đã pass 21/08/2026, dùng làm test suite cho ODC

| # | Test | Pass criterion |
|---|---|---|
| T1 | Configure the tree | `100 → 100.1 → 100.1.1` exists as a three-level parent/child structure |
| T2 | Child-to-parent aggregation | A payment configured under `100.1.1` is shown to consume `100.1` and `100` |
| T3 | Two limit dimensions | `100.1` carries both 1,500,000 per day **and** a 60-day count |
| T4 | Unlimited representation | `100.2` carries a per-event money limit and **no** count limit, expressed unambiguously |
| T5 | Inherited co-payment | `200.1` inherits 20% from `200` without the value being repeated on the child |
| T6 | Waiting period by condition class | Benefit group `200` carries 365 / 365 / 30 days for `PRE` / `SPE` / `GEN` |
| T7 | Three plan tiers | The same tree with three sets of amounts, without duplicating the structure |
| T8 | Several levels on one policy | A policy carrying `HC-STD` and `HC-PLS`, with members assigned to each |

---

## 7. Điều khoản loại trừ

### 7.1 Ba lớp, và quy tắc duy nhất có trọng lượng — nguyên văn A5 §7.1

| Class | Meaning | The engine may | The engine may never |
|---|---|---|---|
| **D — deterministic** | Decidable from facts the claim already holds | Decline automatically | — |
| **J — judgemental** | Requires a person, or a clinical opinion | Raise a **checkpoint** with the clause quoted | Decline automatically |
| **E — evidential** | Decidable only if a named document says so | Require the document; raise a checkpoint if it is absent | Decline automatically |

> **Only class D may auto-decline. J and E may never** (decision **D-2**). A class-J term auto-declined is a decline citing a clause on a judgement no human made — invisible to us, and a regulatory and reputational exposure. **The engine proposes, a person decides.**

**`termClass` và `effect` là HAI trục, không phải một.** `termClass` trả lời *hệ thống có quyết được không* — nó chi phối auto-decline. `effect` trả lời *chuyện gì xảy ra khi term có mặt*. Một term class D **được phép** mang `CHECKPOINT`, và hai trong mười cái làm thế. Cái bị cấm là chiều ngược lại — **class J hoặc class E mang `DECLINE_EVENT` hoặc `DECLINE_LINE`** (`BR-TERM-7`, `V-742`).

**Hai biểu thức, không phải một:**

| Field | Decides | Required |
|---|---|---|
| **`selector`** | Whether the term is **in play** for this claim, this event or this line | **Mandatory and non-null on every class — `D`, `J` and `E` alike** |
| **`predicate`** | Whether the term **fires** — the auto-decline | **Class `D` only**, and only where the effect is `DECLINE_EVENT` or `DECLINE_LINE`. **Empty on `J`, on `E`, and on a class-D term carrying `CHECKPOINT`** |

**`selector` null KHÔNG nghĩa là "không được chọn" — nó nghĩa là "có mặt trên mọi claim".** Đó là chính cái lỗ mà cặp `selector`/`predicate` sinh ra để bịt.

### 7.2 Mười điều khoản — TRÍCH NGUYÊN VĂN A5 §7.2

`clauseRef` is a placeholder in every row, in the visibly-provisional `TBC-nn` form, because the filed wording does not exist. **No decline may be issued citing a `TBC-` reference.**

| # | Term | Class | Scope | Effect | Reason | Clause | `selector` | `predicate` |
|---|---|---|---|---|---|---|---|---|
| 1 | Cosmetic and aesthetic treatment, and its complications | **J** | `PRODUCT` | `CHECKPOINT` | `EXCL` | `TBC-01` | `diagnosisCode startsWith Z41` | **None. Class J may not carry one** |
| 2 | Health checks, screening and vaccination not connected to a diagnosed condition | **D** | `PRODUCT` | `CHECKPOINT` | `EXCL` | `TBC-02` | `serviceCategory in [SCREENING, VACCINATION]` | **None** — the effect is a checkpoint |
| 3 | Dietary supplements, functional foods, vitamins and minerals not prescribed as treatment | **D** | `PRODUCT` | `CHECKPOINT` | `EXCL` | `TBC-03` | `serviceCategory = SUPPLEMENT` | **None** — a person decides |
| 4 | Spectacles, contact lenses, hearing aids and their fitting | **D** | `PRODUCT` | `DECLINE_LINE` | `EXCL` | `TBC-04` | `serviceCategory in [OPTICAL, HEARING]` | **The same expression** |
| 5 | Infertility treatment, assisted reproduction, contraception, voluntary termination | **D** | `PRODUCT` | `DECLINE_LINE` | `EXCL` | `TBC-05` | `lineDiagnosisCode startsWith` any of `N46` · `N97` · `Z30` · `Z31` · `O04` | **The same expression. `DRAFT` — waits on Q-IDP-02** |
| 6 | Dental treatment other than the basic treatment covered under `300` | **D** | `PRODUCT` | `DECLINE_LINE` | `EXCL` | `TBC-06` | `serviceCategory = DENTAL` | `serviceCategory = DENTAL` **and** `lossType in [ILL_IP, DEN_IP, MAT_IP]` |
| 7 | Treatment for alcohol or drug dependency, and injury under their influence | **J** | `PRODUCT` | `CHECKPOINT` | `EXCL` | `TBC-07` | `diagnosisCode startsWith F1` | **None. Class J may not carry one** |
| 8 | Self-inflicted injury, and injury sustained committing a criminal offence | **E** | `PRODUCT` | `REQUIRE_DOC` | `EXCL` | `TBC-08` | `lossType in [ACC_OP, ACC_IP]` | **None. Class E may not carry one. `DRAFT`** |
| 9 | Congenital conditions and their consequences | **D** | `PRODUCT` | `DECLINE_LINE` | `EXCL` | `TBC-09` | `lineDiagnosisCode startsWith Q` | **The same expression. `DRAFT` — waits on Q-IDP-02** |
| 10 | Treatment received outside Vietnam | **D** | `PRODUCT` | `DECLINE_EVENT` | `EXCL` | `TBC-10` | `facilityCountry ≠ VN` | **The same expression. Event-level, so it declines the whole event and outranks any line result. `DRAFT` — waits on Q-IDP-05** |

**Sáu term live trên sản phẩm này: #1, #2, #3, #4, #6, #7.** Bốn `DRAFT` (#5, #8, #9, #10) **vô hình với engine** — không được đánh giá, không sinh outcome, không được đếm.

**Từ vựng fact đóng — đúng 5 giá trị:** `lossType` · `serviceCategory` · `lineDiagnosisCode` · `facilityCountry` · `diagnosisCode`. **`benefitCode` bị cấm ở scope `PRODUCT`** (`BR-TERM-9`) — nó do bước 5 sinh ra và term `PRODUCT` chạy ở bước 4. Thêm một fact là **thay đổi đặc tả, không phải thay đổi cấu hình**.

### 7.3 Hai defect lịch sử phải biết để không tái tạo

**Defect trên `TERM-HC-006` (đã sửa ở v1.5).** Predicate cũ là `serviceCategory = DENTAL` **and not** `benefitCode in [300, 300.1]`. Hai cái sai độc lập:
- **Nó không thể fire** — `benefitCode` null ở bước 4, mọi claim, mọi lần. Biểu thức chạy, chạy trên hư vô, và term hoặc không bao giờ fire hoặc luôn fire tuỳ engine xử null thế nào.
- **Nơi nó đọc được thì nó decline sai claim** — redirect tai nạn của §8.1 đưa mọi benefit về `500`; dòng nha khoa trong sự kiện tai nạn mang `benefitCode = 500`, không nằm trong `[300, 300.1]`, nên predicate **true**. **Răng gãy trong tai nạn giao thông là claim nha khoa được bảo hiểm điển hình nhất, và term này tự động từ chối nó, trích dẫn một điều khoản loại trừ nha khoa.**

**Defect `SUPPLEMENT` và `ADMIN` (đã sửa ở v1.4).** Bảng mapping từng mang `SUPPLEMENT → DECLINE:NOTCOV` và `ADMIN` cũng vậy. Bốn cái sai:

| | What the mapping table did | Why it is wrong |
|---|---|---|
| 1 | Declined at **step 5**, benefit mapping | The contractual ground is an exclusion, and exclusions are step 4 |
| 2 | Cited **`NOTCOV`** | The ground is an exclusion, so the correct code is **`EXCL`**. The two carry different appeal routes |
| 3 | Cited **no clause at all** | The decline letter cannot quote the term it relies on |
| 4 | Declined **unconditionally** | **A supplement that was prescribed for the condition is covered by the wording and was declined by the system** |

**Nguyên tắc tổng quát:** *a mapping table that declines is a mapping table making cover decisions.* Category không có benefit → rơi xuống cover test ở bước 6. Category bị hợp đồng loại trừ → rơi vào term register ở bước 4.

### 7.4 Những gì giám định viên thấy, và những gì kiểm toán viên được nợ

> Every term in scope is evaluated and **every result is recorded, whether it fired or not** — *"6 terms tested · 1 declined · 3 checkpoints raised · 2 passed"*, not the first hit alone.

**Quy tắc đếm:** *"The **terms tested** figure is the number of `ACTIVE` terms, inside their effective window, bound to the claim's `ruleSetRef`, whose `selector` was evaluated. It excludes every `DRAFT` and every `WITHDRAWN` term, and it includes terms whose selector did not match"* — những cái đó ghi là `KHÔNG_THUỘC_PHẠM_VI`, vì *đã xét và không áp dụng* là một câu trả lời khác với im lặng.

**Nhiều term cùng decline một dòng → TẤT CẢ `clauseRef` vào thông điệp decline.** Một dòng bị từ chối trên hai điều khoản là bị từ chối trên hai điều khoản, và một lá thư chỉ trích một trong hai là lá thư khách hàng bác được bằng cách bác điều khoản sai. **A2 chưa mang được điều này — `EXCL` bind một `{exclusionRef}` duy nhất — và đó là `D2-178`, còn mở.**

---

## 8. Cảnh báo triển khai

### 8.1 Mười một chỗ dễ làm sai

| # | Cạm bẫy | Hiện thực SAI | Hiện thực ĐÚNG | Bằng chứng |
|---|---|---|---|---|
| **W-1** | **Cap áp per benefit, không phải per claim** | Gộp 2.600.000 rồi cap ở 1.500.000 | `200.1` giữ 1.700.000 → cap 1.500.000; `200.2` giữ 900.000 → dưới trần 1.000.000, không cắt | A5 §6.2: *"an engine that capped 2,600,000 at 1,500,000 would under-pay by 900,000 and look, from the outside, exactly like one that got it right"* |
| **W-2** | **Cap per **visit**, không phải per event** | Liệu trình 12 buổi vật lý trị liệu = 1 lần áp trần 500.000 | 12 visit = 12 lần áp trần; bộ đếm `OCCURRENCE` +12, không +1 | `BR-LEDGER-14`. Defect này từng nằm trên `200.1`, `200.2`, `200.3`, `300.1` — **mọi benefit tính theo lần trong sản phẩm này** |
| **W-3** | **Base của co-payment vs deduction** | Co-payment trên số billed; deduction trên số billed | **Co-payment: số còn lại sau bước 9** (sau sub-limit và count limit, sau mọi decline ở bước 4/6/7) — **không phải số billed**. **Deduction: số còn lại sau bước 10** — phần của công ty bảo hiểm, không phải số billed | A5 §6.2. Câu bất biến nếu số bước đổi: *"co-payment follows the count limit, the deduction ratio follows co-payment"* |
| **W-4** | **Parent annual cap là bước 12, BỐN bước SAU co-payment** | Coi hạn mức năm là "một limit" nên áp trước co-payment | Hạn mức năm **không** nằm trong base của co-payment. D1 §5.6 sửa riêng điểm này ở v0.5 | D1 §5.6 |
| **W-5** | **Làm tròn ở TỪNG bước, không phải một lần cuối** | `Round(total, 0)` một lần | `Round(x, 0, AwayFromZero)` sau **mỗi** bước sinh tiền | A5 §6.2. Lý do: `BR-TRACK-4` chạy lại toàn chuỗi sau khi giám định viên xác nhận khuyến nghị; chuỗi làm tròn một lần cuối có thể trả tổng khác trên cùng input |
| **W-6** | **`0` trên `maxCount` nghĩa là KHÔNG GIỚI HẠN** | Coi 0 là "không được lần nào" → decline mọi dòng | `0` = unlimited (sentinel convention). UI render **"Không giới hạn"** | A5 §4.2, `BR-TREE-4` |
| **W-7** | **`null` trên `waitingPeriodDays` nghĩa là KHÔNG ÁP DỤNG** | Coi `n/a` là 0 ngày → tổ hợp không thể có lại pass | `n/a` = tổ hợp **không thể phát sinh**; claim sinh ra nó là **lỗi dữ liệu**, không phải thời gian chờ 0 ngày. **Không seed dòng đó chút nào** | `BR-WAIT-4` |
| **W-8** | **Snapshot KHÔNG BAO GIỜ đọc lại** | Đọc benefit schedule hiện hành lúc tính lại | Snapshot chụp tại ngày dịch vụ, **never re-read** (D2 §5.2.4). *Annual sum insured*, *remaining per occurrence*, *co-payment rate*, *deduction rate* đều là `snapshot`. Chỉ *remaining before/after* là `system` đọc từ ledger | D2 §5.9.2 |
| **W-9** | **Ledger final tại APPROVAL, không phải tại PAYMENT** | Post chính thức khi phát lệnh chi | Provisional tại calculation → **final tại approval**. *"An approved claim has committed the limit. A second claim adjudicating at the same moment must see that commitment"* | `BR-LEDGER-1`, D1 §5.9, D3 G-2 |
| **W-10** | **Không bao giờ xoá bản ghi tiêu thụ** | `DELETE` khi claim bị decline | **Reverse hoặc offset**, không xoá (`BR-LEDGER-3`). Không màn hình nào trong Part V xoá một bản ghi tiêu thụ | `V-544` |
| **W-11** | **Va version → CHẠY LẠI adjudication, không post lại số cũ** | Retry ghi lại con số lần chạy đầu | Số tiền là **hàm của bộ đếm mà lần ghi va vào**, nên phát lại nó là vi phạm hạn mức bằng một race condition | `BR-LEDGER-8`, `V-546` |

### 8.2 Ba cạm bẫy phụ, ít gặp hơn nhưng đắt ngang

| # | Cạm bẫy | Đúng là |
|---|---|---|
| **W-12** | **`100.1.1` ICU tiêu thụ BA thứ cùng lúc** | Một ngày ICU tiêu: trần ngày ICU **và** một ngày trong bộ đếm 60 ngày của `100.1` **và** hạn mức năm của `100`. **Đây là phép thử tốt nhất xem platform có hiện thực đúng cây hay không** |
| **W-13** | **`BR-TREE-2` — composite và sub-limit của chính nó không đếm hai lần** | Khi một composite và một sub-limit của nó cùng áp lên một dòng, tiêu thụ của composite đếm **một lần**. Roll-up hiện **một** posting cho mỗi tổ tiên |
| **W-14** | **Bước bị bỏ qua vẫn phải render** | Bước 11 vắng khỏi màn hình và bước 11 áp ở 0% **không phân biệt được**, và A5 §4.3 nói rõ điều kiện không thoả nghĩa là **không giảm trừ**, không phải giảm trừ ở tỷ lệ 0 (`V-540`) |

### 8.3 Open item còn treo — và giá trị tạm đề nghị

**Ba item đổi tiền, được BRD hackathon §3.2 chỉ định phải có quyết định tạm:**

| Item | Câu hỏi | Trạng thái | **Giá trị tạm đề nghị cho hackathon** | Vì sao |
|---|---|---|---|---|
| **B-19** | Tỷ lệ giảm trừ ở bước 11 test benefit của chính dòng, hay một tổ tiên mang tỷ lệ? Hai cách đọc cho **hai số tiền khác nhau** | **Mở** | **Cách đọc per-line: bước 11 KHÔNG FIRE.** Site Setting `DeductionScope = LINE_OWN_BENEFIT`. Hiển thị trên F48 là *"quy tắc tạm B-19"* | Đây là hành vi seed sinh ra hôm nay và là chiều an toàn — **trả thừa chứ không trả thiếu**. Cách đọc ancestor là cái A5 §6.2 tính trong ví dụ (1.920.000 → 1.536.000) nhưng nó áp tỷ lệ ở bước 11 lên một đại lượng mà bước 12 mới là bước tổng hợp |
| **B-20** | Bốn cost line dưới một benefit còn 1.000.000 hạn mức — **dòng nào được trả?** Bốn cách trả, mọi cách thoả mọi rule, mỗi cách một settlement | **Mở** | **Ngày điều trị tăng dần, rồi số thứ tự dòng tăng dần trong cùng một ngày**, và **ghi phân bổ lên claim** để chạy lại tái lập được. Site Setting `CapAllocationOrder = TREATMENT_DATE_ASC` | Theo trình tự thời gian — tổn thất phát sinh trước được trả trước. Giải thích cho khách bằng một câu. **Ổn định khi chạy lại**, điều mà quy tắc theo giá trị hoặc theo tỷ lệ không có sau khi một dòng bị decline. **Đây là đề xuất, không phải quyết định** |
| **B-21** | Người tham gia giữa kỳ có bị pro-rate hạn mức năm không, và bộ đếm chạy theo kỳ nào? **`L-1` key trên năm hợp đồng, `BR-WAIT-1` đo từ ngày thành viên bắt đầu — mâu thuẫn nằm trong cùng một tài liệu** | **Mở** | **`L-1` đứng như đã viết: bộ đếm theo năm hợp đồng, KHÔNG pro-rate.** Site Setting `CounterPeriodBasis = POLICY_YEAR` | Một mốc ranh giới cho mỗi hợp đồng, điều mà quản trị nhóm cần. **Cái giá phải nói ra:** thành viên tham gia ở tháng thứ 9 được **một hạn mức năm đầy đủ cho 9 tháng và một hạn mức đầy đủ nữa từ ngày kỷ niệm — hai hạn mức năm trong mười hai tháng bảo hiểm.** Trên pilot, quyền lợi ngoại trú thành **40.000.000 cover** trong khi sản phẩm bán **20.000.000** |

**Ba item khác của D1 còn treo và chạm tới sản phẩm:**

| Item | Câu hỏi | Ảnh hưởng tới build |
|---|---|---|
| **B-22** | Tổng tích luỹ chưa ai xem của một thành viên | 3 claim × 1.900.000 qua cổng 2.000.000 → **5.700.000 settle không người xem**. **Không có mitigation trong slice hackathon.** Ghi vào phần hạn chế của pitch |
| **B-23** | Cần ít nhất một benefit-level proviso thật | **Bước 7 chạy tập rỗng trên mọi claim.** Vẫn build bước, vẫn render nhóm rỗng kèm câu giải thích. **Không được bịa ra một proviso** để bước có việc làm |
| **B-24** | A4 chưa có document type "police or hospital statement" | Term class E duy nhất (#8) đứng `DRAFT`. Không ảnh hưởng số tiền |

**Hai item của A5 còn treo và đổi tiền:**

| Item | Câu hỏi | Giá trị tạm đề nghị |
|---|---|---|
| **A5-Q8** | Một sự kiện tai nạn **thực sự** phải trả bao nhiêu? Redirect về `500` **vứt bỏ trần phòng, bộ đếm 60 ngày, trần ICU, bộ đếm 15 ngày** trên nội trú, và **trần/lần cùng đồng chi trả** trên ngoại trú. *"On the Standard plan a 30-day accident admission with 10 days in ICU pays billed cost to 50,000,000 with no sub-limit and no count applied"* | **Phương án (a): xác nhận như đang có** — bảo hiểm tai nạn là bồi thường đơn giản vào một pot riêng. Đây là cách viết hợp đồng thật và phổ biến. **Phương án (b) là cách đọc hầu hết hợp đồng mang, và mô hình sản phẩm KHÔNG diễn đạt được** — *"benefit `100.1`, but rolling up to `500`"* không có chỗ để viết. **Đây là thay đổi mô hình, không phải thay số** |
| **A5-Q9** | Một insured event có được mang hai condition class không? Mổ lấy thai **vì** biến chứng thai kỳ là `BIR` (270 ngày) **và** `MAT` (90 ngày) cùng lúc | **Phương án (a): chấp nhận đơn giản hoá, MỘT class mỗi event, VÀ VIẾT XUỐNG QUY TẮC ƯU TIÊN** — đề nghị: **class có thời gian chờ DÀI hơn thắng** (`BIR` trước `MAT`; `PRE` trước `SPE` trước `GEN`), giám định viên tách episode thành hai insured event khi cần. **Phần không được sống sót qua quyết định là "không có quy tắc ưu tiên nào được viết xuống"** — một đơn giản hoá không ai viết ra thì không phân biệt được với một defect |

**Bốn câu hỏi nội dung sản phẩm, không chặn build:** `A5-Q2` (ma trận thời gian chờ + danh sách `SPE`), `A5-Q3` (mười điều khoản loại trừ), `A5-Q4` (mapping `serviceCategory`), `A5-Q5` (rule decline cấp dòng). Mỗi cái là **dữ liệu engine đọc**, không phải hành vi engine hard-code.

**Ba phụ thuộc IDP còn mở:** `Q-IDP-02` (ICD theo dòng — chặn term #5 và #9), `Q-IDP-05` (mã quốc gia cơ sở — chặn term #10), `Q-IDP-06` (số ngày điều trị theo dòng — chặn bộ đếm `DAY`, tức chặn cơ chế duy nhất mà `100.1` và `100.1.1` tồn tại để thực thi).

**Một item của D2:** `D2-181` — `stepOutcome` của A1 có bốn giá trị và **không giá trị nào nghĩa là "không đánh giá được"**, cái mà `V-754`/`BR-CHAIN-1` cần. Interim: render `KHÔNG_ĐẠT` (gần nhất trong bốn, và không phải cái đúng) **và ghi tên fact thiếu vào ô Diễn giải**.

**Một defect trong BRD hackathon phải sửa:** `CH-12 Rounding` ghi *"B-20 interim: round down"*. **B-20 là thứ tự tiêu thụ cap, không phải làm tròn**, và quy tắc làm tròn của A5 là **đồng nguyên, nửa làm tròn ra xa số 0, ở từng bước** — không phải làm tròn xuống 1.000 VND. Hai cái này cho số khác nhau. Sửa BRD, không sửa A5.

---

## 9. Bộ test đối chiếu

Mỗi bước là một Server Action riêng để **unit-test được và giải thích được** (BRD NFR-12). Bảng dưới đây là **trường hợp tối thiểu** cho từng bước: input → output kỳ vọng. Mọi số đều trên plan **HC-STD** trừ khi ghi khác. Đây là sàn, không phải trần — nhưng một bước không có test tương ứng ở đây thì chưa được coi là đã build.

### 9.1 Test theo từng bước

| Bước | Test ID | Input tối thiểu | Output kỳ vọng | Bắt lỗi gì |
|---|---|---|---|---|
| **1** | `T-CH01-a` | Policy hiệu lực 01/04/2026–31/03/2027, ngày dịch vụ 14/07/2026, `premiumPaid = true` | `Eligible`. `stepOutcome = ÁP_DỤNG` | Đường hạnh phúc |
| **1** | `T-CH01-b` | Cùng trên, ngày dịch vụ **01/04/2027** | `Not eligible`, decline `PERIOD` cấp event, **claim không auto-approve** | Decline tính toán vẫn phải chặn cổng |
| **1** | `T-CH01-c` | `premiumPaid = false`, ngày dịch vụ trong hạn | `Not eligible`. **Không có thời gian ân hạn nào** | Ai đó hiện thực hoá grace period |
| **2** | `T-CH02-a` | `conditionClass = GEN`, nhóm `200`, coverage start 01/04/2026, ngày điều trị 14/07/2026 (104 ngày) | Pass — 30 ngày đã phục vụ | |
| **2** | `T-CH02-b` | `conditionClass = GEN`, nhóm `300` (90 ngày), coverage start 01/04/2026, điều trị **20/06/2026** (80 ngày) | Decline `WAIT`. Thông điệp nêu **90 ngày · tính từ 01/04/2026 · điều trị 20/06/2026** (`BR-WAIT-5`) | Thời gian chờ nha khoa 90 ngày bị nhầm thành 30 |
| **2** | `T-CH02-c` | `conditionClass = PRE`, coverage start 01/04/2026, điều trị 14/07/2026 | Decline `WAIT` — 365 ngày | |
| **2** | `T-CH02-d` | Coverage start **của thành viên** 01/09/2026, policy inception 01/04/2026, `GEN`, nhóm `200`, điều trị 15/09/2026 | Decline `WAIT` — đếm từ **01/09**, không phải 01/04 (`BR-WAIT-1`) | Đếm từ ngày phát hành hợp đồng |
| **2** | `T-CH02-e` | `conditionClass = BIR`, nhóm `100` (ô `n/a`) | **Lỗi dữ liệu**, không phải pass 0 ngày (`BR-WAIT-4`) | `n/a` bị seed thành 0 |
| **3** | `T-CH03-a` | `conditionClass = PRE` | `KHÔNG_ÁP_DỤNG` (no-op trên sản phẩm này), **và bước vẫn hiện trên F48** | Bước bị ẩn khi không làm gì |
| **4** | `T-CH04-a` | Dòng `serviceCategory = SUPPLEMENT`, 400.000 | `TERM-HC-003` raise **`CHECKPOINT`**, **không** auto-decline. Claim ngoài cổng theo G3 | Class-D mang `CHECKPOINT` bị auto-decline |
| **4** | `T-CH04-b` | Dòng `serviceCategory = OPTICAL`, 800.000 | `TERM-HC-004` **`DECLINE_LINE`** `EXCL` kèm `clauseRef`. **Nhưng `clauseRef = TBC-04` là tạm → raise checkpoint thay vì decline** (`BR-TERM-10`) | Phát hành decline trích dẫn một `TBC-` |
| **4** | `T-CH04-c` | Claim nha khoa **trong sự kiện tai nạn** (`lossType = ACC_OP`), dòng `serviceCategory = DENTAL` | **KHÔNG decline.** `ACC_*` không nằm trong predicate `[ILL_IP, DEN_IP, MAT_IP]` của `TERM-HC-006` | Defect lịch sử: răng gãy do tai nạn bị từ chối bằng điều khoản loại trừ nha khoa |
| **4** | `T-CH04-d` | Claim bất kỳ, đếm term | Header hiện `n = 10`, **`m = 6`**, và `x + y + z + k = m` | Màn hình đếm 10 term đã xét |
| **4** | `T-CH04-e` | Một dòng bị **hai** term cùng decline | Thông điệp decline mang **cả hai** `clauseRef`, không chỉ cái đầu (`BR-TERM-2`) | Chỉ trích một điều khoản |
| **5** | `T-CH05-a` | `serviceCategory = CONSULT`, `episodeType = OUTPATIENT` | `benefitCode = 200.1` | |
| **5** | `T-CH05-b` | `serviceCategory = CONSULT`, `episodeType = INPATIENT` | `benefitCode = 100` (**không phải `100.1`**) | Map nhầm sang node con |
| **5** | `T-CH05-c` | `serviceCategory = DENTAL`, `episodeType = INPATIENT` | Sentinel **`"NONE"`**. **Bước 5 KHÔNG decline** — dòng rơi xuống bước 6 | Bảng mapping tự decline |
| **5** | `T-CH05-d` | `serviceCategory = OTHER` | Sentinel **`"ASSESSOR"`**. Code tra một benefit tên *ASSESSOR* phải **fail to** | Sentinel bị coi là benefit code |
| **5** | `T-CH05-e` | `serviceCategory = ROOM`, `lossType = ACC_IP` | `benefitCode = 500` (redirect tai nạn), **và mọi sub-limit/count/copay của `100.1` bị mất** | Redirect được hiện thực hoá nhưng hậu quả không được ghi |
| **6** | `T-CH06-a` | Dòng `serviceCategory = ADMIN`, 150.000 | Decline **`NOTCOV`** ở bước 6 (**không** phải `EXCL`, không phải bước 4) | `ADMIN` bị nhét vào term register |
| **6** | `T-CH06-b` | Dòng xét nghiệm ICD không liên quan chẩn đoán, khuyến nghị lâm sàng **đã xác nhận** | Decline `NOTREL`. Nếu khuyến nghị ở `Proposed` → **không** áp, và G3 fail | Áp khuyến nghị chưa ai xác nhận |
| **6** | `T-CH06-c` | Bốn dòng, một bị decline | *Số tiền thuộc phạm vi bảo hiểm* = tổng ba dòng còn lại, **trước** mọi cap/copay/deduction | Đại lượng này bị lẫn với payable |
| **7** | `T-CH07-a` | Claim bất kỳ | `KHÔNG_ÁP_DỤNG`, nhóm step-7 render **rỗng kèm câu giải thích**, không bị ẩn | Nhóm vắng mặt và nhóm rỗng bị lẫn |
| **8** | `T-CH08-a` | `200.1` giữ 1.700.000 (tư vấn 300.000 + xét nghiệm 1.400.000, cùng visit), `200.2` giữ 900.000 | `200.1` → 1.500.000 (**cắt 200.000**); `200.2` → 900.000 (**không cắt**). Tổng 2.400.000 | **Cap gộp cả claim ở 1.500.000 → thiếu 900.000** |
| **8** | `T-CH08-b` | Vật lý trị liệu `200.3`, **12 buổi cùng cơ sở, 12 ngày khác nhau**, mỗi buổi 600.000 | Trần 500.000 áp **12 lần** → 6.000.000, không phải 500.000 | Cả liệu trình bị coi là một visit |
| **8** | `T-CH08-c` | Hai dòng, **cùng ngày, cùng cơ sở**, cùng map `200.1`, 900.000 + 900.000 | **Một visit** → trần 1.500.000 áp một lần → cắt 300.000 | Hai dòng cùng ngày bị đếm thành hai visit |
| **9** | `T-CH09-a` | `200.3`, bộ đếm `OCCURRENCE` đã dùng 18/20, claim mang 5 visit | 2 visit trong hạn, 3 visit vượt → `LIMIT` | |
| **9** | `T-CH09-b` | `200.1`, `count_limit = 0`, claim mang 40 visit | **Pass toàn bộ.** UI render *"Không giới hạn"* | **`0` bị hiểu là không được lần nào** |
| **9** | `T-CH09-c` | `100.1`, `count_unit = DAY`, số ngày điều trị **không có** (`Q-IDP-06`) | **Dòng bị HOLD**, checkpoint nêu tên fact thiếu, claim ngoài cổng. **Không decline, không pass** (`BR-CHAIN-1`) | Bước lặng lẽ pass |
| **10** | `T-CH10-a` | Số còn lại sau bước 9 = 2.400.000, `200.1`/`200.2` mang `"INHERIT"`, `200` mang 20% | Khách chịu 480.000 → còn **1.920.000** | Copay tính trên số **billed** 3.000.000 → 2.400.000, sai 480.000 |
| **10** | `T-CH10-b` | Cùng trên nhưng plan **HC-PRM** | Tỷ lệ **10%**, không phải 20% → còn 2.160.000 | Copay hard-code 20% |
| **10** | `T-CH10-c` | Dòng map `300.1` trên HC-STD | Tỷ lệ **30%** thừa kế từ `300` | Thừa kế không lần lên tổ tiên |
| **11** | `T-CH11-a` | Cơ sở **trong** mạng lưới | Bước **bỏ qua hoàn toàn**. F48 vẫn render dòng bước 11 với `KHÔNG_ÁP_DỤNG` | Bước bị ẩn, hoặc áp ở tỷ lệ 0% |
| **11** | `T-CH11-b` | Cơ sở **ngoài** mạng lưới, dòng map `200.1`, số sau bước 10 = 1.920.000 | Theo quy tắc tạm `B-19` (`DeductionScope = LINE_OWN_BENEFIT`): **không fire**, còn 1.920.000. F48 ghi *"quy tắc tạm B-19"* | Hard-code 1.536.000 — đó là cách đọc thứ hai của một open item |
| **11** | `T-CH11-c` | Một claim, **hai** insured event: một trong mạng, một ngoài mạng | **Hai kết quả bước 11 khác nhau trên một claim** | Điều kiện đánh giá per claim thay vì per event |
| **11** | `T-CH11-d` | Cơ sở **không có** trong provider master | Coi là **off-network** (mặc định an toàn), và cảnh báo nổi lên work-list | Coi là in-network |
| **12** | `T-CH12-a` | `200` đã tiêu **chính thức** 19.000.000/20.000.000, claim tính ra 1.920.000 | Cắt còn 1.000.000, `LIMIT`, **và dòng giải thích nêu benefit + hạn mức còn lại + mức cắt** (`BR-TREE-5`) | Cắt lặng lẽ |
| **12** | `T-CH12-b` | Bốn dòng dưới `200`, cap còn 1.000.000 | Phân bổ theo **ngày điều trị tăng dần, rồi số thứ tự dòng** (`B-20` tạm), **và phân bổ được ghi lên claim** | Chạy lại cho kết quả khác |
| **12** | `T-CH12-c` | Một dòng ICU `100.1.1` | Tiêu thụ tính lên `100.1.1` → `100.1` → `100`; hạn mức năm lấy từ **`100`** (`100.1`, `100.1.1` là `included`) | Tìm `annual_cap` trên node con |
| **13** | `T-CH13-a` | Claim bất kỳ | `KHÔNG_ÁP_DỤNG`, vẫn render | |
| **14** | `T-CH14-a` | Chi trả 1.920.000 = 1.200.000 (`200.1`) + 720.000 (`200.2`) | Posting **tạm** lên `200.1`, `200.2` **và** `200`. Trạng thái `provisional` | Post thẳng thành final |
| **14** | `T-CH14-b` | Claim được **approve** | Posting chuyển **final tại approval** | Chuyển final tại payment |
| **14** | `T-CH14-c` | Claim được **decline** sau khi đã tính | Posting bị **đảo**, không bị **xoá** (`BR-LEDGER-3`) | `DELETE` bản ghi tiêu thụ |
| **14** | `T-CH14-d` | Một dòng ICU, một ngày | **Ba** loại bộ đếm cùng tăng: `AMOUNT` (tiền), `DAY` +1 trên `100.1.1` **và** `DAY` +1 trên `100.1`, `AMOUNT` lên `100` | Chỉ post bộ đếm tiền |
| **14** | `T-CH14-e` | Claim có dòng ngày 25/03/2027 và dòng ngày 05/04/2027, policy year bắt đầu 01/04 | Post vào **hai `periodKey`**, dòng theo dòng (`L-3`). **Không** có một con số "còn lại" duy nhất | Gộp hai năm thành một |
| **14** | `T-CH14-f` | Composite `100` và sub-limit `100.1` cùng áp lên một dòng | Tiêu thụ composite đếm **một lần** (`BR-TREE-2`) | Đếm hai lần |

### 9.2 Test toàn chuỗi (integration)

| Test ID | Kịch bản | Kỳ vọng |
|---|---|---|
| `T-CHAIN-01` | **Ví dụ A5 §6.2 nguyên văn** — ngoại trú HC-STD, 4 dòng, billed 3.000.000, có dòng thực phẩm chức năng 400.000 | Payable **1.920.000**; khách tự chịu 1.080.000 = `EXCL` 400.000 + `CAP_VISIT` 200.000 + `COPAY` 480.000; `200` còn 18.080.000. **14 dòng bước hiện đủ trên F48** |
| `T-CHAIN-02` | Cùng trên, cơ sở **ngoài mạng lưới** | Payable **vẫn 1.920.000** (quy tắc tạm `B-19`), bước 11 `KHÔNG_ÁP_DỤNG` kèm ghi chú *"quy tắc tạm B-19"* |
| `T-CHAIN-03` | **Xác định tính (`SC-H4`, NFR-03)** — chạy `T-CHAIN-01` hai lần trên cùng input | `InputsHash` giống nhau, `StepOutputsJson` giống nhau **từng bước**, tổng giống nhau |
| `T-CHAIN-04` | **Làm tròn từng bước** — dựng input sao cho tổng làm tròn một lần cuối ≠ tổng làm tròn từng bước | Engine trả **kết quả làm tròn từng bước**. Chênh lệch giữa hai cách phải khác 0, nếu không test này vô nghĩa |
| `T-CHAIN-05` | **`BR-TRACK-4`** — chạy chuỗi, xác nhận một khuyến nghị lâm sàng, chạy lại | Chuỗi chạy lại **toàn bộ**, không vá. Limit, tổ tiên, copay và deduction đều tính lại trên base đã giảm |
| `T-CHAIN-06` | **`V-546`** — hai claim cùng thành viên, cùng benefit, thẩm định đồng thời; claim 1 được duyệt trước | Claim 2 chạy lại **trên bộ đếm hiện tại** và giám định viên được báo số đã dịch chuyển. **Không post lại con số lần chạy đầu** (`BR-LEDGER-8`) |
| `T-CHAIN-07` | **Cây ba cấp** — 1 ngày ICU trên HC-STD, billed 4.000.000 | Trần ngày ICU 3.000.000 áp; `DAY` +1 trên `100.1.1` **và** trên `100.1`; `AMOUNT` roll-up tới `100`. **Đây là phép thử tốt nhất xem cây có đúng không** |

### 9.3 Test cổng tự động duyệt

| Test ID | Input | Kỳ vọng |
|---|---|---|
| `T-GATE-01` | Payable 1.450.000, không dòng nào bị cắt, không limit bind, Track 2 sạch, không checkpoint, không fraud signal | **Pass 4/4** → `APPROVED` → `PaymentOrder` → `PAID` |
| `T-GATE-02` | Payable 1.920.000 (dưới ngưỡng) **nhưng** một dòng bị decline và một limit bind | **G1 fail** → `PENDING_REVIEW`, và điều kiện fail được **nêu tên** |
| `T-GATE-03` | Payable 1.920.000, một term checkpoint mở ở bước 4 | **G3 fail** → `PENDING_REVIEW`. *Claim rẻ không đương nhiên là claim sạch* |
| `T-GATE-04` | Ba dòng confidence 0,75 chưa verify | **G2 fail** (sàn 0,80) → `PENDING_REVIEW` |
| `T-GATE-05` | Ngày sự kiện trong tương lai + bệnh viện không tra được | **G4 fail** → **`BLOCKED`** (không phải `PENDING_REVIEW`), và **không có `PaymentOrder` nào tồn tại** |
| `T-GATE-06` | Payable 2.000.000 chẵn, mọi điều kiện khác pass | **Pass** — ngưỡng là *"at or below"*, không phải *"below"* |
| `T-GATE-07` | Ba claim liên tiếp 1.900.000 mỗi claim, cùng thành viên | **Cả ba auto-approve** — 5.700.000 settle không người xem. **Đây là `B-22`, chưa có mitigation**; test tồn tại để hành vi được biết, không phải để pass |

### 9.4 Test seed

| Test ID | Kiểm | Kỳ vọng |
|---|---|---|
| `T-SEED-01` | Số dòng `benefit_schedule_line` | **51** (17 benefit × 3 plan) |
| `T-SEED-02` | Số dòng `benefit_waiting_period` | **51** (17 tổ hợp không-`n/a` × 3 plan). **Không dòng nào ứng với ô `n/a`** |
| `T-SEED-03` | `service_category_benefit_map` | **20 dòng**, và **không dòng nào chứa một decline**. Mọi ô là benefit code, `"NONE"` hoặc `"ASSESSOR"` |
| `T-SEED-04` | `adjudication_rule` | **14 dòng**, cột đúng `{step, name, enabled}` — **không có `benefit_code`** |
| `T-SEED-05` | `decline_reason` | **7 dòng**, đúng bảy mã của §2 |
| `T-SEED-06` | `policy_term` | **10 dòng**, trong đó **6 `ACTIVE`, 4 `DRAFT`**. Mọi dòng có `selector` **non-null**; `predicate` non-null **chỉ** trên class-D có effect decline |
| `T-SEED-07` | `provider` | **6 dòng**, **ít nhất một `in_network = false`** và **một `country ≠ VN`** — nếu không, bước 11 và term #10 không test được theo cả hai chiều |
| `T-SEED-08` | `policy.rule_set_ref` | **Non-null trên mọi policy.** Thiếu nó thì **bước 4 chọn 0 term trên mọi claim** — một bước báo *"0 điều khoản đã xét"* và cho mọi claim đi qua |
| `T-SEED-09` | `copay_pct` trong seed vs trên wire | Seed `20`, wire `2000`. Quy đổi **chỉ** ở script seed | |
| `T-SEED-10` | `line_decline_rule` | **Không tồn tại.** Loader tìm collection này là loader hỏng |
| `T-SEED-11` | Cột `Deductible` | **`null`, không phải `0`** — sản phẩm không có khấu trừ, và 0 nghĩa khác "không tồn tại" |

---

*Hết Build 04.*
