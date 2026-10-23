using MariesWonderland.Configuration;
using MariesWonderland.Extensions;
using MariesWonderland.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace MariesWonderland.Tests.Http;

/// <summary>
/// The asset endpoints over HTTP against a small dump laid out like the real one: files only in revision 0, whose
/// list is the whole catalogue, a later revision's list naming a changed asset, a mangled file name, an info.json
/// alias, and the master database.
/// </summary>
public sealed class AssetApiTests : IAsyncLifetime
{
    private const string MasterVersion = "20240404193219";
    private const string AssetPrefix = "/aaaaaaaaaaaaaaaaaaaa/unso-200116832-";

    private readonly string _root = Path.Combine(Path.GetTempPath(), $"mw-assets-{Guid.NewGuid():N}");
    private readonly byte[] _plain = [.. Enumerable.Range(0, 300).Select(i => (byte)i)];
    private readonly byte[] _mangled = Encoding.ASCII.GetBytes("mangled file name");
    private readonly byte[] _movie = [.. Enumerable.Repeat((byte)7, 400)];
    private readonly byte[] _master = [.. Enumerable.Range(0, 1000).Select(i => (byte)(i * 3))];
    private readonly List<WebApplication> _apps = [];
    private HttpClient _client = null!;
    private string _host = "";

    private string AssetsPath => Path.Combine(_root, "revisions");
    private string MasterPath => Path.Combine(_root, "master");

    public async ValueTask InitializeAsync()
    {
        byte[] bad = Encoding.ASCII.GetBytes("not what the list says");
        WriteFile(Path.Combine(AssetsPath, "0", "assetbundle", "ui", "icon", "plain.assetbundle"), _plain);
        WriteFile(Path.Combine(AssetsPath, "0", "assetbundle", "ui", "icon", "bad.assetbundle"), bad);
        // Stored under its UTF-8 bytes read as Latin-1, as the dump tool wrote non-ASCII names.
        string mangledName = new([.. Encoding.UTF8.GetBytes("カード").Select(b => (char)b)]);
        WriteFile(Path.Combine(AssetsPath, "0", "assetbundle", "text", "ja", mangledName + ".assetbundle"), _mangled);
        // The en movie is stored once, under the ko name; info.json maps it.
        WriteFile(Path.Combine(AssetsPath, "0", "resources", "mv_ko.mp4"), _movie);
        // Where "..)..)secret" would lead.
        WriteFile(Path.Combine(AssetsPath, "secret.assetbundle"), _plain);

        ListEntry[] catalogue =
        [
            new(2, "plain1", "ui)icon)plain", _plain),
            new(2, "mangle", "text)ja)カード", _mangled),
            new(4, "movie1", "mv_en.mp4", _movie),
            new(2, "badmd5", "ui)icon)bad", _plain),
            new(2, "gone01", "ui)icon)gone", _plain),
            new(2, "escape", "..)..)secret", _plain),
        ];
        WriteFile(Path.Combine(AssetsPath, "0", "list.bin"), ListBin(catalogue));
        WriteFile(Path.Combine(AssetsPath, "0", "info.json"), Encoding.UTF8.GetBytes(
            """[{"md5": "x", "from-name": "mv_en.mp4", "to-revision": 0, "to-name": "mv_ko.mp4"}]"""));
        // A later revision lists only what changed after it, and has no files of its own.
        WriteFile(Path.Combine(AssetsPath, "816", "list.bin"), ListBin([catalogue[0]]));
        WriteFile(Path.Combine(MasterPath, $"{MasterVersion}.bin.e"), _master);

        _client = await StartAsync(new PathsOptions { AssetDatabase = AssetsPath, MasterDatabase = MasterPath });
        _host = _client.BaseAddress!.Authority;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (WebApplication app in _apps)
        {
            await app.DisposeAsync();
        }

        Directory.Delete(_root, true);
    }

    private async Task<HttpClient> StartAsync(PathsOptions paths)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = _root, EnvironmentName = "Production" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        WebApplication app = builder.Build();
        AssetDatabase assetDb = new(paths.AssetDatabase, NullLogger<AssetDatabase>.Instance);
        app.MapAssetApis(paths, assetDb);
        await app.StartAsync(TestContext.Current.CancellationToken);
        _apps.Add(app);
        return new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
    }

    private static void WriteFile(string path, byte[] data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, data);
    }

    private sealed record ListEntry(int Field, string ObjectId, string Name, byte[] Content);

    /// <summary>A list.bin: revision header, the CDN URL template, then one entry per asset (field 2 bundles, field 4 resources).</summary>
    private static byte[] ListBin(IEnumerable<ListEntry> entries)
    {
        List<byte> list = [];
        Varint(list, 1 << 3);
        Varint(list, 817);
        Bytes(list, 5, Encoding.ASCII.GetBytes(ResourcesUrl.Original + "/unso-{v}-{type}/{o}?generation={g}&alt=media"));
        foreach (ListEntry e in entries)
        {
            List<byte> entry = [];
            Bytes(entry, 3, Encoding.UTF8.GetBytes(e.Name));
            Varint(entry, 4 << 3);
            Varint(entry, (ulong)e.Content.Length);
            Bytes(entry, 10, Encoding.ASCII.GetBytes(Convert.ToHexStringLower(MD5.HashData(e.Content))));
            Bytes(entry, 11, Encoding.ASCII.GetBytes(e.ObjectId));
            Varint(entry, 12 << 3);
            Varint(entry, 1626410992546317);
            Bytes(list, e.Field, [.. entry]);
        }

        return [.. list];
    }

    private static void Bytes(List<byte> data, int field, byte[] value)
    {
        Varint(data, (ulong)(field << 3 | 2));
        Varint(data, (ulong)value.Length);
        data.AddRange(value);
    }

    private static void Varint(List<byte> data, ulong value)
    {
        while (value >= 0x80)
        {
            data.Add((byte)(value | 0x80));
            value >>= 7;
        }
        data.Add((byte)value);
    }

    private Task<HttpResponseMessage> Get(string path) => _client.GetAsync(path, TestContext.Current.CancellationToken);

    private Task<HttpResponseMessage> Send(HttpRequestMessage request) => _client.SendAsync(request, TestContext.Current.CancellationToken);

    private static Task<byte[]> Body(HttpResponseMessage response) => response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

    [Fact]
    public async Task List_PointsTheCdnUrlAtTheAddressTheClientUsed()
    {
        HttpResponseMessage response = await Get("/v1/list/300116832/0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string list = Encoding.Latin1.GetString(await Body(response));
        Assert.Contains(ResourcesUrl.ForHost("http", _host) + "/unso-", list);
        Assert.DoesNotContain(ResourcesUrl.Original, list);
    }

    [Theory]
    [InlineData("9999")]
    [InlineData("abc")]
    [InlineData("0%5C..%5C0")]
    [InlineData("..%5C..%5Crevisions%5C0")]
    [InlineData("C:%5Cfoo")]
    public async Task List_OfAMissingOrInvalidRevision_IsNotFound(string revision)
        => Assert.Equal(HttpStatusCode.NotFound, (await Get($"/v1/list/300116832/{revision}")).StatusCode);

    [Fact]
    public async Task ConfiguredResourcesUrl_IsUsed_AndAnInvalidOneFallsBackToTheClientAddress()
    {
        const string configured = "http://192.168.1.2/aaaaaaaaaaaaaaaaaaaaaaaa";
        HttpClient valid = await StartAsync(new PathsOptions { AssetDatabase = AssetsPath, ResourcesBaseUrl = configured });
        HttpClient invalid = await StartAsync(new PathsOptions { AssetDatabase = AssetsPath, ResourcesBaseUrl = "http://192.168.1.2/xaaaaaaaaaaaaaaaaaaaaaaa" });

        string validList = Encoding.Latin1.GetString(await valid.GetByteArrayAsync("/v1/list/300116832/0", TestContext.Current.CancellationToken));
        string invalidList = Encoding.Latin1.GetString(await invalid.GetByteArrayAsync("/v1/list/300116832/0", TestContext.Current.CancellationToken));

        Assert.Contains(configured + "/unso-", validList);
        Assert.Contains(ResourcesUrl.ForHost("http", invalid.BaseAddress!.Authority) + "/unso-", invalidList);
    }

    [Fact]
    public async Task Assets_AreFoundInRevisionZero_WhicheverListTheClientFetched()
    {
        Assert.Equal(_plain, await Body(await Get(AssetPrefix + "assetbundle/plain1")));

        Assert.Equal(HttpStatusCode.OK, (await Get("/v1/list/300116832/816")).StatusCode);
        Assert.Equal(_plain, await Body(await Get(AssetPrefix + "assetbundle/plain1")));
        Assert.Equal(_plain, await Body(await Get("/a/unso-1-assetbundle/plain1")));
    }

    [Fact]
    public async Task Assets_StoredUnderAMangledNameOrAnAlias_AreServed()
    {
        Assert.Equal(_mangled, await Body(await Get(AssetPrefix + "assetbundle/mangle")));
        Assert.Equal(_movie, await Body(await Get(AssetPrefix + "resources/movie1")));
    }

    [Theory]
    [InlineData("assetbundle/badmd5")]
    [InlineData("assetbundle/gone01")]
    [InlineData("assetbundle/escape")]
    [InlineData("assetbundle/nope00")]
    [InlineData("other/plain1")]
    public async Task Assets_WithoutAMatchingFile_AreNotFound(string asset)
        => Assert.Equal(HttpStatusCode.NotFound, (await Get(AssetPrefix + asset)).StatusCode);

    [Fact]
    public async Task Asset_Ranges_AreServed()
    {
        using HttpRequestMessage request = new(HttpMethod.Get, AssetPrefix + "assetbundle/plain1");
        request.Headers.Range = new RangeHeaderValue(100, 109);

        HttpResponseMessage response = await Send(request);

        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal(_plain[100..110], await Body(response));
    }

    [Theory]
    [InlineData(MasterVersion)]
    [InlineData(MasterVersion + "_638123456789")]
    public async Task MasterDatabase_IsServed_DevVersionsIncluded(string version)
        => Assert.Equal(_master, await Body(await Get($"/assets/release/{version}/database.bin.e")));

    [Fact]
    public async Task MasterDatabase_RangePastTheEnd_StopsAtTheEnd()
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"/assets/release/{MasterVersion}/database.bin.e");
        request.Headers.Range = new RangeHeaderValue(900, 5000);

        HttpResponseMessage response = await Send(request);

        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal("bytes 900-999/1000", response.Content.Headers.ContentRange!.ToString());
        Assert.Equal(_master[900..], await Body(response));
    }

    [Fact]
    public async Task MasterDatabase_HeadAndIfNoneMatch_SendNoBody()
    {
        string path = $"/assets/release/{MasterVersion}/database.bin.e";
        using HttpRequestMessage head = new(HttpMethod.Head, path);
        HttpResponseMessage headResponse = await Send(head);
        Assert.Equal(HttpStatusCode.OK, headResponse.StatusCode);
        Assert.Equal(1000, headResponse.Content.Headers.ContentLength);
        Assert.Empty(await Body(headResponse));

        using HttpRequestMessage again = new(HttpMethod.Get, path);
        again.Headers.IfNoneMatch.Add(headResponse.Headers.ETag!);
        Assert.Equal(HttpStatusCode.NotModified, (await Send(again)).StatusCode);
    }

    [Theory]
    [InlineData("19990101000000")]
    [InlineData("..%5Cmaster%5C" + MasterVersion)]
    [InlineData("C:%5C" + MasterVersion)]
    public async Task MasterDatabase_MissingOrInvalidVersion_IsNotFound(string version)
        => Assert.Equal(HttpStatusCode.NotFound, (await Get($"/assets/release/{version}/database.bin.e")).StatusCode);
}
