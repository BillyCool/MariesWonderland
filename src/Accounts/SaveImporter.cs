using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using System.Collections;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MariesWonderland.Accounts;

/// <summary>A parsed save: the user data, a summary for the queue, and anything odd found in it.</summary>
public sealed record ParsedSave(DarkUserMemoryDatabase Save, string PlayerName, int Level, long PlayerId, int RowCount, IReadOnlyList<string> Warnings);

/// <summary>
/// Reads a save from the real game in the formats it is likely to come in:
/// <list type="bullet">
/// <item>a zip (or folder) of <c>EntityIUser…Table.json</c> files, the format of <c>Data/UserData</c>;</item>
/// <item>one JSON object of table name → rows, with names like <c>IUserCostume</c>, <c>EntityIUserCostume</c> or
/// <c>EntityIUserCostumeTable</c>, rows as an array or as a JSON string (a captured <c>GetUserData</c> response, also
/// when wrapped in <c>userDataJson</c>);</item>
/// <item>a save point (<c>/debug/save</c>, the admin tool's export).</item>
/// </list>
/// Field names may be PascalCase or camelCase. Unknown tables are ignored with a warning. The public upload takes only
/// the zip (<see cref="ParseZip"/>); administrators' "import now" takes them all.
/// </summary>
public static class SaveImporter
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    private static readonly Dictionary<string, UserTable> TablesByName = BuildNames();

    private static Dictionary<string, UserTable> BuildNames()
    {
        Dictionary<string, UserTable> names = new(StringComparer.OrdinalIgnoreCase);
        foreach (UserTable table in UserTables.All)
        {
            names[table.Name] = table;
            names["Entity" + table.Name] = table;
            names["Entity" + table.Name + "Table"] = table;
            names[table.Name + "Table"] = table;
        }
        return names;
    }

    public static ParsedSave Parse(Stream stream, string fileName, DarkMasterMemoryDatabase masterDb)
    {
        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        byte[] bytes = buffer.ToArray();

        DarkUserMemoryDatabase save = new();
        List<string> warnings = [];
        bool isZip = bytes.Length > 4 && bytes[0] == 'P' && bytes[1] == 'K';
        if (isZip)
        {
            using ZipArchive zip = new(new MemoryStream(bytes), ZipArchiveMode.Read);
            foreach (ZipArchiveEntry entry in zip.Entries.Where(e => e.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
            {
                using StreamReader reader = new(entry.Open());
                string json = reader.ReadToEnd();
                string tableName = Path.GetFileNameWithoutExtension(entry.Name);
                if (!TryReadTable(save, tableName, JsonNode.Parse(json), warnings))
                {
                    // A zip of one combined JSON file.
                    ReadObject(save, JsonNode.Parse(json), warnings);
                }
            }
        }
        else
        {
            ReadObject(save, JsonNode.Parse(bytes), warnings);
        }

        return Finish(save, masterDb, warnings);
    }

    /// <summary>
    /// The public upload's only format: a zip of <c>EntityIUser…Table.json</c> files. Refuses anything that isn't a zip,
    /// and a zip whose files add up to more than <paramref name="maxUnzippedBytes"/> once unpacked (a small zip can
    /// unpack to gigabytes).
    /// </summary>
    public static ParsedSave ParseZip(Stream stream, DarkMasterMemoryDatabase masterDb, long maxUnzippedBytes)
    {
        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        buffer.Position = 0;
        byte[] head = buffer.GetBuffer();
        if (buffer.Length < 4 || head[0] != 'P' || head[1] != 'K' || head[2] != 3 || head[3] != 4)
        {
            throw new InvalidDataException("It isn't a zip file.");
        }

        using ZipArchive zip = new(buffer, ZipArchiveMode.Read);
        List<ZipArchiveEntry> tables = [.. zip.Entries.Where(e => e.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))];
        // The sizes in the zip's directory; reading an entry also fails if it unpacks to more than its stated size.
        if (tables.Sum(e => e.Length) > maxUnzippedBytes)
        {
            throw new InvalidDataException($"It unpacks to more than {maxUnzippedBytes / (1024 * 1024)} MB.");
        }

        DarkUserMemoryDatabase save = new();
        List<string> warnings = [];
        foreach (ZipArchiveEntry entry in tables)
        {
            using StreamReader reader = new(entry.Open());
            TryReadTable(save, Path.GetFileNameWithoutExtension(entry.Name), JsonNode.Parse(reader.ReadToEnd()), warnings);
        }
        return Finish(save, masterDb, warnings);
    }

    /// <summary>A folder of table files (the seed save's format).</summary>
    public static ParsedSave ParseFolder(string folder, DarkMasterMemoryDatabase masterDb)
    {
        DarkUserMemoryDatabase save = new();
        List<string> warnings = [];
        foreach (string file in Directory.EnumerateFiles(folder, "*.json"))
        {
            TryReadTable(save, Path.GetFileNameWithoutExtension(file), JsonNode.Parse(File.ReadAllText(file)), warnings);
        }
        return Finish(save, masterDb, warnings);
    }

    private static void ReadObject(DarkUserMemoryDatabase save, JsonNode? node, List<string> warnings)
    {
        if (node is not JsonObject obj)
        {
            throw new InvalidDataException("The file isn't a JSON object of tables or a zip of table files.");
        }

        // A captured GetUserData response keeps the tables in userDataJson.
        JsonNode? wrapped = obj.FirstOrDefault(p => string.Equals(p.Key, "userDataJson", StringComparison.OrdinalIgnoreCase)).Value;
        if (wrapped is JsonObject inner)
        {
            obj = inner;
        }

        foreach ((string name, JsonNode? value) in obj)
        {
            if (string.Equals(name, "UserId", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            if (!TryReadTable(save, name, value, warnings))
            {
                warnings.Add($"Ignored unknown table '{name}'.");
            }
        }
    }

    private static bool TryReadTable(DarkUserMemoryDatabase save, string name, JsonNode? value, List<string> warnings)
    {
        if (!TablesByName.TryGetValue(name, out UserTable? table))
        {
            return false;
        }

        // GetUserData sends each table as a JSON string.
        if (value is JsonValue text && text.TryGetValue(out string? json))
        {
            value = JsonNode.Parse(json);
        }
        if (value is not JsonArray)
        {
            warnings.Add($"Table '{name}' isn't a list of rows; ignored.");
            return true;
        }

        IList rows = (IList)value.Deserialize(table.Property.PropertyType, Options)!;
        IList target = (IList)table.Property.GetValue(save)!;
        foreach (object row in rows)
        {
            target.Add(row);
        }
        return true;
    }

    private static ParsedSave Finish(DarkUserMemoryDatabase save, DarkMasterMemoryDatabase masterDb, List<string> warnings)
    {
        EntityIUser user = save.EntityIUser.FirstOrDefault()
            ?? throw new InvalidDataException("The save has no user row (IUser table).");
        if (save.EntityIUser.Count > 1)
        {
            throw new InvalidDataException("The save holds more than one user.");
        }

        int unknownCostumes = save.EntityIUserCostume.Count(c => masterDb.EntityMCostume.Find(c.CostumeId) == null);
        int unknownWeapons = save.EntityIUserWeapon.Count(w => masterDb.EntityMWeapon.Find(w.WeaponId) == null);
        if (unknownCostumes > 0)
        {
            warnings.Add($"{unknownCostumes} costumes aren't in the master data.");
        }
        if (unknownWeapons > 0)
        {
            warnings.Add($"{unknownWeapons} weapons aren't in the master data.");
        }
        if (save.EntityIUserStatus.Count == 0 || save.EntityIUserProfile.Count == 0)
        {
            warnings.Add("The save has no status or profile row; it may not load in the game.");
        }

        int rows = UserTables.All.Sum(t => ((IList)t.Property.GetValue(save)!).Count);
        save.UserId = user.UserId;
        DeriveServerState(save);
        return new ParsedSave(save, save.EntityIUserProfile.FirstOrDefault()?.Name ?? "", save.EntityIUserStatus.FirstOrDefault()?.Level ?? 0, user.PlayerId, rows, warnings);
    }

    /// <summary>
    /// Server-only state a save doesn't carry, derived from what it does (the roadmap's "Save import" list): the
    /// lifetime Cage walk from the walking achievements (missions 410001–410013 hold the lifetime metres).
    /// </summary>
    private static void DeriveServerState(DarkUserMemoryDatabase save)
    {
        int walked = save.EntityIUserMission.Where(m => m.MissionId is >= 410001 and <= 410013).Select(m => m.ProgressValue).DefaultIfEmpty(0).Max();
        if (walked > 0)
        {
            PlayHistoryHelper.Best(save, GamePlayHistoryType.CAGE_WALKED_DISTANCE, walked);
        }
    }
}
