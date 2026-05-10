namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Obligation imposed by a policy decision that MUST be enforced during execution.
/// </summary>
public sealed record PolicyObligation
{
    /// <summary>
    /// Whether a signed audit receipt is required.
    /// </summary>
    public bool RequireReceipt
    {
        get; init;
    }

    /// <summary>
    /// Whether human-in-the-loop approval is required.
    /// </summary>
    public bool RequireApproval
    {
        get; init;
    }

    /// <summary>
    /// Whether request binding MUST be present and validated.
    /// </summary>
    public bool RequireRequestBinding
    {
        get; init;
    }

    /// <summary>
    /// Whether proof-of-possession MUST be validated.
    /// </summary>
    public bool RequireProofOfPossession
    {
        get; init;
    }

    /// <summary>
    /// Whether tool registry validation MUST be performed.
    /// </summary>
    public bool RequireToolRegistryValidation
    {
        get; init;
    }

    /// <summary>
    /// Claims that MUST be selectively disclosed.
    /// </summary>
    public IReadOnlyList<string> RequiredDisclosures { get; init; } = [];
}
