# Spec — Tích hợp BnK IDP

> **Văn bản đầy đủ:** Project → `claude/build-02-tich-hop-idp`.
> Nguồn cấp trên trên đĩa: `../../Preparing/03_BnK-IDP/` và `../../insurance BNK/IDP_API_Spec/`.

## Cấu hình chốt

| Mục | Giá trị |
|---|---|
| Tenant | **176** (Phòng 1) · user 203 · partner 40 |
| Base URL | `https://idp-api.idpstg.bnksolution.com` |
| Document set | `HEALTH_CLAIM_V1` — **set 194**, KHÔNG dùng 193 |
| Flow id | **`5ed8e4fc`** — ghim vào Site Setting |
| Endpoint | 32 tổng: 14 P0 · 14 P1 · 2 hỏng · 2 cấm |

## Sáu quirk không được quên

1. **`User-Agent` giả trình duyệt** trên **mọi** method — thiếu là Cloudflare 403 1010. Set trước khi test method thứ hai.
2. **`templates` truyền dạng chuỗi phẩy**, không phải mảng.
3. **HTTP 200 vẫn có thể là lỗi** — luôn đọc `errorCode` trong body. `raw-file` trả zip.
4. **`autoStart` KHÔNG hoạt động** — phải gọi `run-workflow-request`, và endpoint này lúc 400 lúc 200 với cùng tham số ⇒ cần ngoại lệ retry cho `4xx`.
5. **`PUT /request/{id}/result` chạy được** ⇒ kết quả IDP không bất biến ⇒ **cấm dùng endpoint này**, và tự copy `RawExtractionJson` ngay lần đọc đầu.
6. **`extraParams` là type đóng, thực tế trả `{}`** ⇒ correlate bằng trường **`name` = `ClaimNo`**.

## Hai chỗ chặn, chưa có câu trả lời từ đội IDP

- `Q-IDP-01` — **crosswalk document-class đang rỗng**; `classification` hiện chạy trên ánh xạ giả định. **CHẶN phần phân loại.**
- **`T1-03` (phiếu khám) chưa có template ở tenant 176**, dù là `REQUIRED` trên 4/8 `lossType`.

## Số 200 + `{}` là lỗi

Flow `8d873709` trả `{}` 0/4 kèm HTTP 200. Không bao giờ đọc nó là "không có cảnh báo".
