# SRD — External Logic `IdpTables`

**28/09/2026** · phạm vi: đọc `tables` trong response IDP và trả về hình dạng phẳng mà ODC đọc được
**Vì sao tồn tại:** `tables[].data` là **mảng của mảng chuỗi**. Structure của ODC biểu diễn được `Text List` và `List` của Structure, nhưng **không** biểu diễn được `List` của `List`; JSON ở đây lại là mảng thô chứ không phải mảng object, nên `JSONDeserialize` không có gì để ghép. Đây là giới hạn nền tảng, cùng loại với `filename=` đã buộc phải làm `IdpUpload`.

---

## 1. Ranh giới — giữ nguyên bất biến của `IdpUpload`

`IdpUpload` là **một cái ống**: không parse. Bất biến đó không được nới. Nên đây là **library thứ hai**, khai báo rõ nó là bộ đọc bảng, và nó cũng có bất biến riêng:

> **`IdpTables` chỉ chuyển đổi hình dạng. Nó không phán đoán.** Không lọc dòng, không nhận dạng dòng tổng cộng, không ép kiểu tiền, không tra `IdpFieldMap`, không biết `ClaimLine` là gì. Mọi quyết định ở lại ODC, nơi `ReplayMode` xem lại được.

Lý do cụ thể, không phải nguyên tắc suông: dòng dữ liệu duy nhất trong capture thật là `["---","---","0","0",…]`. Nhìn thì rõ là dòng giữ chỗ. Nhưng "bỏ dòng có `---`" là một **quy tắc nghiệp vụ**, và nếu nó nằm trong C# thì nó vô hình với Service Center và không chạy lại được. Trả cả dòng đó về, để ODC bỏ.

## 2. Nền tảng — theo đúng `IdpUpload`, đường đã chứng minh

| Mục | Giá trị |
|---|---|
| Target | **`net10.0`** (không phải .NET 8 — .NET 8 hết hỗ trợ trên ODC 10/11/2026) |
| SDK NuGet | `OutSystems.ExternalLibraries.SDK` **1.7.0** |
| Phụ thuộc thêm | `Microsoft.Extensions.Logging.Abstractions` — cho `ILogger` |
| JSON | `System.Text.Json`, dùng **`JsonDocument`** (duyệt cây), **không** deserialize có kiểu |
| Cấm | `async` trên method `[OSInterface]`; ném exception ra khỏi `[OSAction]`; ghi file; gọi mạng |
| Deploy | `dotnet publish` → zip nội dung (không `.pdb`) → upload qua **ODC Portal** |

Dùng `JsonDocument` chứ không deserialize có kiểu là cả điểm mạnh của việc làm bằng C#: nó đọc được hình dạng mà structure của ODC không tả nổi.

## 3. Hợp đồng

```csharp
[OSInterface(Name = "IdpTables",
  Description = "Đọc tables trong response IDP và trả về dạng phẳng — ODC không deserialize được mảng lồng mảng")]
public interface IIdpTables
{
    [OSAction(Description = "Duyệt RawJson, trả về từng ô của mọi bảng. Không phán đoán, không lọc.")]
    IdpTableParseResult ParseTables(
        [OSParameter(Description = "Body nguyên văn của IDP")] string RawJson,
        [OSParameter(Description = "Trần số ô, <= 0 thì mặc định 20000")] int MaxCells);
}

[OSStructure] public struct IdpTableCell {
    public string TemplateId;     // "6093" — chuỗi, đúng như trên dây
    public int    TableIndex;     // 0-based, trong template đó
    public string TableName;      // "Bảng kê chi phí khám chữa bệnh"
    public int    RowIndex;       // 1-based, ĐÃ BỎ dòng tiêu đề
    public int    ColIndex;       // 0-based
    public string ColumnHeader;   // lấy từ dòng 0 của chính bảng đó
    public string Value;          // luôn là chuỗi, kể cả khi JSON là số
    public decimal ConfOcr;       // 0 nếu thiếu
    public decimal ConfDu;        // 0 nếu thiếu
}

[OSStructure] public struct IdpTableParseResult {
    public bool   Ok;
    public string ErrorMessage;   // rỗng khi Ok
    public int    TableCount;
    public int    CellCount;
    public IdpTableCell[] Cells;
}
```

**Vì sao phẳng tới mức từng ô:** ODC nhận `List` của Structure không vấn đề gì, nhưng bất cứ thứ gì lồng sâu hơn lại rơi vào đúng giới hạn đã sinh ra library này. Một danh sách ô kèm `RowIndex`/`ColIndex` là hình dạng **chắc chắn** biểu diễn được, và ODC gom lại thành dòng bằng một `ForEach`.

**Vì sao `ColumnHeader` nằm trên từng ô** dù lặp: nó tránh cho ODC phải tự ghép dòng 0 với các dòng sau — đúng loại ghép nối dễ lệch một chỉ số. Lặp vài chục chuỗi rẻ hơn một lỗi lệch cột trong bảng tiền.

**Vì sao `Value` luôn `string`:** capture hiện tại toàn chuỗi, nhưng `"0"` và `0` đều có thể xuất hiện. Ép về chuỗi ở đây, để ODC ép kiểu tiền theo quy tắc của nó.

## 4. Thuật toán

1. `JsonDocument.Parse(RawJson)`. Hỏng → `Ok = false`, `ErrorMessage = "PARSE_FAILED: <lý do>"`.
2. **Tìm `templates_data` ở bất kỳ đâu trong cây** bằng duyệt đệ quy. Không đi theo đường cố định. Lý do: cùng một payload xuất hiện ở `result.result` (`/request/sync`), `result` (`/request/{id}/result`) và `data.result` (`check-status`) — hôm nay đã mất hai vòng vì một cờ boolean chọn sai đường. Tìm thì không sai được.
3. Với mỗi phần tử: `templateId`, rồi `data.tables` (thiếu → bỏ qua, không lỗi).
4. Với mỗi bảng: `name`, `data`, `confidence_ocr`, `confidence_du`.
   - `data` rỗng hoặc không phải mảng → bỏ qua bảng đó, **không** lỗi.
   - Dòng 0 là **tiêu đề**. Không phát ra ô nào cho nó.
   - Dòng 1..n: mỗi phần tử thành một `IdpTableCell`.
   - **Dòng lệch số cột** (ragged): phát đúng số ô thực có. Không đệm, không cắt, không ném.
   - `ColumnHeader`: phần tử thứ `ColIndex` của dòng 0; vượt quá → chuỗi rỗng.
   - `ConfOcr`/`ConfDu`: `confidence_ocr[row][col]` / `confidence_du[row][col]`; thiếu hoặc lệch kích thước → `0`.
5. Vượt `MaxCells` → dừng, `Ok = false`, `ErrorMessage = "TOO_MANY_CELLS: <n> > <max>"`, `Cells` trả về phần đã đọc.
6. Không bảng nào → `Ok = true`, `TableCount = 0`, `Cells` rỗng. **Đây là trường hợp bình thường**, không phải lỗi: chứng từ 6093 không có bảng.
7. Không bao giờ ném ra ngoài `[OSAction]`. Log đúng một dòng: `IdpTables.ParseTables tables=<n> cells=<n> elapsedMs=<n>`. **Không bao giờ log `RawJson`** — nó chứa dữ liệu bệnh án.

## 5. Nghiệm thu — chạy offline, 0 quota

Ba file trên đĩa, không cần gọi API:

| Fixture | `Ok` | `TableCount` | `CellCount` | Kiểm thêm |
|---|---|---|---|---|
| `IDP_API_Spec/fixtures/6093-sync-201-21408.json` | `true` | **0** | **0** | không có bảng là bình thường |
| `request-response-json/04-…/POST__request_sync.json` | `true` | **1** | **13** | `TemplateId = "5127"` · `TableName = "Bảng kê chi phí khám chữa bệnh"` · `RowIndex` = 1 ở cả 13 ô · `ColIndex` 0→12 · `ConfOcr = 0.95` ở cả 13 |
| `request-response-json/04-…/GET__request_21383_result.json` | `true` | **1** | **13** | y hệt trên — **chứng minh bước "tìm đệ quy" xử được cả hai hình dạng lồng**, vì file này lồng một lớp còn file kia lồng hai |
| chuỗi rỗng | `false` | 0 | 0 | `PARSE_FAILED` |
| `{"a":1}` | `true` | 0 | 0 | JSON hợp lệ, không có `templates_data` — **không** phải lỗi |

`ColumnHeader` của 13 ô, theo thứ tự, phải đúng bằng:

```
Nội dung · Đơn vị tính · Số lượng · Đơn giá BV (đồng) · Đơn giá BH (đồng) ·
Tỷ lệ thanh toán theo DV · Thành tiền BV (đồng) · Tỷ lệ thanh toán BHYT ·
Thành tiền BH (đồng) · Quỹ BHYT (đồng) · Người bệnh cùng chi trả (đồng) ·
Khác (đồng) · Người bệnh tự trả (đồng)
```

## 6. Phía ODC sau khi có library

Thêm nút thứ ba vào harness: **"Run IdpTables.ParseTables"** trên cùng ô RawJson, hiện `Ok`, `TableCount`, `CellCount` và bảng ô. 0 quota, và nó là chỗ chạy toàn bộ bảng nghiệm thu §5.

Ánh xạ sang `ClaimLine` là việc của ODC, không của library: gom `Cells` theo `RowIndex`, tra `ColumnHeader` trong `IdpFieldMap` để ra `LINE_*`, rồi ép kiểu. **Tra cứu phải `NFC`-normalize hai vế** — cùng luật đã đặt cho `k`.

## 7. Hai chỗ chưa đóng, phải quyết trước khi map `ClaimLine`

**a. Nhãn cột trong seed đã lệch, đã sửa.** Trước hôm nay `IdpFieldMap-seed.csv` ghi nhãn rút gọn — `Đơn giá BV`, `Tỷ lệ thanh toán theo dịch vụ` — trong khi trên dây là `Đơn giá BV (đồng)`, `Tỷ lệ thanh toán theo DV`. **Chỉ 4/13 khớp.** Chín cột trượt, và cả chín đều là cột tiền. Đã sửa theo header thật, giờ **13/13**, mỗi dòng có ghi chú nhãn cũ. Nhưng nó nhắc một điều: nhãn trong seed là *đọc từ tài liệu*, nhãn trên dây là *sự thật*. Mọi nhãn chưa đối chiếu với capture đều đáng ngờ.

**b. Bảng này thuộc template nào?** Capture trả bảng dưới `templateId 5127`, mà trong seed `5127` là **`6092` — "Giấy ra viện (5127)"**, không phải `6091` — "Bảng kê chi phí khám chữa bệnh", nơi 13 dòng `LINE_*` đang nằm. Tức cùng một bảng chi phí có thể xuất hiện **bên trong** nhiều template khác nhau.

Nếu đúng vậy thì khoá `(IdpTemplateId, K)` là khoá **sai** cho cột bảng: cột bảng nên tra theo `ColumnHeader` trước, `TemplateId` chỉ dùng để phân giải khi trùng. `IdpTableCell` đã mang sẵn `TemplateId` nên ODC quyết được cả hai đường — nhưng phải chọn một, và chọn trước khi viết phần map.

## 8. Điều phải nói thẳng về dữ liệu

Cả hai capture có bảng đều chỉ có **một dòng dữ liệu, và nó là `["---","---","0","0",…]`** — dòng giữ chỗ. Nghĩa là ta có **hình dạng** của bảng nhưng **chưa có một dòng chi phí thật nào**.

Library này vì thế kiểm được đúng phần chuyển đổi hình dạng, và không kiểm được phần ép kiểu tiền, dấu phân cách hàng nghìn, hay dòng tổng cộng. Đó là rủi ro còn lại, và nó chỉ đóng được bằng một chứng từ bảng kê thật. Đừng để nó bị phát hiện trên sân khấu.
