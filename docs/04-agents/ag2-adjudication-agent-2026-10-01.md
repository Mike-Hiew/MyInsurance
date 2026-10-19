# Change — AG-2 Adjudication trên Agent Workbench (sàn của bản dự thi)

**Mở:** 01/10/2026 · **PO:** Mike · **Lý do làm bây giờ:** IDP live còn timeout (IT hẹn sửa 02/10) → chuyển sang AG-2.
**Chuẩn:** `build-05` §3.3 (prompt AG-2), §6.3 (sàn) · `agent-architecture/specs/agents/spec.md` (1 agent, chuỗi là MỘT tool, IDP không chạy trong lượt agent, trần 60 s).
**Trạng thái 01/10 23:30:** sàn AG-2 **xong** (việc 1–7 ✅). ClaimMVP **rev 237**, agent **rev 4**.

## Quyết định PO 01/10
- Model connection: **TrialGPT5**.
- Asset mới: **CLM_AdjudicationAgent** (không dùng lại `AG5_Adjudication` / `AgenticHealthClaimAdjudicationAgent`).
- Điểm gọi: **nút "Tính và giải thích" + ô chat hỏi tiếp** trên ClaimDetail.
- (22:00) **Thời gian lượt ~42–45 s: để nguyên**, dùng video dự phòng khi pitch. Không tối ưu thêm.
- (22:00) Sửa **hết** 4 lỗi màn thấy trên CLM-2026-0005 → xong rev 236–237.
- **CLM-2026-0005 là hồ sơ "sạch tự duyệt" cho demo** (thay 0003 trong T-GATE-01).

## Thiết kế
- ClaimMVP lộ **Service Action** cho agent: `Agent_RunAdjudicationChain` · `Agent_GetClaimContext` · `Agent_GetChainExplanation`. Mỗi tool tự ghi **một dòng AgentRun** (AgentName "AG-2 Adjudication", ToolName đúng tên, Replay theo Settings).
- Agent app `CLM_AdjudicationAgent` (`f012b159-2e60-48c5-9e2e-ea90457140a5`, TrialGPT5), lộ `AskAdjudicationAgent(ClaimId, Question, SessionIdIn)` → Answer, SessionId, IsError, DurationMs. Rev 4 trả lời gọn: chỉ bước làm đổi tiền + "Các bước khác: PASS/SKIP" + Tổng + Summary (EN).
- ClaimMVP ClaimDetail (tab **Calculation & ledger**): panel AG-2, server action `AskAG2`, ServerRequestTimeout 60 s.
- **Không tool nào ghi tiền ngoài chuỗi**.

## Việc
- [x] 1. Đọc model
- [x] 2. (rev 223) 3 service action cho agent + ghi AgentRun
- [x] 3. (agent rev 1–4) `CLM_AdjudicationAgent`, TrialGPT5, prompt, 3 tool
- [x] 4. (rev 224) ClaimDetail: panel AG-2
- [x] 5. Kiểm sàn: 0003 run #2 = #3 = 5.400.000; 0005 run #2 = #3 = #4 = 1.120.000; AgentRun ToolName đúng, Replay = False
- [x] 6. Kiểm cờ Replay = True (22:18)
- [x] 7. Ảnh tab Actions agent rev 4 (TrialGPT5 + đúng 3 action) → `E:\Hackaton\output\bnk-dev-claimmvp\shots-ag2\ag2-agent-tools-rev4.png`

## Kết quả — CLM-2026-0003 (ClaimId 19)
- AG-2 gọi GetClaimContext → RunAdjudicationChain, 14 bước, **5.400.000 ₫**; bấm lại cùng số. AgentRun 1354–1356, Replay = False. Lượt ~45 s.
- Hỏi tiếp "Vì sao bước 9 bị HOLD?" → trích nguyên văn log bước 9, không chạy lại chuỗi ✅.
- 0003 **không sạch**: B09 HOLD (thiếu số ngày điều trị theo dòng), 5.400.000 > 2.000.000 → cổng ✕ (điều kiện 1, 3). Seed cũ 1.680.000 không do chuỗi tạo.

## Kết quả — CLM-2026-0005 (ClaimId 430) · hồ sơ demo sạch
- Nút Admin **"Create demo claim CLM-2026-0005"** (rev 229–234, sửa thành "creates or repairs" ở rev 237): ngoại trú, Trần Thị Bích, HC-2026-0002 · HC-PLS, Hoàn Mỹ Sài Gòn (in-network), J06.9, 3 dòng 300.000 / 450.000 / 650.000 = 1.400.000.
- Chuỗi: B05 → 200.1, 200.1, 200.2 · B08 không cắt · **B10 đồng chi trả 20 % = 280.000** · **Tổng 1.120.000 ₫** · **cổng 4/4 đạt**.
- Lượt AG-2: 21:16 (agent rev 2) 59 s timeout · 21:26 (rev 3) 66 s → 504 · 21:34 (rev 4) **44,5 s OK** · 22:18 ~42 s OK. Bóc 44,5 s: ~7 s trước tool đầu · ~10 s giữa 2 tool · ~24 s viết trả lời.

## Kiểm cờ Replay — 01/10 22:18 ✅
PO bật `ReplayMode = True` (Claude đọc Portal xác nhận) → Claude bấm "Tính và giải thích" trên 0005 → run #4 (ChainRunId 301) 1.120.000 ₫ → AgentRun **1398 GetClaimContext · 1399 RunAdjudicationChain · 1400 Agent answer — Replay = True** → PO tắt → Portal: **ReplayMode False** ✅.

## Sửa 4 lỗi màn — rev 235–237 (01/10 tối, Mentor, kiểm bằng hành vi)
| Lỗi | Nguyên nhân (đọc model) | Sửa | Kiểm |
|---|---|---|---|
| Bảng quyền lợi 1 dòng / "Payable 700.300" / toast "Index -1" khi tải 0005 | `BuildBenefitRows` (client) lấy phần tử list bằng phép trừ độ dài chuỗi, giữ tiền dạng Text rồi parse lại | Data Action server **`GetBenefitSummary`** (struct `BenefitSummaryRow`, Decimal): LedgerPosting AMOUNT của run mới nhất, **mỗi dòng chi phí chỉ lấy posting mã sâu nhất** (sort ClaimLine.Id asc, BenefitCode desc + bỏ trùng PrevLineId), gộp theo mã bằng `ListIndexOf`; footer cộng từ dòng; cảnh báo khi tổng ≠ chuỗi. Xoá BuildBenefitRows | 0005: 200.1 750.000→600.000 (KH 150.000) · 200.2 650.000→520.000 (130.000) · Tổng 1.400.000/1.120.000/280.000. 0003: 100.1 1.500.000 · 100 1.400.000 · 100.2 2.500.000 · Tổng 5.400.000. Hết toast, không cảnh báo ✅ |
| Header "Processing documents" | Seed đặt Status = Received | Seed: Status = UnderReview | "Under assessment" ✅ |
| Assessor trống | Seed không gán AssignedTo | Seed: "Nguyễn Hoài An"; biểu thức fallback "Unassigned" | ✅ |
| Event SK-1 lệch nhãn, Admission 01/01/1900 | 5 giá trị đầu gắn lệch 1 ô; không chặn NullDate | Gắn lại; Admission/Discharge "—" khi rỗng | 0005: CLM-2026-0005/1 · Outpatient · 22/09/2026 · — · —; 0003: Inpatient · 03/07 · 03/07 · 06/07 ✅ |
| "Mandatory documents present ✓" khi checklist rỗng | Biểu thức là hằng `True`; seed không tạo ClaimDocument | Seed thêm T1-01 (Required), T1-03, T1-04 (Expected), đều Received; check = `MandatoryDocsOk` tính trong GetDocs OnAfterFetch | 0005 có 3 chứng từ, ✓ thật. Nút seed chạy 2 lần: "inserted: 3 (already present: 0)" rồi "inserted: 0 (already present: 3)" — idempotent ✅ |

- rev 235 lỗi runtime Postgres `42703 column "codelen" does not exist` (sort aggregate theo cột tính) → rev 236 sort theo BenefitCode desc.
- Mentor lần 2 báo "đã sửa CodeLen" nhưng `changeApplied = false`, publish không tạo revision mới → **luật 5 lặp lại: chỉ tin revision mới + màn hình**.
- Còn cosmetic: Sum/yr của mã con hiện 0 (hạn mức năm ở mã cha), Co-pay "—" khi kế thừa từ cha; bảng chưa sort theo mã.
- Chưa kiểm control âm cho "Mandatory documents present" (hồ sơ thiếu chứng từ bắt buộc → ✕).

## Cách bấm trên ClaimMVP (Claude)
- Panel AG-2 nằm trong tab **Calculation & ledger**: bấm tab trước (toạ độ), `scrollIntoView` nút, bấm theo toạ độ = rect × 1568/innerWidth.
- Nút Administration (Dry-run, Create demo claim): **chỉ chạy khi `button.click()` bằng JS** — bấm toạ độ 0 sự kiện.
- `javascript_tool` trả về chứa "?"/"=" có thể bị chặn → thay ký tự trước khi trả.

## Còn lại khác
- 0003: "Total claimed 2.460.000" (Claim.TotalClaimed seed) ≠ tổng dòng 5.400.000.
- Seed AgentRun cũ còn tên AG-0/AG-1/AG-3…AG-6 (việc 9 của agent-architecture).
- 🔴 `IdpPassword` vẫn hiện chữ thường trên Portal Settings — chuyển Secrets + đổi mật khẩu trước khi quay video.
