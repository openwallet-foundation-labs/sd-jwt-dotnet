namespace SdJwt.Net.Mdoc.Verifier;

/// <summary>
/// Options for mdoc verification.
/// </summary>
public class MdocVerificationOptions
{
    /// <summary>
    /// Allowed document types. If empty, all types are allowed.
    /// </summary>
    public List<string> AllowedDocTypes { get; set; } = new();

    /// <summary>
    /// Whether to verify the issuer certificate chain.
    /// </summary>
    public bool VerifyCertificateChain { get; set; } = true;

    /// <summary>
    /// Whether to verify validity timestamps.
    /// </summary>
    public bool VerifyValidity { get; set; } = true;

    /// <summary>
    /// Whether to verify device signature.
    /// </summary>
    public bool VerifyDeviceSignature { get; set; } = true;

    /// <summary>
    /// Clock skew tolerance for validity checks.
    /// </summary>
    public TimeSpan ClockSkewTolerance { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Current time for validity checks. If null, uses DateTimeOffset.UtcNow.
    /// </summary>
    public DateTimeOffset? CurrentTime { get; set; }

    /// <summary>
    /// Trusted issuer certificates for chain validation.
    /// </summary>
    public List<byte[]> TrustedIssuers { get; set; } = new();
}
