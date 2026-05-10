namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Validates DPoP proof-of-possession per RFC 9449.
/// </summary>
public interface IDpopProofValidator
{
    /// <summary>
    /// Validates a DPoP proof JWT against the expected constraints.
    /// </summary>
    /// <param name="dpopProof">The DPoP proof JWT from the DPoP header.</param>
    /// <param name="expectedJwkThumbprint">Expected JWK thumbprint from the capability token cnf.jkt claim.</param>
    /// <param name="httpMethod">Actual HTTP method (htm).</param>
    /// <param name="httpUri">Actual HTTP URI (htu).</param>
    /// <param name="accessTokenHash">Optional SHA-256 hash of the access token (ath).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Proof validation result.</returns>
    Task<ProofValidationResult> ValidateAsync(
        string dpopProof,
        string expectedJwkThumbprint,
        string httpMethod,
        string httpUri,
        string? accessTokenHash = null,
        CancellationToken cancellationToken = default);
}
