// Smoke test cục bộ: gọi library thật vào mock_idp.py (0 quota).
//   python mock_idp.py
//   dotnet run --project IdpUpload.Smoke -- [baseUrl] [đường dẫn file]
using BnK.IdpUpload;
using Microsoft.Extensions.Logging;

var baseUrl = args.Length > 0 ? args[0] : "http://localhost:8099";
var path = args.Length > 1
    ? args[1]
    : @"E:\Hackaton\insurance BNK\PMOInsure\PMOInsure\Claude outputs\6093_vnvision-giay-ra-vien.png";
const string Ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36";
const string Templates = "6090,6091,6092,6093,6094,6095,6096,6097,6098";
const string FakeToken = "eyJhbGciOiJIUzI1NiJ9.FAKE-LOCAL-TOKEN.sig";

var bytes = File.ReadAllBytes(path);
var fileName = Path.GetFileName(path);
IIdpUpload lib = new IdpUpload(new ConsoleLogger());

Show("1. SubmitSync — file 6093 (kỳ vọng 201)",
    lib.SubmitSync(baseUrl, FakeToken, Ua, "TEST-DIAG", Templates, bytes, fileName, "", 60));
Show("2. SubmitSync — thiếu User-Agent (kỳ vọng 403 nguyên vẹn, A6)",
    lib.SubmitSync(baseUrl, FakeToken, "", "TEST-DIAG", Templates, bytes, fileName, "", 60));
Show("3. SubmitSync — .xlsx (kỳ vọng chặn, mock KHÔNG thấy request, A4)",
    lib.SubmitSync(baseUrl, FakeToken, Ua, "TEST-DIAG", Templates, bytes, "a.xlsx", "", 60));
Show("4. Submit — bất đồng bộ (kỳ vọng requestId, A8)",
    lib.Submit(baseUrl, FakeToken, Ua, "TEST-DIAG", Templates, bytes, fileName, "", "", 60));

// A5 — chặn trước khi chạm mạng. Hai case, một ở trên trần một ở đúng trần.
var over  = new byte[5 * 1024 * 1024 + 1];   // 5.242.881 — vượt đúng 1 byte
var exact = new byte[5 * 1024 * 1024];       // 5.242.880 — đúng trần, phải đi qua
Show("5. SubmitSync — 5 MiB + 1 byte (kỳ vọng PAYLOAD_TOO_LARGE, mock KHÔNG thấy request, A5)",
    lib.SubmitSync(baseUrl, FakeToken, Ua, "TEST-DIAG", Templates, over, "big.png", "", 60));
Show("6. SubmitSync — đúng 5 MiB (kỳ vọng 201 — trần là <=, không off-by-one)",
    lib.SubmitSync(baseUrl, FakeToken, Ua, "TEST-DIAG", Templates, exact, "edge.png", "", 60));

static void Show(string title, IdpUploadResult r)
{
    Console.WriteLine($"\n## {title}");
    Console.WriteLine($"   TransportOk={r.TransportOk} HttpStatus={r.HttpStatus} ElapsedMs={r.ElapsedMs}");
    Console.WriteLine($"   ErrorMessage={r.ErrorMessage}");
    Console.WriteLine($"   RawJson={r.RawJson}");
}

sealed class ConsoleLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) => Console.WriteLine($"   [log] {formatter(state, exception)}");
}
