namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Binds a delegated capability token to its parent in an agent-to-agent delegation chain.
/// Child capabilities must never expand authority beyond the parent.
/// </summary>
public record DelegationBinding
{
    /// <summary>
    /// Token identifier of the parent capability.
    /// </summary>
    public string ParentTokenId { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of the parent token for non-repudiation.
    /// </summary>
    public string ParentTokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Current delegation depth (1 = first delegation from root).
    /// </summary>
    public int Depth
    {
        get; set;
    }

    /// <summary>
    /// Maximum allowed delegation depth.
    /// </summary>
    public int MaxDepth { get; set; } = 3;

    /// <summary>
    /// Audiences to which the child may further delegate.
    /// </summary>
    public IReadOnlyList<string>? AllowedDownstreamAudiences
    {
        get; set;
    }

    /// <summary>
    /// Issuer of the root capability in the delegation chain.
    /// </summary>
    public string? RootIssuer
    {
        get; set;
    }

    /// <summary>
    /// Identity that delegated this capability.
    /// </summary>
    public string? DelegatedBy
    {
        get; set;
    }

    /// <summary>
    /// Identity receiving this delegated capability.
    /// </summary>
    public string? DelegatedTo
    {
        get; set;
    }

    /// <summary>
    /// Receipt ID from the parent delegation decision.
    /// </summary>
    public string? ParentReceiptId
    {
        get; set;
    }
}
