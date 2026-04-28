using System.Diagnostics.Metrics;
using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.OpenTelemetry;

/// <summary>
/// OpenTelemetry metrics for Agent Trust Kit operations.
/// Subscribe via <c>MeterProviderBuilder.AddMeter("SdJwt.Net.AgentTrust")</c>.
/// </summary>
public static class AgentTrustMetrics
{
    /// <summary>
    /// Meter name for Agent Trust Kit metrics.
    /// </summary>
    public const string MeterName = "SdJwt.Net.AgentTrust";

    private static readonly Meter Meter = new(MeterName, "1.0.0");

    private static readonly Counter<long> TokensMinted = Meter.CreateCounter<long>(
        "agent_trust.tokens.minted",
        description: "Number of capability tokens minted.");

    private static readonly Counter<long> TokensVerified = Meter.CreateCounter<long>(
        "agent_trust.tokens.verified",
        description: "Number of capability tokens verified.");

    private static readonly Counter<long> TokensRejected = Meter.CreateCounter<long>(
        "agent_trust.tokens.rejected",
        description: "Number of capability tokens rejected.");

    private static readonly Counter<long> PolicyEvaluations = Meter.CreateCounter<long>(
        "agent_trust.policy.evaluations",
        description: "Number of policy evaluations.");

    private static readonly Counter<long> PolicyDenials = Meter.CreateCounter<long>(
        "agent_trust.policy.denials",
        description: "Number of policy denials.");

    private static readonly Histogram<double> MintDuration = Meter.CreateHistogram<double>(
        "agent_trust.mint.duration",
        unit: "ms",
        description: "Duration of capability token minting in milliseconds.");

    private static readonly Histogram<double> VerifyDuration = Meter.CreateHistogram<double>(
        "agent_trust.verify.duration",
        unit: "ms",
        description: "Duration of capability token verification in milliseconds.");

    /// <summary>
    /// Records a token mint event.
    /// </summary>
    /// <param name="tool">The tool name.</param>
    /// <param name="action">The action name.</param>
    /// <param name="durationMs">Mint duration in milliseconds.</param>
    public static void RecordMint(string tool, string action, double? durationMs = null)
    {
        TokensMinted.Add(1,
            new KeyValuePair<string, object?>("agent_trust.tool", tool),
            new KeyValuePair<string, object?>("agent_trust.action", action));

        if (durationMs.HasValue)
        {
            MintDuration.Record(durationMs.Value,
                new KeyValuePair<string, object?>("agent_trust.tool", tool),
                new KeyValuePair<string, object?>("agent_trust.action", action));
        }
    }

    /// <summary>
    /// Records a token verification event.
    /// </summary>
    /// <param name="tool">The tool name.</param>
    /// <param name="action">The action name.</param>
    /// <param name="valid">Whether verification succeeded.</param>
    /// <param name="durationMs">Verify duration in milliseconds.</param>
    public static void RecordVerify(string tool, string action, bool valid, double? durationMs = null)
    {
        if (valid)
        {
            TokensVerified.Add(1,
                new KeyValuePair<string, object?>("agent_trust.tool", tool),
                new KeyValuePair<string, object?>("agent_trust.action", action));
        }
        else
        {
            TokensRejected.Add(1,
                new KeyValuePair<string, object?>("agent_trust.tool", tool),
                new KeyValuePair<string, object?>("agent_trust.action", action));
        }

        if (durationMs.HasValue)
        {
            VerifyDuration.Record(durationMs.Value,
                new KeyValuePair<string, object?>("agent_trust.tool", tool),
                new KeyValuePair<string, object?>("agent_trust.action", action));
        }
    }

    /// <summary>
    /// Records a policy evaluation event.
    /// </summary>
    /// <param name="permitted">Whether the policy evaluation permitted the action.</param>
    /// <param name="tool">The tool name.</param>
    /// <param name="action">The action name.</param>
    public static void RecordPolicyEvaluation(bool permitted, string tool, string action)
    {
        PolicyEvaluations.Add(1,
            new KeyValuePair<string, object?>("agent_trust.tool", tool),
            new KeyValuePair<string, object?>("agent_trust.action", action));

        if (!permitted)
        {
            PolicyDenials.Add(1,
                new KeyValuePair<string, object?>("agent_trust.tool", tool),
                new KeyValuePair<string, object?>("agent_trust.action", action));
        }
    }
}
