# openspec — Agentic Health Claim (BnK · hackathon 07–08/10/2026)

Quy ước thư mục theo **spec-driven development** (OpenSpec), rút gọn cho một người trong 9 ngày.

## Ba lớp

| Thư mục | Là gì | Ai sửa |
|---|---|---|
| `specs/<capability>/spec.md` | **Nguồn chân lý ổn định** — hệ thống *là* gì | Chỉ sửa khi một change được archive |
| `changes/active/<name>/` | **Delta đang chạy** — cái đang *đổi*, chưa vào spec | Sửa tự do khi đang làm |
| `changes/archive/<name>/` | Change đã xong, giữ làm hồ sơ | Không sửa |

Luật duy nhất: **một thay đổi về hành vi hệ thống phải tồn tại ở `changes/active/` trước khi nó tồn tại trong code.**
Chỗ đau đã gặp: "cổng tự duyệt 4 điều kiện" sống song song với một câu "chuẩn là 3" trong `build-00` suốt ~90 rev mà không nằm ở đâu cả. Phân xử 21/09: **code đúng, tài liệu sai** — và điều đó chỉ lộ ra khi có người đặt hai bên cạnh nhau.

## Khác OpenSpec gốc ở ba chỗ, có chủ ý

1. **Không có `proposal.md` / `design.md`.** Một người, mọi quyết định kiến trúc đã nằm trong `build-00` §5. Chỉ giữ `tasks.md`.
2. **Không có `schemas/`** cho tới sau pitch.
3. **Verify không bằng git diff.** Code là một app ODC trên cloud do Mentor sửa — không có repo, không có PR. Thứ thay thế là **runtime audit + screenshot** trong `../output/`. Đừng bỏ cái đó để chạy theo quy trình git.

## specs/ — nội dung đầy đủ nằm ở đâu

Văn bản đầy đủ của 7 spec hiện nằm trong **Project "Hackaton" trên claude.ai** (`claude/build-00…06`). Mỗi `specs/<cap>/spec.md` dưới đây là **bản tóm ràng buộc bất biến + con trỏ**, không phải bản sao.

| `specs/` | Doc gốc trong Project | Nội dung |
|---|---|---|
| `data-model/` | `build-01-data-model-va-enum` | 32 entity · 31 enum · 30 ràng buộc |
| `idp-integration/` | `build-02-tich-hop-idp` | 32 endpoint · 22 templateId |
| `claim-rules/` | `build-03-nghiep-vu-va-rule` | 8 loại chứng từ · 88 field · R1–R9 · ~45 BR-* |
| `adjudication/` | `build-04-chuoi-tinh-tien-va-seed` | 14 bước · 7 mã từ chối · seed · 65 unit test |
| `agents/` | `build-05-agent-va-system-prompt` | 6 agent · 39 tool binding |
| `screens-wording/` | `build-06-man-hinh-va-wording` | màn hình · 53 message nguyên văn |
| *(phạm vi & phân xử)* | `build-00-chuan-va-pham-vi` | chuẩn gốc, 10 phát hiện, 8 xung đột đã phân xử |
| *(tiến độ)* | `trang-thai-build-2026-09-20` | app nào có gì, còn thiếu gì |

**Nguồn cấp trên của các spec đó nằm trên đĩa** tại `../Preparing/04_Nghiep-vu-va-du-lieu/` — A1 Glossary, A2 Message Catalogue, A3 Permission Matrix, A4 Document Catalogue, A5 Product Data Sheet, D1 BRD, D2 Functional Spec, D3 Integration Spec. Khi spec và BRD lệch nhau, **A5/D2 thắng** (`build-00` §5).

## Vòng đời một change

1. `mkdir changes/active/<ten-ngan-gon>/` → viết `tasks.md`
2. Nếu change làm đổi một spec: thêm `specs/<cap>/spec.md` chứa **đúng đoạn khác biệt**, không chép cả spec
3. Làm việc, tick task
4. Xong → merge đoạn delta vào Project doc tương ứng → `mv changes/active/<x> changes/archive/`

## Trạng thái hôm nay — 20/09/2026

| Change | Trạng thái | Chặn gì |
|---|---|---|
| `verify-entity-vs-build01` | 🟡 đợt A đóng (rev 132/50) · **đợt B/B2/B3 đóng 21/09** — ClaimMVP **rev 139** (B4: F-4 + C-1 21/09) + MyInsurance **rev 51**: readout `51 · 12 · 17/17/17 · 0 · 0 · 0 · 0 · 0`, 5 màn blast-radius ✅, hồ sơ 0001 = hero ngoại trú 6 dòng, kịch bản B khớp A5 từng đồng (lối C, NOTREL) · phát hiện F-1…F-4 + C-1 ghi ở `runtime-check-batchB.md` · còn task 12 | **Tiếp theo: `chain-14-steps-a5`** — 14 Server Action theo thứ tự A5, 65 test. Không gì khác · 2 câu hỏi nợ (IDP Q-IDP-01, SA agent↔agent) soạn sẵn ở `questions-pending.md` — PO gửi |
| `idp-integration-live` | 🔴 chưa bắt đầu — **đường găng** | Tenant **177**, bộ **198**, template **6090–6098**. Flow đã có mail ghim đến 08/10; **quota chưa đóng** |
| `approval-gate-4-conditions` | 🟡 PO chốt **4** (21/09) | Còn mở: *4 điều kiện NÀO* — màn hình và doc liệt hai bộ cùng đếm ra 4 |
| `chain-14-steps-a5` | 🟢 21/09 — **14 bước A5 chạy thật trên ClaimMVP rev 162**: `A5Step01…14`, `RunAdjudicationChain(ClaimId, TriggeredBy)` (ChainRun RunNo/InputsHash/DurationMs/StepResults, `LedgerPosting` PROVISIONAL AMOUNT/OCCURRENCE/**DAY**, CalcSnapshot 14 dòng, chỉ bước 14 ghi tiền), `A5Ledger_Finalize/Reverse` (chưa nối approve); `ClaimLine.TreatmentDays` = A4 L18 (bước 9 DAY, bước 14 post DAY lên 100.1.1 + 100.1); hồ sơ 0001 = **2.000.000** (trace 719 ms; ServerRequestTimeout 60 s) · bộ test 71+1 (`ChainTests`, Mode DRYRUN/CONFIRM_RERUN/RUNTWICE/CONCURRENT/RUN): **46/46 pass** (run #11) — T-CHAIN-05 RERUN 2.000.000 → **1.880.000** (máy: +18.000 cap 200.2, +30.000 copay), T-CHAIN-06 MOVED 640.000 → 360.000 (BR-LEDGER-8, Finalize qua runner), T-CHAIN-07 POSTED 3.000.000 "2 DAY (100.1.1 +1, 100.1 +1)", T-CH12-d W-8 PASS · seed fixture cleanup thật từ rev 162 ("Đã xoá 185, hiện có 42") · nhật ký `current-chain.md` §5–8, kết quả `test-run.md` | **Còn 26 test:** T-CH14 (6) · T-SEED (11) · T-GATE BLOCKED (3/4 điều kiện) · T-CH11-c/d MANUAL · **khoảng trống:** T-CHAIN-06 phải chạy lại qua Approve khi Finalize nối gate · F48: card tĩnh "AFTER CONFIRMATION 0 ₫", gate card đọc run cũ, panel ledger Left 0 (F-CH-1…3, batch F) · `Seed_AssessmentAndTerms` — PO sửa tay Studio · log bước 11 "(không có)" → batch F · xoá fixture TST-/FX- trước demo (giờ 1 click) · 2 câu hỏi nợ chưa gửi |
| `ui-100pct-drive` | 🟡 code xong (ClaimMVP rev 128 · MyInsurance rev 46, audit 100 %), **ghi hồi tố** · còn: merge delta vào `build-01`/`build-06` rồi archive | Không chặn gì |

Đã xong và nằm trong `changes/archive/`: toàn bộ pass UI (console rev 30–91, portal rev 1–9). Pass "100 %" (console 96–119, portal 15–38) đang ở `changes/active/ui-100pct-drive/` chờ archive.
