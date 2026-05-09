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

    /// <summary>
    /// Gets or sets the resolver used to retrieve verification keys from DID subjects.
    /// Required when the ID Token subject uses the Decentralized Identifier subject syntax type
    /// (SIOPv2 draft-13 Section 6.2). If null, tokens with DID subjects are rejected.
    /// </summary>
    public IDidKeyResolver? DidKeyResolver
    {
        get; set;
    }
}
