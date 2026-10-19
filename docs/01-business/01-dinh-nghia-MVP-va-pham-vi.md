# Định nghĩa MVP và phạm vi — cái gì làm, cái gì cố tình không làm

## Ba thứ cùng tên "MVP" — đừng lẫn

| Tên | Là gì | Vai trò trong hackathon |
|---|---|---|
| **MVP1** (tài liệu dự án BnK) | Phạm vi hợp đồng đầy đủ: J1 + J2, 8 loại chứng từ, console F42–F67, chi tiết hồ sơ 8 tab, mobile 13 màn, core thật | Nguồn nghiệp vụ. **Không đem đi thi.** |
| **Prototype v4/v5/v6** (file HTML) | Mockup click được, không backend | **Spec** và video dự phòng. Ban tổ chức không nhận. |
| **"MVP version"** ban tổ chức yêu cầu | Bản chạy thật, nhỏ nhất, trên ODC + Agent Workbench, chứng minh được use case | **Đây là bài nộp.** |

## Định nghĩa bài nộp

> Một hồ sơ bồi thường sức khoẻ đi trọn vòng: khách upload ảnh chứng từ → BnK IDP đọc thật → agent trên Agent Workbench diễn giải và quyết định → chuỗi tất định tính ra số tiền → cán bộ thấy kết quả kèm lý do → có nhật ký ai làm gì.

**Trong phạm vi:**

- 1 loại điều trị (nội trú), 3–4 loại chứng từ đã có template IDP
- 3 kịch bản: **B** sạch tự duyệt · **A** có khuyến nghị lâm sàng cần người xác nhận · **D** từ chối vì thời gian chờ
- 5 agent: Orchestrator, Intake, Adjudication, Clinical, Customer-communication
- IDP thật qua REST; hợp đồng seed vào Entity theo bản chụp P-02
- Console: hàng đợi + chi tiết hồ sơ + nhật ký AgentRun

**Ngoài phạm vi, cố ý:** J1, mobile native, 8 loại chứng từ, sandbox InsureMO, portal khách hàng đầy đủ, song ngữ, reissue tài liệu BA.

## MVP của riêng ngày 8/9 (3 giờ tại chỗ)

Nhỏ hơn nhiều, vì 3 giờ không dựng nổi cái trên:

**Sàn (không được thủng):** một luồng hồ sơ B · một agent (AG-5) · một Server Action · một con số đúng · một bản ghi `AgentRun`.

**Mục tiêu (từ 04/09, sau khi biết MCP tạo được agent):** **hai agent nói chuyện được với nhau** — AG-0 điều phối AG-5 — và có nhật ký trao tay đọc được.

AG-5 giờ dựng được ở nhà tối 6/9 bằng `app_create` kind `AIAgent`, nên 3 giờ tại hội trường không còn tiêu vào việc dựng agent đầu tiên. Dùng nó cho thứ đắt hơn: kiểm chứng **callable-action list của một agent có nhận agent khác không**, và hỏi SA trực tiếp câu đó.

## Đường cắt khi trễ

Cắt theo thứ tự, cắt sớm còn hơn cắt muộn:

1. Bỏ hồ sơ A, C, D — chỉ giữ B
2. Bỏ màn hàng đợi — vào thẳng chi tiết bằng URL
3. Bỏ AG-0 — chỉ AG-5 chạy được cũng đủ chứng minh Agent Workbench (nhưng mất điểm agent-to-agent)
4. Bỏ upload thật — chạy ReplayMode với JSON đã ghi

**Sàn không được thủng:** một agent trên Agent Workbench, gọi một Server Action, ra một con số đúng, có bản ghi `AgentRun`.

## Kiến trúc 6 agent (mục tiêu 7–8/10)

| Agent | Vai trò | Tool |
|---|---|---|
| **Orchestrator (AG-0)** | Điều phối, chờ hai nhánh ngã ngũ, gác cổng 4 điều kiện, ghi audit | Server Action trong CLM_Core |
| **Intake (AG-3)** | Nhận file, gọi IDP, checklist theo `templateId`, yêu cầu bổ sung, `ExtractionError` | `IdpSubmitSync`, `IdpGetRawFile` |
| **Adjudication (AG-5)** | Gọi chuỗi 14 bước như tool, diễn giải từng bước bằng tiếng Việt — **không tự tính** | `RunAdjudicationChain` |
| **Clinical (AG-4)** | Nhận cảnh báo flow 28/8 + lines, tra KB, trả NOTREL/NOTCOV + evidence + band | `IdpStartAssessment`, `IdpGetAssessment`, KB |
| **Fraud-signal (AG-6)** | Tra bệnh viện OpenSearch, trùng hồ sơ, điều kiện 4 của cổng | `IdpSearchHospital`, Entity Claim |
| **Customer-communication (AG-7)** | Thư kết quả/từ chối tiếng Việt theo mẫu, không nói "kê sai" | Template thư |

Clinical agent tồn tại vì một lý do cụ thể: IDP **không** trả `proposedReasonCode`, `evidence[]` hay band theo dòng chi phí (khoảng trống H7–H9 trong bảng đối chiếu). Đó chính là chỗ agent lấp — và là lý lẽ mạnh để xây trên Agent Workbench thay vì chờ IDP.
