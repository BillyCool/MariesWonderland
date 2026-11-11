using MessagePack;
using System.Buffers;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace MariesWonderland.MasterEditing;

/// <summary>
/// The master data <c>.bin.e</c> file as raw tables: AES-128-CBC over a MessagePack header (table name →
/// [offset, length]) followed by the tables, each an Lz4Block MessagePack array of rows. Rewriting keeps the header's
/// key order and the tables' file order, and copies every table that isn't replaced byte for byte.
/// </summary>
public sealed class MasterDataFile
{
    private static readonly byte[] AesKey = Encoding.UTF8.GetBytes("6Cb01321EE5e6bBe");
    private static readonly byte[] AesIv = Encoding.UTF8.GetBytes("EfcAef4CAe5f6DaA");
    private static readonly MessagePackSerializerOptions Lz4 = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);

    private readonly List<(string Name, int Offset, int Length)> _header;
    private readonly byte[] _data;

    private MasterDataFile(List<(string Name, int Offset, int Length)> header, byte[] data)
    {
        _header = header;
        _data = data;
    }

    public IReadOnlyList<string> TableNames => [.. _header.Select(h => h.Name)];

    public static MasterDataFile Read(string path) => Parse(File.ReadAllBytes(path));

    public static MasterDataFile Parse(byte[] encrypted)
    {
        byte[] plain = Crypt(encrypted, encrypt: false);
        MessagePackReader reader = new(plain);
        int count = reader.ReadMapHeader();
        List<(string, int, int)> header = new(count);
        for (int i = 0; i < count; i++)
        {
            string name = reader.ReadString()!;
            reader.ReadArrayHeader();
            int offset = reader.ReadInt32();
            int length = reader.ReadInt32();
            header.Add((name, offset, length));
        }
        return new MasterDataFile(header, plain[(int)reader.Consumed..]);
    }

    /// <summary>The raw (compressed) bytes of one table.</summary>
    public ReadOnlyMemory<byte> TableBytes(string fileName)
    {
        (string _, int offset, int length) = _header.Single(h => h.Name == fileName);
        return _data.AsMemory(offset, length);
    }

    /// <summary>Serializes typed rows the way the file stores them.</summary>
    public static byte[] Serialize(MasterTable table, IList rows)
    {
        Array array = Array.CreateInstance(table.RowType, rows.Count);
        rows.CopyTo(array, 0);
        return MessagePackSerializer.Serialize(array.GetType(), array, Lz4);
    }

    /// <summary>The whole file, encrypted, with the given tables replaced (by file name, e.g. <c>m_pvp_season</c>).</summary>
    public byte[] Write(IReadOnlyDictionary<string, byte[]> replaced)
    {
        foreach (string name in replaced.Keys)
        {
            if (!_header.Any(h => h.Name == name))
            {
                throw new InvalidOperationException($"The master data file has no table {name}; adding tables isn't supported.");
            }
        }

        using MemoryStream body = new();
        Dictionary<string, (int Offset, int Length)> positions = [];
        foreach ((string name, int offset, int length) in _header.OrderBy(h => h.Offset))
        {
            ReadOnlySpan<byte> bytes = replaced.TryGetValue(name, out byte[]? b) ? b : _data.AsSpan(offset, length);
            positions[name] = ((int)body.Position, bytes.Length);
            body.Write(bytes);
        }

        ArrayBufferWriter<byte> headerBuffer = new();
        MessagePackWriter writer = new(headerBuffer);
        writer.WriteMapHeader(_header.Count);
        foreach ((string name, int _, int _) in _header)
        {
            writer.Write(name);
            writer.WriteArrayHeader(2);
            writer.Write(positions[name].Offset);
            writer.Write(positions[name].Length);
        }
        writer.Flush();

        byte[] plain = [.. headerBuffer.WrittenSpan, .. body.ToArray()];
        return Crypt(plain, encrypt: true);
    }

    private static byte[] Crypt(byte[] data, bool encrypt)
    {
        using Aes aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = AesKey;
        aes.IV = AesIv;
        using ICryptoTransform transform = encrypt ? aes.CreateEncryptor() : aes.CreateDecryptor();
        return transform.TransformFinalBlock(data, 0, data.Length);
    }
}
