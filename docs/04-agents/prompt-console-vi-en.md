# Prompt Mentor — Console ClaimMVP song ngữ EN/VI (02/10)

**Yêu cầu (build-06 §1, BR-15):** console mặc định **EN**, nút chuyển **VI**. Tên khoản mục, cơ sở KCB, tên người, log chuỗi (đã là tiếng Việt) giữ nguyên. Trong chế độ VI: mức chứng từ hiển thị "Bắt buộc / Nên có / Tùy chọn", không hiện mã `T1-nn`.
**Cơ chế:** chép đúng cách MyInsurance đã làm (rev 60–71): Client variable `Lang` (mặc định "en"), client function `T(En, Vi)`, nút EN|VI trên top bar thay nút "VI" giả (toast "Not in the pilot"). Dropdown dựng phía client (server không đọc được Client.Lang). Trạng thái hồ sơ: `If(Client.Lang = "vi", ClaimStatus.LabelVi, ClaimStatus.Label)`.
**Phạm vi đợt 1:** layout + menu + top bar, Work list, Dashboard, ClaimDetail (header, stepper, 8 tab), Approval. Màn presenter (Technical trace, Chain tests, Administration) giữ EN.

## Bảng thuật ngữ
| EN | VI |
|---|---|
| Work list | Danh sách việc |
| Dashboard | Tổng quan |
| Operations queue | Hàng đợi vận hành |
| Policies & benefits | Hợp đồng & quyền lợi |
| Term register | Sổ điều khoản |
| Checkpoint queue | Hàng đợi điểm kiểm |
| Limit ledger | Sổ hạn mức |
| Administration | Quản trị |
| Technical trace | Nhật ký kỹ thuật |
| Chain tests | Kiểm thử chuỗi |
| Queues / Reference | Hàng đợi / Tra cứu |
| Claim detail | Chi tiết hồ sơ |
| Assessment & decision | Thẩm định & quyết định |
| Proposed payable | Đề xuất chi trả |
| Outpatient / Inpatient | Ngoại trú / Nội trú |
| Provider · in-network | Cơ sở KCB · trong mạng lưới |
| Diagnosis | Chẩn đoán |
| Treatment date | Ngày điều trị |
| Claim submitted / Documents read / Assessment / Approval / Payment & notice | Đã nộp hồ sơ / Đã đọc chứng từ / Thẩm định / Phê duyệt / Chi trả & thông báo |
| General / Documents / Extraction & costs / Events & cover / Calculation & ledger / Settlement plan / Approval & payment / History & log | Chung / Chứng từ / Trích xuất & chi phí / Sự kiện & phạm vi / Tính tiền & sổ hạn mức / Phương án bồi thường / Phê duyệt & chi trả / Lịch sử & nhật ký |
| Document checklist | Danh mục chứng từ |
| Required / Expected / Optional | Bắt buộc / Nên có / Tùy chọn |
| Received / Missing / Read / Error | Đã nhận / Thiếu / Đã đọc / Lỗi |
| Request information | Yêu cầu bổ sung |
| Waive expected | Miễn chứng từ nên có |
| Read with IDP | Đọc bằng IDP |
| Package status · Complete & valid | Trạng thái bộ chứng từ · Đủ và hợp lệ |
| Recommendation · Accept / Reject | Khuyến nghị · Đồng ý / Không đồng ý |
| Claimed / Payable / Co-payment / Customer pays | Yêu cầu / Chi trả / Đồng chi trả / Khách tự chịu |
| Approve / Return to assessor | Phê duyệt / Trả về cán bộ thẩm định |
| Reading… / Replay / Read failed / Read timed out | Đang đọc… / Replay / Đọc lỗi / Quá thời gian đọc |
