namespace SdJwt.Net.Oid4Vci.AspNetCore.Services;

/// <summary>
/// Represents an issued access token with its associated c_nonce.
/// </summary>
public sealed record IssuedAccessToken(
    string Token,
    string CNonce,
    int ExpiresInSeconds,
    int CNonceExpiresInSeconds,
    IReadOnlyList<string> AuthorizedConfigurationIds);

/// <summary>
/// Manages access token issuance and validation for the OID4VCI pre-authorized code flow.
/// </summary>
public interface IAccessTokenService
{
    /// <summary>
    /// Issues a new access token for the given pre-authorized code.
    /// </summary>
    /// <param name="preAuthorizedCode">The pre-authorized code from the credential offer.</param>
    /// <param name="transactionCode">Optional transaction code (PIN) if required.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The issued token, or <c>null</c> if the code is invalid or already used.</returns>
    Task<IssuedAccessToken?> IssueForPreAuthorizedCodeAsync(
        string preAuthorizedCode,
        string? transactionCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an access token and returns the associated metadata.
    /// </summary>
    /// <param name="accessToken">The bearer access token.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The token metadata, or <c>null</c> if invalid.</returns>
    Task<IssuedAccessToken?> ValidateAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}
