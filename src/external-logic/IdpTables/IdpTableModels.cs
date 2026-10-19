using OutSystems.ExternalLibraries.SDK;

namespace BnK.IdpTables;

[OSStructure(Description = "Một ô của một bảng trong response IDP")]
public struct IdpTableCell
{
    [OSStructureField(Description = "templateId đúng như trên dây, dạng chuỗi, vd \"6093\"")]
    public string TemplateId { get; set; }

    [OSStructureField(Description = "Thứ tự bảng trong template, bắt đầu từ 0")]
    public int TableIndex { get; set; }

    [OSStructureField(Description = "name của bảng")]
    public string TableName { get; set; }

    [OSStructureField(Description = "Thứ tự dòng, bắt đầu từ 1 — dòng tiêu đề (dòng 0) không phát ra ô")]
    public int RowIndex { get; set; }

    [OSStructureField(Description = "Thứ tự cột, bắt đầu từ 0")]
    public int ColIndex { get; set; }

    [OSStructureField(Description = "Nhãn cột lấy từ dòng 0 của chính bảng đó; rỗng nếu dòng 0 ngắn hơn")]
    public string ColumnHeader { get; set; }

    [OSStructureField(Description = "Giá trị ô, luôn là chuỗi kể cả khi JSON là số")]
    public string Value { get; set; }

    [OSStructureField(Description = "confidence_ocr[row][col]; 0 nếu thiếu")]
    public decimal ConfOcr { get; set; }

    [OSStructureField(Description = "confidence_du[row][col]; 0 nếu thiếu")]
    public decimal ConfDu { get; set; }
}

[OSStructure(Description = "Kết quả đọc bảng")]
public struct IdpTableParseResult
{
    [OSStructureField(Description = "false khi JSON hỏng hoặc vượt trần số ô")]
    public bool Ok { get; set; }

    [OSStructureField(Description = "Rỗng khi Ok; PARSE_FAILED: … hoặc TOO_MANY_CELLS: …")]
    public string ErrorMessage { get; set; }

    [OSStructureField(Description = "Số bảng có ít nhất một dòng")]
    public int TableCount { get; set; }

    [OSStructureField(Description = "Số ô đã phát ra")]
    public int CellCount { get; set; }

    [OSStructureField(Description = "Mọi ô của mọi bảng, theo thứ tự template → bảng → dòng → cột")]
    public IEnumerable<IdpTableCell> Cells { get; set; }
}
