namespace SdJwt.Net.Wallet.Status;

/// <summary>
/// Status resolution result for a stored credential/document.
/// </summary>
public class DocumentStatusResult
{
    /// <summary>
    /// Resolved status value.
    /// </summary>
    public DocumentStatus Status
    {
        get; set;
    } = DocumentStatus.Reserved;

    /// <summary>
    /// Optional reason text from the resolver.
    /// </summary>
    public string? Reason
    {
        get; set;
    }

    /// <summary>
    /// Optional status metadata.
    /// </summary>
    public IDictionary<string, object>? Metadata
    {
        get; set;
    }
}
