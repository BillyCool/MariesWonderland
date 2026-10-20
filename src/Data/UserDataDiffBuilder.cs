using MariesWonderland.Proto.Data;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MariesWonderland.Data;

/// <summary>
/// Computes the delta between before/after snapshots of a <see cref="DarkUserMemoryDatabase"/>
/// to produce incremental <see cref="DiffData"/> maps for gRPC responses. Uses reflection at
/// startup to discover all EntityI* (client-visible) list properties. Rows are matched across
/// snapshots by their primary key (<see cref="EntityKey{T}"/>): new or changed rows are sent in
/// <c>UpdateRecordsJson</c> and rows removed by a handler in <c>DeleteKeysJson</c>.
/// </summary>
public static class UserDataDiffBuilder
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private sealed record Table(
        Func<DarkUserMemoryDatabase, string> Serialize,
        Func<DarkUserMemoryDatabase, TableSnapshot> Snapshot);

    // Maps "IUserWeapon" -> accessors for db.EntityIUserWeapon
    private static readonly IReadOnlyDictionary<string, Table> Tables;

    /// <summary>
    /// Builds the table-name → accessor map once via reflection. Only EntityI* list properties
    /// are included — EntityS* (server-only) and non-list properties are excluded.
    /// </summary>
    static UserDataDiffBuilder()
    {
        Dictionary<string, Table> tables = [];
        MethodInfo createTable = typeof(UserDataDiffBuilder).GetMethod(nameof(CreateTable), BindingFlags.NonPublic | BindingFlags.Static)!;

        foreach (PropertyInfo prop in typeof(DarkUserMemoryDatabase).GetProperties())
        {
            // Only include client-visible user data tables (EntityI* prefix, List<> type)
            if (!prop.Name.StartsWith("EntityI")) continue;
            if (!prop.PropertyType.IsGenericType) continue;
            if (prop.PropertyType.GetGenericTypeDefinition() != typeof(List<>)) continue;

            // Strip the "Entity" prefix to get the table key the client expects (e.g. "IUserWeapon")
            string tableName = prop.Name["Entity".Length..]; // "EntityIUser" -> "IUser"
            Type rowType = prop.PropertyType.GetGenericArguments()[0];
            tables[tableName] = (Table)createTable.MakeGenericMethod(rowType).Invoke(null, [prop])!;
        }

        Tables = tables;
    }

    private static Table CreateTable<T>(PropertyInfo prop)
    {
        Func<DarkUserMemoryDatabase, List<T>> getRows = db => (List<T>)prop.GetValue(db)!;

        return new Table(
            db => JsonSerializer.Serialize(getRows(db), CamelCaseOptions),
            db =>
            {
                TableSnapshot snapshot = new();
                foreach (T row in getRows(db))
                {
                    snapshot.Add(EntityKey<T>.Of(row), JsonSerializer.Serialize(row, CamelCaseOptions));
                }
                return snapshot;
            });
    }

    /// <summary>All client-visible user table names (IUser* series, excludes EntityS* and EntityM*).</summary>
    public static IEnumerable<string> TableNames => Tables.Keys;

    /// <summary>
    /// Captures the current rows of all non-empty client-visible user tables.
    /// Use this before making changes; pass the result to Delta() after changes.
    /// </summary>
    public static Dictionary<string, TableSnapshot> Snapshot(DarkUserMemoryDatabase db)
    {
        Dictionary<string, TableSnapshot> snapshot = [];
        foreach ((string name, Table table) in Tables)
        {
            TableSnapshot rows = table.Snapshot(db);
            if (rows.Count > 0)
            {
                snapshot[name] = rows;
            }
        }
        return snapshot;
    }

    /// <summary>
    /// Serializes a single named table to a JSON array string.
    /// Returns "[]" if the table name is not recognised.
    /// </summary>
    public static string SerializeTable(DarkUserMemoryDatabase db, string tableName)
        => Tables.TryGetValue(tableName, out Table? table) ? table.Serialize(db) : "[]";

    /// <summary>
    /// Computes the rows that changed since the snapshot, per table. New or changed rows are sent in
    /// <c>UpdateRecordsJson</c>; rows whose key no longer exists are sent in <c>DeleteKeysJson</c>.
    /// Tables with neither are left out. Against an empty snapshot this is the full user state.
    /// </summary>
    public static Dictionary<string, DiffData> Delta(Dictionary<string, TableSnapshot> before, DarkUserMemoryDatabase db)
    {
        Dictionary<string, DiffData> diff = [];
        foreach ((string name, Table table) in Tables)
        {
            TableSnapshot after = table.Snapshot(db);
            TableSnapshot beforeRows = before.TryGetValue(name, out TableSnapshot? rows) ? rows : TableSnapshot.Empty;

            List<string> updated = [.. after.RowsChangedFrom(beforeRows)];
            List<string> deleted = [.. beforeRows.RowsMissingFrom(after)];

            // Skip unchanged tables — only emit tables with actual modifications
            if (updated.Count == 0 && deleted.Count == 0) continue;

            diff[name] = new DiffData
            {
                UpdateRecordsJson = ToJsonArray(updated),
                DeleteKeysJson = ToJsonArray(deleted)
            };
        }
        return diff;
    }

    private static string ToJsonArray(IEnumerable<string> rowsJson)
    {
        StringBuilder sb = new("[");
        foreach (string row in rowsJson)
        {
            if (sb.Length > 1)
            {
                sb.Append(',');
            }
            sb.Append(row);
        }
        return sb.Append(']').ToString();
    }
}

/// <summary>
/// The rows of one user table at a point in time: each row's serialized JSON keyed by its
/// primary key, in table order.
/// </summary>
public sealed class TableSnapshot
{
    public static readonly TableSnapshot Empty = new();

    private readonly List<(object Key, string Json)> _rows = [];
    private readonly Dictionary<object, string> _byKey = [];

    public int Count => _rows.Count;

    public void Add(object key, string json)
    {
        _rows.Add((key, json));
        _byKey.TryAdd(key, json);
    }

    /// <summary>JSON of the rows that are new or whose content differs from <paramref name="previous"/>.</summary>
    public IEnumerable<string> RowsChangedFrom(TableSnapshot previous)
        => _rows.Where(row => !previous._byKey.TryGetValue(row.Key, out string? json) || json != row.Json).Select(row => row.Json);

    /// <summary>JSON of the rows whose key doesn't exist in <paramref name="other"/>.</summary>
    public IEnumerable<string> RowsMissingFrom(TableSnapshot other)
        => _rows.Where(row => !other._byKey.ContainsKey(row.Key)).Select(row => row.Json);
}
