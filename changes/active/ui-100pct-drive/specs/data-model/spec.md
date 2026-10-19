# Delta — Data model: quy tắc `Claim.ClaimNo`

> Áp vào `openspec/specs/data-model/spec.md` (§"Bất biến kiến trúc") và Project doc `build-01` khi archive change này.

## Thêm

5. **`Claim.ClaimNo` là duy nhất và sinh ở server, không ở màn hình.** Định dạng `CLM-<YYYY>-<Id đệm 4 chữ số>` (`CLM-2026-0034`), gán ngay sau `CreateClaim` trong cùng Server Action tạo hồ sơ (`Portal_SubmitClaim2` và mọi đường tạo hồ sơ khác). Service Action tạo hồ sơ **trả `ClaimNo`** để màn hình hiển thị đúng giá trị đã lưu — màn hình không tự ghép chuỗi từ `Id`.

## Ghi chú

- Dữ liệu seed giữ số cũ (`CLM-2026-0001…0004` cho Id 17–20) vì A2/A5 và prototype trích dẫn các số này. Không renumber seed.
- `FixDuplicateClaimNos` (timer chạy khi publish) là biện pháp một lần cho dữ liệu dev; gỡ bỏ sau khi PO chốt số phận 4 hồ sơ test (xem `tasks.md` §E).
