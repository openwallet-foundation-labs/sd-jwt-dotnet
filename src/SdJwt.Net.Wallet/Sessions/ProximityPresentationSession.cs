using SdJwt.Net.Wallet.Protocols;

namespace SdJwt.Net.Wallet.Sessions;

/// <summary>
/// Placeholder session for proximity presentation flows.
/// </summary>
public class ProximityPresentationSession : IPresentationSession
{
    /// <inheritdoc/>
    public PresentationFlowType FlowType => PresentationFlowType.Proximity;

    /// <inheritdoc/>
    public bool IsActive => false;

    /// <inheritdoc/>
    public Task<PresentationRequestInfo> ReceiveRequestAsync(
        string request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Proximity presentation request handling is not implemented.");
    }

    /// <inheritdoc/>
    public Task<PresentationSubmissionResult> SendResponseAsync(
        PresentationRequestInfo request,
        PresentationSubmissionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Proximity presentation response handling is not implemented.");
    }
}
