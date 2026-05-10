namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Registry for tool attestation, schema binding, and trust metadata.
/// Prevents tool poisoning and rug-pull attacks by validating tool identity.
/// </summary>
public interface IToolRegistry
{
    /// <summary>
    /// Resolves tool metadata by tool identifier.
    /// </summary>
    /// <param name="toolId">Canonical tool identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tool registration, or null if not found.</returns>
    Task<ToolRegistration?> ResolveAsync(string toolId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the given schema hash matches the registered tool's expected hash.
    /// </summary>
    /// <param name="toolId">Canonical tool identifier.</param>
    /// <param name="schemaHash">SHA-256 hash of the actual tool schema.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the hash matches or no hash is registered; false if mismatch.</returns>
    Task<bool> ValidateSchemaAsync(string toolId, string schemaHash, CancellationToken cancellationToken = default);
}
