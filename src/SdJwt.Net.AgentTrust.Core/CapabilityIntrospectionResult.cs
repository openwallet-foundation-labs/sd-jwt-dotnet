using System.Text.Json;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Result of a capability token introspection.
/// </summary>
public sealed record CapabilityIntrospectionResult
{
    /// <summary>
    /// Whether the token is currently active (not expired, not revoked).
    /// </summary>
    public required bool IsActive
    {
        get; init;
    }

    /// <summary>
    /// Token issuer.
    /// </summary>
    public string? Issuer
    {
        get; init;
    }

    /// <summary>
    /// Token subject (agent identity).
    /// </summary>
    public string? Subject
    {
        get; init;
    }

    /// <summary>
    /// Token audience.
    /// </summary>
    public string? Audience
    {
        get; init;
    }

    /// <summary>
    /// Token identifier (jti).
    /// </summary>
    public string? TokenId
    {
        get; init;
    }

    /// <summary>
    /// Token expiry.
    /// </summary>
    public DateTimeOffset? ExpiresAt
    {
        get; init;
    }

    /// <summary>
    /// Token issued-at timestamp.
    /// </summary>
    public DateTimeOffset? IssuedAt
    {
        get; init;
    }

    /// <summary>
    /// Capability claims if <see cref="CapabilityIntrospectionRequest.IncludeClaims"/> was true.
    /// </summary>
    public JsonElement? Claims
    {
        get; init;
    }

    /// <summary>
    /// Error message if introspection failed.
    /// </summary>
    public string? Error
    {
        get; init;
    }
}
