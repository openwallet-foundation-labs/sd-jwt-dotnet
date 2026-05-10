namespace ReleaseSupport.Shared;

/// <summary>
/// Result of a release investigation.
/// </summary>
public record ReleaseInvestigationResult
{
    /// <summary>
    /// Whether the investigation completed successfully.
    /// </summary>
    public bool Success
    {
        get; init;
    }

    /// <summary>
    /// Repository investigated.
    /// </summary>
    public string Repository { get; init; } = string.Empty;

    /// <summary>
    /// Package investigated.
    /// </summary>
    public string PackageId { get; init; } = string.Empty;

    /// <summary>
    /// Version investigated.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Whether the Git tag exists.
    /// </summary>
    public CheckStatus GitTag
    {
        get; init;
    }

    /// <summary>
    /// Whether the GitHub release exists.
    /// </summary>
    public CheckStatus GitHubRelease
    {
        get; init;
    }

    /// <summary>
    /// Status of the publish workflow.
    /// </summary>
    public WorkflowStatus PublishWorkflow
    {
        get; init;
    }

    /// <summary>
    /// Whether the NuGet package version exists.
    /// </summary>
    public CheckStatus NuGetPackage
    {
        get; init;
    }

    /// <summary>
    /// Deterministic diagnosis message.
    /// </summary>
    public string Diagnosis { get; init; } = string.Empty;

    /// <summary>
    /// Recommended action.
    /// </summary>
    public string RecommendedAction { get; init; } = string.Empty;

    /// <summary>
    /// Error message if the investigation failed.
    /// </summary>
    public string? Error
    {
        get; init;
    }
}

/// <summary>
/// Status of an individual check.
/// </summary>
public enum CheckStatus
{
    /// <summary>
    /// The resource was found.
    /// </summary>
    Found,

    /// <summary>
    /// The resource was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The check could not be performed.
    /// </summary>
    Unknown
}

/// <summary>
/// Status of a GitHub Actions workflow.
/// </summary>
public enum WorkflowStatus
{
    /// <summary>
    /// The workflow completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The workflow failed.
    /// </summary>
    Failed,

    /// <summary>
    /// No relevant workflow run was found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The workflow status could not be determined.
    /// </summary>
    Unknown
}
