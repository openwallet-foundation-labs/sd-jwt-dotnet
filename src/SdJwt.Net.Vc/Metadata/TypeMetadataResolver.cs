using System.Text.Json;
using SdJwt.Net.Vc.Models;

namespace SdJwt.Net.Vc.Metadata;

/// <summary>
/// Resolves and validates SD-JWT VC Type Metadata documents.
/// </summary>
public interface ITypeMetadataResolver {
        /// <summary>
        /// Resolves and validates type metadata by VCT.
        /// </summary>
        /// <param name="vct">Credential type identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The resolution result.</returns>
        Task<TypeMetadataResolutionResult> ResolveAsync(string vct, CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for type metadata resolution.
/// </summary>
public class TypeMetadataResolverOptions {
        /// <summary>
        /// Maximum response body size in bytes.
        /// </summary>
        public int MaxResponseBytes { get; set; } = 512 * 1024;

        /// <summary>
        /// Maximum number of type extensions to follow.
        /// </summary>
        public int MaxExtensionDepth { get; set; } = 8;

        /// <summary>
        /// Optional preloaded local metadata cache by VCT.
        /// </summary>
        public Dictionary<string, string> LocalTypeMetadataByVct { get; } = new(StringComparer.Ordinal);
}

/// <summary>
/// Resolution output for type metadata.
/// </summary>
/// <param name="Metadata">Resolved root metadata.</param>
/// <param name="SourceUri">Metadata source URI.</param>
/// <param name="RawJson">Raw metadata JSON.</param>
public record TypeMetadataResolutionResult(TypeMetadata Metadata, Uri SourceUri, string RawJson);

/// <summary>
/// HTTP-based type metadata resolver with validation of extension chains and integrity references.
/// </summary>
public class TypeMetadataResolver(HttpClient httpClient, TypeMetadataResolverOptions? options = null) : ITypeMetadataResolver {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly TypeMetadataResolverOptions _options = options ?? new TypeMetadataResolverOptions();

        /// <inheritdoc />
        public async Task<TypeMetadataResolutionResult> ResolveAsync(string vct, CancellationToken cancellationToken = default) {
                if (string.IsNullOrWhiteSpace(vct)) {
                        throw new ArgumentException("Value cannot be null or whitespace.", nameof(vct));
                }

                if (_options.LocalTypeMetadataByVct.TryGetValue(vct, out var cachedJson)) {
                        var (cachedMetadata, cachedUri) = ParseMetadata(cachedJson, CreatePseudoUri(vct));
                        await ValidateExtensionChainAsync(cachedMetadata, 0, new HashSet<string>(StringComparer.Ordinal), cancellationToken).ConfigureAwait(false);
                        return new TypeMetadataResolutionResult(cachedMetadata, cachedUri, cachedJson);
                }

                if (!Uri.TryCreate(vct, UriKind.Absolute, out var vctUri) ||
                    !string.Equals(vctUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
                        throw new InvalidOperationException("Type Metadata resolution requires VCT to be an absolute HTTPS URL or a local cached entry.");
                }

                var rawJson = await DownloadJsonAsync(vctUri, cancellationToken).ConfigureAwait(false);
                var (metadata, sourceUri) = ParseMetadata(rawJson, vctUri);

                if (!string.Equals(metadata.Vct, vct, StringComparison.Ordinal)) {
                        throw new InvalidOperationException("Type Metadata 'vct' does not match credential VCT.");
                }

                ValidateClaimMetadata(metadata);
                await ValidateExtensionChainAsync(metadata, 0, new HashSet<string>(StringComparer.Ordinal), cancellationToken).ConfigureAwait(false);
                return new TypeMetadataResolutionResult(metadata, sourceUri, rawJson);
        }

        private async Task ValidateExtensionChainAsync(TypeMetadata current, int depth, ISet<string> visited, CancellationToken cancellationToken) {
                if (string.IsNullOrWhiteSpace(current.Extends)) {
                        return;
                }

                if (depth >= _options.MaxExtensionDepth) {
                        throw new InvalidOperationException("Type metadata extension depth limit exceeded.");
                }

                if (!visited.Add(current.Vct ?? $"__vct_missing_{depth}")) {
                        throw new InvalidOperationException("Circular type metadata dependency detected.");
                }

                if (!Uri.TryCreate(current.Extends, UriKind.Absolute, out var extendsUri) ||
                    !string.Equals(extendsUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
                        throw new InvalidOperationException("Type metadata 'extends' must be an absolute HTTPS URL.");
                }

                var extendedRawJson = await DownloadJsonAsync(extendsUri, cancellationToken).ConfigureAwait(false);
                var (extendedMetadata, _) = ParseMetadata(extendedRawJson, extendsUri);
                ValidateClaimMetadata(extendedMetadata);

                if (!string.IsNullOrWhiteSpace(current.ExtendsIntegrity) &&
                    !IntegrityMetadataValidator.Validate(extendedRawJson, current.ExtendsIntegrity)) {
                        throw new InvalidOperationException("Type metadata 'extends#integrity' validation failed.");
                }

                if (string.Equals(extendedMetadata.Vct, current.Vct, StringComparison.Ordinal)) {
                        throw new InvalidOperationException("Circular type metadata dependency detected.");
                }

                await ValidateExtensionChainAsync(extendedMetadata, depth + 1, visited, cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> DownloadJsonAsync(Uri uri, CancellationToken cancellationToken) {
                using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode) {
                        throw new InvalidOperationException($"Type metadata endpoint returned HTTP {(int)response.StatusCode}.");
                }

                var mediaType = response.Content.Headers.ContentType?.MediaType;
                if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase)) {
                        throw new InvalidOperationException("Type metadata endpoint must return application/json.");
                }

                var payload = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                if (payload.Length > _options.MaxResponseBytes) {
                        throw new InvalidOperationException("Type metadata response exceeds configured size limit.");
                }

                return System.Text.Encoding.UTF8.GetString(payload);
        }

        private static (TypeMetadata Metadata, Uri SourceUri) ParseMetadata(string rawJson, Uri sourceUri) {
                var metadata = JsonSerializer.Deserialize<TypeMetadata>(rawJson, SdJwtConstants.DefaultJsonSerializerOptions)
                    ?? throw new InvalidOperationException("Type metadata response could not be parsed.");
                if (string.IsNullOrWhiteSpace(metadata.Vct)) {
                        throw new InvalidOperationException("Type metadata is missing required 'vct' property.");
                }
                return (metadata, sourceUri);
        }

        private static void ValidateClaimMetadata(TypeMetadata metadata) {
                if (metadata.Claims == null) {
                        return;
                }

                var svgIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var claim in metadata.Claims) {
                        if (claim.Path == null || claim.Path.Length == 0) {
                                throw new InvalidOperationException("Claim metadata 'path' must be a non-empty array.");
                        }

                        foreach (var segment in claim.Path) {
                                if (segment is null || segment is string) {
                                        continue;
                                }

                                if (segment is int intValue && intValue >= 0) {
                                        continue;
                                }

                                if (segment is long longValue && longValue >= 0) {
                                        continue;
                                }

                                throw new InvalidOperationException("Claim metadata 'path' entries must be string, null, or non-negative integer.");
                        }

                        if (!string.IsNullOrWhiteSpace(claim.SelectiveDisclosure) &&
                            claim.SelectiveDisclosure is not ("always" or "allowed" or "never")) {
                                throw new InvalidOperationException("Claim metadata 'sd' must be one of: always, allowed, never.");
                        }

                        if (!string.IsNullOrWhiteSpace(claim.SvgId)) {
                                if (!IsValidSvgId(claim.SvgId)) {
                                        throw new InvalidOperationException("Claim metadata 'svg_id' must match ^[A-Za-z_][A-Za-z0-9_]*$.");
                                }

                                if (!svgIds.Add(claim.SvgId)) {
                                        throw new InvalidOperationException("Claim metadata 'svg_id' values must be unique.");
                                }
                        }
                }
        }

        private static bool IsValidSvgId(string svgId) {
                if (string.IsNullOrWhiteSpace(svgId)) {
                        return false;
                }

                if (!(char.IsLetter(svgId[0]) || svgId[0] == '_')) {
                        return false;
                }

                for (var i = 1; i < svgId.Length; i++) {
                        var c = svgId[i];
                        if (!(char.IsLetterOrDigit(c) || c == '_')) {
                                return false;
                        }
                }

                return true;
        }

        private static Uri CreatePseudoUri(string vct) {
                var safe = Uri.EscapeDataString(vct);
                return new Uri($"https://local.cache/{safe}");
        }
}
