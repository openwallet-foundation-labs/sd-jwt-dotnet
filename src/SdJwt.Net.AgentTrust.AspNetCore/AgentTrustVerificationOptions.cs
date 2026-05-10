using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.AspNetCore;

/// <summary>
/// Verification middleware options.
/// </summary>
public record AgentTrustVerificationOptions
{
    /// <summary>
    /// Expected audience.
    /// </summary>
    public required string Audience
    {
        get; init;
    }

    /// <summary>
    /// Trusted issuers and keys.
    /// </summary>
    public IReadOnlyDictionary<string, SecurityKey> TrustedIssuers
    {
        get; init;
    } =
        new Dictionary<string, SecurityKey>();

    /// <summary>
    /// Token header name.
    /// </summary>
    public string TokenHeaderName { get; init; } = "Authorization";

    /// <summary>
    /// Token header prefix (RFC 6750 Bearer scheme recommended for SD-JWT tokens).
    /// </summary>
    public string TokenHeaderPrefix { get; init; } = "Bearer";

    /// <summary>
    /// Excluded path patterns.
    /// </summary>
    public IReadOnlyList<string> ExcludedPaths { get; init; } = ["/health", "/ready", "/.well-known/*"];

    /// <summary>
    /// Enforce action constraints.
    /// </summary>
    public bool EnforceActionConstraints { get; init; } = true;

    /// <summary>
    /// Enforce limits.
    /// </summary>
    public bool EnforceLimits { get; init; } = true;

    /// <summary>
    /// Emit receipts.
    /// </summary>
    public bool EmitReceipts { get; init; } = true;

    /// <summary>
    /// Optional JWKS endpoint.
    /// </summary>
    public string? JwksEndpoint
    {
        get; init;
    }

    /// <summary>
    /// JWKS cache duration.
    /// </summary>
    public TimeSpan JwksCacheDuration { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Security mode for verification strictness.
    /// </summary>
    public AgentTrustSecurityMode SecurityMode { get; init; } = AgentTrustSecurityMode.Pilot;

    /// <summary>
    /// Maximum allowed token lifetime. Tokens with longer exp-iat are rejected.
    /// </summary>
    public TimeSpan MaxTokenLifetime { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Algorithms allowed for token signatures. Null allows any.
    /// </summary>
    public IReadOnlyCollection<string>? AllowedAlgorithms { get; init; }

    /// <summary>
    /// Whether to require tool manifest binding on tokens.
    /// </summary>
    public bool RequireToolManifestBinding { get; init; }

    /// <summary>
    /// Whether to enforce replay prevention.
    /// </summary>
    public bool EnforceReplayPrevention { get; init; } = true;
}
