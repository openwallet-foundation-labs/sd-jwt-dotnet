namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Request to mint a capability token via the Capability Authority (broker).
/// Production callers should use <see cref="ICapabilityAuthority"/> instead of
/// directly using <see cref="CapabilityTokenIssuer"/>.
/// </summary>
public record CapabilityMintRequest
{
    /// <summary>
    /// Identifier of the agent requesting the capability.
    /// </summary>
    public required string SubjectAgent
    {
        get; init;
    }

    /// <summary>
    /// Target audience (protected resource or MCP server).
    /// </summary>
    public required string Audience
    {
        get; init;
    }

    /// <summary>
    /// Workload identity of the calling service or agent runtime.
    /// </summary>
    public required WorkloadIdentity Caller
    {
        get; init;
    }

    /// <summary>
    /// Optional human user identity for user-delegated agent actions.
    /// </summary>
    public UserIdentity? User
    {
        get; init;
    }

    /// <summary>
    /// Optional human-in-the-loop approval evidence.
    /// </summary>
    public ApprovalEvidence? Approval
    {
        get; init;
    }

    /// <summary>
    /// Capability scope being requested.
    /// </summary>
    public CapabilityClaim Capability { get; init; } = new();

    /// <summary>
    /// Execution context for correlation and tracing.
    /// </summary>
    public CapabilityContext Context { get; init; } = new();

    /// <summary>
    /// Requested token lifetime. The authority may reduce this based on policy.
    /// </summary>
    public TimeSpan RequestedLifetime { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Optional sender constraint for proof-of-possession binding.
    /// </summary>
    public SenderConstraint? SenderConstraint
    {
        get; init;
    }

    /// <summary>
    /// Optional request binding for tying the token to a specific operation.
    /// </summary>
    public RequestBinding? RequestBinding
    {
        get; init;
    }

    /// <summary>
    /// Optional delegation binding when the capability is delegated from a parent.
    /// </summary>
    public DelegationBinding? Delegation
    {
        get; init;
    }
}
