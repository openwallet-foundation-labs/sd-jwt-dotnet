namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Describes a registered tool's identity, schema, and trust metadata.
/// Used by the <see cref="IToolRegistry"/> for tool attestation and tamper detection.
/// </summary>
public record ToolRegistration
{
    /// <summary>
    /// Canonical tool identifier (e.g., "crm.contacts").
    /// </summary>
    public required string ToolId
    {
        get; init;
    }

    /// <summary>
    /// Human-readable display name.
    /// </summary>
    public string? DisplayName
    {
        get; init;
    }

    /// <summary>
    /// Semantic version of the tool.
    /// </summary>
    public required string Version
    {
        get; init;
    }

    /// <summary>
    /// SHA-256 hash of the tool's JSON schema for tamper detection.
    /// </summary>
    public required string SchemaHash
    {
        get; init;
    }

    /// <summary>
    /// SHA-256 hash of the tool's manifest.
    /// </summary>
    public required string ManifestHash
    {
        get; init;
    }

    /// <summary>
    /// Canonical URI of the MCP server hosting this tool.
    /// </summary>
    public string? McpServerUri
    {
        get; init;
    }

    /// <summary>
    /// Audience URI for capability token binding.
    /// </summary>
    public required string Audience
    {
        get; init;
    }

    /// <summary>
    /// Permitted actions for this tool.
    /// </summary>
    public IReadOnlyList<string> AllowedActions { get; init; } = [];

    /// <summary>
    /// Maximum token lifetime accepted by this tool.
    /// </summary>
    public TimeSpan? MaxTokenLifetime
    {
        get; init;
    }

    /// <summary>
    /// Whether this tool requires proof-of-possession.
    /// </summary>
    public bool RequiresProofOfPossession
    {
        get; init;
    }

    /// <summary>
    /// Whether this tool requires request binding (body hash, method, URI).
    /// </summary>
    public bool RequiresRequestBinding
    {
        get; init;
    }

    /// <summary>
    /// Data classification level for governance.
    /// </summary>
    public string? DataClassification
    {
        get; init;
    }

    /// <summary>
    /// Lifecycle status of the registration.
    /// </summary>
    public RegistrationStatus Status { get; init; } = RegistrationStatus.Proposed;

    /// <summary>
    /// Risk classification tier.
    /// </summary>
    public RiskTier RiskTier { get; init; } = RiskTier.Medium;

    /// <summary>
    /// Identity of the tool owner/maintainer.
    /// </summary>
    public string? ToolOwner
    {
        get; init;
    }

    /// <summary>
    /// Publisher identity for cross-org trust.
    /// </summary>
    public string? PublisherIdentity
    {
        get; init;
    }

    /// <summary>
    /// URI of the signing key for verifying tool attestation.
    /// </summary>
    public string? SigningKeyUri
    {
        get; init;
    }

    /// <summary>
    /// Tenants allowed to invoke this tool.
    /// </summary>
    public IReadOnlyList<string>? AllowedTenants
    {
        get; init;
    }

    /// <summary>
    /// Environments allowed for this tool (e.g., "production", "staging").
    /// </summary>
    public IReadOnlyList<string>? AllowedEnvironments
    {
        get; init;
    }

    /// <summary>
    /// Egress domains allowed for this tool's outbound calls.
    /// </summary>
    public IReadOnlyList<string>? AllowedEgressDomains
    {
        get; init;
    }

    /// <summary>
    /// Approval policy required for this tool.
    /// </summary>
    public string? ApprovalPolicy
    {
        get; init;
    }

    /// <summary>
    /// Previous manifest hashes for schema migration tracking.
    /// </summary>
    public IReadOnlyList<string>? PreviousManifestHashes
    {
        get; init;
    }

    /// <summary>
    /// Timestamp of the last security review.
    /// </summary>
    public DateTimeOffset? LastReviewedAt
    {
        get; init;
    }
}
