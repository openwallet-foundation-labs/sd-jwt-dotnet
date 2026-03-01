namespace SdJwt.Net.Wallet.Core;

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
