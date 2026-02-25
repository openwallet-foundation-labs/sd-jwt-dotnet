using Microsoft.Extensions.Caching.Memory;

namespace SdJwt.Net.StatusList.Verifier;

/// <summary>
/// Provides configuration options for handling Status Lists during verification.
/// </summary>
public class StatusListOptions {
        /// <summary>
        /// Gets or sets whether status checking is enabled.
        /// Default is false to avoid network calls unless explicitly requested.
        /// </summary>
        public bool EnableStatusChecking { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to cache status lists in memory.
        /// Default is true for better performance.
        /// </summary>
        public bool CacheStatusLists { get; set; } = true;

        /// <summary>
        /// The duration for which a fetched Status List Credential should be cached in memory.
        /// Defaults to 5 minutes. Set to TimeSpan.Zero to disable caching.
        /// </summary>
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets the maximum number of status lists to cache.
        /// Default is 100 to prevent excessive memory usage.
        /// </summary>
        public int MaxCacheSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets whether verification should fail if status checking fails.
        /// If false, status check failures are logged but don't fail verification.
        /// Default is true for security.
        /// </summary>
        public bool FailOnStatusCheckError { get; set; } = true;

        /// <summary>
        /// Gets or sets the timeout for status list HTTP requests.
        /// Default is 10 seconds to prevent hanging requests.
        /// </summary>
        public TimeSpan StatusCheckTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// An HttpClient instance to use for fetching Status List Credentials.
        /// It is highly recommended to provide a shared/static instance to avoid socket exhaustion.
        /// If not provided, a new instance will be created per request.
        /// </summary>
        public HttpClient? HttpClient { get; set; }

        /// <summary>
        /// Gets or sets the memory cache to use for status list caching.
        /// If null and caching is enabled, a default MemoryCache will be created.
        /// </summary>
        public IMemoryCache? MemoryCache { get; set; }

        /// <summary>
        /// Gets or sets the allowed status purposes to check.
        /// If empty, all purposes are allowed. Use to restrict which statuses are checked.
        /// </summary>
        public HashSet<string> AllowedStatusPurposes { get; set; } = new();

        /// <summary>
        /// Gets or sets whether to validate the issuer of status lists.
        /// If true, the status list issuer must match the credential issuer.
        /// Default is true for security.
        /// </summary>
        public bool ValidateStatusListIssuer { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to validate the temporal validity of status lists.
        /// If true, status lists must be within their validity period.
        /// Default is true for security.
        /// </summary>
        public bool ValidateStatusListTiming { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum age for status lists.
        /// Status lists older than this will be considered invalid.
        /// Default is 24 hours.
        /// </summary>
        public TimeSpan MaxStatusListAge { get; set; } = TimeSpan.FromHours(24);

        /// <summary>
        /// Gets or sets custom headers to include in status list HTTP requests.
        /// Useful for authentication or other API requirements.
        /// </summary>
        public Dictionary<string, string> CustomHeaders { get; set; } = new();

        /// <summary>
        /// Gets or sets whether to use HTTP ETags for conditional requests.
        /// Helps reduce bandwidth when status lists haven't changed.
        /// Default is true.
        /// </summary>
        public bool UseConditionalRequests { get; set; } = true;

        /// <summary>
        /// Gets or sets the retry policy for failed status list requests.
        /// Specifies how many times to retry and with what delay.
        /// </summary>
        public RetryPolicy RetryPolicy { get; set; } = new();
}

/// <summary>
/// Represents a retry policy for status list requests.
/// </summary>
public class RetryPolicy {
        /// <summary>
        /// Gets or sets the maximum number of retry attempts.
        /// Default is 3 attempts.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets the base delay between retries.
        /// Default is 1 second.
        /// </summary>
        public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets or sets whether to use exponential backoff.
        /// If true, delay doubles with each retry.
        /// Default is true.
        /// </summary>
        public bool UseExponentialBackoff { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum delay between retries.
        /// Default is 10 seconds.
        /// </summary>
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(10);
}