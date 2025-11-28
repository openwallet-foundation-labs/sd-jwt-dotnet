namespace SdJwt.Net.StatusList.Models;

/// <summary>
/// Extension methods for StatusType enumeration.
/// </summary>
public static class StatusTypeExtensions
{
    /// <summary>
    /// Gets the human-readable name for the status type.
    /// </summary>
    /// <param name="statusType">The status type.</param>
    /// <returns>The human-readable name.</returns>
    public static string GetName(this StatusType statusType)
    {
        return statusType switch
        {
            StatusType.Valid => "VALID",
            StatusType.Invalid => "INVALID",
            StatusType.Suspended => "SUSPENDED",
            StatusType.ApplicationSpecific => "APPLICATION_SPECIFIC",
            _ => $"UNKNOWN_{(int)statusType:X2}"
        };
    }

    /// <summary>
    /// Gets the description for the status type.
    /// </summary>
    /// <param name="statusType">The status type.</param>
    /// <returns>The description.</returns>
    public static string GetDescription(this StatusType statusType)
    {
        return statusType switch
        {
            StatusType.Valid => "The status of the Referenced Token is valid, correct or legal.",
            StatusType.Invalid => "The status of the Referenced Token is revoked, annulled, taken back, recalled or cancelled.",
            StatusType.Suspended => "The status of the Referenced Token is temporarily invalid, hanging, debarred from privilege. This state is usually temporary.",
            StatusType.ApplicationSpecific => "The status of the Referenced Token is application specific.",
            _ => $"Unknown status type with value 0x{(int)statusType:X2}"
        };
    }

    /// <summary>
    /// Parses a status value to a StatusType.
    /// </summary>
    /// <param name="value">The numeric status value.</param>
    /// <returns>The corresponding StatusType.</returns>
    public static StatusType FromValue(int value)
    {
        return value switch
        {
            0x00 => StatusType.Valid,
            0x01 => StatusType.Invalid,
            0x02 => StatusType.Suspended,
            0x03 => StatusType.ApplicationSpecific,
            _ when value >= 0x0B && value <= 0x0F => StatusType.ApplicationSpecific,
            _ => (StatusType)value
        };
    }
}