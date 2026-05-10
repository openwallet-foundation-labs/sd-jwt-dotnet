using ReleaseSupport.Shared;

namespace ReleaseSupport.InvestigatorA2A;

/// <summary>
/// Deterministic diagnosis engine for release publication issues.
/// </summary>
public class ReleaseDiagnosisService
{
    private readonly GitHubReleaseClient _github;
    private readonly NuGetPackageClient _nuget;
    private readonly ILogger<ReleaseDiagnosisService> _logger;

    /// <summary>
    /// Initializes the diagnosis service.
    /// </summary>
    public ReleaseDiagnosisService(
        GitHubReleaseClient github,
        NuGetPackageClient nuget,
        ILogger<ReleaseDiagnosisService> logger)
    {
        _github = github ?? throw new ArgumentNullException(nameof(github));
        _nuget = nuget ?? throw new ArgumentNullException(nameof(nuget));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs a release investigation and returns a deterministic diagnosis.
    /// </summary>
    public async Task<ReleaseInvestigationResult> InvestigateAsync(
        ReleaseInvestigationRequest request,
        CancellationToken ct = default)
    {
        var parts = request.Repository.Split('/', 2);
        if (parts.Length != 2)
        {
            return new ReleaseInvestigationResult
            {
                Success = false,
                Repository = request.Repository,
                PackageId = request.PackageId,
                Version = request.Version,
                Error = "Invalid repository format. Expected owner/repo."
            };
        }

        var owner = parts[0];
        var repo = parts[1];

        _logger.LogInformation("Starting investigation for {Owner}/{Repo} package {PackageId} version {Version}",
            owner, repo, request.PackageId, request.Version);

        var tagExists = await _github.TagExistsAsync(owner, repo, request.Version, ct);
        var releaseExists = await _github.ReleaseExistsAsync(owner, repo, request.Version, ct);
        var workflowStatus = await _github.GetPublishWorkflowStatusAsync(owner, repo, ct);
        var nugetExists = await _nuget.PackageVersionExistsAsync(request.PackageId, request.Version, ct);

        var gitTagStatus = tagExists ? CheckStatus.Found : CheckStatus.NotFound;
        var releaseStatus = releaseExists ? CheckStatus.Found : CheckStatus.NotFound;
        var nugetStatus = nugetExists ? CheckStatus.Found : CheckStatus.NotFound;
        var wfStatus = workflowStatus switch
        {
            true => WorkflowStatus.Success,
            false => WorkflowStatus.Failed,
            null => WorkflowStatus.NotFound
        };

        var (diagnosis, recommendedAction) = Diagnose(gitTagStatus, releaseStatus, wfStatus, nugetStatus);

        return new ReleaseInvestigationResult
        {
            Success = true,
            Repository = request.Repository,
            PackageId = request.PackageId,
            Version = request.Version,
            GitTag = gitTagStatus,
            GitHubRelease = releaseStatus,
            PublishWorkflow = wfStatus,
            NuGetPackage = nugetStatus,
            Diagnosis = diagnosis,
            RecommendedAction = recommendedAction
        };
    }

    private static (string Diagnosis, string RecommendedAction) Diagnose(
        CheckStatus tag,
        CheckStatus release,
        WorkflowStatus workflow,
        CheckStatus nuget)
    {
        if (nuget == CheckStatus.Found)
        {
            return (
                "Package is published successfully.",
                "No action required."
            );
        }

        if (tag == CheckStatus.NotFound)
        {
            return (
                "Version was never tagged.",
                "Create a Git tag (e.g., git tag v{version}) and push it to trigger the release pipeline."
            );
        }

        if (release == CheckStatus.NotFound)
        {
            return (
                "Tag exists but GitHub release was not created.",
                "Create a GitHub release from the existing tag, or check if Release Please PR needs to be merged."
            );
        }

        if (workflow == WorkflowStatus.Failed)
        {
            return (
                "Release exists but publish workflow failed.",
                "Check publish-nuget.yml and NuGet trusted publishing / OIDC policy configuration."
            );
        }

        if (workflow == WorkflowStatus.Success)
        {
            return (
                "Publish may have skipped the package, used wrong package ID, or NuGet indexing is delayed.",
                "Verify the workflow logs, check that the package ID matches, and wait for NuGet indexing (up to 30 minutes)."
            );
        }

        return (
            "Release exists but no publish workflow run was found.",
            "Verify that the publish workflow is triggered by GitHub releases and check workflow configuration."
        );
    }
}
