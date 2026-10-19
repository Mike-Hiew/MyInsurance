# Build 06 — Màn hình tối thiểu & Câu chữ

| Mục | Giá trị |
|---|---|
| Tài liệu | Build 06 — Màn hình tối thiểu & Câu chữ cho bản dựng lại trên OutSystems ODC |
| Ngày | 16/09/2026 (bản 2 — đổi chuẩn nền tảng) |
| Đối tượng | Một developer BnK Solution, ~14 ngày build trước hackathon 07–08/10/2026 |
| **Chuẩn nền tảng** | **Claim MVP** — A1 v1.5, A2 v1.2, A3 v1.5, A4 v1.3, D2 v0.6 |
| Vai trò của BRD hackathon | Lớp agentic **đắp thêm** lên chuẩn đó (6 agent, chuỗi 14 bước, cổng 4 điều kiện). Ở đâu BRD và chuẩn nền tảng khác nhau, **chuẩn nền tảng thắng** |
| Tham chiếu thiết kế | `Prototype_MVP1_Console_v6.1.html` — 19 màn, **chỉ là spec hình ảnh**, không phải bài dự thi |

> **Đổi quyết định nền tảng (bản 2).** Bản 1 của tài liệu này coi MBAL-IDP production là chuẩn gốc (4 `claim_type` `MEDICAL_*`, 10 `document_type` `CL05…CLM_04`, 40 `templateId`) và coi A2/A3 là thứ "vay mượn". **Điều đó nay không còn đúng.** A1/A2/A3/A4/D2 **là** chuẩn chính của sản phẩm: A1 là nguồn của thuật ngữ, **A2 là nguồn của câu**, A3 là nguồn của quyền, A4 là nguồn của loại chứng từ và trường trích xuất, D2 là nguồn của màn hình. Toàn bộ nội dung §3–§7 dưới đây là **chuẩn chính, không phải tham chiếu**.
>
> Hệ quả về từ vựng: hồ sơ được phân loại bằng **`lossType` — 12 giá trị, 8 giá trị sống trong MVP 1** (A1 §4.2), không phải bằng `MEDICAL_ILLNESS`/`MEDICAL_INJURY`. Chứng từ là **7 loại `T1-01`…`T2-03`** (A4 §1), không phải `CL05…CLM_04`. Vocabulary `classification` mà IDP trả về là **của IDP**, không phải của ta; crosswalk sang `T1-nn` là một cột trong A4 §3 mà adapter đọc (D2-116) — **không màn hình nào hiển thị giá trị nào trong hai bộ đó**.

---

## 0. Ranh giới và chiến lược

### 0.1 Nguyên tắc cắt phạm vi

Một người, 14 ngày. Không dựng lại 19 màn của prototype v6.1. Chỉ dựng **màn hình nào một trong ba kịch bản demo (A, B, C) thực sự đi qua**, và mỗi màn chỉ dựng đúng phần kịch bản chạm tới.

Ba câu hỏi để quyết một màn có được dựng hay không:

1. Có bước nào trong kịch bản A, B hoặc C **dừng lại ở màn này** không? Nếu không → bỏ.
2. Nếu bỏ màn này, con số tiền hoặc chuỗi hand-off agent có còn chứng minh được không? Nếu còn → bỏ.
3. Giám khảo có hỏi "cho xem cái đó" không? Nếu có mà kịch bản không đi qua → **dùng ảnh prototype trong deck**, không dựng.

**Phần còn lại dùng prototype v6.1 làm ảnh minh hoạ trong deck.** Nói thẳng trong pitch: "đây là thiết kế đầy đủ của pilot; hôm nay chúng tôi chạy live sáu màn nằm trên đường đi của quyết định chi trả." Điều đó trung thực hơn là dựng vội 19 màn rỗng.

### 0.2 Bảng phải có / nên có / bỏ

Mã màn khách hàng dùng **đúng cách đánh số của D2 Part IV**: `S11` Tạo hồ sơ · `S12` Chụp chứng từ · `S13` Danh sách hồ sơ và tiến trình · `S14` Kết quả chi trả. *(BRD hackathon §9.5 đánh số khác — S09/S10/S11/S13. D2 thắng; xem §8 lỗ hổng 10.)*

| Nhóm | Màn (mã chuẩn) | Quyết định | Lý do |
|---|---|---|---|
| Console | **F42 Hàng đợi hồ sơ** | **PHẢI CÓ** | Điểm vào của mọi kịch bản; nơi thấy cờ trạng thái A/B/C khác nhau |
| Console | **F46 Trích xuất & dòng chi phí** | **PHẢI CÓ** | Kịch bản B sống ở đây: thẻ khuyến nghị + Xác nhận/Từ chối có lý do |
| Console | **F48 Tính tiền & sổ hạn mức** | **PHẢI CÓ** | 14 bước + Chạy lại + so sánh trước/sau = bằng chứng "tiền là code" |
| Console | **F49+F50 Phương án & Duyệt** | **PHẢI CÓ** | Bảng cổng 4 điều kiện, checklist 7 điểm, 4 mắt |
| Portal | **S14 Kết quả chi trả** | **PHẢI CÓ** | Chỗ duy nhất câu chữ A2 chạm tới khách hàng thật |
| Console | **Chế độ trình bày** | **PHẢI CÓ** | SC-H2: hand-off agent phải nhìn thấy được, không phải kể lại |
| Portal | **S13 Danh sách hồ sơ và tiến trình** | **NÊN CÓ — ưu tiên cao nhất trong nhóm này** | Rẻ, và là nơi quy tắc che 4 trạng thái nội bộ được chứng minh (§2) |
| Portal | **S12 Chụp chứng từ** | **NÊN CÓ** | Checklist 8+1 dòng theo `lossType`; cũng là màn phản hồi yêu cầu bổ sung |
| Portal | **S11 Tạo hồ sơ** | **NÊN CÓ** | Mở đầu demo 66 giây. Nếu hụt giờ: seed hồ sơ sẵn ở `SUBMITTED`, mở demo từ console |
| Console | F44 Danh mục chứng từ | **NÊN CÓ — nhúng, không tách màn** | Thu thành một dải trạng thái trên đầu F46 |
| Console | F47 Sự kiện & phạm vi (cờ gian lận) | **NÊN CÓ — nhúng, không tách màn** | Kịch bản C cần "Xoá cờ (có lý do)". Nhúng thành panel bên phải F46 |
| Console | F53 Lịch sử & nhật ký | **NÊN CÓ** | Trùng phần lớn với trace ở chế độ trình bày — dùng chung component |
| Console | F54 Dashboard, F60 Hợp đồng, F67 Hàng đợi xác minh, F55/F57 Quản trị | **BỎ** | Ảnh prototype trong deck |
| Console | F65 Sổ điều khoản | **BỎ** | Và xem §8 lỗ hổng 5: `TBC-nn` khiến nhánh từ chối theo loại trừ không dùng được |
| Portal | S08/S09 Bảo hiểm của tôi & quyền lợi, thông báo, chat hỏi về hồ sơ | **BỎ** | S14 đã chứng minh được "cùng một con số ở hai phía" |
| Console | Màn hàng đợi vận hành (`EXTRACTION_ERROR`) | **BỎ khỏi UI, giữ ở dữ liệu** | Trạng thái vẫn ghi vào hàng đợi; demo lỗi IDP nói bằng lời + `SYS-ERR-001` trên S13 |

**Tổng: 6 màn phải có, 7 mục nên có (2 trong đó là nhúng), phần còn lại là ảnh.**

### 0.3 Thứ tự dựng (nếu cắt tiếp thì cắt từ dưới lên)

`F42 → F46 → F48 → F49/F50 → S14 → Chế độ trình bày` → rồi mới `S13 → S12 → S11 → F53`.

Sàn không được phá: **một agent, một Server Action, một con số đúng, một dòng `AgentRun`.**

### 0.4 Từ vựng phân loại hồ sơ — `lossType`, 12 giá trị

Nguyên văn A1 §4.2: **"The loss type is the clinical benefit group crossed with where the treatment happened."** Nó **được nhập, không được suy ra** — cán bộ đặt nó trên insured event, app khách đặt nó trên form hồ sơ từ đúng 8 giá trị sống. **Không có gì tính ra nó.**

| # | Code | Tên tiếng Việt (hiển thị) | Nhóm quyền lợi | Nơi điều trị | MVP 1 |
|---|---|---|---|---|---|
| 1 | `ILL_OP` | Bệnh (ngoại trú) | `200` | Ngoại trú | ✔ |
| 2 | `ILL_IP` | Bệnh (nội trú) | `100` | Nội trú | ✔ |
| 3 | `DEN_OP` | Nha khoa (ngoại trú) | `300` | Ngoại trú | ✔ |
| 4 | `DEN_IP` | Nha khoa (nội trú) | `300` | Nội trú | ✔ |
| 5 | `ACC_OP` | Tai nạn (ngoại trú) | `500` | Ngoại trú | ✔ |
| 6 | `ACC_IP` | Tai nạn (nội trú) | `500` | Nội trú | ✔ |
| 7 | `MAT_OP` | Thai sản (ngoại trú) | `400` | Ngoại trú | ✔ |
| 8 | `MAT_IP` | Thai sản (nội trú) | `400` | Nội trú | ✔ |
| 9 | `DTH_ILL` | Tử vong do bệnh, thai sản | — | — | ✘ |
| 10 | `DTH_ACC` | Tử vong do tai nạn | — | — | ✘ |
| 11 | `PTD_ACC` | Thương tật vĩnh viễn do tai nạn | — | — | ✘ |
| 12 | `PTD_ILL` | Tổn thương sức khoẻ vĩnh viễn do bệnh | — | — | ✘ |

**Tám giá trị sống trong MVP 1; bốn giá trị được đăng ký và ngoài phạm vi.** Nguyên văn A1: *"They are registered rather than omitted because a set that silently cannot express a real claim type is worse than one that names it and refuses it. No screen offers them, and a claim carrying one is a data error, not a claim."*

**`lossType` trực giao với condition class.** Condition class (`PRE`, `SPE`, `GEN`, `BIR`, `MAT`, `ACC`) trả lời *loại bệnh lý nào* và điều khiển **thời gian chờ**. `lossType` trả lời *nhóm quyền lợi nào và điều trị ở đâu*, và điều khiển **danh mục chứng từ** cùng việc ánh xạ quyền lợi. Một bệnh có sẵn điều trị ngoại trú là `conditionClass = PRE` **và** `lossType = ILL_OP` — hai sự kiện được đặt riêng, không cái nào suy ra cái nào. Một ràng buộc duy nhất chạy giữa hai bên: `ACC_*` chỉ nhận `conditionClass = ACC`; `MAT_*` chỉ nhận `BIR` hoặc `MAT`.

**Dùng gì trong demo:** kịch bản A và B dùng `ILL_OP`; kịch bản C dùng `ACC_IP` hoặc `ILL_IP`. Ba giá trị là đủ, nhưng dropdown phải có đủ 8 — một dropdown 3 giá trị là một dropdown nói dối.

### 0.5 Từ vựng chứng từ — 7 loại `T1-01`…`T2-03`

| Mã | Tên tiếng Việt (A1 §6 — **đây là thứ hiển thị**) | Tầng |
|---|---|---|
| `T1-01` | Bảng kê chi phí khám chữa bệnh | Tier 1 — trích xuất đầy đủ |
| `T1-02` | Đơn thuốc | Tier 1 |
| `T1-03` | Phiếu khám bệnh | Tier 1 |
| `T1-04` | Hóa đơn GTGT | Tier 1 — **chỉ nhận XML**, không có đường OCR |
| `T2-01` | Giấy tờ tùy thân | Tier 2 — chỉ phân loại |
| `T2-02` | Kết quả cận lâm sàng | Tier 2 |
| `T2-03` | Báo cáo y tế / GCN phẫu thuật | Tier 2 |
| *(`T2-04`)* | *Biên bản / xác nhận hoàn cảnh sự cố* | *Tier 2, loại thứ 8, chỉ tới hồ sơ qua `BR-TERM-11`. Ngoài phạm vi hackathon* |

**Hai quy tắc hiển thị, tuyệt đối, áp cho mọi màn — console lẫn portal:**

1. **Luôn render tên tiếng Việt theo A1 §6, không bao giờ render mã.** *Bảng kê chi phí khám chữa bệnh*, *Đơn thuốc* — **không bao giờ** `T1-01`. Cơ sở: A2 rule M-3, và D2 §4.6 F1 nói thẳng: *"F1 renders the A4 type names in Vietnamese (A1 §6) … and never the codes `T1-01`."*
2. **Mức độ cần thiết render là *"Bắt buộc"* · *"Nên có"* · *"Tùy chọn"*, không bao giờ là `REQUIRED` / `EXPECTED` / `OPTIONAL`.** D2 §4.6 F2, nguyên văn: *"F2 renders as 'Bắt buộc' · 'Nên có' · 'Tùy chọn', **never as `REQUIRED`**."*

**Ba mức, không phải hai** (A4 §6): *Bắt buộc* chặn việc giải quyết hồ sơ. *Nên có* sinh một yêu cầu bổ sung gửi khách, cán bộ được miễn. *Tùy chọn* không bao giờ được hỏi và không bao giờ chặn.

---

## 1. Màn hình PHẢI CÓ

Quy ước chung cho cả 6 màn:
- Ký hiệu quyền dùng đúng A3 §0.1: **F** toàn quyền · **R** chỉ đọc · **C** có điều kiện · **—** *không nhìn thấy, không phải là bị vô hiệu hoá*.
- Mọi hành động đổi dữ liệu đều đi qua Server Action, không đi qua agent.
- Mọi màn console mặc định tiếng Anh, có nút chuyển VI. Tên khoản mục, tên cơ sở KCB, tên người **luôn giữ tiếng Việt** — đó là nội dung chứng từ thật.
- **Mọi câu tiếng Việt đến dưới dạng một chuỗi hoàn chỉnh đã bind placeholder.** Xem §7.4 — đây là quy tắc, không phải khuyến nghị.

---

### M1 — Hàng đợi hồ sơ (F42)

| Mục | Nội dung |
|---|---|
| **Mục đích** | Điểm vào duy nhất của console. Cho thấy bốn hồ sơ seed ở bốn trạng thái khác nhau để giám khảo thấy ngay hệ thống phân loại được việc. |
| **Vai được vào** | `ClaimOfficer` **F** (scope `QUEUE`) · `ClaimApprover` **F** (scope `QUEUE`) · `ClaimOps` **R** · `Customer` **—** |
| **Dữ liệu hiển thị** | `Claim.ClaimNo` (`CLM-YY-NNNNNN`) · tên người được bảo hiểm · **`lossType` render bằng tên tiếng Việt** (*Bệnh (ngoại trú)*) · trạng thái nội bộ + cờ: có khuyến nghị `Proposed`, có `FraudSignal` mở, có dòng dưới ngưỡng tin cậy · `amountClaimed` · `ChainRun.TotalPayable` mới nhất · số ngày tuổi · người đang giữ hồ sơ |
| **Hành động** | Lọc theo trạng thái / hàng đợi / người giữ; sắp xếp theo tuổi; **Nhận hồ sơ** (chỉ hồ sơ chưa gán trong queue của mình — A3 §4.2); mở chi tiết |
| **Chuyển màn** | Click dòng → **M2**. Hồ sơ ở `PENDING_APPROVAL` và người đăng nhập là `ClaimApprover` → mở thẳng **M4** |
| **Ghi chú build** | 8 hồ sơ seed, trong đó 4 hồ sơ A/B/C/D. Không dựng phân trang. **Console được phép hiển thị tên trạng thái nội bộ** — M-3 là quy tắc dành cho khách, A2 §0.4 nói rõ: với nhân viên thì định danh nội bộ *"are expected, not hidden"* |

---

### M2 — Chi tiết hồ sơ: lưới trích xuất + thẻ khuyến nghị (F46, có nhúng F44 và F47)

| Mục | Nội dung |
|---|---|
| **Mục đích** | Nơi con người nhìn thấy AI đã đọc gì và quyết định đồng ý hay không. Đây là màn của kịch bản B và C. |
| **Vai được vào** | `ClaimOfficer` **F** (scope `ASSIGNED`) · `ClaimApprover` **R** (A3 §2.1: approver **không** được sửa kết quả thẩm định, không đồng ý thì trả hồ sơ) · `ClaimOps` **R** · `Customer` **—** |
| **Bố cục** | Trái: ảnh gốc, có zoom và chuyển trang. Phải: lưới dòng chi phí. Dưới lưới: thẻ khuyến nghị. Phải ngoài cùng: panel "Việc cần làm" — **panel không được đè nội dung**, dùng `padding-right` (prototype v6.1 §2) |
| **Dải trạng thái chứng từ (nhúng F44)** | Với mỗi chứng từ: **tên tiếng Việt theo A1 §6** · mức **"Bắt buộc" / "Nên có" / "Tùy chọn"** · trạng thái (Thiếu / Đã nhận / Đã đọc / Lỗi) · độ tin cậy. **Không mã `T1-nn`, không chữ `REQUIRED`, ở bất kỳ đâu trên màn** |
| **Trạng thái gói chứng từ** | Một trong 6 giá trị A4 §6: `COMPLETE` · `MISSING_DOCUMENT` · `MISSING_FIELD` · `INCONSISTENT` · `AWAITING_ORIGINAL` · `ERROR`. Console hiển thị được (là nhân viên); **engine tiêu thụ trạng thái này chứ không tự suy lại** |
| **Lưới dòng chi phí** | Số dòng · ngày dịch vụ · tên khoản mục (nguyên văn tiếng Việt như in trên chứng từ) · nhóm dịch vụ · ICD-10 · số lượng · số tiền yêu cầu · phần BHYT đã trả · độ tin cậy · mã quyền lợi (hoặc `UNMAPPED`) · mã lý do loại trừ · số tiền chi trả |
| **Quy tắc sửa** | Sửa tại chỗ ghi vào giá trị đã sửa, **giữ nguyên và vẫn hiển thị giá trị gốc**. **Không ai — kể cả admin — được sửa kết quả trích xuất thô** (A3 §3.3: *"Nobody may edit the raw extraction result — not even an administrator."*) |
| **Thẻ khuyến nghị** (dòng có khuyến nghị `Proposed`) | Mã lý do (`NOTREL`/`NOTCOV`) · **câu tiếng Việt A2 §4 của mã đó, render nguyên chuỗi** · danh sách bằng chứng kèm nguồn · dải tin cậy · phiên bản knowledge base |
| **Hành động trên thẻ** | **Xác nhận** → `Confirmed`. **Từ chối** → bắt buộc nhập lý do, hiện `CLM-CONSOLE-014`. Cả hai ghi người quyết, lý do, thời điểm |
| **Panel cờ gian lận (nhúng F47)** | Loại cờ · mức độ · bằng chứng · một câu tiếng Việt giải thích · nút **Xoá cờ** (bắt buộc lý do) |
| **Dòng độ tin cậy thấp (kịch bản C)** | Highlight dòng dưới ngưỡng; nút **Đánh dấu đã xác minh** — bắt buộc trước khi trình duyệt |
| **Cảnh báo hiện tại màn này** | `CLM-CONSOLE-001` (dưới ngưỡng tin cậy, **có nêu tên trường cụ thể**) · `-002` (cảnh báo đối chiếu) · `-013` (có N dòng mang khuyến nghị) · `-015` (đánh giá lâm sàng không hoàn tất — **hồ sơ chưa được coi là sạch**) |
| **Chuyển màn** | Xử lý hết khuyến nghị → **M3** |

---

### M3 — Màn tính tiền (F48)

| Mục | Nội dung |
|---|---|
| **Mục đích** | Chứng minh **tiền là deterministic**. Màn quyết định thắng thua với giám khảo kỹ thuật: cùng đầu vào → cùng đầu ra, và mọi thay đổi số tiền đều truy được về một bước và một quyết định của con người. |
| **Vai được vào** | `ClaimOfficer` **F** (chạy lại) · `ClaimApprover` **R** · `ClaimOps` **R** · `Customer` **—** |
| **Bảng 14 bước** | Mỗi dòng: mã bước (`CH-01`…`CH-14`) · tên quy tắc · đầu vào đã dùng · đầu ra · giải thích VI + EN. Bước nào áp quyết định tạm thời phải in chữ **"quy tắc tạm thời"** ngay trên dòng đó |
| **Sổ hạn mức** | Theo từng mã quyền lợi: Trước / Tiêu thụ / Sau. Ghi rõ là **preview**, chỉ ghi sổ khi duyệt |
| **Chiều kỳ hạn mức** | Sổ hạn mức post dòng chi phí vào **kỳ chứa ngày điều trị của dòng đó**. Hồ sơ vắt qua ranh giới năm hợp đồng **chạm hai counter** → hiển thị một con số cho mỗi kỳ, **không bao giờ cộng lại** |
| **Hành động** | **Chạy lại chuỗi** · **So sánh hai lần chạy** |
| **So sánh trước/sau** | Chọn lần chạy cũ và mới. Hiển thị tổng trước → tổng sau và **từng dòng chênh lệch kèm nguyên nhân**. Kịch bản B phải ra đúng: `2.000.000 → 1.880.000`, dòng 4 `NOTCOV −80.000`, dòng 2 `CAP_VISIT −40.000` |
| **Chỉ báo số liệu cũ** | So hash đầu vào; không khớp → banner "Số tiền đang hiển thị chưa tính lại sau thay đổi gần nhất" và **khoá nút trình duyệt** |
| **Cảnh báo hiện tại màn này** | `CLM-CONSOLE-003` (vượt hạn mức — **bắt buộc có `{periodKey}`**) · `-004` (chưa đủ thời gian chờ, hiện cả phép tính) · `-007` (không có bảng quyền lợi hiệu lực tại ngày điều trị — *escalate, không được tiếp tục*) |
| **Chuyển màn** | Chuỗi chạy xong, không còn khuyến nghị `Proposed` → **M4** |

---

### M4 — Màn duyệt (F49 + F50/F51 gộp làm một)

| Mục | Nội dung |
|---|---|
| **Mục đích** | Một màn làm hai việc: cán bộ lắp phương án và trình; người duyệt xem cổng 4 điều kiện rồi duyệt hoặc trả. Gộp vì quy tắc 4 mắt nằm ở **người**, không nằm ở màn hình. |
| **Vai được vào** | `ClaimOfficer` **F** với phương án + Trình duyệt, **—** với nút Duyệt/Từ chối (ẩn hoàn toàn) · `ClaimApprover` **R** với phương án, **F** với Duyệt/Từ chối/Trả lại · `ClaimOps` **—** · `Customer` **—** |
| **Lưới phương án** | Theo từng quyền lợi: số tiền bảo hiểm · còn lại · trần/lần · % đồng chi trả · yêu cầu · trong phạm vi · chi trả · khách tự chịu. Dòng tổng ở cuối |
| **Bảng cổng 4 điều kiện** | G1 Số tiền ≤ ngưỡng tự duyệt · G2 Chứng từ & chất lượng trích xuất · G3 Lâm sàng sạch · G4 Không có cờ gian lận. Mỗi điều kiện: Đạt/Không đạt **kèm lý do cụ thể** |
| **Checklist 7 điểm trước khi trình** | Đủ chứng từ **Bắt buộc** đã đọc · mọi khuyến nghị đã xử lý · mọi dòng tin cậy thấp đã xác minh · đối chiếu chéo không có `Fail` · lần chạy chuỗi còn hiệu lực · cờ gian lận đã xoá hoặc ghi chú · đã có bản nháp thư. Trạng thái sống, đủ 7/7 mới mở nút |
| **Hành động của cán bộ** | **Trình duyệt** |
| **Hành động của người duyệt** | **Duyệt** · **Từ chối** (bắt buộc mã lý do từ danh sách đóng) · **Trả lại** (bắt buộc bình luận) |
| **Khối thư** | Xem trước thư; thư là **bản nháp cho tới khi có người bấm Gửi**. Bản đã gửi lưu lại nguyên văn |
| **Quy tắc 4 mắt** | Người đã thẩm định **không được duyệt** hồ sơ đó, bất kể giữ vai gì. Kiểm theo dấu vết xử lý của hồ sơ, **không theo session**. Chặn bằng `CLM-CONSOLE-005` |
| **Chuyển màn** | Duyệt → `APPROVED` → lệnh chi → `PAYMENT_IN_PROGRESS` → `PAID`. Trả lại → về **M2** |

---

### M5 — Kết quả chi trả cho khách (S14)

| Mục | Nội dung |
|---|---|
| **Mục đích** | Nguyên văn D2 §4.9: *"Show what was paid, benefit by benefit, what was not, why, and when the money moved."* Đây là chỗ duy nhất trong bản dựng mà câu chữ A2 chạm tới người thật. |
| **Vai được vào** | `Customer` **R**, scope `SELF`, **và chỉ hồ sơ do chính người đó nộp** — hồ sơ của thành viên khác cùng hợp đồng không nhìn thấy, kể cả với bên mua bảo hiểm (A3 §4.3) |
| **Vào từ đâu** | S13, và deep link trên `CLM-NOTIF-005` / `-006` / `-008` / `PAY-NOTIF-002` |
| **Trường hiển thị** | Xem §3.4 — 17 trường F1…F17 theo D2 §4.9, **không trường nào được nhập** |
| **Quy tắc cốt lõi** | Dòng bị từ chối **nằm trong cùng một bảng** với dòng được chi trả. Xem §3.4 |
| **Cấm tuyệt đối trên màn này** | Không tên trạng thái nội bộ, không mã (`NOTCOV`, `PENDING_INFO`) — M-3. Không ghi chú của cán bộ. Không bản nháp AI chưa chốt. **Không nhắc tới IDP, đánh giá lâm sàng, điểm tin cậy, knowledge base hay bằng chứng** — những thứ đó tồn tại trên hồ sơ nhưng **chỉ trên console** |
| **Đồng nhất số liệu** | Số trên màn này phải bằng đúng số trên M3/M4 — cùng đọc từ một lần chạy chuỗi, không tính lại ở tầng UI |

---

### M6 — Chế độ trình bày — xem chi tiết §2

| Mục | Nội dung |
|---|---|
| **Mục đích** | Bằng chứng cho SC-H2 (hand-off agent là thật) và SC-H4 (tiền deterministic). Dành cho giám khảo và đội dự án, **không dành cho người dùng cuối**. |
| **Vai được vào** | Cờ `Presenter` gắn trên `ClaimOfficer`. Bật bằng link ở màn đăng nhập hoặc `Shift + D`. Ngoài chế độ này, mọi cố gắng vào các màn đó **rơi về hàng đợi** (guard trong render, đúng như prototype v6.1 §5) |
| **Gồm** | Bản đồ 4 tầng · Trace `AgentRun` có `HandoffTo` · Bảng số đo · Bảng ROI |

---

## 2. Chế độ trình bày cho giám khảo

Bốn khối, một màn cuộn hoặc bốn tab. Ngôn ngữ **tiếng Anh** (pitch bằng tiếng Anh).

### 2.1 Bản đồ 4 tầng

| Tầng | Module | Hộp bên trong |
|---|---|---|
| Experience | `CLM_Console`, `CLM_Portal` | 6 màn đã dựng, ghi rõ vai nào vào được |
| Agentic | `CLM_Agents` | AG-0 Orchestrator · AG-1 Intake · AG-2 Adjudication · AG-3 Clinical · AG-4 Fraud-signal · AG-5 Customer-comms. Dưới mỗi agent: danh sách tool đóng của nó |
| Deterministic | `CLM_Core` | `RunAdjudicationChain` (14 bước) · `EvaluateAutoApprovalGate` · sổ hạn mức · máy trạng thái · `WriteAgentRun` |
| Integration | `CLM_IDP_Int`, core mock | REST tới BnK IDP · poller đánh giá · `ReplayMode` · seed core |

Mũi tên giữa tầng Agentic và tầng Deterministic **phải có nhãn**: **"calls as a tool · never writes"**. Đây là một câu, không phải chú thích — nó là câu trả lời cho câu hỏi khó nhất của giám khảo về guardrail.

Chú thích dưới bản đồ: *"No agent holds a tool that writes cost-line money fields, the limit ledger, a payment order or the claim state."*

### 2.2 Trace `AgentRun` có `HandoffTo`

Dòng thời gian theo hồ sơ. Mỗi dòng: thời điểm · agent · bước · tool đã gọi · tóm tắt vào · tóm tắt ra · **`HandoffTo`** · thời lượng · model · phiên bản knowledge base · cờ `Replay`.

Bảy hand-off phải nhìn thấy đủ:

| # | Từ → Tới | `HandoffTo` |
|---|---|---|
| 1 | AG-0 → AG-1 | `Intake` |
| 2 | AG-1 → AG-0 | `Orchestrator` |
| 3 | AG-0 → AG-3 và AG-0 → AG-4 (song song) | `Clinical`, `FraudSignal` |
| 4 | AG-3 → AG-0, AG-4 → AG-0 | `Orchestrator` |
| 5 | AG-0 → AG-2 | `Adjudication` |
| 6 | AG-2 → AG-0 | `Orchestrator` |
| 7 | AG-0 → cổng → AG-5 | `CustomerComms` |

**Yêu cầu diễn:** trace phải **cập nhật sống** khi cán bộ bấm Xác nhận trên M2 — đó là khoảnh khắc chứng minh vòng người-trong-vòng-lặp là thật chứ không phải ảnh chụp. Nếu chạy `ReplayMode`, cờ `Replay = true` phải hiện rõ; giấu nó là tự sát trước giám khảo kỹ thuật.

### 2.3 Bảng số đo (measured, không phải minh hoạ)

Mỗi ô ghi **con số + cách đo**. Ô nào chưa đo thì viết thẳng "not measured".

| Chỉ số | Giá trị | Cách đo |
|---|---|---|
| Thời gian trích xuất / trang | 16,1 s | `processingTime` do IDP trả, lưu trên chứng từ |
| Thời gian luồng đánh giá | ~30 s | Mốc bắt đầu → kết thúc của đánh giá lâm sàng |
| Số mục kiểm của luồng đánh giá | 20 | Kết quả luồng |
| Danh mục cơ sở KCB | 13.328 | OpenSearch table 322 |
| Độ chính xác trích xuất | **chưa đo** | `POST /request/compare`, 10–20 chứng từ đã ẩn danh |
| Tỷ lệ STP | từ kết quả cổng | Số hồ sơ cổng duyệt / tổng |
| Thời gian chu trình | từ timeline | `SUBMITTED → APPROVED/DECLINED` |
| Hồ sơ sạch: nộp → chi trả | ≤ 90 s live, ≤ 20 s replay | NFR-01 |

### 2.4 Bảng ROI

Một trang, mỗi dòng có cột **"how measured"**, mọi dòng chưa đo gắn nhãn *assumption*.

| Đầu vào | Giá trị nền | Nguồn |
|---|---|---|
| Số hồ sơ / năm | 990 | Reference insurer, 2026 |
| Số tiền chi trả / năm | ≈ 4,3 tỷ VND | Reference insurer |
| SLA xử lý (đơn giản / phức tạp) | 3 / 5 ngày làm việc | Reference insurer |
| Tỷ lệ nhập liệu tay | 100 % | Quan sát |
| Tỷ lệ STP hiện tại | 0 % | Quan sát |
| Phút cán bộ / hồ sơ trước & sau | *assumption* | Ước lượng đội dự án |
| Chi phí giờ công | *assumption* | Ước lượng đội dự án |

Đầu ra: giờ tiết kiệm/năm · mức giảm thời gian chu trình · rò rỉ tránh được nhờ NOTCOV/NOTREL nhất quán. Máy tính ROI để **sửa được tại chỗ**.

---

## 3. Màn hình khách hàng S11–S14

Bốn màn này là nửa còn lại của hệ thống và là nơi A2 có hiệu lực mạnh nhất. Dựng đủ để demo, không dựng đủ để pilot.

### 3.1 S11 — Tạo hồ sơ bồi thường

| Mục | Nội dung |
|---|---|
| **Mục đích** | Khai bốn sự kiện mà mọi thứ phía sau đọc: ai được điều trị, `lossType`, ngày, và tư cách người yêu cầu. |
| **Vai** | `Customer` **F**, scope `SELF` |
| **Trường then chốt** | Người được bảo hiểm (chọn từ hợp đồng) · **`lossType`, dropdown 8 giá trị sống, hiển thị tên tiếng Việt** · ngày xảy ra · ngày điều trị từ/đến · cơ sở KCB · số tiền yêu cầu · tài khoản nhận tiền · **tư cách người yêu cầu** |
| **Tư cách người yêu cầu** | Nếu chọn `BẢN_THÂN` thì họ tên và số giấy tờ **lấy từ hồ sơ người được bảo hiểm và không sửa được** (A3 §4.18). Nếu khác `BẢN_THÂN` thì checklist ở S12 **thêm một dòng Bắt buộc**: chứng từ chứng minh tư cách |
| **Validation** | `CLM-VAL-001` ngày tương lai · `CLM-VAL-002` ngoài thời hạn bảo hiểm · `CLM-VAL-004` số tiền ≤ 0 · `CLM-VAL-005` chưa xác thực chủ tài khoản |
| **Chuyển màn** | → **S12**. Hồ sơ ở `DRAFT`, **chưa có số hồ sơ** |
| **Cắt được gì** | Nếu hụt giờ: seed hồ sơ sẵn ở `SUBMITTED`. Nhưng dropdown `lossType` là thứ **không nên cắt** — nó là bằng chứng checklist chứng từ được sinh ra từ dữ liệu chứ không phải hard-code |

### 3.2 S12 — Chụp chứng từ — checklist 8+1 dòng

| Mục | Nội dung |
|---|---|
| **Mục đích** | Nguyên văn D2 §4.6: *"Capture the right documents for the episode type, one type at a time, with quality checks at the moment of capture."* |
| **Vai** | `Customer` **F**, scope `SELF`, trên hồ sơ do chính họ tạo |
| **Vào từ đâu** | S11 (chụp lần đầu); thẻ yêu cầu bổ sung trên S13 (chế độ phản hồi) |

**Bảng checklist — 8 dòng theo `lossType` + 1 dòng theo tư cách người yêu cầu.** Nguyên văn D2 §4.6: *"A4 §6 is the authority; the app renders the rows for the chosen loss type **and the row the claimant's standing requires**, and never invents one."*

| `lossType` (S11) | Bắt buộc | Nên có | Tùy chọn |
|---|---|---|---|
| `ILL_OP` — Bệnh, ngoại trú | T1-01, T1-03 | T1-04, **T1-02** | T2-01, T2-02, T2-03 |
| `ILL_IP` — Bệnh, nội trú | T1-01 | T1-04, T2-03 | T1-02, T2-01, T2-02 |
| `DEN_OP` — Nha khoa, ngoại trú | T1-01, T1-03 | T1-04 | T2-01 |
| `DEN_IP` — Nha khoa, nội trú | T1-01 | T1-04, T2-03 | T2-01, T2-02 |
| `ACC_OP` — Tai nạn, ngoại trú | T1-01, T1-03 | T1-04, T2-02 | T1-02, T2-01 |
| `ACC_IP` — Tai nạn, nội trú | T1-01 | T1-04, T2-02, T2-03 | T1-02, T2-01 |
| `MAT_OP` — Thai sản, ngoại trú | T1-01, T1-03 | T1-04 | T1-02, T2-01, T2-03 |
| `MAT_IP` — Thai sản, nội trú | T1-01 | T1-04, T2-03 | T1-02, T2-01, T2-02 |
| **Bất kỳ, khi tư cách người yêu cầu ≠ `BẢN_THÂN`** | **chứng từ chứng minh tư cách** | — | — |

> **Bảng này là bảng dựng, không phải bảng tham khảo.** Mã `T1-nn` ở trên chỉ tồn tại trong dữ liệu; **màn hình render tên tiếng Việt và chữ "Bắt buộc"/"Nên có"/"Tùy chọn"** (§0.5). Hai điều bảng này cho thấy mà bảng 5 giá trị cũ không cho thấy: nha khoa và thai sản **có dạng nội trú**, và checklist nội trú khác ngoại trú đúng theo một quy luật — `T1-03` phiếu khám rơi ra, `T2-03` báo cáo y tế đi vào, vì bệnh nhân nội trú ra viện với một báo cáo chứ không phải một phiếu khám. Và *"ngoại trú có thuốc"* **không phải là một `lossType`** — nó là một dòng trong bảng cũ đặt tên cho một tổ hợp mà khách hàng không thể biết tại thời điểm chụp; nó đã bị bỏ, `T1-02` là **Nên có** trên `ILL_OP` và phần đối chiếu quyết định sau.

**Kiểm tra chất lượng ảnh — chạy trên máy, tại lúc chụp, và KHÔNG chặn.**

| Kiểm | Message khi hỏng |
|---|---|
| Mờ | `DOC-VAL-001` |
| Mất cạnh | `DOC-VAL-002` |
| Loá | `DOC-VAL-003` |

Khách thấy: ảnh vừa chụp, câu A2 bên dưới, và **hai nút: *Chụp lại* và *Dùng ảnh này*.** *Dùng ảnh này* **luôn có sẵn**.

Lý do không chặn, nguyên văn D2 §4.6, và vế thứ hai mạnh hơn: *"a poor image has a recovery path and a blocked capture does not"* — ảnh xấu dẫn tới `MISSING_FIELD`, một yêu cầu bổ sung và `DOC-NOTIF-002` nêu đích danh trường và chứng từ, tức là một lời đề nghị cụ thể và làm được; còn một cú chụp bị app từ chối thì **không tạo ra gì cả**. Giá trị của bước kiểm nằm ở lời nhắc, không nằm ở quyền phủ quyết.

**Sai loại chứng từ chỉ là thông tin.** Khi phân loại của IDP khác với dòng khách đã xếp, `DOC-VAL-004` nói ra điều đã xảy ra và nói rằng khách không phải làm gì; phân loại thắng. *"Silently overriding what someone told you erodes trust; asking them to fix a disagreement the system has already resolved wastes their time."*

**Nhóm "Giấy tờ khác".** Một nhóm, **không phải một dòng checklist**, không có mức độ cần thiết, nằm dưới bảng. Lý do tồn tại: khách hàng **hiện không có chỗ nào để đặt một chứng từ không thuộc bảy loại** — bảy loại là danh mục trích xuất, không phải bản kiểm kê những gì bệnh viện đưa cho bệnh nhân. Nó được nộp và phân loại như mọi trang khác; nếu IDP nhận ra là một trong bảy loại thì `DOC-VAL-004` nói ra và trang được **chuyển vào đúng dòng**. **Nó không bao giờ thoả mãn một dòng Bắt buộc** — *"A group that could satisfy a requirement would be a way around the checklist."*

**Nút Gửi hồ sơ** mở khi mọi dòng **Bắt buộc** có ≥ 1 trang **và mọi tệp đã tải lên xong**. Không có chứng từ nào: `CLM-VAL-003`. Lỗi tải: `SYS-ERR-003`. Trạng thái → `SUBMITTED`; **số hồ sơ được cấp lúc đăng ký, không phải lúc bấm nút** — nên `CLM-NOTIF-001` cũng phát lúc đăng ký (xem §3.3).

**Ràng buộc `T1-04`:** hoá đơn GTGT **chỉ nhận XML**. Không có đường OCR; một tấm ảnh hoá đơn không được nhận, vì nó sẽ lặng lẽ tạo ra một chứng từ không ai đọc. Không có XML thì khách đi tiếp — hoá đơn là **Nên có** trên mọi loại, và quy tắc đối chiếu tương ứng bị bỏ qua kèm cảnh báo chứ không làm hồ sơ đứng lại.

**Ràng buộc nội trú:** hồ sơ nội trú chỉ có `T1-01` là **Bắt buộc**. Màn hình **không được hỏi khách một chứng từ mà không có gì đọc nó**.

### 3.3 S13 — Danh sách hồ sơ và tiến trình — 13 trạng thái nội bộ → 11 trạng thái hiện cho khách

| Mục | Nội dung |
|---|---|
| **Mục đích** | Liệt kê hồ sơ của khách, cho thấy tiến trình từng hồ sơ, và mang mọi yêu cầu bổ sung đang mở. |
| **Vai** | `Customer` **R**, **chỉ hồ sơ do chính họ nộp** — hồ sơ của thành viên khác cùng hợp đồng không nhìn thấy, kể cả với bên mua bảo hiểm (A3 §4.3) |
| **Dòng danh sách** | Số hồ sơ · người được bảo hiểm · ngày điều trị (từ – đến) · số tiền yêu cầu · **trạng thái hiện cho khách** · cập nhật gần nhất |
| **Mục timeline** | Thời điểm (`dd/mm/yyyy HH:mm`, Asia/Ho_Chi_Minh) · **nội dung = một template A2 hoàn chỉnh đã bind placeholder, không bao giờ ghép trên màn** · trạng thái mà mục đó chuyển hồ sơ vào |
| **Số hồ sơ** | `CLM-YY-NNNNNN`. **Vắng mặt trên hồ sơ `DRAFT`** — số được cấp một lần, lúc đăng ký. Dòng `DRAFT` hiển thị ngày điều trị và tên thành viên thay cho số |

**Bảng ánh xạ — 13 trạng thái nội bộ, 11 trạng thái hiện cho khách:**

| Trạng thái nội bộ | Hiện cho khách? | Trạng thái hiện cho khách | Câu trên timeline |
|---|---|---|---|
| `DRAFT` | Có | **Nháp** | *(không có — không thông báo nào phát trên hồ sơ nháp)* |
| `SUBMITTED` | Có | **Đã gửi** | `CLM-NOTIF-001`, **phát lúc đăng ký** |
| `DOC_PROCESSING` | Có | **Đang xử lý chứng từ** | `CLM-NOTIF-002` |
| `EXTRACTION_ERROR` | **KHÔNG — hiện như `DOC_PROCESSING`** | **Đang xử lý chứng từ** | `SYS-ERR-001` |
| *`DOC_PROCESSING` bị treo* | **KHÔNG — hiện y nguyên** | **Đang xử lý chứng từ** | `SYS-ERR-001` **một lần** tại ngưỡng treo, rồi im lặng |
| `PENDING_INFO` | Có | **Cần bổ sung chứng từ** | `CLM-NOTIF-003`, và `DOC-NOTIF-001`/`-002` nêu đích danh cái còn thiếu |
| `UNDER_ASSESSMENT` | Có | **Đang thẩm định** | `CLM-NOTIF-004` |
| `PENDING_APPROVAL` | **KHÔNG — hiện như `UNDER_ASSESSMENT`** | **Đang thẩm định** | *(không có mục mới)* |
| `APPROVED` | Có | **Đã duyệt** | `CLM-NOTIF-005`, hoặc `CLM-NOTIF-008` khi chi trả thấp hơn yêu cầu |
| `DECLINED` | Có | **Từ chối** | `CLM-NOTIF-006` |
| `PAYMENT_IN_PROGRESS` | Có | **Đang chi trả** | `PAY-NOTIF-001` |
| *`PAYMENT_IN_PROGRESS` bị treo* | **KHÔNG — hiện y nguyên** | **Đang chi trả** | *(không có mục mới; không nói gì cho tới khi tài chính trả lời)* |
| `PAID` | Có | **Đã chi trả** | `PAY-NOTIF-002` |
| `CLOSED` | Có | **Đã đóng** | *(không có mục mới — đóng hồ sơ là việc hành chính)* |
| `CANCELLED` | Có | **Đã hủy** | `CLM-NOTIF-009` — chỉ trên hồ sơ đã có số |

**Bốn chỗ cố tình che, và lý do từng chỗ.** Hai trạng thái bị gộp vào chip khác, hai trạng thái treo không thêm gì.

1. **`EXTRACTION_ERROR` → *Đang xử lý chứng từ*.** Đây là phát biểu mạnh nhất của toàn bộ vòng đời: trạng thái vận hành **không phải là quyết định**, và *"a claim declined because a service was down is indistinguishable from a correct decline unless the states are kept apart"*. Nhãn nội bộ của trạng thái này là *"Lỗi xử lý tài liệu"* — chính xác ở bên trong, và **không dùng được trên màn hình của khách**, vì một khách đọc chữ "lỗi" trên hồ sơ mình chỉ học được rằng có cái gì đó hỏng và không làm gì được với thông tin đó (M-3). `SYS-ERR-001` **được viết có chủ ý để trông như một cập nhật tiến độ, vì từ phía khách hàng nó đúng là như vậy**: lỗi này khôi phục được, vô hình với họ, và không đòi hỏi họ làm gì.
2. **`DOC_PROCESSING` bị treo không hiện gì mới.** Cám dỗ là thêm thời gian đã trôi — *"đang xử lý đã 2 giờ"* — điều đó **biến một trạng thái bình thường thành một lỗi ngụ ý**. Treo là việc của người vận hành và có hàng đợi của người vận hành. `SYS-ERR-001` phát **một lần** tại ngưỡng treo, rồi timeline im lặng. **Lặp lại nó sẽ biến sự im lặng thành báo động.**
3. **`PAYMENT_IN_PROGRESS` bị treo không hiện gì mới.** `PAY-NOTIF-001` đã nói là đang chi trả rồi. App **không được nói một giao dịch thất bại cho tới khi tài chính thực sự trả về `REJECTED` hoặc `RETURNED`**, lúc đó `PAY-NOTIF-003` mới phát — và chú ý là `PAY-NOTIF-003` **không đòi khách làm gì**, vì một giao dịch bị trả về thường là dữ liệu của ta chứ không phải của họ.
4. **`PENDING_APPROVAL` → *Đang thẩm định*.** Không phải che vì vận hành mà **che có chủ ý**: A2 **không có thông báo nào cho `PENDING_APPROVAL`**, và sự vắng mặt đó là đúng. Với khách hàng, thẩm định là **một pha**; đặt tên cho một bước duyệt nội bộ sẽ mời câu hỏi *"ai là người duyệt và sao họ chậm thế"* và **phơi bày cấu trúc phân tách nhiệm vụ** (A3 §4.1) mà chẳng được lợi gì. Mục timeline từ `CLM-NOTIF-004` đứng nguyên cho tới khi có kết quả.

**M-6 phát biểu lại thành ràng buộc màn hình:** không ô nào trong cột *Câu trên timeline* là một quyết định được diễn đạt như một sự cố, hay một sự cố được diễn đạt như một quyết định. Những câu duy nhất nêu kết quả là `CLM-NOTIF-005`, `-006`, `-008` và `PAY-NOTIF-002`/`-003` — tất cả đều theo sau một quyết định thật của một người hoặc của cổng tự duyệt. **`SYS-ERR-001` là message lỗi hệ thống duy nhất khách thấy trên một hồ sơ, và nó nói điều ngược lại với một quyết định.**

**Hai message bind `{claimNo}` mà hồ sơ không phải lúc nào cũng có số** — `CLM-NOTIF-001` và `CLM-NOTIF-009`. Hai trigger dịch chuyển, **câu chữ không đổi**:
- `CLM-NOTIF-001` **phát lúc đăng ký, không phải lúc `SUBMITTED`**. Chip chuyển sang *Đã gửi* ngay khi gói chứng từ được nhận; mục timeline đến một lát sau, khi đã có số để điền vào.
- **Hồ sơ nháp bị *xoá*, không phải *hủy*, và không phát gì.** `CLM-NOTIF-009` chỉ phát trên hồ sơ đã có số. *"A claim number is never burned on something that was never a claim."* Hai hành động là hai nút riêng trên S13 đúng vì lý do đó.

**Hành động trên S13:** Mở hồ sơ · **Bổ sung chứng từ** (khi có yêu cầu đang mở → mở S12 ở chế độ phản hồi) · **Xem kết quả** (khi trạng thái ∈ `APPROVED`/`DECLINED`/`PAID`/`CLOSED` → S14) · Tiếp tục hồ sơ nháp · **Xóa hồ sơ nháp** (không chuyển trạng thái, không cấp số, không thông báo) · **Hủy hồ sơ** (chỉ khi hồ sơ **đã đăng ký** và còn trước `UNDER_ASSESSMENT`).

**Timeline là bản ghi.** Mọi message có kênh `IN_APP` đều rơi vào đây dù push có bật hay không, và các mục là **append-only** — một mục đã bị thay thế **không bao giờ được sửa hoặc xoá**.

**Trạng thái gửi thư không hiện cho khách.** Một message bị bounce, retry rồi thành công là một sự thật về hạ tầng của ta; một message đang retry không phải thứ khách hàng làm gì được. **Một trạng thái gửi hiện cạnh một hồ sơ sẽ bị đọc như một kết quả của hồ sơ.**

### 3.4 S14 — Kết quả chi trả

| Mục | Nội dung |
|---|---|
| **Vai** | `Customer` **R**, chỉ hồ sơ do chính họ nộp |
| **Vào từ đâu** | S13, và deep link trên `CLM-NOTIF-005` / `-006` / `-008` / `PAY-NOTIF-002` |

**17 trường, không trường nào được nhập** — đây là view cuối của một hồ sơ đã quyết:

| # | Nhãn tiếng Việt | Nội dung |
|---|---|---|
| F1 | Kết quả | **Một** template A2: `CLM-NOTIF-005` (duyệt toàn bộ), `-008` (chi trả một phần) hoặc `-006` (không thuộc phạm vi). **Đúng một, chọn theo kết quả** |
| F2 | Số hồ sơ | `{claimNo}` |
| F3 | Số hợp đồng · Người được bảo hiểm | Từ snapshot của hồ sơ, **không bao giờ đọc lại** |
| F4 | Quyền lợi | Tên quyền lợi từ snapshot, **một dòng cho mỗi quyền lợi được chi trả** |
| F5 | Chi phí yêu cầu | Trên dòng đó |
| F6 | Được chấp nhận | Sau hạn mức và sau các dòng bị từ chối |
| F7 | Đồng chi trả | Khoản đồng chi trả trừ trên quyền lợi đó |
| F8 | Chi trả | Số thực trả |
| **F9** | **Khoản không được chi trả** | **Nằm TRONG CÙNG BẢNG với F4–F8** — một dòng cho mỗi dòng bị từ chối, ô lý do chứa **một template A2 §4 hoàn chỉnh** |
| F10 | Giảm trừ theo điều kiện hợp đồng | Tỷ lệ giảm trừ **và điều kiện đã kích hoạt nó**. Vắng mặt khi không điều kiện nào được thoả — bước bị bỏ qua, không phải áp ở mức 0 |
| F11 | Tổng số tiền chi trả | `{settlementAmount}`, **bằng số và bằng chữ** |
| F12 | Hạn mức còn lại sau hồ sơ này — năm hợp đồng {kỳ} | Theo nhóm quyền lợi, từ bản ghi sổ **cuối cùng**. **Nêu rõ thuộc năm hợp đồng nào, gắn nhãn bằng hai mốc ngày của năm đó; hồ sơ vắt qua hai kỳ thì hiện một con số cho mỗi kỳ và không bao giờ cộng lại** |
| F13 | Tài khoản nhận tiền | `{maskedAccount}`, chỉ 4 số cuối |
| F14 | Ngày chi trả | `{paidAt}` khi đã xác nhận, trước đó là ngày dự kiến |
| F15 | Mã giao dịch | `{bankReference}` |
| F16 | Thông báo chi trả (PDF) | `CLM-LETTER-001`, hoặc `CLM-LETTER-002` khi từ chối toàn bộ |
| F17 | Yêu cầu xem xét lại | Kênh liên hệ và thời hạn, **có mặt trên MỌI kết quả bị từ chối hoặc từ chối một phần** |

**Quy tắc quan trọng nhất của màn này — dòng bị từ chối nằm TRONG CÙNG BẢNG với dòng được trả.** Nguyên văn D2 §4.9: *"Declined rows sit in the same table as paid rows, never in a collapsed section and never behind a 'xem chi tiết'. Separating them makes the result look as though it is hiding something — which is precisely the reading a customer disputing a partial settlement is already primed for."*

Và nguyên văn A2 §7.1, cùng một quy tắc nhìn từ phía lá thư: *"The benefit-by-benefit table is the point of the letter. A total alone generates a phone call whenever it differs from what was claimed; the itemisation answers the question before it is asked."*

Nghĩa khi build: **không accordion, không "xem chi tiết", không tab riêng, không phụ lục.** Một bảng.

**Từ chối toàn bộ:** render F1, F9, F16, F17 và **bỏ F5–F8, F11–F15**. Không có khoản chi trả nào, và **hiện số 0 ở chỗ lẽ ra là tiền còn tệ hơn là không hiện gì**.

**Văn bản của dòng bị từ chối là dữ liệu của hồ sơ, không phải template app điền.** Câu `NOTREL`/`NOTCOV` có thể do AI soạn nháp rồi **cán bộ sửa trước khi gửi**; cả hai bản đều được lưu. Thứ đến màn này là bản **cuối cùng** nằm trên hồ sơ. App **render nguyên văn và không hậu xử lý gì** — không cắt, không "…", không xuống dòng lại làm đổi câu, không nối thêm tên quyền lợi. Console mới là nơi ràng buộc "bản đã sửa vẫn phải nêu item và amount" được enforce.

**Sáu ràng buộc của quy tắc câu chữ khi từ chối, và S14 là nơi chúng đáp xuống:**

| Yêu cầu | Màn hình thoả bằng cách nào |
|---|---|
| Một quyết định từ chối lâm sàng phải nói cái gì **được bảo hiểm**, không bao giờ nói rằng một đơn thuốc là sai | Hai mã lâm sàng duy nhất tới được khách là `NOTREL` và `NOTCOV`, và câu A2 §4 của cả hai đều là phát biểu về phạm vi bảo hiểm. **Màn hình không có nguồn văn bản từ chối nào khác; không có ô free-text nào để cán bộ đi vòng** |
| **Không bao giờ quy cho một mô hình** | Không thứ gì trên màn này nêu tên IDP, đánh giá lâm sàng, điểm tin cậy, một mục knowledge base hay một tham chiếu bằng chứng. Những thứ đó tồn tại trên hồ sơ nhưng **chỉ trên console**. **Khách được cho biết quyết định của công ty bảo hiểm, vì đó đúng là bản chất của nó — một con người đã xác nhận nó** |
| Nêu lý do và khoản mục mà lý do đó áp vào | M-5, thoả bởi chính các template, mỗi cái đều bind `{itemName}` và `{itemAmount}` |
| Một hạn mức bị chạm phải được giải thích | `LIMIT` nêu tên quyền lợi và `{remainingLimit}`, và F12 cho thấy vị trí sau hồ sơ này. **Chạm trần là được phép; chạm trần trong im lặng thì không** |
| Từ chối vì thời gian chờ phải hiện phép tính | `WAIT` bind `{waitingDays}`, `{coverageFrom}` và `{serviceDate}` — thời gian chờ, ngày bắt đầu đếm, và ngày điều trị |
| **Luôn có đường khiếu nại** | F17 có mặt trên mọi kết quả bị từ chối hoặc từ chối một phần. **Một quyết định từ chối không kèm đường khiếu nại là cách chắc chắn nhất để biến một quyết định thành một khiếu nại** |

**Hành động:** Tải thông báo · **Gửi thông tin y tế bổ sung** (chỉ khi ≥ 1 dòng bị từ chối `NOTREL` và hồ sơ chưa `CLOSED` → mở S12 ở chế độ phản hồi) · Xem tiến trình.

---

## 4. Quy tắc diễn đạt

Mười quy tắc M-1…M-10 của A2 §0.1, **trích nguyên văn**, mỗi quy tắc kèm một dòng giải nghĩa cho người build.

| # | Rule (nguyên văn A2) | Nghĩa khi build |
|---|---|---|
| **M-1** | **Vietnamese is the primary language.** The customer app is Vietnamese-first; English exists for the console and for testing | Portal tiếng Việt. Console mặc định EN có nút VI. Cột EN trong catalogue là cho đội, không phải cho thị trường |
| **M-2** | **Every message says what to do next**, or explicitly says nothing is required | Không câu nào chỉ mô tả trạng thái rồi dừng. Nếu khách không phải làm gì thì phải nói ra là không phải làm gì |
| **M-3** | **Never expose an internal state name, code or identifier to a customer.** `PENDING_INFO` and `NOTCOV` are for the console | 13 trạng thái nội bộ, 7 mã chứng từ `T1-nn`, 3 mức `REQUIRED/EXPECTED/OPTIONAL`, 12 mã `lossType` — **không cái nào được xuất hiện trên portal**. Ánh xạ ở §0.4, §0.5 và §3.3 |
| **M-4** | **A money amount is always formatted and always carries its unit** — `1.200.000 đ`, with `.` as the thousands separator per Vietnamese convention | Định dạng ở tầng render, không bao giờ ghép chuỗi số thô vào câu |
| **M-5** | **A decline always names the reason and the benefit or item it applies to.** Never a bare "từ chối". *One exception, stated so it is a decision: the push and summary notification (`CLM-NOTIF-006`) says only that a result exists — the reason lives one tap away in the app and in the letter, because §0.3 forbids putting a decline reason on a lock screen* | Mọi dòng từ chối phải bind được `{itemName}` và `{itemAmount}`. Push là ngoại lệ duy nhất, và là ngoại lệ có chủ ý |
| **M-6** | **A system failure is never phrased as a decision.** "Hệ thống đang xử lý, chúng tôi sẽ thông báo khi có kết quả" — never "hồ sơ không hợp lệ" | Ràng buộc trực tiếp với §3.3: lỗi IDP / timeout agent **không bao giờ** thành từ chối, và **không bao giờ đổi chip trạng thái của khách** |
| **M-7** | **No message blames the customer.** "Ảnh chưa đủ rõ để đọc" not "Bạn đã chụp ảnh sai" | Ba message chất lượng ảnh ở S12 đều là phát biểu về **tấm ảnh**, không phải về người chụp |
| **M-8** | **Placeholders are named, never positional** — `{benefitName}`, not `{0}` | Template engine dùng tên. Cấm format theo vị trí. Đây là gốc của quy tắc §7.4 |
| **M-9** | **One message, one channel decision.** Every message states its channel(s). A message with no channel is not implemented | `CLM-NOTIF-007` đã bị nghỉ hưu chính vì vi phạm quy tắc này |
| **M-10** | **Tone: plain, respectful, no exclamation marks.** Address the customer as *Quý khách* in correspondence, *bạn* nowhere | Không dấu chấm than. Không giọng vui vẻ. Đây là bảo hiểm |

Hai quy tắc phụ cần nhớ khi build:

- **Mã message là vĩnh viễn.** *"Codes are permanent. A retired message keeps its code; the code is never reused."* Đổi câu chữ của một lý do từ chối hoặc một mục của thư là **change request**, không phải sửa chữ.
- **Phân biệt `CONSOLE` và `STAFF`:** *"The test is one question — is there a person waiting for this response?"* Có người đang chờ → `CONSOLE` (đồng bộ, không hàng đợi, không retry). Sự kiện xảy ra dù có ai nhìn hay không → `STAFF` (kênh `STAFF_INBOX`, có hàng đợi, có trạng thái đã đọc).

---

## 5. Câu chữ dùng thẳng

Toàn bộ cột "Nội dung tiếng Việt" dưới đây là **nguyên văn A2 v1.2**. Copy thẳng vào bảng message, không diễn giải lại.

### 5.1 Thông báo trạng thái cho khách (`CLM-NOTIF`)

| Mã | Ngữ cảnh (trạng thái) | Nội dung tiếng Việt (nguyên văn) | Placeholder | Kênh |
|---|---|---|---|---|
| `CLM-NOTIF-001` | `SUBMITTED` *(phát lúc đăng ký)* | Đã nhận hồ sơ bồi thường {claimNo}. Chúng tôi sẽ thông báo khi có kết quả. | `{claimNo}` | `IN_APP`, `PUSH` |
| `CLM-NOTIF-001` | push text | Đã nhận hồ sơ {claimNo} | `{claimNo}` | `PUSH` |
| `CLM-NOTIF-002` | `DOC_PROCESSING` | Đang xử lý chứng từ của Quý khách. | — | `IN_APP` |
| `CLM-NOTIF-003` | `PENDING_INFO` | Hồ sơ {claimNo} cần bổ sung chứng từ. Xem chi tiết trong ứng dụng. | `{claimNo}` | `IN_APP`, `PUSH`, `EMAIL` |
| `CLM-NOTIF-003` | push text | Hồ sơ {claimNo} cần bổ sung chứng từ | `{claimNo}` | `PUSH` |
| `CLM-NOTIF-004` | `UNDER_ASSESSMENT` *(và `PENDING_APPROVAL`, che)* | Hồ sơ đang được thẩm định. | — | `IN_APP` |
| `CLM-NOTIF-005` | `APPROVED` | Hồ sơ {claimNo} đã được duyệt. Số tiền chi trả: {settlementAmount}. | `{claimNo}`, `{settlementAmount}` | `IN_APP`, `PUSH`, `EMAIL` |
| `CLM-NOTIF-006` | `DECLINED` | Hồ sơ {claimNo} không thuộc phạm vi chi trả. Xem lý do chi tiết trong ứng dụng. | `{claimNo}` | `IN_APP`, `PUSH`, `EMAIL` |
| `CLM-NOTIF-005/006/008` | push text | Đã có kết quả hồ sơ {claimNo} | `{claimNo}` | `PUSH` |
| `CLM-NOTIF-008` | Chi trả một phần | Hồ sơ {claimNo} được chi trả một phần: {settlementAmount} trên {amountClaimed} yêu cầu. Xem chi tiết từng khoản trong ứng dụng. | `{claimNo}`, `{settlementAmount}`, `{amountClaimed}` | `IN_APP`, `PUSH`, `EMAIL` |
| `CLM-NOTIF-009` | `CANCELLED` | Hồ sơ {claimNo} đã được hủy theo yêu cầu của Quý khách ngày {cancelledAt}. Quý khách có thể nộp hồ sơ mới bất kỳ lúc nào. | `{claimNo}`, `{cancelledAt}` | `IN_APP`, `PUSH` |
| ~~`CLM-NOTIF-007`~~ | `PAID` | *Đã nghỉ hưu 17/08/2026 — trùng `PAY-NOTIF-002` trên cùng một trigger, vi phạm M-9. Mã được giữ và không bao giờ dùng lại.* | — | — |

> **Quy tắc push, áp cho toàn bộ nhóm này:** push **không bao giờ nói kết quả là gì**. Mọi push kết quả đều là *"Đã có kết quả hồ sơ {claimNo}"*. Màn khoá điện thoại ai cầm máy cũng đọc được.
>
> **Quyết định cần chốt trước demo:** ranh giới `-005` và `-008`. Xem §8 lỗ hổng 2.

### 5.2 Yêu cầu bổ sung chứng từ (`DOC-NOTIF`, `DOC-VAL`)

| Mã | Ngữ cảnh | Nội dung tiếng Việt (nguyên văn) | Placeholder | Kênh |
|---|---|---|---|---|
| `DOC-NOTIF-001` | Thiếu loại chứng từ Bắt buộc | Hồ sơ {claimNo} còn thiếu: {missingDocuments}. Vui lòng tải lên để chúng tôi tiếp tục xử lý. | `{claimNo}`, `{missingDocuments}` | `IN_APP`, `PUSH`, `EMAIL` |
| `DOC-NOTIF-002` | Không đọc được một trường | Chúng tôi chưa đọc được {fieldLabel} trên {documentType}. Vui lòng chụp lại chứng từ này. | `{fieldLabel}`, `{documentType}` | `IN_APP`, `PUSH` |
| `DOC-NOTIF-003` | Đối chiếu không khớp | Thông tin trên {documentA} và {documentB} chưa khớp nhau. Chúng tôi sẽ kiểm tra và liên hệ nếu cần. | `{documentA}`, `{documentB}` | `IN_APP` |
| `DOC-NOTIF-004` | Cần bản gốc | Hồ sơ cần bản gốc của {documentType}. Vui lòng gửi về {address} trước ngày {deadline}. | `{documentType}`, `{address}`, `{deadline}` | `IN_APP`, `EMAIL` |
| `DOC-NOTIF-005` | Nhắc lại, chưa có phản hồi | Hồ sơ {claimNo} đang chờ bổ sung chứng từ từ ngày {requestedAt}. | `{claimNo}`, `{requestedAt}` | `IN_APP`, `PUSH` |
| `DOC-NOTIF-006` | Điều khoản yêu cầu một chứng từ cụ thể | Để tiếp tục xử lý hồ sơ {claimNo}, chúng tôi cần thêm: {requiredDocument}. Đây là chứng từ mà quy tắc bảo hiểm yêu cầu đối với trường hợp của Quý khách, nên hồ sơ sẽ tạm dừng cho đến khi nhận được. Vui lòng tải lên hoặc liên hệ {contactChannel} nếu Quý khách không có chứng từ này. | `{claimNo}`, `{requiredDocument}`, `{contactChannel}` | `IN_APP`, `PUSH`, `EMAIL` |
| `DOC-VAL-001` | Ảnh mờ | Ảnh chưa đủ rõ để đọc. Vui lòng chụp lại ở nơi đủ sáng, giữ máy cố định. | — | `IN_APP` |
| `DOC-VAL-002` | Ảnh bị cắt cạnh | Có vẻ chứng từ bị cắt mất một phần. Vui lòng chụp lại toàn bộ trang. | — | `IN_APP` |
| `DOC-VAL-003` | Ảnh bị loá | Ảnh bị lóa. Vui lòng tránh ánh sáng chiếu trực tiếp. | — | `IN_APP` |
| `DOC-VAL-004` | Loại chứng từ khác với khai báo | Chứng từ này có vẻ là {detectedType}, không phải {declaredType}. Chúng tôi sẽ xử lý theo nội dung thực tế. | `{detectedType}`, `{declaredType}` | `IN_APP` |

> **Ràng buộc render, áp cho `{missingDocuments}`, `{documentType}`, `{detectedType}`, `{declaredType}`, `{requiredDocument}`:** luôn render **tên tiếng Việt của loại chứng từ theo A1 §6** — *Bảng kê chi phí khám chữa bệnh*, *Đơn thuốc* — **không bao giờ render mã** `T1-01`. §0.5.

### 5.3 Validation khi nộp hồ sơ (`CLM-VAL`) — dùng ở S11/S12

| Mã | Ngữ cảnh | Nội dung tiếng Việt (nguyên văn) | Placeholder | Kênh |
|---|---|---|---|---|
| `CLM-VAL-001` | Ngày điều trị ở tương lai | Ngày điều trị không thể là ngày trong tương lai. | — | `IN_APP` |
| `CLM-VAL-002` | Ngoài thời hạn bảo hiểm | Ngày điều trị {serviceDate} nằm ngoài thời hạn bảo hiểm ({coverageFrom} – {coverageTo}). | `{serviceDate}`, `{coverageFrom}`, `{coverageTo}` | `IN_APP` |
| `CLM-VAL-003` | Chưa đính kèm chứng từ | Vui lòng tải lên ít nhất một chứng từ để tiếp tục. | — | `IN_APP` |
| `CLM-VAL-004` | Số tiền yêu cầu ≤ 0 | Số tiền yêu cầu phải lớn hơn 0. | — | `IN_APP` |
| `CLM-VAL-005` | Chưa xác thực chủ tài khoản | Chưa xác thực được tên chủ tài khoản. Vui lòng kiểm tra lại số tài khoản và ngân hàng. | — | `IN_APP` |
| `CLM-VAL-006` | Tệp quá lớn / sai định dạng | Tệp {fileName} vượt quá {maxSize} hoặc không đúng định dạng. Định dạng hỗ trợ: JPG, PNG, PDF. | `{fileName}`, `{maxSize}` | `IN_APP` |

### 5.4 Thư kết quả chi trả (`CLM-LETTER-001`) và thông báo thanh toán (`PAY-NOTIF`)

**Cấu trúc thư `CLM-LETTER-001` — Thông báo chi trả bồi thường** (nguyên văn A2 §7.1, theo thứ tự):

| Phần | Nội dung |
|---|---|
| Header | Insurer name, logo, correspondence date |
| Reference | Số hồ sơ `{claimNo}` · Số hợp đồng `{policyNo}` · Người được bảo hiểm `{memberName}` |
| Opening | Confirmation that the claim has been assessed and the outcome |
| **Benefit-by-benefit table** | Quyền lợi · Chi phí yêu cầu · Được chấp nhận · Đồng chi trả · Chi trả — **one row per benefit paid**, plus a row per declined item with its reason |
| Deductions | Co-payment, and the deduction ratio **with the condition that triggered it** where one applied — e.g. treatment at a provider outside the listed network. It is a contract term, so there is no assessor's reason to state |
| Total | `{settlementAmount}`, in figures and in words |
| Remaining limits | Hạn mức còn lại per benefit group after this claim |
| Payment | Account (masked), expected date |
| Closing | How to query the outcome, and the deadline for doing so |

| Mã | Ngữ cảnh | Nội dung tiếng Việt (nguyên văn) | Placeholder | Kênh |
|---|---|---|---|---|
| `PAY-NOTIF-001` | `PAYMENT_IN_PROGRESS` | Đang chi trả cho hồ sơ {claimNo}. | `{claimNo}` | `IN_APP` |
| `PAY-NOTIF-002` | `PAID` | Đã chi trả {settlementAmount} vào tài khoản {maskedAccount} ngày {paidAt}. Mã giao dịch: {bankReference}. | `{settlementAmount}`, `{maskedAccount}`, `{paidAt}`, `{bankReference}` | `IN_APP`, `PUSH`, `EMAIL` |
| `PAY-NOTIF-002` | push text | Đã có cập nhật hồ sơ {claimNo} | `{claimNo}` | `PUSH` |
| `PAY-NOTIF-003` | Giao dịch bị trả về | Giao dịch chi trả chưa thành công. Chúng tôi đang kiểm tra thông tin tài khoản và sẽ liên hệ với Quý khách. | — | `IN_APP`, `PUSH`, `EMAIL` |

`{maskedAccount}` chỉ hiển thị 4 số cuối.

### 5.5 Thư từ chối (`CLM-LETTER-002`) và 7 câu lý do §4

**Cấu trúc thư `CLM-LETTER-002` — Thông báo từ chối bồi thường** (nguyên văn A2 §7.2): cùng header và reference block như trên, rồi:

| Phần | Nội dung |
|---|---|
| Outcome | A single clear sentence that the claim is not payable |
| **Reason** | The §4 wording for the applicable code, with every placeholder filled |
| **Basis** | **The clause or clauses of the policy terms relied on — one or many, each with its article and clause number and its `textVi` quoted verbatim, at the version the claim was evaluated against.** Where more than one clause fired on a line, every one of them appears, in the order the engine returned them. **Never a summary, never a paraphrase, and never only the first** |
| What is not affected | An explicit statement that cover continues and other benefits are unaffected |
| Right of review | How to ask for the decision to be reconsidered, to whom, and by when |

**Bảy câu lý do (A2 §4) — đây là câu hiển thị trong app và trong thư, không phải nhãn:**

| Mã | Nội dung tiếng Việt (nguyên văn) | Placeholder |
|---|---|---|
| `LIMIT` | Hạn mức của quyền lợi {benefitName} trong năm hợp đồng **{periodKey}** không đủ cho khoản chi phí này. Hạn mức còn lại trong năm hợp đồng này: {remainingLimit}. | `{benefitName}`, `{periodKey}`, `{remainingLimit}` |
| `PERIOD` | Ngày điều trị {serviceDate} nằm ngoài thời hạn bảo hiểm ({coverageFrom} – {coverageTo}). | `{serviceDate}`, `{coverageFrom}`, `{coverageTo}` |
| `NOBEN` | Chương trình bảo hiểm {planName} không bao gồm quyền lợi {benefitName}. | `{planName}`, `{benefitName}` |
| `WAIT` | Quyền lợi {benefitName} có thời gian chờ {waitingDays} ngày, tính từ {coverageFrom}. Ngày điều trị {serviceDate} chưa đủ thời gian chờ. | `{benefitName}`, `{waitingDays}`, `{coverageFrom}`, `{serviceDate}` |
| `EXCL` | **{itemName}: {itemAmount}** — khoản này thuộc điểm loại trừ **{clauseRefList}** của quy tắc bảo hiểm. Nội dung của (các) điều khoản được trích dẫn nguyên văn trong thư thông báo. | `{itemName}`, `{itemAmount}`, `{clauseRefList}` |
| `NOTREL` | {itemName}: {itemAmount} — khoản này không thuộc phạm vi chi trả cho chẩn đoán {diagnosis} được điều trị trong lần khám này. Nếu Quý khách có thông tin y tế bổ sung, vui lòng gửi để chúng tôi xem xét lại. | `{itemName}`, `{itemAmount}`, `{diagnosis}` |
| `NOTCOV` | {itemName}: {itemAmount} — không thuộc danh mục chi phí y tế được chi trả. | `{itemName}`, `{itemAmount}` |

Ba điểm áp dụng bắt buộc:

- `{clauseRefList}` render **mọi** clauseRef đã fire, theo thứ tự engine trả về, phân cách bằng `·`. Placeholder cũ `{exclusionRef}` (số ít) **đã chết**, không dùng.
- `NOTREL` và `NOTCOV` là **hai mã do AI soạn draft** (theo từng dòng chi phí). Cán bộ được sửa trước khi gửi; **cả bản gốc và bản cuối đều được lưu**; nhưng bản đã sửa **vẫn phải nêu item và amount** — console enforce điều đó.
- `WAIT` cố tình **hiện cả phép tính** vì đây là loại từ chối bị tranh cãi nhiều nhất.

### 5.6 Lỗi hệ thống (`SYS-ERR`)

| Mã | Nguyên nhân nội bộ | Nội dung tiếng Việt (nguyên văn) | Placeholder | Kênh |
|---|---|---|---|---|
| `SYS-ERR-001` | Trích xuất thất bại (`EXTRACTION_ERROR`) | Chúng tôi đang xử lý chứng từ của Quý khách và sẽ thông báo ngay khi có kết quả. | — | `IN_APP` |
| `SYS-ERR-002` | Kho dữ liệu hợp đồng không phản hồi | Hệ thống đang bận. Vui lòng thử lại sau ít phút. | — | `IN_APP` |
| `SYS-ERR-003` | Tải tệp thất bại | Tải tệp không thành công. Vui lòng kiểm tra kết nối và thử lại. | — | `IN_APP` |
| `SYS-ERR-004` | Lỗi không lường trước | Đã có lỗi xảy ra. Chúng tôi đã ghi nhận và đang xử lý. Mã tham chiếu: {correlationId}. | `{correlationId}` | `IN_APP` |

> `SYS-ERR-001` **cố ý trông như một cập nhật tiến độ**, vì từ phía khách hàng nó đúng là như vậy. Đây là câu duy nhất dùng cho `EXTRACTION_ERROR`, cho `DOC_PROCESSING` bị treo, và cho timeout agent. Nó phát **một lần**, không lặp.
>
> `SYS-ERR-004` là **message duy nhất hướng khách hàng mang một định danh nội bộ**, và nó là correlation id chứ không phải state name, nên M-3 vẫn được giữ.

### 5.7 Cảnh báo trên console (`CLM-CONSOLE`) — chỉ tiếng Anh, không bao giờ cho khách xem

| Mã | Ngữ cảnh | Nội dung (nguyên văn, EN) | Màn |
|---|---|---|---|
| `CLM-CONSOLE-001` | Độ tin cậy trích xuất dưới ngưỡng | Confidence {confidence} below threshold {threshold}. Review: {fieldList} | M2 |
| `CLM-CONSOLE-002` | Cảnh báo đối chiếu | {ruleId}: {message} — {documentId}, field {fieldName} | M2 |
| `CLM-CONSOLE-003` | Sẽ vượt hạn mức | {benefitCode} {benefitName}, policy year {periodKey}: remaining {remaining}, claimed {claimed} | M3 |
| `CLM-CONSOLE-004` | Chưa đủ thời gian chờ | {benefitCode}: {conditionClass} requires {waitingDays} days from {coverageFrom}; service date {serviceDate} | M3 |
| `CLM-CONSOLE-005` | Chặn duyệt — chính người này đã thẩm định | You assessed this claim and cannot approve it. Reassign to another approver. | M4 |
| `CLM-CONSOLE-006` | Tài khoản chưa xác thực | Payout account name not verified. Payment cannot be instructed. | M4 |
| `CLM-CONSOLE-007` | Không có snapshot quyền lợi | No benefit schedule effective on {serviceDate} for plan {planCode}. Escalate — do not proceed. | M3 |
| `CLM-CONSOLE-013` | Có khuyến nghị lâm sàng | {lineCount} cost line(s) have a clinical recommendation. Review each and confirm or reject before submitting. | M2 |
| `CLM-CONSOLE-014` | Khi từ chối một khuyến nghị | State why you disagree. This is recorded against the recommendation and is how the knowledge base improves. | M2 |
| `CLM-CONSOLE-015` | Đánh giá lâm sàng không hoàn tất | The clinical assessment did not complete for this claim. It has **not** been assessed as clean — review the cost lines manually. | M2 |

> `CLM-CONSOLE-015` là câu quan trọng nhất của nhóm với kịch bản demo: khi Track 2 timeout, hồ sơ **không được coi là sạch**. Đây là câu chống lại cám dỗ "agent không trả lời thì cho qua".
>
> `CLM-CONSOLE-024` bị **RESERVED, không được dùng**. Quy tắc §11: *"where a number is cited anywhere in the set and defined nowhere, it is reserved, not reused."*

### 5.8 Thông báo cho nhân viên (`STAFF-NOTIF`, kênh `STAFF_INBOX`) — chỉ dựng nếu có chuông thông báo

| Mã | Ngữ cảnh | Nội dung tiếng Việt (nguyên văn) | Placeholder |
|---|---|---|---|
| `STAFF-NOTIF-001` | Hồ sơ được phân giao | Bạn được phân giao hồ sơ {claimNo}. Hạn xử lý: {dueAt}. | `{claimNo}`, `{dueAt}` |
| `STAFF-NOTIF-003` | Người duyệt trả hồ sơ về | Hồ sơ {claimNo} được trả về. Lý do: {returnReason}. | `{claimNo}`, `{returnReason}` |
| `STAFF-NOTIF-004` | Khách đã gửi chứng từ bổ sung | Khách hàng đã gửi chứng từ bổ sung cho hồ sơ {claimNo}. | `{claimNo}` |
| `STAFF-NOTIF-005` | Trích xuất chưa hoàn tất | Trích xuất tài liệu hồ sơ {claimNo} chưa hoàn tất. Hồ sơ đang chờ xử lý sự cố. | `{claimNo}` |
| `STAFF-NOTIF-006` | Trích xuất đã xong | Trích xuất tài liệu hồ sơ {claimNo} đã hoàn tất. | `{claimNo}` |
| `STAFF-NOTIF-008` | Có phương án chờ duyệt | Có phương án bồi thường chờ phê duyệt: hồ sơ {claimNo}, cán bộ trình {submittedBy}. | `{claimNo}`, `{submittedBy}` |

> Hai quy tắc, nguyên văn: **S-3** — *"A `STAFF_INBOX` notification carries no clinical detail — no diagnosis, no ICD-10 code, no item name, no amount"*. **S-5** — *"Addressed to a user, read by that user. No role reads another user's inbox, and there is no digest to a supervisor"*.
>
> Nhóm này **có thể bỏ** nếu hụt giờ — nhưng nếu dựng chuông thì phải dựng đúng S-3, không được nhét số tiền vào cho "sinh động".

**Tổng số message trích được dùng thẳng: 53** (8 `CLM-NOTIF` còn hiệu lực + 6 `DOC-NOTIF` + 4 `DOC-VAL` + 6 `CLM-VAL` + 3 `PAY-NOTIF` + 4 `SYS-ERR` + 7 câu lý do §4 + 10 `CLM-CONSOLE` + 6 `STAFF-NOTIF`, chưa kể 4 dòng push text và 2 cấu trúc thư).

---

## 6. Từ cấm và cách nói thay thế

A2 **không có một bảng "banned words" riêng**; các cấm đoán nằm rải trong M-rules và §7.2. Bảng dưới đây tổng hợp đúng nguyên văn, có ghi nguồn từng dòng.

| Cấm | Thay bằng | Nguồn |
|---|---|---|
| Bất kỳ tên trạng thái / mã nội bộ nào: `PENDING_INFO`, `NOTCOV`, `EXTRACTION_ERROR` | Câu mô tả bằng tiếng Việt thường | M-3 |
| Mã loại chứng từ `T1-01`…`T2-03` | Tên tiếng Việt theo A1 §6 | M-3, D2 §4.6 F1 |
| `REQUIRED` / `EXPECTED` / `OPTIONAL` | "Bắt buộc" / "Nên có" / "Tùy chọn" | D2 §4.6 F2 |
| Mã `lossType` (`ILL_OP`…) | Tên tiếng Việt: *Bệnh (ngoại trú)* | M-3, A1 §4.2 |
| "từ chối" trần trụi (bare "từ chối") | Luôn nêu **lý do** + **quyền lợi/khoản mục** áp dụng | M-5 |
| "hồ sơ không hợp lệ" khi thực chất là lỗi hệ thống | "Hệ thống đang xử lý, chúng tôi sẽ thông báo khi có kết quả" | M-6 |
| "Bạn đã chụp ảnh sai" (đổ lỗi khách) | "Ảnh chưa đủ rõ để đọc" | M-7 |
| Xưng hô **"bạn"** trong thư từ | **"Quý khách"** | M-10 |
| Dấu chấm than (!) / giọng vui vẻ | Giọng "plain, respectful" | M-10 |
| Placeholder vị trí `{0}` | Placeholder có tên `{benefitName}` | M-8 |
| Đưa số tiền / chẩn đoán / tên quyền lợi / lý do từ chối lên **push** | Push chỉ nói "đã có kết quả", chi tiết nằm trong app | §0.3, ngoại lệ M-5 |
| Render mã điều khoản chưa ban hành (`TBC-nn`) trong thư từ chối | **Cấm tuyệt đối** — engine phải raise checkpoint thay vì decline | §7.2 |
| Số tiền trần `1200000` | `1.200.000 đ` (dấu `.` phân cách nghìn) | M-4 |
| Hiện thời gian đã trôi trên trạng thái đang xử lý (*"đang xử lý đã 2 giờ"*) | Không nói gì thêm | D2 §4.7 |
| Hiện trạng thái gửi thư (bounce / retry) cho khách | Không hiện | D2 §4.7 |
| **"kê sai"**, **"gian lận"** trong thư gửi khách | "không thuộc phạm vi…" theo câu §4 của mã tương ứng | **BRD hackathon GR-06** — xem ghi chú §6.3 |

### 6.1 Ba điều thư từ chối tuyệt đối không được làm (nguyên văn A2 §7.2)

> **Three things this letter must never do:** cite a reason not in the catalogue, omit the review route, and **render an unissued clause reference**. A decline without a stated route to challenge it is the single most reliable way to turn a decision into a complaint.

Và lý do của điều cấm thứ ba, nguyên văn:

> **A letter that quotes `TBC-04` at a customer is a document with a placeholder in it, on the insurer's letterhead, about a refusal to pay** — and it cannot be reversed, because it has already been read.

Suy ra ba ràng buộc build, không thương lượng:

1. **Luôn có đường khiếu nại.** Mục *Right of review* — và trường F17 của S14 — không được để trống trong bất kỳ hoàn cảnh nào: nói rõ khiếu nại với ai, bằng cách nào, trước ngày nào. Thư nào thiếu mục này thì không được phép gửi.
2. **Không lý do nào ngoài danh mục.** Hành động từ chối chỉ nhận mã lý do từ danh sách đóng; AG-5 chỉ được render câu §4 của mã đó. Agent **không được viết một lý do mới**. Trên S14 **không có ô free-text nào để đi vòng**.
3. **Không `TBC-nn`.** Xem §8 lỗ hổng 5.

### 6.2 Không quy quyết định cho AI

`CLM-CONSOLE-015` nói rõ trách nhiệm nằm ở người: khi đánh giá lâm sàng không chạy, hồ sơ **"has not been assessed as clean"**. Hệ quả cho câu chữ gửi khách: **không câu nào được nói hoặc ám chỉ rằng AI đã quyết định**. Câu §4 nói *"khoản này không thuộc phạm vi chi trả…"* — chủ ngữ là quy tắc bảo hiểm, không phải mô hình.

Nguyên văn D2 §4.9 về S14: *"Nothing on this screen names IDP, the clinical assessment, a confidence score, a knowledge-base entry or an evidence reference. Those exist on the claim and are console-only. **The customer is told the insurer's decision, because that is what it is** — a human confirmed it."*

Tương ứng ở tầng kiến trúc: khuyến nghị lâm sàng luôn ở `Proposed` cho tới khi một người `Confirmed` với danh tính và thời điểm ghi lại; chỉ khi đó bước `CH-05` mới áp nó vào tiền. AI đề xuất và giải thích; con người quyết định; chuỗi tính tiền.

### 6.3 Ghi chú nguồn cho dòng "kê sai"

Cụm cấm **"kê sai"** và **"gian lận"** **không có trong A2 v1.2** — kiểm bằng grep toàn văn, không khớp. Nó đến từ BRD hackathon (GR-06, FR-CORE-29, FR-AGT-30). A2 cấm ở mức nguyên tắc qua M-7 (*"No message blames the customer"*) và qua việc §4 đóng kín danh sách lý do. Khi dựng bảng message, ghi đúng hai nguồn để sau này không ai tưởng đó là câu của A2.

---

## 7. Phân quyền

### 7.1 Vai thực sự dựng trong hackathon

A3 v1.5 có 4 role + 3 grant. Bản dựng hackathon **rút gọn còn 4 role, bỏ toàn bộ grant của tầng điều khoản** (vì F65–F68 bị cắt ở §0.2).

| Vai A3 | Vai ODC trong bản dựng | Có dựng? | Ghi chú |
|---|---|---|---|
| `CUSTOMER` | `Customer` | **Có** | Scope `SELF`, và chỉ hồ sơ do chính người đó nộp |
| `ASSESSOR` | `ClaimOfficer` | **Có** | Vai chính của demo |
| `APPROVER` | `ClaimApprover` | **Có** | Bắt buộc, vì 4 mắt là điểm chấm |
| `ADMIN` | `ClaimOps` | **Có, rút gọn** | Chỉ hàng đợi sự cố + site settings. **Không** đọc được chi tiết lâm sàng (A3 §4.11) |
| — | `Presenter` | **Có, là cờ** | Cờ gắn trên `ClaimOfficer`, không phải role riêng |
| `TERM_AUTHOR` / `TERM_APPROVE` | — | **Không** | Tầng điều khoản ngoài phạm vi |
| grant `clinical` | — | **Không** | Knowledge base seed cứng, không có màn quản trị |

### 7.2 Ma trận vai × màn hình × quyền

Ký hiệu A3 §0.1: **F** toàn quyền · **R** chỉ đọc · **C** có điều kiện · **—** không nhìn thấy.

| Màn / chức năng | `Customer` | `ClaimOfficer` | `ClaimApprover` | `ClaimOps` | `Presenter` | Data scope |
|---|---|---|---|---|---|---|
| **M1** Hàng đợi hồ sơ (F42) | — | **F** | **F** | **R** | **R** | `QUEUE` |
| M1 · Nhận / trả hồ sơ về queue | — | **C** | **F** | — | — | `QUEUE` — chỉ hồ sơ chưa gán (§4.2) |
| M1 · Gán hồ sơ cho người khác | — | — | **F** | — | — | `QUEUE` — §4.2 |
| **M2** Chi tiết + lưới trích xuất (F46) | — | **F** | **R** | **R** | **R** | `ASSIGNED` |
| M2 · Sửa giá trị dòng chi phí | — | **F** | — | — | — | `ASSIGNED` |
| M2 · Sửa kết quả trích xuất thô | — | — | — | — | — | **`NONE` — không ai, kể cả admin** |
| M2 · Xác nhận / Từ chối khuyến nghị | — | **F** | **R** | — | — | `ASSIGNED` — từ chối bắt buộc lý do |
| M2 · Đánh dấu dòng đã xác minh | — | **F** | — | — | — | `ASSIGNED` |
| M2 · Xoá cờ gian lận (F47) | — | **C** | — | — | — | `ASSIGNED` — bắt buộc lý do |
| M2 · Yêu cầu bổ sung chứng từ (F44) | — | **F** | **R** | — | — | `ASSIGNED` |
| **M3** Tính tiền & sổ hạn mức (F48) | — | **F** | **R** | **R** | **R** | `ASSIGNED` |
| M3 · Chạy lại chuỗi | — | **F** | — | — | — | `ASSIGNED` |
| **M4** Phương án + Trình duyệt (F49) | — | **F** | — | — | **R** | `ASSIGNED` |
| M4 · Duyệt / Từ chối / Trả lại (F50) | — | **—** | **F** | — | — | `QUEUE` — **§4.1** |
| M4 · Lệnh chi trả (F51) | — | — | **C** | — | — | `ASSIGNED` — **§4.6** |
| M4 · Gửi thư | — | **C** | **C** | — | — | `ASSIGNED` — thư là nháp tới khi có người bấm Gửi |
| **S11** Tạo hồ sơ | **F** | — | — | — | — | `SELF` |
| **S12** Chụp chứng từ / phản hồi yêu cầu | **F** | — | — | — | — | `SELF` — chỉ hồ sơ mình tạo |
| **S13** Danh sách & tiến trình | **R** | — | — | — | — | `SELF` — chỉ hồ sơ mình nộp |
| S13 · Xóa hồ sơ nháp | **C** | — | — | — | — | `SELF` — chỉ `DRAFT` |
| S13 · Hủy hồ sơ | **C** | — | — | — | — | `SELF` — đã đăng ký, trước `UNDER_ASSESSMENT` |
| **M5 / S14** Kết quả chi trả | **R** | **R** | **R** | — | — | `SELF` / `ASSIGNED` |
| **M6** Bản đồ agent + trace | — | **C** | — | — | **R** | — — chỉ khi bật chế độ trình bày |
| Hàng đợi sự cố, Site settings, reset dữ liệu demo | — | — | — | **F** | — | `MASTER` |
| Chi tiết lâm sàng (chẩn đoán, ICD-10, tên khoản mục, ảnh chứng từ) | **R** hồ sơ của mình | **R** | **R** | **—** | **—** | §4.11 — redact với `ClaimOps` |

**Điều quan trọng nhất về ký hiệu `—`**, nguyên văn A3 §0.1: *"Not permitted. The function is not visible, not merely disabled."* Và: *"In MVP 1 an unavailable function is absent from the interface, and the API rejects it independently — the UI is a convenience, never the control."*

Khi dựng trên ODC: **không dùng thuộc tính Enabled để ẩn quyền.** Nút Duyệt trên M4 phải **không tồn tại trong DOM** với `ClaimOfficer`, và Server Action duyệt phải tự kiểm lại vai + scope, không tin gì client gửi lên (A3 §6: *"Every endpoint checks role and data scope on every call, from the token, ignoring anything the client asserts"*). Data scope áp **như một predicate ở tầng query**, không phải lọc sau khi fetch — bản ghi ngoài scope **không bao giờ được nạp**, nên nó không thể rò qua một thông báo lỗi, một con đếm hay một dòng log.

### 7.3 Ba dòng giữ toàn bộ kiểm soát

Nguyên văn A3 §2.1:

> **The three lines that carry the control:** the assessor computes and submits but cannot approve (F49 vs F50); the approver reviews but cannot alter the assessment (**R** on F46–F48); and neither can instruct a payment on a claim that is not approved (§4.6).

### 7.4 Quy tắc 4 mắt

Nguyên văn A3 §4.1:

> **Segregation of duties.** The user who performed the assessment (F47/F48/F49) on a claim may not approve it (F50), regardless of the roles they hold | Checked against the claim's processing trail, not the session. Blocked with `CLM-CONSOLE-005`.

Ba điểm bắt buộc khi implement:

1. **Kiểm theo dấu vết xử lý của hồ sơ, không theo session.** Một người đăng xuất rồi đăng nhập lại vẫn bị chặn.
2. **Chặn bằng `CLM-CONSOLE-005`**, đúng câu: *"You assessed this claim and cannot approve it. Reassign to another approver."*
3. **Nút Duyệt vắng mặt, không phải bị mờ**, trên hồ sơ mà người đang đăng nhập đã thẩm định.

Hệ quả cho dữ liệu seed: cần **ít nhất 2 tài khoản người thật** — một `ClaimOfficer`, một `ClaimApprover` — và kịch bản B phải đăng xuất/đăng nhập giữa chừng, hoặc mở hai cửa sổ trình duyệt. **Đừng để một tài khoản làm cả hai**; quy tắc 4 mắt là điểm chấm, không phải chi tiết.

A3 §4.31a nói số người tối thiểu cho pilot là **4**: *"A pilot team of three cannot satisfy all three two-person rules, whatever the allocation."* Hackathon chỉ cần 2 — nhưng ghi rõ trong deck rằng pilot cần 4, vì đó là câu trả lời cho câu hỏi "cái này vận hành thật thì cần bao nhiêu người".

### 7.5 Masking cần dựng

| Trường | Với ai | Cách hiển thị | Nguồn |
|---|---|---|---|
| Số tài khoản nhận tiền | Tất cả | Chỉ 4 số cuối (`{maskedAccount}`) | A2 §5 |
| Chẩn đoán, ICD-10, condition class, tên khoản mục, ảnh chứng từ | `ClaimOps` | Redact, và việc truy cập được ghi log | A3 §4.11 |
| Hồ sơ của thành viên khác cùng hợp đồng | `Customer` | Không nhìn thấy, kể cả bên mua bảo hiểm | A3 §4.3 |
| Hạn mức còn lại (F25 / S14 F12) | `Customer` | **Chỉ số tiền**, không bao giờ tách theo ai đã tiêu | A3 §4.3 |
| Số CMND/CCCD, số thẻ BHYT | Mọi nơi trong demo | Lưu dạng masked; giá trị đầy đủ không tồn tại trên tenant | NFR-07 |
| Chẩn đoán / số tiền trong `STAFF_INBOX` | Nhân viên | Không bao giờ có | A2 §8.1 S-3 |
| Số tiền / chẩn đoán / lý do từ chối trong push | Khách | Không bao giờ có | A2 §0.3 |
| Trạng thái gửi thư (bounce, retry) | Khách | Không hiện | D2 §4.7 |

---

## 8. Quy ước hiển thị

### 8.1 Tiền

- **Định dạng chuẩn theo A2 M-4: `1.200.000 đ`** — dấu `.` phân cách nghìn, đơn vị `đ` viết thường, cách một khoảng trắng.
- **Xung đột cần chốt:** BRD hackathon NFR-10 viết `1.880.000 ₫` (ký hiệu `₫`), A2 M-4 viết `1.200.000 đ`. **A2 thắng cho mọi thứ khách hàng đọc** (S13, S14, thư, thông báo). Console được phép dùng `₫`.
- Định dạng ở **tầng render**, không bao giờ nối chuỗi số thô vào câu. Làm tròn **chỉ ở bước `CH-12`**, không làm tròn ở UI.
- Số tiền trong thư `CLM-LETTER-001` và ở S14 F11 phải có **cả bằng số và bằng chữ**.

### 8.2 Ngày

- `dd/MM/yyyy` ở mọi nơi hiển thị. Timeline dùng `dd/mm/yyyy HH:mm`, múi giờ **Asia/Ho_Chi_Minh**.
- Adapter chấp nhận cả `dd/MM/yyyy` và `yyyy-MM-dd` từ IDP, nhưng **chỉ một định dạng ra màn hình**.
- Khoảng ngày viết `({coverageFrom} – {coverageTo})` — gạch ngang dài, khoảng trắng hai bên, đúng như câu `PERIOD` và `CLM-VAL-002`.
- **Hạn mức luôn kèm kỳ.** S14 F12 và câu `LIMIT` đều bind `{periodKey}`; hồ sơ vắt qua hai năm hợp đồng hiện **hai con số, không bao giờ cộng lại**.

### 8.3 Trạng thái nội bộ so với trạng thái cho khách

**Bảng đầy đủ ở §3.3** — 13 trạng thái nội bộ → 11 trạng thái hiện cho khách, với 4 chỗ cố tình che.

Bản dựng agentic của BRD sinh thêm vài trạng thái con không có trong máy trạng thái chuẩn. **Chúng là cờ nội bộ, không phải trạng thái**, và phải quy về 13 trạng thái chuẩn trước khi chạm tới bất kỳ màn hình nào:

| Trạng thái con của BRD agentic | Quy về trạng thái chuẩn |
|---|---|
| `EXTRACTING` | `DOC_PROCESSING` |
| `PENDING_DOCS` | `PENDING_INFO` |
| `ASSESSING` | `UNDER_ASSESSMENT` |
| `ADJUDICATED` | `UNDER_ASSESSMENT` |
| `PENDING_REVIEW` | `UNDER_ASSESSMENT` |
| `BLOCKED` | `UNDER_ASSESSMENT` *(cờ, không phải trạng thái)* |
| `AGENT_TIMEOUT` | `EXTRACTION_ERROR` *(và do đó hiện là `DOC_PROCESSING`)* |

**`BLOCKED` trông giống hệt `UNDER_ASSESSMENT` với khách hàng.** Đúng như vậy, và đó là chủ ý: khách không được biết hệ thống đang nghi ngờ hồ sơ của họ. Kịch bản C demo trên console, không demo trên portal.

**Ánh xạ này phải nằm trong một Server Action**, không rải trong biểu thức của màn hình. Một chỗ duy nhất quyết định khách thấy gì.

### 8.4 Màn hình ĐẶT câu, không BAO GIỜ soạn câu

Đây là quy tắc trung tâm của toàn bộ tầng câu chữ, và nó có hai nguồn nói cùng một điều.

Nguyên văn D2 §2.6: **"A screen never concatenates a Vietnamese sentence from fragments. Every sentence is a complete A2 template with named parameters. This is A2 rule M-8 and it exists because Vietnamese classifiers and word order do not survive fragment assembly."**

Và nguyên văn D2 §4.9, phát biểu lại cho màn nhiều câu nhất trong app: **"Every sentence on this screen arrives as a complete string with named placeholders already bound; the screen positions sentences, it never composes them."** Cụ thể: app **không bao giờ** dựng `"{benefitName}"` + `" không được chi trả"`. `NOTCOV` là **một** template — *"{itemName}: {itemAmount} — không thuộc danh mục chi phí y tế được chi trả."* — và nó được render **nguyên một chuỗi hoặc không render gì cả**.

**Cái gì là câu và cái gì không:**

| Thành phần | Đến từ đâu | Là câu? |
|---|---|---|
| Dòng kết quả (S14 F1) | **Một** template A2 — `CLM-NOTIF-005`, `-006` hoặc `-008` — đã bind | **Có. Render nguyên chuỗi** |
| Tiêu đề cột (*Quyền lợi*, *Chi phí yêu cầu*, *Được chấp nhận*, *Đồng chi trả*, *Chi trả*) | **Nhãn**, thuộc sở hữu của D2 — A2 §0: *"screen labels, field names and button text — those are D2"* | Không. Chúng là danh từ, và **không câu nào được dựng từ chúng** |
| Tên quyền lợi | Snapshot của hợp đồng | Không |
| Tiền | Engine, format theo M-4 | Không |
| Lý do của mỗi dòng bị từ chối (S14 F9) | **Một** template A2 §4 của mã tương ứng, mọi placeholder đã bind | **Có. Render nguyên chuỗi** |
| Dòng giảm trừ (S14 F10) | Câu giảm trừ của A2 §7.1 với điều kiện kích hoạt đã bind | **Có. Render nguyên chuỗi** |
| Mục timeline (S13 F8) | **Một** template A2 hoàn chỉnh | **Có. Render nguyên chuỗi** |

**Sáu mẫu bị cấm trong code:**

| Mẫu sai | Vì sao | Làm đúng |
|---|---|---|
| `"Hồ sơ " + claimNo + " cần bổ sung"` | Câu không tồn tại trong catalogue, không ai review được, không đổi ngôn ngữ được | Lấy `DOC-NOTIF-001` rồi bind `{claimNo}` |
| `"Khoản này bị từ chối vì " + reasonCode` | Vi phạm M-3 (lộ mã) và M-5 (từ chối trần trụi) | Lấy đúng câu §4 của mã đó |
| Nối nhiều lý do bằng dấu phẩy ở code | `EXCL` yêu cầu `{clauseRefList}` phân cách bằng `·` theo thứ tự engine trả | Để engine dựng `{clauseRefList}`, bind một lần |
| Nội suy tên chứng từ từ mã hoặc từ `templateId` | Phải là tên tiếng Việt theo A1 §6 | Tra bảng tên → bind `{missingDocuments}` |
| Số tiền `ToString()` rồi nhét vào câu | Mất định dạng và mất đơn vị (M-4) | Format ở hàm render placeholder tiền |
| Câu tiếng Việt do agent tự sinh gửi thẳng cho khách | Agent chỉ được **chọn mã và điền placeholder** | AG-5 lấy template, không tự viết |

**Hệ quả kiến trúc: một hàm render duy nhất**, nhận `(messageCode, placeholderMap, language)` và trả về một chuỗi. Mọi màn, mọi agent, mọi thư đều đi qua nó. Đó cũng là điều kiện để §9 lỗ hổng nào chưa có câu thì **fail rõ ràng** (thiếu mã → lỗi), thay vì có người viết tạm một câu rồi nó đi thẳng ra thư gửi khách.

**Và một hệ quả nữa, dễ bỏ qua:** văn bản dòng bị từ chối là **dữ liệu của hồ sơ**, không phải template app điền. App render nó **nguyên văn và không hậu xử lý gì** — không cắt, không "…", không xuống dòng lại làm đổi câu, không nối thêm tên quyền lợi.

### 8.5 Ngôn ngữ

- Portal: **tiếng Việt**, không có nút chuyển.
- Console: **mặc định tiếng Anh**, có nút chuyển VI.
- Giữ nguyên tiếng Việt kể cả ở chế độ EN: **nội dung ảnh chứng từ, tên khoản mục trích xuất từ hoá đơn, tên cơ sở KCB, tên người**. Đây là giấy tờ thật — để tiếng Việt mới đúng, và còn chứng minh được hệ thống đọc được chứng từ tiếng Việt.
- Chế độ trình bày: **tiếng Anh**.
- Định danh, mã enum, tên trường trong payload và log: **tiếng Anh, UPPER_SNAKE** — `PENDING_INFO`, `NOTCOV`. **Không bao giờ dịch, không bao giờ hiện thô cho khách**; A1 §4 ánh xạ từng cái sang chữ hiển thị tiếng Việt.

---

## 9. Những chỗ câu chữ còn thiếu

Ghi ra để **không ai tự chế câu lấp chỗ trống**. Mỗi mục có hướng xử lý cho 14 ngày tới.

| # | Lỗ hổng | Ảnh hưởng | Xử lý trong hackathon |
|---|---|---|---|
| 1 | **4/8 mã nguyên nhân của bước `CH-13` không có câu trong A2 §4.** A2 có `LIMIT`, `PERIOD`, `NOBEN`, `WAIT`, `EXCL`, `NOTREL`, `NOTCOV`. `CH-13` còn dùng **`CAP_VISIT`, `COPAY`, `DEDUCT`, `LIMIT_TOTAL`** — không mã nào có câu tiếng Việt | Cột *Khoản không được chi trả* (S14 F9) sẽ trống đúng ở nhóm nguyên nhân phổ biến nhất — đồng chi trả áp cho gần như mọi hồ sơ | Viết 4 câu mới theo đúng khuôn §4 (nêu khoản mục + số tiền + lý do), đặt mã tạm `CAUSE-CAP_VISIT`… và **ghi rõ trong deck là câu đề xuất, chưa qua A2**. Không dùng lại mã A2 cho chúng |
| 2 | **Ranh giới `CLM-NOTIF-005` vs `-008` chưa quyết** (A2-09) | Vì `CH-09` áp đồng chi trả, gần như mọi hồ sơ đều thấp hơn số yêu cầu → `-005` gần thành message chết. Đây là câu đa số khách hàng đọc, và nó là trường F1 của S14 | Chọn **phương án (b): chỉ dùng `-008` khi có quyết định của con người gây ra chênh lệch**; đồng chi trả theo hợp đồng thì vẫn là `-005`. Kịch bản A → `-005`, kịch bản B → `-008`. Ghi vào Site Setting |
| 3 | **Không có câu nào cho cờ gian lận gửi khách**, và mã `FRAUD` trong hành động từ chối **không có câu §4 tương ứng** | AC-C2 yêu cầu "thư không được buộc tội" nhưng không có câu để dùng | Kịch bản C **không kết thúc bằng thư từ chối**. Dừng ở cờ mở + xoá/giữ cờ trên console. Nếu buộc phải từ chối thì dùng mã nguyên nhân thực tế (`NOTCOV`/`NOTREL`), **không bao giờ dùng `FRAUD` trong văn bản gửi khách** |
| 4 | **Không có message nào cho tầng agentic**: chạy lại chuỗi, timeout agent, `ReplayMode` đang bật, số tiền đã cũ. A2 §8 mới có 3 câu liên quan (`-013`, `-014`, `-015`) và **còn 21 console message chưa được cấp số** (A2-10) | Console sẽ có chữ không nằm trong catalogue | Viết bằng **tiếng Anh, trên console, không gửi khách** — vùng an toàn. Đặt mã tạm dạng `AGT-CONSOLE-0nn`, **không lấn vào dãy `CLM-CONSOLE-0nn`**, đặc biệt không dùng `CLM-CONSOLE-024` (RESERVED) |
| 5 | **`TBC-nn` chặn toàn bộ nhánh từ chối theo loại trừ** (A2-04). Cả 10 term của pilot product mang `clauseRef` dạng `TBC-nn`; `BR-TERM-10` cấm term có clauseRef tạm thời được decline — nó raise checkpoint; §7.2 cấm thư render `TBC-` | **Hiện tại không một thư từ chối theo loại trừ nào ra khỏi hệ thống được.** Câu `EXCL` của §4 không dùng được | Demo **không dùng `EXCL`**. Kịch bản B dùng `NOTCOV` (không cần clauseRef); kịch bản D nếu giữ thì dùng `WAIT`. Nếu giám khảo hỏi về điều khoản loại trừ: trả lời thẳng rằng đó là checkpoint cho người, kèm lý do — **đây là một câu trả lời mạnh, không phải điểm yếu** |
| 6 | **Thư `CLM-LETTER-001/002` chỉ có cấu trúc, không có câu mẫu.** A2 §7 liệt kê từng mục nhưng không có bản tiếng Việt hoàn chỉnh của Opening, What is not affected, Right of review | AG-5 không có gì để render ở ba mục đó, và S14 F17 không có nội dung | Viết ba đoạn tiếng Việt cố định theo tinh thần M-2/M-10 (xưng "Quý khách", không dấu chấm than, mục khiếu nại nêu rõ ai/cách nào/trước ngày nào). Đánh dấu **soạn cho hackathon**, đưa vào backlog xin A2 cấp mã |
| 7 | **Không có câu cho luồng auto-approval.** Khách ở kịch bản A đi từ `SUBMITTED` thẳng tới `PAID` trong ~66 giây | Không thực sự là lỗ hổng — M-2 chỉ yêu cầu nói việc tiếp theo. Chuỗi `-001` → `-005` → `PAY-NOTIF-002` đã đủ | Không viết thêm. **Không quảng cáo AI với khách hàng** |
| 8 | **Xung đột đơn vị tiền `đ` / `₫`** giữa A2 M-4 và BRD hackathon NFR-10 | Hai màn hiển thị khác nhau thì giám khảo sẽ để ý | Đã chốt ở §8.1: `đ` cho mọi thứ khách đọc, `₫` được phép trên console |
| 9 | **`CLM-CONSOLE-008/009/010/011/012`** (second admin, xung đột lợi ích, đổi tài khoản sau khi duyệt) tồn tại trong A2 nhưng **không màn nào trong 6 màn dùng tới** | Không ảnh hưởng demo | Không dựng. Ghi trong deck là phần pilot |
| 10 | **BRD hackathon §9.5 đánh số màn khách hàng khác D2 Part IV** (S09/S10/S11/S13 so với S11/S12/S13/S14) | Hai tài liệu nói về cùng một màn bằng hai mã — sẽ lẫn khi trao đổi trong đội | **D2 thắng.** Tài liệu này dùng S11/S12/S13/S14. Sửa BRD ở lần cập nhật tới, hoặc thêm một dòng ánh xạ vào §18 của BRD |
| 11 | **`CLM-NOTIF-001` ghi trigger là `SUBMITTED`, thực tế phát lúc đăng ký** (D2-38, đang mở) | Không ảnh hưởng câu chữ — **không chữ tiếng Việt nào thay đổi**, chỉ cột trigger của A2 cần viết lại | Build theo D2: phát lúc đăng ký. Ghi nhận D2-38 còn mở |
| 12 | **Hủy hồ sơ khi trạng thái đã chạy tiếp chưa có message** (D2-30, đang mở) | Khách bấm *Hủy hồ sơ* trên hồ sơ vừa chuyển sang `UNDER_ASSESSMENT` sẽ không nhận được câu nào | Kiểm ở server theo trạng thái hiện tại; nếu đã chạy tiếp thì **không phát `CLM-NOTIF-009`**. Nếu cần một câu, dùng `SYS-ERR-002` tạm — và ghi rõ đây là chỗ D2-30 còn thiếu |

---

## 10. Danh sách kiểm trước khi bắt đầu code

- [ ] Bảng message với đủ **53 message ở §5**, cột: `Code`, `Vi`, `En`, `Channels`, `Placeholders`, `Source` (A2 v1.2 / soạn cho hackathon).
- [ ] Viết **một** hàm render `(messageCode, placeholderMap, language) → string`. Không có đường vòng nào khác ra màn hình (§8.4).
- [ ] Hàm format tiền và format ngày dùng chung, không viết inline.
- [ ] Bảng tra **7 loại chứng từ → tên tiếng Việt A1 §6**, và **3 mức → "Bắt buộc"/"Nên có"/"Tùy chọn"**. Không màn nào render mã.
- [ ] Bảng tra **12 `lossType` → tên tiếng Việt**; dropdown ở S11 chỉ mở 8 giá trị sống.
- [ ] Checklist S12 sinh từ bảng 8+1 dòng (§3.2), không hard-code theo kịch bản.
- [ ] Server Action ánh xạ 13 trạng thái nội bộ → 11 trạng thái hiện cho khách (§3.3), một nơi duy nhất, có xử lý 4 chỗ cố tình che.
- [ ] Hai tài khoản người thật: `officer.demo` và `approver.demo`. Kiểm `CLM-CONSOLE-005` chặn được trước ngày rehearsal.
- [ ] Rà lại 6 màn: mọi ô `—` trong ma trận §7.2 là **vắng mặt trong DOM**, không phải `Enabled = False`.
- [ ] Grep HTML render của portal với danh sách 13 trạng thái nội bộ + 7 mã chứng từ + 3 mức `REQUIRED/EXPECTED/OPTIONAL` + 12 mã `lossType` — **không được khớp cái nào**.
- [ ] Kiểm S14: dòng bị từ chối và dòng được trả **trong cùng một `<table>`**, không accordion, không "xem chi tiết".
- [ ] Chốt phương án (b) cho `-005`/`-008` và ghi vào Site Setting.
- [ ] Bốn câu cho `CAP_VISIT` / `COPAY` / `DEDUCT` / `LIMIT_TOTAL` — viết, gắn nhãn "soạn cho hackathon", đưa PO duyệt trước 22/09.

*Hết tài liệu.*
