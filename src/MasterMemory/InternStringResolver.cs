using MessagePack;
using MessagePack.Formatters;

namespace MariesWonderland.MasterMemory;

/// <summary>
/// MessagePack resolver that interns deserialized strings to reduce memory usage from
/// repeated values (e.g. asset paths that appear across many master data records).
/// </summary>
internal sealed class InternStringResolver : IFormatterResolver, IMessagePackFormatter<string>
{
    private readonly IFormatterResolver _inner;

    public InternStringResolver(IFormatterResolver inner) => _inner = inner;

    /// <inheritdoc/>
    public IMessagePackFormatter<T>? GetFormatter<T>() => _inner.GetFormatter<T>();

    /// <inheritdoc/>
    public void Serialize(ref MessagePackWriter writer, string value, MessagePackSerializerOptions options)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public string? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var value = reader.ReadString();
        return value is not null ? string.Intern(value) : value;
    }
}
