# Build 00 — Chuẩn nền tảng & Phạm vi

**Lập:** 16/09/2026 · **Phiên bản:** v2.4 — cập nhật 21/09: **tenant IDP là 177, bộ chứng từ 198** (không phải 176/194); §4 #7 sửa (**cổng tự duyệt là 4 điều kiện**); số unit test sửa 65 → **71**
**Trạng thái:** nguồn chân lý cho **chuẩn và phạm vi**
**Thay thế:** mọi sample, seed và giả định trong prototype v4 / v5 / v6.x

> **Cập nhật 20/09/2026 — đọc trước khi dùng §8 và §10.**
> Lớp giao diện đã dựng xong trên **hai app**: `ClaimMVP` (console cán bộ, 10 màn) và `MyInsurance` (portal khách, 7 màn), tách nhau ở rev 87, cùng dùng Entity và Service Action của ClaimMVP. Nguồn chân lý về **tiến độ** nay là `claude/trang-thai-build-2026-09-20.md`.
>
> **Cập nhật 21/09/2026 (a) — §5 xung đột 5 đã sửa.** Cặp số `2.000.000 → 1.880.000` từng được ghi là đã phân xử; nay xác định là **chép nhầm hồ sơ**. Kịch bản demo mới đã kiểm tay và khớp A5 **đến từng đồng**. Xem §5 dòng 5.
>
> **Cập nhật 21/09/2026 (b) — §4 #7 ĐÃ ĐẢO CHIỀU.** Tài liệu này từng ghi *"cổng tự duyệt là 3 điều kiện, BRD ghi 4 là sai và BRD phải sửa"*. **Sai là câu đó.** PO chốt 21/09: **4 điều kiện**, BRD đúng. `build-04` §3.1 đã ghi đúng từ đầu và là chuẩn.
>
> **Cập nhật 21/09/2026 (c) — số unit test là 71, không phải 65.** `build-04` §9 có **71 test** (§9.1 = 46 · §9.2 = 7 · §9.3 = 7 · §9.4 = 11).
>
> ### ⚠️ Cập nhật 21/09/2026 (d) — BA CON SỐ IDP Ở §1 ĐÃ SAI TỪ 17/09
>
> §1 viết ngày 16/09 ghi **tenant 176 · user 203 · bộ 194**. Lần gọi **thật** ngày 17/09 (Sổ tay API BnK IDP, 35 endpoint) chạy trên **tenant 177 · user 204 · bộ 198 `HEALTH_CLAIM_OS_V1`**, với **9 template tự dựng 6090–6098** — vì tenant 177 không có template y tế nào.
>
> **Sổ tay 17/09 thắng**, vì nó là lần gọi thật. §1 và §4 #5 đã sửa bên dưới. Ba con số này đi thẳng vào Site Setting, và sai một cái thì hồ sơ **trôi qua im lặng, vẫn bị trừ quota** (bẫy 1 của sổ tay).
>
> Bản đầy đủ: `openspec/changes/active/idp-integration-live/` (sổ tay HTML đặt cạnh spec).

---

## 1. Quyết định nền tảng

> **Chuẩn gốc là Claim MVP.** Bộ deliverable A1–A5 + D1–D3, cộng đặc tả `IDP_APIs_for_Claim_MVP_v1.0`.

Đây cũng là bộ mà BRD hackathon đang bám, nên toàn hệ nhất quán.

| Chiều | Giá trị |
|---|---|
| Loại chứng từ | **8 loại, 3 tier** — Tier 1 trích xuất: `T1-01` bảng kê chi phí · `T1-02` đơn thuốc · `T1-03` phiếu khám · `T1-04` hóa đơn VAT (**chỉ XML**). Tier 2 chỉ phân loại: `T2-01` giấy tờ tùy thân · `T2-02` KQ xét nghiệm/CĐHA · `T2-03` báo cáo y tế/GCN phẫu thuật · `T2-04` (mới v1.3). Tier 3 không xử lý |
| Phân loại tổn thất | `lossType` **12 giá trị** (8 sống: `ILL_OP` `ILL_IP` `DEN_OP` `DEN_IP` `ACC_OP` `ACC_IP` `MAT_OP` `MAT_IP` + 4 counter-only) |
| Trạng thái hồ sơ | `claimState` **13 giá trị** → **11 trạng thái** hiện cho khách |
| Mã từ chối | **7 mã đóng**: `LIMIT` `PERIOD` `NOBEN` `WAIT` `EXCL` `NOTREL` `NOTCOV` |
| Cổng tự duyệt | **4 điều kiện** — G1 tiền & không có gì bị cắt · G2 chứng từ · G3 lâm sàng · G4 gian lận (`build-04` §3.1) |
| Chuỗi tính tiền | A5 §6.1 + D1 §5.3 — 14 bước |
| Sản phẩm | HC-STD / HC-PLS / HC-PRM, 17 benefit item mỗi plan |
| **Tenant IDP** | **177** · user **204**, roleId 2, **vai Tenant** — vai Partner KHÔNG gọi được `/request/*` · `https://idp-api.idpstg.bnksolution.com` |
| **Bộ chứng từ** | **198 `HEALTH_CLAIM_OS_V1`**, 12 template · **template 6090–6098 tự dựng** bằng `POST /template` |
| Flow đánh giá lâm sàng | `5ed8e4fc-27a5-45a7-b104-1216fcfd0797` — **IDP đã có mail cam kết giữ nguyên đến 08/10** |

*(Lịch sử: bản 16/09 ghi tenant 176 / user 203 / bộ 194. Sai — xem ô cảnh báo (d) ở trên.)*

**KHÔNG dùng:** `CL05…CLM_04`, 4 `claim_type` `MEDICAL_*`, template `777/776/1091/2645/1141`, Docset Map / Template Map của MBAL.

**CẤM gọi:** `PUT /request/{id}/result` (sửa được kết quả gốc ⇒ kết quả IDP không bất biến) · `POST /tool/*`. **HỎNG:** `POST /request/from-url`.

## 2. Bộ tài liệu build

| Doc | Nội dung | Quy mô |
|---|---|---|
| `build-00-chuan-va-pham-vi` | Tài liệu này | — |
| `build-01-data-model-va-enum` | **32 entity** (10 hợp đồng · 15 claim · 7 IDP), **31 enum**, 30 ràng buộc phải tự thêm, 25 bẫy | ~177 KB |
| `build-02-tich-hop-idp` | **32 endpoint**, bảng templateId, payload thật. **Bổ sung 21/09:** sổ tay 35 endpoint chạy thật trên tenant 177 — `openspec/changes/active/idp-integration-live/` | |
| `build-03-nghiep-vu-va-rule` | 8 loại chứng từ, **88 field**, **R1–R9**, 6 packageState, term engine, ~45 rule BR-* | |
| `build-04-chuoi-tinh-tien-va-seed` | 14 bước, 7 mã từ chối, **cổng tự duyệt 4 điều kiện (§3.1)**, seed 51 benefit + 51 waiting period × 3 plan, **71 unit test** | ~101 KB |
| `build-05-agent-va-system-prompt` | 6 agent, **39 tool binding / 34 Server Action**, system prompt dán thẳng | |
| `build-06-man-hinh-va-wording` | 6 màn console + 4 màn khách, **53 message nguyên văn**, 12 lỗ hổng câu chữ | |
| `trang-thai-build-2026-09-20` | **Tiến độ thật** — app nào có gì, còn thiếu gì, rủi ro mở | — |
| `idp-test-thuc-te-tenant-177-2026-09-17` | Bản ghi lần gọi thật — nguồn của §1 dòng tenant | — |
| `openspec-quy-uoc-thu-muc` | Quy ước `changes/active` trên đĩa | — |

**Tra cứu nhanh:**

| Câu hỏi | Doc |
|---|---|
| Entity này có field gì? Enum này có giá trị nào? | 01 |
| Gọi IDP thế nào, payload ra sao, template id nào? | 02 + `idp-integration-live` |
| Hồ sơ này cần chứng từ nào? Trích được field gì? | 03 |
| Số tiền tính ra từ đâu? Seed sản phẩm ở đâu? Cổng tự duyệt gồm gì? | 04 |
| Agent được phép làm gì? Prompt viết sao? | 05 |
| Màn này hiện gì? Câu này nói với khách thế nào? | 06 |
| Hiện đã build tới đâu? Còn thiếu gì? | `trang-thai-build-2026-09-20` |

## 3. Bỏ gì

| Bỏ | Lý do |
|---|---|
| Prototype v4 / v5 / v6.x làm bài dự thi | BTC nói rõ: *"MVP chạy thật trên ODC + Agent Workbench, không nhận prototype/interactive model"*. Giữ **chỉ làm spec hình ảnh và ảnh trong deck** |
| Toàn bộ bộ mã MBAL-IDP | Đã quyết ở §1. Còn trong Phụ lục của doc 01/02/03 để tham chiếu |
| Số đo "83,49 % trường / 41,67 % bảng" | Đo trên bộ template MBAL, 15 chứng từ, staging — **không phải template Claim MVP**. Không dùng làm số đo của hệ này |
| Contract cảnh báo con dấu | Thuộc MBAL-IDP Phase 2. A4 xếp Tier 2 là classify-only, không có kiểm tra giả mạo |
| Con số "972 câu hỏi QnA" | Sheet `QnA` thực ra **rỗng** — chỉ 2 dòng header |
| `extraParams` để correlate | **Type đóng, chỉ 4 khoá MBAL, thực tế trả `{}`.** Correlate chuyển sang trường **`name`** = `ClaimNo` |
| `HospitalCode` / `PlaceOfTreatment` / `SurgeryCodes` **từ flow** | **Mới 21/09.** Flow trả giá trị giữ chỗ, và khi không tra được thì **bịa** ra `01001` = dòng đầu bảng master 322. Tra bảng 322 qua OpenSearch thay vì tin flow |

## 4. Mười phát hiện phải biết trước khi gõ code

Tất cả đến từ việc đọc nguồn thật, không phải suy đoán.

| # | Phát hiện | Ảnh hưởng |
|---|---|---|
| 1 | **`autoStart` KHÔNG hoạt động** — nhận `true`, trả 200, lưu `false`, phiên nằm im ở status 2 | Luôn gọi `GET /workflow/run-workflow-request/{id}` ngay sau, **retry 3×5 s — ngoại lệ 4xx-retry duy nhất** |
| 2 | **Chọn đúng flow quyết định tất cả.** `5ed8e4fc` trả JSON 4/4; `8d873709` trả `{}` **kèm HTTP 200** | Ghim flow id vào Site Setting. **HTTP 200 + `{}` là lỗi**, không phải "không có cảnh báo". *(21/09: IDP đã có mail giữ flow đến 08/10)* |
| 3 | **`PUT /request/{id}/result` chạy được** ⇒ kết quả IDP **không bất biến** | Phá giả định `D3 §I-09`. Tự copy `RawExtractionJson` ngay lần đọc đầu, và **cấm** dùng endpoint này |
| 4 | ~~`T1-03` phiếu khám chưa có template~~ — **ĐÃ GIẢI QUYẾT 17/09** | Tự dựng **template 6094**, khớp, confidence 86.56. Tenant 177 không có template y tế nào nên đã tự dựng cả 9 cái (6090–6098) bằng `POST /template` |
| 5 | ~~Set 193 không chứa `5383`, dùng set 194~~ — **THAY BẰNG:** dùng **bộ 198 `HEALTH_CLAIM_OS_V1`** trên tenant 177, 12 template, tự tạo 17/09 | Cả 193 và 194 đều thuộc tenant cũ. `POST /template/document-set` **không validate gì** — tạo xong phải `GET /template/{id}` từng id để xác nhận, nếu không sẽ dính bẫy "templates_data rỗng" |
| 6 | **Crosswalk document-class đang rỗng** (`Q-IDP-01`) | **Hạ cấp 21/09:** đợt D đã cắt ⇒ **giới hạn đã biết, không còn chặn**. Nêu trong deck |
| 7 | **Cổng tự duyệt là 4 điều kiện** — G1 số tiền & không có gì bị cắt · G2 chứng từ & chất lượng trích xuất · G3 lâm sàng sạch & không có điểm kiểm mở · G4 không có tín hiệu gian lận | **SỬA 21/09 — dòng này trước đây ghi ngược.** Bản cũ nói *"3 điều kiện, BRD sai"*. **PO chốt 21/09: 4 điều kiện, BRD đúng.** Nguồn chuẩn: **`build-04` §3.1** — A5 §6.4 có 3, BRD §9.1.3 thêm G4 (fraud, vì slice hackathon có AG-4), bộ phải hiện thực hoá là **hợp nhất = 4**. UI đã dựng đúng ⇒ **không sửa code, không sửa BRD.** Còn mở: **4 điều kiện NÀO** (§9) |
| 8 | **Làm tròn: đồng nguyên gần nhất, ở TỪNG BƯỚC** | BRD ghi "làm tròn xuống 1.000 ₫ ở một bước riêng" — sai, phải sửa |
| 9 | **`T1-01` line-item là 17 cột, không phải 14** — v1.2 thêm `L15`, `L16`, v1.3 thêm `L18`. `L17` cố ý bỏ trống. Cả 3 mang `Prov = X?` | Đừng seed 14 cột |
| 10 | **10 term pilot đều mang `clauseRef` = `TBC-nn`**, mà `BR-TERM-10` cấm từ chối viện dẫn một cái tạm | **Nhánh auto-decline `EXCL` không chạy được.** Demo tránh `EXCL`, dùng `NOTREL` / `NOTCOV` / `WAIT` |

### 4.1 Bốn bẫy IDP mức NẶNG — thêm 21/09 từ sổ tay

| Bẫy | Vì sao nguy | Chặn thế nào |
|---|---|---|
| **`templates_data` rỗng nhưng HTTP 200, `errorCode` null, VẪN TRỪ QUOTA** | Nộp với template không thuộc tenant: request vẫn `Completed`, không lỗi, kết quả rỗng. **Một hồ sơ trôi qua mà không ai biết** | Adapter coi `templates_data = []` là **`EXTRACTION_ERROR` (lỗi cấu hình)**, không phải "không phân loại được". Đếm template yêu cầu vs thực khớp |
| **Tiền bị cắt phần nghìn** | OCR thô đọc đúng `300.000`, extraction trả `300`. Với chứng từ tài chính, đây là sai số **tiền bồi thường** | Thêm *"chỉ lấy chữ số, bỏ dấu phân cách hàng nghìn"* vào mô tả **mọi field tiền**. Đã xác thực trên 6091 |
| **Dòng bảng nằm ở khoá `data`, không phải `rows`** | `templates_data[].data.tables[0].data` — mảng 2 chiều, dòng đầu là header. Code đọc `rows` nhận mảng rỗng, **im lặng** | Đọc `data` |
| **Flow bịa mã bệnh viện** | Chứng từ không có tên bệnh viện, flow vẫn trả `01001` = dòng đầu bảng 322 | Không dùng ba trường master data từ flow (§3) |

**`User-Agent` kiểu trình duyệt là BẮT BUỘC cho mọi call** — thiếu thì Cloudflare 403/1010. **Không có refresh token** (401 ⇒ đăng nhập lại). **`templates` gửi chuỗi CSV**, không phải mảng — nhưng `/template/document-set` lại nhận mảng số.

## 5. Xung đột giữa các tài liệu — đã phân xử

| # | Xung đột | Phân xử |
|---|---|---|
| 1 | **Chuỗi 14 bước:** A5 §6.1 + D1 §5.3 lệch BRD ở **4 chỗ làm đổi tiền** | **A5 là chuẩn. BRD phải sửa theo.** Bảng đối chiếu ở `build-04` §1.1 |
| 2 | **Số loại chứng từ:** A4 v1.3 có **8**; A1 v1.5 ghi "seven" | A4 v1.3 mới hơn. `T2-04` mức mặc định còn bỏ ngỏ (`B-24`) |
| 3 | **Trạng thái:** 13 (D2) vs 14 (BRD) | **D2 là chuẩn.** Trạng thái con agentic của BRD quy về 13 + `Claim.Queue` |
| 4 | **Đơn vị tiền:** `1.200.000 đ` (A2 M-4) vs `1.880.000 ₫` (BRD NFR-10) | **A2 thắng** cho mọi thứ khách đọc |
| **5** | **~~Kịch bản B `NOTCOV`, `2.000.000 → 1.880.000`~~** — SỬA 21/09 | **Cặp số bị CHÉP NHẦM HỒ SƠ.** `1.880.000` thuộc **kịch bản A của prototype**: ngoại trú HC-STD, 6 dòng, yêu cầu 3.128.000, có đồng chi trả 20% và trần `200.1`/`200.2` — **khớp A5 §4.2 đến từng đồng**. **Kịch bản demo chuẩn:** `CLM-2026-0001` · run 1 = **2.000.000** · run 2 = **1.880.000** · ba khấu trừ: `COPAY` −500.000/−470.000 · `CAP_VISIT` −610.000 và −18.000 · **`NOTREL`** −168.000. **Dòng bị từ chối là `NOTREL`, KHÔNG phải `NOTCOV`.** Bản kiểm tay: `openspec/changes/active/verify-entity-vs-build01/batchB2-spec.md` §Lối C |
| 6 | **Mã `FRAUD` không có câu thư nào trong A2** | Kịch bản C **không kết thúc bằng thư từ chối** — dừng ở trạng thái chặn + ghi chú cán bộ |
| 7 | **Đánh số màn kết quả:** S14 (D2) vs S13 (BRD) | D2 thắng |
| 8 | **I-15/I-16 clinical assessment** thiếu `proposedReasonCode`, `evidence[]`, band, `linesAssessed` | **Xác nhận 17/09 trên flow thật: đúng, và không có kế hoạch có.** 20 khối văn bản tiếng Việt cho cả hồ sơ ⇒ AG-3 phải tự lấp, **Track 2 trình bày là chế độ tư vấn, KHÔNG phải chốt kiểm soát** |
| **9** | **Cổng tự duyệt: 3 (§4 #7 bản cũ, A5 §6.4) vs 4 (BRD, `build-04` §3.1, UI)** | **SỬA 21/09 — 4 thắng.** PO chốt: BRD đúng, câu "3 điều kiện" sai và đã sửa. Không sửa BRD, không sửa code |
| **10** | **Tenant IDP: 176 / user 203 / bộ 194 (§1 bản 16/09) vs 177 / 204 / 198 (chạy thật 17/09)** | **SỬA 21/09 — lần gọi thật thắng.** §1 đã sửa. Ba con số đi vào Site Setting; sai là hồ sơ trôi qua im lặng và vẫn bị trừ quota |

### 5.1 Câu nói trên sân khấu, rút từ chính phép tính

Từ chối một khoản **168.000 ₫** chỉ làm công ty bảo hiểm **đỡ 120.000 ₫** — phần trần và đồng chi trả hấp thụ mất 48.000 ₫.

Đây là câu trả lời tốt nhất cho *"làm sao biết con số này không phải hard-code"*: một phép trừ thẳng ra 168.000. Chỉ một chuỗi chạy thật mới ra 120.000.

### 5.2 Câu chuyện IDP tốt nhất cho sân khấu — mới 21/09

Trộn cố ý một chứng từ **của bệnh nhân khác** vào hồ sơ 9 chứng từ. Flow bắt đúng **cả bốn chỗ lệch**: ngày vào viện, ngày ra viện, giấy tờ tuỳ thân, mã ICD (bảy cái `K35.8`, một cái `K29.1` — đúng cái lạc).

Đã chạy thật 17/09. Đây là phần kể chuyện mạnh nhất của nửa IDP.

**Biết trước hai chỗ ồn:** *Kiểm tra Danh mục bệnh viện* và *Kiểm tra Mã phẫu thuật* đều ở mức 1 dù chỉ là thông tin. Số lẻ `0.888` cho thấy `warning_level` là **trung bình cộng**, không phải mức nghiêm trọng ⇒ **không dùng nó làm cổng**.

## 6. Bốn mã nguyên nhân không có câu chữ

`CAP_VISIT` · `COPAY` · `DEDUCT` · `LIMIT_TOTAL` — không có câu nào trong A2.

Đồng chi trả áp gần như mọi hồ sơ ⇒ khối **"Vì sao Quý khách tự chịu"** sẽ **trống đúng ở nhóm phổ biến nhất**. Chi tiết ở `build-06` §8.

> **21/09 — ĐÃ DUYỆT hai câu cho hồ sơ demo.** PO duyệt, dán nhãn **"soạn cho hackathon"** trong deck:
>
> - `COPAY` — *"Đồng chi trả 20% theo hợp đồng: {amount}"*
> - `CAP_VISIT` — *"Phần vượt trần {cap} mỗi lần khám của quyền lợi {benefit}: {amount}"*
>
> `DEDUCT` và `LIMIT_TOTAL` **vẫn trống** — không có trong hồ sơ demo, đẩy đợt F.
>
> **Không có mã nào cho trần theo NGÀY** trong 8 `causeCode`. Đừng thiết kế kịch bản nào chạm per-day cap.

## 7. Số đo cho pitch — trạng thái thật

| Chỉ số | Trạng thái |
|---|---|
| **Độ chính xác trích xuất** trên template Claim MVP | **Chưa đo.** `POST /request/compare` có sẵn, nhận XLSX ground truth. **Việc đáng làm nhất cho phần ROI** |
| **Chất lượng phân loại** | ✅ **9/9 đúng** sau khi tinh chỉnh mô tả (17/09). Confidence 70–90: 6091 bảng kê **90.5** · 6090 đơn thuốc **89.4** · 6098 bản tường trình **70.4** (thấp nhất) |
| **Thời gian xử lý** | ✅ **Đo thật:** trích xuất 9 trang song song ~65 s · đánh giá lâm sàng **118 s** · **tổng ~3 phút/hồ sơ**. 1 chứng từ mất 72 s ⇒ không tăng tuyến tính |
| Thông lượng, uptime | Chưa đo |
| Số liệu MBAL (83,49 % / 41,67 %) | **Không dùng được** — bộ template khác, mẫu 15 chứng từ, môi trường khác |

Cái duy nhất chuyển được sang, có điều kiện: **bảng kê chi phí là loại chứng từ đạt thấp nhất (60,9 %) trong mọi loại MBAL đã đo** — mà `T1-01` chính là chứng từ mọi dòng tiền đến từ đó. Nêu như **rủi ro đã biết**, không phải số đo của hệ này. *(Ngược lại, trên template tự dựng 6091 thì confidence là 90.5 — con số này mới của mình.)*

⇒ **Chạy `POST /request/compare` với 10–20 chứng từ đã che PII trước 29/09.**

**Hệ quả kiến trúc của con số 118 s:** trần server action là **10 s mặc định / 60 s tối đa** (SA 21/09). Một bước IDP đã gấp đôi trần. ⇒ **IDP chạy ngoài lượt agent**, agent đọc kết quả đã có. Và **demo phải có màn hình chờ** — 3 phút không được đứng im.

## 8. Lịch còn lại — *cập nhật 21/09*

Hôm nay **21/09**. Pitch **07–08/10**. Tuần 30/09–06/10 là rehearsal → **8 ngày build thực**, một người.

Đợt A và B đã xong. **Chuỗi 14 bước xong 21/09, sớm hơn ngân sách 4 ngày; 46/71 test pass.**

| Mốc | Phải xong |
|---|---|
| **22–25/09** | 25 test còn lại · chốt *4 điều kiện nào* · `F-CH-1`/`F-CH-3` · nối `A5Ledger_Finalize`/`Reverse` vào Approve rồi chạy lại `T-CHAIN-06` + `T-CH14-b/c` qua nút thật |
| **26/09** | **Mốc quyết định.** Chưa pass 71/71 thì cắt tiếp |
| **27/09** | Gọi IDP thật một lần trên tenant 177 → ghi `RawExtractionJson` → bật `ReplayMode` |
| **28/09** | AG-2 trên Agent Workbench ghi `AgentRun` · chạy `compare` lấy accuracy · deck |
| **29/09** | **Đệm — không lấp việc vào đây** |
| **06/10** | **Đóng băng staging và tenant 177** |
| 30/09–06/10 | Rehearsal, không code |

**Đã cắt (quyết 21/09), không làm trước pitch:**

1. Kịch bản **A, C, D** — chỉ giữ `CLM-2026-0001`
2. **Đợt D** — `DocumentChecklistRule` và bộ static checklist
3. **53 message × 17 màn** — chỉ áp ~8 câu của kịch bản demo, cộng hai câu `COPAY`/`CAP_VISIT`
4. **Dọn inline console** — console được phép hiện tên trạng thái nội bộ (A2 §0.4)
5. **Tầng IDP 7 entity** — chỉ dựng `IdpSession` + `IdpReplay` + wrapper REST
6. **AG-0 đã bỏ** — SA xác nhận 21/09 agent không gọi agent lồng nhau
7. **20 node seed `ChainRun`/`CalcSnapshot` của hồ sơ A và C** — nợ có chủ ý, đợt F. Hồ sơ B và D đã dọn 21/09

> **Sàn tuyệt đối không được thủng:** một agent trên Agent Workbench, gọi một Server Action, ra một con số đúng, có bản ghi `AgentRun`.

## 9. Việc phải hỏi người khác

| Hỏi ai | Nội dung | Hạn |
|---|---|---|
| **Đội IDP** | ✅ ~~Ghim flow~~ — **đã có mail 21/09: stable đến 08/10**, gồm flow `5ed8e4fc`, template 6090–6098, bộ 198 trên tenant 177. 🔴 **CÒN CHẶN — QUOTA:** tenant 177 đang `blockLimit 1`, **đã dùng 14 trang**. Không ai biết một block bằng bao nhiêu trang. Demo 3 hồ sơ × 9 chứng từ = **27 trang một lượt**. Cần: *một block = bao nhiêu trang* · nâng lên **≥ 300 trang tới 31/10** · *hết quota thì API báo mã gì* (đang giả định 1003). **"Stable" không bao gồm quota — phải hỏi riêng.** 🟡 Không chặn: ba trường master data trả giá trị giữ chỗ · `mbal/template-map` cho 6090–6098 · TTL token / rate limit thật | **quota: 22/09** |
| **SA OutSystems** | ✅ **Đã đóng 21/09.** Agent **không** gọi agent lồng nhau. Trần server action = **10 s mặc định / 60 s tối đa** qua `ServerRequestTimeout` — **`build-05` §9 ghi 120 s là sai, phải sửa tại nguồn** | đã đóng |
| **PO / BA** | ✅ ~~Cổng 3 hay 4~~ — **4 điều kiện, BRD đúng** (21/09) · ✅ ~~2 câu `COPAY`/`CAP_VISIT`~~ — **đã duyệt**, xem §6 · 🔴 **CÒN MỞ: 4 điều kiện NÀO** — màn Approval và bảng G1–G4 của `build-04` §3.1 đang liệt hai bộ khác nhau cùng đếm ra 4 (audit rev 47 thấy điều kiện #2 trên màn là *"đề xuất chờ người duyệt"*, doc xếp việc đó vào G3). **Đọc `EvaluateAutoApprovalGate`, lấy code làm chuẩn** · `D2-159` term `TBC-` · `B-24` `T2-04` | 22/09 |
| **BnK management** | Có nêu tên khách hàng tham chiếu không | 29/09 |

## 10. Việc làm ngay, theo thứ tự — *cập nhật 21/09*

1. ~~Chuỗi 14 bước theo A5~~ — **xong 21/09**, chạy 719 ms
2. **Publish ClaimMVP** (bản dọn seed hồ sơ B/D đang chờ) → re-run seed → **chạy lại cả 46 test trên fixture sạch**
3. **Hỏi IDP về quota** — gửi hôm nay, không gộp vào mail cũ. Đây là rủi ro duy nhất có thể làm demo đứng giữa chừng
4. **Đọc `EvaluateAutoApprovalGate`** để chốt *4 điều kiện nào*, rồi sửa bảng cổng tab Approval — **mỗi điều kiện Đạt/Không đạt kèm lý do cụ thể, không chỉ ✓/✕**
5. **`F-CH-1` và `F-CH-3`** — hai card `0 ₫` trên màn hero
6. **25 test còn lại** — `T-CH14` ×6 → `T-SEED` ×11 → `T-GATE` ×7
7. **Nối `A5Ledger_Finalize`/`Reverse` vào Approve/Decline**, chạy lại `T-CHAIN-06` + `T-CH14-b/c` qua nút thật
8. **REST Consume IDP** — `POST /auth/login` → `GET /user/me`; **set `User-Agent` trước khi test method thứ hai**
9. **Chuỗi 11 bước IDP** trên tenant 177 → ghi `RawExtractionJson` → `IdpReplay` → bật `ReplayMode`. Chặn đủ 10 bẫy (`idp-integration-live` §3)
10. AG-2 trên Agent Workbench gọi `RunAdjudicationChain`, ghi `AgentRun` · **description mọi `A5StepNN_*` mở đầu "bước nội bộ, không phải tool"**
11. Chạy `POST /request/compare` lấy accuracy thật
12. Áp 2 câu `COPAY`/`CAP_VISIT` + ~8 câu của kịch bản demo
13. **Sửa `build-05` §9 tại nguồn** — 120 s → 10 s/60 s, nguồn SA 21/09
14. Đợt F dọn: nút Migrate A6 (no-op), `DecisionCode`, `Seed_BenefitSnapshots`, nhãn inline console, `CoinsurancePct`, 20 node seed hồ sơ A/C, **xoá fixture `TST-`/`FX-`**

**Đã xong:** data model chạy được UI · 17 màn · `FormatVND` · `DecideRecommendation` · `AgentTrace` · chuẩn hoá static entity · 4 đầu vào của chuỗi · cây quyền lợi A5 · 51 dòng thời gian chờ · `CapMode` · **chuỗi 14 bước + 46/71 test** · **9 template IDP tự dựng + bộ 198 trên tenant 177**.
