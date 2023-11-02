using System.Security.Cryptography;

namespace Owf.Sd.Jwt;
/// <summary>
/// Extension methods for working with hash algorithms.
/// </summary>
public static partial class HashAlgorithmExtension
{

    /// <summary>
    /// Gets the name of the hash algorithm as a string.
    /// </summary>
    /// <param name="supportedHashAlgorithm">The supported hash algorithm enum.</param>
    /// <returns>The name of the hash algorithm.</returns>
    public static string GetHashAlgorithmName(SupportedHashAlgorithm supportedHashAlgorithm)
    {
        return supportedHashAlgorithm switch
        {
            SupportedHashAlgorithm.MD5 => "md5",
            SupportedHashAlgorithm.SHA1 => "sha-1",
            SupportedHashAlgorithm.SHA256 => "sha-256",
            SupportedHashAlgorithm.SHA384 => "sha-384",
            SupportedHashAlgorithm.SHA512 => "sha-512",
            _ => throw new ArgumentException("Unsupported hash algorithm.", nameof(supportedHashAlgorithm)),
        };
    }

    /// <summary>
    /// Gets an instance of the specified hash algorithm.
    /// </summary>
    /// <param name="supportedHashAlgorithm">The supported hash algorithm enum.</param>
    /// <returns>An instance of the requested hash algorithm.</returns>
    public static HashAlgorithm GetHashAlgorithm(SupportedHashAlgorithm supportedHashAlgorithm)
    {
        return supportedHashAlgorithm switch
        {
            SupportedHashAlgorithm.MD5 => MD5.Create(),
            SupportedHashAlgorithm.SHA1 => SHA1.Create(),
            SupportedHashAlgorithm.SHA256 => SHA256.Create(),
            SupportedHashAlgorithm.SHA384 => SHA384.Create(),
            SupportedHashAlgorithm.SHA512 => SHA512.Create(),
            _ => throw new ArgumentException("Unsupported hash algorithm.", nameof(supportedHashAlgorithm)),
        };
    }
}
