# Màn "Chứng từ của hồ sơ" v11 — giao diện AI tương lai (16/10/2026)

**Yêu cầu Mike:** màn tải chứng từ (PortalNewClaim bước 2) "wow" hơn, hơi hướng tương lai nhưng đồng bộ chủ đề; switch 2 chế độ (theo từng loại / tải tất cả 1 lần); khi tải có các bước loading, AI xử lý, nhận diện, thành công. Vòng 2: sửa lỗi "kẹt ở Upload", bố cục, độ rộng; cover mọi trường hợp.

**Bản hiện tại: MyInsurance rev 246** (0 lỗi). Lịch sử: 239 (dựng), 240–245 (sửa sau test thật), 246 (màn chi tiết hồ sơ — bỏ nháy).

## Bố cục (vì sao đổi độ rộng)
- Trước: thẻ bước 2 bị giới hạn `max-width:860px` (kế thừa từ form bước 1) → nửa phải trống, checklist bị đẩy xuống dưới danh sách tệp, chế độ "theo từng loại" thành 7 thẻ cao xếp dọc, tiêu đề cũ bị ép vào cột 53 px (ThemeGrid_Width1).
- Sau: thẻ rộng hết cột trang (1264). Chế độ AI: lưới 2 cột — trái (pipeline, vùng thả, thông báo, danh sách tệp), phải là panel "Giấy tờ cần" 360 px dính khi cuộn. Chế độ theo loại: lưới 3 cột, thẻ gọn, bắt buộc xếp trước, "Chụp thêm chứng từ khác" thành thanh ngang cuối. ≤1100 px: 1 cột, panel về dưới; ≤640 px: thẻ 1 cột, switch full width.
- Làm bằng CSS grid trên `S2Card` (If của OutSystems không sinh thẻ bọc nên các khối là con trực tiếp) — không di chuyển widget.

## Các trường hợp đã test thật (hồ sơ nháp "Nha khoa Test UI", ảnh D2_ca2 A_dien-tap + 1 ảnh không phải chứng từ)
| # | Trường hợp | Kết quả |
|---|---|---|
| 1 | Chưa có tệp | Pipeline bước 1, panel "0/1 bắt buộc", Bảng kê "Còn thiếu" |
| 2 | Đang tải lên | Dòng "Đang tải lên…" (thanh sọc chạy) + banner "Đang tải lên N tệp…" — tự xoá khi server trả về (rev 244: StartAutoUpload + IsUploadingFiles + bắt lỗi) |
| 3 | AI đang đọc | Bước 2 sáng tím, dòng tệp có tia quét, banner vòng xoay "Đang đọc N chứng từ…" |
| 4 | Thiếu bắt buộc | Bước 3 chuyển cam, banner cam "Còn thiếu: Bảng kê chi phí…", panel "1/2" |
| 5 | Đủ bắt buộc | 4 bước xanh ✓, banner xanh "Đã đủ giấy tờ bắt buộc", panel "2/2" |
| 6 | Tệp không phải chứng từ | Dòng cam "Không nhận ra", gợi ý chụp lại; không ảnh hưởng hồ sơ |
| 7 | Thêm tệp tuỳ chọn sau khi đủ | Đơn thuốc nhận diện, panel "Đơn thuốc · Tuỳ chọn · Đã nhận" |
| 8 | Chuyển sang "Theo từng loại" | Bảng kê/Phiếu khám "Đã nhận · AI tự nhận diện", nút "Thêm ảnh", thẻ viền xanh; thẻ bắt buộc còn thiếu viền đỏ |
| 9 | Nền tối | Đã chỉnh icon/nhãn/ánh sáng quét cho nền tối |
| 10 | Lỗi mạng khi tải | Gặp thực tế: trước rev 244 dòng "Đang tải lên" kẹt; rev 244 thêm xử lý lỗi xoá dòng + thông báo "Tải lên chưa thành công — vui lòng thử lại" (chưa tái hiện lại được lỗi mạng để kiểm) |

## Lỗi đã sửa sau test (240–245)
- Pipeline kẹt bước 1 → tính lại từ dữ liệu thật; dùng chung hàm đếm bắt buộc với panel (trước đó báo "đủ" khi còn thiếu).
- Bộ đếm "7/7 bắt buộc" → chỉ đếm giấy Bắt buộc.
- Nhãn loại giấy tờ rơi xuống dòng 2, bị cắt → 1 hàng.
- Chế độ theo loại không thấy giấy AI đã nhận → dùng chung IsDocReceived.
- Dòng "Đang tải lên" không tự xoá, cộng dồn → luồng StartAutoUpload duy nhất.
- Hiệu ứng bật lại mỗi lần poll (nhấp nháy) → tắt animation một lần trên dòng đã xong.
- 2 vòng xoay trong banner; icon cũ chồng icon mới.

## Còn lại / gợi ý
- Tệp "Không nhận ra" chưa có nút xoá/chọn loại thủ công (cần logic mới).
- Chưa đo trên thiết bị di động thật (CSS có breakpoint 1100/640).
- Đã tạo vài hồ sơ nháp test "Nha khoa Test UI" (nháp không hiện trong danh sách hồ sơ của khách).

## Màn chi tiết hồ sơ (PortalClaimDetail) — bỏ nháy, hiệu ứng gương (rev 246)
- **Nguyên nhân nháy:** AutoRefresh chạy mỗi 15 s (setInterval trong OnReady, dừng khi hồ sơ đã có kết quả cuối). Mỗi lần làm mới, GetClaim.IsDataFetched tạm về False nên PayoutSkelIf chèn khung xương (PayoutSkelDiv) vào thẻ chính khoảng 150 ms, làm thẻ giật rồi hiện lại.
- **Sửa:** biến HasLoadedOnce, gán True trong OnAfterFetch của GetClaim. Khung xương chỉ hiện ở lần tải đầu (`not IsDataFetched and not HasLoadedOnce`). Đo lại 33 s: 0 thay đổi DOM trong thẻ chính.
- **Hiệu ứng gương:** node JS SyncSheen ở đầu AutoRefresh và OnRefreshClaim gắn `body.v11-sync` trong 1,7 s. CSS cho một vệt sáng nghiêng quét qua thẻ chính (trắng) và thẻ "Chi tiết từng khoản" (tím nhạt, trễ 0,12 s); chấm xanh cạnh "Cập nhật hh:mm" nháy 3 nhịp. Dùng `::before` vì `::after` đã có chỗ dùng; selector `html body .card.v7-receipt::before` để thắng `.v7-cols2d .v7-receipt::before{content:none}`. Có tắt hiệu ứng khi máy bật chế độ giảm chuyển động (prefers-reduced-motion).
- **Dòng gợi ý DocStateExpr2:** trước là chữ xám #5B6472 trên nền teal đậm nên khó đọc, nội dung chung chung. Giờ là ô bo góc nền trắng 10 %, chữ trắng 92 %, có chấm trạng thái. Khi đang đọc: chấm tím nhấp nháy, "Chúng tôi đang đọc chứng từ… vài phút". Khi có tệp lỗi: chấm cam, "Có N tệp chưa đọc được (ảnh mờ hoặc không phải chứng từ). Quý khách có thể tải lại ảnh rõ hơn bằng nút Bổ sung chứng từ phía trên."
