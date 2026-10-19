# Kiểm runtime sau đợt A — 20/09/2026 22:2x

**Read-only.** ClaimMVP **rev 131** · MyInsurance **rev 47** · `https://bnk-dev.outsystems.app/`. Chrome (phiên đăng nhập của PO: console *Nguyễn Hoài An / Approver*; portal *Nguyễn Văn An / Insured HC-STD*), 1568×726.
Ảnh: `../../../output/bnk-dev-claimmvp/shots-rev131/` (10 ảnh: 3 console + 7 portal). So với `shots-rev88/` (console) và `../../../output/bnk-dev-myinsurance/shots-rev6/` (portal).
Không Mentor, không đổi gì, **không bấm nút Migrate A6**.

---

## CHECK 1 — chip trạng thái phải có chữ (delta §8.1 "Kiểm A xong")

### Console — ClaimMVP rev 131

| Màn | Kết quả | Chi tiết |
|---|---|---|
| **WorkList** — chip strip | ✅ 13 chip, tất cả có chữ | `All 12 · Draft 0 · Received 8 · Extracting 0 · ExtractionError 1 · Pending info 0 · UnderReview 2 · PendingApproval 0 · Approved 0 · Declined 0 · Payment in progress 0 · Paid 1 · Closed 0 · Cancelled 0` |
| **WorkList** — dropdown lọc | ✅ 13 trạng thái + "All statuses" | `Draft · Received · Extracting · Extraction error · Pending info · Under review · Pending approval · Approved · Declined · Payment in progress · Paid · Closed · Cancelled` |
| **WorkList** — pill trạng thái trên 12 dòng | ✅ có chữ | `Under assessment` ×2 (+ sub-pill *Proposal pending* / *Low-confidence document*), `Paid`, `Document processing error`, `Processing documents` ×8 |
| **ClaimDetail** (ClaimId 17, CLM-2026-0001) — tab *Extraction & costs* | ✅ render như rev 88 | 4 dòng, cột Decision: `Payable` ×3, `Proposal: out of cover` ×1; header pill `Under assessment` |
| **Dashboard** | ✅ render như rev 88 | KPI 11/0/0/0, chart, *My queue* pill `Paid` / `Under assessment` |

**Ba điều thấy được, không phải lỗi chặn:**

1. **Chip strip lộ `Label` thô của 5 record cũ.** Chip strip giờ đọc `ClaimStatus.Label` (A3, `WorkList_StatusCounts` data-driven) → hiện `ExtractionError`, `UnderReview`, `PendingApproval` (Label seed = identifier, chưa humanize) trong khi rev 88 hardcode "Under assessment / Pending approval". Dropdown cùng màn thì hiện "Under review / Pending approval" → dropdown vẫn là danh sách chữ hardcode (có thêm 5 mục mới), không đọc `Label`. **Sửa rẻ nhất:** đổi `Label` của 3 record cũ thành "Extraction error / Under review / Pending approval" — hoặc quyết cho console đọc `LabelVi`. Cùng lúc quyết cho dropdown đọc entity.
2. **5 trạng thái mới không quan sát được trên pill dòng.** Không hồ sơ nào đang ở DRAFT / PENDING_INFO / PAYMENT_IN_PROGRESS / CLOSED / CANCELLED (đếm = 0 cả 5). Pill dòng là biểu thức inline (finding 1 đợt A, Mentor xác nhận `StatusUi` 0 caller) ⇒ **rủi ro pill trống với 5 trạng thái mới vẫn còn nguyên, chưa được chứng minh cũng chưa được bác** — chỉ kiểm được khi có dữ liệu, tức sau khi `Seed_Claims` được dạy 13 trạng thái (đợt F) hoặc `TransitionClaimStatus` đẩy một hồ sơ sang trạng thái mới.
3. Dashboard KPI "Under assessment 11" ≠ WorkList `UnderReview 2` — đếm khác định nghĩa (KPI gộp Received+Extracting+UnderReview?). Tồn tại từ trước đợt A, ghi để khỏi tưởng là hồi quy.

### Portal — MyInsurance rev 47

| Màn | Kết quả | Chip đọc được (DOM `.tag`) |
|---|---|---|
| **PortalHome** (My cover) | ✅ | `Under assessment`, `Processing` ×9 — **tiếng Anh**: PortalHome không đi qua A2 (Mentor: "already used StatusId comparisons, no changes needed") ⇒ chip của nó là biểu thức inline riêng, vẫn hardcode EN |
| **PortalClaims** | ✅ 10/10 có chữ | `Đang thẩm định` · `Đang xử lý chứng từ` · `Đã gửi` ×8 — đọc từ `CustomerLabelVi` qua `GetClaimStatusInfo` ✔ (mask `EXTRACTION_ERROR`/`DOC_PROCESSING` → *Đang xử lý chứng từ* hoạt động: CLM-0004 là ExtractionError, CLM-0034… là Extracting, cả hai hiện cùng chữ) |
| **PortalClaimDetail** (17) | ✅ | `Đang thẩm định`; timeline, *Item by item* 4 dòng, "What happens next" như rev 6 |
| **PortalNotifications** | ✅ 10/10 | `Đang thẩm định` · `Đang xử lý chứng từ` · `Đã gửi` ×8 |
| **PortalBenefits** | ✅ không đổi | 5 quyền lợi, progress bar |
| **PortalNewClaim** | ✅ | Dropdown *Loss type* giờ **5 mục: Maternity · Dental · Outpatient · Accident · Inpatient** (A4 thêm Accident; thứ tự không theo `Order`). Vẫn là `TreatmentType`, chưa phải `LossType` 8 giá trị sống (đợt D) |
| **PortalSupplement** | ✅ không đổi | tag `Mandatory` |

**Hai điều thấy được:**

4. **Lẫn ngôn ngữ trên portal.** Trang đang ở chế độ EN (nút "VI" góc phải là toggle) — chrome, tiêu đề, timeline tiếng Anh, nhưng chip trạng thái ở PortalClaims / PortalClaimDetail / PortalNotifications giờ **luôn tiếng Việt** (A2 gắn thẳng `CustomerLabelVi`), còn PortalHome vẫn EN. Rev 6 baseline: chip EN ("Under assessment", "Processing"). Không phải lỗi render — là hệ quả của việc `ClaimStatus` chỉ có một cột khách (`CustomerLabelVi`), không có `CustomerLabelEn`. **Đã quyết 20/09 (`../portal-bilingual-toggle/`): GIỮ toggle VI/EN.** Xử lý bằng A12 (`batchA-spec.md`): cột `CustomerLabelEn` (bản dịch của đội, không phải A2) + `Portal_CustomerState` chọn cột theo `GetCurrentLocale()`; PortalHome nối vào cùng nguồn. Chrome portal đi bằng ODC Translation ở vòng riêng.
5. Màu chip PortalClaims đồng loạt vàng (`background-yellow-lightest`) cho cả 3 trạng thái — **rev 6 cũng vàng đồng loạt**, không phải hồi quy của switch màu A2.

**Kết luận CHECK 1:** không chip nào trống trên 10 màn kiểm; 13 trạng thái có mặt ở chip strip + dropdown; **điểm mù còn lại là pill dòng console với 5 trạng thái mới (chưa có dữ liệu để kiểm)** + 2 việc wording (#1, #4).

---

## CHECK 2 — `ClaimLine.DecisionCodeId IS NOT NULL` trước khi migrate

**Cách đếm:** không có đường SQL read-only (db_query phải fork+publish harness → vi phạm "no changes"). Đếm qua tab *Extraction & costs* của **cả 12 hồ sơ** trong WorkList, cột Decision. Cột này là biểu thức inline: `Payable` khi không có mã NotCovered/Rejected (audit rev 84), `Proposal: …` khi Recommendation còn Proposed, và một chữ khác (Not related / Not covered / Rejected) khi `DecisionCodeId` = NOTREL/NOTCOV. ⇒ **đếm được chính xác các dòng mang mã từ chối; không phân biệt được NULL với COVERED/PARTIAL.**

| ClaimId | Claim no. | Số dòng | Decision hiển thị |
|---|---|---|---|
| 17 | CLM-2026-0001 | 4 | Payable ×3 · **Proposal: out of cover** (dòng 4, DRUG — Recommendation còn Proposed, chưa Confirm) |
| 18 | CLM-2026-0002 (Paid) | 4 | Payable ×4 (kể cả dòng "Thực phẩm chức năng") |
| 19 | CLM-2026-0003 | 5 | Payable ×5 |
| 20 | CLM-2026-0004 (ExtractionError) | 3 | Payable ×3 |
| 34–41 | CLM-2026-0034…0041 | **0** mỗi hồ sơ | — (portal `Portal_SubmitClaim3` chỉ tạo Claim + 5 ClaimDocument + AgentRun, không tạo ClaimLine) |
| **Tổng** | | **16 ClaimLine** | **0 dòng mang NOTREL/NOTCOV** |

**Kết quả:** **0 dòng** có mã từ chối. Theo Mentor (A6), `DecisionCodeId` chỉ được ghi ở `DecideRecommendation` nhánh NOTREL khi Confirm — và dòng duy nhất từng có proposal (CLM-0001 dòng 4) **vẫn ở trạng thái Proposed**, tức lần Confirm thử ở rev 36 đã bị `Seed_ResetDemoClaimData` xoá hoặc chưa từng lưu. Nếu seed có ghi COVERED/PARTIAL (Mentor nói không) thì con số NOT NULL tối đa là 16; nhưng chúng đều map sang `Pay` + null reason — không có gì để "migrate" ngoài việc reseed.

**Kết luận CHECK 2:** con số PO chờ là **0** (không phải ~1). Nút *Migrate decision codes (A6)* là **no-op** với dữ liệu hiện tại → dùng **"Re-run demo seed"** như PO định, **sau khi** `Seed_ClaimLines` được dạy ghi `LineDecisionId`/`DeclineReasonId` (hiện chưa — Mentor A6 không sửa seed vì seed không ghi `DecisionCodeId`). Nút migration + `MigrateA6_DecisionCodes` chờ gỡ ở đợt F (L4 đã bị bác).

---

## Ảnh

`output/bnk-dev-claimmvp/shots-rev131/`: `worklist.jpg` · `claimdetail-lines.jpg` · `dashboard.jpg` · `portal-home.jpg` · `portal-claims.jpg` · `portal-claimdetail.jpg` · `portal-notifications.jpg` · `portal-benefits.jpg` · `portal-newclaim.jpg` · `portal-supplement.jpg`

---

## Bổ sung 20/09 ~23:20 — sau A12 (ClaimMVP rev 132, MyInsurance rev 48)

| Kiểm | Kết quả |
|---|---|
| Nút "VI" header portal (rev 47, trước A12) | **Trơ.** Click → locale vẫn `en-US`, chrome/chip không đổi. Không có cơ chế ngôn ngữ sẵn có (`portal-claims-rev47-after-VI-click.jpg`) |
| PortalHome (rev 48, locale en-US) | ✅ chip **EN, data-driven**: `Submitted` ×8 · `Processing your documents` · `Under assessment`; đủ 10 hồ sơ (0001, 0004 có mặt); thứ tự đổi thành mới-nhất-trước (0041 đầu) do aggregate mới — không phải lỗi, ghi để biết (`portal-home-rev48-en.jpg`) |
| PortalClaims / PortalClaimDetail / PortalNotifications (rev 48, en-US, đã hard-reload) | ❌ vẫn **tiếng Việt** (`Đang thẩm định`, `Đang xử lý chứng từ`, `Đã gửi`). Nguyên nhân: A2 bind `CustomerLabelVi` inline trên 3 màn, không qua `Portal_CustomerState` — A12.2 sửa action nhưng màn không gọi action (`portal-claims-rev48-still-vi.jpg`) |
| Tên khoản mục PortalClaimDetail ở en-US | ✅ vẫn tiếng Việt ("Tiền giường bệnh phòng tiêu chuẩn (2 ngày)", "Xét nghiệm công thức máu…") — ràng buộc build-06 §8.5 dòng 3 giữ |

**Còn 1 bước để đóng A12:** 1 lượt Mentor + 1 publish MyInsurance — đổi biểu thức chip inline trên 3 màn sang cùng nguồn locale (gọi `Portal_CustomerState(StatusId)` hoặc `If(Substr(GetCurrentLocale(),0,2)="en", …En, …Vi)` trên aggregate như PortalHome). Chưa chạy — vượt "2 app, 2 publish" đã duyệt.

---

## Bổ sung 20/09 ~23:45 — sau A13 (MyInsurance rev 49) — ❌ HỒI QUY

A13 (1 lượt Mentor, 0 lỗi validate, publish thành công): 4 màn gọi `Portal_CustomerState` qua helper — mỗi màn `OnAfterFetch` chạy ForEach gọi action từng dòng, ghi vào list cục bộ `StatusChips` (structure mới: key dòng + Label + Color) và **đổi source của list/table widget sang `StatusChips`**. Nút `VIBtn` Visible=False ✓ (DOM không còn nút "VI").

| Màn (en-US, hard-reload) | Kết quả |
|---|---|
| PortalHome | ❌ **Recent claims RỖNG** (0 CLM trong DOM); KPI "10 claims in progress" vẫn đúng (đọc aggregate) — `portal-home-rev49-EMPTY-LIST.jpg` |
| PortalClaims | ❌ **bảng RỖNG**, chỉ còn dòng info "Only four states…" — `portal-claims-rev49-EMPTY-TABLE.jpg` |
| PortalClaimDetail / PortalNotifications | chưa kiểm — cùng cơ chế helper, khả năng cao cùng lỗi |

Console: **0 lỗi JS**. ⇒ `StatusChips` không được nạp lúc render: handler `OnAfterFetch…` nhiều khả năng **không gắn vào sự kiện On After Fetch của aggregate** (hoặc ForEach append không chạy). Hai lựa chọn: (a) 1 lượt Mentor sửa — giữ aggregate làm source của list, `StatusChips` chỉ là list song song tra theo `ClaimId`, và gắn handler đúng sự kiện; (b) `deploy_rollback` MyInsurance về rev 48 (list chạy, 3 màn chip VI-only, nút VI còn).

---

## Bổ sung 20/09 ~23:40 — A13-fix (MyInsurance rev 50) — ✅ ĐẠT, đợt A ĐÓNG

Thiết kế theo PO: aggregate của 4 màn (đã join `ClaimStatus`) là source của list; chip text = `LocalizedStatusLabel(…CustomerLabelVi, …CustomerLabelEn)` (Client function, 2 in / 1 out, chỉ chứa `If(Substr(GetCurrentLocale(),0,2)="en", En, Vi)`); chip colour = `StatusColorToken(…Value)` (Client function, Switch Value→token, tồn tại một nơi); `Portal_CustomerState` thành wrapper mỏng gọi hai hàm; `StatusChips` list + structure + handler OnAfterFetch **xoá**; `VIBtn` Visible=False giữ. 0 lỗi validate. Ảnh: `../../../output/bnk-dev-myinsurance/shots-rev133/*-rev50-en.jpg` (thư mục đặt tên theo yêu cầu PO; rev thực = MyInsurance 50).

| Màn (en-US, hard-reload, đăng nhập lại) | Dòng | Chip | Tên khoản mục / cơ sở KCB |
|---|---|---|---|
| PortalHome | 10 CLM | `Submitted` ×8 · `Processing your documents` · `Under assessment` | 10× "Phòng khám Đa khoa / Bệnh viện Đa khoa…" — **VI giữ** |
| PortalClaims | 10 hàng | `Under assessment` · `Processing your documents` · `Submitted` ×8; màu **khác nhau theo trạng thái** (vàng / đỏ / vàng nhạt) | "Bệnh viện Đa khoa Quốc tế Vinmec Times City", "Phòng khám Đa khoa Quốc tế Victoria Healthcare" — **VI giữ** |
| PortalClaimDetail (17) | 1 | `Under assessment` | 4 khoản "Tiền giường bệnh phòng tiêu chuẩn…", "Xét nghiệm công thức máu…", "Siêu âm ổ bụng…", "Thuốc điều trị nội trú…" — **VI giữ** |
| PortalNotifications | 10 | `Under assessment` · `Processing your documents` · `Submitted` ×8 | — |
| Nút "VI" header | 0 phần tử trong DOM ✓ | | |

**Một điều lộ ra khi màu bắt đầu chạy đúng:** chip `Processing your documents` của CLM-0004 (`EXTRACTION_ERROR`) tô **đỏ** (`StatusColorToken` map EXTRACTION_ERROR → "error", thừa hưởng từ A2), trong khi CLM-0034… (`DOC_PROCESSING`) tô vàng nhạt. Mask của build-06 §3.3 che *chữ* nhưng **màu vẫn lộ** trạng thái lỗi. Sửa = 1 dòng trong `StatusColorToken` (EXTRACTION_ERROR → cùng token với DOC_PROCESSING). **Chưa sửa** — ghi cho việc 3 của `portal-bilingual-toggle` / đợt F, không mở lượt Mentor thứ ba.
