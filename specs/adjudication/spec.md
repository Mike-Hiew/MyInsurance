# Spec — Chuỗi tính tiền & seed

> **Văn bản đầy đủ:** Project → `claude/build-04-chuoi-tinh-tien-va-seed` (~101 KB).
> **Chuẩn gốc: A5 §6.1 + D1 §5.3.** Ở đâu BRD hackathon (`CH-01…CH-14`) lệch A5, **A5 thắng và BRD phải sửa.**

## Bất biến

1. **14 bước, mỗi bước một Server Action.** Chuỗi là deterministic: cùng đầu vào → cùng đầu ra, chạy 2 lần cho cùng kết quả.
2. **Làm tròn: đồng nguyên gần nhất, ở TỪNG BƯỚC.** BRD ghi "làm tròn xuống 1.000 ₫ ở một bước riêng" — **sai**.
3. **Không vá số tiền. Chạy lại chuỗi.** So sánh trước/sau là so **hai lần chạy thật**, không phải `confirmed ? P2 : P1`.
4. **Khuyến nghị lâm sàng ở `Proposed` cho tới khi một người `Confirmed`** với danh tính và thời điểm. Chỉ khi đó bước áp nó vào tiền mới chạy.
5. **Chỉ báo số liệu cũ:** so hash đầu vào; không khớp → banner + **khoá nút trình duyệt**.

## Bốn chỗ BRD lệch A5 (đều làm đổi tiền)

| # | BRD | A5 |
|---|---|---|
| 1 | Có bước khấu trừ BHYT | **Đã loại bỏ** |
| 2 | Có bước Deductible | **Sản phẩm không có** |
| 3 | Làm tròn xuống 1.000 ₫ ở một bước riêng | Đồng nguyên, từng bước |
| 4 | Gộp mất 2 bước term engine | Giữ đủ |

Bảng đối chiếu chi tiết: `build-04` §1.1.

## Cổng tự duyệt

**4 điều kiện** — PO chốt 21/09. A5 §6.4 có 3; BRD §9.1.3 thêm G4 (fraud, vì slice hackathon có AG-4); bộ phải hiện thực hoá là **hợp nhất = 4**, và màn Approval đã dựng đúng. Nội dung 4 điều kiện: `build-04` §3.1. **CÒN MỞ:** màn hình và bảng G1–G4 đang liệt hai bộ khác nhau cùng đếm ra 4 — phải đọc `EvaluateAutoApprovalGate` và lấy code làm chuẩn.
→ Xem `changes/active/approval-gate-4-conditions/`.

## Seed

51 benefit + 51 waiting period × 3 plan (HC-STD / HC-PLS / HC-PRM, 17 benefit item mỗi plan) = **215 bản ghi**. `build-04` §5–§6.

## Bộ test

**65 unit test** — `build-04` §9. Đây là phần duy nhất của hệ có sẵn bộ test, nên là phần duy nhất đo được ngay.

## Số phải ra đúng ở kịch bản B

~~`2.000.000 → 1.880.000` · dòng 4 `NOTCOV −80.000` · dòng 2 `CAP_VISIT −40.000`~~ — **chép nhầm, phát hiện 21/09** (C-PREP-2): cặp số này không sinh ra được từ A5 trên hồ sơ nội trú nào; 1.880.000 là hồ sơ A prototype **ngoại trú HC-STD, 6 dòng, tổng 3.128.000**. Đích đúng, đã tính tay khớp A5 từng đồng (`changes/active/verify-entity-vs-build01/batchB2-spec.md` §Kiểm số kịch bản B, lối C):

**`2.000.000 → 1.880.000` · dòng 4 `NOTREL −168.000` · `CAP_VISIT` 628.000 → 610.000 · `COPAY` 500.000 → 470.000.** Ba loại khấu trừ, đều nằm trong 8 causeCode. **build-00 §5 xung đột 5 (Project doc) ghi "2.000.000 → 1.880.000 nội trú, không co-pay" là sai — phải sửa theo dòng này** (chưa sửa được từ đĩa; PO sửa trên claude.ai).

## Bốn mã nguyên nhân chưa có câu chữ

`CAP_VISIT` · `COPAY` · `DEDUCT` · `LIMIT_TOTAL` — không có câu nào trong A2. Đồng chi trả áp gần như mọi hồ sơ ⇒ khối "Vì sao Quý khách tự chịu" sẽ trống đúng ở nhóm phổ biến nhất. `build-06` §9 lỗ hổng 1.
