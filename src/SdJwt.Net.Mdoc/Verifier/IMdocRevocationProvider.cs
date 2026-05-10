namespace SdJwt.Net.Mdoc.Verifier;

/// <summary>
/// Provides MSO revocation checking for mdoc documents per ISO 18013-5.
/// </summary>
public interface IMdocRevocationProvider
{
    /// <summary>
    /// Checks whether the Mobile Security Object has been revoked.
    /// </summary>
    /// <param name="issuerCertificate">The issuer certificate bytes from x5chain.</param>
    /// <param name="docType">The document type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the credential is revoked; otherwise <c>false</c>.</returns>
    Task<bool> IsRevokedAsync(byte[] issuerCertificate, string docType, CancellationToken cancellationToken = default);
}
