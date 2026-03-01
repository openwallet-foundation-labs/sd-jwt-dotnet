namespace SdJwt.Net.Eudiw.TrustFramework;

/// <summary>
/// Types of trust services defined in the eIDAS framework.
/// </summary>
public enum TrustServiceType
{
    /// <summary>
    /// Qualified certificate for electronic signature.
    /// </summary>
    QualifiedCertificateSignature,

    /// <summary>
    /// Qualified certificate for electronic seal.
    /// </summary>
    QualifiedCertificateSeal,

    /// <summary>
    /// Qualified attestation of attributes provider (QEAA).
    /// </summary>
    QualifiedAttestation,

    /// <summary>
    /// Person Identification Data provider.
    /// </summary>
    PidProvider,

    /// <summary>
    /// Electronic attestation of attributes provider (EAA).
    /// </summary>
    ElectronicAttestation
}

/// <summary>
/// Extension methods for TrustServiceType.
/// </summary>
public static class TrustServiceTypeExtensions
{
    /// <summary>
    /// Determines if the service type is a qualified trust service.
    /// </summary>
    /// <param name="type">The trust service type.</param>
    /// <returns>True if qualified; false otherwise.</returns>
    public static bool IsQualifiedService(this TrustServiceType type)
    {
        return type switch
        {
            TrustServiceType.QualifiedCertificateSignature => true,
            TrustServiceType.QualifiedCertificateSeal => true,
            TrustServiceType.QualifiedAttestation => true,
            TrustServiceType.PidProvider => true,
            TrustServiceType.ElectronicAttestation => false,
            _ => false
        };
    }

    /// <summary>
    /// Gets the ETSI service type URI for the trust service type.
    /// </summary>
    /// <param name="type">The trust service type.</param>
    /// <returns>The ETSI URI for the service type.</returns>
    public static string GetServiceTypeUri(this TrustServiceType type)
    {
        return type switch
        {
            TrustServiceType.QualifiedCertificateSignature => "http://uri.etsi.org/TrstSvc/Svctype/CA/QC",
            TrustServiceType.QualifiedCertificateSeal => "http://uri.etsi.org/TrstSvc/Svctype/CA/QC",
            TrustServiceType.QualifiedAttestation => "http://uri.etsi.org/TrstSvc/Svctype/Certstatus/QEAA",
            TrustServiceType.PidProvider => "http://uri.etsi.org/TrstSvc/Svctype/PID",
            TrustServiceType.ElectronicAttestation => "http://uri.etsi.org/TrstSvc/Svctype/EAA",
            _ => "http://uri.etsi.org/TrstSvc/Svctype/unspecified"
        };
    }
}
