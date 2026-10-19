# archive — change đã hoàn thành

Hồ sơ lịch sử. **Không sửa.**

| Change | Thời gian | Kết quả | Hồ sơ ở đâu |
|---|---|---|---|
| `ui-console-batches` — 6 batch UI console qua `outsystems-design-to-app` | 18–20/09, rev 30–91 | 10 màn console, audit **86 %** | `../../../output/bnk-dev-claimmvp/` — `runtime-audit-rev*.md`, `review-vs-prototype-v05.md`, `shots-rev*/` |
| `portal-split-myinsurance` — tách portal khách thành app riêng | rev 87 | 7 màn portal, audit **86 %**, dùng chung Entity + Service Action của ClaimMVP | `../../../output/bnk-dev-myinsurance/runtime-audit-rev7.md`, `shots-rev6/` |

## Vì sao hồ sơ nằm ở `output/` chứ không copy vào đây

Audit là **bằng chứng chạy thật** — probe DOM, screenshot, số đo. Chúng đã có vị trí và quy ước đặt tên theo rev. Copy vào đây sẽ tạo bản thứ hai và bản thứ hai sẽ lệch.

Ở dự án này, `output/` **là** cơ chế verify, thay cho git diff mà OpenSpec giả định. Xem `../../project.md` §"Khác OpenSpec gốc".

## Bài học rút từ pass UI — đọc trước khi mở change UI mới

1. **Luôn dùng widget `Link`, không bao giờ `Container` OnClick.** OnClick Mentor gắn lên Container/Table row không bao giờ chạy — lỗi này làm portal mất hẳn đường vào màn chi tiết tới rev 3.
2. **`ColumnsMediumLeft` là block hẹp-bên-TRÁI.** Bố cục 2/3–1/3 dùng `ColumnsMediumRight`.
3. **Skip-link không được dùng `href="#…"`** — router SPA điều hướng về màn gốc. Dùng client action OnClick.
4. **Rule `.cc-table td a { inline-flex }` bóp chữ trong ô bảng.** Kiểm lại bảng sau mỗi lần đụng tap-target.
5. **Mentor regress lặng lẽ** — Checkpoint queue hỏng rev 41, phát hiện rev 44. Chụp màn sau mỗi batch là cách rẻ nhất để bắt.
6. **Icon: dùng font riêng `ccicons`**, không `IIcon`/Phosphor — đây là chỗ bảng map block ban đầu sai.
