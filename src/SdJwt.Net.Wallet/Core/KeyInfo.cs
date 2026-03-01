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
