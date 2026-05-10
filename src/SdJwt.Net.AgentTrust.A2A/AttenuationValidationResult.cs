namespace SdJwt.Net.AgentTrust.A2A;

/// <summary>
/// Result of an attenuation validation check per spec Section 18.2.
/// </summary>
public record AttenuationValidationResult
{
    /// <summary>
    /// Whether the attenuation is valid.
    /// </summary>
    public bool IsValid
    {
        get; private init;
    }

    /// <summary>
    /// List of attenuation violations found.
    /// </summary>
    public IReadOnlyList<string> Violations
    {
        get; private init;
    } = Array.Empty<string>();

    /// <summary>
    /// Creates a valid result.
    /// </summary>
    public static AttenuationValidationResult Valid() => new()
    {
        IsValid = true
    };

    /// <summary>
    /// Creates an invalid result with violation details.
    /// </summary>
    /// <param name="violations">List of attenuation violations.</param>
    public static AttenuationValidationResult Invalid(IReadOnlyList<string> violations) => new()
    {
        IsValid = false,
        Violations = violations
    };
}
