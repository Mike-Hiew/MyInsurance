# Biên bản demo nội bộ My Insurance × BnK IDP · 08/10/2026

Bản đầy đủ (doc, có bảng theo dõi): https://claude.ai/code/artifact/2df20fcc-fa11-4675-ac89-4fd2939aa0bc
Nguồn: transcript Teams tự động (~1h25). Dự: Hiếu Mai (trình bày), Nhân Vương, Phú Nguyễn (CDO), Henry Mai (SVP), Tài Trần (Director AI & Consulting), Trung Nguyễn (PMO).

## Demo
- Ca 1 ngoại trú Vinmec 400.000 → AG-4 báo trùng (bộ chứng từ đã nộp trước), gỡ tín hiệu → tự duyệt 320.000.
- Ca 2 nha khoa "Demo Smile": cơ sở ngoài danh mục IDP; AG-3 loại tẩy trắng 3,5tr (95%); phải bấm "Tính và giải thích" tay → 1.020.000.
- Ca 3 thuốc không liên quan / ngày tương lai: AG-4 lỗi nhưng không chặn; AG-3 loại vitamin; IDP đọc thiếu 3 số 0 ở một dòng tiền.
- Mức hoàn thành ~80–90%; làm 1,5–2 tuần.

## Recommend chính
1. Thêm phần kỹ thuật cho hackathon: OutSystems, Agent Workbench, 6 agent phối hợp/chịu lỗi (Nhân, Henry).
2. Liệt kê góp ý của anh Tài, hỏi lại ưu tiên (Henry).
3. Hiển thị agent đang chạy trong lúc chờ ~4 phút (Henry); lập luận từng bước của agent, tool call (Tài).
4. Giao diện + chứng từ mẫu tiếng Anh (Henry, Tài).
5. Ca y khoa khó hơn để thể hiện thẩm định (Phú); nói rõ bước AI đọc → ghi dữ liệu → sửa (Phú).
6. AI điền sẵn từ chứng từ, khách chỉ sửa; tải một lần cả bộ, IDP tự phân loại/tách; báo thiếu chứng từ ngay (Tài). Anh Phú phản biện: quy tắc bảo hiểm bắt khách tự khai rồi đối soát — chưa thống nhất.
7. Đưa luật chính sách lên OutSystems, cấu hình theo gói; gọi IDP từng API, tự điều khiển luồng (Tài, Phú). Trung: workflow IDP là một khối.
8. AI chạy trọn luồng, người chỉ quyết bước cuối; agent dùng tool call/tự lập kế hoạch (Tài).
9. Dashboard cho người duyệt + chế độ nâng cao (Tài); tách bước duyệt sang tab/vai riêng (Nhân).

## Lỗi thấy trong demo
- Bộ chứng từ tái sử dụng bị báo trùng; chuỗi không tự chạy lại sau xác nhận AG-3; AG-4 lỗi ở ca 3; IDP vẫn đọc thiếu 3 số 0; không có tiến trình hiển thị khi chờ.
