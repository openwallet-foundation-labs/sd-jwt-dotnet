using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;

namespace SdJwt.Net.StatusList.Internal;

/// <summary>
/// Encodes and decodes Token Status List compressed byte arrays.
/// </summary>
internal static class StatusListCodec
{
    /// <summary>
    /// Compresses status values into a base64url-encoded compressed byte array.
    /// </summary>
    public static async Task<string> EncodeAsync(byte[] statusValues, int bits)
    {
        var packed = PackStatusValues(statusValues, bits);
        var compressed = await CompressAsync(packed);
        return Base64UrlEncoder.Encode(compressed);
    }

    /// <summary>
    /// Decodes a base64url-encoded compressed byte array into status values.
    /// </summary>
    public static async Task<byte[]> DecodeAsync(string encodedList, int bits)
    {
        var compressedBytes = Base64UrlEncoder.DecodeBytes(encodedList);
        var decompressedBytes = await DecompressAsync(compressedBytes);
        return UnpackStatusValues(decompressedBytes, bits);
    }

    private static byte[] PackStatusValues(byte[] statusValues, int bits)
    {
        var totalBits = statusValues.Length * bits;
        var byteArray = new byte[(totalBits + 7) / 8];

        for (var i = 0; i < statusValues.Length; i++)
        {
            var statusValue = statusValues[i];
            var bitIndex = i * bits;

            for (var bit = 0; bit < bits; bit++)
            {
                var globalBitIndex = bitIndex + bit;
                var byteIndex = globalBitIndex / 8;
                var bitInByte = globalBitIndex % 8;

                if ((statusValue & (1 << bit)) != 0)
                {
                    byteArray[byteIndex] |= (byte)(1 << bitInByte);
                }
            }
        }

        return byteArray;
    }

    private static byte[] UnpackStatusValues(byte[] decompressedBytes, int bits)
    {
        var totalBits = decompressedBytes.Length * 8;
        var statusCount = totalBits / bits;
        var statusValues = new byte[statusCount];

        for (var i = 0; i < statusCount; i++)
        {
            var bitIndex = i * bits;
            byte statusValue = 0;

            for (var bit = 0; bit < bits; bit++)
            {
                var globalBitIndex = bitIndex + bit;
                var byteIndex = globalBitIndex / 8;
                var bitInByte = globalBitIndex % 8;

                if (byteIndex < decompressedBytes.Length &&
                    (decompressedBytes[byteIndex] & (1 << bitInByte)) != 0)
                {
                    statusValue |= (byte)(1 << bit);
                }
            }

            statusValues[i] = statusValue;
        }

        return statusValues;
    }

    private static async Task<byte[]> CompressAsync(byte[] bytes)
    {
#if NET6_0_OR_GREATER
        using var output = new MemoryStream();
        using (var zlib = new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            await zlib.WriteAsync(bytes, 0, bytes.Length);
        }

        return output.ToArray();
#else
        using var deflatedOutput = new MemoryStream();
        using (var deflate = new DeflateStream(deflatedOutput, CompressionLevel.Optimal, leaveOpen: true))
        {
            await deflate.WriteAsync(bytes, 0, bytes.Length);
        }

        var deflated = deflatedOutput.ToArray();
        using var zlibOutput = new MemoryStream();
        zlibOutput.WriteByte(0x78);
        zlibOutput.WriteByte(0x9C);
        zlibOutput.Write(deflated, 0, deflated.Length);
        WriteAdler32(zlibOutput, ComputeAdler32(bytes));
        return zlibOutput.ToArray();
#endif
    }

    private static async Task<byte[]> DecompressAsync(byte[] compressedBytes)
    {
        try
        {
            return await DecompressZlibAsync(compressedBytes);
        }
        catch (InvalidDataException)
        {
            return await DecompressDeflateAsync(compressedBytes);
        }
    }

    private static async Task<byte[]> DecompressZlibAsync(byte[] compressedBytes)
    {
        using var input = new MemoryStream(compressedBytes);
#if NET6_0_OR_GREATER
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);
#else
        if (compressedBytes.Length < 6)
        {
            throw new InvalidDataException("ZLIB compressed data is too short.");
        }

        input.Position = 2;
        using var zlib = new DeflateStream(
            new MemoryStream(compressedBytes, 2, compressedBytes.Length - 6),
            CompressionMode.Decompress);
#endif
        using var output = new MemoryStream();
        await zlib.CopyToAsync(output);
        return output.ToArray();
    }

    private static async Task<byte[]> DecompressDeflateAsync(byte[] compressedBytes)
    {
        using var input = new MemoryStream(compressedBytes);
        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        await deflate.CopyToAsync(output);
        return output.ToArray();
    }

#if !NET6_0_OR_GREATER
    private static uint ComputeAdler32(byte[] bytes)
    {
        const uint modulus = 65521;
        uint a = 1;
        uint b = 0;

        foreach (var value in bytes)
        {
            a = (a + value) % modulus;
            b = (b + a) % modulus;
        }

        return (b << 16) | a;
    }

    private static void WriteAdler32(Stream stream, uint checksum)
    {
        stream.WriteByte((byte)(checksum >> 24));
        stream.WriteByte((byte)(checksum >> 16));
        stream.WriteByte((byte)(checksum >> 8));
        stream.WriteByte((byte)checksum);
    }
#endif
}
