namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Receipt for capability evaluation.
/// </summary>
public record AuditReceipt
{
    /// <summary>
    /// Token id.
    /// </summary>
    public string TokenId { get; set; } = string.Empty;

    /// <summary>
    /// Evaluation timestamp.
    /// </summary>
    public DateTimeOffset Timestamp
    {
        get; set;
    }

    /// <summary>
    /// Evaluation decision.
    /// </summary>
    public ReceiptDecision Decision
    {
        get; set;
    }

    /// <summary>
    /// Tool.
    /// </summary>
    public string Tool { get; set; } = string.Empty;

    /// <summary>
    /// Action.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Correlation id.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Deny reason.
    /// </summary>
    public string? DenyReason
    {
        get; set;
    }

    /// <summary>
    /// Evaluation duration in ms.
    /// </summary>
    public long? DurationMs
    {
        get; set;
    }
}

