using System.Security.Cryptography.X509Certificates;

namespace SdJwt.Net.Eudiw.TrustFramework;

/// <summary>
/// Represents a Trusted Service Provider from EU Trust Lists.
/// </summary>
public class TrustedServiceProvider
{
    /// <summary>
    /// Name of the trust service provider.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of trust service offered.
    /// </summary>
    public TrustServiceType ServiceType
    {
        get; set;
    }

    /// <summary>
    /// Service status URI (e.g., granted, withdrawn).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Service endpoint URL, if applicable.
    /// </summary>
    public string? ServiceEndpoint
    {
        get; set;
    }

    /// <summary>
    /// X.509 certificates for the service.
    /// </summary>
    public IReadOnlyList<X509Certificate2> Certificates { get; set; } = Array.Empty<X509Certificate2>();
}

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

/// <summary>
/// EU List of Trusted Lists (LOTL) structure.
/// </summary>
public class ListOfTrustedLists
{
    /// <summary>
    /// Sequence number/version of the LOTL.
    /// </summary>
    public int SequenceNumber
    {
        get; set;
    }

    /// <summary>
    /// Issue date of this LOTL version.
    /// </summary>
    public DateTimeOffset IssueDate
    {
        get; set;
    }

    /// <summary>
    /// Next scheduled update date.
    /// </summary>
    public DateTimeOffset NextUpdate
    {
        get; set;
    }

    /// <summary>
    /// Pointers to member state Trusted Lists.
    /// </summary>
    public IReadOnlyList<TrustedListPointer> TrustedLists { get; set; } = Array.Empty<TrustedListPointer>();
}
