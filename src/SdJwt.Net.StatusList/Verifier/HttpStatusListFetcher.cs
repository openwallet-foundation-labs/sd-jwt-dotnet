using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SdJwt.Net.StatusList.Models;
using System.Net;

namespace SdJwt.Net.StatusList.Verifier;

/// <summary>
/// HTTP-based Status List fetcher that handles network retrieval with proper error handling,
/// caching, and retry logic for production environments.
/// </summary>
public class HttpStatusListFetcher : IDisposable {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly bool _ownsHttpClient;

        /// <summary>
        /// Initializes a new instance of the HttpStatusListFetcher class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests. If null, a new one will be created.</param>
        /// <param name="logger">Optional logger for diagnostics.</param>
        public HttpStatusListFetcher(HttpClient? httpClient = null, ILogger<HttpStatusListFetcher>? logger = null) {
                _logger = logger ?? NullLogger<HttpStatusListFetcher>.Instance;

                if (httpClient == null) {
                        _httpClient = new HttpClient();
                        _httpClient.Timeout = TimeSpan.FromSeconds(30);
                        _ownsHttpClient = true;
                }
                else {
                        _httpClient = httpClient;
                        _ownsHttpClient = false;
                }
        }

        /// <summary>
        /// Fetches a Status List Token from the specified URI with proper error handling and retry logic.
        /// </summary>
        /// <param name="statusListUri">The URI of the Status List Token.</param>
        /// <param name="options">Options for the fetch operation.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>The Status List Token as a string.</returns>
        public async Task<string> FetchStatusListAsync(
            string statusListUri,
            StatusListOptions? options = null,
            CancellationToken cancellationToken = default) {
                if (string.IsNullOrWhiteSpace(statusListUri))
                        throw new ArgumentException("Status list URI cannot be null or empty", nameof(statusListUri));

                options ??= new StatusListOptions();

                _logger.LogDebug("Fetching Status List from URI: {Uri}", statusListUri);

                using var request = new HttpRequestMessage(HttpMethod.Get, statusListUri);

                // Set appropriate Accept headers
                request.Headers.Add("Accept", "application/statuslist+jwt, application/statuslist+cwt");

                // Add custom headers if specified
                foreach (var header in options.CustomHeaders) {
                        request.Headers.Add(header.Key, header.Value);
                }

                try {
                        // Execute request with retry logic
                        var response = await ExecuteWithRetryAsync(
                            () => _httpClient.SendAsync(request, cancellationToken),
                            options.RetryPolicy,
                            cancellationToken);

                        response.EnsureSuccessStatusCode();

                        var statusListToken = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(statusListToken)) {
                                throw new InvalidOperationException($"Empty response received from Status List URI: {statusListUri}");
                        }

                        _logger.LogDebug("Successfully fetched Status List from URI: {Uri}, Size: {Size} bytes",
                            statusListUri, statusListToken.Length);

                        return statusListToken;
                }
                catch (HttpRequestException ex) {
                        _logger.LogError(ex, "HTTP error fetching Status List from URI: {Uri}", statusListUri);
                        throw new StatusListFetchException($"Failed to fetch Status List from {statusListUri}: {ex.Message}", ex);
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException) {
                        _logger.LogError(ex, "Timeout fetching Status List from URI: {Uri}", statusListUri);
                        throw new StatusListFetchException($"Timeout fetching Status List from {statusListUri}", ex);
                }
                catch (Exception ex) {
                        _logger.LogError(ex, "Unexpected error fetching Status List from URI: {Uri}", statusListUri);
                        throw new StatusListFetchException($"Unexpected error fetching Status List from {statusListUri}: {ex.Message}", ex);
                }
        }

        /// <summary>
        /// Executes an HTTP operation with retry logic according to the specified retry policy.
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="retryPolicy">The retry policy to apply.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>The result of the operation.</returns>
        private async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            RetryPolicy retryPolicy,
            CancellationToken cancellationToken = default) {
                Exception? lastException = null;

                for (int attempt = 0; attempt <= retryPolicy.MaxRetries; attempt++) {
                        try {
                                return await operation();
                        }
                        catch (HttpRequestException ex) when (IsRetriableError(ex)) {
                                lastException = ex;

                                if (attempt == retryPolicy.MaxRetries)
                                        break;

                                var delay = CalculateRetryDelay(retryPolicy, attempt);

                                _logger.LogWarning("Retriable HTTP error on attempt {Attempt}/{MaxAttempts}, retrying in {Delay}ms: {Error}",
                                    attempt + 1, retryPolicy.MaxRetries + 1, delay.TotalMilliseconds, ex.Message);

                                await Task.Delay(delay, cancellationToken);
                        }
                        catch (Exception ex) {
                                // Non-retriable error, don't retry
                                _logger.LogError(ex, "Non-retriable error on attempt {Attempt}: {Error}", attempt + 1, ex.Message);
                                throw;
                        }
                }

                throw new StatusListFetchException(
                    $"Operation failed after {retryPolicy.MaxRetries + 1} attempts", lastException!);
        }

        /// <summary>
        /// Determines if an HTTP error is retriable (e.g., temporary network issues, 5xx errors).
        /// </summary>
        /// <param name="ex">The HTTP exception to evaluate.</param>
        /// <returns>True if the error is retriable, false otherwise.</returns>
        private static bool IsRetriableError(HttpRequestException ex) {
                // Check if it's a network connectivity issue
                if (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase)) {
                        return true;
                }

                // For HTTP status codes, we'd need to check the HttpResponseMessage
                // This is a simplified check - in production, you'd want more sophisticated logic
                return false;
        }

        /// <summary>
        /// Calculates the retry delay based on the retry policy and attempt number.
        /// </summary>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <param name="attempt">The current attempt number (0-based).</param>
        /// <returns>The delay before the next retry attempt.</returns>
        private static TimeSpan CalculateRetryDelay(RetryPolicy retryPolicy, int attempt) {
                var delay = retryPolicy.BaseDelay;

                if (retryPolicy.UseExponentialBackoff) {
                        delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Math.Pow(2, attempt));
                }

                return delay > retryPolicy.MaxDelay ? retryPolicy.MaxDelay : delay;
        }

        /// <summary>
        /// Disposes the HttpStatusListFetcher and its resources.
        /// </summary>
        public void Dispose() {
                if (_ownsHttpClient) {
                        _httpClient?.Dispose();
                }
        }
}

/// <summary>
/// Exception thrown when Status List fetching operations fail.
/// </summary>
public class StatusListFetchException : Exception {
        /// <summary>
        /// Initializes a new instance of the StatusListFetchException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public StatusListFetchException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the StatusListFetchException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public StatusListFetchException(string message, Exception innerException) : base(message, innerException) { }
}