using OutSystems.ExternalLibraries.SDK;

namespace BnK.IdpUpload;

/// <summary>
/// TransportOk tách khỏi HttpStatus là có chủ đích:
/// true + 500 = API đã trả lời và từ chối (việc của ODC) · false = không nối được (việc của hạ tầng).
/// </summary>
[OSStructure(Description = "Kết quả thô của một lần nộp")]
public struct IdpUploadResult
{
    [OSStructureField(Description = "true = có phản hồi HTTP, kể cả 4xx/5xx")]
    public bool TransportOk { get; set; }

    [OSStructureField(Description = "Mã HTTP thật; 0 khi TransportOk = false")]
    public int HttpStatus { get; set; }

    [OSStructureField(Description = "Body nguyên văn, không sửa")]
    public string RawJson { get; set; }

    [OSStructureField(Description = "Chỉ điền khi TransportOk = false")]
    public string ErrorMessage { get; set; }

    [OSStructureField(Description = "Thời gian gọi HTTP, mili giây")]
    public int ElapsedMs { get; set; }

    internal static IdpUploadResult Failed(string errorMessage, int elapsedMs = 0) => new()
    {
        TransportOk = false, HttpStatus = 0, RawJson = "", ErrorMessage = errorMessage, ElapsedMs = elapsedMs,
    };

    internal static IdpUploadResult Answered(int httpStatus, string rawJson, int elapsedMs) => new()
    {
        TransportOk = true, HttpStatus = httpStatus, RawJson = rawJson, ErrorMessage = "", ElapsedMs = elapsedMs,
    };
}
