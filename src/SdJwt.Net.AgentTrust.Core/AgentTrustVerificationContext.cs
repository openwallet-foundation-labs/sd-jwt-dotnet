using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Comprehensive verification context for strict Agent Trust token validation.
/// Replaces the minimal <see cref="CapabilityVerificationOptions"/> for production use.
/// </summary>
public sealed record AgentTrustVerificationContext
{
    /// <summary>
    /// Expected audience or resource identifier.
    /// </summary>
    public required string ExpectedAudience
    {
        get; init;
    }

    /// <summary>
    /// Map of trusted issuers to their signing keys.
    /// </summary>
    public required IReadOnlyDictionary<string, SecurityKey> TrustedIssuers
    {
        get; init;
    }

    /// <summary>
    /// Security mode controlling validation strictness.
    /// </summary>
    public AgentTrustSecurityMode SecurityMode { get; init; } = AgentTrustSecurityMode.Pilot;

    /// <summary>
    /// Maximum allowed token lifetime. Tokens with exp - iat exceeding this are rejected.
    /// </summary>
    public TimeSpan MaxTokenLifetime { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Maximum age of the iat claim. Tokens issued too far in the past are rejected.
    /// </summary>
    public TimeSpan MaxIssuedAtAge { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Clock skew tolerance for time-based validation.
    /// </summary>
    public TimeSpan ClockSkewTolerance { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Whether replay prevention is enforced.
    /// </summary>
    public bool EnforceReplayPrevention { get; init; } = true;

    /// <summary>
    /// Allowed signing algorithms. Tokens signed with algorithms outside this set are rejected.
    /// Defaults to ES256/384/512 and PS256/384/512 per HAIP. HmacSha256 must NOT be in default set.
    /// </summary>
    public IReadOnlyCollection<string> AllowedAlgorithms
    {
        get; init;
    } = new HashSet<string>(StringComparer.Ordinal)
    {
        SecurityAlgorithms.EcdsaSha256,
        SecurityAlgorithms.EcdsaSha384,
        SecurityAlgorithms.EcdsaSha512,
        SecurityAlgorithms.RsaSsaPssSha256,
        SecurityAlgorithms.RsaSsaPssSha384,
        SecurityAlgorithms.RsaSsaPssSha512
    };

    /// <summary>
    /// Whether proof-of-possession (DPoP, mTLS, or SD-JWT+KB) is required.
    /// Defaults to true in Enterprise and Regulated modes.
    /// </summary>
    public bool RequireProofOfPossession
    {
        get; init;
    }

    /// <summary>
    /// Whether request binding (method, URI, body hash) must be validated.
    /// </summary>
    public bool RequireRequestBinding
    {
        get; init;
    }

    /// <summary>
    /// Whether policy binding validation is required.
    /// When true, the token's policy claims are validated against the authority.
    /// </summary>
    public bool RequirePolicyBinding
    {
        get; init;
    }

    /// <summary>
    /// Proof-of-possession material (DPoP proof, mTLS cert, SD-JWT key binding)
    /// for verifying sender-constrained tokens.
    /// </summary>
    public ProofMaterial? ProofMaterial
    {
        get; init;
    }

    /// <summary>
    /// Expected tool identifier. When set, the cap.tool claim must match.
    /// </summary>
    public string? ExpectedToolId
    {
        get; init;
    }

    /// <summary>
    /// Expected action. When set, the cap.action claim must match.
    /// </summary>
    public string? ExpectedAction
    {
        get; init;
    }

    /// <summary>
    /// The actual HTTP request binding for validation against the token's request claims.
    /// </summary>
    public HttpRequestBinding? ActualRequest
    {
        get; init;
    }

    /// <summary>
    /// Expected tenant identifier. When set, the ctx.tenant_id must match.
    /// </summary>
    public string? ExpectedTenantId
    {
        get; init;
    }

    /// <summary>
    /// Whether tool manifest/schema binding should be validated.
    /// </summary>
    public bool RequireToolManifestBinding
    {
        get; init;
    }

    /// <summary>
    /// Expected tool schema hash for tamper detection.
    /// </summary>
    public string? ExpectedToolSchemaHash
    {
        get; init;
    }

    /// <summary>
    /// Accepted token types. Defaults to the Agent Trust capability type.
    /// Includes legacy type for backward compatibility.
    /// </summary>
    public IReadOnlyCollection<string> AcceptedTokenTypes
    {
        get; init;
    } = new HashSet<string>(StringComparer.Ordinal)
    {
        AgentTrustTokenTypes.CapabilitySdJwt,
        AgentTrustTokenTypes.LegacySdJwtVc
    };
}
