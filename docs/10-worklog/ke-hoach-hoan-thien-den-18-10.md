# Kế hoạch hoàn thiện đến 18/10 (lập 10/10/2026)

Bản đầy đủ (doc, có lịch + trạng thái): https://claude.ai/code/artifact/4af35c03-bed2-4b3d-a3a5-1fbe0eb6220b
Mốc xuất phát: ClaimMVP rev 528, MyInsurance rev 147. Giả định hạn chót CN 18/10 (chưa chốt). Code freeze 22:00 T5 15/10.

## 18 việc, ~10–11 ngày công
- C · ClaimMVP: C1 B2 ngày tương lai → DATE_IMPOSSIBLE (+chốt chặn cứng) · C2 CorrelationId từ OrchestrateClaim · C3 AG-4 JSON reasoning[], sửa gốc lỗi mất ký tự đầu · C4 EN còn sót (AG-3 Action taken, ExplanationEn, nhãn bước chuỗi) · C5 kiểm vai bằng 2 tài khoản + duyệt thật ca 2 · C6 IdpUsername/Password → Secrets + đổi mật khẩu.
- M · MyInsurance: M1 sửa "Vì sao tự chịu" tách theo khoản · M2 tiến trình 4 bước (Portal_GetClaimProgress) · M3 A7+B7 checklist + IDP nhận loại (~25 s/tệp, hồ sơ nháp) · M4 quét EN + 390 px.
- D · Dữ liệu: D1 ArchiveDemoClaims + FindDuplicateClaims bỏ qua (bắt buộc từ rev 484) · D2 2 bộ chứng từ/ca + ca trùng cố ý · D3 3 ca khó · D4 dọn CLM-0461/0463/0464.
- P · Pitch: P1 deck EN + slide orchestration + Next steps · P2 kịch bản EN + Q&A · P3 video dự phòng (sau C6) · P4 diễn tập ×2.

## Trạng thái C1–C6 (cập nhật 10/10 23:05)
- C1 XONG — rev 529. CLM-2026-0466: FUTURE_DATE → DATE_IMPOSSIBLE HIGH, gate 2/4, không lỗi tool.
- C2 XONG, đã kiểm — rev 533–537. CLM-2026-0468: cả 37 bước (IDP, AG-1, AG-3, AG-4, chuỗi, gate) chung một lượt; nút "Show earlier runs/Xem các lượt cũ" song ngữ. Id mới chỉ sinh khi trống hoặc 15 phút không có AgentRun; Re-run orchestration luôn sinh id mới.
- C3 XONG, đã kiểm — sửa Substr 1-based ở ClinicalAgent rev 6, FraudSignalAgent rev 8, ClaimMVP rev 530. CLM-0468: AG-3/AG-4 đủ ký tự đầu, lý do đánh số 4 dòng. Lệch kế hoạch: AG-4 trả text có khuôn ("- " từng dòng + "Summary:" + "Summary (EN):", FraudSignalAgent rev 9) thay vì JSON. AgentRun cũ vẫn mang chữ bị cắt.
- C4 XONG phần chính — ExplanationEn (rev 531 + AG-4 rev 9), nhãn 14 bước EN (rev 532), AG-3 dòng "Summary (EN)" (ClinicalAgent rev 7), kết luận trace chọn "Summary (EN)" khi ở EN (rev 540; 538–539 là thử nhầm chỗ, vô hại). Đã kiểm CLM-0468 cả EN và VI. Còn tiếng Việt trong EN: log bước tất định AG-1, chi tiết "Đồng chi trả" ở Calculation, tên quyền lợi ở Settlement plan; bước SetFraudCheckDone của AG-4 ghi tiếng Anh cả ở VI.
- Lỗi phát hiện thêm và đã sửa: ProposeRecommendation so `.Count = LineNo` (Count bỏ qua Max Records) → AG-3 chỉ đề xuất được dòng cuối; sửa `List.Length = LineNo`, rev 535. CLM-0468: AG-3 đề xuất NOTCOV dòng 7 Collagen (92%), dòng 8 Vitamin (90%).
- Chẩn đoán cấp hồ sơ trống dù dòng chi phí có J30.4 → đã sửa ở rev 558 (D). AG-3 không gắn cờ Glucosamin → đã sửa ở ClinicalAgent rev 11 (11/10, xem "Điểm mở đã đóng").
- C5 — server đã chặn: ApproveClaim/DeclineClaim/ReturnToOfficer kiểm CheckClaimApproverRole trước tiên + four-eyes (trừ ReturnToOfficer). IssuePaymentOrder không kiểm vai (chỉ chạy khi đã Approved). ApproveClaim chặn khi còn tín hiệu MEDIUM/HIGH mở. Chờ người dùng tạo tài khoản ClaimOfficer và tự thử.
- C6 — chờ người dùng: đổi mật khẩu IDP + nhập giá trị secret ở ODC Portal.

## Trạng thái M1–M4 (cập nhật 11/10 01:10)
- M1 XONG, đã kiểm — Portal_GetCustomerShare2 (ClaimMVP 541, sửa SQL 546–548): tách NotCovered (bước 6) · vượt hạn mức/lần (bước 8) · đồng chi trả (bước 10+11) · điều khoản khác (còn lại), luôn cộng đúng bằng phần tự chịu. WhyCard MyInsurance 148. Kiểm 6 hồ sơ có kết quả: 0456 (380.000 + 1.600.000 + 3.500.000 = 5.480.000), 0440, 0454, 0459, 0460, 0461 — đều khớp. Lỗi gặp: SUM lồng LAG (Postgres 42803), lệch thứ tự cột với ChainStepResult.
- M2 XONG, đã kiểm — Portal_GetClaimProgress2 (ClaimMVP 542, ẩn tên agent), MyInsurance 149 poll 3 s, dừng khi xong. CLM-0471: bước 3 tự chuyển "Assessment · Checking benefits and costs" không cần tải lại.
- M3 XONG (bản đầy đủ), đã kiểm — Nháp tạo ở bước 2 (Claim.IsDraft, StatusId vẫn Received); mỗi ô gọi Portal_ReadDocument2 ngay, so DetectedTemplateId (IDP) với loại ô; ô bắt buộc thiếu viền đỏ "Missing/Còn thiếu"; bấm gửi khi thiếu/lệch → panel cảnh báo "Submit anyway / Go back and add" (không chặn). Finalize = Portal_FinalizeDraftClaim2. Nháp bị loại khỏi worklist, dashboard, approval queue, incidents, FindDuplicateClaims, danh sách portal (lọc IsDraft). ClaimMVP 543–553, MyInsurance 150–153.
- M4 XONG — wrap T() trên PortalHome/PortalClaims/PortalNewClaim/PortalClaimDetail + CSS ≤420 px (MyInsurance 152–153). EN mode: không còn nhãn UI tiếng Việt; chỉ còn dữ liệu chứng từ. 390 px: 4 màn scrollWidth = 390, không tràn ngang.
- Tracker hồ sơ đã đóng nhưng từ chối một phần (0456) từng hiện bước Thẩm định màu đỏ → đã sửa ở MyInsurance 157.

## Trạng thái D1–D4 (cập nhật 11/10 13:20)
- D1 XONG, đã kiểm — Claim.IsArchived/ArchivedOn/ArchivedBy; ArchiveDemoClaims (giữ CLM-0001…0005, 0440, 0445, 0456 theo Site property DemoKeepClaimNumbers), RestoreArchivedClaims, Archive/RestoreClaimsByNumbers; thẻ "Demo data" ở Administration. IsArchived bị loại khỏi FindDuplicateClaims, worklist, dashboard (kể cả My queue, AG-4/AG-5/ops counters, biểu đồ), approval queue, incidents, checkpoint queue, portal. Kiểm: 0475 tự duyệt 320.000; 0476 trùng bị chặn; sau lưu trữ, 0482 (bộ B) lại tự duyệt 320.000 và 0483 trùng bị chặn.
- D2 XONG phần chạy thử (bộ A + bộ B ca 1/trùng). Kết quả: ca 1 = 320.000 (tự duyệt) · ca trùng = DUPLICATE HIGH · ca 2 nha khoa = 1.020.000 sau xác nhận AG-3 · ca 3 TPCN = 952.000 sau xác nhận AG-3. Bộ B ca 2/ca 3 để dành buổi thật. Chứng từ + _KY-VONG + README-KET-QUA-11-10.md ở E:\Hackaton\Demo-18-10\chung-tu-D2-D3.
- D3 XONG — kho1 bệnh có trước (E11.9) → Decline ở bước kỳ chờ PRE 365 ngày, cần người duyệt (CLM-0489) · kho2 MRI/J02.9 → AG-3 NOTREL 92%, sau xác nhận 200.000 (CLM-0484) — **đề xuất dùng cho pitch** · kho3 nội trú lệch ngày → R4 → AG-4 DATE_IMPOSSIBLE HIGH (CLM-0481).
- D4 XONG — toàn bộ hồ sơ thử 0431…0492 đã lưu trữ; worklist chỉ còn 8 hồ sơ giữ lại; dashboard: 2 AG-4, 2 thư AG-5, 0 ops.

## Lỗi phát hiện & sửa trong D (11/10)
- rev 555: chỉ chứng từ T1-01 tạo dòng chi phí; payable ≤ tổng khai. R4 so ngày ra viện + số ngày giường.
- rev 556–557: keep-list Index ≥ 0; nút Restore.
- rev 558: IcdCode 10 → 50 ký tự (ICD "K04.0; K05.1" làm OrchestrateClaim lỗi); SafeAssignConditionClass; thẻ tiến trình hiện "waiting" khi StartedOn rỗng; điền chẩn đoán từ dữ liệu IDP đã lưu.
- rev 559: số đề xuất = tổng dòng = tổng bút toán (trước đó cộng cả dòng chưa map → 1.795.000 thay vì 780.000).
- rev 560: map thuốc dạng xịt/dung dịch/rửa/nhỏ/kem/mcg vào 200.2; dòng không map được → ASSESSOR + checkpoint thay vì về 0 âm thầm.
- rev 561: lọc IsArchived ở dashboard/menu/ops/incidents/AG-5.
- rev 562–563: map xét nghiệm "Định lượng/HbA1c/Glucose/Công thức máu"; GetClinicalContext trả MedicalHistory/CoverFrom/PossiblePreExisting (thực tế IDP chỉ trả mã ICD, không trả chữ → không dùng được).
- rev 564–566: thử TERM-HC-011 (làm T-CH04-d hỏng) → bỏ ở rev 567; SelectorOp 12 → 50.
- rev 567: AssignConditionClass gán PRE cho ICD mạn tính (E10–E14, I10–I13, I15, J44, J45, N18, C) → BR-WAIT 365 ngày. Nút "Remove TERM-HC-011" (đã chạy, sổ về 10 điều khoản).
- ClinicalAgent rev 8–10: bỏ qua cảnh báo ngày của IDP; chỉ ghi khuyến nghị cho dòng KHÔNG liên quan (hết lỗi Paracetamol NOTREL); thuốc điều trị triệu chứng là REL; quy tắc WAIT khi có tiền sử trước hiệu lực.

## Điểm mở đã đóng (11/10 tối)
- Thẻ "Why" ở ClaimDetail (ClaimMVP 568): dòng "Co-payment and caps" không còn gộp phần AG-3 loại. CLM-0478: −238.000 (trước −1.038.000); 650.000 + 150.000 + 238.000 = 1.990.000 − 952.000 ✓.
- AG-3 Glucosamin (ClinicalAgent rev 11, chỉ sửa prompt): TPCN/sản phẩm khớp (Glucosamin, Chondroitin, Collagen, Omega-3, Vitamin…) là NOTREL/NOTCOV trừ khi chẩn đoán chương M hoặc E50–E64. Kiểm bằng bộ A ca 3 nộp lại → CLM-2026-0492: NOTCOV Glucosamin 90% · Collagen 95% · Vitamin tổng hợp 95%; các thuốc điều trị J30.4 không bị nêu. 0492 đã lưu trữ, không xác nhận đề xuất.
- MyInsurance PortalNewClaim (rev 155 lỗi → rev 156 sửa): rev 155 làm hỏng nút "Tiếp tục" ("ReferenceError: ProviderText is not defined" trong JS ReadDomInputs → toast "Không thể tạo hồ sơ"). Rev 156: $parameters.ProviderText + đọc theo widget Id + try/catch. Kiểm: form trống → chỉ 3 thông báo đỏ theo ô, không toast; điền đủ → bấm 1 lần sang bước 2. Không còn cần "gõ lại ký tự cuối".
- Nháp portal không hiện ở WorkList (lọc IsDraft, ClaimMVP 570–571): 0490/0491 không còn hiện.
- Administration "Archive by claim numbers" (ClaimMVP 568–575): ô nhập giữ chữ (568); khớp theo Claim.ClaimNo đầy đủ, ghi thật vào DB (574: trước đó gán từ bản sao list nên UpdateClaim chế độ Changed Attributes không ghi gì). Thông báo có "Received" + "Verified IsArchived=…". Kiểm: Restore/Archive CLM-2026-0482 và Archive CLM-2026-0492 đều đúng và bấm lại báo 0. Nút "Archive demo claims" không còn toast đỏ (575).
  - Hạn chế số ngắn / nhiều số / nháp: đã xử lý ở 576–583 (xem "Đợt sửa lỗi còn lại").
- Chưa có màn chọn quyền lợi cho dòng ASSESSOR ngoài checkpoint (không làm trong đợt này).

## Đợt sửa lỗi còn lại (11/10 19:00–21:45)
- Lưu trữ/khôi phục theo số (ClaimMVP 576–583): nhận số ngắn "0482", hậu tố "/1", nhiều số cách dấu phẩy/chấm phẩy/xuống dòng, và cả hồ sơ nháp. Thông báo: Received · Normalized · Matched · Verified · Not found. Kiểm: "0490, 0491" → lưu trữ 2 nháp; "0482/1" khôi phục rồi lưu trữ lại; "CLM-2026-0482/1, CLM-2026-0492" khôi phục/lưu trữ 2. Nguyên nhân cũ: nhánh Restore còn Replace("/", ","), hàm chuẩn hoá không được gọi ở Restore. Ghi chú: lần bấm đầu tiên sau khi tải trang bị mất là do công cụ tự động hoá (click đầu không lấy focus), không phải lỗi app — ô nhập không bị refresh (đã đo 8 s, 0 mutation).
- Tracker MyInsurance (rev 157): hồ sơ đóng, chi trả một phần (0456) — bước Thẩm định xanh ✓ + nhãn hổ phách "Một phần không chi trả" / "Partly not covered"; chỉ đỏ khi từ chối toàn bộ (payable = 0).
- Song ngữ ClaimMVP (rev 583–587):
  - Xong: điều kiện EN/VI bị đảo (khối thư kết quả, AG-4 "không có tín hiệu"); dòng "Summary (EN)" không còn hiện ở VI; R1–R9 + kết quả cross-check có EN (thuộc tính RuleNameEn/DetailEn); tên quyền lợi EN ở Policies & benefits; VI: "Nha khoa", "Con người", "Chuyển giao", "Dấu vết tác nhân"; ChainTests EN: "Last run / awaiting step / outside chain"; Technical trace dịch một phần log AG-1.
  - Còn sót (chỉ ở Advanced mode/trang kỹ thuật; màn Summary hai ngôn ngữ sạch): EN — nhóm "Phạm vi bảo hiểm từng dòng / Trần … / Đồng chi trả" + dòng chi tiết ở Calculation & ledger, các dòng History & log (chạy chuỗi, Bước 2/3/6, InferCategory, Ngày/Trùng, CROSSCHECK, lưu bản nháp RESULT), header bảng AG-3; Ops queue "Không đọc được chứng từ"; câu trả lời agent cũ không có "Summary (EN)" vẫn tiếng Việt. VI — "Not a covered item", "· Tool call", "· Agent answer", log IDP "Read … / Submitted …", "rejected: LineNo out of range". Mentor báo đã bọc LocalizeSystemText 6 lượt nhưng hiển thị không đổi.

## Revisions
- ChainTests rev 592: 46/46, gate 5/5 (11/10 23:57); 589 cũng 46/46, 5/5. ChainTests rev 588: 46/46, gate 5/5 (11/10 22:18). ChainTests rev 587: 46/46, gate 5/5 (chạy 11/10 21:40); rev 585: 46/46, 5/5. Validate ClaimMVP giữ 7 lỗi nền (có lúc lên 8 "Invalid Expression" ở 585 trước publish, đã sửa); MyInsurance 0 lỗi; ClinicalAgent 2 lỗi "Unavailable feature" (loại lỗi nền).
- Hiện tại (13/10 17:20): ClaimMVP 689 (tab Chứng từ theo mockup + Xem chứng từ gốc theo từng loại) · MyInsurance 172 · FraudSignalAgent 9 · ClinicalAgent 12. ChainTests rev 689: 46/46, gate 5/5. Validation ClaimMVP 7 lỗi nền. Điểm lùi gần: ClaimMVP 684 / 646 (trước dòng tiền) / 633 / 614.
- Trước đó (12/10 22:15): ClaimMVP 646 (≈ 636, sau revert 637–638 + khôi phục style). ChainTests rev 644: 46/46, gate 5/5.
- Mốc 12/10 15:00: ClaimMVP 613 · MyInsurance 172 · FraudSignalAgent 9 · ClinicalAgent 12. ChainTests rev 613 (EN): 46/46, gate 5/5. (Trước đó: ClaimMVP 592 · MyInsurance 160 · ClinicalAgent 11.) Điểm lùi: ClaimMVP 587 (trước đợt test EN) / 575 (trước đợt lỗi còn lại) / 567 (trước đợt điểm mở) / 553 (trước D) / 540 (trước M) / 528 · MyInsurance 157 (trước đợt test EN; 158 có lỗi ",, VND" ở Claim result) / 156 / 153 (154–155 không dùng: 155 hỏng nút Tiếp tục) / 147 · FraudSignalAgent 7 · ClinicalAgent 10 / 7 (trước D) / 5.
- Lưu ý vận hành: mỗi lần publish ClaimMVP có thể đăng xuất Chrome khỏi bnk-dev. Publish bằng mentor_prompt "Publish" (mentor_publish đã bỏ). Mentor có lúc báo quá tải khi load Agent → mở phiên mới rồi load lại.

## Lịch
CN 11 C1–C3 · T2 12 C4–C6, D1 · T3 13 M1–M2 · T4 14 M3 · T5 15 D2–D4, M4, freeze · T6 16 P1–P2 · T7 17 P3, P4#1 · CN 18 P4#2.
Cắt khi trễ: M3 → D3 (giữ 1 ca) → M2 → C3. Không cắt: C1, C6, D1, D2, M1, P1–P4.

## Cần chốt
Hạn chót thật · đưa lại B4/B6 · R1–R5 chỉ slide · chọn ca D3 cho pitch (đề xuất kho2 MRI).

## Đợt test EN (11/10 từ 22:00) — doc "Test cases demo EN 18/10"
- Bộ ~140 test case (ClaimMVP, MyInsurance, 12 E2E). Quét UI: F01–F16; khi chạy thêm F17–F26.
- ClaimMVP 588: lọc hồ sơ lưu trữ/nháp ở mọi API portal; Dashboard số ít/số nhiều. ChainTests 46/46, gate 5/5.
- MyInsurance 158: lọc IsArchived ở mọi aggregate (portal chỉ còn 0001, 0004, 0440, 0456), tiền theo ngôn ngữ, Claim result (Updated · Refresh, Paid ·, ẩn Add information khi đã đóng). Hồi quy: "1.020.000 ,, VND" ở Claim result (F26).
- Đang dở: prompt ClaimMVP đợt 2 (nhãn trạng thái, tiền, Why/Decline 0445, ẩn chữ kỹ thuật, bước đọc treo 0001, dấu 01/01/1900) — Mentor đang chạy khi MCP OutSystems trên máy bị "Request timed out" (22:35). Chưa publish.
- Chờ gửi: promptC_cm (F06, F11, F13–F16, F20–F23) và promptD_mi (F24–F26, trợ lý trả lời tiếng Việt và sai số).
- Cần người dùng quyết: F19 bút toán hạn mức của hồ sơ test đã lưu trữ; đổi tên thành viên "Demo Thời Gian Chờ"; xử lý hồ sơ giữ lại CLM-0004 đang ExtractionError.
- Thay đổi demo: 0445 đề xuất Decline → sau đợt 2 nút chính sẽ là "Decline claim" (chọn mã lý do), không còn "Approve · 0 VND".

### Cập nhật 12/10 00:50
- Đã publish thêm: MyInsurance 159 (sửa ",, VND", số 0, tiền trong thông báo), 160 (hạn mức New claim); ClaimMVP 589 (nhãn trạng thái, Why/Decline 0445), 590 (popup lý do từ chối: "Decline claim" → popup → "Confirm decline" mới gọi DeclineClaim), 591 (hàm tiền chung EN "2,000,000 VND", GUID → "officer"), 592 (badge Operations queue = thẻ, median).
- Model ClaimMVP đang có thay đổi trợ lý (AskCustomerAssistant: tính hạn mức còn lại + khối FACTS + ngôn ngữ) nhưng CHƯA publish — Mentor treo ở bước Publish (3 lần, chỉ "Thinking…"). Agent CLM_CustomerAssistantAgent không mở được ("A lot of people are currently using Mentor").
- Còn mở: F22 (C1–C3 trên hồ sơ đã xong), câu C1 lưu sẵn, tiền ô Dashboard, F06, F13–F16, F23, F19 (cần người dùng quyết), E2E chưa chạy lại trên bản mới.
- Demo 0445: nút chính giờ là "Decline claim" (popup chọn lý do, mặc định WAIT) rồi "Confirm decline".

### Cập nhật 12/10 15:00 — test lại toàn bộ (EN)
- Publish: ClaimMVP 603–613, MyInsurance 168–172. Validation giữ 7 lỗi nền / 0 lỗi. ChainTests 613 EN 46/46, gate 5/5 (14:45).
- Đạt mới: tiền EN ở các tab Extraction/Events/Calculation; dấu quyết định ghi tên người duyệt ("Confirmed · Mai Trung Hieu"); chẩn đoán đầu hồ sơ "J18.9 · Pneumonia"; Why card 0505 "No calculation yet — an AG-4 signal must be reviewed first"; thẻ Read documents đúng số dòng; bảng AG-3 EN chỉ câu tiếng Anh + tiêu đề "proposed NOTCOV for 3 lines"; thông báo MyInsurance gộp "(N documents)", mới nhất trên đầu, tiền EN; gợi ý cơ sở khám không trùng; ngưỡng Admin "2,000,000 VND".
- Lỗi chức năng tìm thấy và đã sửa (613): AG-3 đề xuất dòng 7–8 của 0503 không có Confirm/Reject, panel báo "No proposals pending" → nguyên nhân đọc lại chứng từ thay dòng chi phí, khuyến nghị trỏ id cũ, join INNER. Đổi LEFT JOIN + khớp theo tên. Đã Confirm dòng 7, bảng tự làm mới (F35 đạt).
- Hồi quy giữa chừng đã sửa: MyInsurance 169–170 trang Notifications "There was a problem" ($data/$variables trong JS node, rồi ListRemove trong vòng lặp) → 171 dùng server action; header AG-3 "lines 0, 0, 0" → 610; câu AG-3 mất/ghép EN+VI → 613.
- Lưu trữ hồ sơ thử 0498–0505 xong; chỉ còn 0497 (của người dùng).
- Còn lại P2: Admin còn dòng nhà cung cấp thử + ngưỡng "1.500.000 ₫"; Technical trace log agent tiếng Việt; mã ICD ghép (K04.0; K05.1) chưa có tên; ngôn ngữ sau đăng nhập chưa kiểm được.
- Cần người dùng quyết: F19 (hạn mức ngoại trú còn 14,647,220 VND do 0502 đã chi trả 320.000), F10 (tên "Demo Thời Gian Chờ").
- Mẹo demo: gõ/chọn đủ tên cơ sở khám; giữ Advanced mode tắt khi trình bày Summary; với hồ sơ có đề xuất AG-3 dùng nút Confirm/Reject ngay ở bước 3.
- Phiên Mentor ClaimMVP mới: 98c44842-9e71-40df-bc31-47e00c113b66 (phiên cũ be1e5560… hay treo).

### Dọn dữ liệu demo 12/10 15:00–15:30 (người dùng duyệt)
- Lưu trữ thêm CLM-0001, 0002, 0003, 0004, 0005 (seed cũ, có sự cố ops/checkpoint tiếng Việt). Work list còn 4: 0440 (Paid), 0445 (Awaiting approval), 0456 (Closed), 0497 (của người dùng). Ops queue 0. ChainTests sau khi lưu trữ: 46/46, gate 5/5 (15:04).
- ClaimMVP 614: Providers ở Admin chỉ đếm hồ sơ thật (bỏ lưu trữ/nháp/fixture), ngưỡng Thresholds định dạng tiền chung, Checkpoint queue tách câu EN/VI; 2 nút Developer tools: "Reverse limit use of archived claims", "Rename demo member".
- Đã chạy: đảo 105 bút toán hạn mức của hồ sơ lưu trữ (tổng 20,865,560 VND; chạy lại = 0). Nguyễn Văn An: ngoại trú dùng 320,000 (0456) → còn 19,680,000; nha khoa dùng 700,000 → còn 4,300,000. Đổi tên "Demo Thời Gian Chờ" → "Phạm Thu Hà" (0445).
- Còn: Providers "Medlatec 8" (có thể là hồ sơ fixture của gate); câu checkpoint của 0497 vẫn tiếng Việt (văn bản AG-3 lưu không có phần EN).

### Tab Trích xuất & chi phí theo mockup v0.8 (12/10 16:00–tối) — ClaimMVP 615–635
- Chi tiết + bảng trạng thái: doc dự án `claude/danh-gia-tab-trich-xuat-vs-mockup-12-10.md`.
- Đã kiểm trên trình duyệt (0497, 0503): hiệu ứng quét khi mở tab; dòng 2/6 của 0497 tô vàng "Đề xuất: loại" + tờ gốc tô dòng; thanh việc cần làm một dòng; layout/pill/✎/Tổng các dòng; chip "Đọc bởi BnK IDP · 27 s"; popup sửa dòng (không đổi logic lưu); thẻ đề xuất ai-surface + "✓ Xác nhận — loại dòng N"/"Bác bỏ"; thẻ đã quyết hiện "… — Mai Trung Hieu xác nhận lúc …" (rev 633; 629–631 hiện GUID, 632 hiện "cán bộ").
- 634–636: khối "Đánh giá lâm sàng (AG-3)" chuyển vào mục thu gọn "AG-3 technical detail ▸ / Chi tiết kỹ thuật AG-3 ▸" cuối tab (trước đó nằm đầu tab, nút ▸ không mở gì). 634 vẫn hiện ở đầu tab; 635 đúng vị trí nhưng không ẩn (rule `[id$="AG3AssessmentCard"]{display:block !important}`); 636 thay bằng `details + [id$="AG3AssessmentCard"]{display:none}` / `details[open] + …{display:block}`. Đã kiểm 0497: đóng → ẩn, bấm → mở, bấm lại → ẩn. Nhỏ: mũi tên vẫn "▸" khi đang mở.
- 12/10 21:10–22:00: thử làm lại thẻ "Why" thành bảng theo nhóm quyền lợi (trần + đồng chi trả) và cột Result hiện số tiền trả (637–638) → người dùng thấy không hợp lý, yêu cầu revert. Publish lại rev 636 bị Mentor tự gộp với 638 (639 = 638), nên gỡ tay ở 640–644. Đã kiểm 0497: Summary như 636 ("Why" top 3, −250,000 / −3,500,000 / −500,000 VND; Result payable/excluded). Còn khác 636 một chút: tiêu đề bảng Line-level cover "6 cost lines · claimed 6.500.000" (636: "2 lines within cover · 5.700.000"). ChainTests 644: 46/46, gate 5/5. 645–646: khôi phục style 636 (pill Result "mc-tag mc-ok"/"mc-tag mc-no", icon thẻ Why ph-stethoscope/ph-x-circle, số tiền màu mặc định, nhãn "Total"); đã kiểm trên trình duyệt. Bài học: KHÔNG revert bằng cách load revision cũ rồi publish — Mentor auto-merge giữ thay đổi mới.
- Sai số hiển thị đã biết (chưa sửa, chờ người dùng chọn cách): thẻ Why trừ cả đề xuất AG-3 đang chờ (dòng 2, 6) dù chuỗi chưa trừ; lý do thật là trần mỗi lần 200.1 (−2.400.000) + 300.1 (−1.600.000) + đồng chi trả 20%/30% (−600.000); Result dòng 4, 5 "excluded" nhưng thật ra nhận 0 vì hết trần 300.1; dòng 1, 2, 3, 6 chỉ trả một phần; Line-level cover 300.1 ghi 1.800.000 (đúng 2.600.000); AG-3 dòng 2 tiêu đề "không liên quan" nhưng giải thích "liên quan".
- 12/10 20:50: lưu trữ lại CLM-2026-0503 (Verified IsArchived=True: 1); "Reverse limit use of archived claims" → 0 bản ghi, 0 VND. ChainTests rev 636: 46/46, gate 5/5.
- Phiên Mentor ClaimMVP hiện tại: d822949c-cf20-4ce6-8704-0b26bbb91c97 (aa9b4036 treo, đã đóng).

## Dòng tiền ClaimDetail › Summary (thiết kế C, rev 647–672, 12–13/10)
- Người dùng duyệt bảng C trên canvas "ClaimDetail Summary — Why & Evidence" (claude.ai/artifact/RWXiie9QLKKNaUkhD9MKXL). Hoãn: nút "Xem trước quyết định của bạn" (cần chạy thử chuỗi không ghi).
- Server action GetMoneyFlow(ClaimId, IsEn) → MoneyFlow (Lines/Groups/Steps); chỉ hiển thị, không đổi chuỗi/sổ/AG-3/dữ liệu.
  - Thẻ 2 "Vì sao": thác nước Yêu cầu → Không thuộc quyền lợi → Vượt trần từng nhóm (đơn vị theo LimitType: lần/ngày/sự kiện) → Đồng chi trả → Được trả; bấm một bước để tô các dòng liên quan; phương trình "… = … ✓ khớp sổ". Hồ sơ từ chối cả hồ sơ (Paid = 0) → câu từ chối của chuỗi.
  - Thẻ 4 "Bằng chứng": bảng theo nhóm quyền lợi, thanh 4 màu mỗi dòng, số đã trả + ghi chú, chip Được chi trả / Hết trần / AG-3: loại? / Loại; giữ nút chứng từ gốc IDP và câu chân thẻ.
  - Từ chối cả hồ sơ: dòng có Recommendation AG-3 đã quyết → "không thuộc quyền lợi (AG-3 xác nhận)"; các dòng khác → "cả hồ sơ bị từ chối ở bước <bước FAIL đầu tiên>" (vd. Kiểm tra kỳ chờ).
- Kiểm trên trình duyệt (rev 670–671, VI và EN): 0497 = 6.500.000 − 2.400.000 (trần 200.1) − 1.600.000 (trần 300.1) − 600.000 (đồng chi trả) = 1.900.000 ✓; 0456 = 1.020.000 ✓; 0440 = 2.000.000 − 80.000 − 40.000 (trần ngày 100.1) = 1.880.000 ✓; 0445 = từ chối, dòng 4 AG-3, dòng 1–3 kỳ chờ ✓. Tab Trích xuất: hiệu ứng quét vẫn hiển thị.
- Lỗi đã gặp và sửa: 664 làm mất bước trần (CapCut nhóm bị đặt 0) → 666; 665/667 gắn "(AG-3 xác nhận)" cho mọi dòng (LineDecisionId = Decline ở mọi dòng khi từ chối cả hồ sơ) → 668 dùng Recommendation; 669 tra "WAIT" không thấy → 670 dùng bước FAIL đầu tiên trong CalcSnapshot.
- Hồi quy cổng: đâu đó trong 647–670 lời gọi FormatVND_Server trong EvaluateAutoApprovalGate bị đổi ShowSymbol = True → lý do C1 ra "1,120,000 VND", T-GATE-01/02 trượt mẫu log (gate 3/5). Rev 671 trả về ShowSymbol = False → 46/46, gate 5/5.
- Rev 672: CSS dòng Tổng thẻ 4 (flex, nowrap) — VI không còn xuống dòng. Chỉ CSS, không cần chạy lại test.
- Bài học: mỗi đợt sửa giao diện có dùng hàm định dạng chung → chạy lại ChainTests + gate ngay, không đợi cuối.
- Tab Trích xuất theo mockup (13/10, rev 673–677): cột Chứng từ gốc rộng 300–360 px (lưới v7-extract-grid); bảng gộp viền (border-collapse), độ rộng cột cố định, pill tin cậy gạch chân, dòng "Tổng các dòng" thẳng cột Yêu cầu; bỏ link "▼ Lịch sử sửa dòng" + bảng, thay bằng khối "Vết sửa (giữ giá trị gốc)" chỉ hiện khi có sửa (giờ · Dòng N · Trường: cũ gạch → mới đậm · lý do dòng dưới · người sửa); chip "Edited"; EN "Diagnosis" có mã ICD. Hiệu ứng quét vẫn chạy. ChainTests rev 677: 46/46, gate 5/5. Validation 7 lỗi nền.
- Sự cố 13/10 14:06: script kiểm thử bấm nhầm ô Xác minh dòng 1 của CLM-0497; bỏ chọn không lưu (ô chỉ ghi khi xác minh). Đã báo người dùng. Bài học: không dùng parentElement.querySelector('input') để tìm công tắc — bấm theo toạ độ hoặc id cụ thể.
- Tab Sự kiện & phạm vi theo mockup (13/10, rev 679–683): thẻ trái dạng lưới nhãn/giá trị (Ngày điều trị · số ngày, chip liên kết, Kết luận Đủ điều kiện / Từ chối — <bước>); thẻ phải chỉ liệt kê dòng bị loại đã xác nhận AG-3 (NOTCOV/NOTREL, chữ đỏ), không có thì "Không có" + số đề xuất đang chờ; từ chối cả hồ sơ → hộp đỏ "Điều khoản áp dụng" + ghi chú "câu hỏi hợp đồng"; ghi chú quy tắc câu chữ như mockup; bảng trên thêm Từ/Đến/Số ngày, bỏ "·" thừa. Kiểm 0497/0456/0440/0445 (EN). ChainTests rev 683: 46/46, gate 5/5. Còn: 0456 ngày điều trị 09/06–09/12/2026 (184 ngày) và ICD trống — dữ liệu hồ sơ, chưa sửa.
- Tab Trích xuất: ô Xác minh vẽ lại kiểu mockup (rev 678).
- Tóm tắt › thẻ 3 "Việc cần người phán đoán" theo mockup (13/10, rev 684, chỉ CSS): hàng gọn cách nhau 8px, ẩn nút Confirm/Reject trong từng mục (quyết qua "Mở →" sang tab Trích xuất), nút Trình duyệt dạng viên thuốc.
- Tab Chứng từ theo mockup (13/10, rev 685–689): ẩn ô <script> làm lệch lưới đối chiếu R1–R9 (chỉ CSS); bỏ thẻ "Trạng thái gói chứng từ", thay bằng dòng gói dưới checklist (Hoàn chỉnh & hợp lệ · lần kiểm tra cuối · lượt); nút Chạy đối chiếu một dòng; chữ phụ theo ngôn ngữ EN/VI; R5 hết lặp "VND". Nút "Xem chứng từ gốc" từng dòng nối OpenOriginalPopup(DocTypeCode) → server action mới GetOriginalZipForDoc(ClaimId, DocTypeCode) để T1-03 mở đúng phiếu khám (trước mở nhầm T1-01); dùng cache IdpRawFileCache, không đổi logic IDP. Nút "▸ Dữ liệu IDP" hiện lại (rev 689, Visible/nhãn dùng ShowIdpTools). Đã kiểm trên 0497: T1-01 → bảng kê, T1-03 → phiếu khám; ChainTests 46/46, gate 5/5 (17:19).
