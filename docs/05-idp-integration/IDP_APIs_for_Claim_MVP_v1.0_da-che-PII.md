> **Bản đã che dữ liệu cá nhân.** Tên người được bảo hiểm, ngày sinh và số thẻ BHYT trong các payload mẫu đã được thay bằng giá trị giả. Bản gốc chưa che nằm ở `insurance BNK/IDP_API_Spec/` — **không đưa bản gốc lên tenant demo, không chiếu, không gửi ra ngoài công ty**.

# API của IDP cho hệ thống Claim MVP

**Môi trường:**

| Hệ | Base URL |
|---|---|
| FO | `https://idp-api.idpstg.bnksolution.com` |

**Hai vai:** `Partner = 1` — gọi tắt **FO Admin**; `Tenant = 2` — gọi tắt **FO End User**. Cột *"Ai gọi được"* trong tài liệu này **không suy từ swagger** mà là kết quả gọi thật bằng cả hai tài khoản.

---

# PHẦN 1 — YÊU CẦU CỦA CLAIM PLATFORM, VÀ MỨC ĐỘ IDP ĐÁP ỨNG

## Bảng đối chiếu — yêu cầu của Claim Platform với IDP

Mỗi dòng là **một yêu cầu rút từ tài liệu ClaimMVP**, kèm mức độ IDP đáp ứng, API liên quan, và ghi chú.

| Mức | Nghĩa |
|---|---|
| ✅ | **Đáp ứng** — dùng được đúng như tài liệu mô tả |
| ⚠️ | **Một phần** — có đường đi nhưng lệch, adapter phải xử lý thêm |
| ❌ | **Không đáp ứng** — không có đường đi, hoặc có nhưng hỏng |
| ❔ | **Chưa kiểm chứng** — chưa có dữ liệu để kết luận |

**Cột giải pháp dùng bốn nhãn:** **`ADAPTER`** vá được ở phía mình · **`XIN IDP`** cần IDP sửa hoặc bổ sung · **`CẤU HÌNH`** chỉ cần nạp dữ liệu, không phải viết code · **`HOÃN`** bỏ khỏi MVP 1 và ghi lại lý do.

### A · Xác thực và vận hành

| # | Yêu cầu của Claim Platform | Mức | API liên quan | Thực trạng | Giải pháp |
|---|---|---|---|---|---|
| A1 | Tích hợp server-to-server bằng API key hoặc OAuth2 client-credentials — *D3 §1.2* | ⚠️ | `POST /auth/login` | Chỉ có đăng nhập bằng tài khoản người dùng. Không thấy refresh token, TTL chưa rõ. Tài khoản tích hợp **bắt buộc vai Tenant** — vai Partner bị `403` trên toàn bộ `/request/*` | **ADAPTER + XIN IDP** — tạo một **Tenant riêng cho tích hợp**, adapter cache token và refresh trước hạn. Song song xin IDP cấp service account thật |
| A2 | Một correlation id xuyên suốt ba hệ thống — *D3 G-5* | ❌ | — | Không header nào mang được. Chẩn đoán sự cố phía IDP phải đưa họ timestamp và `name` | **ADAPTER** — nhét `claimNo/eventNo` vào trường `name` theo quy ước cố định, và ghi correlation id vào log adapter ở cả hai chiều |
| A3 | Circuit breaker và biết rate limit của đối tác — *D3 G-6* | ⚠️ | — | **Đọc được từ code**: FO bật `ThrottlerModule` với mặc định `ttl 60000ms` và `limit 100` — tức **100 request mỗi 60 giây**, và `RequestController` có gắn `ThrottlerGuard`. Hai giá trị này override được bằng biến môi trường `THROTTLE_TTL` / `THROTTLE_LIMIT` nên **giá trị thật trên staging chưa chắc là mặc định** | **ADAPTER + XIN IDP** — thiết kế theo ngưỡng 100 req/phút cho an toàn, bật circuit breaker 5 lỗi liên tiếp mở 60 giây. Xin IDP xác nhận giá trị env thật |
| A4 | Giám sát hạn dùng của tài khoản tích hợp | ✅ | `GET /usage/current` | Partner hết hạn thì **mọi** request OCR dừng, và lỗi trả về **kèm HTTP 200** | **ADAPTER** — cron gọi endpoint này mỗi ngày, cảnh báo trước hạn 30 ngày |
| A5 | Gọi được từ server không phải trình duyệt | ⚠️ | mọi endpoint | Cloudflare chặn user-agent mặc định với `403 · error code 1010`. Không phải lỗi ứng dụng và không có trong swagger | **ADAPTER** — set `User-Agent` giống trình duyệt trong HTTP client dùng chung, một dòng cấu hình |

### B · Nộp chứng từ — I-07

| # | Yêu cầu của Claim Platform | Mức | API liên quan | Thực trạng | Giải pháp |
|---|---|---|---|---|---|
| B1 | Nộp gói chứng từ **theo tham chiếu** — `uri` + `sha256` + `mimeType` + `pageCount` — *D3 §I-07* | ❌ | `POST /request` (bytes)<br>`POST /request/from-url` | `from-url` là đường gần nhất nhưng **đang hỏng**: `create()` coi đường dẫn cục bộ là khoá MinIO. Lỗi code, không cứu được từ phía gọi | **ADAPTER tạm + XIN IDP** — trước mắt nộp bytes từ object storage của mình. Xin IDP sửa `from-url`: thêm nhánh `storageType` trong `create()` hoặc upload file vừa tải lên MinIO |
| B2 | `sha256` để chứng minh file IDP đọc đúng là file mình gửi — *D3 §I-07* | ⚠️ | `GET /request/{id}/raw-file` | Phát hiện được nhưng muộn — đã tốn một lượt OCR. Chấp nhận được vì tần suất sai file rất thấp | **ADAPTER** — tính hash trước khi nộp, lưu cùng `documentId` của mình; sau khi có kết quả thì tải lại qua `raw-file` và so hash. Sai hash thì huỷ kết quả và đưa vào hàng đợi operator |
| B3 | `Idempotency-Key` — nộp lại sau timeout không tạo bản ghi trùng — *D3 §I-07* | ❌ | — | Không endpoint nào có. Đây là loại gap vá ở phía mình rẻ hơn chờ vendor | **ADAPTER** — bảng `idempotency_key → (groupId, requestIds[])` trong DB của mình, tra trước khi gọi. Gap đóng hoàn toàn, không cần IDP |
| B4 | `callbackUrl` để IDP báo về — *D3 §I-07* | ❌ | — | Không có | **ADAPTER** — xem G1: nghe socket.io để giảm độ trễ, poller là nguồn sự thật |
| B5 | Gửi `context` — `episodeType`, `serviceDateFrom/To`, `expectedTypes` — *D3 §I-07* | ❌ | `extraParams` | `extraParams` là **type đóng**, chỉ 4 khoá MBAL. Mất `context` chỉ làm IDP không thu hẹp được vùng tìm, không chặn gì | **ADAPTER + XIN IDP** — giữ `context` trong claim, **không gửi sang IDP**. Xin IDP mở `extraParams` nhận thêm khoá tự do |
| B6 | Một mã bộ chứng từ duy nhất `templateSetCode` — *D3 §I-07* | ✅ | `POST /template/document-set`<br>`POST /mbal/docset-map`<br>`POST /mbal/create-request` | **Đã kiểm chứng bằng cách tạo thật**: bộ id 195 + docset-map id 78, nộp thành công chỉ bằng hai giá trị nghiệp vụ | **CẤU HÌNH** — tạo Document Set tên đúng `HEALTH_CLAIM_V1`, nối `claim_type` + `document_type` vào nó, rồi nộp bằng `/mbal/create-request` |
| B7 | Gói tối đa 20 chứng từ, 50 trang, 100 MB — *D3 §I-07* | ⚠️ | `POST /request/group`<br>`POST /request/merge-multiple` | **Đọc được từ code**: `merge-multiple` giới hạn cứng **10 file** (`FilesInterceptor('document', 10, …)`) — thấp hơn con số 20 chứng từ của D3. Kích thước file giới hạn bằng env `MAX_FILE_SIZE`, giá trị chưa biết. `/request/group` **không giới hạn số request trong nhóm** ở tầng code | **ADAPTER** — chia gói quá 10 file thành nhiều lần gọi `merge-multiple`, nhưng vẫn gom vào **một `requestGroupId`** để poll một lần. Xin IDP giá trị `MAX_FILE_SIZE` |
| B8 | Hoá đơn GTGT `T1-04` đi đường **XML**, không có đường OCR — *A4 §1.5* | ❌ | `POST /request` | A4 §1.5 đã nói rõ: claim không có XML vẫn settle được, chỉ mất `R6` | **HOÃN + XIN IDP** — MVP 1 settle trên bảng kê chi phí, chỉ mất quy tắc đối chiếu `R6`. Xin IDP xác nhận có parser XML hoá đơn không |

### C · Phân loại chứng từ

| # | Yêu cầu của Claim Platform | Mức | API liên quan | Thực trạng | Giải pháp |
|---|---|---|---|---|---|
| C1 | IDP phân loại, lựa chọn của khách hàng chỉ là gợi ý — *A4 §2* | ✅ | `POST /request` với nhiều `templateId` | Đã kiểm chứng: nộp 7 template, đúng 1 khớp | **CẤU HÌNH** — luôn nộp cả danh sách template của bộ, không bao giờ chỉ định một template theo lựa chọn của khách |
| C2 | Trả mã loại chứng từ theo bộ `T1-01` … `T2-04` — *A4 §1, §3* | ⚠️ | `templates_data[].subDocumentType`<br>`GET /mbal/template-map` | Đã kiểm chứng: sau khi nạp thì `subDocumentType` trả `CL05` thay vì `null`. Là việc cấu hình, không phải phát triển | **CẤU HÌNH + ADAPTER** — nạp đủ `mbal/template-map` cho mọi template, và giữ bảng `CL*` → `T1-nn` trong adapter, có version |
| C3 | Bảng crosswalk giữa mã phân loại của đối tác và mã của mình — *A4 §3.1, Q-IDP-01* | ⚠️ | `GET /mbal/template-map` | Bảng tồn tại và có 10 mã `CL*`. Đoán sai một dòng là xếp chứng từ vào loại nó không phải, và checklist coi như đã đủ | **XIN IDP** — đây là việc phải ngồi với team IDP lập bảng, không đoán được. **A4 gọi đây là câu hỏi duy nhất "chặn chứ không làm suy giảm"** |
| C4 | Báo khi có mã phân loại mới hoặc đổi tên — *A4 §3.1 rule X-3* | ❌ | — | Không có cơ chế thông báo. Rule X-3 của A4 tồn tại chính vì giả định là không có | **ADAPTER** — adapter đọc `mbal/template-map` định kỳ và **cảnh báo khi thấy mã lạ**, thay vì im lặng bỏ qua |
| C5 | Tách trang khi một lần upload chứa nhiều chứng từ — *A4 I-6* | ❔ | `separatorType` · `separatorValue`<br>`POST /request/merge-multiple` | **Đọc code thì thấy FO không xử lý gì**: nó chỉ `JSON.stringify(separator)` rồi chuyển nguyên sang lõi OCR. Ngữ nghĩa của `separatorType` và `separatorValue` nằm trong lõi OCR — **ngoài tầm cả swagger lẫn source FO**. `merge-multiple` chỉ nhận ảnh, không nhận PDF | **XIN IDP** — hỏi thẳng lõi OCR chấp nhận `separatorType` nào. Trước mắt mobile app chụp từng chứng từ riêng nên nhu cầu tách trang thấp |

### D · Trích xuất field

| # | Yêu cầu của Claim Platform | Mức | API liên quan | Thực trạng | Giải pháp |
|---|---|---|---|---|---|
| D1 | Mỗi field có tên khoá ổn định — *A4 §8* | ❌ | `fields[].k` | Trả **nhãn tiếng Việt**. Đổi tên nhãn trong template làm adapter **hỏng lặng lẽ** — đây là rủi ro kỹ thuật đáng lo nhất ở tầng dữ liệu | **ADAPTER + XIN IDP** — bảng ánh xạ `(templateId, nhãn) → tên field A4`, **khoá theo version template** và cảnh báo khi gặp nhãn lạ. Xin IDP cấp khoá máy ổn định |
| D2 | Field thiếu ghi `null`, không bao giờ là chuỗi rỗng, không bao giờ đoán — *A4 §2* | ❌ | `fields[].v` | Trả chuỗi rỗng cho field không đọc được | **ADAPTER** — chuẩn hoá `""` thành `null` ngay ở normalizer, một dòng code |
| D3 | Mỗi field có confidence trong `[0,1]` — *A4 §2, §7* | ⚠️ | `fields[].c` · `c_llm` | Hai thang trong cùng một payload: template `98.9` thang 0–100, field thang `[0,1]` | **ADAPTER** — chia mức template cho 100, giữ mức field nguyên. Bỏ qua `c_llm` khi `engineType` là azure |
| D4 | Confidence mức claim là **nhỏ nhất** trong bộ field bắt buộc — *A4 §7* | ❌ | `result.confidence` | IDP tính trung bình. Con số này lái thứ tự hàng đợi thẩm định nên sai là hỏng việc thật | **ADAPTER** — bỏ hẳn số IDP trả, tự tính min trên bộ field bắt buộc sau khi đã map sang A4 |
| D5 | Confidence phản ánh được độ đầy đủ | ❌ | `fields[].c` | **12 trong 21 field rỗng vẫn mang `c: 1`.** Lấy min ra `0.87` cho một chứng từ thiếu quá nửa số trường | **ADAPTER** — tính **hai chỉ số tách rời**: confidence (độ chắc) và completeness (tỷ lệ field bắt buộc có giá trị). Hàng đợi thẩm định xếp theo cả hai |
| D6 | Mỗi field có `documentId`, `page`, `bbox` — *A4 §2 provenance* | ❌ | `fields[].page_number`<br>`boxes` ở `/get-ocr` | Chỉ có `page_number`. `bbox` ở endpoint khác, khớp lại theo thứ tự thì mong manh. `documentId` không tồn tại | **XIN IDP** — đây là gap **không vá được ở phía mình**. Xin IDP trả `documentId` và `bbox` ngay trong `/result`. Trước mắt tab F46 hiển thị được nhưng **mất neo hai chiều** |
| D7 | Đánh dấu field nào bắt buộc, field nào tuỳ chọn — *A4 §2 field status* | ❌ | — | Không có. Nhưng A4 §4 đã liệt kê sẵn `M`/`O` cho từng loại chứng từ nên tự dựng được | **ADAPTER** — giữ danh sách field bắt buộc trong bảng ánh xạ của mình theo A4 §4, không trông vào IDP |
| D8 | Ngày chuẩn hoá `YYYY-MM-DD` — *A4 §2* | ⚠️ | `fields[].v` | Không nhất quán: `"1990-01-01"` đúng chuẩn nhưng `"20/03/2025"` thì không | **ADAPTER** — normalizer nhận cả `dd/mm/yyyy`, `dd-mm-yyyy`, `ngày DD tháng MM năm YYYY` theo đúng A4 §2 |
| D9 | Số tiền là số nguyên VND, không dấu phân cách — *A4 §2* | ❔ | `fields[].v` | **Kiểm chứng được một nửa từ định nghĩa template**: các cột tiền trong `tableFields` đều có `type: null` — **template không ép kiểu**, nên định dạng trả về do lõi OCR quyết định, không suy được từ cấu hình. Chứng từ mẫu không có trường tiền nên chưa thấy giá trị thật | **ADAPTER** — normalizer bỏ dấu `.` phân cách nghìn theo A4 §2 và ép về số nguyên. Vì template không ép kiểu nên **phải phòng cả trường hợp trả chuỗi có dấu phân cách** |
| D10 | Field dẫn xuất do IDP tính: `serviceCategory`, `itemClass`, hình thức điều trị — *A4 I-2* | ❌ | — | A4 I-2 ghi giả định là **tính trong IDP**. Giả định đó sai với hiện trạng | **ADAPTER + XIN IDP** — tự suy `serviceCategory` từ `itemName` bằng bảng từ khoá, chấp nhận sai số và cho thẩm định viên sửa. Xin IDP xác nhận có tính được trong IDP không |

### E · Dòng chi phí

| # | Yêu cầu của Claim Platform | Mức | API liên quan | Thực trạng | Giải pháp |
|---|---|---|---|---|---|
| E1 | Bảng dòng chi phí có cấu trúc — *A4 §8 `lines[]`* | ⚠️ | `GET /template/{id}` → `tableFields`<br>`data.tables[]` | **Kiểm chứng được từ định nghĩa template, không cần chạy hoá đơn.** `tableFields` khai đúng các cột IDP sẽ trả. Nhưng **mỗi template một bộ cột khác nhau**: `5383` *Bảng kê chi phí* có **13 cột**, `5218` cùng tên có **7 cột**, `5125` *Phiếu thanh toán nội trú* có **9 cột**, `5312` *Biên lai viện phí* có **5 cột**. Tên cột là **nhãn tiếng Việt**, không phải khoá máy — cùng vấn đề với `D1` | **ADAPTER** — bảng ánh xạ cột **theo từng templateId**, không dùng một map chung. Vẫn nên chạy một hoá đơn thật để xác nhận giá trị, nhưng cấu trúc thì đã chốt được |
| E2 | `serviceCategory` thuộc bộ 20 giá trị — *A5 §8.1, A4 §4.5* | ❌ | `GET /template/{id}` → `tableFields` | **Xác nhận từ định nghĩa template: không template nào có cột phân loại dịch vụ.** Gần nhất là cột `Mã` của `5125`, nhưng đó là mã dịch vụ của bệnh viện chứ không phải nhóm quyền lợi. Suy được từ cột `Nội dung` | **ADAPTER** — bảng từ khoá `Nội dung` → `serviceCategory`, thẩm định viên sửa được trên tab F46. Bốn giá trị `SCREENING`/`VACCINATION`/`OPTICAL`/`HEARING` là master data của mình, D3 đã nói rõ không cần IDP |
| E3 | `paidBySocialInsurance` trên mỗi dòng, mặc định `0` — *A4 §8* | ⚠️ | `GET /template/{id}` → `tableFields` | **Trường này CÓ, chỉ mang tên khác theo từng template**: `5383` có `Quỹ BHYT`, `Thành tiền BH` và `Tỷ lệ thanh toán BHYT`; `5218` có `Thanh toán bảo hiểm y tế`; `5125` có `BHYT trả`. Trước đây đánh ❌ là **sai** — chưa đọc định nghĩa template | **ADAPTER** — map ba tên gọi đó về `paidBySocialInsurance`. MVP 1 vẫn ghi và **không khấu trừ** theo simplification S-17 của A5 §6.1, nhưng **bắt được giá trị thật ngay từ đầu** thì bật khấu trừ sau này chỉ là đổi cấu hình |
| E4 | `documentDate` (L15) và `icd10` (L16) theo dòng — *A4 Q-IDP-02* | ❌ | — | Chặn loại trừ #5 và #9 của A5 §7. A4 nhấn mạnh **tần suất quan trọng ngang khả năng** | **XIN IDP + ADAPTER** — hỏi IDP có trích được không **và tần suất nhà cung cấp thực sự in ở mức dòng**. Nếu không thì cột do thẩm định viên nhập, và console nói rõ |
| E5 | `unitOfMeasure`, `otherPayerAmount`, `drugRegistrationNo` — *A4 Q-IDP-04* | ⚠️ | `GET /template/{id}` → `tableFields` | **Hai trong ba trường CÓ.** `unitOfMeasure` — cột `Đơn vị tính` ở `5383`, `5218` và `Đơn vị` ở `5125`. `otherPayerAmount` — cột `Các phương thức thanh toán khác` ở `5218` và `Khác` ở `5383`. **`drugRegistrationNo` thì không template nào có** | **ADAPTER + XIN IDP** — map hai trường có sẵn. Hỏi IDP về số đăng ký thuốc: nó nằm trên đơn thuốc `T1-02` chứ không nằm trên bảng kê, nên có thể phải lấy từ template khác |
| E6 | `amountClaimed` có sẵn để engine không phải tính lại — *A4 §8* | ⚠️ | `GET /template/{id}` → `tableFields` | **Có, nhưng tách làm nhiều cột theo từng template.** `5383` có `Thành tiền BV` và `Thành tiền BH` riêng; `5125` và `5312` chỉ có một cột `Thành tiền`. Nghĩa là **số tiền yêu cầu bồi thường phải chọn đúng cột theo loại chứng từ**, không có một trường chung | **ADAPTER** — quy tắc chọn cột theo templateId, mặc định lấy `Thành tiền BV` khi có cả hai. Vẫn đối chiếu với tổng ở footer theo `R5` |

### F · Đối chiếu chéo và trạng thái hồ sơ

| # | Yêu cầu của Claim Platform | Mức | API liên quan | Thực trạng | Giải pháp |
|---|---|---|---|---|---|
| F1 | Chạy 9 quy tắc đối chiếu chéo `R1`–`R9` — *A4 §5, I-7* | ⚠️ | `/result/json` của flow đánh giá | Không có ở bước trích xuất, nhưng bước đánh giá **làm đúng việc này** — sai chỗ và sai định dạng so với A4 §5 | **ADAPTER** — coi nhóm `Cảnh báo` của bước đánh giá là **nguồn cho `R1`–`R4` và `R8`**, map `warning_level` sang mức nghiêm trọng. `R5` và `R6` tự tính bằng số học ở adapter |
| F2 | Trạng thái đủ hồ sơ — 6 giá trị của *A4 §6* | ❌ | — | IDP không tính. Đây là logic thuần nghiệp vụ, không cần vendor | **ADAPTER** — tự tính: so `templates_data[]` nhận được với `templates[]` của document set, kết hợp với danh sách field bắt buộc ở D7 và kết quả `R1`–`R8` |
| F3 | Mảng `warnings[]` mỗi cảnh báo gắn với `documentId` và `field` — *A4 §8* | ❌ | — | Cảnh báo của IDP là văn bản, không neo được vào field | **ADAPTER tạm + XIN IDP** — adapter tự sinh `warnings[]` từ `R1`–`R8` của mình và neo vào field mình biết. Cảnh báo đến từ IDP thì chỉ neo được ở mức chứng từ. Phụ thuộc D6 để neo chính xác |
| F4 | `missing.documentTypes` và `missing.fields` — *A4 §8* | ❌ | — | — | **ADAPTER** — sản phẩm phụ của F2, không cần thêm việc |
| F5 | Công cụ đo độ chính xác so với bộ ground-truth — *A4 §10* | ✅ | `POST /request/compare`<br>`POST /request/compare-multiple` | Nhận XLSX, trả về XLSX kết quả so sánh. Có sẵn mà tài liệu chưa ai biết | **CẤU HÌNH** — dựng bộ ground-truth dạng XLSX theo A4 §10 rồi chạy định kỳ. **Không phải viết công cụ** |

### G · Thông báo và lấy kết quả — I-08, I-09

| # | Yêu cầu của Claim Platform | Mức | API liên quan | Thực trạng | Giải pháp |
|---|---|---|---|---|---|
| G1 | Webhook ký HMAC, giao ít nhất một lần — *D3 §I-08* | ⚠️ | socket.io `Channel_{tenantId}`<br>event `updateOcrResult` | Socket đúng tinh thần I-08 nhưng không ký, không gửi lại, chia phòng theo tenant | **ADAPTER** — nghe socket để giảm độ trễ nhưng **mỗi tin chỉ kích hoạt một lượt gọi G2**, không tin nội dung. Adapter tự phát sự kiện nội bộ đúng shape webhook I-08 để phần còn lại của hệ thống không phải biết |
| G2 | Polling làm lưới an toàn — *D3 §I-08* | ✅ | `GET /request/group/{groupId}/check-status` | Một lời gọi cho cả nhóm. Xong sau ~18 giây với 1 trang | **ADAPTER** — backoff 3s → 5s → 10s → 30s, trần 15 phút rồi chuyển `EXTRACTION_TIMEOUT` vào hàng đợi operator |
| G3 | Lấy kết quả có cấu trúc — *D3 §I-09* | ✅ | `GET /request/{id}/result`<br>`GET /request/{id}/check-status` | Tiện ích không có trong tài liệu nào | **ADAPTER** — poll bằng `check-status` vì khi `Completed` nó **trả kèm luôn kết quả**, tiết kiệm một lời gọi mỗi chứng từ |
| G4 | `409 NOT_READY` kèm `Retry-After` khi chưa xong — *D3 §I-09* | ❌ | — | Request hỏng trả **HTTP 200** với `result: {}` và mã lỗi trong body. Đây là loại lỗi làm một dịch vụ hỏng trông như một claim sạch | **ADAPTER** — **luôn đọc `errorCode` trong body, không bao giờ suy từ HTTP status**. Viết thành một hàm dùng chung để không ai quên |
| G5 | Kết quả **bất biến**, giữ 90 ngày, đọc lại được để audit — *D3 §I-09* | ❌ | `PUT /request/{id}/result` tồn tại | D2 §5.6 nói thẳng: *không ai được sửa kết quả gốc, kể cả admin*. Làm đúng cách này thì việc IDP không cam kết bất biến thôi không còn là vấn đề | **ADAPTER** — copy kết quả sang store của mình **ngay lần đọc đầu**, mọi audit đọc bản copy. **Cấm gọi `PUT /request/{id}/result`** ở tầng adapter |
| G6 | Một dịch vụ hỏng không bao giờ thành quyết định từ chối — *D3 G-3* | ✅ | `check-status` → trạng thái vận hành | Thuộc phía mình hoàn toàn | **ADAPTER** — mọi lỗi IDP chuyển thành `EXTRACTION_ERROR` hoặc `DOC_PROCESSING` timeout vào hàng đợi operator, không bao giờ thành `DECLINED` |

### H · Đánh giá lâm sàng — I-15, I-16

| # | Yêu cầu của Claim Platform | Mức | API liên quan | Thực trạng | Giải pháp |
|---|---|---|---|---|---|
| H1 | Một API đánh giá độc lập, không đi qua workflow | ❌ | `PUT /workflow/request` | Cả 20 endpoint đánh giá đều nằm dưới `/workflow` | **CHẤP NHẬN + XIN IDP** — coi `PUT /workflow/request` là một REST endpoint bình thường và **không đưa logic nghiệp vụ vào flow**. Xin IDP lộ trình mở endpoint riêng. **Đây là quyết định cần sếp chốt** |
| H2 | Gửi `clinicalContext` — `icd10`, `episodeType`, `ageBand`, `sex` — *D3 §I-15* | ❌ | `extraInputData[]` | Không cần IDP mở endpoint mới, nhưng cũng không có gì ràng buộc flow đọc đúng | **ADAPTER + XIN IDP** — gói toàn bộ payload I-15 vào một template ảo `CLINICAL_CONTEXT_V1` trong `extraInputData`. Xin IDP cam kết flow đọc đúng template ảo đó |
| H3 | Gửi `lines[]` kèm `serviceCategory` để đánh giá theo dòng — *D3 §I-15* | ❌ | — | Flow đọc thẳng kết quả OCR | **XIN IDP** — flow phải nhận dòng chi phí thì mới đánh giá theo dòng được. Không vá được ở phía mình |
| H4 | Ghim `knowledgeBaseVersion` theo claim — *D3 §I-15, D1 BR-KB-4* | ⚠️ | `workflowId` | Chỉ đúng nếu kỷ luật vận hành được tuân thủ. Là quy ước, không phải ràng buộc kỹ thuật | **ADAPTER + XIN IDP** — ghim `workflowId` làm phiên bản tri thức và **từ chối mọi `workflowId` lạ**. Xin IDP cam kết quy trình: sửa flow là bump `workflowId` |
| H5 | Chạy bất đồng bộ, poll tới khi xong — *D3 §I-15* | ⚠️ | `PUT /workflow/request`<br>`GET /workflow/run-workflow-request/{id}`<br>`GET /workflow/request/{id}` | `autoStart` không có tác dụng. Bước kích hoạt **lúc `400` lúc `200`** với cùng tham số | **ADAPTER** — luôn gọi `run-workflow-request` sau khi tạo, và **thử lại khi gặp `400`** thay vì kết luận hỏng |
| H6 | Lấy khuyến nghị có cấu trúc — *D3 §I-16* | ✅ | `GET /workflow/request/{id}/result/json` | **Endpoint chạy đúng.** Kiểm chứng trên 8 workflow request: flow `5ed8e4fc` trả JSON đầy đủ **4/4 lần**, flow `8d873709` trả `{}` **0/4**. Cái quyết định là chọn flow, không phải endpoint | **CẤU HÌNH** — ghim `workflowId` là `5ed8e4fc`. Adapter vẫn **coi `{}` rỗng là HỎNG** chứ không phải "không có khuyến nghị", để phòng ai đó đổi flow |
| H7 | `proposedReasonCode` chỉ `NOTREL` hoặc `NOTCOV` — *D3 §I-16, D2 §5.8 F3* | ❌ | — | Đầu ra hiện tại là `Cảnh báo` và `Trích xuất field` — gần R1–R5 của Track 1 hơn là Track 2 | **XIN IDP** — đặt hàng một flow Track 2 riêng đúng hợp đồng I-16. Không suy được từ `instruction_type` |
| H8 | `evidence[]` **bắt buộc** trên mọi khuyến nghị — *D3 §I-16, D1 BR-KB-2* | ❌ | — | D1 nói thẳng: không có clinical sign-off cho cơ sở tri thức thì trích dẫn nguồn **không phải thứ trang trí** | **XIN IDP** — không thương lượng được. **Mất một trong ba chốt kiểm soát mà D1 RK-08 dựa vào**, nên nếu IDP không trả được thì Track 2 phải chạy ở chế độ advisory và ghi rõ |
| H9 | `confidence` kèm band `HIGH`/`MEDIUM`/`LOW` — *D2 §5.8 F5* | ❌ | `warning_level` | Hai khái niệm khác nhau. Map tạm để dựng được panel, không phải để dùng lâu dài | **ADAPTER tạm** — map `warning_level` `1` → `HIGH`, `0.5` → `MEDIUM`, `0` → `LOW` và **ghi rõ trên UI rằng đây là mức nghiêm trọng, không phải độ tin cậy** |
| H10 | Khuyến nghị gắn với **một dòng chi phí** — *D1 §5.10.3, D2 §5.8 F1* | ❌ | — | D1 §5.10.3 nói *khuyến nghị không bao giờ ở mức claim* — hiện trạng vi phạm đúng điều đó | **XIN IDP** — phụ thuộc H3. Trước mắt hiển thị khuyến nghị ở **mức claim** trên tab F46 và nói rõ là chưa theo dòng |
| H11 | `linesAssessed[]` nói rõ dòng nào đã được xem — *D3 §I-16* | ❌ | — | D3 nói rõ: dòng vắng mặt trong `linesAssessed` là **chưa được xem**, khác hẳn với sạch | **XIN IDP** — phụ thuộc H3. Trước mắt **không được coi dòng nào là đã đánh giá sạch** |
| H12 | Khuyến nghị không bao giờ tự động vào sổ — *D1 BR-TRACK-3/4, G-7* | ✅ | — | Thuộc phía mình, giữ nguyên | **ADAPTER** — khuyến nghị chỉ ghi vào store riêng, chỉ khi thẩm định viên xác nhận thì mới chạy lại chuỗi quy tắc |

### I · Màn hình thẩm định — D2 §5.6 tab F46, §5.8 panel F56

| # | Yêu cầu của Claim Platform | Mức | API liên quan | Thực trạng | Giải pháp |
|---|---|---|---|---|---|
| I1 | Ảnh gốc đặt cạnh dữ liệu trích xuất — *D2 §5.6.1* | ✅ | `GET /request/{id}/raw-file` | Cũng là thứ thay thế cho deep link I-10 | **ADAPTER** — tải và cache ảnh gốc, hiển thị vùng trái của tab F46 |
| I2 | **Neo hai chiều** — chọn field thì highlight vùng trên ảnh và ngược lại — *D2 §5.6.1* | ❌ | `fields[].page_number`<br>`boxes` ở `/get-ocr` | D2 gọi neo hai chiều là **lý do duy nhất để ảnh và dữ liệu ở chung một màn hình** | **XIN IDP** — phụ thuộc D6. Trước mắt **giảm cấp xuống neo theo trang**: chọn field thì nhảy đúng trang, không highlight được vùng. Ghi vào D2 là tính năng giảm cấp có chủ ý |
| I3 | Mọi field sửa được, sửa vào bản copy chứ không vào bản gốc — *D2 §5.6, A3 §3.3* | ✅ | — | Điều kiện: không bao giờ gọi `PUT /request/{id}/result` | **ADAPTER** — mọi chỉnh sửa ghi vào bản copy, giữ cặp giá trị máy đọc và giá trị người sửa để so lâu dài |
| I4 | Panel duyệt khuyến nghị với 5 trường từ I-16 — *D2 §5.8.1* | ❌ | `/result/json` | Bốn trong năm trường không có | **XIN IDP + HOÃN** — phụ thuộc H7, H8, H9. Trước mắt dựng panel **rút gọn** chỉ có diễn giải và mức nghiêm trọng, đánh dấu rõ là chưa đủ spec F56 |
| I5 | Deep link sang màn hình review của IDP — *D3 §I-10* | ❌ | — | Không có endpoint `review`, không có cơ chế ký `sig`/`exp` | **HOÃN** — bỏ khỏi MVP 1. I1 còn đúng hơn vì thẩm định viên không rời console và không cần session IDP. **Cần đối chất lại `C-IDP-08`** vì D3 ghi là đã xác nhận |

### J · Master data

| # | Yêu cầu của Claim Platform | Mức | API liên quan | Thực trạng | Giải pháp |
|---|---|---|---|---|---|
| J1 | `facilityCountry` để chạy loại trừ *"điều trị ngoài Việt Nam"* — *A5 §7 loại trừ #10, Q-IDP-05* | ✅ | `GET /metadata/opensearch/table/{tableId}/search`<br>`GET .../rows/table/322/` | **Đóng được Q-IDP-05 mà không cần IDP thay đổi gì.** Bảng Hospital 13.328 dòng có cột `country` | **ADAPTER** — tra tên cơ sở qua OpenSearch, lấy `country`, dùng làm `facilityCountry`. Không khớp thì rơi xuống checkpoint thủ công thay vì đoán |
| J2 | Danh mục mã phẫu thuật, quốc gia, loại claim, loại chứng từ | ✅ | `GET /metadata/forwarder/table-list`<br>`GET .../rows/table/{tableId}/` | Surgery Code 95 · Country 242 · Claim Type 4 · Document Type 10 | **ADAPTER** — đồng bộ 5 bảng này về master data của mình định kỳ, không gọi trực tiếp mỗi claim |

## Đối chiếu với nhận định của PO về checklist đánh giá

Ngày 29/08/2026, PO (BIA) nhận xét về bảng *"Hệ thống hiện đang kiểm được những gì"*. Mục này **kiểm chứng từng nhận định bằng cấu hình thật của flow đánh giá** — đọc trực tiếp `GET /workflow/forwarder/api/v1/flows/{flowId}` trên flow chuẩn `5ed8e4fc-…` "Workflow Claim 28/8", cộng với kết quả chạy thật.

Flow này có **154 node**, trong đó có sẵn các bộ so sánh chuyên dụng: `Insured Name Comparator` · `DOB Comparator` · `Gender Comparator` · `Identification number Comparator` · `Admission Date Comparator` · `Discharge Date Comparator` · `Death Date Comparator` · `Event Date Comparator` · `Diagnosis Codes Comparator` · `Place Of Treatment Comparator` · `Metatable Find Matched Hospital` · `LLM Find Surgery Code`.

| # | Mục trong checklist | PO nhận định | Bằng chứng từ hệ thống | Kết luận |
|---|---|---|---|---|
| 1 | **Hồ sơ đã đủ giấy tờ bắt buộc chưa** | Chưa setup workflow, IDP làm tốt | Tìm trong toàn bộ cấu hình flow: **0 lần** xuất hiện `bắt buộc`, `đủ giấy tờ`, `thiếu chứng từ`. Flow **không có** chỉ thị kiểm tra đủ bộ hồ sơ. Nhưng dữ liệu để làm thì có sẵn: `documentSet.templates` là danh sách bắt buộc, `templates_data[]` là danh sách nhận được | **PO đúng.** Là việc cấu hình flow, không phải giới hạn của IDP. **Và Claim Platform cũng tự tính được** bằng cách so hai danh sách trên — xem dòng `F2` |
| 2 | **Đối chiếu với thông tin khách hàng đã khai** | Chưa setup workflow | ❗ **Flow ĐÃ LÀM việc này.** Có node `So sánh với DP` đối chiếu `ho_va_ten` · `gender` · `dob` với `ho_va_ten_dp` · `gender_dp` · `dob_dp`, cộng `Identification number Comparator`. Kết quả chạy thật ngày 03/09 trả về: *"Dữ liệu chứng từ … trùng khớp với dữ liệu từ DataPlatform"* | **PO chưa chính xác — nó đã chạy.** Nhưng đối chiếu với **DataPlatform**, không phải với đơn khai của khách. **Cần làm rõ DataPlatform chứa gì** — nếu nó chính là nơi giữ thông tin khách khai thì mục này coi như xong |
| 3 | **Ngày tháng có hợp lý không** | Chưa setup workflow | ❗ **Flow ĐÃ LÀM.** Có 4 bộ so sánh ngày: vào viện, ra viện, tử vong, ngày xảy ra sự kiện. Kết quả chạy thật **bắt được lỗi thật**: *"Ngày xảy ra sự kiện bảo hiểm (20/03/2025) thuộc về tương lai so với thời điểm hiện tại"*, và xác nhận ngày vào/ra viện đồng nhất giữa các chứng từ | **PO chưa chính xác — nó đã chạy và đã bắt được lỗi.** Đánh dấu "chưa làm" trong checklist là sai |
| 4 | **Giấy tờ có bị làm giả không** | Chưa phát triển vì thiếu dữ liệu ngành | Tìm `giả mạo`, `làm giả` trong cấu hình flow: **0 lần**. Không có node nào liên quan | **PO đúng** |
| 5 | **Đánh giá chuyên môn y khoa** | Chưa phát triển vì thiếu data tin cậy | Đầu ra chỉ có `Cảnh báo` và `Trích xuất field`. Không có mã lý do `NOTREL`/`NOTCOV`, không đánh giá theo dòng chi phí, **không trích dẫn nguồn tri thức** | **PO đúng.** Khớp với các dòng `H7`, `H8`, `H10` ở bảng trên |
| 6 | **Đánh giá độ tin cậy của việc đọc chứng từ** | Đã có confidence score, tuỳ khách hàng bật; MBAL yêu cầu không hiện | Payload trích xuất thật có `fields[].c` và `c_llm` theo từng field, `data.confidence` theo template, và `confidence` mức request | **PO đúng — có confidence.** Nhưng khi bật cần biết ba điều ở các dòng `D3`, `D4`, `D5`: hai thang đo khác nhau, mức request tính trung bình thay vì nhỏ nhất, và **field rỗng vẫn mang `c: 1`** |
| — | **Tên bệnh viện, tên phẫu thuật có trong danh mục** *(checklist đánh dấu "có tra nhưng kết luận chưa đáng tin")* | — | Flow có `Metatable Find Matched Hospital` và `LLM Find Surgery Code`, tra vào bảng Hospital 13.328 dòng và Surgery Code 95 dòng. Nhưng kết quả chạy thật: `HospitalCode` trả **`00000`** dù bệnh viện có thật trong danh mục, và *"Có phẫu thuật nhưng không có mã tương ứng"* | **Nhận định "chưa đáng tin" là đúng.** Cơ chế tra có, nhưng trả giá trị giữ chỗ. Xem dòng `J1` — tra thẳng bằng `/metadata/opensearch` thì ra đúng `hospital_code` |

**Tóm lại: PO đúng 4/6, và hai chỗ lệch đều theo hướng tốt hơn** — hai mục đang bị đánh dấu "chưa làm" thì thực ra flow đã làm rồi. Mục 2 cần làm rõ DataPlatform là gì; mục 3 nên sửa lại thành "đã làm" trong bảng tổng hợp.

---

# PHẦN 2 — DANH SÁCH API

Toàn bộ request/response dưới đây là **dữ liệu thật**, gọi ngày 03/09/2026 lên môi trường staging. Payload quá lớn được cắt bớt và ghi rõ — **bản nguyên vẹn của mọi endpoint nằm trong thư mục `request-response-json/` gửi kèm**, 192 file, mỗi file một endpoint, có `INDEX.md` tra cứu.

Ba thứ áp dụng cho mọi lời gọi:

```
Authorization: Bearer <access_token>
User-Agent: Mozilla/5.0 (…) Chrome/126.0.0.0 Safari/537.36
Accept: application/json, text/plain, */*
```

> **`User-Agent` là bắt buộc.** Cloudflare chặn user-agent mặc định của thư viện HTTP với `403 · error code: 1010`, không phải lỗi ứng dụng và không có trong swagger.

---

## 2.1 Xác thực và danh tính

#### `POST /auth/login`

**Đăng nhập, lấy JWT. Không thấy refresh token trong response.**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** | `POST` | `201` |

Dùng cho cả FO và BO — đổi base URL. Token đặt vào `Authorization: Bearer`.

**Request**

```json
{
  "username": "<redacted-email>",
  "password": "<redacted>"
}
```

**Response**

```json
{
  "access_token": "<340 ký tự, đã ẩn>"
}
```

#### `GET /user/me`

**Danh tính, vai trò và tenant của token đang dùng**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

Cách nhanh nhất để biết token là Admin hay End User: đọc `roleId` — `1` = Partner (Admin), `2` = Tenant (End User).

**Response**

```json
{
  "id": "203",
  "email": "<redacted-email>",
  "name": "Brian",
  "tenantId": "176",
  "roleId": 2,
  "tenant": {
    "expiredDate": "2027-08-31",
    "name": "Phòng 1"
  }
}
```

#### `GET /partner/permissions`

**Các quyền mà partner được bật**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Cho biết partner có được dùng module MBAL và Workflow hay không — nên kiểm tra trước khi tích hợp.

**Response**

```json
[
  "Export Workflows",
  "Export Templates",
  "Export Metadata tables",
  "MBAL",
  "Workflows"
]
```

#### `GET /tenant/my-tenant`

**Thông tin tenant hiện tại: hạn dùng, block limit**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: 403) | `GET` | `200` |

`expiredDate` ở đây là hạn của **tenant**. Nhưng OCR còn kiểm hạn của **partner** đứng trên tenant — partner hết hạn thì mọi request trả `statusCode 1014 · Partner is expired` **kèm HTTP 200**.

**Response**

```json
{
  "id": "176",
  "name": "Phòng 1",
  "description": null,
  "contactName": null,
  "contactEmail": null,
  "contactPhone": null,
  "minBlocks": 0,
  "maxBlocks": 0,
  "address": null,
  "startDate": "2026-08-24",
  "expiredDate": "2027-08-31",
  "isActive": true,
  "blockLimit": 1,
  "createdAt": "2026-08-24T08:47:08.803Z",
  "updatedAt": "2026-08-25T02:04:51.932Z",
  "deletedAt": null,
  "partnerId": "40"
}
```

#### `GET /usage/current`

**Hạn mức trang đã dùng trong kỳ**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Nên poll định kỳ để cảnh báo trước khi hết quota.

**Response**

```json
{
  "totalBlockRegistered": "1",
  "pageUsed": "1",
  "totalBlockUsedAbove85": "0",
  "totalBlockAllUsed": "0"
}
```

#### `GET /usage`

**Lịch sử sử dụng có phân trang**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

**Response**

```json
{
  "items": [
    {
      "id": "4291",
      "tenantId": "176",
      "month": "2026-09",
      "pageCount": 1,
      "requestCount": 1,
      "blockLimit": 1,
      "createdAt": "2026-09-03T04:56:19.410Z",
      "updatedAt": "2026-09-03T04:56:19.385Z",
      "deletedAt": null,
      "tenant": {
        "id": "176",
        "name": "Phòng 1",
        "startDate": "2026-08-24"
      },
      "status": 1,
      "blockUsed": 1
    },
    {
      "id": "4288",
      "tenantId": "176",
      "month": "2026-08",
      "pageCount": 151,
      "requestCount": 43,
      "blockLimit": 1,
      "createdAt": "2026-08-24T08:47:08.803Z",
      "updatedAt": "2026-08-28T04:19:48.963Z",
      "deletedAt": null,
      "tenant": {
        "id": "176",
        "name": "Phòng 1",
        "startDate": "2026-08-24"
      },
      "status": 1,
      "blockUsed": 1
    }
  ],
  "totalItems": 2
}
```

#### `GET /user`

**Danh sách người dùng — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

**Response**

```json
{
  "items": [
    {
      "id": "204",
      "email": "<redacted-email>",
      "name": "Hieu",
      "tenantId": "177",
      "roleId": 2,
      "isActive": true,
      "createdAt": "2026-09-02T10:46:58.224Z",
      "updatedAt": "2026-09-02T10:46:58.224Z",
      "tenant": {
        "id": "177",
        "name": "Phòng 2",
        "description": "OS",
        "startDate": "2026-09-02",
        "expiredDate": "2026-10-31",
        "isActive": true,
        "blockLimit": 1,
        "partnerId": "40"
      }
    },
    {
      "id": "203",
      "email": "<redacted-email>",
      "name": "Brian",
      "tenantId": "176",
      "roleId": 2,
      "isActive": true,
      "createdAt": "2026-08-24T08:47:34.590Z",
      "updatedAt": "2026-08-24T08:47:34.590Z",
      "tenant": {
        "id": "176",
        "name": "Phòng 1",
        "description": null,
        "startDate": "2026-08-24",
        "expiredDate": "2027-08-31",
        "isActive": true,
        "blockLimit": 1,
        "partnerId": "40"
      }
    }
  ],
  "totalItems": 2
}
```

#### `GET /tenant`

**Danh sách tenant — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

**Response**

```json
{
  "items": [
    {
      "id": "177",
      "name": "Phòng 2",
      "description": "OS",
      "contactName": null,
      "contactEmail": null,
      "contactPhone": null,
      "minBlocks": 0,
      "maxBlocks": 0,
      "address": null,
      "startDate": "2026-09-02",
      "expiredDate": "2026-10-31",
      "isActive": true,
      "blockLimit": 1,
      "createdAt": "2026-09-02T10:46:00.533Z",
      "updatedAt": "2026-09-02T10:46:00.533Z",
      "deletedAt": null,
      "partnerId": "40",
      "noOfUser": 1
    },
    {
      "id": "176",
      "name": "Phòng 1",
      "description": null,
      "contactName": null,
      "contactEmail": null,
      "contactPhone": null,
      "minBlocks": 0,
      "maxBlocks": 0,
      "address": null,
      "startDate": "2026-08-24",
      "expiredDate": "2027-08-31",
      "isActive": true,
      "blockLimit": 1,
      "createdAt": "2026-08-24T08:47:08.803Z",
      "updatedAt": "2026-08-25T02:04:51.932Z",
      "deletedAt": null,
      "partnerId": "40",
      "noOfUser": 1
    }
  ],
  "totalItems": 2
}
```

#### `GET /tenant/compact`

**Danh sách tenant rút gọn — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

**Response**

```json
[
  {
    "id": "177",
    "name": "Phòng 2"
  },
  {
    "id": "176",
    "name": "Phòng 1"
  }
]
```

#### `GET /encryption/public-key`

**Public key của hệ thống**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

**Response**

```json
"-----BEGIN PUBLIC KEY-----\r\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0g6VxR5pig3IQLj9G5+R\r\nmOLuTGiTb1NpwW5ChDcBHphXjdCfqHDHzhogEjgfF9atyTf01rEKBZIz91faqow3\r\n00sag527tQccex9y8+Y2kOAfpbC9hvq435Ia9hls5fkO1w4cq2Mt5Nmqc680KFYK\r\nTBf73mvnhilUb4HOCW/VxSdiZV9D2AGeDkQ4bhY3enm/A2qtTzlzceMM2iVowqdS\r\njmSVr3WAdt8Iyg0OAxj9gmpftHUUCLwgTLLjhESPzFr0/mdVOQn4g64a4mEVsp8O\r\n2ERvVI2Sg9J/Cnw4xWOnCMmX5KbsW8RknMAD6snPkiY6XxGcMnc0wXeR+3kLzGdV\r\nLQIDAQAB\r\n-----END PUBLIC KEY-----\r\n"
```

#### `GET /user/{id}`

*Gọi thật: `/user/203`*

**Chi tiết một người dùng — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

**Response**

```json
{
  "id": "203",
  "email": "<redacted-email>",
  "name": "Brian",
  "tenantId": "176",
  "roleId": 2,
  "isActive": true,
  "createdAt": "2026-08-24T08:47:34.590Z",
  "updatedAt": "2026-08-24T08:47:34.590Z",
  "tenant": {
    "id": "176",
    "name": "Phòng 1",
    "description": null,
    "startDate": "2026-08-24",
    "expiredDate": "2027-08-31",
    "isActive": true,
    "blockLimit": 1,
    "partnerId": "40"
  }
}
```

#### `GET /tenant/usage-detail`

**Chi tiết mức sử dụng của tenant — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

**Response**

```json
{
  "items": [
    {
      "id": "21378",
      "tenantId": "176",
      "userId": "203",
      "status": 2,
      "pageCount": 1,
      "createdAt": "2026-09-03T06:24:15.956Z",
      "user": {
        "id": "203",
        "email": "<redacted-email>",
        "name": "Brian"
      },
      "tenant": {
        "id": "176",
        "name": "Phòng 1"
      }
    },
    {
      "id": "21377",
      "tenantId": "176",
      "userId": "203",
      "status": 2,
      "pageCount": 1,
      "createdAt": "2026-09-03T06:24:15.351Z",
      "user": {
        "id": "203",
        "email": "<redacted-email>",
        "name": "Brian"
      },
      "tenant": {
        "id": "176",
        "name": "Phòng 1"
      }
    },
    {
      "id": "21376",
      "tenantId": "176",
      "userId": "203",
      "status": 2,
      "pageCount": 1,
      "createdAt": "2026-09-03T04:56:01.324Z",
      "user": {
        "id": "203",
        "email": "<redacted-email>",
        "name": "Brian"
      },
      "tenant": {
        "id": "176",
        "name": "Phòng 1"
      }
    },
    {
      "id": "21371",
      "tenantId": "176",
      "userId": "203",
      "status": 2,
      "pageCount": 3,
      "createdAt": "2026-08-28T04:11:24.854Z",
      "user": {
        "id": "203",
        "email": "<redacted-email>",
        "name": "Brian"
      },
      "tenant": {
        "id": "176",
        "name": "Phòng 1"
      }
    },
    {
      "id": "21370",
      "tenantId": "176",
      "userId": "203",
      "status": 2,
      "pageCount": 2,
      "createdAt": "2026-08-28T04:11:23.765Z"
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /usage/export`

**Xuất báo cáo sử dụng ra file**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Trả file, không phải JSON.

**Response**

```json
{
  "__binary__": true,
  "content_type": "application/vnd.openxmlformats",
  "bytes": 8631
}
```

#### `GET /usage/export-pdf`

**Xuất báo cáo sử dụng ra PDF**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

**Response**

```json
"%PDF-1.3\n%����\n9 0 obj\n<<\n/Type /ExtGState\n/ca 0.1\n>>\nendobj\n11 0 obj\n<<\n/Type /ExtGState\n/ca 0.5\n/CA 0.5\n>>\nendobj\n12 0 obj\n<<\n/Type /ExtGState\n/ca 1\n/CA 1\n>>\nendobj\n7 0 obj\n<<\n/Type /Page\n/Parent 1 0 R\n/MediaBox [0 0 841.89 595.28]\n/Contents 5 0 R\n/Resources 6 0 R\n>>\nendobj\n6 0 obj\n<<\n/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]\n/Font <<\n/F1 8 0 R\n/F2 10 0 R\n/F3 13 0 R\n>>\n/ExtGState <<\n/Gs1 9 0 R\n/Gs2 11 0 R\n/Gs3 12 0 R\n>>\n/ColorSpace <<\n>>\n>>\nendobj\n5 0 obj\n<<\n/Length 724\n/Filter /FlateDecode\n>>\nstream\nx��WMo�0\f��W�\u000fL}\u0014EJ\u0
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /tenant/assigned-tenant`

**Tenant đã gán — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

**Response**

```json
{
  "assigned": [],
  "notAssign": [
    {
      "id": "176",
      "name": "Phòng 1",
      "description": null,
      "contactName": null,
      "contactEmail": null,
      "startDate": "2026-08-24",
      "expiredDate": "2027-08-31",
      "isActive": true,
      "blockLimit": 1,
      "noOfUser": 1
    },
    {
      "id": "177",
      "name": "Phòng 2",
      "description": "OS",
      "contactName": null,
      "contactEmail": null,
      "startDate": "2026-09-02",
      "expiredDate": "2026-10-31",
      "isActive": true,
      "blockLimit": 1,
      "noOfUser": 1
    }
  ]
}
```

#### `GET /tenant/{id}`

*Gọi thật: `/tenant/176`*

**Chi tiết một tenant — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

**Response**

```json
{
  "id": "176",
  "name": "Phòng 1",
  "description": null,
  "contactName": null,
  "contactEmail": null,
  "contactPhone": null,
  "minBlocks": 0,
  "maxBlocks": 0,
  "address": null,
  "startDate": "2026-08-24",
  "expiredDate": "2027-08-31",
  "isActive": true,
  "blockLimit": 1,
  "createdAt": "2026-08-24T08:47:08.803Z",
  "updatedAt": "2026-08-25T02:04:51.932Z",
  "deletedAt": null,
  "partnerId": "40"
}
```

#### `GET /tenant/export-excel-usage-detail`

**Xuất chi tiết sử dụng của tenant ra Excel**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

**Response**

```json
"PK\u0003\u0004\n\u0000\u0000\u0000\b\u0000\u001c8#]���\tY\u0001\u0000\u0000�\u0004\u0000\u0000\u0013\u0000\u0000\u0000[Content_Types].xml��Mn�0\u0010��=E�-J\f]TUE¢��\u0016��\u0000�xB,\u001c���w\u0012(�* �`\u0013+�7�{��\u0019O��I�\u0018H;��Q6\u0014\t��)m\u0017������\"�\bV�q\u0016s�C\u0012��n<�y���-墎�?IIe�\rP�<Z�T.4\u0010�6,��r\t\u000b����,��hc\u001a[\u000fQ��X����eˏ�A\u0002\u001a\u0012��^زr\u0001�\u001b]B�\\[���\u001e\b\u0019wv\u001a���\u0001\u000b�<Ih+�\u0001��w�L�\n�\u0019��\u0006\r���ȍ\u000b�/��e�\u0013)]U�\u0012�+W\r�d�\u0003��\u001a16&�֬\u0001m\u0007��NL�[F7\u000er���\u0011�}��z}�Φ\u0007H
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /tenant/export-pdf-usage-detail`

**Xuất chi tiết sử dụng của tenant ra PDF**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

**Response**

```json
"%PDF-1.3\n%����\n9 0 obj\n<<\n/Type /ExtGState\n/ca 0.1\n>>\nendobj\n11 0 obj\n<<\n/Type /ExtGState\n/ca 0.5\n/CA 0.5\n>>\nendobj\n12 0 obj\n<<\n/Type /ExtGState\n/ca 1\n/CA 1\n>>\nendobj\n7 0 obj\n<<\n/Type /Page\n/Parent 1 0 R\n/MediaBox [0 0 841.89 595.28]\n/Contents 5 0 R\n/Resources 6 0 R\n>>\nendobj\n6 0 obj\n<<\n/ProcSet [/PDF /Text /ImageB /ImageC /ImageI]\n/Font <<\n/F1 8 0 R\n/F2 10 0 R\n/F3 13 0 R\n>>\n/ExtGState <<\n/Gs1 9 0 R\n/Gs2 11 0 R\n/Gs3 12 0 R\n>>\n/ColorSpace <<\n>>\n>>\nendobj\n5 0 obj\n<<\n/Length 1399\n/Filter /FlateDecode\n>>\nstream\nx�ś�n�6\u0010��z\n��N�\u001c��\"
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /`

**Health check**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

> Trả chuỗi `"Hello World!"`. Đây là endpoint sống-chết, không phải endpoint dữ liệu.

**Response**

```json
"Hello World!"
```

**Cấu trúc khi có dữ liệu**

```json
// Endpoint hành động — luôn trả đúng giá trị này, không có biến thể.
"Hello World!"
```

#### `GET /user/compact-list-to-share`

**Danh sách người dùng để chia sẻ template — **chỉ End User****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: 403) | `GET` | `200` |

> Có bộ lọc `name` và `email`. Trả `[]` vì **tenant test chỉ có một người dùng** — không phải endpoint hỏng.

**Response**

```json
[]
```

**Cấu trúc khi có dữ liệu**

```json
// Cấu trúc suy từ payload thật của GET /user cùng hệ thống
[
  { "id": "203", "email": "<redacted-email>", "name": "Brian" }
]
```

#### `POST /auth/logout`

**Huỷ phiên hiện tại**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `POST` | `201` |

> ⚠️ Gọi endpoint này **vô hiệu hoá ngay token đang dùng** — mọi lời gọi sau đó trả `401`. Adapter tích hợp không nên gọi nó trừ khi thực sự muốn đóng phiên.



---

## 2.2 Template và bộ chứng từ

> Đây là nhóm quyết định **IDP đọc chứng từ theo khuôn nào**. Với Claim MVP, đây là chỗ khái niệm `templateSetCode` của D3 có tương ứng thật.

#### `GET /template/compact`

**Danh sách rút gọn toàn bộ template — 321 bản ghi**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

Dùng để dựng bảng ánh xạ trong adapter. Payload dưới cắt còn 3 phần tử đầu.

**Response**

```json
[
  {
    "id": "5445",
    "name": "Custom-T5445 - Money transfer request",
    "createdby": null,
    "tenant_id": null,
    "createdat": "2026-08-24T10:04:03.993Z"
  },
  {
    "id": "5444",
    "name": "Custom-T5444 - Sales Invoice",
    "createdby": null,
    "tenant_id": null,
    "createdat": "2026-08-24T10:04:03.980Z"
  },
  {
    "id": "5443",
    "name": "Custom-T5443 - Passport",
    "createdby": null,
    "tenant_id": null,
    "createdat": "2026-08-24T10:04:03.968Z"
  },
  {
    "id": "5442",
    "name": "Custom-T5442 - Driver's License",
    "createdby": null,
    "tenant_id": null,
    "createdat": "2026-08-24T10:04:03.957Z"
  },
  {
    "id": "5441",
    "name": "Custom-T5441 - Resume ",
    "createdby": null,
    "tenant_id": null,
    "createdat": "2026-08-24T10:04:03.950Z"
  },
  {
    "id": "5440",
    "name": "Custom-T5440 - Bachelor's Degree",
    "createdby": null,
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /template/document-set`

**Bộ chứng từ có tên** — tương ứng `templateSetCode` của D3**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

Set 193 *"Chứng từ y tế _ CSYT"* gom 7 template. Nộp cả danh sách `templates[]` này thì IDP tự chọn template khớp.

**Response**

```json
[
  {
    "id": "193",
    "name": "Chứng từ y tế _ CSYT",
    "description": "Bộ chứng từ y tế cho claim_type MEDICAL_CARE (chăm sóc y tế)",
    "templates": [
      "5445",
      "5444",
      "5443",
      "5384",
      "5127",
      "5340",
      "5299"
    ],
    "userId": 203,
    "createdAt": "2026-08-24T10:13:26.676Z",
    "updatedAt": "2026-08-24T10:13:26.676Z",
    "deletedAt": null,
    "partnerId": "40"
  },
  {
    "id": "194",
    "name": "Claim assessment Demo",
    "description": "Claim assessment Demo",
    "templates": [
      "5127",
      "5383",
      "5312",
      "5299",
      "5291",
      "5385",
      "5287"
    ],
    "userId": 203,
    "createdAt": "2026-08-28T03:51:05.573Z",
    "updatedAt": "2026-08-28T03:51:05.573Z",
    "deletedAt": null,
    "partnerId": "40"
  }
]
```

#### `GET /template/document-set/{id}`

*Gọi thật: `/template/document-set/193`*

**Chi tiết một bộ chứng từ**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: 403) | `GET` | `200` |

**Response**

```json
{
  "id": "193",
  "name": "Chứng từ y tế _ CSYT",
  "description": "Bộ chứng từ y tế cho claim_type MEDICAL_CARE (chăm sóc y tế)",
  "templates": [
    "5445",
    "5444",
    "5443",
    "5384",
    "5127",
    "5340",
    "5299"
  ],
  "userId": 203,
  "createdAt": "2026-08-24T10:13:26.676Z",
  "updatedAt": "2026-08-24T10:13:26.676Z",
  "deletedAt": null,
  "partnerId": "40"
}
```

#### `GET /template/{id}`

*Gọi thật: `/template/5384`*

**Định nghĩa field của một template** — đây là master data quan trọng nhất cho việc map sang A4 §8**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

`fields[]` ở đây chính là danh sách khoá mà kết quả trích xuất sẽ trả về trong `fields[].k`. Muốn map sang A4 §8 thì phải đọc bảng này trước.

**Response**

```json
{
  "id": "5384",
  "tenantId": null,
  "code": "Custom-T5384",
  "name": "VNVision-Giấy ra viện/Hospital discharge papers",
  "description": "Giấy ra viện là tài liệu y tế chính thức có tiêu đề in hoa, đậm là \"GIẤY RA VIỆN\". Đặc điểm nhận dạng chính là bố cục dạng danh sách đơn giản, không chứa bảng kê tài chính chi tiết. Tài liệu phải có các mục rõ ràng cho \"Vào viện lúc\", \"Ra viện lúc\", \"Chẩn đoán\", và \"Lời dặn của bác sĩ\". Tài liệu kết thúc bằng hai khối chữ ký riêng biệt của \"Trưởng khoa\" và \"Thủ trưởng đơn vị\" cùng với con dấu của bệnh viện.",
  "category": "Insurance - MBAL",
  "fields": [
    {
      "name": "Place Of Treatment",
      "description": "Tên bệnh viện. Thường nằm ở phía trên bên phải và thường có chữ \"Bệnh viện\""
    },
    {
      "name": "Số vào viện",
      "description": "Số vào viện Thường nằm bên phải của các từ \"Số vào viện\""
    },
    {
      "name": "Tên bệnh nhân",
      "description": "Tên bệnh nhân. Thường nằm bên phải các từ \"Tên bệnh nhân\" hoặc \"Họ và tên\" hoặc \"Tên\". Nếu sai chính tả thì sửa lại cho đúng. Trường hợp lấy được kết quả bị thiếu dấu ở 1 số từ, hãy thêm dấu vào, ví dụ \"Bành Văn Nam\""
    },
    {
      "name": "Tuổi",
      "description": "Tuổi. Số. Thường nằm bên phải các từ \"Tuổi\" hoặc \"số tuổi\",...."
    },
    {
      "name": "Giới tính",
      "description": "Giới tính. Nam hoặc Nữ. Thường nằm bên phải chữ \"Giới tính\",..."
    },
    {
      "name": "Ngày/ tháng/ năm sinh",
      "description": "Ngày/ tháng/ năm sinh, thường nằm ở phía bên phải cụm từ \"Ngày/ tháng/ năm sinh\" hoặc \"Ngày sinh\" hoặc \"NS\". "
    },
    {
      "name": "Dân tộc",
      "description": "Dân tộc. Thường nằm bên phải các từ \"Dân tộc\""
    },
    {
      "name": "Nghề nghiệp",
      "description": "Nghề nghiệp. Thường nằm bên phải các từ \"Nghề nghiệp\"."
    },
    {
      "name": "Số thẻ bảo hiểm y tế",
      "description": "Số thẻ bảo hiểm y tế. số. Thường nằm bên phải dòng chữ \"Số thẻ bảo hiểm y tế\" hoặc \"số thẻ\" hoặc \"số thẻ KCB\" hoặc \"Mã số BHYT\" hoặc \"BHYT\""
    },
    {
      "name": "Thời hạn hiệu lực của thẻ bảo hiểm y tế",
      "description": "Thời hạn hiệu lực của thẻ bảo hiểm y tế. Thường chứa từ \"từ...đến\""
    },
    {
      "name": "Địa chỉ",
      "description": "Địa chỉ. Thường nằm bên phải chữ \"Địa chỉ\", thường có thông tin về thôn, xã, phố, huyện, tỉnh, thành phố."
    },
    {
      "name": "Admission Date",
      "description": "Vào viện lúc. Thời gian và ngày tháng. Thường nằm bên phải các từ \"Vào viện lúc\""
    },
    {
      "name": "Discharge Da
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /template/{id}`

*Gọi thật: `/template/5383`*

**Định nghĩa cột của bảng kê chi phí** — đây là cấu trúc `lines[]` mà IDP sẽ trả**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `GET` | `200` |

> ✅ **Endpoint này cho phép kiểm chứng cấu trúc dòng chi phí mà KHÔNG cần chạy một hoá đơn thật.** `tableFields` khai đúng các cột IDP sẽ trích.
>
> **Nhưng mỗi template một bộ cột khác nhau** — đã đọc bốn template bảng kê:
>
> | Template | Tên | Số cột |
> |---|---|---|
> | `5383` | Bảng kê chi phí khám chữa bệnh | **13 cột** — `Nội dung` · `Đơn vị tính` · `Số lượng` · `Đơn giá BV` · `Đơn giá BH` · `Tỷ lệ thanh toán theo dịch vụ` · `Thành tiền BV` · `Tỷ lệ thanh toán BHYT` · `Thành tiền BH` · `Quỹ BHYT` · `Người bệnh cùng chi trả` · `Khác` · `Người bệnh tự trả` |
> | `5218` | Bảng kê chi phí khám chữa bệnh *(cùng tên, khác bộ cột)* | **7 cột** — `Nội dung` · `Đơn vị tính` · `Số lượng` · `Đơn giá` · `Thanh toán bảo hiểm y tế` · `Người bệnh thanh toán` · `Các phương thức thanh toán khác` |
> | `5125` | Phiếu thanh toán nội trú | **9 cột** — `STT` · `Nội dung` · `Mã` · `Đơn vị` · `Đơn giá` · `Số lượng` · `Thành tiền` · `BHYT trả` · `BN trả` |
> | `5312` | Biên lai thanh toán viện phí | **5 cột** — `STT` · `Tên DV kỹ thuật` · `Số lượng` · `Đơn giá` · `Thành tiền` |
>
> **Ba kết luận cho việc map sang A4 §8 `lines[]`:** tên cột là **nhãn tiếng Việt** chứ không phải khoá máy · **`paidBySocialInsurance` CÓ** dưới ba tên khác nhau (`Quỹ BHYT`, `Thanh toán bảo hiểm y tế`, `BHYT trả`) · **`serviceCategory` thì không template nào có**.
>
> Payload dưới cắt bớt — bản đầy đủ ở `request-response-json/template_cost_statement.json`.

**Response**

```json
{
  "id": "5383",
  "tenantId": null,
  "code": "Custom-T5383",
  "name": "Bảng kê chi phí khám chữa bệnh/List of medical expenses",
  "description": "Bảng kê chi phí khám chữa bệnh là tài liệu phải có tiêu đề chính, in hoa đậm là \"BẢNG KÊ CHI PHÍ KHÁM BỆNH, CHỮA BỆNH...\" (nội trú hoặc ngoại trú). Đặc điểm nhận dạng cốt lõi là một bảng kê tài chính chi tiết, với các cột đặc trưng để phân chia chi phí như  \"Người bệnh cùng chi trả\", và \"Người bệnh tự trả\". Phần cuối tài liệu có phần tổng hợp chi tiết, phân tách rõ ràng số tiền  \"Người bệnh trả\", cùng với nhiều khối chữ ký bao gồm \"KẾ TOÁN VIỆN PHÍ\".",
  "category": "Insurance - MBAL",
  "fields": [
    {
      "name": "Place Of Treatment",
      "description": "Trích xuất tên đơn vị y tế chịu trách nhiệm khám chữa bệnh trong lần điều trị này. Đây là cơ sở trực tiếp thực hiện điều trị nội trú, thường được nêu rõ ở phần đầu văn bản (khác với nơi đăng ký KCB ban đầu được ghi trong bảng). Tuyệt đối không lấy thông tin từ các trường 'Nơi ĐK KCB ban đầu' hay bất kỳ thông tin bệnh viện nào khác nằm trong phần chi tiết điều trị hoặc địa chỉ phụ."
    },
    {
      "name": "Tên bệnh nhân",
      "description": "Tên bệnh nhân. Thường nằm bên phải các từ \"Tên bệnh nhân\", \"Họ và tên\" hoặc \"Tên\". Trường hợp lấy được kết quả bị thiếu dấu ở 1 số từ, hãy thêm dấu vào, ví dụ \"Bành Văn Nam\""
    },
    {
      "name": "Địa chỉ",
      "description": "Địa chỉ. Thường nằm bên phải dòng chữ \"Địa chỉ\""
    },
    {
      "name": "Ngày sinh",
      "description": "Ngày sinh bệnh nhân, Thường nằm bên phải cụm từ \"Ngày sinh:\" hoặc \"Ngày, tháng, năm sinh:\""
    },
    {
      "name": "Giới tính",
      "description": "Giới tính của người bệnh. Thường nằm bên phải chữ \"Giới tính:\" hoặc \"Giới:\""
    },
    {
      "name": "Số CCCD",
      "description": "Số căn cước của bệnh nhân, thường nằm sau cụm từ \"Giấy tờ tùy thân\" hoặc \"Số CCCD\" hoặc \"Số CMND\"."
    },
    {
      "name": "Thẻ bảo hiểm y tế",
      "description": "Thẻ bảo hiểm y tế. Là checkbox. Thường nằm bên phải của từ \"Bảo hiểm y tế\" hoặc bên phải cụm từ \"Có BHYT\". Nếu checkbox được chọn, nó sẽ hiển thị \"có\", và nếu checkbox không được chọn, nó sẽ hiển thị là \"không\""
    },
    {
      "name": "Số bảo hiểm y tế",
      "description": "Số bảo hiểm y tế. Số. Thường nằm bên phải dòng chữ \"Số bảo hiểm y tế\"."
    },
    {
      "name": "Không có thẻ bảo hiểm y tế",
      "description": "Không có thẻ bảo hiểm y tế. Là checkbox. Thường nằm bên phải của từ \"Bảo hiểm y tế\" hoặc bên phải cụm từ \"không có BHYT\". Nếu checkbox được chọn, nó sẽ hiển thị \"có\", và nếu checkbox không được chọn, nó sẽ hiển thị là \"không\""
    },
    {
      "name": "Cơ sở đăng ký bảo hiểm y tế ban đầu",
      "description": "Cơ sở đăng ký bảo hiểm y tế ban đầu. Thường nằm bên phải dòng chữ “Cơ sở đăng ký bảo hiểm y tế ban đầu”."
    },
    {
      "name": "Mã số",
      "description": "Mã số. Thường nằm bên phải chữ \"Mã số\"."
    },
    {
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /template/assigned-template`

**Template đã gán / chưa gán cho tenant**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Payload cắt bớt — 321 bản ghi.

**Response**

```json
{
  "assigned": [],
  "notAssigned": [
    {
      "id": "5125",
      "name": "Phiếu thanh toán nội trú",
      "category": "Giấy tờ",
      "createdAt": "2026-08-24T09:01:35.340Z",
      "updatedAt": "2026-08-24T10:04:01.682Z"
    },
    {
      "id": "5126",
      "name": "Phiếu mua thuốc",
      "category": "MBAL",
      "createdAt": "2026-08-24T09:01:35.357Z",
      "updatedAt": "2026-08-24T10:04:01.695Z"
    },
    {
      "id": "5127",
      "name": "Giấy ra viện/Hospital discharge papers",
      "category": "Chứng từ y tế",
      "createdAt": "2026-08-24T09:49:54.865Z",
      "updatedAt": "2026-08-24T09:49:54.865Z"
    },
    {
      "id": "5128",
      "name": "Biên bản giám định y khoa",
      "category": "MBAL",
      "createdAt": "2026-08-24T10:03:56.349Z",
      "updatedAt": "2026-08-24T10:03:56.349Z"
    },
    {
      "id": "5129",
      "name": "Other",
      "category":
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /template?page={n}&limit={m}`

*Gọi thật: `/template?page=1&limit=5`*

**Danh sách template có phân trang, lọc, sắp xếp**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: 403) | `GET` | `200` |

Khác `GET /template/compact` ở chỗ trả đầy đủ thông tin từng template, có phân trang.

**Response**

```json
{
  "items": [
    {
      "id": "5445",
      "tenantId": null,
      "code": "Custom-T5445",
      "name": "Money transfer request",
      "description": "Extract information from money transfer request ",
      "category": "BANK - Payment ",
      "templateRefId": "14",
      "type": 1,
      "createdBy": null,
      "partnerId": "40",
      "createdAt": "2026-08-24T10:04:03.993Z",
      "updatedAt": "2026-08-24T10:04:03.993Z",
      "deletedAt": null,
      "templateShares": [],
      "user": null
    },
    {
      "id": "5444",
      "tenantId": null,
      "code": "Custom-T5444",
      "name": "Sales Invoice",
      "description": "Extract information from Sales invoice ",
      "category": "OTHR - Sales",
      "templateRefId": "19",
      "type": 1,
      "createdBy": null,
      "partnerId": "40",
      "createdAt": "2026-08-24T10:04:03.980Z",
      "updatedAt": "2026-08-24T10:04:03.980Z",
      "deletedAt": null,
      "templateShares": [],
      "user": null
    },
    {
      "id": "5443",
      "tenantId": null,
      "code": "Custom-T5443",
      "name": "Passport",
      "description": "Extract information from passport",
      "category": "PERS - Personal documents",
      "templateRefId": "21",
      "type": 1,
      "createdBy": null,
      "partnerId": "40",
      "createdAt": "2026-08-24T10:04:03.968Z",
      "updatedAt": "2026-08-24T10:04:03.968Z",
      "deletedAt": null,
      "templateShares": [],
      "user": null
    },
    {
      "id": "5442",
      "tenantId": null,
      "code": "Custom-T5442",
      "name": "Driver's License",
      "description": "Extract information from driver's license",
      "category": "PERS - Personal documents",
      "templateRefId": "23",
      "type": 1,
      "createdBy": null,
      "partnerId": "40",
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /template/partner-templates`

**Template thuộc partner — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

**Response**

```json
{
  "items": [
    {
      "id": "5445",
      "tenantId": null,
      "code": "Custom-T5445",
      "name": "Money transfer request",
      "description": "Extract information from money transfer request ",
      "category": "BANK - Payment ",
      "templateRefId": "14",
      "type": 1,
      "createdBy": null,
      "partnerId": "40",
      "createdAt": "2026-08-24T10:04:03.993Z",
      "updatedAt": "2026-08-24T10:04:03.993Z",
      "deletedAt": null,
      "user": null
    },
    {
      "id": "5444",
      "tenantId": null,
      "code": "Custom-T5444",
      "name": "Sales Invoice",
      "description": "Extract information from Sales invoice ",
      "category": "OTHR - Sales",
      "templateRefId": "19",
      "type": 1,
      "createdBy": null,
      "partnerId": "40",
      "createdAt": "2026-08-24T10:04:03.980Z",
      "updatedAt": "2026-08-24T10:04:03.980Z",
      "deletedA
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /template/compact-for-partner`

**Template rút gọn theo partner — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

**Response**

```json
[
  {
    "id": "721",
    "name": "Custom-T721 - 2",
    "tenant_id": "7"
  },
  {
    "id": "613",
    "name": "Custom-T613 - 1",
    "tenant_id": "7"
  },
  {
    "id": "723",
    "name": "Custom-T723 - B1 _ Giấy tờ nhà - 2 chủ sở hữu",
    "tenant_id": "5"
  },
  {
    "id": "614",
    "name": "Custom-T614 - 2",
    "tenant_id": "7"
  },
  {
    "id": "615",
    "name": "Custom-T615 - Đơn yêu cầu hoàn phí bảo hiểm/Insurance Refund Request Form",
    "tenant_id": "7"
  },
  {
    "id": "625",
    "name": "Custom-T625 - RPA - CCCD",
    "tenant_id": "7"
  },
  {
    "id": "630",
    "name": "Custom-T630 - Ủy nhiệm chi",
    "tenant_id": "7"
  },
  {
    "id": "106",
    "name": "Custom-T106 - Vietbank - Giấy báo Thẻ tiết kiệm bị mất, nhàu nát, rách",
    "tenant_id": null
  },
  {
    "id": "120",
    "name": "Custom-T120 - Tien.do IDP test",
    "tenant_id": null
  },
  {
    "id": "1
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /template/created-by-compact`

**Danh sách người tạo template**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

**Response**

```json
[
  {
    "id": "202",
    "name": "<redacted-email>"
  }
]
```

#### `GET /template/fill-owner`

**Điền `created_by` còn trống cho template**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

> Đây là **lệnh bảo trì dữ liệu**, không phải endpoint đọc — swagger ghi *"Fill null created_by field by assigned"*. Trả chuỗi rỗng là đúng: nó chạy xong và không có gì để trả.

**Response**

```json
""
```

**Cấu trúc khi có dữ liệu**

```json
// Endpoint hành động — luôn trả đúng giá trị này, không có biến thể.
""   // chuỗi rỗng — lệnh chạy xong, không có gì để trả
```

#### `POST /template/document-set`

**Tạo một bộ chứng từ có tên** — đây là cách hiện thực `templateSetCode` của D3**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `POST` | `201` |

> ✅ **Đã chạy thật và tạo được bộ `HEALTH_CLAIM_V1` (id 195).** Đây là bằng chứng trực tiếp rằng khái niệm `templateSetCode = "HEALTH_CLAIM_V1"` của D3 I-07 **hiện thực được bằng cơ chế sẵn có của IDP**, không cần vendor thay đổi gì.
>
> Sau khi tạo, adapter chỉ cần đọc `templates[]` của bộ này rồi nộp cả danh sách — IDP tự chọn template khớp.

**Request**

```json
{
  "name": "HEALTH_CLAIM_V1",
  "description": "Bộ chứng từ sức khoẻ MVP1 — tạo để kiểm chứng đường ánh xạ templateSetCode của D3 I-07",
  "templates": [
    5384,
    5127,
    5299,
    5125,
    5126,
    5130,
    5128
  ]
}
```

**Response**

```json
{
  "name": "HEALTH_CLAIM_V1",
  "description": "Bộ chứng từ sức khoẻ MVP1 — tạo để kiểm chứng đường ánh xạ templateSetCode của D3 I-07",
  "templates": [
    5384,
    5127,
    5299,
    5125,
    5126,
    5130,
    5128
  ],
  "userId": "203",
  "partnerId": "40",
  "id": "195",
  "createdAt": "2026-09-03T07:09:57.829Z",
  "updatedAt": "2026-09-03T07:09:57.829Z",
  "deletedAt": null
}
```

#### `GET /template/sync-bo-templates?override={bool}`

*Gọi thật: `/template/sync-bo-templates?override=false`*

**Đồng bộ template từ BO xuống FO — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

> Tham số `override` là **bắt buộc**; thiếu nó thì trả chuỗi rỗng chứ không báo lỗi. Đây là **lệnh đồng bộ**, không phải endpoint đọc — trả rỗng là đúng.

**Response**

```json
""
```

**Cấu trúc khi có dữ liệu**

```json
// Endpoint hành động — luôn trả đúng giá trị này, không có biến thể.
""   // chuỗi rỗng — lệnh đồng bộ chạy xong
```

#### `GET /template/get-sync-templates-bo`

**Xem template nào có thể đồng bộ từ BO — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

**Response**

```json
"[{\"id\":\"232\",\"name\":\"Biên bản giám định y khoa\",\"code\":\"Custom-T232\",\"description\":\"Biên bản giám định y khoa là một văn bản y-pháp có tiêu đề chính là \\\"BIÊN BẢN GIÁM ĐỊNH Y KHOA\\\", được cấp bởi một \\\"Hội đồng Giám định Y khoa\\\" chứ không phải một bệnh viện thông thường. Đặc điểm nhận dạng cốt lõi là tài liệu này có mục đích rõ ràng là để giám định (\\\"Xác định mức độ khuyết tật\\\") và phải chứa một phần \\\"KẾT LUẬN\\\" cuối cùng, nơi đưa ra phán quyết chính thức về tình trạng khuyết tật. Tài liệu có thể có ảnh thẻ của người được giám định và được ký bởi các thành viên của hội đồng.\",\"category\":\"MBAL\",\"fields\":[{\"name\":\"Nơi xác nhận\",\"description\":\"Nơi xác nhận giấy khai tử, trích xuất thông tin tên cơ quan thực hiện xác nhận, thường nằm ở góc trên bên trái tài liệu. Thường bao gồm thông tin hội đồng và sở y tế.\"},{\"name\":\"Số\",\"description\":\"Số vào sổ, thưởng năm sau chữ \\\"Số:\\\"\"},{\"name\":\"Thời gian\",\"description\":\"Thời gian cấp giấy, thường nằm cùng hàng với Số vào sổ.\"},{\"name\":\"Họ và tên\",\"description\":\"Họ tên, thường nằm sau chữ \\\"Họ, chữ đệm, tên\\\" hoặc \\\"Ông/Bà\\\" hoặc \\\"Cháu hoặc dưới chữ \\\"khám giám định y khoa đối với:\\\". Trường hợp lấy được kết quả bị thiếu dấu ở 1 số từ, hãy thêm dấu vào, ví dụ \\\"Bành Văn Nam\\\"\"},{\"name\":\"Ngày, tháng, năm sinh\",\"description\":\"Ngày, tháng, n
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /template/document-set-list`

**Danh sách bộ chứng từ dạng phân trang — **bắt buộc có tham số `sort`****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **End User** — nhưng endpoint đang lỗi `400` | `GET` | `400` |

> ⚠️ **Gọi không kèm `sort` thì trả `400 · missing FROM-clause entry for table "template"`.** Nguyên nhân nằm trong code: nhánh mặc định chạy `orderBy('template.id')` trong khi alias của query là `document_sets` — một lỗi SQL của IDP.
>
> ✅ **Cách tránh: luôn truyền `sort`.** Đã thử `sort=id`, `sort=name`, `sort=-id`, `sort=-createdAt` — đều trả `200`. Payload dưới dùng `sort=-id`.
>
> Vai Admin trả `403` vì không có quyền trên nhóm template.

**Response**

```json
{
  "statusCode": 400,
  "message": "missing FROM-clause entry for table \"template\""
}
```


---

## 2.3 Ánh xạ MBAL — loại claim, loại chứng từ, bộ chứng từ

> Nhóm này là **cầu nối giữa từ vựng nghiệp vụ bảo hiểm và template của IDP**. Với Claim MVP, đây là nơi đặt bảng ánh xạ `claim_type` → bộ chứng từ.

#### `GET /mbal/docset-map`

**Ánh xạ `claim_type` + `document_type` → bộ chứng từ**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

Đây là bảng cho phép hệ thống claim chỉ cần biết loại claim, không cần biết templateId.

**Response**

```json
{
  "items": [
    {
      "id": "77",
      "documentSetId": "193",
      "claim_type": "MEDICAL_CARE",
      "document_type": "CL10",
      "partnerId": 40,
      "documentSet": {
        "id": "193",
        "name": "Chứng từ y tế _ CSYT",
        "description": "Bộ chứng từ y tế cho claim_type MEDICAL_CARE (chăm sóc y tế)",
        "templates": [
          "5445",
          "5444",
          "5443",
          "5384",
          "5127",
          "5340",
          "5299"
        ],
        "userId": 203,
        "createdAt": "2026-08-24T10:13:26.676Z",
        "updatedAt": "2026-08-24T10:13:26.676Z",
        "deletedAt": null,
        "partnerId": "40"
      }
    }
  ],
  "totalItems": 1
}
```

#### `GET /mbal/template-map`

**Ánh xạ templateId → mã loại chứng từ (`CL05`, `CL10` …)**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Chính là nguồn của `subDocumentType` trong kết quả trích xuất. Đây là bảng phải nạp đủ nếu muốn map sang mã `T1-01` của A4 §3.

**Response**

```json
{
  "items": [
    {
      "id": "15",
      "templateId": "5127",
      "sub_document_type": "CL05",
      "partnerId": 40,
      "template": {
        "id": "5127",
        "tenantId": null,
        "code": "Custom-T5127",
        "name": "Giấy ra viện/Hospital discharge papers",
        "description": "Giấy ra viện là tài liệu y tế chính thức có tiêu đề in hoa, đậm là \"GIẤY RA VIỆN\". Đặc điểm nhận dạng chính là bố cục dạng danh sách đơn giản, không chứa bảng kê tài chính chi tiết. Tài liệu phải có các mục rõ ràng cho \"Vào viện lúc\", \"Ra viện lúc\", \"Chẩn đoán\", và \"Lời dặn của bác sĩ\". Tài liệu kết thúc bằng hai khối chữ ký riêng biệt của \"Trưởng khoa\" và \"Thủ trưởng đơn vị\" cùng với con dấu của bệnh viện.",
        "category": "Chứng từ y tế",
        "fields": [
          {
            "name": "Place Of Treatment",
            "description": "Tên bệnh viện. Thường nằm ở phía trên bên phải và thường có chữ \"Bệnh viện\""
          },
          {
            "name": "Số vào viện",
            "description": "Số vào viện. Thường nằm bên phải của các từ \"Số vào viện\""
          },
          {
            "name": "Tên bệnh nhân",
            "description": "Tên bệnh nhân. Thường nằm bên phải các từ \"Tên bệnh nhân\" hoặc \"Họ và tên\" hoặc \"Tên\". Nếu sai chính tả thì sửa lại cho đúng"
          },
          {
            "name": "Tuổi",
            "description": "Tuổi. Số. Thường nằm bên phải các từ \"Tuổi\" hoặc \"số tuổi\""
          },
          {
            "name": "Giới tính",
            "description": "Giới tính. Nam hoặc Nữ. Thường nằm bên phải chữ \"Giới tính\""
          },
          {
            "name": "Ngày/ tháng/ năm sinh",
            "description": "Thường nằm ở phía bên phải cụm từ \"Ngày/ tháng/ năm sinh\" hoặc \"Ngày sinh\" hoặc \"NS\""
          },
          {
            "name": "Số thẻ bảo hiểm y tế",
            "description": "Số thẻ bảo hiểm y tế của bệnh nhân"
          },
          {
            "name": "Địa chỉ",
            "description": "Địa chỉ của bệnh nhân"
          },
          {
            "name": "Admission Date",
            "description": "Vào viện lúc. Thời gian và ngày tháng. Thường nằm bên phải các từ \"Vào viện lúc\""
          },
          {
            "name": "Discharge Date",
            "description": "Xuất viện lúc. Thường nằm bên phải các từ \"Xuất viện lúc\" hoặc \"Rời bệnh viện lúc\""
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /mbal/docsets`

**Danh sách bộ chứng từ (rút gọn)**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

**Response**

```json
[
  {
    "id": "193",
    "name": "Chứng từ y tế _ CSYT",
    "partnerId": "40"
  },
  {
    "id": "194",
    "name": "Claim assessment Demo",
    "partnerId": "40"
  }
]
```

#### `GET /mbal/templates`

**Danh sách template MBAL kèm `code` và `name`**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Payload cắt bớt.

**Response**

```json
[
  {
    "id": "5125",
    "code": "Custom-T5125",
    "name": "Phiếu thanh toán nội trú",
    "partnerId": "40"
  },
  {
    "id": "5126",
    "code": "Custom-T5126",
    "name": "Phiếu mua thuốc",
    "partnerId": "40"
  },
  {
    "id": "5127",
    "code": "Custom-T5127",
    "name": "Giấy ra viện/Hospital discharge papers",
    "partnerId": "40"
  },
  {
    "id": "5129",
    "code": "Custom-T5129",
    "name": "Other",
    "partnerId": "40"
  },
  {
    "id": "5128",
    "code": "Custom-T5128",
    "name": "Biên bản giám định y khoa",
    "partnerId": "40"
  },
  {
    "id": "5130",
    "code": "Custom-T5130",
    "name": "VNVision-Giấy chứng nhận phẫu thuật",
    "partnerId": "40"
  },
  {
    "id": "5133",
    "code": "Custom-T5133",
    "name": "Đơn bổ sung cho yêu cầu giải quyết quyền lợi bảo hiểm",
    "partnerId": "40"
  },
  {
    "id": "5135",
    "code": "Custom-T5135",
    "name": "VNVision-Bản tường trình",
    "partnerId": "40"
  },
  {
    "id": "5137",
    "code": "Custom-T5137",
    "name": "Trích lục khai sinh",
    "partnerId": "40"
  },
  {
    "id": "5138",
    "code": "Custom-T5138",
    "name": "Phiếu thanh toán ra viện",
    "partnerId": "40"
  },
  {
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /mbal/statistics`

**Thống kê theo request**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Payload cắt bớt.

**Response**

```json
"PK\u0003\u0004\n\u0000\u0000\u0000\b\u0000�2#]�s�Z\\\u0001\u0000\u0000x\u0005\u0000\u0000\u0013\u0000\u0000\u0000[Content_Types].xml�T�n�0\u0010��+\"_Qb�PU\u0015�Ci�-R�\u0007��X8��\u0019���$PTU,B q�\u0015��<�g0Z�6YBD�].zYW$�\n�����k��>�\u0004I9��w��\r�\u0018\r\u001f\u0006�M\u0000L��0\u0017\u0015Qx�\u0012�\nj��\u000f�R�X+��8�A\u0015s5\u0003��v\u001fe�\u001d���\u001a\r1\u001c��T\u000bK�뚷�A\"X\u0014��\u0016�x�B�`M���r��?�t�1��`e\u0002v\u0018 �A��r�`����D�!��H�f�\\[��q���<;-r �/KS��ŢfJ�!��X\u0001Pm�v�je\\�\u000bF�.�\u001b\u0007��_���\u001c��\u001dl�׷��9c���������9�JEП\u0014���<�_�\u0013Wgy�+��Q����A\u0019:�> ��\b���w\u00164�4�\u0010D2�[�wd��\nM�4�\u0003޲\u001d��\u001fPK\u0003\u0004\n\u0000\u0000\u0000\u0000\u0000�2#]\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0006\u0000\u0000\u0000_rels/PK\u0003\u0004\n\u0000\u0000\u0000\b\u0000�2#]�I��\u0000\u0000\u0000K\u00
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `POST /mbal/docset-map`

**Nối `claim_type` + `document_type` vào một bộ chứng từ**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `POST` | `201` |

> ✅ Đã chạy thật: `MEDICAL_ILLNESS` + `CL06` → bộ `HEALTH_CLAIM_V1` (id 195). Sau bước này, hệ thống claim nộp chứng từ chỉ cần từ vựng nghiệp vụ, không cần biết `templateId` nào.

**Request**

```json
{
  "documentSetId": 195,
  "claim_type": "MEDICAL_ILLNESS",
  "document_type": "CL06"
}
```

**Response**

```json
{
  "documentSetId": 195,
  "claim_type": "MEDICAL_ILLNESS",
  "document_type": "CL06",
  "partnerId": "40",
  "id": "78"
}
```

#### `POST /mbal/template-map`

**Nạp ánh xạ template → mã loại chứng từ**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `POST` | `201` |

> ✅ Đã chạy thật: template `5384` → `CL10`.
>
> **Đây chính là lý do `subDocumentType` trả `null` ở lần trích xuất đầu tiên** — bảng ánh xạ chưa có bản ghi. Xem bằng chứng ở mục kế tiếp.

**Request**

```json
{
  "templateId": 5384,
  "sub_document_type": "CL10"
}
```

**Response**

```json
{
  "templateId": 5384,
  "sub_document_type": "CL10",
  "partnerId": "40",
  "id": "16"
}
```

#### `POST /mbal/create-request`

**Nộp bằng cấu hình vừa tạo** — kiểm chứng vòng khép kín**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `POST` | `201` |

> ✅ Nộp với `claim_type: MEDICAL_ILLNESS` + `document_type: CL06`, backend tự tra ra bộ `HEALTH_CLAIM_V1` và nộp thành công. Toàn bộ đường cấu hình chạy đúng từ đầu đến cuối.

**Request**

```json
{
  "name": "VERIFY-…",
  "claim_type": "MEDICAL_ILLNESS",
  "document_type": "CL06",
  "document": "<DP_DATA_case1.pdf>"
}
```

**Response**

```json
{
  "pageCount": 1,
  "requestId": 21383
}
```

#### `GET /mbal/docset-map/{id}`

*Gọi thật: `/mbal/docset-map/77`*

**Chi tiết một ánh xạ docset**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

**Response**

```json
{
  "id": "77",
  "documentSetId": "193",
  "claim_type": "MEDICAL_CARE",
  "document_type": "CL10",
  "partnerId": 40,
  "documentSet": {
    "id": "193",
    "name": "Chứng từ y tế _ CSYT",
    "description": "Bộ chứng từ y tế cho claim_type MEDICAL_CARE (chăm sóc y tế)",
    "templates": [
      "5445",
      "5444",
      "5443",
      "5384",
      "5127",
      "5340",
      "5299"
    ],
    "userId": 203,
    "createdAt": "2026-08-24T10:13:26.676Z",
    "updatedAt": "2026-08-24T10:13:26.676Z",
    "deletedAt": null,
    "partnerId": "40"
  }
}
```

#### `GET /mbal/template-map/{id}`

*Gọi thật: `/mbal/template-map/15`*

**Chi tiết một ánh xạ template — kèm định nghĩa field đầy đủ**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Bản ghi này cho thấy `description` của template chính là **chữ ký phân loại** mà IDP dùng để nhận dạng chứng từ — tương ứng khái niệm classification signature ở A4 §3.

**Response**

```json
{
  "id": "15",
  "templateId": "5127",
  "sub_document_type": "CL05",
  "partnerId": 40,
  "template": {
    "id": "5127",
    "tenantId": null,
    "code": "Custom-T5127",
    "name": "Giấy ra viện/Hospital discharge papers",
    "description": "Giấy ra viện là tài liệu y tế chính thức có tiêu đề in hoa, đậm là \"GIẤY RA VIỆN\". Đặc điểm nhận dạng chính là bố cục dạng danh sách đơn giản, không chứa bảng kê tài chính chi tiết. Tài liệu phải có các mục rõ ràng cho \"Vào viện lúc\", \"Ra viện lúc\", \"Chẩn đoán\", và \"Lời dặn của bác sĩ\". Tài liệu kết thúc bằng hai khối chữ ký riêng biệt của \"Trưởng khoa\" và \"Thủ trưởng đơn vị\" cùng với con dấu của bệnh viện.",
    "category": "Chứng từ y tế",
    "fields": [
      {
        "name": "Place Of Treatment",
        "description": "Tên bệnh viện. Thường nằm ở phía trên bên phải và thường có chữ \"Bệnh viện\""
      },
      {
        "name": "Số vào viện",
        "description": "Số vào viện. Thường nằm bên phải của các từ \"Số vào viện\""
      },
      {
        "name": "Tên bệnh nhân",
        "description": "Tên bệnh nhân. Thường nằm bên phải các từ \"Tên bệnh nhân\" hoặc \"Họ và tên\" hoặc \"Tên\". Nếu sai chính tả thì sửa lại cho đúng"
      },
      {
        "name": "Tuổi",
        "description": "Tuổi. Số. Thường nằm bên phải các từ \"Tuổi\" hoặc \"số tuổi\""
      },
      {
        "name": "Giới tính",
        "description": "Giới tính. Nam hoặc Nữ. Thường nằm bên phải chữ \"Giới tính\""
      },
      {
        "name": "Ngày/ tháng/ năm sinh",
        "description": "Thường nằm ở phía bên phải cụm từ \"Ngày/ tháng/ năm sinh\" hoặc \"Ngày sinh\" hoặc \"NS\""
      },
      {
        "name": "Số thẻ bảo hiểm y tế",
        "description": "Số thẻ bảo hiểm y tế của bệnh nhân"
      },
      {
        "name": "Địa chỉ",
        "description": "Địa chỉ của bệnh nhân"
      },
      {
        "name": "Admission Date",
        "description": "Vào viện lúc. Thời gian và ngày tháng. Thường nằm bên phải các từ \"Vào viện lúc\""
      },
      {
        "name": "Discharge Date",
        "description": "Xuất viện lúc. Thường nằm bên phải các từ \"Xuất viện lúc\" hoặc \"Rời bệnh viện lúc\""
      },
      {
        "name": "Diagnosis Codes",
        "description": "Mã ICD, thường nằm bên phải cụm từ \"Mã ICD\" hoặc là phần mã gồm 1 chữ cái đầu và các số phía sau"
      },
      {
        "name": "Kết quả chẩn đoán",
        "description": "Kết quả chẩn đoán và kết luận của bác sĩ"
      },
      {
        "name": "Phương pháp điều trị",
        "description": "Phương pháp điều trị mà bác sĩ chỉ định"
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```


---

## 2.4 Nộp chứng từ và trích xuất

> Nhóm trung tâm của bước J2-9 và J2-10. **Toàn bộ nhóm này chỉ End User gọi được** — `RequestController` gắn quyền `Tenant` ở cấp class, tài khoản Admin trả `403`.

#### `POST /request/group`

**Tạo nhóm chứng từ cho một claim**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** | `POST` | `201` |

Một claim là một nhóm. Nhóm là thứ cho phép poll một lần cho cả gói ở bước sau. `id` là số tăng dần theo tenant.

**Request**

```json
{}
```

**Response**

```json
{
  "id": "4",
  "createdAt": "2026-09-03T04:56:00.735Z"
}
```

#### `POST /request`

**Nộp chứng từ** — multipart**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** | `POST` | `201` |

Field bắt buộc: `name`, `templates`, `document`. Field tuỳ chọn: `requestGroupId`, `separatorType`, `separatorValue`, `deleteFile`, `extraParams`.

> ⚠️ **`templates` là chuỗi ngăn cách bởi dấu phẩy**, ví dụ `"5384,5127,5299"` — **không phải JSON array**. Gửi array thì lỗi `400 · invalid input syntax for type bigint: "NaN"`. Swagger khai là `array`, đây là chỗ dễ sai nhất.

> Mẹo: nhét `claimNo` / `eventNo` vào `name` — đây là trường duy nhất mang được định danh của hệ thống claim sang IDP.

**Request**

```json
{
  "name": "CLM-TEST-1788411360",
  "templates": "5445,5444,5443,5384,5127,5340,5299",
  "requestGroupId": "4",
  "document": "<file DP_DATA_case1.pdf, 45091 bytes, application/pdf>"
}
```

**Response**

```json
{
  "pageCount": 1,
  "requestId": 21376
}
```

#### `POST /request/sync`

**Nộp và nhận kết quả trong CÙNG một lời gọi** — biến thể đồng bộ**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `POST` | `201` |

> **Đây có thể là endpoint phù hợp nhất cho Claim Platform.** Nó gộp `POST /request` + poll + `GET /result` thành một lời gọi, và trả `confidence` ngay ở tầng ngoài. Đổi lại là phải chờ đồng bộ (~16 giây cho 1 trang), nên chỉ hợp khi số trang nhỏ.

Cùng bộ field với `POST /request`.

**Request**

```json
{
  "name": "SYNC-1788416655",
  "templates": "5384,5127,5299",
  "document": "<file DP_DATA_case1.pdf, 45091 bytes, application/pdf>"
}
```

**Response**

```json
{
  "id": "21503",
  "requestId": "21378",
  "confidence": "97.81",
  "result": {
    "result": {
      "errorCode": null,
      "minioPath": "f33c99bf-a9ff-43fb-9fda-3831dd2dbc0e.pdf",
      "minioBucket": "idp-gateway-bucket",
      "refTenantId": "176",
      "errorMessage": null,
      "refPartnerId": "78",
      "refRequestId": "21378",
      "processingTime": 16.57,
      "templates_data": [
        {
          "data": {
            "fields": [
              {
                "c": 0.9,
                "k": "Place Of Treatment",
                "v": "Benh vien Noi tiet Trung uong",
                "c_llm": 0,
                "match_w": "Ho so: Nguyen Hoang Ha - Benh vien Noi tiet Trung uong - 03/2025",
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Số vào viện",
                "v": "",
                "c_llm": 0,
                "match_w": null,
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Tên bệnh nhân",
                "v": "NGUYỄN VĂN A",
                "c_llm": 0,
                "match_w": "NGUYỄN VĂN A",
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Tuổi",
                "v": "",
                "c_llm": 0,
                "match_w": null,
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Giới tính",
                "v": "FEMALE",
                "c_llm": 0,
                "match_w": "FEMALE",
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Ngày/ tháng/ năm sinh",
                "v": "1990-01-01",
                "c_llm": 0,
                "match_w": "1990-01-01",
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Số thẻ bảo hiểm y tế",
                "v": "DN0000000000000",
                "c_llm": 0,
                "match_w": ": DN0000000000000",
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Địa chỉ",
                "v": "",
                "c_llm": 0,
                "match_w": null,
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Admission Date",
                "v": "20/03/2025",
                "c_llm": 0,
                "match_w": "20/03/2025",
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Discharge Date",
                "v": "26/03/2025",
                "c_llm": 0,
                "match_w": "20/03/2025",
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Diagnosis Codes",
                "v": "",
                "c_llm": 0,
                "match_w": null,
                "page_n
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `POST /mbal/create-request`

**Nộp theo `claim_type` + `document_type`** — không cần biết templateId**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `POST` | `201` |

Backend tự tra `mbal/docset-map` theo `(claim_type, document_type, partnerId)`, lấy `documentSet.templates` rồi gọi tiếp như `POST /request`.

> Với hệ thống claim, đây là dạng gọi **gần nghiệp vụ nhất**: chỉ cần biết loại claim và loại chứng từ. Giá trị hợp lệ lấy từ master data ở §2.6.

**Request**

```json
{
  "name": "VERIFY-…",
  "claim_type": "MEDICAL_ILLNESS",
  "document_type": "CL06",
  "document": "<DP_DATA_case1.pdf>"
}
```

**Response**

```json
{
  "pageCount": 1,
  "requestId": 21383
}
```

#### `GET /request?page={n}&limit={m}`

*Gọi thật: `/request?page=1&limit=3`*

**Danh sách request có phân trang, lọc, sắp xếp**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: 403) | `GET` | `200` |

Tham số: `page`, `limit`, `search`, `fromDate`, `toDate`, `sort` (`id`, `templateId`, `name`, `status`, `results.confidence`, `pageCount`, `createdAt`; thêm dấu trừ để đảo chiều), `templateId`.

**Response**

```json
{
  "items": [
    {
      "id": "21376",
      "tenantId": "176",
      "partnerId": "40",
      "name": "CLM-TEST-1788411360",
      "userId": "203",
      "templateId": null,
      "templates": [
        5445,
        5444,
        5443,
        5384,
        5127,
        5340,
        5299
      ],
      "status": 2,
      "isDownloaded": false,
      "deleteFile": false,
      "pageCount": 1,
      "createdAt": "2026-09-03T04:56:01.324Z",
      "updatedAt": "2026-09-03T04:56:19.385Z",
      "deletedAt": null,
      "requestGroupId": "4",
      "extraParams": {},
      "results": {
        "confidence": "98.90"
      }
    },
    {
      "id": "21375",
      "tenantId": "176",
      "partnerId": "40",
      "name": "RETRY-1788411219",
      "userId": "203",
      "templateId": null,
      "templates": [
        5445,
        5444,
        5443,
        5384,
        5127,
        5340,
        5299
      ],
      "status": 3,
      "isDownloaded": false,
      "deleteFile": false,
      "pageCount": 1,
      "createdAt": "2026-09-03T04:53:40.363Z",
      "updatedAt": "2026-09-03T04:53:40.695Z",
      "deletedAt": null,
      "requestGroupId": "3",
      "extraParams": {},
      "results": {
        "confidence": "0.00"
      }
    },
    {
      "id": "21374",
      "tenantId": "176",
      "partnerId": "40",
      "name": "CLM-TEST-1788410518",
      "userId": "203",
      "templateId": null,
      "templates": [
        5445,
        5444,
        5443,
        5384,
        5127,
        5340,
        5299
      ],
      "status": 3,
      "isDownloaded": false,
      "deleteFile": false,
      "pageCount": 1,
      "createdAt": "2026-09-03T04:41:59.291Z",
      "updatedAt": "2026-09-03T04:41:59.617Z",
      "deletedAt": null,
      "requestGroupId": "2",
      "extraParams": {},
      "results": {
        "confidence": "0.00"
      }
    }
  ],
  "totalItems": 48
}
```

#### `GET /request/{id}/check-status`

*Gọi thật: `/request/21376/check-status`*

**Trạng thái một request — và khi xong thì trả kèm LUÔN kết quả**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

> Tiện ích không có trong tài liệu nào: khi `requestStatus = Completed`, response mang cả kết quả trong trường `data`. Poll bằng endpoint này thì không cần gọi thêm `/result`.

**Response**

```json
{
  "requestId": 21376,
  "requestStatus": "Completed",
  "data": {
    "result": {
      "errorCode": null,
      "minioPath": "a1d86331-9c2d-4c43-99e9-3ad7185bdf5d.pdf",
      "minioBucket": "idp-gateway-bucket",
      "refTenantId": "176",
      "errorMessage": null,
      "refPartnerId": "78",
      "refRequestId": "21376",
      "processingTime": 16.14,
      "templates_data": [
        {
          "data": {
            "fields": [
              {
                "c": 1,
                "k": "Số vào viện",
                "v": "",
                "c_llm": 0,
                "match_w": null,
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Tên bệnh nhân",
                "v": "NGUYỄN VĂN A",
                "c_llm": 0,
                "match_w": "NGUYỄN VĂN A",
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Tuổi",
                "v": "",
                "c_llm": 0,
                "match_w": null,
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Giới tính",
                "v": "Nữ",
                "c_llm": 0,
                "match_w": ":",
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Ngày/ tháng/ năm sinh",
                "v": "1990-01-01",
                "c_llm": 0,
                "match_w": "1990-01-01",
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Dân tộc",
                "v": "",
                "c_llm": 0,
                "match_w": null,
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Nghề nghiệp",
                "v": "",
                "c_llm": 0,
                "match_w": null,
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Số thẻ bảo hiểm y tế",
                "v": "DN0000000000000",
                "c_llm": 0,
                "match_w": ": DN0000000000000",
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Thời hạn hiệu lực của thẻ bảo hiểm y tế",
                "v": "",
                "c_llm": 0,
                "match_w": null,
                "page_number": 1
              },
              {
                "c": 1,
                "k": "Địa chỉ",
                "v": "",
                "c_llm": 0,
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /request/group/{groupId}/check-status`

*Gọi thật: `/request/group/4/check-status`*

**Trạng thái cả nhóm — một lời gọi cho toàn bộ gói chứng từ**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

Đây là endpoint để poll. Enum `requestStatus`: `Sending`(0) · `Processing`(1) · `Completed`(2) · `Failed`(3) · `Cancelling`(4) · `Cancelled`(5).

**Response**

```json
[
  {
    "requestId": "21376",
    "requestStatus": "Processing"
  }
]
```

#### `GET /request/{id}/result`

*Gọi thật: `/request/21376/result`*

**Kết quả trích xuất có cấu trúc** — payload chính để map sang A4 §8**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

Cấu trúc: `result.templates_data[]` → mỗi template khớp có `data.fields[]` với `k` (khoá), `v` (giá trị), `c` (confidence 0–1), `c_llm`, `match_w`, `page_number`.

> ⚠️ Ba điểm phải biết trước khi map: `k` là **nhãn tiếng Việt** chứ không phải mã máy · `data.confidence` là thang **0–100** trong khi `c` là thang **0–1** · **field rỗng vẫn mang `c: 1`**, nên confidence không nói gì về độ đầy đủ.

**Response**

```json
{
  "result": {
    "errorCode": null,
    "minioPath": "a1d86331-9c2d-4c43-99e9-3ad7185bdf5d.pdf",
    "minioBucket": "idp-gateway-bucket",
    "refTenantId": "176",
    "errorMessage": null,
    "refPartnerId": "78",
    "refRequestId": "21376",
    "processingTime": 16.14,
    "templates_data": [
      {
        "data": {
          "fields": [
            {
              "c": 1,
              "k": "Số vào viện",
              "v": "",
              "c_llm": 0,
              "match_w": null,
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Tên bệnh nhân",
              "v": "NGUYỄN VĂN A",
              "c_llm": 0,
              "match_w": "NGUYỄN VĂN A",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Tuổi",
              "v": "",
              "c_llm": 0,
              "match_w": null,
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Giới tính",
              "v": "Nữ",
              "c_llm": 0,
              "match_w": ":",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Ngày/ tháng/ năm sinh",
              "v": "1990-01-01",
              "c_llm": 0,
              "match_w": "1990-01-01",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Dân tộc",
              "v": "",
              "c_llm": 0,
              "match_w": null,
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Nghề nghiệp",
              "v": "",
              "c_llm": 0,
              "match_w": null,
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Số thẻ bảo hiểm y tế",
              "v": "DN0000000000000",
              "c_llm": 0,
              "match_w": ": DN0000000000000",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Thời hạn hiệu lực của thẻ bảo hiểm y tế",
              "v": "",
              "c_llm": 0,
              "match_w": null,
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Địa chỉ",
              "v": "",
              "c_llm": 0,
              "match_w": null,
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Admission Date",
              "v": "20/03/2025",
              "c_llm": 0,
              "match_w": "20/03/2025",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Discharge Date",
              "v": "26/03/2025",
              "c_llm": 0,
              "match_w": "20/03/2025",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Khoa phòng",
              "v": "",
              "c_llm": 0,
              "match_w": null,
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Diagnosis Codes",
              "v": "",
              "c_llm": 0,
              "match_w": null,
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /request/{id}/get-ocr`

*Gọi thật: `/request/21376/get-ocr`*

**Output thô của engine OCR** — có `boxes`, `words`, `confidence_scores`**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

> `bbox` mà A4 §8 cần thì **có ở đây**, không có ở `/result`. Đổi lại `ocrResult.tables` là **chuỗi Python repr** (nháy đơn), không parse JSON được.

**Response**

```json
{
  "refPartnerId": "78",
  "refRequestId": "21376",
  "engineType": "azure",
  "ocrResult": {
    "tables": "[{'page_number': 1, 'box': [1.0269, 1.9678, 7.2629, 6.0961], 'data':              Claim Type                : Chăm sóc y tế\n1  Idenification number              : DN0000000000000\n2          Insured Name              : NGUYỄN VĂN A\n3                Gender                       : FEMALE\n4                   DOB                   : 1990-01-01\n5        Admission Date                   : 20/03/2025\n6        Discharge Date                   : 26/03/2025\n7            Event Date                   : 20/03/2025\n8         Document type  : Chứng từ y tế, Giấy ra viện\n9            Death Date                              :}]",
    "document": "\n-----BEGINING OF PAGE 1-----\nDU LIEU KHACH HANG KHAI\nDATA PLATFORM - MBAL\nHo so: Nguyen Hoang Ha - Benh vien Noi tiet Trung uong - 03/2025\nClaim Type\n:\nChăm sóc y tế\nIdenification number\n: DN0000000000000\nInsured Name\n:\nNGUYỄN VĂN A\nGender\n:\nFEMALE\nDOB\n:\n1990-01-01\nAdmission Date\n:\n20/03/2025\nDischarge Date\n: 26/03/2025\nEvent Date\n:\n20/03/2025\nDocument type\n:\nChứng từ y tế, Giấy ra viện\nDeath Date\n:\nTai lieu mo phong du lieu Data Platform gui sang IDP - dung cho muc dich demo\n-----END OF PAGE 1-----\n",
    "document2d": "\n-----BEGINING OF PAGE 1-----\n                                                            DU LIEU KHACH HANG KHAI\n                                                                        DATA PLATFORM - MBAL\n                                                Ho so: Nguyen Hoang Ha - Benh vien Noi tiet Trung uong - 03/2025\n                          Claim Type                                                 :    Chăm sóc y tế\n                          Idenification number                                       : DN0000000000000\n                          Insured Name                                               :    NGUYỄN VĂN A\n                          Gender                                                     :   FEMALE\n                          DOB                                                        : 1990-01-01\n                          Admission Dat
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /request/{id}/check-workflow-request`

*Gọi thật: `/request/21376/check-workflow-request`*

**Kiểm tra request đã gắn workflow nào chưa**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

> Trả `{message: "Done"}` — một lời xác nhận, không phải dữ liệu. Muốn biết request thuộc workflow nào thì tra ngược bằng `GET /workflow/request/search`.

**Response**

```json
{
  "message": "Done"
}
```

**Cấu trúc khi có dữ liệu**

```json
// Endpoint chỉ trả đúng một dạng, không có biến thể nào khác.
{ "message": "Done" }
```

#### `GET /request/{id}/raw-file`

*Gọi thật: `/request/21376/raw-file`*

**Tải lại chính file gốc đã nộp**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `GET` | `200` |

> Hai công dụng cho Claim MVP: **thay thế cho deep link I-10** — hiện ảnh gốc cạnh giá trị đã trích xuất ngay trong console của mình; và **đối chiếu `sha256`** sau khi nộp, bù cho việc IDP không nhận hash lúc nộp.

**Response**

```json
{
  "__binary__": true,
  "content_type": "application/zip",
  "content_length": 42134,
  "bytes": 42134
}
```

#### `GET /request/{id}/result`

*Gọi thật: `/request/21383/result`*

**Kết quả trích xuất SAU KHI nạp ánh xạ** — `subDocumentType` đã có giá trị**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `GET` | `200` |

> ✅ **Vòng khép kín.** Lần trích xuất đầu tiên trả `subDocumentType: null`; sau khi nạp bảng `mbal/template-map`, lần này trả **`"CL05"`**.
>
> Nghĩa là **mã loại chứng từ của A4 §3 lấy được**, chỉ cần bảng ánh xạ được nạp đủ. Đây là việc cấu hình, không phải việc phát triển.

**Response**

```json
{
  "result": {
    "errorCode": null,
    "minioPath": "1043de79-5fdc-43b6-8b73-1d5289a06cbe.pdf",
    "minioBucket": "idp-gateway-bucket",
    "refTenantId": "176",
    "errorMessage": null,
    "refPartnerId": "78",
    "refRequestId": "21383",
    "processingTime": 19.04,
    "templates_data": [
      {
        "data": {
          "fields": [
            {
              "c": 0.9,
              "k": "Place Of Treatment",
              "v": "Benh vien Noi tiet Trung uong",
              "c_llm": 0,
              "match_w": "Ho so: Nguyen Hoang Ha - Benh vien Noi tiet Trung uong - 03/2025",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Số vào viện",
              "v": "",
              "c_llm": 0,
              "match_w": null,
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Tên bệnh nhân",
              "v": "NGUYỄN VĂN A",
              "c_llm": 0,
              "match_w": "NGUYỄN VĂN A",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Tuổi",
              "v": "",
              "c_llm": 0,
              "match_w": null,
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Giới tính",
              "v": "FEMALE",
              "c_llm": 0,
              "match_w": "FEMALE",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Ngày/ tháng/ năm sinh",
              "v": "1990-01-01",
              "c_llm": 0,
              "match_w": "1990-01-01",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Số thẻ bảo hiểm y tế",
              "v": "DN0000000000000",
              "c_llm": 0,
              "match_w": ": DN0000000000000",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Địa chỉ",
              "v": "",
              "c_llm": 0,
              "match_w": null,
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Admission Date",
              "v": "20/03/2025",
              "c_llm": 0,
              "match_w": "20/03/2025",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Discharge Date",
              "v": "26/03/2025",
              "c_llm": 0,
              "match_w": "20/03/2025",
              "page_number": 1
            },
            {
              "c": 1,
              "k": "Diagnosis Codes",
              "v": "",
              "c_llm": 0,
              "match_w": null,
              "page_number": 1
            },
            {
              "c": 1,
              "k":
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `PUT /request/{id}/result`

**Sửa kết quả trích xuất** — bằng chứng kết quả IDP không bất biến**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `PUT` | `200` |

Body: `{ index, fields: [{c, k, v, match_w, page_number}], tables: [] }`.

> ⚠️ **Đã chạy thật, trả `200` với `affected: 1`.** Kết quả trích xuất của IDP **sửa được sau khi đã tạo**. D3 §I-09 dựa vào tính bất biến để so raw-versus-corrected và để tái lập một claim đã settle — nên **adapter buộc phải copy kết quả sang store của mình ở lần đọc đầu** và không bao giờ đọc lại từ IDP cho mục đích audit.

**Request**

```json
{
  "index": 0,
  "fields": [
    {
      "c": 1,
      "k": "Tên bệnh nhân",
      "v": "NGUYỄN VĂN A (đã sửa tay)",
      "match_w": "",
      "page_number": 1
    }
  ],
  "tables": []
}
```

**Response**

```json
{
  "generatedMaps": [],
  "raw": [],
  "affected": 1
}
```

#### `GET /request/{id}/export`

*Gọi thật: `/request/21376/export`*

**Xuất kết quả một request ra file**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `GET` | `200` |

Trả file, không phải JSON.

**Response**

```json
{
  "__binary__": true,
  "content_type": "application/octet-stream",
  "content_length": "7186",
  "bytes": 7186
}
```

#### `POST /request/export-multiple-request`

**Xuất kết quả nhiều request ra một file**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `POST` | `201` |

**Request**

```json
{
  "requestIds": [
    21376,
    21377
  ]
}
```

**Response**

```json
"PK\u0003\u0004\n\u0000\u0000\u0000\b\u0000-5#]���\tY\u0001\u0000\u0000�\u0004\u0000\u0000\u0013\u0000\u0000\u0000[Content_Types].xml��Mn�0\u0010��=E�-J\f]TUE¢��\u0016��\u0000�xB,\u001c���w\u0012(�* �`\u0013+�7�{��\u0019O��I�\u0018H;��Q6\u0014\t��)m\u0017������\"�\bV�q\u0016s�C\u0012��n<�y���-墎�?IIe�\rP�<Z�T.4\u0010�6,��r\t\u000b����,��hc\u001a[\u000fQ��X����eˏ�A\u0002\u001a\u0012��^زr\u0001�\u001b]B�\\[���\u001e\b\u0019wv\u001a���\u0001\u000b�<Ih+�\u0001��w�L�\n�\u0019��\u0006\r���ȍ\u000b�/��e�\u0013)]U�\u0012�+W\r�d�\u0003��\u001a16&�֬\u0001m\u0007��NL�[F7\u000er���\u0011�}��z}�Φ\u0007Hqg�n=�δ�\\C@�\u0011\u0003\u001f��\u0007��}�]_I��i�\rS�m����<�\u0011\r��]����;�l�!�ˣ=\u0012���b;+��\u0004[v?��\u001bPK\u0003\u0004\n\u0000\u0000\u0000\u0000\u0000-5#]\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0006\u0000\u0000\u0000_rels/PK\u0003\u0004\n\u0000\u0000\u0000\b\u0000-5#]�I��\u0000\u0000\u0000K\u0002\u0000\u0000\u000b\u0000\u0000\u0000_rels/.rels���N�0\f@�|E���nH\b��� ��&4>�$n\u001b���ă�@\f�i\u0007�q��g���<��R�\u001c\f,�\u001a\u0014\u0005�·����iq\u000f*\u000b\u0006�#\u00072p�\f��f�L#J�ɽ�Y\u0015H�\u0006z���u�=M�+�\u0014�O�iB)���v������t�ɀ愩��@ں%��1�5ln[o��a� gZ��(dL\u001d��y�W�*P��]V׻�=��HС���h\u0011S�N��Z�u\u001c�]\t�όKB���\u001c���#wY\tc�2�'7�|\u0000PK\u0003\u0004\n\u0000\u0000\u0000\u0000\u0000-5#]\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0003\u0000\u0000\u0000xl/PK\u0003\u0004\n\u0000\u0000\u0000\u0000\u0000-5#]\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\t\u0000\u0000\u0000xl/_rels/PK\u0003\u0004\n\u0000\u0000\u0000\b\u0000-5#]�$�V�\u0000\u0000\u0000�\u0002\u0000\u0000\u001a\u0000\u0000\u0000xl/_rels/workbook.xml.rels���j�0\u0010D��\n��ZvZJ)�s)�\\[�\u0003���LlIh7m��U\u001bH\u001c\b�\u0007�Ĭؙ�H���8�OL�\u0007��*J\u0010�M���\u0014|4��g\u0010��[=\u0004�\n&$��w�7\u001c4�\u001dr}$�M<)p��EJ2\u000eGME���M\u001bҨ9��ɨ�^w(We�$��\u0003�\u000bO��\n��V �)��C��\u0006_�9���J�$���/\u001a�:d\u0005G]d\u001f���WK�s��s��<\u000e�[\f\u000f�V�tB��)?���\u0016��0_!��!�\u0019�4�E�ǩ\u0019y���\u001fPK\u0003\u0004\n\u0000\u0000\u0000\u0000\u0000-5#]\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u000e\u0000\u0000\u0000xl/worksheets/PK\u0003\u0004\n\u0000\u0000\u0000\b\u0000-5#]�o�e�\u0002\u0000\u0000�\n\u0000\u0000\u0018\u0000\u0000\u0000xl/worksheets/sheet1.xml��[o�0\u0010���WD~/!\t�\u0005\u0011��lK��\r\u000eXM��6��߉S�حF�\u000b��˜��Gb\u0006goy�m�T\\\u00141\tZm�\"\u0011K^�b��_\u001d�\u0012OiZ,i&\n\u0016�w�����`'�Z3�=\u0010(TL�Z�}�Wɚ�T�D�\n ��9�Pʕ�J���4�\u001f���~NyAj���
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `POST /request/merge-multiple`

**Gộp nhiều ảnh thành MỘT request**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `POST` | `201` |

Multipart, nhiều file cùng tên `document` (tối đa 10), kèm `name` và `templates` dạng CSV.

> ⚠️ **Chỉ nhận ảnh.** Gửi PDF thì lỗi `400 · Failed to merge images`. Với Claim MVP thì đây là cách gộp nhiều trang chụp rời của cùng một chứng từ trước khi trích xuất.

**Request**

```json
{
  "name": "MERGE-…",
  "templates": "5384,5127",
  "document": "<2 ảnh PNG>"
}
```

**Response**

```json
{
  "pageCount": 2,
  "requestId": 21382
}
```

#### `POST /request/compare`

**Đối chiếu kết quả trích xuất với file ground-truth**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `POST` | `201` |

Multipart: `requestId` + `document` là một **file XLSX**.

> ⚠️ **Chỉ nhận XLSX** — route này dùng `multerExcelOptions`, gửi PDF hay PNG đều bị chặn `Unsupported file type`.
>
> ✅ **Response là một file XLSX chứa kết quả so sánh**, không phải JSON. Đây chính là công cụ đo độ chính xác mà **A4 §10 yêu cầu cho bộ ground-truth** — có sẵn, không phải tự dựng.

**Request**

```json
{
  "requestId": "21376",
  "document": "<ground-truth.xlsx>"
}
```

**Response**

```json
{
  "__binary__": true,
  "content_type": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  "note": "Trả về FILE XLSX chứa kết quả so sánh (không phải JSON). Ký tự đầu PK\\x03\\x04 là chữ ký ZIP của định dạng xlsx."
}
```

#### `POST /request/compare-multiple`

**Đối chiếu nhiều request cùng lúc với một file ground-truth**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `POST` | `201` |

**Request**

```json
{
  "requestIds": "21376,21378",
  "document": "<ground-truth.xlsx>"
}
```

**Response**

```json
{
  "__binary__": true,
  "content_type": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  "note": "Trả về FILE XLSX chứa kết quả so sánh (không phải JSON). Ký tự đầu PK\\x03\\x04 là chữ ký ZIP của định dạng xlsx."
}
```

#### `PATCH /request/{id}/retry`

**Chạy lại một request đã lỗi**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `PATCH` | `200` |

Chỉ hợp lệ với request ở trạng thái lỗi; gọi trên request đã `Completed` thì trả `400 · Ocr Request Retry Failed Exception`.

**Response**

```json
{
  "id": "21374",
  "tenantId": "176",
  "partnerId": "40",
  "name": "CLM-TEST-1788410518",
  "userId": "203",
  "templateId": null,
  "templates": [
    5445,
    5444,
    5443,
    5384,
    5127,
    5340,
    5299
  ],
  "status": 1,
  "isDownloaded": false,
  "deleteFile": false,
  "pageCount": 1,
  "createdAt": "2026-09-03T04:41:59.291Z",
  "updatedAt": "2026-09-03T06:43:00.682Z",
  "deletedAt": null,
  "requestGroupId": "2",
  "extraParams": {}
}
```

#### `PATCH /request/{id}/cancel`

**Huỷ một request đang chạy**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `PATCH` | `200` |

Chỉ hợp lệ khi request đang chạy — gọi trên request đã `Completed` thì trả `400 · Request is not running!`. Sau khi huỷ, `status` chuyển sang `4` (Cancelling).

**Response**

```json
{
  "id": "21381",
  "tenantId": "176",
  "partnerId": "40",
  "name": "CANCEL-1788417780",
  "userId": "203",
  "templateId": null,
  "templates": [
    5384,
    5127
  ],
  "status": 4,
  "isDownloaded": false,
  "deleteFile": false,
  "pageCount": 1,
  "createdAt": "2026-09-03T06:43:01.124Z",
  "updatedAt": "2026-09-03T06:43:01.578Z",
  "deletedAt": null,
  "requestGroupId": null,
  "extraParams": {}
}
```

#### `POST /request/from-url`

**Nộp theo URL thay vì bytes** — hiện đang lỗi**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `400`) | `POST` | `400` |

Body JSON: `{ name, templates: number[], url, requestGroupId }`. Đây là **đường gần nhất với cách nộp theo tham chiếu của D3 I-07**: IDP tự `GET` url phía server và **không gắn header nào**, nên pre-signed URL dùng được.

> ❌ **Lỗi thật, và đã tìm ra nguyên nhân gốc trong code.** Đã thử **4 URL PDF công khai khác nhau**, thêm `requestGroupId`, đổi kiểu `templates` — đều `400 · The specified key does not exist.`
>
> **Nguyên nhân:** `from-url` tải file về **đĩa cục bộ** rồi truyền `{ originalname, path: <đường dẫn cục bộ> }` sang `requestService.create()`. Nhưng hàm đó gọi thẳng `minIOService.getObjectStream(file.path)` — nó coi `path` là **khoá đối tượng trên MinIO**, vì luồng upload thường đi qua storage engine của MinIO. Đường dẫn cục bộ không phải khoá MinIO ⇒ lỗi. Ở một hàm khác trong cùng service có nhánh `if (file.storageType == StorageType.MinIO) … else fs.createReadStream(...)`; nhánh `create()` **thiếu đúng nhánh đó**.
>
> **Không cứu được từ phía gọi.** Cách sửa cho IDP: hoặc upload file vừa tải lên MinIO trước, hoặc thêm nhánh kiểm tra `storageType` như hàm kia.
>
> **Phát hiện phụ:** ở endpoint này `templates` phải là **mảng số**; gửi chuỗi CSV thì lỗi `ids.join is not a function` — **ngược với `POST /request` vốn đòi chuỗi CSV**.

**Request**

```json
{
  "name": "FROMURL-1788417780",
  "templates": [
    5384,
    5127
  ],
  "url": "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf"
}
```

**Response**

```json
{
  "statusCode": 400,
  "message": "The specified key does not exist."
}
```


---

## 2.5 Đánh giá

> **Toàn bộ đánh giá nằm dưới `/workflow`.** Không tồn tại API đánh giá độc lập. Nhóm này **không chặn theo role** — cả Admin và End User đều gọi được, nhưng dữ liệu lọc theo `tenantId` nên Admin không đọc được bản ghi của tenant khác (trả `400`).

#### `GET /workflow/forwarder/api/v1/flows`

**Danh sách flow đánh giá và `workflowId`**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

> Response nặng ~2,8 MB vì kèm toàn bộ đồ thị node. Adapter chỉ cần `id` — **nên cache, đừng gọi mỗi claim**. Payload dưới cắt mạnh.

> **Flow chuẩn để dùng: `5ed8e4fc-27a5-45a7-b104-1216fcfd0797` — "Workflow Claim 28/8".** Đây là flow đã bỏ bước convert sang markdown nên trả JSON đúng cấu trúc, và cũng là flow giao diện web đang dùng. **Ghim id này trong cấu hình adapter.**
>
> Staging còn flow `8d873709-…` *"Workflow Claim 25/08"* — **không dùng**, vì nó xuất markdown làm `/result/json` trả rỗng.

**Response**

```json
{
  "items": [
    {
      "endpoint_name": null,
      "icon": null,
      "tags": null,
      "description": "Workflow Claim 28/8 bỏ phần convert markdown cuối cùng.",
      "data": {
        "nodes": [
          {
            "id": "Prompt-TSAeA",
            "type": "genericNode",
            "position": {
              "x": 9665.50882208022,
              "y": 2530.878818850673
            },
            "data": {
              "id": "Prompt-TSAeA",
              "node": {
                "template": {
                  "code": {
                    "info": "",
                    "list": false,
                    "name": "code",
                    "show": true,
                    "type": "code",
                    "value": "from langflow.base.prompts.api_utils import process_prompt_template\nfrom langflow.custom import Component\nfrom langflow.inputs.inputs import DefaultPrompt
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /workflow/get-workflow-components?flowId={flowId}`

*Gọi thật: `/workflow/get-workflow-components?flowId=8d873709-4cac-4f26-82f1-b1f9d28c6e73`*

**Liệt kê component của một flow**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

Hữu ích để chẩn đoán khi một flow không chạy được — xem flow gồm những component nào. Cách gọi giống nhau cho mọi flow.

**Response**

```json
[
  {
    "id": "Prompt-9f2Tr",
    "type": "Prompt",
    "display_name": "Main Prompt",
    "inputs": [
      {
        "info": "",
        "list": false,
        "name": "template",
        "show": true,
        "type": "prompt",
        "value": "Below is the user's documents. The structure is: # [Document Type] [Line Break] [Document Data]. If the document data non-exist, then the [Document Data] will be empty.\n\n--- Begin document data ---\nBelow is a markdown table (surgery_table) contains the matched template name, its surgery name and its surgery code:\n{surgery_table}\n---\nBelow is data_comparision_warning_message:\n{data_comparision_result}\n---\n{idp_and_dataplatform_data}\n--- End document data ---\n\n# Below is a markdown bullet point contains your instructions, follows it step by step and return the result in a JSON format. The result should be an array of JSON Object, each JSON Object contains the result of a instruction. An JSON Object should contains:\n  + The instruction name (attribute name: `instruction_name`).\n  + The instruction type in text (attribute name: `instruction_type`).\n  + And if the instruction type is `Trích xuất field` then includes the `Fieldname` of instruction (attribute name: `fieldname`).\n  + And if the instruction type is `Trích xuất field` then includes the `Field Validation` of instruction (attribute name: `field_validation`), the
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `PUT /workflow/request`

**Tạo yêu cầu đánh giá** từ các request đã trích xuất xong**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** | `PUT` | `200` |

`ocrRequestIds` và `workflowId` và `name` là bắt buộc. `extraInputData[] { template, data }` là **chỗ duy nhất nhét thêm dữ liệu** vào flow.

> ⚠️ Ba điều đã kiểm chứng: `autoStart: "true"` **không có tác dụng**, bản ghi lưu `false` và trạng thái đứng ở `1` · khoá trả về tên `workflowRequestId` nhưng khi `GET` thì bản ghi lại có `id` và một `workflowRequestId: null` riêng · **nhận cả OCR request đang `Failed`**, không kiểm tra gì lúc tạo.

**Request**

```json
{
  "ocrRequestIds": [
    "21376"
  ],
  "workflowId": "8d873709-4cac-4f26-82f1-b1f9d28c6e73",
  "name": "ASSESS-B-1788411746",
  "autoStart": "true"
}
```

**Response**

```json
{
  "workflowRequestId": "1403"
}
```

#### `GET /workflow/run-workflow-request/{id}`

**Kích hoạt chạy flow** — bắt buộc phải gọi vì `autoStart` không hoạt động

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200`. Với flow chuẩn `5ed8e4fc-…`: lần gọi đầu từng trả `400`, lần sau `200` |

> ⚠️ **Hành vi không ổn định.** Cùng flow `5ed8e4fc-…`, cùng OCR request `21376`, cùng tham số: lần đầu trả lỗi dưới đây, lần sau trả `200` và chạy tới cùng sau ~30 giây. Adapter phải **thử lại** thay vì kết luận flow hỏng ngay lần đầu.
>
> Lỗi của lần đầu:
> ```json
> { "statusCode": 400, "message": "Unexpected token '\"', \"#\" is not valid JSON", "error": "Bad Request" }
> ```
> Lỗi phát sinh khi FO dựng tham số cho flow, và **không nói component nào hỏng**.


#### `GET /workflow/request/{id}`

*Gọi thật: `/workflow/request/1403`*

**Trạng thái và kết quả thật của một yêu cầu đánh giá**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

Enum `status`: `1` WaitingForInput · `7` InQueue · `2` Running · `3` Finished · `4` Error · `5` NotFound · `6` Unknown.

> **Đọc payload này thế nào — ba trường quyết định, phần còn lại là nhiễu.**
>
> | Trường | Ý nghĩa |
> |---|---|
> | `status` | `3` = chạy xong. Đây là thứ để poll |
> | `result.success` | `true` = flow chạy thành công |
> | `result.result` | Nội dung đánh giá thật, dạng chuỗi |
>
> **`result.logs` không phải lỗi.** Nó là stderr của lần chạy flow, và trong bản ghi này toàn bộ 3.200 ký tự chỉ là `UserWarning` của thư viện Langflow về tham số `top_p` — không có một dòng `Error` hay `Traceback` nào. Payload dưới đã **rút gọn trường này** cho dễ đọc; bản đầy đủ nằm trong `request-response-json/`.

> **Đây mới là nơi chứa nội dung đánh giá thật.** Trường `result.result` là một chuỗi markdown; `result.success` cho biết flow chạy thành công hay không. Payload dưới cắt bớt — bản đầy đủ dài ~11 KB.

**Response**

```json
{
  "id": "1403",
  "name": "ASSESS-B-1788411746",
  "workflowRequestId": null,
  "workflowId": "8d873709-4cac-4f26-82f1-b1f9d28c6e73",
  "tenantId": "176",
  "status": 2,
  "result": {},
  "resultTransactions": [],
  "createdAt": "2026-09-03T05:02:27.227Z",
  "updatedAt": "2026-09-03T05:02:27.871Z",
  "autoStart": false,
  "extraInputData": [],
  "retries": 0,
  "version": 2,
  "ocrRequests": [
    {
      "id": "21376",
      "tenantId": "176",
      "partnerId": "40",
      "name": "CLM-TEST-1788411360",
      "userId": "203",
      "templateId": null,
      "templates": [
        5445,
        5444,
        5443,
        5384,
        5127,
        5340,
        5299
      ],
      "status": 2,
      "isDownloaded": false,
      "deleteFile": false,
      "pageCount": 1,
      "createdAt": "2026-09-03T04:56:01.324Z",
      "updatedAt": "2026-09-03T04:56:19.385Z",
      "deletedAt": null,
      "requestGroupId": "4",
      "extraParams": {},
      "results": [
        {
          "id": "21501",
          "requestId": "21376",
          "confidence": "98.90",
          "result": {
            "result": {
              "errorCode": null,
              "minioPath": "a1d86331-9c2d-4c43-99e9-3ad7185bdf5d.pdf",
              "minioBucket": "idp-gateway-bucket",
              "refTenantId": "176",
              "errorMessage": null,
              "refPartnerId": "78",
              "refRequestId": "21376",
              "processingTime": 16.14,
              "templates_data": [
                {
                  "data": {
                    "fields": [
                      {
                        "c": 1,
                        "k": "Số vào viện",
                        "v": "",
                        "c_llm": 0,
                        "match_w": null,
                        "page_number": 1
                      },
                      {
                        "c": 1,
                        "k": "Tên bệnh nhân",
                        "v": "NGUYỄN VĂN A",
                        "c_llm": 0,
                        "match_w": "NGUYỄN VĂN A",
                        "page_number": 1
                      },
                      {
                        "c": 1,
                        "k": "Tuổi",
                        "v": "",
                        "c_llm": 0,
                        "match_w": null,
                        "page_number": 1
                      },
                      {
                        "c": 1,
                        "k": "Giới tính",
                        "v": "Nữ",
                        "c_llm": 0,
                        "match_w": ":",
                        "page_number": 1
                      },
                      {
                        "c": 1,
                        "k": "Ngày/ tháng/ năm sinh",
                        "v": "1990-01-01",
                        "c_llm": 0,
                        "match_w": "1990-01-01",
                        "page_number": 1
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /workflow/request/{id}/result/json`

*Gọi thật: `/workflow/request/1404/result/json`*

**Lấy kết quả đánh giá**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `GET` | `200` |

> ✅ **Endpoint này hoạt động đúng.** Chạy với flow `5ed8e4fc-…` "Workflow Claim 28/8" thì trả JSON đúng cấu trúc, 20 mục. Đây cũng là flow mà giao diện web đang dùng.
>
> **Kiểm chứng trên 8 workflow request khác nhau, kết quả nhất quán tuyệt đối:**
>
> | Flow | Số lần trả JSON đầy đủ |
> |---|---|
> | `5ed8e4fc-…` *Claim 28/8, đã bỏ bước convert markdown* | **4/4** — 4.171 · 8.369 · 8.618 ký tự |
> | `8d873709-…` *Claim 25/08* | **0/4** — luôn trả `{}` |
> | `7b649d06-…` | 0/1 |
>
> Nghĩa là **cái quyết định là chọn flow nào, không phải endpoint hỏng**.
>
> **Cấu trúc mỗi mục:** `instruction_name` · `instruction_type` (`Cảnh báo` | `Trích xuất field`) · `warning_level` (`0` / `0.5` / `1`, chỉ có ở loại Cảnh báo) · `fieldname` + `field_validation` (chỉ có ở loại Trích xuất field) · `result` (văn bản tiếng Việt).
>
> Lần chạy này: **10 mục Cảnh báo** và **10 mục Trích xuất field**.

**Response**

```json
{
  "data": [
    {
      "instruction_name": "Kiểm tra Danh mục bệnh viện",
      "instruction_type": "Cảnh báo",
      "warning_level": 1,
      "result": "VNVision-Giấy ra viện/Hospital discharge papers: Bệnh viện Nội tiết Trung ương. Bệnh viện Nội tiết Trung ương Đây là bệnh viện mới"
    },
    {
      "instruction_name": "Tìm kiếm HospitalCode",
      "instruction_type": "Trích xuất field",
      "fieldname": "HospitalCode",
      "field_validation": true,
      "result": "00000"
    },
    {
      "instruction_name": "Tìm kiếm PlaceOfTreatment",
      "instruction_type": "Trích xuất field",
      "fieldname": "PlaceOfTreatment",
      "field_validation": true,
      "result": "null"
    },
    {
      "instruction_name": "Kiểm tra Ngày tử vong",
      "instruction_type": "Cảnh báo",
      "warning_level": 0,
      "result": "Không tìm thấy thông tin ngày tử vong trên các chứng từ."
    },
    {
      "instruction_name": "Kiểm tra Ngày vào viện",
      "instruction_type": "Cảnh báo",
      "warning_level": 0,
      "result": "VNVision-Giấy ra viện/Hospital discharge papers: 20/03/2025\nThông tin ngày vào viện đồng nhất."
    },
    {
      "instruction_name": "Kiểm tra Ngày xuất viện",
      "instruction_type": "Cảnh báo",
      "warning_level": 0,
      "result": "VNVision-Giấy ra viện/Hospital discharge papers: 26/03/2025\nThông tin ngày xuất viện đồng nhất."
    },
    {
      "instruction_name": "Kiểm tra Giấy tờ tùy thân",
      "instruction_type": "Cảnh báo",
      "warning_level": 1,
      "result": "VNVision-Giấy ra viện/Hospital discharge papers: Không có thông tin giấy tờ tùy thân.\n\nCảnh báo: Không thể đối soát do thiếu thông tin giấy tờ tùy thân trên chứng từ."
    },
    {
      "instruction_name": "Kiểm tra Ngày xảy ra sự kiện bảo hiểm",
      "instruction_type": "Cảnh báo",
      "warning_level": 1,
      "result": "VNVision-Giấy ra viện/Hospital discharge papers: EventDate (Admission Date) là 20/03/2025.\n\nCảnh báo: Ngày xảy ra sự kiện bảo hiểm (20/03/2025) thuộc về tương lai so với thời điểm hiện tại."
    },
    {
      "instruction_name": "Kiểm tra Mã phẫu thuật từ metadata",
      "instruction_type": "Cảnh báo",
      "warning_level": 0,
      "result": "Không có cảnh báo"
    },
    {
      "instruction_name": "Cảnh báo tài liệu không thể phân loại",
      "instruction_type": "Cảnh báo",
      "warning_level": 0,
      "result": "Tài liệu phân loại thành công."
    },
    {
      "instruction_name": "Tìm kiếm DeathDate",
      "instruction_type": "Trích xuất field",
      "fieldname": "deathDate",
      "field_validation": true,
      "result": null
    },
    {
      "instruction_name": "Tìm kiếm AdmissionDate",
      "instruction_type": "Trích xuất field",
      "fieldname": "AdmissionDate",
      "field_validation": true,
      "result": "20/03/2025"
    },
    {
      "instruction_name": "Tìm kiếm DischargeDate",
      "instruction_type": "Trích xuất field",
      "fieldname": "DischargeDate",
      "field_validat
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /workflow/request/{id}/result/json`

*Gọi thật: `/workflow/request/1403/result/json`*

**Cùng endpoint, nhưng chạy bằng flow sai** — trả `{}` kèm HTTP 200**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `400` |

> ⚠️ **Đây là cái bẫy cần biết, không phải lỗi của endpoint.** Chạy với flow `8d873709-…` "Workflow Claim 25/08": đánh giá **chạy xong, `success: true`, sinh 3.778 ký tự nội dung** — endpoint vẫn trả `{}` kèm **HTTP 200**.
>
> Nguyên nhân: flow đó trả markdown chứ không trả khối ```json. Service tìm khối JSON, không thấy, thử parse cả chuỗi, hỏng, rồi **nuốt lỗi và trả object rỗng**.
>
> **Ba hệ quả phải nhớ:** ghim đúng `workflowId` là việc cấu hình bắt buộc · `{}` phải luôn bị coi là **hỏng**, không bao giờ là "không có khuyến nghị nào" · khi **chưa có** kết quả thì endpoint trả `400 / 4007`, tức là ba tình huống khác nhau cho ra ba hành vi khác nhau.

**Response**

```json
{
  "detail": "Bad Workflow result, something is wrong.",
  "statusCode": 4007,
  "message": "Workflow result is bad, please check again."
}
```

**Cấu trúc khi có dữ liệu**

```json
// Cấu trúc ĐÚNG mà endpoint này trả khi flow xuất JSON — xem mục ngay trên.
{
  "data": [
    { "instruction_name": "Kiểm tra Danh mục bệnh viện",
      "instruction_type": "Cảnh báo",
      "warning_level": 1,
      "result": "…" },
    { "instruction_name": "Tìm kiếm HospitalCode",
      "instruction_type": "Trích xuất field",
      "fieldname": "HospitalCode",
      "field_validation": true,
      "result": "00000" }
  ]
}
```

#### `GET /workflow/request/{id}`

*Gọi thật: `/workflow/request/1404`*

**Bản ghi đầy đủ của một yêu cầu đánh giá chạy bằng flow trả JSON**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `GET` | `200` |

Trường `result.result` vẫn là **chuỗi** chứa khối ```` ```JSON ````; endpoint `/result/json` là thứ bóc nó ra.

> Giống bản ghi trên: `result.logs` chỉ là `UserWarning` của thư viện, **không phải lỗi**, và đã được rút gọn trong payload dưới.

**Response**

```json
{
  "id": "1404",
  "name": "RETRY-FLOWA-1788420635",
  "workflowRequestId": null,
  "workflowId": "5ed8e4fc-27a5-45a7-b104-1216fcfd0797",
  "tenantId": "176",
  "status": 3,
  "result": {
    "logs": "/app/.venv/lib/python3.12/site-packages/lfx/custom/custom_component/component.py:1194: UserWarning: Parameters {'top_p'} should be specified explicitly. Instead  … [rút gọn — 3200 ký tự, toàn bộ là UserWarning của thư viện, không phải lỗi]",
    "type": "message",
    "result": "```JSON\n[\n  {\n    \"instruction_name\": \"Kiểm tra Danh mục bệnh viện\",\n    \"instruction_type\": \"Cảnh báo\",\n    \"warning_level\": 1.0,\n    \"result\": \"VNVision-Giấy ra viện/Hospital discharge papers: Bệnh viện Nội tiết Trung ương. Bệnh viện Nội tiết Trung ương Đây là bệnh viện mới\"\n  },\n  {\n    \"instruction_name\": \"Tìm kiếm HospitalCode\",\n    \"instruction_type\": \"Trích xuất field\",\n    \"fieldname\": \"HospitalCode\",\n    \"field_validation\": true,\n    \"result\": \"00000\"\n  },\n  {\n    \"instruction_name\": \"Tìm kiếm PlaceOfTreatment\",\n    \"instruction_type\": \"Trích xuất field\",\n    \"fieldname\": \"PlaceOfTreatment\",\n    \"field_validation\": true,\n    \"result\": \"null\"\n  },\n  {\n    \"instruction_name\": \"Kiểm tra Ngày tử vong\",\n    \"instruction_type\": \"Cảnh báo\",\n    \"warning_level\": 0.0,\n    \"result\": \"Không tìm thấy thông tin ngày tử vong trên các chứng từ.\"\n  },\n  {\n    \"instruction_name\": \"Kiểm tra Ngày vào viện\",\n    \"instruction_type\": \"Cảnh báo\",\n    \"warning_level\": 0.0,\n    \"result\": \"VNVision-Giấy ra viện/Hospital discharge papers: 20/03/2025\\nThông tin ngày vào viện đồng nhất.\"\n  },\n  {\n    \"instruction_name\": \"Kiểm tra Ngày xuất viện\",\n    \"instruction_type\": \"Cảnh báo\",\n    \"warning_level\": 0.0,\n    \"result\": \"VNVision-Giấy ra viện/Hospital discharge papers: 26/03/2025\\nThông tin ngày xuất viện đồng nhất.\"\n  },\n  {\n    \"instruction_name\": \"Kiểm tra Giấy tờ tùy thân\",\n    \"instruction_type\": \"Cảnh báo\",\n    \"warning_level\": 1.0,\n    \"result\": \"VNVision-Giấy ra viện/Hospital discharge papers: Không có thông tin giấy tờ tùy thân.\\n\\nCảnh báo: Không thể đối soát do thiếu thông tin giấy tờ tùy thân trên chứng từ.\"\n  },\n  {\n    \"instruction_name\": \"Kiểm tra Ngày xảy ra sự kiện bảo hiểm\",\n    \"instruction_type\": \"Cảnh báo\",\n    \"warnin
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /workflow/request/search?page={n}&limit={m}`

*Gọi thật: `/workflow/request/search?page=1&limit=5`*

**Tìm kiếm yêu cầu đánh giá có phân trang**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

> Dữ liệu **lọc theo `tenantId` trong token**. Gọi bằng tài khoản Admin (tenant 175) trả `{items: [], totalItems: 0}` — không phải endpoint hỏng mà là tenant đó không có bản ghi nào. Payload dưới lấy bằng tài khoản End User (tenant 176).

**Response**

```json
{
  "items": [
    {
      "id": "1403",
      "name": "ASSESS-B-1788411746",
      "workflowRequestId": null,
      "workflowId": "8d873709-4cac-4f26-82f1-b1f9d28c6e73",
      "tenantId": "176",
      "status": 3,
      "createdAt": "2026-09-03T05:02:27.227Z",
      "updatedAt": "2026-09-03T05:04:03.189Z",
      "autoStart": false
    },
    {
      "id": "1402",
      "name": "ASSESS-TEST-1788411502",
      "workflowRequestId": null,
      "workflowId": "5ed8e4fc-27a5-45a7-b104-1216fcfd0797",
      "tenantId": "176",
      "status": 1,
      "createdAt": "2026-09-03T04:58:23.123Z",
      "updatedAt": "2026-09-03T04:58:23.123Z",
      "autoStart": false
    },
    {
      "id": "1401",
      "name": "ASSESS-TEST-1788410699",
      "workflowRequestId": null,
      "workflowId": "5ed8e4fc-27a5-45a7-b104-1216fcfd0797",
      "tenantId": "176",
      "status": 1,
      "createdAt": "2026-09-03T04:45:01.967Z",
      "updatedAt": "2026-09-03T04:45:01.967Z",
      "autoStart": false
    },
    {
      "id": "1400",
      "name": "Bo A - PASS (7 chung tu)",
      "workflowRequestId": null,
      "workflowId": "5ed8e4fc-27a5-45a7-b104-1216fcfd0797",
      "tenantId": "176",
      "status": 3,
      "createdAt": "2026-08-28T04:11:25.124Z",
      "updatedAt": "2026-08-28T04:28:38.944Z",
      "autoStart": false
    },
    {
      "id": "1399",
      "name": "Bo B - FAIL (8 chung tu, 3 loi)",
      "workflowRequestId": null,
      "workflowId": "5ed8e4fc-27a5-45a7-b104-1216fcfd0797",
      "tenantId": "176",
      "status": 3,
      "createdAt": "2026-08-28T03:56:39.397Z",
      "updatedAt": "2026-08-28T04:06:36.100Z",
      "autoStart": false
    }
  ],
  "totalItems": 15
}
```

#### `GET /workflow/export`

**Xuất workflow ra file**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Trả file zip.

**Response**

```json
{
  "__binary__": true,
  "content_type": "application/zip",
  "bytes": 22
}
```

#### `GET /workflow/share-with-admin/{flowId}`

*Gọi thật: `/workflow/share-with-admin/8d873709-4cac-4f26-82f1-b1f9d28c6e73`*

**Chia sẻ một flow với admin**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ End User** (Admin: None) | `GET` | `200` |

> **Lệnh hành động**, không trả dữ liệu. Chuỗi rỗng nghĩa là đã chia sẻ xong.

**Response**

```json
""
```

**Cấu trúc khi có dữ liệu**

```json
// Endpoint hành động — luôn trả đúng giá trị này, không có biến thể.
""   // chuỗi rỗng — đã chia sẻ xong
```

#### `GET /workflow/sync-bo-workflows`

**Đồng bộ workflow từ BO — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

> Lệnh đồng bộ; trả `{message: "success"}` là kết quả đầy đủ của nó.

**Response**

```json
{
  "message": "success"
}
```

**Cấu trúc khi có dữ liệu**

```json
// Endpoint hành động — luôn trả đúng giá trị này, không có biến thể.
{ "message": "success" }
```

#### `GET /workflow/forwarder/health_check`

**Health check của Langflow đứng sau**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Kiểm tra nhanh xem tầng đánh giá còn sống không — nên đưa vào giám sát.

**Response**

```json
{
  "status": "ok",
  "chat": "ok",
  "db": "ok"
}
```

#### `GET /workflow/forwarder/api/v1/flows`

**Chi tiết một flow theo id**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `None` · End User `None`) | `GET` | `200` |

> Nhẹ hơn rất nhiều so với `GET /workflow/forwarder/api/v1/flows` (2,8 MB). Nếu adapter đã biết `workflowId` thì dùng endpoint này.

**Response**

```json
{
  "items": [
    {
      "endpoint_name": null,
      "icon": null,
      "tags": null,
      "description": "Workflow Claim 28/8 bỏ phần convert markdown cuối cùng.",
      "data": {
        "nodes": [
          {
            "id": "Prompt-TSAeA",
            "type": "genericNode",
            "position": {
              "x": 9665.50882208022,
              "y": 2530.878818850673
            },
            "data": {
              "id": "Prompt-TSAeA",
              "node": {
                "template": {
                  "code": {
                    "info": "",
                    "list": false,
                    "name": "code",
                    "show": true,
                    "type": "code",
                    "value": "from langflow.base.prompts.api_utils import process_prompt_template\nfrom langflow.custom import Component\nfrom langflow.inputs.inputs import DefaultPromptField\nfrom langflow.io import Output, PromptInput\nfrom langflow.schema.message import Message\nfrom langflow.template.utils import update_template_values\n\n\nclass PromptComponent(Component):\n    display_name: str = \"Prompt\"\n    description: str = \"Create a prompt template with dynamic variables.\"\n    icon = \"prompts\"\n    trace_type = \"prompt\"\n    name = \"Prompt\"\n\n    inputs = [\n        PromptInput(name=\"template\", display_name=\"Template\"),\n    ]\n\n    outputs = [\n
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /workflow/forwarder/api/v1/flows/assigns/all?target_tenant_id={tenantId}`

*Gọi thật: `/workflow/forwarder/api/v1/flows/assigns/all?target_tenant_id=176`*

**Danh sách gán flow theo tenant** — bắt buộc có `target_tenant_id`**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

> ⚠️ **Thiếu `target_tenant_id` thì trả `500 · invalid input syntax for type bigint: "NaN"`.** Code làm `parseInt(targetTenantId)` trên một giá trị `undefined`.
>
> ✅ **Có tham số thì trả `200`.** Payload dưới gọi với `target_tenant_id=176`.

**Response**

```json
{
  "assigned": [
    "236",
    "237"
  ],
  "flows": [
    {
      "id": "236",
      "name": "Workflow Claim 25/08",
      "description": "Workflow Claim 25/08"
    },
    {
      "id": "237",
      "name": "Workflow Claim 28/8",
      "description": "Workflow Claim 28/8 bỏ phần convert markdown cuối cùng."
    }
  ],
  "unassigned": []
}
```

#### `GET /workflow/forwarder/api/v1/auto_login`

**Auto login vào Langflow — **luôn trả 400 theo thiết kế****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `400` · End User `400`) | `GET` | `400` |

> Không phải lỗi: controller hardcode `400` với thông báo *"Auto login is disabled. Please enable it in the settings"*. Hệ thống claim không cần endpoint này.

**Response**

```json
{
  "detail": {
    "message": "Auto login is disabled. Please enable it in the settings",
    "auto_login": false
  }
}
```

#### `GET /tool`

**Gốc của nhóm tool vận hành**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

> Trả `"Hello World!"` — **không phải danh sách tool**, chỉ là route gốc của controller.
>
> Các `POST /tool/*` là thao tác vận hành nguy hiểm — `redis-reset`, `update-format-db`, `move-document-to-minio`, `force-update-workflow-status`. **Không gọi**, và hệ thống claim cũng không nên đụng tới.

**Response**

```json
"Hello World!"
```

**Cấu trúc khi có dữ liệu**

```json
// Endpoint hành động — luôn trả đúng giá trị này, không có biến thể.
"Hello World!"
```


---

## 2.6 Master data — enum và bảng tra

> **Nhóm này ít được để ý nhưng có giá trị cao nhất cho Claim MVP.** Nó chứa danh mục bệnh viện, quốc gia, mã phẫu thuật, loại claim và loại chứng từ — những thứ mà hệ thống claim vẫn đang phải tự dựng.

Đường lấy dữ liệu **không phải** `GET /metadata/forwarder/table-data/{id}` (endpoint đó trả `400`), mà qua **proxy chung**:

```
GET /metadata/forwarder/api/database/rows/table/{tableId}/?user_field_names=true&size=200
GET /metadata/forwarder/api/database/fields/table/{tableId}/
```

#### `GET /metadata/forwarder/table-list`

**Danh sách bảng master data**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Năm bảng, và cả năm đều liên quan trực tiếp tới bài toán claim.

**Response**

```json
{
  "tables": [
    {
      "id": 322,
      "name": "Hospital",
      "order": 74,
      "database_id": 6,
      "data_sync": null
    },
    {
      "id": 321,
      "name": "Country",
      "order": 73,
      "database_id": 6,
      "data_sync": null
    },
    {
      "id": 316,
      "name": "Surgery Code",
      "order": 69,
      "database_id": 6,
      "data_sync": null
    },
    {
      "id": 314,
      "name": "MBAL Document Types",
      "order": 67,
      "database_id": 6,
      "data_sync": null
    },
    {
      "id": 313,
      "name": "MBAL Claim Types",
      "order": 66,
      "database_id": 6,
      "data_sync": null
    }
  ],
  "pagination": {
    "page": 1,
    "page_size": 10,
    "total": 5
  }
}
```

#### `GET /metadata/forwarder/api/database/rows/table/{tableId}/?user_field_names=true&size={n}`

*Gọi thật: `/metadata/forwarder/api/database/rows/table/313/?user_field_names=true&size=200`*

**Enum `claim_type`** — 4 giá trị**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Chính là giá trị hợp lệ cho `POST /mbal/create-request`.

**Response**

```json
{
  "count": 4,
  "next": null,
  "previous": null,
  "results": [
    {
      "id": 3,
      "order": "1.00000000000000000000",
      "claim_type": "MEDICAL_CARE",
      "Notes": "",
      "Active": false
    },
    {
      "id": 4,
      "order": "2.00000000000000000000",
      "claim_type": "MEDICAL_ILLNESS",
      "Notes": "",
      "Active": false
    },
    {
      "id": 5,
      "order": "3.00000000000000000000",
      "claim_type": "MEDICAL_INJURY",
      "Notes": "",
      "Active": false
    },
    {
      "id": 6,
      "order": "4.00000000000000000000",
      "claim_type": "MEDICAL_DEATH",
      "Notes": "",
      "Active": false
    }
  ]
}
```

#### `GET /metadata/forwarder/api/database/rows/table/{tableId}/?user_field_names=true&size={n}`

*Gọi thật: `/metadata/forwarder/api/database/rows/table/314/?user_field_names=true&size=200`*

**Enum `document_type`** — 10 mã `CL*`**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Ghép với `claim_type` để tra ra bộ chứng từ qua `mbal/docset-map`.

**Response**

```json
{
  "count": 10,
  "next": null,
  "previous": null,
  "results": [
    {
      "id": 7,
      "order": "1.00000000000000000000",
      "Name": "CL05",
      "Notes": "",
      "Active": false
    },
    {
      "id": 8,
      "order": "2.00000000000000000000",
      "Name": "CL06",
      "Notes": "",
      "Active": false
    },
    {
      "id": 9,
      "order": "3.00000000000000000000",
      "Name": "CL07",
      "Notes": "",
      "Active": false
    },
    {
      "id": 10,
      "order": "4.00000000000000000000",
      "Name": "CL08",
      "Notes": "",
      "Active": false
    },
    {
      "id": 11,
      "order": "5.00000000000000000000",
      "Name": "CL10",
      "Notes": "",
      "Active": false
    },
    {
      "id": 12,
      "order": "6.00000000000000000000",
      "Name": "CL11",
      "Notes": "",
      "Active": false
    },
    {
      "id": 13,
      "order": "7.00000000000000000000",
      "Name": "CL12",
      "Notes": "",
      "Active": false
    },
    {
      "id": 14,
      "order": "8.00000000000000000000",
      "Name": "CL15",
      "Notes": "",
      "Active": false
    },
    {
      "id": 15,
      "order": "9.00000000000000000000",
      "Name": "CL26",
      "Notes": "",
      "Active": false
    },
    {
      "id": 16,
      "order": "10.00000000000000000000",
      "Name": "CLM_04",
      "Notes": "",
      "Active": false
    }
  ]
}
```

#### `GET /metadata/forwarder/api/database/rows/table/{tableId}/?user_field_names=true&size={n}`

*Gọi thật: `/metadata/forwarder/api/database/rows/table/316/?user_field_names=true&size=200`*

**Bảng mã phẫu thuật** — 95 dòng**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Cột: `surgery_name`, `surgery_code`. Payload cắt bớt.

**Response**

```json
{
  "count": 95,
  "next": null,
  "previous": null,
  "results": [
    {
      "id": 3,
      "order": "3.00000000000000000000",
      "surgery_name": "phẫu thuật Cắt ruột thừa",
      "surgery_code": "1.1",
      "Active": false
    },
    {
      "id": 4,
      "order": "4.00000000000000000000",
      "surgery_name": "phẫu thuật Nối thông dạ dày - ruột non",
      "surgery_code": "1.2",
      "Active": false
    },
    {
      "id": 5,
      "order": "5.00000000000000000000",
      "surgery_name": "phẫu thuật Cắt bỏ túi mật hoặc khác trên túi mật",
      "surgery_code": "1.3",
      "Active": false
    },
    {
      "id": 6,
      "order": "6.00000000000000000000",
      "surgery_name": "phẫu thuật Cắt đoạn ruột non",
      "surgery_code": "1.4",
      "Active": false
    },
    {
      "id": 7,
      "order": "7.00000000000000000000",
      "surgery_name": "phẫu thuật Cắt bỏ toàn bộ trực tràng",
      "surgery_code": "1.5",
      "Active": false
    },
    {
      "id": 8,
      "order": "8.00000000000000000000",
      "surgery_name": "phẫu thuật Đặt stent thực quản",
      "surgery_code": "1.6",
      "Active": false
    },
    {
      "id": 9,
      "order": "9.0000000000000
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /metadata/forwarder/api/database/rows/table/{tableId}/?user_field_names=true&size={n}`

*Gọi thật: `/metadata/forwarder/api/database/rows/table/321/?user_field_names=true&size=200`*

**Bảng quốc gia** — 242 dòng**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Cột: `country_name`, `country_code`, `country_iso3`. Payload cắt bớt.

**Response**

```json
{
  "count": 242,
  "next": "http://metadata-svc.idp-staging/api/database/rows/table/321/?page=2&size=200&user_field_names=true",
  "previous": null,
  "results": [
    {
      "id": 3,
      "order": "1.00000000000000000000",
      "country_name": "Gambia",
      "country_code": "945",
      "country_iso3": "GMB"
    },
    {
      "id": 4,
      "order": "2.00000000000000000000",
      "country_name": "Kiribati",
      "country_code": "959",
      "country_iso3": "KIR"
    },
    {
      "id": 5,
      "order": "3.00000000000000000000",
      "country_name": "Benin",
      "country_code": "913",
      "country_iso3": "BEN"
    },
    {
      "id": 6,
      "order": "4.00000000000000000000",
      "country_name": "Uzbekistan",
      "country_code": "437",
      "country_iso3": "UZB"
    },
    {
      "id": 7,
      "order": "5.00000000000000000000",
      "country_name": "Dominica",
      "country_code": "934",
      "country_iso3": "DMA"
    },
    {
      "id": 8,
      "order": "6
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /metadata/forwarder/api/database/rows/table/{tableId}/?user_field_names=true&size={n}`

*Gọi thật: `/metadata/forwarder/api/database/rows/table/322/?user_field_names=true&size=200`*

**Bảng bệnh viện** — 13.328 dòng, có cột `country`**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

> **Đây là bảng đóng được `facilityCountry` (Q-IDP-05).** Tên cơ sở khám chữa bệnh trích ra từ chứng từ, tra bảng này, ra quốc gia — đủ để loại trừ #10 *"điều trị ngoài Việt Nam"* chạy tự động thay vì rơi xuống checkpoint thủ công. Payload cắt bớt.

**Response**

```json
{
  "count": 13328,
  "next": "http://metadata-svc.idp-staging/api/database/rows/table/322/?page=2&size=200&user_field_names=true",
  "previous": null,
  "results": [
    {
      "id": 3,
      "order": "3.00000000000000000000",
      "hospital_name": "BỆNH VIỆN HỮU NGHỊ",
      "hospital_code": "01001",
      "country": "Viet Nam"
    },
    {
      "id": 4,
      "order": "4.00000000000000000000",
      "hospital_name": "PHÒNG KHÁM A THUỘC BVĐK XANH PÔN",
      "hospital_code": "01002",
      "country": "Viet Nam"
    },
    {
      "id": 5,
      "order": "5.00000000000000000000",
      "hospital_name": "BỆNH VIỆN ĐA KHOA XANH PÔN",
      "hospital_code": "01003",
      "country": "Viet Nam"
    },
    {
      "id": 6,
      "order": "6.00000000000000000000",
      "hospital_name": "BỆNH VIỆN ĐA KHOA ĐỐNG ĐA",
      "hospital_code": "01004",
      "country": "Viet Nam"
    },
    {
      "id": 7,
      "order": "7.00000000000000000000",
      "hospital_name": "BỆNH VIỆN HỮU NGHỊ VIỆ
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /metadata/forwarder/api/database/fields/table/{tableId}/`

*Gọi thật: `/metadata/forwarder/api/database/fields/table/322/`*

**Định nghĩa cột của một bảng master data**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: None) | `GET` | `200` |

**Response**

```json
[
  {
    "id": 2402,
    "table_id": 322,
    "name": "hospital_name",
    "order": 0,
    "type": "text",
    "primary": true,
    "read_only": false,
    "immutable_type": false,
    "immutable_properties": false,
    "description": null,
    "text_default": ""
  },
  {
    "id": 2403,
    "table_id": 322,
    "name": "hospital_code",
    "order": 1,
    "type": "long_text",
    "primary": false,
    "read_only": false,
    "immutable_type": false,
    "immutable_properties": false,
    "description": null,
    "long_text_enable_rich_text": false
  },
  {
    "id": 2405,
    "table_id": 322,
    "name": "country",
    "order": 3,
    "type": "text",
    "primary": false,
    "read_only": false,
    "immutable_type": false,
    "immutable_properties": false,
    "description": null,
    "text_default": ""
  }
]
```

#### `GET /metadata/opensearch/table/{tableId}/index/async`

*Gọi thật: `/metadata/opensearch/table/322/index/async`*

**Tạo index tìm kiếm cho một bảng master data**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: None) | `GET` | `200` |

Chạy bất đồng bộ. Phải chạy một lần trước khi endpoint search dùng được — bảng bệnh viện lúc đầu chưa có index nên search trả `400`.

**Response**

```json
{
  "id": 322,
  "progressPercentage": 100,
  "totalRecords": 13328,
  "indexedRecords": 0,
  "state": "pending",
  "message": "Pending..."
}
```

#### `GET /metadata/opensearch/table/{tableId}/index/status`

*Gọi thật: `/metadata/opensearch/table/322/index/status`*

**Tiến độ đánh index**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Đã chạy thật trên bảng bệnh viện: `13.328` bản ghi, đi từ `26%` đến `finished` trong khoảng 30 giây.

**Response**

```json
{
  "id": 322,
  "progressPercentage": 100,
  "totalRecords": 13328,
  "indexedRecords": 13328,
  "state": "finished",
  "message": "Successfully indexed data for table 322"
}
```

#### `GET /metadata/opensearch/table/{tableId}/info`

*Gọi thật: `/metadata/opensearch/table/322/info`*

**Trạng thái index của một bảng**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Trước khi đánh index thì trả `{ exists: false, message: "Index does not exist" }`.

**Response**

```json
{
  "exists": true,
  "indexName": "metadata_table_322",
  "columnInfo": [
    {
      "id": 2402,
      "columnName": "hospital_name",
      "columnKey": "hospital_name",
      "type": "text",
      "primary": true,
      "description": ""
    },
    {
      "id": 2403,
      "columnName": "hospital_code",
      "columnKey": "hospital_code",
      "type": "long_text",
      "primary": false,
      "description": ""
    },
    {
      "id": 2405,
      "columnName": "country",
      "columnKey": "country",
      "type": "text",
      "primary": false,
      "description": ""
    }
  ],
  "documentCount": 13328,
  "statistics": {
    "uuid": "D4nSCRkATy-9R3CnZ1molg",
    "primaries": {
      "docs": {
        "count": 13328,
        "deleted": 0
      },
      "store": {
        "size_in_bytes": 1812577,
        "reserved_in_bytes": 0
      },
      "indexing": {
        "index_total": 13328,
        "index_time_in_millis": 2613,
        "index_current": 0,
        "index_failed": 0,
        "delete_total": 0,
        "delete_time_in_millis": 0,
        "delete_current": 0,
        "noop_update_total": 0,
        "is_throttled": false,
        "throttle_time_in_millis": 0,
        "doc_status": {}
      },
      "get": {
        "total": 0,
        "time_in_millis": 0,
        "exists_total": 0,
        "exists_time_in_millis": 0,
        "missing_total": 0,
        "missing_time_in_millis": 0,
        "current": 0
      },
      "search": {
        "open_contexts": 0,
        "query_total": 1,
        "query_time_in_millis": 2,
        "query_current": 0,
        "concurrent_query_total": 0,
        "concurrent_query_time_in_millis": 0,
        "concurrent_query_current": 0,
        "concurrent_avg_slice_count": 0,
        "fetch_total": 1,
        "fetch_time_in_millis": 0,
        "fetch_current": 0,
        "scroll_total": 0,
        "scroll_time_in_millis": 0,
        "scroll_current": 0,
        "point_in_time_total": 0,
        "point_in_time_time_in_millis": 0,
        "point_in_time_current": 0,
        "suggest_total": 0,
        "suggest_time_in_millis": 0,
        "suggest_current": 0,
        "search_idle_reactivate_count_total": 0
      },
      "merges": {
        "current": 0,
        "current_docs": 0,
        "current_size_in_bytes": 0,
        "total": 7,
        "total_time_in_millis": 2995,
        "total_docs": 34800,
        "total_size_in_bytes": 5504289,
        "total_stopped_time_in_millis": 0,
        "total_throttled_time_in_millis": 0,
        "total_auto_throttle_in_bytes": 20971520,
        "unreferenced_file_cleanups_perfo
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /metadata/opensearch/table/322/search?searchTerm={term}&fields={column}&size={n}`

*Gọi thật: `/metadata/opensearch/table/322/search?searchTerm=N%E1%BB%99i%20ti%E1%BA%BFt&fields=hospital_name&size=5`*

**Tra cứu toàn văn bảng master data** — có score và highlight**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Tham số: `searchTerm` (bắt buộc), `fields` (bắt buộc, tên cột), `size`, `from`.

> ✅ **Đây là cách đúng để tra bảng bệnh viện 13.328 dòng** thay vì tải cả bảng về. Tìm *"Nội tiết"* ra `344` kết quả, mỗi kết quả có `score` và `highlights`.
>
> **Với Claim MVP:** tên cơ sở khám chữa bệnh trích ra từ chứng từ → tra bảng này → ra `hospital_code` và `country`. Đây là đường đóng gap `facilityCountry` (**Q-IDP-05**) mà không cần IDP thay đổi gì.

**Response**

```json
{
  "total": 344,
  "results": [
    {
      "id": 2160,
      "score": 55.240772,
      "data": {
        "id": 2160,
        "hospital_name": "BỆNH VIỆN NỘI TIẾT",
        "hospital_code": "15113",
        "country": "Viet Nam"
      },
      "highlights": {
        "hospital_name": [
          "BỆNH VIỆN <mark>NỘI</mark> <mark>TIẾT</mark>"
        ]
      }
    },
    {
      "id": 1912,
      "score": 55.240772,
      "data": {
        "id": 1912,
        "hospital_name": "BỆNH VIỆN NỘI TIẾT",
        "hospital_code": "15113",
        "country": "Viet Nam"
      },
      "highlights": {
        "hospital_name": [
          "BỆNH VIỆN <mark>NỘI</mark> <mark>TIẾT</mark>"
        ]
      }
    },
    {
      "id": 251,
      "score": 51.542492,
      "data": {
        "id": 251,
        "hospital_name": "BỆNH VIỆN NỘI TIẾT TW",
        "hospital_code": "",
        "country": "Viet Nam"
      },
      "highlights": {
        "hospital_name": [
          "BỆNH VIỆN <mark>NỘI</mark> <mark>TIẾT</mark> TW"
        ]
      }
    },
    {
      "id": 3230,
      "score": 45.456055,
      "data": {
        "id": 3230,
        "hospital_name": "BỆNH VIỆN NỘI TIẾT TỈNH BẮC GIANG",
        "hospital_code": "24023",
        "country": "Viet Nam"
      },
      "highlights": {
        "hospital_name": [
          "BỆNH VIỆN <mark>NỘI</mark> <mark>TIẾT</mark> TỈNH BẮC GIANG"
        ]
      }
    },
    {
      "id": 6332,
      "score": 45.456055,
      "data": {
        "id": 6332,
        "hospital_name": "BỆNH VIỆN NỘI TIẾT TỈNH NGHỆ AN",
        "hospital_code": "40065",
        "country": "Viet Nam"
      },
      "highlights": {
        "hospital_name": [
          "BỆNH VIỆN <mark>NỘI</mark> <mark>TIẾT</mark> TỈNH NGHỆ AN"
        ]
      }
    }
  ]
}
```

#### `GET /metadata/forwarder/api/database/rows/table/{tableId}/{rowId}/?user_field_names=true`

*Gọi thật: `/metadata/forwarder/api/database/rows/table/322/3/?user_field_names=true`*

**Đọc một dòng master data theo id**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Rẻ hơn nhiều so với tra cả bảng khi đã biết id — ví dụ khi đã cache `hospital_code` và chỉ cần đọc lại một bản ghi.

**Response**

```json
{
  "id": 3,
  "order": "3.00000000000000000000",
  "hospital_name": "BỆNH VIỆN HỮU NGHỊ",
  "hospital_code": "01001",
  "country": "Viet Nam"
}
```

#### `GET /metadata/forwarder/table-fields/{tableId}`

*Gọi thật: `/metadata/forwarder/table-fields/313`*

**Định nghĩa cột của một bảng master data** — biến thể không qua proxy**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

> Endpoint này **chạy được**, khác với `table-data/{id}` cùng nhóm vốn cần view id. Trả kiểu dữ liệu và thuộc tính của từng cột.

**Response**

```json
[
  {
    "id": 2373,
    "table_id": 313,
    "name": "claim_type",
    "order": 0,
    "type": "text",
    "primary": true,
    "read_only": false,
    "immutable_type": false,
    "immutable_properties": false,
    "description": null,
    "text_default": ""
  },
  {
    "id": 2374,
    "table_id": 313,
    "name": "Notes",
    "order": 1,
    "type": "long_text",
    "primary": false,
    "read_only": false,
    "immutable_type": false,
    "immutable_properties": false,
    "description": null,
    "long_text_enable_rich_text": false
  },
  {
    "id": 2375,
    "table_id": 313,
    "name": "Active",
    "order": 2,
    "type": "boolean",
    "primary": false,
    "read_only": false,
    "immutable_type": false,
    "immutable_properties": false,
    "description": null
  }
]
```

#### `GET /metadata/forwarder/trash`

**Thùng rác của master data**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

Xem các bảng/dòng đã xoá, và có endpoint khôi phục kèm theo.

**Response**

```json
{
  "workspaces": [
    {
      "id": 1,
      "trashed": false,
      "name": "bnk's workspace",
      "applications": [
        {
          "id": 6,
          "name": "Default database",
          "trashed": false
        }
      ]
    }
  ]
}
```

#### `GET /metadata/forwarder/api/database/views/table/{tableId}/`

*Gọi thật: `/metadata/forwarder/api/database/views/table/322/`*

**Lấy view id của một bảng master data**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

> Bước bắt buộc trước khi gọi `table-data` — endpoint đó nhận **view id**, không phải table id. Bảng `322` (Hospital) có view id **476**.

**Response**

```json
[
  {
    "id": 476,
    "table_id": 322,
    "name": "Grid",
    "order": 1,
    "type": "grid",
    "table": {
      "id": 322,
      "name": "Hospital",
      "order": 74,
      "database_id": 6
    },
    "filter_type": "AND",
    "filters_disabled": false,
    "public_view_has_password": false,
    "show_logo": true,
    "ownership_type": "collaborative",
    "owned_by_id": 1,
    "row_identifier_type": "id",
    "row_height_size": "small",
    "public": false,
    "slug": "g-iVVfOo-tJBmQpd3ARAx6kMwptqobX8wlKuAwJwSh0"
  }
]
```

#### `GET /metadata/forwarder/table-data/{viewId}`

*Gọi thật: `/metadata/forwarder/table-data/476`*

**Lấy dữ liệu bảng theo view id** — kèm cả field options**

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Cả hai** | `GET` | `200` |

> ⚠️ **Đây là nguồn gốc của hiểu nhầm.** Tên endpoint là `table-data/{id}` nhưng `{id}` phải là **VIEW ID**, không phải table id. Gọi bằng table id `322` trả `400 · Metadata request failed.`; gọi bằng view id `476` trả `200` với đủ 13.328 bản ghi.
>
> Code gọi thẳng Baserow ở `/api/database/views/grid/{id}/`, nên tham số phải là view.
>
> **So với proxy `api/database/rows`:** endpoint này trả kèm `field_options` và `row_metadata`, nhưng tên cột ở dạng `field_2402` thay vì tên thật. Muốn tên cột dễ đọc thì dùng proxy với `user_field_names=true`.

**Response**

```json
{
  "count": 13328,
  "next": "http://metadata-svc.idp-staging/api/database/views/grid/476/?include=field_options%2Crow_metadata&limit=80&offset=80",
  "previous": null,
  "results": [
    {
      "id": 3,
      "order": "3.00000000000000000000",
      "field_2402": "BỆNH VIỆN HỮU NGHỊ",
      "field_2403": "01001",
      "field_2405": "Viet Nam"
    },
    {
      "id": 4,
      "order": "4.00000000000000000000",
      "field_2402": "PHÒNG KHÁM A THUỘC BVĐK XANH PÔN",
      "field_2403": "01002",
      "field_2405": "Viet Nam"
    },
    {
      "id": 5,
      "order": "5.00000000000000000000",
      "field_2402": "BỆNH VIỆN ĐA KHOA XANH PÔN",
      "field_2403": "01003",
      "field_2405": "Viet Nam"
    },
    {
      "id": 6,
      "order": "6.00000000000000000000",
      "field_2402": "BỆNH VIỆN ĐA KHOA ĐỐNG ĐA",
      "field_2403": "01004",
      "field_2405": "Viet Nam"
    },
    {
      "id": 7,
      "order": "7.00000000000000000000",
      "field_2402": "BỆNH VIỆN HỮU NGHỊ VIỆT NAM - CU BA",
      "field_2403": "01005",
      "field_2405": "Viet Nam"
    },
    {
      "id": 8,
      "order": "8.00000000000000000000",
      "field_2402": "BỆNH VIỆN THANH NHÀN",
      "field_2403": "01006",
      "field_2405": "Viet Nam"
    },
    {
      "id": 9,
      "order": "9.00000000000000000000",
      "field_2402": "BỆNH VIỆN E",
      "field_2403": "01007",
      "field_2405": "Viet Nam"
    },
    {
      "id": 10,
      "order": "10.00000000000000000000",
      "field_2402": "BỆNH VIỆN BƯU ĐIỆN HÀ NỘI",
      "field_2403": "",
      "field_2405": "Viet Nam"
    },
    {
      "id": 11,
      "order": "11.00000000000000000000",
      "field_2402": "CÔNG TY CỔ PHẦN BỆNH VIỆN GIAO THÔNG VẬN TẢI",
      "field_2403": "01010",
      "field_2405": "Viet Nam"
    },
    {
      "id": 12,
      "order": "12.00000000000000000000",
      "field_2402": "PHÒNG KHÁM ĐA KHOA KHU VỰC ĐÔNG MỸ",
      "field_2403": "01012",
      "field_2405": "Viet Nam"
    },
    {
      "id": 13,
      "order": "13.00000000000000000000",
      "field_2402": "BỆNH VIỆN ĐA KHOA NÔNG NGHIỆP",
      "field_2403": "01013",
      "field_2405": "Viet Nam"
    },
    {
      "id": 14,
      "o
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `POST /metadata/forwarder/table-data`

**Tạo bảng master data mới — **không phải endpoint đọc dữ liệu****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| Cả hai vai đều lỗi (Admin `400` · End User `None`) | `POST` | `400` |

> ⚠️ **Swagger đặt tên gây hiểu nhầm.** `POST /metadata/forwarder/table-data` trong code có summary *"Create a new table"* và nhận `CreateTableDto`. Nó **tạo bảng**, không phải lấy dữ liệu.
>
> Payload dưới là kết quả gọi với body sai mục đích (`{table_id: 313}`) — giữ lại để thấy thông báo lỗi trông giống hệt lỗi của endpoint đọc, và đó là lý do dễ nhầm.

**Request**

```json
{
  "table_id": 313,
  "page": 1,
  "size": 10
}
```

**Response**

```json
{
  "statusCode": 400,
  "message": "Metadata request failed."
}
```

#### `GET /metadata/forwarder/database/list`

**Danh sách database chứa các bảng master data — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

**Response**

```json
[
  {
    "id": 6,
    "name": "Default database",
    "order": 1,
    "type": "database",
    "workspace": {
      "id": 1,
      "name": "bnk's workspace",
      "generative_ai_models_enabled": {}
    },
    "created_on": "2025-05-20T03:06:31.539279Z",
    "tables": [
      {
        "id": 16,
        "name": "Báo cáo tài chính",
        "order": 1,
        "database_id": 6,
        "data_sync": null
      },
      {
        "id": 30,
        "name": "Hospital code MBAL",
        "order": 3,
        "database_id": 6,
        "data_sync": null
      },
      {
        "id": 31,
        "name": "ICD code",
        "order": 4,
        "database_id": 6,
        "data_sync": null
      },
      {
        "id": 33,
        "name": "Nguyễn Sơn Quyền",
        "order": 6,
        "database_id": 6,
        "data_sync": null
      },
      {
        "id": 34,
        "name": "Nguyễn Sơn Quyềncx",
        "order": 7,
        "database_id": 6,
        "data_sync": null
      },
      {
        "id": 35,
        "name": "Híuu",
        "order": 8,
        "database_id": 6,
        "data_sync": null
      },
      {
        "id": 36,
        "name": "Check log",
        "order": 9,
        "da
  … [CẮT BỚT — payload đầy đủ ở request-response-json/]
```

#### `GET /metadata/forwarder/partner-table-list`

**Bảng master data theo partner — **chỉ Admin****

| Ai gọi được | Method | Trạng thái thật |
|---|---|---|
| **Chỉ Admin** (End User: 403) | `GET` | `200` |

**Response**

```json
{
  "tables": [
    {
      "id": 322,
      "name": "Hospital",
      "order": 74,
      "database_id": 6,
      "data_sync": null
    },
    {
      "id": 321,
      "name": "Country",
      "order": 73,
      "database_id": 6,
      "data_sync": null
    },
    {
      "id": 316,
      "name": "Surgery Code",
      "order": 69,
      "database_id": 6,
      "data_sync": null
    },
    {
      "id": 314,
      "name": "MBAL Document Types",
      "order": 67,
      "database_id": 6,
      "data_sync": null
    },
    {
      "id": 313,
      "name": "MBAL Claim Types",
      "order": 66,
      "database_id": 6,
      "data_sync": null
    }
  ],
  "assigned_table_ids": [],
  "not_assigned_table_ids": [
    322,
    321,
    316,
    314,
    313
  ],
  "pagination": {
    "page": 1,
    "page_size": 10,
    "total": 5
  }
}
```


---

# PHỤ LỤC — Enum

## Enum trạng thái

**Request OCR** — `RequestStatus`

| Giá trị | Text | Nghĩa |
|---|---|---|
| `0` | `Sending` | Đang đẩy sang lõi OCR |
| `1` | `Processing` | Đang xử lý |
| `2` | `Completed` | Xong |
| `3` | `Failed` | Lỗi |
| `4` | `Cancelling` | Đang huỷ |
| `5` | `Cancelled` | Đã huỷ |

**Workflow đánh giá** — `WorkflowRequestStatus`

| Giá trị | Tên |
|---|---|
| `1` | `WaitingForInput` |
| `7` | `InQueue` |
| `2` | `Running` |
| `3` | `Finished` |
| `4` | `Error` |
| `5` | `NotFound` |
| `6` | `Unknown` |

## Enum nghiệp vụ — từ master data

**`claim_type`** (bảng 313): `MEDICAL_CARE` · `MEDICAL_ILLNESS` · `MEDICAL_INJURY` · `MEDICAL_DEATH`

**`document_type`** (bảng 314): `CL05` · `CL06` · `CL07` · `CL08` · `CL10` · `CL11` · `CL12` · `CL15` · `CL26` · `CLM_04`

**Bộ chứng từ hiện có** (`/template/document-set`): `193` — *Chứng từ y tế _ CSYT* (7 template) · `194` — *Claim assessment Demo* (7 template)

## Vì sao một số endpoint trả response rỗng

Đọc lướt tài liệu sẽ thấy một số mục có `Response` là `""`, `[]`, `{}` hoặc `{items: [], totalItems: 0}`. **Không cái nào trong số đó là endpoint hỏng.** Ba lý do, và không lý do nào là lỗi:

**1. Endpoint hành động — trả rỗng là đúng (7 cái).** Chúng làm một việc rồi kết thúc, không có dữ liệu để trả.

| Endpoint | Trả về | Bản chất |
|---|---|---|
| `GET /` | `"Hello World!"` | Health check |
| `GET /tool` | `"Hello World!"` | Route gốc của controller, **không phải danh sách tool** |
| `GET /template/fill-owner` | `""` | Lệnh bảo trì: điền `created_by` còn trống |
| `GET /template/sync-bo-templates` | `""` | Lệnh đồng bộ template từ BO |
| `GET /workflow/share-with-admin/{flowId}` | `""` | Lệnh chia sẻ flow |
| `GET /workflow/sync-bo-workflows` | `{message: "success"}` | Lệnh đồng bộ workflow |
| `GET /request/{id}/check-workflow-request` | `{message: "Done"}` | Lời xác nhận |

**2. Rỗng vì môi trường staging chưa có dữ liệu (6 cái).** Endpoint chạy đúng, chỉ là chưa ai tạo dữ liệu.

| Endpoint | Trả về | Vì sao |
|---|---|---|
| `GET /user/compact-list-to-share` | `[]` | Tenant test chỉ có một người dùng |
| `GET /workflow/forwarder/api/v1/folders` | `[]` | Chưa tạo thư mục flow |
| `GET /workflow/forwarder/api/v1/variables` | `[]` | Chưa khai báo biến toàn cục |
| `GET /workflow/forwarder/api/v1/store/tags` | `[]` | Kho flow chưa gắn tag |
| `GET /workflow/forwarder/api/v1/config` | `{}` | Không expose cấu hình công khai |

**3. Rỗng vì gọi nhầm vai — đã sửa (1 cái).** `GET /workflow/request/search` lọc dữ liệu theo `tenantId` trong token. Bản đầu của tài liệu này render bằng tài khoản Admin (tenant 175) nên ra `{items: [], totalItems: 0}`; gọi bằng End User (tenant 176) thì có bản ghi. **Payload trong tài liệu đã được thay bằng bản gọi đúng vai.** Đây là bài học chung: với mọi endpoint lọc theo tenant, response rỗng có thể chỉ nghĩa là *sai tài khoản*, không phải *không có dữ liệu*.

