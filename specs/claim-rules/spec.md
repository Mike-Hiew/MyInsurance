# Spec — Nghiệp vụ & rule

> **Văn bản đầy đủ:** Project → `claude/build-03-nghiep-vu-va-rule`.

## Quy mô

8 loại chứng từ (3 tier) · **88 field** · rule đối chiếu **R1–R9** · 6 `packageState` · term engine · ~45 rule `BR-*`.

## Tám loại chứng từ

| Tier | Mã | Hành vi |
|---|---|---|
| 1 | `T1-01` bảng kê chi phí · `T1-02` đơn thuốc · `T1-03` phiếu khám · `T1-04` hóa đơn VAT | Trích xuất đầy đủ. **`T1-04` chỉ nhận XML**, không có đường OCR |
| 2 | `T2-01` giấy tờ tùy thân · `T2-02` KQ xét nghiệm/CĐHA · `T2-03` báo cáo y tế/GCN phẫu thuật · `T2-04` | **Chỉ phân loại** |
| 3 | — | Không xử lý |

**`T1-01` line-item là 17 cột, không phải 14.** `L15` ngày theo dòng · `L16` ICD theo dòng · `L18` số ngày điều trị (`L17` cố ý bỏ trống). Cả ba mang `Prov = X?`, IDP **chưa xác nhận**.

## Sáu `packageState`

`COMPLETE` · `MISSING_DOCUMENT` · `MISSING_FIELD` · `INCONSISTENT` · `AWAITING_ORIGINAL` · `ERROR`.
Engine **tiêu thụ** trạng thái này, không tự suy lại.

## Ba mức cần thiết, không phải hai

*Bắt buộc* chặn giải quyết hồ sơ · *Nên có* sinh yêu cầu bổ sung, cán bộ được miễn · *Tùy chọn* không bao giờ được hỏi và không bao giờ chặn.

## Term engine — một nhánh đang chết

**10 term pilot đều mang `clauseRef` = `TBC-nn`**, mà `BR-TERM-10` cấm từ chối viện dẫn một clause tạm.
⇒ **Nhánh auto-decline `EXCL` không chạy được.** Demo dùng `NOTCOV` / `WAIT`.
Nếu giám khảo hỏi: trả lời rằng đó là checkpoint cho người — đây là câu trả lời mạnh, không phải điểm yếu.

## Kiểm tra chất lượng ảnh KHÔNG chặn

Mờ / mất cạnh / loá đều cho **hai nút: Chụp lại và Dùng ảnh này**. *Dùng ảnh này* luôn có sẵn.
Lý do: ảnh xấu có đường khôi phục, một cú chụp bị từ chối thì không tạo ra gì cả.
