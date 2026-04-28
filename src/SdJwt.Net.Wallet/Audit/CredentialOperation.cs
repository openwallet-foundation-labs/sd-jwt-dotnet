namespace SdJwt.Net.Wallet.Audit;

/// <summary>
/// Credential-level operations for audit logging.
/// </summary>
public enum CredentialOperation
{
    /// <summary>
    /// Credential was received and stored (issuance).
    /// </summary>
    Issuance,

    /// <summary>
    /// Credential was presented to a verifier.
    /// </summary>
    Presentation,

    /// <summary>
    /// Credential status was checked.
    /// </summary>
    StatusCheck,

    /// <summary>
    /// Credential was deleted from the wallet.
    /// </summary>
    Deletion,

    /// <summary>
    /// Credential key was rotated.
    /// </summary>
    KeyRotation
}
