using SdJwt.Net.Eudiw.TrustFramework;

namespace SdJwt.Net.Eudiw;

/// <summary>
/// Result of EU Trust validation.
/// </summary>
public class EuTrustValidationResult
{
    /// <summary>
    /// Whether the issuer is trusted.
    /// </summary>
    public bool IsTrusted
    {
        get; set;
    }

    /// <summary>
    /// Member state of the trusted service provider.
    /// </summary>
    public string? MemberState
    {
        get; set;
    }

    /// <summary>
    /// Service type of the provider.
    /// </summary>
    public TrustServiceType ServiceType
    {
        get; set;
    }

    /// <summary>
    /// Validation errors if not trusted.
    /// </summary>
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Creates a trusted result.
    /// </summary>
    /// <param name="memberState">The member state.</param>
    /// <param name="serviceType">The service type.</param>
    /// <returns>A trusted result.</returns>
    public static EuTrustValidationResult Trusted(string memberState, TrustServiceType serviceType = TrustServiceType.QualifiedAttestation)
    {
        return new EuTrustValidationResult
        {
            IsTrusted = true,
            MemberState = memberState,
            ServiceType = serviceType
        };
    }

    /// <summary>
    /// Creates an untrusted result.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <returns>An untrusted result.</returns>
    public static EuTrustValidationResult Untrusted(params string[] errors)
    {
        return new EuTrustValidationResult
        {
            IsTrusted = false,
            Errors = errors
        };
    }
}
