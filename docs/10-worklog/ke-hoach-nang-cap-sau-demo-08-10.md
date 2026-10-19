# Kế hoạch nâng cấp sau demo 08/10 — UI/UX & Tính năng (09/10/2026)

Bản đầy đủ (doc, có tracker Ưu tiên/Trạng thái): https://claude.ai/code/artifact/3e3a2564-954b-4b46-b3a9-53eb3dfaa8eb
Nguồn: claude/bien-ban-demo-08-10.md

## Phần A — UI/UX
- A1 (P0) Panel "Agent đang làm gì" trong lúc chờ ~4 phút: 6 ô IDP → AG-1 → AG-3 → AG-4 → Chuỗi+cổng → AG-5, mũi tên theo HandoffTo, refresh 3 s. Cần B5.
- A2 (P0) Agent trace trong tab Lịch sử & nhật ký: lập luận 2–4 ý, ToolName (chip BnK IDP), InputJson/OutputJson. Cần B5.
- A3 (P0) Tab Trích xuất: dải ① IDP đọc → ② ghi → ③ kiểm & sửa (R1–R9), cột Nguồn IDP/Người sửa, viền đỏ ô vi phạm R.
- A4 (P0) Pitch EN: app đã song ngữ; sửa phần agent/chuỗi sinh ra còn tiếng Việt, AG-3/4/5 theo Lang.
- A5 (P1) Tab Tóm tắt cho người duyệt + chế độ nâng cao. A6 (P1) role ClaimApprover + hàng đợi duyệt. A7 (P1) MyInsurance báo thiếu chứng từ ở màn nộp (cần B7).

## Phần B — Tính năng
- B1 (P0) Tự chạy RunAdjudicationChain khi hết đề xuất AG-3 Proposed (chống trùng theo InputsHash).
- B2 (P0) Sửa AG-4 lỗi ca 3, nhánh "không kết luận được".
- B3 (P0) ParseVndAmount (dấu chấm nghìn), cờ số tiền < 1.000 ₫, đối chiếu tổng dòng vs hóa đơn, mô tả trường tiền ở template 6090–6098.
- B4 (P0) Nút "Chuẩn bị demo" lưu trữ hồ sơ demo cũ, nhiều bộ chứng từ khác số hóa đơn, giữ 1 ca trùng cố ý.
- B5 (P0) Khối reasoning trong output agent, AgentRun cho mỗi tool call, entity AgentProgress cho trạng thái live.
- B6 (P0) 3 ca khó (bệnh có trước hiệu lực, MRI không khớp viêm họng, nội trú lệch ngày); thử 1 chứng từ EN với IDP trước.
- B7 (P1) Kiểm thiếu chứng từ khi tải lên.
- Roadmap P2: R1 AI đối chiếu tờ khai (dung hòa Tài/Phú), R2 tải một lần + tự tách, R3 rule trong OutSystems gọi IDP từng API, R4 AI trọn luồng, R5 agent tự lập kế hoạch.

## Thứ tự
Đợt 1: B1–B5 · Đợt 2: A1–A3 · Đợt 3: B6, A4 · Đợt 4 (P1): B7, A7, A5, A6. P0 ≈ 5–7 ngày công (~3 ngày nếu 2 người song song).
