# Demo 08/10 · My Insurance × BnK IDP

> English translation for reviewers. Vietnamese original: [README-demo-08-10.md](README-demo-08-10.md).

## tai-lieu\
| File | Used for |
|---|---|
| 1-slide-demo.html | Presentation slide deck (←/→ to change slide, click an image to enlarge) |
| 2-so-tay-he-thong.html | Understanding the system: idea, architecture, workflow, agents, BnK IDP, Q&A |
| 3-huong-dan-su-dung.html | User guide for every screen, every button, every scenario |
| 4-slide-bnk-idp.html | **Main presentation deck for 08/10 · short version, focused on the demo**: 17 presentation slides (~15 minutes) + 20-minute demo + 3 backup images; details moved to the appendix. Press N to show notes |
| 5-kich-ban-trinh-bay.html | Scripted version: slide on the left, spoken words + actions + transition lines on the right, with a timer. "Open the presentation window" ("Mở cửa sổ chiếu") lets you drag the slides to the projector. Press R: read everything / print |
| ban-day-du\ | Full 6-part version (~50 minutes) of the slides and the script, for use when a deep-dive presentation is needed |
| 5-kich-ban-trinh-bay.pdf | Pre-printed script (A4 landscape, 19 pages, including the appendix) |

## anh-man-hinh\
Real screenshots taken on 07/10/2026 (MyInsurance rev 147, ClaimMVP rev 471).
00 login · 01 customer home page · 02 create claim · 08 result of claim 0440 · 14 customer assistant ·
03 work list · 04 "What AI did" ("AI đã làm gì") card 0449 · 05 IDP extraction · 06 payout waterfall · 06c 14-step chain ·
07 cross-document check R1–R9 · 09 technical log · 10 result & ROI · 11/12 duplicate claim 0451 blocked

## chung-tu-mau\  (images to submit on MyInsurance)
| Folder | Loss type | Expected outcome | Source |
|---|---|---|---|
| 01_tu-duyet | Illness – outpatient, 08/10/2026 | Auto-approved 320.000 đ | demo-idp sample, date changed |
| 02_trung-ho-so | same as 01 | DUPLICATE → blocked | identical to 01 |
| 03_thieu-phieu-kham | Illness – outpatient | Waiting for supplement; submit bo-sung\ next | case02 |
| 04_ag3-thuoc-khong-lien-quan | Illness – outpatient | AG-3 proposes 3 lines | case03 |
| 05_du-lieu-lech | Illness – outpatient | WARN R1–R8 → blocked | case10 |
| 06_noi-tru-tien-lon | Accident – inpatient | 25.606.000 đ → four-eyes approval | case06 |
| 07_nha-khoa-tham-my | Dental – outpatient | Teeth whitening → checkpoint | case07 |
| 08_anh-xau | Illness – outpatient | Low-confidence line | L15 |
| 09_ngay-tuong-lai | Illness – outpatient | DATE_IMPOSSIBLE → blocked | L08 |
| 10_ngoai-tru-day-du | Illness – outpatient (NGUYỄN VĂN AN) | Clean claim with 4 documents, 1.089.500 đ; submit before L11 | case01 |
| 11_noi-tru-co-BHYT | Illness – inpatient (**TRẦN THỊ BÌNH**) | 5-day inpatient stay, social health insurance (BHYT) pays part, 6.297.000 đ | case04 |
| 12_tai-nan-ngoai-tru | Accident – outpatient (NGUYỄN VĂN AN) | Sprain, X-ray + incident statement, 965.000 đ | case05 |
| 13_thai-san-ngoai-tru | Maternity – outpatient (**TRẦN THỊ BÌNH**) | Threatened miscarriage, ultrasound + prescription, 1.268.000 đ | case08 |
| 14_thai-san-sinh-mo | Maternity – inpatient (**TRẦN THỊ BÌNH**) | Caesarean section, includes newborn costs, 14.432.000 đ | case09 |
| bat-loi\L01–L17 | see _DANH-SACH-LOI.md | Each set plants exactly ONE defect: R1–R8, impossible date, over 60 days, duplicate, paid by another party, pre-existing condition, missing ICD, bad image, image-format invoice, wrong loss type selected | bat-loi |
| mau-tung-template | — | 9 single images 6090–6098, submit them one by one to see IDP classify each template correctly | anh-mau |

**Quick view of all 110 images:** open `chung-tu-mau\_XEM-NHANH.html` (click an image to see it at full size).

Folders 03–14, bat-loi and mau-tung-template were copied from:
E:\Hackaton\insurance BNK\Bnk IDP\00. Deliverables\Templates_Claim_MVP_tenant177\anh-mau
(if they are not there yet, run tao-thu-muc-03-09.ps1). Each folder has a _KY-VONG.md listing the submission order and expected outcomes.

## Notes
- Only set 01 has been proven to auto-approve. Sets 03–14 and bat-loi use fictitious facilities, so AG-4 may raise additional facility signals.
- Sets 11, 13 and 14 belong to **TRẦN THỊ BÌNH**: this customer account is needed on MyInsurance (or change the name in _cases.py and redraw the images).
- Sets 10–14 and bat-loi keep the original dates on the documents (mostly September 2026; L08 deliberately uses 20/10); any set that has already been submitted to the tenant will be flagged as a duplicate by AG-4 if submitted again.
- L11 must be submitted AFTER set 10 (it is a duplicate of set 10).
- Every submission calls the real IDP (consumes quota, takes 30 seconds to a few minutes) and creates a new claim.
- Claims available to open during the demo: 0449 (auto-approved), 0451 (duplicate), 0440 (customer result), 0001 (AG-3 proposal), 0445 (waiting period).
- Log in to both apps again before the demo.
