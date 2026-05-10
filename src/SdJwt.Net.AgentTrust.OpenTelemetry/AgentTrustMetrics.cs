using System.Diagnostics;
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

    /// <summary>
    /// Activity source name for Agent Trust Kit traces.
    /// </summary>
    public const string ActivitySourceName = "SdJwt.Net.AgentTrust";

    private static readonly Meter Meter = new(MeterName, "1.0.0");

    private static readonly ActivitySource Source = new(ActivitySourceName, "1.0.0");

    private static readonly Counter<long> TokensMinted = Meter.CreateCounter<long>(
        "agent_trust.capability.minted",
        description: "Number of capability tokens minted.");

    private static readonly Counter<long> TokensVerified = Meter.CreateCounter<long>(
        "agent_trust.capability.verified",
        description: "Number of capability tokens verified.");

    private static readonly Counter<long> TokensRejected = Meter.CreateCounter<long>(
        "agent_trust.capability.rejected",
        description: "Number of capability tokens rejected.");

    private static readonly Counter<long> PolicyEvaluations = Meter.CreateCounter<long>(
        "agent_trust.policy.evaluated",
        description: "Number of policy evaluations.");

    private static readonly Counter<long> PolicyDenials = Meter.CreateCounter<long>(
        "agent_trust.policy.denials",
        description: "Number of policy denials.");

    private static readonly Counter<long> ReplayDetected = Meter.CreateCounter<long>(
        "agent_trust.replay.detected",
        description: "Number of replay attempts detected.");

    private static readonly Counter<long> PopFailed = Meter.CreateCounter<long>(
        "agent_trust.pop.failed",
        description: "Number of proof-of-possession validation failures.");

    private static readonly Counter<long> RequestBindingFailed = Meter.CreateCounter<long>(
        "agent_trust.request_binding.failed",
        description: "Number of request binding validation failures.");

    private static readonly Counter<long> ReceiptWritten = Meter.CreateCounter<long>(
        "agent_trust.receipt.written",
        description: "Number of audit receipts written.");

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
    /// <param name="profile">The security profile.</param>
    /// <param name="durationMs">Mint duration in milliseconds.</param>
    public static void RecordMint(string tool, string action, string? profile = null, double? durationMs = null)
    {
        TokensMinted.Add(1,
            new KeyValuePair<string, object?>("tool", tool),
            new KeyValuePair<string, object?>("action", action),
            new KeyValuePair<string, object?>("profile", profile ?? "unknown"));

        if (durationMs.HasValue)
        {
            MintDuration.Record(durationMs.Value,
                new KeyValuePair<string, object?>("profile", profile ?? "unknown"),
                new KeyValuePair<string, object?>("tool", tool));
        }
    }

    /// <summary>
    /// Records a token verification event.
    /// </summary>
    /// <param name="tool">The tool name.</param>
    /// <param name="action">The action name.</param>
    /// <param name="valid">Whether verification succeeded.</param>
    /// <param name="profile">The security profile.</param>
    /// <param name="durationMs">Verify duration in milliseconds.</param>
    public static void RecordVerify(string tool, string action, bool valid, string? profile = null, double? durationMs = null)
    {
        if (valid)
        {
            TokensVerified.Add(1,
                new KeyValuePair<string, object?>("tool", tool),
                new KeyValuePair<string, object?>("action", action),
                new KeyValuePair<string, object?>("profile", profile ?? "unknown"));
        }
        else
        {
            TokensRejected.Add(1,
                new KeyValuePair<string, object?>("tool", tool),
                new KeyValuePair<string, object?>("action", action),
                new KeyValuePair<string, object?>("profile", profile ?? "unknown"));
        }

        if (durationMs.HasValue)
        {
            VerifyDuration.Record(durationMs.Value,
                new KeyValuePair<string, object?>("profile", profile ?? "unknown"),
                new KeyValuePair<string, object?>("tool", tool));
        }
    }

    /// <summary>
    /// Records a policy evaluation event.
    /// </summary>
    /// <param name="permitted">Whether the policy evaluation permitted the action.</param>
    /// <param name="tool">The tool name.</param>
    /// <param name="action">The action name.</param>
    /// <param name="policyId">The policy identifier.</param>
    /// <param name="effect">The decision effect.</param>
    public static void RecordPolicyEvaluation(bool permitted, string tool, string action, string? policyId = null, string? effect = null)
    {
        PolicyEvaluations.Add(1,
            new KeyValuePair<string, object?>("tool", tool),
            new KeyValuePair<string, object?>("action", action),
            new KeyValuePair<string, object?>("policy_id", policyId ?? "unknown"),
            new KeyValuePair<string, object?>("effect", effect ?? (permitted ? "permit" : "deny")));

        if (!permitted)
        {
            PolicyDenials.Add(1,
                new KeyValuePair<string, object?>("tool", tool),
                new KeyValuePair<string, object?>("action", action));
        }
    }

    /// <summary>
    /// Records a replay attempt detection.
    /// </summary>
    /// <param name="audience">The audience.</param>
    /// <param name="tenant">The tenant identifier.</param>
    /// <param name="tool">The tool name.</param>
    public static void RecordReplayDetected(string audience, string? tenant = null, string? tool = null)
    {
        ReplayDetected.Add(1,
            new KeyValuePair<string, object?>("audience", audience),
            new KeyValuePair<string, object?>("tenant", tenant ?? "unknown"),
            new KeyValuePair<string, object?>("tool", tool ?? "unknown"));
    }

    /// <summary>
    /// Records a proof-of-possession validation failure.
    /// </summary>
    /// <param name="proofType">The proof type (dpop, mtls, sd-jwt+kb).</param>
    /// <param name="reason">The failure reason.</param>
    public static void RecordPopFailed(string proofType, string reason)
    {
        PopFailed.Add(1,
            new KeyValuePair<string, object?>("proof_type", proofType),
            new KeyValuePair<string, object?>("reason", reason));
    }

    /// <summary>
    /// Records a request binding validation failure.
    /// </summary>
    /// <param name="tool">The tool name.</param>
    /// <param name="action">The action name.</param>
    public static void RecordRequestBindingFailed(string tool, string? action = null)
    {
        RequestBindingFailed.Add(1,
            new KeyValuePair<string, object?>("tool", tool),
            new KeyValuePair<string, object?>("action", action ?? "unknown"));
    }

    /// <summary>
    /// Records an audit receipt write event.
    /// </summary>
    /// <param name="receiptType">The receipt type.</param>
    /// <param name="result">The result (success/failure).</param>
    public static void RecordReceiptWritten(string receiptType, string result)
    {
        ReceiptWritten.Add(1,
            new KeyValuePair<string, object?>("receipt_type", receiptType),
            new KeyValuePair<string, object?>("result", result));
    }

    /// <summary>
    /// Starts an activity for minting a capability token.
    /// </summary>
    public static Activity? StartMintActivity() => Source.StartActivity("AgentTrust.Mint");

    /// <summary>
    /// Starts an activity for verifying a capability token.
    /// </summary>
    public static Activity? StartVerifyActivity() => Source.StartActivity("AgentTrust.Verify");

    /// <summary>
    /// Starts an activity for policy evaluation.
    /// </summary>
    public static Activity? StartPolicyEvaluateActivity() => Source.StartActivity("AgentTrust.PolicyEvaluate");

    /// <summary>
    /// Starts an activity for tool registry resolution.
    /// </summary>
    public static Activity? StartRegistryResolveActivity() => Source.StartActivity("AgentTrust.RegistryResolve");

    /// <summary>
    /// Starts an activity for replay store consumption.
    /// </summary>
    public static Activity? StartReplayConsumeActivity() => Source.StartActivity("AgentTrust.ReplayConsume");

    /// <summary>
    /// Starts an activity for receipt writing.
    /// </summary>
    public static Activity? StartReceiptWriteActivity() => Source.StartActivity("AgentTrust.ReceiptWrite");

    /// <summary>
    /// Starts an activity for delegation chain validation.
    /// </summary>
    public static Activity? StartDelegationValidateActivity() => Source.StartActivity("AgentTrust.DelegationValidate");

    /// <summary>
    /// Starts an activity for proof-of-possession validation.
    /// </summary>
    public static Activity? StartPopValidateActivity() => Source.StartActivity("AgentTrust.PopValidate");

    /// <summary>
    /// Starts an activity for request binding validation.
    /// </summary>
    public static Activity? StartRequestBindingValidateActivity() => Source.StartActivity("AgentTrust.RequestBindingValidate");
}
