namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Thrown when a capability mint request is denied by policy.
/// </summary>
public class AgentTrustPolicyDeniedException : Exception
{
    /// <summary>
    /// Structured error code.
    /// </summary>
    public string ErrorCode
    {
        get;
    }

    /// <summary>
    /// Policy decision identifier.
    /// </summary>
    public string? DecisionId
    {
        get;
    }

    /// <summary>
    /// Initializes a new policy denied exception.
    /// </summary>
    public AgentTrustPolicyDeniedException(string message, string errorCode, string? decisionId = null)
        : base(message)
    {
        ErrorCode = errorCode;
        DecisionId = decisionId;
    }

    /// <summary>
    /// Initializes a new policy denied exception with an inner exception.
    /// </summary>
    public AgentTrustPolicyDeniedException(
        string message,
        string errorCode,
        Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
