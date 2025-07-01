namespace SdJwt.Net.Verifier;

/// <summary>
/// Provides configuration options for handling Status Lists during verification.
/// </summary>
public class StatusListOptions
{
    /// <summary>
    /// An HttpClient instance to use for fetching Status List Credentials.
    /// It is highly recommended to provide a shared/static instance to avoid socket exhaustion.
    /// If not provided, a new instance will be created per request.
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    /// The duration for which a fetched Status List Credential should be cached in memory.
    /// Defaults to 5 minutes. Set to TimeSpan.Zero to disable caching.
    /// </summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);
}