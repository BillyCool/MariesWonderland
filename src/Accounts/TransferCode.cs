using System.Security.Cryptography;
using System.Text;

namespace MariesWonderland.Accounts;

/// <summary>
/// Transfer codes: 12 random Crockford base32 characters (60 bits), shown as <c>XXXX-XXXX-XXXX</c>. Typing is
/// forgiving: case, spaces, dashes and the look-alikes O/I/L are accepted. Only a hash is stored.
/// </summary>
public static class TransferCode
{
    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
    public const int Length = 12;

    public static string New() => Random(Length);

    /// <summary>A random string of <paramref name="length"/> characters from the code alphabet.</summary>
    public static string Random(int length)
    {
        Span<byte> bytes = stackalloc byte[length];
        RandomNumberGenerator.Fill(bytes);
        StringBuilder sb = new(length);
        foreach (byte b in bytes)
        {
            sb.Append(Alphabet[b & 31]);
        }
        return sb.ToString();
    }

    /// <summary>The code in groups of four, for display.</summary>
    public static string Format(string code)
    {
        string normalized = Normalize(code) ?? code;
        return string.Join('-', Enumerable.Range(0, (normalized.Length + 3) / 4)
            .Select(i => normalized.Substring(i * 4, Math.Min(4, normalized.Length - i * 4))));
    }

    /// <summary>The canonical code typed by a player, or null when it can't be one.</summary>
    public static string? Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        StringBuilder sb = new(Length);
        foreach (char raw in input)
        {
            if (raw is '-' or ' ' or '_')
            {
                continue;
            }

            char c = char.ToUpperInvariant(raw) switch
            {
                'O' => '0',
                'I' or 'L' => '1',
                char other => other,
            };
            if (!Alphabet.Contains(c))
            {
                return null;
            }
            sb.Append(c);
        }
        return sb.Length == Length ? sb.ToString() : null;
    }

    /// <summary>SHA-256 of the canonical code. Codes are random and long, so a fast hash is enough.</summary>
    public static string Hash(string normalizedCode)
        => Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(normalizedCode)));
}

/// <summary>PBKDF2 password hashes for passwords people choose (admin logins, transfer passwords).</summary>
public static class PasswordHashing
{
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"pbkdf2-sha256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string hash)
    {
        string[] parts = hash.Split('$');
        if (parts.Length != 4 || parts[0] != "pbkdf2-sha256" || !int.TryParse(parts[1], out int iterations))
        {
            return false;
        }

        byte[] salt = Convert.FromBase64String(parts[2]);
        byte[] expected = Convert.FromBase64String(parts[3]);
        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
