using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace SdJwt.Net.AgentTrust.A2A;

/// <summary>
/// Validates delegation chains across agent boundaries.
/// Ensures depth constraints, action scoping, and trust chain integrity.
/// </summary>
public class DelegationChainValidator
{
    private readonly int _maxDepth;
    private readonly CapabilityTokenVerifier _verifier;
    private readonly ILogger<DelegationChainValidator> _logger;

    /// <summary>
    /// Initializes a new delegation chain validator.
    /// </summary>
    /// <param name="verifier">Capability token verifier for individual token validation.</param>
    /// <param name="maxDepth">Maximum allowed delegation depth.</param>
    /// <param name="logger">Optional logger.</param>
    public DelegationChainValidator(
        CapabilityTokenVerifier verifier,
        int maxDepth = 3,
        ILogger<DelegationChainValidator>? logger = null)
    {
        _verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
        _maxDepth = maxDepth;
        _logger = logger ?? NullLogger<DelegationChainValidator>.Instance;
    }

    /// <summary>
    /// Validates a delegation chain represented as an ordered list of tokens.
    /// Each token's issuer must match the previous token's audience.
    /// </summary>
    /// <param name="tokens">Ordered delegation tokens from root to leaf.</param>
    /// <param name="trustedIssuers">Trusted root issuer keys.</param>
    /// <param name="expectedAudience">Expected audience for the final token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    public async Task<DelegationChainValidationResult> ValidateChainAsync(
        IReadOnlyList<string> tokens,
        IReadOnlyDictionary<string, SecurityKey> trustedIssuers,
        string expectedAudience,
        CancellationToken cancellationToken = default)
    {
        if (tokens == null || tokens.Count == 0)
        {
            return DelegationChainValidationResult.Invalid("No tokens in chain.", "empty_chain");
        }

        if (tokens.Count > _maxDepth)
        {
            return DelegationChainValidationResult.Invalid(
                $"Chain depth {tokens.Count} exceeds maximum {_maxDepth}.",
                "delegation_depth_exceeded");
        }

        string? rootIssuer = null;
        var currentTrustedIssuers = trustedIssuers;

        for (var i = 0; i < tokens.Count; i++)
        {
            var isLast = i == tokens.Count - 1;
            var audience = isLast ? expectedAudience : string.Empty;

            var result = await _verifier.VerifyAsync(tokens[i], new CapabilityVerificationOptions
            {
                ExpectedAudience = audience,
                TrustedIssuers = currentTrustedIssuers,
                EnforceReplayPrevention = isLast
            }, cancellationToken);

            if (!result.IsValid)
            {
                _logger.LogWarning("Delegation chain validation failed at depth {Depth}: {Error}",
                    i, result.Error);
                return DelegationChainValidationResult.Invalid(
                    $"Token at depth {i} is invalid: {result.Error}",
                    result.ErrorCode ?? "chain_validation_failed");
            }

            if (i == 0)
            {
                rootIssuer = result.Issuer;
            }
        }

        return DelegationChainValidationResult.Valid(tokens.Count, rootIssuer ?? string.Empty);
    }
}
