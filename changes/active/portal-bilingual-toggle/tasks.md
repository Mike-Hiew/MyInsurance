# Change — Giữ nút song ngữ VI/EN trên portal khách

**Mở:** 20/09/2026 · **Lý do:** pitch bằng tiếng Anh, giám khảo sẽ nhìn portal
**Trạng thái:** 🟡 quyết định đã chốt, chưa thi hành
**Spec bị đổi:** `specs/screens-wording` — xem `./specs/screens-wording/spec.md`
**Liên quan:** `../verify-entity-vs-build01` (đợt A đang thêm cột cho static entity — cột EN đi nhờ chuyến đó)

## Quyết định

`build-06` §8.5 hiện ghi **"Portal: tiếng Việt, không có nút chuyển"**. Quyết định 20/09: **giữ nút chuyển**, vì bài dự thi trình bày bằng tiếng Anh. Đây là một **thay đổi spec**, không phải một ngoại lệ — nên nó nằm ở đây trước khi nó nằm trong code.

## Cơ chế — chốt một lần, áp cho cả hệ

Đây là lúc trả món nợ mà `prototype-v0-5` §5 mục 2 đã nêu từ đầu và chưa ai chốt.

| Loại chữ | Cơ chế | Vì sao |
|---|---|---|
| **Nhãn sinh từ enum** — trạng thái hồ sơ, mã quyết định, `causeCode` | **Cột trên Static Entity**: `LabelVi` · `CustomerLabelVi` · `CustomerLabelEn` | Service Action phải đọc được. ODC Translation là cơ chế của tầng UI, Server Action không với tới |
| **Chrome màn hình** — nav, tiêu đề, nút, nhãn field | **ODC Translation** | Không phải dữ liệu; nhân bản vào entity là sai chỗ |
| **Câu A2** — 53 message, thư, lý do từ chối | Cột `TextVi` / `TextEn` trên bảng message | Cùng lý do với enum: agent và Server Action render chúng |
| **Nội dung chứng từ** — tên khoản mục, tên cơ sở KCB, tên người | **LUÔN giữ tiếng Việt, kể cả ở chế độ EN** | `build-06` §8.5. Đây là giấy tờ thật — và nó **chứng minh** hệ đọc được chứng từ tiếng Việt. Dịch nó là xoá mất bằng chứng |

Hai cơ chế chứ không một, nhưng ranh giới là **dữ liệu / không phải dữ liệu**, không phải "mỗi màn một kiểu".

## ⚠ Chữ tiếng Anh cho khách KHÔNG có nguồn chuẩn

A2 là catalogue **tiếng Việt**. Không có bản tiếng Anh nào được duyệt cho câu gửi khách.

⇒ Mọi `CustomerLabelEn` và mọi `TextEn` là **bản dịch của đội, không phải A2**. Đánh dấu rõ trong seed và trong deck. Cùng kỷ luật với 4 ô `CustomerLabelVi` để trống của `CauseCode`: không để chữ tự chế trông như chữ đã duyệt.

## Phạm vi cho ngày pitch

| Mức | Nội dung | Khi nào |
|---|---|---|
| **P0 — đủ để pitch** | `CustomerLabelEn` trên `ClaimStatus` (13) · chrome 7 màn portal qua Translation · nút toggle hoạt động | Đi nhờ vòng Mentor của đợt A + một vòng riêng |
| **P1** | `CauseCode` EN · nhãn quyền lợi EN trên `PortalBenefits` | trước 29/09 |
| **P2 — có thể cắt** | 53 câu A2 bản EN | cắt được; deck giải thích rằng thư gửi khách là tiếng Việt theo thiết kế |

**Không bao giờ dịch:** tên khoản mục, tên cơ sở KCB, tên người, nội dung trích xuất.

## Việc

- [x] 1. Thêm `CustomerLabelEn` Text(100) vào `ClaimStatus`, seed đủ 13 — **đi nhờ vòng Mentor đợt A** (đang thêm cột cho static entity rồi) · *20/09: vòng đợt A đã đóng ở rev 131 trước khi việc này được ghi; spec + 13 bản dịch nằm ở `../verify-entity-vs-build01/batchA-spec.md` §A12, chờ vòng Mentor riêng*
- [x] 2. `Portal_CustomerState` / `GetClaimStatusInfo` trả thêm `CustomerLabelEn`; màn chọn cột theo locale hiện hành · *20/09 xong ở MyInsurance rev 50: 4 màn đọc `ClaimStatus` qua aggregate join, chữ qua `LocalizedStatusLabel(Vi,En)`, màu qua `StatusColorToken(Value)` — ánh xạ tồn tại một nơi. Nút VI **ẩn** (Visible=False, không logic) cho tới việc 3–4.*
- [ ] 3. Chốt cơ chế chrome: *(20/09: đã kiểm — nút VI hiện trơ, KHÔNG có cơ chế sẵn; chrome là EN literal. Chỉ có một cơ chế: `GetCurrentLocale()` — toggle cần `SetCurrentLocale` + Translation)* ODC Translation cho 7 màn portal — dựng thử trên **một** màn (`PortalHome`) đo công trước khi làm 6 màn còn lại
- [ ] 4. Nút VI/EN trên portal đổi **cả** chrome lẫn nhãn trạng thái; kiểm `PortalHome` không còn lệch với 3 màn kia
- [ ] 5. Kiểm: ở chế độ EN, **tên khoản mục và tên cơ sở KCB vẫn tiếng Việt**. Nếu chúng bị dịch → lỗi, không phải tính năng
- [ ] 6. Ghi vào deck: câu gửi khách (thư, thông báo) là **tiếng Việt theo thiết kế**; phần EN của portal là tiện ích trình bày, không phải sản phẩm song ngữ đầy đủ
- [ ] 7. Merge quyết định này vào `build-06` §8.5 rồi archive change

## ⚠ Lệch đã biết (ghi nhận 20/09, KHÔNG phải lỗi phải sửa ngay)

**Chrome portal là chữ tiếng Anh literal từ prototype** (nav, tiêu đề, timeline, nút, câu info). Không có ODC Translation, không có `SetCurrentLocale`; nút "VI" chưa từng có logic và nay được ẩn. ⇒ **App hôm nay là app tiếng Anh với dữ liệu tiếng Việt**, trong khi `build-06` §8.5 (kể cả sau delta này) đặc tả app tiếng Việt mặc định có nút EN. Không chặn chuỗi 14 bước. Thuộc **việc 3** (chrome qua Translation + wire toggle) và **deck** (nói rõ chrome EN là tiện ích trình bày), không thuộc đợt A. Khi làm việc 3, đổi `StatusColorToken` để `EXTRACTION_ERROR` cùng màu `DOC_PROCESSING` (hiện đỏ — mask che chữ nhưng lộ màu).

## Xong nghĩa là

- Bấm toggle trên portal: chrome và chip trạng thái đổi ngôn ngữ cùng lúc, cả 7 màn
- Tên khoản mục / cơ sở KCB **không** đổi
- Deck nói rõ chỗ nào là bản dịch của đội, chỗ nào là A2
