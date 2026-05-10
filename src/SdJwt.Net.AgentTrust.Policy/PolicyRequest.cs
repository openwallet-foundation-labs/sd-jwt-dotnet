using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Policy evaluation request per spec Section 16.2.
/// </summary>
public record PolicyRequest
{
    /// <summary>
    /// Requesting agent identifier.
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// User identifier for user-scoped policies.
    /// </summary>
    public string? UserId
    {
        get; set;
    }

    /// <summary>
    /// Tenant identifier for multi-tenant policy evaluation.
    /// </summary>
    public string? TenantId
    {
        get; set;
    }

    /// <summary>
    /// Target tool identifier.
    /// </summary>
    public string Tool { get; set; } = string.Empty;

    /// <summary>
    /// Canonical tool identifier per spec Section 16.2.
    /// </summary>
    public string? ToolId
    {
        get; set;
    }

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
    /// Data classification level of the request target.
    /// </summary>
    public string? DataClassification
    {
        get; set;
    }

    /// <summary>
    /// HTTP request binding for request-level policy checks.
    /// </summary>
    public RequestBinding? RequestBinding
    {
        get; set;
    }

    /// <summary>
    /// Delegation evidence for delegation-aware policies.
    /// </summary>
    public DelegationEvidence? Delegation
    {
        get; set;
    }

    /// <summary>
    /// Approval evidence for approval-gated policies.
    /// </summary>
    public ApprovalEvidence? Approval
    {
        get; set;
    }

    /// <summary>
    /// Additional attributes for extensible policy evaluation.
    /// </summary>
    public IReadOnlyDictionary<string, string> Attributes
    {
        get; set;
    } =
        new Dictionary<string, string>();

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

