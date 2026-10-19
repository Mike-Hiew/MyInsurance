# Delta — Màn hình: trạng thái màn và nhãn console

> Áp vào `openspec/specs/screens-wording/spec.md` và Project doc `build-06` khi archive change này.

## Thêm — trạng thái bắt buộc của mọi màn danh sách/biểu mẫu

10. **Mỗi màn danh sách có đủ 3 trạng thái**: *đang tải* (skeleton `cc-skel`, không spinner toàn trang), *rỗng* (`cc-empty`: icon + câu + một CTA — "No claims match these filters / Clear filters", "No claims yet / New claim", "You're all caught up"), *có dữ liệu*. Bảng **biến mất** khi rỗng, không hiện header + phân trang trống.
11. **Mỗi hành động ghi có phản hồi tại chỗ**: nút `is-loading` + disabled trong lúc chạy; kết quả bằng toast (`cc-toast`, tự ẩn 6 s) hoặc dòng `cc-field-ok` ngay dưới nút; hành động hàng loạt có **Undo** gọi đúng hành động ngược. Toast nói **con số thật** ("No unassigned claims right now" khi 0), không nói số tổng.
12. **Biểu mẫu**: mọi input có `Label` thật (`<label for>`); validate on-blur, thông báo là câu (A2), "Looks good" khi hợp lệ; nút tiếp tục disabled tới khi đủ điều kiện. Wizard có **bước hoàn tất thật** ("Claim received" + số hồ sơ đã lưu + Track/My claims), không dừng ở bước "đang xử lý".
13. **Câu phụ đề phải đúng với filter đang áp**: "N claims · all handlers" khi không lọc theo người; chỉ nói "assigned to me" khi thật sự lọc.

## Nhãn trạng thái trên console (cán bộ) — không lộ định danh enum

| `claimState` | Nhãn console |
|---|---|
| `Received` | Received |
| `Extracting` | Extracting |
| `ExtractionError` | Extraction error |
| `PendingApproval` | Pending approval |
| `UnderReview` | Under review |
| `Approved` | Approved |
| `Declined` | Declined |
| `Paid` | Paid |

Bảng ánh xạ cho **khách** (11 trạng thái tiếng Việt) giữ nguyên theo `build-06`; bảng trên chỉ cho WorkList/ClaimDetail của ClaimMVP (song ngữ VI/EN chưa chốt — vẫn là việc của `README.md` §4 "cơ chế song ngữ").
