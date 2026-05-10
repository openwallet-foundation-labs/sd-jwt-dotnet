using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Policy evaluation decision per spec Section 16.3.
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
    /// Decision effect per spec Section 16.3.
    /// </summary>
    public DecisionEffect Effect { get; set; } = DecisionEffect.Deny;

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
    /// Reason code per spec Section 16.3.
    /// </summary>
    public string? ReasonCode
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
    /// Policy obligations that must be fulfilled.
    /// </summary>
    public IReadOnlyList<PolicyObligation> Obligations { get; set; } = Array.Empty<PolicyObligation>();

    /// <summary>
    /// Unique decision identifier for audit trail.
    /// </summary>
    public string? DecisionId
    {
        get; set;
    }

    /// <summary>
    /// Policy identifier that produced this decision.
    /// </summary>
    public string? PolicyId
    {
        get; set;
    }

    /// <summary>
    /// Policy version that produced this decision.
    /// </summary>
    public string? PolicyVersion
    {
        get; set;
    }

    /// <summary>
    /// Hash of the policy that produced this decision.
    /// </summary>
    public string? PolicyHash
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
            Effect = DecisionEffect.Permit,
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
            Effect = DecisionEffect.Deny,
            DenialReason = reason,
            DenialCode = code,
            ReasonCode = code
        };
    }
}

