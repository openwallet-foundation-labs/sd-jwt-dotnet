namespace SdJwt.Net.Wallet.Core;

/// <summary>
/// Filter criteria for listing keys.
/// </summary>
public class KeyFilter
{
    /// <summary>
    /// Filter by algorithm (e.g., "ES256").
    /// </summary>
    public string? Algorithm
    {
        get; set;
    }

    /// <summary>
    /// Filter by key type (e.g., "EC", "RSA").
    /// </summary>
    public string? KeyType
    {
        get; set;
    }

    /// <summary>
    /// Include only hardware-backed keys.
    /// </summary>
    public bool? IsHardwareBacked
    {
        get; set;
    }

    /// <summary>
    /// Include only keys that have not expired.
    /// </summary>
    public bool OnlyActive { get; set; } = true;
}
