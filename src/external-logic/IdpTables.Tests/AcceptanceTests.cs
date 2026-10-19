using BnK.IdpTables;

namespace BnK.IdpTables.Tests;

/// <summary>Bảng nghiệm thu SRD IdpTables §5 — offline, 0 quota.</summary>
public class AcceptanceTests
{
    private static string Fixture(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));

    private static readonly string[] ExpectedHeaders =
    {
        "Nội dung", "Đơn vị tính", "Số lượng", "Đơn giá BV (đồng)", "Đơn giá BH (đồng)",
        "Tỷ lệ thanh toán theo DV", "Thành tiền BV (đồng)", "Tỷ lệ thanh toán BHYT",
        "Thành tiền BH (đồng)", "Quỹ BHYT (đồng)", "Người bệnh cùng chi trả (đồng)",
        "Khác (đồng)", "Người bệnh tự trả (đồng)",
    };

    [Fact]
    public void Template6093_NoTables_IsNormal()
    {
        var r = TableReader.Parse(Fixture("6093-sync-201-21408.json"), 0);
        Assert.True(r.Ok);
        Assert.Equal(0, r.TableCount);
        Assert.Equal(0, r.CellCount);
        Assert.Empty(r.Cells);
    }

    [Theory]
    [InlineData("POST__request_sync.json")]        // lồng hai lớp: result.result.templates_data
    [InlineData("GET__request_21383_result.json")] // lồng một lớp: result.templates_data
    public void BangKe_13Cells_BothNestings(string file)
    {
        var r = TableReader.Parse(Fixture(file), 0);
        Assert.True(r.Ok, r.ErrorMessage);
        Assert.Equal(1, r.TableCount);
        Assert.Equal(13, r.CellCount);
        Assert.All(r.Cells, c =>
        {
            Assert.Equal("5127", c.TemplateId);
            Assert.Equal("Bảng kê chi phí khám chữa bệnh", c.TableName);
            Assert.Equal(0, c.TableIndex);
            Assert.Equal(1, c.RowIndex);
            Assert.Equal(0.95m, c.ConfOcr);
            Assert.Equal(0m, c.ConfDu);
        });
        Assert.Equal(Enumerable.Range(0, 13), r.Cells.Select(c => c.ColIndex));
        Assert.Equal(ExpectedHeaders, r.Cells.Select(c => c.ColumnHeader));
        Assert.Equal("---", r.Cells.First().Value); // dòng giữ chỗ trả về nguyên — ODC mới là nơi bỏ
    }

    [Fact]
    public void EmptyString_ParseFailed()
    {
        var r = TableReader.Parse("", 0);
        Assert.False(r.Ok);
        Assert.StartsWith("PARSE_FAILED", r.ErrorMessage);
        Assert.Equal(0, r.CellCount);
    }

    [Fact]
    public void ValidJsonWithoutTemplates_IsNotAnError()
    {
        var r = TableReader.Parse("{\"a\":1}", 0);
        Assert.True(r.Ok);
        Assert.Equal(0, r.TableCount);
        Assert.Equal(0, r.CellCount);
    }

    [Fact]
    public void RaggedRows_NumbersAsText_MissingConfidence()
    {
        const string json = """
        {"templates_data":[{"templateId":6091,"data":{"tables":[
          {"name":"T","data":[["A","B"],["x",1500000,"extra"],["y"]]}
        ]}}]}
        """;
        var r = TableReader.Parse(json, 0);
        Assert.True(r.Ok);
        Assert.Equal(4, r.CellCount);
        var cells = r.Cells.ToList();
        Assert.Equal("6091", cells[0].TemplateId);
        Assert.Equal("1500000", cells[1].Value);
        Assert.Equal("", cells[2].ColumnHeader);   // cột vượt dòng tiêu đề
        Assert.Equal(2, cells[3].RowIndex);
        Assert.All(r.Cells, c => Assert.Equal(0m, c.ConfOcr));
    }

    [Fact]
    public void MaxCells_StopsWithPartialResult()
    {
        var r = TableReader.Parse(Fixture("POST__request_sync.json"), 5);
        Assert.False(r.Ok);
        Assert.StartsWith("TOO_MANY_CELLS", r.ErrorMessage);
        Assert.Equal(5, r.CellCount);
    }

    [Fact]
    public void EmptyOrMissingTables_AreSkipped()
    {
        const string json = """
        {"templates_data":[{"templateId":"1","data":{}},{"templateId":"2","data":{"tables":[{"name":"E","data":[]}]}}]}
        """;
        var r = TableReader.Parse(json, 0);
        Assert.True(r.Ok);
        Assert.Equal(0, r.TableCount);
    }
}
