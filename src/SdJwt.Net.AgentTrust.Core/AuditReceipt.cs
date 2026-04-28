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

    /// <summary>
    /// Agent identifier that requested the operation.
    /// </summary>
    public string? AgentId
    {
        get; set;
    }

    /// <summary>
    /// Tenant identifier for multi-tenant environments.
    /// </summary>
    public string? TenantId
    {
        get; set;
    }

    /// <summary>
    /// Policy identifier that governed the decision.
    /// </summary>
    public string? PolicyId
    {
        get; set;
    }

    /// <summary>
    /// Policy version for audit reproducibility.
    /// </summary>
    public string? PolicyVersion
    {
        get; set;
    }

    /// <summary>
    /// SHA-256 hash of the request input for non-repudiation.
    /// </summary>
    public string? InputHash
    {
        get; set;
    }

    /// <summary>
    /// SHA-256 hash of the response output for non-repudiation.
    /// </summary>
    public string? OutputHash
    {
        get; set;
    }
}

