using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Evaluates whether an agent action is permitted.
/// </summary>
public interface IPolicyEngine
{
    /// <summary>
    /// Evaluates a request.
    /// </summary>
    Task<PolicyDecision> EvaluateAsync(PolicyRequest request, CancellationToken cancellationToken = default);
}

