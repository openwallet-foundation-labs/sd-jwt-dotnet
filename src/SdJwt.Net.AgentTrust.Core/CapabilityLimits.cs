namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Machine-enforceable limits on a capability grant.
/// </summary>
public record CapabilityLimits
{
    /// <summary>
    /// Maximum number of returned rows.
    /// </summary>
    public int? MaxResults
    {
        get; set;
    }

    /// <summary>
    /// Maximum number of invocations.
    /// </summary>
    public int? MaxInvocations
    {
        get; set;
    }

    /// <summary>
    /// Maximum payload size in bytes.
    /// </summary>
    public int? MaxPayloadBytes
    {
        get; set;
    }
}

