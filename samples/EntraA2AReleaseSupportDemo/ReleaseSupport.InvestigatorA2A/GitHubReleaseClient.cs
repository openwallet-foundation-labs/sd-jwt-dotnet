using System.Net.Http.Headers;
using System.Text.Json;

namespace ReleaseSupport.InvestigatorA2A;

/// <summary>
/// Client for GitHub REST API checks.
/// </summary>
public class GitHubReleaseClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubReleaseClient> _logger;

    /// <summary>
    /// Initializes the GitHub client.
    /// </summary>
    public GitHubReleaseClient(HttpClient httpClient, ILogger<GitHubReleaseClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Checks whether a Git tag exists for the given version.
    /// </summary>
    public async Task<bool> TagExistsAsync(string owner, string repo, string version, CancellationToken ct = default)
    {
        var url = $"https://api.github.com/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/git/ref/tags/v{Uri.EscapeDataString(version)}";
        using var request = CreateRequest(url);
        using var response = await _httpClient.SendAsync(request, ct);

        _logger.LogDebug("GitHub tag check {Url} returned {Status}", url, response.StatusCode);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Checks whether a GitHub release exists for the given version.
    /// </summary>
    public async Task<bool> ReleaseExistsAsync(string owner, string repo, string version, CancellationToken ct = default)
    {
        var url = $"https://api.github.com/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/releases/tags/v{Uri.EscapeDataString(version)}";
        using var request = CreateRequest(url);
        using var response = await _httpClient.SendAsync(request, ct);

        _logger.LogDebug("GitHub release check {Url} returned {Status}", url, response.StatusCode);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Checks the latest publish workflow run status.
    /// Returns <c>true</c> if the latest relevant run succeeded, <c>false</c> if it failed,
    /// or <c>null</c> if no workflow or run was found.
    /// </summary>
    public async Task<bool?> GetPublishWorkflowStatusAsync(string owner, string repo, CancellationToken ct = default)
    {
        var workflowsUrl = $"https://api.github.com/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/actions/workflows";
        using var wfRequest = CreateRequest(workflowsUrl);
        using var wfResponse = await _httpClient.SendAsync(wfRequest, ct);

        if (!wfResponse.IsSuccessStatusCode)
        {
            _logger.LogDebug("GitHub workflows check returned {Status}", wfResponse.StatusCode);
            return null;
        }

        var wfContent = await wfResponse.Content.ReadAsStringAsync(ct);
        using var wfDoc = JsonDocument.Parse(wfContent);

        long? publishWorkflowId = null;
        if (wfDoc.RootElement.TryGetProperty("workflows", out var workflows))
        {
            foreach (var wf in workflows.EnumerateArray())
            {
                var name = wf.GetProperty("name").GetString() ?? string.Empty;
                var path = wf.GetProperty("path").GetString() ?? string.Empty;

                if (name.Contains("publish", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("nuget", StringComparison.OrdinalIgnoreCase) ||
                    path.Contains("publish", StringComparison.OrdinalIgnoreCase))
                {
                    publishWorkflowId = wf.GetProperty("id").GetInt64();
                    break;
                }
            }
        }

        if (publishWorkflowId == null)
        {
            _logger.LogDebug("No publish workflow found");
            return null;
        }

        var runsUrl = $"https://api.github.com/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/actions/workflows/{publishWorkflowId}/runs?per_page=5";
        using var runsRequest = CreateRequest(runsUrl);
        using var runsResponse = await _httpClient.SendAsync(runsRequest, ct);

        if (!runsResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var runsContent = await runsResponse.Content.ReadAsStringAsync(ct);
        using var runsDoc = JsonDocument.Parse(runsContent);

        if (runsDoc.RootElement.TryGetProperty("workflow_runs", out var runs))
        {
            foreach (var run in runs.EnumerateArray())
            {
                var conclusion = run.GetProperty("conclusion").GetString();
                if (string.Equals(conclusion, "success", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (string.Equals(conclusion, "failure", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
        }

        return null;
    }

    private static HttpRequestMessage CreateRequest(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("SdJwtNet-ReleaseSupport", "1.0"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return request;
    }
}
