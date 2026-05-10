using System.IO.Compression;
using System.Text.Json;

namespace ReleaseSupport.InvestigatorA2A;

/// <summary>
/// Client for NuGet V3 API checks.
/// </summary>
public class NuGetPackageClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NuGetPackageClient> _logger;

    /// <summary>
    /// Initializes the NuGet client.
    /// </summary>
    public NuGetPackageClient(HttpClient httpClient, ILogger<NuGetPackageClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Checks whether a specific version of a package exists on NuGet.org.
    /// </summary>
    public async Task<bool> PackageVersionExistsAsync(string packageId, string version, CancellationToken ct = default)
    {
        var lowerId = packageId.ToLowerInvariant();
        var url = $"https://api.nuget.org/v3/registration5-gz-semver2/{Uri.EscapeDataString(lowerId)}/index.json";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.AcceptEncoding.ParseAdd("gzip");

        using var response = await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogDebug("NuGet registration check for {PackageId} returned {Status}", packageId, response.StatusCode);
            return false;
        }

        string content;
        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        {
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            await using var gzip = new GZipStream(stream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip);
            content = await reader.ReadToEndAsync(ct);
        }
        else
        {
            content = await response.Content.ReadAsStringAsync(ct);
        }

        using var doc = JsonDocument.Parse(content);

        if (!doc.RootElement.TryGetProperty("items", out var pages))
        {
            return false;
        }

        foreach (var page in pages.EnumerateArray())
        {
            if (page.TryGetProperty("items", out var entries))
            {
                foreach (var entry in entries.EnumerateArray())
                {
                    if (entry.TryGetProperty("catalogEntry", out var catalog) &&
                        catalog.TryGetProperty("version", out var ver))
                    {
                        if (string.Equals(ver.GetString(), version, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogDebug("NuGet package {PackageId} version {Version} found", packageId, version);
                            return true;
                        }
                    }
                }
            }
            else
            {
                // Page without inline items; would need to fetch page URL.
                // For this demo, we check inline items only.
                _logger.LogDebug("NuGet registration page without inline items, skipping");
            }
        }

        _logger.LogDebug("NuGet package {PackageId} version {Version} not found", packageId, version);
        return false;
    }
}
