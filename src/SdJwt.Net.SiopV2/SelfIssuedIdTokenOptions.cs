namespace SdJwt.Net.SiopV2;

/// <summary>
/// Options for issuing a SIOPv2 Self-Issued ID Token.
/// </summary>
public class SelfIssuedIdTokenOptions
{
    /// <summary>
    /// Gets or sets the relying party client identifier used as the ID Token audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the nonce from the authorization request.
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token lifetime.
    /// </summary>
    public TimeSpan Lifetime { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets additional self-asserted claims to include in the ID Token.
    /// </summary>
    public IDictionary<string, object>? AdditionalClaims
    {
        get; set;
    }
}
