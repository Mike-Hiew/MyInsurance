using System.Text;
using BnK.IdpUpload;

namespace BnK.IdpUpload.Tests;

/// <summary>
/// SRD §12.2 — bài test tồn tại của cả library: part document phải có filename=.
/// Chạy offline, bắt đúng cái bug ODC REST Consume đang gặp.
/// </summary>
public class MultipartShapeTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static string BuildBody(string? requestGroupId = null)
    {
        using var form = MultipartBuilder.Build(
            "TEST-DIAG", "6090,6091,6093", Png, "6093_vnvision-giay-ra-vien.png", "image/png", requestGroupId);
        return Encoding.Latin1.GetString(form.ReadAsByteArrayAsync().GetAwaiter().GetResult());
    }

    /// <summary>Tách body thành các part, mỗi part = (header block, content).</summary>
    private static List<(string Headers, string Content)> Parts(string body)
    {
        var boundaryLine = body[..body.IndexOf("\r\n", StringComparison.Ordinal)];
        return body.Split(boundaryLine)
            .Where(p => p.StartsWith("\r\n", StringComparison.Ordinal))
            .Select(p =>
            {
                var split = p.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                return (p[2..split], p[(split + 4)..^2]);
            })
            .ToList();
    }

    [Fact]
    public void DocumentPart_HasFilename()
    {
        Assert.Contains("name=\"document\"; filename=\"6093_vnvision-giay-ra-vien.png\"", BuildBody());
    }

    [Fact]
    public void DocumentPart_HasNoFilenameStar()
    {
        // curl -F không sinh filename*=; giữ hình dạng giống hệt request Swagger đã chạy được
        Assert.DoesNotContain("filename*", BuildBody());
    }

    [Fact]
    public void DocumentPart_HasContentTypeAndExactBytes()
    {
        var doc = Parts(BuildBody()).Single(p => p.Headers.Contains("name=\"document\""));
        Assert.Contains("Content-Type: image/png", doc.Headers);
        Assert.Equal(Encoding.Latin1.GetString(Png), doc.Content);
    }

    [Theory]
    [InlineData("name", "TEST-DIAG")]
    [InlineData("templates", "6090,6091,6093")]
    public void TextParts_HaveNoContentType(string partName, string expectedValue)
    {
        var part = Parts(BuildBody()).Single(p => p.Headers.Contains($"name=\"{partName}\""));
        Assert.DoesNotContain("Content-Type", part.Headers, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(expectedValue, part.Content);
    }

    [Fact]
    public void ExactlyThreeParts_WhenNoRequestGroupId()
    {
        Assert.Equal(3, Parts(BuildBody()).Count);
        Assert.Equal(3, Parts(BuildBody("")).Count);
    }

    [Fact]
    public void RequestGroupIdPart_AddedWhenGiven()
    {
        var parts = Parts(BuildBody("42"));
        Assert.Equal(4, parts.Count);
        var grp = parts.Single(p => p.Headers.Contains("name=\"requestGroupId\""));
        Assert.Equal("42", grp.Content);
    }

    [Fact]
    public void ContentTypeHeader_IsMultipartWithBoundary_NotSetByHand()
    {
        using var form = MultipartBuilder.Build("n", "1", Png, "a.png", "image/png", null);
        var ct = form.Headers.ContentType!;
        Assert.Equal("multipart/form-data", ct.MediaType);
        Assert.Contains(ct.Parameters, p => p.Name == "boundary" && !string.IsNullOrEmpty(p.Value));
    }
}
