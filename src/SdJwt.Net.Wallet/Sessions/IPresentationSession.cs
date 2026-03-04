using SdJwt.Net.Wallet.Protocols;

namespace SdJwt.Net.Wallet.Sessions;

/// <summary>
/// Abstraction for a presentation session lifecycle.
/// </summary>
public interface IPresentationSession
{
    /// <summary>
    /// Flow type represented by this session.
    /// </summary>
    PresentationFlowType FlowType
    {
        get;
    }

    /// <summary>
    /// Indicates whether the session is active.
    /// </summary>
    bool IsActive
    {
        get;
    }

    /// <summary>
    /// Receives and parses a verifier request.
    /// </summary>
    /// <param name="request">Request URI or payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed request details.</returns>
    Task<PresentationRequestInfo> ReceiveRequestAsync(
        string request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a response for a parsed request.
    /// </summary>
    /// <param name="request">Parsed request.</param>
    /// <param name="options">Optional submission options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Submission result.</returns>
    Task<PresentationSubmissionResult> SendResponseAsync(
        PresentationRequestInfo request,
        PresentationSubmissionOptions? options = null,
        CancellationToken cancellationToken = default);
}
