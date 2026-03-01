using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.Wallet.Core;

/// <summary>
/// Information about a managed key.
/// </summary>
public class KeyInfo
{
    /// <summary>
    /// Unique key identifier.
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// Algorithm the key is used for.
    /// </summary>
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>
    /// Key type (EC, RSA).
    /// </summary>
    public string KeyType { get; set; } = string.Empty;

    /// <summary>
    /// Curve name for EC keys.
    /// </summary>
    public string? Curve
    {
        get; set;
    }

    /// <summary>
    /// When the key was created.
    /// </summary>
    public DateTimeOffset CreatedAt
    {
        get; set;
    }

    /// <summary>
    /// Whether the key is hardware-backed.
    /// </summary>
    public bool IsHardwareBacked
    {
        get; set;
    }

    /// <summary>
    /// Secure area name if hardware-backed.
    /// </summary>
    public string? SecureAreaName
    {
        get; set;
    }

    /// <summary>
    /// HAIP compliance level if validated.
    /// </summary>
    public int? HaipLevel
    {
        get; set;
    }

    /// <summary>
    /// Additional metadata.
    /// </summary>
    public IDictionary<string, object>? Metadata
    {
        get; set;
    }
}

/// <summary>
/// Options for key generation supporting HAIP compliance levels.
/// </summary>
public class KeyGenerationOptions
{
    /// <summary>
    /// Signing algorithm (e.g., ES256, ES384, ES512).
    /// </summary>
    public string Algorithm { get; set; } = "ES256";

    /// <summary>
    /// Required HAIP compliance level (1, 2, or 3).
    /// </summary>
    public int? RequiredHaipLevel
    {
        get; set;
    }

    /// <summary>
    /// Whether to require hardware-backed key storage.
    /// </summary>
    public bool RequireHsmBacking
    {
        get; set;
    }

    /// <summary>
    /// Explicit key ID to use. If null, one will be generated.
    /// </summary>
    public string? KeyId
    {
        get; set;
    }

    /// <summary>
    /// Additional metadata to associate with the key.
    /// </summary>
    public IDictionary<string, object>? Metadata
    {
        get; set;
    }
}

/// <summary>
/// Manages cryptographic keys for wallet operations.
/// </summary>
public interface IKeyManager
{
    /// <summary>
    /// Generates a new key pair for signing operations.
    /// </summary>
    /// <param name="options">Key generation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated key information.</returns>
    Task<KeyInfo> GenerateKeyAsync(
        KeyGenerationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Signs data using the specified key.
    /// </summary>
    /// <param name="keyId">The ID of the key to use.</param>
    /// <param name="data">The data to sign.</param>
    /// <param name="algorithm">The signing algorithm.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The signature bytes.</returns>
    Task<byte[]> SignAsync(
        string keyId,
        byte[] data,
        string algorithm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the public key as a JWK.
    /// </summary>
    /// <param name="keyId">The key ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The public key as a JsonWebKey.</returns>
    Task<JsonWebKey> GetPublicKeyAsync(
        string keyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the security key for signing operations.
    /// </summary>
    /// <param name="keyId">The key ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The security key.</returns>
    Task<SecurityKey> GetSecurityKeyAsync(
        string keyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all available keys.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of key information.</returns>
    Task<IReadOnlyList<KeyInfo>> ListKeysAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a key.
    /// </summary>
    /// <param name="keyId">The key ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteKeyAsync(
        string keyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists.
    /// </summary>
    /// <param name="keyId">The key ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the key exists.</returns>
    Task<bool> KeyExistsAsync(
        string keyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about a specific key.
    /// </summary>
    /// <param name="keyId">The key ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Key information, or null if not found.</returns>
    Task<KeyInfo?> GetKeyInfoAsync(
        string keyId,
        CancellationToken cancellationToken = default);
}
