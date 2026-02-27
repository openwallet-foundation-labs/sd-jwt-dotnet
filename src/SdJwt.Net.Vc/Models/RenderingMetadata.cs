using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents the rendering metadata for a credential type as defined in draft-ietf-oauth-sd-jwt-vc-14.
/// Contains both simple display metadata and SVG template references for rendering credentials.
/// </summary>
public class RenderingMetadata
{
    /// <summary>
    /// Gets or sets the simple rendering metadata.
    /// Optional. Contains basic display properties such as logo, background color, and text color.
    /// </summary>
    [JsonPropertyName("simple")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public SimpleRenderingMetadata? Simple
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the SVG template references for rendering the credential.
    /// Optional. Array of SVG template objects, each identifying a template for a specific locale or properties.
    /// </summary>
    [JsonPropertyName("svg_templates")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public SvgTemplate[]? SvgTemplates
    {
        get; set;
    }
}

/// <summary>
/// Represents the simple rendering metadata for a credential type.
/// Provides basic display information such as logos and color schemes.
/// </summary>
public class SimpleRenderingMetadata
{
    /// <summary>
    /// Gets or sets the logo for the credential type.
    /// Optional. Contains URI and alt text for the issuer's logo.
    /// </summary>
    [JsonPropertyName("logo")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public LogoMetadata? Logo
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the background image for the credential type.
    /// Optional. Contains URI for the credential's background image.
    /// </summary>
    [JsonPropertyName("background_image")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public LogoMetadata? BackgroundImage
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the background color for the credential card.
    /// Optional. Should be a CSS color value (e.g., "#12107c").
    /// </summary>
    [JsonPropertyName("background_color")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? BackgroundColor
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the text color for the credential card.
    /// Optional. Should be a CSS color value (e.g., "#FFFFFF").
    /// </summary>
    [JsonPropertyName("text_color")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? TextColor
    {
        get; set;
    }
}

/// <summary>
/// Represents a logo or image reference with optional integrity hash.
/// Used for both logos and background images in simple rendering metadata.
/// </summary>
public class LogoMetadata
{
    /// <summary>
    /// Gets or sets the URI of the image.
    /// Required. Must be an HTTPS URI or a data URI per draft-ietf-oauth-sd-jwt-vc-14.
    /// </summary>
    [JsonPropertyName("uri")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Uri
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the Subresource Integrity hash of the image at the URI.
    /// Optional. Used for integrity protection of the referenced image resource.
    /// </summary>
    [JsonPropertyName("uri#integrity")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? UriIntegrity
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the alternative text for the image.
    /// Optional. Used for accessibility purposes.
    /// </summary>
    [JsonPropertyName("alt_text")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? AltText
    {
        get; set;
    }
}

/// <summary>
/// Represents an SVG template reference for rendering a credential.
/// Each SVG template targets a specific rendering context (e.g., orientation, color scheme).
/// </summary>
public class SvgTemplate
{
    /// <summary>
    /// Gets or sets the URI of the SVG template.
    /// Required. Must be an HTTPS URI or a data URI.
    /// </summary>
    [JsonPropertyName("uri")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Uri
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the Subresource Integrity hash of the SVG template at the URI.
    /// Optional. Used for integrity protection of the referenced SVG resource.
    /// </summary>
    [JsonPropertyName("uri#integrity")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? UriIntegrity
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the display properties for this SVG template.
    /// Optional. Describes the rendering context in which this template should be used.
    /// </summary>
    [JsonPropertyName("properties")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public SvgTemplateProperties? Properties
    {
        get; set;
    }
}

/// <summary>
/// Represents the display properties of an SVG template.
/// These properties help select the correct template for a given rendering context.
/// </summary>
public class SvgTemplateProperties
{
    /// <summary>
    /// Gets or sets the orientation for which this template is designed.
    /// Optional. Typical values are "landscape" or "portrait".
    /// </summary>
    [JsonPropertyName("orientation")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Orientation
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the color scheme for which this template is designed.
    /// Optional. Typical values are "light" or "dark".
    /// </summary>
    [JsonPropertyName("color_scheme")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ColorScheme
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the contrast setting for which this template is designed.
    /// Optional. Typical values are "normal" or "high".
    /// </summary>
    [JsonPropertyName("contrast")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Contrast
    {
        get; set;
    }
}
