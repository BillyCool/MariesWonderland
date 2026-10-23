using System.Text;

namespace MariesWonderland.Http;

/// <summary>
/// The asset base URL inside list.bin. The client's list points at the original CDN; the server swaps it for its own
/// address in place, so the replacement must be exactly as long (<see cref="Length"/> bytes). A URL built from a host
/// is <c>{scheme}://{host}/</c> followed by a run of 'a's that fills the remaining length; the asset route accepts
/// any such run as its first path segment.
/// </summary>
public static class ResourcesUrl
{
    /// <summary>The original CDN URL embedded in list.bin.</summary>
    public const string Original = "https://resources.app.nierreincarnation.com";

    /// <summary>The length every replacement must have.</summary>
    public const int Length = 43;

    private static readonly byte[] OriginalBytes = Encoding.ASCII.GetBytes(Original);

    /// <summary>
    /// <c>{scheme}://{host}/</c> padded with 'a's to <see cref="Length"/>, or null when that leaves no room for at
    /// least one 'a' (the host, port included, can be up to 34 characters over http) or the host isn't plain ASCII.
    /// </summary>
    public static string? ForHost(string scheme, string host)
    {
        string prefix = $"{scheme}://{host}/";
        if (prefix.Length >= Length || !Ascii.IsValid(prefix))
        {
            return null;
        }

        return prefix + new string('a', Length - prefix.Length);
    }

    /// <summary>
    /// Whether <paramref name="url"/> can replace the CDN URL and still reach the asset route: exactly
    /// <see cref="Length"/> ASCII characters, http or https, with a path that is one run of 'a's.
    /// </summary>
    public static bool IsValid(string url)
        => url.Length == Length
            && Ascii.IsValid(url)
            && Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && uri.Query.Length == 0
            && uri.AbsolutePath.Length > 1
            && uri.AbsolutePath[1..].All(c => c == 'a');

    /// <summary>
    /// Writes <paramref name="url"/> over the CDN URL in <paramref name="listBin"/>. False when the URL isn't
    /// exactly <see cref="Length"/> ASCII bytes or the list has no CDN URL; the data is left as it was.
    /// </summary>
    public static bool Rewrite(byte[] listBin, string url)
    {
        if (url.Length != Length || !Ascii.IsValid(url))
        {
            return false;
        }

        int index = listBin.AsSpan().IndexOf(OriginalBytes);
        if (index < 0)
        {
            return false;
        }

        Encoding.ASCII.GetBytes(url).CopyTo(listBin, index);
        return true;
    }
}
