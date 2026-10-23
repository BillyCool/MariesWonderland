using MariesWonderland.Models.Entities;
using System.Reflection;

namespace MariesWonderland.Data;

/// <summary>
/// One table of a <see cref="DarkUserMemoryDatabase"/>: its list property, row type and name
/// ("IUserWeapon" for <c>EntityIUserWeapon</c>, "SUserGift" for <c>EntitySUserGift</c>).
/// Client-visible tables are the EntityI* ones; EntityS* tables are server-only.
/// </summary>
public sealed record UserTable(string Name, PropertyInfo Property, Type RowType)
{
    public bool IsClientVisible => RowType.Name.StartsWith("EntityI", StringComparison.Ordinal);
}

/// <summary>Every user table, found once by reflection: the <c>List&lt;T&gt;</c> properties whose rows are user entities.</summary>
public static class UserTables
{
    public static IReadOnlyList<UserTable> All { get; } = Discover();

    private static UserTable[] Discover()
    {
        List<UserTable> tables = [];
        foreach (PropertyInfo prop in typeof(DarkUserMemoryDatabase).GetProperties())
        {
            if (!prop.PropertyType.IsGenericType || prop.PropertyType.GetGenericTypeDefinition() != typeof(List<>))
            {
                continue;
            }

            Type rowType = prop.PropertyType.GetGenericArguments()[0];
            if (!typeof(IUserEntity).IsAssignableFrom(rowType) || !rowType.Name.StartsWith("Entity", StringComparison.Ordinal))
            {
                continue;
            }

            tables.Add(new UserTable(rowType.Name["Entity".Length..], prop, rowType));
        }
        return [.. tables];
    }
}
