# Change — Gọi IDP thật (tenant 177) + ReplayMode

**Trạng thái:** 🔴 chưa bắt đầu · **Đường găng** · **Hạn:** 27–29/09
**Nguồn:** Sổ tay API BnK IDP (chạy thật 17/09/2026) · delta ở `./specs/idp-integration/spec.md`
**Chặn:** AG-2 (cần dữ liệu thật) · số accuracy cho phần ROI của deck

## Trạng thái phía IDP — 21/09

- ✅ **Ghim flow: ĐÃ CÓ MAIL.** IDP cam kết **stable đến 08/10**, gồm flow `5ed8e4fc`, template 6090–6098 và bộ 198 trên tenant 177.
- 🟡 **Quota: ĐÃ GỬI MAIL 22/09, chờ trả lời.** Tenant 177 đang `blockLimit 1`, **đã dùng 14 trang**. Không ai biết một block bằng bao nhiêu trang. Demo 3 hồ sơ × 9 chứng từ = **27 trang một lượt**, chưa kể chạy thử và quay video.
  **Đây là rủi ro duy nhất có thể làm demo đứng giữa chừng, và "stable" không bao gồm nó.** Phải hỏi riêng: một block = bao nhiêu trang · nâng tenant 177 lên **tối thiểu 300 trang** tới 31/10 · hết quota thì API báo mã gì (đang giả định 1003).
- 🟡 Ba trường master data trả giá trị giữ chỗ — **không chặn**, ta tự tra bảng 322.
- 🟡 Không có đánh giá theo dòng — **không chặn**, Track 2 trình bày là chế độ tư vấn.

## Việc

- [ ] **1. Sửa `build-00` §1 và §4 #5 tại nguồn** — tenant **177** · user **204** · bộ **198 `HEALTH_CLAIM_OS_V1`** · template **6090–6098**. Bản cũ ghi 176/203/194. Ba con số này đi vào Site Setting; sai là hồ sơ trôi qua im lặng. Đồng thời gỡ §4 #4 (`T1-03` chưa có template — đã tự dựng 6094)
- [ ] **2. Entity `IdpSession`** — cache 1 dòng token, TTL giả định 30 phút. Không refresh token: 401 ⇒ đăng nhập lại, không retry mù
- [ ] **3. REST Consume `POST /auth/login` → `GET /user/me`** — **set `User-Agent` kiểu trình duyệt TRƯỚC khi test method thứ hai**, nếu không Cloudflare 403/1010
- [ ] **4. Adapter nộp chứng từ** — `/request/group` → `/request` (lặp) → poll nhóm 3→5→10→30 s. `templates` gửi **chuỗi CSV**, không phải mảng
- [ ] **5. Đọc kết quả + ghi `RawExtractionJson` NGAY** — `templates_data[].data.tables[0].`**`data`** (không phải `rows`). Kèm `/raw-file` giữ bản gốc trước khi MinIO xoá
- [ ] **6. Chặn 10 bẫy ở §3 của spec** — mỗi bẫy một kiểm thử nhỏ, đặc biệt bẫy 1 (`templates_data` rỗng ⇒ `EXTRACTION_ERROR`) và bẫy 2 (đọc `statusCode` trong body trước)
- [ ] **7. Phiên đánh giá lâm sàng** — `PUT /workflow/request` → `GET /run-workflow-request/{id}` **retry 3×5 s** (ngoại lệ 4xx duy nhất) → poll tới status 3, **trần ≥ 3 phút** → `/result/json`
- [ ] **8. Tra bệnh viện qua bảng 322** (OpenSearch), **không** dùng `HospitalCode`/`PlaceOfTreatment`/`SurgeryCodes` từ flow
- [ ] **9. `IdpReplay` + `ReplayMode`** — ghi JSON của một lần chạy thật, bật cờ để demo chạy offline. **Đường lùi khi mạng hỏng ngày pitch**
- [ ] **10. Màn hình chờ** — một hồ sơ 9 chứng từ mất ~3 phút, không được đứng im
- [ ] **11. `POST /request/compare`** với 10–20 chứng từ **đã che PII** → con số accuracy cho deck. Việc đáng giá nhất cho phần ROI
- [x] **12. Hỏi IDP về quota — ĐÃ GỬI 22/09.**
- [ ] **12b. Đường lui nếu quota không được nâng (quyết trước 26/09, không chờ vô hạn):** (1) demo chỉ chạy IDP thật trên **một** hồ sơ, không phải ba — 9 trang thay vì 27; (2) hai hồ sơ còn lại chạy `ReplayMode` từ JSON đã ghi; (3) **tuyệt đối không chạy thử IDP thật trong tuần rehearsal 30/09–06/10** — mỗi lần chạy thử là 9 trang bị trừ vĩnh viễn; (4) ghi lại số trang đã dùng sau **mỗi** lần gọi thật, bằng `GET /usage/current`. Không có (4) thì ba cái trên vô nghĩa vì không ai biết còn bao nhiêu
- [ ] **13. Merge vào Project doc `build-02`** rồi archive change

## Xong nghĩa là

- Một hồ sơ thật đi hết 11 bước trên tenant 177, kết quả ghi vào `RawExtractionJson`
- `ReplayMode` bật lên thì demo chạy được **không cần mạng**
- Có một con số accuracy đo bằng `compare`, không phải con số MBAL cũ
- Mười bẫy đều có kiểm thử, không cái nào chỉ nằm trong tài liệu
