using SdJwt.Net.Wallet.Protocols;

namespace SdJwt.Net.Wallet.Issuance;

/// <summary>
/// Represents a resumable credential issuance session.
/// </summary>
public class PendingIssuanceSession
{
    /// <summary>
    /// Session identifier.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Parsed credential offer associated with this session.
    /// </summary>
    public CredentialOfferInfo Offer { get; set; } = new();

    /// <summary>
    /// Optional token exchange options.
    /// </summary>
    public TokenExchangeOptions? TokenExchangeOptions
    {
        get; set;
    }

    /// <summary>
    /// Optional credential configuration ID override.
    /// </summary>
    public string? CredentialConfigurationId
    {
        get; set;
    }

    /// <summary>
    /// Optional key ID override.
    /// </summary>
    public string? KeyId
    {
        get; set;
    }

    /// <summary>
    /// Session creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt
    {
        get; set;
    } = DateTimeOffset.UtcNow;
}
