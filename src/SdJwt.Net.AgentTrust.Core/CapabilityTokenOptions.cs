namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Options for minting a capability token.
/// </summary>
public record CapabilityTokenOptions
{
    /// <summary>
    /// Issuer identity.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Target audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Capability scope.
    /// </summary>
    public CapabilityClaim Capability { get; set; } = new();

    /// <summary>
    /// Correlation context.
    /// </summary>
    public CapabilityContext Context { get; set; } = new();

    /// <summary>
    /// Token lifetime.
    /// </summary>
    public TimeSpan Lifetime { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Optional claims to disclose.
    /// </summary>
    public IReadOnlyList<string>? DisclosableClaims
    {
        get; set;
    }

    /// <summary>
    /// Optional sender constraint for proof-of-possession binding (DPoP, mTLS).
    /// </summary>
    public SenderConstraint? SenderConstraint
    {
        get; set;
    }

    /// <summary>
    /// Optional HTTP request binding for request-level authorization.
    /// </summary>
    public RequestBinding? RequestBinding
    {
        get; set;
    }

    /// <summary>
    /// Optional delegation binding for chained delegation tokens.
    /// </summary>
    public DelegationBinding? Delegation
    {
        get; set;
    }
}

