# Đăng nhập mẫu một chạm — ClaimMVP rev 810 · MyInsurance rev 254 (17/10)

**Yêu cầu của Mike:** bỏ kiểu thẻ vai chỉ điền sẵn tên đăng nhập, đổi sang bấm một lần là vào, cho cả hai app và đủ các vai. Mục đích là để Claude tự đăng nhập và test, không cần Mike.

## Đã làm
| App | Thẻ (RoleKey) | Setting tên đăng nhập / mật khẩu | Màn vào sau khi đăng nhập |
|---|---|---|---|
| ClaimMVP | officer | SampleUser_Officer / SamplePassword_Officer | WorkList |
| ClaimMVP | approver | SampleUser_Approver / SamplePassword_Approver | ApprovalQueue |
| ClaimMVP | ops | SampleUser_Ops / SamplePassword_Ops | OpsQueue |
| ClaimMVP | presenter | SampleUser_Presenter / SamplePassword_Presenter | AgentTrace |
| MyInsurance | Customer | SampleUser_Customer / SamplePassword_Customer | PortalHome |

- `SamplePassword_*` là setting bí mật (Secret). `EnableSampleLogin` mặc định False; khi tắt thì các thẻ bị ẩn và server action không trả về gì.
- Mỗi lần đăng nhập mẫu ghi một dòng log `SampleLogin` gồm RoleKey, tên đăng nhập và kết quả. Log không chứa mật khẩu.
- Đăng nhập thành công: `IsSampleAccount` = True, góc dưới bên trái hiện nhãn cam "Sample account · demo" (sửa ở rev 809), rồi chuyển vào màn của vai.
- Rev 809–810: mỗi thẻ có tên vai theo đúng ngôn ngữ đang chọn, kèm dòng gợi ý "One tap · demo / 1 chạm · demo" nằm dưới tên. Dòng ghi chú dưới các thẻ đổi thành "One tap signs you in with a demo account". Đã bỏ chuỗi "\n" bị hiện nguyên văn và chữ hai thứ tiếng dính liền. Bố cục mỗi thẻ: icon bên trái, tên vai và gợi ý xếp dọc bên phải.

## Kết quả test (17/10, 09:05–09:25, Mike đã điền setting và bật cờ)
| Thẻ | Tài khoản | Màn vào sau khi đăng nhập |
|---|---|---|
| ClaimMVP · Claim officer | mike | WorkList ✓ |
| ClaimMVP · Approver | Mai Trung Hieu | ApprovalQueue ✓ |
| ClaimMVP · Operations | Mai Trung Hieu | OpsQueue ✓ |
| ClaimMVP · Presenter | Mai Trung Hieu | AgentTrace ✓ |
| MyInsurance · Khách hàng | Mai Trung Hieu | PortalHome ✓ ("Chào An") |

- ChainTests rev 808: 46/46, gate 5/5 (chạy 09:06).
- **Lưu ý:** Approver, Ops và Presenter đang trỏ cùng một tài khoản (tài khoản của Mike). Muốn test phân quyền thật, kể cả kiểm tra bốn mắt (người thẩm định và người duyệt phải khác nhau), cần mỗi vai một tài khoản riêng.
- **Lỗi phát hiện:** nút Sign out (`LogoutBtn`) có đăng xuất, nhưng không tự chuyển về trang Login; người dùng phải tự mở lại trang.
- Trình duyệt Chrome dùng chung phiên đăng nhập cho mọi tab. Khi Claude đăng nhập bằng thẻ mẫu, các tab Mike đang mở cũng chuyển sang tài khoản đó. Sau khi test xong, Claude đăng nhập lại bằng thẻ Approver (tài khoản của Mike).

## Rủi ro bảo mật
- Trên ODC, action Login của hệ thống chạy ở trình duyệt (client action). Vì vậy server action (ClaimMVP: `GetSamplePassword`, MyInsurance: `GetSampleCredentials`) phải trả mật khẩu mẫu về trình duyệt, và màn Login cho phép truy cập ẩn danh.
- Hệ quả: khi cờ đang bật, bất kỳ ai mở được trang Login cũng lấy được mật khẩu của các tài khoản mẫu.
- Cách giảm rủi ro:
  - Chỉ dùng tài khoản mẫu riêng, không dùng tài khoản cá nhân.
  - Chỉ dùng trên môi trường Development.
  - Chỉ bật cờ khi test hoặc demo.

## Ghi chú
- MyInsurance: bảng Member không có cột UserId, nên mọi tài khoản khách hàng đều thấy cùng một thành viên demo (thành viên có Id nhỏ nhất).
