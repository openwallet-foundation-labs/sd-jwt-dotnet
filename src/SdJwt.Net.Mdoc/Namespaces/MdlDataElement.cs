namespace SdJwt.Net.Mdoc.Namespaces;

/// <summary>
/// Data elements defined in the mDL namespace per ISO 18013-5.
/// </summary>
public enum MdlDataElement
{
    /// <summary>
    /// Family name of the holder.
    /// </summary>
    FamilyName,

    /// <summary>
    /// Given name of the holder.
    /// </summary>
    GivenName,

    /// <summary>
    /// Date of birth.
    /// </summary>
    BirthDate,

    /// <summary>
    /// Date of issue.
    /// </summary>
    IssueDate,

    /// <summary>
    /// Date of expiry.
    /// </summary>
    ExpiryDate,

    /// <summary>
    /// Issuing country (alpha-2 or alpha-3 code).
    /// </summary>
    IssuingCountry,

    /// <summary>
    /// Issuing authority.
    /// </summary>
    IssuingAuthority,

    /// <summary>
    /// Document number.
    /// </summary>
    DocumentNumber,

    /// <summary>
    /// Portrait image of the holder.
    /// </summary>
    Portrait,

    /// <summary>
    /// Driving privileges.
    /// </summary>
    DrivingPrivileges,

    /// <summary>
    /// UN distinguishing sign.
    /// </summary>
    UnDistinguishingSign,

    /// <summary>
    /// Administrative number.
    /// </summary>
    AdministrativeNumber,

    /// <summary>
    /// Sex of the holder.
    /// </summary>
    Sex,

    /// <summary>
    /// Height of the holder.
    /// </summary>
    Height,

    /// <summary>
    /// Weight of the holder.
    /// </summary>
    Weight,

    /// <summary>
    /// Eye color.
    /// </summary>
    EyeColour,

    /// <summary>
    /// Hair color.
    /// </summary>
    HairColour,

    /// <summary>
    /// Birth place.
    /// </summary>
    BirthPlace,

    /// <summary>
    /// Resident address.
    /// </summary>
    ResidentAddress,

    /// <summary>
    /// Portrait capture date.
    /// </summary>
    PortraitCaptureDate,

    /// <summary>
    /// Age in years.
    /// </summary>
    AgeInYears,

    /// <summary>
    /// Age birth year.
    /// </summary>
    AgeBirthYear,

    /// <summary>
    /// Age over 18 indicator.
    /// </summary>
    AgeOver18,

    /// <summary>
    /// Age over 21 indicator.
    /// </summary>
    AgeOver21,

    /// <summary>
    /// Issuing jurisdiction.
    /// </summary>
    IssuingJurisdiction,

    /// <summary>
    /// Nationality.
    /// </summary>
    Nationality,

    /// <summary>
    /// Resident city.
    /// </summary>
    ResidentCity,

    /// <summary>
    /// Resident state.
    /// </summary>
    ResidentState,

    /// <summary>
    /// Resident postal code.
    /// </summary>
    ResidentPostalCode,

    /// <summary>
    /// Resident country.
    /// </summary>
    ResidentCountry,

    /// <summary>
    /// Family name in national characters.
    /// </summary>
    FamilyNameNationalCharacter,

    /// <summary>
    /// Given name in national characters.
    /// </summary>
    GivenNameNationalCharacter,

    /// <summary>
    /// Signature or usual mark image.
    /// </summary>
    SignatureUsualMark
}

/// <summary>
/// Extension methods for MdlDataElement enum.
/// </summary>
public static class MdlDataElementExtensions
{
    /// <summary>
    /// Gets the element identifier string for the data element.
    /// </summary>
    /// <param name="element">The data element.</param>
    /// <returns>The element identifier string.</returns>
    public static string ToElementIdentifier(this MdlDataElement element)
    {
        return element switch
        {
            MdlDataElement.FamilyName => "family_name",
            MdlDataElement.GivenName => "given_name",
            MdlDataElement.BirthDate => "birth_date",
            MdlDataElement.IssueDate => "issue_date",
            MdlDataElement.ExpiryDate => "expiry_date",
            MdlDataElement.IssuingCountry => "issuing_country",
            MdlDataElement.IssuingAuthority => "issuing_authority",
            MdlDataElement.DocumentNumber => "document_number",
            MdlDataElement.Portrait => "portrait",
            MdlDataElement.DrivingPrivileges => "driving_privileges",
            MdlDataElement.UnDistinguishingSign => "un_distinguishing_sign",
            MdlDataElement.AdministrativeNumber => "administrative_number",
            MdlDataElement.Sex => "sex",
            MdlDataElement.Height => "height",
            MdlDataElement.Weight => "weight",
            MdlDataElement.EyeColour => "eye_colour",
            MdlDataElement.HairColour => "hair_colour",
            MdlDataElement.BirthPlace => "birth_place",
            MdlDataElement.ResidentAddress => "resident_address",
            MdlDataElement.PortraitCaptureDate => "portrait_capture_date",
            MdlDataElement.AgeInYears => "age_in_years",
            MdlDataElement.AgeBirthYear => "age_birth_year",
            MdlDataElement.AgeOver18 => "age_over_18",
            MdlDataElement.AgeOver21 => "age_over_21",
            MdlDataElement.IssuingJurisdiction => "issuing_jurisdiction",
            MdlDataElement.Nationality => "nationality",
            MdlDataElement.ResidentCity => "resident_city",
            MdlDataElement.ResidentState => "resident_state",
            MdlDataElement.ResidentPostalCode => "resident_postal_code",
            MdlDataElement.ResidentCountry => "resident_country",
            MdlDataElement.FamilyNameNationalCharacter => "family_name_national_character",
            MdlDataElement.GivenNameNationalCharacter => "given_name_national_character",
            MdlDataElement.SignatureUsualMark => "signature_usual_mark",
            _ => throw new ArgumentOutOfRangeException(nameof(element))
        };
    }

    /// <summary>
    /// Parses an element identifier string to a MdlDataElement.
    /// </summary>
    /// <param name="elementIdentifier">The element identifier string.</param>
    /// <returns>The corresponding MdlDataElement.</returns>
    public static MdlDataElement? FromElementIdentifier(string elementIdentifier)
    {
        return elementIdentifier switch
        {
            "family_name" => MdlDataElement.FamilyName,
            "given_name" => MdlDataElement.GivenName,
            "birth_date" => MdlDataElement.BirthDate,
            "issue_date" => MdlDataElement.IssueDate,
            "expiry_date" => MdlDataElement.ExpiryDate,
            "issuing_country" => MdlDataElement.IssuingCountry,
            "issuing_authority" => MdlDataElement.IssuingAuthority,
            "document_number" => MdlDataElement.DocumentNumber,
            "portrait" => MdlDataElement.Portrait,
            "driving_privileges" => MdlDataElement.DrivingPrivileges,
            "un_distinguishing_sign" => MdlDataElement.UnDistinguishingSign,
            "administrative_number" => MdlDataElement.AdministrativeNumber,
            "sex" => MdlDataElement.Sex,
            "height" => MdlDataElement.Height,
            "weight" => MdlDataElement.Weight,
            "eye_colour" => MdlDataElement.EyeColour,
            "hair_colour" => MdlDataElement.HairColour,
            "birth_place" => MdlDataElement.BirthPlace,
            "resident_address" => MdlDataElement.ResidentAddress,
            "portrait_capture_date" => MdlDataElement.PortraitCaptureDate,
            "age_in_years" => MdlDataElement.AgeInYears,
            "age_birth_year" => MdlDataElement.AgeBirthYear,
            "age_over_18" => MdlDataElement.AgeOver18,
            "age_over_21" => MdlDataElement.AgeOver21,
            "issuing_jurisdiction" => MdlDataElement.IssuingJurisdiction,
            "nationality" => MdlDataElement.Nationality,
            "resident_city" => MdlDataElement.ResidentCity,
            "resident_state" => MdlDataElement.ResidentState,
            "resident_postal_code" => MdlDataElement.ResidentPostalCode,
            "resident_country" => MdlDataElement.ResidentCountry,
            "family_name_national_character" => MdlDataElement.FamilyNameNationalCharacter,
            "given_name_national_character" => MdlDataElement.GivenNameNationalCharacter,
            "signature_usual_mark" => MdlDataElement.SignatureUsualMark,
            _ => null
        };
    }
}
