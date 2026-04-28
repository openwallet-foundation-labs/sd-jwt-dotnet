using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace SdJwt.Net.AgentTrust.A2A;

/// <summary>
/// Options for creating a delegation token to delegate capabilities to another agent.
/// </summary>
public record A2ADelegationOptions
{
    /// <summary>
    /// Issuer (the delegating agent).
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Audience (the delegate agent).
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Capability being delegated.
    /// </summary>
    public CapabilityClaim Capability { get; set; } = new();

    /// <summary>
    /// Context for correlation.
    /// </summary>
    public CapabilityContext Context { get; set; } = new();

    /// <summary>
    /// Delegation chain metadata.
    /// </summary>
    public DelegationChain Delegation { get; set; } = new();

    /// <summary>
    /// Token lifetime. Defaults to 60 seconds.
    /// </summary>
    public TimeSpan Lifetime { get; set; } = TimeSpan.FromSeconds(60);
}
