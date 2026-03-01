namespace SdJwt.Net.StatusList.Introspection;

/// <summary>
/// Options for configuring the hybrid status checking strategy.
/// </summary>
public class HybridStatusOptions
{
    /// <summary>
    /// The hybrid status checking strategy to use.
    /// Default is PreferStatusList.
    /// </summary>
    public HybridStrategy Strategy { get; set; } = HybridStrategy.PreferStatusList;

    /// <summary>
    /// The introspection endpoint URI. Required when strategy uses introspection.
    /// </summary>
    public string? IntrospectionEndpoint
    {
        get; set;
    }

    /// <summary>
    /// Whether to fall back to the alternative method on error.
    /// Default is true.
    /// </summary>
    public bool FallbackOnError { get; set; } = true;

    /// <summary>
    /// Whether to enable caching of introspection results.
    /// Default is true.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache duration for introspection results.
    /// Default is 5 minutes.
    /// </summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Timeout for parallel operations when using Parallel strategy.
    /// Default is 10 seconds.
    /// </summary>
    public TimeSpan ParallelTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Token introspection options for client authentication.
    /// </summary>
    public TokenIntrospectionOptions? IntrospectionOptions
    {
        get; set;
    }

    /// <summary>
    /// Status list options for Status List checking.
    /// </summary>
    public Verifier.StatusListOptions? StatusListOptions
    {
        get; set;
    }
}

/// <summary>
/// Hybrid status checking strategy.
/// </summary>
public enum HybridStrategy
{
    /// <summary>
    /// Use Status List first, fall back to introspection on error.
    /// Best for: Batch validation with privacy (herd anonymity).
    /// </summary>
    PreferStatusList,

    /// <summary>
    /// Use introspection first, fall back to Status List on error.
    /// Best for: Real-time status with low latency requirements.
    /// </summary>
    PreferIntrospection,

    /// <summary>
    /// Use only Status List tokens.
    /// Best for: Offline or privacy-focused scenarios.
    /// </summary>
    StatusListOnly,

    /// <summary>
    /// Use only Token Introspection.
    /// Best for: High-frequency, low-latency verification.
    /// </summary>
    IntrospectionOnly,

    /// <summary>
    /// Run both methods in parallel, return on first success.
    /// Best for: High availability scenarios where speed matters.
    /// </summary>
    Parallel
}
