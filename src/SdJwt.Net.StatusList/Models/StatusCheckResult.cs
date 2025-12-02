namespace SdJwt.Net.StatusList.Models;

/// <summary>
/// Represents the result of a status check operation.
/// </summary>
public class StatusCheckResult
{
    /// <summary>
    /// Gets or sets the status value from the status list.
    /// </summary>
    public StatusType Status { get; set; }

    /// <summary>
    /// Gets or sets the raw numeric status value from the status list.
    /// </summary>
    public int StatusValue { get; set; }

    /// <summary>
    /// Gets or sets whether this credential is valid.
    /// True if status is VALID (0x00), false otherwise.
    /// </summary>
    public bool IsValid => Status == StatusType.Valid;

    /// <summary>
    /// Gets or sets whether this credential is invalid/revoked.
    /// True if status is INVALID (0x01), false otherwise.
    /// </summary>
    public bool IsInvalid => Status == StatusType.Invalid;

    /// <summary>
    /// Gets or sets whether this credential is suspended.
    /// True if status is SUSPENDED (0x02), false otherwise.
    /// </summary>
    public bool IsSuspended => Status == StatusType.Suspended;

    /// <summary>
    /// Gets whether this credential is active (not revoked or suspended).
    /// True if status is VALID, false otherwise.
    /// </summary>
    public bool IsActive => Status == StatusType.Valid;

    /// <summary>
    /// Gets or sets when this status was last retrieved.
    /// </summary>
    public DateTime RetrievedAt { get; set; }

    /// <summary>
    /// Gets or sets whether this result came from cache.
    /// </summary>
    public bool FromCache { get; set; }

    /// <summary>
    /// Gets or sets the URI of the Status List Token that provided this result.
    /// </summary>
    public string? StatusListUri { get; set; }

    /// <summary>
    /// Gets or sets an error message if the status check failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Creates a successful status check result with status VALID.
    /// </summary>
    /// <returns>A successful StatusCheckResult.</returns>
    public static StatusCheckResult Success()
    {
        return new StatusCheckResult
        {
            Status = StatusType.Valid,
            StatusValue = 0x00,
            RetrievedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a revoked status check result with status INVALID.
    /// </summary>
    /// <returns>A revoked StatusCheckResult.</returns>
    public static StatusCheckResult Revoked()
    {
        return new StatusCheckResult
        {
            Status = StatusType.Invalid,
            StatusValue = 0x01,
            RetrievedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a suspended status check result with status SUSPENDED.
    /// </summary>
    /// <returns>A suspended StatusCheckResult.</returns>
    public static StatusCheckResult Suspended()
    {
        return new StatusCheckResult
        {
            Status = StatusType.Suspended,
            StatusValue = 0x02,
            RetrievedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed status check result.
    /// </summary>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <returns>A failed StatusCheckResult.</returns>
    public static StatusCheckResult Failed(string errorMessage)
    {
        return new StatusCheckResult
        {
            Status = StatusType.Invalid,
            StatusValue = -1,
            ErrorMessage = errorMessage,
            RetrievedAt = DateTime.UtcNow
        };
    }
}