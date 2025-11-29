namespace SdJwt.Net.StatusList.Models;

/// <summary>
/// Represents the standard Status Types as defined in draft-ietf-oauth-status-list-13.
/// </summary>
public enum StatusType
{
    /// <summary>
    /// The status of the Referenced Token is valid, correct or legal.
    /// Value: 0x00
    /// </summary>
    Valid = 0x00,

    /// <summary>
    /// The status of the Referenced Token is revoked, annulled, taken back, recalled or cancelled.
    /// Value: 0x01
    /// </summary>
    Invalid = 0x01,

    /// <summary>
    /// The status of the Referenced Token is temporarily invalid, hanging, debarred from privilege.
    /// This state is usually temporary.
    /// Value: 0x02
    /// </summary>
    Suspended = 0x02,

    /// <summary>
    /// Application-specific status type for under investigation.
    /// Value: 0x03
    /// </summary>
    UnderInvestigation = 0x03,

    /// <summary>
    /// Application-specific status type.
    /// Value: 0x03 (alias for backward compatibility)
    /// </summary>
    ApplicationSpecific = 0x03
}