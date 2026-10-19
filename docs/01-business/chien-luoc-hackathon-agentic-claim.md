# Chiến lược đưa MVP1 + BnK IDP đi thi OutSystems Partner Hackathon APAC 2026

**Ngày:** 03/09/2026 · **Cơ sở:** `claude/hackathon-kbyg-notes.md`, `claude/danh-gia-mvp1-deliverables-vs-hackathon.md`, `claude/bnk-idp-05-api-spec.md`, `claude/odc-rest-consume-spec-bnk-idp.md`.

## Luận điểm
Không đem MVP1 đi thi — đem **một lát cắt agentic của MVP1** đi thi: chạy thật trên ODC + Agent Workbench, dùng IDP thật làm tool, gắn tên khách hàng thật.

## 1. Định vị
**"Agentic Health Claim Adjudication"** — hồ sơ bồi thường sức khoẻ từ ảnh chứng từ đến quyết định chi trả, do nhóm agent phối hợp trên ODC; con người can thiệp đúng chỗ cần phán đoán.
- Agent Workbench = bộ não (phần được chấm).
- BnK IDP = tool đọc chứng từ + cảnh báo (API thật, ~16 s/trang, flow 28/8 ~30 s).
- Core bảo hiểm (InsureMO / Bicins / DataPlatform MBAL) = nguồn dữ liệu hợp đồng, **mock trong hackathon** → "không khoá vào core nào".

## 2. Phạm vi
- **Bỏ J1**, bỏ mobile, bỏ 8 loại chứng từ → chỉ 3–4 loại IDP đã có template (5384/5127 giấy ra viện, 5383 bảng kê, 5299 đơn thuốc, 5340 xét nghiệm).
- Hợp đồng HC-STD/PLS/PRM seed vào Entity (theo P-02 bản chụp).
- **3 kịch bản demo:** (a) sạch < 2 triệu → tự duyệt; (b) NOTCOV thực phẩm chức năng + vượt trần/lần → Clinical agent đề xuất, cán bộ xác nhận, chạy lại before/after; (c) ngày sự kiện tương lai / bệnh viện không khớp → Fraud-signal agent chặn cổng.

## 3. Kiến trúc agent
| Agent | Vai trò | Tool |
|---|---|---|
| Orchestrator | Điều phối, chờ Track 1 + 2 ngã ngũ, cổng 4 điều kiện, audit trail, panel F56 | Server Action CLM_Core |
| Intake | Nhận file, gọi IDP, checklist theo `templateId`, yêu cầu bổ sung, `EXTRACTION_ERROR` | `IdpSubmitSync`, `IdpGetRawFile` |
| Adjudication | Gọi chuỗi 14 bước như tool, diễn giải từng bước tiếng Việt — không tự tính | `RunAdjudicationChain` |
| Clinical | Nhận 20 cảnh báo flow 28/8 + lines, tra KB, trả NOTREL/NOTCOV + evidence + band (lấp H7–H9) | `IdpStartAssessment`, `IdpGetAssessment`, KB |
| Fraud-signal | Tra bệnh viện OpenSearch, trùng hồ sơ, điều kiện 4 của cổng | `IdpSearchHospital`, Entity Claim |
| Customer-communication | Thư kết quả/từ chối tiếng Việt theo mẫu NOTREL, không nói "kê sai" | Template thư |

Nguyên tắc: **tiền nằm trong Server Action (deterministic), phán đoán nằm trong agent**.

## 4. Biến demo thành sản phẩm
- **Số đo thật:** `POST /request/compare` với 10–20 chứng từ ẩn danh + XLSX ground-truth → số độ chính xác đầu tiên; ROI one-pager từ baseline BIC (SLA 3/5 và 10/14 ngày, 990 hồ sơ/năm, 4,3 tỷ) + 16 s/trang.
- **Tên khách hàng:** xác nhận MBAL (dữ liệu IDP toàn nhãn MBAL) hoặc BIC; nếu không nêu tên → "bảo hiểm nhân thọ bancassurance top 5 đang thử nghiệm IDP cho claim".
- **Cam kết nguồn lực:** đội 3 người sau 22/10, mục tiêu Forge/Industry Solutions, pilot với khách hàng nêu trên.

## 5. Lịch 4 tuần
| Giai đoạn | Việc | Kiểm chứng |
|---|---|---|
| 3–6/9 | Chốt 3 kịch bản; interim B-19/20/21; che PII, bộ chứng từ demo; tài khoản IDP vai Tenant; app ODC 4 module; Entity + seed; REST login + `request/sync` | Upload PDF trên ODC → JSON trích xuất trong Entity |
| 7/9 Enablement | Hỏi SA: agent gọi Server Action làm tool, agent gọi agent, guardrail/audit, giới hạn EAP | Chốt kiến trúc agent tối 7/9 |
| 8/9 Hackathon | Dựng Orchestrator + Clinical agent tại chỗ; mock I-16 JSON tĩnh nếu cần | 1 agent chạy trên tenant, SA OutSystems đã thấy |
| 9–15/9 | Chuỗi 14 bước Server Action, cổng 4 điều kiện, hàng đợi, chi tiết hồ sơ, F56 | Kịch bản (a)(b) end-to-end với IDP thật |
| 16–22/9 | Đủ 5 agent, Fraud-signal + OpenSearch, thư tiếng Việt, màn khách hàng | 3 kịch bản pass; ghi response IDP để replay |
| 23–29/9 | Chạy `compare`, trang ROI, deck tiếng Anh, video demo dự phòng | Deck + số liệu + video |
| 30/9–6/10 | Tập pitch tiếng Anh ×3, dry-run tenant sạch, nộp bài | Sẵn sàng 7–8/10 |

## 6. Rủi ro xử lý ngay
1. **EAP chưa bật** (BNK không có trong 10 công ty đã nộp 24/8) → kiểm tra email Deniz Arin hôm nay, gọi Lynn.
2. **PII** trong chứng từ test → che trước khi lên tenant/slide (BC-1).
3. **Flow 28/8 bị sửa giữa sprint** → xin ghim version hoặc clone flow riêng.
4. **Agent Workbench chưa hỗ trợ agent gọi agent** → Orchestrator là BPT/Server Action, 5 agent gọi tuần tự; câu chuyện kể qua audit trail.
5. **Staging IDP chết ngày pitch** → replay JSON đã ghi.

## 7. Cố tình không làm
J1, sandbox InsureMO, mobile, 8 loại chứng từ, reissue tài liệu BA, đóng 7 open item (chỉ chọn interim cho 3 item đổi tiền).

## 8. Khung pitch (tiếng Anh, 7–8/10)
Problem (BIC baseline) → Solution & agent architecture → Live demo 3 kịch bản → Why deterministic money + agentic judgement → ROI + accuracy đo được → Customer + roadmap (Forge) → Ask.
