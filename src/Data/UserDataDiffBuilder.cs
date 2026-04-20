using MariesWonderland.Proto.Data;
using System.Text.Json;

namespace MariesWonderland.Data;

/// <summary>
/// Computes the delta between before/after snapshots of a <see cref="DarkUserMemoryDatabase"/>
/// to produce incremental <see cref="DiffData"/> maps for gRPC responses. Uses reflection at
/// startup to discover all EntityI* (client-visible) list properties and builds per-table serializers.
/// </summary>
public static class UserDataDiffBuilder
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // Maps "IUser" -> func that serializes db.EntityIUser to JSON
    private static readonly IReadOnlyDictionary<string, Func<DarkUserMemoryDatabase, string>> Serializers;

    /// <summary>
    /// Builds the table-name → serializer map once via reflection. Only EntityI* list properties
    /// are included — EntityS* (server-only) and non-list properties are excluded.
    /// </summary>
    static UserDataDiffBuilder()
    {
        var serializers = new Dictionary<string, Func<DarkUserMemoryDatabase, string>>();

        foreach (var prop in typeof(DarkUserMemoryDatabase).GetProperties())
        {
            // Only include client-visible user data tables (EntityI* prefix, List<> type)
            if (!prop.Name.StartsWith("EntityI")) continue;
            if (!prop.PropertyType.IsGenericType) continue;
            if (prop.PropertyType.GetGenericTypeDefinition() != typeof(List<>)) continue;

            // Strip the "Entity" prefix to get the table key the client expects (e.g. "IUserWeapon")
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
    /// Serializes a single named table to a JSON array string.
    /// Returns "[]" if the table name is not recognised.
    /// </summary>
    public static string SerializeTable(DarkUserMemoryDatabase db, string tableName)
        => Serializers.TryGetValue(tableName, out var serialize) ? serialize(db) : "[]";

    /// <summary>
    /// Computes only the tables that changed since the snapshot.
    /// Use this for incremental API responses (e.g. SetUserName).
    /// </summary>
    public static Dictionary<string, DiffData> Delta(Dictionary<string, string> before, DarkUserMemoryDatabase db)
    {
        Dictionary<string, DiffData> diff = [];
        foreach (var (table, serialize) in Serializers)
        {
            // Serialize the current (post-mutation) state of this table
            var afterJson = serialize(db);
            before.TryGetValue(table, out var beforeJson);
            beforeJson ??= "[]";

            // Skip unchanged tables — only emit tables with actual modifications
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
