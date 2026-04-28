namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Represents a resolved workload identity for an agent or service runtime.
/// </summary>
public record WorkloadIdentity
{
    /// <summary>
    /// Unique workload identifier (e.g., SPIFFE ID, Entra client ID, service account name).
    /// </summary>
    public string SubjectId { get; set; } = string.Empty;

    /// <summary>
    /// Identity provider type (e.g., "entra", "spiffe", "kubernetes", "static").
    /// </summary>
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// Issuer of the workload credential.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Optional tenant identifier for multi-tenant environments.
    /// </summary>
    public string? TenantId
    {
        get; set;
    }

    /// <summary>
    /// Optional credential material (JWT, SVID, or opaque token).
    /// </summary>
    public string? Credential
    {
        get; set;
    }

    /// <summary>
    /// Credential expiry, if applicable.
    /// </summary>
    public DateTimeOffset? ExpiresAt
    {
        get; set;
    }

    /// <summary>
    /// Additional claims or attributes from the identity provider.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Attributes
    {
        get; set;
    }
}
