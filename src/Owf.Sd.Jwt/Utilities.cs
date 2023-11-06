using gfoidl.Base64;
using System.Text;

namespace Owf.Sd.Jwt;
public static class Utilities
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
        return ToBase64Url(RandomNumberGenerator.GetBytes(16));
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
        return Constants.RESERVED_KEYS.Contains(key);
    }

    public static string GenerateRandomDigest(HashAlgorithm hashAlgorithm)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var digest = ComputeDigest(hashAlgorithm, randomBytes);

        return ToBase64Url(digest);
    }

    public static Dictionary<string, object> GenerateDecoyArrayElement(HashAlgorithm hashAlgorithm)
    {
        string digest = GenerateRandomDigest(hashAlgorithm);

        // { "...": "<digest>" }
        return new Dictionary<string, object>
        {
             { Constants.KEY_THREE_DOTS, digest }
        };
    }

    public static HashSet<KeyValuePair<TKey, TValue>> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> range)
    {
        var duplicateRecords = new HashSet<KeyValuePair<TKey, TValue>>();

        foreach (var item in range)
        {
            if (!dictionary.TryAdd(item.Key, item.Value))
            {
                duplicateRecords.Add(item);
            }
        }

        return duplicateRecords;
    }
}
