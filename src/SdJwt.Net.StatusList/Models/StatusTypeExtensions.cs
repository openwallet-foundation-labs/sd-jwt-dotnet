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
            StatusType.UnderInvestigation => "UNDER_INVESTIGATION",
            _ => $"UNKNOWN_{(int)statusType:X2}"
        };
    }

    /// <summary>
    /// Gets the string representation of the status type for use in JSON.
    /// </summary>
    /// <param name="statusType">The status type.</param>
    /// <returns>The string representation.</returns>
    public static string ToStringValue(this StatusType statusType)
    {
        return statusType switch
        {
            StatusType.Valid => "valid",
            StatusType.Invalid => "invalid",
            StatusType.Suspended => "suspended",
            StatusType.UnderInvestigation => "under_investigation",
            _ => $"unknown_{(int)statusType}"
        };
    }

    /// <summary>
    /// Parses a string value to a StatusType.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The corresponding StatusType.</returns>
    public static StatusType FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Status type value cannot be null or empty", nameof(value));

        return value.ToLowerInvariant() switch
        {
            "valid" => StatusType.Valid,
            "invalid" => StatusType.Invalid,
            "suspended" => StatusType.Suspended,
            "under_investigation" => StatusType.UnderInvestigation,
            "application_specific" => StatusType.ApplicationSpecific,
            _ => throw new ArgumentException($"Unknown status type value: {value}", nameof(value))
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
            StatusType.UnderInvestigation => "The status of the Referenced Token is under investigation.",
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
            0x03 => StatusType.UnderInvestigation,
            _ when value >= 0x0B && value <= 0x0F => StatusType.ApplicationSpecific,
            _ => (StatusType)value
        };
    }
}