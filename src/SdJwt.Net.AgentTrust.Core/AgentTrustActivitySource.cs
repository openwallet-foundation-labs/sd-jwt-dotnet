using System.Diagnostics;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Shared <see cref="ActivitySource"/> for Agent Trust Kit distributed tracing.
/// Consumers subscribe via OpenTelemetry by adding <c>"SdJwt.Net.AgentTrust"</c>
/// to their <c>TracerProviderBuilder</c>.
/// </summary>
public static class AgentTrustActivitySource
{
    /// <summary>
    /// Source name used by OpenTelemetry listeners.
    /// </summary>
    public const string SourceName = "SdJwt.Net.AgentTrust";

    /// <summary>
    /// The singleton <see cref="ActivitySource"/> instance.
    /// </summary>
    public static ActivitySource Source { get; } = new(SourceName, "1.0.0");
}
