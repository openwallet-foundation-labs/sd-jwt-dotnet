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
