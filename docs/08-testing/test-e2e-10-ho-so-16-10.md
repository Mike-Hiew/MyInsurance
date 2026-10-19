# Test end-to-end 10 bộ hồ sơ — 16/10/2026 + vòng sửa lỗi 16–17/10 + vòng UI/UX 17/10 (cập nhật 18:10 17/10)

Tracker: https://claude.ai/artifact/1obBuNLDUzNPkWEbzFD9P9 (152 test case: 55 chức năng + 97 UI/UX; tab "Kết quả chạy 16/10"; kết quả ghi "Claude · chạy tự động 16/10" / "Claude · chạy lại 17/10" / "Claude · vòng UI 17/10"). Đủ 152/152 case có kết quả.
Bộ chứng từ: `E:\Hackaton\output\bo-chung-tu-test-16-10\` (PNG/JPG + manifest.json). Tải lên bằng canvas dựng lại trên trang (localStorage `bnkR`/`bnkC`/`bnkF` trên bnk-dev) vì extension Chrome không cho file_upload. Biến thể chạy lại: R07, R07C, R05, R05B, R02, R01, R06 (Hà tai nạn 13/10), R04A/R04H (An bảng kê tên An + phiếu khám bổ sung tên Hà), R04B/R04C/R04D.

**Bản đang chạy: ClaimMVP rev 853, MyInsurance rev 281** (sau vòng UI; trước đó 835/268). ChainTests 46/46, cổng 5/5 (chạy lại 17/10 14:50). Tracker: chức năng đều Đạt trừ FC-07 (chờ quyết nghiệp vụ); UI/UX 91 case vòng 3: 73 Đạt · 7 Chưa đạt · 3 Chưa kiểm được · 8 Không áp dụng. Điểm rollback an toàn trước vòng sửa: ClaimMVP 790, MyInsurance 247. Mentor: phiên ClaimMVP `6ae8a2a7…` (mở từ rev 806), MyInsurance `18356fac…`.

## Kết quả vòng 1 (rev 790 / 247)
| Bộ | Hồ sơ | Kết quả |
|---|---|---|
| TC01 An · sạch | CLM-2026-0535 | ✅ Tự duyệt 411.200 ₫, thư AG-5 đã gửi |
| TC02 Hà · sạch | CLM-2026-0536 | ❌ Ghi nhầm cho An |
| TC03 An · vượt trần + vitamin | CLM-2026-0537 | ⏸ đúng, cần người duyệt thứ hai |
| TC04 Hà · thiếu phiếu khám | CLM-2026-0538 | ❌ AG-4 bỏ sót lệch tên trên luồng bổ sung → đã chi |
| TC05 An · nha khoa | CLM-2026-0539 | ❌ 470.000 thay vì 455.000 |
| TC06 Hà · tai nạn | CLM-2026-0540 | ❌ xếp ngoại trú; nẹp bị loại |
| TC07 An · nội trú 3 ngày | CLM-2026-0541 | ❌ cắt oan tiền giường + Ceftriaxone bị loại → 2.170.000 |
| TC08 Hà · thai sản | CLM-2026-0542 | ❌ IDP mất 1 dòng; đồng chi trả 20% (câu hỏi nghiệp vụ) |
| TC09 sai tên + ngoài danh mục | CLM-2026-0543 | ❌ ngày cũ bị giữ; ICD "110" |
| TC10 nộp trùng | CLM-2026-0544 | ❌ cán bộ không đóng/từ chối được |

## Sau vòng sửa — đã kiểm trên hồ sơ thật
| Lỗi | Kết quả | Bằng chứng |
|---|---|---|
| FC-03 trần giường/ngày × số ngày | ✅ | CLM-0545/0547; 0541 chạy lại → 4.780.000 |
| FC-04 thuốc nội trú | ✅ | Ceftriaxone trả theo 100 |
| FC-05 nha khoa | ✅ | CLM-0546/0563 455.000 ₫ |
| FC-06 thẻ tóm tắt theo quyền lợi đã hạch toán | ✅ rev 801 | CLM-0559 Hà tai nạn: nhóm 500, không đồng chi trả, 655.000/655.000 (nẹp được trả) |
| FC-08 trừ 2 lần trên Why | ✅ | |
| FI-05 AG-4 so tên khi bổ sung | ✅ | CLM-0560: bổ sung phiếu khám tên Hà vào hồ sơ An → CROSSCHECK_FAIL MEDIUM, chặn tự duyệt |
| FP-02 chọn người thân | ✅ | CLM-0558 Hà, tự duyệt 417.600 ₫ |
| FP-03 quay lại sửa bước 1 | ✅ (cách tạm) | tạo nháp mới khi bước 1 đổi; nháp cũ bỏ lại |
| FG-06 "Trả về cán bộ" | ✅ rev 802–803 | popup bắt buộc ≥10 ký tự; "Lý do trả về … · 17/10/2026 06:00" đúng giờ địa phương |
| FG-07 "Đề xuất từ chối" | ✅ rev 800 + 804 | CLM-0544 → Awaiting approval; tiêu đề thẻ "Đề xuất từ chối" (đọc sự kiện PROPOSE_REJECT) |
| FG-05 khoá duyệt cho người đề xuất | ✅ code | cần tài khoản duyệt thứ hai |
| FG-10 dòng tin cậy thấp trên hồ sơ đã chi | ✅ | không còn hiện khi đã chi |
| FP-14 / UX-04 trạng thái portal ≠ console | ✅ MyInsurance 249+ | 0561/0562 chip vàng "Chờ bổ sung chứng từ" |
| FP-16 trang bổ sung chứng từ | ✅ MyInsurance 250–253 | tiêu đề "Bổ sung chứng từ còn thiếu", nút Gửi bật, banner "Hồ sơ CLM-2026-0562 (Nguyễn Văn An · Ngoại trú)…", ẩn khi chưa chọn hồ sơ; menu mở hồ sơ chờ bổ sung mới nhất |
| Ngày "Decided · Paid · …" | ✅ rev 807 | lấy CreatedOn sự kiện quyết định + 7h (GetClaimSummary → GetDecisionEventForSummary): 0558 → 17/10, 0546 (quyết lúc 23:59 16/10) → 16/10 |
| ClaimDetail tự làm mới khi điều phối xong | ✅ | CLM-0563 |
| Số ngày nội trú | ✅ | hiện 3 |

### Sự cố trong vòng sửa (đã khắc phục)
- rev 793–794: danh sách dòng chi phí trống rồi hiện mọi dòng "Excluded" → sửa rev 795.
- **rev 795 (00:37–03:55): P0 — mọi hồ sơ portal báo "Claim is not in Draft status" khi gửi.** 6/13 action Portal_* thiếu `Claim.Id = ClaimId` → sửa rev 796. Nháp 0549–0556 bị chuyển "Received" (không hiện ở Work list).
- rev 799: Why/Evidence cộng dồn ledger của mọi lượt chạy (0541 hiện 16.250.000) → rev 800 lọc lượt mới nhất; rev 800 đếm 2 lần dòng ở nhóm cha (0541 hiện 8.380.000) → rev 801 chỉ đếm ở quyền lợi lá.
- Lời bình người dùng bị "dịch" ("và" → "and") trong nhật ký → sửa rev 802.
- Ngày quyết định: rev 806 sửa nhầm widget (DecidedTitle); dòng thật là ItemTitleExpr trong danh sách "What needs human judgement", dựng ở server → rev 807. Phiên Mentor `0da75771…` hết hạn (timeout 300s, 3 lỗi "Unavailable feature" — có sẵn, không chặn publish) → đóng, mở phiên mới.

### Còn mở sau vòng sửa
- Popup "Đề xuất từ chối" chưa hiện số hồ sơ trùng; badge "1 open signals" sau "Clear with reason" chưa kiểm lại; Portal báo "Không thể gửi hồ sơ" khi phiên hết hạn (nên báo đăng nhập lại).
- Tracker còn ghi ❌ từ lượt 16/10, chưa nằm trong vòng sửa này:
  - Tính tiền: FC-07 đồng chi trả 20% thai sản (cần xác nhận nghiệp vụ), FC-09 "Vượt trần" nhóm trống.
  - IDP/AG-4: FI-02 mất dòng TC08, FI-03 ICD "I10"→"110", FI-04/TY-02 "750,000 VND ₫", FI-06 cùng tên Vinmec lúc trong lúc ngoài mạng lưới, FI-07 thiếu PROVIDER_UNMATCHED, FA-06.
  - Portal: FP-04 ngày tương lai qua bước 1, FP-06 loại điều trị, FP-07/FP-11/FP-13 checklist "Tuỳ chọn/Còn thiếu" sai, FP-10 câu báo tiếng Anh, FP-15 "Bước tiếp theo" sai, FP-17 "Xin chào, Hieu!" vs "Chào An".
  - Thư: FM-03 nút Send sau khi gửi, FM-04 nút tải thư lúc còn nháp, FM-05 "sẽ được chuyển" vs "đã chuyển".
  - Ngôn ngữ: LN-01 chuỗi tiếng Anh trên trang VI, LN-02 tiêu đề tab.


## Vòng 2 — 17/10 09:30–12:20 (các đợt sửa đến hết)
Đăng nhập: phiên hết hạn ~1 giờ/lần; Claude tự vào lại bằng thẻ một chạm (Approver cho console, Khách hàng cho portal), không nhập mật khẩu.

| Đợt | Mục | Kết quả | Bằng chứng |
|---|---|---|---|
| 1 Portal | FP-17 lời chào, LN-02 tiêu đề tab, FP-10/LN-01 chuỗi VI, FP-04 chặn ngày tương lai, FP-07/11/13 checklist, FP-15 bước tiếp theo, FM-04/05 thư & trạng thái | ✅ | CLM-0566, 0574, 0575, 0563 |
| 2 Console | TY-02/FI-04 tiền "655.000 ₫", FM-03 khoá thư đã gửi, giờ thanh toán (+7h hai lần), FA-05 badge, popup trùng có số hồ sơ, khối chờ chứng từ VI, nhật ký IDP VI, "Chờ tính" | ✅ | 0541/0558/0559/0563/0583 |
| 3 Logic | FC-09 Điện tim → 200.1; FI-03 chuẩn hoá ICD (110→I10); FI-06 "chưa đối chiếu"; FI-07 PROVIDER_UNMATCHED; FA-06 tín hiệu TC09; FI-02 bộ phân tích bỏ dòng bắt đầu bằng "Tổng" | ✅ | 0574, 0575, 0580 (TC08 đủ 3 dòng) |
| 4 FP-06 | "Hình thức điều trị: Ngoại trú/Nội trú" cho Tai nạn/Thai sản/Nha khoa; console "Tai nạn · Nội trú (3 ngày)" | ✅ | CLM-0578 |
| Bốn mắt | FG-05/UX-02 người trình không thấy Duyệt/Từ chối + câu giải thích; FG-08 người thứ hai (mike) duyệt | ✅ | 0544, 0537 |
| Đầu-cuối sạch | An viêm mũi 15/10 → tự duyệt 144.000 ₫, portal khớp | ✅ | CLM-0582 |

### Hồi quy phát sinh và đã sửa trong vòng 2
- Sửa định dạng tiền làm thẻ "Vì sao" lấy sai nguồn (0559 hiện đồng chi trả 20%, 0558 đếm dòng 2 lần, 0541 ra 8.380.000 rồi 0) → GetMoneyFlow: mỗi dòng lấy đúng một bút toán cụ thể nhất của lượt chạy gần nhất có ghi sổ; "khớp sổ" chỉ khi bằng số đề xuất.
- Đổi kiểm tra bốn mắt làm lộ lại nút Duyệt cho người trình đề xuất từ chối (0544) → lấy người trình từ sự kiện SUBMITTED/PROPOSE_REJECT gần nhất.
- Phiên Mentor hay timeout 300s sau ~10 lượt → đóng/mở phiên mới (ClaimMVP: 0da75771 → 6ae8a2a7 → beb23226 → 204029ca → 04b2f6ac; MyInsurance: 18356fac → 018fa3cc → 1291bd81).

### Phát hiện khác
- TC09 nộp lại: ICD đọc đúng I10 (tăng huyết áp, bệnh có sẵn) → chuỗi từ chối cả hồ sơ theo kỳ chờ 365 ngày — đúng luật; 0543 cũ trả một phần chỉ vì ICD sai "110".
- Bộ chứng từ TC08 in "Tổng cộng 1.070.000" trong khi 3 dòng = 870.000 → R5 báo lệch là đúng.
- Nút Sign out nay chuyển về Login; bấm thẻ một chạm khi còn phiên sẽ đăng xuất trước.

### Chờ Mike
- ~~FC-07~~ → Mike chốt giữ đồng chi trả 20% (17/10 15:00).
- Lưu trữ hồ sơ test 0535–0583 + nháp 0549–0556.
- ~~LY-09~~ → có làm (xong vòng 4).

## Vòng 3 — UI/UX 91 case, 17/10 13:00–14:55 (giờ VN)
Cách chạy: script audit trong trình duyệt (localStorage `bnkA`/`bnkS`/`bnkW` + khung iframe `bnkF2`/`bnkF3` cho bề rộng 1440/1366/1100/420 px), quét 19 màn console + 8 màn portal ở dark và light, rồi kiểm tay chuông, popup, menu, panel trợ lý, EN/VI, điện thoại 420 px. Mọi bản publish đều kiểm lại trên trang thật và xác nhận số bản bằng `env_app` (Mentor nhiều lần báo “đã xong” khi chưa publish).

**Kết quả ghi vào tracker (91 case, nhãn “Claude · vòng UI 17/10”):** 73 Đạt · 7 Chưa đạt · 3 Chưa kiểm được · 8 Không áp dụng. Tracker hiện đủ 152/152 case có kết quả.

| Nhóm | Đã sửa và kiểm lại | Bản |
|---|---|---|
| Chuông (console) | chữ VI, Esc đóng, “Đánh dấu đã đọc tất cả” | ClaimMVP 840 |
| Thao tác không hoàn tác | popup “Trình duyệt hồ sơ?” (Huỷ không đổi gì — thử trên CLM-0545); popup có role=dialog, Esc đóng, focus vào popup, trả focus | 840–843 |
| Tiền | “Cần bạn” 1,900,000 VND → 1.900.000 ₫ | 840 |
| Menu | Esc đóng dropdown; Quản trị chuyển vào Kỹ thuật ▾; thêm Tra cơ sở y tế vào Tra cứu ▾ | 840, 843–845 |
| Tương phản | brand sub 4,48 → ≥4,5 (luật @media ghi đè trong theme); nhãn AgentMap; badge C1/C2; “Đổi tài khoản mẫu” | 841–842 |
| Trợ năng | aria-label chuông, giao diện, EN/VI, DdStatusVi, PolDropdown, MemDropdown | 841, 847–849 |
| Bố cục | bỏ bóng thẻ thường; lề tiêu đề đồng bộ; scrollbar + ::selection theo theme; hàng tab ClaimDetail cuộn; panel trợ lý không che nội dung | 842–847 |
| Điện thoại | header console một hàng có logo; Dashboard 420 px (khối tổng bị ép 0–16 px) → xếp dọc; bảng WorkList cuộn trong khung | 842–845 |
| Chữ VI | Accident, OpsQueue lỗi trích xuất, Policies, AgentMap, AgentTrace, Quản trị, ROI (phút/ngày), ChainTests, ngày ISO ở AgentTrace | 842–848 |
| Portal header | hết “phủ xanh” ở dark; nút giao diện + chuông lên thanh trên cạnh lời chào (dark/light); dropdown chuông nằm dưới chuông và trên nội dung; header điện thoại ☰ + ngăn kéo menu; tooltip cho dòng bị cắt | MyInsurance 274–281 |

**Bản đang chạy sau vòng 3: ClaimMVP rev 853, MyInsurance rev 281.** ChainTests 46/46, cổng 5/5 (chạy lại 17/10 14:50).

### Hồi quy phát sinh trong vòng 3 (đã khắc phục)
- MyInsurance 274: ẩn header cũ làm mất nút giao diện + chuông trên desktop; 277 đưa lên thanh menu nhưng light trắng-trên-trắng, 279 sửa; 280 làm dropdown chuông lệch ra ngoài màn, 281 sửa (và z-index).
- ClaimMVP 844: thêm “Tra cơ sở y tế” thành mục cấp 1, đè lên “Kỹ thuật ▾” → 845 sửa.
- **ClaimMVP 840–850: chuỗi bằng chứng C1 phía server bị đổi sang “1,120,000 VND” rồi dịch sang tiếng Việt → T-GATE-01/02 trượt, cổng 3/5.** 851 khôi phục chuỗi gốc tiếng Anh + tiền VI (“1.120.000 ₫ vs threshold 2.000.000 ₫ · 0 limit cut(s)”), dịch chỉ ở phía hiển thị → cổng 5/5. Bài học: không dịch chuỗi bằng chứng/nhật ký lưu ở server.
- Phiên Mentor ClaimMVP timeout 300 s ở lô dịch lớn → mở phiên mới, chia lô nhỏ (phiên 17d5d38b → 6c26afe3; MyInsurance 55833a09).

### Còn Chưa đạt (cần quyết hoặc làm tiếp)
- TH-05 bo góc: 5–16 giá trị mỗi màn — cần chốt thang (thẻ 16 / input 10 / nút-pill 999) rồi sửa đồng loạt.
- TY-04 cỡ chữ: Dashboard 17–19 cỡ, ClaimDetail 18.
- TY-03 ô ngày native hiện mm/dd/yyyy (locale trình duyệt, `<html lang="en-US">`) — cần datepicker riêng.
- DM-06 khung trắng thoáng qua khi tải trang đầy đủ (vd mở ClaimDetail).
- PF-02 CLS Dashboard 0,21 (> 0,1) — carousel/biểu đồ chưa giữ chỗ.
- LY-09 menu avatar thiếu công tắc “Chỉ xem phần đã có trên hệ thống” và “Xem bản điện thoại” — chờ Mike quyết có làm.
- RS-02 header điện thoại dùng ☰ + ngăn kéo, chưa phải 2 hàng có menu cuộn ngang như mockup.
- Chưa kiểm được: CP-04 (dữ liệu tải < 1 s), CP-10 (extension không cho tải tệp), IX-06 (bấm đúp sẽ tạo hồ sơ thật). Không áp dụng: UX-01, RS-05, XA-05, PF-03 (cần người/thiết bị thật), CP-06/IX-07/IX-11/UX-07 (tránh đổi dữ liệu hoặc không có hồ sơ phù hợp).
- Ghi chú nhỏ: Login console đã yêu cầu EN|VI + chữ VI (ClaimMVP 844) nhưng chưa xem lại được vì phải đăng xuất; sau khi đăng nhập lại console về EN mặc định; nút “Bỏ qua, đến nội dung” portal có màu đúng khi focus nhưng vẫn nằm ngoài khung (top −48 px).

### Tác động lên dữ liệu test trong vòng 3
- **CLM-2026-0541 đã bị chuyển sang “Chờ phê duyệt”** khi thử nút “Trình duyệt” lúc nút này chưa có bước xác nhận (chính là lỗi IX-05). Không tạo hồ sơ mới. Sau khi có popup, thử trên CLM-0545 và bấm Huỷ (hồ sơ vẫn “Đang thẩm định”).
- Đăng xuất portal một lần (để xem trang Login), đăng nhập lại bằng thẻ một chạm cho cả console và portal; ngôn ngữ console đặt lại VI.
- ChainTests và cổng được chạy lại nhiều lần (chỉ ghi kết quả test).

## Vòng 4 — theo quyết định của Mike, 17/10 15:00–15:50
Mike chốt: **FC-07 giữ đồng chi trả 20%** cho khám thai định kỳ (ghi Đạt); **có làm công tắc** trong menu avatar (LY-09). Thang bo góc dùng đề xuất thẻ 16 / ô nhập 10 / nút-pill 999.

| Case | Kết quả | Bản |
|---|---|---|
| FC-07 | ✅ theo quyết định nghiệp vụ | — |
| LY-09 menu avatar | ✅ Đổi tài khoản mẫu · công tắc “Chỉ xem phần đã có trên hệ thống” (lớp `live-only` trên `<html>`, khoá `bnk.liveOnly`; ẩn nhãn/nút demo) · Xem bản điện thoại · Đăng xuất | ClaimMVP 856–862, MyInsurance 283–285 |
| PF-02 nhảy bố cục | ✅ CLS 0,21 → 0 (khung layout có style nội tuyến nền navy + nội dung ẩn tới khi theme tải) | ClaimMVP 854, MyInsurance 283 |
| TH-05 bo góc | ✅ 16/10/999/50% (sót nhỏ PortalHome 14, PortalSupplement 20/12) | ClaimMVP 855–860, MyInsurance 284–286 |
| TY-04 cỡ chữ | ✅ Dashboard 19 → 10, ClaimDetail 18 → 10, các màn khác 5–8 (CSS ghi đè theo selector, đã thử trực tiếp trước khi gửi) | ClaimMVP 857, MyInsurance 284 |
| TY-03 ô ngày | ⚠️ console dd/MM/yyyy + lịch VI (lớp phủ, ô gốc ẩn); portal vẫn mm/dd/yyyy | ClaimMVP 858 |
| RS-02 header điện thoại | ⚠️ console 2 hàng hoàn chỉnh (dark/light); portal đã có 2 hàng + menu cuộn nhưng chuông/avatar chưa mở được trên điện thoại | ClaimMVP 858–862, MyInsurance sau 289 |
| DM-06 khung trắng | ⚠️ hết trang chưa có CSS; còn khung trống trắng ~1–3 s khi tải lại cả trang (trước khi app chạy) | 854 / 283 |

**ClaimMVP rev 863:** ChainTests 46/46, cổng 5/5 (17/10 15:45).

### Hồi quy trong vòng 4 (đã xử lý)
- **MyInsurance 286: DatePicker OSUI làm hỏng màn Tạo hồ sơ** (toast “There was a problem…”, 3 ô ngày trống, lỗi `Cannot read properties of null (reading 'disabled')`) → 287 quay lại ô ngày gốc trong vài phút. Không có hồ sơ nào bị tạo trong khoảng đó từ phía test.
- MyInsurance 289: header điện thoại mất hẳn (CSS áp vào menu nằm trong ngăn kéo đang đóng) → bản sau dựng `#bnkMobHdr` bằng script như console.
- ClaimMVP 856–860: nút Đăng xuất lần lượt đè hàng mới rồi nhảy lên header → sửa bằng CSS định vị trong bảng menu (861).
- Kinh nghiệm: Mentor không “tìm-thay” được giá trị trong theme ~600 KB; cách chắc là đo trên trang, viết CSS ghi đè có `:not(#a)…` đủ độ ưu tiên, thử chèn trực tiếp rồi mới gửi.

### Vòng 4b — sau khi connector chạy lại (17:15–17:30)
- RS-02 portal ✅ (MyInsurance 291–292): chuông trên điện thoại mở màn Thông báo; avatar có chữ tắt, mở ngăn kéo (tên, vai, công tắc chỉ xem phần đã có, xem bản điện thoại, Đăng xuất); nhãn nút tiếng Việt; bỏ CSS 289 làm hỏng ngăn kéo.
- TY-03 portal ✅ (MyInsurance 293–294): ô ngày Tạo hồ sơ hiện dd/mm/yyyy, lịch tiếng Việt, ngày tương lai bị khoá; ô ngày gốc giữ nguyên binding/kiểm tra (chỉ ẩn). Bản 293 không chạy vì script chạy trước khi form hiện → 294 dùng MutationObserver. Đã thử chọn 15/10 → ô gốc 2026-10-15, không lỗi; không bấm Tiếp tục (không tạo nháp).
- Connector "outsystems" trên máy Mike báo failed (Request timed out) từ ~15:45 tới khi Mike khởi động lại; "outsystems-stpl" là tenant khác (stpl-942), không dùng thay được.

**Bản đang chạy cuối: ClaimMVP rev 863, MyInsurance rev 294.** Tracker UI 91 case: 79 Đạt · 1 Chưa đạt (DM-06 khung trắng khi tải lại cả trang — giới hạn nền tảng) · 3 Chưa kiểm được · 8 Không áp dụng.

### Vòng 4c — dọn lỗi giao diện còn sót (17/10 17:45–18:10)
| Mục | Kết quả | Bản |
|---|---|---|
| WorkList "Loại ngày" | ✅ ô chữ "Submitted" → dropdown "Ngày nộp" / "Submitted date", có aria-label | ClaimMVP 864–866 |
| "Cần bạn" ở EN | ✅ "1,900,000 VND", "1 day" (VI giữ "1.900.000 ₫") | ClaimMVP 864 |
| Chuỗi trợ năng tiếng Anh của biểu đồ Dashboard | ✅ không còn | ClaimMVP 864 |
| Sổ điều khoản | ✅ tên loại trừ tiếng Anh có nhãn "(EN)" khi xem VI | ClaimMVP 864 |
| Login console | ✅ EN\|VI, bản VI đủ chữ, tiêu đề tab "Đăng nhập" (đã đăng xuất để xem, rồi vào lại bằng thẻ Approver) | ClaimMVP 864–867 |
| Bo góc portal | ✅ đúng 4 giá trị trên mọi màn đã quét | MyInsurance 295–296 |
| Portal hết phiên | ✅ "Phiên đăng nhập đã hết hạn…" + "Đăng nhập lại" chỉ với lỗi Security/NotRegistered; lỗi khác vẫn "Không thể gửi hồ sơ" (kiểm qua Mentor, chưa thử phiên hết hạn thật) | MyInsurance 296–297 |

**Bản đang chạy cuối: ClaimMVP rev 867, MyInsurance rev 297.** ChainTests 46/46, cổng 5/5 (18:08). Không tạo/đổi hồ sơ; đăng xuất/đăng nhập lại console 3 lần để xem Login.


## Vòng 5 — nút sắp xếp / lọc / phân trang / tìm kiếm (17/10 18:20–20:15)
Bấm thử mọi control sort/lọc/phân trang trên 2 app, sửa qua Mentor. ClaimMVP 868→878, MyInsurance 298→301.

| Màn | Lỗi | Kết quả |
|---|---|---|
| WorkList | Bấm tiêu đề cột chỉ sắp trong từng nhóm ưu tiên | ✅ Sắp toàn danh sách, ▲/▼ + aria-sort; mặc định vẫn theo ưu tiên |
| WorkList | Ô "20 / page" gõ tự do, gõ 10 → hiện cả 35 | ✅ Thay bằng "Số dòng: 10 · 20 · 50" |
| WorkList | Đang ở trang 2 bấm lọc → "21–13 / 13", bảng trống | ✅ Mọi chip/dropdown/tìm/thẻ KPI/sort đưa về trang 1 |
| WorkList | Khoảng ngày không tác dụng, mặc định 01/08–03/09 | ✅ Lọc theo ngày nộp, mặc định 90 ngày gần nhất, đếm chip theo khoảng ngày |
| WorkList | Công tắc "Hiện hồ sơ kiểm thử" bật không lên | ✅ Bật → 85, tắt → 35; đếm chip theo công tắc |
| WorkList | 4 thẻ KPI lọc ra nhiều hơn số trên thẻ (gồm hồ sơ đã đóng) | ✅ 1/1/13/20 khớp số dòng |
| WorkList | Thiếu chip "Chờ bổ sung chứng từ"; dropdown khác tên chip; "Chọn…" | ✅ Có chip (2); tên khớp; "Tất cả trạng thái" |
| Dashboard | Thẻ "Đang thẩm định · bấm để mở" mở WorkList không lọc | ✅ Mở WorkList?Status=… đã lọc |
| Header | Ô tìm hồ sơ: Enter không làm gì | ✅ Enter → WorkList?Q=… |
| ProviderLookup | Enter không tìm | ✅ |
| AgentTrace | Chip chỉ 50 hồ sơ cũ nhất, không có "Tất cả", không dùng bàn phím | ✅ Dropdown "Tất cả hồ sơ" + toàn bộ hồ sơ mới nhất trước, TST cuối |
| LimitLedgerView | Cắt 50 dòng không báo (An 901 dòng); placeholder EN | ✅ Phân trang 50/trang + "Hiển thị a–b / N bản ghi"; "Chọn người được bảo hiểm…" |
| Policies | Placeholder "Select policy…" | ✅ "Chọn hợp đồng…" |
| Portal PortalClaims | Cũ nhất lên đầu, không lọc, không phân trang | ✅ Mới nhất trước; chip Tất cả/Đang xử lý/Cần bổ sung/Đã có kết quả (29=14+2+13); 10 dòng + "Xem thêm" + "Đang hiển thị X / Y" |
| Portal PortalNotifications | 124 mục một lèo | ✅ 20 mục + "Xem thêm" |

Sự cố trong vòng: rev 875 treo trình duyệt ở WorkList (JS FixDropdownPlaceholders do Mentor thêm dùng MutationObserver tự kích hoạt lại). Rev 876 gỡ JS phân trang, rev 877 bỏ observer → hết treo (~20 phút, 12:52–13:05Z).

Vòng 5b (17/10 21:20–21:35, ClaimMVP 879–880) — xử lý nốt 4 mục nhỏ:
- Nhãn đọc màn hình nút phân trang: "Trang trước / Trang N, đang xem / Trang sau" ở WorkList và Sổ hạn mức (VI); EN giữ nguyên. Gán sau mỗi lần tải dữ liệu, không dùng MutationObserver.
- Chế độ EN: dropdown "Date type: Submitted date", dropdown trạng thái có aria-label "Filter by status" (nguyên nhân: JS đọc window.v7Lang trước khi được gán → nay đọc cc.lang trước).
- Sort "Loại điều trị" theo nhãn ngôn ngữ đang dùng (VI: Ngoại trú → Nha khoa → Nội trú → Tai nạn → Thai sản; EN: Accident → … → Outpatient).
- Dashboard: 3 thẻ trạng thái loại hồ sơ kiểm thử, số khớp danh sách mở ra (13 / 3 / 1).

**Bản đang chạy cuối: ClaimMVP rev 880, MyInsurance rev 301.** ChainTests 46/46, cổng 5/5 (21:35). Không tạo/đổi hồ sơ.

## Vòng 6 — chứng từ tiếng Anh, ngoại tệ (17/10 22:00 – 18/10 00:40)
Câu hỏi: hồ sơ toàn tiếng Anh (thị trường nước ngoài), tiền khác VND — hệ thống có chạy được không. Sinh 7 bộ chứng từ mẫu tiếng Anh (bảng kê inpatient/outpatient, medical report, prescription, discharge summary, ER report, official receipt; chân trang "SAMPLE FOR SYSTEM TESTING ONLY"), nộp qua cổng khách hàng như thật, đọc bằng BnK IDP.

**Kết luận ngắn:** trước khi sửa (rev 880) hệ thống đọc được chữ tiếng Anh nhưng coi mọi số là VND → số tiền sai 2–5 bậc. Sau khi sửa (ClaimMVP 881→900, MyInsurance 302) quy đổi đúng tới từng dòng; phần còn lại là quy tắc nghiệp vụ và giới hạn của IDP.

### Thay đổi
| Ở đâu | Thay đổi |
|---|---|
| Dữ liệu | Entity CurrencyRate (Code, RateToVnd, RateDate, Source). Seed 8 mã: VND 1 · USD 25.700 · EUR 28.679,9 · SGD 19.902 · THB 759,82 · JPY 159,42 · GBP 33.972,5 · AUD 17.735,4 — Vietcombank mua chuyển khoản 17/10/2026 (nguồn chogia.vn). Claim: InvoiceCurrency (mặc định VND), FxRateToVnd, FxRateDate, FxSource. ClaimLine: OriginalCurrency, OriginalUnitPrice, OriginalAmount |
| Đọc số | ParseAmountLocale: nhận 1,234.50 (Anh/Mỹ) và 1.234,50 (châu Âu), bỏ ký hiệu tiền/chữ; VND giữ ParseVndAmount cũ. ParseClaimAmount = số gốc × tỷ giá, làm tròn đồng |
| Dòng chi phí | IdpTablesToClaimLines dùng tỷ giá của hồ sơ, lưu cả số gốc; đọc từ mọi chứng từ của hồ sơ (trước chỉ chứng từ đầu) |
| Phân loại | InferServiceCategory thêm từ khoá tiếng Anh (room & board, physician visit, X-ray, blood count, injection, consultation…) |
| Đối chiếu | R4 chấp nhận số ngày giường = số đêm hoặc số đêm + 1. R5 đọc tổng cuối theo loại tiền; ngoại tệ lệch đúng ×100 coi là IDP mất dấu thập phân |
| AG-1 | Ngoại trú / tai nạn ngoại trú / nha khoa / thai sản ngoại trú: báo cáo y tế (T2-03) thay được phiếu khám (T1-03) |
| Cổng KH | Bước 1 "Tạo hồ sơ" có ô "Loại tiền trên hoá đơn" (VND mặc định + 7 ngoại tệ), truyền vào Portal_CreateDraftClaim2/UpdateDraftClaim2 |
| Bảo vệ | CheckDecimalLoss: hồ sơ ngoại tệ có mọi số tiền gốc là số nguyên và số lớn nhất ≥ 10.000 → tín hiệu CROSSCHECK_FAIL "Có thể IDP mất dấu thập phân…". Thứ tự lúc tiếp nhận: dựng dòng xong mới đối chiếu R4/R5 (hết DATE_IMPOSSIBLE báo nhầm) |
| Console | ClaimDetail: mục "TIỀN HOÁ ĐƠN · 1 USD = 25,700.00 ₫ · Vietcombank 17/10/2026"; bảng dòng hiện thêm "USD 760.00". Ô loại tiền + nút "Đổi loại tiền & xử lý lại" (ReprocessClaimCurrency: dựng lại dòng, đối chiếu R4/R5, AG-1, chạy lại chuỗi). Ô tick "Chứng từ mất dấu thập phân (÷100)". Chặn khi hồ sơ đã có bút toán sổ hạn mức (không xoá bút toán — BR-LEDGER-3) |

### Kết quả trước / sau
| Hồ sơ | Bộ chứng từ | Trước sửa (rev 880) | Sau sửa |
|---|---|---|---|
| 0584 | An · Lakeside Medical Centre Singapore · SGD 166.90 · ngoại trú | Yêu cầu 337 ₫ (Amoxicillin 18.90 đọc thành 189), R5 lệch, thiếu phiếu khám → Chờ bổ sung | **3.321.644 ₫** (= 166,90 × 19.902), R5 khớp, AG-1 đủ chứng từ → Đang thẩm định. Chuỗi chờ xử lý tín hiệu PROVIDER_UNMATCHED |
| 0585 | Hà · Harbourview Bangkok · USD 1,845.00 · nội trú 3 đêm | 1.845 ₫, R5 lệch, R4 DATE_IMPOSSIBLE, 5/6 dòng "không thuộc quyền lợi", từ chối | **47.416.500 ₫**, R4/R5 hết, dòng map đúng 100/100.1. Vẫn từ chối ở bước kỳ chờ — đúng quy tắc của thẻ DEMO-W-001, không do tiền tệ |
| 0586 | An · Zahnklinik München · EUR 410,00 (kiểu châu Âu) · nha khoa | 410 ₫, thiếu phiếu khám | **11.758.758 ₫** (95,00 / 280,00 / 35,00 đọc đúng), Đang thẩm định, đề xuất **1.503.037 ₫** (trần 300.1 1.000.000 ₫/lần + đồng chi trả). Còn ghi chú R5 cũ (xem dưới) |
| 0587 | Hà · Pacific Urgent Care Honolulu · USD 503.00 · tai nạn ngoại trú | 503 ₫, receipt "Không nhận ra", thiếu phiếu khám | **12.927.100 ₫**, R5 khớp, Đang thẩm định, chờ xử lý PROVIDER_UNMATCHED |
| 0588 | An · Riverside Clinic TP.HCM · VND, chứng từ tiếng Anh | 1.233.000 ₫ đúng; thiếu phiếu khám | Như cũ về tiền; AG-1 đủ → Đang thẩm định |
| 0589 (mới) | An · Harbourview Bangkok · USD 1,249.00 · nội trú 2 đêm — nộp từ cổng, chọn USD ở bước 1 | — | Cổng hiện ngay **32.099.300 ₫** (= 1.249 × 25.700). Đề xuất **17.067.300 ₫**: tiền giường 19.532.000 ₫ bị cắt về 1.500.000 ₫/ngày × 3 ngày; 5 dòng còn lại trả đủ 12.567.300 ₫. Lúc tiếp nhận bị DATE_IMPOSSIBLE báo nhầm (R4 chạy trước khi có dòng) — đã sửa thứ tự ở rev 896 |
| 0590 (mới) | An · Klinikum am Markt München · EUR 927,50 · nội trú 2 đêm — nộp từ cổng, chọn EUR | — | Lần này IDP bỏ dấu phẩy ở **mọi** số (620,00 → 62000) → cổng hiện 2.660.060.725 ₫ (gấp 100). Hệ thống bật cảnh báo "Có thể IDP mất dấu thập phân"; cán bộ tick ÷100 → **26.600.608 ₫** (17.781.538 + 5.449.181 + 1.218.896 + 2.150.993), cảnh báo tự đóng. Không còn DATE_IMPOSSIBLE lúc tiếp nhận |

ChainTests sau sửa: 46/46, cổng 5/5 (23:45 trên rev 892; 00:35 trên rev 900).

### Còn mở
| # | Vấn đề | Loại |
|---|---|---|
| 1 | IDP (LLM) đọc số kiểu châu Âu không ổn định: 0586 chỉ mất dấu phẩy ở dòng tổng ("410,00" → 41000, R5 báo lệch), 0590 mất ở mọi dòng. Đã có cảnh báo + nút ÷100, nhưng phát hiện dựa vào ngưỡng nên số nguyên lớn thật (vd EUR 12.000) cũng bị cảnh báo, còn số nhỏ mất dấu (vd 95,00 → 9500) thì không. Ghi chú R5 cũ trên 0586 còn vì 0586 đã ghi sổ, không chạy lại được | IDP |
| 2 | Bệnh viện quốc tế tính theo đêm (2 đêm), chuỗi tính trần giường theo ngày tính cả hai đầu (3 ngày) → trần 4.500.000 thay vì 3.000.000 ₫ | Mike quyết |
| 3 | Tỷ giá cố định một ngày (17/10/2026), chưa có cập nhật và chưa chốt quy tắc lấy tỷ giá ngày khám hay ngày chi trả | Mike quyết |
| 4 | Cơ sở nước ngoài luôn PROVIDER_UNMATCHED (không có trong danh mục) → chuỗi chờ cán bộ | Dữ liệu danh mục |
| 5 | IDP: Official receipt không nhận ra; Medical report xếp T2-03; tên dịch vụ dính cột đơn vị ("Scaling and polishing Session") | IDP |
| 6 | Cổng KH chỉ hiện VND (chưa hiện số gốc USD/EUR); thư AG-5 chỉ tiếng Việt | Làm tiếp |
| 7 | Đổi loại tiền chỉ làm được trước khi chuỗi ghi sổ; sau đó bị chặn có báo lý do | Thiết kế |

### Tác động lên dữ liệu test
- Tạo mới hồ sơ 0584–0590 (7 hồ sơ, 21 ảnh chứng từ mẫu, dùng quota IDP).
- 0584–0590: dòng chi phí xoá và dựng lại (0590 ba lần); trạng thái 0584/0586/0587/0588 từ "Chờ bổ sung" → "Đang thẩm định"; tín hiệu CROSSCHECK_FAIL/DATE_IMPOSSIBLE cũ đóng.
- 0586 có 8 bút toán sổ hạn mức tạm (provisional) của An (MB-2026-0001); 0585, 0589 đã chạy chuỗi nên có thể cũng có. 0590 chưa ghi sổ (chờ PROVIDER_UNMATCHED).
- Thêm 8 dòng CurrencyRate.

**Bản đang chạy cuối: ClaimMVP rev 900, MyInsurance rev 302.**

## Vòng 7 — chế độ demo quốc tế (USD), 18/10 08:30–10:05
> Đã thay bằng Vòng 8: bỏ gói quốc tế / John Carter, hồ sơ 0584–0592 đã lưu trữ.
Mike chọn: hiển thị USD + gói quốc tế, bên trong vẫn tính VND (ChainTests không đổi). ClaimMVP 901→910, MyInsurance 303→305.

### Cách dùng khi demo
Console → menu tài khoản (góc phải) → dòng **"Tiền tệ demo": VN ₫ | INTL $**.
- **INTL $**: mọi số tiền ở console và cổng khách hiện $ (tỷ giá USD 25.700), cổng khách đăng nhập 1 chạm sẽ là **John Carter** (thẻ INT-2026-0001, hợp đồng HC-INT-2026-0001), ô "Loại tiền trên hoá đơn" mặc định USD.
- **VN ₫**: trở lại như cũ (Nguyễn Văn An, ₫). Chạy ChainTests ở chế độ này (log kiểm thử so khớp chuỗi ₫).

### Thay đổi
| Ở đâu | Thay đổi |
|---|---|
| Dữ liệu | Entity DemoSetting (SettingKey/SettingValue). GetDemoMode / SetDemoMode / Portal_GetDisplaySettings |
| Hiển thị ClaimMVP | Biến DisplayCurrency, UsdRate; FormatMoney, FormatVND (client), FormatVND_Server, FormatVND (service) trả "$1,234.56" khi INTL |
| Hiển thị MyInsurance | FormatAmount, FormatVND, FormatNotifText theo DisplayCurrency (lấy từ ClaimMVP khi mở trang); lời chào dùng tên đầu khi INTL ("Xin chào, John!") |
| Gói quốc tế | Seed_IntlDemo (tự chạy khi bật INTL, chạy 1 lần): hợp đồng HC-INT-2026-0001 (gói HC-STD, 01/01–31/12/2026), người được bảo hiểm John Carter; quyền lợi chép từ hợp đồng của An rồi làm tròn tới 10 USD (vd trần giường $60/ngày, trần khám $60/lần, ngoại trú $780, nội trú $3,890, nha khoa $190) |
| Cổng KH | Portal_GetCurrentMember(2): khi INTL trả John Carter |
| Sửa thêm | ReprocessClaimCurrency cập nhật lại tổng yêu cầu (TotalClaimed). RaiseFraudSignal bỏ ký tự NUL (AG-4 từng gửi 0x00 làm lỗi DB, hồ sơ kẹt "AG-4 đang chạy") |

### Kiểm thử
| Hồ sơ | Nội dung | Kết quả |
|---|---|---|
| 0591 | John Carter · ngoại trú · Harbourview Bangkok · USD 109.80 (bảng kê, medical report, đơn thuốc) | Cổng: tổng $109.80, từng dòng $55.00 / $32.00 / $16.80 / $6.00. Console: vượt trần 200.1 ($87.00 so với $60.00/lần, −$27.00), đồng chi trả 20% (−$16.56), **đề xuất $66.24**. Bảng kê lần 1–2 IDP "Không nhận ra", lần 3 (tải theo loại chứng từ) nhận đúng |
| 0592 | John Carter · nội trú 2 đêm · USD 1,249.00 | IDP mất dấu thập phân (760.00 → 76000) → cổng hiện $124,900; cảnh báo "Có thể IDP mất dấu thập phân" bật đúng; tick ÷100 → $1,249.00. AG-4 kẹt do lỗi NUL, sửa ở rev 910 rồi chạy lại: **đề xuất $669.00** (giường $760 cắt về $60/ngày × 3 ngày = $180). Cổng: "Tổng chi phí $1,249.00 · Dự kiến chi trả $669.00" |

ChainTests 46/46, cổng 5/5 (18/10 10:00, rev 910, chế độ VN).

### Lưu ý / còn mở
- Lịch sử cũ (log agent, kết quả R5 đã lưu) vẫn ghi ₫ — chỉ số tính lúc hiển thị mới đổi $.
- Ngưỡng tự duyệt 2.000.000 ₫ hiện thành $77.82 (số lẻ) — nếu cần số tròn thì đổi ngưỡng ở ODC Portal.
- Trần giường tính 3 ngày cho 2 đêm (như mục 2 Vòng 6) — chờ Mike quyết.
- IDP vẫn có lúc mất dấu thập phân hoặc không nhận ra bảng kê; khi demo, nếu gặp: tick ÷100 + "Đổi loại tiền & xử lý lại", hoặc tải bảng kê vào ô "Theo loại chứng từ".
- Cơ sở nước ngoài luôn PROVIDER_UNMATCHED → cán bộ "Gỡ kèm lý do" (đã gỡ trên 0592 với lý do "Test demo quốc tế…").
- Cổng: chuyển EN bằng nút "Tiếng Anh" trong menu tài khoản (chưa kiểm hết bản EN ở chế độ USD).

### Tác động lên dữ liệu test
- Thêm 1 dòng DemoSetting (đang để **INTL**), 1 Policy + 1 Member (John Carter) + bộ PolicyBenefitSnapshot của hợp đồng mới.
- Tạo hồ sơ 0591, 0592 (8 ảnh chứng từ mẫu, có 2 bản bảng kê không nhận ra nằm trong 0591); 0592 dòng dựng lại 2 lần, tín hiệu PROVIDER_UNMATCHED được gỡ kèm lý do; 0591/0592 có thể đã có bút toán tạm trên hợp đồng HC-INT.

**Bản đang chạy cuối: ClaimMVP rev 910, MyInsurance rev 305. Chế độ demo đang bật: INTL $.**

## Vòng 8 — bệnh viện quốc tế tại Việt Nam, hoá đơn tiếng Anh/USD (18/10 10:10–12:30)
Mike đổi phạm vi: **bỏ thị trường nước ngoài**. Bệnh viện vẫn là bệnh viện Việt Nam có trong danh mục IDP (bảng 322); chỉ hoá đơn là tiếng Anh, tiền USD vì bệnh viện quốc tế thu tiền khách nước ngoài. Thêm nút đổi VND ↔ USD cho khách nước ngoài. Dữ liệu test sai thì dọn đi. ClaimMVP 911→927, MyInsurance 306→309.

### Thay đổi
| Ở đâu | Thay đổi |
|---|---|
| Bỏ gói quốc tế | Cổng KH luôn là Nguyễn Văn An (bỏ John Carter khỏi đăng nhập 1 chạm). DemoSetting chỉ còn là cài đặt **hiển thị** VND/USD, không đổi hợp đồng |
| Nút VND ↔ USD | Console: menu tài khoản → "Hiển thị tiền" VND \| USD (toàn hệ thống). Cổng KH: công tắc **VND \| USD** trên thanh menu cạnh nút sáng/tối, theo từng người dùng. Bên trong vẫn tính VND, tỷ giá USD 25.700 |
| R4 (ngày vào/ra viện) | **Sửa lỗi**: so ngày ra viện của chứng từ sau với chuỗi đã nối ("13/10/2026 \| 13/10/2026") nên luôn lệch → mọi hồ sơ nội trú có ≥ 2 chứng từ ghi ngày ra viện đều bị DATE_IMPOSSIBLE (HIGH) chặn tự duyệt. Nay so với giá trị đầu tiên, bỏ qua ô trống |
| R3 (tên cơ sở) | Bệnh viện quốc tế in tên tiếng Anh và tiếng Việt trên cùng bộ chứng từ ("VINMEC CENTRAL PARK INTERNATIONAL HOSPITAL" / "BỆNH VIỆN ĐA KHOA QUỐC TẾ VINMEC CENTRAL PARK"). Hàm mới FacilityCoreName bỏ dấu, Đ→D, bỏ từ chung (Bệnh viện, Đa khoa, Quốc tế, Hospital, International, Clinic…) rồi so phần lõi; trùng hoặc chứa nhau là khớp |
| Tên dòng chi phí | IDP hay dính cột đơn vị vào tên ("Room & board - standard room Day"). CleanLineDescription bỏ đơn vị ở cuối tên khi dựng dòng; nút "Làm sạch tên dòng chi phí" cho dòng đã có: quét 310 dòng của hồ sơ chưa lưu trữ, sửa 31 dòng (6 dòng của 0599, 25 dòng của hồ sơ test cũ) |
| Console ClaimDetail | Nhãn "TIỀN HOÁ ĐƠN" có bản EN "INVOICE CURRENCY"; dòng nguồn tỷ giá hết lặp ngày |
| Cổng KH bước 2 | Ngoại trú / tai nạn ngoại trú / nha khoa / thai sản ngoại trú: báo cáo y tế (T2-03) được tính là phiếu khám (T1-03) — hết "Examination record — Required/Missing" khi đã tải medical report (giống AG-1) |
| Lưu trữ hồ sơ test | ArchiveClaimsByNumbers nay **giải phóng cả bút toán tạm** (Status = REVERSED, không xoá — BR-LEDGER-3), trước chỉ đảo bút toán FINAL. Nút "Giải phóng bút toán tạm của hồ sơ đã lưu trữ" chạy cho mọi hồ sơ đã lưu trữ |
| Mất dấu thập phân | AutoFixDecimalLoss: hồ sơ ngoại tệ có mọi số tiền gốc là số nguyên và số lớn nhất ≥ 10.000 → tự chia 100 lúc dựng dòng, để lại ghi chú LOW cho cán bộ (trước phải tick ÷100 tay) |
| Tra cơ sở (LookupProvider) | Thêm nhánh khớp theo tên lõi (CATALOGUE_CORE) khi khách gõ tên ngắn. **Chưa chạy được** trên hồ sơ thật (xem Còn mở) |

### Dọn dữ liệu test
| Hồ sơ | Lý do | Xử lý |
|---|---|---|
| 0584–0590 | Bộ chứng từ bệnh viện nước ngoài (SGD/EUR/THB, PROVIDER_UNMATCHED) — ngoài phạm vi mới | Lưu trữ |
| 0591, 0592 | John Carter (gói quốc tế) — đã bỏ | Lưu trữ |
| 0594 | IDP mất dấu thập phân (×100), không đổi lại được vì đã ghi sổ | Lưu trữ, nộp lại thành 0595 |
| 0595 | Bị R3/R4 báo nhầm (2 tín hiệu HIGH/MEDIUM) — lỗi đã sửa | Lưu trữ |
| 0596, 0597, 0598 | Thử nhập tên cơ sở ngắn "Vinmec Central Park" → "Not in catalogue" | Lưu trữ |
Lưu trữ = ẩn khỏi mọi danh sách, khôi phục được bằng "Khôi phục". Bút toán tạm của các hồ sơ này đã chuyển REVERSED nên hạn mức của An không còn bị trừ oan.

### Hồ sơ mẫu dùng khi demo
| Hồ sơ | Nội dung | Kết quả |
|---|---|---|
| **0593** | An · **Bệnh viện Quốc tế City** · ngoại trú 16/10/2026 · cúm A (J10.1) · hoá đơn tiếng Anh **USD 110.00** (khám $45, test cúm $28, Oseltamivir 10 × $3.20, Paracetamol 20 × $0.25) | Trong mạng lưới. 2.827.000 ₫; đề xuất **1.960.720 ₫** (trần khám 1.500.000 ₫/lần cắt 376.100; đồng chi trả 490.180). Không còn việc mở |
| **0599** | An · **Bệnh viện Đa khoa Quốc tế Vinmec Central Park** · nội trú 2 đêm 11–13/10/2026 · viêm phổi (J18.9) · hoá đơn tiếng Anh **USD 528.00** | Trong mạng lưới, AG-4 không tín hiệu, R1–R5, R7 đạt. 13.569.600 ₫; đề xuất **10.359.600 ₫** (tiền giường $300 = 7.710.000 ₫ cắt về 1.500.000 ₫/ngày × 3 ngày). Còn: 4 dòng IDP độ tin cậy thấp chờ cán bộ xác nhận, 1 checkpoint kỳ hạn |
Cổng KH ở chế độ USD hiện "$528.00", từng dòng $300.00 / $120.00 / $40.00 / $18.00 / $38.00 / $12.00; bấm VND đổi lại ₫.

ChainTests 46/46, cổng 5/5 (18/10 12:28, rev 927, hiển thị VND).

### Còn mở
| # | Vấn đề | Loại |
|---|---|---|
| 1 | Khách gõ tên ngắn/tên tiếng Anh ("Vinmec Central Park") thay vì chọn từ gợi ý → hồ sơ "Not in catalogue" + PROVIDER_UNMATCHED, dù tra bảng 322 ra đúng cơ sở (79525, điểm 91,17, CATALOGUE_PARTIAL). Đã thêm nhánh CATALOGUE_CORE nhưng thử lại vẫn chưa ăn. Khi demo: gõ "Quốc tế" hoặc "Vinmec" rồi **chọn tên tiếng Việt trong danh sách gợi ý** | Làm tiếp |
| 2 | Gợi ý cơ sở ở cổng lấy từ tên cơ sở đã có trong hồ sơ, không phải bảng 322 → bệnh viện chưa từng có hồ sơ không hiện trong gợi ý | Thiết kế |
| 3 | Trần giường tính 3 ngày cho 2 đêm (Vòng 6 mục 2) | Mike quyết |
| 4 | IDP đọc số lượng NaCl là 1 (hoá đơn ghi 4) — tiền vẫn đúng | IDP |
| 5 | Hồ sơ test cũ 0535–0583 vẫn còn, chờ Mike quyết có lưu trữ không | Mike quyết |
| 6 | Nút làm sạch tên đã sửa 25 dòng trên hồ sơ test cũ (bỏ từ đơn vị cuối tên như "Viên", "Lần", "Day") — chưa soát từng dòng | Kiểm lại |

### Tác động lên dữ liệu test
- Lưu trữ 0584–0592, 0594–0598 (14 hồ sơ); bút toán tạm của chúng chuyển REVERSED.
- Tạo 0593–0599 (7 hồ sơ, 21 ảnh chứng từ mẫu tiếng Anh, dùng quota IDP); đang dùng 0593 và 0599.
- Hợp đồng/người được bảo hiểm John Carter còn trong DB nhưng không còn dùng ở đâu.
- DemoSetting đang để **VN** (hiển thị ₫). Cổng KH của An: công tắc đang để VND.
- 31 dòng chi phí đổi tên (bỏ từ đơn vị ở cuối), không đổi tiền/số lượng.
- Administration có thêm panel "Dọn hồ sơ kiểm thử" (Lưu trữ / Giải phóng bút toán tạm / Làm sạch tên dòng / Khôi phục).

**Bản đang chạy cuối: ClaimMVP rev 927, MyInsurance rev 309.**

## Chưa làm
- Đổi FP-03 sang Portal_UpdateDraftClaim2 khi chỉ mục tenant nhận (chỉ mục đang dừng ở ~16/10 16:41Z).
- Chưa test: hoá đơn XML (R6), kỳ chờ nha khoa, người yêu cầu ≠ bản thân.
- Ngoại tệ: hiện số gốc trên cổng KH, thư AG-5 tiếng Anh, cập nhật tỷ giá định kỳ, chốt quy tắc ngày giường theo đêm (xem Vòng 6 · Còn mở).
