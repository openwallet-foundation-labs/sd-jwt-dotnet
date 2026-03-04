using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Options for presentation submission.
/// </summary>
public class PresentationSubmissionOptions
{
    /// <summary>
    /// The credential matches to include.
    /// </summary>
    public IReadOnlyList<CredentialMatch> Matches { get; set; } = [];

    /// <summary>
    /// The key manager for signing.
    /// </summary>
    public IKeyManager? KeyManager
    {
        get; set;
    }

    /// <summary>
    /// Additional state to include.
    /// </summary>
    public string? State
    {
        get; set;
    }
}
