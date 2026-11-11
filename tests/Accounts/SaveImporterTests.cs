using MariesWonderland.Accounts;
using MariesWonderland.Data;
using MariesWonderland.Models.Type;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace MariesWonderland.Tests.Accounts;

/// <summary>The save importer's formats, all checked against the real save in <c>Data/UserData</c>.</summary>
public sealed class SaveImporterTests
{
    private static readonly DarkMasterMemoryDatabase MasterDb = new();

    private static ParsedSave Folder() => SaveImporter.ParseFolder("Data/UserData", MasterDb);

    [Fact]
    public void Folder_ReadsTheRealSave()
    {
        ParsedSave parsed = Folder();

        Assert.Equal(1237766254823960123, parsed.Save.UserId);
        Assert.NotEmpty(parsed.Save.EntityIUserCostume);
        Assert.True(parsed.Level > 0);
        Assert.True(parsed.RowCount > 1000);
    }

    [Fact]
    public void Zip_OfTheTableFiles_ReadsTheSame()
    {
        using MemoryStream zip = new();
        using (ZipArchive archive = new(zip, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (string file in Directory.EnumerateFiles("Data/UserData", "*.json"))
            {
                archive.CreateEntryFromFile(file, "save/" + Path.GetFileName(file));
            }
        }
        zip.Position = 0;

        ParsedSave parsed = SaveImporter.Parse(zip, "save.zip", MasterDb);

        Assert.Equal(Folder().RowCount, parsed.RowCount);
    }

    private static MemoryStream ZipOfTheRealSave(CompressionLevel level = CompressionLevel.Optimal)
    {
        MemoryStream zip = new();
        using (ZipArchive archive = new(zip, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (string file in Directory.EnumerateFiles("Data/UserData", "*.json"))
            {
                archive.CreateEntryFromFile(file, Path.GetFileName(file), level);
            }
        }
        zip.Position = 0;
        return zip;
    }

    [Fact]
    public void PublicUpload_TheRealSaveZipped_FitsTheLimits()
    {
        Configuration.WebOptions limits = new();
        using MemoryStream zip = ZipOfTheRealSave();

        Assert.True(zip.Length <= limits.MaxImportZipKilobytes * 1024L, $"{zip.Length} bytes");
        ParsedSave parsed = SaveImporter.ParseZip(zip, MasterDb, limits.MaxImportUnzippedMegabytes * 1024L * 1024L);
        Assert.Equal(Folder().RowCount, parsed.RowCount);
    }

    [Fact]
    public void PublicUpload_RefusesAnythingButAZip()
    {
        byte[] json = Encoding.UTF8.GetBytes("""{ "IUser": [ { "UserId": 1 } ] }""");

        Assert.Throws<InvalidDataException>(() => SaveImporter.ParseZip(new MemoryStream(json), MasterDb, 1024 * 1024));
    }

    [Fact]
    public void PublicUpload_RefusesAZipThatUnpacksTooLarge()
    {
        using MemoryStream zip = ZipOfTheRealSave();

        InvalidDataException error = Assert.Throws<InvalidDataException>(() => SaveImporter.ParseZip(zip, MasterDb, 1024 * 1024));
        Assert.Contains("unpacks to more than", error.Message);
    }

    [Fact]
    public void GetUserDataResponse_WithTablesAsJsonStrings_ReadsTheSame()
    {
        // A captured GetUserData response: table name → JSON string, camelCase fields, inside userDataJson.
        JsonSerializerOptions camel = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        DarkUserMemoryDatabase save = Folder().Save;
        Dictionary<string, string> tables = UserTables.All
            .Where(t => t.IsClientVisible)
            .ToDictionary(t => t.Name, t => JsonSerializer.Serialize(t.Property.GetValue(save), t.Property.PropertyType, camel));
        string json = JsonSerializer.Serialize(new { userDataJson = tables });

        ParsedSave parsed = SaveImporter.Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)), "capture.json", MasterDb);

        Assert.Equal(save.EntityIUserCostume.Count, parsed.Save.EntityIUserCostume.Count);
        Assert.Equal(save.EntityIUser.Single().PlayerId, parsed.PlayerId);
    }

    [Fact]
    public void SavePoint_ReadsTheSame()
    {
        DarkUserMemoryDatabase save = Folder().Save;
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(save);

        ParsedSave parsed = SaveImporter.Parse(new MemoryStream(json), "point.json", MasterDb);

        Assert.Equal(save.EntityIUserWeapon.Count, parsed.Save.EntityIUserWeapon.Count);
    }

    [Fact]
    public void ASaveWithoutAUser_IsRefused()
        => Assert.Throws<InvalidDataException>(() => SaveImporter.Parse(new MemoryStream("{\"IUserCostume\":[]}"u8.ToArray()), "x.json", MasterDb));

    [Fact]
    public void UnknownTables_AreIgnoredWithAWarning()
    {
        string json = "{\"IUser\":[{\"UserId\":5,\"PlayerId\":123456789012}],\"ISomethingElse\":[]}";

        ParsedSave parsed = SaveImporter.Parse(new MemoryStream(Encoding.UTF8.GetBytes(json)), "x.json", MasterDb);

        Assert.Contains(parsed.Warnings, w => w.Contains("ISomethingElse"));
    }

    [Fact]
    public void TheCageWalk_IsTakenFromTheWalkingAchievements()
    {
        ParsedSave parsed = Folder();
        int expected = parsed.Save.EntityIUserMission.Where(m => m.MissionId is >= 410001 and <= 410013).Select(m => m.ProgressValue).DefaultIfEmpty(0).Max();

        Assert.Equal(expected, MariesWonderland.Helpers.PlayHistoryHelper.Get(parsed.Save, GamePlayHistoryType.CAGE_WALKED_DISTANCE));
    }
}
