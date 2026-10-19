# DELTA — `specs/adjudication` · cổng tự duyệt

> Đây là **đoạn khác biệt đang chờ chốt**, không phải spec đầy đủ.
> Khi PO xác nhận, merge đoạn này vào Project doc `build-04` rồi archive change.

## Hiện trạng (code, rev 91)

Tab Approval của `ClaimDetail` hiển thị **4 điều kiện**:

| # | Điều kiện | Nguồn |
|---|---|---|
| G1 | Số tiền ≤ ngưỡng tự duyệt | BRD |
| G2 | Chứng từ & chất lượng trích xuất | BRD |
| G3 | Lâm sàng sạch | BRD |
| G4 | Không có cờ gian lận | BRD |

## Chuẩn nền tảng (`build-00` §4 #7)

**3 điều kiện.** Doc gốc nói BRD ghi 4 là sai và **BRD phải sửa**, nhưng chưa ghi cụ thể điều kiện nào bị gộp hoặc bỏ.

## Câu hỏi chặn cho PO/BA

1. Chuẩn là **3** hay **4**?
2. Nếu 3 — trong G1…G4, cái nào bị bỏ, hay hai cái nào gộp lại?
3. Điều kiện "1 proposal awaiting a human" là một **điều kiện của cổng**, hay là một **tiền đề** phải sạch trước khi cổng chạy? (Hai cách hiểu này cho ra 3 và 4.)

## Quyết định — PO, 21/09/2026 (Mike)

**Chuẩn là 4 điều kiện.** BRD đúng; `build-00` §4 #7 ("3 điều kiện, BRD phải sửa") **sai và phải sửa tại nguồn**.
Hệ quả: câu hỏi 2 và 3 ở trên tự đóng — không có điều kiện nào bị gộp hay bỏ, và "còn đề xuất chờ người duyệt"
là **một điều kiện của cổng**, không phải tiền đề.

**Wording:** duyệt nguyên văn hai câu đề xuất, dán nhãn "soạn cho hackathon" trong A2:
- `COPAY` — "Đồng chi trả 20% theo hợp đồng: {amount}"
- `CAP_VISIT` — "Phần vượt trần {cap} mỗi lần khám của quyền lợi {benefit}: {amount}"

`DEDUCT` và `LIMIT_TOTAL` vẫn trống — không có trong hồ sơ demo, đẩy sang batch F.

## CÒN MỘT CÂU CHƯA ĐÓNG — 4 nào?

Con số đã chốt. **Danh sách thì chưa.** Bảng doc ở trên là G1 tiền · G2 chứng từ · G3 lâm sàng · G4 gian lận.
Audit rev 47 đọc màn Approval thấy: *"condition 1 ✕ (co-pay cut), condition 2 '1 proposal awaiting a human' ✕, 3–4 ✓"*
⇒ **điều kiện #2 trên màn hình là "đề xuất chờ người duyệt", không phải "chứng từ"**, và #1 đang fail vì cắt co-pay
chứ không phải vì vượt ngưỡng tiền.

Nghĩa là màn hình và tài liệu đang có **hai bộ 4 điều kiện khác nhau**, cùng đếm ra 4. Đây đúng họ lỗi đã ăn của dự án
này nhiều lần (hai thứ cùng tên, cùng hình dạng, khác nội dung).

**Việc chặn:** đọc `EvaluateAutoApprovalGate` trên tenant, liệt kê 4 điều kiện **như code đang tính**, rồi đối chiếu
với bảng G1–G4. Chốt một danh sách duy nhất trước khi viết T-GATE.
