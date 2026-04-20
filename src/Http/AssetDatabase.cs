using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MariesWonderland.Http;

/// <summary>
/// Resolves asset bundle/resource requests by parsing list.bin protobuf indexes and info.json alias maps.
///
/// <para>
/// Asset revisions are deltas, but revision 0 is a complete superset: it contains every objectId
/// that appears across all 818 revisions (confirmed by exhaustive analysis). Later revisions carry
/// updated versions of existing assets, not new ones. <see cref="Resolve"/> therefore checks the
/// client's current revision first, then falls back to revision 0 — a single 25 MB index that
/// covers the entire asset catalogue.
/// </para>
/// </summary>
public sealed class AssetDatabase(string basePath, ILogger<AssetDatabase> logger)
{
    // Lazy-initialized per-revision list.bin indexes (objectId → entry).
    private readonly ConcurrentDictionary<string, Lazy<Dictionary<string, ListBinEntry>>> _listBinCache = new();

    // Lazy-initialized per-revision info.json alias maps (fromName → alias target).
    private readonly ConcurrentDictionary<string, Lazy<Dictionary<string, InfoAlias>?>> _infoCache = new();

    // Per-client-IP active revision (set when client fetches list.bin).
    private readonly ConcurrentDictionary<string, string> _clientRevisions = new();

    // Fallback when no per-client revision is known.
    private volatile string _lastKnownRevision = "0";

    /// <summary>Records that a client fetched list.bin for the given revision.</summary>
    public void RememberRevision(string clientIp, string revision)
    {
        _clientRevisions[clientIp] = revision;
        _lastKnownRevision = revision;
    }

    /// <summary>
    /// Resolves an asset request to ordered file-path candidates.
    /// Caller should try each candidate in order, validating size and MD5, and serve the first valid one.
    /// </summary>
    /// <remarks>
    /// The client's current revision is checked first. If the objectId is not present (e.g. a
    /// recent revision carries only updated entries, not the full catalogue), revision 0 is used
    /// as the fallback. Revision 0 is a confirmed superset of all objectIds across every revision.
    /// </remarks>
    public IEnumerable<AssetCandidate> Resolve(string clientIp, string assetType, string objectId)
    {
        string revision = _clientRevisions.TryGetValue(clientIp, out string? rev) ? rev : _lastKnownRevision;

        // If the objectId isn't in the client's current revision, fall back to revision 0.
        // Revision 0 is a complete superset — every objectId in the game is present there.
        Dictionary<string, ListBinEntry>? currentIndex = LoadListBinIndex(revision);
        if ((currentIndex is null || !currentIndex.ContainsKey(objectId)) && revision != "0")
            revision = "0";

        return ResolveForRevision(revision, assetType, objectId);
    }

    private IEnumerable<AssetCandidate> ResolveForRevision(string revision, string assetType, string objectId)
    {
        Dictionary<string, ListBinEntry>? index = LoadListBinIndex(revision);
        if (index is null || !index.TryGetValue(objectId, out ListBinEntry? entry))
            yield break;

        List<(string Path, bool IsLocaleFallback)> primaryPaths = BuildCandidatePaths(revision, assetType, entry.Path);
        HashSet<string> seen = [];

        foreach ((string path, bool isLocaleFallback) in primaryPaths)
        {
            if (!seen.Add(path))
            {
                continue;
            }

            yield return new AssetCandidate(path, revision, "list.bin", isLocaleFallback ? "" : entry.MD5, entry.Size, isLocaleFallback);
        }

        // info.json alias redirects: if the file name maps to a different target (possibly in another revision)
        Dictionary<string, InfoAlias>? infoIndex = LoadInfoIndex(revision);
        if (infoIndex is not null)
        {
            foreach ((string path, bool _) in primaryPaths)
            {
                string baseName = Path.GetFileName(path);
                if (!infoIndex.TryGetValue(baseName, out InfoAlias? alias)) continue;

                string targetRevision = alias.ToRevision ?? revision;
                string? altPath = BuildAliasPath(path, revision, assetType, targetRevision, alias.ToName);
                if (altPath is null) continue;

                string cacheKey = $"{targetRevision}:{altPath}";
                if (!seen.Add(cacheKey)) continue;

                yield return new AssetCandidate(altPath, targetRevision, "info.json redirect", alias.MD5 ?? "", 0);
            }
        }
    }

    /// <summary>
    /// Builds candidate filesystem paths for a list.bin path string.
    /// Original path first, then locale fallbacks. Non-ASCII paths also get mojibake/fullwidth variants.
    /// </summary>
    private List<(string Path, bool IsLocaleFallback)> BuildCandidatePaths(string revision, string assetType, string pathStr)
    {
        // Safety check on raw path before any substitution
        string rawFsPath = pathStr.Replace(')', '/');
        if (rawFsPath.Contains("..") || Path.IsPathRooted(rawFsPath))
        {
            return [];
        }

        // Build tagged entries: original first, then mojibake/fullwidth variants, then locale fallbacks
        List<(string PathStr, bool IsLocaleFallback)> entries = [(pathStr, false)];

        if (HasNonAscii(pathStr))
        {
            entries.Add((Utf8ToMojibake(pathStr), false));
            entries.Add((NormalizeFullwidth(pathStr), false));
        }

        if (pathStr.Contains(")ja)"))
        {
            entries.Add((pathStr.Replace(")ja)", ")en)"), true));
        }

        if (pathStr.Contains(")ko)"))
        {
            entries.Add((pathStr.Replace(")ko)", ")en)"), true));
        }

        List<(string Path, bool IsLocaleFallback)> result = [];
        HashSet<string> seen = [];

        foreach ((string variant, bool isLocaleFallback) in entries)
        {
            string cleaned = variant.Replace(')', '/');
            if (cleaned.Contains("..") || Path.IsPathRooted(cleaned))
            {
                continue;
            }

            if (!seen.Add(cleaned))
            {
                continue;
            }

            string fullPath = assetType switch
            {
                "assetbundle" => Path.Combine(basePath, revision, "assetbundle", cleaned + ".assetbundle"),
                "resources" => Path.Combine(basePath, revision, "resources", cleaned),
                _ => null!
            };

            if (fullPath is not null)
            {
                result.Add((fullPath, isLocaleFallback));
            }
        }

        return result;
    }

    private static bool HasNonAscii(string s) => s.Any(c => c >= '\x80');

    // Re-encodes non-ASCII chars as if each UTF-8 byte were a Latin-1 codepoint (double-encoding).
    // Matches filenames extracted by tools that misinterpret UTF-8 paths as Latin-1.
    private static string Utf8ToMojibake(string s) =>
        new string(Encoding.UTF8.GetBytes(s).Select(b => (char)b).ToArray());

    // Replaces fullwidth Unicode chars (U+FF01–U+FF5E) with their ASCII equivalents (U+0021–U+007E).
    private static string NormalizeFullwidth(string s) =>
        new string(s.Select(c => c is >= '\uFF01' and <= '\uFF5E' ? (char)(c - 0xFF01 + 0x21) : c).ToArray());

    /// <summary>
    /// Builds the filesystem path for an info.json alias: same directory structure, different revision + filename.
    /// </summary>
    private string? BuildAliasPath(string originalPath, string originalRevision, string assetType,
        string targetRevision, string targetName)
    {
        string typeRoot = Path.Combine(basePath, originalRevision, assetType);
        string rel = Path.GetRelativePath(typeRoot, originalPath);
        if (rel.StartsWith("..") || Path.IsPathRooted(rel)) return null;

        string dir = Path.GetDirectoryName(rel) ?? "";
        return Path.Combine(basePath, targetRevision, assetType, dir, targetName);
    }

    private Dictionary<string, ListBinEntry>? LoadListBinIndex(string revision)
    {
        Lazy<Dictionary<string, ListBinEntry>> lazy = _listBinCache.GetOrAdd(
            revision, rev => new Lazy<Dictionary<string, ListBinEntry>>(() =>
            {
                string path = Path.Combine(basePath, rev, "list.bin");
                if (!File.Exists(path)) return [];
                byte[] data = File.ReadAllBytes(path);
                Dictionary<string, ListBinEntry> index = ListBinParser.Parse(data.AsSpan());
                logger.LogDebug("Loaded list.bin for revision {Revision}: {Count} entries", rev, index.Count);
                return index;
            }));
        return lazy.Value;
    }

    private Dictionary<string, InfoAlias>? LoadInfoIndex(string revision)
    {
        Lazy<Dictionary<string, InfoAlias>?> lazy = _infoCache.GetOrAdd(
            revision, rev => new Lazy<Dictionary<string, InfoAlias>?>(() =>
            {
                string path = Path.Combine(basePath, rev, "info.json");
                if (!File.Exists(path)) return null;
                try
                {
                    InfoJsonEntry[]? entries = JsonSerializer.Deserialize<InfoJsonEntry[]>(File.ReadAllText(path));
                    if (entries is null) return null;

                    Dictionary<string, InfoAlias> result = [];
                    foreach (InfoJsonEntry e in entries)
                    {
                        if (!string.IsNullOrEmpty(e.FromName) && !string.IsNullOrEmpty(e.ToName))
                            result[e.FromName] = new InfoAlias(e.ToName, e.ToRevision?.ToString(), e.MD5);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to parse info.json for revision {Revision}", rev);
                    return null;
                }
            }));
        return lazy.Value;
    }

    // MD5 cache: path → (size, modTimeUtcTicks, md5Hex)
    private readonly ConcurrentDictionary<string, (long Size, long ModTimeTicks, string Md5)> _md5Cache = new();

    /// <summary>
    /// Computes and caches the MD5 hex digest for a file, using cached result when size and modification time are unchanged.
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
            byte[] hash = MD5.HashData(File.ReadAllBytes(filePath));
            string hex = Convert.ToHexStringLower(hash);
            _md5Cache[filePath] = (info.Length, modTicks, hex);
            return hex;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to compute MD5 for {FilePath}", filePath);
            return null;
        }
    }
}

public record ListBinEntry(string Path, long Size, string MD5);
public record AssetCandidate(string Path, string Revision, string Source, string ExpectedMD5, long ExpectedSize, bool IsLocaleFallback = false);
public record InfoAlias(string ToName, string? ToRevision, string? MD5);

public sealed class InfoJsonEntry
{
    [JsonPropertyName("from-name")] public string FromName { get; init; } = "";
    [JsonPropertyName("to-name")] public string ToName { get; init; } = "";
    [JsonPropertyName("to-revision")] public int? ToRevision { get; init; }
    [JsonPropertyName("md5")] public string? MD5 { get; init; }
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
