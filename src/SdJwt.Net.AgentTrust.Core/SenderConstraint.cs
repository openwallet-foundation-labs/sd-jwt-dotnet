namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Sender-constraint binding for capability tokens.
/// Maps to the JWT <c>cnf</c> (confirmation) claim per RFC 7800.
/// Supports DPoP proof-of-possession and mTLS certificate binding.
/// </summary>
public record SenderConstraint
{
    /// <summary>
    /// Constraint method: "dpop", "mtls", or "jwk".
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// JWK thumbprint (<c>jkt</c>) of the proof key for DPoP binding per RFC 9449.
    /// </summary>
    public string? JwkThumbprint
    {
        get; set;
    }

    /// <summary>
    /// X.509 certificate SHA-256 thumbprint (<c>x5t#S256</c>) for mTLS binding per RFC 8705.
    /// </summary>
    public string? CertificateThumbprint
    {
        get; set;
    }

    /// <summary>
    /// Creates a DPoP sender constraint.
    /// </summary>
    /// <param name="jwkThumbprint">JWK thumbprint of the DPoP proof key.</param>
    /// <returns>A DPoP-bound sender constraint.</returns>
    public static SenderConstraint Dpop(string jwkThumbprint)
    {
        return new SenderConstraint
        {
            Method = "dpop",
            JwkThumbprint = jwkThumbprint
        };
    }

    /// <summary>
    /// Creates an mTLS sender constraint.
    /// </summary>
    /// <param name="certificateThumbprint">X.509 certificate SHA-256 thumbprint.</param>
    /// <returns>An mTLS-bound sender constraint.</returns>
    public static SenderConstraint Mtls(string certificateThumbprint)
    {
        return new SenderConstraint
        {
            Method = "mtls",
            CertificateThumbprint = certificateThumbprint
        };
    }
}
