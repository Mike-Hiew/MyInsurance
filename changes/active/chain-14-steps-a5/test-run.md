# test-run.md — kết quả chạy bộ 71 (build-04 §9) trên runtime

Nguồn kỳ vọng: `expected-71.md` nguyên văn. Cách chạy: màn `ChainTests` → Re-seed 71 rows → Seed fixtures → Run runnable tests (Server Action `RunChainTests`, gọi `A5Chain_DryRun` — không ghi tiền).

## Run #1 — rev 145, 21/09/2026 ~16:10 (steps 1–9)

**26/29 pass · 3 fail · 42 chưa chạy** (PENDING/MANUAL, chờ bước 10–14).

| Test | Kết quả | Actual | Ghi chú |
|---|---|---|---|
| T-CH01-a/b/c | ✓✓✓ | PASS 300.000 · FAIL 0 · FAIL 0 | |
| T-CH02-a/b/c/d/e | ✓✓✓✓✓ | PASS · FAIL · FAIL · FAIL · HOLD 1.000.000 | BR-WAIT-5 ba mảnh (90 ngày · 01/04/2026 · 20/06/2026) đều có; BR-WAIT-4 HOLD |
| T-CH03-a | ✓ | PASS 300.000 | |
| T-CH04-a/b/c/d | ✓✓✓✓ | HOLD 400.000 · HOLD 800.000 · PASS 500.000 · HOLD | 04-d: log "10 điều khoản trong sổ · 6 ACTIVE đã kiểm · 5 không thuộc phạm vi · 1 checkpoint · 0 EXCL" ⇒ n = 10, m = 6, 5+1+0 = 6 |
| **T-CH04-e** | **✗** | HOLD 400.000 | Log chỉ có TERM-HC-003 checkpoint; **TERM-HC-001 báo KHÔNG_THUỘC_PHẠM_VI** dù fixture IcdCode = Z41.1 ⇒ predicate #1 (`diagnosisCode startsWith Z41`) hoặc `Ctx.DiagnosisCode` hoặc fixture IcdCode sai — điều tra lượt K |
| T-CH05-a/b/c/d | ✓✓✓✓ | PASS 300.000 · PASS 300.000 · PASS 500.000 · PASS 200.000 | NONE/ASSESSOR không decline |
| **T-CH05-e** | **✗** | PASS 1.000.000 | Outcome/amount đúng; log thiếu mảnh "→ 500" và/hoặc "tai nạn" — xem log thật ở lượt K |
| T-CH06-a/b/c | ✓✓✓ | CUT 0 · CUT 300.000 · CUT 1.200.000 | NOTCOV · NOTREL (Recommendation Confirmed) · covered = 3 dòng còn lại |
| T-CH07-a | ✓ | SKIP 300.000 | |
| T-CH08-a/b/c | ✓✓✓ | CUT 2.400.000 · CUT 6.000.000 · CUT 1.500.000 | cắt 200.000 · trần ×12 · một visit cắt 300.000 |
| **T-CH09-a** | **✗** | CUT 1.000.000 | Số và outcome đúng (2 lượt trong hạn = 1.000.000). Log "200.3: 5 lượt mới, đã dùng 18/20 — từ chối 3 dòng" **không có chữ "vượt hạn mức"** — sửa wording bước 9 (lượt K) |
| T-CH09-b/c | ✓✓ | PASS 4.000.000 · HOLD 1.000.000 | không giới hạn lượt · Q-IDP-06 |

### Lỗi harness thấy trong run #1 (sửa lượt K)
- `ChainTest_Assert` Reason in "log thiếu fragment; " không nêu tên mảnh thiếu.
- Header: "0 chờ bước · 0 ngoài chuỗi" — phải là 42 (PENDING + MANUAL); "Chưa chạy" vẫn hiện sau khi chạy (max LastRunAt không refresh).
- Nút "Re-run demo seed" (Administration) bấm lúc 16:02 không thấy log `RunSeedBatch0Now done` trong app_logs — bảng ChainTest rỗng sau đó; "Re-seed 71 rows" trên màn ChainTests chạy đúng (71 dòng).

## Run #2 — rev 146 (lượt K), 21/09/2026 16:04

**28/29 pass · 1 fail · 35 chờ bước · 7 ngoài chuỗi** (header đúng sau khi bỏ MaxRecords = 1).

- T-CH04-e ✓ — sau khi sửa `Substr` 0-based: TERM-HC-001 checkpoint (TBC-01) + TERM-HC-003 (TBC-03) cùng trên dòng SUPPLEMENT Z41.1. **Test này bắt được lỗi engine thật** (predicate #1 và #7 chưa bao giờ khớp ở rev 144–145).
- T-CH09-a ✓ — log "3 lượt vượt hạn mức, từ chối 3 dòng (LIMIT)".
- **T-CH05-e ✗** — Reason: `log thiếu «→ 500»; log thiếu «tai nạn»`; log bước 5 thật = **"1 dòng → 100.1"** ⇒ accident redirect **không bắn** (không phải wording). Nghi: fixture `LossTypeId` của TST-T-CH05-e không phải AccidentInpatient, hoặc `Ctx.LossTypeValue` không được nạp (`LossType.Value` bắt đầu "ACC"), hoặc điều kiện redirect trong Step05 so sai. Chẩn đoán lượt K2 (sau J).

## Run #3 — rev 147 (lượt J/J-b), 21/09/2026 16:14 — **BLOCKED**

`RunChainTests` → `A5Chain_DryRun` → `A5Step05_BenefitMapping`: **`OS-BERT-60202 Index 1 is out of range`** (OSList.get_Item, traceId `95ea97ec…`). Lượt J-b sửa điều kiện redirect (`Substr(LossTypeValue, 0, 3)`) và dòng log redirect — một trong hai đụng chỉ số list. Toàn bộ bộ test bị chặn ở bước 5 (mọi hồ sơ). Sửa ở lượt K3 ngay sau I6 (Mentor một run/phiên).

## Run #3 — rev 148 (K3'' + I6-A), 21/09/2026 16:31

**29/29 pass · 0 fail** (35 chờ bước · 7 ngoài chuỗi). T-CH05-e ✓ sau khi (a) sửa `Substr(LossTypeValue, 0, 3)` (J-b) và (b) viết lại vòng lặp Step05 trên `Lines.Current` (K3'' — nguyên nhân `OS-BERT-60202`: hai ForEach cùng dùng counter `I`, nhánh redirect đọc `Lines[I]` sau `I + 1`).

Mốc PO "29 test bước 1–9 chạy được ngay": **đạt 29/29**.

## Run #4 — rev 149 (I6-B: 38 RUNNABLE), 21/09/2026 16:35

**37/38 pass · 1 fail · 24 chờ bước · 9 ngoài chuỗi.**

| Test | Kết quả | Actual | Ghi chú |
|---|---|---|---|
| T-CH10-a | ✓ | CUT 1.920.000 | copay 20 % trên 2.400.000 (base sau bước 9) |
| T-CH10-b | ✓ | CUT 2.160.000 | HC-PRM 10 %, log không có "20%" |
| **T-CH10-c** | **✗** | CUT 350.000 | Số đúng (30 % × 500.000 = 150.000); log có "30%" nhưng **thiếu "từ 300"** — log copay không nêu nguồn thừa kế → sửa wording bước 10 |
| T-CH11-a | ✓ | SKIP · 0 | KHÔNG_ÁP_DỤNG (ResultAmount 0 — sửa I6-C) |
| T-CH11-b | ✓ | PASS 1.920.000 | ngoài mạng, log "quy tắc tạm B-19" ✅ (mục 3 báo cáo PO) |
| T-CH12-a | ✓ | CUT 1.000.000 | ledger 200 = 19.000.000 → còn 1.000.000, cắt 920.000, log nêu benefit + còn + cắt (BR-TREE-5) |
| T-CH12-b | ✓ | CUT 1.000.000 | 4 × 400.000 → copay → 1.280.000 → cắt 280.000 ở dòng cuối |
| T-CH12-c | ✓ (PARTIAL) | **CUT · 0** | Log "100: dùng 0/0 — còn 0 — cắt 2000000" ⇒ **lỗi thật**: nút 100 đọc AnnualLimit = 0 (HC-STD 100 = 100.000.000) → cắt sạch. Assertion PARTIAL quá lỏng — sau I6-C đặt AssertAmount = 2.000.000, log "trong hạn" |
| T-CH13-a | ✓ | SKIP · 0 | ResultAmount 0 — sửa I6-C |

## Run #5 — rev 150 (I6-C), 16:40 — 37/38
T-CH11-a nay SKIP · 240.000 (skip path đúng). T-CH12-c vẫn CUT · 0, T-CH13-a = 0 ⇒ xác nhận lỗi pass 2 bước 12: `AnnualLimit = ledger.Remaining + ledger.Used`, `Remaining = ledger.RemainingAmount` ⇒ không có dòng ledger ⇒ 0 ⇒ cắt sạch. **Hồ sơ 0001 ra 2.000.000 chỉ vì có ledger seed** — bộ test bắt được điều hand-run không thấy.

## Run #6 — rev 151 (I6-D/E), 21/09/2026 16:44

**38/38 pass · 0 fail** (24 chờ bước · 9 ngoài chuỗi).
- T-CH10-c ✓ — log "dòng 1 300.1 copay 30% (từ 300) = 150000".
- T-CH11-a ✓ SKIP = 240.000 · T-CH13-a ✓ SKIP = 240.000 (assertion siết thêm số tiền).
- T-CH12-c ✓ PASS = 2.000.000, log "100: 2000000 so hạn mức còn 100000000 (đã dùng 0/100000000) — trong hạn" (nút hạn mức năm = 100, đi cây theo CapModeId; AnnualLimit từ snapshot, không từ ledger).

Còn lại 33: T-CH14-a…f (6) · T-CHAIN-01…07 (7) · T-GATE-01…07 (7) · T-SEED-01…11 (11) · T-CH11-c/d MANUAL (2).

---

## Báo cáo sau bước 14 (PO yêu cầu 21/09) — trạng thái rev 155

**(1) Hồ sơ 0001 sau bước 10 = 2.000.000 ₫** — dry-run rev 148 (B10 CUT 2.000.000: copay 500.000 = 300.000 trên 200.1 + 200.000 trên 200.2, base = 2.500.000 sau bước 9) và chạy thật rev 154 (`RunAdjudicationChain(17)` → ChainRun 104 & 105, TotalPayable 2.000.000, `14:POSTED`). F48 rev 155 hiện B10 = 2.000.000 ₫ với log "dòng 1 200.1 copay 20% (từ 200) = 60000; …".

**(2) Thời gian chạy `RunAdjudicationChain`** — trace ODC run 104 (lạnh, rev 154): **719,22 ms** server-side, 370 span, 0 lỗi (`traceId 993e6406…`). Round-trip trình duyệt của screen action: 811 ms (lạnh) / **279 ms** (ấm, run 105). `ChainRun.DurationMs` ghi 0 vì `DiffSeconds` chỉ có độ phân giải giây — cần đọc trace để có ms.

**(3) Bước 11 KHÔNG_ÁP_DỤNG + ghi chú "quy tắc tạm B-19"** — T-CH11-b (ngoài mạng, PASS 1.920.000) log: *"Điều kiện PROVIDER_OFF_NETWORK đúng; benefit (không có) không mang deduction_pct — quy tắc tạm B-19 (DeductionScope = LINE_OWN_BENEFIT): không áp. (Đọc theo tổ tiên sẽ là 20% trên 200/300 — chưa chốt B-19.)"* Trong mạng (hồ sơ 0001, T-CH11-a): *"KHÔNG_ÁP_DỤNG — cơ sở trong mạng lưới, không điều kiện khấu trừ nào đúng"*. Lỗi cosmetic: "benefit (không có)" lẽ ra liệt kê 200.1/200.2 — sửa lượt sau.

**(4) Pass/fail** — 29/29 test bước 1–9 (run #3, rev 148) → **38/38** sau khi lật 9 test bước 10–13 (run #6, rev 151; giữ nguyên ở rev 152). Còn 33: T-CH14-a…f (6) · T-CHAIN-01…07 (7) · T-GATE-01…07 (7, change khác) · T-SEED-01…11 (11, assertion đếm bảng) · T-CH11-c/d MANUAL (2).

**Lỗi thật bộ test bắt được (không thấy bằng hand-run):** `Substr` 0-based ×3 (predicate Z41/F1, redirect ACC) · Step05 counter `Lines[I]` sau `I+1` (OS-BERT-60202) · **AnnualLimit đọc từ LimitLedger thay vì PolicyBenefitSnapshot ⇒ mọi hồ sơ không có dòng ledger bị cắt về 0** (hồ sơ 0001 "đúng" chỉ vì có ledger seed) · counters màn ChainTests MaxRecords = 1 · F48 ghim RunNo 1.

**Việc còn lại:** `Seed_AssessmentAndTerms` bỏ ChainRun/CalcSnapshot (Mentor hết bộ nhớ, 3/… node đã xoá — sửa tay Studio hoặc tách) · K5 (B14 trên F48, card run, tổng settlement) · lật T-CH14/T-CHAIN sang RUNNABLE (runner cần đọc LedgerPosting/ChainRun) · T-SEED assertions · A5Ledger_Finalize/Reverse chưa nối Approve/Decline (change `approval-gate-4-conditions`) · xoá fixture TST-/FX- trước demo.

## Run #7 (rev 152, 16:51) và Run #8 (rev 156, 17:21) — **38/38** cả hai: bước 14, wrapper, K4/K5 không ảnh hưởng đường dry-run.

## Run #9 — rev 157 (L1/L2), 21/09/2026 21:38 — **44/44** (2,47 s round-trip, gồm 6 lần `RunAdjudicationChain` thật)

Đọc đầy đủ trên rev 158 (L3: grid 50 → 200 dòng; 72 dòng hiện đủ; header "Mốc: 71/71 (+1 bổ sung PO)").

| Test | Mode | Kỳ vọng (expected-71 nguyên văn) | Actual | Log |
|---|---|---|---|---|
| T-CHAIN-05 | CONFIRM_RERUN | bước 14 = 1.880.000 | **RERUN · 1880000 ✓** | `run 1 = 2000000; xác nhận NOTREL; run 2 = 1880000; chênh 120000 (dòng 168.000 chỉ tiết kiệm 120000)` |
| T-CHAIN-03 | RUNTWICE | bước 14 = 1.920.000, hash/steps giống | **SAME · 1920000 ✓** | `run 1/2: InputsHash giống nhau; StepOutputsJson giống nhau; tổng 1920000 / 1920000; hash = 168\|436:CONSULT:300000:2026-07-14;437:LAB:1400000:…;438:DRUG:900000:…;439:SUPPLEMENT:400000:…` |
| T-CHAIN-01 | DRYRUN | bước 13 SKIP = 1.920.000 | SKIP · 1920000 ✓ | — |
| T-CHAIN-02 | DRYRUN | bước 11 PASS = 1.920.000 log∋"quy tắc tạm B-19" | PASS · 1920000 ✓ | — |
| T-CHAIN-04 | DRYRUN | bước 10 CUT = 532 | CUT · 532 ✓ | "67 + 67" (làm tròn từng dòng, không phải 133 làm tròn một lần) |
| T-CH12-d (bổ sung PO, W-8) | DRYRUN | bước 12 PASS = 1.920.000 log∋"đã dùng 0/20000000"‖"trong hạn" | **PASS · 1920000 ✓** | `200: 1920000 so hạn mức còn 20000000 (đã dùng 0/20000000) — trong hạn` |

**Phân rã 120.000 — đọc từ CalcSnapshot run 2 của TST-T-CHAIN-05 (ClaimId 171, F48 "RUN #2 (TEST) 1.880.000 ₫"), con số của MÁY:**

| Bước | Run 1 (2.000.000) | Run 2 (1.880.000) | Chênh |
|---|---|---|---|
| B06 | 6 dòng trong phạm vi = 3.128.000 | `5 dòng thuộc phạm vi bảo hiểm = 2960000 ₫; 1 dòng từ chối (NOTREL); dòng 4 NOTREL (khuyến nghị Confirmed)` | −168.000 |
| B08 | `200.2 …: 1018000 so trần 1000000 — cắt 18000` (200.1 cắt 610.000 cả hai run) | `200.2 200.2\|2026-06-10\|Phòng khám Fixture: 850000 so trần 1000000 — không cắt` → 2.350.000 | **+18.000** (cap relief) |
| B10 | Đồng chi trả 500.000 | `Đồng chi trả: 470000 ₫; dòng 1 200.1 = 60000; dòng 2 = 240000; dòng 3 = 0; dòng 5 200.2 = 140000; dòng 6 = 30000` → 1.880.000 | **+30.000** (copay relief) |
| B12 | trong hạn | `200: 1880000 so hạn mức còn 20000000 (đã dùng 0/20000000) — trong hạn` | — |
| B14 | 14:POSTED | `Post tạm: 8 posting AMOUNT (1880000 ₫), 0 OCCURRENCE; trạng thái PROVISIONAL` | — |

168.000 − 18.000 − 30.000 = **120.000** — khớp hand-run của PO (cap relief 200.2 + copay relief) và khớp log runner. Bằng chứng: `output/bnk-dev-claimmvp/shots-rev154/f48-claim171-run2-b06-b14-rev158.jpg`.

**Phát hiện phụ trên F48 hồ sơ 171 (không chặn, ghi để sửa batch F):**
- F-CH-1 · Card thứ hai "RUN #2 (AFTER CONFIRMATION) 0 ₫ · line excluded · re-capped · co-pay recomputed" là card tĩnh từ prototype, hiện 0 ₫ cạnh card thật 1.880.000 — gỡ hoặc nối dữ liệu.
- F-CH-2 · Gate card điều kiện 1 trích log bước 8 của **run 1** ("200.2 …: 1018000 so trần 1000000 — cắt 18000") trong khi F48 hiện run 2 — gate đọc ChainRun/CalcSnapshot cũ (RunNo asc?) — cùng lớp lỗi K4 đã sửa trên F48.
- F-CH-3 · Panel "Limit ledger — outpatient (200)": Used 0 ₫ / **Left 0 ₫** — Left phải = AnnualLimit snapshot − Used = 20.000.000; panel UI vẫn đọc LimitLedger (W-8 phía màn hình).
- F-CH-4 · Fixture trùng: TST-T-CH12-a/b/c tồn tại 2 lần (ClaimId 127–129 và 165–167) — `Seed_ChainTestFixtures` Part 4 không idempotent; runner lấy `max 1` nên kết quả phụ thuộc thứ tự. Xoá bản trùng khi dọn TST-/FX-.

Bằng chứng: `output/bnk-dev-claimmvp/shots-rev154/chaintests-rev158-tchain05-rerun-1880000.jpg`.

**Còn 28** (trong 72): T-CHAIN-06 (cần Finalize + 2 hồ sơ cùng thành viên) · T-CHAIN-07 (bộ đếm DAY, Q-IDP-06) · T-CH14-a…f (6) · T-SEED-01…11 (11) · T-GATE-01…07 (7, BLOCKED — câu hỏi 3/4 điều kiện 20/09) · T-CH11-c/d MANUAL (2).

### Ghi chú cho deck — slide "1.880.000 là của máy" (số MÁY, đọc 21/09 ~22:00)

- Ảnh 1: `chaintests-rev158-tchain05-rerun-1880000.jpg` — dòng runner T-CHAIN-05 ✓ RERUN · 1880000, log `run 1 = 2000000; xác nhận NOTREL; run 2 = 1880000; chênh 120000 (dòng 168.000 chỉ tiết kiệm 120000)`.
- Ảnh 2: `f48-claim171-run2-b06-b14-rev158.jpg` — F48 hồ sơ TST-T-CHAIN-05 RUN #2: B06 `5 dòng thuộc phạm vi = 2960000 ₫; 1 dòng từ chối (NOTREL)` · B08 `200.2 …: 850000 so trần 1000000 — không cắt` · B10 `Đồng chi trả: 470000 ₫` · B14 `8 posting AMOUNT (1880000 ₫) … PROVISIONAL`.
- Câu trả lời khi giám khảo hỏi "48.000 đi đâu": 168.000 bị loại ⇒ nhóm 200.2 xuống 850.000 nên **không còn chạm trần 1.000.000 (run 1 cắt 18.000)** ⇒ +18.000; base copay giảm 150.000 ⇒ copay 20 % **500.000 → 470.000** ⇒ +30.000. 168.000 − 18.000 − 30.000 = 120.000. Cả ba số đều trên F48 run 2, không có số nào tính tay.

## Run #10 — rev 159 (M1: T-CHAIN-06 CONCURRENT), 21/09/2026 22:09 — **43/45**, 2 ✗ đều là lỗi harness, không phải lỗi chuỗi

| Test | Actual | Log máy | Vì sao ✗ |
|---|---|---|---|
| T-CHAIN-06 | MOVED · **360.000** | `hồ sơ 1 = 640000 → FINAL, ledger 200 19000000 → 19640000; hồ sơ 2 run 1 = 640000; run 2 = 360000 (còn 360000); posting run 1 đảo 2, còn tạm 0; run 2 tạm 2` — **đúng từng số theo thiết kế §7** | `[FAIL: TotalB1 640000 ≠ 2000000]` — dòng seed T-CHAIN-06 mang AssertAmountBefore 2.000.000 thay vì 640.000 (lỗi Seed_ChainTests_C) |
| T-CHAIN-05 | RERUN · 1.880.000 | run 1 = 1.880.000 | `[FAIL: run 1 1880000 ≠ 2000000]` — Recommendation NOTREL của hồ sơ 171 vẫn "Confirmed" từ run #9; re-seed không reset |

**Nguyên nhân gốc F-CH-4 (đọc Work list sau seed lần 4):** `Seed_ChainTestFixtures` **chưa bao giờ xoá** gì — mỗi lần bấm "Seed fixtures" nối thêm trọn bộ: TST-T-CH01…09 ×4 (Id 67–94, 95–122, 133–160, 172–199), Part4 ×3, Part5 ×2 (168–171, 207–210), Part6 ×1 (211–212); 41 mã, ~150 hồ sơ. Runner `max 1` chọn bản bất kỳ — pass được vì các bản giống nhau, vỡ ngay khi một bản bị test làm đổi (171 Confirmed). Sửa ở lượt M3: cleanup xoá đúng (con trước: LedgerPosting, CalcSnapshot, ChainRun, TermCheckpoint, Recommendation, Assessment, ClaimLine → Claim; FX- member/ledger/policy), `ChainTest_ConfirmRerun` tự reset NOTREL về Proposed trước run 1, sửa AssertAmountBefore T-CHAIN-06.

## Run #11 — rev 162 (M6 + M5'), 21/09/2026 22:41 — **46/46 pass**, 5,0 s round-trip (8 lần chạy thật)

Trước khi chạy: "Seed fixtures" toast **"Đã xoá 185 hồ sơ TST-, hiện có 42"** (5 s) — cleanup thật lần đầu tiên; "Re-seed 71 rows".

| Test | Mode | Kỳ vọng (expected-71 nguyên văn) | Actual | Log máy |
|---|---|---|---|---|
| T-CHAIN-06 | CONCURRENT | Claim 2 chạy lại trên bộ đếm hiện tại; không post lại con số lần chạy đầu (BR-LEDGER-8) | **MOVED · 360.000 ✓** | `hồ sơ 1 = 640000 → FINAL, ledger 200 19000000 → 19640000; hồ sơ 2 run 1 = 640000; run 2 = 360000 (còn 360000); posting run 1 đảo 2, còn tạm 0; run 2 tạm 2` |
| T-CHAIN-07 | RUN | Trần ngày ICU 3.000.000 áp; DAY +1 trên 100.1.1 **và** trên 100.1; AMOUNT roll-up tới 100 | **POSTED · 3.000.000 ✓** | `Post tạm: 3 posting AMOUNT (3000000 ₫), 0 OCCURRENCE, 2 DAY (100.1.1 +1, 100.1 +1); trạng thái PROVISIONAL` |
| T-CHAIN-05 | CONFIRM_RERUN | 1.880.000 | RERUN · 1.880.000 ✓ (NOTREL tự reset) | — |
| T-CH09-c (hồi quy) | DRYRUN | HOLD Q-IDP-06 khi TreatmentDays = 0 | HOLD · 1.000.000 ✓ | — |

Rev 161 trước đó: 0/46 vì `A5Step09_CountLimit` `OS-BERT-60202 Index 1 is out of range` (nhánh DAY dùng `Ctx.Lines[I]`) → M6 sửa về record hiện tại của ForEach. Hai publish rev 160 thất bại `OS-BEW-50000` do node ForEach chết trong `Seed_ChainTestFixtures` (M4 xoá).

Bằng chứng: `output/bnk-dev-claimmvp/shots-rev154/chaintests-rev162-tchain07-posted-3000000.jpg`, `chaintests-rev162-tchain03-07-pass.jpg`.

**Khoảng trống giữ nguyên (tasks 5d-iv):** T-CHAIN-06 mới thử hàm `A5Ledger_Finalize` qua runner; khi Finalize nối vào Approve thật phải chạy lại qua Approve. **Còn 26:** T-CH14-a…f (6) · T-SEED-01…11 (11) · T-GATE-01…07 (7, BLOCKED) · T-CH11-c/d MANUAL (2).

## Run #12 / #13 — rev 166 (165 của PO + panel Chain data), 22/09/2026 00:53 / 00:54

- **Run #12: 45/46**, 5,1 s — ✗ duy nhất **T-CH01-c** (`[FAIL: outcome PASS ≠ FAIL]`, log bước 1 "phí đã ghi nhận"). Hồ sơ 257 vẫn trỏ `FX-STD-UNPAID` nhưng policy đó hiện **"premium paid"** → dữ liệu fixture bị ghi đè sau run #11 (22:41 21/09), không phải chuỗi sai. Nghi phạm: seed 23:26 tối 21/09 (rev 163–165 + RunSeedBatch0Now) cập nhật PremiumPaid trên mọi Policy.
- **Run #13: 46/46**, 4,0 s — sau "Seed fixtures" (toast "Đã xoá 42 hồ sơ TST-, hiện có 42" — idempotent đúng) + "Re-seed 71 rows". Cùng code, chỉ đổi dữ liệu ⇒ khẳng định run #12 là data drift.

**Ghi nợ:** một seed ngoài harness ghi `Policy.PremiumPaid` lên policy FX- → fixture không được bảo vệ. Hoặc seed batch 0 phải bỏ qua PolicyNo like "FX-%", hoặc harness reseed fixtures trước mỗi lần chạy (thêm vào đầu `RunChainTests`). Chưa sửa — chờ PO chọn.
