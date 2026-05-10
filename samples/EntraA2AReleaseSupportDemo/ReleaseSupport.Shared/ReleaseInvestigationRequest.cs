namespace ReleaseSupport.Shared;

/// <summary>
/// Request model for a release investigation.
/// </summary>
public record ReleaseInvestigationRequest
{
    /// <summary>
    /// GitHub repository in <c>owner/repo</c> format.
    /// </summary>
    public string Repository { get; init; } = string.Empty;

    /// <summary>
    /// NuGet package identifier.
    /// </summary>
    public string PackageId { get; init; } = string.Empty;

    /// <summary>
    /// Version to investigate.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Requested action (must match capability token).
    /// </summary>
    public string Action { get; init; } = Constants.Tools.InvestigateAction;
}
