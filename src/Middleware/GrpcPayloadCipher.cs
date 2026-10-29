using System.Buffers;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace MariesWonderland.Middleware;

/// <summary>
/// The client's gRPC message encryption (<c>HandleNet.Encrypt</c>/<c>Decrypt</c>): AES-128-CBC with PKCS7 padding over
/// each serialized protobuf message, under the length prefix. Key and IV are the same in every client: the unpatched
/// game reads them from the EXIF of <c>assets/Icon-60@3x.jpg</c> in the APK (IV from the 6th word of Software, key
/// from the 7th of Model, as dotted decimal ASCII codes). <c>Icon-60@2x.jpg</c> holds the master-data pair instead.
/// </summary>
public static class GrpcPayloadCipher
{
    public const int FrameHeaderLength = 5;

    private static readonly byte[] Key = "1234567890ABCDEF"u8.ToArray();
    private static readonly byte[] Iv = "it8bAjktKdFIBYtU"u8.ToArray();

    public static byte[] Encrypt(ReadOnlySpan<byte> plain)
    {
        using Aes aes = CreateAes();
        return aes.EncryptCbc(plain, Iv, PaddingMode.PKCS7);
    }

    public static byte[] Decrypt(ReadOnlySpan<byte> cipher)
    {
        using Aes aes = CreateAes();
        return aes.DecryptCbc(cipher, Iv, PaddingMode.PKCS7);
    }

    /// <summary>
    /// Encrypts or decrypts the message of every complete gRPC frame (flag byte, 4-byte big-endian length, message)
    /// in <paramref name="input"/> and writes the frames with their new lengths to <paramref name="output"/>.
    /// Returns how many input bytes were consumed; a partial frame at the end is left for the next call.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame is compressed, which the client never sends.</exception>
    /// <exception cref="CryptographicException">A message to decrypt isn't valid ciphertext.</exception>
    public static int TransformFrames(ReadOnlySpan<byte> input, IBufferWriter<byte> output, bool encrypt)
    {
        int consumed = 0;
        while (input.Length - consumed >= FrameHeaderLength)
        {
            ReadOnlySpan<byte> header = input.Slice(consumed, FrameHeaderLength);
            uint length = BinaryPrimitives.ReadUInt32BigEndian(header[1..]);
            if (input.Length - consumed - FrameHeaderLength < length)
            {
                break;
            }

            if (header[0] != 0)
            {
                throw new InvalidDataException("Compressed gRPC messages aren't supported.");
            }

            ReadOnlySpan<byte> message = input.Slice(consumed + FrameHeaderLength, (int)length);
            byte[] result = encrypt ? Encrypt(message) : Decrypt(message);

            Span<byte> frame = output.GetSpan(FrameHeaderLength + result.Length);
            frame[0] = 0;
            BinaryPrimitives.WriteUInt32BigEndian(frame[1..], (uint)result.Length);
            result.CopyTo(frame[FrameHeaderLength..]);
            output.Advance(FrameHeaderLength + result.Length);

            consumed += FrameHeaderLength + (int)length;
        }

        return consumed;
    }

    private static Aes CreateAes()
    {
        Aes aes = Aes.Create();
        aes.Key = Key;
        return aes;
    }
}
