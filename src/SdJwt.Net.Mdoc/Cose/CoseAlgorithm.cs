namespace SdJwt.Net.Mdoc.Cose;

/// <summary>
/// COSE algorithm identifiers per RFC 8152 and ISO 18013-5.
/// </summary>
public enum CoseAlgorithm
{
    /// <summary>ECDSA with SHA-256 on P-256 curve.</summary>
    ES256 = -7,

    /// <summary>ECDSA with SHA-384 on P-384 curve.</summary>
    ES384 = -35,

    /// <summary>ECDSA with SHA-512 on P-521 curve.</summary>
    ES512 = -36,

    /// <summary>EdDSA (Ed25519 or Ed448).</summary>
    EdDSA = -8,

    /// <summary>HMAC with SHA-256.</summary>
    HMAC256 = 5
}

/// <summary>
/// Extension methods for CoseAlgorithm.
/// </summary>
public static class CoseAlgorithmExtensions
{
    /// <summary>
    /// Gets the corresponding digest algorithm name for the COSE algorithm.
    /// </summary>
    /// <param name="algorithm">The COSE algorithm.</param>
    /// <returns>The digest algorithm name (e.g., "SHA-256").</returns>
    public static string GetDigestAlgorithm(this CoseAlgorithm algorithm)
    {
        return algorithm switch
        {
            CoseAlgorithm.ES256 => "SHA-256",
            CoseAlgorithm.ES384 => "SHA-384",
            CoseAlgorithm.ES512 => "SHA-512",
            CoseAlgorithm.EdDSA => "SHA-512",
            CoseAlgorithm.HMAC256 => "SHA-256",
            _ => throw new NotSupportedException($"Algorithm {algorithm} is not supported.")
        };
    }

    /// <summary>
    /// Checks if the algorithm is supported for HAIP compliance.
    /// </summary>
    /// <param name="algorithm">The COSE algorithm.</param>
    /// <returns>True if the algorithm is HAIP-compliant.</returns>
    public static bool IsSupportedForHaip(this CoseAlgorithm algorithm)
    {
        return algorithm switch
        {
            CoseAlgorithm.ES256 => true,
            CoseAlgorithm.ES384 => true,
            CoseAlgorithm.ES512 => true,
            CoseAlgorithm.EdDSA => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the expected key size in bits for the algorithm.
    /// </summary>
    /// <param name="algorithm">The COSE algorithm.</param>
    /// <returns>The key size in bits.</returns>
    public static int GetKeySizeBits(this CoseAlgorithm algorithm)
    {
        return algorithm switch
        {
            CoseAlgorithm.ES256 => 256,
            CoseAlgorithm.ES384 => 384,
            CoseAlgorithm.ES512 => 521,
            CoseAlgorithm.EdDSA => 256,
            CoseAlgorithm.HMAC256 => 256,
            _ => throw new NotSupportedException($"Algorithm {algorithm} is not supported.")
        };
    }
}
