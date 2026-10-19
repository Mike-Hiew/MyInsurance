# E2E vòng cuối trước quay demo — 18/10/2026

Bản cuối: **ClaimMVP rev 951 · MyInsurance rev 333** (môi trường bnk-dev). ChainTests: **46/46 pass · gate 5/5** (chạy lại trên ClaimMVP 951 lúc 22:32).

## 1. Kết quả từng ca (hồ sơ tạo mới hôm nay)

| Ca | Hồ sơ | Kịch bản | Kết quả hệ thống | Đạt? |
|---|---|---|---|---|
| E04 + E04S | CLM-2026-0604 | Ngoại trú, Hà, Thanh Nhàn, thiếu phiếu khám → bổ sung | Chờ bổ sung → bổ sung qua cổng → tiếp tục; AG-4 bắt **DUPLICATE HIGH** (trùng 0602/0603 do test lặp) | ✅ (luồng bổ sung + trùng) |
| E05 | CLM-2026-0606 | Nha khoa, An, Vinmec | 750.000 → **525.000 ₫** (đồng chi trả 30%), tự duyệt, Paid | ✅ |
| E06 | CLM-2026-0607 | Tai nạn ngoại trú, Hà, Bạch Mai | **830.000 ₫**, không đồng chi trả, tự duyệt, Paid | ✅ |
| E08 | CLM-2026-0608 | Thai sản ngoại trú, Hà, Vinmec | 1.600.000 → trần 200.1 1,5tr → **1.200.000 ₫**; officer trình → approver duyệt | ✅ (xem ghi chú 6) |
| E09 | CLM-2026-0609 | I10, An, Tâm Anh | Chuỗi 14 bước từ chối ở bước thời gian chờ; officer trình → approver **Decline** | ✅ |
| E07B + E07BS | CLM-2026-0614 | Nội trú, An, Tâm Anh, thiếu giấy ra viện → bổ sung | Chờ bổ sung → bổ sung giấy ra viện → **tự chạy lại pipeline** → **5.900.000 ₫** (trần giường 1,5tr/ngày); AG-4 DATE_IMPOSSIBLE do ngày ký trên giấy ra viện mẫu (lỗi dữ liệu test, AG-4 bắt đúng) | ✅ |
| E11 | CLM-2026-0611 | Hoá đơn USD tiếng Anh, Vinmec Central Park | 65 USD × 25.700 = 1.670.500 → **1.336.400 ₫**, tự duyệt, Paid | ✅ |
| E12 | CLM-2026-0612 | Chứng từ tên LÊ VĂN BÌNH, khai cho An | AG-4 **CROSSCHECK_FAIL** (R1 tên không khớp); officer đề xuất từ chối → approver xác nhận | ✅ |
| E03 | CLM-2026-0613 | "Tam Anh General Hospital", có TPCN | Khớp cơ sở in-network; AG-3 NOTCOV vitamin; trần 200.1 → **1.296.000 ₫** chờ người quyết | ✅ |
| S0445 | CLM-2026-0445 | Bổ sung giấy ra viện cho hồ sơ cũ | Hết "chờ bổ sung", pipeline chạy tiếp | ✅ |
| Kiểm cuối (bản 951/333) | CLM-2026-0631 | Ngoại trú E01, An, Vinmec 16/10 | Checklist 2/2 "Đã đủ giấy tờ" → gửi → đọc 3 chứng từ/4 dòng, 484.000 ₫ → AG-1 Package complete, AG-4 không tín hiệu → **387.200 ₫** (đồng chi trả 20%), gate 4/4, tự duyệt, Paid | ✅ |
| Four-eyes | 0604/0608/0609/0612 | Người đề xuất ≠ người duyệt | Server chặn "Approver must differ…"; officer → approver chạy đúng | ✅ |

## 2. Lỗi đã sửa trong vòng này (đã publish, đã kiểm lại trên giao diện)

**ClaimMVP (937 → 951)**
1. **P0 tiền VND bị chia 1000** (150.000 → 150): IDP trả ô "150" cho "150.000". Thêm lưới an toàn sau khi tạo dòng: nếu tổng dòng × 1000 ≈ TOTAL_COST (±2%) thì nhân 1000 và ghi AgentRun "VND scale fix ×1000". Đã kiểm 0604 → 0614: số tiền đúng.
2. Hồ sơ nội trú: yêu cầu "Giấy ra viện **hoặc** Báo cáo y tế" (trước chỉ nhận Báo cáo y tế → kẹt chờ bổ sung).
3. Pipeline **kẹt sau khi bổ sung chứng từ** (trạng thái track cũ chặn AG-3/AG-4/chuỗi): reset khi hồ sơ rời "chờ bổ sung".
4. Thẻ "AI đang xử lý" hiển thị **Tạm dừng — chờ khách hàng bổ sung**, đồng hồ dừng.
5. Header/thẻ đề xuất từ chối không còn in nguyên ghi chú "DUP:…" cỡ chữ lớn; hiện "Proposed: Reject · lý do · 0 VND".
6. Approver thấy nút **Confirm rejection** (thay "Approve · 0 VND"); xác nhận từ chối đi đường decline (0 ₫, đảo bút toán tạm), không bị chặn bởi tín hiệu gian lận đang mở. Đã sửa lỗi lưu "value too long varchar(20)" khi xác nhận từ chối.
7. Four-eyes lấy người đề xuất **gần nhất**; nhãn "Lý do trả về"; chip "Thời gian chờ" thay "Cap reached" cho hồ sơ bị từ chối vì thời gian chờ; dòng chi phí trước khi chạy chuỗi hiện "Pending calculation".
8. Console: **Sign out nằm trong menu tài khoản** (dòng cuối), nút ngoài header đã ẩn; vẫn qua popup xác nhận (rev 949–950).
9. Đăng xuất / vào trang Đăng nhập: **xoá toast lỗi cũ** còn sót từ màn trước (rev 951).

**MyInsurance (311 → 333)**
1. Chuông thông báo **không hiện hồ sơ của người phụ thuộc** (Hà) → lọc theo cùng hợp đồng, mới nhất lên đầu.
2. Trang Bổ sung chứng từ: danh sách hồ sơ thiếu hồ sơ người phụ thuộc, không chọn sẵn hồ sơ từ link, banner ghi sai tên bệnh nhân → đã sửa.
3. **Tạo hồ sơ lần 2 trong cùng phiên không tải được chứng từ** (OS-CLRT-60801, StartAutoUpload của màn hình cũ) → gắn lại listener mỗi lần vào màn hình.
4. Nút "Hình thức điều trị" (Nha khoa/Tai nạn/Thai sản) chưa có CSS → đã có kiểu segmented.
5. Checklist chứng từ: ngoại trú nhận Báo cáo y tế/Giấy ra viện thay Phiếu khám; nội trú bắt buộc Giấy ra viện/Báo cáo y tế; Hoá đơn VAT chỉ "Nên có" (sửa lỗi map mã loại tổn thất).
6. Hồ sơ bị từ chối: hero "0 ₫ · Không chi trả", hiện **Lý do: …** (văn bản gửi khách).
7. Hết phiên: chuyển về Đăng nhập + toast "Phiên đăng nhập đã hết hạn" thay vì báo lỗi chung.
8. Đăng xuất (cả hai app): popup xác nhận → về trang Đăng nhập → toast "Bạn đã đăng xuất thành công", EN/VI, sáng/tối, Esc/Huỷ hoạt động.
9. **Checklist bước 2 (Tạo hồ sơ) báo sai "Còn thiếu: Phiếu khám bệnh"** dù đã tải đủ → đã sửa (rev 329): dòng checklist tính lại từ loại chứng từ AI nhận diện; nhận cả chứng từ thay thế (Báo cáo y tế/Giấy ra viện thay Phiếu khám; Giấy ra viện ↔ Báo cáo y tế cho nội trú). Đã kiểm: ngoại trú E01 → 2/2 bắt buộc "Đã đủ giấy tờ"; nội trú E07 → thiếu Giấy ra viện, thêm E07S → 2/2.
10. **Trang Bổ sung, vùng "Đính kèm + Gửi"**: bấm Gửi khi chưa chọn hồ sơ / trống nội dung và không có file → trước không phản hồi (hoặc gửi bản rỗng) → nay báo "Vui lòng chọn hồ sơ cần bổ sung." / "Vui lòng nhập nội dung hoặc đính kèm chứng từ."; gửi thành công có thông báo (rev 330–333).
11. Toast cảnh báo cũ còn hiện trên trang Đăng nhập sau khi đăng xuất → đã xoá khi đăng xuất và khi mở trang Đăng nhập; toast "đăng xuất thành công" vẫn hiện (rev 332).

## 3. Còn lại / lưu ý khi quay demo

1. **Hai app dùng chung phiên đăng nhập** trên cùng trình duyệt: đăng xuất console là đăng xuất luôn cổng khách hàng, và đăng nhập console bằng tài khoản mẫu sẽ đổi danh tính ở cổng. Khi quay: **mở cổng khách hàng và console ở 2 profile Chrome khác nhau (hoặc 1 cửa sổ ẩn danh)**.
2. Phiên hết hạn sau khoảng 1 giờ — đăng nhập lại ngay trước khi quay.
3. Trang Bổ sung chứng từ: vùng tải phía trên (AI tự nhận diện, tự gửi) là đường đẹp nhất để quay. Hồ sơ cũ CLM-2026-0004 có 4 chứng từ treo "Đang nhận diện…" từ đầu tháng — đừng chọn hồ sơ này khi quay.
4. Cần Mike xác nhận nghiệp vụ: (a) thai sản ngoại trú đang tính theo quyền lợi 200.1 (trần 1,5tr/lần, 20%) → 1.200.000 ₫; (b) màn Events & cover ghi "2 days" cho nội trú 08–10/10 trong khi trần giường tính 3 ngày.
5. Danh mục cơ sở y tế chưa có "Bệnh viện Thanh Nhàn" (vẫn gửi được, ghi "Không có trong danh mục").
6. **Dữ liệu test bẩn** (đề xuất Mike quyết định lưu trữ/archive): CLM-2026-0535–0583 (cũ), 0600–0603 (số tiền sai ÷1000 trước khi sửa + bản lặp), 0605 (nháp hỏng), 0610 (kẹt trước bản sửa), 0631 (gửi kiểm lần cuối), các bản nháp chưa gửi tạo khi test checklist (An · Vinmec 16–17/10, An · Tâm Anh 01–03/10), và **CLM-2026-0004 nhận 3 bản bổ sung thử** (2 rỗng + 1 "Test nội dung") trong lúc kiểm nút Gửi. Các hồ sơ này làm AG-4 báo trùng nếu demo lại cùng ngày/cùng cơ sở.

## 4. Checklist trước khi quay

- [ ] Đăng xuất: console = dòng "Sign out" cuối menu tài khoản; cổng = "Đăng xuất" trong menu tài khoản — cả hai có popup xác nhận.
- [ ] 2 profile Chrome: (A) cổng khách hàng — thẻ "Khách hàng · mẫu"; (B) console — Claim officer / Approver.
- [ ] Đăng nhập lại cả hai ngay trước khi quay (tránh hết phiên).
- [ ] Dùng ngày điều trị/cơ sở **chưa dùng** để không dính cảnh báo trùng (hoặc archive dữ liệu test ở mục 3.6).
- [ ] Kịch bản gợi ý: E05 (tự duyệt nhanh) → E11 USD → E04 thiếu chứng từ + bổ sung → E12 gian lận tên → officer đề xuất từ chối → approver xác nhận → khách xem lý do.
