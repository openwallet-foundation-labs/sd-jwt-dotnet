using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Adapter interface for OpenID4VP presentation protocol.
/// </summary>
public interface IOid4VpAdapter
{
    /// <summary>
    /// Parses a presentation request from URI or JSON.
    /// </summary>
    /// <param name="request">The request URI, JSON, or same-device request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed presentation request information.</returns>
    Task<PresentationRequestInfo> ParseRequestAsync(
        string request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a request_uri to the actual request.
    /// </summary>
    /// <param name="requestUri">The request_uri to resolve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resolved presentation request.</returns>
    Task<PresentationRequestInfo> ResolveRequestUriAsync(
        string requestUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds matching credentials for the request.
    /// </summary>
    /// <param name="request">The presentation request.</param>
    /// <param name="availableCredentials">Available credentials to match against.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of credential matches.</returns>
    Task<IReadOnlyList<CredentialMatch>> FindMatchingCredentialsAsync(
        PresentationRequestInfo request,
        IReadOnlyList<StoredCredential> availableCredentials,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates and submits a presentation response.
    /// </summary>
    /// <param name="request">The original presentation request.</param>
    /// <param name="options">Submission options with matches and key manager.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Submission result.</returns>
    Task<PresentationSubmissionResult> SubmitPresentationAsync(
        PresentationRequestInfo request,
        PresentationSubmissionOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a presentation error response.
    /// </summary>
    /// <param name="request">The presentation request.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="errorDescription">Optional error description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Submission result with error sent.</returns>
    Task<PresentationSubmissionResult> SendErrorResponseAsync(
        PresentationRequestInfo request,
        string errorCode,
        string? errorDescription = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the client (verifier) using the specified scheme.
    /// </summary>
    /// <param name="request">The presentation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the client is valid.</returns>
    Task<bool> ValidateClientAsync(
        PresentationRequestInfo request,
        CancellationToken cancellationToken = default);
}
