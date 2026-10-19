# "Tải lên tất cả chứng từ — hệ thống tự phân loại" (16/10/2026, 01:00–07:00)

**Yêu cầu Mike:** khách chỉ cần tải tất cả chứng từ; hệ thống tự phân loại; thiếu chứng từ bắt buộc thì báo khách và yêu cầu bổ sung. Toàn quyền publish/test; không khả thi thì báo để quyết revert.
**Kết luận: KHẢ THI — đã chạy trọn vòng trên hệ thống thật. Giữ, không revert.**

## Bản hiện tại
- **ClaimMVP rev 769** · **MyInsurance rev 238** · ChainTests (rev 769): **46/46, gate 5/5** (16/10 05:10).
- Điểm lùi: ClaimMVP **764**, MyInsurance **224**. Lùi nhẹ: PortalNewClaim `UseAutoUpload` = False → bước 2 về giao diện chụp theo từng loại. Không revert bằng cách load revision cũ rồi publish qua Mentor.

## Kết quả test thật (16/10)
| Ca | Hồ sơ | Kết quả |
|---|---|---|
| 1 · Đủ chứng từ: thả phiếu khám + bảng kê (D2 ca1 A) cùng lúc | **CLM-2026-0509** | IDP nhận đúng 2 loại (~30 s) → gửi → **tự duyệt, Đã chi trả 320.000 ₫** ✅ |
| 2 · Thiếu: bảng kê (D2 ca1 B) + 1 ảnh không phải chứng từ | **CLM-2026-0511** | Banner "Còn thiếu: Phiếu khám bệnh" → "Gửi hồ sơ, bổ sung sau" → hồ sơ hiện "Chúng tôi cần Quý khách bổ sung: Phiếu khám bệnh (Bắt buộc), Đơn thuốc, Hoá đơn GTGT (Nên có)" → Bổ sung chứng từ (tự chọn đúng hồ sơ, chế độ Tự nhận diện) → thả phiếu khám → đọc → **tự duyệt, Đã chi trả 320.000 ₫** ✅ |
| 3 · Giấy lạ (sơ đồ kỹ thuật) | trong 0511 | "Không nhận ra giấy tờ — nếu là chứng từ y tế, vui lòng chụp lại rõ hơn"; không làm hỏng hồ sơ ✅ |
| 4 · 3 file (phiếu khám, bảng kê, đơn thuốc) | nháp | 3 nhãn đúng, banner "Đã đủ giấy tờ bắt buộc" ✅ |
| 5 · Kiểm tra cuối rev 238: phiếu khám + đơn thuốc (thiếu bảng kê) | nháp | Checklist: Bảng kê "Còn thiếu", Phiếu khám + Đơn thuốc "Đã nhận"; banner "Còn thiếu: Bảng kê chi phí…" — khớp nhau ✅ |

## Thay đổi
### ClaimMVP (765–769)
- `IdpApplyExtraction`: ClaimDocument có TemplateId "AUTO" / "AUTO-…" → sau khi IDP đọc, tra `DocumentTemplateMap` → gán mã T1-01/T1-03/…; không khớp → "UNCLASSIFIED". Ghi AgentRun "IDP · Auto-classify".
- `Portal_GetDraftDocuments2`: 1 dòng / ClaimDocument (SlotCode = mã hiện tại, nhãn "Đang nhận diện…"…). `Portal_GetMissingDocuments2` chạy được cho nháp.
- **Sửa lỗi có sẵn (lộ ra khi test):** gửi nháp đã đọc xong mà thiếu chứng từ bắt buộc → `Portal_FinalizeDraftClaim2` chạy luôn điều phối → RequestAdditionalDocuments → gọi AG-5 → quá thời gian → "Không thể gửi hồ sơ". Rev 769: Finalize chỉ làm phần nhanh + commit, đặt `Claim.OrchestratePending`, timer **FinalizeOrchestrator** chạy điều phối nền; RequestAdditionalDocuments commit trạng thái + thông báo trước khi gọi AG-5.
- 2 service action `Portal_ReadAnyDocument2`, `Portal_GetUploadStatus2` có trong ClaimMVP nhưng **không dùng** (danh mục dependency của tenant không index được) — có thể xoá sau.

### MyInsurance (225–238)
- PortalNewClaim bước 2: khối "Tải lên tất cả chứng từ" (kéo-thả nhiều file, JPG/PNG/PDF ≤10 MB) → `AutoUploadServer` (giải base64 ở server) → `Portal_ReadDocument2("AUTO-<hhmmssfff>-<i>")`; poll 3 s; chip từng file; checklist Bắt buộc/Nên có/Tuỳ chọn + Đã nhận/Còn thiếu; banner đang đọc / còn thiếu X / đã đủ; panel gửi "Gửi hồ sơ, bổ sung sau / Quay lại tải thêm". Ô cũ ẩn sau link "Hoặc tải theo từng loại giấy tờ". Stepper "tự nhận diện".
- Rev 237–238: "Đã nhận" tính ở client — `CodeForChecklistLabel(LabelVi)` (Bảng kê→T1-01, Phiếu khám→T1-03, Đơn thuốc→T1-02, Hoá đơn VAT→T1-04, Giấy tờ tuỳ thân→T2-01, Xét nghiệm→T2-02, Giấy ra viện/Báo cáo y tế→T2-03) + `IsDocReceived(Code)` (ReadState DONE). Banner và panel gửi dùng cùng logic → không còn lệch (trước đó banner báo đủ dù thiếu bảng kê; checklist lệch dòng).
- PortalSupplement: nhận `ClaimId` từ PortalClaimDetail (chọn đúng hồ sơ), loại chứng từ mặc định "Tự nhận diện (khuyên dùng)" nhiều file, danh sách "Hồ sơ đang cần" tự tick; rev 234 ẩn danh sách này khi hồ sơ đã đóng/chi trả.

## Dọn dữ liệu test
- Đã lưu trữ CLM-2026-0507 → 0514 + chạy "Reverse limit use of archived claims" (12 bản ghi, 1.280.000 ₫).
- Đã lưu trữ CLM-2026-0515 → 0519 (Verified IsArchived=True: 5).
- Ảnh D2 ca1 bộ A/B đã dùng cho 0509/0511 — 2 hồ sơ này đã lưu trữ nên buổi thật không bị AG-4 báo trùng.

## Còn lại (nhỏ, không chặn demo)
- Chip trạng thái ở thẻ hồ sơ khi đang chờ bổ sung vẫn ghi "Chờ thẩm định" (lệch có từ trước).
- 1 file PDF chứa nhiều chứng từ vẫn tính là 1 chứng từ (IDP chỉ tách theo trang khi bật split; chưa dùng).
- Xoá 2 service action thừa trong ClaimMVP khi rảnh.
