using BnK.IdpUpload;

namespace BnK.IdpUpload.Tests;

/// <summary>
/// SRD §12.1 / BRD A4, A5 — chặn trước khi chạm mạng. Mock server đếm lời gọi để chứng minh "0 quota".
/// </summary>
public class ValidationTests : IDisposable
{
    private readonly MockIdpServer _server = new();
    private readonly CapturingLogger _log = new();
    private readonly IIdpUpload _sut;

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47];

    public ValidationTests() => _sut = new IdpUpload(_log);

    public void Dispose() => _server.Dispose();

    private IdpUploadResult Sync(byte[]? doc = null, string fileName = "a.png", string contentType = "",
        string name = "CLM-1", string templates = "6093", string token = "tok") =>
        _sut.SubmitSync(_server.BaseUrl, token, "UA", name, templates, doc ?? Png, fileName, contentType, 5);

    private void AssertBlocked(IdpUploadResult r, string expectedError)
    {
        Assert.False(r.TransportOk);
        Assert.Equal(0, r.HttpStatus);
        Assert.Equal(expectedError, r.ErrorMessage);
        Assert.Equal("", r.RawJson);
        Assert.Empty(_server.Requests);
    }

    [Fact]
    public void Xlsx_IsUnsupported_NoHttpCall() =>
        AssertBlocked(Sync(fileName: "a.xlsx"), "UNSUPPORTED_FILE_TYPE: .xlsx");

    [Fact]
    public void NoExtension_IsUnsupported_NoHttpCall() =>
        AssertBlocked(Sync(fileName: "scan"), "UNSUPPORTED_FILE_TYPE: ");

    [Fact]
    public void OctetStreamIsNeverInferred_ExplicitContentTypeWins()
    {
        // ContentType truyền vào thì dùng nguyên, kể cả khi đuôi file lạ
        var r = Sync(fileName: "a.bin", contentType: "image/png");
        Assert.True(r.TransportOk);
        Assert.Contains("Content-Type: image/png", Assert.Single(_server.Requests).BodyText);
    }

    [Fact]
    public void SixMegabytes_IsTooLarge_NoHttpCall() =>
        AssertBlocked(Sync(doc: new byte[6_291_456]), "PAYLOAD_TOO_LARGE: 6291456 bytes > 5242880");

    [Fact]
    public void ExactlyFiveMiB_IsAllowed()
    {
        var r = Sync(doc: new byte[5_242_880]);
        Assert.True(r.TransportOk);
        Assert.Single(_server.Requests);
    }

    [Fact]
    public void EmptyDocument_IsMissingInput() => AssertBlocked(Sync(doc: []), "MISSING_INPUT: Document");

    [Fact]
    public void NullDocument_IsMissingInput() =>
        AssertBlocked(_sut.SubmitSync(_server.BaseUrl, "tok", "UA", "n", "1", null!, "a.png", "", 5),
            "MISSING_INPUT: Document");

    [Fact]
    public void EmptyName_IsMissingInput() => AssertBlocked(Sync(name: ""), "MISSING_INPUT: Name");

    [Fact]
    public void EmptyTemplates_IsMissingInput() => AssertBlocked(Sync(templates: " "), "MISSING_INPUT: TemplatesCsv");

    [Fact]
    public void EmptyToken_IsMissingInput() => AssertBlocked(Sync(token: ""), "MISSING_INPUT: AccessToken");

    [Fact]
    public void EmptyBaseUrl_IsMissingInput() =>
        AssertBlocked(_sut.SubmitSync("", "tok", "UA", "n", "1", Png, "a.png", "", 5), "MISSING_INPUT: BaseUrl");

    [Fact]
    public void Submit_AppliesSameValidation() =>
        AssertBlocked(_sut.Submit(_server.BaseUrl, "tok", "UA", "n", "1", Png, "a.xlsx", "", "", 5),
            "UNSUPPORTED_FILE_TYPE: .xlsx");

    [Fact]
    public void BlockedCall_LogsExactlyOneLine_WithoutToken()
    {
        Sync(fileName: "a.xlsx", token: "SECRET-TOKEN-123");
        var line = Assert.Single(_log.Lines);
        Assert.StartsWith("IdpUpload.SubmitSync name=CLM-1", line);
        Assert.DoesNotContain("SECRET-TOKEN-123", line);
    }
}
