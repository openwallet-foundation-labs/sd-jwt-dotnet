using SdJwt.Net.Oid4Vci.AspNetCore.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Options;

/// <summary>
/// Configuration options for the OID4VCI Credential Issuer server.
/// </summary>
public sealed class CredentialIssuerOptions
{
    /// <summary>
    /// Gets or sets the base URL of the credential issuer (e.g. <c>https://issuer.example.com</c>).
    /// REQUIRED.
    /// </summary>
    [Required]
    [Url]
    public string IssuerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path at which the credential endpoint is hosted.
    /// Defaults to <c>/credential</c>.
    /// </summary>
    public string CredentialEndpointPath { get; set; } = "/credential";

    /// <summary>
    /// Gets or sets the path at which the token endpoint is hosted.
    /// Defaults to <c>/token</c>.
    /// </summary>
    public string TokenEndpointPath { get; set; } = "/token";

    /// <summary>
    /// Gets or sets the path at which the deferred credential endpoint is hosted.
    /// Defaults to <c>/deferred-credential</c>.
    /// </summary>
    public string DeferredCredentialEndpointPath { get; set; } = "/deferred-credential";

    /// <summary>
    /// Gets or sets the access token lifetime in seconds.
    /// Defaults to 300 seconds (5 minutes).
    /// </summary>
    [Range(1, int.MaxValue)]
    public int AccessTokenLifetimeSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the c_nonce lifetime in seconds.
    /// Defaults to 300 seconds (5 minutes).
    /// </summary>
    [Range(1, int.MaxValue)]
    public int CNonceLifetimeSeconds { get; set; } = 300;

    // ── caching ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets how long (in seconds) the issuer metadata document is cached in memory.
    /// Set to <c>0</c> to disable caching and rebuild on every request.
    /// Defaults to <c>300</c> (5 minutes).
    /// </summary>
    [Range(0, int.MaxValue)]
    public int CacheMetadataSeconds { get; set; } = 300;

    // ── rate limiting ──────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets whether to attach a rate-limiting policy to the token and credential endpoints.
    /// Requires <see cref="RateLimiterPolicyName"/> to be set and a matching policy registered via
    /// <c>builder.Services.AddRateLimiter(...)</c>.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool EnableRateLimiting { get; set; }

    /// <summary>
    /// Gets or sets the rate-limiter policy name to apply when <see cref="EnableRateLimiting"/> is <c>true</c>.
    /// Defaults to <c>"oid4vci"</c>.
    /// </summary>
    public string RateLimiterPolicyName { get; set; } = "oid4vci";

    // ── correlation ID ─────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets whether to read an inbound correlation / trace ID header and propagate it
    /// through logging scopes on every endpoint.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableCorrelationId { get; set; } = true;

    /// <summary>
    /// Gets or sets the HTTP request header used to carry the correlation ID.
    /// Defaults to <c>"X-Correlation-ID"</c>.
    /// </summary>
    public string CorrelationIdHeaderName { get; set; } = "X-Correlation-ID";

    // ── in-memory cleanup ──────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets how often (in seconds) the background cleanup service sweeps expired tokens
    /// when the in-memory token service is in use.
    /// Defaults to 60 seconds.
    /// </summary>
    [Range(5, int.MaxValue)]
    public int TokenCleanupIntervalSeconds { get; set; } = 60;

    // ── credential configurations ──────────────────────────────────────────

    /// <summary>
    /// Gets or sets the supported credential configurations keyed by configuration identifier.
    /// </summary>
    public Dictionary<string, JsonElement> CredentialConfigurationsSupported { get; set; } = new();

    /// <summary>
    /// Gets or sets optional authorization server URLs.
    /// When empty, the issuer itself acts as the authorization server.
    /// </summary>
    public string[] AuthorizationServers { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets optional display metadata for the issuer.
    /// </summary>
    public object[]? Display { get; set; }

    /// <summary>
    /// Builds the <see cref="CredentialIssuerMetadata"/> from the current options.
    /// </summary>
    /// <returns>The constructed metadata document.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required fields are not configured.</exception>
    public CredentialIssuerMetadata BuildMetadata()
    {
        if (string.IsNullOrWhiteSpace(IssuerUrl))
        {
            throw new InvalidOperationException("IssuerUrl is required.");
        }

        var credentialEndpoint = IssuerUrl.TrimEnd('/') + CredentialEndpointPath;
        var deferredEndpoint = IssuerUrl.TrimEnd('/') + DeferredCredentialEndpointPath;

        return new CredentialIssuerMetadata
        {
            CredentialIssuer = IssuerUrl,
            CredentialEndpoint = credentialEndpoint,
            DeferredCredentialEndpoint = deferredEndpoint,
            AuthorizationServers = AuthorizationServers.Length > 0 ? AuthorizationServers : null,
            CredentialConfigurationsSupported = CredentialConfigurationsSupported,
            Display = Display
        };
    }
}
