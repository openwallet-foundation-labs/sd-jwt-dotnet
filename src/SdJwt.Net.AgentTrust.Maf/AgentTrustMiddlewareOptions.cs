namespace SdJwt.Net.AgentTrust.Maf;

/// <summary>
/// Middleware configuration options.
/// </summary>
public record AgentTrustMiddlewareOptions
{
    /// <summary>
    /// Issuer agent identifier.
    /// </summary>
    public required string AgentId
    {
        get; init;
    }

    /// <summary>
    /// Default token lifetime.
    /// </summary>
    public TimeSpan DefaultTokenLifetime { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Tool to audience mapping.
    /// </summary>
    public IReadOnlyDictionary<string, string> ToolAudienceMapping
    {
        get; init;
    } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Header name used for token propagation.
    /// </summary>
    public string TokenHeaderName { get; init; } = "Authorization";

    /// <summary>
    /// Header prefix.
    /// </summary>
    public string TokenHeaderPrefix { get; init; } = "SdJwt";

    /// <summary>
    /// Whether to emit receipts.
    /// </summary>
    public bool EmitReceipts { get; init; } = true;

    /// <summary>
    /// Whether middleware should fail closed on token mint errors.
    /// </summary>
    public bool FailOnMintError { get; init; } = true;
}
