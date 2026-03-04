namespace SdJwt.Net.Wallet.Core;

/// <summary>
/// Filter criteria for finding credentials.
/// </summary>
public class CredentialFilter
{
    /// <summary>
    /// Filter by credential type.
    /// </summary>
    public string? CredentialType
    {
        get; set;
    }

    /// <summary>
    /// Filter by issuer.
    /// </summary>
    public string? Issuer
    {
        get; set;
    }

    /// <summary>
    /// Filter by format.
    /// </summary>
    public string? Format
    {
        get; set;
    }

    /// <summary>
    /// Filter by document ID.
    /// </summary>
    public string? DocumentId
    {
        get; set;
    }

    /// <summary>
    /// Whether to include expired credentials.
    /// </summary>
    public bool IncludeExpired { get; set; } = false;
}
