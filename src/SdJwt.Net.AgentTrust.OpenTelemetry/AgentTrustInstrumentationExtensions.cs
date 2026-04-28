using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.OpenTelemetry;

/// <summary>
/// Extension methods for registering Agent Trust Kit instrumentation with OpenTelemetry.
/// </summary>
public static class AgentTrustInstrumentationExtensions
{
    /// <summary>
    /// Adds Agent Trust Kit tracing instrumentation.
    /// Subscribes to the <c>SdJwt.Net.AgentTrust</c> <see cref="System.Diagnostics.ActivitySource"/>.
    /// </summary>
    /// <param name="builder">The tracer provider builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static TracerProviderBuilder AddAgentTrustInstrumentation(this TracerProviderBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.AddSource(AgentTrustActivitySource.SourceName);
    }

    /// <summary>
    /// Adds Agent Trust Kit metrics instrumentation.
    /// Subscribes to the <c>SdJwt.Net.AgentTrust</c> <see cref="System.Diagnostics.Metrics.Meter"/>.
    /// </summary>
    /// <param name="builder">The meter provider builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static MeterProviderBuilder AddAgentTrustInstrumentation(this MeterProviderBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder.AddMeter(AgentTrustMetrics.MeterName);
    }
}
