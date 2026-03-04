namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Field constraint information.
/// </summary>
public class FieldConstraintInfo
{
    /// <summary>
    /// JSONPath to the field.
    /// </summary>
    public IReadOnlyList<string> Paths { get; set; } = [];

    /// <summary>
    /// Optional filter for the field value.
    /// </summary>
    public string? Filter
    {
        get; set;
    }

    /// <summary>
    /// Whether intent to retain is requested.
    /// </summary>
    public bool IntentToRetain
    {
        get; set;
    }
}
