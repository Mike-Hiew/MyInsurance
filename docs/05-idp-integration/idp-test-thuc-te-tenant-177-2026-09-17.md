# Test API BnK IDP thật — tenant 177 "Phòng 2" (OutSystems) — 17/09/2026

**Tài khoản:** user 204, vai Tenant (roleId 2), tenant 177 (tạo 02/09, hết hạn **2026-10-31**, blockLimit 1), partner 40. Mật khẩu KHÔNG lưu ở đâu.
**Chứng từ test:** ảnh PNG giả lập vẽ bằng canvas (không PII). 4 request (21388, 21389, sync 21391, 21392) = 4 trang quota. 1 phiên đánh giá (1405). Bộ chứng từ test 196 đã xoá; bộ **198 HEALTH_CLAIM_OS_V1 giữ lại để dùng**.
**Chi tiết từng bước:** `Integration_Inventory_v1.3.xlsx` sheet "Test 17-09 — tenant 177" (32 dòng), "Tenant 177 — hiện trạng" và **"Template tenant 177"** (map id cũ → mới).

## CẬP NHẬT 08:45 — việc chặn số 0 ĐÃ TỰ GỠ, không cần IDP admin

Vai Tenant được phép `POST /template` → tái tạo 9 template Claim MVP trên tenant 177 từ định nghĩa JSON đã lưu trong `IDP_API_Spec/request-response-json` (GET__template_5383/5384, template-map 5127, get-sync-templates-bo cho BO149/175/2245/2641/230/2651). Template mới type 3, tenantId null, hiện trong `/template` list (45 mục).

| Chứng từ | Tên | **Id mới (177)** | Id gốc | Field/bảng |
|---|---|---|---|---|
| T1-01 | Bảng kê chi phí khám chữa bệnh | **6091** | 5383 | 26 / 1 — thêm quy tắc "chỉ lấy chữ số" |
| T1-02 | Đơn thuốc | **6090** | BO 149 | 12 / 1 |
| T1-05 | Giấy ra viện | **6092** | 5127 | 16 / 1 |
| T1-05 (VNVision) | VNVision-Giấy ra viện | **6093** | 5384 | 21 / 0 |
| T1-03 | Phiếu khám bệnh | **6094** | BO 175 | 23 / 0 |
| T2-02 | Kết quả cận lâm sàng | **6095** | BO 2245 | 16 / 0 |
| T2-03 | Báo cáo y tế | **6096** | BO 2641 | 19 / 0 |
| T2-03 (phẫu thuật) | VNVision-Giấy chứng nhận phẫu thuật | **6097** | BO 230 | 19 / 0 |
| T2-04 | VNVision-Bản tường trình | **6098** | BO 2651 | 12 / 0 |
| T0 | CMND/CCCD · Passport · Giấy khai sinh | 5397 · 5443 · 5398 | (có sẵn) | |
| Document set | HEALTH_CLAIM_OS_V1 | **198** | 194/195 (tenant 176) | 12 template |

**Xác thực (request 21392):** nộp 1 ảnh "BẢNG KÊ CHI PHÍ KHÁM BỆNH, CHỮA BỆNH" giả lập với `templates="6090,…,6098"` → Completed 16,82 s, **phân loại 6091**, 26 field (Tổng chi phí `5820000`, BHYT `4656000`, người bệnh `1164000`, ICD `K35.8`, Diagnosis Date `2026-09-12`), bảng 13 cột × 5 dòng với Đơn giá/Thành tiền **đủ chữ số** (`150000`, `4500000`, `1500`). → Quy tắc "chỉ lấy chữ số, bỏ dấu phân cách" trong mô tả field có tác dụng; lo ngại "300.000 → 300" ở kết luận 4 được gỡ **cho template 6091** (vẫn đối chiếu `get-ocr` theo build-02).

**Cho adapter (CLM_IDP_Int):** `IdpTemplateSetId = 198` chỉ để quản lý — `POST /request` vẫn truyền CSV `6090,6091,6092,6093,6094,6095,6096,6097,6098`. DocType crosswalk theo cột "Id mới". `templates_data = []` vẫn là EXTRACTION_ERROR.

**Chưa tái tạo (không chặn demo 3 hồ sơ):** 5299/5340/5312/5218/5125/5126/5128/5130 (hoá đơn GTGT, giấy nghỉ việc BHXH, claim form…) vì nguồn đã cung cấp không có định nghĩa. T1-04 XML hoá đơn điện tử là parser của ta. Việc còn lại (tuỳ chọn) cho IDP: nạp `mbal/template-map` cho 6090–6098 nếu muốn flow trả `subDocumentType`.

## Kết luận chính (08:21–08:26)

1. **Cơ chế I-07 / I-09 / I-15 / I-16 chạy được trên tenant 177.** Nộp → poll → kết quả: 12,85 s / 1 trang, phân loại đúng (5400 trong 3 template), confidence 96,61. Flow `5ed8e4fc` chạy 72 s (03/09 là ~30 s), 20 mục, `/result/json` 200 ngay lần đầu. `/request/sync` xong sau 13,6 s.
2. **Tenant 177 CHƯA có template y tế** (lúc 08:21). `/template/{5383,5384,5127,…}` → 404; `/template/compact` chỉ 36 template demo chung; 0 document set. → **Đã gỡ** bằng cách tái tạo (mục trên).
3. **Bẫy nguy hiểm nhất:** nộp với template chưa assign (`5383,5127`) → request **Completed, errorCode null, `templates_data = []`, vẫn mất quota**. Adapter phải coi `templates_data` rỗng là **lỗi cấu hình → EXTRACTION_ERROR**, không phải "không phân loại được".
4. **Số tiền trong bảng bị trích sai với template không có quy tắc:** OCR thô đọc đúng `300.000`, extraction của template 5400 trả `300`. Với 6091 (có quy tắc) trả đủ chữ số — luôn đối chiếu với `get-ocr`.
5. **Flow bịa `HospitalCode`:** hoá đơn không có bệnh viện nhưng flow trả `01001` (= BỆNH VIỆN HỮU NGHỊ, dòng đầu bảng 322). Không dùng HospitalCode/PlaceOfTreatment từ flow — tra bảng 322 thay (đã có trong build-02 §9.7).
6. **Nội dung flow đã đổi so với 03/09:** thêm "Kiểm tra thông tin cá nhân" (warning_level 1) và "Kiểm tra mã bệnh bẩm sinh". Cùng `workflowId` nhưng khác nội dung → yêu cầu IDP: sửa flow = bump workflowId (đã có trong câu hỏi mở).
7. **Kết quả `/result` đọc lại được** sau Completed (FO đã lưu DB); "chỉ đọc 1 lần" là ở cache ocr-service. Vẫn copy 1 lần vào `RawExtractionJson` vì `PUT /result` sửa được.
8. **`get-ocr` box:** PIXEL với ảnh (PNG), INCH với PDF (03/09). Adapter phải nhìn loại file.
9. Khẳng định lại: `templates` mảng → 400 NaN; `autoStart` bị bỏ qua; `document-set-list` không `sort` → 400; `table-data/{id}` → 400; cancel/retry trên request đã xong → 400; tạo document set nhận cả template chưa assign (không validate); body document set là `templates:[number]` (không phải `templateIds`).
10. Quota: `pageUsed` 0 → 4, tính theo trang, cập nhật khi Completed. Trần trang/block chưa biết. `/tenant/compact?withUsage` → 403 với vai Tenant.

## Việc còn lại cho đội IDP (không chặn)
- Gia hạn tenant 177 sau 31/10 nếu pilot.
- Hỏi trần trang/block và tăng nếu cần cho demo 3 hồ sơ.
- (Tuỳ chọn) `mbal/template-map` cho 6090–6098; export định nghĩa 5299/5340/5312/5218/5125/5126/5128/5130 nếu cần thêm loại chứng từ.
