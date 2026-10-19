# Kiểm chứng: "Upload cả gói chứng từ, BnK IDP tự phân loại và trả về có những chứng từ nào"

**Ngày:** 16/10/2026 · **Phạm vi:** chỉ đọc source + tài liệu trong `PMOInsure` (chưa chạy trên hệ thống)
**Kết luận:** ✅ **Có thật** — nhưng chỉ đúng khi cả gói nằm trong **một file PDF nhiều trang**, và có 6 điều kiện phải thiết kế quanh.

## 1. Code thực sự làm gì

Đường chạy: `POST /request` → ocr-service → Kafka → `llm-du/src/document_processing.py::run_document_processing_pipeline` → `du_flow` → `_process_standard_templates` → `process_classify_v3` (`llm-du/src/process_classify.py:1096`).

| Bước | Làm gì | Model |
|---|---|---|
| 1 | OCR toàn file, tách nội dung theo trang | OCR engine |
| 2 | **Tách ranh giới chứng từ:** so **từng cặp trang liền kề** (1-2, 2-3…) bằng ảnh, hỏi "cùng một chứng từ không?" (`CLASSIFY_DOCUMENT_PAGE_PROMPT_VISION`). Không chắc → coi là khác | Gemini vision |
| 3 | Gom các trang liền nhau thành nhóm = 1 chứng từ | — |
| 4 | **Phân loại từng nhóm** vào **một** template trong danh sách `templates` truyền vào (`MATCH_PROMPT_V3`, dựa trên **mô tả template**). Không khớp → `"Unknown"` | Gemini (text OCR) |
| 5 | Trích xuất field/bảng cho từng nhóm theo template đã chọn | LLM DU |
| Output | Mỗi chứng từ = 1 phần tử `templates_data[]`, tên `SET{n}_Custom-T…\|Tên`; cùng loại xuất hiện 2 lần → `SET1_`, `SET2_` | — |

→ "Trả về có những chứng từ nào" = **danh sách `templates_data`**. Không có endpoint riêng kiểu "classify-only".

## 2. Sáu điều kiện / giới hạn (đọc từ code)

| # | Giới hạn | Bằng chứng | Hệ quả cho ClaimMVP |
|---|---|---|---|
| 1 | **1 request = 1 file.** "Upload all" phải là **1 PDF nhiều trang** | `request.controller.ts` `@Post()` dùng `FileInterceptor('document')` | Nhiều ảnh rời phải ghép thành PDF trước khi gửi |
| 2 | **`merge-multiple` KHÔNG dùng được cho việc này** — nó ghép dọc ≤10 **ảnh** thành **1 file PNG = 1 trang** → bước 2 không chạy → cả gói thành **1 chứng từ**, 1 template | `merge-image.service.ts` (`sharp`, `mode 'vertical'`, xuất `.png`) | Tránh `merge-multiple`; chỉ nhận ảnh, PDF bị lỗi |
| 3 | **Tập đóng.** Chỉ chọn trong template truyền vào. Nhóm `Unknown` bị **bỏ im lặng**, không có danh sách "unclassified". Không nhóm nào khớp → `templates_data = []` | `process_classify.py` ~dòng 1432 (`unknown_count += 1 … skipping`), lọc `{k:v … if v}` | Bộ 6090–6098 không có template "Khác" → giấy tờ tầng 3 **biến mất** khỏi kết quả. Phải tự đối chiếu số trang gửi vs số trang được nhận |
| 4 | **Chỉ 1 template → không phân loại**, ép cả file vào template đó | `apply_single_template_fallback` | Luôn gửi đủ danh sách template |
| 5 | **Chỉ so trang liền kề.** Thứ tự trang quyết định. Chứng từ bị xen giữa bị cắt thành 2 SET; 1 ảnh chụp 2 giấy = 1 chứng từ | Bước 2 chỉ duyệt `page_numbers[i], [i+1]` | Hướng dẫn khách: mỗi trang một giấy, giữ thứ tự |
| 6 | **Không trả khoảng trang** của từng chứng từ (`page_range` chỉ thêm cho template AIG) | `_process_standard_templates` | Muốn "chứng từ nằm trang mấy" phải suy từ `page_number` của field |

**Tham số liên quan:**
- `separatorType` chỉ hỗ trợ **`"number"`** = cắt cứng mỗi N trang, chạy **trước** bước AI (`process_split.py`). Không phải tách thông minh → không gửi. (Đóng câu hỏi mở số 10 ở build-02 §13.)
- Giới hạn trang/request: env `limitPagesPerRequest` (lỗi `1002`) — **giá trị thật chưa biết**. `MAX_FILE_SIZE` env. Định dạng chỉ `jpg/jpeg/png/pdf` → XML hóa đơn T1-04 không đi đường này.
- Chi phí: PDF N trang = (N−1) lời gọi vision so cặp + 1 lời gọi phân loại mỗi nhóm + trích xuất; quota tính theo trang như cũ.
- Hậu xử lý riêng chỉ có cho OCB và AIG (hard-code theo tên template); y tế không có.

## 3. Đã có bằng chứng chạy thật tới đâu

| Đã có | Chưa có |
|---|---|
| 17/09 tenant 177: 9/9 chứng từ phân loại đúng — nhưng **mỗi request 1 ảnh** (chỉ kiểm bước 4) | **Chưa từng chạy 1 PDF nhiều chứng từ** → bước 2 (tách ranh giới) chưa được kiểm trên bộ y tế |
| MBAL: vòng 2 đo 0/100 phân loại sai | Không rõ MBAL đo theo từng file hay cả gói |

## 4. Đề xuất test khi lên hệ thống (≈ 12–15 trang quota)

| Case | Input | Kỳ vọng theo code |
|---|---|---|
| A | 1 PDF 5 trang: bảng kê (2 trang) + đơn thuốc + phiếu khám + CCCD, templates = 6090–6098,5397,5443,5398 | 4 phần tử `templates_data`; bảng kê là 1 SET |
| B | Như A nhưng chèn 1 trang "giấy tờ khác" (biên lai) | Trang đó **biến mất**, không báo lỗi |
| C | 2 bảng kê khác bệnh viện liền nhau | `SET1_` và `SET2_` cùng template 6091 |
| D | Đảo thứ tự: bảng kê trang 1, đơn thuốc, bảng kê trang 2 | Bảng kê bị cắt thành 2 SET |
| E | 3 ảnh qua `merge-multiple` | 1 phần tử duy nhất |
