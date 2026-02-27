using System.Text.Json;
using System.Text;
using SdJwt.Net.Vc.Models;

namespace SdJwt.Net.Vc.Metadata;

/// <summary>
/// Resolves and validates SD-JWT VC Type Metadata documents.
/// </summary>
public interface ITypeMetadataResolver
{
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
public class TypeMetadataResolverOptions
{
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

    /// <summary>
    /// Whether to validate display and rendering metadata structures.
    /// </summary>
    public bool ValidateDisplayMetadata { get; set; } = true;

    /// <summary>
    /// Maximum text length accepted for display strings (name/label/description/alt text).
    /// </summary>
    public int MaxDisplayTextLength { get; set; } = 512;

    /// <summary>
    /// Maximum size of remote rendering resources downloaded for integrity validation.
    /// </summary>
    public int MaxRenderingResourceBytes { get; set; } = 512 * 1024;

    /// <summary>
    /// Gets or sets a value indicating whether remote rendering resources should be downloaded when integrity metadata is present.
    /// </summary>
    public bool ResolveRemoteRenderingResourcesWithIntegrity { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether remote rendering resources must include integrity metadata.
    /// </summary>
    public bool RequireIntegrityForRemoteRenderingResources
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether extension claim compatibility merge checks are enforced.
    /// </summary>
    public bool EnforceExtensionMergeSemantics { get; set; } = true;
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
public class TypeMetadataResolver(HttpClient httpClient, TypeMetadataResolverOptions? options = null) : ITypeMetadataResolver
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly TypeMetadataResolverOptions _options = options ?? new TypeMetadataResolverOptions();

    /// <inheritdoc />
    public async Task<TypeMetadataResolutionResult> ResolveAsync(string vct, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vct))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(vct));
        }

        if (_options.LocalTypeMetadataByVct.TryGetValue(vct, out var cachedJson))
        {
            var (cachedMetadata, cachedUri) = ParseMetadata(cachedJson, CreatePseudoUri(vct));
            ValidateClaimMetadata(cachedMetadata);
            await ValidateDisplayAndRenderingMetadataAsync(cachedMetadata, cancellationToken).ConfigureAwait(false);
            await ValidateExtensionChainAsync(cachedMetadata, 0, new HashSet<string>(StringComparer.Ordinal), cancellationToken).ConfigureAwait(false);
            return new TypeMetadataResolutionResult(cachedMetadata, cachedUri, cachedJson);
        }

        if (!Uri.TryCreate(vct, UriKind.Absolute, out var vctUri) ||
            !string.Equals(vctUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Type Metadata resolution requires VCT to be an absolute HTTPS URL or a local cached entry.");
        }

        var rawJson = await DownloadJsonAsync(vctUri, cancellationToken).ConfigureAwait(false);
        var (metadata, sourceUri) = ParseMetadata(rawJson, vctUri);

        if (!string.Equals(metadata.Vct, vct, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Type Metadata 'vct' does not match credential VCT.");
        }

        ValidateClaimMetadata(metadata);
        await ValidateDisplayAndRenderingMetadataAsync(metadata, cancellationToken).ConfigureAwait(false);
        await ValidateExtensionChainAsync(metadata, 0, new HashSet<string>(StringComparer.Ordinal), cancellationToken).ConfigureAwait(false);
        return new TypeMetadataResolutionResult(metadata, sourceUri, rawJson);
    }

    private async Task ValidateExtensionChainAsync(TypeMetadata current, int depth, ISet<string> visited, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(current.Extends))
        {
            return;
        }

        if (depth >= _options.MaxExtensionDepth)
        {
            throw new InvalidOperationException("Type metadata extension depth limit exceeded.");
        }

        if (!visited.Add(current.Vct ?? $"__vct_missing_{depth}"))
        {
            throw new InvalidOperationException("Circular type metadata dependency detected.");
        }

        if (!Uri.TryCreate(current.Extends, UriKind.Absolute, out var extendsUri) ||
            !string.Equals(extendsUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Type metadata 'extends' must be an absolute HTTPS URL.");
        }

        var extendedRawJson = await DownloadJsonAsync(extendsUri, cancellationToken).ConfigureAwait(false);
        var (extendedMetadata, _) = ParseMetadata(extendedRawJson, extendsUri);
        ValidateClaimMetadata(extendedMetadata);
        await ValidateDisplayAndRenderingMetadataAsync(extendedMetadata, cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(current.ExtendsIntegrity) &&
            !IntegrityMetadataValidator.Validate(extendedRawJson, current.ExtendsIntegrity))
        {
            throw new InvalidOperationException("Type metadata 'extends#integrity' validation failed.");
        }

        if (string.Equals(extendedMetadata.Vct, current.Vct, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Circular type metadata dependency detected.");
        }

        if (_options.EnforceExtensionMergeSemantics)
        {
            ValidateExtensionClaimCompatibility(current, extendedMetadata);
        }

        await ValidateExtensionChainAsync(extendedMetadata, depth + 1, visited, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> DownloadJsonAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Type metadata endpoint returned HTTP {(int)response.StatusCode}.");
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Type metadata endpoint must return application/json.");
        }

        var payload = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        if (payload.Length > _options.MaxResponseBytes)
        {
            throw new InvalidOperationException("Type metadata response exceeds configured size limit.");
        }

        return System.Text.Encoding.UTF8.GetString(payload);
    }

    private static (TypeMetadata Metadata, Uri SourceUri) ParseMetadata(string rawJson, Uri sourceUri)
    {
        var metadata = JsonSerializer.Deserialize<TypeMetadata>(rawJson, SdJwtConstants.DefaultJsonSerializerOptions)
            ?? throw new InvalidOperationException("Type metadata response could not be parsed.");
        if (string.IsNullOrWhiteSpace(metadata.Vct))
        {
            throw new InvalidOperationException("Type metadata is missing required 'vct' property.");
        }
        return (metadata, sourceUri);
    }

    private static void ValidateClaimMetadata(TypeMetadata metadata)
    {
        if (metadata.Claims == null)
        {
            return;
        }

        var svgIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var claim in metadata.Claims)
        {
            if (claim.Path == null || claim.Path.Length == 0)
            {
                throw new InvalidOperationException("Claim metadata 'path' must be a non-empty array.");
            }

            foreach (var segment in claim.Path)
            {
                if (segment is null || segment is string)
                {
                    continue;
                }

                if (segment is int intValue && intValue >= 0)
                {
                    continue;
                }

                if (segment is long longValue && longValue >= 0)
                {
                    continue;
                }

                throw new InvalidOperationException("Claim metadata 'path' entries must be string, null, or non-negative integer.");
            }

            if (!string.IsNullOrWhiteSpace(claim.SelectiveDisclosure) &&
                claim.SelectiveDisclosure is not ("always" or "allowed" or "never"))
            {
                throw new InvalidOperationException("Claim metadata 'sd' must be one of: always, allowed, never.");
            }

            if (!string.IsNullOrWhiteSpace(claim.SvgId))
            {
                if (!IsValidSvgId(claim.SvgId))
                {
                    throw new InvalidOperationException("Claim metadata 'svg_id' must match ^[A-Za-z_][A-Za-z0-9_]*$.");
                }

                if (!svgIds.Add(claim.SvgId))
                {
                    throw new InvalidOperationException("Claim metadata 'svg_id' values must be unique.");
                }
            }
        }
    }

    private async Task ValidateDisplayAndRenderingMetadataAsync(TypeMetadata metadata, CancellationToken cancellationToken)
    {
        if (!_options.ValidateDisplayMetadata)
        {
            return;
        }

        await ValidateDisplayEntriesAsync(metadata.Display, requireName: true, requireLabel: false, cancellationToken).ConfigureAwait(false);

        if (metadata.Claims == null)
        {
            return;
        }

        foreach (var claim in metadata.Claims)
        {
            await ValidateDisplayEntriesAsync(claim.Display, requireName: false, requireLabel: true, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ValidateDisplayEntriesAsync(
        DisplayMetadata[]? displayEntries,
        bool requireName,
        bool requireLabel,
        CancellationToken cancellationToken)
    {
        if (displayEntries == null)
        {
            return;
        }

        var locales = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in displayEntries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(entry.Locale) || !IsValidLanguageTag(entry.Locale))
            {
                throw new InvalidOperationException("Display metadata 'locale' must be a valid RFC5646 language tag.");
            }

            if (!locales.Add(entry.Locale))
            {
                throw new InvalidOperationException("Display metadata must not contain duplicate locales.");
            }

            if (requireName && string.IsNullOrWhiteSpace(entry.Name))
            {
                throw new InvalidOperationException("Display metadata for type entries must contain 'name'.");
            }

            if (requireLabel && string.IsNullOrWhiteSpace(entry.Label))
            {
                throw new InvalidOperationException("Display metadata for claim entries must contain 'label'.");
            }

            ValidateDisplayText(entry.Name, "name");
            ValidateDisplayText(entry.Label, "label");
            ValidateDisplayText(entry.Description, "description");

            await ValidateRenderingAsync(entry.Rendering, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ValidateRenderingAsync(RenderingMetadata? rendering, CancellationToken cancellationToken)
    {
        if (rendering == null)
        {
            return;
        }

        var hasSimple = rendering.Simple != null;
        var hasSvgTemplates = rendering.SvgTemplates != null && rendering.SvgTemplates.Length > 0;
        if (!hasSimple && !hasSvgTemplates)
        {
            throw new InvalidOperationException("Rendering metadata must include at least one rendering method.");
        }

        if (rendering.Simple != null)
        {
            await ValidateImageReferenceAsync(rendering.Simple.Logo, allowAltText: true, "rendering.simple.logo", cancellationToken).ConfigureAwait(false);
            await ValidateImageReferenceAsync(rendering.Simple.BackgroundImage, allowAltText: false, "rendering.simple.background_image", cancellationToken).ConfigureAwait(false);
            ValidateColor(rendering.Simple.BackgroundColor, "rendering.simple.background_color");
            ValidateColor(rendering.Simple.TextColor, "rendering.simple.text_color");
        }

        if (rendering.SvgTemplates == null)
        {
            return;
        }

        foreach (var template in rendering.SvgTemplates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(template.Uri))
            {
                throw new InvalidOperationException("SVG template must include 'uri'.");
            }

            var (isDataUri, mediaType, content, remoteUri) = ParseSupportedUri(template.Uri, "rendering.svg_templates[].uri");

            if (remoteUri != null &&
                _options.RequireIntegrityForRemoteRenderingResources &&
                string.IsNullOrWhiteSpace(template.UriIntegrity))
            {
                throw new InvalidOperationException("Remote SVG template resources must include 'uri#integrity' metadata.");
            }

            if (!string.IsNullOrWhiteSpace(template.UriIntegrity))
            {
                if (isDataUri)
                {
                    if (content == null || !IntegrityMetadataValidator.Validate(content, template.UriIntegrity))
                    {
                        throw new InvalidOperationException("SVG template 'uri#integrity' validation failed.");
                    }
                }
                else if (remoteUri != null && _options.ResolveRemoteRenderingResourcesWithIntegrity)
                {
                    content = await DownloadResourceAsync(remoteUri, cancellationToken).ConfigureAwait(false);
                    if (!IntegrityMetadataValidator.Validate(content, template.UriIntegrity))
                    {
                        throw new InvalidOperationException("SVG template 'uri#integrity' validation failed.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("SVG template 'uri#integrity' cannot be validated for remote URI without resolver support.");
                }
            }

            if (isDataUri && content != null)
            {
                if (!string.Equals(mediaType, "image/svg+xml", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("SVG template data URI must use media type image/svg+xml.");
                }

                ValidateSvgTemplateContent(content);
            }
            else if (remoteUri != null && content != null)
            {
                var inferredSvg = string.Equals(mediaType, "image/svg+xml", StringComparison.OrdinalIgnoreCase) ||
                                  remoteUri.AbsolutePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase);
                if (inferredSvg)
                {
                    ValidateSvgTemplateContent(content);
                }
            }

            if (template.Properties != null)
            {
                ValidateSvgTemplateProperties(template.Properties);
            }
        }
    }

    private async Task ValidateImageReferenceAsync(
        LogoMetadata? image,
        bool allowAltText,
        string context,
        CancellationToken cancellationToken)
    {
        if (image == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(image.Uri))
        {
            throw new InvalidOperationException($"{context} must include 'uri'.");
        }

        var (isDataUri, mediaType, content, remoteUri) = ParseSupportedUri(image.Uri, $"{context}.uri");
        if (remoteUri != null &&
            _options.RequireIntegrityForRemoteRenderingResources &&
            string.IsNullOrWhiteSpace(image.UriIntegrity))
        {
            throw new InvalidOperationException($"{context} remote URI must include 'uri#integrity'.");
        }

        if (!string.IsNullOrWhiteSpace(image.UriIntegrity))
        {
            if (isDataUri)
            {
                if (content == null || !IntegrityMetadataValidator.Validate(content, image.UriIntegrity))
                {
                    throw new InvalidOperationException($"{context} 'uri#integrity' validation failed.");
                }
            }
            else if (remoteUri != null && _options.ResolveRemoteRenderingResourcesWithIntegrity)
            {
                content = await DownloadResourceAsync(remoteUri, cancellationToken).ConfigureAwait(false);
                if (!IntegrityMetadataValidator.Validate(content, image.UriIntegrity))
                {
                    throw new InvalidOperationException($"{context} 'uri#integrity' validation failed.");
                }
            }
            else
            {
                throw new InvalidOperationException($"{context} 'uri#integrity' cannot be validated for remote URI without resolver support.");
            }
        }

        if (content != null)
        {
            var inferredSvg = string.Equals(mediaType, "image/svg+xml", StringComparison.OrdinalIgnoreCase) ||
                              (remoteUri != null && remoteUri.AbsolutePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase));
            if (inferredSvg)
            {
                ValidateSvgTemplateContent(content);
            }
        }

        if (allowAltText)
        {
            ValidateDisplayText(image.AltText, $"{context}.alt_text");
        }
        else if (!string.IsNullOrWhiteSpace(image.AltText))
        {
            ValidateDisplayText(image.AltText, $"{context}.alt_text");
        }
    }

    private static void ValidateSvgTemplateProperties(SvgTemplateProperties properties)
    {
        if (!string.IsNullOrWhiteSpace(properties.Orientation) &&
            properties.Orientation is not ("portrait" or "landscape"))
        {
            throw new InvalidOperationException("SVG template property 'orientation' must be 'portrait' or 'landscape'.");
        }

        if (!string.IsNullOrWhiteSpace(properties.ColorScheme) &&
            properties.ColorScheme is not ("light" or "dark"))
        {
            throw new InvalidOperationException("SVG template property 'color_scheme' must be 'light' or 'dark'.");
        }

        if (!string.IsNullOrWhiteSpace(properties.Contrast) &&
            properties.Contrast is not ("normal" or "high"))
        {
            throw new InvalidOperationException("SVG template property 'contrast' must be 'normal' or 'high'.");
        }
    }

    private void ValidateDisplayText(string? value, string context)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        if (value.Length > _options.MaxDisplayTextLength)
        {
            throw new InvalidOperationException($"Display text '{context}' exceeds maximum length {_options.MaxDisplayTextLength}.");
        }
    }

    private static bool IsValidLanguageTag(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return false;
        }

        var parts = locale.Split('-');
        if (parts.Length == 0 || parts[0].Length < 2 || parts[0].Length > 8)
        {
            return false;
        }

        foreach (var part in parts)
        {
            if (part.Length == 0 || part.Length > 8)
            {
                return false;
            }

            for (var i = 0; i < part.Length; i++)
            {
                if (!char.IsLetterOrDigit(part[i]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static void ValidateColor(string? color, string context)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return;
        }

        var isShortHex = color.Length == 4 && color[0] == '#';
        var isLongHex = color.Length == 7 && color[0] == '#';
        if (!isShortHex && !isLongHex)
        {
            throw new InvalidOperationException($"{context} must be a hexadecimal RGB color (e.g., #FFF or #112233).");
        }

        for (var i = 1; i < color.Length; i++)
        {
            var c = color[i];
            var isHex = (c >= '0' && c <= '9') ||
                        (c >= 'a' && c <= 'f') ||
                        (c >= 'A' && c <= 'F');
            if (!isHex)
            {
                throw new InvalidOperationException($"{context} must be a hexadecimal RGB color (e.g., #FFF or #112233).");
            }
        }
    }

    private static (bool IsDataUri, string? MediaType, byte[]? Content, Uri? RemoteUri) ParseSupportedUri(string value, string context)
    {
        if (value.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return ParseDataUri(value, context);
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"{context} must be an absolute HTTPS URL or data URI.");
        }

        return (false, null, null, uri);
    }

    private static (bool IsDataUri, string? MediaType, byte[]? Content, Uri? RemoteUri) ParseDataUri(string dataUri, string context)
    {
        var commaIndex = dataUri.IndexOf(',');
        if (commaIndex <= 5 || commaIndex == dataUri.Length - 1)
        {
            throw new InvalidOperationException($"{context} is not a valid data URI.");
        }

        var metadata = dataUri.Substring(5, commaIndex - 5);
        var data = dataUri[(commaIndex + 1)..];
        var parts = metadata.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var mediaType = parts.Length > 0 ? parts[0] : "text/plain";
        var isBase64 = parts.Any(p => string.Equals(p, "base64", StringComparison.OrdinalIgnoreCase));

        try
        {
            byte[] bytes = isBase64
                ? Convert.FromBase64String(data)
                : Encoding.UTF8.GetBytes(Uri.UnescapeDataString(data));
            return (true, mediaType, bytes, null);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"{context} contains invalid data URI payload.", ex);
        }
    }

    private async Task<byte[]> DownloadResourceAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Rendering resource endpoint returned HTTP {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        if (payload.Length > _options.MaxRenderingResourceBytes)
        {
            throw new InvalidOperationException("Rendering resource exceeds configured size limit.");
        }

        return payload;
    }

    private static void ValidateSvgTemplateContent(byte[] content)
    {
        var text = Encoding.UTF8.GetString(content);
        var normalized = text.ToLowerInvariant();

        var blockedTokens = new[]
        {
                    "<script",
                    "javascript:",
                    "onload=",
                    "onerror=",
                    "onclick=",
                    "<iframe",
                    "<object",
                    "<embed",
                    "<foreignobject"
                };

        foreach (var token in blockedTokens)
        {
            if (normalized.Contains(token, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("SVG template contains blocked executable or active content.");
            }
        }
    }

    private static bool IsValidSvgId(string svgId)
    {
        if (string.IsNullOrWhiteSpace(svgId))
        {
            return false;
        }

        if (!(char.IsLetter(svgId[0]) || svgId[0] == '_'))
        {
            return false;
        }

        for (var i = 1; i < svgId.Length; i++)
        {
            var c = svgId[i];
            if (!(char.IsLetterOrDigit(c) || c == '_'))
            {
                return false;
            }
        }

        return true;
    }

    private static Uri CreatePseudoUri(string vct)
    {
        var safe = Uri.EscapeDataString(vct);
        return new Uri($"https://local.cache/{safe}");
    }

    private static void ValidateExtensionClaimCompatibility(TypeMetadata derivedMetadata, TypeMetadata baseMetadata)
    {
        if (derivedMetadata.Claims == null || baseMetadata.Claims == null)
        {
            return;
        }

        var baseClaimsByPath = new Dictionary<string, ClaimMetadata>(StringComparer.Ordinal);
        foreach (var claim in baseMetadata.Claims)
        {
            var key = BuildClaimPathKey(claim.Path);
            if (!string.IsNullOrWhiteSpace(key))
            {
                baseClaimsByPath[key] = claim;
            }
        }

        foreach (var derivedClaim in derivedMetadata.Claims)
        {
            var key = BuildClaimPathKey(derivedClaim.Path);
            if (string.IsNullOrWhiteSpace(key) || !baseClaimsByPath.TryGetValue(key, out var baseClaim))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(baseClaim.SelectiveDisclosure) &&
                !string.IsNullOrWhiteSpace(derivedClaim.SelectiveDisclosure) &&
                !string.Equals(baseClaim.SelectiveDisclosure, derivedClaim.SelectiveDisclosure, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Type metadata extension conflict detected for claim 'sd' rule.");
            }

            if (baseClaim.Mandatory == true && derivedClaim.Mandatory != true)
            {
                throw new InvalidOperationException("Type metadata extension conflict detected: mandatory claim cannot be relaxed.");
            }
        }
    }

    private static string BuildClaimPathKey(object[]? path)
    {
        if (path == null || path.Length == 0)
        {
            return string.Empty;
        }

        var parts = new List<string>(path.Length);
        foreach (var segment in path)
        {
            if (segment == null)
            {
                parts.Add("*");
                continue;
            }

            parts.Add(segment.ToString() ?? string.Empty);
        }

        return string.Join("/", parts);
    }
}
