using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MariesWonderland.Http;

/// <summary>
/// Finds the files of the asset dump (<c>Server:Paths:AssetDatabase</c>, a folder of numbered revisions) that answer
/// asset requests.
///
/// <para>
/// Each revision's list.bin lists the assets changed after that revision, always at their newest version, and only
/// revision 0's folder holds files. Revision 0's list is therefore the whole catalogue (every entry of every other
/// list is in it with the same MD5), so every request is looked up there, whichever list the client fetched.
/// </para>
///
/// <para>
/// A few files aren't on disk under their listed name: names the dump tool mangled (UTF-8 read as Latin-1), and files
/// stored once under another name, which revision 0's info.json maps (<c>from-name</c> → <c>to-name</c>).
/// </para>
/// </summary>
public sealed class AssetDatabase
{
    /// <summary>The revision whose list is the whole catalogue and whose folder holds the files.</summary>
    public const string CatalogueRevision = "0";

    private readonly string _basePath;
    private readonly ILogger<AssetDatabase> _logger;
    private readonly Lazy<Dictionary<string, ListBinEntry>> _index;
    private readonly Lazy<Dictionary<string, InfoAlias>> _aliases;

    // path → (size, modTimeUtcTicks, md5Hex)
    private readonly ConcurrentDictionary<string, (long Size, long ModTimeTicks, string Md5)> _md5Cache = new();

    public AssetDatabase(string basePath, ILogger<AssetDatabase> logger)
    {
        _basePath = string.IsNullOrEmpty(basePath) ? "" : Path.GetFullPath(basePath);
        _logger = logger;
        _index = new Lazy<Dictionary<string, ListBinEntry>>(LoadIndex);
        _aliases = new Lazy<Dictionary<string, InfoAlias>>(LoadAliases);
    }

    /// <summary>Whether <paramref name="revision"/> is a revision number (digits only), so it can name a folder.</summary>
    public static bool IsRevision(string revision) => revision.Length is > 0 and <= 9 && revision.All(char.IsAsciiDigit);

    /// <summary>The path of a revision's list.bin, or null when <paramref name="revision"/> isn't a revision number or no dump is set.</summary>
    public string? ListPath(string revision)
        => _basePath.Length > 0 && IsRevision(revision) ? Path.Combine(_basePath, revision, "list.bin") : null;

    /// <summary>Loads the catalogue now, so the first asset request doesn't wait for it. Returns its entry count.</summary>
    public int Prewarm()
    {
        int count = _index.Value.Count;
        _ = _aliases.Value;
        return count;
    }

    /// <summary>
    /// Looks up an asset request: the catalogue entry for <paramref name="objectId"/> and the files that may hold
    /// it, best first. Null for an unknown object or a type other than <c>assetbundle</c> or <c>resources</c>.
    /// The caller serves the first file whose size and MD5 match the entry.
    /// </summary>
    public AssetLookup? Find(string assetType, string objectId)
    {
        if (assetType is not ("assetbundle" or "resources") || !_index.Value.TryGetValue(objectId, out ListBinEntry? entry))
        {
            return null;
        }

        List<string> paths = [];
        AddPath(paths, assetType, CatalogueRevision, entry.Path);
        if (entry.Path.Any(c => c >= '\x80'))
        {
            AddPath(paths, assetType, CatalogueRevision, Utf8ToMojibake(entry.Path));
        }

        if (_aliases.Value.TryGetValue(entry.Path, out InfoAlias? alias))
        {
            AddPath(paths, assetType, alias.ToRevision, alias.ToName);
        }

        return new AssetLookup(entry, paths);
    }

    /// <summary>Adds the file path of a listed name (')' separates folders), skipping names that would leave the dump.</summary>
    private void AddPath(List<string> paths, string assetType, string revision, string name)
    {
        string relative = name.Replace(')', '/');
        if (relative.Split('/', '\\').Contains("..") || Path.IsPathRooted(relative))
        {
            return;
        }

        string path = assetType == "assetbundle"
            ? Path.Combine(_basePath, revision, "assetbundle", relative + ".assetbundle")
            : Path.Combine(_basePath, revision, "resources", relative);
        if (!paths.Contains(path))
        {
            paths.Add(path);
        }
    }

    // Each UTF-8 byte read as a Latin-1 character: how some extraction tools wrote non-ASCII file names.
    private static string Utf8ToMojibake(string s) => new([.. Encoding.UTF8.GetBytes(s).Select(b => (char)b)]);

    private Dictionary<string, ListBinEntry> LoadIndex()
    {
        string? path = ListPath(CatalogueRevision);
        if (path == null || !File.Exists(path))
        {
            _logger.LogWarning("No asset catalogue at {Path} (Server:Paths:AssetDatabase); every asset request will be answered with 404", path ?? "(not set)");
            return [];
        }

        try
        {
            Dictionary<string, ListBinEntry> index = ListBinParser.Parse(File.ReadAllBytes(path));
            _logger.LogInformation("Asset catalogue loaded: {Count} assets from {Path}", index.Count, path);
            return index;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reading the asset catalogue {Path} failed; every asset request will be answered with 404", path);
            return [];
        }
    }

    private Dictionary<string, InfoAlias> LoadAliases()
    {
        Dictionary<string, InfoAlias> aliases = [];
        if (_basePath.Length == 0)
        {
            return aliases;
        }

        string path = Path.Combine(_basePath, CatalogueRevision, "info.json");
        if (!File.Exists(path))
        {
            return aliases;
        }

        try
        {
            InfoJsonEntry[] entries = JsonSerializer.Deserialize<InfoJsonEntry[]>(File.ReadAllText(path)) ?? [];
            foreach (InfoJsonEntry e in entries)
            {
                string revision = e.ToRevision?.ToString() ?? CatalogueRevision;
                if (!string.IsNullOrEmpty(e.FromName) && !string.IsNullOrEmpty(e.ToName) && IsRevision(revision))
                {
                    aliases[e.FromName] = new InfoAlias(e.ToName, revision);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Reading the asset aliases {Path} failed; aliased assets will be answered with 404", path);
        }

        return aliases;
    }

    /// <summary>
    /// The MD5 hex digest of a file, read as a stream and cached while its size and write time stay the same.
    /// Null when it can't be read.
    /// </summary>
    public string? ComputeMd5(string filePath, FileInfo info)
    {
        long modTicks = info.LastWriteTimeUtc.Ticks;
        if (_md5Cache.TryGetValue(filePath, out (long Size, long ModTimeTicks, string Md5) cached)
            && cached.Size == info.Length && cached.ModTimeTicks == modTicks)
        {
            return cached.Md5;
        }

        try
        {
            using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.SequentialScan);
            string hex = Convert.ToHexStringLower(MD5.HashData(stream));
            _md5Cache[filePath] = (info.Length, modTicks, hex);
            return hex;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to compute MD5 for {FilePath}", filePath);
            return null;
        }
    }
}

public record ListBinEntry(string Path, long Size, string MD5);

/// <summary>A catalogue entry and the files that may hold it, best first.</summary>
public record AssetLookup(ListBinEntry Entry, IReadOnlyList<string> Paths);

public record InfoAlias(string ToName, string ToRevision);

public sealed class InfoJsonEntry
{
    [JsonPropertyName("from-name")] public string FromName { get; init; } = "";
    [JsonPropertyName("to-name")] public string ToName { get; init; } = "";
    [JsonPropertyName("to-revision")] public int? ToRevision { get; init; }
}

/// <summary>
/// Parses the Octo asset management list.bin binary into a dictionary of objectId → asset entry.
///
/// <para>
/// <b>list.bin outer format</b> — a protobuf message with mixed fields:
/// <list type="bullet">
///   <item>Field 1 (varint): revision number (header metadata, skipped)</item>
///   <item>Field 2 (repeated, length-delimited): one entry per asset</item>
/// </list>
/// The outer loop treats every length-delimited field as a potential entry regardless of field
/// number, skipping non-length-delimited fields.
/// </para>
///
/// <para>
/// <b>Entry inner format</b>:
/// <list type="bullet">
///   <item>Field 1  (varint):  category index (ignored)</item>
///   <item>Field 3  (string):  path, using <c>)</c> as directory separator</item>
///   <item>Field 4  (varint):  file size in bytes</item>
///   <item>Field 5  (varint):  CRC / hash (ignored)</item>
///   <item>Field 6  (varint):  unknown (ignored)</item>
///   <item>Field 9  (varint):  asset type index (ignored)</item>
///   <item>Field 10 (string):  MD5 hex digest</item>
///   <item>Field 11 (string):  objectId — 6-byte ASCII key used in asset request URLs</item>
///   <item>Field 12 (varint):  timestamp (8-byte varint — <b>requires reading up to 10 varint bytes</b>)</item>
///   <item>Field 13 (varint):  revision number (ignored)</item>
/// </list>
/// </para>
///
/// <para>
/// <b>Important</b>: varints in list.bin can be up to 8 bytes (field 12 carries a Unix timestamp
/// in milliseconds). The reader must not bail out after 5 bytes (35-bit limit) like a naive
/// int32 varint reader would — it must continue reading up to 10 bytes, discarding high bits
/// that do not fit in int32, as the source format uses 64-bit integers.
/// </para>
/// </summary>
internal static class ListBinParser
{
    /// <summary>
    /// Parses the list.bin binary data into a dictionary mapping objectId to asset entry.
    /// </summary>
    public static Dictionary<string, ListBinEntry> Parse(ReadOnlySpan<byte> data)
    {
        Dictionary<string, ListBinEntry> idx = [];
        int pos = 0;

        while (pos < data.Length)
        {
            if (!TryReadVarint(data, ref pos, out int tag)) break;
            int wireType = tag & 0x7;

            if (wireType == 2)
            {
                if (!TryReadVarint(data, ref pos, out int length) || length < 0 || pos + length > data.Length)
                    break;

                // Always advance past this field, whether or not the entry parses successfully.
                int entryStart = pos;
                if (TryParseEntry(data.Slice(pos, length), out string? objectId, out ListBinEntry entry) && objectId != null)
                    idx[objectId] = entry;

                pos = entryStart + length;
            }
            else
            {
                // Skip varint / fixed-width outer fields (e.g. field 1 = revision header).
                if (!TrySkipField(wireType, data, ref pos)) break;
            }
        }

        return idx;
    }

    private static bool TryParseEntry(ReadOnlySpan<byte> data, out string? objectId, out ListBinEntry entry)
    {
        objectId = null;
        string path = "";
        long size = 0;
        string md5 = "";
        int pos = 0;

        while (pos < data.Length)
        {
            if (!TryReadVarint(data, ref pos, out int tag)) { entry = default!; return false; }
            int fieldNum = tag >> 3;
            int wireType = tag & 0x7;

            switch (fieldNum)
            {
                case 3: // path
                    if (wireType != 2 || !TryReadString(data, ref pos, out path)) { entry = default!; return false; }
                    break;
                case 4: // size (varint)
                    if (wireType != 0 || !TryReadVarint(data, ref pos, out int sz)) { entry = default!; return false; }
                    if (sz >= 256) size = sz;
                    break;
                case 10: // md5
                    if (wireType != 2 || !TryReadString(data, ref pos, out md5)) { entry = default!; return false; }
                    break;
                case 11: // objectId
                    if (wireType != 2 || !TryReadString(data, ref pos, out string oid)) { entry = default!; return false; }
                    objectId = oid;
                    break;
                default:
                    // Unknown field — skip and continue
                    if (!TrySkipField(wireType, data, ref pos)) { entry = default!; return false; }
                    break;
            }
        }

        if (objectId is null || string.IsNullOrEmpty(path)) { entry = default!; return false; }
        entry = new ListBinEntry(path, size, md5);
        return true;
    }

    private static bool TryReadString(ReadOnlySpan<byte> data, ref int pos, out string value)
    {
        value = "";
        if (!TryReadVarint(data, ref pos, out int length) || length < 0 || pos + length > data.Length)
            return false;
        value = Encoding.UTF8.GetString(data.Slice(pos, length));
        pos += length;
        return true;
    }

    /// <summary>Reads a protobuf varint from <paramref name="data"/> at <paramref name="pos"/>, advancing pos.</summary>
    private static bool TryReadVarint(ReadOnlySpan<byte> data, ref int pos, out int value)
    {
        value = 0;
        int shift = 0;
        while (pos < data.Length)
        {
            byte b = data[pos++];
            // Accumulate into value only while bits fit in int32; still read (and discard) higher bytes.
            if (shift < 32)
                value |= (b & 0x7F) << shift;
            if ((b & 0x80) == 0) return true;
            shift += 7;
            if (shift >= 70) return false; // max 10 bytes
        }
        return false;
    }

    private static bool TrySkipField(int wireType, ReadOnlySpan<byte> data, ref int pos)
    {
        switch (wireType)
        {
            case 0: return TryReadVarint(data, ref pos, out _);
            case 1: if (pos + 8 > data.Length) return false; pos += 8; return true;
            case 2:
                if (!TryReadVarint(data, ref pos, out int len) || len < 0 || pos + len > data.Length) return false;
                pos += len; return true;
            case 5: if (pos + 4 > data.Length) return false; pos += 4; return true;
            default: return false;
        }
    }
}
