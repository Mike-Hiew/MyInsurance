# Demo 08/10 · My Insurance × BnK IDP

## tai-lieu\
| File | Dùng để |
|---|---|
| 1-slide-demo.html | Bộ slide trình chiếu (←/→ chuyển slide, bấm ảnh để phóng to) |
| 2-so-tay-he-thong.html | Hiểu hệ thống: ý tưởng, kiến trúc, workflow, agent, BnK IDP, hỏi đáp |
| 3-huong-dan-su-dung.html | Hướng dẫn từng màn, từng nút, mọi trường hợp |
| 4-slide-bnk-idp.html | **Slide trình bày chính 08/10 · bản ngắn, tập trung demo**: 17 slide trình chiếu (~15 phút) + demo 20 phút + 3 ảnh dự phòng; phần chi tiết dồn vào phụ lục. Phím N bật ghi chú |
| 5-kich-ban-trinh-bay.html | Bản có kịch bản: slide bên trái, lời nói + thao tác + câu chuyển ý bên phải, đồng hồ bấm giờ. "Mở cửa sổ chiếu" để kéo slide sang máy chiếu. Phím R: đọc toàn bộ / in |
| ban-day-du\ | Bản đầy đủ 6 phần (~50 phút) của slide và kịch bản, dùng khi cần trình bày sâu |
| 5-kich-ban-trinh-bay.pdf | Kịch bản in sẵn (A4 ngang, 19 trang, gồm cả phụ lục) |

## anh-man-hinh\
Ảnh chụp màn hình thật ngày 07/10/2026 (MyInsurance rev 147, ClaimMVP rev 471).
00 login · 01 trang chủ khách · 02 tạo hồ sơ · 08 kết quả hồ sơ 0440 · 14 trợ lý khách ·
03 danh sách việc · 04 thẻ "AI đã làm gì" 0449 · 05 trích xuất IDP · 06 thác tiền · 06c chuỗi 14 bước ·
07 đối chiếu R1–R9 · 09 nhật ký kỹ thuật · 10 kết quả & ROI · 11/12 hồ sơ trùng 0451 bị chặn

## chung-tu-mau\  (ảnh để nộp trên MyInsurance)
| Thư mục | Loại tổn thất | Kỳ vọng | Nguồn |
|---|---|---|---|
| 01_tu-duyet | Bệnh – ngoại trú, ngày 08/10/2026 | Tự duyệt 320.000 đ | mẫu demo-idp, đổi ngày |
| 02_trung-ho-so | như 01 | DUPLICATE → bị chặn | y hệt 01 |
| 03_thieu-phieu-kham | Bệnh – ngoại trú | Chờ bổ sung; nộp tiếp bo-sung\ | case02 |
| 04_ag3-thuoc-khong-lien-quan | Bệnh – ngoại trú | AG-3 đề xuất 3 dòng | case03 |
| 05_du-lieu-lech | Bệnh – ngoại trú | WARN R1–R8 → chặn | case10 |
| 06_noi-tru-tien-lon | Tai nạn – nội trú | 25.606.000 đ → phê duyệt 4 mắt | case06 |
| 07_nha-khoa-tham-my | Nha khoa – ngoại trú | Tẩy trắng → điểm kiểm | case07 |
| 08_anh-xau | Bệnh – ngoại trú | Dòng tin cậy thấp | L15 |
| 09_ngay-tuong-lai | Bệnh – ngoại trú | DATE_IMPOSSIBLE → chặn | L08 |
| 10_ngoai-tru-day-du | Bệnh – ngoại trú (NGUYỄN VĂN AN) | Hồ sơ sạch 4 chứng từ, 1.089.500 đ; nộp trước L11 | case01 |
| 11_noi-tru-co-BHYT | Bệnh – nội trú (**TRẦN THỊ BÌNH**) | Nội trú 5 ngày, BHYT trả một phần, 6.297.000 đ | case04 |
| 12_tai-nan-ngoai-tru | Tai nạn – ngoại trú (NGUYỄN VĂN AN) | Bong gân, X-quang + bản tường trình, 965.000 đ | case05 |
| 13_thai-san-ngoai-tru | Thai sản – ngoại trú (**TRẦN THỊ BÌNH**) | Doạ sảy thai, siêu âm + đơn thuốc, 1.268.000 đ | case08 |
| 14_thai-san-sinh-mo | Thai sản – nội trú (**TRẦN THỊ BÌNH**) | Mổ lấy thai, có chi phí bé sơ sinh, 14.432.000 đ | case09 |
| bat-loi\L01–L17 | xem _DANH-SACH-LOI.md | Mỗi bộ cài đúng MỘT lỗi: R1–R8, ngày vô lý, quá 60 ngày, trùng, bên khác đã trả, bệnh có sẵn, thiếu ICD, ảnh xấu, hoá đơn dạng ảnh, chọn sai loại tổn thất | bat-loi |
| mau-tung-template | — | 9 ảnh lẻ 6090–6098, nộp từng ảnh để xem IDP phân loại đúng template | anh-mau |

**Xem nhanh toàn bộ 110 ảnh:** mở `chung-tu-mau\_XEM-NHANH.html` (bấm ảnh để xem cỡ thật).

Thư mục 03–14, bat-loi và mau-tung-template được chép từ:
E:\Hackaton\insurance BNK\Bnk IDP\00. Deliverables\Templates_Claim_MVP_tenant177\anh-mau
(nếu chưa có, chạy tao-thu-muc-03-09.ps1). Mỗi thư mục có _KY-VONG.md ghi thứ tự nộp và kỳ vọng.

## Lưu ý
- Chỉ bộ 01 đã chứng minh tự duyệt. Bộ 03–14 và bat-loi dùng cơ sở giả, AG-4 có thể nêu thêm tín hiệu cơ sở.
- Bộ 11, 13, 14 là của **TRẦN THỊ BÌNH**: cần tài khoản khách này trên MyInsurance (hoặc đổi tên trong _cases.py rồi vẽ lại ảnh).
- Bộ 10–14 và bat-loi giữ ngày gốc trên chứng từ (đa số tháng 9/2026; L08 cố ý để 20/10); bộ nào đã từng nộp lên tenant thì nộp lại sẽ bị AG-4 báo trùng.
- L11 phải nộp SAU bộ 10 (nó là bản trùng của bộ 10).
- Mỗi lần nộp gọi IDP thật (tốn quota, 30 giây – vài phút) và tạo hồ sơ mới.
- Hồ sơ có sẵn để mở khi demo: 0449 (tự duyệt), 0451 (trùng), 0440 (kết quả khách), 0001 (đề xuất AG-3), 0445 (kỳ chờ).
- Đăng nhập lại cả hai app trước giờ demo.
