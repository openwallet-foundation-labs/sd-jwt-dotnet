namespace SdJwt.Net.Eudiw.RelyingParty;

/// <summary>
/// Status of a Relying Party registration.
/// </summary>
public enum RpStatus
{
    /// <summary>
    /// RP is pending approval.
    /// </summary>
    Pending,

    /// <summary>
    /// RP is actively registered.
    /// </summary>
    Active,

    /// <summary>
    /// RP registration is temporarily suspended.
    /// </summary>
    Suspended,

    /// <summary>
    /// RP registration has been revoked.
    /// </summary>
    Revoked
}

/// <summary>
/// Relying Party registration data for EUDIW trust framework.
/// </summary>
public class RpRegistration
{
    /// <summary>
    /// The client identifier (typically a URI).
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the organization.
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Authorized redirect URIs.
    /// </summary>
    public string[] RedirectUris { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Supported response types (e.g., "vp_token").
    /// </summary>
    public string[] ResponseTypes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Current registration status.
    /// </summary>
    public RpStatus Status
    {
        get; set;
    }

    /// <summary>
    /// Trust framework identifier.
    /// </summary>
    public string? TrustFramework
    {
        get; set;
    }
}

/// <summary>
/// Result of RP registration validation.
/// </summary>
public class RpValidationResult
{
    /// <summary>
    /// Indicates whether validation succeeded.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Error message if validation failed.
    /// </summary>
    public string? Error
    {
        get; set;
    }

    /// <summary>
    /// Validation warnings (non-blocking issues).
    /// </summary>
    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful RP validation result.</returns>
    public static RpValidationResult Success()
    {
        return new RpValidationResult { IsValid = true };
    }

    /// <summary>
    /// Creates a successful validation result with warnings.
    /// </summary>
    /// <param name="warnings">The warnings.</param>
    /// <returns>A successful RP validation result with warnings.</returns>
    public static RpValidationResult SuccessWithWarnings(params string[] warnings)
    {
        return new RpValidationResult { IsValid = true, Warnings = warnings };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed RP validation result.</returns>
    public static RpValidationResult Failure(string error)
    {
        return new RpValidationResult { IsValid = false, Error = error };
    }
}
