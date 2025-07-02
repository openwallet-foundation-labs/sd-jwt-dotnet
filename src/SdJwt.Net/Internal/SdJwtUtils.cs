using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.Internal;

/// <summary>  
/// Utility functions for SD-JWT processing. This class is not part of the public API.  
/// </summary>  
public static class SdJwtUtils
{
    /// <summary>  
    /// Creates a cryptographically secure salt value, encoded as Base64Url.  
    /// </summary>  
    /// <returns>A Base64Url encoded salt string.</returns>  
    public static string GenerateSalt()
    {
        var saltBytes = new byte[16];
        RandomNumberGenerator.Fill(saltBytes);
        return Base64UrlEncoder.Encode(saltBytes);
    }

    /// <summary>  
    /// Maps an IANA-registered hash algorithm name to the .NET <see cref="HashAlgorithmName"/>.  
    /// </summary>  
    private static HashAlgorithmName GetHashAlgorithmName(string hashAlgorithm)
    {
        return hashAlgorithm.ToUpperInvariant() switch
        {
            "SHA-256" => HashAlgorithmName.SHA256,
            "SHA-384" => HashAlgorithmName.SHA384,
            "SHA-512" => HashAlgorithmName.SHA512,
#if NET9_0_OR_GREATER
            "SHA3-256" => HashAlgorithmName.SHA3_256,
            "SHA3-384" => HashAlgorithmName.SHA3_384,
            "SHA3-512" => HashAlgorithmName.SHA3_512,
#endif
            "MD5" => HashAlgorithmName.MD5,
            "SHA-1" => HashAlgorithmName.SHA1,
            _ => throw new NotSupportedException($"Unsupported hash algorithm: {hashAlgorithm}")
        };
    }

    /// <summary>  
    /// Calculates the digest of a disclosure according to the specified hash algorithm.  
    /// </summary>  
    /// <param name="hashAlgorithm">The IANA name of the hash algorithm (e.g., "sha-256").</param>  
    /// <param name="encodedDisclosure">The Base64Url encoded disclosure string.</param>  
    /// <returns>A Base64Url encoded digest string.</returns>  
    public static string CreateDigest(string hashAlgorithm, string encodedDisclosure)
    {
        var algorithmName = GetHashAlgorithmName(hashAlgorithm);
        var disclosureBytes = Encoding.ASCII.GetBytes(encodedDisclosure);

        // Replace the obsolete HashAlgorithm.Create(string) with specific algorithm instantiation  
        using HashAlgorithm hashAlgorithmInstance = algorithmName.Name switch
        {
            nameof(HashAlgorithmName.SHA256) => SHA256.Create(),
            nameof(HashAlgorithmName.SHA384) => SHA384.Create(),
            nameof(HashAlgorithmName.SHA512) => SHA512.Create(),
#if NET9_0_OR_GREATER
            nameof(HashAlgorithmName.SHA3_256) => SHA3_256.Create(),
            nameof(HashAlgorithmName.SHA3_384) => SHA3_384.Create(),
            nameof(HashAlgorithmName.SHA3_512) => SHA3_512.Create(),
#endif
            nameof(HashAlgorithmName.MD5) => MD5.Create(),
            nameof(HashAlgorithmName.SHA1) => SHA1.Create(),
            _ => throw new NotSupportedException($"Hash algorithm '{algorithmName.Name}' is not supported.")
        };

        var digestBytes = hashAlgorithmInstance.ComputeHash(disclosureBytes);
        return Base64UrlEncoder.Encode(digestBytes);
    }

    /// <summary>  
    /// A helper method to convert a <see cref="JsonElement"/> to a primitive .NET type,  
    /// a dictionary, or a list.  
    /// </summary>  
    public static object ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            JsonValueKind.Object => element.Deserialize<Dictionary<string, object>>(SdJwtConstants.DefaultJsonSerializerOptions)!,
            JsonValueKind.Array => element.Deserialize<List<object>>(SdJwtConstants.DefaultJsonSerializerOptions)!,
            _ => throw new InvalidOperationException($"Unsupported JsonValueKind: {element.ValueKind}")
        };
    }
}