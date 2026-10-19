# DELTA — `specs/screens-wording` · portal song ngữ

> Đoạn khác biệt. Merge vào Project doc `build-06` §8.5 khi archive.

## Spec hiện tại (`build-06` §8.5)

> - Portal: **tiếng Việt**, không có nút chuyển.
> - Console: **mặc định tiếng Anh**, có nút chuyển VI.
> - Giữ nguyên tiếng Việt kể cả ở chế độ EN: **nội dung ảnh chứng từ, tên khoản mục trích xuất từ hoá đơn, tên cơ sở KCB, tên người**.

## Thay đổi

Dòng 1 đổi thành: **Portal: mặc định tiếng Việt, CÓ nút chuyển EN.**

Dòng 2 và dòng 3 **không đổi**. Dòng 3 là ràng buộc, không phải gợi ý — ở chế độ EN, tên khoản mục và tên cơ sở KCB vẫn phải là tiếng Việt.

**Lý do:** bài dự thi trình bày bằng tiếng Anh; giám khảo sẽ mở portal. Đánh đổi đã cân: một app bảo hiểm cho người Việt hiển thị tiếng Việt là điểm cộng về tính thật, nhưng nếu giám khảo không đọc được gì trên màn khách thì phần "cùng một con số ở hai phía" không chứng minh được.

## Ràng buộc mới kèm theo

1. **Chữ EN gửi khách không có nguồn chuẩn.** A2 là catalogue tiếng Việt. Mọi `CustomerLabelEn` / `TextEn` là **bản dịch của đội**, phải đánh dấu trong seed và trong deck — không để chữ tự chế trông như chữ đã duyệt.
2. **Ranh giới cơ chế:** chữ **là dữ liệu** (nhãn enum, câu A2) đi bằng **cột trên entity**; chữ **không phải dữ liệu** (chrome màn hình) đi bằng **ODC Translation**. Service Action không đọc được Translation — đó là lý do ranh giới nằm đúng chỗ này, không phải "mỗi màn một kiểu".
3. **Quy tắc M-3 không đổi.** Chế độ EN không cho phép hiện mã nội bộ. `CustomerLabelEn` là nhãn người đọc, không phải `Value`.

## Quyết định

> *(để trống — điền khi merge vào build-06)*
