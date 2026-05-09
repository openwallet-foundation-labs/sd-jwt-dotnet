using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents an entry in the <c>credential_configurations_supported</c> object of issuer metadata
/// per OID4VCI 1.0 Section 11.2.3. Serves as the base class for format-specific configurations.
/// </summary>
public class CredentialConfiguration
{
    /// <summary>
    /// Gets or sets the credential format identifier.
    /// REQUIRED. One of <c>dc+sd-jwt</c>, <c>mso_mdoc</c>, <c>jwt_vc_json</c>, <c>ldp_vc</c>, etc.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth 2.0 scope for requesting this credential configuration.
    /// OPTIONAL.
    /// </summary>
    [JsonPropertyName("scope")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Scope
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the cryptographic binding methods supported by the issuer.
    /// OPTIONAL. E.g., <c>jwk</c>, <c>did:example</c>.
    /// </summary>
    [JsonPropertyName("cryptographic_binding_methods_supported")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? CryptographicBindingMethodsSupported
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential signing algorithms supported by the issuer.
    /// OPTIONAL.
    /// </summary>
    [JsonPropertyName("credential_signing_alg_values_supported")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? CredentialSigningAlgValuesSupported
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the proof types supported for this credential configuration.
    /// OPTIONAL. Object keyed by proof type name (e.g., <c>jwt</c>), value describes supported algorithms.
    /// </summary>
    [JsonPropertyName("proof_types_supported")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Dictionary<string, ProofTypeConfig>? ProofTypesSupported
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the display metadata for this credential configuration.
    /// OPTIONAL. Localized display properties for wallets to show the user.
    /// </summary>
    [JsonPropertyName("display")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public CredentialDisplayProperties[]? Display
    {
        get; set;
    }
}

/// <summary>
/// Describes the algorithms and key attestation requirements for a single proof type.
/// </summary>
public class ProofTypeConfig
{
    /// <summary>
    /// Gets or sets the proof signing algorithms supported by the issuer.
    /// OPTIONAL.
    /// </summary>
    [JsonPropertyName("proof_signing_alg_values_supported")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? ProofSigningAlgValuesSupported
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the required key attestation properties.
    /// OPTIONAL. Object keyed by attestation property (e.g., <c>key_storage</c>) with allowed value arrays.
    /// </summary>
    [JsonPropertyName("key_attestations_required")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Dictionary<string, string[]>? KeyAttestationsRequired
    {
        get; set;
    }
}

/// <summary>
/// Localized display properties for a credential or issuer, per OID4VCI 1.0 Section 11.2.
/// </summary>
public class CredentialDisplayProperties
{
    /// <summary>
    /// Gets or sets the display name.
    /// REQUIRED.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the locale (BCP47 language tag, e.g., <c>en-US</c>).
    /// OPTIONAL.
    /// </summary>
    [JsonPropertyName("locale")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Locale
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the background color in CSS hex format (e.g., <c>#12107c</c>).
    /// OPTIONAL.
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
    /// Gets or sets the text color in CSS hex format.
    /// OPTIONAL.
    /// </summary>
    [JsonPropertyName("text_color")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? TextColor
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential logo.
    /// OPTIONAL.
    /// </summary>
    [JsonPropertyName("logo")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public CredentialLogo? Logo
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the description of the credential.
    /// OPTIONAL.
    /// </summary>
    [JsonPropertyName("description")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Description
    {
        get; set;
    }
}

/// <summary>
/// Represents a credential logo in issuer metadata display properties.
/// </summary>
public class CredentialLogo
{
    /// <summary>
    /// Gets or sets the logo URI.
    /// REQUIRED.
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alt text for the logo image.
    /// OPTIONAL.
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
