namespace SdJwt.Net.SiopV2;

/// <summary>
/// Validation parameters for SIOPv2 Self-Issued ID Tokens.
/// </summary>
public class SelfIssuedIdTokenValidationParameters
{
    /// <summary>
    /// Gets or sets the expected relying party client identifier.
    /// </summary>
    public string ExpectedAudience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected nonce.
    /// </summary>
    public string ExpectedNonce { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the clock skew allowed for lifetime validation.
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(2);
}
