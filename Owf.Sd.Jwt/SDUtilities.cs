using gfoidl.Base64;
using System.Security.Cryptography;
using System.Text;

namespace Owf.Sd.Jwt;

public static class SDUtilities
{
    public static byte[] ComputeDigest(HashAlgorithm algorithm, byte[] data)
    {
        return algorithm.ComputeHash(data);
    }

    public static string ComputeDigest(HashAlgorithm algorithm, string data)
    {
        byte[] digest = ComputeDigest(algorithm, Encoding.UTF8.GetBytes(data));

        return Base64.Url.Encode(digest);
    }

    public static string GenerateSalt()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
    }

    public static string ToBase64Url(string input)
    {
        // Convert the input string to a byte array.
        var bytes = Encoding.UTF8.GetBytes(input);

        return ToBase64Url(bytes);
    }

    public static string ToBase64Url(byte[] input)
    {
        return Base64.Url.Encode(input);
    }

    public static string FromBase64Url(string base64Url)
    {
        return Encoding.UTF8.GetString(Base64.Url.Decode(base64Url));
    }

    public static bool IsReservedKey(string key)
    {
        return SDConstants.RESERVED_KEYS.Contains(key);
    }
}
