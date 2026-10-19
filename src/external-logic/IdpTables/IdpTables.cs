using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BnK.IdpTables;

public class IdpTables : IIdpTables
{
    private readonly ILogger _logger;

    public IdpTables(ILogger logger)
    {
        _logger = logger;
    }

    public IdpTableParseResult ParseTables(string RawJson, int MaxCells)
    {
        var sw = Stopwatch.StartNew();
        IdpTableParseResult result;
        try
        {
            result = TableReader.Parse(RawJson, MaxCells);
        }
        catch (Exception ex)
        {
            // Không bao giờ ném exception ra khỏi [OSAction]
            result = new IdpTableParseResult
            {
                Ok = false, ErrorMessage = "PARSE_FAILED: " + ex.GetType().Name,
                TableCount = 0, CellCount = 0, Cells = new List<IdpTableCell>(),
            };
        }

        // Đúng một dòng. Không bao giờ log RawJson — nó chứa dữ liệu bệnh án.
        _logger.LogInformation("IdpTables.ParseTables tables={Tables} cells={Cells} elapsedMs={ElapsedMs}{Error}",
            result.TableCount, result.CellCount, sw.ElapsedMilliseconds,
            result.Ok ? "" : " error=" + result.ErrorMessage.Split(':')[0]);
        return result;
    }
}
