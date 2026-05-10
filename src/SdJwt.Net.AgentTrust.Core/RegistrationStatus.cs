namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Lifecycle status of a tool registration in the governance registry.
/// </summary>
public enum RegistrationStatus
{
    /// <summary>
    /// Tool has been proposed but not yet approved.
    /// </summary>
    Proposed,

    /// <summary>
    /// Tool has been approved for use.
    /// </summary>
    Approved,

    /// <summary>
    /// Tool has been temporarily disabled.
    /// </summary>
    Disabled,

    /// <summary>
    /// Tool registration has been revoked.
    /// </summary>
    Revoked
}
