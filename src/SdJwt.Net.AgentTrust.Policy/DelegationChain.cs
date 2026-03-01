namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Delegation chain metadata.
/// </summary>
public record DelegationChain
{
    /// <summary>
    /// Original delegating agent.
    /// </summary>
    public string DelegatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Current delegation depth.
    /// </summary>
    public int Depth
    {
        get; set;
    }

    /// <summary>
    /// Maximum allowed depth.
    /// </summary>
    public int MaxDepth
    {
        get; set;
    }

    /// <summary>
    /// Allowed actions.
    /// </summary>
    public IReadOnlyList<string>? AllowedActions
    {
        get; set;
    }

    /// <summary>
    /// Parent token identifier.
    /// </summary>
    public string? ParentTokenId
    {
        get; set;
    }
}

