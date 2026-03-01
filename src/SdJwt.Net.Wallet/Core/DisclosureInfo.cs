namespace SdJwt.Net.Wallet.Core;

/// <summary>
/// Information about a single disclosure.
/// </summary>
public class DisclosureInfo
{
    /// <summary>
    /// JSON path or claim name.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The disclosed value.
    /// </summary>
    public object? Value
    {
        get; set;
    }

    /// <summary>
    /// The disclosure digest.
    /// </summary>
    public string? Digest
    {
        get; set;
    }

    /// <summary>
    /// The salt used in the disclosure.
    /// </summary>
    public string? Salt
    {
        get; set;
    }

    /// <summary>
    /// Whether this disclosure is selected for presentation.
    /// </summary>
    public bool IsSelected
    {
        get; set;
    }
}
