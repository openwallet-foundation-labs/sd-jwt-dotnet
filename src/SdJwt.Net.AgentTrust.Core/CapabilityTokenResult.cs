namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Result of token minting.
/// </summary>
public record CapabilityTokenResult
{
    /// <summary>
    /// Minted token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token identifier.
    /// </summary>
    public string TokenId { get; set; } = string.Empty;

    /// <summary>
    /// Token expiry.
    /// </summary>
    public DateTimeOffset ExpiresAt
    {
        get; set;
    }
}

