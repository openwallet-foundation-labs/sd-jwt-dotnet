namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Check input for the revocation store.
/// Supports 10 revocation targets: jti, issuer, key id, agent id, tool id,
/// policy id/version, tenant id, workflow id, approval id, and manifest hash.
/// </summary>
public sealed record RevocationCheck
{
    /// <summary>
    /// Token identifier (jti) to check.
    /// </summary>
    public string? TokenId
    {
        get; init;
    }

    /// <summary>
    /// Issuer to check.
    /// </summary>
    public string? Issuer
    {
        get; init;
    }

    /// <summary>
    /// Issuer key identifier to check.
    /// </summary>
    public string? KeyId
    {
        get; init;
    }

    /// <summary>
    /// Agent identifier to check.
    /// </summary>
    public string? AgentId
    {
        get; init;
    }

    /// <summary>
    /// Tool identifier to check.
    /// </summary>
    public string? ToolId
    {
        get; init;
    }

    /// <summary>
    /// Policy identifier to check.
    /// </summary>
    public string? PolicyId
    {
        get; init;
    }

    /// <summary>
    /// Policy version to check.
    /// </summary>
    public string? PolicyVersion
    {
        get; init;
    }

    /// <summary>
    /// Tenant identifier to check.
    /// </summary>
    public string? TenantId
    {
        get; init;
    }

    /// <summary>
    /// Workflow identifier to check.
    /// </summary>
    public string? WorkflowId
    {
        get; init;
    }

    /// <summary>
    /// Approval identifier to check.
    /// </summary>
    public string? ApprovalId
    {
        get; init;
    }

    /// <summary>
    /// Manifest hash to check.
    /// </summary>
    public string? ManifestHash
    {
        get; init;
    }
}
