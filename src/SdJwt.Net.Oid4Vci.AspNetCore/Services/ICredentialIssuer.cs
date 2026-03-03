using SdJwt.Net.Oid4Vci.Models;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Services;

/// <summary>
/// Represents the result of a credential issuance.
/// </summary>
public sealed record CredentialIssuanceResult(
    CredentialResponse Response,
    bool IsDeferred = false);

/// <summary>
/// Handles the core credential issuance logic for the credential endpoint.
/// Implement this interface to integrate with your credential signing pipeline.
/// </summary>
public interface ICredentialIssuer
{
    /// <summary>
    /// Issues a credential based on the validated credential request.
    /// </summary>
    /// <param name="request">The validated credential request containing proof and format details.</param>
    /// <param name="accessToken">The bearer access token authorizing this issuance.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The issuance result which may contain the credential directly or a deferred token.</returns>
    Task<CredentialIssuanceResult> IssueAsync(
        CredentialRequest request,
        string accessToken,
        CancellationToken cancellationToken = default);
}
