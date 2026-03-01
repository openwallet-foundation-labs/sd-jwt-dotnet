using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Policy evaluation request.
/// </summary>
public record PolicyRequest
{
    /// <summary>
    /// Requesting agent identifier.
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Target tool identifier.
    /// </summary>
    public string Tool { get; set; } = string.Empty;

    /// <summary>
    /// Target action.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Optional resource scope.
    /// </summary>
    public string? Resource
    {
        get; set;
    }

    /// <summary>
    /// Optional capability context.
    /// </summary>
    public CapabilityContext? Context
    {
        get; set;
    }

    /// <summary>
    /// Optional delegation chain.
    /// </summary>
    public DelegationChain? DelegationChain
    {
        get; set;
    }
}

