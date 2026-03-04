namespace SdJwt.Net.Wallet.Core;

/// <summary>
/// Parsed credential with extracted claims.
/// </summary>
public class ParsedCredential
{
    /// <summary>
    /// The credential format.
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Alias for Format property.
    /// </summary>
    public string FormatId
    {
        get => Format;
        set => Format = value;
    }

    /// <summary>
    /// The raw credential string.
    /// </summary>
    public string RawCredential { get; set; } = string.Empty;

    /// <summary>
    /// Issuer identifier.
    /// </summary>
    public string? Issuer
    {
        get; set;
    }

    /// <summary>
    /// Subject identifier.
    /// </summary>
    public string? Subject
    {
        get; set;
    }

    /// <summary>
    /// Credential type.
    /// </summary>
    public string? CredentialType
    {
        get; set;
    }

    /// <summary>
    /// Alias for CredentialType property.
    /// </summary>
    public string? Type
    {
        get => CredentialType;
        set => CredentialType = value;
    }

    /// <summary>
    /// When issued.
    /// </summary>
    public DateTimeOffset? IssuedAt
    {
        get; set;
    }

    /// <summary>
    /// When expires.
    /// </summary>
    public DateTimeOffset? ExpiresAt
    {
        get; set;
    }

    /// <summary>
    /// Key binding information (cnf claim).
    /// </summary>
    public string? KeyBinding
    {
        get; set;
    }

    /// <summary>
    /// Available disclosures for SD-JWT credentials.
    /// </summary>
    public IList<DisclosureInfo>? Disclosures
    {
        get; set;
    }

    /// <summary>
    /// All claims extracted from the credential.
    /// </summary>
    public IDictionary<string, object>? Claims
    {
        get; set;
    }

    /// <summary>
    /// Additional format-specific metadata.
    /// </summary>
    public IDictionary<string, object?>? Metadata
    {
        get; set;
    }
}
