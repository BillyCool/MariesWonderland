using MessagePack;
using MessagePack.Formatters;

namespace MariesWonderland.MasterMemory;

/// <summary>
/// MessagePack resolver used exclusively to deserialize the binary database header
/// (<c>Dictionary&lt;string, (int, int)&gt;</c> mapping table names to data offsets).
/// </summary>
internal sealed class HeaderFormatterResolver : IFormatterResolver
{
    public static readonly IFormatterResolver Instance = new HeaderFormatterResolver();

    /// <summary>Pre-configured options using this resolver (no compression for header).</summary>
    public static readonly MessagePackSerializerOptions StandardOptions =
        MessagePackSerializerOptions.Standard.WithResolver(Instance);

    private HeaderFormatterResolver() { }

    /// <inheritdoc/>
    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        if (typeof(T) == typeof(Dictionary<string, (int, int)>))
            return (IMessagePackFormatter<T>)(object)new DictionaryFormatter<string, (int, int)>();

        if (typeof(T) == typeof(string))
            return (IMessagePackFormatter<T>)(object)NullableStringFormatter.Instance;

        if (typeof(T) == typeof((int, int)))
            return (IMessagePackFormatter<T>)(object)new IntIntValueTupleFormatter();

        if (typeof(T) == typeof(int))
            return (IMessagePackFormatter<T>)(object)Int32Formatter.Instance;

        return null;
    }
}
