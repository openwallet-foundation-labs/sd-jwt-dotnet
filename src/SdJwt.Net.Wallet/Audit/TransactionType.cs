namespace SdJwt.Net.Wallet.Audit;

/// <summary>
/// Supported wallet transaction categories.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Credential issuance transaction.
    /// </summary>
    Issuance,

    /// <summary>
    /// Credential presentation transaction.
    /// </summary>
    Presentation,

    /// <summary>
    /// Wallet attestation transaction.
    /// </summary>
    Attestation
}
