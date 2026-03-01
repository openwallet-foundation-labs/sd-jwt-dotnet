using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Options for verifying a capability token.
/// </summary>
public record CapabilityVerificationOptions
{
    /// <summary>
    /// Expected audience.
    /// </summary>
    public string ExpectedAudience { get; set; } = string.Empty;

    /// <summary>
    /// Trusted issuers and keys.
    /// </summary>
    public IReadOnlyDictionary<string, SecurityKey> TrustedIssuers
    {
        get; set;
    } =
        new Dictionary<string, SecurityKey>(StringComparer.Ordinal);

    /// <summary>
    /// Whether replay prevention is enforced.
    /// </summary>
    public bool EnforceReplayPrevention { get; set; } = true;

    /// <summary>
    /// Clock skew tolerance.
    /// </summary>
    public TimeSpan ClockSkewTolerance { get; set; } = TimeSpan.FromSeconds(30);
}

