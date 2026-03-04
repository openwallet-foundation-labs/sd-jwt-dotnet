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
