namespace SdJwt.Net.Wallet.Core;

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
