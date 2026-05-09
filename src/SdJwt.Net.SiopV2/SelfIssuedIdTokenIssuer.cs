using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace SdJwt.Net.SiopV2;

/// <summary>
/// Issues subject-signed SIOPv2 ID Tokens using JWK thumbprint subject syntax.
/// </summary>
public class SelfIssuedIdTokenIssuer
{
    private readonly SecurityKey _signingKey;
    private readonly string _signingAlgorithm;
    private readonly JsonWebKey _subjectPublicJwk;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelfIssuedIdTokenIssuer"/> class.
    /// </summary>
    /// <param name="signingKey">The subject-controlled private signing key.</param>
    /// <param name="signingAlgorithm">The JWS signing algorithm.</param>
    /// <param name="subjectPublicJwk">The public JWK corresponding to <paramref name="signingKey"/>.</param>
    public SelfIssuedIdTokenIssuer(SecurityKey signingKey, string signingAlgorithm, JsonWebKey subjectPublicJwk)
    {
        _signingKey = signingKey ?? throw new ArgumentNullException(nameof(signingKey));
        _signingAlgorithm = string.IsNullOrWhiteSpace(signingAlgorithm)
            ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(signingAlgorithm))
            : signingAlgorithm;
        _subjectPublicJwk = subjectPublicJwk ?? throw new ArgumentNullException(nameof(subjectPublicJwk));
    }

    /// <summary>
    /// Issues a subject-signed ID Token.
    /// </summary>
    /// <param name="options">ID Token issuance options.</param>
    /// <returns>A compact serialized JWT.</returns>
    public string Issue(SelfIssuedIdTokenOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new ArgumentException("Audience is required.", nameof(options));
        }
        if (string.IsNullOrWhiteSpace(options.Nonce))
        {
            throw new ArgumentException("Nonce is required.", nameof(options));
        }

        var now = DateTimeOffset.UtcNow;
        var subject = SiopSubject.CreateJwkThumbprintSubject(_subjectPublicJwk);
        var payload = new JwtPayload
        {
            { JwtRegisteredClaimNames.Iss, subject },
            { JwtRegisteredClaimNames.Sub, subject },
            { JwtRegisteredClaimNames.Aud, options.Audience },
            { JwtRegisteredClaimNames.Nonce, options.Nonce },
            { JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds() },
            { JwtRegisteredClaimNames.Exp, now.Add(options.Lifetime).ToUnixTimeSeconds() },
            { SiopConstants.Claims.SubJwk, SiopSubject.CreatePublicJwk(_subjectPublicJwk) }
        };

        if (options.AdditionalClaims != null)
        {
            foreach (var claim in options.AdditionalClaims)
            {
                if (payload.ContainsKey(claim.Key))
                {
                    throw new InvalidOperationException($"Additional claim '{claim.Key}' conflicts with a required SIOPv2 ID Token claim.");
                }

                payload[claim.Key] = claim.Value;
            }
        }

        var header = new JwtHeader(new SigningCredentials(_signingKey, _signingAlgorithm))
        {
            [JwtHeaderParameterNames.Typ] = "JWT"
        };

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(header, payload));
    }
}
