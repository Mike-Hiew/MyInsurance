# Change — Đối chiếu data model hiện có với `build-01`

**Trạng thái:** 🟡 đang làm — task 1–10 xong; task 11: đợt A đóng · **đợt B/B2/B3 đóng 21/09 (ClaimMVP rev 138, MyInsurance rev 51, readout toàn 0 lệch, 5 màn ✅)**; còn 12 · **Hạn:** 22/09 · **Tiếp theo:** `chain-14-steps-a5`
**Spec liên quan:** `specs/data-model/spec.md`

## Vấn đề

Entity trên ClaimMVP được suy ra từ `Prototype_Insurance_BnK_v0.5.html` để màn hình chạy được. `build-01` có **32 entity · 31 enum · 30 ràng buộc phải tự thêm**. Hai bộ **chưa từng được so**.

Chuỗi 14 bước đọc ghi thẳng vào entity ⇒ nếu model sai, chuỗi sai, và phát hiện ở tuần rehearsal thì không sửa kịp.

## Nguyên tắc

> **Ra danh sách delta trước. Không sửa gì cho tới khi có danh sách.**
> Một số delta sẽ được xử lý bằng cách **sửa spec**, không phải sửa entity — prototype có thể đúng hơn ở vài chỗ. Nhưng phải là một quyết định có ghi lý do, không phải một sự trôi dạt.

## Việc

- [x] **1. Xuất model hiện có.** *(20/09 — `current-model.md`, 18 entity / 162 attribute)* Dùng MCP `context_*` hoặc Service Studio: tên entity · attribute · kiểu · nullable · khoá ngoại → `current-model.md` trong thư mục này
- [x] **2. Xuất Static Entity + records** *(20/09 — `current-enums.md`, 4 static / 20 record; Label/Order chỉ có ở runtime)* → `current-enums.md`
- [x] **3. So 4 enum chặn demo trước** (rẻ nhất, đau nhất nếu sai):
  - [x] `claimState` — phải đủ **13** giá trị
  - [x] `lossType` — **12** giá trị, 8 sống
  - [x] Mã từ chối — đúng **7**, danh sách đóng, không thừa mã nào
  - [x] `LineCategory` (6) và `BenefitCode` (200.1 / 200.2 / 300)
- [x] **4. So 15 entity nhóm claim** (`build-01` §2)
- [x] **5. So 10 entity nhóm hợp đồng** (`build-01` §1)
- [x] **6. So 7 entity nhóm IDP** (`build-01` §3) — nhóm này gần như chắc chắn thiếu, vì REST Consume chưa làm
- [~] **7. Kiểm 30 ràng buộc** của `build-01` — ODC không sinh hộ cái nào
- [x] **8. Viết `delta.md`**, mỗi dòng phân loại: `THIẾU` / `THỪA` / `SAI KIỂU` / `THIẾU RÀNG BUỘC` / `TÊN KHÁC`
- [x] **9. Gắn mức chặn cho từng delta:** `chặn-chuỗi-14-bước` · `chặn-IDP` · `chặn-wording` · `cosmetic`
- [x] **10. Quyết từng dòng:** *(20/09 xong — §7 trả lời, S1–S5 duyệt, đợt A–F duyệt theo delta.md §8)* sửa entity hay sửa spec — **ghi lý do một dòng**. Dòng nào sửa spec thì mở delta trong `changes/active/<x>/specs/data-model/spec.md`
- [~] **11. Thi hành** *(20/09: **ĐỢT A ĐÓNG** — ClaimMVP rev 132, MyInsurance rev 50 (A1–A6 + A12 + A13); kế tiếp = đợt B; xem `batchA-spec.md` §Kết quả; còn: **A12 CustomerLabelEn** (theo `../portal-bilingual-toggle/`, chờ vòng Mentor riêng), CauseCode (L3), Re-run demo seed thay migration (L4 bác), gỡ inline console, dọn DecisionCode; **đợt B: rev 133/134, xem `batchB-spec.md` §Kết quả — chờ Re-run demo seed + phán §Phát hiện 2 (BenefitCode không phải mã A5)**; đợt C–F chưa bắt đầu)* — entity mới sinh qua `outsystems-spec-driven-build` từ spec, **không qua Mentor tự do**
- [ ] **12. Kiểm:** màn hình cũ vẫn chạy (WorkList, ClaimDetail 8 tab, 7 màn portal). Chụp màn so với `../../../output/bnk-dev-claimmvp/shots-rev88/`

## Xong nghĩa là

- `delta.md` tồn tại, mỗi dòng có quyết định và lý do
- 4 enum chặn demo khớp chuẩn 100 %
- 17 màn vẫn render như trước khi sửa
- Chuỗi 14 bước có chỗ để ghi vào
