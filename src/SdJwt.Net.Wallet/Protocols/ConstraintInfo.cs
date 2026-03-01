namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Credential constraints information.
/// </summary>
public class ConstraintInfo
{
    /// <summary>
    /// Field constraints.
    /// </summary>
    public IReadOnlyList<FieldConstraintInfo> Fields { get; set; } = [];

    /// <summary>
    /// Limit disclosure policy.
    /// </summary>
    public string? LimitDisclosure
    {
        get; set;
    }
}
