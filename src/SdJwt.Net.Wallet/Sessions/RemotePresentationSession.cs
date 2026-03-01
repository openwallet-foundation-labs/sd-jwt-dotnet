using SdJwt.Net.Wallet.Protocols;

namespace SdJwt.Net.Wallet.Sessions;

/// <summary>
/// Session implementation for remote OpenID4VP interactions.
/// </summary>
public class RemotePresentationSession : IPresentationSession
{
    private readonly GenericWallet _wallet;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemotePresentationSession"/> class.
    /// </summary>
    /// <param name="wallet">Wallet used to process and submit presentations.</param>
    public RemotePresentationSession(GenericWallet wallet)
    {
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
    }

    /// <inheritdoc/>
    public PresentationFlowType FlowType => PresentationFlowType.Remote;

    /// <inheritdoc/>
    public bool IsActive
    {
        get; private set;
    }

    /// <inheritdoc/>
    public async Task<PresentationRequestInfo> ReceiveRequestAsync(
        string request,
        CancellationToken cancellationToken = default)
    {
        var parsedRequest = await _wallet
            .ProcessPresentationRequestAsync(request, cancellationToken)
            .ConfigureAwait(false);
        IsActive = true;
        return parsedRequest;
    }

    /// <inheritdoc/>
    public async Task<PresentationSubmissionResult> SendResponseAsync(
        PresentationRequestInfo request,
        PresentationSubmissionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _wallet
            .CreateAndSubmitPresentationAsync(request, options, cancellationToken)
            .ConfigureAwait(false);
        IsActive = result.IsSuccessful;
        return result;
    }
}
