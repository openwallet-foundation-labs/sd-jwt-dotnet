namespace SdJwt.Net.Wallet.Formats;

/// <summary>
/// Context for creating presentations.
/// </summary>
public class PresentationContext
{
    /// <summary>
    /// The audience (verifier) for the presentation.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Nonce from the verifier.
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Key ID to use for key binding JWT.
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// Signing algorithm for key binding JWT.
    /// </summary>
    public string SigningAlgorithm { get; set; } = "ES256";

    /// <summary>
    /// Optional state value.
    /// </summary>
    public string? State
    {
        get; set;
    }

    /// <summary>
    /// Issued at time. Defaults to now.
    /// </summary>
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
}
