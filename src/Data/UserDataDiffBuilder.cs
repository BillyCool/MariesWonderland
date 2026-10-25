using MariesWonderland.Proto.Data;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace MariesWonderland.Data;

/// <summary>
/// Computes the delta between before/after snapshots of a <see cref="DarkUserMemoryDatabase"/>.
/// Uses reflection at startup to discover every user table (<see cref="UserTables"/>). Rows are matched across
/// snapshots by their primary key (<see cref="EntityKey{T}"/>). Two views of the same delta:
/// <list type="bullet">
/// <item>the client diff (<see cref="Delta"/>, <see cref="ClientDiff"/>): client-visible EntityI* tables only, new or
/// changed rows in <c>UpdateRecordsJson</c> and removed ones in <c>DeleteKeysJson</c>;</item>
/// <item>the change set (<see cref="Changes"/>): every table, server-only EntityS* included, as row objects for the
/// user database to write.</item>
/// </list>
/// </summary>
public static class UserDataDiffBuilder
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private sealed record Table(
        UserTable Info,
        Func<DarkUserMemoryDatabase, string> Serialize,
        Func<DarkUserMemoryDatabase, TableSnapshot> Snapshot,
        Func<string, object> Deserialize,
        string[] KeyNames,
        Action<DarkUserMemoryDatabase, IEnumerable<string>> ReplaceRows);

    // Maps "IUserWeapon" -> accessors for db.EntityIUserWeapon, "SUserGift" -> db.EntitySUserGift, …
    private static readonly IReadOnlyDictionary<string, Table> AllTables;
    private static readonly IReadOnlyDictionary<string, Table> ClientTables;

    /// <summary>Builds the table-name → accessor maps once via reflection.</summary>
    static UserDataDiffBuilder()
    {
        Dictionary<string, Table> tables = [];
        MethodInfo createTable = typeof(UserDataDiffBuilder).GetMethod(nameof(CreateTable), BindingFlags.NonPublic | BindingFlags.Static)!;

        foreach (UserTable info in UserTables.All)
        {
            tables[info.Name] = (Table)createTable.MakeGenericMethod(info.RowType).Invoke(null, [info])!;
        }

        AllTables = tables;
        ClientTables = tables.Where(t => t.Value.Info.IsClientVisible).ToDictionary();
    }

    private static Table CreateTable<T>(UserTable info)
    {
        Func<DarkUserMemoryDatabase, List<T>> getRows = db => (List<T>)info.Property.GetValue(db)!;

        return new Table(
            info,
            db => JsonSerializer.Serialize(getRows(db), CamelCaseOptions),
            db =>
            {
                TableSnapshot snapshot = new();
                foreach (T row in getRows(db))
                {
                    snapshot.Add(EntityKey<T>.Of(row), JsonSerializer.Serialize(row, CamelCaseOptions), row);
                }
                return snapshot;
            },
            json => JsonSerializer.Deserialize<T>(json, CamelCaseOptions)!,
            [.. EntityKey<T>.Properties.Select(p => CamelCaseOptions.PropertyNamingPolicy!.ConvertName(p.Name))],
            (db, rowsJson) =>
            {
                // Same list instance, new row objects: the old ones may have been changed in place.
                List<T> rows = getRows(db);
                rows.Clear();
                rows.AddRange(rowsJson.Select(json => JsonSerializer.Deserialize<T>(json, CamelCaseOptions)!));
            });
    }

    /// <summary>
    /// Puts every table that differs from <paramref name="before"/> back the way it was (server-only tables included),
    /// rebuilding the rows from the snapshot's JSON. Used to undo a call that failed part-way. Returns the number of
    /// tables put back.
    /// </summary>
    public static int Restore(DarkUserMemoryDatabase db, Dictionary<string, TableSnapshot> before)
    {
        Dictionary<string, TableSnapshot> now = Snapshot(db);
        int restored = 0;
        foreach ((string name, Table table) in AllTables)
        {
            TableSnapshot beforeRows = before.TryGetValue(name, out TableSnapshot? b) ? b : TableSnapshot.Empty;
            TableSnapshot nowRows = now.TryGetValue(name, out TableSnapshot? n) ? n : TableSnapshot.Empty;
            if (nowRows.HasSameRowsAs(beforeRows))
            {
                continue;
            }

            table.ReplaceRows(db, beforeRows.RowsJson);
            restored++;
        }
        return restored;
    }

    /// <summary>All client-visible user table names (IUser* series, excludes EntityS* and EntityM*).</summary>
    public static IEnumerable<string> TableNames => ClientTables.Keys;

    /// <summary>
    /// Captures the current rows of every non-empty user table (server-only ones included).
    /// Use this before making changes; pass the result to <see cref="Delta"/> or <see cref="Changes"/> after.
    /// </summary>
    public static Dictionary<string, TableSnapshot> Snapshot(DarkUserMemoryDatabase db)
    {
        Dictionary<string, TableSnapshot> snapshot = [];
        foreach ((string name, Table table) in AllTables)
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
    /// Serializes a single named client table to a JSON array string.
    /// Returns "[]" if the table name is not recognised.
    /// </summary>
    public static string SerializeTable(DarkUserMemoryDatabase db, string tableName)
        => ClientTables.TryGetValue(tableName, out Table? table) ? table.Serialize(db) : "[]";

    /// <summary>
    /// Computes the client rows that changed since the snapshot, per table. New or changed rows are sent in
    /// <c>UpdateRecordsJson</c>; rows whose key no longer exists are sent in <c>DeleteKeysJson</c>, as their key fields only.
    /// Tables with neither are left out. Against an empty snapshot this is the full user state.
    /// </summary>
    public static Dictionary<string, DiffData> Delta(Dictionary<string, TableSnapshot> before, DarkUserMemoryDatabase db)
        => ClientDiff(before, Snapshot(db));

    /// <summary>Like <see cref="Delta"/>, between two snapshots.</summary>
    public static Dictionary<string, DiffData> ClientDiff(Dictionary<string, TableSnapshot> before, Dictionary<string, TableSnapshot> after)
    {
        Dictionary<string, DiffData> diff = [];
        foreach ((string name, Table table) in ClientTables)
        {
            TableSnapshot afterRows = after.TryGetValue(name, out TableSnapshot? a) ? a : TableSnapshot.Empty;
            TableSnapshot beforeRows = before.TryGetValue(name, out TableSnapshot? b) ? b : TableSnapshot.Empty;

            List<string> updated = [.. afterRows.RowsChangedFrom(beforeRows)];
            // The client builds a delete key from the key fields alone, so send nothing else.
            List<string> deleted = [.. beforeRows.RowsMissingFrom(afterRows).Select(json => KeyFieldsOnly(json, table.KeyNames))];

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

    /// <summary>
    /// Every row that changed between the snapshots, in every table, as row objects: new rows, changed rows
    /// (key already there before), and removed rows (rebuilt from their earlier JSON). A key that appears twice in a
    /// table counts once (its first row).
    /// </summary>
    public static UserChangeSet Changes(Dictionary<string, TableSnapshot> before, Dictionary<string, TableSnapshot> after)
    {
        List<TableChange> tables = [];
        foreach ((string name, Table table) in AllTables)
        {
            TableSnapshot afterRows = after.TryGetValue(name, out TableSnapshot? a) ? a : TableSnapshot.Empty;
            TableSnapshot beforeRows = before.TryGetValue(name, out TableSnapshot? b) ? b : TableSnapshot.Empty;

            List<object> added = [];
            List<object> modified = [];
            foreach ((object row, bool existed) in afterRows.ObjectsChangedFrom(beforeRows))
            {
                (existed ? modified : added).Add(row);
            }
            List<object> deleted = [.. beforeRows.JsonMissingFrom(afterRows).Select(table.Deserialize)];

            if (added.Count > 0 || modified.Count > 0 || deleted.Count > 0)
            {
                tables.Add(new TableChange(table.Info, added, modified, deleted));
            }
        }
        return new UserChangeSet(tables);
    }

    /// <summary>A row's JSON reduced to its primary-key properties, values copied as they were.</summary>
    private static string KeyFieldsOnly(string rowJson, string[] keyNames)
    {
        using JsonDocument row = JsonDocument.Parse(rowJson);
        using MemoryStream buffer = new();
        using (Utf8JsonWriter writer = new(buffer))
        {
            writer.WriteStartObject();
            foreach (string keyName in keyNames)
            {
                writer.WritePropertyName(keyName);
                row.RootElement.GetProperty(keyName).WriteTo(writer);
            }
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(buffer.ToArray());
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
/// primary key, in table order, with the row object itself when it was taken from a database.
/// </summary>
public sealed class TableSnapshot
{
    public static readonly TableSnapshot Empty = new();

    private readonly List<(object Key, string Json, object? Row)> _rows = [];
    private readonly Dictionary<object, string> _byKey = [];

    public int Count => _rows.Count;

    /// <summary>Each row's JSON, in table order.</summary>
    public IEnumerable<string> RowsJson => _rows.Select(row => row.Json);

    /// <summary>Whether both snapshots hold the same rows, with the same content, in the same order.</summary>
    public bool HasSameRowsAs(TableSnapshot other) => RowsJson.SequenceEqual(other.RowsJson);

    public void Add(object key, string json, object? row = null)
    {
        _rows.Add((key, json, row));
        _byKey.TryAdd(key, json);
    }

    /// <summary>JSON of the rows that are new or whose content differs from <paramref name="previous"/>.</summary>
    public IEnumerable<string> RowsChangedFrom(TableSnapshot previous)
        => _rows.Where(row => !previous._byKey.TryGetValue(row.Key, out string? json) || json != row.Json).Select(row => row.Json);

    /// <summary>JSON of the rows whose key doesn't exist in <paramref name="other"/>.</summary>
    public IEnumerable<string> RowsMissingFrom(TableSnapshot other)
        => _rows.Where(row => !other._byKey.ContainsKey(row.Key)).Select(row => row.Json);

    /// <summary>
    /// The row objects that are new or changed since <paramref name="previous"/>, each with whether its key was
    /// there before; only the first row of a repeated key.
    /// </summary>
    internal IEnumerable<(object Row, bool Existed)> ObjectsChangedFrom(TableSnapshot previous)
    {
        HashSet<object> seen = [];
        foreach ((object key, string json, object? row) in _rows)
        {
            if (row == null || !seen.Add(key))
            {
                continue;
            }

            bool existed = previous._byKey.TryGetValue(key, out string? oldJson);
            if (!existed || oldJson != json)
            {
                yield return (row, existed);
            }
        }
    }

    /// <summary>
    /// Each key (its first row) that is new, changed or gone since <paramref name="previous"/>, as its JSON before
    /// (null when new) and after (null when gone): new and changed rows in this table's order, then the gone ones.
    /// </summary>
    public IEnumerable<(string? Before, string? After)> CompareTo(TableSnapshot previous)
    {
        HashSet<object> seen = [];
        foreach ((object key, string json, _) in _rows)
        {
            if (!seen.Add(key))
            {
                continue;
            }

            if (!previous._byKey.TryGetValue(key, out string? oldJson))
            {
                yield return (null, json);
            }
            else if (oldJson != json)
            {
                yield return (oldJson, json);
            }
        }

        foreach ((object key, string json, _) in previous._rows)
        {
            if (!_byKey.ContainsKey(key) && seen.Add(key))
            {
                yield return (json, null);
            }
        }
    }

    /// <summary>The JSON of each key (its first row) that doesn't exist in <paramref name="other"/>.</summary>
    internal IEnumerable<string> JsonMissingFrom(TableSnapshot other)
        => _byKey.Where(entry => !other._byKey.ContainsKey(entry.Key)).Select(entry => entry.Value);
}

/// <summary>A user's tables before and after a request (<see cref="UserDataDiffBuilder.Snapshot"/>).</summary>
public sealed record UserChanges(long UserId, Dictionary<string, TableSnapshot> Before, Dictionary<string, TableSnapshot> After);

/// <summary>The rows of one user table that a request added, changed or removed.</summary>
public sealed record TableChange(UserTable Table, IReadOnlyList<object> Added, IReadOnlyList<object> Modified, IReadOnlyList<object> Deleted);

/// <summary>Every user-table row a request added, changed or removed.</summary>
public sealed record UserChangeSet(IReadOnlyList<TableChange> Tables)
{
    public static readonly UserChangeSet None = new([]);

    public bool IsEmpty => Tables.Count == 0;
}
