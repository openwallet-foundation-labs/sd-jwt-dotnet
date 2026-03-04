using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Delegation issuance options.
/// </summary>
public record DelegationTokenOptions : CapabilityTokenOptions
{
    /// <summary>
    /// Delegation metadata.
    /// </summary>
    public DelegationChain Delegation { get; set; } = new();
}

