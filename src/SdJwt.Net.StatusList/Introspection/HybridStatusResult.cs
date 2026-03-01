using SdJwt.Net.StatusList.Models;

namespace SdJwt.Net.StatusList.Introspection;

/// <summary>
/// Result of a hybrid status check combining Status List and Token Introspection methods.
/// </summary>
public class HybridStatusResult
{
    /// <summary>
    /// The determined status type.
    /// </summary>
    public StatusType Status
    {
        get; set;
    }

    /// <summary>
    /// The raw status value from the check.
    /// </summary>
    public int StatusValue
    {
        get; set;
    }

    /// <summary>
    /// The method that was used to determine the status.
    /// </summary>
    public StatusCheckMethod Method
    {
        get; set;
    }

    /// <summary>
    /// Whether a fallback method was used due to primary method failure.
    /// </summary>
    public bool UsedFallback
    {
        get; set;
    }

    /// <summary>
    /// The original Status List check result, if Status List was used.
    /// </summary>
    public StatusCheckResult? StatusListResult
    {
        get; set;
    }

    /// <summary>
    /// The original introspection result, if introspection was used.
    /// </summary>
    public IntrospectionResult? IntrospectionResult
    {
        get; set;
    }

    /// <summary>
    /// Timestamp when the check was performed.
    /// </summary>
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Error from the primary method if fallback was used.
    /// </summary>
    public string? PrimaryMethodError
    {
        get; set;
    }

    /// <summary>
    /// Creates a HybridStatusResult from a StatusCheckResult.
    /// </summary>
    /// <param name="result">The Status List result.</param>
    /// <param name="usedFallback">Whether this was a fallback.</param>
    /// <param name="primaryError">Error from primary method if fallback.</param>
    /// <returns>A new HybridStatusResult.</returns>
    public static HybridStatusResult FromStatusListResult(
        StatusCheckResult result,
        bool usedFallback = false,
        string? primaryError = null)
    {
        return new HybridStatusResult
        {
            Status = result.Status,
            StatusValue = result.StatusValue,
            Method = StatusCheckMethod.StatusList,
            UsedFallback = usedFallback,
            StatusListResult = result,
            PrimaryMethodError = primaryError,
            CheckedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a HybridStatusResult from an IntrospectionResult.
    /// </summary>
    /// <param name="result">The introspection result.</param>
    /// <param name="usedFallback">Whether this was a fallback.</param>
    /// <param name="primaryError">Error from primary method if fallback.</param>
    /// <returns>A new HybridStatusResult.</returns>
    public static HybridStatusResult FromIntrospectionResult(
        IntrospectionResult result,
        bool usedFallback = false,
        string? primaryError = null)
    {
        var status = DetermineStatusFromIntrospection(result);

        return new HybridStatusResult
        {
            Status = status,
            StatusValue = (int)status,
            Method = StatusCheckMethod.TokenIntrospection,
            UsedFallback = usedFallback,
            IntrospectionResult = result,
            PrimaryMethodError = primaryError,
            CheckedAt = DateTimeOffset.UtcNow
        };
    }

    private static StatusType DetermineStatusFromIntrospection(IntrospectionResult result)
    {
        if (result.IsActive)
        {
            return StatusType.Valid;
        }

        // Check for specific status values in extension field
        return result.Status?.ToLowerInvariant() switch
        {
            "revoked" => StatusType.Invalid,
            "suspended" => StatusType.Suspended,
            _ => StatusType.Invalid
        };
    }
}

/// <summary>
/// Methods used for status checking.
/// </summary>
public enum StatusCheckMethod
{
    /// <summary>
    /// Status List Token per draft-ietf-oauth-status-list.
    /// </summary>
    StatusList,

    /// <summary>
    /// OAuth 2.0 Token Introspection per RFC 7662.
    /// </summary>
    TokenIntrospection
}
