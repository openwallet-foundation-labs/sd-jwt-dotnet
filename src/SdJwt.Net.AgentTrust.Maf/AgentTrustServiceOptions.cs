using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace SdJwt.Net.AgentTrust.Maf;

/// <summary>
/// Service registration options for MAF integration.
/// </summary>
public record AgentTrustServiceOptions
{
    /// <summary>
    /// Key custody provider type.
    /// </summary>
    public Type KeyCustodyProviderType { get; init; } = typeof(InMemoryKeyCustodyProvider);

    /// <summary>
    /// Nonce store type.
    /// </summary>
    public Type NonceStoreType { get; init; } = typeof(MemoryNonceStore);

    /// <summary>
    /// Receipt writer type.
    /// </summary>
    public Type ReceiptWriterType { get; init; } = typeof(LoggingReceiptWriter);

    /// <summary>
    /// Policy engine type.
    /// </summary>
    public Type PolicyEngineType { get; init; } = typeof(DefaultPolicyEngine);
}
