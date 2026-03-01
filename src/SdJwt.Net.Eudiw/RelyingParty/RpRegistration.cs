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
