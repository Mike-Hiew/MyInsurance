using System.Net.Http.Headers;
using System.Text;

namespace BnK.IdpUpload;

/// <summary>
/// Lý do library tồn tại. Part "document" PHẢI có filename= — busboy (multer) phía IDP chỉ coi part là
/// file khi có token đó; thiếu nó thì req.file undefined → 500 "reading 'path'".
/// Dựng toàn bộ trong bộ nhớ, không ghi đĩa.
/// </summary>
internal static class MultipartBuilder
{
    public static MultipartFormDataContent Build(
        string name, string templatesCsv, byte[] document, string fileName, string contentType, string? requestGroupId)
    {
        // Không bao giờ đặt Content-Type của cả request bằng tay: MultipartFormDataContent tự sinh kèm boundary.
        // Bẫy đã biết: .NET sinh boundary="..." có nháy kép. Busboy chịu được; nếu server báo lỗi parse lạ
        // thì gỡ nháy ở form.Headers.ContentType.Parameters — nhìn chỗ này đầu tiên.
        var form = new MultipartFormDataContent();

        form.AddText(name, "name");
        form.AddText(templatesCsv, "templates");   // chuỗi phẩy; gửi mảng JSON → 400 bigint "NaN"

        var fileContent = new ByteArrayContent(document);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "document", fileName);     // overload 3 tham số — tham số thứ 3 sinh ra filename=
        var disposition = fileContent.Headers.ContentDisposition!;
        // .NET 8 sinh name=document; filename=a.png KHÔNG nháy, và thêm filename*=utf-8''…; curl -F thì
        // name="document"; filename="a.png". Đặt lại cho giống hệt request Swagger đã chạy được.
        disposition.Name = Quote("document");
        disposition.FileName = Quote(fileName);
        disposition.FileNameStar = null;

        if (!string.IsNullOrEmpty(requestGroupId))
            form.AddText(requestGroupId, "requestGroupId");

        return form;
    }

    // Part text: KHÔNG có Content-Type, giống curl -F 'name=X'
    private static void AddText(this MultipartFormDataContent form, string value, string partName)
    {
        var content = new StringContent(value, Encoding.UTF8);
        content.Headers.ContentType = null;
        form.Add(content);
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = Quote(partName) };
    }

    private static string Quote(string value) => "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
}
