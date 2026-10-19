# Matching medical facility names against the IDP catalogue — ClaimMVP rev 933 (18/10/2026)

> English translation for reviewers. Vietnamese original: [khop-ten-co-so-y-te-rev933-18-10.md](khop-ten-co-so-y-te-rev933-18-10.md).

## Changes (rev 929 → 933)
- English names are translated into Vietnamese before comparison: Hospital → Bệnh viện, Clinic → Phòng khám, General → Đa khoa, International → Quốc tế, Medical Center → Trung tâm y tế, Polyclinic → Phòng khám đa khoa; the abbreviations BV, BVĐK, PK, PKĐK, TTYT, TYT, ĐK, TW are expanded.
- Generic words (bệnh viện, phòng khám, đa khoa, quốc tế, trung tâm, trạm y tế, công ty, TNHH, xã, phường…) are removed, then the proper-name part is compared.
  - The phrases "trung tâm", "thành phố", "thị trấn", "chi nhánh", "cổ phần" are only removed when they appear as a whole phrase (rev 933).
  - As a result, the words Tâm, Trưng, Phố, Phan… are still kept.
- Match levels:
  - Same proper name, same order → CATALOGUE.
  - One side contains the other → PARTIAL.
  - A 1-character spelling error is accepted only when **both** words are ≥ 5 letters long (rev 933).
  - A short 1-word proper name (FV) is accepted only when exactly 1 row contains that word.
- Row selection (identical on the ProviderLookup screen and in LookupProvider/AG-4), in priority order:
  1. CATALOGUE.
  2. Fewer extra words.
  3. Same facility type as the looked-up name (BV / TTYT / TYT / PK — added in rev 933).
  4. Higher IDP score.
- No matching row → Found = False, MatchType = NOT_FOUND.

## Verification on rev 933
- Name-matching self-check: 47/47 pass (36 old cases + 11 new cases).
- ChainTests: 46/46, gate 5/5.
- Live lookup:

| Looked-up name | Row selected |
|---|---|
| Vinmec International Hospital | 01161 |
| Tam Anh General Hospital | 01250 BV ĐK Tâm Anh (rev 932 selected no row) |
| Bệnh viện Trưng Vương | 79026 |
| Hong Ngoc Hospital | BV ĐK Hồng Ngọc |
| Nội tiết Trung ương | TW row |
| FV Hospital | 79509 |
| Chợ Rẫy | 79048 |
| Phòng khám Fixture | No row selected |
| Clinic | No row selected |

## Still open
- Hanoi French Hospital, Raffles: not in table 322 → a different name table (alias) is needed.
- Catalogue rows without a code (BV ĐK Hồng Ngọc, BV Nội tiết TW): still selected, but the facility code is empty.
- When the IDP API fails, LookupProvider returns Found = True, MatchType = CATALOGUE_UNAVAILABLE. This behaviour pre-dates this change and is unchanged.
- The "Từ đặc trưng" (EN: "Distinctive word") row and the self-check button on the ProviderLookup screen are test tools and can be hidden before the demo.
