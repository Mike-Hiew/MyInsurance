# Kế hoạch áp mockup v1.0 lên ClaimMVP & MyInsurance (lập 13/10/2026, sửa 23:55)

Bản đầy đủ (doc): https://claude.ai/code/artifact/028ea5b4-491b-4d5d-9bda-33423af41085
Nguồn: `E:\Hackaton\Prototype_Insurance_BnK_v1.0.html`. Điểm xuất phát: ClaimMVP rev 689, MyInsurance rev 172 (đã kiểm 13/10 23:30).

## Phần mới v1.0 so với v0.9 (22 mục, đều có điểm trong plan)
Theme navy + Sora · nút sáng/tối hiển thị (N3) · hiệu ứng · đầu hồ sơ hero + dải 5 cột teal + ánh gương · thẻ AI gọn · tab viên · Work list: 4 thẻ lọc, thanh lọc gọn, chip 3 giai đoạn, sắp theo ưu tiên, bảng mới, xem theo giai đoạn (C12, có thể cắt) · Dashboard kiểu HomeBankingPortal + dải phải · PortalHome 3 thẻ quyền lợi · kết quả hồ sơ teal + confetti · top menu · chuông · header điện thoại 2 hàng · Login mới · nhãn nền nhạt · **đăng nhập nhanh bằng tài khoản mẫu theo vai (L1–L6)**.

## Nút sáng/tối (N3, đợt 0)
Hiện cạnh EN|VI ở cả hai app. :root sáng / :root.dark-mode tối; ToggleTheme đổi class trên html + localStorage (try/catch); OnApplicationReady áp trước khi vẽ. Mọi rule màu dùng token. Lỗi cũ rev 426: chỉ gắn os-dark-mode mà không có rule CSS. (HomeBankingPortal có ToggleDarkMode theo client.IsDarkMode.)

## Đăng nhập bằng tài khoản mẫu (L1–L6)
- Cách HomeBankingPortal: DoLogin(Username, Password) gọi login hệ thống + CheckAndGrantRole(UserId). ClaimMVP/MyInsurance: cùng cách đăng nhập nhưng vai gán sẵn, không cấp vai lúc login.
- L1 5 tài khoản Development: ClaimOfficer, ClaimApprover, ClaimOps, Presenter, Customer (người dùng tạo trong ODC Portal; Claude không tạo tài khoản/nhập mật khẩu).
- L2 username = setting, mật khẩu = Secret setting. L3 server action SampleLogin(RoleKey) có chốt EnableSampleLogin (mặc định False) + log. L4 khối thẻ vai trên Login (ẩn khi tắt). L5 vào thẳng: WorkList / ApprovalQueue / OpsQueue / AgentTrace / PortalHome. L6 vòng cam quanh avatar + nhãn "Tài khoản mẫu · demo" + "Đổi tài khoản mẫu / vai".
- L1–L2 sáng 14/10 (người dùng); L3–L6 trong đợt 1 (ClaimMVP) và đợt 4 (MyInsurance). Đóng luôn C5 (kiểm vai bằng 2 tài khoản, bốn mắt).
- Mockup v1.0 đã có khối này (nhãn "chưa có trên hệ thống").

## 6 đợt, xong trước freeze 22:00 T5 15/10 (giả định)
- Đợt 0 (T4 14/10 sáng): N1–N6 + nút sáng/tối, cả hai app.
- Đợt 1: ClaimDetail C1–C3 + đăng nhập mẫu L3–L6 (ClaimMVP).
- Đợt 2: WorkList C4–C6 + C12 board (có thể cắt).
- Đợt 3 (T5 15/10 sáng): Dashboard C7–C8.
- Đợt 4: MyInsurance P1–P5, P9 + đăng nhập mẫu khách hàng.
- Đợt 5 (có thể cắt): top menu + chuông + Login, C9–C11, P6–P8.

## Cổng sau mỗi publish
Validation ClaimMVP 7 lỗi nền / MyInsurance 0 · ChainTests 46/46, gate 5/5 · theme không mất rule (`content:''`) · kiểm VI + EN, sáng + tối.

## Cần chốt
Chế độ mặc định khi chưa bấm nút (đề xuất ClaimMVP tối, MyInsurance sáng) · có làm đợt 5 trước freeze không · nguồn ô "Chờ chi trả" và "Độ chính xác đọc chứng từ" · ngày freeze thật · ai tạo 5 tài khoản mẫu + tài khoản khách gắn thành viên nào (đề xuất Nguyễn Văn An, có 0440/0456).
