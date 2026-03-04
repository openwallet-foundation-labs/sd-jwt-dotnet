namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Result of submitting a presentation.
/// </summary>
public class PresentationSubmissionResult
{
    /// <summary>
    /// Whether the submission was successful.
    /// </summary>
    public bool IsSuccessful
    {
        get; set;
    }

    /// <summary>
    /// Redirect URL if the verifier responded with one.
    /// </summary>
    public string? RedirectUri
    {
        get; set;
    }

    /// <summary>
    /// Error code if unsuccessful.
    /// </summary>
    public string? ErrorCode
    {
        get; set;
    }

    /// <summary>
    /// Error description.
    /// </summary>
    public string? ErrorDescription
    {
        get; set;
    }

    /// <summary>
    /// Response code for OAuth callback.
    /// </summary>
    public string? ResponseCode
    {
        get; set;
    }
}
