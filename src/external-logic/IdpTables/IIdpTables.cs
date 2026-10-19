using OutSystems.ExternalLibraries.SDK;

namespace BnK.IdpTables;

[OSInterface(
    Name = "IdpTables",
    Description = "Đọc tables trong response IDP và trả về dạng phẳng — ODC không deserialize được mảng lồng mảng")]
public interface IIdpTables
{
    [OSAction(Description = "Duyệt RawJson, trả về từng ô của mọi bảng. Không phán đoán, không lọc.")]
    IdpTableParseResult ParseTables(
        [OSParameter(Description = "Body nguyên văn của IDP")] string RawJson,
        [OSParameter(Description = "Trần số ô, <= 0 thì mặc định 20000")] int MaxCells);
}
