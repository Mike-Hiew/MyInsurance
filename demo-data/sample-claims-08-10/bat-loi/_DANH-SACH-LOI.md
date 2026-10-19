# Bộ ảnh bắt lỗi — mỗi case cài đúng MỘT lỗi

Nền mỗi case là một hồ sơ ngoại trú sạch, nhỏ (thường ≤ 2.000.000 đ), đủ chứng từ bắt buộc, ngày khác nhau.
**Nếu lỗi không bị bắt, hồ sơ có thể đi thẳng tới tự duyệt** — xem ClaimMVP: tín hiệu AG-4, màn CrossCheck R1–R9, đề xuất AG-3, cổng 4 điều kiện.

| Case | lossType | Lỗi cài | Ai phải bắt | Ảnh |
|---|---|---|---|--:|
| `L01_R1_ten-benh-nhan-lech` | `ILL_OP` | Bảng kê mang tên một người khác | Đối chiếu chéo R1 → AG-4 CROSSCHECK_FAIL | 3 |
| `L02_R2_ngay-sinh-lech` | `ILL_OP` | Ngày sinh trên bảng kê khác phiếu khám | Đối chiếu chéo R2 → AG-4 CROSSCHECK_FAIL | 3 |
| `L03_R3_co-so-y-te-lech` | `ILL_OP` | Bảng kê do một cơ sở khác (ở tỉnh khác) phát hành | Đối chiếu chéo R3 → AG-4 CROSSCHECK_FAIL | 3 |
| `L04_R4_don-thuoc-ngoai-dot` | `ILL_OP` | Đơn thuốc ghi ngày 21/08, trước ngày khám 08/09 | Đối chiếu chéo R4 → AG-4 DATE_IMPOSSIBLE | 3 |
| `L05_R5_tong-bang-ke-sai` | `ILL_OP` | Dòng tổng in trên bảng kê lớn hơn cộng các dòng | Đối chiếu chéo R5 → AG-4 CROSSCHECK_FAIL | 3 |
| `L06_R7_thuoc-don-khong-co-tren-bang-ke` | `ILL_OP` | Đơn kê Cefixim + Loratadin, bảng kê lại tính Azithromycin + Methylprednisolon | Đối chiếu chéo R7 → AG-4 (LOW) + AG-3 | 3 |
| `L07_R8_chan-doan-lech` | `ILL_OP` | Bảng kê ghi K21.9 trào ngược dạ dày, phiếu khám + đơn ghi J20.9 | Đối chiếu chéo R8 → AG-4 CROSSCHECK_FAIL; AG-3 | 3 |
| `L08_ngay-kham-o-tuong-lai` | `ILL_OP` | Mọi chứng từ ghi ngày 20/10/2026 — sau ngày nộp | AG-4 ValidateClaimDates → DATE_IMPOSSIBLE (FUTURE_DATE) | 3 |
| `L09_vao-vien-sau-ra-vien` | `ILL_IP` | Bảng kê nội trú: vào viện 26/09, ra viện 22/09 | AG-4 ValidateClaimDates → DATE_IMPOSSIBLE (ADMIT_AFTER_DISCHARGE) | 2 |
| `L10_ngoai-tru-qua-60-ngay` | `ILL_OP` | Một bảng kê ngoại trú gộp 4 lần khám từ 05/07 đến 25/09 (82 ngày) | AG-4 ValidateClaimDates → OP_WINDOW_OVER_60 | 3 |
| `L11_trung-ho-so-case01` | `ILL_OP` | Nộp lại y hệt bộ chứng từ case01 (cùng người, cơ sở, ngày 18/09) | AG-4 FindDuplicateClaims → DUPLICATE (HIGH) | 4 |
| `L12_ben-khac-da-chi-tra` | `ILL_OP` | Bảng kê ghi "Nguồn khác chi trả 500.000 đ" (công ty bảo hiểm khác bảo lãnh) | Chuỗi 14 bước (trừ otherPayerShare) + AG-4 OTHER_PAYER_PRESENT | 3 |
| `L13_benh-co-san-thuoc-man-tinh` | `ILL_OP` | Khám viêm họng nhưng kèm 1 tháng thuốc tiểu đường + huyết áp và xét nghiệm HbA1c | AG-3 (lâm sàng) + conditionClass PRE | 3 |
| `L14_kham-suc-khoe-khong-ICD` | `ILL_OP` | Khám sức khoẻ định kỳ, không có bệnh, không có mã ICD | AG-3 / chuỗi (BR-CHAIN-1) — thiếu ICD + NOTCOV | 3 |
| `L15_anh-chup-kem` | `ILL_OP` | Bảng kê chụp nghiêng, mờ, tối một góc và mất phần chân trang (dòng tổng) | Kiểm ảnh lúc chụp (DOC-VAL-001/002) + IDP confidence thấp → MISSING_FIELD | 3 |
| `L16_hoa-don-GTGT-dang-anh` | `ILL_OP` | Khách chụp ảnh hoá đơn GTGT thay vì nộp file XML | Luật T1-04 chỉ nhận XML (AG-1/AG-5 nhắc khách) — không OCR | 4 |
| `L17_chon-tai-nan-nhung-la-benh` | `ACC_OP` | Khách chọn lossType Tai nạn, chứng từ là bệnh thoái hoá mạn tính (M47.8), không có tường trình/tổn thương | AG-3 + ràng buộc ACC_* ↔ conditionClass ACC | 3 |

Thứ tự gợi ý: nộp `case01` (hồ sơ sạch) trước để có mốc, rồi các case L01–L17. L11 BẮT BUỘC sau case01.
Mọi ảnh là chứng từ **giả lập** (chân trang ghi rõ); tên, số giấy tờ, cơ sở y tế đều bịa.
