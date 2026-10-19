# DELTA — `specs/data-model` · 5 chỗ spec phải sửa theo hiện thực

> Đây là **đoạn khác biệt**, không phải spec đầy đủ. Nguồn: `../../delta.md` §6.1.
> Khi duyệt xong, merge vào Project doc `build-01` rồi archive change.

## S1 — Tiền lưu bằng `Decimal(18,0)`, không phải `Currency`

`build-01` §6.1 ánh xạ `money` → ODC `Currency`. Hiện thực dùng `Decimal(18,0)` **nhất quán trên mọi entity tiền**.

**Giữ hiện thực.** `Decimal(18,0)` ép *"số nguyên đồng, KHÔNG thập phân"* của chính §6.1 ở tầng schema, còn `Currency` thì không. Sửa bảng §6.1.

## S2 — `RecEvidence` là entity riêng, không phải `EvidenceJson`

`build-01` §2.9 để bằng chứng ở `ClinicalRecommendation.EvidenceJson` (Text). Hiện thực có entity `RecEvidence` (`Kind`, `Reference`, `Summary`, `SortOrder`).

**Giữ hiện thực.** Ràng buộc #15 (*bằng chứng rỗng là response dị dạng*) kiểm bằng `Count() > 0` thay vì parse JSON, và `SortOrder` giữ đúng thứ tự engine trả về.

## S3 — `CalcSnapshot` tồn tại song song `ChainRun.StepOutputsJson`

**Giữ cả hai, với phân vai rõ:**
- `ChainRun.StepOutputsJson` — **bản ghi bất biến** của lần chạy, là thứ so sánh hai lần chạy.
- `CalcSnapshot` — **hàng hiển thị** cho tab calc, sinh ra từ JSON trên.

**Không được để hai nguồn lệch nhau:** `CalcSnapshot` chỉ ghi trong cùng transaction với `ChainRun`, không bao giờ sửa riêng.

## S4 — `TermRegister` phải có trong §2

`build-01` §4.21 định nghĩa `termStatus` / `termOutcome` nhưng §2 **không có entity nào chứa chúng**. `build-06` có màn Sổ điều khoản (F65) và hiện thực đã dựng `TermRegister` (`Code`, `Name`, `Scope`, `TermClass`, `ClauseRef`, `IsActive`, `AppliedCount30d`).

**Bổ sung vào §2 thành §2.17.** Kèm ràng buộc của §4.21: chỉ term `ACTIVE` được đánh giá **và được đếm** — `AppliedCount30d` không được đếm term `DRAFT`.

## S5 — Sửa số đếm trong chính `build-01`

| Chỗ | Ghi | Thực tế |
|---|---|---|
| §2 tiêu đề | "Mười lăm entity" | §2.1–2.16 = **16** |
| §0 / bảng tổng | "32 entity" | 10 + 16 + 7 = **33** (chưa kể 2 entity nối §8.9, §3.5) |

Cộng S4 ⇒ tầng claim thành **17**, tổng **34** (+2 nối = 36).

---

## Quyết định

**Duyệt ngày 20/09/2026 — cả 5 dòng: SỬA SPEC, giữ hiện thực.**

| # | Quyết | Lý do một dòng |
|---|---|---|
| S1 | ✅ Sửa spec | `Decimal(18,0)` ép số nguyên VND ở tầng schema — chặt hơn `Currency`, đúng tinh thần §6.1 |
| S2 | ✅ Sửa spec | Ràng buộc #15 kiểm bằng `Count() > 0`, không parse JSON; `SortOrder` giữ thứ tự engine |
| S3 | ✅ Sửa spec | `StepOutputsJson` = bản ghi bất biến, `CalcSnapshot` = hàng hiển thị; ghi cùng transaction, không sửa riêng |
| S4 | ✅ Sửa spec | `build-06` có màn Sổ điều khoản (F65) mà §2 không có entity chứa `termStatus`/`termOutcome` |
| S5 | ✅ Sửa spec | Số đếm trong `build-01` sai so với chính nội dung của nó (16 ≠ 15, 33 ≠ 32) |

**Việc còn lại để đóng delta này:** merge 5 điểm vào Project doc `build-01` trên claude.ai (không có trên đĩa), rồi cập nhật `specs/data-model/spec.md` gốc: quy mô chuẩn thành **34 entity** (10 · 17 · 7) và ghi `Decimal(18,0)` là kiểu tiền.
