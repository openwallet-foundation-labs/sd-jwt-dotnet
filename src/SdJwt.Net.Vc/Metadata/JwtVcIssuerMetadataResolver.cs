using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using SdJwt.Net.Vc.Models;

namespace SdJwt.Net.Vc.Metadata;

/// <summary>
/// Resolves and validates JWT VC Issuer Metadata documents.
/// </summary>
public interface IJwtVcIssuerMetadataResolver
{
    /// <summary>
    /// Resolves and validates issuer metadata for the given issuer identifier.
    /// </summary>
    /// <param name="issuer">Issuer identifier (<c>iss</c>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validated issuer metadata result.</returns>
    Task<JwtVcIssuerMetadataResolutionResult> ResolveAsync(string issuer, CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for issuer metadata resolution.
/// </summary>
public class JwtVcIssuerMetadataResolverOptions
{
    /// <summary>
    /// Maximum response body size in bytes.
    /// </summary>
    public int MaxResponseBytes { get; set; } = 512 * 1024;

    /// <summary>
    /// Whether private/local hosts are allowed.
    /// </summary>
    public bool AllowPrivateHosts { get; set; } = false;
}

/// <summary>
/// Resolution output for JWT VC Issuer Metadata.
/// </summary>
/// <param name="Metadata">Validated metadata object.</param>
/// <param name="SourceUri">Source URI used to retrieve metadata.</param>
/// <param name="RawJson">Raw response JSON.</param>
public record JwtVcIssuerMetadataResolutionResult(JwtVcIssuerMetadata Metadata, Uri SourceUri, string RawJson);

/// <summary>
/// HTTP resolver for JWT VC Issuer Metadata.
/// </summary>
public class JwtVcIssuerMetadataResolver(HttpClient httpClient, JwtVcIssuerMetadataResolverOptions? options = null) : IJwtVcIssuerMetadataResolver
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly JwtVcIssuerMetadataResolverOptions _options = options ?? new JwtVcIssuerMetadataResolverOptions();

    /// <inheritdoc />
    public async Task<JwtVcIssuerMetadataResolutionResult> ResolveAsync(string issuer, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(issuer, UriKind.Absolute, out var issuerUri) ||
            !string.Equals(issuerUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(issuerUri.Query) ||
            !string.IsNullOrEmpty(issuerUri.Fragment))
        {
            throw new InvalidOperationException("Issuer identifier must be an HTTPS URL with no query or fragment.");
        }

        ValidateHostSafety(issuerUri.Host);
        var metadataUri = BuildWellKnownUri(issuerUri);
        ValidateHostSafety(metadataUri.Host);

        using var response = await _httpClient.GetAsync(metadataUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Issuer metadata endpoint returned HTTP {(int)response.StatusCode}.");
        }

        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Issuer metadata endpoint must return application/json.");
        }

        var payloadBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        if (payloadBytes.Length > _options.MaxResponseBytes)
        {
            throw new InvalidOperationException("Issuer metadata response exceeds configured size limit.");
        }

        var rawJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
        var metadata = JsonSerializer.Deserialize<JwtVcIssuerMetadata>(rawJson, SdJwtConstants.DefaultJsonSerializerOptions)
            ?? throw new InvalidOperationException("Issuer metadata response could not be parsed.");

        ValidateMetadata(metadata, issuer);
        return new JwtVcIssuerMetadataResolutionResult(metadata, metadataUri, rawJson);
    }

    private void ValidateMetadata(JwtVcIssuerMetadata metadata, string expectedIssuer)
    {
        if (string.IsNullOrWhiteSpace(metadata.Issuer))
        {
            throw new InvalidOperationException("Issuer metadata is missing required 'issuer' property.");
        }

        if (!string.Equals(metadata.Issuer, expectedIssuer, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Issuer metadata 'issuer' does not match credential 'iss'.");
        }

        var hasJwksUri = !string.IsNullOrWhiteSpace(metadata.JwksUri);
        var hasJwks = metadata.Jwks != null;
        if (hasJwksUri == hasJwks)
        {
            throw new InvalidOperationException("Issuer metadata must contain exactly one of 'jwks_uri' or 'jwks'.");
        }

        if (hasJwksUri)
        {
            if (!Uri.TryCreate(metadata.JwksUri, UriKind.Absolute, out var jwksUri) ||
                !string.Equals(jwksUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("'jwks_uri' must be an absolute HTTPS URL.");
            }
            ValidateHostSafety(jwksUri.Host);
        }

        if (hasJwks && (metadata.Jwks!.Keys == null || metadata.Jwks.Keys.Length == 0))
        {
            throw new InvalidOperationException("'jwks' must contain at least one key.");
        }
    }

    private static Uri BuildWellKnownUri(Uri issuerUri)
    {
        var issuerPath = issuerUri.AbsolutePath;
        if (issuerPath != "/")
        {
            issuerPath = issuerPath.TrimEnd('/');
        }
        else
        {
            issuerPath = string.Empty;
        }

        var builder = new UriBuilder(issuerUri)
        {
            Path = $"{SdJwtConstants.JwtVcIssuerWellKnownUri}{issuerPath}",
            Query = string.Empty,
            Fragment = string.Empty
        };
        return builder.Uri;
    }

    private void ValidateHostSafety(string host)
    {
        if (_options.AllowPrivateHosts)
        {
            return;
        }

        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Localhost endpoints are not allowed for metadata retrieval.");
        }

        if (IPAddress.TryParse(host, out var ipAddress) && IsPrivateAddress(ipAddress))
        {
            throw new InvalidOperationException("Private IP hosts are not allowed for metadata retrieval.");
        }
    }

    private static bool IsPrivateAddress(IPAddress ipAddress)
    {
        if (IPAddress.IsLoopback(ipAddress))
        {
            return true;
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ipAddress.GetAddressBytes();
            if (bytes[0] == 10)
                return true;
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;
            if (bytes[0] == 127)
                return true;
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6SiteLocal)
            {
                return true;
            }
        }

        return false;
    }
}
