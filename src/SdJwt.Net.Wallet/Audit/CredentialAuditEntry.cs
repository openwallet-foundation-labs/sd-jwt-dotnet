namespace SdJwt.Net.Wallet.Audit;

/// <summary>
/// Extended transaction log entry that records credential-level audit information
/// such as which credential was involved and what claims were disclosed.
/// </summary>
public class CredentialAuditEntry : TransactionLog
{
    /// <summary>
    /// The wallet-internal credential ID involved in the transaction.
    /// </summary>
    public string? CredentialId
    {
        get; set;
    }

    /// <summary>
    /// Credential operation performed.
    /// </summary>
    public CredentialOperation CredentialOperation
    {
        get; set;
    }

    /// <summary>
    /// Identifier of the counterparty (issuer URI for issuance, verifier URI for presentation).
    /// </summary>
    public string? CounterpartyId
    {
        get; set;
    }

    /// <summary>
    /// Claim names that were selectively disclosed during presentation.
    /// Null for issuance transactions.
    /// </summary>
    public IReadOnlyList<string>? DisclosedClaims
    {
        get; set;
    }
}
