namespace SdJwt.Net.Eudiw.TrustFramework;

/// <summary>
/// Result of trust validation against EU Trust Lists.
/// </summary>
public class TrustValidationResult
{
    /// <summary>
    /// Indicates whether the issuer is trusted.
    /// </summary>
    public bool IsTrusted
    {
        get; set;
    }

    /// <summary>
    /// Reason for untrusted status, if applicable.
    /// </summary>
    public string? Reason
    {
        get; set;
    }

    /// <summary>
    /// Information about the trusted issuer.
    /// </summary>
    public TrustedServiceProvider? IssuerInfo
    {
        get; set;
    }

    /// <summary>
    /// The member state where the issuer is registered.
    /// </summary>
    public string? MemberState
    {
        get; set;
    }

    /// <summary>
    /// Creates a trusted result.
    /// </summary>
    /// <param name="provider">The trusted service provider information.</param>
    /// <param name="memberState">The member state code.</param>
    /// <returns>A trusted validation result.</returns>
    public static TrustValidationResult Trusted(TrustedServiceProvider provider, string memberState)
    {
        return new TrustValidationResult
        {
            IsTrusted = true,
            IssuerInfo = provider,
            MemberState = memberState
        };
    }

    /// <summary>
    /// Creates an untrusted result.
    /// </summary>
    /// <param name="reason">The reason for being untrusted.</param>
    /// <returns>An untrusted validation result.</returns>
    public static TrustValidationResult Untrusted(string reason)
    {
        return new TrustValidationResult
        {
            IsTrusted = false,
            Reason = reason
        };
    }
}
