using BnK.IdpUpload;

namespace BnK.IdpUpload.Tests;

public class ContentTypeMapTests
{
    [Theory]
    [InlineData("a.PNG", "image/png")]
    [InlineData("a.png", "image/png")]
    [InlineData("a.jpg", "image/jpeg")]
    [InlineData("a.jpeg", "image/jpeg")]
    [InlineData("a.pdf", "application/pdf")]
    [InlineData("a.tif", "image/tiff")]
    [InlineData("a.TIFF", "image/tiff")]
    public void Resolve_RecognizesSupportedExtensions(string fileName, string expected)
    {
        Assert.Equal(expected, ContentTypeMap.Resolve(fileName));
    }

    [Theory]
    [InlineData("a.xlsx")]
    [InlineData("a.txt")]
    [InlineData("a")]
    [InlineData("")]
    [InlineData(null)]
    public void Resolve_RejectsUnsupportedExtensions(string? fileName)
    {
        Assert.Null(ContentTypeMap.Resolve(fileName));
    }
}
