namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Pre-authorized code grant details.
/// </summary>
public class PreAuthorizedCodeGrant
{
    /// <summary>
    /// The pre-authorized code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether TX code (PIN) is required.
    /// </summary>
    public bool TxCodeRequired
    {
        get; set;
    }

    /// <summary>
    /// TX code input mode hint.
    /// </summary>
    public string? TxCodeInputMode
    {
        get; set;
    }

    /// <summary>
    /// TX code length hint.
    /// </summary>
    public int? TxCodeLength
    {
        get; set;
    }

    /// <summary>
    /// Description for TX code.
    /// </summary>
    public string? TxCodeDescription
    {
        get; set;
    }
}
