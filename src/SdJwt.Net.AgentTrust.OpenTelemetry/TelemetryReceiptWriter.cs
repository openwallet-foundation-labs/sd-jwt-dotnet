using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.OpenTelemetry;

/// <summary>
/// Receipt writer that emits audit receipts as OpenTelemetry metrics
/// in addition to delegating to an inner writer.
/// </summary>
public class TelemetryReceiptWriter : IReceiptWriter
{
    private readonly IReceiptWriter? _inner;

    /// <summary>
    /// Initializes a new telemetry receipt writer.
    /// </summary>
    /// <param name="inner">Optional inner writer to delegate to.</param>
    public TelemetryReceiptWriter(IReceiptWriter? inner = null)
    {
        _inner = inner;
    }

    /// <inheritdoc/>
    public async Task WriteAsync(AuditReceipt receipt, CancellationToken cancellationToken = default)
    {
        AgentTrustMetrics.RecordPolicyEvaluation(
            receipt.Decision == ReceiptDecision.Allow,
            receipt.Tool,
            receipt.Action);

        if (_inner != null)
        {
            await _inner.WriteAsync(receipt, cancellationToken);
        }
    }
}
