using System.Text.Json;

namespace BnK.IdpTables;

/// <summary>
/// Chỉ chuyển đổi hình dạng. Không lọc dòng, không nhận dạng dòng tổng, không ép kiểu tiền,
/// không tra IdpFieldMap. Mọi quyết định ở lại ODC (SRD §1).
/// </summary>
internal static class TableReader
{
    internal const int DefaultMaxCells = 20000;

    internal static IdpTableParseResult Parse(string? rawJson, int maxCells)
    {
        var max = maxCells <= 0 ? DefaultMaxCells : maxCells;
        var cells = new List<IdpTableCell>();
        var tableCount = 0;

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(rawJson ?? "");
        }
        catch (Exception ex)
        {
            return Result(false, "PARSE_FAILED: " + ex.GetType().Name, 0, cells);
        }

        using (doc)
        {
            foreach (var templates in FindTemplateArrays(doc.RootElement))
            {
                foreach (var template in templates.EnumerateArray())
                {
                    if (template.ValueKind != JsonValueKind.Object) continue;
                    var templateId = template.TryGetProperty("templateId", out var tid) ? AsString(tid) : "";
                    if (!template.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object) continue;
                    if (!data.TryGetProperty("tables", out var tables) || tables.ValueKind != JsonValueKind.Array) continue;

                    var tableIndex = 0;
                    foreach (var table in tables.EnumerateArray())
                    {
                        if (table.ValueKind != JsonValueKind.Object) continue;
                        if (!table.TryGetProperty("data", out var rows) || rows.ValueKind != JsonValueKind.Array
                            || rows.GetArrayLength() == 0) continue;

                        var thisIndex = tableIndex++;
                        tableCount++;
                        var name = table.TryGetProperty("name", out var n) ? AsString(n) : "";
                        var header = rows[0];
                        table.TryGetProperty("confidence_ocr", out var ocr);
                        table.TryGetProperty("confidence_du", out var du);

                        var rowIndex = -1;
                        foreach (var row in rows.EnumerateArray())
                        {
                            rowIndex++;
                            if (rowIndex == 0 || row.ValueKind != JsonValueKind.Array) continue;
                            var colIndex = -1;
                            foreach (var value in row.EnumerateArray())
                            {
                                colIndex++;
                                if (cells.Count >= max)
                                {
                                    return Result(false, $"TOO_MANY_CELLS: > {max}", tableCount, cells);
                                }

                                cells.Add(new IdpTableCell
                                {
                                    TemplateId = templateId,
                                    TableIndex = thisIndex,
                                    TableName = name,
                                    RowIndex = rowIndex,
                                    ColIndex = colIndex,
                                    ColumnHeader = Cell(header, colIndex),
                                    Value = AsString(value),
                                    ConfOcr = Number(ocr, rowIndex, colIndex),
                                    ConfDu = Number(du, rowIndex, colIndex),
                                });
                            }
                        }
                    }
                }
            }
        }

        return Result(true, "", tableCount, cells);
    }

    /// <summary>Tìm templates_data ở bất kỳ đâu — cùng payload nằm ở result.result, result hoặc data.result tuỳ endpoint.</summary>
    private static IEnumerable<JsonElement> FindTemplateArrays(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    if (prop.NameEquals("templates_data") && prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        yield return prop.Value;
                        continue;
                    }

                    foreach (var found in FindTemplateArrays(prop.Value)) yield return found;
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    foreach (var found in FindTemplateArrays(item)) yield return found;
                }
                break;
        }
    }

    private static string Cell(JsonElement row, int col)
    {
        if (row.ValueKind != JsonValueKind.Array || col >= row.GetArrayLength()) return "";
        return AsString(row[col]);
    }

    private static decimal Number(JsonElement matrix, int row, int col)
    {
        if (matrix.ValueKind != JsonValueKind.Array || row >= matrix.GetArrayLength()) return 0m;
        var r = matrix[row];
        if (r.ValueKind != JsonValueKind.Array || col >= r.GetArrayLength()) return 0m;
        var v = r[col];
        return v.ValueKind == JsonValueKind.Number && v.TryGetDecimal(out var d) ? d : 0m;
    }

    private static string AsString(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.String => e.GetString() ?? "",
        JsonValueKind.Null or JsonValueKind.Undefined => "",
        _ => e.GetRawText(),
    };

    private static IdpTableParseResult Result(bool ok, string error, int tables, List<IdpTableCell> cells) => new()
    {
        Ok = ok,
        ErrorMessage = error,
        TableCount = tables,
        CellCount = cells.Count,
        Cells = cells,
    };
}
