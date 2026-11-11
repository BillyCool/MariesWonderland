using MariesWonderland.Data;
using MariesWonderland.MasterMemory;
using MessagePack;
using System.Collections;
using System.Reflection;

namespace MariesWonderland.MasterEditing;

/// <summary>One column of a master table: a property with its MessagePack key, in file order.</summary>
public sealed record MasterColumn(string Name, int Index, PropertyInfo Property, bool IsKey)
{
    public Type Type => Property.PropertyType;

    /// <summary>Unix-millisecond date columns (<c>…Datetime</c>), shown as dates.</summary>
    public bool IsDate => Type == typeof(long) && Name.EndsWith("Datetime", StringComparison.Ordinal);

    public object? Get(object row) => Property.GetValue(row);

    public void Set(object row, object? value) => Property.SetValue(row, value);
}

/// <summary>A master table: its name in the file (<c>m_pvp_season</c>), entity type, list property and columns.</summary>
public sealed record MasterTable(string Name, string FileName, Type RowType, PropertyInfo Property, IReadOnlyList<MasterColumn> Columns)
{
    public IReadOnlyList<MasterColumn> KeyColumns { get; } = [.. Columns.Where(c => c.IsKey)];

    /// <summary>The rows of this table in <paramref name="db"/>.</summary>
    public IList Rows(DarkMasterMemoryDatabase db) => (IList)Property.GetValue(db)!;

    /// <summary>The primary key of a row, as text ("202040" or "801/6").</summary>
    public string KeyOf(object row) => string.Join('/', KeyColumns.Select(c => Convert.ToString(c.Get(row), System.Globalization.CultureInfo.InvariantCulture)));

    /// <summary>A deep copy of a row (MessagePack round trip).</summary>
    public object Copy(object row)
        => MessagePackSerializer.Deserialize(RowType, MessagePackSerializer.Serialize(RowType, row, MasterTables.Options), MasterTables.Options)!;

    public object NewRow() => Activator.CreateInstance(RowType)!;
}

/// <summary>Every master table, found once by reflection over <see cref="DarkMasterMemoryDatabase"/>.</summary>
public static class MasterTables
{
    public static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard;

    public static IReadOnlyList<MasterTable> All { get; } = Discover();

    private static readonly Dictionary<string, MasterTable> ByName = All.ToDictionary(t => t.Name);

    public static MasterTable? Find(string name) => ByName.GetValueOrDefault(name);

    public static MasterTable Get<T>() => ByName[typeof(T).Name];

    private static MasterTable[] Discover()
    {
        List<MasterTable> tables = [];
        foreach (PropertyInfo prop in typeof(DarkMasterMemoryDatabase).GetProperties())
        {
            if (!prop.PropertyType.IsGenericType || prop.PropertyType.GetGenericTypeDefinition() != typeof(List<>))
            {
                continue;
            }

            Type rowType = prop.PropertyType.GetGenericArguments()[0];
            MemoryTableAttribute? table = rowType.GetCustomAttribute<MemoryTableAttribute>();
            if (table == null)
            {
                continue;
            }

            HashSet<string> keys = [.. rowType.GetCustomAttribute<Microsoft.EntityFrameworkCore.PrimaryKeyAttribute>()?.PropertyNames ?? []];
            List<MasterColumn> columns = [.. rowType.GetProperties()
                .Select(p => (Property: p, Key: p.GetCustomAttribute<KeyAttribute>()))
                .Where(p => p.Key?.IntKey != null)
                .OrderBy(p => p.Key!.IntKey)
                .Select(p => new MasterColumn(p.Property.Name, p.Key!.IntKey!.Value, p.Property, keys.Contains(p.Property.Name)))];
            tables.Add(new MasterTable(rowType.Name, table.TableName, rowType, prop, columns));
        }
        return [.. tables.OrderBy(t => t.Name, StringComparer.Ordinal)];
    }
}
