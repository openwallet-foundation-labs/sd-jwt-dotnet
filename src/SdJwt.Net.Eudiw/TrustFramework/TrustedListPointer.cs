namespace SdJwt.Net.Eudiw.TrustFramework;

/// <summary>
/// Pointer to a member state's Trusted List within the LOTL.
/// </summary>
public class TrustedListPointer
{
    /// <summary>
    /// Member state code (ISO 3166-1 alpha-2).
    /// </summary>
    public string Territory { get; set; } = string.Empty;

    /// <summary>
    /// URL of the member state's Trusted List.
    /// </summary>
    public string TslLocation { get; set; } = string.Empty;
}
