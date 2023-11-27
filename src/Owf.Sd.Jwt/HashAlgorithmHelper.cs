﻿namespace Owf.Sd.Jwt;
/// <summary>
/// Extension methods for working with hash algorithms.
/// </summary>
public static class HashAlgorithmHelper
{
    /// <summary>
    /// Gets the name of the hash algorithm as a string.
    /// </summary>
    /// <param name="supportedHashAlgorithm">The supported hash algorithm enum.</param>
    /// <returns>The name of the hash algorithm.</returns>
    public static string GetHashAlgorithmName(SupportHashAlgorithm supportedHashAlgorithm)
    {
        return supportedHashAlgorithm switch
        {
            SupportHashAlgorithm.MD5 => "md5"
            ,
            SupportHashAlgorithm.SHA1 => "sha-1"
            ,
            SupportHashAlgorithm.SHA256 => "sha-256"
            ,
            SupportHashAlgorithm.SHA384 => "sha-384"
            ,
            SupportHashAlgorithm.SHA512 => "sha-512"
#if NET8_0
            ,SupportHashAlgorithm.SHA3_256 => "sha3-256"
            ,SupportHashAlgorithm.SHA3_384 => "sha3-384"
            ,SupportHashAlgorithm.SHA3_512 => "sha3-512"
#endif
           ,
            _ => throw new ArgumentException("Unsupported hash algorithm.", nameof(supportedHashAlgorithm)),
        };
    }

    public static SupportHashAlgorithm GetSupportHashAlgorithm(string hashAlgorithm)
    {
        return hashAlgorithm.ToLower() switch
        {
            "md5" => SupportHashAlgorithm.MD5
            ,
            "sha-1" => SupportHashAlgorithm.SHA1
            ,
            "sha-256" => SupportHashAlgorithm.SHA256
            ,
            "sha-384" => SupportHashAlgorithm.SHA384
            ,
            "sha-512" => SupportHashAlgorithm.SHA512
#if NET8_0
            ,"sha3-256" => SupportHashAlgorithm.SHA3_256
            ,"sha3-384" => SupportHashAlgorithm.SHA3_384
            ,"sha3-512" => SupportHashAlgorithm.SHA3_512
#endif
            ,
            _ => throw new ArgumentException("Unsupported hash algorithm.", nameof(hashAlgorithm)),
        };
    }

    /// <summary>
    /// Gets an instance of the specified hash algorithm.
    /// </summary>
    /// <param name="supportedHashAlgorithm">The supported hash algorithm enum.</param>
    /// <returns>An instance of the requested hash algorithm.</returns>
    public static HashAlgorithm GetHashAlgorithm(SupportHashAlgorithm supportedHashAlgorithm)
    {
        return supportedHashAlgorithm switch
        {
            SupportHashAlgorithm.MD5 => MD5.Create()
            ,SupportHashAlgorithm.SHA1 => SHA1.Create()
            ,SupportHashAlgorithm.SHA256 => SHA256.Create()
            ,SupportHashAlgorithm.SHA384 => SHA384.Create()
            ,SupportHashAlgorithm.SHA512 => SHA512.Create()
#if NET8_0
            ,SupportHashAlgorithm.SHA3_256 => SHA3_256.Create()
            ,SupportHashAlgorithm.SHA3_384 => SHA3_384.Create()
            ,SupportHashAlgorithm.SHA3_512 => SHA3_512.Create()
#endif
            ,_ => throw new ArgumentException("Unsupported hash algorithm.", nameof(supportedHashAlgorithm)),
        };
    }
}
