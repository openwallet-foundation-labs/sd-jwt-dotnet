namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Represents an input descriptor.
/// </summary>
public class InputDescriptorInfo
{
    /// <summary>
    /// Unique ID of the descriptor.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Optional name.
    /// </summary>
    public string? Name
    {
        get; set;
    }

    /// <summary>
    /// Optional purpose.
    /// </summary>
    public string? Purpose
    {
        get; set;
    }

    /// <summary>
    /// Accepted credential formats.
    /// </summary>
    public IReadOnlyList<string> Formats { get; set; } = [];

    /// <summary>
    /// Required claim paths.
    /// </summary>
    public IReadOnlyList<string> RequiredClaims { get; set; } = [];

    /// <summary>
    /// Optional claim paths.
    /// </summary>
    public IReadOnlyList<string> OptionalClaims { get; set; } = [];

    /// <summary>
    /// Constraints on the credential.
    /// </summary>
    public ConstraintInfo? Constraints
    {
        get; set;
    }
}
