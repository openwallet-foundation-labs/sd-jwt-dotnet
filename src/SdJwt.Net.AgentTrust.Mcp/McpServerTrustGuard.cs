using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace SdJwt.Net.AgentTrust.Mcp;

/// <summary>
/// Server-side guard that verifies capability tokens on inbound MCP tool calls.
/// </summary>
public class McpServerTrustGuard
{
    private readonly CapabilityTokenVerifier _verifier;
    private readonly IPolicyEngine _policyEngine;
    private readonly McpServerTrustOptions _options;
    private readonly IReceiptWriter _receiptWriter;
    private readonly ILogger<McpServerTrustGuard> _logger;

    /// <summary>
    /// Initializes a new MCP server trust guard.
    /// </summary>
    /// <param name="verifier">Capability token verifier.</param>
    /// <param name="policyEngine">Policy engine for post-verification evaluation.</param>
    /// <param name="options">Server trust options.</param>
    /// <param name="receiptWriter">Audit receipt writer.</param>
    /// <param name="logger">Optional logger.</param>
    public McpServerTrustGuard(
        CapabilityTokenVerifier verifier,
        IPolicyEngine policyEngine,
        McpServerTrustOptions options,
        IReceiptWriter? receiptWriter = null,
        ILogger<McpServerTrustGuard>? logger = null)
    {
        _verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
        _policyEngine = policyEngine ?? throw new ArgumentNullException(nameof(policyEngine));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _receiptWriter = receiptWriter ?? new LoggingReceiptWriter();
        _logger = logger ?? NullLogger<McpServerTrustGuard>.Instance;
    }

    /// <summary>
    /// Verifies a capability token for an inbound MCP tool call.
    /// </summary>
    /// <param name="toolName">The tool being called.</param>
    /// <param name="token">The capability token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The verification result.</returns>
    public async Task<CapabilityVerificationResult> VerifyToolCallAsync(
        string toolName,
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return CapabilityVerificationResult.Failure("Tool name is required.", "invalid_tool");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return CapabilityVerificationResult.Failure("Token is required.", "missing_token");
        }

        var verificationResult = await _verifier.VerifyAsync(token, new CapabilityVerificationOptions
        {
            ExpectedAudience = _options.Audience,
            TrustedIssuers = _options.TrustedIssuers
        }, cancellationToken);

        if (!verificationResult.IsValid)
        {
            await WriteReceiptAsync(toolName, verificationResult, ReceiptDecision.Deny, verificationResult.Error);
            return verificationResult;
        }

        if (verificationResult.Capability != null &&
            !string.Equals(verificationResult.Capability.Tool, toolName, StringComparison.OrdinalIgnoreCase))
        {
            var mismatchResult = CapabilityVerificationResult.Failure(
                $"Token tool '{verificationResult.Capability.Tool}' does not match requested tool '{toolName}'.",
                "tool_mismatch");
            await WriteReceiptAsync(toolName, verificationResult, ReceiptDecision.Deny, mismatchResult.Error);
            return mismatchResult;
        }

        var policyResult = await _policyEngine.EvaluateAsync(new PolicyRequest
        {
            AgentId = verificationResult.Issuer ?? string.Empty,
            Tool = toolName,
            Action = verificationResult.Capability?.Action ?? string.Empty,
            Context = verificationResult.Context
        }, cancellationToken);

        if (!policyResult.IsPermitted)
        {
            await WriteReceiptAsync(toolName, verificationResult, ReceiptDecision.Deny, policyResult.DenialReason);
            return CapabilityVerificationResult.Failure(
                policyResult.DenialReason ?? "Policy denied.",
                policyResult.DenialCode ?? "policy_denied");
        }

        await WriteReceiptAsync(toolName, verificationResult, ReceiptDecision.Allow, null);
        return verificationResult;
    }

    private Task WriteReceiptAsync(
        string toolName,
        CapabilityVerificationResult result,
        ReceiptDecision decision,
        string? denyReason)
    {
        return _receiptWriter.WriteAsync(new AuditReceipt
        {
            TokenId = result.TokenId ?? string.Empty,
            Timestamp = DateTimeOffset.UtcNow,
            Decision = decision,
            Tool = toolName,
            Action = result.Capability?.Action ?? string.Empty,
            CorrelationId = result.Context?.CorrelationId ?? string.Empty,
            AgentId = result.Issuer,
            DenyReason = denyReason
        });
    }
}
