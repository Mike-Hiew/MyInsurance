# Open bugs — My Insurance Portal + BackOffice (19/10/2026)

Bàn giao cho session sửa lỗi. Các lỗi dưới đây được phát hiện khi quay video demo hackathon ngày 19/10 trên **bnk-dev** (ClaimMVP rev 951 = My Insurance BackOffice, MyInsurance rev 333 = My Insurance Portal). Trạng thái sửa từng đợt: xem mục **D** cuối doc.

## 0. Bối cảnh cho session nhận việc

| Mục | Giá trị |
|---|---|
| Môi trường | `https://bnk-dev.outsystems.app` (ODC) |
| BackOffice | `/ClaimMVP/...` — ClaimDetail, ApprovalQueue, AgentMap, AgentTrace, ChainTests, Administration |
| Portal | `/MyInsurance/...` — PortalHome, PortalNewClaim, PortalClaims, PortalClaimDetail, PortalSupplement |
| Tài khoản mẫu BackOffice | Login → Quick sign-in: **Claim officer** (mike), **Approver** (Mai Trung Hieu) |
| Tài khoản mẫu Portal | Login → **Khách hàng · mẫu** (Nguyễn Văn An) |
| Lưu ý session | Hai app dùng chung phiên đăng nhập: đổi tài khoản ở BackOffice sẽ đăng xuất Portal |
| Ngôn ngữ | BackOffice: menu avatar → Language EN/VI. Portal: menu avatar → EN/VI |

**Hồ sơ dùng để tái hiện (KHÔNG xoá/archive — đang dùng trong video demo):**

| Claim | Tình huống | Trạng thái hiện tại |
|---|---|---|
| CLM-2026-0635 | Nha khoa, An, Vinmec Times City | Auto-approved · Paid 525,000 |
| CLM-2026-0634 | Hà, thiếu phiếu khám → bổ sung | Paid 376,000 |
| CLM-2026-0638 | An, Tam Anh, vượt cap + multivitamin bị loại | Approved 1,360,000 (mike nộp, Hieu duyệt) |
| CLM-2026-0633 | Chứng từ tên LE VAN BINH (AG-4 CROSSCHECK_FAIL) | Declined 0 (mike đề xuất, Hieu xác nhận) |
| CLM-2026-0611 | Hoá đơn tiếng Anh, USD, Vinmec Central Park | Paid 1,336,400 ($52.00) |
| CLM-2026-0612 | Hồ sơ cũ đã Declined | Declined |

**Quy ước ưu tiên:** P1 = khách hàng / giám khảo thấy trực tiếp trong demo · P2 = cán bộ thấy trong BackOffice · P3 = trang kỹ thuật hoặc dữ liệu.

**Cách verify chung cho nhóm L (localisation):** chuyển app sang **EN**, đi lại bước tái hiện, chụp màn hình. Không còn ký tự tiếng Việt trên UI (trừ tên riêng như tên người). Sau đó chuyển **VI** kiểm lại để chắc không làm hỏng bản tiếng Việt.

---

## A. Nhóm L — Còn tiếng Việt khi app ở chế độ EN

### Portal

**L-01 · P1 · Lịch chọn ngày hiển thị tiếng Việt** — ✅ Đã sửa (đợt 3)
- Tái hiện: Portal EN → New claim → bấm ô Event date.
- Thực tế: "Tháng 10 2026", thứ "T2 T3 T4 T5 T6 T7 CN".
- Mong đợi: "October 2026", "Mo Tu We Th Fr Sa Su" (locale theo ngôn ngữ đang chọn).

**L-02 · P1 · Checklist chứng từ: nhãn "Received (Báo cáo y tế)"** — 🟡 Đã sửa trong code (đợt 2), chưa tải chứng từ thật để kiểm
- Tái hiện: Portal EN → New claim → bước Photograph documents → tải chứng từ → xem cột "Documents this claim needs".
- Thực tế: dòng Examination record có badge "Received (Báo cáo y tế)".
- Mong đợi: "Received (Medical report)" — tên loại chứng từ lấy theo ngôn ngữ.

**L-03 · P1 · Bước Decision hiện trạng thái tiếng Việt** — ✅ Đã sửa (đợt 4)
- Tái hiện: Portal EN → PortalClaimDetail của 0635 / 0638 / 0633.
- Thực tế: dưới bước Decision là "Đã chi trả" / "Đã duyệt" / "Từ chối".
- Mong đợi: "Paid" / "Approved" / "Declined".

**L-04 · P1 · Lý do từ chối hiện tiếng Việt và bị cắt chữ** — ✅ Đã sửa (đợt 1)
- Tái hiện: Portal EN → `PortalClaimDetail?ClaimId=633`.
- Thực tế: "Reason: Chứng từ không hợp 1" (mất chữ "lệ", và là tiếng Việt).
- Mong đợi: "Reason: Invalid documents" hiển thị đủ. Liên quan L-09 (lưu lý do dạng mã).
- Kết quả: EN "Reason: Invalid documents", VI "Lý do: Chứng từ không hợp lệ" (MyInsurance rev 334).

**L-05 · P1 · Danh sách chứng từ còn thiếu bằng tiếng Việt** — ✅ Đã sửa (đợt 2, kiểm trên CLM-2026-0497)
- Tái hiện: Portal EN → hồ sơ đang Waiting for documents → khối "We need a few more documents"; và trang Send documents (PortalSupplement) → "This claim needs".
- Thực tế: "Phiếu khám bệnh", "Đơn thuốc", "Hoá đơn GTGT điện tử (XML)".
- Mong đợi: "Examination record", "Prescription", "VAT e-invoice (XML)".
- Ghi chú: cần tạo một hồ sơ mới thiếu phiếu khám để tái hiện (0634 đã đủ).

**L-06 · P2 · Định dạng số tiền lẫn VI/EN sau khi đổi ngôn ngữ** — ✅ Đã sửa (đợt 3)
- Tái hiện: Portal → chuyển VI → mở 0638 → chuyển lại EN (không reload).
- Thực tế: "Insurer pays 1.360.000 ₫" nằm cạnh "You bear 770,000 VND".
- Mong đợi: toàn bộ theo locale EN: "1,360,000 VND".

**L-07 · P2 · Trợ lý khách hàng trả lời/chào bằng tiếng Việt khi đang EN** — ✅ Đã sửa (đợt 7)
- Tái hiện: Portal EN → Ask the assistant → hỏi "CLM-2026-0638 - why was it reduced?".
- Thực tế: câu trả lời tiếng Anh nhưng số tiền "280.000 ₫". Trong một số lần (sau khi đổi VI→EN) lời chào là "Xin chào anh An! …" và lỗi "Xin lỗi, hiện tôi chưa trả lời được. Quý khách vui lòng thử lại sau ít phút."
- Mong đợi: lời chào, câu trả lời, câu báo lỗi và định dạng số theo ngôn ngữ đang chọn.

**L-08 · P3 · Tên cơ sở y tế hiện bản tiếng Việt**
- Tái hiện: Portal EN → Claims / Home / Claim detail.
- Thực tế: "Bệnh viện Đa khoa Quốc tế Vinmec Times City", "Bệnh viện Đa khoa Quốc tế Vinmec Central Park".
- Mong đợi (tuỳ quyết định sản phẩm): nếu danh mục có tên tiếng Anh thì hiện tên tiếng Anh khi EN ("Vinmec Times City International Hospital"); nếu không thì giữ nguyên — ghi rõ quyết định.
- Quyết định (19/10): thêm cột NameEn cho cơ sở y tế; khi trống thì dịch theo quy tắc dự phòng.

### BackOffice

**L-09 · P1 · Lý do đề xuất từ chối là tiếng Việt cố định** — ✅ Đã sửa (đợt 1)
- Tái hiện: BackOffice EN, đăng nhập Claim officer → mở hồ sơ đang Under review có tín hiệu AG-4 → What needs human judgement → Propose rejection.
- Thực tế: các lựa chọn "Trùng hồ sơ / Chứng từ không hợp lệ / Ngoài phạm vi bảo hiểm / Khác".
- Mong đợi: "Duplicate claim / Invalid documents / Not covered by the policy / Other".
- Gợi ý: lưu lý do dạng mã (`DUPLICATE`, `INVALID_DOCS`, `NOT_COVERED`, `OTHER`), dịch lúc hiển thị. Sửa đúng chỗ này sẽ hết luôn L-04 và L-10. Dữ liệu cũ (0633 đang lưu text "Chứng từ không hợp lệ") cần map sang mã.
- Kết quả: dialog EN "Duplicate claim / Invalid documents / Not covered by the policy / Other", VI giữ nguyên nhãn cũ; chỉ lưu mã (ClaimMVP rev 952). Kiểm trên CLM-2026-0593 (chỉ mở dialog, không gửi).

**L-10 · P1 · Lý do từ chối hiện tiếng Việt ở các màn sau** — ✅ Đã sửa (đợt 1)
- Tái hiện: BackOffice EN → ClaimDetail 633.
- Thực tế: header "Proposed: Reject · Chứng từ không hợp lệ"; thẻ What the AI proposes "Chứng từ không hợp lệ"; dialog "Confirm rejection · CLM-2026-0633/1 · Chứng từ không hợp lệ".
- Mong đợi: "Invalid documents". Phụ thuộc L-09.
- Kết quả: EN "Proposed: Reject · Invalid documents"; VI "Đề xuất: Từ chối · Chứng từ không hợp lệ".

**L-11 · P1 · Nhãn ô lý do trong dialog Return to assessor** — ✅ Đã sửa (đợt 6)
- Tái hiện: đăng nhập Approver → Approval queue → mở hồ sơ → Return to assessor.
- Thực tế: nhãn "Lý do trả về (bắt buộc, tối thiểu 10 ký tự)".
- Mong đợi: "Reason (required, at least 10 characters)".

**L-12 · P2 · Toast four-eyes in kèm câu tiếng Việt** — 🟡 Đã sửa trong code (đợt 6); không còn tái hiện được vì nút đã bị khoá (F-13)
- Tái hiện: Claim officer nộp hồ sơ (Submit for approval), rồi chính tài khoản đó bấm Approve → Approve.
- Thực tế: "Approver must differ from the latest assessor (mike) — four-eyes rule. / Người phê duyệt phải khác người thẩm định gần nhất (mike) — quy tắc bốn mắt."
- Mong đợi: chỉ một ngôn ngữ theo locale. (Logic chặn ở server đúng — giữ nguyên.)

**L-13 · P2 · Hồ sơ chờ chứng từ: tên chứng từ thiếu và kết quả check AG-1** — ✅ Đã sửa (đợt 2, kiểm trên CLM-2026-0497)
- Tái hiện: BackOffice EN → hồ sơ Waiting for documents.
- Thực tế: khối "Waiting for documents from the customer" → MISSING "Phiếu khám bệnh"; dưới bước Intake: "✓ R1 Tên bệnh nhân nhất quán".
- Mong đợi: "Examination record"; "R1 Patient name consistent".

**L-14 · P2 · Nguồn tỷ giá trong ô Invoice currency**
- Tái hiện: BackOffice EN → ClaimDetail 611 → header, ô INVOICE CURRENCY.
- Thực tế: "Vietcombank – tỷ giá mua chuyển khoản".
- Mong đợi: "Vietcombank – transfer buying rate".

**L-15 · P2 · Thư khách hàng AG-5 chỉ có tiếng Việt**
- Tái hiện: BackOffice EN → ClaimDetail 635 → Advanced mode → Approval & payment → Customer letters → Edit.
- Thực tế: "Kính gửi Quý khách Nguyễn Văn An, Hồ sơ bồi thường CLM-2026-0635 … đã được xét duyệt."
- Mong đợi: thư theo ngôn ngữ của khách (thêm thuộc tính ngôn ngữ ưa thích của member, truyền vào prompt AG-5). Ít nhất phải tạo được bản tiếng Anh cho khách chọn EN.

**L-16 · P2 · Mô tả 14 bước trong Calculation & ledger**
- Tái hiện: BackOffice EN → ClaimDetail 635 → Advanced mode → Calculation & ledger.
- Thực tế: "TERM-HC-001 — KHÔNG_THUỘC_PHẠM_VI", "KHÔNG_ÁP_DỤNG — cơ sở trong mạng lưới, không điều kiện loại trừ nào đúng", "Post lặp 4 posting … trạng thái PROVISIONAL", "copay 30% (tỉ lệ 300)".
- Mong đợi: câu mô tả tiếng Anh; mã enum có thể giữ nhưng nên hiển thị nhãn ("Not in scope", "Not applicable — in-network provider…").

**L-17 · P2 · "Đồng chi trả" trong waterfall — CẦN XÁC NHẬN**
- Thấy ở phiên quay trước (trước 19/10), lần quay cuối waterfall đã hiện "Co-payment". Kiểm lại ở ClaimDetail (Summary → Why) và Portal (Why you pay part…); nếu không còn thì đóng.

**L-18 · P3 · Tín hiệu AG-4 ghi tên danh mục tiếng Việt**
- Tái hiện: ClaimDetail 635 → Approval & payment → Anomaly signals.
- Thực tế: "Provider 'Bệnh viện Đa khoa Quốc tế Vinmec Times City' partially matched 'BỆNH VIỆN ĐA KHOA QUỐC TẾ VINMEC' in catalogue".
- Mong đợi: chấp nhận được vì là dữ liệu; chỉ sửa nếu có tên tiếng Anh trong danh mục.
- Quyết định (19/10): AG-4 giữ tên gốc trong danh mục, kèm tên EN trong ngoặc khi có.

**L-19 · P3 · Payload agent trong Technical trace**
- Tái hiện: `/ClaimMVP/AgentTrace?ClaimId=635`.
- Thực tế: "Lưu bản nháp RESULT cho CLM-2026-0635. Cần cán bộ lưu ý: không có.", AG-3 "Tôi đã đọc clinical context bằng Agent_GetClinicalContext…".
- Mong đợi: agent trả lời theo ngôn ngữ của phiên làm việc (truyền locale vào system prompt AG-3/AG-5).

**L-20 · P3 · Cột Input/Expected của Chain Tests**
- Tái hiện: `/ClaimMVP/ChainTests`.
- Thực tế: "Policy hiệu lực 01/04/2026-31/03/2027, ngày dịch vụ…", "Not eligible, decline PERIOD cấp event…".
- Mong đợi: tiếng Anh (hoặc song ngữ có cột riêng). Ưu tiên thấp.

---

## B. Nhóm F — Lỗi chức năng / hiển thị khác

**F-01 · P1 · Date picker Portal không cập nhật giá trị** — ✅ Đã sửa (đợt 5)
- Tái hiện: Portal → New claim → chọn Event date, Treated from, Treated to bằng lịch → Continue.
- Thực tế: báo "Treated-from date is required" dù ô đã có ngày. Giá trị hiển thị không được đẩy vào biến của màn hình.
- Mong đợi: chọn ngày bằng lịch lưu đúng giá trị. (Lúc quay phải dùng script gọi onChange để vượt qua.)

**F-02 · P1 · Danh sách Claims: hồ sơ bị từ chối vẫn hiện số tiền chi trả** — ✅ Đã sửa (đợt 4)
- Tái hiện: Portal → Claims.
- Thực tế: 0633 Declined · Payout 496,000 VND; 0612 Declined · Payout 376,000 VND ($14.63 khi USD).
- Mong đợi: Payout "—" hoặc 0 khi Declined.

**F-03 · P1 · "Why you pay part of this…" của 0638 cộng sai** — ✅ Đã sửa (đợt 4)
- Tái hiện: Portal → `PortalClaimDetail?ClaimId=638`.
- Thực tế: liệt kê 340,000 (co-pay) + 150,000 (vượt cap) + 280,000 "Other policy terms (waiting period, exclusions, annual limit)" + 280,000 "Multivitamin … not covered" = 1,050,000 ≠ 770,000.
- Mong đợi: khoản 280,000 của multivitamin chỉ xuất hiện một lần (không đếm vào "Other policy terms"); tổng các khoản = 770,000.

**F-04 · P1 · Hồ sơ bị từ chối: tiêu đề "Why you pay part of this 620,000 VND"** — ✅ Đã sửa (đợt 4)
- Tái hiện: Portal → claim 633.
- Mong đợi: tiêu đề phù hợp cho hồ sơ từ chối, ví dụ "Why this claim was not paid".

**F-05 · P2 · Portal USD: thẻ hero hiện "5.200 ₫"** — ✅ Đã sửa (đợt 3)
- Tái hiện: Portal → chọn USD ở header → Home (hồ sơ 0611).
- Thực tế: "5.200 ₫".
- Mong đợi: "$52.00".

**F-06 · P2 · Home: hồ sơ đã Approved vẫn ghi "We are reading your documents."** — ✅ Đã sửa (đợt 7)
- Tái hiện: Portal → Home, thẻ hồ sơ mới nhất (0638 Approved).
- Mong đợi: câu mô tả theo trạng thái (ví dụ "Approved — payment on the way").

**F-07 · P2 · Giao diện điện thoại (390px) vỡ chữ** — ✅ Đã sửa (đợt 7)
- Tái hiện: mở Portal ở viewport 390×844.
- Thực tế: "Specialis t consultat ion" bị ngắt giữa từ; số "1,360,000" bị cắt; nhãn stepper chồng nhau ("Assessmen Decision").
- Mong đợi: chữ xuống dòng theo từ, số tiền co nhỏ hoặc xuống dòng.

**F-08 · P2 · Menu Portal: "Phone view" không có tác dụng** — ✅ Đã sửa (đợt 7)
- Tái hiện: Portal → avatar → Phone view.
- Mong đợi: bật khung điện thoại như bên BackOffice, hoặc ẩn mục này.

**F-09 · P2 · Trợ lý khách hàng không hiểu "my latest claim"** — ✅ Đã sửa (đợt 7)
- Tái hiện: Portal → Ask the assistant → "What is the status of my latest claim?".
- Thực tế: "I need a specific claim number…".
- Mong đợi: tự lấy hồ sơ mới nhất của chính khách hàng (gợi ý có sẵn "What's the current step of my latest claim?" cũng phải chạy được).

**F-10 · P2 · Sau "Propose rejection", Summary và Approval queue vẫn ghi đề xuất Approve** — ✅ Đã sửa (đợt 6)
- Tái hiện: officer Propose rejection trên một hồ sơ → xem Summary và Approval queue.
- Thực tế: thẻ What the AI proposes vẫn "Approve 496,000 VND"; cột AI PROPOSAL ở queue vẫn "Approve". Chỉ nút của approver đổi thành "Confirm rejection".
- Mong đợi: hiện rõ "Proposed by officer: Reject · <lý do>" ở cả hai nơi.

**F-11 · P2 · Stepper ghi "Auto-approved" dù người duyệt bấm Approve** — ✅ Đã sửa (đợt 6)
- Tái hiện: BackOffice → ClaimDetail 638.
- Mong đợi: "Approved" (hoặc "Approved by approver") khi có người duyệt.

**F-12 · P2 · AG-4 hiện "No signal" sau khi hồ sơ bị từ chối** — ✅ Đã sửa (đợt 6)
- Tái hiện: BackOffice → ClaimDetail 633.
- Thực tế: strip "What AI did" → Anomaly "No signal", trong khi thẻ "AG-4 · CROSSCHECK_FAIL (MEDIUM) — open" vẫn còn bên dưới.
- Mong đợi: strip giữ "CROSSCHECK_FAIL · MEDIUM" (tín hiệu không mất khi quyết định).

**F-13 · P3 · Nút Approve hiện cho chính người đã thẩm định** — ✅ Đã sửa (đợt 6)
- Tái hiện: officer (mike, có role Approver) nộp hồ sơ → vẫn thấy "Approve · 1,360,000 VND".
- Thực tế: server chặn đúng (xem L-12), nhưng nút không nên hiện.
- Mong đợi: ẩn/disable nút Approve kèm tooltip "You assessed this claim — another approver must decide."

**F-14 · P2 · System checks báo sai trên hồ sơ sạch**
- Tái hiện: BackOffice → ClaimDetail 635 → Advanced mode → Extraction & costs → System checks.
- Thực tế: "✕ Cross-document checks have no failure" (dấu ✕) trong khi R1–R9 = 0 warning.
- Mong đợi: ✓ và câu chữ "Cross-document checks passed".

**F-15 · P2 · "Gastroscopy" (tiếng Anh) không map được quyền lợi**
- Tái hiện: tạo hồ sơ có dòng "Gastroscopy" trên hoá đơn tiếng Anh.
- Thực tế: bị loại là quyền lợi không được bảo hiểm.
- Mong đợi: map vào 200.1 (hoặc nhóm thủ thuật chẩn đoán phù hợp) như bản tiếng Việt "Nội soi dạ dày".

**F-16 · P2 · Trợ lý cán bộ bỏ sót phần cắt theo cap mỗi lần khám**
- Tái hiện: BackOffice → ClaimDetail 638 → Ask the assistant → "Why was this claim reduced?".
- Thực tế: chỉ nói multivitamin bị loại, "No other line-level reduction reason".
- Mong đợi: nêu đủ: loại 280,000 (multivitamin), vượt cap 200.1 150,000, đồng chi trả 340,000.

**F-17 · P3 · Agent map là danh sách thô**
- Tái hiện: `/ClaimMVP/AgentMap`.
- Mong đợi: dạng thẻ/sơ đồ theo style các màn khác.

**F-18 · P3 · Hiển thị nhỏ** — ✅ Đã sửa (đợt 7; phần badge checklist ở đợt 2)
- Ô "Decided · Paid" trong Auto-approval conditions (Approval & payment) gần như không đọc được (chữ nhạt trên nền trắng).
- Checklist Portal: badge chồng lên nhãn ở dòng "VAT invoice" và "Prescription".

---

## C. Gợi ý thứ tự làm

1. **L-09 → L-10 → L-04** (lưu lý do từ chối dạng mã) — một thay đổi giải quyết 3 lỗi P1.
2. **Tên loại chứng từ theo ngôn ngữ** → L-02, L-05, L-13.
3. **Locale ngày/số** → L-01, L-06, F-05.
4. **F-02, F-03, F-04** (số tiền sai trên Portal) — giám khảo dễ thấy nhất.
5. **F-01** date picker.
6. Phần còn lại theo ưu tiên P2 → P3.

Sau mỗi nhóm: publish, chạy `/ClaimMVP/ChainTests` (Run runnable tests + Run gate tests, kỳ vọng 46/46 và 5/5), và đi lại luồng E2E của hồ sơ liên quan.

---

## D. Trạng thái sửa (session 19/10)

Ràng buộc: không archive/xoá/đổi trạng thái hồ sơ demo 0611, 0612, 0633, 0634, 0635, 0638.

| Đợt | Bug | Trạng thái | Revision | ChainTests | EN / VI |
|---|---|---|---|---|---|
| 1 | L-09, L-10, L-04 | ✅ Xong | ClaimMVP 952 · MyInsurance 334 | 46/46 · cổng 5/5 (12:48) | ✅ / ✅ |
| 2 | L-02, L-05, L-13 (+ F-18 badge) | ✅ Xong (L-02 chưa kiểm bằng upload thật) | ClaimMVP 953 · MyInsurance 337 | 46/46 · cổng 5/5 (13:23) | ✅ / ✅ |
| 3 | L-01, L-06, F-05 | ✅ Xong | MyInsurance 338 | 46/46 · cổng 5/5 (13:29) | ✅ / ✅ |
| 4 | F-02, F-03, F-04, L-03 | ✅ Xong | MyInsurance 339 → 342 | 46/46 · cổng 5/5 (13:44) | ✅ / ✅ |
| 5 | F-01 | ✅ Xong | MyInsurance 343 → 344 | 46/46 · cổng 5/5 (13:58) | không liên quan ngôn ngữ |
| 6 | L-11, L-12, F-10–F-13 | ✅ Xong (L-12 chưa tái hiện được) | ClaimMVP 954 → 962 | 46/46 · cổng 5/5 (14:19) | ✅ / ✅ |
| 7 | L-07, F-06–F-09, F-18 | ✅ Xong | MyInsurance 345 → 350 · ClaimMVP 963, 966 | 46/46 · cổng 5/5 (14:48) | ✅ / ✅ |
| 8 | L-14–L-17, L-19, F-14–F-16 | ⏳ Chưa làm | | | |
| 9 | L-08, L-18, L-20, F-17 | ⏳ Chưa làm | | | |

### Đợt 1 — Lý do từ chối lưu dạng mã (19/10, 12:30–12:55)

**ClaimMVP rev 952**
- `Claim.DecisionReasonCode` Text(20) → Text(50) (nguyên nhân chữ bị cắt "Chứng từ không hợp l").
- Static entity `RejectReason`: `DUPLICATE`, `INVALID_DOCS`, `NOT_COVERED`, `OTHER` + LabelEn / LabelVi.
- `RejectReasonLabel(Code, Lang)` bản server + client: nhận mã, nhận cả text tiếng Việt cũ (đủ hoặc bị cắt 20 ký tự), không khớp thì trả nguyên văn.
- ClaimDetail: dialog Propose rejection dùng mã làm giá trị, nhãn theo ngôn ngữ, mặc định `DUPLICATE` (giữ như cũ). 5 chỗ hiển thị (header Proposed, thẻ What the AI proposes ×2, tiêu đề dialog Confirm rejection, Submitted by) gọi `RejectReasonLabel`.
- `Portal_GetClaimProgress2` trả thêm `DecisionReasonCode`, `ReasonLabelEn`, `ReasonLabelVi`.
- Administration → **Migrate rejection reason codes** (idempotent, chỉ sửa trường lý do). Đã chạy 1 lần: 5 dòng ClaimEvent (821, 1106, 1135 → `DUPLICATE`; 1179, 1303 → `INVALID_DOCS`) + 2 Claim (CLM-2026-0633, CLM-2026-0612: "Chứng từ không hợp l" → `INVALID_DOCS`). Không đổi trạng thái, số tiền hay IsArchived.

**MyInsurance rev 334**
- Client function `RejectReasonLabel(Code, Lang)` (nhận cả tiền tố ≥ 10 ký tự của nhãn cũ). Dòng "Reason:" ở PortalClaimDetail và `V7ReasonText` dùng hàm này; không cắt chữ.

**Kiểm tra**
- BackOffice 633 — EN: "Proposed: Reject · Invalid documents"; VI: "Đề xuất: Từ chối · Chứng từ không hợp lệ".
- Portal 633 — EN: "Reason: Invalid documents"; VI: "Lý do: Chứng từ không hợp lệ"; đổi lại EN đúng.
- Dialog Propose rejection (mở trên CLM-2026-0593, không gửi) — EN và VI đúng nhãn.
- ChainTests: 46/46 đạt, cổng 5/5 (T-GATE-01…05 ✓, 06–07 thủ công).
- Còn thấy trên 633 nhưng thuộc đợt khác: "Từ chối" dưới bước Decision ở Portal EN (L-03), tiêu đề "Why you pay part of this…" (F-04), tên cơ sở y tế tiếng Việt (L-08).

### Đợt 2 — Tên loại chứng từ theo ngôn ngữ (19/10, 12:55–13:25)

Nguyên nhân: catalogue `DocumentType` đã có sẵn `LabelEn` / `LabelVi` (11 dòng) nhưng chỗ hiển thị đọc text tiếng Việt lưu trong sự kiện hoặc dùng bảng map tự viết.

**ClaimMVP rev 953**
- Client function `DocNamesEn(Text)`: đổi các nhãn tiếng Việt của 11 loại chứng từ sang `LabelEn` (thay chuỗi dài trước).
- ClaimDetail: `PiValMissing` (dòng MISSING ở header) và `EvDetailExpr` (cột Detail của sự kiện `DOCS_REQUESTED`) dùng `DocNamesEn` khi EN. Nhãn R1–R9 (`CcRuleExpr`) đã dùng `RuleNameEn`, không cần sửa.

**MyInsurance rev 335 → 337**
- Function `DocTypeLabelEn(Code)` lấy tên EN theo mã loại chứng từ (vd. `T1-04` → "VAT e-invoice (XML)"), dùng ở PortalSupplement ("This claim needs"), PortalClaimDetail ("We need a few more documents") và badge "Received (…)" ở PortalNewClaim.
- Rev 335 hiện "VAT invoice" cho T1-04 (bảng map tự viết) → rev 337 lấy đúng theo catalogue.
- F-18 (phần checklist): badge "Recommended" không còn đè lên tên chứng từ.

**Kiểm tra (CLM-2026-0497, đang chờ hoá đơn điện tử)**
- BackOffice EN: "MISSING · VAT e-invoice (XML)", R1–R9 tiếng Anh ("R1 · Patient name consistent"). VI: "CÒN THIẾU · Hoá đơn GTGT điện tử (XML)".
- Portal EN: "This claim needs: VAT e-invoice (XML) · Recommended", "We need a few more documents: VAT e-invoice (XML)". VI: "Hồ sơ đang cần: Hoá đơn GTGT điện tử (XML) · Nên có".
- L-02: badge "Received (…)" đã đổi sang `DocTypeLabelEn` nhưng chưa tải chứng từ thật để xem (sẽ tạo thêm hồ sơ nháp).
- ChainTests: 46/46, cổng 5/5 (13:23).
- Tiếng Việt còn lại trên 497 là dữ liệu (tên dòng chi phí trên hoá đơn, tên cơ sở y tế → L-08).
- Phát hiện thêm khi bật Advanced mode trên 497 (xếp vào đợt 8): "Trần per-visit/per-day/per-event", "Đồng chi trả" (→ L-17 còn), câu AG-4 "Không phát hiện vi phạm ngày… R4 SKIPPED…" (→ L-19).
- Ghi chú vận hành: Mentor phiên ClaimMVP cũ bị timeout 300 s hai lần khi quét cả màn ClaimDetail; mở phiên mới và giao từng bước nhỏ thì chạy được.

### Đợt 3 — Locale ngày / số trên Portal (19/10, 13:25–13:30)

**MyInsurance rev 338**
- Date picker (Event date, Treated from/to) lấy tên tháng / thứ theo ngôn ngữ đang chọn.
- `FormatAmount` định dạng theo ngôn ngữ và tiền hiển thị ngay lúc render. Hiệu ứng đếm số (count-up) trên thẻ số tiền bị tắt vì nó ghi đè số đã định dạng bằng giá trị cũ — đây là nguyên nhân của L-06 và F-05. Nếu cần hiệu ứng này cho video thì phải viết lại để đọc định dạng hiện hành.

**Kiểm tra**
- L-01: EN "October 2026 · Mo Tu We Th Fr Sa Su"; VI "Tháng 10 2026 · T2 … CN".
- L-06 (0638, đổi VI rồi EN không reload): VI toàn bộ "1.360.000 ₫ / 770.000 ₫"; EN toàn bộ "1,360,000 VND / 770,000 VND".
- F-05 (USD): Home hero 0638 "$52.92", thẻ My claims toàn $; PortalClaimDetail 0611 "$52.00"; PortalClaims không còn "₫". Đã đổi lại VND sau khi kiểm.
- ChainTests: 46/46, cổng 5/5 (13:29).

### Đợt 4 — Số tiền & trạng thái trên Portal (19/10, 13:30–13:45)

**MyInsurance rev 339 → 342**
- F-02: PortalClaims và thẻ "My claims" ở Home hiện "—" ở cột Payout khi hồ sơ Declined (trước đó lấy số đề xuất). Hero ở Home: hồ sơ Declined hiện 0.
- F-04: hồ sơ Declined dùng tiêu đề EN "Why this claim was not paid" / VI "Vì sao hồ sơ không được chi trả", giữ dòng Lý do.
- L-03: nhãn dưới bước Decision theo ngôn ngữ (Paid / Approved / Declined).
- F-03: dòng "Other policy terms" = phần còn lại sau khi trừ đồng chi trả, vượt hạn mức và các dòng ✕ (dòng có số được trả = 0); bằng 0 thì ẩn. Rev 340 tính trong OnAfterFetch của Claim khi danh sách dòng chưa tải xong nên vẫn ra 280,000 → rev 341 chuyển sang OnAfterFetch của danh sách dòng → rev 342 chỉ trừ các dòng ✕ (không trừ phần đồng chi trả của dòng thường).

**Kiểm tra**
- PortalClaims EN: 0633 Declined "—", 0612 Declined "—", 0638 1,360,000, 0635 525,000.
- 0638 EN: 340,000 + 150,000 + ✕ Multivitamin 280,000 = 770,000 (không còn Other). VI: 340.000 ₫ + 150.000 ₫ + 280.000 ₫ = 770.000 ₫.
- 0633 EN: "Why this claim was not paid · Reason: Invalid documents" · Decision "Declined". VI: "Vì sao hồ sơ không được chi trả · Lý do: Chứng từ không hợp lệ" · "Từ chối".
- 0635 EN: Decision "Paid", co-pay 225,000 = header. 0611: co-pay 334,100 = header.
- ChainTests: 46/46, cổng 5/5 (13:44).

### Đợt 5 — Date picker Portal (19/10, 13:45–14:00)

Nguyên nhân: lịch tự vẽ đặt giá trị vào ô `input type=date` (#EvtInput, #FromInput, #ToInput) và bắn sự kiện input/change, nhưng binding OutSystems không nghe trên các phần tử này nên biến màn hình (`FEventDate`, `FTreatedFrom`, `FTreatedTo`) giữ giá trị mặc định.

**MyInsurance rev 343 → 344**
- Rev 343 thêm bước `ReadDomDates` trong action Continue nhưng tìm phần tử theo widget id nên không thấy → vẫn lỗi.
- Rev 344: `ReadDomDates` chạy đầu tiên, tìm theo id thật (`EvtInput` / `FromInput` / `ToInput`), xoá trạng thái Valid=False cũ trước khi kiểm lại; lịch cập nhật biến ngay khi bấm ngày.

**Kiểm tra** (không tạo hồ sơ): chọn Treated from 14/10 và Treated to 15/10 bằng lịch, để trống Loss type và cơ sở y tế, bấm Continue → chỉ còn "Loss type is required" và "Please enter the healthcare provider"; không còn "Treated-from date is required". ChainTests 46/46, cổng 5/5 (13:58).

### Đợt 6 — Luồng duyệt / từ chối BackOffice (19/10, 13:30–14:20)

**ClaimMVP rev 954 → 962** (sau timeout, chuyển sang phiên Mentor mới ở rev 960)
- L-11: nhãn và placeholder ô lý do trong "Return to assessor" theo ngôn ngữ ("Reason for return (required, at least 10 characters)…" / "Lý do trả về (bắt buộc, tối thiểu 10 ký tự)…").
- L-12: toast four-eyes chỉ hiện câu của ngôn ngữ đang dùng; kiểm tra phía server giữ nguyên.
- F-11: bước 4 của stepper chỉ ghi "Auto-approved / Tự duyệt" khi cổng tự duyệt; có người duyệt thì ghi "Approval / Phê duyệt" (0638).
- F-12: `GetClaimProgress` → aggregate `GetOpenSignal` đổi điều kiện từ Status = OPEN sang Status ≠ SUPERSEDED, sắp theo RaisedOn giảm dần. `SigCountExpr` (Approval & payment) dùng `GetTopActiveSignal`.
- F-13: nguyên nhân là sự kiện SUBMITTED lưu Actor = tên hiển thị, PROPOSE_REJECT lưu Actor = GUID, và aggregate lấy sự kiện mới nhất không có sort. Đã sửa: sort theo ClaimEvent.Id, so sánh cả hai dạng. Người thẩm định gần nhất không còn thấy nút Approve / Confirm rejection, thay bằng ghi chú "You assessed this claim — another approver must decide." / "Bạn đã thẩm định hồ sơ này — cần người phê duyệt khác quyết định."
- F-10: ClaimDetail Summary hiện "Proposed by officer: Reject · <lý do>"; ApprovalQueue có thêm `OfficerRejectCode` (sự kiện mới nhất theo Id là PROPOSE_REJECT), nhãn dịch ở client. Rev 960 lấy mã lệch 1 ký tự ("UPLICATE") → 961 sửa → 962 chuyển nhãn sang client vì server trả tiếng Việt khi đang EN.

**Kiểm tra**
- 0633 strip "What AI did": "CROSSCHECK_FAIL · MEDIUM" (trước: "No signal"); 0635 vẫn "No signal".
- 0638 stepper: "Phê duyệt" (trước: "Tự duyệt").
- CLM-2026-0604 (mike đề xuất từ chối "Trùng hồ sơ", đang chờ duyệt), đăng nhập mike: chỉ còn "Return to assessor", có ghi chú four-eyes. Summary EN "Proposed by officer: Reject · Duplicate claim"; VI "Cán bộ đề xuất: Từ chối · Trùng hồ sơ". ApprovalQueue EN/VI giống vậy.
- ChainTests: 46/46, cổng 5/5 (14:19).
- Không thao tác duyệt/từ chối trên hồ sơ nào; trạng thái 0604 giữ nguyên.

### Đợt 7 — Portal: trợ lý, Home, mobile; độ tương phản BackOffice (19/10, 14:20–14:50)

**MyInsurance rev 345 → 350**
- F-06: câu phụ trên thẻ hồ sơ mới nhất ở Home theo trạng thái (Approved → "Approved — payment on the way", Declined, Pending information…) thay cho "We are reading your documents." cố định.
- F-08: ẩn mục "Phone view" trong menu avatar Portal (không có khung điện thoại như BackOffice).
- L-07: trợ lý khách hàng gửi ngôn ngữ đang chọn vào mỗi lượt; lời chào, câu trả lời và câu báo lỗi theo EN/VI.
- F-09: "my latest claim" / "hồ sơ mới nhất" tự lấy hồ sơ mới nhất của chính khách hàng (sắp theo Claim.Id giảm dần, rev 347). Rev 345 khoá nhầm câu trả lời vào hồ sơ đã chọn trước (0633) → rev 346 cho số hồ sơ ghi rõ trong câu hỏi được ưu tiên lại.
- F-07: CSS ≤ 480px — header xuống dòng, stepper không chồng nhãn (349); số tiền hero `.v7-receipt-top .mk-receipt-amt` 30px, bỏ ellipsis; tên dòng chi phí `.v7-item-name` ngắt theo từ (350).

**ClaimMVP rev 963, 966**
- L-07 (phía dữ liệu): `CustomerAssist_GetClaim` định dạng số tiền theo ngôn ngữ (EN "280,000 VND", VI "280.000 ₫").
- F-18 (phần BackOffice): ô "Decided · Paid" trong Auto-approval conditions đổi sang nền tối, chữ sáng — đọc được trên theme tối.

**Kiểm tra**
- Khung 390×844, PortalClaimDetail 0638: "1,360,000 VND" hiện đủ (30px, scrollWidth = clientWidth); "Specialist consultation", "Multivitamin (dietary supplement)" xuống dòng theo từ; không cuộn ngang (386px).
- Trợ lý Portal EN: "latest claim" → CLM-2026-0638; số tiền "280,000 VND"; VI trả lời tiếng Việt, "280.000 ₫".
- 0635 Approval & payment: "Decided · Paid · 19/10/2026" đọc rõ.
- ChainTests: 46/46, cổng 5/5 (14:48).
- Chưa sửa (ghi nhận): aria-label ô "Treated to" vẫn là "Điều trị đến" khi EN; tên dòng chi phí / cơ sở y tế tiếng Việt là dữ liệu (cơ sở y tế thuộc L-08, đợt 9).
