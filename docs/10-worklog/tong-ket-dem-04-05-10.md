# Tổng kết phiên đêm 04→05/10/2026 (ClaimMVP, tự publish theo uỷ quyền PO đến 9:00)

## 1. Đã publish
| App | Revision | Nội dung |
|---|---|---|
| ClaimMVP | 309–320 | (đầu đêm) P6a sửa dòng + lịch sử, P6b đối chiếu R1–R9, P6c OpsQueue, LookupProvider, Dashboard bỏ số giả, AgentMap |
| ClaimMVP | 321 | Seed scenario W (thời gian chờ, BRD D) |
| ClaimMVP | 322–324 | Cổng tự duyệt: điều kiện 1 trượt khi chuỗi từ chối toàn hồ sơ hoặc payable = 0; mục To-do "Chain proposes declining the whole claim" |
| ClaimMVP | 325 | ApproveClaim chặn duyệt hồ sơ bị chuỗi từ chối toàn bộ |
| ClaimMVP | 326 | Màn ProviderLookup (/ClaimMVP/ProviderLookup); sửa ghi chú sai ở Administration > Providers |
| ClaimMVP | 327 | Sửa IdpSearchHospital — trước đây KHÔNG BAO GIỜ đọc được kết quả danh mục bệnh viện |
| ClaimMVP | 328–330 | Luật khớp tên cơ sở tất định (NormalizeFacilityName + MatchFacilityName): CATALOGUE / CATALOGUE_PARTIAL / NOT_FOUND |
| ClaimMVP | 331–332 | Dashboard My queue bỏ hồ sơ đã Paid/Closed/Declined/… |
| ClaimMVP | 333 | Agent_GetLetterContext: hasClauseRef; DECLINE/WAIT có waitingPeriodDays + waitingEligibleFrom |
| ClaimMVP | 334–336 | Thử thời lượng AgentMap → đã trả về như rev 333 (không thay đổi hiển thị) |
| CLM_FraudSignalAgent (AG-4) | 6 | CATALOGUE_PARTIAL → không nêu PROVIDER_UNMATCHED |
| CLM_CommsAgent (AG-5) | 5 | clauseRef rỗng → "theo quy tắc bảo hiểm của sản phẩm"; thư DECLINE thêm câu thời gian chờ |

Mọi lần publish: lỗi validation = baseline (ClaimMVP 7, agent 2), kiểm lại bằng màn hình sau publish.

## 2. Kết quả kiểm thử chính
- **CLM-2026-0445 (seed W):** B02 FAIL "[BR-WAIT-5] Kỳ chờ 30 ngày… đủ điều kiện từ 14/10/2026; điều trị 28/09/2026"; B03–B13 bỏ qua; payable 0; cổng: điều kiện 1 ✕ "Chuỗi tính từ chối toàn hồ sơ ở bước Kiểm tra kỳ chờ — quyết định từ chối luôn cần người duyệt". Claude (cán bộ = Mai Trung Hieu) đã xác nhận đề xuất AG-3 dòng Omega-3, clear PROVIDER_UNMATCHED (lý do ghi rõ), Submit → **Pending approval**.
- **Danh mục bệnh viện:** "Vinmec" → 7 kết quả; "Bệnh viện Đa khoa Quốc tế Vinmec Times City" → 01161 CATALOGUE_PARTIAL; "Phòng khám Fixture" → không khớp. AG-4 chạy lại trên 0444 không nêu PROVIDER_UNMATCHED mới.
- **Dashboard My queue:** hiện 0001, 0004, 0005, 0431…; không còn Paid.

## 3. Việc PO cần làm / quyết định
1. **Test Decline 0445** bằng tài khoản người duyệt KHÁC Mai Trung Hieu: bấm Approve trước (phải bị chặn "Chuỗi tính đã từ chối toàn hồ sơ…"), rồi Decline mã WAIT + bình luận ≥ 10 ký tự → kiểm event DECLINED, bản nháp thư DECLINE của AG-5 (kỳ vọng có câu "thời gian chờ 30 ngày… từ ngày 14/10/2026", căn cứ "theo quy tắc bảo hiểm của sản phẩm") → Send nếu đạt.
2. **Tín hiệu AG-4 cũ không tự đóng** khi chạy lại (vd PROVIDER_UNMATCHED MEDIUM cũ trên 0444 vẫn OPEN): cán bộ Clear tay, hay thêm cơ chế "thay thế khi AG-4 chạy lại"?
3. **CATALOGUE_PARTIAL không nêu tín hiệu** — PO có muốn nêu LOW để cán bộ thấy "khớp một phần" không?
4. ~~ClaimDetail cần ODC Studio (Mentor không mở được màn này)~~ — sáng 05/10 Mentor đã sửa được ClaimDetail (xem §5); khung Clinical assessment đã hết bị bóp. Còn: ảnh chứng từ gốc nên hiện giá trị gốc khi dòng đã sửa; "Cover from" hiện ngày hợp đồng thay vì ngày hiệu lực thành viên.
5. Xem ảnh gốc qua IdpGetRawFile (gọi IDP live, ZIP) — chưa làm.
6. P8 Results & ROI / AccuracyRun — chưa làm (cần số đo thật).

## 4. Lưu ý vận hành
- Mỗi lần publish, trang đang mở tự nạp phiên bản mới → tải lại trang trước khi demo.
- Từ 9:00 sáng 05/10 quay lại quy tắc cũ: hỏi PO trước mỗi lần publish.

## 5. Sáng 05/10 (05:30–07:05) — UI v7 cho ClaimMVP (phiên Cowork, theo yêu cầu PO kiểm & enhance UI)
| Rev | Nội dung |
|---|---|
| 337–338 | **D3 Biểu đồ thác tiền** — Block `V7PayoutWaterfall` (data action GetWaterfall đọc CalcSnapshot của ChainRun mới nhất, chỉ các bước có Delta ≠ 0) chèn đầu tab Calculation & ledger: cột xám Khách yêu cầu → cột đỏ từng bước trừ tiền → cột xanh Chi trả + thẻ lý do (Explain của bước). Rev 338 sửa style nằm nhầm trong Style Classes + wrapper List phá flex. |
| 339 | Chip delta "−80.000 ₫ so với lần tính trước"; sửa "latest run changed payable by —" (formatter in "—" khi âm); bỏ "(F58)"; chip "Đọc bởi BnK IDP" cạnh Extracted data; CSS token `--ai*`, `.v7-ai-chip`. |
| 340–343 | **D2 Thẻ "AI đã làm gì cho hồ sơ này"** — Block `V7AiWorkCard` (data action GetAiWork, chỉ đọc) chèn trên thanh tab: 6 bước Đọc chứng từ (BnK IDP · AG-1) → Kiểm tra bất thường (AG-4) → Đánh giá lâm sàng (AG-3, tên dòng + mã + đã xác nhận/chờ) → Chuỗi 14 bước (AG-2, số tiền + lần chạy) → Cổng tự duyệt (AG-0, x/4) → Quyết định (người, nhãn trạng thái). Nút "Hỏi AG-2 về hồ sơ" chuyển tab Calculation + cuộn tới panel AG-2. Khung Clinical assessment (AG3AssessmentCard) hết bị bóp (bỏ ThemeGrid_Width1). Thẻ "Proposal pending" chỉ hiện khi khuyến nghị chưa quyết và hồ sơ Under review (trước đó hiện cả trên hồ sơ Paid đã xác nhận). |
| 344 | **Sửa lỗi có sẵn:** khối To-do / checklist ở VI hiện chuỗi thoát `Đã đ…` → nay tiếng Việt thật ("Đã đọc đủ chứng từ bắt buộc", "Phép tính đã cập nhật", "Không còn tín hiệu bất thường", "Đã xác thực tài khoản nhận tiền", "Mọi khuyến nghị đã được quyết định"…). |
| 345–346 | Thẻ AI song ngữ (truyền Lang vào data action server); bước 1 xanh khi có chứng từ; nút Hỏi AG-2 cuộn đúng. |

**Kiểm trên màn:** CLM-2026-0440 (Paid: thác 2.000.000 → −80.000 NOTCOV → −40.000 trần giường → 1.880.000; thẻ AI đủ 6 bước), CLM-2026-0445 (thác 2.000.000 → −2.000.000 Kiểm tra kỳ chờ → 0), CLM-2026-0001 (VI: thẻ AI tiếng Việt, đề xuất Atorvastatin đang chờ, thẻ Confirm/Reject vẫn hiện đúng). Không đổi logic tiền/cổng/agent → không cần chạy lại T-GATE. Lỗi validation = baseline 7.
**Còn mở (UI):** dòng điều kiện cổng trong khối To-do vẫn tiếng Anh khi ở VI; tên bước chuỗi (Explain) chỉ có tiếng Việt khi ở EN; thời gian từng bước trên thẻ AI chưa hiện (AgentRun không có thời lượng dùng được).
