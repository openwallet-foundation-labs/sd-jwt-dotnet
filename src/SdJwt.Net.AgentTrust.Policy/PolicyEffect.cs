namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Rule effect.
/// </summary>
public enum PolicyEffect
{
    /// <summary>
    /// Permit matching requests.
    /// </summary>
    Allow,

    /// <summary>
    /// Deny matching requests.
    /// </summary>
    Deny
}

/// <summary>
/// Decision effect for policy evaluation results per spec Section 16.3.
/// </summary>
public enum DecisionEffect
{
    /// <summary>
    /// Request is permitted.
    /// </summary>
    Permit,

    /// <summary>
    /// Request is denied.
    /// </summary>
    Deny,

    /// <summary>
    /// Request requires explicit human or organizational approval before proceeding.
    /// </summary>
    RequireApproval,

    /// <summary>
    /// Request requires a higher assurance level (e.g., proof-of-possession, request binding).
    /// </summary>
    StepUpRequired
}

