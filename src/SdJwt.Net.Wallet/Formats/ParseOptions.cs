namespace SdJwt.Net.Wallet.Formats;

/// <summary>
/// Options for parsing credentials.
/// </summary>
public class ParseOptions
{
    /// <summary>
    /// Whether to strictly validate the credential structure.
    /// </summary>
    public bool StrictValidation { get; set; } = true;

    /// <summary>
    /// Whether to extract all disclosures.
    /// </summary>
    public bool ExtractDisclosures { get; set; } = true;
}
