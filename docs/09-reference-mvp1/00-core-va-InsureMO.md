# Core bảo hiểm và InsureMO — trạng thái thật và cách xử lý trong hackathon

## Sự thật cần nói thẳng

Tính đến 03/09/2026, **chưa có payload InsureMO nào được ghi lại**, không có API spec, không có sandbox. Thư mục `insurance BNK/Bnk IDP/InsureMO/` chỉ có hai deck marketing:

- `InsureMO_Channels_Deck_Bancassurance_2025.pptx`
- `InsureMO_Insurer_Deck_2025.pptx`

Tương tự với Bicins và DataPlatform MBAL — chưa có đường kết nối thật.

## Cách xử lý trong hackathon

Dữ liệu hợp đồng và cây quyền lợi được **seed tay vào Entity** theo **bản chụp P-02** của MVP1, và core nằm sau một **adapter**.

```
Agent  →  Server Action (CLM_Core)  →  IPolicyProvider (adapter)  →  [ MockPolicyProvider ]  hôm nay
                                                                  →  [ InsureMOProvider   ]  mai
```

Nguyên tắc: **lớp agent và lớp tính toán không biết dữ liệu đến từ đâu.** Đổi core chỉ đổi một implementation của adapter.

## Đây là điểm mạnh khi pitch, không phải điểm yếu

Câu trả lời chuẩn khi ban giám khảo hỏi "dữ liệu hợp đồng lấy ở đâu":

> "Hợp đồng và cây quyền lợi được seed vào Entity theo bản chụp P-02. Core — InsureMO, Bicins hay DataPlatform — nằm sau một adapter: hôm nay mock, mai cắm thật, lớp agent không đổi. Chúng tôi cố ý không khoá giải pháp vào một core nào."

Đừng giấu chuyện mock. Nói trước, kèm lý do kiến trúc, thì nó thành lựa chọn có chủ đích thay vì lỗ hổng bị phát hiện.

## Cần gì để rời khỏi mock (sau hackathon)

1. Xác nhận core thật của khách hàng mục tiêu (MBAL hay BIC).
2. Lấy spec hoặc payload mẫu của endpoint tra cứu hợp đồng và quyền lợi.
3. Viết `InsureMOProvider` implement đúng interface đã có.
4. Giữ nguyên `PolicyBenefitSnapshot` — vẫn phải chụp lại tại thời điểm nhận hồ sơ, kể cả khi có core thật, vì quy tắc "mọi giá trị lấy từ bản chụp".

## Dữ liệu cần trích ra từ P-02 (việc còn treo)

Cho mỗi gói HC-STD / HC-PLS / HC-PRM và mỗi mã quyền lợi (`INPATIENT_ROOM`, `INPATIENT_SURGERY`, `INPATIENT_OTHER`, `OUTPATIENT`, `DENTAL`):

| Trường | Ghi chú |
|---|---|
| `SumInsured` | Số tiền bảo hiểm |
| `PerVisitCap` | Trần mỗi lần |
| `CoinsurancePct` | Tỷ lệ đồng chi trả |
| `WaitingPeriodDays` | Thời gian chờ — quyết định kịch bản D |
| `AnnualLimit` | Hạn mức năm |

Cho đến khi có số thật, `BootstrapDemoData` để placeholder kèm comment `// TODO: replace with P-02 snapshot values`.
