# Đánh giá bộ MVP1 (00. Deliverables) so với yêu cầu Hackathon OutSystems

**Ngày:** 03/09/2026. **Nguồn:** `E:\Hackaton\insurance BNK\Bnk IDP\00. Deliverables` (14 file: A1–A5, D1–D3, Product_Definition_MVP1_v3.1.pptx, Prototype_MVP1_v1.6.html, Prototype_Clinical_Track_MVP1 v1.0→v3.2) đối chiếu với `claude/hackathon-kbyg-notes.md`.

## Kết luận ngắn
Bộ MVP1 **đủ và dư về mặt tài liệu BA** (BRD 12 phần, FS ~1 MB, Integration Spec 16 giao diện, 5 phụ lục, 2 prototype HTML ~60 màn hình) nhưng **chưa đủ để nộp hackathon**, vì thiếu đúng phần được chấm điểm: thiết kế agentic, ODC + Agent Workbench, ROI, khách hàng thật.

## Đối chiếu từng tiêu chí

| Tiêu chí hackathon | Trạng thái trong MVP1 | Ghi chú |
|---|---|---|
| Ngành focus (Insurance) | ✅ Đạt | Bảo hiểm sức khoẻ, bồi thường chi trả sau |
| Use case trọn vẹn, có đầu–cuối | ✅ Đạt | J1 mua 13 bước, J2 bồi thường 9 bước, 13 trạng thái, 14 bước tính |
| Deployable, không slideware | ⚠️ Một phần | Prototype HTML rất chi tiết, nhưng chưa có gì chạy trên ODC |
| **Build trên ODC** | ⚠️ Chỉ có bản đồ | Prototype v3.2 có "Bản đồ triển khai OutSystems": 4 module (CLM_Core, CLM_IDP_Int, CLM_Console, CLM_MobileApp), Entity, Server Action, BPT, Timer, REST — nhưng là ODC cổ điển |
| **Agent Workbench** | ❌ Không có | 0 lần nhắc trong toàn bộ 14 file |
| **Agent-to-agent** | ❌ Không có | AI duy nhất là BnK IDP gọi qua REST (I-07/I-09/I-15/I-16), nguyên tắc "đề xuất, không quyết định" (AR-1). Không có orchestration, không có agent nào nói chuyện với agent nào |
| ROI rõ ràng | ❌ Không có số | Slide 4 baseline "Days to weeks / 100% keyed by hand" ghi rõ *"Illustrative… to be replaced with the client's measured baseline"*. Từ "ROI" không xuất hiện trong D1/D2/A5 theo nghĩa business case |
| Khách hàng thật (external) | ❌ Chưa gắn | Tài liệu là sản phẩm của BnK; BIC chỉ là hệ tham chiếu; MVP1 tự khai là demo (BC-1..4: không khách thật, không tiền thật) |
| Nguồn lực cam kết | ❌ Chưa nêu | B-11 (ngày demo/dev start) còn Open; team shape chưa chốt (slide 34) |
| Pitch tiếng Anh | ✅ Có sẵn | Deck và D1–D3 viết tiếng Anh; prototype tiếng Việt |

## Điểm còn hở bên trong MVP1 (ảnh hưởng demo)
- 7 open item trong D1 Part XI, trong đó **B-19, B-20, B-21 làm đổi số tiền chi trả** trên chính sản phẩm pilot; B-01 (pilot product) và B-07 (roster) re-opened ở v0.5.
- RK-14 InsureMO là phụ thuộc cứng không fallback; RK-08 KB lâm sàng chưa có sign-off y khoa.
- ~20 điểm lệch pha giữa các tài liệu (7 vs 8 loại chứng từ, cổng 3 vs 4 điều kiện, 1.920.000 vs 2.000.000…).
- 16 giao diện mock chưa xây.

## Cần bổ sung để đủ hackathon
1. **Lớp Agentic trên Agent Workbench** (đây là thứ được chấm): tách J2 thành các agent có vai trò — Intake/Checklist agent (đối chiếu chứng từ, hỏi bổ sung), Adjudication agent (gọi chuỗi 14 bước như một tool, không tự tính), Clinical agent (Track 2), Duplicate/Fraud-signal agent (điều kiện 4 của cổng), Customer-communication agent (thư từ chối NOTREL/NOTCOV bằng tiếng Việt), và một Orchestrator điều phối + audit trail. Diễn giải được luồng agent-to-agent khi pitch.
2. **Build thật trên ODC** theo "Thứ tự dựng cho hackathon" đã có sẵn trong Prototype v3.2 (Entity + seed → mock I-16 → panel F56 → Server Action 14 bước → cổng 4 điều kiện → màn kết quả), nhưng đưa các bước điều phối vào Agent Workbench thay vì chỉ BPT.
3. **ROI one-pager**: lấy baseline từ BIC (SLA 10/14 ngày dự phòng, 3/5 ngày xử lý; 990 hồ sơ/2026; 4,3 tỷ đã chi) và mục tiêu SC-01..SC-09 → giờ thẩm định tiết kiệm, tỷ lệ STP, cycle time.
4. **Gắn tên khách hàng thật** (BIC hoặc ngân hàng bancassurance) và **cam kết nguồn lực** sau 22/10.
5. Chốt tối thiểu B-19/B-20/B-21 (chọn phương án interim và ghi rõ) để số tiền trong demo không bị chất vấn.
