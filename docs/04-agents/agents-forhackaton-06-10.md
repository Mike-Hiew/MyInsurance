# Agent → ForHackaton + chỉnh cách trả lời (06/10/2026, phiên Cowork)

## Model
| Agent | Model | Rev |
|---|---|---|
| AG-2 `CLM_AdjudicationAgent` | TrialGPT5 → **ForHackaton** | 5 (đổi model), 6–10 (prompt + ngôn ngữ) |
| Trợ lý khách `CLM_CustomerAssistantAgent` | TrialGPT5 → **ForHackaton** | 4 (đổi model), 5 (prompt) |
| **Trợ lý cán bộ `CLM_StaffAssistantAgent`** (mới, key `685696f2-06ec-4946-9dc1-06c679f7a1ab`) | ForHackaton | 1–3 |
| AG-4 `CLM_FraudSignalAgent` | ForHackaton (đã có từ 04/10) | 6, không đổi |
| AG-3 `CLM_ClinicalAgent` | ForHackaton (ghi chép 04/10, rev 5) | chưa mở lại kiểm |
| AG-5 `CLM_CommsAgent` | ForHackaton theo ghi chép P5b | **chưa xác nhận** — phiên Mentor thứ 5 bị chặn |

ForHackaton = OpenAI gpt-5.4-mini, `reasoning_effort: low`. Mọi lần publish agent đều báo 2 lỗi "Unavailable feature" nhưng publish vẫn thành công (có sẵn từ trước, kể cả agent không sửa).

## AG-2 — cách trình bày + song ngữ
- Mục "Cách trình bày câu trả lời (bắt buộc)": tiền 320.000 ₫ (VI) / 320,000 VND (EN); đổi mã PASS/SKIP/HOLD/CUT/POSTED/DENY sang lời thường; liệt kê MỌI bước HOLD và mọi bước đổi tiền; "Các bước còn lại / Other steps" chỉ cho PASS/SKIP; có HOLD thì thêm "Cán bộ cần làm / Officer action needed". Giữ 2 dòng cố định ClaimMVP đọc: `Action taken: …` (đầu) và `Summary (EN): …` (cuối).
- Rev 6 bỏ sót HOLD 2–3 → sửa rev 7.
- **Ngôn ngữ:** agent không có input Lang (chỉ `ClaimId`, `Question`, `SessionIdIn`). Gốc lỗi luôn ra tiếng Việt: câu vai trò trong `BuildMessages → BuildPrompt` ghi cứng "GIẢI THÍCH kết quả bằng tiếng Việt" (gỡ ở rev 9). Chỉ sửa prompt chưa đủ với gpt-5.4-mini → **rev 10:** node `DetectLanguage` trong BuildMessages: Question có ký tự tiếng Việt có dấu → chèn "[Ngôn ngữ trả lời: tiếng Việt]", không có → chèn "[Reply language: ENGLISH …]" vào đầu user message.
- Kiểm live CLM-2026-0444 (console EN, chuỗi #15, ChainRunId 401): nội dung tiếng Anh đúng. Còn sót: dòng đầu "Action taken: chạy chuỗi lần #15 (ChainRunId …)" (mẫu cố định, có thể ClaimMVP đọc — để nguyên) và tiền tố "Bước 2/3/10/14" ở mỗi dòng.
- Lưu ý: `RunAdjudicationChain` tạo một `ChainRun` mới mỗi lần được gọi (append-only) — vì vậy KHÔNG nối khung chat chung vào AG-2.

## ClaimMVP rev 428 (phiên Cowork, PO đồng ý) — khung AG-2 song ngữ
ClaimDetail, panel AG-2: nút `T("Calculate and explain","Tính và giải thích")`; câu nút gửi `T("Calculate this claim and explain each step.","Hãy tính hồ sơ này và giải thích từng bước.")`; `T("New conversation","Cuộc hội thoại mới")`; `T("Send","Gửi")`; placeholder `T("Ask a follow-up, e.g. why was line 2 reduced?", …)`; mô tả panel. Không đổi logic. Kiểm live EN ✓.

## ClaimMVP rev 429–445 (phiên Cowork, PO đồng ý) — tab Trích xuất & chi phí theo mockup v0.7
- Cột trái `ExtractCols` trước đây trống → thẻ **"Chứng từ gốc — T1-01"** (sticky): tên cơ sở `Claim.FacilityName`, họ tên `Member.FullName`, ngày KCB `Claim.EventDate`, `Claim.IcdCode`, các dòng `GetLines` (tên cắt 26 ký tự, `AmountClaimed`), CỘNG `Claim.TotalClaimed`.
- Header bảng: chip "Đọc bởi BnK IDP" (+ " · x s" chỉ khi `AgentRun.DurationMs` > 0 của lượt `AgentName = "IDP"` mới nhất); pill "Đã xác minh n/m" theo `ClaimLine.IsVerified` hoặc assessment đã acknowledge; ẩn khi m = 0.
- Tô vàng dòng (`v7-scan-row-warn`): độ tin cậy < 0,75 chưa xác minh, hoặc dòng có Recommendation PENDING đúng ClaimLineId.
- Hiệu ứng scan (CSS tiền tố `v7-scan-`): vệt tím quét tờ chứng từ 1,9 s + các dòng (giấy và bảng `table.cc-table`) hiện dần 0,35 s + 0,2 s/dòng, fill-mode backwards; chỉ lần đầu mở tab mỗi hồ sơ; tự tắt ~2,5 s sau khi chạy (rev 434) để tick/sửa dòng không làm bảng "tải lại"; tắt khi prefers-reduced-motion.
- Ô Xác minh (rev 434): ô tick + giờ "dd/MM HH:mm"; "Đã xác minh bởi … · …" ở tooltip. Verify chỉ refresh dòng + số đếm.
- Bảng (rev 435, 437): bỏ cột Ngày và SL → dòng phụ "SL x · dd/MM/yyyy" dưới tên khoản mục; class `.v7-extract-table` (CSS đã thử live trước khi đưa vào): header, "Sửa", giờ không xuống dòng; padding 12px 10px.
- **Nút "Xem chứng từ gốc (IDP)" — bản gốc thật (rev 436, 438–445):** server action `GetOriginalZip(ClaimId)` lấy `ClaimDocument.IdpRequestId`; bỏ qua (coi là hồ sơ mẫu) khi rỗng, không phải số hoặc `IsReplay = True` — kiểm số bằng logic sau aggregate, **không** dùng TextToInteger trong filter (cast SQL lỗi với seed "DOC-DEFAULT"). Có trong `IdpRawFileCache` thì dùng; chưa có thì gọi `IdpRawFileMethod` (`GET /request/{id}/raw-file`, trả zip) một lần rồi lưu cache. Giải nén trong trình duyệt bằng JS thuần (central directory + `DecompressionStream('deflate-raw')`, không cần thư viện, đã test bằng node), hiện ảnh/PDF trong popup; bên dưới có mục thu gọn "▸ Dữ liệu IDP đã đọc". Hồ sơ mẫu → bảng dữ liệu IDP + ghi chú. Popup (rev 443–445): header có nút ×, thân cuộn, footer "Đóng"; CSS `.mc-modal [id$=OriginalBodyPart1]{width:100%;…}` và `> *{flex-shrink:0}` (đã thử live). Esc chưa đóng được.
- Kiểm live: ClaimId=452 (CLM-2026-0452) hiện bảng kê gốc "Bệnh viện Đa khoa Demo OS" ✓; ClaimId=17 (CLM-2026-0001, hồ sơ mẫu) hiện dữ liệu IDP + ghi chú ✓.
- **Dữ liệu mâu thuẫn chưa xử lý (CLM-2026-0001):** thanh Việc cần làm báo "Quyết định đề xuất trên khoản mục 1 (Men vi sinh…) · 1 khuyến nghị chưa quyết định" (Men vi sinh là dòng 6), còn thẻ AG-3 báo "Không có đề xuất chờ xử lý"; tab Hoạt động của khung trợ lý lại hiện đề xuất NOTCOV Men vi sinh đang chờ xác nhận. Dòng 6 chưa được tô vàng.
- Bài học Mentor: kiểm trình duyệt sau mỗi publish; prompt lớn hay treo > 15 phút → huỷ, mở phiên mới, tách nhỏ; Mentor từng ghi chuỗi tiếng Việt dạng `\uXXXX` (OutSystems không giải mã) → phải yêu cầu ký tự thật; thử CSS live bằng JS trước rồi mới đưa Mentor; mapping chữ nên so khớp theo mẫu (Index/StartsWith), không so khớp chính xác.

## ClaimMVP rev 446–452 + agent mới — khung chat "Hỏi trợ lý" (PO chọn "Nối AI chỉ đọc")
- Trước: khung chat nổi (block `LayoutSideMenu`, action `AskSend`, chip `ChipDocsMissing/ChipLimitLeft/ChipCapBind`) **không có AI** — câu trả lời soạn sẵn tiếng Anh, giọng nói với khách ("an assessor will get back…"), chip trả lời sai ("0 required documents missing: cost statement…").
- Agent mới `CLM_StaffAssistantAgent` = nhân bản `CLM_CustomerAssistantAgent` (giữ 2 tool chỉ đọc `Tool_GetClaim`/`Tool_GetLimits` → `ClaimMVP.CustomerAssist_GetClaim/GetLimits`, guard MemberId). Service action `AskStaffAssistant(MemberId, Question, ClaimNo, Lang, SessionIdIn) → Answer, SessionId, IsError, DurationMs`. Prompt cho cán bộ: chỉ đọc, chỉ chỗ thao tác trên màn hình, ≤5 gạch đầu dòng, số cụ thể, không mã kỹ thuật, không markdown, theo Lang. Rev 2: chèn "[Hồ sơ đang mở: ClaimNo]" vào user message + mặc định ClaimNo cho Tool_GetClaim. Rev 3: bỏ markdown, không tự mâu thuẫn.
- ClaimMVP: server action `StaffAssistant_Ask(ClaimId, Question, Lang, SessionIdIn)` (446); `AskSend` gọi agent, bong bóng "Đang đọc hồ sơ…" rồi thay bằng câu trả lời, giữ SessionId theo hồ sơ (447, 449); chip song ngữ gửi câu hỏi thật (448–449); footer + lời chào song ngữ (450); lời chào dùng tên giống chân sidebar (452 — `Client.UserName`, rỗng thì mặc định "Nguyễn Hoài An").
- Kiểm live CLM-2026-0452 (VI): hỏi chứng từ ✓, hỏi hạn mức ✓, tóm tắt ✓.

## ClaimMVP rev 453–457 — bố cục + nút trợ lý theo mockup + tab Hoạt động dễ đọc
- 453 (CSS, đã thử live): khung `.cc-console-chatbox` từ top 76px tới bottom 16px (trước đây đầu khung bị thanh trên che); danh sách chat `[id$=CCChatList]` và tab Hoạt động `.cc-ab-activity` bỏ max-height cố định (240px / 380px), giãn hết chỗ trống; ô nhập giãn ngang; chip gọn; tự cuộn xuống câu mới.
- 454: nút nổi giống mockup v0.7 — icon robot riêng của mockup (SVG data URI, thay `ph-robot`), nền gradient #12454a→#0c3036; nhãn "Hỏi trợ lý / Ask the assistant" (class `v7-fablbl`, chấm xanh mint) cạnh nút khi khung đóng; icon robot ở tiêu đề khung. Kiểm live ✓.
- 455–457: tab Hoạt động — tiêu đề mục, "Cần xác nhận", "Xác nhận/Bác bỏ", "xong/lỗi" song ngữ; NOTCOV/NOTREL/MEDIUM… đổi thành lời; hàm `StepLabel` (17 mẫu) đổi tên bước kỹ thuật sang lời thường (vd "CLAIM_SUBMITTED received" → "Khách gửi hồ sơ", "Call IDP POST /request/sync" → "Gửi chứng từ sang BnK IDP"); tên tác nhân theo ngôn ngữ (Điều phối, Chuỗi 14 bước, Dò trùng lặp…); ẩn dòng "Tool call". Rev 457 (so khớp theo mẫu cho "14-step chain", "duplicate/fraud signals", "NOTREL recommendation", "EXCEPTION", tác nhân "Idp…") — **chưa kiểm live** vì máy người dùng mất kết nối lúc publish.
- Lỗi "bấm nút trợ lý lần đầu không mở": bấm bằng code thì mở ngay lần đầu; khi bấm bằng chuột qua tiện ích Chrome thì sự kiện không tới trang → nhiều khả năng do công cụ kiểm thử, không phải lỗi app.

## Trợ lý khách
Mục "Cách trả lời khách hàng (bắt buộc)": "Quý khách", 2–4 câu, không lặp ý; đủ trạng thái + bước tiếp theo + khách có cần làm gì; không hứa thời gian; tiền 320.000 ₫; không in mã kỹ thuật; trả lời theo ngôn ngữ khách. Prompt vượt 2.000 ký tự/setting → tách sang setting `CustomerAssistant_SystemPrompt2`. Kiểm live ✓ (VI).

## Ghi chú dữ liệu
Mỗi lần bấm "Tính và giải thích" sinh một lần chạy chuỗi tạm: CLM-2026-0444 có thêm #11–#15 (ChainRunId 397–401) trong ngày 06/10.
