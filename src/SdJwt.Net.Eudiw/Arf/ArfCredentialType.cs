namespace SdJwt.Net.Eudiw.Arf;

/// <summary>
/// Credential types defined in the EU Architecture Reference Framework (ARF).
/// </summary>
public enum ArfCredentialType
{
    /// <summary>
    /// Person Identification Data - National ID equivalent in mdoc format.
    /// </summary>
    Pid,

    /// <summary>
    /// Mobile Driving License - Driver's license in mdoc format per ISO 18013-5.
    /// </summary>
    Mdl,

    /// <summary>
    /// Qualified Electronic Attestation of Attributes - Diplomas, professional certs in SD-JWT VC format.
    /// Requires qualified trust service provider.
    /// </summary>
    Qeaa,

    /// <summary>
    /// Electronic Attestation of Attributes - Loyalty cards, memberships in SD-JWT VC format.
    /// Does not require qualified trust service provider.
    /// </summary>
    Eaa
}

/// <summary>
/// Extension methods for ArfCredentialType.
/// </summary>
public static class ArfCredentialTypeExtensions
{
    /// <summary>
    /// Gets the document type identifier for mdoc-based credentials.
    /// </summary>
    /// <param name="type">The credential type.</param>
    /// <returns>The document type string, or null for SD-JWT VC types.</returns>
    public static string? GetDocType(this ArfCredentialType type)
    {
        return type switch
        {
            ArfCredentialType.Pid => EudiwConstants.Pid.DocType,
            ArfCredentialType.Mdl => EudiwConstants.Mdl.DocType,
            _ => null
        };
    }

    /// <summary>
    /// Determines if the credential type uses mdoc format.
    /// </summary>
    /// <param name="type">The credential type.</param>
    /// <returns>True if mdoc format; false if SD-JWT VC format.</returns>
    public static bool IsMdocFormat(this ArfCredentialType type)
    {
        return type == ArfCredentialType.Pid || type == ArfCredentialType.Mdl;
    }

    /// <summary>
    /// Determines if the credential type uses SD-JWT VC format.
    /// </summary>
    /// <param name="type">The credential type.</param>
    /// <returns>True if SD-JWT VC format; false if mdoc format.</returns>
    public static bool IsSdJwtVcFormat(this ArfCredentialType type)
    {
        return type == ArfCredentialType.Qeaa || type == ArfCredentialType.Eaa;
    }

    /// <summary>
    /// Determines if the credential type requires a qualified trust service provider.
    /// </summary>
    /// <param name="type">The credential type.</param>
    /// <returns>True if qualified trust is required.</returns>
    public static bool RequiresQualifiedTrust(this ArfCredentialType type)
    {
        return type == ArfCredentialType.Pid || type == ArfCredentialType.Qeaa;
    }
}
