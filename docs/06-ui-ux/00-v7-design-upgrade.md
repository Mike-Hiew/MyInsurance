# UI v7 — Design upgrade spec (chờ chốt để implement)

- **Ngày:** 03/10/2026
- **Bản:** 2. Bỏ mọi số minh hoạ và giả định; chỉ trình bày lại dữ liệu hệ thống đang có.
- **Mockup:** `Prototype_Insurance_BnK_v0.6.html`. Đây là v0.5 (Prototype v6.1) cộng một lớp v7 chỉ bọc lên trên, không xoá màn nào.
- **App:**
  - ClaimMVP: console cán bộ, phiên ClaimMVP làm.
  - MyInsurance: portal khách, phiên Cowork làm.

## Nguyên tắc số 1 — chỉ enhance UI, không bịa

Hệ thống đã chạy thật. v7 chỉ **trình bày lại** dữ liệu đang có sao cho dễ hiểu và ấn tượng hơn.

- **Không** số minh hoạ, **không** số ROI/giả định, **không** sparkline xu hướng tự đặt.
- **Không** câu hứa thời gian ("có kết quả trong hôm nay").
- **Không** nút cho chức năng chưa có, ví dụ "Thư kết quả" khi chưa có ClaimLetter.
- Mỗi phần tử trên màn phải chỉ ra được **nguồn dữ liệu đang tồn tại**. Nếu nguồn chưa có, phần tử **ẩn đi**, không hiện số giả.
- Không đụng logic: chuỗi 14 bước, cổng C1–C4, AG-2, trợ lý khách giữ nguyên.

Mockup dùng dữ liệu mẫu A/B/C/D của prototype. Trên ODC, mọi giá trị lấy từ nguồn ghi trong cột "Nguồn".

---

## 0. Cách xem mockup

1. Mở HTML bằng Chrome và đăng nhập (cán bộ hoặc khách).
2. Bấm nút tím **"Kịch bản demo"** cạnh sidebar.
   - Có 9 bước, chỉ đi qua các màn đã có trên ODC.
   - Phím `←` / `→` để chuyển bước, `Esc` để thoát.
3. Thử thêm VI/EN và chế độ tối.

Code lớp v7 nằm trong file, tìm khối `/* ===== v7 design upgrade ===== */`.

---

## 1. Quy tắc trình bày chung

| # | Quy tắc |
|---|---|
| P1 | **Tím = AI, teal đậm = người.** Hai màu này không dùng cho việc khác. |
| P2 | Không lộ mã nội bộ trên UI nghiệp vụ, ví dụ `UnderReview`, `F58`, `TERM-HC-002`, `[BR-…]`. Mã chỉ nằm trong khối "Chi tiết kỹ thuật" thu gọn. |
| P3 | Song ngữ theo ngôn ngữ đang chọn. Tên thuốc, cơ sở KCB và tên người giữ tiếng Việt. |
| P4 | Animation ≤ 600 ms, chỉ chạy lần đầu, tôn trọng `prefers-reduced-motion`. |
| P5 | Class mới có tiền tố `v7-`. Selector SVG giới hạn bằng `>`. Theme gốc có `.bar{width:100%}`, quy tắc này đã làm vỡ biểu đồ thác trong mockup. |

**Design tokens** — đặt vào Theme → Style Sheet của cả hai app:

```css
:root{
  --ai:#6E56CF; --ai-2:#9F7AEA; --ai-tint:#F1EDFD; --ai-border:#D9CFFB; --ai-ink:#3E2E8C;
  --ai-grad:linear-gradient(135deg,#6E56CF 0%,#9F7AEA 100%);
  --human:#0C3036; --mint:#35D3A3;
}
[data-theme="dark"]{ --ai-tint:#2A2346; --ai-border:#4B3F86; --ai-ink:#CFC4FF; }
```

---

## 2. Danh sách hạng mục

| ID | Tên | App | Màn ODC | Nguồn dữ liệu (đã có) | Lượt Mentor | Ưu tiên |
|---|---|---|---|---|---|---|
| D1 | Chip AI / Người | Cả hai | Theme + chỗ có đề xuất AI | `ClinicalRecommendation.Status` | 1 | P0 |
| D2 | Thẻ "AI đã làm gì cho hồ sơ này" | ClaimMVP | ClaimDetail, trên thanh tab | ClaimDocument, ClaimLine, ClinicalRecommendation, ChainRun, GateResult | 2 | P0 |
| D3 | Biểu đồ thác tiền | ClaimMVP | ClaimDetail › Calculation & ledger | `ChainRun.StepOutputsJson` | 2 | P0 |
| D4 | Chip IDP + hiệu ứng quét ở tab trích xuất | ClaimMVP | ClaimDetail › Extraction & costs | ClaimDocument (IDP) | 1 | P1 |
| D5 | "Biên nhận" kết quả cho khách | MyInsurance | PortalClaimDetail (web + 390 px) | ChainRun tổng tiền + dữ liệu khối "Vì sao tự chịu" hiện có | 2 | P0 |
| D6 | Lời chào theo trạng thái | MyInsurance | Home | Claim mới nhất + Member | 1 | P1 |
| D7 | Dọn nhãn (tuần, trạng thái) | ClaimMVP | Dashboard, Worklist | Dữ liệu biểu đồ hiện có | 1 | P1 |
| D8 | Kịch bản demo 9 bước | ClaimMVP | Layout | Không cần dữ liệu mới | 2 / 0 nếu dùng cue card | P2 |
| D9 | Chuyển động | Cả hai | Theme | — | 1 | P2 |
| D10 | Nút chat có nhãn + đồng hồ chờ thật | MyInsurance | Layout (FAB) | — | 0,5 | P1 |

**Đã bỏ khỏi bản 1:**

- Dải "tác động" với 82% / 41% / 4,2 h / 650 h giờ công.
- Sparkline.
- Phễu hành trình với số minh hoạ.
- Chỉ số "~35 phút nếu gõ tay".
- Bước "Đối chiếu chéo AG-3", vì chưa có CrossCheckResult.
- Chữ "AG-5" trên đề xuất y khoa, vì agent này chưa có.
- Nút "Thư kết quả".
- Số tài khoản ngân hàng viết cứng.
- Câu "có kết quả trong hôm nay".
- Bước kịch bản "ROI".
- Câu chờ chat giả lập tiến trình.

---

## 3. Chi tiết

### D1 — Chip AI / Người

- `AiChip(Text)`: viên thuốc gradient tím, icon lấp lánh. Dùng cho mọi khối do IDP assessment hoặc agent sinh ra.
- `HumanChip(Text)`: nền teal nhạt, icon người. Dùng cho trạng thái quyết định của cán bộ.
- `ai-surface`: nền cho recommendation card và panel AG-2.
- `human-row`: dải ngay trên nút Xác nhận / Bác bỏ, với câu "Bạn quyết định — số tiền chỉ đổi khi bạn xác nhận".
- **ODC:** 2 Block dùng lại. CSS nằm trong Theme.
- **Chấp nhận:** mọi đề xuất AI có chip tím; mọi nút quyết định có `human-row`.

### D2 — Thẻ "AI đã làm gì cho hồ sơ này" (ClaimMVP)

**Mockup:** `v7AiSteps`, `v7AiCard`.

Card tím đặt giữa header hồ sơ và thanh tab, gồm:

- Chip tiêu đề.
- Câu "Máy đọc và tính, người quyết phần cần phán đoán. Agent không ghi tiền."
- Nút **"Hỏi AG-2 về hồ sơ"**: cuộn tới panel AG-2 hiện có và đặt con trỏ vào ô hỏi.
- 5 bước, mỗi bước chỉ hiển thị khi có dữ liệu:

| # | Bước | Ai làm | Kết quả hiển thị | Nguồn |
|---|---|---|---|---|
| 1 | Đọc chứng từ | BnK IDP | "N chứng từ · M dòng chi phí" (+ thời gian đọc nếu có) | Count ClaimDocument; count ClaimLine; thời gian = processingTime IDP **nếu đang lưu**, không có thì bỏ |
| 2 | Kiểm tra y khoa | BnK IDP · luồng thẩm định | "Đề xuất cho dòng N" / "Không có đề xuất" / "PARTIAL" | ClinicalRecommendation; trạng thái assessment |
| 3 | Chuỗi 14 bước | Hệ thống · AG-2 giải thích | Số đề xuất, hoặc "Dừng ở bước X · lý do" | ChainRun mới nhất |
| 4 | Cổng tự duyệt | Hệ thống | "x/4 điều kiện đạt" | GateResult của run mới nhất |
| 5 | Quyết định | Cán bộ thẩm định / "Không cần người" khi 4/4 | "Chờ bạn quyết dòng N" → "Đã xác nhận dòng N" / "Đã bác bỏ dòng N" | ClinicalRecommendation.Status |

- Khi phiên ClaimMVP làm xong P1 (Submit/Approve), bước 5 đọc thêm trạng thái Claim: "Đã trình phê duyệt" / "Đã duyệt" / "Đã chi trả". Trước đó thì không hiển thị các trạng thái này.
- **ODC:**
  - Block `AiWorkCard(ClaimId)`.
  - Server action `GetAiWorkSummary(ClaimId, Lang)`, chỉ đọc, trả List `{Title, Who, Outcome, Duration, State, IsHuman}`.
  - Tham số `Lang` truyền vào, vì client var không đọc được ở server.
  - Dưới 1100 px, các bước xếp dọc.
- **Chấp nhận:**
  - [ ] Mọi chữ và số trên thẻ truy được về bảng nguồn.
  - [ ] Hồ sơ không có đề xuất thì bước 2 ghi "Không có đề xuất".
  - [ ] CLM-2026-0005: bước 4 "4/4", bước 5 "Không cần người".

### D3 — Biểu đồ thác tiền (ClaimMVP)

**Mockup:** `v7Waterfall`. `paneCalc` được bọc: biểu đồ thác ở trên, bảng 14 bước gốc thu vào "Xem chuỗi 14 bước & sổ hạn mức (chi tiết kỹ thuật)".

- **Header:** "Số tiền được tính ra sao" và số đề xuất to.
  - Có đề xuất đang mở: chip "Đang chờ bạn quyết dòng N". **Không dự đoán số mới.**
  - Nếu có lần chạy trước: chip "−X ₫ so với lần tính trước", với X = `TotalPayable` (run trước) − `TotalPayable` (run này).
- **Cột:**
  - Cột xám "Khách yêu cầu".
  - Một cột đỏ cho mỗi bước có `output ≠ input`.
  - Cột xanh "Chi trả".
- **Thẻ lý do** dưới biểu đồ, mỗi bước trừ tiền một thẻ, câu lấy từ `explainVi` / `explainEn` của bước. Dòng có đề xuất AI thì thêm `AiChip` và `HumanChip` theo trạng thái.
- **Ví dụ thật (CLM-2026-0005):** 1.400.000 → −280.000 đồng chi trả 20% → 1.120.000.
- **ODC:**
  - Block `PayoutWaterfall(ClaimId, Lang)`.
  - Cột vẽ bằng Container `position:absolute` với `%` chiều cao, không cần SVG.
  - Server action `GetWaterfall` đọc `StepOutputsJson` của run mới nhất.
  - Bảng 14 bước hiện có bọc trong khối thu gọn, mặc định đóng.
- **Chấp nhận:**
  - [ ] Tổng các cột đỏ = Requested − Payable, đúng từng đồng.
  - [ ] Không số nào viết cứng.
  - [ ] EN không lẫn VI. Dark mode đọc được.

### D4 — Tab Extraction & costs (ClaimMVP)

- Cạnh tiêu đề "Dữ liệu trích xuất" có `AiChip` "Đọc bởi BnK IDP" (+ thời gian nếu có lưu).
- Recommendation card dùng nền `ai-surface`, chip "AI đề xuất · BnK IDP". `human-row` đặt trên hai nút quyết định.
- Vệt quét tím chạy 1,2 s, chỉ **lần đầu** mở tab của mỗi hồ sơ. Dùng biến local trên screen, **không** dùng Client Variable.
- **Chấp nhận:**
  - [ ] Vệt quét không chạy lại khi đổi tab.
  - [ ] Không giật layout.

### D5 — "Biên nhận" kết quả cho khách (MyInsurance)

**Mockup:** `v7Receipt`, `v7Donut`, `v7Why`. Bản mobile: `MOB.claimdetail`.

1. **Đầu teal đậm:**
   - Cơ sở KCB · số hồ sơ.
   - Số tiền to.
   - Câu trạng thái theo bảng dưới.

   | Trạng thái | Câu |
   |---|---|
   | Đã chi trả | "đã chi trả" |
   | Có khoản đang xem | "số tiền dự kiến · cán bộ đang xem một khoản" |
   | Từ chối | "không được chi trả lần này" |
   | Từ chối chưa quyết | "dự kiến không được chi trả · cán bộ đang xác nhận" |

   Chỉ hiện số tài khoản (che) khi Member đã có trường payout. Phiên ClaimMVP đang thêm trường này ở P1.
2. **Donut + chú giải:** Bảo hiểm chi trả / Quý khách tự chịu / Tổng chi phí, lấy từ tổng ChainRun mà portal đang hiển thị. Khi 0 % thì không vẽ cung.
3. **Thanh 4 trạng thái** cho khách. Đã có, chỉ đổi giao diện.
4. **Nút:**
   - "Bổ sung thông tin": màn Supplement đã có.
   - **"Hỏi trợ lý về hồ sơ này"**: chat đã có, mở bằng JS click FAB như rev 76–80.
5. **Cột phải "Vì sao Quý khách tự chịu X ₫":**
   - Dùng **đúng dữ liệu khối "Vì sao" hiện có trên portal**, chỉ đổi giao diện thành thẻ có icon.
   - Câu đã duyệt, không dùng từ buộc tội.
   - Lúc đang thẩm định thì câu là "sẽ nhận thông báo khi có kết quả", không hứa thời gian.

- **ODC:**
  - Donut bằng CSS `conic-gradient`.
  - Mép răng cưa bằng `mask-image`.
  - Không tạo entity hay nguồn dữ liệu mới.
- **Phụ thuộc:** muốn thấy trạng thái "đã chi trả" trên demo cần một hồ sơ PAID. Có được hồ sơ này khi phiên ClaimMVP xong P1.
- **Chấp nhận:**
  - [ ] Số trên biên nhận = số trên màn hiện tại.
  - [ ] Tổng thẻ "Vì sao" = phần tự chịu.
  - [ ] Ở 390 px không tràn ngang.

### D6 — Lời chào (MyInsurance Home)

- Dải teal: "Chào anh/chị {tên}," kèm một câu theo trạng thái hồ sơ mới nhất và nút "Xem hồ sơ →".

  | Trạng thái | Câu |
  |---|---|
  | PAID | "Hồ sơ … đã được chi trả X ₫." |
  | Đang thẩm định | "Hồ sơ … đang được thẩm định." |
  | Khác | "Hồ sơ … đang được xử lý." |
  | Không có hồ sơ | Ẩn dải chào |

- **Nguồn:** `Member.Gender` và `FullName` (lấy từ cuối); Claim mới nhất.
- **Chấp nhận:** [ ] câu đúng trạng thái; EN "Hi {name},".

### D7 — Dọn nhãn (ClaimMVP)

- Không thêm khối mới trên Dashboard.
- Biểu đồ tuần: nhãn "T1…T9" → "Tuần 36…44" / "W36…44", tính từ ngày thật của dữ liệu.
- Chip trạng thái dùng nhãn người đọc được ("Under review", "Pending approval"…), không dùng mã (`UnderReview`, `ExtractionError`).
- **Chấp nhận:** [ ] không còn mã nội bộ trên Dashboard và Worklist.

### D8 — Kịch bản demo 9 bước

Chỉ đi qua các màn đã có:

| # | Màn | Khoanh | Câu nói |
|---|---|---|---|
| 1 | Portal mobile — Home | — | Khách chỉ chụp chứng từ, không gõ số |
| 2 | Portal — Tạo hồ sơ, bước đọc | màn | Chứng từ được BnK IDP đọc khi tải lên |
| 3 | Console — Hàng đợi | bảng | Hồ sơ vào hàng đợi đã có số tiền do chuỗi tính |
| 4 | Chi tiết hồ sơ | thẻ D2 | Năm bước, ai làm, ra gì — máy đọc và tính, người quyết |
| 5 | Extraction & costs | recommendation card | Kiểm tra y khoa đề xuất kèm bằng chứng; tiền chỉ đổi khi cán bộ xác nhận |
| 6 | Calculation & ledger | thác D3 | Loại 168.000 nhưng chi trả chỉ giảm 120.000 vì trần và đồng chi trả đã áp trước |
| 7 | Cổng tự duyệt | 4 điều kiện | Không đạt cổng thì chuyển người duyệt |
| 8 | Portal — Kết quả | biên nhận D5 | Bảo hiểm trả bao nhiêu, tự chịu bao nhiêu, vì sao |
| 9 | Technical trace | — | Mọi lượt agent / IDP ghi vào AgentRun |

- **Đề xuất:** **không build**. In thành cue card. Nếu PO muốn build thì làm Block `StoryBar` khi PresenterMode = True; các bước portal là link mở MyInsurance ở tab khác.
- Câu số 6 phải dùng số của hồ sơ demo thật hôm pitch.

### D9 — Chuyển động

- Fade màn 250 ms.
- Đếm số 700 ms.
- Cột thác mọc lần lượt.
- CSS trong Theme, bọc trong `prefers-reduced-motion:no-preference`.

### D10 — Nút chat (MyInsurance)

- Nhãn "Hỏi trợ lý" cạnh FAB; ẩn khi panel mở (biến local `IsChatOpen`); ở 390 px chỉ hiện icon.
- Lúc chờ agent (~40 s): bong bóng "Trợ lý đang xem hồ sơ… 12 s" với **đồng hồ đếm giây thật** (`setInterval` phía client). **Không** giả lập các bước tiến trình mà agent không báo về.

---

## 4. Phân công & thứ tự

| Phiên | Hạng mục | Thứ tự |
|---|---|---|
| ClaimMVP | D1 (console), D2, D3, D4, D7, D9 (console) | D1 → D2 → D3 → D4 → D7 → D9 |
| MyInsurance (Cowork) | D1 (portal), D5, D6, D10, D9 (portal) | D1 → D5 → D10 → D6 → D9 |
| PO | D8 cue card | — |

**Quy tắc:**

- Trước mỗi publish, so `app_revisions` với rev đã load.
- Mỗi hạng mục một lượt Mentor và kiểm trên màn hình, vì Mentor hay báo đã làm những việc nó chưa lưu.
- Chỉ publish Dev.
- Không sửa Portal Settings.

**Mẫu prompt Mentor:**

```
App <ClaimMVP|MyInsurance>. Chỉ thay đổi lớp trình bày, KHÔNG đổi logic, entity, action ghi dữ liệu.
Hạng mục <Dx> — <dán mục tương ứng>.
Chỉ đọc từ nguồn: <bảng/action đã có>. Không thêm số, câu hay nút nào không có nguồn.
Nếu nguồn không có trường X thì ẩn phần tử đó, không hiện giá trị mặc định.
CSS trong Theme, class tiền tố v7-. Chuỗi qua hàm dịch theo Lang.
Xong thì liệt kê widget/action đã tạo và chỗ đặt.
```

## 5. Kiểm thử sau khi implement

- [ ] Các hồ sơ demo (0001, 0005 và hồ sơ PAID nếu có) × VI/EN × sáng/tối: không lỗi, không lẫn ngôn ngữ, không lộ mã.
- [ ] Mọi số trên D2/D3/D5 khớp ChainRun và màn hiện tại, đúng từng đồng.
- [ ] Không còn chữ "minh hoạ", số giả định hay nút không có chức năng.
- [ ] Portal ở 390 px: không tràn ngang, FAB không che số tiền.

## 6. Còn cần PO chốt

1. D8: cue card hay build StoryBar? Đề xuất: cue card.
2. Tên khách demo: mockup dùng "Nguyễn Thị Mai", portal thật dùng "Nguyễn Văn An". Thống nhất một tên trước khi quay video.
