namespace SdJwt.Net.StatusList.Introspection;

/// <summary>
/// Result of an OAuth 2.0 Token Introspection request per RFC 7662.
/// </summary>
public class IntrospectionResult
{
    /// <summary>
    /// Boolean indicator of whether or not the presented token is currently active.
    /// REQUIRED per RFC 7662.
    /// </summary>
    public bool IsActive
    {
        get; set;
    }

    /// <summary>
    /// A JSON string containing a space-separated list of scopes associated with this token.
    /// </summary>
    public string? Scope
    {
        get; set;
    }

    /// <summary>
    /// Client identifier for the OAuth 2.0 client that requested this token.
    /// </summary>
    public string? ClientId
    {
        get; set;
    }

    /// <summary>
    /// Human-readable identifier for the resource owner who authorized this token.
    /// </summary>
    public string? Username
    {
        get; set;
    }

    /// <summary>
    /// Type of the token as defined in Section 5.1 of OAuth 2.0 (e.g., "Bearer").
    /// </summary>
    public string? TokenType
    {
        get; set;
    }

    /// <summary>
    /// Integer timestamp indicating when this token will expire (Unix timestamp).
    /// </summary>
    public DateTimeOffset? ExpiresAt
    {
        get; set;
    }

    /// <summary>
    /// Integer timestamp indicating when this token was originally issued (Unix timestamp).
    /// </summary>
    public DateTimeOffset? IssuedAt
    {
        get; set;
    }

    /// <summary>
    /// Integer timestamp indicating when this token is not to be used before (Unix timestamp).
    /// </summary>
    public DateTimeOffset? NotBefore
    {
        get; set;
    }

    /// <summary>
    /// Subject of the token, as defined in JWT Section 4.1.2 (RFC 7519).
    /// Usually a machine-readable identifier of the resource owner.
    /// </summary>
    public string? Subject
    {
        get; set;
    }

    /// <summary>
    /// Service-specific string identifier or list of string identifiers representing
    /// the intended audience for this token.
    /// </summary>
    public string? Audience
    {
        get; set;
    }

    /// <summary>
    /// String representing the issuer of this token.
    /// </summary>
    public string? Issuer
    {
        get; set;
    }

    /// <summary>
    /// String identifier for the token, as defined in JWT Section 4.1.7 (RFC 7519).
    /// </summary>
    public string? JwtId
    {
        get; set;
    }

    /// <summary>
    /// Optional status value indicating the reason for inactivity (e.g., "revoked", "suspended").
    /// Extension field not in base RFC 7662.
    /// </summary>
    public string? Status
    {
        get; set;
    }

    /// <summary>
    /// Additional claims not covered by standard RFC 7662 response fields.
    /// </summary>
    public IDictionary<string, object> AdditionalClaims { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Timestamp when this introspection result was retrieved.
    /// </summary>
    public DateTimeOffset RetrievedAt { get; set; } = DateTimeOffset.UtcNow;
}
