using SdJwt.Net.Wallet.Attestation;
using SdJwt.Net.Wallet.Audit;
using SdJwt.Net.Wallet.Core;
using SdJwt.Net.Wallet.Protocols;
using SdJwt.Net.Wallet.Status;

namespace SdJwt.Net.Wallet;

/// <summary>
/// Configuration options for the generic wallet.
/// </summary>
public class WalletOptions
{
    /// <summary>
    /// Wallet instance identifier.
    /// </summary>
    public string? WalletId
    {
        get; set;
    }

    /// <summary>
    /// Display name for the wallet.
    /// </summary>
    public string? DisplayName
    {
        get; set;
    }

    /// <summary>
    /// Whether to automatically validate credentials on add.
    /// </summary>
    public bool ValidateOnAdd { get; set; } = true;

    /// <summary>
    /// Whether to automatically check credential status.
    /// </summary>
    public bool AutoCheckStatus
    {
        get; set;
    }

    /// <summary>
    /// Default key generation options.
    /// </summary>
    public KeyGenerationOptions? DefaultKeyOptions
    {
        get; set;
    }

    /// <summary>
    /// Optional OpenID4VCI adapter for issuance flows.
    /// </summary>
    public IOid4VciAdapter? Oid4VciAdapter
    {
        get; set;
    }

    /// <summary>
    /// Optional OpenID4VP adapter for presentation flows.
    /// </summary>
    public IOid4VpAdapter? Oid4VpAdapter
    {
        get; set;
    }

    /// <summary>
    /// Optional wallet attestations provider (WIA/WUA).
    /// </summary>
    public IWalletAttestationsProvider? WalletAttestationsProvider
    {
        get; set;
    }

    /// <summary>
    /// Optional transaction logger for wallet operations.
    /// </summary>
    public ITransactionLogger? TransactionLogger
    {
        get; set;
    }

    /// <summary>
    /// Optional DPoP proof provider for token and credential endpoint requests.
    /// </summary>
    public IDPoPProofProvider? DPoPProofProvider
    {
        get; set;
    }

    /// <summary>
    /// Optional document status resolver for live status checks.
    /// </summary>
    public IDocumentStatusResolver? DocumentStatusResolver
    {
        get; set;
    }
}
