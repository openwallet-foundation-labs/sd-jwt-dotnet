namespace SdJwt.Net.Mdoc.Issuer;

/// <summary>
/// Options for mdoc issuance.
/// </summary>
public class MdocIssuerOptions
{
    /// <summary>
    /// Document type (e.g., "org.iso.18013.5.1.mDL").
    /// </summary>
    public string DocType { get; set; } = string.Empty;

    /// <summary>
    /// Validity period start (signed timestamp).
    /// </summary>
    public DateTimeOffset ValidFrom { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Validity period end (signed timestamp).
    /// </summary>
    public DateTimeOffset ValidUntil { get; set; } = DateTimeOffset.UtcNow.AddYears(1);

    /// <summary>
    /// Expected update timestamp (optional).
    /// </summary>
    public DateTimeOffset? ExpectedUpdate
    {
        get; set;
    }

    /// <summary>
    /// Include device key in MSO for holder binding.
    /// </summary>
    public bool IncludeDeviceKey { get; set; } = true;
}
