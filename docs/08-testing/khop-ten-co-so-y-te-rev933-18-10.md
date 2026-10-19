# Khớp tên cơ sở y tế với danh mục IDP — ClaimMVP rev 933 (18/10/2026)

## Thay đổi (rev 929 → 933)
- Tên tiếng Anh được dịch sang tiếng Việt trước khi so: Hospital → Bệnh viện, Clinic → Phòng khám, General → Đa khoa, International → Quốc tế, Medical Center → Trung tâm y tế, Polyclinic → Phòng khám đa khoa; viết tắt BV, BVĐK, PK, PKĐK, TTYT, TYT, ĐK, TW được mở rộng.
- Bỏ từ chung (bệnh viện, phòng khám, đa khoa, quốc tế, trung tâm, trạm y tế, công ty, TNHH, xã, phường…) rồi so phần tên riêng.
  - Cụm "trung tâm", "thành phố", "thị trấn", "chi nhánh", "cổ phần" chỉ bị bỏ khi đi nguyên cụm (rev 933).
  - Vì vậy các chữ Tâm, Trưng, Phố, Phan… vẫn được giữ.
- Mức khớp:
  - Cùng tên riêng, cùng thứ tự → CATALOGUE.
  - Một bên chứa bên kia → PARTIAL.
  - Sai chính tả 1 ký tự chỉ được chấp nhận khi **cả hai** từ dài ≥ 5 chữ (rev 933).
  - Tên riêng 1 từ ngắn (FV) chỉ được nhận khi đúng 1 dòng chứa từ đó.
- Chọn dòng (giống nhau ở màn ProviderLookup và LookupProvider/AG-4), theo thứ tự ưu tiên:
  1. CATALOGUE.
  2. Ít từ thừa hơn.
  3. Cùng loại cơ sở với tên tra (BV / TTYT / TYT / PK — thêm ở rev 933).
  4. Điểm IDP cao hơn.
- Không có dòng nào khớp → Found = False, MatchType = NOT_FOUND.

## Kiểm tra trên rev 933
- Tự kiểm khớp tên: 47/47 đạt (36 ca cũ + 11 ca mới).
- ChainTests: 46/46, cổng 5/5.
- Tra trực tiếp:

| Tên tra | Dòng được chọn |
|---|---|
| Vinmec International Hospital | 01161 |
| Tam Anh General Hospital | 01250 BV ĐK Tâm Anh (rev 932 không chọn được dòng nào) |
| Bệnh viện Trưng Vương | 79026 |
| Hong Ngoc Hospital | BV ĐK Hồng Ngọc |
| Nội tiết Trung ương | Dòng TW |
| FV Hospital | 79509 |
| Chợ Rẫy | 79048 |
| Phòng khám Fixture | Không chọn dòng nào |
| Clinic | Không chọn dòng nào |

## Còn mở
- Hanoi French Hospital, Raffles: không có trong bảng 322 → cần bảng tên khác (alias).
- Dòng danh mục không có mã (BV ĐK Hồng Ngọc, BV Nội tiết TW): vẫn được chọn, nhưng mã cơ sở rỗng.
- Khi API IDP lỗi, LookupProvider trả Found = True, MatchType = CATALOGUE_UNAVAILABLE. Hành vi này đã có từ trước, chưa đổi.
- Dòng "Từ đặc trưng" và nút tự kiểm trên màn ProviderLookup là công cụ test, có thể ẩn trước demo.
