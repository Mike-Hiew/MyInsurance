# Spec — Data model & enum

> **Văn bản đầy đủ:** Project "Hackaton" → `claude/build-01-data-model-va-enum` (~177 KB).
> File này giữ các **ràng buộc bất biến** — thứ mà mọi change phải kiểm lại trước khi đụng vào.

## Quy mô chuẩn

- **32 entity**: 10 hợp đồng · 15 claim · 7 IDP
- **31 enum**
- **30 ràng buộc phải tự thêm** (ODC không sinh hộ)
- 25 bẫy đã ghi trong doc gốc

## Bốn enum quyết định demo — không được lệch

| Enum | Số giá trị | Ghi chú |
|---|---|---|
| `claimState` | **13** | → 11 trạng thái hiện cho khách. Bảng ánh xạ ở `screens-wording` |
| `lossType` | **12** | 8 giá trị sống trong MVP 1 · 4 counter-only |
| Mã từ chối | **7, danh sách đóng** | `LIMIT` `PERIOD` `NOBEN` `WAIT` `EXCL` `NOTREL` `NOTCOV` |
| `LineCategory` / `BenefitCode` | — | CONSULT/LAB/IMAGING/DRUG/SUPPLEMENT/DENTAL · 200.1 / 200.2 / 300 |

## Bất biến kiến trúc

1. **Không agent nào — kể cả Mentor — được tạo Entity.** Entity sinh từ spec, qua `outsystems-spec-driven-build`.
2. **Tiền chỉ chảy qua `RunAdjudicationChain`.** Không tool nào của agent ghi được trường tiền, sổ hạn mức, lệnh chi hay trạng thái hồ sơ.
3. **Kết quả trích xuất thô bất biến.** Không ai, kể cả admin, sửa `RawExtractionJson`. Sửa tại chỗ ghi vào giá trị đã sửa và **giữ nguyên giá trị gốc**.
4. **Sổ hạn mức post theo kỳ chứa ngày điều trị của từng dòng.** Hồ sơ vắt qua hai năm hợp đồng chạm hai counter và **không bao giờ cộng lại**.

## ⚠ Trạng thái thực tế — 20/09/2026

Entity hiện có trên ClaimMVP được **suy ra từ `Prototype_Insurance_BnK_v0.5.html`** để màn hình chạy được, **chưa đối chiếu với spec này**.
→ Xem `changes/active/verify-entity-vs-build01/`.
