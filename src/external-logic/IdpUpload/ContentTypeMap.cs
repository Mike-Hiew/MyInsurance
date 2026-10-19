namespace BnK.IdpUpload;

/// <summary>
/// Bảng đuôi file → mime. DUY NHẤT một bản trong library.
/// Không map được thì trả null — cấm lùi về application/octet-stream, đó chính là thứ server IDP từ chối.
/// </summary>
public static class ContentTypeMap
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"]  = "application/pdf",
        [".png"]  = "image/png",
        [".jpg"]  = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".tif"]  = "image/tiff",
        [".tiff"] = "image/tiff",
    };

    public static string? Resolve(string? fileName) =>
        Map.TryGetValue(Path.GetExtension(fileName ?? ""), out var v) ? v : null;
}
