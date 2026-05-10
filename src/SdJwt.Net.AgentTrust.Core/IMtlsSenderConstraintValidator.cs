using System.Security.Cryptography.X509Certificates;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Validates mTLS sender-constrained tokens per RFC 8705.
/// </summary>
public interface IMtlsSenderConstraintValidator
{
    /// <summary>
    /// Validates that the client certificate matches the capability token's cnf.x5t#S256 claim.
    /// </summary>
    /// <param name="clientCertificate">The TLS client certificate from the connection.</param>
    /// <param name="expectedThumbprint">Expected SHA-256 thumbprint from the capability token cnf claim.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Proof validation result.</returns>
    Task<ProofValidationResult> ValidateAsync(
        X509Certificate2 clientCertificate,
        string expectedThumbprint,
        CancellationToken cancellationToken = default);
}
