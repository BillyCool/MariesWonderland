using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MariesWonderland.Data;

/// <summary>
/// Primary-key metadata for an entity type, read once from its <see cref="PrimaryKeyAttribute"/>.
/// Keys are boxed: a single-column key is the property value itself, a composite key is a
/// <see cref="CompositeKey"/>. Used by table lookups and anything else that needs to identify rows.
/// </summary>
public static class EntityKey<T>
{
    /// <summary>The key properties, in declaration order of the attribute.</summary>
    public static IReadOnlyList<PropertyInfo> Properties { get; } = ResolveProperties();

    /// <summary>Returns the key of <paramref name="entity"/>.</summary>
    public static object Of(T entity)
    {
        if (Properties.Count == 1)
            return Properties[0].GetValue(entity)!;

        object[] parts = new object[Properties.Count];
        for (int i = 0; i < parts.Length; i++)
            parts[i] = Properties[i].GetValue(entity)!;
        return new CompositeKey(parts);
    }

    /// <summary>
    /// Builds a key from caller-supplied values, converting each to its property type
    /// (e.g. an <c>int</c> literal for a <c>long</c> or enum column).
    /// </summary>
    public static object From(object[] values)
    {
        if (values.Length != Properties.Count)
            throw new ArgumentException($"{typeof(T).Name} has a {Properties.Count}-part key but {values.Length} values were given.");

        if (values.Length == 1)
            return Convert(values[0], Properties[0].PropertyType);

        object[] parts = new object[values.Length];
        for (int i = 0; i < parts.Length; i++)
            parts[i] = Convert(values[i], Properties[i].PropertyType);
        return new CompositeKey(parts);
    }

    private static object Convert(object value, Type type)
    {
        if (value.GetType() == type)
            return value;
        return type.IsEnum
            ? Enum.ToObject(type, value)
            : System.Convert.ChangeType(value, type);
    }

    private static PropertyInfo[] ResolveProperties()
    {
        PrimaryKeyAttribute attribute = typeof(T).GetCustomAttribute<PrimaryKeyAttribute>()
            ?? throw new InvalidOperationException($"{typeof(T).Name} has no [PrimaryKey] attribute.");

        return [.. attribute.PropertyNames.Select(name => typeof(T).GetProperty(name)
            ?? throw new InvalidOperationException($"{typeof(T).Name} has no key property '{name}'."))];
    }
}

/// <summary>A multi-column key with structural equality.</summary>
public readonly struct CompositeKey(object[] parts) : IEquatable<CompositeKey>
{
    private readonly object[] _parts = parts;

    public bool Equals(CompositeKey other) => _parts.AsSpan().SequenceEqual(other._parts);

    public override bool Equals(object? obj) => obj is CompositeKey other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (object part in _parts)
            hash.Add(part);
        return hash.ToHashCode();
    }

    public override string ToString() => $"({string.Join(", ", _parts)})";
}
