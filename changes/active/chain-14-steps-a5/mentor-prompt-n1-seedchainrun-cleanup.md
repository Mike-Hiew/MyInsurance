# Prompt lượt N1 — gỡ ChainRun/CalcSnapshot khỏi Seed_AssessmentAndTerms

Bối cảnh: lượt J2-d chết giữa chừng ("hết bộ nhớ sandbox 500 MB", action ~200 node,
mỗi lần duyệt `.Nodes` nạp cả graph). Đã xoá được 3 node CalcSnapshot hồ sơ A
(CrCSA7/8/9). Còn lại: ChainRun + CalcSnapshot của hồ sơ B, C, D (và phần còn sót của A).

**Luật cho lượt này: mỗi prompt là MỘT lượt Mentor riêng. Không gộp. Không lượt nào
xoá quá 4 node.** Nếu một lượt báo hết bộ nhớ, dừng lại và chuyển sang sửa tay ODC Studio.

---

## N1-a — CHỈ ĐỌC, không sửa gì

Read-only turn. Do not modify anything.

In app ClaimMVP, Server Action `Seed_AssessmentAndTerms`:

List every node that creates or deletes a `ChainRun` or a `CalcSnapshot` row.
For each, report: node name, node type, its immediate predecessor node name,
its immediate successor node name, and which claim (A / B / C / D) it belongs to.

Also report, for each `CalcSnapshot` create node, whether its ChainRunId input
reads the Id of a `ChainRun` create node in the same action, and which one.

Do not report the whole action. Do not change anything. No publish.

---

## N1-b — xoá CalcSnapshot hồ sơ B

Edit `Seed_AssessmentAndTerms` only. Delete ONLY the CreateCalcSnapshot nodes that
belong to claim B (the names N1-a reported). Reconnect each deleted node's predecessor
directly to its successor. Delete nothing else — leave every ChainRun node in place.

Then report, in order, the node sequence of `Seed_AssessmentAndTerms` from its start
node to its end node, and the validation error count. Do not publish.

---

## N1-c — xoá ChainRun hồ sơ B

Edit `Seed_AssessmentAndTerms` only. Delete ONLY the CreateChainRun node(s) of claim B,
plus any Assign node whose only purpose is to set fields of that ChainRun record.
Reconnect predecessor to successor. Delete nothing belonging to claim C or D.

Report the node sequence and the validation error count. Do not publish.

---

## N1-d — hồ sơ C

Same as N1-b + N1-c, for claim C: CalcSnapshot nodes first, then the ChainRun node.
If this turn runs low on memory, stop after the CalcSnapshot deletions and say so.

Report the node sequence and the validation error count. Do not publish.

---

## N1-e — hồ sơ D, rồi phần sót của A

Same, for claim D. Then delete any remaining ChainRun/CalcSnapshot create node of
claim A that N1-a still listed.

Report the node sequence and the validation error count. Do not publish.

---

## N1-f — kiểm rồi publish

Read-only first, then one publish.

1. Confirm `Seed_AssessmentAndTerms` now contains zero create and zero delete nodes
   for `ChainRun` and for `CalcSnapshot`. List any that remain.
2. Confirm `RunSeedBatch0Now` still contains the `CallRunChain` node added in turn J2-b
   (`RunAdjudicationChain` on the claim looked up by ClaimNo "CLM-2026-0001",
   TriggeredBy "seed"), and that it still runs after `CallSeedChainTests`.
   If that node is missing, say so and do not publish.
3. Report row counts of `ChainRun` and `CalcSnapshot` as they are right now.
4. Publish ClaimMVP. Report the revision number and the validation error/warning counts.

---

## N1-g — kiểm bằng dữ liệu (sau publish, người bấm nút)

Không phải prompt Mentor. Mike bấm "Re-run demo seed" trên màn Administration,
rồi bảo Mentor đọc lại:

Read-only. Report the current row count of `ChainRun` and of `CalcSnapshot`,
and for claim CLM-2026-0001 the RunNo values present in `ChainRun`.

**Điều kiện đạt:** số dòng `ChainRun` sau lần seed này **bằng** số ở N1-f bước 3
cộng đúng số run mà `RunAdjudicationChain` tạo ra (1 cho CLM-2026-0001).
Nếu nó nhảy thêm 3–4 dòng thì vẫn còn node seed chưa xoá.

---

## QUYẾT ĐỊNH PO 21/09 — DỪNG LƯỢT N1, CHUYỂN SANG SỬA TAY

Báo cáo N1-f cho thấy **27 node CreateCalcSnapshot vẫn còn** (A 9 · B 9 · C 9 · D 3
theo phép cộng của chính Mentor, và 9+9+9+3=30 chứ không phải 27 — con số cũng không
khớp), **3 node CreateChainRun vẫn còn** (`CrCRA`/`CrCRC`/`CrCRD`). Nghĩa là các lượt
xoá N1-b…N1-e **không đọng lại**.

Mentor tự giải thích rằng `CrCSB1`–`CrCSB6` "thực ra là assignNode40–48 bị đánh số lại".
**Không chấp nhận lời giải thích này**: một Assign node không thể biến thành
`IExecuteServerActionNode` gọi `CreateCalcSnapshot`. Mentor đang bịa. Phần đáng ghi nhận
duy nhất là nó tự gắn cờ mâu thuẫn ở cuối.

**KHÔNG chạy tiếp N1-b…N1-e.** Ba lần xoá không đọng = cơ chế hỏng, không phải xui.

**Điểm sáng, đã xác nhận và phải giữ:** `RunSeedBatch0Now` còn nguyên chuỗi
`CallSeedChainTests → GetClaim0001 → CallRunChain → LogDone → End`, `CallRunChain` gọi
`RunAdjudicationChain(GetClaim0001.List.Current.Claim.Id, "seed")`. Không ai được đụng.

**Mentor KHÔNG publish** — nó chỉ "save", rev 943 là số revision model, không phải app
revision. App đang chạy vẫn là **rev 162, 46/46 pass**. Trạng thái tốt đang được giữ.

**Rủi ro phải xử trước:** nếu bản save 943 có chứa một `Seed_AssessmentAndTerms` sửa dở,
lần publish sau của bất kỳ ai sẽ đẩy nó lên. Phải kiểm trước khi publish bất cứ thứ gì.

**Đường đi mới:** sửa tay trong ODC Studio (xem hướng dẫn PO 21/09). Không giao lại
việc xoá node trong action này cho Mentor.
