# Mentor prompt — ClaimMVP console bilingual EN/VI (02/10)

> English translation for reviewers. Vietnamese original: [prompt-console-vi-en.md](prompt-console-vi-en.md).

**Requirement (build-06 §1, BR-15):** the console defaults to **EN**, with a **VI** toggle button. Item names, medical facility names, person names, and the chain log (already in Vietnamese) stay unchanged. In VI mode: the document level is displayed as "Bắt buộc / Nên có / Tùy chọn" (EN: "Required / Expected / Optional"), and the `T1-nn` codes are not shown.
**Mechanism:** copy exactly how MyInsurance did it (rev 60–71): Client variable `Lang` (default "en"), client function `T(En, Vi)`, an EN|VI button on the top bar replacing the fake "VI" button (toast "Not in the pilot"). Dropdowns are built client-side (the server cannot read Client.Lang). Claim status: `If(Client.Lang = "vi", ClaimStatus.LabelVi, ClaimStatus.Label)`.
**Scope of batch 1:** layout + menu + top bar, Work list, Dashboard, ClaimDetail (header, stepper, 8 tabs), Approval. The presenter screens (Technical trace, Chain tests, Administration) stay EN.

## Glossary
The VI column holds the literal Vietnamese UI strings (kept in Vietnamese; the EN column is their English meaning).

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
