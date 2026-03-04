namespace SdJwt.Net.Wallet.Core;

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
