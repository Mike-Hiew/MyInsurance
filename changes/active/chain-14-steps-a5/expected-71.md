# Bộ test đối chiếu — trích NGUYÊN VĂN `build-04` §9

> Chép xuống đĩa 21/09. **Không suy giá trị kỳ vọng nào từ hiện thực.** Nguồn: Project doc `build-04-chuoi-tinh-tien-va-seed` §9.
>
> ⚠ **Số test thật là 71, không phải 65** — §9.1 = 46 · §9.2 = 7 · §9.3 = 7 · §9.4 = 11. Con số "65" trong `build-00` là ước lượng. Mốc phải là **71/71**.
>
> Mọi số trên plan **HC-STD** trừ khi ghi khác. Đây là **sàn, không phải trần** — nhưng một bước không có test tương ứng ở đây thì **chưa được coi là đã build**.

## 9.1 Test theo từng bước (46)

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
| **4** | `T-CH04-c` | Claim nha khoa **trong sự kiện tai nạn** (`lossType = ACC_OP`), dòng `serviceCategory = DENTAL` | **KHÔNG decline.** `ACC_*` không nằm trong predicate `[ILL_IP, DEN_IP, MAT_IP]` của `TERM-HC-006` | Răng gãy do tai nạn bị từ chối bằng điều khoản loại trừ nha khoa |
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

## 9.2 Test toàn chuỗi (7)

| Test ID | Kịch bản | Kỳ vọng |
|---|---|---|
| `T-CHAIN-01` | **Ví dụ A5 §6.2 nguyên văn** — ngoại trú HC-STD, 4 dòng, billed 3.000.000, có dòng thực phẩm chức năng 400.000 | Payable **1.920.000**; khách tự chịu 1.080.000 = `EXCL` 400.000 + `CAP_VISIT` 200.000 + `COPAY` 480.000; `200` còn 18.080.000. **14 dòng bước hiện đủ trên F48** |
| `T-CHAIN-02` | Cùng trên, cơ sở **ngoài mạng lưới** | Payable **vẫn 1.920.000** (quy tắc tạm `B-19`), bước 11 `KHÔNG_ÁP_DỤNG` kèm ghi chú *"quy tắc tạm B-19"* |
| `T-CHAIN-03` | **Xác định tính (`SC-H4`, NFR-03)** — chạy `T-CHAIN-01` hai lần trên cùng input | `InputsHash` giống nhau, `StepOutputsJson` giống nhau **từng bước**, tổng giống nhau |
| `T-CHAIN-04` | **Làm tròn từng bước** — dựng input sao cho tổng làm tròn một lần cuối ≠ tổng làm tròn từng bước | Engine trả **kết quả làm tròn từng bước**. Chênh lệch giữa hai cách phải khác 0, nếu không test này vô nghĩa |
| `T-CHAIN-05` | **`BR-TRACK-4`** — chạy chuỗi, xác nhận một khuyến nghị lâm sàng, chạy lại | Chuỗi chạy lại **toàn bộ**, không vá. Limit, tổ tiên, copay và deduction đều tính lại trên base đã giảm |
| `T-CHAIN-06` | **`V-546`** — hai claim cùng thành viên, cùng benefit, thẩm định đồng thời; claim 1 được duyệt trước | Claim 2 chạy lại **trên bộ đếm hiện tại** và giám định viên được báo số đã dịch chuyển. **Không post lại con số lần chạy đầu** (`BR-LEDGER-8`) |
| `T-CHAIN-07` | **Cây ba cấp** — 1 ngày ICU trên HC-STD, billed 4.000.000 | Trần ngày ICU 3.000.000 áp; `DAY` +1 trên `100.1.1` **và** trên `100.1`; `AMOUNT` roll-up tới `100`. **Đây là phép thử tốt nhất xem cây có đúng không** |

## 9.3 Test cổng tự động duyệt (7)

| Test ID | Input | Kỳ vọng |
|---|---|---|
| `T-GATE-01` | Payable 1.450.000, không dòng nào bị cắt, không limit bind, Track 2 sạch, không checkpoint, không fraud signal | **Pass 4/4** → `APPROVED` → `PaymentOrder` → `PAID` |
| `T-GATE-02` | Payable 1.920.000 (dưới ngưỡng) **nhưng** một dòng bị decline và một limit bind | **G1 fail** → `PENDING_REVIEW`, và điều kiện fail được **nêu tên** |
| `T-GATE-03` | Payable 1.920.000, một term checkpoint mở ở bước 4 | **G3 fail** → `PENDING_REVIEW`. *Claim rẻ không đương nhiên là claim sạch* |
| `T-GATE-04` | Ba dòng confidence 0,75 chưa verify | **G2 fail** (sàn 0,80) → `PENDING_REVIEW` |
| `T-GATE-05` | Ngày sự kiện trong tương lai + bệnh viện không tra được | **G4 fail** → **`BLOCKED`** (không phải `PENDING_REVIEW`), và **không có `PaymentOrder` nào tồn tại** |
| `T-GATE-06` | Payable 2.000.000 chẵn, mọi điều kiện khác pass | **Pass** — ngưỡng là *"at or below"*, không phải *"below"* |
| `T-GATE-07` | Ba claim liên tiếp 1.900.000 mỗi claim, cùng thành viên | **Cả ba auto-approve** — 5.700.000 settle không người xem. **Đây là `B-22`, chưa có mitigation**; test tồn tại để hành vi được biết, không phải để pass |

## 9.4 Test seed (11)

| Test ID | Kiểm | Kỳ vọng |
|---|---|---|
| `T-SEED-01` | Số dòng `benefit_schedule_line` | **51** (17 benefit × 3 plan) |
| `T-SEED-02` | Số dòng `benefit_waiting_period` | **51** (17 tổ hợp không-`n/a` × 3 plan). **Không dòng nào ứng với ô `n/a`** |
| `T-SEED-03` | `service_category_benefit_map` | **20 dòng**, và **không dòng nào chứa một decline**. Mọi ô là benefit code, `"NONE"` hoặc `"ASSESSOR"` |
| `T-SEED-04` | `adjudication_rule` | **14 dòng**, cột đúng `{step, name, enabled}` — **không có `benefit_code`** |
| `T-SEED-05` | `decline_reason` | **7 dòng**, đúng bảy mã |
| `T-SEED-06` | `policy_term` | **10 dòng**, trong đó **6 `ACTIVE`, 4 `DRAFT`**. Mọi dòng có `selector` **non-null**; `predicate` non-null **chỉ** trên class-D có effect decline |
| `T-SEED-07` | `provider` | **6 dòng**, **ít nhất một `in_network = false`** và **một `country ≠ VN`** — nếu không, bước 11 và term #10 không test được theo cả hai chiều |
| `T-SEED-08` | `policy.rule_set_ref` | **Non-null trên mọi policy.** Thiếu nó thì **bước 4 chọn 0 term trên mọi claim** |
| `T-SEED-09` | `copay_pct` trong seed vs trên wire | Seed `20`, wire `2000`. Quy đổi **chỉ** ở script seed |
| `T-SEED-10` | `line_decline_rule` | **Không tồn tại.** Loader tìm collection này là loader hỏng |
| `T-SEED-11` | Cột `Deductible` | **`null`, không phải `0`** — sản phẩm không có khấu trừ, và 0 nghĩa khác "không tồn tại" |
