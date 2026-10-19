using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace BnK.IdpUpload.Tests;

/// <summary>
/// HTTP server giả chạy thẳng trên máy (thay container — BRD §11, Docker không bắt buộc).
/// Ghi lại request thô nhận được, và bắt chước Cloudflare: thiếu User-Agent → 403 "error code: 1010".
/// Đọc byte thẳng từ socket để thấy đúng cái lên dây, không qua parser nào.
/// </summary>
internal sealed class MockIdpServer : IDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource _cts = new();

    public List<CapturedRequest> Requests { get; } = [];
    public int ResponseStatus { get; set; } = 201;
    public string ResponseBody { get; set; } = "{\"id\":1}";
    public TimeSpan ResponseDelay { get; set; } = TimeSpan.Zero;
    public string BaseUrl { get; }

    public MockIdpServer()
    {
        _listener.Start();
        BaseUrl = $"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/";
        _ = Task.Run(AcceptLoop);
    }

    private async Task AcceptLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            TcpClient client;
            try { client = await _listener.AcceptTcpClientAsync(_cts.Token); }
            catch { return; }
            _ = Task.Run(() => Handle(client));
        }
    }

    private async Task Handle(TcpClient client)
    {
        using var _ = client;
        var stream = client.GetStream();
        var buffer = new List<byte>();
        var chunk = new byte[8192];
        int headerEnd = -1;
        while (headerEnd < 0)
        {
            var n = await stream.ReadAsync(chunk);
            if (n == 0) return;
            buffer.AddRange(chunk.AsSpan(0, n).ToArray());
            headerEnd = IndexOf(buffer, "\r\n\r\n"u8.ToArray());
        }

        var headerText = Encoding.ASCII.GetString(buffer.GetRange(0, headerEnd).ToArray());
        var lines = headerText.Split("\r\n");
        var headers = lines.Skip(1)
            .Select(l => l.Split(':', 2))
            .ToDictionary(p => p[0].Trim(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);

        var contentLength = headers.TryGetValue("Content-Length", out var cl) ? int.Parse(cl) : 0;
        var bodyStart = headerEnd + 4;
        while (buffer.Count - bodyStart < contentLength)
        {
            var n = await stream.ReadAsync(chunk);
            if (n == 0) break;
            buffer.AddRange(chunk.AsSpan(0, n).ToArray());
        }
        var body = buffer.GetRange(bodyStart, Math.Min(contentLength, buffer.Count - bodyStart)).ToArray();

        lock (Requests) Requests.Add(new CapturedRequest(lines[0], headers, body));

        if (ResponseDelay > TimeSpan.Zero)
        {
            try { await Task.Delay(ResponseDelay, _cts.Token); } catch { return; }
        }

        var (status, respBody, reason) = headers.ContainsKey("User-Agent")
            ? (ResponseStatus, ResponseBody, "OK")
            : (403, "error code: 1010", "Forbidden");
        var respBytes = Encoding.UTF8.GetBytes(respBody);
        var head = $"HTTP/1.1 {status} {reason}\r\nContent-Type: application/json\r\nContent-Length: {respBytes.Length}\r\nConnection: close\r\n\r\n";
        try
        {
            await stream.WriteAsync(Encoding.ASCII.GetBytes(head));
            await stream.WriteAsync(respBytes);
        }
        catch (IOException) { /* client đã bỏ đi (timeout test) */ }
    }

    private static int IndexOf(List<byte> haystack, byte[] needle)
    {
        for (var i = 0; i <= haystack.Count - needle.Length; i++)
        {
            var match = true;
            for (var j = 0; j < needle.Length && match; j++) match = haystack[i + j] == needle[j];
            if (match) return i;
        }
        return -1;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
    }
}

internal sealed record CapturedRequest(string RequestLine, Dictionary<string, string> Headers, byte[] Body)
{
    public string BodyText => Encoding.Latin1.GetString(Body);
}

/// <summary>Logger ghi lại mọi dòng — để khẳng định token/RawJson không bao giờ lọt vào log.</summary>
internal sealed class CapturingLogger : ILogger
{
    public List<string> Lines { get; } = [];
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        lock (Lines) Lines.Add(formatter(state, exception) + (exception is null ? "" : " " + exception));
    }
}
