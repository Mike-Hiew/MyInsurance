# ClaimMVP — Static Entity hiện có (rev 128)

Xuất tự động từ MCP `context_entities` / `context_search` (full_fields) ngày 2026-09-20. Nguồn: trường `records` của từng static entity = **danh sách record identifier** (tên record trong Studio).

**Giới hạn của nguồn:** API design-time chỉ trả *tên* record. Giá trị `Label` / `Order` / `Is_Active` của từng record là dữ liệu runtime, `context_*` không trả — muốn có phải mở Studio hoặc query runtime. Thứ tự record trong API không phải thứ tự Studio; dưới đây xếp theo bảng chữ cái.

**Tổng: 4 static entity** do ClaimMVP sở hữu. Tất cả cùng bộ attribute chuẩn ODC: `Id` (Integer, PK) · `Label` (Text 50) · `Order` (Integer) · `Is_Active` (Boolean) — chi tiết ở `current-model.md` §2.

| Static entity | Số record | Records (identifier) |
|---|---|---|
| ClaimStatus | 8 | `Approved`, `Declined`, `Extracting`, `ExtractionError`, `Paid`, `PendingApproval`, `Received`, `UnderReview` |
| DecisionCode | 5 | `COVERED`, `NOTCOV`, `NOTREL`, `PARTIAL`, `PENDING` |
| PolicyStatus | 3 | `Active`, `Expired`, `Lapsed` |
| TreatmentType | 4 | `Dental`, `Inpatient`, `Maternity`, `Outpatient` |

## ClaimStatus
> key `137904b2-1943-4915-a4a9-065cd60e4f02` · public: True · physical `claim_jj8jjgtc9mtpf6lzgexm1wh0` · last change: 2026-09-20

| # | Record identifier | Label | Order | Is_Active |
|---|---|---|---|---|
| 1 | `Approved` | *(runtime)* | *(runtime)* | *(runtime)* |
| 2 | `Declined` | *(runtime)* | *(runtime)* | *(runtime)* |
| 3 | `Extracting` | *(runtime)* | *(runtime)* | *(runtime)* |
| 4 | `ExtractionError` | *(runtime)* | *(runtime)* | *(runtime)* |
| 5 | `Paid` | *(runtime)* | *(runtime)* | *(runtime)* |
| 6 | `PendingApproval` | *(runtime)* | *(runtime)* | *(runtime)* |
| 7 | `Received` | *(runtime)* | *(runtime)* | *(runtime)* |
| 8 | `UnderReview` | *(runtime)* | *(runtime)* | *(runtime)* |

## DecisionCode
> key `87180646-df8a-4981-887d-b5496773f1cf` · public: True · physical `decis_jj8jjgtc9mtr8puq095q1ct3` · last change: 2026-09-20

| # | Record identifier | Label | Order | Is_Active |
|---|---|---|---|---|
| 1 | `COVERED` | *(runtime)* | *(runtime)* | *(runtime)* |
| 2 | `NOTCOV` | *(runtime)* | *(runtime)* | *(runtime)* |
| 3 | `NOTREL` | *(runtime)* | *(runtime)* | *(runtime)* |
| 4 | `PARTIAL` | *(runtime)* | *(runtime)* | *(runtime)* |
| 5 | `PENDING` | *(runtime)* | *(runtime)* | *(runtime)* |

## PolicyStatus
> key `bed05caa-c5e9-437f-8c39-c747a395cbe2` · public: True · physical `polic_jj8jjgtc9mtq389v3tb7l1o7` · last change: 2026-09-20

| # | Record identifier | Label | Order | Is_Active |
|---|---|---|---|---|
| 1 | `Active` | *(runtime)* | *(runtime)* | *(runtime)* |
| 2 | `Expired` | *(runtime)* | *(runtime)* | *(runtime)* |
| 3 | `Lapsed` | *(runtime)* | *(runtime)* | *(runtime)* |

## TreatmentType
> key `c79b470b-9cee-4835-8609-2a6a14277158` · public: True · physical `treat_jj8jjgtc9mty2ko_7tnhbq72` · last change: 2026-09-20

| # | Record identifier | Label | Order | Is_Active |
|---|---|---|---|---|
| 1 | `Dental` | *(runtime)* | *(runtime)* | *(runtime)* |
| 2 | `Inpatient` | *(runtime)* | *(runtime)* | *(runtime)* |
| 3 | `Maternity` | *(runtime)* | *(runtime)* | *(runtime)* |
| 4 | `Outpatient` | *(runtime)* | *(runtime)* | *(runtime)* |

## Được dùng làm FK từ đâu (theo `current-model.md`)

| Static entity | Attribute tham chiếu |
|---|---|
| ClaimStatus | Claim.StatusId |
| DecisionCode | ClaimLine.DecisionCodeId |
| PolicyStatus | Policy.StatusId |
| TreatmentType | Claim.TreatmentTypeId |
