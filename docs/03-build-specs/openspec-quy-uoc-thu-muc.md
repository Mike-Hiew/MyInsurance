# Quy ước thư mục openspec — chốt 20/09/2026

Áp dụng quy ước **spec-driven development** (OpenSpec, OutSystems giới thiệu), rút gọn cho một người trong 9 ngày còn lại.

**Vị trí trên đĩa:** `E:\Hackaton\openspec\`

---

## Ba lớp

| Thư mục | Là gì | Khi nào sửa |
|---|---|---|
| `specs/<capability>/spec.md` | Nguồn chân lý ổn định — hệ thống *là* gì | Chỉ khi một change được archive |
| `changes/active/<name>/` | Delta đang chạy — cái đang *đổi* | Tự do khi đang làm |
| `changes/archive/<name>/` | Change đã xong | Không sửa |

**Luật duy nhất:** một thay đổi về hành vi hệ thống phải tồn tại ở `changes/active/` **trước khi** nó tồn tại trong code.

Chỗ đau đã gặp và là lý do lập thư mục này: *cổng tự duyệt 4 điều kiện* sống song song với chuẩn 3 điều kiện suốt ~90 rev mà không nằm ở đâu cả.

---

## Ba chỗ cắt so với OpenSpec gốc, có chủ ý

1. **Bỏ `proposal.md` và `design.md`.** Chúng có giá trị khi cần thuyết phục người khác. Một người, 9 ngày, mọi quyết định kiến trúc đã nằm trong `build-00` §5 — viết proposal là nghi lễ. Giữ `tasks.md` vì nó là điểm nối giữa các phiên làm việc và là đầu vào của `outsystems-spec-driven-build`.
2. **Bỏ `schemas/`** cho tới sau pitch.
3. **Verify không bằng git diff.** OpenSpec giả định code nằm trong git: spec đổi → diff → PR → archive. Ở đây code là một app ODC trên cloud do Mentor sửa — không repo, không diff, không PR. Thứ thay thế là **runtime audit + screenshot** trong `E:\Hackaton\output\`, và nó đang hoạt động tốt (9 vòng audit, probe DOM thật). Không bỏ cái đó để chạy theo quy trình git.

---

## Phân vai giữa đĩa và Project — tránh hai bản lệch nhau

| Nội dung | Gốc ở đâu |
|---|---|
| **Văn bản đầy đủ của 7 spec** (`build-00…06`) | **Project claude.ai** — không copy xuống đĩa |
| **Tóm tắt ràng buộc bất biến + con trỏ** | `openspec/specs/<cap>/spec.md` trên đĩa |
| **Change đang chạy** (`tasks.md`, delta spec) | **Đĩa** — Project không giữ bản sao |
| **Nguồn cấp trên** (A1–A5, D1–D3, BRD) | Đĩa, `Preparing/04_Nghiep-vu-va-du-lieu/` |
| **Bằng chứng chạy thật** (audit, shots) | Đĩa, `output/` |

Lý do không copy `build-01…06` xuống đĩa: ~400 KB, và hai bản sẽ lệch — đúng vấn đề mà OpenSpec sinh ra để giải quyết, chỉ ở một tầng khác. `specs/<cap>/spec.md` trên đĩa giữ đúng phần mà một agent cần biết để **không làm sai**: enum, con số, bất biến, và đường tới văn bản đầy đủ.

---

## Vòng đời một change

1. `mkdir changes/active/<ten-ngan-gon>/` → viết `tasks.md`
2. Nếu change làm đổi một spec: thêm `specs/<cap>/spec.md` chứa **đúng đoạn khác biệt**, không chép cả spec
3. Làm việc, tick task
4. Xong → merge đoạn delta vào Project doc tương ứng → `mv changes/active/<x> changes/archive/`

---

## Ba change đang mở

| Change | Hạn | Chặn |
|---|---|---|
| `verify-entity-vs-build01` | 22/09 | Mọi việc còn lại |
| `approval-gate-3-conditions` | 22/09 | Màn Approval + bước cuối của chuỗi · **chờ PO trả lời** |
| `chain-14-steps-a5` | 25/09 | Số tiền, tức là chặn bài dự thi |

Đã archive: `ui-console-batches` (rev 30–91) và `portal-split-myinsurance` (rev 87).
