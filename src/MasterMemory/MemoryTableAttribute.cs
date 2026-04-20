using System.Text;

namespace MariesWonderland.MasterMemory;

/// <summary>
/// Maps an EntityM* class name to its binary table key used in the master database header.
/// The table name is derived by converting the class name to snake_case and dropping the
/// leading "entity_" prefix (e.g. "EntityMCharacter" → "m_character").
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class MemoryTableAttribute : Attribute
{
    /// <summary>The table key as it appears in the binary database header.</summary>
    public string TableName { get; }

    public MemoryTableAttribute(string tableName)
    {
        // "EntityMCharacter" → "entity_m_character" → split once on '_' → "m_character"
        TableName = ToSnakeCase(tableName).Split('_', 2)[1];
    }

    private static string ToSnakeCase(string value)
    {
        var sb = new StringBuilder(value.Length + 10);
        for (int i = 0; i < value.Length; i++)
        {
            if (i > 0 && char.IsUpper(value[i]))
                sb.Append('_');
            sb.Append(char.ToLowerInvariant(value[i]));
        }
        return sb.ToString();
    }
}
