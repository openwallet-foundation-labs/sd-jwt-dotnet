namespace SdJwt.Net.Mdoc.Namespaces;

/// <summary>
/// Constants and helpers for the mDL namespace per ISO 18013-5.
/// </summary>
public static class MdlNamespace
{
    /// <summary>
    /// The ISO 18013-5 mDL namespace identifier.
    /// </summary>
    public const string Namespace = "org.iso.18013.5.1";

    /// <summary>
    /// The mDL document type identifier.
    /// </summary>
    public const string DocType = "org.iso.18013.5.1.mDL";

    /// <summary>
    /// Mandatory elements for mDL per ISO 18013-5.
    /// </summary>
    public static readonly IReadOnlyList<MdlDataElement> MandatoryElements = new[]
    {
        MdlDataElement.FamilyName,
        MdlDataElement.GivenName,
        MdlDataElement.BirthDate,
        MdlDataElement.IssueDate,
        MdlDataElement.ExpiryDate,
        MdlDataElement.IssuingCountry,
        MdlDataElement.IssuingAuthority,
        MdlDataElement.DocumentNumber,
        MdlDataElement.Portrait,
        MdlDataElement.DrivingPrivileges,
        MdlDataElement.UnDistinguishingSign
    };

    /// <summary>
    /// Gets the full element identifier with namespace prefix.
    /// </summary>
    /// <param name="element">The data element.</param>
    /// <returns>The full element identifier.</returns>
    public static string GetFullIdentifier(MdlDataElement element)
    {
        return $"{Namespace}.{element.ToElementIdentifier()}";
    }

    /// <summary>
    /// Checks if an element is mandatory for mDL.
    /// </summary>
    /// <param name="element">The data element.</param>
    /// <returns>True if the element is mandatory.</returns>
    public static bool IsMandatory(MdlDataElement element)
    {
        return MandatoryElements.Contains(element);
    }

    /// <summary>
    /// Creates a claims dictionary for the mDL namespace.
    /// </summary>
    /// <param name="claims">The claims to add.</param>
    /// <returns>A dictionary with the mDL namespace as key.</returns>
    public static Dictionary<string, Dictionary<string, object>> CreateClaims(
        Dictionary<MdlDataElement, object> claims)
    {
        var result = new Dictionary<string, Dictionary<string, object>>
        {
            [Namespace] = new Dictionary<string, object>()
        };

        foreach (var (element, value) in claims)
        {
            result[Namespace][element.ToElementIdentifier()] = value;
        }

        return result;
    }
}
