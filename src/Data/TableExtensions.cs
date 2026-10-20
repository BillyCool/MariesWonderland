using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MariesWonderland.Data;

/// <summary>
/// Primary-key lookups on entity tables, in the style of EF Core's <c>DbSet&lt;T&gt;.Find</c>:
/// <code>
/// EntityMQuestScene? scene = masterDb.EntityMQuestScene.Find(sceneId);
/// EntityMQuestMission? mission = masterDb.EntityMQuestMission.Find(questId, questMissionId);
/// EntityIUserCostume? costume = userDb.EntityIUserCostume.Find(userId, userCostumeUuid);
/// </code>
/// Tables marked read-only with <see cref="MarkReadOnly"/> (master data, after loading) are
/// indexed on first lookup. Other tables, such as per-user data, are scanned.
/// </summary>
public static class TableExtensions
{
    private static readonly ConditionalWeakTable<object, Lazy<object>> ReadOnlyIndexes = [];

    /// <summary>Returns the row with the given primary key, or null if there is none.</summary>
    public static T? Find<T>(this List<T> table, params object[] keyValues) where T : class
    {
        object key = EntityKey<T>.From(keyValues);

        if (ReadOnlyIndexes.TryGetValue(table, out Lazy<object>? index))
        {
            return ((FrozenDictionary<object, T>)index.Value).GetValueOrDefault(key);
        }

        foreach (T row in table)
        {
            if (EntityKey<T>.Of(row).Equals(key))
            {
                return row;
            }
        }
        return null;
    }

    /// <summary>
    /// Marks every table of <paramref name="masterDb"/> as read-only so <see cref="Find{T}"/> can
    /// index it. Call once after loading; the tables must not be modified afterwards.
    /// </summary>
    public static void MarkReadOnly(DarkMasterMemoryDatabase masterDb)
    {
        foreach (PropertyInfo property in typeof(DarkMasterMemoryDatabase).GetProperties())
        {
            if (property.GetValue(masterDb) is not { } table || !property.PropertyType.IsGenericType) continue;

            Type rowType = property.PropertyType.GetGenericArguments()[0];
            Func<object> build = IndexBuilders.GetOrAdd(rowType, CreateIndexBuilder)(table);
            ReadOnlyIndexes.AddOrUpdate(table, new Lazy<object>(build));
        }
    }

    private static readonly ConcurrentDictionary<Type, Func<object, Func<object>>> IndexBuilders = new();

    private static Func<object, Func<object>> CreateIndexBuilder(Type rowType)
    {
        MethodInfo method = typeof(TableExtensions)
            .GetMethod(nameof(BuildIndex), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(rowType);
        return table => () => method.Invoke(null, [table])!;
    }

    private static FrozenDictionary<object, T> BuildIndex<T>(List<T> table) where T : class
    {
        // Master data can contain repeated keys; the first row wins, as with a linear scan.
        Dictionary<object, T> index = new(table.Count);
        foreach (T row in table)
        {
            index.TryAdd(EntityKey<T>.Of(row), row);
        }
        return index.ToFrozenDictionary();
    }
}
