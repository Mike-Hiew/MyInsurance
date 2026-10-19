# Tổng kết phiên ngày 05/10/2026 (ClaimMVP, PO uỷ quyền tự publish đến 15:00 VN)

Phiên Mentor `fbd28ef5`, nạp từ rev 346 (rev 337–346 là UI v7 của phiên Cowork — không đè). Mọi lần publish: lỗi validation = baseline 7, kiểm `app_revisions` trước khi publish, kiểm lại trên màn hình sau publish.

## 1. Đã publish
| Rev | Nội dung | Kiểm trên màn |
|---|---|---|
| 347–348 | **AG-4 thay thế tín hiệu cũ khi chạy lại:** FraudSignal.LastRaisedOn; sau lần chạy AG-4 thành công, tín hiệu OPEN của AG-4 không được nêu lại → `SUPERSEDED` ("Đã thay thế", xám) + event `SIGNAL_SUPERSEDED`; tín hiệu CLEARED không bị đụng; cổng/Submit/Approve vẫn chỉ xét OPEN. (347 lọc sai do `Index()` 0-based → 348 sửa.) | ✅ 0444: PROVIDER_UNMATCHED cũ → Đã thay thế; To-do hết "điều kiện 4" |
| 348–349 | **Xem chứng từ gốc từ IDP:** màn `RawDocuments?ClaimId=` (link "Xem chứng từ gốc (IDP)" ở tab Chứng từ); IdpGetRawFile → cache `IdpRawFileCache`; giải ZIP ngay trên trình duyệt (DecompressionStream, không thư viện ngoài); chứng từ seed có mã giả → "Chứng từ mẫu — không có bản gốc trên IDP" | ✅ 0433: lần 1 tải từ IDP hiện ảnh bảng kê gốc; lần 2 lấy từ bộ nhớ đệm. ✅ 0444: nút khoá đúng |
| 349–354 | **P8 — màn Results & ROI** (`/ClaimMVP/ResultsRoi`, menu "Kết quả & ROI") — mọi số đọc từ dữ liệu thật, mỗi thẻ ghi n, n < 5 → "Chưa đủ mẫu"; lọc kỳ 7/30/90/Tất cả và phạm vi Tất cả / Bỏ seed / Chỉ IDP; AccuracyRun (đo từ dòng đã xác minh); bảng 5 tham số ROI có nhãn Giả định / Đo thật | ✅ Xem §2 |
| 355–356 | **Nhãn "Khớp một phần với danh mục"** (không tạo tín hiệu — theo PO): Claim.ProviderMatch* ghi khi AG-4 tra danh mục; chip trên ClaimDetail cạnh tên cơ sở | ✅ 0444: "Khớp một phần với danh mục: 01161 — BỆNH VIỆN ĐA KHOA QUỐC TẾ VINMEC" |
| 356–357 | **AgentMap:** thời lượng TB cho AG-2/AG-3/AG-4 từ DurationMs thật (kèm "n = k lượt có đo"); nhãn VI; đổi "AG-4 Gian lận" → "AG-4 Tín hiệu bất thường" | ✅ AG-2 11,8 s · AG-3 22,5 s · AG-4 13,9 s · AG-0 4,2 s |
| 358–361 | **ClaimDetail (trước đây tưởng phải dùng Studio):** "Hiệu lực từ" = ngày hiệu lực của thành viên + "Hợp đồng từ …"; ảnh chứng từ dựng lại hiện **giá trị gốc** của dòng đã sửa; nút Administration "Bổ sung giá trị gốc cho dòng đã sửa" (đã chạy: 1 dòng) | ✅ 0445 "14/09/2026 · Hợp đồng từ 01/01/2026"; ✅ 0444 dòng 3 hiện "Paracetamol 500mg" |
| 362–365 | ResultsRoi: độ chính xác = dòng đã xác minh không có ClaimLineEdit; thông báo nút và câu "Chưa đủ tham số… Thiếu: …" theo ngôn ngữ, liệt kê đủ tham số thiếu | ✅ 1/1 (100%); "Thiếu: Phút xử lý trên Claim Console/hồ sơ, Chi phí giờ cán bộ, Phút xử lý tay/hồ sơ" |

## 2. Số thật trên Results & ROI (30 ngày, Tất cả hồ sơ, ~11:20 VN)
- 31 hồ sơ qua cổng, **0 tự duyệt** — C2 trượt 31/31 (thẩm định lâm sàng PARTIAL), C4 26, C3 11, C1 7. ⇒ Muốn demo có tự duyệt cần hồ sơ có thẩm định lâm sàng sạch.
- 2 hồ sơ có quyết định trong kỳ → "Chưa đủ mẫu" cho thời gian xử lý và tiền.
- 13 chứng từ đọc bằng IDP, độ tin cậy TB 68,3%; WARN đối chiếu 5 (R1 1, R2 1, R3 1, R5 2); tín hiệu AG-4: 5 mở, 1 đã xử lý, 1 thay thế; 462 lượt agent, 11 lỗi (2,4%).
- ROI: chỉ 5 tham số chuẩn — 990 hồ sơ/năm và 4,3 tỷ ₫/năm (BRD §14, gắn "Giả định"); 3 tham số còn 0 nên hiện "Chưa đủ tham số": phút xử lý tay/hồ sơ, phút xử lý trên Claim Console/hồ sơ (bấm giờ 5 hồ sơ), chi phí giờ cán bộ.
- Độ chính xác bóc tách: 1/1 dòng đã xác minh không phải sửa (rev 362 sửa cách tính; lần đo 11:15 ghi "0/1" là do lỗi cách tính cũ — lần đo đúng là 12:44 "100% · 1/1").
- IDP compare (AccuracyRun bằng `POST /request/compare`) chưa chạy — cần bộ ground truth XLSX 10–20 chứng từ ẩn danh.
- **Phát hiện quan trọng cho demo:** C2 trượt trên mọi hồ sơ vì AG-3 chỉ cho COMPLETE khi có kết quả thẩm định IDP (flow 28/8), mà Site Setting `ClinicalUseIdpAssessment` = False (tắt để giữ quota) → luôn PARTIAL → **kịch bản A (tự duyệt → Paid) hiện không thể xảy ra**. Cần PO chọn: bật setting và dùng hồ sơ có OCR thật, hoặc cho phép AG-3 kết luận COMPLETE khi đã đánh giá đủ dòng mà không có đề xuất.

## 3. Sự cố trong phiên (đã xử lý)
- Mentor tự bịa 10 tham số ROI (có dòng gắn "Đo thật" sai, có chữ "gian lận") khi bấm "Tạo tham số mặc định" ở rev 353 → rev 354 xoá hết, chỉ tạo đúng 5 mã chuẩn; đã chạy lại trên màn.
- Ngôn ngữ trên ResultsRoi bị đảo (rev 351) → sửa ở 352–353.
- Các nút trên ResultsRoi / Administration không ăn khi bấm qua công cụ tự động (click toạ độ), chỉ ăn với `button.click()` — **PO bấm chuột thật một lần để xác nhận**.

## 4. Việc PO cần làm / quyết định
1. **Test Decline 0445** bằng tài khoản người duyệt khác Mai Trung Hieu (Approve phải bị chặn → Decline WAIT → kiểm thư DECLINE của AG-5 → Send).
2. **Bấm giờ 5 hồ sơ** (tay và trên Claim Console) + nhập chi phí giờ cán bộ vào bảng tham số ROI, tick "Đo thật" cho số đo.
3. Ground truth XLSX cho IDP compare (nếu muốn có độ chính xác theo trường của IDP).
4. **Chạy lại T-GATE + chuỗi** (đã đổi trạng thái tín hiệu AG-4 — cổng chỉ xét OPEN, không đổi công thức).
5. Chưa làm (cần PO quyết): ghi WAIT lên từng ClaimLine (chạm đầu ra của chuỗi); kiểm IsPendingInfo với nhãn "Pending info" (MyInsurance — phiên Cowork).

## 4b. Chiều 05/10 — bật thẩm định IDP thật (rev 366–376)
- PO yêu cầu hệ thống chạy real → bật `ClinicalUseIdpAssessment`. Luồng thẩm định IDP (flow 28/8) chưa từng chạy thật trong ClaimMVP; sửa 6 lỗi theo phản hồi thật của IDP (dạng body, mảng ocrRequestIds, khoá workflowRequestId, mã 4005 "đang chạy" là thành công, độ dài trường Assessment, trạng thái kẹt RUNNING).
- ✅ 0433 (14:56): IDP thẩm định xong → AG-3 đánh giá đủ 3 dòng → **điều kiện C2 đạt lần đầu**.
- Còn chặn tự duyệt cho hồ sơ thật: ConditionClass của hồ sơ portal (C3), ảnh bảng kê IDP đọc đúng số tiền (×1000 → C4), tên cơ sở trong danh mục, demo ngoại trú.

## 4c. Chiều tối 05/10 — hồ sơ thật chạy tự động tới Paid (rev 377–389, PO gia hạn tự publish tới 21:00)
- Tự gán nhóm bệnh (ACC/GEN; thai sản, bệnh đặc biệt, thiếu ICD → cán bộ); danh sách bệnh đặc biệt A5 §5.
- Chứng từ thử theo mẫu chuẩn IDP (bảng kê 13 cột + phiếu khám) → IDP đọc đúng số tiền (hết ×1000).
- Sửa 12 lỗi chỉ lộ ra với hồ sơ thật: ngày điều trị portal, RuleSetRef, nhóm dịch vụ dòng, NormalizeFacilityName bỏ dấu sai, Index() 0-based, nhánh tự duyệt thiếu thư + actor, ngữ cảnh thư AG-5 (số theo dòng, lặp dòng, đồng chi trả, boolean JSON)…
- ✅ **CLM-2026-0447** tự duyệt → Paid 320.000 ₫ (16:43, sau các lần sửa); thư RESULT đúng, đang DRAFT.
- ✅ **CLM-2026-0448** tự động hoàn toàn: nộp 17:38:52 → Paid 17:42:45 → thư AG-5 17:43.
- PO cần: Send thư 447/448/449 sau khi đọc; quyết định xử lý 0450 (trùng, đã Paid giả lập do lỗi) và gỡ CROSSCHECK_FAIL giả trên 0447/0448; quyết định có làm tín hiệu đối chiếu chéo tất định (không qua LLM) không.

## 4d. Tối 05/10 (PO gia hạn tự publish đến 09:00 sáng 06/10)
- **Rev 390 (19:50 VN):** R1/R3 so tên đã chuẩn hoá cả hai phía (trước so với tên thành viên chưa chuẩn hoá → WARN giả "NGUYỄN VĂN AN | NGUYỄN VĂN AN"); R3 không dùng lại biến của R1; R7 `Substr(…, 0, 500)` (trước bắt đầu từ 1 → "aracetamol"). ✅ Chạy lại đối chiếu 0448 (lần #2): R1, R3, R4, R5, R7, R8 PASS; R9 COMPLETE; R2/R6 SKIPPED (không có ngày sinh / XML).
- **Rev 391 (19:54 VN):** xoá 2 log debug LogR1Debug/LogR3Debug (ghi tên thành viên vào log). Validation 7.
- AG-4 "dispatch 2 lần": Mentor kiểm — guard đã có; lần nêu rồi thay thế tín hiệu trên 0448 là do R3 WARN giả. ✅ Xác nhận trên hồ sơ mới: chỉ 1 DispatchFraudCheck.
- ✅ **CLM-2026-0449** (nộp qua MyInsurance 19:58:20, chứng từ mẫu IDP ngày 02/10) → IDP đọc → AG-4 1 lượt, không tín hiệu → AG-3 + thẩm định IDP, không đề xuất → GEN tự gán (J02.9) → chuỗi run #1 320.000 ₫ → cổng 4/4 → **tự duyệt → Paid 20:02** (PAY-CLM-2026-0449-202610052002) → thư RESULT AG-5 20:03 (DRAFT). Hồ sơ thật thứ ba tự động hoàn toàn, ~4 phút.
- Nhỏ: trace hiện AG-4 dưới nhãn "Automated check", callback AG-5 dưới nhãn "Document reading … → Officer" (mỹ quan).

## 4e. Tối 05/10 (20:00–21:00) — kịch bản C thật lộ lỗi nghiêm trọng, đã sửa (rev 392–395)
- **⛔ Lỗi nghiêm trọng (có từ rev 348):** nộp **CLM-2026-0450** trùng người/cơ sở/ngày 02/10 với 0449 → AG-4 nêu đúng **DUPLICATE (HIGH)** lúc 20:10 nhưng cùng giây bị **SUPERSEDED** → cổng C4 đạt → **tự duyệt, Paid 320.000 ₫** (lệnh chi giả lập; thư kết quả chỉ DRAFT). Nguyên nhân: RaiseFraudSignal gán `LastRaisedOn` SAU DoCreate/DoUpdate nên không bao giờ lưu → bước "thay thế tín hiệu cũ" coi mọi tín hiệu của chính lượt đang chạy là cũ. Tín hiệu CROSSCHECK_FAIL trên 0448 (và 0447) bị "thay thế" chiều nay cũng do lỗi này — không phải AG-4 chạy 2 lần như Mentor nói.
- **Rev 392:** mã lượt chạy `Claim.FraudRunKey` + `FraudSignal.LastRaisedRunKey`; RaiseFraudSignal gán LastRaisedOn + LastRaisedRunKey TRƯỚC khi ghi (cả tạo mới và cập nhật); bước thay thế chỉ chạm tín hiệu OPEN cùng ClaimId có run key khác lượt hiện tại VÀ LastRaisedOn < giờ bắt đầu lượt. Kèm: nhãn trace AgentTrace (AG-4/AG-5), log IDP chỉ còn "HTTP <status>", bộ đếm danh sách bệnh đặc biệt (aggregate Count luôn 1 dòng → đọc giá trị Count: **55 dòng** ✅).
- **Hồi quy rev 392 → sửa rev 393:** ExecuteFraudRun ghi Claim.FraudRunKey rồi gọi agent cùng transaction → **55P03 lock timeout** (CLM-2026-0451: AG-4 AGENT_ERROR → cổng không đạt C4 → chuyển cán bộ — cơ chế an toàn đúng). Chuyển việc sinh/lưu run key sang PrepareFraudRun (timer riêng, đã commit); ExecuteFraudRun chỉ đọc trước khi gọi agent.
- ✅ **Kịch bản C thật (rev 393, 20:31):** chạy lại AG-4 trên 0451 → DUPLICATE (HIGH) giữ **OPEN** → "Bị chặn — tín hiệu bất thường", điều kiện 4 ✕, chuyển cán bộ, không lệnh chi.
- **Sửa dữ liệu:** nút Administration "Mở lại tín hiệu bị thay thế nhầm (lỗi 05/10)" (chỉ tín hiệu bị thay thế < 10 phút sau khi nêu; không đổi trạng thái/tiền) → **mở lại 3 tín hiệu trên 0447, 0448, 0450** (event SIGNAL_REOPENED). 0447/0448: CROSSCHECK_FAIL từ R3 giả — cán bộ Gỡ kèm lý do; 0450: DUPLICATE thật trên hồ sơ đã Paid (giả lập) — PO quyết xử lý.
- **Rev 394:** nhãn trace ClaimDetail "Anomaly signals (AG-4)" / "Tín hiệu bất thường (AG-4)" ✅; thông báo nút sửa dữ liệu đếm đúng số hồ sơ.
- **Rev 395 — ResultsRoi:** STP đọc sai trường (DecisionComment) → nay **7,5% · 4 tự duyệt / 53 qua cổng** ✅; Tiền đã chi **8.560.000 ₫** ✅ (4×320.000 + 1.880.000 + 5.400.000). **Rev 396:** "Yêu cầu" = tổng số tiền dòng của các hồ sơ đã chi → **9.000.000 ₫ / đã chi 8.560.000 ₫ / khách tự chịu 440.000 ₫** ✅ (4×80.000 + 120.000); thời gian xử lý gồm cả hồ sơ tự duyệt (n = 6: tự động 4, người 2). Thử đổi sang trung vị thật thì Mentor lặp vòng > 15 phút → huỷ, đóng phiên fbd28ef5, mở phiên mới **c7694f61** nạp rev 396; **rev 397** chỉ đổi nhãn cho trung thực: "Thời gian xử lý (trung bình)" (giá trị là trung bình: tự động 22 phút do 0447 mất 75 phút khi đang sửa lỗi; trung vị thật ~4 phút). Smoke test rev 397: Dashboard, OpsQueue, AgentMap, CrossCheck 0448, ResultsRoi chạy; log ClaimMVP không có Error từ rev 393.
- **Bài học:** test âm (hồ sơ trùng) phải chạy sau mỗi thay đổi đụng tín hiệu/cổng; 3 hồ sơ "tự động đến Paid" trước đó không phát hiện được lỗi này vì chúng không có tín hiệu thật.

## 4f. Tối 05/10 (21:15–21:50) — hoàn thiện phần còn lại (rev 398–401, phiên Mentor c7694f61)
- ✅ **Test hồi quy đường cập nhật tín hiệu:** chạy lại AG-4 trên 0451 (21:21) → DUPLICATE (HIGH) được nêu lại (cập nhật cùng loại) và **giữ OPEN**, không bị thay thế.
- ✅ **Bộ test sau thay đổi tín hiệu/cổng (21:24, rev 397):** chuỗi **46/46 pass**, cổng **5/5 pass**.
- **Rev 398 — thẻ kịch bản demo U16 gắn vào ClaimMVP** (LayoutSideMenu → OnReady, JS node Lang = Client.Lang; CSS cuối theme) — đúng code của MyInsurance, dùng chung trạng thái localStorage. ✅ `?demo=1` hiện thẻ (EN + VI đúng dấu), bước 3 "Hồ sơ vào hàng đợi cán bộ" ở WorkList; `?demo=0` tắt hẳn (đã tắt lại sau khi thử).
- **Rev 399 — hiển thị:** tuổi thành viên hiện "—" khi không có ngày sinh (hết "126 y" trên 0445). Bảng "Cost lines out of cover" trước hiện PENDING cho mọi dòng chuỗi đã xử lý (chuỗi không ghi ClaimLine.Decision — việc ghi là thay đổi đầu ra chuỗi, chờ PO) → sửa **chỉ ở hiển thị**:
- **Rev 400:** chuỗi từ chối toàn hồ sơ → "Declined by chain — Kiểm tra kỳ chờ" ✅ 0445.
- **Rev 401:** chuỗi đã chi → "Covered" / "Partial · Paid: x of y" theo ClaimLine.AmountPayable của lần chạy mới nhất ✅ 0449 (3 dòng Partial do đồng chi trả 20%: 160.000/200.000, 120.000/150.000, 40.000/50.000), ✅ 0440 (Covered, Partial giường 1.500.000/1.540.000, NOTCOV Omega-3). Không ghi gì vào DB.

## 4g. Tối 05/10 — Mockup v0.7 khớp chức năng ClaimMVP thật (không đụng app)
- File mới **`E:\Hackaton\Prototype_Insurance_BnK_v0.7.html`** (v0.6 giữ nguyên). Chỉ cập nhật các màn chính của cán bộ; màn bàn giao kỹ thuật/trình bày giữ như v0.6 theo yêu cầu PO.
- **Đánh số agent theo hệ thống thật:** AG-0 Điều phối · AG-1 Tiếp nhận (BnK IDP) · AG-2 Chuỗi 14 bước · AG-3 Lâm sàng · AG-4 Tín hiệu bất thường · AG-5 Thư khách hàng. Thẻ "AI đã làm gì" có 6 bước; Bản đồ agent và Nhật ký agent viết lại theo số mới.
- **Hai hồ sơ mẫu mới:** E CLM-26-000425 (trùng với 000418 → AG-4 DUPLICATE HIGH mở, PROVIDER_UNMATCHED của lượt trước SUPERSEDED, bị chặn tự duyệt) và F CLM-26-000431 (thiếu Phiếu khám bệnh → Chờ bổ sung, thư ADDDOCS đã gửi).
- **Chức năng thể hiện trên mockup:**
  - gỡ tín hiệu (lý do ≥ 10 ký tự) → cổng 4/4 → tự duyệt; đề nghị khách giải trình;
  - đối chiếu R1–R9 (PASS / WARN / SKIPPED / INCOMPLETE) + nút chạy lại;
  - xem chứng từ gốc từ bản lưu IDP;
  - xác minh từng dòng độ tin cậy thấp; sửa dòng (chỉ trường chữ, giữ giá trị gốc, bắt buộc lý do, có lịch sử);
  - thư AG-5 RESULT / DECLINE / ADDDOCS (nháp → sửa → soạn lại → gửi);
  - mô phỏng khách bổ sung → tự duyệt;
  - phê duyệt 4 mắt: Duyệt / Từ chối theo mã (WAIT, NOTCOV, NOTREL, FRAUD, NOT_ELIGIBLE, DOCS, có xem trước câu khách đọc) / Trả lại. Hồ sơ D không duyệt chi trả được;
  - hàng đợi vận hành: Thử lại / Xử lý / Bỏ qua kèm lý do;
  - dải "Cần bạn chú ý" trên Bảng điều khiển; chip lọc ở Hàng đợi;
  - Story mode thêm bước "AG-4 chặn hồ sơ trùng".
- **Đã kiểm (Playwright):**
  - 6 hồ sơ × 8 tab × VI/EN và các màn danh sách: không lỗi JS; không có "gian lận", "undefined" hay "NaN" trên màn.
  - Các luồng E→Paid, F→Paid, C xác minh → gửi → trả lại, A duyệt → chi, D từ chối WAIT → gửi thư → đóng đều đúng trạng thái.
- Còn mở: kiểm Dashboard rev 402 (đếm tự duyệt hôm nay, không trùng dòng ở "Hàng đợi của tôi") — chưa xác minh trên màn.

## 5. Trạng thái (~21:50 VN)
- ClaimMVP **rev 401** (21:47 VN); agent AG-4 rev 6, AG-5 rev 5 (không đổi hôm nay). Mentor session ClaimMVP: `c7694f61` (fbd28ef5 đã đóng). Tự publish được đến 09:00 VN 06/10.
- Smoke test: rev 365 — ResultsRoi, Dashboard, ClaimDetail 0445; rev 359–362 — ClaimDetail 0444, CrossCheck 0433, OpsQueue; rev 357 — AgentMap; rev 349 — RawDocuments. Đều chạy; log ClaimMVP không có Error từ 09:30 VN.

## 6. Lưu ý
- Hai phiên (ClaimMVP và Cowork) cùng sửa ClaimMVP hôm nay → phiên nào cũng phải `app_revisions` và nạp lại trước khi publish.
- Mỗi lần publish trang đang mở tự nạp lại → tải lại trang trước khi demo.
