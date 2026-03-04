namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Represents a parsed presentation request.
/// </summary>
public class PresentationRequestInfo
{
    /// <summary>
    /// Unique request ID.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// The verifier's client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The response URI where the presentation should be sent.
    /// </summary>
    public string ResponseUri { get; set; } = string.Empty;

    /// <summary>
    /// The nonce for replay protection.
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Response mode (direct_post, direct_post.jwt, fragment, etc.).
    /// </summary>
    public string ResponseMode { get; set; } = string.Empty;

    /// <summary>
    /// Response type (vp_token, id_token, etc.).
    /// </summary>
    public string ResponseType { get; set; } = string.Empty;

    /// <summary>
    /// State value for correlation.
    /// </summary>
    public string? State
    {
        get; set;
    }

    /// <summary>
    /// The presentation definition or reference.
    /// </summary>
    public PresentationDefinitionInfo? PresentationDefinition
    {
        get; set;
    }

    /// <summary>
    /// Client ID scheme used.
    /// </summary>
    public string? ClientIdScheme
    {
        get; set;
    }

    /// <summary>
    /// Client metadata if provided.
    /// </summary>
    public IDictionary<string, object>? ClientMetadata
    {
        get; set;
    }

    /// <summary>
    /// DC API provider information if applicable.
    /// </summary>
    public string? DcApiProvider
    {
        get; set;
    }
}
