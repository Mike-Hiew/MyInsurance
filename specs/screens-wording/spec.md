# Spec — Màn hình & câu chữ

> **Văn bản đầy đủ:** Project → `claude/build-06-man-hinh-va-wording`.
> Nguồn cấp trên trên đĩa: `../../Preparing/04_Nghiep-vu-va-du-lieu/A2_Message_and_Notification_Catalogue_v1.2.md` (câu) và `D2_Functional_Specification_v0.6.md` (màn).

## ⚠ Phạm vi thực tế đã vượt spec

`build-06` §0.2 cắt còn "6 màn phải có". Thực tế đã dựng **17 màn** (10 console ClaimMVP + 7 portal MyInsurance).
⇒ **53 message của `build-06` §5 không phủ hết chữ đang hiện trên màn.** Bảng đối chiếu 17 màn: Project → `trang-thai-build-2026-09-20` §8.

## Bất biến câu chữ

1. **Màn hình ĐẶT câu, không BAO GIỜ soạn câu.** Mọi câu tiếng Việt đến dưới dạng **một template A2 hoàn chỉnh đã bind placeholder**. Không nối chuỗi. Lý do: classifier và trật tự từ tiếng Việt không sống sót qua việc ghép mảnh.
2. **Một hàm render duy nhất** `(messageCode, placeholderMap, language) → string`. Mọi màn, mọi agent, mọi thư đều đi qua nó. Thiếu mã thì **fail rõ ràng**, để không ai viết tạm một câu rồi nó đi thẳng ra thư gửi khách.
3. **Không bao giờ lộ định danh nội bộ cho khách** — 13 `claimState`, 7 mã chứng từ `T1-nn`, `REQUIRED/EXPECTED/OPTIONAL`, 12 `lossType`. Render tên tiếng Việt theo A1 §6 và "Bắt buộc"/"Nên có"/"Tùy chọn".
4. **Lỗi hệ thống không bao giờ diễn đạt như một quyết định** (M-6). `SYS-ERR-001` là message lỗi duy nhất khách thấy trên một hồ sơ, phát **một lần**, không lặp.
5. **Push không bao giờ nói kết quả là gì** — luôn là *"Đã có kết quả hồ sơ {claimNo}"*.
6. **Dòng bị từ chối nằm TRONG CÙNG BẢNG với dòng được trả** trên màn kết quả. Không accordion, không "xem chi tiết".
7. **Luôn có đường khiếu nại** trên mọi kết quả bị từ chối hoặc từ chối một phần.
8. **Không `TBC-nn` trong thư gửi khách.** Một lá thư trích `TBC-04` là một văn bản có placeholder, trên letterhead công ty, về việc từ chối chi trả — và không rút lại được vì nó đã được đọc.
9. **Quyền là vắng mặt, không phải bị vô hiệu hoá.** Ô `—` trong ma trận nghĩa là nút **không tồn tại trong DOM**, và Server Action tự kiểm lại vai + scope.

## Bốn chỗ cố tình che với khách

`EXTRACTION_ERROR` → *Đang xử lý chứng từ* · `DOC_PROCESSING` treo → không thêm gì · `PAYMENT_IN_PROGRESS` treo → không thêm gì · `PENDING_APPROVAL` → *Đang thẩm định*.

## Đơn vị tiền

`1.200.000 đ` cho **mọi thứ khách đọc** (A2 M-4). Console được phép dùng `₫`.

## Checklist trước khi áp wording — chạy trên 17 màn, không phải 6

`build-06` §10. Mục quan trọng nhất: **grep HTML render của portal** với 13 `claimState` + 7 mã chứng từ + 3 mức + 12 `lossType` — không được khớp cái nào. Ba màn portal mới (`PortalHome`, `PortalBenefits`, `PortalNotifications`) chưa từng nằm trong phạm vi soát câu chữ của A2 ⇒ nhiều khả năng rò nhất.
