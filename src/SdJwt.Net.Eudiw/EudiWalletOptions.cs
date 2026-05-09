using SdJwt.Net.Wallet.Attestation;
using SdJwt.Net.Wallet.Audit;
using SdJwt.Net.Wallet.Protocols;
using SdJwt.Net.Wallet.Status;

namespace SdJwt.Net.Eudiw;

/// <summary>
/// Configuration options for EU Digital Identity Wallet.
/// </summary>
public class EudiWalletOptions
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
    public string? DisplayName { get; set; } = "EUDI Wallet";

    /// <summary>
    /// EU Trust List endpoints for trust anchor resolution.
    /// </summary>
    public IReadOnlyList<string> TrustListEndpoints
    {
        get; set;
    } = new[]
    {
        EudiwConstants.TrustList.LotlUrl,
        EudiwConstants.TrustList.LotlJsonUrl
    };

    /// <summary>
    /// Legacy minimum HAIP policy level retained for compatibility with existing EUDIW integrations.
    /// </summary>
    public int MinimumHaipLevel { get; set; } = 2;

    /// <summary>
    /// Supported credential types for EUDI ecosystem.
    /// </summary>
    public IReadOnlyList<string> SupportedCredentialTypes
    {
        get; set;
    } = new[]
    {
        EudiwConstants.Pid.DocType,           // Person Identification Data
        EudiwConstants.Mdl.DocType,           // Mobile Driving License
        "eu.europa.ec.eudi.loyalty.1",        // Loyalty credentials
        "eu.europa.ec.eudi.health.1"          // Health credentials
    };

    /// <summary>
    /// Whether to enforce ARF (Architecture Reference Framework) requirements.
    /// </summary>
    public bool EnforceArfCompliance { get; set; } = true;

    /// <summary>
    /// Whether to automatically validate credentials on add.
    /// </summary>
    public bool ValidateOnAdd { get; set; } = true;

    /// <summary>
    /// Whether to validate issuer against EU Trust List.
    /// </summary>
    public bool ValidateIssuerTrust { get; set; } = true;

    /// <summary>
    /// Preferred language for error messages and UI (ISO 639-1).
    /// </summary>
    public string PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// Whether to require hardware-backed key storage.
    /// </summary>
    public bool RequireHardwareKeys
    {
        get; set;
    }

    /// <summary>
    /// Trust list cache timeout in hours.
    /// </summary>
    public int TrustListCacheHours { get; set; } = 6;

    /// <summary>
    /// Supported algorithms per ARF.
    /// </summary>
    public IReadOnlyList<string> SupportedAlgorithms
    {
        get; set;
    } =
        EudiwConstants.Algorithms.SupportedAlgorithms.ToList();

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
    /// Optional wallet and key attestations provider.
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
    /// Optional DPoP proof provider for issuance requests.
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
