using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using MsJsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace SdJwt.Net.Vc.Metadata;

/// <summary>
/// Resolves issuer signing keys for SD-JWT VC verification using JWT VC Issuer Metadata.
/// </summary>
public interface IJwtVcIssuerSigningKeyResolver {
        /// <summary>
        /// Resolves the issuer signing key for an unverified SD-JWT.
        /// </summary>
        /// <param name="token">Unverified SD-JWT token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The signing key to validate the issuer signature.</returns>
        Task<SecurityKey> ResolveSigningKeyAsync(JwtSecurityToken token, CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for issuer signing key resolution from metadata.
/// </summary>
public class JwtVcIssuerSigningKeyResolverOptions {
        /// <summary>
        /// Gets or sets the maximum JWKS response size in bytes.
        /// </summary>
        public int MaxJwksResponseBytes { get; set; } = 512 * 1024;

        /// <summary>
        /// Gets or sets a value indicating whether the JWT header <c>alg</c> must match JWK <c>alg</c> when present.
        /// </summary>
        public bool RequireMatchingAlg { get; set; } = false;
}

/// <summary>
/// Metadata-based issuer signing key resolver.
/// </summary>
public class JwtVcIssuerSigningKeyResolver(
    IJwtVcIssuerMetadataResolver metadataResolver,
    HttpClient httpClient,
    JwtVcIssuerSigningKeyResolverOptions? options = null) : IJwtVcIssuerSigningKeyResolver {
        private readonly IJwtVcIssuerMetadataResolver _metadataResolver = metadataResolver ?? throw new ArgumentNullException(nameof(metadataResolver));
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JwtVcIssuerSigningKeyResolverOptions _options = options ?? new JwtVcIssuerSigningKeyResolverOptions();

        /// <inheritdoc />
        public async Task<SecurityKey> ResolveSigningKeyAsync(JwtSecurityToken token, CancellationToken cancellationToken = default) {
                if (token == null) {
                        throw new ArgumentNullException(nameof(token));
                }

                var issuer = token.Payload.Iss;
                if (string.IsNullOrWhiteSpace(issuer)) {
                        throw new SecurityTokenException("Issuer signing key resolution requires the 'iss' claim.");
                }

                var metadataResult = await _metadataResolver.ResolveAsync(issuer, cancellationToken).ConfigureAwait(false);
                var jwkSet = metadataResult.Metadata.Jwks ?? await ResolveRemoteJwksAsync(metadataResult.Metadata.JwksUri!, cancellationToken).ConfigureAwait(false);

                return SelectSigningKey(token, jwkSet);
        }

        private async Task<Models.JwkSet> ResolveRemoteJwksAsync(string jwksUri, CancellationToken cancellationToken) {
                if (!Uri.TryCreate(jwksUri, UriKind.Absolute, out var uri) ||
                    !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
                        throw new SecurityTokenException("Issuer metadata 'jwks_uri' must be an absolute HTTPS URL.");
                }

                using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.OK) {
                        throw new SecurityTokenException($"JWKS endpoint returned HTTP {(int)response.StatusCode}.");
                }

                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (!string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase)) {
                        throw new SecurityTokenException("JWKS endpoint must return application/json.");
                }

                var payload = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                if (payload.Length > _options.MaxJwksResponseBytes) {
                        throw new SecurityTokenException("JWKS response exceeds configured size limit.");
                }

                var json = Encoding.UTF8.GetString(payload);
                var jwks = JsonSerializer.Deserialize<Models.JwkSet>(json, SdJwtConstants.DefaultJsonSerializerOptions);
                if (jwks?.Keys == null || jwks.Keys.Length == 0) {
                        throw new SecurityTokenException("JWKS must contain at least one key.");
                }

                return jwks;
        }

        private SecurityKey SelectSigningKey(JwtSecurityToken token, Models.JwkSet jwkSet) {
                var keys = jwkSet.Keys;
                if (keys == null) {
                        keys = Array.Empty<Models.JsonWebKey>();
                }
                if (keys.Length == 0) {
                        throw new SecurityTokenException("Issuer metadata does not provide any signing keys.");
                }

                var kid = token.Header.Kid;
                var alg = token.Header.Alg;

                var candidates = keys.Where(IsSigningCandidate).ToList();
                if (!string.IsNullOrWhiteSpace(kid)) {
                        candidates = candidates
                            .Where(k => string.Equals(k.KeyId, kid, StringComparison.Ordinal))
                            .ToList();
                }

                if (!string.IsNullOrWhiteSpace(alg)) {
                        if (_options.RequireMatchingAlg) {
                                candidates = candidates
                                    .Where(k => string.IsNullOrWhiteSpace(k.Algorithm) || string.Equals(k.Algorithm, alg, StringComparison.Ordinal))
                                    .ToList();
                        }
                        else {
                                var exactMatches = candidates
                                    .Where(k => string.Equals(k.Algorithm, alg, StringComparison.Ordinal))
                                    .ToList();
                                if (exactMatches.Count > 0) {
                                        candidates = exactMatches;
                                }
                        }
                }

                if (candidates.Count == 1) {
                        return ToSecurityKey(candidates[0]);
                }

                if (candidates.Count == 0) {
                        throw new SecurityTokenException("No issuer signing key matched token header constraints.");
                }

                throw new SecurityTokenException("Multiple issuer signing keys matched token header constraints.");
        }

        private static bool IsSigningCandidate(Models.JsonWebKey key) {
                if (key == null) {
                        return false;
                }

                if (!string.IsNullOrWhiteSpace(key.Use) &&
                    !string.Equals(key.Use, "sig", StringComparison.Ordinal)) {
                        return false;
                }

                if (key.KeyOperations != null && key.KeyOperations.Length > 0 &&
                    !key.KeyOperations.Contains("verify", StringComparer.Ordinal)) {
                        return false;
                }

                return true;
        }

        private static SecurityKey ToSecurityKey(Models.JsonWebKey key) {
                var json = JsonSerializer.Serialize(key, SdJwtConstants.DefaultJsonSerializerOptions);
                return new MsJsonWebKey(json);
        }
}
