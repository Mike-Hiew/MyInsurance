using BnK.IdpUpload;

namespace BnK.IdpUpload.Tests;

/// <summary>
/// SRD §12.3 — chạm mạng thật nhưng vào mock server local, 0 quota tenant 177.
/// Khẳng định đúng 3 header, đúng 3 part, và hợp đồng lỗi §10.
/// </summary>
public class MockIdpTests : IDisposable
{
    private const string Ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36";
    private const string Token = "eyJhbGciOiJIUzI1NiJ9.SECRET.SIG";
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0xFF];

    private readonly MockIdpServer _server = new();
    private readonly CapturingLogger _log = new();
    private readonly IIdpUpload _sut;

    public MockIdpTests() => _sut = new IdpUpload(_log);
    public void Dispose() => _server.Dispose();

    private IdpUploadResult Sync(string userAgent = Ua, int timeout = 10) =>
        _sut.SubmitSync(_server.BaseUrl, Token, userAgent, "CLM-2026-0001",
            "6090,6091,6092,6093,6094,6095,6096,6097,6098", Png, "6093_vnvision-giay-ra-vien.png", "", timeout);

    [Fact]
    public void SubmitSync_PostsToRequestSync_WithThreeHeaders()
    {
        var r = Sync();
        var req = Assert.Single(_server.Requests);

        Assert.Equal("POST /request/sync HTTP/1.1", req.RequestLine);
        Assert.Equal("Bearer " + Token, req.Headers["Authorization"]);
        Assert.Equal(Ua, req.Headers["User-Agent"]);
        Assert.Equal("application/json, text/plain, */*", req.Headers["Accept"]);
        Assert.StartsWith("multipart/form-data; boundary=", req.Headers["Content-Type"]);

        Assert.True(r.TransportOk);
        Assert.Equal(201, r.HttpStatus);
        Assert.Equal("{\"id\":1}", r.RawJson);
        Assert.Equal("", r.ErrorMessage);
    }

    [Fact]
    public void SubmitSync_BodyHasThreePartsAndFilename_OnTheWire()
    {
        Sync();
        var body = Assert.Single(_server.Requests).BodyText;

        Assert.Contains("name=\"document\"; filename=\"6093_vnvision-giay-ra-vien.png\"", body);
        Assert.Contains("Content-Type: image/png", body);
        Assert.Contains("name=\"name\"\r\n\r\nCLM-2026-0001\r\n", body);
        Assert.Contains("name=\"templates\"\r\n\r\n6090,6091,6092,6093,6094,6095,6096,6097,6098\r\n", body);
        Assert.DoesNotContain("requestGroupId", body);
        // bytes nhị phân đi nguyên vẹn
        Assert.Contains(System.Text.Encoding.Latin1.GetString(Png), body);
    }

    [Fact]
    public void BaseUrlTrailingSlash_IsNormalised()
    {
        _sut.SubmitSync(_server.BaseUrl + "/", Token, Ua, "n", "1", Png, "a.png", "", 10);
        Assert.Equal("POST /request/sync HTTP/1.1", Assert.Single(_server.Requests).RequestLine);
    }

    [Fact]
    public void Submit_PostsToRequest_WithRequestGroupId()
    {
        _server.ResponseBody = "{\"requestId\":99,\"pageCount\":1}";
        var r = _sut.Submit(_server.BaseUrl, Token, Ua, "CLM-1", "6093", Png, "a.png", "", "7", 10);
        var req = Assert.Single(_server.Requests);

        Assert.Equal("POST /request HTTP/1.1", req.RequestLine);
        Assert.Contains("name=\"requestGroupId\"\r\n\r\n7\r\n", req.BodyText);
        Assert.Equal("{\"requestId\":99,\"pageCount\":1}", r.RawJson);
    }

    [Fact]
    public void MissingUserAgent_403IsReturnedVerbatim()   // A6
    {
        var r = Sync(userAgent: "");
        Assert.False(Assert.Single(_server.Requests).Headers.ContainsKey("User-Agent"));
        Assert.True(r.TransportOk);
        Assert.Equal(403, r.HttpStatus);
        Assert.Equal("error code: 1010", r.RawJson);
        Assert.Equal("", r.ErrorMessage);
    }

    [Theory]
    [InlineData(500, "{\"statusCode\":500,\"message\":\"Cannot read properties of undefined (reading 'path')\"}")]
    [InlineData(400, "{\"statusCode\":400,\"message\":\"invalid input syntax for type bigint: \\\"NaN\\\"\"}")]
    [InlineData(200, "{\"errorCode\":1014,\"message\":\"Partner is expired\"}")]
    public void ApiErrors_AreTransportOk_WithRawBodyUntouched(int status, string body)
    {
        _server.ResponseStatus = status;
        _server.ResponseBody = body;
        var r = Sync();
        Assert.True(r.TransportOk);
        Assert.Equal(status, r.HttpStatus);
        Assert.Equal(body, r.RawJson);
        Assert.Equal("", r.ErrorMessage);
    }

    [Fact]
    public void Timeout_IsTransportFailure()
    {
        _server.ResponseDelay = TimeSpan.FromSeconds(5);
        var r = Sync(timeout: 1);
        Assert.False(r.TransportOk);
        Assert.Equal(0, r.HttpStatus);
        Assert.Equal("TIMEOUT: 1s", r.ErrorMessage);
        Assert.InRange(r.ElapsedMs, 900, 4000);
    }

    [Fact]
    public void ConnectionRefused_IsTransportFailure()
    {
        var deadUrl = _server.BaseUrl;
        _server.Dispose();
        var r = _sut.SubmitSync(deadUrl, Token, Ua, "n", "1", Png, "a.png", "", 5);
        Assert.False(r.TransportOk);
        Assert.Equal(0, r.HttpStatus);
        Assert.StartsWith("TRANSPORT: HttpRequestException: ", r.ErrorMessage);
        Assert.DoesNotContain(Token, r.ErrorMessage);
    }

    [Fact]
    public void Log_IsOneLine_NoTokenNoBodyNoDocument()   // A7 ở mức library
    {
        _server.ResponseBody = "{\"templates_data\":[{\"patientName\":\"NGUYEN VAN PII\"}]}";
        Sync();
        var line = Assert.Single(_log.Lines);
        Assert.Matches(
            @"^IdpUpload\.SubmitSync name=CLM-2026-0001 bytes=10 contentType=image/png status=201 elapsedMs=\d+$", line);
        Assert.DoesNotContain("SECRET", line);
        Assert.DoesNotContain("PII", line);
    }

    [Fact]
    public void DefaultTimeout_AppliesWhenZero()
    {
        var r = Sync(timeout: 0);
        Assert.True(r.TransportOk);
    }
}
