using MariesWonderland.Http;
using System.Text;

namespace MariesWonderland.Tests.Data;

public class ResourcesUrlTests
{
    [Theory]
    [InlineData("http", "192.168.1.2", "http://192.168.1.2/aaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("http", "10.0.2.2", "http://10.0.2.2/aaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("http", "192.168.1.2:8080", "http://192.168.1.2:8080/aaaaaaaaaaaaaaaaaaa")]
    [InlineData("https", "[::1]:8443", "https://[::1]:8443/aaaaaaaaaaaaaaaaaaaaaaaa")]
    public void ForHost_PadsTheAddressToTheListLength(string scheme, string host, string expected)
    {
        string? url = ResourcesUrl.ForHost(scheme, host);

        Assert.Equal(expected, url);
        Assert.Equal(ResourcesUrl.Length, url!.Length);
    }

    [Fact]
    public void ForHost_RejectsHostsWithNoRoomForPadding()
    {
        Assert.NotNull(ResourcesUrl.ForHost("http", new string('h', 34)));
        Assert.Null(ResourcesUrl.ForHost("http", new string('h', 35)));
        Assert.Null(ResourcesUrl.ForHost("http", "bücher.example"));
    }

    [Fact]
    public void Rewrite_ReplacesTheCdnUrlInPlace()
    {
        byte[] list = [0x0A, 0x2B, .. Encoding.ASCII.GetBytes(ResourcesUrl.Original), 0x12, 0x00];
        string url = ResourcesUrl.ForHost("http", "192.168.1.2")!;

        Assert.True(ResourcesUrl.Rewrite(list, url));

        Assert.Equal([0x0A, 0x2B, .. Encoding.ASCII.GetBytes(url), 0x12, 0x00], list);
    }

    [Fact]
    public void Rewrite_LeavesTheListAloneWhenTheUrlDoesNotFit()
    {
        byte[] original = [.. Encoding.ASCII.GetBytes(ResourcesUrl.Original)];
        byte[] list = [.. original];

        Assert.False(ResourcesUrl.Rewrite(list, "http://short/"));
        Assert.False(ResourcesUrl.Rewrite([1, 2, 3], ResourcesUrl.ForHost("http", "192.168.1.2")!));
        Assert.Equal(original, list);
    }

    [Theory]
    [InlineData("http://192.168.1.2/aaaaaaaaaaaaaaaaaaaaaaaa", true)]
    [InlineData("https://example.org:8443/aaaaaaaaaaaaaaaaaa", true)]
    [InlineData("http://192.168.1.2/xaaaaaaaaaaaaaaaaaaaaaaa", false)]
    [InlineData("http://192.168.1.2/aaaaaaaaaa/aaaaaaaaaaaaa", false)]
    [InlineData("http://192.168.1.2/aaaaaaaaaaaaaaaaaaaaa?aa", false)]
    [InlineData("ftp://192.168.1.2/aaaaaaaaaaaaaaaaaaaaaaaaa", false)]
    [InlineData("http://192.168.1.2/aaaaaaaaaaaaaaaaaaaaaaa", false)]
    [InlineData("http://192.168.1.2.example.org.aaaaaaaaaaa/", false)]
    public void IsValid_NeedsTheListLengthAndAPathOfOnlyAs(string url, bool valid)
        => Assert.Equal(valid, ResourcesUrl.IsValid(url));
}
