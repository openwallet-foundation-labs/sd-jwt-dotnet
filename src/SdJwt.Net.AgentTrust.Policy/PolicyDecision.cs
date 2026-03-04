namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Policy evaluation decision.
/// </summary>
public record PolicyDecision
{
    /// <summary>
    /// Indicates whether request is permitted.
    /// </summary>
    public bool IsPermitted
    {
        get; set;
    }

    /// <summary>
    /// Denial reason.
    /// </summary>
    public string? DenialReason
    {
        get; set;
    }

    /// <summary>
    /// Denial code.
    /// </summary>
    public string? DenialCode
    {
        get; set;
    }

    /// <summary>
    /// Constraints when permitted.
    /// </summary>
    public PolicyConstraints? Constraints
    {
        get; set;
    }

    /// <summary>
    /// Creates permit decision.
    /// </summary>
    public static PolicyDecision Permit(PolicyConstraints? constraints = null)
    {
        return new PolicyDecision
        {
            IsPermitted = true,
            Constraints = constraints
        };
    }

    /// <summary>
    /// Creates deny decision.
    /// </summary>
    public static PolicyDecision Deny(string reason, string code)
    {
        return new PolicyDecision
        {
            IsPermitted = false,
            DenialReason = reason,
            DenialCode = code
        };
    }
}

