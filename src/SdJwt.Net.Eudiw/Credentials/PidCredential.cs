namespace SdJwt.Net.Eudiw.Credentials;

/// <summary>
/// Model representing PID credential data.
/// </summary>
public class PidCredential
{
    /// <summary>
    /// Family name of the holder.
    /// </summary>
    public string FamilyName { get; set; } = string.Empty;

    /// <summary>
    /// Given name of the holder.
    /// </summary>
    public string GivenName { get; set; } = string.Empty;

    /// <summary>
    /// Birth date of the holder.
    /// </summary>
    public DateTime BirthDate
    {
        get; set;
    }

    /// <summary>
    /// Issuance date of the credential.
    /// </summary>
    public DateTime IssuanceDate
    {
        get; set;
    }

    /// <summary>
    /// Expiry date of the credential.
    /// </summary>
    public DateTime ExpiryDate
    {
        get; set;
    }

    /// <summary>
    /// Issuing authority name.
    /// </summary>
    public string IssuingAuthority { get; set; } = string.Empty;

    /// <summary>
    /// Issuing country (ISO 3166-1 alpha-2).
    /// </summary>
    public string IssuingCountry { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if holder is over 18, if disclosed.
    /// </summary>
    public bool? AgeOver18
    {
        get; set;
    }

    /// <summary>
    /// Indicates if holder is over 21, if disclosed.
    /// </summary>
    public bool? AgeOver21
    {
        get; set;
    }

    /// <summary>
    /// Nationality of the holder, if disclosed.
    /// </summary>
    public string? Nationality
    {
        get; set;
    }

    /// <summary>
    /// Resident address, if disclosed.
    /// </summary>
    public string? ResidentAddress
    {
        get; set;
    }
}
