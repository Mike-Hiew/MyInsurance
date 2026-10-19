# Change — Nối IDP vào hồ sơ thật (staff + portal) + IdpTables

**Mở:** 01/10/2026 · **App:** ClaimMVP (rev 215 → 222), MyInsurance (rev 52 → 54) · **PO:** Mike chọn "cả hai" (ClaimDetail trước, portal sau)

## Vì sao
Đọc model rev 215 (Mentor, 01/10): `IdpSubmitSync` chỉ được gọi từ nút chẩn đoán trên Administration; `IdpSubmitDocument` không ai gọi;
không action nào ghi kết quả trích xuất vào `ClaimDocument` (RawExtractionJson/Confidence/Completeness/IdpRequestId có cột nhưng chưa ai ghi);
portal nộp hồ sơ tạo 5 ClaimDocument tên cứng, confidence 0.96 cứng; nút "Capture/Upload" trên portal là stub "not in the pilot".

## Đã làm
- [x] rev 215 — `IdpParseExtraction`: FIELDMAP_MISSING chỉ khi template không có dòng map nào (`FieldMapRowTotal = 0`); 0 mandatory ⇒ Completeness = 1.
      Harness: 6093 → 0.5544 / 1 / không lỗi; 6090 giả → không lỗi, Completeness 1, ClaimConfidence 0; 9999 → FIELDMAP_MISSING.
- [x] rev 216 — `IdpSubmitSync` thêm output `RawJson` (live + replay). Action mới `IdpReadClaimDocument(ClaimDocumentId, Document, FileName)`:
      gọi IdpSubmitSync (tôn trọng ReplayMode, key = ClaimNo) → ghi ClaimDocument.RawExtractionJson/Confidence/Completeness/IdpRequestId/UploadedOn → WriteAgentRun (AgentName IDP).
- [x] rev 217 — ClaimDetail tab Documents: panel "Read a document with IDP" (dropdown chứng từ + Upload + nút).
- [x] rev 218 — SỬA: không ghi đè `ClaimDocument.TemplateId` (cột này là MÃ CHECKLIST T1-01…). Service action `Portal_ReadDocument2(ClaimId, DocumentCode, Document, FileName)`.
- [x] Live 01/10 14:38 (CLM-2026-0004, 13.021 byte PNG): login 474 ms + user/me OK; IdpUpload.SubmitSync **TIMEOUT 55 s**; app xử lý đúng (không ghi ClaimDocument, AgentRun #1321 failed, log không token).
- [x] IdpReplay key `CLM-2026-0004` (POST /request/sync) = body 6093 (21408) để kiểm luồng ghi 0 quota.
- [x] Library `IdpTables` (C#) viết xong: `extension/basic_template/IdpTables` + 7 test + `build-idptables.ps1`. Thuật toán kiểm bằng bản Python trên 3 fixture: 0/0, 1/13, 1/13 đúng SRD §5.
- [x] Mike build + publish + release IdpTables **1.1.0** (01/10). rev 219 — ClaimMVP tham chiếu IdpTables, `HarnessParseTables` + nút "Run IdpTables.ParseTables": mẫu 5127 → Ok, 1 bảng, 13 ô.
- [x] rev 220–222 — `IdpTablesToClaimLines(ClaimDocumentId, RawJson, DryRun=True)` + `IdpFlushCostRow` (gom ô theo TemplateId/TableIndex/RowIndex, map theo ColumnHeader → IdpFieldMap TargetEntity=ClaimLine; bỏ dòng tên rỗng/"---", dòng bắt đầu "Cộng"/"Tổng", tiền = 0;
      không tạo nếu hồ sơ đã có dòng không-IDP; idempotent theo ClaimDocumentId + SourceDoc "IDP <tpl> T<t> R<r>"; Category/BenefitCode rỗng, không đặt quyết định, không đổi tiền Claim).
      Lỗi đã sửa: rev 220 flush lặp dòng 1 (~40 dòng R1) → rev 221 tách IdpFlushCostRow; rev 221 dòng "Cộng" không bị bỏ (Substr 0-based) → rev 222 dùng Index(ToUpper(..)).
      Harness Administration "Preview cost lines (dry run)", body tổng hợp 5127 (4 dòng): **RowsRead 4 · Skipped 2 · R1 Khám bệnh 300000 conf 0.97 · R2 Xét nghiệm công thức máu qty 2 1400000 conf 0.88** — đúng kỳ vọng.
- [x] rev 222 — `IdpReadClaimDocument` (nhánh thành công): CommitTransaction → IdpTablesToClaimLines(DryRun=False) → LineItemsExtracted = RowsRead − RowsSkipped; Summary/AgentRun thêm "cost lines: n created, m skipped". Lỗi trong bước dòng chi phí không làm mất kết quả trích xuất.
- [x] Ghi chú: replay `CLM-2026-0004` là giấy ra viện 6093 (21 field, **0 bảng**) và hồ sơ đã có dòng không-IDP → kiểm ghi trên hồ sơ này chỉ chứng minh trích xuất + chặn an toàn, không sinh dòng.

## Còn lại
- [ ] Kiểm luồng ghi bằng ReplayMode trên CLM-2026-0004 (Mike bật ReplayMode).
- [ ] Kiểm ghi dòng chi phí thật (DryRun=False) trên một hồ sơ CHƯA có dòng, có fixture bảng kê — cần PO chọn/tạo hồ sơ test.
- [x] MyInsurance rev 52–53 — PortalSupplement: khối "Attach a document" (Document type: Cost statement/Prescription/Examination record/Identity document/Lab result → T1-01/T1-02/T1-03/T2-01/T2-02, chỉ hiện nhãn) + Upload;
      OnSend: Portal_SendSupplement3 chỉ khi có lời nhắn; có file → Portal_ReadDocument2(SelectedClaimId = dropdown Claim) → hiện Message (Success/Warning); Send bật khi có lời nhắn hoặc file. Đã xem trên màn (khách Nguyễn Văn An).
      Ghi chú: dòng "Discharge paper · Mandatory" là text cứng chỉ hiện cho CLM-2026-0001, không gắn mã chứng từ.
- [x] MyInsurance rev 54 — PortalNewClaim bước 2: 5 dòng (Cost statement, Examination record, Identity, Prescription, Lab) có Upload thật; VAT invoice (XML) giữ nguyên.
      OnSubmitClaim: sau Portal_SubmitClaim3 (NewClaimId) gọi Portal_ReadDocument2 cho từng file → "Your claim was submitted. We have read N of M documents." Không lộ mã/lỗi. Đã xem bước 2 trên màn, CHƯA bấm Submit (sẽ tạo hồ sơ thật).
- [ ] Portal: chữ phụ "2 photos · sharp" / "1 photo · sharp" / dấu ✓ ở bước 2 là text cứng (mock cũ) — nên đổi thành tên file thật / "chưa đính kèm".
- [ ] Portal end-to-end: hồ sơ mới có ClaimNo mới → không có IdpReplay → khi ReplayMode=True sẽ không đọc được; khi False sẽ gọi live (≤55 s/file, tốn quota). Cần PO chọn cách demo.
- [ ] Live lần 2 khi IDP hết chậm (A1–A3, A8). Thời gian sync >55 s là rủi ro demo: cân nhắc dùng ReplayMode khi pitch.
- [ ] Hiển thị Completeness/IdpRequestId trên bảng checklist (hiện chỉ có Confidence).
