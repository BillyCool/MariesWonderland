using MariesWonderland.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MariesWonderland.Data;

/// <summary>
/// Loads pre-seeded user data from Entity*Table.json files on disk into a <see cref="DarkUserMemoryDatabase"/>.
/// Uses reflection to match JSON file names to database properties, allowing new entity types to be
/// loaded without code changes.
/// </summary>
public class UserDataSeeder(IOptions<ServerOptions> options)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Type DbType = typeof(DarkUserMemoryDatabase);

    /// <summary>
    /// Reads all Entity*Table.json files from the configured UserDataPath and populates
    /// a new DarkUserMemoryDatabase. Returns an empty database if no files are found.
    /// </summary>
    public DarkUserMemoryDatabase LoadFromFiles()
    {
        DarkUserMemoryDatabase db = new();

        string rawPath = options.Value.Data.UserDataPath;
        if (string.IsNullOrEmpty(rawPath))
            return db;

        string dataPath = Path.IsPathRooted(rawPath)
            ? rawPath
            : Path.Combine(AppContext.BaseDirectory, rawPath);

        if (!Directory.Exists(dataPath))
            return db;

        foreach (string file in Directory.EnumerateFiles(dataPath, "Entity*Table.json"))
        {
            // "EntityIUserCostumeTable.json" → "EntityIUserCostume"
            string fileName = Path.GetFileName(file);
            string propName = fileName[..^"Table.json".Length];

            var prop = DbType.GetProperty(propName);
            if (prop == null) continue;

            string json = File.ReadAllText(file);
            var list = JsonSerializer.Deserialize(json, prop.PropertyType, JsonOptions);
            if (list != null)
                prop.SetValue(db, list);
        }

        return db;
    }
}
