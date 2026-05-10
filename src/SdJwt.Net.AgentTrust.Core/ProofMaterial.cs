namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Carries proof-of-possession material for capability token verification.
/// Supports DPoP proofs, mTLS certificate thumbprints, and SD-JWT key binding.
/// </summary>
public sealed record ProofMaterial
{
    /// <summary>
    /// DPoP proof JWT from the <c>DPoP</c> or <c>Agent-Capability-DPoP</c> header.
    /// </summary>
    public string? DpopProof
    {
        get; init;
    }

    /// <summary>
    /// SHA-256 thumbprint of the client mTLS certificate.
    /// </summary>
    public string? MtlsCertificateThumbprint
    {
        get; init;
    }

    /// <summary>
    /// SD-JWT key binding presentation string.
    /// </summary>
    public string? SdJwtKeyBinding
    {
        get; init;
    }

    /// <summary>
    /// OAuth access token for dual-DPoP binding (<c>ath</c> computation).
    /// </summary>
    public string? OAuthAccessToken
    {
        get; init;
    }
}
