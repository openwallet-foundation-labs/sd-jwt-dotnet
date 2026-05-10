namespace ReleaseSupport.Shared;

/// <summary>
/// Extended capability claims for release investigation delegation.
/// </summary>
public static class CapabilityClaims
{
    /// <summary>
    /// Capability type for agent delegation.
    /// </summary>
    public const string DelegationType = "agent-delegation";

    /// <summary>
    /// Claim key for the repository scope.
    /// </summary>
    public const string Repository = "repository";

    /// <summary>
    /// Claim key for the package ID scope.
    /// </summary>
    public const string PackageId = "packageId";

    /// <summary>
    /// Claim key for the version scope.
    /// </summary>
    public const string Version = "version";

    /// <summary>
    /// Claim key for the delegation type.
    /// </summary>
    public const string Type = "type";

    /// <summary>
    /// Claim key for the delegating agent.
    /// </summary>
    public const string FromAgent = "fromAgent";

    /// <summary>
    /// Claim key for the delegate agent.
    /// </summary>
    public const string ToAgent = "toAgent";
}
