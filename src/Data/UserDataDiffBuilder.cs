using MariesWonderland.Proto.Data;
using System.Text.Json;

namespace MariesWonderland.Data;

public static class UserDataDiffBuilder
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // Maps "IUser" -> func that serializes db.EntityIUser to JSON
    private static readonly IReadOnlyDictionary<string, Func<DarkUserMemoryDatabase, string>> Serializers;

    static UserDataDiffBuilder()
    {
        var serializers = new Dictionary<string, Func<DarkUserMemoryDatabase, string>>();

        foreach (var prop in typeof(DarkUserMemoryDatabase).GetProperties())
        {
            // Only client-visible user data tables
            if (!prop.Name.StartsWith("EntityI")) continue;
            if (!prop.PropertyType.IsGenericType) continue;
            if (prop.PropertyType.GetGenericTypeDefinition() != typeof(List<>)) continue;

            var tableName = prop.Name["Entity".Length..]; // "EntityIUser" -> "IUser"
            var capturedProp = prop; // capture for lambda
            serializers[tableName] = db =>
            {
                var list = capturedProp.GetValue(db);
                return JsonSerializer.Serialize(list, CamelCaseOptions);
            };
        }

        Serializers = serializers;
    }

    /// <summary>All client-visible user table names (IUser* series, excludes EntityS* and EntityM*).</summary>
    public static IEnumerable<string> TableNames => Serializers.Keys;

    /// <summary>
    /// Captures the current state of all non-empty client-visible user tables as serialized JSON.
    /// Use this before making changes; pass the result to Delta() after changes.
    /// </summary>
    public static Dictionary<string, string> Snapshot(DarkUserMemoryDatabase db)
    {
        var snapshot = new Dictionary<string, string>();
        foreach (var (table, serialize) in Serializers)
        {
            var json = serialize(db);
            if (json != "[]")
                snapshot[table] = json;
        }
        return snapshot;
    }

    /// <summary>
    /// Builds a full diff of all client-visible user tables.
    /// Non-empty tables contain their data; empty tables are explicitly sent as "[]".
    /// Use this for Auth and RegisterUser to perform a complete client sync.
    /// </summary>
    public static Dictionary<string, DiffData> FullDiff(DarkUserMemoryDatabase db)
    {
        var diff = new Dictionary<string, DiffData>();
        foreach (var (table, serialize) in Serializers)
        {
            diff[table] = new DiffData
            {
                UpdateRecordsJson = serialize(db),
                DeleteKeysJson = "[]"
            };
        }
        return diff;
    }

    /// <summary>
    /// Serializes a single named table to a JSON array string.
    /// Returns "[]" if the table name is not recognised.
    /// </summary>
    public static string SerializeTable(DarkUserMemoryDatabase db, string tableName)
        => Serializers.TryGetValue(tableName, out var serialize) ? serialize(db) : "[]";

    /// <summary>
    /// Computes only the tables that changed since the snapshot.
    /// Use this for incremental API responses (e.g. SetUserName, GameStart).
    /// </summary>
    public static Dictionary<string, DiffData> Delta(Dictionary<string, string> before, DarkUserMemoryDatabase db)
    {
        var diff = new Dictionary<string, DiffData>();
        foreach (var (table, serialize) in Serializers)
        {
            var afterJson = serialize(db);
            before.TryGetValue(table, out var beforeJson);
            beforeJson ??= "[]";

            if (afterJson == beforeJson) continue;

            diff[table] = new DiffData
            {
                UpdateRecordsJson = afterJson,
                DeleteKeysJson = "[]"
            };
        }
        return diff;
    }
}
