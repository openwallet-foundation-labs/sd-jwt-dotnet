namespace SdJwt.Net.Wallet.Storage;

/// <summary>
/// Query options for filtering credentials.
/// </summary>
public class CredentialQuery
{
    /// <summary>
    /// Filter by credential types.
    /// </summary>
    public IReadOnlyList<string>? Types
    {
        get; set;
    }

    /// <summary>
    /// Filter by issuers.
    /// </summary>
    public IReadOnlyList<string>? Issuers
    {
        get; set;
    }

    /// <summary>
    /// Filter by format ID.
    /// </summary>
    public string? FormatId
    {
        get; set;
    }

    /// <summary>
    /// Include only valid (not expired, not revoked) credentials.
    /// </summary>
    public bool OnlyValid { get; set; } = true;

    /// <summary>
    /// Include only credentials with key binding.
    /// </summary>
    public bool? HasKeyBinding
    {
        get; set;
    }

    /// <summary>
    /// Maximum number of results.
    /// </summary>
    public int? Limit
    {
        get; set;
    }

    /// <summary>
    /// Order by field.
    /// </summary>
    public string? OrderBy
    {
        get; set;
    }

    /// <summary>
    /// Order direction (asc/desc).
    /// </summary>
    public bool Descending
    {
        get; set;
    }
}
