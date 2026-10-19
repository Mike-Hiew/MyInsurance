# Test tầng IDP sau 4 batch Mentor — thang 4 mức, 20 case

**Ngày:** 22/09/2026 · **Đối tượng:** `CLM_IDP_Int` trên tenant **177**, sau khi Mentor chạy xong batch 1–4.
**Tenant 177 hết hạn 31/10/2026.** Quota tính **theo trang**, `blockLimit 1`, trần thật chưa biết → mọi thiết kế dưới đây tối ưu cho **gọi thật ít nhất có thể**.

---

## 1. Ba thứ khác nhau, hay bị gộp làm một

| Câu hỏi | Công cụ | Không dùng được gì |
|---|---|---|
| **Mentor có dựng đúng thứ mình bảo không?** (đủ action, đúng tên, đúng kiểu) | **MCP read-only** — `context_entities`, `context_search`, `context_graph`, `app_refs` | Không nói được code có *chạy đúng* không |
| **Gọi API thật có ra kết quả đúng không?** | **Debugger của ODC Studio** + action `IdpPing` / `IdpSelfTest` | MCP không có tool chạy Server Action |
| **Lần sau sửa có vỡ cái cũ không?** | **Bộ test `T-IDP-*`** chạy trên JSON đã ghi | Mentor **regress lặng lẽ** — đã dính ở rev 41→44 |

> **Mentor không phải công cụ test.** `mentor-copilot` có task *test generation* và *quality review*, nhưng cái nó sinh ra vẫn là test **phải chạy** ở đâu đó. Đừng nhầm "Mentor báo xong" với "đã kiểm".

---

## 2. Thang 4 mức — chạy theo đúng thứ tự này

| Mức | Tên | Chạy bằng | Tốn quota | Bắt được lỗi gì |
|---|---|---|---|---|
| **L0** | Kiểm cấu trúc | MCP `context_*`, hoặc mắt trong Studio | 0 | Mentor thiếu action, sai tên, sai kiểu, tự đẻ Entity |
| **L1** | Parser offline | Debugger + JSON đã ghi trong `IdpReplay` | **0** | Toàn bộ logic chuẩn hoá: confidence, ngày, tiền, `{}`, `templates_data` rỗng |
| **L2** | Một lời gọi thật / endpoint | Debugger, tenant 177 | ~8–10 trang | Header, auth, shape response đổi, quirk mạng |
| **L3** | End-to-end 1 hồ sơ | Debugger + màn thật | ~2–3 trang | Ghép nối, Timer, trạng thái |

**Nguyên tắc xuyên suốt: ghi một lần, test nhiều lần.** Mỗi lời gọi thật ở L2 phải **ghi response vào `IdpReplay`** ngay lần đầu. Sau đó mọi lần chạy lại là L1 — miễn phí, offline, chạy được cả khi mất mạng ngày pitch.

Đây cũng chính là lý do `ReplayMode` (`trang-thai-build` §3 #7) không nên để cuối: **nó là hạ tầng test, không chỉ là đường lùi.** Làm nó trước L2, không phải sau.

---

## 3. 20 test case

Cột **Quota** = số trang IDP tiêu tốn. `0` nghĩa là chạy được vô hạn lần.

### Batch 1 — auth

| # | Case | Mức | Cách chạy | Quota | Kỳ vọng |
|---|---|---|---|---|---|
| T-IDP-01 | Login thành công | L2 | `IdpPing()` | 0 | HTTP **201**, token ~340 ký tự, `Report` cuối là `OK` |
| T-IDP-02 | Vai đúng | L2 | `IdpRefreshIdentity()` | 0 | `RoleId = 2`, `TenantId = 177` |
| T-IDP-03 | **Thiếu User-Agent → 403** | L2 | Tạm đổi Setting `IdpUserAgent` thành rỗng, gọi `IdpPing()` | 0 | **403** kèm `error code: 1010`. Đây là test chứng minh header thật sự có tác dụng — không có nó, người sau sẽ xoá header vì "không thấy dùng ở đâu" |
| T-IDP-04 | **401 → tự login lại** | L2 | Sửa tay `IdpSession.AccessToken` thành chuỗi rác, gọi `IdpPing()` | 0 | Một vòng re-login, trả `OK`. **Không** lặp vô hạn |
| T-IDP-05 | Cache token | L2 | Gọi `IdpGetToken(False)` hai lần liên tiếp | 0 | Lần hai **không** phát sinh lời gọi mạng |

> T-IDP-04 dùng mẹo sửa token thay vì gọi `/auth/logout`, vì logout giết token thật và đội chỉ có một tài khoản.

### Batch 2 — trích xuất

| # | Case | Mức | Cách chạy | Quota | Kỳ vọng |
|---|---|---|---|---|---|
| T-IDP-06 | `templates` gửi dạng mảng | L2 | Tạm sửa code gửi array | 0 | `400 invalid input syntax for type bigint: "NaN"` — xong thì hoàn nguyên |
| T-IDP-07 | Nộp sync 1 trang | L2 | `IdpSubmitSync` với ảnh giả lập | **1** | Completed ~13–17 s, phân loại **6091**, parse ra field + bảng. **Ghi response vào `IdpReplay`** |
| T-IDP-08 | **`templates_data` rỗng** | L1 | JSON đã ghi 17/09 | **0** | `ErrorCode = TEMPLATE_NOT_ASSIGNED`, outcome **FAILED**. Đừng chạy lại thật — đã chứng minh rồi và nó **vẫn mất quota** |
| T-IDP-09 | Confidence loại field rỗng | L1 | JSON có 8/14 field `v: ""` cùng `c: 1` | 0 | `ClaimConfidence` = **min** trên field bắt buộc **có giá trị**, **không** ra 0,989. `Completeness` là số **riêng** |
| T-IDP-10 | Hai thang confidence | L1 | JSON có `c: 0.9` và `data.confidence: 98.90` và `"97.81"` | 0 | Tất cả về `0..1`. Chuỗi được ép kiểu |
| T-IDP-11 | Ngày hai định dạng | L1 | JSON có `"20/03/2025"` và `"1973-10-09"` | 0 | Cả hai ra `Date` đúng |
| T-IDP-12 | Tiền có dấu phân cách | L1 | Bảng kê `150000` / `4.500.000` | 0 | Số nguyên VND, không thập phân. **Đối chiếu với `get-ocr`** — template không có quy tắc đã từng đọc `300.000` thành `300` |
| T-IDP-13 | Giá trị giữ chỗ | L1 | JSON có `""`, `"None"`, `"null"`, `"00000"` | 0 | Cả bốn → `null` |
| T-IDP-14 | Ba shape một parser | L1 | 3 JSON: `/sync` (2 lớp), `/result` (1 lớp), `/check-status` (`data.result`) | 0 | Cùng ra một `IdpExtractionResult`. Grep: `IdpParseExtraction` tồn tại **đúng một bản** |
| T-IDP-15 | Đường bất đồng bộ | L2 | `IdpCreateGroup` → `IdpSubmitDocument` → poll | **1** | Nhóm chuyển `Sending → Processing → Completed` |
| T-IDP-16 | `raw-file` là ZIP | L2 | `IdpGetRawFile` | 0 | `application/zip`, không phải PDF/ảnh. Giải nén ra đúng file gốc |
| T-IDP-17 | **Timeout 60 s** | L2 | `IdpSubmitSync` với file 2 trang | **2** | Chạy tới cùng. Nếu đứt ở ~10 s thì `ServerRequestTimeout` **chưa** được nâng |

### Batch 3 — đánh giá

| # | Case | Mức | Cách chạy | Quota | Kỳ vọng |
|---|---|---|---|---|---|
| T-IDP-18 | Chặn OCR chưa xong | L1 | Truyền `ocrRequestIds` của một request `Failed` | 0 | **Phía mình** chặn. IDP không kiểm gì — nó sẽ nhận |
| T-IDP-19 | `run-workflow` 400 → retry | L2 | `IdpStartAssessment` | 0 | Log có ≥ 1 lần thử lại rồi 200. Nếu lần đầu 200 luôn thì ép test bằng cách gọi `run` trước khi request sẵn sàng |
| T-IDP-20 | `{}` là HỎNG | L1 | JSON `{}` của flow `8d873709` | 0 | `State = ERROR`. **Không bao giờ** `COMPLETE` với 0 mục |
| T-IDP-21 | Poll tới cùng | L3 | Timer, hồ sơ thật | 0 | `status 3`, 20 mục. Đo thời gian — 17/09 là **72 s**, 03/09 là ~30 s |
| T-IDP-22 | Trần 15 phút | L1 | Ép `IdpPollMaxMinutes = 1` | 0 | `State = PARTIAL` + cảnh báo `CLM-CONSOLE-015`, **không** poll vĩnh viễn |
| T-IDP-23 | Không tin `HospitalCode` của flow | L1 | JSON 17/09 (hoá đơn không có bệnh viện, flow trả `01001`) | 0 | Giá trị này **không** được ghi vào entity nào |

### Batch 4 — master data

| # | Case | Mức | Cách chạy | Quota | Kỳ vọng |
|---|---|---|---|---|---|
| T-IDP-24 | Document set 198 | L2 | `IdpGetTemplateCsv()` | 0 | 9 id ghép phẩy, `RequestedCount = ReturnedCount = 9` |
| T-IDP-25 | Bộ teo âm thầm | L1 | JSON giả thiếu 2 id | 0 | Chênh lệch **lộ ra ngoài**, không nuốt |
| T-IDP-26 | `hospital_code` rỗng | L1 | JSON có row `hospital_code: ""` | 0 | → `null`, không phải `""` |
| T-IDP-27 | Dòng trùng | L1 | JSON có 2 row cùng tên cùng mã | 0 | Lấy theo `score` cao nhất, không vỡ |

### Xuyên suốt

| # | Case | Mức | Kỳ vọng |
|---|---|---|---|
| T-IDP-28 | `ReplayMode = On` | L1 | **Không một lời gọi mạng nào**. Tắt wifi chạy lại vẫn xanh. `AgentRun.Replay = true` |
| T-IDP-29 | Không có `PUT /request/{id}/result` | L0 | Grep toàn module: 0 kết quả |
| T-IDP-30 | Lỗi IDP không thành DECLINED | L1 | Mọi outcome FAILED → `EXTRACTION_ERROR` vào hàng đợi operator |

---

## 4. Ngân sách quota

| Hạng mục | Trang |
|---|---|
| T-IDP-07 sync 1 trang | 1 |
| T-IDP-15 đường bất đồng bộ | 1 |
| T-IDP-17 file 2 trang | 2 |
| L3 end-to-end 1 hồ sơ | 2–3 |
| Dự phòng chạy lại | 3 |
| **Tổng** | **~10 trang** |

Ngày 17/09 đã tiêu 4 trang. Cộng lại vẫn nhỏ, nhưng **trần thật chưa ai xác nhận** — theo dõi bằng `GET /usage/current` trước và sau đợt test, và nếu `pageUsed` nhảy bất thường thì dừng, vì hết quota là **mọi request OCR dừng với HTTP 200** kèm `1014 Partner is expired`, nhìn không giống hết quota chút nào.

---

## 5. MCP kiểm được gì — và không kiểm được gì

**Kiểm được (rẻ, nên chạy sau mỗi batch):**

```
context_entities  app:<CLM_IDP_Int>  owned_only:false  limit:100
```
→ đủ 2 Entity `IdpSession`, `IdpFieldMap`, **không có Entity lạ nào Mentor tự đẻ**. Nhớ `owned_only:false`, nếu không entity kế thừa biến mất và dễ tạo trùng; và `limit:100` vì mặc định chỉ **20**, đọc cụt mà không biết.

```
context_search  objects:["ServerActions"]
```
→ đủ 15 Server Action theo tên đã khai, `IdpParseExtraction` **đúng một bản**.

**Không kiểm được:** giá trị trả về, header thật gửi đi, timeout, thứ tự gọi, wiring của REST method. `app_refs` còn không hiện cả `AIModelConnection` — muốn chắc thì mở Studio nhìn.

---

## 6. Bốn cái bẫy đã ăn dự án này rồi

1. **Rule 11 — test xanh vì fixture trùng nhau trông giống hệt test đúng.** Nếu JSON trong `IdpReplay` dùng chung một file mẫu cho cả 3 shape thì T-IDP-14 sẽ xanh mà parser vẫn sai. Ba shape phải là **ba file khác nhau, chụp từ ba endpoint khác nhau**.
2. **Rule 13 — đếm trước và đếm sau phải nằm trong cùng một lượt.** Sửa parser xong chạy lại **cả 30 case**, không suy ra từ case vừa sửa.
3. **Rule 14 — "saved revision 943" không phải "published rev 163".** Mentor sửa model chứ không deploy. Test trên bản chưa publish là test một thứ không tồn tại trên tenant.
4. **Mentor regress lặng lẽ.** Màn Checkpoint hỏng ở rev 41, phát hiện ở rev 44. Chạy lại bộ L1 sau **mỗi** batch là việc rẻ nhất để bắt chuyện này — L1 không tốn quota, không tốn mạng, chạy hết trong vài giây.

---

## 7. Thứ tự làm, nếu hôm nay bắt đầu

1. Batch 1 → L0 + T-IDP-01…05 *(0 quota)*
2. **Dựng `IdpReplay` + `ReplayMode` ngay tại đây** — trước batch 2, không phải sau batch 4
3. Batch 2 → L0 → T-IDP-07 gọi thật **1 lần, ghi lại** → T-IDP-08…14 chạy offline
4. Batch 3 → L0 → T-IDP-19, 21 gọi thật → T-IDP-18, 20, 22, 23 offline
5. Batch 4 → L0 → T-IDP-24 gọi thật → còn lại offline
6. L3 end-to-end 1 hồ sơ `CLM-2026-0001`
7. Tắt wifi, chạy lại toàn bộ với `ReplayMode = On` → đây là **bài diễn tập ngày pitch**, không phải test thừa
