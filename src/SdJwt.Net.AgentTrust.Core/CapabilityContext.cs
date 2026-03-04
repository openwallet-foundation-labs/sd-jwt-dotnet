namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Execution context for token correlation and tracing.
/// </summary>
public record CapabilityContext
{
    /// <summary>
    /// Unique correlation identifier.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Optional workflow identifier.
    /// </summary>
    public string? WorkflowId
    {
        get; set;
    }

    /// <summary>
    /// Optional step identifier.
    /// </summary>
    public string? StepId
    {
        get; set;
    }

    /// <summary>
    /// Optional tenant identifier.
    /// </summary>
    public string? TenantId
    {
        get; set;
    }
}

