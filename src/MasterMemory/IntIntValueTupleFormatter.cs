using MessagePack;
using MessagePack.Formatters;

namespace MariesWonderland.MasterMemory;

/// <summary>
/// MessagePack formatter for <c>(int, int)</c> value tuples stored as 2-element arrays
/// in the master database binary header.
/// </summary>
internal sealed class IntIntValueTupleFormatter : IMessagePackFormatter<(int, int)>
{
    public void Serialize(ref MessagePackWriter writer, (int, int) value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        writer.WriteInt32(value.Item1);
        writer.WriteInt32(value.Item2);
    }

    public (int, int) Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.IsNil)
            throw new InvalidOperationException("Header tuple entry is nil.");

        var count = reader.ReadArrayHeader();
        if (count != 2)
            throw new InvalidOperationException($"Expected 2-element tuple, got {count}.");

        return (reader.ReadInt32(), reader.ReadInt32());
    }
}
