using MariesWonderland.Data;
using System.Collections;
using System.Text.Json;

namespace MariesWonderland.MasterEditing;

public enum MasterChangeKind
{
    Added,
    Removed,
    Modified,
}

/// <summary>One row changed by a draft: its table, key and the row as JSON before and after.</summary>
public sealed record MasterRowChange(string Table, string Key, MasterChangeKind Kind, string? Before, string? After);

/// <summary>
/// The server's one pending set of master data edits, shared by every administrator. Tables are copied from the live
/// data on their first edit; publishing (<see cref="MasterDataPublisher"/>) writes them to the file. Thread-safe.
/// </summary>
public sealed class MasterDraft(MasterDataHolder holder)
{
    private readonly Lock _lock = new();
    private readonly Dictionary<string, List<object>> _tables = [];
    private DarkMasterMemoryDatabase? _base;

    /// <summary>Raised after every change (and after discard or publish), for open pages to refresh.</summary>
    public event Action? Changed;

    /// <summary>The live data the draft was started from; null while the draft is empty.</summary>
    public DarkMasterMemoryDatabase? Base
    {
        get
        {
            lock (_lock)
            {
                return _base;
            }
        }
    }

    /// <summary>Whether the live data was replaced (a publish, a reload) after this draft started.</summary>
    public bool IsStale
    {
        get
        {
            lock (_lock)
            {
                return _base != null && !ReferenceEquals(_base, holder.Current.Database);
            }
        }
    }

    public IReadOnlyCollection<string> EditedTables
    {
        get
        {
            lock (_lock)
            {
                return [.. _tables.Keys];
            }
        }
    }

    public bool IsEdited(MasterTable table)
    {
        lock (_lock)
        {
            return _tables.ContainsKey(table.Name);
        }
    }

    /// <summary>The rows as the draft has them: the edited copy, or the live rows when the table is untouched.</summary>
    public IReadOnlyList<object> Rows(MasterTable table)
    {
        lock (_lock)
        {
            return _tables.TryGetValue(table.Name, out List<object>? rows)
                ? [.. rows]
                : [.. table.Rows(holder.Current.Database).Cast<object>()];
        }
    }

    /// <summary>The draft's row with this key, or null.</summary>
    public object? Find(MasterTable table, string key) => Rows(table).FirstOrDefault(r => table.KeyOf(r) == key);

    /// <summary>
    /// Replaces the row with key <paramref name="originalKey"/> (a copy of <paramref name="row"/> goes in). Fails when
    /// the row's key changed to one another row already has.
    /// </summary>
    public void Update(MasterTable table, string originalKey, object row)
    {
        lock (_lock)
        {
            List<object> rows = Editable(table);
            int index = rows.FindIndex(r => table.KeyOf(r) == originalKey);
            if (index < 0)
            {
                throw new InvalidOperationException($"{table.Name} has no row {originalKey}.");
            }

            string newKey = table.KeyOf(row);
            if (newKey != originalKey && rows.Any(r => table.KeyOf(r) == newKey))
            {
                throw new InvalidOperationException($"{table.Name} already has a row {newKey}.");
            }
            rows[index] = table.Copy(row);
        }
        Changed?.Invoke();
    }

    /// <summary>Adds rows; fails (adding none) if any key is taken.</summary>
    public void Add(MasterTable table, IEnumerable<object> newRows)
    {
        lock (_lock)
        {
            List<object> rows = Editable(table);
            HashSet<string> keys = [.. rows.Select(table.KeyOf)];
            List<object> copies = [.. newRows.Select(table.Copy)];
            foreach (object row in copies)
            {
                if (!keys.Add(table.KeyOf(row)))
                {
                    throw new InvalidOperationException($"{table.Name} already has a row {table.KeyOf(row)}.");
                }
            }
            rows.AddRange(copies);
        }
        Changed?.Invoke();
    }

    public void Delete(MasterTable table, IEnumerable<string> keys)
    {
        HashSet<string> remove = [.. keys];
        lock (_lock)
        {
            Editable(table).RemoveAll(r => remove.Contains(table.KeyOf(r)));
        }
        Changed?.Invoke();
    }

    /// <summary>Changes every row matching <paramref name="match"/> in place (on copies).</summary>
    public int Modify(MasterTable table, Func<object, bool> match, Action<object> change)
    {
        int count = 0;
        lock (_lock)
        {
            List<object> rows = Editable(table);
            for (int i = 0; i < rows.Count; i++)
            {
                if (match(rows[i]))
                {
                    object copy = table.Copy(rows[i]);
                    change(copy);
                    rows[i] = copy;
                    count++;
                }
            }
        }
        Changed?.Invoke();
        return count;
    }

    /// <summary>Puts one table back to the live data.</summary>
    public void Revert(MasterTable table)
    {
        lock (_lock)
        {
            _tables.Remove(table.Name);
            if (_tables.Count == 0)
            {
                _base = null;
            }
        }
        Changed?.Invoke();
    }

    public void Discard()
    {
        lock (_lock)
        {
            _tables.Clear();
            _base = null;
        }
        Changed?.Invoke();
    }

    /// <summary>The draft's tables, for publishing: name → rows sorted by primary key.</summary>
    public IReadOnlyDictionary<MasterTable, IList> Snapshot()
    {
        lock (_lock)
        {
            return _tables.ToDictionary(t => MasterTables.Find(t.Key)!, t => (IList)SortByKey(MasterTables.Find(t.Key)!, t.Value));
        }
    }

    /// <summary>Every row the draft adds, removes or changes, compared with the data it started from.</summary>
    public IReadOnlyList<MasterRowChange> Changes()
    {
        lock (_lock)
        {
            if (_base == null)
            {
                return [];
            }

            List<MasterRowChange> changes = [];
            foreach ((string name, List<object> rows) in _tables)
            {
                MasterTable table = MasterTables.Find(name)!;
                changes.AddRange(Compare(table, table.Rows(_base).Cast<object>(), rows));
            }
            return changes;
        }
    }

    /// <summary>The row changes between two versions of a table.</summary>
    public static IEnumerable<MasterRowChange> Compare(MasterTable table, IEnumerable<object> before, IEnumerable<object> after)
    {
        Dictionary<string, object> old = [];
        foreach (object row in before)
        {
            old.TryAdd(table.KeyOf(row), row);
        }
        HashSet<string> seen = [];
        foreach (object row in after)
        {
            string key = table.KeyOf(row);
            seen.Add(key);
            string json = Json(table, row);
            if (!old.TryGetValue(key, out object? previous))
            {
                yield return new MasterRowChange(table.Name, key, MasterChangeKind.Added, null, json);
            }
            else
            {
                string previousJson = Json(table, previous);
                if (previousJson != json)
                {
                    yield return new MasterRowChange(table.Name, key, MasterChangeKind.Modified, previousJson, json);
                }
            }
        }
        foreach ((string key, object row) in old)
        {
            if (!seen.Contains(key))
            {
                yield return new MasterRowChange(table.Name, key, MasterChangeKind.Removed, Json(table, row), null);
            }
        }
    }

    /// <summary>
    /// Starts (or adds to) the draft with the opposite of a published edit: removed rows come back, added ones go,
    /// changed ones get their old values. Rows changed again since are overwritten with the old values too.
    /// </summary>
    public void ApplyInverse(IEnumerable<MasterRowChange> changes)
    {
        lock (_lock)
        {
            foreach (IGrouping<string, MasterRowChange> group in changes.GroupBy(c => c.Table))
            {
                MasterTable table = MasterTables.Find(group.Key) ?? throw new InvalidOperationException($"Unknown table {group.Key}.");
                List<object> rows = Editable(table);
                foreach (MasterRowChange change in group)
                {
                    rows.RemoveAll(r => table.KeyOf(r) == change.Key);
                    if (change.Before != null)
                    {
                        rows.Add(JsonSerializer.Deserialize(change.Before, table.RowType)!);
                    }
                }
            }
        }
        Changed?.Invoke();
    }

    public static string Json(MasterTable table, object row) => JsonSerializer.Serialize(row, table.RowType);

    private List<object> Editable(MasterTable table)
    {
        _base ??= holder.Current.Database;
        if (!_tables.TryGetValue(table.Name, out List<object>? rows))
        {
            rows = [.. table.Rows(_base).Cast<object>()];
            _tables[table.Name] = rows;
        }
        return rows;
    }

    /// <summary>Rows sorted by their primary key columns (numbers as numbers), as the file keeps them.</summary>
    public static List<object> SortByKey(MasterTable table, IEnumerable<object> rows)
    {
        if (table.KeyColumns.Count == 0)
        {
            return [.. rows];
        }

        IOrderedEnumerable<object>? ordered = null;
        foreach (MasterColumn column in table.KeyColumns)
        {
            Func<object, object?> key = r => column.Get(r) is Enum e ? Convert.ToInt64(e) : column.Get(r);
            ordered = ordered == null ? rows.OrderBy(key, Comparer<object?>.Default) : ordered.ThenBy(key, Comparer<object?>.Default);
        }
        return [.. ordered!];
    }
}
