namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Risk classification tier for a tool registration.
/// Determines the security controls required for the tool.
/// </summary>
public enum RiskTier
{
    /// <summary>
    /// Low risk: read-only, no side effects.
    /// </summary>
    Low,

    /// <summary>
    /// Medium risk: modifies data within controlled scope.
    /// </summary>
    Medium,

    /// <summary>
    /// High risk: privileged operations, external calls, or PII access.
    /// </summary>
    High,

    /// <summary>
    /// Critical risk: financial transactions, compliance-sensitive, or irreversible actions.
    /// </summary>
    Critical
}
