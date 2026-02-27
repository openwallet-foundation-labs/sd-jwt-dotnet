namespace SdJwt.Net.Verifier;

/// <summary>
/// Policy controls for Key Binding JWT validation.
/// </summary>
public class KeyBindingValidationPolicy
{
    /// <summary>
    /// Requires that the presentation includes a valid Key Binding JWT.
    /// Defaults to false.
    /// </summary>
    public bool RequireKeyBinding
    {
        get; set;
    }

    /// <summary>
    /// Optional nonce expected in the Key Binding JWT.
    /// </summary>
    public string? ExpectedNonce
    {
        get; set;
    }

    /// <summary>
    /// Optional audience expected in the Key Binding JWT.
    /// </summary>
    public string? ExpectedAudience
    {
        get; set;
    }

    /// <summary>
    /// Optional maximum accepted age for the Key Binding JWT iat claim.
    /// </summary>
    public TimeSpan? MaxKeyBindingJwtAge
    {
        get; set;
    }
}
