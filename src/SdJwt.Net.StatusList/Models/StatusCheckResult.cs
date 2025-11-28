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
}