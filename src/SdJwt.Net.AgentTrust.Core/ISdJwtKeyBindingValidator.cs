namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Validates SD-JWT Key Binding (SD-JWT+KB) proofs.
/// </summary>
public interface ISdJwtKeyBindingValidator
{
    /// <summary>
    /// Validates the key binding JWT in an SD-JWT+KB presentation.
    /// </summary>
    /// <param name="sdJwtPresentation">The full SD-JWT+KB presentation string.</param>
    /// <param name="expectedJwkThumbprint">Expected JWK thumbprint from the cnf claim.</param>
    /// <param name="expectedAudience">Expected audience in the KB-JWT.</param>
    /// <param name="expectedNonce">Expected nonce in the KB-JWT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Proof validation result.</returns>
    Task<ProofValidationResult> ValidateAsync(
        string sdJwtPresentation,
        string expectedJwkThumbprint,
        string expectedAudience,
        string? expectedNonce = null,
        CancellationToken cancellationToken = default);
}
