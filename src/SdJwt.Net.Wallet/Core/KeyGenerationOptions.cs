namespace SdJwt.Net.Wallet.Core;

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
