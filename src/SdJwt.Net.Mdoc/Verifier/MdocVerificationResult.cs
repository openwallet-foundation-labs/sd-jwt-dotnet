using SdJwt.Net.Mdoc.Models;

namespace SdJwt.Net.Mdoc.Verifier;

/// <summary>
/// Result of mdoc verification.
/// </summary>
public class MdocVerificationResult
{
    /// <summary>
    /// Whether the verification was successful overall.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Whether the issuer signature was valid.
    /// </summary>
    public bool IssuerSignatureValid { get; set; }

    /// <summary>
    /// Whether the device signature was valid (if present).
    /// </summary>
    public bool DeviceSignatureValid { get; set; } = true;

    /// <summary>
    /// Whether all value digests matched.
    /// </summary>
    public bool DigestsValid { get; set; }

    /// <summary>
    /// Whether the credential is within its validity period.
    /// </summary>
    public bool ValidityPeriodValid { get; set; }

    /// <summary>
    /// Whether the certificate chain was valid.
    /// </summary>
    public bool CertificateChainValid { get; set; } = true;

    /// <summary>
    /// Error messages for any failed checks.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Verified claims organized by namespace and element identifier.
    /// </summary>
    public Dictionary<string, Dictionary<string, object>> VerifiedClaims { get; set; } = new();

    /// <summary>
    /// The verified Mobile Security Object.
    /// </summary>
    public MobileSecurityObject? MobileSecurityObject { get; set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful verification result.</returns>
    public static MdocVerificationResult Success()
    {
        return new MdocVerificationResult
        {
            IsValid = true,
            IssuerSignatureValid = true,
            DigestsValid = true,
            ValidityPeriodValid = true
        };
    }

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed verification result.</returns>
    public static MdocVerificationResult Failure(string error)
    {
        return new MdocVerificationResult
        {
            IsValid = false,
            Errors = new List<string> { error }
        };
    }
}
