using OutSystems.ExternalLibraries.SDK;

namespace BnK.IdpUpload;

[OSInterface(
    Name = "IdpUpload",
    Description = "Nộp chứng từ multipart lên BnK IDP, có filename= mà ODC REST Consume không đặt được")]
public interface IIdpUpload
{
    [OSAction(Description = "POST /request/sync — nộp và chờ kết quả trong một lời gọi")]
    IdpUploadResult SubmitSync(
        [OSParameter(Description = "https://idp-api.idpstg.bnksolution.com")] string BaseUrl,
        [OSParameter(Description = "JWT, không kèm chữ Bearer")] string AccessToken,
        [OSParameter(Description = "Chuỗi user-agent giả trình duyệt")] string UserAgent,
        [OSParameter(Description = "ClaimNo — chỗ duy nhất mang được sang IDP")] string Name,
        [OSParameter(Description = "Chuỗi ngăn cách phẩy, KHÔNG phải mảng")] string TemplatesCsv,
        [OSParameter(Description = "Nội dung file")] byte[] Document,
        [OSParameter(Description = "Tên file kèm đuôi, ví dụ a.png")] string FileName,
        [OSParameter(Description = "Để rỗng thì suy từ đuôi file")] string ContentType,
        [OSParameter(Description = "Mặc định 60 nếu <= 0")] int TimeoutSeconds);

    [OSAction(Description = "POST /request — nộp bất đồng bộ, trả requestId")]
    IdpUploadResult Submit(
        [OSParameter(Description = "https://idp-api.idpstg.bnksolution.com")] string BaseUrl,
        [OSParameter(Description = "JWT, không kèm chữ Bearer")] string AccessToken,
        [OSParameter(Description = "Chuỗi user-agent giả trình duyệt")] string UserAgent,
        [OSParameter(Description = "ClaimNo — chỗ duy nhất mang được sang IDP")] string Name,
        [OSParameter(Description = "Chuỗi ngăn cách phẩy, KHÔNG phải mảng")] string TemplatesCsv,
        [OSParameter(Description = "Nội dung file")] byte[] Document,
        [OSParameter(Description = "Tên file kèm đuôi, ví dụ a.png")] string FileName,
        [OSParameter(Description = "Để rỗng thì suy từ đuôi file")] string ContentType,
        [OSParameter(Description = "Bỏ qua nếu rỗng")] string RequestGroupId,
        [OSParameter(Description = "Mặc định 60 nếu <= 0")] int TimeoutSeconds);
}
