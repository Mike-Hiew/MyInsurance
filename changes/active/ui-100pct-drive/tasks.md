# Change — Đưa hai app lên 100 % Market Leading (audit runtime UI)

**Trạng thái:** 🟡 code đã xong (20/09), **ghi hồi tố** — change này được lập sau khi code đã chạy, vi phạm luật "change trước code" của `README.md` §1. Ghi lại để không lặp lại tình trạng "delta sống ngoài spec".
**Rev:** ClaimMVP 96 → **128** · MyInsurance 15 → **46**
**Bằng chứng:** `../../../output/bnk-dev-claimmvp/runtime-audit-rev114.md` (audit #11, **100 %**), `../../../output/bnk-dev-myinsurance/runtime-audit-rev37.md` (audit #3, **100 %**), `shots-rev113/`, `shots-rev36/`
**Spec liên quan:** delta ở `./specs/data-model/spec.md` (quy tắc `ClaimNo`) và `./specs/screens-wording/spec.md` (nhãn trạng thái console, trạng thái màn)
**Mentor session:** console `9b488a41-fc3d-458d-b704-4db36910d566` · portal `90dc37fd-7b73-4991-b121-2c51c80d5673`

## Phạm vi

Chỉ UI/UX + logic màn hình + hai server action phụ trợ. **Không** chạm trường tiền, sổ hạn mức, trạng thái hồ sơ, `RawExtractionJson`, Entity. Mentor không sinh Entity, không sinh logic tiền (đúng `README.md` §4).

## Việc

### A. Theme token (cả 2 app) — rev 96–102 / 15–22
- [x] Radius 4/8/12/16/999 · `--cc-dur:.18s` · `--cc-ease` · màu muted/faint/link/ok/warn/danger · lưới 4 pt 12/16/20 · palette alert mềm
- [x] Probe: grid 21 → 100 %, contrast fail 57/9/16 → 0 (ClaimDetail/WorkList/Dashboard), 0 trên 7 màn portal

### B. Trạng thái component (cả 2 app) — rev 103–105 / 23–25
- [x] `.btn` hover/active/disabled/`is-loading` · focus ring `#35D3A3` · `.not-valid` / `.cc-field-ok` · `.cc-skel` · `.cc-empty` · `.cc-toast` · `cc-rise`/`cc-pop` · `.cc-done-mark` · `prefers-reduced-motion`

### C. Console WorkList / ClaimDetail — rev 106–114
- [x] Dropdown trạng thái hiện nhãn người đọc (Received / Extracting / Extraction error / Pending approval / Under review / Approved / Declined / Paid)
- [x] Chip strip đếm live — **Server Action `WorkList_StatusCounts`** trả đúng một dòng cho mỗi `ClaimStatus` (aggregate client không group được; truy cập list theo vị trí làm màn crash `OS-CLRT-60500` ở rev 108–109, sửa rev 110)
- [x] Skeleton 5 dòng + chip skeleton · `Filter` `is-loading` · empty state thay bảng + "Clear filters"
- [x] Sub-header nói thật: "All claims · …", "N claims · all handlers" (trước ghi "assigned to me" mà không có filter)
- [x] **`TakeUnassigned`** trả đúng số đã gán + ids · **`UndoTakeUnassigned`** gỡ đúng các id đó · `HideToastLater` 6 s · toast "No unassigned claims right now" khi N = 0
- [x] Card "Proposal pending" (`cc-pcard`) hết trắng-trên-trắng → card `#0C3036`
- [x] `PageSizeInput` có `aria-label` · chip CSS trên lưới 4 pt · radius ProgressBar

### D. Portal — rev 26–38
- [x] PortalNewClaim: Label widget cho 7/7 input · `ValidateField` on-blur + "Looks good" · Continue gated + `is-loading` · **Step4Card** "Claim received" (`pt-done`, `cc-done-mark`) tự chuyển sau xử lý · context strip `pt-ctx` · hiện `ClaimNo` đã lưu
- [x] PortalHome/Claims/Notifications: skeleton + empty state có CTA · KPI card bấm được (`pt-kpi-link` overlay)
- [x] PortalSupplement: Label · Send `is-loading` → "Sent — the assessor has been notified" · `cc-back`
- [x] PortalBenefits: ProgressBar theo brand (`is-warn` ≥ 80 %, `is-full`) + "used of limit" bằng `FormatVND`
- [x] `cc-back` trên mọi màn không phải Home · PortalClaimDetail "Updated HH:mm · Refresh" + toast
- [x] Phone 390 px: header "+" 44 px, `pt-row` wrap

### E. Dữ liệu — rev 115–119
- [x] `Portal_SubmitClaim2` lưu `ClaimNo = "CLM-" + năm + "-" + Id đệm 4` và trả `ClaimNo` (trước đây 4 hồ sơ portal cùng nhận "CLM-2026-0002")
- [x] Timer `FixDuplicateClaimNos` (chạy khi publish): chỉ đổi số hồ sơ **trùng** và không phải Id thấp nhất
- [x] **Sự cố:** rev 115/116/118 đổi số **mọi** hồ sơ (aggregate trùng không lọc) → demo 17–20 thành 0017–0020. Rev 119 restore về CLM-2026-0001…0004 và lọc đúng `ClaimNo = C.ClaimNo AND Id < C.Id`. Bằng chứng: log category `FixClaimNos` ("skip Id 17 count=0" … "done"), `shots-rev113/worklist-final-rev119.jpg`
- [x] **PO quyết (20/09 tối): giữ hồ sơ test Id 34–41**; gỡ timer `FixDuplicateClaimNos` + `Timer_SeedBatch0` và 2 action riêng của chúng (`FixDuplicateClaimNos`, `Timer_SeedBatch0_Action`, IsReferenced=False) — ClaimMVP rev 128. Các action seed khác (`RunSeedBatch0Now`, `Seed_AssessmentAndTerms`…) giữ nguyên. Kiểm sau publish: WorkList vẫn 12 hồ sơ CLM-2026-0001…0004, 0034…0041

### E2. Content pass sau khi PO xem (20/09 chiều) — ClaimMVP 120–125 · MyInsurance 39–45
- [x] Portal: bỏ mã màn `S07…S12` (`cc-fc`) khỏi menu khách; PortalHome đọc PolicyBenefitSnapshot (KPI ngoại trú 1.000.000/3.000.000, Main benefits OUTPATIENT/INPATIENT_ROOM/DENTAL, tên không mã); KPI 3 = tổng hồ sơ mở + "n under assessment · m processing"; "—" thay "0 ₫" cho hồ sơ chưa quyết; PortalClaimDetail theo trạng thái (Amount you claimed / Not yet / What happens next; ẩn Why-card, Download letter, Item by item khi chưa quyết; bước "Documents read" chỉ ✓ khi qua Extracting; "Updated HH:mm Refresh" một dòng); PortalClaims panel tiến trình bind hồ sơ mới nhất + câu cho khách; PortalSupplement khối "needs more documents" có điều kiện + separator; PortalBenefits tên quyền lợi không mã enum; Notifications tag "Policy"
- [x] Console: Policies (snapshot date "—", bảng quyền lợi 5 dòng, tên người được BH), Checkpoints (code/title 2 dòng, hết trùng), TermRegister (điều khoản xuống dòng), Dashboard ("no auto-approvals yet"), ClaimDetail null-safe (—) + Diagnosis đúng binding + Provider tab General
- [x] Dữ liệu: Portal_SubmitClaim2 lưu TotalClaimed/EventDate/ProviderName (đã kiểm Id 38–39); backfill ngày cho 34–38
- [x] AdmissionDate/DischargeDate: log "SC2-ENTRY adm=1900-01-01" chứng minh **portal** gửi null — OnSubmitClaim map sai biến (sửa rev 45: AdmissionDate ← FTreatedFrom, DischargeDate ← FTreatedTo); ClaimMVP rev 125 gán 2 thuộc tính trước CreateClaim + LogMessage "SubmitClaim". Kiểm Id 41: 08/09–09/09/2026, 640.000 ₫ ✓
- [x] **PO báo 3 lỗi (20/09 tối)** — (1) WorkList chip trạng thái không bấm được bằng chuột: `FetchStatusCounts` nằm trong **OnRender** → gán `StatusCounts` → render lại → OnRender… ~40 lần/giây, `ChipList` bị huỷ/dựng lại giữa mousedown và mouseup (MutationObserver: 3.744 mutation/4 s; JS `.click()` vẫn chạy nên audit không thấy). Chuyển sang OnReady (ClaimMVP rev 126) → 0 mutation, click thật Received → 8 dòng, Paid → 1 dòng ✓. (2) Menu console chữ xanh `rgb(26,99,214)` do rule Batch A `.layout a{color:var(--cc-link)!important}` → thêm 4 rule override cho `.app-menu-links a` (rev 127) ✓. (3) Portal menu 2 mục active trên PortalClaims: OS UI `SetActiveMenuItems` so khớp URL theo **substring** nên "PortalClaim" khớp cả PortalNewClaim lẫn PortalClaims → so sánh chính xác từng link (MyInsurance rev 46); kiểm 6 màn, mỗi màn đúng 1 active ✓
- [x] Mã F-xx trên menu/tab console: PO quyết **ẩn** → `.cc-fc,.cc-fc-tab{display:none!important}` (rev 128; widget giữ nguyên để truy vết, 6 badge menu + 14 badge tab đều width 0)
- [x] Hồ sơ test Id 38–41: giữ (cùng quyết định với 34–37)

### F. Đóng
- [x] Audit #11 console + audit #3 portal, desktop + phone (iframe 390×844)
- [x] Cập nhật `../../../CLAUDE.md` §1 và `../../project.md`
- [ ] Merge delta vào Project doc `build-06` (nhãn trạng thái, trạng thái màn) và `build-01` (quy tắc `ClaimNo`) → `mv` sang `../../archive/`

## Xong nghĩa là

- Hai audit 100 % có screenshot đối chiếu — **đã đạt**
- Quy tắc `ClaimNo` nằm trong spec, không chỉ trong code — delta ở `./specs/data-model/spec.md`
- Không còn hồ sơ demo bị đổi số — **đã kiểm rev 119**

## Bài học (bổ sung vào `archive/README.md` khi đóng)

1. **Mentor nuốt prompt khi run trước còn "working"** — kể cả sau `publishSucceeded` nó còn "verify" vài phút. `attemptedChange:false` + không event = bị nuốt. Luôn poll tới `succeeded`.
2. **Label widget** sinh kèm text con "label" và `Mandatory=True` tự thêm " *" → bỏ cả hai. Text widget đôi khi nhận **tên** của nó làm Value hoặc giữ nguyên dấu ngoặc kép.
3. **Aggregate trên màn không group được** → đếm theo nhóm phải làm ở Server Action và trả list cố định (một dòng/giá trị enum). Không bao giờ đọc list theo vị trí.
4. **Một timer "sửa dữ liệu" phải có LogMessage** trước/sau mỗi update — không có log thì không biết nó đã làm gì; ba rev liền không thấy vì thế.
5. **CloudFront WAF chặn prompt CSS > ~1450 ký tự** hoặc có `[class*=…]` → chia chunk, bắt Mentor đọc lại số ký tự.
6. **OS ProgressBar ghi `--progress-color` inline** → override theme cần `!important`.
7. **Không bao giờ gọi fetch/gán biến trong OnRender** — tạo vòng render vô hạn; widget List bị dựng lại liên tục nên click chuột thật không ăn dù `.click()` bằng JS vẫn chạy. Audit phải có bước đếm mutation (MutationObserver 2–4 s phải ≈ 0 khi màn đứng yên) và **click chuột thật**, không chỉ JS.
8. **`SetActiveMenuItems` của OS UI so URL theo substring** → tên màn lồng nhau (PortalClaims / PortalNewClaim) làm 2 mục active. Đặt active bằng so sánh chính xác.
