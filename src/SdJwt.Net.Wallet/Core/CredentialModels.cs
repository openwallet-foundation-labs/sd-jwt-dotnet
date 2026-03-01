namespace SdJwt.Net.Wallet.Core;

/// <summary>
/// Simple credential status enum for tracking.
/// </summary>
public enum CredentialStatusType
{
    /// <summary>
    /// Credential is valid and active.
    /// </summary>
    Valid,

    /// <summary>
    /// Credential has expired.
    /// </summary>
    Expired,

    /// <summary>
    /// Credential has been revoked.
    /// </summary>
    Revoked,

    /// <summary>
    /// Credential has been suspended.
    /// </summary>
    Suspended,

    /// <summary>
    /// Status is unknown.
    /// </summary>
    Unknown
}

/// <summary>
/// Represents a stored credential in the wallet.
/// </summary>
public record StoredCredential
{
    /// <summary>
    /// Unique identifier for this credential in the wallet.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The credential format (e.g., "vc+sd-jwt", "mso_mdoc").
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// The raw credential data.
    /// </summary>
    public string RawCredential { get; set; } = string.Empty;

    /// <summary>
    /// Credential type or vct (verifiable credential type).
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
    /// When the credential was issued.
    /// </summary>
    public DateTimeOffset? IssuedAt
    {
        get; set;
    }

    /// <summary>
    /// When the credential expires.
    /// </summary>
    public DateTimeOffset? ExpiresAt
    {
        get; set;
    }

    /// <summary>
    /// When the credential was stored in the wallet.
    /// </summary>
    public DateTimeOffset StoredAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Key ID for holder binding (cnf claim).
    /// </summary>
    public string? HolderKeyId
    {
        get; set;
    }

    /// <summary>
    /// Alias for HolderKeyId property.
    /// </summary>
    public string? BoundKeyId
    {
        get => HolderKeyId;
        set => HolderKeyId = value;
    }

    /// <summary>
    /// Current credential status.
    /// </summary>
    public CredentialStatusType Status { get; set; } = CredentialStatusType.Valid;

    /// <summary>
    /// Credential usage policy.
    /// </summary>
    public CredentialPolicy Policy { get; set; } = CredentialPolicy.Default;

    /// <summary>
    /// Usage count for tracking (RotateUse policy).
    /// </summary>
    public int UsageCount
    {
        get; set;
    }

    /// <summary>
    /// Document ID for batch credentials (same docId = same logical credential).
    /// </summary>
    public string? DocumentId
    {
        get; set;
    }

    /// <summary>
    /// Additional metadata about the credential.
    /// </summary>
    public IDictionary<string, object>? Metadata
    {
        get; set;
    }
}

/// <summary>
/// Filter criteria for finding credentials.
/// </summary>
public class CredentialFilter
{
    /// <summary>
    /// Filter by credential type.
    /// </summary>
    public string? CredentialType
    {
        get; set;
    }

    /// <summary>
    /// Filter by issuer.
    /// </summary>
    public string? Issuer
    {
        get; set;
    }

    /// <summary>
    /// Filter by format.
    /// </summary>
    public string? Format
    {
        get; set;
    }

    /// <summary>
    /// Filter by document ID.
    /// </summary>
    public string? DocumentId
    {
        get; set;
    }

    /// <summary>
    /// Whether to include expired credentials.
    /// </summary>
    public bool IncludeExpired { get; set; } = false;
}

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

/// <summary>
/// Information about a single disclosure.
/// </summary>
public class DisclosureInfo
{
    /// <summary>
    /// JSON path or claim name.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The disclosed value.
    /// </summary>
    public object? Value
    {
        get; set;
    }

    /// <summary>
    /// The disclosure digest.
    /// </summary>
    public string? Digest
    {
        get; set;
    }

    /// <summary>
    /// The salt used in the disclosure.
    /// </summary>
    public string? Salt
    {
        get; set;
    }

    /// <summary>
    /// Whether this disclosure is selected for presentation.
    /// </summary>
    public bool IsSelected
    {
        get; set;
    }
}

/// <summary>
/// Result of credential validation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Whether the credential is valid.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Validation errors if not valid.
    /// </summary>
    public IList<string>? Errors
    {
        get; set;
    }

    /// <summary>
    /// Validation warnings (non-fatal).
    /// </summary>
    public IList<string>? Warnings
    {
        get; set;
    }

    /// <summary>
    /// Status check result if performed.
    /// </summary>
    public CredentialStatus? Status
    {
        get; set;
    }
}

/// <summary>
/// Credential status from revocation check.
/// </summary>
public class CredentialStatus
{
    /// <summary>
    /// Whether the credential is active (not revoked/suspended).
    /// </summary>
    public bool IsActive
    {
        get; set;
    }

    /// <summary>
    /// Status type (Valid, Revoked, Suspended).
    /// </summary>
    public string? StatusType
    {
        get; set;
    }

    /// <summary>
    /// When the status was checked.
    /// </summary>
    public DateTimeOffset? CheckedAt
    {
        get; set;
    }
}

/// <summary>
/// Credential usage policy (from EUDI Android/iOS).
/// </summary>
public enum CredentialPolicy
{
    /// <summary>
    /// Default policy - same as RotateUse.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Credential is deleted after single use. Batch issuance provides multiple copies.
    /// </summary>
    OneTimeUse = 1,

    /// <summary>
    /// Credential usage is tracked; selects least-used credential first.
    /// </summary>
    RotateUse = 2
}
