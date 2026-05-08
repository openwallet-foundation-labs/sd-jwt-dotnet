using SdJwt.Net.Oid4Vp.Models.Dcql;

namespace SdJwt.Net.Oid4Vp.Verifier.Formats;

/// <summary>
/// Validates one OID4VP VP Token entry for a specific credential format.
/// </summary>
public interface IVpFormatValidator
{
    /// <summary>
    /// Gets the credential format identifier handled by this validator.
    /// </summary>
    string Format { get; }

    /// <summary>
    /// Validates the presentation for the supplied DCQL credential query.
    /// </summary>
    /// <param name="presentation">The presentation value from <c>vp_token</c>.</param>
    /// <param name="query">The matching DCQL credential query.</param>
    /// <param name="context">The validation context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result.</returns>
    Task<VpFormatValidationResult> ValidateAsync(
        object presentation,
        DcqlCredentialQuery query,
        VpFormatValidationContext context,
        CancellationToken cancellationToken = default);
}
