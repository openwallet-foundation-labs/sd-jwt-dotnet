using SdJwt.Net.Mdoc.Cose;

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
    public DateTimeOffset? CurrentTime
    {
        get; set;
    }

    /// <summary>
    /// Trusted issuer certificates for chain validation.
    /// </summary>
    public List<byte[]> TrustedIssuers { get; set; } = new();

    /// <summary>
    /// Whether to verify MSO revocation status per ISO 18013-5.
    /// </summary>
    public bool VerifyRevocation
    {
        get; set;
    }

    /// <summary>
    /// Revocation provider for checking MSO revocation status.
    /// Required when <see cref="VerifyRevocation"/> is <c>true</c>.
    /// </summary>
    public IMdocRevocationProvider? RevocationProvider
    {
        get; set;
    }

    /// <summary>
    /// The reader's ephemeral key (EReaderKey), including its private component, used to derive the
    /// EMacKey via ECDH for verifying a <c>DeviceMac</c> (COSE_Mac0). Required only when the device
    /// authenticates with a MAC rather than a signature; if absent, MAC-based device authentication
    /// fails closed. Not used for the <c>DeviceSignature</c> (COSE_Sign1) path.
    /// </summary>
    public CoseKey? ReaderEphemeralKey
    {
        get; set;
    }
}
