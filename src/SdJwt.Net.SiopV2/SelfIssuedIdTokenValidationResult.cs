using System.IdentityModel.Tokens.Jwt;

namespace SdJwt.Net.SiopV2;

/// <summary>
/// Result of successful SIOPv2 ID Token validation.
/// </summary>
public class SelfIssuedIdTokenValidationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelfIssuedIdTokenValidationResult"/> class.
    /// </summary>
    /// <param name="subject">The validated subject identifier.</param>
    /// <param name="payload">The validated JWT payload.</param>
    public SelfIssuedIdTokenValidationResult(string subject, JwtPayload payload)
    {
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }

    /// <summary>
    /// Gets the validated subject identifier.
    /// </summary>
    public string Subject
    {
        get;
    }

    /// <summary>
    /// Gets the validated JWT payload.
    /// </summary>
    public JwtPayload Payload
    {
        get;
    }
}
