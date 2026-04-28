namespace SdJwt.Net.Wallet.Core;

/// <summary>
/// Options controlling how a key is rotated.
/// </summary>
public class KeyRotationOptions
{
    /// <summary>
    /// The signing algorithm for the new key. If null, the same algorithm
    /// as the current key is used.
    /// </summary>
    public string? NewAlgorithm
    {
        get; set;
    }

    /// <summary>
    /// Whether to immediately decommission (delete) the old key after rotation.
    /// Default is false, keeping the old key for verification of previously
    /// issued credentials.
    /// </summary>
    public bool DecommissionOldKey
    {
        get; set;
    }

    /// <summary>
    /// Whether to require hardware-backed storage for the new key.
    /// </summary>
    public bool RequireHsmBacking
    {
        get; set;
    }

    /// <summary>
    /// Optional metadata to associate with the new key.
    /// </summary>
    public IDictionary<string, object>? Metadata
    {
        get; set;
    }
}
