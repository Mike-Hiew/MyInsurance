using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BnK.IdpUpload;

/// <summary>
/// Một cái ống: dựng multipart, gắn header, gửi, trả body thô. Không parse, không token, không retry,
/// không ghi entity/file. Mọi phán đoán ở lại ODC.
/// </summary>
public class IdpUpload : IIdpUpload
{
    internal const int MaxDocumentBytes = 5_242_880;   // 5 MiB — chừa chỗ cho overhead multipart dưới trần 5,5 MB
    private const int DefaultTimeoutSeconds = 60;

    // Static, dùng lại — new mỗi lần gọi là cạn socket. Timeout của instance tắt hẳn vì nó là toàn cục;
    // timeout thật đặt theo từng request qua CancellationTokenSource.
    private static readonly HttpClient Http = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    })
    {
        Timeout = Timeout.InfiniteTimeSpan,
    };

    private readonly ILogger _logger;

    public IdpUpload(ILogger logger)
    {
        _logger = logger;
    }

    public IdpUploadResult SubmitSync(
        string BaseUrl, string AccessToken, string UserAgent, string Name, string TemplatesCsv,
        byte[] Document, string FileName, string ContentType, int TimeoutSeconds) =>
        Send(nameof(SubmitSync), "/request/sync", BaseUrl, AccessToken, UserAgent, Name, TemplatesCsv,
            Document, FileName, ContentType, null, TimeoutSeconds);

    public IdpUploadResult Submit(
        string BaseUrl, string AccessToken, string UserAgent, string Name, string TemplatesCsv,
        byte[] Document, string FileName, string ContentType, string RequestGroupId, int TimeoutSeconds) =>
        Send(nameof(Submit), "/request", BaseUrl, AccessToken, UserAgent, Name, TemplatesCsv,
            Document, FileName, ContentType, RequestGroupId, TimeoutSeconds);

    private IdpUploadResult Send(
        string operation, string path, string baseUrl, string accessToken, string userAgent, string name,
        string templatesCsv, byte[] document, string fileName, string contentType, string? requestGroupId,
        int timeoutSeconds)
    {
        var bytes = document?.Length ?? 0;
        string resolvedContentType = "";
        IdpUploadResult result;
        try
        {
            result = Validate(baseUrl, accessToken, name, templatesCsv, document, fileName, contentType,
                         out resolvedContentType)
                     ?? Post(path, baseUrl, accessToken, userAgent, name, templatesCsv, document!, fileName,
                         resolvedContentType, requestGroupId, timeoutSeconds <= 0 ? DefaultTimeoutSeconds : timeoutSeconds);
        }
        catch (Exception ex)
        {
            // Không bao giờ ném exception ra khỏi [OSAction]
            result = IdpUploadResult.Failed($"TRANSPORT: {ex.GetType().Name}: {ex.Message}");
        }

        // Đúng một dòng mỗi lời gọi. Tuyệt đối không ghi AccessToken, Document, RawJson (PII).
        var errorCode = result.TransportOk ? "" : " error=" + result.ErrorMessage.Split(':')[0];
        _logger.LogInformation(
            "IdpUpload.{Operation} name={Name} bytes={Bytes} contentType={ContentType} status={Status} elapsedMs={ElapsedMs}{Error}",
            operation, name, bytes, resolvedContentType, result.HttpStatus, result.ElapsedMs, errorCode);
        return result;
    }

    /// <summary>Chặn trước khi chạm mạng — mỗi lời gọi thật tốn một trang quota không hoàn lại.</summary>
    private static IdpUploadResult? Validate(
        string baseUrl, string accessToken, string name, string templatesCsv, byte[]? document,
        string fileName, string contentType, out string resolvedContentType)
    {
        resolvedContentType = "";
        if (document is null || document.Length == 0) return IdpUploadResult.Failed("MISSING_INPUT: Document");
        if (string.IsNullOrWhiteSpace(fileName)) return IdpUploadResult.Failed("MISSING_INPUT: FileName");
        if (string.IsNullOrWhiteSpace(name)) return IdpUploadResult.Failed("MISSING_INPUT: Name");
        if (string.IsNullOrWhiteSpace(templatesCsv)) return IdpUploadResult.Failed("MISSING_INPUT: TemplatesCsv");
        if (string.IsNullOrWhiteSpace(accessToken)) return IdpUploadResult.Failed("MISSING_INPUT: AccessToken");
        if (string.IsNullOrWhiteSpace(baseUrl)) return IdpUploadResult.Failed("MISSING_INPUT: BaseUrl");

        var ct = string.IsNullOrWhiteSpace(contentType) ? ContentTypeMap.Resolve(fileName) : contentType.Trim();
        if (ct is null) return IdpUploadResult.Failed($"UNSUPPORTED_FILE_TYPE: {Path.GetExtension(fileName)}");

        if (document.Length > MaxDocumentBytes)
            return IdpUploadResult.Failed($"PAYLOAD_TOO_LARGE: {document.Length} bytes > {MaxDocumentBytes}");

        resolvedContentType = ct;
        return null;
    }

    private static IdpUploadResult Post(
        string path, string baseUrl, string accessToken, string userAgent, string name, string templatesCsv,
        byte[] document, string fileName, string contentType, string? requestGroupId, int timeoutSeconds)
    {
        using var form = MultipartBuilder.Build(name, templatesCsv, document, fileName, contentType, requestGroupId);
        using var request = new HttpRequestMessage(HttpMethod.Post, baseUrl.TrimEnd('/') + path) { Content = form };

        request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);
        // User-Agent không phải tuỳ chọn: thiếu nó Cloudflare trả 403 "error code: 1010".
        // TryAddWithoutValidation vì chuỗi giả trình duyệt không qua validator mặc định của .NET.
        // Rỗng thì không gắn — để 403 đi về ODC nguyên vẹn thay vì đoán hộ.
        if (!string.IsNullOrWhiteSpace(userAgent))
            request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
        request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        var sw = Stopwatch.StartNew();
        try
        {
            // [OSInterface] cấm chữ ký async, nên phải chặn đồng bộ ở đây. Không phải ẩu:
            // ConfigureAwait(false) ở mọi chỗ để không bám SynchronizationContext → không deadlock.
            using var response = Http.SendAsync(request, cts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
            var body = response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
            return IdpUploadResult.Answered((int)response.StatusCode, body, (int)sw.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            return IdpUploadResult.Failed($"TIMEOUT: {timeoutSeconds}s", (int)sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            return IdpUploadResult.Failed($"TRANSPORT: {ex.GetType().Name}: {ex.Message}", (int)sw.ElapsedMilliseconds);
        }
    }
}
